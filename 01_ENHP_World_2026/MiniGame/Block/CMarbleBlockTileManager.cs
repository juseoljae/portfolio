using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.RestAPI;
using UniRx;

public class CMarbleBlockTileManager : MonoBehaviour
{
    private CMarbleWorldManager WorldMgr;
    private PageMarble UIPage;
    private DicegameInfo GameInfo;
    private List<DicegameTileMap> BlockTileInfos;
    private List<DiceTileMapData> TileMapData;
    private List<DiceTileData> TileDataList;

    private Dictionary<int, CMarbleBlock> BlockTileList;
    private int BlockTile_ListIndex = 0;
    
    private int CurrentBlockIndex;
    private int TargetBlockIndex;

    //temp. will receve from server
    private List<BlockTileInfo> blockTileInfos;

    // Buff/DeBuff FlyEffect 완료 추적
    private int PendingFlyEffectCount;
    private object FlyEffectLock = new object();
    private Dictionary<string, GameObject> EffectObjDic = new Dictionary<string, GameObject>();
    
    [SerializeField] private GameObject StartBlockObj;
    [SerializeField] private int BlockRotEff;

    public void Init(CMarbleWorldManager worldMgr)
    {
        WorldMgr = worldMgr;
        SetServerInfo();
        
        TileDataList = new List<DiceTileData>();
        BlockTileList = new Dictionary<int, CMarbleBlock>();
        blockTileInfos = new List<BlockTileInfo>();

        MEMBER_TYPE curMemberType = worldMgr.GetCurrentMemberType();

        BlockTile_ListIndex = 0;

        SetTileData();

        LoadBlockOrgEffectObj();
    }

    public void SetMarblePage(PageMarble marblePage)
    {
        UIPage = marblePage;
    }

    private void SetServerInfo()
    {
        GameInfo = CMarbleServerDataManager.Instance.GetGameInfo();
        BlockTileInfos = CMarbleServerDataManager.Instance.GetMapInfos();
    }

    private void SetTileData()
    {        
        for (int i=0 ; i< BlockTileInfos.Count ; i++)
        {
            DicegameTileMap info = BlockTileInfos[i];
            blockTileInfos.Add(new BlockTileInfo() { Tile_ID = info.id, RewardValue = info.vol, BlockNumber = (i + 1) });
        }

        foreach (var tileInfo in blockTileInfos)
        {
            DiceTileData tileData = CMarbleDataManager.Instance.GetTileDataByID(tileInfo.Tile_ID);
            if (tileData != null)
            {
                TileDataList.Add(tileData);
            }
        }
    }

    private void LoadBlockOrgEffectObj()
    {        
        GameObject orgEffectObj = LoadOrgEffectObj(CMarbleDefine.EFFECT_NAME_START_TILE);
        EffectObjDic.Add(CMarbleDefine.EFFECT_NAME_START_TILE, orgEffectObj);

        orgEffectObj = LoadOrgEffectObj(CMarbleDefine.EFFECT_NAME_REWARD);
        EffectObjDic.Add(CMarbleDefine.EFFECT_NAME_REWARD, orgEffectObj);

        orgEffectObj = LoadOrgEffectObj(CMarbleDefine.EFFECT_NAME_MOVE_TILE);
        EffectObjDic.Add(CMarbleDefine.EFFECT_NAME_MOVE_TILE, orgEffectObj);
    }    

    private GameObject LoadOrgEffectObj(string resPath)
    {
        var resData = CResourceManager.Instance.GetResourceData(resPath);
        if (resData != null)
        {
            GameObject effObj = resData.Load<GameObject>(this.gameObject);
            if (effObj != null)
            {
                return effObj;
            }
        }
        return null;
    }

    public GameObject GetOrgEffectObj(string effName)
    {
        if (EffectObjDic.ContainsKey(effName))
        {
            return EffectObjDic[effName];
        }
        return null;
    }

    public void RenewalBlockTileData()
    {
        var renewalMapInfos = CMarbleServerDataManager.Instance.GetRenewalMapInfos();
        foreach (var blockTile in BlockTileList.Values)
        {
            blockTile.ReleaseEffectObj();
        }

        CMarbleServerDataManager.Instance.UpdateGameInfoToRenewal();
        SetServerInfo();

        ClearBlockDatas();
        
        SetTileData();

        foreach (var blockTile in BlockTileList)
        {
            int blockTileIndex = blockTile.Key - 1;
            blockTile.Value.SetBlockDatas(TileDataList[blockTileIndex], blockTileInfos[blockTileIndex], true);
            blockTile.Value.SetBlockInfo(true);
            blockTile.Value.SetBlockState(MARBLE_BLOCK_STATE.RENEWAL);
        }

        WorldMgr.SetImproveRewardValue();
    }


