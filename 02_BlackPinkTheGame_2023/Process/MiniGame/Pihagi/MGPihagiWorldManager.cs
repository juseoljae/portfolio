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
using static APIHelper.GameServerCommonResponseHandler;
using BPWPacketDefine;
using DG.Tweening;
using ui.navigation;

public class MGPihagiWorldManager : MonoBehaviour
{
    private MGPihagiCanvasManager CanvasMgr;
    private MGPihagiPageUI PageUI;
    public BPWorldSceneParentObjects ParentObjects { get; private set; }

    public CStateMachine<MGPihagiWorldManager> PihagiMainSM;

    [HideInInspector]
    public MGPihagiPlayersManager PlayerMgr;
    [HideInInspector]
    public MGPihagiEnemysManager EnemyMgr;
    [HideInInspector]
    public MGPihagiMapManager MapMgr;


    private MGPihagiTutorial PihagiTutorial;

    public Transform MainCameraObj;
    private Camera MainCamera;
    [Header("[Camera Value] ")]
    public Vector3 CamZoomInPos;
    public Vector3 CamZoomInRot;
    public Vector3 CamDefaultPos;
    public Vector3 CamDefaultRotation;
    public float CamDefaultFOV;

    [Header("[Result Expression]")]
    public Vector3 ResultCamPos;
    public Vector3 ResultCamRot;
    public float ResultCamFOV;
    public Vector3 ResultPlayerPos;
    public float ResultPlayerAngle;
    public PopupMGPihagiResult ResultPopup;
    //public bool bFinishResultSet;

    private byte GameTime;
    private byte PlayerLife;

    //Player death target Position
    [Header("[Player Death Targets]")]
    public Dictionary<ENEMY_POSDIR, Vector3> PlayerDeathTargetPos; // KEY: Enemy direction
	

    private PopupAlert popupAlert;
    private CPopupService popupService;

    private BPWorldIconAndTagCreator IATCreator;
    public const string NAME_POOL_PARENT_OBJECT = "POOL_MONOOBJECTS(Dynamic Create)";
    public const string NAME_POOL_NAME_TAG = "NAME TAG POOL(Dynamic Create)";
    private List<UI_BPWorldNameTag> NameTagList = new List<UI_BPWorldNameTag>();

    public void Initialize()
    {
        InitMainCamera();
        CanvasMgr = (MGPihagiCanvasManager)MGPihagiManager.Instance.GetCanvasManager();
        PageUI = CanvasMgr.PageUI;
        PlayerMgr = transform.Find("World/Players").GetComponent<MGPihagiPlayersManager>();

        EnemyMgr = transform.Find("World/Enemys").GetComponent<MGPihagiEnemysManager>();
        EnemyMgr.Initialize(this);

        MapMgr = new MGPihagiMapManager();
        Transform fxGround = transform.Find("World/BG/FX_Ground");
        MapMgr.Initialize(this, fxGround);
		
        popupService = CCoreServices.GetCoreService<CPopupService>();

        PihagiMainSM = new CStateMachine<MGPihagiWorldManager>(this);
        PlayerDeathTargetPos = new Dictionary<ENEMY_POSDIR, Vector3>();

        IATCreator = new BPWorldIconAndTagCreator();

        PihagiTutorial = new MGPihagiTutorial();

        FindAndCreateObject(PlayerMgr.gameObject, EnemyMgr.gameObject);

        PlayerMgr.Initialize(this);

        SetGameInfo();

        PageUI.InitMGPihagiPageUI(this);

        //SetBG NPCs
        SetBGNPCs();

        //fx origin object
        //LoadFX_OriginObject();

        SetPlayerDeathTargetPos();

		
        SetupNameTag();
    }

 
    public void SetupNameTag()
    {
        MGPihagiCanvasManager canvasMgr = GetCanvasMgr();
        RectTransform canvasRect = canvasMgr.GetComponent<RectTransform>();
        BPWorldLayerObject layerObj = canvasMgr.LayerObjects;
        IATCreator.Intialize(gameObject, canvasRect, PlayerMgr.NameTagPoolMgr, MainCameraObj );
        IATCreator.SetUpLayerObject(layerObj);
    }

