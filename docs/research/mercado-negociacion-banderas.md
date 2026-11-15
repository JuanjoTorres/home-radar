# HomeRadar Mallorca: margen de negociación, referencias €/m² y banderas rojas

**Fase 3: investigación de base**
- Fecha de elaboración: 2026-11-15
- Todas las URLs se consultaron el 2026-11-15, salvo indicación en contrario.
- Convenciones:
  - **[DATO]**: cifra o hecho tomado de una fuente, con su fecha.
  - **[CÁLCULO PROPIO]**: operación aritmética sobre datos citados.
  - **[JUICIO]**: propuesta de diseño, no respaldada por una fuente.
  - **NO VERIFICADO**: no se pudo confirmar; se indica el motivo.

> Aviso: este documento es un apoyo técnico para un motor de reglas, no asesoramiento jurídico. Antes de firmar arras, cualquier bandera Crítica o Alta debe revisarla un abogado, un notario o un técnico.

---

## Resumen ejecutivo

1. **Margen de negociación real en España (cierre frente a precio de salida): ~6 %.** Tecnocasa/UPF mide un 6,2 % a cierre de 2025, en mínimos similares a 2007, con 77 días de venta media en 2025 (73 en 2024).
2. **Baleares y Palma:**
   - Rebajas en anuncio: idealista registra una rebaja media del **7 %** en Baleares y en Palma (T2 2026). En euros son 96.326 € en Baleares y 77.906 € en Palma, la mayor rebaja absoluta entre capitales.
   - Anuncios rebajados: el **13 %** en Baleares (T2 2026), frente al **16 %** nacional.
   - Brecha oferta/demanda de Fotocasa en Palma: **16 %** (S1 2026) y **18 %** (S2 2025). Es de las más altas de España, pero mide la distancia entre lo que se pide y lo que la demanda está dispuesta a pagar, no la rebaja efectivamente cerrada.
3. **Bancos y servicers:** las campañas anuncian descuentos "de hasta" un 35–60 %. Son máximos de campaña, no medias. Las únicas medias documentadas son de mercados de nicho:
   - Viviendas "sin posesión": 30–70 % (Brikex, sep. 2026).
   - Ocupadas: 60 % (OCU, abr. 2025).
   - **No se encontró ninguna fuente pública con la tasa de aceptación de ofertas por debajo del precio en servicers (NO VERIFICADO).**
4. **Benchmark recomendado:**
   - **Principal, automatizable:** valor tasado del MIVAU para los 6 municipios mallorquines de más de 25.000 habitantes (Excel trimestral).
   - **Automático complementario:** IBESTAT en JSON para el total de Baleares.
   - **Contraste trimestral manual:** Registradores (ERI, Palma).
   - **Consulta manual:** Portal del Notariado (municipios pequeños y códigos postales). No se ha documentado exportación ni API.
5. **Red flags:** catálogo de 24 códigos en la sección D. Los críticos son `noPossession`, `occupied`, `auctionAssignment`, `bareOwnership`, `undividedShare`, `protectedRuralLand` y `noHabitabilityCertificate`.

---

## A. Margen de negociación (precio de salida frente a precio de cierre)

### A.1 Cifras de brecha (España, Baleares y Palma)

| # | Métrica | Ámbito | Valor | Periodo | Fuente (fecha publicación) | Qué mide exactamente |
|---|---|---|---|---|---|---|
| A1 | Margen de negociación medio | España | **6,2 %** | Cierre 2025 | Tecnocasa/Cátedra UPF vía idealista/news (05-03-2026) | Rebaja final frente a precio de salida en operaciones de la red Tecnocasa. "Similar a 2007" |
| A2 | Tiempo medio de venta | España | **77 días** (73 en 2024) | 2025 | Ídem | Red Tecnocasa |
| A3 | Rebaja media en anuncios rebajados | España | **7 %** (29.390 €) | T1 2026 | idealista (25-06-2026) | Rebaja frente al precio inicial de salida en el anuncio |
| A4 | Rebaja media | Palma | **70.950 €** (sin % publicado) | T1 2026 | idealista (25-06-2026) | Mayor rebaja absoluta entre grandes ciudades |
| A5 | % anuncios con bajada | España | **14 %** (11 % un año antes) | T1 2026 | idealista (26-05-2026 / 05-06-2026) | Sin dato Baleares en esa edición |
| A6 | % anuncios con bajada | España | **16 %** (13 % T2 2025) | T2 2026 | idealista vía Diari de Tarragona y Mallorca.com (24-09-2026) | Rebaja media nacional: 28.280 € (~6 %) |
| A7 | % viviendas vendidas tras rebaja | **Baleares** | **13 %** | T2 2026 | idealista vía Europa Press / Mallorca.com (24-09-2026) | Texto: "sold only after their asking price was reduced" |
| A8 | Rebaja media | **Baleares** | **7 %** (96.326 €) | T2 2026 | Ídem | |
| A9 | Rebaja media | **Palma** | **7 %** (77.906 €) | T2 2026 | Ídem | Mayor rebaja absoluta entre capitales |
| A10 | Índice de Negociación (oferta frente a demanda) | **Palma** | **16 %** (la capital con mayor diferencia) | S1 2026 | Fotocasa vía Bolsamanía (17-06-2026) | Distancia entre precio de oferta y precio que la demanda está dispuesta a pagar. Mínimo: San Sebastián, 1 % |
| A11 | Índice de Negociación | **Palma** | **18 %** (2.ª tras Granada, 22 %) | S2 2025 | Fotocasa (DataVenues) vía Brainsre (04-12-2025) | Ídem |
| A12 | % anuncios que rebajan en un mes | España | <10 % la mayoría de meses (4,5–10,5 %); rebajas típicas del 5–9 % | 2025 | pisos.com vía Spanish Property Insight (15-02-2026) | En **octubre de 2025**, ~1/3 de vendedores en **Baleares** rebajó (estacional, fin de temporada alta) |
| A13 | Vendidas con descuento | Red Alfa Inmobiliaria | 38 % (24 % un año antes) | T2 2026 | El Español (11-08-2026) | Solo la red de Alfa; sin descuento medio |
| A14 | Brecha portales (Idealista/Fotocasa) frente a notarial | España | "hasta un 44 %"; 12 % en 2021 | 2021–T2 2025 | UVE Valoraciones vía Brainsre (28-10-2025) | Máximo, no media. Parte se explica por la menor superficie de lo comprado. idealista ha criticado la metodología (idealista/news nodo 869670) |
| A15 | Brecha idealista frente a notarios | España | ~29 % | NO VERIFICADO (sin fecha en la página) | spainhousing.eu vía The Corner | Serie histórica, no datada |
| A16 | Tiempo de venta | España | De 2–3 semanas a 6–9 semanas; previsión 8–10 | 2026 | FAI vía Merca2 (15-07-2026) | Encuesta sectorial, sin cifra de descuento |

**Comparación asking frente a registrado (Baleares y Palma) [CÁLCULO PROPIO]**

Precios de oferta (Fotocasa):
- Baleares, junio 2026: 5.441 €/m².
- Palma, julio 2026: 5.200 €/m².

Precios registrados (Registradores, T2 2026):
- Baleares: 4.311 €/m².
- Palma: 4.291 €/m².

Resultado: el precio de oferta supera al registrado en un **~26 %** en Baleares (5.441/4.311) y un **~21 %** en Palma (5.200/4.291).

Esta diferencia **no** es un margen de negociación, por cuatro motivos:
- La composición de lo anunciado y lo vendido es distinta.
- La superficie se mide de forma distinta: el anuncio suele usar la construida declarada y el registro la escriturada.
- Los periodos no coinciden.
- Los portales tienen sesgo de stock: lo caro tarda más en venderse y permanece más tiempo anunciado.

Sirve como **cota superior** de "sobreprecio de anuncio" y para detectar anuncios fuera de mercado.

**Mercado mallorquín en 2026 (tensión)**

Señales de mercado todavía tenso, pero en enfriamiento:
- La oferta en venta cae un 7 % en Baleares y un 4 % en Palma (T1 2026 interanual, idealista vía Noudiari, 14-05-2026).
- Las compraventas en Palma caen un 24,96 % interanual (T2 2026, Gloval vía Mallorca.com, 01-10-2026), con 4.351 €/m² (+11,5 %).
- La cuota de compradores extranjeros en Baleares es del 32,27 %, la más alta de España (Registradores, T2 2026).
- Las compraventas en Baleares caen un 18,3 % (OVS-MIVAU, T1 2026).

Conclusión [JUICIO]: más rebajas y más días en anuncio en 2026 que en 2025, pero el margen medio de cierre sigue en el rango de un solo dígito.

