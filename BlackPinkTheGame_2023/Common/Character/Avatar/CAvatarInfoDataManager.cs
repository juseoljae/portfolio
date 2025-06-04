
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
using System.Linq;




// 아바타 컨디션
public class ConditionClassData
{
    public long ID { get; set; }
    public long Lv_Group { get; set; }          // 레벨 그룹
    public byte Class { get; set; }             // 1 ~ 5단계
    public byte Class_Value_Type { get; set; }  // 1 절대값 계산, 2 백분률 계산
    public long Class_Value { get; set; }       // 계산 기준 값
}

public class AvatarTitleStatRangeData
{
    //public byte StatType;
    public long StatValueMin;
    public long StatValueMax;

    public AvatarTitleStatRangeData(/*byte type, */long min, long max)
    {
        //StatType = type;
        StatValueMin = min;
        StatValueMax = max;
    }
}

public class MotionData
{
    public long ID { get; set; }
    public AVATAR_TYPE Type { get; set; }
    public string AnimName { get; set; }
    public string AnimParam { get; set; }

    public long[] Object_ID;

    public string ResPath { get; set; }

    public long RandObjectID
    {
        get
        {
            var list = Object_ID.ToList().Where(d => d > 0).ToList();
            if (list.Count > 0)
            {
                var idx = UnityEngine.Random.Range(0, list.Count);
                return list[idx];
            }
            return 0;
        }
    }
}


// 아바타 stat
public class AvatarStatData
{
    public long AvatarStat_ID { get; set; }
    public byte Stat_Type { get; set; }
    public long Stat_Lv { get; set; }
    public long Stat_Exp { get; set; }
    public long Stat_Value { get; set; }
}


//===============================================//

public class CAvatarInfoDataManager : Singleton<CAvatarInfoDataManager>
{
    private Dictionary<long, ConditionClassData> _conditionClassDic = new Dictionary<long, ConditionClassData> ();
    private Dictionary<byte, List<AvatarStatData>> _avatarStatDic = new Dictionary<byte, List<AvatarStatData>>();

    private Dictionary<AVATAR_TYPE, List<MotionData>> MotionDataDic = new Dictionary<AVATAR_TYPE, List<MotionData>>();

    //===============================================//


    // 레벨링 컨디션 단계 테이블 로드
    public static void LoadConditionClassData ()
    {
        Instance._LoadConditionClassData ();
    }

    // 아바타 스탯 테이블 로드
    public static void LoadAvatarStatData()
    {
        Instance._LoadAvatarStatData();
    }

    public static void LoadMotionData()
    {
        Instance._LoadMotionData();
    }


    // 레벨링 컨디션 단계 데이터 로드
    private void _LoadConditionClassData ()
    {
        string tableName = ETableDefine.TABLE_AVATAR_CONDITION_CLASS;
        DataTable table = CDataManager.GetTable (tableName);

        if (null == table)
        {
            CDebug.LogError (string.Format ("Not Found Table : [{0}]", tableName));
            return;
        }

        _conditionClassDic?.Clear ();
        _conditionClassDic = new Dictionary<long, ConditionClassData> ();

        for (int i = 0; i < table.RowCount; i++)
        {
            if (!table.TryGetValue ("id", i, out string getID) || string.IsNullOrWhiteSpace (getID))
            {
                // 테이블 오류로 인한 임시 데이터 처리
                CDebug.LogError (string.Format ($"Load [ {tableName} ] i:[{i}] [id] not valid"));
                break;
            }

            ConditionClassData data = new ConditionClassData ()
            {
                ID = table.GetValue<long> ("id", i),
                Lv_Group = table.GetValue<long> ("lv_group", i),
                Class = table.GetValue<byte> ("class", i),
                Class_Value_Type = table.GetValue<byte> ("class_value_type", i),
                Class_Value = table.GetValue<long> ("class_value", i)
            };

            if (!_conditionClassDic.ContainsKey (data.ID))
            {
                _conditionClassDic.Add (data.ID, data);
            }
            else
            {
                CDebug.LogError (string.Format ($"Load [ {tableName} ]. i:[{i}] Lv {data.ID} Key Duplication!!!!!"));
            }

        }
    }

