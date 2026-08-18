# Personnel & positions master data (Phase 1)

Editable semicolon CSV files for local testing. Import via planner **Master data** or API.

## Personnel file (`personnel-template.csv`)

No daily post assignment — employees rotate positions in the plan later.

| Column | Example | Notes |
|--------|---------|--------|
| PersonnelNumber | 1001 | Unique per tenant |
| DisplayName | Max Mustermann | |
| ContractedHoursMonthly | 174 | Drives 6/2–6/3 rotation (130 part-time) |
| GenderCode | M / F / D / X | For post gender rules (Phase 2 assignment) |
| PrimaryRole | Security / LSKP | |
| Email | optional | Not used by import (wizard invites separately) |
| ExternalLegacyId | SEC-1001 | SecPlan reference |

**API:** `POST /api/personnel/import` with `{ "csvText": "..." }`  
**Export:** `GET /api/personnel/export`

## Positions file (`positions-template.csv`)

Deployment posts: headcount and gender minimums when a post is staffed.

| Column | Example |
|--------|---------|
| Name | Haupteingang Tag |
| WindowStart / WindowEnd | 06:00 / 14:00 |
| RequiredHeadcount | 2 |
| MinRequiredFemale | 1 |
| MinRequiredMale | 0 |
| GenderIrrelevant | 0 = gender rules apply, 1 / irrelevant / unerheblich = sex not considered |
| RequiredQualificationCode | SCHUER |
| BufferPercent | 10 |

**API:** `POST /api/personnel/positions/import` with `{ "csvText": "...", "replaceAllPositions": true }`  
**Export:** `GET /api/personnel/positions/export`

Replace-all import clears existing posts for the tenant before loading the file.
