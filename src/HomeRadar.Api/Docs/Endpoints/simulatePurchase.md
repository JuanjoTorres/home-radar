Runs the same analysis as `getListingAnalysis` on a purchase described in the request, without a listing: price, resale or new
build, municipality (it decides the island value limit), optional cadastral reference value and renovation budget. Financing
comes from `financing` (inline assumptions), else from `financingProfileId`, else from the default profile.

Use it to play with margins: "if I buy at X and renovate for Y, what does it cost and can I still afford it?", or to see how
a different Euríbor (`euriborPercent`) changes the payments of a variable or mixed mortgage. Renovation adds the contingency
percentage of the business configuration (10 % by default). `on` evaluates the rules, island limits and aid valid on another
date. Nothing is stored.
