using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using SNG;
using Game.RestAPI;

public class CMemberAvatarDataManager : Singleton<CMemberAvatarDataManager>
{
    public Dictionary<MEMBER_TYPE, MemberCharacterData> MemberCharacterDic = null;
    public Dictionary<long, StylingItemData> MemberStylingItemDic = null;
    public Dictionary<long, MemberAnimationData> MemberAnimationDic = null;
    //public new Dictionary<int, Dictionary<FACE_TYPE, List<MemberFaceData>>> MemberFaceDic = null;
    private Dictionary<int, Dictionary<EFFECT_TYPE, EffectListInfo>> MemberEffInfoDic = null;
    private Dictionary<EFFECT_TYPE, EffectListInfo> MemberEffectDic = null;
    private Dictionary<int, List<CitizenTouchInfo>> MemberTouchInfoDic = null;
    public Dictionary<long, List<StyleItemBuffData>> MemberStyleItemBuffDic = null;
    public Dictionary<MEMBER_TYPE, List<StyleItemBuffData>> EachMemberStyleItemBuffDic = null;
    public Dictionary<STYLE_ITEM_TYPE, StyleShopTabData> MemberStyleShopTabDic = null;
    public Dictionary<long, StyleShopGoodsData> MemberStyleShopGoodsDic = null;
    public List<StyleBuffFilterData> MemberStyleBuffFilterDatas = null;

    public static void OnLoadMemberCharList()
    {
        Instance.LoadMemberCharList();
    }

    public static void OnLoadMemberStylingItem()
    {
        Instance.LoadMemberStylingItem();
    }

    public static void OnLoadMemberAnimationList()
    {
        Instance.LoadMemberAnimationList();
    }

    // public static void OnLoadMemberFaceList()
    // {
    //     Instance.LoadMemberFaceList();
    // }

    public static void OnLoadMemberEffectList()
    {
        Instance.LoadMemberEffectList();
    }

    public static void OnLoadMemberTouchList()
    {
        Instance.LoadMemberTouchList();
    }

    public static void OnLoadMemberStylingBuffList()
    {
        Instance.LoadMemberStylingBuffList();
    }

    public static void OnLoadMemberStylingShopList()
    {
        Instance.LoadMemberStylingShopList();
    }

    public static void OnLoadMemberStylingShopGoodsList()
    {
        Instance.LoadMemberStylingShopGoodsList();
    }

    public static void OnLoadMemberBuffFilterList()
    {
        Instance.LoadMemberStyleBuffFilterList();
    }

    private void LoadMemberCharList()
    {
        DataTable table = CDataManager.GetTable(EDataDefine.DATA_MEMBER_CHAR_LIST);

        if (table != null)
        {
            if (MemberCharacterDic != null)
                MemberCharacterDic.Clear();
            else
                MemberCharacterDic = new Dictionary<MEMBER_TYPE, MemberCharacterData>();

            for (int index = 0; index < table.RowCount; index++)
            {
                MemberCharacterData data = new MemberCharacterData()
                {
                    MemberType = (MEMBER_TYPE)table.GetValue<long>("character_id", index),
                    NameStrID = table.GetValue<string>("character_name_string_id", index),
                    SkillGroupID = table.GetValue<int>("skill_group_id", index),
                    BasicHairID = table.GetValue<int>("basic_hair", index),
                    BasicSkinID = table.GetValue<int>("basic_skin", index),
                    CutSceneResPath = table.GetValue<string>("member_cut_resource", index),
                    IconResPath = table.GetValue<string>("resource_icon", index),
                    TouchStoryIndex = table.GetValue<int>("touch_story_index", index),
                    FaceGroupID = table.GetValue<int>("ani_member_face_group_id", index),
                    EffectGroupID = table.GetValue<int>("ani_member_effect_group_id", index),
                    PoseAniID = table.GetValue<long>("member_pose_ani_id", index),
                    GiftAniID = table.GetValue<long>("member_gift_ani_id", index)
                };

                if (MemberCharacterDic.ContainsKey(data.MemberType) == false)
                {
                    MemberCharacterDic.Add(data.MemberType, data);
                }
                else
                    CDebug.LogError($"LoadMemberDefaultStylingItem() index: {index}, ID: {data.MemberType}");
            }
        }
    }

