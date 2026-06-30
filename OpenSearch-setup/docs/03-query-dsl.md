# Module 3: OpenSearch Query DSL

## 🎯 Learning Objectives

By the end of this module, you will:

- ✅ Understand the Query DSL structure
- ✅ Use full-text search queries (match, multi_match)
- ✅ Use term-level queries (term, terms, range)
- ✅ Combine queries with bool queries
- ✅ Implement highlighting and pagination

---

## 3.1 Query DSL Overview

### What is Query DSL?

**Query DSL** (Domain Specific Language) is OpenSearch's JSON-based query language.

### Basic Query Structure

```json
GET /index/_search
{
  "query": {
    "query_type": {
      "field_name": "search_value"
    }
  }
}
```

### Query Categories

```
┌─────────────────────────────────────────────────────────────────┐
│                      QUERY TYPES                                 │
├─────────────────────────────────────────────────────────────────┤
│                                                                   │
│  FULL-TEXT QUERIES                   TERM-LEVEL QUERIES         │
│  ├── match                           ├── term                    │
│  ├── match_phrase                    ├── terms                   │
│  ├── multi_match                     ├── range                   │
│  └── query_string                    ├── exists                  │
│                                      ├── prefix                  │
│                                      └── wildcard                │
│                                                                   │
│  COMPOUND QUERIES                    SPECIALIZED QUERIES         │
│  ├── bool                            ├── fuzzy                   │
│  ├── boosting                        ├── ids                     │
│  └── dis_max                         └── nested                  │
│                                                                   │
└─────────────────────────────────────────────────────────────────┘
```

---

## 3.2 Full-Text Queries

### Match Query

The most common query - analyzes the search text:

```json
# Simple match query
GET /products/_search
{
  "query": {
    "match": {
      "description": "wireless headphones"
    }
  }
}

# Match with operator (AND vs OR)
GET /products/_search
{
  "query": {
    "match": {
      "description": {
        "query": "wireless noise canceling",
        "operator": "and"
      }
    }
  }
}

# Match with minimum_should_match
GET /products/_search
{
  "query": {
    "match": {
      "description": {
        "query": "wireless noise canceling bluetooth",
        "minimum_should_match": "75%"
      }
    }
  }
}
```

### Match Phrase Query

Searches for exact phrase (word order matters):

```json
GET /products/_search
{
  "query": {
    "match_phrase": {
      "description": "noise canceling"
    }
  }
}

# With slop (allows words in between)
GET /products/_search
{
  "query": {
    "match_phrase": {
      "description": {
        "query": "wireless headphones",
        "slop": 2
      }
    }
  }
}
```

### Multi-Match Query

Search across multiple fields:

```json
# Search in name and description
GET /products/_search
{
  "query": {
    "multi_match": {
      "query": "wireless audio",
      "fields": ["name", "description"]
    }
  }
}

# With field boosting (name is 2x more important)
GET /products/_search
{
  "query": {
    "multi_match": {
      "query": "headphones",
      "fields": ["name^2", "description", "tags^1.5"]
    }
  }
}

# Different multi_match types
GET /products/_search
{
  "query": {
    "multi_match": {
      "query": "wireless bluetooth",
      "fields": ["name", "description"],
      "type": "best_fields"  // or "most_fields", "cross_fields"
    }
  }
}
```

---

## 3.3 Term-Level Queries

### Term Query

Exact match (no analysis):

```json
# Exact category match
GET /products/_search
{
  "query": {
    "term": {
      "category": "Electronics"
    }
  }
}

# Case-sensitive! This won't match "Electronics"
GET /products/_search
{
  "query": {
    "term": {
      "category": "electronics"  // Won't work if stored as "Electronics"
    }
  }
}
```

### Terms Query

Match any of multiple values:

```json
GET /products/_search
{
  "query": {
    "terms": {
      "category": ["Electronics", "Furniture", "Clothing"]
    }
  }
}
```

### Range Query

Numeric or date ranges:

