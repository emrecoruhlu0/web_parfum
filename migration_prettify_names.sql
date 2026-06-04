-- Perfume Name/Brand alanlarını slug formatından okunabilir Title Case'e çevirir.
-- Örn: "jean-paul-gaultier" -> "Jean Paul Gaultier", "xerjoff" -> "Xerjoff"
-- Idempotent: tekrar çalıştırmak güvenlidir (yalnızca dönüşüm sonucu farklı olan satırları günceller).
-- Not: "SearchKey" / "SearchKeyName" GENERATED kolonları Name/Brand'e bağlı olduğundan
-- bu UPDATE sonrası otomatik yeniden hesaplanır.

UPDATE "Perfumes"
SET "Name" = initcap(regexp_replace(replace(replace("Name", '-', ' '), '_', ' '), '\s+', ' ', 'g'))
WHERE "Name" IS DISTINCT FROM
      initcap(regexp_replace(replace(replace("Name", '-', ' '), '_', ' '), '\s+', ' ', 'g'));

UPDATE "Perfumes"
SET "Brand" = initcap(regexp_replace(replace(replace("Brand", '-', ' '), '_', ' '), '\s+', ' ', 'g'))
WHERE "Brand" IS DISTINCT FROM
      initcap(regexp_replace(replace(replace("Brand", '-', ' '), '_', ' '), '\s+', ' ', 'g'));
