# Colonya, Caixa Pollença — catálogo de inmuebles

## 1. Resumen
- **Entidad:** Colonya, Caixa d'Estalvis de Pollença (caja balear). Vende activos adjudicados ("dación en pago o adjudicación hipotecaria"), descritos como "un pequeño catálogo de inmuebles a la venta", segundas residencias y locales en Illes Balears (https://colonya.com/es/particulares/ahorro-e-inversion/compra-un-inmueble).
- **Canal real:** **no tiene buscador propio**; la página enlaza a su perfil **Idealista Pro**: `https://www.idealista.com/pro/colonya/venta-{viviendas|edificios|locales|oficinas|terrenos|garajes}/`. También perfil en Habitaclia: https://www.habitaclia.com/inmobiliaria-colonya__caixa_pollenca_4267_1/ (WebFetch: client_error).
- **Fecha de verificación:** 2026-11-14.

## 2. Stock en Mallorca
- Número **no verificado** (la página de Colonya no da cifra; el perfil Idealista `/pro/colonya/venta-viviendas/` no estaba en la lista permitida de WebFetch). Por definición, el 100 % del stock es balear (red de 24 oficinas: Pollença, Palma, Alcúdia, Inca, Manacor, Sóller, Maó, Ciutadella, Eivissa…).

## 3. Cómo expone los datos
- **(a) API JSON propia:** no existe listado propio; los datos viven en Idealista (fuera de alcance de este inventario).
- **(b) XML/sitemap:** no aplica (no leído `/robots.txt` de colonya.com).
- **(c) HTML:** la página de Colonya es informativa (GTM `GTM-NL9834X`, token CSRF, Cloudflare email-protection) y sólo contiene enlaces externos.

## 4. Paginación, filtros, IDs y campos
- Dependen de Idealista Pro (`/pro/colonya/...`): ID = id de anuncio Idealista (no verificado). Sin identificador propio de Colonya visible (sólo `product_37` interno de la web corporativa).

## 5. robots.txt, condiciones, antibot
- Aviso legal: https://colonya.com/es/pagina-legal/aviso-legal (no leído). Sin scraping posible/necesario en colonya.com.
- La fuente efectiva (Idealista) tiene términos restrictivos y antibot fuerte → fuera de alcance.

## 6. Frecuencia de sondeo propuesta
- **Semanal**, vía perfil Idealista Pro `colonya` (si se decide cubrir Idealista Pro de forma manual/alertas Idealista). Sin RSS ni alertas propias.
- Alternativa: contacto directo con oficinas (formulario en la página).

## 7. Riesgos y notas
- Catálogo muy pequeño pero 100 % balear y vendido por el propio banco: candidato ideal a **lista de vigilancia manual** más que a scraping.
- Financiación propia no detallada en la página (no verificado).
