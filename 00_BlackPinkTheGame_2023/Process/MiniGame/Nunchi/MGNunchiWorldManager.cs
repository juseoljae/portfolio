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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using DG.Tweening;

public class MGNunchiWorldManager : MonoBehaviour
{
    private MGNunchiCanvasManager CanvasMgr;

    public MGNunchiPlayersManager PlayerMgr;
    private GameObject PlayerRootObj;

    public MGNunchiMapManager MapMgr;
    private GameObject MapRootObj;

    public MGNunchiCoinManager CoinMgr;

    private PopupAlert popupAlert;
    private CPopupService popupService;

    private MGNunchiTutorial NunchiTutorial;
    private Camera MainCam;

    //Server Seat Index Order
    // [0]   [1]
    //    [4]
    // [2]   [3]
    public int[] CoinItemSeatMapIdx = new int[] { 0, 2, 6, 8, 4 };

    //    [0]
    // [1]   [2]
    //    [3]
    public Vector3[] ResultCamTargetPosition = new Vector3[]
    {
        new Vector3(0.86f, 0.72f, -0.8f),
        new Vector3(-1.14f, 0.72f, -2.8f),
        new Vector3(2.86f, 0.72f, -2.8f),
        new Vector3(0.86f, 0.72f, -4.8f),
    };

	public Vector3[] PlayerHelloCamPosition = new Vector3[]
    {
        new Vector3(0, 0.85f, -1.6f),
        new Vector3(-2, 0.85f, -3.6f),
        new Vector3(2, 0.85f, -3.6f),
        new Vector3(0, 0.85f, -5.6f),
	};

	public Vector3[] PlayerHelloCamRotation = new Vector3[]		
	{
        new Vector3(0, 0, 0),
        new Vector3(0, 0, 0),
        new Vector3(0, 0, 0),
        new Vector3(0, 0, 0),
	};

    public float Default_FOV = 30;
    public float Direct_FOV = 43;
    public float CurFOV;

    public Vector3 CamZoomInPos;
    public Vector3 CamDefaultPos;
    public Vector3 CamDefaultRotation;
    public Transform MainCamera;
    public Camera MainCamComponent;
    private float CurHelloCamRot;

    public CStateMachine<MGNunchiWorldManager> NunchiMainSM;
    private bool CheckerTimeToCleanCoinUp;
    private bool IsTimeToCleanCoinItemUp;
    private SingleAssignmentDisposable CleanCoinUpDispose;

    public bool IsStarted;
	public bool IsPlayHello;
    
    private int RoundTime;
    private int MAX_ROUND;
    private int RoundCount;
    public int PLAYER_COUNT;

    //server data temporary
    private long GroupID;

    public PopupMGNunchiResult ResultPopup;

    //3D nameTag    
    private Dictionary<NAMETAG_TYPE, List<UI_BPWorldNameTag>> NameTagsDic = new Dictionary<NAMETAG_TYPE, List<UI_BPWorldNameTag>>();
    private BPWorldIconAndTagCreator IATCreator;
    private GameObject poolParentObject;
    public GameObject nameTagPoolParentObject;        

    private const string NAME_POOL_PARENT_OBJECT = "POOL_MONOOBJECTS(Dynamic Create)";
    private const string NAME_POOL_NAME_TAG = "NAME TAG POOL(Dynamic Create)";

