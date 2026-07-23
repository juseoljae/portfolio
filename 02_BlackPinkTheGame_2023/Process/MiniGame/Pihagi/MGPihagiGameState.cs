
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
# endif

#endregion


using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using DG.Tweening;


public class MGPihagiGameState_SetServerInfo : CState<MGPihagiWorldManager>
{
    static MGPihagiGameState_SetServerInfo s_this = null;

    public static MGPihagiGameState_SetServerInfo Instance()
    {
        if (s_this == null) s_this = new MGPihagiGameState_SetServerInfo();
        return s_this;
    }

    public override void Enter(MGPihagiWorldManager mgr)
    {
		//when final, fade out loading
		BPWPacketDefine.PihagiGameStageType _stageType = MGPihagiServerDataManager.Instance.StageType;		
		if(_stageType == BPWPacketDefine.PihagiGameStageType.FINAL)
		{
			mgr.CloseForFinal().Subscribe(_ => { });
		}
        mgr.PlayerMgr.SetNetPlayer();
        mgr.PlayerMgr.SetPlayerInfoUI();
        mgr.GetPageUI().SetUp();
        mgr.GetPageUI().SetActiveUI(false);

        mgr.SetChangeState(MGPihagiGameState_Ready.Instance());
    }

    public override void Excute(MGPihagiWorldManager mgr)
    {
    }

    public override void Exit(MGPihagiWorldManager mgr)
    {
    }
}

public class MGPihagiGameState_Ready : CState<MGPihagiWorldManager>
{
    static MGPihagiGameState_Ready s_this = null;

    public static MGPihagiGameState_Ready Instance()
    {
        if (s_this == null) s_this = new MGPihagiGameState_Ready();
        return s_this;
    }

    public override void Enter(MGPihagiWorldManager mgr)
    {
        CDebug.Log( $"      $$$$$$   MGPihagiGameState_Ready.Enter() Stage type : {MGPihagiServerDataManager.Instance.StageType}" );

        //GameLog
        APIHelper.GameLog.MiniGame_Start( (int)BPWPacketDefine.MatchType.PIHAGI_GAME, (int)MGPihagiServerDataManager.Instance.StageType );

        SingleAssignmentDisposable showTitleDisposer = new SingleAssignmentDisposable();
        showTitleDisposer.Disposable = Observable.Timer(TimeSpan.FromSeconds(2))
            .Subscribe(_ =>
            {
                mgr.SetChangeState(MGPihagiGameState_CameraZoom.Instance());
                showTitleDisposer.Dispose();
                showTitleDisposer = null;
            }).AddTo(mgr);
    }

    public override void Excute(MGPihagiWorldManager mgr)
    {
    }

    public override void Exit(MGPihagiWorldManager mgr)
    {
    }
}


public class MGPihagiGameState_CameraZoom : CState<MGPihagiWorldManager>
{
    static MGPihagiGameState_CameraZoom s_this = null;

    public static MGPihagiGameState_CameraZoom Instance()
    {
        if (s_this == null) s_this = new MGPihagiGameState_CameraZoom();
        return s_this;
    }

    public override void Enter(MGPihagiWorldManager mgr)
    {
        //Zoom 끝나면 
        mgr.SetStartCamZoomOut();
    }

    public override void Excute(MGPihagiWorldManager mgr)
    {
    }

    public override void Exit(MGPihagiWorldManager mgr)
    {
    }
}


public class MGPihagiGameState_ShowTitle : CState<MGPihagiWorldManager>
{
    static MGPihagiGameState_ShowTitle s_this = null;

    public static MGPihagiGameState_ShowTitle Instance()
    {
        if (s_this == null) s_this = new MGPihagiGameState_ShowTitle();
        return s_this;
    }

