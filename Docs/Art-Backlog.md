# งาน Art ที่ค้าง

รวบรวม 2 ต.ค. 2026 · สำหรับส่งฝั่ง graphic
เรียงตามความเร่งด่วน ข้อที่ขึ้น 🔴 คือตัวบล็อกงาน dev

---

## ✅ 1. PSB ตัวละคร — เสร็จแล้วสำหรับทุกตัวที่เกมใช้จริง

ตรวจใหม่ 2 ต.ค. **ตัวละครทั้ง 12 ตัวใน catalog เป็น 600×600 ครบแล้ว** ทั้งแมว หมา กระต่าย

เหลือ 3 ไฟล์ที่ยัง crop อยู่ แต่**ไม่มีไฟล์ไหนบล็อกงาน**:

| ไฟล์ | สถานะ | ใครใช้ | ต้องแก้ไหม |
| --- | --- | --- | --- |
| `R_cat_Callie.psb` | 0/11 rect เป็น 600 | ยังไม่ขึ้น catalog | แก้ก่อนปล่อยตัวนี้ |
| `002_Cat_04072026.psb` | 62/173 | `CharacterSpriteMixer_TestScene` เท่านั้น | ไม่จำเป็น |
| `003_Dog _04072026.psb` | 37/150 | `CharacterSpriteMixer_TestScene` เท่านั้น | ไม่จำเป็น |

> **แก้ข้อมูลที่เคยแจ้งไป** — ก่อนหน้านี้ผมขึ้นไฟล์รวมสองตัวเป็น 🔴 ตัวบล็อก นั่นผิด
> ไล่ reference แล้วพบว่าไฟล์รวมป้อน `002_Cat_base.spriteLib` / `003_Dog _base.spriteLib`
> ซึ่งป้อนต่อไปแค่ `002_Cat_base.prefab` / `003_Dog _base.prefab` ที่**ไม่มีใครอ้างถึงเลย**
> ส่วน `002_Cat.prefab` / `003_Dog.prefab` ถูกใช้แค่ใน test scene
> catalog ทั้ง 12 ตัวใช้ PSB รายตัว (`UC_*`) ซึ่งแก้ครบแล้ว **ไม่ต้องไปยุ่งกับไฟล์รวม**

**R_* และ SR_* ที่เหลือแก้ครบแล้วเช่นกัน** — ตรวจ `R_cat_Duke`, `R_dog_Nacho`, `R_rabbit_Willy`,
`SR_cat_Draco`, `SR_dog_Keros`, `SR_rabbit_Yuki` ผ่านหมด เหลือ `R_cat_Callie` ตัวเดียว

วิธีแก้ `R_cat_Callie`: ใส่ fill เต็มผืน 600×600 ที่ **alpha 1/255** ทุกเลเยอร์
แล้วเช็คใน Sprite Editor ว่าทุก rect อ่าน `600 x 600`

> ⚠️ **ห้ามสร้างไฟล์ PSB ใหม่** sprite ID มาจาก layer ID ของ PSD สร้างใหม่ = reference หลุด
> เงียบ ๆ ทั้งหมด **ให้แก้ไฟล์เดิมเท่านั้น**

รายละเอียดเต็ม: `Docs/Character-PSB-Export-Spec.md`

---

## 🔴 2. Outfit ขาด 48 ชิ้น — 12 ชุด

ลิสต์เต็มพร้อม ID และชื่อไอเทมทุกชิ้น: **`Docs/Outfit-Art-Gap-2026-09-30.md`**

| Rarity | เอกสารมี | มี Art แล้ว | **ขาด** |
| --- | --- | --- | --- |
| Common | 24 | 24 | 0 ✅ |
| Uncommon | 36 | 12 | **24** |
| Rare | 36 | 12 | **24** |
| Ultra Rare | 28 | 28 | 0 ✅ |

**Uncommon ที่ขาด** — Rimทะเล, เด็กเรียน, ปาร์ตี้, นักบอล, ฤดูหนาว, ฮัมเพลง
**Rare ที่ขาด** — ขุนนาง, สาวใช้, ชาวร็อก, นักเวทย์, จิตกร, ตำรวจ

