# Sample App
## Components

### InvoiceProcessor.Api
- ProcessInvoices
  - receive multiple invoices
- basic validation
- store -> blob -> trigger Azure function -> workflow
- Manage durable function
  - list workflows
  - (kill workflow)
  - (start workflow)
  
### Azure Function
- invoicesReceived
  - transformation (magyar elements to english elements)
    Invoice:
      InvoiceNumber
      ContactName
      TotalAmount
        LineItems
      
      LineItem:
      Name
        UnitAmount
        Quantity  		
  - validation
  - store (table, blob, sql)
- sendInvoices
  - schedule, ethernal orchestration
  - batch invoices
  - send to external API (sub orchestration)
    - (get token)
    - send invoices
    - get status loop

### External.Api (NAV)
- postInvoices
- getInvoiceStatus


## Tasks
API Manager

InvoiceProcessor Sample

Tracing
  Basic tracing sample endpoint
    TelemetryCLient API
	Activity	https://tsuyoshiushio.medium.com/correlation-with-activity-with-application-insights-1-overview-753a48a645fb
    
  Add trace info to blob
  Restore trace info from blob metadata in Azure function
  
  View timeline view
  
  Application map
  
Debugging
  Attach remote
  Snapshot debugging
  
Logging
  Guidelines
  
  Basic logging sample endpoint
  
  LogAnalytics
  
Metrics
  Basics
  
  Built in metrics
    Blob
	Web app
	Azure function
	
  Custom metric
  
  Alerting
  
  Auto scale
    Scheduled scaling for web app
	
Azure Dashboard / Workbook

PWC Sample

  
    
  
 