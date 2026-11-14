# Bankinter — Portal Inmobiliario (bankinter.com/www/es-es/cgi/ebk+inm+home)

## 1. Resumen
- **Entidad:** Bankinter, S.A.; portal propio desde 2016 (https://www.bankinter.com/blog/noticias-bankinter/portal-inmobiliario-bankinter — WebFetch: client_error). Citado como portal propio en 2026 (https://www.helpmycash.com/hipotecas/inmobiliarias-de-bancos ; https://radarpro.ai/blog/que-inmobiliaria-gestiona-cada-banco-pisos).
- **Portal:** https://www.bankinter.com/www/es-es/cgi/ebk+inm+home ; listado por provincia https://www.bankinter.com/www/es-es/cgi/ebk+inmuebles+listado?codProvincia=07 (07 = Illes Balears; URL vista en resultados de búsqueda). Perfil Idealista: https://www.idealista.com/pro/bankinter/venta-viviendas/.
- **Fecha de verificación:** 2026-11-14.

## 2. Stock en Mallorca
- Idealista Pro: **"Ver 6 viviendas"** / "17 Inmuebles"; **ninguna en Baleares** (Almería, Castellón, Granada, Jaén, Madrid, Zaragoza) (https://www.idealista.com/pro/bankinter/venta-viviendas/).
- El propio portal **no pudo leerse**: todas las URL de bankinter.com devolvieron `client_error` a WebFetch (bloqueo o 4xx). Stock en `codProvincia=07` → (no verificado; muy probablemente 0).

## 3. Cómo expone los datos
- **(a)/(b)/(c):** no verificables (bloqueo). El patrón `cgi/ebk+inmuebles+listado?codProvincia=NN` sugiere HTML server-side con parámetros de consulta (inferencia).

## 4. Paginación, filtros, IDs y campos
- Único parámetro observado: `codProvincia` (código INE de provincia, `07` Baleares, `51` Ceuta). Resto (no verificado).

## 5. robots.txt, condiciones, antibot
- **Antibot:** sí, observado: WebFetch recibió error de cliente en home, listado y blog → probable WAF/bloqueo de user-agents automáticos.
- robots.txt y condiciones (no leídos).

## 6. Frecuencia de sondeo propuesta
- **No implementar.** Comprobación manual trimestral de `codProvincia=07` desde navegador. Alternativa: Idealista Pro `bankinter` con filtro Balears.

## 7. Riesgos y notas
- Bankinter financia "90–100 %" de sus propios inmuebles según perfil (https://financer.es/prestamos/pisos-de-bancos/).
- Stock nacional mínimo (6 viviendas en Idealista) → fuente descartada para Mallorca.
