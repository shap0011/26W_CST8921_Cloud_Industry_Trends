
# CST8921 – Cloud Industry Trends

## Lab 7 – Serverless Computing

**Completed by: Olga Durham** \
**St#: 040687883**

---

### Introduction

In this lab, students will gain hands-on experience in building and managing serverless framework. They will learn how to deploy first serverless microservice in cloud. Serverless computing is one of the most interesting and useful parts the cloud offers. It allows engineers to design and code their applications, and then execute them without worrying about the underlying server infrastructure. One of the most famous paradigms Serverless computing introduced is the FaaS (Function as a Service). That means you focus more on single tasks and functions, instead of thinking about the whole application structure. It's very powerful because all the resources needed to serve and maintain the functions are handled automatically by the providers.
If you want to build a serverless application, it could be difficult to see the benefits if you don't leverage a framework to create resources to perform tasks to let the serverless application run. The Serverless Framework is a solution to easily manage the process of packaging and deployment of serverless applications. It's cloud-agnostic, so you can leverage the framework by using the most popular public cloud providers like Amazon Web Services, Google Cloud Platform, and Microsoft Azure.
In this lab, you will understand the basic components of the Serverless Framework, you'll migrate Event Hubs captured data from Azure Blob Storage to Azure Synapse Analytics, specifically a dedicated SQL pool, using Azure Event Grid and Azure Functions.

---

### Objective

**Goal:** Ingest telemetry → score health in an Azure Function → store in Azure Table Storage → trigger logic App email alert when urgent

---

### Learning Outcomes: By the end of this lab, you will be able to

1. Deploy an event-driven ingestion stack using Azure CLI.
2. Process streaming telemetry with an Azure Function (rule engine).
3. Store structured telemetry in Azure Table Storage (low-cost NoSQL).
4. Automate alerts using a Logic App trigger + condition.
5. Monitor end-to-end flow and validate output.

---

### Architecture Overview (What You’re Building)

WindTurbineDataGenerator.exe → Azure Event Hub → Azure Function (Rule Engine) → Azure Table Storage (TurbineMetrics) → Logic App → Email Alert

---

### Health Logic

If WindSpeed > 15 AND GeneratedPower < 5 → Status = "URGENT" Else → Status = "HEALTHY"

---

### Prerequisites (Before You Start)

1. Azure account with permission to create resources
2. Azure CLI installed and logged in:
3. az login
4. az account show
5. A code editor (VS Code recommended)
6. WindTurbineDataGenerator.exe available (provided by instructor)
7. Function project available (provided by instructor): FunctionDWDumper (you will modify it)

To avoid conflicts and grading confusion:

1. Resource Group: `rg-SmartTurbine`
2. Region: `eastus`
3. Table Name: `TurbineMetrics`
4. Event Hub Namespace: `hubdatamigration-<yourinitials>-<2digits>`
5. Storage Account: `stsmartturb<yourinitials><2digits> (must be lowercase, 3–24 chars)`

Example for “RM12”:
Namespace: `hubdatamigration-rm-12`
Storage: `stsmartturbrm12`

#### Final Resource Names

| Resource            | Name                     |
| ------------------- | ------------------------ |
| Resource Group      | `rg-SmartTurbine`        |
| Region              | `eastus`                 |
| Storage Account     | `stsmartturbod12`        |
| Event Hub Namespace | `hubdatamigration-od-12` |
| Event Hub           | `turbine-telemetry`      |
| Function App        | `func-smartturbine-od12` |
| Logic App           | `la-smartturbine-od12`   |
| Table Name          | `TurbineMetrics`         |

*Fig 1 - az account show (cropped)*
![az account show](./screenshots/01-az-account-show.png)

Create resource group

```
az group create -l eastus -n rg-SmartTurbine
```

Verify the group exists

```
az group list --output table
```

Create Storage Account

```
az storage account create -n stsmartturbod12 -g rg-SmartTurbine -l eastus --sku Standard_LRS
```

Create the Event Hub namespace

