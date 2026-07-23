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
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UniRx;
using Game.RestAPI;
using System.Globalization;

public class CMarbleBlock : MonoBehaviour
{
    private CMarbleWorldManager WorldMgr;
    private CMarbleBlockTileManager BlockTileMgr;
    private BlockTileInfo BlockInfo;
    private DiceTileData BlockData;
    private DiceTileData BlockData_NonType;
    private CStateMachine<CMarbleBlock> BlockSM;
    private Animator animator;

    [SerializeField] private GameObject Block_TopObj;   //block top layer
    [SerializeField] private GameObject Block_BottomObj;//block bottom layer
    [SerializeField] private GameObject Block_BaseObj;  //block base layer
    [SerializeField] private GameObject Block_TopBottomObj;
    //None Block Type
    [SerializeField] private GameObject Block_NoneObj;
    [SerializeField] private GameObject Block_NoneTopObj;   //none block top layer
    [SerializeField] private GameObject Block_NoneBottomObj;//none block bottom layer
    [SerializeField] private Image RewardIconImage;
    [SerializeField] private TextMeshProUGUI RewardValueText;
    
    [SerializeField] private GameObject RewardUIObj;
    [SerializeField] private GameObject RewardProbObj;
    [SerializeField] private Image ProbIconImage;
    [SerializeField] private GameObject RewardInfoObj;

    
    [SerializeField] private GameObject MoveArrRootObj;
    [SerializeField] private GameObject MoveArrTopObj;
    [SerializeField] private GameObject MoveArrBottomObj;
    [SerializeField] private GameObject MoveArrLeftObj;
    [SerializeField] private GameObject MoveArrRightObj;
    
    [SerializeField] private GameObject BlockEffRootObj;
   
    // ---- Efffect ------------------------------------------------------------------
    private CMarbleBlockReward RewardFXController;
    private GameObject BlockEffObj; //player arrive effect move tile fx, start fx
    private GameObject BlockEffStartBlkObj;
    private GameObject BlockEffMoveBlkObj;
    private float BlockEffPlayTime;
    private float BlockEffStartBlkPlayTime;
    private float BlockEffMoveBlkPlayTime;
    // private float BlockBuffHitEffPlayTime;
    // private float BlockDeBuffHitEffPlayTime;
    private bool IsFinishFlyEffect;

    // ---- Buff/Debuff Effect ----
    private int BuffStartBlockIndex;
    private GameObject BlockBuffEffPAObj; //player arrive to buff block
    private float BlockBuffPAEffPlayTime;
    private GameObject BlockBuffEffObj; //Set on buff effect on target block
    private GameObject BlockBuffFlyEffObj; //fly to target block
    private GameObject BlockBuffHitEffObj; //hit effect after flying
    
    private GameObject BlockDeBuffEffPAObj; //player arrive to buff block
    private float BlockDeBuffPAEffPlayTime;
    private GameObject BlockDeBuffEffObj; //Set on buff effect on target block
    private GameObject BlockDeBuffFlyEffObj; //fly to target block
    private GameObject BlockDeBuffHitEffObj; //hit effect after flying
    //------------------------------------------------------------------------------------ Effect end
    
    private MeshRenderer Block_TopMR;
    private MeshRenderer Block_BottomMR;
    private MeshRenderer Block_BaseMR;
    private MaterialPropertyBlock TopMpb;
    private MaterialPropertyBlock BottomMpb;
    private MaterialPropertyBlock BaseMpb;

    //None Block Type
    private MeshRenderer Block_NoneTopMR;
    private MeshRenderer Block_NoneBottomMR;
    private MaterialPropertyBlock NoneTopMpb;
    private MaterialPropertyBlock NoneBottomMpb;
        
    private int[] ApexBlockIndices;
    private Color BlockOrgColor;
    private Color BlockInactiveColor;

    private float DownClipTime;
    private MARBLE_BLOCK_STATE CurrentState = MARBLE_BLOCK_STATE.IDLE;


    public void Init(CMarbleWorldManager worldMgr, CMarbleBlockTileManager blockTileMgr, DiceTileData blockTileData, BlockTileInfo blockInfos)
    {        
        WorldMgr = worldMgr;
        BlockTileMgr = blockTileMgr;
        SetBlockDatas(blockTileData, blockInfos);

        if (BlockSM == null) BlockSM = new CStateMachine<CMarbleBlock>(this);

        animator = gameObject.GetComponent<Animator>();

        BlockData_NonType = CMarbleDataManager.Instance.GetTileDataByTileType(MARBLE_BLOCK_TYPE.NONE);

        ChangeState(CMarbleBlockState_Idle.Instance());
        
        ApexBlockIndices = new int[]
        {
            CMarbleDefine.BLOCK_APX_INDEX_1,
            CMarbleDefine.BLOCK_APX_INDEX_2,
            CMarbleDefine.BLOCK_APX_INDEX_3,
            CMarbleDefine.BLOCK_APX_INDEX_4,
        };
    
        BlockOrgColor = new Color(1f, 1f, 1f, 1f);
        BlockInactiveColor = new Color(0.5f, 0.5f, 0.5f, 1f);
        
        BuffStartBlockIndex = -1;


        SetBlockInfo();

        SetAnimationInfo();

        if (GetBlockType() == MARBLE_BLOCK_TYPE.NONE)
        {
            HideAllEffectObj();
        }

        //Debug.Log($"#### CMarbleBlock Init Block Index : {GetBlockIndex()}, Type : {GetBlockType()}");
    }

    public void SetBlockDatas(DiceTileData blockTileData, BlockTileInfo blockInfos, bool bRenewal = false)
    {
        DiceTileData copyData = DeepCopy(blockTileData);

        BlockData = copyData;
        BlockInfo = blockInfos;

        
        ImproveRewardValue = 0;
    }

    private DiceTileData DeepCopy(DiceTileData original)
    {
        return new DiceTileData
        {
            TileID = original.TileID,
            TileType = original.TileType,
            IsSpecialTile = original.IsSpecialTile,
            TileSubGroup = original.TileSubGroup,
            TileRewardType = original.TileRewardType,
            TileRewardSubType = original.TileRewardSubType,
            TileRewardMinValue = original.TileRewardMinValue,
            TileRewardMaxValue = original.TileRewardMaxValue,
            TileResPath_Rect_Bottom = original.TileResPath_Rect_Bottom,
            TileResPath_Square_Bottom = original.TileResPath_Square_Bottom,
            TileResPath_Rect_Top = original.TileResPath_Rect_Top,
            TileResPath_Square_Top = original.TileResPath_Square_Top,
            TileIconResPath = original.TileIconResPath,
            ArrivalAniID = original.ArrivalAniID
        };
    }

