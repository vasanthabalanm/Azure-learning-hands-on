# Module 2: OpenSearch Core Concepts

## 🎯 Learning Objectives

By the end of this module, you will:

- ✅ Understand indices, documents, and mappings
- ✅ Create indices with explicit mappings
- ✅ Perform CRUD operations on documents
- ✅ Understand data types and field properties

---

## 2.1 Core Terminology

### The Data Hierarchy

```
┌─────────────────────────────────────────────────────────────────┐
│                         CLUSTER                                  │
│   ┌─────────────────────────────────────────────────────────┐   │
│   │                        INDEX                             │   │
│   │   ┌─────────────┐  ┌─────────────┐  ┌─────────────┐     │   │
│   │   │  DOCUMENT   │  │  DOCUMENT   │  │  DOCUMENT   │     │   │
│   │   │ {id: 1}     │  │ {id: 2}     │  │ {id: 3}     │     │   │
│   │   │ field: val  │  │ field: val  │  │ field: val  │     │   │
│   │   └─────────────┘  └─────────────┘  └─────────────┘     │   │
│   └─────────────────────────────────────────────────────────┘   │
│                                                                   │
│   ┌─────────────────────────────────────────────────────────┐   │
│   │                     ANOTHER INDEX                        │   │
│   │   ┌─────────────┐  ┌─────────────┐                      │   │
│   │   │  DOCUMENT   │  │  DOCUMENT   │                      │   │
│   │   └─────────────┘  └─────────────┘                      │   │
│   └─────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────┘
```

### Terminology Reference

| Term | Definition | SQL Equivalent |
|------|------------|----------------|
| **Cluster** | Collection of nodes | Database server |
| **Index** | Collection of documents | Table |
| **Document** | JSON object with data | Row |
| **Field** | Key-value in document | Column |
| **Mapping** | Schema definition | CREATE TABLE |
| **Shard** | Partition of an index | Partition |
| **Replica** | Copy of a shard | Read replica |

---

## 2.2 Understanding Mappings

### What is a Mapping?

A **mapping** defines how documents and their fields are stored and indexed:

- Data types for each field
- Whether a field should be searchable
- How text should be analyzed

### Dynamic vs Explicit Mapping

| Type | Behavior | Use Case |
|------|----------|----------|
| **Dynamic** | Auto-detects field types | Quick prototyping |
| **Explicit** | You define all fields | Production systems |

### Data Types

| Type | Description | Example |
|------|-------------|---------|
| `text` | Full-text search (analyzed) | Product descriptions |
| `keyword` | Exact match (not analyzed) | IDs, categories, emails |
| `integer` | 32-bit integer | Age, quantity |
| `long` | 64-bit integer | Large IDs |
| `float` | Single-precision decimal | Ratings |
| `double` | Double-precision decimal | Prices |
| `boolean` | true/false | isActive |
| `date` | Date/datetime | createdAt |
| `object` | Nested JSON object | Address |
| `nested` | Array of objects (searchable) | Order items |

### Text vs Keyword

This is the **most important distinction** to understand:

```
┌─────────────────────────────────────────────────────────────────┐
│                        TEXT FIELD                                │
├─────────────────────────────────────────────────────────────────┤
│  Input: "The Quick Brown Fox"                                    │
│                                                                   │
│  Stored tokens: ["the", "quick", "brown", "fox"]                │
│                                                                   │
│  ✅ Searchable by: "quick", "brown", "Quick", "BROWN"           │
│  ❌ NOT for: exact match, sorting, aggregations                 │
└─────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│                      KEYWORD FIELD                               │
├─────────────────────────────────────────────────────────────────┤
│  Input: "The Quick Brown Fox"                                    │
│                                                                   │
│  Stored as-is: "The Quick Brown Fox"                            │
│                                                                   │
│  ✅ Exact match: "The Quick Brown Fox" only                     │
│  ✅ Good for: IDs, categories, sorting, aggregations            │
│  ❌ NOT searchable by partial terms                             │
└─────────────────────────────────────────────────────────────────┘
```

---

## 2.3 Hands-On: Creating the Products Index

