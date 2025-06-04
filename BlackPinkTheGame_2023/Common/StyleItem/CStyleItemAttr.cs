
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;


#region STYLE item
// JH about STYLE item
public enum MARK_TYPE
{
    NONE = 0,
    NEW = 1,
    HOT = 2,
}


public enum ITEM_CATEGORY
{
    NONE = 0,
    MEMBER,
    SCHEDULE,
    PHOTO_CARD,
    ETC,
    BPWORLD,
}

public enum STYLE_ITEM_CATEGORY
{
    NONE = -1,
    MY,
    HEAD,
    TOP,
    BOTTOM,
    SUIT,
    SOCKS,
    SHOES,
    ACC, 
    ACC_ETC
}

public enum EMOTION_ITEM_CATEGORY
{
    MY = 0,
    ALL_MOTION = 1, 
    ALL_EMOTICON = 2, 
}

public enum STYLE_ITEM_TYPE
{
    NONE = 0,
    HEAD,
    TOP,
    BOTTOM,
    SUIT,
    SOCKS,  // 5
    SHOES,
    ACC_HAIR,
    ACC_EYE,
    ACC_MOUTH,
    ACC_EARING, // 10
    ACC_NECK,
    ACC_HAND,
    ACC_WRIST,
    ACC_BACK,
    ACC_FACE, // tatoo
    ACC_BODY,
    ACC_PROP,

    EFFECT_HAND_RIGHT = 18,
    EFFECT_HAND_LEFT,
    EFFECT_HAND_BOTH,

    EFFECT_HEAD_TOP,//21
    EFFECT_HEAD_BACK,
    EFFECT_HEAD_NOSE,
    EFFECT_HEAD_MOUTH,
    EFFECT_HEAD_CHEEK,
    
    EFFECT_SPINE_SHOULDER, //26
    EFFECT_SPINE_BACK,
    EFFECT_SPINE_BACK2,
    EFFECT_COM_WAIST, //29
    EFFECT_COM_BODY,

    EFFECT_ROOT_FLOOR, //31
    EFFECT_ROOT_ALL,

    ACC_BROOM = 40, // broom of witch

    FACE = 99,
}


public enum STYLE_ITEM_EFFECT_TYPE
{
    HAND_L = 1,
    HAND_R = 2, 
    HAND = 3,
    HEAD = 8,
    SPINE = 11,
    COM = 13,
    ROOT = 15
}

#endregion
[System.Serializable]
public class CStyleItemAttr //: CItemAttr
{
    public long ID { get; set; }
    public long Name { get; set; }
    public long Desc { get; set; }
    public AVATAR_TYPE AvatarType { get; set; }

    public STYLE_ITEM_TYPE EquipItemType { get; set; }//subType을 enum으로 넣는다// Set Item
    public STYLE_ITEM_CATEGORY EquipItemCategory { get; set; }
    public byte EquipItemSubCategory { get; set; }

    public CConsume Consume { get; set; }

    //=============================================
    //삭제예정
    //public CURRENCY_TYPE CsumeRequireType { get; set; }
    //public long CsumeRequireTypeValue1 { get; set; }
    //public long CsumeRequireTypeValue2 { get; set; }
    //=============================================

    public string ResID { get; set; }
    public string MakeUP_ResID { get; set; }
    public string ThumbNailID { get; set; }
    public string AnimID { get; set; }
    public long ItemOrder { get; set; }

    public byte VisibleType { get; set; }
    public byte ShopType { get; set; }
    public int Collect_Point { get; set; } // 꾸미기 점수

    public List<CRequire> Requires = new List<CRequire>();

    public long PlaceGroup { get; set; }

    //public STYLE_ITEM_STATUS EquipStatus { get; set; }
    public AVATAR_TYPE ItemOwner { get; set; }
    public GameObject ItemObject { get; set; }


    /// <summary>
    // 서버에서 받은 값을 설정해서 사용
    public MARK_TYPE MarkType { get; set; }     // hot, new
    public long discount_mts { get; set; }      // 할인 남은 시간
    public bool bReqPass { get; set; }      // 해금 아이템 여부
    /// </summary>

    public CStyleItemAttr()
    {
        //EquipStatus = STYLE_ITEM_STATUS.NORMAL;
        EquipItemType = STYLE_ITEM_TYPE.NONE;
        ItemOwner = AVATAR_TYPE.AVATAR_BLACKPINK; // Common

        Consume = new CConsume();
    }
}


