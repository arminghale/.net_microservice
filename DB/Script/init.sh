#!/bin/bash
set -e

IFS=',' read -ra DBS <<< "$ALL_DATABASES"

for db in "${DBS[@]}"; do
  echo "Creating database: $db"
  psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" <<-EOSQL
    CREATE DATABASE "$db";
EOSQL
    found_sql_file=""
    echo "Searching for SQL initialization file with pattern: /SQL/*${db}.sql"
    for potential_file in /SQL/*"${db}".sql; do
    if [ -f "$potential_file" ]; then
        echo "Found matching SQL file: $potential_file"
        found_sql_file="$potential_file"
        break
    fi
    done
    if [ -f "$found_sql_file" ]; then
    echo "Initializing database '$db' with $found_sql_file"
    psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname="$db" -f "$found_sql_file"
    else
    echo "No SQL file found for database '$db'"
    fi
done