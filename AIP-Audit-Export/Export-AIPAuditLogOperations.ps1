<#   
The MIT License (MIT)

Copyright (c) 2015 Microsoft Corporation

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
#>



<#
Script      : AzureDataResourcesEnumeration.ps1
Author      : Krishna V
Co-Author   : Aashish Ramdas
Version     : 1.0.5
Description : The script exports AIP data from the M365 Unified Audit Log and pushes it into a customer-specified Log Analytics table
#>



param (
    # Log Analytics table where the data is written to. Log Analytics will add an _CL to this name.
    [string]$TableName = "UnifiedLog",

    # Script pause interval / Sleep internal. After retrieving the logs, the script will sleep until the next run.
    [int16]$SleepIntervalMinutes = 60,

    # Logs will be retrieved starting at this time. Default is the timestamp for now() - 1 hour
    [datetime]$StartTime = [DateTime]::UtcNow.AddHours(-1),

    # If an end time is specified, the script will run only once with the logs retrieved until the specified time. If an end time is not specified, the script will loop until stopped
    [datetime]$EndTime
)

# your Log Analytics workspace ID
$LogAnalyticsWorkspaceId = ""

# Use either the primary or the secondary Connected Sources client authentication key   
$LogAnalyticsPrimaryKey = "" 

if($LogAnalyticsWorkspaceId -eq "") { throw "Log Analytics workspace Id is missing! Update the script and run again" }
if($LogAnalyticsPrimaryKey -eq "")  { throw "Log Analytics primary key is missing! Update the script and run again" }

# Audit export size
$resultSize = 100 

# File where the Search-UnifiedAuditLog input parameters StartDate and EndDate values are stored. Useful if you want to know when the last time data was successfully exported to Log Analytics
$file_PersistTimeStamp = "./endTimestamp.txt"



Function Build-Signature ($customerId, $sharedKey, $date, $contentLength, $method, $contentType, $resource) {
    # ---------------------------------------------------------------   
    #    Name           : Build-Signature
    #    Value          : Creates the authorization signature used in the REST API call to Log Analytics
    # ---------------------------------------------------------------

    $xHeaders = "x-ms-date:" + $date
    $stringToHash = $method + "`n" + $contentLength + "`n" + $contentType + "`n" + $xHeaders + "`n" + $resource

    $bytesToHash = [Text.Encoding]::UTF8.GetBytes($stringToHash)
    $keyBytes = [Convert]::FromBase64String($sharedKey)

    $sha256 = New-Object System.Security.Cryptography.HMACSHA256
    $sha256.Key = $keyBytes
    $calculatedHash = $sha256.ComputeHash($bytesToHash)
    $encodedHash = [Convert]::ToBase64String($calculatedHash)
    $authorization = 'SharedKey {0}:{1}' -f $customerId,$encodedHash
    return $authorization
}


Function Post-LogAnalyticsData($body, $LogAnalyticsTableName) {
    # ---------------------------------------------------------------   
    #    Name           : Post-LogAnalyticsData
    #    Value          : Writes the data to Log Analytics using a REST API
    #    Input          : 1) PSObject with the data
    #                     2) Table name in Log Analytics
    #    Return         : None
    # ---------------------------------------------------------------
    
    #Step 0: sanity checks
    if($body -isnot [array]) {return}
    if($body.Count -eq 0) {return}

    #Step 1: convert the PSObject to JSON
    $bodyJson = $body | ConvertTo-Json

    #Step 2: get the UTF8 bytestream for the JSON
    $bodyJsonUTF8 = ([System.Text.Encoding]::UTF8.GetBytes($bodyJson))

    #Step 3: build the signature        
    $method = "POST"
    $contentType = "application/json"
    $resource = "/api/logs"
    $rfc1123date = [DateTime]::UtcNow.ToString("r")
    $contentLength = $bodyJsonUTF8.Length    
    $signature = Build-Signature -customerId $LogAnalyticsWorkspaceId -sharedKey $LogAnalyticsPrimaryKey -date $rfc1123date -contentLength $contentLength -method $method -contentType $contentType -resource $resource
    
    #Step 4: create the header
    $headers = @{
        "Authorization" = $signature;
        "Log-Type" = $LogAnalyticsTableName;
        "x-ms-date" = $rfc1123date;
        #"time-generated-field" = $TimeStampField;
    };

    #Step 5: REST API call
    $uri = "https://" + $LogAnalyticsWorkspaceId + ".ods.opinsights.azure.com" + $resource + "?api-version=2016-04-01"
    $response = Invoke-WebRequest -Uri $uri -Method Post -Headers $headers -ContentType $contentType -Body $bodyJsonUTF8 -UseBasicParsing

    if ($Response.StatusCode -eq 200) {   
        $rows = $body.Count
        Write-Information -MessageData "   $rows rows written to Log Analytics workspace $uri" -InformationAction Continue
    }

}


