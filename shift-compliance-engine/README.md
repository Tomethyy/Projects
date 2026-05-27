# Shift Compliance Engine

All-in-One Shift Planning and Compliance Engine (Phase 0–4 skeleton + core flows).

## Stack

- **Backend:** .NET 8, PostgreSQL, EF Core, ASP.NET Core Identity + JWT
- **Web:** `web/employee-portal`, `web/planner-app` (Vite + React + TypeScript + react-i18next)
- **Shared client:** `web/shift-api-client` (`@shift-engine/api-client`, `file:` dependency from both apps)
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
   cd web/employee-portal; npm install; npm run dev -- --port 5173
   cd web/planner-app; npm install; npm run dev -- --port 5174
   ```

Default API URL in dev: `https://localhost:7xxx` or `http://localhost:5xxx` — check console output; set `VITE_API_URL` in each web app `.env` if needed.

## Phase 0 exit checklist

See [docs/PHASE0_CHECKLIST.md](docs/PHASE0_CHECKLIST.md).

## Pre-build / SecPlan import

- [docs/PREBUILD_CHECKLIST.md](docs/PREBUILD_CHECKLIST.md)
- [installer/secplan-import/README.md](installer/secplan-import/README.md)

## Companion mobile (Phase 4)

See [mobile/README.md](mobile/README.md).