    public void SetBlockTileActive()
    {
        foreach (var blockTile in BlockTileList.Values)
        {
            blockTile.SetBlockState(MARBLE_BLOCK_STATE.ACTIVE);
        }
    }


    public void SetBlockTileInActive()
    {
        if (WorldMgr.GetIsAutoMode())
        {
            //UIPage.SetActiveAutoObj(false);
            UIPage.WaitMoveBlockSelect();
        }

        WorldMgr.SetActiveMoveNoticeUI(true);
        foreach (var blockTile in BlockTileList)
        {
            bool isTouchable = (blockTile.Key == CMarbleDefine.BLOCK_APX_INDEX_1) || (blockTile.Key > CurrentBlockIndex - 1);

            MARBLE_BLOCK_STATE targetState = isTouchable? MARBLE_BLOCK_STATE.TOUCHABLE : MARBLE_BLOCK_STATE.INACTIVE;

            blockTile.Value.SetBlockState(targetState);
        }

        WorldMgr.SetMainState(MARBLE_GAME_STATE.CAM_ACTION_RESTORE);
    }

    private List<int> GetBuffBlockTileList(MARBLE_BLOCK_STATE state)
    {
        DicegameInfo info = CMarbleServerDataManager.Instance.GetGameInfo();
        List<int> blockList = new List<int>();
        if (state == MARBLE_BLOCK_STATE.BUFF)
        {
            blockList = info.tile_buff;
        }
        else if (state == MARBLE_BLOCK_STATE.DEBUFF)
        {
            blockList = info.tile_debuff;
        }

        return blockList;
    }

    public void SetBuffBlockTileAtFist(MARBLE_BLOCK_STATE state)
    {    
        List<int> blockList = GetBuffBlockTileList(state);

        foreach (var blockIdx in blockList)
        {
            if (blockIdx < CurrentBlockIndex) continue;
            if (BlockTileList.ContainsKey(blockIdx))
            {
                if (BlockTileList[blockIdx].IsBlockType(MARBLE_BLOCK_TYPE.NONE)) continue;
                
                bool isBuff = state == MARBLE_BLOCK_STATE.BUFF;
                BlockTileList[blockIdx].SetActiveBlockBuffEffObj(isBuff, true);
            }
        }        
    }

    public void SetBuffBlockTile(MARBLE_BLOCK_STATE state)
    {        
        DicegameInfo info = CMarbleServerDataManager.Instance.GetGameInfo();
        if (info != null)
        {
            int buffTileIdx = info.tile_last;

            List<int> blockList = new List<int>();
            if (state == MARBLE_BLOCK_STATE.BUFF)
            {
                blockList = info.tile_buff;
            }
            else if (state == MARBLE_BLOCK_STATE.DEBUFF)
            {
                blockList = info.tile_debuff;
            }

            bool allBlockHasBuffDebuff = CheckEveryBlockHasBuffDebuff(blockList, state);            

            if (!allBlockHasBuffDebuff)
            {
                if (blockList.Count > 0)
                {
                    WorldMgr.SetMainState(MARBLE_GAME_STATE.CAM_ACTION_RESTORE);
                }

                foreach (var buffInfo in blockList)
                {
                    if (BlockTileList.ContainsKey(buffInfo))
                    {
                        //Debug.Log($"###### SetBuffBlockTile - buffTileIdx : {buffTileIdx}, buffInfo : {buffInfo}, state : {state}");
                        SetBuffState(buffTileIdx, buffInfo, state);
                    }      
                }
            }
            else
            {                
                WorldMgr.SetMainState(MARBLE_GAME_STATE.IDLE);
            }
        }
    }

    private void SetBuffState(int startBuffBlockIdx, int blockIdx, MARBLE_BLOCK_STATE state)
    {
        CMarbleBlock block = BlockTileList[blockIdx];
        if (block != null)
        {
            if (block.IsBlockType(MARBLE_BLOCK_TYPE.NONE) ||
                block.IsBlockType(MARBLE_BLOCK_TYPE.MOVE) ||
                state == MARBLE_BLOCK_STATE.BUFF && block.GetBlockState() == MARBLE_BLOCK_STATE.BUFF ||
                state == MARBLE_BLOCK_STATE.DEBUFF && block.GetBlockState() == MARBLE_BLOCK_STATE.DEBUFF ||
                block.GetBlockIndex() <= CurrentBlockIndex)
                {//                WorldMgr.SetMainState(MARBLE_GAME_STATE.IDLE);
                    return;
                }
            block.SetActiveBlockBuffPAObj(state == MARBLE_BLOCK_STATE.BUFF, true);
            IncreasePendingFlyEffectCount();
            block.SetBuffStartBlockIndex(startBuffBlockIdx);
            block.SetBlockState(state);
        }
    }

