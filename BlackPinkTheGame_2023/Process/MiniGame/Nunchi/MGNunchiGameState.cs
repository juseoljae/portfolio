
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
using UnityEngine;
using UniRx;


public class MGNunchiGameState_SetServerInfo : CState<MGNunchiWorldManager>
{
    static MGNunchiGameState_SetServerInfo s_this = null;

    public static MGNunchiGameState_SetServerInfo Instance()
    {
        if (s_this == null) 
            s_this = new MGNunchiGameState_SetServerInfo();
        
        return s_this;
    }


    public override void Enter(MGNunchiWorldManager mgr)
    {
		CDebug.Log("$$$$$$   MGNunchiGameState_SetServerInfo.Enter()", CDebugTag.MINIGAME_NUNCHI);
        //mgr.GetPageUI().SetActiveDirectionButtons(false);
        mgr.GetPageUI().SetActivePageUI(true);
        mgr.GetPageUI().SetActivePopupMsgRoot(false);
        mgr.GetPageUI().SetActiveUI(false);
        mgr.GetPageUI().SetStageText();
        mgr.GetCanvasManager().SetActiveNameLayer(false);
        //mgr.GetPageUI().CleanControllerButtons();


        ///[deprecated] 푸딩으로 변경된다
        //mgr.GetPageUI().SetControllerButtonsCallBack();
        mgr.MapMgr.IsMapObjectClick = false;


        mgr.PlayerMgr.AddGamePlayers();
        mgr.PlayerMgr.SetPlayerInfoUI();

        mgr.SetActivePlayerNameTag(false);

        //don't use Emoticon, Motion in Minigame
        //mgr.GetPageUI().SetRadialButtons();

        mgr.SetHelloCamPos();

        mgr.PlayerMgr.SetAllPlayerLookAtCamera();


        foreach (int playerKey in mgr.PlayerMgr.PlayersDic.Keys)
        {
            MGNunchiServerDataManager.Instance.InitRoundResultInfo_GainItem(playerKey);
        }

        TournamentInfo _stageInfo = MGNunchiServerDataManager.Instance.GetTournamentGameRoomInfo();
        if(_stageInfo != null)
        {
            if(_stageInfo.StageType == BPWPacketDefine.NunchiGameStageType.FINAL)
            {
                mgr.CloseForFinal().Subscribe(_ => 
                {
                    mgr.SetChangeState(MGNunchiGameState_IntroCam.Instance());
                }).AddTo(mgr);
            }
        }


    }
    public override void Excute(MGNunchiWorldManager mgr)
    {
    }
    public override void Exit(MGNunchiWorldManager mgr) { }
}

public class MGNunchiGameState_IntroCam : CState<MGNunchiWorldManager>
{
    static MGNunchiGameState_IntroCam s_this = null;

    public static MGNunchiGameState_IntroCam Instance()
    {
        if (s_this == null) s_this = new MGNunchiGameState_IntroCam();
        return s_this;
    }
    public override void Enter(MGNunchiWorldManager mgr)
    {
		CDebug.Log($"      $$$$$$   MGNunchiGameState_IntroCam.Enter() Stage type : {MGNunchiServerDataManager.Instance.StageType}");
        //GameLog
        APIHelper.GameLog.MiniGame_Start( (int)BPWPacketDefine.MatchType.NUNCHI_GAME, (int)MGNunchiServerDataManager.Instance.StageType );

        MGNunchiPlayer player = mgr.PlayerMgr.GetMyPlayer();
		player.PlayerController.SetAnimationTrigger("handhi");


        var disposer = new SingleAssignmentDisposable();
        disposer.Disposable = Observable.Timer(TimeSpan.FromSeconds(2))
        .Subscribe(_ =>
        {
            mgr.SetIsGameStart(true);
            mgr.GetPageUI().SetActiveStageTextObj(false);
            mgr.MoveIntroCamera();
            disposer.Dispose();
        })
        .AddTo(mgr);
    }
    public override void Excute(MGNunchiWorldManager mgr) 
    {
        //CDebug.Log($"             ############# introCam.Excute() cam rotation X:{mgr.GetCurCamRotX()}");
        //if(mgr.GetIsGameStart())
        //{
        //    mgr.RotateHelloCamToDefault();
        //}
    }
    public override void Exit(MGNunchiWorldManager mgr) { }
}

public class MGNunchiGameState_Ready : CState<MGNunchiWorldManager>
{
    static MGNunchiGameState_Ready s_this = null;

