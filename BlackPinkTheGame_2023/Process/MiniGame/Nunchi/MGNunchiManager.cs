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

using System;
using UnityEngine;
using UniRx;
using ui.navigation;
using FlatBuffers;
using BPWPacketDefine;
using System.Collections.Generic;
using System.Linq;



public class MGNunchiManager : BaseSceneManager
{
    #region MEMBER_VAR
    static MGNunchiManager s_this = null;
    public static MGNunchiManager Instance { get { return s_this; } }
    #endregion MEMBER_VAR

    public MGNunchiWorldManager NunchiWorldManager;
    public GameObject WorldMgrObj;
    private GameObject ScoreUILayer;

    public GameObject NameTagLayerObj;

    private SingleAssignmentDisposable smDisposer;

    private SingleAssignmentDisposable roundEndDisposer;
    public bool check_round_end_execute { get; set; } = false;


    public MGNunchiWorldManager WorldManager
    {
        get { return NunchiWorldManager; }
        set { NunchiWorldManager = value; }
    }

    public Dictionary<int, RoundResultInfo> dic_player_round_result_infos = null;
    private Dictionary<int, List<int>> targetSvrMapID = null;

    public int round_count { get; set; } = 0;

    public int nunchiGameRoundItemSeatChoiceRes { get; set; } = -1;

    public bool is_nunchi_game_leave { get; set; } = false;

    private void Awake()
    {
        s_this = this;
    }



    #region SceneManager Life Cycle
    public override void Initalize(CBaseSceneExtends _scene)
    {
        CDebug.Log($"{GetType()} Initialize ");

        scene = _scene;

        cUIService = CCoreServices.GetCoreService<CUIService>();
        //entranceService = CCoreServices.GetCoreService<EntranceProcessService>();

        pageRootObject = cUIService.GetContentsRootObject((int)SceneUIID.MINIGAME_NUNCHI_UI);
        CDebug.Log($"{GetType()} PageRootObject : {pageRootObject}");

        WorldMgrObj = GameObject.Find("MiniGame_Nunchi");
        NunchiWorldManager = WorldMgrObj.GetComponent<MGNunchiWorldManager>();

        var canvas = pageRootObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = CDirector.Instance.ui_camera;

        if (BPWorldManager.is_bpworld_standalone)
        {
            dic_player_round_result_infos = new Dictionary<int, RoundResultInfo>();

            // 유저별 값 집합을 정의
            targetSvrMapID = new Dictionary<int, List<int>>
            {
                { 0, new List<int> { 0, 1, 4 } }, // 유저 0의 값
                { 1, new List<int> { 0, 4, 2 } }, // 유저 1의 값
                { 2, new List<int> { 1, 4, 3 } }, // 유저 2의 값
                { 3, new List<int> { 2, 4, 3 } }  // 유저 3의 값
            };

        }
    }