    private bool CheckEveryBlockHasBuffDebuff(List<int> blockList, MARBLE_BLOCK_STATE checkState)
    {        
        foreach (var blockIdx in blockList)
        {
            if (blockIdx <= CurrentBlockIndex) continue;

            if (BlockTileList.ContainsKey(blockIdx))
            {
                CMarbleBlock block = BlockTileList[blockIdx];
                if (block == null) return false;

                if (block.GetBlockState() != checkState)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        return blockList.Count > 0;
    }

    public void IncreasePendingFlyEffectCount()
    {
        lock (FlyEffectLock)
        {
            PendingFlyEffectCount++;
        }
    }

    public void OnFlyEffectComplete()
    {
        lock (FlyEffectLock)
        {
            PendingFlyEffectCount--;
            if (PendingFlyEffectCount <= 0)
            {
                PendingFlyEffectCount = 0;
                //GetWorldManager().CamFocusToTarget();
                
                WorldMgr.SetMainState(MARBLE_GAME_STATE.IDLE);
            }
        }
    }

    // public bool IsAllBlockFinishFlyEffectObj()
    // {
        
    // }

    public bool IsBuffBlock(int blockIdx)
    {
        DicegameInfo info = CMarbleServerDataManager.Instance.GetGameInfo();

        if (info != null &&info.tile_buff.Count > 0)
        {
            return info.tile_buff.Contains(blockIdx);
        }

        return false;
    }
    

    public bool IsDeBuffBlock(int blockIdx)
    {
        DicegameInfo info = CMarbleServerDataManager.Instance.GetGameInfo();

        if (info != null &&info.tile_debuff.Count > 0)
        {
            return info.tile_debuff.Contains(blockIdx);
        }

        return false;
    }

    public void MakeBlockObjByType(MARBLE_BLOCK_SHAPE_TYPE blockShapeType, Vector3 localPosition, Quaternion localRotation, Transform parent)
    {
        string blockResPath = CMarbleDefine.BASE_BLOCK_PREFAB_NAME;
        if (blockShapeType == MARBLE_BLOCK_SHAPE_TYPE.APEX)
        {
            blockResPath = CMarbleDefine.BASE_APEX_PREFAB_NAME;
        }
        CResourceData resData = CResourceManager.Instance.GetResourceData(blockResPath);
        if (resData == null)
        {
            Debug.LogError(string.Format("CMarbleBoardGenerator Not Found Resource Data : [{0}]", blockResPath));
            return;
        }
        
        GameObject blockObj = null;
        if (BlockTile_ListIndex == 0)
        {
            blockObj = StartBlockObj;
        }
        else 
        {
            GameObject obj = resData.LoadObject(this) as GameObject;
            blockObj = GameObject.Instantiate(obj, parent) as GameObject;

            blockObj.transform.localPosition = localPosition;
            blockObj.transform.localRotation = localRotation;
        }

        // Debug.Log($"#### MakeBlock idx : {BlockTile_ListIndex}, pos : {blockObj.transform.localPosition}, g.pos : {blockObj.transform.position}");

        blockObj.name = blockResPath + "_" + (BlockTile_ListIndex + 1);

        CMarbleBlock blockTile = blockObj.GetComponent<CMarbleBlock>();
        if (blockTile == null)
        {
            blockTile = blockObj.AddComponent<CMarbleBlock>();
        }

        int blockTileIdx = blockTileInfos[BlockTile_ListIndex].BlockNumber;
        blockTile.Init(GetWorldManager(), this, TileDataList[BlockTile_ListIndex], blockTileInfos[BlockTile_ListIndex]);

        if (BlockTileList.ContainsKey(blockTileIdx) == false)
        {
            BlockTileList.Add(blockTileIdx, blockTile);
        }

        BlockTile_ListIndex++;
    }

    public void SetBlockNoneType()
    {
        foreach (var blockTile in BlockTileList)
        {
            if (blockTile.Key < CurrentBlockIndex)
            {
                blockTile.Value.SetActiveNoneBlock(true);
            }
        }
    }

    public void SetCurrentBlockIndex(int index)
    {
        CurrentBlockIndex = index;
    }

    public int GetCurrentBlockIndex()
    {
        return CurrentBlockIndex;
    }

    public int GetNextBlockIndex()
    {
        int nextIndex = CurrentBlockIndex + 1;
        if (nextIndex > CMarbleDefine.BLOCK_MAX_COUNT)
        {
            nextIndex = 1;
        }

        return nextIndex;
    }

    public CMarbleBlock GetBlockTileByIndexForDicePos(int index)
    {
        if (BlockTileList.ContainsKey(index))
        {
            return BlockTileList[index];
        }

        return null;
    }

    

    public void SetBlockState(MARBLE_BLOCK_STATE state, int index)
    {
        CMarbleBlock block = GetBlockTileByIndexForDicePos(index);
        if (block != null)
        {
            block.SetBlockState(state);
        }
    }

    public void SetSelectMovingBlock(int targetBlockIdx)
    {
        int blockIdx = targetBlockIdx;
        SingleAssignmentDisposable _disposer = new SingleAssignmentDisposable();
        _disposer.Disposable = APIHelper.MarbleService.MGMarbleBlockSelect(targetBlockIdx)
            .Subscribe(result =>
            {
                CMarbleBlock block = GetBlockTileByIndexForDicePos(blockIdx);
                if (block != null)
                {
                    block.SetIsSelMoveToBlock(false);
                }
                WorldMgr.SetActiveMoveNoticeUI(false);
                WorldMgr.SetIsFastMove(true);
                WorldMgr.SetMoveResult(result.d.dicegame_info, result.d.dicegame_tile_map, result.d.reward_list, true);
                SetBlockTileActive();
                WorldMgr.SetActiveMoveEff_CharTrail(true);
                WorldMgr.SetMainState(MARBLE_GAME_STATE.PLAY);
                _disposer.Dispose();
            }).AddTo(this);

    }

    public void DisposeWaitMoveBlockSelect()
    {
        if (UIPage != null)
        {
            UIPage.DisposeWaitMoveBlockSelect();
        }
    }

    public void SetActiveAutoModeUI(bool isActive)
    {
        if (UIPage != null)
        {
            UIPage.SetActiveAutoObj(isActive);
        }
    }

    // Update is called once per frame
    public void UpdateBlockStateMachine()
    {
        if (BlockTileList != null && BlockTileList.Count > 0)
        {
            foreach (var blockTile in BlockTileList.Values)
            {
                blockTile.Update_StateMachine();
            }
        }

        if (Input.GetKeyDown(KeyCode.N) )
        {
            DicegameInfo info = new DicegameInfo();
            info.tile_last = 1;
            info.tile_buff = new List<int>() { 8, 19, 27 };
            CMarbleServerDataManager.Instance.SetMarbleGameInfo(info);
            SetBuffBlockTile(MARBLE_BLOCK_STATE.BUFF);
        }
    }

    

    public int GetBlockRotEff()
    {
        return BlockRotEff;
    }

    private void ClearBlockDatas()
    {
        TileDataList.Clear();
        blockTileInfos.Clear();
    }

    public CMarbleWorldManager GetWorldManager()
    {
        return WorldMgr;
    }

    public MARBLE_BLOCK_STATE GetCurrentBlockState()
    {
        int curBlockIndex = GetCurrentBlockIndex();        

        CMarbleBlock block = GetBlockTileByIndexForDicePos(curBlockIndex);
        if (block != null)
        {
           return block.GetBlockState();
        }

        return MARBLE_BLOCK_STATE.IDLE;
    }

    public MARBLE_BLOCK_TYPE GetCurrentBlockType()
    {
        int curBlockIndex = GetCurrentBlockIndex();        

        CMarbleBlock block = GetBlockTileByIndexForDicePos(curBlockIndex);
        if (block != null)
        {
           return block.GetBlockType();
        }
        
        return MARBLE_BLOCK_TYPE.NONE;
    }

    public void AddImproveRewardValue(int subTypeID, int value)
    {
        foreach (var blockTile in BlockTileList.Values)
        {
            if (blockTile.GetBlockSubGroup() == subTypeID)
            {
                blockTile.AddImproveRewardValue(value);
            }
        }
    }

    public void Release()
    {
        if (BlockTileInfos != null)
        {
            BlockTileInfos.Clear();
            BlockTileInfos = null;
        }

        if (TileMapData != null)
        {
            TileMapData.Clear();
            TileMapData = null;
        }

        if (TileDataList != null)
        {
            TileDataList.Clear();
            TileDataList = null;
        }

        if (BlockTileList != null)
        {
            foreach (var blockTile in BlockTileList.Values)
            {
                blockTile.Release();
            }
            
            BlockTileList.Clear();
            BlockTileList = null;
        }

        if (blockTileInfos != null)
        {
            blockTileInfos.Clear();
            blockTileInfos = null;
        }

        if (EffectObjDic != null)
        {
            EffectObjDic.Clear();
            EffectObjDic = null;
        }
    }
}


//temp. will receive from server
public class BlockTileInfo
{
    public int Tile_ID;
    public string RewardValue;
    public int BlockNumber;
}