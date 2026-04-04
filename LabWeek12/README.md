# CST8921 – Cloud Industry Trends

## Lab 11 – Set Up Azure AI Search Index and Deploy Embedding & LLM Models

**Completed by: Olga Durham** \
**St#: 040687883**

---

### Objective:

By the end of this lab, learners will:

- Provision Azure AI Search and Azure OpenAI resources
- Deploy an embedding model and an LLM/chat model
- Create a vector-enabled search index
- Ingest and index documents
- Run vector and hybrid search queries
- Use an LLM to generate grounded answers from retrieved search results

Azure AI Search supports vector search, hybrid search, and integrated vectorization; Azure OpenAI supports embeddings and chat-completion models used in retrieval-augmented generation workflows.

---

### Architecture Overview

**Flow:**

1. Upload documents
2. Generate embeddings
3. Store chunks + vectors in Azure AI Search
4. Query the index
5. Retrieve top matching chunks
6. Send retrieved context to the chat model
7. Return grounded answer

This is the standard vector-search/RAG pattern described by Azure AI Search and Azure OpenAI documentation.

---

### Learning Outcomes:

At the end of the lab, students should be able to:
• Explain what embeddings are and why vector search is useful
• Differentiate keyword, vector, and hybrid search
• Deploy an embedding model and an LLM deployment
• Build a vector-enabled index schema
• Load and query searchable document chunks
• Use retrieved search content to ground an LLM answer

---

### Suggested Lab Scenario

Your team is building an internal HR policy assistant. Employees ask questions like:
“How many vacation days do I get?”
“What is the remote work policy?”
“How do I submit benefits claims?”
The assistant should search policy documents using Azure AI Search and use an LLM to generate a concise answer grounded in those documents.

---

### Lab Modules

**Module 1 — Provision Azure Resources**

Tasks

Create the following resources in Azure Portal:

1. Resource Group: rg-ai-search-lab
2. Azure AI Search service: srch-rag-lab
3. Pricing tier: choose a lab-appropriate tier
4. Azure OpenAI / Azure AI Foundry-backed OpenAI resource: aoai-rag-lab
5. Storage Account : stsearchlabfiles
6. Optional: Azure AI Foundry project for model management

**Module 2 — Deploy Embedding and LLM Models**

Objective

Deploy:

- one embedding model
- one chat/completion model

Recommended model roles

Use:

- an embedding deployment for document chunk vectors and query vectors
- a chat model deployment for grounded answer generation

Azure’s embeddings documentation describes generating vectors for downstream search scenarios, while chat-completion documentation covers conversational generation with models such as GPT-4o in Azure OpenAI.

Suggested deployment names

- Embedding deployment: text-embedding-3-small
- Chat deployment: gpt-4o

Note that actual model availability varies by region and subscription, so students should confirm supported models in their Azure environment before deployment. Azure’s model availability is region- and offering-dependent.

Student activity: In the portal:

1. Open Azure OpenAI / Foundry
2. Create model deployment for embeddings
3. Create model deployment for chat
4. Save:
   - Endpoint
   - API key
   - deployment names

Checkpoint:

Students should record:

- AZURE_OPENAI_ENDPOINT
- AZURE_OPENAI_API_KEY
- EMBEDDING_MODEL_DEPLOYMENT
- CHAT_MODEL_DEPLOYMENT

**Module 3 — Prepare Source Documents**

Objective

Create a small knowledge base of documents.

Sample files

Use 3–5 text or PDF files such as:

- vacation-policy.txt
- remote-work-policy.txt
- benefits-overview.txt

Student activity

Upload files to a container in Azure Storage:

Container name: `documents`

Why chunking matters

Azure notes that chunking is usually necessary because raw documents are often too large for efficient embedding workflows and token limits.

**Module 4 — Create the Search Index**

You can teach this in two ways:

Option A — Easier / Portal-based

Use the Import data / Import and vectorize data experience in Azure AI Search.

Azure documents show that the portal quickstart supports integrated vectorization, chunking, and vector index creation through the wizard.

Option B — Better for learning

Create the index manually so students understand the schema.

I recommend Option B for a class lab.

**Module 5 — Design the Vector Index Schema**

Core fields

Your index should include:

- id — key field
- title — searchable text
- content — searchable text chunk
- category — filterable field
- contentVector — vector field for embeddings
- sourceFile — original document name

Azure AI Search vector indexes are defined through index schemas containing normal fields plus vector fields and vector configuration.