    public override void ChangeProcess(ProcessStep _step)
    {
        processStep = _step;

        CDebug.Log($"{GetType()} NUnchi manager ChangeProcess {processStep.ToString()} ", CDebugTag.MINIGAME_NUNCHI);

        switch (_step)
        {
            case ProcessStep.Start:
                if (pageRootObject == null)
                {
                    throw new Exception($"{GetType()} : RootObject is null");
                }

                is_nunchi_game_leave = false;
                ChangeProcess(ProcessStep.Step1);
                break;
            case ProcessStep.Step1:

                canvas_manager = pageRootObject.GetComponent<MGNunchiCanvasManager>();
                var assetService = CCoreServices.GetCoreService<CAssetService>();

                var pageManager = NavigationManager.GetPageManager(SceneID.MINIGAME_NUNCHI_SCENE);
                pageManager.SetPageBackProcess(PageManager.BackProcessType.BACK_STACKORDER);

                pageManager.AddPage<MGNunchiPageUI>(canvas_manager.GetSafeAreaObject(),
                    assetService.Get_Object_Info(AssetPathDefine.TRANSACTION_MINIGAME_NUNCHI_UI_PREFAB));

                //CreateLayer(ref ScoreUILayer, assetService, "PlayerScoreUILayer");

                //NameTagLayerObj = Utility.AddChild
                //    (
                //    canvas_manager.GetSafeAreaObject(),
                //    assetService.Get_Object_Info(AssetPathDefine.TRANSACTION_MINIGAME_NUNCHI_LAYERGROUP1_PREFAB)
                //    );
                //NameTagLayerObj.name = name;
                //NameTagLayerObj.SetActive(true);
                //NameTagLayerObj.transform.SetAsFirstSibling();

                ChangeProcess(ProcessStep.Step2);
                break;

            case ProcessStep.Step2:

                //#if UNITY_EDITOR
                //                string IP = "172.27.10.8";//"172.27.14.140";
                //                uint PORT = 14000;
                //                uint grpID = 3000;
                //                uint roomID = 1;

                //                //MGNunchiServerDataManager.Instance.SetTournamentGameRoomInfo(IP, PORT, grpID, roomID);

                //                if (MGNunchiServerDataManager.Instance.EnterType == EMGNunchiEnterType.DIRECT_NUNCHI )
                //                {
                //                    MGNunchiServerDataManager.Instance.SetTournamentGameRoomInfo(IP, PORT, grpID, roomID, 1);
                //                    TCPConnector.Instance.Connect(IP, PORT);
                //                }
                //                else
                //#endif
                {
                    if (MGNunchiServerDataManager.Instance.EnterType == EMGNunchiEnterType.NEED_REQ_LOGIN)
                    {
                        CDebug.Log("     $$$$$$$$$$$$$        MGNunchiManager.Step2.........SendReqLogin", CDebugTag.MINIGAME_NUNCHI);
                        if (BPWorldManager.is_bpworld_standalone)
                        {
                            CDebug.Log("MGNunchiManager.Step2.........SendReqLogin - TCPConnector.Instance.Connector.Session.SendHander(BPWPacket.BPWPacketId.LoginRes);", CDebugTag.BPWORLD_CONNECTION_ENTERWORLD);
                            TCPConnector.Instance.Connector.Session.SendHander(BPWPacket.BPWPacketId.LoginRes);
                        }
                        else
                        {
                            TCPCommonRequestManager.Instance.SendReqLogin();
                        }
                    }
                    else if (MGNunchiServerDataManager.Instance.EnterType == EMGNunchiEnterType.ROOM_ENTER)
                    {
                        CDebug.Log("     $$$$$$$$$$$$$        MGNunchiManager.Step2.........Req_RoomEnter", CDebugTag.MINIGAME_NUNCHI);
                        TCPMGNunchiRequestManager.Instance.Req_RoomEnter();
                    }
                }
                ChangeProcess(ProcessStep.Step3);
                break;

            case ProcessStep.Step3:
                canvas_manager.Initialize(this);
                NunchiWorldManager.Initialize();
                ChangeProcess(ProcessStep.Step4);
                break;

            case ProcessStep.Step4:
                smDisposer = new SingleAssignmentDisposable();

                smDisposer.Disposable = Observable.EveryUpdate()
                    .Select(_ => NunchiWorldManager.NunchiMainSM)
                    //.SelectMany(state=> CreateNUNCHI(state))
                    .Where(state =>
                    {
                        var ss = state.GetCurrentState();
                        var ff = MGNunchiGameState_SetServerInfo.Instance();
                        return ss == ff;
                    })
                    .Do(state =>
                    {
                        CDebug.Log($"             @@@@@@@@@@@@@@   check state = {state.GetCurrentState()}.");
                    })
                    .First()
                    .Subscribe(_ =>
                    {
                        SoundManager.Instance.PlayBGMSound(ESOUND_BGM.ID_BPWORLD_MINIGAME1);
                        ChangeProcess(ProcessStep.Complete);
                        smDisposer.Dispose();
                    }).AddTo(this);


                if (BPWorldManager.is_bpworld_standalone)
                {
                    var bcMatchingCompleteDisposer = new SingleAssignmentDisposable();
                    bcMatchingCompleteDisposer.Disposable = Observable.Timer(TimeSpan.FromSeconds(3f))
                    .Subscribe(_ =>
                    {
                        NunchiGameStageStartHandler_Execute(NunchiGameStageType.TRYOUT);
                        bcMatchingCompleteDisposer.Dispose();
                    });
                }
                break;

            case ProcessStep.Complete:
                break;
        }
    }

    public void DisposeSMDiposer()
    {
        smDisposer.Dispose();
        smDisposer = null;
    }

    public IObservable<CStateMachine<MGNunchiWorldManager>> CreateNUNCHI(CStateMachine<MGNunchiWorldManager> nunchiMgr)
    {

        return Observable.Create<CStateMachine<MGNunchiWorldManager>>(ob =>
        {
            if (nunchiMgr == null)
            {
                //널인 값 생성한다
                nunchiMgr = new CStateMachine<MGNunchiWorldManager>(NunchiWorldManager);

                ob.OnNext(nunchiMgr);
                ob.OnCompleted();
            }
            else
            {
                ob.OnNext(nunchiMgr);
                ob.OnCompleted();
            }

            return Disposable.Empty;
        });
    }

    #endregion SceneManager Life Cycle



    public BaseSceneCanvasManager GetCanvasManager()
    {
        return canvas_manager;
    }

    /// <summary>
    /// ProcessStep 이 ProcessStep.Complete 되었을때 호출
    /// </summary>
    /// <returns></returns>
    //public override IObservable<Unit> DidFinishProcessObservable()
    //{
    //    return Observable.Create<Unit>(observer =>
    //    {
    //        CDebug.Log($"{GetType()} DidFinishProcessObservable", CDebugTag.SCENE);

    //        SoundManager.Instance.PlayBGMSound(6810082);

    //        TCPConnectInfoManager.Instance.SetIsServerMove(true);
    //        TournamentInfo _info = MGNunchiServerDataManager.Instance.GetTournamentGameRoomInfo();
    //        TCPMGNunchiRequestManager.Instance.TCPConnect(_info.IP, _info.PORT);

    //        observer.OnNext(Unit.Default);
    //        observer.OnCompleted();

    //        return Disposable.Empty;
    //    });
    //}

