# ADR-0003 — Arquitectura y patrones

- **Estado:** aceptado (fase 0, 2026-11-14)

## Estilo
Monolito modular con Clean Architecture y vertical slices por feature. Dos ejecutables (`Api`, `Worker`) que comparten `Domain`, `Application`, `Infrastructure` y `Sources`. Se mantienen separados para que un fallo de Playwright o un pico de memoria del worker no tire la API.

```
src/HomeRadar.Domain | .Application | .Infrastructure | .Sources | .Api | .Worker
tests/ ...Domain.Tests | ...Application.Tests | ...Sources.ContractTests | ...Api.IntegrationTests | ...ArchitectureTests
```

## Patrones adoptados (y los matices respecto a la propuesta inicial)

| Patrón | Dónde | Decisión |
|---|---|---|
| Ports & Adapters | `Application` define `IListingSource`, `IRawSnapshotStore`, `IPhotoStorage`, `ICadastreClient`, `INotificationChannel`, `IEmailInbox`, `IListingEnricher` | Sin cambios. `IListingEnricher` existe desde la fase 1 con una implementación `RulesEnricher`; la LLM entrará ahí. |
| Adapter | Uno por servicer, por CRM y por formato de feed; más `EmailAlertSource` (IMAP) | **Ampliado:** el email es un adaptador normal, no un apaño. |
| Registry + keyed services | `SourceAdapterRegistry` con `[FromKeyedServices]` | Sin cambios. La clave es `Source.AdapterKey` (p.ej. `solvia`, `inmovilla`, `kyero-feed`). |
| **Composición en lugar de Template Method** | `SourceAdapter` sellado compuesto por `IPageEnumerator`, `IListingExtractor`, `IListingNormalizer` | **Cambio:** respeta "sealed por defecto" y evita jerarquías con `protected virtual`. |
| **Pipeline** (no Chain of Responsibility) | Ingesta: `Normalize → DetectChanges → Enrich → Deduplicate → Analyze → Match → Notify`, cada etapa `IIngestionStage` con orden explícito y todas se ejecutan | **Cambio de nombre:** CoR se detiene en el primer manejador; aquí todas las etapas corren. |
| Strategy | `ITaxRule`, `IRenovationCostEstimator`, `IScoreComponent`, `IMortgagePaymentCalculator` por tipo de hipoteca | Sin cambios. Las reglas fiscales se cargan desde `BusinessConfigVersion` (datos), no como clases por tramo. |
| Specification | `ListingSpecification` con `Expression<Func<Listing,bool>>`; se traduce a EF Core y se compila para evaluar en memoria. Los filtros geográficos usan NetTopologySuite en ambos lados | Sin cambios. Test obligatorio: para cada spec, EF y memoria dan el mismo resultado sobre el mismo conjunto. |
| Domain events + Transactional Outbox | `ListingEvent` y `OutboxMessage` en la misma transacción; `OutboxDispatcher` en el worker con bloqueo `FOR UPDATE SKIP LOCKED` | Sin cambios. |
| Append-only change log | `Listing` (estado) + `ListingSnapshot` + `ListingEvent` inmutables | Sin cambios. `Relisted` en fase 1 solo para misma fuente y mismo `ExternalId`; con otro ID depende de `Property` (fase 4). |
| CQRS ligero | Comandos con tracking; lecturas `AsNoTracking` + proyecciones | **Sin Dapper** hasta que haga falta. |
| Value Objects | `Money` (decimal, EUR), `Area`, `PricePerSquareMeter`, `CadastralReference`, `InterestRate`, `Percentage`, `PossessionStatus` | **Añadido** `PossessionStatus` (ocupado / sin posesión / alquilado hasta): aparece explícitamente en Solvia, Aliseda, Altamira e Hipoges. |
| Result pattern | Errores de negocio esperados → `ProblemDetails` con `type` propio | Sin cambios. |
| Decorator (`DelegatingHandler`) | Rate limiting por dominio, `robots.txt` (se lee y cachea; aborta si prohíbe la ruta), caché condicional, métricas, User-Agent identificable | **Añadido** el handler de `robots.txt`. |
| Options pattern | Solo configuración técnica | Sin cambios. Configuración de negocio en `BusinessConfigVersion`. |

## Decisiones adicionales
1. **Snapshots crudos en Postgres** (`ListingSnapshot.Body bytea` Brotli) detrás de `IRawSnapshotStore`; migrar a S3 cuando la tabla supere ~5 GB.
2. **Fotos:** diff por URL y posición en cada sondeo; descarga y hash perceptual solo de URLs nuevas o reposicionadas. `PhotoReplaced` se detecta cuando una URL nueva ocupa la posición de una antigua y la distancia de Hamming del hash es alta.
3. **Entidades añadidas:** `SourceRun` (una fila por ejecución: inicio, fin, anuncios vistos, nuevos, cambiados, errores) e `IdempotencyKey` (clave, hash del cuerpo, respuesta, caducidad 24 h).
4. **Regla de "esquema cambiado":** si una ejecución devuelve 0 anuncios donde la anterior devolvió > 0, o > 20 % de anuncios sin precio o sin URL, la ejecución se marca `Degraded`, no se persiste nada y se notifica por Telegram.
5. **`ETag`/`If-Match` solo en recursos editables** (`financing-profiles`, `search-profiles`, `watchlist`, `renovation-override`), no en `listings`.
6. **Datos financieros:** disco cifrado en el host, nunca en logs (filtro de Serilog por nombre de propiedad), sin cifrado por columna.
7. **Valor de referencia del Catastro:** no hay API pública (exige Cl@ve); `FinancingProfile`/`ListingAnalysis` lo aceptan como dato manual opcional y el motor fiscal avisa (`warnings[]`) cuando calcula la base solo con el precio.
