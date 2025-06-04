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

using System;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using DG.Tweening;

public class MGNunchiPlayer
{
    public MGNunchiWorldManager WorldMgr;
    private MGNunchiMapManager MapMgr;
    private MGNunchiPlayersManager PlayersMgr;
    private MGNunchiPageUI PageUI;

    public CCharacterController_BPWorld PlayerController;

    public CStateMachine<MGNunchiPlayer> PlayerSM;

    private RectTransform CanvasRect;
    private BPWorldLayerObject LayerObj;

    private GameObject UIDummyBone;
    public static string CHARACTER_UI_DUMMY_BONE = "Dummy_Name";

    private GameObject PlayerRootObj;

    public MGNunchiPlayerInfo PlayerInfo;

    public MGNunchiScoreFxUI ScoreGainFxUI;
    public GameObject ScoreGainFxUIObj;
	private SingleAssignmentDisposable ScoreFxDisposable;

    public MGNunchiScoreFxUI ScoreItemFxUI;
    public GameObject ScoreItemFxUIObj;

    public MGNunchiScoreFxUI ScoreLoseFxUI;
    public GameObject ScoreLoseFxUIObj;

    private int MySeatIdxFromSvr;
    public DIRECTION TargetDir;
    public int TargetMapArrIndex;
    public int TargetAngle;
    public bool IsSuccess;
    public float AnimLength_Sucess;
    public bool bStartTurnAround;
    public bool bReturnFinish;

    private int prevYAngle;
	
	private float JumpHeight;

    //Motion, Emoticon
    private GameObject NameTagObj;
    private GameObject EmoticonTagObj;

    private GameObject indecatorObj;
    private SingleAssignmentDisposable IndicatorObservable;

    private long AnimControllerGroupID;
    private string AnimControllerKeyID;

    public void Initialize(int pid, string name, int mapIdx, int rotValue, int svrSeatIdx, MGNunchiPlayerInfo playerInfo)
    {
        MGNunchiCanvasManager canvasMgr = (MGNunchiCanvasManager)MGNunchiManager.Instance.GetCanvasManager();
        CanvasRect = canvasMgr.GetComponent<RectTransform>();
        LayerObj = canvasMgr.LayerObjects;

        WorldMgr = MGNunchiManager.Instance.WorldManager;
        MapMgr = WorldMgr.MapMgr;
        PlayersMgr = WorldMgr.PlayerMgr;
        PlayerRootObj = PlayersMgr.gameObject;
        PageUI = WorldMgr.GetPageUI();

        SetPlayerInfo(pid, mapIdx, rotValue, svrSeatIdx, name, playerInfo);

        //SetPlayerComponent(CHARACTER_TYPE.AVATAR);
        SetPlayerComponent(playerInfo.CharacterType);

        PlayerSM = new CStateMachine<MGNunchiPlayer>(this);

        InitInfo();

        LoadScoreFxUI();

        LoadMyPlayerIndicator(PlayerInfo.PID);
    }



    public void InitInfo()
    {
        TargetDir = DIRECTION.NONE;
        TargetAngle = 0;
        TargetMapArrIndex = -1;
        AnimLength_Sucess = 0;
        SetIsSuccess(false);
        SetStartTurnAround(false);
    }

    public void UpdateStateMachine()
    {
        if(PlayerSM != null)
        {
            PlayerSM.StateMachine_Update();
        }
    }

    private void SetPlayerComponent(CHARACTER_TYPE charType)
    {
        PlayerController = PlayerInfo.PlayerObj.AddComponent<CCharacterController_BPWorld>();
        PlayerController.Initialize(charType);
        PlayerController.SetPlayerUID(PlayerInfo.UID);

        switch(charType)
        {
            case CHARACTER_TYPE.AVATAR:
                AnimControllerKeyID = string.Format( "{0}", PlayerInfo.UID );
                AnimControllerGroupID = (long)PlayerInfo.AvatarType;
                break;
            case CHARACTER_TYPE.NPC:
                long npcDataID = PlayerInfo.CharacterID;
                CNpcInfo npcData = CNpcDataManager.Instance.GetNpcInfo( npcDataID );
                AnimControllerKeyID = string.Format( "{0}{1}", PlayerInfo.UID, npcDataID );
                AnimControllerGroupID = npcData.AnimControllerGrpID;
                break;
        }
        PlayerController.SetRuntimeAnimatorController( AnimControllerKeyID, AnimControllerGroupID, ANIMCONTROLLER_USE_TYPE.BPW_MINIGAME );
    }

