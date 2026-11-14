# Fuentes públicas de enriquecimiento — Mallorca

Fecha del informe: **2026-11-14** (segunda pasada, con verificación en línea).

> **Estado de verificación.** Cada afirmación lleva una de estas marcas:
> - **verificado 2026-11-14** — página/servicio abierto hoy; se cita la URL.
> - **NO VERIFICADO: <motivo>** — no se pudo abrir (proxy con lista blanca, `robots.txt`, o sin presupuesto); tratar como hipótesis hasta comprobar.
>
> Limitación de esta sesión: el proxy sólo permite abrir URLs que hayan aparecido antes en resultados de búsqueda; `subastas.boe.es`, `clientebancario.bde.es`, `data.ecb.europa.eu` y `goveasy.eu` rechazan robots.

---

## 1. Catastro (Dirección General del Catastro)

### 1.1 Servicios OVC "libres" (sin registro, sin clave)

La ayuda de la Sede distingue **datos no protegidos** (consulta libre: todo lo que no sea titular ni valor catastral) y **datos protegidos** (requieren certificado X.509, Cl@ve o DNI + nº de soporte) — verificado 2026-11-14, `https://www.catastro.hacienda.gob.es/ayuda/default.htm`. La página oficial de esquemas lista los servicios de **acceso libre** y los de **acceso restringido** — verificado 2026-11-14, `https://www.catastro.hacienda.gob.es/ws/esquemas.htm`:

| Servicio | Protocolo | Endpoint | Estado |
|---|---|---|---|
| Callejero y bienes inmuebles (`Consulta_DNPRC`, `Consulta_DNPLOC`, `Consulta_DNPPP`, `ObtenerProvincias/Municipios/Callejero`) | WCF (XML y JSON) | `http://ovc.catastro.meh.es/OVCServWeb/OVCWcfCallejero/COVCCallejero.svc` | Base verificada 2026-11-14 (esquemas.htm). Sufijo `/json/Consulta_DNPRC?Provincia=&Municipio=&RC=` **NO VERIFICADO: el proxy no permitió abrir el .svc; los nombres de operación proceden de los XSD `ovc_consulta_*` y del conocimiento previo** |
| Conversión de coordenadas (`Consulta_RCCOOR`, `Consulta_RCCOOR_Distancia`, `Consulta_CPMRC`) | WCF (XML y JSON) | `http://ovc.catastro.meh.es/OVCServWeb/OVCWcfCallejero/COVCCoordenadas.svc` | Base verificada 2026-11-14 (esquemas.htm); sufijo `/json/` NO VERIFICADO (mismo motivo) |
| Mismas operaciones de coordenadas | SOAP/GET ASMX | `https://ovc.catastro.meh.es/ovcservweb/OVCSWLocalizacionRC/OVCCoordenadas.asmx/Consulta_RCCOOR?SRS=EPSG:4326&Coordenada_X=<lon>&Coordenada_Y=<lat>`; `…/Consulta_CPMRC?Provincia=&Municipio=&SRS=&RC=`; `…?op=Consulta_RCCOOR_Distancia` | verificado 2026-11-14 vía código fuente del paquete CRAN CatastRo: `https://rdrr.io/cran/CatastRo/src/R/ovc-rccoor.R`, `https://rdrr.io/cran/CatastRo/src/R/ovc_cpmrc.R`, `https://rdrr.io/cran/CatastRo/src/R/docs.R` |
| Códigos de provincia/municipio | ASMX | `https://ovc.catastro.meh.es/ovcservweb/ovcswlocalizacionrc/ovccallejerocodigos.asmx?op=ConsultaProvincia` / `?op=ConsultaMunicipioCodigos` | verificado 2026-11-14 (docs.R de CatastRo) |
| Consulta de datos catastrales / certificaciones / titularidad | WCF **restringido** | `https://serwebr.sedecatastro.gob.es/ServiciosWCF/OVCWcfConsulta/OVCConsulta.svc` (y `OVCWcfCDYG`, `OVCWcfCerTitular`, `OVCWcfCSV`) | verificado 2026-11-14, `https://www.catastro.hacienda.gob.es/ws/WCFservices_Consulta.pdf`: exige registro en la SEC, datos protegidos sólo del ámbito asignado, error si > 6.000 inmuebles, CSV fallidos limitados por IP/día. **No aplicable a HomeRadar** |

