using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using KSPlugins;
using Colorful;

public sealed class InGameManager : KSManager
{
    #region     INSTANCE
    private static InGameManager m_Instance;
    public static InGameManager instance
    {
        get
        {
            if (m_Instance == null)
            {
                if (MainManager.instance != null)
                    m_Instance = CreateManager(typeof(InGameManager).Name).AddComponent<InGameManager>();
                else
                    m_Instance = new InGameManager();
            }

            return m_Instance;
        }
    }
    #endregion  INSTANCE


    #region ENUM
    public enum eSTATUS : byte
    {
        ePREPARE = 0,
        eREADY,
        ePLAY,
        eEND,
        eREADY_RAID,
    }

    public enum ePVP_STATUS : byte
    {
        NORMAL = 0,
        MATCHING,
        MATCHED,
        BATTLEPLAY
    }

    public enum eGAME_SAVE_STATE : byte
    {
        NONE = 0,
        NEXT_FLOOR,
        AGAIN,
        //REBIRTH_INIT,
        REBIRTH,
        RESEN,
        LEVELUP,
        DIE,
        NPCGROUP_ALL_DIE,
        PUTINVENTORY,
        OPEN_GOLDBOX
    }

    public enum eGAME_FLOW_STATE : byte
    {
        NONE = 0,
        INTO_GAME,
        GOOUT_GAME
    }


	#endregion ENUM


	#region     MEMBERS
	public eSTATUS m_eStatus;
    public const float NPCSPWANTIME = 0.8f;
    public const int CONST_SECTER_MAXNUM = 4;
    public const float CONST_LAST_ATTACK_EVENT_TIMESCALE = 10;
    public const float CONST_LAST_ATTACK_EVENT_MAXTIME = 2.8f;

    public const float CONST_MENU_SLOW_TIME = 0f;
    public const float CONST_INVEN_CLOSE_DELAY = 0.5f;

    public const int CONST_FLOOR_SEL_AGAIN = 1;
    public const int CONST_FLOOR_SEL_REBIRTH = 416;
    public const int CONST_FLOOR_SEL_NEXT = 3208;

    public const int CONST_DAILYDG_EASY_SINDEX = 38000;

	public const int CONST_ENCHANTDUNGEON_SINDEX = 40001;

    public const int CONST_OPEN_LEVEL_DAILY_DUNGEON = 10;
    public const int CONST_OPEN_LEVEL_NONSYNC_PVP = 25;

    public GameObject m_gobjEffectStandParent;

    private List<KSManager> m_Managers;

    public bool m_lastAtkEvent;
    public float m_lastAtkEventStartTime;
    // 보스 죽었을때 이팩트 실행 여부 /// 이건 EventTrigger에서 체크 안함
    public bool m_bLastAttackEffect;
    //Taylor End

    //Camera Path bezier
    public GameObject m_camPathObj;

    public bool m_pauseGame;

    public bool m_bOffLine = true;
    public bool m_bHostPlay = false;

    /// 게임 플레이 시간
    public float m_fGameDurationTime = 0;
    public bool m_bCheckTime = false;
    public float m_TrialDungeonPTime = 0;
    ///  게임 진행중 얻은 경험치
    public int m_nTotalGainExp = 0;
    public int m_nCurretnGainExp = 0;


    public int m_TotalGainCoin = 0;
    public int m_TotalGainCash = 0;
    public bool m_bLevelUp = false;
    public eDROP_TYPE m_BossDropType;
       public ItemData dropItem;
    public ItemData m_BossDropItem;
    public SummonData m_BossDropPet;

    //npc spawn sequential
    public bool m_bNpcSpawnSeq;
    public int m_NpcSpawnGrpID;
    public float m_NpcSpnedSeqTime;
    private int m_NpcSpnSeqIdx;
    private const float NPC_SPAWNTIME_SEQUENTIALLY = 0.2f;

    public EventTriggerData bossEvtTriggerData;

    public bool m_bIngameFail = false;

    public blame_messages.RebirthRewardRepl m_RebirthFinishInfo = null;


    public int m_PreThemeIdx;
    public int m_CurTheme;
    public int m_CurFloor;
    public bool m_bFinishFloor;

    public bool m_bPauseAll;

    //UI
    public UI_PurchasePopupTwoButton m_uiPurchasePopupTwoButton;
    public UI_InGameInfo m_uiInGameInfo;
    private UI_Fade m_uiFade;  //
    public UI_DPad m_uiDPad;
    public UI_Skill m_uiSkill;
    public UI_Inventory m_uiInven;
    public UI_InGameTopInfo m_uiInGameTopInfo;
    public UI_Rebirth m_uiRebirth;
    public UI_RebirthNotice m_uiRebirthNotice;
    public UI_InGameButtons m_uiIngameButtons;
    public UI_ShopInfo m_uiShopInfo;
    public UI_InGameMenu m_uiIngameMenu;
    public UI_Combo m_uiCombo;
    public UI_CameraViewChange m_uiCamViewChange;
    public UI_InGameFloor m_uiIngameFloor;
    public UI_InGameCharacterInfo m_uiIngameCharInfo;
	public UI_InGameEnemyCharacterInfo m_uiInGamePvPEnemyCharacterInfo = null;
	public UI_IngamePvpTimeCount m_uiInGamePvPTimeCount = null;
	public UI_InGameSummonInfo m_uiIngameSummonInfo;
    public UI_SimulResult m_uiSimulationResult;
	public UI_BlacksmithShop m_uiBlacksmithShop = null;
	public UI_RebirthLevelAttribute m_uiRebirthLevelAttribute = null;
#if DAILYDG
    public UI_DungeonsLobby m_uiDungeonsLobby;
	public UI_InGameEnchant m_uiEnchantDungeon = null;
	private float m_fTime = 0;
	//private EventTriggerData.eCONDITIONKIND_TIMER m_eTimerKind;
	public UI_Timer m_uiTimer;
#endif
	//public UI_Mail m_uiMail;

	public UI_Loading m_Loading;

	public UI_PvpRanking m_uiPvpRanking;

    public UI_StageClear m_uiStageClear;
    //UI end

    public bool bLoadAllPlayerItem;
    public GameObject m_PlayerAllItemRoot;
    public CameraManager m_MainCam;

    public UI_Summon m_uiSummon;
    public UI_Notice m_uiNotice;

    public UI_Menu m_uiMenu;
    	public bool m_ResetEvent = false;
	public UI_Event			m_uiEvent			= null;
	public UI_InGameHotTime m_uiInGameHotTime	= null;



	public UI_BossHP m_uiBossHP;

	//private int m_ReadyStep;
	public int m_ReadyStep;
    public bool m_WarpInFinish;	
	public bool m_ThemeMoveStart;
	public bool m_bRisenPopup;
    public float m_RisenTimer;
    public const float MAX_RISEN_TIME = 10.9f;
    public bool m_bRibirthPopup;
    public int m_fadeOutStep;

    public bool m_bRebirthStart;
    public Dictionary<int, PotionData> m_RebirthPotionReward;
    public long m_RebirthExpReward;
    public long m_RebirthCoinReward;

    public ePVP_STATUS m_PvpStatus;

    public bool m_bStartCheckLoadTime;
    public float m_LoadApplyTime = 15;

    public bool m_StopTimer;
    public bool m_bAutoNextFloor;

    public eGAME_SAVE_STATE m_GameSaveState = eGAME_SAVE_STATE.NONE;

    public float m_PutInPktTime = 0.0f;

    private int bossGroupIndex = -1;

    public blame_messages.RankerInfo m_MyRankers = new blame_messages.RankerInfo();
    public List<blame_messages.RankerInfo> m_CurGroupRankers = new List<blame_messages.RankerInfo>();
    public List<blame_messages.RankGradeInfo> m_GroupRankers = new List<blame_messages.RankGradeInfo>();

	public List<blame_messages.LeaderBoardInfo> m_PvpRanking = new List<blame_messages.LeaderBoardInfo>();
	public blame_messages.LeaderBoardInfo m_MyPvpRanking = new blame_messages.LeaderBoardInfo();
	

    public List<int> m_myPartys = new List<int>();
    public List<int> m_otherPartys = new List<int>();
    public bool m_bNonPvpLoadComplete;
    public UI_PvpLobby m_uiPvpLobby;
    public bool m_bFinish4v4Pvp;
    public UI_PvpEnd m_uiPvpEnd;
    public bool m_bEnterPvpByCash;



	//PVP에서만 쓰는 스피드 아이콘
	public UI_PvpSpeedChange m_uiPvpGameSpeedChange;
    public UI_HotTimePopup m_uiHotTimePopup;
    public UI_MaintenancePopup m_uiMainTenancePopup;

#if DAILYDG
    public blame_messages.DungeonType m_DailyDG_type;
    public long m_EnteredDailyDG_Index;
    public long m_EnchantDGWaveCount;
    public bool m_bEnterRebirthDGFree;
    public uint m_EnterCount_RebirthDG;
    public bool m_bEnterEnchantDGFree;
    public uint m_EnterCount_EnchantDG;
#endif

    public bool m_MailNew = false;
    public bool[] m_MailBoxTapNew = new bool[3];

    public eGAME_FLOW_STATE m_GameFlowState;

    private IEnumerator m_Coroutine_CheckPvpTicket;
    #endregion  MEMBERS


    #region     OVERRIDE METHODS
    public override void Initialize()
    {
        UnityEngine.Debug.Log("IngameManager Initialize() start");
        m_SlowTimeScale_Flag = false;
        m_Managers = new List<KSManager>();
        //SceneManager.instance.SetUIComponent(SceneManager.eCOMPONENT.Loading);
        MainManager.instance.StartCoroutine(MapManager.instance.LoadGameMap());

        MapManager.instance.m_bVillage = false;
        m_bLastAttackEffect = false;
        m_NpcSpnSeqIdx = 0;
        m_bNpcSpawnSeq = false;
        //m_CurTheme = 1;
        //m_CurFloor = 1;

        //LocalDBManager.instance.InitLocalDB();
        //LocalDBManager.instance.RemoveLocalDB();

        m_eStatus = eSTATUS.ePREPARE;

    }


    public override void Process()
    {
        CheckStatus();

        if (m_bCheckTime == true)
            m_fGameDurationTime += Time.deltaTime;

        for (int i = 0; i < m_Managers.Count; i++)
        {
            m_Managers[i].Process();
        }

        //for test
        TestCheat();

        if (m_bNpcSpawnSeq)
        {
            SetNpcSpawnSequential();
        }

    }

    public void PauseGame()
    {
        Time.timeScale = 0;
    }

    public void ResumeGame()
    {
        Time.timeScale = 1;
    }

    public override void Destroy()
    {
        m_eStatus = eSTATUS.eEND;
        EffectResourcesManager.instance.Destroy();

        CinemaSceneManager.instance.DestroyCutSceneCameras();
        PlayerManager.instance.DestroyCutScene();
        for (int i = 0; i < m_Managers.Count; i++)
        {
            if(m_Managers[i] != null)
            {
                m_Managers[i].Destroy();
                m_Managers[i] = null;
            }
        }


        //StopRadialBlur();
        m_Managers.Clear();
        base.Destroy();
    }


    public void CleanHierarchy()
    {
    }

    #endregion  OVERRIDE METHODS

