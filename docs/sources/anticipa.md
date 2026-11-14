# Anticipa Real Estate (Blackstone) — anticipa.com

## 1. Resumen
- **Quién es:** Anticipa Real Estate, gestora de activos y préstamos inmobiliarios del grupo Blackstone en España (>10 años). Aliseda es su hermana de comercialización (https://brainsre.news/servicers-inmobiliarios-espana/ , act. 1-oct-2026).
- **Dueño en 2026:** Blackstone (fuente brainsre.news; la web corporativa no nombra accionista).
- **Portal:** https://anticipa.com es **solo corporativa** (Quiénes somos, Servicios, Gestión responsable). La venta de inmuebles se canaliza por https://www.alisedainmobiliaria.com (enlazado desde el footer "Más de Anticipa"). Existen portales privados: `portalapi.anticipa.com/es-ES/` ("Portal personalizado", bloqueado por robots) y `portaldecomunicaciones.anticipa.com/Identity/Account/Login` (login).
- **Fecha de verificación:** 2026-11-14.

## 2. Stock en Mallorca
- **No aplica como fuente directa.** Los activos de Anticipa aparecen en Aliseda con referencia con prefijo `ant` (ej. `https://www.alisedainmobiliaria.com/inmueble/ant00020715107`, piso en c/ San Isidro, Palma, 78 m², 125.000 €). Ver `aliseda.md`.

## 3. Cómo expone los datos
- (a) API JSON: no (web corporativa). (b) XML/sitemap: no verificado. (c) HTML corporativo server-side, sin anuncios.

## 4. Paginación, filtros, IDs y campos
- Sin listados. Identificador de activos Anticipa en Aliseda: `ant` + 11 dígitos (observado 1 caso).

## 5. robots.txt, condiciones, antibot
- **robots.txt:** bloquea `portalapi.anticipa.com` a agentes genéricos (observado `robots_disallowed`); anticipa.com fue leído sin problema.
- **Aviso legal:** https://anticipa.com/aviso-legal (no leído en detalle; no verificado si contiene cláusula de extracción). PDFs: Código de conducta de proveedores (10-jun-2026), Normas de seguridad (11-mar-2025), CCGG Sareb (21-feb-2025), alojados en `storage.googleapis.com/aliseda/FrontWeb/pdf/…` (mismo bucket que Aliseda).
- **Antibot:** ninguno observado.

## 6. Frecuencia de sondeo propuesta
- Ninguna. Monitorizar vía Aliseda filtrando refs `ant*` si se quiere distinguir cartera Anticipa.

## 7. Riesgos y notas
- Anticipa gestiona carteras de crédito (ex-Catalunya Caixa/Blackstone); sus activos suelen ser adjudicados con posesión pendiente ("Sin posesión", "Alquilado hasta…") en Aliseda.
- Comparte infraestructura (bucket GCS) con Aliseda; cualquier cambio técnico afectará a ambas.
