Business assumptions that are not law and therefore are not tax rules: the Euríbor 12M reference and the scenarios used for
variable and mixed mortgages, the stress scenario of the verdict, the Borderline tolerances, the usual maximum age at loan
maturity, the renovation contingency and the market estimates of gestoria and appraisal.

They come from `config/seed/business-config.yaml`, loaded into the database as immutable versions keyed by validity date and
revision. Returns the version valid on `on` (today by default): the latest `validFrom` on or before that date, highest
revision. Every stored analysis records which version it used.