```
az eventhubs namespace create -g rg-SmartTurbine -n hubdatamigration-od-12 -l eastus --sku Basic
```

---

### Lab Activity Overview

#### Phase 1 — Infrastructure Setup

1. Create Resource Group

```
az group create -l eastus -n rg-SmartTurbine
```

2. Create Storage Account (for Table Storage): Pick a globally unique name:

```
az storage account create \
  -n <YOUR_STORAGE_ACCOUNT_NAME> \
  -g rg-SmartTurbine \
  -l eastus \
  --sku Standard_LRS
```

3. Create Event Hub Namespace + Event Hub

```
az eventhubs namespace create \
  -g rg-SmartTurbine \
  -n <YOUR_EVENTHUB_NAMESPACE> \
  -l eastus \
  --sku Basic
```

Create the hub:

```
az eventhubs eventhub create \
  -g rg-SmartTurbine \
  --namespace-name <YOUR_EVENTHUB_NAMESPACE> \
  -n turbine-telemetry
```

4.	Create a Functions-compatible storage account is usually required; we will reuse your storage account 

Create Function App (Consumption Plan) 

```
az functionapp create \
  -g rg-SmartTurbine \
  -n func-smartturbine-<yourinitials><2digits> \
  --consumption-plan-location eastus \
  --runtime dotnet \
  --functions-version 4 \
  --storage-account <YOUR_STORAGE_ACCOUNT_NAME>
```

5.	Create the NoSQL Table: TurbineMetrics

Option A (Portal):
Storage Account → Storage browser → Tables → + Table
Name: TurbineMetrics
Option B (CLI):

Get storage key:

```
az storage account keys list \
  -g rg-SmartTurbine \
  -n <YOUR_STORAGE_ACCOUNT_NAME> \
  --query "[0].value" -o tsv
```

Create table:

```
az storage table create \
  --name TurbineMetrics \
  --account-name <YOUR_STORAGE_ACCOUNT_NAME> \
  --account-key <PASTE_KEY_HERE>
```

#### Phase 2 : Building the brain

1. Open the Function Project: Open FunctionDWDumper

a) What you must change: Right now it probably “dumps” messages. You will:
b) Parse telemetry fields from the incoming event
c) Compute Status based on thresholds
d) Write the record to Azure Table Storage via output binding

FunctionDWDumper function

```
using Azure.Data.Tables;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;


namespace FunctionDWDumper
{
    public class FunctionDWDumper
    {
        private readonly ILogger<FunctionDWDumper> _logger;

        public FunctionDWDumper(ILogger<FunctionDWDumper> logger)
        {
            _logger = logger;
        }

        [Function("FunctionDWDumper")]
        public async Task Run(
            [EventHubTrigger("turbine-telemetry", Connection = "EventHubConnection", IsBatched = false)] string eventData,
            FunctionContext context)
        {
            _logger.LogInformation($"Event received: {eventData}");

            // Parse the incoming JSON
            var telemetry = JsonSerializer.Deserialize<TelemetryPayload>(eventData,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            // Health scoring rule
            string status = (telemetry.WindSpeed > 15 && telemetry.GeneratedPower < 5)
                ? "URGENT"
                : "HEALTHY";

            // Connect to Table Storage and write the row
            string connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
            var tableClient = new TableClient(connectionString, "TurbineMetrics");
            await tableClient.CreateIfNotExistsAsync();

            var entity = new TableEntity(telemetry.DeviceId, telemetry.Timestamp)
            {
                { "WindSpeed", telemetry.WindSpeed },
                { "GeneratedPower", telemetry.GeneratedPower },
                { "TurbineSpeed", telemetry.TurbineSpeed },
                { "Status", status }
            };

            await tableClient.AddEntityAsync(entity);
            _logger.LogInformation($"Saved: DeviceId={telemetry.DeviceId}, Status={status}");
        }
    }
}

```

2. Implement the Health Scoring Logic

  Rule: If WindSpeed > 15 AND GeneratedPower < 5 → Status="URGENT" Else →  Status="HEALTHY"

