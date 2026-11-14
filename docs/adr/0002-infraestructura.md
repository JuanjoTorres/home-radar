# ADR-0002 — Infraestructura: VPS europeo + servicios de pago por uso en AWS

- **Estado:** aceptado (fase 0, 2026-11-14). Precios verificados en `docs/infra/costes-2026-11.md` (cambio BCE 1 € = 1,1081 $, 2026-11-12).
- **Objetivo de coste:** < 15 €/mes con IVA.

## Comparativa de la máquina siempre encendida (≥ 4 GB RAM, UE)

| Opción | Spec | €/mes sin IVA | €/mes con IVA 21 % | Notas |
|---|---|---|---|---|
| **Hetzner CAX11 (ARM64)** | 2 vCPU Ampere / 4 GB / 40 GB | 6,49 (IPv4 incl.) | **7,85** | Facturación horaria, API, snapshots, backups +20 %. Tras las dos subidas de 2026. |
| Hetzner CX23 (x86) | 2 vCPU / 4 GB / 40 GB | 5,99 | 7,25 | Misma gama; ARM subió menos en términos relativos pero hoy CX23 es algo más barato. |
| netcup VPS 500 ARM | 4 vCPU / 8 GB / 256 GB | 7,98 | 9,66 | Plan B si CX/CAX siguen "unavailable". |
| OVHcloud VPS-1 | 2 vCore / 4 GB / 40 GB | 3,81 (anual) – 6,49 | 4,61 – 7,85 | Precio bajo solo con compromiso anual. |
| Contabo VPS 10 | 4 vCPU / 8 GB | 4,50 | 5,45 | Reputación de rendimiento irregular; sin API decente. |
| AWS Lightsail 4 GB | 2 vCPU / 4 GB / 80 GB | ≈ 21,7 (24 $) | ≈ 26,2 | Supera el presupuesto él solo. |
| AWS EC2 t4g.medium | 2 vCPU Graviton / 4 GB + gp3 40 GB + IPv4 | ≈ 21,8 (RI 1 año) / 30,7 (on-demand) | ≈ 26–37 | Descartado por coste. |
| AWS RDS db.t4g.micro | 2 vCPU / 1 GiB | ≈ 11,2 solo instancia | ≈ 13,5 | Descartado: 1 GiB y más caro que la máquina entera. |

## Decisión
1. **Cómputo:** Hetzner Cloud **CAX11** (ARM64) en Falkenstein, Ubuntu 24.04 LTS, Docker Compose con tres contenedores: `api`, `worker`, `postgis`. **Verificado el 2026-11-14: los ocho planes CX/CAX siguen "currently unavailable" en FSN/NBG/HEL**, así que el plan operativo al contratar es, por orden: CAX11/CX23 si vuelven a estar disponibles → **OVH VPS-1** (≈ 4,61–7,85 € con IVA según compromiso) → **netcup VPS 500 ARM G12.5** (9,66 € mensual, 7,14 € a 24 meses). La arquitectura no cambia (imágenes multi-arch).
2. **Snapshots crudos en Postgres, no en S3** (ver ADR-0003): comprimidos con Brotli en `bytea`. Motivo verificado: en S3 el coste lo dominan las peticiones PUT (200 k PUT/mes ≈ 1 $) y las clases IA/Glacier penalizan objetos pequeños (mínimo 128 KB y 30–180 días). Solo van a S3 los **backups nocturnos** (`pg_dump` + `pg_dumpall --globals`) y las **fotos** (pocas, solo las que cambian).
3. **AWS (eu-west-1, Irlanda):** S3 con lifecycle (Standard → Standard-IA a 30 días → Glacier Deep Archive a 180 días, solo para backups), SES (Essentials, 0,16 $/1.000; ya no hay free tier) opcional, AWS Budgets (gratis) con alerta a 10 $ y 15 $. Sin NAT Gateway, sin Secrets Manager, sin CloudWatch Logs.
4. **IAM:** un usuario con política mínima (`s3:PutObject/GetObject/ListBucket` sobre el bucket y `ses:SendEmail`), credenciales en variables de entorno del host (no en el repo).
5. **IaC:** AWS CDK en C# (`infra/cdk`) para bucket, lifecycle, IAM y Budgets; Compose para la máquina; `cloud-init` mínimo documentado.
6. **Sin Lambda para los fetchers:** sondeo continuo, Playwright pesado, obligaría a exponer Postgres o añadir colas; no ahorra nada frente a una máquina de 8 €.
7. **Backups:** Hetzner Backups (+20 %, ≈ 1,3 €) para la máquina completa **y** `pg_dump` a S3 (independencia del proveedor).

## Estimación mensual (con IVA)
| Partida | € / mes |
|---|---|
| CAX11 | 7,85 |
| Hetzner Backups (20 %) | 1,57 |
| S3 (≈ 20 GB creciendo 2 GB/mes, pocos PUT) | ≈ 0,6 |
| SES (≈ 60 emails/mes) | ≈ 0,02 |
| Dominio (opcional, prorrateado) | ≈ 1,0 |
| Telegram, Budgets, Let's Encrypt | 0 |
| **Total** | **≈ 11 €** |

## Riesgos
- Hetzner ha subido precios dos veces en 2026 y los planes CX/CAX han estado "currently unavailable" en varias ubicaciones durante semanas; de ahí el plan B.
- SES exige salir del sandbox (verificación de dominio) aunque solo te escribas a ti.
- Reconsiderar RDS únicamente si el sistema pasa a ser multiusuario o si el tamaño de la base supera lo que cabe cómodamente en 40 GB (ampliar volumen antes que migrar).