### A.2 Variación por factores

| Factor | Evidencia | Valoración |
|---|---|---|
| **Días en mercado** | Venta media de 77 días en España en 2025 (Tecnocasa). Ventas que pasan de 2–3 a 6–9 semanas (FAI 2026). **No se encontró ningún estudio público español con la relación cuantitativa entre días en anuncio y descuento de cierre.** | La relación positiva es plausible e intuitiva, pero su pendiente es **[JUICIO]** (NO VERIFICADO). |
| **Bajadas previas** | Rebaja media de un anuncio rebajado del 6–7 % (idealista 2026); rebajas típicas del 5–9 % (pisos.com 2025). En Baleares, el 13 % de las ventas se cerró tras rebaja (T2 2026). | Una bajada observada es una rebaja **ya materializada** en el precio publicado. La fuente no indica cuánto margen adicional queda tras ella. **[JUICIO]**: añadir un margen menor que la propia rebaja. |
| **Tipo de vendedor: banco o servicer** | Campañas: Servihabitat, "hasta 35 %" residencial (feb. 2024); Solvia, "hasta 40 % sobre valor de tasación en algunos casos" (oct. 2025); Diglo, "hasta 45 %" (sep. 2022) y "hasta 60 %" (2023). En las bases legales de Diglo (mayo 2025), la base 5 dice que Diglo "puede aceptar ofertas en condiciones diferentes". OCU (abr. 2025): "No todos tienen precios inferiores a la media del mercado"; muchos con cargas. | Ninguna campaña documentada menciona Baleares. Los "hasta X %" no son medias. Tasa de aceptación de contraofertas: **NO VERIFICADO**. |
| **Viviendas sin posesión u ocupadas** | Brikex (vía Europa Press, 18-09-2026): descuentos estructurales del 30–70 %; más de 23.000 viviendas, ~3 % de la oferta; "No se puede visitar ni hipotecar". OCU (abr. 2025): ocupadas al 60 % de descuento. | El descuento compensa un riesgo jurídico y la falta de financiación. **No es un margen de negociación** para un comprador de primera vivienda: es bandera roja (sección C). |
| **Estado (reforma)** | No se encontró ninguna cifra de descuento de cierre por estado en Baleares. | **[JUICIO]**: +1 a +2 pp. |
| **Obra nueva o promotor** | No se encontró ninguna cifra. | **[JUICIO]**: margen bajo (0–3 %), porque el precio suele estar fijado por promoción. |
| **Estacionalidad (Baleares)** | ~1/3 de vendedores de Baleares rebajó en octubre de 2025 (pisos.com). | **[JUICIO]**: pequeño ajuste positivo de octubre a febrero. |

### A.3 Palabras clave de urgencia o motivación (normalizar: minúsculas, sin acentos)

La lista de términos es **[JUICIO]**, construida a partir del uso habitual en portales. Las fuentes no aportan ninguna validación estadística de su poder predictivo.

| Señal | Español | Català | English | Deutsch |
|---|---|---|---|---|
| Rebaja publicada (`priceDropKeyword`) | rebajado, rebajada, rebaja, bajada de precio, precio reducido, nuevo precio, precio rebajado, ha bajado | rebaixat, rebaixada, rebaixa, preu rebaixat, nou preu, baixada de preu | reduced, price reduced, price drop, new price, price cut, now only | reduziert, Preis reduziert, Preissenkung, neuer Preis, jetzt nur |
| Urgencia (`urgentSaleKeyword`) | urge venta, urge vender, venta urgente, necesita vender, liquidación, por traslado, por viaje | urgeix venda, venda urgent, cal vendre, per trasllat | urgent sale, must sell, quick sale, relocating, motivated seller, distressed sale | dringend zu verkaufen, dringender Verkauf, Notverkauf, muss verkaufen, motivierter Verkäufer, wegen Umzug |
| Apertura a ofertas (`openToOffersKeyword`) | precio negociable, negociable, se escuchan ofertas, escucha ofertas, abierto a ofertas, admite ofertas, acepta ofertas, haga su oferta | preu negociable, s'escolten ofertes, accepta ofertes, obert a ofertes, faci la seva oferta | negotiable, open to offers, offers considered, OIRO, ONO, make an offer, best offer | Preis verhandelbar, VB, Verhandlungsbasis, Angebote erwünscht, auf Anfrage verhandelbar |
| Oportunidad (`bargainKeyword`, peso bajo, suele ser marketing) | oportunidad, ocasión, chollo, precio de derribo, por debajo de mercado, ideal inversores | oportunitat, ocasió, ganga, per sota de mercat | bargain, opportunity, below market value, great deal, priced to sell | Gelegenheit, Schnäppchen, unter Marktwert, Preis-Leistungs-Hit |
| Reforma (`needsRenovationKeyword`) | a reformar, para reformar, necesita reforma, para actualizar, a rehabilitar | a reformar, per reformar, necessita reforma | needs renovation, to renovate, fixer-upper, project, needs updating | renovierungsbedürftig, sanierungsbedürftig, zu renovieren, Renovierungsobjekt |
| Herencia (`inheritanceKeyword`, señal débil) | herencia, por herencia, procedente de herencia | herència, per herència | inheritance, probate, estate sale | Erbschaft, Nachlass, Erbengemeinschaft |

> Ojo con los falsos positivos: "oportunidad" y "chollo" también aparecen en anuncios de stock de servicers "sin posesión" y en "solo inversores". Las reglas de bandera roja (sección C) deben evaluarse **antes** y pueden anular el bonus de margen.

### A.4 Tabla de reglas propuesta para el estimador de margen

Salida del estimador: un **rango** `[marginLow, marginHigh]` y un valor central `marginExpected`, en % sobre el precio publicado actual.

| Regla | Condición | Efecto sobre `marginExpected` | Base | Tipo |
|---|---|---|---|---|
| R0 base particular o agencia | `sellerType ∈ {private, agency}` | **6 %** | Tecnocasa 6,2 % (2025); rebaja media del 7 % en Baleares y Palma (T2 2026) | Anclado en dato; el redondeo es [JUICIO] |
| R1 base banco o servicer | `sellerType = bankServicer` (Solvia, Aliseda, Diglo/Servihabitat, Haya, Hipoges, Sareb, Altamira, Anticipa, etc.) | **8 %** | Las campañas anuncian descuentos frente a tasación y Diglo admite ofertas distintas, pero falta una media de cierre | [JUICIO] |
| R2 base obra nueva o promotor | `sellerType = developer` o `isNewBuild` | **2 %** | Sin dato | [JUICIO] |
| R3 bajadas previas | Por cada bajada registrada por HomeRadar en el historial del anuncio | **+1,5 pp** por bajada, máx. +4,5 pp | Las rebajas de anuncio rondan el 5–9 %; se descuenta parcialmente porque ya están aplicadas | [JUICIO] |
| R4 magnitud de bajada acumulada | `cumulativeDropPct ≥ 10 %` | **+1 pp** (vendedor que ya ha cedido y probablemente sigue cediendo) | Sin dato | [JUICIO] |
| R5 días en mercado | Por cada 30 días por encima de 60 días en anuncio | **+1 pp**, máx. +5 pp | Venta media de 77 días (2025); en 2026 se alarga | Umbral de 60 días anclado en el dato; la pendiente es [JUICIO] |
| R6 anuncio fresco | `daysOnMarket < 14` **y** municipio con Índice de Negociación de Fotocasa < 10 % (o sin dato) | **−2 pp** | Mercado tenso | [JUICIO] |
| R7 urgencia | `urgentSaleKeyword` u `openToOffersKeyword` | **+2 pp** (no acumulables entre sí) | Sin dato | [JUICIO] |
| R8 reforma | `needsRenovationKeyword` | **+1,5 pp** | Sin dato | [JUICIO] |
| R9 sobreprecio frente a benchmark | €/m² del anuncio > 1,30 × benchmark del municipio (sección B) | **+2 pp** | Brecha asking/registrado del ~21–26 % en Palma y Baleares [CÁLCULO PROPIO] | Umbral 1,30 = [JUICIO] |
| R10 estacional | Mes del anuncio entre octubre y febrero | **+0,5 pp** | Pico de rebajas en Baleares en octubre de 2025 (pisos.com) | [JUICIO] |
| R11 tope | Siempre | `marginExpected ≤ 15 %` | Índice Fotocasa de Palma, 16–18 % (oferta frente a demanda), usado como techo | Anclado; el tope exacto es [JUICIO] |
| R12 exclusión | Cualquier bandera Crítica (sin posesión, ocupado, cesión de remate, nuda propiedad, proindiviso) | **No calcular margen**: mostrar "precio no comparable" | El descuento del 30–70 % es prima de riesgo (Brikex, OCU) | Anclado |
| Rango | Siempre | `marginLow = max(0, expected − 3)`, `marginHigh = min(15, expected + 4)` | Asimetría al alza por la evidencia de Fotocasa | [JUICIO] |

