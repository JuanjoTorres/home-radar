# Solvia (Intrum) — solvia.es

## 1. Resumen
- **Quién es:** Solvia Servicios Inmobiliarios, S.A.U. (CIF A-86744349), c/ Vía de los Poblados 3, Madrid. Servicer nacido de Banco Sabadell y vendido a Intrum en 2019. Desde diciembre de 2024 Intrum unificó Haya Real Estate, Solvia, Aktua y HRE bajo la marca Solvia (fuente: https://brainsre.news/servicers-inmobiliarios-espana/ , actualizado 1-oct-2026; https://www.idealista.com/news/inmobiliario/empresas/2024/12/16/824621-intrum-fusiona-sus-servicers-inmobiliarios-para-simplificar-su-estructura-en-espana).
- **Dueño en 2026:** Intrum (cotizada en Estocolmo). **Atención:** en octubre de 2026 la prensa informó de que Intrum explora la venta de su negocio inmobiliario español con Bain (https://www.placera.se/telegram/intrum-undersoker-forsaljning-av-spanienverksamhet-media-20261019 ; https://es.tradingview.com/news/moodys:2c3063fc495eb:0/). Posible cambio de dueño en 2027 (no verificado).
- **Carteras que comercializa:** Sabadell, CaixaBank (adjudicados recientes, proyecto BEAT), BBVA (ex-Haya/Divarian), Ibercaja, fondos (https://radarpro.ai/blog/solvia-pisos-banco , act. 15-sep-2026). No vende Sareb ni Santander.
- **Portal:** https://www.solvia.es/es/ . Existen hosts de preproducción indexados: `pro2-web.solvia.es`, `pre1-web.solvia.es` (vistos en resultados de búsqueda; WebFetch bloqueado por robots en pro2-web).
- **Fecha de verificación:** 2026-11-14.

## 2. Stock en Mallorca
- **Pisos, provincia Illes Balears: 74** (título y meta de https://www.solvia.es/es/comprar/piso/balears-illes , 4 páginas de ~21 tarjetas). Visto en el HTML.
- **Viviendas, Palma de Mallorca: 64** (4 páginas de 20) en https://www.solvia.es/es/comprar/viviendas/balears-illes/palma-de-mallorca . Rango 68.000 € – 810.000 €; 8 con etiqueta "EN SITUACIÓN ESPECIAL", 2 "Venta flash".
- **Calvià: 1 vivienda** (https://www.solvia.es/es/comprar/viviendas/balears-illes/calvia).
- Hay páginas municipales para Felanitx, Santa Margalida, Inca, Marratxí, Capdepera (vistas en búsqueda, no contadas). Estimación total Baleares (viviendas + garajes + locales): **100–150 anuncios** (hipótesis).

## 3. Cómo expone los datos
- **(a) API JSON interna:** NO observada. En el HTML de listado y detalle no vimos `/api/`, `__NEXT_DATA__`, `__INITIAL_STATE__` ni GraphQL; solo GTM (GTM-MTCN7ZH, GTM-TWXNW64) y rutas Cloudflare `cdn-cgi`. Puede existir una llamada XHR para el buscador por mapa (hipótesis, no verificado).
- **(b) XML/sitemap:** No se pudo leer `/robots.txt` ni `/sitemap.xml` (URL no permitida por WebFetch). El "Mapa Web" HTML (https://www.solvia.es/es/paginas/mapa-de-sitio-120.html) no enlaza sitemap XML ni RSS.
- **(c) HTML:** Las páginas de **listado son server-side** (tarjetas con precio, m², dormitorios, baños, municipio presentes sin JS). Las **fichas de detalle**, en cambio, devolvieron solo head + navegación + footer: el cuerpo (precio, descripción, fotos) parece cargarse por JS (observado en dos fichas). Imágenes en `cdnsolvproep.solvia.es/uploaded/<id>.jpg`.

## 4. Paginación, filtros, IDs y campos
- **Rutas de listado:** `/es/comprar/{tipo}/{provincia-slug}[/{municipio-slug}]` (tipo = `viviendas`, `piso`, `chalet-adosado`, `garajes`, `locales`…; provincia = `balears-illes`). Paginación: 20–21 por página; el enlace a página 2 no aparece como href en el HTML (control JS; parámetro desconocido, no verificado). Orden por precio, m², dormitorios, baños, antigüedad.
- **ID estable:** URL `/es/propiedades/comprar/{tipo}-{municipio}-{N}-dormitorios-{id1}-{id2}`; ej. `piso-palma-de-mallorca-3-dormitorios-168860-207029`. Usar el par `id1-id2` como clave (significado no documentado).
- **Listado:** título, municipio, m², dormitorios, baños, estado (segunda mano/a estrenar), precio, precio anterior tachado y % descuento, badges ("EN SITUACIÓN ESPECIAL", "EN RENTABILIDAD", "Venta flash", "Precio negociable"), nº de fotos.
- **Detalle (meta tags vistos):** m², calle (ej. "C/ Cotlliure"), municipio, og:image. No vistos: coordenadas, referencia catastral, fecha de publicación (sí existe orden "antigüedad", lo que implica fecha interna).

## 5. robots.txt, condiciones, antibot
- **robots.txt:** no se pudo leer (no verificado). Las páginas de listado llevan `meta robots noindex,follow`.
- **Aviso legal** (https://www.solvia.es/es/aviso-legal): sin cláusula explícita de scraping. Relevante: 4.4.2 "se abstendrán de realizar cualquier acto que, directa o indirectamente, suponga una explotación comercial de los mismos"; 4.4.5 "los usuarios se comprometen a no captar datos o contenidos con fines publicitarios"; 5.4 "Cualquier otra utilización … requerirá la autorización expresa y por escrito de Solvia".
- **Antibot:** Cloudflare (rutas `/cdn-cgi/`, email-protection); reCAPTCHA en el formulario de visita. WebFetch **pudo leer** listados y aviso legal sin challenge.

## 6. Frecuencia de sondeo propuesta
- ~8 páginas de listado cubren todo Baleares (viviendas + garajes + locales). **1 sondeo cada 6 h** (≈32 peticiones/día) es suficiente; detectar altas por ID nuevo y bajadas de precio comparando precio tachado.
- **Alternativa legal:** alertas por email del área privada ("Búsquedas Guardadas", https://www.solvia.es/es/avisos) — parsear el correo en lugar de scrapear.

## 7. Riesgos y notas
- "EN SITUACIÓN ESPECIAL" (sin explicación en la web; probablemente ocupado/sin posesión) marca ~12 % del stock de Palma. "EN RENTABILIDAD" = alquilado.
- Datos anómalos (ficha Felanitx con "0.02 m²").
- Precios "no incluyen ningún gasto ni impuesto"; campañas con descuentos 3–19 %.
- Riesgo corporativo: venta de Solvia por Intrum en estudio (oct-2026) → posible rebranding del portal.
