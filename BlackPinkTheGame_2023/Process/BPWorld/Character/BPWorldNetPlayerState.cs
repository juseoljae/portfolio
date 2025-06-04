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

public class BPWorldNetPlayerState_Wait : CState<BPWorldNetPlayer>
{
    static BPWorldNetPlayerState_Wait s_this = null;

    public static BPWorldNetPlayerState_Wait Instance()
    {
        if (s_this == null)
        {
            s_this = new BPWorldNetPlayerState_Wait();
        }
        return s_this;
    }

    public override void Enter(BPWorldNetPlayer player)
    {
    }

    public override void Excute(BPWorldNetPlayer player)
    {
    }

    public override void Exit(BPWorldNetPlayer player)
    {
    }
}

public class BPWorldNetPlayerState_FarLODAreaEnter : CState<BPWorldNetPlayer>
{
    static BPWorldNetPlayerState_FarLODAreaEnter s_this = null;

    public static BPWorldNetPlayerState_FarLODAreaEnter Instance()
    {
        if (s_this == null)
        {
            s_this = new BPWorldNetPlayerState_FarLODAreaEnter();
        }
        return s_this;
    }

    public override void Enter(BPWorldNetPlayer player)
    {
        CDebug.Log( "   ****** BPWorldNetPlayerState_FarLODAreaEnter.Enter()" );
        player.SetActiveDynamicBone( true );
        //player.SetChangeLODState( BPWorldNetPlayerState_Wait.Instance() );
    }

    public override void Excute(BPWorldNetPlayer player)
    {
    }

    public override void Exit(BPWorldNetPlayer player)
    {
    }
}

public class BPWorldNetPlayerState_FarLODAreaExit : CState<BPWorldNetPlayer>
{
    static BPWorldNetPlayerState_FarLODAreaExit s_this = null;

    public static BPWorldNetPlayerState_FarLODAreaExit Instance()
    {
        if (s_this == null)
        {
            s_this = new BPWorldNetPlayerState_FarLODAreaExit();
        }
        return s_this;
    }

    public override void Enter(BPWorldNetPlayer player)
    {
        player.SetActiveDynamicBone( false );
        //player.SetChangeLODState( BPWorldNetPlayerState_Wait.Instance() );
    }

    public override void Excute(BPWorldNetPlayer player)
    {
    }

    public override void Exit(BPWorldNetPlayer player)
    {
    }
}

