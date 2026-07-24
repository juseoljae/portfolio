using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

public class ObjSlotItem : MonoBehaviour
{
    [SerializeField] private Button slotButton;
    [SerializeField] private Image defaultSlotImage;
    [SerializeField] private Image slotImage;
    private StyleitemUIinfo ItemUIInfo;
    private MEMBER_TYPE MemberType = MEMBER_TYPE.NONE;
    private Action<StyleitemUIinfo> OnClickEvent;
    private Action RefreshEvent;
    private SingleAssignmentDisposable SlotBtnDisposable = null;

    public void Init(StyleitemUIinfo itemInfo, MEMBER_TYPE memberType, Action<StyleitemUIinfo> onClickEvt = null, Action RefEvt = null)
    {
        ItemUIInfo = itemInfo;
        MemberType = memberType;
        
        OnClickEvent = onClickEvt;
        RefreshEvent = RefEvt;

        if (slotButton != null)
        {
            if (SlotBtnDisposable != null)
            {
                SlotBtnDisposable.Dispose();
            }
            SlotBtnDisposable = new SingleAssignmentDisposable();
            SlotBtnDisposable.Disposable = slotButton.BindToOnClick(_ =>
            {
                OnClickSlot();

                return Observable.Timer(System.TimeSpan.FromSeconds(CDefines.BUTTONDELAYTIME)).AsSingleUnitObservable();
            }).AddTo(this);
        }
        else
        {
            CDebug.LogError("slotButton is null in ObjSlotItem.Init()");
        }

        //
        

        bool bActiveSlotImage = true;
        if (itemInfo != null)
        {
            SetSlotIconImage(itemInfo.StyleItemData.ResourceIconPath);
        }
        else
        {
            bActiveSlotImage = false;
        }

        slotImage.transform.parent.gameObject.SetActive(bActiveSlotImage);
    }

    public MEMBER_TYPE GetMemberType()
    {
        return MemberType;
    }

    public void SetActiveSlotBtn(bool isActive)
    {        
        slotButton.gameObject.SetActive(isActive);
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

    public void SetSlotIcon(StyleitemUIinfo itemInfo, Action<StyleitemUIinfo> onClickEvt = null, Action RefEvt = null)
    {
        if (itemInfo != null)
        {
            ItemUIInfo = itemInfo;

            OnClickEvent = onClickEvt;
            RefreshEvent = RefEvt;

            SetSlotIconImage(itemInfo.StyleItemData.ResourceIconPath);
        }
        else
        {
            ItemUIInfo = null;
            slotImage.transform.parent.gameObject.SetActive(false);
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

    public StylingItemData GetSlotItemData()
    {
        if (ItemUIInfo != null)
        {
            return ItemUIInfo.StyleItemData;
        }

        return null;
    }


    private void OnClickSlot()
    {
        if (ItemUIInfo == null)
        {
            CCoreServices.GetCoreService<CPopupService>().NoticeMessageDisposable("smg_nowear_item");
            return;
        }
        
        CPopupService popupService = CCoreServices.GetCoreService<CPopupService>();
        var popup = popupService.OpenPopupStyleItemDetail(new PopupStyleItemDetail.Setting()
        {
            StyleItemInfo = ItemUIInfo,
            IsJustInfo = true,
            OnClickEvent = OnClickEvent,
            //RefreshEvent = RefreshEvent
            RefreshPurchasedSlotEvt = OnRefreshMySlot

        });

        SingleAssignmentDisposable disposable = new SingleAssignmentDisposable();
        disposable.Disposable = popup.ShowAsObservable().Subscribe(res =>
        {
            disposable.Dispose();
        });
    }

    private void OnRefreshMySlot(StyleitemUIinfo info)
    {
        if (RefreshEvent != null)
        {
            RefreshEvent.Invoke();
        }
    }

    public void Release()
    {
        MemberType = MEMBER_TYPE.NONE;
    }
}
