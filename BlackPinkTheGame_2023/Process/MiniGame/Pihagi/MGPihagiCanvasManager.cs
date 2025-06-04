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
using UnityEngine;
using UnityEngine.SceneManagement;
using ui.navigation;

public class MGPihagiCanvasManager : BaseSceneCanvasManager
{
    //MGPihagi
    public MGPihagiPageUI PageUI;
    private CPopupService popupService;
    public BPWorldLayerObject LayerObjects;
	private BPWorldSessionExpiredManager sessionTimeoutManager = null;


	public override void Initialize(BaseSceneManager manager)
    {
        CDebug.Log($"{GetType()} Initialize ");
        Manager = manager;
        assetService = CCoreServices.GetCoreService<CAssetService>();
        sceneTransitionService = CCoreServices.GetCoreService<SceneTransitionService>();
        popupService = CCoreServices.GetCoreService<CPopupService>();

        SetStartingContent();

        var assetPathFinder = CCoreServices.GetCoreService<CAssetService>().GetAssetPathFinder();
        var scenename = assetPathFinder.Get_Path(CWorldViewId.MINIGAME_PIHAGI_VIEW).Name;
        scenename = scenename.Split('/').Last().Split('.').FirstOrDefault();
        var scene = SceneManager.GetSceneByName(scenename);
        SceneManager.SetActiveScene(scene);

        SetupLayers(GetSafeAreaObject());

		AttachSessionTimeoutChecker();
		//기획이 나올때까지 꺼둔다
		//var HUD = CStaticCanvas.Instance.GetLayer<HUDLayer>(HUDLayer.NAME);
		//HUD.SetActiveMenu(true);
		//HUD.ShowMenuByType(HUD_MENU_TYPE.MANAGEMENT);
	}

    void SetStartingContent()
    {
        // Todo 
        // 시작 컨텐츠 정보를 받아서 처리한다...

        var pageManager = NavigationManager.GetPageManager(SceneID.MINIGAME_PIHAGI_SCENE);
        var data = pageManager.GetNavigationData();
        CDebug.Log(data.startingContentOrder.ToString());
        CDebug.Log(data.targetSceneID.ToString());
        CDebug.Log(data.prevSceneID.ToString());

        PageUI = pageManager.GetPage<MGPihagiPageUI>((uint)MGPihagiPageUI.LayerOrder);

        var disposer = new SingleAssignmentDisposable();
        disposer.Disposable = pageManager.ShowPageWithoutFade(data.startingContentOrder)
        .Do(layer => {
            CDebug.Log($"MainCanvasManager :: SetStartingContent name  : {layer.Go.name}");
            //layer.Initialize();
        })
        .Subscribe(layer => disposer.Dispose())
        .AddTo(this);
    }


    public void SetupLayers(GameObject safeAreaObject)
    {
        GameObject characterIconlayerObject = assetService.Get_Object_Info(AssetPathDefine.TRANSACTION_MANAGEMENT_UI_CHAR_ICON_LAYERGROUP1_PREFAB);


        GameObject nameLayer = Utility.AddChild(safeAreaObject, characterIconlayerObject);
        nameLayer.name = "CharIcon_NameLayer";
        nameLayer.SetActive(true);
        nameLayer.transform.SetAsFirstSibling();

        GameObject bubbleLayer = Utility.AddChild(safeAreaObject, characterIconlayerObject);
        bubbleLayer.name = "CharIcon_BubbleLayer";
        bubbleLayer.SetActive(true);
        bubbleLayer.transform.SetAsFirstSibling();

        LayerObjects = new BPWorldLayerObject(bubbleLayer, nameLayer, null);
    }

	private void AttachSessionTimeoutChecker()
	{
		GameObject attachObject = new GameObject( "BPWorldSessionTimeoutManager(Dynamic Create)" );
		attachObject.transform.SetParent( transform );

		sessionTimeoutManager = GameObjectHelperUtils.GetOrAddComponent<BPWorldSessionExpiredManager>( attachObject );
		sessionTimeoutManager.SetActive_TimeoutManager( true );
	}


	public override void Release()
    {
		//if(PageUI != null)
		//{
		//}
		//if(goPageCardInventory != null)
		//{
		//    goPageCardInventory.GetComponent<CardUIInventory>().Release();
		//    goPageCardInventory = null;
		//}

		if( sessionTimeoutManager != null )
		{
			sessionTimeoutManager.SetActive_TimeoutManager( false );
			sessionTimeoutManager = null;
		}
	}

    public CPopupService GetPopupService()
    {
        return popupService;
    }
}
