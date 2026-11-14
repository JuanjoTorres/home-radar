# Huellas de detección de CRM / plataforma web (Mallorca)

Fecha: 2026-11-14. Las huellas salen de la lectura de webs reales (ver `agencies-mallorca.csv`). WebFetch devuelve texto renderizado, por lo que las señales en `<script src>` no se pudieron ver; el detector debería aplicar estos regex sobre el **HTML crudo completo** (head + body + comentarios), sobre la **URL de los enlaces** y sobre los **hostnames de imágenes**. Confianza: `alta` = señal exclusiva del producto; `media` = fuerte pero compartible; `baja` = indicio.

Regla de decisión sugerida: puntuar cada CRM sumando señales (alta=3, media=2, baja=1); ganador si ≥3 y supera al segundo en ≥2. Si solo hay señales `wordpress`, clasificar como `wordpress-generic`. Si ninguna, `custom`.

| CRM | Tipo de señal | Patrón (regex, case-insensitive) | Confianza | Evidencia |
|---|---|---|---|---|
| inmovilla | meta author | `<meta\s+name=["']author["']\s+content=["'][^"']*inmovilla` | alta | https://www.inmobiliariamir.es/?idio=1 ; https://www.incasaproperties.com/es/ |
| inmovilla | texto pie | `Dise[ñn]ado por CRM Inmovilla` | alta | https://www.inmobiliariamir.es/?idio=1 |
| inmovilla | enlace | `facebook\.com/crminmovilla` | alta | https://www.inmobiliariamir.es/?idio=1 |
| inmovilla | ruta detalle (nativo) | `/ficha/[a-z0-9-]+/[a-z0-9-]+/[a-z0-9-]*/?\d{3,6}/\d{7,9}/(es|en|de|ca)/` | alta | https://www.incasaproperties.com/es/ |
| inmovilla | hostname imagen | `fotos\d{1,3}\.apinmo\.com/\d+/\d{7,9}/` | alta | https://fincasfiol.com/propiedad/piso-en-venta-en-paseo-mallorca-27575741/ |
| inmovilla | ruta plugin WP | `/wp-content/plugins/pluginInmovilla/` | alta | https://fincasfiol.com/propiedad/piso-en-venta-en-paseo-mallorca-27575741/ |
| inmovilla | ruta uploads WP | `/wp-content/uploads/[a-z-]*inmovilla[a-z-]*/\d{7,9}/` | alta | https://lighthouse-living.com/ |
| inmovilla | texto | `inmovilla-basico` | media | https://fincasfiol.com/propiedad/piso-en-venta-en-paseo-mallorca-27575741/ |
| inmovilla | rutas PHP | `(seccion|alertas)\.php` | baja | https://www.inmobiliariamir.es/?idio=1 |
| inmovilla | cookie | `Cookieguard` | baja | https://www.inmobiliariamir.es/?idio=1 ; https://www.incasaproperties.com/es/ |
| inmovilla | ruta detalle (WP) | `/propiedad/[a-z0-9-]+-\d{8}/` | media | https://fincasfiol.com/propiedad/piso-en-venta-en-paseo-mallorca-27575741/ |
| witei | hostname | `witei-media\.s3\.amazonaws\.com` | alta | https://www.inmosoller.com/ |
| witei | hostname | `d2ctzk1imdlpfx\.cloudfront\.net` | media | https://www.inmosoller.com/ |
| witei | ruta detalle | `/house/\d{6,8}/` | media | https://www.inmosoller.com/ |
| witei | ruta assets | `/(web_logos|web_carousels|blog_articles)/` | media | https://www.inmosoller.com/ |
| witei | hostname | `(api|app|www)\.witei\.com` | alta | https://faq.witei.com/en/articles/231207-how-do-i-connect-an-external-website-with-witei (no verificado en HTML de agencia) |
| mobilia | hostname imagen | `media\.mobiliagestion\.es/Portals/[^/]+/Images/` | alta | https://www.inmovyp.es/ |
| mobilia | texto pie | `Creado con Mobilia` | alta | https://www.inmovyp.es/ |
| mobilia | enlace | `https?://(www\.)?mobiliagestion\.es` | alta | https://www.inmovyp.es/ |
| mobilia | ruta detalle | `/(es|en|de|ca)/[a-z0-9_]+(/[a-z0-9_]+)?/[a-z_]+/ref-\d+` | media | https://www.inmovyp.es/ |
| mobilia | API | `api\.mobiliagestion\.es/api/v1` | alta | https://claudskills.com/skills/mobilia-api/SKILL.md |
| inmopc | hostname back-office | `crm\d+\.inmovirtual\.es/GenWeb` | alta | https://www.nensiola.com/ |
| inmopc | texto pie | `Crm Web Inmobiliario INMOPC` | alta | https://www.nensiola.com/ |
| inmopc | enlace | `https?://(www\.)?inmopc\.com` | alta | https://www.nensiola.com/ |
| inmopc | ruta listado | `/venta-comprar/[a-z-]+/islas-baleares/[a-z-]+/[a-z-]+/venta-comprar-[a-z-]+\.html` | media | https://www.nensiola.com/ |
| inmopc | ruta | `/mapa-web\.html` | baja | https://www.nensiola.com/ |
| infocasa | ruta assets | `/core/components/infocasa/` | alta | https://www.prestigevillas.com/es/ ; https://www.mpcweb.net/es/ ; https://www.lucie-hauri.com/es/ |
| infocasa | texto pie | `(Software inmobiliario por|Real Estate Software by) InfoCasa CRM` | alta | https://www.prestigevillas.com/es/ |
| infocasa | enlace | `infocasa\.com\?o=` | alta | https://www.prestigevillas.com/es/ |
| infocasa | ruta detalle | `/(es|en|de)/property/id/\d{5,7}-[a-z0-9-]+` | alta | https://www.mpcweb.net/es/ |
| infocasa | ruta | `/(es|en|de)/holiday-rental/\d+-` | media | https://www.prestigevillas.com/es/ |
| infocasa | alt text | `MODX Revolution` | baja | https://www.mpcweb.net/es/ |
| infocasa | enlace diseñador | `rex4media\.com` | media | https://www.prestigevillas.com/es/ |
| immoled | meta generator | `<meta\s+name=["']generator["']\s+content=["']ImmoLED` | alta | https://www.futurinca.com/ |
| immoled | enlace | `led-media\.net` | alta | https://www.futurinca.com/ |
| immoled | rutas imagen | `/(fotografias|miniaturas|galeries/imatges)/` | media | https://www.futurinca.com/ |
| immoled | ruta | `/buscador\?poblacio=` | media | https://www.futurinca.com/ |
| solutra | hostname API | `[a-z0-9-]+-crm\.solutra\.ai/api/public/v1/listings/` | alta | https://casalmallorqui.com/es/comprar-casa-manacor |
| solutra | enlace | `https?://(www\.)?solutra\.ai` | alta | https://casalmallorqui.com/es/comprar-casa-manacor |
| solutra | hostname | `[a-z]+\.supabase\.co` | baja | https://casalmallorqui.com/es/comprar-casa-manacor |
| typo3-realestate | ruta ext | `/typo3conf/ext/nw_realestatemanager/` | alta | https://www.am-inmobiliaria.com/es/index.html |
| typo3-realestate | feed | `/rss-[a-z-]+\?type=9819` | alta | https://www.am-inmobiliaria.com/es/index.html |
| typo3-realestate | meta generator | `content=["']TYPO3 CMS` | media | https://www.am-inmobiliaria.com/es/index.html |
| wordpress-generic | ruta | `/wp-content/(themes|plugins|uploads)/` | media | https://www.inmobiliariacoll.com/ ; https://gestpropiedad.com/ |
| wordpress-generic | meta generator | `content=["'](WordPress|Elementor|Powered by (WPBakery|Slider Revolution))` | media | https://www.inmobiliariacoll.com/ ; https://remax-calm.es/ ; https://fincasfiol.com/propiedad/piso-en-venta-en-paseo-mallorca-27575741/ |
| wordpress-generic | ruta REST | `/wp-json/wp/v2/` | media | (visto indexado en privatepropertymallorca.com en resultados de búsqueda) |
| wordpress-realestate-theme | rutas taxonomía | `/(property|status|label|city)/[a-z0-9-]+/` | baja | https://www.inmobiliariacoll.com/ |
| wordpress-houzez | nombre asset | `houzez` | baja | https://www.mallorcasol.com/inmobiliaria-inca |
| remax-es | hostname | `(propiedades\.[a-z0-9-]+\.(es|com)|[a-z0-9-]+\.remax\.es)` + ruta `/es/venta-[a-z_]+/en-baleares` | media | https://remax-calm.es/ ; https://www.mallorca.com/es/directorio/empresa/remax-del-mar-centro-de-negocios-inmobiliarios-marratxi |
| engel-voelkers | ruta detalle | `/inmueble-mallorca/[a-z0-9-]+-W-[A-Z0-9]{6}` | alta | https://ev-mallorca.com/es/ubicaciones-de-oficinas/noreste/arta |
| engel-voelkers | hostname | `cdn\.ev-mallorca\.com/images/properties/` | alta | https://ev-mallorca.com/es/ubicaciones-de-oficinas/noreste/arta |
| dahler | meta generator | `content=["']Drupal \d+` + `dahlercompany\.com` | alta | https://www.dahlercompany.com/es/mallorca/agente-inmobiliario/santanyi |
| custom-rierataylor | hostname | `rierataylor\.s3\.gra\.io\.cloud\.ovh\.net` ; ruta `/rt\d{4,6}/` | alta | https://www.rierataylor.com/ |
| custom-mundopisos | meta author | `content=["']GRDAR` ; ruta `/propiedades-inmobiliarias/[a-z-]+-[a-z]{1,3}\d{2,4}$` | alta | https://www.mundopisos.com/es/lp/casas-y-pisos-con-piscina-mallorca |
| custom-riusech | ruta | `/propiedades-en-venta/[a-z-]+/\d{1,5}$` ; `\?v=\d+\.\d+\.\d+` | media | https://www.riusech.com/es |
| custom-teamhost | enlace | `teamhost\.es` ; ruta `/(venta|alquiler)/[a-z]+/[a-z]+/[a-z]+/[a-z]*\d+/\d+` | media | https://angelashome.es/es/ |
| custom-corsoft | enlace | `corsoft\.es` ; ruta `/propiedades/[a-z0-9-]+/` + `/static/img/` | media | https://artaimmobilien.com/propiedades/ |
| resales-online | hostname feed | `xmlout\.resales-online\.com/live/Resales/Export/CreateXMLFeed` | alta | https://intercom.help/ReSales-Online/en/articles/5194881-we-are-a-property-portal-can-we-have-a-feed |
| resales-online | hostname API | `webapi\.resales-online\.com` | media | (no verificado) |
| kyero-feed | contenido XML | `<kyero>\s*<feed_version>3` | alta | https://help.kyero.com/article/354-xml-import-specification |
| kyero-feed | contenido XML | `<property>\s*<id>\d+</id>\s*<date>\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}</date>` | alta | https://help.kyero.com/article/354-xml-import-specification |
| spainhouses-xcp | contenido XML | `<carteraPropiedades[^>]*>\s*<fecha>` | alta | https://www.spainhouses.net/es/inmobiliarias/especificaciones-publicacion.html |
| idealista-json | contenido JSON | `"customerCode"\s*:` + `"propertyCode"\s*:` | alta | https://gist.github.com/jjmontalban/2ed1fe14fa88a7f2b03671df53ae643d |
| buscovivienda-shared | hostname | `buscovivienda\.es/[a-z0-9-]+/` | baja | https://www.mallorca.com/en/directory/business/inmobiliaria-servinord-sa-pobla (no verificado) |
| sooprema | hostname/texto | `sooprema` | baja | (no verificado; sin agencia observada) |
| inmoweb | hostname/texto | `inmoweb\.(es|com)` | baja | (no verificado; sin agencia observada) |
| ego-real-estate | hostname/texto | `(egorealestate|janela)\.(com|pt)` | baja | (no verificado; sin agencia observada) |
| inmogesco | hostname/texto | `inmogesco` | baja | (no verificado) |
| inmofactory | hostname/texto | `inmofactory` | baja | (no verificado) |
| inmobalia | hostname/texto | `inmobalia` | baja | https://www.wp-immomakler.de/en/docs-artikel/kyero-idealista-inmobalia-immobilien-import-export-wordpress-plugin/ (no verificado en webs de agencia) |

