using SNG;


public class MemberCharacterData
{
    public MEMBER_TYPE MemberType;//by id
    public string NameStrID;
    public int SkillGroupID;
    public long BasicHairID;
    public long BasicSkinID;
    public string CutSceneResPath;
    public string IconResPath;
    public int TouchStoryIndex;
    public int FaceGroupID;
    public int EffectGroupID;
    public long PoseAniID;
    public long GiftAniID;
}

public class StylingItemData
{
    public int ID;
    public int Sort;
    public string NameStrID;
    public string InfoStrID;
    public STYLE_ITEM_TYPE ItemType;
    public BODYACC_SUB_TYPE ItemSubType;
    public MEMBER_TYPE MemberType;
    public int BuffGroupID;

    public string ItemTAG;
    //public int AvatarID;
    public string ResourceIconPath;
    public string ResourcePath;
    public int NaviGroupID;
    public COMMON_CATEGORY OverlapItemType;
    public int OverlapItemSubType;
    public int OverlayItemValue;
}


public class DefaultStylingItemData
{
    public long ID;
    public string NameStrID;
    public int Part_HairID;
    public int Part_SkinID;
    public long SignaturePosAniID;
}

public class MemberAnimationData
{
    public long ID;
    public eAniBehavior AniBehavior;
    public EFFECT_TYPE EffectType;
    public int AniTarget;
    public eAniFatigaType AniFatigaType;
    public string ResPath;
    public string ObjResPath;
    public string ObjDummyObj;
    public float AniPlayTime;
}

public class MemberFaceData
{
    public long ID;
    public int GroupID;
    public FACE_TYPE FaceType;
    public string ResPath;
}

public class StyleItemBuffData
{
    public long ID;
    public int BuffGroupID;
    public string Desc;
    public STYLE_BUFF_TYPE BuffType;
    public MEMBER_TYPE MemberTarget;
    public int BuffValue;
    public string IconResPath;
    public string SkillNameStrID;
    public string SkillIconResPath;
    public int ItemID;
}

public class StyleShopTabData
{
    public STYLE_ITEM_TYPE TabType;
    public string IconResPath;
}

public class StyleShopGoodsData
{
    public long ID;
    public long StyleItemID;
    public STYLE_ITEM_TYPE ItemType;
    public COMMON_CATEGORY CostType;
    public long CostSubType;
    public int CostValue;
    public int SortValue;
    public bool isSale;

}

public class StyleBuffFilterData
{
    public int ID;
    public int FilterSort;
    public STYLE_BUFF_TYPE BuffType;
    public string IconResPath;
}
