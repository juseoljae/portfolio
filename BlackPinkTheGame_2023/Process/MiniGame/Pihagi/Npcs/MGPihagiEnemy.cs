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
# endif

#endregion

using UnityEngine;
using BPWPacketDefine;
using System.Collections.Generic;

public class MGPihagiEnemy
{
    public MGPihagiWorldManager WorldMgr;
    public MGPihagiEnemysManager EnemyMgr;

    public MGPihagiEnemyController EnemyController;

    public CStateMachine<MGPihagiEnemy> EnemySM;

    public PihagiGameSnapshotEnemy EnemyInfo;

    public byte SpawnPosIdx;
    public GameObject EnemyObj;
    private GameObject DashFXObj;

    public PihagiGameEnemyState EnemyState;
    private PihagiGameEnemyState PrevEnemyState;

    private int EnemyAngle;
    public ENEMY_POSDIR EnemyDir{ get; set; }


    public void Initialize(MGPihagiWorldManager worldMgr, PihagiGameSnapshotEnemy pktInfo)
    {
        WorldMgr = worldMgr;
        EnemyMgr = worldMgr.EnemyMgr;

        EnemySM = new CStateMachine<MGPihagiEnemy>(this);

        EnemyInfo = pktInfo;

        LoadEnemy( pktInfo );

        LoadEnemyFX();
    }

    public void LoadEnemy(PihagiGameSnapshotEnemy pktInfo)
    {

        MGPihagiGameDataInfo _dataInfo = MGPihagiDataManager.Instance.GetPihagiGameDataInfo( pktInfo.PihagiGameTid);

        SpawnPosIdx = _dataInfo.SpawnPoint;

        GameObject originObj = EnemyMgr.GetEnemyOriginObjectByEnemyID(_dataInfo.EnemyID);
        EnemyObj = Utility.AddChild(EnemyMgr.gameObject, originObj);
        Vector3 _spawnPos  = WorldMgr.GetVector3( pktInfo.PosX, 0, pktInfo.PosZ);
        _spawnPos.y = MGPihagiDefine.ENEMY_SPAWN_YPOS;
        EnemyObj.transform.position = _spawnPos;

        GameObject shadowObj = AvatarManager.Instance.SetCharacterShadow(EnemyMgr.gameObject);

        AvatarManager.ChangeLayersRecursively(EnemyObj.transform, CDefines.AVATAR_BPW_OBJECT_LAYER_NAME);

        //Component
        EnemyController = EnemyObj.AddComponent<MGPihagiEnemyController>();
        EnemyController.Initialize(EnemyMgr, pktInfo );
        EnemyController.SetShadowObj(shadowObj.transform);

        ENEMY_POSDIR dir = WorldMgr.GetEnemyDir( SpawnPosIdx );
        SetEnemyAngleDir( dir );

        //CDebug.Log( "Enemy Spawn index = " + SpawnPosIdx + "/ dir = "+ dir + " //// EnemyInfo.PihagiEnemyTid = " + pktInfo.PihagiGameTid );////

        MGPihagiEnemyInfo _info = MGPihagiDataManager.Instance.GetPihagiEnemyDataInfo( _dataInfo.EnemyID );
        EnemyController.SetRuntimeAnimatorController( _dataInfo.EnemyID.ToString(), _info.ControllerGrpID, ANIMCONTROLLER_USE_TYPE.BPW_MINIGAME, EnemyObj );
    }


    //NPC/NPC_92_001/Animations/NPC_92_001_Animator_Controller_minigame.controller
    private void LoadEnemyFX()
    {
        DashFXObj = GameObject.Instantiate(EnemyMgr.DashFX_OrgObj);
        DashFXObj.transform.SetParent(EnemyObj.transform);
        DashFXObj.transform.localPosition = Vector3.zero;
        DashFXObj.transform.localRotation = Quaternion.Euler(Vector3.zero);
        DashFXObj.transform.localScale = Vector3.one;
        DashFXObj.SetActive(false);
    }

