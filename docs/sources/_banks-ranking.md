# Ranking de portales bancarios y agregadores de REO para Mallorca

Fecha de verificación: 2026-11-14. Método: WebSearch + WebFetch (sin navegador, sin evasión de robots/antibot). Límite: WebFetch sólo abre URL aparecidas en resultados de búsqueda, por lo que `robots.txt`/`sitemap.xml` no pudieron leerse en ningún dominio; se anota como "no verificado".

## Tabla (1 = mejor; escala 0–3 por criterio)

| # | Fuente | Fichero | Stock Baleares (verificado) | Facilidad acceso | Señales de actualización | Riesgo legal | Total |
|---|---|---|---|---|---|---|---|
| 1 | Diglo (Santander) | diglo.md | 14 activos (3 locales, garajes, 1–3 viviendas) → 1 | HTML server-side Drupal, coordenadas y ref. catastral inline → 3 | Orden por fecha de publicación, página "Novedades" → 3 | Aviso legal prohíbe extracción/reutilización, sin antibot → 1 | **8** |
| 2 | Banca March | banca-march.md | 3–4 (Manacor, Palma; sólo locales/despachos) → 1 | Catálogo completo en 1 URL HTML; fichas `.do` disallow → 2 | Precio "Antes/Ahora", sin fechas → 1 | Términos sin cláusula anti-scraping; robots veda fichas → 2 | **6** |
| 3 | DonComparador | doncomparador.md | ~176 Baleares / 44 Palma (republicados) → 2 | HTML server-side, IDs numéricos → 3 | Orden "más recientes" → 2 | Condiciones no leídas; sin antibot → 1 | **8*** |
| 4 | Facilitea Casa (CaixaBank) | faciliteacasa.md | 3.408 Baleares, pero mayoría agencias; filtro "inmueble de banco" sin verificar → 1 | HTML server-side → 3 | Orden "más recientes", "precio rebajado" → 2 | Términos: prohíbe "extraer información del portal" → 0 | **6** |
| 5 | EscogeCasa (Abanca) | escogecasa.md | ≥1 (Palma); Mallorca/Ibiza en selector → 1 | Next.js (posible `/_next/data` no verificado); errores de servidor en fichas → 1 | Sin señales vistas → 0 | Aviso legal no leído → 1 | **3** |
| 6 | Unicaja Inmuebles (GIA) | unicaja-inmuebles.md | 0 probable (Baleares en selector, ninguno en Idealista) → 0 | HTML `.do`, Cloudflare → 2 | Filtro "fecha de publicación", alertas email → 2 | Aviso legal sólo PI → 2 | **6**, sin stock |
| 7 | Colonya Caixa Pollença | colonya.md | "pequeño catálogo", 100 % balear, nº no verificado → 1 | Sólo vía Idealista Pro → 0 | — → 0 | Idealista → 0 | **1** |
| 8 | Cimenta2 (Cajamar) | cimenta2.md | 0 probable (campaña 2025 sin Baleares) → 0 | Salesforce Experience Cloud (SPA) → 1 | — → 0 | Aviso legal prohíbe uso comercial → 1 | **2** |
| 9 | Inmobiliaria Bancaria | inmobiliariabancaria.md | 123 Baleares (republicados) → 2 | Challenge antibot en municipios → 0 | Orden por fecha → 2 | No leído → 1 | **5**, bloqueado |
| 10 | Bankinter | bankinter.md | 0 (6 viviendas en España, ninguna balear) → 0 | Portal bloquea WebFetch → 0 | — | — | **0** |
| 11 | Inmobanco | inmobanco.md | no verificado | no verificado | — | — | n/d |

\* DonComparador puntúa alto pero republica anuncios de servicers ya cubiertos (Solvia, Aliseda, Altamira), por lo que su valor incremental es bajo.