    //bool tmpStop = false;
    #region     PRIVATE METHODS
    private void CheckStatus()
    {
        switch (m_eStatus)
        {
            case eSTATUS.ePREPARE:
                {
					AsyncOperation async = MapManager.instance.async;

					if (async != null && async.isDone == true)
					{
						NetworkManager.instance.m_bNetTouch = false;

						MapManager.instance.async = null;

						m_fGameDurationTime = 0;
						m_ReadyStep = 0;
						async = null;
					
						
						if (m_Loading == null)
						{

							KSPlugins.KSResource.ClearResources();

							m_Loading = (UI_Loading)SceneManager.instance.SetUIComponent(SceneManager.eCOMPONENT.Loading);
							m_Loading.Initialize();
							m_Loading.LoadingSetGameObjectInit();
						}

					    /// 컴포넌트 로드
					    LoadUIComponent();

					    if (m_PlayerAllItemRoot == null)
                        {
                            m_PlayerAllItemRoot = new GameObject("PlayerAllItemRoot");
                            m_PlayerAllItemRoot.transform.localPosition = new Vector3(5000, 0, 0);
                        }


                        switch (SceneManager.instance.GetCurGameMode())
                        {
                            case SceneManager.eGAMEMODE.eMODE_CHAPTER:
                            case SceneManager.eGAMEMODE.eMODE_DAILY:
                            case SceneManager.eGAMEMODE.eNONSYNC_PVP:
                            case SceneManager.eGAMEMODE.eNPC_TEST_TOOL:

                                if (SceneManager.instance.GetCurGameMode() != SceneManager.eGAMEMODE.eNONSYNC_PVP)
                                {
                                    //Hot Time
                                    if (UserManager.instance.m_HotTimeInfo != null)
                                    {
                                        if (UserManager.instance.m_HotTimeInfo.state == 1)
                                        {
                                            m_uiInGameHotTime.SetHotEvent();
                                        }
                                    }
                                }

                                m_bOffLine = true;
                                if (PlayerManager.instance.m_PlayerInfo[PlayerManager.MYPLAYER_INDEX].jobType == eCharacter.eWarrior)
                                {
                                    if (CDataManager.m_WarriorItemModelDic.Count == 0)
                                    {
                                        //bLoadAllPlayerItem = true;
                                        LoadMyPlayerItems();
                                    }
                                }
                                else if (PlayerManager.instance.m_PlayerInfo[PlayerManager.MYPLAYER_INDEX].jobType == eCharacter.eWizard)
                                {
                                    if (CDataManager.m_MagicianItemModelDic.Count == 0)
                                    {
                                        //bLoadAllPlayerItem = true;
                                        LoadMyPlayerItems();
                                    }
                                }

                                break;
                            default:
                                m_bOffLine = false;
                                if (PlayerManager.instance.m_PlayerInfo[PlayerManager.MYPLAYER_INDEX].jobType == eCharacter.eWarrior)
                                {
                                    if (CDataManager.m_WarriorItemModelDic.Count == 0)
                                    {
                                        //bLoadAllPlayerItem = true;
                                        LoadMyPlayerItems();
                                    }
                                }
                                else if (PlayerManager.instance.m_PlayerInfo[PlayerManager.MYPLAYER_INDEX].jobType == eCharacter.eWizard)
                                {
                                    if (CDataManager.m_MagicianItemModelDic.Count == 0)
                                    {
                                        //bLoadAllPlayerItem = true;
                                        LoadMyPlayerItems();
                                    }
                                }
                                break;
                        }
                        m_gobjEffectStandParent = new GameObject("EffectStandResources");
                        m_gobjEffectStandParent.transform.localPosition = Vector3.zero;
                        m_gobjEffectStandParent.transform.localEulerAngles = Vector3.zero;
                        m_gobjEffectStandParent.transform.localScale = Vector3.one;


                        /// dpad 생성
                        m_uiDPad = (UI_DPad)SceneManager.instance.SetUIComponent(SceneManager.eCOMPONENT.DPad);
						m_uiDPad.Initialize();
						/// InGameButton 생성
						m_uiIngameButtons = (UI_InGameButtons)SceneManager.instance.SetUIComponent(SceneManager.eCOMPONENT.InGameButtons);

                        m_uiShopInfo = (UI_ShopInfo)SceneManager.instance.SetUIComponent(SceneManager.eCOMPONENT.ShopInfo);

                        if(SceneManager.instance.GetCurGameMode() == SceneManager.eGAMEMODE.eNONSYNC_PVP)
                        {
                            m_uiPvpEnd = (UI_PvpEnd)SceneManager.instance.SetUIComponent(SceneManager.eCOMPONENT.PvpEnd);
							m_uiPvpEnd.Initialize();
							m_uiPvpEnd.gameObject.SetActive(false);

							m_uiIngameCharInfo = (UI_InGameCharacterInfo)SceneManager.instance.SetUIComponent(SceneManager.eCOMPONENT.InGameCharacterInfo);
							m_uiInGamePvPEnemyCharacterInfo = (UI_InGameEnemyCharacterInfo)SceneManager.instance.SetUIComponent(SceneManager.eCOMPONENT.InGameEnemyCharacterInfo);
							m_uiInGamePvPTimeCount = (UI_IngamePvpTimeCount)SceneManager.instance.SetUIComponent(SceneManager.eCOMPONENT.IngamePvpTimeCount);
							//pvp모드에서만 쓰는 스피드업 UI 추가
							m_uiPvpGameSpeedChange = (UI_PvpSpeedChange)SceneManager.instance.SetUIComponent(SceneManager.eCOMPONENT.PvpSpeedChange);
							m_uiPvpGameSpeedChange.Initialize();
						}
                        else
                        {
                            m_uiIngameMenu = (UI_InGameMenu)SceneManager.instance.SetUIComponent(SceneManager.eCOMPONENT.InGameMenu);
							m_uiIngameMenu.Initialize();
							// ost add start
							// 좌측 메뉴 패널 생성
							m_uiMenu = (UI_Menu)SceneManager.instance.SetUIComponent(SceneManager.eCOMPONENT.Menu);
							m_uiMenu.Initialize();
							//m_uiMenu.gameObject.SetActive(false);
							// ost add end

                            SceneManager.instance.SetUIComponent(SceneManager.eCOMPONENT.InGameInfo);

                            m_Loading.SetValue(0.3f, 1);

                            /// 승리 UI 이팩트
                            SceneManager.instance.SetUIComponent(SceneManager.eCOMPONENT.BattleEnd).gameObject.SetActive(false);

                            /// 경고 이팩트
                            SceneManager.instance.SetUIComponent(SceneManager.eCOMPONENT.Warning);

                            

                            /// 아이템 획득
                            SceneManager.instance.SetUIComponent(SceneManager.eCOMPONENT.GetDropItem);

                            m_uiInGameTopInfo = (UI_InGameTopInfo)SceneManager.instance.SetUIComponent(SceneManager.eCOMPONENT.InGameTopInfo);
                            //m_uiInGameTopInfo.SetInfo();
                            m_uiSkill = (UI_Skill)SceneManager.instance.SetUIComponent(SceneManager.eCOMPONENT.Skill);

                            UnityEngine.Debug.Log("UI_Inventory loading before");
                            m_uiInven = (UI_Inventory)SceneManager.instance.SetUIComponent(SceneManager.eCOMPONENT.Inventory);
                            UnityEngine.Debug.Log("UI_Inventory loading after");

                            UnityEngine.Debug.Log("UI_BlacksmithShop loading before");
                            m_uiBlacksmithShop = (UI_BlacksmithShop)SceneManager.instance.SetUIComponent(SceneManager.eCOMPONENT.BlacksmithShop);
                            UnityEngine.Debug.Log("UI_BlacksmithShop loading after");

							m_uiRebirthLevelAttribute = (UI_RebirthLevelAttribute)SceneManager.instance.SetUIComponent( SceneManager.eCOMPONENT.RebirthLevelAttribute );

                            m_uiRebirth = (UI_Rebirth)SceneManager.instance.SetUIComponent(SceneManager.eCOMPONENT.Rebirth);
                            m_uiInGameInfo = (UI_InGameInfo)SceneManager.instance.GetUIComponent(SceneManager.eCOMPONENT.InGameInfo);

                            m_uiDungeonsLobby = (UI_DungeonsLobby)SceneManager.instance.SetUIComponent(SceneManager.eCOMPONENT.DungeonsLobby);

							InstanceDungeonManager.Instance.Initialize();
							//강화 던전 UI셋팅
							InstanceDungeonManager.Instance.Set_UIEnchantDungeon();
							//Set_UIEnchantDungeon();


							//m_uiMail = (UI_Mail)SceneManager.instance.SetUIComponent(SceneManager.eCOMPONENT.Mail);

                            m_uiIngameFloor = (UI_InGameFloor)SceneManager.instance.SetUIComponent(SceneManager.eCOMPONENT.InGameFloor);

							m_uiPvpRanking = (UI_PvpRanking)SceneManager.instance.SetUIComponent(SceneManager.eCOMPONENT.PvpRanking);


							m_uiPvpLobby = (UI_PvpLobby)SceneManager.instance.SetUIComponent(SceneManager.eCOMPONENT.PvpLobby);

							m_uiBossHP = (UI_BossHP)SceneManager.instance.SetUIComponent(SceneManager.eCOMPONENT.BossHP);
							m_uiBossHP.Initialized();

                            m_uiStageClear = (UI_StageClear)SceneManager.instance.SetUIComponent(SceneManager.eCOMPONENT.StageClear);

                        }

						if (SceneManager.instance.GetCurGameMode() == SceneManager.eGAMEMODE.eNPC_TEST_TOOL)
                        {
                            SceneManager.instance.SetUIComponent(SceneManager.eCOMPONENT.InGameCharacterInfo);
                        }
                        m_Loading.SetValue(0.4f, 1);

#if UNITY_EDITOR
                        if (MainManager.instance.m_bShowCamOption)
                        {
                            SceneManager.instance.SetUIComponent(SceneManager.eCOMPONENT.CameraGaugeControl);
                        }
#endif


                        if (SceneManager.instance.GetCurGameMode() == SceneManager.eGAMEMODE.eMODE_RAID ||
                            SceneManager.instance.GetCurGameMode() == SceneManager.eGAMEMODE.eMODE_PVP)
                        {
                            SpawnDataManager.instance.SpawnDataLoad(CDataManager.GetSpawnBinFile((uint)RaidManager.instance.dungeonID), false);
                        }
						
                        else if (SceneManager.instance.GetCurGameMode() == SceneManager.eGAMEMODE.eNONSYNC_PVP)
                        {
                            SpawnDataManager.instance.SpawnDataLoad(CDataManager.GetSpawnBinFile((uint)RaidManager.instance.dungeonID), true);
                        }
                        else
                        {
                            SpawnDataManager.instance.SpawnDataLoad(CDataManager.GetSpawnBinFile((uint)SpawnDataManager.instance.EventTriggerIndex), true);
                        }

						PlayerManager.instance.InitPartyMemberAbilities();

						if (SceneManager.instance.GetCurGameMode() == SceneManager.eGAMEMODE.eMODE_CHAPTER || SceneManager.instance.GetCurGameMode() == SceneManager.eGAMEMODE.eMODE_DAILY)
                        {
                            PlayerManager.instance.CreatePlayers(PlayerManager.MYPLAYER_INDEX, SceneManager.eMAP_CREATE_TYPE.IN_GAME);
                            
                            foreach (KeyValuePair<int, ShopData> myShop in PlayerManager.instance.m_PlayerInfo[PlayerManager.MYPLAYER_INDEX].myShopPurchaseDic)
                            {
                                PlayerManager.instance.SetAdd_ShopStatValue((int)myShop.Value.Index, PlayerManager.MYPLAYER_INDEX);
                            }
                        }
                        else
                        {
                            if (SceneManager.instance.GetCurGameMode() != SceneManager.eGAMEMODE.eMODE_FREEFIGHT)
                            {
                                SetPlayersCharInfo();
                            }

                            if (SceneManager.instance.GetCurGameMode() == SceneManager.eGAMEMODE.eMODE_PVP)
                            {

                            }
                            else if (SceneManager.instance.GetCurGameMode() == SceneManager.eGAMEMODE.eNONSYNC_PVP)
                            {
								//LoadMy Player
								PlayerManager.instance.CreatePlayers(PlayerManager.MYPLAYER_INDEX, SceneManager.eMAP_CREATE_TYPE.IN_GAME);
                                m_myPartys.Add(PlayerManager.instance.m_PlayerInfo[PlayerManager.MYPLAYER_INDEX].playerCharBase.charUniqID);
                                PlayerManager.instance.CreatePlayers(PlayerManager.PVPPLAYER_INDEX, SceneManager.eMAP_CREATE_TYPE.IN_GAME);
                                m_otherPartys.Add(PlayerManager.instance.m_PlayerInfo[PlayerManager.PVPPLAYER_INDEX].playerCharBase.charUniqID);

                                var enumerator = PlayerManager.instance.m_PlayerInfo[PlayerManager.PVPPLAYER_INDEX].myShopPurchaseDic.GetEnumerator();
                                while (enumerator.MoveNext())
                                {
                                    ShopData myShop = enumerator.Current.Value;
                                    PlayerManager.instance.SetAdd_ShopStatValue((int)myShop.Index, PlayerManager.PVPPLAYER_INDEX);
                                }
                            }
                        }

                        //zunghoon add Start 20170728

                        if (m_uiIngameSummonInfo == null)
                        {
                            m_uiIngameSummonInfo = (UI_InGameSummonInfo)SceneManager.instance.SetUIComponent(SceneManager.eCOMPONENT.InGameSummonInfo);
                            if (SceneManager.instance.GetCurGameMode() == SceneManager.eGAMEMODE.eNONSYNC_PVP)
                            {
                                m_uiIngameSummonInfo.Set4v4PvpSummonSlot();
                            }
                        }
						SceneManager.instance.GetUIComponent(SceneManager.eCOMPONENT.CameraViewChange).gameObject.SetActive(true);
						
						//zunghoon add End 20170728
						m_Loading.SetValue(0.5f, 1);

                        // gunny 20160406
                        vintage = Camera.main.gameObject.AddComponent<VintageFast>();
                        vintage.Shader2D = Shader.Find("Hidden/Colorful/Lookup Filter 2D");
                        vintage.Filter = Vintage.InstragramFilter.Inkwell;
                        vintage.LookupTexture = Resources.Load<Texture2D>("InstagramFast/Inkwell");
                        vintage.Amount = 0.8f;

                        vintage.enabled = false;

                        m_Wiggle = Camera.main.gameObject.AddComponent<Wiggle>();

                        m_Wiggle.Shader = Shader.Find("Hidden/Colorful/Wiggle");
                        m_Wiggle.Mode = Wiggle.Algorithm.Simple;
                        m_Wiggle.AutomaticTimer = true;
                        m_Wiggle.Speed = 1;
                        m_Wiggle.Frequency = 20;
                        m_Wiggle.Amplitude = 0.003f;

                        m_Wiggle.enabled = false;

                        /////////////////////////////////////////////////////////////////////////////////
                        m_RadialBlur = Camera.main.gameObject.AddComponent<RadialBlur>();
                        m_RadialBlur.Shader = Shader.Find("Hidden/Colorful/RadialBlur");

                        m_RadialBlur.Center = new Vector2(0.5f, 0.5f);
                        m_RadialBlur.Strength = 0f;
                        m_RadialBlur.EnableVignette = true;
                        m_RadialBlur.Sharpness = 0;
                        m_RadialBlur.Darkness = 53;
                        m_RadialBlur.enabled = false;
                        /////////////////////////////////////////////////////////////////////////////////

                        
                        ////////캐릭터 동결을 위한 라이트 세팅
                        //zunghoon 실시간 라이팅 추가
                        m_Directionallight = new GameObject("EffectLight");
                        m_LightComp = m_Directionallight.AddComponent<Light>();
                        m_LightComp.type = LightType.Directional;

                        m_LightComp.shadows = LightShadows.None;
                        string[] layer = new string[] { "Character", "NPC" };
                        m_LightComp.cullingMask = LayerMask.GetMask(layer);
                        //m_LightComp.color = new Color(0.2196f, 0.2196f, 1.0f);

                        m_LightComp.transform.rotation = Quaternion.Euler(45, 0, 0);
                        

                        NpcManager.instance.m_bLoadNpcPrefab = true;

                        m_fadeOutStep = 0;

                        NpcManager.instance.Initialize();
                        DamageUIManager.instance.Initialize();


						SummonManager.instance.Initialize();

#if ADD_TUTORIAL

						TutorialManager.instance.Initialize();
						if (!TutorialManager.instance.m_CompleteTutoria || TutorialManager.instance.m_Complete_SummonTutorial ==  false || TutorialManager.instance.m_Complete_ItemEnchantTutorial == false)
						{
							TutorialManager.instance.Init_Tuto_Obj();
							//PlayerManager.instance.playerInfoList[PlayerManager.MYPLAYER_INDEX].m_CurFloor

							TutorialManager.instance.SetTutorial_Init(PlayerManager.instance.playerInfoList[PlayerManager.MYPLAYER_INDEX].m_CurFloor);
						}
#endif
						m_ReadyStep = 0;
                        m_eStatus = eSTATUS.eREADY;
                        UnityEngine.Debug.Log("################# Check Status PrePare ##################################");
                    }
					
				}

					break;

            case eSTATUS.eREADY:
                {
                    switch (m_ReadyStep)
                    {
                        case 0:
                            UnityEngine.Debug.Log("eSTATUS.eREADY 0");
                            if (m_Loading == null)
                            {
                                m_Loading = (UI_Loading)SceneManager.instance.SetUIComponent(SceneManager.eCOMPONENT.Loading);
                            }
                            m_Loading.SetValue(0.55f, 1);
							m_ThemeMoveStart = false;

							if (SceneManager.instance.GetCurGameMode() == SceneManager.eGAMEMODE.eMODE_CHAPTER || SceneManager.instance.GetCurGameMode() == SceneManager.eGAMEMODE.eMODE_DAILY)
                            {
                                m_bPauseAll = false;
                                m_bFinishFloor = false;
                            }
                            else
                            {
								m_bPauseAll = true;
                            }
                            NetworkManager.instance.m_bNetTouch = false;
                            m_bRisenPopup = false;
                            m_bRibirthPopup = false;
                            m_bLastAttackEffect = false;
                            m_WarpInFinish = false;
                            m_bRebirthStart = false;
                            m_bStartCheckLoadTime = false;
                            m_StopTimer = false;
                            m_bAutoNextFloor = false;

                            m_nTotalGainExp = 0;
                            m_bNonPvpLoadComplete = false;
                            m_bFinish4v4Pvp = false;
                            bossGroupIndex = -1;
                            m_GameSaveState = eGAME_SAVE_STATE.NONE;
                            m_GameFlowState = eGAME_FLOW_STATE.INTO_GAME;
                            if (SceneManager.instance.GetCurGameMode() == SceneManager.eGAMEMODE.eMODE_CHAPTER|| SceneManager.instance.GetCurGameMode() == SceneManager.eGAMEMODE.eMODE_DAILY)
                            {
                                m_uiInven.m_inventoryItems.inventoryStep = (int)InventoryManager.eINVENTORY_CLOSE_STATE.CLOSE;
                                m_uiIngameFloor.Init();
                                m_uiInGameTopInfo.RefleshAll();

                                PlayerManager.instance.m_PlayerInfo[PlayerManager.MYPLAYER_INDEX].playerObj.SetActive(false);
                                SpawnDataManager.instance.InitSpawnData();
                                SpawnDataManager.instance.SpawnDataLoad(CDataManager.GetSpawnBinFile((uint)MapManager.instance.LastPlayMapData.EventTriggerIndex), false);

                                //층 진입에 성공했다면 무조건 LIVE상태
                                PlayerManager.instance.m_PlayerInfo[PlayerManager.MYPLAYER_INDEX].playerCharBase.SetCharacterState(CHAR_STATE.ALIVE);
                                PlayerManager.instance.m_PlayerInfo[PlayerManager.MYPLAYER_INDEX].playerCharBase.m_CharAi.m_Ai_ActionPool = null;
                                PlayerManager.instance.m_PlayerInfo[PlayerManager.MYPLAYER_INDEX].playerCharBase.m_CharAi.Init(eAIType.ePC, PlayerManager.instance.m_PlayerInfo[PlayerManager.MYPLAYER_INDEX].playerCharBase.OnAiEventEnd);
                                PlayerManager.instance.m_PlayerInfo[PlayerManager.MYPLAYER_INDEX].playerObj.transform.localPosition = SpawnDataManager.instance.HeroSpawnerDataList[PlayerManager.MYPLAYER_INDEX].m_v3Position;
                                PlayerManager.instance.m_PlayerInfo[PlayerManager.MYPLAYER_INDEX].naviObj.SetActive(true);
                                PlayerManager.instance.m_PlayerInfo[PlayerManager.MYPLAYER_INDEX].playerObj.SetActive(true);
                                PlayerManager.instance.m_PlayerInfo[PlayerManager.MYPLAYER_INDEX].playerObj.transform.localScale = SpawnDataManager.instance.HeroSpawnerDataList[PlayerManager.MYPLAYER_INDEX].m_v3LocalScale;

                                PlayerManager.instance.m_DpadMovePlayer.gobjTarget = PlayerManager.instance.m_PlayerInfo[PlayerManager.MYPLAYER_INDEX].playerObj;

                                //CharacterFactory.SetHideEquipParts(false);
                                CharacterFactory.SetHideEquipParts(true);


                                RenderSettings.fog = true;
                                RenderSettings.fogColor = new Color(
                                    (float)mapData.FogColor[0] / 255,
                                    (float)mapData.FogColor[1] / 255,
                                    (float)mapData.FogColor[2] / 255
                                    );


                                UnityEngine.Debug.Log("GetModefiedCurrentFloor()  == " + GetModefiedCurrentFloor());
                                UnityEngine.Debug.Log("PlayerManager.instance.m_PlayerInfo[PlayerManager.MYPLAYER_INDEX].m_CurFloor  == " + PlayerManager.instance.m_PlayerInfo[PlayerManager.MYPLAYER_INDEX].m_CurFloor);
                            }
                            else if (SceneManager.instance.GetCurGameMode() == SceneManager.eGAMEMODE.eNONSYNC_PVP)
                            {
                                SpawnDataManager.instance.InitSpawnData();
                                SpawnDataManager.instance.SpawnDataLoad(CDataManager.GetSpawnBinFile((uint)RaidManager.instance.dungeonID), false);

                                for(int i=0; i<PlayerManager.instance.m_PlayerInfo.Count; ++i)
                                {
                                    PlayerManager.instance.m_PlayerInfo[i].playerCharBase.SetCharacterState(CHAR_STATE.ALIVE);
                                    PlayerManager.instance.m_PlayerInfo[i].playerCharBase.m_CharAi.m_Ai_ActionPool = null;
                                    PlayerManager.instance.m_PlayerInfo[i].playerCharBase.m_CharAi.Init(eAIType.ePC, PlayerManager.instance.m_PlayerInfo[i].playerCharBase.OnAiEventEnd);
                                    PlayerManager.instance.m_PlayerInfo[i].playerObj.transform.localPosition = SpawnDataManager.instance.HeroSpawnerDataList[i].m_v3Position;
                                    PlayerManager.instance.m_PlayerInfo[i].playerObj.transform.localEulerAngles = SpawnDataManager.instance.HeroSpawnerDataList[i].m_v3EulerAngles;
                                    if (i == 0)
                                    {
                                        PlayerManager.instance.m_PlayerInfo[i].naviObj.SetActive(true);
                                    }
                                    PlayerManager.instance.m_PlayerInfo[i].playerObj.SetActive(true);
                                    PlayerManager.instance.m_PlayerInfo[i].playerObj.transform.localScale = SpawnDataManager.instance.HeroSpawnerDataList[i].m_v3LocalScale;
                                }

                                PlayerManager.instance.m_DpadMovePlayer.gobjTarget = PlayerManager.instance.m_PlayerInfo[PlayerManager.MYPLAYER_INDEX].playerObj;
								
							}
							m_ReadyStep = 1;
                            break;
                        case 1:
                            UnityEngine.Debug.Log("eSTATUS.eREADY 1");
                            NpcManager.instance.SetInGame();
                            
                            m_Loading.SetValue(0.7f, 1);
                            m_ReadyStep = 2;
                            break;
                        case 2:
                            //NpcManager.instance.SetInGame2();
                            m_ReadyStep = 3;
                            break;
                        case 3:
                            UnityEngine.Debug.Log("eSTATUS.eREADY 2");
                            m_Loading.SetValue(0.8f, 1);
                            EventTriggerManager.instance.Initialize();
                            m_MainCam = Camera.main.GetComponent<CameraManager>();

                            if (SceneManager.instance.GetCurGameMode() == SceneManager.eGAMEMODE.eMODE_CHAPTER)
                            {
                                if (NpcManager.instance.NpcBoss != null)
                                {
                                    m_uiInGameInfo.SetDifficulty();
                                }
                                m_MainCam.m_CamZoomDist = CameraManager.CAM_ZOOM_MAX;

                                m_MainCam.m_CamZoomState = CameraManager.eBATTLEZOON_STATE.eZOOMIN;

                                EventTriggerManager.instance.m_cameraManager.ChangeCameraMode(CameraManager.CAMERA_VIEW.eQUATERVIEW);
                            }
                            //SetInGameUI();
                            m_lastAtkEvent = false;
                            m_uiIngameSummonInfo.SummonAllSlotInit();


                            //UnityEngine.Debug.Log("%%% InGameManager 0005");
                            int iStance = (int)UserManager.instance.m_CurPlayingCharInfo.playerInfo.m_eStanceType - 1;
                            PlayerStanceInfo StanceInfo = PlayerManager.instance.GetPlayerStanceInfo(PlayerManager.instance.playerInfoList[PlayerManager.MYPLAYER_INDEX].playerCharBase, iStance);

                            //UnityEngine.Debug.Log("%%% InGameManager 0006");

                            List<AffectData> affectList = new List<AffectData>();

                            for (int j = 0; j < StanceInfo.system_ComboAffect_Code.Count; j++)
                            {
                                affectList.Add(CDataManager.GetAffect((int)StanceInfo.system_ComboAffect_Code[j]));
                            }
                            PlayerManager.instance.playerInfoList[PlayerManager.MYPLAYER_INDEX].playerCharBase.ComboOption_Set(StanceInfo);
                            if (m_uiCombo != null)
                            {
                                m_uiCombo.DoCombo(affectList, StanceInfo, 0);
                            }

                            UnityEngine.Debug.Log("eSTATUS.eREADY 3");
                            m_Loading.SetValue(0.9f, 1);
                            if (SceneManager.instance.GetCurGameMode() == SceneManager.eGAMEMODE.eMODE_PVP || SceneManager.instance.GetCurGameMode() == SceneManager.eGAMEMODE.eNONSYNC_PVP)
                            {
                                if (SceneManager.instance.GetCurGameMode() == SceneManager.eGAMEMODE.eMODE_PVP)
                                {
                                    /// 레이드 로딩이 끝났음을 알림
                                    NetworkManager.instance.networkSender.SendCompleteMultiLoadReq();
                                    m_eStatus = eSTATUS.eREADY_RAID;

                                    RaidManager.instance.InitVarInRaidStart();
                                    RaidManager.instance.Start();
                                }
                                //CameraManager mainCam = Camera.main.GetComponent<CameraManager>();

                                if (m_MainCam != null)
                                {
                                    if (SceneManager.instance.GetCurGameMode() == SceneManager.eGAMEMODE.eMODE_PVP || SceneManager.instance.GetCurGameMode() == SceneManager.eGAMEMODE.eNONSYNC_PVP)
                                    {
                                        m_MainCam.ChangeCameraMode(CameraManager.CAMERA_VIEW.eQUATERVIEW);
										m_MainCam.ChangeQuaterViewMode((CameraManager.CAME_DIST_MODE)m_MainCam.GetCameraViewMode("PvpCameraViewMode"));
										
										//Mission
										MissionManager.instance.CheckMission(eMISSION_KIND.eTRIED_PVP);
                                        
                                        NetworkManager.instance.networkSender.SendLoadPvpComplete();
                                    }
                                }
                               
                                m_MainCam = null;

								//Pvp 베틀 타임리밋 UI 셋팅
								m_uiInGamePvPTimeCount.Init();
								//pvp 진입시 상대 케릭터 체력 및 버프 디버프 UI 초기화
								m_uiIngameCharInfo.Init(PlayerManager.instance.m_PlayerInfo[PlayerManager.MYPLAYER_INDEX].playerCharBase);
								m_uiInGamePvPEnemyCharacterInfo.Init(PlayerManager.instance.m_PlayerInfo[1].playerCharBase);
								//내 펫 셋팅
								NpcManager.instance.SetAllSummonRevival();
                                //상대 펫
                                NpcManager.instance.SetPvpPlayerPets();
                                //UI Set
                                m_uiIngameButtons.m_uiAutoSkillButton.SetShow(true);

                            }
                            else
                            {

                                TitleManager.instance.m_bFromTitle = false;
								//if (MainManager.instance.m_bSimReward)
								//{
								//    m_uiSimulationResult = (UI_SimulResult)SceneManager.instance.SetUIComponent(SceneManager.eCOMPONENT.SimulResult);
								//}

								/// 환생 튜토리얼을 제거 zunghoon Delete Start 20180528
								
								if ((PlayerManager.instance.m_PlayerInfo[PlayerManager.MYPLAYER_INDEX].m_CurFloor >= 31 && PlayerManager.instance.playerInfoList[PlayerManager.MYPLAYER_INDEX].m_MaxRebirthTimes == 0)
                                    || PlayerManager.instance.playerInfoList[PlayerManager.MYPLAYER_INDEX].m_MaxRebirthTimes >= 1)
                                {
                                    if(m_RebirthFinishInfo != null)
                                    {
                                        m_uiRebirthNotice.SetNotice(m_RebirthFinishInfo);
                                        m_uiRebirthNotice.SetActiveGameObject(true);
                                    }
                                    SetRebirthGoodsReward();

                                    m_uiRebirth.InitReward();
                                    m_uiIngameMenu.SetActiveRebirthButton(true);
                                }
                                else
                                {
                                    m_uiIngameMenu.SetActiveRebirthButton(false);
                                }								



								m_uiIngameFloor.gameObject.SetActive(true);
                                m_uiInGameTopInfo.gameObject.SetActive(true);
                                
                                for (int i = 0; i < NpcManager.instance.m_PlayerSummonList.Count; i++)
                                {
                                    CheckUpMarkPet(NpcManager.instance.m_PlayerSummonList[i]);
                                    if (NpcManager.instance.m_PlayerSummonList[i].isAcquire)
                                    {
                                        m_uiIngameMenu.Set_isNewMark_Pet(true);
                                        break;
                                    }

                                }
                            }

                            //SetMenuUIActive(false);

                            if (m_uiNotice == null)
                            {
                                m_uiNotice = (UI_Notice)SceneManager.instance.SetUIComponent(SceneManager.eCOMPONENT.Notice);
                                m_uiNotice.SetActive_SummonDie(false);
                                m_uiNotice.SetActive_SkillAcquire(false);
                            }
                            

#if UNITY_EDITOR
                            Debug.Log("#### m_ReadyStep = 4;");
#endif


							m_Loading.SetValue(1, 1);

                            if (SceneManager.instance.GetCurGameMode() == SceneManager.eGAMEMODE.eNONSYNC_PVP)
                            {
                                PlayerManager.instance.m_PlayerInfo[PlayerManager.PVPPLAYER_INDEX].playerCharBase.damageCalculationData.SetSyncHP();
                                RaidManager.instance.SetNonSyncPvpPlayerPetSyncHP();
                                m_uiInGamePvPEnemyCharacterInfo.Init(PlayerManager.instance.m_PlayerInfo[1].playerCharBase);
                            }

                            PlayerManager.instance.m_PlayerInfo[PlayerManager.MYPLAYER_INDEX].playerCharBase.damageCalculationData.SetSyncHP();
                            NpcManager.instance.SetMasterSummonSyncHP();


                            m_ReadyStep = 4;
                            UnityEngine.Debug.Log("eSTATUS.eREADY 4");
                            break;
                    }
        }
                break;

            case eSTATUS.ePLAY:
                {
                    if (m_lastAtkEvent)
                    {
                        if (Time.time - m_lastAtkEventStartTime > CONST_LAST_ATTACK_EVENT_MAXTIME / CONST_LAST_ATTACK_EVENT_TIMESCALE)
                        {
                            m_lastAtkEvent = false;
                            Time.timeScale = 1;

                            //NetworkManager.instance.networkSender.SendPutInCharacterInfoReq();
                            m_eStatus = eSTATUS.eEND;                            
                        }
                    }
                    else
                    {
                        if (NpcManager.instance.NpcBoss != null && NpcManager.instance.NpcBoss.m_CharState == CHAR_STATE.DEATH)
                        {
                            UnityEngine.Debug.Log("eSTATUS.ePLAY      NpcManager.instance.NpcBoss 처치 완료 m_bLastAttackEffect === " + m_bLastAttackEffect);
                            if (m_bLastAttackEffect == false)
                            {
                                m_bLastAttackEffect = true;
                                SetBattleEndEffect();
                            }
                        }
                        
                        if (MainManager.instance.m_bCheckSimul)
                        {
                            if (NpcManager.instance.m_NpcSpawnGroupInfos[1].m_NpcObjects[0] != null)
                            {
                                MainManager.instance.CheckDoingSimulation();
                                MainManager.instance.m_bCheckSimul = false;
                            }
                        }
                    }

                    PlayerManager.instance.playerInfoList[PlayerManager.MYPLAYER_INDEX].m_PlayTime += Time.unscaledDeltaTime;
                    PlayerManager.instance.playerInfoList[PlayerManager.MYPLAYER_INDEX].m_TotalPlayTime += Time.unscaledDeltaTime;
                }
                break;
            case eSTATUS.eEND:
                break;
            case eSTATUS.eREADY_RAID:
                { }
                break;
        }
    }

