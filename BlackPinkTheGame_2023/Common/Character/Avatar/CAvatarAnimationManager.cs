
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
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UniRx;
using System.Collections;

public class CAvatarAnimationManager : SingletonMono<CAvatarAnimationManager>
{
    private Dictionary<string, AnimationClip> ClipDic = new Dictionary<string, AnimationClip>();

    private string PrevPlayClipName = string.Empty;
    private SingleAssignmentDisposable AnimPlayDisposer = null;

    private int RandomIdleCount = 0;

    private IEnumerator RandomCoroutine   = null;
    private IEnumerator loadClipCoroutine = null;

    private const string IdlePath    = "Avatar/Common/Animations/shopAnimations/Open/Animations/animation_m01_idle.anim";
    private const string IDLE_RANDOM = "random_idle_";

    private void InitAnimPlayDisposer()
    {
        if (AnimPlayDisposer != null)
        {
            AnimPlayDisposer.Dispose();
            AnimPlayDisposer = null;
        }
    }

    public void SetIdeleClip(Animation anim, GameObject obj)
    {
        AnimationClip clip = LoadClip( IdlePath, "idle", obj );
        if(clip == null)
        {
            CDebug.Log( $"SetIdeleClip] wrong path : {IdlePath}, obj:{obj}");                
            return;
        }

        anim.AddClip( clip, "idle" );
        Play( anim, "idle" );
    }

    public void SetIdleClipList(Animation anim, GameObject obj, AVATAR_TYPE aType)
    {
        RandomIdleCount = 0;

        List<ConfigureData> list = Configure.GetConfigureDataArray(CONFIGURE_TYPE.MEMBER_ANIMATION_IDLE);

        for (int i = 0; i < list.Count; i++)
        {
            ConfigureData data = list[i];

            if (data.Value1.ToInt32() == 2)
            {
                MotionData mData = CAvatarInfoDataManager.GetMotionDataByID(data.Value2.ToLong());

                if (mData != null)
                {
                    string clipName = IDLE_RANDOM + RandomIdleCount.ToString();

                    AnimationClip clip = LoadClip(mData.ResPath, clipName, obj);
                    if (clip != null)
                    {
                        anim.AddClip(clip, clipName);

                        RandomIdleCount++;
                    }
                }
            }
        }

        StartRandomPlay(anim, obj, 1f);

    }

    public AnimationClip LoadClip(string path, string clipName, GameObject obj)
    {
        CResourceData resData = CResourceManager.Instance.GetResourceData(path);
        if(resData != null)
        {
            AnimationClip clip = resData.Load<AnimationClip>( obj );
            
            if(clip.legacy == false)
            {
                CDebug.LogError($"Not Play Animation : {path} ");
                //clip.legacy = true;
                return null;
            }

            clip.name = clipName;

            if (ClipDic.ContainsKey( clipName ) == false)
            {
                ClipDic.Add( clip.name, clip );
            }

            return clip;
        }

        return null;
    }

    public void LoadAndPlayAnimation(Animation anim, string path, string clipName, string clipNameAfter, GameObject obj, Action endAction = null)
    {
        AnimationClip clip = LoadClip( path, clipName, obj );
        anim.AddClip( clip, clipName );
        CrossFade( anim, clipName, clipNameAfter, obj, endAction);
    }

    public void Play(Animation anim, string clipName)
    {
        if(anim != null)
        {
            anim.Play(clipName);
        }
    }

    public void CrossFade(Animation anim, string clipName, string clipNameAfter, GameObject obj, Action endAction = null)
    {
        if (AnimPlayDisposer != null)
        {
            AnimPlayDisposer.Dispose();
            AnimPlayDisposer = null;
        }

        if (anim != null)
        {
            if (anim.isPlaying)
            {
                if(PrevPlayClipName.Equals( clipNameAfter ) == false)
                {
                    anim.Stop();
                    if (PrevPlayClipName.Equals( string.Empty ) == false)
                    {
                        ReleaseClip( anim, PrevPlayClipName );

                        InitAnimPlayDisposer();
                    }
                }
            }
            PrevPlayClipName = clipName;
            anim.CrossFade( clipName );
            float clipLen = 2;
            if (ClipDic.ContainsKey( clipName ))
            {
                clipLen = ClipDic[clipName].length;
            }

            AnimPlayDisposer = new SingleAssignmentDisposable();
            AnimPlayDisposer.Disposable = Observable.Timer( TimeSpan.FromSeconds( clipLen ) )
            .Subscribe( _ =>
            {
                ReleaseClip( anim, clipName );
                Play( anim, clipNameAfter );
                endAction?.Invoke();
                InitAnimPlayDisposer();

            } )
            .AddTo( obj );
        }
    }