    public void Initialize()
    {
        InitMainCamera();
        PLAYER_COUNT = MGNunchiDefines.PLAYERS_4P;
        CanvasMgr = (MGNunchiCanvasManager)MGNunchiManager.Instance.GetCanvasManager();
        CanvasMgr.PageUI.InitMgNunchiPageUI(this);

        popupService = CCoreServices.GetCoreService<CPopupService>();

        NunchiTutorial = new MGNunchiTutorial();
        SetTutorialStep(TutorialStep.NONE);

        NunchiMainSM = new CStateMachine<MGNunchiWorldManager>(this);

        //coin
        CoinMgr = transform.Find("World/CoinItems").gameObject.AddComponent<MGNunchiCoinManager>();
        CoinMgr.Initialized();

        //Map
        GameObject mapRootObj = transform.Find("World/PlayGround").gameObject;
        MapMgr = new MGNunchiMapManager();
        MapMgr.Initialize(mapRootObj);

        //Players
        PlayerRootObj = transform.Find("World/Players").gameObject;
        PlayerMgr = PlayerRootObj.AddComponent<MGNunchiPlayersManager>();
        PlayerMgr.Initialize();

        ResultPopup = null;

        IATCreator = new BPWorldIconAndTagCreator();

        //RoundCount = 1;
        SetCheckerTimeToCleanCoinUp(false);
        SetIsTimeToCleanCoinItemUp(false);


        CreatePoolParentObject(transform);

        SetupNameTag();
	}


    private void CreatePoolParentObject(Transform pageUITransform)
    {
        poolParentObject = new GameObject(NAME_POOL_PARENT_OBJECT);
        poolParentObject.transform.SetParent(pageUITransform);
        poolParentObject.transform.localScale = Vector3.one;
        poolParentObject.transform.SetAsLastSibling();

        nameTagPoolParentObject = new GameObject(NAME_POOL_NAME_TAG);
        nameTagPoolParentObject.transform.SetParent(poolParentObject.transform);
        nameTagPoolParentObject.transform.localScale = Vector3.one;
    }

    public void SetupNameTag()
    {
        MGNunchiCanvasManager canvasMgr = (MGNunchiCanvasManager)MGNunchiManager.Instance.GetCanvasManager();
        var canvasRect = canvasMgr.GetComponent<RectTransform>();
        BPWorldLayerObject layerObj = canvasMgr.LayerObjects;
        IATCreator.Intialize(gameObject, canvasRect, PlayerMgr.NameTagPoolMgr, MainCamera );
        IATCreator.SetUpLayerObject(layerObj);
    }

    public void AddPlayerNameTag(long uid, int pid, string name, GameObject charObj)
    {
		Tuple<bool, int> beforeRankInfo = BPWorldRankingManager.Instance.IsInRank( uid );
		GameObject dummyObj = PlayerMgr.GetPlayerDummyBone(pid);
        UI_BPWorldNameTag nameTag = null;
		//bool addTop = false;
        

        if (charObj == null || dummyObj == null) return;

        if (CPlayer.GetStatusLoginValue(PLAYER_STATUS.USER_ID) == uid.ToString())
        {
			nameTag = IATCreator.NameTagCreator.AddNameTagByMyPlayer( name, beforeRankInfo.Item2, dummyObj, MainCamera );
            //addTop = true;
        }

		else
		{
			bool isMyFriend = APIHelper.Friend.friendData.ContainsKey( uid );
			nameTag = IATCreator.NameTagCreator.AddNameTagByOtherPlayer( name, isMyFriend, beforeRankInfo.Item2, dummyObj, MainCamera );
		}

        
        if (NameTagsDic.ContainsKey(NAMETAG_TYPE.PLAYER) == false)
        {
            NameTagsDic.Add(NAMETAG_TYPE.PLAYER, new List<UI_BPWorldNameTag>());
            NameTagsDic[NAMETAG_TYPE.PLAYER].Add(nameTag);
        }
        else
        {
            NameTagsDic[NAMETAG_TYPE.PLAYER].Add(nameTag);
        }
    }

    public void AddItemNameTag(string name, GameObject itemObj)
    {
        UI_BPWorldNameTag itemNameTag = IATCreator.NameTagCreator.AddNameTagByItem(name, itemObj );
        itemNameTag.transform.localPosition = new Vector3(0, 1.15f, 0);

        if (NameTagsDic.ContainsKey(NAMETAG_TYPE.ITEM) == false)
        {
            NameTagsDic.Add(NAMETAG_TYPE.ITEM, new List<UI_BPWorldNameTag>());
            NameTagsDic[NAMETAG_TYPE.ITEM].Add(itemNameTag);
        }
        else
        {
            NameTagsDic[NAMETAG_TYPE.ITEM].Add(itemNameTag);
        }
    }