Ejemplo:
- Situación: particular en Palma, 120 días anunciado, 2 bajadas, "precio negociable".
- Cálculo: 6 + 3 + 2 (120−60 = 60 días → 2 tramos) + 2 = **13 %**. Rango 10–15 %.

**Calibración recomendada [JUICIO]:** guardar el precio final cuando el usuario conozca un cierre real, y comparar trimestralmente el €/m² medio de los anuncios retirados de HomeRadar con el registrado (Registradores) en Palma. Si la brecha cambia más de 5 pp, reajustar R0.

---

## B. Referencias €/m² de mercado (accesibles por máquina)

### B.1 Comparativa de fuentes

| Fuente | Tipo de precio | Granularidad Mallorca | Frecuencia | Acceso máquina | Último dato verificado |
|---|---|---|---|---|---|
| **Portal Estadístico del Notariado** (penotariado.com/inmobiliario) | Transacción (escrituras, IUIN) | CCAA, provincia, **municipio, código postal** y área dibujada ≤5 km² | **Mensual** (últimos 12 meses; histórico de 12 años) | **No se documenta exportación ni API** en ninguna fuente consultada. El dominio está bloqueado desde este entorno (no se pudo inspeccionar). | Media 2016–2025: Deià 6.486; Andratx 5.973; Calvià 4.695 €/m² (Vozpópuli, 21-07-2026). Palma actual: **NO VERIFICADO** |
| **MIVAU, valor tasado de la vivienda libre** | Tasación hipotecaria | **Municipios >25.000 hab.:** Palma, Calvià, Manacor, Llucmajor, Marratxí, Inca | Trimestral | Excel (XLS) en la web del Ministerio; reutilizado por ICANE, datos.gob.es, etc. | **Palma, T1 2026: 3.789 €/m²** (+6,0 % trimestral, +14,7 % interanual). Baleares, T1 2026: 3.886 €/m² (+13,1 %); protegida: 1.155 €/m² (AHE, informe precios 1T 2026) |
| **IBESTAT** (reelabora MIVAU) | Tasación | Solo **Illes Balears** (total, ≤5 años, >5 años) | Trimestral | **Sí: JSON, JSON-stat y PC-Axis**, con URL estable | **Baleares, T2 2026: 3.980,8 €/m²**; ≤5 años 4.551,1; >5 años 3.956,1 |
| **Registradores (ERI trimestral)** | Transacción registrada | Baleares + **Palma** (solo capitales) | Trimestral (PDF) | PDF; no consta como open data | **T2 2026:** Baleares 4.311 €/m² (+9,7 % interanual), usada 4.347, nueva 4.158. **Palma 4.291 €/m²** (+10,6 %). Extranjeros: 32,27 % |
| **Tinsa IMIE Mercados Locales** | Tasación | Palma y Baleares | Trimestral | Web; informe tras formulario | **Palma, T3 2026: 3.581 €/m²** (+11,0 %); Baleares 3.908 €/m² (+15,0 %) |
| **INE** | IPV (índice, sin €/m²) y valor tasado (operación 20007, del MIVAU) | CCAA | Trimestral | API JSON del INE (IPV) | Sin €/m² municipal |
| **Fotocasa / idealista** (solo contexto, **no scrapear**) | Oferta | Municipio | Mensual | Informes públicos | Fotocasa, julio 2026: Palma 5.200; Calvià 7.260; Campos 6.803; Manacor 3.432; Inca 3.276 €/m²; Baleares 5.401. Junio 2026: Baleares 5.441 €/m² |

### B.2 Valores municipales del MIVAU (municipios mallorquines >25.000 hab.)

| Municipio | Valor tasado €/m² | Trimestre | Estado |
|---|---|---|---|
| Palma | 3.789 | T1 2026 | Verificado (AHE, a partir del MIVAU) |
| Palma | 3.337 | 2025 (trimestre no indicado; contexto Baleares T2 2025 = 3.518,2) | Verificado (Última Hora, 20-10-2025) |
| Calvià | — | — | **NO VERIFICADO**: el Excel municipal del MIVAU no se pudo descargar (dominio bloqueado desde el entorno de investigación) |
| Manacor | — | — | **NO VERIFICADO** (ídem) |
| Llucmajor | — | — | **NO VERIFICADO** (ídem) |
| Marratxí | — | — | **NO VERIFICADO** (ídem) |
| Inca | — | — | **NO VERIFICADO** (ídem) |

### B.3 URLs de descarga

- **MIVAU, página de la estadística de valor tasado** (metodología y datos; enlazada desde la ficha del INE, operación 20007): `https://www.mitma.gob.es/el-ministerio/informacion-estadistica/vivienda-y-actuaciones-urbanas/estadisticas/valor-tasado-de-la-vivienda`
  - Es la URL heredada de MITMA. El ministerio actual es MIVAU (mivau.gob.es) y es probable que redirija.
  - **El nombre exacto del fichero XLS de "municipios de más de 25.000 habitantes" está NO VERIFICADO** porque el dominio está bloqueado desde este entorno. Implementación: descargar la página índice, localizar el enlace cuyo título contenga "municipios" y "25.000", y parsear el Excel por nombre de municipio.
- **Réplica en datos.gob.es** (serie base 2005, por municipios): `https://datos.gob.es/en/catalogo/a13002908-valor-tasado-de-la-vivienda-libre-por-municipios-de-mas-de-25-00-habitantes-total-base-20054`. Sin URL de distribución visible al consultarla.
- **IBESTAT, JSON** (Baleares, por antigüedad): `https://intranet.caib.es/ibestat-jaxi/tabla.do?typeDownload=7&px=d64a71a9-7818-4329-bd1d-9e366ed62ad2`
  - JSON-stat: `typeDownload=6`. PC-Axis: `typeDownload=0`.
  - Ficha: `https://intranet.caib.es/opendatacataleg/es/dataset/valor-tasado-de-la-vivienda-libre-por-periodo-y-antiguedad-desde-1995-primer-trimestre-a-2025-s`
- **Registradores**: `https://www.registradores.org/actualidad/portalestadistico-registral/estadisticas-de-propiedad` (PDF ERI trimestral, p. ej. `https://www.registradores.org/documents/d/guest/eri_2t_2026`).
- **Notariado** (consulta manual): `https://penotariado.com/inmobiliario/es/home`.

### B.4 Recomendación de benchmark

1. **Benchmark principal (automático, trimestral):** valor tasado del MIVAU para Palma, Calvià, Manacor, Llucmajor, Marratxí e Inca.
   - Ventajas: fuente oficial, gratuita, trimestral, municipal y en Excel.
   - Sesgo: el valor tasado queda **por debajo** del precio de oferta. En Palma, T1 2026: 3.789 €/m² tasado frente a 5.200 €/m² de oferta (Fotocasa, julio 2026). Por eso se usa como suelo de referencia, con un ratio `asking/benchmark`.
   - Cadencia: el MIVAU publica trimestralmente. Revisar mensualmente si hay trimestre nuevo y, como mínimo, cargar en marzo, junio, septiembre y diciembre. El calendario exacto de publicación está **NO VERIFICADO**.
2. **Respaldo para municipios <25.000 hab.:** IBESTAT JSON (Baleares) × factor municipal **[JUICIO]**, o un valor manual mensual del Portal del Notariado introducido por el usuario. No hay API documentada, así que **no automatizar** su extracción sin revisar antes sus condiciones de uso.
3. **Contraste de transacción (manual, trimestral):** Registradores, Palma (4.291 €/m², T2 2026). Sirve para calibrar R9 y el estimador.
4. **No usar como benchmark** los precios de oferta de idealista o Fotocasa. Solo valen como contexto del ratio de sobreprecio.

---

## C. Banderas rojas: base legal y práctica

### C.1 Cédula de habitabilidad (Baleares/Mallorca)

