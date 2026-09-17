# Implementation plan: [FEATURE NAME]

**Feature:** [link to spec.md]  
**Host path:** `.`  
**Created:** [DATE]

Filled by `/nuvyn.plan`. Stack is locked. Domain comes from the spec. Chrome is **NavigationPage** (`UseNavigationPage` + `Map`). Do not add `AppShell` unless Complexity tracking records a spec MP-NAV flyout/Shell override.

## Summary

[Primary requirement + approach. Default host only unless the user asked for more.]

## Technical context

| | Locked / filled |
| --- | --- |
| Language | C# / .NET 10 |
| Host | Plugin.Maui.MVVMExpress (`UseNavigationPage`) |
| UI | NuvyntraLabs.UIKit (Lumina) |
| HTTP | Plugin.Maui.HttpForge |
| Forms / keyboard | FormValidation, KeyboardManager |
| Storage | None unless the user asked (then LocalStore + NuvexaDB) |
| Testing | `dotnet test` if the spec asked for tests; else skip |
| TFMs | net10.0-android, net10.0-ios, net10.0-maccatalyst, net10.0-windows10.0.19041.0 |
| Project type | Three-project MAUI host (already created by `nuvyn init`) |

## Constitution check

*GATE before research. Re-check after screens.*

- [ ] Default host only — no extra PackageReference unless the user asked
- [ ] UIKit first, same tokens
- [ ] Four TFMs; native plugins not sold as Windows
- [ ] Fail-closed only for plugins the user asked to add
- [ ] No `AppShell` / `.UseShell()` unless Complexity tracking records the override

## Package map

Default host (already on the tree). Keep these rows. Agents cannot see CLI `DefaultHostPackages` — this table is the source.

| Need | Package | Register | csproj |
| --- | --- | --- | --- |
| MVVM / nav | Plugin.Maui.MVVMExpress (+ Dialogs, Navigation) | `UseMvvmExpress` + `UseNavigationPage` + `UseDialogs` | MAUI app |
| ViewModels | Plugin.Maui.MVVMExpress.Core + SourceGenerators | `[RegisterViewModel]` is **not** DI | Core |
| Tests | Plugin.Maui.MVVMExpress.Testing | — | Tests |
| UI | NuvyntraLabs.UIKit | `UseNuvyntraUIKit()` | MAUI app |
| HTTP | Plugin.Maui.HttpForge | `UseHttpForge()` | MAUI app |
| Forms | Plugin.Maui.FormValidation | `UseMauiFormValidation()` | MAUI app |
| Keyboard | Plugin.Maui.KeyboardManager | `UseKeyboardManager()` | MAUI app |

One extra row per **user-asked** package. `Register` = the `UseX` call. `csproj` = MAUI app unless the plugin README says Core.

| Need (user asked) | Package | Register | csproj | Notes |
| --- | --- | --- | --- | --- |
| | | | | |

## Data

| Concern | Choice | Why |
| --- | --- | --- |
| Domain data | HttpForge API or in-memory seed | No LocalStore unless the user asked to persist |
| Offline / jobs | none | Add OfflineSync / JobQueue / RetryQueue / BackgroundTasks only if asked |

## Screens (Lumina)

Route = `Map<TVm, TPage>("name")` string (same as `[Route("name")]`). Not a Shell `//` URI.

| ID | User story | Recipe | Extra `NV*` | ViewModel (Core) | Route |
| --- | --- | --- | --- | --- | --- |
| SC-1 | US-1 | `NVSignInView` | `NVInputField` | `SignInViewModel` | `signin` |

Start route from spec MP-NAV (example: `signin`). Recipes seed demo children. Put bound `NV*` primitives **inside**. Do not invent a new `NV*View`.

Chrome from MP-NAV: NavigationPage (default) · tabs = `NVTabView` / `NVBottomNavigation` inside a mapped page · flyout/Shell = Complexity tracking override only.

## Design artifacts

Write beside this file:

- `research.md` — extra packages only if asked; else `None — default host`
- `data-model.md` — only if the user asked to persist
- `quickstart.md` — how to run the MAUI host
- `contracts/` — HttpForge interface sketches if the spec has an API

## Project structure (this host)

```text
./
├── {App}.sln
├── {App}/                         # MAUI app
│   ├── MauiProgram.cs             # UseX + Map + both AddTransient
│   ├── App.xaml.cs                # NavigationPage + start page only
│   ├── Pages/
│   └── Platforms/Android|iOS|MacCatalyst|Windows/
├── {App}.Core/                    # ViewModels (not under the MAUI project)
├── {App}.Tests/
└── specs/NNN-slug/
    ├── spec.md
    ├── plan.md
    ├── research.md
    ├── tasks.md
    └── checklists/
```

No `AppShell.xaml`. No `{App}/ViewModels/`.

## Security defaults

Fill **only** for plugins the user asked to add.

| Plugin | Required setting |
| --- | --- |
| DeepLinks | explicit `Hosts` / `CustomSchemes`; no `PermissiveMode` |
| PushRouter | `Map(...)` + `DefaultRoute`; no `AllowUnmappedPayloadRoutes`. Routes are NavigationPage names — not a reason to add AppShell |
| SmartUpload | `https`; `RequireHttps = true` |
| FeatureFlags | `https`; optional `SignatureKey` |

## Platform declarations

| Platform | File | Keys |
| --- | --- | --- |
| Android | `AndroidManifest.xml` | [only for packages the user asked] |
| iOS / Mac Catalyst | `Info.plist` | [from those READMEs] |
| Windows | `Package.appxmanifest` | [only if that plugin supports Windows] |

## Catalog gap

None.

## Complexity tracking

Fill only if a constitution MUST is overridden (including AppShell / `.UseShell()`).

| Violation | Why needed | Simpler alternative rejected |
| --- | --- | --- |
| | | |
