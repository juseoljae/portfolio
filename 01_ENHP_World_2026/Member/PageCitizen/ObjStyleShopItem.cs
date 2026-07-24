using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using Game.RestAPI;

public class ObjStyleShopItem : MonoBehaviour
{
    [SerializeField] private Image IconImage;
    [SerializeField] private Image NewImage;
    [SerializeField] private Button AccReleaseBtn;
    [SerializeField] private GameObject LockObj;

    [SerializeField] private Button InfoBtn;
    [SerializeField] private Button RegBtn;
    [SerializeField] private Button BuyBtn;
    [SerializeField] private Text PriceText;
    [SerializeField] private Image GoodsImage;
    [SerializeField] private GameObject NewObj;

    private bool IsInitUI = false;

    private StyleitemUIinfo StyleItemInfo = null;
    private Action<STYLING_TAB> onClickAccRelaseEvent;
    private Action<StyleitemUIinfo> onClickEvent;
    private Action<StyleitemUIinfo> RefreshEvent;
    private Action<STYLE_ITEM_TYPE> UpdateEvent;
    private Action<StyleitemUIinfo> UnEquipEvt;
    private STYLING_TAB CurrentTab;
    private SingleAssignmentDisposable AccReleaseBtnDisposable = new SingleAssignmentDisposable();
    private SingleAssignmentDisposable InfoBtnDisposable = new SingleAssignmentDisposable();
    private SingleAssignmentDisposable BuyBtnDisposable = new SingleAssignmentDisposable();
    private SingleAssignmentDisposable RegBtnDisposable = new SingleAssignmentDisposable();
    public void Init()
    {
        if (AccReleaseBtnDisposable != null)
        {
            AccReleaseBtnDisposable.Dispose();
        }
        AccReleaseBtnDisposable = new SingleAssignmentDisposable();
        AccReleaseBtnDisposable.Disposable = AccReleaseBtn.BindToOnClick(_ =>
        {
            OnClickReleaseAcc();

            return Observable.Timer(TimeSpan.FromSeconds(CDefines.BUTTONDELAYTIME)).AsUnitObservable();
        }).AddTo(this);

        if (InfoBtnDisposable != null)
        {
            InfoBtnDisposable.Dispose();
        }
        InfoBtnDisposable = new SingleAssignmentDisposable();
        InfoBtnDisposable.Disposable = InfoBtn.BindToOnClick(_ =>
        {
            OnClickInfo();
            return Observable.Timer(TimeSpan.FromSeconds(CDefines.BUTTONDELAYTIME)).AsUnitObservable();
        }).AddTo(this);

        if (BuyBtnDisposable != null)
        {
            BuyBtnDisposable.Dispose();
        }
        BuyBtnDisposable = new SingleAssignmentDisposable();
        BuyBtnDisposable.Disposable = BuyBtn.BindToOnClick(_ =>
        {
            OnClickBuy();
            return Observable.Timer(TimeSpan.FromSeconds(CDefines.BUTTONDELAYTIME)).AsUnitObservable();
        }).AddTo(this);

        if (RegBtnDisposable != null)
        {
            RegBtnDisposable.Dispose();
        }
        RegBtnDisposable = new SingleAssignmentDisposable();
        RegBtnDisposable.Disposable = RegBtn.BindToOnClick(_ =>
        {
            OnClickRegist();
            return Observable.Timer(TimeSpan.FromSeconds(CDefines.BUTTONDELAYTIME)).AsUnitObservable();
        }).AddTo(this);
    }

    public void SetCurrentTab(STYLING_TAB tab)
    {
        CurrentTab = tab;
    }

    public void SetData(Setting setting)
    {
        StyleItemInfo = setting.Data;
        onClickEvent = setting.OnClickEvent;
        onClickAccRelaseEvent = setting.OnClickAccRelaseEvent;
        RefreshEvent = setting.RefreshEvent;
        UpdateEvent = setting.UpdateEvent;
        UnEquipEvt = setting.UnEquipEvt;

        SetPrice();
        SetLockBtnActive();
        SetActiveAccReleaseBtn();
        SetIconImage();
        SetNew();


        if (StyleItemInfo.StyleItemData != null)
        {
            InfoBtn.gameObject.SetActive(true);
        }
        else
        {
            InfoBtn.gameObject.SetActive(false);
        }

        SetEnable(true);
    }

