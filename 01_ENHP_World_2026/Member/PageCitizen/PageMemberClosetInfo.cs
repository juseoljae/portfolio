using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Game.RestAPI;
using UniRx;
using System.Linq;
using CitizenUI;
using System;

public class PageMemberClosetInfo : MonoBehaviour
{
    [SerializeField] private UIObjectTouchRotation TouchRotation;
    [SerializeField] private Button SaveBtn;
    [SerializeField] private GameObject SaveBtnDisableObj;
    [SerializeField] private Button ResetBtn;
    [SerializeField] private GameObject ResetBtnDisableObj;
    [SerializeField] private Button SlotOnBtn;
    [SerializeField] private Button SlotOffBtn;
    [SerializeField] private ObjSlotItem[] SlotObjs;
    private GameObject RawImgObj;
    private RectTransform RawImgRT;
    private RawImage RawImg;
    private float RawImgPosX;
    private float PrevRawImgPosX;
    private Dictionary<STYLE_ITEM_TYPE, int> slotIndexDic = null;
    private MEMBER_TYPE CurrentMemberType = MEMBER_TYPE.NONE;
    private Dictionary<MEMBER_TYPE, CMemberAvatar> MemberAvatarDic = new Dictionary<MEMBER_TYPE, CMemberAvatar>();
    private MemberStyleItemList ClosetItemInfos;
    private SingleAssignmentDisposable SlotOnBtnDisposable = new SingleAssignmentDisposable();
    private SingleAssignmentDisposable SlotOffBtnDisposable = new SingleAssignmentDisposable();
    private SingleAssignmentDisposable SaveBtnDisposable = new SingleAssignmentDisposable();
    private SingleAssignmentDisposable ResetBtnDisposable = new SingleAssignmentDisposable();
    private SingleAssignmentDisposable WaitAvatarObjLoadDisposable = new SingleAssignmentDisposable();
    private SingleAssignmentDisposable WaitAvatarObjActiveDisposable = new SingleAssignmentDisposable();
    private PageCitizenMiddleUI MiddleUI;
    private PageClosetInfo ClosetInfo;

    public void Init(MemberStyleItemList itemList, MEMBER_TYPE memberType, PageCitizenMiddleUI middleUI, PageClosetInfo closetInfo, bool isChangeMember = false)
    {
        ClosetItemInfos = itemList;
        MiddleUI = middleUI;
        ClosetInfo = closetInfo;

        SetSlotOnOffBtn(isChangeMember);
        SetSaveResetBtn();
        if (!isChangeMember)
            SetActiveSlotOnBtn(false);

        SetUICharacter(memberType);

        //for ui avatar
        for (MEMBER_TYPE member = MEMBER_TYPE.JUNGWON; member <= MEMBER_TYPE.MAX; member++)
        {
            if (MemberAvatarDic.ContainsKey(member) == false)
            {
                CMemberAvatar avatar = new CMemberAvatar(member);
                MemberAvatarDic.Add(member, avatar);
            }

            AvatarList _info = CMemberAvatarServerDataManager.Instance.GetMemberAvatarInfo((int)member);
            if (_info != null)
            {
                MemberAvatarDic[member].SetMemberAvatarInfo(_info);
                MemberAvatarDic[member].SetCurEquipSItemDic();
            }
        }


        SetSlotButtons();

        SetActiveSaveResetDisableObj(true);

        SetMemberObj();

    }

