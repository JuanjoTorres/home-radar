# Coste de reforma en Mallorca: investigación para el estimador de HomeRadar (Fase 3)

- **Fecha de la investigación:** 2026-11-15. Todas las URL se consultaron en esa fecha.
- **Ámbito:** primera vivienda (piso o casa pequeña) en Mallorca, sobre todo Palma, en gama media.
- **Moneda:** EUR. Las cifras de la propuesta YAML se dan **con IVA incluido**. Casi ninguna fuente dice si sus precios llevan IVA; ver el apartado 0.3.
- **Regla de trabajo:** cada cifra lleva su fuente y su fecha. Lo que no se pudo comprobar aparece como **NO VERIFICADO**. Las cifras marcadas como *derivado* son cálculos propios a partir de cifras con fuente, y se indica la operación.

---

## 0. Resumen y criterios

### 0.1 Tabla resumen por nivel (Mallorca, €/m² construido, IVA incluido)

| Nivel | Qué incluye | Mín | Probable | Máx | Base |
|---|---|---|---|---|---|
| ReadyToMoveIn | Nada o detalles | 0 | 0 | 0 | Por definición |
| Light | Pintura, suelos, pequeños arreglos | 120 | 220 | 400 | *Derivado*: nacional 100/180/300 × 1,20 (ver 1.2) |
| Medium | Cocina, baños e instalaciones parciales | 450 | 650 | 950 | *Derivado*: nacional 375/500/700 × 1,20, con el máximo ajustado al rango de Palma |
| Full | Integral: instalaciones, carpintería, distribución | 800 | 1.100 | 1.600 | *Derivado*: nacional 2026 (700–1.300) × 1,20, comparado con Palma (1.200–1.800) |

**Factor regional propuesto: 1,20**, con un rango defendible de 1,15 a 1,25. Ver 1.3.

### 0.2 Por qué hay tanta dispersión

En 2026 las fuentes nacionales no coinciden en el precio de una reforma integral:

- **Precios bajos:** Habitissimo da 450–700 €/m² y Cronoshare da 400–700 €/m² en calidad media.
- **Precios altos:** idealista da 850–1.000 €/m² en calidad media, y El Español da de 850 a más de 1.200 €/m².

La explicación más probable es que las plataformas de presupuestos (Habitissimo, Cronoshare) promedian solicitudes de usuarios, que suelen ser reformas más pequeñas o con menos alcance. Los artículos con arquitectos (El Español, idealista) describen integrales completas con calidades medias. Para un comprador conviene **no quedarse corto**, así que el valor "probable" de Full se apoya en el grupo alto. Las cifras de agencias de Mallorca (1.500–2.500 €/m²) se usan solo como techo, porque se centran en villas y gama alta y no citan fuentes.

### 0.3 IVA

