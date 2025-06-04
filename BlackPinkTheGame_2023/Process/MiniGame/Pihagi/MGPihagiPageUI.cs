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
using UnityEngine.UI;
using UniRx;
using System.Collections.Generic;
using ui.navigation;
using BPWPacketDefine;
using DG.Tweening;

public class MGPihagiPageUI : APage
{
    //MGPihagi
    public const uint LayerOrder = 0;

    private CPopupService popupService;

    private MGPihagiWorldManager WorldMgr;

    //private BPWorldIconAndTagCreator IATCreator;
    private GameObject poolParentObject;
    public GameObject nameTagPoolParentObject;

    //private UI_BPWorldExpressionButtons_LEGACY expressionButtons = null;

    #region UI
    private FixedJoystick fixedJoyStick = null;
    private Button BtnClose;

    private GameObject TimerObj;
    private Text TimerTxt;

    public class PlayerInfoUI
    {
        public GameObject MyInfoObj;
        public Text MyName;
        public GameObject[] MyLifeObjs;

        public GameObject PlayerInfoObj;
        public Text PlayerName;
        public GameObject[] PlayerLifeObjs;
    }
    private PlayerInfoUI[] PlayerUI;

    //Roots
    private Transform PlayerInfoRoot;
    private Transform GameInfoRoot;
    //private Transform BtnGroupRoot;
    //private float BtnGroupRootOrgYPos;
    //private float BtnGroupRootTutorialYPos;
    private Transform JoyStickRoot;
    private float JoyStickOrgYPos;
    private float JoyStickTutorialYPos;
    private Transform StartInfoRoot;
	public Transform FinishRoot;
    private Text[] StartInfoText;
    private Text StartInfoStageText;
    private Transform CountDownRoot;
    private Text CountDownNumText;
    private float StartTime;
    private bool bStartTimer;
    private int CurCountDownTime;
    private Transform StartRoot;
    private Animation StartAnim;
    public bool bPlaiedStartAnim;

    public enum SHOW_TITLE_STEP
    {
        TITLE,
        INFO,
        COUNTDOWN,
        START,
        ALL
    }
    private SHOW_TITLE_STEP showTitleStep;

    public GameObject PopupMsgRoot;
    public GameObject PopupMsg_TieProcing;

    public GameObject PopupMsg_TieProc;
    public Text PopupMsg_DrawTxt;
    public Text PopupMsg_SubFinalistTxt;
    public GameObject PopupMsg_Finalist;
    public Text PopupMsg_FinalistTxt;
    #endregion UI

    #region TUTORIAL
    private RectTransform PlayerBox;
    private RectTransform JoyStickBox;
    //private RectTransform ButtonGroupBox;
    #endregion TUTORIAL

    AVATAR_TYPE MyAvatarType;

    public FixedJoystick FixedJoystick => fixedJoyStick;

    private SingleAssignmentDisposable timerDisposer;
    private TimeStream PlayTimeStream;
    private float remainTimeSeconds;
    private int prevTimeSeconds;

    private long[] strID = new long[] {
        9171001,    //0 예선전
        9171003,    //1 결승전
        9170051,    //2 2명만 결승전 진출!
        9170052,    //3 최후의 1인이 되어보자!
        9170053,    //4 지금 퇴장하면 보상을 받을 수 없습니다.
        9170054,    //5 무승부가 되어 다음 경기 진출자를 추첨합니다
        9171017,    //6 다음 라운드 진출자 : 
        9171013,    //7 진출자 명단 : {0}, { 1}\n진출: 
    };


    void Awake()
    {
        if (!enabled)
        {
            return;
        }

        Order = LayerOrder;
        Name = gameObject.name;
        Go = gameObject;

        popupService = CCoreServices.GetCoreService<CPopupService>();
    }

    public override void Initialize(){}

    public void InitMGPihagiPageUI(MGPihagiWorldManager worldMgr)
    {
        WorldMgr = worldMgr;

        //IATCreator = new BPWorldIconAndTagCreator();
        //expressionButtons = new UI_BPWorldExpressionButtons_LEGACY();

        PlayerUI = new PlayerInfoUI[MGPihagiDefine.PLAYER_COUNT];

        bPlaiedStartAnim = false;

        FindComponents();
        SetUIObjects();
        SetUIButton();

    }

