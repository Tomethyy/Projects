# Phase 2 status (Tier 2 — usable product)

Last verified: local dev stack (API 5050, planner 5174, portal 5173).

## Delivered

### Modern planner UI (`web/planner-app`)
- React Router + sidebar shell; consumes **`@shift-engine/ui`** shared package
- **VirtualRosterGrid**: row virtualization, arrow-key nav, Shift+click multi-select, bulk tier apply
- Lazy-loaded route chunks
- Routes: Roster, Deployment grid, Personnel, Positions, Compliance, **Exceptions (sick/replan)**, Leave, Import, Rules, Audit, Setup
- Dedicated login page with JWT redirect + session user in top bar
- **Draft publish banner** on roster when period is unpublished
- Editable master data: personnel CSV + row grid, positions CSV + row grid
- Editable shift tiers (Rules page → `PUT /api/shift-tiers/{id}`)
- Compliance: evaluate, BV checklist, propose/apply fixes (ArbZG)
- Deployment grid: posts × days staffing view
- **Exceptions page:** record sick/call-out, propose/apply replan via ledger + `/api/replan/*`

### Employee portal (`web/employee-portal`)
- `portal-theme.css`, login hero image, roster month view, published-only filter
- **Mini calendar** with shift-day highlights and day filter
- **Draft empty-state card** when unpublished draft exists
- **Mobile shift cards** (table on wider screens)

### Backend (Phase 1 completion)
- Team month generate with stagger + post assignment
- `GET /api/roster/deployment-grid`
- `POST /api/compliance/propose-fixes`, `apply-fixes` (**ReassignAssignment** + RemoveAssignment)
- `PATCH /api/roster/assignments/tiers/bulk` for bulk tier edits
- `ShiftTiersController` for tier editing
- Audit on roster/personnel imports
- Daily rest rule fixed for overnight shift end times

## Phase 2 acceptance (from PHASE2_UI.md)

| Criterion | Status |
|-----------|--------|
| Planner design system on main routes | Done (roster, personnel, positions, compliance, audit, exceptions) |
| Portal mobile-friendly roster | Done (cards + calendar, no forced horizontal scroll) |
| P1 API regressions | Verify via `dotnet test` |

## Still out of scope / future

- Full `@shift-engine/ui` migration of every planner page (Card/Button on all routes)
- Compliance tier-change fixes (reassign/remove only today)
- Onboarding / shift-ban / shift-transition rules
- BV checklist auto-enforcement

## Run locally

```powershell
docker compose up -d postgres
cd src/ShiftEngine.Api; dotnet run
cd web/planner-app; npm run dev -- --port 5174
cd web/employee-portal; npm run dev -- --port 5173
```

Login: `admin@example.com` / `StrongPassword123.`
