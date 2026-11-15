Inverse calculation: the highest price the profile can pay with verdict `Affordable` (or `Borderline` with `minimumVerdict`),
taxes and closing costs included, for a resale or new-build home in a municipality, optionally keeping a renovation budget on
top.

Every constraint grows with the price — cash needed (deposit, taxes, fees), debt-to-income — and taxes only jump upwards (for
example when the value crosses the island limit and the reduced rate is lost), so the set of affordable prices is an interval.
The engine finds its upper end by doubling from 50,000 EUR and then bisecting to the cent; the price returned is affordable
and one cent more is not. `bindingConstraints` tells what limits it (usually the savings shortfall or the debt-to-income).

With `includeGuarantees`, each candidate price is evaluated with the best ICO or IBAVI guarantee the buyer qualifies for at
that price (IBAVI requires a price up to 382,744.84 EUR). `atMaximum` holds the full analysis at the maximum price.
