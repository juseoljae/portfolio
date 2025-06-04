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

public class MGPihagiManager : BaseSceneManager
{
    //MGPihagi
    #region MEMBER_VAR
    static MGPihagiManager s_this = null;
    public static MGPihagiManager Instance { get { return s_this; } }
    #endregion MEMBER_VAR

    private MGPihagiWorldManager PihagiWorldManager;
    public GameObject WorldMgrObj;
    private GameObject ScoreUILayer;

    public GameObject NameTagLayerObj;

    public ReactiveProperty<bool> bRecvGameEnter;

    public MGPihagiWorldManager WorldManager
    {
        get { return PihagiWorldManager; }
        set { PihagiWorldManager = value; }
    }

#if UNITY_EDITOR
    private BPWorldServerLayer bpworldServerLayer = null;
    private BPWorldEventDispatcher eventMessageDispatcher = null;
    private BPWorldLoadTaskDispatcher loadTaskDispatcher = null;
#endif

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

        pageRootObject = cUIService.GetContentsRootObject((int)SceneUIID.MINIGAME_PIHAGI_UI);
        CDebug.Log($"{GetType()} PageRootObject : {pageRootObject}");

        WorldMgrObj = GameObject.Find("MiniGame_Pihagi");
        PihagiWorldManager = WorldMgrObj.GetComponent<MGPihagiWorldManager>();

        var canvas = pageRootObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = CDirector.Instance.ui_camera;

        bRecvGameEnter = new ReactiveProperty<bool>();

#if UNITY_EDITOR
        bpworldServerLayer = new BPWorldServerLayer();
        eventMessageDispatcher = new BPWorldEventDispatcher();
        loadTaskDispatcher = new BPWorldLoadTaskDispatcher();

        bpworldServerLayer.Initialize(eventMessageDispatcher, loadTaskDispatcher, null);
#endif

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

                canvas_manager = pageRootObject.GetComponent<MGPihagiCanvasManager>();
                var assetService = CCoreServices.GetCoreService<CAssetService>();

                var pageManager = NavigationManager.GetPageManager(SceneID.MINIGAME_PIHAGI_SCENE);
                pageManager.SetPageBackProcess(PageManager.BackProcessType.BACK_STACKORDER);

                pageManager.AddPage<MGPihagiPageUI>(canvas_manager.GetSafeAreaObject(),
                    assetService.Get_Object_Info(AssetPathDefine.TRANSACTION_MINIGAME_PIHAGI_UI_PREFAB));

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

                CDebug.Log("     $$$$$$$$$$$$$        MGNunchiManager.Step2.........SendReqLogin");

                TCPCommonRequestManager.Instance.SendReqLogin();
                SingleAssignmentDisposable checkerDisposer = new SingleAssignmentDisposable();

                checkerDisposer.Disposable = bRecvGameEnter.Where( value => value).Subscribe(_ =>
                {
                    ChangeProcess(ProcessStep.Step3);
                    checkerDisposer.Dispose();
                }).AddTo(this);
                break;

            case ProcessStep.Step3:
                canvas_manager.Initialize(this);
                PihagiWorldManager.Initialize();
                ChangeProcess(ProcessStep.Step4);
                break;

            case ProcessStep.Step4:
                SingleAssignmentDisposable StateDisposer = new SingleAssignmentDisposable();


                CDebug.Log("     $$$$$$$$$$$$$        MGPihagiManager.Step3........." + PihagiWorldManager);
                CDebug.Log("     $$$$$$$$$$$$$        MGPihagiManager.Step3.........111111 ///" + PihagiWorldManager.PihagiMainSM);

                StateDisposer.Disposable = Observable.EveryUpdate()
                .Select(_ => PihagiWorldManager.PihagiMainSM)
                .Where(state =>
                {
                    var ss = state.GetCurrentState();
                    var ff = MGPihagiGameState_Ready.Instance();
                    return ss == ff;
                })
                .Do(state =>
                {
                    CDebug.Log($"             @@@@@@@@@@@@@@   check state = {state.GetCurrentState()}.");
                })
                .First()
                .Subscribe(_ =>
                {
                    SoundManager.Instance.PlayBGMSound(ESOUND_BGM.ID_BPWORLD_MINIGAME2);
                    ChangeProcess(ProcessStep.Complete);
                    StateDisposer.Dispose();
                    StateDisposer = null;
                }).AddTo(this);
                //ChangeProcess(ProcessStep.Complete);
                break;

            case ProcessStep.Complete:
                //WorldManager.SetChangeState(MGPihagiGameState_CameraZoom.Instance());
                break;
        }
    }
    #endregion SceneManager Life Cycle

    public BaseSceneCanvasManager GetCanvasManager()
    {
        return canvas_manager;
    }

    public void CreateLayer(ref GameObject target, CAssetService assetService, string name)
    {
        target = Utility.AddChild
            (
            canvas_manager.GetSafeAreaObject(),
            assetService.Get_Object_Info(AssetPathDefine.TRANSACTION_MINIGAME_PIHAGI_LAYERGROUP1_PREFAB)
            );
        target.name = name;
        target.SetActive(true);
        target.transform.SetAsFirstSibling();
    }


    public override void Release()
    {
        CDebug.Log($"{GetType()} Release ");

        NavigationManager.GetPageManager(SceneID.MINIGAME_PIHAGI_SCENE).Release();
        canvas_manager.Release();
        canvas_manager = null;
        if (PihagiWorldManager != null)
        {
            PihagiWorldManager.Release();
            PihagiWorldManager = null;
        }

		MGPihagiServerDataManager.Instance.Release();

        DestroyImmediate(WorldMgrObj);
    }
}
