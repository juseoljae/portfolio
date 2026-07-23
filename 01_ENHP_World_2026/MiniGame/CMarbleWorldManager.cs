//#define _EDITOR_DIRECT_PLAY_MARBLE

using System;//
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using Game.RestAPI;

public class CMarbleWorldManager : MonoBehaviour
{
    private MarbleCanvasManager CanvasMgr;
    private PageMarble MarblePage;
    private CMarblePlayerManager PlayerMgr;
    private CMarbleDiceManager DiceMgr;
    private CStateMachine<CMarbleWorldManager> MainSM;
    //private CMarbleCameraController CamController;
    private CMarbleBoardGenerator BoardGenerator;
    private GameObject MainBoardObj;

    private CamController CamCntlr;

    //Server Top info
    private DicegameInfo GameInfo;
    //private List<DicegameTileMap> BlockInfos;

    private MEMBER_TYPE curMemberType = MEMBER_TYPE.JUNGWON;

    private MARBLE_GAME_STATE CurrentMainState;
    
    private bool bBlockDiceRolling;
    private int DiceRollResult;
    
    private bool IsFastMove;
    
    private int BonusRewardRate; // bunus reward rate from member improve level

    private bool IsAutoMode;
    

    //[SerializeField] private Camera MainCamera;
    [SerializeField] private GameObject MapRootObj;

#if _EDITOR_DIRECT_PLAY_MARBLE
    void Awake()
    {
        Initialize(null);
    }
#endif

    public void Initialize(MarbleCanvasManager canvasMgr)
    {
        GameInfo = CMarbleServerDataManager.Instance.GetGameInfo();
        //BlockInfos = CMarbleServerDataManager.Instance.GetMapInfos();

        MainSM = new CStateMachine<CMarbleWorldManager>(this);

        CanvasMgr = canvasMgr;

        CamCntlr = gameObject.GetComponent<CamController>();
        if (CamCntlr != null)
        {
            CamCntlr.Init(this);
            CamCntlr.SetBaseDirection();
        }


        GameObject boardGenObj = MapRootObj.transform.Find("GameBoard").gameObject;
        if (boardGenObj != null)
        {
            BoardGenerator = boardGenObj.GetComponent<CMarbleBoardGenerator>();
            BoardGenerator.Init(this); // -> need to go state machine
            
            //SetCurrentBlockIndex(GameInfo.tile_last);//set Server info
        }
        
        MEMBER_TYPE curMember = GameInfo.member_id.ToEnum<MEMBER_TYPE>();
        SetCurrentMemberType(curMember);//set Server info

        GameObject playerObj = transform.Find("Player").gameObject;
        if (playerObj != null)
        {
            PlayerMgr = playerObj.GetComponent<CMarblePlayerManager>();
            if (PlayerMgr != null)
            {
                PlayerMgr.Initialize(this);
            }
        }


        GameObject diceMgrObj = transform.Find("DiceParent").gameObject;
        if (diceMgrObj != null)
        {
            DiceMgr = diceMgrObj.GetComponent<CMarbleDiceManager>();
            DiceMgr.Init(this);
        }
        

        SetMainState(MARBLE_GAME_STATE.IDLE);

        CanvasMgr.SetUIPage(this);
        MarblePage = CanvasMgr.GetMarblePage();
        BoardGenerator.SetMarblePage(MarblePage);

        SetImproveRewardValue();

        PlayerMgr.SetPlayerPosition();

        SetBlockStateContinuous();


        // SingleAssignmentDisposable camFucusDisp = new SingleAssignmentDisposable();
        // camFucusDisp.Disposable = Observable.Timer(System.TimeSpan.FromSeconds(0.5f))
        //     .Subscribe(_ =>
        //     {
        //         CamFocusToTarget(1.5f);
        //         camFucusDisp.Dispose();
        //     });
        //MainBoardObj = MapRootObj.transform.Find("BgBoard").gameObject;

        //CamController = MainCamera.gameObject.GetComponent<CMarbleCameraController>();
        //CamController.Initialize(this);
    }
    
