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

**Affordability.** `affordability.financingProfileId` keeps listings whose price is at most the profile's maximum affordable
price for the listing's island (value limits differ per island) and kind (resale pays ITP; new build pays VAT + AJD). The
maximum is the same one `calculateMaximumAffordablePrice` returns: highest price with verdict `Affordable` (or `Borderline`
with `minimumVerdict`), taxes and closing costs included, with the profile's loan-to-value or an eligible ICO/IBAVI guarantee
when `includeGuarantees` is true. `affordability.budget` keeps listings whose price + taxes + closing costs fit in the budget,
taxed at standard rates unless a profile is also given. Listings without a price never pass. Renovation is not included until
the rule-based estimator of phase 3. Unknown municipalities are treated as Mallorca, like in the analysis.
