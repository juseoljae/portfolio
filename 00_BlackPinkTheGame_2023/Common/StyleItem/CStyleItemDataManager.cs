
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
using UnityEngine;
using System.Linq;

public class CStyleItemDataManager : Singleton<CStyleItemDataManager>
{
    //table data of each avatar
    private Dictionary<long, CStyleItemAttr> StyleItemDataByIdDic = new Dictionary<long, CStyleItemAttr>();


    #region Style Data Load
    public static void LoadStyleItemData()
    {
        Instance._LoadStyleItemData();
    }


    private void _LoadStyleItemData()
    {
        DataTable table = CDataManager.GetTable(ETableDefine.TABLE_AVATAR_STYLE_ITEM);
        if (null == table)
        {
            CDebug.LogError(string.Format("Not Found Table : [{0}]", ETableDefine.TABLE_AVATAR_STYLE_ITEM));
            return;
        }

        if (StyleItemDataByIdDic != null)
            StyleItemDataByIdDic.Clear();
        else
            StyleItemDataByIdDic = new Dictionary<long, CStyleItemAttr>();


        //Set Default Style Item ID(string)
        foreach (AVATAR_TYPE avatarType in Enum.GetValues(typeof(AVATAR_TYPE)))
        {
            if (avatarType >= AVATAR_TYPE.AVATAR_JISOO && avatarType < AVATAR_TYPE.AVATAR_BLACKPINK)
            {
                AvatarManager.SetDefaultSItemID(avatarType);
            }
        }

        Dictionary<long, CStyleItemAttr> dicItem = new Dictionary<long, CStyleItemAttr>();
        for (int index = 0; index < table.RowCount; index++)
        {
            CStyleItemAttr data = new CStyleItemAttr
            {
                ID = table.GetValue<long>("StylingItem_ID", index),
                Name = table.GetValue<long>("Name", index),
                Desc = table.GetValue<long>("Desc", index),
                AvatarType = (AVATAR_TYPE)table.GetValue<byte>("Member_Type", index),
                EquipItemType = (STYLE_ITEM_TYPE)table.GetValue<byte>("Item_Type", index),
                EquipItemCategory = (STYLE_ITEM_CATEGORY)table.GetValue<byte>("Item_Category", index),
                EquipItemSubCategory = table.GetValue<byte>("Item_SubCategory", index),

                ResID = table.GetValue<string>("Resource_ID", index),
                MakeUP_ResID = table.GetValue<string>("Resource_Makeup_ID", index),
                ThumbNailID = table.GetValue<string>("Thumbnail_ID", index),
                AnimID = table.GetValue<string>("Animation_ID", index),
                //ItemOrder = table.GetValue<long>("Item_Order", index),
                //MarkType = (MARK_TYPE)table.GetValue<byte>("Mark_Type", index),
                VisibleType = table.GetValue<byte>("Visible_Type", index),
                ShopType = table.GetValue<byte>("Shop_Type", index),
                Collect_Point = table.GetValue<int>("Collect_Point", index),
                PlaceGroup = table.GetValue<long>("Place_Group_ID", index),

            };

            data.Consume.Type = (REWARD_CONSUME_TYPES)table.GetValue<byte>("Consume_Type", index);
            data.Consume.Value1 = table.GetValue<long>("Consume_Type_Value1", index);
            data.Consume.Value2 = table.GetValue<long>("Consume_Type_Value2", index);

            byte REQ_COUNT = 3;
            for (int i = 1; i <= REQ_COUNT; i++)
            {
                CRequire req = new CRequire
                {
                    Type = (REQUIRE_TYPES)table.GetValue<int>(string.Format($"Req{i}_Type"), index),
                    Value1 = table.GetValue<long>(string.Format($"Req{i}_Value1"), index),
                    Value2 = table.GetValue<long>(string.Format($"Req{i}_Value2"), index),
                    Value3 = table.GetValue<long>(string.Format($"Req{i}_Value3"), index),

                    StringID = table.GetValue<long>(string.Format($"Req{i}_String"), index),
                    ShortcutLink = table.GetValue<long>(string.Format($"Req{i}_Link"), index)
                };

                if (req.Type != REQUIRE_TYPES.NULL)
                {
                    //Debug.Log(data.ID + " / Require Type = "+req.Type +"/ value1 = "+req.Value1+"/value2 = "+req.Value2);
                    data.Requires.Add(req);
                }
            }




            if (StyleItemDataByIdDic.ContainsKey(data.ID))
            {
                StyleItemDataByIdDic[data.ID] = data;
            }
            else
            {
                StyleItemDataByIdDic.Add(data.ID, data);
            }
        }

    }
    #endregion

