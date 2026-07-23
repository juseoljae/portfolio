using System.Collections.Generic;
using System.Linq;

public class CMarbleDataManager  : Singleton<CMarbleDataManager>
{
    private Dictionary<int, DiceTileData> DiceTileDataDic = new Dictionary<int, DiceTileData>();
    private Dictionary<int, DiceTileMapData> DiceTileMapDataDic = new Dictionary<int, DiceTileMapData>();
    private Dictionary<int, List<DiceRollingAnimData>> DiceRollingAnimDataDic = new Dictionary<int, List<DiceRollingAnimData>>();
    private Dictionary<int, List<DiceRangeGroupData>> DiceRangeGroupDataDic = new Dictionary<int, List<DiceRangeGroupData>>();
    private Dictionary<int, DiceListData> DiceListDataDic = new Dictionary<int, DiceListData>();
    private const int MAX_TILE_MAP_RATE_COUNT = 11;

    public static void OnLoadDiceTile()
    {
        Instance.LoadDiceTileData();
    }

    public static void OnLoadDiceTileMap()
    {
        Instance.LoadDiceTileMapData();
    }

    public static void OnLoadDiceRollingAnim()
    {
        Instance.LoadDiceRollingAnimData();
    }

    public static void OnLoadDiceRangeGroup()
    {
        Instance.LoadDiceRangeGroupData();
    }
    
    public static void OnLoadDiceList()
    {
        Instance.LoadDiceListData();
    }


    private void LoadDiceTileData()
    {
        DataTable table = CDataManager.GetTable(EDataDefine.DATA_MINIGAME_MARBLE_DICE_TILE);
                
        if( table == null)
        {
            CDebug.LogError(string.Format("Not Found Table : [{0}]", EDataDefine.DATA_MINIGAME_MARBLE_DICE_TILE));
            return;
        }

        if (DiceTileDataDic != null)
        {
            DiceTileDataDic.Clear();
        }
        else
            DiceTileDataDic = new Dictionary<int, DiceTileData>();

        for (int i = 0; i < table.RowCount; i++)
        {
            DiceTileData data = new DiceTileData();

            data.TileID = table.GetValue<int>("tile_id", i);
            data.TileType = (MARBLE_BLOCK_TYPE)table.GetValue<int>("tile_type", i);
            data.IsSpecialTile = (COMMON_AVAILABILLITY)table.GetValue<int>("tile_special_bool", i);
            data.TileSubGroup = table.GetValue<int>("tile_subgroup", i);
            data.TileRewardType = (REWARD_TYPE)table.GetValue<int>("tile_rwd_type", i);
            data.TileRewardSubType = table.GetValue<int>("tile_rwd_subtype", i);
            data.TileRewardMinValue = table.GetValue<long>("tile_rwd_min_value", i);
            data.TileRewardMaxValue = table.GetValue<long>("tile_rwd_max_value", i);
            // texture path on botttom, top
            data.TileResPath_Rect_Bottom = table.GetValue<string>("default_bottom_tile_tex", i);
            data.TileResPath_Square_Bottom = table.GetValue<string>("square_bottom_tile_tex", i);
            data.TileResPath_Rect_Top = table.GetValue<string>("default_top_tile_tex", i);
            data.TileResPath_Square_Top = table.GetValue<string>("square_top_tile_tex", i);

            data.TileIconResPath = table.GetValue<string>("tile_icon", i);
            data.ArrivalAniID = table.GetValue<int>("arrival_ani_id", i);

            if (!DiceTileDataDic.ContainsKey(data.TileID))
            {
                DiceTileDataDic.Add(data.TileID, data);
            }
        }
    }


