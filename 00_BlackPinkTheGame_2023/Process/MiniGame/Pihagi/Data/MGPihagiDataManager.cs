
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

public class MGPihagiDataManager : Singleton<MGPihagiDataManager>
{
    private Dictionary<long, MGPihagiGameDataInfo> PihagiGameDataDic = new Dictionary<long, MGPihagiGameDataInfo>();
    private Dictionary<long, MGPihagiEnemyInfo> PihagiEnemyDataDic = new Dictionary<long, MGPihagiEnemyInfo>();

    #region LOAD_DATA
    public static void LoadMGPihagiGameData()
    {
        Instance._LoadMGPihagiGameData();
    }

    public static void LoadMGPihagiEnemyData()
    {
        Instance._LoadMGPihagiEnemyData();
    }


    private void _LoadMGPihagiGameData()
    {
        DataTable table = CDataManager.GetTable(ETableDefine.TABLE_MG_PIHAGI_GAME);
        if (null == table)
        {
            CDebug.LogError(string.Format("Not Found Table : [{0}]", ETableDefine.TABLE_MG_PIHAGI_GAME));
            return;
        }

        if (PihagiGameDataDic != null)
            PihagiGameDataDic.Clear();
        else
            PihagiGameDataDic = new Dictionary<long, MGPihagiGameDataInfo>();

        for (int i = 0; i < table.RowCount; ++i)
        {
            MGPihagiGameDataInfo data = new MGPihagiGameDataInfo() {
                ID = table.GetValue<long>("ID", i),
                Time = table.GetValue<byte>("Time", i),
                SpawnGroupID = table.GetValue<long>("Spawn_Group_ID", i),
                SpawnPoint = table.GetValue<byte>("Spawn_Point", i),
                EnemyID = table.GetValue<long>("Enemy_ID", i),
                EnemyDelayTime = table.GetValue<int>("Enemy_Delay_Time", i),
            };

            if (!PihagiGameDataDic.ContainsKey(data.ID))
            {
                PihagiGameDataDic.Add(data.ID, new MGPihagiGameDataInfo());
                PihagiGameDataDic[data.ID] = data;
            }
            else
            {
                PihagiGameDataDic[data.ID] = data;
            }
        }
    }

    private void _LoadMGPihagiEnemyData()
    {
        DataTable table = CDataManager.GetTable(ETableDefine.TABLE_MG_PIHAGI_ENEMY);
        if (null == table)
        {
            CDebug.LogError(string.Format("Not Found Table : [{0}]", ETableDefine.TABLE_MG_PIHAGI_ENEMY));
            return;
        }

        if (PihagiEnemyDataDic != null)
            PihagiEnemyDataDic.Clear();
        else
            PihagiEnemyDataDic = new Dictionary<long, MGPihagiEnemyInfo>();

        for (int i = 0; i < table.RowCount; ++i)
        {
            MGPihagiEnemyInfo data = new MGPihagiEnemyInfo()
            {
                ID = table.GetValue<long>("ID", i),
                Type = (ENEMY_TYPE)table.GetValue<int>("Enemy_Type", i),
                ControllerGrpID = table.GetValue<long>( "Ani_Controller_ID", i ),
                Resource = table.GetValue<string>("Enemy_Resource", i),
                Speed = table.GetValue<int>("Enemy_Speed", i),
            };

            if (!PihagiEnemyDataDic.ContainsKey(data.ID))
            {
                PihagiEnemyDataDic.Add(data.ID, new MGPihagiEnemyInfo());
                PihagiEnemyDataDic[data.ID] = data;
            }
            else
            {
                PihagiEnemyDataDic[data.ID] = data;
            }
        }
    }
    #endregion LOAD_DATA





    #region GET_DATA
    public MGPihagiGameDataInfo GetPihagiGameDataInfo(long id)
    {
        if(PihagiGameDataDic.ContainsKey(id))
        {
            return PihagiGameDataDic[id];
        }
        return null;
    }

    public long GetPihagiGameEnemyID(long id)
    {
        MGPihagiGameDataInfo _info = GetPihagiGameDataInfo(id);

        if(_info != null)
        {
            return _info.EnemyID;
        }

        return 0;
    }

    public Dictionary<long, MGPihagiEnemyInfo> GetPihagiAllEnemyInfo()
    {
        return PihagiEnemyDataDic;
    }

    public MGPihagiEnemyInfo GetPihagiEnemyDataInfo(long id)
    {
        if (PihagiEnemyDataDic.ContainsKey(id))
        {
            return PihagiEnemyDataDic[id];
        }
        return null;
    }

    public string GetPihagiEnemyResPath(long id)
    {
        MGPihagiEnemyInfo _info = GetPihagiEnemyDataInfo(id);

        if(_info !=null)
        {
            return _info.Resource;
        }

        return string.Empty;
    }
    #endregion GET_DATA
}
