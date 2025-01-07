#!/bin/bash

DB_NAME="Identity"
DB_USER="admin"
DB_PASS="GIGA258"

RESULT=$(psql -U $DB_USER -tc "SELECT 1 FROM pg_database WHERE datname = '$DB_NAME'")
if [ -z "$RESULT" ]; then
  echo "Database does not exist. Creating..."
  PGPASSWORD=$DB_PASS psql -U $DB_USER -c "CREATE DATABASE $DB_NAME"
else
  echo "Database already exists. Skipping creation."
fi
