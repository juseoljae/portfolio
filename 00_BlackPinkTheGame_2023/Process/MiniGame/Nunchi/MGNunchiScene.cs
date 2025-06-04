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

public class MGNunchiScene : CBaseSceneExtends
{
    private SingleAssignmentDisposable disposer;

    public override void OnStart(int previousSceneId)
    {
        CDebug.Log($"{GetType()} OnStart ");

        director = MGNunchiDirector.Instance;
        Input.multiTouchEnabled = false;

        sceneTransitionService = CCoreServices.GetCoreService<SceneTransitionService>();
        sceneTransitionService.SetTransition(SceneTransitionLoading.Instance);

        SetManagerComponent();

        OnEntering();
    }

    protected override void SetManagerComponent()
    {
        CDebug.Log($"{GetType()}. SetManagerComponent ");

        manager_Gameobject = new GameObject(CDefines.BPW_MINIGAME_NUNCHI_MANAGER_NAME);
        manager = manager_Gameobject.AddComponent<MGNunchiManager>();

        //JH 3D View Scene
        CWorldViewFactory worldViewFactory = CCoreFactories.GetCoreFactory<CWorldViewFactory>();
        CWorldView worldView = worldViewFactory.GetView(CWorldViewId.MINIGAME_NUNCHI_VIEW);

    }

    public override void SetSceneState(SceneState state)
    {
        _scene_state = state;
        Debug.Log($"SetSceneState:: {_scene_state}");

        switch (_scene_state)
        {
            case SceneState.None:
                break;
            case SceneState.Enter:
                sceneTransitionService.Out().Subscribe(d =>
                {
                    Debug.Log("SetSceneState::loading out");

                }).AddTo(manager);
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

    protected override IObservable<Unit> _didFinishTransitionOut()
    {
        CDebug.Log($"{GetType()}. override _didFinishTransitionOut", CDebugTag.SCENE);
        //

        MGNunchiManager mgr = (MGNunchiManager)manager;

        mgr.WorldManager.SetChangeState(MGNunchiGameState_IntroCam.Instance());
        return base._didFinishTransitionOut();
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
        MGNunchiDirector.Instance.Leave();

        Input.multiTouchEnabled = true;
    }
}
