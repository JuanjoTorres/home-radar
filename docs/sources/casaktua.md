# Casaktua (marca del grupo Intrum/Solvia) — casaktua.com

## 1. Resumen
- **Quién es:** portal multibanco nacido de Banesto (2009) y después operado por Aktua. Intrum compró Aktua (2016) y en diciembre de 2024 fusionó Aktua, Haya, HRE y Solvia bajo la marca Solvia (https://brainsre.news/intrum-fusiona-inmobiliarias-haya-solvia-aktua-hre/). En 2022 Intrum hablaba de "fortalecer las marcas inmobiliarias Solvia y Casaktua" (https://www.idealista.com/news/etiquetas/casaktua).
- **Dueño en 2026:** Intrum / Solvia Servicios Inmobiliarios (inferencia; la web no pudo leerse). **No verificado** si Casaktua sigue siendo marca comercial activa tras la unificación; las últimas noticias en idealista/news son de 2022, pero Google sigue indexando fichas de 2026 con etiquetas "propiedad-santander", "propiedad-bankinter", "propiedad-ibercaja", "propiedad-targobank" → es un **marketplace multibanco**.
- **Portal:** https://www.casaktua.com (también `demos.casaktua.com` y `blog.casaktua.com` indexados).
- **Fecha de verificación:** 2026-11-14.

## 2. Stock en Mallorca
- **No se pudo contar** (WebFetch bloqueado por robots.txt). Fichas baleares indexadas: oficinas c/ Aragón 178 Palma (114199K), locales c/ Gremi de Passamaners 24 (109819K, Targobank) y Ferrer de Pallares 45 (122383K), estudio Puig d'Alaró 8 Palma (122874K, Santander), piso Pico Cebollera Palma (115555K), suelo en Manacor (308878, Bankinter) y Campos (102685K, Ibercaja). Fuente: búsqueda `site:casaktua.com Baleares`. Total nacional indexado: "8165 Resultados". Estimación balear: **10–30 anuncios** (no verificado).

## 3. Cómo expone los datos
- **(a) API JSON:** no observado (sin acceso).
- **(b) XML/sitemap:** no verificado.
- **(c) HTML:** desconocido. Las URLs con `?ficha=inmueble` y `?pg=322&or=0` sugieren render server-side clásico (hipótesis).

## 4. Paginación, filtros, IDs y campos
- **Listado:** `/comprar/Vivienda&ci={CIUDAD}/en-cualquier-estado?pg={N}&or={orden}`; también `/es/comprar/viviendas/{provincia-slug}` (ej. `palmas-las`).
- **ID estable:** `/inmueble/{slug}[/propiedad-{banco}]/{ref}?ficha=inmueble`; ref numérica con sufijo `K` (`114199K`, `122383K`) o sin él (`308878`). El segmento `propiedad-{banco}` identifica el propietario (santander, bankinter, ibercaja, targobank).
- **Campos:** no verificados.

## 5. robots.txt, condiciones, antibot
- **robots.txt:** WebFetch devolvió `robots_disallowed` en ficha y listado → el robots.txt **prohíbe** el rastreo a agentes genéricos (reglas exactas no leídas).
- **Condiciones de uso:** no leídas (no verificado).
- **Antibot:** no observado (no se llegó a la página).

## 6. Frecuencia de sondeo propuesta
- **No scrapear** (robots.txt lo prohíbe). Alternativas: agregadores que republican su stock — tucasa.com (https://www.tucasa.com/ofertas-de-compra-venta-de-viviendas-de-casaktua.com?a=&ges=2&idc=6005308&idz=0007&v=99 , zona 0007 = Baleares) y milanuncios (https://www.milanuncios.com/venta-de-viviendas-en-baleares/casaktua.htm); alertas del propio portal (no verificado).

## 7. Riesgos y notas
- Dependencia de la estrategia de marcas de Intrum (venta de Solvia en estudio, oct-2026): el dominio podría redirigir a solvia.es.
- Stock balear visto es mayoritariamente locales, oficinas y suelo, no vivienda.
- Campañas históricas: "200 viviendas en Baleares por 350 €/mes" (alquiler, https://economiademallorca.com/not/8402/casaktua-ofrece-en-islas-baleares-200-viviendas-por-350-euros-al-mes , fecha antigua).
