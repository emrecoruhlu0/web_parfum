const prisma = require('../src/prisma/client');

const BATCH_SIZE = 50;
const DELAY_MS = 150; // rate limit: ~6 req/sec

const sleep = (ms) => new Promise(r => setTimeout(r, ms));

async function fetchImageUrl(name, brand) {
  const query = encodeURIComponent(`${brand.replace(/-/g, ' ')} ${name.replace(/-/g, ' ')}`);
  const url = `https://world.openbeautyfacts.org/cgi/search.pl?search_terms=${query}&search_simple=1&action=process&json=1&page_size=1`;

  try {
    const res = await fetch(url, { signal: AbortSignal.timeout(8000) });
    const data = await res.json();
    return data.products?.[0]?.image_url || null;
  } catch {
    return null;
  }
}

async function main() {
  // Get perfumes without imageUrl, ordered by popularity
  const perfumes = await prisma.perfume.findMany({
    where: { imageUrl: null },
    select: { id: true, name: true, brand: true },
    orderBy: { ratingCount: 'desc' },
  });

  console.log(`${perfumes.length} parfüm görseli aranacak...`);

  let found = 0;
  let checked = 0;

  for (let i = 0; i < perfumes.length; i += BATCH_SIZE) {
    const batch = perfumes.slice(i, i + BATCH_SIZE);

    for (const p of batch) {
      const imageUrl = await fetchImageUrl(p.name, p.brand);
      checked++;

      if (imageUrl) {
        await prisma.perfume.update({
          where: { id: p.id },
          data: { imageUrl },
        });
        found++;
      }

      if (checked % 100 === 0) {
        console.log(`  ${checked}/${perfumes.length} kontrol edildi, ${found} görsel bulundu`);
      }

      await sleep(DELAY_MS);
    }
  }

  console.log(`\nTamamlandı: ${checked} kontrol, ${found} görsel kaydedildi (${(found/checked*100).toFixed(1)}%)`);
  process.exit(0);
}

main().catch(e => { console.error(e); process.exit(1); });
