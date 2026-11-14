# Aliseda (Blackstone) — alisedainmobiliaria.com

## 1. Resumen
- **Quién es:** Aliseda Servicios de Gestión Inmobiliaria, S.L.U. (CIF B-86875689), Avda. de Manoteras 12, Madrid. Brazo de venta de activos del grupo Blackstone en España; Anticipa es la hermana que gestiona crédito hipotecario (https://brainsre.news/servicers-inmobiliarios-espana/ , act. 1-oct-2026).
- **Dueño en 2026:** Blackstone (no aparece en la web; fuente: brainsre.news). En mayo de 2026 Aliseda y Servihabitat ganaron la gestión de la cartera de Sareb de 4.860 M€ (misma fuente). El footer enlaza "CCGG Sareb" y "CCGG SMO II".
- **Portal:** https://www.alisedainmobiliaria.com (es/en/fr). Es también la salida comercial de Anticipa (https://anticipa.com enlaza a Aliseda).
- **Fecha de verificación:** 2026-11-14.

## 2. Stock en Mallorca
- **Viviendas, Islas Baleares: 18** ("18 Viviendas en venta", 2 páginas de 12) en https://www.alisedainmobiliaria.com/comprar-viviendas/islas-baleares . Municipios vistos: Llucmajor, Andratx, Palma.
- **Palma: 3 viviendas** (https://www.alisedainmobiliaria.com/comprar-viviendas/islas-baleares/palma): 176.035 € (-9 %), 125.000 €, 232.900 €.
- Existen páginas de garajes, terrenos y "land" para Baleares (no contadas). Estimación total **25–40 anuncios** (hipótesis).

## 3. Cómo expone los datos
- **(a) API JSON interna:** NO observada en el texto renderizado. Indicadores de SPA parcial: literal "Cargando" al final de listado y ficha; iconos Material Icons. Los parámetros de orden (`sort_by=field_prom_prec&sort_order=ASC&page=2`) tienen nombre de campo tipo Drupal/Views, lo que sugiere backend Drupal con render server-side + hidratación (hipótesis). No vimos `drupalSettings`, `__NEXT_DATA__` ni GraphQL.
- **(b) XML/sitemap:** `/robots.txt` y `/sitemap.xml` no pudieron leerse (URL no permitida). No verificado.
- **(c) HTML:** Listado y ficha devuelven contenido útil sin JS (precio, m², habitaciones, baños, municipio, descripción). Imágenes en `storage.googleapis.com/aliseda/ImagenesActivos/Espana/Islas_Baleares/Baleares/{ref}/{ref}-A1.jpg`.

## 4. Paginación, filtros, IDs y campos
- **Rutas:** `/comprar-viviendas/{provincia-slug}[/{municipio}]`, `/comprar-viviendas/casas/{prov}`, `/comprar-garajes/{prov}`, `/comprar-terrenos/{prov}`; provincia = `islas-baleares`. Query: `?page=N&sort_by=field_prom_prec&sort_order=ASC`. 12 tarjetas/página.
- **ID estable:** `/inmueble/{ref}`; ref numérica (`52717053`, `52890793`) o con prefijo Anticipa (`ant00020715107`).
- **Listado:** tipo, calle/zona, municipio, provincia, m², hab., baños, precio, precio anterior + % descuento, etiquetas de campaña, "Alquilado hasta MM/AAAA", "Sin posesión".
- **Detalle (ficha 52890793):** precio 232.900 € junto a 274.000 €, 122 m², calle y CP 07004, municipio, "Ver en mapa" (coordenadas no visibles en texto), descripción, ref., certificado energético ("En trámite"), estado "Sin posesión" con nota de moratoria de desahucio hasta mayo 2028, calculadora hipoteca. No vistos: referencia catastral, fecha de publicación.

## 5. robots.txt, condiciones, antibot
- **robots.txt:** no verificado.
- **Aviso legal** (https://www.alisedainmobiliaria.com/sobre-aliseda/aviso-legal): "queda expresamente prohibido al Usuario la reproducción, transformación, distribución, comunicación pública, puesta a disposición, extracción y/o reutilización del Sitio Web, sus Contenidos…"; reclama "el derecho sui generis sobre las bases de datos subyacentes"; "no realizar acto alguno con objeto de dañar, inutilizar o sobrecargar el Sitio Web"; Aliseda "se reserva el derecho a limitar, suspender o terminar su acceso". Prohíbe enlazar sin autorización. → Riesgo legal medio-alto para extracción sistemática.
- **Antibot:** ninguno observado; WebFetch **leyó** listado, ficha y aviso legal.

## 6. Frecuencia de sondeo propuesta
- Stock balear pequeño (2 páginas). **1 sondeo cada 12 h** sobre `/comprar-viviendas/islas-baleares` (+ garajes/terrenos si interesa). Sin fecha de publicación visible: detectar novedades por ref. nueva.
- **Alternativa:** alertas de búsqueda por email del portal (requiere cuenta; no verificado que existan para Baleares), campañas "Descuentos como una casa".

## 7. Riesgos y notas
- Alta proporción de activos **"Sin posesión"** o **"Alquilado hasta 2070/2099"** (fechas placeholder). Las fotos pueden no reflejar el estado actual; visita interior imposible.
- Financiación propia anunciada ("financiación muy competitiva", simulador).
- Un precio de 2.035 €/mes (-4 %) apareció como primera tarjeta en venta: posible error de datos o alquiler mezclado.
