# Dependencias — HomeRadar Mallorca

Fecha de verificación: **2026-11-14**. Todas las versiones y licencias se comprobaron contra las páginas de nuget.org (o un espejo cuando se indica) ese día. Método: WebSearch + WebFetch de las páginas oficiales; el proxy de esta sesión bloquea `api.nuget.org`, así que no se usó la API JSON de NuGet.

## 1. Plataforma .NET

| Dato | Valor | Fuente |
|---|---|---|
| LTS vigente | **.NET 10** (lanzado 2025-11-11, parche actual 10.0.15) | https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-core |
| Fin de soporte .NET 10 | **2028-11-14** | ídem |
| .NET 11 | **GA el 2026-11-10**, STS, fin de soporte 2028-11-14 (ya no es preview) | ídem; https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-11/overview |
| .NET 8 / .NET 9 | **Fuera de soporte desde 2026-11-10** | ídem |
| C# por defecto | C# 14 con .NET 10; **C# 15** con .NET 11 (union types, closed hierarchies, extension indexers…) | https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-11/overview |

**Decisión recomendada:** `net10.0` (LTS) como TFM del backend y del worker; .NET 11 solo si se necesita alguna feature de C# 15, sabiendo que caduca el mismo día que .NET 10. Los paquetes `Microsoft.*` 11.0.0 y 10.0.15 salieron el mismo día (2026-11-10); usar la línea 10.0.x.

## 2. Tabla de paquetes

Leyenda «cambio?»: ¿hubo cambio de licencia o aparición de tier comercial entre 2024 y 2026? Veredicto: **usar / evitar / alternativa**.