    public void AddEmoticon(int pid, long emoticonID)
    {
        GameObject uiDummuBone = PlayerMgr.GetPlayerDummyBone( pid );
        BPWorldEmoticon emoticon = IATCreator.AddEmoticon( emoticonID, uiDummuBone.transform, MainCamera );

		emoticon.ShowEmoticon();
	}

	public void SetActivePlayerNameTag(bool bActive)
    {
        foreach (UI_BPWorldNameTag obj in NameTagsDic[NAMETAG_TYPE.PLAYER])
        {
            //CDebug.Log("                SetActiveNameTag " + obj.name + "/Active = " + bActive);
            obj.gameObject.SetActive(bActive);
        }
    }

    public void SetActiveItemNameTag(bool bActive)
    {
        if (NameTagsDic.ContainsKey(NAMETAG_TYPE.ITEM))
        {
            foreach (UI_BPWorldNameTag obj in NameTagsDic[NAMETAG_TYPE.ITEM])
            {
                obj.gameObject.SetActive(bActive);
            }
        }
    }

    public void InitMainCamera()
    {
        MainCamera = transform.Find("MainViewCamera");
        MainCamComponent = MainCamera.GetComponent<Camera>();
        MainCamera.localPosition = CamZoomInPos;
        MainCamera.localRotation = Quaternion.Euler(CamDefaultRotation);
    }

    public void SetCameraFOV(float targetValue, bool useTween = false)
    {
        if(MainCamComponent != null)
        {
            if (useTween)
            {
                //CurFOV = MainCamComponent.fieldOfView;
                DOTween.To(() => MainCamComponent.fieldOfView, x => MainCamComponent.fieldOfView = x, targetValue, 1.0f);
            }
            else
            {
                MainCamComponent.fieldOfView = targetValue;
            }
        }
    }

    public Camera GetMainCamera()
    {
        return MainCamComponent;
    }

	public void SetHelloCamPos()
	{
        MGNunchiPlayer myPlayer = PlayerMgr.GetMyPlayer();
        int myPlayerSeatId = myPlayer.GetMySeatIdx();//
        
        SetCameraFOV(Direct_FOV);

        MainCamera.localPosition = PlayerHelloCamPosition[myPlayerSeatId];
        MainCamera.localRotation = Quaternion.Euler(PlayerHelloCamRotation[myPlayerSeatId]);
	}

    public Vector3 GetCurCamPos()
    {
        return MainCamera.localPosition;
    }

    public float GetCurCamRotX()
    {
        return MainCamera.localRotation.x;
    }

    public Vector3 GetCameraDefaultRot()
    {
        return CamDefaultRotation;
    }

    public Vector3 GetCameraDefaultPos()
    {
        return CamDefaultPos;
    }

	public void MoveIntroCamera()
	{
		CDebug.Log("      $$$$$$   MoveIntroCamera() ");
		float _time = 1.0f;

        CurHelloCamRot = MainCamera.localRotation.x;
        DOTween.To(() => CurHelloCamRot,
            changeValue =>
            {
                //CDebug.Log("^^^^^^^^^^^^^^^^^^^^^^^^^^^   "+x);
                RotateHelloCamToDefault(changeValue);
            },
            CamDefaultRotation.x, _time)
        .SetEase(Ease.OutQuad)
        .OnComplete(() =>
        {
            CurHelloCamRot = CamDefaultRotation.x;
            SetIsGameStart(false);
        });

        MainCamera.DOLocalMove(GetCameraDefaultPos(), _time).OnComplete(() =>
        {
            GetPageUI().SetActiveUI(true);
            CanvasMgr.SetActiveNameLayer(true);
            PlayerMgr.SetActiveMyPlayerIndecator(true);
            PlayerMgr.SetAllPlayerDefaultRotation();
            if (GetCurrentState() != MGNunchiGameState_Ready.Instance())
            {
                CDebug.Log("TCPMGNunchiRequestManager.Instance.Req_AllPlayerRoundFinish(BPWPacketDefine.NunchiGameReadyType.TO_STAGE_START);", CDebugTag.MINIGAME_NUNCH_WORLD_MANAGER);
                TCPMGNunchiRequestManager.Instance.Req_AllPlayerRoundFinish(BPWPacketDefine.NunchiGameReadyType.TO_STAGE_START);
            }
        });

        SetCameraFOV(Default_FOV, true);
    }

