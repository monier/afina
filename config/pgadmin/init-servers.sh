#!/bin/bash
# This script initializes pgAdmin with the database server connection
# It runs after pgAdmin starts and uses environment variables from docker-compose

set -e

# Wait for pgAdmin to be ready (polling the API)
MAX_RETRIES=30
RETRY_COUNT=0

echo "⏳ Waiting for pgAdmin to be ready..."
while [ $RETRY_COUNT -lt $MAX_RETRIES ]; do
  if curl -s http://localhost:5050/misc/ping 2>/dev/null | grep -q "pong"; then
    echo "✅ pgAdmin is ready"
    break
  fi
  RETRY_COUNT=$((RETRY_COUNT + 1))
  sleep 2
done

if [ $RETRY_COUNT -eq $MAX_RETRIES ]; then
  echo "⚠️  pgAdmin took too long to start, skipping server configuration"
  exit 0
fi

# Create servers.json with environment variables
cat > /var/lib/pgadmin/servers.json <<EOF
{
  "Servers": {
    "1": {
      "Name": "Afina Database",
      "Group": "Development",
      "Port": ${DB_PORT:-5432},
      "Username": "${DB_USER:-postgres}",
      "Host": "${DB_HOST:-postgres}",
      "SSLMode": "prefer",
      "MaintenanceDB": "${DB_NAME:-postgres}",
      "Shared": false
    }
  }
}
EOF

echo "✅ Database server configured in pgAdmin"
echo "   Host: ${DB_HOST:-postgres}"
echo "   Database: ${DB_NAME:-postgres}"
echo "   User: ${DB_USER:-postgres}"
echo "   Port: ${DB_PORT:-5432}"
