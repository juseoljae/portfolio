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
using Unity.Linq;
using UniRx;
using UniRx.Triggers;
using System.Collections.Generic;
using ui.navigation;
using UnityEngine.EventSystems;


public class MGNunchiPageUI : APage
{
    public const uint LayerOrder = 0;

    private CPopupService popupService;

    private MGNunchiWorldManager WorldMgr;

    private MGNunchiPlayer MyPlayer;

    public BPWorldIconAndTagCreator IATCreator;

    //private UI_BPWorldExpressionButtons_NEW expressionButtons = null;

    private MGNunchiTutorial NunchiTutorial = null;

	private string[] directionTxt = new string[] { "up", "down", "left", "right" };

    private long[] strID = new long[]
    {
        9171004,  //0 {0}라운드
        9171001,  //1 예선전
        9171003,  //2 결승전
        9171002,  //3 {0}위

        9171010,  //4 게임을 중단하고 나가시겠습니까?
        9171011,  //5 게임종료
        
        927003,	  //6 블핑월드 미니게임1	동점으로 인한 결승전 진출자를 추첨합니다.
        9171017,  //7 다음 라운드 진출자 : 
        9171013,  //8 진출자 명단 : {0}, { 1}\n진출: 
    };

    private GameObject poolParentObject;
    public GameObject nameTagPoolParentObject;

    #region UI
    private Button BtnClose;
    //private GameObject StartObj;
    //private GameObject RoundObj;
    //private Text RoundCountTxt;
    private GameObject TimerObj;
    private Text TimerTxt;


    public class InfoText
    {
        public Text NameTxt;
        public Text ScoreTxt;
    }

    public class PlayerInfoUI
    {
        public InfoText MyText;
        public GameObject MyInfoObj;
        public InfoText PlayerText;
        public GameObject PlayerInfoObj;
        public Image Thumnail;
        public Image BG;
        public GameObject Badge_Invincible;
        public GameObject Badge_DoubleItem;
        public GameObject Badge_Steal;
    }
    private PlayerInfoUI[] PlayerUI;
    //public Dictionary<long, PlayerInfoUI> PlayerInfoUIDic; 
    Transform PlayerInfoRoot;
    Transform GameInfoRoot;
    Transform FXStartObj;
    Dictionary<int, GameObject> FXRoundTextObjDic = new Dictionary<int, GameObject>();
    GameObject[] FXRoundTextObj;

    //Score by each players(get score ui)
    public class GainItemInfo
    {
        public GameObject ItemObj; //idx : player seat index
        public Image IconImg;
        public Text ScoreTxt;
    }
    private GainItemInfo[] ScoreObj;

    //Controller
    private Transform ControllerRootObj;
#if SHOW_MINIGAME_BTN
    private ButtonInfo[] ControllerBtns;
    private SingleAssignmentDisposable[] ControllerBtnDisposer;
#endif
    private Transform ButtonGroupRootObj;

    public class ButtonInfo
    {
        public int ID;
        public Button Btn;
        public GameObject NorObj;
        public GameObject DisObj;
        public GameObject PressObj;
    }

    public class RoundInfo //상단에 oooo
    {
        public GameObject RoundInfObj;//01
        public GameObject NormalObj;
        public GameObject SelectObj;
        public GameObject DimObj;
    }


    public RoundInfo[] RoundInfoObjs;

    public GameObject StageStartRootObj;
    public GameObject StageTextRootObj;//parent of below StageTextObj
    public Text StageTextObj;
    private GameObject subTitleObj;
    private Text subTitle;
    public Text StartRoundTextObj;

    public GameObject PopupMsgRoot;
    public GameObject PopupMsg_TieProcing;

    public GameObject PopupMsg_TieProc;
    public Text PopupMsg_DrawTxt;
    public Text PopupMsg_SubFinalistTxt;
    public GameObject PopupMsg_Finalist;
    public Text PopupMsg_FinalistTxt;

    private MiniGameTooltip ItemToolTip;
    //private UI_BPWorldNameTag NameTag;

    //Tutorial
    private GameObject TutorialRoot;
    private RectTransform Mid_arrBox;
    private RectTransform PlayerBox;
    #endregion UI

    private Animation TimerCountAnim;
    private Animation StartAnim;
    public bool bPlaiedStartAnim;

    private SingleAssignmentDisposable timerDisposer;
    private TimeStream PlayTimeStream;
    private float remainTimeSeconds;
    private float prevTimeSeconds;

    private const string NAME_POOL_PARENT_OBJECT = "POOL_MONOOBJECTS(Dynamic Create)";
    private const string NAME_POOL_NAME_TAG = "NAME TAG POOL(Dynamic Create)";

    

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

    public override void Initialize()
    {
    }

