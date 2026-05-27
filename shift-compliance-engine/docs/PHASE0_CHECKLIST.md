# Phase 0 exit checklist (go / no-go before Phase 1)

Sign off when verified on **staging** or local Docker.

| ID | Criterion | Verified |
|----|-----------|----------|
| P0-1 | `docker compose up` (postgres + api) or local `dotnet run` + Postgres | ☐ |
| P0-2 | CI green (`dotnet build`, `dotnet test`) | ☐ |
| P0-3 | Tenant isolation: two tenants cannot read each other's data | ☐ |
| P0-4 | Auth + roles: Admin login; Planner/Employee/Auditor roles return 403 where expected | ☐ |
| P0-5 | Setup wizard completes on empty DB (`POST /api/setup/wizard`) incl. optional SMTP, AI placeholder/key, CSV invites (`PersonnelNumber;DisplayName;Email`) | ☐ |
| P0-6 | DE/EN: API/UI locale switch without mixed strings | ☐ |
| P0-7 | Employee portal + planner shell load after login (happy path) | ☐ |
| P0-8 | Audit log written on critical admin actions (baseline) | ☐ |
| P0-9 | README documents run, env vars, E2E command | ☐ |

**Pre-build (before Phase 1 roster logic):** SecPlan sample exports + operations rulebook (Track A1–A2).
