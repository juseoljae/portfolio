
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


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class CRuntimeAnimControllerSwitchManager : Singleton<CRuntimeAnimControllerSwitchManager>
{
    //Key(string) : character_id / net_uid(for tcp) / npc
    //value(List) : Common and other
    private Dictionary<string, List<AnimControllerInfo>> charAnimListDic = new Dictionary<string, List<AnimControllerInfo>>();

    //Key(string) : character_id
    private Dictionary<string, List<string>> UsingClipNamesDic = new Dictionary<string, List<string>>();

    /// <summary>
    /// SetRuntimeAnimatorController doesn't use timeline scene.
    /// excepted for the 'timeline(mainlobby)' or 'base contents', put animation clip names to 'UsingClipNamesDic' using SetUsingClipNamesByContents().
    /// </summary>
    public RuntimeAnimatorController SetRuntimeAnimatorController(string charKey, long groupID, ANIMCONTROLLER_USE_TYPE useType, GameObject charObj )
    {
        RuntimeAnimatorController _rac = null;
        CAnimationControllerInfo _info = CCharAnimControllerDataManager.Instance.GetAnimControllerInfoByType( groupID, useType );
        if (_info == null)
        {
            CDebug.LogError( $" SetRuntimeAnimatorController, Group ID:{groupID}, useType:{useType} doesn't have no data" );
            return null;
        }

        ANIMCONTROLLER_TYPE controllerType = _info.ControllerType;

        _rac = GetRuntimeAnimController( charKey, groupID, controllerType );

        if (_rac == null)
        {
            _rac = LoadRuntimeAnimController( useType, charObj, _info.ResPath );

            if(_rac != null )
            {
                SetCharAnimListDic( _rac, charKey, groupID, controllerType, useType );
                //set clips in base contents
                if(useType == ANIMCONTROLLER_USE_TYPE.MANAGEMENT || useType == ANIMCONTROLLER_USE_TYPE.BPWORLD)
                {
                    SetUsingClipNamesByContents( charKey, _rac.animationClips );
                }
            }
        }


        return _rac;
    }

    private RuntimeAnimatorController LoadRuntimeAnimController( ANIMCONTROLLER_USE_TYPE useType, GameObject charObj, string resPath )
    {
        RuntimeAnimatorController _rac = null;

        if (resPath.Equals( string.Empty ))
        {
            CDebug.LogError( $" LoadRuntimeAnimController, useType:{useType} doesn't have no controller" );
            return null;
        }

        CResourceData resData = CResourceManager.Instance.GetResourceData( resPath );
        _rac = resData.Load<RuntimeAnimatorController>( charObj );
        if (_rac == null)
        {
            CDebug.Log( $" [avatar controller] LoadRuntimeAnimController _rac null. resPath:{resPath}, type:{useType}" );
        }

        return _rac;
    }

    private void SetCharAnimListDic(RuntimeAnimatorController controller, string charKey, long grpID, ANIMCONTROLLER_TYPE controllerType, ANIMCONTROLLER_USE_TYPE useType)
    {
        if (charAnimListDic.ContainsKey( charKey ) == false)
        {
            charAnimListDic.Add( charKey, new List<AnimControllerInfo>() );
            charAnimListDic[charKey].Add( new AnimControllerInfo( controller, useType, controllerType, grpID ) );
        }
        else
        {
            charAnimListDic[charKey].Add( new AnimControllerInfo( controller, useType, controllerType, grpID ) );
        }
    }

    public void SetUsingClipNamesByContents(string charKey, AnimationClip[] clip)
    {
        if (UsingClipNamesDic.ContainsKey( charKey ) == false )
        {
            UsingClipNamesDic.Add( charKey, new List<string>() );
            for(int i=0; i< clip.Length; i++)
            {
                UsingClipNamesDic[charKey].Add( clip[i].name );
            }
        }
    }

    public bool CanRemoveAnimationClip(string charKey, string clipName)
    {
        if(UsingClipNamesDic.ContainsKey( charKey ))
        {
            for(int i=0; i< UsingClipNamesDic[charKey].Count; ++i)
            {
                string name = UsingClipNamesDic[charKey][i];
                if (name.Equals(clipName))
                {
                    return false;
                }
            }
        }

        return true;
    }

    public bool CanRemoveAnimationClipInAll(string clipName)
    {
        foreach(string charKey in UsingClipNamesDic.Keys )
        {
            if (CanRemoveAnimationClip( charKey, clipName ) == false)
            {
                return false;
            }
        }

        return true;
    }

    private bool IsThereSameControllerType(string charKey, ANIMCONTROLLER_TYPE controllerType)
    {
        for (int i = 0; i < charAnimListDic[charKey].Count; ++i)
        {
            if (charAnimListDic[charKey][i].ControllerType != controllerType)
            {
                return true;
            }
        }

        return false;
    }

    public AnimControllerInfo GetAnimControllerInfo(string charKey, long groupID, ANIMCONTROLLER_TYPE controllerType)
    {
        if (charAnimListDic.ContainsKey( charKey ))
        {
            for(int i=0; i< charAnimListDic[charKey].Count; ++i)
            {
                if (charAnimListDic[charKey][i].GroupID == groupID)
                {
                    if (charAnimListDic[charKey][i].ControllerType == controllerType)
                    {
                        return charAnimListDic[charKey][i];
                    }
                }
            }
        }

        return null;
    }

    public RuntimeAnimatorController GetRuntimeAnimController(string charKey, long groupID, ANIMCONTROLLER_TYPE controllerType)
    {
        AnimControllerInfo _info = GetAnimControllerInfo(charKey, groupID, controllerType);

        if(_info != null)
        {
            CDebug.Log( $" [avatar controller] GetRuntimeAnimController load complete. key:{charKey}, groupid:{groupID}, type:{controllerType}" );
            return _info.RunAnimController;
        }

        CDebug.Log( $" [avatar controller] GetRuntimeAnimController null. key:{charKey}, groupid:{groupID}, type:{controllerType}" );
        return null;
    }

    public float GetAnimationClipLength(string charKey, long groupID, string clipName, ANIMCONTROLLER_USE_TYPE useType)
    {
        CAnimationControllerInfo _animCntlrinfo = CCharAnimControllerDataManager.Instance.GetAnimControllerInfoByType( groupID, useType );
        if (_animCntlrinfo == null)
        {
            CDebug.LogError( $" SetRuntimeAnimatorController, Group ID:{groupID}, useType:{useType} doesn't have no data" );
            return 0f;
        }
        AnimControllerInfo _info = GetAnimControllerInfo( charKey, groupID, _animCntlrinfo.ControllerType );

        if (_info != null)
        {
            return _info.GetAnimClipLength( clipName );
        }

        return 0f;
    }


    public void ReleaseRuntimeAnimController(string charKey, long groupID, bool releaseAll = false)
    {
        if(charAnimListDic != null && charAnimListDic.Count > 0)
        {
            if (ischarAnimListDicContainKey( charKey ))
            {
                for (int i = charAnimListDic[charKey].Count - 1; i >= 0; --i)
                {
                    if (ischarAnimListDicContainKey( charKey ))
                    {
                        bool bReleased = charAnimListDic[charKey][i].Release( releaseAll );
                        if (bReleased)
                        {
                            charAnimListDic.Remove( charKey );
                        }
                    }
                }
            }
        }
    }

    private bool ischarAnimListDicContainKey(string charKey )
    {
        if (charAnimListDic.ContainsKey( charKey ))
        {
            return true;
        }

        return false;
    }

    public void ClearUsingClipNameDic()
    {
        CDebug.Log( "   ** [ ClearUsingClipNameDic ] **  " );
        UsingClipNamesDic.Clear();
    }

    public void ReleaseAll()
    {
        if(charAnimListDic != null)
        {
            foreach ( KeyValuePair<string, List<AnimControllerInfo>> item in charAnimListDic)
            {
                item.Value.ForEach( info =>
                {
                    info.Release( true );
                } );
                item.Value.Clear();
            }

            charAnimListDic.Clear();
            charAnimListDic = null;
        }

    }
}


