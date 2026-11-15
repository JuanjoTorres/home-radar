Estimates a renovation without changing anything. Two ways:

- **For a listing** (`listingId`): the same estimate the analysis uses, including the manual override if there is one. Any
  other field given overrides the listing's value for this estimate only (for example `level` to see what a full renovation
  would cost).
- **From parameters**: built area, bathrooms, rooms, construction year, property type, new build and a description (Spanish,
  Catalan, English or German keywords are read).

The level is chosen from the description keywords and the construction year (see `getListingAnalysis`); `level` forces it.
The cost range is the level's €/m² for Mallorca × area (80 m² when not given); `lines` breaks it down into kitchen, bathrooms,
flooring, painting, installations, windows, doors… with the remainder under `otherWorks`, plus permits and ICIO. Figures are
estimates with VAT included from public price surveys corrected for Mallorca (see `docs/research/reforma-mallorca.md`); get
real quotes before signing.
