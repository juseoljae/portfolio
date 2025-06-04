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

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking.Types;
using BPWPacketDefine;
using BPWPacket;

public class MGPihagiPlayersManager : MonoBehaviour
{
    private MGPihagiWorldManager WorldMgr;

    private MGPihagiPlayerMoveRequester MyPlayerMoveReauester = null;

    private MGPihagiPageUI PageUI { get => WorldMgr.GetPageUI(); }

    public BPWorldUIPoolManager NameTagPoolMgr;
    public long MyPlayerUID { get; private set; }
    public int MYPID { get; private set; }

    public Dictionary<int, MGPihagiPlayer> PlayersDic;  //KEY: PID

    private List<int> SortedPlayerUI;

    //Stun origin object
    public GameObject StunFX_OrgObj;

    public GameObject InvcbFX_OrgObj;

    private GameObject FX_ConfettiOrgObj;

    public void Initialize(MGPihagiWorldManager worldMgr)
    {
        WorldMgr = worldMgr;

        PlayersDic = new Dictionary<int, MGPihagiPlayer>();
        this.MyPlayerMoveReauester = new MGPihagiPlayerMoveRequester();
        NameTagPoolMgr = new BPWorldUIPoolManager();
        SortedPlayerUI = new List<int>();
        this.MyPlayerUID = long.Parse(CPlayer.GetStatusLoginValue(PLAYER_STATUS.USER_ID));

		SetNameTag();

        LoadFXOrigin();
    }

    public void LoadFXOrigin()
    {
        LoadStunFXOrigin();
        LoadInvincibleFXOrigin();
        LoadConfettiFXOrigin();
    }

    private void LoadStunFXOrigin()
    {
        CResourceData resData = CResourceManager.Instance.GetResourceData(MGPihagiConstants.FX_STUN_PATH);
        StunFX_OrgObj = resData.Load<GameObject>(this.gameObject);
    }

    private void LoadInvincibleFXOrigin()
    {
        CResourceData resData = CResourceManager.Instance.GetResourceData(MGPihagiConstants.FX_INVINCIBLE_PATH);
        InvcbFX_OrgObj = resData.Load<GameObject>(this.gameObject);
    }

    private void LoadConfettiFXOrigin()
    {
        CResourceData resData = CResourceManager.Instance.GetResourceData(MGPihagiConstants.CONFETTI_FX_PATH);
        FX_ConfettiOrgObj = resData.Load<GameObject>(this.gameObject);
    }

    public GameObject GetFXConfettiOrgObj()
    {
        if(FX_ConfettiOrgObj != null)
        {
            return FX_ConfettiOrgObj;
        }

        return null;
    }

    // Update is called once per frame
    void Update()
    {
        if (PlayersDic != null)
        {
            foreach (MGPihagiPlayer player in PlayersDic.Values)
            {
                player.UpdateStateMachine();
            }
        }
    }

    public void SetNetPlayer()
    {
        List<MGPihagiPlayerInfo> _playerList = MGPihagiServerDataManager.Instance.GetAllGamePlayerInfoList();
        MYPID = MGPihagiServerDataManager.Instance.GetMyPlayerPID();

        CDebug.Log($"      ############## SetNetPlayer() player count :{_playerList.Count}");
        for(int i=0;i <_playerList.Count; ++i)
        {
            MGPihagiPlayer _player = new MGPihagiPlayer();
            MGPihagiPlayerInfo _info = _playerList[i];
            int pid = _info.PlayerID;

            if(PlayersDic.ContainsKey(pid) == false)
            {
                PlayersDic.Add(pid, _player);

                SortedPlayerUI.Add(pid);
				CDebug.Log($"      ############## SetNetPlayer() SortedPlayerUI.Add :{pid}, player name = {_info.NickName}, uid = {_info.UID}");
                                
                PlayersDic[pid].Initialize(pid);

                if (pid == MYPID)
                {
                    //MYPlayerPID = pid;

                    MyPlayerMoveReauester.Initialize();
                    MyPlayerMoveReauester.SetTargetPlayer( PlayersDic[pid] );
                    MyPlayerMoveReauester.SetTargetPlayerController(PlayersDic[pid].PlayerController);
                    MyPlayerMoveReauester.SetMoveState(EBPWorldCharacterMoveRequesterState.START);
                }
				WorldMgr.AddNameTag(pid, PlayersDic[pid].PlayerInfo.NickName, PlayersDic[pid].PlayerInfo.PlayerObj);
            }

        }
    }