Campos de respuesta confirmados (coordenadas, XML `consulta_coordenadas → coordenadas → coord`): `pc.pc1`+`pc.pc2` (RC 14), `ldt` (dirección literal), `geo.xcen`, `geo.ycen`, `geo.srs`, `cmun_ine`, `dt.loine.cp/cm`, `dt.lourb.dir.cv/pnp`, `dis` (distancia en `_Distancia`); errores en `lerr` (p. ej. código 11 "LA PROVINCIA ES OBLIGATORIA") — verificado 2026-11-14, `https://stat.ethz.ch/CRAN/web/packages/CatastRo/vignettes/ovcservice.Rmd`. Campos de `Consulta_DNPRC` (`luso`, `sfc`, `ant`, `cn`, `bico/bi`): **NO VERIFICADO: no se abrió una respuesta real; los nombres provienen del esquema LDC `https://www.catastro.hacienda.gob.es/ws/esquemas/ldc.xsd` (no descargado hoy) y del conocimiento previo.**

Notas de uso:
- Cuota: ninguna cifra publicada para los servicios libres — NO VERIFICADO (la licencia INSPIRE §1.2 sí prevé suspensión automática por intensidad). Mantener ≤ 1 req/s, caché por RC, backoff.
- Las RC de 20 caracteres identifican la unidad; las de 14 la parcela. Flujo realista: **coordenadas → `Consulta_RCCOOR` → RC(14) → `Consulta_DNPRC` → unidades**, y casar por planta/superficie con el anuncio (flujo inferido, no verificado como tal).

### 1.2 INSPIRE (WFS / ATOM / WMS) — verificado 2026-11-14, `https://www.catastro.hacienda.gob.es/webinspire/index.html`

| Conjunto | Servicio | URL | Notas |
|---|---|---|---|
| Parcelas (CP) | ATOM | `https://www.catastro.hacienda.gob.es/INSPIRE/CadastralParcels/ES.SDGC.CP.atom.xml` | un GML/ZIP por municipio; "los datos se actualizan 2 veces al año" |
| Edificios (BU) | ATOM | `https://www.catastro.hacienda.gob.es/INSPIRE/buildings/ES.SDGC.BU.atom.xml` | ídem |
| Direcciones (AD) | ATOM | `https://www.catastro.hacienda.gob.es/INSPIRE/Addresses/ES.SDGC.AD.atom.xml` | ídem |
| Parcelas / Direcciones / Edificios | WFS 2.0 | `http://ovc.catastro.meh.es/INSPIRE/wfsCP.aspx?`, `…/wfsAD.aspx?`, `…/wfsBU.aspx?` | nombres de feature type (`CP:CadastralParcel`, `BU:Building`…) y límites de BBOX/`count`: NO VERIFICADO (no se abrió GetCapabilities) |
| Cartografía INSPIRE | WMS | `http://ovc.catastro.meh.es/cartografia/INSPIRE/spadgcwms.aspx` | "información actualizada tal y como es mostrada en la Sede" |

Licencia — verificado 2026-11-14, `https://www.catastro.hacienda.gob.es/webinspire/documentos/Licencia.pdf`: licencia automática al usar el servicio; se permite descarga masiva y uso de servicios; **uso público/comercial sólo de productos transformados** (no redistribuir ni publicar el dato original, no llamarlos "información catastral"); el servicio puede suspenderse automáticamente por intensidad/frecuencia que degrade el servicio. Para HomeRadar: cargar ATOM de la provincia 07 en PostGIS y exponer sólo datos derivados.

### 1.3 Campos que podemos poblar desde Catastro

`referencia_catastral`, dirección literal y centroide (verificado vía `Consulta_RCCOOR`/`CPMRC`), geometría y `areaValue` de parcela (ATOM CP), y — pendientes de verificar en respuesta real — `superficie_construida`, `anio_construccion`, `uso`, `clase UR/RU`, `numberOfDwellings`, plantas (DNPRC / BU). **No** se obtiene valor catastral ni titular (datos protegidos, verificado 2026-11-14).

### 1.4 Valor de referencia — veredicto