## Notas para el detector

- **Normalizar el host** (quitar `www.`) y buscar también en `href`, `src`, `srcset`, `content` de meta OG y comentarios HTML.
- **Inmovilla nativo vs. plugin WP:** ambos cuentan como `inmovilla`, pero el parser de ficha difiere; guardar sub-tipo (`inmovilla-native` si hay `/ficha/.../{id}/es/`, `inmovilla-wp` si hay `wp-content` + `apinmo`).
- **Extracción de ID:** Inmovilla `\d{8}` del final de `/ficha/.../{id}/{lang}/` o `-{id}/` en WP; Witei `/house/(\d+)/`; Mobilia `ref-(\d+)`; InfoCasa `/property/id/(\d+)-`; E&V `W-[A-Z0-9]{6}`; Riera & Taylor `/rt(\d+)/`; Mundo Pisos sufijo `-([a-z]{1,3}\d{2,4})$`.
- **Fechas:** solo Mobilia (entero YYYYMMDD en tarjeta), GestPropiedad (texto relativo "hace N horas"), Inmovilla-WP (`article:published_time` en meta) y AM Inmobiliaria (RSS) exponen algo parecido a fecha. Para el resto, la "fecha de alta" debe inferirse como primera vez que el ID fue visto por HomeRadar.
- **Antibot observado:** Cloudflare en Witei (`/cdn-cgi/`); `robots.txt` bloqueó `pollensaestates.es`; `kensington-international.com` y `yes-mallorca-inmuebles.es` devolvieron error de cliente a WebFetch. Respetar `robots.txt` y limitar a 1 req/s por host.

