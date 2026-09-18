#!/usr/bin/env bash
# Prove nuvyn init produces a buildable host on the smallest Nuvyntra set,
# and that nuvyn update refreshes skills without overlaying host code.
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
WORKDIR="${NUVYN_PROVE_DIR:-$(mktemp -d)}"
WORKDIR="$(mkdir -p "$WORKDIR" && cd "$WORKDIR" && pwd -P)"
KEEP="${NUVYN_PROVE_KEEP:-}"

cleanup() {
  if [[ -z "$KEEP" ]]; then
    rm -rf "$WORKDIR"
  else
    echo "Kept prove tree at $WORKDIR"
  fi
}
trap cleanup EXIT

mkdir -p "$WORKDIR"
cd "$WORKDIR"

CLI=(dotnet run --project "$ROOT/src/NuvyntraLabs.Nuvyn.Cli/NuvyntraLabs.Nuvyn.Cli.csproj" -c Release --no-launch-profile --)

echo "==> nuvyn init ProveHost"
"${CLI[@]}" init ProveHost --agent cursor

APP="$WORKDIR/ProveHost"
test -f "$APP/.nuvyn/constitution.md"
test -f "$APP/.nuvyn/reference/constraints.md"
test -f "$APP/.cursor/skills/nuvyn-implement/SKILL.md"
test -f "$APP/ProveHost/MauiProgram.cs"
test -f "$APP/ProveHost/Pages/MainPage.xaml"

echo "==> smallest package set"
for pkg in Plugin.Maui.MVVMExpress NuvyntraLabs.UIKit Plugin.Maui.HttpForge Plugin.Maui.FormValidation Plugin.Maui.KeyboardManager; do
  grep -F "$pkg" "$APP/ProveHost/ProveHost.csproj" >/dev/null
done

echo "==> no catalog dump"
for pkg in Plugin.Maui.LocalStore Nuventra.NuvexaDB Plugin.Maui.AppLock Plugin.Maui.Observability Plugin.Maui.JobQueue Plugin.Maui.OfflineSync; do
  if grep -F "$pkg" "$APP/ProveHost/ProveHost.csproj" >/dev/null; then
    echo "::error::$pkg must not be on the default host"
    exit 1
  fi
done

echo "==> MauiProgram UseX + DI"
for call in UseMvvmExpress UseNuvyntraUIKit UseHttpForge UseMauiFormValidation UseKeyboardManager \
            'AddTransient<MainPageViewModel>()' 'AddTransient<MainPage>()'; do
  grep -F "$call" "$APP/ProveHost/MauiProgram.cs" >/dev/null
done
if grep -F 'AddGeneratedViewModels()' "$APP/ProveHost/MauiProgram.cs" >/dev/null; then
  echo "::error::MauiProgram must not call AddGeneratedViewModels()"
  exit 1
fi

# Restoring the .sln (or the MAUI project with -p:TargetFrameworks=net10.0-android)
# rewrites ProveHost.Core/obj/project.assets.json to android-only. Hide the sln
# and wipe obj/bin so Core + Tests restore as net10.0 only.
echo "==> isolate Core + Tests from the MAUI sln"
SLN="$APP/ProveHost.sln"
if [[ -f "$SLN" ]]; then
  mv "$SLN" "$SLN.hidden"
fi
find "$APP" -type d \( -name obj -o -name bin \) -prune -exec rm -rf {} +

echo "==> build Core + Tests (MVVMExpress)"
dotnet test "$APP/ProveHost.Tests/ProveHost.Tests.csproj" \
  -f net10.0 \
  -p:TargetFramework=net10.0 \
  --nologo

if [[ -f "$SLN.hidden" ]]; then
  mv "$SLN.hidden" "$SLN"
fi

if [[ "${NUVYN_PROVE_MAUI:-}" == "1" ]]; then
  echo "==> build MAUI android host"
  # Ubuntu CI installs maui-android only. Do this after Core/Tests so a leaked
  # android TargetFrameworks property cannot rewrite their assets.
  dotnet build "$APP/ProveHost/ProveHost.csproj" \
    -f net10.0-android \
    -p:TargetFrameworks=net10.0-android \
    --nologo
fi

echo "==> nuvyn update leaves host and specs alone"
mkdir -p "$APP/specs/001-desk"
echo "# user spec" > "$APP/specs/001-desk/spec.md"
echo "<!-- prove-host marker -->" >> "$APP/ProveHost/Pages/MainPage.xaml"
(
  cd "$APP"
  "${CLI[@]}" update --agent cursor
)
grep -F "# user spec" "$APP/specs/001-desk/spec.md" >/dev/null
grep -F "<!-- prove-host marker -->" "$APP/ProveHost/Pages/MainPage.xaml" >/dev/null
grep -F "Build is the gate" "$APP/.cursor/skills/nuvyn-implement/SKILL.md" >/dev/null

echo "Nuvyn 1.0 host proof passed"