### Step 1: Create Index with Explicit Mapping

In OpenSearch Dashboards Dev Tools (`http://localhost:5601`):

```json
PUT /products
{
  "settings": {
    "number_of_shards": 1,
    "number_of_replicas": 0
  },
  "mappings": {
    "properties": {
      "id": {
        "type": "keyword"
      },
      "name": {
        "type": "text",
        "fields": {
          "keyword": {
            "type": "keyword"
          }
        }
      },
      "description": {
        "type": "text"
      },
      "category": {
        "type": "keyword"
      },
      "subcategory": {
        "type": "keyword"
      },
      "price": {
        "type": "float"
      },
      "brand": {
        "type": "keyword"
      },
      "tags": {
        "type": "keyword"
      },
      "rating": {
        "type": "float"
      },
      "stock": {
        "type": "integer"
      },
      "isActive": {
        "type": "boolean"
      },
      "createdAt": {
        "type": "date"
      },
      "specifications": {
        "type": "object",
        "properties": {
          "weight": { "type": "float" },
          "dimensions": { "type": "keyword" },
          "color": { "type": "keyword" }
        }
      }
    }
  }
}
```

### Step 2: Verify the Mapping

```json
GET /products/_mapping
```

### Understanding Multi-Fields

Notice `name` has both `text` and `keyword`:

```json
"name": {
  "type": "text",           // For full-text search
  "fields": {
    "keyword": {
      "type": "keyword"     // For exact match/sorting
    }
  }
}
```

Usage:
- Search: `name` (full-text)
- Sort/Aggregate: `name.keyword` (exact)

---

## 2.4 Document Operations (CRUD)

### CREATE - Adding Documents

```json
# Create with auto-generated ID
POST /products/_doc
{
  "id": "PROD001",
  "name": "Wireless Bluetooth Headphones",
  "description": "Premium noise-canceling wireless headphones with 30-hour battery",
  "category": "Electronics",
  "subcategory": "Audio",
  "price": 149.99,
  "brand": "TechAudio",
  "tags": ["wireless", "bluetooth", "noise-canceling"],
  "rating": 4.5,
  "stock": 250,
  "isActive": true,
  "createdAt": "2024-01-15T10:30:00Z",
  "specifications": {
    "weight": 0.25,
    "dimensions": "18x15x8cm",
    "color": "Black"
  }
}

# Create with specific ID
PUT /products/_doc/PROD002
{
  "id": "PROD002",
  "name": "Ergonomic Office Chair",
  "description": "Adjustable lumbar support ergonomic chair for home office",
  "category": "Furniture",
  "subcategory": "Chairs",
  "price": 299.99,
  "brand": "ComfortPlus",
  "tags": ["ergonomic", "office", "adjustable"],
  "rating": 4.2,
  "stock": 50,
  "isActive": true,
  "createdAt": "2024-01-20T14:00:00Z",
  "specifications": {
    "weight": 15.5,
    "dimensions": "60x60x120cm",
    "color": "Gray"
  }
}
```

### READ - Retrieving Documents

```json
# Get by ID
GET /products/_doc/PROD002

# Get multiple documents
GET /products/_mget
{
  "ids": ["PROD001", "PROD002"]
}

# Check if document exists
HEAD /products/_doc/PROD002
```

### UPDATE - Modifying Documents

```json
# Partial update (preferred)
POST /products/_update/PROD002
{
  "doc": {
    "price": 279.99,
    "stock": 45
  }
}

# Full document replacement
PUT /products/_doc/PROD002
{
  "id": "PROD002",
  "name": "Ergonomic Office Chair Pro",
  "description": "Premium adjustable lumbar support ergonomic chair",
  "category": "Furniture",
  "subcategory": "Chairs",
  "price": 349.99,
  "brand": "ComfortPlus",
  "tags": ["ergonomic", "office", "premium", "adjustable"],
  "rating": 4.5,
  "stock": 30,
  "isActive": true,
  "createdAt": "2024-01-20T14:00:00Z",
  "specifications": {
    "weight": 16.0,
    "dimensions": "60x60x125cm",
    "color": "Black"
  }
}

# Update with script
POST /products/_update/PROD002
{
  "script": {
    "source": "ctx._source.stock -= params.sold",
    "params": {
      "sold": 5
    }
  }
}
```