    public void SetUp()
    {
        //SetupExpressionButtons();
		SetTimerUIText(WorldMgr.GetGameTime());
		SetActiveTimerObj(true);
    }


    //private void SetupExpressionButtons()
    //{
    //    MyAvatarType = WorldMgr.PlayerMgr.GetMyPlayer().PlayerInfo.AvatarType;
    //    expressionButtons.Initialize(transform , "obj_metaverse_ButtonGroup" );
    //    //if (MyAvatarType != AVATAR_TYPE.AVATAR_NONE)
    //    {
    //        expressionButtons.BindToOnClick(expressionButtons.SetMotionSlot, OnClickMotionSlot, expressionButtons.SetEmoticonSlot, OnClickEmoticonSlot);
    //        expressionButtons.SetMotionSlotIcon(MyAvatarType, true);
    //        expressionButtons.SetEmoticonSlotIcon(MyAvatarType, true);
    //    }
    //}

    public void SetPlayerInfoUI(MGPihagiPlayerInfo pInfo, int idx)
    {
        if (idx > PlayerUI.Length - 1) return;

        int myPID = MGPihagiServerDataManager.Instance.GetMyPlayerPID();
        if (pInfo.PlayerID == myPID)
        {
            PlayerUI[idx].MyInfoObj.SetActive(true);
            PlayerUI[idx].PlayerInfoObj.SetActive(false);
            PlayerUI[idx].MyName.text = pInfo.NickName;

            SetPlayerLifeUI(pInfo.LifeCount, PlayerUI[idx].MyLifeObjs);
        }
        else
        {
            PlayerUI[idx].MyInfoObj.SetActive(false);
            PlayerUI[idx].PlayerInfoObj.SetActive(true);
            PlayerUI[idx].PlayerName.text = pInfo.NickName;

            SetPlayerLifeUI(pInfo.LifeCount, PlayerUI[idx].PlayerLifeObjs);
        }
    }

    private void SetPlayerLifeUI(int lifeCount, GameObject[] lifeObjs)
    {
        for (int i = 0; i < MGPihagiDefine.PLAYER_MAX_LIFE_COUNT; ++i)
        {
            if (i < lifeCount)
            {
                lifeObjs[i].SetActive(true);
            }
            else
            {
                lifeObjs[i].SetActive(false);
            }
        }
    }

    private void FindComponents()
    {
        fixedJoyStick = GameObjectHelperUtils.FindComponent<FixedJoystick>(transform, "btn/Joystick");
		
		//if(fixedJoyStick != null)
		//{
		//	SingleAssignmentDisposable joyStickDisposable = new SingleAssignmentDisposable();
		//	joyStickDisposable.Disposable = fixedJoyStick.IsPointerUp.Where(releaseJoystick => releaseJoystick == true)
		//		.Subscribe(_ =>
		//		{					
		//			CDebug.Log("%%%%%%%%%%%%%%%%%% JoyStick Release %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%");
		//			//PointerEventData curPosition = new PointerEventData(EventSystem.current);
		//			//fixedJoyStick.OnPointerUp(curPosition);
		//			//joyStickDisposable.Dispose();
		//		} ).AddTo(this);
		//}
	}