    public void AddNameTag(int pid, string name, GameObject charObj)
    {
        GameObject dummyObj = PlayerMgr.GetPlayerDummyBone(pid);
		long uid = PlayerMgr.GetPlayerUIDByPID(pid);
		Tuple<bool, int> beforeRankInfo = BPWorldRankingManager.Instance.IsInRank( uid );
		UI_BPWorldNameTag nameTag = null;
        bool isMyPlayer = ( uid == PlayerMgr.MyPlayerUID );

        if (charObj == null || dummyObj == null) return;

        if ( isMyPlayer == true )
        {
			nameTag = IATCreator.NameTagCreator.AddNameTagByMyPlayer( name, beforeRankInfo.Item2, dummyObj );
        }
		else
		{
			bool isMyFriend = APIHelper.Friend.friendData.ContainsKey( uid );
			nameTag = IATCreator.NameTagCreator.AddNameTagByOtherPlayer( name, isMyFriend, beforeRankInfo.Item2, dummyObj );
		}

        nameTag.transform.localScale = Vector3.one;
        NameTagList.Add(nameTag);
    }

    public void SetActiveNameTag(bool bActive)
    {
        foreach(UI_BPWorldNameTag obj in NameTagList)
        {
            obj.gameObject.SetActive(bActive);
        }
        //IATCreator.SetActiveNameTag(bActive);
    }


    public void AddEmoticon(int pid, long emoticonID)
    {
        GameObject uiDummuBone = PlayerMgr.GetPlayerDummyBone(pid);
        BPWorldEmoticon emoticon = null;

        emoticon = IATCreator.AddEmoticon(emoticonID, uiDummuBone.transform, MainCameraObj);
		emoticon.ShowEmoticon();
	}

    public void InitMainCamera()
    {
        MainCameraObj = transform.Find("MainViewCamera");
        MainCamera = MainCameraObj.GetComponent<Camera>();
        CamDefaultFOV = MainCamera.fieldOfView;
        MainCameraObj.localPosition = CamZoomInPos;
        MainCameraObj.localRotation = Quaternion.Euler(CamZoomInRot);
    }

    public Vector3 GetCameraDefaultRot()
    {
        return CamDefaultRotation;
    }

    public Vector3 GetCameraDefaultPos()
    {
        return CamDefaultPos;
    }

    public void SetStartCamZoomOut()
    {
		if(CamDefaultFOV != MainCamera.fieldOfView)
		{
			SetCameraFOV(CamDefaultFOV);
		}
        MainCameraObj.DOLocalRotate(CamDefaultRotation, 1);
        MainCameraObj.DOLocalMove(CamDefaultPos, 1)
			.OnComplete(() =>
            {
				CDebug.Log(" #############  DOLocalMove().OnComplete  ######################### ");
                SetChangeState(MGPihagiGameState_ShowTitle.Instance());
            });
    }

    public void SetResultCamera()
    {
		SetCameraFOV(ResultCamFOV);
        MainCameraObj.DOLocalRotate(ResultCamRot, 1);
        MainCameraObj.DOLocalMove(ResultCamPos, 1)
        .OnComplete(() =>
        {
			CDebug.Log( "*********************************************************** finish result cam " );
			GameResultInfo _resultInfo = MGPihagiServerDataManager.Instance.GetGameResultInfo();
			if(_resultInfo != null)
			{
					
				MGPihagiPlayer _player = PlayerMgr.GetMyPlayer();
				if(_resultInfo.MyRank <= 2)
				{
                    SoundManager.Instance.PlayEffect( 6830020 ); // list 119
					_player.PlayAnimation("game_victory");
				}
				else
				{
                    SoundManager.Instance.PlayEffect( 6830021 ); // list 120
                    _player.PlayAnimation("game_failure");
				}
			}
            GetPageUI().FixedJoystick.HandlePointerUp();
			SetChangeState( MGPihagiGameState_ResultPopup.Instance());
        });
    }

