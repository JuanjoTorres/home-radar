# Fase 0 — Resumen (cerrada el 2026-11-14)

## Hecho
- **Fuentes de banca y servicers** verificadas (`docs/sources/*.md`, 21 fichas + 2 rankings). Dueños 2026 confirmados: Solvia = Intrum (fusionó Haya/Aktua; en venta), Servihabitat e Hipoges = Pollen Street, Aliseda/Anticipa = Blackstone, Altamira = doValue, Diglo = Santander. BBVA, Sabadell y CaixaBank no tienen portal REO propio.
- **Catálogo de agencias**: 76 agencias, 62 clasificadas en dos pasadas (`agencies-mallorca.csv`), catálogo de CRM y huellas regex para el detector (`crm-catalog.md`, `crm-fingerprints.md`). Inmovilla 11, a medida 12, WordPress genérico 10, InfoCasa 5, Inmopc/onOffice/RE/MAX 2.
- **Fiscalidad de Baleares** verificada con fuentes primarias (`fiscalidad-baleares.md`): Orden 13/2026 (límites por isla), DA 5.ª del TR, aranceles notarial/registral, adenda ICO 2026, IPREM 2026, Nova Hipoteca Jove de IBAVI y la ayuda directa de 10.000 € (Orden 21/2026, aprobada, hasta 31/12/2027 o agotar 6 M€). YAML `tax_rules_draft` listo para sembrar la tabla fiscal.
- **Datos públicos** reverificados (`public-data.md`): Catastro OVC (`COVCCallejero.svc`, `COVCCoordenadas.svc`, ATOM INSPIRE), valor de referencia **sin API** (exige identificación), Euríbor CSV oficial del BdE (`be1901.csv`, oct-2026 = 3,239 %), capas WMS de IDEIB (APR inundació, sòl rústic ANEI/ARIP, incendis), MITECO, IBESTAT, subastas BOE/AEAT/TGSS.
- **Infraestructura** (`infra/costes-2026-11.md`, ADR-0002): precios oficiales confirmados; **Hetzner CX/CAX sin stock hoy** → plan operativo OVH VPS-1 o netcup VPS 500 ARM; total ≈ 9–14 €/mes con IVA según máquina.
- **Licencias** (`dependencies.md`, ADR-0001): .NET 10 LTS; fuera ImageSharp/ImageHash, NetArchTest, Verify.Xunit, FluentAssertions v8, MediatR/AutoMapper.
- ADR-0001…0006, `docs/sources/README.md` (ranking consolidado) y `docs/api/openapi.v1.draft.yaml` (28 operaciones, 0 errores en Spectral).

## Decisiones del propietario (2026-11-14)
- Zona, presupuesto, situación fiscal y obra nueva: **parámetros**. Se implementa como fichero semilla idempotente + perfiles en base de datos desde la fase 1 (ADR-0006).
- Cifras fiscales **nunca en código**: tabla `TaxRuleSet` por jurisdicción (comunidad/provincia/isla) y fecha de vigencia (ADR-0006).
- Repositorio en `C:\dev\HomeRadar` (máquina del propietario).
- Fuentes de la fase 1 confirmadas: **Solvia + Inmovilla + Aliseda**, más Diglo y alertas por email (ADR-0005).

## Lo que sigue abierto (no bloquea la fase 1)
- Criterio de la ATIB sobre el exceso entre 270.151,20 € y el límite insular con bonificación 100 %; compatibilidad IBAVI–ICO.
- `robots.txt` y cabeceras HTTP de cada fuente: se leerán con el cliente real en la fase 1 (handler de robots).
- Hipoges: SPA sin API documentada; pendiente de DevTools manual.
- 6 agencias bloqueadas y 4 sin web; sufijo `/json/` y campos exactos de `Consulta_DNPRC`.