- **Base legal:** Ley 5/2018, de 19 de junio, de la vivienda de las Illes Balears, **art. 16** (BOE-A-2018-9774, versión consolidada; última modificación 13-06-2026).
  - **Art. 16.1:** la emite el **consejo insular**. En Mallorca, el **Consell de Mallorca**, Departamento de Territorio (Mallorca.com, 28-09-2026). Es obligatoria para **ocupar o arrendar**, salvo que la sustituya la licencia de ocupación o primera utilización.
  - **Art. 16.2:** en VPO, en primera ocupación, la sustituye la calificación definitiva.
  - **Art. 16.3:** las compañías de **electricidad, agua, gas y telecomunicaciones** deben exigirla para contratar definitivamente.
  - **Art. 16.4:** en **venta, alquiler o cesión de uso** se incorpora copia de la cédula vigente. **Si no existe, debe hacerse constar en el documento privado o en la escritura.** La ley, por tanto, no prohíbe vender sin cédula; obliga a declararlo. Una guía comercial (Mallorca.com) afirma que "el notario no puede escriturar sin cédula vigente"; **esa afirmación contradice la redacción literal del art. 16.4** y no debe usarse.
  - **Art. 16.5:** la pérdida de las condiciones mínimas supone la revocación.
  - El reglamento de condiciones es el **Decreto 145/1997**, citado por el art. 14.1 de la Ley 5/2018.
- **Vigencia:** 10 años (Mallorca.com, 28-09-2026). **Fuente secundaria**: verificar en el Decreto 145/1997 (NO VERIFICADO en texto oficial).
- **Hipoteca:**
  - Los bancos suelen exigirla (iAhorro, 20-05-2026).
  - En tasación, la Orden ECO/805/2003 obliga a **condicionar** el valor cuando falta documentación (art. 10.1.d) y a poner **advertencia** si el inmueble no es conforme con el planeamiento (art. 11.b). La orden no nombra la cédula expresamente.
  - En la práctica, sin cédula la financiación es difícil o imposible [secundario].
- **Efectos para el comprador de primera vivienda:** sin cédula no se pueden contratar suministros definitivos ni alquilar. A menudo indica problemas urbanísticos: ampliaciones sin licencia, fuera de ordenación (Mallorca.com, Hosteltur 2022).
- **Cambios recientes:**
  - **Ley 4/2026, de 11 de junio** (BOIB 74, 13-06-2026; en vigor el 14-06-2026), de aceleración de proyectos estratégicos y simplificación administrativa. En las partes revisadas del texto consolidado (BOE-A-2026-15579) **no se encontró ninguna modificación del art. 16 de la Ley 5/2018 ni del régimen de la cédula**. Sí modifica:
    - la LUIB, art. 125 (IEE), y
    - la ley de VPO (visado de todos los contratos de compraventa y alquiler).
  - "Llei 3/2024" balear (**Ley 3/2024, de 3 de mayo, de medidas urgentes en materia de vivienda**): crea la vivienda de precio limitado (VPL). No se encontró ningún cambio en la cédula.
  - **NO VERIFICADO:** que exista en 2024–2026 un nuevo decreto de habitabilidad que sustituya al 145/1997. Normatia lo lista como vigente en su "versión 2023".

**Palabras clave** (normalizar acentos y apóstrofos):
- Español: sin cédula, sin cedula, no tiene cédula, no cédula, cédula en trámite, cédula pendiente, cédula caducada, sin habitabilidad, sin licencia de primera ocupación, sin LPO.
- Català: sense cèdula, sense cedula, cèdula en tràmit, cèdula pendent, cèdula caducada, sense habitabilitat, sense llicència de primera ocupació.
- English: no habitation certificate, no habitability certificate, no cedula, cedula pending, cedula in process, without occupancy licence.
- Deutsch: ohne Cédula, ohne Bewohnbarkeitsbescheinigung, Cédula in Bearbeitung, ohne Bewohnbarkeitszertifikat, keine Bewohnbarkeitsbescheinigung.

### C.2 IEE (Informe de Evaluación del Edificio, antes ITE) en Palma y Baleares

- **Base:** LUIB, Ley 12/2017, **art. 125**, modificado por la **Ley 4/2026** (disp. final 26.ª).
  - Los propietarios de **edificios de 50 años o más** deben presentar el IEE **cada 10 años**.
  - Debe inscribirse en el **Registro de Informes de Evaluación de Edificios (RIAE)** antes de presentarlo al ayuntamiento.
  - Excluye construcciones de escasa entidad.
  - Contenido: conservación, accesibilidad y certificado energético.
  - Estatal: RDL 7/2015, art. 29 (vivienda colectiva).
  - Fuentes: CAIB, sitio de avaluació d'edificis; BOE consolidado de la LUIB; Mallorca.com IT (09-10-2026).
- **Plazos:** el primer IEE, como tarde al cumplir 50 años; después, cada 10 años. Los defectos estructurales de la parte 1 deben corregirse en 6 meses, con informe final (Mallorca.com, fuente secundaria).
- **Palma:** tiene ordenanza propia de IEE (normatia, "evaluación edificios Palma", 2015). Su texto vigente y sus umbrales concretos están **NO VERIFICADOS**. Las ordenanzas municipales pueden fijar reglas más estrictas (CAIB). Marratxí y Eivissa tienen ordenanza propia.
- **Uso en HomeRadar:** si el año de construcción (Catastro) indica una antigüedad ≥ 50 años y el anuncio no menciona IEE, generar bandera Media de *due diligence* (pedir el IEE y las derramas previstas).

### C.3 Suelo rústico en Mallorca

- **Normas:**
  - Ley 6/1997 de suelo rústico de las Illes Balears.
  - LUIB, Ley 12/2017.
  - Plan Territorial de Mallorca (PTM).
  - Decreto Ley 9/2020.
- **Parcela mínima para vivienda nueva (Mallorca.com, 28-09-2026, secundaria):**
  - 14.000 m² en suelo rústico común.
  - 50.000 m² en suelo protegido o forestal.
  - En ANEI, AANP, APT y similares, la nueva construcción está prácticamente prohibida.
  - Ocupación ~2 % (Decreto Ley 9/2020).
- **Restablecimiento de la legalidad (LUIB, art. 196, texto consolidado BOE):**
  - **8 años** desde la finalización completa (196.1).
  - **No prescribe** (196.2) en **suelo rústico protegido** ni en bienes o espacios catalogados, BIC, parques, espacios libres, etc.
  - Infracciones graves y muy graves: prescriben a los 8 años (art. 205).
  - Corrección: una opinión de Hosteltur (2022) sostiene que no prescribe en todo el suelo rústico. **El texto consolidado vigente lo limita a suelo rústico *protegido*** y a los supuestos del 196.2.
- **Legalización extraordinaria ("amnistía"):**
  - Origen: Decreto Ley 3/2024 (24-05-2024), incorporado como **disposición adicional 7.ª de la Ley 7/2024, de 11 de diciembre**, de simplificación administrativa.
  - El **Consell de Mallorca** acordó aplicarla (Pleno de 13-02-2025; **BOIB 21, 15-02-2025**). Hay **3 años** desde esa publicación, es decir, aproximadamente hasta el 15-02-2028 [CÁLCULO PROPIO].
  - Requisito: la acción de restablecimiento debe estar prescrita.
  - Prestación económica: 10 %, 12,5 % o 15 % del coste de ejecución material, según el año de solicitud (año 1, 2 o 3).
  - Prohibición de uso turístico, inscrita en el Registro (PwC, 26-07-2024; Ley 4/2026, DF 19.ª: inscripción en 3 meses).
  - Exclusiones: dominio público, costas, carreteras, edificaciones sujetas a derribo o expropiación.
- **Hipoteca:** no se encontró ninguna fuente fiable que cuantifique la restricción bancaria (**NO VERIFICADO**). La tasación se condiciona o advierte si hay falta de documentación o disconformidad con el planeamiento (ECO/805/2003, arts. 10–11).
- **Palabras clave:**
  - Español: rústico, rústica, suelo rústico, finca rústica, terreno rústico, casa de campo, finca agrícola, possessió, fuera de ordenación, en proceso de legalización, legalizable, ANEI, ARIP, APR.
  - Català: rústic, rústica, sòl rústic, finca rústica, fora d'ordenació, legalitzable, possessió.
  - English: rustic land, rural land, country estate, agricultural land, finca, out of planning, legalisable.
  - Deutsch: rustikales Land, Außenbereich, Landgut, Finca auf rustikalem Land, landwirtschaftliche Fläche, nicht legalisiert, Legalisierung möglich.

### C.4 VPO, precio tasado y VPL en Baleares

- **VPO (vivienda protegida):**
  - **Reventa**: el precio máximo lo fija la Administración (Mallorca.com, 04-09-2026).
  - En segundas transmisiones, el comprador presenta una **declaración responsable** a la DG de Vivienda y Arquitectura en 30 días. **Sin ella no se puede elevar a escritura pública ni inscribir** (CAIB, sitio habitatgesprotegits).
  - El vendedor debe notificar al **IBAVI**: hay **derecho de adquisición preferente (tanteo y retracto)** a favor de la Administración durante el periodo de protección (CAIB).
  - La Ley 4/2026, DF 29.ª, añade que **todos los contratos de compraventa y alquiler serán visados**.
  - Duración de la protección (Mallorca.com, secundaria): 50 años antes del RDL 31/1978; 30 años hasta el 18/19-06-2018; **permanente** desde la Ley 5/2018.
