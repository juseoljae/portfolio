
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Game.RestAPI;

public class CMemberAvatarServerDataManager : Singleton<CMemberAvatarServerDataManager>
{
    private Dictionary<long, AvatarList> MemberAvatarInfoDic = new Dictionary<long, AvatarList>();

    private bool ClearAvatarInfos;

    private bool IsCreateAvatars;

    public void SetMemberAvatarInfo(List<AvatarList> avatarInfos)
    {
        if (avatarInfos == null || avatarInfos.Count == 0) return;
        foreach (var avatarInfo in avatarInfos)
        {
            //if (avatarInfo.character_id != 200) continue;// || avatarInfo.character_id != 201
            AddMemberAvatarInfo(avatarInfo);
        }
    }

    public void AddMemberAvatarInfo(AvatarList avatarInfo)
    {
        if (MemberAvatarInfoDic.ContainsKey(avatarInfo.character_id) == false)
        {
            MemberAvatarInfoDic.Add(avatarInfo.character_id, avatarInfo);
        }
        else
        {
            UpdateMemberAvatarInfo(avatarInfo);
        }
    }

    public void UpdateMemberAvatarInfo(AvatarList avatarInfo)
    {
        if (MemberAvatarInfoDic.ContainsKey(avatarInfo.character_id))
        {
            MemberAvatarInfoDic[avatarInfo.character_id] = avatarInfo;
        }
    }

    public void UpdateMemberAvatarImproveInfo(int charId, List<ImproveList> improveList)
    {
        if (MemberAvatarInfoDic.TryGetValue(charId, out AvatarList avatarInfo))
        {
            if (avatarInfo.improve_list != null)
            {
                foreach (var newData in improveList)
                {
                    var existingData = avatarInfo.improve_list.FirstOrDefault(x => x.improve_id == newData.improve_id);
                    if (existingData != null)
                    {
                        existingData.lv = newData.lv;
                        existingData.current_value = newData.current_value;
                        existingData.level_condition_value = newData.level_condition_value;
                    }
                }
            }
        }
    }

    public void UpdatePutOnAvatarInfo(StylingItemData itemData)
    {
        int charId = (int)itemData.MemberType;
        if (MemberAvatarInfoDic.ContainsKey(charId))
        {
            switch (itemData.ItemType)
            {
                case STYLE_ITEM_TYPE.HAIR:
                    MemberAvatarInfoDic[charId].parts1 = itemData.ID;
                    break;
                case STYLE_ITEM_TYPE.SKIN:
                    MemberAvatarInfoDic[charId].parts2 = itemData.ID;
                    break;
                case STYLE_ITEM_TYPE.ACC_HEAD:
                    MemberAvatarInfoDic[charId].parts3 = itemData.ID;
                    break;
                case STYLE_ITEM_TYPE.ACC_FACE:
                    MemberAvatarInfoDic[charId].parts4 = itemData.ID;
                    break;
                case STYLE_ITEM_TYPE.ACC_BODY:
                    MemberAvatarInfoDic[charId].parts5 = itemData.ID;
                    break;
            }
        }
    }

    public void UnEquipAvatarPart(StylingItemData itemData)
    {
        int charId = (int)itemData.MemberType;
        if (MemberAvatarInfoDic.ContainsKey(charId))
        {
            switch (itemData.ItemType)
            {
                case STYLE_ITEM_TYPE.HAIR:
                    MemberAvatarInfoDic[charId].parts1 = 0;
                    break;
                case STYLE_ITEM_TYPE.SKIN:
                    MemberAvatarInfoDic[charId].parts2 = 0;
                    break;
                case STYLE_ITEM_TYPE.ACC_HEAD:
                    MemberAvatarInfoDic[charId].parts3 = 0;
                    break;
                case STYLE_ITEM_TYPE.ACC_FACE:
                    MemberAvatarInfoDic[charId].parts4 = 0;
                    break;
                case STYLE_ITEM_TYPE.ACC_BODY:
                    MemberAvatarInfoDic[charId].parts5 = 0;
                    break;
            }
        }
    }


    public Dictionary<long, AvatarList> GetMemberAvatarInfoDic()
    {
        return MemberAvatarInfoDic;
    }

    public AvatarList GetMemberAvatarInfo(long gdid)
    {
        if (MemberAvatarInfoDic.ContainsKey(gdid))
        {
            return MemberAvatarInfoDic[gdid];
        }

        return null;
    }

    public List<ImproveList> GetMemberAvatarImproveList(long gdid)
    {
        if (MemberAvatarInfoDic.ContainsKey(gdid) && MemberAvatarInfoDic[gdid].improve_list != null)
        {
            return new List<ImproveList>(MemberAvatarInfoDic[gdid].improve_list);
        }

        return null;
    }

    public int GetMemberAvatarInfoCount()
    {
        return MemberAvatarInfoDic.Count;
    }

    public AvatarList GetCurEquipPartInAvatarInfo(long gdid)
    {
        if (MemberAvatarInfoDic.ContainsKey(gdid))
        {
            AvatarList avatarInfo = MemberAvatarInfoDic[gdid];
            return avatarInfo;
        }

        return null;
    }

    
    public List<AvatarList> GetAllWorkingInBuilding()
    {
        return MemberAvatarInfoDic.Values.Where(d => d.work_type.ToEnum<SNG.eWorkType>() != SNG.eWorkType.NONE).ToList();
    }
    
    public void ClearMemberAvatarInfoDic()
    {
        if (IsClearAvatarInfos())
        {
            if (MemberAvatarInfoDic != null)
            {
                MemberAvatarInfoDic.Clear();
            }            
        }
    }

    public void SetClearAvatarInfos(bool isClear)
    {
        ClearAvatarInfos = isClear;
    }

    public bool IsClearAvatarInfos()
    {
        return ClearAvatarInfos;
    }

    public void SetIsCreateAvatars(bool isCreate)
    {
        IsCreateAvatars = isCreate;
    }

    public bool GetIsCreateAvatars()
    {
        return IsCreateAvatars;
    }
}
