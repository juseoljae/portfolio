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
using GroupManagement.ManagementEnums;
using MANAGEMENT_ACTIVITY;



public class ManagementManager : BaseSceneManager
{
    #region MEMBER_VAR
    static ManagementManager s_this = null;
    public static ManagementManager Instance { get { return s_this; } }
    #endregion MEMBER_VAR

    //private EntranceProcessService entranceService;
    private ManagementWorldManager _managementWorldManager;
    public GameObject WorldMgrObj;

    private GameObject IntraMailLayer;
	private GameObject EventIconLayer;
	private GameObject BubbleLayer;
    private GameObject TouchLayer;
    private GameObject NameTagLayer;
    private GameObject ArchiveTrophyInfoLayer;

	private GameObject NameTagItem;
    private GameObject NameTag3DItem;


    public const string TOUCH_FIX_REWARD_TIMEKEY = "TOUCH_FIX_REWARD_TIMEKEY";
    private SingleAssignmentDisposable[] TouchFixRewardTimeDisposable;


    public ManagementWorldManager worldManager
    {
        get { return _managementWorldManager; }
        set { _managementWorldManager = value; }
    }

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
        
        pageRootObject = cUIService.GetContentsRootObject((int)SceneUIID.MANAGEMENT_UI);
        CDebug.Log($"{GetType()} PageRootObject : {pageRootObject}");

        //_managementWorldManager = GameObject.Find("Management").GetComponent<ManagementWorldManager>();
        WorldMgrObj = GameObject.Find("Management");
        _managementWorldManager = WorldMgrObj.GetComponent<ManagementWorldManager>();
        _managementWorldManager.Initialize();

        //Debug.Log("Management Manager = "+ _managementWorldManager);

        var canvas = pageRootObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = CDirector.Instance.ui_camera;