    public void InitMgNunchiPageUI(MGNunchiWorldManager worldMgr)
    {
        WorldMgr = worldMgr;

        IATCreator = new BPWorldIconAndTagCreator();
        //expressionButtons = new UI_BPWorldExpressionButtons_NEW();
        //timerDisposer = new SingleAssignmentDisposable();

        //PlayerInfoUIDic = new Dictionary<long, PlayerInfoUI>();
        PlayerUI = new PlayerInfoUI[WorldMgr.PLAYER_COUNT];


#if SHOW_MINIGAME_BTN
        ControllerBtns = new ButtonInfo[MGNunchiDefines.DIRECTION_BUTTON_LENGTH];//U, D, L, R
        ControllerBtnDisposer = new SingleAssignmentDisposable[MGNunchiDefines.DIRECTION_BUTTON_LENGTH];
#endif
        ScoreObj = new GainItemInfo[WorldMgr.PLAYER_COUNT];

        RoundInfoObjs = new RoundInfo[MGNunchiDefines.MAX_ROUND];//UI가 5개뿐

        //StageTextObj = new Text[1];//Shadow 2개, 메인 1개

        bPlaiedStartAnim = false;

        SetUIObjects();

        SetUIButton();

        //CreatePoolParentObject(transform);

        //SetupNameTag();
    }


	private void SetUIObjects()
    {
        TimerObj = transform.Find("gameInfo/count").gameObject;
        TimerTxt = TimerObj.transform.Find("text/text_count").GetComponent<Text>();

        //Anim
        GameInfoRoot = transform.Find("gameInfo");
        TimerCountAnim = GameInfoRoot.GetComponent<Animation>();
        FXStartObj = transform.Find("fu_minigame01_start_img");
        //FXRoundTextObj = new GameObject[5];
        
        string path = "start_text_img/ROUND_Text/{0:00}_Text";
        for (int i=1; i< 6; ++i)
        {
            string eachPath = string.Format(path, i);
            //FXRoundTextObj[i] = FXStartObj.Find(eachPath).gameObject;
            GameObject obj = FXStartObj.Find(eachPath).gameObject;
            FXRoundTextObjDic.Add(i, obj);
        }
        StartAnim = FXStartObj.GetComponent<Animation>();
        

        PlayerInfoRoot = transform.Find("playerInfo");
        for (int i = 0; i < PlayerUI.Length; ++i)
        {
            PlayerUI[i] = new PlayerInfoUI();
            PlayerUI[i].MyText = new InfoText();
            PlayerUI[i].PlayerText = new InfoText();

            PlayerUI[i].MyInfoObj = PlayerInfoRoot.Find(i + "/user_mine").gameObject;
            PlayerUI[i].MyText.NameTxt = PlayerUI[i].MyInfoObj.transform.Find("name/text").GetComponent<Text>();
            PlayerUI[i].MyText.ScoreTxt = PlayerUI[i].MyInfoObj.transform.Find("score/text/text_score").GetComponent<Text>();
            PlayerUI[i].MyInfoObj.SetActive(true);

            PlayerUI[i].PlayerInfoObj = PlayerInfoRoot.Find(i + "/normal").gameObject;
            PlayerUI[i].PlayerText.NameTxt = PlayerUI[i].PlayerInfoObj.transform.Find("name/text").GetComponent<Text>();
            PlayerUI[i].PlayerText.ScoreTxt = PlayerUI[i].PlayerInfoObj.transform.Find("score/text/text_score").GetComponent<Text>();
            PlayerUI[i].PlayerInfoObj.SetActive(true);

            PlayerUI[i].Badge_Invincible = PlayerInfoRoot.Find(i + "/badge/state/item_invincible").gameObject;
            PlayerUI[i].Badge_DoubleItem = PlayerInfoRoot.Find(i + "/badge/state/item_x2").gameObject;
            PlayerUI[i].Badge_Steal = PlayerInfoRoot.Find(i + "/badge/state/item_steal").gameObject;
            SetActiveBadgeObjByItemType(i, BPWPacketDefine.NunchiGameItemType.NONE);
        }

        SetActiveTimerObj(false);

        Transform roundRootObj = transform.Find("gameInfo/round");
        for (int i = 0; i < RoundInfoObjs.Length; ++i)
        {
            RoundInfoObjs[i] = new RoundInfo();
            RoundInfoObjs[i].RoundInfObj = roundRootObj.Find(string.Format("{0:D2}", (i + 1))).gameObject;
            RoundInfoObjs[i].NormalObj = RoundInfoObjs[i].RoundInfObj.transform.Find("state/normal").gameObject;
            RoundInfoObjs[i].SelectObj = RoundInfoObjs[i].RoundInfObj.transform.Find("state/select").gameObject;
            RoundInfoObjs[i].DimObj = RoundInfoObjs[i].RoundInfObj.transform.Find("state/dim").gameObject;

            RoundInfoObjs[i].NormalObj.SetActive(false);
            RoundInfoObjs[i].SelectObj.SetActive(false);
            RoundInfoObjs[i].DimObj.SetActive(false);
            RoundInfoObjs[i].RoundInfObj.SetActive(false);
        }

        StageStartRootObj = transform.Find("popup_minigame_start").gameObject;
        StageTextRootObj = transform.Find("title_text").gameObject;
        StageTextObj = StageTextRootObj.transform.Find("text").GetComponent<Text>();
        MakeSubTitleText();
        //for (int i = 0; i < StageTextObj.Length; ++i)
        //{
        //    StageTextObj[i] = StageTextRootObj.transform.Find("text0" + (i + 1)).GetComponent<Text>();
        //}
        StartRoundTextObj = StageStartRootObj.transform.Find("info/stage/stage_num").GetComponent<Text>();
        SetActiveStageStartObj(false);//ok

        PopupMsgRoot = transform.Find("pop_message").gameObject;
        PopupMsg_TieProcing = PopupMsgRoot.transform.Find("ani_dog_run").gameObject;
        PopupMsg_TieProc = PopupMsgRoot.transform.Find("alert_msg01").gameObject;
        PopupMsg_DrawTxt = PopupMsg_TieProc.transform.Find("text").GetComponent<Text>();
        PopupMsg_DrawTxt.text = CResourceManager.Instance.GetString(strID[6]);
        PopupMsg_SubFinalistTxt = PopupMsg_TieProc.transform.Find("text02").GetComponent<Text>();
        PopupMsg_SubFinalistTxt.text = CResourceManager.Instance.GetString(strID[7]);
        PopupMsg_Finalist = PopupMsgRoot.transform.Find("alert_msg02").gameObject;
        PopupMsg_FinalistTxt = PopupMsg_Finalist.transform.Find("text").GetComponent<Text>();
        PopupMsg_FinalistTxt.text = CResourceManager.Instance.GetString(strID[8]);
        SetActivePopupMsgRoot(false);

        if (ItemToolTip == null)
        {
            ItemToolTip = this.transform.Find("tooltip_common_item")?.gameObject.AddComponent<MiniGameTooltip>();
        }

        //if (NameTag == null)
        //{
        //    GameObject nameTagObj = this.transform.Find("obj_metaverse_username").gameObject;
        //    NameTag = nameTagObj.GetComponent<UI_BPWorldNameTag>();
        //    NameTag.Initialize();
        //}

        //tutorial
        TutorialRoot = transform.Find("Tutorial").gameObject;
        Mid_arrBox = TutorialRoot.transform.Find("arr_box").GetComponent<RectTransform>();
        PlayerBox = TutorialRoot.transform.Find("player_box").GetComponent<RectTransform>();
        SetActiveTutorialRoot(false);
	}

