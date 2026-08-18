# Phase 1 exit checklist (Tier 1 core roster)

Sign off when verified locally or on staging.

| ID | Criterion | Verified |
|----|-----------|----------|
| P1-1 | `POST /api/Roster/generate-team-month` builds one month for **all active employees** (no per-person ID in planner) | ☐ |
| P1-2 | Rotation is **automatic** (6/2 then 6/3, scaled by `ContractedHoursMonthly` from personnel data) | ☐ |
| P1-3 | `GET /api/Roster/matrix?year=&month=` returns team grid (employees × days) | ☐ |
| P1-4 | Planner matrix: sort by personnel no., name, monthly hours; edit shift tier per cell | ☐ |
| P1-5 | `POST /api/Roster/publish` marks period published; employee portal shows published shifts for linked user | ☐ |
| P1-6 | Leave carryover freeze + locked carryover read (`/api/Leave/*`) | ☐ |
| P1-7 | ArbZG baseline evaluate + BV audit endpoints (`/api/Compliance/*`) | ☐ |
| P1-8 | Works Council auditor role read-only (403 on writes) | ☐ |
| P1-9 | SecPlan import dry-run (`POST /api/import/secplan/dry-run`) | ☐ |
| P1-10 | Personnel CSV import/export + editable grid (`/api/personnel/*`) | ☐ |
| P1-11 | Positions CSV import/export + gender/headcount rules (`/api/personnel/positions/*`, `/api/deployment/posts`) | ☐ |

## Master data (personnel & positions)

Templates: [installer/personnel/](../installer/personnel/)

1. Edit `personnel-template.csv` (hours, gender, role — **no daily post**).
2. Import in planner **Personnel file** or `POST /api/personnel/import`.
3. Edit `positions-template.csv` (headcount, min female/male).
4. Import in planner **Positions** or `POST /api/personnel/positions/import`.

## Planner workflow (Phase 1)

1. Sign in as Admin/Planner.
2. Import **positions** (Mappe1) and **personnel** if not already in DB.
3. Choose **year** + **month** → **Generate team month** (rotation + post assignment).
4. Review matrix (tier + post label per cell) → **Publish draft**.
5. Existing month without posts? **Assign posts** button re-runs post assignment only.
4. Employees see their rows in the portal (month view).
5. **Phase 1 tools** (collapsible): compliance, leave/carryover, SecPlan dry-run, audit log.
6. Portal auto-loads current month; shows hint when draft exists but is unpublished.

## Phase 2 UI

Modern redesign is **deferred** until this checklist is signed off. See [PHASE2_UI.md](./PHASE2_UI.md).

## Data ownership

- **Contracted hours / personnel master:** personnel CSV + grid in planner (`/api/personnel/*`); not tied to daily post assignment.
- **SynComNet:** remains operational; this app uses shadow planning metadata (`LegacySource`, `LegacyReferenceMode`).

## Next (Phase 2a)

Sick-leave auto-replan, daily ledger as planner home, deployment grid MVP.
