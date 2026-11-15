Runs the same analysis as `getListingAnalysis` on a purchase described in the request, without a listing: price, resale or new
build, municipality (it decides the island value limit), optional cadastral reference value and renovation budget. Financing
comes from `financing` (inline assumptions), else from `financingProfileId`, else from the default profile.

Use it to play with margins: "if I buy at X and renovate for Y, what does it cost and can I still afford it?", or to see how
a different Euríbor (`euriborPercent`) changes the payments of a variable or mixed mortgage. Renovation adds the contingency
percentage of the business configuration (15 %).

**Margins.** `margins.maximumAffordableRenovation` is the largest renovation budget that keeps the verdict `Affordable` at
this price. With `builtSquareMetres`, the value of an equivalent renovated home (median €/m² of ready-to-move-in comparables
in the municipality or island) gives `maximumSensibleRenovation` — the budget at which price, taxes, costs and renovation
equal what a renovated home costs — and `totalCostVersusRenovatedValuePercent`. `on` evaluates the rules, island limits and aid valid on another
date. Nothing is stored.
