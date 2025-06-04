#define _DEBUG_MSG_ENABLED
#define _DEBUG_WARN_ENABLED
#define _DEBUG_ERROR_ENABLED

using UnityEngine;
using UniRx;

public class MGNunchiDirector : BaseDirector
{
    private static MGNunchiDirector instance = null;
    private CWorldViewFactory worldViewFactory;
    public static MGNunchiDirector Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject director_gameobject = new GameObject(CDefines.BPW_MINIGAME_NUNCHI_DIRECTOR_NAME);
                instance = director_gameobject.AddComponent<MGNunchiDirector>();
            }

            return instance;
        }
    }

    public override void Prepare()
    {
        base.Prepare();
        worldViewFactory = CCoreFactories.GetCoreFactory<CWorldViewFactory>();
    }

    public override void Start_Enter()
    {

        ///------------------------ register ----------------------------------
        TransactionRegister();


        //-------------------- 2D Canvas Renderer Scene --------------------------------------
        TransactionUI(SceneUIID: SceneUIID.MINIGAME_NUNCHI_UI);



        //-------------------- 3D View Scene --------------------------------------
        TransactionView(CWorldViewId.MINIGAME_NUNCHI_VIEW);



        //--------------------- Object  -------------------------------------
        TransactionObject(CDefinesID: AssetPathDefine.TRANSACTION_MINIGAME_NUNCHI_UI_PREFAB);
        TransactionObject(CDefinesID: AssetPathDefine.TRANSACTION_MANAGEMENT_UI_CHAR_ICON_LAYERGROUP1_PREFAB);


        //------------------------- closed ---------------------------------
        TransactionClosed();

        //})
        //.Subscribe()
        //.AddTo(this);
    }

    //JH
    // 3D View Scene
    void TransactionView(int viewIndex)
    {
        CWorldViewTransaction view_transaction = new CWorldViewTransaction();
        view_transaction.SetInfo(viewIndex, enabledOnLoaded: true, bUseAssetbundle: true);
        transactionExcutor.AddTransaction(view_transaction);
    }

    public override void Finish_Enter()
    {

    }
    public override void Leave()
    {
        base.Leave();

        //----------------------------------------------------------
        // Remove UI.
        //----------------------------------------------------------
        uiFactory.DestroyUI((int)SceneUIID.MINIGAME_NUNCHI_UI);

        //----------------------------------------------------------
        // Remove View.
        //----------------------------------------------------------

        //----------------------------------------------------------
        // Remove Object.
        //----------------------------------------------------------
        assetService.Remove_Object_Info(AssetPathDefine.TRANSACTION_MINIGAME_NUNCHI_UI_PREFAB);
        assetService.Remove_Object_Info(AssetPathDefine.TRANSACTION_MANAGEMENT_UI_CHAR_ICON_LAYERGROUP1_PREFAB);

        worldViewFactory.DestroyView(CWorldViewId.MINIGAME_NUNCHI_VIEW, true);

    }
}