    public void SetBlockInfo(bool bRenewal = false)    
    {
        MoveArrRootObj.SetActive(false);

        if (!bRenewal) SetRenderer();

        if (!bRenewal) SetNoneBlockTexture();

        SetBlockByBlockData();
        //Debug.Log($"@@@@ Block id: {BlockData.TileID}, Block Type : {BlockData.TileType}, index : {BlockInfo.BlockNumber}");

        if (!bRenewal) SetEffectObj();

        if (bRenewal)
        {
            if (GetBlockType() == MARBLE_BLOCK_TYPE.START)
            {
                SetRewardState();
            }
        }
    }


    
    private void SetRenderer()
    {
        if (BlockData.TileType != MARBLE_BLOCK_TYPE.START)
        {
            if (Block_TopMR == null) Block_TopMR = Block_TopObj.GetComponent<MeshRenderer>();
            if (TopMpb == null) TopMpb = new MaterialPropertyBlock();
            
            if (Block_BottomMR == null) Block_BottomMR = Block_BottomObj.GetComponent<MeshRenderer>();
            if (BottomMpb == null) BottomMpb = new MaterialPropertyBlock();
        
            if (Block_BaseMR == null) Block_BaseMR = Block_BaseObj.GetComponent<MeshRenderer>();
            if (BaseMpb == null) BaseMpb = new MaterialPropertyBlock();         
            
            if (Block_NoneTopMR == null) Block_NoneTopMR  = Block_NoneTopObj.GetComponent<MeshRenderer>();
            if (NoneTopMpb == null) NoneTopMpb = new MaterialPropertyBlock();
            if (Block_NoneBottomMR == null) Block_NoneBottomMR  = Block_NoneBottomObj.GetComponent<MeshRenderer>();
            if (NoneBottomMpb == null) NoneBottomMpb = new MaterialPropertyBlock();   
        }

                
    }

    //blockData use when block renewal
    public void SetBlockByBlockData()
    {
        //Debug.Log($"###### SetBlockByBlockData Block Index : {GetBlockIndex()}, Type : {GetBlockType()}");
        SetUIType();

        if (BlockData.TileType != MARBLE_BLOCK_TYPE.START && BlockData.TileType != MARBLE_BLOCK_TYPE.NONE)
        {
            SetReward();
        }
        
        SetBlockTexture();
    }

    private void SetUIType()
    {
        switch (BlockData.TileType)
        {
            case MARBLE_BLOCK_TYPE.START:            
                SetActiveTopBottomObj(true);
                SetActiveRewardUI(false);
                break;
            case MARBLE_BLOCK_TYPE.NONE:
                SetActiveTopBottomObj(false);
                SetActiveRewardUI(false);
                break;
            case MARBLE_BLOCK_TYPE.MOVE:
            case MARBLE_BLOCK_TYPE.REWARD_DEBUFF:
            case MARBLE_BLOCK_TYPE.REWARD_BUFF:
                SetActiveRewardUI(true);
                SetActiveTopBottomObj(true);
                SetActiveRewardInfo(false);
                break;
            case MARBLE_BLOCK_TYPE.REWARD:
                SetActiveRewardUI(true);
                SetActiveTopBottomObj(true);
                if (BlockData.TileRewardType == REWARD_TYPE.RW_PROB)
                {
                    SetActiveRewardInfo(false);
                }
                else
                {
                    SetActiveRewardInfo(true);
                }
                break;
            default:
                SetActiveTopBottomObj(true);
                SetActiveRewardUI(true);
                SetActiveRewardInfo(true);
                break;
        }
    }

    private void SetEffectObj()
    {
        // switch (GetBlockType())
        // {
        //     case MARBLE_BLOCK_TYPE.START:
        //         SetEffObj(CMarbleDefine.EFFECT_NAME_START_TILE);
        //         break;
        //     case MARBLE_BLOCK_TYPE.REWARD:
        //         SetEffObj(CMarbleDefine.EFFECT_NAME_REWARD);
        //         break;
        //     case MARBLE_BLOCK_TYPE.MOVE:
        //         SetEffObj(CMarbleDefine.EFFECT_NAME_MOVE_TILE);
        //         break;
        // }

        BlockEffStartBlkObj = SetEffObj(CMarbleDefine.EFFECT_NAME_START_TILE);
        BlockEffStartBlkPlayTime = SetBlockEffPlayTime(BlockEffStartBlkObj);
        SetActiveBlockEffStartBlkObj(false);

        BlockEffObj = SetEffObj(CMarbleDefine.EFFECT_NAME_REWARD);//for reward
        BlockEffPlayTime = SetBlockEffPlayTime(BlockEffObj);
        SetActiveBlockEffObj(false);
        
        //if (IsBlockType(MARBLE_BLOCK_TYPE.START) || IsBlockType(MARBLE_BLOCK_TYPE.REWARD))
        {
            SetRewardFxController();                    
        }

        BlockEffMoveBlkObj = SetEffObj(CMarbleDefine.EFFECT_NAME_MOVE_TILE);
        BlockEffMoveBlkPlayTime = SetBlockEffPlayTime(BlockEffMoveBlkObj);
        //SetActiveBlockEffMoveBlkObj(false);

        //every block can be buff/debuff target
        SetBuffEffObj(CMarbleDefine.BUFF_EFFECT_NAME, CMarbleDefine.BUFF_APPLY_EFFECT_NAME, CMarbleDefine.BUFF_MOVE_EFFECT_NAME, CMarbleDefine.BUFF_MOVE_HIT_EFFECT_NAME);
        SetDeBuffEffObj(CMarbleDefine.DEBUFF_EFFECT_NAME, CMarbleDefine.DEBUFF_APPLY_EFFECT_NAME, CMarbleDefine.DEBUFF_MOVE_EFFECT_NAME, CMarbleDefine.DEBUFF_MOVE_HIT_EFFECT_NAME);
    }

    public GameObject SetEffObj(string resPath)
    {
        //if (BlockEffObj == null)
        //{
            GameObject effObj = null;
            GameObject orgEffectObj = BlockTileMgr.GetOrgEffectObj(resPath);
            if (orgEffectObj != null)
            {
                effObj = Utility.AddChild(BlockEffRootObj, orgEffectObj);
                // if (IsBlockType(MARBLE_BLOCK_TYPE.START) || IsBlockType(MARBLE_BLOCK_TYPE.REWARD))
                // {
                //     SetRewardFxController();                    
                // }
                effObj.SetActive(false);
                // BlockEffPlayTime = SetBlockEffPlayTime(effObj);
                // SetActiveBlockEffObj(false);
            }
        //}
        return effObj;
    }

    private void SetEffectPropForStartRewardBlock(GameObject orgEffectObj)
    {        
        BlockEffObj = Utility.AddChild(BlockEffRootObj, orgEffectObj);
        if (IsBlockType(MARBLE_BLOCK_TYPE.START) || IsBlockType(MARBLE_BLOCK_TYPE.REWARD))
        {
            SetRewardFxController();                    
        }

        BlockEffPlayTime = SetBlockEffPlayTime(BlockEffObj);
        SetActiveBlockEffObj(false);
    }

    public void ReleaseEffectObj()
    {
        BuffStartBlockIndex = -1;
        BlockEffObj?.SetActive(false);
        BlockEffStartBlkObj?.SetActive(false);
        BlockEffMoveBlkObj?.SetActive(false);

        // Destroy(BlockEffObj);        
        // BlockEffObj = null;
        // RewardFXController = null;
    }
    