    public void CreateLayer(ref GameObject target, CAssetService assetService, string name)
    {
        target = Utility.AddChild
            (
            canvas_manager.GetSafeAreaObject(),
            assetService.Get_Object_Info(AssetPathDefine.TRANSACTION_MINIGAME_NUNCHI_LAYERGROUP1_PREFAB)
            );
        target.name = name;
        target.SetActive(true);
        target.transform.SetAsFirstSibling();
    }


    public GameObject GetScoreUILayer()
    {
        return ScoreUILayer;
    }

    public void BackToBPWorld()
    {
        StaticAvatarManager.SetAvatarObjBack();
        NavigationManager.BackTo();
    }


    //private bool IsAppPause;
    //private float AppPauseStartTime;


    //void OnApplicationPause(bool pauseStatus)
    //{
    //    if (IsAppPause == false)
    //    {
    //        Debug.Log("      **** [Nunchi] OnApplicationPause");
    //        //AppPauseStartTime = Time.time;
    //        IsAppPause = true;
    //    }
    //}

    //void OnApplicationFocus(bool hasFocus)
    //{
    //    Debug.Log("      **** [Nunchi] OnApplicationFocus / IsAppPause = " + IsAppPause);
    //    if (IsAppPause)
    //    {
    //        //float pausedTime = Time.time - AppPauseStartTime;
    //        Debug.Log("      **** [Nunchi] OnApplicationFocus paused time = " + AppPauseStartTime /*+ pausedTime*/);

    //        //if(pausedTime >= 5)
    //        if(AppPauseStartTime >= 5)
    //        {
    //            Debug.Log("      **** [Nunchi] OnApplicationFocus   go enter to BPWorld");
    //        }

    //        IsAppPause = false;
    //    }

    //}


    //=======================================================================================================================
    //=======================================================================================================================
    //=======================================================================================================================
    //=======================================================================================================================


    #region - 블핑월드 스탠드얼론 브로드캐스팅 우회 로직 

    public void NunchiGameStageStartHandler_Execute(NunchiGameStageType type)
    {
        CDebug.Log($"NunchiGameStageStartHandler_Execute(NunchiGameStageType type) type - {type}", CDebugTag.MINIGAME_NUNCHI);

        ByteBuffer byteBuffer = new ByteBuffer(100);
        BPWPacket.NunchiGameStageStartBc _recvPkt = new BPWPacket.NunchiGameStageStartBc();
        _recvPkt.__init(0, byteBuffer);
        _recvPkt.StageType = type;
        _recvPkt.PlayerSeatInfosLength = 4;
        _recvPkt.MaxRound = 5;

        MGNunchiServerDataManager.Instance.StageType = _recvPkt.StageType;
        MGNunchiServerDataManager.Instance.SetTournamentGameRoomInfo(_recvPkt.StageType, _recvPkt.MaxRound);
        MGNunchiServerDataManager.Instance.SetStateLatencyTime(Time.time);

        float delayTime = 0;

        if (_recvPkt.StageType == NunchiGameStageType.FINAL)
        {
            CDebug.Log("Bc_NunchiGameStageStartHandler _recvPkt.StageType == NunchiGameStageType.FINAL", CDebugTag.MINIGAME_STAGE_START_HANDLER);

            if (dic_player_round_result_infos != null)
            {
                dic_player_round_result_infos.Clear();
            }

            delayTime = 0.5f;
            WorldManager.SetLoadingForFinal();

            SingleAssignmentDisposable disposer = new SingleAssignmentDisposable();
            disposer.Disposable = Observable.Timer(TimeSpan.FromSeconds(0.1f))
            .Subscribe(_ =>
            {
                if (WorldManager.ResultPopup != null)
                {
                    WorldManager.ResultPopup.ClosePopup();
                }
                WorldManager.SetChangeState(MGNunchiGameState_PrepareFinal.Instance());
                disposer.Dispose();
            });
        }

        SingleAssignmentDisposable prepareDisposer = new SingleAssignmentDisposable();
        prepareDisposer.Disposable = Observable.Timer(TimeSpan.FromSeconds(delayTime))
        .Subscribe(_ =>
        {
            CDebug.Log("Bc_NunchiGameStageStartHandler SetPlayersInfo()", CDebugTag.MINIGAME_STAGE_START_HANDLER);
            SetPlayersInfo(_recvPkt);
            prepareDisposer.Dispose();
        });
    }




