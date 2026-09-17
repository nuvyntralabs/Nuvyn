# /nuvyn.task

Read `.nuvyn/reference/constraints.md` and `.nuvyn/reference/implement-recipes.md` once. Write **only** `tasks.md`. No host code.

You **MUST** consider `$ARGUMENTS`.

## User input

```text
$ARGUMENTS
```

Read `.nuvyn/feature.json`. Require `spec.md` and `plan.md`. Load `research.md` / `data-model.md` if they exist.

## Format (required)

```text
- [ ] T001 [P?] [US1?] Imperative description in exact/file/path
```

- IDs `T001`, `T002`… in execution order
- `[P]` only when a different file and no unfinished dependency
- `[US1]` required on story-phase tasks only
- Every task has a file path
- Host already exists — no "create the MAUI app" task

## Phases

1. Foundation (blocking `UseX`, both `AddTransient<MainPageViewModel>()` and `AddTransient<MainPage>()`, permissions, HttpForge / LocalStore only if planned)
2. One phase per user story (Core ViewModel, Lumina page + ctor injection, `Map` + both transients in `MauiProgram.cs`, acceptance). Never `AppShell.xaml`.
3. Polish (empty/error/busy, `maui-dev doctor`, no permissive opt-outs)

No PackageReference task for a package not in `plan.md`. Tests only if the spec asked for TDD.

## Completion report

Task count, count per story, parallel opportunities, suggested MVP (usually US-1).

## Done when

- [ ] `tasks.md` uses the checklist format
- [ ] Completion reported

Next: `/nuvyn.analysis` then `/nuvyn.implement`.