	private void SetUIObjects()
    {
        TimerObj = transform.Find("gameInfo/count").gameObject;
        TimerTxt = TimerObj.transform.Find("text/text_count").GetComponent<Text>();
        SetTimerUIText(WorldMgr.GetGameTime());

        PlayerInfoRoot = transform.Find("playerInfo");
        PlayerBox = PlayerInfoRoot.GetComponent<RectTransform>();
        for (int i=0;i < MGPihagiDefine.PLAYER_COUNT; ++i)
        {
            PlayerUI[i] = new PlayerInfoUI();
            PlayerUI[i].MyInfoObj = PlayerInfoRoot.Find(i + "/user_mine").gameObject;
            PlayerUI[i].MyName = PlayerUI[i].MyInfoObj.transform.Find("name/text").GetComponent<Text>();

            PlayerUI[i].PlayerInfoObj = PlayerInfoRoot.Find(i + "/normal").gameObject;
            PlayerUI[i].PlayerName = PlayerUI[i].PlayerInfoObj.transform.Find("name/text").GetComponent<Text>();

            Transform heartObj = PlayerUI[i].MyInfoObj.transform.Find("heart");
            PlayerUI[i].MyLifeObjs = new GameObject[MGPihagiDefine.PLAYER_MAX_LIFE_COUNT];
            Transform pheartObj = PlayerUI[i].PlayerInfoObj.transform.Find("heart");
            PlayerUI[i].PlayerLifeObjs = new GameObject[MGPihagiDefine.PLAYER_MAX_LIFE_COUNT];

            string formatString = string.Empty;
            for(int j=0; j< MGPihagiDefine.PLAYER_MAX_LIFE_COUNT; ++j)
            {
                formatString = (j + 1).ToString("D2");

                PlayerUI[i].MyLifeObjs[j] = heartObj.Find(formatString).gameObject;
                PlayerUI[i].PlayerLifeObjs[j] = pheartObj.Find(formatString).gameObject;

                PlayerUI[i].MyLifeObjs[j].SetActive(true);
                PlayerUI[i].PlayerLifeObjs[j].SetActive(true);
            }
        }

        GameInfoRoot = transform.Find("gameInfo");
        //BtnGroupRoot = transform.Find("obj_metaverse_ButtonGroup/JumpButton");
        //ButtonGroupBox = BtnGroupRoot.GetComponent<RectTransform>();
        //BtnGroupRootOrgYPos = ButtonGroupBox.localPosition.y;
        //BtnGroupRootOrgYPos = ButtonGroupBox.anchoredPosition.y;
        //GameObject interactionObj = BtnGroupRoot.Find("btn_function").gameObject;
        //interactionObj.SetActive(false);
        JoyStickRoot = transform.Find("btn/Joystick");
        JoyStickBox = JoyStickRoot.GetComponent<RectTransform>();
        //JoyStickOrgYPos = JoyStickBox.localPosition.y;
        JoyStickOrgYPos = JoyStickBox.anchoredPosition.y;

        StartInfoRoot = transform.Find("popup_minigame_start");
        StartInfoText = new Text[3];
        for(int i=0;i < StartInfoText.Length; ++i)
        {
            StartInfoText[i] = StartInfoRoot.Find("info/title/text0" + (i + 1)).GetComponent<Text>();
        }
        StartInfoStageText = StartInfoRoot.Find("info/stage/stage_num").GetComponent<Text>();

		FinishRoot = transform.Find("popup_minigame_finish");

        CountDownRoot = transform.Find("popup_minigame_count");
        CountDownNumText = CountDownRoot.Find("count/text").GetComponent<Text>();

        StartRoot = transform.Find("fu_minigame01_start_img");
        StartAnim = StartRoot.GetComponent<Animation>();


        //======== pop_message ================
        PopupMsgRoot = transform.Find("pop_message").gameObject;
        PopupMsg_TieProcing = PopupMsgRoot.transform.Find("ani_dog_run").gameObject;
        PopupMsg_TieProc = PopupMsgRoot.transform.Find("alert_msg01").gameObject;
        PopupMsg_DrawTxt = PopupMsg_TieProc.transform.Find("text").GetComponent<Text>();
        PopupMsg_DrawTxt.text = CResourceManager.Instance.GetString(strID[5]);
        PopupMsg_SubFinalistTxt = PopupMsg_TieProc.transform.Find("text02").GetComponent<Text>();
        PopupMsg_SubFinalistTxt.text = CResourceManager.Instance.GetString(strID[6]);
        PopupMsg_Finalist = PopupMsgRoot.transform.Find("alert_msg02").gameObject;
        PopupMsg_FinalistTxt = PopupMsg_Finalist.transform.Find("text").GetComponent<Text>();
        PopupMsg_FinalistTxt.text = CResourceManager.Instance.GetString(strID[7]);
        SetActivePopupMsgRoot(false);
    }

