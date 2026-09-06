 ```
                 ┌─────────────────┐
                 │      USER       │
                 └────────┬────────┘
                          │
        ┌─────────────────┼───────────────────┐
        │                 │                   │
        ▼                 ▼                   ▼
   Goal Management   Meal Management     Reports (Queries)
        │                 │                   │
        │                 │                   │
        ▼                 ▼                   ▼
    Set Goals        Add Food             View Charts
    Edit Goals       Edit Food            View Trends
    View Goals       Delete Food          Compare Goals
```

## VLM Use case
1. I can use genaral purpose VLM 
2. I can use following especially trained for food nutrition llms
    - Food-R1
    - CaLoRAify

## Food and reporting APIs

All food-entry and report endpoints require `Authorization: Bearer <jwt>`.

| Method | Route | Purpose |
| --- | --- | --- |
| `POST` | `/api/food-entries` | Add a food entry with nutrition values |
| `GET` | `/api/food-entries?from=YYYY-MM-DD&to=YYYY-MM-DD` | List the authenticated user's entries |
| `PUT` | `/api/food-entries/{id}` | Edit an owned food entry |
| `DELETE` | `/api/food-entries/{id}` | Delete an owned food entry |
| `GET` | `/api/reports/daily?from=YYYY-MM-DD&to=YYYY-MM-DD` | Return daily calorie and macro totals |
| `GET` | `/api/reports/trends?from=YYYY-MM-DD&to=YYYY-MM-DD` | Return chart-ready daily trend points |

When dates are omitted, food history defaults to the previous 30 days and reports default to the previous 7 days. Swagger descriptions and response codes are generated from the endpoint summaries in the API project.