    public void SetBuffEffObj(string arrEffPath, string applyEffPath, string flyEffPath, string hitEffPath)
    {
        if (BlockBuffEffPAObj == null)
        {
            BlockBuffEffPAObj  = LoadBuffEffObj(arrEffPath, true, true);
            SetActiveBlockBuffPAObj(true, false);
        }

        if (BlockBuffEffObj == null)
        {
            BlockBuffEffObj = LoadBuffEffObj(applyEffPath, true);
            SetActiveBlockBuffEffObj(true, false);
        }

        if (BlockBuffFlyEffObj == null)
        {
            BlockBuffFlyEffObj = LoadBuffEffObj(flyEffPath, true);
            SetActiveBlockBuffFlyEffObj(true, false);
        }

        if (BlockBuffHitEffObj == null)
        {
            BlockBuffHitEffObj = LoadBuffEffObj(hitEffPath, true);
            SetActiveBlockBuffHitEffObj(true, false);
        }
    }
    
    public void SetDeBuffEffObj(string arrEffPath, string applyEffPath, string flyEffPath, string hitEffPath)
    {
        if (BlockDeBuffEffPAObj == null)
        {
            BlockDeBuffEffPAObj  = LoadBuffEffObj(arrEffPath, false, true);
            SetActiveBlockBuffPAObj(false, false);
        }

        if (BlockDeBuffEffObj == null)
        {
            BlockDeBuffEffObj = LoadBuffEffObj(applyEffPath, false);
            SetActiveBlockBuffEffObj(false, false);
        }

        if (BlockDeBuffFlyEffObj == null)
        {
            BlockDeBuffFlyEffObj = LoadBuffEffObj(flyEffPath, false);
            SetActiveBlockBuffFlyEffObj(false, false);
        }

        if (BlockDeBuffHitEffObj == null)
        {
            BlockDeBuffHitEffObj = LoadBuffEffObj(hitEffPath, false);
            SetActiveBlockBuffHitEffObj(false, false);
        }
    }

    public GameObject LoadEffectObj(GameObject parent, string resPath)
    {
        var resData = CResourceManager.Instance.GetResourceData(resPath);
        if (resData != null)
        {
            GameObject effObj = resData.Load<GameObject>(this.gameObject);
            if (effObj != null)
            {
                return Utility.AddChild(parent, effObj);
            }
        }

        return null;
    }

    private GameObject LoadBuffEffObj(string resPath, bool isBuff, bool isPAEff = false)
    {
        GameObject buffObj = LoadEffectObj(BlockEffRootObj, resPath);
        if (buffObj != null)
        {
            if (isPAEff)
            {
                if (isBuff)
                    BlockBuffPAEffPlayTime = SetBlockEffPlayTime(buffObj);   
                else
                    BlockDeBuffPAEffPlayTime = SetBlockEffPlayTime(buffObj);   
            }
                                     
        } 

        return buffObj;
    }

    public void SetRewardFxController()
    {
        GameObject effObj = GetBlockType() == MARBLE_BLOCK_TYPE.START ? BlockEffStartBlkObj : BlockEffObj;
        //if (BlockEffObj != null)
        if (RewardFXController == null)
        {
            RewardFXController = effObj.GetComponentInChildren<CMarbleBlockReward>();
            if (RewardFXController != null)
            {
                RewardFXController.Init(BlockTileMgr.GetWorldManager());
            }
        }
    }

    public void SetBuffStartBlockIndex(int startBlockIndex)
    {
        BuffStartBlockIndex = startBlockIndex;
    }

    private float SetBlockEffPlayTime(GameObject effObj)
    {
        ParticleSystem ps = effObj.GetComponent<ParticleSystem>();
        if (ps != null)
        {
             return ps.main.duration;
        }

        return 1f;
    }

    public void SetActiveRewardUI(bool bActive)
    {
        RewardUIObj.SetActive(bActive);
    }

    public void SetActiveTopBottomObj(bool bActive)
    {
        Block_TopBottomObj.SetActive(bActive);
        Block_NoneObj.SetActive(!bActive);
    }

    public void SetActiveBlockEffObj(bool bActive)
    {
        if (BlockEffObj != null)
        {
            BlockEffObj.SetActive(bActive);

            if (bActive)
            {
                SingleAssignmentDisposable _effDisposer = new SingleAssignmentDisposable();
                _effDisposer.Disposable = Observable.Timer(TimeSpan.FromSeconds(BlockEffPlayTime))
                    .Subscribe(_ =>
                    {
                        SetActiveBlockEffObj(false);
                        _effDisposer.Dispose();
                    }).AddTo(this);
            }
        }
    }

    public void SetActiveBlockEffStartBlkObj(bool bActive)
    {
        if (BlockEffStartBlkObj != null)
        {
            BlockEffStartBlkObj.SetActive(bActive);

            if (bActive)
            {
                SingleAssignmentDisposable _effDisposer = new SingleAssignmentDisposable();
                _effDisposer.Disposable = Observable.Timer(TimeSpan.FromSeconds(BlockEffStartBlkPlayTime))
                    .Subscribe(_ =>
                    {
                        SetActiveBlockEffStartBlkObj(false);
                        _effDisposer.Dispose();
                    }).AddTo(this);
            }
        }
    }

    public void SetActiveBlockBuffPAObj(bool isBuff, bool bActive)
    {
        GameObject buffObj = isBuff ? BlockBuffEffPAObj : BlockDeBuffEffPAObj;
        float playTime = isBuff ? BlockBuffPAEffPlayTime : BlockDeBuffPAEffPlayTime;

        if (buffObj != null)
        {
            buffObj.SetActive(bActive);

            if (bActive)
            {
                SingleAssignmentDisposable _effDisposer = new SingleAssignmentDisposable();
                _effDisposer.Disposable = Observable.Timer(TimeSpan.FromSeconds(playTime))
                    .Subscribe(_ =>
                    {
                        SetActiveBlockBuffPAObj(isBuff, false);

                        _effDisposer.Dispose();
                    }).AddTo(this);
            }
        }
    }

    public void SetActiveBlockBuffEffObj(bool isBuff, bool bActive)
    {
        if (GetBlockType() == MARBLE_BLOCK_TYPE.NONE)
        {
            bActive = false;
        }

        if (bActive)
        {
            GameObject oppositeObj = isBuff ? BlockDeBuffEffObj : BlockBuffEffObj;
            if (oppositeObj != null && oppositeObj.activeSelf)
            {
                oppositeObj.SetActive(false);
            }
        }

        GameObject buffObj = isBuff ? BlockBuffEffObj : BlockDeBuffEffObj;
        if (buffObj != null)
        {
            buffObj.SetActive(bActive);
        }
    }

    public void HideBuffEffObj()
    {
        SetActiveBlockBuffEffObj(true, false);
        SetActiveBlockBuffEffObj(false, false);
    }

    public void SetActiveBlockBuffFlyEffObj(bool isBuff, bool bActive)
    {
        GameObject buffObj = isBuff ? BlockBuffFlyEffObj : BlockDeBuffFlyEffObj;
        if (buffObj != null)
        {
            buffObj.SetActive(bActive);
        }
    }

    public void SetActiveBlockBuffHitEffObj(bool isBuff, bool bActive)
    {
        GameObject buffObj = isBuff ? BlockBuffHitEffObj : BlockDeBuffHitEffObj;
        if (buffObj != null)
        {
            buffObj.SetActive(bActive);
        }
    }

