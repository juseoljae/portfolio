using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarbleCanvasManager : MonoBehaviour
{
    [SerializeField]
    private GameObject SafeArea;
    public Camera static_uicamera;
    public Camera uicamera;

    private PageMarble MarblePage;

    private Canvas canvas;
    private MarbleScene scene_instance = null;

    private CAssetService assetService;
    
    public void Initialize(MarbleScene _scene_instance)
    {
        scene_instance = _scene_instance;

        // Test button
        if (SafeArea != null)
        {
            canvas = GetComponent<Canvas>();
            static_uicamera = CStaticCanvas.Instance.Cavans.worldCamera;
        }
    }

    public void SetUIPage(CMarbleWorldManager worldMgr)
    { 
        MarblePage = CResourceManager.Instance.LoadPagePopup<PageMarble>(CPREFAB_KEY.page_marble);
        if (MarblePage != null)
        {
            MarblePage.Init(worldMgr);
        }
        
        if (BarManager.Instance.hud == null)
            BarManager.Instance.CreateHUD();
        BarManager.Instance.FrameTransform(FrameIndex.Last, BarMoveState.BarIn);
    }
    
    public PageMarble GetMarblePage()
    {
        return MarblePage;
    }

    public Canvas GetCanvas() { return canvas; }

    public GameObject GetSafeAreaObject()
    {
        return SafeArea;
    }
}
