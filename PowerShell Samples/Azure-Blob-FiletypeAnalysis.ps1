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
Author      : Aashish Ramdas
Co-Author   : Chris Boehm
Version     : 3.0.1
Description : The script recursively enumerates the subscriptions and data resources that you own.
Output      : The output of this script is split into four tables in Log Analytics
              
              1) Table : IP4A_AZSUBSCRIPTIONS
                 Fields: CustomerTenantID, SubscriptionID, State

              2) Table : IP4A_AZRESOURCES
                 Fields: CustomerTenantID, SubscriptionID, ResourceGroupName, ResourceType, ResourceName, Location
                 
                 ResourceType = Microsoft.Storage/StorageAccount
                                Microsoft.Sql/servers
                                Microsoft.DocumentDb/databaseAccounts

              3) Table : IP4A_AZDATACONTAINERS 
                 Fields: CustomerTenantID, SubscriptionID, ContainerType, ContainerName, ContainerURL, ResourceType, ResourceName, ResourceURL, Location, PublicAccess

                 ContainerType = Microsoft.Storage/StorageAccount/BlobContainer
                                 Microsoft.Storage/StorageAccount/FileShare
                                 Microsoft.Storage/StorageAccount/DataLakeGen2
                                 Microsoft.Sql/servers/databases

              4) Table : IP4A_BlobAnalysis         
                 Fields: (see function for PSObject)
#>



# INPUT PARAMETER DEFINITION
param( [string]$InputSubscription = "",
       [bool]$AnonymizeNames = $false)
       

# GLOBAL VARIABLES
$VerbosePreference   = "continue"
$DebugPreference     = "SilentlyContinue"
$Progress_Activity   = "Enumerating data sources"
$Progress_Task       = ""
$AZSUBSCRIPTIONS     = @()
$AZRESOURCES         = @()
$AZDATACONTAINERS    = @()
$blobanalysis_hashtable = @{}

# INITIAL SETUP
$LogAnalyticsWorkspaceId = "<<insert workspaceID from step 3 in documentation here>>"
$LogAnalyticsPrimaryKey = "<<insert PrimaryKey from step 3 in documentation here>>"
$TenantId = "<<insert TenantID here>>"
$LOGNAME_sublist   = "IP4A_AZSUBSCRIPTIONS"
$LOGNAME_azres     = "IP4A_AZRESOURCES"
$LOGNAME_container = "IP4A_AZDATACONTAINERS"
$LOGNAME_blobsum   = "IP4A_BLOBANALYSIS"



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
    #    Output         : None
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
}