Logical schema

```
{
  "name": "hr-policy-index",
  "fields": [
    { "name": "id", "type": "Edm.String", "key": true, "filterable": true },
    { "name": "title", "type": "Edm.String", "searchable": true },
    { "name": "content", "type": "Edm.String", "searchable": true },
    { "name": "category", "type": "Edm.String", "filterable": true, "facetable": true },
    { "name": "sourceFile", "type": "Edm.String", "filterable": true },
    {
      "name": "contentVector",
      "type": "Collection(Edm.Single)",
      "searchable": true,
      "dimensions": 1536,
      "vectorSearchProfile": "vector-profile"
    }
  ],
  "vectorSearch": {
    "algorithms": [
      {
        "name": "hnsw-config",
        "kind": "hnsw"
      }
    ],
    "profiles": [
      {
        "name": "vector-profile",
        "algorithm": "hnsw-config"
      }
    ]
  }
}
```

**Module 6 — Generate Embeddings and Load Documents**

Objective

Chunk documents, generate embeddings, and push them into the index

Approach

Students will:

1. Read document text
2. Split into chunks
3. Call embedding model
4. Upload documents with vectors to Azure AI Search

Azure supports both precomputed embeddings and integrated vectorization. This lab uses precomputed embeddings first because it helps students understand the moving parts.

Python packages

```
pip install openai azure-search-documents python-dotenv
```

Azure documentation also notes the newer OpenAI v1 API direction for Azure OpenAI.

.env example

```
AZURE_OPENAI_ENDPOINT=...
AZURE_OPENAI_API_KEY=...
EMBEDDING_MODEL_DEPLOYMENT=text-embedding-3-small
CHAT_MODEL_DEPLOYMENT=gpt-4o

AZURE_SEARCH_ENDPOINT=...
AZURE_SEARCH_KEY=...
AZURE_SEARCH_INDEX=hr-policy-index



import os
from openai import OpenAI
from azure.core.credentials import AzureKeyCredential
from azure.search.documents import SearchClient
from dotenv import load_dotenv

load_dotenv()

openai_client = OpenAI(
    api_key=os.getenv("AZURE_OPENAI_API_KEY"),
    base_url=f"{os.getenv('AZURE_OPENAI_ENDPOINT').rstrip('/')}/openai/v1/"
)

search_client = SearchClient(
    endpoint=os.getenv("AZURE_SEARCH_ENDPOINT"),
    index_name=os.getenv("AZURE_SEARCH_INDEX"),
    credential=AzureKeyCredential(os.getenv("AZURE_SEARCH_KEY"))
)

def chunk_text(text, chunk_size=800):
    chunks = []
    start = 0
    while start < len(text):
        chunks.append(text[start:start+chunk_size])
        start += chunk_size
    return chunks

def get_embedding(text):
    response = openai_client.embeddings.create(
        model=os.getenv("EMBEDDING_MODEL_DEPLOYMENT"),
        input=text
    )
    return response.data[0].embedding

```

---

### Upload step

For each chunk, upload:

- id
- title
- content
- category
- sourceFile
- contentVector

**Module 7 — Run Vector and Hybrid Search Queries**

Objective

Compare the different search approaches.

Azure AI Search recommends hybrid search in many cases because keyword and vector search can complement each other.

Experiments

Run three search types:

A. Keyword search

Search text only: “vacation carryover”

B. Vector search

Embed the user question and search against contentVector

C. Hybrid search

Use both keyword text and vector query together

**Module 8 — Add the LLM Layer**

Objective

Generate a grounded answer from retrieved chunks.

Flow

1. User asks question
2. Search index retrieves top 3–5 chunks
3. Construct prompt with context
4. Send prompt to chat model
5. Return answer with sources

Grounded prompt

You are an HR assistant.

Answer the question using only the context below.

If the answer is not in the context, say you do not know.

```
Context:
[chunk 1]
[chunk 2]
[chunk 3]
```

Question:

How many vacation days do employees receive after 5 years?

Python example

```
def ask_llm(question, context_chunks):
    context = "\n\n".join(context_chunks)
    response = openai_client.chat.completions.create(
        model=os.getenv("CHAT_MODEL_DEPLOYMENT"),
        messages=[
            {
                "role": "system",
                "content": "You answer only from supplied context and avoid making up facts."
            },
            {
                "role": "user",
                "content": f"Context:\n{context}\n\nQuestion: {question}"
            }
        ]
    )
    return response.choices[0].message.content
```
