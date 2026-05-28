# Tomethy's Stellar Reserve

Static website for **Tomethy's Stellar Reserve** — a Star Citizen fueling contractor and ship financing bank operated by Tomethy.

## Quick start

```bash
npm install
cp .env.example .env
# Edit .env with your Formspree ID and social links
npm run dev
```

Open [http://localhost:4321](http://localhost:4321).

## Configuration

| Variable | Description |
|----------|-------------|
| `PUBLIC_FORMSPREE_ID` | Formspree form ID or full URL for contract submissions |
| `PUBLIC_DISCORD_URL` | Discord invite URL |
| `PUBLIC_RSI_ORG_URL` | RSI organization page URL |

Update contact email in `src/config/site.ts`.

## Custom images

Replace placeholders with your own in-game screenshots:

1. Save a wide hero shot as `public/images/gemini-hero.png` (or `.jpg`)
2. Save a fleet card image as `public/images/gemini-card.png`
3. Update paths in `src/data/fleet.ts` if using different filenames

## Updating content

| Content | File |
|---------|------|
| Live status board | `src/data/status.json` |
| Refuel & banking rates | `src/data/rates.ts` |
| Fleet ships | `src/data/fleet.ts` |
| Stanton coverage | `src/data/locations.ts` |
| Testimonials | `src/data/testimonials.ts` |
| FAQ | `src/data/faq.ts` |
| Recruitment | `src/data/recruitment.ts` |

## Deploy

Build: `npm run build` → output in `dist/`

Deploy to **Netlify** or **Vercel** with:

- **Root directory:** `tomethys-stellar-reserve`
- **Build command:** `npm run build`
- **Publish directory:** `dist`

## Disclaimer

Fan site. Not affiliated with Cloud Imperium Games. Star Citizen® is a trademark of Cloud Imperium Games Corporation.
