# Character PSB Export Spec

How cat, dog and any new fashion art must be authored so that one decoration sprite can be
worn by every character without being repositioned per species.

Written 2026-09-30 after measuring the current art. Read the **Why** section before changing
anything — the rabbit is the character that is already correct, and cat and dog are the ones
that need to change.

## Why

Every character prefab is a paper doll: a fixed set of GameObjects, each with a
`SpriteResolver` that swaps its sprite by *category* (which body slot) and *label* (which
variant). The GameObject transform never moves. That means **a sprite's own pivot decides
where it is drawn.**

Measured bounds of the current prefabs at scale 1:

| Prefab | Height x Width |
| --- | --- |
| `UC_rabbit_Chloe` | 3.00 x 3.00 |
| `UC_rabbit_Enzo` | 3.00 x 3.00 |
| `UC_rabbit_Momo` | 3.00 x 3.00 |
| `UC_rabbit_Yawn` | 3.00 x 3.00 |
| `UC_cat_Mint` | 2.27 x 1.53 |
| `UC_cat_Ben` | 2.13 x 1.57 |
| `UC_dog_Milo` | 2.29 x 1.59 |
| `SR_cat_Draco` | 2.97 x 2.72 |

All four rabbits measure an identical perfect square. Four different characters cannot
genuinely be square to the centimetre — 3.00 x 3.00 is the **frame**, not the animal. The cats
and dogs report their true drawn silhouette instead.

The source files agree:

```
001_Rabbit_04072026.psb  ->  every sprite 600x600
002_Cat_04072026.psb     ->  315x312, 186x160, 88x123, 136x17, ...
003_Dog _04072026.psb    ->  420x249, 185x160, 97x90, ...
```

Import settings are identical across all three (PPU 100, pivot 0.5/0.5, `characterMode: 1`,
`documentAlignment: 7`, `layerMappingOption: 2`), so this is a property of the PSB files
themselves, not of how Unity imports them.

Two consequences:

1. **The rabbit looks smaller in game.** Its 3.00 frame is mostly transparent margin, so the
   drawn rabbit inside is well under 3.00, while a cat at 2.27 is 2.27 of solid cat.
2. **Shared fashion will land in the wrong place on cats and dogs.** With pivot 0.5/0.5 on a
   cropped sprite, the pivot is the centre of *that sprite's own bounding box*. Swap in a tall
   hat and a small hairpin and both centre themselves on the same point instead of sitting
   where they were drawn. On the rabbit, every part shares the one 600x600 frame and therefore
   one common origin, so anything drawn in that frame lands exactly where the artist put it.

The rabbit convention is the correct one for this system.

## Rules for every layer

1. **Document is 600 x 600.** Unchanged.

2. **Every exported layer must span the full 600 x 600 canvas.** Unity's PSD Importer trims
   each layer to its opaque bounds, so a layer with a transparent margin gets cropped and loses
   the shared origin. Give each layer a full-canvas fill at alpha 1/255 (visually invisible,
   enough to defeat the trim), or another full-canvas marker, so the exported sprite stays
   600x600. Verify after import: in the Sprite Editor every sprite's rect must read
   `600 x 600`.

3. **Draw each species at a consistent visual size inside the frame.** The frame being equal
   does not make the animals equal — that is still the artist's call, but now it is a visual
   judgement rather than an accident of cropping.

4. **Edit the existing `.psb` files. Do not create new ones.** Unity derives each sprite's ID
   from the PSD layer ID. Re-saving the same document keeps those IDs, so prefabs and sprite
   libraries keep working. A rebuilt-from-scratch file gets new IDs and every character
   silently loses its sprites — the cat PSB alone carries 397 layer entries.

5. **Commit and push before re-importing.** The re-import rewrites `.meta` files in place and
   is awkward to undo by hand.

## Naming

### Categories — flexible

`CharacterSpriteUtilities.NormalizeCategoryName` lowercases the name and strips spaces and
underscores, so `Head_dec`, `Head Dec`, `Head_Decor` and `HeadDecoration` all resolve to the
same slot. Existing category names do not need to change.

Slots the code knows (`CharacterPartType`):

```
Mount  Head  Body  Eyebrow  Eye  Mouth
Arm_Left  Arm_Right  Leg_Left  Leg_Right  Tail
Head_Decor  Arm_Decor  Body_Decor  Back_Decor
```

### Labels — must match exactly

Labels are **not** normalized. `CharacterSpriteMixer.SetPartLabel` passes the label straight
through to `SpriteResolver.SetCategoryAndLabel`, and `HasLabel` compares it verbatim. For one
fashion item to be wearable by every character, **the label string must be byte-identical in
the rabbit, cat and dog libraries.**

Today they are not:

| Slot | Rabbit | Cat |
| --- | --- | --- |
| `Head_dec` | `Head_Dec0000`, `Head_Dec0001`, `Head_Dec0002` | `pin001`, `hat`, `glass001`, `prop`, `Layer 38` |
| `Body_dec` | `Body_Dec0001`, `Body_Dec0002`, `Body_Dec0003` | `outfit`, `lower` |
| `Arm_dec` | `Arm_Dec0000`, `Arm_Dec0001`, `Arm_Dec0002` | `prop`, `prop`, `prop` |
| `Back_dec` | `Back_Dec0000` .. `Back_Dec0003` | `Layer 6` |

Not one name is shared, and the cat has duplicate labels (`prop` three times, `hat` twice),
which makes those variants unaddressable.

**Adopt the rabbit's scheme** — it is the only consistent one already in the project:

```
Head_Dec0000, Head_Dec0001, Head_Dec0002, ...
Body_Dec0001, Body_Dec0002, Body_Dec0003, ...
Arm_Dec0000,  Arm_Dec0001,  Arm_Dec0002,  ...
Back_Dec0000, Back_Dec0001, Back_Dec0002, ...
```

Four digits, zero padded, no spaces, no duplicates within a category. A given number must mean
the same garment in every species: if `Head_Dec0001` is the straw hat on the rabbit, it is the
straw hat on the cat and the dog too.

## New fashion items

A new item is one more label in the right category, added to **every** character's PSB in the
same pass:

1. Pick the next free number in that category, e.g. `Head_Dec0003`.
2. Draw it for each species in that species' own 600x600 frame, positioned to fit that body.
   The art differs per species; only the label is shared.
3. Add the layer to `001_Rabbit_04072026.psb`, `002_Cat_04072026.psb` and
   `003_Dog _04072026.psb`.
4. Full-canvas 600x600 per rule 2.

An item present on only some species will simply not appear on the others —
`CharacterSpriteMixer.HasLabel` returns false and the slot is left as-is, with no error.

For a first test set, three or four items in one category across all three species is enough to
prove alignment end to end.

## Checklist before handing the files back

- [ ] Every sprite rect reads `600 x 600` in the Sprite Editor, for rabbit, cat and dog
- [ ] Decoration labels follow `<Slot>_Dec####` and are identical across the three species
- [ ] No duplicate labels inside one category
- [ ] Files edited in place, same filenames, same layer IDs
- [ ] Repo committed and pushed before the re-import