    public void SetUICharacter(MEMBER_TYPE memberType)
    {
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
        if (SlotOnBtnDisposable.IsDisposed == false)
        {
            SlotOnBtnDisposable.Dispose();
        }
        SlotOnBtnDisposable = new SingleAssignmentDisposable();        
        SlotOnBtnDisposable.Disposable = SlotOnBtn.BindToOnClick(_ =>
        {
            OnClickActiveSlot(true);
            return Observable.Timer(System.TimeSpan.FromSeconds(CDefines.BUTTONDELAYTIME)).AsUnitObservable();
        }).AddTo(this);

        if (SlotOffBtnDisposable.IsDisposed == false)
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
        if (SaveBtnDisposable.IsDisposed == false)
        {
            SaveBtnDisposable.Dispose();
        }
        SaveBtnDisposable = new SingleAssignmentDisposable();
        SaveBtnDisposable.Disposable = SaveBtn.BindToOnClick(_ =>
        {
            OnClickSaveBtn();
            return Observable.Timer(System.TimeSpan.FromSeconds(CDefines.BUTTONDELAYTIME)).AsUnitObservable();
        }).AddTo(this);

        if (ResetBtnDisposable.IsDisposed == false)
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

    // Set current member type for UI avatar
    private void SetCurEquipItemOfMemberAvatar(MEMBER_TYPE mType, StylingItemInfo itemInfo)
    {
        if (MemberAvatarDic.ContainsKey(mType))
        {
            MemberAvatarDic[mType].SetCurEquipStylingItemDic(itemInfo.ItemData.ItemType, itemInfo);
        }
    }

    public CMemberAvatar GetCurrentMemberAvatar()
    {
        if (MemberAvatarDic.ContainsKey(CurrentMemberType))
        {
            return MemberAvatarDic[CurrentMemberType];
        }
        return null;
    }

    public void SetMemberObj()
    {
        if (MemberAvatarDic.ContainsKey(CurrentMemberType) == false) return;
        DestroyMemberAvatarObj();
        MiddleUI.UICharacter.SetMember(MemberAvatarDic[CurrentMemberType]);
    }

    private void SetActiveSlotOnBtn(bool isActive)
    {
        SlotOnBtn.gameObject.SetActive(isActive);
        SlotOffBtn.gameObject.SetActive(!isActive);
    }


    private void SetActiveResetBtn()
    {
        CMemberAvatar curMemberAvatar = GetCurrentMemberAvatar();
        if (curMemberAvatar != null)
        {
            bool diff = curMemberAvatar.IsDifferentCurWithPutOn();
            SetActiveSaveResetDisableObj(!diff);
        }
    }

    private void SetActiveSaveResetDisableObj(bool isActive)
    {
        SaveBtnDisableObj.SetActive(isActive);
        ResetBtnDisableObj.SetActive(isActive);
    }
    
    private void SetSlotButtons()
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
            SlotObjs[kvp.Value].SetSlotIcon(null);
            SlotObjs[kvp.Value].SetDefaultSlotIconImage(kvp.Key);
        }


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
                itemUIInfo.StyleGoodsData = null;
                SlotObjs[slotIndexDic[itemData.ItemType]].Init(itemUIInfo, CurrentMemberType, OnClickSlot, UpdateSlot);
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

