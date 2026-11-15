Audit view of the tax rules the engine applies: the effective tax rule set of every level of `jurisdiction` (default: the
business configuration's jurisdiction, `ES-IB`, which also returns `ES`) on date `on` (today by default).

Each set contains the island value limits (`subdivisions`), the rate, scale and bonus rules with their eligibility conditions
and legal source (`rules`), the notary and land registry tariffs (`fees`) and the aid programmes (`aidPrograms`). Rules marked
`verified: false` are interpretations pending an official criterion and surface as warnings in analyses. Sets are seeded from
`config/seed/tax-rules/*.yaml`, validated on load (a set with errors is rejected and logged) and never edited in place.