    public void RotateHelloCamToDefault(float x)
    {
        Vector3 curRotation = new Vector3(x, CamDefaultRotation.y, CamDefaultRotation.z);
        MainCamera.localRotation = Quaternion.Euler(curRotation);
    }

    public Vector3 GetResultCameraPosition(int idx)
    {
        return ResultCamTargetPosition[idx];
    }

    public void PlayerLookAtToCamera()
    {
        MGNunchiPlayer myPlayer = PlayerMgr.GetMyPlayer();
        if (myPlayer != null)
        {
            myPlayer.LookAtResultCamera();
        }
    }

    void Update()
    {
        if(NunchiMainSM != null)
        {
            NunchiMainSM.StateMachine_Update();
        }
    }

    public void SetMaxRound(int round)
    {
        MAX_ROUND = round;
    }

    public int GetMaxRound()
    {
        return MAX_ROUND;
    }

    public void SetRoundTime(int time)
    {
        RoundTime = time;// TimeSpan.FromMilliseconds(time).Seconds;
    }

    public int GetRoundTime()
    {
        return RoundTime;
    }

    public void SetGroupID(long grpID)
    {
        GroupID = grpID;
    }

    public long GetGroupID()
    {
        return GroupID;
    }

    public void SetIsGameStart(bool bStart)
    {
        IsStarted = bStart;
    }

    public bool GetIsGameStart()
    {
        return IsStarted;
    }

	//public void SetIsPlayHello(bool isPlay)
	//{
	//	IsPlayHello = isPlay;
	//}

	//public bool GetIsPlayHello()
	//{
	//	return IsPlayHello;
	//}

    //
    public void SetCheckerTimeToCleanCoinUp(bool bCheck)
    {
        CheckerTimeToCleanCoinUp = bCheck;
    }

    public bool GetCheckerTimeToCleanCoinUp()
    {
        return CheckerTimeToCleanCoinUp;
    }

    public void SetIsTimeToCleanCoinItemUp(bool bTime)
    {
        IsTimeToCleanCoinItemUp = bTime;
    }

    public bool GetIsTimeToCleanCoinItemUpValue()
    {
        return IsTimeToCleanCoinItemUp;
    }

    public void IncreaseRoundCount()
    {
        RoundCount++;
    }

    public void SetRoundCount(int round)
    {
        RoundCount = round;
    }

    public int GetRoundCount()
    {
        return RoundCount;
    }

    
    public void SetGameTimer()
    {
        TimeStream timeStream;
        string _streamKey = MGNunchiDefines.BPW_MINIGAME_NUNCHI_TIMESTREAMKEY;
        int totalSec = GetRoundTime();

        if (GlobalTimer.Instance.HasTimeStream(_streamKey) == false)
        {
            GlobalTimer.Instance.GetTimeStream(out timeStream, _streamKey);
            timeStream.SetTime(_streamKey, totalSec, 0, TimeStreamType.DECREASE).OnTimeStreamObservable().Subscribe();
        }
    }

    public void SetFXObj(ref GameObject fxObj, ref ParticleSystem particle, GameObject origin, string particlePath, Transform parent, Vector3 pos, Quaternion rot)
    {
        fxObj = GameObject.Instantiate(origin);
        GameObject _particleObj = fxObj.transform.Find(particlePath).gameObject;
        particle = _particleObj.GetComponent<ParticleSystem>();

        fxObj.transform.SetParent(parent);
        fxObj.transform.localPosition = pos;
        fxObj.transform.localRotation = rot;
        fxObj.transform.localScale = Vector3.one;
    }

