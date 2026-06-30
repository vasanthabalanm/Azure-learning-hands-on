# Module 6: Advanced Topics

## 🎯 Learning Objectives

By the end of this module, you will:

- ✅ Understand text analysis and custom analyzers
- ✅ Use aggregations for analytics
- ✅ Implement autocomplete with edge n-grams
- ✅ Optimize search performance
- ✅ Production deployment considerations

---

## 6.1 Text Analysis Deep Dive

### How Text Analysis Works

```
┌─────────────────────────────────────────────────────────────────┐
│                      TEXT ANALYSIS PIPELINE                      │
├─────────────────────────────────────────────────────────────────┤
│                                                                   │
│  Input: "The QUICK Brown Fox-Jumper!"                            │
│                        │                                          │
│                        ▼                                          │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │  Character Filters                                        │   │
│  │  - html_strip: Removes HTML tags                          │   │
│  │  - mapping: Character replacements                        │   │
│  │  Output: "The QUICK Brown Fox-Jumper!"                    │   │
│  └──────────────────────────────────────────────────────────┘   │
│                        │                                          │
│                        ▼                                          │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │  Tokenizer                                                │   │
│  │  - standard: Word-based tokenization                      │   │
│  │  Output: ["The", "QUICK", "Brown", "Fox", "Jumper"]       │   │
│  └──────────────────────────────────────────────────────────┘   │
│                        │                                          │
│                        ▼                                          │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │  Token Filters                                            │   │
│  │  - lowercase: Convert to lowercase                        │   │
│  │  - stemmer: Reduce to root form                           │   │
│  │  - stop: Remove common words                              │   │
│  │  Output: ["quick", "brown", "fox", "jumper"]              │   │
│  └──────────────────────────────────────────────────────────┘   │
│                                                                   │
└─────────────────────────────────────────────────────────────────┘
```

### Testing Analyzers

```json
# Test the standard analyzer
POST /_analyze
{
  "analyzer": "standard",
  "text": "The QUICK Brown Fox-Jumper!"
}

# Test custom analysis
POST /_analyze
{
  "tokenizer": "standard",
  "filter": ["lowercase", "porter_stem"],
  "text": "Running quickly through the jumping foxes"
}
```

### Creating Custom Analyzers

```json
PUT /products_v2
{
  "settings": {
    "analysis": {
      "char_filter": {
        "html_stripper": {
          "type": "html_strip"
        }
      },
      "tokenizer": {
        "autocomplete_tokenizer": {
          "type": "edge_ngram",
          "min_gram": 2,
          "max_gram": 15,
          "token_chars": ["letter", "digit"]
        }
      },
      "filter": {
        "english_stemmer": {
          "type": "stemmer",
          "language": "english"
        },
        "english_stop": {
          "type": "stop",
          "stopwords": "_english_"
        }
      },
      "analyzer": {
        "product_analyzer": {
          "type": "custom",
          "char_filter": ["html_stripper"],
          "tokenizer": "standard",
          "filter": ["lowercase", "english_stop", "english_stemmer"]
        },
        "autocomplete_index": {
          "type": "custom",
          "tokenizer": "autocomplete_tokenizer",
          "filter": ["lowercase"]
        },
        "autocomplete_search": {
          "type": "custom",
          "tokenizer": "standard",
          "filter": ["lowercase"]
        }
      }
    }
  },
  "mappings": {
    "properties": {
      "name": {
        "type": "text",
        "analyzer": "product_analyzer",
        "fields": {
          "autocomplete": {
            "type": "text",
            "analyzer": "autocomplete_index",
            "search_analyzer": "autocomplete_search"
          },
          "keyword": {
            "type": "keyword"
          }
        }
      },
      "description": {
        "type": "text",
        "analyzer": "product_analyzer"
      }
    }
  }
}
```

### Autocomplete with Edge N-Grams

```json
# Index a product
POST /products_v2/_doc/1
{
  "name": "Wireless Bluetooth Headphones",
  "description": "Premium noise-canceling headphones"
}

# Autocomplete search
GET /products_v2/_search
{
  "query": {
    "match": {
      "name.autocomplete": "wire"
    }
  }
}

# This will match because "wire" matches edge n-grams:
# "wi", "wir", "wire", "wirel", "wirele", "wireles", "wireless"
```

