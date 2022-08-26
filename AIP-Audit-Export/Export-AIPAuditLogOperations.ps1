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
      Version     : 1.0.1
      Description : The script exports AIP data from the M365 Unified Audit Log and pushes it into a customer-specified Log Analytics table
#>

Import-Module ExchangeOnlineManagement

#connect to exchangeonline
Connect-ExchangeOnline 



{
       param (
       # Log Analytics table where the data is written to
       [string]$LogAnalyticsTableName,

       # Script pause interval / Sleep internal. After retrieving the logs, the script will sleep until the next run.
       [int16]$SleepIntervalMinutes,

       # Logs will be retrieved starting at this time. Default is the timestamp for now() - 1 hour
       [datetime]$StartTime,

       # If an end time is specified, the script will run only once with the logs retrieved until the specified time. If an end time is not specified, the script will loop until stopped
       [datetime]$EndTime,

       [bool]$true=1,
       [bool]$Verbose  ## ACTIONITEM: Convert this from value-param "-Verbose $true" to a flag-param "-Verbose"
       )

}

# Log Analytics table where the data is written to
$LogAnalyticsTableName = "AzureInformationProtectionLogs_CL"
# Script pause interval / Sleep internal. After retrieving the logs, the script will sleep until the next run.
$SleepIntervalMinutes = 60
# Logs will be retrieved starting at this time. Default is the timestamp for now() - 1 hour
$StartTime = [System.DateTime]::UtcNow.Adddays(-1)
# If an end time is specified, the script will run only once with the logs retrieved until the specified time. If an end time is not specified, the script will loop until stopped
$EndTime = [System.DateTime]::UtcNow
$Verbose =$true  ## ACTIONITEM: Convert this from value-param "-Verbose $true" to a flag-param "-Verbose"

if($Verbose -eq $true) 
{
       $VerbosePreference = "Continue";
}  ## ACTIONITEM:  Update the if-condition once -Verbose becomes a flag param

# Log analytics information. Learn how to find the right values here: https://docs.microsoft.com/en-us/azure/azure-monitor/agents/agent-windows?tabs=setup-wizard#workspace-id-and-key
$CustomerId = "xxxxxxxxx"  

$SharedKey =  "rxxxxxxxxx"   # Primary key for your Log Analytics workspace

$logFile = "C:\log\logforaipexport.txt"

# Only AIP-related operations are selected for export. For Full list review here: https://docs.microsoft.com/en-us/microsoft-365/compliance/search-the-audit-log-in-security-and-compliance?view=o365-worldwide
#$record = "AipDiscover"

##,"AipSensitivityLabelAction","AipProtectionAction","AipFileDeleted","AipHeartBeat"

$record =  "AzureActiveDirectory" 

# Audit export size

$resultSize = 5000 

$intervalMinutes = 60


  

      [DateTime]$start  = [System.DateTime]::UtcNow.AddHours(-11)
       # If an end time is specified, the script will run only once with the logs retrieved until the specified time. If an end time is not specified, the script will loop until stopped
       [DateTime]$end = [System.DateTime]::UtcNow

       #Start script
[DateTime]$currentStart = $start
[DateTime]$currentEnd = $end

Write-Host "Current start" +  $currentStart
Write-Host "Current end " + $currentEnd




##########################################################################################################################


#Step 0: sanity checks
#if($body -isnot [array]) {return}
#if($body.Count -eq 0) {return}

##########################################################################################################################

# Step 1 : #Write execution steps into a log File
##########################################################################################################################


Function Write-LogFile ([String]$Message)

{
    $final = [DateTime]::Now.ToUniversalTime().ToString("s") + ":" + $Message

    $final | Out-File $logFile -Append
}

Write-LogFile "BEGIN: Retrieving audit records between $($start) and $($end), RecordType=$record, PageSize=$resultSize."

Write-Host "Retrieving audit records for the date range between $($start) and $($end), RecordType=$record, ResultsSize=$resultSize"

#To hold Retrieved audit records between $($start) and $($end)

$totalCount = 0

#Loop through until current Start date is equal to current end date

