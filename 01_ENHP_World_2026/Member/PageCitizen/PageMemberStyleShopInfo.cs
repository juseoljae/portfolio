using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Game.RestAPI;
using UniRx;
using CitizenUI;

public class PageMemberStyleShopInfo : MonoBehaviour
{
    [SerializeField] private UIObjectTouchRotation TouchRotation;
    [SerializeField] private Button BuyBtn;
    [SerializeField] private Button DisableBuyBtn;
    [SerializeField] private GameObject BuyBtnDisableObj;
    [SerializeField] private Button ResetBtn;
    [SerializeField] private GameObject ResetBtnDisableObj;
    [SerializeField] private Button SlotOnBtn;
    [SerializeField] private Button SlotOffBtn;
    [SerializeField] private ObjShopSlotItem[] SlotObjs;

    private PageCitizenMiddleUI MiddleUI;
    private MemberStyleShopItemList ShopItemList = null;
    private Dictionary<STYLE_ITEM_TYPE, int> slotIndexDic = null;
    private MEMBER_TYPE CurrentMemberType = MEMBER_TYPE.NONE;
    private Dictionary<MEMBER_TYPE, CMemberAvatar> MemberAvatarDic;
    private Action RefreshSubTabNewAction = null;
    private Dictionary<STYLE_ITEM_TYPE, StyleitemUIinfo> RegistedItemSlotDic;
    private SingleAssignmentDisposable SlotOnBtnDisposable = new SingleAssignmentDisposable();
    private SingleAssignmentDisposable SlotOffBtnDisposable = new SingleAssignmentDisposable();
    private SingleAssignmentDisposable BuyBtnDisposable = new SingleAssignmentDisposable();
    private SingleAssignmentDisposable DisableBuyBtnDisposable = new SingleAssignmentDisposable();
    private SingleAssignmentDisposable ResetBtnDisposable = new SingleAssignmentDisposable();


    public void Init(PageCitizenMiddleUI middleUI,MemberStyleShopItemList itemList, MEMBER_TYPE memberType, Action subTabNewAction, bool isChangeMember = false)
    {
        MiddleUI = middleUI;
        ShopItemList = itemList;

        RefreshSubTabNewAction = subTabNewAction;

        if (MemberAvatarDic == null)
        {
            MemberAvatarDic = new Dictionary<MEMBER_TYPE, CMemberAvatar>();
        }

        if (RegistedItemSlotDic == null)
        {
            RegistedItemSlotDic = new Dictionary<STYLE_ITEM_TYPE, StyleitemUIinfo>();
        }

        SetSlotOnOffBtn(isChangeMember);
        SetSaveResetBtn();
        if (!isChangeMember) SetActiveSlotOnBtn(false);

        SetUICharacter(memberType);

        for (MEMBER_TYPE member = MEMBER_TYPE.JUNGWON; member <= MEMBER_TYPE.MAX; member++)
        {
            if (MemberAvatarDic.ContainsKey(member) == false)
            {
                AvatarList _info = CMemberAvatarServerDataManager.Instance.GetMemberAvatarInfo((int)member);
                CMemberAvatar avatar = new CMemberAvatar(member);
                avatar.SetMemberAvatarInfo(_info);
                avatar.SetCurEquipSItemDic();
                MemberAvatarDic.Add(member, avatar);
            }

        }

        SetActiveSaveDisableObj(true);
        SetActiveResetDisableObj(true);

        SetSlotButtons();

        SetMemberObj();

    }
    
    
    public void SetUICharacter(MEMBER_TYPE memberType)
    {
        if (MiddleUI.UICharacter == null)
        {
            MiddleUI.UICharacter = this.gameObject.AddComponent<UICharacterController>();
        }

        MiddleUI.UICharacter.SetUICharacterBase();

        if (TouchRotation != null)
        {
            TouchRotation.Initialized(MiddleUI.UICharacter.GetRotationObject);
            TouchRotation.SetOnClickEvent = OnClickCharacter;
        }
        
        SetMemberType(memberType);
    }

