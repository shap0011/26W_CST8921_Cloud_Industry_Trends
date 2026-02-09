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

#### Part 1

##### Step 1: Upload Data to Azure Data Lake

1. Sign in to the Azure Portal
2. Navigate to your Storage Account

*Figure 1: Storage Account Created*
![Storage Account created](./screenshots/1-storage-account-overview.png)


3. Open Containers
4. Create a container named: `raw`
5. Upload the provided sample dataset in weekly lab 4 files into
    - `raw/customers/customers.parquet`
    - `raw/orders/orders.parquet`
    - `raw/order_events/order_events.parquet`

*Figure 2: File customers.parquet added*
![File customers.parquet added](./screenshots/2-file-customers-added.png)

*Figure 3: File customers.parquet added*
![File orders.parquet added](./screenshots/3-file-orders-added.png)

*Figure 4: File customers.parquet added*
![File order_events.parquet added](./screenshots/4-file-order_events-added.png)

##### Step 2: Explore Data using Serverless SQL

1. Open Azure Synapse Studio

*Figure 5: Synapse workspace created*
![Synapse workspace created](./screenshots/5-synapse-workspace-created.png)

2. Navigate to Develop → SQL script
3. Run the following query to explore Parquet files directly from the Data Lake:

```
SELECT TOP 100 *
FROM OPENROWSET(
    BULK 'https://cst8921lab4olga.dfs.core.windows.net/raw/*/*.parquet',
    FORMAT = 'PARQUET'
) AS rows;

```

4. Observe: Column names, Data types, Sample records

*Figure 6: Explore Parquet files directly from the Data Lake*
![Explore Parquet files directly from the Data Lake](./screenshots/6-explore-parquet-files-directly-from-the-data-lake.png)

*Figure 7: Explore Customers Parquet file directly from the Data Lake*
![Explore Customers Parquet files directly from the Data Lake](./screenshots/7-explore-customers-parquet-file.png)

*Figure 8: Explore Orders Parquet file directly from the Data Lake*
![Explore Orders Parquet file directly from the Data Lake](./screenshots/8-explore-orders-parquet-file.png)

*Figure 9: Explore Order Events Parquet file directly from the Data Lake*
![Explore Order Events Parquet file directly from the Data Lake](./screenshots/9-explore-order_events-parquet.png)

5. Try adding a filter: `WHERE Year > 2022`

```
SELECT TOP 10 *
FROM OPENROWSET(
    BULK 'https://cst8921lab4olga.dfs.core.windows.net/raw/customers/*.parquet',
    FORMAT = 'PARQUET'
) AS cgr
WHERE YEAR(signup_date) > 2022;

```

*Figure 10: Explore Customers Parquet file directly from the Data Lake where year > 2022*
![Explore Customers Parquet file directly from the Data Lake where year > 2022](./screenshots/10-explore-customers-parquet-file-year-gr-2022.png)

```
SELECT TOP 10 *,
       DATEADD(
           SECOND,
           CAST(order_date / 1000000000 AS BIGINT),
           '1970-01-01'
       ) AS order_datetime
FROM OPENROWSET(
    BULK 'https://cst8921lab4olga.dfs.core.windows.net/raw/orders/*.parquet',
    FORMAT = 'PARQUET'
) AS ogr2022
WHERE YEAR(
    DATEADD(
        SECOND,
        CAST(order_date / 1000000000 AS BIGINT),
        '1970-01-01'
    )
) > 2022;

```

*Figure 11: Explore Orders Parquet file directly from the Data Lake where year > 2022*
![Explore Orders Parquet file directly from the Data Lake where year > 2022](./screenshots/12-explore-order-events-parquet-file-year-gr-2022.png)

```
SELECT TOP 10 *,
       DATEADD(
           SECOND,
           CAST(event_time / 1000000000 AS BIGINT),
           '1970-01-01'
       ) AS event_datetime
FROM OPENROWSET(
    BULK 'https://cst8921lab4olga.dfs.core.windows.net/raw/order_events/*.parquet',
    FORMAT = 'PARQUET'
) AS e
WHERE YEAR(
    DATEADD(
        SECOND,
        CAST(event_time / 1000000000 AS BIGINT),
        '1970-01-01'
    )
) > 2022;

```

