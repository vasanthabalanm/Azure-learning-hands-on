# OpenSearch Learning Guide - Complete Hands-On Tutorial

## 📚 Table of Contents

1. [Introduction to OpenSearch](#introduction-to-opensearch)
2. [Learning Roadmap](#learning-roadmap)
3. [Prerequisites](#prerequisites)
4. [Module Overview](#module-overview)

---

## Introduction to OpenSearch

### What is OpenSearch?

**OpenSearch** is an open-source, distributed search and analytics engine derived from Elasticsearch 7.10.2. It's maintained by AWS and the community under the Apache 2.0 license.

### Key Features

| Feature | Description |
|---------|-------------|
| **Full-Text Search** | Powerful search capabilities with relevance scoring |
| **Analytics** | Real-time data analysis and aggregations |
| **Scalability** | Horizontal scaling across multiple nodes |
| **RESTful API** | Easy integration via HTTP/JSON |
| **OpenSearch Dashboards** | Visualization tool (like Kibana) |

### When to Use OpenSearch?

- **Search functionality**: Product search, document search, log search
- **Log analytics**: Centralized logging with ELK-like stack
- **Real-time analytics**: Dashboards, metrics, monitoring
- **Full-text search**: Content management, knowledge bases
- **Autocomplete/Suggestions**: Type-ahead search features

### OpenSearch vs Traditional Database Search

| Aspect | PostgreSQL (LIKE/ILIKE) | OpenSearch |
|--------|------------------------|------------|
| Search Speed | Slower on large datasets | Optimized for search |
| Relevance Scoring | No built-in scoring | TF-IDF, BM25 scoring |
| Fuzzy Search | Limited | Excellent |
| Autocomplete | Complex to implement | Native support |
| Scaling | Vertical (limited) | Horizontal (elastic) |

---

## Learning Roadmap

```
┌─────────────────────────────────────────────────────────────────┐
│                    OPENSEARCH LEARNING PATH                      │
├─────────────────────────────────────────────────────────────────┤
│                                                                   │
│  Module 1: Foundations (Day 1)                                   │
│  ├── Understanding search concepts                               │
│  ├── Docker setup & cluster basics                               │
│  └── OpenSearch Dashboards exploration                           │
│                                                                   │
│  Module 2: Core Concepts (Day 2)                                 │
│  ├── Indices, Documents, Mappings                                │
│  ├── CRUD operations via REST API                                │
│  └── Basic queries (match, term, range)                          │
│                                                                   │
│  Module 3: .NET Backend Integration (Day 3-4)                    │
│  ├── OpenSearch.Client NuGet setup                               │
│  ├── Index management & document operations                      │
│  ├── Search API implementation                                   │
│  └── Sync data from PostgreSQL                                   │
│                                                                   │
│  Module 4: Angular Frontend Integration (Day 5-6)                │
│  ├── Search UI components                                        │
│  ├── Autocomplete/Typeahead                                      │
│  ├── Faceted search & filters                                    │
│  └── Search results with highlighting                            │
│                                                                   │
│  Module 5: Advanced Topics (Day 7+)                              │
│  ├── Analyzers & Tokenizers                                      │
│  ├── Aggregations & Analytics                                    │
│  ├── Performance tuning                                          │
│  └── Production considerations                                   │
│                                                                   │
└─────────────────────────────────────────────────────────────────┘
```

---

## Prerequisites

### Required Tools

- [x] **Docker Desktop** - For running OpenSearch cluster
- [x] **.NET 8 SDK** - Backend development
- [x] **Node.js 18+** - Angular development
- [x] **Angular CLI** - Frontend scaffolding
- [x] **VS Code** - IDE with extensions

### Recommended VS Code Extensions

- REST Client (for testing APIs)
- Docker extension
- C# Dev Kit
- Angular Language Service

---

## Module Overview

### 📖 Learning Documents

| Module | Document | Description |
|--------|----------|-------------|
| 1 | [01-docker-setup.md](./docs/01-docker-setup.md) | OpenSearch Docker Compose setup |
| 2 | [02-core-concepts.md](./docs/02-core-concepts.md) | Indices, mappings, documents |
| 3 | [03-query-dsl.md](./docs/03-query-dsl.md) | Search query fundamentals |
| 4 | [04-dotnet-integration.md](./docs/04-dotnet-integration.md) | .NET backend setup |
| 5 | [05-angular-integration.md](./docs/05-angular-integration.md) | Angular frontend setup |
| 6 | [06-advanced-topics.md](./docs/06-advanced-topics.md) | Analyzers, aggregations |

### 🛠️ Project Structure

```
OpenSearch-setup/
├── README.md                    # This file
├── docker-compose.yml           # OpenSearch cluster setup
├── docs/                        # Learning documentation
│   ├── 01-docker-setup.md
│   ├── 02-core-concepts.md
│   ├── 03-query-dsl.md
│   ├── 04-dotnet-integration.md
│   ├── 05-angular-integration.md
│   └── 06-advanced-topics.md
├── backend/                     # .NET Web API project
│   └── OpenSearchDemo.Api/
├── frontend/                    # Angular application
│   └── opensearch-ui/
└── sample-data/                 # Test data files
    └── products.json
```

---

## Quick Start

### Step 1: Start OpenSearch with Docker

```bash
cd OpenSearch-setup
docker-compose up -d
```

### Step 2: Verify OpenSearch is Running

```bash
curl -XGET "http://localhost:9200" -ku admin:admin
```

### Step 3: Access OpenSearch Dashboards

Open [http://localhost:5601](http://localhost:5601) in your browser.

- **Username**: `admin`
- **Password**: `admin`

---

## Next Steps

👉 **Start with [Module 1: Docker Setup](./docs/01-docker-setup.md)** to set up your local OpenSearch cluster.

---

## Use Case: Product Search Demo

Throughout this guide, we'll build a **Product Search Application**:

| Feature | Technology |
|---------|------------|
| Product catalog indexing | OpenSearch + .NET |
| Full-text search | OpenSearch Query DSL |
| Autocomplete suggestions | OpenSearch + Angular |
| Faceted filtering | Aggregations |
| Search highlighting | OpenSearch highlighter |

### Sample Data Model

```json
{
  "id": "PROD001",
  "name": "Wireless Bluetooth Headphones",
  "description": "Premium noise-canceling wireless headphones with 30-hour battery life",
  "category": "Electronics",
  "subcategory": "Audio",
  "price": 149.99,
  "brand": "TechAudio",
  "tags": ["wireless", "bluetooth", "noise-canceling"],
  "rating": 4.5,
  "stock": 250,
  "createdAt": "2024-01-15T10:30:00Z"
}
```

---

## Resources

- [OpenSearch Documentation](https://opensearch.org/docs/latest/)
- [OpenSearch .NET Client](https://opensearch.org/docs/latest/clients/net/)
- [OpenSearch Query DSL](https://opensearch.org/docs/latest/query-dsl/)

---

**Ready to start? Let's begin with Module 1! 🚀**