    public override void Enter(MGPihagiWorldManager mgr)
    {
        //예, 결선 표현(1sec)
        mgr.GetPageUI().SetShowTitleStep(MGPihagiPageUI.SHOW_TITLE_STEP.TITLE);
		
		mgr.SetMyPlayerJoyStickStop(false);
        //결승진출 정보 표현(1sec)
        //1sec후 카운트 다운 시작
        SingleAssignmentDisposable showTitleDisposer= new SingleAssignmentDisposable();
        showTitleDisposer.Disposable = Observable.Timer(TimeSpan.FromSeconds(1))
            .Subscribe(_ =>
            {
                mgr.GetPageUI().SetShowTitleStep(MGPihagiPageUI.SHOW_TITLE_STEP.INFO);
                showTitleDisposer.Dispose();
                showTitleDisposer = null;
            }).AddTo(mgr);

        SingleAssignmentDisposable reqReadyDisposer = new SingleAssignmentDisposable();
        reqReadyDisposer.Disposable = Observable.Timer(TimeSpan.FromSeconds(3))
            .Subscribe(_ =>
            {
                TCPMGPihagiRequestManager.Instance.Req_GameReady(BPWPacketDefine.PihagiGameReadyType.TO_GAME_START);
                reqReadyDisposer.Dispose();
                reqReadyDisposer = null;
            }).AddTo(mgr);
    }

    public override void Excute(MGPihagiWorldManager mgr)
    {
        //다보여주고
        //
    }

    public override void Exit(MGPihagiWorldManager mgr)
    {
    }
}




public class MGPihagiGameState_CountDown : CState<MGPihagiWorldManager>
{
    static MGPihagiGameState_CountDown s_this = null;

    public static MGPihagiGameState_CountDown Instance()
    {
        if (s_this == null) s_this = new MGPihagiGameState_CountDown();
        return s_this;
    }

    public override void Enter(MGPihagiWorldManager mgr)
    {
        //카운트 다운 시작 ( 3,2,1)
        //후에 바로 start표현(서버에서 Bc_PihagiGameStateStartHandler가 올때까지)
        //오면 MGPihagiGameState_Play
        mgr.GetPageUI().bPlaiedStartAnim = false;
    }

    public override void Excute(MGPihagiWorldManager mgr)
    {
    }

    public override void Exit(MGPihagiWorldManager mgr)
    {
    }
}


public class MGPihagiGameState_Play : CState<MGPihagiWorldManager>
{
    static MGPihagiGameState_Play s_this = null;

    public static MGPihagiGameState_Play Instance()
    {
        if (s_this == null) s_this = new MGPihagiGameState_Play();
        return s_this;
    }

    public override void Enter(MGPihagiWorldManager mgr)
    {
        CDebug.Log( $"[TCP] MGPihagiGameState_Play STATE", CDebugTag.BPWORLD_PACKET );
        //Timer Start
        mgr.SetGameTimer();
        mgr.GetPageUI().ShowTimerUI();
        //JoyStick unlock

    }

    public override void Excute(MGPihagiWorldManager mgr)
    {
    }

    public override void Exit(MGPihagiWorldManager mgr)
    {
    }
}



public class MGPihagiGameState_ShowPlayFinish : CState<MGPihagiWorldManager>
{
    static MGPihagiGameState_ShowPlayFinish s_this = null;

    public static MGPihagiGameState_ShowPlayFinish Instance()
    {
        if (s_this == null) s_this = new MGPihagiGameState_ShowPlayFinish();
        return s_this;
    }

    public override void Enter(MGPihagiWorldManager mgr)
    {
		mgr.SetMyPlayerJoyStickStop(true);//PlayerMgr.GetMyPlayer().SetJoyStickStop(true);
        //에너미 가리고
        mgr.EnemyMgr.SetHideAllEnemy();
        SoundManager.Instance.PlayEffect( 6830029 ); // list 129
		//show finish effect
		mgr.GetPageUI().FinishRoot.gameObject.SetActive(true);

		mgr.PlayerMgr.SetChangeAllPlayerState(MGPihagiPlayerState_Stop.Instance());

        SingleAssignmentDisposable showFinishDisposer = new SingleAssignmentDisposable();
        showFinishDisposer.Disposable = Observable.Timer(TimeSpan.FromSeconds(1))
            .Subscribe(_ =>
            {
				//GameResultInfo _resultInfo = MGPihagiServerDataManager.Instance.GetGameResultInfo();
				BPWPacketDefine.PihagiGameStageType stageType = MGPihagiServerDataManager.Instance.StageType;
				if(stageType == BPWPacketDefine.PihagiGameStageType.FINAL)
				{					
					mgr.GetPageUI().SetActiveTimerObj(false);
					mgr.GetPageUI().FinishRoot.gameObject.SetActive(false);
					mgr.SetChangeState(MGPihagiGameState_Result.Instance());
				}
				else
				{
					mgr.SetChangeState(MGPihagiGameState_PlayFinish.Instance());
				}
                showFinishDisposer.Dispose();
                showFinishDisposer = null;
            }).AddTo(mgr);
    }

