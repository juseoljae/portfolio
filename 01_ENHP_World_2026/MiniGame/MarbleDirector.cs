using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

public class MarbleDirector : MonoBehaviour, IEntityDirector
{
    private const string DIRECTOR_OBJECT_NAME = "MarbleDirector";
    private static MarbleDirector instance = null;

    private CAssetService assetService;
    private CUIFactory uiFactory;
    private CTransactionExcutor transactionExcutor;
    private CTransactionService transactionService;

    private bool bProcess;
    
    public static MarbleDirector Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject director_gameobject = new GameObject(DIRECTOR_OBJECT_NAME);
                instance = director_gameobject.AddComponent<MarbleDirector>();
            }

            return instance;
        }
    }


    void Awake()
    {
        DontDestroyOnLoad(this);
    }
    void OnDestroy()
    {
#if _DEBUG_MSG_ENABLED
        Debug.Log("OnDestroy() - " + ToString());
#endif
    }


    public void Prepare()
    {
        assetService = CCoreServices.GetCoreService<CAssetService>();
        uiFactory = CCoreFactories.GetCoreFactory<CUIFactory>();
    }

    public void Start_Enter()
    {
        bProcess = false;

        //----------------------------------------------------------
        //
        //----------------------------------------------------------
        TransactionRegister();

        //----------------------------------------------------------
        //
        //----------------------------------------------------------
        TransactionObjectInfo(CDefines.TRANSACTION_MINIGAME_MARBLE_UI_SCENE);
        TransactionObjectInfo(CDefines.TRANSACTION_MINIGAME_MARBLE_UI_PREFAB);
        //----------------------------------------------------------
        //
        //TransactionObjectInfo(CDefines.TRANSACTION_SNG_UITOWNMAIN_PREFAB);

        TransactionObjectInfo(CDefines.TRANSACTION_COMMON_HUD_PREFAB);
        TransactionObjectInfo(CDefines.TRANSACTION_COMMON_BDD_PREFAB);
        //----------------------------------------------------------
        TransactionClosed();

    }


    void TransactionRegister()
    {
        transactionService = CCoreServices.GetCoreService<CTransactionService>();
        transactionExcutor = transactionService.CreateExcutor();
    }


    // 2D Canvas Renderer Scene
    void TransactionUI()
    {
        CUITransaction ui_transaction = new CUITransaction();
        ui_transaction.SetInfo(CUIId.MINIGAME_MARBLE_UI, true);
        transactionExcutor.AddTransaction(ui_transaction);
    }


    // 3D View Scene
    void TransactionView(int viewIndex)
    {
    }

    void AddObjectTransaction(int transction_object_type_index)
    {
        CObjectTransaction object_transaction = new CObjectTransaction();
        object_transaction.SetInfo(transction_object_type_index, true);
        transactionExcutor.AddTransaction(object_transaction);
    }


    void TransactionObjectInfo(int infoType)
    {
        switch (infoType)
        {
            case CDefines.TRANSACTION_MINIGAME_MARBLE_UI_SCENE:    // LOGO UI Scene(2D Canvas Scene)
                TransactionUI();
                break;

            case CDefines.TRANSACTION_MINIGAME_MARBLE_UI_PREFAB:
                //break;

            case CDefines.TRANSACTION_COMMON_HUD_PREFAB:
            case CDefines.TRANSACTION_COMMON_BDD_PREFAB:
                AddObjectTransaction(infoType);
                break;

        }
    }


    void TransactionClosed()
    {
        transactionService.RegisterRenderer<CAssetTransactionRenderer>();
        transactionService.Excute(_enter_transaction_callback);
    }

    private void _enter_transaction_callback()
    {
        bProcess = true;
        transactionService.Finish();
    }

    public bool CompletedTransaction()
    {
        return bProcess;
    }

    public void Finish_Enter()
    {

    }

    public void Leave()
    {
        bProcess = false;

        //----------------------------------------------------------
        // Remove UI.
        //----------------------------------------------------------
        uiFactory.DestroyUI(CUIId.MINIGAME_MARBLE_UI);
    }
}
