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
    Script      : Azure-Blob-FiletypeAnalysis.ps1

    Author      : Aashish Ramdas

    Version     : 1.1

    Description : The script recursively enumerates the subscriptions, storage accounts, and blob containers 
    that you own. [See functions "EnumerateSubscriptions", "EnumerateStorageAccounts", and "EnumerateContainers"]
    In each storage account, the blobs are enumerated [See function "Run-MainScript"] the blob metadata is analyzed
    and aggregated using the function "ParseBlobList". The output is written to a bunch of .TXT files

    Output      : The output of this script is split into four .TXT files
                  1) SubscriptionList.txt     -  Enumerates the TenantID, SubscriptionID, and the Enabled state
                  2) StorageAccountList.txt   -  Enumerates the storage accounts with additional metadata like the Location, 
                                                 AccessTier, SubscriptionId, and TenantId
                  3) StorageContainerList.txt -  Enumerates the containers in the storage accounts with additional metadata 
                                                 like PublicAccess, BlobEndpoint, SubscriptionId, and TenantId
                  4) BlobAnalysis.txt         -  Aggregated and summarized analysis of the blob metadata, including the 
                                                 File extension, Total data size, File count etc.  

#>


<#  INPUT PARAMETER DEFINITION #>
param( [string]$InputSubscription = "",
       [bool]$AnonymizeNames = $false )

<#  GLOBAL VARIABLES  #>
$VerbosePreference   = "continue"
$DebugPreference     = "SilentlyContinue"
$Progress_Activity   = "Enumerating data sources"
$Progress_Id         = 1
$Progress_Task       = ""
$SUBSCRIPTION_LIST   = $null
$STORAGEACCOUNTLIST  = $null
$BLOB_CONTAINER_LIST = $null
$blobanalysis_hashtable = @{}

<#  INITIAL SETUP  #>
$FOLDER       = (Get-Date -Format "MM.dd.yyyy-hh.mm").ToString() + " AzBlob_Analysis"
$FILE_sublist = "./$FOLDER/SubscriptionList.txt"
$FILE_storacc = "./$FOLDER/StorageAccountList.txt"
$FILE_storcon = "./$FOLDER/StorageContainerList.txt"
$FILE_blobsum = "./$FOLDER/BlobAnalysis.txt"
$dir = New-Item -Name $FOLDER -ItemType "directory" -Force


function EnumerateSubscriptions {
    
    # ---------------------------------------------------------------   
    #    Function       : EnumerateSubscriptions
    #
    #    Input          : None
    #
    #    Output         : Array of PSObject
    #                     [
    #                        TenantId
    #                        SubscriptionId
    #                        State
    #                     ]
    #
    #    File Output    : SubscriptionList.txt
    # ---------------------------------------------------------------


    $Progress_Task     = "Enumerating subscriptions"
    Write-Progress -Id $Progress_Id -Activity $Progress_Activity -Status $Progress_Task
    
    $subscriptions = Get-AzSubscription | Select-Object TenantId,SubscriptionId,State
    if($subscriptions -eq $null   )    { $subscriptions = @() }  
    if($subscriptions -isnot [array] ) { $subscriptions = @($subscriptions)  }


    #Write output to file
    $subscriptions | Export-CSV $FILE_sublist -NoTypeInformation
    Write-Verbose -Message ("Subscription: " + $subscriptions.Count.ToString() + " [count] , File location: " + $FILE_sublist)

    return $subscriptions
}



function EnumerateStorageAccounts {
    param( [PSObject]$subscription )

    # ---------------------------------------------------------------
    # Function       : EnumerateStorageAccounts
    #
    # Input          : 1) Subscription info
    #                  2) percent completed status 
    #
    # Output         : Array of PSObject
    #                  [
    #                     StorageAccountName
    #                     Id
    #                     Location
    #                     AccessTier
    #                     Context
    #                     PrimaryEndpoints
    #                     TenantId
    #                     SubscriptionId
    #                  ]
    # 
    # File Output    : StorageAccountList.txt
    # ---------------------------------------------------------------

    ## Update which subscription is being enumerated
    $Progress_Task     = "Enumerating storage accounts for subscription " + $subscription.SubscriptionId
    Write-Progress -Id $Progress_Id -Activity $Progress_Activity -Status $Progress_Task
    
    ## Set the subscription context, if the current context is different
    if( (Get-AzContext).Id -ne $subscription.SubscriptionId ) {  $a = Set-AzContext (Get-AzSubscription -SubscriptionId $subscription.SubscriptionId -Verbose) -Verbose  }

    ## Enumerate the storage accounts in the subscription
    $saList = Get-AzStorageAccount | Select-Object StorageAccountName, Id, Location, AccessTier, Context, PrimaryEndpoints 
    if($saList -eq $null   )    { $saList = @() }  
    if($saList -isnot [array] ) { $saList = @($saList)  }

    ## Merge tenant and subscription info
    foreach($sa in $saList) 
    { 
        Add-Member -InputObject $sa -MemberType NoteProperty -Name "TenantId" -Value $subscription.TenantId 
        Add-Member -InputObject $sa -MemberType NoteProperty -Name "SubscriptionId" -Value $subscription.SubscriptionId 
    }

    ## Write output to file
    $saList | Export-CSV $FILE_storacc -NoTypeInformation -Append
    Write-Verbose -Message ("Subscription: " + $subscription.SubscriptionId + " , Storage Account: " + $saList.Count.ToString() + " [count] , File location: " + $FILE_storacc)

    return $saList
}