    public void SetImproveRewardValue()
    {        
        MEMBER_TYPE currentMemberType = GetCurrentMemberType();
        int diceRangeGroup = 0;
        List<ImproveFinalInfo> improveFinalInfos = new List<ImproveFinalInfo>();
        List<ImproveList> finalImmpInfos = CMemberAvatarServerDataManager.Instance.GetMemberAvatarImproveList((int)currentMemberType);
        if (finalImmpInfos != null)
        {
            foreach (var info in finalImmpInfos)
            {
                if (info.lv > 0)
                {                    
                    VamkidzImproveData improveData = VamkidzImproveDataManager.Instance.GetVamkidzImproveDataByID(info.improve_id);
                    if (improveData != null)
                    {
                        if (improveData.improve_type == VAMKIDZ_IMPROVE_TYPE.VAMKIDZ_DICE_TILE_REWARD_UP)
                        {                            
                            int value = VamkidzImproveDataManager.Instance.GetImproveLevelValue(currentMemberType, improveData.improve_type, info.lv, improveData.improve_subtype);
                            if (value > 0)
                            {
                                ImproveFinalInfo improveFinalInfo = new ImproveFinalInfo();
                                improveFinalInfo.SubTypeID = improveData.improve_subtype;
                                improveFinalInfo.Value = value;
                                improveFinalInfos.Add(improveFinalInfo);
                            }
                        }
                        else if (improveData.improve_type == VAMKIDZ_IMPROVE_TYPE.VAMKIDZ_DICE_RANGE_GROUP)
                        {
                            diceRangeGroup = VamkidzImproveDataManager.Instance.GetImproveLevelValue(currentMemberType, improveData.improve_type, info.lv);
                        }
                        else if (improveData.improve_type == VAMKIDZ_IMPROVE_TYPE.VAMKIDZ_DICE_AUTO)
                        {
                            MarblePage.SetIsUnLockAutoMode(true);
                        }
                    }
                }
            }

            SetImpoveValues(improveFinalInfos, diceRangeGroup);
        }

    }

    public void SetImproveNewAlert()
    {       
        MarblePage.ConfirmImproveNewAlert();
    }

    private void Update()
    {
        if (CResourceManager.Instance.GetSafeAreaObjectDic() != null && GetCamController() != null)
        {
            //화면을 덮는 page가 있는지 체크(주사위ui 페이지 한개가 기본으로 깔림)
            bool has_page = CResourceManager.Instance.GetSafeAreaObjectDic().transform.childCount > 1;
            
            if(has_page)
            {
                GetCamController().enabled = false;
            }
             
            if (!has_page)
            {
                //화면을 덮는 popup 있는지 체크(팝업레이어에 Frame라는 오브젝트 한개가 디폴트로 깔림)
                bool has_popup = false;
                var popupLayer = CStaticCanvas.Instance.GetLayer<PopupLayer>(PopupLayer.NAME);
                has_popup = popupLayer != null && popupLayer.transform.childCount > 1;
                
                if (has_popup)
                {
                    GetCamController().enabled = false;
                }
                else
                {
                    if (TutorialManager.Instance.IsPlay)
                    {
                        GetCamController().enabled = false;
                    }
                    else
                    {
                        GetCamController().enabled = true;    
                    }
                }
            }
        }
        

        
        if (BoardGenerator != null)
        {
            BoardGenerator.UpdateBlockStateMachine();
        }

        if (PlayerMgr != null)
        {
            PlayerMgr.UpdatePlayerStateMachine();
        }

        if (DiceMgr != null)
        {
            DiceMgr.UpdateStateMachine();
        }

        if (MainSM != null)
        {
            MainSM.StateMachine_Update();
        }

        #if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.M))
        {
            CamCntlr.MarbleCamFocusToTarget(PlayerMgr.GetCurrentPlayerObj());
        }
        #endif
    }
    

