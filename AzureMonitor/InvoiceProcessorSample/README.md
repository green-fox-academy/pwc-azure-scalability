## App insights
Add "APPINSIGHTS_INSTRUMENTATIONKEY": "your key" to secrets.json.
Right click on Project/Manage User Secrets.

Change Storage Emulator ports in C:\Program Files (x86)\Microsoft SDKs\Azure\Storage Emulator\AzureStorageEmulator.exe.config
```xml
    <services>
      <service name="Blob" url="http://127.0.0.1:20000/"/>
      <service name="Queue" url="http://127.0.0.1:20001/"/>
      <service name="Table" url="http://127.0.0.1:20002/"/>
    </services>
```