    public void SetSimResultUI()
    {
        m_uiSimulationResult = (UI_SimulResult)SceneManager.instance.SetUIComponent(SceneManager.eCOMPONENT.SimulResult);
    }

    private void CheckUpMarkPet(SummonData summonData)
    {
        if(summonData.Acquire == false)
        {
            return;
        }

        //if(NpcManager.instance.m_PlayerSummonProp == null)
		if(NpcManager.instance.m_PlayerSummonProp == null || NpcManager.instance.m_PlayerSummonProp.Count == 0)
		{
            return;
        }

        NpcInfo.NpcProp                     a_kSummonProp                   = null;
        a_kSummonProp                                                       = NpcManager.instance.m_PlayerSummonProp[summonData.nContentIndex];


        m_uiIngameMenu.Set_isUpMark_Pet(true, a_kSummonProp);
    }


    public void SetHideUI(bool bActive)
    {
        m_uiIngameButtons.gameObject.SetActive(bActive);
        m_uiShopInfo.gameObject.SetActive(bActive);
        m_uiCombo.gameObject.SetActive(bActive);

        if (UserManager.instance.m_HotTimeInfo != null)
        {
            if (UserManager.instance.m_HotTimeInfo.state == 1)
            {
                if (m_uiInGameHotTime != null)
                {
                    m_uiInGameHotTime.SetActiveGameObject(bActive);
                }
            }
        }
        m_uiGoldShopButton.SetActive(bActive);
		SetActiveInGameMenu(bActive);

		m_uiCamViewChange.gameObject.SetActive(bActive);        
        m_uiMenu.gameObject.SetActive(bActive);
        m_uiGPGSButton.gameObject.SetActive(bActive);
        m_uiPotionButtons.SetActive(bActive);
        m_uiFreeGiftButton.SetActive(bActive);


        UiIngameSummonSetActive(bActive);

        if (bActive)
        {
            if (m_uiBossHP != null && m_uiBossHP.BossActiveFlag == true)
            {
				SetActiveInGameBossHp(true);
            }
            else
            {
                m_uiInGameInfo.gameObject.SetActive(bActive);
            }
#if ADD_TUTORIAL
			if (SetTutorialButton())
			{
		//		return;
			}
#endif
		}
        else
        {
            m_uiInGameInfo.gameObject.SetActive(bActive);
        }
    }

