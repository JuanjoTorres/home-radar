# Servihabitat (Pollen Street / Hipoges-Finsolutia) — servihabitat.com

## 1. Resumen
- **Quién es:** Servihabitat Servicios Inmobiliarios, S.L.U. (Cornellà de Llobregat). Servicer histórico de La Caixa/CaixaBank; Lone Star lo compró en 2015. Comercializa la cartera histórica de CaixaBank (Coral Homes: Cataluña, C. Valenciana, **Baleares**, Murcia, Andalucía), Kutxabank y Cajasur (https://radarpro.ai/blog/servihabitat-pisos-banco , act. 1-nov-2026; https://www.helpmycash.com/hipotecas/inmobiliarias-de-bancos , act. 9-nov-2026).
- **Dueño en 2026 — SÍ, Pollen Street Capital:** el 4-5 de agosto de 2026 Finsolutia e Hipoges (grupo Pollen Street) acordaron comprar Servihabitat a Lone Star, creando un grupo de ~60.000 M€ bajo gestión (https://www.merca2.es/2026/08/05/pollen-street-compra-servihabitat-2430695/ ; https://www.eldiario.es/economia/fondo-pollen-street-compra-servihabitat-alcanza-60-000-millones-euros-gestion_1_13426058.html ; https://www.alimarket.es/construccion/noticia_amp/429986/el-fondo-britanico-pollen-street-capital--propietario-de-finsolutia-e-hipoges--adquiere-servihabitat). Precio no desvelado; cierre/aprobaciones: no verificado. Se mantiene la marca por ahora (no verificado). En mayo de 2026 ganó con Aliseda la gestión de la cartera Sareb (brainsre.news), lote recurrido por Hipoges (radarpro.ai).
- **Portal:** https://www.servihabitat.com/es/ (según https://spain.estatenearme.com/companies/real-estate-portals/servihabitat y la nota de prensa PDF del propio dominio).
- **Fecha de verificación:** 2026-11-14.

## 2. Stock en Mallorca
- **No se pudo contar en el portal** (WebFetch no permitido sobre servihabitat.com; el buscador no indexa sus páginas de listado).
- Indicios externos: idealista muestra **"Servihabitat: 3 casas y pisos en venta en Balears (Illes)"** (título de https://www.idealista.com/pro/servihabitat2/venta-viviendas/balears-illes/ , fetch bloqueado); yaencontre lista 32 inmuebles de Servihabitat, ninguno en Baleares (https://www.yaencontre.com/inmobiliarias/servihabitat-75878). Un directorio no oficial enumera ciudades baleares (Palma, Calvià, Felanitx, Inca, Sóller) en su ficha (estatenearme). Estimación: **stock balear bajo (3–30 anuncios)** (no verificado).

## 3. Cómo expone los datos
- **(a) API JSON:** no observado (sin acceso).
- **(b) XML/sitemap:** no verificado. La ruta `/documents/15601896/21905509/…pdf/8e20a16d-…` del dominio sigue el patrón de biblioteca de documentos de **Liferay** (inferencia fuerte). Liferay suele servir `/sitemap.xml` por sitio (hipótesis).
- **(c) HTML:** desconocido. Trustpilot y notas de prensa confirman portal activo; radarpro.ai describe filtros por comunidad/provincia/municipio/tipo/precio/superficie y **alertas por email**.
- Única lectura directa lograda: PDF de nota de prensa (feb-2024) en el dominio (campaña de >6.000 activos; sin mención a Baleares).

## 4. Paginación, filtros, IDs y campos
- No verificado. Según radarpro.ai las fichas indican si el inmueble está ocupado, deudas de comunidad conocidas y **fecha de publicación**.

## 5. robots.txt, condiciones, antibot
- **robots.txt / aviso legal:** no leídos (no verificado).
- **Antibot:** no observado directamente. WebFetch **bloqueado** (dominio no permitido en esta sesión), salvo el PDF.
- Trustpilot (1,4/5): https://es.trustpilot.com/review/www.servihabitat.com (reputación de atención al cliente, no técnico).

## 6. Frecuencia de sondeo propuesta
- Hasta poder inspeccionar el portal: **no implementar scraping**. Usar **alertas por email** del portal (confirmadas por radarpro.ai) para Illes Balears y parsearlas.
- Si más adelante se verifica HTML server-side y robots permisivo: 1 sondeo/12 h bastaría dado el stock reducido.

## 7. Riesgos y notas
- Integración con Hipoges/Finsolutia en curso: posible migración del portal a `realestate.hipoges.com` o rebranding (hipótesis).
- Cartera Coral Homes (CaixaBank) con activos antiguos y ocupados; la parte reciente de CaixaBank la vende Solvia (proyecto BEAT).
- Posible stock Sareb a partir de 2026 (gestión adjudicada en mayo, recurrida).