    public void FlyBlockFlyBuffEffObj(bool isBuff)
    {
        if (IsBlockType(MARBLE_BLOCK_TYPE.NONE))
            return;

        SetActiveBlockBuffFlyEffObj(isBuff, true);
        //Debug.Log($"###### FlyBlockFlyBuffEffObj Fly Start, BuffStartBlockIndex : {BuffStartBlockIndex}, Block Index : {GetBlockIndex()}");
        if (BuffStartBlockIndex > 0)
        {
            CMarbleBlock startBlock = BlockTileMgr.GetBlockTileByIndexForDicePos(BuffStartBlockIndex);
            //Debug.Log($"###### FlyBlockFlyBuffEffObj Start Block startBlock : {startBlock}, Target Block Pos : {this.transform.position}");
            if (startBlock != null)
            {
                Vector3 startPos = startBlock.transform.position;
                Vector3 targetPos = this.transform.position;
                GameObject buffObj = isBuff ? BlockBuffFlyEffObj : BlockDeBuffFlyEffObj;
                // if (buffObj == null)
                // {
                //     Debug.LogError($"#### FlyBlockFlyBuffEffObj buffObj is null isBuff : {isBuff}");
                // }
                buffObj.transform.position = startPos;
                buffObj.transform.DOJump(targetPos, 20f, 1, CMarbleDefine.BLOCK_BUFF_FLYING_TIME)
                        .SetEase(Ease.OutQuad) 
                        .OnComplete(() => 
                        {
                            SetActiveBlockBuffFlyEffObj(isBuff, false);
                            SetActiveBlockBuffEffObj(isBuff, true);
                            SetBlockRewardValueText();

                            SetActiveBlockBuffHitEffObj(isBuff, true);

                            //float hitEffTime = GetHitEffectPlayTime(isBuff);
                            SingleAssignmentDisposable finishDisposable = new SingleAssignmentDisposable();
                            finishDisposable.Disposable = Observable.Timer(TimeSpan.FromSeconds(1))
                                .Subscribe(_ =>
                                {
                                    SetActiveBlockBuffHitEffObj(isBuff, false);
                                    IsFinishFlyEffect = true;
                                    BlockTileMgr.OnFlyEffectComplete();
                                    finishDisposable.Dispose();
                                }).AddTo(this);
                        });
            }
        }
    }

    public void SetFinishBuffFly()
    {
        WorldMgr.SetMainState(MARBLE_GAME_STATE.IDLE);
    }

    public bool IsFinishFlyEffectObj()
    {
        return IsFinishFlyEffect;
    }

    private bool IsApexBlock(int blockIndex)
    {
        for (int i=0; i<ApexBlockIndices.Length; i++)
        {
            if (blockIndex == ApexBlockIndices[i])
            {
                return true;
            }
        }
        return false;
    }
    
    public void SetBlockTexture()
    {
        if (IsBlockType(MARBLE_BLOCK_TYPE.START))
        {
            return;
        }

        bool isApexBlock = IsApexBlock(GetBlockIndex());

        SetBlockTexture(Block_TopMR, TopMpb, isApexBlock ? BlockData.TileResPath_Square_Top : BlockData.TileResPath_Rect_Top);
        SetBlockTexture(Block_BottomMR, BottomMpb, isApexBlock ? BlockData.TileResPath_Square_Bottom : BlockData.TileResPath_Rect_Bottom);
    }

    public void SetNoneBlockTexture()
    {
        if (IsBlockType(MARBLE_BLOCK_TYPE.START))
        {
            return;
        }

        bool isApexBlock = IsApexBlock(GetBlockIndex());
        
        SetBlockTexture(Block_NoneTopMR, NoneTopMpb, isApexBlock ? BlockData_NonType.TileResPath_Square_Top : BlockData_NonType.TileResPath_Rect_Top);
        SetBlockTexture(Block_NoneBottomMR, NoneBottomMpb, isApexBlock ? BlockData_NonType.TileResPath_Square_Bottom : BlockData_NonType.TileResPath_Rect_Bottom);
    }

    public void SetActiveNoneBlock(bool bActive)
    {
        if (IsBlockType(MARBLE_BLOCK_TYPE.START))
        {
            return;
        }

        HideAllEffectObj();

        if (BlockTileMgr.GetBlockRotEff() == 1)
        {
            float angle = CMarbleDefine.BLOCK_ROTATION_ANGLE;
            if (GetBlockIndex() >= CMarbleDefine.BLOCK_APX_INDEX_2 && GetBlockIndex() < CMarbleDefine.BLOCK_APX_INDEX_4)
            {
                angle = -CMarbleDefine.BLOCK_ROTATION_ANGLE;
            }

            Vector3 rot = new Vector3(0, 0, angle);
            if (GetBlockIndex() == CMarbleDefine.BLOCK_APX_INDEX_3)
            {
                rot = new Vector3(angle, 0, 0);
            }
            transform.DOLocalRotate(rot, 0.2f, RotateMode.FastBeyond360)
                    .SetRelative(true)
                    .SetEase(Ease.OutQuad)
                    .OnComplete(() =>
                    {
                        transform.localEulerAngles = new Vector3(0, transform.localEulerAngles.y, 0);
                        if (!bActive)
                        {
                            SetBlockState(MARBLE_BLOCK_STATE.IDLE);
                        }
                    });
            
            SingleAssignmentDisposable noneDisposable = new SingleAssignmentDisposable();
            noneDisposable.Disposable = Observable.Timer(TimeSpan.FromSeconds(0.1f))
                    .Subscribe(_ =>
                    {
                        SetActiveTopBottomObj(!bActive);
                        noneDisposable.Dispose();
                    }).AddTo(this); 
        }
        else
        {
            SetActiveTopBottomObj(!bActive);
        }

    }

    private void HideAllEffectObj()
    {
        SetActiveBlockEffObj(false);

        SetActiveBlockBuffPAObj(true, false);
        SetActiveBlockBuffPAObj(false, false);

        SetActiveBlockBuffEffObj(true, false);
        SetActiveBlockBuffEffObj(false, false);

        SetActiveBlockBuffFlyEffObj(true, false);
        SetActiveBlockBuffFlyEffObj(false, false);
    }

    private void SetBlockTexture(Renderer renderer, MaterialPropertyBlock mpb, string texResPath)
    {
        var resData = CResourceManager.Instance.GetResourceData(texResPath);
        if (resData != null)
        {
            Texture tex = resData.LoadTexture(gameObject) as Texture;

            if (tex != null && mpb != null)
            {
                renderer.GetPropertyBlock(mpb);

                mpb.SetTexture("_BaseMap", tex);
                mpb.SetColor("_BaseColor", Color.white);

                renderer.SetPropertyBlock(mpb); 
            }           
        }
    }

    public void SetAnimationInfo()
    {
        if (animator != null)
        {
            string downClipName = "down";

            AnimationClip[] _clips = animator.runtimeAnimatorController.animationClips;
            foreach (AnimationClip clip in _clips)
            {
                string clipName = clip.name;

                if (clipName.Contains(downClipName))
                {
                    DownClipTime = clip.length;
                }
            }        
        }
    }

