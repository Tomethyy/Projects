# Phase 2 — Modern UI overhaul

Phase 1 keeps **functional, collapsible** planner and portal screens wired to all Tier‑1 APIs. After Phase 1 sign-off, replace the scaffold UI with a product-grade web app.

## Goals

- Single coherent design system (typography, spacing, color, components).
- App shell: sidebar nav, top bar (tenant, user, locale), responsive layout.
- Planner as primary surface: roster matrix as home; master data in secondary routes.
- Employee portal: calendar/month view, mobile-friendly, clear published vs draft states.

## Planner (5174)

| Area | Phase 1 | Phase 2 target |
|------|---------|----------------|
| Auth | Inline forms on same page | Dedicated login route; redirect after JWT |
| Navigation | Stacked cards | Routes: Roster, Personnel, Positions, Compliance, Leave, Import, Audit, Settings |
| Roster grid | HTML table + tier dropdown | Virtualized grid, keyboard nav, bulk edit, publish banner |
| Master data | CSV textarea + tables | Spreadsheet-style editors, inline validation, import wizard |
| Phase 1 tools | `<details>` sections | Compliance dashboard, leave admin, SecPlan upload modal |
| Exceptions | Debug-style block | Phase 2a: sick/replan workflow (separate epic) |

## Employee portal (5173)

| Area | Phase 1 | Phase 2 target |
|------|---------|----------------|
| Roster | Table after manual load | Auto-load current month; mini calendar |
| Draft state | Text hint | Empty state + “not published yet” card |
| Branding | Minimal CSS | Tenant logo, hero, accessible contrast |

## Technical approach

1. **Shared UI package** under `web/` (e.g. `@shift-engine/ui`): Button, Card, Table, Badge, PageLayout, tokens.
2. **Router**: React Router in planner + portal; lazy-loaded route chunks.
3. **Styling**: Tailwind or CSS modules with design tokens; dark mode optional.
4. **i18n**: Keep `react-i18n`; move strings out of components into locale files only.
5. **API client**: Extend `@shift-engine/api-client` with typed helpers (upload, pagination) instead of raw `fetch` in components.

## Out of scope for UI-only phase

- Sick-leave auto-replan (Phase 2a backend).
- Deployment grid MVP.
- Daily ledger as planner home.

## Acceptance (Phase 2 UI done)

- [ ] Planner matches design system on roster, personnel, positions, compliance, audit routes.
- [ ] Portal usable on phone width; roster readable without horizontal scroll on common dates.
- [ ] No Phase 1 API regressions; existing checklist P1-1…P1-11 still pass.
