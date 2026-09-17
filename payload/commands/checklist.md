# /nuvyn.checklist

Reviewer quality gate, locked to Nuvyntra. Write a **reviewer-owned** checklist from spec + plan. Do not write host code. Do not tick `checklists/requirements.md` (that file belongs to specify/clarify).

You **MUST** consider `$ARGUMENTS` (optional domain: ux, security, offline).

## User input

```text
$ARGUMENTS
```

Read `.nuvyn/feature.json`. Require `spec.md` and `plan.md`.

## Steps

1. Copy `.nuvyn/templates/checklist.md` → `checklists/<domain>.md` (`requirements` is reserved — use `ux.md`, `security.md`, `offline.md`, or `review.md`).
2. Each item is a requirements-quality question a reviewer can answer without running the app (`[x]` = the criterion is satisfied in the spec/plan, **not** that code exists).
3. Include: every US has a Lumina recipe; catalog packages match spec needs; persist/LocalStore only if asked; four TFMs; fail-closed rows when those plugins are in the plan; no invented `NV*View`; routes are `Map` names (no `AppShell` unless plan Complexity tracking records a spec MP-NAV override).
4. Do not mark items complete yourself unless the user is reviewing with you.

## Done when

- [ ] Checklist file written
- [ ] Completion reported (path + item count)

Next: `/nuvyn.task`.
