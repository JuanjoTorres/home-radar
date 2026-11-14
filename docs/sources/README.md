# Inventario de fuentes — HomeRadar Mallorca (fase 0, 2026-11-14)

Método: WebSearch + WebFetch sin navegador, sin evadir robots/antibot ni iniciar sesión. Las cabeceras HTTP (ETag/Last-Modified) y varios `robots.txt` **no pudieron verificarse** desde esta sesión; quedan marcados para comprobar con el cliente HTTP real en la fase 1.

## Ranking consolidado (todas las familias)

| # | Fuente | Tipo | Stock Mallorca/Baleares | Acceso | Riesgo legal | Fase | Ficha |
|---|---|---|---|---|---|---|---|
| 1 | Solvia (Intrum) | Servicer | ≈ 74 pisos prov., 64 viv. Palma | HTML SSR + ficha JS | Medio (solo prohíbe uso comercial) | 1 | `solvia.md` |
| 2 | Inmovilla (CRM) | Agencias | 11/62 webs clasificadas; mayor cuota | HTML SSR, ID global 8 dígitos | Bajo-medio (por agencia) | 1 | `crm-catalog.md` |
| 3 | Aliseda (Blackstone) | Servicer | ≈ 18 viviendas | HTML SSR `?page=N` | Alto (prohíbe extracción) | 1 | `aliseda.md` |
| 4 | Diglo (Santander) | Banco | 14 activos (terciario) | Drupal SSR, catastral + lat/lng | Alto (prohíbe extracción) | 1 (canario) | `diglo.md` |
| 5 | Alertas por email (Unicaja, Servihabitat, Solvia) | Plan B | — | IMAP | Nulo | 1 | — |
| 6 | Kyero V3 feed | Agencias | Witei, Inmobalia, Resales… | XML con `id` + `date` | Bajo | 4 | `crm-catalog.md` |
| 7 | InfoCasa (CRM local) | Agencias | 5/62, patrón idéntico | HTML SSR (MODX) | Bajo | 4 | `crm-catalog.md` |
| 8 | WordPress genérico / onOffice / Mobilia | Agencias | 10 + 2 + 1 /62 | `wp-json` / HTML / API REST | Bajo | 4 | `crm-catalog.md` |
| 9 | Hipoges (Pollen Street, absorbe Servihabitat) | Servicer | ≈ 12 viviendas | SPA, API no identificada | No verificado | 4 | `hipoges.md` |
| 10 | Banca March | Banco | 3–4 locales | Catálogo HTML 1 URL | Bajo-medio | 4 | `banca-march.md` |
| 11 | DonComparador | Agregador | ≈ 176 (republicados) | HTML SSR | No leído | 4 (redundancia) | `doncomparador.md` |
| 12 | Servihabitat | Servicer | ≈ 3 | No inspeccionable | No verificado | email | `servihabitat.md` |
| 13 | Altamira (doValue) | Servicer | residual | HTML móvil | Medio | — | `altamira.md` |
| — | Casaktua, Haya, Aktua, Anticipa, Bankinter, Unicaja, Cimenta2, Ibercaja, Kutxabank, Colonya, Facilitea, Inmobiliaria Bancaria, Inmobanco | — | 0 / bloqueado / cubierto por otro | — | — | descartado | ver `_servicers-ranking.md`, `_banks-ranking.md` |

## Ficheros
- Servicers: `altamira.md`, `solvia.md`, `servihabitat.md`, `aliseda.md`, `anticipa.md`, `hipoges.md`, `diglo.md`, `casaktua.md`, `haya.md`, `aktua.md`, `otros-servicers.md`, ranking en `_servicers-ranking.md`.
- Bancos y agregadores: `banca-march.md`, `unicaja-inmuebles.md`, `escogecasa.md`, `cimenta2.md`, `bankinter.md`, `colonya.md`, `faciliteacasa.md`, `doncomparador.md`, `inmobiliariabancaria.md`, `inmobanco.md`, ranking en `_banks-ranking.md`.
- Agencias: `crm-catalog.md` (CRM, feeds, ranking por cobertura), `crm-fingerprints.md` (huellas regex para el detector, dos pasadas), `agencies-mallorca.csv` (76 agencias: 62 clasificadas, 6 bloqueadas, 4 sin web, 4 sin localizar).
- Datos públicos: `public-data.md` (Catastro, valor de referencia, Euríbor, estadísticas, GIS de riesgos, subastas), reverificado en línea el 2026-11-14; lo que sigue abierto está marcado NO VERIFICADO.

## Pendiente de verificar en fase 1 (con el cliente HTTP real)
1. `robots.txt` y cabeceras condicionales de Solvia, Aliseda, Diglo, Banca March y de cada web Inmovilla.
2. API XHR de Hipoges (con DevTools en el navegador del propietario, sin automatizar).
3. Reintentar las 6 agencias bloqueadas y localizar las 4 sin web.
4. Sufijo `/json/` y campos exactos de `Consulta_DNPRC` del Catastro (verificar con el cliente real).
