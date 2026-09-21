# /nuvyn.analysis

Read-only consistency check **after** `tasks.md` exists. Do not edit files. Do not write host code.

You **MUST** consider `$ARGUMENTS`.

## User input

```text
$ARGUMENTS
```

Read `.nuvyn/feature.json` and `.nuvyn/constitution.md`. Require `spec.md`, `plan.md`, and `tasks.md`. If tasks are missing, stop — run `/nuvyn.task`. Read `.nuvyn/reference/implement-recipes.md` for host shape. If `.nuvyn/adopt-report.md` exists, a plan that migrates MVVM / UI / HTTP without a user ask is CRITICAL.

## Passes (max 50 findings)

| ID prefix | Look for |
| --- | --- |
| D | Duplicate / conflicting FRs |
| A | Vague words with no measure; leftover `[NEEDS CLARIFICATION]` |
| U | Story or FR with zero tasks; task with no US/FR |
| C | Constitution MUST violated; catalog skipped with no Catalog gap; `AppShell` / `.UseShell()` / `{App}/ViewModels/` without Complexity tracking |
| I | Recipe / TFM / persist / route (`Map` vs Shell) drift vs plan |
| N | Naming only |

Severity: CRITICAL (constitution vs spec/plan, or AppShell / `.UseShell()` without a plan override, or zero-coverage P1) · HIGH · MEDIUM · LOW.

Print a table (ID, Category, Severity, Location, Summary, Recommendation) plus coverage % (FRs with ≥1 task). Offer remediation; do not apply it.

## Next action

- Any CRITICAL → **do not** run `/nuvyn.implement`. Name `/nuvyn.specify` / `/nuvyn.plan` / `/nuvyn.task`.
- Else → `/nuvyn.implement`.
- Workload / TFM / `UseMaui` / permission problems are environment findings — recommend `maui-dev doctor --path .`, not a spec rewrite.

## Done when

- [ ] Report printed (zero issues is OK)
- [ ] Next action named
