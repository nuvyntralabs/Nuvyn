# /nuvyn.specify

Read `.nuvyn/reference/constraints.md` once. One feature spec (what/why) from **the user's requirements**. Host is always .NET MAUI (Android, iOS, Windows, Mac Catalyst). No package names. No `plan.md` / `tasks.md` / host code.

You **MUST** consider `$ARGUMENTS` before proceeding. Empty args → stop and ask for a description.

## User input

```text
$ARGUMENTS
```

## Steps

1. **Short name** — 2–4 words, action-noun (`user-auth`, `booking-inbox`). Preserve acronyms.
2. **Feature directory** — next `specs/NNN-short-name` (3-digit, scan existing). Or the `specs/…` path in args. Create it. One feature per run.
3. Copy `.nuvyn/templates/spec.md` → `spec.md`. Fill Intent, P1/P2 stories (Independent test + Given/when/then), Edge cases, MP table, FR-00n, SC-00n. Leave none as a placeholder. Stay catalog-free (no `Plugin.Maui.*` / `NV*` names). **MP-NAV must name a start route and chrome:** NavigationPage (default) vs tabs vs flyout. Do not leave MP-NAV as “tabs / flyout / deep links” only — plan must not invent Shell.
4. Write `.nuvyn/feature.json`: `{ "feature_directory": "specs/NNN-short-name" }`.
5. Identify actors, flows, mobile needs (offline, permissions, lock, PII, background, navigation). Testable FRs. User-facing success criteria (no `Plugin.Maui.*` / `NV*` names).
6. At most **three** `[NEEDS CLARIFICATION]` markers. Guess the rest; record guesses under Assumptions.
7. Copy `.nuvyn/templates/requirements.md` → `checklists/requirements.md`. Fill the **Pass** column with `pass` or `fail` (not `[x]` / `[ ]`). Validate the spec against each row (max 3 rewrite passes). Do not embed a checklist inside `spec.md`.

## Completion report

Feature directory, `spec.md` path, checklist pass/fail, next command (`/nuvyn.clarify` if markers remain, else `/nuvyn.plan`).

## Done when

- [ ] `spec.md` written and checklist updated
- [ ] `.nuvyn/feature.json` points at the directory
- [ ] Completion reported
