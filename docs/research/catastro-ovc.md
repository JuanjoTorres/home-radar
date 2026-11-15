# Catastro OVC (servicios libres) — formatos exactos para la Fase 3

Fecha del informe: **2026-11-15**. Complementa `docs/sources/public-data.md` §1 (que verificó los endpoints base, no los formatos).

> **Cómo se verificó.** El proxy de esta sesión **no** deja abrir `ovc.catastro.meh.es` ni `*.catastro.hacienda.gob.es/robots.txt` (respuesta `URL_NOT_ON_ALLOWLIST`), y el shell no llega a los hosts del Catastro. Por eso los formatos se han verificado con **respuestas reales grabadas** (fixtures) en repositorios de código abierto que llaman al servicio en vivo, más la documentación oficial que sí se pudo abrir. Cada dato lleva su fuente; lo que no se pudo contrastar va marcado **NO VERIFICADO**.
>
> Fuentes primarias de formato (todas abiertas el 2026-11-15):
> - **[VC]** `Jamarlli/viviendaChecker` (GPL-3.0): `fixtures/catastro/*.json` = "respuestas reales grabadas", spec `specs/02-emparejamiento.md`, cliente `src/adapters/catastro/cliente.ts`.
> - **[IT]** `sergi039/idealista-tracker-ai`: `services/cadastre_service.py` ("verified live on 2026-08-20") y `tests/data/catastro_dnprc_*.json` ("taken from the live service").
> - **[JB]** `jorgebg90/catastro-rest-api`: `tests/fixtures/soap/*.xml` (respuestas XML/SOAP).
> - **[BQ]** `BquantFinance/Administracion-fuentes-publicas`: `sources/territorio-cartografia/catastro-ovc.yaml` (`verified: "2026-10-01"`, con límites observados).
> - **[GO]** `alejndr0/go-catastro`: `catastro/struct.go`, `client.go` (mapa de etiquetas JSON y nota sobre el WAF).
> - **[CR]** `rOpenSpain/CatastRo` (paquete CRAN): `R/ovc-rccoor-dist.R`, `R/utils-ovc.R`, `R/docs.R`.
> - **[PY]** `gisce/pycatastro` (`pycatastro/__init__.py`) y `josejuanmartinez/libreCatastro` (`scrapper_xml.py`): URLs y parámetros ASMX.

---

## 1. Endpoints

Hosts: el host operativo sigue siendo **`ovc.catastro.meh.es`**; la página oficial de esquemas (dominio `catastro.hacienda.gob.es`) lo sigue listando así en 2026 (verificado 2026-11-15, `https://www.catastro.hacienda.gob.es/ws/esquemas.htm`). **HTTPS funciona**: [VC], [IT] y `jesusvaros/CaseroVerificado` usan `https://ovc.catastro.meh.es/...` en producción ([IT] lo verificó en vivo el 2026-08-20). Un alias `ovc.catastro.hacienda.gob.es`: **NO VERIFICADO** (no aparece en ninguna fuente oficial ni de código; no usarlo).

| Operación | Variante | URL (GET) | Parámetros | Formato | Estado |
|---|---|---|---|---|---|
| `Consulta_DNPRC` | **JSON (WCF)** — recomendada | `https://ovc.catastro.meh.es/OVCServWeb/OVCWcfCallejero/COVCCallejero.svc/json/Consulta_DNPRC?RefCat={RC}` | `RefCat` (14, 18 o 20 car.). `Provincia`/`Municipio` **opcionales: se pueden omitir** | JSON | Verificado ([VC] cliente + fixtures, [IT], [BQ]) |
| `Consulta_DNPRC` | XML (ASMX) | `https://ovc.catastro.meh.es/ovcservweb/OVCSWLocalizacionRC/OVCCallejero.asmx/Consulta_DNPRC?Provincia=&Municipio=&RC={RC}` | **`RC`** (no `RefCat`); `Provincia` y `Municipio` se envían **vacíos** | XML, ns `http://www.catastro.meh.es/` | URL/params verificados en código ([PY]: libreCatastro documenta "Name of the province (can be set to '')"). Que la ausencia del parámetro (en vez de vacío) dé error 500 de ASP.NET: **NO VERIFICADO** — enviarlos siempre vacíos |
| `Consulta_RCCOOR` | JSON (WCF) | `https://ovc.catastro.meh.es/OVCServWeb/OVCWcfCallejero/COVCCoordenadas.svc/json/Consulta_RCCOOR?SRS=EPSG:4326&CoorX={lon}&CoorY={lat}` | **`CoorX`/`CoorY`** (¡no `Coordenada_X`!), `SRS` | JSON | Verificado en código ([GO], CaseroVerificado). [BQ] observó **error 76** al usar `Coordenada_X` contra el JSON: es el nombre de parámetro equivocado |
| `Consulta_RCCOOR` | XML (ASMX) | `https://ovc.catastro.meh.es/ovcservweb/OVCSWLocalizacionRC/OVCCoordenadas.asmx/Consulta_RCCOOR?SRS=EPSG:4326&Coordenada_X={lon}&Coordenada_Y={lat}` | `SRS`, `Coordenada_X` (lon), `Coordenada_Y` (lat) | XML | Verificado ([BQ] ejemplo curl; [PY]; [CR]) |
| `Consulta_RCCOOR_Distancia` | JSON (WCF) | `https://ovc.catastro.meh.es/OVCServWeb/OVCWcfCallejero/COVCCoordenadas.svc/json/Consulta_RCCOOR_Distancia?CoorX={lon}&CoorY={lat}&SRS=EPSG:4326` | ídem | JSON | Verificado ([VC] fixture `rccoor-varias.json`) |
| `Consulta_RCCOOR_Distancia` | XML (ASMX) | `http://ovc.catastro.meh.es/ovcservweb/OVCSWLocalizacionRC/OVCCoordenadas.asmx/Consulta_RCCOOR_Distancia?SRS=EPSG:4326&Coordenada_X={lon}&Coordenada_Y={lat}` | ídem ASMX | XML | Verificado ([CR] snapshot de test con la URL exacta) |
| `Consulta_CPMRC` (RC → coordenadas) | JSON / ASMX | `…/COVCCoordenadas.svc/json/Consulta_CPMRC?Provincia=&Municipio=&SRS=EPSG:4326&RefCat={RC14}` / `…/OVCCoordenadas.asmx/Consulta_CPMRC?Provincia=&Municipio=&SRS=EPSG:4326&RC={RC14}` | | | JSON: parámetros de [GO] (no probados en vivo por otra fuente → **NO VERIFICADO**); ASMX: [PY] + public-data.md |
| `Consulta_DNPLOC` (dirección) | JSON | `…/COVCCallejero.svc/json/Consulta_DNPLOC?Provincia=ILLES BALEARS&Municipio=PALMA&Sigla=CL&Calle=…&Numero=…` (+ `Bloque`,`Escalera`,`Planta`,`Puerta` opcionales) | `Sigla` obligatoria ([VC]: vacía → "no existe ningún inmueble") | JSON | Verificado [VC]. Nombre exacto de provincia/municipio para Baleares: **NO VERIFICADO** (obtenerlo con `ObtenerMunicipios?Provincia=…`) |

