# Catálogo de CRM / plataformas web de agencias inmobiliarias en Mallorca

Fecha de verificación: 2026-11-14. Método: búsquedas web + lectura (WebFetch, render Markdown) de la home o de una página de listado de ~30 webs de agencias de Mallorca y de documentación pública de los CRM. WebFetch solo devuelve texto renderizado, no el HTML crudo: los *scripts* no son visibles, así que las huellas se basan en rutas, hostnames de imágenes, meta tags visibles (`generator`/`author`), créditos de pie de página y patrones de URL. Todo lo que no se pudo comprobar está marcado **(no verificado)**. Las URLs de feed que son deducción llevan **(inferido)**.

Objetivo: un adaptador por CRM/feed, no un scraper por agencia.

---

## 1. Inmovilla (by idealista) — `inmovilla.com`

- **Quién es:** CRM inmobiliario de Elche, hoy "Inmovilla by idealista" (Trustpilot lo lista así; soporte soporte@inmovilla.com, +34 966 673 149: https://ca.trustpilot.com/review/inmovilla.com). Afirma "más de 3 mil agencias" (https://www.softwaredoit.es/inmovilla/inmovilla.html). Desde 45 €/usuario/mes, +20 portales, "web y app incluidas" (https://www.upliora.es/blog/automatizar-inmobiliaria-ia-crm-portales-whatsapp-2026).
- **Cuota en Mallorca:** la más alta entre las detectadas: 4 de 27 webs leídas (Inmobiliaria Mir, Incasa Properties, Fincas Fiol, Lighthouse Living). Aparece en dos variantes:
  - **Web nativa Inmovilla (PHP):** `inmobiliariamir.es`, `incasaproperties.com`.
  - **WordPress + "pluginInmovilla":** `fincasfiol.com` (fotos en `fotos15.apinmo.com`), `lighthouse-living.com` (ruta `wp-content/uploads/lh-inmovilla/{id}/`).
- **Cómo expone los anuncios (web nativa):** HTML server-side (PHP). Rutas: `index.php`, `seccion.php`, `alertas.php`; detalle `/ficha/{tipo}/{municipio}/{zona}/{idAgencia}/{idAnuncio}/es/` (ej. `/ficha/piso/llucmajor/s-arenal/4445/29483722/es/` en Mir; `/ficha/apartamento/felanitx/portocolom/11897/28021423/es/` en Incasa). El penúltimo segmento es constante por agencia (código de cuenta), el último es el **ID global de anuncio (8 dígitos)**.
- **Variante WordPress:** detalle `/propiedad/{slug}-{idAnuncio}/` (Fiol: `/propiedad/piso-en-venta-en-paseo-mallorca-27575741/`; Lighthouse: `/propiedad/...-30517032/`). Imprimibles vía `/wp-content/plugins/pluginInmovilla/imprimibles.php`. Fotos en `fotosNN.apinmo.com/{idAgencia}/{idAnuncio}/{n}-{tamaño}.jpg` (`fotos15.apinmo.com/3257/27575741/4-1.jpg`). Meta `article:published_time` / `article:modified_time` presentes en la ficha WP (2023-05-05 en Fiol), pero no visibles en la página.
- **ID estable:** el ID numérico de 8 dígitos de Inmovilla (compartido entre web, fotos y plugin). Además "Referencia" corta de agencia (ej. 7098, 26002, C-3057).
- **Campos visibles:** referencia, tipo, municipio/zona, precio, dormitorios, baños, m², descripción, fotos, orden "Más reciente / Más antiguo" (implica fecha interna) pero **sin fecha de publicación visible**.
- **Feeds/API:** Inmovilla genera XML para portales (Kyero y "Inmovilla XML") y tiene una API propia bajo `apinmo.com` **(no verificado; no se pudo abrir la documentación en esta sesión)**. Patrón de feed público por agencia: no observado en ninguna web → **no inventar**; obtenerlo por acuerdo con la agencia.
- **Antibot:** cookie `PHPSESSID`, banner "Cookieguard"; WebFetch leyó sin problema las dos webs nativas y las dos WP.

## 2. Witei — `witei.com`

- **Quién es:** CRM madrileño, "más de 18.000 profesionales" (https://get.witei.com/en/real-estate-software), desde 29 €/usuario/mes (upliora). Ciclo medio de sincronización con portales: 15 minutos.
- **Cuota en Mallorca:** 1 de 27 webs leídas (`inmosoller.com`, Sóller). Muy extendido en la península; en Mallorca presencia menor **(estimación)**.
- **Cómo expone los anuncios:** web alojada por Witei bajo dominio de la agencia (Cloudflare: rutas `/cdn-cgi/`). Detalle `/house/{idNumérico}/` (ej. `https://www.inmosoller.com/house/4297342/`). Imágenes de marca/blog en `witei-media.s3.amazonaws.com` (`web_logos`, `web_carousels`, `blog_articles`) y `d2ctzk1imdlpfx.cloudfront.net`.
- **ID estable:** ID numérico global de Witei (7 dígitos) en la URL. Sin referencia visible ni fecha en tarjetas; etiquetas "Vendido/Reservado".
- **Feeds/API (documentado):** (a) **XML Kyero V3** por agencia, "actualizado en tiempo real", enlace privado que se copia en Ajustes > Interconexiones (https://faq.witei.com/en/articles/231207-how-do-i-connect-an-external-website-with-witei); (b) **Webhooks** POST JSON por cada actualización de inmueble; (c) **API** con token de agencia (Configuración > Desarrolladores > API). Patrón de URL del XML: no publicado **(no verificado)**.
- **Antibot:** Cloudflare (email-protection). WebFetch OK.

## 3. Mobilia Gestión — `mobiliagestion.es`

- **Quién es:** CRM + web para agencias "desde autónomos a franquicias" (https://www.softwaredoit.es/mobilia/mobilia.html; https://www.capterra.es/software/198549/mobilia-gestion-crm). Empresa con base en Baleares **(no verificado en esta sesión)**.
- **Cuota en Mallorca:** 1 de 27 (`inmovyp.es`, Inca). Se estima presencia relevante en agencias pequeñas de la isla **(estimación)**.
- **Cómo expone los anuncios:** HTML server-side; pie "Creado con Mobilia" enlazando a mobiliagestion.es. Detalle `/es/{municipio}/{zona?}/{tipo}/ref-{id}` (`/es/inca/cristo_rey/fincas_rusticas/ref-2440`). Fotos en `media.mobiliagestion.es/Portals/{nombrePortal}/Images/{ref}/{fotoId}.jpg` (`/Portals/inmoinmovyp/Images/...`).
- **ID estable:** `ref-{n}` (referencia de agencia). **Las tarjetas muestran un entero YYYYMMDD (ej. 20261111) que parece fecha de alta/actualización** — único CRM observado con fecha en el listado.
- **API pública (documentada):** `https://api.mobiliagestion.es/api/v1`, OAuth2 client credentials (`POST /api/v1/token`), `GET /inmuebles`, `/inmuebles/todos`, `/inmuebles/desactivados`, `/inmuebles/{referencia}`; paginación `NumeroPagina`/`TamanoPagina`(≤100); filtro `FechaUltimaModificacion` ISO-8601; límite 10 req/min (https://claudskills.com/skills/mobilia-api/SKILL.md). Requiere credenciales de la agencia.
- **Antibot:** ninguno observado.

## 4. Inmopc / Inmovirtual — `inmopc.com`

- **Quién es:** software de gestión + diseño web; "exportación automática desde INMO PC a los principales portales" (https://www.appvizer.es/construccion/software-inmobiliario/inmopc?nocache=true).
- **Cuota en Mallorca:** 1 de 27 (`nensiola.com`, Colònia de Sant Jordi/Santanyí).
- **Cómo expone los anuncios:** HTML estático-like `.html`; listados `/venta-comprar/{tipo}/islas-baleares/{municipio}/{zona}/venta-comprar-....html`; "Mapa Web" HTML `/mapa-web.html`. Back-office `crm325.inmovirtual.es/GenWeb/login` (enlace "Admin" en el pie) → patrón `crm{N}.inmovirtual.es`. Pie: "Design Crm Web Inmobiliario INMOPC".
- **ID / fechas:** no visibles en la home; patrón de ficha **(no verificado)**.
- **Feeds:** exporta a portales (formato no documentado públicamente) **(no verificado)**.

## 5. InfoCasa CRM (MODX) — `infocasa.com`

- **Quién es:** "InfoCasa CRM S.L."; webs diseñadas por la agencia mallorquina Rex4media (`rex4media.com`). No aparece en los comparadores nacionales → producto local/regional **(inferido)**.
- **Cuota en Mallorca:** 3 de 27 (`prestigevillas.com` Pollença, `mpcweb.net` Artà, `lucie-hauri.com` Santanyí). Perfil medio-alto/lujo pero con stock de pueblo.
- **Cómo expone los anuncios:** CMS MODX Revolution; assets en `/core/components/infocasa/files/themes/{tema}/`. Detalle `/es/property/id/{idNumérico}-{slug}` (`/es/property/id/977826-townhouse-...-in-arta`). Alquiler vacacional `/es/holiday-rental/{id}-{slug}`.
- **ID estable:** ID numérico global de 6 dígitos (973083, 977826…) + "Ref" de agencia (Ref: 2718; "970338 6126W"). Badges "Nueva/Nuevo", "Premium"; sin fecha.
- **Feeds:** no observados **(no verificado)**.

## 6. ImmoLED (LEDMedia) — `led-media.net`

- **Quién es:** desarrollador web mallorquín (LEDMedia, "art digital"); producto "ImmoLED".
- **Cuota:** 1 de 27 (`futurinca.com`, Inca).
- **Huella:** `<meta name="generator" content="ImmoLED® 4.2">`. Fotos en `/fotografias/`, `/miniaturas/`, `/galeries/imatges/`; placeholder `/imatges/no_image.png`; buscador `/buscador?poblacio=Inca`; formulario "Directo a inmueble" por referencia numérica. Las tarjetas de la home no enlazan a ficha (patrón de detalle **no verificado**). Sin fecha.

## 7. Solutra — `solutra.ai`

- **Quién es:** CRM/web nuevo (IA) usado por Casal Mallorquí (Manacor). "Made by Solutra".
- **Cómo expone:** front moderno (Supabase para assets: `rofcgmpuemezwlskdrmp.supabase.co`); **API pública JSON**: `https://casalmallorqui-crm.solutra.ai/api/public/v1/listings/{ref}/images/{n}` → patrón `{agencia}-crm.solutra.ai/api/public/v1/listings/...` **(inferido para el listado; solo se observó el endpoint de imágenes)**. Detalle `/es/propiedades/{ref}` (`V743`). Orden "Más recientes", sin fecha visible.

## 8. WordPress con tema inmobiliario (sin CRM detectable)

- Casos: `inmobiliariacoll.com` (Sóller; `/property/{slug}/`, `/status/venta/`, `/city/soller-es/`, Slider Revolution, WPML), `gestpropiedad.com` (red de oficinas Palma/Inca/Manacor/Llucmajor; `/inmueble/{venta|alquiler}/{tipo}/{illes-balears}/{municipio}/{ref}/`, uploads `gp-social/`, **tiempo relativo "hace 7 horas"** en tarjetas), `remax-calm.es` (Elementor; listados delegados a `propiedades.remax-calm.es`), `mallorcasol.com` (Houzez, inferido por nombre de logo).
- Para estos el adaptador es "WordPress genérico": sitemap `wp-sitemap.xml`/`sitemap_index.xml` y REST `/wp-json/wp/v2/` **(inferido; no comprobado por sitio)**. `/wp-json/wp/v2/posts/{id}` sí se vio indexado en otra web mallorquina (privatepropertymallorca.com) en resultados de búsqueda.

## 9. Desarrollos a medida (un adaptador por web)

| Agencia | Municipio | Señales |
|---|---|---|
| Riera & Taylor `rierataylor.com` | Manacor | `/rt{id}/{slug}/`; fotos `rierataylor.s3.gra.io.cloud.ovh.net/{id}/{id}-foto-{hash}-mini.webp`; sin créditos |
| Mundo Pisos `mundopisos.com` | Palma | meta author "GRDAR"; `/es/propiedades-inmobiliarias/{tipo}-{zona}-{ref}`; `/upload/g-*.webp`; `/es/sitemap`; parámetros `buscador-referencia`, `buscador-minimo`…; salida `print_r` de PHP visible |
| Riusech `riusech.com` | Port de Pollença | `/es/propiedades-en-venta/{pueblo}/{id}`; assets `?v=1.14.0`; meta `fragment: !` |
| Angela's Home `angelashome.es` | Santanyí | Teamhost Studio; `/es/venta/{tipo}/{ciudad}/{cat}/{ref}/{id}` |
| Artà Immobilien `artaimmobilien.com` | Artà | Corsoft D+D; `/propiedades/{slug}/`, `/static/img/`, `/login/`, filtros `?tipo_transaccion=&zona=&localidad=&tipo=` |
| Engel & Völkers Mallorca `ev-mallorca.com` | varias | `/es/inmueble-mallorca/{slug}-W-XXXXXX`; CDN `cdn.ev-mallorca.com/images/properties/` |
| DAHLER `dahlercompany.com` | Santanyí/Campos | Drupal 11; búsqueda `?officeId=MSE&dcRegion=mallorca` |
| AM Inmobiliaria `am-inmobiliaria.com` | Santanyí | TYPO3 + ext. `nw_realestatemanager`; **RSS de todas las propiedades** `/es/rss-todas-las-propiedades-mallorca?type=9819` |

## 10. CRM nacionales sin agencia detectada en la muestra

Documentados para que el detector los reconozca; cuota en Mallorca desconocida.

- **Sooprema** (`sooprema.com`): SaaS CRM + web + marketing, desde 29 €/mes, +40 portales (https://www.softwaredoit.es/software-inmobiliarias-promotoras/software-agencias-inmobiliarias.html). Orientado a agencias pequeñas, "solo España" (upliora). Huellas de web **(no verificado)**.
- **Inmoweb** (`inmoweb.es`, Web Experience): "más de 8 mil agentes… desde 2003", incluye página web (https://www.softwaredoit.es/inmoweb-soluciones/inmoweb-soluciones.html). Perfil redes/franquicias multioficina. Huellas **(no verificado)**.
- **eGO Real Estate / Janela** (`egorealestate.com`): CRM portugués con "API completa", clientes internacionales, desde 55 €/usuario/mes (upliora). Huellas **(no verificado)**.
- **Resales Online** (`resales-online.com`): MLS de la Costa del Sol. Feeds XML documentados: tipos OwnProperties / ReSales / Rentals / New Developments / Portals (https://intercom.help/ReSales-Online/en/articles/5683244-what-are-the-different-types-of-xml-export-feeds); URL de ejemplo `https://xmlout.resales-online.com/live/Resales/Export/CreateXMLFeedV3.asp?U=OWNPROPERTIES@DEMO&P=…&NPT=TRUE&N=10&Language=1,2` (formato "similar a Kyero V2.1"; https://intercom.help/ReSales-Online/en/articles/5194881-we-are-a-property-portal-can-we-have-a-feed); feeds incrementales vs completos (https://intercom.help/ReSales-Online/en/articles/5409759-what-are-incremental-and-non-incremental-feeds); WebAPI en `webapi.resales-online.com` **(no verificado; devdocs no accesible)**. Ninguna agencia de Mallorca de la muestra lo usa.
- **Inmogesco**, **Inmofactory** (Adevinta/Fotocasa **(no verificado)**), **Nexo**, **Habitania**, **Inmotek**, **HU Ingenieros**, **Inmobalia** (Marbella; su XML es variante de Kyero según https://www.wp-immomakler.de/en/docs-artikel/kyero-idealista-inmobalia-immobilien-import-export-wordpress-plugin/), **Propertybase** (Salesforce, internacional), **HubSpot** (solo CRM de contactos, no publica inmuebles): sin evidencias en Mallorca **(no verificado)**.
- **Idealista Tools / Fotocasa Pro:** son back-offices de portal, no webs de agencia; en Mallorca muchas agencias pequeñas (p. ej. Martorell Arrom, Inca) solo tienen página `idealista.com/pro/{slug}/` sin web propia. Para ellas la fuente es el portal, no un CRM.
- **"Olvera"**: no se encontró ningún CRM con ese nombre **(no verificado; posible confusión)**.
- **Plataformas de franquicia:** RE/MAX España (`{oficina}.remax.es`, `propiedades.{dominio}` con rutas `/es/venta-{tipo}/en-baleares-{municipio}`), Engel & Völkers, Kensington, DAHLER: un adaptador por franquicia.
- **Plataforma compartida `buscovivienda.es/{agencia}/`:** Servinord (Sa Pobla) publica en un subdirectorio de ese dominio → posible multi-agencia **(no verificado)**.

---

## 11. Formatos de feed XML

| Formato | Esquema / doc | Notas |
|---|---|---|
| **Kyero V3** | https://help.kyero.com/article/354-xml-import-specification (enlaza `kyeroV3.0.xsd` y spec V3 .txt; el .txt en `feeds.kyero.com` está bloqueado por robots) ; export: https://help.kyero.com/xml-export-specification (`http://feeds.kyero.com/assets/kyero_v3_export_spec.txt`) | Raíz `<kyero>` con `<feed_version>3</feed_version>`; uno `<property>` por anuncio: `id` (clave de alta/baja), `date` ("YYYY-MM-DD HH:MM:SS", **debe cambiar para que se procese la actualización**), `ref`, `price`+`currency` (EUR/GBP/USD), `price_freq` (sale/month), `new_build`, `type`, `town`, `province`, `country`, `location` (lat/long), `beds`, `baths`, `pool`, `surface_area` (built/plot), `energy_rating`, `url`, `desc` por idioma, `features`, `images` (≤50, atributo `id`, `tags` floorplan), `video_url`, `virtual_tour_url`, `catastral`, `prime`, `contact_number`. UTF-8, minúsculas, sin CDATA. Es el formato de facto: lo emiten Witei, Inmobalia, Resales, WP-ImmoMakler; lo aceptan idealista, thinkSPAIN y "la mayoría de portales" (https://www.calpeonline24.com/info/guide-to-xml-feed-building-and-maintenance-information-from-property-portal-marketing). |
| **thinkSPAIN** | https://www.thinkspain.com/faqs/sellers-and-advertisers | Acepta feeds Kyero V3 (Resales lista ThinkSpain entre destinos de feed completo). Formato propio **(no verificado)**. |
| **Inmovilla XML** | documentación no accesible en esta sesión **(no verificado)** | Formato propio de Inmovilla aceptado por portales españoles; muchos CRM ofrecen "exportar en formato Inmovilla". |
| **idealista** | JSON de ejemplo: https://gist.github.com/jjmontalban/2ed1fe14fa88a7f2b03671df53ae643d ; plugin WP: https://wordpress.org/plugins/properties-feed-for-idealista/ ; guía: https://tesoro-crm-website-bmq.pages.dev/en/knowledge-base/idealista-integration-guide | Estructura `customerCode`/`customerReference` → `properties[]` con `propertyCode`, `propertyReference`, `propertyUrl`, `propertyOperation{operationType,operationPrice}`, `propertyAddress{…Latitude,…Longitude}`, `propertyFeatures{featuresType, featuresAreaConstructed, featuresBedroomNumber…}`, `propertyDescriptions[]`, `propertyImages[]`. Requiere código ILC de idealista (https://intercom.help/ReSales-Online/en/articles/5194736-how-can-i-get-a-feed-of-my-properties-for-idealista). idealista también acepta Kyero XML. |
| **Fotocasa** | https://tesoro-crm-website-bmq.pages.dev/en/knowledge-base/fotocasa-integration-guide | Campos `title`(≤100), `description`(≤2000), `price`, `propertyType`, `operation`, `address`, `city`, `postalCode`, `surface`, `images`(1-40); límite 1.000 req/h. Formato XML/API propio (Inmofactory) **(no verificado)**. |
| **SpainHouses XCP** | https://www.spainhouses.net/es/inmobiliarias/especificaciones-publicacion.html (XSD `https://xcp.entersoftweb.com/sh/xsd/cartera.xsd`) | Raíz `<carteraPropiedades>` con `<fecha>` ISO; `<propiedad>` con `referencia`, `fecha`, `tipoInmueble`, `tipoOferta`, `precio`, `provincia`, `localidad`, `codigoPostal`, `geoLocalizacion`, `descripcion` por idioma. |
| **Resales Online XML** | ver §10 | Variante Kyero V2.1; feeds completos o incrementales. |

---

## 12. Ranking de CRM por cobertura (muestra de agencias de Mallorca)

Fuente: `agencies-mallorca.csv` (76 filas; 27 webs leídas y clasificadas; 49 `unknown` porque WebFetch solo admite URLs aparecidas en resultados de búsqueda y el cupo de búsquedas de la sesión se agotó antes de poder localizar esas webs).

| CRM / plataforma (`crm_detected`) | Agencias | Quiénes |
|---|---|---|
| inmovilla (nativo + plugin WP) | 4 | Mir, Incasa, Fincas Fiol, Lighthouse Living |
| custom (desarrollo a medida) | 5 | Riera & Taylor, Mundo Pisos, Riusech, Angela's Home, Artà Immobilien |
| infocasa (MODX) | 3 | Prestige Villas, MPC, Lucie Hauri |
| wordpress-generic (sin CRM visible) | 3 | Coll, GestPropiedad, Mallorcasol |
| remax-es (plataforma franquicia) | 2 | RE/MAX Calm, RE/MAX Del Mar (inferido) |
| solutra (API JSON) | 1 | Casal Mallorquí |
| mobilia | 1 | InmoVYP |
| witei | 1 | Inmosoller |
| inmopc / inmovirtual | 1 | N'Ensiola |
| immoled (LEDMedia) | 1 | Futurinca |
| typo3-realestate (con RSS) | 1 | AM Inmobiliaria |
| engel-voelkers / dahler-drupal | 1 + 1 | E&V Artà, DAHLER Santanyí |
| portal-only (solo idealista) | 1 | Martorell Arrom |
| buscovivienda-shared (no verificado) | 1 | Servinord |
| unknown (bloqueada o no leída) | 49 | Pollensa Estates (robots), Yes Mallorca, Kensington, Toñi Ruiz + 45 webs no leídas |

### Tres adaptadores a implementar primero

1. **Inmovilla** (nativo PHP + plugin WordPress). Mayor cobertura observada y, por ser el CRM de idealista con "+3.000 agencias", el de mayor probabilidad de aparecer en las ~43 webs aún sin clasificar. El ID de 8 dígitos es global y aparece en URL y en `fotosNN.apinmo.com/{agencia}/{id}/`, lo que permite deduplicar entre la web de la agencia y los portales. Dos parsers de HTML (ficha nativa `/ficha/…/{id}/es/` y ficha WP `/propiedad/…-{id}/`) comparten el modelo.
2. **Kyero V3 XML (adaptador de feed genérico).** Cubre de golpe Witei (XML Kyero por agencia, tiempo real), Inmobalia, Resales, WP-ImmoMakler y cualquier agencia que acceda a compartir su feed de portal; también sirve para importar desde thinkSPAIN/idealista si se obtiene el feed. Trae `id` y `date` (fecha de actualización), que es justo lo que falta en casi todas las webs. Coste bajo: un deserializador XML + mapeo de tipos.
3. **Mobilia** (API REST `api.mobiliagestion.es/api/v1` + web "Creado con Mobilia"). Es el único CRM observado que expone fecha (YYYYMMDD en tarjetas) y `FechaUltimaModificacion` en API, es de ámbito balear y su web tiene un patrón muy regular (`/ref-{id}`, `media.mobiliagestion.es/Portals/{portal}/Images/{ref}/`). Buen candidato para agencias pequeñas de Inca/Pla/Nord.

Siguiente candidato: **InfoCasa CRM** (3 agencias, patrón `/es/property/id/{id}-{slug}` idéntico en las tres) y un adaptador **WordPress genérico** (REST `wp-json` + sitemap) para el bloque de webs WP sin CRM.

### Ranking actualizado (segunda pasada, 2026-11-14)

Fuente: `agencies-mallorca.csv` tras la segunda pasada (76 filas: 60 clasificadas con plataforma, 4 sin web, 6 bloqueadas, 4 `unknown` sin web localizable, 2 inferidas sin leer). De las 49 `unknown` de la primera pasada se resolvieron 35 (más 4 `no-web` y 6 `blocked`).

| CRM / plataforma (`crm_detected`) | Agencias | Quiénes (nuevas en negrita) |
|---|---|---|
| custom (desarrollo a medida) | 12 | Riera & Taylor, Mundo Pisos, Riusech, Angela's Home, Artà Immobilien, **Hernán Nogués, Inmobiliaria Alcudia (Artífices), Propiedades Pollensa, Palmer (Melvyx), Palmacasa (SaaS no identif.), Inmogestión Balear, Inmobiliaria Campos (SPA)** |
| inmovilla (nativo + plugin WP + front propio) | 11 | Mir, Incasa, Fincas Fiol, Lighthouse, **Bono, Socies i Fills, Mallorca Gestión, Coco (WP), Amelia (Next.js), A8, Mallorhouse** (+ GestPropiedad probable) |
| wordpress-generic (sin CRM visible) | 10 | Coll, GestPropiedad, Mallorcasol, **Inmobiliaria Port, Llucmajor, Tresidence, Holzmüller, Fincas Torrens, Perera & Partner, Inmoforo** |
| infocasa (MODX + Rex4media) | 5 | Prestige, MPC, Lucie Hauri, **SolMallorca, Pollentia Properties** |
| inmopc / inmovirtual | 2 | N'Ensiola, **CFemenía (cfemenia.es)** |
| onoffice (smartSite, alemán) | 2 | **Immallo (SSP Serveis Sa Pleta), Sóller Studio** |
| remax-es | 2 | RE/MAX Calm, RE/MAX Del Mar |
| resales-online (plugin RealtySoft WP) | 1 | **AZ Inmobiliaria** |
| verkia (Joomla) | 1 | **Living Palma** |
| mediaelx | 1 | **VG Inmobiliaria** |
| pisoscom-web (web alojada por pisos.com) | 1 | **IBhabitat** |
| joomla-osproperty | 1 | **Infocasa Colom / Tony Martínez** |
| wordpress-houzez | 1 | **Pascual Associats** (Mallorcasol solo inferido) |
| wordpress-estatik | 1 | **MT Inmobiliaria Manacor** |
| wordpress-realestate-theme (CPT estate, WPBakery) | 1 | **Fincas Cala Major** |
| sitebuilder-tucomunidad (sin listados) | 1 | **Andreu Felipe** |
| solutra / mobilia / witei / immoled / typo3-realestate / engel-voelkers / dahler-drupal / portal-only / buscovivienda-shared | 1 cada uno | Casal Mallorquí, InmoVYP, Inmosoller, Futurinca, AM Inmobiliaria, E&V Artà, DAHLER, Martorell Arrom, Servinord |
| no-web | 4 | Inmo Conspro, Ca Teva, Inmobiliaria Solidaria, Marsant |
| blocked (robots/antibot/redirect http) | 6 | Pollensa Estates, Sun Villas, Royal Haus (robots); Yes Mallorca, Kensington (client_error); Toñi Ruiz (solo http) |
| unknown (web no localizable) | 4 | Alcanada (publica en Kyero/Casasapo), Fair Mallorca, Firamon, Grans Viles |

**Lectura para los adaptadores.** Inmovilla pasa de 4 a 11 agencias (15 % de la muestra; 18 % de las webs con listados) y es claramente el primer adaptador: además del nativo y del plugin WP aparecen un front Next.js (Amelia) con los mismos IDs de 8 dígitos y un feed `/rss` en los nativos. InfoCasa sube a 5 y supera a Mobilia (1) y Witei (1) en cobertura directa; el feed Kyero sigue siendo el mejor segundo adaptador por su efecto multiplicador (Witei, Resales Online/RealtySoft, Alcanada vía Kyero, Inmobalia), pero **Mobilia como tercer adaptador ya no se sostiene con los datos de Mallorca**: la recomendación revisada es Inmovilla → Kyero XML → InfoCasa (patrón `/property/id/{id}-{slug}` idéntico en 5 webs), dejando Mobilia y onOffice (2 agencias, plantilla muy regular `immobilien.xhtml?id[obj0]=`) como cuarto/quinto. El bloque WordPress genérico (10) justifica un adaptador `wp-json` + sitemap de bajo coste.
