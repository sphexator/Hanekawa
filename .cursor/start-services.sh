#!/usr/bin/env bash
# Per-boot startup for the Hanekawa Cloud Agent environment.
# Idempotently starts PostgreSQL and Redis and ensures the development
# database/credentials the bot expects exist. Safe to run on every boot.
set -euo pipefail

DB_NAME="hanekawa-development"
DB_PASSWORD="1023"

echo "==> Starting PostgreSQL"
sudo service postgresql start

echo "==> Waiting for PostgreSQL to accept connections"
for _ in $(seq 1 30); do
    if sudo -u postgres pg_isready -q; then
        break
    fi
    sleep 1
done
sudo -u postgres pg_isready

echo "==> Ensuring 'postgres' role password"
sudo -u postgres psql -c "ALTER USER postgres PASSWORD '${DB_PASSWORD}';"

echo "==> Ensuring database '${DB_NAME}' exists"
if ! sudo -u postgres psql -tAc "SELECT 1 FROM pg_database WHERE datname = '${DB_NAME}'" | grep -q 1; then
    sudo -u postgres psql -c "CREATE DATABASE \"${DB_NAME}\";"
fi

echo "==> Starting Redis"
sudo service redis-server start

echo "==> Waiting for Redis to respond"
for _ in $(seq 1 30); do
    if redis-cli ping >/dev/null 2>&1; then
        break
    fi
    sleep 1
done
redis-cli ping

echo "==> Local services are ready (PostgreSQL:5432, Redis:6379)"