Esquemas XSD oficiales (verificado 2026-11-15, esquemas.htm): `consulta_dnp.xsd`, `consulta_coordenadas.xsd`, `consulta_coordenadas_distancias.xsd`, `ldc.xsd` en `https://www.catastro.hacienda.gob.es/ws/esquemas/`. La página **no menciona JSON** ni límites de uso; dice que los servicios "devuelven un XML con la información solicitada o, en su caso, un código de error".

**Recomendación: JSON WCF** (`COVCCallejero.svc/json/…`, `COVCCoordenadas.svc/json/…`) — sin espacios de nombres, `RefCat` solo, y es lo que usan los clientes más recientes (2026). ASMX XML como fallback.

---

## 2. `Consulta_DNPRC` — estructura de respuesta

Reglas generales (todas verificadas en fixtures):
- **Siempre HTTP 200**, también en error; comprobar `control.cuerr` / presencia de `lerr` ([IT], [BQ]).
- RC de **14** car. con varios inmuebles → **lista** `lrcdnp.rcdnp[]` (sin `ldt`, sin `lcons`, sin `idbi`).
- RC de **20** car. (o parcela con un solo inmueble) → **objeto** `bico.bi` + `bico.lcons[]` + `bico.finca`.
- Todo son **strings**: `sfc` puede traer **punto de miles** (`"1.149"` = 1149 m², [VC] `dnprc-sfc-miles.json`); `cpt` usa **coma decimal** (`"2,150000"`); `ant` es año (`"1930"`), ausente en solares ([VC]).
- Diferencias JSON ↔ XML: en JSON las listas son arrays (`rcdnp: [...]`, `lcons: [...]`, `lerr: [...]`, `lspr: [...]`); en XML son elementos repetidos con contenedor (`<lrcdnp><rcdnp/>…`, `<lcons><cons/>…`, `<lerr><err/>`, `<lspr><spr/>`) ([IT] lo midió para `lspr`). Raíz JSON: `consulta_dnprcResult`; raíz XML (GET ASMX): `<consulta_dnp xmlns="http://www.catastro.meh.es/">` (vía SOAP va envuelta en `soap:Envelope/…/Consulta_DNP`).
- La dirección de inmuebles urbanos está en **`dt.locs.lous.lourb`** (no `lors`, como declara por error el struct de [GO]); rústicos en `dt.locs.lous.lorus` ([IT], [VC], [JB]).

### 2.1 Multi-inmueble (bloque de pisos) — JSON real, recortado
Fuente: [VC] `fixtures/catastro/dnprc-parcela.json` (CL ALCALA 100, Madrid, `RefCat=2853604VK4725D`, 34 inmuebles).

```json
{"consulta_dnprcResult":{
  "control":{"cudnp":34},
  "lrcdnp":{"rcdnp":[
    {"rc":{"pc1":"2853604","pc2":"VK4725D","car":"0001","cc1":"Y","cc2":"P"},
     "dt":{"loine":{"cp":"28","cm":"79"},"cmc":"900","np":"MADRID","nm":"MADRID",
           "locs":{"lous":{"lourb":{"dir":{"cv":"132","tv":"CL","nv":"ALCALA","pnp":"100","snp":"0"},
                                    "loint":{"es":"1","pt":"00","pu":"B"},"dp":"28009","dm":"4"}}}},
     "debi":{"luso":"Comercial","sfc":"41","cpt":"2,150000","ant":"1930"}},
    {"rc":{"pc1":"2853604","pc2":"VK4725D","car":"0007","cc1":"S","cc2":"H"},
     "dt":{ "...": "...", "locs":{"lous":{"lourb":{"dir":{"...":"..."},"loint":{"es":"1","pt":"01","pu":"IZ"},"dp":"28009","dm":"4"}}}},
     "debi":{"luso":"Residencial","sfc":"80","cpt":"2,620000","ant":"1930"}}
    /* … 32 más … */
  ]}}}
```

Otro ejemplo real ([VC] `dnprc-sfc-miles.json`, CL PRINCIPE DE VERGARA 76): `{"loint":{"es":"G","pt":"-1","pu":"01"}},"debi":{"luso":"Almacen-Estacionamiento","sfc":"1.149","cpt":"38,190000","ant":"2019"}` y un ático `{"loint":{"pt":"AT","pu":"A"}},"debi":{"luso":"Residencial","sfc":"207",…}`. Nótese que `es` puede faltar.

