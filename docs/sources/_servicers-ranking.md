# Ranking de servicers para HomeRadar Mallorca

Fecha: 2026-11-14. Puntuación 1–5 por criterio; **Valor = stock × acceso × señales de actualización × (6 − riesgo legal)** normalizado. Cifras de stock = lo observado en los portales o en agregadores (ver ficha de cada fuente).

| # | Servicer (dueño 2026) | Stock Baleares observado | Acceso técnico | Señales de actualización | Riesgo legal / robots | Valor | Justificación breve |
|---|---|---|---|---|---|---|---|
| 1 | **Solvia** (Intrum; en venta) | 74 pisos prov. + 64 viviendas Palma (5) | Listado HTML server-side, ~8 páginas; ficha requiere JS (4) | Orden por antigüedad, precio tachado, badges (4) | Aviso legal sin cláusula de scraping explícita; Cloudflare; robots no leído (2) | **Alto** | Mayor stock balear con diferencia; páginas provinciales y municipales limpias; alertas por email como plan B. |
| 2 | **Aliseda** (Blackstone, incl. Anticipa) | 18 viviendas prov. + garajes/terrenos (3) | HTML server-side con "Cargando" parcial; `?page=N` (4) | % descuento, "Sin posesión", "Alquilado hasta" (3) | Aviso legal prohíbe "extracción y/o reutilización" y reclama derecho sui generis BD (4) | **Medio-alto** | Segundo stock; fichas ricas (descripción, CEE, precio anterior); estado posesorio explícito. |
| 3 | **Diglo** (Santander) | 14 inmuebles, casi todo locales/garajes (2) | Drupal server-side; filtros Views; coordenadas y ref. catastral inline (5) | Orden por fecha de publicación; "Novedades" (4) | Condiciones prohíben extracción/reutilización; robots no leído (4) | **Medio** | Técnicamente el más fácil y completo (catastral, lat/lng), pero stock residencial balear casi nulo hoy. |
| 4 | **Hipoges** (Pollen Street) | ~12 viviendas (idealista) (3) | SPA sin HTML; API no identificada; `client_error` (1) | Campañas mensuales de bajadas (3) | No verificado (3) | **Bajo-medio** | Potencial creciente (absorbe Servihabitat), pero hoy requiere headless o reverse-engineering de API. |
| 5 | **Servihabitat** (Pollen Street desde ago-2026) | ~3 viviendas (idealista); cartera Coral Homes incluye Baleares (2) | No inspeccionable en esta sesión; Liferay (hipótesis) (1) | Fecha de publicación y ocupación en ficha (fuente secundaria) (3) | No verificado (3) | **Bajo-medio** | Usar alertas por email; reevaluar tras integración con Hipoges. |
| 6 | **Altamira** (doValue) | No cuantificable; ≥1 activo (Santa Margalida) (1) | HTML móvil server-side; filtro provincial poco fiable (2) | Campañas "Black Weeks", "Low Cost" (2) | robots bloquea /avisolegal; condiciones no leídas (3) | **Bajo** | Mezcla de provincias en páginas "Baleares"; stock aparentemente residual. |
| 7 | **Casaktua** (Intrum) | 10–30 (locales/suelo mayormente, no verificado) (2) | Bloqueado por robots.txt (0) | Desconocido (1) | robots.txt prohíbe (5) | **Muy bajo** | Solo vía agregadores (tucasa, milanuncios). |
| 8 | **Haya** (Intrum) | 0 (web caída/migrada a Solvia) (0) | robots bloquea (0) | — | — | **Nulo** | Cubierto por Solvia. |
| 9 | **Anticipa / Aktua** | — | — | — | — | **Nulo** | Sin portal propio; cubiertos por Aliseda y Solvia respectivamente. |
| 10 | Finsolutia, Redwood/BCMGlobal, Gescobro, Zolva, Zapp | Sin portal público (no verificado) | — | — | — | **Nulo** | B2B; no implementar. |

## Los 3 a implementar primero
1. **Solvia** — un `SolviaSource` que recorra `/es/comprar/viviendas/balears-illes` (+ `garajes`, `locales`) cada 6 h, clave `id1-id2` de la URL, detecte altas y bajadas de precio; enriquecer ficha con navegador headless solo para nuevos IDs. Registrar también una alerta por email de "Búsquedas Guardadas" como respaldo.
2. **Aliseda** — `AlisedaSource` sobre `/comprar-viviendas/islas-baleares?page=N` cada 12 h; clave `/inmueble/{ref}`; marcar `SinPosesion`/`AlquiladoHasta` desde el listado. Respetar el aviso legal: volumen mínimo, sin republicar contenido, uso personal.
3. **Diglo** — `DigloSource` sobre `/venta-activos/cualquiera/islas-baleares` cada 12 h; aprovechar coordenadas y referencia catastral inline para geocodificar y cruzar con Catastro. Coste casi nulo; sirve de "canario" para cuando Santander suba vivienda balear.

**Siguiente ola:** Hipoges (identificar API XHR desde DevTools, luego reevaluar) y Servihabitat (alertas por email ya; scraping solo tras leer robots/condiciones), ambos del grupo Pollen Street y candidatos a unificar portal en 2027 (hipótesis).

## Advertencias transversales
- Todos los avisos legales leídos (Solvia, Aliseda, Diglo) prohíben la explotación comercial y, en Aliseda/Diglo, la "extracción y/o reutilización" de contenidos. HomeRadar es personal y no republica: mantener volumen bajo, User-Agent identificable, caché y respeto a robots.txt (leerlo desde la app en el primer arranque y abortar si prohíbe la ruta).
- Ningún portal verificado expone sitemap XML de anuncios ni API pública documentada; todo lo anterior es HTML server-side o JS.
- Ocupación / sin posesión: Solvia ("EN SITUACIÓN ESPECIAL"), Aliseda ("Sin posesión", moratoria hasta mayo 2028), Altamira ("Activos ocupados"), Hipoges (>1.800 sin posesión). Modelar un campo `PossessionStatus` desde el día 1.
