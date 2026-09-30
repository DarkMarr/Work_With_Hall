# Item reference — OUTFIT (30 September 2026)

Source supplied by the owner: [ALL ITEM DATA / OUTFIT](https://docs.google.com/spreadsheets/d/15_KKnDDTVKIF29PtCILSZbBibvWfulZQf2vUM2wGW4E/edit?gid=1596582054#gid=1596582054).
Read through the Google Sheets connector on 2026-09-30: `OUTFIT!A1:AF35` and `OUTFIT!A36:K140`.
The source spreadsheet was not edited. This is a reference audit, not an approved migration or drop-table change.

## What the sheet defines

Columns A:K: ID, Rarity, Type, Set, Name (TH), EN name, JP name, Description TH/EN/JP, NOTE.
124 item rows occupy rows 2–125. Each item has its own numeric ID and a Head, Body, Hand or Back type.

| Rarity as written | Items |
| --- | ---: |
| Common | 24 |
| Uncommon | 36 |
| Rare | 36 |
| Ultra Rare | 28 |

| NOTE value as written | Items |
| --- | ---: |
| S.GOLD | 40 |
| CRAFT | 28 |
| S.GEM | 20 |
| DROP | 36 |

DROP rows 90–125 cover Bangkok, Tokyo, New York, Paris, London and Cairo: six items per city, two each Uncommon/Rare/Ultra Rare. The sheet does not provide the material quantity or drop probabilities. No Common item in this tab is marked DROP; reconcile this with the multiplayer probability table before replacing the current pools.

## Differences from the current prototype

- Runtime reward IDs such as `Fashion_C_Baseball` represent **whole sets**. The sheet identifies **individual pieces**: e.g. `211002` Red Cap, `221002` Black Shirt, `231002` Baseball Bat, `241002` Baseball Vibe (rows 6–9). Do not replace IDs already stored in inventory without a migration decision.
- The current six Common sets and four SuperRare sets are included in every city pool. Their corresponding sheet groups carry S.GOLD/CRAFT/S.GEM notes, not DROP. The current pools remain prototypes, not verified production catalog data.
- The sheet says **Ultra Rare**, while code/PSB prefixes say `SuperRare`/`SR`. Keep enum ordinals stable; confirm the intended label/mapping before migration.
- `Fashion_UC_<city>` and `Fashion_R_<city>` group artwork by set/PSB. The city items in the sheet split rarity and slot per individual piece. A set cannot safely inherit one rarity for all of those pieces.
- Some slot/name pairs need confirmation: `233008` is Hand but named ชุดยูกาตะ (row 99); `224006` is Body but named แอปเปิ้ลเคลือบน้ำตาล (row 100). Similar patterns occur for other cities. Preserve the source values; do not silently swap them.
- Source city Tokyo versus existing `Osaka.asset`/destination configuration needs an explicit mapping check before importing city-specific rewards.

## Missing source content and next import work

15 Thai names are blank: `211004`, `211005`, `211006`, `212001`, `212004`, `212005`, `232006`, `243002`, `213007`, `233007`, `224005`, `244005`, `213009`, `244009`, `222012`.
All 36 DROP rows have blank English/Japanese names/descriptions in the read range. Preserve blanks as pending; do not treat art notes in descriptions as finalized player-facing copy.

Next: author an explicit item-ID → PSB/layer → equipment slot mapping, resolve the slot/rarity questions, then import item names/descriptions into Item localization tables and create per-piece reward assets. Validate icons and pivots before wiring inventory/MyRoom. Existing PSBs and their layer IDs must be preserved.

For this audit round only, generated `EquipmentItemSO` assets with no localization reference use a readable set-name fallback (e.g. Baseball, New York). This prevents the Lucky Draw result/Accept flow from throwing; it does not claim the catalog migration is finished.
