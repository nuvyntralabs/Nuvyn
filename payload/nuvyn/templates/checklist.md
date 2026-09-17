# Review checklist: [DOMAIN]

**Purpose:** Requirements-quality review (not implementation complete)  
**Created:** [DATE]  
**Feature:** [link to spec.md]

`[x]` means the spec/plan already satisfies the criterion. It does **not** mean the code is done.

## Coverage

- [ ] Every user story maps to a Lumina recipe in `plan.md`
- [ ] Every extra package is justified by a spec need (or Catalog gap)
- [ ] Persist / LocalStore appears only if the user asked
- [ ] Four TFMs named; native plugins not sold as Windows
- [ ] Fail-closed settings present for DeepLinks / PushRouter / SmartUpload / FeatureFlags when selected
- [ ] Routes are `Map` names; no `AppShell` unless Complexity tracking records a spec MP-NAV override

## Clarity

- [ ] No leftover `[NEEDS CLARIFICATION]`
- [ ] Acceptance criteria are Given / when / then
- [ ] Out of scope is explicit

## Notes

- Incomplete items must be fixed in `/nuvyn.specify` or `/nuvyn.plan` before `/nuvyn.implement`
