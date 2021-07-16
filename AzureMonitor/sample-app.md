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