    public void SetFXParticleFinish(float lifeTime, GameObject fxObj)
    {
        var disposer = new SingleAssignmentDisposable();
        disposer.Disposable = Observable.Timer(TimeSpan.FromSeconds(lifeTime))
        .Subscribe(_ =>
        {
            fxObj.SetActive(false);
            //CDebug.Log(GainFXParticle + " disappear "+ + lifeTime + " sec later.");
            disposer.Dispose();
        })
        .AddTo(this);
    }

    public void SetChangeState(CState<MGNunchiWorldManager> state)
    {
        //Debug.Log("WorldManager.SetChangeState() " + state);
        NunchiMainSM.ChangeState(state);
    }

    public CState<MGNunchiWorldManager> GetCurrentState()
    {
        return NunchiMainSM.GetCurrentState();
    }

    public int GetCoinItemSeatMapIdx(int mapIdx)
    {
        for(int i=0; i< CoinItemSeatMapIdx.Length; ++i)
        {
            if(CoinItemSeatMapIdx[i] == mapIdx)
            {
                return i;
            }
        }

        return -1;
    }

    public GameObject GetResourceObj(string resPath)
    {
        CResourceData resData = CResourceManager.Instance.GetResourceData(resPath);
        return resData.Load<GameObject>(gameObject);
    }

    public Sprite GetResourceSprite(string path, GameObject targetObj)
    {
        CResourceData _resData = new CResourceData()
        {
            Index = path,
            From = ResourceLoadFrom.ASSETBUNDLE
        };

        return _resData.LoadSprite(targetObj);

    }

    public bool IsEffectItem(BPWPacketDefine.NunchiGameItemType type)
    {
        if (type == BPWPacketDefine.NunchiGameItemType.COIN || type == BPWPacketDefine.NunchiGameItemType.NONE)
        {
            return false;
        }

        return true;
    }


	public bool CheckFailPlayer()
	{
		return PlayerMgr.CheckFailPlayer();
	}

    #region ANIM_EVENT
    public void PlayPlatformAnimationFromAnimEvent(int pid)
    {
        MGNunchiPlayer _player = PlayerMgr.GetPlayer(pid);
        MapMgr.PlayJumpDownAnimByInfo(_player.PlayerInfo.Map_MyLocation);
    }
    #endregion ANIM_EVENT


    #region RESPONSE
    public void Res_PerformMotion(int motionIndex)
    {
		MGNunchiPlayer myNetPlayer = PlayerMgr.GetMyPlayer();
		long motionID = ExpressionEquipManager.Instance.GetEquipedExpressionID( myNetPlayer.PlayerInfo.AvatarType, motionIndex );
		BPWorldMotionData motionData = BPWorldDataManager.Instance.GetMotionData( motionID );
        MotionData motion = CAvatarInfoDataManager.GetMotionDataByID(motionData.Animation);
        if (motionData.AnimaionLoop == EBPWorldMotionLoopState.ONCE)
        {
            myNetPlayer.PlayAnimation(motion.AnimParam);
        }
        else
        {
            myNetPlayer.PlayAnimation(motion.AnimParam);
        }
    }

    #endregion RESPONSE

    #region BROADCAST

    public void BroadCast_PerformEmoticon(int pid, long emoticonID)
    {
        if (GetPageUI() != null)
        {
            var emoticonTableData = BPWorldDataManager.Instance.GetEmoticonData(emoticonID);
            if (emoticonTableData != null)
            {
                /*GetPageUI().*/AddEmoticon(pid, emoticonID);
            }
        }
    }


    public void BC_PerformMotion(int pid, BPWorldMotionData motionTableData)
    {
        //if (worldLoader.IsFinishLoadBPWorld == false)
        //{
        //    return;
        //}
        MGNunchiPlayer netPlayer = PlayerMgr.GetPlayer(pid);

        if (netPlayer == null || motionTableData == null)
        {
            return;
        }
        MotionData motion = CAvatarInfoDataManager.GetMotionDataByID(motionTableData.Animation);

        if (motionTableData.AnimaionLoop == EBPWorldMotionLoopState.ONCE)
        {
            netPlayer.PlayAnimation(motion.AnimParam);
        }
        else
        {
            netPlayer.PlayAnimation(motion.AnimParam);
        }
    }