    /// <summary>
    /// Set Player information and movable map info
    /// </summary>
    /// <param name="pid">Player pid</param>
    /// <param name="mapIdx">one of 1, 3, 5, 7</param>
    private void SetPlayerInfo(int pid, int mapIdx, int rotValue, int svrSeatIdx, string name, MGNunchiPlayerInfo playerInfo)
    {
        PlayerInfo = MGNunchiServerDataManager.Instance.GetGamePlayerInfo(pid);

        PlayerInfo.CoinScore = 0;
        PlayerInfo.RotationValue = rotValue;
        PlayerInfo.GainItem = BPWPacketDefine.NunchiGameItemType.NONE;

        PlayerInfo.Map_MyLocation = new MGNunchiMap();
        PlayerInfo.Map_MyLocation.X = mapIdx / MGNunchiDefines.MAP_SIZE_4P_WIDTH;
        PlayerInfo.Map_MyLocation.Y = mapIdx % MGNunchiDefines.MAP_SIZE_4P_WIDTH;
        MySeatIdxFromSvr = svrSeatIdx;

        //Up, Down, Left, Right
        PlayerInfo.MapIdxByDir = new int[4];
        PlayerInfo.IsMovable = new bool[4];
        PlayerInfo.MovablePosition = new Vector3[PlayerInfo.IsMovable.Length];
        SetMovable();

        switch(playerInfo.CharacterType)
        {
            case CHARACTER_TYPE.AVATAR:
                PlayerInfo.StyleItem.SetPlayerEquipItemDic(PlayerInfo.AvatarType, PlayerInfo.StylingItemInfo, PlayerRootObj );
                PlayerInfo.PlayerObj = PlayerInfo.StyleItem.LoadNetAvatarObject(PlayersMgr.MyPlayerUID, PlayerInfo.UID, PlayerInfo.AvatarType, PlayerRootObj);
                //AnimControllerKeyID = string.Format("{0}", playerInfo.UID);
                //AnimControllerGroupID = (long)PlayerInfo.AvatarType;
                break;
            case CHARACTER_TYPE.NPC:
                long npcDataID = playerInfo.CharacterID;
                CNpcInfo npcData = CNpcDataManager.Instance.GetNpcInfo(npcDataID);
                CResourceData resData = CResourceManager.Instance.GetResourceData(npcData.NpcResID);
                GameObject _npcOrgObj = resData.Load<GameObject>(PlayerRootObj);

                PlayerInfo.PlayerObj = Utility.AddChild(PlayerRootObj, _npcOrgObj);
                //AnimControllerKeyID = string.Format( "{0}{1}", playerInfo.UID, npcDataID );
                //AnimControllerGroupID = npcData.AnimControllerGrpID;
                break;
        }

        PlayerInfo.PlayerAnimator = PlayerInfo.PlayerObj.GetComponent<Animator>();

        //RuntimeAnimatorController _runAnimController = CRuntimeAnimControllerSwitchManager.Instance.SetRuntimeAnimatorController( AnimControllerKeyID, AnimControllerGroupID, ANIMCONTROLLER_USE_TYPE.BPW_MINIGAME, PlayerInfo.PlayerObj );
        //if (_runAnimController != null)
        //{
        //    PlayerInfo.PlayerAnimator.runtimeAnimatorController = _runAnimController;
        //}

        UIDummyBone = PlayerInfo.PlayerObj.transform.Find(CHARACTER_UI_DUMMY_BONE).gameObject;

        WorldMgr.AddPlayerNameTag( PlayerInfo.UID, pid, name, PlayerInfo.PlayerObj);

        SetPlayerObject();
    }

    public void CleanUpPlayer()
    {
        TargetDir = DIRECTION.NONE;

        ScoreGainFxUI = null;
        GameObject.Destroy(ScoreGainFxUIObj);

        ScoreItemFxUI = null;
        GameObject.Destroy(ScoreItemFxUIObj);

        ScoreLoseFxUI = null;
        GameObject.Destroy(ScoreLoseFxUIObj);

        if (PlayerInfo.CharacterType == CHARACTER_TYPE.AVATAR)
        {
            if (PlayerInfo.StyleItem != null)
            {
                PlayerInfo.StyleItem.CleanUp();
            }
        }


        if(PlayersMgr.MyPlayerUID == PlayerInfo.UID)
        {
            if (indecatorObj != null)
            {
                IndicatorObservable.Dispose();
                UnityEngine.Object.Destroy(indecatorObj);
                indecatorObj = null;
            }
            if (PlayerInfo.CharacterType == CHARACTER_TYPE.AVATAR)
            {
                StaticAvatarManager.SetAvatarObjBack();
            }
            else
            {
                GameObject.Destroy(PlayerInfo.PlayerObj);
                PlayerInfo.PlayerObj = null;
            }
        }
        else
        {
            GameObject.Destroy(PlayerInfo.PlayerObj);
            PlayerInfo.PlayerObj = null;
        }

        //CRuntimeAnimControllerSwitchManager.Instance.ReleaseRuntimeAnimController( AnimControllerKeyID, AnimControllerGroupID );
        //PlayerInfo.PlayerAnimator.runtimeAnimatorController = null;
        PlayerController.ReleaseRuntimeAnimatorController();
        PlayerInfo.PlayerAnimator = null;
    }

