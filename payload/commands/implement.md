# /nuvyn.implement

Execute `tasks.md` in order on the host (`nuvyn init`, or the existing app attached by `nuvyn adopt`). You **MUST** consider `$ARGUMENTS` (`Implement only Foundation`). Empty args → remaining unchecked tasks.

## User input

```text
$ARGUMENTS
```

Require `.nuvyn/feature.json`, `spec.md`, `plan.md`, `tasks.md`, `.nuvyn/constitution.md`. Load `research.md` / `data-model.md` if present. Read `.nuvyn/reference/implement-recipes.md`.

## Block (CRITICAL)

Halt and do **not** write host code if any of these hold:

- Constitution MUST vs spec/plan with no Complexity tracking override
- `AppShell` / `.UseShell()` in plan or tasks without a Complexity tracking row
- Last `/nuvyn.analysis` reported CRITICAL

Name the finding. Next: `/nuvyn.plan` or `/nuvyn.task`.

## Checklist gate (read-only)

Scan `checklists/*.md` **except** `requirements.md`. Count `[ ]` vs `[x]`. If any item is unchecked, **ask** before continuing. Do not tick checklist files.

`checklists/requirements.md` uses a **Pass** column (`pass` / `fail`). Do **not** treat `pass` (or the word Pass) as `[ ]`. Do not block implement on that file.

## Rules

1. Read `.nuvyn/reference/constraints.md` and implement-recipes. If `.nuvyn/adopt-report.md` exists, follow its Keep column: same MVVM, same UI, same HTTP. Do not add MVVMExpress / UIKit / HttpForge or rewrite `HttpClient` unless the user asked. Else UIKit first. Catalog first. No invented APIs / `NV*View` / `AppShell`.
2. Register every `plan.md` `UseX`. No PackageReference not in the plan.
3. Fail-closed: no `PermissiveMode`, `AllowUnmappedPayloadRoutes`, or `RequireHttps = false` unless spec + plan both require it.
4. Sequential tasks in order. `[P]` may run together only on different files.
5. Halt if a non-parallel task fails.
6. **Build is the gate.** Creating a file is not enough. Do not mark `[X]` because a path exists.
7. After **Foundation** (Phase 1), run `dotnet build` on the solution (or MAUI csproj). Halt on failure. Do not start user-story UI until it succeeds.
8. If the build fails on workloads, TFMs, permissions, `UseMaui`, or signing, run `maui-dev doctor --path .`. Do **not** re-run `nuvyn init`. If `maui-dev` is missing: `dotnet tool install -g Plugin.Maui.MauiDev.Cli --source https://api.nuget.org/v3/index.json`.
9. Do not `dotnet nuget push`.

## Completion report

Tasks completed, files changed, `dotnet build` result, remaining IDs, next `/nuvyn.implement` or `/nuvyn.converge`.

## Done when

- [ ] In-scope tasks marked `[X]` after a successful gate (Foundation: `dotnet build`) or a clear halt reason
- [ ] Completion reported
