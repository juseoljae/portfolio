#define _DEBUG_MSG_ENABLED
#define _DEBUG_WARN_ENABLED
#define _DEBUG_ERROR_ENABLED

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CCharacterTouchEventDataManager : Singleton<CCharacterTouchEventDataManager>
{
    private Dictionary<AVATAR_TYPE, List<TouchEventInfo>> CharTouchEventDataDic = new Dictionary<AVATAR_TYPE, List<TouchEventInfo>>();
    private Dictionary<long, List<TouchOutputInfo>> CharTouchOutputDataDic = new Dictionary<long, List<TouchOutputInfo>>(); //key : groupID


    public static void LoadTouchEventData()
    {
        Instance._LoadTouchEventData();
    }

    public static void LoadTouchOutPutData()
    {
        Instance._LoadTouchOutPutData();
    }

    private void _LoadTouchEventData()
    {
        if (CharTouchEventDataDic != null)
            CharTouchEventDataDic.Clear();
        else
            CharTouchEventDataDic = new Dictionary<AVATAR_TYPE, List<TouchEventInfo>>();

        DataTable table = CDataManager.GetTable(ETableDefine.TABLE_AVATAR_TOUCH_EVENT);
        if (null == table)
        {
            CDebug.LogError(string.Format("Not Found Table : [{0}]", ETableDefine.TABLE_AVATAR_TOUCH_EVENT));
            return;
        }

        for (int i = 0; i < table.RowCount; i++)
        {
            TouchEventInfo _data = new TouchEventInfo()
            {
                ID = table.GetValue<long>("ID", i),
                SceneType = (TOUCHEVENT_SCENE_TYPE)table.GetValue<byte>("Scene_Type", i),
                ManageType = table.GetValue<byte>("Manage_Type", i),
                AvatarType = (AVATAR_TYPE)table.GetValue<byte>("Member_Type", i),
                IntimacyLv = table.GetValue<int>("Intimacy_Lv", i),
                ConditionLv = table.GetValue<int>("Condition_Lv", i),
                OutputGroupID = table.GetValue<long>("Output_Group_ID", i),
                Fix_RewardID = table.GetValue<long>("Fix_Reward_ID", i),
                RanddomIntimacy_RewardID = table.GetValue<long>("Random_Intimacy_Reward_ID", i),
                Random_RewardID = table.GetValue<long>("Random_Reward_ID", i),
            };

            if(CharTouchEventDataDic.ContainsKey(_data.AvatarType) == false)
            {
                CharTouchEventDataDic.Add(_data.AvatarType, new List<TouchEventInfo>());
                CharTouchEventDataDic[_data.AvatarType].Add(_data);
            }
            else
            {
                CharTouchEventDataDic[_data.AvatarType].Add(_data);
            }
        }
    }

    private void _LoadTouchOutPutData()
    {
        if (CharTouchOutputDataDic != null)
            CharTouchOutputDataDic.Clear();
        else
            CharTouchOutputDataDic = new Dictionary<long, List<TouchOutputInfo>>();

        DataTable table = CDataManager.GetTable(ETableDefine.TABLE_AVATAR_TOUCH_OUTPUT);
        if (null == table)
        {
            CDebug.LogError(string.Format("Not Found Table : [{0}]", ETableDefine.TABLE_AVATAR_TOUCH_OUTPUT));
            return;
        }

        for (int i = 0; i < table.RowCount; i++)
        {
            TouchOutputInfo _data = new TouchOutputInfo()
            {
                ID = table.GetValue<long>("ID", i),
                GroupID = table.GetValue<long>("Group_ID", i),
                BubbleMsgID = table.GetValue<long>("string", i),
                //Animation_Param = table.GetValue<string> ("ani", i),
                MotionID = table.GetValue<long>("ani", i),
                OtherAnimation_Param = table.GetValue<string> ("Other_Ani", i),
                //Effect_ResPath = table.GetValue<string>("effect", i),
                Rate = table.GetValue<int>("rate", i),
            };

            if (CharTouchOutputDataDic.ContainsKey(_data.GroupID) == false)
            {
                CharTouchOutputDataDic.Add(_data.GroupID, new List<TouchOutputInfo>());
                CharTouchOutputDataDic[_data.GroupID].Add(_data);
            }
            else
            {
                CharTouchOutputDataDic[_data.GroupID].Add(_data);
            }

        }
    }


    public TouchEventInfo GetTouchEventInfo(AVATAR_TYPE aType, TOUCHEVENT_SCENE_TYPE sceneType, bool isTraining, int intiLv, int conLv)
    {
        List<TouchEventInfo> _list = CharTouchEventDataDic[aType];
        byte _manageType = 0;
        if(isTraining)
        {
            _manageType = 1;
        }

        for(int i=0; i<_list.Count; ++i)
        {
            if (_list[i].SceneType == sceneType)
            {
                if(_list[i].SceneType == TOUCHEVENT_SCENE_TYPE.MANAGEMENT)
                {
                    if (_list[i].ManageType != _manageType)
                    {
                        continue;
                    }
                }

                if (_list[i].IntimacyLv == intiLv)
                {
                    if (_list[i].ConditionLv == conLv)
                    {
                        return _list[i];
                    }
                }
            }
        }

        return null;
    }
    
    public List<TouchEventInfo> GetTouchEventInfo(AVATAR_TYPE aType, TOUCHEVENT_SCENE_TYPE sceneType, bool isTraining)
    {
        List<TouchEventInfo> _list = CharTouchEventDataDic[aType];

        byte _manageType = 0;
        if(isTraining)
        {
            _manageType = 1;
        }

        return _list.Where(d => d.SceneType == sceneType || (d.SceneType == TOUCHEVENT_SCENE_TYPE.MANAGEMENT && d.ManageType != _manageType)).ToList();
    }

    public TouchOutputInfo GetTouchOutputInfoByID(long grpID)
    {
        if(CharTouchOutputDataDic.ContainsKey(grpID) == false)
        {
            CDebug.Log(string.Format("GetTouchOutputInfoByID ID = {0}. There is no matching info", grpID));
            return null;
        }

        return GetTouchOutputInfoByRandomValue(grpID);
    }

    private TouchOutputInfo GetTouchOutputInfoByRandomValue(long grpID)
    {
        List<TouchOutputInfo> _list = CharTouchOutputDataDic[grpID];

        int total = 0;
        for(int i=0; i< _list.Count; ++i)
        {
            total += _list[i].Rate;
        }

        int randomValue = Random.Range(0, total);

        int selectIdx = 0;
        for (int i = 0; i < _list.Count; ++i)
        {
            if(randomValue <= _list[i].Rate)
            {
                selectIdx = i;
                break;
            }
            else
            {
                randomValue -= _list[i].Rate;
            }
        }

        return _list[selectIdx];
    }

    /// <summary>
    /// [deprecated] not used intiLv & conLv
    /// </summary>
    /// <param name="aType"></param>
    /// <param name="sceneType"></param>
    /// <param name="isTraining"></param>
    /// <param name="intiLv"></param>
    /// <param name="conLv"></param>
    /// <returns></returns>
    public TouchOutputInfo GetTouchOutputInfo(AVATAR_TYPE aType, TOUCHEVENT_SCENE_TYPE sceneType, bool isTraining, int intiLv, int conLv)
    {
        TouchEventInfo _info = GetTouchEventInfo(aType, sceneType, isTraining, intiLv, conLv);

        if(_info == null)
        {
            CDebug.Log(string.Format("GetTouchOutputInfo avatar = {0}, inti_Lv = {1}, con_Lv = {2}. There is no matching info", aType, intiLv, conLv));
            return null;
        }

        return GetTouchOutputInfoByID(_info.OutputGroupID);
    }


    public TouchOutputInfo GetTouchOutputInfo(AVATAR_TYPE aType, TOUCHEVENT_SCENE_TYPE sceneType, bool isTraining)
    {
        var list = GetTouchEventInfo(aType, sceneType, isTraining); //여러개일경우..어쩌징..?
        TouchEventInfo _info = list.FirstOrDefault();
        if (_info == null)
        {
            CDebug.Log(string.Format("GetTouchOutputInfo avatar = {0}, There is no matching info", aType));
            return null;
        }

        return GetTouchOutputInfoByID(_info.OutputGroupID);
    }


}


public class TouchEventInfo
{
    public long ID { get; set; }
    public TOUCHEVENT_SCENE_TYPE SceneType { get; set; }
    public byte ManageType { get; set; }
    public AVATAR_TYPE AvatarType { get; set; }
    public int IntimacyLv { get; set; }
    public int ConditionLv { get; set; }
    public long OutputGroupID { get; set; }
    public long Fix_RewardID { get; set; }
    public long RanddomIntimacy_RewardID { get; set; }
    public long Random_RewardID { get; set; }
}

public class TouchOutputInfo
{
    public long ID { get; set; }
    public long GroupID { get; set; }
    public long BubbleMsgID { get; set; }
    public string Animation_Param { get; set; }
    public long MotionID { get; set; }
    public string OtherAnimation_Param { get; set; }
    public string Effect_ResPath { get; set; }
    public int Rate { get; set; }
}

public class TouchedEventInfo
{
    public string AnimParam;
    public string OtherAnimParam;
    public long MsgID;
    public float CheckEventTime;
    public float PrevYAngle;
    public List<GameObject> EffectObj;
}