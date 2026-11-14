# Banca March — Inmuebles (bancamarch.es/es/inmuebles/)

## 1. Resumen
- **Entidad:** Banca March, S.A. (banco con sede en Palma). Vende **directamente** sus activos adjudicados: cada ficha indica "El inmueble es comercializado directamente por la entidad propietaria" y "La entidad propietaria no repercute al comprador honorarios de intermediación" (https://www.bancamarch.es/es/inmuebles/).
- **Portal:** https://www.bancamarch.es/es/inmuebles/ (versión EN: https://www.bancamarch.es/en/estate/). Sin servicer externo observado.
- **Catálogo completo (PDF/listado):** https://www.bancamarch.es/cargarPdfInmuebleListado.do?idPoblacion=0&idTipo=0&idZona=0 (WebFetch lo devolvió como listado HTML con 24 activos).
- **Fecha de verificación:** 2026-11-14.
- **Fuentes:** las URL anteriores; enlace listado en https://www.dondeestamicasa.com/enlaces.html; perfil en https://www.indomio.es/agencias-inmobiliarias/495369/banca-march/ (no leído).

## 2. Stock en Mallorca
- **24 activos en total** (catálogo completo). **En Mallorca: 3–4** → ref. 20018 local comercial Manacor 456 m² 290.000 € (antes 377.000 €); ref. 40171 despacho Palma, Polígono Son Castelló 193 m² 275.000 €; ref. SBM11079/3 despacho Palma 488 m² 1.300.000 € (antes 1.430.000 €); ref. SBM10233/2 local 92 m² 200.000 € con ubicación "Mallorca" en el listado paginado (sin municipio).
- **Ningún piso/casa residencial en Mallorca** a fecha de verificación; el resto son solares (Tenerife, Gran Canaria, Alicante), locales y un piso en Calpe.
- Cómo se comprobó: lectura de la página de inmuebles, de la página 3 del listado paginado y del catálogo completo.

## 3. Cómo expone los datos
- **(a) API JSON:** no observada. Aplicación Java (acciones `.do`); sin referencias a `/api/`, JSON ni GraphQL en el HTML leído.
- **(b) XML/sitemap:** `/robots.txt` y `/sitemap.xml` **no leídos** (WebFetch no permitió la URL). Existe "Mapa Web" HTML (https://www.bancamarch.es/en/website-map/).
- **(c) HTML:** **server-side** completo; el listado y los campos (ref., tipo, ubicación, m², precio, "Antes") aparecen en el HTML sin JS. Imágenes en `/resources/inmuebles/imagenes/`.
- Observado: GTM `GTM-WKKH6QN`, widget de llamada llamamegratis.es.

## 4. Paginación, filtros, IDs y campos
- **Listado paginado:** `/es/tratarAplicacionInmuebleBusquedaAvanzada.do?idTipo=0&idZona=0&idPoblacion=0&paginaActual=N` (también `/es/tratarAplicacionInmueble.do?...&paginaActual=N&csrfToken=...` en resultados de buscador). 3 páginas × ~9 activos. `0` = todos.
- **Filtros:** `idZona` (28 zonas: A Coruña … **Mallorca, Menorca, Eivissa, Formentera** …), `idPoblacion` (municipio), `idTipo`. Los valores numéricos de Mallorca **no se vieron** (el extracto sólo mostró etiquetas) → (no verificado).
- **Ficha:** `/es/cargarAplicacionInmueble.do?identificador={ref}&idPoblacion=0&idTipo=0&idZona=0`; **ID estable = referencia** (`20018`, `40171`, `SBM11079/3`; alfanumérica, con `/sufijo` de unidad). WebFetch recibió `robots_disallowed` en la ficha y en el listado paginado, pero sí leyó `/es/inmuebles/`, `/en/estate/` y el catálogo.
- **Campos en listado:** miniatura, referencia, tipo + estado ("Local comercial / Buen estado"), ubicación (municipio/isla), habitaciones, m², precio actual, precio anterior ("Antes"), descripción breve, disponibilidad ("Disponible"), a veces "CONSULTAR PRECIO".
- **Campos en ficha:** (no verificado; bloqueado por robots). No se vieron coordenadas, ref. catastral ni fechas.

## 5. robots.txt, condiciones, antibot
- **robots.txt:** no leído, pero WebFetch reportó `robots_disallowed` para `cargarAplicacionInmueble.do` y `tratarAplicacionInmuebleBusquedaAvanzada.do` → las rutas `.do` del buscador están **vedadas a robots**; `/es/inmuebles/` y el catálogo `cargarPdfInmuebleListado.do` sí fueron servidos.
- **Condiciones de uso:** https://www.bancamarch.es/en/terms-and-conditions-of-use/ — sin cláusula de scraping/robots; reserva de propiedad intelectual. (ES: enlace "Términos y condiciones de uso" en el pie).
- **Antibot:** ninguno observado (sin Cloudflare challenge). Token `csrfToken` en algunas URL de listado.

## 6. Frecuencia de sondeo propuesta
- Stock de 24 activos y rotación muy baja: **1 sondeo/día** al catálogo completo `cargarPdfInmuebleListado.do?...` (una sola petición, no bloqueada por robots) y diff por referencia.
- Evitar las rutas `.do` de ficha (disallow). Sin RSS ni alertas por email observadas; alternativa: contacto por formulario/WhatsApp del portal.

## 7. Riesgos y notas
- Sólo terciario y suelo en Mallorca hoy; útil como "radar" por si aparece residencial (banco local).
- Sin honorarios de intermediación; precios "Antes/Ahora" permiten detectar rebajas.
- No se indica financiación específica ni estado de ocupación en el listado.
