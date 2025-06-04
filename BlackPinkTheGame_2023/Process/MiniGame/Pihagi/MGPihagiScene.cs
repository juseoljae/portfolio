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

public class MGPihagiScene : CBaseSceneExtends
{
    //MGPihagi
    private SingleAssignmentDisposable disposer;

    public override void OnStart(int previousSceneId)
    {
        CDebug.Log($"{GetType()} OnStart ");

        director = MGPihagiDirector.Instance;
        Input.multiTouchEnabled = false;

        sceneTransitionService = CCoreServices.GetCoreService<SceneTransitionService>();
        sceneTransitionService.SetTransition(SceneTransitionLoading.Instance);

        SetManagerComponent();

        OnEntering();
    }

    protected override void SetManagerComponent()
    {
        CDebug.Log($"{GetType()}. SetManagerComponent ");

        manager_Gameobject = new GameObject(CDefines.BPW_MINIGAME_PIHAGI_MANAGER_NAME);
        manager = manager_Gameobject.AddComponent<MGPihagiManager>();

        //JH 3D View Scene
        CWorldViewFactory worldViewFactory = CCoreFactories.GetCoreFactory<CWorldViewFactory>();
        CWorldView worldView = worldViewFactory.GetView(CWorldViewId.MINIGAME_PIHAGI_VIEW);

    }

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
        MGPihagiDirector.Instance.Leave();

        Input.multiTouchEnabled = true;
    }

}
