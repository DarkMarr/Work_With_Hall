using System.IO;
using QuizGame.Gameplay;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public static class LuckyDrawTemplateBuilder
{
    public const string MachinePath = "Assets/Prefabs/UI/LuckyDraw/LuckyDrawMachine_Template.prefab";
    public const string ScenePath = "Assets/Scenes/Preview/LuckyDrawAnimationPreview.unity";
    private static Color Ink = new Color(.17f, .27f, .28f);
    private static Color Gold = new Color(.96f, .65f, .19f);

    [MenuItem("HALL900/Lucky Draw/Open Animation Preview")]
    public static void OpenPreview()
    {
        if (EditorApplication.isPlaying) { Debug.LogWarning("Stop Play Mode before opening the preview."); return; }
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        if (!File.Exists(ScenePath)) { Debug.LogError("Lucky Draw preview scene is missing."); return; }
        EditorSceneManager.OpenScene(ScenePath);
    }

    // Invoked once to assemble editable template pieces. Never automatically overwrites art.
    public static string Build()
    {
        if (EditorApplication.isPlaying) return "Stop Play Mode before building.";
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty) return "Save the current scene first.";
        if (File.Exists(MachinePath) || File.Exists(ScenePath)) return "Template already exists; edit the prefab instead.";
        Directory.CreateDirectory(Path.GetDirectoryName(MachinePath));
        Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));
        AssetDatabase.Refresh();
        var root = Rect("LuckyDrawMachine_Template", null, Vector2.zero, new Vector2(640, 560));
        var view = root.gameObject.AddComponent<LuckyDrawMachineView>();
        Shape("Ground-shadow", root, new Vector2(0,-232), new Vector2(525,38), new Color(.14f,.25f,.23f,.15f), 48);
        var body = Rect("Body-Pivot", root, Vector2.zero, new Vector2(640,560));
        Box("Base", body, new Vector2(0,-220), new Vector2(530,38), Gold);
        Box("Base-highlight", body, new Vector2(0,-204), new Vector2(530,7), new Color(1,.87f,.5f));
        Box("Stand-left", body, new Vector2(-146,-79), new Vector2(32,236), Ink);
        Box("Stand-right", body, new Vector2(104,-79), new Vector2(32,236), Ink);
        var drum = Rect("Drum-Pivot", body, new Vector2(-32,70), new Vector2(320,320));
        Shape("Drum-rim", drum, Vector2.zero, new Vector2(335,335), new Color(.69f,.39f,.12f),8);
        Shape("Drum-face", drum, Vector2.zero, new Vector2(304,304), Gold,8);
        Shape("Drum-inset", drum, Vector2.zero, new Vector2(233,233), new Color(1,.77f,.33f),8);
        for (int i=0;i<8;i++)
        {
            float a=i*Mathf.PI/4;
            Shape("Drum-bolt-"+i, drum, new Vector2(Mathf.Cos(a)*128,Mathf.Sin(a)*128), new Vector2(12,12), Ink,24);
        }
        Shape("Drum-centre", drum, Vector2.zero, new Vector2(62,62), new Color(1,.9f,.59f),32);
        Box("Crank-axle", body, new Vector2(161,70), new Vector2(82,17), Ink);
        var crank=Rect("Crank-Pivot",body,new Vector2(194,70),new Vector2(100,130));
        Box("Crank-arm",crank,new Vector2(0,35),new Vector2(15,80),new Color(.91f,.87f,.71f));
        Box("Crank-grip",crank,new Vector2(29,75),new Vector2(76,25),new Color(.65f,.39f,.19f));
        Shape("Crank-hub",crank,Vector2.zero,new Vector2(28,28),Ink,32);
        Box("Outlet",body,new Vector2(92,-43),new Vector2(65,40),Ink);
        Box("Outlet-lip",body,new Vector2(103,-67),new Vector2(89,10),new Color(.75f,.48f,.20f));
        var glow=Shape("Reveal-glow",root,new Vector2(164,-171),new Vector2(105,105),new Color(1,.80f,.25f,.25f),12);
        var ball=Rect("Ball-Pivot",root,new Vector2(92,-58),new Vector2(45,45));
        var ballShape=Shape("Ball-color",ball,Vector2.zero,new Vector2(44,44),new Color(.95f,.86f,.66f),40);
        Shape("Ball-highlight",ball,new Vector2(-7,8),new Vector2(11,8),new Color(1,1,1,.75f),24);
        Box("Tray-front",root,new Vector2(156,-197),new Vector2(148,16),Ink);
        Box("Tray-left",root,new Vector2(84,-179),new Vector2(10,45),Ink);
        Box("Tray-right",root,new Vector2(229,-179),new Vector2(10,45),Ink);
        var so=new SerializedObject(view);
        Assign(so,"body",body); Assign(so,"drum",drum); Assign(so,"crank",crank); Assign(so,"ball",ball);
        Assign(so,"glow",glow.rectTransform); Assign(so,"ballGraphic",ballShape); so.ApplyModifiedPropertiesWithoutUndo();
        view.ResetPose();
        var prefab=PrefabUtility.SaveAsPrefabAsset(root.gameObject,MachinePath);
        Object.DestroyImmediate(root.gameObject);

        var uiRoot=PrefabUtility.LoadPrefabContents("Assets/Resources/UI/Gameplay/LuckyDrawUI.prefab");
        try
        {
            var ui=new SerializedObject(uiRoot.GetComponent<LuckyDrawUI>());
            Assign(ui,"machinePrefab",prefab.GetComponent<LuckyDrawMachineView>()); ui.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(uiRoot,"Assets/Resources/UI/Gameplay/LuckyDrawUI.prefab");
        }
        finally { PrefabUtility.UnloadPrefabContents(uiRoot); }

        var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        var camera=new GameObject("Preview Camera",typeof(Camera)).GetComponent<Camera>();
        camera.clearFlags=CameraClearFlags.SolidColor; camera.backgroundColor=new Color(.96f,.94f,.86f); camera.orthographic=true;
        var canvas=new GameObject("Preview Canvas",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
        canvas.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;
        var scaler=canvas.GetComponent<CanvasScaler>(); scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution=new Vector2(1080,1920); scaler.matchWidthOrHeight=.5f;
        var panel=Box("Background",canvas.transform,Vector2.zero,new Vector2(1080,1920),new Color(.97f,.95f,.89f));
        Stretch(panel.rectTransform,Vector2.zero,Vector2.one);
        var preview=canvas.AddComponent<LuckyDrawAnimationPreview>();
        var title=Label("Title",canvas.transform,"LUCKY DRAW",72,Ink); Stretch(title.rectTransform,new Vector2(.08f,.85f),new Vector2(.92f,.94f));
        var subtitle=Label("Subtitle",canvas.transform,"ANIMATION PREVIEW  /  TEMPLATE ART",24,Ink); Stretch(subtitle.rectTransform,new Vector2(.05f,.81f),new Vector2(.95f,.86f));
        var slot=Rect("Machine-Slot",canvas.transform,Vector2.zero,Vector2.zero); Stretch(slot,new Vector2(.04f,.33f),new Vector2(.96f,.78f));
        var instance=(GameObject)PrefabUtility.InstantiatePrefab(prefab,slot);
        preview.machine=instance.GetComponent<LuckyDrawMachineView>();
        preview.statusText=Label("Phase",canvas.transform,"Tap below to try the timing",34,Ink);
        Stretch(preview.statusText.rectTransform,new Vector2(.05f,.27f),new Vector2(.95f,.33f));
        preview.playButton=Button("Replay",canvas.transform,"TAP TO DRAW / REPLAY",new Vector2(.12f,.17f),new Vector2(.88f,.245f),Gold);
        preview.speedButton=Button("Speed",canvas.transform,"SPEED  1.0x",new Vector2(.12f,.105f),new Vector2(.48f,.155f),new Color(.83f,.89f,.86f));
        preview.colorButton=Button("Ball Color",canvas.transform,"BALL  1 / 5",new Vector2(.52f,.105f),new Vector2(.88f,.155f),new Color(.83f,.89f,.86f));
        preview.speedText=preview.speedButton.GetComponentInChildren<Text>(); preview.colorText=preview.colorButton.GetComponentInChildren<Text>();
        var note=Label("Note",canvas.transform,"Visual test only - no items or points awarded",23,Ink);
        Stretch(note.rectTransform,new Vector2(.03f,.045f),new Vector2(.97f,.085f));
        var reward=Box("Template Reward",canvas.transform,Vector2.zero,Vector2.zero,new Color(.17f,.27f,.28f,.95f));
        Stretch(reward.rectTransform,new Vector2(.16f,.72f),new Vector2(.84f,.80f));
        var rewardText=Label("Reward Label",reward.transform,"REVEAL!   Sample item x1",32,Color.white);
        Stretch(rewardText.rectTransform,Vector2.zero,Vector2.one);
        preview.rewardPanel=reward.gameObject; reward.gameObject.SetActive(false);
        new GameObject("EventSystem",typeof(EventSystem),typeof(InputSystemUIInputModule));
        EditorSceneManager.SaveScene(scene,ScenePath);
        Selection.activeGameObject=instance;
        return "Created machine prefab, wired LuckyDrawUI, saved isolated preview scene. Build Settings unchanged.";
    }
    private static void Assign(SerializedObject so,string key,Object value) { so.FindProperty(key).objectReferenceValue=value; }
    private static RectTransform Rect(string name,Transform parent,Vector2 pos,Vector2 size)
    {
        var rect=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>(); rect.gameObject.layer=5;
        rect.SetParent(parent,false); rect.anchorMin=rect.anchorMax=rect.pivot=Vector2.one*.5f;
        rect.anchoredPosition=pos; rect.sizeDelta=size; return rect;
    }
    private static Image Box(string name,Transform parent,Vector2 pos,Vector2 size,Color color)
    { var img=Rect(name,parent,pos,size).gameObject.AddComponent<Image>(); img.color=color; img.raycastTarget=false; return img; }
    private static LuckyDrawTemplateShape Shape(string name,Transform parent,Vector2 pos,Vector2 size,Color color,int sides)
    { var g=Rect(name,parent,pos,size).gameObject.AddComponent<LuckyDrawTemplateShape>(); g.color=color; g.sides=sides; g.raycastTarget=false; return g; }
    private static void Stretch(RectTransform r,Vector2 min,Vector2 max)
    { r.anchorMin=min; r.anchorMax=max; r.offsetMin=r.offsetMax=Vector2.zero; }
    private static Text Label(string name,Transform parent,string content,int size,Color color)
    {
        var text=Rect(name,parent,Vector2.zero,Vector2.zero).gameObject.AddComponent<Text>();
        text.font=UnityEngine.Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); text.text=content; text.fontSize=size;
        text.alignment=TextAnchor.MiddleCenter; text.color=color; text.raycastTarget=false;
        text.resizeTextForBestFit=true; text.resizeTextMinSize=16; text.resizeTextMaxSize=size; return text;
    }
    private static Button Button(string name,Transform parent,string content,Vector2 min,Vector2 max,Color color)
    {
        var image=Box(name,parent,Vector2.zero,Vector2.zero,color); Stretch(image.rectTransform,min,max); image.raycastTarget=true;
        var button=image.gameObject.AddComponent<Button>(); button.targetGraphic=image;
        var text=Label("Label",image.transform,content,30,Ink); Stretch(text.rectTransform,new Vector2(.04f,.1f),new Vector2(.96f,.9f)); return button;
    }
}