    public override void Excute(MGPihagiWorldManager mgr)
    {
    }

    public override void Exit(MGPihagiWorldManager mgr)
    {
    }
}



public class MGPihagiGameState_PlayFinish : CState<MGPihagiWorldManager>
{
    static MGPihagiGameState_PlayFinish s_this = null;

    public static MGPihagiGameState_PlayFinish Instance()
    {
        if (s_this == null) s_this = new MGPihagiGameState_PlayFinish();
        return s_this;
    }

    public override void Enter(MGPihagiWorldManager mgr)
    {
        //플레이어 멈추고
        //타이머 가리고
		mgr.GetPageUI().RemoveTimeStream();
        mgr.GetPageUI().SetActiveTimerObj(false);
		mgr.GetPageUI().FinishRoot.gameObject.SetActive(false);
        //MGPihagiServerDataManager.Instance.SetStateLatencyTime( Time.time );

        //mgr.bFinishResultSet = false;

        GameResultInfo _info = MGPihagiServerDataManager.Instance.GetGameResultInfo();

        //동점자가 있다면 MGPihagiGameState_ProcTiePlayer 로
        if(_info.IsPickingWinner)
        {
            mgr.SetChangeState(MGPihagiGameState_ProcTiePlayer.Instance());
        }
        //없다면 MGPihagiGameState_ShowFinalRoundPlayer 로
        else
        {
            mgr.SetChangeState(MGPihagiGameState_ShowFinalRoundPlayer.Instance());
        }
    }

    public override void Excute(MGPihagiWorldManager mgr)
    {
    }

    public override void Exit(MGPihagiWorldManager mgr)
    {
    }
}






public class MGPihagiGameState_ProcTiePlayer : CState<MGPihagiWorldManager>
{
    static MGPihagiGameState_ProcTiePlayer s_this = null;

    public static MGPihagiGameState_ProcTiePlayer Instance()
    {
        if (s_this == null) s_this = new MGPihagiGameState_ProcTiePlayer();
        return s_this;
    }

    public override void Enter(MGPihagiWorldManager mgr)
    {
        mgr.GetPageUI().SetActiveTieProc(true);

        var disposer = new SingleAssignmentDisposable();
        disposer.Disposable = Observable.Timer(TimeSpan.FromSeconds(2))
        .Subscribe(_ =>
        {
            mgr.SetChangeState(MGPihagiGameState_ShowFinalRoundPlayer.Instance());

            disposer.Dispose();
        })
        .AddTo(mgr);
    }

    public override void Excute(MGPihagiWorldManager mgr)
    {
    }

    public override void Exit(MGPihagiWorldManager mgr)
    {
    }
}

public class MGPihagiGameState_ShowFinalRoundPlayer : CState<MGPihagiWorldManager>
{
    static MGPihagiGameState_ShowFinalRoundPlayer s_this = null;

    public static MGPihagiGameState_ShowFinalRoundPlayer Instance()
    {
        if (s_this == null) s_this = new MGPihagiGameState_ShowFinalRoundPlayer();
        return s_this;
    }

    public override void Enter(MGPihagiWorldManager mgr)
    {
        SoundManager.Instance.PlayEffect( 6830022 ); // list 121
        mgr.GetPageUI().SetActiveFinallist(true);

        var disposer = new SingleAssignmentDisposable();
        disposer.Disposable = Observable.Timer(TimeSpan.FromSeconds(1))
        .Subscribe(_ =>
        {
            mgr.SetChangeState(MGPihagiGameState_Result.Instance());

            disposer.Dispose();
        })
        .AddTo(mgr);
    }

    public override void Excute(MGPihagiWorldManager mgr)
    {
    }

    public override void Exit(MGPihagiWorldManager mgr)
    {
    }
}


public class MGPihagiGameState_Result : CState<MGPihagiWorldManager>
{
    static MGPihagiGameState_Result s_this = null;

    public static MGPihagiGameState_Result Instance()
    {
        if (s_this == null) s_this = new MGPihagiGameState_Result();
        return s_this;
    }

