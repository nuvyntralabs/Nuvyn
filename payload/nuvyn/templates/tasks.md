# Tasks: [FEATURE NAME]

**Input:** spec.md + plan.md (+ research.md / data-model.md / quickstart.md if present)  
**Created:** [DATE]

**Tests:** include test tasks **only** if the spec asked for TDD.  
**Packages:** do not add a PackageReference task unless the user asked for that package.

Format: `- [ ] T001 [P?] [US1?] Imperative description in exact/file/path`

Read `.nuvyn/reference/implement-recipes.md`. ViewModels live in **Core**. Routes are `Map` in `MauiProgram.cs`, not Shell.

## Phase 1: Foundational (blocking)

**Purpose:** Wire the default host. No extra NuGet.

- [ ] T001 Confirm `UseMvvmExpress` + `UseNavigationPage` + `UseNuvyntraUIKit()` + `UseHttpForge()` + `UseMauiFormValidation()` + `UseKeyboardManager()` and `AddTransient<MainPageViewModel>()` + `AddTransient<MainPage>()` in {App}/MauiProgram.cs
- [ ] T002 [P] Confirm FormValidation / KeyboardManager host attach if `plan.md` lists extra `UseX`
- [ ] T003 HttpForge interface or in-memory seed in the host (no LocalStore unless asked)

**Checkpoint:** Foundation ready. `/nuvyn.implement` must `dotnet build` here. Creating files is not enough. No user-story UI until build succeeds.

## Phase 2: User Story 1 — [title] (P1) 🎯 MVP

**Goal:** [what US-1 delivers]  
**Independent test:** [from spec]

### Implementation

- [ ] T010 [P] [US1] ViewModel + commands in {App}.Core/…ViewModel.cs
- [ ] T011 [P] [US1] View from the mapped Lumina recipe in {App}/Pages/…Page.xaml (ctor injection)
- [ ] T012 [US1] Append `.Map<…ViewModel, …Page>("name")` plus both `AddTransient` lines in {App}/MauiProgram.cs
- [ ] T013 [US1] Acceptance from spec US-1

**Checkpoint:** US-1 works alone.

## Phase 3: User Story 2 — [title] (P2)

**Goal:** [what US-2 delivers]  
**Independent test:** [from spec]

- [ ] T020 [P] [US2] ViewModel in {App}.Core/…ViewModel.cs
- [ ] T021 [US2] Lumina view in {App}/Pages/…Page.xaml (ctor injection)
- [ ] T022 [US2] Append `Map` + both transients in {App}/MauiProgram.cs + acceptance

**Checkpoint:** US-1 and US-2 both work independently.

## Phase N: Polish

- [ ] T900 Empty / error / busy (`NVEmptyView` / `NVBusyIndicator` / `NVToast`)
- [ ] T901 Follow `quickstart.md` on at least one TFM (or document why not)
- [ ] T902 No extra PackageReference; no permissive hardened opt-outs; no AppShell unless plan override

## Dependencies

- Foundational blocks every story
- Stories may proceed in P1 → P2 order after foundation
- `[P]` = different file, no unfinished dependency
- MVP = stop after User Story 1 and validate
