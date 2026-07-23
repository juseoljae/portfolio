using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using Game.RestAPI;
using Navigation;

public class MarbleScene : CBaseScene
{
    private CAssetService assetService;
    private CUIService cUIService = null;
    private GameObject m_manager_Gameobject;
    private MarbleManager manager;
    private MarbleCanvasManager canvas_manager;
    private CMarbleWorldManager MarbleWorldMgr;

    public MarbleScene()
    {
        assetService = CCoreServices.GetCoreService<CAssetService>();
    }

    public MarbleCanvasManager GetCanvasManager()
    {
        return canvas_manager;
    }

    public CMarbleWorldManager GetWorldManager()
    {
        return MarbleWorldMgr;
    }

    public override void OnStart(int previousSceneId)
    {
        Input.multiTouchEnabled = true;

        MarbleDirector.Instance.Start_Enter();
        SetSceneState(SceneState.None);
    }

    public void SetSceneState(SceneState state)
    {
        scene_state = state;

        switch (scene_state)
        {
            case SceneState.None:
                break;

            case SceneState.Enter:
                cUIService = CCoreServices.GetCoreService<CUIService>();
                GameObject PageRootObject = cUIService.GetContentsRootObject(CUIId.MINIGAME_MARBLE_UI);

                canvas_manager = PageRootObject.GetComponent<MarbleCanvasManager>();
                canvas_manager.Initialize(this);

                GameObject Scene = GameObject.Find("Scene");
                if (Scene != null)
                {
                    MarbleWorldMgr = Scene.GetComponent<CMarbleWorldManager>();
                    if (MarbleWorldMgr != null)
                    {
                        NavigationInfo naviInfo = CNavigationManager.Instance.GetCurrentNavigationInfo;
                        if (naviInfo != null)
                        {
                            if (naviInfo.TData.navi_scene != (int)CSceneId.MINIGAME_MARBLE_SCENE)
                            {
                                MarbleAPI_TOP();
                            }
                            else
                            {
                                int memberSelectNavi = naviInfo.navi_sub_value;
                                if (memberSelectNavi != 0)
                                {
                                    MGMarbleMemberSelectForNavi(memberSelectNavi.ToEnum<MEMBER_TYPE>());
                                }
                                else
                                {
                                    if (CMarbleServerDataManager.Instance.GetMarbleMemberSelectInfo() == MEMBER_TYPE.NONE)
                                    {
                                        MEMBER_TYPE member = LobbyDirector.Instance.GetLobbyMemberData().member;
                                        MGMarbleMemberSelectForNavi(member);
                                    }
                                    else
                                    {
                                        MarbleAPI_TOP(CMarbleDefine.IS_FROM_NAVIGATION);                                
                                    }
                                }                                
                            }

                        }
                        else
                        {
                            MarbleAPI_TOP();
                        }
                    }
                    else
                    {
                        MarbleAPI_TOP();
                    }
                }
                else
                {
                    MarbleAPI_TOP();
                }

                if (LoadingLayer.Instance.isFadeing)
                {
                    LoadingLayer.Instance.StartFadeAnim(LoadingLayer.FADE_TYPE.IN, () => {} );
                }

                SetSceneState(SceneState.Proc);

                break;

            case SceneState.Permision:
                break;

            case SceneState.Proc:
                CSoundControl.Instance.LoadSoundPlay((int)SCENE_BGM.BGM_DICEGAME);

                break;

            case SceneState.Leave:
                LeaveScene();
                break;
        }
    }