    public void SetPlayerObject()
    {
        PlayerInfo.PlayerObj.transform.SetParent(PlayerRootObj.transform);
        PlayerInfo.PlayerObj.transform.localPosition = GetMapPositionByIndex(PlayerInfo.Map_MyLocation);

        SetPlayingRotation();

        prevYAngle = (int)PlayerInfo.PlayerObj.transform.localRotation.eulerAngles.y;
        PlayerInfo.PlayerObj.SetActive(true);
    }

    public void LookAtCamera()
    {
        Vector3 lookAtPos = WorldMgr.GetCurCamPos();
        lookAtPos.y = PlayerInfo.PlayerObj.transform.localPosition.y;

        PlayerInfo.PlayerObj.transform.LookAt(lookAtPos);
    }

    public void LookAtResultCamera()
    {
        if(PlayerInfo != null)
        {
            if (PlayerInfo.PlayerObj != null)
            {
                Transform playerObj = PlayerInfo.PlayerObj.transform;
                Vector3 myPos = playerObj.localPosition;
                int myPlayerSeatId = GetMySeatIdx();
                Vector3 CamTargetPosition = WorldMgr.GetResultCameraPosition( myPlayerSeatId );
                Vector3 lookAtCamPos = new Vector3( CamTargetPosition.x, myPos.y, CamTargetPosition.z );
                playerObj.LookAt( lookAtCamPos );
            }
        }
    }

    public void SetPlayingRotation()
    {
        Vector3 playerObjRot = new Vector3(0, PlayerInfo.RotationValue, 0);
        PlayerInfo.PlayerObj.transform.localRotation = Quaternion.Euler(playerObjRot);
    }

	public void SetPlayerObjectRotation()
	{		
        //PlayerInfo.PlayerObj.transform.localRotation = Quaternion.Euler(0, PlayerInfo.RotationValue, 0);
		Vector3 targetRot = new Vector3(0, PlayerInfo.RotationValue, 0);
		PlayerInfo.PlayerObj.transform.DOLocalRotate(targetRot, 0.2f);
	}

    public GameObject GetUIDummyBone()
    {
        return UIDummyBone;
    }

    public void SetPlayerInfoUIInPageUI(int idx)
    {
        PageUI.SetPlayerInfoUI(PlayerInfo, idx);

        RoundResultInfo _info = MGNunchiServerDataManager.Instance.GetRoundResultInfoDic(PlayerInfo.PID);
        if (_info != null)
        {
            PageUI.SetActiveBadgeObjByItemType(idx, _info.GainItemType);
        }
    }

    public MGNunchiMap GetMapInfoByDir(DIRECTION dir)
    {
        MGNunchiMap _map = new MGNunchiMap();

        switch(dir)
        {
            case DIRECTION.UP:
                _map.X = PlayerInfo.Map_MyLocation.X - 1;
                _map.Y = PlayerInfo.Map_MyLocation.Y;
                break;
            case DIRECTION.DOWN:
                _map.X = PlayerInfo.Map_MyLocation.X + 1;
                _map.Y = PlayerInfo.Map_MyLocation.Y;
                break;
            case DIRECTION.LEFT:
                _map.X = PlayerInfo.Map_MyLocation.X;
                _map.Y = PlayerInfo.Map_MyLocation.Y - 1;
                break;
            case DIRECTION.RIGHT:
                _map.X = PlayerInfo.Map_MyLocation.X;
                _map.Y = PlayerInfo.Map_MyLocation.Y + 1;
                break;
        }

        return _map;
    }