    private void LoadMemberStylingItem()
    {
        DataTable table = CDataManager.GetTable(EDataDefine.DATA_MEMVER_STYLINGITEM_LIST);

        if (table != null)
        {
            if (MemberStylingItemDic != null)
                MemberStylingItemDic.Clear();
            else
                MemberStylingItemDic = new Dictionary<long, StylingItemData>();

            for (int index = 0; index < table.RowCount; index++)
            {
                StylingItemData data = new StylingItemData()
                {
                    ID = table.GetValue<int>("style_id", index),
                    Sort = table.GetValue<int>("style_sort", index),
                    NameStrID = table.GetValue<string>("style_name_string", index),
                    InfoStrID = table.GetValue<string>("style_info_string", index),
                    ItemType = (STYLE_ITEM_TYPE)table.GetValue<int>("style_type", index),
                    ItemSubType = (BODYACC_SUB_TYPE)table.GetValue<int>("style_subtype", index),
                    MemberType = (MEMBER_TYPE)table.GetValue<int>("style_member", index),
                    BuffGroupID = table.GetValue<int>("style_buff_group_id", index),
                    //AvatarID = table.GetValue<int>("style_member", index),
                    ResourceIconPath = table.GetValue<string>("style_resource_icon", index),
                    ResourcePath = table.GetValue<string>("style_3d_prefab", index),
                    NaviGroupID = table.GetValue<int>("navi_group_idx", index),
                    OverlapItemType = (COMMON_CATEGORY)table.GetValue<int>("overlap_item_type", index),
                    OverlapItemSubType = table.GetValue<int>("overlap_item_subtype", index),
                    OverlayItemValue = table.GetValue<int>("overlap_item_value", index)
                };

                if (MemberStylingItemDic.ContainsKey(data.ID) == false)
                {
                    MemberStylingItemDic.Add(data.ID, data);
                }
                else
                    CDebug.LogError($"LoadMemberStylingItem() index: {index}, ID: {data.ID}");
            }
        }

    }

    private void LoadMemberAnimationList()
    {
        DataTable table = CDataManager.GetTable(EDataDefine.DATA_MEMBER_ANIMATION_LIST);

        if (table != null)
        {
            if (MemberAnimationDic != null)
                MemberAnimationDic.Clear();
            else
                MemberAnimationDic = new Dictionary<long, MemberAnimationData>();

            for (int index = 0; index < table.RowCount; index++)
            {
                MemberAnimationData data = new MemberAnimationData()
                {
                    ID = table.GetValue<long>("ani_id", index),
                    AniBehavior = (eAniBehavior)table.GetValue<int>("ani_behavior", index),
                    EffectType = (EFFECT_TYPE)table.GetValue<int>("ani_effect_type", index),
                    AniTarget = table.GetValue<int>("ani_target", index),
                    AniFatigaType = (eAniFatigaType)table.GetValue<int>("ani_con_type", index),
                    ResPath = table.GetValue<string>("ani_resource", index),
                    ObjResPath = table.GetValue<string>("ani_obj_resource", index),
                    ObjDummyObj = table.GetValue<string>("ani_obj_point", index),
                    AniPlayTime = table.GetValue<float>("ani_playtime_sec", index)
                };

                if (MemberAnimationDic.ContainsKey(data.ID) == false)
                {
                    MemberAnimationDic.Add(data.ID, data);
                }
                else
                    CDebug.LogError($"LoadMemberAnimationList() index: {index}, ID: {data.ID}");
            }
        }
    }

    // private void LoadMemberFaceList()
    // {
    //     DataTable table = CDataManager.GetTable(EDataDefine.DATA_MEMBER_FACE_LIST);

    //     if (table != null)
    //     {
    //         if (MemberFaceDic != null)
    //             MemberFaceDic.Clear();
    //         else
    //             MemberFaceDic = new Dictionary<int, Dictionary<FACE_TYPE, List<MemberFaceData>>>();