    private string GetRewardValue()
    {
        string rewardTxt = string.Empty;

        int blockIdx = GetBlockIndex();
        float buffVal = 0;

        if (BlockTileMgr.IsBuffBlock(blockIdx))
        {
            buffVal = (int)CAccountConfigDataManager.Instance.GetGameConfigData("dice_tile_buff_per").value;
        }
        else if (BlockTileMgr.IsDeBuffBlock(blockIdx))
        {
            buffVal = (int)CAccountConfigDataManager.Instance.GetGameConfigData("dice_tile_debuff_per").value * (-1);
        }

        if (long.TryParse(BlockInfo.RewardValue, out long value))
        { 
            float buffCalc = (buffVal / 10000f);
            float improveValue = (1 + (ImproveRewardValue * 0.0001f));
            float rewardValue = value * (1 + buffCalc) * improveValue;
            rewardTxt = FormatReward(rewardValue);
        }

        return rewardTxt;
    }

    private RewardList GetRewardListByOrder(int idx)
    {
        List<RewardList> rwValue = CMarbleServerDataManager.Instance.GetRewardInfos();
        if (rwValue == null || rwValue.Count == 0)
            return null;

        if (!IsContainStartBlockSalary())
            return rwValue[0];

        int targetOrder = (idx == CMarbleDefine.BLOCK_APX_INDEX_1) ? 1 : 2;
        foreach (RewardList reward in rwValue)
        {
            if (reward.order == targetOrder)
                return reward;
        }

        return null;
    }

    private string GetRollReward()
    {
        string rewardTxt = string.Empty;
        
        int blockIdx = GetBlockIndex();

        RewardList reward = GetRewardListByOrder(blockIdx);
        if (reward != null)
        {
            rewardTxt = FormatReward(reward.value);
        }
        
        return rewardTxt;
    }

    public void SetRewardState()
    {
        //Debug.Log($"###### SetRewardState Block Index : {GetBlockIndex()}, RewardFXController : {RewardFXController}, Reward Value : {BlockInfo.RewardValue}, ImproveRewardValue : {ImproveRewardValue}");
        
        if (RewardFXController != null)
        {
            // if (GetBlockIndex() == CMarbleDefine.BLOCK_APX_INDEX_1)
            // {
            //     Debug.Log($"###### SetRewardState after START Block");
            // }
            // Debug.Log($"###### SetRewardState after Block Index : {GetBlockIndex()}");
            //set icon
            string iconPath = GetRewardIconResPath();
            RewardFXController.SetIconImage(iconPath);
            //set value
            string rewardTxt = GetRollReward();//GetRewardValue();
            RewardFXController.SetValueText(rewardTxt);
        }
        
        if (GetBlockType() == MARBLE_BLOCK_TYPE.START)
        {
            SetActiveBlockEffStartBlkObj(true);
        }
        else
        {
            SetActiveBlockEffObj(true);            
        }

        WorldMgr.RefreshExpItemInfo();
    }

    public REWARD_TYPE GetBlockRewardType()
    {
        return BlockData.TileRewardType;
    }

    public int GetBlockRewardSubType()
    {
        return BlockData.TileRewardSubType;
    }

    public void UpdateExpItemValue(int itemID)
    {
        if (GetBlockRewardType() == REWARD_TYPE.RW_ITEM)
        {
            if (GetBlockRewardSubType() == itemID)
            {
                WorldMgr.UpdateExpItemValue(itemID);
            }
        }
    }

    public void SetBlockActive()
    {    
        isSelectingBlock = false;
        // onle use none block
        ChangeBlockMaterialColor(Block_NoneTopMR, NoneTopMpb, BlockInactiveColor, BlockOrgColor);
        ChangeBlockMaterialColor(Block_NoneBottomMR, NoneBottomMpb, BlockInactiveColor, BlockOrgColor);
        
        SetHideArrow();

        SetBlockStateIdleAfter();
    }

    public void SetBlockInActive()
    {
        if (IsBlockType(MARBLE_BLOCK_TYPE.START))
        {
            return;
        }

        // onle use none block
        ChangeBlockMaterialColor(Block_NoneTopMR, NoneTopMpb, BlockOrgColor, BlockInactiveColor);
        ChangeBlockMaterialColor(Block_NoneBottomMR, NoneBottomMpb, BlockOrgColor, BlockInactiveColor);

        SetBlockTypeNONE();

        SetBlockStateIdleAfter();
    }

    private void ChangeBlockMaterialColor(Renderer renderer, MaterialPropertyBlock targetMB, Color startColor, Color targetColor, float duration = 0.5f)
    {
        if (renderer == null || targetMB == null) return;

        renderer.GetPropertyBlock(targetMB);

        DOVirtual.Float(0, 1, duration, (changeValue) =>
        {
            Color lerpedColor = Color.Lerp(startColor, targetColor, changeValue);

            targetMB.SetColor("_BaseColor", lerpedColor);
            renderer.SetPropertyBlock(targetMB);

        }).SetEase(Ease.Linear)
        .SetTarget(renderer);
    }
    
    public void SetPlayerArrive()
    {
        SetAnimationTrigger(CMarbleDefine.ANIM_NAME_BLOCK_WORK);
        SingleAssignmentDisposable resetDisposable = new SingleAssignmentDisposable();
        resetDisposable.Disposable = Observable.Timer(TimeSpan.FromSeconds(1f))
                    .Subscribe(_ =>
                    {
                        SetBlockState(MARBLE_BLOCK_STATE.IDLE);
                        resetDisposable.Dispose();
                    }).AddTo(this);
    }

    private void SetReward()
    {      
        string iconPath = GetIconResPath();
        if (!iconPath.IsNullOrEmpty())
        {
            CResourceManager.Instance.LoadImage(iconPath, RewardIconImage);
        }
        else
        {
            CDebug.LogError($"CMarbleBlock::SetReward - Invalid reward icon path for Block ID: {BlockData.TileID}, Block rwd Type : {BlockData.TileRewardType}, Block subType index: {BlockData.TileRewardSubType}");
        }

        iconPath = BlockData.TileIconResPath;
        if (!iconPath.IsNullOrEmpty())
        {
            
            CResourceManager.Instance.LoadImage(iconPath, ProbIconImage);
        }
        else
        {
            CDebug.LogError($"CMarbleBlock::SetReward - Invalid reward prob icon path for Block ID: {BlockData.TileID}, Block rwd Type : {BlockData.TileRewardType}, Block subType index: {BlockData.TileRewardSubType}");
        }

        SetBlockRewardValueText();
    }

    private void SetBlockRewardValueText()
    {
        string rewardTxt = GetRewardValue();
        if (RewardValueText != null)
        {
            RewardValueText.text = rewardTxt;
        }
    }

    private string GetIconResPath()
    {
        string iconPath = string.Empty;
        if (!BlockData.TileIconResPath.Equals("0"))
        {
            iconPath = BlockData.TileIconResPath;
        }
        else
        {
            iconPath = BlockData.TileRewardType.GetRewardResourceIcon(BlockData.TileRewardSubType);
        }
        return iconPath;
    }

    private bool IsContainStartBlockSalary()
    {
        bool isSalaryReward = false;
        
        DicegameInfo gameInfo = CMarbleServerDataManager.Instance.GetGameInfo();
        if (gameInfo != null)
        {
            if (gameInfo.reset_start_flag == 1)
            {
                isSalaryReward = true;
            }
        }
        
        return isSalaryReward;
    }

