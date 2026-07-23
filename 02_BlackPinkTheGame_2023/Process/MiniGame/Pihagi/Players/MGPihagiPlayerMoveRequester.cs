using UnityEngine;
using UniRx;
using RequesterState = EBPWorldCharacterMoveRequesterState;
using System;

public class MGPihagiPlayerMoveRequester
{
    private RequesterState MoveState = RequesterState.STOP;
    private float elapsedTime = 0.0f;
    private MGPihagiPlayer targetNetPlayer = null;
    private CCharacterController_BPWorld targetNetPlayerCntlr = null;

    private SingleAssignmentDisposable updateDisposable = null;

    public void Initialize()
    {
        SetUpdater();
    }

    public void SetTargetPlayer(MGPihagiPlayer netPlayer)
    {
        this.targetNetPlayer = netPlayer;
    }

    public void SetTargetPlayerController(CCharacterController_BPWorld cntlr)
    {
        this.targetNetPlayerCntlr = cntlr;
    }

    private void SetUpdater()
    {
        if (updateDisposable != null && updateDisposable.IsDisposed == false)
        {
            updateDisposable.Dispose();
        }

        updateDisposable = new SingleAssignmentDisposable();
        
        updateDisposable.Disposable = Observable.EveryFixedUpdate().Subscribe(EveryFixedUpdate);
    }

    public void SetMoveState(RequesterState state)
    {
        MoveState = state;
    }

    private void EveryFixedUpdate(long frame)
    {
        if (MoveState == RequesterState.STOP)
        {
            return;
        }

        //if (targetNetPlayer == null)
        if(targetNetPlayerCntlr == null)
        {
            return;
        }

        elapsedTime += Time.fixedDeltaTime;
        if (elapsedTime >= MGPihagiDefine.REQUEST_MOVESEQ)
        {
            ExecuteRequest();
        }
    }


    private void ExecuteRequest()
    {
        //CCharacterController_BPWorld characterController = targetNetPlayer.GetCharacterController();
        elapsedTime = 0.0f;
        if (targetNetPlayer != null)
        {
            if(targetNetPlayer.GetPlayerState() == BPWPacketDefine.PihagiGamePlayerState.DEATH)
            {
                return;
            }
        }

        if (targetNetPlayerCntlr == null)
        {
            return;
        }

        Vector3 position = targetNetPlayerCntlr.GetPosition();
        Vector3 direction = targetNetPlayerCntlr.GetMoveDirection();
        
        TCPMGPihagiRequestManager.Instance.Req_GamePlayerMove(position, direction);
    }
}
