# Inmobiliaria Bancaria (agregador) — inmobiliariabancaria.com

## 1. Resumen
- **Qué es:** agregador de "pisos de bancos" ("© 2008 - 2026 Inmobiliaria Bancaria", info@inmobiliariabancaria.com). No nombra entidad jurídica ni los bancos/servicers de origen (https://www.inmobiliariabancaria.com/pisos-bancos/illes-balears-provincia).
- **Portal:** https://www.inmobiliariabancaria.com ; listado nacional de pisos: https://www.inmobiliariabancaria.com/pisos-venta ("Pisos o apartamentos de bancos ( 10523 )").
- **Fecha de verificación:** 2026-11-14.

## 2. Stock en Mallorca
- **"Pisos en la provincia de Illes Balears ( 123 )"**; 29 municipios con stock; **Palma de Mallorca: 41**, Alaior 2, etc. (la suma por municipio da 119; inconsistencia interna).
- Comprobado leyendo la página de provincia. La página de municipio (https://www.inmobiliariabancaria.com/pisos-bancos/palma-de-mallorca) devolvió un interstitial "One moment, please... Please wait while your request is being verified" → **antibot activo**.

## 3. Cómo expone los datos
- **(a) API JSON:** no observada (sin `/api/`, JSON ni ajax en el HTML leído).
- **(b) XML/sitemap:** enlace "Mapa del sitio" en pie (URL no capturada); `/robots.txt` no leído.
- **(c) HTML:** página de provincia server-side (sólo contadores por municipio); páginas de municipio protegidas por challenge JS.

## 4. Paginación, filtros, IDs y campos
- **Rutas:** `/pisos-bancos/{provincia}-provincia`, `/pisos-bancos/{municipio-slug}`; orden `/index/orden/fecha-desc` (existe **orden por fecha** → señal de actualización).
- **Ficha:** `/{tipo}-bancos/{localidad}/{slug}-{id}.html` (ej. id 909405). **ID estable = numérico final**.
- **Campos:** (no verificados; bloqueado). Banner visto: título, precio, m².

## 5. robots.txt, condiciones, antibot
- **Antibot: sí** (verificación JS tipo "request is being verified" en páginas de municipio). WebFetch sólo pudo leer la página de provincia.
- Aviso legal / condiciones: enlazados en pie sin URL capturada (no leídos).

## 6. Frecuencia de sondeo propuesta
- **No scrapear** (challenge antibot). Alternativa: comprobar manualmente el contador provincial 1 vez/semana; no se observaron RSS ni alertas.

## 7. Riesgos y notas
- Los 123 anuncios son republicaciones de servicers (Solvia, Aliseda, Altamira…) ya cubiertos; valor añadido bajo y origen no trazable.