    private void SetUIButton()
    {
        BtnClose = transform.Find("btn_close").GetComponent<Button>();
        BtnClose.BindToOnClick(_ =>
        {
            if (TutorialManager.Instance.CurrentTutorialRandomID == 0)
            {
                WorldMgr.ShowQuitAlert();
            }
            //NavigationManager.Goto<NavigationData>(SceneID.LOBBY_SCENE);
            return Observable.Timer(TimeSpan.FromSeconds(CDefines.BUTTONDELAYTIME)).AsUnitObservable();
        })
        .AddTo(this);
    }

    public void SetShowTitleStep(SHOW_TITLE_STEP step)
    {
        switch (step)
        {
            case SHOW_TITLE_STEP.TITLE:
                SoundManager.Instance.PlayEffect( 6830029 ); // list 121-1
                CountDownRoot.gameObject.SetActive(false);
                StartInfoRoot.gameObject.SetActive(true);
				FinishRoot.gameObject.SetActive(false);
                StartInfoStageText.gameObject.SetActive(false);
                StartRoot.gameObject.SetActive(false);
                string strStageType = string.Empty;
                PihagiGameStageType type = MGPihagiServerDataManager.Instance.StageType;
                if(type == PihagiGameStageType.TRYOUT)
                {                    
                    strStageType = CResourceManager.Instance.GetString(strID[0]);//예선전
                }
                else if(type == PihagiGameStageType.FINAL)
                {                    
                    strStageType = CResourceManager.Instance.GetString(strID[1]);//결승전
                }
                for(int i=0; i< StartInfoText.Length; ++i)
                {
                    StartInfoText[i].text = strStageType;
                    StartInfoText[i].gameObject.SetActive(true);
                }
                break;
            case SHOW_TITLE_STEP.INFO:
                string infoTxt = string.Empty;
                PihagiGameStageType _type = MGPihagiServerDataManager.Instance.StageType;
                if (_type == PihagiGameStageType.TRYOUT)
                {                    
                    infoTxt = CResourceManager.Instance.GetString(strID[2]);//예선전용
                }
                else if (_type == PihagiGameStageType.FINAL)
                {                    
                    infoTxt = CResourceManager.Instance.GetString(strID[3]);//결승전용
                }
                StartInfoStageText.text = infoTxt;
                StartInfoStageText.gameObject.SetActive(true);
                break;
            case SHOW_TITLE_STEP.COUNTDOWN:
                SetActiveUI(false);
                StartInfoRoot.gameObject.SetActive(false);
				FinishRoot.gameObject.SetActive(false);
                StartRoot.gameObject.SetActive(false);
                CountDownRoot.gameObject.SetActive(true);
                StartTime = MGPihagiDefine.COUNTDOWN_MAX;
                CurCountDownTime = MGPihagiDefine.COUNTDOWN_MAX;
                bStartTimer = true;
                break;
            case SHOW_TITLE_STEP.START:
                bStartTimer = false;
                SetActiveUI(true);                
                CountDownRoot.gameObject.SetActive(false);
                StartInfoRoot.gameObject.SetActive(false);
				FinishRoot.gameObject.SetActive(false);
                StartRoot.gameObject.SetActive(true);
                PlayStartAnim();
				//turn on joystick to move
				WorldMgr.SetMyPlayerJoyStickStop(false);
                break;
        }
    }


    public void PlayStartAnim()
    {
        if (bPlaiedStartAnim) return;
        if (StartAnim != null)
        {
            StartAnim.Stop();
            StartAnim.Play(MGPihagiDefine.ANIM_UI_START);
            SoundManager.Instance.PlayEffect( 6830019 );//list 118
            bPlaiedStartAnim = true;
        }
    }

    public void SetActiveUI(bool bActive)
    {
        BtnClose.gameObject.SetActive(bActive);
        PlayerInfoRoot.gameObject.SetActive(bActive);
        GameInfoRoot.gameObject.SetActive(bActive);
        SetActiveJoyStickGroupUI(bActive);
        //WorldMgr.SetActiveNameTag(bActive);
        WorldMgr.SetActiveNameTag(bActive);
    }

	public void SetActiveJoyStickGroupUI(bool bActive)
	{
        //BtnGroupRoot.gameObject.SetActive(bActive);
        //BtnGroupRoot.gameObject.SetActive(false);
        JoyStickRoot.gameObject.SetActive(bActive);
	}