1. Ruta oficial: `https://www1.sedecatastro.gob.es/Accesos/SECAccvr.aspx` ("VALOR DE REFERENCIA", destacados del mapa web) — verificado 2026-11-14, `https://www.catastro.hacienda.gob.es/ayuda/mapaweb.htm`. La página en sí no se pudo abrir (proxy).
2. **Requiere identificación**: el tutorial paso a paso indica acceso "como ciudadano" con datos del DNI, certificado digital o Cl@ve permanente, introducir finalidad, RC, NIF del consultante, fecha y reCAPTCHA; el resultado muestra el valor y permite obtener certificado — verificado 2026-11-14 (fuente secundaria), `https://www.xataka.com/basics/como-saber-valor-casa-piso-valor-referencia-catastro`. Que pueda consultarse **cualquier** inmueble: las FAQ dicen que "la Sede permite a cualquier persona consultar y certificar el valor de referencia de un inmueble a una fecha"; el certificado de motivación sólo para titulares — verificado 2026-11-14, `https://www.fiscal-impuestos.com/preguntas-frecuentes-valor-referencia-catastral.html`.
3. **No existe API ni servicio OVC** del valor de referencia: ninguno aparece en `esquemas.htm` ni en el PDF de servicios WCF (verificado 2026-11-14). Automatizar la consulta con identificación personal + reCAPTCHA → **descartado**.
4. **Mapas de valores** e **Informe anual del mercado inmobiliario**: publicados en la Sede por mandato legal; el mapa rústico se define por "ámbitos territoriales homogéneos" y módulos de valor medio — verificado 2026-11-14, `https://www.catastro.hacienda.gob.es/ayuda/mapamodulosrustica.htm`. Acceso **sin identificación** y formato descargable (tablas/PDF): NO VERIFICADO (la página de ayuda no lo indica; no se pudo abrir la Sede).
5. Implicación: el valor de referencia entra en HomeRadar sólo como (a) estimación por módulos si los mapas resultan descargables, o (b) campo **manual** que el usuario rellena tras consultarlo él mismo.

---

## 2. Euríbor

| Fuente | Qué publica | Acceso máquina | Estado |
|---|---|---|---|
| **Banco de España — Boletín Estadístico, cuadro 19.1** "Tipos de interés legales, euríbor, míbor y otros tipos oficiales de referencia" | Euríbor 1 semana/1/3/6/12 m (media mensual), €STR compuesto, IRS 5 años, Míbor | **CSV** `https://www.bde.es/webbe/es/estadisticas/compartido/datos/csv/be1901.csv`; XLSX `…/xlsx/be1901.xlsx`; PDF `…/pdf/a1901.pdf` | verificado 2026-11-14, índice en `https://www.bde.es/webbe/es/estadisticas/otras-clasificaciones/publicaciones/boletin-estadistico/capitulo-19.html` |
| Formato del CSV | 6 filas de cabecera (`CÓDIGO DE LA SERIE`, `NÚMERO SECUENCIAL`, `ALIAS`, `DESCRIPCIÓN`, `UNIDADES`, `FRECUENCIA`) + una fila por mes `"OCT 2026",…`; decimal `.`; `_` = sin dato | Serie Euríbor 12 m: código **`D_1NBAF472`**, nº secuencial `587853`, alias **`BE_19_1.5`**, "Tipos de interés. UEM. Mercado monetario. Euríbor. A 12 meses", mensual | verificado 2026-11-14 (lectura directa del CSV) |
| Cuadro 19.2 | Tipos de referencia **no oficiales** y otros | `…/csv/be1902.csv` | verificado 2026-11-14 (sólo existencia) |
| Nota de prensa mensual del BdE | Tipos oficiales del mes | `https://www.bde.es/f/webbe/GAP/Secciones/SalaPrensa/NotasInformativas/26/presbe2026-104.pdf` | verificado 2026-11-14 |
| **BOE** | Resolución mensual del BdE | API de datos abiertos del BOE: NO VERIFICADO (no se buscó hoy) | — |
| **ECB Data Portal** (dataset `FM`) | Serie `FM.M.U2.EUR.RT.MM.EURIBOR1YD_.HSTA` (Euríbor 1 año, media mensual, fuente Refinitiv) | Clave de serie verificada 2026-11-14 vía espejo DBnomics `https://db.nomics.world/ECB/FM/M.U2.EUR.RT.MM.EURIBOR1YD_.HSTA` (último dato 2026-09 = 3,247). URL SDMX `https://data-api.ecb.europa.eu/service/data/FM/M.U2.EUR.RT.MM.EURIBOR1YD_.HSTA?format=csvdata`: NO VERIFICADO (data.ecb.europa.eu bloquea robots) | Segunda fuente |
| **EMMI** | Administrador; diario con retraso para no suscritos | NO VERIFICADO hoy | No usar |

