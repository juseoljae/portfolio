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

using System;
using System.Collections.Generic;
using UnityEngine;
using CharacterControl;
using DG.Tweening;
using UniRx;
using Game.RestAPI;
using System.Linq;

public class CMarblePlayer : MonoBehaviour
{
    private CMarbleWorldManager WorldMgr;
    private CMarblePlayerManager PlayerMgr;
    private CMarbleBlockTileManager BlockTileMgr;
    public CStateMachine<CMarblePlayer> PlayerSM;

    private DicegameInfo GameInfo;

    private CMemberAvatar MemberAvatar;
    private CMemberAvatarController MemberAvatarCntlr;
    
    private GameObject MoveBlockEffObj;

    private MEMBER_TYPE PlayerType;
    private int ExpItemID;

    private float MoveStartClipTime;
    private float MoveClipTime;
    private float MoveFinishClipTime;
    private float SMoveStartClipTime;
    private float SMoveClipTime;
    private float SMoveFinishClipTime;
    private float[] StandClipTimes;

    private float NormalBlocckMoveDist;
    private float ApexBlocckMoveDist;
    private MARBLE_PLAYER_STATE CurrentState;
    
    private GameObject MoveEffectObj;
    private GameObject[] MoveEff_CharTrailObjs;
    private bool bTargetBlockSpecialType;
    private CMarbleAnimController MarbleAnimController;
    private Dictionary<string, AnimationClip> ArriveAnimClips;
    private List<AnimationClip> TouchAnimClips;
    public bool IsPlayingTouchAnim;


    private readonly List<string> ArriveStateKeyNames = new List<string>()
    {
        CMarbleDefine.ANIM_ARRIVESTATE_NAME_REWARD,
        CMarbleDefine.ANIM_ARRIVESTATE_NAME_MOVE,
        CMarbleDefine.ANIM_ARRIVESTATE_NAME_BUFF,
        CMarbleDefine.ANIM_ARRIVESTATE_NAME_DEBUFF
    };
    //{ "reward", "move", "buff", "debuff" };
    private readonly List<string> ArriveClipName = new List<string>()
    {
        CMarbleDefine.ANIMCLIP_NAME_ARRIVE_REWARD,
        CMarbleDefine.ANIMCLIP_NAME_ARRIVE_MOVE,
        CMarbleDefine.ANIMCLIP_NAME_ARRIVE_BUFF,
        CMarbleDefine.ANIMCLIP_NAME_ARRIVE_DEBUFF
    };

    private readonly List<string> TouchClipName = new List<string>()
    {
        CMarbleDefine.ANIMCLIP_NAME_TOUCH_01,
        CMarbleDefine.ANIMCLIP_NAME_TOUCH_02
    };

    
    private readonly List<int> ApexBlockIdxes = new List<int>() 
    { 
        CMarbleDefine.BLOCK_APX_INDEX_1, 
        CMarbleDefine.BLOCK_APX_INDEX_2, 
        CMarbleDefine.BLOCK_APX_INDEX_3, 
        CMarbleDefine.BLOCK_APX_INDEX_4 
    };