	//private void CreatePoolParentObject(Transform pageUITransform)
	//{
	//    poolParentObject = new GameObject(NAME_POOL_PARENT_OBJECT);
	//    poolParentObject.transform.SetParent(pageUITransform);
	//    poolParentObject.transform.localScale = Vector3.one;
	//    poolParentObject.transform.SetAsLastSibling();

	//    nameTagPoolParentObject = new GameObject(NAME_POOL_NAME_TAG);
	//    nameTagPoolParentObject.transform.SetParent(poolParentObject.transform);
	//    nameTagPoolParentObject.transform.localScale = Vector3.one;
	//}

	//public void SetupNameTag()
	//{
	//    MGNunchiCanvasManager canvasMgr = (MGNunchiCanvasManager)MGNunchiManager.Instance.GetCanvasManager();
	//    var canvasRect = canvasMgr.GetComponent<RectTransform>();
	//    BPWorldLayerObject layerObj = canvasMgr.LayerObjects;
	//    IATCreator.Intialize(gameObject, canvasRect, WorldMgr.PlayerMgr.NameTagPoolMgr, WorldMgr.MainCamera);
	//    IATCreator.SetUpLayerObject(layerObj);


	//}

	public void SetUp()
    {
		//SetupExpressionButtons( WorldMgr.PlayerMgr.GetMyPlayer().PlayerInfo.AvatarType );
        //SetStageText();
        SetTimerUIText(WorldMgr.GetRoundTime());
        SetActiveTimerObj(true);
        SetRoundInfoUI();
        SetActivePageUI(true);
    }

    public void SetRadialButtons()
    {
        SetupExpressionButtons(WorldMgr.PlayerMgr.GetMyPlayer().PlayerInfo.AvatarType);
    }

    public void SetActivePageUI(bool bActive)
    {
        gameObject.SetActive(bActive);
    }

    //public void AddNameTag(long uid, string name, GameObject charObj)
    //{
    //    GameObject dummyObj = WorldMgr.PlayerMgr.GetPlayerDummyBone(uid);
    //    bool addTop = false;
    //    bool isMyPlayer = false;

    //    if (charObj == null || dummyObj == null) return;

    //    if (CPlayer.GetStatusLoginValue(PLAYER_STATUS.USER_ID) == uid.ToString())
    //    {
    //        isMyPlayer = true;
    //        addTop = true;
    //    }

    //    IATCreator.AddNameTag(name, isMyPlayer, charObj, dummyObj, addTop);
    //}


    public void SetStageText()
    {
        TournamentInfo _info = MGNunchiServerDataManager.Instance.GetTournamentGameRoomInfo();
        string title = "";

        CDebug.Log($"        ##### SetStageText() stage = {_info.StageType}");

        if(_info.StageType == BPWPacketDefine.NunchiGameStageType.TRYOUT)
        {
            title = CResourceManager.Instance.GetString(strID[1]);
            subTitleObj.SetActive( true );
        }
        else if(_info.StageType == BPWPacketDefine.NunchiGameStageType.FINAL)
        {
            title = CResourceManager.Instance.GetString(strID[2]);
            subTitleObj.SetActive( false );
        }

        StageTextObj.text = title;

        SetRoundText();

        //SetActiveStageStartObj(true); 
        SoundManager.Instance.PlayEffect( 6830029 ); // list 121-1
        SetActiveStageTextObj(true);
        SetActiveRoundTextObj(false);
    }