    private void MarbleAPI_TOP(int fromNavi = 0)
    {
        SingleAssignmentDisposable _disposer = new SingleAssignmentDisposable();
        _disposer.Disposable = APIHelper.MarbleService.MGMarbleTop(fromNavi)
            .Subscribe(result =>
            {
                if (result.d.dicegame_info == null || result.d.dicegame_tile_map == null)
                {
                    CDebug.LogError("MarbleScene::SceneState.Enter - 'result.d.dicegame_info' or 'result.d.dicegame_tile_map' is null.");
                    return;
                }

                DicegameInfo game_info = result.d.dicegame_info;
                List<DicegameTileMap> tile_map = result.d.dicegame_tile_map;

                CMarbleServerDataManager.Instance.SetMarbleGameInfo(result.d.dicegame_info, result.d.dicegame_tile_map);
                
                MEMBER_TYPE curMemberType = result.d.dicegame_info.member_id.ToEnum<MEMBER_TYPE>();
                SingleAssignmentDisposable disposer = new SingleAssignmentDisposable();
                disposer.Disposable = APIHelper.SNGService.ReqSNG_Avatar(((int)curMemberType).ToString(), 0)
                .Subscribe(result =>
                {
                    CRuntimeAnimControllerManager.Instance.SetControllerDic();
                    MarbleWorldMgr.Initialize(canvas_manager);
                    
                    SNGAPI.SNG_SimpleRequestData req = new SNGAPI.SNG_SimpleRequestData();
                    req.uid = CPlayer.GetStatusLoginValue(PLAYER_STATUS.USER_ID);
                    APIHelper.SNGService.ReqSNG_Products(req, false, null).Subscribe(_ =>
                    {
                        CDebug.Log("MarbleScene::MarbleAPI_TOP - Product data refreshed after entering Marble scene.");
                    });

                    disposer.Dispose();
                });

                if (result.d.avatar_list != null && result.d.avatar_list.Count > 0)
                {
                    CMemberAvatarServerDataManager.Instance.SetMemberAvatarInfo(result.d.avatar_list);
                }

                if (result.d.dicegame_ap != null)
                {
                    CMarbleServerDataManager.Instance.SetGameApInfo(result.d.dicegame_ap);
                }
 
                CNavigationManager.Instance.DisposeNavigationData();   
                _disposer.Dispose();
            });
    }

    private void MGMarbleMemberSelectForNavi(MEMBER_TYPE member)
    {
        if (member == MEMBER_TYPE.NONE)
        {
            CDebug.LogError("MarbleScene::MGMarbleMemberSelectForNavi - Invalid member type: NONE");
            return;
        }
        
        SingleAssignmentDisposable _disposer = new SingleAssignmentDisposable();
        _disposer.Disposable = APIHelper.MarbleService.MGMarbleMemberSelect(member)
            .Subscribe(resData =>
            {    
                MarbleAPI_TOP(CMarbleDefine.IS_FROM_NAVIGATION);
                _disposer.Dispose();
            });
    }

    void SetupTownMapMgr()
    {
        /*
        GameObject go = GameObject.Find("TownMapMgr");
        if (go)
        {
            GameObject.Destroy(go);
        }

        GameObject TownMapMgrObj = new GameObject("TownMapMgr");
        GameObject Scene = GameObject.Find("Scene");

        TownMapMgrObj.transform.parent = Scene.transform;

        TownMapMgr townMapMgr = TownMapMgrObj.GetComponent<TownMapMgr>();
        if (!townMapMgr)
            townMapMgr = TownMapMgrObj.AddComponent<TownMapMgr>();
        */

        // recv 에서 처리해야 할 것을 테스트
        //TownMapMgr.Instance.SetupTiles();

        // 초기값은 일단 None 모드
        //TouchMgr.Inst.SetTouchMode(TouchMgr.eTouch_mode.Play, false);
    }

    private void LeaveScene()
    {
        Input.multiTouchEnabled = false;
    }


    public override void OnUpdate()
    {
        switch (scene_state)
        {
            case SceneState.None:
                if (MarbleDirector.Instance.CompletedTransaction())
                {
                    SetSceneState(SceneState.Enter);
                }
                break;

            case SceneState.Enter:
                break;

            case SceneState.Proc:
                break;

            case SceneState.Leave:
                break;
        }
    }

    public override void OnAppPaused() 
    {
        base.OnAppPaused();

        // if (SceneTownBase.Inst != null)
        // {
        //     GameObject Scene = SceneTownBase.Inst.SCENE;
        //     if (Scene)
        //     {
        //         Scene.GetComponent<CamController>().PauseHandler();
        //         //Scene.GetComponent<DanceCamController>().PauseHandler();
        //     }
        // }

        //TouchManagerPause(true);

    }

    public override void OnAppResumed()
    {
        base.OnAppResumed();

        // if (SceneTownBase.Inst != null)
        // {
        //     GameObject Scene = SceneTownBase.Inst.SCENE;
        //     if (Scene)
        //     {
        //         Scene.GetComponent<CamController>().ResumeHandler();
        //         //Scene.GetComponent<DanceCamController>().ResumeHandler();
        //     }
        // }

      //  TouchManagerPause(false);
    }
    

    public override void OnFinish()
    {
        Input.multiTouchEnabled = false;

        MarbleWorldMgr.Release();

        scene_state = SceneState.None;
        MarbleDirector.Instance.Leave();
    }
}
