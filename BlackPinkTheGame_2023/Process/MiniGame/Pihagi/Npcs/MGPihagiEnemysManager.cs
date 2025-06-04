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

using BPWPacketDefine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MGPihagiEnemysManager : MonoBehaviour
{
    private MGPihagiWorldManager WorldMgr;

    private Dictionary<byte, MGPihagiEnemy> EnemyDic = new Dictionary<byte, MGPihagiEnemy>(); // Key : PihagiGameSnapshotEnemy.enemy_id:ubyte;
    private Dictionary<long, GameObject> EnemyOriginObjDic = new Dictionary<long, GameObject>();
    public Dictionary<ENEMY_POSDIR, Dictionary<PihagiGameEnemyState, bool>> EnemyCheckPlaySound = new Dictionary<ENEMY_POSDIR, Dictionary<PihagiGameEnemyState, bool>>();

    public GameObject DashFX_OrgObj;


    public void Initialize(MGPihagiWorldManager worldMgr)
    {
        WorldMgr = worldMgr;
        //SetEnemySpawnPosition();
        LoadEnemyOriginObject();

        LoadDashFXOrigin();
    }

    public void LoadDashFXOrigin()
    {
        CResourceData resData = CResourceManager.Instance.GetResourceData(MGPihagiConstants.FX_ENEMYDASH_PATH);
        DashFX_OrgObj = resData.Load<GameObject>(this.gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        foreach (MGPihagiEnemy enemy in EnemyDic.Values)
        {
            if (enemy.EnemySM != null)
            {
                enemy.EnemySM.StateMachine_Update();
            }
        }
    }

    public void LoadEnemyOriginObject()
    {
        Dictionary<long, MGPihagiEnemyInfo> _dic = MGPihagiDataManager.Instance.GetPihagiAllEnemyInfo();

        foreach(KeyValuePair<long, MGPihagiEnemyInfo> info in _dic)
        {
            string path = info.Value.Resource;
            CResourceData resData = CResourceManager.Instance.GetResourceData(path);
            GameObject obj = resData.Load<GameObject>(WorldMgr.gameObject);
            if (EnemyOriginObjDic.ContainsKey(info.Key) == false)
            {
                EnemyOriginObjDic.Add(info.Key, obj);
            }
        }
    }

    public GameObject GetEnemyOriginObjectByEnemyID(long eid)
    {
        if(EnemyOriginObjDic.ContainsKey(eid) == false)
        {
            CDebug.LogError($"GetEnemyOriginObjectByEnemyID() eid:{eid} is not contain EnemyOriginObjDic");
            return null;
        }

        return EnemyOriginObjDic[eid];
    }

    public void SetEnemyState(BPWPacket.PihagiGameRoomSnapshotBc _recvPkt)
    {
        for (int i = 0; i < _recvPkt.EnemiesLength; ++i)
        {
            PihagiGameSnapshotEnemy _enemySnapShotInfo = _recvPkt.Enemies( i ).Value;
            SetEnemyBySnapShotInfo( _enemySnapShotInfo );
        }

    }

    public void SetEnemyBySnapShotInfo(PihagiGameSnapshotEnemy pktInfo)
    {
        //CDebug.Log( $"          *********** SetEnemyBySnapShotInfo     state : {pktInfo.State}" );//
        switch (pktInfo.State)
        {
            case PihagiGameEnemyState.DEATH:
                break;
            case PihagiGameEnemyState.REBIRTH:
                SetEnemy(pktInfo);
                break; 
            case PihagiGameEnemyState.DELAY:
                EnemyDic[pktInfo.EnemyId].CheckSpawnYpos();
                //CDebug.Log( "      [Get DElay pos ] = " + pktInfo.PosX + "," + pktInfo.PosZ );
                break;
            case PihagiGameEnemyState.MOVE:
                UpdateEnemyMovePos(pktInfo);
                break;
            default:
                break;
        }
        SetEnemyState(pktInfo.EnemyId, pktInfo.State);
    }

    public void SetEnemy(PihagiGameSnapshotEnemy pktInfo)
    {
        if(EnemyDic.ContainsKey(pktInfo.EnemyId) == false)
        {
            //CDebug.Log( $"      [SetEnemy]    REBIRTH !!!!!     EnemyDic.Add : {pktInfo.EnemyId}" );
            EnemyDic.Add(pktInfo.EnemyId, new MGPihagiEnemy());
            EnemyDic[pktInfo.EnemyId].Initialize(WorldMgr, pktInfo);
            //ENEMY_POSDIR dir = GetEnemyDir(WorldMgr.GetVector3(pktInfo.PosX, 0, pktInfo.PosZ));
            //EnemyDic[pktInfo.EnemyId].SetEnemyAngleDir(dir);
        }
    }

    public MGPihagiEnemy GetEnemyByID(byte enemyID)
    {
        if (EnemyDic.ContainsKey(enemyID))
        {
            return EnemyDic[enemyID];
        }

        return null;
    }

    private void SetHideEnemy(byte enemyID)
    {
        if (EnemyDic.ContainsKey(enemyID))
        {
            SoundManager.Instance.SoundStop( SoundCategory.EFFECT );
            EnemyDic[enemyID].SetHideEnemyObject();
        }
    }

    public void SetHideAllEnemy()
    {
        foreach(byte enemyID in EnemyDic.Keys)
        {
            SetHideEnemy(enemyID);
        }
    }


    private void SetEnemyState(byte ID, PihagiGameEnemyState state)
    {
        if(EnemyDic.ContainsKey(ID))
        {
            if (EnemyDic[ID].GetEnemyState() != state)
            {
                EnemyDic[ID].SetEnemyState(state);
            }
        }
    }

    public bool CompareEnemyState(byte ID, PihagiGameEnemyState state)
    {
        if (EnemyDic.ContainsKey(ID))
        {
            if (EnemyDic[ID].GetEnemyState() == state)
            {
                return true;
            }
        }

        return false;
    }

    public void UpdateEnemyMovePos(PihagiGameSnapshotEnemy pktInfo)
    {
        if (EnemyDic.ContainsKey(pktInfo.EnemyId) == false)
        {
            CDebug.LogError($"UpdateEnemyMovePos() {pktInfo.EnemyId} is not contain EnemyDic", CDebugTag.MINIGAME_PIHAGI);
            return;
        }

        EnemyDic[pktInfo.EnemyId].UpdateMovePos(pktInfo);
    }

    public ENEMY_JUNPSTATE GetEnemyJumpState(ENEMY_POSDIR dir, Vector3 pos)
    {
        ENEMY_JUNPSTATE _retJumpState = ENEMY_JUNPSTATE.POS_UP;

        switch (dir)
        {
            case ENEMY_POSDIR.LEFT:
                if(bCheckJumpDistanceBetween(MGPihagiDefine.JUMPDOWN_LINE_POS_LEFT, pos.x))
                {
                    _retJumpState = ENEMY_JUNPSTATE.JUMPDOWN;
                }

                if(bCheckJumpDistanceBetween(MGPihagiDefine.JUMPUP_LINE_POS_RIGHT, pos.x))
                {
                    _retJumpState = ENEMY_JUNPSTATE.JUMPUP;
                }
                break;

            case ENEMY_POSDIR.TOP:
                if (bCheckJumpDistanceBetween(pos.z, MGPihagiDefine.JUMPDOWN_LINE_POS_TOP))
                {
                    _retJumpState = ENEMY_JUNPSTATE.JUMPDOWN;
                }

                if (bCheckJumpDistanceBetween(pos.z, MGPihagiDefine.JUMPUP_LINE_POS_DOWN))
                {
                    _retJumpState = ENEMY_JUNPSTATE.JUMPUP;
                }
                break;

            case ENEMY_POSDIR.RIGHT:
                if (bCheckJumpDistanceBetween(pos.x, MGPihagiDefine.JUMPDOWN_LINE_POS_RIGHT))
                {
                    _retJumpState = ENEMY_JUNPSTATE.JUMPDOWN;
                }

                if (bCheckJumpDistanceBetween(pos.x, MGPihagiDefine.JUMPUP_LINE_POS_LEFT))
                {
                    _retJumpState = ENEMY_JUNPSTATE.JUMPUP;
                }
                break;
        }

        return _retJumpState;
    }

    private bool bCheckJumpDistanceBetween(float pntA, float pntB)
    {
        float dist = pntA - pntB;

        return bCheckJumpDistance(dist);
    }

    private bool bCheckJumpDistance(float dist)
    {
        if(dist < MGPihagiDefine.CHECK_JUMP_DIST)
        {
            return true;
        }

        return false;
    }

	public void CleanUpEnemy()
	{
		if( EnemyDic != null)
		{
			foreach(MGPihagiEnemy enemy in EnemyDic.Values)
			{
				enemy.CleanUp();
			}
			EnemyDic.Clear();
		}

        if(EnemyCheckPlaySound != null)
        {
            foreach(ENEMY_POSDIR key in EnemyCheckPlaySound.Keys)
            {
                EnemyCheckPlaySound[key].Clear();
            }
            EnemyCheckPlaySound.Clear();
        }
	}

    public void Release()
    {
        if (WorldMgr != null) WorldMgr = null;		
		if (EnemyDic != null)
		{
			foreach(MGPihagiEnemy enemy in EnemyDic.Values)
			{
				enemy.Release();
			}
			EnemyDic.Clear();
			EnemyDic = null;
		}
		if(EnemyOriginObjDic != null)
		{			
			EnemyOriginObjDic.Clear();
			EnemyOriginObjDic = null;
		}
		if (DashFX_OrgObj != null) DashFX_OrgObj = null;
    }
}
