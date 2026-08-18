# Shift Compliance Engine

All-in-One Shift Planning and Compliance Engine (Phase 0–4 skeleton + core flows).

## Stack

- **Backend:** .NET 8, PostgreSQL, EF Core, ASP.NET Core Identity + JWT
- **Web:** `web/employee-portal`, `web/planner-app` (Vite + React + TypeScript + react-i18next)
- **Shared client:** `web/shift-api-client` (`@shift-engine/api-client`, `file:` dependency from both apps)
- **Shared UI:** `web/shift-ui` (`@shift-engine/ui` — tokens, Button/Card/Badge, `VirtualRosterGrid`)
- **Compliance:** `ShiftEngine.Compliance.ArbZG` (weekly hours, daily rest)
- **Replanning:** `ShiftEngine.Replanning` + `SickLeaveReplanService`

## Quick start (local)

1. Start PostgreSQL (or use Docker):

   ```powershell
   docker compose up -d postgres
   ```

2. Apply migrations and run API:

   ```powershell
   cd src/ShiftEngine.Api
   dotnet ef database update --project ../ShiftEngine.Infrastructure
   dotnet run
   ```

3. **First-time setup wizard** (empty DB only): creates tenant, admin, default shift tiers, optional **SMTP** fields, **AI key** (or placeholder), and **employee CSV invites** (`PersonnelNumber;DisplayName;Email` per line). Invited users receive `Employee` role; response JSON lists generated temporary passwords (dev/demo only — use proper invites in production).

   `POST http://localhost:5xxx/api/setup/wizard` — see OpenAPI `/swagger` or the planner app wizard form.

4. **Login:** `POST /api/auth/login` → use returned JWT as `Authorization: Bearer …` for other endpoints.

5. **Web UIs:** from repo root:

   ```powershell
   cd web/planner-app; npm install; npm run dev -- --port 5174
   cd web/employee-portal; npm install; npm run dev -- --port 5173
   ```

   - **Planner (Phase 2 UI):** [http://localhost:5174](http://localhost:5174) — sidebar nav, roster, deployment grid, personnel, positions, compliance, rules, audit.
   - **Employee portal:** [http://localhost:5173](http://localhost:5173)
   - **API / Swagger:** [http://localhost:5050/swagger](http://localhost:5050/swagger) (root `/` has no HTML UI)

   Status: [docs/PHASE2_STATUS.md](docs/PHASE2_STATUS.md)

## Phase 0 exit checklist

See [docs/PHASE0_CHECKLIST.md](docs/PHASE0_CHECKLIST.md).

## Phase 1 (Tier 1 roster)

- **Team month generate:** `POST /api/Roster/generate-team-month` with `{ "year": 2026, "month": 3 }` — all active employees, automatic 6/2–6/3 rhythm from each `contractedHoursMonthly`.
- **Matrix grid:** `GET /api/Roster/matrix?year=2026&month=3` — planner UI shows sortable employee × day table.
- **Publish:** `POST /api/Roster/publish` with `{ "rosterPeriodId": "…" }`.

Checklist: [docs/PHASE1_CHECKLIST.md](docs/PHASE1_CHECKLIST.md).

**Phase 1 tools in planner:** collapsible sections for compliance (ArbZG + BV), leave/carryover, SecPlan dry-run, and audit log. Employee portal shows **published** shifts only and auto-loads the selected month.

**UI overhaul (after Phase 1):** [docs/PHASE2_UI.md](docs/PHASE2_UI.md).

**Personnel & positions (Phase 1):** sample CSV in [installer/personnel/](installer/personnel/). Import via planner master-data sections or `POST /api/personnel/import` and `POST /api/personnel/positions/import`.

## Pre-build / SecPlan import

- [docs/PREBUILD_CHECKLIST.md](docs/PREBUILD_CHECKLIST.md)
- [installer/secplan-import/README.md](installer/secplan-import/README.md)

## Companion mobile (Phase 4)

See [mobile/README.md](mobile/README.md).
