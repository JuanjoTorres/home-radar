# Cimenta2 (Grupo Cooperativo Cajamar) — cimenta2.com

## 1. Resumen
- **Entidad:** Grupo Cooperativo Cajamar; Cimenta2 es "empresa participada" del grupo (https://valenciaplaza.com/valenciaplaza/tierra-de-empresas/cimenta2-pone-a-la-venta-mas-de-1500-inmuebles-con-descuentos-de-hasta-el-30-, 2-jul-2025). Titular: **CIMENTA2 GESTIÓN E INVERSIONES, S.A.U.**, NIF A-04764767, PITA Almería (https://cimenta2.com/aviso-legal/).
- **Historia:** en 2014 Cajamar vendió la gestión de Cimenta2 a Haya Real Estate por 10 años (https://www.alimarket.es/construccion/noticia/157783/cajamar-vende-su-plataforma-inmobiliaria); desde julio 2024 Cajamar vende por portal propio (https://radarpro.ai/blog/que-inmobiliaria-gestiona-cada-banco-pisos).
- **Portal:** https://cimenta2.com/ (WordPress, "#buscatuespacio"); buscador real en subdominio **https://inmuebles.cimenta2.com/inmuebles/s/** (no accesible con WebFetch: fuera de lista permitida).
- **Fecha de verificación:** 2026-11-14.

## 2. Stock en Mallorca
- **Probablemente nulo.** La campaña 2025 (>1.500 activos) se reparte entre C. Valenciana (~630), Andalucía (>500) y Murcia (>200); **Baleares no aparece** (valenciaplaza.com). La home sólo destaca Roquetas de Mar y Castellón. No se pudo ejecutar una búsqueda por Baleares → (no verificado).

## 3. Cómo expone los datos
- **(a) API JSON:** no observada en cimenta2.com (sin `/api/`, `/wp-json/`, `admin-ajax`). Los IDs de ficha (`a4hQw00000Far5BIAR`, `a1q3X000005Y4cRQAS`) y la ruta `/inmuebles/s/` son el patrón de **Salesforce Experience Cloud** (inferencia). Si es así, los datos se cargan vía **Aura/LWC** (`/s/sfsites/aura`, POST con `message`/`aura.context`), es decir, una SPA con JSON interno (no verificado).
- **(b) XML/sitemap:** `/robots.txt` no leído (fuera de lista permitida).
- **(c) HTML:** la home de WordPress es server-side; el buscador de inmuebles es muy probablemente SPA (no verificado).

## 4. Paginación, filtros, IDs y campos
- **Listado:** `https://cimenta2.com/buscador-avanzado/?tipoinmueble=07` (viviendas) / `=03` (garajes) → redirige a `https://inmuebles.cimenta2.com/inmuebles/s/?tipoinmueble=07`.
- **Ficha:** `https://cimenta2.com/buscador-avanzado/inv-expediente/{sfId}` y `https://inmuebles.cimenta2.com/inmuebles/s/inv-expediente/{sfId}/`; promociones `…/ga-promocion/{sfId}/`. **ID estable = Salesforce record id (18 chars)**.
- **Campos:** según prensa, "fotos, ubicación, metros cuadrados y precio" (valenciaplaza). Detalle, paginación y filtros (no verificados).

## 5. robots.txt, condiciones, antibot
- **robots.txt:** no leído.
- **Aviso legal** (https://cimenta2.com/aviso-legal/): prohíbe "modificar, copiar, reutilizar, explotar, reproducir, transformar, comunicar públicamente"; "Cualquier uso comercial no autorizado de los mismos, queda prohibido"; prohíbe obstaculizar el acceso "mediante el consumo masivo de los recursos informáticos". Sin mención literal a robots/scraping.
- **Antibot:** ninguno observado en cimenta2.com (GTM `GTM-5F2FNN59`). Subdominio Salesforce (no verificado).

## 6. Frecuencia de sondeo propuesta
- **Semanal**, sólo comprobación de existencia de stock Baleares (1 petición). No merece scraping continuo.
- Alternativa: solicitar información/alertas por formulario; oficinas de Cajamar en Baleares.

## 7. Riesgos y notas
- Campañas con descuentos hasta 30 %; financiación Cajamar (no detallada; no verificado).
- Si se implementa, hay que hacer ingeniería inversa de Aura → fragilidad alta y posible choque con "uso comercial no autorizado".
