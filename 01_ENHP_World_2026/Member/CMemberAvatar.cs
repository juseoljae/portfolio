using SNG;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Game.RestAPI;
using CharacterControl;

public class CMemberAvatar
{
    public MEMBER_TYPE MemberType = MEMBER_TYPE.NONE;
    public MemberCharacterData MemberCharacterData = null;
    public AvatarList MemberAvatarInfo = null;
    public GameObject MemberAvatarObj = null;
    public Dictionary<STYLE_ITEM_TYPE, StylingItemInfo> CurEquipSItemDic = null;
    public Dictionary<STYLE_ITEM_TYPE, StylingItemInfo> PutOnEquipSItemDic = null;
    public CMemberEquipFactory EquipFactory = null;
    public CMemberAvatarController MemberAvatarController = null;
    private TownNPCSimpleMove SimpleMove = null;



    public CMemberAvatar(MEMBER_TYPE mType)
    {
        MemberType = mType;

        if (CurEquipSItemDic == null)
        {
            CurEquipSItemDic = new Dictionary<STYLE_ITEM_TYPE, StylingItemInfo>();
        }

        if (PutOnEquipSItemDic == null)
        {
            PutOnEquipSItemDic = new Dictionary<STYLE_ITEM_TYPE, StylingItemInfo>();
        }

        if (EquipFactory == null)
        {
            EquipFactory = new CMemberEquipFactory(this);
        }
    }

    public void SetMemberAvatarInfo(AvatarList avatarInfo, GameObject parentObj)
    {
        SetMemberAvatarInfo(avatarInfo);

        SetBaseMemberAvatar(parentObj);
        SetCurEquipSItemDic();
        SetAvatarObjDefaultFaceTexture();
    }

    public void SetMemberAvatarInfo(AvatarList info)
    {
        MemberAvatarInfo = info;

        MemberCharacterData = CMemberAvatarDataManager.Instance.GetMemberCharacterData(MemberType);
        if (MemberCharacterData == null)
        {
            CDebug.LogError($"SetMemberAvatarInfo() MemberCharacterData is null, MemberType: {MemberType}");
        }
    }

    public AvatarList GetMemberAvatarInfo()
    {
        return MemberAvatarInfo;
    }

    public int GetFatigrade()
    {
        return MemberAvatarInfo.fatigability_grade;
    }
     

    public void SetAvatarObjDefaultFaceTexture(string faceState = FACE_FUNC_NAME.NORMAL)
    {
        MemberAvatarController.SetAvatarFaceObj(faceState);
    }

#if UNITY_EDITOR

    public void SetToolMemberAvatarInfo(AvatarList avatarInfo, GameObject parentObj, Dictionary<long, StylingItemData> itemDataDic)
    {
        MemberAvatarInfo = avatarInfo;

        SetBaseAvatarObjTool(parentObj);
        //SetCurEquipSItemDicToTool(itemDataDic);
        SetAvatarObjDefaultFaceTexture();
        
        
        if (MemberAvatarController == null)
        {
            var controller = MemberAvatarObj.GetComponent<CMemberAvatarController>();
            SetMemberAvatarController(controller);
        }
        
    }

    private void SetCurEquipSItemDicToTool(Dictionary<long, StylingItemData> itemDataDic)
    {
        SetToolCurEquipSItemDicByEachPart(itemDataDic, MemberAvatarInfo.parts1);
        SetToolCurEquipSItemDicByEachPart(itemDataDic, MemberAvatarInfo.parts2);
        SetToolCurEquipSItemDicByEachPart(itemDataDic, MemberAvatarInfo.parts3);
        SetToolCurEquipSItemDicByEachPart(itemDataDic, MemberAvatarInfo.parts4);
    }

