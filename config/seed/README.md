# Seed files

Idempotent seeds applied at startup (see ADR-0006). Files ending in `.local.yaml` hold personal data and are git-ignored.
- `financing-profiles.yaml` / `financing-profiles.local.yaml`
- `search-profiles.yaml`
- `tax-rules/ES-IB.yaml` (generated from `docs/fiscalidad-baleares.md`)
