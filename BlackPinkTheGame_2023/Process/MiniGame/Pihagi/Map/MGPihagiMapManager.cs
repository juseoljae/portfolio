#define _DEBUG_MSG_ENABLED
#define _DEBUG_WARN_ENABLED
#define _DEBUG_ERROR_ENABLED

#region Global project preprocessor directives.

#if _DEBUG_MSG_ALL_DISABLED
#undef _DEBUG_MSG_ENABLED
#endif

#if _DEBUG_WARN_ALL_DISABLED
#undef _DEBUG_WARN_ENABLED
#endif

#if _DEBUG_ERROR_ALL_DISABLED
#undef _DEBUG_ERROR_ENABLED
#endif

#endregion

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MGPihagiMapManager
{
    public MGPihagiWorldManager WorldMgr;

    //FX Origin gameobject
    public GameObject GroundFX_OrgObj;
    public Transform FXGroundRootObj;

    public Dictionary<ENEMY_POSDIR, List<GameObject>> FxGroundObjDic;

    public void Initialize(MGPihagiWorldManager mgr, Transform fxRoot)
    {
        WorldMgr = mgr;

        FXGroundRootObj = fxRoot;
        FxGroundObjDic = new Dictionary<ENEMY_POSDIR, List<GameObject>>();

        LoadGroundFXOrigin();
        SetFxGroundObj();
    }

    public void LoadGroundFXOrigin()
    {
        CResourceData resData = CResourceManager.Instance.GetResourceData(MGPihagiConstants.FX_GROUND_PATH);
        GroundFX_OrgObj = resData.Load<GameObject>(WorldMgr.gameObject);
    }

    private void SetFxGroundObj()
    {
        FxGroundObjDic.Add(ENEMY_POSDIR.LEFT, new List<GameObject>());
        FxGroundObjDic.Add(ENEMY_POSDIR.TOP, new List<GameObject>());
        FxGroundObjDic.Add(ENEMY_POSDIR.RIGHT, new List<GameObject>());
        for (int i = 0; i<MGPihagiDefine.GROUND_SPAWN_COUNT; ++i)
        {
            Transform parentObj = FXGroundRootObj.Find("left_" + i);
            GameObject fxObj = GetFXObj(parentObj);
            FxGroundObjDic[ENEMY_POSDIR.LEFT].Add(fxObj);

            parentObj = FXGroundRootObj.Find("top_" + i);
            fxObj = GetFXObj(parentObj);
            FxGroundObjDic[ENEMY_POSDIR.TOP].Add(fxObj);

            parentObj = FXGroundRootObj.Find("right_" + i);
            fxObj = GetFXObj(parentObj);
            FxGroundObjDic[ENEMY_POSDIR.RIGHT].Add(fxObj);
        }
    }

    public void SetActiveFxGroundObjByIndex(int spawnidx, bool bActive)
    {
        //bactive가 false일 때
        //출발선상에 prepare하는 enemy가 있으면 리턴
        ENEMY_POSDIR _dir = WorldMgr.GetEnemyDir( spawnidx );
        int objIdx = 0;

        switch (_dir)
        {
            case ENEMY_POSDIR.LEFT:  objIdx = spawnidx - MGPihagiDefine.GROUND_DIR_LEFT_STARTIDX;  break;
            case ENEMY_POSDIR.TOP:   objIdx = spawnidx - MGPihagiDefine.GROUND_DIR_TOP_STARTIDX;   break;
            case ENEMY_POSDIR.RIGHT: objIdx = spawnidx - MGPihagiDefine.GROUND_DIR_RIGHT_STARTIDX; break;
        }
        //CDebug.Log( $"      [GetFxGroundObjByIndex]      dir : {_dir}, idx : {spawnidx}, objIdx = {objIdx}, count = {FxGroundObjDic[_dir].Count}" );
        if (FxGroundObjDic.ContainsKey( _dir ))
        {
            if(objIdx < 0 || objIdx >= MGPihagiDefine.GROUND_SPAWN_COUNT)
            {
                CDebug.LogError($"GetFxGroundObjByIndex() dir : {_dir}, idx : {spawnidx} is not arange in FxGroundObjDic");
                return;
            }

            FxGroundObjDic[_dir][objIdx].SetActive( bActive );
            //try
            //{
            //}
            //catch (System.Exception e)
            //{
            //    CDebug.LogError( $"      [GetFxGroundObjByIndex] dir : {_dir}, idx : {spawnidx}, objIdx = {objIdx}" );
            //}
        }

    }

    private GameObject GetFXObj(Transform parentObj)
    {
        GameObject fxObj = GameObject.Instantiate(GroundFX_OrgObj);
        fxObj.transform.SetParent(parentObj);
        fxObj.transform.localPosition = new Vector3(0, 0.04f, 0);
        fxObj.transform.localRotation = Quaternion.Euler(Vector3.zero);
        fxObj.transform.localScale = Vector3.one;
        fxObj.SetActive(false);

        return fxObj;
    }
}
