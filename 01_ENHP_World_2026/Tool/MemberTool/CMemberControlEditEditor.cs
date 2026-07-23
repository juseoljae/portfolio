#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

[CustomEditor(typeof(CMemberControlEdit))]
public class CMemberControlEditEditor : Editor
{
    private CMemberControlEdit MControl;
    private Animator animator;

    private List<string> ControllerParams = new List<string>();

    private static GameObject HairObj;
    private static GameObject SkinObj;
    private static GameObject HeadAccObj;
    private static GameObject FaceAccObj;
    private static GameObject BodyAccObj;


    private static AvatarToolEditSettings settings;
    private const string SettingsPath = "Assets/Project/Editor/AvatarToolEditor/AvatarToolEditSettings.asset";
    private bool isWearingFirsst;


    private void LoadSettings()
    {
        if (settings == null)
        {
            string dir = Path.GetDirectoryName(SettingsPath);
            if (!Directory.Exists(dir))
            {
                Debug.Log($"Directory does not exist: {dir}");
                Directory.CreateDirectory(dir);
                AssetDatabase.Refresh(); 
            }

            settings = AssetDatabase.LoadAssetAtPath<AvatarToolEditSettings>(SettingsPath);

            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<AvatarToolEditSettings>();
                AssetDatabase.CreateAsset(settings, SettingsPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }
    }

    public override void OnInspectorGUI()
    {
        LoadSettings(); 

        EditorGUILayout.LabelField("캐릭터 파츠");
        EditorGUILayout.BeginVertical("AnimBox");

        settings.HairObj = (GameObject)EditorGUILayout.ObjectField("Hair 오브젝트", settings.HairObj, typeof(GameObject), true);
        settings.SkinObj = (GameObject)EditorGUILayout.ObjectField("Skin 오브젝트", settings.SkinObj, typeof(GameObject), true);
        settings.HeadAccObj = (GameObject)EditorGUILayout.ObjectField("Hair Acc 오브젝트", settings.HeadAccObj, typeof(GameObject), true);
        settings.FaceAccObj = (GameObject)EditorGUILayout.ObjectField("Face Acc 오브젝트", settings.FaceAccObj, typeof(GameObject), true);
        settings.BodyAccObj = (GameObject)EditorGUILayout.ObjectField("Body Acc 오브젝트", settings.BodyAccObj, typeof(GameObject), true);

        if (EditorApplication.isPlaying && !isWearingFirsst)
        {
            MControl.ChangeStyling(settings.HairObj, settings.SkinObj, settings.HeadAccObj, settings.FaceAccObj, settings.BodyAccObj);
            isWearingFirsst = true;
        }

        //
        if (GUILayout.Button("스타일 교체하기"))
        {
            //MControl.ChangeStyling(HairObj, SkinObj, HeadAccObj, FaceAccObj, BodyAccObj);
            MControl.ChangeStyling(settings.HairObj, settings.SkinObj, settings.HeadAccObj, settings.FaceAccObj, settings.BodyAccObj);
        }
        EditorGUILayout.EndVertical();

        GUILayout.Space(5);
        GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
        GUILayout.Space(5);

        EditorGUILayout.BeginVertical("AnimBox");
        EditorGUILayout.LabelField("애니메이션");

        foreach (var param in ControllerParams)
        {
            if (GUILayout.Button(param))
            {
                MControl.SetTrigger(param);
            }
        }
        EditorGUILayout.EndVertical();
    }


    public void OnEnable()
    { 
        if (MControl == null)
        {
            MControl = (CMemberControlEdit)target;
            MControl.Init();
            animator = MControl.MemberAnimator;
        }

        if (animator != null)
        {
            AnimatorControllerParameter[] parameters = animator.parameters;

            foreach(var param in parameters)
            {
                ControllerParams.Add(param.name);
                Debug.Log($"param.name: {param.name}");
            }

        }
    }
     
}


[CreateAssetMenu(fileName = "AvatarToolEditSettings", menuName = "Editor/AvatarToolEditor/AvatarTool Edit Settings")]
public class AvatarToolEditSettings : ScriptableObject
{
    public GameObject HairObj;
    public GameObject SkinObj;
    public GameObject HeadAccObj;
    public GameObject FaceAccObj;
    public GameObject BodyAccObj;
}

#endif