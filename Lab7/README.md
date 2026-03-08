# CST8921 – Cloud Industry Trends

## Lab 7 Report – Serverless Computing

**Completed by: Olga Durham** \
**St#: 040687883**

---

### Findings and Analysis

During the implementation of the serverless architecture, several limitations related to the CloudLabs training environment were encountered. The lab instructions required the use of Azure Event Hubs to ingest telemetry data and an Azure Table Storage trigger in Logic Apps to monitor the `TurbineMetrics` table. However, these components were partially restricted by Azure policies applied to the CloudLabs subscription.

The creation of the Event Hub namespace was blocked by an Azure Policy restriction. As a result, the full telemetry ingestion pipeline described in the lab instructions could not be implemented exactly as specified. Despite this limitation, the Azure Function application was successfully developed, built, and deployed to the cloud environment. The function code implements the required rule engine logic to evaluate wind turbine telemetry and determine turbine health status based on wind speed and generated power thresholds.

Another limitation occurred when configuring the Logic App workflow. The expected Azure Table Storage trigger described in the lab instructions was not available in the Logic App designer within the CloudLabs tenant. Because of this restriction, the workflow was implemented using an alternative trigger approach while maintaining the intended automation logic. The Logic App still demonstrates the automated monitoring pattern required by the lab by evaluating turbine status values and triggering an alert workflow.

Despite the tenant restrictions, the overall architecture of the solution was successfully implemented. The core components—including the Azure Function rule engine, Azure Table Storage for telemetry persistence, and the Logic App automation workflow—were configured and deployed. The system demonstrates how serverless cloud services can be used to process telemetry data, store structured metrics, and automate operational alerts.

These limitations highlight an important practical consideration in cloud development: environment policies and subscription restrictions can affect available services. Developers must sometimes adapt their implementation while preserving the architectural intent of the system.

---




