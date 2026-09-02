#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using QuizGame.UI.Audit;

namespace QuizGame.Editor.UI
{
    public class UIAuditorWindow : EditorWindow
    {
        private UIAuditReport report;
        private Vector2 scroll;

        [MenuItem("HALL900/UI Auditor")]
        public static void Open()
        {
            GetWindow<UIAuditorWindow>("HALL900 UI Auditor");
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("HALL900 UI Auditor - Phase 1", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Phase 1 scans Scene/Prefab UI and, while Play Mode is running, the runtime UI instantiated by the game. It reports UI structure and assigned graphics, and discovers reference images. It does not modify scenes or graphic resources.",
                MessageType.Info);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Audit Scene UI", GUILayout.Height(28)))
                {
                    AuditSceneUI();
                }

                using (new EditorGUI.DisabledScope(!Application.isPlaying))
                {
                    if (GUILayout.Button("Audit Runtime UI", GUILayout.Height(28)))
                    {
                        AuditRuntimeUI();
                    }
                }

                if (GUILayout.Button("Refresh References", GUILayout.Height(28)))
                {
                    RefreshReferences();
                }
            }

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Runtime UI audit requires Play Mode because this project instantiates many UI prefabs through UIManager at runtime.", MessageType.Warning);
            }

            EditorGUILayout.Space(8);

            if (report == null)
            {
                EditorGUILayout.LabelField("No audit run yet.");
                return;
            }