    private string GetRewardIconResPath()
    {
        List<RewardList> rewardInfos = CMarbleServerDataManager.Instance.GetRewardInfos();

        if (rewardInfos == null || rewardInfos.Count == 0)
            return string.Empty;

        bool isSalaryReward = IsContainStartBlockSalary();
        bool isStartBlock = IsBlockType(MARBLE_BLOCK_TYPE.START);

        int targetIndex = 0;
        if (!isStartBlock && isSalaryReward)
        {
            targetIndex = 1;
        }

        if (targetIndex < rewardInfos.Count)
        {
            return GetRewardIconResPathByType(rewardInfos[targetIndex]);
        }
        else
        {
            return GetRewardIconResPathByType(rewardInfos[0]);
        }
    }


    private string GetRewardIconResPathByType(RewardList reward)
    {
        string iconPath = string.Empty;

        REWARD_TYPE rewardType = reward.type.ToEnum<REWARD_TYPE>();
        if (RewardFXController != null)
        {
            RewardFXController.SetRewardType(rewardType, reward.sub);
        }
        
        switch (rewardType)
        {
            case REWARD_TYPE.RW_CASH:
                iconPath = REWARD_TYPE.RW_CASH.GetRewardResourceIcon();
                break;
            case REWARD_TYPE.RW_GMONEY:
                iconPath = REWARD_TYPE.RW_GMONEY.GetRewardResourceIcon();
                break;
            case REWARD_TYPE.RW_ITEM:
            case REWARD_TYPE.RW_AP:
                iconPath = rewardType.GetRewardResourceIcon(reward.sub);
                break;
            case REWARD_TYPE.RW_STICKER:
                iconPath = rewardType.GetRewardResourceIcon(reward.sub);

                
                EnforceRecvEff(reward);
                break;
        }

        return iconPath;
    }
    
    public void EnforceRecvEff(RewardList reward)
    {
        if(reward == null)
        {
            CDebug.LogError("EnforceRecvEff() reward_list is null");
            return;
        }
        if (reward != null)
        {
            GameObject effectStartObj = Block_TopObj != null ? Block_TopObj : this.gameObject;
            EffectLayer.ShowEffect(effectStartObj, reward, false);
        }
    }

    #region MyRegion

    public string FormatReward(float value)
    {
        if (value <= 0f) return "0";

        value = MathF.Ceiling(value);

        // 문서: 1 <= 보상 < 1,000 은 평범한 숫자 단위(정수)
        if (value < 1000f)
            return MathF.Floor(value).ToString("0", CultureInfo.InvariantCulture);

        float unit;
        string suffix;

        if (value < 1_000_000f)
        {
            unit = 1_000f;
            suffix = "K";
        }
        else if (value < 1_000_000_000f)
        {
            unit = 1_000_000f;
            suffix = "M";
        }
        else
        {
            unit = 1_000_000_000f;
            suffix = "T";
        }

        float shortValue = value / unit;

        int maxDecimals =
            shortValue < 10f ? 2 :
            shortValue < 100f ? 1 :
            0;

        // 절삭(Truncate) 적용: 반올림으로 다음 구간/단위로 튀는 걸 방지
        float truncated = Truncate(shortValue, maxDecimals);

        // maxDecimals만큼 고정 소수로 찍고, 뒤의 0과 '.' 제거해서 "최대 n자리"를 만족
        string s = truncated.ToString($"F{maxDecimals}", CultureInfo.InvariantCulture)
            .TrimEnd('0')
            .TrimEnd('.');

        return s + suffix;
    }

    private static float Truncate(float v, int decimals)
    {
        if (decimals <= 0) return MathF.Floor(v);

        float p = MathF.Pow(10f, decimals);
        return MathF.Floor(v * p) / p;
    }

    #endregion

    public string FormatReward_(float value)
    {
        if (value < 1000f)
            return value.ToString("F0");

        float unit;
        string suffix;

        if (value < 1_000_000f)
        {
            unit = 1_000f;
            suffix = "K";
        }
        else if (value < 1_000_000_000f)
        {
            unit = 1_000_000f;
            suffix = "M";
        }
        else
        {
            unit = 1_000_000_000f;
            suffix = "T";
        }

        float shortValue = value / unit;

        if (shortValue < 10f)
        {
            return $"{shortValue:0.##}{suffix}";
        }
        else if (shortValue < 100f)
        {
            return $"{shortValue:0.#}{suffix}";
        }
        else
        {
            return $"{shortValue:0}{suffix}";
        }
    }

    private void SetActiveRewardInfo(bool bActive)
    {        
        if (RewardProbObj == null || RewardInfoObj == null) return;

        RewardProbObj.SetActive(!bActive);
        RewardInfoObj.SetActive(bActive);
    }


    public void SetAnimationTrigger(string param)
    {
        if (animator != null)
        {
            animator.SetTrigger(param);
        }
    }

    public int GetBlockIndex()
    {
        if (BlockInfo != null)
        {
            return BlockInfo.BlockNumber;
        }
        return -1;
    }

    public MARBLE_BLOCK_TYPE GetBlockType()
    {
        if (BlockData != null)
        {
            return BlockData.TileType;
        }

        return MARBLE_BLOCK_TYPE.NONE;
    }

    public bool IsBlockType(MARBLE_BLOCK_TYPE type)
    {
        if (BlockData != null)
        {
            return BlockData.TileType == type;
        }
        return false;
    }

    public void SetBlockTypeNONE()
    {
        BlockData.TileType = MARBLE_BLOCK_TYPE.NONE;
    }

    public void SetActiveArrow()
    {
        if (IsBlockType(MARBLE_BLOCK_TYPE.MOVE))
        {
            return;
        }
        
        MoveArrRootObj.SetActive(true);

        switch (GetBlockIndex())
        {
            case CMarbleDefine.BLOCK_APX_INDEX_1:
                SetActiveRewardUI(true);
                MoveArrTopObj.SetActive(false);
                MoveArrBottomObj.SetActive(true);
                MoveArrLeftObj.SetActive(false);
                MoveArrRightObj.SetActive(false);
                break;
            case CMarbleDefine.BLOCK_APX_INDEX_2:
                MoveArrTopObj.SetActive(false);
                MoveArrBottomObj.SetActive(false);
                MoveArrLeftObj.SetActive(true);
                MoveArrRightObj.SetActive(false);
                break;
            case CMarbleDefine.BLOCK_APX_INDEX_3:
                MoveArrTopObj.SetActive(true);
                MoveArrBottomObj.SetActive(false);
                MoveArrLeftObj.SetActive(false);
                MoveArrRightObj.SetActive(false);
                break;
            case CMarbleDefine.BLOCK_APX_INDEX_4:
                MoveArrTopObj.SetActive(false);
                MoveArrBottomObj.SetActive(false);
                MoveArrLeftObj.SetActive(false);
                MoveArrRightObj.SetActive(true);
                break;
            default:
                int area = GetCurrentBlockArea();
                switch (area)
                {
                    case CMarbleDefine.BLOCK_AREA_1:
                    case CMarbleDefine.BLOCK_AREA_4:
                        MoveArrTopObj.SetActive(false);
                        MoveArrBottomObj.SetActive(true);
                        break;
                    case CMarbleDefine.BLOCK_AREA_2:
                    case CMarbleDefine.BLOCK_AREA_3:
                        MoveArrTopObj.SetActive(true);
                        MoveArrBottomObj.SetActive(false);
                        break;
                }
                break;
        }
    }