    private void LoadDiceTileMapData()
    {
		DataTable table = CDataManager.GetTable(EDataDefine.DATA_MINIGAME_MARBLE_DICE_TILEMAP);
		if (null == table)
		{
			CDebug.LogError(string.Format("Not Found Table : [{0}]", EDataDefine.DATA_MINIGAME_MARBLE_DICE_TILEMAP));
			return;
		}


        if (DiceTileMapDataDic != null)
            DiceTileMapDataDic.Clear();
        else
            DiceTileMapDataDic = new Dictionary<int, DiceTileMapData>();

		for (int index = 0; index < table.RowCount; ++index)
        {
            DiceTileMapData data = new DiceTileMapData();
            data.MapID = table.GetValue<int>("tile_map_id", index);
            data.MapGroupID = table.GetValue<int>("map_group", index);
            data.TileNum = table.GetValue<int>("tile_number", index);
            
            for(int i=1 ; i< MAX_TILE_MAP_RATE_COUNT ; ++i)
            {
                int tileID = table.GetValue<int>($"tile_{i:D2}_id", index);
                int tileRate = table.GetValue<int>($"tile_{i:D2}_rate", index);
                TileMapRateInfo tileMapRateInfo = new TileMapRateInfo();
                tileMapRateInfo.TileID = tileID;
                tileMapRateInfo.Rate = tileRate;
                data.TileMapRateInfoList.Add(tileMapRateInfo);
            }
            
            if (!DiceTileMapDataDic.ContainsKey(data.MapID))
            {
                DiceTileMapDataDic.Add(data.MapID, data);
            }
        }
        
    }

    private void LoadDiceRollingAnimData()
    {
		DataTable table = CDataManager.GetTable(EDataDefine.DATA_MINIGAME_MARBLE_DICE_ROLLING_ANIM);
		if (null == table)
		{
			CDebug.LogError(string.Format("Not Found Table : [{0}]", EDataDefine.DATA_MINIGAME_MARBLE_DICE_ROLLING_ANIM));
			return;
		}


        if (DiceRollingAnimDataDic != null)
            DiceRollingAnimDataDic.Clear();
        else
            DiceRollingAnimDataDic = new Dictionary<int, List<DiceRollingAnimData>>();

		for (int index = 0; index < table.RowCount; ++index)
        {
            DiceRollingAnimData data = new DiceRollingAnimData();
            data.RollingID = table.GetValue<int>("rolling_id", index);
            data.MoveCount = table.GetValue<int>("move_count", index);
            data.AnimParam = table.GetValue<string>("dice_animation", index);
            data.DiceNum_1st = table.GetValue<int>("first_dice_number", index);
            data.DiceNum_2nd = table.GetValue<int>("second_dice_number", index);

            if (!DiceRollingAnimDataDic.ContainsKey(data.MoveCount))
            {
                DiceRollingAnimDataDic.Add(data.MoveCount, new List<DiceRollingAnimData>());
            }

            DiceRollingAnimDataDic[data.MoveCount].Add(data);
        }
    }
    
    private void LoadDiceRangeGroupData()
    {
		DataTable table = CDataManager.GetTable(EDataDefine.DATA_MINIGAME_MARBLE_DICE_RANGE_GROUP);
		if (null == table)
		{
			CDebug.LogError(string.Format("Not Found Table : [{0}]", EDataDefine.DATA_MINIGAME_MARBLE_DICE_RANGE_GROUP));
			return;
		}


        if (DiceRangeGroupDataDic != null)
            DiceRangeGroupDataDic.Clear();
        else
            DiceRangeGroupDataDic = new Dictionary<int, List<DiceRangeGroupData>>();

		for (int index = 0; index < table.RowCount; ++index)
        {
            DiceRangeGroupData data = new DiceRangeGroupData();
            data.RangeID = table.GetValue<int>("range_id", index);
            data.GroupID = table.GetValue<int>("group_id", index);
            data.MinRange = table.GetValue<int>("min_range", index);
            data.MaxRange = table.GetValue<int>("max_range", index);

            if (!DiceRangeGroupDataDic.ContainsKey(data.GroupID))
            {
                DiceRangeGroupDataDic.Add(data.GroupID, new List<DiceRangeGroupData>());
            }

            DiceRangeGroupDataDic[data.GroupID].Add(data);
        }
        
    }

        
    private void LoadDiceListData()
    {
		DataTable table = CDataManager.GetTable(EDataDefine.DATA_MINIGAME_MARBLE_DICE_LIST);
		if (null == table)
		{
			CDebug.LogError(string.Format("Not Found Table : [{0}]", EDataDefine.DATA_MINIGAME_MARBLE_DICE_LIST));
			return;
		}


        if (DiceListDataDic != null)
            DiceListDataDic.Clear();
        else
            DiceListDataDic = new Dictionary<int, DiceListData>();

		for (int index = 0; index < table.RowCount; ++index)
        {
            DiceListData data = new DiceListData();
            data.DiceID = table.GetValue<int>("dice_id", index);
            data.DiceResPath = table.GetValue<string>("dice_resource_mat", index);

            if (!DiceListDataDic.ContainsKey(data.DiceID))
            {
                DiceListDataDic.Add(data.DiceID, data);
            }
        }
        
    }

