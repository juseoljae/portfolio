using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using TouchScript.Gestures;

public class AttachObjectList : AssetPostprocessor
{
    private const string PROJECT_MENU_NAME = "Assets/SNG BG Object Preprocessor";
    //private const string PROJECT_MENU_ALL = "Assets/AttachObjectList_AllChildDir";

    public const string Layer_BD = "Building";
    public const string Tag_TOUCHCOL = "TouchCollider";

    //void OnPostprocessPrefab(GameObject g)
    //{
    //    Debug.Log("Prefab: " + g.name);
    //    string objPath = AssetDatabase.GetAssetPath(g);
    //    //SetObjectList(g, objPath);
    //}

    public const string DG_STR = "sng_bg";
    //public const string 

    [MenuItem(PROJECT_MENU_NAME)]
    public static void NewPrefabCheckerMenu()
    {                
        List<GameObject> selectedObj = Selection.gameObjects.ToList();
        SetObjectList(selectedObj);
    }


    // [MenuItem(PROJECT_MENU_ALL)]
    // public static void NewPrefabCheckerAll()
    // {
    //     List<GameObject> objs = GetPrefabsInDirectories();
    //     SetObjectList(objs);
    // }


    private static void SetObjectList(List<GameObject> objs)
    {
        Debug.Log($"start Object Path is not contail {objs?.Count}");

        var originPaths = "sng_bg";

        foreach (GameObject obj in objs)
        {
            string objPath = AssetDatabase.GetAssetPath(obj);

            if (!objPath.Contains(originPaths))
            {
                Debug.Log($"==== SetObjectList(). objPath: {objPath}");
                Debug.Log($"return Object Path is not contail {originPaths}");
                return;
            } 

            Debug.Log($"New Prefab Menu Checker. NewPrefabCheckerMenu() Prefab: {obj.name}");

            GameObject loadObj = AssetDatabase.LoadAssetAtPath<GameObject>(objPath);
            GameObject instObj = PrefabUtility.InstantiatePrefab(loadObj) as GameObject;
            

            //SetObjectList(obj);
            SetObjectList(instObj, objPath);
        }
    }



    private static void SetObjectList(GameObject obj, string objPath)
    {
        ObjectsListInBuilding objList = obj.GetComponent<ObjectsListInBuilding>();
        GameObject.DestroyImmediate(objList, true);

        if (objList == null)
        {
            objList = obj.AddComponent<ObjectsListInBuilding>();
        }

        if(obj.transform.childCount > 0)
        {
            Transform firstChild = obj.transform.GetChild(0);
            if (firstChild != null)
            {
                objList.ChildObj = firstChild.gameObject;
            }
        }

        objList.InitList();

        Transform[] transforms = obj.GetComponentsInChildren<Transform>();

        foreach (Transform t in transforms)
        {
            if (t.name.Equals("collider"))
            {
                objList.ColliderObj = t.gameObject;
            }
            else if (t.name.Equals("IconPos"))
            {
                objList.IconPosObj = t.gameObject;
            }
            else if (t.name.Equals("ExitPos"))
            {
                objList.ExitObj = t.gameObject;
                Transform[] childTMs = t.gameObject.GetComponentsInChildren<Transform>(true);
                foreach (var child in childTMs)
                {
                    if (t == child) continue;
                    objList.AddExitObjList(child.gameObject);
                }
            }
            else if (t.name.Equals("InterPos"))
            {
                objList.InterPosObj = t.gameObject;
                Transform[] childTMs = t.gameObject.GetComponentsInChildren<Transform>(true);
                foreach (var child in childTMs)
                {
                    if (t == child) continue;
                    if (int.TryParse(child.name, out int posIdx))
                    {
                        // 숫자로 정상 파싱된 경우만
                        objList.AddInterObjList(child.gameObject);
                    }
                }
            }
            else if (t.name.Equals("InterPos_F"))
            {
                objList.InterPos_FriendObj = t.gameObject;
                Transform[] childTMs = t.gameObject.GetComponentsInChildren<Transform>(true);
                foreach (var child in childTMs)
                {
                    if (t == child) continue;
                    if (int.TryParse(child.name, out int posIdx))
                    {
                        // 숫자로 정상 파싱된 경우만
                        objList.AddInterPos_FriendObjList(child.gameObject);
                    }
                    
                }
            }
            else if (t.name.Contains("MountPos0"))
            {
                objList.AddInterObjList(t.gameObject);
            }
            else if (t.name.Equals("MountPos"))
            {
                objList.MountPosObj = t.gameObject;
            }
            else if (t.name.Equals("ObjPos"))
            {
                objList.ObjPosObj = t.gameObject;
                Transform[] childTMs = t.gameObject.GetComponentsInChildren<Transform>(true);
                foreach (var child in childTMs)
                {
                    if (t == child) continue;
                    objList.AddObjList(child.gameObject);
                }
            }
            else if (t.name.Equals("DRT"))
            {
                SetRendererObjs(t.gameObject, obj, objList);
                SetTagLayerToMeshRenderer(t.gameObject, Tag_TOUCHCOL, LayerMask.NameToLayer(Layer_BD), objList, obj);
                objList.DrtObj = t.gameObject;
            }
            else if (t.name.Equals("SRT"))
            {
                SetRendererObjs(t.gameObject, obj, objList);
                //only touchable object in DRT
                //SetTagLayerToMeshRenderer(t.gameObject, Tag_TOUCHCOL, LayerMask.NameToLayer(Layer_BD), objList, obj);
                objList.SrtObj = t.gameObject;
            }
            else if (t.name.Equals("WorkPos"))
            {
                objList.WorkPosObj = t.gameObject;
            }
            else if (t.name.Equals("eff_idle"))
            {
                objList.EffIdleObj = t.gameObject;
            }
            else if (t.name.Equals("eff_working"))
            {
                objList.EffWorkingObj = t.gameObject;
            }
        }

        PrefabUtility.SaveAsPrefabAsset(obj, objPath);

        GameObject.DestroyImmediate(obj);

    }

