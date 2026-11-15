Replaces the rule-based renovation estimate of a listing with your own figures after a visit or a builder's quote. Give one
of:

- `items`: amounts per work (`kitchen`, `bathroom`, `painting`, or any code you like); the total is their sum.
- `totalBudget`: a single amount.
- `level`: only the level; the cost is then the level's €/m² range applied to the listing's area.

`notes` keeps your comments. The override is used everywhere from then on: analysis, affordability search, alerts, score and
negotiation (`source: Manual`), and no contingency is added to a manual amount. The response is the resulting estimate.

**Concurrency.** The response carries an `ETag`. Send it back in `If-Match` to update only if nobody changed the override in
between; a stale value returns 412. Without `If-Match` the last write wins.
