
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
using UnityEditor;

/*
 * State     -   Motion
 * Ready     -   Idle
 * Start     -   Idle
 * Round     -   Idle
 * GoGetCoin -   Jump
 * Result    -   Success or fail
 * Return    -   Jump
 */

public class MGNunchiPlayerState_Ready : CState<MGNunchiPlayer>
{
    static MGNunchiPlayerState_Ready s_this = null;

    public static MGNunchiPlayerState_Ready Instance()
    {
        if (s_this == null) s_this = new MGNunchiPlayerState_Ready();
        return s_this;
    }

    public override void Enter(MGNunchiPlayer player)
    {
        player.InitInfo();

        

        player.PlayerController.SetAnimationTrigger(MGNunchiDefines.ANIM_IDLE);
    }
    public override void Excute(MGNunchiPlayer player) { }

    public override void Exit(MGNunchiPlayer player)
    {
        //CDebug.Log("    ### CollectCoinPlayerState_Ready.Exit()", CDebugTag.MINIGAME_NUNCHI);
    }
}


public class MGNunchiPlayerState_Round : CState<MGNunchiPlayer>
{
    static MGNunchiPlayerState_Round s_this = null;

    public static MGNunchiPlayerState_Round Instance()
    {
        if (s_this == null) s_this = new MGNunchiPlayerState_Round();
        return s_this;
    }

    public override void Enter(MGNunchiPlayer player)
    {
        //if (player.PlayerInfo.UID == player.WorldMgr.PlayerMgr.MyPlayerUID)
        //{
        //    player.WorldMgr.GetPageUI().SetActiveControllerBtn();
        //}
    }
    public override void Excute(MGNunchiPlayer player) { }

    public override void Exit(MGNunchiPlayer player)
    {
    }
}


public class MGNunchiPlayerState_GoGetCoinPrepare : CState<MGNunchiPlayer>
{
    static MGNunchiPlayerState_GoGetCoinPrepare s_this = null;

    private const float ROTATION_TIME = 0.2f;

    public static MGNunchiPlayerState_GoGetCoinPrepare Instance()
    {
        if (s_this == null) s_this = new MGNunchiPlayerState_GoGetCoinPrepare();
        return s_this;
    }

    public override void Enter(MGNunchiPlayer player)
    {
        //CDebug.Log("    ### CollectCoinPlayerState_GoGetCoinPrepare.Enter()", CDebugTag.MINIGAME_NUNCHI);
        player.PlayerController.SetAnimationTrigger(MGNunchiDefines.ANIM_IDLE);

        //player.WorldMgr.SetIsGameStart(false);

        //other players set target map 
        if (player.PlayerInfo.UID != player.WorldMgr.PlayerMgr.MyPlayerUID)
        {
            player.SetTargetMapIndex();
        }
        else
        {
            if (player.GetTargetDirectionInMap() == DIRECTION.NONE)
            {
                CDebug.Log("           ######## Target Dir is NONE !!!!!!!!");
                player.SetTargetMapIndex();
            }
        }


        var disposer = new SingleAssignmentDisposable();
        disposer.Disposable = Observable.Timer(TimeSpan.FromSeconds(ROTATION_TIME))
        .Subscribe(_ =>
        {
            player.PlayerSM.ChangeState(MGNunchiPlayerState_GoGetCoin.Instance());

            disposer.Dispose();
        })
        .AddTo(player.WorldMgr);
    }
    public override void Excute(MGNunchiPlayer player)
    {
        if (player.GetTargetDirectionInMap() != DIRECTION.NONE)
        {
            player.PlayerInfo.PlayerObj.transform.DOLocalRotate(new Vector3(0, player.GetTargetAngle(), 0), ROTATION_TIME);
        }
    }

    public override void Exit(MGNunchiPlayer player)
    {
        //CDebug.Log("    ### CollectCoinPlayerState_GoGetCoinPrepare.Exit()", CDebugTag.MINIGAME_NUNCHI);
    }
}