    static List<GameObject> GetPrefabsInDirectories()
    {
        var originPaths = Directory.GetDirectories("Assets/Project/Master_Object/ResourcesFromAssetbundle/3D");
        string sngBdPath = "sng_bg";// sng_bg_building";

        List<GameObject> prefabList = new List<GameObject>();

        foreach (var path in originPaths)
        {
            if (path.Contains(sngBdPath))
            {
                var prefabPaths = Directory.GetFiles(path, "*.prefab", SearchOption.AllDirectories);

                foreach (var prefabPath in prefabPaths)
                {
                    var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                    if (prefab != null)
                    {
                        prefabList.Add(prefab);
                    }
                }
            }
            else
            {
                Debug.Log($"==== GetPrefabsInDirectories(). path: {path}");
            }
        }

        return prefabList;
    }


    public static void SetLayer(GameObject go, int layer)
    {
        go.layer = layer;
        Transform t = go.transform;
        for (int i = 0; i < t.childCount; i++)
            SetLayer(t.GetChild(i).gameObject, layer);
    }

    public static void SetTag(GameObject go, string tag)
    {
        go.tag = tag;
        Transform t = go.transform;
        for (int i = 0; i < t.childCount; i++)
            SetTag(t.GetChild(i).gameObject, tag);
    }



    public static void SetTagLayerToMeshRenderer(GameObject go, string tag, int layer, ObjectsListInBuilding objList, GameObject obj)
    {
        GameObject touchableObj = GetTouchableObj(go, obj, objList);
        if(touchableObj != null)
        {
            touchableObj.tag = tag;
            if(layer >= 0)
                touchableObj.layer = layer;
            objList.RendererObj = touchableObj;
        }
    }
   
    public static GameObject GetTouchableObj(GameObject go, GameObject obj, ObjectsListInBuilding objList)
    {
        foreach(Renderer rgo in objList.DrtRendererObjs)
        {
            if (rgo.gameObject.GetComponent<Collider>() != null)
            {
                return rgo.gameObject;
            }
        }
        //Transform[] _tr = go.GetComponentsInChildren<Transform>();
        //foreach (Transform tr in _tr)
        //{
        //    if(tr.GetComponent<MeshRenderer>() != null || tr.GetComponent<SkinnedMeshRenderer>() != null)
        //    {
        //        if (tr.gameObject.GetComponent<Collider>() != null)
        //        {
        //            return tr.gameObject;
        //        }
        //    }
        //}

        Debug.Log($"No touchable object. factory: {obj?.name}");
        return null;
    }

    public static void SetRendererObjs(GameObject go, GameObject obj, ObjectsListInBuilding objList)
    {
        if (go.name.Equals("DRT"))
        {
            if (objList.DrtRendererObjs == null)
            {
                objList.DrtRendererObjs = new List<Renderer>();
            }
            else
            {
                objList.DrtRendererObjs.Clear();
            }
        }

        if (go.name.Equals("SRT"))
        {
            if (objList.SrtRendererObjs == null)
            {
                objList.SrtRendererObjs = new List<Renderer>();
            }
            else
            {
                objList.SrtRendererObjs.Clear();
            }
        }

        Transform[] _tr = go.GetComponentsInChildren<Transform>();
        foreach (Transform tr in _tr)
        {
            if (tr.GetComponent<MeshRenderer>() != null || tr.GetComponent<SkinnedMeshRenderer>() != null)
            {
                Renderer rnd = tr.GetComponent<Renderer>();

                if (go.name.Equals("DRT"))
                {
                    if (objList.DrtRendererObjs.Contains(rnd) == false)
                    {
                        objList.DrtRendererObjs.Add(rnd);
                    }
                }
                else if (go.name.Equals("SRT"))
                {
                    if (objList.SrtRendererObjs.Contains(rnd) == false)
                    {
                        objList.SrtRendererObjs.Add(rnd);
                    }
                }
            }
        }
    }
}
