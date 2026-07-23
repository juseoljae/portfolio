
using UnityEngine;
using UniRx;
using System;

public class MarbleDiceState_Idle : CState<CMarbleDice>
{
    static MarbleDiceState_Idle s_this = null;

    public static MarbleDiceState_Idle Instance()
    {
        if (s_this == null) s_this = new MarbleDiceState_Idle();
        return s_this;
    }

    public override void Enter(CMarbleDice dice)
    {
    }

    public override void Excute(CMarbleDice dice) { }

    public override void Exit(CMarbleDice dice)
    {
    }
}

public class MarbleDiceState_ReadyToWork : CState<CMarbleDice>
{
    static MarbleDiceState_ReadyToWork s_this = null;

    public static MarbleDiceState_ReadyToWork Instance()
    {
        if (s_this == null) s_this = new MarbleDiceState_ReadyToWork();
        return s_this;
    }

    public override void Enter(CMarbleDice dice)
    {
        dice.SetDiceObjActive(false);
        dice.SetDiceStartPosition();
        
        var disposer = new SingleAssignmentDisposable();
        disposer.Disposable = Observable.Timer(TimeSpan.FromSeconds(0.1f))
        .Subscribe(_ =>
        {
            dice.SetDiceObjActive(true);
            dice.SetDiceState(DICE_STATE.WORK);

            disposer.Dispose();
        })
        .AddTo(dice);
    }

    public override void Excute(CMarbleDice dice) { }

    public override void Exit(CMarbleDice dice)
    {
    }
}

public class MarbleDiceState_Work : CState<CMarbleDice>
{
    static MarbleDiceState_Work s_this = null;

    public static MarbleDiceState_Work Instance()
    {
        if (s_this == null) s_this = new MarbleDiceState_Work();
        return s_this;
    }

    public override void Enter(CMarbleDice dice)
    {        
        CSoundControl.Instance.EffectPlay(SOUND_ID.SFX_DICEGAME_DICEROLL);
        dice.SetAnimatorTrigger(CMarbleDefine.ANIM_NAME_DICE_WORK);
        dice.MoveToTarget();
    }

    public override void Excute(CMarbleDice dice) { }

    public override void Exit(CMarbleDice dice)
    {
    }
}

public class MarbleDiceState_WorkFinish : CState<CMarbleDice>
{
    static MarbleDiceState_WorkFinish s_this = null;

    public static MarbleDiceState_WorkFinish Instance()
    {
        if (s_this == null) s_this = new MarbleDiceState_WorkFinish();
        return s_this;
    }

    public override void Enter(CMarbleDice dice)
    {
        dice.WorldMgr.SetMainState(MARBLE_GAME_STATE.PLAY);
    }

    public override void Excute(CMarbleDice dice) { }

    public override void Exit(CMarbleDice dice)
    {
    }
}