**Valores oficiales verificados 2026-11-14** (nota BdE de 3-nov-2026 y cuadro 19.1): **Euríbor 12 m octubre 2026 = 3,239 %**; septiembre 2026 = 3,247 %; agosto 2026 = 2,954 %. Resto octubre 2026: 1 sem 2,415 %, 1 m 2,436 %, 3 m 2,635 %, 6 m 2,975 %; IRS 5 años 3,455 %; €STR 1 año 2,077 % (ojo: el cuadro 19.1 tiene **dos** bloques de columnas; el segundo es €STR, no Euríbor). El worker debe leer `be1901.csv`, filtrar la columna cuyo código sea `D_1NBAF472` y guardar `(mes, valor, fuente, fecha_descarga)`.

---

## 3. Estadísticas de precios

| Fuente | Granularidad | Descarga / API | Estado |
|---|---|---|---|
| **Portal Estadístico del Notariado** `https://www.penotariado.com` (lanzado oct-2025) | €/m², importe medio, superficie y nº de compraventas por CCAA, provincia, **municipio y código postal**; filtros por tipo, edad, nacionalidad; actualización mensual (últimos 12 meses) | Sólo visor web; **ningún export CSV/Excel ni API documentado** (existe al menos un scraper de terceros en Apify, señal de ausencia de API) | verificado 2026-11-14 (fuentes secundarias): `https://www.vozpopuli.com/economia/el-portal-estadistico-del-notariado-supera-el-medio-millon-de-usuarios-en-su-primer-ano.html`, `https://www.spanishpropertyinsight.com/2025/10/27/notaries-launch-new-portal-giving-deeper-insight-into-spains-property-market/`. Export: NO VERIFICADO (no se abrió el portal) |
| **MIVAU – Valor tasado de vivienda libre** | CCAA, provincia, municipios > 25.000 hab. | Dataset en datos.gob.es `https://datos.gob.es/es/catalogo/e05233601-valor-tasado-de-la-vivienda` y `…/a13002908-valor-tasado-de-la-vivienda-libre-por-municipios-de-mas-de-25-00-habitantes-total-base-20054` | Existencia verificada 2026-11-14 (resultados de búsqueda); URLs de Excel en mivau.gob.es: NO VERIFICADO (datos.gob.es devolvió error al abrir) |
| **INE – API Tempus3** | IPV por CCAA (operación IOE 30457), ETDP por provincia | Patrón verificado 2026-11-14: `https://servicios.ine.es/wstempus/js/ES/DATOS_TABLA/<id>` (JSON), `…/csv/ES/DATOS_TABLA/tpx/<id>`, `…/px/…`, `…/xlsx/…` (`https://ine.es/dynt3/inebase/en/APIdownTabla.htm?tpx=67539`) | Id de la tabla IPV Illes Balears: NO VERIFICADO (localizar con `…/wstempus/js/ES/TABLAS_OPERACION/<cod>`) |
| **IBESTAT** vía Catàleg de dades obertes CAIB | "Compravenda d'habitatges per període, illa i estat i règim" 2007M01–2026M07, cubo `I212002_0003`; también visados por municipio | **JSON-stat** `https://intranet.caib.es/ibestat-jaxi/tabla.do?typeDownload=6&px=2e0b5300-bf3e-44f2-90e0-b08e0dc032e6` (JSON `typeDownload=7`, PC-Axis `=0`); catálogo CKAN con API `https://intranet.caib.es/opendatacataleg/ca/openapi/API/` | verificado 2026-11-14, `https://intranet.caib.es/opendatacataleg/dataset/3753a29d-5032-4975-9ba7-9354cc6b74c8`. Licencia "no indicada". Precios por municipio: NO VERIFICADO |
| Colegio de Registradores, OHIB, Idealista/Fotocasa informes, API Idealista | — | — | NO VERIFICADO hoy (sin presupuesto); secundarias |

