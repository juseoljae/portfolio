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

using System.Linq;
using UniRx;
using UnityEngine.SceneManagement;
using GroupManagement.ManagementEnums;
using ui.navigation;
using UnityEngine;

public class ManagementCanvasManager : BaseSceneCanvasManager
{
    public ManagementPageUI PageUI;
    private CPopupService popupService;
    private Canvas UICanvas;

    public override void Initialize(BaseSceneManager manager)
    {
        CDebug.Log($"{GetType()} Initialize ");
        Manager = manager;
        assetService = CCoreServices.GetCoreService<CAssetService>();
        sceneTransitionService = CCoreServices.GetCoreService<SceneTransitionService>();
        popupService = CCoreServices.GetCoreService<CPopupService>();
        //SetUIMainObject();
        SetStartingContent();


        var assetPathFinder = CCoreServices.GetCoreService<CAssetService>().GetAssetPathFinder();
        var scenename = assetPathFinder.Get_Path(CWorldViewId.MANAGEMENT_VIEW).Name;
        scenename = scenename.Split('/').Last().Split('.').FirstOrDefault();
        var scene = SceneManager.GetSceneByName(scenename);
        SceneManager.SetActiveScene(scene);


        var HUD = CStaticCanvas.Instance.GetLayer<HUDLayer>(HUDLayer.NAME);
        HUD.SetActiveMenu(true);
        HUD.ShowMenuByType(HUD_MENU_TYPE.MANAGEMENT);

        UICanvas = transform.GetComponent<Canvas>();
    }

    void SetStartingContent()
    {
        // Todo 
        // 시작 컨텐츠 정보를 받아서 처리한다...

        var pageManager = NavigationManager.GetPageManager(SceneID.MANAGEMENT_SCENE);
        var data = pageManager.GetNavigationData();
        CDebug.Log(data.startingContentOrder.ToString());
        CDebug.Log(data.targetSceneID.ToString());
        CDebug.Log(data.prevSceneID.ToString());

        PageUI = pageManager.GetPage<ManagementPageUI>((uint)ManagementPageOrder.Main);

        var disposer = new SingleAssignmentDisposable();
        disposer.Disposable = pageManager.ShowPageWithoutFade(data.startingContentOrder)
        .Do(layer => {
            CDebug.Log($"MainCanvasManager :: SetStartingContent name  : {layer.Go.name}");
            //layer.Initialize();
        })
        .Subscribe(layer => disposer.Dispose())
        .AddTo(this);
    }

    ManagementManager GetManagementManager()
    {
        return (ManagementManager)Manager;
    }

    public Canvas GetCanvas()
    {
        return UICanvas;
    }

    //void SetUIMainObject()
    //{
    //    MainObject = Utility.AddChild(GetSafeAreaObject(), assetService.Get_Object_Info(AssetPathDefine.TRANSACTION_MANAGEMENT_UI_PREFAB));

    //    RectTransform rectTransform = MainObject.GetComponent<RectTransform>();
    //    rectTransform.offsetMin = Vector2.zero;
    //    rectTransform.offsetMax = Vector2.zero;
    //    mainUI = MainObject.AddComponent<ManagementMainUI>();
    //    mainUI.Initialize();

    //}

    public override void Release()
    {
        //if(goPageCardInventory != null)
        //{
        //    goPageCardInventory.GetComponent<CardUIInventory>().Release();
        //    goPageCardInventory = null;
        //}
    }

    public CPopupService GetPopupService()
    {
        return popupService;
    }

   

}