### DELETE - Removing Documents

```json
# Delete single document
DELETE /products/_doc/PROD002

# Delete by query
POST /products/_delete_by_query
{
  "query": {
    "term": {
      "isActive": false
    }
  }
}
```

---

## 2.5 Bulk Operations

For better performance, use bulk operations:

```json
POST /products/_bulk
{"index": {"_id": "PROD003"}}
{"id": "PROD003", "name": "Mechanical Keyboard", "description": "RGB mechanical gaming keyboard with Cherry MX switches", "category": "Electronics", "subcategory": "Peripherals", "price": 129.99, "brand": "GameTech", "tags": ["mechanical", "rgb", "gaming"], "rating": 4.7, "stock": 100, "isActive": true, "createdAt": "2024-02-01T09:00:00Z"}
{"index": {"_id": "PROD004"}}
{"id": "PROD004", "name": "Standing Desk", "description": "Electric height adjustable standing desk 60x30 inches", "category": "Furniture", "subcategory": "Desks", "price": 449.99, "brand": "ErgoDesk", "tags": ["standing", "electric", "adjustable"], "rating": 4.4, "stock": 25, "isActive": true, "createdAt": "2024-02-05T11:30:00Z"}
{"index": {"_id": "PROD005"}}
{"id": "PROD005", "name": "Laptop Stand", "description": "Aluminum adjustable laptop stand for MacBook and laptops", "category": "Electronics", "subcategory": "Accessories", "price": 49.99, "brand": "TechGear", "tags": ["aluminum", "adjustable", "portable"], "rating": 4.3, "stock": 200, "isActive": true, "createdAt": "2024-02-10T08:00:00Z"}
{"index": {"_id": "PROD006"}}
{"id": "PROD006", "name": "Noise Canceling Earbuds", "description": "True wireless earbuds with active noise cancellation", "category": "Electronics", "subcategory": "Audio", "price": 199.99, "brand": "TechAudio", "tags": ["wireless", "noise-canceling", "earbuds"], "rating": 4.6, "stock": 150, "isActive": true, "createdAt": "2024-02-15T13:00:00Z"}
```

---

## 2.6 Index Operations

### List All Indices

```json
GET _cat/indices?v
```

### Get Index Info

```json
GET /products
```

### Delete an Index

```json
DELETE /products
```

### Refresh an Index

Forces all changes to be searchable immediately:

```json
POST /products/_refresh
```

### Index Aliases

```json
# Create alias
POST /_aliases
{
  "actions": [
    { "add": { "index": "products", "alias": "products-live" } }
  ]
}

# Query via alias
GET /products-live/_search
```

---

## 2.7 Checkpoint Questions

1. ✅ What's the difference between `text` and `keyword`?
2. ✅ When would you use multi-fields?
3. ✅ What's the difference between `POST` and `PUT` for document creation?
4. ✅ How do you update only specific fields in a document?
5. ✅ Why use bulk operations?

---

## 2.8 Exercises

### Exercise 1: Create a Users Index

Create an index for users with these fields:

| Field | Type | Notes |
|-------|------|-------|
| userId | keyword | Unique ID |
| email | keyword | For exact match |
| fullName | text + keyword | Search + sort |
| department | keyword | For filtering |
| joinDate | date | Date joined |
| isActive | boolean | Status |
| skills | keyword (array) | Multiple skills |

```json
# Your turn! Create the mapping:
PUT /users
{
  "mappings": {
    "properties": {
      // Add your fields here
    }
  }
}
```

### Exercise 2: Add Sample Users

Add 5 user documents using bulk API with different departments and skills.

### Exercise 3: Practice Updates

1. Update a user's department
2. Add a new skill to a user
3. Deactivate a user (set isActive to false)

---

## Next Steps

✅ **Module 2 Complete!**

👉 Continue to [Module 3: Query DSL](./03-query-dsl.md) to learn how to search and filter data.
