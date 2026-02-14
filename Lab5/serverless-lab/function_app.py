import logging
import requests
import azure.functions as func

app = func.FunctionApp()

@app.event_grid_trigger(arg_name="event")
def ProcessBlobUpload(event: func.EventGridEvent):
    logging.info("Event Grid trigger received")

    data = event.get_json()
    blob_url = data["url"]

    logging.info(f"Blob URL: {blob_url}")

    # Download blob content
    response = requests.get(blob_url)
    blob_content = response.text

    logging.info("Blob content retrieved successfully")
    logging.info(f"Blob content (first 200 chars): {blob_content[:200]}")

