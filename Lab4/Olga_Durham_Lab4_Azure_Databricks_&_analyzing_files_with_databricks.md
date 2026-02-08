# CST8921 – Cloud Industry Trends

## Lab 4 – Azure Databricks and analyzing files with databricks

**Completed by: Olga Durham**
**St#: 040687883**

---

### Introduction

Azure Databricks is an analytics platform powered by Apache Spark. Spark is a unified analytics engine capable of working with virtually every major database, data caching service, and data warehouse provider.\
In Databricks you have the option of working with Spark, Scala, and Python to manage, analyze, and visualize data. Notebooks in Databricks clusters provide the ability to programmatically interact with data from virtually any major data source.\
The goal is to be able to experiment and learn with little start time or overhead. Feel free to experiment with loading data to ADLS, managing data and folders in ADLS using Databricks, working with Databricks clusters and notebooks, and more. 

### Objective

In this lab, you will explore raw data stored in Azure Data Lake Storage, transform it using Apache Spark, and load the refined data into an analytics store for querying.

### Prerequisites

- An active Azure subscription or sandbox
- Azure Storage Account with Hierarchical Namespace enabled (ADLS Gen2)
- Azure Synapse Analytics workspace OR Azure Databricks workspace
- Basic knowledge of SQL and Python

---

### Lab Activity Overview

#### Step 1: Upload Data to Azure Data Lake

1. Sign in to the Azure Portal
2. Navigate to your Storage Account
3. Open Containers
4. Create a container named: `raw`
5. Upload the provided sample dataset in weekly lab 4 files into
    - `raw/customers/customers.parquet`
    - `raw/orders/orders.parquet`
    - `raw/order_events/order_events.parquet`

