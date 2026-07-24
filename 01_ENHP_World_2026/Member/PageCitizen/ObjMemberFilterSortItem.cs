using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class ObjMemberFilterSortItem : MonoBehaviour
{
    [SerializeField] private Button Btn;
    [SerializeField] private GameObject DisableObj;
    [SerializeField] private Image DisableIconImg = null;
    [SerializeField] private Text DisableText = null;
    [SerializeField] private GameObject SelectObj;
    [SerializeField] private Image SelectIconImg = null;
    [SerializeField] private Text SelectText = null;
    private StyleBuffFilterData BuffFilterData;
    private FILTER_GROUP_TYPE GroupType;
    public FILTER_ITEM_OWN_TYPE OwnType;
    public int curIndex;
    public FilterSortInfo SortInfo;
    private SingleAssignmentDisposable BtnDisposable = null;
    public Action<FILTER_ITEM_OWN_TYPE, bool> OnClickOwnTypeBtnEvent;
    public Action<int, bool> OnClickBuffTypeBtnEvent;

    private string[] HavingTexts = new string[]
    {
        "ui_filter_all",
        "ui_check_get_item",
        "ui_check_noget_item"
    };

    private bool prevActiveSelectObj;
     

    public void SetData(int idx, FILTER_GROUP_TYPE grpType)
    {
        GroupType = grpType;
        curIndex = idx;

        SetButton();

        if (SortInfo == null)
        {
            SortInfo = new FilterSortInfo
            {
                GroupType = GroupType,
                CurIndex = curIndex
            };
        }

        bool isActive = SelectObj.activeSelf;
        SetPrevActiveSelectObj();

        switch (GroupType)
        {
            case FILTER_GROUP_TYPE.OWN:
                OwnType = (FILTER_ITEM_OWN_TYPE)idx;
                DisableText.text = SelectText.text = CStringDataManager.Instance.GetStringData(HavingTexts[idx]);
                SetActiveText(isActive);
                SetActiveIcon(!isActive);
                DisableObj.SetActive(!isActive);
                break;
            case FILTER_GROUP_TYPE.BUFF:
                if (idx == 0)
                {
                    DisableText.text = SelectText.text = CStringDataManager.Instance.GetStringData(HavingTexts[idx]);
                    SetActiveIcon(!isActive);
                }
                else
                {
                    List<StyleBuffFilterData> buffFilterList = CMemberAvatarDataManager.Instance.GetStyleBuffFilterDatas();
                    int index = idx - 1;
                    BuffFilterData = buffFilterList[index];
                    SetActiveText(!isActive);
                    CResourceManager.Instance.LoadImage(BuffFilterData.IconResPath, DisableIconImg);
                    CResourceManager.Instance.LoadImage(BuffFilterData.IconResPath, SelectIconImg);
                    SetActiveIcon(isActive);
                }
                break;
        }
        SetFilterSortInfo(OwnType, curIndex, BuffFilterData, SelectObj.activeSelf);
    }

    private void SetButton()
    {
        if (BtnDisposable != null)
        {
            BtnDisposable.Dispose();
        }
        BtnDisposable = new SingleAssignmentDisposable();
        BtnDisposable.Disposable = Btn.BindToOnClick(_ =>
        {
            OnClickFilterBtn();
            return Observable.Timer(TimeSpan.FromSeconds(CDefines.BUTTONDELAYTIME)).AsSingleUnitObservable();
        }).AddTo(this);
    }

    public void SetPrevActiveSelectObj()
    {
        prevActiveSelectObj = SelectObj.activeSelf;
    }

    public bool GetPrevActiveSelectObj()
    {
        return prevActiveSelectObj;
    }

    private void SetActiveText(bool isActive)
    {
        DisableText.gameObject.SetActive(isActive);
        SelectText.gameObject.SetActive(isActive);
    }

    private void SetActiveIcon(bool isActive)
    {
        DisableIconImg.gameObject.SetActive(isActive);
        SelectIconImg.gameObject.SetActive(isActive);
    }

    public void SetOwnTypeAction(Action<FILTER_ITEM_OWN_TYPE, bool> action)
    {
        OnClickOwnTypeBtnEvent = action;
    }

    public void SetBuffTypeAction(Action<int, bool> action)
    {
        OnClickBuffTypeBtnEvent = action;
    }

    private void OnClickFilterBtn()
    {
        bool isActive = !SelectObj.gameObject.activeSelf;

        SetFilterSortInfo(OwnType, curIndex, BuffFilterData, isActive);

        switch (GroupType)
        {
            case FILTER_GROUP_TYPE.OWN:
                //disable object is active is turn off,
                if (OnClickOwnTypeBtnEvent != null)
                {
                    OnClickOwnTypeBtnEvent.Invoke(OwnType, isActive);
                }
                break;
            case FILTER_GROUP_TYPE.BUFF:
                if (OnClickBuffTypeBtnEvent != null)
                {
                    OnClickBuffTypeBtnEvent.Invoke(curIndex, isActive);
                }
                break;
        }
    }

    public void SetActiveSelectBtn(bool isActive)
    {
        SelectObj.SetActive(isActive);
        DisableObj.SetActive(!isActive);
        SetFilterSortInfo(OwnType, curIndex, BuffFilterData, isActive);
    }

    public void SetActive(bool bActive)
    {
        SetActiveSelectBtn(bActive);
    }

    public bool GetActiveState()
    {
        return SelectObj.activeSelf;
    }

    private void SetFilterSortInfo(FILTER_ITEM_OWN_TYPE ownType, int idx, StyleBuffFilterData buffData, bool bActive)
    {
        SortInfo.OwnType = ownType;
        SortInfo.CurIndex = idx;
        SortInfo.BuffFilterData = buffData;
        SortInfo.ActiveSelf = bActive;
    }

    public FilterSortInfo GetFilterSortInfo()
    {
        return SortInfo;
    }
}


public class  FilterSortInfo
{
    public FILTER_GROUP_TYPE GroupType;
    public FILTER_ITEM_OWN_TYPE OwnType;
    public int CurIndex;
    public StyleBuffFilterData BuffFilterData;
    public bool ActiveSelf;
}
