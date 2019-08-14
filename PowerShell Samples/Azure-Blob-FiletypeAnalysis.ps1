$VerbosePreference = "continue"
$DebugPreference   = "SilentlyContinue"



function EnumerateSubscriptions {
    $Activity = "Enumerating data sources"
    $Id       = 1
    $Task     = "Enumerating subscriptions"
    Write-Progress -Id $Id -Activity $Activity -Status $Task
    
    $subscriptions = Get-AzSubscription | Select-Object TenantId,SubscriptionId,State

    ## Write verbose message
    if($subscriptions -eq $null   ) { $message = "No Azure subscription found" }
    if($subscriptions -is [array] ) { $message = $subscriptions.Count.ToString() + " subscriptions found" }
    else                            { $message = "1 subscription found" }
    Write-Verbose -Message $message

    return $subscriptions
}


function EnumerateStorageAccounts {
    param( [PSObject]$subscription, 
           [int]$percentCompleted )

    $Activity = "Enumerating data sources"
    $Id       = 1
    $Task     = "Enumerating storage accounts for subscription " + $subscription.SubscriptionId
    Write-Progress -Id $Id -Activity $Activity -Status $Task -PercentComplete $percentCompleted
    
    ## Set the subscription context
    $local:context = Get-AzSubscription -SubscriptionId $subscription.SubscriptionId -Verbose
    $x = Set-AzContext $context -Verbose

    ## Get a list of storage accounts in te subscription
    $local:saList = Get-AzStorageAccount | Select-Object StorageAccountName, Id, Location, AccessTier, Context, PrimaryEndpoints 
    
    #merge tenant and subscription info
    foreach($sa in $saList) 
    { 
        Add-Member -InputObject $sa -MemberType NoteProperty -Name "TenantId" -Value $subscription.TenantId 
        Add-Member -InputObject $sa -MemberType NoteProperty -Name "SubscriptionId" -Value $subscription.SubscriptionId 
    }

    ## Write verbose message
    if($saList -eq $null   ) { $message = "Subscription " + $subscription.SubscriptionId + "doesn't have storage accounts" }
    if($saList -is [array] ) { $message = $storageAccountList.Count.ToString() + " storage accounts found in subscription " + $subscription.SubscriptionId }
    else                     { $message = "1 storage account found in subscription " + $subscription.SubscriptionId }
    Write-Debug -Message $message

    return $saList
}


function EnumerateContainers {
    param( [PSObject]$storageAccount,
           [int]$percentCompleted )

    $Activity = "Enumerating data sources"
    $Id       = 1
    $Task     = "Enumerating containers for Storage Account " + $storageAccountContext.StorageAccountName
    Write-Progress -Id $Id -Activity $Activity -Status $Task -PercentComplete $percentCompleted

    $local:containerList = $null
    $containerList = Get-AzStorageContainer -Context $storageAccount.Context | Select-Object Name, PublicAccess, Context
    
    #merge tenant and subscription info
    foreach($c in $containerList) 
    { 
        Add-Member -InputObject $c -MemberType NoteProperty -Name "TenantId" -Value $storageAccount.TenantId 
        Add-Member -InputObject $c -MemberType NoteProperty -Name "SubscriptionId" -Value $storageAccount.SubscriptionId 
        Add-Member -InputObject $c -MemberType NoteProperty -Name "StorageAccountName" -Value $storageAccount.StorageAccountName
        Add-Member -InputObject $c -MemberType NoteProperty -Name "BlobEndpoint" -Value $storageAccount.PrimaryEndpoints.Blob
    }

    if     ($containerList -eq $null   ) { $message = "No container found in storage acccount " + $storageAccountContext.StorageAccountName }
    elseif ($containerList -is [array] ) { $message = $containerList.Count.ToString() + " containers found in storage account " + $storageAccountContext.StorageAccountName }
    else                                 { $message = "1 container found in storage account " + $storageAccountContext.StorageAccountName }
    Write-Verbose -Message $message

    return $containerList
}


<#  FILE EXTENSION ANALYSIS - DATA STRUCTURE DEFINITION

    Hashtable
    ---------
    Key   : FileExt (string)
    Value : PSObejct
            [
                TenantID
                SubscriptionID
                StorageAccountURL
                ContainerName
                ContainerType
                ContainerPublicAccess
                Extension
                ContentType

                TotalFileCount
                LT1KBFileCount    (count of files <1KB)
                LT10KBFileCount   (count of files >=1KB and <10KB)
                LT100KBFileCount  (count of files >=10KB and <100KB)
                LT1MBFileCount    (count of files >=100KB and <1MB)
                LT10MBFileCount   (count of files >=1MB and <10MB)
                GE10MBFileCount   (count of files >=10MB)
                HotTierFileCount
                CoolTierFileCount
                OtherTierFileCount
                
                TotalSize
                LT1KBFileSize      
                LT10KBFileSize     
                LT100KBFileSize    
                LT1MBFileSize      
                LT10MBFileSize     
                GE10MBFileSize
                HotTierFileSize
                CoolTierFileSize
                OtherTierFileSize
                
            ]                       
