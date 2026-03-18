#!/bin/bash
set -euo pipefail

# Schema Drift Validation Script
#
# Validates that the deployed Oracle schema matches the EF Core model.
# Requires: ConnectionStrings__OracleDb environment variable pointing to the target database.
#
# Usage:
#   export ConnectionStrings__OracleDb="User Id=MEMBER_APP;Password=...;Data Source=host:1521/FREEPDB1"
#   bash tools/schema-validation/validate-schema.sh

if [ -z "${ConnectionStrings__OracleDb:-}" ]; then
    echo "ERROR: ConnectionStrings__OracleDb environment variable is not set."
    echo "Set it to the Oracle connection string for the target environment."
    exit 1
fi

PROJECT_PATH="src/DockerNetOci.Api"

echo "Building project..."
dotnet build "$PROJECT_PATH" -c Release --nologo -q

echo "Generating schema validation migration..."
dotnet ef migrations add SchemaValidation --project "$PROJECT_PATH" --no-build

# Find the generated migration file (exclude Designer and snapshot files)
MIGRATION_FILE=$(find "$PROJECT_PATH/Migrations" -name "*_SchemaValidation.cs" ! -name "*Designer*" | head -1)

if [ -z "$MIGRATION_FILE" ]; then
    echo "ERROR: Migration file not found."
    dotnet ef migrations remove --project "$PROJECT_PATH" --force --no-build
    exit 1
fi

echo "Checking migration for schema drift..."

# Check if the Up method contains any migrationBuilder calls (indicating drift)
if grep -q "migrationBuilder\." "$MIGRATION_FILE"; then
    echo ""
    echo "SCHEMA DRIFT DETECTED"
    echo "====================="
    echo "The deployed database schema does not match the EF Core model."
    echo "Review the generated migration:"
    echo ""
    cat "$MIGRATION_FILE"
    echo ""

    dotnet ef migrations remove --project "$PROJECT_PATH" --force --no-build
    exit 1
else
    echo "No schema drift detected. Database schema matches the EF Core model."
    dotnet ef migrations remove --project "$PROJECT_PATH" --force --no-build
    exit 0
fi
