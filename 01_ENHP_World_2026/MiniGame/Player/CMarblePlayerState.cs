using System;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public class CMarblePlayerState_Stand : CState<CMarblePlayer>
{
    static CMarblePlayerState_Stand s_this = null;
    public SerialDisposable loopDisposable = new SerialDisposable();

    public static CMarblePlayerState_Stand Instance()
    {
        if (s_this == null)
        {
            s_this = new CMarblePlayerState_Stand();
        }
        return s_this;
    }

    public override void Enter(CMarblePlayer player)
    {
        player.SetAnimationTrigger(CMarbleDefine.ANIM_NAME_PLAYER_STAND);
        StartRandomAnimationLoop(player);
    }

    private void StartRandomAnimationLoop(CMarblePlayer player)
    {        
        float waitTime = UnityEngine.Random.Range(2f, 4f);
        
        loopDisposable.Disposable = Observable.Timer(TimeSpan.FromSeconds(waitTime))
            .Subscribe(_ =>
            {
                int randIdx = UnityEngine.Random.Range(1, 3);
                string param = string.Format(CMarbleDefine.ANIM_NAME_PLAYER_STAND_VAR, randIdx);
                player.SetAnimationTrigger(param);

                float clipTime = player.GetStandClipTime(randIdx);
                loopDisposable.Disposable = Observable.Timer(TimeSpan.FromSeconds(clipTime))
                    .Subscribe(__ =>
                    {
                        player.SetAnimationTrigger(CMarbleDefine.ANIM_NAME_PLAYER_STAND);
                        StartRandomAnimationLoop(player); 
                    });
            });
    }

    public override void Excute(CMarblePlayer player) { }

    public override void Exit(CMarblePlayer player)
    {
        loopDisposable?.Dispose();
        loopDisposable = new SerialDisposable();
    }
}

public class CMarblePlayerState_Play : CState<CMarblePlayer>
{
    static CMarblePlayerState_Play s_this = null;

    public static CMarblePlayerState_Play Instance()
    {
        if (s_this == null)
        {
            s_this = new CMarblePlayerState_Play();
        }
        return s_this;
    }

    public override void Enter(CMarblePlayer player)
    {
        //stop effect
        player.StopPlayerEffect();
        
        //move
        int moveCount = player.GetMoveCount();

        var worldMgr = player.GetWorldManager();
        if (worldMgr.GetIsFastMove())
        {
            CSoundControl.Instance.EffectPlay(SOUND_ID.SFX_DICEGAME_CHAR_MOVE_FAST);
            player.MoveFast(moveCount);
        }
        else
        {
            player.SetTargetBlockSpecialType(moveCount);

            string param = CMarbleDefine.ANIM_NAME_PLAYER_MOVE_READY;
            float clipTime = player.GetMoveStartClipTime();
            bool isTargetBlockSppecial = player.GetTargetBlockSpecialType();
            if (isTargetBlockSppecial)
            {
                param = CMarbleDefine.ANIM_NAME_PLAYER_SMOVE_READY;
                clipTime = player.GetSMoveStartClipTime();
            }
            player.SetAnimationTrigger(param);
            
            var disposer = new SingleAssignmentDisposable();
            disposer.Disposable = Observable.Timer(TimeSpan.FromSeconds(clipTime))
            .Subscribe(_ =>
            {            
                player.MoveBlockForward(moveCount, isTargetBlockSppecial);
                disposer.Dispose();
            })
            .AddTo(player);    
        }    
    }

    public override void Excute(CMarblePlayer player)
    {
        
    }

    public override void Exit(CMarblePlayer player){ }
}

public class CMarblePlayerState_Wait : CState<CMarblePlayer>
{
    static CMarblePlayerState_Wait s_this = null;

    public static CMarblePlayerState_Wait Instance()
    {
        if (s_this == null)
        {
            s_this = new CMarblePlayerState_Wait();
        }
        return s_this;
    }

    public override void Enter(CMarblePlayer player)
    {
        // player.SetAnimationTrigger(CMarbleDefine.ANIM_NAME_PLAYER_STAND);
    }

    public override void Excute(CMarblePlayer player) { }

    public override void Exit(CMarblePlayer player){ }
}

public class CMarblePlayerState_Finish : CState<CMarblePlayer>
{
    static CMarblePlayerState_Finish s_this = null;

    public static CMarblePlayerState_Finish Instance()
    {
        if (s_this == null)
        {
            s_this = new CMarblePlayerState_Finish();
        }
        return s_this;
    }

    public override void Enter(CMarblePlayer player)
    {
        player.SetMoveFinish();
    }

    public override void Excute(CMarblePlayer player)
    {
        
    }

    public override void Exit(CMarblePlayer player){ }
}

public class CMarblePlayerState_Arrive : CState<CMarblePlayer>
{
    static CMarblePlayerState_Arrive s_this = null;

    public static CMarblePlayerState_Arrive Instance()
    {
        if (s_this == null)
        {
            s_this = new CMarblePlayerState_Arrive();
        }
        return s_this;
    }

    public override void Enter(CMarblePlayer player)
    {
        player.SetArrive();
    }

    public override void Excute(CMarblePlayer player)
    {
        
    }

    public override void Exit(CMarblePlayer player){ }
}


public class CMarblePlayerState_Touched : CState<CMarblePlayer>
{
    static CMarblePlayerState_Touched s_this = null;

    public static CMarblePlayerState_Touched Instance()
    {
        if (s_this == null)
        {
            s_this = new CMarblePlayerState_Touched();
        }
        return s_this;
    }

    public override void Enter(CMarblePlayer player)
    {
        player.PlayTouchAnim();
    }

    public override void Excute(CMarblePlayer player)
    {
        
    }

    public override void Exit(CMarblePlayer player){ }
}
// 