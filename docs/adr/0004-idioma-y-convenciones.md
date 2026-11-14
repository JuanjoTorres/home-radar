# ADR-0004 — Idioma, documentación y convenciones de repositorio

- **Estado:** aceptado (fase 0, 2026-11-14)

## Decisión
- **Inglés:** código, identificadores, comentarios XML de contratos e interfaces, OpenAPI, mensajes de commit, `docs/api/*`, `docs/dependencies.md` (tabla) y nombres de ficheros de código.
- **Español:** ADR (`docs/adr/NNNN-titulo.md`), resúmenes de fase, `docs/sources/*`, `docs/fiscalidad-baleares.md`, `docs/infra/*` y toda comunicación con el propietario.
- **Sin comentarios en código** salvo `///` en contratos de API e interfaces.
- **Commits:** Conventional Commits (`feat(sources): add solvia adapter`).
- **ADR:** formato Estado / Contexto / Decisión / Consecuencias; un ADR nuevo supersede, nunca se edita uno aceptado salvo erratas.
- **Fuentes de datos:** cada fuente tiene `docs/sources/<slug>.md` y un `SourceDefinition` en código con la misma clave; el test de contrato del adaptador referencia el fichero de fixtures grabado bajo `tests/HomeRadar.Sources.ContractTests/Fixtures/<slug>/`.
- **Ubicación del repositorio:** pendiente de respuesta del propietario (GitHub o carpeta local). Mientras tanto el trabajo vive en `/home/claude/homeradar` y se entrega como ficheros.
