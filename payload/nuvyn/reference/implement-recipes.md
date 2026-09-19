# Implement recipes

Read with `.nuvyn/reference/constraints.md`. The host is the three-project tree from `nuvyn init` (`payload/host/MauiApp1`). Do not invent `AppShell`.

If `dotnet build` fails on the machine or csproj (workloads, TFMs, permissions, `UseMaui`), run `maui-dev doctor --path .`. Do not re-run `nuvyn init`.

`[RegisterViewModel]` on a Core type is **not** DI. It does not register the view-model (or the page) in the MAUI host. `AddGeneratedViewModels()` is forbidden.

## New screen (one checklist)

Do every row for each `plan.md` screen.

| Step | Where | What |
| --- | --- | --- |
| 1. ViewModel | `{App}.Core/` (same project as `MainPageViewModel`; optional `ViewModels/` folder under Core) | `PageViewModel` + `[RegisterViewModel]` + `[Route("name")]`. Never `{App}/ViewModels/`. |
| 2. Page | `{App}/Pages/…Page.xaml` + `.xaml.cs` | Ctor injection: `public FooPage(FooViewModel vm) { InitializeComponent(); BindingContext = vm; }` |
| 3. Both transients | `{App}/MauiProgram.cs` | `AddTransient<FooViewModel>();` **and** `AddTransient<FooPage>();` |
| 4. Map | `{App}/MauiProgram.cs` inside existing `UseNavigationPage((nav, _) => …)` | `.Map<FooViewModel, FooPage>("name")` — same `name` as `[Route]` and the plan Route column |
| 5. Start page only | `{App}/App.xaml.cs` | Touch **only** when this screen is the launch page: `new NavigationPage(_services.GetRequiredService<FooPage>())`. Keep the `IServiceProvider` ctor. |

Do **not**:

- Add `AppShell.xaml` / `.UseShell()` / `new AppShell()`
- Treat `[RegisterViewModel]` or a reflection scan as a substitute for the two `AddTransient` lines
- Call `AddGeneratedViewModels()` or add `using Plugin.Maui.MVVMExpress.Generated`
- Insert a second bare `.UseMvvmExpress()`
- Put ViewModels in the MAUI project

Tabs = `NVTabView` / `NVBottomNavigation` **inside** a mapped page. Flyout / MAUI Shell only if spec MP-NAV named that chrome **and** `plan.md` Complexity tracking records the override.

## Default `UseX` (already on the host)

| Call | Package | csproj |
| --- | --- | --- |
| `UseMvvmExpress` + `UseNavigationPage` + `UseDialogs` | `Plugin.Maui.MVVMExpress` (+ `.Dialogs`, `.Navigation`) | MAUI app |
| — | `Plugin.Maui.MVVMExpress.Core` + `.SourceGenerators` | Core |
| — | `Plugin.Maui.MVVMExpress.Testing` | Tests |
| `UseNuvyntraUIKit()` | `NuvyntraLabs.UIKit` | MAUI app |
| `UseHttpForge()` | `Plugin.Maui.HttpForge` | MAUI app |
| `UseMauiFormValidation()` | `Plugin.Maui.FormValidation` | MAUI app |
| `UseKeyboardManager()` | `Plugin.Maui.KeyboardManager` | MAUI app |

Extra packages: `PackageReference` + `UseX` on the **MAUI app** unless the plugin README says Core. Never add a package that is not in `plan.md`.
