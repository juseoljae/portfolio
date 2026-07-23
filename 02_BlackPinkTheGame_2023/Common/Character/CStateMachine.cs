using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CStateMachine<entity_type>
{
    public entity_type Owner;
    public CState<entity_type> CurState;
    public CState<entity_type> PreState;
    public CState<entity_type> GlobalState;

    public bool bPause;

    public CStateMachine(entity_type owner)
    {
        Owner = owner;
        CurState = null;
        PreState = null;
        GlobalState = null;

        bPause = false;
    }

    public void ReleaseStateMachine()
    {
        Owner = default;
        CurState = null;
        PreState = null;
        GlobalState = null;
    }

    public void SetCurrentState(CState<entity_type> s) { CurState = s; }
    public void SetGlobalState(CState<entity_type> s) { GlobalState = s; }
    public void SetPreviousState(CState<entity_type> s) { PreState = s; }

    public void StateMachine_Update()
    {
        if(GlobalState != null)
        {
            GlobalState.Excute(Owner);
        }

        if(CurState != null)
        {
            CurState.Excute(Owner);
        }
    }

    public void ChangeState(CState<entity_type> newState)
    {
        PreState = CurState;

        if(CurState != null)
        {
            CurState.Exit(Owner);
        }

        CurState = newState;
        CurState.Enter(Owner);
    }

    public void DestroyStateMachine()
    {
        if (CurState != null) CurState = null;
    }

    public void Pause()
    {
        bPause = true;
    }

    public void Resume()
    {
        bPause = false;
    }

    public bool IsPause()
    {
        return bPause;
    }
    public CState<entity_type> GetCurrentState() { return CurState; }
    public CState<entity_type> GetGlobalState() { return GlobalState; }
    public CState<entity_type> GetPreviousState() { return PreState; }

}

public class CState<entity_type>
{
    public virtual void Enter(entity_type et) { }
    public virtual void OnEnterAsyncLoadComplete() { }
    public virtual void Excute(entity_type et) { }
    public virtual void Exit(entity_type et) { }
    public virtual MOTION_STATE GetEnumType() { return MOTION_STATE.NONE; }
    public virtual void InputProcess(RaycastHit[] hits) { }
}