    public static MGNunchiGameState_Ready Instance()
    {
        if (s_this == null) s_this = new MGNunchiGameState_Ready();
        return s_this;
    }
    public override void Enter(MGNunchiWorldManager mgr) 
    {
		CDebug.Log("      $$$$$$   MGNunchiGameState_Ready.Enter()", CDebugTag.NUNCH_GAME_STATE_READY);
        //mgr.SetIsGameStart(false);

        mgr.GetPageUI().SetUp();
#if SHOW_MINIGAME_BTN
        mgr.GetPageUI().SetActiveButtonGroup(true);
#endif

        //Make coin item
        mgr.CoinMgr.ResetGameCoinObjectEndRound();        
        mgr.MapMgr.SetCoinItemOnMap();

        mgr.PlayerMgr.ChangeAllPlayersState(MGNunchiPlayerState_Ready.Instance());

        mgr.CoinMgr.SetActiveAllCoinObject(true);
        mgr.CoinMgr.SetChangeEachCoinState(MGNunchiCoinState_Spawn.Instance());

        mgr.SetActivePlayerNameTag(true);

        var stateChangeDisposer = new SingleAssignmentDisposable();
        stateChangeDisposer.Disposable = Observable.Timer(TimeSpan.FromSeconds(2))
        .Subscribe(_ =>
        {
			mgr.SetChangeState(MGNunchiGameState_Round.Instance());
            stateChangeDisposer.Dispose();
        })
        .AddTo(mgr);

    }
    public override void Excute(MGNunchiWorldManager mgr)
    {
    }
    public override void Exit(MGNunchiWorldManager mgr) { }
}

public class MGNunchiGameState_Round : CState<MGNunchiWorldManager>
{
    static MGNunchiGameState_Round s_this = null;

    public static MGNunchiGameState_Round Instance()
    {
        if (s_this == null) s_this = new MGNunchiGameState_Round();
        return s_this;
    }
    public override void Enter(MGNunchiWorldManager mgr)
    {
        SoundManager.Instance.PlayEffect( 6830019 ); // list 118
        mgr.GetPageUI().SetRoundText();// SetStageText();
        //mgr.GetPageUI().SetActiveRoundTextObj(true);
        //u can see All Scene Object
        //will remove below. all things do in Excute
        //mgr.GetPageUI().SetActiveStageStartObj(false);
        mgr.GetPageUI().bPlaiedStartAnim = false;
    }
    public override void Excute(MGNunchiWorldManager mgr) 
    {
    }
    public override void Exit(MGNunchiWorldManager mgr) { }
}

public class MGNunchiGameState_InPlayingGame : CState<MGNunchiWorldManager>
{
    static MGNunchiGameState_InPlayingGame s_this = null;

    public static MGNunchiGameState_InPlayingGame Instance()
    {
        if (s_this == null) s_this = new MGNunchiGameState_InPlayingGame();
        return s_this;

    }


    public override void Enter(MGNunchiWorldManager mgr)
    {
        //#if UNITY_EDITOR
        //		Time.timeScale = 0.25f;
        //#endif
        //mgr.SetIsGameStart(true);
        mgr.GetPageUI().SetActiveCloseButton( true );
		mgr.GetPageUI().SetActiveStageStartObj(false);

        //Timer
        mgr.SetGameTimer();
        mgr.GetPageUI().ShowTimerUI();

#if SHOW_MINIGAME_BTN
        mgr.GetPageUI().SetActiveDirectionButtons(true);
        mgr.GetPageUI().SetActiveControllerBtn();
#endif

        mgr.MapMgr.SetEffectActivate();
        mgr.MapMgr.IsMapObjectClick = true;
        

        mgr.PlayerMgr.ChangeAllPlayersState(MGNunchiPlayerState_Round.Instance());
    }


    public override void Excute(MGNunchiWorldManager mgr) 
    {
        if (mgr.GetCheckerTimeToCleanCoinUp() == false)
        {
            if (mgr.GetIsTimeToCleanCoinItemUpValue())
            {
                mgr.CoinMgr.SetCoinStateRemain();
                mgr.SetCheckerTimeToCleanCoinUp(true);
            }
        }


        //새로운 Round를 시작할건가?
        bool bAllPlayerRoundFinish = mgr.PlayerMgr.CheckAllPlayersRoundFinish();
        
        if (bAllPlayerRoundFinish)
        {
            CDebug.Log($"     **** MGNunchiGameState_InPlayingGame.Excute() bAllPlayerRoundFinish:{bAllPlayerRoundFinish}");
            mgr.PlayerMgr.InitAllPlayersRoundFinish();

            mgr.PlayerMgr.SortPlayerUIDByScore();
            mgr.PlayerMgr.SetPlayerInfoUI();

            int curRound = mgr.GetRoundCount();
            int maxRound = mgr.GetMaxRound();
            CDebug.Log($"     **** MGNunchiGameState_InPlayingGame.Excute() curRound:{curRound} / maxRound:{maxRound}", CDebugTag.MINIGAME_NUNCHI);

            if (curRound == maxRound)
            {
                MGNunchiServerDataManager.Instance.SetStateLatencyTime( Time.time );
                TournamentInfo _info = MGNunchiServerDataManager.Instance.GetTournamentGameRoomInfo();

                CDebug.Log($"_info.StageType == {_info.StageType}", CDebugTag.MINIGAME_NUNCHI);

                if (_info.StageType == BPWPacketDefine.NunchiGameStageType.TRYOUT)
                {
                    if (MGNunchiServerDataManager.Instance.GetIsThereTiePlayers())
                    {
                        //동점자가 있으면? 
                        mgr.SetChangeState(MGNunchiGameState_ProcTiePlayer.Instance());
                    }
                    else
                    {
                        //없으면?  
                        mgr.SetChangeState(MGNunchiGameState_ShowFinalRoundPlayer.Instance());
                    }
                }
                else if(_info.StageType == BPWPacketDefine.NunchiGameStageType.FINAL)
                {
                    mgr.SetChangeState(MGNunchiGameState_Result.Instance());
                }
            }
            else
            {
                TCPMGNunchiRequestManager.Instance.Req_AllPlayerRoundFinish(BPWPacketDefine.NunchiGameReadyType.TO_NEXT_ROUND);
            }
        }
    }


