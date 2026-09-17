# Research — [FEATURE NAME]

**Feature:** [link to spec.md]  
**Created:** [DATE]

Extra PackageReference rows **only** if the user asked. Default host is already MVVMExpress, UIKit, HttpForge, FormValidation, KeyboardManager.

## Decisions

### Default host

- **Decision:** Keep the five init packages and `UseNavigationPage` + `Map`. Add nothing. No `AppShell`.
- **Rationale:** User did not ask for another capability. Host is NavigationPage.
- **Alternatives:** LocalStore, AppLock, Observability, MAUI Shell — rejected until asked (Shell also needs Complexity tracking).

### [Extra package — only if asked]

- **Decision:** [package id]
- **Rationale:** [quote the user ask]
- **Alternatives:** [catalog neighbors you did not pick]
