using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Game.RestAPI;
using UniRx;

public class ObjMemberStyleItem : MonoBehaviour
{
    [SerializeField] private Image IconImage;
    [SerializeField] private Image NewImage;
    [SerializeField] private Button AccReleaseBtn;
    [SerializeField] private Button InfoBtn;
    [SerializeField] private Button RegBtn;

    private bool IsInitUI = false;

    private Setting SettingData = null;
    private CMemberAvatar CurMemberAvatar;
    private StyleitemUIinfo StyleItemInfo = null;
    private Action<StyleitemUIinfo> OnSelectEvent = null;
    public Action<StyleitemUIinfo> SetOnSelectEvent { set { OnSelectEvent = value; } }

    private STYLING_TAB CurrentTab;
    private Action<STYLING_TAB> onClickAccRelaseEvent;
    private Action<StyleitemUIinfo> onClickEvent;
    private Action RefreshEvent;
    private SingleAssignmentDisposable AccReleaseBtnDisposable = new SingleAssignmentDisposable();
    private SingleAssignmentDisposable InfoBtnDisposable = new SingleAssignmentDisposable();
    private SingleAssignmentDisposable RegBtnDisposable = new SingleAssignmentDisposable();
    public void Init()
    {
    }
    
    public void SetCurrentTab(STYLING_TAB tab)
    {
        CurrentTab = tab;
    }

    public void SetData(Setting setting)
    {
        SettingData = setting;
        StyleItemInfo = setting.Data;
        onClickEvent = setting.OnClickEvent;
        onClickAccRelaseEvent = setting.OnClickAccRelaseEvent;
        RefreshEvent = setting.RefreshEvent;

        CurMemberAvatar = setting.MemberAvatar;

        SetButtons();

        SetNew();

        SetActiveAccReleaseBtn();
        SetIconImage();
        
        SetEnable(true);
    }

    private void SetButtons()
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

        if (StyleItemInfo.StyleItemData != null)
        {
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
            
        }
        else
        {
            InfoBtn.gameObject.SetActive(false);
        }

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

    private void SetNew()
    {
        NewImage.enabled = false;
        if (StyleItemInfo != null && StyleItemInfo.StyleInfo != null)
        {
            int isNew = StyleItemInfo.StyleInfo.new_flag;
            NewImage.enabled = isNew == 1;
        }
    }

    private void SetActiveAccReleaseBtn()
    {
        AccReleaseBtn.gameObject.SetActive(StyleItemInfo.StyleItemData == null);
    }

    private void SetIconImage()
    {
        bool bActive = false;
        if (StyleItemInfo.StyleItemData == null)
        {
            bActive = false;
        }
        else
        {
            CResourceManager.Instance.LoadImage(StyleItemInfo.StyleItemData.ResourceIconPath, IconImage);
            bActive = true;
        }

        IconImage.gameObject.SetActive(bActive);
        gameObject.SetActive(bActive);
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

    private void OnClickInfo()
    {
        CSoundControl.Instance.EffectPlay(SOUND_ID.SFX_COMMON_TOUCH_01);
        CPopupService popupService = CCoreServices.GetCoreService<CPopupService>();
        var popup = popupService.OpenPopupStyleItemDetail(new PopupStyleItemDetail.Setting()
        {
            StyleItemInfo = StyleItemInfo,
            IsJustInfo = true,
            
        });

        SingleAssignmentDisposable disposable = new SingleAssignmentDisposable();
        disposable.Disposable = popup.ShowAsObservable().Subscribe(res =>
        {
            disposable.Dispose();
        });
    }

    private void OnClickRegist()
    {
        if (onClickEvent != null)
        {
            onClickEvent.Invoke(StyleItemInfo);
        }
    }

    public class Setting
    {
        public bool IsPage = false;
        public bool IsSelectUI = true;
        public CMemberAvatar MemberAvatar;
        public Action<StyleitemUIinfo> OnClickEvent;
        public Action<STYLING_TAB> OnClickAccRelaseEvent;
        public Action RefreshEvent;
        public StyleitemUIinfo Data;
        // public bool IsSelect => Data.GDID == CCitizenUIDataManager.Instance.CurrentSelectID && IsSelectUI;
        // public bool IsLock =>  CITIZEN_STATE.UNLOCK >= Data.State;
        // public int ID => Data.TData.ID;
        // public string Icon => Data.TData.IconResPath;

    }
}