---

## 6.2 Aggregations for Analytics

### Types of Aggregations

| Type | Purpose | Example |
|------|---------|---------|
| **Bucket** | Group documents | Terms, Range, Date histogram |
| **Metric** | Calculate values | Sum, Avg, Min, Max, Stats |
| **Pipeline** | Process other aggs | Moving avg, Cumulative sum |

### Terms Aggregation

```json
# Get product count by category
GET /products/_search
{
  "size": 0,
  "aggs": {
    "categories": {
      "terms": {
        "field": "category",
        "size": 20
      }
    }
  }
}
```

### Nested Aggregations

```json
# Average price by category
GET /products/_search
{
  "size": 0,
  "aggs": {
    "categories": {
      "terms": {
        "field": "category",
        "size": 10
      },
      "aggs": {
        "avg_price": {
          "avg": {
            "field": "price"
          }
        },
        "price_stats": {
          "stats": {
            "field": "price"
          }
        }
      }
    }
  }
}
```

### Range Aggregations

```json
# Price distribution
GET /products/_search
{
  "size": 0,
  "aggs": {
    "price_ranges": {
      "range": {
        "field": "price",
        "ranges": [
          { "key": "Budget", "to": 50 },
          { "key": "Mid-range", "from": 50, "to": 200 },
          { "key": "Premium", "from": 200, "to": 500 },
          { "key": "Luxury", "from": 500 }
        ]
      }
    }
  }
}
```

### Date Histogram

```json
# Products added per month
GET /products/_search
{
  "size": 0,
  "aggs": {
    "products_over_time": {
      "date_histogram": {
        "field": "createdAt",
        "calendar_interval": "month"
      },
      "aggs": {
        "total_value": {
          "sum": {
            "field": "price"
          }
        }
      }
    }
  }
}
```

### Combined Query + Aggregations

```json
# Search with facets
GET /products/_search
{
  "query": {
    "bool": {
      "must": [
        { "match": { "description": "wireless" } }
      ],
      "filter": [
        { "term": { "isActive": true } }
      ]
    }
  },
  "aggs": {
    "categories": {
      "terms": { "field": "category" }
    },
    "brands": {
      "terms": { "field": "brand" }
    },
    "price_stats": {
      "stats": { "field": "price" }
    },
    "avg_rating": {
      "avg": { "field": "rating" }
    }
  }
}
```

---

## 6.3 Performance Optimization

### Index Settings for Performance

```json
PUT /products_optimized
{
  "settings": {
    "number_of_shards": 1,
    "number_of_replicas": 0,
    "refresh_interval": "30s",
    "index": {
      "sort.field": ["category", "createdAt"],
      "sort.order": ["asc", "desc"]
    }
  }
}
```

### Query Optimization Tips

| Technique | Description |
|-----------|-------------|
| Use `filter` over `must` | Filters are cached, don't affect scoring |
| Limit `_source` fields | Reduce data transfer |
| Avoid wildcards at start | `*term` is slow |
| Use `keyword` for exact match | Faster than `text` for exact searches |
| Prefer `terms` over multiple `term` | Single query vs multiple |
| Set appropriate `size` | Don't fetch more than needed |

### Optimized Search Query

```json
GET /products/_search
{
  "query": {
    "bool": {
      "must": [
        {
          "multi_match": {
            "query": "wireless headphones",
            "fields": ["name^3", "description"],
            "type": "best_fields"
          }
        }
      ],
      "filter": [
        { "term": { "category": "Electronics" } },
        { "range": { "price": { "lte": 200 } } },
        { "term": { "isActive": true } }
      ]
    }
  },
  "_source": ["id", "name", "price", "brand", "rating"],
  "from": 0,
  "size": 20
}
```

### Caching

```json
# Named queries for caching
GET /products/_search
{
  "query": {
    "bool": {
      "filter": [
        {
          "term": {
            "category": {
              "value": "Electronics",
              "_name": "category_filter"
            }
          }
        }
      ]
    }
  }
}

# Request cache
GET /products/_search?request_cache=true
{
  "query": { "match_all": {} },
  "aggs": {
    "categories": { "terms": { "field": "category" } }
  }
}
```

---