- **Precios máximos de VPO de obra nueva, municipios del grupo A** (Palma, Calvià, Manacor, Llucmajor, Marratxí e Inca; Acuerdo del Consell de Govern de 20-02-2026, BOIB 24 de 21-02-2026, ficha CAIB). En € por m² útil:
  - Régimen especial: 1.973,74.
  - Régimen general: 2.105,32.
  - Concertado: 3.256,67.
  - Por calificación energética: A, 2.918,67; E, 2.383,76.
- **Brecha entre protegida y libre en Baleares:** 70,1 %, la mayor de España (OVS-MIVAU, T1 2026). Valor tasado de protegida: 1.155 €/m² frente a 3.886 libre (T1 2026).
- **VPL (Ley 3/2024):**
  - Útil ≤ 90 m².
  - Precio máximo: tipo 1, +10 % sobre VPO; tipo 2, +30 %.
  - **Régimen permanente**.
  - Comprador: residencia ≥ 5 años en Baleares, vivienda habitual, sin otra vivienda.
  - Inscripción en el registro autonómico antes de escriturar.
  - **Sin tanteo ni retracto** (CAIB, FAQ de VPL).
- **Relevancia:** para un comprador de primera vivienda puede ser una **oportunidad**, si cumple requisitos, o un **bloqueo**, si no los cumple, si el precio pedido supera el máximo legal ("sobreprecio" o "complemento por muebles", que es ilegal) o si el tanteo retrasa la operación.
- **Palabras clave:**
  - Español: VPO, V.P.O., vivienda protegida, protección oficial, HPO, precio tasado, precio limitado, VPL, IBAVI, régimen especial, régimen general, calificación definitiva.
  - Català: habitatge protegit, HPO, protecció oficial, preu taxat, preu limitat, IBAVI.
  - English: social housing, protected housing, price-controlled, VPO, subsidised housing.
  - Deutsch: Sozialwohnung, geförderte Wohnung, preisgebunden, VPO, Preisbindung.

### C.5 Especificidades de banco o servicer y otras situaciones jurídicas

Detalle y palabras clave en la sección D (catálogo). Hechos de base:
- **Sin posesión o sin llaves:** "No se puede visitar ni hipotecar". Pago al contado. Descuentos del 30–70 %. Procesos que tradicionalmente duraban 18–24 meses; la LO 1/2025 añade mediación previa (Brikex vía Europa Press, 18-09-2026).
- **Ocupado:** tasación imposible, compra al contado, desalojo de hasta ~2 años y ~5.000 € (respuesta en el foro de idealista/news, 06-11-2025, **fuente débil**). OCU (abr. 2025): ocupadas al 60 % de descuento.
- **Cesión de remate:** el comprador adquiere el derecho del adjudicatario de una subasta judicial. Suele ir sin posesión (Brikex).
- **Nuda propiedad:** el comprador no puede usar la vivienda hasta que se extinga el usufructo (muerte o plazo). Descuento del 20–50 % (Consumer, 2023/2025).
- **Con inquilino:** el comprador se subroga durante los primeros 5 años de contrato (7 si el arrendador era persona jurídica), según el art. 14 de la LAU (Legal Fournier, 14-07-2026).
- **Cargas y embargos:** se comprueban en la nota simple (idealista/news). El inmueble queda **afecto a las deudas de comunidad** de la parte vencida del año en curso y los 3 años naturales anteriores (LPH, art. 9.1.e). Sin certificado de deudas, el notario no autoriza salvo dispensa del comprador (Legálitas).
- **Proindiviso o cuota:** los bancos no suelen dar hipoteca normal por un porcentaje. Los copropietarios tienen tanteo y la salida es difícil (HelpMyCash, 29-09-2026).
- **Obra nueva sin final de obra o sin LPO:** riesgo de hipoteca condicionada o denegada, suministros provisionales e inscripción incompleta (TRLSRU, arts. 27–28; ECO/805/2003; Legal Fournier, 09-07-2026).
- **Piso turístico ilegal (Palma):**
  - Palma prohibía el alquiler turístico en plurifamiliares. La modificación del PGOU con aprobación inicial en **febrero de 2026** lo extiende a **todas las tipologías**, aunque su tramitación definitiva está **NO VERIFICADA**.
  - Las 4.473 plazas legales en 632 viviendas siguen operando.
  - Multas de hasta ~500.000 € tras el decreto de contención (+25 %) (El Debate, 05-02-2026).
  - La vivienda legalizada en rústico tiene prohibido el uso turístico (Ley 7/2024, DA 7.ª).

### C.6 Superficie anunciada frente a Catastro

- **Qué mide el Catastro:**
  - Mide la **construida**: útil + muros + parte proporcional de zonas comunes según coeficiente (hipotecas.me, 27-09-2026; Tinsa, glosario).
  - Terrazas cubiertas: 50 %; cubiertas por tres lados: 100 % (Bankinter).
- **Útil frente a construida:** diferencia "entre un 15 % y un 25 %" (idealista/news, 17-02-2023). No existe coeficiente oficial (hipotecas.me).
- **Tasación:** el tasador usa la superficie comprobada. Si no puede comprobarla, usa la **menor** entre catastral y registral (ECO/805/2003, art. 5, según hipotecas.me).
- **Fincas antiguas o rústicas:** discrepancias Catastro/Registro/realidad del 10–20 % "habituales" (Mallorca.com, 28-09-2026, secundaria).
- **Umbrales propuestos [JUICIO]** (comparar m² anunciados con la construida del Catastro):
  - Anuncio ≤ Catastro y ratio entre 0,75 y 1,05: **normal**. Puede ser útil (−15 a −25 %) o construida sin comunes.
  - Anuncio entre 1,05 y 1,15 × Catastro: **Baja**. Posible inclusión de terraza o parcela, o redondeo.
  - Anuncio > 1,15 × Catastro: **Media**. Posible ampliación no declarada o cerramiento de terraza sin licencia.
  - Anuncio > 1,30 × Catastro: **Alta**. Muy probable superficie no legalizada o error de anuncio; impacta en la tasación y en la cédula.
  - Anuncio < 0,70 × Catastro: **Baja**. Posible mala catalogación o que el anuncio sea solo útil interior; pedir aclaración.

---

## D. Catálogo de banderas rojas (para el motor de reglas)

Severidad:
- **Critical:** bloquea la compra como primera vivienda o la financiación hipotecaria estándar.
- **High:** riesgo jurídico o económico serio; requiere revisión profesional antes de las arras.
- **Medium:** comprobación documental obligatoria.
- **Low:** informativa.