    public void SetRoundText()
    {
        int round = WorldMgr.GetRoundCount();
        //StartRoundTextObj.text = string.Format(CResourceManager.Instance.GetString(strID[0]), round);
        FXStartObj.gameObject.SetActive(true);
        SetHideAllFXRoundObj();
        if (FXRoundTextObjDic.ContainsKey(round))
        {
            FXRoundTextObjDic[round].SetActive(true);
        }
        PlayStartAnim();
    }

    public float GetFXStartAnimLength()
    {
        if(StartAnim == null)
        {
            StartAnim = FXStartObj.GetComponent<Animation>();
        }
        return StartAnim.clip.length;
    }

    private void SetHideAllFXRoundObj()
    {
        foreach(GameObject obj in FXRoundTextObjDic.Values)
        {
            obj.SetActive(false);
        }
    }

    public void SetPlayerInfoUI(MGNunchiPlayerInfo pInfo, int idx)
    {
        SetPlayerScore(pInfo, idx);

        //UI set
        if (WorldMgr.PlayerMgr.MyPlayerPID == pInfo.PID)
        {
            PlayerUI[idx].MyInfoObj.SetActive(true);
            PlayerUI[idx].MyText.NameTxt.gameObject.SetActive(true);
            PlayerUI[idx].MyText.ScoreTxt.gameObject.SetActive(true);

            PlayerUI[idx].PlayerInfoObj.SetActive(false);
            PlayerUI[idx].PlayerText.NameTxt.gameObject.SetActive(false);
            PlayerUI[idx].PlayerText.ScoreTxt.gameObject.SetActive(false);
        }
        else
        {
            PlayerUI[idx].MyInfoObj.SetActive(false);
            PlayerUI[idx].MyText.NameTxt.gameObject.SetActive(false);
            PlayerUI[idx].MyText.ScoreTxt.gameObject.SetActive(false);

            PlayerUI[idx].PlayerInfoObj.SetActive(true);
            PlayerUI[idx].PlayerText.NameTxt.gameObject.SetActive(true);
            PlayerUI[idx].PlayerText.ScoreTxt.gameObject.SetActive(true);
        }
    }

    public void SetPlayerScore(MGNunchiPlayerInfo pInfo, int idx)
    {
        if(pInfo == null) return;

        
        CDebug.Log($"SetPlayerScore idx = {idx}, length = {PlayerUI.Length}");


        string _name = pInfo.NickName;
        string _score = pInfo.CoinScore.ToString();

#if UNITY_EDITOR
        if (idx >= PlayerUI.Length-1)
        {
            CDebug.Log($"SetPlayerScore idx = {idx}, length = {PlayerUI.Length}");
        }
#endif

        //UI set
        if (WorldMgr.PlayerMgr.MyPlayerUID == pInfo.UID)
        {
            PlayerUI[idx].MyText.NameTxt.text = _name;
            PlayerUI[idx].MyText.ScoreTxt.text = _score;
        }
        else
        {
            PlayerUI[idx].PlayerText.NameTxt.text = _name;
            PlayerUI[idx].PlayerText.ScoreTxt.text = _score;
        }
    }

    //private void SetScoreGainItem(Transform playerObj, int idx)
    //{
    //    MGNunchiCanvasManager _canvas = (MGNunchiCanvasManager)MGNunchiManager.Instance.GetCanvasManager();
    //    var canvasRect = _canvas.GetComponent<RectTransform>();
    //    var resData = CResourceManager.Instance.GetResourceData(MGNunchiConstants.COINITEM_GAINUI_PATH);
    //    GameObject resObj = resData.Load<GameObject>(gameObject);

    //    ScoreObj[idx] = new GainItemInfo();
    //    ScoreObj[idx].ItemObj = Instantiate(resObj);
    //    Transform _scoreTxtObj = ScoreObj[idx].ItemObj.transform.Find("count/text");
    //    ScoreObj[idx].ScoreTxt = _scoreTxtObj.GetComponent<Text>();

    //    var fop = ScoreObj[idx].ItemObj.AddComponent<FollowObjectPositionFor2D>();
    //    fop.Init(canvasRect, playerObj, MGNunchiDefines.SCOREUI_HEIGHT);

    //    SetActiveScoreObj(idx, false);
    //}

    private void SetUIButton()
    {
        BtnClose = transform.Find("btn_close").GetComponent<Button>();

        BtnClose.BindToOnClick(_ =>
        {
            //튜토리얼 중에는 닫기 안눌리게 막는다
            if (TutorialManager.Instance.CurrentTutorialRandomID == 0)
            {
                WorldMgr.ShowQuitAlert();
            }

            return Observable.Timer(TimeSpan.FromSeconds(CDefines.BUTTONDELAYTIME)).AsUnitObservable();
        })
        .AddTo(this);
        SetActiveCloseButton( false );

        ///[deprecated]
        ControllerRootObj = transform.Find("btn/btn_play");
        //ButtonGroupRootObj = transform.Find("obj_metaverse_ButtonGroup");
    }

    public void SetActiveCloseButton(bool bActive)
    {
        BtnClose.gameObject.SetActive(bActive);
    }