public class MGNunchiPlayerState_GoGetCoin : CState<MGNunchiPlayer>
{
    static MGNunchiPlayerState_GoGetCoin s_this = null;
    private float jumpClipLengthTime;

    public static MGNunchiPlayerState_GoGetCoin Instance()
    {
        if (s_this == null) s_this = new MGNunchiPlayerState_GoGetCoin();
        return s_this;
    }

    public override void Enter(MGNunchiPlayer player)
    {
        //CDebug.Log("    ### CollectCoinPlayerState_GoGetCoin.Enter()", CDebugTag.MINIGAME_NUNCHI);

		//Goto Jump!!
		player.SetJump(player.GetTargetPositonInMap(), player.GetTargetMapInfo());

        player.PlayMySound( 6830014 );// list 113

        var coinDisposer = new SingleAssignmentDisposable();
        coinDisposer.Disposable = Observable.Timer(TimeSpan.FromSeconds(MGNunchiDefines.JUMP_GETCOIN_TIME))            
        .Subscribe(_ =>
        {
            //turn off coin object who player success 
            if (player.GetCoinSuccess())
            {
                player.SetChangeState(MGNunchiPlayerState_SetGainSussess.Instance());
            }
            else
            {
				player.SetChangeState(MGNunchiPlayerState_SetGainFail.Instance());
            }

            player.PlayMySound( 6830015 ); // list 114

            coinDisposer.Dispose();
        })
        .AddTo(player.WorldMgr);
    }
    public override void Excute(MGNunchiPlayer player) 
    {
    }

    public override void Exit(MGNunchiPlayer player)
    {
    }
}



public class MGNunchiPlayerState_SetGainSussess : CState<MGNunchiPlayer>
{
    static MGNunchiPlayerState_SetGainSussess s_this = null;

    public static MGNunchiPlayerState_SetGainSussess Instance()
    {
        if (s_this == null) s_this = new MGNunchiPlayerState_SetGainSussess();
        return s_this;
    }

    public override void Enter(MGNunchiPlayer player)
    {
        //long _uid = player.PlayerInfo.UID;
        int _pid = player.PlayerInfo.PID;
        RoundResultInfo _info = MGNunchiServerDataManager.Instance.GetRoundResultInfoDic( _pid );

        MGNunchiMap _mapInfo = player.GetTargetMapInfo();
        int mapIdx = player.WorldMgr.MapMgr.GetMapIndexByXY(_mapInfo.X, _mapInfo.Y);
        player.WorldMgr.CoinMgr.SetActiveGameCoinGainFXObject(mapIdx, true);
		
        player.PlayerInfo.CoinScore = _info.Quantity_TotalCoin;
        int uiIdx = player.WorldMgr.PlayerMgr.GetIndexOfSortedPlayerUI( _pid );
        player.WorldMgr.GetPageUI().SetPlayerScore(player.PlayerInfo, uiIdx);
        player.WorldMgr.GetPageUI().SetActiveBadgeObjByItemType(uiIdx, _info.GainItemType);
        player.AddScoreFxUI(_info.Quantity_GainCoin, _info.GainItemType, player);		


		//Effect----
        var disappearDisposer = new SingleAssignmentDisposable();
        disappearDisposer.Disposable = Observable.Timer(TimeSpan.FromSeconds(0.1f))
        .Subscribe(_ =>
        {
            player.WorldMgr.CoinMgr.SetActiveGameCoinFBXObject(mapIdx, false);
            disappearDisposer.Dispose();
        })
        .AddTo(player.WorldMgr);

        var hideCoinDisposer = new SingleAssignmentDisposable();
        hideCoinDisposer.Disposable = Observable.Timer(TimeSpan.FromSeconds(0.3f))
        .Subscribe(_ =>
        {
            player.WorldMgr.CoinMgr.SetActiveGameCoinObject(mapIdx, false);
            //player.WorldMgr.GetPageUI().ShowItemNameTag("", null);
            player.WorldMgr.SetActiveItemNameTag(false);
            hideCoinDisposer.Dispose();
        })
        .AddTo(player.WorldMgr);
		//----


        var disposer = new SingleAssignmentDisposable();
        disposer.Disposable = Observable.Timer(TimeSpan.FromSeconds(MGNunchiDefines.JUMP_DUR_TIME))
        .Subscribe(_ =>
        {
            player.PlayerSM.ChangeState(MGNunchiPlayerState_GainSuccess.Instance());

            disposer.Dispose();
        })
        .AddTo(player.WorldMgr);
    }
    public override void Excute(MGNunchiPlayer player){ }