    private List<StylingList> GetPutOnList()
    {
        List<StylingList> putOnList = new List<StylingList>();

        List<StylingList> styleList = CStylingItemInvenManager.Instance.GetStyleItemListByTab(1, CurrentMemberType);
        if (styleList != null)
        {
            foreach (StylingList item in styleList)
            {
                if (item.puton == 1)
                {
                    putOnList.Add(item);
                }
            }
        }
        
        return putOnList;
    }

    
    public void RegistStyleItemToSlot(StyleitemUIinfo item)
    {
        STYLE_ITEM_TYPE itemType = item.StyleItemData.ItemType;
        int slotIdx = slotIndexDic[itemType];
            
        CSoundControl.Instance.EffectPlay(SOUND_ID.SFX_SNG_CLOTHES);
        MiddleUI.UICharacter.SwapWearAnimation(itemType);

        if (SlotObjs[slotIdx].GetSlotItemID() == item.StyleItemData.ID)
        {
            return;
        }

        SlotObjs[slotIdx].SetSlotIcon(item, OnClickSlot, UpdateSlot);

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

            SetActiveResetBtn();
        }
    }

    private void OnClickCharacter()
    {
        MiddleUI.UICharacter.PlayMemberTouchSignatureAnimation();
    }

    private void OnClickActiveSlot(bool bActive)
    {
        foreach (ObjSlotItem slot in SlotObjs)
        {
            slot.SetActiveSlotBtn(bActive);
        }
        
        SetActiveSlotOnBtn(!bActive);
    }

    private void OnClickSaveBtn()
    {
        if (SaveBtnDisableObj.activeSelf)
        {
            CCoreServices.GetCoreService<CPopupService>().NoticeMessageDisposable("smg_style_savestate");
        }
        else
        {
            RequestSaveItem();
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
                        SetSlotButtons();
                        SetActiveSaveResetDisableObj(true);
                    }
                }
                noticeDispose.Dispose();
            }).Subscribe().AddTo(this);
    }

    public void RequestSaveItem(int charID = -1)
    {
        // save item to server
        CMemberAvatar curMemberAvatar = GetCurrentMemberAvatar();
        if (curMemberAvatar == null)
        {
            CDebug.LogError("Current member avatar is null. Cannot save style item.");
            return;
        }

        AvatarList avatarInfo = CMemberAvatarManager.Instance.GetAvatarListToSaveStyleItemAsPutOn(curMemberAvatar);
        if (avatarInfo != null)
        {
            APIHelper.SNGService.ReqSNG_AvatarStylingPutOn(avatarInfo, "0")
                .Subscribe(result =>
                {
                    if (result.d.avatar_list != null && result.d.avatar_list.Count > 0)
                    {
                        AvatarList avatarList = result.d.avatar_list.FirstOrDefault();
                        CMemberAvatarManager.Instance.UpdateMemberAvatarCurEquipSItem(avatarList);
                        UpdateAvatarCurEquip(avatarList, result.d.styling_list);
                        SetActiveSaveResetDisableObj(true);
                        List<StyleitemUIinfo> curItemInfos = ClosetInfo.GetCurrentItemInfos();
                        ClosetItemInfos.SetStyleItemList(curItemInfos);
                        ClosetItemInfos.SortClosetItems();

                        if (charID != -1)
                        {
                            MiddleUI.ChangeMember(1, charID, ChangeMemberClosetInfo);
                        }

                        CCoreServices.GetCoreService<CPopupService>().NoticeMessageDisposable("smg_style_save");
                    }
                }).AddTo(this);
        }
    }
    
    // public void RequestSaveItem()
    // {
    //     // save item to server
    //     CMemberAvatar curMemberAvatar = GetCurrentMemberAvatar();
    //     if (curMemberAvatar == null)
    //     {
    //         CDebug.LogError("Current member avatar is null. Cannot save style item.");
    //         return;
    //     }

    //     AvatarList avatarInfo = CMemberAvatarManager.Instance.GetAvatarListToSaveStyleItemAsPutOn(curMemberAvatar);
    //     if (avatarInfo != null)
    //     {
    //         APIHelper.SNGService.ReqSNG_AvatarStylingPutOn(avatarInfo, "0")
    //             .Subscribe(result =>
    //             {
    //                 if (result.d.avatar_list != null && result.d.avatar_list.Count > 0)
    //                 {
    //                     AvatarList avatarList = result.d.avatar_list.FirstOrDefault();
    //                     CMemberAvatarManager.Instance.UpdateMemberAvatarCurEquipSItem(avatarList);
    //                     UpdateAvatarCurEquip(avatarList, result.d.styling_list);
    //                     SetActiveSaveResetDisableObj(true);
    //                     ClosetItemInfos.SortClosetItems();
    //                     CCoreServices.GetCoreService<CPopupService>().NoticeMessageDisposable("smg_style_save");
    //                 }
    //             }).AddTo(this);
    //     }
    // }

    
    private void ChangeMemberClosetInfo(MEMBER_TYPE memberType)
    {
        CStylingItemInvenManager.Instance.DisposeAvatarApi();
        ClosetInfo.InitMemberClosetInfo(memberType);
    }

    
    private void UpdateAvatarCurEquip(AvatarList avatarList, List<StylingList> itemInfos)
    {
        GameObject avatarObj = MiddleUI.UICharacter.GetUICharacterBase();
        if (avatarObj == null)
        {
            CDebug.LogError("Avatar object is null. Cannot update put on items.");
            return;
        }

        CMemberAvatar curMemberAvatar = MemberAvatarDic[avatarList.character_id.ToEnum<MEMBER_TYPE>()]; //GetCurrentMemberAvatar();
        if (curMemberAvatar != null)
        {
            Dictionary<STYLE_ITEM_TYPE, StylingItemData> curEquipItems = CMemberAvatarDataManager.Instance.GetAllPartsStylingItemDataByAvatarList(avatarList, slotIndexDic);

            foreach (var item in curEquipItems)
            {
                StylingItemInfo _itemInfo = new StylingItemInfo(item.Value, avatarObj);
                curMemberAvatar.SetCurEquipStylingItemDic(item.Key, _itemInfo);
            }

            CStylingItemInvenManager.Instance.UpdateStyleItemListByTab(1, CurrentMemberType, itemInfos);

            SetSlotButtons();
        }
    }
    
    public void ReleaseStyleItemFromSlot(STYLE_ITEM_TYPE itemType)
    {
        if (slotIndexDic.ContainsKey(itemType))
        {
            int slotIdx = slotIndexDic[itemType];
            SlotObjs[slotIdx].SetSlotIcon(null);
            UnEquipStyleItemAcc(itemType);
            SetActiveResetBtn();
        }
    }

    private void UnEquipStyleItemAcc(STYLE_ITEM_TYPE itemType)
    {
        CMemberAvatar curMemberAvatar = GetCurrentMemberAvatar();
        if (curMemberAvatar != null)
        {
            curMemberAvatar.UnEquipStyleItemAcc(itemType);
            //curMemberAvatar.SetCurEquipStylingItemDic(itemType, null);
        }
    }

    private void OnClickSlot(StyleitemUIinfo itemInfo)
    {
        ClosetItemInfos.OnClickItem(itemInfo);
    }

    private void UpdateSlot()
    {
        ClosetItemInfos.UpdateInfoAndRefresh();
    }


    public void ReleaseCharacterObj()
    {
        // if (MiddleUI.UICharacter != null)
        // {
        //     MiddleUI.UICharacter.ReleaseBaseCharObj();
        // }
    }



    private void DestroyMemberAvatarObj()
    {
        foreach (CMemberAvatar member in MemberAvatarDic.Values)
        {
            if (member.GetAvatarObj() != null)
            {
                member.DestroyMemberAvatarObj();
            }
        }

    }


    public void Release()
    {
        DestroyMemberAvatarObj();
       // ReleaseCharacterObj();

        if (MemberAvatarDic != null)
        {
            foreach (CMemberAvatar member in MemberAvatarDic.Values)
            {
                member.ClearEquipItem();
            }
            MemberAvatarDic.Clear();
        }
        
        foreach (ObjSlotItem slot in SlotObjs)
        {
            slot.Release();
        }
    }
}