    public void CleanUpPageUI()
    {
        MyPlayer = null;
#if SHOW_MINIGAME_BTN
        for (int i = 0; i < ControllerBtnDisposer.Length; ++i)
        {
            if (ControllerBtnDisposer[i] != null)
            {
                ControllerBtnDisposer[i].Dispose();
            }
        }
#endif
    }

#if SHOW_MINIGAME_BTN
    private void IniControllerBtnDisposer(int idx)
    {
        if (ControllerBtnDisposer[idx] != null)
        {
            ControllerBtnDisposer[idx].Dispose();
        }

        ControllerBtnDisposer[idx] = new SingleAssignmentDisposable();
    }

    public void SetControllerButtonsCallBack()
    {
        for (int i = 0; i < ControllerBtns.Length; ++i)
        {
            ControllerBtns[i] = new ButtonInfo();
            ControllerBtns[i].Btn = ControllerRootObj.Find(directionTxt[i]).GetComponent<Button>();
            ControllerBtns[i].NorObj = ControllerBtns[i].Btn.transform.Find("state/normal").gameObject;
            ControllerBtns[i].PressObj = ControllerBtns[i].Btn.transform.Find("state/press").gameObject;
            ControllerBtns[i].ID = i;
            ControllerBtns[i].PressObj.SetActive(false);
        }

        for (int i = 0; i < ControllerBtns.Length; ++i)
        {
            IniControllerBtnDisposer(i);

            ButtonInfo _info = ControllerBtns[i];

            CDebug.Log($"        $$$$$$$$$$$$$$$$          ControllerButtons Binding i = {i}, ID = {_info.ID}, Btn = {_info.Btn}");

            ControllerBtnDisposer[i].Disposable = ControllerBtns[i].Btn.BindToOnClick(_ =>
            {
                OnClickControllerBtn(_info.ID);
                return Observable.Timer(TimeSpan.FromSeconds(CDefines.BUTTONDELAYTIME)).AsUnitObservable();
            })
            .AddTo(this);
        }
    }

    public void CleanControllerButtons()
    {
        for (int i = 0; i < ControllerBtns.Length; ++i)
        {
            if (ControllerBtnDisposer[i] != null)
                ControllerBtnDisposer[i].Dispose();
            if (ControllerBtns[i] != null)
            {
                ControllerBtns[i].Btn = null;
                ControllerBtns[i].NorObj = null;
                ControllerBtns[i].PressObj = null;
                ControllerBtns[i].ID = i;
                ControllerBtns[i] = null;
            }
        }
    }
#endif

    public void SetRoundInfoUI()
    {
        int curRound = WorldMgr.GetRoundCount() - 1;
        int maxRound = WorldMgr.GetMaxRound();
        if(maxRound > MGNunchiDefines.MAX_ROUND || curRound < 0)
        {
            Debug.LogError($"SetRoundInfoUI(). max round{maxRound} is over {MGNunchiDefines.MAX_ROUND}. curRound = {curRound}");
            return;
        }

        for (int i = 0; i < maxRound; ++i)
        {
            if(i<curRound)
            {
                RoundInfoObjs[i].NormalObj.SetActive(true);
                RoundInfoObjs[i].SelectObj.SetActive(false);
                RoundInfoObjs[i].DimObj.SetActive(false);
            }
            else if(i == curRound)
            {
                RoundInfoObjs[i].NormalObj.SetActive(false);
                RoundInfoObjs[i].SelectObj.SetActive(true);
                RoundInfoObjs[i].DimObj.SetActive(false);
            }
            else if(i > curRound)
            {
                RoundInfoObjs[i].NormalObj.SetActive(false);
                RoundInfoObjs[i].SelectObj.SetActive(false);
                RoundInfoObjs[i].DimObj.SetActive(true);
            }
            RoundInfoObjs[i].RoundInfObj.SetActive(true);
        }
    }


    /// <summary>
    /// NavigationManager.BackTo()호출로 되돌아올때 호출된다.
    /// </summary>
    public override void OnFromBackTo()
    {
        Initialize();
    }

    public void SetPlayerBG(int idx, bool myPlayer)
    {
        if (myPlayer)
        {
            PlayerUI[idx].BG.color = new Color32(255, 139, 0, 118);
        }
        else
        {
            PlayerUI[idx].BG.color = new Color32(186, 186, 186, 118);
        }
    }

    public void SetActiveTimerObj(bool bActive)
    {
        TimerObj.SetActive(bActive);
    }

    public void SetActiveScoreObj(int idx, bool bActive)
    {
        ScoreObj[idx].ItemObj.SetActive(bActive);
    }

    //2D UI where upper on Avatar head
    //public void SetPlayerScoreTxt(int getScore, int idx)
    //{
    //    //ScoreTxt[idx].text = getScore.ToString();

    //    //MGNunchiCanvasManager _canvas = (MGNunchiCanvasManager)MGNunchiManager.Instance.GetCanvasManager();
    //    //var canvasRect = _canvas.GetComponent<RectTransform>();
    //    //var fop = ScoreObj[idx].ItemObj.AddComponent<FollowObjectPositionFor2D>();
    //    //fop.Init(canvasRect, playerObj, 1.5f);
    //    ScoreObj[idx].ScoreTxt.text = getScore.ToString();
    //    SetActiveScoreObj(idx, true);
    //}