    private void SetToolCurEquipSItemDicByEachPart(Dictionary<long, StylingItemData> itemDataDic, long partID)
    {
        if (itemDataDic.ContainsKey(partID) == false)
        {
            CDebug.LogError($"SetCurEquipSItemDicByEachPart() itemDataDic.ContainsKey(partID) is false, partID: {partID}");
            return;
        }

        // memberInfo.part1_ID
        StylingItemData _data = itemDataDic[partID];
        if (_data == null)
        {
            CDebug.LogError($"SetCurEquipSItemDicByEachPart() _data is null, partID: {partID}");
            return;
        }

        //load item object
        StylingItemInfo _itemInfo = new StylingItemInfo(_data, MemberAvatarObj);
        _itemInfo.LoadItemObj();

        SetCurEquipStylingItemDic(_data.ItemType, _itemInfo);
    }

    public void EquipAllStylingItemsForTool(float scale = 1.0f)
    {
        foreach (var item in CurEquipSItemDic)
        {
            EquipStylingItemsForTool(item.Key, item.Value, scale);
        }
    }

    public void EquipStylingItemsForTool(STYLE_ITEM_TYPE type, StylingItemInfo itemInfo, float scale = 1.0f)
    {
        GameObject dummyAccObj = GetDummyAccObj(type);
        EquipFactory.EquipParts(this, MemberAvatarObj, itemInfo, type, dummyAccObj);
        MemberAvatarObj.transform.localScale = new Vector3(scale, scale, scale);
    }

    public void DestroyAllCurEquipSItemObjForTool()
    {
        foreach (var item in CurEquipSItemDic)
        {
            Utility.Destroy(item.Value.ItemObj);
            CurEquipSItemDic[item.Key] = null;
        }
    }

    public void DestroyAllCurEquipSItemObj()
    {
        foreach (var item in CurEquipSItemDic)
        {
            Utility.Destroy(item.Value.ItemObj);
            CurEquipSItemDic[item.Key] = null;
        }
    }
    
    private void SetBaseAvatarObjTool(GameObject parentObj)
    {
        if (MemberAvatarController == null)
        {
            var controller = parentObj.GetComponentsInChildren<CMemberAvatarController>().FirstOrDefault();
            SetMemberAvatarController(controller, MEMBER_TYPE.JUNGWON);            
            SetAvatarObj(MemberAvatarController.gameObject);


            // MemberAvatarController = parentObj.GetComponentsInChildren<CMemberAvatarController>().FirstOrDefault();// MemberAvatarObj.GetComponent<CMemberAvatarController>();
            // if (MemberAvatarController != null)
            // {
            //     SetAvatarObj(MemberAvatarController.gameObject);
            //     MemberAvatarController.Init(this, MEMBER_TYPE.JUNGWON);
            // }
        }
    }
#endif

    public void SetMemberAvatarObjScale(bool forUI = false)
    {
        float scale = SNGDefines.MEMBERAVATER_BASE_SCALE;
        if (forUI)
        {
            scale = SNGDefines.MEMBERAVATER_UI_SCALE;
        }
        SetMemberAvatarObjScale(scale);
        // MemberAvatarObj.transform.localScale = new Vector3(scale, scale, scale);
    }

    public void SetMemberAvatarObjScale(float scale)
    {
        MemberAvatarObj.transform.localScale = new Vector3(scale, scale, scale);
    }

    public void SetBaseAvatarDefaultProp()
    {
        MemberAvatarObj.name = MemberType.ToString();

        if (MemberAvatarController == null)
        {
            var controller = MemberAvatarObj.GetComponent<CMemberAvatarController>();
            SetMemberAvatarController(controller);
        }
    }

    public void SetMemberAvatarController(CMemberAvatarController controller, MEMBER_TYPE mType = MEMBER_TYPE.NONE)
    {
        MemberAvatarController = controller;
        if (MemberAvatarController != null)
        {
            MemberAvatarController.Init(this, mType);        
        }
    }

    public CMemberAvatarController GetMemberAvatarController()
    {
        return MemberAvatarController;
    }

    public void SetBaseMemberAvatar(GameObject parentObj, bool forUI = false)
    {
        SetBaseMemberAvatarNotProp(parentObj);

        SetMemberAvatarProp(forUI);
    }

    public void SetBaseMemberAvatarNotProp(GameObject parentObj)
    {
        SetAvatarObj(GetBaseAvatarObj(parentObj));
    }

