using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using DG.Tweening;

public enum MOTION_STATE
{
    NONE = 0,
    SPAWN_METAVERSE,
    WALK_CONTROL,
    WALK_AUTO,
    WALK_ANIM,
    INTERACTION_WITH_OBJ,
    WALK_CONTROL_OTHER_PLAYER,

    //BT
    PLAYANIM,
    PLAYANIM_LOOP,
}

public class CharacterState_ManagementSpawnWait : CState<CCharacterController>
{
    static CharacterState_ManagementSpawnWait s_this = null;

    public static CharacterState_ManagementSpawnWait Instance()
    {
        if (s_this == null)
        {
            s_this = new CharacterState_ManagementSpawnWait();
        }
        return s_this;
    }

    public override void Enter(CCharacterController charController)
    {
        charController.MyAnimator.SetTrigger("idle");
    }

    public override void Excute(CCharacterController charController)
    {
    }

    public override void Exit(CCharacterController charController)
    {
    }
}




public class CharacterState_WalkAuto : CState<CCharacterController>
{
    static CharacterState_WalkAuto s_this = null;

    public static CharacterState_WalkAuto Instance()
    {
        if (s_this == null)
        {
            s_this = new CharacterState_WalkAuto();
        }

        return s_this;
    }

    public override void Enter(CCharacterController charController)
    {
        charController.MyAnimator.SetTrigger("walk");
    }

    public override void Excute(CCharacterController charController)
    {
    }

    public override void Exit(CCharacterController charController) { }
}




public class CharacterState_WalkWithAnim : CState<CCharacterController>
{
    static CharacterState_WalkWithAnim s_this = null;

    public static CharacterState_WalkWithAnim Instance()
    {
        if (s_this == null)
        {
            s_this = new CharacterState_WalkWithAnim();
        }

        return s_this;
    }


    public override void Enter(CCharacterController charController)
    {
        string animParam = charController.GetAnimationParam();
        charController.MyAnimator.speed = charController.AnimSpeed;
        charController.MyAnimator.SetTrigger(animParam);
    }

    public override void Excute(CCharacterController charController)
    {
    }

    public override void Exit(CCharacterController charController)
    {
    }
}




//////// BT
public class CharacterState_PlayAnim : CState<CCharacterController>
{
    static CharacterState_PlayAnim s_this = null;

    public static CharacterState_PlayAnim Instance()
    {
        if (s_this == null)
        {
            s_this = new CharacterState_PlayAnim();
        }
        return s_this;
    }

    public override void Enter(CCharacterController charCntrl)
    {
        string param = charCntrl.AnimationParam;
        charCntrl.MyAnimator.SetTrigger(param);
    }

    public override void Excute(CCharacterController charCntrl)
    {
        if (charCntrl.IsCurrentAnimClipFinished())
        {
            charCntrl.HideAllObject();
            charCntrl.InitAnimPlayingFrame();
        }
    }

    public override void Exit(CCharacterController charCntrl)
    {
        if(charCntrl.GetCharacterType() == CHARACTER_TYPE.AVATAR)
        {
            charCntrl.HideAllObject();
            charCntrl.InitAnimPlayingFrame();
        }
    }
}
public class CharacterState_PlayAnimLoop : CState<CCharacterController>
{
    static CharacterState_PlayAnimLoop s_this = null;

    public static CharacterState_PlayAnimLoop Instance()
    {
        if (s_this == null)
        {
            s_this = new CharacterState_PlayAnimLoop();
        }
        return s_this;
    }

    public override void Enter(CCharacterController charCntrl)
    {
        string param = charCntrl.AnimationParam;
        charCntrl.MyAnimator.SetTrigger(param);
    }

    public override void Excute(CCharacterController charCntrl)
    {
    }

    public override void Exit(CCharacterController charCntrl)
    {
        charCntrl.HideAllObject();
    }
}



public class CharacterState_JumpPrev : CState<CCharacterController>
{
    static CharacterState_JumpPrev s_this = null;

    public static CharacterState_JumpPrev Instance()
    {
        if (s_this == null)
        {
            s_this = new CharacterState_JumpPrev();
        }
        return s_this;
    }