XML equivalente ([JB] `parcel-found.xml`, CL SERRANO 30, Madrid, 11 inmuebles):
```xml
<consulta_dnp xmlns="http://www.catastro.meh.es/">
  <control><cudnp>11</cudnp></control>
  <lrcdnp>
    <rcdnp>
      <rc><pc1>1853111</pc1><pc2>VK4715D</pc2><car>0001</car><cc1>O</cc1><cc2>Y</cc2></rc>
      <dt><loine><cp>28</cp><cm>79</cm></loine><cmc>900</cmc><np>MADRID</np><nm>MADRID</nm>
        <locs><lous><lourb>
          <dir><cv>5757</cv><tv>CL</tv><nv>SERRANO</nv><pnp>30</pnp><snp>0</snp><td>0000</td></dir>
          <loint><es>1</es><pt>00</pt><pu>DR</pu></loint><dp>28001</dp><dm>4</dm>
        </lourb></lous></locs></dt>
    </rcdnp>
    <!-- … -->
  </lrcdnp>
</consulta_dnp>
```
Observación: en este fixture XML los `rcdnp` **no traen `debi`** (el JSON de [VC] sí). No está claro si es un recorte del autor o una diferencia real del ASMX → **NO VERIFICADO**. Si se usa XML y falta `debi`, hay que pedir cada RC de 20 car. Motivo adicional para preferir JSON.

### 2.2 Inmueble único — JSON real
Fuente: [VC] `fixtures/catastro/dnprc-rc.json` (`RefCat=2853604VK4725D0001YP`).
```json
{"consulta_dnprcResult":{
  "control":{"cudnp":1,"cucons":2},
  "bico":{
    "bi":{
      "idbi":{"cn":"UR","rc":{"pc1":"2853604","pc2":"VK4725D","car":"0001","cc1":"Y","cc2":"P"}},
      "dt":{"loine":{"cp":"28","cm":"79"},"cmc":"900","np":"MADRID","nm":"MADRID",
            "locs":{"lous":{"lourb":{"dir":{"cv":"132","tv":"CL","nv":"ALCALA","pnp":"100","snp":"0"},
                                     "loint":{"es":"1","pt":"00","pu":"B"},"dp":"28009","dm":"4"}}}},
      "ldt":"CL ALCALA 100 Es:1 Pl:00 Pt:B 28009 MADRID (MADRID)",
      "debi":{"luso":"Comercial","sfc":"41","cpt":"2,150000","ant":"1930"}},
    "finca":{"ldt":"CL ALCALA 100  MADRID (MADRID)",
             "ltp":"Parcela con varios inmuebles (division horizontal)",
             "dff":{"ss":"361"},
             "infgraf":{"igraf":"https://www1.sedecatastro.gob.es/Cartografia/mapa.aspx?del=28&mun=900&refcat=2853604VK4725D"}},
    "lcons":[
      {"lcd":"COMERCIO","dt":{"lourb":{"loint":{"es":"1","pt":"00","pu":"B"}}},"dfcons":{"stl":"36"},"dvcons":{"dtip":"COMERCIO"}},
      {"lcd":"ELEMENTOS COMUNES","dfcons":{"stl":"5"}}]}}}
```
**Dato clave:** `sfc` (41) = suma de `lcons[].dfcons.stl` (36 privativos + 5 de **ELEMENTOS COMUNES**). Es decir, `sfc` incluye la parte proporcional de comunes, y `lcons` permite separar la superficie privativa. Coherente con la definición catastral citada por la OCU: superficie construida = "la superficie privativa y la parte correspondiente de la superficie de los elementos comunes" (ocu.org, abierto 2026-11-15).

XML equivalente ([JB] `property-found.xml`, `9872023VH5797S0001WX`, vivienda unifamiliar):
```xml
<consulta_dnp xmlns="http://www.catastro.meh.es/">
  <control><cudnp>1</cudnp><cucons>3</cucons><cucul>0</cucul></control>
  <bico>
    <bi>
      <idbi><cn>UR</cn><rc><pc1>9872023</pc1><pc2>VH5797S</pc2><car>0001</car><cc1>W</cc1><cc2>X</cc2></rc></idbi>
      <dt>… <locs><lous><lourb><dir><cv>33</cv><tv>CL</tv><nv>GLORIA</nv><pnp>51</pnp><snp>0</snp></dir>
             <loint><es>T</es><pt>OD</pt><pu>OS</pu></loint><dp>13730</dp><dm>3</dm></lourb></lous></locs></dt>
      <ldt>CL GLORIA 51 13730 SANTA CRUZ DE MUDELA (CIUDAD REAL)</ldt>
      <debi><luso>Residencial</luso><sfc>308</sfc><cpt>100,000000</cpt><ant>1980</ant></debi>
    </bi>
    <lcons>
      <cons><lcd>VIVIENDA</lcd><dt><lourb><loint><pt>00</pt><pu>01</pu></loint></lourb></dt><dfcons><stl>109</stl></dfcons></cons>
      <cons><lcd>ALMACEN</lcd>…<dfcons><stl>187</stl></dfcons></cons>
      <cons><lcd>OTROS USOS</lcd>…<dfcons><stl>12</stl></dfcons></cons>
    </lcons>
  </bico>
</consulta_dnp>
```
(`es=T, pt=OD, pu=OS` = "toda la finca / otros"; típico de unifamiliares.) [BQ] confirma con la misma RC: `sfc 308, ant 1980`, `cpt` "100,000000".

Parcela rústica/mixta ([IT] `catastro_dnprc_33016A003001530001HQ.json`): además `dt.locs.lous.lorus.{czc,cpp:{cpo,cpa},npa,cpaj}`, `finca.ltp` "Parcela … con inmuebles de distinta clase", `lspr[]` (subparcelas: `cspr`, `dspr.{ccc,dcc,ip,ssp}`); `debi` sin `ant`.

### 2.3 Tabla de campos → HomeRadar

