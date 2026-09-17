# /nuvyn.plan

Read `.nuvyn/reference/constraints.md` and `.nuvyn/reference/implement-recipes.md` once. Write design artifacts for the user's product on .NET MAUI (Android, iOS, Windows, Mac Catalyst). No `tasks.md`. No host code.

You **MUST** consider `$ARGUMENTS` (extra constraints).

## User input

```text
$ARGUMENTS
```

Read `.nuvyn/feature.json`. Require `spec.md` and `.nuvyn/constitution.md`. If missing, stop.

## Steps

1. Copy `.nuvyn/templates/plan.md` → `plan.md`. Keep the default Package map rows (`UseX` + which csproj). Agents cannot see CLI `DefaultHostPackages`.
2. **Default host only.** MVVMExpress, UIKit, HttpForge, FormValidation, KeyboardManager. Do not add any other package unless the user explicitly asked.
3. **NavigationPage.** `UseNavigationPage` + `Map`. Do **not** invent `AppShell` / `.UseShell()`. Tabs = `NVTabView` / `NVBottomNavigation`. Flyout/Shell only if spec MP-NAV named that chrome **and** Complexity tracking records the override. PushRouter routes are Map names — not a reason for Shell.
4. **UIKit first** — one Lumina recipe per screen. Compose primitives inside the recipe. Do not invent a new `NV*View`.
5. Copy `research.md` / `quickstart.md` templates. Copy `data-model.md` only if the user asked to persist.
6. HttpForge contracts under `contracts/` if the spec has an API.
7. Fail-closed rows only for DeepLinks / PushRouter / SmartUpload / FeatureFlags you selected. Permissions from those READMEs only.
8. Constitution check — ERROR if a MUST principle is violated without a recorded override.

## Completion report

`plan.md` path, `research.md`, `data-model.md` if written, catalog-gap row.

## Done when

- [ ] `plan.md` filled; research written
- [ ] No invented APIs / permissive hardened opt-outs
- [ ] Completion reported

Next: `/nuvyn.checklist` or `/nuvyn.task`.
