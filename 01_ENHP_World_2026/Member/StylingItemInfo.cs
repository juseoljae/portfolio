using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StylingItemInfo
{
    public StylingItemData ItemData;
    private GameObject MAvatarRootObj;
    public GameObject ItemObj;

    public StylingItemInfo(StylingItemData data, GameObject avatarRootObj)
    {
        ItemData = data;
        MAvatarRootObj = avatarRootObj;
        if (data == null) return;
        switch (data.ItemType)
        {
            case STYLE_ITEM_TYPE.HAIR:
                ItemData.ItemTAG = OBJ_TAG.MEMBER_HAIR;
                break;
            case STYLE_ITEM_TYPE.SKIN:
                ItemData.ItemTAG = OBJ_TAG.MEMBER_SKIN;
                break;
            case STYLE_ITEM_TYPE.ACC_HEAD:
            case STYLE_ITEM_TYPE.ACC_FACE:
            case STYLE_ITEM_TYPE.ACC_BODY:
                ItemData.ItemTAG = OBJ_TAG.MEMBER_ACC;
                break;
        }
    }

    public void LoadItemObj()
    {
        if (ItemData == null) return;

        var resData = CResourceManager.Instance.GetResourceData(ItemData.ResourcePath);
        if (resData != null)
        {
            ItemObj = resData.Load<GameObject>(MAvatarRootObj);
        }
    }

    public void UnLoadItemObj()
    {
        if (ItemObj != null)
        {
            ItemObj = null;
            //GameObject.Destroy(ItemObj);
        }
    }

#if UNITY_EDITOR
    public void LoadItemObjForTool(GameObject itemObj)
    {
        ItemObj = itemObj;
    }
#endif
}




public enum STYLE_ITEM_TYPE
{
    NONE = 0,

    //part
    HAIR,
    SKIN,

    //accecery
    ACC_HEAD,
    ACC_FACE,
    ACC_BODY,
    MAX
}

public enum BODYACC_SUB_TYPE
{
    NONE = 0,
    STYLE_SUBTYPE_ACC_BAG
}