    public DiceTileData GetTileDataByID(int tileID)
    {
        if (DiceTileDataDic.TryGetValue(tileID, out DiceTileData data))
        {
            return data;
        }

        return null;
    }

    public DiceTileData GetTileDataByTileType(MARBLE_BLOCK_TYPE type)
    {
        return DiceTileDataDic.Values.FirstOrDefault(tileData => tileData.TileType == type);
    }

    public List<DiceTileMapData> GetTileMapDataListByGroupID(int groupID)
    {
        return DiceTileMapDataDic
            .Where(kvp => kvp.Value.MapGroupID == groupID)
            .Select(kvp => kvp.Value)
            .ToList();
    }

    public DiceListData GetDiceListDataByID(int diceID)
    {
        if (DiceListDataDic.ContainsKey(diceID))
        {
            return DiceListDataDic[diceID];
        }

        return null;
    }

    public List<DiceRangeGroupData> GetDiceRangeGroupDataByGroupID(int groupID)
    {
        if (DiceRangeGroupDataDic.ContainsKey(groupID))
        {
            return DiceRangeGroupDataDic[groupID];
        }

        return null;
    }

    public List<DiceRollingAnimData> GetDiceRollingAnimDataByMoveCount(int moveCount)
    {
        if (DiceRollingAnimDataDic.ContainsKey(moveCount))
        {
            return DiceRollingAnimDataDic[moveCount];
        }

        return null;
    }
}



public class DiceTileData
{
    public int               TileID;
    public MARBLE_BLOCK_TYPE TileType;
    public COMMON_AVAILABILLITY IsSpecialTile;
    public int               TileSubGroup;
    public REWARD_TYPE       TileRewardType;
    public int               TileRewardSubType;
    public long               TileRewardMinValue;
    public long               TileRewardMaxValue;
    public string            TileResPath_Rect_Bottom; //직사각형
    public string            TileResPath_Square_Bottom; //정사각형
    public string            TileResPath_Rect_Top;
    public string            TileResPath_Square_Top;
    public string            TileIconResPath;
    public int               ArrivalAniID;
}

public class DiceTileMapData
{
    public int MapID;
    public int MapGroupID;
    public int TileNum;
    public List<TileMapRateInfo> TileMapRateInfoList;

    public DiceTileMapData()
    {
        TileMapRateInfoList = new List<TileMapRateInfo>();
    }
}

public class TileMapRateInfo
{
    public int TileID;
    public float Rate;
}

public class DiceRollingAnimData
{
    public int RollingID;
    public int MoveCount;
    public string AnimParam;
    public int DiceNum_1st;
    public int DiceNum_2nd;
}

public class DiceRangeGroupData
{
    public int RangeID;
    public int GroupID;
    public int MinRange;
    public int MaxRange;
}

public class DiceListData
{
    public int DiceID;
    public string DiceResPath;
}