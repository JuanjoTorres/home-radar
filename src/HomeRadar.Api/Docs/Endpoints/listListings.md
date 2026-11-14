Quick listing lookup driven by query-string parameters. It is a thin front for `searchListings` with the most common filters
(municipality, price range, source, "new within N hours") and the same cursor envelope. Use `searchListings` for geographic,
history-based or multi-value filters.

Results default to `Active` and `Relisted` listings, newest first (`firstSeenAt`). `daysOnMarket` counts from the publication
date when the source publishes one (Solvia, Aliseda, Inmovilla) and from the first time HomeRadar saw the listing otherwise.
