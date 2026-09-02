#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
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
                "Phase 1 scans the currently open Unity scene, reports UI structure and assigned graphics, and discovers reference images. It does not modify scenes or graphic resources.",
                MessageType.Info);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Audit Active Scene", GUILayout.Height(28)))
                {
                    AuditActiveScene();
                }

                if (GUILayout.Button("Refresh References", GUILayout.Height(28)))
                {
                    RefreshReferences();
                }
            }

            EditorGUILayout.Space(8);

            if (report == null)
            {
                EditorGUILayout.LabelField("No audit run yet.");
                return;
            }

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

        private void AuditActiveScene()
        {
            var scene = SceneManager.GetActiveScene();
            if (!scene.IsValid())
            {
                Debug.LogWarning("[HALL900 UI Auditor] Active scene is not valid.");
                return;
            }

            report = new UIAuditReport
            {
                sceneName = scene.name,
                scenePath = scene.path,
                generatedAtUtc = DateTime.UtcNow.ToString("O")
            };

            foreach (var root in scene.GetRootGameObjects())
            {
                ScanTransform(root.transform, root.name, report);
            }

            RefreshReferences();
            SaveReport(report);
            Repaint();
        }

        private void ScanTransform(Transform current, string hierarchyPath, UIAuditReport target)
        {
            var go = current.gameObject;
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
                ScanTransform(child, hierarchyPath + "/" + child.name, target);
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
            var path = $"{directory}/{safeName}_UIAudit.json";
            File.WriteAllText(path, JsonUtility.ToJson(value, true));
            AssetDatabase.ImportAsset(path);
        }
    }
}
#endif
