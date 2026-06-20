#!/usr/bin/env bash
# Exports Bountyscape for macOS, Windows, and Linux using Godot's headless CLI.
#
# Prerequisites:
#   - Export templates installed (Editor → Export → Manage Export Templates)
#   - Windows and Linux presets added in export_presets.cfg via the Godot editor
#
# Usage:
#   ./build.sh              # builds all configured platforms
#   ./build.sh mac          # builds only macOS
#   ./build.sh mac windows  # builds specific platforms
#
# Override the Godot binary location:
#   GODOT_PATH=/path/to/Godot ./build.sh

set -euo pipefail

GODOT="${GODOT_PATH:-/Applications/Godot_mono.app/Contents/MacOS/Godot}"
PROJECT_DIR="$(cd "$(dirname "$0")" && pwd)"
BUILD_DIR="$(dirname "$PROJECT_DIR")/bountyscape_builds"

# ── Sanity checks ────────────────────────────────────────────────────────────

if [ ! -f "$GODOT" ]; then
    echo "Error: Godot binary not found at '$GODOT'"
    echo "Set GODOT_PATH to point at the Godot CLI binary and try again."
    exit 1
fi

# ── Helper ───────────────────────────────────────────────────────────────────

has_preset() {
    grep -q "name=\"$1\"" "$PROJECT_DIR/export_presets.cfg"
}

build_platform() {
    local label="$1"   # human name for output
    local preset="$2"  # must match name= in export_presets.cfg
    local output="$3"  # full path to the output file/bundle

    if ! has_preset "$preset"; then
        echo "⚠  $label: preset '$preset' not found in export_presets.cfg — skipping."
        echo "   Add it via the Godot editor: Project → Export → Add → $label"
        return
    fi

    mkdir -p "$(dirname "$output")"
    echo "→  $label..."
    "$GODOT" --headless --path "$PROJECT_DIR" --export-release "$preset" "$output"
    echo "   ✓  $output"
}

# ── Platform definitions ─────────────────────────────────────────────────────

build_mac()     { build_platform "macOS"   "macOS"           "$BUILD_DIR/mac/Bountyscape.app"; }
build_windows() { build_platform "Windows" "Windows Desktop" "$BUILD_DIR/windows/Bountyscape.exe"; }
build_linux()   { build_platform "Linux"   "Linux/X11"       "$BUILD_DIR/linux/Bountyscape.x86_64"; }

# ── Entry point ──────────────────────────────────────────────────────────────

echo "Bountyscape build — $(date '+%Y-%m-%d %H:%M:%S')"
echo "Project : $PROJECT_DIR"
echo "Output  : $BUILD_DIR"
echo "Godot   : $GODOT"
echo ""

# If args were given, build only those platforms; otherwise build all.
if [ $# -eq 0 ]; then
    build_mac
    build_windows
    build_linux
else
    for target in "$@"; do
        case "$target" in
            mac|macos)   build_mac ;;
            win|windows) build_windows ;;
            lin|linux)   build_linux ;;
            *)
                echo "Unknown platform '$target'. Valid options: mac, windows, linux"
                exit 1
                ;;
        esac
    done
fi

echo ""
echo "Done."
