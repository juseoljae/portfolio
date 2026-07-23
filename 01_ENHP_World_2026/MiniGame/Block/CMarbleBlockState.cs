using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UniRx;


public class CMarbleBlockState_Idle : CState<CMarbleBlock>
{
    private static CMarbleBlockState_Idle s_this;
    public static CMarbleBlockState_Idle Instance() => s_this ??= new CMarbleBlockState_Idle();
    public override void Enter(CMarbleBlock block)
    {
        block.SetAnimationTrigger(CMarbleDefine.ANIM_NAME_BLOCK_RESET);
    }

    public override void Excute(CMarbleBlock block) { }
    public override void Exit(CMarbleBlock block)
    {
    }
}

public class CMarbleBlockStat_Renewal : CState<CMarbleBlock>
{
    private static CMarbleBlockStat_Renewal s_this;
    public static CMarbleBlockStat_Renewal Instance() => s_this ??= new CMarbleBlockStat_Renewal();

    public override void Enter(CMarbleBlock block)
    {
        block.SetActiveNoneBlock(false);
    }

    public override void Excute(CMarbleBlock block) { }
    public override void Exit(CMarbleBlock block)
    {
    }
}

public class CMarbleBlockStat_Reward : CState<CMarbleBlock>
{
    private static CMarbleBlockStat_Reward s_this;
    public static CMarbleBlockStat_Reward Instance() => s_this ??= new CMarbleBlockStat_Reward();

    public override void Enter(CMarbleBlock block)
    {
        block.SetRewardState();
    }

    public override void Excute(CMarbleBlock block) { }
    public override void Exit(CMarbleBlock block)
    {
    }
}

public class CMarbleBlockStat_Active : CState<CMarbleBlock>
{
    private static CMarbleBlockStat_Active s_this;
    public static CMarbleBlockStat_Active Instance() => s_this ??= new CMarbleBlockStat_Active();

    public override void Enter(CMarbleBlock block)
    {
        block.SetBlockActive();
    }

    public override void Excute(CMarbleBlock block) { }
    public override void Exit(CMarbleBlock block)
    {
    }
}

public class CMarbleBlockStat_InAcive : CState<CMarbleBlock>
{
    private static CMarbleBlockStat_InAcive s_this;
    public static CMarbleBlockStat_InAcive Instance() => s_this ??= new CMarbleBlockStat_InAcive();

    public override void Enter(CMarbleBlock block)
    {
        block.SetBlockInActive();
    }

    public override void Excute(CMarbleBlock block) { }
    public override void Exit(CMarbleBlock block)
    {
    }
}

public class CMarbleBlockStat_Touchable : CState<CMarbleBlock>
{
    private static CMarbleBlockStat_Touchable s_this;
    public static CMarbleBlockStat_Touchable Instance() => s_this ??= new CMarbleBlockStat_Touchable();

    public override void Enter(CMarbleBlock block)
    {
        block.SetActiveArrow();
    }

    public override void Excute(CMarbleBlock block) { }
    public override void Exit(CMarbleBlock block)
    {
    }
}

public class CMarbleBlockStat_ChangeNone : CState<CMarbleBlock>
{
    private static CMarbleBlockStat_ChangeNone s_this;
    public static CMarbleBlockStat_ChangeNone Instance() => s_this ??= new CMarbleBlockStat_ChangeNone();

    public override void Enter(CMarbleBlock block)
    {
        block.SetActiveNoneBlock(true);
    }

    public override void Excute(CMarbleBlock block) { }
    public override void Exit(CMarbleBlock block)
    {
    }
}

public class CMarbleBlockStat_PlayerArrive : CState<CMarbleBlock>
{
    private static CMarbleBlockStat_PlayerArrive s_this;
    public static CMarbleBlockStat_PlayerArrive Instance() => s_this ??= new CMarbleBlockStat_PlayerArrive();

    public override void Enter(CMarbleBlock block)
    {
        block.SetPlayerArrive();
    }

    public override void Excute(CMarbleBlock block) { }
    public override void Exit(CMarbleBlock block)
    {
    }
}


// Buff target block state
public class CMarbleBlockStat_Buff : CState<CMarbleBlock>
{
    private static CMarbleBlockStat_Buff s_this;
    public static CMarbleBlockStat_Buff Instance() => s_this ??= new CMarbleBlockStat_Buff();

    public override void Enter(CMarbleBlock block)
    {
        block.FlyBlockFlyBuffEffObj(true);
    }

    public override void Excute(CMarbleBlock block) { }
    public override void Exit(CMarbleBlock block)
    {
    }
}

// DeBuff target block state
public class CMarbleBlockStat_DeBuff : CState<CMarbleBlock>
{
    private static CMarbleBlockStat_DeBuff s_this;
    public static CMarbleBlockStat_DeBuff Instance() => s_this ??= new CMarbleBlockStat_DeBuff();

    public override void Enter(CMarbleBlock block)
    {
        block.FlyBlockFlyBuffEffObj(false);
    }

    public override void Excute(CMarbleBlock block) { }
    public override void Exit(CMarbleBlock block)
    {
    }
}
