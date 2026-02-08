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