## Los 3 a implementar primero
1. **Diglo (digloservicer.com)** — único portal 100 % de un banco con stock balear verificable, HTML limpio con coordenadas y referencia catastral, ordenación por fecha. 1 petición cada 12 h a `/venta-activos/cualquiera/islas-baleares`. Respetar el aviso legal: uso personal, sin republicar.
2. **Banca March (bancamarch.es)** — banco mallorquín que vende directamente sin intermediarios; stock pequeño pero 100 % propio. 1 petición/día al catálogo `cargarPdfInmuebleListado.do?idPoblacion=0&idTipo=0&idZona=0` y diff por referencia; no tocar fichas `.do` (disallow).
3. **Unicaja Inmuebles (formAlerta.do) + EscogeCasa** como "radar pasivo": alta en alertas por email de Unicaja y sondeo diario de 2 URL de EscogeCasa (Mallorca, Ibiza). Coste casi nulo; sirven para detectar si aparece residencial balear.

(Si se acepta cubrir agregadores pese a los duplicados, DonComparador sería el 4.º: 1 petición/día por municipio con orden "más recientes".)

## Candidatos descartados y motivo
- **BBVA** — no tiene portal propio: sus adjudicados se venden en Haya, hoy integrada en Solvia (https://radarpro.ai/blog/pisos-banco-bbva, act. 15-sep-2026; https://radarpro.ai/blog/que-inmobiliaria-gestiona-cada-banco-pisos). Divarian/Anida no operan como portal. → cubierto por solvia.md.
- **Banco Sabadell** — vendió Solvia a Intrum; canal actual = Solvia (https://www.eldiario.es/economia/banco-sabadell-inmobiliaria-solvia-millones_1_1793754.html; radarpro.ai). → solvia.md.
- **CaixaBank / BuildingCenter / Servihabitat** — cartera propia vía Solvia desde 2024 y cartera antigua en Servihabitat (radarpro.ai); Servihabitat tiene sólo 3 viviendas en Baleares en Idealista (https://www.idealista.com/pro/servihabitat2/venta-viviendas/balears-illes/). Facilitea Casa es marketplace de agencias, no REO. → solvia.md.
- **Santander: Altamira, Aliseda, Casaktua** — servicers ya cubiertos (altamira.md, aliseda.md); Casaktua pertenece al grupo Santander Inmuebles (idealista/news 15-ene-2026), no verificado stock balear.
- **Ibercaja** — portal https://portalinmobiliario.ibercaja.es/ operado con Solvia; ámbito Aragón/La Rioja/Guadalajara, sin Baleares (https://radarpro.ai/blog/pisos-banco-kutxabank-ibercaja, act. 4-nov-2026); WebFetch: `robots_disallowed`.
- **Kutxabank** — https://www.kutxabankinmobiliaria.es/ (`robots_disallowed`); cartera vendida por Servihabitat; ámbito País Vasco/Navarra, sin Baleares (radarpro.ai).
- **Cajasur** — integrada en Kutxabank/BBK; portal cajasurinmobiliaria.com de ámbito andaluz (https://www.inmonews.es/portales-inmobiliarios-bancos-2021/); sin indicios de Baleares (no verificado).
- **Eurocaja Rural, Globalcaja, Ruralvía, Caja Rural Central/Granada/Sur, Laboral Kutxa, Bantierra** — cajas sin implantación balear; portales regionales (inmonews.es, fincasuleta.com). Ruralvía sólo para clientes.
- **Caja Rural de Baleares / Caixa Ontinyent** — no se encontró portal de inmuebles ni referencia en los listados de 2026 (helpmycash, rankia, radarpro). Caixa Ontinyent es valenciana. → sin fuente (no verificado).
- **Deutsche Bank (Inmuebles DB)** — portal "actualmente no operativo" (idealista/news 15-ene-2026).
- **Haya Real Estate** — absorbida por Solvia en dic-2024 (radarpro.ai); haya.es/bbva, /cajamar, /liberbank obsoletos.
- **Sareb / Aliseda** — Sareb reduce ventas y cesa en 2027 (financer.es); cubierto en aliseda.md.
- **pisosdebancos.com** — ninguna búsqueda devolvió el dominio; probablemente inactivo (no verificado).
- **Secciones "pisos de bancos" de Idealista/Fotocasa/yaencontre/Housage/cocampo** — fuera de alcance (portales generalistas, antibot fuerte; cocampo y housage dieron error al leer).
