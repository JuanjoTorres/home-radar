# Hipoges (Pollen Street) — realestate.hipoges.com

## 1. Resumen
- **Quién es:** Hipoges Iberia, servicer de crédito e inmuebles (España, Portugal, Grecia). KKR acordó venderlo a Pollen Street en noviembre de 2025; cierre 30-jul-2026 (https://brainsre.news/servicers-inmobiliarios-espana/ , act. 1-oct-2026; https://www.alimarket.es/construccion/noticia_amp/416932/pollen-street-capital-adquiere-la-plataforma-de-gestion-inmobiliaria-y-deuda-hipotecaria-hipoges). En agosto de 2026 Hipoges+Finsolutia compraron Servihabitat (https://www.merca2.es/2026/08/04/hipoges-compra-servihabitat-gigante-60000-millones-2430054/).
- **Dueño en 2026:** Pollen Street Capital (Reino Unido).
- **Portal:** https://realestate.hipoges.com/es (lanzado nov-2023 sustituyendo a Portalnow, >30.000 activos, búsqueda por mapa, campañas mensuales de bajadas de precio, portal inversor; https://forbes.es/ultima-hora/368174/hipoges-lanza-su-nuevo-portal-web-inmobiliario-con-mas-de-30-000-inmuebles/). Blog headless en WordPress: `content.realestate.hipoges.com`.
- **Fecha de verificación:** 2026-11-14.

## 2. Stock en Mallorca
- **Portal propio: no cuantificable** (ver §3). Indicios externos: idealista titula **"Hipoges: 12 casas y pisos en venta en Balears (Illes)"** y "1 local en Palma" (https://www.idealista.com/pro/realestate-hipoges/venta-viviendas/balears-illes/ , fetch bloqueado); housage.es tiene página "Hipogesworks en Palma de Mallorca" (https://housage.es/viviendas-de-bancos/islas-baleares/palma-de-mallorca/hipogesworks/ , error al leer). El blog de Hipoges habla de "amplia cartera de propiedades en Palma" (oct-2024). Estimación: **10–20 viviendas** (no verificado).

## 3. Cómo expone los datos
- **(a) API JSON interna:** **probable pero no identificada.** Todas las rutas (`/es`, `/es/venta/vivienda`, `/es/detail/REGA-03121`, blog `/es/blog/detail/...`) devolvieron **solo `<head>`** con `meta csp-nonce` y GTM-K5H4NDRX: es una **SPA** que carga datos por XHR. El endpoint no se vio (no inventar). `/es` y `/es/venta/vivienda` devolvieron `client_error` en un intento y head-only en otro.
- **(b) XML/sitemap:** no verificado (`/robots.txt`, `/sitemap.xml` no permitidos). Meta robots `index,follow`.
- **(c) HTML:** SPA; sin contenido server-side. Requiere navegador headless o descubrir la API.

## 4. Paginación, filtros, IDs y campos
- **Rutas vistas:** `/es/venta/vivienda`, `/es/investor`, `/es/landing/descuentos-especiales-fin-de-ano`, `/{es|pt|gr}/detail/{REF}`.
- **ID estable:** referencias con prefijo de cartera: `REGA-03121`, `CIRE-01150`, `TWRE-00055` (patrón `XXXX-NNNNN`).
- **Campos:** no verificados (no se pudo renderizar). Según nota de prensa: mapa, geolocalización, servicios cercanos, campañas y descuentos.

## 5. robots.txt, condiciones, antibot
- **robots.txt / aviso legal:** no verificados (sin enlaces en el head; el blog enlaza https://www.hipoges.com/contacto/).
- **Antibot:** CSP con nonce; `client_error` intermitente (posible 403 a agentes no navegador, hipótesis). WebFetch **no obtuvo cuerpo** en ninguna ruta.
- Hipoges vendió >1.800 inmuebles **sin posesión** a inversores profesionales (https://www.bolsamania.com/nota-de-prensa/empresas/hipoges-saca-a-la-venta-mas-de-1800-inmuebles-sin-posesion-para-inversores-profesionales--21053167.html).

## 6. Frecuencia de sondeo propuesta
- No scrapear hasta identificar la API (abrir DevTools en un navegador y observar la petición XHR del listado). Si la API es abierta: 1 sondeo/12 h filtrado por provincia.
- **Alternativas:** página de agencia en idealista (12 viviendas Baleares; idealista bloquea bots), yaencontre (8.223 anuncios, filtro por zona), alertas del portal (no verificado).

## 7. Riesgos y notas
- Mucho producto para inversor, ocupado o sin posesión; descuentos "hasta el 40 %".
- Integración con Servihabitat: el portal podría absorber su stock (positivo para Baleares) o cambiar de dominio.
- Prefijos de referencia revelan la cartera de origen; útil para deduplicar con Servihabitat en el futuro.

## Verificación 2026-11-14

**Presupuesto usado:** 6 búsquedas web + 6 fetches. Sin bypass de antibot.

- **API / app pública: no existe documentación.** Ninguna búsqueda ("realestate.hipoges.com API", "Hipoges app", `site:realestate.hipoges.com`) devuelve documentación de API, SDK, app móvil (App Store/Google Play) ni feed XML. El buscador **no indexa ninguna URL de `realestate.hipoges.com`**: coherente con la SPA head-only vista en §3 (los bots de búsqueda tampoco ven contenido server-side). Las notas de prensa del lanzamiento (nov-2023: forbes.es, que.es, emprendedores.es, essential-business.pt, brainsre.news) no mencionan API, app ni proveedor tecnológico.
- **Pista de proveedor:** existe el documento `https://realestate.hipoges.com/assets/documents/política_de_privacidad.portalnow-finanwin.pdf` (indexado; el fetch devolvió `client_error`). El nombre sugiere que el portal actual sigue apoyándose en la plataforma **Portalnow / Finanwin** (el antecesor del portal era "Portalnow"). Útil para buscar fingerprints del CRM (ver `crm-fingerprints.md`), no verificado.
- **robots.txt:** `https://realestate.hipoges.com/robots.txt` y `https://www.hipoges.com/robots.txt` **no son accesibles desde esta sesión** (fuera de la lista de permitidos). **No se puede afirmar si permite o bloquea las rutas de listado** (`/es/venta/*`, `/es/detail/*`). Pendiente: comprobarlo con `curl` desde la VPS antes de cualquier sondeo.
- **Condiciones de uso** (https://www.hipoges.com/wp-content/uploads/Condiciones-de-uso_SP.pdf, sin fecha, verificado 14-11-2026): cubren solo `www.hipoges.com`, no el portal inmobiliario. Sin cláusula específica de scraping/robots/API; solo (§3) "ningún material publicado en esta web podrá ser reproducido, copiado o publicado" y (§5) prohibición de impedir u obstaculizar el funcionamiento normal del sitio. Versión PT: https://www.hipoges.com/wp-content/uploads/2026/03/Condicoes-de-Uso.pdf.
- **Política de privacidad del portal** (https://content.realestate.hipoges.com/wp-content/uploads/2025/03/PrivacyPolicy_HipogesRE_ES.pdf, sin fecha, verificado 14-11-2026): cubre únicamente `https://realestate.hipoges.com/es`; responsable Hipoges Iberia S.L. (CIF B85610228, Madrid); **no menciona app móvil, API ni alertas**; no nombra encargados tecnológicos. Política de cookies: https://content.realestate.hipoges.com/wp-content/uploads/2025/04/CookiesPolicy_HipogesRE_ES.pdf (no leída).
- **Conclusión:** sin API pública documentada ni app; el listado solo es accesible vía la XHR interna de la SPA (por descubrir con un navegador) y su uso no está regulado explícitamente por las condiciones publicadas. Mantener la frecuencia propuesta en §6 y **no sondear hasta leer `robots.txt` desde la VPS**.