Recomendación: `price_benchmark(fuente, municipio_ine, periodo, tipo, eur_m2, n_operaciones)` cargada desde IBESTAT (JSON-stat) + MIVAU + INE; Notariado sólo si se confirma un export.

---

## 4. Riesgos territoriales (GIS) en Mallorca

### 4.1 IDEIB — ArcGIS Server REST `https://ideib.caib.es/geoserveis/rest/services/public/<servicio>/MapServer` — verificado 2026-11-14

Todos los servicios abiertos hoy: SRS **EPSG:25831**, extensión **WMSServer** (WFS **no** listado), `MaxRecordCount` 60.000, formatos de query JSON/geoJSON/PBF. WMS: `https://ideib.caib.es/geoserveis/services/public/<servicio>/MapServer/WMSServer?request=GetCapabilities&service=WMS`.

| Servicio | Capas (id: nombre) | Uso |
|---|---|---|
| **`GOIB_MUIB`** (Mapa Urbanístic de les Illes Balears; "només caràcter informatiu"; Palma y Andratx sin su último planeamiento) | 0 APR Inundació · 1 APR Esllevissaments · 2 **APR Incendis (ZAR)** · 3 APR Erosió · 4 APR Despreniments · 5 Protecció Aqüífers · 6 SISTEMES · 7 APT · 8 GESTIO · 9 CATALEG · 10 **QUALIFICACIONS** · 11 **CATEGORIES DEL SÒL RÚSTIC** · 12 **CLASSIFICACIO** | Capa clave. Campos de la 11: `CODIPLA, NOM, CODIMUIB, CODIMUIB_G, CODIMUNI, MUNICIPI, CODICLAS, DINIVIGEN, DFIVIGEN, URL…`; valores de `CODIMUIB_G`: `AANP, ANEI, ARIP, ARIP_B, ANIT, AIP, SRPMUN, AIA, AIA_E, AIA_I, AT, ATC, ATH, SRG, SRG_F, VIA, NUR` |
| **`GOIB_XarxaHidro_RiscInun_IB`** | 0 Xarxa hidrogràfica · 2 ARPSI DPH cartogràfic · 3 ARPSI Zona Flux Preferent · 5 Àrea inundable ARPSI 500 anys · 7/8/13 ídem No-ARPSI en tramitació · **21 Zones Potencialment Inundables (ZPI)** | Inundabilidad Govern (sólo T=500; no hay Q10/Q100) |
| **`GOIB_RiscIncendi_IB`** | 0 **ZAR incendis forestals 2014** · 1 Risc incendis IV Pla 2015-2024 | Incendios |
| `GOIB_OrdTerr_IB` | 0 Figures LEN (Llei 1/91: ANEI, ARIP, AAPI) | Espacios naturales |
| `GOIB_PTI_PIT` | Sólo se vieron capas de **Eivissa/Formentera** (PTI 2005, NNSS 2010) | **PTM Mallorca: NO VERIFICADO** — no se localizó servicio propio; usar MUIB capa 11 (categorías de sòl rústic del PTM refundidas) |

Patrón de consulta puntual (API REST ArcGIS estándar; no ejecutado hoy): `GET …/GOIB_MUIB/MapServer/11/query?geometry=<lon>,<lat>&geometryType=esriGeometryPoint&inSR=4326&spatialRel=esriSpatialRelIntersects&outFields=*&f=json`. Licencia de datos del Govern (CC BY 4.0): NO VERIFICADO.

### 4.2 Inundabilidad estatal — SNCZI (MITECO) — verificado 2026-11-14

Los antiguos `wms.mapama.gob.es/sig/Agua/...` están **retirados** (sus PDFs redirigen a 404). Infraestructura actual:

