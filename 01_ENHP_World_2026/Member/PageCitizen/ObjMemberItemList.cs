using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class ObjMemberItemList : MonoBehaviour
{
    [SerializeField] private List<ObjMemberStyleItem> MemberStyleItems;

    private STYLING_TAB CurrentTab;
    private Action<StyleitemUIinfo> OnSelectEvent = null;
    public Action<StyleitemUIinfo> SetOnSelectEvent { set { OnSelectEvent = value; } }
    public void Init()
    {
        MemberStyleItems.ForEach(item =>
        {
            item.Init();
        });
    }

    public void SetCurrentTab(STYLING_TAB tab)
    {
        CurrentTab = tab;
        MemberStyleItems.ForEach(item =>
        {
            item.SetCurrentTab(CurrentTab);
        });
    }

    public void SetData(List<StyleitemUIinfo> list, CMemberAvatar memberAvatr, bool isPage = true, bool selectUI = true, Action<StyleitemUIinfo> onClickEvent = null, Action<STYLING_TAB> onClickRelease = null, Action refreshEvent = null)
    {
        for (int i = 0; i < MemberStyleItems.Count; ++i)
        {
            if (i < list.Count)
            {
                ObjMemberStyleItem.Setting setting = new ObjMemberStyleItem.Setting()
                {
                    IsPage = isPage,
                    Data = list[i],
                    IsSelectUI = selectUI,
                    MemberAvatar = memberAvatr,
                    OnClickEvent = onClickEvent,
                    OnClickAccRelaseEvent = onClickRelease,
                    RefreshEvent = refreshEvent
                };

                MemberStyleItems[i].SetData(setting);
            }
            else
            {
                MemberStyleItems[i].SetEnable(false);
            }
        }
    }
    
    // public void OnClickSelect(StyleitemUIinfo uiData)
    // {
    //     // Handle the click event for the selected item
    //      OnSelectEvent.Invoke(uiData);
    //     Debug.Log("Selected item: " + uiData.StyleItemData.ItemType); // Example action
    // }
}