    private void SetSlotOnOffBtn(bool isChangeMember)
    {
        if (!isChangeMember) OnClickActiveSlot(true);

        if (SlotOnBtnDisposable != null)
        {
            SlotOnBtnDisposable.Dispose();
        }
        SlotOnBtnDisposable = new SingleAssignmentDisposable();
        SlotOnBtnDisposable.Disposable = SlotOnBtn.BindToOnClick(_ =>
        {
            OnClickActiveSlot(true);
            return Observable.Timer(System.TimeSpan.FromSeconds(CDefines.BUTTONDELAYTIME)).AsUnitObservable();
        }).AddTo(this);

        if (SlotOffBtnDisposable != null)
        {
            SlotOffBtnDisposable.Dispose();
        }
        SlotOffBtnDisposable = new SingleAssignmentDisposable();
        SlotOffBtnDisposable.Disposable = SlotOffBtn.BindToOnClick(_ =>
        {
            OnClickActiveSlot(false);
            return Observable.Timer(System.TimeSpan.FromSeconds(CDefines.BUTTONDELAYTIME)).AsUnitObservable();
        }).AddTo(this);

    }

    private void SetSaveResetBtn()
    {
        if (BuyBtnDisposable != null)
        {
            BuyBtnDisposable.Dispose();
        }
        BuyBtnDisposable = new SingleAssignmentDisposable();
        BuyBtnDisposable.Disposable = BuyBtn.BindToOnClick(_ =>
        {
            OnClickBuyBtn();
            return Observable.Timer(System.TimeSpan.FromSeconds(CDefines.BUTTONDELAYTIME)).AsUnitObservable();
        }).AddTo(this);

        if (DisableBuyBtnDisposable != null)
        {
            DisableBuyBtnDisposable.Dispose();
        }
        DisableBuyBtnDisposable = new SingleAssignmentDisposable();
        DisableBuyBtnDisposable.Disposable = DisableBuyBtn.BindToOnClick(_ =>
        {
            OnClickDisableBuyBtn();
            return Observable.Timer(System.TimeSpan.FromSeconds(CDefines.BUTTONDELAYTIME)).AsUnitObservable();
        }).AddTo(this);

        if (ResetBtnDisposable != null)
        {
            ResetBtnDisposable.Dispose();
        }
        ResetBtnDisposable = new SingleAssignmentDisposable();
        ResetBtnDisposable.Disposable = ResetBtn.BindToOnClick(_ =>
        {
            OnClickResetBtn();
            return Observable.Timer(System.TimeSpan.FromSeconds(CDefines.BUTTONDELAYTIME)).AsUnitObservable();
        }).AddTo(this);
    }

    public void SetMemberType(MEMBER_TYPE mType)
    {
        CurrentMemberType = mType;
    }

    public MEMBER_TYPE GetCurrentMemberType()
    {
        return CurrentMemberType;
    }

    public CMemberAvatar GetCurrentMemberAvatar()
    {
        if (MemberAvatarDic.ContainsKey(CurrentMemberType))
        {
            return MemberAvatarDic[CurrentMemberType];
        }

        CDebug.LogError($"CurrentMemberType {CurrentMemberType} not found in MemberAvatarDic.");
        return null;
    }

    public void SetMemberObj()
    {
        CMemberAvatar curMemberAvatar = GetCurrentMemberAvatar();
        if (curMemberAvatar == null) return;
        DestroyMemberAvatarObj();
        MiddleUI.UICharacter.SetMember(curMemberAvatar);
    }

    private void SetActiveSlotOnBtn(bool isActive)
    {
        SlotOnBtn.gameObject.SetActive(isActive);
        SlotOffBtn.gameObject.SetActive(!isActive);
    }

    private List<StyleitemUIinfo> GetSaleItemInfos()
    {
        List<StyleitemUIinfo> saleItemInfos = new List<StyleitemUIinfo>();
        foreach (var obj in SlotObjs)
        {
            StyleitemUIinfo itemInfo = obj.GetItemUIInfo();
            if (itemInfo != null && itemInfo.StyleGoodsData != null && itemInfo.StyleGoodsData.isSale)
            {
                if (itemInfo.StyleInfo.having == 0)
                {
                    saleItemInfos.Add(itemInfo);
                }
            }
        }

        return saleItemInfos;
    }



    private void SetActiveResetBtn(StyleitemUIinfo itemInfo)
    {
        CMemberAvatar curMemberAvatar = GetCurrentMemberAvatar();
        if (curMemberAvatar != null)
        {
            //초기화 버튼 관리
            bool diff = curMemberAvatar.IsDifferentCurWithPutOn();   
            SetActiveResetDisableObj(!diff);

            //구매 버튼 관리
            List<StyleitemUIinfo> sailItemInfos = GetSaleItemInfos();
            SetActiveSaveDisableObj(sailItemInfos.Count <= 0);
        }
    }
    
