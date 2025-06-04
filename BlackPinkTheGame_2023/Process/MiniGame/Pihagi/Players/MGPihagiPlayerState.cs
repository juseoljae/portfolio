
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



public class MGPihagiPlayerState_Idle : CState<MGPihagiPlayer>
{
    static MGPihagiPlayerState_Idle s_this = null;

    public static MGPihagiPlayerState_Idle Instance()
    {
        if (s_this == null) s_this = new MGPihagiPlayerState_Idle();
        return s_this;
    }

    public override void Enter(MGPihagiPlayer player)
    {
        //player.SetAnimationTrigger("idle");
    }

    public override void Excute(MGPihagiPlayer player) { }

    public override void Exit(MGPihagiPlayer player) { }
}



public class MGPihagiPlayerState_Move : CState<MGPihagiPlayer>
{
    static MGPihagiPlayerState_Move s_this = null;

    public static MGPihagiPlayerState_Move Instance()
    {
        if (s_this == null) s_this = new MGPihagiPlayerState_Move();
        return s_this;
    }

    public override void Enter(MGPihagiPlayer player)
    {
        player.SetAnimationTrigger("run");
    }

    public override void Excute(MGPihagiPlayer player) { }

    public override void Exit(MGPihagiPlayer player){ }
}

public class MGPihagiPlayerState_MoveStop : CState<MGPihagiPlayer>
{
    static MGPihagiPlayerState_MoveStop s_this = null;

    public static MGPihagiPlayerState_MoveStop Instance()
    {
        if (s_this == null) s_this = new MGPihagiPlayerState_MoveStop();
        return s_this;
    }

    public override void Enter(MGPihagiPlayer player)
    {
		if(player.PlayersMgr.MyPlayerUID != player.PlayerInfo.UID)
		{
		  player.SetAnimationTrigger("idle");
		}
    }

    public override void Excute(MGPihagiPlayer player) { }

    public override void Exit(MGPihagiPlayer player) { }
}


public class MGPihagiPlayerState_Stun : CState<MGPihagiPlayer>
{
    static MGPihagiPlayerState_Stun s_this = null;

    public static MGPihagiPlayerState_Stun Instance()
    {
        if (s_this == null) s_this = new MGPihagiPlayerState_Stun();
        return s_this;
    }

    public override void Enter(MGPihagiPlayer player)
    {
        player.SetStun();
        SoundManager.Instance.PlayEffect( 6830027 ); // list 127
        player.SetAnimationTrigger("game_twinkle");     
		
		if(player.PlayerInfo.UID == player.WorldMgr.PlayerMgr.MyPlayerUID)
		{
			player.LockJoyStick();
			var disposer = new SingleAssignmentDisposable();
			disposer.Disposable = Observable.Timer(TimeSpan.FromSeconds(1))
			.Subscribe(_ =>
			{
				player.SetMyPlayerControl();
				disposer.Dispose();
			})
			.AddTo(player.WorldMgr);   
		}
    }

    public override void Excute(MGPihagiPlayer player) { }

    public override void Exit(MGPihagiPlayer player) 
    {
		if(player.PlayerInfo.UID == player.WorldMgr.PlayerMgr.MyPlayerUID)
		{
			player.UnlockJoyStick();
		}
        player.ReleaseStun();
    }
}


//public class MGPihagiPlayerState_Invincible : CState<MGPihagiPlayer>
//{
//    static MGPihagiPlayerState_Invincible s_this = null;

//    public static MGPihagiPlayerState_Invincible Instance()
//    {
//        if (s_this == null) s_this = new MGPihagiPlayerState_Invincible();
//        return s_this;
//    }

//    public override void Enter(MGPihagiPlayer player)
//    {
//        player.SetInvincible();
//    }

//    public override void Excute(MGPihagiPlayer player) { }

//    public override void Exit(MGPihagiPlayer player)
//    {
//        player.ReleaseInvincible();
//    }
//}




public class MGPihagiPlayerState_Death : CState<MGPihagiPlayer>
{
    static MGPihagiPlayerState_Death s_this = null;

    public static MGPihagiPlayerState_Death Instance()
    {
        if (s_this == null) s_this = new MGPihagiPlayerState_Death();
        return s_this;
    }

    public override void Enter(MGPihagiPlayer player)
    {
        //player.SetLifeCount(0);
		
		if(player.PlayerInfo.UID == player.PlayersMgr.MyPlayerUID)
		{
			player.PlayersMgr.SetMyPlayerMoveState(EBPWorldCharacterMoveRequesterState.STOP);
		}
        SoundManager.Instance.PlayEffect( 6830028 ); // list 128
        player.WorldMgr.PlayerMgr.SetPlayerLife(player.PlayerInfo.PlayerID, 0);
        player.SetAnimationTrigger("game_knockdown");

		player.SetDeath();
        player.ReleaseInvincible();
        //Vector3 targetPos = player.GetDeathTargetPosition();
        //player.PlayerInfo.PlayerObj.transform.DOLocalMove(targetPos, 3)
        //    .OnComplete(() =>
        //    {
        //        player.SetActivePlayerObj(false);
        //        player.SetChangeState(MGPihagiPlayerState_Observe.Instance());
        //    });
    }

    public override void Excute(MGPihagiPlayer player)
    {
    }

    public override void Exit(MGPihagiPlayer player) { }
}


public class MGPihagiPlayerState_Stop : CState<MGPihagiPlayer>
{
    static MGPihagiPlayerState_Stop s_this = null;

    public static MGPihagiPlayerState_Stop Instance()
    {
        if (s_this == null) s_this = new MGPihagiPlayerState_Stop();
        return s_this;
    }

    public override void Enter(MGPihagiPlayer player)
    {
        player.SetAnimationTrigger("idle");
        player.ReleaseInvincible();


        if (player.PlayerInfo.UID == player.PlayersMgr.MyPlayerUID)
		{
			player.PlayersMgr.SetMyPlayerMoveState(EBPWorldCharacterMoveRequesterState.STOP);
			player.PlayerController.SetChangeStateMachine(CharState_BPWorld_MetaverseSpawnWait.Instance());
		}
    }

    public override void Excute(MGPihagiPlayer player) { }

    public override void Exit(MGPihagiPlayer player) { }
}


public class MGPihagiPlayerState_Observe : CState<MGPihagiPlayer>
{
    static MGPihagiPlayerState_Observe s_this = null;

    public static MGPihagiPlayerState_Observe Instance()
    {
        if (s_this == null) s_this = new MGPihagiPlayerState_Observe();
        return s_this;
    }

    public override void Enter(MGPihagiPlayer player)
    {
		if(player.PlayerInfo.UID == player.PlayersMgr.MyPlayerUID)
		{
			player.PlayersMgr.SetMyPlayerMoveState(EBPWorldCharacterMoveRequesterState.STOP);
			player.WorldMgr.GetPageUI().SetActiveJoyStickGroupUI(false);
		}
    }

    public override void Excute(MGPihagiPlayer player) { }

    public override void Exit(MGPihagiPlayer player) { }
}