| Código | Severidad | Descripción (por qué importa a un comprador de primera vivienda) | Palabras clave ES | CA | EN | DE | Fuente |
|---|---|---|---|---|---|---|---|
| `noPossession` | Critical | Sin posesión ni llaves: no se puede visitar, tasar ni hipotecar; solo al contado | sin posesión, sin llaves, no visitable, no se puede visitar, no hipotecable | sense possessió, sense claus, no visitable | no possession, no keys, cannot be viewed, no viewings | ohne Besitz, ohne Schlüssel, keine Besichtigung möglich | Brikex/Europa Press, 18-09-2026 |
| `occupied` | Critical | Ocupada sin título: desalojo largo (≈2 años, fuente débil), coste, daños; no tasable | ocupado, ocupada, okupado, okupas, ocupantes sin título, ocupación ilegal | ocupat, ocupada, okupes | occupied, squatters, illegally occupied | besetzt, Hausbesetzer, Okupas, illegal bewohnt | OCU, 04-2025; idealista/news foro, 11-2025 |
| `auctionAssignment` | Critical | Cesión de remate o subasta: compra de un derecho procesal, normalmente sin posesión | cesión de remate, cesion remate, subasta, adjudicación judicial, NPL, crédito hipotecario | cessió de remat, subhasta, adjudicació | auction assignment, court auction, NPL, loan sale | Zuschlagsabtretung, Zwangsversteigerung, NPL | Brikex/Europa Press, 18-09-2026 |
| `bareOwnership` | Critical | Nuda propiedad: no se puede habitar hasta que se extinga el usufructo | nuda propiedad, usufructo, usufructuario, usufructo vitalicio | nua propietat, usdefruit, usufructuari | bare ownership, usufruct, life interest | nacktes Eigentum, Nießbrauch, lebenslanges Wohnrecht | Consumer, 2023 (mod. 2025) |
| `undividedShare` | Critical | Proindiviso o cuota: sin hipoteca estándar, uso compartido, tanteo de copropietarios | proindiviso, pro indiviso, % de la propiedad, cuota indivisa, participación indivisa, mitad indivisa | proindivís, quota indivisa, meitat indivisa | undivided share, co-ownership share, fractional ownership, % share | Miteigentumsanteil, Bruchteilseigentum, ideeller Anteil | HelpMyCash, 29-09-2026 |
| `protectedRuralLand` | Critical | Edificación en suelo rústico protegido: la acción de restablecimiento no prescribe | ANEI, ARIP, APR, AANP, área natural, suelo rústico protegido | sòl rústic protegit, ANEI, ARIP | protected rural land, natural area | geschütztes Gebiet, Naturschutzgebiet | LUIB art. 196.2 (BOE consolidado) |
| `noHabitabilityCertificate` | Critical | Sin cédula: suministros definitivos, alquiler e hipoteca comprometidos; indicio de ilegalidad urbanística | sin cédula, no tiene cédula, sin habitabilidad, sin licencia de primera ocupación | sense cèdula, sense habitabilitat | no habitation certificate, no cedula, no occupancy licence | ohne Cédula, ohne Bewohnbarkeitsbescheinigung | Ley 5/2018 art. 16; iAhorro, 20-05-2026 |
| `habitabilityCertificatePending` | High | Cédula en trámite o caducada: condicionar arras a su obtención | cédula en trámite, cédula pendiente, cédula caducada, renovar cédula | cèdula en tràmit, cèdula pendent, cèdula caducada | cedula pending, cedula expired, in process | Cédula in Bearbeitung, Cédula abgelaufen | Ley 5/2018 art. 16; Mallorca.com, 28-09-2026 |
| `ruralLand` | High | Suelo rústico común: limitaciones de obra, legalizaciones y tasación; parcela mínima de 14.000 m² para vivienda nueva | rústico, rústica, suelo rústico, finca rústica, terreno rústico | rústic, rústica, sòl rústic, finca rústica | rustic land, rural land, agricultural land | rustikales Land, Außenbereich, landwirtschaftliche Fläche | Ley 6/1997; LUIB; Mallorca.com, 28-09-2026 |
| `outOfPlanning` | High | Fuera de ordenación o ampliación ilegal: obras limitadas, cédula y tasación afectadas | fuera de ordenación, ampliación sin licencia, anexo ilegal, legalizable, pendiente de legalizar, amnistía | fora d'ordenació, ampliació sense llicència, legalitzable | out of planning, unlicensed extension, legalisable | baurechtswidrig, ohne Baugenehmigung, legalisierbar | LUIB arts. 196 y 205; ECO/805 art. 11.b |
| `protectedHousingVpo` | High | VPO: precio máximo legal, requisitos de comprador, visado/declaración y tanteo del IBAVI | VPO, vivienda protegida, protección oficial, HPO, precio tasado, IBAVI, calificación definitiva | habitatge protegit, protecció oficial, HPO, preu taxat | social housing, protected housing, price-controlled | Sozialwohnung, geförderte Wohnung, Preisbindung | CAIB habitatgesprotegits; Ley 4/2026 DF 29.ª |
| `investorsOnly` | High | "Solo inversores": suele ocultar ocupación, inquilino, mal estado o falta de documentación | solo inversores, ideal inversores, exclusivo inversores, rentabilidad garantizada | només inversors, ideal inversors | investors only, investment opportunity only | nur für Investoren, Kapitalanlage only | OCU, 04-2025 (cargas y documentación) [criterio parcialmente JUICIO] |
| `cashOnly` | High | "Solo contado" o "no financiable": la vivienda no es tasable o hipotecable | solo contado, pago al contado, no financiable, no hipotecable, sin financiación | només al comptat, no finançable | cash only, cash buyers only, no mortgage, not mortgageable | nur Barzahlung, keine Finanzierung möglich, nicht finanzierbar | Brikex/Europa Press, 18-09-2026 |
| `tenantInPlace` | High | Con inquilino: subrogación de 5 años de contrato (7 si el arrendador era persona jurídica); no se puede entrar a vivir | con inquilino, alquilado, arrendado, con contrato de alquiler, rentabilidad actual, vendido con inquilino | amb llogater, llogat, arrendat | tenanted, sitting tenant, sold with tenant, rented out | vermietet, mit Mieter, Mieter vorhanden | LAU art. 14 (Legal Fournier, 14-07-2026) |
| `seizure` | High | Embargo anotado: hay que cancelarlo antes o en la firma | embargo, embargado, anotación de embargo, procedente de embargo | embargament, embargat | seizure, lien, attachment, repossessed | Pfändung, gepfändet, Zwangsvollstreckung | idealista/news (cargas en nota simple) |
| `newBuildNoCompletion` | High | Obra nueva sin final de obra, sin LPO o sin cédula de primera ocupación | sin final de obra, obra sin terminar, sin licencia de primera ocupación, pendiente de LPO, en construcción | sense final d'obra, obra sense acabar | no completion certificate, unfinished, off-plan pending licence | ohne Bauabnahme, unfertig, ohne Erstbezugsgenehmigung | TRLSRU arts. 27–28; Legal Fournier, 09-07-2026 |
| `illegalTouristRental` | Medium | Uso turístico sin licencia o anunciado como "rentable en alquiler vacacional": Palma no admite nuevas plazas; multas de hasta ~500.000 € | alquiler vacacional, alquiler turístico, ETV sin licencia, ideal Airbnb, apto turístico | lloguer turístic, lloguer vacacional, sense llicència ETV | holiday rental potential, Airbnb, tourist rental without licence | Ferienvermietung, ohne Ferienlizenz, Airbnb-tauglich | El Debate, 05-02-2026 |
| `charges` | Medium | Cargas genéricas (hipoteca, afecciones, servidumbres): pedir nota simple | cargas, con cargas, gravámenes, afección fiscal, servidumbre, hipoteca pendiente | càrregues, gravàmens, servitud | encumbrances, charges, easement | Belastungen, Grundschulden, Dienstbarkeit | idealista/news (nota simple) |
| `communityDebts` | Medium | Deudas de comunidad o derramas: el inmueble responde de la parte vencida del año en curso y los 3 años anteriores | deudas comunidad, derrama, derrama pendiente, cuotas pendientes | deutes comunitat, derrama | community debts, special levy, pending levy | Hausgeldschulden, Sonderumlage | LPH art. 9.1.e (Legálitas) |
| `priceLimitedHousingVpl` | Medium | VPL: régimen permanente, requisitos de residencia de 5 años; sin tanteo | VPL, vivienda de precio limitado, precio limitado | habitatge de preu limitat, HPL | price-limited housing | preisbegrenzte Wohnung | CAIB, FAQ de VPL; Ley 3/2024 |
| `buildingOver50NoIee` | Medium | Edificio de 50 años o más sin mención de IEE: riesgo de derramas y obras obligatorias | (por dato Catastro: año de construcción) ITE, IEE, inspección técnica, ITE desfavorable | IAE, ITE, informe d'avaluació | building inspection, ITE failed | Gebäudeinspektion, ITE | LUIB art. 125 (Ley 4/2026); CAIB |
| `areaMismatchHigh` | High | m² anunciados > 1,30 × construida en Catastro | (cálculo) | — | — | — | [JUICIO] sobre idealista/news 2023 y hipotecas.me 2026 |
| `areaMismatch` | Medium | m² anunciados entre > 1,15 y ≤ 1,30 × Catastro | (cálculo) | — | — | — | Ídem |
| `areaMismatchLow` | Low | Ratio entre 1,05 y 1,15, o < 0,70 | (cálculo) | — | — | — | Ídem |

**Notas de implementación [JUICIO]:**
- Evaluar las banderas sobre el título y la descripción normalizados (minúsculas, sin diacríticos, apóstrofos unificados). Usar límites de palabra para evitar falsos positivos:
  - "ocupado" puede aparecer en "plaza de garaje ocupada"; requerir contexto de vivienda.
  - "rústico" puede referirse a un estilo decorativo ("estilo rústico", "decoración rústica"). Usar una lista de exclusión y priorizar "suelo rústico", "finca rústica" o el dato catastral de clase de suelo.
- Las banderas Critical desactivan el estimador de margen (regla R12) y penalizan fuertemente el *opportunity score*.
- `sellerType = bankServicer` no es una bandera por sí mismo, pero activa la búsqueda reforzada de `noPossession`, `occupied`, `cashOnly`, `investorsOnly` y `auctionAssignment`.

