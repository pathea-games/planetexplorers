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

# Build and capture all diagnostics
dotnet build "$CSPROJ" \
    -consoleloggerparameters:"NoSummary;ForceNoAlign" \
    -property:GenerateFullPaths=true \
    /flp:"logfile=$REPORT_FILE;warningsonly;errorsonly" \
    2>&1 | tee /tmp/analyzer-build-output.txt

BUILD_EXIT=$?

if [[ $BUILD_EXIT -ne 0 ]]; then
    echo ""
    echo "WARNING: Build exited with code $BUILD_EXIT (compilation errors present)"
    echo "Analyzer diagnostics may be incomplete due to compilation failures."
    echo ""
fi

if [[ ! -f "$REPORT_FILE" ]]; then
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
    ["Code Quality (CA1xxx)"]="CA1[0-9]"
    ["Performance (CA18xx)"]="CA18"
    ["Security (CA2xxx)"]="CA2[0-9]"
    ["Maintainability (CA15xx)"]="CA15"
    ["Unity Correctness (UNTxxxx)"]="UNT[0-9]"
    ["Dead Code (RCS1xxx)"]="RCS1"
    ["Roslynator Style (RCS0xxx)"]="RCS0"
    ["Compiler Warnings (CS)"]="CS[0-9]"
)

TOTAL=0
{
    echo "| Category | Count | Sample Rule |"
    echo "|----------|-------|-------------|"

    for category in "Code Quality (CA1xxx)" "Performance (CA18xx)" "Security (CA2xxx)" "Maintainability (CA15xx)" "Unity Correctness (UNTxxxx)" "Dead Code (RCS1xxx)" "Roslynator Style (RCS0xxx)" "Compiler Warnings (CS)"; do
        pattern="${CATEGORIES[$category]}"
        count=$(grep -cE "$pattern" "$REPORT_FILE" 2>/dev/null || echo "0")
        sample=$(grep -oE "${pattern}[0-9]*" "$REPORT_FILE" 2>/dev/null | sort | uniq -c | sort -rn | head -1 | awk '{print $2}' || echo "—")
        TOTAL=$((TOTAL + count))
        if [[ "$count" -gt 0 ]]; then
            echo "| $category | $count | $sample |"
        fi
    done
} | tee "$SUMMARY_FILE"

echo ""
echo "**Total: $TOTAL diagnostics**"
echo ""
echo "Full report: $REPORT_FILE"
echo "Summary: $SUMMARY_FILE"

# Also produce a per-subsystem breakdown
echo ""
echo "=== Per-Subsystem Breakdown ==="
echo ""
echo "| Subsystem | Count |" | tee -a "$SUMMARY_FILE"
echo "|-----------|-------|" | tee -a "$SUMMARY_FILE"

for dir in "$PROJECT_ROOT"/Assets/Scripts/*/; do
    subsystem=$(basename "$dir")
    count=$(grep -c "Assets/Scripts/$subsystem/" "$REPORT_FILE" 2>/dev/null || echo "0")
    if [[ "$count" -gt 0 ]]; then
        echo "| $subsystem | $count |" | tee -a "$SUMMARY_FILE"
    fi
done
