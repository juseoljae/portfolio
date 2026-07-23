using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CMarbleDiceManager : MonoBehaviour
{
    public CMarbleWorldManager WorldMgr;
    private CMarblePlayerManager PlayerMgr;
    private CMarbleBlockTileManager BlockTileMgr;
    private CMarbleDice Dice;
    int DiceID;
    private GameObject DiceObj;    
    private Dictionary<int, GameObject> DiceFaceObjDic;
    private MEMBER_TYPE CurMemberType;
    
    private Vector3 DiceWork_TargetPos;
    private Vector3 DiceWork_StartPos;


    private const string DiceFaceResPath = "minigame_dice_";
    private const string DICE_DEFAULT = "dice_default_id";
    private const string DICE_BASE_PATH = "minigame_dice_base";

    public void Init(CMarbleWorldManager worldMgr)
    {
        WorldMgr = worldMgr;
        PlayerMgr = WorldMgr.GetPlayerManager();
        BlockTileMgr = WorldMgr.GetBlockTileManager();

        DiceFaceObjDic = new Dictionary<int, GameObject>();

        LoadDice();
        
        LoadDiceFaces();

        CurMemberType = PlayerMgr.GetCurrentMemberType();
    }

    private void LoadDice()
    {
        var resData = CResourceManager.Instance.GetResourceData(DICE_BASE_PATH);
        if (resData != null)
        {
            GameObject obj = resData.Load<GameObject>(this.gameObject);
            if (obj != null)
            {
                DiceObj = Instantiate(obj, this.transform);  

                Dice = DiceObj.AddComponent<CMarbleDice>();
                Dice.Init(this);

                DiceObj.SetActive(true);
            }
        }            
    }


    private void LoadDiceFaces()
    {
        GameObject obj = null;

        for (int i = 1; i <= 6; i++)
        {
            string resPath = string.Format("{0}{1}", DiceFaceResPath, i);
            var resData = CResourceManager.Instance.GetResourceData(resPath);
            if (resData != null)
            {
                obj = resData.Load<GameObject>(this.gameObject);

                if (obj != null)
                {
                    DiceFaceObjDic.Add(i, obj);
                }
            }
        }

        SetDiceSkin();

        SetDefaultDice();
    }

    private void SetDiceSkin()
    {
        string skinResPath = "dice_base_tex";
        Texture diceSkinTex = null;
        MeshRenderer diceMeshRenderer = null;


        DiceID = (int)CAccountConfigDataManager.Instance.GetGameConfigData(DICE_DEFAULT).value;
        DiceListData diceListData = CMarbleDataManager.Instance.GetDiceListDataByID(DiceID);
        if (diceListData != null)
        {
            //skinResPath = diceListData.DiceResPath;
            var resData = CResourceManager.Instance.GetResourceData(skinResPath);
            if (resData != null)
            {
                diceSkinTex = resData.LoadTexture(gameObject) as Texture;
            }
            
        }

        if (diceSkinTex != null)
        {
            MaterialPropertyBlock matPropBlock = new MaterialPropertyBlock();

            foreach (var faceObj in DiceFaceObjDic.Values)
            {
                diceMeshRenderer = faceObj.GetComponentInChildren<MeshRenderer>(true);
                if (diceMeshRenderer == null)
                    continue;

                diceMeshRenderer.GetPropertyBlock(matPropBlock);
                
                matPropBlock.SetTexture("_BaseMap", diceSkinTex);
                matPropBlock.SetColor("_BaseColor", Color.white);
 
                diceMeshRenderer.SetPropertyBlock(matPropBlock);
            }
        }
    }

    private void SetDefaultDice()
    {
        int diceRand_1st = Random.Range(1, 7);
        int diceRand_2nd = Random.Range(1, 7);

        // Debug.Log($"#### Default Dice : {diceRand_1st}, {diceRand_2nd}");
        // Debug.Log($"#### Dice 1st Obj: {DiceFaceObjDic[diceRand_1st].name}, 2nd Obj: {DiceFaceObjDic[diceRand_2nd].name}");

        MakeDiceObject(diceRand_1st, diceRand_2nd, DICE_STATE.READY);
    }

    public void SetDiceWork(int diceNum)
    {
        WorldMgr.SetDiceRollResult(diceNum);

        List<DiceRollingAnimData> animDataList = CMarbleDataManager.Instance.GetDiceRollingAnimDataByMoveCount(diceNum);
        if (animDataList != null && animDataList.Count > 0)
        {
            WorldMgr.CamFocusToTarget();
            SetDiceWorkPosition();

            int randIdx = Random.Range(0, animDataList.Count);
            MakeDiceObject(animDataList[randIdx].DiceNum_1st, animDataList[randIdx].DiceNum_2nd, DICE_STATE.READYTO);
        }
    }

    private void MakeDiceObject(int DiceNum_1st, int DiceNum_2nd, DICE_STATE state)
    {        
        GameObject dice_1st = Instantiate(DiceFaceObjDic[DiceNum_1st]);
        GameObject dice_2nd = Instantiate(DiceFaceObjDic[DiceNum_2nd]);

        Dice.SetDice(dice_1st, dice_2nd, state);
    }

    public void SetDiceWorkPosition()
    {
        if (BlockTileMgr == null) return;
        
        int curBlockIndex = BlockTileMgr.GetCurrentBlockIndex();        
        int blockIdx = curBlockIndex + 1;
        if (blockIdx >= CMarbleDefine.BLOCK_MAX_COUNT)
        {
            blockIdx = CMarbleDefine.BLOCK_MAX_COUNT - 1;
        }

        Vector3[] blockWorkPos = GetDiceWorkPositionsByArea(blockIdx);
        DiceWork_TargetPos = blockWorkPos[0];
        DiceWork_StartPos = blockWorkPos[1];
    }

    private BLOCK_AREA_TYPE GetBlockTileArea(int blockIdx)
    {
        if (blockIdx >= CMarbleDefine.BLOCK_AREA_1_IDX_START && blockIdx <= CMarbleDefine.BLOCK_AREA_1_IDX_END)
        {
            return BLOCK_AREA_TYPE.AREA_1;
        }
        else if (blockIdx >= CMarbleDefine.BLOCK_AREA_2_IDX_START && blockIdx <= CMarbleDefine.BLOCK_AREA_2_IDX_END)
        {
            return BLOCK_AREA_TYPE.AREA_2;
        }
        else if (blockIdx >= CMarbleDefine.BLOCK_AREA_3_IDX_START && blockIdx <= CMarbleDefine.BLOCK_AREA_3_IDX_END)
        {
            return BLOCK_AREA_TYPE.AREA_3;
        }
        else if (blockIdx >= CMarbleDefine.BLOCK_AREA_4_IDX_START && blockIdx <= CMarbleDefine.BLOCK_AREA_4_IDX_END)
        {
            return BLOCK_AREA_TYPE.AREA_4;
        }

        return BLOCK_AREA_TYPE.NONE;
    }

    private Vector3[] GetDiceWorkPositionsByArea(int blockIdx)
    {
        Vector3[] blockWorkPositions = new Vector3[2]; //0: target pos, 1: start pos
        BLOCK_AREA_TYPE area = GetBlockTileArea(blockIdx);
        
        CMarbleBlock blockTile = BlockTileMgr.GetBlockTileByIndexForDicePos(blockIdx);
        Vector3 blockPos = blockTile.transform.localPosition;
        float blockPosY = Dice.transform.localPosition.y;
        float blockX = 0.0f;
        float blockZ = 0.0f;

        switch (area)
        {
            case BLOCK_AREA_TYPE.AREA_1:
                blockX = Random.Range(CMarbleDefine.BLOCK_LIMIT_RANGE_TYPE_1_MIN, CMarbleDefine.BLOCK_LIMIT_RANGE_TYPE_1_MAX);
                blockZ = Mathf.Clamp(blockPos.z, CMarbleDefine.BLOCK_LIMIT_RANGE_TYPE_1_MIN, CMarbleDefine.BLOCK_LIMIT_RANGE_TYPE_2_MAX);
                break;
            case BLOCK_AREA_TYPE.AREA_2:
                blockX = Mathf.Clamp(blockPos.x, CMarbleDefine.BLOCK_LIMIT_RANGE_TYPE_1_MIN, CMarbleDefine.BLOCK_LIMIT_RANGE_TYPE_2_MAX);
                blockZ = Random.Range(CMarbleDefine.BLOCK_LIMIT_RANGE_TYPE_2_MAX, CMarbleDefine.BLOCK_LIMIT_RANGE_TYPE_3_MAX);
                break;
            case BLOCK_AREA_TYPE.AREA_3:
                blockX = Random.Range(CMarbleDefine.BLOCK_LIMIT_RANGE_TYPE_2_MAX, CMarbleDefine.BLOCK_LIMIT_RANGE_TYPE_3_MAX);
                blockZ = Mathf.Clamp(blockPos.z, CMarbleDefine.BLOCK_LIMIT_RANGE_TYPE_1_MIN, CMarbleDefine.BLOCK_LIMIT_RANGE_TYPE_2_MAX);
                break;
            case BLOCK_AREA_TYPE.AREA_4:
                blockX = Mathf.Clamp(blockPos.x, CMarbleDefine.BLOCK_LIMIT_RANGE_TYPE_1_MIN, CMarbleDefine.BLOCK_LIMIT_RANGE_TYPE_2_MAX);
                blockZ = Random.Range(CMarbleDefine.BLOCK_LIMIT_RANGE_TYPE_1_MIN, CMarbleDefine.BLOCK_LIMIT_RANGE_TYPE_1_MAX);
                break;
        }

        Vector3 targetPos = new Vector3(blockX, blockPosY, blockZ);

        float startX = (area == BLOCK_AREA_TYPE.AREA_1) ? -CMarbleDefine.BLOCK_AREA_AXIS_X_DIST : targetPos.x - CMarbleDefine.BLOCK_AREA_AXIS_X_DIST;
        Vector3 startPos = new Vector3(startX, targetPos.y, targetPos.z);

        blockWorkPositions[0] = targetPos;
        blockWorkPositions[1] = startPos;

        return blockWorkPositions;
    }

    public Vector3 GetDiceWorkTargetPos()
    {
        return DiceWork_TargetPos;
    }

    public Vector3 GetDiceWorkStartPos()
    {
        return DiceWork_StartPos;
    }

    public void UpdateStateMachine()
    {
        if (Dice != null)
        {
            Dice.UpdateStateMachine();
        }
    }

    public void Release()
    {
        if (Dice != null)
        {
            Dice.Release();
            Dice = null;
        }

        if (DiceFaceObjDic != null)
        {
            DiceFaceObjDic.Clear();
            DiceFaceObjDic = null;
        }

        if (DiceObj != null)
        {
            Destroy(DiceObj);
            DiceObj = null;
        }
    }
}
