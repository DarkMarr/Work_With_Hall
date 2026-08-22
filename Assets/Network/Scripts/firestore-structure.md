/users/{userId}
├── userId: (Firebase Authentication UID)
│   ├── profileData:
│   │   ├── gameUid: (generated unique ID = 6 alphanumeric characters) (~2,1xx million)
│   │   ├── profileName: "Player123"
│   │   ├── bodyType: 0
│   │   ├── rank: "Bronze"
│   │   ├── createdAt: (Timestamp)
│   │   ├── lastLogin: (Timestamp)
│   ├── energy:
│   │   ├── current: 5
│   │   ├── max: 10
│   │   ├── regenRate: 1
│   │   ├── regenIntervalSeconds: 300 // 5min
│   │   ├── lastUpdateTimestamp: 
│   ├── multiPlayerStats
│   │   ├── score: 100
│   │   ├── gamesPlayed: 10
│   │   ├── gamesWon: 5
│   │   ├── rank: 90
│   ├── singlePlayerStats
│   │   ├── {category}  (General, Geography, History, Pop Culture, Entertainment, Science, Sports)
│   │   │   ├── score: 100
│   │   │   ├── gamesPlayed: 10
│   │   │   ├── gamesWon: 5
│   │   │   ├── rank: 90
│   │   ├── {category}
│   ├── inventory {
│   │       ├── gems: 100
│   │       ├── coins: 10000
│   │       ├── materials: {
                - leave: 1
                - fire: 1
                - crystal: 2
                - pebble: 2
                - heart: 2
            }
│   │   }
│   ├── /inventoryItems (Subcollection)
│   │   ├── item123: { (Document ID)
│   │   │   ├── itemId: "owl_wind_spirit"
│   │   │   ├── name: "Owl the wind spirit"
│   │   │   ├── type: "decoration"
│   │   │   ├── quantity: 1
│   │   │   ├── acquiredAt: (Timestamp)
│   │   │   ├── equipped: true
│   │   │ }
│   ├── /roomDecorations (Subcollection for individual room decoration placements)
│   │   ├── slot_1: {
│   │   │   ├── decorationId: "deco_table_wood"
│   │   │   ├── placedAt: (Timestamp)
│   │   │ }
│   │   ├── decor_id_2: {
│   │   │   ├── decorationId: "deco_chair_fancy"
│   │   │   ├── placedAt: (Timestamp)
│   │   │ }
│   ├── /achievements (Subcollection for achievements earned by this player)
│   │   ├── achievement_id_1: { (Document ID, e.g., "first_win")
│   │   │   ├── achievementId: "first_win"
│   │   │   ├── earnedAt: (Timestamp)
│   │   │   ├── progress: 1 (if it's a tiered achievement)
│   │   │ }
│   ├── /friends (Subcollection for player's friends list, allows quick lookup)
│   │   ├── friend_uid_1: {
│   │   │   ├── userId: "friend_uid_1"
│   │   │   ├── username: "FriendOne"
│   │   │   ├── rank: "Bronze"
│   │   │   ├── friendRequest: "pending", "accepted"
│   │   │ }
│   │   ├── friend_uid_2: { ... }