    public override void Exit(MGNunchiPlayer player){ }
}



public class MGNunchiPlayerState_GainSuccess : CState<MGNunchiPlayer>
{
    static MGNunchiPlayerState_GainSuccess s_this = null;

    public static MGNunchiPlayerState_GainSuccess Instance()
    {
        if (s_this == null) s_this = new MGNunchiPlayerState_GainSuccess();
        return s_this;
    }

    public override void Enter(MGNunchiPlayer player)
    {
		float time = 0;
		if(player.WorldMgr.CheckFailPlayer())
		{
            time = MGNunchiDefines.KNOCKDOWN_TIME;// + MGNunchiDefines.BASEWAIT_TIME;
		}

        if(time > 0)
        {
            player.PlayerController.SetAnimationTrigger(MGNunchiDefines.ANIM_IDLE);
        }

        var victoryDisposer = new SingleAssignmentDisposable();
        victoryDisposer.Disposable = Observable.Timer(TimeSpan.FromSeconds(time))
        .Subscribe(_ =>
        {
			player.PlayerController.SetAnimationTrigger(MGNunchiDefines.ANIM_VICTORY);
            victoryDisposer.Dispose();
        })
        .AddTo(player.WorldMgr);

        var rotationDisposer = new SingleAssignmentDisposable();
        rotationDisposer.Disposable = Observable.Timer(TimeSpan.FromSeconds(MGNunchiDefines.VICTORY_TIME + time))
        .Subscribe(_ =>
        {
            player.PlayerController.SetAnimationTrigger(MGNunchiDefines.ANIM_VICTORY_IDLE);
            player.SetStartTurnAround(true);
            player.PlayerInfo.PlayerObj.transform.DOLocalRotate(new Vector3(0, (player.GetReturnAngle()), 0), MGNunchiDefines.ROTATION_TIME);

            rotationDisposer.Dispose();
        })
        .AddTo(player.WorldMgr);


		//go to return place
        var stateChangeDisposer = new SingleAssignmentDisposable();
        stateChangeDisposer.Disposable = Observable.Timer(TimeSpan.FromSeconds(MGNunchiDefines.VICTORY_TIME + MGNunchiDefines.ROTATION_TIME + time))
        .Subscribe(_ =>
        {
            player.PlayerSM.ChangeState(MGNunchiPlayerState_Return.Instance());
            stateChangeDisposer.Dispose();
        })
        .AddTo(player.WorldMgr);

    }
    public override void Excute(MGNunchiPlayer player)
    { }

    public override void Exit(MGNunchiPlayer player)
    {
    }
}



public class MGNunchiPlayerState_SetGainFail : CState<MGNunchiPlayer>
{
    static MGNunchiPlayerState_SetGainFail s_this = null;

    public static MGNunchiPlayerState_SetGainFail Instance()
    {
        if (s_this == null) s_this = new MGNunchiPlayerState_SetGainFail();
        return s_this;
    }

