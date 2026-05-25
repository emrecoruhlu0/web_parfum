using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebParfum.Migrations
{
    /// <inheritdoc />
    public partial class AddPerfumeSearchIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1) Extensions
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS unaccent;");
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS pg_trgm;");

            // 2) IMMUTABLE normalize wrapper. unaccent() default'ta IMMUTABLE değil;
            //    iki-parametreli formla bağlamı IMMUTABLE yaparız — GENERATED için şart.
            migrationBuilder.Sql(@"
CREATE OR REPLACE FUNCTION perfume_normalize(text) RETURNS text
LANGUAGE sql IMMUTABLE STRICT PARALLEL SAFE AS $$
  SELECT lower(
    regexp_replace(
      public.unaccent('public.unaccent', coalesce($1,'')),
      '[^a-z0-9]+', ' ', 'gi'
    )
  )
$$;");

            // 3) SearchKey: Name + Brand + Notes + Accord1..5 normalize edilmiş birleşim.
            migrationBuilder.Sql(@"
ALTER TABLE ""Perfumes"" DROP COLUMN IF EXISTS ""SearchKey"";
ALTER TABLE ""Perfumes"" ADD COLUMN ""SearchKey"" text
  GENERATED ALWAYS AS (
    perfume_normalize(
      coalesce(""Name"",'')        || ' ' ||
      coalesce(""Brand"",'')       || ' ' ||
      coalesce(""TopNotes"",'')    || ' ' ||
      coalesce(""MiddleNotes"",'') || ' ' ||
      coalesce(""BaseNotes"",'')   || ' ' ||
      coalesce(""Accord1"",'')     || ' ' ||
      coalesce(""Accord2"",'')     || ' ' ||
      coalesce(""Accord3"",'')     || ' ' ||
      coalesce(""Accord4"",'')     || ' ' ||
      coalesce(""Accord5"",'')
    )
  ) STORED;");

            // 4) SearchKeyName: sadece Name + Brand — ranking için ayrı kolon.
            migrationBuilder.Sql(@"
ALTER TABLE ""Perfumes"" DROP COLUMN IF EXISTS ""SearchKeyName"";
ALTER TABLE ""Perfumes"" ADD COLUMN ""SearchKeyName"" text
  GENERATED ALWAYS AS (
    perfume_normalize(coalesce(""Name"",'') || ' ' || coalesce(""Brand"",''))
  ) STORED;");

            // 5) GIN trigram indeksleri.
            migrationBuilder.Sql(@"
CREATE INDEX IF NOT EXISTS ix_perfumes_search_key_trgm
  ON ""Perfumes"" USING gin (""SearchKey"" gin_trgm_ops);");
            migrationBuilder.Sql(@"
CREATE INDEX IF NOT EXISTS ix_perfumes_search_key_name_trgm
  ON ""Perfumes"" USING gin (""SearchKeyName"" gin_trgm_ops);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ix_perfumes_search_key_trgm;");
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ix_perfumes_search_key_name_trgm;");
            migrationBuilder.Sql(@"ALTER TABLE ""Perfumes"" DROP COLUMN IF EXISTS ""SearchKeyName"";");
            migrationBuilder.Sql(@"ALTER TABLE ""Perfumes"" DROP COLUMN IF EXISTS ""SearchKey"";");
            migrationBuilder.Sql(@"DROP FUNCTION IF EXISTS perfume_normalize(text);");
            // Extension'lar bırakılır (DB-genel).
        }
    }
}
