# ADR-0005 — Fuentes de la fase 1 (propuesta pendiente de confirmación)

- **Estado:** aceptado (confirmado por el propietario el 2026-11-14). Detalle y evidencias en `docs/sources/README.md`.

## Contexto (hallazgo principal de la fase 0)
El stock bancario en Mallorca es **pequeño**: Solvia ≈ 74 pisos en la provincia, Aliseda ≈ 18 viviendas, Hipoges ≈ 12, Servihabitat ≈ 3, Diglo 14 activos casi todos terciarios, Banca March solo locales, Unicaja/Cimenta2/Bankinter ≈ 0. Ningún portal expone API JSON ni sitemap de anuncios; todo es HTML server-side (Solvia, Aliseda, Diglo, Banca March) o SPA (Hipoges, Cimenta2, EscogeCasa). Los avisos legales de Aliseda y Diglo prohíben la "extracción y/o reutilización"; Solvia solo la explotación comercial.

En cambio, las **agencias locales** son la vía real para escalar: tras la segunda pasada (62 de 76 webs clasificadas), Inmovilla es el CRM más frecuente con 11 agencias (web PHP nativa o plugin WordPress, ID global de 8 dígitos), seguido de desarrollos a medida (12), WordPress genérico (10), InfoCasa (5, CRM local con patrón idéntico), Inmopc, onOffice y RE/MAX (2 cada uno). El feed Kyero V3 XML cubre a Witei, Inmobalia, Resales y varios más.

## Decisión propuesta: las 3 fuentes de la fase 1
1. **Solvia** (`solvia`): listado provincial HTML cada **6 h** (≈ 8 páginas), clave `id1-id2`; ficha con Playwright solo para IDs nuevos. Alta en "Búsquedas guardadas" por email como respaldo (adaptador IMAP).
2. **Inmovilla** (`inmovilla`): adaptador de CRM que cubre varias agencias a la vez (dos parsers: web nativa `/ficha/.../{id8}/es/` y plugin WordPress `/propiedad/{slug}-{id8}/`), sondeo por agencia cada **2–4 h** según tamaño. Es la fuente con más potencial de "enterarme antes que nadie".
3. **Aliseda** (`aliseda`): `/comprar-viviendas/islas-baleares?page=N` cada **12 h**, campos de posesión desde el listado. Volumen mínimo por su aviso legal.

**Casi gratis, incluidos como 4.º y 5.º:** **Diglo** (1 petición/12 h, trae coordenadas y referencia catastral; canario para cuando Santander suba vivienda balear) y **EmailAlertSource** (Unicaja `formAlerta.do`, Servihabitat, Solvia).

**Segunda ola (fase 4):** feed Kyero V3, InfoCasa, WordPress genérico (`wp-json`/sitemap), onOffice, Mobilia, Hipoges (SPA sin API documentada; requiere identificar su XHR con DevTools en el navegador y leer su `robots.txt` desde el cliente real), DonComparador como redundancia.

## Latencia de alerta < 15 min
Con estos sondeos no se cumple en todas las fuentes. Propuesta: la latencia objetivo de 15 min se aplica a las fuentes marcadas `Priority` (Inmovilla de las agencias de la zona objetivo del propietario y Solvia), con sondeo cada 10–15 min **solo de la primera página ordenada por "más recientes"** y sondeo completo cada 6 h. El resto, 6–24 h. Esto exige que el propietario indique la zona (pregunta 1 de la fase 0).

## Consecuencias
- Fase 1 implementa 3 adaptadores HTML (Solvia, Inmovilla ×2 parsers, Aliseda) + Diglo + IMAP; Playwright solo para fichas de Solvia.
- El catálogo de agencias (`agencies-mallorca.csv`, 76 agencias, 62 clasificadas, 6 bloqueadas, 4 sin web, 4 sin localizar) se amplía en la fase 4 con el detector automático.
