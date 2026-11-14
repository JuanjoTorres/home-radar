Returns the current state of one listing: summary fields, description, normalised features, photos with their perceptual hash
(once computed) and the cadastral reference when the source publishes it. `features` holds source-specific facts under stable
keys (`elevator`, `terrace`, `parking`, `pool`, `yearBuilt`, `energyRating`, `previousPrice`, `discountPercent`, `badges`,
`condition`, `postalCode`, `address`, `reference`, `campaign`, `specialSituation`, `cadastralParcel`…). `enrichments` holds facts
derived by the rules engine, each labelled with its origin.

`siblingListings` lists other listings of the same physical home once cross-source deduplication (phase 4) has matched them.
