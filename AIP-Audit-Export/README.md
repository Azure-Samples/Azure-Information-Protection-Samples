---
page_type: sample
languages:
- PowerShell
products:
- m365
- office-365
description: "Export AIP audit events to Azure log analytics with PowerShell cript sample and Azure Workbook sample"
urlFragment: AIP-Audit-Export
---

# AIP Audit Export

## PowerShell Script


Before you use the AIP Audit Export review the following 
https://docs.microsoft.com/en-us/microsoft-365/compliance/audit-log-search-script?view=o365-worldwide#before-you-run-the-script to understand about permission and access requirement.

To learn about ExchangeOnlineManagement - review https://docs.microsoft.com/en-us/powershell/exchange/exchange-online-powershell-v2?view=exchange-ps

The solution uses Search-UnifiedAuditLog powershell will be used to extract the audit information as first Step , the script will  Connect to Exchange Online PowerShell and then execute retrieve audit records.

Information regarding permission and enablement.
1. Microsoft 365 audit log search must be turned on for the audit log connector to work. More information: Turn audit log search on or off
2.You must have access to the audit log.  More information: Search the audit log in the Security & Compliance Center
3. Your tenant must have a subscription that supports unified audit logging. More information: Security & Compliance Center availability for business and enterprise plans.

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

