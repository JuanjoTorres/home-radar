# Casos de cálculo verificados a mano

Cifras calculadas con aritmética decimal independiente del código C# (script de referencia en Python con `decimal`,
redondeo bancario a céntimos) y comprobadas por los tests de `tests/HomeRadar.Application.Tests/Finance`.
Fecha de cálculo 15/11/2026, Mallorca (límite insular 331.859,70 €), tramo reducido 270.151,20 €.

## ITP (vivienda usada)
| Caso | Comprador | Precio | Cálculo | Cuota |
|---|---|---|---|---|
| A | 34 años, primera vivienda | 250.000 | 2 % × 250.000 | 5.000,00 |
| B | 34 años, primera vivienda | 300.000 | 2 % × 270.151,20 + 8 % × 29.848,80 | 7.790,93 |
| B2 | 34 años, primera vivienda | 331.859,70 | 2 % × 270.151,20 + 8 % × 61.708,50 | 10.339,70 |
| C | 34 años, primera vivienda | 340.000 | supera el límite insular → 8 % × 340.000 | 27.200,00 |
| D | 40 años, vivienda habitual | 300.000 | 4 % × 270.151,20 + 8 % × 29.848,80 | 13.193,95 |
| E | 28 años, base IRPF 30.000 individual, LTV 80 % | 300.000 | bonificación 100 % del primer tramo → 8 % × 29.848,80 | 2.387,90 |
| E2 | 28 años sin base IRPF | 300.000 | se aplica el 2 % (7.790,93); ahorro potencial si se cumple: 5.403,02 | 7.790,93 |
| F | 45 años, segunda vivienda | 600.000 / 700.000 | 8 % × 400.000 + 9 % × 200.000 (+ 10 % × 100.000) | 50.000,00 / 60.000,00 |
| G | 45 años, familia numerosa | 370.000 | límite × 1,15 = 381.638,66 → 2 % × 270.151,20 + 8 % × 99.848,80 | 13.390,93 |

## Obra nueva
| Caso | Comprador | Precio | IVA | AJD |
|---|---|---|---|---|
| H | 34 años, primera vivienda | 250.000 | 25.000,00 | 0,5 % → 1.250,00 |
| I | 45 años, primera vivienda | 300.000 | 30.000,00 | 1 % × 270.151,20 + 1,5 % × 29.848,80 = 3.149,24 |
| J | 50 años, segunda vivienda | 1.200.000 | 120.000,00 | 2 % → 24.000,00 |

## Aranceles (con IVA y extras)
| Base | Notaría (escala × 0,95 × 1,21 + 150) | Registro (escala × 0,95 × 1,21 + 3,64) |
|---|---|---|
| 5.000 | 253,63 | 31,27 |
| 250.000 | 590,76 | 234,93 |
| 300.000 | 619,50 | 252,18 |
| 700.000 | 826,64 | 378,74 |

## Cuotas (sistema francés)
| Préstamo | Tipo | Plazo | Cuota |
|---|---|---|---|
| 200.000 | 3 % fijo | 360 | 843,21 |
| 200.000 | 2,9 % fijo | 360 | 832,46 |
| 200.000 | variable: 2,5 % 12 meses, luego euríbor 3,239 + 0,6 | 360 | 790,24 → 932,01 (base), 1.046,04 (+1), 1.166,52 (+2) |
| 200.000 | mixta: 2,2 % 120 meses, luego euríbor + 0,7 | 360 | 759,40 → 888,12 |

## Asequibilidad (perfil: 60.000 € de ahorro, 3.000 €/mes, fijo 2,9 % 30 años, LTV 80 %, 34 años)
Piso de 250.000 € en Palma: ITP 5.000 + notaría 590,76 + registro 234,93 + gestoría 400 + tasación 450 → coste total
256.675,69 €; préstamo 200.000 €; efectivo necesario 56.675,69 €; quedan 3.324,31 €; cuota 832,46 € (27,75 % de ingresos)
→ **Asequible**. Con aval IBAVI (renta 45.000 €): préstamo 250.000 €, comisión 0,5 % × 50.000 = 250 €, efectivo necesario
6.925,69 €.
