using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using Game.RestAPI;

public class ObjShopSlotItem : MonoBehaviour
{
    [SerializeField] private Image slotImage;
    [SerializeField] private Image defaultSlotImage;
    [SerializeField] private Button SlotBtn;
    [SerializeField] private GameObject BuyObj;
    [SerializeField] private Text PriceText;
    [SerializeField] private Image GoodsImage;
    [SerializeField] private GameObject LockObj;
    private StyleitemUIinfo ItemUIInfo;
    private MEMBER_TYPE MemberType = MEMBER_TYPE.NONE;
    private Action<StyleitemUIinfo> OnClickEvent;
    private Action RefreshEvent;
    private Action RefreshMySlotEvent;
    private Action<StyleitemUIinfo> UnEquipEvt;
    private SingleAssignmentDisposable SlotBtnDisposable = new SingleAssignmentDisposable();
    public void Init(StyleitemUIinfo itemInfo, MEMBER_TYPE memberType, Action<StyleitemUIinfo> onClickEvt = null, Action RefEvt = null, Action<StyleitemUIinfo> unEquipEvt = null)
    {
        ItemUIInfo = itemInfo;
        MemberType = memberType;

        OnClickEvent = onClickEvt;
        RefreshEvent = RefEvt;
        UnEquipEvt = unEquipEvt;

        if (SlotBtn != null)
        {
            if (SlotBtnDisposable != null)
            {
                SlotBtnDisposable.Dispose();
            }
            SlotBtnDisposable = new SingleAssignmentDisposable();
            SlotBtnDisposable.Disposable = SlotBtn.BindToOnClick(_ =>
           {
               OnClickSlot();

               return Observable.Timer(System.TimeSpan.FromSeconds(CDefines.BUTTONDELAYTIME)).AsSingleUnitObservable();
           }).AddTo(this);
        }
        
        SetPrice();

        SetBuyIcon();

        SetLockIcon();

        bool isActiveSlotIcon = true;
        if (itemInfo != null && itemInfo.StyleItemData != null)
        {
            SetSlotIconImage(itemInfo.StyleItemData.ResourceIconPath);
        }
        else
        {
            isActiveSlotIcon = false;
        }
        slotImage.transform.parent.gameObject.SetActive(isActiveSlotIcon);
    }

    public MEMBER_TYPE GetMemberType()
    {
        return MemberType;
    }

    public StylingList GetStylingItemInfo()
    {
        if (ItemUIInfo != null && ItemUIInfo.StyleInfo != null)
        {
            return ItemUIInfo.StyleInfo;
        }

        return null;
    }

    public StyleitemUIinfo GetItemUIInfo()
    {
        return ItemUIInfo;
    }

    public void SetSlotIconImage(string iconPath)
    {
        CResourceManager.Instance.LoadImage(iconPath, slotImage);
        slotImage.transform.parent.gameObject.SetActive(true);
    }
    

    public void SetDefaultSlotIconImage(STYLE_ITEM_TYPE type)
    {
        string iconPath = CMemberAvatarDataManager.Instance.GetStyleTabIconPath(type);
        CResourceManager.Instance.LoadImage(iconPath, defaultSlotImage);
    }

    public void SetSlotIcon(StyleitemUIinfo itemInfo, Action<StyleitemUIinfo> onClickEvt = null, Action RefEvt = null, Action<StyleitemUIinfo> unEquipEvt = null)
    {
        if (itemInfo != null)
        {
            SetIconStatus(itemInfo);
            //ItemUIInfo = itemInfo;

            OnClickEvent = onClickEvt;
            RefreshEvent = RefEvt;
            UnEquipEvt = unEquipEvt;

            SetSlotIconImage(itemInfo.StyleItemData.ResourceIconPath);

            // SetBuyIcon();
            // SetLockIcon();
        }
        else
        {
            ItemUIInfo = null;
            slotImage.transform.parent.gameObject.SetActive(false);
            BuyObj.SetActive(false);
        }
    }

    public void SetIconStatus(StyleitemUIinfo itemInfo)
    {
        if (itemInfo != null)
        {
            ItemUIInfo = itemInfo;
            SetPrice();
            SetBuyIcon();
            SetLockIcon();
        }
    }


