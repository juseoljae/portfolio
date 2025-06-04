#define _DEBUG_MSG_ENABLED
#define _DEBUG_WARN_ENABLED
#define _DEBUG_ERROR_ENABLED

using UnityEngine;
using UniRx;

public class ManagementDirector : BaseDirector
{
    private static ManagementDirector instance = null;
    private CWorldViewFactory worldViewFactory;
    private static GameObject cardBlockPoolContainer;

    public static ManagementDirector Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject director_gameobject = new GameObject(CDefines.MANAGEMENT_DIRECTOR_NAME);
                instance = director_gameobject.AddComponent<ManagementDirector>();

                cardBlockPoolContainer = new GameObject("cardBlockPoolContainer");
                cardBlockPoolContainer.transform.SetParent(instance.transform);
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
        //CDataManager.RxManagementDataLoad()
        //.Do(_ => {


        ///------------------------ register ----------------------------------
        TransactionRegister();


        //-------------------- 2D Canvas Renderer Scene --------------------------------------
        TransactionUI(SceneUIID: SceneUIID.MANAGEMENT_UI);



        //-------------------- 3D View Scene --------------------------------------
        TransactionView(CWorldViewId.MANAGEMENT_VIEW);



        //--------------------- Object  -------------------------------------
        TransactionObject(CDefinesID: AssetPathDefine.TRANSACTION_MANAGEMENT_UI_PREFAB);
        TransactionObject(CDefinesID: AssetPathDefine.TRANSACTION_MANAGEMENT_UI_ADDSECTION_PREFAB);
        TransactionObject(CDefinesID: AssetPathDefine.TRANSACTION_MANAGEMENT_UI_ADDSECTION_SELECTPOSITION_PREFAB);
        TransactionObject(CDefinesID: AssetPathDefine.TRANSACTION_MANAGEMENT_UI_TRAINING_PREFAB);
        TransactionObject(CDefinesID: AssetPathDefine.TRANSACTION_MANAGEMENT_UI_PHOTOSTUDIO_PREFAB);
        TransactionObject(CDefinesID: AssetPathDefine.TRANSACTION_MANAGEMENT_UI_ARCHIVE_PREFAB);
        TransactionObject(CDefinesID: AssetPathDefine.TRANSACTION_MANAGEMENT_UI_CONDITION_PREFAB);
        TransactionObject(CDefinesID: AssetPathDefine.TRANSACTION_MANAGEMENT_UI_TRENDY_PREFAB);
        TransactionObject(CDefinesID: AssetPathDefine.TRANSACTION_MANAGEMENT_UI_MOVESECTION_PREFAB);

        TransactionObject(CDefinesID: AssetPathDefine.TRANSACTION_MANAGEMENT_UI_CHAR_ICON_LAYERGROUP1_PREFAB);
        TransactionObject(CDefinesID: AssetPathDefine.TRANSACTION_MANAGEMENT_UI_CHAR_ICON_LAYERGROUP2_PREFAB);
		TransactionObject( CDefinesID: AssetPathDefine.TRANSACTION_MOVIE_PREFAB );
        
        UI.Schedule.puzzle.block.PuzzleBlockGenerator.CardBlockPreload(cardBlockPoolContainer, 5);

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
        uiFactory.DestroyUI((int)SceneUIID.MANAGEMENT_UI);

        //----------------------------------------------------------
        // Remove View.
        //----------------------------------------------------------

        //----------------------------------------------------------
        // Remove Object.
        //----------------------------------------------------------
        assetService.Remove_Object_Info(AssetPathDefine.TRANSACTION_MANAGEMENT_UI_PREFAB);
        assetService.Remove_Object_Info(AssetPathDefine.TRANSACTION_MANAGEMENT_UI_ADDSECTION_PREFAB);
        assetService.Remove_Object_Info(AssetPathDefine.TRANSACTION_MANAGEMENT_UI_ADDSECTION_SELECTPOSITION_PREFAB);
        assetService.Remove_Object_Info(AssetPathDefine.TRANSACTION_MANAGEMENT_UI_TRAINING_PREFAB);
        assetService.Remove_Object_Info(AssetPathDefine.TRANSACTION_MANAGEMENT_UI_PHOTOSTUDIO_PREFAB);
        assetService.Remove_Object_Info(AssetPathDefine.TRANSACTION_MANAGEMENT_UI_ARCHIVE_PREFAB);
        assetService.Remove_Object_Info(AssetPathDefine.TRANSACTION_MANAGEMENT_UI_CONDITION_PREFAB);
        assetService.Remove_Object_Info(AssetPathDefine.TRANSACTION_MANAGEMENT_UI_TRENDY_PREFAB);
        assetService.Remove_Object_Info(AssetPathDefine.TRANSACTION_MANAGEMENT_UI_MOVESECTION_PREFAB);

        assetService.Remove_Object_Info(AssetPathDefine.TRANSACTION_MANAGEMENT_UI_CHAR_ICON_LAYERGROUP1_PREFAB);
        assetService.Remove_Object_Info(AssetPathDefine.TRANSACTION_MANAGEMENT_UI_CHAR_ICON_LAYERGROUP2_PREFAB);
		assetService.Remove_Object_Info( AssetPathDefine.TRANSACTION_MOVIE_PREFAB );

        UI.Schedule.puzzle.block.PuzzleBlockGenerator.ReleaseCardBlockPrefab();

        worldViewFactory.DestroyView(CWorldViewId.MANAGEMENT_VIEW, true);
    }
}