| Paquete | Versión estable (2026-11-14) | Licencia (SPDX) | Cambio 2024-26? | Veredicto | Fuente |
|---|---|---|---|---|---|
| Microsoft.AspNetCore.OpenApi | 10.0.15 (línea .NET 10) / 11.0.0 | MIT | No | **Usar** | https://www.nuget.org/packages/Microsoft.AspNetCore.OpenApi |
| Scalar.AspNetCore | 2.21.0 (2026-11-12) | MIT | No | **Usar** (UI de OpenAPI; sustituye Swashbuckle) | https://www.nuget.org/packages/Scalar.AspNetCore |
| Asp.Versioning.Http / Asp.Versioning.Mvc.ApiExplorer | 10.2.1 (2026-08-06) | MIT | No | **Usar** (org `dotnet`, owner API-Versioning-Team) | https://www.nuget.org/packages/Asp.Versioning.Mvc.ApiExplorer |
| FluentValidation | 12.2.0 (2026-10-10) | Apache-2.0 | **No.** Sigue Apache-2.0; solo pide patrocinio a usuarios comerciales (GitHub Sponsors/OpenCollective). No hay tier comercial ni cambio de licencia en 12.x | **Usar** (vigilar releases 13.x) | https://www.nuget.org/packages/fluentvalidation |
| Npgsql.EntityFrameworkCore.PostgreSQL | 10.0.1 estable; 11.0.0 aún en preview/rc (EF Core 11 rc.2) | PostgreSQL | No | **Usar** 10.0.x con .NET 10 | https://packages.ecosyste.ms/registries/nuget.org/packages/npgsql.entityframeworkcore.postgresql ; https://www.npgsql.org/efcore/release-notes/11.0.html |
| Npgsql.EntityFrameworkCore.PostgreSQL.NetTopologySuite | 10.0.1 (misma línea) | PostgreSQL | No | **Usar** (PostGIS) | https://packages.ecosyste.ms/registries/nuget.org/packages/Npgsql.EntityFrameworkCore.PostgreSQL.NetTopologySuite |
| Quartz | 4.7.3 (2026-11-09) | Apache-2.0 | No | **Usar** | https://www.nuget.org/packages/Quartz |
| Quartz.Extensions.Hosting | 4.7.1 — **paquete vacío**: desde Quartz 4.0 su contenido está en `Quartz`; eliminar la referencia | Apache-2.0 | No | **No referenciar** (basta `Quartz`) | https://www.nuget.org/packages/Quartz.Extensions.Hosting/ |
| Quartz.Serialization.SystemTextJson | 4.x (sigue la versión de Quartz; 4.6.0 visto en feed, 4.7.x esperado — verificar al restaurar) | Apache-2.0 | No | **Usar** para el JobStore persistente | https://www.nuget.org/packages/Quartz.Serialization.SystemTextJson |
| Microsoft.Extensions.Http.Resilience | 10.x (línea .NET 10) / 11.0.0 (2026-11-10) | MIT | No | **Usar** (pipeline estándar sobre Polly) | https://www.nuget.org/packages/Microsoft.Extensions.Http.Resilience |
| Polly | 8.9.1 (2026-11-07) | BSD-3-Clause | No | **Usar** (ya viene transitivamente vía Http.Resilience) | https://www.nuget.org/packages/Polly |
| AngleSharp | 1.10.3 (2026-11-14, dato de espejo nuget.rza.ru; nuget.org no devolvió número) | MIT | No | **Usar** (parser HTML para scrapers) | https://nuget.rza.ru/packages/anglesharp ; https://packages.ecosyste.ms/registries/nuget.org/packages/AngleSharp |
| Microsoft.Playwright | 1.64.1 (2026-10-22) | MIT | No | **Usar** (solo para portales con JS; coste en imagen Docker) | https://packages.nuget.org/packages/microsoft.playwright |
| CoenM.ImageSharp.ImageHash | 1.3.6 (**2022-07-05**, sin releases desde entonces) | MIT | No, pero **depende de SixLabors.ImageSharp >= 2.1.3** (línea 2.x, Apache-2.0 pero sin soporte y con CVEs históricas) | **Evitar / alternativa**: implementar pHash/dHash propio (~100 líneas) sobre la librería de imagen que elijamos, o SkiaSharp (MIT) / Magick.NET (Apache-2.0) | https://www.nuget.org/packages/CoenM.ImageSharp.ImageHash ; https://github.com/coenm/imagehash |
| SixLabors.ImageSharp | 4.1.3 (2026-10-09) | Six Labors Split License 1.0 (no SPDX; Apache-2.0 **o** comercial según uso) | **Sí**: (a) Split License desde 3.0 (2022); (b) **desde 3.2.0/4.0 el build exige clave de licencia** (`sixlabors.lic` / `SixLaborsLicenseKey`) en proyectos que lo referencian directamente; falla en Release con «No Six Labors license found». Hay claves «community» (gratuitas) para apps open-source/source-available, ligadas al `AssemblyName`. Umbral Apache: **< 1 M USD** de ingresos brutos anuales, uso transitivo, OSS o sin ánimo de lucro | **Evitar como dependencia directa**. Si se usa: proyecto bajo licencia OSS + clave community, o ImageSharp vía dependencia transitiva. Alternativa: **SkiaSharp (MIT)** o **Magick.NET (Apache-2.0)** | https://www.nuget.org/packages/sixlabors.imagesharp ; https://docs.sixlabors.com/articles/imagesharp/ ; https://scancode-licensedb.aboutcode.org/six-labors-split-1.0.html ; https://github.com/SixLabors/ImageSharp/discussions/2151 ; https://github.com/SixLabors/ImageSharp/discussions/3129 |
| Telegram.Bot | 22.10.4 (2026-11-09) | MIT | No | **Usar** | https://www.nuget.org/packages/telegram.bot/ |
| Riok.Mapperly | 4.3.1 (2025-12-22); 5.0.0-next.13 en prerelease (2026-11-13) | Apache-2.0 | No | **Usar** (source generator; reemplaza AutoMapper) | https://www.nuget.org/packages/Riok.Mapperly/ |
| Serilog | 4.4.0 (2026-07-10) | Apache-2.0 | No | **Usar** | https://nuget.org/packages/Serilog |
| Serilog.AspNetCore | 10.0.0 (2025-11-28); 11.0.0-dev en prerelease | Apache-2.0 | No | **Usar** | https://www.nuget.org/packages/serilog.aspnetcore |
| Serilog.Sinks.Console | 6.1.1 (2025-11-02) | Apache-2.0 | No | **Usar** | https://nuget.org/profiles/serilog |
| OpenTelemetry | 1.20.1 (dependencia del exporter) | Apache-2.0 | No | **Usar** | https://www.nuget.org/packages/opentelemetry.exporter.opentelemetryprotocol/ |
| OpenTelemetry.Exporter.OpenTelemetryProtocol | 1.20.1 (2026-10-20); 1.21.0-rc.1 | Apache-2.0 | No | **Usar** | ídem |
| AWSSDK.S3 | 4.1.0 (2026-11-12) | Apache-2.0 | No | **Usar** | https://www.nuget.org/packages/AWSSDK.S3 |
| AWSSDK.SimpleEmailV2 | 4.1.0 (2026-11-12) | Apache-2.0 | No | **Usar** | https://feed.nuget.org/packages/AWSSDK.SimpleEmailV2 |
| Amazon.CDK.Lib | 2.277.0 (2026-11-13) | Apache-2.0 | No | **Usar** (versiones <2.235 tienen aviso de vulnerabilidad baja) | https://feed.nuget.org/packages/Amazon.CDK.Lib/2.234.0 |
| xunit.v3 | 4.0.3 (2026-11-09) | Apache-2.0 | No | **Usar** | https://www.nuget.org/packages/xunit.v3/ |
| Testcontainers.PostgreSql | 4.16.0 (2026-10-08) | MIT | No | **Usar** | https://www.nuget.org/packages/Testcontainers.PostgreSql |
| WireMock.Net | 2.31.0 (2026-11-12) | Apache-2.0 | No | **Usar** (mock de portales/HTTP) | https://nuget.org/packages/wiremock.net |
| Verify.Xunit / Verify.XunitV3 | 33.3.4 | MIT **+ "Maintenance Fee"**: el build falla (SC021) si no se declara sponsor o exención en MSBuild | **Sí (2026)** | **Evitar**: snapshots propios (comprobado al compilar el 2026-11-14) | https://www.nuget.org/packages/verify.xunit ; https://nuget.org/packages/Verify.XunitV3/ |
| TngTech.ArchUnitNET | 0.13.5 (2026-10-26) | Apache-2.0 | No | **Usar** | https://www.nuget.org/packages/TngTech.ArchUnitNET |
| NetArchTest.Rules | 1.3.2 (**2021-05-23**, sin actividad) | Sin SPDX en NuGet (repo: MIT) | No | **Evitar** (abandonado) → ArchUnitNET | https://www.nuget.org/packages/NetArchTest.Rules |
| CsCheck | 5.0.1 (2026-11-03) | Apache-2.0 | No | **Usar** (property testing en C# idiomático) | https://feed.nuget.org/packages/CsCheck/2.0.0 |
| FsCheck | 3.6.0 (2026-10-21) | BSD-3-Clause | No | Usar si se prefiere; con CsCheck basta uno | https://www.nuget.org/packages/FsCheck/2.9.0 ; https://socket.dev/nuget/package/fscheck/overview/3.4.0 |
| Microsoft.AspNetCore.Mvc.Testing | 10.0.15 / 11.0.0 (inferido: viaja con ASP.NET Core) | MIT | No | **Usar** | https://www.nuget.org/packages/Microsoft.AspNetCore.Mvc.Testing (inferido) |
| Dapper (opcional) | 2.1.89 (2026-09-23) | Apache-2.0 | No | Usar solo para consultas geoespaciales ad hoc | https://packages.nuget.org/packages/Dapper |
| MailKit | 5.0.0 (2026-11-10) | MIT | No | **Usar** para IMAP (alertas por e-mail) | https://www.nuget.org/packages/MailKit/5.0.0 |
| Brotli / System.IO.Compression | integrado en BCL (.NET 10) | MIT | No | **Usar** | https://dotnet.microsoft.com/ (BCL) |
| Microsoft.Extensions.TimeProvider.Testing | 10.x / 11.0.0 (2026-11-10) | MIT | No | **Usar** (FakeTimeProvider) | https://www.nuget.org/packages/Microsoft.Extensions.TimeProvider.Testing/ |
| Hangfire.Core (alternativa a Quartz) | 1.8.23 | LGPL-3.0 **o** EULA comercial (Pro/Ace de pago) | Sin cambio reciente, pero modelo dual desde siempre | **No necesario**: Quartz es Apache-2.0 sin tiers | https://www.hangfire.io/licenses.html ; https://www.nuget.org/packages/hangfire.core |

### 2.1 Pivotes comerciales confirmados (para no caer en ellos)

| Librería | Qué pasó | Fuente | Alternativa libre |
|---|---|---|---|
| **AutoMapper v15 / MediatR v13** | 2025-07-02: Lucky Penny Software (Jimmy Bogard) pasa a doble licencia RPL-1.5 + comercial; «Community» gratis si ingresos < 5 M USD pero **exige registrar clave de licencia**; tiers Standard/Professional/Enterprise. Versiones anteriores conservan Apache/MIT | https://www.jimmybogard.com/automapper-and-mediatr-commercial-editions-launch-today/ | **Riok.Mapperly** (mapeo) y **sin mediator** (handlers inyectados directamente / endpoints Minimal API) |
| **MassTransit v9** | Anuncio: v9 comercial (prerelease Q3-2025, GA Q1-2026, ~400 $/mes PYME, 1.200 $/mes enterprise); v8 sigue Apache-2.0 con parches de seguridad «durante 2026». Ahora gestionado por Massient, Inc. | https://milanjovanovic.tech/blog/mediatr-and-masstransit-going-commercial-what-this-means-for-you | No se necesita bus; si hiciera falta, Wolverine (MIT) o cliente nativo |
| **FluentAssertions v8** | 2025-01: v8 pasa a licencia propietaria (Xceed), 129,95 $/dev/año; v7 queda Apache-2.0 | https://www.infoq.com/news/2025/01/fluent-assertions-v8-license | **Shouldly** o **AwesomeAssertions** (fork Apache) o `Assert` de xunit.v3 |
| **ImageSharp 3.x/4.x** | Split License (2022) + clave de build obligatoria (2026) | ver tabla | SkiaSharp / Magick.NET / hash propio |
| **Moq 4.20.0** | 2023-08: SponsorLink leía el e-mail de git y lo enviaba hasheado a un endpoint; versión despublicada | https://snyk.io/blog/moq-package-exfiltrates-user-emails/ | **NSubstitute** (BSD-3) o FakeItEasy (MIT) |
| **Hangfire** | Core LGPL-3.0; Pro/Ace de pago | https://www.hangfire.io/licenses.html | **Quartz.NET** |

## 3. Riesgos

El riesgo principal de la pila no está en las licencias «clásicas» (todo lo central —ASP.NET Core, EF Core/Npgsql, Quartz, Polly, Serilog, OpenTelemetry, AWS SDK, xunit— es MIT/Apache/BSD/PostgreSQL sin tiers comerciales) sino en el **procesado de imágenes**: `CoenM.ImageSharp.ImageHash` lleva cuatro años sin releases y arrastra ImageSharp 2.x, y la línea actual de ImageSharp (4.1.3) impone desde 2026 una clave de licencia en tiempo de build incluso para quien cumple el umbral Apache (< 1 M USD), lo que rompe compilaciones Release sin aviso y ata el proyecto a un registro externo; la recomendación es calcular pHash/dHash sobre **SkiaSharp** (MIT) y no referenciar Six Labors directamente. Segundo riesgo: la tendencia 2025-2026 a «comercializar» librerías populares de .NET (AutoMapper, MediatR, MassTransit, FluentAssertions) —ninguna está en nuestra lista, y hay que mantenerlo así: Mapperly, sin mediator, Shouldly/AwesomeAssertions, NSubstitute. Tercero: **FluentValidation sigue Apache-2.0** en 12.2.0 pero el autor pide patrocinio a usuarios comerciales; conviene fijar la versión con rango `[12.*,13.0)` y revisar la licencia antes de cualquier salto de major. Cuarto: .NET 8 y 9 quedaron fuera de soporte el 2026-11-10, así que cualquier imagen base o plantilla CDK que aún los use debe migrar a .NET 10 (LTS hasta 2028-11-14). Finalmente, dos paquetes de la lista están **abandonados o deprecados** (`NetArchTest.Rules` 2021, `Verify.Xunit` legacy) y `Quartz.Extensions.Hosting` es un paquete vacío: sustituir por ArchUnitNET, Verify.XunitV3 y `Quartz` respectivamente. Se recomienda activar `<NuGetAudit>` y `dotnet list package --vulnerable --include-transitive` en CI, y un *lockfile* (`RestorePackagesWithLockFile`) para que los cambios de licencia no entren por una actualización automática.