| Ruta JSON (unidad en lista / unidad única) | Significado | Campo HomeRadar sugerido |
|---|---|---|
| `control.cudnp` | nº de inmuebles devueltos | `CadastreUnitCount` |
| `control.cucons` | nº de construcciones en `lcons` | — |
| `control.cuerr` | nº de errores (si ≥1 → leer `lerr`) | — |
| `rc.{pc1,pc2,car,cc1,cc2}` / `bi.idbi.rc.{…}` | RC: `pc1`(7)+`pc2`(7) = parcela; `car`(4) = cargo/inmueble; `cc1`+`cc2` = dígitos de control | `CadastralReference` (20) y `CadastralParcel` (14) |
| `bi.idbi.cn` | clase: `UR` urbano / `RU` rústico | `CadastreClass` |
| `dt.loine.{cp,cm}` | códigos **INE** provincia/municipio, "como texto sin ceros a la izquierda" ([BQ]; para Baleares se espera `cp:"7"`, NO VERIFICADO) | `IneMunicipality` = cp rellenado a 2 + cm rellenado a 3 |
| `dt.cmc` | código de municipio **catastral** (puede diferir del INE: Madrid 79 vs 900) | — |
| `dt.np`, `dt.nm` | nombre provincia / municipio | `Province`, `Municipality` |
| `dt.locs.lous.lourb.dir.{tv,nv,pnp,plp,snp,km,cv}` | tipo vía, nombre vía, nº policía, letra, 2º número, km, código vía | `Street` |
| `dt.locs.lous.lourb.loint.es` | escalera (`1`, `G`, `T`…; puede faltar) | `Staircase` |
| `dt.locs.lous.lourb.loint.pt` | planta (`00`, `01`…, `-1`, `AT`, `OD`; [VC] también cita `BJ`, `EN`, `SS`, `ST`) | `Floor` (normalizado) |
| `dt.locs.lous.lourb.loint.pu` | puerta (`A`, `B`, `IZ`, `DR`, `CI`, `CD`, `01`, `OS`) | `Door` |
| `dt.locs.lous.lourb.{dp,dm}` | código postal, distrito municipal | `PostalCode` |
| `bi.ldt` (solo unidad única) | dirección literal | `CadastreAddress` |
| `debi.luso` | uso principal (`Residencial`, `Comercial`, `Almacen-Estacionamiento`, `Industrial`, …) | `CadastreUse` |
| `debi.sfc` | **superficie construida total m², incluye parte de elementos comunes**; string, `.` = miles | `CadastreBuiltAreaM2` |
| `debi.cpt` | coeficiente de participación % (coma decimal) | `ParticipationCoefficient` |
| `debi.ant` | **año de construcción** (antigüedad); ausente en solares | `YearBuilt` |
| `bico.lcons[].lcd` | uso de cada construcción (`VIVIENDA`, `ALMACEN`, `COMERCIO`, `ELEMENTOS COMUNES`, `OTROS USOS`, …) | — |
| `bico.lcons[].dfcons.stl` | superficie de esa construcción (m²) | `CadastrePrivateAreaM2` = Σ stl donde `lcd ≠ "ELEMENTOS COMUNES"` |
| `bico.lcons[].dt.lourb.loint.{es,pt,pu}` | situación de esa construcción | — |
| `bico.lcons[].dvcons.dtip` | tipología | — |
| `bico.finca.ltp` | tipo de finca ("Parcela con varios inmuebles (division horizontal)") | `IsHorizontalProperty` |
| `bico.finca.dff.ss` | superficie de suelo de la parcela (m²) | `PlotAreaM2` |
| `bico.finca.infgraf.igraf` | enlace al mapa de la Sede | `CadastreMapUrl` |

---

## 3. `Consulta_RCCOOR` y `Consulta_RCCOOR_Distancia`

### 3.1 RCCOOR (punto → parcela)
XML real ([JB] `coordinates-found.xml`):
```xml
<consulta_coordenadas xmlns="http://www.catastro.meh.es/">
  <control><cucoor>1</cucoor><cuerr>0</cuerr></control>
  <coordenadas><coord>
    <pc><pc1>0142208</pc1><pc2>VK4704A</pc2></pc>
    <geo><xcen>-3.70720</xcen><ycen>40.41446</ycen><srs>EPSG:4326</srs></geo>
    <ldt>CL IMPERIAL 14 MADRID (MADRID)</ldt>
  </coord></coordenadas>
</consulta_coordenadas>
```
JSON: raíz **`Consulta_RCCOORResult`** (C mayúscula) → `coordenadas.coord[0].pc.{pc1,pc2}`, `geo.{xcen,ycen,srs}`, `ldt` ([GO] struct; CaseroVerificado lee `data?.Consulta_RCCOORResult?.coordenadas?.coord?.[0]?.pc`).
- Devuelve **solo la parcela (14 car.)**, nunca el piso.
- `xcen/ycen` **repiten las coordenadas pedidas**, no el centroide ([BQ]).
- Exige ns por defecto `http://www.catastro.meh.es/` en XPath ([BQ]).
- **Punto fuera de parcela** (calle, plaza, mar): error **16 "PARA ESAS COORDENADAS NO HAY REFERENCIA DISPONIBLE"** ([JB] `coordinates-not-found.xml`, con `cuerr=1`).

### 3.2 RCCOOR_Distancia (vecinas con distancia)
Comportamiento documentado: "En caso de no encontrar ninguna referencia catastral en dicho punto, se buscará en un área cuadrada de 50 metros de lado, centrada en dichas coordenadas, y se devolverá la lista de referencias catastrales encontradas en dicha área" ([PY] docstring copiado de la doc oficial; [CR] lo repite).