    public void SetActivePopupMsgRoot(bool bActive)
    {
        PopupMsgRoot.SetActive(bActive);
    }

    public void SetActiveTieProc(bool bActive)
    {
        SetActivePopupMsgRoot(bActive);
        PopupMsg_TieProcing.SetActive(bActive);
        PopupMsg_TieProc.SetActive(bActive);
        PopupMsg_DrawTxt.gameObject.SetActive(bActive);

        PopupMsg_SubFinalistTxt.gameObject.SetActive(!bActive);
        PopupMsg_Finalist.SetActive(!bActive);
    }

    public void SetActiveFinallist(bool bActive)
    {
        SetActivePopupMsgRoot(bActive);
        PopupMsg_TieProcing.SetActive(!bActive);
        PopupMsg_TieProc.SetActive(bActive);
        PopupMsg_DrawTxt.gameObject.SetActive(!bActive);
        PopupMsg_SubFinalistTxt.gameObject.SetActive(bActive);
        PopupMsg_Finalist.SetActive(bActive);

        //set finalist name
        //        
        List<int> _list = MGPihagiServerDataManager.Instance.GetWinnerList();

        string[] _finalistName = new string[MGPihagiDefine.FINALIST_RANK];
        for (int i = 0; i < MGPihagiDefine.FINALIST_RANK; ++i)
        {
            MGPihagiPlayerInfo _info = MGPihagiServerDataManager.Instance.GetGamePlayerInfo(_list[i]);
            _finalistName[i] = _info.NickName;
        }

        PopupMsg_FinalistTxt.text = _finalistName[0] + ", " + _finalistName[1];
    }


    public void ShowTimerUI()
    {
        string _streamKey = MGPihagiDefine.BPW_MINIGAME_PIHAGI_TIMESTREAMKEY;

        if (timerDisposer != null)
        {
            timerDisposer.Dispose();
        }
        timerDisposer = new SingleAssignmentDisposable();

        if (GlobalTimer.Instance.HasTimeStream(_streamKey))
        {
            GlobalTimer.Instance.GetTimeStream(out PlayTimeStream, _streamKey);
            if (PlayTimeStream != null)
            {
                timerDisposer.Disposable =
                    PlayTimeStream.OnTimeStreamObservable().Subscribe(UpdateRemainTime).AddTo(this);
            }
            else
            {
                CDebug.Log("Do not exist timestream!!");
            }
        }
        else
        {
            //SetActiveTimerObj(false);
        }
    }

	public int GetCurrentGameTime()
	{
		return (int)remainTimeSeconds;
	}

    private void Update()
    {
        if(bStartTimer)
        {
            if (CurCountDownTime >= 1)
            {
                StartTime -= Time.deltaTime;
                CurCountDownTime = Mathf.FloorToInt(StartTime % 60);

                if (CurCountDownTime>0 && prevTimeSeconds != CurCountDownTime)
                {
                    CDebug.Log( $"              [PIHAGI SOUND] CountDown {CurCountDownTime}" );
                    SoundManager.Instance.PlayEffect( 6830023 );// list 123
                }

                //CDebug.Log("PageUI update() CurCountDownTime = " + CurCountDownTime);
                if (CurCountDownTime > 0)
                {
                    CountDownNumText.text = CurCountDownTime.ToString();
                }

                prevTimeSeconds = CurCountDownTime;
            }
            //else
            //{
            //    SetShowTitleStep(SHOW_TITLE_STEP.START);
            //}
        }
    }

    private void UpdateRemainTime(TimeStreamData timeStreamData)
    {
        if (timeStreamData.IsEnd == true)
        {
            RemoveTimeStream();
            return;
        }

        remainTimeSeconds = timeStreamData.CurrentTime;


        if (remainTimeSeconds >= 0)
        {
            SetTimerUIText(remainTimeSeconds);
        }

        //else if (remainTimeSeconds == 0)
        //{
        //    //PlayStartAnim();
        //}

        //if (remainTimeSeconds < 1)
        //{
            // 시간 종료 상태로 변경해줘야 함(버튼 등)
            //SetActiveTimerObj(false);
            //SetActiveDirectionButtons(false);
            //SetActiveButtonGroup(false);
            //SetHideAllDialSlot();
        //}
    }