    public override void Enter(MGPihagiWorldManager mgr)
    {
        mgr.PlayerMgr.SetActiveIndicatorOfMyPlayer(false);
        mgr.GetPageUI().SetActiveUI(false);
        //결과 출력위치로 캐릭터와 카메라 이동(순간이동)
        mgr.SetPlayerForResult();
        mgr.GetPageUI().SetActivePopupMsgRoot(false);
		
        mgr.SetResultCamera();
    }

    public override void Excute(MGPihagiWorldManager mgr)
    {
    }

    public override void Exit(MGPihagiWorldManager mgr)
    {
    }
}

public class MGPihagiGameState_ResultPopup : CState<MGPihagiWorldManager>
{
    static MGPihagiGameState_ResultPopup s_this = null;

    public static MGPihagiGameState_ResultPopup Instance()
    {
        if (s_this == null) s_this = new MGPihagiGameState_ResultPopup();
        return s_this;
    }

    public override void Enter(MGPihagiWorldManager mgr)
    {
        //Result POPUP
        CPopupService _popupService = CCoreServices.GetCoreService<CPopupService>();
        mgr.ResultPopup = _popupService.ShowMGPihagiResultPopup();
        mgr.ResultPopup.SetData(new PopupMGPihagiResult.Setting()
        {
            WorldMgr = mgr,
        });

        mgr.ClosePopupAlert();

        SingleAssignmentDisposable disposer = new SingleAssignmentDisposable();
        disposer.Disposable = mgr.ResultPopup.ShowAsObservable()
            .Subscribe(_ =>
            {
                disposer.Dispose();
                CDebug.Log("PopupMGPihagiResult close");
            });
    }

    public override void Excute(MGPihagiWorldManager mgr)
    {
    }

    public override void Exit(MGPihagiWorldManager mgr)
    {
    }
}



public class MGPihagiGameState_PrepareFinal : CState<MGPihagiWorldManager>
{
    static MGPihagiGameState_PrepareFinal s_this = null;

    public static MGPihagiGameState_PrepareFinal Instance()
    {
        if (s_this == null) s_this = new MGPihagiGameState_PrepareFinal();
        return s_this;
    }

    public override void Enter(MGPihagiWorldManager mgr)
    {
        MGPihagiServerDataManager.Instance.CleanUpServerData();

        mgr.PlayerMgr.CleanUpPlayers();
		mgr.EnemyMgr.CleanUpEnemy();
        mgr.GetPageUI().CleanUpPageUI();
    }

    public override void Excute(MGPihagiWorldManager mgr)
    {
    }

    public override void Exit(MGPihagiWorldManager mgr)
    {
        mgr.GetPageUI().SetActivePopupMsgRoot(false);
    }
}


public class MGPihagiGameState_Exit_Leave : CState<MGPihagiWorldManager>
{
    static MGPihagiGameState_Exit_Leave s_this = null;

    public static MGPihagiGameState_Exit_Leave Instance()
    {
        if (s_this == null) s_this = new MGPihagiGameState_Exit_Leave();
        return s_this;
    }
    public override void Enter(MGPihagiWorldManager mgr)
    {
        CDebug.Log( $"[TCP] CALL PihagiGameLeaveReq | Req_RoomLeave()", CDebugTag.MINIGAME_PIHAGI );
        TCPMGPihagiRequestManager.Instance.Req_RoomLeave();
    }

    public override void Excute(MGPihagiWorldManager mgr) { }
    public override void Exit(MGPihagiWorldManager mgr) { }
}




public class MGPihagiGameState_Tutorial : CState<MGPihagiWorldManager>
{
    static MGPihagiGameState_Tutorial s_this = null;

    public static MGPihagiGameState_Tutorial Instance()
    {
        if (s_this == null) s_this = new MGPihagiGameState_Tutorial();
        return s_this;
    }
    public override void Enter(MGPihagiWorldManager mgr)
    {
        TutorialStep _tutoStep = mgr.GetTutorialStep();
        switch (_tutoStep)
        {
            case TutorialStep.NONE:
                //go to Step 1
                mgr.SetTutorial_Step1();
                break;
            //case TutorialStep.STEP_1:
            //    //go to Step 2
            //    mgr.SetTutorial_Step2();
            //    break;
        }
    }

    public override void Excute(MGPihagiWorldManager mgr) { }
    public override void Exit(MGPihagiWorldManager mgr) { }
}