JSON real ([VC] `rccoor-varias.json`, recortado):
```json
{"Consulta_RCCOOR_DistanciaResult":{"control":{"cucoor":1},
 "coordenadas_distancias":{"coordd":[{
   "geo":{"xcen":"-3.70500","ycen":"40.41500","srs":"EPSG:4326"},
   "lpcd":[
     {"pc":{"pc1":"0343111","pc2":"VK4704C"},
      "dt":{"loine":{"cp":"28","cm":"79"},"lourb":{"dir":{"cv":"862","pnp":"12"}}},
      "ldt":"CL BOLSA 12 MADRID (MADRID)","dis":"3.18"},
     {"pc":{"pc1":"0343110","pc2":"VK4704C"}, "…":"…", "ldt":"CL BOLSA 10 MADRID (MADRID)","dis":"4.54"}
     /* … hasta 10 parcelas en el ejemplo, dis máx. 24.47 m … */ ]}]}}}
```
- `dis` = metros (string, punto decimal), lista ordenada por distancia.
- Sin parcelas: **sin `lpcd` y sin error** ([VC] `rccoor-vacia.json`: `{"coordd":[{"geo":{…}}]}`) — tratar como "sin resultados"; o bien error 16 ([CR] test "OVC service error 16").
- XML: raíz `consulta_coordenadas_distancias` → `coordenadas_distancias/coordd/{geo, lpcd*}` ([CR] `ovc-rccoor-dist.R`).
- SRS aceptados según [CR]: 4230, 4258, 4326, 23029–23031, 25829–25831, 32627–32631. Para Mallorca: `EPSG:4326` (o 25831).

---

## 4. Errores

Estructura: JSON `{"<op>Result":{"control":{"cuerr":1},"lerr":[{"cod":"5","des":"…"}]}}`; XML `<control><cuerr>1</cuerr></control><lerr><err><cod>5</cod><des>…</des></err></lerr>`. Siempre con HTTP 200. Excepción: `ObtenerNumerero` JSON envuelve en `lerr.err` ([VC] comentario del cliente). `Consulta_DNPLOC` con nº inexistente devuelve error 43 **y** datos (`numerero.nump[]` con números cercanos) ([VC]).

| Código | Texto literal | Servicio | Fuente | Tratamiento |
|---|---|---|---|---|
| 3 | `LA REFERENCIA CATASTRAL DEBE TENER 14,18 o 20 POSICIONES` | DNPRC | [VC], [IT] fixtures | Validación local previa |
| 4 | `LA REFERENCIA CATASTRAL NO ESTÁ CORRECTAMENTE FORMADA` | DNPRC | [IT], [JB] | RC mal tecleada (dígitos de control) |
| 5 | `NO EXISTE NINGÚN INMUEBLE CON LOS PARÁMETROS INDICADOS` | DNPRC/DNPLOC | [VC], [JB] | **No encontrado** (cacheable) |
| 10 | `NO HAY COINCIDENCIAS EN LA BÚSQUEDA DE VÍAS` | ConsultaVia | [JB] | No encontrado |
| 11 | `LA PROVINCIA ES OBLIGATORIA` | CPMRC/callejero | public-data.md (vignette CatastRo) | Bug de petición |
| 12 | `LA PROVINCIA NO EXISTE` | ConsultaMunicipio | [JB] | Bug de petición |
| 16 | `PARA ESAS COORDENADAS NO HAY REFERENCIA DISPONIBLE` | RCCOOR(_Distancia) | [JB], [CR] | Punto fuera de parcela → usar `_Distancia` |
| 33 | `LA VÍA NO EXISTE` | DNPLOC | [VC], [JB] | No encontrado |
| 41 | `EL NÚMERO ES OBLIGATORIO.` | DNPLOC/Numerero | [JB]; [VC] ("Vacío da error 41") | Bug de petición |
| 43 | `EL NUMERO NO EXISTE` | DNPLOC | [VC] | No encontrado (trae números cercanos) |
| 75 | `EL CAMPO SRS ES OBLIGATORIO` | coordenadas | [JB] | Bug |
| 76 | "Coordenada X no informada" (texto exacto NO VERIFICADO) | coordenadas | guía `CabhuDev/mcp_Catastro`; [BQ] lo obtuvo con `Coordenada_X` en el JSON | Usar `CoorX`/`CoorY` en JSON |
| 77 | `LA COORDENADA X DEBE SER UN VALOR NUMÉRICO` | coordenadas | [JB] | Formatear con `CultureInfo.InvariantCulture` |
| 79 | `LA COORDENADA Y OBLIGATORIA` | coordenadas | [JB] | Bug |

Los códigos del servicio **restringido** (`CON 1…40`, p. ej. 40 = límite de 6.000 inmuebles) del PDF `WCFservices_Consulta.pdf` **no** son los del servicio libre: no mezclar.

Además, fallos de transporte: el WAF puede devolver **HTTP 400/403** ("Petición HTTP bloqueada") o cerrar la conexión sin respuesta (ver §5). Esos no son "no encontrado": no cachear ([IT] separa `not_found` de `refused`/`unavailable`).

---

## 5. Límites, condiciones, licencia

