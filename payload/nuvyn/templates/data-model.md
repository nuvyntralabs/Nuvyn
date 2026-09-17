# Data model — [FEATURE NAME]

**Feature:** [link to spec.md]  
**Created:** [DATE]

Write this file **only** if the user asked to persist on device. Otherwise delete it and keep HttpForge / in-memory seed.

Engine (when asked): `Plugin.Maui.LocalStore` + `Nuventra.NuvexaDB` (`.nvx`). Do not use SQLite unless the user said so.

## Entities

### [Entity]

| Field | Type | Rules |
| --- | --- | --- |
| Id | string | required |
| | | |

**Relationships:** [none / belongs to …]

## LocalStore

- `Map<T>` types: [list]
- Path: `FileSystem.AppDataDirectory` / `app.nvx`
