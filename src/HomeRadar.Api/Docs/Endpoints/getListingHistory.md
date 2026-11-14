The complete, immutable timeline of a listing, newest first. Every event points to the raw snapshot that evidences it, so a
price drop or a changed sentence can be audited later.

Payload per type: `Created` {price, municipality, propertyType}; `PriceChanged` {before, after, deltaAmount, deltaPercent};
`DescriptionChanged` {before, after}; `PhotoAdded` / `PhotoRemoved` / `PhotoReplaced` {url, position, replacedUrl};
`FeaturesChanged` {before, after} (only changed keys; `_structure` summarises rooms/bathrooms/area/type);
`StatusChanged` {before, after, possessionBefore, possessionAfter}; `Relisted` {lastSeenBefore, priceBefore, priceAfter};
`CrossListed` and `AgencyChanged` {before, after}.
