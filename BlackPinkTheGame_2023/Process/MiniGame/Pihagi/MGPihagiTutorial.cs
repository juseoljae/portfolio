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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using ui.navigation;
using Cysharp.Threading.Tasks;
using System.Threading;


public class MGPihagiTutorial
{
    private MGPihagiWorldManager WorldMgr;
    private TutorialSequenceProcedure tsp = null;
    private static long TutorialID => (long)TUTORIAL_IDS.MINIGAME_PIHAGI;

    private RectTransform PlayerBox;
    private RectTransform JoyStickBox;
    //private RectTransform ButtonGroupBox;

    private TutorialStep tutorialStep;


    public void Initialize_Step1(MGPihagiWorldManager worldMgr)
    {
        tsp = new TutorialSequenceProcedure();
        WorldMgr = worldMgr;
        TutorialManager.Instance.CurrentTutorialRandomID = TutorialID;

        SingleAssignmentDisposable dispose = new SingleAssignmentDisposable();
        dispose.Disposable = tsp.EachOneCompleteAsObservable(TutorialID, false).Subscribe(_ =>
        {
            dispose.Dispose();

        }).AddTo(cancellationToken: tsp.disable_ctk);

        TutorialManager.Instance.Show();
        TutorialManager.Instance.AssetController.DimRaycast(true);
        TutorialManager.Instance.AssetController.DimAlpha(true);
        SetTutorialStep(TutorialStep.STEP_1);

        TutorialManager.Instance.Show();

        SetTutorial_Step_CommonDialog(943009001, Constants.NPC_TWINKEY_action01);
    }

    public void Initialize_Step2()
    {
        //튜토리얼 중에 나가지면 플레이 하지 않는다
        if (TutorialManager.Instance.CurrentTutorialRandomID == 0) return;

        SetTutorialStep(TutorialStep.STEP_2);

        TutorialManager.Instance.Show();

        SetTutorial_Step_CommonDialog(943009002, Constants.NPC_TWINKEY_point02);
    }

    public void Initialize_Step3(RectTransform playerBox, RectTransform joyStickBox/*, RectTransform btnBox*/)
    {
        //튜토리얼 중에 나가지면 플레이 하지 않는다
        if (TutorialManager.Instance.CurrentTutorialRandomID == 0) return;

        PlayerBox = playerBox;
        JoyStickBox = joyStickBox;
        //ButtonGroupBox = btnBox;

        SetTutorialStep(TutorialStep.STEP_3);

        TutorialManager.Instance.Show();

        SetTutorial_Step3_01();
    }

    public void Initialize_Step4()
    {
        //튜토리얼 중에 나가지면 플레이 하지 않는다
        if (TutorialManager.Instance.CurrentTutorialRandomID == 0) return;

        TutorialManager.Instance.Show();
        SetTutorial_Step4().Forget();
    }

    //use below step1, step2
    private void SetTutorial_Step_CommonDialog(long strID, string tweenkeyFace)
    {
        //튜토리얼 중에 나가지면 플레이 하지 않는다
        if (TutorialManager.Instance.CurrentTutorialRandomID == 0) return;

        AffordanceStruct as1 = new AffordanceStruct()
        {
            affordanceType = AffordanceType.DIALOGUE,
            dialogue = new TutorialDialogue(TutorialDialogue.Type.LEFT, strID, tweenkeyFace),
            targetTransform = null,
            affordanceTouch = AffordanceType.DIM,
            affordanceCallback = null,
        };

        tsp.CurrentTaskExcuteAsObservable(new TutorialTask(as1)).Subscribe(_ =>
        {
            TCPMGPihagiRequestManager.Instance.Req_GamePlayResume();
            TutorialManager.Instance.Hide();

        }).AddTo(cancellationToken: tsp.disable_ctk);

        CDebug.Log("  $$$$$$ SetTutorial_Step1() ");
    }

    private void SetTutorial_Step3_01()
    {
        //튜토리얼 중에 나가지면 플레이 하지 않는다
        if (TutorialManager.Instance.CurrentTutorialRandomID == 0) return;

        AffordanceStruct as1 = new AffordanceStruct()
        {
            affordanceType = AffordanceType.RECT_UNMASK,
            targetTransform = PlayerBox,
            affordanceTouch = AffordanceType.DIM,
        };
        AffordanceStruct as2 = new AffordanceStruct()
        {
            affordanceType = AffordanceType.DIALOGUE,
            dialogue = new TutorialDialogue( TutorialDialogue.Type.LEFT, 943009003, Constants.NPC_TWINKEY_point02 ),
            targetTransform = null,
            affordanceTouch = AffordanceType.DIM,
            affordanceCallback = () =>
            {
            },
        };

        TutorialTask task3 = new TutorialTask( as1, as2);
    
        tsp.CurrentTaskExcuteAsObservable( /*new TutorialTask( as1 )*/task3).Subscribe(_ =>
        {
            //SetTutorial_Step3_02();
            TutorialManager.Instance.Hide();
            //조이스틱 슬라이드 등장
            WorldMgr.SetTutorial_Step3_AppearUI();

        }).AddTo(cancellationToken: tsp.disable_ctk);
    }

    private void SetTutorial_Step3_02()
    {
        //튜토리얼 중에 나가지면 플레이 하지 않는다
        if (TutorialManager.Instance.CurrentTutorialRandomID == 0) return;

        AffordanceStruct as2 = new AffordanceStruct()
        {
            affordanceType = AffordanceType.DIALOGUE,
            dialogue = new TutorialDialogue(TutorialDialogue.Type.LEFT, 943009003, Constants.NPC_TWINKEY_point02),
            targetTransform = null,
            affordanceTouch = AffordanceType.DIM,
            affordanceCallback = () =>
            {
            },
        };

        tsp.CurrentTaskExcuteAsObservable(new TutorialTask(as2)).Subscribe(_ =>
        {
            TutorialManager.Instance.Hide();
            //조이스틱 슬라이드 등장
            WorldMgr.SetTutorial_Step3_AppearUI();
        }).AddTo(cancellationToken: tsp.disable_ctk);

    }

    private async UniTask SetTutorial_Step4()
    {
        //튜토리얼 중에 나가지면 플레이 하지 않는다
        if (TutorialManager.Instance.CurrentTutorialRandomID == 0) return;

        await tsp.CurrentTaskExcuteAsObservable(new TutorialTask(
            new AffordanceStruct()
            {
                affordanceType = AffordanceType.DIALOGUE,
                dialogue = new TutorialDialogue(TutorialDialogue.Type.LEFT, 943009004, Constants.NPC_TWINKEY_joy),
                targetTransform = null,
                affordanceTouch = AffordanceType.DIM,

            })).ToUniTask(cancellationToken: tsp.disable_ctk);


        SetTutorialStep(TutorialStep.NONE);

        TutorialManager.Instance.CurrentTutorialRandomID = 0;
        TutorialManager.Instance.AssetController.DimRaycast(false);
        TutorialManager.Instance.AssetController.DimAlpha(false);
        TutorialManager.Instance.Hide();

        WorldMgr.SetTutorialDone();
        ReleaseTutorial();
        CDebug.Log("      ********** Finish MiniGame Play Tutorial");


    }

    public void SetTutorialStep(TutorialStep step)
    {
        tutorialStep = step;
    }

    public TutorialStep GetTutorialStep()
    {
        return tutorialStep;
    }


    public void ReleaseTutorial()
    {
        TutorialManager.Instance.ReleaseCancelTokenSource();
    }
}