function ParseBlobs {
    param( [PSObject]$blobs,
           [PSObject]$container,
           [string]$StorAccAccessTier )
    
    # ---------------------------------------------------------------   
    # Function       : ParseBlobList 
    # 
    # Input          : 1) Array of blob objects
    #                  2) blob container
    #                  3) storage account access tier
    # 
    # Output         : none
    # 
    # File Output    : BlobAnalysis.txt
    # ---------------------------------------------------------------


    foreach($b in $blobs)
    {
        $fileext = [System.IO.Path]::GetExtension($b.Name)    #old style
        #$fileext = Split-Path $b.Name -Extension   #PowerShell version 6.0 compatible; for machines that dont have access to System.IO. 

        #add empty new entry IF hashtable didn't contain the file extension object
        if($blobanalysis_hashtable[$fileext] -eq $null) 
        {
            $f = [PSCustomObject]@{    
                CustomerTenantID   = $container.CustomerTenantId
                SubscriptionID     = $container.SubscriptionId
                StorageAccountURL  = $container.BlobEndpoint
                ContainerName      = $container.ContainerName
                ContainerType      = $container.ContainerType
                ContainerAccess    = $container.ContainerAccess
                Extension          = $fileext
                ContentType        = $b.ContentType

                TotalFileCount     = 0
                LT1KBFileCount     = 0  #count of files <1KB
                LT10KBFileCount    = 0  #count of files >=1KB and <10KB
                LT100KBFileCount   = 0  #count of files >=10KB and <100KB
                LT1MBFileCount     = 0  #count of files >=100KB and <1MB
                LT10MBFileCount    = 0  #count of files >=1MB and <10MB
                GE10MBFileCount    = 0  #count of files >=10MB
                HotTierFileCount   = 0
                CoolTierFileCount  = 0
                OtherTierFileCount = 0

                TotalSize          = 0
                LT1KBFileSize      = 0
                LT10KBFileSize     = 0
                LT100KBFileSize    = 0
                LT1MBFileSize      = 0
                LT10MBFileSize     = 0
                GE10MBFileSize     = 0
                HotTierFileSize    = 0
                CoolTierFileSize   = 0
                OtherTierFileSize  = 0
            }

            $blobanalysis_hashtable.Add($fileext, $f) 
        }
        
        #increment 
        $temp = $blobanalysis_hashtable[$fileext]

            $temp.TotalFileCount     += 1
            $temp.TotalSize          += $b.Length

            $temp.LT1KBFileCount     += if($b.Length -lt 1024) { 1 } else { 0 }
            $temp.LT10KBFileCount    += if($b.Length -lt 10240  -and $b.Length -ge 1024)  { 1 } else { 0 }
            $temp.LT100KBFileCount   += if($b.Length -lt 102400 -and $b.Length -ge 10240) { 1 } else { 0 }
            $temp.LT1MBFileCount     += if($b.Length -lt 1024*1024 -and $b.Length -ge 102400) { 1 } else { 0 }
            $temp.LT10MBFileCount    += if($b.Length -lt 1024*1024*10 -and $b.Length -ge 1024*1024) { 1 } else { 0 }
            $temp.GE10MBFileCount    += if($b.Length -ge 1024*1024*10) { 1 } else { 0 }
            $temp.HotTierFileCount   += if(($b.ICloudBlob.AccessTier -eq $null -and $StorAccAccessTier -eq "Hot") -or $b.ICloudBlob.AccessTier -eq "Hot") { 1 } else { 0 }
            $temp.CoolTierFileCount  += if(($b.ICloudBlob.AccessTier -eq $null -and $StorAccAccessTier -eq "Cool") -or $b.ICloudBlob.AccessTier -eq "Cool") { 1 } else { 0 }
            $temp.OtherTierFileCount += if($b.ICloudBlob.AccessTier -ne $null -and $b.AccessTier -ne "Cool" -and $b.AccessTier -ne "Hot") { 1 } else { 0 }

            $temp.LT1KBFileSize      += if($b.Length -lt 1024) { $b.Length } else { 0 }
            $temp.LT10KBFileSize     += if($b.Length -lt 10240  -and $b.Length -ge 1024)  { $b.Length } else { 0 }
            $temp.LT100KBFileSize    += if($b.Length -lt 102400 -and $b.Length -ge 10240) { $b.Length } else { 0 }
            $temp.LT1MBFileSize      += if($b.Length -lt 1024*1024 -and $b.Length -ge 102400) { $b.Length } else { 0 }
            $temp.LT10MBFileSize     += if($b.Length -lt 1024*1024*10 -and $b.Length -ge 1024*1024) { $b.Length } else { 0 }
            $temp.GE10MBFileSize     += if($b.Length -ge 1024*1024*10) { $b.Length } else { 0 }
            $temp.HotTierFileSize    += if(($b.ICloudBlob.AccessTier -eq $null -and $StorAccAccessTier -eq "Hot") -or $b.ICloudBlob.AccessTier -eq "Hot") { $b.Length } else { 0 }
            $temp.CoolTierFileSize   += if(($b.ICloudBlob.AccessTier -eq $null -and $StorAccAccessTier -eq "Cool") -or $b.ICloudBlob.AccessTier -eq "Cool")  { $b.Length } else { 0 }
            $temp.OtherTierFileSize  += if($b.ICloudBlob.AccessTier -ne $null -and $b.AccessTier -ne "Cool" -and $b.AccessTier -ne "Hot") { $b.Length } else { 0 }

        $blobanalysis_hashtable[$fileext] = $temp
    }
}


