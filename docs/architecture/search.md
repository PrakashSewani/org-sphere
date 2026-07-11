# Search

## Search Architecture

OrgSphere provides powerful search across the entire Organization Graph with relationship-aware results.

---

## Search Capabilities

### What Can Be Searched

| Entity | Searchable Fields |
|--------|-------------------|
| Employee | Name, role, skills, certifications, email |
| Department | Name, description, head |
| Team | Name, focus, members |
| Project | Name, description, members |
| Document | Name, content, type |
| Approval | Type, requester, status |

### Search Types

| Type | Description |
|------|-------------|
| Exact Match | Precise field matching |
| Fuzzy Search | Typo-tolerant matching |
| Full-Text | Content search across fields |
| Relationship | Find by connections |
| Aggregated | Count and group results |

---

## Relationship-Aware Search

### Graph Traversal Queries

Find "React developers in Engineering":

```
MATCH (emp:Employee)-[:HAS_SKILL]->(skill:Skill {name: "React"})
WHERE (emp)-[:MEMBER_OF]->()-[:CONTAINS]->(:Department {name: "Engineering"})
RETURN emp
```

Find "People reporting to Alice":

```
MATCH (alice:Employee {name: "Alice"})<-[:REPORTS_TO]-(emp:Employee)
RETURN emp
```

### Faceted Search

Results include facets based on graph structure:

- Department breakdown
- Team breakdown
- Location breakdown
- Role breakdown

---

## Search Interface

### Global Search

Search bar available everywhere:

- Type-ahead suggestions
- Recent searches
- Saved searches
- Quick filters

### Advanced Search

Power user interface:

- Multiple field filters
- Relationship filters
- Date ranges
- Boolean operators

### AI-Powered Search

Natural language queries:

- "Find all senior engineers in New York"
- "Who has AWS certification?"
- "Show me the marketing team"
- "Which managers are overloaded?"

---

## Indexing

### Index Types

| Index | Purpose |
|-------|---------|
| Primary | Entity field indexing |
| Full-Text | Content search indexing |
| Relationship | Edge-based indexing |
| Geospatial | Location-based indexing |

### Index Updates

- Real-time updates on entity changes
- Batch reindexing for bulk operations
- Async indexing for performance

---

## Search Results

### Result Format

```json
{
  "query": "React developers",
  "total": 25,
  "results": [
    {
      "type": "employee",
      "id": "emp-123",
      "name": "John Doe",
      "role": "Senior Engineer",
      "department": "Engineering",
      "skills": ["React", "TypeScript"],
      "relevance": 0.95
    }
  ],
  "facets": {
    "department": [
      { "name": "Engineering", "count": 20 },
      { "name": "Frontend", "count": 5 }
    ]
  }
}
```

### Ranking

Results ranked by:

- Relevance score
- Relationship proximity
- Entity importance
- Recency

---

## Performance

### Optimization

- Elasticsearch/OpenSearch for indexing
- Read replicas for search queries
- Caching frequent searches
- Pagination for large result sets

### Scale

- Handle millions of entities
- Sub-second search response
- Real-time index updates
- Concurrent search queries
