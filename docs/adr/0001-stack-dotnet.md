# ADR-0001 — Stack tecnológico (.NET 10 LTS)

- **Estado:** aceptado (fase 0, 2026-11-14)
- **Contexto:** servicio API sin frontal + worker de ingesta, un solo usuario, coste bajo, librerías con licencia libre. Verificación de versiones y licencias en `docs/dependencies.md`.

## Decisión

| Necesidad | Elección | Motivo / cambio respecto a la propuesta inicial |
|---|---|---|
| Runtime | **.NET 10 (LTS, soporte hasta 2028-11-14)**, `net10.0`, C# 14 | .NET 11 es GA desde 2026-11-10 pero STS; no compensa para un servicio que debe vivir tranquilo dos años. .NET 8/9 ya están fuera de soporte. |
| API | ASP.NET Core Minimal APIs, `MapGroup` por feature, `TypedResults` | Sin cambios. |
| OpenAPI | `Microsoft.AspNetCore.OpenApi` (3.1) + comentarios XML + **Scalar.AspNetCore** (MIT) | Sin cambios. Exportación en build con `Microsoft.Extensions.ApiDescription.Server`. |
| Versionado | `Asp.Versioning.Http` (MIT), `/v1` en la URL | Sin cambios. |
| Validación | **FluentValidation 12.x (Apache-2.0, sin tier comercial, verificado)** | Se mantiene; la validación integrada de Minimal APIs se usa solo para atributos triviales. |
| Errores | `ProblemDetails` RFC 9457 con `type` propio por error de negocio | Sin cambios. |
| Persistencia | PostgreSQL 17 + PostGIS, EF Core 10 + `Npgsql.EntityFrameworkCore.PostgreSQL(.NetTopologySuite)` | Sin cambios. **Sin Dapper** hasta que un endpoint lo justifique (ADR futuro). |
| Workers | `BackgroundService` + **Quartz 4.x** con `AdoJobStore` en Postgres | `Quartz.Extensions.Hosting` ya no existe como paquete útil en Quartz 4: se usa `Quartz` + `Quartz.AspNetCore`/hosting integrado. |
| HTTP | `IHttpClientFactory` + `Microsoft.Extensions.Http.Resilience` (Polly v8) | Sin cambios. |
| Rate limiting saliente | `System.Threading.RateLimiting` en un `DelegatingHandler` por dominio | Sin cambios. |
| HTML | AngleSharp (MIT) | Sin cambios. |
| Renderizado | `Microsoft.Playwright` (Apache-2.0), solo para fuentes SPA (Hipoges) y bajo demanda | Sin cambios. |
| Fotos | **Hash perceptual propio (dHash + pHash 64 bits) sobre SkiaSharp (MIT)** | **Cambio:** `CoenM.ImageSharp.ImageHash` está abandonado (2022) y arrastra `SixLabors.ImageSharp`, cuya Split License exige clave de licencia en build desde la 3.2/4.0. Un dHash/pHash son ~60 líneas y se testean con propiedades. |
| Email entrante (plan B de fuentes) | **MailKit (MIT)** vía IMAP | **Nuevo:** las alertas por email de los portales (Solvia, Unicaja, Servihabitat) son un adaptador de primera clase. |
| Notificaciones | Telegram.Bot (MIT) + webhooks salientes (fase 4) | Sin cambios. |
| Mapeo | Mapperly (`Riok.Mapperly`, Apache-2.0) | Sin cambios. |
| Observabilidad | OpenTelemetry + Serilog (consola JSON; sin CloudWatch) | Sin cambios. |
| Local | Docker Compose con PostGIS y MinIO | Aspire descartado: añade un dashboard y un orquestador para dos procesos. |
| Tests | xunit.v3, Testcontainers.PostgreSql, WireMock.Net, **Verify.XunitV3**, **ArchUnitNET**, **CsCheck** | **Cambios:** `Verify.Xunit` está deprecado a favor de `Verify.XunitV3`; `NetArchTest.Rules` lleva sin mantenimiento desde 2021; CsCheck (MIT) en lugar de FsCheck (menos fricción desde C#). Aserciones: **Shouldly** o las de xunit, nunca FluentAssertions v8. Mocks: NSubstitute. |
| Calidad | `Directory.Build.props`, `Directory.Packages.props`, `.editorconfig`, nullable, `TreatWarningsAsErrors`, analizadores, `dotnet format` en CI | Sin cambios. |

## Paquetes vetados y por qué
MediatR y AutoMapper (licencia RPL + comercial desde 2025), MassTransit v9 (comercial), FluentAssertions v8 (Xceed), Moq ≥ 4.20 (SponsorLink), Hangfire (LGPL/Pro), SixLabors.ImageSharp (Split License + clave de build), NetArchTest.Rules (abandonado).

## Consecuencias
- Cero dependencias con riesgo de licencia; `docs/dependencies.md` se revisa en cada actualización de paquetes (job trimestral en CI que falla si cambia el campo `license` en NuGet).
- Implementar el hash perceptual propio añade un día de trabajo y tests, a cambio de no depender de ImageSharp.
