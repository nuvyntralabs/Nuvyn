# Quickstart — [FEATURE NAME]

**Host:** `.` (created by `nuvyn init`, or an existing app attached by `nuvyn adopt`)

Restore and compile first. A file on disk is not a run.

```bash
dotnet restore
dotnet build
```

Optional: `dotnet tool install -g Plugin.Maui.MauiDev.Cli --source https://api.nuget.org/v3/index.json` then `maui-dev doctor`.

## Android emulator

API **24+** (host `SupportedOSPlatformVersion` 24 — same as `dotnet new maui` in .NET 10). Avoids AndroidX merge failures and desugaring crashes on API 21–23.

```bash
dotnet build {App}/{App}.csproj -f net10.0-android -t:Run
```

Host loopback from the emulator is `10.0.2.2` (not `127.0.0.1`). Example API: `http://10.0.2.2:5174/`.

## Android USB device

`127.0.0.1` on the device is the phone, not the PC. Either:

```bash
adb reverse tcp:5174 tcp:5174
```

and use `http://127.0.0.1:5174/`, or use the PC’s LAN IP.

Fast Deployment often fails on USB. Non-incremental install:

```bash
dotnet build {App}/{App}.csproj -f net10.0-android -t:Run -p:EmbedAssembliesIntoApk=true -p:AndroidEnableAssemblyCompression=false
```

Do not leave `EmbedAssembliesIntoApk=true` in the csproj unless you intend every build to embed.

## Other TFMs

```bash
dotnet build {App}/{App}.csproj -f net10.0-ios
dotnet build {App}/{App}.csproj -f net10.0-maccatalyst
dotnet build {App}/{App}.csproj -f net10.0-windows10.0.19041.0
```

## Validate the feature

1. [Launch the start route from plan.md]
2. [US-1 independent test]
3. [Empty / error / busy visible]

Do not `dotnet nuget push`. Do not add packages here.
