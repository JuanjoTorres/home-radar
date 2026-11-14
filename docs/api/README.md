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
The response carries an `ETag`; send it back as `If-Match` when you `PUT` changes. Phase 2 will use the profile to answer
"can I afford this listing?".

## 2. Search what is on the market
```http
POST /v1/listings/search
{ "price": {"max": 300000}, "municipalities": ["Inca", "Lloseta", "Binissalem"], "rooms": {"min": 2},
  "excludePossession": ["NoPossession", "Occupied"], "sort": "PriceAsc", "limit": 25 }
```
Follow `nextCursor` for the next page. `GET /v1/listings?municipality=Inca&priceMax=300000` covers the simple cases.

## 3. Save the search as an alert
`POST /v1/search-profiles` with the same `filter` object and `"channels": ["Telegram"]`. New listings matching it arrive on
Telegram within one polling interval of the source (15 minutes for priority sources).

## 4. Follow a listing
`POST /v1/watchlist {"listingId": "…", "notes": "visita sábado"}` — from then on every price, text, photo or status change is
notified with a diff, and `GET /v1/listings/{id}/history` shows the full timeline.

## 5. Watch the sources
`GET /v1/sources` shows health; `GET /v1/sources/{id}/health` the last runs with counters and the reason for any degraded or
failed run.