| Capa | Acceso |
|---|---|
| Zonas inundables T=10/100/500 (Península y Baleares, ETRS89) | Shapefiles `https://gis.miteco.gob.es/descargas/app/DescargaFichero?f=laminasPB-q10.zip` / `…q100.zip` / `…q500.zip` (`https://www.miteco.gob.es/es/cartografia-y-sig/ide/descargas/agua/zi-lamina.html`) |
| Riesgo fluvial T=100 (población, económico, puntos, medioambiental) | WMS `https://gis.miteco.gob.es/geoserver/agua/Zif_riesgo_pob_q100/wms` (y `Zif_riesgo_eco_q100`, `Zif_riesgoinundacion_q100`, `Zif_riesgo_medamb_q100`); shapefiles `…?f=riesgo-poblacion-PB-t100.zip` (`https://www.miteco.gob.es/es/cartografia-y-sig/ide/descargas/agua/riesgo-inundacion-fluvial-t100.html`) |
| WMS de láminas T=10/100/500 | NO VERIFICADO (la página de descargas no lo lista; probar `https://gis.miteco.gob.es/geoserver/agua/wms`) |

Baleares aparece incluida en los ficheros "Península y Baleares". La capa del Govern (§4.1) sigue siendo la referencia administrativa (ARPSI/APR).

### 4.3 Incendios
- Autonómico: `GOIB_RiscIncendi_IB` (ZAR 2014 y riesgo IV Pla 2015-2024) y `GOIB_MUIB` capa 2 — verificado 2026-11-14. Obligación de franja de 30 m en interfaz y Decret 125/2007: NO VERIFICADO hoy.
- Estatal (MITECO frecuencia de incendios): NO VERIFICADO.

### 4.4 Cédula d'habitabilitat (Consell de Mallorca)
- Lookup abierto por dirección/RC: **no encontrado** en ninguna búsqueda de hoy (NO VERIFICADO de forma concluyente; no se abrió la web del Consell). Mantener campo manual + heurística sobre el texto del anuncio.

### 4.5 ITE / IEE
- Sin dataset abierto insular localizado — NO VERIFICADO hoy. Semáforo: `anio_construccion < hoy − 50` ⇒ "exigir IEE".

---

## 5. Subastas

### 5.1 Portal de Subastas del BOE (`https://subastas.boe.es`)
- El portal **no pudo abrirse** desde esta sesión (fuera de la lista blanca del proxy); la afirmación anterior sobre `robots.txt` queda **NO VERIFICADA**.
- **No se encontró API ni dataset abierto** del portal en las búsquedas de hoy; la existencia de scrapers comerciales (Apify `olican/spain-subastas-boe-catastro`, `tikitakaclaw/subastas-boe-espana`) sugiere que no lo hay — verificado 2026-11-14 como indicio, `https://apify.com/olican/spain-subastas-boe-catastro.md`.
- Patrón de ficha verificado 2026-11-14 (fuente secundaria `https://www.sites.google.com/site/despachomerellesperez/subastas/caso-pr%C3%A1ctico-subastas-electr%C3%B3nicas`): `https://subastas.boe.es/detalleSubasta.php?idSub=SUB-JA-2016-18088&ver=N` con pestañas `ver=1` general, `2` autoridad gestora, `3` bienes, `5` acreedores, `6` pujas; `verCertificadoCierre.php?idSub=`; manual `https://subastas.boe.es/ayuda/ManualPortalSubastas_v_1_4.pdf`. Los scrapers extraen: id, puja mínima, valor de tasación, fecha de cierre, estado, juzgado, dirección, provincia, municipio, descripción y **referencia catastral**.
- Búsqueda avanzada `subastas_ava.php` con `campo[]/dato[]` (`BIEN.PROVINCIA=07`, `SUBASTA.ESTADO`, `BIEN.TIPO`): **NO VERIFICADO** (ninguna fuente abierta hoy muestra esos parámetros).
- Reglas: depósito fijado por la autoridad gestora, exención para acreedores listados, reserva de puja, devolución al cierre — verificado 2026-11-14 (misma fuente secundaria). Depósito del 5 %, cesión de remate (art. 647.3 LEC) y umbrales 70/50 % (art. 670 LEC): NO VERIFICADO hoy (derecho conocido, sin cita abierta).