## 6.4 Index Aliases and Zero-Downtime Reindexing

### Creating Aliases

```json
# Create alias
POST /_aliases
{
  "actions": [
    { "add": { "index": "products_v1", "alias": "products" } }
  ]
}

# Application always uses alias "products"
GET /products/_search
```

### Zero-Downtime Reindex

```json
# Step 1: Create new index with updated mapping
PUT /products_v2
{
  "settings": { ... },
  "mappings": { ... }
}

# Step 2: Reindex data
POST /_reindex
{
  "source": { "index": "products_v1" },
  "dest": { "index": "products_v2" }
}

# Step 3: Switch alias atomically
POST /_aliases
{
  "actions": [
    { "remove": { "index": "products_v1", "alias": "products" } },
    { "add": { "index": "products_v2", "alias": "products" } }
  ]
}

# Step 4: Delete old index (optional)
DELETE /products_v1
```

---

## 6.5 Monitoring and Debugging

### Cluster Health

```json
GET /_cluster/health

GET /_cat/health?v

GET /_cat/nodes?v

GET /_cat/indices?v
```

### Index Stats

```json
GET /products/_stats

GET /products/_segments
```

### Query Profiling

```json
GET /products/_search
{
  "profile": true,
  "query": {
    "match": { "name": "headphones" }
  }
}
```

### Explain Query

```json
GET /products/_explain/PROD001
{
  "query": {
    "match": { "name": "wireless" }
  }
}
```

### Slow Log

```json
PUT /products/_settings
{
  "index.search.slowlog.threshold.query.warn": "10s",
  "index.search.slowlog.threshold.query.info": "5s",
  "index.search.slowlog.threshold.query.debug": "2s",
  "index.search.slowlog.threshold.fetch.warn": "1s"
}
```

---

## 6.6 Production Checklist

### Security

- [ ] Enable TLS/SSL encryption
- [ ] Configure authentication (SAML, LDAP, or internal users)
- [ ] Set up role-based access control
- [ ] Use API keys for applications
- [ ] Enable audit logging

### Cluster Configuration

- [ ] Multiple nodes for high availability
- [ ] Dedicated master nodes (3 minimum)
- [ ] Separate data and coordinating nodes
- [ ] Configure heap size (50% of RAM, max 32GB)

### Monitoring

- [ ] Set up OpenSearch Dashboards
- [ ] Configure alerting for cluster health
- [ ] Monitor disk space and heap usage
- [ ] Set up backup/snapshot policies

### Performance

- [ ] Use SSDs for data nodes
- [ ] Proper shard sizing (10-50GB per shard)
- [ ] Configure refresh interval based on needs
- [ ] Use bulk APIs for indexing

---

## 6.7 Exercises

### Exercise 1: Custom Analyzer

Create an index with a custom analyzer for product names that:
- Strips HTML
- Tokenizes on whitespace and special characters
- Converts to lowercase
- Applies English stemming

### Exercise 2: Analytics Dashboard Query

Write aggregations to answer:
- Top 5 categories by product count
- Average price per brand
- Product count by rating ranges (1-2, 2-3, 3-4, 4-5)

### Exercise 3: Performance Optimization

Take the following slow query and optimize it:

```json
GET /products/_search
{
  "query": {
    "bool": {
      "must": [
        { "wildcard": { "name": "*phone*" } },
        { "match": { "category": "Electronics" } },
        { "range": { "price": { "lte": 200 } } }
      ]
    }
  }
}
```

---

## 6.8 Summary

### What You've Learned

| Module | Topics |
|--------|--------|
| 1 | Docker setup, cluster basics |
| 2 | Indices, mappings, CRUD |
| 3 | Query DSL, filters, sorting |
| 4 | .NET client, services, APIs |
| 5 | Angular UI, autocomplete, facets |
| 6 | Analyzers, aggregations, optimization |

### Next Steps

- 🔗 [OpenSearch Documentation](https://opensearch.org/docs/latest/)
- 🔗 [OpenSearch.Client GitHub](https://github.com/opensearch-project/opensearch-net)
- 🔗 [OpenSearch Plugins](https://opensearch.org/docs/latest/install-and-configure/plugins/)

---

**🎉 Congratulations! You've completed the OpenSearch learning guide!**
