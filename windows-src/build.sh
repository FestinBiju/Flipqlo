#!/usr/bin/env bash
# Build and package the Windows screensaver.
# Run from the repository root: bash windows-src/build.sh
#
# Requires: .NET SDK (dotnet)
# Output:   windows-src/FliqloScr/bin/Release/net10.0-windows/win-x64/publish/FliqloScr.scr

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
PROJECT="$SCRIPT_DIR/FliqloScr/FliqloScr.csproj"

echo "==> Publishing FliqloScr (net10.0-windows, win-x64, self-contained single-file)..."
dotnet publish "$PROJECT" -c Release -f net10.0-windows -r win-x64 --self-contained true \
    -p:PublishSingleFile=true \
    -p:IncludeNativeLibrariesForSelfExtract=true \
    -p:EnableCompressionInSingleFile=true

# .scr is just a renamed .exe
OUT_DIR="$SCRIPT_DIR/FliqloScr/bin/Release/net10.0-windows/win-x64/publish"
EXE="$OUT_DIR/FliqloScr.exe"
SCR="$OUT_DIR/FliqloScr.scr"

if [ -f "$EXE" ]; then
    cp "$EXE" "$SCR"
    echo "==> Screensaver built: $SCR"
    echo ""
    echo "To install:"
    echo "  1. Copy FliqloScr.scr to C:\\Windows\\System32\\"
    echo "  2. Right-click desktop → Personalize → Lock screen → Screen saver settings"
    echo "  3. Select 'FliqloScr' from the dropdown"
    echo ""
    echo "To test:"
    echo "  Full screen:  $SCR /s"
    echo "  Config:       $SCR /c"
else
    echo "ERROR: Build output not found at $EXE"
    exit 1
fi