    public void Init(CMarblePlayerManager playerManager, CMemberAvatar avatar, DicegameInfo gameInfo)
    {
        PlayerMgr = playerManager;
        WorldMgr = PlayerMgr.WorldMgr;
        BlockTileMgr = WorldMgr.GetBlockTileManager();

        GameInfo = gameInfo;

        MemberAvatar = avatar;

        PlayerSM = new CStateMachine<CMarblePlayer>(this);

        MoveEff_CharTrailObjs = new GameObject[2];

        StandClipTimes = new float[CMarbleDefine.AVATAR_STAND_CLIP_COUNT];

        PlayerType = WorldMgr.GetCurrentMemberType();
        MemberListData memberData = CMemberDataManager.Instance.GetMemberListData(PlayerType);
        if (memberData != null)
        {
            ExpItemID = memberData.diceGameExpItemID;
        }
        //SetPlayer();

        MemberAvatarCntlr = MemberAvatar.GetMemberAvatarController();
        SetAnimatorController();

        SetAnimSwapController();

        //SetAnimationInfo();

        PlayerSM.ChangeState(CMarblePlayerState_Stand.Instance());

        var rot = gameObject.GetComponent<CharacterRotation>();
        if (rot != null)
        {
            rot.enabled = false;
        }

        NormalBlocckMoveDist = (CMarbleDefine.BLOCK_SIZE * 0.5f) * 2;
        ApexBlocckMoveDist = (CMarbleDefine.APEX_SIZE * 0.5f) + (CMarbleDefine.BLOCK_SIZE * 0.5f);

        var resData = CResourceManager.Instance.GetResourceData(CMarbleDefine.EFF_NAME_PLAYER_MOVE);
        if (resData != null)
        {
            GameObject effObj = resData.Load<GameObject>(this.gameObject);
            if (effObj != null)
            {
                MoveEffectObj = Utility.AddChild(this.gameObject, effObj);
                MoveEff_CharTrailObjs[0] = MoveEffectObj.transform.Find("trail_cha")?.gameObject;
                MoveEff_CharTrailObjs[1] = MoveEffectObj.transform.Find("trail_line01")?.gameObject;
                SetActiveMoveEffectObj(false);
                SetActiveMoveEff_CharTrailObjs(false);
            }
        }
    }

    private void SetAnimatorController()
    {
        if (MemberAvatar == null)
        {
            Debug.LogError("CMarblePlayer::SetAnimatorController - MemberAvatar is null.");
            return;
        }

        if (MemberAvatarCntlr != null)
        {
            CRuntimeAnimControllerManager.Instance.ChangeController(MemberAvatarCntlr.animator, ANIM_CONTROLLER_TYPE.MARBLE);
        }
    }

    private void SetAnimSwapController()
    {        
        ArriveAnimClips = new Dictionary<string, AnimationClip>();
        TouchAnimClips = new List<AnimationClip>();
        MarbleAnimController = new CMarbleAnimController();
        MarbleAnimController.Initialize(MemberAvatarCntlr.animator);

        List<string> swapClipName = new List<string>();
        //swapClipName.Add("arrive"); //"arrive" is for swap key name //arrive is not used it didn't control by table
        swapClipName.Add("touch");

        MarbleAnimController.SetSwapClipsBaseName(swapClipName);
        //LoadArriveAnimClips();
        LoadTouchAnimClips();

        GetAnimationLength();
    }
    
    private void LoadArriveAnimClips()
    {
        for (int i = 0; i < ArriveStateKeyNames.Count; i++)
        {
            string name = ArriveStateKeyNames[i];
            string clipName = ArriveClipName[i];
            AnimationClip clip = MemberAvatarCntlr.LoadAnimationClip(clipName);
            if (clip != null)
            {
                ArriveAnimClips.Add(name, clip);
            }
        }
    }

    private void LoadTouchAnimClips()
    {
        foreach (var clipName in TouchClipName)
        {
            AnimationClip clip = MemberAvatarCntlr.LoadAnimationClip(clipName);
            if (clip != null)
            {
                TouchAnimClips.Add(clip);
            }
        }
    }

    private void GetAnimationLength()
    {
        MoveStartClipTime = MarbleAnimController.GetClipTime("move_start");
        MoveClipTime = MarbleAnimController.GetClipTime("move");
        MoveFinishClipTime = MarbleAnimController.GetClipTime("move_end");

        SMoveStartClipTime = MarbleAnimController.GetClipTime("special_move_start");
        SMoveClipTime = MarbleAnimController.GetClipTime("special_move");
        SMoveFinishClipTime = MarbleAnimController.GetClipTime("special_move_end");

        StandClipTimes[0] = MarbleAnimController.GetClipTime("idle_default");
        StandClipTimes[1] = MarbleAnimController.GetClipTime("idle_lookaround");
        StandClipTimes[2] = MarbleAnimController.GetClipTime("idle_yawn");
    }

    public void SwapArriveAnimClip(string name)
    {
        MarbleAnimController.SwapAnimClip(ArriveAnimClips[name], "arrive");
    }