    private void SetActiveSaveDisableObj(bool isActive)
    {
        BuyBtnDisableObj.SetActive(isActive);
    }
    private void SetActiveResetDisableObj(bool isActive)
    {
        ResetBtnDisableObj.SetActive(isActive);
    }

    private void SetSlotIndexDic()
    {
        if (slotIndexDic == null)
        {
            slotIndexDic = new Dictionary<STYLE_ITEM_TYPE, int>
            {
                { STYLE_ITEM_TYPE.ACC_HEAD, 0 },
                { STYLE_ITEM_TYPE.HAIR, 1 },
                { STYLE_ITEM_TYPE.ACC_FACE, 2 },
                { STYLE_ITEM_TYPE.SKIN, 3 },
                { STYLE_ITEM_TYPE.ACC_BODY, 4 }
            };
        }

        foreach (KeyValuePair<STYLE_ITEM_TYPE, int> kvp in slotIndexDic)
        {
            SlotObjs[kvp.Value].SetDefaultSlotIconImage(kvp.Key);
        }

        for (int i = 0; i < SlotObjs.Length; i++)
            SlotObjs[i].SetSlotIcon(null);
    }

    private void SetSlotButtons()
    {
        SetSlotIndexDic();

        List<StylingList> purOnList = GetPutOnList();
        for (int i = 0; i < purOnList.Count; i++)
        {
            StyleitemUIinfo itemUIInfo = new StyleitemUIinfo();
            StylingList item = purOnList[i];
            StylingItemData itemData = CMemberAvatarDataManager.Instance.GetStylingItemData(item.style_id);

            if (itemData != null && slotIndexDic.ContainsKey(itemData.ItemType))
            {
                itemUIInfo.StyleItemData = itemData;
                itemUIInfo.StyleInfo = item;
                itemUIInfo.StyleGoodsData = CMemberAvatarDataManager.Instance.GetShopGoodsData(itemData.ID);
                SlotObjs[slotIndexDic[itemData.ItemType]].Init(itemUIInfo, CurrentMemberType, OnClickSlot, UpdateShopScrollItems, ResetPutonItem);
            }
        }

        for (int i = 0; i < SlotObjs.Length; i++)
        {
            MEMBER_TYPE slotMemberType = SlotObjs[i].GetMemberType();
            if (slotMemberType == MEMBER_TYPE.NONE)
            {
                SlotObjs[i].Init(null, MEMBER_TYPE.NONE);
            }
        }
    }

    private List<StylingList> GetEquipItemList()
    {
        List<StylingList> EquipItemList = new List<StylingList>();

        CMemberAvatar curMemberAvatar = GetCurrentMemberAvatar();
        var curEquipItems = curMemberAvatar.CurEquipSItemDic;
        foreach (var item in curEquipItems.Values)
        {

        }

        List<StylingList> styleList = CStylingItemInvenManager.Instance.GetStyleItemListByTab(2, CurrentMemberType);
        if (styleList != null)
        {
            foreach (StylingList item in styleList)
            {
                if (IsContainStylingList(EquipItemList, item.style_id) == false)
                {
                    if (item.puton == 1)
                    {
                        EquipItemList.Add(item);
                    }

                }
            }
        }

        return EquipItemList;
    }

    private List<StylingList> GetPutOnList()
    {
        List<StylingList> putOnList = new List<StylingList>();

        CMemberAvatar curMemberAvatar = GetCurrentMemberAvatar();
        var curPutOnItems = curMemberAvatar.GetCurPutOnSItemDic();
        foreach (var item in curPutOnItems.Values)
        {
            StylingList listItem = CStylingItemInvenManager.Instance.GetStyleItemByID(2, curMemberAvatar.MemberType, item.ItemData.ID);
            if (listItem != null)
            {
                putOnList.Add(listItem);
            }
        }

        List<StylingList> styleList = CStylingItemInvenManager.Instance.GetStyleItemListByTab(2, CurrentMemberType);
        if (styleList != null)
        {
            foreach (StylingList item in styleList)
            {
                if (IsContainStylingList(putOnList, item.style_id) == false)
                {
                    if (item.puton == 1)
                    {
                        putOnList.Add(item);
                    }

                }
            }
        }

        return putOnList;
    }