    public void NetworkPause(bool Pause)
    {
        if(Pause)
        {
            m_NetworkPause_Time = Time.timeScale;
            Time.timeScale = 0;
        }
        else
        {
			
			if(m_NetworkPause_Time == 0)
			{
				//Debug.LogError("m_NetworkPause_Time ===" + m_NetworkPause_Time);
				m_NetworkPause_Time = 1;
			}

            Time.timeScale = m_NetworkPause_Time;
        }
    }

    public void SlowTimeScale(bool Pause)
    {
        
        if (m_eStatus == eSTATUS.ePLAY)
        {
            if (Pause)
            {
                if(Time.timeScale <= 0)
                {
                    Time.timeScale = 1;
                }
                m_SlowTimeScale = Time.timeScale;
                m_SlowTimeScale_Flag = true;
                Time.timeScale = CONST_MENU_SLOW_TIME;
            }
            else
            {
                if (m_SlowTimeScale_Flag)
                {
                    m_SlowTimeScale_Flag = false;
                    //Time.timeScale = m_SlowTimeScale;
					Time.timeScale = 1;
				}
            }
        }       
		        
    }


#if ADD_TUTORIAL
	public void TutorialPopup()
	{
		//CDataManager.m_LocalText["start_Tutoial"], CDataManager.m_LocalText["yes"],CDataManager.m_LocalText["no"]
		m_TutoialStart_Popup = true;
		PopupManager.instance.ShowPopupTwoButton(CDataManager.m_LocalText["start_Tutoial"], CDataManager.m_LocalText["yes"], CDataManager.m_LocalText["no"], delegate { TutorialPopupClose(true); }, delegate { TutorialPopupClose(false); });
		InGameManager.instance.NetworkPause(true);
	}
	public void TutorialPopupClose(bool setTutorialStart)
	{
		InGameManager.instance.NetworkPause(true);
		if (setTutorialStart)
		{
			NetworkManager.instance.networkSender.SendSkipTutorial(false);
		}
		else
		{
			//TutorialManager.instance.m_CompleteTutoria = true;
			NetworkManager.instance.networkSender.SendSkipTutorial(true);
		}
	}
	public void TutorialSkip(bool receiveMessageError)
	{
		InGameManager.instance.NetworkPause(false);
		if (receiveMessageError)
		{
			PopupManager.instance.PopupTwoButtonClose();
			PopupManager.instance.ShowPopupOneButton(CDataManager.m_LocalText["wrong_request"], CDataManager.m_LocalText["ok"], null, false);
		}
		else
		{
			if (TutorialManager.instance.m_CompleteTutoria)
			{
				InGameManager.instance.m_uiInGameInfo.SetGameInfoActive(true);
				InGameManager.instance.SetHideUI(true);
			}
			else
			{
                TutorialManager.instance.m_TutoriaStep = 0;
                TutorialManager.instance.SetTutorial_Change(PlayerManager.instance.playerInfoList[PlayerManager.MYPLAYER_INDEX].m_CurFloor);
			}

			PopupManager.instance.PopupTwoButtonClose();
		}
	}
#endif


	public void SetActiveAllUI(bool bActive)
    {
        ////UnityEngine.Debug.Log("### SetActiveAllUI(false) ==========================");
        m_uiIngameButtons.gameObject.SetActive(bActive);
        m_uiShopInfo.gameObject.SetActive(bActive);
        m_uiCombo.gameObject.SetActive(bActive);
        m_uiInGameTopInfo.gameObject.SetActive(bActive);
        
        m_uiGoldShopButton.SetActive(bActive);

        SceneManager.instance.GetUIComponent(SceneManager.eCOMPONENT.InGameCharacterInfo).gameObject.SetActive(bActive);

        if(m_uiBossHP != null)
        {
            m_uiBossHP.gameObject.SetActive(bActive);
        }

        if(m_uiDPad != null)
            m_uiDPad.gameObject.SetActive(bActive);


        if (bActive == false)
        {
            m_uiDPad.DoRelease();
            SetActiveAllSummonHpBar(bActive);
        }

        //Debug.Break();
        m_uiCamViewChange.gameObject.SetActive(bActive);
        m_uiPotionButtons.SetActive(bActive);
        UiIngameSummonSetActive(bActive);

		if (m_uiPvpRanking != null)
		{
			m_uiPvpRanking.SetActivePvpRanking(false);
		}

        if (UserManager.instance.m_HotTimeInfo != null)
        {
            if (UserManager.instance.m_HotTimeInfo.state == 1)
            {
                if (m_uiInGameHotTime != null)
                {
                    m_uiInGameHotTime.SetActiveGameObject(bActive);
                }
            }
        }

		if (SceneManager.instance.GetCurGameMode() == SceneManager.eGAMEMODE.eMODE_CHAPTER)
        {
			//m_uiIngameMenu.gameObject.SetActive(bActive);
			SetActiveInGameMenu(bActive);

			m_uiMenu.gameObject.SetActive(bActive);
            m_uiGPGSButton.gameObject.SetActive(bActive);
            m_uiFreeGiftButton.SetActive(bActive);
            m_uiInGameInfo.gameObject.SetActive(bActive);
            m_uiIngameFloor.gameObject.SetActive(bActive);
        }
#if ADD_TUTORIAL
		if (bActive)
		{
			if (SetTutorialButton())
			{
			//	return;
			}
		}
#endif
	}
	public void SetActiveInCamViewButton(bool bActive)
	{
		//m_uiBossHP.m_BossActiveFlag = bActive;
		if (m_uiCamViewChange.gameObject.activeSelf != bActive)
		{
			m_uiCamViewChange.gameObject.SetActive(bActive);
		}
	}
	public void SetActiveInGameBossHp(bool bActive)
	{
		//m_uiBossHP.m_BossActiveFlag = bActive;
		if (m_uiBossHP.gameObject.activeSelf != bActive)
		{
			m_uiBossHP.gameObject.SetActive(bActive);
		}
	}
	public void SetActiveInGameCharInfo(bool bActive)
	{
		if (m_uiIngameCharInfo.gameObject.activeSelf != bActive)
		{
			m_uiIngameCharInfo.gameObject.SetActive(bActive);
		}
	}
	public void SetActiveInGameMenu(bool bActive)
	{
		if(m_uiIngameMenu.gameObject.activeSelf != bActive)
		{
			m_uiIngameMenu.gameObject.SetActive(bActive);
		}
		if(bActive == true)
		{
			m_uiIngameMenu.StartNewMarkSkill();
			m_uiIngameMenu.Set_isNewMark_Event(SetCheckActiveEventNewMark());
		}
		
	}
	public void SetActiveUiMenu(bool bActive)
	{
		if (m_uiMenu.gameObject.activeSelf != bActive)
		{
			m_uiMenu.gameObject.SetActive(bActive);
		}
		if (bActive == true)
		{
			m_uiMenu.Clear();
			m_uiIngameMenu.Set_isNewMark_Event(SetCheckActiveEventNewMark());
			
		}
	}
	public bool SetCheckActiveEventNewMark()
	{
		bool bEventCompley = false;

		for (int i = 0; i < UserManager.instance.m_AttendInfo.Count; i++)
		{
			if (UserManager.instance.m_AttendInfo[i].status == blame_messages.AttendanceStatus.AS_NOTYET)
			{
				return  true;
			}
		}
		return bEventCompley;
	}		
	public void SetActiveAllSummonHpBar(bool bActive)
    {
        if (NpcManager.instance.m_PlayerSummonObjs != null)
        {
            for (int i = 0; i < NpcManager.instance.m_PlayerSummonObjs.Count; i++)
            {
                if (NpcManager.instance.m_PlayerSummonObjs[i].m_MyObj.gameObject.activeSelf == true)
                {
                    NpcManager.instance.m_PlayerSummonObjs[i].m_hpBar.SetActiveHP(bActive);
                }
            }
        }
    }
    public void Set_isNewMark_Pet(bool isNewMark)
    {
        // ost modify before 20170209
        //m_uiIngameMenu.m_btPet_isNewMark.SetActive(isNewMark);
        // ost modify before 20170209
        m_uiIngameMenu.Set_isNewMark_Pet(isNewMark);
        // ost modify before 20170209
    }
    public void UiIngameSummonSetActive(bool _active)
    {
        if (m_uiIngameSummonInfo == null)
        {
            return;
        }


        if (_active)
        {
			if (SceneManager.instance.GetCurGameMode() == SceneManager.eGAMEMODE.eMODE_CHAPTER)
			{
				if (m_uiIngameSummonInfo.CheckEquipSummon() == false)
				{
					_active = false;
				}
			}
			else if (SceneManager.instance.GetCurGameMode() == SceneManager.eGAMEMODE.eNONSYNC_PVP)
			{
				bool mySummonActive = m_uiIngameSummonInfo.CheckEquipSummon();
				m_uiIngameSummonInfo.SetActiveSummonSlot(mySummonActive);

				bool pvpSummonActive = m_uiIngameSummonInfo.Check_4v4PvpSummon();
				m_uiIngameSummonInfo.SetActiveEnemySummonSlot(pvpSummonActive);
			}
        }

        if(m_uiIngameSummonInfo.gameObject.activeSelf != _active)
        {
            m_uiIngameSummonInfo.gameObject.SetActive(_active);
        }
    }
    public void SetHideSummon(bool bActive)
    {
        if (m_uiSummon != null)
        {
			m_uiSummon.SetActivePlayerSummonInfo(bActive, UI_SummonInfo.eOPTION.DETAILINFO, false);
            //m_uiSummon.gameObject.SetActive(bActive);
        }
    }
 