    public void SetActiveBadgeObjByItemType(int idx, BPWPacketDefine.NunchiGameItemType type)
    {
        switch (type)
        {
            case BPWPacketDefine.NunchiGameItemType.NONE:
            case BPWPacketDefine.NunchiGameItemType.COIN:
                PlayerUI[idx].Badge_Invincible.SetActive(false);
                PlayerUI[idx].Badge_DoubleItem.SetActive(false);
                PlayerUI[idx].Badge_Steal.SetActive(false);
                break;
            case BPWPacketDefine.NunchiGameItemType.COIN_DOUBLE:
                PlayerUI[idx].Badge_Invincible.SetActive(false);
                PlayerUI[idx].Badge_DoubleItem.SetActive(true);
                PlayerUI[idx].Badge_Steal.SetActive(false);
                break;
            case BPWPacketDefine.NunchiGameItemType.INVINCIBLE:
                PlayerUI[idx].Badge_Invincible.SetActive(true);
                PlayerUI[idx].Badge_DoubleItem.SetActive(false);
                PlayerUI[idx].Badge_Steal.SetActive(false);
                break;
            case BPWPacketDefine.NunchiGameItemType.STEAL:
                PlayerUI[idx].Badge_Invincible.SetActive(false);
                PlayerUI[idx].Badge_DoubleItem.SetActive(false);
                PlayerUI[idx].Badge_Steal.SetActive(true);
                break;
        }

    }

    public void ShowTimerUI()
    {
        string _streamKey = MGNunchiDefines.BPW_MINIGAME_NUNCHI_TIMESTREAMKEY;

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
            SetActiveTimerObj(false);
        }
    }

    public void ShowToolTip(BPWPacketDefine.NunchiGameItemType itemInfo)
    {
        DEFAULT_RES_ICON_TYPE resType = DEFAULT_RES_ICON_TYPE.NULL;
        switch (itemInfo)
        {
            case BPWPacketDefine.NunchiGameItemType.INVINCIBLE:
                resType = DEFAULT_RES_ICON_TYPE.ITEM_INVINCIBLE;
                break;
            case BPWPacketDefine.NunchiGameItemType.COIN_DOUBLE:
                resType = DEFAULT_RES_ICON_TYPE.ITEM_DOUBLE;
                break;
            case BPWPacketDefine.NunchiGameItemType.STEAL:
                resType = DEFAULT_RES_ICON_TYPE.ITEM_STEAL;
                break;
        }

        if (resType != DEFAULT_RES_ICON_TYPE.NULL)
            ItemToolTip.SetData(resType);

        //ItemToolTip.SetData(DEFAULT_RES_ICON_TYPE.ITEM_DOUBLE);
    }

    
    //public void ShowItemNameTag(string name,Transform target, int idx)
    //{
        
    //    if (string.IsNullOrEmpty(name))
    //    {
    //        CDebug.Log("         ^^^^ ShowItemNameTag SetActive(false);");
    //        NameTag.gameObject.SetActive(false);
    //    }
    //    else
    //    {
    //        CDebug.Log("         ^^^^ ShowItemNameTag SetActive(true); name = "+name);
    //        NameTag.gameObject.SetActive(true);
    //        NameTag.Setup(name);

    //        /*
    //        if (target != null)
    //        {
    //            NameTag.transform.localPosition = Vector3.zero;
    //            //NameTag.transform.position = new Vector3(0f, 100f, 0f);
    //        }
    //        */
    //    }
    //}


    private void UpdateRemainTime(TimeStreamData timeStreamData)
    {
        if (timeStreamData.IsEnd == true)
        {

            timerDisposer.Dispose();

            PlayTimeStream.Remove();
            return;
        }

        if(remainTimeSeconds != timeStreamData.CurrentTime)
        {
           // CDebug.Log("Start Play Effect !!!!!!!!!!!!!!!!!!!!!!!!!!!");//
            PlayTimerCountAnim();
        }

        remainTimeSeconds = timeStreamData.CurrentTime;

        if(remainTimeSeconds > 0 && prevTimeSeconds != remainTimeSeconds)
        {
            SoundManager.Instance.PlayEffect( 6830012 ); // list 111
        }

        if (remainTimeSeconds >= 1)
        {
            SetTimerUIText(remainTimeSeconds);
        }
        else
        {
            WorldMgr.MapMgr.SetAllmapEffectNormal();//.AllAnimStop();
            // 시간 종료 상태로 변경해줘야 함(버튼 등)
            SetActiveTimerObj(false);
#if SHOW_MINIGAME_BTN
            SetActiveDirectionButtons(false);
            SetActiveButtonGroup(false);
#endif
            SetHideAllDialSlot();

            //WorldMgr.MapMgr.SetEffectAllActivate(false);
            WorldMgr.MapMgr.IsMapObjectClick = false;
        }

        prevTimeSeconds = remainTimeSeconds;
    }

    public void SetActiveUI(bool bActive)
    {
        PlayerInfoRoot.gameObject.SetActive(bActive);
        GameInfoRoot.gameObject.SetActive(bActive);
        //ButtonGroupRootObj.gameObject.SetActive(bActive);
    }

    public void SetTimerUIText(float time)
    {
        TimerTxt.text = time.ToString();
    }

    public void SetActiveStageStartObj(bool bActive)
    {
        StageStartRootObj.SetActive(bActive);
    }

    public void SetActiveStageTextObj(bool bActive)
    {
        StageTextRootObj.SetActive(bActive);
        StageTextObj.gameObject.SetActive(bActive);
        //subTitleObj.SetActive( bActive );
    }

    public void SetActiveRoundTextObj(bool bActive)
    {
        //StartRoundTextObj.gameObject.SetActive(bActive);
        
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
        List<FinalPlayerInfo> _list = MGNunchiServerDataManager.Instance.GetFinallistInfoList();
        string[] _finalistName = new string[MGNunchiDefines.FINALIST_RANK];
        for(int i=0; i< _list.Count; ++i)
        {
            _finalistName[i] = "winner";
            MGNunchiPlayerInfo _info = MGNunchiServerDataManager.Instance.GetGamePlayerInfo(_list[i].PID);
            _finalistName[i] = _info.NickName;
        }

        PopupMsg_FinalistTxt.text = string.Format( PopupMsg_FinalistTxt.text, _finalistName[0], _finalistName[1]);
    }

