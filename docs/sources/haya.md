# Haya Real Estate (absorbida por Solvia/Intrum) — haya.es

## 1. Resumen
- **Quién es:** Haya Real Estate (ex-Cerberus), comprada por Intrum en 2023 (https://www.franquiciasaldia.es/noticias/2023-09-26/franquicia-solvia-intrum-completa-la-adquisicion-de-haya-real-estate/). En diciembre de 2024 Intrum fusionó Haya, Solvia, Aktua y HRE bajo la marca **Solvia Servicios Inmobiliarios** (https://brainsre.news/intrum-fusiona-inmobiliarias-haya-solvia-aktua-hre/ ; https://observatorioinmobiliario.es/noticias/inversion-inmobiliaria/intrum-reorganiza-su-estructura-en-espa%C3%B1a-bajo-la-marca-solvia-servicios-inmobiliarios/).
- **¿Sigue siendo marca en 2026?** **Residual.** La web haya.es devolvió error de servidor y sin inmuebles el 15 y 28 de septiembre de 2026 según https://radarpro.ai/blog/haya-inmobiliaria-bbva-donde-estan-pisos (29-sep-2026); los activos BBVA/Divarian aparecen en Solvia ("Inmuebles de BBVA en venta", 493 anuncios el 28-sep-2026). Similarweb aún estima tráfico (46,8 K visitas ago–oct 2026) y describe el sitio con el eslogan de Solvia (https://www.similarweb.com/company/haya.es/). Google indexa todavía rutas `haya.es/comprar/viviendas/baleares/`.
- **Dueño:** Intrum (en proceso de posible venta, oct-2026).
- **Fecha de verificación:** 2026-11-14.

## 2. Stock en Mallorca
- **No verificable**: WebFetch devolvió `robots_disallowed` para https://www.haya.es/comprar/viviendas/baleares/ . Rutas indexadas: `/comprar/baleares/?p=2`, `/comprar/viviendas/baleares/palma/`, `/cajamar/baleares`, `/caixabank/baleares/`, `/comprar/garajes-trasteros/baleares/`, `/comprar/locales-comerciales/baleares/`. Se asume que el stock ha migrado a solvia.es (hipótesis respaldada por radarpro.ai).

## 3. Cómo expone los datos
- **(a) API JSON:** no observado.
- **(b) XML/sitemap:** no verificado.
- **(c) HTML:** no verificado. Una URL indexada muestra parámetros de buscador server-side: `?pagination_random_token=…&asset_selling_mode_id=1&microsite_asset_type_id=16&geo_province_id=7&is_sell_cache=1&order_by[0]=order_relevance+DESC&order_by[1]=haya_asset_identifier+ASC` (provincia 7 = Baleares; `haya_asset_identifier` = ID interno). Sugiere formularios GET clásicos (hipótesis).

## 4. Paginación, filtros, IDs y campos
- Paginación `?p=N`; filtros `geo_province_id`, `microsite_asset_type_id`, `asset_selling_mode_id`; microsites por banco (`/caixabank/`, `/cajamar/`). Campos y ID de ficha: no verificados.

## 5. robots.txt, condiciones, antibot
- **robots.txt:** bloquea agentes genéricos en `/comprar/…` (observado vía `robots_disallowed`). Reglas exactas no leídas.
- **Condiciones de uso:** no leídas.
- **Antibot:** no observado.

## 6. Frecuencia de sondeo propuesta
- **No implementar.** Cubrir vía Solvia (sección BBVA y Cajamar) y revisar trimestralmente si haya.es redirige a solvia.es.

## 7. Riesgos y notas
- Dominio en desmantelamiento: errores de servidor y posible redirección. Cualquier integración quedaría obsoleta.
- Los activos ex-BBVA son de Cerberus/Divarian; negociación con Solvia, no con el banco.
