# ADR-0007 — Entorno de desarrollo sin acceso directo a NuGet ni a registros Docker

- **Estado:** aceptado (2026-11-14)
- **Contexto:** el entorno donde Claude desarrolla solo alcanza GitHub, npm, PyPI y los repositorios de Ubuntu. `api.nuget.org`, `builds.dotnet.microsoft.com`, Docker Hub/ghcr y los portales inmobiliarios están bloqueados. El propietario no puede ampliar la lista de dominios de momento y quiere que todo se haga en local.

## Decisión
1. **SDK:** `dotnet-sdk-10.0` del repositorio de Ubuntu 24.04 (10.0.1xx). `global.json` fija `10.0.100` con `rollForward: latestFeature`.
2. **Paquetes NuGet:** el workflow `.github/workflows/nuget-cache.yml` restaura `build/NuGetCache/NuGetCache.csproj` (que referencia **todos** los paquetes de `Directory.Packages.props`) y publica `~/.nuget/packages` como *asset* de la release técnica `nuget-cache` (prerelease, no es una versión del software). Se regenera automáticamente cuando cambia `Directory.Packages.props`. En el entorno de desarrollo se descarga con la API REST (`gh api … releases/assets/{id}` con `Accept: application/octet-stream`) y se extrae en `~/.nuget/packages`; con versiones exactas (gestión central) `dotnet restore` no necesita red. La auditoría de vulnerabilidades (`NU1900`) solo puede ejecutarse en CI; en local queda como aviso no bloqueante.
3. **Base de datos de tests:** PostgreSQL 16 + PostGIS instalados en el entorno. Los tests de integración usan `HOMERADAR_TEST_CONNECTION` si existe; si no, levantan Testcontainers (CI y máquina del propietario). Misma suite, dos proveedores.
4. **Fixtures reales de las fuentes:** el workflow `record-fixtures.yml` (manual) descarga desde GitHub Actions el HTML crudo de las URL indicadas — leyendo antes `robots.txt` y con el mismo User-Agent del sistema — y lo sube como artefacto/commit bajo `tests/HomeRadar.Sources.ContractTests/Fixtures/<source>/`. Así los adaptadores se escriben contra HTML real sin que el entorno de desarrollo toque los portales.
5. **Playwright:** los navegadores no se pueden descargar en local; los tests que lo necesitan se marcan `[Trait("Category","Browser")]` y solo corren en CI.

## Consecuencias
- Cada cambio de versión de paquete pasa por un push a `main` y una espera de ~3 minutos antes de poder restaurarlo en local.
- Los tests de arquitectura, dominio y aplicación corren en local sin red; los de contrato dependen de fixtures grabados; CI es la fuente de verdad para Playwright y Testcontainers.