- **Cuota numérica publicada: ninguna** para los servicios libres (esquemas.htm no lista límites; verificado 2026-11-15). [IT]: "Catastro publishes no requests-per-second figure anywhere".
- **Política oficial de la Sede** (verificado 2026-11-15, `https://www.catastro.hacienda.gob.es/ayuda/condicionesuso.htm`): los servicios interactivos "no pueden utilizarse para descargas masivas" ni por procesos automatizados; hay "límites de accesos concurrentes", "análisis del número de peticiones realizadas desde una misma dirección IP en un intervalo de tiempo" y, si se supera, "se le deniega el servicio durante un período de tiempo, generalmente 10 días"; también listas negras y bloqueos manuales. (La página habla de "la SEC" en general; que aplique literalmente a los servicios web OVC es interpretación prudente.)
- **Límite observado** ([BQ], 2026-10-01): "tras unas 15 llamadas en un minuto el servidor cierra la conexión sin respuesta (curl 000) durante unos 10 minutos"; además "bloquea rangos de centros de datos de forma variable" (403 desde GitHub Actions el 30/09/2026, 200 al día siguiente). Fuente secundaria, no oficial.
- **User-Agent**: [GO] `client.go`: el WAF (F5 ASM) devuelve **400** a peticiones sin User-Agent, con `curl/*`, `Go-http-client/*` o con un UA de Chrome 113 concreto ya en lista negra; recomiendan un UA de navegador actual. Para .NET: el `HttpClient` por defecto **no envía User-Agent** → fijar uno explícito. Recomendación HomeRadar: UA descriptivo tipo navegador (p. ej. `Mozilla/5.0 (compatible; HomeRadar/1.0)`) y, si da 400, cambiar a UA de navegador. Qué UAs exactos acepta: **NO VERIFICADO**.
- **CORS**: el JSON envía cabeceras CORS (CaseroVerificado lo llama desde el navegador). Irrelevante para backend .NET.
- **robots.txt** de `ovc.catastro.meh.es` y `ovc.catastro.hacienda.gob.es`: **NO VERIFICADO** (no se pudo abrir: el proxy no lo permite).
- **Licencia de reutilización**: la licencia-tipo de descarga (`https://www.catastro.hacienda.gob.es/pdf/ovc/licdescargaES.pdf`, verificado 2026-11-15) exige **citar la fuente y la fecha** en todos los productos (cl. 12), no permite uso público de la información **original** sin transformar (cl. 5 y 8: no distribuirla por Internet), permite productos transformados incluso comerciales (cl. 6) sin presentarse como "información catastral" oficial (cl. 7). Esta licencia cubre la **descarga masiva/INSPIRE**; para los servicios web libres no hay licencia específica publicada (**NO VERIFICADO** que aplique). Para un uso personal de una sola persona el riesgo es nulo, pero conviene: mostrar "Fuente: Dirección General del Catastro, consultado el AAAA-MM-DD", no republicar respuestas crudas y enlazar a la Sede (`finca.infgraf.igraf` o `https://www1.sedecatastro.gob.es/CYCBienInmueble/OVCConCiud.aspx?RefC={RC}&del=&mun=` según [IT]).
- **Codificación**: el JSON viene en UTF-8 con acentos en `des` (fixtures). El WFS/WMS responde ISO-8859-1 ([BQ]). Leer bytes y respetar `Content-Type`.

**Política recomendada para HomeRadar**: ≤ 1 petición/s y ≤ 10/min por proceso (con ráfaga máx. 5), caché por RC 30 días (los datos no cambian entre consultas; [IT] usa 30 días), caché de "no encontrado" (cód. 5/16/33/43) 7 días, **sin reintentos automáticos** ante cierre de conexión/400/403 (abrir circuito 15 min), timeout 20 s.

---

## 6. Valor de referencia — veredicto

**No hay vía no autenticada y automatizable para obtener el valor de referencia de un inmueble concreto.**

- Datos protegidos: la ayuda de la Sede define como protegidos "el valor catastral y los valores del suelo y, en su caso, de la construcción"; los libres son los que "no hacen referencia al titular y valor catastral" (verificado 2026-11-15, `https://www.catastro.hacienda.gob.es/ayuda/default.htm`). Ningún servicio OVC libre devuelve VR (revisados todos los campos de `consulta_dnp`).
- Consulta individual en la Sede: requiere identificarse (DNI/NIF, certificado o Cl@ve), indicar la RC, la finalidad ("Efectos informativos"), el NIF, la **fecha** del valor y superar un **reCAPTCHA** (Xataka, abierto 2026-11-15, `https://www.xataka.com/basics/como-saber-valor-casa-piso-valor-referencia-catastro`). Hipotecas.me afirma "Solo se puede consultar con certificado digital o Cl@ve". Ambas son fuentes secundarias; sea cual sea la modalidad, hay identificación + reCAPTCHA → **no automatizable** (y automatizarlo violaría las condiciones de uso).
- Publicación anual: la resolución de VR 2026 se sometió a audiencia ("la propuesta de resolución puede ser consultada en la Sede Electrónica del Catastro", BOE-B-2025-36293); los VR individuales se consultan "de forma permanente en la Sede Electrónica del Catastro" desde el 1-1-2026 (BOE-B-2025-46479, rústicos). Ninguna fuente menciona API ni descarga.
- **Mapas de valores** (Informe Anual del Mercado Inmobiliario): publicados "en abierto, sobre la cartografía catastral"; delimitan "ámbitos territoriales homogéneos de valoración", con el producto representativo (tipología, superficie, calidad, antigüedad, conservación) y su **módulo de valor medio**; para vivienda la escala de colores corresponde a rangos en €/m², pero la leyenda **es una imagen** (verificado 2026-11-15, `https://www.catastro.hacienda.gob.es/ayuda/leyenda_mapavalores.htm`). La nota de prensa de 2021 los describe como la "primera" etapa del VR; el valor individual se obtiene ajustando por "calidad, antigüedad y estado de conservación" y un coeficiente de minoración 0,9 (`hacienda.gob.es …14-10-21-NP-MAPAS-DE-VALORES…pdf`). La Circular 02.04/2026/P confirma que el mapa de valores "asigna módulos de valor medio de los productos inmobiliarios representativos en dichos ámbitos".
  - Descarga en formato estructurado (shapefile/CSV/WMS con valores numéricos por ámbito): **NO VERIFICADO — no encontrada** en ninguna fuente oficial ni repositorio (búsqueda de código GitHub "mapa de valores"/"mapavalores": 0 resultados).
  - Aunque se obtuviera el módulo del ámbito, el VR de un piso concreto depende de coeficientes (calidad, antigüedad, conservación, superficie, planta…) y de la minoración; un €/m² de ámbito × `sfc` daría solo un **orden de magnitud**.
