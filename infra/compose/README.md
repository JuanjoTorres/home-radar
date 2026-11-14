# Host deployment (Docker Compose)

Target: one Linux VPS (ADR-0002) running three containers: `postgis`, `api`, `worker`.

1. Install Docker Engine + Compose plugin; create `/opt/homeradar` and copy this folder plus `config/seed/*.yaml` (the deploy workflow does it).
2. Copy `.env.example` to `.env` and fill `POSTGRES_PASSWORD`, `HomeRadar__ApiKey`, `Telegram__BotToken`, `Telegram__ChatId`, the AWS keys of the `homeradar-host` IAM user (created by the CDK stack) and `Aws__BackupBucket`.
3. Put personal seeds in `/opt/homeradar/config/seed/*.local.yaml` (never committed).
4. `docker compose up -d` — the worker applies migrations and seeds on start; the API listens on `127.0.0.1:8080`. Put Caddy or nginx in front for TLS if the API must be reachable from outside; otherwise use an SSH tunnel.
5. Backups: `crontab` the `backup.sh` script (nightly `pg_dump` to S3, lifecycle rules move old dumps to cheaper classes).
6. Updates: push a tag `vX.Y.Z` — the `deploy` workflow builds multi-arch images, pushes them to GHCR and restarts the stack over SSH.