    private void SetActiveAccReleaseBtn()
    {
        AccReleaseBtn.gameObject.SetActive(StyleItemInfo.StyleItemData == null);
    }

    private void SetIconImage()
    {
        if (StyleItemInfo.StyleItemData == null)
        {
            IconImage.gameObject.SetActive(false);
            return;
        }


        CResourceManager.Instance.LoadImage(StyleItemInfo.StyleItemData.ResourceIconPath, IconImage);
        IconImage.gameObject.SetActive(true);
    }
    

    private void SetNew()
    {
        if (StyleItemInfo != null && StyleItemInfo.StyleInfo != null)
        {
            int isNew = StyleItemInfo.StyleInfo.new_flag;
            NewImage.enabled = isNew == 1;
        }
    }

    private void SetLockBtnActive()
    {
        bool isActive = false;
        if (StyleItemInfo.StyleInfo != null && StyleItemInfo.StyleInfo.having == 0)
        {
            StyleShopGoodsData StyleGoodsData = StyleItemInfo.StyleGoodsData;
            if (StyleGoodsData != null)
            {
                isActive = !StyleGoodsData.isSale;
            }
        }

        LockObj.SetActive(isActive);
    }

    private void SetPrice()
    {
        bool isActive = false;

        if (StyleItemInfo.StyleInfo != null && StyleItemInfo.StyleInfo.having == 0)
        {
            StyleShopGoodsData StyleGoodsData = StyleItemInfo.StyleGoodsData;

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
                CDebug.Log($"#### SetPrice() id: {StyleItemInfo.StyleItemData.ID}, goodsid:{StyleGoodsData.ID}, part: {StyleGoodsData.ItemType}, rwdType: {rwdType}, itemID: {itemID}, price: {StyleGoodsData.CostValue}");
                isActive = true;
            }
        }
        else
        {
            isActive = false;
        }

        PriceText.gameObject.SetActive(isActive);
        GoodsImage.gameObject.SetActive(isActive);
        BuyBtn.gameObject.SetActive(isActive);
        InfoBtn.gameObject.SetActive(!isActive);
    }

    public void SetEnable(bool isEnable)
    {
        this.gameObject.SetActive(isEnable);
    }

    private void OnClickReleaseAcc()
    {
        if (onClickAccRelaseEvent != null)
        {
            onClickAccRelaseEvent.Invoke(CurrentTab);
        }
    }

    private void OnClickBuy()
    {
        OpenItemDetailPopup(false);
    }

    private void OnClickInfo()
    {
        CDebug.Log("OnClickInfo");
        
        OpenItemDetailPopup(LockObj.activeSelf == false);
    }

    private void OnClickRegist()
    {
        if (onClickEvent != null)
        {
            onClickEvent.Invoke(StyleItemInfo);
        }
    }

    private void OpenItemDetailPopup(bool bInfo)
    {
        CSoundControl.Instance.EffectPlay(SOUND_ID.SFX_COMMON_TOUCH_01);
        CPopupService popupService = CCoreServices.GetCoreService<CPopupService>();
        var popup = popupService.OpenPopupStyleItemDetail(new PopupStyleItemDetail.Setting()
        {
            StyleItemInfo = StyleItemInfo,
            IsJustInfo = bInfo,
            OnClickEvent = onClickEvent,
            RefreshEvent = RefreshEvent,
            UpdateEvent = UpdateEvent,
            UnEquipEvt = UnEquipEvt
        });

        SingleAssignmentDisposable disposable = new SingleAssignmentDisposable();
        disposable.Disposable = popup.ShowAsObservable().Subscribe(res =>
        {
            disposable.Dispose();
        });
    }

    public class Setting
    {
        public bool IsPage = false;
        public bool IsSelectUI = true;
        public Action<StyleitemUIinfo> OnClickEvent;
        public Action<STYLING_TAB> OnClickAccRelaseEvent;
        public Action<StyleitemUIinfo> RefreshEvent;
        public Action<STYLE_ITEM_TYPE> UpdateEvent;
        public StyleitemUIinfo Data;
        public Action<StyleitemUIinfo> UnEquipEvt;
        // public bool IsSelect => Data.GDID == CCitizenUIDataManager.Instance.CurrentSelectID && IsSelectUI;
        // public bool IsLock =>  CITIZEN_STATE.UNLOCK >= Data.State;
        // public int ID => Data.TData.ID;
        // public string Icon => Data.TData.IconResPath;

    }
}