    public bool SetTutorialButton()
    {
		if (TutorialManager.instance.m_CompleteTutoria)
		{
			return false;
		}
		else if (PlayerManager.instance.playerInfoList[PlayerManager.MYPLAYER_INDEX].m_CurFloor <= 3 && PlayerManager.instance.playerInfoList[PlayerManager.MYPLAYER_INDEX].m_MaxRebirthTimes == 0)
		{
			m_uiInGameInfo.SetGameInfoActive(false);
		}
		return false;
		
    }

#endregion  PRIVATE METHODS

#region  PUBLIC METHODS
    public void SetBattleEndEffect()
    {
        UnityEngine.Debug.Log("SetBattleEndEffect()  ---------- 1");
        if (MainManager.instance.m_VibratioinState == eTOGGLESTATE.PLAYABLE)
        {
            Handheld.Vibrate();
            //Debug.Log("Start Vibration !!!! ");
        }

        if (NpcManager.instance.NpcBoss != null)
        {
            NpcManager.instance.NpcBoss.damageCalculationData.fCURRENT_HIT_POINT = 0;
            //NpcManager.instance.NpcBoss.CheckDie(DIE_TYPE.eDIE_NORMAL);
        }
        UnityEngine.Debug.Log("SetBattleEndEffect()  ---------- 2");
        //All Live Npcs make Die
        NpcManager.instance.SetAllNpcDie();
        m_lastAtkEvent = true;
        Time.timeScale = 1 / CONST_LAST_ATTACK_EVENT_TIMESCALE;

        EventTriggerManager.instance.BossDieSceneStart();

        PlayerManager.instance.m_PlayerInfo[PlayerManager.MYPLAYER_INDEX].naviObj.SetActive(false);

        //SceneManager.instance.DeleteUIComponent(SceneManager.eCOMPONENT.Warning);
        UnityEngine.Debug.Log("SetBattleEndEffect()  ---------- 3");


        m_lastAtkEventStartTime = Time.time;

        //콤보 초기화
        m_uiCombo.ResetCombo();

        SetActiveAllUI(false);
        SetMenuUIActive(false);
        if (SceneManager.instance.GetUIComponent(SceneManager.eCOMPONENT.BossHP) != null)
            SceneManager.instance.GetUIComponent(SceneManager.eCOMPONENT.BossHP).gameObject.SetActive(false);
        
        UnityEngine.Debug.Log("SetBattleEndEffect()  ---------- 4");
    }

    //부활 팝업시 활성화된 메뉴들 닫는다.일괄적으로
    public void SetHideActiveMenu()
    {
        m_uiInven.SetActiveInventory(false);
        m_uiInven.CloseItemPopup();
        m_uiSkill.SetActiveSkill(false, false);  
        m_uiSummon.SetActivePlayerSummonInfo(false, UI_SummonInfo.eOPTION.DETAILINFO, false);
        m_uiRebirth.SetActiveRebirth(false);
        m_uiMenu.Hide();
        m_uiShop.Hide();
#if DAILYDG
        m_uiDungeonsLobby.SetActive(false);
#endif
        //m_uiMail.SetActiveMail(false);
        //m_uiShop.bFromDeath = true;

        m_uiGoldShopPopup.Hide();        
        m_uiFreeGiftPopup.Hide();
        m_uiFreePotionPopup.Hide();
        // ost add start 20170303
        m_uiPurchasePopup.Hide();
        // ost add end 20170303

        // ost add start 20170327
        m_uiGotoMarket.ClickClose();
        // ost add end 20170327

        //if (GetPvpStatus() == ePVP_STATUS.MATCHING)
        //{
        //    m_uiIngameMenu.ClickPvpCancel();
        //}

        //모든 팝업은 닫는다
        PopupManager.instance.PopupOneButtonClose();
        PopupManager.instance.PopupTwoButtonClose();

        if (m_uiInven.GetActiveInventory())
        {
            m_uiInven.ClickInvenClose();
        }
		if (m_uiPvpRanking != null)
		{
			m_uiPvpRanking.SetActivePvpRanking(false);
		}
	}




    public void SetActiveNpcGroupSpawn(bool bTurnOn, int nGroupIndex = 1)
    {
        if (NpcManager.instance.m_NpcSpawnGroupInfos.ContainsKey(nGroupIndex) == false)
            return;
//#if UNITY_EDITOR
        if (nGroupIndex >= 200 && nGroupIndex<1000)
            return;
//#endif
        if (NpcManager.instance.m_NpcSpawnGroupInfos == null || NpcManager.instance.m_NpcSpawnGroupInfos.Count == 0 || NpcManager.instance.m_NpcSpawnGroupInfos[nGroupIndex] == null)
            return;

        bool isResetChecker = EventTriggerManager.instance.IsResetChecker();
        //Debug.Log("nGroupIndex " + nGroupIndex);
        int nCount = NpcManager.instance.m_NpcSpawnGroupInfos[nGroupIndex].m_NpcObjects.Count;
        for (int i = 0; i < nCount; ++i)
        {
            if (NpcManager.instance.m_NpcSpawnGroupInfos[nGroupIndex].m_NpcObjects[i] != null)
            {
                NpcManager.instance.m_NpcSpawnGroupInfos[nGroupIndex].m_NpcObjects[i].gameObject.SetActive(bTurnOn);
            }
            //
            NpcManager.instance.m_NpcSpawnGroupInfos[nGroupIndex].bEnable = true;
            NpcManager.instance.NpcSpwanedListAdd(NpcManager.instance.m_NpcSpawnGroupInfos[nGroupIndex].m_NpcObjects[i]);

            // 바리케이트 빼고 소환 작업
            if (NpcManager.instance.m_NpcSpawnGroupInfos[nGroupIndex].m_NpcObjects[i].m_CharAi != null)
            {
                if (NpcManager.instance.NpcBoss == NpcManager.instance.m_NpcSpawnGroupInfos[nGroupIndex].m_NpcObjects[i] &&
                    NpcManager.instance.NpcBoss.m_CharAi.GetNpcProp().Npc_Class != eNPC_CLASS.eRAIDBOSS &&
                    EventTriggerManager.instance.Cutscene_Boss != null) // 현재 레이드 보스는 제외함 투두
                {
                    bossGroupIndex = nGroupIndex;
                }

                if (MapManager.instance.m_bVillage == false)
                {
                    if (NpcManager.instance.m_NpcSpawnGroupInfos[nGroupIndex].m_NpcObjects[i].m_CharAi.GetNpcProp().Npc_Type == eNPC_TYPE.eMONSTER)
                    {
                        if (NpcManager.instance.m_NpcSpawnGroupInfos[nGroupIndex].m_NpcObjects[i].ksAnimation.GetAnimationClip("Spawn") == null)
                        {
                            SkillDataInfo.EffectResInfo tmpResInfo = NpcManager.instance.m_NpcSpawnGroupInfos[nGroupIndex].m_NpcObjects[i].m_CharAi.GetNpcProp().SpawnEffResInfo;
                            NpcManager.instance.m_NpcSpawnGroupInfos[nGroupIndex].m_NpcObjects[i].m_EffectSkillManager.FX_BUFF(NpcManager.instance.m_NpcSpawnGroupInfos[nGroupIndex].m_NpcObjects[i], tmpResInfo, -1, true);
                        }
                    }
                }
            }
        }
    }



    private void SetNpcSpawnSequential()
    {
        float pastTime = 0;
        if (m_NpcSpnSeqIdx < NpcManager.instance.m_NpcSpawnGroupInfos[m_NpcSpawnGrpID].m_NpcObjects.Count)
        {
            pastTime = (Time.time - m_NpcSpnedSeqTime);
            //Debug.Log("#### 111 GroupIdx = " + m_NpcSpawnGrpID + "/ past Time = " + pastTime);
            if (pastTime >= NPC_SPAWNTIME_SEQUENTIALLY || m_NpcSpnSeqIdx == 0)
            {
                bool isResetChecker = EventTriggerManager.instance.IsResetChecker();

                NpcManager.instance.m_NpcSpawnGroupInfos[m_NpcSpawnGrpID].m_NpcObjects[m_NpcSpnSeqIdx].gameObject.SetActive(true);

                NpcManager.instance.m_NpcSpawnGroupInfos[m_NpcSpawnGrpID].bEnable = true;
                NpcManager.instance.NpcSpwanedListAdd(NpcManager.instance.m_NpcSpawnGroupInfos[m_NpcSpawnGrpID].m_NpcObjects[m_NpcSpnSeqIdx]);

                //Debug.Log("#### 222 GroupIdx = " + m_NpcSpawnGrpID + " / npc name = " + NpcManager.instance.m_NpcSpawnGroupInfos[m_NpcSpawnGrpID].m_NpcObjects[m_NpcSpnSeqIdx].gameObject.name + "/ idx = " + m_NpcSpnSeqIdx + "/" + NpcManager.instance.m_NpcSpawnGroupInfos[m_NpcSpawnGrpID].m_NpcObjects.Count + "/// enable = " + NpcManager.instance.m_NpcSpawnGroupInfos[m_NpcSpawnGrpID].m_NpcObjects[m_NpcSpnSeqIdx].gameObject.activeSelf);

                // 바리케이트 빼고 소환 작업
                if (NpcManager.instance.m_NpcSpawnGroupInfos[m_NpcSpawnGrpID].m_NpcObjects[m_NpcSpnSeqIdx].m_CharAi != null)
                {
                    if ((NpcManager.instance.NpcBoss == NpcManager.instance.m_NpcSpawnGroupInfos[m_NpcSpawnGrpID].m_NpcObjects[m_NpcSpnSeqIdx] &&
                            NpcManager.instance.NpcBoss.m_CharAi.GetNpcProp().Npc_Class != eNPC_CLASS.eRAIDBOSS &&
                            EventTriggerManager.instance.Cutscene_Boss != null)) // 현재 레이드 보스는 제외함 투두
                    { }
                    else
                    {
                        if (isResetChecker == true)
                        {
                            int sID = NpcManager.instance.m_NpcSpawnGroupInfos[m_NpcSpawnGrpID].m_NpcObjects[m_NpcSpnSeqIdx].m_nSpawnerId;
                            int npcSpawnerListIdx = SpawnDataManager.instance.GetNpcSpawnerDataListIndexByID(sID);

                            NpcManager.instance.m_NpcSpawnGroupInfos[m_NpcSpawnGrpID].m_NpcObjects[m_NpcSpnSeqIdx].damageCalculationData.fCURRENT_HIT_POINT = NpcManager.instance.m_NpcSpawnGroupInfos[m_NpcSpawnGrpID].m_NpcObjects[m_NpcSpnSeqIdx].damageCalculationData.fMAX_HIT_POINT;
                            NpcManager.instance.m_NpcSpawnGroupInfos[m_NpcSpawnGrpID].m_NpcObjects[m_NpcSpnSeqIdx].m_CharState = CHAR_STATE.ALIVE;

                            NpcManager.instance.m_NpcSpawnGroupInfos[m_NpcSpawnGrpID].m_NpcObjects[m_NpcSpnSeqIdx].gameObject.transform.localPosition = SpawnDataManager.instance.NpcSpawnerDataList[npcSpawnerListIdx].m_v3Position;
                            NpcManager.instance.m_NpcSpawnGroupInfos[m_NpcSpawnGrpID].m_NpcObjects[m_NpcSpnSeqIdx].gameObject.transform.localEulerAngles = SpawnDataManager.instance.NpcSpawnerDataList[npcSpawnerListIdx].m_v3EulerAngles;
                            //EventTriggerManager.instance.SetResetChecker(false);
                        }

                        NpcManager.instance.m_NpcSpawnGroupInfos[m_NpcSpawnGrpID].m_NpcObjects[m_NpcSpnSeqIdx].SetChangeMotionState(MOTION_STATE.eMOTION_IDLE);
                        NpcManager.instance.m_NpcSpawnGroupInfos[m_NpcSpawnGrpID].m_NpcObjects[m_NpcSpnSeqIdx].SetChangeMotionState(MOTION_STATE.eMOTION_SPWAN);
                        ((NpcAI)(NpcManager.instance.m_NpcSpawnGroupInfos[m_NpcSpawnGrpID].m_NpcObjects[m_NpcSpnSeqIdx].m_CharAi)).SetPeaceState();
                    }

                    if (MapManager.instance.m_bVillage == false)
                    {
                        SkillDataInfo.EffectResInfo tmpResInfo = NpcManager.instance.m_NpcSpawnGroupInfos[m_NpcSpawnGrpID].m_NpcObjects[m_NpcSpnSeqIdx].m_CharAi.GetNpcProp().SpawnEffResInfo;
                        NpcManager.instance.m_NpcSpawnGroupInfos[m_NpcSpawnGrpID].m_NpcObjects[m_NpcSpnSeqIdx].m_EffectSkillManager.FX_BUFF(NpcManager.instance.m_NpcSpawnGroupInfos[m_NpcSpawnGrpID].m_NpcObjects[m_NpcSpnSeqIdx], tmpResInfo, -1, true);
                    }

                }
                m_NpcSpnSeqIdx++;
                m_NpcSpnedSeqTime = Time.time;

            }
        }
        else
        {
            m_NpcSpnSeqIdx = 0;
            m_bNpcSpawnSeq = false;

        }
    }