    public void SetTownNPCSimpleMove()
    {
        SimpleMove = MemberAvatarObj.GetComponent<TownNPCSimpleMove>();
        if (SimpleMove == null)
        {
            SimpleMove = MemberAvatarObj.AddComponent<TownNPCSimpleMove>();
            SimpleMove.SetData((int)MemberType, MemberAvatarObj.transform.position, 1, 3, MemberAvatarController);
        }
    }

    public TownNPCSimpleMove GetTownNPCSimpleMove()
    {
        return SimpleMove;
    }

    public void SetBaseMemberAvatarFromObj(GameObject parentObj, bool forUI = false)
    {
        SetAvatarObj(parentObj);

        SetMemberAvatarProp(forUI);
    }

    public void SetMemberAvatarProp(bool forUI = false)
    {
        if (MemberAvatarObj != null)
        {
            MemberAvatarObj.transform.rotation = Quaternion.Euler(0, 180, 0);
            SetBaseAvatarDefaultProp();
            SetMemberAvatarObjScale(forUI);
        }
    }


    public GameObject GetBaseAvatarObj(GameObject parentObj)
    {
        GameObject avatarObj = null;
        var resData = CResourceManager.Instance.GetResourceData(SNGDefines.MEMBER_AVATAR_BASEOBJ_PATH);
        if (resData != null)
        {
            GameObject basicBodyObj = resData.Load<GameObject>(MemberAvatarObj);
            avatarObj = Utility.AddChild(parentObj, basicBodyObj);
        }

        return avatarObj;
    }

    public async Task<CoupleData> AsyncLoadBaseAvatarObj(AvatarList avatarInfo, GameObject parentObj)
    {
        SetMemberAvatarInfo(avatarInfo);

        CoupleData coupleData = await ResourceManager.Instance.GetLoadCharAsync(EResourceType.Object, SNGDefines.MEMBER_AVATAR_BASEOBJ_PATH);
        if (coupleData != null)
        {
            SetMemberAvatarObj(coupleData.instObject, parentObj);
        }

        return coupleData;
    }

    public void LoadBaseAvatarObj(AvatarList avatarInfo, GameObject parentObj)
    {
        var resData = CResourceManager.Instance.GetResourceData(SNGDefines.MEMBER_AVATAR_BASEOBJ_PATH);
        if (resData != null)
        {
            GameObject basicBodyObj = resData.Load<GameObject>(MemberAvatarObj);
            if (basicBodyObj == null)
            {
                CDebug.LogError($"LoadBaseAvatarObj() basicBodyObj is null, path: {SNGDefines.MEMBER_AVATAR_BASEOBJ_PATH}");
                return;
            }
            SetMemberAvatarInfo(avatarInfo);
            SetMemberAvatarObj(Utility.AddChild(parentObj, basicBodyObj), parentObj);
            
        }
    }

    public void SetMemberAvatarObj(GameObject basicObj, GameObject parentObj)
    {
        SetAvatarObj(basicObj);
        MemberAvatarObj.transform.SetParent(parentObj.transform);
        MemberAvatarObj.transform.localPosition = Vector3.zero;
        float angle = UnityEngine.Random.Range(0, 360);
        MemberAvatarObj.transform.localEulerAngles = new Vector3(0, angle, 0);
        
        SetBaseAvatarDefaultProp();
        SetTownNPCSimpleMove();
        
        SetCurEquipSItemDic();
        SetAvatarObjDefaultFaceTexture();

        EquipAllStylingItems();
        
        SetMemberAvatarObjScale();
    }

    public void SetAvatarObj(GameObject avatarObj)
    {
        MemberAvatarObj = avatarObj;
    }

    public GameObject GetAvatarObj()
    {
        return MemberAvatarObj;
    }

    public void SetAvatarObjPosition(Vector3 pos)
    {
        MemberAvatarObj.transform.localPosition = pos;
    }

    //temporary: change MEMBER_TYPE from avatarid
    private long GetIDByMemberType(MEMBER_TYPE type)
    {
        return  (int)type + 1801;  //(MEMBER_TYPE)(id - 1801);
    }
     

    public void SetDefaultEquipSItem(MEMBER_TYPE mType)
    {
        SetCurEquipSItemDic();
    }