    private void SetPlayersInfo(BPWPacket.NunchiGameStageStartBc _recvPkt)
    {
        CDebug.Log("SetPlayersInfo()", CDebugTag.NUNCH_GAME_MANAGER);

        if (WorldManager.PlayerMgr != null)
        {
            WorldManager.PlayerMgr.ClearSortedPlayerUI();
        }

        bool isMatchingUser = false;

        for (int i = 0; i < _recvPkt.PlayerSeatInfosLength; ++i)
        {
            MGNunchiPlayerInfo _recvInfo = new MGNunchiPlayerInfo();

            //BPWPacketDefine.NunchiGamePlayerSeatInfo pktInfo = _recvPkt.PlayerSeatInfos(i).Value;
            //_recvInfo.PID = pktInfo.PlayerId;
            //_recvInfo.UID = (long)pktInfo.Uid;
            //_recvInfo.IsBot = pktInfo.Bot;
            //_recvInfo.SeatID = pktInfo.SeatId;


            isMatchingUser = i == _recvPkt.PlayerSeatInfosLength - 1 ? true : false;

            if (!isMatchingUser)
            {
                // bot
                _recvInfo.PID = i + 5;      // 5는 임의... 원본 테스트와 동일한 값으로 보기 위함
                _recvInfo.IsBot = true;
                _recvInfo.UID = 2300001 + i;
                _recvInfo.SeatID = i;
            }
            else
            {
                // user
                _recvInfo.PID = i + 5;
                _recvInfo.IsBot = false;
                _recvInfo.UID = long.Parse(CPlayer.GetStatusLoginValue(PLAYER_STATUS.USER_ID));
                _recvInfo.SeatID = i;
            }

            if (_recvInfo.IsBot)
            {
                BPWorldMiniGameBotData _botData = BPWorldDataManager.Instance.GetBotData(_recvInfo.UID);
                if (_botData == null)
                {
                    UnityEngine.Debug.LogError($"S2C_BroadCastHandler_NunchiGameTournamentStageStart. bot data is null. ID = {_recvInfo.UID}");
                    return;
                }

                _recvInfo.NickName = _botData.Name;


                #region - Just For Test
                //#if UNITY_EDITOR
                //                //Just For Test
                //                _recvInfo.CharacterID = npcIDs[i];
                //                if (_recvInfo.CharacterID > 4)
                //                {
                //                    _recvInfo.CharacterType = CHARACTER_TYPE.NPC;
                //                }
                //                else
                //#endif
                #endregion
                {
                    _recvInfo.CharacterType = CHARACTER_TYPE.AVATAR;
                    _recvInfo.AvatarType = _botData.AvatarType;
                    _recvInfo.StylingItemInfo = NetAvatarCommonManager.Instance.GetStylingInfoByBotTable(_botData);
                }
            }
            else
            {
                // 2024.10.21 check naru :: 매칭 정보 나중 정리.. (  pktInfo.CharacterId 이것도 패킷 필요.. )
                _recvInfo.NickName = CPlayer.GetStatusLoginValue(PLAYER_STATUS.NICK);

                if(BPWorldManager.is_costumeplay_character_iD != 0)
                    _recvInfo.CharacterID = BPWorldManager.is_costumeplay_character_iD;
                else
                    _recvInfo.CharacterID = BPWorldServerDataManager.Instance.GetMyUserInfo().CharacterID; 

                CDebug.Log($"BPWorldServerDataManager.Instance.GetMyUserInfo().CharacterID - {BPWorldServerDataManager.Instance.GetMyUserInfo().CharacterID}", CDebugTag.NUNCH_GAME_MANAGER);

                if (_recvInfo.CharacterID <= 4)
                {
                    _recvInfo.CharacterType = CHARACTER_TYPE.AVATAR;
                    _recvInfo.AvatarType = (AVATAR_TYPE)_recvInfo.CharacterID;    // MemberType

                    // list

                    //if (pktInfo.AvatarInfosLength > 0)
                    //{
                    //    AvatarPartType partType = default;
                    //    ulong tid = 0;
                    //    _recvInfo.StylingItemInfo = new StylingInfo();

                    //    for (int index = 0; index < pktInfo.AvatarInfosLength; ++index)
                    //    {
                    //        partType = pktInfo.AvatarInfos(index).Value.PartType;
                    //        tid = pktInfo.AvatarInfos(index).Value.Tid;

                    //        NetAvatarCommonManager.Instance.UpdateStylingParts(ref _recvInfo.StylingItemInfo, partType, tid);
                    //    }
                    //}
                }
                else
                {
                    _recvInfo.CharacterType = CHARACTER_TYPE.NPC;
                }
            }
            CDebug.Log($"##########  TournamentStageStart uid:{_recvInfo.UID}, pid:{_recvInfo.PID}, name:{_recvInfo.NickName}, SeatID:{_recvInfo.SeatID}, isBot:{_recvInfo.IsBot}", CDebugTag.MINIGAME_STAGE_START_HANDLER);


            // 2024.10.21 naru 블핑월드 눈치게임 라운드 정보를.. 로컬에서 계산하기 위해서 유저 기본 정보를 저장
            if (BPWorldManager.is_bpworld_standalone)
            {
                if (!dic_player_round_result_infos.ContainsKey(i))   // 0부터 카운팅 되게..
                {
                    RoundResultInfo info = new RoundResultInfo();

                    info.UID = _recvInfo.UID;
                    info.PID = _recvInfo.PID;
                    info.PlayerSvrSeatID = _recvInfo.SeatID;
                    info.TargetSvrMapID = 0;
                    info.bGainSuccess = false;
                    info.Quantity_GainCoin = 0;
                    info.Quantity_TotalCoin = 0;
                    info.GainItemType = NunchiGameItemType.NONE;

                    dic_player_round_result_infos.Add(i, info);
                }
            }

            MGNunchiServerDataManager.Instance.SetGamePlayerInfoDic(_recvInfo.PID, _recvInfo);
        }

        WorldManager.SetMaxRound(_recvPkt.MaxRound);
        WorldManager.SetChangeState(MGNunchiGameState_SetServerInfo.Instance());
    }