    public override void Enter(MGNunchiPlayer player)
    {
        //long _uid = player.PlayerInfo.UID;
        if(player == null || player.PlayerInfo == null) return;
        
        int _pid = player.PlayerInfo.PID;

        RoundResultInfo _info = MGNunchiServerDataManager.Instance.GetRoundResultInfoDic( _pid );

        if (_info == null) return;

        BPWPacketDefine.NunchiGameItemType itemType = _info.GainItemType;
        if (_info.Quantity_GainCoin < 0)
        {
            itemType = BPWPacketDefine.NunchiGameItemType.COIN;
        }
        //CDebug.Log($"    ### Fail AddScoreFxUI() /ui idx = {_info.UID}, name = {player.PlayerInfo.NickName}, item type = {_info.GainItemType}, {player.GetIsSuccess()}", CDebugTag.MINIGAME_NUNCHI);

        if (player.CanAddScoreFxUI(_info.Quantity_GainCoin, itemType))
        {
            player.PlayerInfo.CoinScore = _info.Quantity_TotalCoin;
            int uiIdx = player.WorldMgr.PlayerMgr.GetIndexOfSortedPlayerUI( _pid );
            player.WorldMgr.GetPageUI().SetPlayerScore(player.PlayerInfo, uiIdx);

            player.AddScoreFxUI(_info.Quantity_GainCoin, itemType, player);
        }

        if (player.GetTargetDirectionInMap() == DIRECTION.NONE)
        {
            player.PlayerSM.ChangeState(MGNunchiPlayerState_PlayFailAnim.Instance());
        }
        else
        {
            player.PlayerSM.ChangeState(MGNunchiPlayerState_GainFail.Instance());
        }
    }
    public override void Excute(MGNunchiPlayer player)
    {
    }

    public override void Exit(MGNunchiPlayer player)
    {
    }
}



public class MGNunchiPlayerState_GainFail : CState<MGNunchiPlayer>
{
    static MGNunchiPlayerState_GainFail s_this = null;

    public static MGNunchiPlayerState_GainFail Instance()
    {
        if (s_this == null) s_this = new MGNunchiPlayerState_GainFail();
        return s_this;
    }

    public override void Enter(MGNunchiPlayer player)
    {
        MGNunchiMap _mapInfo = player.GetTargetMapInfo();
        MGNunchiMapInfo _map = player.WorldMgr.MapMgr.GetMapInfoByXY(_mapInfo.X, _mapInfo.Y);
        _map.SetActiveHitFxObj(true);

        player.PlayMySound( 6830018 );// list 117
        player.PlayerController.SetAnimationTrigger(MGNunchiDefines.ANIM_KNOCKDOWN);//            
		player.PlayerInfo.PlayerObj.transform.DOLocalMove(player.GetMapPositionByIndex(player.PlayerInfo.Map_MyLocation), MGNunchiDefines.KNOCKDOWN_TIME);

        var disposer = new SingleAssignmentDisposable();
        disposer.Disposable = Observable.Timer(TimeSpan.FromSeconds(MGNunchiDefines.KNOCKDOWN_TIME))
        .Subscribe(_ =>
        {
            player.PlayerController.SetAnimationTrigger(MGNunchiDefines.ANIM_KNOCKDOWN_STAND);//

            disposer.Dispose();
        })
        .AddTo(player.WorldMgr);

        var failAniDisposer = new SingleAssignmentDisposable();
        failAniDisposer.Disposable = Observable.Timer(TimeSpan.FromSeconds(MGNunchiDefines.BASEWAIT_TIME))
        .Subscribe(_ =>
        {
            player.SetStartTurnAround(true);
            player.PlayerInfo.PlayerObj.transform.DOLocalRotate(new Vector3(0, (player.GetPrevAngle()), 0), MGNunchiDefines.ROTATION_TIME);
            player.PlayerController.SetAnimationTrigger(MGNunchiDefines.ANIM_FAIL);
            failAniDisposer.Dispose();
        })
        .AddTo(player.WorldMgr);

        var stateChangeDisposer = new SingleAssignmentDisposable();
        stateChangeDisposer.Disposable = Observable.Timer(TimeSpan.FromSeconds(MGNunchiDefines.BASEWAIT_TIME + MGNunchiDefines.BASEWAIT_TIME))
        .Subscribe(_ =>
        {
            player.PlayerSM.ChangeState(MGNunchiPlayerState_Finish.Instance());
            stateChangeDisposer.Dispose();
        })
        .AddTo(player.WorldMgr);

    }
    public override void Excute(MGNunchiPlayer player)
    {}

    public override void Exit(MGNunchiPlayer player)
    {
    }
}


public class MGNunchiPlayerState_PlayFailAnim : CState<MGNunchiPlayer>
{
    static MGNunchiPlayerState_PlayFailAnim s_this = null;