    public override void Exit(MGNunchiWorldManager mgr)
    {
//#if UNITY_EDITOR
//		Time.timeScale =1f;
//#endif
        mgr.SetCheckerTimeToCleanCoinUp(false);
        mgr.SetIsTimeToCleanCoinItemUp(false);
    }
}



public class MGNunchiGameState_ReadyToNextRound : CState<MGNunchiWorldManager>
{
    static MGNunchiGameState_ReadyToNextRound s_this = null;

    public static MGNunchiGameState_ReadyToNextRound Instance()
    {
        if (s_this == null) s_this = new MGNunchiGameState_ReadyToNextRound();
        return s_this;
    }
    public override void Enter(MGNunchiWorldManager mgr)
    {
    }
    public override void Excute(MGNunchiWorldManager mgr)
    {
    }
    public override void Exit(MGNunchiWorldManager mgr) { }
}


public class MGNunchiGameState_ProcTiePlayer : CState<MGNunchiWorldManager>
{
    static MGNunchiGameState_ProcTiePlayer s_this = null;

    public static MGNunchiGameState_ProcTiePlayer Instance()
    {
        if (s_this == null) s_this = new MGNunchiGameState_ProcTiePlayer();
        return s_this;
    }
    public override void Enter(MGNunchiWorldManager mgr)
    {
        mgr.GetPageUI().SetActiveTieProc(true);

        var disposer = new SingleAssignmentDisposable();
        disposer.Disposable = Observable.Timer(TimeSpan.FromSeconds(2))
        .Subscribe(_ =>
        {
            mgr.SetChangeState(MGNunchiGameState_ShowFinalRoundPlayer.Instance());

            disposer.Dispose();
        })
        .AddTo(mgr);
    }
    public override void Excute(MGNunchiWorldManager mgr)
    {
    }
    public override void Exit(MGNunchiWorldManager mgr) { }
}



public class MGNunchiGameState_ShowFinalRoundPlayer : CState<MGNunchiWorldManager>
{
    static MGNunchiGameState_ShowFinalRoundPlayer s_this = null;

    public static MGNunchiGameState_ShowFinalRoundPlayer Instance()
    {
        if (s_this == null) s_this = new MGNunchiGameState_ShowFinalRoundPlayer();
        return s_this;
    }
    public override void Enter(MGNunchiWorldManager mgr)
    {
        mgr.GetPageUI().SetActiveFinallist(true);
        SoundManager.Instance.PlayEffect( 6830022 ); // list 121
        var disposer = new SingleAssignmentDisposable();
        disposer.Disposable = Observable.Timer(TimeSpan.FromSeconds(2))
        .Subscribe(_ =>
        {
            mgr.SetChangeState(MGNunchiGameState_Result.Instance());

            disposer.Dispose();
        })
        .AddTo(mgr);
    }
    public override void Excute(MGNunchiWorldManager mgr)
    {
    }
    public override void Exit(MGNunchiWorldManager mgr) { }
}




public class MGNunchiGameState_Result : CState<MGNunchiWorldManager>
{
    static MGNunchiGameState_Result s_this = null;

    public static MGNunchiGameState_Result Instance()
    {
        if (s_this == null) s_this = new MGNunchiGameState_Result();
        return s_this;
    }

