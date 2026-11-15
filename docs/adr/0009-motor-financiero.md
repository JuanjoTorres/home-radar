# ADR-0009 — Motor financiero (fase 2)

- **Estado:** aceptado (2026-11-15)

## Contexto
La fase 2 añade el análisis de asequibilidad: impuestos, gastos, préstamo, cuotas, veredicto, precio máximo y simulación.
ADR-0006 fijó que las cifras fiscales viven en base de datos por jurisdicción y fecha de vigencia, nunca en código.

## Decisiones

| Decisión | Motivo | Reversible |
|---|---|---|
| **Cálculo en el dominio (`HomeRadar.Domain/Finance`), sin E/S** | El motor es lógica pura: se prueba con tests rápidos y con propiedades, y la API, la ingesta y la búsqueda usan exactamente el mismo código. | — |
| **`TaxRuleSet` por jurisdicción en dos niveles: `ES` (IVA, aranceles de notaría y registro) y `ES-IB` (ITP, AJD, límites por isla, ayudas)** | Lo estatal no se duplica al añadir otra comunidad; el motor carga la cadena `ES` → `ES-IB`. | Sí |
| **Documento JSON (jsonb) por conjunto, no una fila por regla** | Las condiciones son recursivas (`anyOf`) y heterogéneas; un documento validado al cargar es más simple y se audita entero por API (`listTaxRules`). | Sí, con migración |
| **Filas inmutables con `(jurisdicción, validFrom, revision)` y `(validFrom, revision)` en `business_config_versions`** | Un análisis guardado referencia ids que nunca cambian; corregir una cifra es añadir una revisión. Sustituye el `ValidTo`/`CloseOn` de la fase 1. | — |
| **Reglas como datos interpretados por estrategias genéricas** (`ProgressiveScale`, `ReducedRate` con tope y tipo del exceso, `FlatRate`, `QuotaBonus`) con rol `Standard` / `Override` / `Relief` | Cubre toda la normativa balear vigente sin una clase por artículo; un recargo obligatorio (AJD 2 % desde 1 M €) no puede confundirse con un beneficio. | — |
| **Requisitos con lógica de tres valores (`Met`, `NotMet`, `Unknown`)** | Un dato que falta (base IRPF, renta, patrimonio) no aplica la reducción, pero el análisis dice cuánto se ahorraría: nunca se inventa un beneficio. | — |
| **Entre las opciones cumplidas se aplica la más barata; las bonificaciones no se acumulan (se toma la mayor)** | Es el resultado al que tiene derecho el comprador; la norma no prevé acumular bonificaciones de cuota. | — |
| **Préstamo = precio × LTV del perfil (tasación = precio)** | Determinista y monótono; el perfil baja el LTV si quiere pedir menos. Aviso explícito en cada análisis. | Sí |
| **Avales ICO/IBAVI como escenario aparte (`withGuarantee`), nunca combinados; ayuda de 10.000 € nunca cuenta como efectivo a la firma** | La compatibilidad ICO–IBAVI no está regulada y la ayuda se cobra después y puede agotarse. | — |
| **Veredicto con tolerancias de configuración** (déficit ≤ 5.000 €, ratio +3 puntos, escenario de estrés, edad a vencimiento 75) | Son criterios bancarios, no ley: van en `BusinessConfigVersion`. | — |
| **Precio máximo por duplicación desde 50.000 € y bisección al céntimo** | Todas las restricciones crecen con el precio y los impuestos solo saltan hacia arriba, así que lo asequible es un intervalo; un test de propiedades lo comprueba (el máximo es asequible y un céntimo más no). | — |
| **Filtro de asequibilidad en la búsqueda: precio máximo por isla y tipo, traducido a SQL por la misma especificación** | Calcular impuestos en SQL no es viable; con 4 islas × 2 tipos son 8 bisecciones por petición y la especificación sigue compartida con las alertas. | — |
| **`ListingAnalysis` guarda entradas, ids de configuración y resultado, con hash de entradas** | Reproducibilidad (sección 11 del encargo) e idempotencia: las mismas entradas devuelven el mismo análisis. | — |
| **Reforma = 0 hasta la fase 3, con aviso**; la simulación y el precio máximo ya aceptan un presupuesto de reforma con colchón configurable | El estimador por reglas llega con el Catastro en la fase 3. | — |
| **Puja recomendada y precio de retirada, pospuestos a la fase 3** | Dependen del valor estimado (comparables), que no existe aún; hoy el precio máximo es el único "precio de retirada" defendible. | — |

## Consecuencias
- Cambiar una cifra fiscal o un supuesto es editar YAML y subir la revisión; la CI valida las semillas (`Seed_files_are_valid`).
- `getListingAnalysis` escribe en base de datos (guarda el análisis); es idempotente por hash.
- Los rangos (`DecimalRangeDto`) exigen `min` y `max` en el contrato (pueden ser `null`); se mantiene por compatibilidad con la v1.
