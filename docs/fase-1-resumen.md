# Fase 1 — Resumen (2026-11-14)

## Hecho
- **Esqueleto .NET 10** (6 proyectos + 5 de tests + CDK), gestión central de paquetes, analizadores como errores, `dotnet format` en CI, tests de arquitectura (ArchUnitNET) que impiden que Dominio/Aplicación dependan de Infraestructura, Fuentes o API.
- **Dominio**: `Listing` con detección de cambios (precio con delta y %, descripción, fotos añadidas/quitadas/sustituidas, características, estado/posesión, retirada y reaparición), `ListingSnapshot` (crudo comprimido en Postgres), `ListingEvent` inmutable, `Source`/`SourceRun` con salud, perfiles, watchlist, outbox, idempotencia, `BusinessConfigVersion` (vacía hasta la fase 2).
- **Esquema PostGIS** con migración inicial; dos geometrías por anuncio (WGS84 para la API, EPSG:25831 para distancias en metros).
- **Pipeline de ingesta**: detectar cambios → enriquecer (`IListingEnricher`, punto de extensión de la fase 5) → emparejar alertas → outbox. Regla de esquema cambiado (0 anuncios tras una buena ejecución o > 20 % incompletos → `Degraded`, nada se retira). Reprocesar un anuncio idéntico no genera eventos.
- **Specification compartida**: el mismo filtro se traduce a SQL y se evalúa en memoria; 11 casos verificados idénticos (incluidos radio y polígono).
- **Adaptadores** con tests de contrato sobre HTML real grabado desde CI (`record-fixtures.yml`, respetando `robots.txt`): Solvia (bisección por precio + ficha con registro completo), Aliseda (JSON embebido, paginado), Diglo (Drupal + ficha), Inmovilla nativo (JSON embebido, `res=48&pag=N`). Diglo y las alertas por email quedan como fuentes 4.ª y 5.ª; el adaptador IMAP está pospuesto.
- **Cliente HTTP** con `robots.txt` (coincidencia por prefijo más largo, `Crawl-delay`, falla cerrado), rate limit por host, User-Agent identificable, reintentos y circuit breaker.
- **Worker**: Quartz (semilla al arrancar, sincronización de planificaciones cada 5 min desde la tabla `sources`, sondeo completo y rápido por fuente, outbox cada 30 s), Telegram (foto + texto corto), semillas YAML idempotentes (ADR-0006).
- **API v1** completa para esta fase y documentada: OpenAPI 3.1 generado en cada build, Spectral sin errores, contrato versionado en `docs/api/openapi.v1.json`, `oasdiff` en PR, Scalar en `/scalar`, `docs/api/README.md`.
- **Infra**: CDK en C# (S3 con ciclo de vida, usuario IAM mínimo, presupuesto), Compose del host, Dockerfiles multi-arch, `deploy.yml` por etiqueta, `backup.sh`.
- **56 tests** en verde en local (CI: build, formato, tests con PostGIS, lint OpenAPI, contrato commiteado).

## Lo que falló o cambió sobre lo previsto
- Verify (snapshots) exige licencia en build desde 2026: sustituido por aserciones directas (ADR-0001).
- `robots.txt` de Solvia prohíbe su API interna: adaptador por HTML con bisección de precio (ADR-0008).
- Sin red en el entorno de desarrollo: caché de NuGet publicada por CI y fixtures grabados por CI (ADR-0007); el worker se probó en local hasta donde la red permite (todas las fuentes fallan cerrado con "robots.txt disallows…" porque ni el robots.txt es alcanzable desde aquí).
- Quartz en memoria en lugar de Postgres (ADR-0008).
- Latencia < 15 min: solo para fuentes `priority` (sondeo rápido de la primera página); el resto 4–12 h.

## Pendiente antes de que funcione en producción (necesita tu parte)
1. Contratar el VPS (OVH VPS-1 o netcup VPS 500 ARM; CAX11 si Hetzner vuelve a tener stock), Docker instalado, `DEPLOY_HOST`/`DEPLOY_USER`/`DEPLOY_SSH_KEY` como secrets del repo.
2. Bot de Telegram y chat ID en `.env` del host.
3. Cuenta AWS: `cdk bootstrap` + `cdk deploy` (con `HOMERADAR_BUDGET_EMAIL`), crear la clave de acceso del usuario `homeradar-host` y ponerla en `.env`.
4. Primera etiqueta `v0.1.0` para desplegar.
5. Tus perfiles reales en `config/seed/*.local.yaml` en el host.

## Propuesta para la fase 2
Motor financiero: tabla `TaxRuleSet` sembrada desde `docs/fiscalidad-baleares.md`, cálculo de coste total, cuota (fija/variable/mixta con escenarios de Euríbor), asequibilidad, precio máximo (bisección), simulación libre y los filtros `financingProfileId`/`budget` en la búsqueda; tests con casos a mano y propiedades (CsCheck).