        TouchFixRewardTimeDisposable = new SingleAssignmentDisposable[4];
    }

    public override void ChangeProcess(ProcessStep _step)
    {
        processStep = _step;

        CDebug.Log($"{GetType()} ChangeProcess {processStep.ToString()} ");

        switch (_step)
        {
            case ProcessStep.Start:
                if (pageRootObject == null)
                {
                    throw new Exception($"{GetType()} : RootObject is null");
                }

                ChangeProcess(ProcessStep.Step1);
                break;
            case ProcessStep.Step1:

                canvas_manager = pageRootObject.GetComponent<ManagementCanvasManager>();
                var assetService = CCoreServices.GetCoreService<CAssetService>();

                var pageManager = NavigationManager.GetPageManager(SceneID.MANAGEMENT_SCENE);
                pageManager.SetPageBackProcess(PageManager.BackProcessType.BACK_STACKORDER);


                pageManager.AddPage<ManagementPageUI>(canvas_manager.GetSafeAreaObject(), 
                    assetService.Get_Object_Info(AssetPathDefine.TRANSACTION_MANAGEMENT_UI_PREFAB));

                pageManager.AddPage<PageAddSection>(canvas_manager.GetSafeAreaObject(),
                    assetService.Get_Object_Info(AssetPathDefine.TRANSACTION_MANAGEMENT_UI_ADDSECTION_PREFAB));

                pageManager.AddPage<PageAddSectionSelectPosition>(canvas_manager.GetSafeAreaObject(),
                    assetService.Get_Object_Info(AssetPathDefine.TRANSACTION_MANAGEMENT_UI_ADDSECTION_SELECTPOSITION_PREFAB));

                pageManager.AddPage<PageTraining>(canvas_manager.GetSafeAreaObject(),
                    assetService.Get_Object_Info(AssetPathDefine.TRANSACTION_MANAGEMENT_UI_TRAINING_PREFAB));

                pageManager.AddPage<PagePhotoStudio>(canvas_manager.GetSafeAreaObject(),
                    assetService.Get_Object_Info(AssetPathDefine.TRANSACTION_MANAGEMENT_UI_PHOTOSTUDIO_PREFAB));

                // 아카이브
                pageManager.AddPage<PageArchiveUI>(canvas_manager.GetSafeAreaObject(),
                    assetService.Get_Object_Info(AssetPathDefine.TRANSACTION_MANAGEMENT_UI_ARCHIVE_PREFAB));
                // 컨디션
                pageManager.AddPage<PageCondition> (canvas_manager.GetSafeAreaObject (),
                    assetService.Get_Object_Info (AssetPathDefine.TRANSACTION_MANAGEMENT_UI_CONDITION_PREFAB));
                // 트렌디 (진화석)
                pageManager.AddPage<PageTrendy> (canvas_manager.GetSafeAreaObject (),
                    assetService.Get_Object_Info (AssetPathDefine.TRANSACTION_MANAGEMENT_UI_TRENDY_PREFAB));

                pageManager.AddPage<PageMoveSection>(canvas_manager.GetSafeAreaObject(),
                    assetService.Get_Object_Info(AssetPathDefine.TRANSACTION_MANAGEMENT_UI_MOVESECTION_PREFAB));
                
                CreateLayer(ref IntraMailLayer, assetService, "IntraMailEffect_Layer");
				CreateLayer(ref EventIconLayer, assetService, "CharIcon_EventLayer");
				CreateLayer(ref BubbleLayer, assetService, "CharIcon_BubbleLayer");
                CreateLayer(ref TouchLayer, assetService, "CharIcon_TouchLayer");
                CreateLayer(ref NameTagLayer, assetService, "CharIcon_NameLayer");
                CreateLayer(ref ArchiveTrophyInfoLayer, assetService, "Archive_TrophyInfoLayer");

                ChangeProcess(ProcessStep.Step2);
                break;

            case ProcessStep.Step2:

                NetworkLoadingProgress.AddIgnore();
                NetworkLoadingProgress.HideDog();

                //APIHelper.MainPage.IntraMail_Top()
                //.Subscribe(res =>
                //{
                    APIHelper.ManagementSvc.Management_Layer_All()
                    .Do(res =>
                    {
                        _managementWorldManager.UpdateLayerAllResponseData(res);

                        //아이템 획득 알림
                        NotifiItemGainDataManager.Instance.SetNotifiItemGainData(res.itemGainNoticeInfos);

                    })
                    .Do(_ => NetworkLoadingProgress.RemoveIgnore())
                    .Do(_ => NetworkLoadingProgress.ShowDog())
                    .Do(_ => ChangeProcess(ProcessStep.Step3))
                    .Subscribe()
                    .AddTo(this);
                //})
                //.AddTo(this);

				break;

            case ProcessStep.Step3:
                foreach (AVATAR_TYPE type in CDefines.VIEW_AVATAR_TYPE)
                {
                    CAvatar avatar = CPlayer.GetAvatar(type);
                    ManagementManager.Instance.SetTouchEvtFixRewardTime(avatar);
                }
                canvas_manager.Initialize( this );

                _managementWorldManager.ChangeState(ManagementWorldState_Enter.Instance());

                CDebug.Log($"{GetType()} Wait for Management setup to complete", CDebugTag.SCENE);

                SingleAssignmentDisposable disposable = new SingleAssignmentDisposable();
                disposable.Disposable = Observable.EveryUpdate().Select(_ => ManagementWorldState_Enter.Instance())
                    .Where(state => state.AsyncLoadComplete).Subscribe(d =>
                    {
                        ChangeProcess(ProcessStep.Step4);
                        disposable.Dispose();

                    });
                break;

            case ProcessStep.Step4:
                var fxuilayer = CStaticCanvas.Instance.GetLayer<FXUILayer>(FXUILayer.NAME);
                fxuilayer.InitProductsFlyingEffectObject();

                // 팝업 스토어 이월된 생산품 회수 체크
                _managementWorldManager.CheckGatherForCarryOverGoods();

                ChangeProcess(ProcessStep.Complete);
                break;

            case ProcessStep.Complete:
				break;
		}
    }





    /// <summary>
    /// ProcessStep 이 ProcessStep.Complete 되었을때 호출
    /// </summary>
    /// <returns></returns>
    public override IObservable<Unit> DidFinishProcessObservable()
    {
        return Observable.Create<Unit>(observer =>
        {
            CDebug.Log($"{GetType()} DidFinishProcessObservable", CDebugTag.SCENE);

            SoundManager.Instance.PlayBGMSound(ESOUND_BGM.ID_MANAGEMNET);

            SetStartingContent();

            observer.OnNext(Unit.Default);
            observer.OnCompleted();

            return Disposable.Empty;
        });
    }






    public override void Release()
    {
        CDebug.Log($"{GetType()} Release ");

        var fxlayer = CStaticCanvas.Instance.GetLayer<FXUILayer>(FXUILayer.NAME);
        fxlayer.ReleaseEffectObjects();

        NavigationManager.GetPageManager(SceneID.MANAGEMENT_SCENE).Release();
        canvas_manager.Release();
        canvas_manager = null;

        if(worldManager != null)
        {
            Destroy(worldManager);
        }

        if (_managementWorldManager != null)
        {
            //_managementWorldManager.Release();
            _managementWorldManager = null;
        }

        //DestroyImmediate(WorldMgrObj);
    }




