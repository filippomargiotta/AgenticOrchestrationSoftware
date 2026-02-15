#!/usr/bin/env bash
set -euo pipefail

repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

mode="full"
timestamp() {
  date +"%Y-%m-%d %H:%M:%S"
}

if [[ "${1:-}" == "--fast" ]]; then
  mode="fast"
elif [[ "${1:-}" == "--test" ]]; then
  mode="test"
elif [[ "${1:-}" != "" ]]; then
  echo "Usage: $(basename "$0") [--fast|--test]"
  exit 2
fi

cd "$repo_root"

if [[ "$mode" == "test" ]]; then
  echo "[$(timestamp)] Running: dotnet test $repo_root/source/Aos.sln -c Release"
  dotnet test "$repo_root/source/Aos.sln" -c Release
  exit 0
fi

echo "[$(timestamp)] Running: dotnet restore $repo_root/source/Aos.sln"
dotnet restore "$repo_root/source/Aos.sln"

if [[ "$mode" == "fast" ]]; then
  echo "[$(timestamp)] Running: dotnet build $repo_root/source/Aos.sln -c Release --no-restore"
  dotnet build "$repo_root/source/Aos.sln" -c Release --no-restore

  echo "[$(timestamp)] Running: dotnet test $repo_root/source/Aos.sln -c Release --no-build"
  dotnet test "$repo_root/source/Aos.sln" -c Release --no-build
  exit 0
fi

echo "[$(timestamp)] Running: dotnet build $repo_root/source/Aos.sln -c Release --no-restore"
dotnet build "$repo_root/source/Aos.sln" -c Release --no-restore

echo "[$(timestamp)] Running: dotnet test $repo_root/source/Aos.sln -c Release --no-build"
dotnet test "$repo_root/source/Aos.sln" -c Release --no-build
