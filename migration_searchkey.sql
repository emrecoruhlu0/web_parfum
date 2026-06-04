CREATE EXTENSION IF NOT EXISTS unaccent;
CREATE EXTENSION IF NOT EXISTS pg_trgm;

CREATE OR REPLACE FUNCTION perfume_normalize(text) RETURNS text
LANGUAGE sql IMMUTABLE STRICT PARALLEL SAFE AS $$
  SELECT lower(
    regexp_replace(
      public.unaccent('public.unaccent', coalesce($1,'')),
      '[^a-z0-9]+', ' ', 'gi'
    )
  )
$$;

ALTER TABLE "Perfumes" DROP COLUMN IF EXISTS "SearchKey";
ALTER TABLE "Perfumes" ADD COLUMN "SearchKey" text
  GENERATED ALWAYS AS (
    perfume_normalize(
      coalesce("Name",'')        || ' ' ||
      coalesce("Brand",'')       || ' ' ||
      coalesce("TopNotes",'')    || ' ' ||
      coalesce("MiddleNotes",'') || ' ' ||
      coalesce("BaseNotes",'')   || ' ' ||
      coalesce("Accord1",'')     || ' ' ||
      coalesce("Accord2",'')     || ' ' ||
      coalesce("Accord3",'')     || ' ' ||
      coalesce("Accord4",'')     || ' ' ||
      coalesce("Accord5",'')
    )
  ) STORED;

ALTER TABLE "Perfumes" DROP COLUMN IF EXISTS "SearchKeyName";
ALTER TABLE "Perfumes" ADD COLUMN "SearchKeyName" text
  GENERATED ALWAYS AS (
    perfume_normalize(coalesce("Name",'') || ' ' || coalesce("Brand",''))
  ) STORED;

CREATE INDEX IF NOT EXISTS ix_perfumes_search_key_trgm
  ON "Perfumes" USING gin ("SearchKey" gin_trgm_ops);
CREATE INDEX IF NOT EXISTS ix_perfumes_search_key_name_trgm
  ON "Perfumes" USING gin ("SearchKeyName" gin_trgm_ops);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260525175130_AddPerfumeSearchIndex', '9.0.0')
ON CONFLICT ("MigrationId") DO NOTHING;