    public override void Enter(MGNunchiWorldManager mgr) 
    {
        mgr.PlayerMgr.ChangeAllPlayersState(MGNunchiPlayerState_Result.Instance());


        mgr.GetPageUI().SetActivePageUI(false);
        mgr.SetActivePlayerNameTag(false);
        //mgr.GetPageUI().ShowItemNameTag("", null);
        mgr.SetActiveItemNameTag(false);


        //    [0]
        // [1]   [2]
        //    [3]
        MGNunchiPlayer myPlayer = mgr.PlayerMgr.GetMyPlayer();
        int myPlayerSeatId = myPlayer.GetMySeatIdx();//
        Vector3 camTargetPos = mgr.GetResultCameraPosition(myPlayerSeatId);
        mgr.PlayerMgr.SetActiveMyPlayerIndecator(false);//.GetCanvasManager().SetActiveNameLayer()

        //Result POPUP
        CPopupService _popupService = CCoreServices.GetCoreService<CPopupService>();
        mgr.ResultPopup = _popupService.ShowMGNunchiResultPopup();
        mgr.ResultPopup.SetData(new PopupMGNunchiResult.Setting()
        {
            WorldMgr = mgr,
            CamTargetPosition = camTargetPos,
            bStartCamAction = false,
        });

        mgr.ClosePopupAlert();

        SingleAssignmentDisposable disposer = new SingleAssignmentDisposable();
        disposer.Disposable = mgr.ResultPopup.ShowAsObservable()
            .Subscribe(_ =>
            {
                disposer.Dispose();
                CDebug.Log("PopupMGNunchiResult close");
            });
    }
    public override void Excute(MGNunchiWorldManager mgr) 
    {
        if(mgr.ResultPopup != null)
        {
            if(mgr.ResultPopup.GetbStartCameraAction() == false)
            {
                //CDebug.Log("MGNunchiGameState_Result.Excute(). Player LookAt to Camera");//
                mgr.PlayerLookAtToCamera();
            }
        }
    }
    public override void Exit(MGNunchiWorldManager mgr) { }
}


public class MGNunchiGameState_PrepareFinal : CState<MGNunchiWorldManager>
{
    static MGNunchiGameState_PrepareFinal s_this = null;

    public static MGNunchiGameState_PrepareFinal Instance()
    {
        if (s_this == null) s_this = new MGNunchiGameState_PrepareFinal();
        return s_this;
    }
    public override void Enter(MGNunchiWorldManager mgr)
    {			
		//mgr.SetIsPlayHello(false);
        //Server data clean up
        MGNunchiServerDataManager.Instance.CleanUpServerData();

        //my avatar back
        //other player destroy
        //mgr.PlayerMgr.CleanUpPlayers();
        //clean up coin
        mgr.CoinMgr.ResetGameCoinObjectEndRound();

        mgr.GetPageUI().CleanUpPageUI();

    }
    public override void Excute(MGNunchiWorldManager mgr) { }
    public override void Exit(MGNunchiWorldManager mgr) { }
}


public class MGNunchiGameState_Exit_Leave : CState<MGNunchiWorldManager>
{
    static MGNunchiGameState_Exit_Leave s_this = null;

    public static MGNunchiGameState_Exit_Leave Instance()
    {
        if (s_this == null) s_this = new MGNunchiGameState_Exit_Leave();
        return s_this;
    }
    public override void Enter(MGNunchiWorldManager mgr)
    {
		TCPMGNunchiRequestManager.Instance.Req_RoomLeave();
    }

    public override void Excute(MGNunchiWorldManager mgr) { }
    public override void Exit(MGNunchiWorldManager mgr) { }
}


//For Tutorial

public class MGNunchiGameState_Tutorial : CState<MGNunchiWorldManager>
{
    static MGNunchiGameState_Tutorial s_this = null;

    public static MGNunchiGameState_Tutorial Instance()
    {
        if (s_this == null) s_this = new MGNunchiGameState_Tutorial();
        return s_this;
    }
    public override void Enter(MGNunchiWorldManager mgr)
    {

        TutorialStep _tutoStep = mgr.GetTutorialStep();
        switch (_tutoStep)
        {
            case TutorialStep.NONE:
                //go to Step 1
                mgr.SetTutorial_Step1();
                break;
            case TutorialStep.STEP_1:
                //go to Step 2
                mgr.SetTutorial_Step2();
                break;
        }

    }
    public override void Excute(MGNunchiWorldManager mgr) { }
    public override void Exit(MGNunchiWorldManager mgr) { }
}




/*
public class MGNunchiGameState : CState<MGNunchiWorldManager>
{
    static MGNunchiGameState s_this = null;

    public static MGNunchiGameState Instance()
    {
        if (s_this == null) s_this = new MGNunchiGameState();
        return s_this;
    }
    public override void Enter (MGNunchiWorldManager mgr) { }
    public override void Excute(MGNunchiWorldManager mgr) { }
    public override void Exit  (MGNunchiWorldManager mgr) { }
}


*/