    public void SetNameTag()
    {
        NameTagPoolMgr.NameTag.LoadNameTagPrefab( WorldMgr.GetPageUI().nameTagPoolParentObject );
        NameTagPoolMgr.NameTag.GenerateNameTagPool( WorldMgr.GetPageUI().nameTagPoolParentObject );
    }

    public void SetPlayerInfoUI()
    {
        for(int i=0; i<SortedPlayerUI.Count; ++i)
        {
            int _pid = SortedPlayerUI[i];
            MGPihagiPlayerInfo playerInfo = PlayersDic[_pid].PlayerInfo;

            PageUI.SetPlayerInfoUI(playerInfo, i);

			if(playerInfo.UID == MyPlayerUID)
			{
				SetMyPlayerMoveState(EBPWorldCharacterMoveRequesterState.START);
			}
        }
    }

    //call from gain coin state
    public void SortPlayerUIDByScore()
    {
        Dictionary<int, int> _playerLifeCount = new Dictionary<int, int>();

        foreach(KeyValuePair<int, MGPihagiPlayer> player in PlayersDic)
        {
            _playerLifeCount.Add(player.Key, player.Value.PlayerInfo.LifeCount);
        }

        SortedPlayerUI.Clear();
        var _sortList = _playerLifeCount.OrderByDescending(x => x.Value);
        foreach (var item in _sortList)
        {
            SortedPlayerUI.Add(item.Key);
        }

        //call SortPlayerUIDByScore()
        //then SetPlayerInfoUI()
    }


    public void SetPlayerBySnapShotInfo(PihagiGameSnapshotPlayer pktInfo)
    {
        switch (pktInfo.State)
        {
            case PihagiGamePlayerState.DEATH:
                SetAttackerEnemyInfo(pktInfo);
                break;
            //case PihagiGamePlayerState.REBIRTH:
            //    SpawnPlayer(pktInfo);
            //    break;
			case PihagiGamePlayerState.IDLE:
				break;
            case PihagiGamePlayerState.MOVE:
            //case PihagiGamePlayerState.INVINCIBLE:
                UpdatePlayerMove(pktInfo);
                break;
            case PihagiGamePlayerState.STUN:
                SetPlayerLife(pktInfo.PlayerId, pktInfo.Life);
                break;
        }

        int pid = pktInfo.PlayerId;

        if (PlayersDic.ContainsKey(pid) == false)
        {
            CDebug.LogError($"SetPlayerBySnapShotInfo() {pid} is not contain PlayersDic", CDebugTag.MINIGAME_PIHAGI);
            return;
        }

        PlayersDic[pid].SetPlayerState(pktInfo.State, pktInfo.IsInvincible);
    }

    private void UpdatePlayerMove(PihagiGameSnapshotPlayer pktInfo)
    {
        int pid = pktInfo.PlayerId;

        if (PlayersDic.ContainsKey(pid) == false)
        {
            CDebug.LogError($"UpdatePlayerMove() {pid} is not contain PlayersDic", CDebugTag.MINIGAME_PIHAGI);
            return;
        }

        PlayersDic[pid].UpdateMovePos(WorldMgr.GetVector3(pktInfo.PosX, 0, pktInfo.PosZ));
    }

	public void SetMyPlayerMoveState(EBPWorldCharacterMoveRequesterState state)
	{
		MyPlayerMoveReauester.SetMoveState(state);
	}

    public void SetPlayerLife(int pid, int lifeCount)
    {
        if (PlayersDic.ContainsKey(pid) == false)
        {
            CDebug.LogError($"SetPlayerLife() {pid} is not contain PlayersDic", CDebugTag.MINIGAME_PIHAGI);
            return;
        }
        //Set Life Count from Server Data
        PlayersDic[pid].SetLifeCount(lifeCount);
        //then sort
        SortPlayerUIDByScore();
        SetPlayerInfoUI();
    }

