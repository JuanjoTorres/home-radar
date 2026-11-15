# HomeRadar API — quick start

Base URL: `https://<host>/v1`. Every call needs `X-Api-Key: <key>`. Interactive docs: `/scalar`; raw contract: `/openapi/v1.json`
(the committed copy lives in `docs/api/openapi.v1.json`).

## 1. Create your financing profile
```http
POST /v1/financing-profiles
Idempotency-Key: 5d2d…
{ "key": "main", "name": "Yo", "savings": {"amount": 60000}, "netMonthlyIncome": {"amount": 3000},
  "mortgage": {"type": "Fixed", "maxLoanToValue": 0.8, "nominalRatePercent": 2.9, "termMonths": 360},
  "buyer": {"age": 34, "firstHome": true, "primaryResidence": true, "residentInBalearics": true, "yearsResidentInBalearics": 5},
  "isDefault": true }
```
The response carries an `ETag`; send it back as `If-Match` when you `PUT` changes. Optional buyer fields unlock or rule out
reliefs and aid: `taxableIncomeBase` and `jointTaxReturn` (100 % ITP bonus under 30), `grossAnnualIncome`, `netWorth`,
`buyerCount` and `yearsResidentInSpain` (ICO and IBAVI guarantees, 10,000 € grant). Missing data never invents a relief: the
analysis marks it `Unknown` and tells you how much you would save if it applied.

## 2. Find out what you can pay
```http
POST /v1/affordability/maximum-price
{ "municipality": "Inca", "purchaseKind": "Resale", "includeGuarantees": true }
```
Returns the highest price that keeps the verdict `Affordable` (taxes and closing costs included) and what limits it
(`CashShortfall`, `DebtToIncomeAboveLimit`…). `POST /v1/analysis/simulations` analyses any price, renovation budget or Euríbor
you want, and `GET /v1/business-config` / `GET /v1/tax-rules` show the assumptions and the rules behind every number.

## 3. Search only what you can buy
```http
POST /v1/listings/search
{ "price": {"max": 300000}, "municipalities": ["Inca", "Lloseta", "Binissalem"], "rooms": {"min": 2},
  "excludePossession": ["NoPossession", "Occupied"], "affordability": {"financingProfileId": "<id>"},
  "sort": "PriceAsc", "limit": 25 }
```
Add `"minScore": 50`, `"renovationLevels": ["ReadyToMoveIn", "Light"]`, `"excludeRedFlags": ["occupied", "noPossession"]`
and `"sort": "ScoreDesc"` to see the best opportunities first. Follow `nextCursor` for the next page. `GET /v1/listings?municipality=Inca&affordableFor=<id>` covers the simple cases;
`budget=330000` keeps listings whose price + taxes + costs fit in a total budget.

## 4. Analyse a listing
`GET /v1/listings/{id}/analysis` (default profile, or `?financingProfileId=`) returns the verdict, every tax with the rule
applied and its legal source, closing costs, loan, cash needed on signing day, payments under each Euríbor scenario, aid
eligibility, a version financed with an ICO/IBAVI guarantee when it helps, and the maximum price for that listing. The analysis
is stored with its inputs and configuration versions, so it can be reproduced later.

`intelligence` in the same response adds the estimated renovation (level, cost range, breakdown and what to check on the
visit), red flags, comparison with similar listings, negotiation margin, an opportunity score 0–100 and the bid: opening offer,
recommended bid and walk-away price. Renovation is already included in the total cost and the verdict.

## 4b. After the visit
- `PUT /v1/listings/{id}/renovation-override` with the builder's figures (`items`, `totalBudget` or just `level`): from then on
  analysis, search, score and alerts use them. `DELETE` goes back to the estimate.
- `PUT /v1/listings/{id}/cadastral-data` with the cadastral reference and the reference value you looked up in the Sede
  Electrónica: transfer tax is then computed on the higher of price and reference value.
- `POST /v1/renovation/estimates` to try another level or area without changing anything, and `POST /v1/analysis/simulations`
  with `builtSquareMetres` to see the largest renovation you can afford and the largest one that still makes sense against
  renovated homes nearby.

## 5. Save the search as an alert
`POST /v1/search-profiles` with the same `filter` object and `"channels": ["Telegram"]`. New listings matching it arrive on
Telegram within one polling interval of the source (15 minutes for priority sources), with the verdict for your default
profile, the score, the estimated renovation and any red flag. Put the `affordability` object in the filter to be alerted only about what you can buy.

## 6. Follow a listing
`POST /v1/watchlist {"listingId": "…", "notes": "visita sábado"}` — from then on every price, text, photo or status change is
notified with a diff, and `GET /v1/listings/{id}/history` shows the full timeline.

## 7. Watch the sources
`GET /v1/sources` shows health; `GET /v1/sources/{id}/health` the last runs with counters and the reason for any degraded or
failed run.