    //         for (int index = 0; index < table.RowCount; index++)
    //         {
    //             MemberFaceData data = new MemberFaceData()
    //             {
    //                 ID = table.GetValue<long>("face_id", index),
    //                 GroupID = table.GetValue<int>("face_group_id", index),
    //                 FaceType = (FACE_TYPE)table.GetValue<int>("face_type", index),
    //                 ResPath = table.GetValue<string>("face_resource", index)
    //             };

    //         if (!MemberFaceDic.ContainsKey(data.GroupID))
    //         {
    //             MemberFaceDic.Add(data.GroupID, new Dictionary<FACE_TYPE, List<MemberFaceData>>());
    //         }

    //         if (!MemberFaceDic[data.GroupID].ContainsKey(data.FaceType))
    //         {
    //             MemberFaceDic[data.GroupID].Add(data.FaceType, new List<MemberFaceData>());
    //         }

    //         MemberFaceDic[data.GroupID][data.FaceType].Add(data);
    //             // if (MemberFaceDic.ContainsKey(data.GroupID) == false)
    //             // {
    //             //     MemberFaceDic.Add(data.GroupID, new Dictionary<FACE_TYPE, MemberFaceData>());
    //             //     MemberFaceDic[data.GroupID].Add(data.FaceType, data);
    //             // }
    //             // else
    //             // {
    //             //     if (MemberFaceDic[data.GroupID].ContainsKey(data.FaceType) == false)
    //             //     {
    //             //         MemberFaceDic[data.GroupID].Add(data.FaceType, data);
    //             //     }
    //             //     else
    //             //         CDebug.LogError($"LoadMemberFaceList() index: {index}, ID: {data.ID}");
    //             // }
    //         }
    //     }
    // }

    private void LoadMemberEffectList()
    {
        DataTable table = CDataManager.GetTable(EDataDefine.DATA_MEMBER_EFFECT_LIST);

        if (table != null)
        {
            if (MemberEffInfoDic != null)
                MemberEffInfoDic.Clear();
            else
                MemberEffInfoDic = new Dictionary<int, Dictionary<EFFECT_TYPE, EffectListInfo>>();

            if (MemberEffectDic != null)
                MemberEffectDic.Clear();
            else
                MemberEffectDic = new Dictionary<EFFECT_TYPE, EffectListInfo>();

            for (int index = 0; index < table.RowCount; index++)
            {
                EffectListInfo data = new EffectListInfo()
                {
                    GroupID = table.GetValue<int>("effect_group_id", index),
                    type = (EFFECT_TYPE)table.GetValue<int>("effect_type", index),
                    ResPath = table.GetValue<string>("effect_resource", index)
                };

                if (MemberEffectDic.ContainsKey(data.type) == false)
                {
                    MemberEffectDic.Add(data.type, data);
                }


                if (MemberEffInfoDic.ContainsKey(data.GroupID) == false)
                {
                    MemberEffInfoDic.Add(data.GroupID, new Dictionary<EFFECT_TYPE, EffectListInfo>());
                    MemberEffInfoDic[data.GroupID].Add(data.type, data);
                }
                else
                {
                    if (MemberEffInfoDic[data.GroupID].ContainsKey(data.type) == false)
                    {
                        MemberEffInfoDic[data.GroupID].Add(data.type, data);
                    }
                    else
                        CDebug.LogError($"LoadMemberEffectList() index: {index}, ID: {data.GroupID}");
                }

            }
        }
    }

    private void LoadMemberTouchList()
    {
        DataTable table = CDataManager.GetTable(EDataDefine.DATA_MEMBER_TOUCH_LIST);

        if (table != null)
        {
            if (MemberTouchInfoDic != null)
                MemberTouchInfoDic.Clear();
            else
                MemberTouchInfoDic = new Dictionary<int, List<CitizenTouchInfo>>();

            for (int index = 0; index < table.RowCount; index++)
            {
                CitizenTouchInfo data = new CitizenTouchInfo()
                {
                    ID = table.GetValue<int>("touch_story_id", index),
                    StoryIndex = table.GetValue<int>("touch_story_index", index),
                    StoryString = table.GetValue<string>("touch_story_string", index),
                    ConditionType = table.GetValue<int>("touch_story_con_type", index),
                    SoundID = table.GetValue<int>("sound_group_id", index)
                };


                if (MemberTouchInfoDic.ContainsKey(data.StoryIndex) == false)
                {
                    MemberTouchInfoDic.Add(data.StoryIndex, new List<CitizenTouchInfo>());
                    MemberTouchInfoDic[data.StoryIndex].Add(data);
                }
                else
                {
                    MemberTouchInfoDic[data.StoryIndex].Add(data);
                }
            }
        }
    }