    public int GetSlotItemID()
    {
        if (ItemUIInfo != null && ItemUIInfo.StyleItemData != null)
        {
            return ItemUIInfo.StyleItemData.ID;
        }

        return -1;
    }

    private void SetBuyIcon()
    {
        bool isHavingItem = false;
        if (ItemUIInfo != null && ItemUIInfo.StyleInfo != null)
        {
            isHavingItem = !(ItemUIInfo.StyleInfo.having == 1);
            if (isHavingItem)
            {
                if (ItemUIInfo.StyleGoodsData != null)
                {
                    isHavingItem = ItemUIInfo.StyleGoodsData.isSale;
                }
            }

        }

        BuyObj.SetActive(isHavingItem);
    }

    private void SetPrice()
    {
        bool isActive = false;
        if (ItemUIInfo != null && ItemUIInfo.StyleInfo != null && ItemUIInfo.StyleInfo.having == 0)
        {
            StyleShopGoodsData StyleGoodsData = ItemUIInfo.StyleGoodsData;

            if (StyleGoodsData != null && StyleGoodsData.isSale)
            {
                REWARD_TYPE rwdType = (REWARD_TYPE)StyleGoodsData.CostType;
                int itemID = 0;
                if (rwdType == REWARD_TYPE.RW_ITEM)
                {
                    itemID = (int)StyleGoodsData.CostSubType;
                }

                string iconPath = rwdType.GetRewardResourceIcon(itemID);
                CResourceManager.Instance.LoadImage(iconPath, GoodsImage);

                PriceText.text = StyleGoodsData.CostValue.ToCommaString();
                isActive = true;
            }
        }
        else
        {
            isActive = false;
        }
        
        PriceText.gameObject.SetActive(isActive);
        GoodsImage.gameObject.SetActive(isActive);
    }

    private void SetLockIcon()
    {
        bool isActive = false;
        if (ItemUIInfo != null && ItemUIInfo.StyleItemData != null)
        {
            if (ItemUIInfo.StyleGoodsData != null)
            {
                isActive = !ItemUIInfo.StyleGoodsData.isSale;
            }

            if (ItemUIInfo.StyleInfo != null && ItemUIInfo.StyleInfo.having == 1)
            {
                isActive = false;
            }
        }
        LockObj.SetActive(isActive);
    }


    public void SetActiveSlotBtn(bool isActive)
    {
        SlotBtn.gameObject.SetActive(isActive);
    }

    private void OnClickSlot()
    {
        bool bInfo = false;
        if (ItemUIInfo == null)
        {
            CCoreServices.GetCoreService<CPopupService>().NoticeMessageDisposable("smg_nowear_item");
            return;
        }
        // if (ItemUIInfo.StyleGoodsData != null && ItemUIInfo.StyleGoodsData.isSale)
        // {
        //     bInfo = ItemUIInfo.StyleInfo.having == 1;
        // }

        bInfo = ItemUIInfo.StyleInfo.having == 1;

        CPopupService popupService = CCoreServices.GetCoreService<CPopupService>();
        var popup = popupService.OpenPopupStyleItemDetail(new PopupStyleItemDetail.Setting()
        {
            StyleItemInfo = ItemUIInfo,
            IsJustInfo = bInfo,
            OnClickEvent = OnClickEvent,
            //RefreshEvent = RefreshEvent
            RefreshPurchasedSlotEvt = OnRefreshMySlot,
            UnEquipEvt = OnUnEquipMySlot,
        });

        SingleAssignmentDisposable disposable = new SingleAssignmentDisposable();
        disposable.Disposable = popup.ShowAsObservable().Subscribe(res =>
        {
            disposable.Dispose();
        });
    }

    private void OnRefreshMySlot(StyleitemUIinfo info)
    {
        if (info.StyleInfo != null)
        {
            BuyObj.SetActive(info.StyleInfo.having == 1);
        }

        if (RefreshEvent != null)
        {
            RefreshEvent.Invoke();
        }
    }

    private void OnUnEquipMySlot(StyleitemUIinfo info)
    {
        if (UnEquipEvt != null)
        {
            UnEquipEvt.Invoke(info);
        }
    }

    public void Release()
    {
        MemberType = MEMBER_TYPE.NONE;
    }
}
