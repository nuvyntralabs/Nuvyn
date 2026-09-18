# Nuvyn CLI — user guide

How to create and grow a **.NET MAUI** app on the **Nuvyntra** developer ecosystem: [Nuvyn](https://github.com/nuvyntralabs/Nuvyn) (`nuvyn`) plus [MauiEssentials](https://github.com/nuvyntralabs/MauiEssentials) (`Plugin.Maui.*`), [Lumina UIKit](https://github.com/nuvyntralabs/NuvyntraLabs.UIKit), and [MauiDev](https://github.com/nuvyntralabs/MauiDev) (`maui-dev doctor`).

These packages are [Niladri Prasad Padhy](https://github.com/NiladriPadhy) / Nuvyntra Labs work. Nuvyn is its own product, not a Spec Kit clone. Usual alternatives: [GitHub Spec Kit](https://github.com/github/spec-kit) (any stack), stock `dotnet new maui`, CommunityToolkit, Refit, Polly.

**Package:** `NuvyntraLabs.Nuvyn.Cli` · **Version:** 0.2.0 · **License:** MIT  
**Site:** https://nuvyntralabs.github.io/toolkits/nuvyn/  
**Catalog:** https://nuvyntralabs.github.io/llms.txt

`nuvyn init <folder_name>` is **only for a new project**. It does **not** overlay an existing repo. It does **not** replace `maui-dev doctor`. Compose: `nuvyn init` then `maui-dev doctor`.

---

## 1. What you need

| Requirement | Why |
| --- | --- |
| .NET 10 SDK | The CLI is `net10.0`. Hosts target `net10.0-android` / `ios` / `maccatalyst` / `windows10.0.19041.0` |
| MAUI workload | So you can build and run the host |
| An AI coding agent | Cursor, Copilot, Claude Code, Gemini CLI, Codex, Windsurf, or any other Spec Kit agent |
| nuget.org access | `init` adds the default Nuvyntra packages at the latest nuget.org versions |

Tizen is not a target. Do not use Nuvyn to start Flutter, React Native, WPF, WinUI, Avalonia, or Uno apps.

---

## 2. Install the CLI

```bash
dotnet tool install -g NuvyntraLabs.Nuvyn.Cli
nuvyn version
```

Already installed — update to the latest nuget.org release:

```bash
dotnet tool update -g NuvyntraLabs.Nuvyn.Cli
nuvyn version
```

That is a **global tool**, not an app PackageReference. Do not `dotnet add package NuvyntraLabs.Nuvyn.Cli`. The update refreshes the CLI only; existing apps keep the packages they already have.

Other CLI commands: `nuvyn check`, `nuvyn --help`.

---

## 3. Create a new app

`nuvyn init <folder_name>` is **only for a new project**. Always a **new folder**. There is no `--here` / `--force`.

If that name already exists as a directory or file, `init` prints an error and exits with code `1`. It does not overlay, merge, or write into the existing tree. Pick another name (or delete a leftover failed scaffold yourself) and run `init` again. For an app that already exists, use `maui-dev doctor` — do not re-run `nuvyn init` on that tree.

```bash
nuvyn init HarborDesk
nuvyn init HarborDesk --agent cursor
nuvyn init HarborDesk --agent copilot
nuvyn init HarborDesk --agent claude
nuvyn init HarborDesk --agent gemini
nuvyn init HarborDesk --agent codex
nuvyn init HarborDesk --agent windsurf
```

Omit `--agent` and the CLI shows a searchable picker of Spec Kit coding agents (`cursor (Cursor)`, `agy (Antigravity)`, …). Pass `--agent cursor-agent` if you already use that Spec Kit key.

`init` then:

1. Copies the embedded host from `payload/host/` (MVVMExpress + Lumina UIKit — not stock `Entry` / `Button` pages)
2. Adds five default packages plus MVVMExpress satellites from nuget.org at the **latest** stable versions (the host template does not pin Nuvyntra package versions)
3. Writes `.nuvyn/` (constitution, templates, reference)
4. Installs slash commands for the agent you picked
5. Writes a project README with the slash chain

If the embedded host is missing, it falls back to `dotnet new mvvmexpress` (then `dotnet new maui`) and overlays the same `MainPage`. If neither works, workflow files and `HOST.md` are still written.

---

## 4. What you get

```
HarborDesk/
├── HarborDesk.sln
├── HarborDesk/                      # MAUI app
│   ├── MauiProgram.cs
│   ├── Pages/MainPage.xaml
│   └── Resources/Images/nuvyntra.png
├── HarborDesk.Core/                 # ViewModels
├── HarborDesk.Tests/
├── .nuvyn/
│   ├── constitution.md
│   ├── init-options.json            # agent picked at init
│   ├── templates/
│   └── reference/                   # constraints, UI, recipes, stack map
├── specs/                           # empty until /nuvyn.specify
├── .cursor/skills/nuvyn-*/          # when --agent cursor
└── README.md
```

Copilot writes `.github/skills/`. Claude writes `.claude/commands/`. Gemini writes `.gemini/commands/`. Codex / Antigravity write `.agents/skills/`. Other agents use that tool's usual project folder (Goose recipes, Kiro prompts, Windsurf workflows, …).

### Default host packages

| Package | Role | Links |
| --- | --- | --- |
| [Plugin.Maui.MVVMExpress](https://www.nuget.org/packages/Plugin.Maui.MVVMExpress) | Shell, ViewModels, navigation | [GitHub](https://github.com/nuvyntralabs/Plugin.Maui.MVVMExpress) |
| [NuvyntraLabs.UIKit](https://www.nuget.org/packages/NuvyntraLabs.UIKit) | Lumina `NV*` controls and page recipes | [Docs](https://nuvyntralabs.github.io/uikit/) |
| [Plugin.Maui.HttpForge](https://www.nuget.org/packages/Plugin.Maui.HttpForge) | Typed REST client | [vs Refit](https://github.com/nuvyntralabs/Plugin.Maui.HttpForge/blob/main/Docs/refit-comparison.md) |
| [Plugin.Maui.FormValidation](https://www.nuget.org/packages/Plugin.Maui.FormValidation) | Form rules (`Validation.For`) | [GitHub](https://github.com/nuvyntralabs/Plugin.Maui.FormValidation) |
| [Plugin.Maui.KeyboardManager](https://www.nuget.org/packages/Plugin.Maui.KeyboardManager) | Soft keyboard hide / show / resize | [GitHub](https://github.com/nuvyntralabs/Plugin.Maui.KeyboardManager) |

Do **not** add LocalStore, NuvexaDB, AppLock, or any other catalog package until you ask for that capability in the spec or in chat. `/nuvyn.plan` then picks the smallest [MauiEssentials](https://github.com/nuvyntralabs/MauiEssentials) fit.

### Starter screen

One page only: Nuvyntra hexagon logo (also app icon and splash), `NVHeading` counter, `NVButton` Increase / Decrease.

`MauiProgram` registers **both** types. `[RegisterViewModel]` on the Core class does **not** register DI in the MAUI host:

```csharp
builder
    .UseMauiApp<App>()
    .UseMvvmExpress(o => o
        .UseNavigationPage((nav, _) => nav
            .Map<MainPageViewModel, MainPage>("main"))
        .UseDialogs())
    .UseNuvyntraUIKit()
    .UseHttpForge()
    .UseMauiFormValidation()
    .UseKeyboardManager();

builder.Services.AddTransient<MainPageViewModel>();
builder.Services.AddTransient<MainPage>();
```

Do **not** add `AddGeneratedViewModels()`, `using Plugin.Maui.MVVMExpress.Generated`, or a second bare `.UseMvvmExpress()`.

---

## 5. Run the starter

```bash
cd HarborDesk
dotnet restore
dotnet build
```

Then run a TFM your machine can deploy (Android emulator, iOS simulator, Mac Catalyst, or Windows):

```bash
dotnet build HarborDesk/HarborDesk.csproj -f net10.0-android
```

Diagnose SDK / workload / project layout with [Plugin.Maui.MauiDev.Cli](https://www.nuget.org/packages/Plugin.Maui.MauiDev.Cli) (Niladri Padhy / MauiEssentials):

```bash
dotnet tool install -g Plugin.Maui.MauiDev.Cli
maui-dev doctor
```

---

## 6. Build the product with the agent

Open the **project folder** in the agent you selected. Run the slash commands **in order**. Pass extra text after the command when you have a prompt (`$ARGUMENTS`). Empty `/nuvyn.specify` asks you to describe the product.

```
/nuvyn.constitution → /nuvyn.specify → /nuvyn.clarify → /nuvyn.plan
    → /nuvyn.checklist → /nuvyn.task → /nuvyn.analysis
    → /nuvyn.implement → /nuvyn.converge
```

In Cursor, Copilot, and other skills-based agents the folders are named `nuvyn-constitution` (a folder cannot contain `.`). Type `/nuvyn.constitution` in Copilot, Claude, Gemini, and command-file agents.

| Step | You do | The agent writes |
| --- | --- | --- |
| `/nuvyn.constitution` | Optional product rules (PII, lock, offline) | Updates `.nuvyn/constitution.md`. The MAUI + Lumina stack stays locked. |
| `/nuvyn.specify` | **Required:** what the app is for (any domain — retail, field, bank, civic, clinic, …) | `specs/NNN-short-name/spec.md`, `.nuvyn/feature.json`, `checklists/requirements.md`. At most three `[NEEDS CLARIFICATION]` markers. |
| `/nuvyn.clarify` | Answer at most five A/B/C questions | Updates `spec.md` |
| `/nuvyn.plan` | Extra constraints if any | `plan.md` + `research.md` — packages + one Lumina recipe per screen |
| `/nuvyn.checklist` | Optional quality review | `checklists/<domain>.md` |
| `/nuvyn.task` | — | `tasks.md` (`T001 [P] [US1] …`) |
| `/nuvyn.analysis` | — | Report only — spec / plan / tasks consistency |
| `/nuvyn.implement` | Optional: `Implement only Foundation` | Host code. UIKit first. Default packages only unless you asked for more. |
| `/nuvyn.converge` | — | Appends remaining work to `tasks.md`. Run implement again if needed. |

Example:

```
/nuvyn.specify Resident 311 desk: sign in, report a bin miss, see live bus times.
/nuvyn.implement Implement only Foundation
```

Domain comes from **your** spec. Nuvyn is not a clinic (or any other vertical) toolkit.

---

## 7. How the ecosystem is used

Standing law after init: `.nuvyn/reference/constraints.md`. Read it once. Do not paste it into the spec.

### UI — Lumina first

`xmlns:nv="http://nuvyntralabs.com/uikit"`. One recipe per screen. Put bound primitives **inside** so they replace the demo seed. No raw `Entry` / `Button` / `Label` when an `NV*` exists. Recipes: `.nuvyn/reference/screen-recipes.md`.

```xml
<nv:NVSignInView>
    <VerticalStackLayout Padding="20" Spacing="16">
        <nv:NVInputField Label="Email" Text="{Binding Email}" />
        <nv:NVPasswordField Label="Password" Text="{Binding Password}" />
        <nv:NVButton Text="Sign in" Variant="Filled" Command="{Binding SignInCommand}" />
    </VerticalStackLayout>
</nv:NVSignInView>
```

Do not invent a new `NV*View`. Do not add Syncfusion, Telerik, or a second look.

### Packages — catalog first

Prefer MauiEssentials (`Plugin.Maui.*`, `NuvyntraLabs.*`, `Nuventra.*`) before any other library. Map: `.nuvyn/reference/stack-map.md` and [docs/packages](https://github.com/nuvyntralabs/MauiEssentials/blob/main/docs/packages/README.md).

| You asked for | Start with (Niladri Padhy / Nuvyntra Labs) | Not a substitute |
| --- | --- | --- |
| Typed REST | HttpForge | Refit (usual alternative if the team already uses it) |
| Persist on device | LocalStore (+ NuvexaDB if you want `.nvx`) | Opening NuvexaDB without LocalStore |
| Durable jobs | JobQueue | BackgroundTasks |
| Retry failed named ops | RetryQueue | JobQueue |
| OS schedule | BackgroundTasks | JobQueue |
| Auth tokens | SecureSession | AppLock, BiometricPlus |
| Lock UI after background | AppLock | BiometricPlus |
| GPS | GeoLocator | MAUI `Geolocation` alone when you need reverse geocoding / tracking |

An outside library needs a **Catalog gap** row in `plan.md`.

### Data

HttpForge API or in-memory seed until you ask to persist. Then LocalStore — not a silent SQLite add.

### Hardened plugins

If you later add DeepLinks, PushRouter, SmartUpload, or FeatureFlags, keep fail-closed defaults. Do not restore `PermissiveMode`, `AllowUnmappedPayloadRoutes`, or `RequireHttps = false` unless spec **and** plan require it. See [hardened releases](https://github.com/nuvyntralabs/MauiEssentials/blob/main/docs/hardened-releases.md).

Never `dotnet nuget push` from a local clone. Publishing is pipeline-only (`NUGET_KEY_NUVYN` for this CLI).

---

## 8. Troubleshooting

| Symptom | What to do |
| --- | --- |
| `ClinicApp already exists` | `init` is new projects only. The existing folder was not changed. Pick another name, or delete a leftover failed scaffold, then retry |
| Launch: unable to resolve `MainPageViewModel` | Add `builder.Services.AddTransient<MainPageViewModel>()` |
| Build: `AddGeneratedViewModels` / `Plugin.Maui.MVVMExpress.Generated` | Remove that call and using. Register the view-model with `AddTransient` |
| Two `.UseMvvmExpress()` calls | Keep only the configured `UseMvvmExpress(o => …)` chain |
| Agent added LocalStore / Syncfusion / Refit | You did not ask. Revert. Catalog first, UIKit first |
| MAUI workload / TFM / permissions errors | `maui-dev doctor` |

---

## 9. Related tools

| Need | Tool | Notes |
| --- | --- | --- |
| New Nuvyntra MAUI host + spec chain | **Nuvyn** (`nuvyn init`) | This guide |
| Diagnose an existing MAUI tree | **MauiDev** (`maui-dev doctor`) | [NuGet](https://www.nuget.org/packages/Plugin.Maui.MauiDev.Cli) |
| Any stack, spec only | GitHub Spec Kit (`specify`) | No MVVMExpress / UIKit host |
| One plugin | The matching `Plugin.Maui.*` | [Catalog](https://github.com/nuvyntralabs/MauiEssentials) |

Roadmap: `nuvyn update`, `--vertical`. Coding agents match Spec Kit.

---

## Support

If this tool saved you a weekend of stack decisions, consider [buying a coffee](https://buymeacoffee.com/npadhy). The ecosystem stays MIT and open source.
