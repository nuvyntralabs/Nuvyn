# /nuvyn.clarify

Read `.nuvyn/reference/constraints.md` once. Do not steer to outside UI kits. Questions follow **this** spec's domain.

You **MUST** consider `$ARGUMENTS` (optional focus, e.g. "lock behavior").

## User input

```text
$ARGUMENTS
```

Read `.nuvyn/feature.json`. Require `spec.md`. If missing, stop — run `/nuvyn.specify`.

## Steps

1. Find `[NEEDS CLARIFICATION]` and high-impact gaps (scope, security, permissions, offline, **MP-NAV start route + chrome**). Stay catalog-free.
2. If MP-NAV lacks a start route or NavigationPage vs tabs vs flyout, that is a question — so plan cannot invent Shell.
3. If no markers and MP-NAV already has start route + chrome, say so and stop. Next is `/nuvyn.plan`.
4. At most **five** questions, each A/B/C + Custom, presented together.
5. Wait for answers. Do not plan.
6. Replace markers. Update Assumptions. Re-check `checklists/requirements.md` (Pass column, not `[ ]`).

## Done when

- [ ] Markers resolved or none existed
- [ ] Checklist re-evaluated
- [ ] Completion reported

Next: `/nuvyn.plan`.
