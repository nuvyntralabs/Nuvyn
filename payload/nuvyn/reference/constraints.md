# Nuvyntra constraints

Read **once** per `/nuvyn.*` turn. Do not paste this file into spec/plan/tasks.

Index: https://nuvyntralabs.github.io/llms.txt  
Packages: https://github.com/nuvyntralabs/MauiEssentials/blob/main/docs/packages/README.md  
Hardened: https://github.com/nuvyntralabs/MauiEssentials/blob/main/docs/hardened-releases.md  
UI: `.nuvyn/reference/ui-and-catalog.md` · recipes: `.nuvyn/reference/screen-recipes.md` · implement: `.nuvyn/reference/implement-recipes.md`

## Scope

Skills are **domain-agnostic**. Build the product the user described (retail, field, bank, civic, clinic, or anything else). Do not assume a clinic, booking flow, or any other vertical. Product rules live only in that host's `.nuvyn/constitution.md` **App-specific principles**.

Skills **are** locked to **.NET MAUI** on **Android, iOS, Windows, and Mac Catalyst** (`net10.0-android`, `net10.0-ios` 15+, `net10.0-maccatalyst` 15+, `net10.0-windows10.0.19041.0`). Nuvyn hosts declare Android **API 24+** (`SupportedOSPlatformVersion` 24). Catalog plugins may still compile at API 21. Tizen is not a target. Do not emit Flutter, React Native, or a phone-only stack. Native Android/iOS plugins stay on those TFMs — on Windows/Catalyst use a MAUI built-in or skip and say so.

The UI is a **sleek, modern business** MAUI app: Lumina tokens, real copy, empty/error/busy. Not a dummy gallery.

## Standing rules

1. **UIKit first** — Lumina recipes + `NVTheme` / `NVTokens`. No Syncfusion, Telerik, or a second look.
2. **Catalog first** — `Plugin.Maui.*` / `NuvyntraLabs.*` / `Nuventra.*` (map, README, sample). MAUI built-ins if enough. Outside lib → **Catalog gap** in `plan.md`.
3. **No new package until asked.** Default host is enough. Do not `dotnet add package` LocalStore, NuvexaDB, AppLock, or anything else unless the user explicitly asked in this conversation or the spec.
4. **API / in-memory** — persist only if the user asked. No API contract → in-memory seed.
5. **Real product** — user URL (spec, Figma, OpenAPI) is source of truth. Else live-product knowledge. No "Item 1" / lorem.

## Token budget (agent)

Keep `/nuvyn.*` cheap:

- Law lives here. Commands stay short. Do not restate these rules in spec/plan/chat.
- Open a plugin README **only** for packages you add this turn.
- Do not dump `llms.txt`, the package map, or the full `NV*` list.
- Spec / plan / tasks: tables only. One line per need.
- Implement: edit files + a short file list. No type-by-type walkthrough.
- Search with `rg` / targeted reads. Do not slurp the repo.
- Compose samples live in `ui-and-catalog.md` (form + list + empty/busy). Do not recopy them.

## Adopted host

If `.nuvyn/adopt-report.md` exists, this tree was attached with `nuvyn adopt`. Do **not** apply Default host / NavigationPage / Lumina rules. Keep the architecture, UI kit, and HTTP client in that report. Do not add MVVMExpress, UIKit, or HttpForge unless already present or the user asks. New work matches the Keep column. Do not restyle existing pages.

## Default host

MVVMExpress, UIKit, HttpForge, FormValidation, KeyboardManager. Nothing else until the user asks. Skip this section when `.nuvyn/adopt-report.md` exists.

Chrome is **NavigationPage**: `UseNavigationPage` + `.Map<TVm, TPage>("name")` + `AddTransient` for **both** the Core view-model and the page. `[RegisterViewModel]` is not DI. Do not add `AppShell` / `.UseShell()` unless spec MP-NAV named flyout/Shell **and** `plan.md` Complexity tracking records the override. Tabs = `NVTabView` / `NVBottomNavigation` inside a mapped page. PushRouter `Map` uses those route names — it does not require Shell.

## Compose

| Need | Start with | Not a substitute |
| --- | --- | --- |
| OS schedule | BackgroundTasks | JobQueue, RetryQueue |
| Durable jobs | JobQueue | BackgroundTasks |
| Failed named ops | RetryQueue | JobQueue |
| Offline writes | OfflineSync | LocalStore, JobQueue |
| App documents (if asked) | LocalStore | NuvexaDB direct |
| UI lock | AppLock | Biometric, SecureSession |
| Push tap → route | PushRouter | FCM / APNs; route names are `Map` strings, not AppShell |
| Auth tokens | SecureSession | AppLock, BiometricPlus |

`IAuthGateway` before `UseSecureSession`. Do not `.AddSecureSession()` on the login/refresh client (DI cycle). `UseMauiLocalStore` / `UseMauiJobQueue` are lazy: resolve `ILocalStore` / `IJobQueue`; never `.Current` in a factory.

## Hardened / limits

No `PermissiveMode`, `AllowUnmappedPayloadRoutes`, or `RequireHttps = false` unless spec + plan require it.

Do not invent: Geofence Android is `GeofencingClient` (persist + `Raise()` for samples; `Denied` / `NotSupported` typed results), BluetoothSerial = SPP, VideoPipeline is `FromCamera()` / `FromGallery()` (not `FromCameraAsync`; no FFmpeg; `CannotTranscode` if the device cannot encode; `DefaultMaxDuration` from `UseVideoPipeline`), TlsPin needs backup pin, AppReview is `GetEligibilityAsync` / `RequestAsync` (not `IsEligibleAsync`; Android Play Core `ReviewManager` on Play-installed builds, else `Unavailable` → `OpenStoreListingAsync`), ScreenGuard iOS = overlay, VoipCore ≠ PJSIP.

Never `dotnet nuget push`.
