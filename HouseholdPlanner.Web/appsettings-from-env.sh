#!/bin/sh
# Runs when the container starts (nginx runs everything in /docker-entrypoint.d/ first).
# Writes the app's appsettings.json from API_URL, so the API address lives in .env.
# Leave API_URL empty when the API is on the app's own domain under /api/: the app then calls that.
set -eu

file=/usr/share/nginx/html/appsettings.json
url="${API_URL:-}"

# Without the trailing slash, the last part of a path like https://example.com/hearth would be dropped.
case "$url" in
    "" | */) ;;
    *) url="$url/" ;;
esac

# An older compose file mounts its own appsettings.json read-only. That one wins. (test -w can't tell:
# as root it says yes on a read-only mount.)
if ! printf '{\n  "ApiBaseUrl": "%s"\n}\n' "$url" 2>/dev/null > "$file"; then
    echo "$0: $file is read-only, leaving it as is"
    exit 0
fi
# The compressed copies from publish still hold the old file.
rm -f "$file.gz" "$file.br"
echo "$0: ApiBaseUrl is ${url:-the app's own domain}"