	public void RemoveTimeStream()
	{
		if(timerDisposer != null) timerDisposer.Dispose();

        if(PlayTimeStream != null) PlayTimeStream.Remove();
	}


    public void SetTimerUIText(float time)
    {
        TimerTxt.text = time.ToString();
    }

    public void SetActiveTimerObj(bool bActive)
    {
        TimerObj.SetActive(bActive);
    }

    private void CreatePoolParentObject(Transform pageUITransform)
    {
        poolParentObject = new GameObject(MGPihagiWorldManager.NAME_POOL_PARENT_OBJECT);
        poolParentObject.transform.SetParent(pageUITransform);
        poolParentObject.transform.localScale = Vector3.one;
        poolParentObject.transform.SetAsLastSibling();

        nameTagPoolParentObject = new GameObject(MGPihagiWorldManager.NAME_POOL_NAME_TAG);
        nameTagPoolParentObject.transform.SetParent(poolParentObject.transform);
        nameTagPoolParentObject.transform.localScale = Vector3.one;
    }

 //   public void SetupNameTag()
 //   {
 //       MGPihagiCanvasManager canvasMgr = WorldMgr.GetCanvasMgr();
 //       RectTransform canvasRect = canvasMgr.GetComponent<RectTransform>();
 //       BPWorldLayerObject layerObj = canvasMgr.LayerObjects;
 //       IATCreator.Intialize(gameObject, canvasRect, WorldMgr.PlayerMgr.NameTagPoolMgr, WorldMgr.MainCameraObj);
 //       IATCreator.SetUpLayerObject(layerObj);
 //   }

	//public void AddNameTag(int pid, string name, GameObject charObj)
	//{
	//	GameObject dummyObj = WorldMgr.PlayerMgr.GetPlayerDummyBone(pid);
	//	bool addTop = false;
	//	bool isMyPlayer = false;

	//	if( charObj == null || dummyObj == null) return;

	//	if( WorldMgr.PlayerMgr.GetPlayerUIDByPID(pid) == WorldMgr.PlayerMgr.MyPlayerUID )
	//	{
	//		isMyPlayer =true;
	//		addTop = true;
	//	}

	//	UI_BPWorldNameTag nameTag = IATCreator.AddNameTag( name, isMyPlayer, charObj, dummyObj, addTop );
	//	nameTag.transform.localScale = Vector3.one;
	//}

    /// <summary>
    /// NavigationManager.BackTo()호출로 되돌아올때 호출된다.
    /// </summary>
    public override void OnFromBackTo()
    {
        Initialize();
    }

    #region Button_Click
    private void OnButtonClickBack()
    {
        NavigationManager.BackTo();
    }


    public override void CallByOnBackPress()
    {
        // NavigationManager.BackTo() 를 호출하면  여기가 자동호출 된다. 

    }

    public override IObservable<bool> OnBackPress()
    {
        // OnClickBack();
        return Observable.Return<bool>(true);
    }


    private void OnClickMotionSlot(int motionSlotIndex)
    {
        RequestPlayMotion(motionSlotIndex - 1);
    }

    private void OnClickEmoticonSlot(int emoticonSlotIndex)
    {
        //이모티콘
        ShowEmoticon(emoticonSlotIndex - 1);
    }
    #endregion Button_Click


    #region TCP_REQ
    private void RequestPlayMotion(int slotIndex)
    {
        Animator animator = StaticAvatarManager.GetAvatarAnimator(MyAvatarType);

        if (animator == null)
        {
            return;
        }
        CDebug.Log($" Pihagi RequestPlayMotion slotIndex:{slotIndex}");
        TCPMGPihagiRequestManager.Instance.Req_GameMotionPerform(slotIndex);
    }


    public void AddEmoticon(int pid, long emoticonID)
    {
        MGPihagiPlayersManager _playersMgr = WorldMgr.PlayerMgr;
        GameObject uiDummuBone = _playersMgr.GetPlayerDummyBone(pid);

        //IATCreator.AddEmoticon(emoticonID, uiDummuBone.transform);
    }

