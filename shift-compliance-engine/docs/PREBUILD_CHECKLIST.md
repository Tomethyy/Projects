# Pre-build checklist (SecPlan + operations)

Complete **before** treating Tier 1 roster as production-ready for pilot data.

| Track | Item | Owner | Done |
|-------|------|-------|------|
| A1 | SecPlan export samples (personnel + shift objects) archived for column mapping | Ops / IT | ☐ |
| A2 | Rotation and post rules documented (6/2, 6/3, 174h, LSKP vs Security weighting) | Ops | ☐ |
| B1 | Betriebsvereinbarung excerpts relevant to rest periods, Sunday work, consultation | HR / BR | ☐ |
| B2 | Pilot sign-off scope (which sites, read-only vs shadow planning) | Management | ☐ |
| C1 | Golden sick-leave scenarios (`tests/golden/sick-leave`) exercised on staging | QA | ☐ |

**Changelog (new app only):** publishing a roster in this platform records `RosterPeriod` metadata and optional `LegacySource` (for example `SecPlan` for shadow comparison). It does **not** update SynComNet SecPlan.