    private void LoadMemberStylingBuffList()
    {
        DataTable table = CDataManager.GetTable(EDataDefine.DATA_MEMBER_STYLING_BUFF_LIST);

        if (table != null)
        {
            if (MemberStyleItemBuffDic != null)
                MemberStyleItemBuffDic.Clear();
            else
                MemberStyleItemBuffDic = new Dictionary<long, List<StyleItemBuffData>>();

            for (int index = 0; index < table.RowCount; index++)
            {
                StyleItemBuffData data = new StyleItemBuffData()
                {
                    ID = table.GetValue<long>("style_buff_id", index),
                    BuffGroupID = table.GetValue<int>("style_buff_group_id", index),
                    Desc = table.GetValue<string>("style_buff_desc", index),
                    BuffType = (STYLE_BUFF_TYPE)table.GetValue<int>("style_buff_type", index),
                    MemberTarget = (MEMBER_TYPE)table.GetValue<int>("style_buff_target", index),
                    BuffValue = table.GetValue<int>("style_buff_value", index),
                    IconResPath = table.GetValue<string>("style_buff_resource_icon", index)
                };

                if (MemberStyleItemBuffDic.ContainsKey(data.BuffGroupID) == false)
                {
                    MemberStyleItemBuffDic.Add(data.BuffGroupID, new List<StyleItemBuffData>());
                    MemberStyleItemBuffDic[data.BuffGroupID].Add(data);
                }
                else
                {
                    MemberStyleItemBuffDic[data.BuffGroupID].Add(data);
                }
            }
        }

        SetEachMemberStyleItemBuffDic();
    }

    private void LoadMemberStylingShopList()
    {
        DataTable table = CDataManager.GetTable(EDataDefine.DATA_MEMBER_STYLING_SHOP_LIST);

        if (table != null)
        {
            if (MemberStyleShopTabDic != null)
                MemberStyleShopTabDic.Clear();
            else
                MemberStyleShopTabDic = new Dictionary<STYLE_ITEM_TYPE, StyleShopTabData>();

            for (int index = 0; index < table.RowCount; index++)
            {
                StyleShopTabData data = new StyleShopTabData()
                {
                    TabType = (STYLE_ITEM_TYPE)table.GetValue<int>("shop_item_type", index),
                    IconResPath = table.GetValue<string>("shop_resource_icon", index)
                };

                if (MemberStyleShopTabDic.ContainsKey(data.TabType) == false)
                {
                    MemberStyleShopTabDic.Add(data.TabType, data);
                }
                else
                    CDebug.LogError($"LoadMemberStylingItem() index: {index}, ID: {data.TabType}");
            }
        }
    }

    private void LoadMemberStylingShopGoodsList()
    {
        DataTable table = CDataManager.GetTable(EDataDefine.DATA_MEMBER_STYLING_SHOP_GOODS_LIST);

        if (table != null)
        {
            if (MemberStyleShopGoodsDic != null)
                MemberStyleShopGoodsDic.Clear();
            else
                MemberStyleShopGoodsDic = new Dictionary<long, StyleShopGoodsData>();

            for (int index = 0; index < table.RowCount; index++)
            {
                StyleShopGoodsData data = new StyleShopGoodsData()
                {
                    ID = table.GetValue<long>("goods_id", index),
                    StyleItemID = table.GetValue<long>("style_item_id", index),
                    ItemType = (STYLE_ITEM_TYPE)table.GetValue<int>("style_type", index),
                    CostType = (COMMON_CATEGORY)table.GetValue<int>("goods_cost_type", index),
                    CostSubType = table.GetValue<long>("goods_cost_subtype", index),
                    CostValue = table.GetValue<int>("goods_cost_value", index),
                    SortValue = table.GetValue<int>("goods_sort", index),
                    isSale = table.GetValue<int>("goods_sale", index) == 1 ? true : false
                };

                if (MemberStyleShopGoodsDic.ContainsKey(data.ID) == false)
                {
                    MemberStyleShopGoodsDic.Add(data.ID, data);
                }
                else
                    CDebug.LogError($"LoadMemberStylingItem() index: {index}, ID: {data.ID}");
            }
        }
    }