#>
function ParseBlobList {
    param( [PSObject]$blobs,
           [PSObject]$container,
           [string]$filepath,
           [string]$StorAccAccessTier )
    
    $blobanalysis_hashtable = @{}

    foreach($b in $blobs)
    {
        $fileext = [System.IO.Path]::GetExtension($b.Name)
        if($blobanalysis_hashtable[$fileext] -eq $null) #add new
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

                TotalFileCount     = 1
                LT1KBFileCount     = if($b.Length -lt 1024) { 1 } else { 0 }
                LT10KBFileCount    = if($b.Length -lt 10240  -and $b.Length -ge 1024)  { 1 } else { 0 }
                LT100KBFileCount   = if($b.Length -lt 102400 -and $b.Length -ge 10240) { 1 } else { 0 }
                LT1MBFileCount     = if($b.Length -lt 1024*1024 -and $b.Length -ge 102400) { 1 } else { 0 }
                LT10MBFileCount    = if($b.Length -lt 1024*1024*10 -and $b.Length -ge 1024*1024) { 1 } else { 0 }
                GE10MBFileCount    = if($b.Length -ge 1024*1024*10) { 1 } else { 0 }
                HotTierFileCount   = if(($b.ICloudBlob.AccessTier -eq $null -and $StorAccAccessTier -eq "Hot") -or $b.ICloudBlob.AccessTier -eq "Hot") { 1 } else { 0 }
                CoolTierFileCount  = if(($b.ICloudBlob.AccessTier -eq $null -and $StorAccAccessTier -eq "Cool") -or $b.ICloudBlob.AccessTier -eq "Cool") { 1 } else { 0 }
                OtherTierFileCount = if($b.ICloudBlob.AccessTier -ne $null -and $b.AccessTier -ne "Cool" -and $b.AccessTier -ne "Hot") { 1 } else { 0 }

                TotalSize          = $b.Length
                LT1KBFileSize      = if($b.Length -lt 1024) { $b.Length } else { 0 }
                LT10KBFileSize     = if($b.Length -lt 10240  -and $b.Length -ge 1024)  { $b.Length } else { 0 }
                LT100KBFileSize    = if($b.Length -lt 102400 -and $b.Length -ge 10240) { $b.Length } else { 0 }
                LT1MBFileSize      = if($b.Length -lt 1024*1024 -and $b.Length -ge 102400) { $b.Length } else { 0 }
                LT10MBFileSize     = if($b.Length -lt 1024*1024*10 -and $b.Length -ge 1024*1024) { $b.Length } else { 0 }
                GE10MBFileSize     = if($b.Length -ge 1024*1024*10) { $b.Length } else { 0 }
                HotTierFileSize    = if(($b.ICloudBlob.AccessTier -eq $null -and $StorAccAccessTier -eq "Hot") -or $b.ICloudBlob.AccessTier -eq "Hot") { $b.Length } else { 0 }
                CoolTierFileSize   = if(($b.ICloudBlob.AccessTier -eq $null -and $StorAccAccessTier -eq "Cool") -or $b.ICloudBlob.AccessTier -eq "Cool") { $b.Length } else { 0 }
                OtherTierFileSize  = if($b.ICloudBlob.AccessTier -ne $null -and $b.AccessTier -ne "Cool" -and $b.AccessTier -ne "Hot") { $b.Length } else { 0 }
            }

            $blobanalysis_hashtable.Add($fileext, $f) 
        }
        else #increment 
        {
            $blobanalysis_hashtable[$fileext].TotalFileCount     += 1
            $blobanalysis_hashtable[$fileext].TotalSize          += $b.Length

            $blobanalysis_hashtable[$fileext].LT1KBFileCount     += if($b.Length -lt 1024) { 1 } else { 0 }
            $blobanalysis_hashtable[$fileext].LT10KBFileCount    += if($b.Length -lt 10240  -and $b.Length -ge 1024)  { 1 } else { 0 }
            $blobanalysis_hashtable[$fileext].LT100KBFileCount   += if($b.Length -lt 102400 -and $b.Length -ge 10240) { 1 } else { 0 }
            $blobanalysis_hashtable[$fileext].LT1MBFileCount     += if($b.Length -lt 1024*1024 -and $b.Length -ge 102400) { 1 } else { 0 }
            $blobanalysis_hashtable[$fileext].LT10MBFileCount    += if($b.Length -lt 1024*1024*10 -and $b.Length -ge 1024*1024) { 1 } else { 0 }
            $blobanalysis_hashtable[$fileext].GE10MBFileCount    += if($b.Length -ge 1024*1024*10) { 1 } else { 0 }
            $blobanalysis_hashtable[$fileext].HotTierFileCount   += if(($b.ICloudBlob.AccessTier -eq $null -and $StorAccAccessTier -eq "Hot") -or $b.ICloudBlob.AccessTier -eq "Hot") { 1 } else { 0 }
            $blobanalysis_hashtable[$fileext].CoolTierFileCount  += if(($b.ICloudBlob.AccessTier -eq $null -and $StorAccAccessTier -eq "Cool") -or $b.ICloudBlob.AccessTier -eq "Cool") { 1 } else { 0 }
            $blobanalysis_hashtable[$fileext].OtherTierFileCount += if($b.ICloudBlob.AccessTier -ne $null -and $b.AccessTier -ne "Cool" -and $b.AccessTier -ne "Hot") { 1 } else { 0 }

            $blobanalysis_hashtable[$fileext].LT1KBFileSize      += if($b.Length -lt 1024) { $b.Length } else { 0 }
            $blobanalysis_hashtable[$fileext].LT10KBFileSize     += if($b.Length -lt 10240  -and $b.Length -ge 1024)  { $b.Length } else { 0 }
            $blobanalysis_hashtable[$fileext].LT100KBFileSize    += if($b.Length -lt 102400 -and $b.Length -ge 10240) { $b.Length } else { 0 }
            $blobanalysis_hashtable[$fileext].LT1MBFileSize      += if($b.Length -lt 1024*1024 -and $b.Length -ge 102400) { $b.Length } else { 0 }
            $blobanalysis_hashtable[$fileext].LT10MBFileSize     += if($b.Length -lt 1024*1024*10 -and $b.Length -ge 1024*1024) { $b.Length } else { 0 }
            $blobanalysis_hashtable[$fileext].GE10MBFileSize     += if($b.Length -ge 1024*1024*10) { $b.Length } else { 0 }
            $blobanalysis_hashtable[$fileext].HotTierFileSize    += if(($b.ICloudBlob.AccessTier -eq $null -and $StorAccAccessTier -eq "Hot") -or $b.ICloudBlob.AccessTier -eq "Hot") { $b.Length } else { 0 }
            $blobanalysis_hashtable[$fileext].CoolTierFileSize   += if(($b.ICloudBlob.AccessTier -eq $null -and $StorAccAccessTier -eq "Cool") -or $b.ICloudBlob.AccessTier -eq "Cool")  { $b.Length } else { 0 }
            $blobanalysis_hashtable[$fileext].OtherTierFileSize  += if($b.ICloudBlob.AccessTier -ne $null -and $b.AccessTier -ne "Cool" -and $b.AccessTier -ne "Hot") { $b.Length } else { 0 }
        }
    }

    $outputarray = @()
    foreach($k in $blobanalysis_hashtable.Keys) {
      
      $outputarray += $blobanalysis_hashtable[$k]  
    }
    $outputarray | Export-CSV $filepath -NoTypeInformation -Append
}