### 5.2 Agencia Tributaria (AEAT)
- Los procedimientos de subasta de la AEAT "se publican a través del Portal del BOE"; la Sede ofrece un **buscador de inmuebles en subasta** (filtro por provincia/tipo/valoración, mapa, alertas) **sin autenticación** para buscar, sin API ni descarga — verificado 2026-11-14, `https://sede.agenciatributaria.gob.es/Sede/en_gb/notas-prensa/notas-prensa/2022/noviembre/25/agencia-tributaria-pone-marcha-aplicacion-subastas.html`. Ruta en Sede: *Deudas, apremios, embargos y subastas → Subastas*.

### 5.3 Tesorería General de la Seguridad Social (TGSS)
- Catálogo propio en **`https://w6.seg-social.es/subastas/`** (filtro por tipo de bien y CCAA; precio de salida, descripción, ubicación); **depósito 25 %** del precio de salida a nombre de la TGSS — verificado 2026-11-14 (fuente secundaria), `https://www.xataka.com/magnet/seguridad-social-ha-entrado-mercado-vivienda-esta-subastando-casas-pisos-embargados-10-000-euros`. API/descarga: no mencionada (NO VERIFICADO). Publicación paralela en el portal del BOE: NO VERIFICADO.

---

## 6. Catálogo de agencias inmobiliarias (semilla)

| Fuente | Qué es | Acceso | Estado |
|---|---|---|---|
| **Registre Oficial d'Agents Immobiliaris de les Illes Balears** — `https://www.caib.es/sites/agentsimmobiliaris/ca/inici` | Registro **administrativo**, creado por la **Llei 3/2024** (DA 13ª y 14ª) y gestionado por la DG d'Habitatge i Arquitectura; obligaciones de los agentes según Llei 5/2018. **Voluntario** desde la Ley 4/2026 (en vigor 13-06-2026); los inscritos reciben placa/distintivo | La web oficial **no ofrece buscador público ni listado descargable** (menú: Normativa, Formularis, Cartell distintiu, Tràmit, FAQ, Mapa); el registro es público y la DG facilita datos/certificados **a petición** | verificado 2026-11-14 (web oficial + `https://www.caib.es/sites/agentsimmobiliaris/ca/faqs/` + `https://www.mallorca.com/en/advice/property/buying/registro-agentes-inmobiliarios-baleares`). Dataset en catalegdades.caib.cat: NO VERIFICADO (no apareció en búsquedas) |
| COAPI / API Balears | Colegio profesional, buscador de colegiados | NO VERIFICADO hoy | Complementaria |
| Idealista / Fotocasa | Fichas de agencia en cada anuncio | Scraping | Única semilla práctica a corto plazo |

Modelo sugerido: `agency(id, nombre_normalizado, nif?, registro_ib?, coapi?, municipio, web, telefono, fuente, fecha_vista)`; deduplicar por `registro_ib` → `nif` → nombre normalizado + municipio. Enviar solicitud de acceso a datos a la DG d'Habitatge (el registro es público a petición).

---

## 7. Resumen de endpoints prioritarios (estado 2026-11-14)

1. **Catastro**: ASMX `OVCCoordenadas.asmx/Consulta_RCCOOR` (verificado) + WCF `COVCCallejero.svc` (base verificada, `/json/` pendiente) + **ATOM INSPIRE** CP/BU/AD (verificado) con licencia de "producto transformado".
2. **Euríbor**: `be1901.csv` del BdE, serie `D_1NBAF472` (verificado; oct-2026 = **3,239 %**); ECB `FM.M.U2.EUR.RT.MM.EURIBOR1YD_.HSTA` como respaldo.
3. **Valor de referencia**: no automatizable (identificación + reCAPTCHA; sin API, verificado); mapas de valores pendientes de confirmar como descarga abierta.
4. **Riesgos**: IDEIB `GOIB_MUIB` capas 0/2/10/11/12 y `GOIB_XarxaHidro_RiscInun_IB` capa 21 (verificado, REST ArcGIS + WMS, EPSG:25831); MITECO `gis.miteco.gob.es` shapefiles Q10/Q100/Q500 (verificado).
5. **Precios**: IBESTAT JSON-stat (verificado) + INE Tempus3 (patrón verificado); Notariado sólo visor.
6. **Subastas**: sin API (indicio); fichas `detalleSubasta.php?idSub=` (verificado); parámetros de búsqueda avanzada pendientes.
7. **Agencias**: registro oficial sin buscador abierto (verificado); pedir datos a la DG o sembrar desde portales.
