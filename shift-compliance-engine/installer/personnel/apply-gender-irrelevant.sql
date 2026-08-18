ALTER TABLE "DeploymentPosts"
  ADD COLUMN IF NOT EXISTS "IsGenderIrrelevant" boolean NOT NULL DEFAULT false;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260531120000_DeploymentPostGenderIrrelevant', '8.0.11')
ON CONFLICT DO NOTHING;
