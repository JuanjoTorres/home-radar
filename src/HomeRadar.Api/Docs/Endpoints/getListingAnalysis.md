Answers "can I buy this one, and what would it really cost?" for one listing and one financing profile (the default profile
when `financingProfileId` is omitted). Deterministic, rule-based and reproducible.

**What goes into the total cost.** Price + taxes + notary + land registry + gestoria + appraisal (only with a mortgage, the
only mortgage cost left to the borrower by Ley 5/2019) + mortgage opening fee (if the profile has one) + guarantee fee (IBAVI
charges 0.5 % of the guaranteed amount) + renovation (the manual override if there is one, otherwise the probable cost of the
rule-based estimate) + a contingency on estimated renovations (15 % in the business configuration).

**Taxes.** Resale pays transfer tax (ITP) on the higher of price and cadastral reference value; new homes pay VAT (10 %, 4 %
for protected special regime) and stamp duty (AJD). The engine reads the tax rule sets valid on the calculation date for the
state (`ES`) and the Balearic Islands (`ES-IB`) — none of the figures live in code. For every tax it starts from the standard
rule (or a mandatory override such as AJD 2 % from 1 M EUR), evaluates every reduced rate and bonus against the buyer data,
and applies the cheapest option whose requirements are **all met**. Requirements that cannot be checked with the profile
(IRPF base, gross income, net worth…) make a relief `Unknown`: it is not applied, and `potentialSaving` tells how much less
would be paid if they were met. Reduced rates in the Balearic Islands cover the first 270,151.20 EUR and need the value to be
within the island limit (331,859.70 EUR in Mallorca since 2026-06-14); the excess pays 8 % ITP or 1.5 % AJD.

**Loan and payments.** Loan = price × loan-to-value (appraisal assumed equal to price). Monthly payment with the French formula
`L·i / (1 − (1 + i)^−n)`, `i` = annual rate / 12. Variable loans pay the initial rate for `fixedMonths` (12 by default when an
initial rate is given) and then Euríbor + spread; mixed loans pay the fixed rate for `fixedMonths`. Both are computed under
every Euríbor scenario of the business configuration (base, +1, +2 points).

**Verdict.** `Affordable` when savings cover deposit, taxes and costs, and (highest base payment + existing debts) / net
income is within the profile's limit, also under the stress scenario. `Borderline` for small gaps: shortfall up to the
configured tolerance (5,000 EUR), debt-to-income up to 3 points over the limit, the stress scenario over the limit, or a loan
ending after age 75. `NotAffordable` for anything larger. Each reason carries its amount or ratio.

**Guarantees and aid.** Every aid programme valid on the date is evaluated (ICO and IBAVI guarantees, the Govern's 10,000 EUR
grant). When a guarantee raises the loan-to-value, `withGuarantee` repeats the analysis with it (ICO and IBAVI are never
combined). Grants are paid after the purchase and are budget-limited, so they never count as cash on signing day.

**Maximum price.** `maximumPrice` is the highest price, to the cent, whose verdict stays `Affordable`; `bindingConstraints`
are the reasons that appear one cent above it.

**Intelligence.** `intelligence` adds the rule-based reading of the listing, also returned when the listing has no price:

- `renovation`: level (ReadyToMoveIn, Light, Medium, Full) from description keywords in Spanish, Catalan, English and German
  (negations such as "no necesita reforma" are ignored, "reformado en 2021" counts as recent) and from the construction year
  (Catastro first): before 1980 full, 1980–2002 medium, 2003–2008 light. Cost range = level €/m² × built area (80 m² assumed
  when unknown), with kitchen, bathrooms, flooring, windows… as an indicative breakdown, the rest as `otherWorks` and permits
  plus ICIO (~3.1 %). A manual override (`setRenovationOverride`) always prevails. `visitChecklist` lists what to check.
- `redFlags`: occupation, no possession, auction assignment, bare ownership, undivided share, missing habitability
  certificate, rustic land, VPO, tenant in place, seizure, encumbrances… from configurable keyword rules with exclusions, plus
  data rules: advertised area 15–30 % or more above the Catastro, rustic parcel, building of 50 years or more without IEE.
- `market`: €/m² median and quartiles of active and recent comparables (same kind, ±35 % area) within 1.5 km, else the
  municipality, else the island (at least 5); `deviationPercent` and estimated value.
- `negotiation`: margin range from seller type, price drops, days on the market, relisting, urgency wording, renovation need,
  premium over comparables and season, capped at 15 %. Not applicable with a Critical red flag.
- `score`: 0–100 weighted average of the available components (price vs market, affordability, negotiation, renovation,
  risk, competition of the source); weights live in the business configuration. Capped at 30 with a Critical red flag.
- `bid`: opening offer, recommended bid (asking price minus the probable margin, rounded down to 500 EUR) and walk-away price
  (the lower of the maximum affordable price and the estimated value plus 5 %).

**Reproducibility.** The analysis is stored with its inputs (listing content hash, profile version, business configuration
and tax rule set ids, renovation level and cost, reference value, Euríbor month, date). The same inputs return the stored analysis (`reused: true`); any change creates a new one.
