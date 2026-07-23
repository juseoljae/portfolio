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
using UnityEngine;
using System.Linq;//.Enumerable;

public class MGNunchiPlayersManager : MonoBehaviour
{
    public MGNunchiWorldManager WorldMgr;
    private MGNunchiPageUI PageUI;

    public BPWorldUIPoolManager NameTagPoolMgr;

    public Dictionary<int, MGNunchiPlayer> PlayersDic; // KEY:pid //now max 4 players
    public List<int> SortedPlayerUI; //pid

    public long MyPlayerUID;
    public int MyPlayerPID;

    //    [0]
    // [1]   [2]
    //    [3]
    private int[] playerPositionIndex = new int[] { 1, 3, 5, 7 };
    private int[] playerRotationValus = new int[] { 180, 110, -110, 0 };

    public int[] GetPlayerPositionIndex() { return playerPositionIndex; }

    private GameObject FX_ConfettiOrgObj;

    public void Initialize()
    {
        WorldMgr = MGNunchiManager.Instance.WorldManager;

        PageUI = WorldMgr.GetPageUI();

        PlayersDic = new Dictionary<int, MGNunchiPlayer>();
        SortedPlayerUI = new List<int>();
        NameTagPoolMgr = new BPWorldUIPoolManager();

        MyPlayerUID = long.Parse(CPlayer.GetStatusLoginValue(PLAYER_STATUS.USER_ID));

        SetNameTag();

        LoadFXConfettiOrgObj();
    }

    private void Update()
    {
        if (PlayersDic != null)
        {
            foreach (MGNunchiPlayer player in PlayersDic.Values)
            {
                player.UpdateStateMachine();
            }
        }
    }


    
    public void AddGamePlayers()
    {
        CleanUpPlayers();
        Dictionary<int, MGNunchiPlayerInfo> _playerSvrData = MGNunchiServerDataManager.Instance.GetGamePlayerInfoDic();
        if(_playerSvrData == null)
        {
            Debug.LogError("AddGamePlayers() Server Player Info is empty");
            return;
        }

        MyPlayerPID = MGNunchiServerDataManager.Instance.GetMYPlayerPID();

        CDebug.Log("AddGamePlayer() _playerSvrData.count= " + _playerSvrData.Count);
        foreach(KeyValuePair<int, MGNunchiPlayerInfo> playerInfo in _playerSvrData)
        {
            MGNunchiPlayer _player = new MGNunchiPlayer();
            int pid = playerInfo.Key;
            CDebug.Log( "AddGamePlayer() _playerSvrData.pid= " + pid );

            //if (MyPlayerUID == playerInfo.Value.UID)
            //{
            //    MyPlayerPID = playerInfo.Value.PID;
            //}

            if (PlayersDic.ContainsKey( pid ) == false)
            {
                PlayersDic.Add( pid, _player);
                CDebug.Log("AddGamePlayer() SortedPlayerUI.count= " + SortedPlayerUI.Count);
                SortedPlayerUI.Add( pid );

                string name = playerInfo.Value.NickName;
                int svrSeatIdx = playerInfo.Value.SeatID;
                CDebug.Log("      @@@@@@@   Player Seat Map Index = " + svrSeatIdx +"/"+ name);
                int rotValue = playerRotationValus[svrSeatIdx];
                int locMapIdx = playerPositionIndex[svrSeatIdx];

                PlayersDic[pid].Initialize( pid, name, locMapIdx, rotValue, svrSeatIdx, playerInfo.Value);
                //switch (playerInfo.Value.CharacterType)
                //{
                //    case CHARACTER_TYPE.AVATAR:
                //        AVATAR_TYPE avatarType = playerInfo.Value.AvatarType;
                //        break;
                //    case CHARACTER_TYPE.NPC:
                //        long npcID = playerInfo.Value.CharacterID;
                //        break;
                //}
            }
        }
    }