    private void ShowEmoticon(int slotIndex)
    {
        //이모티콘 출력
        var emoditonId = BPWorldServerDataManager.Instance.ExpressionManager.GetEquipedEmoticonID(MyAvatarType, slotIndex);
        if (emoditonId != 0)
        {
            int myPid = WorldMgr.PlayerMgr.MYPID;
            WorldMgr.AddEmoticon(myPid, emoditonId);
            TCPMGPihagiRequestManager.Instance.Req_GameEmoticonPerform(slotIndex);
        }
    }
    #endregion TCP_REQ


    #region TUTORIAL
    public RectTransform GetPlayerBox()
    {
        return PlayerBox;
    }

    public RectTransform GetJoyStickBox()
    {
        return JoyStickBox;
    }

    //public RectTransform GetButtonGrpBox()
    //{
    //    return ButtonGroupBox;
    //}

    public void InitBottomUITutorialPos()
    {
        //Vector3 pos = ButtonGroupBox.localPosition;
        //pos.y -= 500;
        //ButtonGroupBox.localPosition = pos;
        //BtnGroupRootTutorialYPos = ButtonGroupBox.localPosition.y;

        //pos = JoyStickBox.localPosition;
        //pos.y -= 500;
        //JoyStickBox.localPosition = pos;
        //JoyStickTutorialYPos = JoyStickBox.localPosition.y;



        //Vector2 apos = ButtonGroupBox.anchoredPosition;
        //apos.y -= 500;
        //ButtonGroupBox.anchoredPosition = apos;
        ////BtnGroupRootTutorialYPos = ButtonGroupBox.anchoredPosition.y;

        Vector2 apos = JoyStickBox.anchoredPosition;
        apos.y -= 500;
        JoyStickBox.anchoredPosition = apos;
        JoyStickTutorialYPos = JoyStickBox.anchoredPosition.y;
    }

    public void AppearBottomUITutorialPos()
    {
        DOTween.To(() => JoyStickTutorialYPos,
            changevalue =>
            {
                SetUIJoyStickPos(changevalue);
            },
            JoyStickOrgYPos, 0.5f
            )
            .OnComplete( () =>
            {
                WorldMgr.SetTutorial_Step4();
            } );

        //DOTween.To(() => BtnGroupRootTutorialYPos,
        //    changevalue =>
        //    {
        //        SetUIBtnGroupPos(changevalue);
        //    },
        //    BtnGroupRootOrgYPos, 0.5f
        //    )
        //    .OnComplete(() =>
        //    {
        //        WorldMgr.SetTutorial_Step4();
        //    });
    }


    //private void SetUIBtnGroupPos(float yPos)
    //{
    //    //Vector3 curPosition = new Vector3(ButtonGroupBox.localPosition.x, yPos, ButtonGroupBox.localPosition.z);
    //    //ButtonGroupBox.localPosition = curPosition;
    //    Vector2 curPosition = new Vector2(ButtonGroupBox.anchoredPosition.x, yPos);
    //    ButtonGroupBox.anchoredPosition = curPosition;
    //}

    private void SetUIJoyStickPos(float yPos)
    {
        //Vector3 curPosition = new Vector3(JoyStickBox.localPosition.x, yPos, JoyStickBox.localPosition.z);
        //JoyStickBox.localPosition = curPosition;
        Vector2 curPosition = new Vector2(JoyStickBox.anchoredPosition.x, yPos);
        JoyStickBox.anchoredPosition = curPosition;

    }
    #endregion TUTORIAL


    public void CleanUpPageUI()
    {
        //if (PlayerUI != null)
        //{
        //    for (int i = 0; i < PlayerUI.Length; ++i)
        //    {
        //        PlayerUI[i].MyLifeObjs = null;
        //        PlayerUI[i].PlayerLifeObjs = null;
        //        PlayerUI[i] = null;
        //    }
        //}

        //if (StartInfoText != null) StartInfoText = null;
        if (timerDisposer != null)
        {
            timerDisposer.Dispose();
            timerDisposer = null;
        }
        if (PlayTimeStream != null) PlayTimeStream = null;

        WorldMgr.ClearObjects();
    }

    public override void Release()
    { 
		
	}
}