#if SHOW_MINIGAME_BTN

    public void SetActiveControllerBtn()
    {
        if (MyPlayer == null)
        {
            MyPlayer = WorldMgr.PlayerMgr.GetMyPlayer();
            //CDebug.Log("        ########## SET MyPlayer !!!!!!!!!!!!!!!!!!!!!");
        }
        //CDebug.Log("        ########## SET Active Controller Buttons By IsMovable !!!!!!!!!!!!!!!!!!!!!");
        for (int i = 0; i < MyPlayer.PlayerInfo.IsMovable.Length; ++i)
        {
            if (MyPlayer.PlayerInfo.IsMovable[i])
            {
                //CDebug.Log($"        ##########  IsMovable {(DIRECTION)i}  true  !!!!!!!!!!!!!!!!!!!!!");
                SetActiveControllerObj(true, i);
            }
            else
            {
                SetActiveControllerObj(false, i);
                //ControllerBtns[i].Btn.enabled = false;
            }
        }
    }

    public void SetActiveControllerObj(bool bActive, int idx)
    {
        //ControllerBtns[idx].NorObj.SetActive(bActive);
        ControllerBtns[idx].Btn.gameObject.SetActive(bActive);
    }

    public void SetActivePressBtn(int idx, bool bActive)
    {
        ControllerBtns[idx].PressObj.SetActive(bActive);
        ControllerBtns[idx].NorObj.SetActive(!bActive);
    }

    public void SetActiveDirectionButtons(bool bActive)
    {
        ControllerRootObj.gameObject.SetActive(bActive);
        if(bActive)
        {
            for (int i = 0; i < ControllerBtns.Length; ++i)
            {
                SetActivePressBtn(i, false);
            }
        
            }
    }

    public void SetActiveButtonGroup(bool bActive)
    {
        ButtonGroupRootObj.gameObject.SetActive(bActive);
    }

    public void InitPressedDirectionButtons()
    {
        for (int i = 0; i < MyPlayer.PlayerInfo.IsMovable.Length; ++i)
        {
            if (MyPlayer.PlayerInfo.IsMovable[i])
            {
                SetActivePressBtn(i, false);
            }
        }
    }
