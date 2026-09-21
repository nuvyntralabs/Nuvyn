# Changelog

## 1.2.0

- `nuvyn adopt` attaches the slash chain to an existing MAUI app. Writes `.nuvyn/`, skills, and `adopt-report.md` only. Does not change host architecture, UI kit, or HTTP. `nuvyn check` skips greenfield HostProof when `mode` is `adopt`.

## 1.1.1

- When `maui-dev doctor` returns findings (exit 1), say it found issues. Do not phrase a successful doctor run as “exited 1”.

## 1.1.0

- After a successful `nuvyn init` (and on `nuvyn check` inside a project), call `maui-dev doctor` when that tool is on PATH. Missing MauiDev is a warning, not a Nuvyn failure. Floor: maui-dev 1.2.0. Do not pass `--no-update-check` (MauiDev 1.2.1 rejects it). Print doctor output when it exits non-zero.

## 1.0.1

- Interactive nuget.org self-update check every 4 hours (`--no-update-check` or `NUVYNTRA_NO_UPDATE_CHECK=1` to skip). Cache: `~/.nuvyntra/cli-updates.json`. The CLI does not phone home.

## 1.0.0

- `nuvyn init`, `update`, `version`, `check`
- Embedded MVVMExpress + UIKit host and Spec Kit coding-agent slash chain