3. Configure Table Storage Output Binding (Azure Functions)

You will output an entity with:

PartitionKey = `DeviceId`

RowKey = `Timestamp` (string; must be unique per device)

Fields: `WindSpeed`, `GeneratedPower`, `TurbineSpeed`, `Status`

A) Example Entity Model (C#)

Create a model like this (or adapt your existing one):

```
public class TurbineMetricEntity
{
    public string PartitionKey { get; set; }   // DeviceId
    public string RowKey { get; set; }         // Timestamp
    public double WindSpeed { get; set; }
    public double GeneratedPower { get; set; }
    public double TurbineSpeed { get; set; }
    public string Status { get; set; }
}
```

B) Example Function Output Binding (typical pattern)

Depending on your project template, your binding may be in:

function.json (older style), or

attributes in C# (common in .NET Functions), or

host.json + extension bundles.

If your project uses function.json, your output binding block will look like:

```
{
  "type": "table",
  "name": "outputTableEntity",
  "tableName": "TurbineMetrics",
  "connection": "AzureWebJobsStorage",
  "direction": "out"
}
```

And your function returns the entity object (or sets outputTableEntity).

Key point: Table Storage uses the storage connection in AzureWebJobsStorage (or another app setting you define).

4. Set the Function App Settings (connection + event hub)

You must configure Event Hub connection and (if needed) storage connection.

A) Event Hub connection string → App Setting

In Azure Portal:

Event Hub Namespace → Shared access policies

Use RootManageSharedAccessKey (lab-friendly)

Copy Connection string–primary key

In CLI (optional), set it:

```
az functionapp config appsettings set \
  -g rg-SmartTurbine \
  -n func-smartturbine-<yourinitials><2digits> \
  --settings "EventHubConnection=<PASTE_EVENTHUB_CONNECTION_STRING>"
```

B) Ensure storage setting exists

Azure Functions usually already has:

AzureWebJobsStorage (created during Function App provisioning)

Checkpoint: Function builds locally and contains:

scoring logic

table output binding

event hub trigger binding

#### Phase 3 — The Automation Layer (50 minutes)

1. Create a Logic App (Consumption)

Azure Portal:

Create Resource → Logic App (Consumption)

Resource Group: `rg-SmartTurbine`

Region: `eastus`

Name: `la-smartturbine-<yourinitials><2digits>`

2. Build the Workflow
Trigger

Use:

Azure Table Storage → When an entity is added (or similar)

Configure:

Storage account: `<YOUR_STORAGE_ACCOUNT_NAME>`

Table: `TurbineMetrics`

If the connector asks for auth: use Access Key or sign-in depending on portal options.

Condition

Add Condition:

Status is equal to URGENT

If true → Send Email

Use either:

Office 365 Outlook connector, or

Gmail connector (whichever is available for students)

Subject
ALERT: Turbine {PartitionKey} Failure Detected!

Body
Critical failure detected at {RowKey}. Power output is too low for current wind speeds.

If the trigger fields show different names, use:

{PartitionKey} = DeviceId

{RowKey} = Timestamp

#### Phase 4 — Live Simulation & Monitoring

1. Run the Data Generator

Run:

`WindTurbineDataGenerator.exe`

Configure it (if it asks):

`Event Hub namespace/hub`

`Connection string`

`Send interval (default is fine)`

2. Watch the Function Execute

Azure Portal → Function App → Functions → your function → Monitor

You should see runs occurring as the generator sends telemetry.

3. Validate Table Storage Writes

Use Storage browser or Azure Storage Explorer:

Storage Account → Tables → TurbineMetrics

You should see rows with:

PartitionKey = `device id`

RowKey = `timestamp`

Status = `HEALTHY/URGENT`

4. Confirm the Alert

When at least one URGENT row appears, your Logic App should send an email.

Check:

Inbox

Logic App → Run history (you should see a successful run)

---

### Important Notes

For grading prepare a lab report with your findings and analysis and share that in an Assignments tab in Brightspace.