    public override void Enter(CCharacterController charCntrl)
    {
        CCharacterController.JumpInfo _info = charCntrl.GetCurBTJumpInfo();

        charCntrl.MyAnimator.SetTrigger( _info.AnimParams[0] );

        SingleAssignmentDisposable checkDisposer = new SingleAssignmentDisposable();
        checkDisposer.Disposable = Observable.Timer( TimeSpan.FromSeconds( _info.AnimClipLengthes[0] ) )
        .Subscribe( _ =>
        {
            charCntrl.SetChangeState( CharacterState_Jump.Instance() );
            checkDisposer.Dispose();
        } )
        .AddTo( charCntrl );
    }

    public override void Excute(CCharacterController charCntrl)
    {
    }

    public override void Exit(CCharacterController charCntrl)
    {
    }
}

public class CharacterState_Jump : CState<CCharacterController>
{
    static CharacterState_Jump s_this = null;

    public static CharacterState_Jump Instance()
    {
        if (s_this == null)
        {
            s_this = new CharacterState_Jump();
        }
        return s_this;
    }

    public override void Enter(CCharacterController charCntrl)
    {
        CCharacterController.JumpInfo _info = charCntrl.GetCurBTJumpInfo();
        charCntrl.MyAnimator.SetTrigger( _info.AnimParams[1] );

        float totalTime = _info.AnimClipLengthes[1];
        float jumpHeight = _info.Height;

        charCntrl.transform.DOLocalJump( _info.TargetPos, jumpHeight, 1, totalTime ).SetEase(Ease.InQuad);
        if (_info.TargetRot.y != -1)
        {
            charCntrl.SetRotation( _info.TargetRot, 0.5f );
        }
        //change jump state to final
        SingleAssignmentDisposable checkDisposer = new SingleAssignmentDisposable();
        checkDisposer.Disposable = Observable.Timer( TimeSpan.FromSeconds( totalTime ) )
        .Subscribe( _ =>
        {
            charCntrl.SetChangeState( CharacterState_JumpFinal.Instance() );
            checkDisposer.Dispose();
        } )
        .AddTo( charCntrl );
    }

    public override void Excute(CCharacterController charCntrl)
    {

    }

    public override void Exit(CCharacterController charCntrl)
    {
    }

    private void SetJumpHeight(Transform charObj, float jumpValue)
    {
        Vector3 curPos = new Vector3( charObj.localPosition.x, jumpValue, charObj.localPosition.z );
        charObj.localPosition = curPos;
    }
}


public class CharacterState_JumpFinal : CState<CCharacterController>
{
    static CharacterState_JumpFinal s_this = null;

    public static CharacterState_JumpFinal Instance()
    {
        if (s_this == null)
        {
            s_this = new CharacterState_JumpFinal();
        }
        return s_this;
    }

    public override void Enter(CCharacterController charCntrl)
    {
        CCharacterController.JumpInfo _info = charCntrl.GetCurBTJumpInfo();
        charCntrl.MyAnimator.SetTrigger( _info.AnimParams[2] );

        float totalTime = _info.AnimClipLengthes[2];
        SingleAssignmentDisposable checkDisposer = new SingleAssignmentDisposable();
        checkDisposer.Disposable = Observable.Timer( TimeSpan.FromSeconds( totalTime ) )
        .Subscribe( _ =>
        {
            charCntrl.SetChangeState( CharacterState_JumpComplete.Instance() );
            checkDisposer.Dispose();
        } )
        .AddTo( charCntrl );
    }

    public override void Excute(CCharacterController charCntrl)
    {
    }

    public override void Exit(CCharacterController charCntrl)
    {
    }
}

public class CharacterState_JumpComplete : CState<CCharacterController>
{
    static CharacterState_JumpComplete s_this = null;

    public static CharacterState_JumpComplete Instance()
    {
        if (s_this == null)
        {
            s_this = new CharacterState_JumpComplete();
        }
        return s_this;
    }

    public override void Enter(CCharacterController charCntrl)
    {
        //charCntrl.MyAnimator.SetTrigger( "idle" );
    }

    public override void Excute(CCharacterController charCntrl)
    {
    }

    public override void Exit(CCharacterController charCntrl)
    {
    }
}