    #region Get Info
    public static CStyleItemAttr GetStyleItemByID(long ID)
    {
        if (ID == 0)
            return null;

        if (Instance.StyleItemDataByIdDic.ContainsKey(ID))
        {
            return Instance.StyleItemDataByIdDic[ID];
        }

        CDebug.LogError($"GetStyleItemByID. Not Found ID [{ID}]");

        return null;
    }

    public static Dictionary<long, CStyleItemAttr> GetStyleItemDatasByMembertype(AVATAR_TYPE mType)
    {
        AVATAR_TYPE avatarType = CPlayer.GetEnumAvatarType(mType);
        var list = Instance.StyleItemDataByIdDic.Where(d => d.Value.AvatarType == avatarType).ToDictionary(d => d.Key, d => d.Value);

        return list;
    }

    //Get Item List by Category 
    public static List<CStyleItemAttr> GetAvatarSItemCategoryList(AVATAR_TYPE avatarType, STYLE_ITEM_CATEGORY category = STYLE_ITEM_CATEGORY.NONE)
    {

        List<CStyleItemAttr> _itemList = new List<CStyleItemAttr>();

        if (category == STYLE_ITEM_CATEGORY.MY)
        {
            CStyleItemInven inven = CStyleItemInvenManager.Instance.GetStyleInvenInfo(avatarType);
            Dictionary<long, CStyleItemAttr> itemDataDic = GetStyleItemDatasByMembertype(avatarType);


            var list = CStyleItemInvenManager.Instance.GetPurchasedList().Where(purchased => itemDataDic.Any(itemdata => itemdata.Key == purchased.itemAttr.ID)).ToList();
            list.ForEach(d => _itemList.Add(d.itemAttr));

            /*
            foreach (KeyValuePair<STYLE_ITEM_TYPE, List<ClosetData>> pair in inven.PurchasedStyleItemInvenDic)
            {

                foreach (KeyValuePair<long, CStyleItemAttr> pair2 in itemDataDic)
                {
                    //pair.Value is List<CStyleItemAttr>
                    for (int i = 0; i < pair.Value.Count; ++i)
                    {
                        if (pair.Value[i].itemAttr.ID == pair2.Key)
                        {
                            _itemList.Add(pair.Value[i].itemAttr);
                            break;
                        }
                    }
                }
            }
            */


        }
        else
        {
            return Instance.StyleItemDataByIdDic.Where(d => d.Value.EquipItemCategory == category && d.Value.AvatarType == avatarType).Select(d => d.Value).ToList();
        }


        if (_itemList.Count == 0)
            return null;

        return _itemList;
    }

    //Get Item List by SubCategory 
    public static List<CStyleItemAttr> GetAvatarSItemSubCategoryList(List<CStyleItemAttr> categoryItems, STYLE_ITEM_CATEGORY category)
    {
        List<CStyleItemAttr> _itemList = new List<CStyleItemAttr>();

        if (categoryItems == null || categoryItems.Count == 0)
            return null;

        foreach (CStyleItemAttr item in categoryItems)
        {
            //Put Out default item id
            if (AvatarManager.IsDefaultStyleItemID(item.AvatarType, item.EquipItemType, item.ID))
            {
                continue;
            }

            if (item.VisibleType != 0)
            {
                _itemList.Add(item);
            }
        }

        return _itemList;
    }

    public static List<CStyleItemAttr> GetPurchasedItemList(List<CStyleItemAttr> categoryItems, STYLE_ITEM_CATEGORY category)
    {
        List<CStyleItemAttr> _itemList = new List<CStyleItemAttr>();

        if (categoryItems == null || categoryItems.Count == 0)
            return null;

        foreach (CStyleItemAttr item in categoryItems)
        {
            //Put Out default item id
            if (AvatarManager.IsDefaultStyleItemID(item.AvatarType, item.EquipItemType, item.ID))
            {
                continue;
            }

            if (item.VisibleType != 0)
            {
                if (CStyleItemInvenManager.Instance.IsStyleItemInInventory(item.ID))
                {
                    _itemList.Add(item);
                }
            }
        }

        return _itemList;
    }
    #endregion






}
