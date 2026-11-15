# ADR-0010 — Inteligencia por reglas (fase 3)

- **Estado:** aceptado (2026-11-15)

## Contexto
La fase 3 añade la lectura de cada anuncio: reforma, banderas rojas, datos del Catastro, comparación con el mercado, margen de
negociación, puntuación de oportunidad y puja recomendada. Todo por reglas deterministas y explicables, sin aprendizaje
automático. Las cifras salen de la investigación de `docs/research/` (reforma en Mallorca, negociación y banderas, Catastro),
con sus fuentes y lo que no se pudo verificar.

## Decisiones

| Decisión | Motivo | Reversible |
|---|---|---|
| **Todos los parámetros en `BusinessConfigVersion`** (niveles y partidas de reforma, reglas de antigüedad, diccionarios, banderas, factores de negociación, pesos de la puntuación, comparables, tolerancia de puja), nueva versión `validFrom 2026-11-15` | Son supuestos de mercado, no ley: se corrigen añadiendo una revisión, y cada análisis guarda la versión que usó. | — |
| **Coste de reforma = el mayor entre €/m² del nivel × superficie y la suma de partidas; partidas como desglose orientativo y el resto como `otherWorks`; licencia + ICIO ≈ 3,1 %** | La investigación muestra que las partidas típicas suman la mitad del coste real por m²: sumar solo partidas infravaloraría la reforma. | Sí |
| **Nivel por palabras clave (ES, CA, EN, DE) con negaciones de dos palabras y "reformado en AAAA"; si no hay texto, por año (Catastro primero): antes de 1980 integral, 1980–2002 media, 2003–2008 ligera** | Explicable y auditable (`signals`); el texto del anunciante prevalece sobre la antigüedad salvo que la contradiga, y entonces baja la confianza. | Sí |
| **Sin descripción ni año, se asume reforma ligera con confianza baja y 80 m² si falta la superficie** | Es más prudente sumar un coste moderado que suponer cero: un piso sin datos no debe parecer más asequible que uno bien descrito. | Sí |
| **La reforma manual (`setRenovationOverride`) siempre prevalece y no lleva colchón; la estimada lleva un 15 % de imprevistos** | Un presupuesto real ya incluye sus márgenes; uno estimado no. El anuncio guarda si su reforma es manual para que búsqueda y análisis apliquen lo mismo. | — |
| **La reforma entra en el análisis, la búsqueda por asequibilidad y las alertas** | El objetivo es saber si se puede pagar el piso terminado, no solo el precio. SQL acota candidatos con los límites sin reforma y cada candidato se comprueba exacto con su propio coste. | — |
| **Banderas: reglas de palabras clave con exclusiones ("libre de cargas", "estilo rústico") + reglas de datos** (posesión, m² anunciados frente a Catastro, suelo rústico, edificio de 50 años o más sin IEE) | Las palabras clave solas dan falsos positivos; cruzarlas con datos estructurados es más fiable. "Embargo" suelto se descartó porque "sin embargo" es habitual. | Sí |
| **Comparables de la propia base de datos**: mismo tipo, ±35 % de superficie, activos o vistos en 365 días; radio de 1,5 km, si no municipio, si no isla, con un mínimo de 5 | No hay fuente pública de €/m² por municipio que se pueda leer por máquina con fiabilidad (MIVAU e IBESTAT quedan documentados para más adelante). | Sí |
| **Negociación: reglas aditivas** (tipo de vendedor 6 % / 8 % banco o servicer / 2 % obra nueva; bajadas; días en mercado; reaparición; urgencia; reforma; sobreprecio; temporada) con tope del 15 % y rango −3/+4 puntos; **sin margen con una bandera crítica** | No existe estudio público español que cuantifique cada factor: los pesos son juicio documentado y configurable. Con un ocupado o sin posesión no se negocia el precio, se descarta o se estudia aparte. | Sí |
| **Puntuación 0–100 = media ponderada de los componentes disponibles (`IScoreComponent`)**; tope de 30 con bandera crítica | Un dato que falta no penaliza ni premia: se reparte el peso entre lo que sí se sabe. Añadir un componente es añadir una clase y su peso. | — |
| **Puja: inicial = precio − margen máximo; recomendada = precio − margen probable; retirada = el menor entre precio máximo asequible y valor estimado + 5 %; redondeo a 500 € hacia abajo. Si ningún precio es asequible, no hay puja** | Nunca recomendar pagar más de lo que se puede pagar ni muy por encima del mercado. | Sí |
| **Catastro por los servicios libres de la OVC (JSON), con caché de 180 días por parcela, 1 petición/s y sin reintentos tras un bloqueo; si el punto cae en la calle (error 16), la parcela más cercana a 15 m o menos; unidad emparejada por referencia de 20 caracteres, unidad residencial única o superficie dentro del 20 %** | Son datos no protegidos y de uso libre; casar el piso exacto permite comparar superficie y año. Si no se puede, se usan los datos de la parcela con calidad `Parcel`. | — |
| **Valor de referencia manual (`setCadastralData`)** | La Sede solo lo da con identificación o CAPTCHA y prohíbe el acceso automatizado. | — |
| **Euríbor mensual del CSV del Banco de España en `reference_rates`, superpuesto a la configuración cuando es más reciente** | Evita editar la configuración cada mes y no oculta revisiones posteriores del YAML; el mes de euríbor entra en el hash del análisis. | Sí |
| **La consulta de un análisis guarda en el anuncio nivel, coste, puntuación y banderas solo si cambian; un trabajo nocturno (04:17 UTC) recalcula todos** | Los filtros de búsqueda y la ordenación por puntuación funcionan en SQL sin recalcular nada en cada consulta. | — |

## Consecuencias
- Cambiar un precio de reforma, una palabra clave o un peso es editar `config/seed/business-config.yaml` con una nueva revisión.
- Un anuncio sin descripción ni año aparece con reforma ligera estimada: sus análisis pueden pasar de "asequible" a "límite"
  o "no asequible" respecto a la fase 2. Es intencionado; se corrige con `setRenovationOverride` tras la visita.
- Los comparables mejoran con el tiempo: al principio habrá pocos y muchos anuncios no tendrán `market`.
- Las respuestas reales del Catastro y del Banco de España se graban con el flujo `record-fixtures` y los tests de contrato
  las validan; si la OVC cambia de formato, esos tests lo detectan al volver a grabar.
