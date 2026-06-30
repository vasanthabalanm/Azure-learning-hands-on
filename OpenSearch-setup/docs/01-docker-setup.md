# Module 1: Docker Setup for OpenSearch

## 🎯 Learning Objectives

By the end of this module, you will:

- ✅ Understand OpenSearch cluster architecture
- ✅ Run OpenSearch and Dashboards using Docker Compose
- ✅ Verify cluster health and basic connectivity
- ✅ Navigate OpenSearch Dashboards

---

## 1.1 OpenSearch Architecture Overview

### Single Node vs Multi-Node Cluster

```
┌─────────────────────────────────────────────────────────────────┐
│                    SINGLE NODE (Development)                     │
├─────────────────────────────────────────────────────────────────┤
│                                                                   │
│   ┌─────────────────────────────────────────────────────────┐   │
│   │              OpenSearch Node                             │   │
│   │  ┌──────────┐  ┌──────────┐  ┌──────────┐               │   │
│   │  │  Index   │  │  Index   │  │  Index   │               │   │
│   │  │ products │  │  users   │  │   logs   │               │   │
│   │  └──────────┘  └──────────┘  └──────────┘               │   │
│   │                                                          │   │
│   │  Port 9200 (REST API)                                    │   │
│   │  Port 9600 (Performance Analyzer)                        │   │
│   └─────────────────────────────────────────────────────────┘   │
│                                                                   │
│   ┌─────────────────────────────────────────────────────────┐   │
│   │           OpenSearch Dashboards                          │   │
│   │           Port 5601 (Web UI)                             │   │
│   └─────────────────────────────────────────────────────────┘   │
│                                                                   │
└─────────────────────────────────────────────────────────────────┘
```

### Key Ports

| Port | Service | Purpose |
|------|---------|---------|
| 9200 | OpenSearch REST API | Query/index documents |
| 9600 | Performance Analyzer | Metrics & monitoring |
| 5601 | OpenSearch Dashboards | Web UI |

---

## 1.2 Docker Compose Configuration

### Understanding the Configuration

Create the following `docker-compose.yml` file:

```yaml
# OpenSearch Development Setup
# Single-node cluster for learning and development

version: '3.8'

services:
  # ─────────────────────────────────────────────────────────────────
  # OpenSearch Node - Search & Analytics Engine
  # ─────────────────────────────────────────────────────────────────
  opensearch:
    image: opensearchproject/opensearch:2.11.1
    container_name: opensearch-node
    environment:
      # Cluster configuration
      - cluster.name=opensearch-demo
      - node.name=opensearch-node
      - discovery.type=single-node
      
      # Disable security for development (NOT for production!)
      - plugins.security.disabled=true
      
      # Memory settings
      - "OPENSEARCH_JAVA_OPTS=-Xms512m -Xmx512m"
      
      # Bootstrap checks
      - bootstrap.memory_lock=true
    ulimits:
      memlock:
        soft: -1
        hard: -1
      nofile:
        soft: 65536
        hard: 65536
    volumes:
      - opensearch-data:/usr/share/opensearch/data
    ports:
      - "9200:9200"    # REST API
      - "9600:9600"    # Performance Analyzer
    networks:
      - opensearch-net
    healthcheck:
      test: ["CMD-SHELL", "curl -s http://localhost:9200 || exit 1"]
      interval: 30s
      timeout: 10s
      retries: 5

  # ─────────────────────────────────────────────────────────────────
  # OpenSearch Dashboards - Visualization UI
  # ─────────────────────────────────────────────────────────────────
  dashboards:
    image: opensearchproject/opensearch-dashboards:2.11.1
    container_name: opensearch-dashboards
    environment:
      # Connect to OpenSearch
      - OPENSEARCH_HOSTS=["http://opensearch:9200"]
      
      # Disable security for development
      - DISABLE_SECURITY_DASHBOARDS_PLUGIN=true
    ports:
      - "5601:5601"
    networks:
      - opensearch-net
    depends_on:
      opensearch:
        condition: service_healthy

networks:
  opensearch-net:
    driver: bridge

volumes:
  opensearch-data:
    driver: local
```

### Configuration Explained

| Setting | Purpose |
|---------|---------|
| `discovery.type=single-node` | Disables cluster formation for dev |
| `plugins.security.disabled=true` | No authentication (dev only!) |
| `OPENSEARCH_JAVA_OPTS` | JVM heap size (512MB for dev) |
| `bootstrap.memory_lock=true` | Prevents swapping for performance |

---

## 1.3 Hands-On: Starting the Cluster

### Step 1: Create Project Structure

Open your terminal and run:

```powershell
# Navigate to OpenSearch-setup folder
cd d:\personal\Azure-leaaning\Azure-learning-hands-on\OpenSearch-setup

# Create docs folder
mkdir docs

# The docker-compose.yml should already be created (next step)
```

