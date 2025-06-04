using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class AnimationData
{
    public AnimationClip clip;
    public string clipName;
    public float speed = 1;
    public float transitionDuration = -1;
    public WrapMode wrapMode;
    [HideInInspector]
    public int timesPlayed = 0;
    [HideInInspector]
    public int stateHash;
}

public class KSAnimation : MonoBehaviour {

    public AnimationData defaultAnimation;
    public AnimationData[] animations;
    public Dictionary<string, AnimationData> animDataDic;
    public Dictionary<AnimationClip, AnimationData> animClipDic;
    private List<AnimationClip> m_MyAnimClip;
    //public AnimationData[] Clips = new AnimationData[0];
    public bool debugMode = false;
    public bool alwaysPlay = false;
    public float defaultTransitionDuration = 0.15f;
    public WrapMode defaultWrapMode = WrapMode.Once;
    //public bool bDontPlay = false;

    public bool isBlending = false;
    public string sPlayClipName;

    private Animator animator;

    private string[] m_StateName;
    private const int STATE_NAME_NUM = 20;

    private RuntimeAnimatorController controller;
    AnimatorOverrideController overrideController;

    public Animation m_Animation;
    private AnimationClip m_CurAnimClip;
    private AnimationState m_CurAnimState;
    private bool m_bEndEvent;
    
    private AnimationData currentAnimationData;
    private bool invertedSpeed;

    ////public delegate void AnimEvent(AnimationData animationData);
    public delegate void AnimEvent(AnimationClip animationData);
    //public event AnimEvent OnAnimationBegin;
    public event AnimEvent OnAnimationEnd;
    //public event AnimEvent OnAnimationLoop;

    public float m_fPauseTimer = 0.0f;
    public float normalizedTime = 0.0f;
    // Use this for initialization
    void Awake()
    {
        defaultAnimation = new AnimationData();
        animations = new AnimationData[0];
        animator = gameObject.GetComponent<Animator>();
        animDataDic = new Dictionary<string, AnimationData>();
        animClipDic = new Dictionary<AnimationClip, AnimationData>();
        m_Animation = gameObject.GetComponent<Animation>();
        m_MyAnimClip = new List<AnimationClip>();


        //SetDontPlayAnymore(false);
        m_StateName = new string[STATE_NAME_NUM];
        for(int i=0 ; i<m_StateName.Length ; i++)
        {
            m_StateName[i] = "SKILL_" + i;
        }



        m_bEndEvent = false;
    }

    public void DestroyKSAnimation()
    {
        if(m_MyAnimClip != null)
        {
            for (int i = 0; i < m_MyAnimClip.Count; ++i)
            {
                m_Animation.RemoveClip(m_MyAnimClip[i]);
                m_MyAnimClip[i] = null;
            }
        }
        m_MyAnimClip = null;
        if (defaultAnimation != null) defaultAnimation = null;
        if (animations != null) animations = null;
        if (animator != null) animator = null;
        if (animDataDic != null) animDataDic = null;
        if (animClipDic != null) animClipDic = null;
        if (m_StateName != null) m_StateName = null;
        if (overrideController != null) overrideController = null;
        if (controller != null) controller = null;
        if (currentAnimationData != null) currentAnimationData = null;
        if (m_Animation != null) m_Animation = null;
    }
    
    public void InitCharacterAnimations(string path, Dictionary<string, string> dic)
    {
        AnimationClip clip = null;
 
        foreach (KeyValuePair<string, string> pair in dic)
        {
            string fname = path + pair.Value;
            clip = KSPlugins.KSResource.AssetBundleInstantiate<AnimationClip>(fname, true);
            if (clip != null)
            {
                if (clip.wrapMode != WrapMode.Loop)
                    clip.wrapMode = WrapMode.ClampForever;

                m_Animation.AddClip(clip, pair.Value);
                m_MyAnimClip.Add(clip);
            }
        }
    }

    public void InitCharacterSkillAnimations(string path, Dictionary<string, string> dic, Dictionary<string, WrapMode> wDic)
    {
        AnimationClip clip = null;

        int i = 0;
        foreach (KeyValuePair<string, string> pair in dic)
        {
            bool bExist = false;
            string fname = path + pair.Value;
            //Debug.Log("init Skill Animation wDic[ "+pair.Key+" ] = " +wDic[pair.Key]);
            foreach (KeyValuePair<string, AnimationData> pair2 in animDataDic)
            {
                if( pair2.Key == pair.Key )
                {
                    bExist = true;
                    break;
                }
            }
            if ( bExist == false )
            {
                clip = KSPlugins.KSResource.AssetBundleInstantiate<AnimationClip>(fname, true);
                if (clip != null)
                {
                    if (clip.wrapMode != WrapMode.Loop)
                        clip.wrapMode = WrapMode.ClampForever;

                    m_Animation.AddClip(clip, pair.Key);
                    m_MyAnimClip.Add(clip);
                }
            }
            i++;
        }
    }

    public void UpdateAnimation()
    {
        if (m_fPauseTimer >= 0.0f)
        {
            m_fPauseTimer -= Time.deltaTime;
            if (m_fPauseTimer <= 0.0f)
                Resume();
            else
                return;
        }
        //Debug.Log("0000 // m_CurAnimClip = " + m_CurAnimClip);
        if(m_CurAnimClip!=null)
        {
            
            if (m_CurAnimClip.wrapMode != WrapMode.Loop)
            {
                if (m_CurAnimState.normalizedTime >= 1.0f)
                {
                    if (!bEventEnd)
                    {
                        if (OnAnimationEnd != null)
                        {
                            bEventEnd = true;
                            OnAnimationEnd(m_CurAnimClip);
                        }
                    }
                }
            }
        }
    }