    public void CheckSpawnYpos()
    {
        //CDebug.Log($"      [CheckSpawnYpos] id : {EnemyInfo.EnemyId}, spawnPos :{SpawnPosIdx} / yPos : {EnemyObj.transform.position.y}" );
        if (EnemyObj.transform.position.y < MGPihagiDefine.ENEMY_SPAWN_YPOS)
        {
            CDebug.LogError( $"      [CheckSpawnYpos] id : {EnemyInfo.EnemyId}, spawnPos :{SpawnPosIdx} / yPos : {EnemyObj.transform.position.y}" );
            Vector3 pos = EnemyObj.transform.position;
            pos.y = MGPihagiDefine.ENEMY_SPAWN_YPOS;
            EnemyObj.transform.position = pos;
        }
    }

    public void UpdateMovePos(PihagiGameSnapshotEnemy pktInfo)
    {
        Vector3 targetPos = WorldMgr.GetVector3(pktInfo.PosX, 0, pktInfo.PosZ);
        
        EnemyController.SetPlayerMoveRecv(targetPos);
    }


    public void SetEnemyState(PihagiGameEnemyState state)
    {
        EnemyState = state;
        //CDebug.Log($"SetEnemyState() ID:{EnemyInfo.EnemyId}, state:{state}");//

        switch (state)
        {
            case PihagiGameEnemyState.REBIRTH:
                if (PrevEnemyState != EnemyState)
                {
                    SetChangeState(MGPihagiEnemyState_Spawn.Instance());
                }
                break;
            case PihagiGameEnemyState.DELAY:
                if (PrevEnemyState != EnemyState)
                {
                    SetChangeState(MGPihagiEnemyState_Prepare.Instance());
                }
                break;
            case PihagiGameEnemyState.MOVE:
                if (PrevEnemyState != EnemyState)
                {
                    SetChangeState(MGPihagiEnemyState_Dash.Instance());
                }
                break;
            case PihagiGameEnemyState.DEATH:
                if (PrevEnemyState != EnemyState)
                {
                    SetChangeState(MGPihagiEnemyState_Deth.Instance());
                }
                break;
        }
        PrevEnemyState = EnemyState;
    }

    public void SetEnemyAngleDir(ENEMY_POSDIR dir)
    {
        EnemyDir = dir;
        EnemyController.EnemyDir = dir;
    }

    public void SetActiveGroundFX(bool bActive)
    {
        WorldMgr.MapMgr.SetActiveFxGroundObjByIndex(SpawnPosIdx, bActive);
    }

    public void SetDash()
    {
        DashFXObj.SetActive(true);
    }

    public void ReleaseDash()
    {
        DashFXObj.SetActive(false);
    }

    public void SetHideEnemyObject()
    {
        SetActiveGroundFX(false);
        EnemyObj.SetActive(false);
        EnemyController.SetActiveEnemyShadowObject(false);
    }

    public PihagiGameEnemyState GetEnemyState()
    {
        return EnemyState;
    }


    public CState<MGPihagiEnemy> GetPreState()
    {
        return EnemySM.GetPreviousState();
    }

    public void SetChangeState(CState<MGPihagiEnemy> state)
    {
        EnemySM.ChangeState(state);
    }

	public void CleanUp()
	{
		if (EnemyController != null)
		{
            //EnemyController.ReleaseRuntimeAnimatorController();

            EnemyController.CleanUp();
			GameObject.Destroy(EnemyController);
		}

		if(EnemyObj != null)
		{
			GameObject.Destroy(EnemyObj);
		}
	}

	public void Release()
	{
		if (EnemyController != null)
        {
            EnemyController.ReleaseRuntimeAnimatorController(true);
            GameObject.Destroy(EnemyController);
			EnemyController = null;
		}
		if (WorldMgr != null) WorldMgr = null;
		if (EnemyMgr != null) EnemyMgr = null;
		if (EnemySM != null) EnemySM = null;
		if (EnemyObj != null) EnemyObj = null;
		if (DashFXObj != null) DashFXObj = null;

	}
}