```json
# Price range
GET /products/_search
{
  "query": {
    "range": {
      "price": {
        "gte": 100,
        "lte": 300
      }
    }
  }
}

# Date range
GET /products/_search
{
  "query": {
    "range": {
      "createdAt": {
        "gte": "2024-01-01",
        "lt": "2024-02-01",
        "format": "yyyy-MM-dd"
      }
    }
  }
}

# Relative date range
GET /products/_search
{
  "query": {
    "range": {
      "createdAt": {
        "gte": "now-30d/d",
        "lte": "now/d"
      }
    }
  }
}
```

### Exists Query

Check if field has a value:

```json
GET /products/_search
{
  "query": {
    "exists": {
      "field": "specifications.color"
    }
  }
}
```

### Prefix Query

Match field starting with value:

```json
GET /products/_search
{
  "query": {
    "prefix": {
      "name.keyword": "Wire"
    }
  }
}
```

### Wildcard Query

Pattern matching with * and ?:

```json
GET /products/_search
{
  "query": {
    "wildcard": {
      "name.keyword": "*Headphone*"
    }
  }
}
```

---

## 3.4 Bool Query (Compound Query)

The **bool** query combines multiple queries:

```
┌─────────────────────────────────────────────────────────────────┐
│                      BOOL QUERY CLAUSES                          │
├─────────────────────────────────────────────────────────────────┤
│                                                                   │
│  must       - Query MUST match (affects score)                   │
│  filter     - Query MUST match (no scoring, cached)              │
│  should     - Query SHOULD match (optional, boosts score)        │
│  must_not   - Query MUST NOT match (excludes docs)               │
│                                                                   │
└─────────────────────────────────────────────────────────────────┘
```

### Basic Bool Query

```json
GET /products/_search
{
  "query": {
    "bool": {
      "must": [
        { "match": { "description": "wireless" } }
      ],
      "filter": [
        { "term": { "category": "Electronics" } },
        { "range": { "price": { "lte": 200 } } }
      ],
      "should": [
        { "term": { "brand": "TechAudio" } }
      ],
      "must_not": [
        { "term": { "isActive": false } }
      ]
    }
  }
}
```

### Real-World Example: Product Search

```json
# Search "headphones" in Electronics, under $200, prefer TechAudio brand
GET /products/_search
{
  "query": {
    "bool": {
      "must": [
        {
          "multi_match": {
            "query": "headphones",
            "fields": ["name^2", "description"]
          }
        }
      ],
      "filter": [
        { "term": { "category": "Electronics" } },
        { "range": { "price": { "lte": 200 } } },
        { "term": { "isActive": true } },
        { "range": { "stock": { "gt": 0 } } }
      ],
      "should": [
        { "term": { "brand": "TechAudio" } },
        { "range": { "rating": { "gte": 4.5 } } }
      ],
      "minimum_should_match": 0
    }
  }
}
```

---

## 3.5 Fuzzy Search

Handle typos and misspellings:

```json
# Fuzzy match (allows typos)
GET /products/_search
{
  "query": {
    "fuzzy": {
      "name": {
        "value": "headphons",  // Typo
        "fuzziness": "AUTO"
      }
    }
  }
}

# Fuzzy within match query
GET /products/_search
{
  "query": {
    "match": {
      "name": {
        "query": "blootooth headphons",
        "fuzziness": "AUTO"
      }
    }
  }
}
```

---

## 3.6 Sorting and Pagination

### Sorting Results

```json
# Sort by price ascending
GET /products/_search
{
  "query": { "match_all": {} },
  "sort": [
    { "price": "asc" }
  ]
}

# Multiple sort fields
GET /products/_search
{
  "query": { "match_all": {} },
  "sort": [
    { "category": "asc" },
    { "price": "desc" },
    "_score"
  ]
}

# Sort by text field (use .keyword)
GET /products/_search
{
  "query": { "match_all": {} },
  "sort": [
    { "name.keyword": "asc" }
  ]
}
```

### Pagination

```json
# Page 1 (first 10 results)
GET /products/_search
{
  "query": { "match_all": {} },
  "from": 0,
  "size": 10
}

# Page 2
GET /products/_search
{
  "query": { "match_all": {} },
  "from": 10,
  "size": 10
}

# Page 3
GET /products/_search
{
  "query": { "match_all": {} },
  "from": 20,
  "size": 10
}
```