#endif

    public void PlayTimerCountAnim()
    {
        if (TimerCountAnim != null)
        {
            TimerCountAnim.Stop();
            TimerCountAnim.Play(MGNunchiDefines.ANIM_UI_TIMERCOUNT);
        }
    }


    public void PlayStartAnim()
    {
        //if (bPlaiedStartAnim) return;
        if(StartAnim != null)
        {
            StartAnim.Stop();
            StartAnim.Play(MGNunchiDefines.ANIM_UI_START);
            //bPlaiedStartAnim = true;
        }
    }

	private void SetupExpressionButtons( AVATAR_TYPE myAvatarType )
	{
		//expressionButtons.Initialize( transform, "obj_metaverse_ButtonGroup");
  //      expressionButtons.BindToOnClick( OnClickExpressionSlotButton );
  //      expressionButtons.SetExpressionSlotIcon( myAvatarType );

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
        if (TutorialManager.Instance.CurrentTutorialRandomID == 0)
        {
            WorldMgr.ShowQuitAlert();
        }
        return Observable.Return<bool>(true);
    }

    private void OnClickControllerBtn(int idx)
    {
        CDebug.Log("OnClickControllerBtn idx = " + idx);
#if SHOW_MINIGAME_BTN
        InitPressedDirectionButtons();
        SetActivePressBtn(idx, true);
#endif

        MyPlayer = WorldMgr.PlayerMgr.GetMyPlayer();
        MyPlayer.SetTargetDirectionInMap((DIRECTION)idx);
        MyPlayer.SetTargetAngle();
        WorldMgr.MapMgr.AddSelectMapIndex(MyPlayer.PlayerInfo.UID, MyPlayer.GetMapIndexByDirectioin());
    }

    private void OnClickMotionSlot( int motionSlotIndex )
    {
        RequestPlayMotion( motionSlotIndex - 1 );
    }


    private void ShowEmoticon(int slotIndex)
    {
        //이모티콘 출력
        MGNunchiPlayer myPlayer = WorldMgr.PlayerMgr.GetMyPlayer();
        var emoditonId = BPWorldServerDataManager.Instance.ExpressionManager.GetEquipedEmoticonID(myPlayer.PlayerInfo.AvatarType, slotIndex);
        if (emoditonId != 0)
        {
            //var myUid = long.Parse(CPlayer.GetStatusLoginValue(PLAYER_STATUS.USER_ID));
            int myPid = WorldMgr.PlayerMgr.MyPlayerPID;
            MGNunchiManager.Instance.WorldManager.AddEmoticon( myPid, emoditonId);
            TCPMGNunchiRequestManager.Instance.Req_EmoticonPerform(slotIndex);
        }
    }

  //  public void AddEmoticon(long uid, long emoticonID)
  //  {
		//MGNunchiPlayersManager _playersMgr = MGNunchiManager.Instance.WorldManager.PlayerMgr;
		//GameObject uiDummuBone = _playersMgr.GetPlayerDummyBone( uid );

		//IATCreator.AddEmoticon( emoticonID, uiDummuBone.transform );
  //  }

    private void OnClickEmoticonSlot( int emoticonSlotIndex )
    {
        //이모티콘
        ShowEmoticon(emoticonSlotIndex - 1);
    }

    public void SetHideAllDialSlot()
    {
        //expressionButtons.SetHideAllSlots();
    }

    //private void OnClickExpressionSlotButton(int radialButtonIndex)
    //{
    //    MsgUIBaseEntity msgEntity = null;
    //    int slotIndex = radialButtonIndex + 1;

    //    TCPMGNunchiRequestManager.Instance.Req_SlotExcute( slotIndex );
    //}
    #endregion Button_Click


    #region TCP_REQ
    private void RequestPlayMotion(int slotIndex)
    {
        AVATAR_TYPE nowMyAvatarType = BPWorldServerDataManager.Instance.GetMyUserInfo().AvatarType;
        Animator animator = StaticAvatarManager.GetAvatarAnimator(nowMyAvatarType);

        if (animator == null)
        {
            return;
        }
        CDebug.Log($" nunchi RequestPlayMotion slotIndex:{slotIndex}");
        TCPMGNunchiRequestManager.Instance.Req_MotionPerform(slotIndex);
    }
    #endregion TCP_REQ


    #region TUTORIAL
    public RectTransform GetMidArrBox()
    {
        return Mid_arrBox;
    }

    public RectTransform GetPlayerBox()
    {
        return PlayerBox;
    }

    public void SetActiveTutorialRoot(bool bActive)
    {
        TutorialRoot.SetActive(bActive);
    }

    public void SetTutorial(MGNunchiTutorial tutorial)
    {
        NunchiTutorial = tutorial;
    }
    #endregion TUTORIAL

    private void MakeSubTitleText()
    {
        subTitleObj = new GameObject( "sub_title", typeof( RectTransform ) );
        subTitleObj.layer = LayerMask.NameToLayer( "UI" );
        RectTransform rect = subTitleObj.GetComponent<RectTransform>();
        subTitleObj.transform.SetParent( StageTextRootObj.transform );
        rect.localPosition = Vector3.zero;
        rect.anchoredPosition = new Vector2( 0, -120 );
        rect.sizeDelta = new Vector2( 1020, 200 );
        rect.localScale = Vector3.one;
        subTitle = subTitleObj.AddComponent<Text>();
        subTitle.alignment = TextAnchor.MiddleCenter;
        subTitleObj.AddComponent<ChangeToFontByCountry>();
        subTitle.text = CResourceManager.Instance.GetString( 9170051 );
        subTitle.fontStyle = FontStyle.Italic;
        subTitle.fontSize = 52;
        subTitle.alignByGeometry = true;
        subTitle.resizeTextForBestFit = true;
        subTitle.resizeTextMinSize = 3;
        subTitle.resizeTextMaxSize = 52;
        subTitle.color = Color.white;
        Shadow shadowCmpnt = subTitleObj.AddComponent<Shadow>();
        Color clr = new Color( 0.57f, 0.4f, 0.83f, 1 );
        shadowCmpnt.effectColor = clr;
        shadowCmpnt.effectDistance = new Vector2( 2, -2 );
        shadowCmpnt.useGraphicAlpha = true;
        Outline outline = subTitleObj.AddComponent<Outline>();
        outline.effectColor = clr;
        outline.effectDistance = new Vector2( 2, -2 );
        outline.useGraphicAlpha = true;
    }

    public override void Release()
    {
        if (WorldMgr != null) WorldMgr = null;
        if (MyPlayer != null) MyPlayer = null;
        if (PlayerUI != null)
        {
            for (int i = 0; i < PlayerUI.Length; ++i)
            {
                PlayerUI[i].MyText = null;
                PlayerUI[i].PlayerText = null;
            }
            PlayerUI = null;
        }

        if(ScoreObj != null) ScoreObj = null;

#if SHOW_MINIGAME_BTN
        if (ControllerBtns != null) ControllerBtns = null;
#endif


        if (timerDisposer != null)
        {
            timerDisposer.Dispose();
            timerDisposer = null;
        }

		//if( expressionButtons != null )
		//{
		//	expressionButtons.Destroy();
		//	expressionButtons = null;
		//}
    }
}