    public void SetMoveResult(DicegameInfo dicegame_info, List<DicegameTileMap> dicegame_tile_map, List<RewardList> reward_list, bool isSelectMove = false)
    {        
        if (dicegame_info == null)
        {
            CDebug.LogError("response value of MGMarbleDiceRoll is null");
            return;
        }

        CMarbleServerDataManager.Instance.SetMarbleGameInfo(dicegame_info);

        int lastTileIndex = dicegame_info.tile_last;
        int curTileIndex = BoardGenerator.GetBlockTileManager().GetCurrentBlockIndex();
        int moveCount = lastTileIndex - curTileIndex;

        if (moveCount < 0)
        {
            moveCount = (CMarbleDefine.BLOCK_MAX_COUNT - curTileIndex) + lastTileIndex;
        }
        
        if (isSelectMove)
        {
            SetDiceRollResult(moveCount);
        }
        else
        {
            //set dice working
            DiceMgr.SetDiceWork(moveCount);            
        }

        if (dicegame_tile_map != null && dicegame_tile_map.Count > 0)
        {
            CMarbleServerDataManager.Instance.SetRenewalMapInfos(dicegame_info, dicegame_tile_map);
        }

        if (reward_list != null && reward_list.Count > 0)
        {
            CMarbleServerDataManager.Instance.SetRewardInfos(reward_list);

            // foreach (var reward in reward_list)
            // {
            //     Debug.Log($"###### SetMoveResult Reward type: {reward.type}, Reward Value: {reward.value} /// lastTileIndex : {lastTileIndex}");
            // }
        }
    }

    public void SetActiveMoveNoticeUI(bool bActive)
    {
        MarblePage.SetActiveMoveNoticeUI(bActive);
    }

    public void SetCurrentBlockIndex(int blockIndex)
    {
        BoardGenerator.SetCurrentBlockIndex(blockIndex);
    }

    public void SetDiceRollResult(int rollResult)
    {
        DiceRollResult = rollResult;
    }

    public int GetDiceRollResult()
    {
        return DiceRollResult;
    }

    // public void SetSelectMovingBlock_Result(int targetBlockIdx)
    // {        
    //     int curBlockIdx = BordGenerator.GetBlockTileManager().GetCurrentBlockIndex();
    //     int moveCount = targetBlockIdx - curBlockIdx;

    //     if (moveCount < 0)
    //     {
    //         moveCount = (CMarbleDefine.BLOCK_MAX_COUNT - curBlockIdx) + targetBlockIdx;
    //     }

    //     SetDiceRollResult(moveCount);
    // }
    
    public void SetMainState(MARBLE_GAME_STATE state)
    {
        CurrentMainState = state;

        switch (state)
        {
            case MARBLE_GAME_STATE.ENTRY:
                MainSM.ChangeState(CMarbleMainState_Idle.Instance());
                break;
            case MARBLE_GAME_STATE.IDLE:
                MainSM.ChangeState(CMarbleMainState_Idle.Instance());
                break;
            case MARBLE_GAME_STATE.PLAY:
                MainSM.ChangeState(CMarbleMainState_Play.Instance());
                break;
            case MARBLE_GAME_STATE.CAM_ACTION_RESTORE:
                MainSM.ChangeState(CMarbleMainState_CamActionRestore.Instance());
                break;
            // case MARBLE_GAME_STATE.FLY_BUFF_EFFECT:
            //     MainSM.ChangeState(CMarbleMainState_FlyBuffEff.Instance());
                //break;
            case MARBLE_GAME_STATE.FINISH:
                //MainSM.ChangeState(CMarbleMainState_Finish.Instance());
                break;
        }
    }

    public MARBLE_GAME_STATE GetMainState()
    {
        return CurrentMainState;
    }

    public void SetPlayerRestoreState()
    {
        PlayerMgr.SetRestorePlayer();
    }

    public void SetPlayerState(MARBLE_PLAYER_STATE state)
    {
        PlayerMgr.SetPlayerState(state);
    }

    public void SetBonusRewardRateFromImproveLevel(int rate)
    {
        BonusRewardRate += rate;
    }

    public void CamFocusToTarget(float duration = 0.5f)
    {
        CamCntlr.MarbleCamFocusToTarget(PlayerMgr.GetCurrentPlayerObj(), null, CMarbleDefine.MARBLE_TARGET_DIST, duration);
    }

    public void RestoreCamPosition(float duration = 0.5f)
    {
        SetCanSelectMove(false);
        CamCntlr.ResetMarbleCamFocus(FinishRestoreCam, duration);
    }
    

    public void FinishRestoreCam()
    {
        SetCanSelectMove(true);
    }
    bool CanSelectMove;
    public void SetCanSelectMove(bool canSelect)
    {
        CanSelectMove = canSelect;
    }

    public bool GetCanSelectMove()
    {
        return CanSelectMove;
    }

    public void SetFollowTarget()
    {
        CamCntlr.SetFollowTarget(PlayerMgr.GetCurrentPlayerObj());
    }

    public void FollowCamToPlayer()
    {
        CamCntlr.FollowTarget();
    }

