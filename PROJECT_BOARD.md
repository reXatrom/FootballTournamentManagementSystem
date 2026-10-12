# Project Board

This repository board tracks work against the application rubric. The items below are implemented and build-verified; runtime and accessibility checks remain open where listed under **Next**.

## Implemented

| Rubric area | Work | Evidence |
| --- | --- | --- |
| Application function | Tournament, team, player, match, event, standings, and dashboard workflows are implemented. | Corresponding pages under `Components/Pages/` |
| Application function | Role-aware controls and protected create/edit pages are implemented. | ASP.NET Core Identity roles and page authorization |
| Error handling | Dashboard data failures show a logged, user-facing message with retry. | Dashboard load state |
| Error handling | Unhandled routed-page failures have an accessible recovery screen with retry. | Router `ErrorBoundary` |
| Documentation | Setup, configuration, roles, workflows, data validation, and build instructions are documented. | `README.md` |

## Next

| Rubric area | Task | Acceptance criteria |
| --- | --- | --- |
| Error handling | Add focused feedback for database failures during create, edit, and delete operations. | Failed saves remain on the form, preserve user input, and display a safe actionable message; details are logged. |
| Application design / UX | Review loading, empty, validation, and error states across all pages. | States are consistent, keyboard accessible, and understandable without color alone. |
| Application design / UX | Complete a WCAG 2.1 Level AA accessibility pass. | Keyboard navigation, focus visibility, form labels, contrast, and screen-reader announcements are checked and issues recorded. |
| Application function | Add automated tests for core tournament and match rules. | Tests cover valid operations and key invalid states and pass in the normal build workflow. |
| Documentation | Add a concise operator guide with screenshots and troubleshooting for database startup. | A new team member can configure the database, sign in, and complete a tournament workflow from the guide. |

## Board Maintenance

- Keep tasks small enough to verify in one focused review.
- Link implementation changes and test evidence from each completed task.
- For team collaboration, mirror these columns in the repository's GitHub Projects board and keep this file as the project overview.