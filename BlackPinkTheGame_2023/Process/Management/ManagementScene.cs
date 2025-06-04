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

using UnityEngine;
using UniRx;
using System;

public class ManagementScene : CBaseSceneExtends
{

    public override void OnStart(int previousSceneId)
    {
        CDebug.Log($"{GetType()} OnStart ");

        director = ManagementDirector.Instance;
        Input.multiTouchEnabled = true;

        sceneTransitionService = CCoreServices.GetCoreService<SceneTransitionService>();
        sceneTransitionService.SetTransition(SceneTransitionLoading.Instance);

        SetManagerComponent();

        OnEntering();
    }


    protected override void SetManagerComponent()
    {
        CDebug.Log($"{GetType()}. SetManagerComponent ");
        
        manager_Gameobject = new GameObject(CDefines.MANAGEMENT_MANAGER_NAME);
        manager = manager_Gameobject.AddComponent<ManagementManager>();

        //JH 3D View Scene
        CWorldViewFactory worldViewFactory = CCoreFactories.GetCoreFactory<CWorldViewFactory>();
        CWorldView worldView = worldViewFactory.GetView(CWorldViewId.MANAGEMENT_VIEW);

    }


    /// <summary>
    /// Scene Enter Process
    /// </summary>
    /* 
    protected override void OnEntering()
    {
    } 
    */

    public override void SetSceneState(SceneState state)
    {
        _scene_state = state;

        switch (_scene_state)
        {
            case SceneState.None:
                break;
            case SceneState.Enter:
                break;
            case SceneState.Proc:
                break;

            case SceneState.Leave:
                sceneTransitionService.SetTransition(SceneTransitionFade.Instance);
                sceneTransitionService.In()
                .Subscribe(_ => CDirector.Instance.ChangeScene((int)SceneID.LOBBY_SCENE))
                .AddTo(manager);
                break;
        }
    }


    public override void OnFinish()
    {
        base.OnFinish();

        if (manager != null)
        {
            manager.Release();
            manager = null;
        }

        sceneTransitionService = null;

        _scene_state = SceneState.None;
        ManagementDirector.Instance.Leave();

        Input.multiTouchEnabled = true;
    }
}