Function Export-AuditLogData([datetime]$start, [datetime]$end, [string]$recordType) {
    # ---------------------------------------------------------------   
    #    Name           : Export-AuditLogData
    #    Value          : Creates a single session defined by start and end times, and exports data within this session to Log Analytics
    #    Input          : 1) start time
    #                     2) end time
    #    Return         : None
    # ---------------------------------------------------------------
    

    $sessionID = "ExtractLogs_" + $start.ToString("yyyyMMddHHmmssfff") + "_" + $end.ToString("yyyyMMddHHmmssfff")
    Write-Information -MessageData "Retrieving $recordType records between $start and $end" -InformationAction Continue

    do {
        # Run the commandlet to search through the Audit logs and get the AIP events in the specified timeframe
        $auditLogSearchResults = Search-UnifiedAuditLog -StartDate $start -EndDate $end -RecordType $recordType -SessionId $sessionID -SessionCommand ReturnLargeSet -ResultSize $resultSize
        

        if($auditLogSearchResults -eq $null   )    { $auditLogSearchResults = @() }  
        if($auditLogSearchResults -isnot [array] ) { $auditLogSearchResults = @($auditLogSearchResults)  }

        # Status update
        $recordsCount = $auditLogSearchResults.Count
        Write-Information -MessageData "   $recordsCount rows returned by Search-UnifiedAuditLog" -InformationAction Continue

        # If there is no data, skip
        if ($auditLogSearchResults.Count -eq 0) { continue; }

        # Else format for Log Analytics
        $log_analytics_array = @()            
        foreach($i in $auditLogSearchResults) {
            $auditData = $i.AuditData | ConvertFrom-Json
            $newitem = [PSCustomObject]@{    
                RunspaceId               = $i.RunspaceId
                ApplicationName          = $auditData.Common.ApplicationName
                Common_ProcessName       = $auditData.Common.ProcessName 
                Common_DeviceName        = $auditData.Common.DeviceName
                Common_Location          = $auditData.Common.Location
                Common_ProductVersion    = $auditData.Common.ProductVersion
                RecordType               = $auditData.RecordType
                Workload                 = $auditData.workload
                UserId                   = $auditData.UserId
                UserKey                  = $auditData.UserKey
                ClientIP                 = $auditData.ClientIP
                CreationTime             = $auditData.CreationTime
                Operation                = $auditData.Operation
                OrganizationId           = $auditData.OrganizationId
            }

            #$value = [System.Web.HttpUtility]::UrlEncode($auditData.objectId)
            $value = $auditData.objectId
            $newitem | Add-Member -MemberType NoteProperty -Name ObjectId -Value $value

            $value= if($auditData.SensitivityLabelEventData.LabelEventType -eq $null)  { "" } else { $auditData.SensitivityLabelEventData.LabelEventType }
            $newitem | Add-Member -MemberType NoteProperty -Name LabelEventType -Value $value

            $value = if($auditData.SensitivityLabelEventData.ActionSource -eq $null)   { "" } else { $auditData.SensitivityLabelEventData.ActionSource }
            $newitem | Add-Member -MemberType NoteProperty -Name ActionSource -Value $value

            $value = if($auditData.SensitivityLabelEventData.SensitivityLabelId -eq $null) { "" } else { $auditData.SensitivityLabelEventData.SensitivityLabelId }
            $newitem | Add-Member -MemberType NoteProperty -Name SensitivityLabelEventData_SensitivityLabelId -Value $value

            $value = if($auditData.ProtectionEventData.ProtectionEventType -eq $null)      { "" } else { $auditData.ProtectionEventData.ProtectionEventType }
            $newitem | Add-Member -MemberType NoteProperty -Name ProtectionEventType -Value $value

            $value = if($auditData.ProtectionEventData.PreviousProtectionOwner -eq $null)  { "" } else { $auditData.ProtectionEventData.PreviousProtectionOwner }
            $newitem | Add-Member -MemberType NoteProperty -Name PreviousProtectionOwner -Value $value

            $log_analytics_array += $newitem
        }

        # Push data to Log Analytics
        Post-LogAnalyticsData -LogAnalyticsTableName $TableName -body $log_analytics_array
        
    } while (($auditLogSearchResults | Measure-Object).Count -ne 0)   # loop until the session has no records left to retrieve
}



#import exchange online management module
Import-Module ExchangeOnlineManagement

#connect to exchangeonline
Connect-ExchangeOnline

$isContinuousLoop = if($PSBoundParameters.ContainsKey('EndTime')) { $false} else { $true }

# main code
do {
    # set end time for continuous export
    $temp_EndTime = if ($true -eq $isContinuousLoop) { [DateTime]::UtcNow } else { $EndTime }
   
    # Main export operation. Only AIP-related operations are selected for export. For Full list review here: https://docs.microsoft.com/en-us/microsoft-365/compliance/search-the-audit-log-in-security-and-compliance?view=o365-worldwide
    Export-AuditLogData -start $StartTime -end $temp_EndTime -recordType "AipDiscover"
    Export-AuditLogData -start $StartTime -end $temp_EndTime -recordType "AipSensitivityLabelAction"
    Export-AuditLogData -start $StartTime -end $temp_EndTime -recordType "AipProtectionAction"
    Export-AuditLogData -start $StartTime -end $temp_EndTime -recordType "AipFileDeleted"
    Export-AuditLogData -start $StartTime -end $temp_EndTime -recordType "AipHeartBeat"

    # persist end timestamp
    $logstring = "Export complete for audit logs between Start-time: " + $temp_EndTime.ToString("yyyyMMddHHmmssfff") + " and End-Time: " + $temp_EndTime.ToString("yyyyMMddHHmmssfff") 
    $logstring | Out-File $file_PersistTimeStamp -Append
    
    # check if loop or one-time
    if($true -eq $isContinuousLoop) {
        # adjust start value
        $StartTime = $temp_EndTime   

        # sleep if it is a continuous loop
        Write-Information -MessageData "Starting sleep for $SleepIntervalMinutes minutes" -InformationAction Continue
        Start-Sleep -Seconds ($SleepIntervalMinutes * 60)
    }  

} while ($true -eq $isContinuousLoop)