            EditorGUILayout.LabelField("Mode", report.auditMode);
            EditorGUILayout.LabelField("Scene", report.sceneName);
            EditorGUILayout.LabelField("Path", report.scenePath);
            EditorGUILayout.LabelField("Canvas", report.canvasCount.ToString());
            EditorGUILayout.LabelField("UI Elements", report.uiElementCount.ToString());
            EditorGUILayout.LabelField("Images", $"{report.imageWithSpriteCount}/{report.imageCount} assigned");
            EditorGUILayout.LabelField("Images Missing Graphic", report.imageMissingSpriteCount.ToString());
            EditorGUILayout.LabelField("Buttons", report.buttonCount.ToString());
            EditorGUILayout.LabelField("TMP Text", report.tmpTextCount.ToString());
            EditorGUILayout.LabelField("Input Fields", report.inputFieldCount.ToString());

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Discovered Reference Images", EditorStyles.boldLabel);
            foreach (var reference in report.discoveredReferenceImages)
            {
                EditorGUILayout.LabelField("• " + reference);
            }

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Elements", EditorStyles.boldLabel);
            scroll = EditorGUILayout.BeginScrollView(scroll);
            foreach (var element in report.elements)
            {
                using (new EditorGUILayout.VerticalScope("box"))
                {
                    EditorGUILayout.LabelField(element.hierarchyPath, EditorStyles.boldLabel);
                    EditorGUILayout.LabelField("Type", element.componentType);
                    EditorGUILayout.LabelField("Active", element.activeInHierarchy.ToString());
                    if (element.hasGraphic)
                    {
                        EditorGUILayout.LabelField("Graphic", element.hasAssignedGraphic ? "ASSIGNED" : "MISSING");
                        if (!string.IsNullOrEmpty(element.assetName))
                        {
                            EditorGUILayout.LabelField("Asset", element.assetName);
                        }
                    }
                }
            }
            EditorGUILayout.EndScrollView();
        }

        private void AuditSceneUI()
        {
            var scene = SceneManager.GetActiveScene();
            if (!scene.IsValid())
            {
                Debug.LogWarning("[HALL900 UI Auditor] Active scene is not valid.");
                return;
            }

            report = CreateReport(scene.name, scene.path, "Scene");

            foreach (var root in scene.GetRootGameObjects())
            {
                ScanTransform(root.transform, root.name, report, null);
            }

            RefreshReferences();
            SaveReport(report);
            Repaint();
        }

        private void AuditRuntimeUI()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("[HALL900 UI Auditor] Enter Play Mode before running the runtime UI audit.");
                return;
            }

            var scene = SceneManager.GetActiveScene();
            report = CreateReport(scene.name, scene.path, "Runtime");
            var visited = new HashSet<int>();

            var canvases = FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var canvas in canvases)
            {
                if (canvas == null) continue;
                ScanTransform(canvas.transform, canvas.name, report, visited);
            }

            RefreshReferences();
            SaveReport(report);
            Repaint();
            Debug.Log($"[HALL900 UI Auditor] Runtime audit complete. Canvases={report.canvasCount}, UI Elements={report.uiElementCount}, Images={report.imageCount}, Buttons={report.buttonCount}, TMP={report.tmpTextCount}.");
        }

        private static UIAuditReport CreateReport(string sceneName, string scenePath, string mode)
        {
            return new UIAuditReport
            {
                sceneName = sceneName,
                scenePath = scenePath,
                auditMode = mode,
                generatedAtUtc = DateTime.UtcNow.ToString("O")
            };
        }

        private void ScanTransform(Transform current, string hierarchyPath, UIAuditReport target, HashSet<int> visited)
        {
            if (current == null) return;

            var go = current.gameObject;
            if (visited != null && !visited.Add(go.GetInstanceID())) return;

            var canvas = go.GetComponent<Canvas>();
            if (canvas != null)
            {
                target.canvasCount++;
            }

            var image = go.GetComponent<Image>();
            if (image != null)
            {
                target.imageCount++;
                target.uiElementCount++;
                var assigned = image.sprite != null;
                if (assigned) target.imageWithSpriteCount++; else target.imageMissingSpriteCount++;
                target.elements.Add(new UIAuditElement
                {
                    hierarchyPath = hierarchyPath,
                    gameObjectName = go.name,
                    componentType = "Image",
                    activeInHierarchy = go.activeInHierarchy,
                    hasGraphic = true,
                    hasAssignedGraphic = assigned,
                    assetName = assigned ? image.sprite.name : string.Empty
                });
            }

            var rawImage = go.GetComponent<RawImage>();
            if (rawImage != null)
            {
                target.uiElementCount++;
                var assigned = rawImage.texture != null;
                target.elements.Add(new UIAuditElement
                {
                    hierarchyPath = hierarchyPath,
                    gameObjectName = go.name,
                    componentType = "RawImage",
                    activeInHierarchy = go.activeInHierarchy,
                    hasGraphic = true,
                    hasAssignedGraphic = assigned,
                    assetName = assigned ? rawImage.texture.name : string.Empty
                });
            }

            var button = go.GetComponent<Button>();
            if (button != null)
            {
                target.buttonCount++;
                target.uiElementCount++;
            }

            var tmpText = go.GetComponent<TMP_Text>();
            if (tmpText != null)
            {
                target.tmpTextCount++;
                target.uiElementCount++;
            }

            var input = go.GetComponent<TMP_InputField>();
            if (input != null)
            {
                target.inputFieldCount++;
                target.uiElementCount++;
            }

            foreach (Transform child in current)
            {
                ScanTransform(child, hierarchyPath + "/" + child.name, target, visited);
            }
        }

        private void RefreshReferences()
        {
            if (report == null) return;

            report.discoveredReferenceImages.Clear();
            const string root = "Assets/Art/UI_Reference";
            if (!AssetDatabase.IsValidFolder(root)) return;

            var guids = AssetDatabase.FindAssets("t:Texture2D", new[] { root });
            Array.Sort(guids, StringComparer.Ordinal);
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (IsReferenceImage(path))
                {
                    report.discoveredReferenceImages.Add(path);
                }
            }

            Repaint();
        }

        private static bool IsReferenceImage(string path)
        {
            var extension = Path.GetExtension(path);
            return extension.Equals(".png", StringComparison.OrdinalIgnoreCase)
                   || extension.Equals(".jpg", StringComparison.OrdinalIgnoreCase)
                   || extension.Equals(".jpeg", StringComparison.OrdinalIgnoreCase)
                   || extension.Equals(".webp", StringComparison.OrdinalIgnoreCase);
        }

        private static void SaveReport(UIAuditReport value)
        {
            const string directory = "Assets/Art/UI_Reference/Manifest/Reports";
            if (!AssetDatabase.IsValidFolder("Assets/Art/UI_Reference/Manifest"))
            {
                AssetDatabase.CreateFolder("Assets/Art/UI_Reference", "Manifest");
            }

            if (!AssetDatabase.IsValidFolder(directory))
            {
                AssetDatabase.CreateFolder("Assets/Art/UI_Reference/Manifest", "Reports");
            }

            var safeName = string.IsNullOrEmpty(value.sceneName) ? "UnknownScene" : value.sceneName;
            var safeMode = string.IsNullOrEmpty(value.auditMode) ? "Unknown" : value.auditMode;
            var path = $"{directory}/{safeName}_{safeMode}_UIAudit.json";
            File.WriteAllText(path, JsonUtility.ToJson(value, true));
            AssetDatabase.ImportAsset(path);
        }
    }
}
#endif
