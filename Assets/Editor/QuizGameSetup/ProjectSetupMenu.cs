using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using QuizGame.Character;
using QuizGame.Destination;
using QuizGame.Store;

namespace QuizGame.Editor.Setup
{
    /// <summary>
    /// Editor menu for setting up the project's ScriptableObject assets and scene configuration.
    /// Use these menu items after importing new character prefabs or NPC prefabs to quickly
    /// generate the required SO assets and configure scenes.
    /// </summary>
    public static class ProjectSetupMenu
    {
        private const string MENU_ROOT = "QuizGame/Setup/";

        #region Character Assets

        [MenuItem(MENU_ROOT + "Create Character SO Assets (Rabbit/Cat/Dog)")]
        public static void CreateCharacterAssets()
        {
            var resourcePath = "Assets/Resources/Characters";
            EnsureDirectoryExists(resourcePath);

            // Find character prefabs in the project.
            var rabbitPrefab = FindPrefabByPartialName("001_Rabbit");
            var catPrefab = FindPrefabByPartialName("002_Cat");
            var dogPrefab = FindPrefabByPartialName("003_Dog");

            CreateCharacterSO(resourcePath, "001_Rabbit_base", rabbitPrefab);
            CreateCharacterSO(resourcePath, "002_Cat_base", catPrefab);
            CreateCharacterSO(resourcePath, "003_Dog_base", dogPrefab);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[QuizGameSetup] Character SO assets created in Resources/Characters.");
        }

        private static void CreateCharacterSO(string path, string characterName, GameObject prefab)
        {
            var assetPath = $"{path}/{characterName}.asset";
            if (File.Exists(assetPath))
            {
                Debug.Log($"[QuizGameSetup] Character SO '{characterName}' already exists, skipping.");
                return;
            }

            var so = ScriptableObject.CreateInstance<CharacterInfoSO>();
            // Use SerializedObject to set private fields via reflection-free approach.
            var serializedObj = new SerializedObject(so);
            var characterIDProp = serializedObj.FindProperty("characterID");
            if (characterIDProp != null)
            {
                characterIDProp.stringValue = characterName;
            }
            var prefabProp = serializedObj.FindProperty("characterPrefab");
            if (prefabProp != null && prefab != null)
            {
                prefabProp.objectReferenceValue = prefab;
            }
            serializedObj.ApplyModifiedPropertiesWithoutUndo();

            AssetDatabase.CreateAsset(so, assetPath);
            Debug.Log($"[QuizGameSetup] Created Character SO: {assetPath}");
        }

        #endregion

        #region NPC Assets

        [MenuItem(MENU_ROOT + "Create NPC SO Assets (from existing NPC prefabs)")]
        public static void CreateNpcAssets()
        {
            var resourcePath = "Assets/Resources/NPCs";
            EnsureDirectoryExists(resourcePath);

            // Find NPC prefabs (they are used in Destination SOs).
            var npcPrefabGuids = AssetDatabase.FindAssets("t:GameObject", new[] { "Assets/Prefabs" });
            int created = 0;

            foreach (var guid in npcPrefabGuids)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                if (!assetPath.Contains("NPC") && !assetPath.Contains("npc")) continue;

                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                if (prefab == null) continue;

                var npcName = prefab.name.Replace("(Clone)", "").Trim();
                var soPath = $"{resourcePath}/{npcName}.asset";

                if (File.Exists(soPath))
                {
                    Debug.Log($"[QuizGameSetup] NPC SO '{npcName}' already exists, skipping.");
                    continue;
                }

                var so = ScriptableObject.CreateInstance<NpcInfoSO>();
                var serializedObj = new SerializedObject(so);
                var npcIDProp = serializedObj.FindProperty("npcID");
                if (npcIDProp != null) npcIDProp.stringValue = npcName;
                var prefabProp = serializedObj.FindProperty("npcPrefab");
                if (prefabProp != null) prefabProp.objectReferenceValue = prefab;
                serializedObj.ApplyModifiedPropertiesWithoutUndo();

                AssetDatabase.CreateAsset(so, soPath);
                created++;
                Debug.Log($"[QuizGameSetup] Created NPC SO: {soPath}");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[QuizGameSetup] Created {created} NPC SO assets in Resources/NPCs.");
        }