    public bool bEventEnd
    {
        get { return m_bEndEvent; }
        set { m_bEndEvent = value; }
    }

    public void InitCurAnimClip()
    {
        m_CurAnimClip = null;
        m_CurAnimState = null;
    }

    private void AddSkillClip(AnimationClip clip, string name, string stateName, WrapMode wrapMode)
    {
        AnimationData animData = new AnimationData();
        animData.clip = clip;
        animData.clip.wrapMode = clip.wrapMode;
        animData.wrapMode = clip.wrapMode;
        animData.clipName = name;
        animData.speed = 1;
        //Debug.Log("Key = " + name + "@@" + stateName + "#### clip = " + clip + "/" + gameObject.name);
        animDataDic.Add(name, animData);
        animData.stateHash = Animator.StringToHash(stateName);
        animClipDic.Add(clip, animData);

        List<AnimationData> animationDataList = new List<AnimationData>(animations);
        animationDataList.Add(animData);
        animations = animationDataList.ToArray();
    }

    public AnimationData GetAnimationData(string clipName)
    {
        AnimationData data = animDataDic[clipName];
        if (data != null)
        {
            return data;
        }
        return null;
    }

    public AnimationClip GetAnimationClip(string name)
    {
        return m_Animation.GetClip(name);
    }
    public AnimationState GetAnimationState(string name)
    {
        if( m_Animation.GetClip(name) != null )
            return m_Animation[name];
        return null;
    }

    public void PlayNoBlend(string clipName,float fSpeed)
    {
        bEventEnd = false;
        m_CurAnimClip = m_Animation.GetClip(clipName);

        m_CurAnimState = m_Animation[m_CurAnimClip.name];
        m_CurAnimState.speed = fSpeed;
        m_CurAnimState.wrapMode = m_Animation[m_CurAnimClip.name].wrapMode;
        m_Animation.Play(clipName, PlayMode.StopAll);
//        m_Animation.CrossFadeQueued("Idle",0.15f,QueueMode.CompleteOthers);

        ////Debug.Log("######## Play(string clipName) Start ##########");
        ////Debug.Log("Play(string clipName) name = " + clipName);
        //sPlayClipName = clipName;
        //bBlend = false;
        ////Debug.Log("Play(string clipName) sPlayClipName = " + sPlayClipName + "/bBlend = " + bBlend + "/bDontPlay = " + bDontPlay);
        //_playAnimation(GetAnimationData(clipName).clip, 0, 0);
        ////Debug.Log("######## Play(string clipName) Fisnish ##########");
    }



    public void PlayAnim(string clipName, float time , float Speed = 1.0f)
    {
        bEventEnd = false;
        if (m_CurAnimClip != null)
        {
            m_Animation[m_CurAnimClip.name].speed = 0.0f;
        }
        m_CurAnimClip = m_Animation.GetClip(clipName);
        if (m_CurAnimClip == null)
            return;

        m_CurAnimState = m_Animation[m_CurAnimClip.name];
        m_CurAnimState.speed = Speed;
        m_CurAnimState.wrapMode = m_Animation[m_CurAnimClip.name].wrapMode;
        if (m_Animation.isPlaying == false)
        {
            m_Animation.Play(clipName);
        }
        else
            m_Animation.CrossFade(clipName, time);
    }




    public bool IsPlaying(string clipName)
    {
        return m_Animation.IsPlaying(clipName);
    }


    public string GetCurrentClipName()
    {
        return m_CurAnimClip.name;
        //return currentAnimationData.clipName;
    }

    public void Pause( float p_fPauseTime )
    {

        //Debug.Log("KSAnimation.Pause() name = " + m_CurAnimClip.name + "/ time = " + p_fPauseTime);
        m_fPauseTimer = p_fPauseTime;
        if (p_fPauseTime != 0.0f)
        {
            if (m_CurAnimState != null && m_CurAnimState != null)
            {
                m_CurAnimState.speed = 0;// 0.1f;
            }
        }
            //Pause();
    }

    public AnimationState GetAnimation(string name)
    {
        return m_Animation[name];
    }

    public void ReSample()
    {
        
        foreach (AnimationState animState in m_Animation)
        {
            AnimationClip clip = m_Animation.GetClip(animState.name);
            if (clip != null)
            {
                animState.time = 0.0f;
                animState.wrapMode = clip.wrapMode;
                animState.enabled = true;
                animState.weight = 1;
                m_Animation.Sample();
                animState.enabled = false;
            }
        }
    }

    public void Resume()
    {
        if (m_Animation != null)
        {
            if (m_CurAnimState != null)
                m_CurAnimState.speed = 1;
        }
    }

    public void SetSpeed(string clipName, float speed)
    {
        m_Animation[clipName].speed = speed;
    }

    public void Rewind()
    {
        m_Animation.Rewind();
    }

    public void SetAnimationCullType( AnimationCullingType p_eType )
    {
        m_Animation.cullingType = p_eType;
    }

    public bool bBlend
    {
        get { return isBlending; }
        set { isBlending = value; }
    }
}


