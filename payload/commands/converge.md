# /nuvyn.converge

After `/nuvyn.implement`, compare the **current codebase** to spec + plan + tasks. Append remaining work. Do not rewrite existing tasks. Do not edit `spec.md` / `plan.md`. Do not write app code.

You **MUST** consider `$ARGUMENTS`.

## User input

```text
$ARGUMENTS
```

Require `spec.md`, `plan.md`, `tasks.md`. If missing, stop. Constitution is law (`.nuvyn/constitution.md`).

## Build first

Run `dotnet build` on the solution (or MAUI csproj) **before** appending. A failed build is a CRITICAL finding. Append a fix-build task first. Do not append story work on top of a red build.

## Assess

For each FR / US acceptance / plan screen / constitution MUST, inspect code in the paths named by plan/tasks.

| Gap | Meaning |
| --- | --- |
| missing | Not in the code |
| partial | Started, not done |
| contradicts | Conflicts with spec/plan/constitution |
| unrequested | Extra work not in the artifacts (append a review task; do not delete code) |

Severity: CRITICAL (constitution MUST, P1 missing, failed `dotnet build`, AppShell without override) · HIGH · MEDIUM · LOW.

Print a findings table **before** writing.

## Write (append-only)

If findings exist: scan max task ID `M` and max phase `N`. Append:

```markdown
## Phase N+1: Convergence

- [ ] T0xx <imperative> per <FR-003|US1/AC2|plan: screen|Constitution|build> (<gap>)
```

CRITICAL first. Never reuse IDs. Never edit earlier tasks.

If nothing remains: **do not touch** `tasks.md`. Report "✅ Converged".

## Done when

- [ ] `dotnet build` result printed
- [ ] Findings table printed
- [ ] Tasks appended **or** file left unchanged
- [ ] Next: `/nuvyn.implement` (if appended) or review / PR (if converged)