    public void SetLastAttackEffect()
    {
        //SetBattleEndEffect();

        PlayerManager.instance.m_PlayerInfo[0].playerCharBase.m_charController.PressingAttack(false);

        /// 게임 클리어시 계속 공격을 하는 버그 때문에 넣음
        //PlayerManager.instance.m_CharScr.m_charController.PressingAttack(false);
    }

    public CharacterBase FindCharacterBase(long p_InGameObject)
    {
        for (int i = 0; i < PlayerManager.instance.m_PlayerInfo.Count; ++i)
        {
            if (PlayerManager.instance.m_PlayerInfo[i].playerObj != null)
            {
                if (PlayerManager.instance.m_PlayerInfo[i].playerObj.activeSelf)
                {
                    if (PlayerManager.instance.m_PlayerInfo[i].playerCharBase.m_IngameObjectID == p_InGameObject)
                    {
                        return PlayerManager.instance.m_PlayerInfo[i].playerCharBase;
                    }
                }
            }
        }
        for (int i = 0; i < NpcManager.instance.m_monsterCharObjects.Count; ++i)
        {
            if (NpcManager.instance.m_monsterCharObjects[i].m_IngameObjectID == p_InGameObject)
            {
                return NpcManager.instance.m_monsterCharObjects[i];
            }
        }
        return null;
    }

    //Taylor : FreeFight
    public void SetPlayersCharInfo()
    {
        if (SceneManager.instance.GetCurGameMode() == SceneManager.eGAMEMODE.eMODE_CHAPTER ||
#if DAILYDG
            SceneManager.instance.GetCurGameMode() == SceneManager.eGAMEMODE.eMODE_DAILY ||
#endif
            SceneManager.instance.GetCurGameMode() == SceneManager.eGAMEMODE.eMODE_SCENARIO ||
            SceneManager.instance.GetCurGameMode() == SceneManager.eGAMEMODE.eGOLD_DUNGEON ||
            SceneManager.instance.GetCurGameMode() == SceneManager.eGAMEMODE.eMODE_DAY_OF_THE_WEEK_DUNGEON ||
            SceneManager.instance.GetCurGameMode() == SceneManager.eGAMEMODE.eMODE_TRIAL_DUNGEON ||
            SceneManager.instance.GetCurGameMode() == SceneManager.eGAMEMODE.eNPC_TEST_TOOL)
            return;

        UI_InGameFreeFightInfo component = null;

        if (SceneManager.instance.GetCurGameMode() == SceneManager.eGAMEMODE.eMODE_FREEFIGHT)
        {
            if (SceneManager.instance.GetUIComponent(SceneManager.eCOMPONENT.InGameFreeFightInfo) == null)
            {
                component = (UI_InGameFreeFightInfo)SceneManager.instance.SetUIComponent(SceneManager.eCOMPONENT.InGameFreeFightInfo);
            }
            else
            {
                component = (UI_InGameFreeFightInfo)SceneManager.instance.GetUIComponent(SceneManager.eCOMPONENT.InGameFreeFightInfo);
            }
        }

        for (int i = 0; i < RaidManager.instance.m_RaidPcInfo.Length; i++)
        {
            //Debug.Log("IngameManager.....PcInfo name = " + RaidManager.instance.m_RaidPcInfo[i].name);
            if (RaidManager.instance.m_RaidPcInfo[i].name != null)
            {
                int pIdx = PlayerManager.instance.GetPlayerIndexByName(RaidManager.instance.m_RaidPcInfo[i].name);

                //Debug.Log(i + "IngameManager.....RaidPcInfo = " + RaidManager.instance.m_RaidPcInfo[i].name + " / index = " + pIdx);
                if (pIdx != -1)
                {
                    //Debug.Log("###### SetPlayersCharInfo() Raid name = " + RaidManager.instance.m_RaidPcInfo[i].name + "/ Object = " + PlayerManager.instance.m_PlayerInfo[pIdx].playerObj + "/ PlayerName = " + PlayerManager.instance.m_PlayerInfo[pIdx].strName);
                    if (PlayerManager.instance.m_PlayerInfo[pIdx].playerObj != null)
                    {
                        continue;
                    }
                    PlayerManager.instance.m_PlayerInfo[pIdx].objID = RaidManager.instance.m_RaidPcInfo[i].id;
                    PlayerManager.instance.m_PlayerInfo[pIdx].iHP = RaidManager.instance.m_RaidPcInfo[i].iHP;
                    PlayerManager.instance.m_PlayerInfo[pIdx].imaxHP = RaidManager.instance.m_RaidPcInfo[i].imax_hp;

                    //Debug.Log("IngameManager.....call SetPlayersCharInfo() player Index = " + pIdx + "/" + RaidManager.instance.m_RaidPcInfo[i].name);

                    PlayerManager.instance.CreatePlayers(pIdx, SceneManager.eMAP_CREATE_TYPE.IN_GAME);

                    PlayerManager.instance.m_PlayerInfo[pIdx].playerCharBase.m_IngameObjectID = RaidManager.instance.m_RaidPcInfo[i].id;

					if (SceneManager.instance.GetCurGameMode() == SceneManager.eGAMEMODE.eMODE_FREEFIGHT)
                    {
                        //UI_InGameFreeFightInfo component = (UI_InGameFreeFightInfo)SceneManager.instance.SetUIComponent(SceneManager.eCOMPONENT.InGameFreeFightInfo);
                        PlayerManager.instance.m_PlayerInfo[pIdx].playerObj.transform.localPosition = RaidManager.instance.m_RaidPcInfo[i].iStartPos;
                        //Debug.Log("@@@@ IngameManager.277 line UI_InGameFreeFightInfo.AddUserInfo() userIDX = " + pIdx);
                        component.AddUserInfo(PlayerManager.instance.m_PlayerInfo[pIdx]);
                        if ((PlayerManager.instance.m_PlayerInfo[pIdx].playerObj.GetComponent<UI_PlayerName>() == null) && (!PlayerManager.instance.m_PlayerInfo[pIdx].strName.Equals(PlayerManager.instance.playerInfoList[PlayerManager.MYPLAYER_INDEX].strName)))
                        {
                            Debug.Log("@@@@@@@@@ nameInfo " + PlayerManager.instance.m_PlayerInfo[pIdx].strName);
                            PlayerManager.instance.m_PlayerInfo[pIdx].playerObj.AddComponent<UI_PlayerName>().Init(pIdx);
                        }
                    }

                    if (pIdx != PlayerManager.MYPLAYER_INDEX)
                    {
                        PlayerManager.instance.m_PlayerInfo[pIdx].playerCharBase.m_CharAi.enabled = true;
                    }
                    else if ((pIdx == PlayerManager.MYPLAYER_INDEX) && SceneManager.instance.GetCurGameMode() == SceneManager.eGAMEMODE.eMODE_FREEFIGHT)
                    {
                        AI ai = PlayerManager.instance.playerInfoList[PlayerManager.MYPLAYER_INDEX].playerObj.GetComponent<AI>();

                        if (PlayerManager.instance.m_bCheckAutoInMpde[(int)SceneManager.eGAMEMODE.eMODE_FREEFIGHT] == true)
                        {
                            ((AutoAI)ai).SetAutoPlayState(AutoAI.eAutoProcessState.ePlay);
                        }
                        else
                        {
                            ((AutoAI)ai).SetAutoPlayState(AutoAI.eAutoProcessState.eNone);
                        }
                    }
                }
                //RaidManager.instance.m_RaidPcInfo[i].name = null;
                //Debug.Log("RaidManager.instance.m_RaidPcInfo[" + i + "].name = " + RaidManager.instance.m_RaidPcInfo[i].name);
            }
            //RaidManager.instance.m_RaidPcInfo[i] = null;
        }
    }

    public void SetPlayerInfo(int objID, string name)
    {
        int pIdx = -1;
        int team = 1;

        pIdx = PlayerManager.instance.GetPlayerIndexByName(name);

        //Debug.Log("######### SetPlayerObject() objID = " + objID + " / pIdx = " + pIdx + " / name = " + name);
        //Debug.Log("@@@@ SEtPlayerInfo GetUIComponent(FreeFight)");

        if (pIdx == -1)
        {
            if (objID < PlayerManager.instance.m_PlayerInfo.Count - 1)
            {
                int emptyIdx = PlayerManager.instance.GetEmptyPlayerIndex();
                if (emptyIdx != -1)
                {
                    PlayerManager.instance.InsertPlayerInfo(RaidManager.instance.m_PartyMemberList[objID].rMemInfo, emptyIdx, team);
                }
                else
                {
                    PlayerManager.instance.LoadPlayerInfo(RaidManager.instance.m_PartyMemberList[objID].rMemInfo, team);
                }
            }
            else
            {
                PlayerManager.instance.LoadPlayerInfo(RaidManager.instance.m_PartyMemberList[objID].rMemInfo, team);
            }
        }
    }

    public void ShowUI()
    {
		//UI_BossHP uihp = (UI_BossHP)SceneManager.instance.GetUIComponent(SceneManager.eCOMPONENT.BossHP);


        switch (SceneManager.instance.GetCurGameMode())
        {
            case SceneManager.eGAMEMODE.eMODE_CHAPTER:
				//uihp = (UI_BossHP)SceneManager.instance.GetUIComponent(SceneManager.eCOMPONENT.BossHP);
				if (m_uiBossHP != null && m_uiBossHP.BossActiveFlag == true)
				{
					SetActiveInGameBossHp(true);
					m_uiBossHP.SetBossHP(SceneManager.eGAMEMODE.eMODE_CHAPTER);
				}

				m_uiInGameInfo.gameObject.SetActive(false);
                SetActiveAllSummonHpBar(true);

                break;
            case SceneManager.eGAMEMODE.eMODE_RAID:
				//uihp = (UI_BossHP)SceneManager.instance.GetUIComponent(SceneManager.eCOMPONENT.BossHP);

				//uihp = (UI_BossHP)SceneManager.instance.GetUIComponent(SceneManager.eCOMPONENT.BossHP);
				if (m_uiBossHP != null && m_uiBossHP.BossActiveFlag == true)
				{
					SetActiveInGameBossHp(true);
					m_uiBossHP.SetBossHP(SceneManager.eGAMEMODE.eMODE_RAID);
				}				


                SceneManager.instance.SetUIComponent(SceneManager.eCOMPONENT.MultiPlayUserInfo);
                break;
            case SceneManager.eGAMEMODE.eMODE_PVP:
                break;
            case SceneManager.eGAMEMODE.eMODE_FREEFIGHT:
                break;
        }
    }

    public void HideUI()
    {
        switch (SceneManager.instance.GetCurGameMode())
        {
            case SceneManager.eGAMEMODE.eMODE_RAID:
                SceneManager.instance.DeleteUIComponent(SceneManager.eCOMPONENT.BossHP);
                SceneManager.instance.DeleteUIComponent(SceneManager.eCOMPONENT.MultiPlayUserInfo);
                break;
            case SceneManager.eGAMEMODE.eMODE_PVP:
                break;
            case SceneManager.eGAMEMODE.eMODE_FREEFIGHT:
                break;
        }
    }


    public void RemoveSummonAllPartyAffect()
    {
        PlayerInfo a_kPlayerInfo = PlayerManager.instance.m_PlayerInfo[PlayerManager.MYPLAYER_INDEX];
        List<SummonSlotData> a_kSummonSlotData = new List<SummonSlotData>();

        if (NpcManager.instance.m_TeamSwap == false)
        {
            if (NpcManager.instance.m_TeamSummonSlotChange == false)
            {
                return;
            }
        }

        if (a_kPlayerInfo.m_SummonSlotGroup == null)
        {
            return;
        }

        //a_kPlayerInfo.m_SummonSlotGroup.TryGetValue(NpcManager.instance.m_TeamSlotGroupIndex, out a_kSummonSlotData);
        a_kPlayerInfo.m_SummonSlotGroup.TryGetValue(1, out a_kSummonSlotData);

        for (int i = 0; i < a_kSummonSlotData.Count; i++)
        {
            if (a_kSummonSlotData[i].m_SlotSummonIndex == -1)
            {
                continue;
            }

            InGameItemManager.instance.RemovePartyAffect(a_kSummonSlotData[i].m_SlotSummonIndex);
        }
    }
	public void AddSummonAllPartyAffect()
	{
		PlayerInfo a_kPlayerInfo = PlayerManager.instance.m_PlayerInfo[PlayerManager.MYPLAYER_INDEX];
		List<SummonSlotData> a_kSummonSlotData = new List<SummonSlotData>();
		if (NpcManager.instance.m_TeamSwap == false)
		{
			if (NpcManager.instance.m_TeamSummonSlotChange == false)
			{
				return;
			}
		}

		if (a_kPlayerInfo.m_SummonSlotGroup == null)
		{
			return;
		}

		//a_kPlayerInfo.m_SummonSlotGroup.TryGetValue(NpcManager.instance.m_TeamSlotGroupIndex, out a_kSummonSlotData);
		a_kPlayerInfo.m_SummonSlotGroup.TryGetValue(1, out a_kSummonSlotData);

		for (int i = 0; i < a_kSummonSlotData.Count; i++)
		{
			if (a_kSummonSlotData[i].m_SlotSummonIndex == -1)
			{
				continue;
			}

			InGameItemManager.instance.AddPartyAffect(a_kSummonSlotData[i].m_SlotSummonIndex);
		}
	}
	private void CleanUpPartySummon()
	{
		PlayerInfo a_kPlayerInfo = PlayerManager.instance.m_PlayerInfo[PlayerManager.MYPLAYER_INDEX];
		List<SummonSlotData> a_kSummonSlotData = new List<SummonSlotData>();

		a_kPlayerInfo.m_SummonSlotGroup.TryGetValue(1, out a_kSummonSlotData);

		for (int i = 0; i < a_kSummonSlotData.Count; i++)
		{
			if (a_kSummonSlotData[i].m_SlotSummonIndex == -1)
			{
				continue;
			}
			InGameItemManager.instance.RemovePartyAffect(a_kSummonSlotData[i].m_SlotSummonIndex);
		}
	}

