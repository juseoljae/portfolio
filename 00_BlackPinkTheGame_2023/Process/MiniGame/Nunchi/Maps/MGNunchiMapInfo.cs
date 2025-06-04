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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using DG.Tweening;

public class MGNunchiMapInfo
{
    private MGNunchiWorldManager WorldMgr;
    public MGNunchiMapManager MapMgr;
    
    public CStateMachine<MGNunchiMapInfo> MapEffSM;

    public int Index;
    public int Prop_Layer;
    public Transform MapParent;
    public Transform MapObj;

    private GameObject DustFXObj;
    private ParticleSystem DustFXParticle;
    private GameObject HitFXObj;
    private ParticleSystem HitFXParticle;

    private int CurrnetCoinCount;

    private Animation PlatformAnim;

    //private SkinnedMeshRenderer Renderer;
    private Material EffectiveMat;
    private Tween SelectableTween;
    //private float CurEffectiveMatPropValue;

    //public MGNunchiMapEffect MapEffect { get; private set; }

    public void Initialize(MGNunchiWorldManager worldMgr, int layer, Transform mapObj)
    {
        WorldMgr = worldMgr;
        MapMgr = WorldMgr.MapMgr;

        MapEffSM = new CStateMachine<MGNunchiMapInfo>(this);

        Prop_Layer = layer;
        MapObj = mapObj;
        MapParent = MapObj.parent;

        //MapEffect = mapObj.GetComponent<MGNunchiMapEffect>();
        //MapEffect.Init(this);

        SetFXObj();

        SetMaterial();
    }

    public void InitPlatformAnim(Animation anim)
    {
        PlatformAnim = anim;
    }

    public void SetGameCoinCurrentCount(int count)
    {
        CurrnetCoinCount = count;
    }

    public int GetGameCoinCurrentCount()
    {
        return CurrnetCoinCount;
    }

    public Vector3 GetMapPosition()
    {
        return MapObj.localPosition;
        //return MapParent.localPosition;
    }


    public void PlayJumpDownAnim()
    {
        if (PlatformAnim == null)
        {
            //Debug.LogError("PlayJumpDownAnim() PlatformAnim is null");
            return;
        }

        PlatformAnim.Play(MGNunchiDefines.ANIM_PLAT_DOWN);
    }


    IObservable<Unit> CheckAnimPlayFinish(string name)
    {
        return Observable.FromCoroutine<Unit>(x => PlayNextAnimAfterCheckEndAnim(x, name));
    }


    private IEnumerator PlayNextAnimAfterCheckEndAnim(IObserver<Unit> observer, string name)
    {
        while (PlatformAnim.IsPlaying(name))
        {
            yield return null;
        }


        observer.OnNext(Unit.Default);
        observer.OnCompleted();
    }

    public bool bPlayAnimByName(string name)
    {
        if (PlatformAnim == null)
        {
            //Debug.LogError("bPlayAnimByName() PlatformAnim is null");
            return false;
        }

        if(PlatformAnim.IsPlaying(name))
        {
            return true;
        }

        return false;
    }

    public void SetMapEffectNormal()
    {
        MapEffSM.ChangeState(MGNunchiMapEffectState_Normal.Instance());
    }

    public void SetMapEffectTouched()
    {
        MapEffSM.ChangeState(MGNunchiMapEffectState_Touched.Instance());
    }

    public void SetMapEffectSelectable()
    {
        MapEffSM.ChangeState(MGNunchiMapEffectState_Selectable.Instance());
    }

    public void SetMapEffectFadeOut()
    {
        //SetMapEffectNormal();

        float alpha = EffectiveMat.color.a;
        DOTween.To(
            () => alpha,
            alphaValue =>
            {
                SetShaderProp_ChangeAlphaValue(alphaValue);
                //CDebug.Log($" #### MGNunchiMapEffectState_Selectable. alphaValue = {alphaValue}");
            },
            0, MGNunchiDefines.JUMP_DUR_TIME
            ).OnComplete(() =>
            {
                SetMapEffectNormal();
            });
    }

