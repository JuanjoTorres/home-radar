# DonComparador (agregador) — doncomparador.com

## 1. Resumen
- **Qué es:** comparador de "pisos y casas de bancos" ("© DonComparador 2012 - 2026"; "comparando entre las ofertas de 30 bancos"). Operador no identificado en la página; crédito a "Proyectízate" (posicionamiento web) (https://www.doncomparador.com/illes-balears/pisos-bancos-en-palma-de-mallorca).
- **Portal:** https://www.doncomparador.com ; Baleares: https://www.doncomparador.com/illes-balears/palma-de-mallorca.
- **Fecha de verificación:** 2026-11-14.

## 2. Stock en Mallorca
- **"Tenemos 44 pisos y casas a la venta en Palma de Mallorca"**; el lateral lista 36 localidades baleares cuyos contadores suman ~**176**; el paginador mostraba "1-12 / 20" (vista filtrada). Comprobado leyendo la página de Palma.
- Las tarjetas **no indican banco/servicer de origen**.

## 3. Cómo expone los datos
- **(a) API JSON:** no observada.
- **(b) XML/sitemap:** "Mapa del sitio" en pie (URL no capturada); `/robots.txt` no leído.
- **(c) HTML:** **server-side** (listado completo legible por WebFetch). Imágenes en subdominio propio; login con búsquedas guardadas.

## 4. Paginación, filtros, IDs y campos
- **Listado:** `/illes-balears/pisos-bancos-en-{municipio}` y `/illes-balears/{municipio}[/{zona}]`; paginación `/index/page/N`; orden `/index/orden/precio-desc` (también **más recientes/antiguos**, cercanía).
- **Ficha:** `/illes-balears/pisos-bancos-en-palma-de-mallorca/piso-palma-de-mallorca-illes-balears-{ID}.html`; favoritos `/productos/viviendas/favoritos/id/{ID}`. **ID estable = numérico = "Ref"**.
- **Campos en listado:** título ("Piso de 67.0 m² en Palma de Mallorca"), Ref, calle, m², hab., baños, €/m², precio, extras, descripción (a veces con datos de arrendamiento: "La renta mensual actual de este inmueble es de 375,39€…").
- Ficha, coordenadas, fechas y ref. catastral (no verificados).

## 5. robots.txt, condiciones, antibot
- **Antibot:** ninguno observado; WebFetch leyó el listado.
- Condiciones generales / aviso legal: enlazados en pie, URL no capturada (no leídos).

## 6. Frecuencia de sondeo propuesta
- **1 sondeo/día** por municipio de interés con orden "más recientes" (1–3 peticiones), sólo como fuente secundaria.
- Alternativa: cuenta con búsquedas guardadas (alertas no verificadas); sin RSS visto.

## 7. Riesgos y notas
- Republica anuncios de servicers (Solvia, Servihabitat, Aliseda…) sin trazabilidad del origen → riesgo de duplicados y de datos desactualizados.
- Varios anuncios son viviendas **arrendadas** (se transmiten con inquilino).
