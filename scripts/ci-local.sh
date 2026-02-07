#!/usr/bin/env bash
set -euo pipefail

repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

cd "$repo_root"

echo "Running: dotnet restore $repo_root/source/Aos.sln"
dotnet restore "$repo_root/source/Aos.sln"

echo "Running: dotnet build $repo_root/source/Aos.sln -c Release --no-restore"
dotnet build "$repo_root/source/Aos.sln" -c Release --no-restore

echo "Running: dotnet test $repo_root/source/Aos.sln -c Release --no-build"
dotnet test "$repo_root/source/Aos.sln" -c Release --no-build