function Run-MainScript {
    #Get a list of SUBSCRIPTIONS
    $SUBSCRIPTION_LIST = $null
    $SUBSCRIPTION_LIST = (EnumerateSubscriptions)
    $SUBSCRIPTION_LIST | Export-CSV subscriptionlist.txt -NoTypeInformation
    Write-Verbose "Subscription list written to ./subscriptionlist.txt"

    #Get a list of STORAGE ACCOUNTs
    $step = 1
    if($SUBSCRIPTION_LIST -is [array]) {$subcount = $SUBSCRIPTION_LIST.Count}
    else {$subcount = 1}
    foreach($sub in $SUBSCRIPTION_LIST)
    {
        $STORAGEACCOUNTLIST = $null
        $STORAGEACCOUNTLIST = (EnumerateStorageAccounts -subscription $sub -percentCompleted ($step/$subcount*100))
        $step += 1
        $STORAGEACCOUNTLIST | Export-CSV storageAccountList.txt -NoTypeInformation -Append
        Write-Verbose ("Storage account list for " + $sub.SubscriptionId + " written to ./storageAccountList.txt")
    }

    #Get a list of STORAGE CONTAINERS
    $step = 1
    if($STORAGEACCOUNTLIST -is [array]) {$subcount = $STORAGEACCOUNTLIST.Count}
    else {$subcount = 1}
    $containerList = $null
    $sacontext = $null
    foreach( $acc in $STORAGEACCOUNTLIST )
    {
        $containerList += (EnumerateContainers -storageAccount $acc -percentCompleted ($step/$subcount*100))
        $step += 1
        if( $containerList -eq $null ) { continue; }
        $containerList | Export-CSV storageContainerList.txt -NoTypeInformation -Append
        Write-Verbose ("Storage container list for " + $acc.StorageAccountName + " written to ./storageContainerList.txt")
    }

    #Enumerate blobs per container
    foreach($c in $containerList)
    {
        Write-Verbose -Message ("Enumerating container: " + $c.BlobEndpoint + $c.Name )

        $bloblist = Get-AzStorageBlob -Context $c.Context -Container $c.Name
        ParseBlobList -container $c -blobs $bloblist -filepath "blobanalysis.txt" -StorAccAccessTier $acc.AccessTier
    }
}


Run-MainScript