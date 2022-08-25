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

## AIP Information Protection Analytics Workbook

### Getting Started 

1. To get started with the Azure Information Protection Analytics workbook, navigate to a log analytics workspace, select the workbooks tab, and create a new workbook.
2. Select the Advanced Editor icon within the new workbook.  
3. Go to the AIPAnalyticsWorkbook.json file within GitHub and copy the contents. 
4.Paste the copied contents into the advanced editor text book, removing any existing code that might already be there.  
5. Apply the changes and save the workbook. If the logs are properly set up, visualizations should start to populate within the workbook.   

### Filter 

Adjust the time range that is being displayed within the workbook by clicking on the **TimeRange** filter and selecting the appropriate time range from the dropdown.  

### Visualizations  

The KPI cards at the top display the label and protect activity as well as user and device usage that has happened in the specified time range. It also display the percent of change from the beginning to the end of the time range.

A specific breakdown of the time that labeling and protection activity occurred can be found within the **Label and Protect Activity** time chart.  

In addition, a specific breakdown of when users and devices were active can be found within the **Users and Devices** time chart.  

A list of labels used by the organization along with the number of times that activity has occurred with a specific label can be found within the **Labels** pie chart. 

The applications that have been opened using labels can be found using the **Labels by Application** pie chart.  

### Customization 

Any of the visualizations within the workbook can be edited or deleted through the edit button. New queries and visualizations can also be added to fit the needs of the organization. Additional details on how to edit workbooks can be found within the documentation: [Azure Workbooks overview - Azure Monitor | Microsoft Docs](https://docs.microsoft.com/en-us/azure/azure-monitor/visualize/workbooks-overview)
