
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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MGNunchiDataManager : Singleton<MGNunchiDataManager>
{
    private Dictionary<long, List<MGNunchiDataInfo>> NunchiGameDataDic = new Dictionary<long, List<MGNunchiDataInfo>>();

    #region LOAD_DATA
    public static void LoadMGNunchiGameData()
    {
        Instance._LoadMGNunchiGameData();
    }

    private void _LoadMGNunchiGameData()
    {
        DataTable table = CDataManager.GetTable(ETableDefine.TABLE_MG_NUNCHI_GAME);
        if (null == table)
        {
            CDebug.LogError(string.Format("Not Found Table : [{0}]", ETableDefine.TABLE_MG_NUNCHI_GAME));
            return;
        }

        if (NunchiGameDataDic != null)
            NunchiGameDataDic.Clear();
        else
            NunchiGameDataDic = new Dictionary<long, List<MGNunchiDataInfo>>();

        for (int i = 0; i < table.RowCount; ++i)
        {
            MGNunchiDataInfo data = new MGNunchiDataInfo();

            data.ID = table.GetValue<long>("ID", i);
            data.GroupID = table.GetValue<long>("Group_ID", i);
            data.Round = table.GetValue<byte>("Round", i);
            data.RoundTime = table.GetValue<byte>("Round_Time", i);
            data.CoinNum[0] = new CoinCapacity(table.GetValue<byte>("Coin1_Min", i), table.GetValue<byte>("Coin1_Max", i));
            data.CoinNum[1] = new CoinCapacity(table.GetValue<byte>("Coin2_Min", i), table.GetValue<byte>("Coin2_Max", i));
            data.CoinNum[2] = new CoinCapacity(table.GetValue<byte>("Coin3_Min", i), table.GetValue<byte>("Coin3_Max", i));
            data.CoinNum[3] = new CoinCapacity(table.GetValue<byte>("Coin4_Min", i), table.GetValue<byte>("Coin4_Max", i));
            data.SpecialType = (BPWPacketDefine.NunchiGameItemType)table.GetValue<byte>("Special_Type", i);
            data.SpecialCount = table.GetValue<byte>("Special_Count", i);

            if (!NunchiGameDataDic.ContainsKey(data.GroupID))
            {
                NunchiGameDataDic.Add(data.GroupID, new List<MGNunchiDataInfo>());
                NunchiGameDataDic[data.GroupID].Add(data);
            }
            else
            {
                NunchiGameDataDic[data.GroupID].Add(data);
            }
        }
    }

    #endregion LOAD_DATA


    public MGNunchiDataInfo GetMGNunchiGameDataByRound(long grpID, int round)
    {
        if(NunchiGameDataDic.ContainsKey(grpID) == false)
        {
            Debug.LogError($"GetMGNunchiGameDataByRound() NunchiGameDataDic doesn't contain {grpID}.");
            return null;
        }

        List<MGNunchiDataInfo> _list = NunchiGameDataDic[grpID];

        for(int i=0; i<_list.Count; ++i)
        {
            if(_list[i].Round == round)
            {
                return _list[i];
            }
        }

        return null;
    }

    public CoinCapacity[] GetCoinCapacityByRound(long grpID, int round)
    {
        MGNunchiDataInfo _data = GetMGNunchiGameDataByRound(grpID, round);

        if(_data == null)
        {
            Debug.LogError($"GetCoinCapacityByRound() MGNunchiTableData is null.");
            return null;
        }

        return _data.CoinNum;
    }
}
