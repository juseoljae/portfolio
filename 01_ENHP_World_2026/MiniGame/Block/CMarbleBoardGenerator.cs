using System.Collections.Generic;
using UnityEngine;
using Game.RestAPI;

public class CMarbleBoardGenerator : MonoBehaviour
{
    private CMarbleWorldManager worldMgr;
    private CMarbleBlockTileManager BlockTileMgr;
    private DicegameInfo GameInfo;
    private const int Y_POSITION = 2; 

    private float HALF_BOARD_SIZE;

    private float SHIFT_X;
    private float SHIFT_Z;
    
    public void Init(CMarbleWorldManager _worldMgr)
    {
        worldMgr = _worldMgr;
        GameInfo = CMarbleServerDataManager.Instance.GetGameInfo();

        BlockTileMgr = gameObject.GetComponent<CMarbleBlockTileManager>();
        if (BlockTileMgr != null)
        {
            BlockTileMgr.Init(worldMgr);
        }

        SetCurrentBlockIndex(GameInfo.tile_last);

        float boardLength = CMarbleDefine.APEX_SIZE + (CMarbleDefine.BLOCK_COUNT_PER_SIDE * CMarbleDefine.BLOCK_SIZE); 
        
        HALF_BOARD_SIZE = boardLength / 2.0f; 
        
        SHIFT_X = HALF_BOARD_SIZE;
        SHIFT_Z = HALF_BOARD_SIZE;

        // parent of blocks
        Transform parentTransform = this.transform;

        GenerateBoard(parentTransform);

        BlockTileMgr.SetBlockNoneType();
    }

    public void SetMarblePage(PageMarble marblePage)
    {
        if (BlockTileMgr != null)
        {
            BlockTileMgr.SetMarblePage(marblePage);
        }
    }

    private void GenerateBoard(Transform parent)
    {
        Vector3[] originalApexPositions = new Vector3[]
        {
            new Vector3(0, Y_POSITION, 0), // (start point)
            new Vector3(-HALF_BOARD_SIZE, Y_POSITION, HALF_BOARD_SIZE),  // Left 
            new Vector3(HALF_BOARD_SIZE, Y_POSITION, HALF_BOARD_SIZE),   // Top 
            new Vector3(HALF_BOARD_SIZE, Y_POSITION, -HALF_BOARD_SIZE)   // Right 
        };

        Quaternion rotationNo = Quaternion.identity;
        Quaternion rotation90 = Quaternion.Euler(0f, 90f, 0f);

        // index 1 - apex
        Vector3 originalPos = originalApexPositions[0];
        BlockTileMgr.MakeBlockObjByType(MARBLE_BLOCK_SHAPE_TYPE.APEX, originalPos, rotation90, parent);

        float startOffset = CMarbleDefine.APEX_SIZE / 2.0f + CMarbleDefine.BLOCK_SIZE / 2.0f;

        // --- Left Edge ---
        float currentZ = startOffset;
        float xPos = 0;
        for (int i = 0; i < CMarbleDefine.BLOCK_COUNT_PER_SIDE; i++)
        {
            Vector3 blkOriginalPos = new Vector3(xPos, Y_POSITION, currentZ);
            BlockTileMgr.MakeBlockObjByType(MARBLE_BLOCK_SHAPE_TYPE.NORMAL, blkOriginalPos, rotation90, parent);
            
            currentZ += CMarbleDefine.BLOCK_SIZE;
        }

        // index 2 - apex
        originalPos = originalApexPositions[1];
        Vector3 finalPos = GetFinalPosition(originalPos);
        BlockTileMgr.MakeBlockObjByType(MARBLE_BLOCK_SHAPE_TYPE.APEX, finalPos, rotationNo, parent);
        
        // --- Top Edge ---
        float currentX = startOffset;
        float zPos = HALF_BOARD_SIZE * 2;
        for (int i = 0; i < CMarbleDefine.BLOCK_COUNT_PER_SIDE; i++)
        {
            Vector3 blkOriginalPos = new Vector3(currentX, Y_POSITION, zPos);
            BlockTileMgr.MakeBlockObjByType(MARBLE_BLOCK_SHAPE_TYPE.NORMAL, blkOriginalPos, rotationNo, parent);
            
            currentX += CMarbleDefine.BLOCK_SIZE;
        }
        
        // index 3 - apex
        originalPos = originalApexPositions[2];
        finalPos = GetFinalPosition(originalPos);
        BlockTileMgr.MakeBlockObjByType(MARBLE_BLOCK_SHAPE_TYPE.APEX, finalPos, rotationNo, parent);

        // --- Right Edge ---
        currentZ = (HALF_BOARD_SIZE * 2) - startOffset; 
        xPos = HALF_BOARD_SIZE * 2;
        for (int i = 0; i < CMarbleDefine.BLOCK_COUNT_PER_SIDE; i++)
        {
            Vector3 blkOriginalPos = new Vector3(xPos, Y_POSITION, currentZ);
            BlockTileMgr.MakeBlockObjByType(MARBLE_BLOCK_SHAPE_TYPE.NORMAL, blkOriginalPos, rotation90, parent);
            
            currentZ -= CMarbleDefine.BLOCK_SIZE;
        }

        // index 4 - apex
        originalPos = originalApexPositions[3];
        finalPos = GetFinalPosition(originalPos);
        BlockTileMgr.MakeBlockObjByType(MARBLE_BLOCK_SHAPE_TYPE.APEX, finalPos, rotationNo, parent);

        // --- Bottom Edge ---
        currentX = (HALF_BOARD_SIZE * 2) - startOffset; 
        zPos = 0;
        for (int i = 0; i < CMarbleDefine.BLOCK_COUNT_PER_SIDE; i++)
        {
            Vector3 blkOriginalPos = new Vector3(currentX, Y_POSITION, zPos);
            BlockTileMgr.MakeBlockObjByType(MARBLE_BLOCK_SHAPE_TYPE.NORMAL, blkOriginalPos, rotationNo, parent);
            
            currentX -= CMarbleDefine.BLOCK_SIZE;
        }
    }


    public void SetBlockBuffState()
    {
        BlockTileMgr.SetBuffBlockTileAtFist(MARBLE_BLOCK_STATE.BUFF);
        BlockTileMgr.SetBuffBlockTileAtFist(MARBLE_BLOCK_STATE.DEBUFF);
    }

    public void UpdateBlockStateMachine()
    {
        if (BlockTileMgr != null)
        {
            BlockTileMgr.UpdateBlockStateMachine();
        }
    }

    private Vector3 GetFinalPosition(Vector3 originalPos)
    {
        return new Vector3(originalPos.x + SHIFT_X, Y_POSITION, originalPos.z + SHIFT_Z);
    }

    public CMarbleBlockTileManager GetBlockTileManager()
    {
        return BlockTileMgr;
    }

    public void SetCurrentBlockIndex(int blockIndex)
    {
        BlockTileMgr.SetCurrentBlockIndex(blockIndex);
    }

    public MARBLE_BLOCK_STATE GetCurBlockState()
    {
        return BlockTileMgr.GetCurrentBlockState();
    }

    public void AddImproveRewardValue(int subtype, int value)
    {
        BlockTileMgr.AddImproveRewardValue(subtype, value);
    }

    public void Release()
    {
        if (BlockTileMgr != null)
        {
            BlockTileMgr.Release();
            BlockTileMgr = null;
        }
    }
}