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
#endregion Global project preprocessor directives.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CMarbleAnimController
{
    private Dictionary<string, AnimationClip> MarbleBaseAnimClipDic = null;
    private Dictionary<string, float> ClipTimeDic = null;

    private Animator AnimatorController;
    private AnimatorOverrideController OverrideController;
    private List<KeyValuePair<AnimationClip, AnimationClip>> overrides;
    private List<string> SwapBaseNames = null;

    public void Initialize(Animator animator)
    {
        MarbleBaseAnimClipDic = new Dictionary<string, AnimationClip>();
        ClipTimeDic = new Dictionary<string, float>();
        SwapBaseNames = new List<string>();

        AnimatorController = animator;
        SetAnimatorController();
        SetAnimClipTime();
    }

    public void SetSwapClipsBaseName(List<string> baseNames)
    {
        foreach (var name in baseNames)
        {
            if (!SwapBaseNames.Contains(name))
            {
                SwapBaseNames.Add(name);
            }
        }

        SetBaseAnimClipDic();
    }

    private void SetAnimatorController()
    { 
        OverrideController = new AnimatorOverrideController(AnimatorController.runtimeAnimatorController);
        AnimatorController.runtimeAnimatorController = OverrideController;

        overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
        OverrideController.GetOverrides(overrides);
    }

    private void SetAnimClipTime()
    {
        AnimationClip[] _clips = AnimatorController.runtimeAnimatorController.animationClips;
        foreach (AnimationClip clip in _clips)
        {
            string clipName = clip.name;
            string key = GetClipKey(clipName);
            
            if (!ClipTimeDic.ContainsKey(key))
            {
                ClipTimeDic.Add(key, clip.length);
                //CDebug.Log($"#### [CMarbleAnimController] SetAnimClipTime - Key: {key}, Length: {clip.length}");
            }
        }
    }

    private string GetClipKey(string clipName)
    {
        // special_move family. check first to avoid overlap with move family
        if (clipName.Contains("special_move"))
        {
            if (clipName.Contains("start")) return "special_move_start";
            if (clipName.Contains("end")) return "special_move_end";
            return "special_move";
        }
        
        // move family
        if (clipName.Contains("move"))
        {
            if (clipName.Contains("start")) return "move_start";
            if (clipName.Contains("end")) return "move_end";
            return "move";
        }
        
        // idle family
        if (clipName.Contains("idle"))
        {
            if (clipName.Contains("default")) return "idle_default";
            if (clipName.Contains("lookaround")) return "idle_lookaround";
            if (clipName.Contains("yawn")) return "idle_yawn";
            return "idle";
        }
        
        // levelup
        if (clipName.Contains("touch_high"))
        {
            return "touch_high";
        }

        // 그 외는 원본 이름 사용
        return clipName;
    }
    
    private void SetBaseAnimClipDic()
    {
        if (AnimatorController.runtimeAnimatorController == null)
            return;

        AnimationClip[] _clips = AnimatorController.runtimeAnimatorController.animationClips;
        foreach (AnimationClip clip in _clips)
        {
            // set MarbleAnimationClipDic
            foreach (var baseName in SwapBaseNames)
            {
                string clipName = clip.name;

                if (clipName.Contains(baseName))
                {
                    if (!MarbleBaseAnimClipDic.ContainsKey(baseName))
                    {
                        MarbleBaseAnimClipDic.Add(baseName, clip);
                    }
                }
            }
        }
    }
    
    public void SwapAnimClip(AnimationClip newClip, string name)
    {
        if (newClip == null || MarbleBaseAnimClipDic[name] == newClip) return;

        SwapClip(name, newClip);

        MarbleBaseAnimClipDic[name] = newClip;
    }


    private void SwapClip(string name, AnimationClip newClip)
    {
        int index = GetOverrideIndexByClipName(name);
        if (index != -1)
        {
            ApplyOverrideAtIndex(newClip, index);
        }
    }
    private int GetOverrideIndexByClipName(string clipName)
    {
        return overrides.FindIndex(kvp => kvp.Key.name.Contains(clipName));
    }    

    private void ApplyOverrideAtIndex(AnimationClip newClip, int index)
    {
        overrides[index] = new KeyValuePair<AnimationClip, AnimationClip>(overrides[index].Key, newClip);
        OverrideController.ApplyOverrides(overrides);
    }


    public float GetClipTime(string clipName)
    {
        if (ClipTimeDic.ContainsKey(clipName))
        {
            return ClipTimeDic[clipName];
        }

        return 1f;
    }

    public void Release()
    {
        if (MarbleBaseAnimClipDic != null)
        {
            MarbleBaseAnimClipDic.Clear();
            MarbleBaseAnimClipDic = null;
        }

        if (ClipTimeDic != null)
        {
            ClipTimeDic.Clear();
            ClipTimeDic = null;
        }

        if (OverrideController != null)
        {
            OverrideController = null;
        }

        if (AnimatorController != null)
        {
            AnimatorController = null;
        }

        if (overrides != null)
        {
            overrides.Clear();
            overrides = null;
        }

        if (SwapBaseNames != null)
        {
            SwapBaseNames.Clear();
            SwapBaseNames = null;
        }
    }
}