    private void LoadMemberStyleBuffFilterList()
    {
        DataTable table = CDataManager.GetTable(EDataDefine.DATA_MEMBER_BUFF_FILTER_LIST);

        if (table != null)
        {
            List<StyleBuffFilterData> datas = new List<StyleBuffFilterData>();
            if (MemberStyleBuffFilterDatas != null)
                MemberStyleBuffFilterDatas.Clear();
            else
                MemberStyleBuffFilterDatas = new List<StyleBuffFilterData>();

            for (int index = 0; index < table.RowCount; index++)
            {
                StyleBuffFilterData data = new StyleBuffFilterData()
                {
                    ID = table.GetValue<int>("filter_id", index),
                    FilterSort = table.GetValue<int>("filter_sort", index),
                    BuffType = (STYLE_BUFF_TYPE)table.GetValue<int>("style_buff_type", index),
                    IconResPath = table.GetValue<string>("style_buff_resource_icon", index)
                };

                if (datas.Contains(data) == false)
                {
                    datas.Add(data);
                }
                else
                    CDebug.LogError($"LoadMemberStyleBuffFilterList() index: {index}, ID: {data.ID}");

            }

            MemberStyleBuffFilterDatas = datas.OrderBy(x => x.FilterSort).ToList();
        }
    }



    public MemberCharacterData GetMemberCharacterData(MEMBER_TYPE type)
    {
        if (MemberCharacterDic.ContainsKey(type))
        {
            return MemberCharacterDic[type];
        }

        return null;
    }

    public AvatarList GetDefaultMemberAvatar(MEMBER_TYPE type)
    {
        MemberCharacterData _data = GetMemberCharacterData(type);
        if (_data != null)
        {
            return new AvatarList()
            {
                character_id = (int)type,
                parts1 = (int)_data.BasicHairID,
                parts2 = (int)_data.BasicSkinID
            };
        }
        return null;
    }

    public string GetTouchStory(MEMBER_TYPE type)
    {
        AvatarList info = CMemberAvatarServerDataManager.Instance.GetMemberAvatarInfo((int)type);
        if (info != null)
        {
            MemberCharacterData _data = GetMemberCharacterData(type);
            if (_data != null)
            {


                List<CitizenTouchInfo> touchInfoList = GetTouchStoryListByCitizenID(_data.TouchStoryIndex, info.fatigability_grade);

                if (touchInfoList.Count > 0)
                {
                    int randomIndex = UnityEngine.Random.Range(0, touchInfoList.Count);
                    return touchInfoList[randomIndex].StoryString;
                }
            }
        }

        return string.Empty;
    }

    public List<CitizenTouchInfo> GetTouchStoryListByCitizenID(int storyIdx, int grade)
    {
        if (MemberTouchInfoDic.ContainsKey(storyIdx))
        {
            return MemberTouchInfoDic[storyIdx]
                        .Where(value => value.ConditionType == grade)
                        .ToList();
        }
        return null;
    }

    // public DefaultStylingItemData GetDefaultStylingItemData(long id)
    // {
    //     if (MemberDefaultStylingItemDic.ContainsKey(id))
    //     {
    //         return MemberDefaultStylingItemDic[id];
    //     }

    //     return null;
    // }

    public StylingItemData GetStylingItemData(long id)
    {
        if (MemberStylingItemDic.ContainsKey(id))
        {
            return MemberStylingItemDic[id];
        }

        return null;
    }

#if UNITY_EDITOR
    public StylingItemData GetStylingItemDataByResPath(string name)
    {
        if (MemberStylingItemDic == null)
            return null;
            
        return MemberStylingItemDic.Values
            .FirstOrDefault(data => data.ResourcePath == name);
    }
#endif