    public void StopFollowCam()
    {
        CamCntlr.StopFollowTarget();
    }

    public void UpdateExpItemValue(int itemID)
    {
        MarblePage.SetExpItemValue(itemID);
    }

    public DicegameInfo GetGameInfo()
    {
        return GameInfo;
    }
    
    public CMarblePlayerManager GetPlayerManager()
    {
        return PlayerMgr;
    }
    public CMarbleDiceManager GetDiceManager()
    {
        return DiceMgr;
    }

    public CMarbleBlockTileManager GetBlockTileManager()
    {
        return BoardGenerator.GetBlockTileManager();
    }

    public GameObject GetMapRootObj()
    {
        return MapRootObj;
    }

    public GameObject GetMainBoardObj()
    {
        return MainBoardObj;
    }

    public CamController GetCamController()
    {
        return CamCntlr;
    }

    public void SetCurrentMemberType(MEMBER_TYPE memberType)
    {
        curMemberType = memberType;
        SNGDataManager.Instance.SetCurDiceMemberType(memberType);
    }

    public MEMBER_TYPE GetCurrentMemberType()
    {
        return curMemberType;
    }

    public bool CanPlayDiceRolling()
    {
        if (CurrentMainState == MARBLE_GAME_STATE.PLAY || CurrentMainState == MARBLE_GAME_STATE.CAM_ACTION_RESTORE)
        {
            return false;
        }

        return true;
    }

    public void SetBlockDiceRolling(bool bRolling)
    {
        bBlockDiceRolling = bRolling;

        SetActiveBottomDisableUI(bRolling);
    }

    public void SetActiveBottomDisableUI(bool bActive)
    {
        if (MarblePage != null) MarblePage.SetActiveDisableUI(bActive);
    }

    public bool IsBlockDiceRolling()
    {
        return bBlockDiceRolling;
    }

    public void SetMainStateMovingComplete(MARBLE_BLOCK_TYPE blockState)
    {
        float waitTime = 0.5f;
        bool reset = true;
        switch (blockState)
        {
            case MARBLE_BLOCK_TYPE.REWARD:
                waitTime = 0.3f;
                break;
            case MARBLE_BLOCK_TYPE.MOVE:
                reset = false;
                break;
            case MARBLE_BLOCK_TYPE.REWARD_BUFF:
            case MARBLE_BLOCK_TYPE.REWARD_DEBUFF:
                waitTime = CMarbleDefine.BLOCK_BUFF_FLYING_TIME;
                break;
        }

        if (!reset) return;

        SingleAssignmentDisposable disposable = new SingleAssignmentDisposable();
        disposable.Disposable = Observable.Timer(TimeSpan.FromSeconds(waitTime))
            .Subscribe(__ =>
            {
                SetMainState(MARBLE_GAME_STATE.IDLE);
                disposable.Dispose();
            }).AddTo(this);
    }

    private void SetBlockStateContinuous()
    {
        DicegameInfo gameInfo = CMarbleServerDataManager.Instance.GetGameInfo();
        CMarbleBlockTileManager bTileMgr = GetBlockTileManager();

        if (gameInfo == null || bTileMgr == null) return;

        CMarbleBlock block = bTileMgr.GetBlockTileByIndexForDicePos(gameInfo.tile_last);
        if (block == null) return;

        bool isMoveType = block.GetBlockType() == MARBLE_BLOCK_TYPE.MOVE;
        bool shouldSetState = !isMoveType || (gameInfo.tile_move_flag == 0);

        //true is not move tile
        if (shouldSetState)
        {
            PlayerMgr.SetBlockStateByBlockType(block, true);
        }
        else
        {
            BoardGenerator.GetBlockTileManager().SetBlockTileInActive();
        }
        
        BoardGenerator.SetBlockBuffState();
    }

    

    public bool IsBlockFollowCam()
    {
        if (PlayerMgr.GetPlayerState() == MARBLE_PLAYER_STATE.WAIT ||
            PlayerMgr.GetPlayerState() == MARBLE_PLAYER_STATE.ARRIVE)
        {
            return true;
        }

        if (BoardGenerator.GetCurBlockState() == MARBLE_BLOCK_STATE.BUFF || 
            BoardGenerator.GetCurBlockState() == MARBLE_BLOCK_STATE.DEBUFF)
        {
            return true;
        }
        
        return false;
    }