*Figure 12: Explore Order Events Parquet file directly from the Data Lake where year > 2022*
![Explore Order Events Parquet file directly from the Data Lake where year > 2022](./screenshots/12-explore-order-events-parquet-file-year-gr-2022.png)


##### Step 3: Explore Data using Spark Notebook

1. In Synapse Studio, go to Develop → Notebooks
2. Create a new Spark Notebook
3. Select Python as the language
4. Load the data:

```
df = spark.read.parquet( "abfss://raw@<storage-account>.dfs.core.windows.net/*.parquet”)

```

5. Inspect the data: 

```
df.printSchema() 
df.show(5)

```

---

#### PART 2: Data Transformation using Spark 

##### Step 4

1. Remove Duplicates : Remove duplicate records:

```
df_dedup = df.dropDuplicates()

```

2. Verify record count:

```
print(df.count())
print(df_dedup.count())

```

##### Step 5: Fix Data Types 

1. Convert timestamp columns to proper datetime format:

```
from pyspark.sql.functions import to_timestamp
df_clean = df_dedup.withColumn(  "event_time",  to_timestamp("event_time"))

```

2. Verify schema:

```
df_clean.printSchema()

```

##### Step 6: Create Derived Columns 

1.	Add Year and Month columns:

```
from pyspark.sql.functions import year, month
df_transformed = (
    df_clean
    .withColumn("Year", year("event_time"))
    .withColumn("Month", month("event_time"))
)

```

2. Preview results:

```
df_transformed.show(5)

```

##### Step 7: Write Transformed Data to Refined Zone

1. Create a new container named: refined
2. Write transformed data:

```
df_transformed.write.mode("overwrite").parquet( "abfss://refined@<storage-account>.dfs.core.windows.net/")

```

---

#### PART 3: Load & Analyze Data

##### Step 8: Create External Table using SQL

1. Open Synapse SQL Script
2. Create an external table over refined data:

```
CREATE EXTERNAL TABLE refined_events
WITH (
    LOCATION = 'refined/',
    DATA_SOURCE = MyDataLake,
    FILE_FORMAT = ParquetFormat
)
AS
SELECT *
FROM OPENROWSET(
    BULK 'refined/*.parquet',
    FORMAT = 'PARQUET'
) AS data;

```

3. Query the table:

```
SELECT Year, COUNT(*) AS total_events
FROM refined_events
GROUP BY Year
ORDER BY Year;

```

##### Step 9: Analyze & Visualize Data 

1. Notebook Visualization

```
df_transformed.groupBy("Year").count().show()

```

##### Step 10: Clean all the resources created during this lab

---

### Important Lab4 Notes README

E-commerce Synthetic Dataset (DP-203 Lab)

Folder: raw_parquet/

Files:
1) customers.parquet
   Columns: customer_id (string), country (string), signup_date (date)
   Rows: 50,000

2) orders.parquet
   Columns: order_id (string), customer_id (string), order_date (timestamp), order_amount (double), order_status (string)
   Rows: 202,000  (includes ~1% duplicates)

3) order_events.parquet
   Columns: event_id (string), order_id (string), event_time (timestamp), event_type (string)
   Rows: 422,100  (includes ~0.5% duplicates)

Notes:
- signup_date range: 2022-01-01 to 2025-12-31
- order_date range:  2023-01-01 to 2025-12-31
- event_time occurs within 0-7 days after the related order_date
- Designed for: ADLS Gen2 -> Spark transforms -> Synapse Serverless SQL exploration.

Suggested student exercises:
- Deduplicate orders/events
- Add Year/Month columns
- Partition refined outputs by Year/Month
- Analytics: revenue by Year; top countries by revenue; event counts by type

---

### Important Notes

For grading prepare a lab report with your findings and analysis and share that in an Assignments tab in Brightspace.