    private void SetMovable()
    {
        TargetDir = DIRECTION.NONE;
        //Up
        MGNunchiMap _mapInfo = GetMapInfoByDir(DIRECTION.UP);
        PlayerInfo.IsMovable[(int)DIRECTION.UP] = MapMgr.IsMovableMapByIndex(_mapInfo);
        PlayerInfo.MovablePosition[(int)DIRECTION.UP] = GetMapPositionByIndex(_mapInfo);
        int _mapIdx = MapMgr.GetMapIndexByXY(_mapInfo.X, _mapInfo.Y);
        PlayerInfo.MapIdxByDir[(int)DIRECTION.UP] = _mapIdx;
        //Down
        _mapInfo = GetMapInfoByDir(DIRECTION.DOWN);
        PlayerInfo.IsMovable[(int)DIRECTION.DOWN] = MapMgr.IsMovableMapByIndex(_mapInfo);
        PlayerInfo.MovablePosition[(int)DIRECTION.DOWN] = GetMapPositionByIndex(_mapInfo);
        _mapIdx = MapMgr.GetMapIndexByXY(_mapInfo.X, _mapInfo.Y);
        PlayerInfo.MapIdxByDir[(int)DIRECTION.DOWN] = _mapIdx;
        //Left
        _mapInfo = GetMapInfoByDir(DIRECTION.LEFT);
        PlayerInfo.IsMovable[(int)DIRECTION.LEFT] = MapMgr.IsMovableMapByIndex(_mapInfo);
        PlayerInfo.MovablePosition[(int)DIRECTION.LEFT] = GetMapPositionByIndex(_mapInfo);
        _mapIdx = MapMgr.GetMapIndexByXY(_mapInfo.X, _mapInfo.Y);
        PlayerInfo.MapIdxByDir[(int)DIRECTION.LEFT] = _mapIdx;
        //Right
        _mapInfo = GetMapInfoByDir(DIRECTION.RIGHT);
        PlayerInfo.IsMovable[(int)DIRECTION.RIGHT] = MapMgr.IsMovableMapByIndex(_mapInfo);
        PlayerInfo.MovablePosition[(int)DIRECTION.RIGHT] = GetMapPositionByIndex(_mapInfo);
        _mapIdx = MapMgr.GetMapIndexByXY(_mapInfo.X, _mapInfo.Y);
        PlayerInfo.MapIdxByDir[(int)DIRECTION.RIGHT] = _mapIdx;


        //if (PlayerInfo.UID == PlayersMgr.MyPlayerUID)
        //{
        //    CDebug.Log("    ######## SetMovable() avatar = " + PlayerInfo.UID + " / name = " + PlayerInfo.NickName + "///map loc = " + PlayerInfo.Map_MyLocation);
        //    for (int i = 0; i < PlayerInfo.IsMovable.Length; ++i)
        //    {
        //        //CDebug.Log($"    ######## dir:{(DIRECTION)i} map idx = {PlayerInfo.MapIdxByDir[i]}, movable idx = {PlayerInfo.IsMovable[i]}, movable Pos = {PlayerInfo.MovablePosition[i]}");
        //        CDebug.Log($"    ######## dir:{(DIRECTION)i}  movable idx = {PlayerInfo.IsMovable[i]}, map idx = {PlayerInfo.MapIdxByDir[i]}");
        //    }
        //}
    }

    public bool CheckMovableByMapInfo(MGNunchiMap mapInfo)
    {
        int _mapIdx = MapMgr.GetMapIndexByXY(mapInfo.X, mapInfo.Y);
        for(int i=0; i< PlayerInfo.MapIdxByDir.Length; ++i)
        {
            //CDebug.Log($"      ****** CheckMovableByMapInfo() Movable map Idx = {PlayerInfo.MapIdxByDir[i]}");
            if (PlayerInfo.MapIdxByDir[i] == _mapIdx)
            {
                if (PlayerInfo.IsMovable[i])
                {
                    //CDebug.Log($"      ****** CheckMovableByMapInfo() return true //// map Idx = {_mapIdx}");
                    return true;
                }
            }
        }

        return false;
    }

    private List<int> GetMovableDir()
    {
        List<int> _targetMapDir = new List<int>();

        for(int i=0; i< PlayerInfo.IsMovable.Length; ++i)
        {
            if(PlayerInfo.IsMovable[i])
            {
                _targetMapDir.Add(i);
            }
        }

        return _targetMapDir;
    }

    private DIRECTION GetJumpDirection(int mapIdx)
    {
        for (int i = 0; i < PlayerInfo.MapIdxByDir.Length; ++i)
        {
            if (PlayerInfo.MapIdxByDir[i] == mapIdx)
            {
                //CDebug.Log("   0000     ###### GetDirectionByMapIndex() return dir = "+ ((DIRECTION)i) + " / pid = " + PlayerInfo.UID);
                return (DIRECTION)i;
            }
        }


        return DIRECTION.NONE;
    }

    private DIRECTION GetDirectionByMapIndex(int mapIdx)
    {
        DIRECTION _dir = GetJumpDirection( mapIdx );

        if (_dir == DIRECTION.NONE)
        {
            _dir = GetJumpDirection( 4 );
        }
        //CDebug.Log("        ###### GetDirectionByMapIndex() return dir = DIRECTION.NONE / pid = " + PlayerInfo.UID);
        return _dir;
    }

    public void SetTargetMapIndex()
    {
        RoundResultInfo _info = MGNunchiServerDataManager.Instance.GetRoundResultInfoDic(PlayerInfo.PID);
        if (_info == null)
        {
            CDebug.LogError( "MGNunchiPlayer.SetTargetMapIndex() RoundResultInfo from server is null" );
            return;
        }

        int _mapIdx = WorldMgr.CoinItemSeatMapIdx[_info.TargetSvrMapID];
        TargetDir = GetDirectionByMapIndex(_mapIdx);
        CDebug.Log("        ###### Others SetTargetMapIndex() dir = " + TargetDir + " / pid = " + PlayerInfo.UID);

        SetTargetAngle();
        MapMgr.AddSelectMapIndex(PlayerInfo.UID, GetMapInfoByDir(TargetDir));
    }

    public void SetTargetDirectionInMap(DIRECTION dir)
    {
        TargetDir = dir;
        //CDebug.Log("        ###### SetTargetDirectionInMap() dir = " + TargetDir + " / pid = "+PlayerInfo.UID);
        SetTargetMapArrIndex();
    }