        #endregion

        #region Store Product Directories

        [MenuItem(MENU_ROOT + "Create Store Product Directories")]
        public static void CreateStoreProductDirectories()
        {
            EnsureDirectoryExists("Assets/Resources/InGameProducts/AvatarStore");
            EnsureDirectoryExists("Assets/Resources/InGameProducts/ItemStore");
            EnsureDirectoryExists("Assets/Resources/InGameProducts/DecorationStore");

            AssetDatabase.Refresh();
            Debug.Log("[QuizGameSetup] Store product directories created in Resources/InGameProducts/.");
        }

        #endregion

        #region Scene Setup

        [MenuItem(MENU_ROOT + "Add PlayerSpawnPoint to Current Scene")]
        public static void AddPlayerSpawnPointToScene()
        {
            var existingSpawn = Object.FindFirstObjectByType<PlayerSpawnPoint>();
            if (existingSpawn != null)
            {
                Debug.Log("[QuizGameSetup] PlayerSpawnPoint already exists in this scene.");
                Selection.activeGameObject = existingSpawn.gameObject;
                EditorGUIUtility.PingObject(existingSpawn.gameObject);
                return;
            }

            var spawnGO = new GameObject("PlayerSpawnPoint");
            spawnGO.AddComponent<PlayerSpawnPoint>();
            spawnGO.transform.position = Vector3.zero;

            Undo.RegisterCreatedObjectUndo(spawnGO, "Create PlayerSpawnPoint");
            Selection.activeGameObject = spawnGO;

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log("[QuizGameSetup] PlayerSpawnPoint added to scene. Position it where the player should appear.");
        }

        [MenuItem(MENU_ROOT + "Add CharacterSpriteMixer to Selected Prefab")]
        public static void AddCharacterSpriteMixerToSelected()
        {
            var selected = Selection.activeGameObject;
            if (selected == null)
            {
                Debug.LogWarning("[QuizGameSetup] No GameObject selected. Select a character prefab first.");
                return;
            }

            // Check if already has the component.
            var existingMixer = selected.GetComponentInChildren<CharacterSpriteMixer>();
            if (existingMixer != null)
            {
                Debug.Log($"[QuizGameSetup] '{selected.name}' already has a CharacterSpriteMixer.");
                Selection.activeGameObject = existingMixer.gameObject;
                return;
            }

            // Add PlayerCharacter component (which requires CharacterSpriteMixer).
            var mixerGO = new GameObject("CharacterSpriteMixer");
            mixerGO.transform.SetParent(selected.transform, false);
            var mixer = mixerGO.AddComponent<CharacterSpriteMixer>();

            // Auto-find resolvers.
            mixer.AutoFindResolveSpriteInChildren();

            var playerChar = selected.GetComponent<PlayerCharacter>();
            if (playerChar == null)
            {
                playerChar = selected.AddComponent<PlayerCharacter>();
            }

            EditorUtility.SetDirty(selected);
            Debug.Log($"[QuizGameSetup] CharacterSpriteMixer + PlayerCharacter added to '{selected.name}' with {mixer.GetCategories().Length} categories found.");
        }

        #endregion

        #region Helpers

        private static void EnsureDirectoryExists(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                var parent = Path.GetDirectoryName(path)?.Replace("\\", "/");
                var folderName = Path.GetFileName(path);
                if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
                {
                    EnsureDirectoryExists(parent);
                }
                AssetDatabase.CreateFolder(parent, folderName);
            }
        }

        private static GameObject FindPrefabByPartialName(string partialName)
        {
            var guids = AssetDatabase.FindAssets("t:GameObject", new[] { "Assets/Prefabs" });
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.Contains(partialName))
                {
                    return AssetDatabase.LoadAssetAtPath<GameObject>(path);
                }
            }
            Debug.LogWarning($"[QuizGameSetup] Prefab matching '{partialName}' not found in Assets/Prefabs.");
            return null;
        }

        #endregion
    }
}
