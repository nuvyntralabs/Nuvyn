# Feature specification: [FEATURE NAME]

**Feature directory:** [specs/NNN-slug]  
**Status:** draft  
**Created:** [DATE]  
**Input:** $ARGUMENTS

No package names. No `NV*` names. What / why only.

## Intent

[One paragraph: who this is for and why it exists.]

## Actors

| Actor | Goal |
| --- | --- |
| [role from the user's requirements] | [what they need to finish] |

## User stories *(mandatory)*

### US-1 — [title] (Priority: P1)

**As a** [actor], **I want** [action] **so that** [outcome].

**Why this priority:** [value]

**Independent test:** [how to verify this story alone]

**Acceptance**

1. **Given** [state], **When** [action], **Then** [outcome]
2. **Given** [state], **When** [action], **Then** [outcome]

### US-2 — [title] (Priority: P2)

**As a** [actor], **I want** [action] **so that** [outcome].

**Why this priority:** [value]

**Independent test:** [how to verify this story alone]

**Acceptance**

1. **Given** [state], **When** [action], **Then** [outcome]

[Add US-3+ as needed.]

### Edge cases

- What happens when [boundary]?
- How does the app handle [error / empty / offline]?

## Mobile product requirements

Product behaviors, not stack choices.

| ID | Area | Requirement |
| --- | --- | --- |
| MP-OFF | Offline | [what works with no internet; what waits] |
| MP-PERM | Permissions | [location / camera / notifications / Face ID — when and why] |
| MP-LOCK | App lock | [lock after background? only if the user asked] |
| MP-SEC | Sensitive data | [tokens, PII, screenshots] |
| MP-BG | Background | [what must survive process death — only if the user asked] |
| MP-NAV | Navigation | Start route: [name, e.g. signin]. Chrome: NavigationPage (default) / tabs / flyout. Destinations: [list]. Do not imply Shell unless chrome is flyout |

## Functional requirements *(mandatory)*

- **FR-001**: System MUST [testable behavior]
- **FR-002**: Users MUST be able to [key interaction]

## Key entities *(if the feature has data)*

- **[Entity]**: [what it represents; no storage engine]

## Out of scope

- [explicit non-goals]

## Success criteria *(mandatory)*

- **SC-001**: [user-facing, measurable, no framework names]
- **SC-002**: [user-facing, measurable]

## Assumptions

- [defaults you took so the spec can proceed]
- Extra packages (LocalStore, AppLock, …) are **out** unless the user asked

## Open questions

[At most three. Remove the section if none.]

- [NEEDS CLARIFICATION: …]