    public void BC_SlotExecuteByMotion(int pid, long motionTid)
    {
        MGNunchiPlayer netPlayer = PlayerMgr.GetPlayer( pid );
        BPWorldMotionData _motionData = BPWorldDataManager.Instance.GetMotionData( motionTid );
        MotionData motion = CAvatarInfoDataManager.GetMotionDataByID( _motionData.Animation );

        if (netPlayer == null || _motionData == null || motion == null)
        {
            return;
        }

        if (_motionData.AnimaionLoop == EBPWorldMotionLoopState.ONCE)
        {
            netPlayer.PlayAnimation( motion.AnimParam );
        }
        else
        {
            netPlayer.PlayAnimation( motion.AnimParam );
        }
    }


    public void BC_SlotExecuteByEmoticon(int pid, long emoticonTid)
    {
        MGNunchiPlayer netPlayer = PlayerMgr.GetPlayer( pid );
        BPWorldEmoticonData emoticonData = BPWorldDataManager.Instance.GetEmoticonData( emoticonTid );
        if (netPlayer == null || emoticonData == null)//|| motion == null)
        {
            return;
        }

        Transform rootTransform = netPlayer.GetUIDummyBone().transform;

        BPWorldEmoticon _emoticonObj = GetPageUI().IATCreator.AddEmoticon( emoticonTid, rootTransform );
        netPlayer.SetEmoticonTag( _emoticonObj.gameObject );
    }
    #endregion BROADCAST


    public void ShowQuitAlert()
    {
        //스트링 수정 해야함
        popupAlert = popupService.Alert();
        popupAlert.SetData(new PopupAlert.Setting()
        {
            Flags = PopupFlags.NORMAL,
            btnType = PopupAlert.BUTTON_TYPE.BTN_TWO,
            bLauncher = false,
            Title = 91350004,           // 확인
            Desc = 9171010,            // 게임을 중단하고 나가시겠습니까? 
            StringOk = 91350006,        // 나가기
            StringCancel = 91350005,    //취소
        });

        popupAlert.ShowAsObservable().Subscribe(res =>
        {
            if (res.IsSucess)
            {
				TCPMGNunchiRequestManager.Instance.Req_RoomLeave();
                //TitleDirector.SetAvatarObjBack();
                //SoundManager.Instance.PlayEffect(6810021); // management sound : 부서 정보 버튼 (se_ui_005)
                //NavigationManager.Goto<NavigationData>(SceneID.LOBBY_SCENE);
                //TCPCommonRequestManager.Instance.ReqLogout();
            }
            else
            {
                popupAlert.Close();
            }

            popupAlert = null;
        } );
    }

    public void ClosePopupAlert()
    {
        if (popupAlert != null)
        {
            popupAlert.Close();
        }
    }


    public void SetLoadingForFinal()
    {
        AddLoadingForFinal().Subscribe( _ => { } ).AddTo( this );
    }

    public IObservable<Unit> AddLoadingForFinal()
    {
        SceneTransitionService transitionService = CCoreServices.GetCoreService<SceneTransitionService>().SetTransition(SceneTransitionLoading.Instance);
        CDebug.Log("AddLoadingForFinal", CDebugTag.MINIGAME_PIHAGI);
        return transitionService.In(new SceneTransitionLoadingSetting(SceneID.MINIGAME_NUNCHI_SCENE)).Do(_ =>
        {
        }).AsUnitObservable();
    }


    public IObservable<Unit> CloseForFinal()
    {
        SceneTransitionService transitionService = CCoreServices.GetCoreService<SceneTransitionService>().SetTransition(SceneTransitionLoading.Instance);
        CDebug.Log("CloseForFinal", CDebugTag.MINIGAME_PIHAGI);
        return transitionService.Out();
    }

    public MGNunchiCanvasManager GetCanvasManager()
    {
        return CanvasMgr;
    }

