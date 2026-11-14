#!/usr/bin/env bash
# Nightly PostgreSQL dump to S3. Install with: (crontab -l; echo "15 3 * * * /opt/homeradar/infra/compose/backup.sh") | crontab -
set -euo pipefail
cd "$(dirname "$0")"
set -a; source .env; set +a
STAMP=$(date -u +%Y-%m-%dT%H%MZ)
FILE="/tmp/homeradar-${STAMP}.dump"
docker compose exec -T postgis pg_dump -U "${POSTGRES_USER:-homeradar}" -Fc "${POSTGRES_DB:-homeradar}" > "${FILE}"
docker run --rm -v /tmp:/tmp -e AWS_ACCESS_KEY_ID -e AWS_SECRET_ACCESS_KEY -e AWS_DEFAULT_REGION="${Aws__Region:-eu-west-1}" \
  public.ecr.aws/aws-cli/aws-cli:latest s3 cp "${FILE}" "s3://${Aws__BackupBucket}/backups/$(basename "${FILE}")" --storage-class STANDARD
rm -f "${FILE}"