    public void StartRandomPlay(Animation anim, GameObject obj, float fDelay)
    {
        ReleaseRandomIdle();

        if (anim != null)
        {
            RandomCoroutine = PlayRandomIdle(anim, obj, fDelay);

            if (RandomCoroutine != null)
            {
                StartCoroutine(RandomCoroutine);
            }
        }
    }

    private IEnumerator PlayRandomIdle(Animation anim, GameObject obj, float fDelay = 0)
    { 
        int randIdx = UnityEngine.Random.Range(0, RandomIdleCount);

        if (anim != null)
        {
            //기본 아이들
            anim.Play("idle");
            PrevPlayClipName = "idle";
        }

        yield return new WaitForSeconds(fDelay);

        if (anim != null)
        {
            string strKey = IDLE_RANDOM + randIdx.ToString();
            PrevPlayClipName = strKey;
            anim.Play(strKey);

            if (ClipDic.ContainsKey(strKey))
            {
                yield return new WaitForSeconds(ClipDic[strKey].length);
            }
            else
            {
                CDebug.LogError($"Not Found RandomClip key:{strKey}");
            }
        }

        int randSec = UnityEngine.Random.Range(4, 8);
        StartRandomPlay(anim, obj, randSec);
    }

    private void RemoveClipDic(string clipName)
    {
        if (ClipDic.ContainsKey( clipName ))
        {
            bool canRemove = CRuntimeAnimControllerSwitchManager.Instance.CanRemoveAnimationClipInAll( clipName );

            if (canRemove)
            {
                //release, if it can be
                Addressables.Release( ClipDic[clipName] );
            }
        }
    }

    private void RemoveAnimClip(Animation anim, string clipName)
    {
        foreach (AnimationState state in anim)
        {
            if (state.name.Equals( clipName ))
            {
                anim.RemoveClip( state.clip );
                break;
            }
        }
        PrevPlayClipName = string.Empty;
    }


    public void ReleaseClip(Animation anim, string clipName)
    {
        RemoveAnimClip( anim, clipName );
        //RemoveClipDic( clipName );
        ClipDic.Remove( clipName );
    }

    public void ReleaseRandomIdle()
    {
        if (RandomCoroutine != null)
        {
            StopCoroutine(RandomCoroutine);
        }
    }

    public void Release(Animation anim)
    {
        RandomIdleCount = 0;
        ReleaseRandomIdle();
        RandomCoroutine = null;

        if (ClipDic.Count > 0)
        {
            foreach (KeyValuePair< string, AnimationClip > clip in ClipDic)
            {
                RemoveAnimClip( anim, clip.Key );
                RemoveClipDic( clip.Key );
            }
            ClipDic.Clear();
        }
        PrevPlayClipName = string.Empty;
        InitAnimPlayDisposer();

    }

    #region 보드판(스페셜 모션 클립)

    public void StartClipPlay(Animation anim, string clipName, float _delay)
    {
        if (loadClipCoroutine != null)
        {
            StopCoroutine(loadClipCoroutine);
            loadClipCoroutine = null;
        }

        if (anim != null && ClipDic != null)
        {
            if (ClipDic.ContainsKey( clipName ))
            {
                if (ClipDic[clipName] != null)
                {
                    loadClipCoroutine = PlayLoopClip( anim, clipName, _delay );
                    if (loadClipCoroutine != null)
                    {
                        StartCoroutine(loadClipCoroutine);
                    }
                }
            }
        }
    }

    private IEnumerator PlayLoopClip(Animation anim, string clipName, float fDelay)
    {
        yield return new WaitForSeconds(fDelay);

        //애니메이션 딜레이 후 anim이 사라졌는지 확인
        if (anim != null)
        {
            if (anim.isPlaying)
                anim.Stop();

            // 재생
            Play(anim, clipName);


            if (anim.isPlaying)
            {
                if (ClipDic.ContainsKey(clipName))
                {
                    yield return new WaitForSeconds(ClipDic[clipName].length);
                }
            }
        }

        //딜레이 이후 anim이 사라져 있을 수 있기 때문에 한번더 체크
        if (anim != null)
        {

            StartClipPlay(anim, clipName, 0.5f);
        }

    }

    #endregion
}
