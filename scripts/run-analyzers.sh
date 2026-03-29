#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
CSPROJ="$PROJECT_ROOT/analyzers/PlanetExplorers.Analyzers.csproj"
REPORT_FILE="$PROJECT_ROOT/analyzer-report.txt"
SUMMARY_FILE="$PROJECT_ROOT/analyzer-summary.txt"

echo "=== Planet Explorers Rebirth: Roslyn Analyzer Run ==="
echo "Project: $CSPROJ"
echo ""

# Clean first to force full re-analysis (incremental builds skip unchanged files)
dotnet clean "$CSPROJ" -v q 2>/dev/null || true

# Build and capture all output
BUILD_OUTPUT=$(dotnet build "$CSPROJ" \
    --no-incremental \
    -property:GenerateFullPaths=true \
    2>&1) || true

echo "$BUILD_OUTPUT"

# Extract warning lines into report file
echo "$BUILD_OUTPUT" | grep ": warning " > "$REPORT_FILE" || true

WARN_COUNT=$(wc -l < "$REPORT_FILE")

if [[ "$WARN_COUNT" -eq 0 ]]; then
    echo ""
    echo "No diagnostics found."
    echo "0 total diagnostics" > "$SUMMARY_FILE"
    exit 0
fi

echo ""
echo "=== Diagnostic Summary ==="
echo ""

# Count by category
declare -A CATEGORIES
CATEGORIES=(
    ["Code Quality (CA1xxx)"]="CA1[0-7]"
    ["Performance (CA18xx)"]="CA18"
    ["Security (CA2xxx)"]="CA2[0-9]"
    ["Maintainability (CA15xx)"]="CA15"
    ["Unity Correctness (UNTxxxx)"]="UNT[0-9]"
    ["Dead Code (RCS1xxx)"]="RCS1"
    ["Roslynator Style (RCS0xxx)"]="RCS0"
    ["Compiler Warnings (CS)"]="CS[0-9]"
)

TOTAL=0
: > "$SUMMARY_FILE"

echo "| Category | Count | Sample Rule |" | tee -a "$SUMMARY_FILE"
echo "|----------|-------|-------------|" | tee -a "$SUMMARY_FILE"

for category in "Code Quality (CA1xxx)" "Performance (CA18xx)" "Security (CA2xxx)" "Maintainability (CA15xx)" "Unity Correctness (UNTxxxx)" "Dead Code (RCS1xxx)" "Roslynator Style (RCS0xxx)" "Compiler Warnings (CS)"; do
    pattern="${CATEGORIES[$category]}"
    count=$(grep -cE "$pattern" "$REPORT_FILE" 2>/dev/null || true)
    count=${count:-0}
    sample=$(grep -oE "${pattern}[0-9]*" "$REPORT_FILE" 2>/dev/null | sort | uniq -c | sort -rn | head -1 | awk '{print $2}' || true)
    sample=${sample:-—}
    TOTAL=$((TOTAL + count))
    if [[ "$count" -gt 0 ]]; then
        echo "| $category | $count | $sample |" | tee -a "$SUMMARY_FILE"
    fi
done

echo "" | tee -a "$SUMMARY_FILE"
echo "**Total: $TOTAL diagnostics**" | tee -a "$SUMMARY_FILE"
echo ""
echo "Full report: $REPORT_FILE"
echo "Summary: $SUMMARY_FILE"

# Per-subsystem breakdown
echo "" | tee -a "$SUMMARY_FILE"
echo "=== Per-Subsystem Breakdown ===" | tee -a "$SUMMARY_FILE"
echo "" | tee -a "$SUMMARY_FILE"
echo "| Subsystem | Count |" | tee -a "$SUMMARY_FILE"
echo "|-----------|-------|" | tee -a "$SUMMARY_FILE"

for dir in "$PROJECT_ROOT"/Assets/Scripts/*/; do
    subsystem=$(basename "$dir")
    count=$(grep -c "Assets/Scripts/$subsystem/" "$REPORT_FILE" 2>/dev/null || true)
    count=${count:-0}
    if [[ "$count" -gt 0 ]]; then
        echo "| $subsystem | $count |" | tee -a "$SUMMARY_FILE"
    fi
done