    private void SetTargetMapArrIndex()
    {
        MGNunchiMap _targetMap = GetMapInfoByDir(TargetDir);
        if (_targetMap == null)
        {
            CDebug.LogError( "MGNunchiPlayer.SetTargetMapArrIndex() MGNunchiMap is null" );
            return;
        }
        int _mapIdx = MapMgr.GetMapIndexByXY(_targetMap.X, _targetMap.Y);

        TargetMapArrIndex = WorldMgr.GetCoinItemSeatMapIdx(_mapIdx);
        //CDebug.Log($"             %%%%%%%%%%%% Req_RoundItemSeatChoice() pid = {PlayerInfo.UID}, _mapIdx = {_mapIdx} ({TargetMapArrIndex})");
        TCPMGNunchiRequestManager.Instance.Req_RoundItemSeatChoice((sbyte)TargetMapArrIndex);
    }

    public bool SetMapIndex(int _mapIdx)
    {       
        int seatMapIdx = WorldMgr.GetCoinItemSeatMapIdx(_mapIdx);

        if (seatMapIdx != TargetMapArrIndex)
        {
            TargetMapArrIndex = seatMapIdx;
            TargetDir = GetDirectionByMapIndex(_mapIdx);
            CDebug.Log($"             %%%%%%%%%%%% Req_RoundItemSeatChoice() pid = {PlayerInfo.UID}, _mapIdx = {_mapIdx} ({TargetMapArrIndex})");
            TCPMGNunchiRequestManager.Instance.Req_RoundItemSeatChoice((sbyte)TargetMapArrIndex);
            return true;
        }
        return false;
    }


    public DIRECTION GetTargetDirectionInMap()
    {
        //CDebug.Log("        ###### GetTargetDirectionInMap() TargetDir = " + TargetDir + " / pid = " + PlayerInfo.UID);
        return TargetDir;
    }

    public void SetTargetAngle()
    {
        switch(TargetDir)
        {
            case DIRECTION.UP:    TargetAngle = 0; break;
            case DIRECTION.DOWN:  TargetAngle = 180; break;
            case DIRECTION.LEFT:  TargetAngle = -90; break;
            case DIRECTION.RIGHT: TargetAngle = 90; break;
        }
    }

    public int GetTargetAngle()
    {
        return TargetAngle;
    }

    public int GetPrevAngle()
    {
        return prevYAngle;
    }

    public int GetReturnAngle(bool isFail = false)
    {
        int _angle = 0;

        if (isFail)
        {
            if (TargetDir == DIRECTION.NONE) return (int)PlayerInfo.PlayerObj.transform.localRotation.y;

            switch (TargetDir)
            {
                case DIRECTION.UP: TargetAngle = 0; break;
                case DIRECTION.DOWN: TargetAngle = 180; break;
                case DIRECTION.LEFT: TargetAngle = -90; break;
                case DIRECTION.RIGHT: TargetAngle = 90; break;
            }

            return _angle;
        }

        _angle = TargetAngle * (-1);
        if (_angle == -180)
            _angle = 0;
        else if (_angle == 0)
            _angle = 180;

        return _angle;
    }

    public int GetMySeatIdx()
    {
        return MySeatIdxFromSvr;
    }

    public MGNunchiMap GetMapIndexByDirectioin()
    {
        return GetMapInfoByDir(TargetDir);
    }

    public void SetIsSuccess(bool bsuc)
    {
        IsSuccess = bsuc;
    }

    public bool GetIsSuccess()
    {
        return IsSuccess;
    }

    public void SetStartTurnAround(bool bTurn)
    {
        bStartTurnAround = bTurn;
    }

    public bool GetStartTurnAround()
    {
        return bStartTurnAround;
    }

    public void SetReturnFinished(bool bFinish)
    {
        bReturnFinish = bFinish;
    }

    public bool GetReturnFinished()
    {
        return bReturnFinish;
    }

    public int GetGameCounCountOnTargetMap()
    {
        MGNunchiMap _map = GetMapInfoByDir(TargetDir);
        return MapMgr.GetGaimCoinCurrentOnEachMap(_map);
    }

    //public void SetCoinScore()
    //{
    //    PlayerInfo.CoinScore += GetGameCounCountOnTargetMap();

    //    Debug.Log("avatar = "+PlayerInfo.AvatarType+" / coinScore = "+ PlayerInfo.CoinScore);
    //}

    public int GetCoinScore()
    {
        return PlayerInfo.CoinScore;
    }

    public Vector3 GetTargetPositonInMap()
    {
        if(TargetDir == DIRECTION.NONE)
        {
            return PlayerInfo.PlayerObj.transform.localPosition;
        }

        Vector3 targetPos = PlayerInfo.MovablePosition[(int)TargetDir];
        Vector3 _pos = new Vector3(targetPos.x, PlayerInfo.PlayerObj.transform.localPosition.y, targetPos.z);

        return _pos;
    }

