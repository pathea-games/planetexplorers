#!/usr/bin/env bash
set -euo pipefail

# Copies Unity managed DLLs needed for Roslyn analyzer type resolution.
# Run once locally; output is committed to analyzers/unity-stubs/.

UNITY_MANAGED="/home/esfisher/Unity/Hub/Editor/6000.3.12f1/Editor/Data/Managed"
DEST_DIR="$(cd "$(dirname "$0")/.." && pwd)/analyzers/unity-stubs"

if [[ ! -d "$UNITY_MANAGED" ]]; then
    echo "ERROR: Unity managed directory not found at $UNITY_MANAGED"
    echo "Update UNITY_MANAGED in this script to match your Unity installation."
    exit 1
fi

rm -rf "$DEST_DIR"
mkdir -p "$DEST_DIR"

# Core facade DLL
cp "$UNITY_MANAGED/UnityEngine.dll" "$DEST_DIR/"
cp "$UNITY_MANAGED/UnityEditor.dll" "$DEST_DIR/"

# Module DLLs (Unity 6 splits UnityEngine into modules)
for dll in "$UNITY_MANAGED/UnityEngine"/UnityEngine.*.dll; do
    cp "$dll" "$DEST_DIR/"
done

echo "Copied $(ls "$DEST_DIR"/*.dll | wc -l) DLLs to $DEST_DIR"
