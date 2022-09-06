---
page_type: sample
languages:
- PowerShell
products:
- m365
- office-365
description: "Export AIP audit events to Azure log analytics with PowerShell script sample and Azure Workbook sample"
urlFragment: AIP-Audit-Export
---

# AIP Audit Export
Azure Log Analytics is an interactive workspace that enables ingestion and storage of massive amounts of data, indexes the data, and allows complex querying through an interface or API using the Kusto Query Language. This AIP Audit Export folder includes the tools to ingest data into a Azure Log Analytics workspace with custom logs and view the data in a customizable dashboard.

The following PowerShell Script and Azure Workbook samples will describe how to:
-  [PowerShell Script](https://github.com/Azure-Samples/Azure-Information-Protection-Samples/edit/master/AIP-Audit-Export/README.md#powershell-script): Continuously export data from the unified audit log to Azure Log Analytics
-  [Azure Workbook](https://github.com/Azure-Samples/Azure-Information-Protection-Samples/edit/master/AIP-Audit-Export/README.md#aip-information-protection-analytics-workbook): Set up a customizable dashboard with charting and custom queries for audit logs

For more information about auditing solutions with Microsoft Purview, review the [Microsoft tech community blog](https://techcommunity.microsoft.com/t5/security-compliance-and-identity/admin-guide-to-auditing-and-reporting-for-the-aip-unified/ba-p/3610727) for an admin guide to auditing and reporting for the AIP Unified Labeling client.

## PowerShell Script to Export Audit Data from Unified Audit Log to Azure Log Analytics Workspace
Microsoft Purview provides PowerShell commands to export data from the unified audit log. To continuously export data from the unified audit log to Azure Log Analytics, this sample PowerShell script will help you ingest the audit data into a custom table of your choice. The fields of this custom Log Analytics table are aligned with the fields in the unified audit log and are similar to the InformationProtectionLogs_CL table used with AIP analytics. 

**NOTE:** The script simplifies the export of AIP data in an easy-to-consume table structure. However, the script has limits. Microsoft guidance is to use the Office 365 Management API for scale and performance when millions of records need to be exported.

### Run the PowerShell Script
Download, save and run the [AIP Audit Export Powershell script](https://github.com/Azure-Samples/Azure-Information-Protection-Samples/blob/71a6a805e66c10d8553c48cc92e4688cf50ecf48/AIP-Audit-Export/Export-AIPAuditLogOperations.ps1).

The script uses the following cmdlets:
- `Search-UnifiedAuditLog` to extract audit information from the unified audit log.
- `Connect-ExchangeOnline` to authenticate.

### Troubleshooting
Find documentation about PowerShell cmdlets used in the script:
- Prerequisites and permissions: Audit logging is enabled by default for all organizations. If `UnifiedAuditLogIngestionEnabled` is false, review the guide for [using PowerShell to search the unified audit log](https://docs.microsoft.com/en-us/microsoft-365/compliance/audit-log-search-script?view=o365-worldwide#before-you-run-the-script).
- Exchange Online PowerShell: This script uses Exchange Online PowerShell to authenticate. For more information about the `Connect-ExchangeOnline` cmdlet, review the [Exchange Online PowerShell documentation](https://docs.microsoft.com/en-us/powershell/exchange/exchange-online-powershell-v2?view=exchange-ps).


## AIP Information Protection Analytics Workbook

### Getting Started 

1. To get started with the Azure Information Protection Analytics workbook, navigate to a log analytics workspace, select the workbooks tab, and create a new workbook.
2. Select the Advanced Editor icon within the new workbook.  
3. Go to the [AIPAnalyticsWorkbook.json](https://github.com/Azure-Samples/Azure-Information-Protection-Samples/blob/2c32c959bf354c00757dadf74eddabde829edca3/AIP-Audit-Export/AIPAnalyticsWorkbook.json) file within GitHub and copy the contents. 
4. Paste the copied contents into the advanced editor text book, removing any existing code that might already be there.  
5. Apply the changes and save the workbook. If the logs are properly set up, visualizations should start to populate within the workbook. 

![image](https://user-images.githubusercontent.com/25543918/186781810-a91ac5f3-afff-4f0e-965d-7fabf82c40c0.png)

### Filter 

Adjust the time range that is being displayed within the workbook by clicking on the **TimeRange** filter and selecting the appropriate time range from the dropdown.  

![image](https://user-images.githubusercontent.com/25543918/186781864-499e3be4-f6b0-46f2-a07b-2c4eb1daf678.png)

### Visualizations  

The KPI cards at the top display the label and protect activity as well as user and device usage that has happened in the specified time range. It also display the percent of change from the beginning to the end of the time range.

![image](https://user-images.githubusercontent.com/25543918/186781877-3f33ed39-a231-4de0-b477-649106e6d28a.png)

A specific breakdown of the time that labeling and protection activity occurred can be found within the **Label and Protect Activity** time chart.  

![image](https://user-images.githubusercontent.com/25543918/186782443-c38bd52b-d48f-4b03-ac83-2dcfc1bdad25.png)

In addition, a specific breakdown of when users and devices were active can be found within the **Users and Devices** time chart.  

![image](https://user-images.githubusercontent.com/25543918/186781915-f8619fc8-f179-49a3-aa73-b11a895b1b7c.png)

A list of labels used by the organization along with the number of times that activity has occurred with a specific label can be found within the **Labels** pie chart.

![image](https://user-images.githubusercontent.com/25543918/186781932-29c31ab6-e626-45cc-a43e-4fb55aba1055.png)

The applications that have been opened using labels can be found using the **Labels by Application** pie chart.  

![image](https://user-images.githubusercontent.com/25543918/186781947-b7825ea8-0cec-4da0-b3be-bcd9ad9d1eb5.png)

### Customization 

Any of the visualizations within the workbook can be edited or deleted through the edit button. New queries and visualizations can also be added to fit the needs of the organization. Additional details on how to edit workbooks can be found within the documentation: [Azure Workbooks overview - Azure Monitor | Microsoft Docs](https://docs.microsoft.com/en-us/azure/azure-monitor/visualize/workbooks-overview)

![image](https://user-images.githubusercontent.com/25543918/186781963-ce740f29-6740-4af8-89b7-2e767f9ba74a.png)