    public MGNunchiPageUI GetPageUI()
    {
        return CanvasMgr.PageUI;
    }

    #region TUTORIAL

    private SkinnedMeshRenderer GetMidFlatRenderer()
    {
        MGNunchiMapInfo _mapInfo = MapMgr.GetMapInfoByXY( 1, 1 );
        SkinnedMeshRenderer[] _render = _mapInfo.MapObj.GetComponentsInChildren<SkinnedMeshRenderer>();
        if(_render != null )
        {
            return _render[0];//.transform;
        }

        return null;
    }

    //block tutorial
    public void SetTutorial_Step1()
    {
        MGNunchiPageUI pageUI = GetPageUI();
        if (pageUI != null)
        {
            RectTransform midArrBox = pageUI.GetMidArrBox();
            SkinnedMeshRenderer midFlatRender = GetMidFlatRenderer();
            NunchiTutorial.Initialize_Step1(midFlatRender, this);
        }
    }

    public void SetTutorial_Step2()
    {
        MGNunchiPageUI pageUI = GetPageUI();
        if (pageUI != null)
        {
            RectTransform playerBox = pageUI.GetPlayerBox();
            NunchiTutorial.Initialize_Step2(playerBox);
        }
    }

    public void SetTutorialDone()
    {
        MGNunchiServerDataManager.Instance.IsSinglePlay = false;
        BPWorldServerDataManager.Instance.IsMGHasStartTutorial = false;
        TCPMGNunchiRequestManager.Instance.Req_GamePlayResume();
        SetChangeState(MGNunchiGameState_InPlayingGame.Instance());
    }

    //Step 1 done
    public void SetTutorial_SelectMapIndex(int mapIdx)
    {
        CDebug.Log($"SetTutorial_SelectMapIndex(int mapIdx) - {mapIdx}", CDebugTag.MINIGAME_NUNCHI);

        TCPMGNunchiRequestManager.Instance.Req_GamePlayResume();
        MapMgr.OnClickSelectMap(mapIdx);
    }

    public void SetTutorialStep(TutorialStep step)
    {
        NunchiTutorial.SetTutorialStep(step);
    }

    public TutorialStep GetTutorialStep()
    {
        return NunchiTutorial.GetTutorialStep();
    }

    #endregion TUTORIAL

    public void ClearNameTagDic()
    {
        if (NameTagsDic != null)
        {
            foreach(List<UI_BPWorldNameTag> list in NameTagsDic.Values)
            {
                foreach(UI_BPWorldNameTag obj in list)
                {
                    Destroy(obj);
                }
            }
            NameTagsDic.Clear();
        }
    }

    public void Release()
    {
		if(PlayerMgr.GetMyPlayer() != null)
		{
			CCharacterController_BPWorld cntlr = PlayerMgr.GetMyPlayer().PlayerInfo.PlayerObj.GetComponent<CCharacterController_BPWorld>();
			if(cntlr != null)
			{
				cntlr.DestroyComponents();
				UnityEngine.Object.Destroy(cntlr);
				cntlr = null;
			}
		}

		StaticAvatarManager.SetAvatarObjBack();

        if (NameTagsDic != null)
        {
            foreach (List<UI_BPWorldNameTag> list in NameTagsDic.Values)
            {
                foreach (UI_BPWorldNameTag obj in list)
                {
                    Destroy(obj);
                }
            }
            NameTagsDic.Clear();
            NameTagsDic = null;
        }

        if (CanvasMgr != null)
        {
            CanvasMgr.Release();
            CanvasMgr = null;
        }

        if (PlayerMgr != null)
        {
            PlayerMgr.Release();
            PlayerMgr = null;
        }

        if (MapMgr != null)
        {
            MapMgr.Release();
            MapMgr = null;
        }

        if (CoinMgr != null)
        {
            CoinMgr.Release();
            CoinMgr = null;
        }

        MGNunchiServerDataManager.Instance.Clear();


        if (NunchiMainSM != null) NunchiMainSM = null;
    }
}


public enum NAMETAG_TYPE
{
    PLAYER = 0,
    ITEM
}