    private void _LoadMotionData()
    {
        string tableName = ETableDefine.TABLE_AVATAR_MOTION;
        DataTable table = CDataManager.GetTable(tableName);

        if (null == table)
        {
            CDebug.LogError(string.Format("Not Found Table : [{0}]", tableName));
            return;
        }

        MotionDataDic?.Clear();
        MotionDataDic = new Dictionary<AVATAR_TYPE, List<MotionData>>();

        for (int i = 0; i < table.RowCount; i++)
        {
            if (!table.TryGetValue("id", i, out string getID) || string.IsNullOrWhiteSpace(getID))
            {
                // 테이블 오류로 인한 임시 데이터 처리
                CDebug.LogError(string.Format($"Load [ {tableName} ] i:[{i}] [id] not valid"));
                break;
            }

            MotionData data = new MotionData();

            data.ID = table.GetValue<long>("id", i);
            data.Type = (AVATAR_TYPE)table.GetValue<byte>("Type", i);
            data.AnimName = table.GetValue<string>("Ainm_Name", i);
            data.AnimParam = table.GetValue<string>("ani", i);
            data.ResPath = table.GetValue<string>( "ani_resource", i );
            data.Object_ID = new long[3];
            for (int j = 0; j < data.Object_ID.Length; ++j)
            {
                string _name = string.Format("Object_0{0}_ID", (j + 1));
                data.Object_ID[j] = table.GetValue<long>(_name, i);
            }

            if(MotionDataDic.ContainsKey(data.Type) == false)
            {
                MotionDataDic.Add(data.Type, new List<MotionData>());
                MotionDataDic[data.Type].Add(data);
            }
            else
            {
                MotionDataDic[data.Type].Add(data);
            }
        }
    }

    // 아바타 스탯 데이타 로드
    private void _LoadAvatarStatData()
    {
        string tableName = ETableDefine.TABLE_AVATAR_STAT;
        DataTable table = CDataManager.GetTable(tableName);

        if (null == table)
        {
            CDebug.LogError(string.Format("Not Found Table : [{0}]", tableName));
            return;
        }

        _avatarStatDic?.Clear();
        _avatarStatDic = new Dictionary<byte, List<AvatarStatData>>();

        for (int i = 0; i < table.RowCount; i++)
        {
            if (!table.TryGetValue("AvatarStat_ID", i, out string getID) || string.IsNullOrWhiteSpace(getID))
            {
                // 테이블 오류로 인한 임시 데이터 처리
                CDebug.LogError(string.Format($"Load [ {tableName} ] i:[{i}] [id] not valid"));
                break;
            }

            AvatarStatData data = new AvatarStatData()
            {
                AvatarStat_ID = table.GetValue<long>("AvatarStat_ID", i),
                Stat_Type = table.GetValue<byte>("Stat_Type", i),
                Stat_Lv = table.GetValue<long>("Stat_Lv", i),
                Stat_Exp = table.GetValue<long>("Stat_Exp", i),
                Stat_Value = table.GetValue<long>("Stat_Value", i),
            };

            if (_avatarStatDic.ContainsKey(data.Stat_Type) == false)
            {
                _avatarStatDic.Add(data.Stat_Type, new List<AvatarStatData>());
                _avatarStatDic[data.Stat_Type].Add(data);
            }
            else
            {
                _avatarStatDic[data.Stat_Type].Add(data);
            }
        }
    }
        public static int GetStatMaxLevel(int stattype)
    {
        // stat_type은 0부터, avatarstattable은 type을 1 부터 사용
        byte _stattype = (byte)GetActivityStatType((STAT_TYPE)stattype);
        if (Instance._avatarStatDic.ContainsKey(_stattype)) 
        {
            List<AvatarStatData> lstStatData = Instance._avatarStatDic[_stattype].OrderByDescending(x => x.Stat_Lv).ToList();
            return (int)lstStatData[0].Stat_Lv;
        }

        return 0;
    }

    public static AvatarStatData GetStatDataByLevelWithType(int stattype, int statlevel)
    {
        byte _stattype = (byte)GetActivityStatType((STAT_TYPE)stattype);
        if (Instance._avatarStatDic.ContainsKey(_stattype))
        {
            List<AvatarStatData> lstStatData = Instance._avatarStatDic[_stattype].OrderByDescending(x => x.Stat_Lv).ToList();
            foreach(AvatarStatData data in lstStatData)
            {
                if(data.Stat_Lv == statlevel)
                {
                    return data;
                }
            }
        }

        return null;
    }


    public static ConditionClassData[] GetConditionClassData (int lvGrup)
    {
        var searchArr = Instance._conditionClassDic.Values
            .Where (c => c.Lv_Group == lvGrup)
            .OrderByDescending(c => c.Class)
            .ToArray ();

        // 무조건 5개씩
        if (searchArr.Length != 5)
        {
            CDebug.LogError (string.Format ($"GetConditionClassData. is not valid."));
        }
        return searchArr;
    }

    public static List<MotionData> GetMotionDataByAvatarType(AVATAR_TYPE aType)
    {
        if (aType == 0)
        {
            return null;
        }


        return Instance.MotionDataDic[aType];
    }

    public static MotionData GetMotionDataByID(long ID)
    {
        return Instance.MotionDataDic.SelectMany(d => d.Value).Where(d => d.ID == ID).ToList().FirstOrDefault();
    }


}