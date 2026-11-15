Answers "can I buy this one, and what would it really cost?" for one listing and one financing profile (the default profile
when `financingProfileId` is omitted). Deterministic, rule-based and reproducible.

**What goes into the total cost.** Price + taxes + notary + land registry + gestoria + appraisal (only with a mortgage, the
only mortgage cost left to the borrower by Ley 5/2019) + mortgage opening fee (if the profile has one) + guarantee fee (IBAVI
charges 0.5 % of the guaranteed amount). Renovation is added from phase 3; until then a warning reminds it is missing.

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

**Reproducibility.** The analysis is stored with its inputs (listing content hash, profile version, business configuration
and tax rule set ids, date). The same inputs return the stored analysis (`reused: true`); any change creates a new one.
