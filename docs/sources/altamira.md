# Altamira (doValue) — altamirainmuebles.com

## 1. Resumen
- **Quién es:** Altamira Asset Management, servicer creado por Santander (2013), hoy parte del grupo doValue (cotizado en Milán; Fortress y Bain Capital como accionistas de referencia). Santander vendió su 15 % de doValue (https://www.alimarket.es/construccion/noticia_amp/379106/banco-santander-se-desprende-del-15--que-mantenia-en-dovalue). Mapa 2026: https://brainsre.news/servicers-inmobiliarios-espana/ (act. 1-oct-2026); doValue perdió la puja por Servihabitat en agosto de 2026.
- **Marca:** el portal sigue como "Altamira Inmuebles"; el enlace "Quiénes somos" apunta a dovalue.es (visto en el footer móvil).
- **Portal:** https://www.altamirainmuebles.com (versión móvil en `/m/…`).
- **Fecha de verificación:** 2026-11-14.

## 2. Stock en Mallorca
- **No se pudo cuantificar.** Las páginas tituladas "…en Baleares" (Felanitx, Alcudia, Sa Pobla, locales Baleares, obra nueva Baleares) devolvieron o bien solo `<head>` o bien tarjetas de **otras provincias** (Barcelona, Tarragona, Alicante) con un único activo balear (piso en Santa Margalida, ref `9186-2599-pe0001/376419`). Hipótesis: sin JS/cookies el filtro provincial del `q=` no se aplica y el servidor muestra "similares"; o el stock balear es casi nulo. Google indexa páginas municipales de Palma, Alcudia, Felanitx, Marratxí, Sa Pobla (fuente: búsqueda `site:altamirainmuebles.com Baleares`).
- Existe una promoción en Palma ("Promoción La Veleta – Palma de Mallorca", id 797) indexada. Estimación: **<20 anuncios** (no verificado).

## 3. Cómo expone los datos
- **(a) API JSON interna:** no vista, pero la URL de promociones incluye el **estado del buscador como JSON en base64** (visto en búsqueda): `{"campana":-1,"estado":0,"provincia":7,"poblacion":1107040,"filtro":{"precioMax":1000000,"precioMin":0,"superficieMinima":0,"conFoto":false,…},"orden":1,"coordenadas":{"longitud":3.054,"latitud":39.778,"zoom":7},"superTipo":1,"tipoOperacion":1,"tipoInmuebleResidencial":3,"urlVuelta":"q=-1_-1_1_3_7_1107040_-1_-1&urlCanonical=venta/garajes-banco/baleares/palma-de-mallorca"}`. Esto confirma un backend con filtro estructurado; el endpoint que lo consume **no se identificó** (no inventar).
- **(b) XML/sitemap:** `/robots.txt` y `/sitemap.xml` no leídos (URL no permitida). No verificado.
- **(c) HTML:** la versión **móvil `/m/`** devolvió 15 tarjetas server-side (precio, m², hab., municipio). La versión escritorio devolvió a veces solo `<head>` (posible render JS o respuesta condicionada). Solo GTM (GTM-W98TSH) como script visible.

## 4. Paginación, filtros, IDs y campos
- **Rutas de listado:** `/{venta|compra|alquiler}/{pisos-y-casas|garajes|trasteros|locales|suelos}-banco/{provincia}[/{municipio}][/obra-nueva]/q={campana}_{estado}_{tipoOperacion}_{superTipo}_{provincia}_{poblacion}_{?}_{?}`. Ej.: `q=-1_-1_1_1_7_1107022_-1_-1` = venta, residencial, provincia 7 (Baleares), población 1107022 (Felanitx). Códigos vistos: Palma 1107040, Alcudia 1107003, Felanitx 1107022, Marratxí 1107036, Sa Pobla 1107044. Paginación no visible en HTML (no verificado).
- **ID estable:** `/venta-de-{piso|duplex|local-comercial}/{prov}/{mun}/{segunda-mano|obra-nueva}/{codigo}/{listingId}/1`, ej. `/venta-de-piso/baleares/santa-margalida/segunda-mano/9186-2599-pe0001/376419/1`. Promociones: `/{prov}/{mun}/{slug}/{id}/1`.
- **Listado (móvil):** precio, m², hab., baños, municipio, fotos, etiquetas "Condiciones especiales", "CESIÓN DE REMATE", "Impuestos no incluidos". Detalle: no verificado.

## 5. robots.txt, condiciones, antibot
- **robots.txt:** no leído, pero WebFetch devolvió `robots_disallowed` al pedir `/avisolegal` → el robots.txt **bloquea rutas** a agentes genéricos (al menos `/avisolegal`); las rutas de listado sí fueron servidas.
- **Aviso legal:** https://www.altamirainmuebles.com/avisolegal — **no se pudo leer** (bloqueado por robots). Cláusulas de scraping: no verificado.
- **Antibot:** ninguno evidente; respuestas inconsistentes (head-only) sugieren render JS o protección ligera (hipótesis).

## 6. Frecuencia de sondeo propuesta
- Hasta confirmar stock y comportamiento del filtro, **1 sondeo diario** sobre la ruta móvil provincial de viviendas; validar que cada tarjeta tenga `/baleares/` en la URL antes de aceptarla.
- **Alternativa:** campañas periódicas ("Low Cost Residencial", "2026 Black Weeks", "2026 Ocupados Residencial"); área de usuario con favoritos (alertas por email no verificadas).

## 7. Riesgos y notas
- Sección explícita de **"Activos ocupados"** y campaña "2026 Ocupados Residencial": filtrar.
- "Cesión de remate" (adjudicación judicial pendiente) y "Condiciones especiales" sin explicación.
- Mezcla de provincias en páginas "Baleares": riesgo de falsos positivos si se confía en el título.
- doValue como marca corporativa; posible rebranding del portal (no verificado).
