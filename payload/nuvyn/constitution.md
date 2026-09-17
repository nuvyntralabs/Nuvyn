# Nuvyntra constitution

**App / repo:** [NAME]
**Updated:** [DATE]

Later `/nuvyn.*` commands treat this as law. Standing detail: `.nuvyn/reference/constraints.md`.

The CLI is **not** a clinic (or any other vertical) toolkit. Domain comes from **App-specific principles** and the user's spec. The stack is always **.NET MAUI** on Android, iOS, Windows, and Mac Catalyst.

## Stack

1. **Catalog first.** MauiEssentials (`Plugin.Maui.*`, `NuvyntraLabs.*`, `Nuventra.*`) before any other library. Outside package only with a recorded gap.
2. **Four platforms.** `net10.0-android`, `net10.0-ios`, `net10.0-maccatalyst`, `net10.0-windows10.0.19041.0`. Native Android/iOS plugins stay on those TFMs.
3. **Default host.** MVVMExpress, UIKit, HttpForge, FormValidation, KeyboardManager. Do **not** add any other PackageReference until the user explicitly asks (LocalStore, NuvexaDB, AppLock, …).
4. **NavigationPage chrome.** `UseNavigationPage` + `Map` in `MauiProgram`. ViewModels in Core. Both types `AddTransient`. `[RegisterViewModel]` is not DI. No `AppShell` / `.UseShell()` unless App-specific principles or spec MP-NAV require flyout/Shell and Complexity tracking records the override.
5. **Sleek Lumina UI.** Recipes + `NVTokens`. Business-grade, not a dummy. Compose inside the recipe. See `.nuvyn/reference/ui-and-catalog.md`.
6. **No extra plugin until asked.** No Observability, LocalStore, or the rest of the catalog unless the user names it.
7. **API / in-memory until asked to persist.** In-memory seed if no contract. No LocalStore / `Map<T>` until the user asks.
8. **Real product.** User reference URL wins. Else live-product knowledge.

## Compose

JobQueue ≠ RetryQueue ≠ BackgroundTasks. PushRouter ≠ FCM. AppLock ≠ Biometric ≠ SecureSession. LocalStore ≠ Nuvexa engine. UIKit does not reference Plugin.Maui.* — attach FormValidation / KeyboardManager / MVVMExpress on the host.

## Hardened / quality

Fail-closed DeepLinks, PushRouter, SmartUpload, FeatureFlags. `UseX` in `MauiProgram`. Permissions from each plugin README. No `dotnet nuget push`. No second MVVM or paid UI kit.

**Agent tokens:** follow the budget in `constraints.md` — short tables, no rule restatement, README only for packages you add.

## App-specific principles

[Product rules: lock, PII, offline, …]