    public void SetCurEquipSItemDic()
    {
        if (MemberAvatarInfo == null) return;
        //Hair
        long hairpartID = MemberAvatarInfo.parts1;
        if (hairpartID == 0)
        {
            hairpartID = MemberCharacterData.BasicHairID;
        }
        SetCurEquipSItemDicByEachPart(hairpartID);
        //Skin
        long skinpartID = MemberAvatarInfo.parts2;
        if (skinpartID == 0)
        {
            skinpartID = MemberCharacterData.BasicSkinID;
        }
        SetCurEquipSItemDicByEachPart(skinpartID);

        if (MemberAvatarInfo.parts3 != 0) SetCurEquipSItemDicByEachPart(MemberAvatarInfo.parts3);
        if (MemberAvatarInfo.parts4 != 0) SetCurEquipSItemDicByEachPart(MemberAvatarInfo.parts4);
        if (MemberAvatarInfo.parts5 != 0) SetCurEquipSItemDicByEachPart(MemberAvatarInfo.parts5);
    }

    public void UpdateCurEquipSItemDic(AvatarList avatarInfo)
    {        
        MemberAvatarInfo = avatarInfo;
        SetCurEquipSItemByEachPartType(STYLE_ITEM_TYPE.HAIR, avatarInfo.parts1);
        SetCurEquipSItemByEachPartType(STYLE_ITEM_TYPE.SKIN, avatarInfo.parts2);
        SetCurEquipSItemByEachPartType(STYLE_ITEM_TYPE.ACC_HEAD, avatarInfo.parts3);
        SetCurEquipSItemByEachPartType(STYLE_ITEM_TYPE.ACC_FACE, avatarInfo.parts4);
        SetCurEquipSItemByEachPartType(STYLE_ITEM_TYPE.ACC_BODY, avatarInfo.parts5);
    }

    private void SetCurEquipSItemByEachPartType(STYLE_ITEM_TYPE itemType, long partID)
    {
        if (partID == 0)
        {
            if (CurEquipSItemDic.ContainsKey(itemType))
            {
                CurEquipSItemDic[itemType].UnLoadItemObj();
                CurEquipSItemDic.Remove(itemType);
            }
        }
        else
        {
            SetCurEquipSItemDicByEachPart(partID);
        }
    }

    public void SetCurEquipSItemDicByEachPart(long partID)
    {
        // memberInfo.part1_ID
        StylingItemData _data = CMemberAvatarDataManager.Instance.GetStylingItemData(partID);
        if (_data == null)
        {
            CDebug.LogError($"SetCurEquipSItemDicByEachPart() _data is null, partID: {partID}");
            return;
        }

        EquipItemByItemData(_data);
    }

    public void EquipItemByItemData(StylingItemData itemData)
    {
        //load item object
        StylingItemInfo _itemInfo = new StylingItemInfo(itemData, MemberAvatarObj);
        _itemInfo.LoadItemObj();

        SetCurEquipStylingItemDic(itemData.ItemType, _itemInfo);
    }

    public void SetCurEquipStylingItemDic(STYLE_ITEM_TYPE itemType, StylingItemInfo _itemInfo)
    {
        if (CurEquipSItemDic.ContainsKey(itemType))
        {
            if (_itemInfo.ItemData == null)
            {
                CurEquipSItemDic[itemType].UnLoadItemObj();
                CurEquipSItemDic.Remove(itemType);
            }
            else
            {
                if (CurEquipSItemDic[itemType] != null)
                {
                    if (CurEquipSItemDic[itemType].ItemData.ID != _itemInfo.ItemData.ID)
                    {
                        // last equipped item object unload
                        CurEquipSItemDic[itemType].UnLoadItemObj();
                    }
                }
                
                CurEquipSItemDic[itemType] = _itemInfo;
            }
        }
        else
        {
            if (_itemInfo.ItemData != null)
            {
                CurEquipSItemDic.Add(itemType, _itemInfo);
            }
        }
    }