function EnumerateSubscriptions {
    
    # ---------------------------------------------------------------   
    #    Function       : EnumerateSubscriptions
    #    Input          : None
    #    Output         : Array of PSObject written to IP4A_AZSUBSCRIPTIONS table
    # ---------------------------------------------------------------
    
    $subscriptions = Get-AzSubscription -TenantId $TenantId | Select-Object TenantId,SubscriptionId,State
    if($subscriptions -eq $null   )    { $subscriptions = @() }  
    if($subscriptions -isnot [array] ) { $subscriptions = @($subscriptions)  }


    $azsub = @()
    foreach($s in $subscriptions)  
    {
        $new_s = [PSCustomObject]@{    
            CustomerTenantID   = $s.TenantId
            SubscriptionID     = $s.SubscriptionId
            State              = $s.State
        }

        $azsub += $new_s
    }

    if($azsub.Count -ne 0) {
        $azsub | Export-CSV "./subscriptions.txt" -NoTypeInformation
        Write-Verbose -Message ("Subscription: " + $azsub.Count.ToString() + " [count] , Log Analytics table: " + $LOGNAME_sublist)
        Post-LogAnalyticsData -LogAnalyticsTableName $LOGNAME_sublist -body $azsub
    }
    else {
        Write-Verbose -Message ("Subscription: " + $azsub.Count.ToString() + " [count] , NO DATA WRITTEN TO LOG ANALYTICS!")
    }

    return $azsub
}


function EnumerateResources {
    param( [PSObject]$subscription )

    Write-Verbose -Message ("Enumerating data resources for subscription: " + $subscription.SubscriptionID)
    
    $res = @()
    $res = $res + (Get-AzResource -ResourceType "Microsoft.Storage/storageaccounts")       #Blobs, Files, ADLS
    $res = $res + (Get-AzResource -ResourceType "Microsoft.Sql/servers")                   #Azure SQL
    $res = $res + (Get-AzResource -ResourceType "Microsoft.DocumentDb/databaseAccounts")   #CosmosDB


    $azres = @()
    foreach($r in $res)  
    {
        $new_r = [PSCustomObject]@{    
            CustomerTenantID   = $subscription.CustomerTenantId
            SubscriptionID     = $subscription.SubscriptionId
            ResourceGroupName  = $r.ResourceGroupName
            ResourceType       = $r.ResourceType
            ResourceName       = $r.Name
            Location           = $r.location
        }

        $azres += $new_r
    }

    if($azres.Count -ne 0) {
        $azres | Export-CSV "./azres.txt" -NoTypeInformation -Append
        Write-Verbose -Message ("Subscription: " + $subscription.SubscriptionId + " , Resources: " + $azres.Count.ToString() + " [count] , Log Analytics table: " + $LOGNAME_azres)
        Post-LogAnalyticsData -LogAnalyticsTableName $LOGNAME_azres -body $azres
    }
    else {
        Write-Verbose -Message ("Subscription: " + $subscription.SubscriptionId + " , Resources: " + $azres.Count.ToString() + " [count] , NO DATA WRITTEN TO LOG ANALYTICS!")
    }

    return $azres
}


