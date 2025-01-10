#!/bin/bash

DB_USER=$1
DB_PASS=$2
DB_NAME=$3

RESULT=$(psql -U $DB_USER -tc "SELECT 1 FROM pg_database WHERE datname = '$DB_NAME'")
if [ -z "$RESULT" ]; then
  echo "Database does not exist. Creating..."
  PGPASSWORD=$DB_PASS psql -U $DB_USER -c "CREATE DATABASE $DB_NAME"
else
  echo "Database already exists. Skipping creation."
fi
