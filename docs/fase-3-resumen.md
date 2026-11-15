# Fase 3 — Resumen (2026-11-15)

## Hecho
- **Estimador de reforma por reglas** con cuatro niveles (para entrar a vivir, ligera, media, integral):
  - lee la descripción en español, catalán, inglés y alemán, ignora negaciones ("no necesita reforma") y reconoce "reformado en 2021";
  - sin texto, decide por el año de construcción (el del Catastro primero);
  - coste mínimo, probable y máximo con precios de Mallorca con IVA: ligera 120/220/400 €/m², media 450/650/950, integral 800/1.100/1.600;
  - desglose orientativo (cocina, baños, suelos, pintura, instalaciones, ventanas, puertas, aerotermia, demolición…), resto de obra, licencia e ICIO;
  - lista de qué comprobar en la visita según el nivel (aluminosis, humedades de capilaridad, tuberías de plomo, boletín eléctrico…).
- **Reforma manual** (`setRenovationOverride`, `deleteRenovationOverride`): partidas, presupuesto total o solo el nivel. Siempre prevalece, con ETag para no pisar cambios.
- **Estimación libre** (`estimateRenovation`) para un anuncio o con parámetros a mano.
- **Banderas rojas**: 20 reglas configurables con exclusiones (ocupado, sin posesión, cesión de remate, nuda propiedad, proindiviso, sin cédula, rústico, VPO/VPL, inquilino, embargo, cargas, deudas de comunidad, alquiler turístico…) y reglas cruzadas con datos: m² anunciados frente al Catastro, parcela rústica, edificio de 50 años o más sin IEE.
- **Catastro**: cliente de los servicios libres de la OVC (punto → parcela → unidades; si el punto cae en la calle, la parcela más cercana a 15 m o menos), caché de 180 días y emparejamiento con el piso concreto. El valor de referencia se registra a mano (`setCadastralData`) porque no hay acceso automatizado legal; desde entonces el ITP se calcula sobre el mayor entre precio y valor de referencia.
- **Mercado**: €/m² mediano y cuartiles de anuncios comparables de la propia base (1,5 km, municipio o isla), desviación del anuncio y valor estimado.
- **Margen de negociación** en rango, con cada factor explicado; no aplica si hay una bandera crítica.
- **Puntuación de oportunidad 0–100** con seis componentes y pesos configurables (precio frente a mercado, asequibilidad, negociación, reforma, riesgo y competencia según la fuente).
- **Puja**: oferta inicial, puja recomendada y precio de retirada. Si no hay ningún precio asequible con esa reforma, no recomienda puja y lo explica.
- **Asequibilidad con reforma**: el análisis, la búsqueda por asequibilidad y las alertas incluyen la reforma estimada (con un 15 % de imprevistos) o la manual.
- **Simulación con márgenes**: reforma máxima que sigue siendo asequible, valor de una vivienda reformada equivalente y reforma máxima sensata.
- **Búsqueda**: filtros `minScore`, `renovationLevels` y `excludeRedFlags`, y orden `ScoreDesc`.
- **Telegram**: el aviso muestra puntuación, reforma estimada y banderas en español.
- **Euríbor automático**: el CSV mensual del Banco de España se descarga cada día y actualiza el escenario base sin crear versiones de configuración.
- **Trabajos**: recálculo nocturno de puntuaciones y actualización diaria del euríbor.
- **Contrato OpenAPI**: 4 operaciones nuevas y 3 actualizadas, con ejemplos reales; Spectral sin errores ni avisos.
- **Tests**: de 111 a 143, todos en verde y sin omitidos. Unitarios de reforma, negaciones, banderas, negociación (con propiedades), puntuación, puja y emparejamiento catastral; contrato con respuestas reales del Catastro y del Banco de España grabadas desde GitHub Actions; integración de los endpoints, filtros, orden y alertas.
- ADR-0010 con las decisiones e informes de investigación en `docs/research/`.

## Lo que falló o cambió sobre lo previsto
- **El coste por partidas infravaloraba la reforma** a la mitad. Ahora el total es el mayor entre €/m² del nivel y la suma de partidas; las partidas quedan como desglose.
- **Cambio de comportamiento**: un anuncio sin descripción ni año se asume con reforma ligera. Algunos pisos que en la fase 2 salían asequibles pueden salir ahora "límite" o "no asequibles". Es lo prudente; tras visitar, fija la reforma real.
- **"embargo" como palabra clave** daba falsos positivos con "sin embargo"; se usan solo formas inequívocas.
- **Comparables externos (MIVAU, IBESTAT, Notariado)**: no se pudieron descargar de forma fiable desde la investigación. De momento solo hay comparables de nuestra base, que crecerá con el tiempo.
- **Coordenadas en la calle**: la primera grabación real mostró que un punto del centro de Palma no cae dentro de ninguna parcela (error 16). Como las coordenadas de los portales suelen ser aproximadas, ahora se busca la parcela más cercana a 15 m o menos.

## Pendiente de confirmar (no bloquea)
- Los pesos de la negociación son juicio documentado: no existe estudio público español que cuantifique cada factor.
- Los precios de reforma llevan un factor regional de Mallorca (×1,20) derivado de varias fuentes, sin índice oficial.
- Vigencia de la cédula de habitabilidad (10 años) confirmada solo en fuente secundaria.

## Lo que necesito de ti
- Confirmar la fase para seguir con la siguiente.
- Tras visitar un piso, registrar la reforma real (`setRenovationOverride`) y el valor de referencia (`setCadastralData`).