ทุกชุดต้องครบ 4 ช่อง: `hat` → Head, `lower`+`L_arm`+`R_arm` → Body, `prop` → Hand, `back` → Back

---

## 🔴 3. DECOR ขาดเกือบทั้งหมด — ประมาณ 83 ชิ้น

เอกสารมี **104 ชิ้น** แต่ Art ที่มีอยู่ประมาณ **21 ชิ้น**

| Type ในเอกสาร | เอกสารมี | โฟลเดอร์ที่น่าจะตรงกัน | ไฟล์ที่มี | **ขาด** |
| --- | --- | --- | --- | --- |
| Small | 48 | `room/ShelfDec` | 6 | **~42** |
| Big | 24 | `room/FloorDec` | 9 | **~15** |
| Window | 8 | `room/WallDec` | 6 | **~2** |
| Suitcase | 8 | *(ไม่มีโฟลเดอร์)* | 0 | **8** |

> การจับคู่ Type กับโฟลเดอร์เป็นการตีความของฝั่ง dev จาก `GAME DESIGN DOC 02` แท็บ `item type`
> ที่เขียนว่า wall = ของแขวน, shelf = ของวางโต๊ะเล็ก, floor = ของตั้งพื้นใหญ่
> **รบกวนยืนยันว่าจับคู่ถูก** ก่อนลงมือทำจำนวนมาก
>
> และ `Suitcase` ยังไม่ชัดว่าเป็นของแต่งห้องหรือของสวมใส่ — รอ Game Designer ตอบ
> (ดู `Docs/Design-Decisions-Pending.md` ข้อ 9) **อย่าเพิ่งทำจนกว่าจะเคาะ**

**24 ชิ้นที่เป็นของดรอป ควรทำก่อน** เพราะจำเป็นต่อ Lucky Draw — 4 ชิ้นต่อเมือง × 6 เมือง:

| เมือง | Uncommon Small | Rare Small | Rare Big | Ultra Rare Window |
| --- | --- | --- | --- | --- |
| Bangkok | ตุ๊กตาช้าง | รถตุ๊กตุ๊ก | ยักษ์ผู้พิทักษ์ | วิววัด |
| Tokyo | ตู้ไปรษณีย์ | เสาโทริอิ | วิวฟูจิ | โตเกียวทาวเวอร์ |
| New York | แฮมเบอร์เกอร์ | แท็กซี่เหลือง | เทพีเสรีภาพ | วิวเทพีเสรีภาพ |
| Paris | น้ำหอมสุดหรู | รูปโมนาลิซ่า | หอไอเฟล | วิวหอไอเฟล |
| London | ชุดน้ำชาสุดหรู | รถบัสแดง | บิ๊กเบน | วิวลอนดอน |
| Cairo | ตุ๊กตาอูฐ | พีระมิดจิ๋ว | โลงศพฟาโรห์ | วิวพีระมิด |

---

## 4. ROOM ขาดประมาณ 6 ชิ้น

เอกสารมี 18 ชิ้น (Common 6, Uncommon 6, Rare 4, Ultra Rare 2)
Art ที่มี: `C_0001-0003`, `UC_0001-0003`, `R_0001-0003`, `SR_0001-0003` = **12**

ไม่มีชิ้นไหนเป็น `DROP` จึงไม่บล็อก Lucky Draw

---

## 5. Avatar — ขอยืนยันจำนวน

เอกสารมี **16 ชิ้น** (`112001–134002`) แต่ catalog ในเกมมี **12 ตัว**

ยังไม่ได้ไล่ทีละตัวว่าขาดใคร ถ้าต้องการลิสต์เฉพาะเจาะจงบอกได้ จะเทียบให้

---

## 6. ของเล็ก ๆ ที่ควรเก็บ

### `back` ซ้ำสองชั้นใน 3 ไฟล์