- Estadísticas municipales abiertas (IBESTAT "Número de bienes catastrales | Valor | Valor medio") son de **valor catastral**, no de VR: no usar como proxy.

**Recomendación**
1. Campo **manual** `ValorReferenciaManual` (€, fecha de consulta, nota) en la ficha; botón "Consultar en la Sede" que abre la Sede con la RC copiada al portapapeles.
2. **No estimar automáticamente** el VR. Para impuestos (ITP Baleares: base = mayor entre precio y VR), calcular con el **precio** si no hay VR manual y mostrar el aviso.
3. Si en el futuro se quiere un "rango orientativo", hacerlo solo a partir de un módulo de ámbito introducido a mano por el usuario desde el visor, etiquetado como estimación.

Texto de aviso propuesto:
> "Valor de referencia del Catastro: **no disponible automáticamente**. Solo puede consultarse identificándote en la Sede Electrónica del Catastro (sede.catastro / Valor de referencia). Si el valor de referencia es **mayor** que el precio, Hacienda calculará el ITP sobre el valor de referencia y el impuesto será más alto que el mostrado aquí. Introdúcelo manualmente para un cálculo exacto."

---

## 7. Casar el piso del anuncio con su unidad catastral

Problema: el anuncio da coordenadas aproximadas (los portales suelen desplazar el punto) y m² anunciados; el Catastro da por parcela una lista de unidades con planta/puerta/`sfc`/año/uso.

Estrategia (adaptada de la spec de [VC] `specs/02-emparejamiento.md`, que la aplica a anuncios de Idealista, y de [IT]):

1. **RC directa**: si el anuncio trae RC (algunos la publican) → `Consulta_DNPRC` con la de 20 car.; confianza alta. Permitir que el usuario pegue/elija la RC y guardarla (tiene prioridad).
2. **Dirección** (si hay calle y número): `Consulta_DNPLOC` (trae ya las unidades). Error 43 → usar números cercanos.
3. **Coordenadas**: `Consulta_RCCOOR_Distancia` (cuadrado de 50 m) → parcelas ordenadas por `dis`; tomar las **3–5 más cercanas** (cada parcela = 1 petición `DNPRC` de 14 car.; respetar §5). [VC] usa además el WFS INSPIRE por BBOX hasta 250 m, pero mide 9–11 s y ~0,9 MB en Madrid centro: no necesario para Mallorca en la Fase 3.
4. **Filtrar** unidades con `luso` = Residencial (puntuar, no descartar, las demás para detectar "local como vivienda").
5. **Puntuar** cada unidad (pesos de [VC]): superficie dentro de ±15 % → 0,45; planta coincide → 0,35; año ±5 → 0,10 (>10 años de diferencia resta 0,10); uso residencial → 0,10. Normalizar sobre las señales disponibles. Desempate por `dis`.
   - Superficie: comparar los m² **construidos** del anuncio con `sfc`; si el anuncio solo da útiles, usar útil × 1,2 ([VC]). Si se tiene `lcons` (consulta de 20 car.), comparar también con la superficie privativa (Σ `stl` sin "ELEMENTOS COMUNES").
   - Garaje/trastero incluidos: en el Catastro son **unidades aparte** (`Almacen-Estacionamiento`, misma RC14); probar anuncio ≈ vivienda + anexo (±5 %) con anexo ≤ 50 m² de la misma parcela ([VC]).
   - Planta: normalizar `pt` (`00`/`BJ` → bajo, `EN` → entreplanta, `AT` → ático, negativas/`SS`/`ST` → sótano, `01` → 1, `OD` → otro) y la del anuncio ("3ª", "bajo", "ático").
6. **Confianza**: alta si 1.º ≥ 0,8 y ventaja ≥ 0,2 (nunca alta solo con coordenadas aproximadas); media si 1.º ≥ 0,6 y (ventaja ≥ 0,1 o ≤ 3 empatados); baja en otro caso. En baja/media, mostrar los candidatos y pedir confirmación.

**Tolerancias superficie anuncio vs `sfc`** ([VC] `specs/03-discrepancias.md`): ≤ 10 % "ok" ("El Catastro incluye zonas comunes; diferencias de hasta un 10 % son normales"), 10–20 % "revisar", > 20 % "aviso" (salvo que lo explique un anexo). Base empírica: en el ejemplo real de §2.2 los comunes son 5/41 ≈ 12 % de `sfc`; en edificios con muchas zonas comunes la diferencia puede ser mayor (la OCU dice que el desfase "puede ser mayor en grandes promociones o chalés", sin cifras). Las terrazas cubiertas computan al 50 % (100 % si están cerradas por tres lados) y los espacios < 1,50 m de altura no computan (OCU). Rango de tolerancia ±15 % para emparejar y ±10/20 % para avisar: **heurística de [VC], no norma oficial**.

---

## 8. Notas de implementación .NET

- Deserializador tolerante: algunas propiedades pueden venir como objeto o como array (`bico` vs `lrcdnp.rcdnp`, `lcons`, `lspr`, `lerr`); [VC] normaliza ambos a lista. Usar `JsonElement` o un `JsonConverter` "SingleOrArray".
- Números: `sfc` → quitar `.` de miles; `cpt` → `,` decimal; `dis`, `xcen` → punto decimal. Parsear con `CultureInfo.InvariantCulture` tras normalizar.
- Coordenadas en query: formatear con `InvariantCulture` (una coma decimal provocaría error 77).
- Recomponer RC = `pc1+pc2+car+cc1+cc2`; parcela = `pc1+pc2`.
- Fixtures para tests: copiar las respuestas literales de §2–§4 (no llamar a la red en tests).

---

## 9. Fuentes (consultadas 2026-11-15)

