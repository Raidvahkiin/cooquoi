#!/usr/bin/env bash
# One-time dev machine setup: trust the .NET dev certificate and export it as PEM
# (ADR 0018). Idempotent: safe to run again.
set -euo pipefail

PEM_DIR="${HOME}/.aspnet/dev-certs/https"
PEM_PATH="${PEM_DIR}/aspnetcore-dev.pem"

# On Linux --trust exits non-zero (110) until SSL_CERT_DIR lists the trust folder: keep going.
dotnet dev-certs https --check --trust || dotnet dev-certs https --trust \
  || echo "dev-certs could not fully trust the certificate; see the notes below."

mkdir -p "${PEM_DIR}"
dotnet dev-certs https --export-path "${PEM_PATH}" --format Pem --no-password

echo "Dev certificate PEM: ${PEM_PATH}"

if [[ "$(uname -s)" == "Linux" ]]; then
  cat <<MSG

Linux: 'dotnet dev-certs' trusts the certificate for .NET only. Chromium and Firefox
use the NSS store; trust the certificate there once (needs 'certutil', package
libnss3-tools / nss-tools):

  export SSL_CERT_DIR="\$HOME/.aspnet/dev-certs/trust:/usr/lib/ssl/certs"   # OpenSSL (curl, Node tools), add to your shell profile

  certutil -d sql:\$HOME/.pki/nssdb -A -t "C,," -n cooquoi-dev -i "${PEM_PATH}"

Firefox: Settings > Privacy & Security > Certificates > View Certificates > Authorities
> Import, then pick the PEM above. Node is covered by NODE_EXTRA_CA_CERTS (set by the AppHost).
MSG
fi
