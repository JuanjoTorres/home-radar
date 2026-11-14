# Diglo (Banco Santander) — digloservicer.com

## 1. Resumen
- **Quién es:** Diglo Servicer Company 2021, S.L. (CIF B-67915298), c/ Josefa Valcárcel 34, Madrid. Servicer propio de Banco Santander creado en 2022 (https://www.idealista.com/news/inmobiliario/empresas/2022/05/30/796903-diglo-el-nuevo-servicer-de-santander-para-vender-viviendas-usadas-baratas).
- **Dueño en 2026:** Banco Santander (vía Deva Capital según https://brainsre.news/servicers-inmobiliarios-espana/ ; Santander decidió no venderlo y reforzarlo en 2025). Desde abril de 2026 gestiona la venta de los adjudicados del banco (https://radarpro.ai/blog/diglo-servicer-santander , act. 27-sep-2026).
- **Portal:** https://digloservicer.com (buscador canónico `/buscador`). **Ojo:** `www.diglo.com` es una tienda de audífonos de EE. UU., sin relación.
- **Fecha de verificación:** 2026-11-14.

## 2. Stock en Mallorca
- **Islas Baleares, todos los tipos: 14 inmuebles** ("14 inmuebles de banco en venta en la provincia de Islas Baleares", https://digloservicer.com/venta-activos/cualquiera/islas-baleares). Vistos: locales en Manacor (122 m², 295.000 €) y Palma (137 m², 195.000 €), 4 garajes en Andratx (13 m², 12.000 €), 1 piso en Palma (ref IB00100024192) en el listado nacional.
- Página de locales Baleares: https://digloservicer.com/venta-locales/islas-baleares . Stock residencial balear: **muy escaso (1–3 viviendas)**.

## 3. Cómo expone los datos
- **(a) API JSON interna:** NO observada. Rutas `/sites/default/files`, `modules/custom/yera_custom_login`, tema `gavias_lozin` → **Drupal** (inferencia). Los filtros son parámetros de Views de Drupal, no una API JSON. Coordenadas lat/lng aparecen **inline en el texto** de tarjetas y fichas (ej. `39.539632321743 2.4186641373954`).
- **(b) XML/sitemap:** `/robots.txt` y `/sitemap.xml` no leídos (URL no permitida). Drupal suele exponer `/sitemap.xml` (hipótesis, no verificado).
- **(c) HTML:** server-side; listado y ficha completos sin JS. Imágenes en `storage.googleapis.com/gcs-diglo-web-contents-pd/imagenes/activos/Espana/Islas_Baleares/Baleares/{REF}/{REF}-{A|B|C}.jpg`.

## 4. Paginación, filtros, IDs y campos
- **Rutas:** `/venta-pisos[/{provincia}[/{municipio}]]`, `/venta-locales/{prov}`, `/venta-garajes/{prov}/{mun}`, `/venta-activos/cualquiera/{prov}`, `/alquiler-pisos/…`; provincia = `islas-baleares`.
- **Query (Drupal Views):** `field_yera_categoria_target_id` (95 = venta, 96 = alquiler), `field_yera_tipo_producto_target_id`, `field_yera_precio_value[min|max]`, `sort_by`, `sort_order`, `page=N` (0-based; "Load More"). Orden disponible por precio, m², descuento y **fecha de publicación** (dato interno existe, no se muestra).
- **ID estable:** `/venta-{tipo}/{prov}/{mun}/{ref}`; refs `IB00100041913`, `RET0000206481`, `GCO0000199872`, `CRD…`, `0049IB…`, numéricas.
- **Listado:** tipo, dirección, municipio, precio o "Precio a consultar", m² (en algunos), hab./baños (pisos), coordenadas.
- **Detalle (IB00100041913):** "Precio de salida", m² construidos, dirección, municipio, coordenadas, 3–4 fotos, descripción, **referencia catastral** (0170802DD5707S0035FE), simulador hipotecario, botón "Hacer una oferta". No vistos: fecha de publicación, certificado energético.

## 5. robots.txt, condiciones, antibot
- **robots.txt:** no verificado. Meta robots `index, follow`.
- **Condiciones de uso** (https://digloservicer.com/aviso-legal#aviso-legal-registro): "queda expresamente prohibido al Usuario la reproducción, transformación, distribución … extracción y/o reutilización del Sitio Web, sus Contenidos"; reclama "el derecho sui generis sobre las bases de datos subyacentes"; prohíbe "dañar, inutilizar o sobrecargar el Sitio Web" y enlazar sin autorización. Sin mención explícita a robots/scraping.
- **Antibot:** ninguno observado; WebFetch **leyó** listado, ficha y aviso legal. Existe host `integracion.digloservicer.com` (entorno de integración indexado).

## 6. Frecuencia de sondeo propuesta
- 1 petición cubre Baleares. **1 sondeo cada 12 h** a `/venta-activos/cualquiera/islas-baleares`. Dado el stock residencial casi nulo, prioridad baja.
- **Alternativa:** perfil de búsqueda + alertas por email (recomendado por radarpro.ai); sección "Novedades recién publicadas" y "Súper Descuentos".

## 7. Riesgos y notas
- Muchos activos "Precio a consultar"; modalidad "El precio lo pones tú" (puja de 12 días sobre precio de salida).
- Financiación Santander "hasta el 90 %"; precios sin gastos ni impuestos; "Gastos e impuestos no incluidos".
- No se indica ocupación en las fichas vistas; la visita solo "si el inmueble está libre" (radarpro.ai).