function EnumerateBlobStorageContainers {
    param( [PSObject]$storageAccount )

    # ---------------------------------------------------------------   
    # Function       : EnumerateBlobStorageContainers 
    # 
    # Input          : 1) storage account info
    #                  2) percent completed status
    # 
    # Output         : Array of PSObject
    #                  [
    #                     Name
    #                     PublicAccess
    #                     Context
    #                     TenantId
    #                     SubscriptionId
    #                     StorageAccountName
    #                     BlobEndpoint
    #                  ]
    # 
    # File Output    : StorageContainerList.txt
    # ---------------------------------------------------------------


    $Progress_Task     = "Enumerating containers for Storage Account " + $storageAccount.StorageAccountName
    Write-Progress -Id $Progress_Id -Activity $Progress_Activity -Status $Progress_Task

    ## Enumerate the blob storage containers in the storage account
    $local:containerList = $null
    $containerList = Get-AzStorageContainer -Context $storageAccount.Context | Select-Object Name, PublicAccess, Context
    if($containerList -eq $null   )    { $containerList = @() }  
    if($containerList -isnot [array] ) { $containerList = @($containerList)  }


    ## Merge tenant and subscription info
    foreach($c in $containerList) 
    { 
        Add-Member -InputObject $c -MemberType NoteProperty -Name "TenantId" -Value $storageAccount.TenantId 
        Add-Member -InputObject $c -MemberType NoteProperty -Name "SubscriptionId" -Value $storageAccount.SubscriptionId 
        Add-Member -InputObject $c -MemberType NoteProperty -Name "StorageAccountName" -Value $storageAccount.StorageAccountName
        Add-Member -InputObject $c -MemberType NoteProperty -Name "BlobEndpoint" -Value $storageAccount.PrimaryEndpoints.Blob
    }

    ## Write output to file
    $containerList | Export-CSV $FILE_storcon -NoTypeInformation -Append
    Write-Verbose -Message ("Subscription: " + $storageAccount.SubscriptionId  + " , Storage Account: " + $acc.StorageAccountName + " , Container : " + $containerList.Count.ToString() + " [count] , File location: " + $FILE_storcon)

    return $containerList
}



function ParseBlobList {
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
        $fileext = [System.IO.Path]::GetExtension($b.Name)   
        #$fileext = Split-Path $b.Name -Extension   #PowerShell version 6.0 compatible; for machines that dont have access to System.IO. 

        #add empty new entry IF hashtable didn't contain the file extension object
        if($blobanalysis_hashtable[$fileext] -eq $null) 
        {
            $f = [PSCustomObject]@{    
                TenantID           = $container.TenantId
                SubscriptionID     = $container.SubscriptionId
                StorageAccountURL  = $container.BlobEndpoint
                ContainerName      = $container.Name
                ContainerType      = $b.BlobType
                ContainerPublicAccess = $container.PublicAccess
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



#### C++ main() equivalent ####
function Run-MainScript {

    ## Get a list of SUBSCRIPTIONS
    $SUBSCRIPTION_LIST = (EnumerateSubscriptions)
    
    ## if an explicit subscription has been provided, filter list to just this subscription
    $filtered = $false
    if($InputSubscription -ne "")
    {  
        foreach($s in $SUBSCRIPTION_LIST)
        {
            if( $s.SubscriptionId -eq $InputSubscription )
            {
                $SUBSCRIPTION_LIST = @($s)
                $filtered = $true
                break;
            }
        }

        #check if filtering actually worked
        if($filtered -eq $false) {  Write-Error "Input subscription was not found in the list of enumerated subscriptions! Exiting script..." -Category InvalidArgument ; return;  }
    }
    
    ## Get a list of STORAGE ACCOUNTs
    foreach( $sub in $SUBSCRIPTION_LIST )   
    {  
        $STORAGEACCOUNTLIST += (EnumerateStorageAccounts -subscription $sub)   
    }

    ## Get a list of BLOB STORAGE CONTAINERS
    foreach( $acc in $STORAGEACCOUNTLIST )  
    {  
        $BLOB_CONTAINER_LIST += (EnumerateBlobStorageContainers -storageAccount $acc)
        if( $BLOB_CONTAINER_LIST -isnot [array] ) { $BLOB_CONTAINER_LIST = @($BLOB_CONTAINER_LIST) }   
    }

    ## Enumerate and Analyze blobs in each container
    foreach($c in $BLOB_CONTAINER_LIST)
    {
        $c_Token = $null
        $c_total = 0
        $blobanalysis_hashtable = @{}

        do #enumerate blobs in batches of 10000 and analyze
        {
            #show progress
            $Progress_Task     = "Enumerating blobs for container " + $c.Name + ", batch " + $c_total.ToString() + " - " + ($c_total+10000).ToString()
            Write-Progress -Id $Progress_Id -Activity $Progress_Activity -Status $Progress_Task

            #enumerate
            $bloblist = Get-AzStorageBlob -Context $c.Context -Container $c.Name -MaxCount 10000 -ContinuationToken $c_Token
            $c_total += $bloblist.Count
            Write-Verbose -Message ("Subscription: " + $c.SubscriptionId  + " , Storage Account: " + $c.StorageAccountName + " , Container: " + $c.Name  + " , Enumerated : " + $c_total)

            #analyze
            ParseBlobList -container $c -blobs $bloblist -StorAccAccessTier $acc.AccessTier

            #prep for next fetch
            if($bloblist.Count -le 0) { break; }
            else                       { $c_Token = $bloblist[$bloblist.Count - 1].ContinuationToken }
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
        $outputarray | Export-CSV $FILE_blobsum -NoTypeInformation -Append
        Write-Verbose -Message ("Subscription: " + $c.SubscriptionId  + " , Storage Account: " + $c.StorageAccountName + " , Container: " + (split-path -path $c.Name -leaf) + " , File location: " + $FILE_blobsum)

    }
}

Run-MainScript