While ($true)
{
      #Passing the start and end date with timeinterval.
      $currentEnd = $currentStart.AddMinutes($intervalMinutes)
      #Current end dates and adding interval minutes.
      # if current date greatert than end date - then current end date - after it - the currentend is getting assigned, in the above script.
      
      if ($currentEnd -gt $end)
      {
             $currentEnd = $end
      }
      
      if ($currentStart -eq $currentEnd)
      {
             break
      }




##########################################################################################################################
# Step 2# 

##########################################################################################################################

#Function Export-AuditLogData($start, $end) 
#{
       # ---------------------------------------------------------------   
       #    Name           : Export-AuditLogData
       #    Value          : Creates a single session defined by start and end times, and exports data within this session to Log Analytics
       #    Input          : 1) start time
       #                     2) end time
       #    Return         : None
       # ---------------------------------------------------------------

      

       $sessionID = [Guid]::NewGuid().ToString() + "_" +  "ExtractLogs" + (Get-Date).ToString("yyyyMMddHHmmssfff")

      
       Write-LogFile "INFO: Retrieving audit records for activities performed between $($currentStart) and $($currentEnd)"
      
       Write-Host "Retrieving audit records for activities performed between $($currentStart) and $($currentEnd)"

       #$sessionID = "ExtractLogs_" + $start.ToString("yyyyMMddHHmmssfff") + "_" + $end.ToString("yyyyMMddHHmmssfff")

       Write-Verbose "Start SessionId: ($sessionID);  Retrieving audit records between $($currentStart) and $($currentEnd)"
#}

##########################################################################################################################

###Loop Logic :

##########################################################################################################################
do 
{
       # Run the commandlet to search through the Audit logs and get the AIP events in the specified timeframe
       $auditLogSearchResults = Search-UnifiedAuditLog -StartDate $currentStart -EndDate $currentEnd -RecordType $record -SessionId $sessionID -SessionCommand ReturnLargeSet -ResultSize $resultSize

       ($auditLogSearchResults | Measure-Object).Count
       # Check if there is data, and push to Log Analytics
    if (($auditLogSearchResults | Measure-Object).Count -ne 0) 
       {
              #convert  string to JSON and store in $bodyjson variable
              #$bodyJson = ConvertTo-Json -InputObject $auditLogSearchResults

              $bodyJson = ConvertTo-Json -InputObject $auditLogSearchResults
              # get the UTF8 bytestream for the JSON
              $bodyJsonUTF8 = ([System.Text.Encoding]::UTF8.GetBytes($bodyJson))
              
       ##########################################################################################################################  
               
              #Step 3: build the signature 
               
       ##########################################################################################################################    
               
              $method = "POST"
              
              $contentType = "application/json"
              
              $resource = "/api/logs"
              
              $xHeaders = "x-ms-date:" + $date
              
              $rfc1123date = [DateTime]::UtcNow.ToString("r")
              
              $contentLength = $bodyJsonUTF8.Length 
              
              
              
              ## Hard coding the vales for testing later you can un comment line no: 151 and comment 153
              #$stringToHash = $method + "`n" + $contentLength + "`n" + $contentType + "`n" + $xHeaders + "`n" + $resource

              $stringToHash = "POST" + "`n" + $bodyJsonUTF8.Length + "`n" + "application/json" + "`n" + $("x-ms-date:" + [DateTime]::UtcNow.ToString("r")) + "`n" + "/api/logs"
              
       ##########################################################################################################################   
              
              #Step 4: 
              
              
              #Function Build-Signature ($customerId, $sharedKey, $date, $contentLength, $method, $contentType, $resource) {
              # ---------------------------------------------------------------   
              #    Name           : Build-Signature
              #    Value          : Creates the authorization signature used in the REST API call to Log Analytics
              # ---------------------------------------------------------------
              
              
              
              
              #$bytesToHash = [Text.Encoding]::UTF8.GetBytes($stringToHash)
              #$keyBytes = [Convert]::FromBase64String($sharedKey)
              
              #$sha256 = New-Object System.Security.Cryptography.HMACSHA256
              #$sha256.Key = $keyBytes
              #$calculatedHash = $sha256.ComputeHash($bytesToHash)
              #$encodedHash = [Convert]::ToBase64String($calculatedHash)
              #$authorization = 'SharedKey {0}:{1}' -f $customerId,$encodedHash
              #return $authorization
              
               
       ##########################################################################################################################       
               
              # ---------------------------------------------------------------   
              #    Name           : Build-Signature
              #    Value          : Creates the authorization signature used in the REST API call to Log Analytics
              # ---------------------------------------------------------------
              
              
              $bytesToHash = [Text.Encoding]::UTF8.GetBytes($stringToHash)
              
              $keyBytes = [Convert]::FromBase64String($sharedKey)
              
              $sha256 = New-Object System.Security.Cryptography.HMACSHA256
              
              $sha256.Key = $keyBytes
              
              $calculatedHash = $sha256.ComputeHash($bytesToHash)
              
              $encodedHash = [Convert]::ToBase64String($calculatedHash)
              
              $authorization = 'SharedKey {0}:{1}' -f $customerId,$encodedHash
              
              #$TimeStampField = $(Get-Date)
              
              ##return $authorization
              
       ########################################################################################################################## 
               
              #Function Post-LogAnalyticsData($body, $LogAnalyticsTableName) {
              # ---------------------------------------------------------------   
              #    Name           : Post-LogAnalyticsData
              #    Value          : Writes the data to Log Analytics using a REST API
              #    Input          : 1) PSObject with the data
              #                     2) Table name in Log Analytics
              #    Return         : None
              # ---------------------------------------------------------------
              #Step 5: REST API call
              # ---------------------------------------------------------------
              
              $uri ='https://' + $CustomerId + ".ods.opinsights.azure.com" + "/api/logs" + "?api-version=2016-04-01"   
              
              # --------------------------------------------------------------- 
              #Step 6: create the header
              # ---------------------------------------------------------------
              
              $headers = @{
                     "Authorization"= $Authorization;"Log-Type"=$LogAnalyticsTableName;"x-ms-date" = [DateTime]::UtcNow.ToString("r"); #"time-generated-field" = $TimeStampField;
              }
              
               # ---------------------------------------------------------------
              #Step 7: Response
              # --------------------------------------------------------------- 
               
               $Response = Invoke-WebRequest -Uri $uri -Method Post  -ContentType "application/json" -Headers $headers -Body $bodyJsonUTF8 -UseBasicParsing
               
               ###-ErrorAction Stop (Un comment if required)
              
              if ($Response.StatusCode -eq 200) 
              {   
                     Write-Information -MessageData "Logs are Successfully Stored in Log Analytics Workspace" -InformationAction Continue
                     ## ACTIONITEM: Write $EndTime into a file - appended. 
              }

       #################################################################################################################
              # Step 8 : Variables will hold Total records count and current batch count
       #################################################################################################################
                        

              $currentTotal = $auditLogSearchResults[0].ResultCount
                        
              $totalCount += $auditLogSearchResults.Count
                        
              $currentCount += $auditLogSearchResults.Count
                        
               Write-LogFile "INFO: Retrieved $($currentCount) audit records out of the total $($currentTotal)"
                        
              #If all records are read for the current time range, log in the text file 
              if ($currentTotal -eq $auditLogSearchResults[$auditLogSearchResults.Count - 1].ResultIndex)
              {
                  $message = "INFO: Successfully retrieved $($currentTotal) audit records for the current time range. Moving on!"
                     Write-LogFile $message
                     Write-Host "Successfully retrieved $($currentTotal) audit records for the current time range. Moving on to the next interval." -foregroundColor Yellow
                     break
              }
       }   

    #Update Current Start to continue the loop
    
}
while (($auditLogSearchResults | Measure-Object).Count -ne 0)

$currentStart = $currentEnd

}


Write-LogFile "END: Retrieving audit records between $($start) and $($end), RecordType=$record, PageSize=$resultSize, total count: $totalCount."
Write-Host "Script complete! Finished retrieving audit records for the date range between $($start) and $($end). Total count: $totalCount" -foregroundColor Green





