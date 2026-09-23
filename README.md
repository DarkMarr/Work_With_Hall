# QuizGame — Project Wiki

Unity 6000.1.5f1 mobile quiz game with Firebase backend, 2D sprite-mixing character customization, in-game store, and NPC-driven single/multiplayer gameplay.

---

## Table of Contents

1. [Architecture Overview](#architecture-overview)
2. [Module Reference](#module-reference)
3. [Character System Guide](#character-system-guide)
4. [Store System Guide](#store-system-guide)
5. [Database & Firestore Integration](#database--firestore-integration)
6. [NPC System](#npc-system)
7. [Scene Flow & Lifecycle](#scene-flow--lifecycle)
8. [Setup Instructions](#setup-instructions)

---

## Architecture Overview

The project uses **modular Assembly Definitions** (`.asmdef`) to enforce compile-time boundaries. Each feature lives in its own independently compiled assembly under `Assets/_Scripts/`.

### Dependency Graph (simplified)

```
┌─────────────────────────────────────────────────────────────────┐
│                        Scene Controllers                         │
│  QuizGame.MainMenu ─                                           │
│  QuizGame.Gameplay ─┼──────► QuizGame.Store                     │
│  QuizGame.MyRoom ───┘       QuizGame.Authentication              │
│                              QuizGame.Matchmaking                │
└──────┬──────────────────────────┬────────────────────┬───────────┘
       │                          │                    │
       ▼                          ▼                    ▼
┌─────────────┐  ┌────────────────────────────────────────────┐
│ QuizGame.   │  │           Shared Domain Modules             │
│ Character   │  │  QuizGame.Destination  QuizGame.Item        │
│             │  │  QuizGame.Currency     QuizGame.UI          │
│             │  │  QuizGame.Player       QuizGame.Material    │
└──────┬──────┘  └──────────┬─────────────────────┬───────────┘
       │                    │                     │
       ▼                    ▼                     ▼
┌──────────────────────────────────────────────────────────┐
│                   Infrastructure Layers                    │
│  QuizGame.Network    QuizGame.Resources                   │
│  QuizGame.Utilities  QuizGame.Interfaces                  │
│  QuizGame.Scene      QuizGame.Sound  QuizGame.Ads         │
└──────────────────────────────────────────────────────────┘
```

### Key Patterns

| Pattern | Usage |
|---------|-------|
| `MonoSingleton<T>` | All persistent managers (`PlayerDataManager`, `PlayerCharacterManager`, `UIManager`) |
| `ResourceManager<TManager, TContent>` | Loading ScriptableObject assets from `Resources/` by ID |
| `BaseUI` / `UIManager` | Prefab-based UI panels, loaded from `Resources/UI/` by type |
| `IHasID` | Required interface for any SO managed by a ResourceManager |
| `[Serializable]` controller classes | Non-MonoBehaviour controllers (`StoreController`, `TradeController`) owned by scene controllers |

---

## Module Reference

### Core Infrastructure

| Module | Location | Purpose |
|--------|----------|---------|
| `QuizGame.Utilities` | `_Scripts/Utilities/` | `MonoSingleton<T>`, `StringUtilities` |
| `QuizGame.Resources` | `_Scripts/Resources/` | `ResourceManager<T,U>` base, `IHasID` |
| `QuizGame.Interfaces` | `_Scripts/Interfaces/` | `IHasSprite`, `IHasName`, `IHasDescription` |
| `QuizGame.UI` | `_Scripts/UI/` | `UIManager`, `BaseUI` |
| `QuizGame.Scene` | `_Scripts/Scene/` | `SceneList` enum, scene initializers |
| `QuizGame.Network` | `Network/Scripts/` | Firebase Auth, Firestore, `PlayerDataManager` |

### Feature Modules

| Module | Location | Purpose |
|--------|----------|---------|
| `QuizGame.Character` | `_Scripts/Character/` | 2D sprite-mixing player characters, outfits, cosmetics |
| `QuizGame.Destination` | `_Scripts/Destination/` | Destinations, NPC definitions, destination resources |
| `QuizGame.Item` | `_Scripts/Item/` | Base items, equipment, decoration items, item selection UI |
| `QuizGame.Currency` | `_Scripts/Currency/` | `CurrencyType` enum |
| `QuizGame.Store` | `_Scripts/Store/` | In-game products, store controllers, purchase flow |
| `QuizGame.Gameplay` | `_Scripts/Gameplay/` | Quiz logic, gameplay controller, NPC spawn |
| `QuizGame.MainMenu` | `_Scripts/MainMenu/` | Main menu, matchmaking UI, leaderboard |
| `QuizGame.MyRoom` | `_Scripts/MyRoom/` | Room decoration, trading, equipping cosmetics |
| `QuizGame.Authentication` | `_Scripts/Authentication/` | Firebase sign-in/register, profile creation |
| `QuizGame.Matchmaking` | `_Scripts/Matchmaking/` | Multiplayer matchmaking sequence |
| `QuizGame.Ads` | `_Scripts/Ads/` | LevelPlay rewarded/interstitial ads |
| `QuizGame.Sound` | `_Scripts/Sound/` | Audio management |

### Third-Party / Plugins

| Package | Version | Purpose |
|---------|---------|---------|
| `com.unity.localization` | 1.5.4 | `LocalizedString` for all user-facing strings |
| `com.unity.2d.animation` | 12.0.2 | `SpriteLibrary` / `SpriteResolver` for character mixing |
| `com.unity.purchasing` | 5.0.4 | IAP (bundles, top-up) |
| NaughtyAttributes | 2.1.4 | Editor attributes (`[ReadOnly]`, `[ShowAssetPreview]`) |
| Firebase SDK | — | Auth + Firestore |
| DOTween | — | Animation/tweening |

---

## Character System Guide

### Architecture

```
CharacterInfoSO (ScriptableObject)
    └── references characterPrefab (e.g. "001_Rabbit")
            └── has CharacterSpriteMixer component
                    └── uses SpriteLibrary + SpriteResolver (Unity 2D Animation)

PlayerCharacterManager (MonoSingleton, DontDestroyOnLoad)
    ├── listens to SceneManager.sceneLoaded
    ├── finds PlayerSpawnPoint in active scene
    ├── instantiates prefab from CharacterInfoSO
    ├── attaches PlayerCharacter component
    ├── loads outfit from Firestore (via PlayerDataManager)
    └── applies cosmetics through CharacterSpriteMixer

CharacterCosmeticItemSO (extends EquipmentItemSO)
    ├── partType: CharacterPartType
    ├── spriteLabel: string (label in the sprite library)
    └── restrictedCharacterId: optional
```

### How It Works

1. **Sprite Mixing**: Each character prefab uses Unity's `SpriteLibrary` + `SpriteResolver` system. Each body part (head, eyes, arms, etc.) is a separate `SpriteResolver` on a child GameObject. The `CharacterSpriteMixer` maps `CharacterPartType` enums to resolvers.

2. **Category Name Normalization**: Different sprite libraries use different naming conventions (`"Arm L"`, `"Arm_Left"`, `"Arm_Dec"`). `CharacterSpriteUtilities` normalizes all category names (lowercase, strip spaces/underscores) and matches them via an alias dictionary.

3. **Outfit Persistence**: The equipped cosmetics are stored in Firestore as `profileData.equippedItems: Dictionary<string, string>` where key = `CharacterPartType` name, value = sprite label.

### Adding a New Character

1. Create the character prefab with `SpriteResolver` children (one per body part) in `Assets/Prefabs/Char/`.
2. Assign a `SpriteLibraryAsset` with appropriate categories/labels.
3. Create a `CharacterInfoSO`: **Assets → Create → QuizGame → Character → Character**
4. Place it in `Assets/Resources/Characters/` with name matching the prefab (e.g. `004_Bear.asset`). Where several prefabs share a name, match the one that actually carries the `CharacterSpriteMixer` — for the shipped characters that is `001_Rabbit` / `002_Cat` / `003_Dog`, **not** the `_base` variants, which have no mixer.
5. Add a `PlayerCharacter` component and `CharacterSpriteMixer` to the prefab (or use the editor tool).

### Adding a Cosmetic Item

1. Create: **Assets → Create → QuizGame → Character → Cosmetic Item**
2. Set the fields:
   - **Part Type**: which body part this applies to (e.g. `HeadDecoration`)
   - **Sprite Label**: the exact label name in the character's sprite library category
   - **Restricted Character ID**: (optional) limit to one character; leave empty for universal
   - **Item ID / Sprite / Name**: standard item metadata from `BaseItemSO`
3. Create a corresponding `InGameProductMetadataSO` and place it in `Assets/Resources/InGameProducts/AvatarStore/` so the item reaches the game through the store. There is no general cosmetics catalog folder yet — no `ResourceManager` loads `CharacterCosmeticItemSO` on its own.

### Equipping Cosmetics at Runtime

```csharp
// Equip — cosmetics are only reachable through their store product,
// so look the product up by its productID and unwrap the item.
var product = AvatarStoreProductsResourceManager.Instance.GetResource("hat_red");
if (product?.GetItemProduct() is CharacterCosmeticItemSO cosmetic)
{
    await PlayerCharacterManager.Instance.EquipCosmetic(cosmetic);
}

// Unequip
await PlayerCharacterManager.Instance.UnequipPart(CharacterPartType.HeadDecoration);

// Change character
await PlayerCharacterManager.Instance.SetSelectedCharacter("003_Dog");
```

---

## Store System Guide

### Product Data Flow

```
InGameProductMetadataSO (ScriptableObject in Resources/)
    ├── product: BaseItemSO (the item being sold)
    ├── purchasedCurrency: CurrencyInfoSO (Gem or Coin)
    └── price: int

ResourceManager loads products at runtime:
    ├── AvatarStoreProductsResourceManager  → Resources/InGameProducts/AvatarStore/
    ├── ItemStoreProductsResourceManager    → Resources/InGameProducts/ItemStore/
    └── DecorationStoreProductsResourceManager → Resources/InGameProducts/DecorationStore/
```

### Purchase Flow (StoreController.PurchaseProduct)

```csharp
// 1. User clicks "Buy" in ItemStoreUI → fires OnPurchaseProduct event
// 2. StoreController.PurchaseProduct(product):
var currencyType = product.GetPurchasedCurrency().GetCurrencyType(); // Gem or Coin
var price = product.GetPrice();

// 3. Try to spend currency (checks balance in Firestore):
var success = await PlayerDataManager.Instance.TrySpendCurrency(currencyType, price);

// 4. If sufficient, grant item to inventory:
await PlayerDataManager.Instance.AddInventoryItem(item.GetID(), item.GetName(), ...);
```

### Adding a New Store Product

1. Create the item SO first (e.g. `CharacterCosmeticItemSO`, `EquipmentItemSO`, etc.)
2. Create product: **Assets → Create → QuizGame → ProductMetada**
3. Assign the item, currency type (`Coin` or `Gem`), and price.
4. Place in the appropriate `Resources/InGameProducts/<StoreName>/` folder.
5. The product automatically appears in the store UI via the ResourceManager.

### Store UI Architecture

| UI | Type | Used For |
|----|------|----------|
| `MainStoreUI` | Category selector | Top-level store tabs |
| `ItemStoreUI` | Generic item grid + buy button | Item store, Avatar store |
| `RoomStoreUI` | Decoration-specific grid | Room furniture/decor |
| `BundleStoreUI` | IAP bundle list | Premium bundles (real money) |
| `TopUpStoreUI` | Currency packs | Buy coins/gems (real money) |

---

## Database & Firestore Integration

### Document Structure

```
users/{userId}/
├── profileData: {
│     gameUid, profileName, bodyType,
│     characterId, equippedItems: {partName → spriteLabel},
│     createdAt, lastLogin
│   }
├── energy: { current, max, energyRegenRate, ... }
├── inventory: {
│     gems, coins,
│     materials: {materialId → count},
│     items: [{itemId, name, type, quantity, equipped, acquiredAt}]
│   }
├── multiPlayerStats: { ... }
├── singlePlayerStats: { category → { ... } }
├── achievements/ (subcollection)
└── friends/ (subcollection)
```

### PlayerDataManager API

The `PlayerDataManager` (singleton, DontDestroyOnLoad) provides:

| Category | Methods |
|----------|---------|
| **Lifecycle** | `EnsureUserDocumentExists()` |
| **Profile** | `GetProfileData()`, `UpdateProfileName()`, `UpdateProfileBodyType()`, `UpdateSelectedCharacter()`, `UpdateEquippedItems()`, `UpdateEquippedPart()` |
| **Energy** | `GetEnergyData()`, `UpdateEnergy()` |
| **Currency** | `GetInventory()`, `UpdateCurrencies()`, `TrySpendCurrency()`, `AddCurrency()` |
| **Items** | `AddInventoryItem()`, `RemoveInventoryItem()`, `GetInventoryItems()`, `SetInventoryItemEquipped()` |
| **Stats** | `GetSinglePlayerStats()`, `UpdateSinglePlayerStats()`, `GetMultiPlayerStats()`, `UpdateMultiPlayerStats()` |
| **Social** | `GetFriends()`, `AddFriend()`, `RemoveFriend()`, `GetAchievements()` |

All methods are `async Task<T>` and **fail-safe** (return `null`/`false` instead of throwing).

### Important: Document Creation

After every sign-in (email, Google, sign-up), `EnsureUserDocumentExists()` is called. This creates the user document with default data if it doesn't exist yet, because Firestore `UpdateAsync` fails on non-existent documents.

---

## NPC System

### Single Player Mode

When no destination is selected (single player "Library" or "Playground"), the `GameplayController` spawns a random NPC from `Resources/NPCs/`:

```csharp
var npcManager = NpcResourceManager.Instance;
var randomNpc = npcManager.GetRandomResource();
var npcPrefab = randomNpc.GetNpcPrefab();
// singlePlayerNpcPlaceHolder, not npcPlaceHolder: the multiplayer narrator slot
// sits under multiplayerScenario, which SetGameMode disables in single player.
Instantiate(npcPrefab, singlePlayerNpcPlaceHolder);
```

### Multiplayer Mode

NPC comes from the destination selected during matchmaking:
```csharp
var npcPrefab = GameplayController.SelectedDestinationInfo.GetNPCPrefab();
```

### Adding an NPC

1. Create an NPC prefab in `Assets/Prefabs/`.
2. Create: **Assets → Create → QuizGame → Destination → NPC**
3. Assign the prefab, name, description.
4. Place the SO in `Assets/Resources/NPCs/`.
5. The NPC is automatically available for random spawning in single player.

---

## Scene Flow & Lifecycle

```
Init → Authentication → MainMenu → ┬─► Gameplay (SinglePlayer)
                                   ├─► Gameplay (Multiplayer via Matchmaking)
                                   ├─► MyRoom
                                   └─► Store (UI overlay, no scene load)
```

| Scene | Key Components | Player Spawns? |
|-------|---------------|----------------|
| **Init** | Firebase init, scene manager | No |
| **Authentication** | `AuthenticationController`, login UI | No |
| **MainMenu** | `MainMenuController`, `MatchmakingController`, `PlayerSpawnPoint` | **Yes** |
| **Gameplay** | `GameplayController`, quiz UI, NPC placeholder, `PlayerSpawnPoint` | **Yes** |
| **MyRoom** | `MyRoomSceneController`, decoration, equip button, `PlayerSpawnPoint` | **Yes** |

The `PlayerCharacterManager` automatically spawns the player in any scene that contains an active `PlayerSpawnPoint` component.

---

## Setup Instructions

### Prerequisites

- Unity **6000.1.5f1** (6000.1 LTS)
- Firebase project with Auth (Email/Google/Apple) and Firestore enabled
- `google-services.json` / `GoogleService-Info.plist` in the correct location
- Sprite libraries (`.spriteLib`) assigned to character prefabs

### Initial Setup via Editor Tools

Access all setup tools via the menu bar: **QuizGame → Setup → ...**

| Menu Item | What It Does |
|-----------|-------------|
| Create Character SO Assets (Rabbit/Cat/Dog) | Generates `CharacterInfoSO` assets in `Resources/Characters/` from existing prefabs |
| Create NPC SO Assets | Scans for NPC prefabs and generates `NpcInfoSO` in `Resources/NPCs/` |
| Create Store Product Directories | Ensures `Resources/InGameProducts/AvatarStore/`, `ItemStore/`, `DecorationStore/` exist |
| Add PlayerSpawnPoint to Current Scene | Adds a `PlayerSpawnPoint` GameObject at origin in the open scene |
| Add CharacterSpriteMixer to Selected Prefab | Adds mixer + `PlayerCharacter` component and auto-discovers all `SpriteResolver` children |

### Recommended Setup Order

1. **Open any scene** → `QuizGame → Setup → Add PlayerSpawnPoint to Current Scene` → position it where the player should stand → save. Repeat for **MainMenu**, **Gameplay**, and **MyRoom** scenes.

2. **Select a character prefab** in the Project window → `QuizGame → Setup → Add CharacterSpriteMixer to Selected Prefab`. Verify the SpriteLibrary is assigned and categories were found (check the component in the Inspector).

3. `QuizGame → Setup → Create Character SO Assets` — this reads the prefabs and creates the ScriptableObjects. Verify the prefab references are correct in the generated assets.

4. `QuizGame → Setup → Create NPC SO Assets` — generates NPC resources from existing prefabs.

5. `QuizGame → Setup → Create Store Product Directories` — ensures all `Resources/InGameProducts/` subfolders exist.

### Creating Cosmetic Items for the Store

1. Create a cosmetic: **Assets → Create → QuizGame → Character → Cosmetic Item**
   - Set `partType`, `spriteLabel`, `restrictedCharacterId` (optional), `itemID`, display name, sprite.
2. Create a product wrapper: **Assets → Create → QuizGame → ProductMetada**
   - Set `product` = the cosmetic SO, `purchasedCurrency` = Coin or Gem, `price` = cost.
3. Place the product SO in `Assets/Resources/InGameProducts/AvatarStore/`.
4. Open the game → Store → Avatar Store tab — the item should appear automatically.

### Firestore Security Rules (development)

```
rules_version = '2';
service cloud.firestore {
  match /databases/{database}/documents {
    match /users/{userId} {
      allow read, write: if request.auth != null && request.auth.uid == userId;
    }
  }
}
```

### Troubleshooting

| Issue | Solution |
|-------|----------|
| Safe Mode: `LocalizedString not found` | Ensure the consuming `.asmdef` has reference GUID `eec0964c48f6f4e40bc3ec2257ccf8c5` (Unity.Localization) |
| Safe Mode: `ReadOnly` / `ShowAssetPreview` not found | Add reference GUID `776d03a35f1b52c4a9aed9f56d7b4229` (NaughtyAttributes.Core) |
| Player doesn't appear in scene | Check that a `PlayerSpawnPoint` exists in the scene with `spawnOnSceneLoad = true` |
| Cosmetic doesn't apply | Verify the `spriteLabel` matches a label in the character's sprite library for that category |
| Character SO not found at runtime | Must be in `Resources/Characters/` (case-sensitive folder name) |
| Purchase silently fails | Check that `EnsureUserDocumentExists()` ran after sign-in; Firestore `Update` on missing doc returns error |
| Assembly reference errors after adding new types | Check the `.asmdef` of both the providing and consuming modules; use the **target .asmdef.meta GUID**, not the folder GUID |
