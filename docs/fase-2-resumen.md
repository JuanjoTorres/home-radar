# Fase 2 — Resumen (2026-11-15)

## Hecho
- **Reglas fiscales en base de datos** (`tax_rule_sets`), nunca en código: un conjunto estatal (`ES`: IVA 10 %/4 %, aranceles de notaría y registro) y otro balear (`ES-IB`: escala del ITP, 4 %, 2 % para menores de 36 / discapacidad / familias, bonificación del 100 % para menores de 30, VPL, AJD 1,5 %/1 %/0,5 %/2 %, límites por isla con sus fechas, avales ICO e IBAVI y ayuda de 10.000 €). Se siembran desde `config/seed/tax-rules/*.yaml`, se validan al cargar (un fichero erróneo se rechaza y se registra) y cada revisión es una fila inmutable.
- **Configuración de negocio versionada** (`business_config_versions`): euríbor de referencia (3,239 %, octubre 2026, confirmado en el Banco de España), escenarios +1 y +2 puntos, tolerancias del veredicto y estimaciones de gestoría y tasación.
- **Motor de cálculo** en el dominio, determinista y sin E/S:
  - impuestos con la regla aplicada, su fuente legal y la evaluación de cada requisito (`Met`, `NotMet` o `Unknown`); si falta un dato, la reducción no se aplica y se informa del ahorro potencial;
  - coste total (precio, impuestos, notaría, registro, gestoría, tasación, comisión de apertura, comisión del aval, reforma y su colchón);
  - cuota francesa para fija, variable y mixta, con los tres escenarios de euríbor;
  - veredicto `Affordable` / `Borderline` / `NotAffordable` con motivos y cifras;
  - escenario con aval ICO o IBAVI (nunca combinados) y elegibilidad de cada ayuda;
  - precio máximo asequible por bisección al céntimo e indicación de qué lo limita;
  - precio máximo dentro de un presupuesto total.
- **API**: `getListingAnalysis`, `simulatePurchase`, `calculateMaximumAffordablePrice`, `getBusinessConfig`, `listBusinessConfigVersions`, `listTaxRules`, y el filtro `affordability` (perfil y/o presupuesto) en `searchListings`, en `listListings` (`affordableFor`, `budget`) y en las búsquedas guardadas.
- **Reproducibilidad**: cada análisis de anuncio se guarda con sus entradas (huella del anuncio, versión del perfil, ids de configuración y fecha) y un hash; las mismas entradas devuelven el mismo análisis.
- **Ingesta y alertas**: nueva etapa `Analyze` del pipeline (anuncios nuevos, bajadas de precio y reapariciones, con el perfil por defecto). El mensaje de Telegram incluye el veredicto (✅ / ⚠️ / ❌), el motivo, si un aval lo cambiaría y tu precio máximo.
- **Contrato OpenAPI**: 6 operaciones nuevas documentadas, con ejemplos reales de petición y respuesta (también para `searchListings`, `createFinancingProfile` y `createSearchProfile`). Spectral valida esos ejemplos contra los esquemas.
- **Tests**: de 56 a 111.
  - Casos calculados a mano con aritmética decimal independiente (`docs/finance/casos-verificados.md`).
  - Propiedades con CsCheck: el precio máximo es asequible y un céntimo más no lo es; el coste total nunca baja al subir el precio; el límite por presupuesto encaja al céntimo.
  - Integración con PostGIS: endpoints, errores, búsqueda por asequibilidad y alerta con veredicto.
- ADR-0009 con las decisiones de la fase y `docs/api/README.md` con el flujo completo (perfil → precio máximo → buscar lo que puedo pagar → análisis → alerta).

## Lo que falló o cambió sobre lo previsto
- **Defecto de la fase 1 corregido en el contrato**: los decimales opcionales se publicaban como obligatorios (el transformador perdía el `null`). Ahora aceptan `null`; para un cliente es un cambio compatible.
- **Los rangos (`price`, `rooms`…) exigen `min` y `max` en el contrato** (pueden ir a `null`). Lo dejo así para no romper la v1.
- **Ejemplos**: la fase 1 no tenía ejemplos en el contrato. Ahora los tienen las operaciones nuevas y los POST de la fase 1; faltan las respuestas de las operaciones de anuncios, que necesitan datos reales.
- **Reforma**: no se estima hasta la fase 3. Los análisis de anuncios avisan de que no la incluyen; la simulación y el precio máximo ya aceptan un presupuesto de reforma.
- **Puja recomendada y precio de retirada**: pospuestos a la fase 3, porque necesitan un valor estimado por comparables.

## Pendiente de confirmar (no bloquea)
- **Bonificación del 100 % para menores de 30**: el 8 % sobre el exceso de 270.151,20 € es una interpretación sin criterio publicado de la ATIB. Está marcada `verified: false` y sale como aviso cuando se aplica.
- **Compatibilidad ICO–IBAVI**: no está regulada; el motor nunca los combina.
- **Ayuda de 10.000 €**: depende de que quede presupuesto. Nunca cuenta como efectivo a la firma.
- **Valor de referencia del Catastro**: no tiene API sin identificación. Hasta la fase 3, la base imponible es el precio y cada análisis lo avisa.

## Lo que necesito de ti
Tus datos reales en `config/seed/financing-profiles.local.yaml` del host (o por API). Sin ellos no se comprueban la bonificación del ITP ni los avales:
- `taxableIncomeBase` y `jointTaxReturn`;
- `grossAnnualIncome`, `netWorth` y `buyerCount`.

## Propuesta para la fase 3
- **Catastro (OVC)** por referencia catastral o coordenadas: superficie, año y uso, con aviso si los m² no cuadran. Campo manual por API para el valor de referencia.
- **Estimador de reforma por reglas** con corrección manual (`setRenovationOverride`), integrado en el análisis y en el filtro de asequibilidad.
- **Margen de negociación** a partir del historial.
- **Score de oportunidad, banderas rojas y comparables** de la propia base de datos.
- **Puja recomendada y precio de retirada.**
- **Actualización mensual automática del euríbor** desde la serie oficial del Banco de España.