    public static MGNunchiPlayerState_PlayFailAnim Instance()
    {
        if (s_this == null) s_this = new MGNunchiPlayerState_PlayFailAnim();
        return s_this;
    }

    public override void Enter(MGNunchiPlayer player)
    {
        player.PlayerController.SetAnimationTrigger(MGNunchiDefines.ANIM_FAIL);

        var stateChangeDisposer = new SingleAssignmentDisposable();
        stateChangeDisposer.Disposable = Observable.Timer(TimeSpan.FromSeconds(MGNunchiDefines.BASEWAIT_TIME))
        .Subscribe(_ =>
        {
            player.PlayerSM.ChangeState(MGNunchiPlayerState_Finish.Instance());
            stateChangeDisposer.Dispose();
        })
        .AddTo(player.WorldMgr);
    }
    public override void Excute(MGNunchiPlayer player)
    {
    }

    public override void Exit(MGNunchiPlayer player)
    {
    }
}


public class MGNunchiPlayerState_Finish : CState<MGNunchiPlayer>
{
    static MGNunchiPlayerState_Finish s_this = null;

    public static MGNunchiPlayerState_Finish Instance()
    {
        if (s_this == null) s_this = new MGNunchiPlayerState_Finish();
        return s_this;
    }

    public override void Enter(MGNunchiPlayer player)
    {
        player.SetReturnFinished(true);
        player.PlayerSM.ChangeState(MGNunchiPlayerState_Ready.Instance());
    }
    public override void Excute(MGNunchiPlayer player)
    {
    }

    public override void Exit(MGNunchiPlayer player)
    {
        //CDebug.Log("    ### CollectCoinPlayerState_Return.Exit()", CDebugTag.MINIGAME_NUNCHI);
    }
}


public class MGNunchiPlayerState_Return : CState<MGNunchiPlayer>
{
    static MGNunchiPlayerState_Return s_this = null;

    public static MGNunchiPlayerState_Return Instance()
    {
        if (s_this == null) s_this = new MGNunchiPlayerState_Return();
        return s_this;
    }
     
    public override void Enter(MGNunchiPlayer player)
    {
        player.PlayMySound( 6830014 ); // list 113
        player.SetJump(player.GetMapPositionByIndex(player.PlayerInfo.Map_MyLocation), player.PlayerInfo.Map_MyLocation);

		var disposer = new SingleAssignmentDisposable();
		disposer.Disposable = Observable.Timer( TimeSpan.FromSeconds( MGNunchiDefines.JUMP_DUR_TIME + 0.1f ) )
		.Subscribe( _ =>
        {
            player.PlayMySound( 6830015 ); // list 114
            player.PlayerController.SetAnimationTrigger( MGNunchiDefines.ANIM_IDLE );//

			disposer.Dispose();
		 } )
		.AddTo( player.WorldMgr );

		var changeDisposer = new SingleAssignmentDisposable();
        changeDisposer.Disposable = Observable.Timer(TimeSpan.FromSeconds(1 + MGNunchiDefines.ROTATION_TIME))
        .Subscribe(_ =>
        {
            player.SetPlayerObject();
            player.PlayerController.SetAnimationTrigger(MGNunchiDefines.ANIM_IDLE);//
            player.PlayerSM.ChangeState(MGNunchiPlayerState_Finish.Instance());

            changeDisposer.Dispose();
        })
        .AddTo(player.WorldMgr);
    }
    public override void Excute(MGNunchiPlayer player)
    {
    }

    public override void Exit(MGNunchiPlayer player)
    {
        //CDebug.Log("    ### CollectCoinPlayerState_Return.Exit()", CDebugTag.MINIGAME_NUNCHI);
    }
}


public class MGNunchiPlayerState_Result : CState<MGNunchiPlayer>
{
    static MGNunchiPlayerState_Result s_this = null;

    public static MGNunchiPlayerState_Result Instance()
    {
        if (s_this == null) s_this = new MGNunchiPlayerState_Result();
        return s_this;
    }

