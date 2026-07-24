using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ObjMemberFilterGroup : MonoBehaviour
{
    [SerializeField] private Text TitleText = null;
    public List<ObjMemberFilterSortItem> MemberFilters;

    private FILTER_GROUP_TYPE GroupType;
    public Action<FILTER_ITEM_OWN_TYPE, bool> OwnTypeBtnEvent;
    public Action<int, bool> BuffTypeBtnEvent;
    private const string ITEM_OBJECT_NAME = "obj_sort_member_item";

    public void Init(FILTER_GROUP_TYPE grpType, Action<FILTER_ITEM_OWN_TYPE, bool> ownAction = null, Action<int, bool> buffAction = null)
    {
        GroupType = grpType;

        SetTitleTxt();
        if (GroupType == FILTER_GROUP_TYPE.OWN)
        {
            OwnTypeBtnEvent = ownAction;
            SetOwnTypeFilter();
        }
        else
        {
            BuffTypeBtnEvent = buffAction;
            SetBuffFilter();
        }
    }

    private void SetTitleTxt()
    {
        switch (GroupType)
        {
            case FILTER_GROUP_TYPE.OWN:
                TitleText.text = CStringDataManager.Instance.GetStringData("ui_filter_styleitem");
                break;
            case FILTER_GROUP_TYPE.BUFF:
                TitleText.text = CStringDataManager.Instance.GetStringData("ui_member_style_buff");
                break;
        }
    }

    private void SetBuffFilter()
    {
        var OrgObj = MemberFilters[0];
        OrgObj.SetData(0, GroupType);
        OrgObj.SetBuffTypeAction(BuffTypeBtnEvent);

        List<StyleBuffFilterData> buffFilterList = CMemberAvatarDataManager.Instance.GetStyleBuffFilterDatas();
        for (int i = 1; i < buffFilterList.Count + 1; ++i)
        {
            var item = Instantiate(OrgObj, OrgObj.transform.parent);
            item.gameObject.SetActive(true);
            item.SetData(i, GroupType);
            item.SetBuffTypeAction(BuffTypeBtnEvent);
            MemberFilters.Add(item);
        }
    }

    private void SetOwnTypeFilter()
    {
        for (int i = 0; i < MemberFilters.Count; ++i)
        {
            var item = MemberFilters[i];
            item.SetData(i, GroupType);
            item.SetOwnTypeAction(OwnTypeBtnEvent);
        }
    }


    //own tye, buff type control
    public void SetActiveAllFilterButtons<TKey>(Dictionary<TKey, bool> stateMap, TKey allKey)
    {
        if (stateMap.TryGetValue(allKey, out bool isActive))
        {
            SetAllActiveFilterButtons(isActive);
        }
    }

    public void SetActiveFilterButton(int index, bool isActive)
    {
        MemberFilters[index].SetActiveSelectBtn(isActive);
        UpdateAllFilterButtonState();
        UpdateAllTypeButtonActive();
    }



    private (bool allDisabled, bool allEnabled) CheckAllButtonStates()
    {
        bool allDisabled = true;
        bool allEnabled = true;

        foreach (var filter in MemberFilters)
        {
            int index = (GroupType == FILTER_GROUP_TYPE.BUFF) ? filter.curIndex : (int)filter.OwnType;

            if (index != 0)
            {
                bool isActive = filter.GetActiveState();

                if (isActive)
                {
                    allDisabled = false;
                }
                else
                {
                    allEnabled = false;
                }
            }
        }


        return (allDisabled, allEnabled);
    }

    private void UpdateAllFilterButtonState()
    {
        ValueTuple<bool, bool> result = CheckAllButtonStates();
        if (result.Item1)
        {
            SetAllActiveFilterButtons(false);
        }
        else if (result.Item2)
        {
            SetAllActiveFilterButtons(true);
        }
    }

    private void UpdateAllTypeButtonActive()
    {
        bool isActive = true;
        for (int i = 1; i < MemberFilters.Count; ++i)
        {
            if (!MemberFilters[i].GetActiveState())
            {
                isActive = false;
                break;
            }
        }

        MemberFilters[0].SetActiveSelectBtn(isActive);
    }

    public bool IsAllFilterButtonHide()
    {
        foreach (var filter in MemberFilters)
        {
            if (filter.GetActiveState())
            {
                return false;
            }
        }
        
        return true;
    }

    public void SetActiveFilterButton(FILTER_ITEM_OWN_TYPE ownType, bool isActive)
    {
        SetActiveFilterButton((int)ownType, isActive);
    }

    public void SetPrevActiveAllFilterButtonsBySelectObjState()
    {
        foreach (var filter in MemberFilters)
        {
            filter.SetActive(filter.GetPrevActiveSelectObj());
        }
    }

    public void SetPrevActiveAllFilterButtons()
    {
        foreach (var filter in MemberFilters)
        {
            filter.SetPrevActiveSelectObj();
        }
    }

    public void SetAllActiveFilterButtons(bool bActive)
    {
        foreach (var filter in MemberFilters)
        {
            filter.SetActive(bActive);
        }
    }

    public bool GetAllButtonHide()
    {
        foreach (var filter in MemberFilters)
        {
            if (filter.GetActiveState())
            {
                return false;
            }
        }
        return true;
    }

    public List<FilterSortInfo> GetFilterSortInfos()
    {
        List<FilterSortInfo> filterSortInfos = new List<FilterSortInfo>();
        foreach (var filter in MemberFilters)
        {
            filterSortInfos.Add(filter.GetFilterSortInfo());
        }

        return filterSortInfos;
    }
}
