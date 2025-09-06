#!/usr/bin/env bash
set -euo pipefail

# generate-token.sh [SIGNING_KEY] [USER_ID]
# If SIGNING_KEY is omitted, uses $JWT_SIGNING_KEY env var.
# Outputs token to stdout and copies to clipboard if pbcopy/xclip is available.

SIGNING_KEY="${1:-${JWT_SIGNING_KEY:-}}"
USER_ID="${2:-00000000-0000-0000-0000-000000000123}"

if [ -z "$SIGNING_KEY" ]; then
  echo "Error: signing key not provided. Pass as first arg or set JWT_SIGNING_KEY env var." >&2
  exit 1
fi

# prefer openssl HMAC if available
if command -v openssl >/dev/null 2>&1; then
  SIG=$(printf '%s' "$USER_ID" | openssl dgst -sha256 -mac HMAC -macopt "key:$SIGNING_KEY" -binary | base64)
else
  # fallback to python
  SIG=$(python3 - <<PY
import sys, hmac, hashlib, base64
user = b"%s"
key = b"%s"
print(base64.b64encode(hmac.new(key, user, digestmod=hashlib.sha256).digest()).decode())
PY
)
fi

token="${USER_ID}.${SIG}"
printf '%s\n' "$token"

# copy to clipboard if possible
if command -v pbcopy >/dev/null 2>&1; then
  printf '%s' "$token" | pbcopy
elif command -v xclip >/dev/null 2>&1; then
  printf '%s' "$token" | xclip -selection clipboard
fi