    private void SetHideArrow()
    {
        if (MoveArrRootObj != null) MoveArrRootObj.SetActive(false);
        if (MoveArrTopObj != null) MoveArrTopObj.SetActive(false);
        if (MoveArrBottomObj != null) MoveArrBottomObj.SetActive(false);
        if (MoveArrLeftObj != null) MoveArrLeftObj.SetActive(false);
        if (MoveArrRightObj != null) MoveArrRightObj.SetActive(false);
    }



    #region State_Machine
    
    public void SetBlockState(MARBLE_BLOCK_STATE state)
    {
        CurrentState = state;

        // if (GetBlockIndex() == BlockTileMgr.GetCurrentBlockIndex())
        //     Debug.Log($"#### state CMarbleBlock::SetBlockState - State : {state}");

        switch (state)
        {
            case MARBLE_BLOCK_STATE.IDLE:
                BlockSM.ChangeState(CMarbleBlockState_Idle.Instance());
                break;
            case MARBLE_BLOCK_STATE.PLAYER_ARRIVE:
                BlockSM.ChangeState(CMarbleBlockStat_PlayerArrive.Instance());
                break;
            case MARBLE_BLOCK_STATE.TO_NONE:
                BlockSM.ChangeState(CMarbleBlockStat_ChangeNone.Instance());
                break;
            case MARBLE_BLOCK_STATE.RENEWAL:
                BlockSM.ChangeState(CMarbleBlockStat_Renewal.Instance());
                break;
            case MARBLE_BLOCK_STATE.ACTIVE:
                BlockSM.ChangeState(CMarbleBlockStat_Active.Instance());
                break;
            case MARBLE_BLOCK_STATE.INACTIVE:
                BlockSM.ChangeState(CMarbleBlockStat_InAcive.Instance());
                break;
            case MARBLE_BLOCK_STATE.TOUCHABLE:
                BlockSM.ChangeState(CMarbleBlockStat_Touchable.Instance());
                break;
            case MARBLE_BLOCK_STATE.REWARD:
                BlockSM.ChangeState(CMarbleBlockStat_Reward.Instance());
                break;
            case MARBLE_BLOCK_STATE.BUFF:
                BlockSM.ChangeState(CMarbleBlockStat_Buff.Instance());
                break;
            case MARBLE_BLOCK_STATE.DEBUFF:
                BlockSM.ChangeState(CMarbleBlockStat_DeBuff.Instance());
                break;
        }
    }

    public MARBLE_BLOCK_STATE GetBlockState()
    {
        return CurrentState;
    }

    public void SetBlockStateIdleAfter()
    {
        SingleAssignmentDisposable resetDisposable = new SingleAssignmentDisposable();
        resetDisposable.Disposable = Observable.Timer(TimeSpan.FromSeconds(0.5f))
                  .Subscribe(_ =>
                  {
                      SetBlockState(MARBLE_BLOCK_STATE.IDLE);
                      resetDisposable.Dispose();
                  }).AddTo(this);
    }

    public void Update_StateMachine()
    {
        if (BlockSM != null)
        {
            BlockSM.StateMachine_Update();
        }
    }

    public void ChangeState(CState<CMarbleBlock> newState)
    {
        if (BlockSM != null)
        {
            BlockSM.ChangeState(newState);
        }
    }

    public CState<CMarbleBlock> GetCurrentState()
    {
        if (BlockSM == null)
            return null;
        else
            return BlockSM.GetCurrentState();
    }
    
    public int GetCurrentBlockArea()
    {
        int blockIndex = GetBlockIndex();

        if (blockIndex < CMarbleDefine.BLOCK_APX_INDEX_2)
        {
            return CMarbleDefine.BLOCK_AREA_1;
        }
        else if (blockIndex < CMarbleDefine.BLOCK_APX_INDEX_3)
        {
            return CMarbleDefine.BLOCK_AREA_2;
        }
        else if (blockIndex < CMarbleDefine.BLOCK_APX_INDEX_4)
        {
            return CMarbleDefine.BLOCK_AREA_3;
        }

        return CMarbleDefine.BLOCK_AREA_4;
    }

    public bool IsSpecialBlock()
    {
        if (BlockData == null)
        {
            CDebug.LogError("CMarbleBlock::IsSpecialBlock - BlockData is null");
            return false;
        }


        return BlockData.IsSpecialTile == COMMON_AVAILABILLITY.BOOL_ENABLE;
    }

    public string GetBlockTypeName()
    {
        string typeName = string.Empty;
        switch (GetBlockType())
        {
            case MARBLE_BLOCK_TYPE.REWARD:
                typeName = CMarbleDefine.ANIM_ARRIVESTATE_NAME_REWARD;
                break;
            case MARBLE_BLOCK_TYPE.MOVE:
                typeName = CMarbleDefine.ANIM_ARRIVESTATE_NAME_MOVE;
                break;
            case MARBLE_BLOCK_TYPE.REWARD_BUFF:
                typeName = CMarbleDefine.ANIM_ARRIVESTATE_NAME_BUFF;
                break;
            case MARBLE_BLOCK_TYPE.REWARD_DEBUFF:
                typeName = CMarbleDefine.ANIM_ARRIVESTATE_NAME_DEBUFF;
                break;
        }

        return typeName;
    }

    
    public string GetArriveParamNameByBlockType()
    {
        string paramName = string.Empty;
        switch (GetBlockType())
        {
            case MARBLE_BLOCK_TYPE.REWARD:
                int blockIdx = GetBlockIndex();
                if (!BlockTileMgr.IsDeBuffBlock(blockIdx))
                {
                    paramName = CMarbleDefine.ANIM_PARAM_NAME_REWARD;

                    if (BlockTileMgr.IsBuffBlock(blockIdx))
                    {
                        CSoundControl.Instance.EffectPlay(SOUND_ID.SFX_DICEGAME_REWARD_BUFF);
                        //paramName = CMarbleDefine.ANIM_PARAM_NAME_BUFF;
                    }
                    else
                    {
                        CSoundControl.Instance.EffectPlay(SOUND_ID.SFX_DICEGAME_REWARD);
                    }
                }
                else
                {
                    CSoundControl.Instance.EffectPlay(SOUND_ID.SFX_DICEGAME_REWARD_DEBUFF);
                }
                //paramName = CMarbleDefine.ANIM_PARAM_NAME_REWARD;
                // int blockIdx = GetBlockIndex();
                // if (BlockTileMgr.IsBuffBlock(blockIdx))
                // {
                //     CSoundControl.Instance.EffectPlay(SOUND_ID.SFX_DICEGAME_REWARD_BUFF);
                //     //paramName = CMarbleDefine.ANIM_PARAM_NAME_BUFF;
                // }
                // else if (BlockTileMgr.IsDeBuffBlock(blockIdx))
                // {
                //     CSoundControl.Instance.EffectPlay(SOUND_ID.SFX_DICEGAME_REWARD_DEBUFF);
                //     //paramName = CMarbleDefine.ANIM_PARAM_NAME_DEBUFF;
                // }
                break;
            case MARBLE_BLOCK_TYPE.MOVE:
                paramName = CMarbleDefine.ANIM_PARAM_NAME_MOVE;
                CSoundControl.Instance.EffectPlay(SOUND_ID.SFX_DICEGAME_ONMOVE);
                PlayBuffFlyEffAfter(true);
                break;
            case MARBLE_BLOCK_TYPE.REWARD_BUFF:
                paramName = CMarbleDefine.ANIM_PARAM_NAME_BUFF;
                CSoundControl.Instance.EffectPlay(SOUND_ID.SFX_DICEGAME_ONBUFF);
                PlayBuffFlyEffAfter(false);
                break;
            case MARBLE_BLOCK_TYPE.REWARD_DEBUFF:
                paramName = CMarbleDefine.ANIM_PARAM_NAME_DEBUFF;
                CSoundControl.Instance.EffectPlay(SOUND_ID.SFX_DICEGAME_ONDEBUFF);
                break;
        }

        return paramName;
    }