Oficiales:
- Esquemas y endpoints: https://www.catastro.hacienda.gob.es/ws/esquemas.htm
- Servicios restringidos (para contraste): https://www.catastro.hacienda.gob.es/ws/WCFservices_Consulta.pdf
- Condiciones de uso SEC: https://www.catastro.hacienda.gob.es/ayuda/condicionesuso.htm
- Ayuda Sede (datos protegidos/no protegidos): https://www.catastro.hacienda.gob.es/ayuda/default.htm
- FAQ Sede: https://www.catastro.hacienda.gob.es/ayuda/faq.htm
- Licencia-tipo de descarga: https://www.catastro.hacienda.gob.es/pdf/ovc/licdescargaES.pdf
- Mapas de valores (leyenda): https://www.catastro.hacienda.gob.es/ayuda/leyenda_mapavalores.htm
- Circular 02.04/2026/P: https://www.catastro.hacienda.gob.es/documentos/02042026_P.pdf
- Nota de prensa mapas de valores 2021: https://www.hacienda.gob.es/Documentacion/Publico/GabineteMinistro/Notas%20Prensa/2021/S.E.HACIENDA/14-10-21-NP-MAPAS-DE-VALORES-DEL-CATASTRO-Y-OM-DE-MINORACION.pdf y https://www.lamoncloa.gob.es/serviciosdeprensa/notasprensa/hacienda/paginas/2021/141021-catastro.aspx
- BOE-B-2025-36293 (audiencia VR 2026): https://www.boe.es/boe/dias/2025/10/08/pdfs/BOE-B-2025-36293.pdf
- BOE-B-2025-46479 (VR rústicos 2026): https://www.boe.es/boe/dias/2025/12/16/pdfs/BOE-B-2025-46479.pdf

Código abierto (respuestas reales y clientes):
- https://github.com/Jamarlli/viviendaChecker (`fixtures/catastro/`, `specs/02-emparejamiento.md`, `specs/03-discrepancias.md`, `src/adapters/catastro/cliente.ts`)
- https://github.com/sergi039/idealista-tracker-ai (`services/cadastre_service.py`, `tests/data/catastro_dnprc_*.json`)
- https://github.com/jorgebg90/catastro-rest-api (`tests/fixtures/soap/`)
- https://github.com/BquantFinance/Administracion-fuentes-publicas (`sources/territorio-cartografia/catastro-ovc.yaml`)
- https://github.com/alejndr0/go-catastro (`catastro/struct.go`, `catastro/client.go`, `catastro/COVCCoordenadas.go`)
- https://github.com/rOpenSpain/CatastRo (`R/ovc-rccoor-dist.R`, `R/utils-ovc.R`, `R/docs.R`, `tests/testthat/_snaps/ovc-rccoor-dist.md`)
- https://github.com/gisce/pycatastro (`pycatastro/__init__.py`)
- https://github.com/josejuanmartinez/libreCatastro (`src/librecatastro/scrapping/scrappers/scrapper_xml.py`)
- https://github.com/jesusvaros/CaseroVerificado (`src/services/publicData/catastro.ts`)
- https://github.com/CabhuDev/mcp_Catastro (`docs/Guia_Completa_API_Catastro.md`; **poco fiable**: su ejemplo JSON usa raíz `consulta_dnp`, que no coincide con las respuestas reales `consulta_dnprcResult`)

Secundarias:
- Xataka (consulta VR): https://www.xataka.com/basics/como-saber-valor-casa-piso-valor-referencia-catastro
- Hipotecas.me (VR): https://www.hipotecas.me/consulta-catastral/valor-de-referencia
- OCU (superficie construida y comunes): https://www.ocu.org/fincas-y-casas/gestion/urbanismo-y-construccion/analisis/2020/09/superficie-util
- Tinsa (catastro = superficie construida): https://www.tinsa.es/blog/arquitectura/diferencia-superficie-construida-util/

---

## 10. Lista NO VERIFICADO

1. Ninguna respuesta se ha obtenido **en directo** en esta sesión (hosts del Catastro inaccesibles desde el proxy/shell); todo formato procede de fixtures grabados por terceros (2026-08/10) y código.
2. `robots.txt` de `ovc.catastro.meh.es` y `ovc.catastro.hacienda.gob.es`.
3. Existencia/uso de un host `ovc.catastro.hacienda.gob.es`.
4. Que el ASMX falle si se **omiten** (en vez de enviar vacíos) `Provincia`/`Municipio`.
5. Si `lrcdnp.rcdnp[]` en **XML ASMX** trae `debi` (el fixture XML disponible no lo trae; el JSON sí).
6. Parámetros JSON de `Consulta_CPMRC` (`RefCat`, `Parcela`) — solo vistos en [GO].
7. Texto literal del error 76 y lista completa de códigos del servicio libre (no hay documento oficial público con la tabla).
8. Qué User-Agents exactos acepta el WAF; cifra real del límite (solo observación de [BQ]: ~15/min → corte ~10 min).
9. Que la licencia-tipo de descarga se aplique a los servicios web libres.
10. Descarga estructurada (shapefile/CSV/WMS con valores) de los **mapas de valores**; nombre de capa WMS.
11. Si un **no titular** puede obtener el VR en la Sede sin certificado (Xataka sugiere NIF propio + reCAPTCHA; Hipotecas.me dice certificado/Cl@ve).
12. Nombres exactos de provincia/municipio de Mallorca para `DNPLOC` (p. ej. `ILLES BALEARS`/`BALEARS`, `PALMA`) — obtener con `ObtenerProvincias`/`ObtenerMunicipios`.
13. Valores posibles de `luso`/`lcd`/`pt` más allá de los vistos (`Residencial`, `Comercial`, `Almacen-Estacionamiento`, `Obras de urbanización y jardineria, suelos sin edificar`; `VIVIENDA`, `ALMACEN`, `COMERCIO`, `ELEMENTOS COMUNES`, `OTROS USOS`; `00`, `01`…, `-1`, `AT`, `OD`).
14. Tolerancias ±10/15/20 %: heurística de proyectos de terceros, no norma.