function EnumerateContainers {
    param( [PSObject]$Subscription, 
           [PSObject]$Resources )

    $containers = @()

    Write-Verbose "Enumerating containers"

    # Parse storage resources
    $Resources_Storage = $Resources | Where-Object {$_.ResourceType -eq 'Microsoft.Storage/storageaccounts'}
    
    foreach( $r in $Resources_Storage )
    {
        # get storage account 
        $local:sa = Get-AzStorageAccount -ResourceGroupName $r.ResourceGroupName -Name $r.ResourceName  | Select-Object StorageAccountName, Id, Location, AccessTier, Context, PrimaryEndpoints, EnableHierarchicalNamespace

        # get blob/adls containers + file shares
        $local:blobcon    = Get-AzStorageContainer -Context $sa.Context
        $local:fileshare  = Get-AzStorageShare -Context $sa.Context

        # cleanup
        if($blobcon -eq $null   )    { $blobcon = @() }  
        if($blobcon -isnot [array] ) { $blobcon = @($blobcon)  }
        if($fileshare -eq $null   )    { $fileshare = @() }  
        if($fileshare -isnot [array] ) { $fileshare = @($fileshare)  }


        foreach($b in $blobcon)  
        {
            $c = [PSCustomObject]@{    
                CustomerTenantID   = $r.CustomerTenantId
                SubscriptionID     = $r.SubscriptionId
                ResourceGroupName  = $r.ResourceGroupName
                ResourceType       = $r.ResourceType
                ResourceName       = $r.ResourceName
                Location           = $r.location
                ContainerName      = $b.Name
                ContainerType      = if ($sa.EnableHierarchicalNamespace -eq $true) { "Azure Data Lake Storage" } else { "Azure Blob Storage" } 
                ContainerUrl       = $sa.Context.BlobEndPoint
                ContainerAccess    = $b.PublicAccess
            }

            $containers += $c
        }

        foreach($fs in $fileshare)  
        {
            $c = [PSCustomObject]@{    
                CustomerTenantID   = $r.CustomerTenantId
                SubscriptionID     = $r.SubscriptionId
                ResourceGroupName  = $r.ResourceGroupName
                ResourceType       = $r.ResourceType
                ResourceName       = $r.ResourceName
                Location           = $r.location
                ContainerName      = $fs.Name
                ContainerType      = "Azure Files Share"
                ContainerUrl       = $fs.Uri
                ContainerAccess    = ""
            }

            $containers += $c
        }

    }


    # Parse SQL servers
    $Resources_SQL = $Resources | Where-Object {$_.ResourceType -eq 'Microsoft.Sql/servers'}
    foreach( $r in $Resources_SQL )
    {
        $local:sqldbs = Get-AzSqlDatabase -ResourceGroupName $r.ResourceGroupName -ServerName $r.ResourceName
        if($sqldbs -eq $null   )    { $sqldbs = @() }  
        if($sqldbs -isnot [array] ) { $sqldbs = @($sqldbs)  }

        foreach($db in $sqldbs)  
        {
            $c = [PSCustomObject]@{    
                CustomerTenantID   = $r.CustomerTenantId
                SubscriptionID     = $r.SubscriptionId
                ResourceGroupName  = $r.ResourceGroupName
                ResourceType       = $r.ResourceType
                ResourceName       = $r.ResourceName
                Location           = $r.location
                ContainerName      = $db.DatabaseName
                ContainerType      = "Azure SQL Database"
                ContainerUrl       = $db.ResourceId
                ContainerAccess    = ""
            }

            $containers += $c
        }

    }


    # Parse CosmosDB
    <#
    $Resources_CosmosDB = $Resources | Where-Object {$_.ResourceType -eq 'Microsoft.DocumentDb/databaseAccounts'}
    foreach( $r in $Resources_CosmosDB )
    {
        $local:cosmosdbs = Get-AzResource -ResourceType "Microsoft.DocumentDb/databaseAccounts/apis/databases" -ResourceGroupName $r.ResourceGroupName -Name ($ResourceName+"/sql/")
        if($cosmosdbs -eq $null   )    { $cosmosdbs = @() }  
        if($cosmosdbs -isnot [array] ) { $cosmosdbs = @($cosmosdbs)  }

        foreach($db in $cosmosdbs)  
        {
            $c = [PSCustomObject]@{    
                TenantID           = $r.TenantId
                SubscriptionID     = $r.SubscriptionId
                ResourceGroupName  = $r.ResourceGroupName
                ResourceType       = $r.ResourceType
                ResourceName       = $r.ResourceName
                Location           = $r.location
                ContainerName      = $db.DatabaseName
                ContainerType      = "Azure SQL Database"
                ContainerUrl       = $db.ResourceId
                ContainerAccess    = ""
            }

            $containers += $c
        }
    }
    #>


    if($containers.Count -ne 0) {
        $containers | Export-CSV "./containers.txt" -NoTypeInformation -Append
        Write-Verbose -Message ("Subscription: " + $subscription.SubscriptionId + " , Resources: " + $azres.Count.ToString() + " [count] , Log Analytics table: " + $LOGNAME_azres)
        Post-LogAnalyticsData -LogAnalyticsTableName $LOGNAME_container -body $containers
    }
    else {
        Write-Verbose -Message ("Subscription: " + $subscription.SubscriptionId + " , Resources: " + $azres.Count.ToString() + " [count] , NO DATA WRITTEN TO LOG ANALYTICS!")
    }

    return $containers
}