    // 시트 번호 및 각 시트의 아이템 혹은 코인 갯수 등의 셋팅
    public bool NunchiGameRoundInfoHandler_Execute()
    {
        CDebug.Log("==================== Bc_NunchiGameRoundInfoHandler_Execute() ====================", CDebugTag.NUNCH_GAME_ROUND_READY_HANDLER);


        ///세션만료, 점검 공지, 포커스 타임오버 등의 팝업이 떠 있다면 Packet을 받아서 처리 하지 않는다.
        if (BPWorldExitEventPopupManager.Instance.IsActiveExitEventPopup == true)
        {
            return true;
        }

        SceneID CurSceneID = (SceneID)CDirector.Instance.GetCurrentSceneID();

        if (CurSceneID != SceneID.MINIGAME_NUNCHI_SCENE)
            return true;

        MGNunchiServerDataManager.Instance.SetStateLatencyTime(Time.time);

        //================================================================================

        #region - 눈치 게임 라운드 데이터 정보 셋팅

        round_count++;

        CDebug.Log($"NunchiGameRoundInfoHandler_Execute() round_count - {round_count}", CDebugTag.NUNCH_GAME_ROUND_READY_HANDLER);

        int round_time = 3;
        MGNunchiServerDataManager.Instance.SetCoinItemInfo();
        WorldManager.SetRoundCount(round_count);
        Instance.WorldManager.SetRoundTime(round_time);

        TournamentInfo _gameInfo = MGNunchiServerDataManager.Instance.GetTournamentGameRoomInfo();
        if (_gameInfo.StageType == BPWPacketDefine.NunchiGameStageType.FINAL)
        {
            if (WorldManager.ResultPopup != null)
            {
                WorldManager.ResultPopup.ClosePopup();
                WorldManager.ResultPopup = null;
            }
        }

        WorldManager.SetChangeState(MGNunchiGameState_Ready.Instance());

        SingleAssignmentDisposable stageDisposer = new SingleAssignmentDisposable();
        stageDisposer.Disposable = Observable.Timer(TimeSpan.FromSeconds(4f))
        .Subscribe(_ =>
        {
            if(!is_nunchi_game_leave)
                NunchiGameRoundStartHandler_Execute();
            stageDisposer.Dispose();
        });
        #endregion

        return true;
    }



    public bool NunchiGameRoundStartHandler_Execute()
    {
        CDebug.Log("         @@@@@@@@@@@@ NunchiGameRoundStartHandler_Execute", CDebugTag.NUNCH_GAME_ROUND_START_HANDLER);

        nunchiGameRoundItemSeatChoiceRes = -1;

        ///세션만료, 점검 공지, 포커스 타임오버 등의 팝업이 떠 있다면 Packet을 받아서 처리 하지 않는다.
        if (BPWorldExitEventPopupManager.Instance.IsActiveExitEventPopup == true)
        {
            return true;
        }

        
        MGNunchiWorldManager worldMgr = MGNunchiManager.Instance.WorldManager;
        //block tutorial

        check_round_end_execute = false;


        if (MGNunchiServerDataManager.Instance.IsSinglePlay)
        {
            TCPMGNunchiRequestManager.Instance.Req_GamePlayPause();

            MGNunchiServerDataManager.Instance.SetStateLatencyTime(0, true);

            //go to Step 1
            float animLength = 1.5f;
            var stateChangeDisposer = new SingleAssignmentDisposable();
            stateChangeDisposer.Disposable = Observable.Timer(TimeSpan.FromSeconds(animLength))
            .Subscribe(_ =>
            {
                worldMgr.SetChangeState(MGNunchiGameState_Tutorial.Instance());
                stateChangeDisposer.Dispose();
            })
            .AddTo(worldMgr);
        }
        else
        {
            if(!is_nunchi_game_leave)
            {
                MGNunchiServerDataManager.Instance.SetStateLatencyTime(Time.time);
                worldMgr.SetChangeState(MGNunchiGameState_InPlayingGame.Instance());
            }
        }


        if(!TCPMGNunchiRequestManager.Instance.is_GamePlayPause  && !is_nunchi_game_leave)
        {
            roundEndDisposer = new SingleAssignmentDisposable();
            roundEndDisposer.Disposable = Observable.Timer(TimeSpan.FromSeconds(4f))
            .Subscribe(_ =>
            {
                CDebug.Log("Observable.Timer(TimeSpan.FromSeconds(4f)) Bc_NunchiGameRoundEnd_Execute();", CDebugTag.MINIGAME_NUNCHI);
                if (!check_round_end_execute && !is_nunchi_game_leave)
                {
                    check_round_end_execute = true;
                    Bc_NunchiGameRoundEnd_Execute();
                }

                roundEndDisposer.Dispose();
            });
        }

        return true;
    }





