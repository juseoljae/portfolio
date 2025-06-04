using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CCharacterAnimationInfo 
{
    private Dictionary<string, float> AnimationLengthDic = new Dictionary<string, float>();
         

    public void Initialize(Animator animator)
    {
        SetAnimLengthDic(animator);
    }

    private void SetAnimLengthDic(Animator animator)
    {
        if (animator.runtimeAnimatorController == null)
            return;

        AnimationClip[] _clips = animator.runtimeAnimatorController.animationClips;
        foreach(AnimationClip clip in _clips)
        {
            string clipName = clip.name;
            if(AnimationLengthDic.ContainsKey(clipName) == false)
            {
                AnimationLengthDic.Add(clipName, clip.length);
            }
        }
    }

    public float GetAnimationLength(string clipName)
    {
        if (AnimationLengthDic.ContainsKey(clipName))
        {
            return AnimationLengthDic[clipName];
        }

        return 0f;
    }
}
