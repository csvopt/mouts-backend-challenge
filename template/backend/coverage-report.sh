#!/bin/bash

set -euo pipefail

echo "Restoring solution"
dotnet restore Ambev.DeveloperEvaluation.sln

echo "Run tests with coverage"
dotnet test Ambev.DeveloperEvaluation.sln \
  --no-restore \
  --configuration Release \
  --collect:"XPlat Code Coverage" \
  --results-directory ./TestResults

echo ""
echo "Cobertura coverage files generated under TestResults/"