    //public void StopAnim()
    //{
    //    if (PlatformAnim == null)
    //    {
    //        //Debug.LogError("StopAnim() PlatformAnim is null");
    //        return;
    //    }

    //    CDebug.Log($"Stop Animation :{MapObj.name} - {EffectiveMat.color.a}");

    //    float curAlpha = EffectiveMat.color.a;//.GetColor(MGNunchiDefines.PLAT_EFF_SHADERPROP_NAME);
    //    //DOTween.To(() => curAlpha,
    //    //    changevalue =>
    //    //    {
    //    //        SetEffectiveMatProp(changevalue);
    //    //       // SetJumpHeight(changevalue);
    //    //    },
    //    //    0, 1f
    //    //    ).SetEase(Ease.InQuad).OnComplete(() =>
    //    //    {
    //    //        PlatformAnim.Stop();
    //    //        //PlatformAnim.Play(MGNunchiDefines.ANIM_PLAT_IDLE);
    //    //    });

    //}

    
    //public void ResetAnimation(Action act)
    //{

    //    if (PlatformAnim == null)
    //    {
    //        //Debug.LogError("StopAnim() PlatformAnim is null");
    //        return;
    //    }

    //    PlatformAnim.Stop();
    //    //PlatformAnim.Play(MGNunchiDefines.ANIM_PLAT_IDLE);
    //    act?.Invoke();

    //    //Color color1 = EffectiveMat.GetColor("_MainTex");
    //    //Color color2 = EffectiveMat.GetColor("_Color");


    //    //CDebug.Log($"ResetAnimation :{MapObj.name} -MainTex: {color1.a}, _Color {color2.a}" );
    //    /*
    //    float curAlpha = 1;
    //    DOTween.To(() => 1,
    //        changevalue =>
    //        {
    //            //Color color = new Color(color1.r, color1.g, color1.b, changevalue);
    //            //EffectiveMat.SetColor("_MainTex", color);
    //        },
    //        0, 1f)
    //        .SetEase(Ease.InQuad).OnComplete(() =>
    //        {
    //            PlatformAnim.Play(MGNunchiDefines.ANIM_PLAT_IDLE);
    //            act?.Invoke();
    //        });
    //    */
    //}

    //private void SetEffectiveMatProp(float value)
    //{
       
    //    Color color = new Color(EffectiveMat.color.r, EffectiveMat.color.g, EffectiveMat.color.b, value); //EffectiveMat.GetColor(MGNunchiDefines.PLAT_EFF_SHADERPROP_NAME);

    //    if (EffectiveMat.color.a != 0)
    //    {
    //        CDebug.Log($"    ############# {MapObj.name} / value = {value}  / color = " + color);
    //    }
    //    EffectiveMat.color = color;//.SetColor(MGNunchiDefines.PLAT_EFF_SHADERPROP_NAME, color);

        

    //}

    private void SetFXObj()
    {
        WorldMgr.SetFXObj(ref DustFXObj, ref DustFXParticle, MapMgr.OriginDustFXObj, "dust", MapObj.transform, Vector3.zero, Quaternion.Euler(Vector3.zero));
        //DustFXObj = GameObject.Instantiate(MapMgr.OriginDustFXObj);
        //GameObject _particleObj = DustFXObj.transform.Find("dust").gameObject;
        //DustFXParticle = _particleObj.GetComponent<ParticleSystem>();

        //DustFXObj.transform.SetParent(MapObj.transform);
        //DustFXObj.transform.localPosition = Vector3.zero;
        //DustFXObj.transform.localRotation = Quaternion.Euler(Vector3.zero);
        //DustFXObj.transform.localScale = Vector3.one;
        WorldMgr.SetFXObj(ref HitFXObj, ref HitFXParticle, MapMgr.OriginHitFXObj, "hit01", MapObj.transform, Vector3.zero, Quaternion.Euler(Vector3.zero));

        SetActiveDustFxObj(false);
        //SetActiveHitFxObj(false);
        HitFXObj.SetActive(false);
    }