    public MGNunchiMap GetTargetMapInfo()
    {
        return GetMapInfoByDir(TargetDir);
    }

    public Vector3 GetMapPositionByIndex(MGNunchiMap map)
    {
        return WorldMgr.MapMgr.GetMapPosition(map.X, map.Y);
    }

    public void SetPlayerPosition(Vector3 pos)
    {
        PlayerInfo.PlayerObj.transform.localPosition = pos;
    }

    public bool GetCoinSuccess()
    {
        //Debug.Log(" 00000  %%%% GetCoinSuccess() avatar = " + PlayerInfo.AvatarType + " / pid = " + PlayerInfo.UID + " / TargetDir = " + TargetDir);
        if (TargetDir == DIRECTION.NONE)
        {
            return false;
        }

        //MGNunchiMap targetMapInfo = GetMapIndexByDirectioin();
        //bool bSuccess = !MapMgr.CompareSameMapIndex(PlayerInfo.UID, targetMapInfo);

        //follow Server Data
        RoundResultInfo _resultInfo = MGNunchiServerDataManager.Instance.GetRoundResultInfoDic(PlayerInfo.PID);
        if(_resultInfo == null)
        {
            Debug.LogError("MGNunchiPlayer.GetCoinSuccess() RoundResultInfo is null !!!! ");
            return false;
        }

        bool bSuccess = _resultInfo.bGainSuccess;

        SetIsSuccess(bSuccess);

        return bSuccess;
    }

    public int GetGainCoinSussessMapIdx()
    {
        if(GetCoinSuccess())
        {
            MGNunchiMap _map = GetMapIndexByDirectioin();
            return WorldMgr.MapMgr.GetMapIndexByXY(_map.X, _map.Y);
        }

        return -1;
    }

    public void LoadMyPlayerIndicator(int pid)
    {
        if (indecatorObj == null)
        {
            if (PlayersMgr.MyPlayerPID == pid)
            {
                CResourceData resData = CResourceManager.Instance.GetResourceData(MGNunchiConstants.MY_PLAYER_INDICATOR_PATH);
                GameObject obj = resData.Load<GameObject>(PlayerInfo.PlayerObj);
                indecatorObj = Utility.AddChild(UIDummyBone, obj);

                //float charHeight = 1.8f;//default
                //BPWorldCharacterData characterData = BPWorldDataManager.Instance.GetBPWorldCharacterDataByNpcDataID(PlayerInfo.CharacterID);
                //if(characterData != null)
                //{
                //    charHeight = characterData.Height;
                //}
                //indecatorObj.transform.localPosition = new Vector3(0, charHeight, 0);

                IndicatorObservable = new SingleAssignmentDisposable();
                IndicatorObservable.Disposable = Observable.EveryUpdate()
                    .Subscribe(x => 
                    {
                        if(indecatorObj != null)
                        {
                            indecatorObj.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
                        }
                    }).AddTo(PlayerInfo.PlayerObj);

                SetActiveIndecatorObj(false);
            }
        }
    }

    public void SetActiveIndecatorObj(bool bActive)
    {
        indecatorObj.SetActive(bActive);
    }

    public void LoadScoreFxUI()
    {
        CResourceData resData = CResourceManager.Instance.GetResourceData(MGNunchiConstants.SCORE_GAIN_FX_PATH);
        ScoreGainFxUIObj = Utility.AddChild(LayerObj.NameLayer, resData.Load<GameObject>(PlayerInfo.PlayerObj));
        ScoreGainFxUI = ScoreGainFxUIObj.GetComponent<MGNunchiScoreFxUI>();
        ScoreGainFxUI.Initialize(WorldMgr);
        ScoreGainFxUIObj.SetActive(false);

        resData = CResourceManager.Instance.GetResourceData(MGNunchiConstants.SCORE_LOSE_FX_PATH);
        ScoreLoseFxUIObj = Utility.AddChild(LayerObj.NameLayer, resData.Load<GameObject>(PlayerInfo.PlayerObj));
        ScoreLoseFxUI = ScoreLoseFxUIObj.GetComponent<MGNunchiScoreFxUI>();
        ScoreLoseFxUI.Initialize(WorldMgr);
        ScoreLoseFxUIObj.SetActive(false);

        resData = CResourceManager.Instance.GetResourceData(MGNunchiConstants.SCORE_GAINITEM_FX_PATH);
        ScoreItemFxUIObj = Utility.AddChild(LayerObj.NameLayer, resData.Load<GameObject>(PlayerInfo.PlayerObj));
        ScoreItemFxUI = ScoreItemFxUIObj.GetComponent<MGNunchiScoreFxUI>();
        ScoreItemFxUI.Initialize(WorldMgr);
        ScoreItemFxUIObj.SetActive(false);
    }