    // 이동 타겟... 성공유무, 등등의 정보 셋팅하여 내려옴......인데.... 
    // 따로 만들어야겠지..??????
    private int GetRandomValueByUserIndex(int userIndex)
    {
        System.Random random = new System.Random();

        if (!targetSvrMapID.ContainsKey(userIndex))
        {
            throw new ArgumentException("유효하지 않은 유저 인덱스입니다.");
        }

        List<int> values = targetSvrMapID[userIndex];   // 해당 유저의 값 리스트 가져오기
        int index = random.Next(values.Count);          // 랜덤 인덱스 선택
        return values[index];                           // 랜덤 값 반환
    }



    private void SetLocalRoundResultInfo()
    {
        // 움직일 타겟 위치의 id셋팅
        for (int i = 0; i < dic_player_round_result_infos.Count; i++)
        {
            if ((i == (dic_player_round_result_infos.Count - 1)) && (nunchiGameRoundItemSeatChoiceRes != -1))
            {
                CDebug.Log($"SetLocalRoundResultInfo() nunchiGameRoundItemSeatChoiceRes != -1 - {nunchiGameRoundItemSeatChoiceRes}", CDebugTag.MINIGAME_NUNCHI);
                dic_player_round_result_infos[i].TargetSvrMapID = nunchiGameRoundItemSeatChoiceRes;
            }
            else
            {
                dic_player_round_result_infos[i].TargetSvrMapID = GetRandomValueByUserIndex(i);
            }
        }

        if(MGNunchiServerDataManager.Instance.IsSinglePlay)
        {
            dic_player_round_result_infos[0].TargetSvrMapID = 0;
            dic_player_round_result_infos[1].TargetSvrMapID = 0;
            dic_player_round_result_infos[2].TargetSvrMapID = 3;
        }



        MGNunchiCoinItemInfo[] _coinItemInfo = MGNunchiServerDataManager.Instance.GetCoinItemInfos();

        for (int i = 0; i < dic_player_round_result_infos.Count; i++)
        {
            bool bCheck = CheckForDuplicateTargetSvrMapID(i);

            if (bCheck)
            {
                dic_player_round_result_infos[i].bGainSuccess = false;
                dic_player_round_result_infos[i].Quantity_GainCoin = 0;
                dic_player_round_result_infos[i].GainItemType = _coinItemInfo[dic_player_round_result_infos[i].TargetSvrMapID].ItemType;
            }
            else
            {
                dic_player_round_result_infos[i].bGainSuccess = true;
                dic_player_round_result_infos[i].Quantity_GainCoin = _coinItemInfo[dic_player_round_result_infos[i].TargetSvrMapID].Quantity;
                dic_player_round_result_infos[i].Quantity_TotalCoin += dic_player_round_result_infos[i].Quantity_GainCoin;
                dic_player_round_result_infos[i].GainItemType = _coinItemInfo[dic_player_round_result_infos[i].TargetSvrMapID].ItemType;
            }
        }
    }

    private bool CheckForDuplicateTargetSvrMapID(int userIndex)
    {
        int targetID = dic_player_round_result_infos[userIndex].TargetSvrMapID;

        foreach (var item in dic_player_round_result_infos)
        {
            // 본인(userIndex) 이외의 유저들에 대해 체크
            if (item.Key != userIndex && item.Value.TargetSvrMapID == targetID)
            {
                return true; // 중복된 값이 있는 경우 true 반환
            }
        }

        return false; // 중복된 값이 없으면 false 반환
    }



    //public void ChangeSeatChoiceIndex()
    //{
    //    CDebug.Log("ChangeSeatChoiceIndex()", CDebugTag.MINIGAME_NUNCHI);

    //    if(!check_round_end_execute)
    //    {
    //        check_round_end_execute = true;

    //        roundEndDisposer.Dispose();

    //        Bc_NunchiGameRoundEnd_Execute();
    //    }
    //}