    private void SetMaterial()
    {
        Transform rendererObj = MapObj.transform.Find(MGNunchiConstants.PLAT_PUDDING_OBJ_PATH);
        SkinnedMeshRenderer Renderer = rendererObj.GetComponent<SkinnedMeshRenderer>();
        
        Material[] mats = new Material[Renderer.sharedMaterials.Length + 1];            
        mats[0] = Renderer.sharedMaterials[0];

        CResourceData resData = CResourceManager.Instance.GetResourceData(MGNunchiConstants.PLAT_ORIGIN_MATERIAL_PATH);
        mats[1] = GameObject.Instantiate(resData.LoadMaterial(MapObj.gameObject)) as Material;

        EffectiveMat = mats[1];
        Renderer.sharedMaterials = mats;

        MapEffSM.ChangeState(MGNunchiMapEffectState_Normal.Instance());
        //MapEffSM.ChangeState(MGNunchiMapEffectState_Selectable.Instance());
    }

    public void SetShaderProp(PLAT_STATE state)
    {
        MGNunchiShaderProp _prop  = MapMgr.GetShaderPropByState(state);
        
        Color _albedoColor = new Color(_prop.Albedo.x, _prop.Albedo.y, _prop.Albedo.z, _prop.Albedo.w);
        EffectiveMat.color = _albedoColor;
        Color _emissionColor = new Color(_prop.Emission.x, _prop.Emission.y, _prop.Emission.z, _prop.Emission.w);
        EffectiveMat.SetColor(MGNunchiDefines.PLAT_SHADERPROP_EMISSION_NAME, _emissionColor);
    }

    public void SetShaderProp_Selectable()
    {
        //if (MapEffSM.GetPreviousState() == MGNunchiMapEffectState_Selectable.Instance()) return;
        if (SelectableTween != null && SelectableTween.IsPlaying()) return;

        SetShaderProp(PLAT_STATE.SELECTABLE);
        
        float alpha = 0.0f;
        SelectableTween = DOTween.To(
            () => alpha,
            alphaValue =>
            {
                SetShaderProp_ChangeAlphaValue(alphaValue);
                //CDebug.Log($" #### MGNunchiMapEffectState_Selectable. alphaValue = {alphaValue}");
            },
            MGNunchiDefines.PLAT_SHADERPROP_SELECTABLE_ALPHAVALUE_MAX, MGNunchiDefines.JUMP_DUR_TIME
            ).SetLoops(-1, LoopType.Yoyo);
    }

    public void StopSelectable()
    {
        SelectableTween.SetLoops(0);
        SelectableTween.Complete();
        SelectableTween.Kill(); //?
    }

    private void SetShaderProp_ChangeAlphaValue(float value)
    {
        Color _albedoColor = new Color(EffectiveMat.color.r, EffectiveMat.color.g, EffectiveMat.color.b, value);
        EffectiveMat.color = _albedoColor;
    }

    public void SetActiveDustFxObj(bool bActive)
    {
        DustFXObj.SetActive(bActive);

        if(bActive)
        {
            WorldMgr.SetFXParticleFinish(DustFXParticle.main.startLifetime.constant, DustFXObj);
        }
    }

    public void SetActiveHitFxObj(bool bActive)
    {
        //CDebug.Log($"SetActiveHitFxObj() bActive = {bActive}");

        if (HitFXObj.activeSelf == false)
        {
            HitFXObj.SetActive(bActive);

            if (bActive)
            {
                WorldMgr.SetFXParticleFinish(HitFXParticle.main.startLifetime.constant, HitFXObj);
            }
        }
    }

    //public void SetActiveSelectEffect(bool bActive)
    //{
    //    MapEffect.SetEffectActive(bActive);
    //}


    public CState<MGNunchiMapInfo> GetCurrentState()
    {
        return MapEffSM.GetCurrentState();
    }
}
