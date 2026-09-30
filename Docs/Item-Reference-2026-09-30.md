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

## รหัสไอเทม (ตรวจ 30 ก.ย. 2026)

จาก sheet `1c6CRIdE...` ซึ่งเจ้าของระบุว่าเป็น item ทั้งหมดที่มีในเวอร์ชันนี้

```
2 1 1 001
│ │ │  └── ลำดับในกลุ่ม
│ │ └───── rarity : 1=Common  2=Uncommon  3=Rare  4=Ultra Rare
│ └─────── slot   : 1=Head  2=Body  3=Hand  4=Back
└───────── หมวด   : 1=Avatar 2=Outfit 3=Decor 4=Room 5=Material 6=Ticket 7=Quiz Item
```

| หมวด | ช่วง ID | จำนวน |
| --- | --- | --- |
| Avatar | 112001–134002 | 16 |
| Outfit | 211001–244010 | 86 |
| Decor | 311001–344006 | 104 |
| Room | 401001–404002 | 12 |
| Material | 501001–501005 | 5 |
| Ticket | 602001–602002 | 2 |
| Quiz Item | 712001–732003 | 6 |

ชื่อภายในเป็น `outfit_<slot>_<rarity>_<nn>` เช่น `outfit_head_c_01` = Propeller Hat

### Material

| ID | Name EN | ที่มา |
| --- | --- | --- |
| 501001 | Fine Fabric | ย่อย outfit |
| 501002 | Blueprint | ย่อย decor |
| 501003 | Hardwood | ดรอปจาก quiz |
| 501004 | Shiny Crystal | ดรอปจาก quiz |
| 501005 | Paint Bucket | ดรอปจาก quiz |

คอลัมน์ `Recycle To` ของ outfit ทุกชิ้นชี้ไป `501001` ตรงกับ DOC 02 ที่ระบุว่า fabric
ได้จากการย่อย item fashion

### ข้อสรุปที่ใช้ตัดสินใจไปแล้ว

**Outfit มี 4 ช่องเท่านั้น: Head / Body / Hand / Back** ค้นทั้ง sheet ไม่มีไอเทมที่เกี่ยวกับ
แขนหรือแขนเสื้อเลยสักชิ้น เลเยอร์ `L_arm` / `R_arm` ใน PSB จึงเป็นส่วนหนึ่งของชุด Body
ไม่ใช่ไอเทมแยก และ `Hand` ในเอกสารคือช่องเดียวกับ `Prop` ในโค้ด (DOC 02 เขียนว่า
`arm 腕 = ของถือ`)

### ยังไม่ได้ทำ

ID ของ `FashionItemSO` ตอนนี้เป็นชื่อที่ generator ตั้งเอง (`Fashion_C_Summer__HeadDecoration`)
**ยังไม่ผูกกับรหัสจริงในเอกสาร** (`211001` / `outfit_head_c_01`) ถ้าจะให้ client กับข้อมูล
ดีไซน์อ้างอิงตัวเดียวกันได้ ต้องเคาะว่าจะใช้รหัสไหนเป็นหลักแล้วทำตารางเทียบ
