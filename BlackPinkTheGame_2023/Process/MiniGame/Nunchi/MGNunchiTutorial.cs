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

using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;
using System.Threading;
public enum TutorialStep
{
    NONE = 0,
    STEP_1,
    STEP_2,
    STEP_3,
}



public class MGNunchiTutorial
{
    private TutorialSequenceProcedure tsp = null;

    private SkinnedMeshRenderer MidPlatRender;    
    RectTransform waveTarget = null;//3d오브젝트 용 Wave

    private RectTransform PlayerBox;
    private MGNunchiWorldManager WorldMgr;

    //Turorial
    private TutorialStep tutorialStep;

    public void Initialize_Step1(SkinnedMeshRenderer midPlatRender, MGNunchiWorldManager worldMgr)
    {
        tsp = new TutorialSequenceProcedure();

        MidPlatRender = midPlatRender;
        WorldMgr = worldMgr;

        SetTutorialStep(TutorialStep.STEP_1);

        TutorialManager.Instance.Show();
        TutorialManager.Instance.CurrentTutorialRandomID = (long)TUTORIAL_IDS.MINIGAME_NUNCHI;
        SetTutorial_Step1_01();
    }

    public void Initialize_Step2(RectTransform playerBox)
    {
        if (TutorialManager.Instance.CurrentTutorialRandomID == 0) return;

        PlayerBox = playerBox;

        SetTutorialStep(TutorialStep.STEP_2);

        SetTutorial_Step2().AttachExternalCancellation(cancellationToken:tsp.disable_ctk);
    }


    private void SetTutorial_Step1_01()
    {
        if (TutorialManager.Instance.CurrentTutorialRandomID == 0) return;

        TutorialManager.Instance.Show();
        TutorialManager.Instance.AssetController.DimRaycast(true);
        TutorialManager.Instance.AssetController.DimAlpha(true);

        SingleAssignmentDisposable disposable = new SingleAssignmentDisposable();
        disposable.Disposable = tsp.EachOneCompleteAsObservable((long)TUTORIAL_IDS.MINIGAME_NUNCHI, false)
            .Subscribe(_ =>
            {
                disposable.Dispose();

            }).AddTo(cancellationToken: tsp.disable_ctk);

        AffordanceStruct as1 = new AffordanceStruct()
        {
            affordanceType = AffordanceType.DIALOGUE,
            dialogue = new TutorialDialogue(TutorialDialogue.Type.LEFT, 943007001, Constants.NPC_TWINKEY_joy),
            targetTransform = null,
            affordanceTouch = AffordanceType.DIM,
            affordanceCallback = null,
        };

        tsp.CurrentTaskExcuteAsObservable(new TutorialTask(as1)).Subscribe(_ =>
        {
            SetTutorial_Step1_02Async().Forget();

        }).AddTo(cancellationToken: tsp.disable_ctk);

        CDebug.Log("  $$$$$$ SetTutorial_Step1_01() ");
    }

    private async UniTask SetTutorial_Step1_02Async()
    {
        Canvas _uiCanvas = CStaticCanvas.Instance.gameObject.GetComponent<Canvas>();
        Camera _mainCam = WorldMgr.GetMainCamera();
        var postion = RectTransformExtensions.ConvertPositionToAnchoredPosition( MidPlatRender.gameObject.transform.position, _mainCam );
        //postion.y = postion.y + 5f;
        Transform3DInfo _info = new Transform3DInfo( _uiCanvas, MidPlatRender.gameObject, _mainCam, postion );

        GameObject objWaveTarget = new GameObject( "wave" );
        waveTarget = objWaveTarget.AddComponent<RectTransform>();
        waveTarget.anchorMin = new Vector2( 0.5f, 0.5f );
        waveTarget.anchorMax = new Vector2( 0.5f, 0.5f );
        waveTarget.pivot = new Vector2( 0.5f, 0.5f );

        var layer = CStaticCanvas.Instance.GetLayer<TutorialLayer>( TutorialLayer.NAME );
        objWaveTarget.transform.parent = layer.transform.GetChild( 0 );
        objWaveTarget.transform.localPosition = new Vector3( postion.x, postion.y, 0f );
        objWaveTarget.transform.localScale = Vector3.one;
        objWaveTarget.transform.rotation = Quaternion.identity;

        var disposable = tsp.CurrentTaskExcuteAsObservable(new TutorialTask(
            new AffordanceStruct()
            {
                affordanceType = AffordanceType.DIALOGUE,
                dialogue = new TutorialDialogue(TutorialDialogue.Type.LEFT, 943007002, Constants.NPC_TWINKEY_point02),
            }
            )).Subscribe().AddTo(cancellationToken: tsp.disable_ctk);

        await UniTask.Delay (2000, cancellationToken: tsp.disable_ctk);

        await tsp.CurrentTaskExcuteAsObservable (new TutorialTask (
            new AffordanceStruct()
            {
                affordanceType = AffordanceType.AFFORDANCE_WAVE,
                targetTransform = waveTarget,
                affordanceTouch = AffordanceType.AFFORDANCE_WAVE,
                affordanceCallback = () =>
                {
                    TutorialManager.Instance.Hide ();
                    if (WorldMgr != null)
                    {
                        WorldMgr.SetTutorial_SelectMapIndex(4);
                        WorldMgr.SetChangeState(MGNunchiGameState_InPlayingGame.Instance());
                    }
                    else
                    {
                        TutorialManager.Instance.ReleaseCancelTokenSource();
                    }
                }
            },
            new AffordanceStruct ()
            {
                affordanceType = AffordanceType.CIRCLE_UNMASK,
                target3D = _info,
            }
            )).ToUniTask (cancellationToken: tsp.disable_ctk);

        if( disposable != null )
        {
            disposable.Dispose();
        }

    }


    private async UniTask SetTutorial_Step2()
    {
        TutorialManager.Instance.Show();
        TutorialManager.Instance.AssetController.DimRaycast(true);
        TutorialManager.Instance.AssetController.DimAlpha(true);

        await UniTask.WaitForEndOfFrame(cancellationToken: tsp.disable_ctk);

        await tsp.CurrentTaskExcuteAsObservable(new TutorialTask (
            new AffordanceStruct()
            {
                affordanceType = AffordanceType.RECT_UNMASK,
                targetTransform = PlayerBox,
            },
            new AffordanceStruct()
            {
                affordanceType = AffordanceType.DIALOGUE,
                dialogue = new TutorialDialogue(TutorialDialogue.Type.LEFT, 943007003, Constants.NPC_TWINKEY_point02),
                affordanceTouch = AffordanceType.DIM,

            }
            )).ToUniTask(cancellationToken: tsp.disable_ctk);

        await UniTask.WaitForEndOfFrame(cancellationToken: tsp.disable_ctk);

        SetTutorialStep(TutorialStep.NONE);
        WorldMgr.SetTutorialDone();

        TutorialManager.Instance.CurrentTutorialRandomID = 0;
        TutorialManager.Instance.AssetController.DimRaycast(false);
        TutorialManager.Instance.AssetController.DimAlpha(false);
        TutorialManager.Instance.Hide();

        ReleaseTutorial();

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
        if (tsp != null) tsp = null;
        if (MidPlatRender != null) MidPlatRender = null;
        if (PlayerBox != null) PlayerBox = null;
        if (WorldMgr != null) WorldMgr = null;

        TutorialManager.Instance.ReleaseCancelTokenSource();

    }
}