    private void CleanUpPVPPartySummon()
    {
        //Remove Party SetValue
        EquipedItems a_kEquipItems = null;
        ItemStatus a_kItemStatus = null;
        List<SetItemData> curSetItems;
        for (int i=0; i< NpcManager.instance.m_PlayerSummonProp.Count; ++i)
        {
            a_kEquipItems = NpcManager.instance.m_PlayerSummonProp[i].curSummonEquipItems;
            a_kItemStatus = NpcManager.instance.m_PlayerSummonProp[i].summonItemStatus;
            curSetItems = InventoryManager.instance.GetSetItemDatas(a_kEquipItems);
            a_kItemStatus.RemoveParty_Affect(curSetItems);
        }

    }


    public void CleanUpFloor(bool bRemovePartyAbilitels = false)
    {
		if(bRemovePartyAbilitels == true)
		{
            if (SceneManager.instance.GetCurGameMode() == SceneManager.eGAMEMODE.eNONSYNC_PVP)
            {
                CleanUpPVPPartySummon();
            }
            else
            {
                CleanUpPartySummon();
            }
		}

        NpcManager.instance.CleanUpBoss();
        NpcManager.instance.CleanNpcObjects();
		EventTriggerManager.instance.CleanEventTrigger();
		NpcManager.instance.ClearLists();
		NpcManager.instance.ClearAllNpcEffectObject();
		SpawnDataManager.instance.Destroy();

		KSResource.ClearResources();
    }

    public void SetAgainCurFloor()
    {
        CleanUpFloor();

        m_PreThemeIdx = MapManager.instance.LastPlayMapData.ThemeIndex;

        m_GameSaveState = eGAME_SAVE_STATE.AGAIN;

        SetNextFloorInfos(MapManager.instance.LastPlayMapData.StageIndex, blame_messages.NextStageState.AGAIN);

    }

    public void SetNextFloor()
    {
        MapData nextMapData;
        bool reload = false;
        //UnityEngine.Debug.LogError("##############  SetNextFloor()() ##############~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
        int maxReturnThemeFloor = CExcelData_MAP_FACTOR_DATA.instance.MAP_FACTOR_DATABASE_GetMAX_FLOOR(0);
        int nextStage;
        m_PreThemeIdx = MapManager.instance.LastPlayMapData.ThemeIndex;

        nextStage = MapManager.instance.LastPlayMapData.NextStage;

        nextMapData = MapManager.instance.GetMapDataByStageIndex(nextStage);
        if(m_PreThemeIdx != nextMapData.ThemeIndex)
        {
            reload = true;
        }
        CleanUpFloor(reload);

        m_GameSaveState = eGAME_SAVE_STATE.NEXT_FLOOR;


        SetNextFloorInfos(nextStage, blame_messages.NextStageState.NEXT_FLOOR);
        

    }

    public void SetNextFloorInfos(int stageIndex, blame_messages.NextStageState stageSelect)
    {
        //MapManager.instance.LastPlayMapData = MapManager.instance.GetMapDataByStageIndex(stageIndex);
        //SpawnDataManager.instance.EventTriggerIndex = MapManager.instance.LastPlayMapData.EventTriggerIndex;

        PlayerManager.instance.m_PlayerInfo[PlayerManager.MYPLAYER_INDEX].playerCharBase.damageCalculationData.fCURRENT_HIT_POINT = PlayerManager.instance.m_PlayerInfo[PlayerManager.MYPLAYER_INDEX].playerCharBase.damageCalculationData.fMAX_HIT_POINT;
        PlayerManager.instance.playerInfoList[PlayerManager.MYPLAYER_INDEX].playerCharBase.m_ComboCount = 0;
        PlayerManager.instance.playerInfoList[PlayerManager.MYPLAYER_INDEX].playerCharBase.InitCoolTime();
        PlayerManager.instance.playerInfoList[PlayerManager.MYPLAYER_INDEX].playerCharBase.m_EffectSkillManager.PauseAllPlayingEffect();
        PlayerManager.instance.playerInfoList[PlayerManager.MYPLAYER_INDEX].playerCharBase.CameraAnimatorInit();

        //if (GetPvpStatus() == ePVP_STATUS.MATCHING)
        //{
        //    m_uiIngameMenu.ClickPvpCancel();
        //}

        m_TotalGainGold = 0;
        m_TotalGainCoin = 0;
        m_TotalGainCash = 0;
        m_bLevelUp = false;

        PopupManager.instance.PopupAllClose();
        if (m_uiInven != null)
        {
            m_uiInven.CloseItemPopup();
        }
        if (m_uiSkill != null)
        {
            m_uiSkill.CloseEquipPopup();
        }
        if (m_uiBossHP != null)
        {
            m_uiBossHP.gameObject.SetActive(false);
        }
        SceneManager.instance.DeleteUIComponent(SceneManager.eCOMPONENT.EquipJewelPopup);
        SetPutInCharacterInfoForSend(stageSelect);
    }

    public void GoNextTheme()
    {
        m_fadeOutStep = 0;
        SceneManager.instance.GoDirectToMyLobby();
        SceneManager.instance.SetCurGameMode(SceneManager.eGAMEMODE.eMODE_NONE);
        LoadingManager.instance.ChanageMainFlow(eMAIN_FLOW.eCONTENTS);
    }


    private void InitRebirthDatas()
    {
        PlayerManager.instance.playerInfoList[PlayerManager.MYPLAYER_INDEX].RebirthTimes = 0;
        PlayerManager.instance.playerInfoList[PlayerManager.MYPLAYER_INDEX].m_MaxRebirthTimes++;

        PlayerManager.instance.playerInfoList[PlayerManager.MYPLAYER_INDEX].m_PlayTime = 0f;
        PlayerManager.instance.playerInfoList[PlayerManager.MYPLAYER_INDEX].m_KillMonsterCount = 0;
        PlayerManager.instance.playerInfoList[PlayerManager.MYPLAYER_INDEX].m_GainGold = 0;
        PlayerManager.instance.playerInfoList[PlayerManager.MYPLAYER_INDEX].m_GainCoin = 0;

        // gunny delete 
        // 환생시 아이템 레벨 초기화 하던 클라 코드를 삭제해야 한다.
        //Equiped Item level initialize;
        //InGameItemManager.instance.InitEquipedItemLevel();

        m_bRebirthStart = true;
    }


    public void SetRebirthGoodsReward()
    {
        m_RebirthExpReward = 0;
        m_RebirthCoinReward = 0;

        //Rebirth Exp
        m_RebirthExpReward += GetRebirthExp();

        //Reward Coin
        m_RebirthCoinReward += GetRebirthCoinReward();
    }

    public void SetRebirthPotionReward()
    {
        int curModifiedFloor = GetModefiedCurrentFloor();

        if (m_RebirthPotionReward == null)
        {
            m_RebirthPotionReward = new Dictionary<int, PotionData>();
        }

        m_RebirthPotionReward.Clear();

    }

    //Rebirth
    public long GetRebirthGoldReward()
    {
        float goldStart = CExcelData_MAP_FACTOR_DATA.instance.MAP_FACTOR_DATABASE_GetBASE_GOLD_VALUE(0);
        float goldNormal = CExcelData_MAP_FACTOR_DATA.instance.MAP_FACTOR_DATABASE_GetGOLD_NORM(0);
        float goldQuot = CExcelData_MAP_FACTOR_DATA.instance.MAP_FACTOR_DATABASE_GetGOLD_QUOT(0);
        float pow = UtilManager.instance.CalcPow(PlayerManager.instance.m_PlayerInfo[PlayerManager.MYPLAYER_INDEX].m_CurFloor, goldQuot);

        return (long)((goldStart + goldNormal * pow) * (1.5f));
    }

    public long GetRebirthCoinReward()
    {
        int curFloor = PlayerManager.instance.m_PlayerInfo[PlayerManager.MYPLAYER_INDEX].m_CurFloor;        
        float coinStart = CExcelData_MAP_FACTOR_DATA.instance.MAP_FACTOR_DATABASE_GetBASE_MEDAL_VALUE(0);
        float coinNormal = CExcelData_MAP_FACTOR_DATA.instance.MAP_FACTOR_DATABASE_GetMEDAL_NORM(0);
        float coinQuot = CExcelData_MAP_FACTOR_DATA.instance.MAP_FACTOR_DATABASE_GetMEDAL_QUOT(0);
        float pow = UtilManager.instance.CalcPow(curFloor, coinQuot);

		AttributeAffect a_kAttributeAffect = PlayerManager.instance.m_PlayerInfo[ PlayerManager.MYPLAYER_INDEX ].GetAttributeAffect( EAttributeAffect.COIN_GAIN_RATIO );
		float l_fGainMedalRate = 1 + ( PlayerManager.instance.m_PlayerInfo[ PlayerManager.MYPLAYER_INDEX ].gainRate_coin + a_kAttributeAffect.Value ) / 100;
        //float l_fStageMedalReward = coinStart + (coinNormal * pow * (0.9f + 0.1f * (int)(curFloor / 30)));
        float l_fStageMedalReward = coinStart + (coinNormal * pow * (0.8f + 0.2f * (int)(curFloor / 30)));
        //long RewardCoin = (long)(Mathf.Round((coinStart + (coinNormal * pow * (0.5f + 0.5f * (curFloor / 30))))) * (1 + PlayerManager.instance.m_PlayerInfo[PlayerManager.MYPLAYER_INDEX].gainRate_coin / 100));
        long RewardCoin = (long)Mathf.Round(l_fStageMedalReward * l_fGainMedalRate);

        return RewardCoin;
    }


    public void SetPvpMode()
    {
        //Debug.Log("InGameManager.SetPvpMode() =============");

        CleanUpFloor(true);

        MapData pvpMapData = MapManager.instance.GetMapDataByStageIndex(RaidManager.instance.dungeonID);
        //SetPvpStatus(ePVP_STATUS.NORMAL);
        m_PreThemeIdx = pvpMapData.ThemeIndex;
        SpawnDataManager.instance.EventTriggerIndex = MapManager.instance.LastPlayMapData.EventTriggerIndex;

        LocalDBManager.instance.PutCharacterData(PlayerManager.instance.playerInfoList[PlayerManager.MYPLAYER_INDEX]);

        PlayerManager.instance.m_PlayerInfo[PlayerManager.MYPLAYER_INDEX].playerCharBase.damageCalculationData.fCURRENT_HIT_POINT = PlayerManager.instance.m_PlayerInfo[PlayerManager.MYPLAYER_INDEX].playerCharBase.damageCalculationData.fMAX_HIT_POINT;
        PlayerManager.instance.playerInfoList[PlayerManager.MYPLAYER_INDEX].playerCharBase.m_ComboCount = 0;

        m_TotalGainGold = 0;
        m_TotalGainCoin = 0;
        m_TotalGainCash = 0;
        m_bLevelUp = false;

        //NetworkManager.instance.networkSender.SendPutInCharacterInfoReq();
        //LocalDBManager.instance.SetSaveVersionNSendPutInCharInfo();

        LocalDBManager.instance.SaveLocalDB();

        SceneManager.instance.GoDirectToMyLobby();
        LoadingManager.instance.ChanageMainFlow(eMAIN_FLOW.eCONTENTS);
        //GoNextTheme();
    }

    public void SetMoveToDungeons(SceneManager.eGAMEMODE dungeonMode, RaidManager.SelectedMode selMode, int mapDataIndex)
    {
		
		CleanUpFloor(true);

		//use m_IndexOfDifficulty : map data index
		MapData DgMapData = MapManager.instance.GetMapDataByStageIndex(mapDataIndex);
        MapManager.instance.m_strSceneMapName = DgMapData.StageFileName;

        MapManager.instance.LastPlayMapData = MapManager.instance.GetMapDataByStageIndex(mapDataIndex);

        m_bCheckTime = true;



        SceneManager.instance.SetCurGameMode(dungeonMode);
        RaidManager.instance.m_SelectedMode = selMode;
        m_fadeOutStep = 0;
        m_TotalGainGold = 0;
        m_fGameDurationTime = 0;
        m_Loading = null;
        SceneManager.instance.DeleteUIComponent(SceneManager.eCOMPONENT.Loading);
        SceneManager.instance.SetUIComponent(SceneManager.eCOMPONENT.Loading);
        //m_Loading.gameObject.SetActive(true);
        //SetActiveFadeOut();
        SceneManager.instance.GoDirectToMyLobby();
        LoadingManager.instance.ChanageMainFlow(eMAIN_FLOW.eCONTENTS);
        SlowTimeScale(false);
    }
	public void IngameStartInitialize()
	{
		switch (m_fadeOutStep)
		{
			case 0:
				if (m_GameFlowState == eGAME_FLOW_STATE.INTO_GAME)
				{
#if UNITY_EDITOR
					Debug.Log("#### m_Loading.SetFade(UI_Loading.eSTATE.IN, delegate { UIFade_IN(); });");
#endif

                    if (SceneManager.instance.GetCurGameMode() != SceneManager.eGAMEMODE.eNONSYNC_PVP)
                    {
                        m_uiSummon.SummonInfoInit();
                    }
					IngameStartSetting();
				}
				break;
			case 1:
				IngameStartSetting();
				break;
		}
	}
    