    private void LoadFXConfettiOrgObj()
    {
        CResourceData resData = CResourceManager.Instance.GetResourceData(MGNunchiConstants.CONFETTI_FX_PATH);
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

    public void SetAllPlayerLookAtCamera()
    {
        foreach(MGNunchiPlayer player in PlayersDic.Values)
        {
            player.LookAtCamera();
        }
    }

    //public void SetAllPlayerLookAtPlayingAngle()
    //{
    //    foreach (MGNunchiPlayer player in PlayersDic.Values)
    //    {
    //        player.SetPlayingRotation();
    //    }
    //}

    public GameObject GetPlayerDummyBone(int pid)
    {
        if (PlayersDic.ContainsKey(pid))
        {
            return PlayersDic[pid].GetUIDummyBone();
        }

        return null;
    }

    public void SetNameTag()
    {
        NameTagPoolMgr.NameTag.LoadNameTagPrefab( WorldMgr.GetPageUI().nameTagPoolParentObject  );
        NameTagPoolMgr.NameTag.GenerateNameTagPool( WorldMgr.GetPageUI().nameTagPoolParentObject );
    }

	public void ClearSortedPlayerUI()
	{
		SortedPlayerUI.Clear();
	}

    public void SetPlayerInfoUI()
    {
        //SortedPlayerUI just player order
        CDebug.Log("SetPlayerInfoUI() SortedPlayerUI.Count = "+ SortedPlayerUI.Count);
        for (int i=0; i<SortedPlayerUI.Count; ++i)
        {
            int _pid = SortedPlayerUI[i];
            PlayersDic[_pid].SetPlayerInfoUIInPageUI(i);
        }
    }

    //call from gain coin state
    public void SortPlayerUIDByScore()
    {
        Dictionary<int, int> _playerScore = new Dictionary<int, int>();

        foreach (KeyValuePair<int, MGNunchiPlayer> player in PlayersDic)
        {
            _playerScore.Add(player.Key, player.Value.PlayerInfo.CoinScore);
        }

        SortedPlayerUI.Clear();
        var _sortList = _playerScore.OrderByDescending(x => x.Value);
        foreach(var item in _sortList)
        {
            SortedPlayerUI.Add(item.Key);
        }

        //call SortPlayerUIDByScore()
        //then SetPlayerInfoUI()
    }

    public int GetIndexOfSortedPlayerUI(long uid)
    {
        for(int i=0; i< SortedPlayerUI.Count; ++i)
        {
            if(uid == SortedPlayerUI[i])
            {
                return i;
            }
        }
        return -1;
    }

    public List<int> GetSuccessMapIndexList()
    {
        List<int> _list = new List<int>();
        foreach (KeyValuePair<int, MGNunchiPlayer> player in PlayersDic)
        {
            int _mapIdx = player.Value.GetGainCoinSussessMapIdx();
            if(_mapIdx != -1)
            {
                _list.Add(_mapIdx);
            }
        }

        return _list;
    }

    public void SetActiveMyPlayerIndecator(bool bActive)
    {
        MGNunchiPlayer _myPlayer = GetMyPlayer();
        if (_myPlayer != null)
        {
            _myPlayer.SetActiveIndecatorObj(bActive);
        }
    }
    

    public MGNunchiPlayer GetMyPlayer()
    {
        if(PlayersDic.ContainsKey(MyPlayerPID))
        {
            return PlayersDic[MyPlayerPID];
        }

        return null;
    }

    public MGNunchiPlayer GetPlayer(int pid)
    {
        if(PlayersDic.ContainsKey(pid))
        {
            return PlayersDic[pid];
        }

        return null;
    }

	public void SetAllPlayerDefaultRotation()
	{
        foreach (MGNunchiPlayer player in PlayersDic.Values)
        {
			player.SetPlayerObjectRotation();
        }
	}

    public bool CheckAllPlayersRoundFinish()
    {
        foreach (MGNunchiPlayer player in PlayersDic.Values)
        {
            if (player.GetReturnFinished() == false)
            {
                return false;
            }
        }

        return true;
    }


	//return true if one of player fail
	public bool CheckFailPlayer()
	{
        foreach (MGNunchiPlayer player in PlayersDic.Values)
        {
			if( player.GetIsSuccess() == false) return true;
        }

		return false;
	}

    public void InitAllPlayersRoundFinish()
    {
        foreach (MGNunchiPlayer player in PlayersDic.Values)
        {
            player.SetReturnFinished( false );
        }
    }

    public void ChangeAllPlayersState(CState<MGNunchiPlayer> state)
    {
        foreach (MGNunchiPlayer player in PlayersDic.Values)
        {
            player.SetChangeState( state );
        }
    }

    public void SetNetPlayerEmoticonTagObj(int pid, GameObject emoticonObj)
    {
        if (PlayersDic.ContainsKey(pid))
        {
            PlayersDic[pid].SetEmoticonTag(emoticonObj);
        }
    }


    public void CleanUpPlayers()
    {
        foreach(MGNunchiPlayer player in PlayersDic.Values)
        {
            player.CleanUpPlayer();
        }

        PlayersDic.Clear();
        SortedPlayerUI.Clear();

        WorldMgr.ClearNameTagDic();
    }

    public void Release()
    {
        if (WorldMgr != null) WorldMgr = null;
        if(PlayersDic != null)
        {
            foreach (MGNunchiPlayer player in PlayersDic.Values)
            {
                player.Release();
            }
            PlayersDic.Clear();
            PlayersDic = null;
        }
        if(SortedPlayerUI != null)
        {
            SortedPlayerUI.Clear();
            SortedPlayerUI = null;
        }
        if(playerPositionIndex != null)
        {
            playerPositionIndex = null;
        }
        if(playerRotationValus != null)
        {
            playerRotationValus = null;
        }
        if (FX_ConfettiOrgObj != null)
        {
            FX_ConfettiOrgObj = null;
        }
        //Destroy( gameObject );
    }
}