`Fashion_R_Bangkok`, `Fashion_R_Tokyo`, `Fashion_SR_MagicalGirl` มีเลเยอร์ที่ map ไปช่อง `back`
สองชั้น ระบบเอาชั้นแรกและข้ามชั้นที่สองพร้อม warning

ถ้าตั้งใจให้มีสองชิ้นต้องตั้งชื่อเลเยอร์แยกกัน ถ้าไม่ได้ตั้งใจให้ลบชั้นที่เกิน

### ไฟล์เก่า 4 ตัวที่ชื่อไม่ตามแบบแผน

```
Assets/Art/Sprites/Fashion/Fashion_exported_Cset.psd
Assets/Art/Sprites/Fashion/Fashion_exported_SRset.psd
Assets/Art/Sprites/Fashion/FashionB_exported_Rset.psd
Assets/Art/Sprites/Fashion/FashionB_exported_UCset.psd
```

ระบบข้ามไปเพราะไม่ตรงรูปแบบ `Fashion_<Rarity>_<Set>` — **ลบทิ้งได้ไหม?** ยังไม่ได้ลบเพราะ
ไม่แน่ใจว่ายังใช้อยู่หรือเปล่า

### ชื่อ label ของ decoration ในตัวละครยังไม่ตรงกัน

| | Head_dec | Body_dec | Arm_dec | Back_dec |
| --- | --- | --- | --- | --- |
| กระต่าย (ถูก) | `Head_Dec0000/0001/0002` | `Body_Dec0001-0003` | `Arm_Dec0000-0002` | `Back_Dec0000-0003` |
| แมว | `pin001`, `hat`, `glass001`, `prop`, `Layer 38` | `outfit`, `lower` | `prop` ×3 | `Layer 6` |

ไม่ตรงกันสักชื่อ และแมวมี label ซ้ำ (`prop` สามตัว, `hat` สองตัว) ซึ่งทำให้เรียกใช้ตัวที่ซ้ำไม่ได้
ให้ยึดแบบกระต่าย `<Slot>_Dec####` เหมือนกันทุกสายพันธุ์

### App icon 0 จาก 38 ขนาด

ยังไม่มีสักขนาดเดียว จำเป็นต่อการขึ้น store ทั้ง App Store และ Google Play

---

## รูปแบบไฟล์ที่ต้องส่งมา (Fashion)

PSB หนึ่งไฟล์ต่อหนึ่งชุด วางใน `Assets/Art/Sprites/Fashion/`
ตั้งชื่อ `Fashion_<Rarity>_<SetName>.psb` เช่น `Fashion_UC_Party.psb`, `Fashion_R_Noble.psb`

- document **600×600** และ **ทุกเลเยอร์ต้องกินเต็มผืน** (fill alpha 1/255 กัน importer crop)
- ชื่อเลเยอร์ชุดเดิม: `hat` `lower` `L_arm` `R_arm` `prop` `back`
- ให้เห็นเฉพาะ group ของชุดนั้น group เดียว เพราะ importer ส่ง sprite เฉพาะเลเยอร์ที่เปิดอยู่

พอไฟล์เข้ามาแล้วฝั่ง dev รันสองเมนูนี้ ระบบจะสร้าง asset ให้เอง:

1. `QuizGame/Setup/Create Fashion Set SO Assets (from Fashion PSBs)`
2. `QuizGame/Setup/Create Fashion Reward Items + Fill Destination Pools`

---

## ลำดับที่แนะนำ

1. **DECOR 24 ชิ้นที่เป็นของดรอป** — บล็อก Lucky Draw (รอ Game Designer เคาะข้อ Common ก่อน)
2. **Outfit 48 ชิ้น** — ไม่บล็อก Lucky Draw แต่บล็อกร้านค้าและระบบคราฟต์
3. **DECOR ที่เหลือ + ROOM + Avatar**
4. **`R_cat_Callie.psb`** — ก่อนปล่อยตัวละครตัวนั้น ไม่เร่ง
5. **App icon** — ก่อนส่ง store