function BlobFiletypeAnalysis {
    param( [PSObject]$container )

    # Set the subscription context, if the current context is different
    if( (Get-AzContext).Subscription -ne $container.SubscriptionID ) {  $a = Set-AzContext (Get-AzSubscription -TenantId $TenantId -SubscriptionId $container.SubscriptionID -Verbose) -Verbose  }

    # Set the storage account context
    $local:sa = Get-AzStorageAccount -ResourceGroupName $container.ResourceGroupName -Name $container.ResourceName  | Select-Object StorageAccountName, Id, Location, AccessTier, Context, PrimaryEndpoints

    #read blob data in batches of 10000 and analyze
    $c_Token = $null
    $c_total = 0
    $blobanalysis_hashtable = @{}        
    do 
    {
        #enumerate
        $blobs = Get-AzStorageBlob -Context $sa.Context -Container $container.ContainerName -MaxCount 10000 -ContinuationToken $c_Token
        $c_total += $blobs.Count
        Write-Verbose -Message ("Subscription: " + $container.SubscriptionID  + " , Storage Account: " + $container.ResourceName + " , Container: " + $container.ContainerName  + " , Enumerated : " + $c_total)

        #analyze
        Parseblobs -container $container -blobs $blobs -StorAccAccessTier $sa.AccessTier

        #prep for next fetch
        if($blobs.Count -le 0) { break; }
        else                   { $c_Token = $blobs[$blobs.Count - 1].ContinuationToken }
    } 
    while ($c_Token -ne $null)


    #anonymize StorageAccountURL and ContainerNames if the -Anonymize parameter is set to $true
    if( $AnonymizeNames -eq $true )
    {
        $sha256 = [System.Security.Cryptography.HashAlgorithm]::Create('sha256')
        foreach ($k in $blobanalysis_hashtable.Keys) 
        { 
            $blobanalysis_hashtable[$k].StorageAccountURL = [System.BitConverter]::ToString($sha256.ComputeHash([System.Text.Encoding]::UTF8.GetBytes($blobanalysis_hashtable[$k].StorageAccountURL))).replace('-', '')
            $blobanalysis_hashtable[$k].ContainerName = [System.BitConverter]::ToString($sha256.ComputeHash([System.Text.Encoding]::UTF8.GetBytes($blobanalysis_hashtable[$k].ContainerName))).replace('-','')
        }
    }

    #convert hashtable to array for easy csv export
    $outputarray = @()
    foreach($k in $blobanalysis_hashtable.Keys) {  $outputarray += $blobanalysis_hashtable[$k] }

    #write output to csv
    $outputarray | Export-CSV "./blobanalysis.txt" -NoTypeInformation -Append
    Post-LogAnalyticsData -LogAnalyticsTableName $LOGNAME_blobsum -body $outputarray

}


#### C++ main() equivalent ####
function Run-MainScript {

    # Checks Tenant - If not logged into the configured tenant script will establish az-context to appropate tenant
    $ChckTenantID = Get-AzContext
    if(($ChckTenantID.Tenant.Id) -eq $TenantId) { } else { Clear-AzContext -Force
    Connect-AzAccount -Tenant $TenantId }

    # Get a list of SUBSCRIPTIONS
    $AZSUBSCRIPTIONS += (EnumerateSubscriptions)
    
    # if an explicit subscription has been provided, filter list to just this subscription
    if($InputSubscription -ne "") { $AZSUBSCRIPTIONS = $AZSUBSCRIPTIONS | Where-Object { $_.SubscriptionId -eq $InputSubscription } }
    if($AZSUBSCRIPTIONS.Count -eq 0) { Write-Error "Input subscription was not found in the list of enumerated subscriptions! Exiting script..." -Category InvalidArgument ; return;  }
 
    # Get a list of resources and containers within each subscription
    foreach( $sub in $AZSUBSCRIPTIONS )   
    {  
        Set-AzContext (Get-AzSubscription -TenantId $TenantId -SubscriptionId $sub.SubscriptionID -Verbose) -Verbose
        $AZRESOURCES += (EnumerateResources -subscription $sub)
        $AZDATACONTAINERS += (EnumerateContainers -Subscription $sub -Resources $AZRESOURCES)
    }

    # Run filetype analysis on the blobs in the container
    $blob_containers = $AZDATACONTAINERS | Where-Object { $_.ContainerType -eq "Azure Blob Storage" }
    foreach( $c in $blob_containers )   {   BlobFiletypeAnalysis -container $c   }
}

Run-MainScript
