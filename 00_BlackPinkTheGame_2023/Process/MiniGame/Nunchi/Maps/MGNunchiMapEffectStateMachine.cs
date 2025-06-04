
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
using UnityEngine;
using UniRx;
using DG.Tweening;

public class MGNunchiMapEffectState_Normal : CState<MGNunchiMapInfo>
{
    static MGNunchiMapEffectState_Normal s_this = null;

    public static MGNunchiMapEffectState_Normal Instance()
    {
        if (s_this == null) s_this = new MGNunchiMapEffectState_Normal();
        return s_this;
    }
    public override void Enter (MGNunchiMapInfo info)
    {
        info.SetShaderProp(PLAT_STATE.DEFAULT);
    }
    public override void Excute(MGNunchiMapInfo info) { }
    public override void Exit  (MGNunchiMapInfo info) { }
}

public class MGNunchiMapEffectState_Selectable : CState<MGNunchiMapInfo>
{
    static MGNunchiMapEffectState_Selectable s_this = null;

    public static MGNunchiMapEffectState_Selectable Instance()
    {
        if (s_this == null) s_this = new MGNunchiMapEffectState_Selectable();
        return s_this;
    }
    public override void Enter (MGNunchiMapInfo info) 
    {
        //SoundManager.Instance.PlayEffect( 6830013 ); //list 112
        //init
        info.SetShaderProp_Selectable();
    }
    public override void Excute(MGNunchiMapInfo info) { }
    public override void Exit  (MGNunchiMapInfo info)
    {
        //DOTween.Kill(info.EffectiveMat);
        info.StopSelectable();
    }
}


public class MGNunchiMapEffectState_Touched : CState<MGNunchiMapInfo>
{
    static MGNunchiMapEffectState_Touched s_this = null;

    public static MGNunchiMapEffectState_Touched Instance()
    {
        if (s_this == null) s_this = new MGNunchiMapEffectState_Touched();
        return s_this;
    }
    public override void Enter (MGNunchiMapInfo info) 
    {
        //info.MapObj.DOKill();
        info.SetShaderProp(PLAT_STATE.DEFAULT);
        info.SetShaderProp(PLAT_STATE.TOUCHED);
    }
    public override void Excute(MGNunchiMapInfo info) { }
    public override void Exit  (MGNunchiMapInfo info) { }
}

public class MGNunchiMapEffectState_Selected : CState<MGNunchiMapInfo>
{
    static MGNunchiMapEffectState_Selected s_this = null;

    public static MGNunchiMapEffectState_Selected Instance()
    {
        if (s_this == null) s_this = new MGNunchiMapEffectState_Selected();
        return s_this;
    }
    public override void Enter (MGNunchiMapInfo info) 
    {
        info.SetShaderProp(PLAT_STATE.SELECTED);
    }
    public override void Excute(MGNunchiMapInfo info) { }
    public override void Exit  (MGNunchiMapInfo info) { }
}


/*
 
public class MGNunchiMapEffectState_Normal : CState<MGNunchiMapInfo>
{
    static MGNunchiMapEffectState_Normal s_this = null;

    public static MGNunchiMapEffectState_Normal Instance()
    {
        if (s_this == null) s_this = new MGNunchiMapEffectState_Normal();
        return s_this;
    }
    public override void Enter (MGNunchiMapInfo info) { }
    public override void Excute(MGNunchiMapInfo info) { }
    public override void Exit  (MGNunchiMapInfo info) { }
}
 */
