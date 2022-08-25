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
Version     : 1.0.0
Description : The script exports AIP data from the M365 Unified Audit Log and pushes it into a customer-specified Log Analytics table
#>



param (
    # Log Analytics table where the data is written to
    [string]$TableName = "AzureInformationProtectionLogs_CL",

    # Script pause interval / Sleep internal. After retrieving the logs, the script will sleep until the next run.
    [int16]$SleepIntervalMinutes = 60,

    # Logs will be retrieved starting at this time. Default is the timestamp for now() - 1 hour
    [datetime]$StartTime = [DateTime]::UtcNow.AddHours(-1),

    # If an end time is specified, the script will run only once with the logs retrieved until the specified time. If an end time is not specified, the script will loop until stopped
    [datetime]$EndTime = $null,

    [bool]$Verbose = $true  ## ACTIONITEM: Convert this from value-param "-Verbose $true" to a flag-param "-Verbose"
)

if($Verbose -eq $true) { $VerbosePreference = "Continue";}  ## ACTIONITEM:  Update the if-condition once -Verbose becomes a flag param

# Log analytics information. Learn how to find the right values here: <<find link>>
$CustomerId = "d7936737-b7f9-4e35-939e-cbcd2a00fccf"  
$SharedKey =  "rXjBssgozWg/QGvpijDkMCLYGxxO5WZhMeiGJ118NMRuxHK7XovP7CoqicQ47rzcEiS6Ulsl8v7dTT1evlp1NQ=="   # Primary key for your Log Analytics workspace

# Only AIP-related operations are selected for export. For Full list review here: https://docs.microsoft.com/en-us/microsoft-365/compliance/search-the-audit-log-in-security-and-compliance?view=o365-worldwide
$record = "AipDiscover","AipSensitivityLabelAction","AipProtectionAction","AipFileDeleted","AipHeartBeat"

# Audit export size
$resultSize = 100 



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
        Write-Information -MessageData "Logs are Successfully Stored in Log Analytics Workspace" -InformationAction Continue
        
        ## ACTIONITEM: Write $EndTime into a file - appended. 
    }

}


Function Export-AuditLogData($start, $end) {
    # ---------------------------------------------------------------   
    #    Name           : Export-AuditLogData
    #    Value          : Creates a single session defined by start and end times, and exports data within this session to Log Analytics
    #    Input          : 1) start time
    #                     2) end time
    #    Return         : None
    # ---------------------------------------------------------------
    
    $sessionID = "ExtractLogs_" + $start.ToString("yyyyMMddHHmmssfff") + "_" + $end.ToString("yyyyMMddHHmmssfff")
    Write-Verbose "Start SessionId: ($sessionID);  Retrieving audit records between $($start) and $($end)"

    do {
        # Run the commandlet to search through the Audit logs and get the AIP events in the specified timeframe
        $auditLogSearchResults = Search-UnifiedAuditLog -StartDate $start -EndDate $end -RecordType $record -SessionId $sessionID -SessionCommand ReturnLargeSet -ResultSize $resultSize

        # Check if there is data, and push to Log Analytics
        if (($auditLogSearchResults | Measure-Object).Count -ne 0) {  Post-LogAnalyticsData($auditLogSearchResults, $TableName);  } 
        
        # Status update
        Write-Verbose "- Retrieved and pushed " + ($auditLogSearchResults | Measure-Object).Count.ToString() + " to Log Analytics"

    } while (($auditLogSearchResults | Measure-Object).Count -ne 0)   # loop until the session has no records left to retrieve

    Write-Verbose "End SessionId: ($sessionID)"
}



#import exchange online management module
Import-Module ExchangeOnlineManagement

#connect to exchangeonline
Connect-ExchangeOnline

# main code
while ($true) {
    # set end time for one-time export
    if($null -eq $EndTime) { $temp_EndTime = [DateTime]::UtcNow }
    
    # main export operation
    Export-AuditLogData($StartTime, $temp_EndTime)    

    # persist end timestamp
    "Export complete for audit logs between Start-time: " + $temp_EndTime.ToString("yyyyMMddHHmmssfff") + " and End-Time: " $temp_EndTime.ToString("yyyyMMddHHmmssfff") | Out-File "./endTimestamp.txt" -Append 
    
    # break if one-time operation
    if($null -eq $EndTime) { break; }  

    # adjust start value
    $StartTime = $temp_EndTime   

    # sleep if there is no end time
    Start-Sleep -Seconds ($SleepIntervalMinutes * 60)
}
