# ADR-0008 — Decisiones tomadas durante la fase 1

- **Estado:** aceptado (2026-11-14)

| Decisión | Motivo | Reversible |
|---|---|---|
| **Quartz con almacén en memoria**, no en Postgres | Las planificaciones se derivan de la tabla `sources` en cada arranque (`ScheduleSyncJob`), así que persistir triggers no aporta nada; el estado que importa (`LastRunAt`) ya vive en la base de datos. Evita los scripts SQL de Quartz. | Sí: `UsePersistentStore` + scripts, sin tocar los jobs. |
| **Solvia por HTML con bisección por bandas de precio**, no por su API interna | `robots.txt` de solvia.es prohíbe `/api/` y `/ajax/`; las páginas de listado no aceptan parámetro de página pero sí filtros de precio por URL. Las fichas llevan el registro completo embebido en el estado Angular. | Sí: si abren la API, el adaptador cambia solo el enumerador. |
| **Aliseda e Inmovilla desde el JSON embebido en las páginas** | Angular `serverApp-state` (Aliseda) y `listados.paginacion` (Inmovilla) traen datos estructurados completos; más robusto que raspar tarjetas. | — |
| **`FetchedListing.Unchanged`**: los adaptadores pueden declarar un anuncio "visto sin cambios" por huella (`_fingerprint`) | Evita descargar fichas que no han cambiado; el orquestador solo actualiza `LastSeenAt`. | — |
| **ETags fuertes** (`"<xmin>"`) en lugar de débiles | RFC 9110 exige comparación fuerte en `If-Match`; los débiles nunca coincidirían. | — |
| **`Idempotency-Key` hashea el cuerpo ya deserializado** | El `Stream` ya está consumido cuando corre el filtro; serializar el DTO es equivalente y determinista. | — |
| **Snapshots crudos en `listing_snapshots.CompressedBody` (Brotli)** | Según ADR-0003; en Inmovilla/Aliseda el "crudo" es el JSON del anuncio, en Solvia/Diglo el HTML de la ficha. | Sí (`IRawSnapshotStore`). |
| **Sin `EFCore.NamingConventions`**: tablas en snake_case explícitas, columnas en PascalCase | Paquete fuera de la caché; cambiarlo luego requeriría una migración. | Sí, con migración. |
| **`Microsoft.Extensions.*` 10.0.0 y `Microsoft.Extensions.Hosting` 10.0.15** | Versiones disponibles en la caché; `Mvc.Testing` 10.0.15 exige Hosting ≥ 10.0.15. | — |
| **Adaptador de email (IMAP) pospuesto** | Sin buzón ni correos reales de alertas no hay fixtures; MailKit queda referenciado y el puerto `IListingSource` lo admite. | — |
| **Despliegue por etiqueta `vX.Y.Z`**: imágenes multi-arch en GHCR + `docker compose pull` por SSH | Un comando por versión; la CI no necesita acceso a AWS para desplegar la máquina. | — |
