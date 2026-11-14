Creates a saved search. `filter` uses the `searchListings` schema (sort, cursor and limit are ignored). Searches marked
`priority` make the worker poll their sources' first page every 15 minutes in addition to the full scans. Supports
`Idempotency-Key`.