## Añadidos 2026-11-14 (segunda pasada)

| CRM | Tipo de señal | Patrón (regex, case-insensitive) | Confianza | Evidencia |
|---|---|---|---|---|
| inmovilla | feed RSS (nativo) | `href=["']https?://[^"']+/rss["']` junto a `/img/social/rss2.png` | media | https://www.immobono.com/ ; https://www.sociesifills.es/ |
| inmovilla | ruta alertas | `/alertas(\.php|/)` | baja | https://www.a8inmobiliaria.com/ |
| inmovilla | rutas sección | `/(venta|alquiler)/(es|en|de)/` + `/promociones\.php` | baja | https://www.mallorhouse.com/es |
| inmovilla | ruta detalle (WP, 2º patrón) | `/inmueble/(venta|alquiler)/[a-z-]+/illes-balears/[a-z-]+/[a-z]{1,2}-?\d{2,5}/` | media | https://www.cocoinmobiliaria.com/ (con apinmo.com); mismo patrón en gestpropiedad.com |
| inmovilla | ID en slug (front propio) | `/propiedades/[a-z0-9-]+-(2\d{7}|3\d{7})$` (ID global Inmovilla 8 dígitos; el mismo ID aparece en varias agencias por MLS) | media | https://www.ameliainmobiliaria.com/ (30789667 = Mallorca Gestión) |
| inmovilla | imagen icono | `/img/(mapapropiedades/iconoMapa\.(svg|png)|header/bandera_\d{2}\.png|ficha/1/360\.svg)` | media | https://www.a8inmobiliaria.com/ ; https://www.mallorcagestion.com/es |
| infocasa | ruta tema | `/core/components/infocasa/files/themes/[a-z0-9]+/` | alta | https://www.solmallorca.com/ ; https://www.pollentiaproperties.com/real-estate-mallorca |
| infocasa | tarjeta | ID 6 dígitos + ref `[A-Z]\d{4}` en la misma tarjeta (`978240 C2445`) | baja | https://www.solmallorca.com/ |
| onoffice | hostname | `(image|crm|res)\.onoffice\.de/` | alta | https://www.sollerstudio.com/immobilien.xhtml?language=ESP ; https://www.immallo.com/immobilien.xhtml?id%5Bobj0%5D=71&language=ESP |
| onoffice | ruta detalle | `/immobilien(details)?\.xhtml\?id(%5B|\[)obj0(%5D|\])=\d+` | alta | idem |
| onoffice | ruta assets | `/smart20/(Objekte|Dateien)/[A-Za-z]+/` ; `/sitemap\.xhtml` | media | idem |
| resales-online | crédito | `realtysoft\.eu` ; texto `Built by RealtySoft` | alta | https://az-inmobiliaria.com/ |
| resales-online | ruta detalle (WP) | `/propiedad/\?ref_no=R\d{6,8}-[A-Z]` ; `/busqueda-de-propiedades/\?.*listing_type=` | alta | https://az-inmobiliaria.com/ |
| verkia | crédito | `verkia\.com` ; texto `(Desarrollado|Powered) by Verkia` | alta | https://living-palma.com/es/ |
| verkia | ruta | `/templates/verkia/` + Joomla `/components/com_k2/` ; `/sitemap-(es|en)` ; `/es/propiedades\.html` | alta | https://living-palma.com/es/ |
| mediaelx | crédito | `mediaelx\.net` ; texto `Design & CRM: Mediaelx` | alta | https://vginmobiliaria.com/en/contact/ |
| mediaelx | ruta | `/media/images/website/` + `/(en|es|de)/(properties|advanced-search|sitemap)/` | media | https://vginmobiliaria.com/en/contact/ |
| pisoscom-web | ruta detalle | `/detail/(for_sale|for_rent)/[a-z_]+/islas_baleares_illes_balears/[a-z_]+/\d{10,12}\.\d{6}/` | alta | https://www.ibhabitat.es/our_properties/for_sale/ |
| pisoscom-web | ruta | `/our_properties/(for_sale|for_rent)/` ; `/MapaWeb` ; `/imgs/w\d{3,4}/[a-f0-9]{32}\.` ; texto cookies `pisos\.com` | media | https://www.ibhabitat.es/our_properties/for_sale/ |
| joomla-osproperty | ruta | `/images/osproperty/properties/` ; `/media/yootheme/` ; detalle `/property-type/[a-z-]+/\d{5,7}-[a-z0-9-]+\.html` | alta | https://immobilien-portocolom.com/en |
| joomla-generic | meta generator | `content=["']Joomla!` | media | https://immobilien-portocolom.com/en |
| wordpress-houzez | ruta tema | `/wp-content/themes/houzez/` ; listado `/resultados/\?status(%5B%5D|\[\])=` | alta | https://immopascual.com/es/contacto/ |
| wordpress-estatik | crédito | `estatik\.net` ; texto `Powered by Estatik` ; detalle `/property/[a-z0-9-]+/` | alta | https://mtinmobiliariamanacor.es/ |
| wordpress-realestate-theme | ruta detalle | `/estate-propiedad-propri[eé]t[eé]/[a-z0-9-]+-\d{5}-(es|en)/` (CPT `estate`) + tarjetas `\d+ (weeks|months|years) ago` | media | https://fincascalamajor.com/en |
| wordpress-jetengine | ruta | `/wp-content/uploads/jet-form-builder/` + detalle `/properties/[a-z0-9-]+/` | baja | https://inmobport.com/single-user/portinmobiliaria/ |
| sitebuilder-tucomunidad | meta generator | `content=["']Tucomunidad\.cloud` ; `/gallery_gen/[a-f0-9]{32}_` | alta | https://www.andreufelipe.com/Sobre-nosotros/ |
| custom-artifices | meta author | `content=["']https?://www\.artifices\.net` ; detalle `/(es|en|de)-ficha-(venta|alquiler)\.php\?Id=\d+` | alta | https://www.inmobiliariaalcudia.com/ |
| custom-melvyx | crédito | `melvyx\.com` ; listado `/marketplace\?accion=\d` | media | https://palmerinmobiliaria.com/ |
| custom-palmacasa-saas | ruta | `/files/[a-z0-9]+/(logo|fotoseditor)/` ; listado `/ads/(venta|alquiler)/[a-z-]+/[a-z-]+` | baja | https://www.palmacasa.es/ (plataforma no identificada) |
| custom-propiedadespollensa | ruta | `/front/public/img/` ; detalle `/(en|es)/[a-z-]+-for-sale-in-[a-z-]+/[a-z0-9-]+/` | media | https://propiedadespollensa.com/ |
| inmopc (variante clásica) | enlace/ruta | `inmopc\.es` ; `/recursos/nlistadom1\.php\?.*cbo_operacion=` | alta | https://www.cfemenia.es/venta-pisos-casas-mallorca.html |

### Notas de la segunda pasada

- **Allowlist de WebFetch:** solo se pudieron abrir URLs aparecidas literalmente en resultados de `WebSearch` (modo `extended` con el dominio entre comillas; `OR` de 2-4 dominios por consulta funciona). Pedir la home una vez surgida otra URL del mismo host **no** vale: hay que pedir la URL exacta del resultado.
- **Bloqueos nuevos:** `royalhaus.net` y `sun-villas.com` por robots.txt; `kensington-international.com` y `yes-mallorca-inmuebles.es` devuelven client_error; `toniruiz.com` fuerza https→http (WebFetch no sigue). `inmobiliariacampos.com` sirve solo `<head>` (SPA) → necesita headless.
- **Inmovilla como MLS:** el ID 30789667 aparece en mallorcagestion.com y ameliainmobiliaria.com; para deduplicar usar el ID de 8 dígitos como clave global, no `host+id`.
- **Fechas:** Fincas Cala Major (tema WP) muestra tiempo relativo en tarjetas; Inmovilla nativo expone `/rss` (probar si trae `pubDate`); Living Palma (Verkia/Joomla) expone `?format=feed` en listados K2.