    private void PlayBuffFlyEffAfter(bool isBuff)
    {
        SingleAssignmentDisposable _disposer = new SingleAssignmentDisposable();
        _disposer.Disposable = Observable.Timer(TimeSpan.FromSeconds(0.2f))
            .Subscribe(_ =>
            {
                if (isBuff)
                    CSoundControl.Instance.EffectPlay(SOUND_ID.SFX_DICEGAME_FLYEFF_BUFF);
                else
                    CSoundControl.Instance.EffectPlay(SOUND_ID.SFX_DICEGAME_FLYEFF_DEBUFF);
                _disposer.Dispose();
            }).AddTo(this);
    }

    public string GetArriveClipNameByBlockType()
    {
        string clipName = string.Empty;
        switch (GetBlockType())
        {
            case MARBLE_BLOCK_TYPE.REWARD:
                clipName = CMarbleDefine.ANIMCLIP_NAME_ARRIVE_REWARD;
                break;
            case MARBLE_BLOCK_TYPE.MOVE:
                clipName = CMarbleDefine.ANIMCLIP_NAME_ARRIVE_MOVE;
                break;
            case MARBLE_BLOCK_TYPE.REWARD_BUFF:
                clipName = CMarbleDefine.ANIMCLIP_NAME_ARRIVE_BUFF;
                break;
            case MARBLE_BLOCK_TYPE.REWARD_DEBUFF:
                clipName = CMarbleDefine.ANIMCLIP_NAME_ARRIVE_DEBUFF;
                break;
        }

        return clipName;
    }

    public int GetBlockSubGroup()
    {
        if (BlockData != null)
        {
            return BlockData.TileSubGroup;
        }

        return -1;
    }

    private float ImproveRewardValue;
    public void AddImproveRewardValue(int value)
    {
        ImproveRewardValue = value;
        SetBlockRewardValueText();
    }

    public CMarbleWorldManager GetWorldManager()
    {
        return WorldMgr;
    }


#endregion State_Machine


    private bool isSelectingBlock = false;

    public void OnClicked()
    {
        if (IsBlockType(MARBLE_BLOCK_TYPE.MOVE))
        {
            return;
        }

        if (WorldMgr.GetCanSelectMove() == false)
        {
            return;
        }
        
        if (CurrentState == MARBLE_BLOCK_STATE.TOUCHABLE && !isSelectingBlock)
        {
            if (WorldMgr.GetIsAutoMode())
            {
                WorldMgr.StopAutoRolling();
                // WorldMgr.SetAutoMode(false);
                // BlockTileMgr.SetActiveAutoModeUI(true);
                // BlockTileMgr.DisposeWaitMoveBlockSelect();
            }

            SetIsSelMoveToBlock(true);
            //CDebug.Log($"CMarbleBlock::OnClicked - Block Clicked!, BlockIndex: {GetBlockIndex()} ");
            BlockTileMgr.SetSelectMovingBlock(GetBlockIndex());
        }
    }

    public void SetIsSelMoveToBlock(bool isSel)
    {
        isSelectingBlock = isSel;
    }

    public void Release()
    {
        if (BlockInfo != null)
        {
            BlockInfo = null;
        }

        if (BlockData != null)
        {
            BlockData = null;
        }

        if (BlockData_NonType != null)
        {
            BlockData_NonType = null;
        }

        if (BlockSM != null)
        {
            BlockSM = null;
        }

        if (animator != null)
        {
            animator = null;
        }

        if (RewardFXController != null)
        {
            RewardFXController = null;
        }

        if (BlockEffStartBlkObj != null)
        {
            Destroy(BlockEffStartBlkObj);
            BlockEffStartBlkObj = null;
        }

        if (BlockEffObj != null)
        {
            Destroy(BlockEffObj);
            BlockEffObj = null;
        }  

        if (BlockEffMoveBlkObj != null)
        {
            Destroy(BlockEffMoveBlkObj);
            BlockEffMoveBlkObj = null;
        }      

        if (BlockBuffEffPAObj != null)
        {
            Destroy(BlockBuffEffPAObj);
            BlockBuffEffPAObj = null;
        }

        if (BlockBuffFlyEffObj != null)
        {
            Destroy(BlockBuffFlyEffObj);
            BlockBuffFlyEffObj = null;
        }       

        if (BlockBuffHitEffObj != null)
        {
            Destroy(BlockBuffHitEffObj);
            BlockBuffHitEffObj = null;
        }       

        if (BlockDeBuffEffObj != null)
        {
            Destroy(BlockDeBuffEffObj);
            BlockDeBuffEffObj = null;
        }       

        if (BlockDeBuffFlyEffObj != null)
        {
            Destroy(BlockDeBuffFlyEffObj);
            BlockDeBuffFlyEffObj = null;
        }       

        if (BlockDeBuffHitEffObj != null)
        {
            Destroy(BlockDeBuffHitEffObj);
            BlockDeBuffHitEffObj = null;
        }   
        

        if (Block_TopMR != null)
        {
            Block_TopMR = null;
        }

        if (Block_BottomMR != null)
        {
            Block_BottomMR = null;
        }

        if (Block_BaseMR != null)
        {
            Block_BaseMR = null;
        }

        if (TopMpb != null)
        {
            TopMpb = null;
        }

        if (BottomMpb != null)
        {
            BottomMpb = null;
        }

        if (BaseMpb != null)
        {
            BaseMpb = null;
        }

        if (Block_NoneTopMR != null)
        {
            Block_NoneTopMR = null;
        }
        

        if (Block_NoneBottomMR != null)
        {
            Block_NoneBottomMR = null;
        }

        if (NoneTopMpb != null)
        {
            NoneTopMpb = null;
        }

        if (NoneBottomMpb != null)
        {
            NoneBottomMpb = null;
        }

        if (ApexBlockIndices != null)
        {
            ApexBlockIndices = null;
        }
    }
}

public class BlockRewardInfo
{
    public string RewardIconPath;
    public string RewardValue;
}