    private void SetAttackerEnemyInfo(PihagiGameSnapshotPlayer pktInfo)
    {
        int pid = pktInfo.PlayerId;

        if (PlayersDic.ContainsKey(pid) == false)
        {
            CDebug.LogError($"SetAttackerEnemyID() {pid} is not contain PlayersDic", CDebugTag.MINIGAME_PIHAGI);
            return;
        }

        MGPihagiEnemy _attacker = WorldMgr.EnemyMgr.GetEnemyByID(pktInfo.AttackerEnemyId);
		if(_attacker != null)
		{
            PlayersDic[pid].SetDeath(_attacker);
		}
    }

     
    public void SetPlayerJump(PihagiGameJumpBc pktInfo)
    {
        int pid = pktInfo.PlayerId;

        if (PlayersDic.ContainsKey(pid) == false)
        {
            CDebug.LogError($"SetPlayerJump() {pid} is not contain PlayersDic", CDebugTag.MINIGAME_PIHAGI);
            return;
        }

        bool isJump = false;

        if (pktInfo.JumpState == JumpState.JUMP) isJump = true;

        PlayersDic[pid].SetJump(isJump);
    }

    public Vector3 GetDeathTargetPos()
    {
        Vector3 targetPos = WorldMgr.PlayerDeathTargetPos[0];



        return targetPos;
    }

    public void SetHideOtherPlayer()
    {
        foreach(MGPihagiPlayer player in PlayersDic.Values)
        {
            if(player.PlayerInfo.PlayerID != MYPID)
            {
				player.SetActivePlayerObj(false);
            }
        }
    }

	public void SetActiveMyPlayer()
	{
		GetMyPlayer().SetActivePlayerObj(true);
	}

    public void SetActiveIndicatorOfMyPlayer(bool bActive)
    {
        GetMyPlayer().SetActiveIndecatorObj(bActive);
    }

    public MGPihagiPlayer GetMyPlayer()
    {
        if (PlayersDic.ContainsKey( MYPID ))
        {
            return PlayersDic[MYPID];
        }

        return null;
    }

	public MGPihagiPlayer GetPlayer(int pid)
	{
        if (PlayersDic.ContainsKey(pid))
        {
			return PlayersDic[pid];
        }		
            
		return null;
	}

	public long GetPlayerUIDByPID(int pid)
	{
        if (PlayersDic.ContainsKey(pid) == false)
        {
            CDebug.LogError($"GetPlayerUIDByPID() {pid} is not contain PlayersDic", CDebugTag.MINIGAME_PIHAGI);
            return -1;
        }

		return PlayersDic[pid].PlayerInfo.UID;
	}

    public GameObject GetPlayerDummyBone(int pid)
    {
        return PlayersDic[pid].GetUIDummyBone();
    }


	public void SetChangeAllPlayerState(CState<MGPihagiPlayer> state)
	{
		foreach(MGPihagiPlayer player in PlayersDic.Values)
		{
			player.SetChangeState(state);
		}
	}


    public void CleanUpPlayers()
    {
        foreach (MGPihagiPlayer player in PlayersDic.Values)
        {
            player.CleanUpPlayer();
        }

        PlayersDic.Clear();
        SortedPlayerUI.Clear();
    }

    public void Release()
    {
        if (WorldMgr != null) WorldMgr = null;
        if (MyPlayerMoveReauester != null) MyPlayerMoveReauester = null;
        if (NameTagPoolMgr != null) NameTagPoolMgr = null;
        if (PlayersDic != null)
        {
            foreach(MGPihagiPlayer player in PlayersDic.Values)
            {
                player.Release();
            }
            PlayersDic.Clear();
            PlayersDic = null;
        }
        if (SortedPlayerUI != null)
        {
            SortedPlayerUI.Clear();
            SortedPlayerUI = null;
        }
		if (StunFX_OrgObj != null)
		{
			StunFX_OrgObj = null;
        }
        if (InvcbFX_OrgObj != null)
        {
            InvcbFX_OrgObj = null;
        }
        if (FX_ConfettiOrgObj != null)
        {
            FX_ConfettiOrgObj = null;
        }
    }
}