	private void SetCameraFOV(float targetFOV)
	{
        //MainCamera.fieldOfView.
        DOTween.To(() => MainCamera.fieldOfView, x => MainCamera.fieldOfView = x, targetFOV, 1)
        .SetEase(Ease.Linear)
        .OnComplete(() =>
        {
            MainCamera.fieldOfView = targetFOV;
            //
        });
	}

    public void SetPlayerForResult()
    {
        MGPihagiPlayer myPlayer = PlayerMgr.GetMyPlayer();
        myPlayer.PlayerInfo.PlayerObj.transform.localPosition = ResultPlayerPos;
        myPlayer.PlayerInfo.PlayerObj.transform.localRotation = Quaternion.Euler(0, ResultPlayerAngle, 0);
        myPlayer.PlayerController.SetAnimationParam("idle");
		PlayerMgr.SetActiveMyPlayer();
        PlayerMgr.SetHideOtherPlayer();
    }

    private void FindAndCreateObject(GameObject playerParent, GameObject enemyParent)
    {
        GameObject poolParentObject = new GameObject(NAME_POOL_PARENT_OBJECT);

        GameObject nameTagPoolParentObject = new GameObject(NAME_POOL_NAME_TAG);
        nameTagPoolParentObject.transform.SetParent(transform);
        nameTagPoolParentObject.transform.localScale = Vector3.one;

        ParentObjects = new BPWorldSceneParentObjects(playerParent, playerParent, enemyParent, null, poolParentObject, null, null, null, null, null, nameTagPoolParentObject, null);
    }

    public void LoadFX_OriginObject()
    {
        //PlayerMgr.LoadFXOrigin();
        EnemyMgr.LoadDashFXOrigin();
    }


	//bool test;

    // Update is called once per frame
    void Update()
    {
        if(PihagiMainSM != null)
        {
            PihagiMainSM.StateMachine_Update();
        }

		//if(Input.GetKeyUp(KeyCode.Space))
		//{
		//	test = !test;
		//	PlayerMgr.GetMyPlayer().SetJoyStickStop(test);
		//}
    }

    private void SetGameInfo()
    {
        //from Basetable
        List<ConfigureData> _info = Configure.GetConfigureDataArray(CONFIGURE_TYPE.MINIGAME_PIHAGI_DATA);
        for(int i = 0; i < _info.Count; i++)
        {
            ConfigureData info = _info[i];
            if(info != null)
            {
                if(info.Value1.ToInt32() == 1)
                {
                    PlayerLife = (byte)info.Value2.ToInt32();
                }
                else if(info.Value1.ToInt32() == 2)
                {
                    GameTime = (byte)info.Value2.ToInt32();
                }
            }
        }
    }

    public byte GetGameTime()
    {
        return GameTime;
    }

    public void SetGameTimer()
    {
        TimeStream timeStream;
        string _streamKey = MGPihagiDefine.BPW_MINIGAME_PIHAGI_TIMESTREAMKEY;
        int totalSec = GetGameTime();

        if (GlobalTimer.Instance.HasTimeStream(_streamKey))
        {
            GlobalTimer.Instance.RemoveTimeStream( _streamKey);
        }

        GlobalTimer.Instance.GetTimeStream( out timeStream, _streamKey );
        timeStream.SetTime( _streamKey, totalSec, 0, TimeStreamType.DECREASE ).OnTimeStreamObservable().Subscribe();
    }


    private void SetBGNPCs()
    {
        Transform bgNPCObj = transform.Find("World/BG/BG NPC");
        Animator[] animators = bgNPCObj.GetComponentsInChildren<Animator>();

        for(int i = 0; i<animators.Length; ++i)
        {
            animators[i].SetTrigger("cheer");
        }
    }

