using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CRuntimeAnimControllerManager : SingletonMono<CRuntimeAnimControllerManager>
{
    private Dictionary<ANIM_CONTROLLER_TYPE, RuntimeAnimatorController> AnimControllerDics = new Dictionary<ANIM_CONTROLLER_TYPE, RuntimeAnimatorController>();
    private const string ANIMATOR_NAME_SNG = "member_controller";
    private const string ANIMATOR_NAME_MARBLE = "member_mgmarble_controller";
    private GameObject releaseObj;


    public void SetControllerDic()
    {
        releaseObj = new GameObject("AnimControllerReleaseObj");
        
        if (AnimControllerDics != null && AnimControllerDics.Count > 0) return;
        
        var sngController = LoadRuntimeAnimController(ANIMATOR_NAME_SNG);
        if (sngController != null) AnimControllerDics.Add(ANIM_CONTROLLER_TYPE.SNG, sngController);

        var marbleController = LoadRuntimeAnimController(ANIMATOR_NAME_MARBLE);
        if (marbleController != null) AnimControllerDics.Add(ANIM_CONTROLLER_TYPE.MARBLE, marbleController);
        
    }


    public void ChangeController(Animator targetAnimator, ANIM_CONTROLLER_TYPE type)
    {
        if (targetAnimator == null) return;

        if (AnimControllerDics.TryGetValue(type, out RuntimeAnimatorController newController))
        {
            targetAnimator.runtimeAnimatorController = newController;
        }
    }

    
    public void RestoreController(Animator target)
    {
        if (target == null) return;

        if (AnimControllerDics.TryGetValue(ANIM_CONTROLLER_TYPE.SNG, out RuntimeAnimatorController original))
        {
            target.runtimeAnimatorController = original;
        }
    }

    

    private RuntimeAnimatorController LoadRuntimeAnimController(string resPath)
    {
        RuntimeAnimatorController _rac = null;

        if (resPath.Equals( string.Empty ))
        {
            CDebug.LogError( $" LoadRuntimeAnimController, doesn't have no controller" );
            return null;
        }

        CResourceData resData = CResourceManager.Instance.GetResourceData( resPath );
        _rac = resData.Load<RuntimeAnimatorController>(releaseObj);
        if (_rac == null)
        {
            CDebug.Log( $" [avatar controller] LoadRuntimeAnimController _rac null. resPath:{resPath}" );
        }

        return _rac;
    }
}