    public int SwapTouchAnimClip()
    {
        int clipIdx = UnityEngine.Random.Range(0, TouchClipName.Count);
        MarbleAnimController.SwapAnimClip(TouchAnimClips[clipIdx], "touch");

        return clipIdx;
    }

    public void RestoreAnimatorController()
    {
        if (MemberAvatar == null)
        {
            Debug.LogError("CMarblePlayer::RestoreAnimatorController - MemberAvatar is null.");
            return;
        }

        if (MemberAvatarCntlr != null)
        {
            CRuntimeAnimControllerManager.Instance.RestoreController(MemberAvatarCntlr.animator);
        }
    }

    public void SetCurPosition()
    {
        int curBlockIndex = GameInfo.tile_last;
        CMarbleBlock curBlock = GetCMarbleBlockByBlockIndex(curBlockIndex);
        if (curBlock != null)
        {
            bool isStartBlock = curBlock.IsBlockType(MARBLE_BLOCK_TYPE.START);
            if (isStartBlock)
            {
                Vector3 pos = new Vector3(0, CMarbleDefine.AVATAR_SPAWN_HEIGHT, 0);
                transform.position = pos;
            }
            else
            {
                transform.position = GetPlayerPosition(curBlock);
            }
            //Debug.Log($"#### SetCurPosition idx : {curBlockIndex}, pos : {transform.position}");
            transform.localRotation = Quaternion.Euler(0, CMarbleDefine.PLAYER_ROTATION_ANGLE * (curBlock.GetCurrentBlockArea() - 1), 0);

            //SetBlockStateByBlockType(curBlock);
        }
    }

    private Vector3 GetPlayerPosition(CMarbleBlock curBlock)
    {
        float offset = 0;//0.5f;
        float offsetX = offset;
        float offsetZ = offset;
        // int blockArea = curBlock.GetCurrentBlockArea();
        // switch (blockArea)
        // {
        //     case CMarbleDefine.BLOCK_AREA_1:
        //         offsetZ = -offsetZ;
        //         break;
        //     case CMarbleDefine.BLOCK_AREA_2:
        //         offsetX = -offset;
        //         break;
        //     case CMarbleDefine.BLOCK_AREA_3:
        //         offsetX = 0;
        //         offsetZ = offset;
        //         break;
        //     case CMarbleDefine.BLOCK_AREA_4:
        //         offsetX = -offsetX;
        //         break;
        // }

        Vector3 blockPos = curBlock.gameObject.transform.position;
        return new Vector3(blockPos.x + offsetX, CMarbleDefine.AVATAR_SPAWN_HEIGHT, blockPos.z + offsetZ);
    }

    public void SetPlayerState(MARBLE_PLAYER_STATE state)
    {
        CurrentState = state;
        
        //if (state != MARBLE_PLAYER_STATE.PLAY)
           // Debug.Log($"#### state CMarblePlayer::SetPlayerState - State : {state}");

        switch (state)
        {
            case MARBLE_PLAYER_STATE.IDLE:
                PlayerSM.ChangeState(CMarblePlayerState_Stand.Instance());
                break;
            case MARBLE_PLAYER_STATE.PLAY:
                PlayerSM.ChangeState(CMarblePlayerState_Play.Instance());
                break;
            case MARBLE_PLAYER_STATE.WAIT:
                PlayerSM.ChangeState(CMarblePlayerState_Wait.Instance());
                break;
            case MARBLE_PLAYER_STATE.FINISH:
                PlayerSM.ChangeState(CMarblePlayerState_Finish.Instance());
                break;
            case MARBLE_PLAYER_STATE.ARRIVE:
                PlayerSM.ChangeState(CMarblePlayerState_Arrive.Instance());
                break;
            case MARBLE_PLAYER_STATE.TOUCHED:
                PlayerSM.ChangeState(CMarblePlayerState_Touched.Instance());
                break;
        }
    }

    public MARBLE_PLAYER_STATE GetPlayerState()
    {
        return CurrentState;
    }