    private bool IsContainStylingList(List<StylingList> putOnList, int id)
    {
        foreach (var item in putOnList)
        {
            if (item.style_id == id)
            {
                return true;
            }
        }

        return false;
    }

    public void RegistStyleItemToSlot(StyleitemUIinfo item)
    {
        STYLE_ITEM_TYPE itemType = item.StyleItemData.ItemType;
        int slotIdx = slotIndexDic[itemType];
        
        if (RegistedItemSlotDic.ContainsKey(itemType))
        {
            RegistedItemSlotDic[itemType] = item;
        }
        else
        {
            RegistedItemSlotDic.Add(itemType, item);
        }

        SlotObjs[slotIdx].SetSlotIcon(item, OnClickSlot, UpdateShopScrollItems, ResetPutonItem);

        CMemberAvatar curMemberAvatar = GetCurrentMemberAvatar();
        if (curMemberAvatar != null)
        {
            //release acc
            switch (itemType)
            {
                case STYLE_ITEM_TYPE.ACC_HEAD:
                case STYLE_ITEM_TYPE.ACC_FACE:
                case STYLE_ITEM_TYPE.ACC_BODY:
                    bool isEquiped = curMemberAvatar.IsEquipedStyleItemAcc(itemType);
                    if (isEquiped)
                    {
                        UnEquipStyleItemAcc(itemType);
                    }
                    break;
            }

            //equip
            GameObject avatarObj = curMemberAvatar.MemberAvatarObj;
            curMemberAvatar.EquipStyleItemByItemData(item.StyleItemData, avatarObj);

            SetActiveResetBtn(item);
            CSoundControl.Instance.EffectPlay(SOUND_ID.SFX_SNG_CLOTHES);
            MiddleUI.UICharacter.SwapWearAnimation(itemType);
        }
    }

    public void ReleaseStyleItemFromSlot(STYLE_ITEM_TYPE itemType)
    {
        if (slotIndexDic.ContainsKey(itemType))
        {
            int slotIdx = slotIndexDic[itemType];
            SlotObjs[slotIdx].SetSlotIcon(null);
        }
        UnEquipStyleItemAcc(itemType);
    }

    private StylingList GetServerItemInfoByItemID(List<StylingList> infos, int itemID)
    {
        foreach (var info in infos)
        {
            if (info.style_id == itemID)
            {
                return info;
            }
        }
        return null;
    }

    public void UpdateMemberSlotItems(List<StylingList> infos)
    {
        foreach (var slotItem in RegistedItemSlotDic.Values)
        {
            StylingList info = GetServerItemInfoByItemID(infos, slotItem.StyleItemData.ID);
            if (info != null)
            {
                slotItem.StyleInfo.having = info.having;
                UpdateMemberSlotItem(slotItem);
            }
        }
    } 

    public void UpdateMemberSlotItem(StyleitemUIinfo itemInfo)
    {
        if (itemInfo != null)
        {
            STYLE_ITEM_TYPE itemType = itemInfo.StyleItemData.ItemType;
            if (slotIndexDic.ContainsKey(itemType))
            {
                int slotIdx = slotIndexDic[itemType];
                SlotObjs[slotIdx].SetIconStatus(itemInfo);
            }
        }
    }

    private void UnEquipStyleItemAcc(STYLE_ITEM_TYPE itemType)
    {
        CMemberAvatar curMemberAvatar = GetCurrentMemberAvatar();
        if (curMemberAvatar != null)
        {
            curMemberAvatar.UnEquipStyleItemAcc(itemType);
        }
    }

    public void UpdateMemberAvatarCurItemInfo(STYLE_ITEM_TYPE type)
    {
        Dictionary<STYLE_ITEM_TYPE, long> changedPartID;
        int partID = -1;
        // get different part id 
        AvatarList avatarInfo = CMemberAvatarManager.Instance.GetAvatarListToSaveStyleItemByItemType(type, GetCurrentMemberAvatar(), out partID);
        if (avatarInfo != null)
        {
            AvatarList avatarList = CMemberAvatarServerDataManager.Instance.GetMemberAvatarInfo((int)GetCurrentMemberType());
            if (avatarList != null)
            {
                CMemberAvatarManager.Instance.UpdateMemberAvatarCurEquipSItem(avatarList);
                if (partID != -1)
                {
                    UpdateAvatarPutOnItems(new List<long> { partID });
                }

                CCoreServices.GetCoreService<CPopupService>().NoticeMessageDisposable("smg_style_save");
            }
        }
    }
    