    private void SetWarpInFinish()
    {
        PlayerManager.instance.m_PlayerInfo[PlayerManager.MYPLAYER_INDEX].playerCharBase.SetChangeMotionState(MOTION_STATE.eMOTION_IDLE);
        if (PlayerManager.instance.playerInfoList[PlayerManager.MYPLAYER_INDEX].playerObj != null)
        {
            if (SceneManager.instance.GetCurGameMode() != SceneManager.eGAMEMODE.eMODE_PVP)
            {
                InGameManager.instance.m_MainCam.m_CamZoomState = CameraManager.eBATTLEZOON_STATE.eNONE;
            }

            AI ai = PlayerManager.instance.playerInfoList[PlayerManager.MYPLAYER_INDEX].playerObj.GetComponent<AI>();
            ((AutoAI)ai).SetAutoPlayState(AutoAI.eAutoProcessState.eReady);
        }
        NpcManager.instance.SetSummonAllWork(false);


        InGameManager.instance.m_WarpInFinish = true;
    }

    public void GoNextFloor()
    {
        UnityEngine.Debug.Log("GoNextFloor() ##############");
        m_ReadyStep = 0;
        m_MainCam.m_CamZoomState = CameraManager.eBATTLEZOON_STATE.eNONE;
        if (m_PreThemeIdx == MapManager.instance.LastPlayMapData.ThemeIndex && m_bRebirthStart == false)
        {
            m_eStatus = eSTATUS.eREADY;
        }
        else
        {
            m_eStatus = eSTATUS.ePREPARE;
        }
        CharacterBase cBase = PlayerManager.instance.m_PlayerInfo[0].playerObj.GetComponent<CharacterBase>();
        cBase.m_CharacterDamageEffect.RecoveryMaterial();
 
        if (CinemaSceneManager.instance.m_bSkillCutScenePlaying)
            CinemaSceneManager.instance.m_bSkillCutScenePlaying = false;
        cBase.m_CameraManager.CameraAnimationEnd();
        cBase.CameraAnimatorInit();

        Resources.UnloadUnusedAssets();
    }

    public void GoBackInGame()
    {
        m_LoadApplyTime = 15;
        m_bStartCheckLoadTime = false;
        m_uiIngameFloor.gameObject.SetActive(true);
        m_uiInGameTopInfo.gameObject.SetActive(true);
        //m_uiIngameCharInfo.gameObject.SetActive(true);
		SetActiveInGameCharInfo(true);
		m_uiInGameInfo.gameObject.SetActive(true);
        if (m_uiDPad != null)
        {
            m_uiDPad.gameObject.SetActive(true);
            m_uiDPad.DoRelease();
        }
        //UI_DPad dpad = (UI_DPad)SceneManager.instance.GetUIComponent(SceneManager.eCOMPONENT.DPad);
        //dpad.gameObject.SetActive(true);
        //dpad.DoRelease();
        SetActiveAllSummonHpBar(true);

        ((AutoAI)PlayerManager.instance.m_PlayerInfo[PlayerManager.MYPLAYER_INDEX].playerCharBase.m_CharAi).SetAutoPlayState(AutoAI.eAutoProcessState.eReady);
        m_bFinishFloor = false;
        NpcManager.instance.HideAllNpcHPbar(true);
        SetHideUI(true);
        //SetPvpStatus(InGameManager.ePVP_STATUS.NORMAL);
        //m_uiIngameMenu.gameObject.SetActive(true);
		SetActiveInGameMenu(true);

		for (int i = 0; i < RaidManager.instance.m_PartyMemberList.Length; i++)
        {
            if (RaidManager.instance.m_PartyMemberList[i].rMemInfo != null)
            {
                RaidManager.instance.m_PartyMemberList[i].rMemInfo.strName = null;
            }
        }

        for (int i = PlayerManager.instance.m_PlayerInfo.Count - 1; i >= 1; i--)
        {
            if (PlayerManager.instance.m_PlayerInfo[i].playerObj != null)
            {
                PlayerManager.instance.m_PlayerInfo[i].playerObj.SetActive(false);
            }
            PlayerManager.instance.m_PlayerInfo.RemoveAt(i);
        }
        SceneManager.instance.DeleteUIComponent(SceneManager.eCOMPONENT.PvpLoading);

        NetworkManager.instance.networkSender.SendExitRaid();
    }

    public void InitInGameDatas()
    {
        m_bPauseAll = false;
        m_StopTimer = false;
        CleanUpFloor(true);
        InventoryManager.instance.m_IngameEquipItem = false;
        //Item to Server
        //InGameItemManager.instance.ItemUniqueIDs.Clear();
        UserManager.instance.m_CharacterInfos.Clear();
        //Dont use anymore
        //LocalDBManager.instance.m_DbDataInfoPool_Item.Clear();
        if (m_uiMenu != null)
        {
            m_uiMenu.CancelCheckDailyInvoke();
        }
        LocalDBManager.instance.SaveLocalDB();
    }

    // Use UIs
    public bool CanAccessState()
    {
        if (SceneManager.instance.GetCurGameMode() == SceneManager.eGAMEMODE.eMODE_CHAPTER
           || SceneManager.instance.GetCurGameMode() == SceneManager.eGAMEMODE.eMODE_DAILY)
        {
            if (m_WarpInFinish == false)
            {
                return false;
            }
        }

        if (m_bRisenPopup)
        {
            //return false;
        }

        if (m_bRibirthPopup)
        {
            return false;
        }

        return true;
    }

    public bool DontMoveEveryOne()
    {
        if (m_bFinishFloor || MainManager.instance.m_bSimReward || GetPvpStatus() == ePVP_STATUS.MATCHED || m_bPauseAll)
            return true;

        return false;
    }


    public void SetPvpStatus(ePVP_STATUS status)
    {
        m_PvpStatus = status;
        if (m_uiIngameMenu != null)
        {
            m_uiIngameMenu.SetPvpState();
        }
    }

    public ePVP_STATUS GetPvpStatus()
    {
        return m_PvpStatus;
    }

    public int GetModefiedCurrentFloor()
    {
        return GetModefiedFloor(PlayerManager.instance.playerInfoList[PlayerManager.MYPLAYER_INDEX].m_CurFloor);
    }

    public int GetModefiedFloor(int floor)
    {
        int maxReturnThemeFloor = CExcelData_MAP_FACTOR_DATA.instance.MAP_FACTOR_DATABASE_GetMAX_FLOOR(0);
        int curModifiedFloor = floor % maxReturnThemeFloor;

        //90의 배수에 해당하는 층 일 경우
        if (curModifiedFloor == 0)
        {
            curModifiedFloor = maxReturnThemeFloor;
        }

        return curModifiedFloor;
    }

    public void SetSimpleCharacterInfo(eGAME_SAVE_STATE state)
    {
        LocalDBManager.instance.PutCharacterData(PlayerManager.instance.m_PlayerInfo[PlayerManager.MYPLAYER_INDEX]);
        if (state == eGAME_SAVE_STATE.LEVELUP)
        {
            m_GameSaveState = state;
        }
        NetworkManager.instance.networkSender.SendSimpleCharacterInfoReq();
    }
    
    public float GetInvenCloseDelayTime()
    {
        return CONST_MENU_SLOW_TIME * CONST_INVEN_CLOSE_DELAY;
    }

    public void SetCharacterChangeDatas()
    {
        CleanUpFloor(true);


        m_fadeOutStep = 0;
        m_CurGroupRankers.Clear();
		m_PvpRanking.Clear();
        CDataManager.InitShopDatas();
        UserManager.instance.PurchaseSkillPoint = 0;
        LocalDBManager.instance.m_DbDataInfoSummon.Clear();
        LocalDBManager.instance.m_DbDataInfo_MyShop.Clear();
        LocalDBManager.instance.m_DbDataInfo_Mission.Clear();
        LocalDBManager.instance.m_DbDataInfo_Potion.Clear();        
        PlayerManager.instance.m_PlayerInfo[PlayerManager.MYPLAYER_INDEX].myPotionDic.Clear();
        PlayerManager.instance.m_PlayerInfo[PlayerManager.MYPLAYER_INDEX].myShopPurchaseDic.Clear();
        PlayerManager.instance.m_PlayerInfo[PlayerManager.MYPLAYER_INDEX].purchasedShopInfoDic.Clear();
        PlayerManager.instance.m_PlayerInfo[PlayerManager.MYPLAYER_INDEX].m_ItemStatus.affects.Clear();
        PlayerManager.instance.m_PlayerInfo[PlayerManager.MYPLAYER_INDEX].m_ItemStatus = null;
        PlayerManager.instance.m_PlayerInfo.Clear();

	//< add ggango 2017.09.29
		FixedPriceManager.Instance.Clear();
	//>

    }
#endregion  PUBLIC METHODS

#region RENDER
    public static GameObject AddShadow(GameObject p_gobjCharacter)
    {
        GameObject SShadow = KSPlugins.KSResource.Instantiate("Shadow/SimpleShadow", p_gobjCharacter.transform);
        CapsuleCollider CCColider = p_gobjCharacter.GetComponent<CapsuleCollider>();

        SShadow.transform.localPosition = new Vector3(0, 0.01f, 0);
        SShadow.transform.localScale = new Vector3(CCColider.radius * 1.25f, CCColider.radius * 1.25f, CCColider.radius * 1.25f);

        return SShadow;
    }
#endregion  RENDER

    private void UpdatePvpTicketChargingTime()
    {
        if (PlayerManager.instance.m_bStartPvpTicketCharge)
        {
            long nowTime = DateTime.Now.Ticks;
            long finishTime = UI_Menu.ServerTimeToTicks(PlayerManager.instance.m_FinishPvpTicketChargeTime);
            PlayerManager.instance.m_RemainPvpTicketChargeTime = UI_Menu.TicksToSec(finishTime - nowTime);
//#if UNITY_EDITOR
//            Debug.Log("InGameManager.UpdatePvpTicketChargingTime() remain = "+ UI_Menu.DisplayRemainTimeMinSec(PlayerManager.instance.m_RemainPvpTicketChargeTime));
//#endif
        }
    }

    public void BeginCheck4v4PvpTicketTime()
    {
        m_Coroutine_CheckPvpTicket = StartCheck4v4PvpTicketTime();
        StartCoroutine(m_Coroutine_CheckPvpTicket);
    }

    public void StopCheck4v4PvpTicketTime()
    {
        StopCoroutine(m_Coroutine_CheckPvpTicket);
    }

    IEnumerator StartCheck4v4PvpTicketTime()
    {
        while (true)
        {
            NetworkManager.instance.networkSender.SendCheck4v4PvpTicketTime();
            yield return new WaitForSecondsRealtime(10.0f);
        }
    }

    public void SetPutInCharacterInfoForSend(blame_messages.NextStageState stageSelect)
    {
        if (NetworkManager.instance.m_bSessionClosed)
        {
            NetworkManager.instance.SetDisConnectPopup();
        }
        else
        {
            UnityEngine.Debug.Log("@@@@ Send message PutInCharacterInfoReq");
            //SaveVersion();
            NetworkManager.instance.networkSender.SendPutInCharacterInfoReq(stageSelect);

            //< modify ggango 2017.11.13
            ISummonAbilities a_iSummonAbilities = PlayerManager.instance.GetPlayerSummonAbilities();
            a_iSummonAbilities.SyncPlayerSummonListWithDicSummons();
            //>
        }
    }


    public void SetNextStageDatas(int stageIndex)
    {
        MapManager.instance.LastPlayMapData = MapManager.instance.GetMapDataByStageIndex(stageIndex);
        SpawnDataManager.instance.EventTriggerIndex = MapManager.instance.LastPlayMapData.EventTriggerIndex;
        m_fadeOutStep = 1;
    }

    public void SetNextStageForLoad(blame_messages.NextStageState stageState)
    {
        //InGameManager.instance.m_Loading.gameObject.SetActive(true);
        m_Loading.LoadingSetGameObjectInit();
        m_Loading.SetActive(true);
        // 스테이지 클리어시 오토를 중지
        if (PlayerManager.instance.playerInfoList[PlayerManager.MYPLAYER_INDEX].playerObj != null)
        {
            AI ai = PlayerManager.instance.playerInfoList[PlayerManager.MYPLAYER_INDEX].playerObj.GetComponent<AI>();

            ((AutoAI)ai).SetAutoPlayState(AutoAI.eAutoProcessState.eNone);
            ((AutoAI)ai).StopAutoPlay();
        }
    }

    public void SetClearNextTheme()
    {
        m_ThemeMoveStart = true;
        NpcManager.instance.DestroyDropObjects();
        GoNextTheme();
        UnityEngine.Debug.Log("Stageclear( ===== ) ===== GoNextTheme");
    }

    public void SetOpenMode()
    {
        long currentLevel = PlayerManager.instance.m_PlayerInfo[PlayerManager.MYPLAYER_INDEX].level;
        if (currentLevel == CONST_OPEN_LEVEL_DAILY_DUNGEON)
        {
            m_uiIngameMenu.btDailyDG.gameObject.SetActive(true);
            m_uiIngameMenu.btDailyDGBlock.gameObject.SetActive(false);
        }
        if (currentLevel == CONST_OPEN_LEVEL_NONSYNC_PVP)
        {
            m_uiIngameMenu.btPvpMode.gameObject.SetActive(true);
            m_uiIngameMenu.btPvpModeBlock.gameObject.SetActive(false);
        }
    }

    public void ClearBossStage()
    {
        InGameItemManager.instance.ClearDungeon();
    }


	public void ContentsRenewal()
	{	
		EventContentsReset();
	}
	private void EventContentsReset()
	{
		m_ResetEvent								= true;

		if(m_eStatus == eSTATUS.ePLAY)
		{
			m_uiIngameMenu.Set_isNewMark_Event(true);
		}
		//m_uiEvent.ResetDataRunningAttendReward();
		//m_uiEvent.ResetDataAdayAttendReward();
	}
}