---

## E. Fuentes (consultadas el 2026-11-15)

**Negociación y mercado**
1. idealista/news, "Spain's housing price negotiation margin hits historic lows" (Tecnocasa-UPF), 05-03-2026: https://www.idealista.com/en/news/node/885027
2. idealista/news, etiqueta "rebajas de precios" (rebaja media del 7 % T1 2026, 25-06-2026; 14 % de anuncios rebajados, 26-05-2026): https://www.idealista.com/news/etiquetas/rebajas-de-precios
3. idealista/news, "Spanish property sellers cut asking prices in 2026", 05-06-2026: https://www.idealista.com/en/news/node/899912
4. idealista/news, margen de negociación en mínimos (Tecnocasa), 18-02-2026: https://www.idealista.com/news/taxonomy/term/71135
5. Mallorca.com / Europa Press, "Property price cuts Balearics", 24-09-2026: https://www.mallorca.com/en/magazin/news/property-price-cuts-balearics
6. Diari de Tarragona, rebajas por provincias (idealista, T2 2026), 24-09-2026: https://www.diaridetarragona.com/vivienda/271418/16-viviendas-venta-tarragona-rebaja-precio-inicial.html
7. Bolsamanía, Índice de Negociación de Fotocasa S1 2026, 17-06-2026: https://www.bolsamania.com/noticias/economia/negociar-el-precio-de-la-vivienda-estas-son-las-ciudades-espanolas-con-mas-margen--22760416.html
8. Brainsre, Índice de Negociación de Fotocasa S2 2025, 04-12-2025: https://brainsre.news/ciudades-margen-negociar-precio-compra-vivienda/
9. Spanish Property Insight, pisos.com rebajas 2025, 15-02-2026: https://www.spanishpropertyinsight.com/2026/02/15/less-than-10pc-of-listings-cut-their-asking-price-in-2025/
10. El Español, Alfa Inmobiliaria 24 %→38 %, 11-08-2026: https://www.elespanol.com/sociedad/20260811/antonio-perez-viviendas-vendidas-descuentos-pasado-solo-ano/1003744340132_0.html
11. Brainsre, UVE Valoraciones, brecha de hasta el 44 %, 28-10-2025: https://brainsre.news/divergencia-44-precios-oferta-declarados-vivienda/
12. The Corner, brecha del 29 % (sin fecha): https://thecorner.eu/?p=127440
13. Merca2, FAI, tiempos de venta, 15-07-2026: https://www.merca2.es/2026/07/15/negociacion-compraventa-vivienda-2026-2417810/
14. Mallorca.com, Palma T2 2026 (Gloval), 01-10-2026: https://www.mallorca.com/en/magazin/news/palma-home-prices-sales-fall
15. Noudiari, oferta Baleares T1 2026 (idealista), 14-05-2026: https://www.noudiari.es/?p=545572

**Bancos y servicers**

16. OCU, "¿Pisos baratos de bancos?", 04-04-2025: https://www.ocu.org/fincas-y-casas/compraventa/compraventa/analisis/2025/04/ofertas-pisos-de-bancos
17. Brainsre, campaña Solvia de 1.400 activos, 21-10-2025: https://brainsre.news/solvia-campana-descuentos-1400-inmuebles-espana/
18. Valencia Plaza, campaña Servihabitat, 05-02-2024: https://valenciaplaza.com/valenciaplaza/servihabitat-lanza-una-campana-de-mas-de-6000-inmuebles-con-hasta-un-45-de-descuento
19. Brainsre, Diglo, 800 inmuebles, hasta el 60 % (2023, act. 2026): https://brainsre.news/diglo-venta-800-inmuebles-descuentos-60/
20. Brainsre, Diglo, hasta el 45 %, 06-09-2022: https://brainsre.news/diglo-rebajas-500-viviendas-45-descuento/
21. Diglo, bases legales de la campaña de rebajas de mayo de 2025: https://integracion.digloservicer.com/sites/default/files/LEGAL/2025/BASES%20LEGALES%20REBAJAS%20MAYO%202025%20DEF.pdf
22. Diario de León / Europa Press, viviendas "sin llaves" (Brikex), 18-09-2026: https://www.diariodeleon.es/nacional/260918/2097765/comprar-piso-150-000-euros-60-000-oportunidad-riesgo-viviendas-llaves.html
23. idealista/news, foro "comprar una vivienda okupada", 06-11-2025: https://www.idealista.com/news/node/861461

**Benchmarks €/m²**

24. AHE, "Precios 1T 2026" (MIVAU: Palma 3.789, Baleares 3.886): https://ahe.es/estadisticas/indicadores-inmobiliarios/archivos/precios-1trim-2026.pdf
25. IBESTAT, JSON del valor tasado (T2 2026): https://intranet.caib.es/ibestat-jaxi/tabla.do?typeDownload=7&px=d64a71a9-7818-4329-bd1d-9e366ed62ad2
26. Catálogo de datos abiertos de la CAIB, ficha IBESTAT: https://intranet.caib.es/opendatacataleg/es/dataset/valor-tasado-de-la-vivienda-libre-por-periodo-y-antiguedad-desde-1995-primer-trimestre-a-2025-s
27. INE, ficha IOE 20007 (valor tasado; enlace al MIVAU): https://ine.es/dyngs/IOE/en/operacion.htm?numinv=20007
28. datos.gob.es, valor tasado por municipios >25.000 hab.: https://datos.gob.es/en/catalogo/a13002908-valor-tasado-de-la-vivienda-libre-por-municipios-de-mas-de-25-00-habitantes-total-base-20054
29. ICANE, recurso de valor tasado (XLS): https://datos.icane.es/id/dataset/valor-tasado-vivienda/resource/7afbede1-f603-464b-9ce2-8632c61040f1
30. Registradores, ERI 2T 2026: https://www.registradores.org/documents/d/guest/eri_2t_2026
31. Tinsa, IMIE Palma (T3 2026): https://www.tinsa.es/precio-vivienda/islas-baleares/palma-de-mallorca/
32. Tinsa, nota de prensa IMIE 2T 2026, 30-06-2026: https://www.tinsa.es/sala-de-prensa/notas-de-prensa/imie_2t2026/
33. Spanish Property Insight, portal de notarios, 27-10-2025: https://www.spanishpropertyinsight.com/2025/10/27/notaries-launch-new-portal-giving-deeper-insight-into-spains-property-market/
34. Spanish Property Insight, limitaciones del portal de notarios, 27-08-2026: https://www.spanishpropertyinsight.com/2026/08/27/the-notaries-recently-launched-data-portal-is-useful-but-it-still-wont-tell-you-what-a-property-actually-sold-for/
35. Vozpópuli, primer año del Portal del Notariado, 28-10-2026: https://www.vozpopuli.com/economia/el-portal-estadistico-del-notariado-supera-el-medio-millon-de-usuarios-en-su-primer-ano.html
36. Vozpópuli, Baleares en la década 2016–2025 (Notariado), 21-07-2026: https://www.vozpopuli.com/economia/las-islas-baleares-acaparan-el-listado-de-los-precios-medios-de-vivienda-mas-elevados-de-la-ultima-decada.html
37. Última Hora, OVS-MIVAU T1 2026, brecha protegida/libre, 22-07-2026: https://www.ultimahora.es/noticias/local/2026/07/22/2675205/baleares-lidera-brecha-entre-vivienda-protegida-libre-cuesta-menos-primera-segunda.html
38. Última Hora, valor tasado Palma 3.337, 20-10-2025: https://www.ultimahora.es/noticias/local/2025/10/20/2488593/vivienda-acelera-subida-baleares-revaloriza-mas-decada.html
39. Menorca.info / Europa Press, Fotocasa julio 2026 por municipios, 03-08-2026: https://www.menorca.info/balears/noticias/2026/08/03/2682379/vivienda-segunda-mano-sube-media-26500-euros-ano-baleares.html
40. Forbes, Fotocasa Baleares, junio 2026, 21-07-2026: https://forbes.es/?p=983627
41. Ara Balears, Registradores T3 2025, 06-01-2026: https://en.arabalears.cat/society/the-actual-price-recorded-in-property-sales-in-the-balearic-islands-no-longer-falls-below-an-average-of-4-000-euros_1_5610278.html

**Legal**

