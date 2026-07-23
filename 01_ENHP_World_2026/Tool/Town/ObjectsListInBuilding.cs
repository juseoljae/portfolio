using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectsListInBuilding : MonoBehaviour
{
    [SerializeField] public GameObject ChildObj;
    [SerializeField] public GameObject ColliderObj;
    [SerializeField] public GameObject IconPosObj;
    [SerializeField] public GameObject ExitObj;
    [SerializeField] public List<GameObject> ExitObjList;
    [SerializeField] public GameObject InterPosObj;
    [SerializeField] public List<GameObject> InterObjList;
    [SerializeField] public GameObject InterPos_FriendObj;
    [SerializeField] public List<GameObject> InterPos_FriendObjList;
    [SerializeField] public GameObject MountPosObj;
    [SerializeField] public GameObject ObjPosObj;
    [SerializeField] public List<GameObject> ObjList;
    [SerializeField] public GameObject DrtObj;
    [SerializeField] public GameObject SrtObj;
    [SerializeField] public GameObject WorkPosObj;
    [SerializeField] public GameObject EffIdleObj;
    [SerializeField] public GameObject EffWorkingObj;

    [SerializeField] public GameObject RendererObj;
    [SerializeField] public List<Renderer> DrtRendererObjs;
    [SerializeField] public List<Renderer> SrtRendererObjs;

    public void InitList()
    {
        if (ExitObjList == null)
        {
            ExitObjList = new List<GameObject>();
        }

        ExitObjList.Clear();

        if (InterObjList == null)
        {
            InterObjList = new List<GameObject>();
        }

        InterObjList.Clear();

        if (InterPos_FriendObjList == null)
        {
            InterPos_FriendObjList = new List<GameObject>();
        }
        InterPos_FriendObjList.Clear();

        if (ObjList == null)
        {
            ObjList = new List<GameObject>();
        }

        ObjList.Clear();
    }

    public void AddExitObjList(GameObject obj)
    {

        if (ExitObjList.Contains(obj) == false)
        {
            ExitObjList.Add(obj);
        }
    }

    public void AddInterObjList(GameObject obj)
    {

        if (InterObjList.Contains(obj) == false)
        {
            InterObjList.Add(obj);
        }
    }
    
    public void AddInterPos_FriendObjList(GameObject obj)
    {
        if (InterPos_FriendObjList.Contains(obj) == false)
        {
            InterPos_FriendObjList.Add(obj);
        }
    }

    public void AddObjList(GameObject obj)
    {

        if (ObjList.Contains(obj) == false)
        {
            ObjList.Add(obj);
        }
    }

    public List<GameObject> GetExitObjList()
    {
        return ExitObjList;
    }


    public List<GameObject> GetInterObjList()
    {
        return InterObjList;
    }
    public List<GameObject> GetObjList()
    {
        return ObjList;
    }
}