    public void AddScoreFxUI(int gainScore, BPWPacketDefine.NunchiGameItemType itemType, MGNunchiPlayer player)
    {
        GameObject scoreFxUIObj = null;
        MGNunchiScoreFxUI scoreFxUI = null;
        FollowObjectPositionFor2D fop = null;

        //CDebug.Log("             #### AddScoreFxUI() itemType = " + itemType +"/"+PlayerInfo.UID +"/ gain Score = "+ gainScore);

        if (itemType == BPWPacketDefine.NunchiGameItemType.COIN)
        {
            if (gainScore >= 0)
            {
                //scoreFxUI = ScoreGainFxUI;
                ScoreGainFxUI.SetData(gainScore, itemType);
                scoreFxUIObj = ScoreGainFxUIObj;
                player.PlayMySound( 6830016 ); // list 115
            }
            else
            {
                //scoreFxUI = ScoreLoseFxUI;
                ScoreLoseFxUI.SetData(gainScore, itemType);
                scoreFxUIObj = ScoreLoseFxUIObj;
                player.PlayMySound( 6830017 ); // list 116
            }
        }
        else
        {
            ScoreItemFxUI.SetData(0, itemType);
            scoreFxUIObj = ScoreItemFxUIObj;
        }

        GameObject dummyObj = WorldMgr.PlayerMgr.GetPlayerDummyBone(PlayerInfo.PID);
        scoreFxUIObj.SetActive(true);

        //scoreFxUI.SetData(gainScore, itemType);
        fop = GameObjectHelperUtils.GetOrAddComponent<FollowObjectPositionFor2D>(scoreFxUIObj);
        fop.Init(CanvasRect, dummyObj.transform, MGNunchiDefines.SCORE_UI_ADD_HEIGHT, 0.0f, true);


        ScoreFxDisposable = new SingleAssignmentDisposable();
        ScoreFxDisposable.Disposable = Observable.Timer(TimeSpan.FromSeconds(2))
        .Subscribe(_ =>
        {
            scoreFxUIObj.SetActive(false);
            ScoreFxDisposable.Dispose();
        })
        .AddTo(PlayerInfo.PlayerObj);
    }

    public bool CanAddScoreFxUI(int gainCoinCount, BPWPacketDefine.NunchiGameItemType type)
    {
        if (gainCoinCount == 0) return false;

        if (type == BPWPacketDefine.NunchiGameItemType.NONE) return false;

        return true;
    }

    public void SetNameTag(GameObject nameTag)
    {
        if (NameTagObj != null)
        {
            UnityEngine.Object.Destroy(NameTagObj);
        }
        NameTagObj = nameTag;
    }

    public void SetEmoticonTag(GameObject emoticonObj)
    {
        if (EmoticonTagObj != null)
        {
            UnityEngine.Object.Destroy(EmoticonTagObj);
        }
        EmoticonTagObj = emoticonObj;
    }

    public void SetChangeState(CState<MGNunchiPlayer> state)
    {
        PlayerSM.ChangeState(state);
    }

    public float GetAnimLength(string animName)
    {
        float time = 0;
        RuntimeAnimatorController ac = PlayerController.MyAnimator.runtimeAnimatorController;

        for (int i = 0; i < ac.animationClips.Length; i++)
        {
            if (ac.animationClips[i].name == animName)
            {
                time = ac.animationClips[i].length;
            }
        }

        return time;
    }

    //public void SetJumpAnimation(BPWPacketDefine.JumpState jumpState)
    //{
    //    if (PlayerController)
    //    {
    //        switch (jumpState)
    //        {
    //            case BPWPacketDefine.JumpState.JUMP:
    //                PlayerController.SetNunchiJump(true);
    //                //PlayerController.SetUserJump(true);
    //                break;
    //            //case BPWPacketDefine.JumpState.LANDING:
    //            //    PlayerController.SetUserJump(false);
    //            //    break;
    //            //case BPWPacketDefine.JumpState.NONE:
    //            //    break;
    //        }
    //    }
    //}

    public void PlayAnimation(string animParam)
    {
        if(PlayerController == null)
        {
            Debug.LogError($"MGNunchiPlayer.PlayAnimation. controller is null. param = {animParam}");
            return;
        }

        PlayerController.SetChangeMotionStateWithParameter(MOTION_STATE.PLAYANIM, animParam);
    }


