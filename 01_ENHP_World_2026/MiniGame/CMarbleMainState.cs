using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CMarbleMainState_Idle : CState<CMarbleWorldManager>
{
    private static CMarbleMainState_Idle s_this;
    public static CMarbleMainState_Idle Instance () => s_this ??= new CMarbleMainState_Idle ();
    
    public override void Enter (CMarbleWorldManager worldMgr)
    {
        worldMgr.SetBlockDiceRolling(false);
        worldMgr.SetPlayerState(MARBLE_PLAYER_STATE.IDLE);

        if (worldMgr.GetIsAutoMode())
        {
            worldMgr.StartAutoRolling();
        }
    }

    public override void Excute (CMarbleWorldManager worldMgr) 
    { 
        
    }
    public override void Exit (CMarbleWorldManager worldMgr)
    {
    }
}

public class CMarbleMainState_Play : CState<CMarbleWorldManager>
{
    private static CMarbleMainState_Play s_this;
    public static CMarbleMainState_Play Instance () => s_this ??= new CMarbleMainState_Play ();
    
    public override void Enter (CMarbleWorldManager worldMgr)
    {
        worldMgr.SetPlayerState(MARBLE_PLAYER_STATE.PLAY);
        worldMgr.SetFollowTarget();
        //worldMgr.FollowCamToPlayer();
    }

    public override void Excute (CMarbleWorldManager worldMgr) 
    { 
        worldMgr.FollowCamToPlayer();
        // if (worldMgr.IsBlockFollowCam())
        // {
        //     worldMgr.StopFollowCam();
        //     return;
        // }
    }
    public override void Exit (CMarbleWorldManager worldMgr)
    {
        worldMgr.StopFollowCam();
    }
}

public class CMarbleMainState_CamActionRestore : CState<CMarbleWorldManager>
{
    private static CMarbleMainState_CamActionRestore s_this;
    public static CMarbleMainState_CamActionRestore Instance () => s_this ??= new CMarbleMainState_CamActionRestore ();
    
    public override void Enter (CMarbleWorldManager worldMgr)
    {
        worldMgr.RestoreCamPosition();
        //worldMgr.SetActiveMoveNoticeUI(true);
        worldMgr.SetPlayerState(MARBLE_PLAYER_STATE.WAIT);
    }

    public override void Excute (CMarbleWorldManager worldMgr) 
    { 
    }
    public override void Exit (CMarbleWorldManager worldMgr)
    {
        //worldMgr.SetActiveMoveNoticeUI(false);
        worldMgr.CamFocusToTarget();
    }
}


public class CMarbleMainState_FlyBuffEff : CState<CMarbleWorldManager>
{
    private static CMarbleMainState_FlyBuffEff s_this;
    public static CMarbleMainState_FlyBuffEff Instance () => s_this ??= new CMarbleMainState_FlyBuffEff ();
    
    public override void Enter (CMarbleWorldManager worldMgr)
    {
        worldMgr.RestoreCamPosition();//CamFocusToTarget();
        worldMgr.SetActiveMoveNoticeUI(true);
        worldMgr.SetPlayerState(MARBLE_PLAYER_STATE.WAIT);
    }

    public override void Excute (CMarbleWorldManager worldMgr) 
    { 
    }
    public override void Exit (CMarbleWorldManager worldMgr)
    {
        worldMgr.SetActiveMoveNoticeUI(false);
        worldMgr.CamFocusToTarget();
    }
}


public class CMarbleMainState_BlockRenewal : CState<CMarbleWorldManager>
{
    private static CMarbleMainState_BlockRenewal s_this;
    public static CMarbleMainState_BlockRenewal Instance () => s_this ??= new CMarbleMainState_BlockRenewal ();
    
    public override void Enter (CMarbleWorldManager worldMgr)
    {
    }

    public override void Excute (CMarbleWorldManager worldMgr) 
    { 
    }
    public override void Exit (CMarbleWorldManager worldMgr)
    {
    }
}
