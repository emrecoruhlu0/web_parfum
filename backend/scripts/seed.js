const fs = require('fs');
const readline = require('readline');
const prisma = require('../src/prisma/client');
const CSV_PATH = '/home/emreraspi/.cache/kagglehub/datasets/olgagmiufana1/fragrantica-com-fragrance-dataset/versions/3/fra_cleaned.csv';
const BATCH_SIZE = 500;

function parseYear(val) {
  const n = parseInt(val);
  return n > 1800 && n < 2100 ? n : null;
}

function parseRating(val) {
  if (!val) return null;
  const n = parseFloat(val.replace(',', '.'));
  return isNaN(n) ? null : n;
}

function clean(val) {
  if (!val || val.trim() === '' || val.trim().toLowerCase() === 'unknown') return null;
  return val.trim();
}

async function main() {
  const existing = await prisma.perfume.count();
  if (existing > 0) {
    console.log(`Veritabanında zaten ${existing} parfüm var. Seed atlanıyor.`);
    return;
  }

  console.log('CSV okunuyor...');
  const fileStream = fs.createReadStream(CSV_PATH, { encoding: 'latin1' });
  const rl = readline.createInterface({ input: fileStream, crlfDelay: Infinity });

  let header = null;
  let batch = [];
  let total = 0;
  let skipped = 0;

  for await (const line of rl) {
    if (!header) {
      header = line.split(';');
      continue;
    }

    const cols = line.split(';');
    if (cols.length < header.length) { skipped++; continue; }

    const row = {};
    header.forEach((h, i) => { row[h.trim()] = cols[i]; });

    const name = clean(row['Perfume']);
    const brand = clean(row['Brand']);
    if (!name || !brand) { skipped++; continue; }

    batch.push({
      name,
      brand,
      country: clean(row['Country']),
      gender: clean(row['Gender']),
      year: parseYear(row['Year']),
      ratingValue: parseRating(row['Rating Value']),
      ratingCount: parseInt(row['Rating Count']) || null,
      topNotes: clean(row['Top']),
      middleNotes: clean(row['Middle']),
      baseNotes: clean(row['Base']),
      accord1: clean(row['mainaccord1']),
      accord2: clean(row['mainaccord2']),
      accord3: clean(row['mainaccord3']),
      accord4: clean(row['mainaccord4']),
      accord5: clean(row['mainaccord5']),
    });

    if (batch.length >= BATCH_SIZE) {
      await prisma.perfume.createMany({ data: batch });
      total += batch.length;
      process.stdout.write(`\r${total} parfüm eklendi...`);
      batch = [];
    }
  }

  if (batch.length > 0) {
    await prisma.perfume.createMany({ data: batch });
    total += batch.length;
  }

  console.log(`\nTamamlandı: ${total} parfüm eklendi, ${skipped} satır atlandı.`);
}

main()
  .catch(console.error)
  .finally(() => prisma.$disconnect());