    public void EquipAllStylingItems(float scale = 1.0f)
    {
        //check equip acc for unequip
        for (STYLE_ITEM_TYPE type = STYLE_ITEM_TYPE.ACC_HEAD ; type < STYLE_ITEM_TYPE.MAX; type++)
        {
            if (!CurEquipSItemDic.ContainsKey(type))
            {
                if (IsEquipedStyleItemAcc(type))
                {
                    UnEquipStyleItemAcc(type);
                }
            }
        }

        foreach (var item in CurEquipSItemDic)
        {
            EquipStylingItems(item.Key, item.Value, scale);
        }
    }

    public void EquipStyligItem(STYLE_ITEM_TYPE type, float scale = 1.0f)
    {
        if (CurEquipSItemDic.ContainsKey(type) == false) return;
        var item = CurEquipSItemDic[type];
        EquipStylingItems(type, item, scale);
    }

    public void EquipStyleItemByItemData(StylingItemData itemData, GameObject avatarObj, float scale = 1.0f)
    {
        StylingItemInfo _itemInfo = new StylingItemInfo(itemData, avatarObj);
        _itemInfo.LoadItemObj();

        EquipStylingItems(itemData.ItemType, _itemInfo, scale);
    }

    public void EquipStylingItems(STYLE_ITEM_TYPE type, StylingItemInfo itemInfo, float scale = 1.0f)
    {
        GameObject dummyAccObj = GetDummyAccObj(type);
        EquipFactory.EquipParts(this, MemberAvatarObj, itemInfo, type, dummyAccObj);
        MemberAvatarObj.transform.localScale = new Vector3(scale, scale, scale);
    }

    public bool IsEquipedStyleItemAcc(STYLE_ITEM_TYPE itemType)
    {
        return PutOnEquipSItemDic.ContainsKey(itemType);
    }

    public void UnEquipAllStyleItemAcc()
    {
        for (STYLE_ITEM_TYPE type = STYLE_ITEM_TYPE.ACC_HEAD; type <= STYLE_ITEM_TYPE.ACC_BODY; type++)
        {
            UnEquipStyleItemAcc(type);
        }
    }

    public void UnEquipStyleItemAcc(STYLE_ITEM_TYPE itemType)
    {
        if (PutOnEquipSItemDic.ContainsKey(itemType))
        {
            Utility.Destroy(PutOnEquipSItemDic[itemType].ItemObj);
            PutOnEquipSItemDic.Remove(itemType);
        }
    }

    private GameObject GetDummyAccObj(STYLE_ITEM_TYPE type)
    {
        if (MemberAvatarController == null)
        {
            CDebug.LogError("GetDummyAccObj() MemberAvatarController is null");
            return null;
        }

        return MemberAvatarController.GetDummyObj(type);
    }

    public void SetCurPutOnSItemObjDic(STYLE_ITEM_TYPE type, StylingItemInfo itemInfo)
    {
        if (PutOnEquipSItemDic.ContainsKey(type))
        {
            PutOnEquipSItemDic[type] = itemInfo;
        }
        else
        {
            PutOnEquipSItemDic.Add(type, itemInfo);
        }
    }

    public void LoadFieldAvatarObject(AvatarList avatarInfo, GameObject parentObj, float scale, float yPos)
    {        
        SetMemberAvatarInfo(avatarInfo);
        SetCurEquipSItemDic();
        SetBaseMemberAvatarNotProp(parentObj);
        SetBaseAvatarDefaultProp();
        EquipAllStylingItems();
        SetMemberAvatarObjScale(scale);
        SetAvatarObjDefaultFaceTexture();
        SetAvatarObjPosition(new Vector3(0, yPos, 0));
    }

    public Dictionary<STYLE_ITEM_TYPE, StylingItemInfo> GetCurPutOnSItemDic()
    {
        return PutOnEquipSItemDic;
    }

    public GameObject GetCurPunOnSItemObjByType(STYLE_ITEM_TYPE type)
    {
        if (PutOnEquipSItemDic.ContainsKey(type))
        {
            return PutOnEquipSItemDic[type].ItemObj;
        }

        return null;
    }