### Step 2: Start the Cluster

```powershell
# Start OpenSearch in detached mode
docker-compose up -d

# Watch the logs
docker-compose logs -f
```

### Step 3: Verify OpenSearch is Running

Wait 30-60 seconds, then run:

```powershell
# Check cluster health
curl http://localhost:9200

# Expected response:
# {
#   "name" : "opensearch-node",
#   "cluster_name" : "opensearch-demo",
#   "cluster_uuid" : "...",
#   "version" : {
#     "distribution" : "opensearch",
#     "number" : "2.11.1",
#     ...
#   },
#   "tagline" : "The OpenSearch Project: https://opensearch.org/"
# }
```

### Step 4: Check Cluster Health

```powershell
# Cluster health endpoint
curl http://localhost:9200/_cluster/health?pretty

# Expected: "status": "green" (or "yellow" for single node)
```

---

## 1.4 Exploring OpenSearch Dashboards

### Accessing the UI

1. Open your browser
2. Navigate to: [http://localhost:5601](http://localhost:5601)
3. Wait for Dashboards to initialize

### Dashboard Features Overview

| Feature | Location | Purpose |
|---------|----------|---------|
| **Dev Tools** | ☰ → Management → Dev Tools | Execute queries |
| **Index Management** | ☰ → Management → Index Management | View/manage indices |
| **Discover** | ☰ → OpenSearch Dashboards → Discover | Explore data |
| **Visualize** | ☰ → OpenSearch Dashboards → Visualize | Create charts |

### Try Dev Tools

Navigate to **Dev Tools** and run:

```json
# Check cluster health
GET _cluster/health

# List all indices
GET _cat/indices?v

# Get node info
GET _nodes/stats
```

---

## 1.5 Understanding Index Basics

### What is an Index?

An **index** in OpenSearch is like a "database" in SQL terms:

| SQL Concept | OpenSearch Equivalent |
|-------------|----------------------|
| Database | Cluster |
| Table | Index |
| Row | Document |
| Column | Field |
| Schema | Mapping |

### Creating Your First Index

In Dev Tools, run:

```json
# Create a simple index
PUT /my-first-index

# Verify it was created
GET _cat/indices?v
```

### Adding a Document

```json
# Add a document to the index
POST /my-first-index/_doc/1
{
  "title": "Learning OpenSearch",
  "author": "Student",
  "date": "2024-01-15",
  "content": "This is my first document in OpenSearch!"
}

# Retrieve the document
GET /my-first-index/_doc/1
```

### Search the Document

```json
# Simple search
GET /my-first-index/_search
{
  "query": {
    "match": {
      "content": "first document"
    }
  }
}
```

---

## 1.6 Troubleshooting

### Common Issues

| Issue | Cause | Solution |
|-------|-------|----------|
| Container won't start | Low memory | Reduce `OPENSEARCH_JAVA_OPTS` to `-Xms256m -Xmx256m` |
| Port 9200 in use | Another service | Change port in docker-compose |
| Dashboards shows error | OpenSearch not ready | Wait longer, check `docker-compose logs opensearch` |
| "Connection refused" | Container not running | Run `docker-compose up -d` |

### Useful Commands

```powershell
# Check container status
docker-compose ps

# View logs
docker-compose logs opensearch
docker-compose logs dashboards

# Restart containers
docker-compose restart

# Stop and remove containers
docker-compose down

# Stop and remove everything (including data)
docker-compose down -v
```

### Windows-Specific: WSL2 Memory

If Docker uses too much memory, create `.wslconfig`:

```ini
# C:\Users\<YourUser>\.wslconfig
[wsl2]
memory=4GB
processors=2
```

Then restart WSL: `wsl --shutdown`

---

## 1.7 Checkpoint Questions

Before moving to Module 2, verify you can answer:

1. ✅ What port does the OpenSearch REST API use?
2. ✅ How do you check cluster health?
3. ✅ What is the Dev Tools location in Dashboards?
4. ✅ How do you create an index?
5. ✅ How do you add a document to an index?

---

## 1.8 Exercises

### Exercise 1: Create a Books Index

```json
# Create an index called "books"
PUT /books

# Add 3 book documents with fields:
# - title, author, year, genre, rating

# Example:
POST /books/_doc/1
{
  "title": "The Great Gatsby",
  "author": "F. Scott Fitzgerald",
  "year": 1925,
  "genre": "Fiction",
  "rating": 4.5
}

# TODO: Add 2 more books yourself!
```

### Exercise 2: Search Books

```json
# Search for books by a specific author
GET /books/_search
{
  "query": {
    "match": {
      "author": "your search term"
    }
  }
}
```

---

## Next Steps

✅ **Module 1 Complete!**

👉 Continue to [Module 2: Core Concepts](./02-core-concepts.md) to learn about mappings, data types, and document operations.