	public void SetJump(Vector3 targetPos, MGNunchiMap _mapInfo)
	{
		if (GetTargetDirectionInMap() != DIRECTION.NONE)
        {
			PlayerController.SetAnimationTrigger(MGNunchiDefines.ANIM_JUMP_START);
			var jumpStartDisposer = new SingleAssignmentDisposable();
			jumpStartDisposer.Disposable = Observable.Timer( TimeSpan.FromSeconds( MGNunchiDefines.JUMP_START_TIME ) )
			.Subscribe( _ =>
			 {
				 PlayerController.SetAnimationTrigger( MGNunchiDefines.ANIM_JUMP_FINISH );
				 jumpStartDisposer.Dispose();
			 } )
			.AddTo( WorldMgr );

			//go to jump
			PlayerInfo.PlayerObj.transform.DOLocalMove(targetPos, MGNunchiDefines.JUMP_DUR_TIME).SetEase(Ease.Linear)
			.OnComplete(() => 
			{
				if (WorldMgr.GetIsTimeToCleanCoinItemUpValue() == false)
				{
					WorldMgr.SetIsTimeToCleanCoinItemUp(true);
                }
            }); 
			JumpUp();
		}

		
		//Platform(puding) animation
        var jumpDisposer = new SingleAssignmentDisposable();
        jumpDisposer.Disposable = Observable.Timer(TimeSpan.FromSeconds(MGNunchiDefines.JUMP_DUR_TIME))
        .Subscribe(_ =>
        {
            MGNunchiMapInfo _map = WorldMgr.MapMgr.GetMapInfoByXY(_mapInfo.X, _mapInfo.Y);
            _map.SetActiveDustFxObj(true);
            WorldMgr.MapMgr.PlayJumpDownAnimByInfo(_mapInfo);
            jumpDisposer.Dispose();
        })
        .AddTo(WorldMgr);
	}


	public void JumpUp()
	{
		JumpHeight = PlayerInfo.PlayerObj.transform.localPosition.y;
		float targetHeight = PlayerInfo.PlayerObj.transform.localPosition.y + MGNunchiDefines.JUMP_HEIGHT;
		float jumpDuringTime = MGNunchiDefines.JUMP_DUR_TIME/2 + MGNunchiDefines.JUMP_START_DELAY_TIME;

		DOTween.To( () => JumpHeight,
			changevalue =>
			{
				SetJumpHeight(changevalue);
			},
			targetHeight, jumpDuringTime
			);
		
		var jumpDownDisposer = new SingleAssignmentDisposable();
		jumpDownDisposer.Disposable = Observable.Timer( TimeSpan.FromSeconds( jumpDuringTime ) )
		.Subscribe( _ =>
			{
				JumpHeight = targetHeight;
				DOTween.To( () => JumpHeight,
				changevalue =>
				{
					SetJumpHeight(changevalue);
				},
				0, MGNunchiDefines.JUMP_DUR_TIME/2
				);
				jumpDownDisposer.Dispose();
			} )
		.AddTo( WorldMgr );
	}

	private void SetJumpHeight(float value)
	{
		Vector3 curPosition = new Vector3(PlayerInfo.PlayerObj.transform.localPosition.x, value, PlayerInfo.PlayerObj.transform.localPosition.z);
		PlayerInfo.PlayerObj.transform.localPosition = curPosition;
	}

    public bool IsMyPlayer()
    {
        if (PlayersMgr.MyPlayerUID == PlayerInfo.UID)
            return true;

        return false;
    }

    public void PlayMySound(long soundID)
    {
        if(IsMyPlayer())
        {
            SoundManager.Instance.PlayEffect( soundID ); 
        }
    }


    public void Release()
    {
        if (WorldMgr != null) WorldMgr = null;
        if (MapMgr != null) MapMgr = null;
        if (PlayersMgr != null) PlayersMgr = null;
        if (PageUI != null) PageUI = null;
        if (PlayerController != null)
        {
            PlayerController.ReleaseRuntimeAnimatorController();
            PlayerController.DestroyComponents();
            UnityEngine.Object.Destroy(PlayerController);
            PlayerController = null;
        }
        if (PlayerSM != null) PlayerSM = null;
        if (CanvasRect != null) CanvasRect = null;
        if (LayerObj != null) LayerObj = null;
        if (UIDummyBone != null) UIDummyBone = null;
        if (PlayerRootObj != null) PlayerRootObj = null;
        if (PlayerInfo != null)
        {
            //CRuntimeAnimControllerSwitchManager.Instance.ReleaseRuntimeAnimController( AnimControllerKeyID, AnimControllerGroupID );
            //PlayerInfo.PlayerAnimator.runtimeAnimatorController = null;
            PlayerInfo.PlayerAnimator = null;
            PlayerInfo.PlayerObj = null;
            PlayerInfo = null;
        }
        if (ScoreGainFxUI != null) ScoreGainFxUI = null;
        if (ScoreGainFxUIObj != null) ScoreGainFxUIObj = null;
        if (ScoreItemFxUI != null) ScoreItemFxUI = null;
        if (ScoreItemFxUIObj != null) ScoreItemFxUIObj = null;
        if (ScoreLoseFxUI != null) ScoreLoseFxUI = null;
        if (ScoreLoseFxUIObj != null) ScoreLoseFxUIObj = null;
		if (ScoreFxDisposable != null)
		{
			ScoreFxDisposable.Dispose();
			ScoreFxDisposable = null;
		}
        if (indecatorObj != null)
        {
            IndicatorObservable.Dispose();
            UnityEngine.Object.Destroy(indecatorObj);
            indecatorObj = null;
        }
	}
}