#endregion




    public void CreateLayer(ref GameObject target, CAssetService assetService, string name)
	{
		target = Utility.AddChild
            (
            canvas_manager.GetSafeAreaObject(), 
            assetService.Get_Object_Info(AssetPathDefine.TRANSACTION_MANAGEMENT_UI_CHAR_ICON_LAYERGROUP1_PREFAB)
            ); 
        target.name = name;                
        target.SetActive(true); 
		target.transform.SetAsFirstSibling();
	}

    public BaseSceneCanvasManager GetCanvasManager()
    {
        return canvas_manager;
    }

	public GameObject GetIntraMailEffectLayer()
	{
		return IntraMailLayer;
	}

	public GameObject GetEventIconLayer()
	{
		return EventIconLayer;
	}

	public GameObject GetBubbleLayer()
	{
		return BubbleLayer;
	}

    public GameObject GetTouchLayer()
    {
        return TouchLayer;
    }

	public GameObject GetNameTagLayer()
	{
		return NameTagLayer;
	}

    public GameObject GetArchiveTrophyLayer ()
    {
        return ArchiveTrophyInfoLayer;
    }

    private void InitNameTag()
	{
        string NameTagItemStringIdx = "UI/Management/Prefabs/obj_management_nametag.prefab";

        var resData = CResourceManager.Instance.GetResourceData(NameTagItemStringIdx);
		NameTagItem = resData.Load<GameObject>(gameObject);
	}

	public GameObject AddNameTag(GameObject uiDummy, string name)
	{
		if(NameTagItem == null)
		{
			InitNameTag();
		}

        GameObject insNameTagItem = Utility.AddChild(GetNameTagLayer(), NameTagItem);
        var nameTagItem = insNameTagItem.GetComponent<ManagementNameTag>();
        nameTagItem.Initialize();
        nameTagItem.SetData(name);
        var canvasRect = GetCanvasManager().GetComponent<RectTransform>();
        var fop = nameTagItem.gameObject.AddComponent<FollowObjectPositionFor2D>();
        fop.Init(canvasRect, uiDummy.transform);
        return insNameTagItem;
	}

    public GameObject AddNameTag(GameObject uiDummy, string name, int lv)
    {
        if (NameTagItem == null)
        {
            InitNameTag();
        }

        GameObject insNameTagItem = Utility.AddChild(GetNameTagLayer(), NameTagItem);
        var nameTagItem = insNameTagItem.GetComponent<ManagementNameTag>();
        nameTagItem.Initialize();
        nameTagItem.SetData(name, lv);
        var canvasRect = GetCanvasManager().GetComponent<RectTransform>();
        var fop = nameTagItem.gameObject.AddComponent<FollowObjectPositionFor2D>();
        fop.Init(canvasRect, uiDummy.transform);
        return insNameTagItem;
    }


    private void InitNameTag3D()
    {
        string NameTagItemStringIdx = "Views/Management/BG/Open/Prefabs/management_user_name_3d.prefab";

        var resData = CResourceManager.Instance.GetResourceData(NameTagItemStringIdx);
        NameTag3DItem = resData.Load<GameObject>(gameObject);
    }

    public GameObject AddNameTag3D(GameObject uiDummy, string name, int lv = 0)
    {
        if (NameTag3DItem == null)
        {
            InitNameTag3D();
        }

        GameObject insNameTagItem = Utility.AddChild(uiDummy, NameTag3DItem);
        ManagementNameTag3D namgTag3D = insNameTagItem.AddComponent<ManagementNameTag3D>();
        namgTag3D.Init(name, worldManager.GetMainCameraObj(), lv);
        return insNameTagItem;
    }


    //below use only Management

    public void SetTouchEvtFixRewardTime(CAvatar avatar)
    {
        double timeSeconds = TimeSpan.FromMilliseconds(avatar.touch_fix_reward_remain_mts).TotalSeconds;

        string streamKey = TOUCH_FIX_REWARD_TIMEKEY + "_" + avatar.GetAvatarType();

        if (GlobalTimer.Instance.HasTimeStream(streamKey) == false)
        {
            TimeStream timeStream = GlobalTimer.Instance.GetTimeStream(out timeStream, streamKey) as TimeStream;

            if (timeStream != null)
            {
                timeStream.SetTime(streamKey, (float)timeSeconds, 0, TimeStreamType.DECREASE).OnTimeStreamObservable();
            }
            else
            {
                CDebug.Log("SetTouchEvtFixRewardTime() timestream is null");
            }
        }
    }


    public void CheckTouchEvtFixRewardTime(CAvatar avatar)
    {
        ManagementPageUI pageUI = ManagementManager.Instance.worldManager.GetPageUIManager();
        AVATAR_TYPE aType = avatar.GetAvatarType();
        int _avatarIdx = (int)aType - 1;
        DisposeTouchEvtFixRewardTime(aType);

        TouchFixRewardTimeDisposable[_avatarIdx] = new SingleAssignmentDisposable();

        string streamKey = TOUCH_FIX_REWARD_TIMEKEY + "_" + avatar.GetAvatarType();

        if (GlobalTimer.Instance.HasTimeStream(streamKey))
        {
            TimeStream timeStream = GlobalTimer.Instance.GetTimeStream(out timeStream, streamKey);
            if (timeStream != null)
            {
                TouchFixRewardTimeDisposable[_avatarIdx].Disposable = timeStream.OnTimeStreamObservable()
                    .Subscribe(streamData =>
                    {
                        avatar.TouchEvtFixReward_CurTime = (float)TimeSpan.FromSeconds(streamData.CurrentTime).TotalSeconds;
                        //CDebug.Log($"   *** AVATAR_TYPE = {aType}, curTime = {avatar.TouchEvtFixReward_CurTime}");//
                        //pageUI.avatarUI.SetHideTouchEvtRewardIconNotice (aType);

                        if (streamData.IsEnd)
                        {
                            //pageUI.avatarUI.SetAvatarCanReceiveTouchEvtFixReward (avatar, aType);
                            DisposeTouchEvtFixRewardTime(aType);
                            GlobalTimer.Instance.RemoveTimeStream(streamKey);
                        }
                    }).AddTo(ManagementManager.Instance.worldManager);
            }
            else
            {
                CDebug.Log("CheckTouchEvtFixRewardTime() timestream is null");
            }
        }
        //else
        //{
        //    if (avatar.touch_fix_reward_remain_mts <= 0)
        //    {
        //        pageUI.avatarUI.SetAvatarCanReceiveTouchEvtFixReward(avatar, aType);
        //    }
        //}
    }

    public void DisposeTouchEvtFixRewardTime(AVATAR_TYPE aType)
    {
        if (Instance == null) return;
        int _avatarIdx = (int)aType - 1;
        if (TouchFixRewardTimeDisposable != null)
        {
            if (TouchFixRewardTimeDisposable[_avatarIdx] != null)
            {
                TouchFixRewardTimeDisposable[_avatarIdx].Dispose();
            }
        }
    }


    public void LeaveScene()
    {
        scene.SetSceneState(CBaseSceneExtends.SceneState.Leave);
    }

    private void SetStartingContent()
    {
        NavigationData tData = (NavigationData)NavigationManager.GetNavigationData();
        if (tData.shortCutLinkData != null)
        {
            switch(tData.shortCutType)
            {
                case SHORTCUT_TYPE.GOTO_MANAGEMENT:
                    break;
                case SHORTCUT_TYPE.GOTO_MANAGEMENT_NEW_SECTION:
                    ShortCutManager.Instance.ShowAddSectionPageForShortCut(tData.shortCutLinkData.Value1);
                    break;
                case SHORTCUT_TYPE.GOTO_MANAGEMENT_SELECT_SECTION:
                    ShortCutManager.Instance.SelectSectionForShortCut(tData.shortCutLinkData);
                    break;
                case SHORTCUT_TYPE.GOTO_MANAGEMENT_PRACTICE_SECTION:
                    ShortCutManager.Instance.ShowPracticeSectionShortCut();
                    break;
                case SHORTCUT_TYPE.GOTO_MANAGEMENT_NPC:
                    {
                        if (!TutorialManager.Instance.IsRunTutorial)
                        {
                            ShortCutManager.Instance.SelectManagementNPC(tData.shortCutLinkData);
                        }
                    }
                    break;
            }

        }
    }

    // 매니지먼트 HUD 우편함 상태 변경
    // > UI(Main), AddSection(추가),
    public void ActiveHUDPostBox(bool _isActive)
    {
        /*
         * ManagementPageUI (Main)
         * PageAddSection   (건물 추가)
         * PageArchiveUI    (트로피 전시실)
         * PageCondition    (휴식)
         * PagePhotoShop    (촬영실)
         * PageTraining     (연습실)
         * PageTrendey      (트랜디샵)
         */
        var HUD = CStaticCanvas.Instance.GetLayer<HUDLayer>(HUDLayer.NAME);
        HUD.SetActivePostBox(_isActive);
    }
}