    private void SetPlayerDeathTargetPos()
    {
        PlayerDeathTargetPos.Add(ENEMY_POSDIR.LEFT, new Vector3(16.3f, 6, 0));
        PlayerDeathTargetPos.Add(ENEMY_POSDIR.RIGHT, new Vector3(-2.3f, 6, 0));
        PlayerDeathTargetPos.Add(ENEMY_POSDIR.TOP, new Vector3(0, 6, -2.3f));
    }

    public Vector3 GetPlayerDeathTargetPos(ENEMY_POSDIR attackerDir)
    {
        return PlayerDeathTargetPos[attackerDir];
    }


    public Vector3 GetVector3(float xpos, float ypos, float zpos)
    {
        //pktVec.y is actually z pos
        return new Vector3(xpos * 0.001f, ypos * 0.001f, zpos * 0.001f);
    }

    public void SetChangeState(CState<MGPihagiWorldManager> state)
    {
        PihagiMainSM.ChangeState(state);
    }

    public MGPihagiCanvasManager GetCanvasMgr()
    {
        return CanvasMgr;
    }

	
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
				SetChangeState(MGPihagiGameState_Exit_Leave.Instance());
				//TCPMGPihagiRequestManager.Instance.Req_RoomLeave();
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

    public MGPihagiPageUI GetPageUI()
    {
        return CanvasMgr.PageUI;
    }


	public void SetMyPlayerJoyStickStop(bool bStop)
	{			
		PlayerMgr.GetMyPlayer().SetJoyStickStop(bStop);	
	}