    public bool IsBlockCamContol()
    {
        if (PlayerMgr.GetPlayerState() == MARBLE_PLAYER_STATE.PLAY)
        {
            return true;
        }

        if (BoardGenerator.GetCurBlockState() == MARBLE_BLOCK_STATE.BUFF || 
            BoardGenerator.GetCurBlockState() == MARBLE_BLOCK_STATE.DEBUFF ||
            BoardGenerator.GetCurBlockState() == MARBLE_BLOCK_STATE.INACTIVE)
        {
            return true;
        }

        if (CurrentMainState == MARBLE_GAME_STATE.CAM_ACTION_RESTORE)
        {
            return true;
        }

        return false;
    }

    public void SetActiveMoveEff_CharTrail(bool isActive)
    {
        PlayerMgr.SetActiveMoveEff_CharTrail(isActive);
    }

    public void SetIsFastMove(bool isFast)
    {
        IsFastMove = isFast;
    }

    public void SetIsRestorePlayer(bool isRestore)
    {
        PlayerMgr.SetIsRestorePlayer(isRestore);
    }

    public bool GetIsFastMove()
    {
        return IsFastMove;
    }

    public void SetImpoveValues(List<ImproveFinalInfo> infos, int diceRangeGroupID)
    {
        if (infos.Count > 0)
        {
            foreach (var info in infos)
            {
                BoardGenerator.AddImproveRewardValue(info.SubTypeID, info.Value);                
            }
        }

        if (diceRangeGroupID > 0)
        {
            MarblePage.ClearDiceRangeGrpDataList();
            MarblePage.SetDiceRange(diceRangeGroupID);
        }

        RefreshExpItemInfo();
    }

    public void SetActiveObjsForImproveOpen(bool isActive)
    {
        MarblePage.SetActiveExpPointObj(isActive);
        PlayerMgr.SetActivePlayerObj(isActive);
    }

    public void RefreshExpItemInfo()
    {
        MarblePage.RefreshExpItemInfo();
    }

    public CMarbleBlock GetCurrentBlock()
    {
        int curBlockIdx = BoardGenerator.GetBlockTileManager().GetCurrentBlockIndex();
        CMarbleBlock curBlock = BoardGenerator.GetBlockTileManager().GetBlockTileByIndexForDicePos(curBlockIdx);
        return curBlock;
    }

    public void SetAutoMode(bool isAuto)
    {
        IsAutoMode = isAuto;

        if (isAuto)
        {
            StartAutoRolling();
        }
        else
        {
            MarblePage.SetActiveAutoModeUI(false);
            if (GetMainState() == MARBLE_GAME_STATE.CAM_ACTION_RESTORE)
            {
                if (GetBlockTileManager().GetCurrentBlockType() == MARBLE_BLOCK_TYPE.MOVE)
                {
                    MarblePage.SetActiveAutoModeObj(false);
                }
            }
            else
            {
                MarblePage.SetActiveAutoModeObj(true);
            }
        }
    }

    public bool GetIsAutoMode()
    {
        return IsAutoMode;
    }

    public void StartAutoRolling()
    {
        if (CurrentMainState != MARBLE_GAME_STATE.IDLE)
        {
            return;
        }
        
        if (MarblePage != null)
        {
            MarblePage.StartAutoRolling();
        }
    }

    public void StopAutoRolling()
    {        
        SetAutoMode(false);
        GetBlockTileManager().SetActiveAutoModeUI(true);
        GetBlockTileManager().DisposeWaitMoveBlockSelect();
    }

    public void Release()
    {
        if (CamCntlr != null)
        {
            CamCntlr.Release();
            CamCntlr = null;
        }

        if (MarblePage != null)
        {
            MarblePage.Release();
            MarblePage = null;
        }

        if (PlayerMgr != null)
        {
            PlayerMgr.Release();
            PlayerMgr = null;
        }

        if (DiceMgr != null)
        {
            DiceMgr.Release();
            DiceMgr = null;
        }

        if (BoardGenerator != null)
        {
            BoardGenerator.Release();
            BoardGenerator = null;
        }

        if (MainSM != null)
        {
            MainSM = null;
        }

        if (CamCntlr != null)
        {
            CamCntlr.Release();
            CamCntlr = null;
        }
    }
}
