# Requirement → package

Use this table during `/nuvyn.plan`. **Catalog first** — this list and the plugin README / `src/` / NuGet page are the primary source. Canonical table: https://github.com/nuvyntralabs/MauiEssentials/blob/main/docs/packages/README.md

## Always (already on the host after `nuvyn init`)

CLI `DefaultHostPackages` is not in the app tree. Use this table (same rows as `plan.md`).

| Package | Register | csproj |
| --- | --- | --- |
| Plugin.Maui.MVVMExpress (+ Dialogs, Navigation) | `UseMvvmExpress` + `UseNavigationPage` + `UseDialogs` | MAUI app |
| Plugin.Maui.MVVMExpress.Core + SourceGenerators | `[RegisterViewModel]` is not DI | Core |
| Plugin.Maui.MVVMExpress.Testing | — | Tests |
| NuvyntraLabs.UIKit | `UseNuvyntraUIKit()` | MAUI app |
| Plugin.Maui.HttpForge | `UseHttpForge()` | MAUI app |
| Plugin.Maui.FormValidation | `UseMauiFormValidation()` | MAUI app |
| Plugin.Maui.KeyboardManager | `UseKeyboardManager()` | MAUI app |
| Plugin.Maui.MauiDev.Cli | `maui-dev` tool — not a PackageReference | — |

Do **not** add a row from the table below unless the user explicitly asked for that capability.

## If the user asked for…

| Spec signal | Package |
| --- | --- |
| Retry / circuit | ApiResilience |
| GET cache | ApiCache |
| Captive portal | NetworkMonitor |
| DNS / TLS probe | NetworkDiagnostics |
| Auth tokens | SecureSession |
| AES store | SecureStoragePlus |
| Lock after background | AppLock |
| One-shot Face ID / PIN | BiometricPlus |
| Screenshot / recents | ScreenGuard |
| App documents (only if the user asked to persist on device) | LocalStore |
| Embedded `.nvx` (only with LocalStore persist) | NuvexaDB |
| Offline writes | OfflineSync |
| Survive process death | JobQueue |
| Retry payments / telemetry | RetryQueue |
| OS periodic work | BackgroundTasks |
| Resumable upload | SmartUpload |
| Camera image / video | MediaPipeline / VideoPipeline |
| Encrypted files | FileVault |
| GPS / geofence | GeoLocator / Geofence |
| Permission UX | PermissionFlow |
| Push tap / local notify | PushRouter / LocalNotifications |
| App Links | DeepLinks |
| Feature flags | FeatureFlags |
| TLS pin | TlsPin |
| NFC / BLE / SPP | NfcPlus / BluetoothManager / BluetoothSerial |
| Print / share / clipboard | Printing / SharePlus / ClipboardPlus |
| Unified telemetry | Observability (only if asked) |