    public override void Enter(MGNunchiPlayer player)
    {
        if(player.PlayerInfo.UID != player.WorldMgr.PlayerMgr.MyPlayerUID)
        {
            player.PlayerInfo.PlayerObj.SetActive(false);
        }

        player.SetReturnFinished(true);

        TournamentInfo _roomInfo = MGNunchiServerDataManager.Instance.GetTournamentGameRoomInfo();
        int myRank = MGNunchiServerDataManager.Instance.GetMyStageRank();

        CDebug.Log($"MGNunchiPlayerState_Result :: Enter(MGNunchiPlayer player) - _roomInfo.StageType - {_roomInfo.StageType}, myRank - {myRank}", CDebugTag.MINIGAME_NUNCHI);
    


        if(player.IsMyPlayer())
        {
            if (_roomInfo.StageType == BPWPacketDefine.NunchiGameStageType.TRYOUT)
            {
                switch (myRank)
                {
                    case 1:
                        player.PlayerController.SetAnimationTrigger( MGNunchiDefines.ANIM_VICTORY_TURN );
                        CDebug.Log( " MGNunchiPlayerState_Result TRYOUT rank 1 victory sound" );
                        SoundManager.Instance.PlayEffect( 6830020 ); // list 119
                        break;
                    case 2:
                        player.PlayerController.SetAnimationTrigger( MGNunchiDefines.ANIM_VICTORY );
                        CDebug.Log( " MGNunchiPlayerState_Result TRYOUT rank 2 victory sound" );
                        SoundManager.Instance.PlayEffect( 6830020 ); // list 119
                        break;
                    case 3:
                    case 4:
                        CDebug.Log( " MGNunchiPlayerState_Result TRYOUT rank 3,4 fail sound" );
                        player.PlayerController.SetAnimationTrigger( MGNunchiDefines.ANIM_FAIL );
                        SoundManager.Instance.PlayEffect( 6830021 ); // list 120m
                        break;
                }
            }
            else if (_roomInfo.StageType == BPWPacketDefine.NunchiGameStageType.FINAL)
            {
                switch (myRank)
                {
                    case 1:
                        player.PlayerController.SetAnimationTrigger( MGNunchiDefines.ANIM_VICTORY_TURN );
                        CDebug.Log( " MGNunchiPlayerState_Result FINAL rank 1 victory sound" );
                        SoundManager.Instance.PlayEffect( 6830020 ); // list 119
                        break;
                    case 2:
                    case 3:
                        player.PlayerController.SetAnimationTrigger( MGNunchiDefines.ANIM_VICTORY );
                        CDebug.Log( " MGNunchiPlayerState_Result FINAL rank 2, 3 victory sound" );
                        SoundManager.Instance.PlayEffect( 6830020 ); // list 119
                        break;
                    case 4:
                        player.PlayerController.SetAnimationTrigger( MGNunchiDefines.ANIM_FAIL );
                        CDebug.Log( " MGNunchiPlayerState_Result FINAL rank 4 fail sound" );
                        SoundManager.Instance.PlayEffect( 6830021 ); // list 120
                        break;
                }
            }
        }
        
        //float animTime = 1.5f;
        //bool bWinner = false;

        //if (_roomInfo.StageType == BPWPacketDefine.NunchiGameStageType.TRYOUT)
        //{
        //    if (myRank <= MGNunchiDefines.FINALIST_RANK)
        //    {
        //        bWinner = true;
        //    }
        //}
        //else if (_roomInfo.StageType == BPWPacketDefine.NunchiGameStageType.FINAL)
        //{
        //    if (myRank == 1)
        //    {
        //        bWinner = true;
        //    }
        //}

        //if (bWinner)
        //{
        //    player.PlayerController.SetAnimationTrigger(MGNunchiDefines.ANIM_VICTORY);
        //}
        //else
        //{
        //    player.PlayerController.SetAnimationTrigger(MGNunchiDefines.ANIM_FAIL);
        //}
    }
    public override void Excute(MGNunchiPlayer player){ }

    public override void Exit(MGNunchiPlayer player){ }
}