- **Tipo reducido del 10 %:** se aplica a la reforma de una vivienda particular con más de 2 años, si los materiales que aporta el contratista no superan el 40 % de la base imponible. Si se supera ese 40 %, se aplica el 21 % ([cuantomecuesta.com, abr-2026](https://cuantomecuesta.com/es/reformar-piso/); [hacecuentas.com, rev. 28-07-2026](https://hacecuentas.com/es/calculadora-coste-reforma-piso-cocina-bano-espana-m2); [Cronoshare, 04-01-2026](https://www.cronoshare.com/cuanto-cuesta/reforma-integral)).
- **Honorarios técnicos:** los encargados por separado (arquitecto, aparejador) llevan siempre el 21 % ([mallorca.com, act. 28-09-2026](https://www.mallorca.com/en/advice/property/building-renovating/full-renovation-mallorca-costs)).
- **Qué dicen las fuentes:** solo las calculadoras de Cronoshare (Palma, 08-01-2026, y electricidad, 03-01-2026) dicen expresamente "IVA incluido". Habitissimo, idealista, El Español, Trustlocal y la agencia Yes! Mallorca no lo indican.
- **Supuesto adoptado:** se tratan como precio final al consumidor, IVA incluido. **NO VERIFICADO** para cada fuente. Si alguna fuera sin IVA, el error sería de un 10 % a la baja, que cubre el colchón de imprevistos (ver 2.3).

---

## 1. Coste por m² por nivel

### 1.1 Fuentes nacionales (España, 2026)

| Fuente (fecha) | Nivel | €/m² | Notas |
|---|---|---|---|
| [Habitissimo – reforma integral piso](https://www.habitissimo.es/presupuestos/reforma-integral-piso) (act. 16-09-2026) | Integral | 450–700 (media 550) | En otro apartado da 400–650. IVA no indicado |
| [Habitissimo – reformar piso](https://www.habitissimo.es/presupuestos/reformar-piso) (act. 16-09-2026) | Parcial | ~200 | 90 m² parcial: 15.000–25.000 € con muebles |
| [Cronoshare – reforma integral](https://www.cronoshare.com/cuanto-cuesta/reforma-integral) (04-01-2026) | Integral básica / media / alta | desde ~300 / 400–700 / 1.000–1.200 | 100 m²: 35.000 / 55.000 / 95.000 € |
| [idealista – reforma integral (ca)](https://www.idealista.com/ca/news/node/905678) (08-09-2026) | Integral básica / media / alta | 700–850 / 850–1.000 / 1.000–1.300 | Incluye instalaciones, cocina, baño, ventanas, puertas, suelos y distribución |
| [El Español – arquitecto José Ramón García](https://www.elespanol.com/sociedad/20260730/jose-ramon-garcia-arquitecto-reformar-casa-tamano-precio-eur-tarda-meses/1003744336742_0.html) (30-07-2026) | Integral, 80–90 m² | 850 a más de 1.200 | Antes de la pandemia: 500–700. Subidas del 3–4,5 % anual desde 2025. Ejemplo: 73 m² por ~88.000 € (≈1.205 €/m², *derivado*) |
| [cuantomecuesta.com](https://cuantomecuesta.com/es/reformar-piso/) (abr-2026) | Económica / media / premium | 300 / 700 (600–900) / 1.500 | Integral de calidad media para 80 m²: 70.000–100.000 € (≈875–1.250 €/m², *derivado*). Pintura y suelos solo: 8.000–15.000 € para 80 m² (≈100–190 €/m², *derivado*). Media (cocina, baño, suelos, pintura): 30.000–50.000 € para 80 m² (≈375–625 €/m², *derivado*). Costes +30 % desde 2022 |

### 1.2 Fuentes de Mallorca y Palma

| Fuente (fecha) | Dato | Notas |
|---|---|---|
| [Cronoshare – reforma integral Palma](https://www.cronoshare.com/cuanto-cuesta/reforma-integral/palma-de-mallorca) (08-01-2026) | Media local 24.129–40.779 €. 100 m²: 40.000–48.000 € (≈400–480 €/m²). 120 m²: 49.000–66.000 €. 150 m²: 55.000–102.000 € | Calculadora "IVA incluido". La tabla por tamaños no es coherente (90 m² sale más barato que 100 m²). Precios bajos, parecidos a los de Habitissimo |
| [Habitissimo – reforma integral piso](https://www.habitissimo.es/presupuestos/reforma-integral-piso) (16-09-2026) | Palma: 24.000–63.000 € por reforma. Madrid: 28.000–64.000 €. Sevilla: 8.000–30.000 € | Sitúa Baleares "entre las comunidades más caras" |
| [Yes! Mallorca Property – costes 2026](https://yes-mallorca-property.com/blog/info/how-much-does-renovation-cost-in-mallorca-in-2026/) (2026, sin fecha exacta) | Cosmética 400–900 €/m². Parcial 900–1.500 €/m². Integral 1.500–2.500 €/m². **Piso en Palma, integral: 1.200–1.800 €/m²**, con un +15 % en el casco antiguo | Agencia sin fuentes citadas, enfocada en villas y gama alta. Se usa solo como techo |
| [Balearic Properties – coste de construir](https://www.balearic-properties.com/en/cost-to-build-a-new-home-mallorca-ibiza) (jul-2026, rev. 19-09-2026) | Obra nueva básica 1.200–1.900 €/m², estándar 1.600–2.200 €/m² | Es obra nueva, no reforma. Sirve de techo absoluto: una integral no debería superar la obra nueva estándar |
| [mallorca.com – reforma integral](https://www.mallorca.com/es/consejos/inmobiliaria/construir-reformar/reforma-integral-mallorca-costes) (act. 28-09-2026) | Sin cifras: "no existe un precio oficial de construcción por metro cuadrado" | Solo trata IVA e ICIO |
| [Imperial Properties – guía 2026](https://www.imperial-properties.com/es/blog/costes-reforma-mallorca-2026-guia-completa) (pub. 04-06-2026, mod. 13-11-2026) | **NO VERIFICADO**: el cuerpo del artículo no se pudo leer, solo los metadatos | Conviene revisarlo a mano |

### 1.3 Sobrecoste de Mallorca frente a la media española

| Evidencia | Valor | Fuente (fecha) |
|---|---|---|
| Reformas en Mallorca frente a la península | +15–20 %. Mano de obra +15–20 %; ~90 % de los materiales llegan por mar, +15–20 % | [Yes! Mallorca](https://yes-mallorca-property.com/blog/info/how-much-does-renovation-cost-in-mallorca-in-2026/) (2026, sin fuentes) |
| Construir en Menorca frente a la península | Hasta +25 %, por menos proveedores y menos competencia. Cemento más caro: solo un par de fabricantes en la isla. Coste por m² de vivienda unifamiliar medio-alta: ~2.000 € en 2021 y más de 4.000 € en 2025 | [Es Diari / menorca.info](https://www.menorca.info/menorca/local/2025/11/19/2514793/coste-construir-vivienda-menorca-duplica-cuatro-anos-por-precio-materiales.html) (19-11-2025), declaraciones del presidente del Colegio de Arquitectos en Menorca |
| Falta de mano de obra en Baleares | Faltan ~6.000 trabajadores (61.000 ocupados). Menores de 30 años: 10,3 %. Mayores de 55: 21,9 %. Esperas de hasta un año para empezar obras. Insularidad y vivienda cara dificultan atraer trabajadores | [Periódico de Ibiza](https://www.periodicodeibiza.es/noticias/baleares/2025/11/02/2503761/constructores-denuncian-falta-mano-obra-sector.html) (02-11-2025) |
| Causas estructurales | Flete marítimo, temporada de obra corta por el turismo, escasez de mano de obra en verano, exigencias de consumo casi nulo. Índice de coste de construcción del Banco de España: 124,1 en abr-2026, +3,5 % interanual | [Balearic Properties](https://www.balearic-properties.com/en/cost-to-build-a-new-home-mallorca-ibiza) (jul/sep-2026) |
| Recargos por ciudad | Madrid +25–30 % y Barcelona +20–25 % en mano de obra; Baleares no aparece | [cuantomecuesta.com](https://cuantomecuesta.com/es/reformar-piso/) (abr-2026) |

**Conclusión:** `regionalFactor = 1.20`, en un rango de 1,15 a 1,25.

- **Verificación:** **NO VERIFICADO** con una fuente oficial (CYPE, COAIB o COAATMCA). Las dos cifras con nombre propio son de 15–20 % (agencia) y de hasta 25 % (arquitecto, sobre Menorca).
- **Cuándo aplicarlo:** solo a fuentes nacionales. No debe aplicarse a cifras que ya son de Palma.

### 1.4 Fuentes oficiales que no se pudieron verificar

- **CYPE Generador de precios (zona Baleares):** la web ([generadordeprecios.info](https://generadordeprecios.info/rehabilitacion/Instalaciones/Gas/Instalacion_interior.html)) existe, pero el precio por zona geográfica requiere seleccionarla de forma interactiva. **NO VERIFICADO.**
- **Módulos del COAATMCA o del COAIB (mòduls de cost):** no se encontraron publicados en abierto. El Informe COAIB 2024 (11-03-2025) solo da estadísticas de visados: las reformas son el 66 % de las obras visadas en 2024 ([PDF](https://mallorcadiario.cibeles.net/adjuntos/618154/INFORME-COAIB-BALEARES-2024.pdf)). **NO VERIFICADO.**
- **Imperial Properties:** el contenido no se pudo leer (ver 1.2). **NO VERIFICADO.**

### 1.5 Cómo se derivan los niveles

- **Light** (pintura, suelos y arreglos):
  - Base nacional: pintura y suelos para 80 m² entre 8.000 y 15.000 €, es decir, 100–190 €/m² (cuantomecuesta, abr-2026). La calidad "económica" llega a 300 €/m².
  - Base nacional resultante: 100 / 180 / 300.
  - × 1,2 = 120 / 216 / 360. Redondeado: **120 / 220 / 400**. El máximo de 400 coincide con el mínimo de "cosmética" de Yes! Mallorca.
- **Medium:**
  - Base nacional: Habitissimo parcial ~200 €/m²; cuantomecuesta media (cocina, baño, suelos y pintura) 375–625 €/m², y su calculadora 600–900 €/m².
  - Base nacional resultante: 375 / 500 / 700.
  - × 1,2 = 450 / 600 / 840. Se sube a **450 / 650 / 950** porque Medium incluye instalaciones parciales y el mínimo de "parcial" de la agencia de Mallorca es 900.
- **Full:**
  - Base nacional 2026: idealista 700–1.300 (media 850–1.000); El Español 850–1.200 o más; cuantomecuesta 875–1.250.
  - × 1,2 ≈ 840–1.560, con un probable de ~1.100 (925 × 1,2).
  - Contraste con Palma: piso integral 1.200–1.800 (agencia). Las plataformas dan 400–700 (Cronoshare Palma), así que el mínimo se fija en 800.
  - Resultado: **800 / 1.100 / 1.600**.

---

## 2. Partidas (Mallorca, IVA incluido)

Las cifras "Mallorca" son la cifra de la fuente × 1,20 cuando la fuente es nacional. Si la fuente ya es de Palma, se dejan como están.

### 2.1 Tabla de partidas

| Código | Partida | Unidad | Mín | Prob. | Máx | Fuente (fecha) y cálculo |
|---|---|---|---|---|---|---|
| kitchen | Cocina completa, 8–12 m², con muebles e instalaciones | Unidad | 7.000 | 11.000 | 18.000 | Palma: 6.500–12.000 € para 8–12 m² ([Cronoshare Palma](https://www.cronoshare.com/cuanto-cuesta/reforma-integral/palma-de-mallorca), 08-01-2026). Nacional: 3.500–12.500 €, media 8.000 €; 12 m² ≈ 10.200 € ([idealista](https://www.idealista.com/news/node/914014), 10-10-2026). Nacional: 8.000–20.000 € para 15 m² ([cuantomecuesta](https://cuantomecuesta.com/es/reformar-piso/), abr-2026). El máximo es 15.000 × 1,2 |
| kitchenLinearMetre | Muebles de cocina por metro lineal | ml | 480 | 1.200 | 3.000 | Nacional: 400–2.500 €/ml, hasta 4.000 en lujo ([idealista](https://www.idealista.com/news/node/914014), 10-10-2026) × 1,2. El probable es *derivado* (punto medio bajo) |
| bathroom | Baño completo, 4–6 m² | Unidad | 4.000 | 6.000 | 9.000 | Palma: 2.100–3.800 € para 3–4 m² y 5.500–7.500 € para 8–10 m² (Cronoshare Palma, 08-01-2026). Palma: 1.500–5.000 € ([Habitissimo reforma baño](https://www.habitissimo.es/presupuestos/reforma-bano), 10-06-2026). Nacional: 3.500–6.000 € ([Qué!](https://www.que.es/2026/09/04/precio-reforma-bano-ducha/), 04-09-2026). Nacional: 4.000–8.000 € (cuantomecuesta, abr-2026) |
| flooring | Suelo porcelánico colocado, con retirada básica | m² de suelo | 35 | 55 | 95 | Nacional: 28–60 €/m², media 45, más 2–3 €/m² por retirar el suelo antiguo ([Habitissimo porcelánico](https://www.habitissimo.es/presupuestos/suelo-porcelanico), 10-06-2026). Cerámica de calidad alta: 45–80 €/m² (Cronoshare nacional, 04-01-2026). Todo × 1,2 |
| painting | Pintura completa de la vivienda | m² de suelo | 14 | 25 | 45 | Nacional: 90 m² por 900–1.300 € (≈10–13 €/m² de suelo); quitar gotelé y pintar 90 m²: 2.500–3.000 € (≈28–33 €/m²) ([Habitissimo pintar](https://www.habitissimo.es/presupuestos/pintar-piso), 16-09-2026). 80 m²: 1.500–3.500 € (cuantomecuesta). Todo × 1,2 |
| electrical | Renovar la instalación eléctrica completa, con CIE (boletín) | m² | 40 | 60 | 85 | Nacional: 70–100 m² por 3.600–6.500 €; casas antiguas cerca de 7.000 €; CIE ~150 € ([El Español](https://www.elespanol.com/mujer/interiorismo/20260719/electricista-reformar-casa-cambiar-instalacion-electrica-costaba-eur-ahora-supera/1003744319172_0.html), 19-07-2026). 90 m²: 2.200–2.500 €; 100 m²: 3.500–4.000 €; CIE 80–300 € ([Cronoshare electricidad](https://www.cronoshare.com/cuanto-cuesta/reformar-instalacion-electrica), 03-01-2026). Rango *derivado*: ≈35–70 €/m² × 1,2 |
| electricalCertificate | Solo el boletín o CIE | Vivienda | 100 | 180 | 360 | Nacional: 70–300 €, media 100–150 € ([OCU](https://www.ocu.org/vivienda-y-energia/gas-luz/consejos/boletin-electrico), 10-07-2025); 80–300 € (Cronoshare, 03-01-2026). Todo × 1,2 |
| plumbing | Renovar la fontanería (tuberías y bajantes interiores) | Vivienda | 2.000 | 3.800 | 6.000 | Palma: ~3.800 € para 100 m² (Cronoshare Palma, 08-01-2026). Nacional: 70 m² ~3.000 €; cambio completo ~5.000 € (Habitissimo, 16-09-2026). Nacional: 1.500–4.000 € (cuantomecuesta). El máximo es 5.000 × 1,2 |
| window | Ventana de aluminio con RPT o PVC, ~120×120, colocada | Unidad | 350 | 550 | 850 | Nacional: aluminio con RPT 200–400 € (hasta 500); PVC 250–450 €; mano de obra 80–200 € por ventana, "aparte" ([Habitissimo ventanas](https://www.habitissimo.es/presupuestos/cambiar-ventanas), 16-09-2026). Rango *derivado*: (200+80)–(500+200) × 1,2. **No está claro si el precio de la ventana incluye la colocación** |
| exteriorCarpentryFull | Toda la carpintería exterior de una vivienda de gama alta | Vivienda | — | — | — | Palma: 12.000–25.000 € (Cronoshare Palma, 08-01-2026). Se cita como referencia de techo; no está en el YAML |
| interiorDoor | Puerta de paso colocada | Unidad | 200 | 350 | 700 | Nacional: 150–600 €, habitual más de 250 € ([El Español](https://www.elespanol.com/mujer/interiorismo/20260725/carpintero-poner-puertas-nuevas-casa-euros-unidad-ahora-pasan/1003744328690_0.html), 25-07-2026); gama económica 250–400 € (cuantomecuesta). Todo × 1,2, redondeado |
| airConditioningSplit | Split 1×1 de ~2.500 frigorías, instalado | Unidad | 1.200 | 1.500 | 1.800 | Nacional: 1.000–1.500 € ([Trustlocal](https://trustlocal.es/coste/instalador-aire-acondicionado-coste/), 03-09-2026) × 1,2 |
| airConditioningDucted | Aire por conductos, vivienda de ~100 m² | Vivienda | 2.400 | 3.600 | 4.800 | Nacional: 2.000–4.000 € (Trustlocal, 03-09-2026) × 1,2. **Parece bajo: probablemente excluye el falso techo.** Revisar |
| dhwHeatPump | Aerotermia solo para ACS, 200 L | Unidad | 2.200 | 2.700 | 4.700 | Nacional: 200 L desde 2.250 €; rango 1.800–3.900 € ([Habitissimo aerotermia](https://habitissimo.es/presupuestos/aerotermia), 10-06-2026). Todo × 1,2 |
| aerothermalHeating | Aerotermia para calefacción y ACS, hasta 120 m², sin emisores | Vivienda | 9.600 | 10.000 | 13.200 | Nacional: desde 8.000 € (7 kW) hasta 11.000 € (14,5 kW); radiadores ~1.000 € por unidad; suelo radiante +80 €/m² (Habitissimo, 10-06-2026). Todo × 1,2. Opcional |
| boilerAndRadiators | Cambio de caldera y radiadores | Vivienda | — | 4.800 | — | Nacional: "hasta 4.000 €" (Habitissimo reformar piso, 16-09-2026) × 1,2. Solo hay un valor |
| asbestosRemoval | Retirada de fibrocemento (uralita) por empresa inscrita en el RERA | m² | 32 | 45 | 65 | Nacional: 27–52 €/m²; mínimo desde ~800 €; transporte 90–100 €/m³ ([Habitissimo amianto](https://www.habitissimo.es/presupuestos/retirar-amianto-uralita), 16-09-2026). Todo × 1,2. En Mallorca, 3.000–7.000 € por actuación (Yes! Mallorca, 2026) |
| risingDamp | Tratamiento de humedad por capilaridad (planta baja) | Vivienda | 1.800 | 3.500 | 7.200 | Nacional: vivienda entera 2.000–6.000 €; rango general 600–7.000 €; inyecciones 100–120 €/m² o ~90 €/ml ([Habitissimo capilaridad](https://www.habitissimo.es/presupuestos/humedades-capilaridad), 15-09-2026). Todo × 1,2. El probable es *derivado* |
| demolitionDebris | Demoliciones y desescombro en una integral | m² | 25 | 35 | 50 | Palma: demolición desde 25 €/m² (Cronoshare Palma, 08-01-2026). Gestión de residuos (Tirme) 30–50 €/m² (Yes! Mallorca, 2026). Nacional: desescombro 8–11 €/m² (Cronoshare, 04-01-2026) |
| structuralReinforcement | Refuerzo estructural (forjados, aluminosis) | m² afectado | 150 | 175 | 200 | Mallorca: 150–200 €/m² (Yes! Mallorca, 2026, sin fuente). Baja fiabilidad |
| roofFacade | Cubierta o fachada (casas) | — | — | — | — | **NO VERIFICADO.** Solo hay un titular: "antes costaba poco más de 2.000 €, ahora supera los 5.000" ([El Español](https://www.elespanol.com/sociedad/20260730/jose-ramon-garcia-arquitecto-reformar-casa-tamano-precio-eur-tarda-meses/1003744336742_0.html), 30-07-2026), sin superficie ni alcance |

### 2.2 Licencias, impuestos y honorarios (Palma)

| Concepto | Valor | Fuente (fecha) |
|---|---|---|
| ICIO en Palma | **4 %** de la base imponible (coste real de ejecución, sin IVA, honorarios ni beneficio). Bonificaciones: 25 % por reforma de eficiencia energética, 95 % por solar de autoconsumo, 90 % por accesibilidad | [Ordenanza fiscal ICIO de Palma, BOIB 168 de 27-12-2022, vigente desde el 01-01-2023](https://seuelectronica.palma.es/portal/PALMA/sede/RecursosWeb/DOCUMENTOS/1/4_151112_2.pdf) |
| Tasa de licencia en Palma | **2,61 %** del coste de la obra (mínimo 18,47 €). Palma descuenta del ICIO el 50 % de la tasa pagada | [mallorca.com – ICIO](https://www.mallorca.com/en/advice/property/building-renovating/building-licence-fee-icio) (act. 08-09-2026). Ejemplo de cálculo; **conviene confirmarlo en la ordenanza de tasas** |
| ICIO en otros municipios | Manacor 2 %, Palma 4 % (el máximo legal). En Marratxí se infiere ~3,2 % | mallorca.com (08-09-2026) |
| Comunicación previa | Para baño, cocina, ventanas del mismo tamaño, climatización, electricidad o fontanería, suelos y **tabiques no estructurales**. No requiere arquitecto. Tasa de ~50–200 € más el ICIO. Plazo de 1–2 semanas | [mallorca.com – licencia o comunicación previa](https://www.mallorca.com/es/consejos/inmobiliaria/construir-reformar/licencia-obra-comunicacion-previa-mallorca) (act. 28-09-2026) |
| Licencia (obra mayor) | Para estructura, ampliación, demolición o fachada. Requiere proyecto básico de arquitecto visado por el COAIB. Tarda "varios meses" | mallorca.com (28-09-2026) |
| Sin licencia | Obra menor de 3.000 € por edificio y año, sin técnico y en el interior (por ejemplo, pintar o cambiar grifos) | mallorca.com (28-09-2026) |
| Permisos de obra mayor (nacional) | 4–6 % del presupuesto sin IVA; ~3.000 € de media para 70 m² | [Habitissimo reformar piso](https://www.habitissimo.es/presupuestos/reformar-piso) (16-09-2026) |
| Honorarios de arquitecto | 4–12 % del PEM (Madrid ~10 %; Barcelona 10–15 %). Proyecto y dirección de obra: 2.000–6.000 €. Coordinación de seguridad: 3–5 % del PEM | [Habitissimo arquitectos](https://www.habitissimo.es/presupuestos/arquitectos) (28-10-2026) |
| Honorarios en Mallorca | Arquitecto y aparejador 8–12 % (obra nueva) | [Balearic Properties](https://www.balearic-properties.com/en/cost-to-build-a-new-home-mallorca-ibiza) (sep-2026) |
| Honorarios en Mallorca (agencia) | Arquitectura y diseño 8–15 %; licencias y tasas 6–10 %; visado COAIB 2.000–5.000 € en obra mayor | Yes! Mallorca (2026, sin fuentes) |
| Honorarios del aparejador por separado | **NO VERIFICADO** | — |

**Supuesto para el estimador:** el PEM es aproximadamente el presupuesto con IVA dividido entre 1,10 (IVA) y entre 1,19 (gastos generales y beneficio industrial), es decir, ~0,76 × presupuesto con IVA.

- **Licencia en Palma:** ICIO + tasa − 50 % de la tasa ≈ (4 % + 1,305 %) × PEM ≈ **4,0 % del presupuesto con IVA**.
- **Comunicación previa:** ICIO 4 % × PEM ≈ **3,0 %** del presupuesto con IVA, más la tasa.
- **Verificación:** el factor 1,19 (13 % de gastos generales y 6 % de beneficio industrial) es una convención de la contratación pública y está **NO VERIFICADO** para obra privada.

### 2.3 Imprevistos

- Hacecuentas (rev. 28-07-2026) usa ~12 % en su ejemplo.
- Yes! Mallorca (2026) recomienda 15–20 %.
- **Propuesta:** 10 / 15 / 20 %.

---

## 3. Antigüedad del edificio y necesidad de reforma

### 3.1 Hitos normativos y técnicos

| Hito | Año | Qué implica | Fuente |
|---|---|---|---|
| Cemento aluminoso (aluminosis) | Uso masivo desde finales de los 50; la EP-77 (1977) prohíbe su uso en viguetas | Forjados de 1955–1977 con viguetas pretensadas. Peor en zonas costeras y en baños, cocinas, cubiertas y forjados sanitarios. ~300.000 viviendas afectadas en España | [Consumer Eroski](https://www.consumer.es/vivienda/aluminosis-en-edificios.html) (20-06-2005, antiguo pero técnico) |
| NBE-CT-79 | 1979–1980 | Primera exigencia mínima de aislamiento en fachadas y cubiertas: antes de 1980, en la práctica no hay aislamiento | [EsadeEcPol, informe sobre rehabilitación](https://www.esade.edu/ecpol/wp-content/uploads/2026/07/Brief-Rehabilitacion-de-Vivienda.pdf) (jul-2026) |
| Amianto | Uso extensivo 1974–1991; prohibiciones en 1984 y 1993; **prohibición total en 2002** (Orden de 7-12-2001) | Antes de 2002, casi cualquier edificio puede contener amianto: bajantes, depósitos, cubiertas ligeras, conductos de humos y calorifugados. No hay obligación de retirarlo, pero al hacer obra hay que recurrir a una empresa inscrita en el RERA | [INSST NTP 1006](https://www.insst.es/es/documentacion/colecciones-tecnicas/ntp-notas-tecnicas-de-prevencion/29-serie-ntp-numeros-996-a-1030-ano-2014/nota-tecnica-de-prevencion-ntp-1006) (2014); [Xataka](https://www.xatakahome.com/hogar/primero-que-hice-antes-comprar-mi-casa-fue-comprobar-que-tipo-eran-sus-tuberias-tienes-este-tipo-puede-salir-muy-caro) (23-04-2025) |
| REBT 2002 | RD 842/2002 | Las instalaciones anteriores suelen tener fusibles, carecer de toma de tierra o de diferencial, y tener pocos circuitos. El boletín "caduca" a los 20 años (la propia OCU se contradice): con más de 20 años se exige revisión si se cambia el contrato o la potencia | [OCU](https://www.ocu.org/vivienda-y-energia/gas-luz/consejos/boletin-electrico) (10-07-2025) |
| Vida útil de la instalación eléctrica | Revisar a partir de 20–30 años | Señales: saltos frecuentes, enchufes que se calientan, sin toma de tierra, fusibles antiguos | [El Español](https://www.elespanol.com/mujer/interiorismo/20260719/electricista-reformar-casa-cambiar-instalacion-electrica-costaba-eur-ahora-supera/1003744319172_0.html) (19-07-2026) |
| Tuberías de plomo | El RD 3/2023 obliga a sustituirlas siempre que haya obras de reparación o reconstrucción. Antes del 02-01-2030 en edificios públicos, comerciales y pisos de alquiler. Límite de 5 µg/L desde el 02-01-2035 | Si hay plomo, la fontanería es obligatoria en cualquier reforma | [Interempresas sobre el RD 3/2023](https://www.interempresas.net/Climatizacion/543147-Nuevas-obligaciones-instalaciones-hidraulicas-para-mejora-calidad-del-agua-consumo.html) (04-03-2024); [BOE-A-2023-628](https://www.boe.es/buscar/doc.php?id=BOE-A-2023-628) |
| Vida útil de las tuberías | Sustitución recomendada a los 20–25 años; plomo y hierro galvanizado ya no se admiten | — | Xataka (23-04-2025) |
| Año en que se dejó de usar plomo o galvanizado | **NO VERIFICADO.** Se cita a menudo "hasta ~1975–1980", pero no se encontró una fuente primaria | — | — |
| Ventanas de aluminio sin RPT | **NO VERIFICADO** como dato con fecha. Es una inferencia: antes del CTE (2006) era la carpintería habitual; el CTE introdujo exigencias reales de eficiencia | — | EsadeEcPol (jul-2026), que solo menciona el CTE de 2006 |
| IEE en Baleares | Obligatorio a los **50 años** del edificio (art. 125 de la LUIB, modificado por la Ley 4/2026) y cada 10 años. Los defectos de la parte 1 deben corregirse en 6 meses | Edificios de 1976 o anteriores (en 2026) tienen IEE obligatorio; si es negativo, sirve para negociar el precio | [mallorca.com – IEE/ITE](https://www.mallorca.com/it/consigli/immobili/costruire-ristrutturare/iee-ite-mallorca-obbligatorio) (09-10-2026) |
| Cédula de habitabilidad | Obligatoria en Baleares para vender o alquilar; validez de 10 años. Para edificios anteriores al 01-03-1987 sin cédula existe la "cédula de carencia" | — | [mallorca.com – cédula](https://www.mallorca.com/en/advice/property/building-renovating/mallorca-habitability-certificate) (28-09-2026); [OCU](https://www.ocu.org/fincas-y-casas/gestion/gestion-patrimonial/analisis/2026/03/cedula-habitabilidad) (18-03-2026) |

### 3.2 Umbrales propuestos

Estos umbrales solo se usan si el anuncio no da señales de estado. Las palabras clave del apartado 4 tienen prioridad sobre el año de construcción.

| Rango del año de construcción | Nivel por defecto | Razonamiento |
|---|---|---|
| ≤ 1979 | **Full** | Sin aislamiento (antes de la NBE-CT-79); época de aluminosis (1955–1977); amianto probable; electricidad anterior a 2002 y con más de 20 años; plomo o galvanizado probables (**NO VERIFICADO**); IEE obligatorio (50 años) |
| 1980–2002 | **Medium** | Amianto posible (prohibido en 2002); instalación eléctrica anterior al REBT 2002 y con más de 20 años de boletín; ventanas de aluminio sin RPT probables (inferencia); cocina y baños de más de 20 años |
| 2003–2008 | **Light** | Ya con REBT 2002 y sin amianto; anterior o en transición al CTE (2006); acabados de unos 20 años |
| ≥ 2009 | **ReadyToMoveIn** | Ya con CTE; instalaciones de menos de 20 años |

Ajustes:

- **Reforma reciente:** si el anuncio dice "reformado" con un año igual o posterior a (año actual − 15), el nivel baja a Light o ReadyToMoveIn. Si el año es anterior, el nivel queda como mínimo en Medium.
- **Edificios de 50 años o más:** se añade la alerta "IEE obligatorio".
- **Plantas bajas o sótanos en Mallorca:** se añade la alerta de humedad por capilaridad. Según una nota de prensa de una empresa del sector (Murprotec, 21-03-2025), Baleares es "una de las zonas de España más afectadas por problemas de humedades", con una humedad relativa media cercana al 80 % ([Periódico de Ibiza](https://www.periodicodeibiza.es/noticias/escaparate/2025/03/21/2350599/murprotec-celebra-anos-espana-solido-crecimiento-presencia-clave-baleares.html)). Es una fuente comercial sin estadísticas.

---

## 4. Diccionarios de palabras clave (ES, CA, EN, DE)

Notas de implementación:

- Normalizar el texto antes de buscar: minúsculas y sin acentos (ü→u, ß→ss, à→a).
- Buscar como frase completa, con límites de palabra.
- Detectar negaciones en una ventana de ~3 tokens antes del término: "no necesita reforma", "sin necesidad de reforma", "keine Renovierung nötig", "no need to renovate".
- Si coinciden varios grupos, gana el más grave (Full > Medium > Light > ReadyToMoveIn). Excepción: si el término de ReadyToMoveIn va acompañado de un año reciente.
- ⚠ marca un término ambiguo.
- **Origen:** las listas se han compilado a partir del vocabulario habitual de los portales inmobiliarios en cada idioma. **NO VERIFICADO** con un corpus de anuncios reales: conviene validarlas con los anuncios de las fases 0–2.

### 4.1 Ruina o integral (Full; opcionalmente con la etiqueta `ruin`)

| ES | CA | EN | DE |
|---|---|---|---|
| ruina | ruïna | ruin | Ruine |
| en ruinas | en ruïnes | in ruins | ruinös |
| para derribar | per enderrocar | for demolition | Abrissobjekt |
| a derribar | a enderrocar | to be demolished | abrissreif |
| a demoler | per demolir | teardown | Abriss |
| a rehabilitar | a rehabilitar | to rehabilitate | Kernsanierung erforderlich |
| para rehabilitar | per rehabilitar | complete rebuild | komplett zu sanieren |
| reforma integral (sin "reformado") ⚠ | reforma integral ⚠ | gut renovation | Totalsanierung |
| para reformar integramente | per reformar integrament | shell | Rohbau ⚠ |
| solar ⚠ (es terreno, no vivienda) | solar ⚠ | building plot ⚠ | Baugrundstück ⚠ |
| cascarón | carcassa | derelict | verfallen |
| sin habitabilidad / sin cédula ⚠ | sense cèdula ⚠ | uninhabitable | unbewohnbar |
| finca para restaurar | finca per restaurar | restoration project | Restaurierungsobjekt |
| con proyecto de reforma ⚠ (hay que pagar la obra, aunque el proyecto ya esté hecho) | amb projecte ⚠ | with renovation project/licence ⚠ | mit Baugenehmigung ⚠ |

### 4.2 A reformar (Full por defecto, para no quedarse corto)

| ES | CA | EN | DE |
|---|---|---|---|
| a reformar | a reformar | to renovate | renovierungsbedürftig |
| para reformar | per reformar | to reform | sanierungsbedürftig |
| necesita reforma | necessita reforma | needs renovation | Renovierungsobjekt |
| necesita reforma completa | necessita reforma completa | in need of renovation | Sanierungsobjekt |
| necesita obras | necessita obres | needs complete renovation | Handwerkerobjekt |
| estado original | estat original | original condition | Originalzustand |
| original de los 60 / 70 / 80 | original dels anys 70 | original 1970s | im Originalzustand der 70er |
| para reformar a su gusto | per reformar al seu gust | renovate to your taste | nach eigenen Wünschen renovieren |
| a reformar totalmente | per reformar totalment | fixer-upper | stark renovierungsbedürftig |
| ideal inversores ⚠ | ideal inversors ⚠ | investment opportunity ⚠ | Investitionsobjekt ⚠ |
| oportunidad para reformar | oportunitat per reformar | renovation opportunity | Renovierungspotenzial |
| con mucho potencial ⚠ | amb molt de potencial ⚠ | lots of potential ⚠ | viel Potenzial ⚠ |
| para restaurar | per restaurar | to restore | zu restaurieren |
| precisa reforma | precisa reforma | requires renovation | Modernisierungsstau |
| reforma pendiente | reforma pendent | renovation needed | Sanierungsbedarf |
| a reformar por completo | — | renovation project | Liebhaberobjekt ⚠ |

### 4.3 Para actualizar (Medium)

| ES | CA | EN | DE |
|---|---|---|---|
| para actualizar | per actualitzar | needs updating | modernisierungsbedürftig |
| a actualizar | a actualitzar | to update | teilrenoviert ⚠ (ya parcialmente renovado) |
| necesita actualización | necessita actualització | in need of modernisation | Modernisierungsbedarf |
| necesita algunas reformas | necessita algunes reformes | needs some work | renovierungsbedürftig in Teilen |
| reformar cocina y baños | reformar cuina i banys | kitchen and bathrooms to update | Küche und Bäder erneuerungsbedürftig |
| para modernizar | per modernitzar | to modernise | zu modernisieren |
| parcialmente reformado ⚠ | parcialment reformat ⚠ | partially renovated ⚠ | teilsaniert ⚠ |
| reformado en 19xx / 200x ⚠ (reforma antigua) | reformat l'any 19xx ⚠ | renovated in 1995 ⚠ | renoviert 1995 ⚠ |
| cocina original | cuina original | original kitchen | Originalküche |
| baños originales | banys originals | original bathrooms | Originalbäder |
| a reformar parcialmente | per reformar parcialment | partial renovation needed | teilweise renovierungsbedürftig |
| para poner al día | per posar al dia | dated | veraltet |
| anticuado | antiquat | outdated | in die Jahre gekommen |
| conservado ⚠ | conservat ⚠ | well kept but dated ⚠ | gepflegt, aber älter ⚠ |
| habitable pero necesita reforma | habitable però necessita reforma | habitable but needs work | bewohnbar, aber renovierungsbedürftig |

### 4.4 Pequeños retoques (Light)

| ES | CA | EN | DE |
|---|---|---|---|
| lavado de cara | rentat de cara | cosmetic updates | Schönheitsreparaturen |
| necesita pintura / pintar | necessita pintura | needs a lick of paint | leicht renovierungsbedürftig |
| pequeñas reformas | petites reformes | minor works | kleinere Renovierungsarbeiten |
| algunos retoques | alguns retocs | some touch-ups | kleine Arbeiten |
| buen estado ⚠ | bon estat ⚠ | good condition ⚠ | guter Zustand ⚠ |
| buen estado de conservación | bon estat de conservació | well maintained | gepflegter Zustand |
| para entrar a vivir ⚠ (se usa también en viviendas anticuadas pero habitables) | per entrar a viure ⚠ | ready to live in ⚠ | sofort bewohnbar ⚠ |
| mejorable | millorable | could do with refreshing | auffrischungsbedürftig |
| a refrescar | per refrescar | to refresh | Auffrischung nötig |
| cosmética | cosmètica | cosmetic | kosmetisch |
| actualizado ⚠ | actualitzat ⚠ | updated ⚠ | modernisiert ⚠ |

### 4.5 Reformado o listo (ReadyToMoveIn)

| ES | CA | EN | DE |
|---|---|---|---|
| reformado (sin año, o con año reciente) ⚠ | reformat ⚠ | renovated ⚠ | renoviert ⚠ |
| recién reformado | acabat de reformar | newly renovated | frisch renoviert |
| totalmente reformado | totalment reformat | fully renovated | komplett renoviert |
| reforma integral reciente | reforma integral recent | completely refurbished | kernsaniert |
| reformado a estrenar | reformat a estrenar | recently refurbished | neu saniert |
| a estrenar | a estrenar | brand new | Erstbezug |
| obra nueva | obra nova | new build | Neubau |
| listo para entrar a vivir | llest per entrar a viure | move-in ready | bezugsfertig |
| llave en mano | clau en mà | turnkey | schlüsselfertig |
| impecable | impecable | immaculate | makellos |
| como nuevo | com nou | as new | neuwertig |
| de diseño / reformado por arquitecto | de disseny | designer renovation | hochwertig saniert |
| reformado con calidades altas | reformat amb qualitats altes | high-spec renovation | luxuriös renoviert |
| sin gastos de reforma | sense despeses de reforma | no work needed | keine Renovierung nötig |
| exquisitamente reformado | exquisidament reformat | beautifully renovated | aufwendig renoviert |
| rehabilitado ⚠ (puede ser antiguo) | rehabilitat ⚠ | restored ⚠ | restauriert ⚠ |

### 4.6 Ambigüedades y cómo resolverlas

1. **"Reformado en AAAA" o "renoviert AAAA":** extraer el año con `(19|20)\d{2}`. Si es igual o posterior al año actual − 15, el nivel es ReadyToMoveIn o Light. Si es anterior, el nivel es Medium.
2. **Reforma de una sola estancia** ("cocina reformada", "baño reformado"): no hace la vivienda ReadyToMoveIn; como mucho, rebaja una partida.
3. **"Para entrar a vivir" o "buen estado":** en España se usan también para pisos habitables pero anticuados. Tratarlos como Light, salvo que vayan con "reformado".
4. **"Solar" o "Baugrundstück":** es terreno, no vivienda. Marcar `ruin` y excluir la estimación por m² construido.
5. **"Con proyecto" o "con licencia":** hay que contar la obra completa (Full), pero los honorarios del proyecto ya están hechos.
6. **"Ideal inversores", "potencial", "Liebhaberobjekt":** solo son señales débiles; requieren otra palabra clave para clasificar.
7. **"Reforma" como posibilidad** ("posibilidad de reforma", "Ausbaupotenzial"): no indica el estado actual.
8. **Negaciones:** "no necesita reforma" o "keine Renovierung nötig" pasan a ReadyToMoveIn.
9. **"Rehabilitado" o "restauriert":** se usa en fincas rústicas; comprobar si hay año.
10. **"Teilrenoviert" o "parcialmente reformado":** es Medium, no Light.

---

## 5. Qué comprobar en la visita, por nivel

### Todas las viviendas
- **Cédula de habitabilidad vigente:** obligatoria en Baleares para vender; caduca a los 10 años. Sin ella, el notario puede no autorizar la escritura.
- **IEE:** si el edificio tiene 50 años o más, pedirlo y ver si es favorable, y preguntar por derramas pendientes.
- **Certificado energético:** pedir la letra y el año.
- **Boletín (CIE):** pedir el año. Si tiene más de 20 años, cambiar la potencia o el titular exigirá revisión (OCU, 10-07-2025).
- **Licencias:** comprobar que las reformas anteriores tenían licencia o comunicación previa, sobre todo si se cerró una terraza o se cambió la distribución.

### ReadyToMoveIn y Light
- Cuadro eléctrico con IGA, diferenciales (30 mA) y varios circuitos, y toma de tierra en los enchufes.
- Probar grifos y cisternas: presión, agua caliente y tiempo hasta que sale caliente.
- Ventanas: comprobar si son de aluminio con o sin RPT y si el vidrio es doble.
- Manchas de humedad o moho en techos, esquinas y detrás de muebles, sobre todo en el baño y en las paredes que dan al norte.
- Climatización existente: número de splits, su antigüedad y el tipo de gas.
- Calentador de agua: tipo (gas butano, termo eléctrico o aerotermia) y año.

### Medium (además de lo anterior)
- **Material de las tuberías:** mirar bajo el fregadero y en el contador. Plomo (gris, blando, con soldaduras abultadas) o hierro galvanizado obligan a renovar la fontanería (RD 3/2023).
- Bajantes y depósitos de fibrocemento (gris, con fibras): posible amianto si el edificio es anterior a 2002 (INSST NTP 1006).
- Estado de la cocina: muebles, encimera y las tomas de agua y gas.
- Baños: bañera o plato de ducha, ventilación, y grietas o juntas ennegrecidas.
- Suelos: si suenan a hueco, el gres está levantado. Si es terrazo hidráulico original, vale la pena recuperarlo.
- Gotelé en paredes y techos: quitarlo encarece la pintura de ~10–13 a ~28–33 €/m² de suelo (Habitissimo, 16-09-2026).

### Full (además de lo anterior)
- **Aluminosis:** en edificios de 1955–1977, revisar los techos de baño y cocina y la cubierta. Señales: fisuras paralelas a las viguetas, óxido y desconchones. Si hay sospecha, pedir un test de aluminosis antes de firmar.
- **Capilaridad** (muy común en plantas bajas de Mallorca): salitre o manchas en la parte baja de los muros (los primeros ~1 m), pintura que se abomba y olor a humedad. Usar un medidor de humedad si es posible.
- **Estructura:** grietas en diagonal en los muros de carga, forjados con flecha visible y puertas que rozan.
- **Cubierta o azotea (casas o último piso):** impermeabilización, tejas rotas, cubierta de uralita y canalones.
- **Fachada:** desprendimientos, armaduras vistas (oxidadas) y estado de los balcones.
- **Instalación eléctrica antigua:** fusibles de porcelana, cables de tela y ausencia de toma de tierra. En ese caso, toda la instalación es nueva.
- **Distribución:** saber si los tabiques que se quieren tirar son de carga. Si se toca la estructura, hace falta licencia de obra mayor con proyecto de arquitecto visado y la obra se alarga "varios meses" (mallorca.com, 28-09-2026). Los tabiques no estructurales van por comunicación previa.
- **Acometida eléctrica:** si se necesita más potencia, la ampliación de red con la distribuidora cuesta desde 2.000 € y añade 2–4 meses (Yes! Mallorca, 2026; **sin verificar**).
- **Edificio protegido o en el casco antiguo de Palma:** más trámites y ~+15 % de coste (Yes! Mallorca, 2026; **sin verificar**).

---

## 6. Propuesta YAML

```yaml
# HomeRadar – Fase 3: tabla de costes de reforma (Mallorca)
# Fecha de investigación: 2026-11-15. EUR, IVA incluido (supuesto; la mayoría de las fuentes no lo indican).
# CONVENIO: todos los valores de levels e items YA ESTÁN AJUSTADOS a Mallorca.
#   regionalFactor solo sirve para convertir NUEVAS fuentes nacionales: NO volver a multiplicar.
renovation:
  regionalFactor: 1.20          # rango 1.15–1.25; ver la sección 1.3. NO VERIFICADO con fuente oficial
  regionalFactorAppliedInValues: true
  currency: EUR
  vatIncluded: true
  researchedOn: 2026-11-15
  levels:
    - { level: ReadyToMoveIn, minPerSquareMetre: 0,   probablePerSquareMetre: 0,    maxPerSquareMetre: 0,    sourceUrl: null }
    - { level: Light,  minPerSquareMetre: 120, probablePerSquareMetre: 220,  maxPerSquareMetre: 400,  sourceUrl: "https://cuantomecuesta.com/es/reformar-piso/" }
    - { level: Medium, minPerSquareMetre: 450, probablePerSquareMetre: 650,  maxPerSquareMetre: 950,  sourceUrl: "https://cuantomecuesta.com/es/reformar-piso/" }
    - { level: Full,   minPerSquareMetre: 800, probablePerSquareMetre: 1100, maxPerSquareMetre: 1600, sourceUrl: "https://www.idealista.com/ca/news/node/905678" }
  items:
    - { code: kitchen,               unit: Unit,        min: 7000, probable: 11000, max: 18000, appliesTo: [Medium, Full], sourceUrl: "https://www.cronoshare.com/cuanto-cuesta/reforma-integral/palma-de-mallorca" }
    - { code: kitchenLinearMetre,    unit: LinearMetre, min: 480,  probable: 1200,  max: 3000,  appliesTo: [],             sourceUrl: "https://www.idealista.com/news/node/914014" }
    - { code: bathroom,              unit: Unit,        min: 4000, probable: 6000,  max: 9000,  appliesTo: [Medium, Full], sourceUrl: "https://www.cronoshare.com/cuanto-cuesta/reforma-integral/palma-de-mallorca" }
    - { code: flooring,              unit: SquareMetre, min: 35,   probable: 55,    max: 95,    appliesTo: [Light, Full],  sourceUrl: "https://www.habitissimo.es/presupuestos/suelo-porcelanico" }
    - { code: painting,              unit: SquareMetre, min: 14,   probable: 25,    max: 45,    appliesTo: [Light, Medium, Full], sourceUrl: "https://www.habitissimo.es/presupuestos/pintar-piso" }
    - { code: electrical,            unit: SquareMetre, min: 40,   probable: 60,    max: 85,    appliesTo: [Full],         sourceUrl: "https://www.elespanol.com/mujer/interiorismo/20260719/electricista-reformar-casa-cambiar-instalacion-electrica-costaba-eur-ahora-supera/1003744319172_0.html" }
    - { code: electricalCertificate, unit: Dwelling,    min: 100,  probable: 180,   max: 360,   appliesTo: [Medium],       sourceUrl: "https://www.ocu.org/vivienda-y-energia/gas-luz/consejos/boletin-electrico" }
    - { code: plumbing,              unit: Dwelling,    min: 2000, probable: 3800,  max: 6000,  appliesTo: [Full],         sourceUrl: "https://www.cronoshare.com/cuanto-cuesta/reforma-integral/palma-de-mallorca" }
    - { code: window,                unit: Unit,        min: 350,  probable: 550,   max: 850,   appliesTo: [Full],         sourceUrl: "https://www.habitissimo.es/presupuestos/cambiar-ventanas" }
    - { code: interiorDoor,          unit: Unit,        min: 200,  probable: 350,   max: 700,   appliesTo: [Full],         sourceUrl: "https://www.elespanol.com/mujer/interiorismo/20260725/carpintero-poner-puertas-nuevas-casa-euros-unidad-ahora-pasan/1003744328690_0.html" }
    - { code: airConditioningSplit,  unit: Unit,        min: 1200, probable: 1500,  max: 1800,  appliesTo: [],             sourceUrl: "https://trustlocal.es/coste/instalador-aire-acondicionado-coste/" }
    - { code: airConditioningDucted, unit: Dwelling,    min: 2400, probable: 3600,  max: 4800,  appliesTo: [],             sourceUrl: "https://trustlocal.es/coste/instalador-aire-acondicionado-coste/" }
    - { code: dhwHeatPump,           unit: Unit,        min: 2200, probable: 2700,  max: 4700,  appliesTo: [Full],         sourceUrl: "https://habitissimo.es/presupuestos/aerotermia" }
    - { code: aerothermalHeating,    unit: Dwelling,    min: 9600, probable: 10000, max: 13200, appliesTo: [],             sourceUrl: "https://habitissimo.es/presupuestos/aerotermia" }
    - { code: asbestosRemoval,       unit: SquareMetre, min: 32,   probable: 45,    max: 65,    appliesTo: [],             sourceUrl: "https://www.habitissimo.es/presupuestos/retirar-amianto-uralita", minimumCharge: 1000 }
    - { code: risingDamp,            unit: Dwelling,    min: 1800, probable: 3500,  max: 7200,  appliesTo: [],             sourceUrl: "https://www.habitissimo.es/presupuestos/humedades-capilaridad" }
    - { code: demolitionDebris,      unit: SquareMetre, min: 25,   probable: 35,    max: 50,    appliesTo: [Full],         sourceUrl: "https://www.cronoshare.com/cuanto-cuesta/reforma-integral/palma-de-mallorca" }
    - { code: structuralReinforcement, unit: SquareMetre, min: 150, probable: 175,  max: 200,   appliesTo: [],             sourceUrl: "https://yes-mallorca-property.com/blog/info/how-much-does-renovation-cost-in-mallorca-in-2026/", lowConfidence: true }
  # appliesTo: [] = partida opcional, que se activa con una alerta (planta baja, edificio anterior a 2002, sin climatización, etc.)
  fees:
    # Palma. Base = PEM ≈ presupuesto con IVA / 1.10 / 1.19 (el 1.19 está NO VERIFICADO en obra privada)
    pemFromVatIncludedBudget: 0.764
    icioPercentOfPem: 4.0            # Ordenanza ICIO de Palma, art. 8.3, BOIB 168 de 27-12-2022
    icioEnergyEfficiencyBonusPercent: 25
    licenceFeePercentOfPem: 2.61     # mallorca.com, 08-09-2026 (ejemplo). Confirmar en la ordenanza de tasas
    licenceFeeCreditAgainstIcio: 0.5 # Palma descuenta del ICIO el 50 % de la tasa
    priorNotificationFee: { min: 50, max: 200 }   # comunicación previa
    architectFeePercentOfPem: { min: 6, probable: 9, max: 12, appliesWhen: majorLicence }  # Habitissimo 4–12 %; Balearic Properties 8–12 %
    contingencyPercent: { min: 10, probable: 15, max: 20 }
  permitRegime:
    # obra mayor con proyecto visado solo si se toca estructura, fachada, ampliación o demolición
    Light: priorNotification  # none solo si la obra es de menos de 3.000 € por edificio y año, en el interior y sin técnico
    Medium: priorNotification
    Full: priorNotification  # pasa a majorLicence si afecta a la estructura o a la fachada
  ageThresholds:
    - { maxYearBuilt: 1979, defaultLevel: Full,          flags: [noInsulation, aluminosisRisk, asbestosRisk, oldElectrical, leadPipeRisk] }
    - { minYearBuilt: 1980, maxYearBuilt: 2002, defaultLevel: Medium, flags: [asbestosRisk, preRebt2002Electrical, noThermalBreakWindows] }
    - { minYearBuilt: 2003, maxYearBuilt: 2008, defaultLevel: Light,  flags: [preCteWindows] }
    - { minYearBuilt: 2009, defaultLevel: ReadyToMoveIn, flags: [] }
  ageRules:
    ieeRequiredAtAgeYears: 50          # LUIB art. 125 (Ley 4/2026)
    recentRenovationMaxAgeYears: 15    # "reformado en AAAA" con año >= actual-15 baja a Light/ReadyToMoveIn
    boletinReviewAfterYears: 20        # OCU, 10-07-2025
    groundFloorFlag: risingDamp
  keywords:
    Full:
      - "ruina"
      - "en ruinas"
      - "para derribar"
      - "a demoler"
      - "a rehabilitar"
      - "para rehabilitar"
      - "a reformar"
      - "para reformar"
      - "necesita reforma"
      - "necesita reforma completa"
      - "necesita obras"
      - "estado original"
      - "original de los"
      - "para reformar a su gusto"
      - "para restaurar"
      - "per reformar"
      - "necessita reforma"
      - "estat original"
      - "per rehabilitar"
      - "per enderrocar"
      - "ruïna"
      - "to renovate"
      - "needs renovation"
      - "in need of renovation"
      - "needs complete renovation"
      - "renovation project"
      - "fixer-upper"
      - "restoration project"
      - "original condition"
      - "for demolition"
      - "renovierungsbedürftig"
      - "sanierungsbedürftig"
      - "sanierungsobjekt"
      - "renovierungsobjekt"
      - "handwerkerobjekt"
      - "restaurierungsobjekt"
      - "originalzustand"
      - "abrissobjekt"
      - "ruine"
      - "totalsanierung"
    Medium:
      - "para actualizar"
      - "a actualizar"
      - "necesita actualización"
      - "necesita algunas reformas"
      - "parcialmente reformado"
      - "cocina original"
      - "baños originales"
      - "para modernizar"
      - "anticuado"
      - "per actualitzar"
      - "parcialment reformat"
      - "cuina original"
      - "needs updating"
      - "needs some work"
      - "partially renovated"
      - "original kitchen"
      - "dated"
      - "outdated"
      - "modernisierungsbedürftig"
      - "modernisierungsbedarf"
      - "teilrenoviert"
      - "teilsaniert"
      - "veraltet"
    Light:
      - "lavado de cara"
      - "necesita pintura"
      - "pequeñas reformas"
      - "algunos retoques"
      - "mejorable"
      - "buen estado"
      - "para entrar a vivir"
      - "rentat de cara"
      - "petites reformes"
      - "bon estat"
      - "per entrar a viure"
      - "cosmetic updates"
      - "lick of paint"
      - "minor works"
      - "good condition"
      - "ready to live in"
      - "schönheitsreparaturen"
      - "leicht renovierungsbedürftig"
      - "kleinere renovierungsarbeiten"
      - "guter zustand"
      - "sofort bewohnbar"
    ReadyToMoveIn:
      - "recién reformado"
      - "totalmente reformado"
      - "reformado a estrenar"
      - "a estrenar"
      - "obra nueva"
      - "listo para entrar a vivir"
      - "llave en mano"
      - "impecable"
      - "como nuevo"
      - "totalment reformat"
      - "acabat de reformar"
      - "obra nova"
      - "clau en mà"
      - "newly renovated"
      - "fully renovated"
      - "recently refurbished"
      - "completely refurbished"
      - "move-in ready"
      - "turnkey"
      - "brand new"
      - "new build"
      - "immaculate"
      - "frisch renoviert"
      - "komplett renoviert"
      - "kernsaniert"
      - "bezugsfertig"
      - "schlüsselfertig"
      - "erstbezug"
      - "neubau"
      - "neuwertig"
    ambiguous:          # requieren contexto (año, estancia, negación)
      - "reformado"
      - "reformat"
      - "renovated"
      - "renoviert"
      - "saniert"
      - "rehabilitado"
      - "reforma integral"
      - "con proyecto"
      - "solar"
      - "baugrundstück"
      - "ideal inversores"
      - "potencial"
      - "liebhaberobjekt"
      - "conservado"
      - "actualizado"
    negations: ["no necesita", "sin necesidad de", "no requiere", "keine", "no need", "sense necessitat"]
    renovationYearPattern: "(reformad[oa]|reformat|renovated|renoviert|saniert)\\D{0,15}((19|20)\\d{2})"
```

---

## 7. Fuentes (todas consultadas el 2026-11-15)

**Mallorca y Baleares**
1. Cronoshare – ¿Cuánto cuesta una reforma integral en Palma? (08-01-2026). https://www.cronoshare.com/cuanto-cuesta/reforma-integral/palma-de-mallorca
2. Yes! Mallorca Property – How much does renovation cost in Mallorca in 2026 (2026). https://yes-mallorca-property.com/blog/info/how-much-does-renovation-cost-in-mallorca-in-2026/
3. Balearic Properties – Cost to build a new home Mallorca/Ibiza (jul-2026, rev. 19-09-2026). https://www.balearic-properties.com/en/cost-to-build-a-new-home-mallorca-ibiza
4. Ajuntament de Palma – Ordenanza fiscal del ICIO (BOIB 168, 27-12-2022). https://seuelectronica.palma.es/portal/PALMA/sede/RecursosWeb/DOCUMENTOS/1/4_151112_2.pdf
5. mallorca.com – Building licence fee / ICIO (08-09-2026). https://www.mallorca.com/en/advice/property/building-renovating/building-licence-fee-icio
6. mallorca.com – Licencia de obra o comunicación previa (28-09-2026). https://www.mallorca.com/es/consejos/inmobiliaria/construir-reformar/licencia-obra-comunicacion-previa-mallorca
7. mallorca.com – Reforma integral: costes (28-09-2026). https://www.mallorca.com/es/consejos/inmobiliaria/construir-reformar/reforma-integral-mallorca-costes
8. mallorca.com – Habitability certificate (28-09-2026). https://www.mallorca.com/en/advice/property/building-renovating/mallorca-habitability-certificate
9. mallorca.com – IEE/ITE (09-10-2026). https://www.mallorca.com/it/consigli/immobili/costruire-ristrutturare/iee-ite-mallorca-obbligatorio
10. menorca.info / Es Diari – El coste de construir se duplica en cuatro años (19-11-2025). https://www.menorca.info/menorca/local/2025/11/19/2514793/coste-construir-vivienda-menorca-duplica-cuatro-anos-por-precio-materiales.html
11. Periódico de Ibiza – Los constructores denuncian falta de mano de obra (02-11-2025). https://www.periodicodeibiza.es/noticias/baleares/2025/11/02/2503761/constructores-denuncian-falta-mano-obra-sector.html
12. Periódico de Ibiza – Murprotec y las humedades en Baleares (21-03-2025, nota comercial). https://www.periodicodeibiza.es/noticias/escaparate/2025/03/21/2350599/murprotec-celebra-anos-espana-solido-crecimiento-presencia-clave-baleares.html
13. COAIB – Informe Baleares 2024 (11-03-2025). https://mallorcadiario.cibeles.net/adjuntos/618154/INFORME-COAIB-BALEARES-2024.pdf
14. Imperial Properties – Costes de reforma en Mallorca 2026 (04-06-2026; **contenido no leído**). https://www.imperial-properties.com/es/blog/costes-reforma-mallorca-2026-guia-completa

**España (nacional)**
15. Habitissimo – Reforma integral de piso (16-09-2026). https://www.habitissimo.es/presupuestos/reforma-integral-piso
16. Habitissimo – Reformar piso (16-09-2026). https://www.habitissimo.es/presupuestos/reformar-piso
17. Habitissimo – Reforma de baño (10-06-2026). https://www.habitissimo.es/presupuestos/reforma-bano
18. Habitissimo – Cambiar ventanas (16-09-2026). https://www.habitissimo.es/presupuestos/cambiar-ventanas
19. Habitissimo – Pintar piso (16-09-2026). https://www.habitissimo.es/presupuestos/pintar-piso
20. Habitissimo – Suelo porcelánico (10-06-2026). https://www.habitissimo.es/presupuestos/suelo-porcelanico
21. Habitissimo – Aerotermia (10-06-2026). https://habitissimo.es/presupuestos/aerotermia
22. Habitissimo – Retirar amianto o uralita (16-09-2026). https://www.habitissimo.es/presupuestos/retirar-amianto-uralita
23. Habitissimo – Humedades por capilaridad (15-09-2026). https://www.habitissimo.es/presupuestos/humedades-capilaridad
24. Habitissimo – Arquitectos (28-10-2026). https://www.habitissimo.es/presupuestos/arquitectos
25. Cronoshare – Reforma integral, nacional (04-01-2026). https://www.cronoshare.com/cuanto-cuesta/reforma-integral
26. Cronoshare – Reformar instalación eléctrica (03-01-2026). https://www.cronoshare.com/cuanto-cuesta/reformar-instalacion-electrica
27. idealista – Quant costa una reforma integral (08-09-2026). https://www.idealista.com/ca/news/node/905678
28. idealista – ¿Cuánto cuesta reformar una cocina? (10-10-2026). https://www.idealista.com/news/node/914014
29. El Español – Arquitecto: reformar una casa (30-07-2026). https://www.elespanol.com/sociedad/20260730/jose-ramon-garcia-arquitecto-reformar-casa-tamano-precio-eur-tarda-meses/1003744336742_0.html
30. El Español – Electricista: cambiar la instalación eléctrica (19-07-2026). https://www.elespanol.com/mujer/interiorismo/20260719/electricista-reformar-casa-cambiar-instalacion-electrica-costaba-eur-ahora-supera/1003744319172_0.html
31. El Español – Carpintero: puertas nuevas (25-07-2026). https://www.elespanol.com/mujer/interiorismo/20260725/carpintero-poner-puertas-nuevas-casa-euros-unidad-ahora-pasan/1003744328690_0.html
32. Qué! – Precio de reformar un baño (04-09-2026). https://www.que.es/2026/09/04/precio-reforma-bano-ducha/
33. cuantomecuesta.com – Reformar piso (abr-2026). https://cuantomecuesta.com/es/reformar-piso/
34. Trustlocal – Instalar aire acondicionado (03-09-2026). https://trustlocal.es/coste/instalador-aire-acondicionado-coste/
35. hacecuentas.com – Calculadora de reforma (rev. 28-07-2026). https://hacecuentas.com/es/calculadora-coste-reforma-piso-cocina-bano-espana-m2
36. OCU – Boletín eléctrico (10-07-2025). https://www.ocu.org/vivienda-y-energia/gas-luz/consejos/boletin-electrico
37. OCU – Cédula de habitabilidad por comunidades (18-03-2026). https://www.ocu.org/fincas-y-casas/gestion/gestion-patrimonial/analisis/2026/03/cedula-habitabilidad

**Normativa y técnica**
38. INSST – NTP 1006: Materiales con amianto en viviendas (2014). https://www.insst.es/es/documentacion/colecciones-tecnicas/ntp-notas-tecnicas-de-prevencion/29-serie-ntp-numeros-996-a-1030-ano-2014/nota-tecnica-de-prevencion-ntp-1006
39. Interempresas – Obligaciones del RD 3/2023 (04-03-2024). https://www.interempresas.net/Climatizacion/543147-Nuevas-obligaciones-instalaciones-hidraulicas-para-mejora-calidad-del-agua-consumo.html
40. BOE – Real Decreto 3/2023. https://www.boe.es/buscar/doc.php?id=BOE-A-2023-628
41. Xataka Home – Tipos de tuberías antes de comprar (23-04-2025). https://www.xatakahome.com/hogar/primero-que-hice-antes-comprar-mi-casa-fue-comprobar-que-tipo-eran-sus-tuberias-tienes-este-tipo-puede-salir-muy-caro
42. Consumer Eroski – Aluminosis en edificios (20-06-2005). https://www.consumer.es/vivienda/aluminosis-en-edificios.html
43. EsadeEcPol – Informe sobre rehabilitación de vivienda (jul-2026). https://www.esade.edu/ecpol/wp-content/uploads/2026/07/Brief-Rehabilitacion-de-Vivienda.pdf

---

## 8. Lo que NO está verificado

1. **Módulos oficiales:** CYPE Generador de precios para la zona Baleares, los módulos de coste del COAATMCA y los mòduls del COAIB. No se obtuvieron cifras.
2. **Factor regional de 1,20:** se apoya en una agencia sin fuentes (+15–20 %) y en un arquitecto que habla de Menorca (hasta +25 %). No hay índice oficial.
3. **IVA incluido** en Habitissimo, idealista, El Español, Trustlocal y Yes! Mallorca: ninguna lo indica.
4. **Ventanas:** no está claro si el precio por ventana de Habitissimo incluye la colocación (la página dice que la mano de obra es "aparte").
5. **Aire por conductos:** el rango de 2.000–4.000 € de Trustlocal parece bajo y probablemente excluye el falso techo.
6. **Cubierta y fachada:** no hay €/m² fiables; solo un titular de prensa.
7. **Honorarios del aparejador** por separado.
8. **Tasa de licencia de Palma del 2,61 %:** sale de un ejemplo de mallorca.com y no se contrastó con la ordenanza de tasas.
9. **Conversión del presupuesto al PEM** (gastos generales y beneficio industrial, 1,19) en obra privada.
10. **Año en que se dejó de instalar plomo o hierro galvanizado** en España. Se usa "≤1979" como heurística.
11. **Ventanas de aluminio sin RPT antes del CTE (2006):** es una inferencia, sin fuente con fecha.
12. **Diccionarios de palabras clave:** se compilaron a partir del vocabulario habitual, sin validarlos con un corpus de anuncios de Mallorca.
13. **Cifras de Yes! Mallorca** (refuerzo estructural 150–200 €/m², acometida de Endesa desde 2.000 €, +15 % en el casco antiguo y residuos de Tirme a 30–50 €/m²): son de una agencia y no citan fuentes.
14. **Guía de Imperial Properties 2026:** no se pudo leer el contenido; conviene revisarla a mano porque dice estar "verificada".
15. **Coherencia interna de las fuentes:** Cronoshare Palma y Habitissimo tienen incoherencias en sus tablas por tamaño (por ejemplo, 90 m² sale más barato que 100 m²). Se usaron como rango, no como valor exacto.