    //update ui avatar put on items
    public void UpdateAvatarPutOnItems(List<long> saveItemIDs)//(Dictionary<STYLE_ITEM_TYPE, int> changedPartID)
    {
        GameObject avatarObj = MiddleUI.UICharacter.GetUICharacterBase();
        if (avatarObj == null)
        {
            CDebug.LogError("Avatar object is null. Cannot update put on items.");
            return;
        }

        CMemberAvatar curMemberAvatar = GetCurrentMemberAvatar();
        if (curMemberAvatar != null)
        {
            foreach (var item in saveItemIDs)
            {
                StylingItemData itemData = CMemberAvatarDataManager.Instance.GetStylingItemData(item);
                if (itemData != null)
                {
                    StylingItemInfo _itemInfo = new StylingItemInfo(itemData, avatarObj);
                    //_itemInfo.LoadItemObj();
                    SetCurEquipItemOfMemberAvatar(CurrentMemberType, _itemInfo);
                }
            }
        }
    }

    public void UpdateAvatarPutOnItems(List<StylingList> itemInfos)
    {
        AvatarList avatarList = CMemberAvatarServerDataManager.Instance.GetMemberAvatarInfo((int)GetCurrentMemberType());
        if (avatarList != null)
        {
            UpdateAvatarCurEquip(avatarList, itemInfos);
        }
    }

    
    private void UpdateAvatarCurEquip(AvatarList avatarList, List<StylingList> itemInfos)
    {
        GameObject avatarObj = MiddleUI.UICharacter.GetUICharacterBase();
        if (avatarObj == null)
        {
            CDebug.LogError("Avatar object is null. Cannot update put on items.");
            return;
        }

        CMemberAvatar curMemberAvatar = GetCurrentMemberAvatar();
        if (curMemberAvatar != null)
        {
            Dictionary<STYLE_ITEM_TYPE, StylingItemData> curEquipItems = CMemberAvatarDataManager.Instance.GetAllPartsStylingItemDataByAvatarList(avatarList, slotIndexDic);

            foreach (var item in curEquipItems)
            {
                StylingItemInfo _itemInfo = new StylingItemInfo(item.Value, avatarObj);
                curMemberAvatar.SetCurEquipStylingItemDic(item.Key, _itemInfo);
            }

            CStylingItemInvenManager.Instance.UpdateStyleItemListByTab(2, CurrentMemberType, itemInfos);

            SetSlotButtons();
        }
    }
    // Set current member type for UI avatar
    private void SetCurEquipItemOfMemberAvatar(MEMBER_TYPE mType, StylingItemInfo itemInfo)
    {
        if (MemberAvatarDic.ContainsKey(mType))
        {
            MemberAvatarDic[mType].SetCurEquipStylingItemDic(itemInfo.ItemData.ItemType, itemInfo);
        }
    }

    private void OnClickSlot(StyleitemUIinfo itemInfo)
    {
        ShopItemList.OnClickItem(itemInfo);
    }

    public void UpdateBuyResetButtonStatus()
    {
        bool isNotHavingItem = false;
        foreach (ObjShopSlotItem slotItem in SlotObjs)
        {
            StylingList info = slotItem.GetStylingItemInfo();
            if (info != null)
            {
                var item_ui_info = slotItem.GetItemUIInfo();
                if (info.having == 0 && (item_ui_info != null && item_ui_info.StyleGoodsData != null && item_ui_info.StyleGoodsData.isSale))
                {
                    isNotHavingItem = true;
                    break;
                }
            }
        }

        if (isNotHavingItem)
        {
            SetActiveSaveDisableObj(false);
            SetActiveResetDisableObj(false);
        }
        else // all items purchased in slot
        {
            bool isNotCurItem = false;
            CMemberAvatar curMemberAvatar = GetCurrentMemberAvatar();
            if (curMemberAvatar != null)
            {
                foreach (ObjShopSlotItem slotItem in SlotObjs)
                {
                    StylingList info = slotItem.GetStylingItemInfo();
                    if (info != null)
                    {
                        bool isCurItem = curMemberAvatar.IsCurrentEquipItem(info);
                        if (!isCurItem)
                        {
                            isNotCurItem = true;
                            break;
                        }
                    }                    
                }
            }

            if (isNotCurItem)
            {
                SetActiveSaveDisableObj(true);
                SetActiveResetDisableObj(false);
            }
            else
            {
                SetActiveSaveDisableObj(true);
                SetActiveResetDisableObj(true);
            }
        }
    }