    public bool Bc_NunchiGameRoundEnd_Execute()
    {
        CDebug.Log("Bc_NunchiGameRoundEnd_Execute() ===========================================", CDebugTag.NUNCH_GAME_ROUND_END);

        ///세션만료, 점검 공지, 포커스 타임오버 등의 팝업이 떠 있다면 Packet을 받아서 처리 하지 않는다.
        if (BPWorldExitEventPopupManager.Instance.IsActiveExitEventPopup == true)
        {
            return true;
        }

        MGNunchiWorldManager worldMgr = MGNunchiManager.Instance.WorldManager;

        MGNunchiServerDataManager.Instance.SetStateLatencyTime(Time.time);

        // 로컬에서 라운드 결과 처리... 
        SetLocalRoundResultInfo();


        for (int i = 0; i < dic_player_round_result_infos.Count; ++i)
        {
            RoundResultInfo _recvResultInfo = new RoundResultInfo();
            _recvResultInfo.UID = dic_player_round_result_infos[i].UID;
            _recvResultInfo.PID = dic_player_round_result_infos[i].PID;
            _recvResultInfo.PlayerSvrSeatID = dic_player_round_result_infos[i].PlayerSvrSeatID;
            _recvResultInfo.TargetSvrMapID = dic_player_round_result_infos[i].TargetSvrMapID;
            _recvResultInfo.bGainSuccess = dic_player_round_result_infos[i].bGainSuccess;
            _recvResultInfo.Quantity_GainCoin = dic_player_round_result_infos[i].Quantity_GainCoin;
            _recvResultInfo.Quantity_TotalCoin = dic_player_round_result_infos[i].Quantity_TotalCoin;

            CDebug.Log($"Bc_NunchiGameRoundEnd :: Execute() _recvResultInfo.UID - {_recvResultInfo.UID}, _recvResultInfo.PID - {_recvResultInfo.PID}, " +
                $"_recvResultInfo.PlayerSvrSeatID - {_recvResultInfo.PlayerSvrSeatID}, _recvResultInfo.TargetSvrMapID - {_recvResultInfo.TargetSvrMapID}, " +
                $"_recvResultInfo.bGainSuccess - {_recvResultInfo.bGainSuccess}, _recvResultInfo.Quantity_GainCoin - {_recvResultInfo.Quantity_GainCoin}, _recvResultInfo.Quantity_TotalCoin - {_recvResultInfo.Quantity_TotalCoin}", CDebugTag.MINIGAME_NUNCHI);

            if (dic_player_round_result_infos[i].GainItemType != BPWPacketDefine.NunchiGameItemType.NONE)
            {
                CDebug.Log($"NunchiRoundEnd // uid = {_recvResultInfo.UID}, ROUND = {worldMgr.GetRoundCount()}, effective Item = {dic_player_round_result_infos[i].GainItemType}");
                CDebug.Log("==============================================================================");
                MGNunchiServerDataManager.Instance.SetRoundEffectiveItemInfoDic(_recvResultInfo.PID, worldMgr.GetRoundCount(), dic_player_round_result_infos[i].GainItemType);
            }


            _recvResultInfo.GainItemType = dic_player_round_result_infos[i].GainItemType;
            if (dic_player_round_result_infos[i].GainItemType == BPWPacketDefine.NunchiGameItemType.NONE && dic_player_round_result_infos[i].GainItemType != 0)
            {
                _recvResultInfo.GainItemType = BPWPacketDefine.NunchiGameItemType.COIN;
            }

            MGNunchiServerDataManager.Instance.SetRoundResultInfoDic(_recvResultInfo.PID, _recvResultInfo);

        }


        //Jump Players!!!!
        worldMgr.PlayerMgr.ChangeAllPlayersState(MGNunchiPlayerState_GoGetCoinPrepare.Instance());



        // check naru :: 여기서 마지막 라운드 기능 셋팅까지 되었다면..랭킹관련.. 
        //               NunchiGameStageEnd :: Execute( PhSession session, Packet packet ) <-- 브로드 캐스팅 형태.. (BC 없어서 .. )
        //               강제 호출......
        if (WorldManager.GetRoundCount() == 5)
        {
            CDebug.Log("WorldManager.GetRoundCount() == 5", CDebugTag.MINIGAME_NUNCHI);
            TournamentInfo _info = MGNunchiServerDataManager.Instance.GetTournamentGameRoomInfo();

            if (_info.StageType == BPWPacketDefine.NunchiGameStageType.FINAL)
            {
                NunchiGameStageFinalEnd_Execute();
            }
            else
            {
                NunchiGameStageEnd_Execute();
            }
        }

        return true;
    }




    private List<KeyValuePair<int, RoundResultInfo>> GetTopTwoPlayersByTotalCoin()
    {
        // dic_player_round_result_infos를 Quantity_TotalCoin을 기준으로 정렬 후 상위 2개의 항목 추출
        var topTwoPlayers = dic_player_round_result_infos
            .OrderByDescending(x => x.Value.Quantity_TotalCoin) // Quantity_TotalCoin을 기준으로 내림차순 정렬
            .Take(2)                                            // 상위 2개의 항목 가져오기
            .ToList();

        return topTwoPlayers;
    }


    private int GetLastPlayerRankingByTotalCoin()
    {
        // dic_player_round_result_infos의 마지막 유저의 Quantity_TotalCoin 값
        int lastPlayerIndex = dic_player_round_result_infos.Count - 1;
        int lastPlayerTotalCoin = dic_player_round_result_infos[lastPlayerIndex].Quantity_TotalCoin;

        // Quantity_TotalCoin을 기준으로 내림차순으로 정렬한 리스트에서 인덱스 찾기
        var sortedList = dic_player_round_result_infos
            .OrderByDescending(x => x.Value.Quantity_TotalCoin)
            .ToList();

        // 정렬된 리스트에서 마지막 유저의 TotalCoin 값이 몇 번째에 위치하는지 찾기
        int ranking = sortedList.FindIndex(x => x.Value.Quantity_TotalCoin == lastPlayerTotalCoin);


        int _rank = ranking + 1;
        CDebug.Log($"GetLastPlayerRankingByTotalCoin() ranking - {_rank}", CDebugTag.NUNCH_GAME_STAGE_END);

        return ranking + 1; // 0-based 인덱스이므로 1을 더해 순위를 반환
    }


    private int GetPlayerTotalCoin()
    {
        int lastPlayerIndex = dic_player_round_result_infos.Count - 1;
        int lastPlayerTotalCoin = dic_player_round_result_infos[lastPlayerIndex].Quantity_TotalCoin;

        return lastPlayerTotalCoin;
    }