    public void DestroyCurPonItem(STYLE_ITEM_TYPE type)
    {
        if (PutOnEquipSItemDic.ContainsKey(type))
        {
            var obj = PutOnEquipSItemDic[type].ItemObj;

            if (obj != null)
            {
                if (obj.scene.IsValid())
                    GameObject.Destroy(obj);
            }

            PutOnEquipSItemDic.Remove(type);
        }
    }

    // public void PutOnItemsToCurEquipSItemDic(STYLE_ITEM_TYPE type)
    // {
    //     if (PutOnEquipSItemDic == null || PutOnEquipSItemDic.Count == 0)
    //     {
    //         return;
    //     }
        
    //     if (CurEquipSItemDic.ContainsKey(type))
    //     {
    //         CurEquipSItemDic[type] = PutOnEquipSItemDic[type];
    //     }
    // }
     

    public bool IsDifferentCurWithPutOn()
    {
        if (PutOnEquipSItemDic == null || PutOnEquipSItemDic.Count == 0 ||
            CurEquipSItemDic == null || CurEquipSItemDic.Count == 0)
        {
            return false;
        }

        if (PutOnEquipSItemDic.Count != CurEquipSItemDic.Count)
        {
            return true;
        }

        foreach (var pair in PutOnEquipSItemDic)
        {
            if (!CurEquipSItemDic.TryGetValue(pair.Key, out var curStyleItem))
            {
                return true;
            }

            CDebug.Log($"IsDifferentCurWithPutOn() {pair.Key} / cur: {curStyleItem.ItemData.ID}, puton: {pair.Value.ItemData.ID}");
            if (curStyleItem.ItemData.ID != pair.Value.ItemData.ID)
            {
                return true;
            }
        }

        return false;
    }

    // return true if item is not in CurEquipSItemDic
    public bool IsCurrentEquipItem(StylingList itemInfo)
    {
        StylingItemData itemData = CMemberAvatarDataManager.Instance.GetStylingItemData(itemInfo.style_id);

        if (CurEquipSItemDic != null && CurEquipSItemDic.TryGetValue(itemData.ItemType, out StylingItemInfo curEquipItemInfo))
        {
            StylingItemData curEquipItemData = curEquipItemInfo.ItemData;

            if (curEquipItemData.ID == itemData.ID)
            {
                return true;
            }
        }

        return false;
    }

    public StyleitemUIinfo GetEquipItemUIInfo(STYLE_ITEM_TYPE itemType, int tabIdx)
    {
        if (CurEquipSItemDic.ContainsKey(itemType))
        {
            int itemID = CurEquipSItemDic[itemType].ItemData.ID;
            StyleitemUIinfo itemUIInfo = new StyleitemUIinfo();
            itemUIInfo.StyleInfo = CStylingItemInvenManager.Instance.GetStyleItemByID(tabIdx, MemberType, itemID);
            itemUIInfo.StyleItemData = CurEquipSItemDic[itemType].ItemData; 
            itemUIInfo.StyleGoodsData = CMemberAvatarDataManager.Instance.GetShopGoodsData(itemID);
            return itemUIInfo;
        }
        return null;
    }

    public void SetOutLine()
    {
        SimpleMove.SetOutLine();
    }

    public void ClearEquipItem()
    {
        if (CurEquipSItemDic != null)
        {
            CurEquipSItemDic.Clear();
        }

        if (PutOnEquipSItemDic != null)
        {
            PutOnEquipSItemDic.Clear();
        }
    }
     

    public void DestroyMemberAvatarObj()
    {
        if (MemberAvatarObj != null)
        {
            GameObject.Destroy(MemberAvatarObj);
        }
    }

    public void Release()
    {
        if (PutOnEquipSItemDic != null)
            {
                PutOnEquipSItemDic.Clear();
                PutOnEquipSItemDic = null;
            }

        if (CurEquipSItemDic != null)
        {
            CurEquipSItemDic.Clear();
            CurEquipSItemDic = null;
        }

        if (EquipFactory != null)
        {
            EquipFactory = null;
        }

        if (MemberAvatarObj != null)
        {
            GameObject.Destroy(MemberAvatarObj);
            MemberAvatarObj = null;
        }
    }
     
}



