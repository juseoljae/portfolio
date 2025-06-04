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


public class EVDoorSM_Wait : CState<CEVDoorController>
{
    static EVDoorSM_Wait s_this = null;

    public static EVDoorSM_Wait Instance()
    {
        if (s_this == null) { s_this = new EVDoorSM_Wait(); }
        return s_this;
    }

    public override void Enter(CEVDoorController controller)
    {
        //CDebug.Log($"    %%%% EVDoorSM_Wait.Enter() floor = {controller.GetEVFloor()}");
    }

    public override void Excute(CEVDoorController controller)
    {
    }

    public override void Exit(CEVDoorController controller)
    {
        //CDebug.Log($"    %%%% EVDoorSM_Wait.Exit() floor = {controller.GetEVFloor()}");

    }
}

public class EVDoorSM_OpenSFDoor : CState<CEVDoorController>
{
    static EVDoorSM_OpenSFDoor s_this = null;

    public static EVDoorSM_OpenSFDoor Instance()
    {
        if (s_this == null) { s_this = new EVDoorSM_OpenSFDoor(); }
        return s_this;
    }

    public override void Enter(CEVDoorController controller)
    {
       //CDebug.Log($"    %%%% EVDoorSM_OpenSFDoor.Enter() floor = {controller.GetEVFloor()}");
        if (controller.HasDoorOpened() == false)
        {
            //CDebug.Log("    %%%% EVDoorSM_OpenDoor.Enter() floor = " + controller.MyFloor);
            controller.MyAnimator.SetTrigger("open");

            controller.InitCheckVar();
            controller.OpenSFDoor();
        }
    }

    public override void Excute(CEVDoorController controller)
    {
    }

    public override void Exit(CEVDoorController controller)
    {
        //CDebug.Log($"    %%%% EVDoorSM_OpenSFDoor.Exit() floor = {controller.GetEVFloor()}");
    }
}

public class EVDoorSM_CloseSFDoor : CState<CEVDoorController>
{
    static EVDoorSM_CloseSFDoor s_this = null;

    public static EVDoorSM_CloseSFDoor Instance()
    {
        if (s_this == null) { s_this = new EVDoorSM_CloseSFDoor(); }
        return s_this;
    }

    public override void Enter(CEVDoorController controller)
    {
        //CDebug.Log($"    %%%% EVDoorSM_CloseSFDoor.Enter() floor = {controller.GetEVFloor()}");
        if (controller.HasDoorClosed() == false)
        {
            //CDebug.Log("    %%%% EVDoorSM_CloseDoor.Enter() floor = " + controller.MyFloor);
            controller.MyAnimator.SetTrigger("close");
            controller.CloseDoor();
        }
    }

    public override void Excute(CEVDoorController controller)
    {
    }

    public override void Exit(CEVDoorController controller)
    {
        //CDebug.Log($"    %%%% EVDoorSM_CloseSFDoor.Exit() floor = {controller.GetEVFloor()}");
        //controller.SetFinishDoorClose(false);
        //TF
        //controller.SetFinishAllAvatarArrivedAtTargetFloor(false);
    }
}

/// <summary>
/// Open door state when avatars arrived their target floor
/// </summary>
public class EVDoorSM_OpenTFDoor : CState<CEVDoorController>
{
    static EVDoorSM_OpenTFDoor s_this = null;

    public static EVDoorSM_OpenTFDoor Instance()
    {
        if (s_this == null) { s_this = new EVDoorSM_OpenTFDoor(); }
        return s_this;
    }

    public override void Enter(CEVDoorController controller)
    {
        //CDebug.Log($"    %%%% EVDoorSM_OpenTFDoor.Enter() floor = {controller.GetEVFloor()}");
        if (controller.HasDoorOpened() == false)
        {
            //CDebug.Log("    %%%% EVDoorSM_OpenTFDoor.Enter() floor = " + controller.MyFloor);
            controller.MyAnimator.SetTrigger("open");
            controller.OpenTFDoor();
        }
    }

    public override void Excute(CEVDoorController controller)
    {
    }

    public override void Exit(CEVDoorController controller)
    {
        //CDebug.Log($"    %%%% EVDoorSM_OpenTFDoor.Exit() floor = {controller.GetEVFloor()}");

    }
}



public class EVDoorSM_CloseTFDoor : CState<CEVDoorController>
{
    static EVDoorSM_CloseTFDoor s_this = null;

    public static EVDoorSM_CloseTFDoor Instance()
    {
        if (s_this == null) { s_this = new EVDoorSM_CloseTFDoor(); }
        return s_this;
    }

    public override void Enter(CEVDoorController controller)
    {
        //CDebug.Log($"    %%%% EVDoorSM_CloseTFDoor.Enter() floor = {controller.GetEVFloor()}");
        if (controller.HasDoorClosed() == false)
        {
            controller.MyAnimator.SetTrigger("close");
            controller.SetChangeEVDoorState(EVDoorSM_Wait.Instance());
            controller.CloseDoor();
        }
    }

    public override void Excute(CEVDoorController controller)
    {
    }

    public override void Exit(CEVDoorController controller)
    {
        //CDebug.Log($"    %%%% EVDoorSM_CloseTFDoor.Exit() floor = {controller.GetEVFloor()}");

        //controller.SetFinishDoorClose(false);
        //TF
        //controller.SetFinishAllAvatarArrivedAtTargetFloor(false);
    }
}