    public Dictionary<STYLE_ITEM_TYPE, StylingItemData> GetAllPartsStylingItemDataByAvatarList(AvatarList avatarList, Dictionary<STYLE_ITEM_TYPE, int> slotIndexDic)
    {
        Dictionary<STYLE_ITEM_TYPE, StylingItemData> curEquipItems = new Dictionary<STYLE_ITEM_TYPE, StylingItemData>();
        foreach (STYLE_ITEM_TYPE itemType in slotIndexDic.Keys)
        {
            curEquipItems.Add(itemType, null);
        }

        StylingItemData itemData = GetStylingItemData(avatarList.parts1);
        curEquipItems[STYLE_ITEM_TYPE.HAIR] = itemData;
        itemData = GetStylingItemData(avatarList.parts2);
        curEquipItems[STYLE_ITEM_TYPE.SKIN] = itemData;
        itemData = GetStylingItemData(avatarList.parts3);
        curEquipItems[STYLE_ITEM_TYPE.ACC_HEAD] = itemData;
        itemData = GetStylingItemData(avatarList.parts4);
        curEquipItems[STYLE_ITEM_TYPE.ACC_FACE] = itemData;
        itemData = GetStylingItemData(avatarList.parts5);
        curEquipItems[STYLE_ITEM_TYPE.ACC_BODY] = itemData;

        return curEquipItems;
    }


    public MemberAnimationData GetMemberAnimationData(long id)
    {
        if (MemberAnimationDic.ContainsKey(id))
        {
            return MemberAnimationDic[id];
        }

        return null;
    }

    public MemberAnimationData GetMemberAnimationDataByID(long animID)
    {
        return MemberAnimationDic.Values
            .FirstOrDefault(data => data.ID == animID);
    }

    public MemberAnimationData GetMemberAnimationData(eAniBehavior behavior, int target)
    {
        return MemberAnimationDic.Values
            .FirstOrDefault(data => data.AniTarget == target && data.AniBehavior == behavior);
    }

    public EFFECT_TYPE GetMemberLoopEffectType(eAniBehavior behavior, FATIGABILITY fatigability)
    {
        eAniFatigaType fatigueType = eAniFatigaType.ANI_CON_NONE;
        switch (fatigability)
        {
            case FATIGABILITY.HIGH:
                fatigueType = eAniFatigaType.ANI_CON_HIGH;
                break;
            case FATIGABILITY.MID:
                fatigueType = eAniFatigaType.ANI_CON_MID;
                break;
            case FATIGABILITY.LOW:
                fatigueType = eAniFatigaType.ANI_CON_LOW;
                break;
        }

        MemberAnimationData animDataList = MemberAnimationDic.Values
            .Where(data => data.AniBehavior == behavior && data.AniFatigaType == fatigueType)
            .FirstOrDefault();

        if (animDataList != null)
        {
            return animDataList.EffectType;
        }

        return EFFECT_TYPE.NONE;
    }

    public List<MemberAnimationData> GetMemberAnimDataList(eAniBehavior behavior)
    {
        return MemberAnimationDic.Values
            .Where(data => data.AniBehavior == behavior)
            .ToList();
    }

    // public Dictionary<FACE_TYPE, MemberFaceData> GetMemberFaceData(int groupID)
    // {
    //     if (MemberFaceDic.ContainsKey(groupID))
    //     {
    //         return MemberFaceDic[groupID];
    //     }

    //     return null;
    // }

    private void SetEachMemberStyleItemBuffDic()
    {
        if (EachMemberStyleItemBuffDic == null)
            EachMemberStyleItemBuffDic = new Dictionary<MEMBER_TYPE, List<StyleItemBuffData>>();

        EachMemberStyleItemBuffDic.Clear();

        foreach (var buffList in MemberStyleItemBuffDic.Values)
        {
            foreach (var buff in buffList)
            {
                if (EachMemberStyleItemBuffDic.ContainsKey(buff.MemberTarget) == false)
                {
                    EachMemberStyleItemBuffDic.Add(buff.MemberTarget, new List<StyleItemBuffData>());
                }
                EachMemberStyleItemBuffDic[buff.MemberTarget].Add(buff);
            }
        }
    }

    public List<StyleItemBuffData> GetMemberStyleItemBuffData(MEMBER_TYPE type)
    {
        if (EachMemberStyleItemBuffDic.ContainsKey(type))
        {
            return EachMemberStyleItemBuffDic[type];
        }

        return null;
    }