    private void UpdateShopScrollItemsByBasket(List<StyleitemUIinfo> itemInfos)
    {
        foreach (var item in itemInfos)
        {
            ShopItemList.UpdateShopScrollItemsAndRefresh(item);
        }
        UpdateBuyResetButtonStatus();
        
    }

    private void UpdateShopScrollItems()
    {
        ShopItemList.UpdateShopScrollItemsAndRefresh(null);
        UpdateBuyResetButtonStatus();
    }

    private void UpdateCurrentMemberStyleItems(List<long> saveItemIDs)
    {
        ShopItemList.SavePurchasedItems(saveItemIDs);
    }

    private void UpdateCurMemberStyleItems(List<StylingList> items)
    {
        ShopItemList.SavePurchasedItems(items);
        
        CMemberAvatar curMemberAvatar = GetCurrentMemberAvatar();
        if (curMemberAvatar != null)
        {
            //초기화 버튼 관리
            bool diff = curMemberAvatar.IsDifferentCurWithPutOn();
            SetActiveResetDisableObj(!diff);
        }
    }


    private void OnClickCharacter()
    {
        MiddleUI.UICharacter.PlayMemberTouchSignatureAnimation();
    }

    private void OnClickActiveSlot(bool bActive)
    {
        foreach (ObjShopSlotItem slot in SlotObjs)
        {
            slot.SetActiveSlotBtn(bActive);
        }

        SetActiveSlotOnBtn(!bActive);
    }

    private void OnClickBuyBtn()
    {
        BuyItems();
    }
    

    private void OnClickDisableBuyBtn()
    {
        if (BuyBtnDisableObj.activeSelf)
        {
            CCoreServices.GetCoreService<CPopupService>().NoticeMessageDisposable("smg_buy_noitem");
        }
    }
    

    private void BuyItems()
    {
        List<StyleitemUIinfo> basketItems = new List<StyleitemUIinfo>();

        APIHelper.SNGService.ReqSNG_AvatarStyling_having(((int)CurrentMemberType).ToString())
        .Subscribe(recvData =>
        {
            CMemberAvatar curMemberAvatar = GetCurrentMemberAvatar();
            if (curMemberAvatar != null)
            {
                foreach (ObjShopSlotItem slotItem in SlotObjs)
                {
                    StylingList info = slotItem.GetStylingItemInfo();
                    if (info != null)
                    {
                        StylingList invenInfo = CStylingItemInvenManager.Instance.GetStylingListByID(info.style_id);
                        if (invenInfo == null)
                        {
                            StyleShopGoodsData goodsData = CMemberAvatarDataManager.Instance.GetShopGoodsData(info.style_id);
                            if (goodsData.isSale)
                            {
                                StyleitemUIinfo uiItemInfo = new StyleitemUIinfo();
                                uiItemInfo.StyleItemData = CMemberAvatarDataManager.Instance.GetStylingItemData(info.style_id);
                                uiItemInfo.StyleGoodsData = goodsData;
                                basketItems.Add(uiItemInfo);
                            }
                        }
                    }
                }

                CPopupService popupService = CCoreServices.GetCoreService<CPopupService>();
                var popup = popupService.OpenPopupStyleItemShopBasket(new PopupStyleItemShopBasket.Setting()
                {
                    BasketItems = basketItems,
                    OnClickEvent = OnClickItem,
                    ConfirmPurchaseEvent = null,
                    RefreshEvent = UpdateMemberSlotItems,
                    SlotEvent = UpdateShopScrollItemsByBasket,
                    SlotSaveEvent = UpdateCurMemberStyleItems,
                    RefreshSubTabNewEvent = RefreshSubTabNewAction,
                    CurMemberType = CurrentMemberType,
                    UnEquipEvt = ResetPutonItems
                });

                SingleAssignmentDisposable disposable = new SingleAssignmentDisposable();
                disposable.Disposable = popup.ShowAsObservable().Subscribe(res =>
                {
                    disposable.Dispose();
                });
            }

        }).AddTo(this);
    }