	#region RESPONSE
	public void Res_PerformMotion(int motionIndex)
    {
		MGPihagiPlayer myNetPlayer = PlayerMgr.GetMyPlayer();
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
                AddEmoticon(pid, emoticonID);
            }
        }
    }
	

    public void BC_PerformMotion(int pid, BPWorldMotionData motionTableData)
    {
        MGPihagiPlayer netPlayer = PlayerMgr.GetPlayer(pid);

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
    #endregion BROADCAST

    #region TUTORIAL

    //block tutorial
    public void SetTutorial_Step1()
    {
        PihagiTutorial.Initialize_Step1(this);
    }

    public void SetTutorial_Step2()
    {
        PihagiTutorial.Initialize_Step2();
    }

    public void SetTutorial_Step3()
    {
        RectTransform playerBox = GetPageUI().GetPlayerBox();
        RectTransform joyStickBox = GetPageUI().GetJoyStickBox();
        //RectTransform btnGrpBox = GetPageUI().GetButtonGrpBox();

        PihagiTutorial.Initialize_Step3(playerBox, joyStickBox/*, btnGrpBox*/);
    }

    public void SetTutorial_Step4()
    {
        PihagiTutorial.Initialize_Step4();
    }

    public void SetTutorial_InitUIPos()
    {
        GetPageUI().InitBottomUITutorialPos();
    }

    public void SetTutorial_Step3_AppearUI()
    {
        GetPageUI().AppearBottomUITutorialPos();
    }

    public void SetTutorialDone()
    {
        MGNunchiServerDataManager.Instance.IsSinglePlay = false;
        BPWorldServerDataManager.Instance.IsMGHasStartTutorial = false;
        TCPMGPihagiRequestManager.Instance.Req_GamePlayResume();
        SetChangeState(MGPihagiGameState_Play.Instance());
    }

    
    public void SetTutorialStep(TutorialStep step)
    {
        PihagiTutorial.SetTutorialStep(step);
    }

    public TutorialStep GetTutorialStep()
    {
        return PihagiTutorial.GetTutorialStep();
    }

    #endregion TUTORIAL


    public ENEMY_POSDIR GetEnemyDir(int spawnIdx)
    {
        ENEMY_POSDIR _retDir = ENEMY_POSDIR.NONE; // gap : 0.6f;

        if (spawnIdx >= MGPihagiDefine.GROUND_DIR_LEFT_STARTIDX && spawnIdx < MGPihagiDefine.GROUND_DIR_TOP_STARTIDX)
        {
            _retDir = ENEMY_POSDIR.LEFT;
        }
        else if (spawnIdx >= MGPihagiDefine.GROUND_DIR_TOP_STARTIDX && spawnIdx < MGPihagiDefine.GROUND_DIR_RIGHT_STARTIDX)
        {
            _retDir = ENEMY_POSDIR.TOP;
        }
        else if (spawnIdx >= MGPihagiDefine.GROUND_DIR_RIGHT_STARTIDX)
        {
            _retDir = ENEMY_POSDIR.RIGHT;
        }

        //if (pos.x < 3.6f && pos.z < 10.6f)
        //{
        //    _retDir = ENEMY_POSDIR.LEFT;
        //}
        //else if(pos.x > 10.6f && pos.z < 10.6f)
        //{
        //    _retDir = ENEMY_POSDIR.RIGHT;
        //}
        //else if(pos.z > 10.6f)
        //{
        //    _retDir = ENEMY_POSDIR.TOP;
        //}

        //CDebug.Log( $"      [GetEnemyDir] spawnIdx = {spawnIdx}, dir = {_retDir}" );

        return _retDir;
    }

    public void SetLoadingForFinal()
    {
        AddLoadingForFinal().Subscribe( _ => { } ).AddTo( this );
    }

    public IObservable<Unit> AddLoadingForFinal()
	{
		SceneTransitionService transitionService = CCoreServices.GetCoreService<SceneTransitionService>().SetTransition(SceneTransitionLoading.Instance);
		CDebug.Log("AddLoadingForFinal" , CDebugTag.MINIGAME_PIHAGI);
        return transitionService.In(new SceneTransitionLoadingSetting(SceneID.MINIGAME_PIHAGI_SCENE)).Do(_ =>
        {
        }).AsUnitObservable();
	}

	
    public IObservable<Unit> CloseForFinal()
    {
		SceneTransitionService transitionService = CCoreServices.GetCoreService<SceneTransitionService>().SetTransition(SceneTransitionLoading.Instance);
        CDebug.Log("CloseForFinal" , CDebugTag.MINIGAME_PIHAGI);
        return transitionService.Out();
		//.Do(_ =>
		//	{
		//});
    } 

    public void ClearObjects()
    {
        if (NameTagList != null)
        {
            foreach(UI_BPWorldNameTag obj in NameTagList)
            {
                Destroy(obj);
            }
            NameTagList.Clear();
        }
    }


	public void Release()
    {		
		if(PlayerMgr != null)
		{
			if(PlayerMgr.GetMyPlayer() != null)
			{
				SetMyPlayerJoyStickStop(false);
				CCharacterController_BPWorld cntlr = PlayerMgr.GetMyPlayer().PlayerInfo.PlayerObj.GetComponent<CCharacterController_BPWorld>();
				if(cntlr != null)
				{
					cntlr.DestroyComponents();
					GameObject.Destroy(cntlr);
					cntlr = null;
				}
			}
		}
		StaticAvatarManager.SetAvatarObjBack();
        if (CanvasMgr != null) CanvasMgr = null;
        if (PageUI != null)
        {
            PageUI.Release();
            PageUI = null;
        }
        if (ParentObjects != null) ParentObjects = null;
        if (PihagiMainSM != null) PihagiMainSM = null;
        if (PlayerMgr != null)
        {
            PlayerMgr.Release();
            PlayerMgr = null;
        }
        if (EnemyMgr != null)
        {
            EnemyMgr.Release();
            EnemyMgr = null;
        }

        if (NameTagList != null)
        {
            foreach (UI_BPWorldNameTag obj in NameTagList)
            {
                Destroy(obj);
            }
            NameTagList.Clear();
            NameTagList = null;
        }
    }
}
