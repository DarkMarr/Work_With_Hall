using System;
using System.Collections.Generic;

namespace QuizGame.UI.Audit
{
    [Serializable]
    public class UIReferenceManifest
    {
        public int version = 1;
        public string referenceRoot = "Assets/Art/UI_Reference";
        public bool autoDiscoverReferenceImages = true;
        public List<UIScreenReference> screens = new List<UIScreenReference>();
    }

    [Serializable]
    public class UIScreenReference
    {
        public string id;
        public string scenePath;
        public string referenceImage;
    }

    [Serializable]
    public class UIAuditReport
    {
        public string sceneName;
        public string scenePath;
        public string auditMode;
        public int canvasCount;
        public int uiElementCount;
        public int imageCount;
        public int imageWithSpriteCount;
        public int imageMissingSpriteCount;
        public int buttonCount;
        public int tmpTextCount;
        public int inputFieldCount;
        public List<UIAuditElement> elements = new List<UIAuditElement>();
        public List<string> discoveredReferenceImages = new List<string>();
        public string generatedAtUtc;
    }

    [Serializable]
    public class UIAuditElement
    {
        public string hierarchyPath;
        public string gameObjectName;
        public string componentType;
        public bool activeInHierarchy;
        public bool hasGraphic;
        public bool hasAssignedGraphic;
        public string assetName;
    }
}
