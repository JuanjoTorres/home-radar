Full-text-free structured search over every listing HomeRadar has ingested. All filters are optional and combine with AND.

**Geography.** `municipalities` compares case-insensitively with the municipality published by the source. `polygonWkt` is a
WKT polygon in WGS84 (`POLYGON((lon lat, …))`) evaluated with PostGIS. `radius` is a circle in metres around a point, evaluated
on an EPSG:25831 projection so distances are metric. Listings without coordinates never match geographic filters.

**History.** `daysOnMarket`, `priceDropsMin`, `cumulativeDropPercentMin` and `newWithinHours` come from the append-only change
log, not from the source: a drop is only counted once HomeRadar has seen both prices.

**Possession.** Bank-owned stock often carries `NoPossession` (occupied, moratorium) or `RentedUntil`; `excludePossession`
removes those. `Unknown` means the source gave no signal.

**Pagination.** Keyset cursors: pass back `nextCursor` unchanged with the same filters and sort. Changing the filters or sort
with an old cursor returns a 400. Sorts: `FirstSeenDesc` (default), `PriceAsc`, `PriceDesc`, `PricePerM2`, `PriceDropDesc`.
Listings without a price sort last for price-based sorts.

Affordability filtering (`financingProfileId`, `budget`) arrives with the analysis engine in phase 2 as an additive change.