    private void NunchiGameStageEnd_Execute()
    {
        CDebug.Log("NunchiGameStageEndHandler_Execute()", CDebugTag.MINIGAME_NUNCHI);

        // 랭킹 로컬 계산... 
        MGNunchiServerDataManager.Instance.SetMyStageRank(GetLastPlayerRankingByTotalCoin());

        #region - 리워드 보상 셋팅  :: 승현님에게 서버 통신해서 받아와야 할 듯......????????????????????
        //for (int i = 0; i < _recvPkt.RewardInfosLength; ++i)
        //{
        //    MGNunchiServerDataManager.Instance.SetStageRewardInfosFromRecvPacket(_recvPkt.RewardInfos(i).Value);
        //}

        int rank = MGNunchiServerDataManager.Instance.GetMyStageRank();
        int winner = rank < 3 ? 1 : 0;
        int last = 0;           // 0 : 예선, 1 : 결승
        int mid = (int)BPWorldServerDataManager.Instance.GetMyUserInfo().CharacterID;             
        int map_id = 201002;    // 눈치게임 worldmaptable.ID

        SingleAssignmentDisposable disposable = new SingleAssignmentDisposable();
        disposable.Disposable = APIHelper.BPWorldSvc.BPWorldMinigameReward(mid, map_id, 0, GetPlayerTotalCoin(), 0, 0, 0, last, winner, rank).Subscribe(responseData =>
        {
            RewardInfo rewardInfo = responseData.Common.Reward;

            MGNunchiServerDataManager.Instance.SetStageRewardInfosFromRecvPacket(rewardInfo.items[0].changeCnt, responseData.Value.daily_cnt.metaverse_minigame_reward);

            disposable.Dispose();
        });


        #endregion

        // 동점자가 있는지...... 이거 일단 false로 해놓고.. 처리..
        //MGNunchiServerDataManager.Instance.SetIsThereTiePlayers(_recvPkt.Tie);
        MGNunchiServerDataManager.Instance.SetIsThereTiePlayers(false);     // 이 값은 체크 필요.. false , true가 뭘 뜻하는지..

        // 결승전 진입 유저에 대한 정보 
        var topTwoPlayers = GetTopTwoPlayersByTotalCoin();

        int _ranking = 1;
        foreach (var player in topTwoPlayers)
        {
            Console.WriteLine($"{player.Key}의 총 코인: {player.Value.Quantity_TotalCoin}");

            FinalPlayerInfo _info = new FinalPlayerInfo();
            _info.Rank = _ranking;
            _info.UID = player.Value.UID;
            _info.PID = player.Value.PID;
            MGNunchiServerDataManager.Instance.SetFinallistInfo(_info);
            _ranking++;
        }
    }


    private void NunchiGameStageFinalEnd_Execute()
    {
        CDebug.Log("NunchiGameStageFinalEnd :: Execute( PhSession session, Packet packet )", CDebugTag.NUNCH_GAME_STAGE_END);


        MGNunchiServerDataManager.Instance.SetMyStageRank(GetLastPlayerRankingByTotalCoin());

        // 여기도 승현님한테 보상처리 어떻게 할지... 문의
        /*
        for (int i = 0; i < _recvPkt.RewardInfosLength; ++i)
        {
            MGNunchiServerDataManager.Instance.SetStageRewardInfosFromRecvPacket(_recvPkt.RewardInfos(i).Value);
        }
        */

        int rank = MGNunchiServerDataManager.Instance.GetMyStageRank();
        int winner = rank < 3 ? 1 : 0;
        int last = 1;  // 0 : 예선, 1 : 결승
        int mid = (int)BPWorldServerDataManager.Instance.GetMyUserInfo().CharacterID;
        int map_id = 201002; // 눈치게임 worldmaptable.ID

        SingleAssignmentDisposable disposable = new SingleAssignmentDisposable();
        disposable.Disposable = APIHelper.BPWorldSvc.BPWorldMinigameReward(mid, map_id, 0, GetPlayerTotalCoin(), 0, 0, 0, last, winner, rank).Subscribe(responseData =>
        {
            RewardInfo rewardInfo = responseData.Common.Reward;

            MGNunchiServerDataManager.Instance.SetStageRewardInfosFromRecvPacket(rewardInfo.items[0].changeCnt, responseData.Value.daily_cnt.metaverse_minigame_reward);

            disposable.Dispose();
        });
    }



    #endregion



    //=======================================================================================================================

    public override void Release()
    {
        CDebug.Log($"{GetType()} Release ");

        NavigationManager.GetPageManager(SceneID.MINIGAME_NUNCHI_SCENE).Release();
        canvas_manager.Release();
        canvas_manager = null;
        if (NunchiWorldManager != null)
        {
            NunchiWorldManager.Release();
            NunchiWorldManager = null;
        }

        if (BPWorldManager.is_bpworld_standalone)
        {
            if (dic_player_round_result_infos != null)
            {
                dic_player_round_result_infos.Clear();
                dic_player_round_result_infos = null;
            }


            if (targetSvrMapID != null)
            {
                targetSvrMapID.Clear();
                targetSvrMapID = null;
            }
        }

        DisposeSMDiposer();

        DestroyImmediate(WorldMgrObj);
    }
}
