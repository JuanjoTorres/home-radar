# ADR-0006 — Perfiles del comprador y reglas fiscales: semilla por fichero, verdad en base de datos

- **Estado:** aceptado (2026-11-14, a petición del propietario)
- **Contexto:** la zona de búsqueda, el presupuesto, la situación fiscal del comprador y si entra obra nueva deben ser parámetros, no constantes. El propietario pide empezar con un fichero de configuración y pasar más adelante a perfiles en base de datos; y que las cifras fiscales no vivan en código sino en una tabla con los datos actuales por comunidad autónoma o provincia.

## Decisión

### 1. Perfiles: fichero semilla desde el primer día, base de datos como única verdad
En lugar de "primero `appsettings`, luego migrar", el fichero es **una semilla idempotente** y la base de datos es la fuente de verdad desde la fase 1:

- `config/seed/financing-profiles.yaml` y `config/seed/search-profiles.yaml` (fuera de `appsettings`, versionados en git salvo los datos sensibles, que van en `config/seed/*.local.yaml` ignorado por git).
- Al arrancar, un `SeedProfilesJob` hace *upsert* por `key` (p. ej. `default`, `zona-inca`): crea el perfil si no existe y lo actualiza solo si el fichero tiene una `revision` mayor que la guardada. Así los cambios por API no se pisan al reiniciar.
- La API de `financing-profiles` y `search-profiles` existe ya en la fase 1 (CRUD básico), de modo que el paso "perfiles en BBDD" no es una migración futura sino el estado normal; el fichero solo evita teclear JSON por API el primer día.
- El `SearchProfile` lleva la zona (municipios, polígono o radio), el rango de precio, `includeNewBuild` y la marca `priority`; el `FinancingProfile`, ahorros, ingresos, hipoteca y los datos del comprador que condicionan impuestos y ayudas (edad, residencia, primera vivienda, familia, discapacidad). Nada de esto se lee de `appsettings` en ningún punto del código.

Motivo de no hacerlo con `appsettings` + `IOptions`: obligaría a duplicar el modelo (POCO de configuración y entidad) y a una migración de datos sin valor; el coste de la semilla es un job de 50 líneas.

### 2. Reglas fiscales: tabla por territorio y fecha de vigencia, sin cifras en código
- Entidad `TaxRuleSet` con clave `(jurisdictionCode, validFrom)` donde `jurisdictionCode` es ISO 3166-2 de la comunidad (`ES-IB`) y, opcionalmente, un `subdivisionCode` (provincia o isla, p. ej. `ES-IB/MALLORCA`), porque en Baleares los límites del tipo reducido ya son **por isla** (Orden 13/2026).
- Cada `TaxRule` es un registro con `ruleId`, `tax` (ITP/IVA/AJD), `appliesTo` (resale/newBuild), `conditions` (JSONB: edad máxima, primera vivienda, residencia, renta, patrimonio, precio máximo…), `rate` o `brackets` (JSONB), `validFrom`, `validTo`, `sourceUrl`, `verifiedOn`.
- El motor (`ITaxRule` como Strategy) **interpreta datos**: lee el `TaxRuleSet` vigente para la jurisdicción de la vivienda (derivada del municipio) en la fecha de cálculo, evalúa condiciones y devuelve la regla aplicada, el porqué y las condiciones no comprobables. Las clases de reglas son genéricas (escala progresiva, tipo fijo con condiciones, bonificación sobre cuota); las cifras están siempre en la tabla.
- Semilla inicial: `config/seed/tax-rules/ES-IB.yaml`, generada a partir del bloque `tax_rules_draft` de `docs/fiscalidad-baleares.md`. Otras comunidades se añaden como ficheros nuevos sin tocar código. Gastos de notaría, registro y tasación entran en la misma tabla (`closingCostRules`) porque el arancel es estatal pero puede cambiar.
- `BusinessConfigVersion` referencia el `TaxRuleSet` usado, de modo que todo `ListingAnalysis` sigue siendo reproducible.

### 3. Obra nueva
Parámetro `includeNewBuild` en el `SearchProfile` y `isNewBuild` por anuncio (detectado por reglas o fijado a mano). El camino IVA + AJD se implementa en la fase 2 junto con el ITP, alimentado por la misma tabla.

## Consecuencias
- Fase 1 incluye el CRUD de perfiles y el job de semilla; fase 2 incluye `TaxRuleSet`, el job de semilla fiscal y una comprobación en CI de que el YAML semilla valida contra el esquema.
- Hace falta un endpoint `GET /v1/tax-rules?jurisdiction=ES-IB&on=2026-11-14` (añadido al borrador OpenAPI en fase 2) para auditar qué reglas aplica el sistema.