    public CState<CMarblePlayer> GetCurrentState()
    {
        return PlayerSM.GetCurrentState();
    }

    public bool IsTargetBlockSpecialType(int remainCnt)
    {
        int curBlockIndex = BlockTileMgr.GetCurrentBlockIndex();
        
        int targetBlkIdx = (curBlockIndex - 1 + remainCnt) % CMarbleDefine.BLOCK_MAX_COUNT + 1;
        CMarbleBlock targetBlock = GetCMarbleBlockByBlockIndex(targetBlkIdx);
        if (targetBlock != null)
        {
            return targetBlock.IsSpecialBlock();
        }
        //Debug.Log($"#### CMarblePlayer::MoveBlockForward - curBlockIndex : {curBlockIndex}, target block Index : {targetBlockIndex}");
        return false;
    }
    
    public void SetTargetBlockSpecialType(int remainCnt)
    {
        bTargetBlockSpecialType = IsTargetBlockSpecialType(remainCnt);
    }

    public bool GetTargetBlockSpecialType()
    {
        return bTargetBlockSpecialType;
    }

    public void MoveFast(int remainCnt)
    {
        int curBlockIndex = BlockTileMgr.GetCurrentBlockIndex();

        CMarbleBlock curBlock = GetCMarbleBlockByBlockIndex(curBlockIndex);
        if (curBlock == null)
        {
            CDebug.LogError("CMarblePlayer::MoveFast - curBlock is null. Index : " + curBlockIndex);
            return;
        }
        
        List<int> targetBlockIdx = new List<int>();
        
        for (int i = 1; i <= remainCnt; i++)
        {
            int blockIdx = (curBlockIndex - 1 + i) % CMarbleDefine.BLOCK_MAX_COUNT + 1;
            if (ApexBlockIdxes.Contains(blockIdx))
            {
                targetBlockIdx.Add(blockIdx);
            }
        }
        SetActiveMoveEffectObj(true);
        SetActiveMoveEff_CharTrailObjs(true);
        SetAnimationTrigger("move_fast");
        int targetBlockIndex = (curBlockIndex - 1 + remainCnt) % CMarbleDefine.BLOCK_MAX_COUNT + 1;
        MoveFastToBlock(curBlockIndex, targetBlockIndex, targetBlockIdx);
    }

