# Facilitea Casa (CaixaBank) — faciliteacasa.com

## 1. Resumen
- **Entidad:** CaixaBank. `og:site_name` = "CaixaBank S.A."; el pie nombra a **Facilitea Selectplace, S.A.U.** (Gran Via de les Corts Catalanes 159, Barcelona), intermediario de crédito inmobiliario "vinculado en exclusiva a CaixaBank, S.A." nº E579 (https://faciliteacasa.com/viviendas/comprar/Islas%20Baleares).
- **Qué es:** marketplace de vivienda de CaixaBank, **no** un portal de adjudicados: la mayoría de anuncios son de agencias ("Ofrecido por ACN Real Estate SL"). Tiene filtro "inmueble de banco". Los artículos de 2026 lo citan como canal de CaixaBank/BuildingCenter (https://financer.es/prestamos/pisos-de-bancos/), pero "BuildingCenter" no aparece en la web.
- **Fecha de verificación:** 2026-11-14.

## 2. Stock en Mallorca
- **"3.408 casas y pisos en Islas Baleares"** ("1 - 12 de 3408 resultados") en https://faciliteacasa.com/viviendas/comprar/Islas%20Baleares ; **Palma: 762** (https://faciliteacasa.com/viviendas/comprar/Islas%20Baleares?area=Palma). Incluye Menorca (Mahón) e Ibiza.
- Stock específicamente bancario: **no verificado** (el filtro "inmueble de banco" existe pero no se aplicó).

## 3. Cómo expone los datos
- **(a) API JSON:** no observada en el HTML leído (sin `__NEXT_DATA__`, `/_next/data/`, `/api/`, JSON-LD). Activos en `/files/download/{uuid}`. Framework no determinado (no verificado).
- **(b) XML/sitemap:** `/robots.txt` no leído (URL no permitida por WebFetch). Sitemap (no verificado).
- **(c) HTML:** **server-side**: listado con 12 tarjetas y ficha completa legibles sin JS.

## 4. Paginación, filtros, IDs y campos
- **Listado:** `/viviendas/comprar/{Provincia}?page=N` (12/página), `?area={Municipio}`. Filtros UI: tipo, precio, superficie, hab./baños, obra nueva, amueblado, extras (ascensor, garaje, piscina…), "inmueble de banco", "precio rebajado"; orden por relevancia, **más recientes**, precio, superficie, €/m²; vista mapa. Nombres de parámetros de filtro **no vistos** (no verificado).
- **Ficha:** `/vivienda/{venta|alquiler}-{tipo}-{municipio-slug}-{id}` → ej. `/vivienda/venta-piso-palma-425709`. **ID estable = sufijo numérico** (5875 … 425709); también `refInmueble=425709` en el simulador hipotecario.
- **Listado:** tipo, municipio, precio (+ nota de bajada), hab., baños, m² construidos, descripción corta, logo agencia.
- **Ficha (425709):** precio 599.000 €, €/m², 107 m², 3 hab., 2 baños, "Palma, Islas Baleares", dirección aproximada ("Dirección aproximada por deseo del anunciante"), mapa `#property-map` sin coordenadas en texto, fotos (3 visibles + "Ver todas"), descripción, certificado energético, extras, "Ofrecido por {agencia}". **No** hay ref. catastral ni fechas de publicación.

## 5. robots.txt, condiciones, antibot
- **robots.txt:** no leído. Meta robots presente (índice).
- **Términos** (https://faciliteacasa.com/terminos-y-condiciones, cláusula NOVENA): prohíbe "actuaciones dirigidas a extraer información del portal", "No copiar o reproducir el contenido del portal sin nuestro permiso", "No modificar o alterar nuestros materiales o contenidos". Sin mención literal a robots/scraping.
- **Antibot:** ninguno observado; WebFetch leyó listado, ficha y términos.

## 6. Frecuencia de sondeo propuesta
- Stock enorme pero mayoritariamente de agencias (alta competencia). Si se usa: sólo con filtro "inmueble de banco" + orden "más recientes", **1 sondeo cada 6–12 h**, primeras 2–3 páginas.
- Alternativa: sin RSS/alertas observadas; existe formulario de contacto.

## 7. Riesgos y notas
- Cláusula explícita contra extracción de información → riesgo legal medio-alto para scraping sistemático.
- No es fuente de adjudicados puros; valor para HomeRadar = filtro bancario (no verificado si devuelve REO de CaixaBank/BuildingCenter o de Solvia/Servihabitat).
- Financiación: intermediario vinculado a CaixaBank; condiciones no publicadas en la ficha.