42. BOE, Ley 5/2018 de vivienda de las Illes Balears (consolidada): https://www.boe.es/buscar/pdf/2018/BOE-A-2018-9774-consolidado.pdf
43. BOE, Ley 12/2017 LUIB (consolidada; arts. 125, 196 y 205): https://www.boe.es/buscar/pdf/2018/BOE-A-2018-806-consolidado.pdf
44. BOE, Ley 4/2026, de 11 de junio (consolidada): https://www.boe.es/buscar/pdf/2026/BOE-A-2026-15579-consolidado.pdf
45. Derecho Local, ficha de la Ley 4/2026: https://derecholocal.es/novedad-legislativa/ley-de-aceleracion-de-proyectos-estrategicos-y-simplificacion-administrativa-de-balears
46. BOE, Decreto-ley 8/2025 (derogado por la Ley 4/2026): https://boe.es/buscar/pdf/2026/BOE-A-2026-487-consolidado.pdf
47. BOE, Orden ECO/805/2003 (consolidada): https://www.boe.es/buscar/pdf/2003/BOE-A-2003-7253-consolidado.pdf
48. Derecho Local, aplicación en Mallorca de la legalización extraordinaria (BOIB 21/2025): https://derecholocal.es/novedad-legislativa/aplicacion-en-mallorca-del-procedimiento-de-legalizacion-extraordinaria-de-edificaciones-construcciones-instalaciones-y-usos-existentes-en-suelo-rustico
49. PwC Periscopio, legalización extraordinaria en suelo rústico, 26-07-2024: https://periscopiofiscalylegal.pwc.es/legalizacion-extraordinaria-de-edificaciones-y-actividades-en-suelo-rustico-en-las-islas-baleares/
50. CAIB, Avaluació d'edificis (IEE): https://caib.es/sites/avaluacioedificis/es/
51. CAIB, segundas y posteriores transmisiones de vivienda protegida: https://www.caib.es/sites/habitatgesprotegits/ca/segones_i_posteriors_transmissions/
52. CAIB, FAQ de vivienda de precio limitado: https://www.caib.es/sites/habitatge_de_preu_limitat/es/faqs/
53. CAIB, ficha de precios máximos de VPO de obra nueva (BOIB 24/2026): https://www.caib.es/seucaib/ca/arxiuServlet?id=6497813
54. Mallorca.com, cédula de habitabilidad en Mallorca, act. 28-09-2026: https://www.mallorca.com/es/consejos/inmobiliaria/construir-reformar/cedula-de-habitabilidad-mallorca
55. Mallorca.com, IEE/ITE en Mallorca, act. 09-10-2026: https://www.mallorca.com/it/consigli/immobili/costruire-ristrutturare/iee-ite-mallorca-obbligatorio
56. Mallorca.com, finca en suelo rústico, act. 28-09-2026: https://www.mallorca.com/es/consejos/inmobiliaria/comprar/finca-suelo-rustico-mallorca
57. Mallorca.com, prescripción de construcción ilegal, act. 04-09-2026: https://www.mallorca.com/es/consejos/inmobiliaria/construir-reformar/prescripcion-construccion-ilegal-mallorca
58. Mallorca.com, vender o descalificar VPO, act. 04-09-2026: https://www.mallorca.com/es/consejos/inmobiliaria/comprar/vender-descalificar-vpo-mallorca
59. Hosteltur Comunidad (Monlex), rústicos en Mallorca, 02-05-2022: https://hosteltur.com/comunidad/005047_la-compraventa-de-inmuebles-rusticos-en-mallorca-cuestiones-urbanisticas-clave.html
60. iAhorro, hipoteca sin cédula, 20-05-2026: https://www.iahorro.com/faqs/se-puede-firmar-hipoteca-sin-cedula-habitalidad
61. Legal Fournier, comprar piso con inquilino, 14-07-2026: https://legalfournier.com/?p=4968
62. Legal Fournier, vivienda sin licencia de primera ocupación, 09-07-2026: https://legalfournier.com/es/comprar-vivienda-espana/vivienda-sin-licencia-primera-ocupacion/
63. Consumer, nuda propiedad, 2023 (mod. 2025): https://www.consumer.es/?p=435399
64. HelpMyCash, comprar una habitación o cuota, 29-09-2026: https://www.helpmycash.com/hipotecas/comprar-habitacion/
65. idealista/news, cómo saber si una vivienda tiene cargas, 2013 (act. 2023): https://www.idealista.com/news/node/592215
66. Legálitas, deudas de comunidad (LPH art. 9.1.e), 2014 (rev. 2016): https://www.legalitas.com/actualidad/quien-paga-las-deudas-pendientes-de-la-comunidad-en-la-compr-
67. El Debate, Palma prohíbe nuevas plazas turísticas, 05-02-2026: https://www.eldebate.com/espana/baleares/20260205/palma-prohibe-nuevas-licencias-alquiler-turistico-pacificar-barrios_381587.html
68. idealista/news, superficie útil frente a construida, 17-02-2023: https://www.idealista.com/news/node/804021
69. hipotecas.me, superficie catastral, 27-09-2026: https://www.hipotecas.me/consulta-catastral/superficie-catastral
70. Tinsa, glosario "superficie construida con zonas comunes": https://www.tinsa.es/glosario-inmobiliario/superficie-construida-zonas-comunes/
71. Bankinter, útiles frente a construidos: https://www.bankinter.com/banca/preguntas-frecuentes/hipotecas/que-diferencia-hay-entre-metros-utiles-y-metros-construidos-al-comprar-una-casa-o-valorarla

---

## F. Lista NO VERIFICADO

| # | Elemento | Motivo |
|---|---|---|
| 1 | Valor tasado del MIVAU para **Calvià, Manacor, Llucmajor, Marratxí e Inca** (último trimestre) | El dominio del MIVAU/MITMA y el Excel municipal están bloqueados desde el entorno de investigación; no se encontró ninguna fuente secundaria con esas cifras |
| 2 | Valor tasado del MIVAU para **Palma, T2 y T3 2026** | El PDF AHE 2T 2026 no es accesible; solo se verificó T1 2026 |
| 3 | **Nombre exacto del fichero XLS** municipal del MIVAU | Ídem; se documenta solo la página índice |
| 4 | Calendario exacto de publicación del valor tasado | La página del calendario ministerial no se consultó |
| 5 | **Exportación o API** del Portal del Notariado y €/m² actual de Palma y los municipios | Dominio bloqueado. Ninguna fuente menciona descarga ni API, lo que no prueba que no existan. Revisar manualmente las condiciones de uso |
| 6 | **Tasa de aceptación de ofertas por debajo del precio** en Solvia, Aliseda, Diglo/Servihabitat, Haya e Hipoges, y descuento medio real de cierre | Ninguna fuente pública lo cuantifica; solo hay máximos de campaña "hasta X %". Diglo admite ofertas en otras condiciones (bases 2025) sin cifra |
| 7 | Campañas de servicers con stock en **Baleares** | Ninguna de las campañas revisadas menciona Baleares |
| 8 | Relación cuantitativa entre **días en mercado** y descuento de cierre en España o Mallorca | No se encontró ningún estudio; la regla R5 es juicio |
| 9 | Tiempo medio de venta en **Palma o Mallorca** 2025–2026 | La fuente (Mallorcadiario, "ventas exprés en Palma") no se pudo descargar |
| 10 | % de rebaja media de Palma en T1 2026 (solo 70.950 €) | idealista no publicó el porcentaje en el extracto disponible |
| 11 | Fecha y metodología de la brecha del 29 % (spainhousing.eu) | Página sin fecha |
| 12 | Vigencia de **10 años** de la cédula según el texto oficial (Decreto 145/1997) | Solo hay confirmación en una fuente secundaria (Mallorca.com) |
| 13 | Existencia de un nuevo decreto de habitabilidad 2024–2026 o de cambios en la cédula por la Ley 4/2026 | No hallados en las partes revisadas del texto consolidado; la revisión de disposiciones no fue exhaustiva al 100 % |
| 14 | Umbrales concretos de la **ordenanza IEE de Palma** | Texto municipal no consultado |
| 15 | Parcela mínima de 14.000 m² y 50.000 m² y ocupación del 2 % en rústico | Fuente secundaria (Mallorca.com); falta cotejo con el PTM y el DL 9/2020 |
| 16 | Restricción bancaria cuantificada para hipotecas en suelo rústico | No hay fuente fiable |
| 17 | Aprobación definitiva de la modificación del PGOU de Palma (feb. 2026) que prohíbe nuevas plazas turísticas en unifamiliares | Solo consta la aprobación inicial |
| 18 | Precio máximo de **reventa** de VPO (fórmula para segundas transmisiones) | La página CAIB de precios máximos de reventa no se consultó |
| 19 | Plazos de desalojo (≈2 años, ≈5.000 €) | Fuente débil (respuesta en un foro) |
| 20 | Todos los pesos de la tabla A.4 marcados [JUICIO] y los umbrales de superficie de C.6 | Son propuestas de diseño, no datos |