    public void SetPutonItemsFromEquipItems(MEMBER_TYPE mType)
    {
        if (MemberAvatarDic.ContainsKey(mType))
        {
            CMemberAvatar memberAvatar = MemberAvatarDic[mType];
            memberAvatar.PutOnEquipSItemDic.Clear();
            foreach (var item in memberAvatar.CurEquipSItemDic)
            {
                memberAvatar.PutOnEquipSItemDic.Add(item.Key, item.Value);
            }

        }
    }

    private void OnClickResetBtn()
    {
        CMemberAvatar curMemberAvatar = GetCurrentMemberAvatar();
        if (curMemberAvatar != null)
        {
            bool bDiff = curMemberAvatar.IsDifferentCurWithPutOn();
            if (bDiff)
            {
                OpenResetPopup();
            }
            else
            {

                CCoreServices.GetCoreService<CPopupService>().NoticeMessageDisposable("smg_noreset");
            }
        }
    }

    public void OnClickItem(List<StyleitemUIinfo> itemInfos)
    {
        foreach (var item in itemInfos)
        {
            ShopItemList.OnClickItem(item); //as StyleitemUIinfo
        }

    }
    
    private void OpenResetPopup()
    {
        var title = CStringDataManager.Instance.GetStringData("button_check");
        var msg = CStringDataManager.Instance.GetStringData("ui_style_reset");

        var notice = CCoreServices.GetCoreService<CPopupService>().OpenPopup_CommonAlert(title, msg, PopupAlert.BUTTON_TYPE.BTN_TWO);
        SingleAssignmentDisposable noticeDispose = new SingleAssignmentDisposable();
        noticeDispose.Disposable = notice.ShowAsObservable()
            .Do(result =>
            {
                if (result.IsSucess)
                {
                    CMemberAvatar curMemberAvatar = GetCurrentMemberAvatar();
                    if (curMemberAvatar != null)
                    {
                        // unquip all acc
                        curMemberAvatar.UnEquipAllStyleItemAcc();
                        //change Hair, skin to current equip
                        // equip all add by current equip
                        curMemberAvatar.EquipAllStylingItems();
                        
                        SetActiveSaveDisableObj(true);
                        SetActiveResetDisableObj(true);

                        SetSlotButtons();
                    }
                }
                noticeDispose.Dispose();
            }).Subscribe().AddTo(this);
    }

    public void ResetPutonItems(List<StyleitemUIinfo> itemInfos)
    {
        foreach (var item in itemInfos)
        {
            ResetPutonItem(item);
        }
    }

    public void ResetPutonItem(StyleitemUIinfo iteminfo)
    {
        CMemberAvatar curAvatar = GetCurrentMemberAvatar();
        if (curAvatar != null && iteminfo != null && iteminfo.StyleItemData != null)
        {
            STYLE_ITEM_TYPE itemType = iteminfo.StyleItemData.ItemType;

            //item slot
            StyleitemUIinfo item = curAvatar.GetEquipItemUIInfo(itemType, 2);
            if (item != null)
            {
                RegistStyleItemToSlot(item);
            }
            else
            {
                if (itemType == STYLE_ITEM_TYPE.ACC_HEAD || itemType == STYLE_ITEM_TYPE.ACC_FACE || itemType == STYLE_ITEM_TYPE.ACC_BODY)
                {
                    ReleaseStyleItemFromSlot(itemType);
                }
            }

            curAvatar.EquipAllStylingItems();
        }
    }

    private void DestroyMemberAvatarObj()
    {
        if (MemberAvatarDic != null)
        {
            foreach (CMemberAvatar member in MemberAvatarDic.Values)
            {
                if (member.GetAvatarObj() != null)
                {
                    member.DestroyMemberAvatarObj();
                }
            }

        }

    }


    public void Release()
    {
        DestroyMemberAvatarObj();
        // if (UICharacter != null)
        // {
        //     UICharacter.ReleaseBaseCharObj();
        // }

        if (MemberAvatarDic != null)
        {
            foreach (CMemberAvatar member in MemberAvatarDic.Values)
            {
                member.ClearEquipItem();
            }
            MemberAvatarDic.Clear();
        }

        if (slotIndexDic != null)
        {
            slotIndexDic.Clear();
            slotIndexDic = null;
        }

        if (RegistedItemSlotDic != null)
        {
            RegistedItemSlotDic.Clear();
            RegistedItemSlotDic = null;
        }
    }
}
