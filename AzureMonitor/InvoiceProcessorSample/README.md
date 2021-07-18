## App insights
Right click on Project/Manage User Secrets.
```json
{
  "APPINSIGHTS_INSTRUMENTATIONKEY": null,
  "AzureWebJobsStorage": "AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://127.0.0.1:20000/devstoreaccount1;TableEndpoint=http://127.0.0.1:20002/devstoreaccount1;QueueEndpoint=http://127.0.0.1:20001/devstoreaccount1;",
  "AzureStorageConnectionString": "AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://127.0.0.1:20000/devstoreaccount1;TableEndpoint=http://127.0.0.1:20002/devstoreaccount1;QueueEndpoint=http://127.0.0.1:20001/devstoreaccount1;",
  "CosmosDBConnection": "AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw=="
}
```

Change Storage Emulator ports in C:\Program Files (x86)\Microsoft SDKs\Azure\Storage Emulator\AzureStorageEmulator.exe.config
```xml
    <services>
      <service name="Blob" url="http://127.0.0.1:20000/"/>
      <service name="Queue" url="http://127.0.0.1:20001/"/>
      <service name="Table" url="http://127.0.0.1:20002/"/>
    </services>
```