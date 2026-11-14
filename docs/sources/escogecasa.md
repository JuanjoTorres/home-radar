# EscogeCasa (ABANCA) — escogecasa.es

## 1. Resumen
- **Entidad:** ABANCA. "EscogeCasa es la inmobiliaria de ABANCA" (https://www.escogecasa.es/en). Portal propio, sin servicer externo citado (https://radarpro.ai/blog/que-inmobiliaria-gestiona-cada-banco-pisos, act. 29-oct-2026).
- **Portal:** https://www.escogecasa.es (ES/EN). Blog: https://blog.escogecasa.es/. Perfil Idealista: https://www.idealista.com/pro/abanca/ ("622 Inmuebles", "96 casas y pisos en venta").
- **Fecha de verificación:** 2026-11-14.

## 2. Stock en Mallorca
- El desplegable de provincias del buscador lista **"Mallorca"** y **"Eivissa / Ibiza"** como entradas propias (no "Baleares"; sin Menorca) (https://www.escogecasa.es/en).
- En Idealista Pro se vio **1 anuncio en Palma (Son Ferriol)**; sin recuento por provincia (https://www.idealista.com/pro/abanca/). Stock en Mallorca estimado **muy bajo (1–5)** → (no verificado; la página `/comprar/piso/mallorca/` no pudo abrirse con WebFetch por no estar en la lista permitida).

## 3. Cómo expone los datos
- **(a) API JSON:** sitio **Next.js** (imágenes vía `/_next/image?url=...`, meta `next-size-adjust`). `__NEXT_DATA__` y `/_next/data/` **no vistos** en el extracto (no verificado; muy probable que exista JSON embebido o rutas `/_next/data/{buildId}/...json` si usa pages router). Sin `/api/` ni JSON-LD observados.
- **(b) XML/sitemap:** no leídos (`/robots.txt` fuera de lista permitida). Sin RSS ni alertas observadas; sólo newsletter en sibforms.com.
- **(c) HTML:** la home se sirvió renderizada (SSR); listados (no verificado).

## 4. Paginación, filtros, IDs y campos
- **Listado:** `/{es|en}/comprar/{tipo}/{provincia}/{municipio}/` (ej. `/en/comprar/piso/madrid/madrid/`, `/en/comprar/local-comercial/pontevedra/vigo/`). Para Mallorca la ruta probable es `/es/comprar/piso/mallorca/` (no verificado). Paginación y parámetros (no verificados). Búsqueda por referencia disponible.
- **Ficha:** `/{lang}/inmueble/{slug}/{id}/{id2}/` → ej. `/en/inmueble/flat-en-venta-oviedo-asturias/9313092/51051/`. **ID estable = primer número** (9313092); el segundo (51051) no se explica (no verificado). La ficha devolvió error de servidor al leerla con WebFetch.
- **Campos:** (no verificados en ficha). Home muestra tipo, municipio, precio.

## 5. robots.txt, condiciones, antibot
- **robots.txt:** no leído. 
- **Aviso legal:** https://www.escogecasa.es/en/aviso-legal — WebFetch devolvió error de servidor; cláusulas (no verificadas).
- **Antibot:** home legible; ficha y aviso legal fallaron con "SERVER_ERROR" (posible bloqueo intermitente o WAF; no verificado).

## 6. Frecuencia de sondeo propuesta
- **1 sondeo/día** a los listados Mallorca + Ibiza (2 URL). Stock mínimo.
- Alternativa: Idealista Pro `abanca` con filtro Balears; newsletter del blog.

## 7. Riesgos y notas
- ABANCA figura entre los bancos que financian "90–100 %" de sus propios inmuebles según perfil (https://financer.es/prestamos/pisos-de-bancos/). Simulador en `/financiacion`.
- Portal orientado a Galicia/Asturias; Mallorca es residual.
