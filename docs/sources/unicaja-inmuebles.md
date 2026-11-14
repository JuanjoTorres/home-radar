# Unicaja Inmuebles (GIA) — unicajainmuebles.com

## 1. Resumen
- **Entidad:** Unicaja Banco S.A. ("titular de la página web", aviso de cookies). Operador: **Gestión de Inmuebles Adquiridos, S.L. (GIA)**, "© Gestión de Inmuebles Adquiridos, S.L. 2026" (https://unicajainmuebles.com/inicio.do). Portal propio del grupo (https://www.helpmycash.com/hipotecas/inmobiliarias-de-bancos, act. 9-nov-2026).
- **Portal:** https://unicajainmuebles.com/inicio.do (ES/EN/"ge"). Perfil Idealista: https://www.idealista.com/pro/unicaja-inmuebles/ ("1.466 Inmuebles", "641 casas y pisos en venta").
- **Fecha de verificación:** 2026-11-14.

## 2. Stock en Mallorca
- El selector de provincia incluye **"BALEARES (ILLES)"** (https://unicajainmuebles.com/inicio.do).
- En Idealista Pro el filtro de provincias de GIA (37) **no incluye Illes Balears** y no se vio ningún anuncio balear (https://www.idealista.com/pro/unicaja-inmuebles/). Stock en Mallorca estimado **0–muy bajo** (no verificado: no se pudo ejecutar la búsqueda por provincia porque el formulario no expone URL en el HTML extraído).

## 3. Cómo expone los datos
- **(a) API JSON:** no observada. Aplicación Java (`*.do`, `jsessionid` en URL). Sin `/api/`, JSON ni GraphQL en el HTML leído.
- **(b) XML/sitemap:** `/robots.txt` y `/sitemap.xml` no leídos (fuera de lista permitida). Mapa web HTML: https://unicajainmuebles.com/mapaWeb.do (sin URL de listados ni RSS).
- **(c) HTML:** **server-side**; home con 12 destacados y enlaces a fichas sin JS. Cloudflare email-protection (`/cdn-cgi/l/email-protection`) → detrás de **Cloudflare**.

## 4. Paginación, filtros, IDs y campos
- **Búsqueda:** formulario (tipo, operación, provincia, municipio, dormitorios, zona, precio, **fecha de publicación**, m², referencia, CP, "SOLO CON FOTO"). Acción y nombres de parámetros **no vistos** (no verificado). Páginas conocidas: `favoritos.do`, `ultimasBusquedas.do`, `formAlerta.do`.
- **Ficha:** `https://unicajainmuebles.com/fichainmueble.do?referencia={ref}`; **ID estable = referencia de 10 dígitos** (ej. `0021133140`). Imágenes `/unicaja/datos/BaseInmueble--{n}/{id}/{num}_208x150.png`.
- **Campos:** en home: tipo, municipio, precio ("Compra*"). Ficha (no verificada).
- Nota del portal: "La información facilitada tendrá una validez de 24 horas desde su publicación".

## 5. robots.txt, condiciones, antibot
- **robots.txt:** no leído.
- **Aviso legal** (https://unicajainmuebles.com/avisoLegal.do): prohíbe "reproducción, distribución, comunicación pública, y transformación, sin la autorización previa de www.unicajainmuebles.com". Sin cláusula de scraping/robots.
- **Antibot:** Cloudflare presente; WebFetch leyó home, mapa web y aviso legal sin challenge.

## 6. Frecuencia de sondeo propuesta
- Si se confirma stock balear: **1 sondeo/día** filtrando provincia Baleares y "fecha de publicación".
- **Alternativa oficial:** **alertas por email** en https://unicajainmuebles.com/formAlerta.do ("Recibir alertas") — opción preferible dada la ausencia de stock conocido.

## 7. Riesgos y notas
- Unicaja figura entre los que financian "90–100 %" según perfil (https://financer.es/prestamos/pisos-de-bancos/); simulador hipotecario en portal.
- Precios sin gastos de notaría/registro/impuestos. Stock concentrado en Andalucía, Castilla y León, Cantabria, Asturias (Idealista Pro).