    private void MoveFastToBlock(int curBlockIndex, int targetBlockIndex, List<int> apexBlocksToPass, int apexIdx = 0)
    {
        const float moveSpeed = 0.1f;
        
        if (curBlockIndex == targetBlockIndex)
        {
            WorldMgr.SetIsFastMove(false);
            WorldMgr.SetCurrentBlockIndex(targetBlockIndex);
            SetPlayerState(MARBLE_PLAYER_STATE.FINISH);
            return;
        }

        int nextBlockIndex = (curBlockIndex % CMarbleDefine.BLOCK_MAX_COUNT) + 1;
        
        CMarbleBlock nextBlock = GetCMarbleBlockByBlockIndex(nextBlockIndex);
        if (nextBlock == null)
        {
            CDebug.LogError("CMarblePlayer::MoveFastToBlock - nextBlock is null. Index : " + nextBlockIndex);
            return;
        }

        CMarbleBlock curBlock = GetCMarbleBlockByBlockIndex(curBlockIndex);
        float moveDist = NormalBlocckMoveDist;
        if (curBlock != null)
        {
            ChangeToNoneBlock(curBlock);
            if (IsApexBlockContain(curBlock.gameObject, nextBlock.gameObject))
            {
                moveDist = ApexBlocckMoveDist;
            }
        }

        Vector3 nextPos = GetPlayerPosition(nextBlock);
        transform.DOMove(nextPos, moveSpeed).SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                nextBlock.SetPlayerArrive();
                WorldMgr.SetCurrentBlockIndex(nextBlockIndex);

                if (ApexBlockIdxes.Contains(nextBlockIndex))
                {
                    transform.localRotation *= Quaternion.Euler(0, CMarbleDefine.PLAYER_ROTATION_ANGLE, 0);
                    
                    if (nextBlock.IsBlockType(MARBLE_BLOCK_TYPE.START))
                    {
                        //Debug.Log($"###### MoveFastToBlock MARBLE_BLOCK_TYPE.START");
                        CSoundControl.Instance.EffectPlay(SOUND_ID.SFX_DICEGAME_ONSTART);
                        //Debug.Log($"###### MoveFastToBlock Arrive Start Block! Block Index : {nextBlockIndex}");
                        nextBlock.SetBlockState(MARBLE_BLOCK_STATE.REWARD);
                    }
                    
                    SetRenewal(nextBlockIndex);
                }

                MoveFastToBlock(nextBlockIndex, targetBlockIndex, apexBlocksToPass, apexIdx);
            });
    }

    private int GetBlockDistance(int fromIndex, int toIndex)
    {
        if (toIndex >= fromIndex)
        {
            return toIndex - fromIndex;
        }
        else
        {
            return (CMarbleDefine.BLOCK_MAX_COUNT - fromIndex) + toIndex;
        }
    }

    public void MoveBlockForward(int remainMoveCnt, bool isTargetBlockSppecial)
    {        
        int curBlockIndex = BlockTileMgr.GetCurrentBlockIndex();

        CMarbleBlock curBlock = GetCMarbleBlockByBlockIndex(curBlockIndex);
        if (curBlock == null)
        {
            CDebug.LogError("CMarblePlayer::MoveBlockForward - curBlock is null. Index : " + curBlockIndex);
            return;
        }

        if (remainMoveCnt <= 0)
        {
            SetPlayerState(MARBLE_PLAYER_STATE.FINISH);
            SetRenewal(curBlockIndex);
            return;
        }

        CSoundControl.Instance.EffectPlay(SOUND_ID.SFX_DICEGAME_CHAR_MOVE);

        int nextBlockIndex = BlockTileMgr.GetNextBlockIndex();
        CMarbleBlock nextBlock = GetCMarbleBlockByBlockIndex(nextBlockIndex);
        Vector3 nextTarget = GetPlayerPosition(nextBlock);

        SetActiveMoveEffectObj(true);
        
        string param = CMarbleDefine.ANIM_NAME_PLAYER_MOVE;
        float moveClipTime = MoveClipTime;
        if (isTargetBlockSppecial)
        {
            param = CMarbleDefine.ANIM_NAME_PLAYER_SMOVE;
            moveClipTime = SMoveClipTime;
        }
        SetAnimationTrigger(param);

        ChangeToNoneBlock(curBlock);

        transform.DOMove(nextTarget, moveClipTime).SetEase(Ease.Linear)
                .OnComplete(() =>
                {
                    SetMoveToNextBlock(remainMoveCnt, isTargetBlockSppecial);
                });
    }

    private CMarbleBlock GetCMarbleBlockByBlockIndex(int blockIdx)
    {
        return BlockTileMgr.GetBlockTileByIndexForDicePos(blockIdx);
    }

    private void SetMoveToNextBlock(int remainMoveCnt, bool isTargetBlockSppecial)
    {
        int nextBlockIndex = BlockTileMgr.GetNextBlockIndex();
        CMarbleBlock block = GetCMarbleBlockByBlockIndex(nextBlockIndex);
        if (block != null)
        {
            //block.SetBlockState(MARBLE_BLOCK_STATE.PLAYER_ARRIVE);
            WorldMgr.SetCurrentBlockIndex(nextBlockIndex);
            if (block.gameObject.CompareTag(CMarbleDefine.TAG_APEX_BLOCK))
            {
                if (block.IsBlockType(MARBLE_BLOCK_TYPE.START))
                {
                    //Debug.Log($"###### SetMoveToNextBlock MARBLE_BLOCK_TYPE.START");
                    CSoundControl.Instance.EffectPlay(SOUND_ID.SFX_DICEGAME_ONSTART);
                    block.SetBlockState(MARBLE_BLOCK_STATE.REWARD);//SetActiveBlockEffObj(true);
                }
                transform.localRotation *= Quaternion.Euler(0, CMarbleDefine.PLAYER_ROTATION_ANGLE, 0);

                SetRenewal(nextBlockIndex);
            }
            block.SetPlayerArrive();

            MoveBlockForward(remainMoveCnt - 1, isTargetBlockSppecial);
        }
    }

    private void SetRenewal(int blockIndex)
    {
        if (blockIndex == CMarbleDefine.BLOCK_AREA_1_IDX_START)
        {
            BlockTileMgr.RenewalBlockTileData();
        }
    }

    private float GetMoveDistance(CMarbleBlock curBlock)
    {
        int nextBlockIndex = BlockTileMgr.GetNextBlockIndex();
        CMarbleBlock nextBlock = GetCMarbleBlockByBlockIndex(nextBlockIndex);
        float moveDist = NormalBlocckMoveDist;

        GameObject curBlockObj = curBlock.gameObject;
        GameObject nextBlockObj = nextBlock.gameObject;
        if (IsApexBlockContain(curBlockObj, nextBlockObj))
        {
            moveDist = ApexBlocckMoveDist;
        }

        return moveDist;
    }

    private bool IsApexBlockContain(GameObject curBlockObj, GameObject nextBlockObj)
    {
        if (curBlockObj.CompareTag(CMarbleDefine.TAG_NORMAL_BLOCK) && nextBlockObj.CompareTag(CMarbleDefine.TAG_APEX_BLOCK) ||
            curBlockObj.CompareTag(CMarbleDefine.TAG_APEX_BLOCK) && nextBlockObj.CompareTag(CMarbleDefine.TAG_NORMAL_BLOCK))
        {
            return true;
        }

        return false;
    }

    public void SetMoveFinish()
    {
        bool isTargetBlockSppecial = GetTargetBlockSpecialType();
        float clipTime = MoveFinishClipTime;
        string param = CMarbleDefine.ANIM_NAME_PLAYER_MOVE_FINISH;
        if (isTargetBlockSppecial)
        {
            param = CMarbleDefine.ANIM_NAME_PLAYER_SMOVE_FINISH;
            clipTime = SMoveFinishClipTime;
        }
        SetAnimationTrigger(param);
        SetActiveMoveEffectObj(false);
        

        SetIsRestorePlayer(false);

        SingleAssignmentDisposable resetDisposable = new SingleAssignmentDisposable();
        resetDisposable.Disposable = Observable.Timer(TimeSpan.FromSeconds(clipTime))
                .Subscribe(_ =>
                {
                    int curBlockIndex = BlockTileMgr.GetCurrentBlockIndex();
                    CMarbleBlock curBlock = GetCMarbleBlockByBlockIndex(curBlockIndex);
                    
                    if (curBlock != null)
                    {
                        SetBlockStateByBlockType(curBlock);                        

                        //curBlock.SetActiveBlockEffObj(true);
                    }

                    SetPlayerState(MARBLE_PLAYER_STATE.ARRIVE);

                    resetDisposable.Dispose();
                }).AddTo(this);
    }

    public void SetArrive()
    {
        int curBlockIndex = BlockTileMgr.GetCurrentBlockIndex();
        CMarbleBlock curBlock = GetCMarbleBlockByBlockIndex(curBlockIndex);
        if (curBlock != null)
        {
            SetActiveMoveEff_CharTrailObjs(false);

            string arriveParamName = curBlock.GetArriveParamNameByBlockType();
            SetAnimationTrigger(arriveParamName);

            float arriveClipTime = 2;
            
            string arriveClipKeyName = curBlock.GetArriveClipNameByBlockType();
            arriveClipTime = MarbleAnimController.GetClipTime(arriveClipKeyName);
            if (curBlock.GetBlockType() == MARBLE_BLOCK_TYPE.NONE || BlockTileMgr.IsDeBuffBlock(curBlockIndex))
            {
                arriveClipTime = 0.5f;
            }

            SingleAssignmentDisposable dispose = new SingleAssignmentDisposable();
            dispose.Disposable = Observable.Timer(TimeSpan.FromSeconds(arriveClipTime))
                .Subscribe(_ =>
                {
                    if (WorldMgr.GetMainState() != MARBLE_GAME_STATE.CAM_ACTION_RESTORE)
                    {
                        if (CurrentState != MARBLE_PLAYER_STATE.PLAY)
                        {
                            WorldMgr.SetMainState(MARBLE_GAME_STATE.IDLE);
                        }
                    }
                    
                    dispose.Dispose();
                }).AddTo(this);
        }
    }

    public void SetBlockStateByBlockType(CMarbleBlock curBlock, bool atFirst = false)
    {        
        if (curBlock == null)
            return;

        MARBLE_BLOCK_TYPE blockType = curBlock.GetBlockType();
        //CDebug.Log($"###### SetMoveFinish BlockType : {blockType}");

        switch (blockType)
        {
            case MARBLE_BLOCK_TYPE.REWARD:
                if (!atFirst)
                {
                    //Debug.Log($"###### SetBlockStateByBlockType Arrive reward Block! Block Index : {curBlock.GetBlockIndex()}");
                    curBlock.SetBlockState(MARBLE_BLOCK_STATE.REWARD);

                    curBlock.UpdateExpItemValue(ExpItemID);
                    curBlock.SetActiveBlockEffObj(true);
                }
                break;
            case MARBLE_BLOCK_TYPE.MOVE:
                if (atFirst)
                {
                    DicegameInfo info = CMarbleServerDataManager.Instance.GetGameInfo();
                    if (info != null)
                    {
                        if (info.tile_move_flag == 0) return;
                    }
                }
                BlockTileMgr.SetBlockTileInActive();
                break;
            case MARBLE_BLOCK_TYPE.REWARD_BUFF:
                //curBlock.SetActiveBlockBuffPAObj(true, true);
                BlockTileMgr.SetBuffBlockTile(MARBLE_BLOCK_STATE.BUFF);
                break;
            case MARBLE_BLOCK_TYPE.REWARD_DEBUFF:
                //curBlock.SetActiveBlockBuffPAObj(false, true);
                BlockTileMgr.SetBuffBlockTile(MARBLE_BLOCK_STATE.DEBUFF);
                break;
            default:
                SetPlayerState(MARBLE_PLAYER_STATE.IDLE);
                break;
        }

        curBlock.HideBuffEffObj();
        //WorldMgr.SetMainStateMovingComplete(blockType);
    }

    private void ChangeToNoneBlock(CMarbleBlock block)
    {
        float time = CMarbleDefine.BLOCK_CHANGE_TIME;

        if (BlockTileMgr.GetBlockRotEff() == 1)
        {
            time = CMarbleDefine.BLOCK_CHANGE_HALFTIME;
        }

        SingleAssignmentDisposable noneDisposable = new SingleAssignmentDisposable();
        noneDisposable.Disposable = Observable.Timer(TimeSpan.FromSeconds(time))
                .Subscribe(_ =>
                {
                    block.SetBlockState(MARBLE_BLOCK_STATE.TO_NONE);
                    noneDisposable.Dispose();
                }).AddTo(this);
    }

    public void UpdateStateMachine()
    {
        if (PlayerSM != null)
        {
            PlayerSM.StateMachine_Update();
        }

        if (Input.GetKeyDown(KeyCode.N))
        {
            SetAnimationTrigger(CMarbleDefine.ANIM_NAME_PLAYER_ARRIVE);
        }
    }


    public void SetAnimationTrigger(string param)
    {
        if (MemberAvatarCntlr != null && MemberAvatarCntlr.animator != null)
        {
            MemberAvatarCntlr.animator.SetTrigger(param);
        }
    }

    private void SetActiveMoveEffectObj(bool isActive)
    {
        if (MoveEffectObj != null && MoveEffectObj.activeSelf != isActive)
        {
            MoveEffectObj.SetActive(isActive);
        }
    }

    public void SetActiveMoveEff_CharTrailObjs(bool isActive)
    {
        foreach (var trailObj in MoveEff_CharTrailObjs)
        {
            if (trailObj != null && trailObj.activeSelf != isActive)
            {
                trailObj.SetActive(isActive);
            }
        }
    }


    public CMarbleWorldManager GetWorldManager()
    {
        return WorldMgr;
    }

    public int GetMoveCount()
    {
        return WorldMgr.GetDiceRollResult();
    }

    public float GetMoveStartClipTime()
    {
        return MoveStartClipTime;
    }

    public float GetSMoveStartClipTime()
    {
        return SMoveStartClipTime;
    }

    public float GetStandClipTime(int idx)
    {
        if (StandClipTimes == null)
        {
            Debug.LogError($"CMarblePlayer::GetStandClipTime - Invalid index: {idx}");
            return 1f;
        }
        
        return StandClipTimes[idx];
    }

    public void StopPlayerEffect()
    {        
        if (MemberAvatarCntlr.IsEffectPlaying(EFFECT_TYPE.YAWN))
        {
            MemberAvatarCntlr.StopEffect(EFFECT_TYPE.YAWN);
        }
    }
    
    private Quaternion OriginRotation;
    public void SetPlayerOrgRotation()
    {
        OriginRotation = transform.rotation;
    }
    public void PlayTouchAnim()
    {
        if (IsPlayingTouchAnim || WorldMgr.GetMainState() == MARBLE_GAME_STATE.PLAY)
        {
            return;
        }
        
        if (IsRestorePlayer)
        {
            return;
        }

        IsPlayingTouchAnim = true;
        SetPlayerOrgRotation();
        Vector3 lookTarget = new Vector3(Camera.main.transform.position.x, transform.position.y, Camera.main.transform.position.z);
        transform.DOLookAt(lookTarget, 0.2f).SetEase(Ease.OutQuad).OnComplete(() =>
        {
            int clipIdx = UnityEngine.Random.Range(0, TouchClipName.Count);
            string param = string.Format(CMarbleDefine.ANIM_NAME_PLAYER_INTERACTION, clipIdx);
            SetAnimationTrigger(param);
            
            RestorePlayer(TouchAnimClips[clipIdx].length);
        });
    }

    private bool IsRestorePlayer;
    public void RestorePlayer(float time)
    {        
        if (CurrentState != MARBLE_PLAYER_STATE.TOUCHED)
        {
            return;
        }
        SetIsRestorePlayer(true);
        SingleAssignmentDisposable disposable = new SingleAssignmentDisposable();
        disposable.Disposable = Observable.Timer(TimeSpan.FromSeconds(time))
            .Subscribe(__ =>
            {
                transform.DORotateQuaternion(OriginRotation, 0.2f).SetEase(Ease.OutQuad).OnComplete(()=>
                {
                    SetIsRestorePlayer(false);
                    SetPlayerState(MARBLE_PLAYER_STATE.IDLE);
                    IsPlayingTouchAnim = false;
                });
                
                disposable.Dispose();
            }).AddTo(this);
    }

    public void SetIsRestorePlayer(bool isRestore)
    {
        IsRestorePlayer = isRestore;
    }

    public void Release()
    {
        if (MemberAvatar != null)
        {
            MemberAvatar.Release();
            MemberAvatar = null;
        }

        if (PlayerSM != null)
        {
            PlayerSM = null;
        }

        if (GameInfo != null)
        {
            GameInfo = null;
        }

        if (MemberAvatarCntlr != null)
        {
            MemberAvatarCntlr = null;
        }

        if (MoveEff_CharTrailObjs != null)
        {
            MoveEff_CharTrailObjs = null;
        }

        if (MarbleAnimController != null)
        {
            MarbleAnimController.Release();
            MarbleAnimController = null;
        }

        if (ArriveAnimClips != null)
        {
            ArriveAnimClips.Clear();
            ArriveAnimClips = null;
        }

        if (TouchAnimClips != null)
        {
            TouchAnimClips.Clear();
            TouchAnimClips = null;
        }

        if (StandClipTimes != null)
        {
            StandClipTimes = null;
        }

        CMarblePlayerState_Stand.Instance().loopDisposable?.Dispose();
    }
}