### Deep Pagination Warning

⚠️ **Don't use `from` > 10000** - Use `search_after` for deep pagination:

```json
# First page with sort
GET /products/_search
{
  "size": 10,
  "query": { "match_all": {} },
  "sort": [
    { "createdAt": "desc" },
    { "_id": "asc" }
  ]
}

# Next page using search_after (use last doc's sort values)
GET /products/_search
{
  "size": 10,
  "query": { "match_all": {} },
  "search_after": ["2024-02-15T13:00:00.000Z", "PROD006"],
  "sort": [
    { "createdAt": "desc" },
    { "_id": "asc" }
  ]
}
```

---

## 3.7 Highlighting

Show matching terms in results:

```json
GET /products/_search
{
  "query": {
    "match": {
      "description": "wireless noise canceling"
    }
  },
  "highlight": {
    "fields": {
      "description": {}
    }
  }
}

# Custom highlight tags
GET /products/_search
{
  "query": {
    "multi_match": {
      "query": "headphones",
      "fields": ["name", "description"]
    }
  },
  "highlight": {
    "pre_tags": ["<mark>"],
    "post_tags": ["</mark>"],
    "fields": {
      "name": {},
      "description": {
        "fragment_size": 150,
        "number_of_fragments": 3
      }
    }
  }
}
```

---

## 3.8 Source Filtering

Control which fields to return:

```json
# Only return specific fields
GET /products/_search
{
  "query": { "match_all": {} },
  "_source": ["name", "price", "category"]
}

# Exclude fields
GET /products/_search
{
  "query": { "match_all": {} },
  "_source": {
    "excludes": ["description", "specifications"]
  }
}

# Include and exclude
GET /products/_search
{
  "query": { "match_all": {} },
  "_source": {
    "includes": ["name", "price", "specifications.*"],
    "excludes": ["specifications.dimensions"]
  }
}
```

---

## 3.9 Complete Search Example

Combining everything:

```json
GET /products/_search
{
  "query": {
    "bool": {
      "must": [
        {
          "multi_match": {
            "query": "wireless headphones",
            "fields": ["name^3", "description", "tags^2"],
            "type": "best_fields",
            "fuzziness": "AUTO"
          }
        }
      ],
      "filter": [
        { "term": { "category": "Electronics" } },
        { "range": { "price": { "gte": 50, "lte": 300 } } },
        { "term": { "isActive": true } }
      ],
      "should": [
        { "term": { "brand": "TechAudio" } },
        { "range": { "rating": { "gte": 4.0 } } }
      ]
    }
  },
  "highlight": {
    "pre_tags": ["<strong>"],
    "post_tags": ["</strong>"],
    "fields": {
      "name": {},
      "description": { "fragment_size": 200 }
    }
  },
  "_source": ["name", "description", "price", "brand", "rating", "category"],
  "sort": [
    "_score",
    { "rating": "desc" }
  ],
  "from": 0,
  "size": 10
}
```

---

## 3.10 Checkpoint Questions

1. ✅ What's the difference between `match` and `term` queries?
2. ✅ When should you use `filter` vs `must` in a bool query?
3. ✅ How do you search across multiple fields?
4. ✅ What's the purpose of field boosting?
5. ✅ Why avoid deep pagination with `from`?

---

## 3.11 Exercises

### Exercise 1: Product Search

Write a query that:
- Searches for "gaming keyboard" in name and description
- Filters to Electronics category
- Price between $50-$200
- Rating >= 4.0
- Returns highlighted name and description

### Exercise 2: Autocomplete Simulation

Write a prefix query for autocomplete:
- User types "Wir"
- Should match products starting with "Wir" in name.keyword

### Exercise 3: Recent Products

Write a query that:
- Gets products created in the last 30 days
- Sorted by creation date (newest first)
- Paginated (10 per page)

---

## Next Steps

✅ **Module 3 Complete!**

👉 Continue to [Module 4: .NET Integration](./04-dotnet-integration.md) to build the backend API.