    public List<StyleItemBuffData> GetMemberStyleItemBuffData(long groupID)
    {
        if (MemberStyleItemBuffDic.ContainsKey(groupID))
        {
            return MemberStyleItemBuffDic[groupID];
        }

        return null;
    }

    public StyleItemBuffData GetMemberStyleItemBuffDataByID(long buffID)
    {
        return MemberStyleItemBuffDic
                .SelectMany(kvp => kvp.Value)
                .FirstOrDefault(buff => buff.ID == buffID);
    }

    public StyleShopGoodsData GetShopGoodsData(long id)
    {
        if (MemberStyleShopGoodsDic == null)
            return null;

        return MemberStyleShopGoodsDic.Values
            .FirstOrDefault(data => data.StyleItemID == id);
    }

    public List<StyleBuffFilterData> GetStyleBuffFilterDatas()
    {
        if (MemberStyleBuffFilterDatas != null)
        {
            return MemberStyleBuffFilterDatas;
        }

        return null;
    }

    public StyleBuffFilterData GetStyleBuffFilterData(int id)
    {
        if (MemberStyleBuffFilterDatas != null)
        {
            return MemberStyleBuffFilterDatas.FirstOrDefault(data => data.ID == id);
        }

        return null;
    }

    public string GetStyleTabIconPath(STYLE_ITEM_TYPE type)
    {
        if (MemberStyleShopTabDic != null && MemberStyleShopTabDic.ContainsKey(type))
        {
            return MemberStyleShopTabDic[type].IconResPath;
        }

        return string.Empty;
    }

    public List<AvatarList> GetMemberAvatarListBySkillGroupID(long skillGroupID)
    {
        List<AvatarList> avatars = null;
        List<MemberCharacterData> _list = MemberCharacterDic.Values.Where(value => value.SkillGroupID == skillGroupID).ToList();

        if (_list.Count > 0)
        {
            avatars = new List<AvatarList>();
            foreach (MemberCharacterData info in _list)
            {
                AvatarList avatarSvrInfo = CMemberAvatarServerDataManager.Instance.GetMemberAvatarInfo((int)info.MemberType);
                if (avatarSvrInfo != null)
                {
                    if (avatars.Contains(avatarSvrInfo) == false)
                    {
                        avatars.Add(avatarSvrInfo);
                    }
                }
            }
        }

        return avatars;
    }

    public string GetMemberEffectResPath(EFFECT_TYPE type)
    {
        if (MemberEffectDic.ContainsKey(type))
        {
            return MemberEffectDic[type].ResPath;
        }

        return string.Empty;
    }

    public EFFECT_TYPE GetEffectTypeByAnimID(long animID)
    {
        if (MemberAnimationDic.ContainsKey(animID))
        {
            return MemberAnimationDic[animID].EffectType;
        }

        return EFFECT_TYPE.NONE;
    }

    public string GetAnimResClipPath(long animID)
    {
        if (MemberAnimationDic.ContainsKey(animID))
        {
            return MemberAnimationDic[animID].ResPath;
        }

        return string.Empty;
    }
    
    public NctAvatarData GetNctAvatarData(long id)
    {
        return new NctAvatarData();
        
        /*
        AvatarInfo info = new AvatarInfo();
        var memberData = list[i].memberData;
        info.avatarName = CStringDataManager.Instance.GetStringData(memberData.member_name_string_id);
        info.avatarId = memberData.Id;

        bool haveavatar = SNGDataManager.Instance.AvatarsData.ContainsKey(info.avatarId);
        info.avatarLv = haveavatar ? SNGDataManager.Instance.AvatarsData[info.avatarId].avatarLv : 0;
        info.avatarExp = haveavatar ? SNGDataManager.Instance.AvatarsData[info.avatarId].avatarExp : 0;
        info.rewardLv = haveavatar ? SNGDataManager.Instance.AvatarsData[info.avatarId].rewardLv : 0;

        return info.avatardata;
        */
        /*
        NctAvatarData value;

        if (avatarDic == null)
            return null;

        if (avatarDic.TryGetValue(id, out value))
            return value;

        CDebug.LogError(string.Format("Resource is not exist : {0}", id));
        return null;
        */
    }    
}