public class AnimControllerInfo
{
    public RuntimeAnimatorController RunAnimController;
    public long GroupID;
    public ANIMCONTROLLER_USE_TYPE UseType;
    public ANIMCONTROLLER_TYPE ControllerType;
    public Dictionary<string, float> AnimClipLengthDic;

    public AnimControllerInfo(RuntimeAnimatorController controller, ANIMCONTROLLER_USE_TYPE useType, ANIMCONTROLLER_TYPE type, long grpID)
    {
        this.RunAnimController = controller;
        this.UseType= useType;
        this.ControllerType = type;
        this.GroupID = grpID;
        this.AnimClipLengthDic = new Dictionary<string, float>();
        if (RunAnimController != null)
        {
            SetAnimClipLength();
        }
    }

    public void SetAnimClipLength()
    {
        AnimationClip[] clips = RunAnimController.animationClips;

        for(int i=0; i<clips.Length; ++i)
        {
            AnimationClip clip = clips[i];

            if (AnimClipLengthDic.ContainsKey( clip.name ) == false)
            {
                AnimClipLengthDic.Add( clip.name, clip.length );
            }
        }
    }

    public float GetAnimClipLength( string clipName )
    {
        if(AnimClipLengthDic.ContainsKey(clipName))
        {
            return AnimClipLengthDic[clipName];
        }

        return 0f;
    }

    public bool Release(bool releaseAll = false)
    {
        if (releaseAll)
        {
            if (ControllerType != ANIMCONTROLLER_TYPE.COMMON)
            {
                if (RunAnimController != null)
                {
                    CDebug.Log( $"   ** [ Release RunAnimController] **  type : {ControllerType}" );
                    //Addressables.Release( RunAnimController );
                    RunAnimController = null;
                }

                return true;
            }
        }

        return false;
    }
}
