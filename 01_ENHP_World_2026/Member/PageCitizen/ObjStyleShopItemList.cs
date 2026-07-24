using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using SuperScrollView;
using System.Linq;
using Game.RestAPI;

public class ObjStyleShopItemList : MonoBehaviour
{
    [SerializeField] private List<ObjStyleShopItem> MemberStyleItems;
    
    private STYLING_TAB CurrentTab;

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
    
    public void SetData(List<StyleitemUIinfo> list, bool isPage = true, bool selectUI = true,
                        Action<StyleitemUIinfo> onClickEvent = null,
                        Action<STYLING_TAB> onClickRelease = null,
                        Action<StyleitemUIinfo> refreshEvent = null,
                        Action<STYLE_ITEM_TYPE> updateEvent = null,
                        Action<StyleitemUIinfo> unEquipEvent = null)
    {
        for (int i = 0; i < MemberStyleItems.Count; ++i)
        {
            if (i < list.Count)
            {
                ObjStyleShopItem.Setting setting = new ObjStyleShopItem.Setting()
                {
                    IsPage = isPage,
                    Data = list[i],
                    IsSelectUI = selectUI,
                    OnClickEvent = onClickEvent,
                    OnClickAccRelaseEvent = onClickRelease,
                    RefreshEvent = refreshEvent,
                    UpdateEvent = updateEvent,
                    UnEquipEvt = unEquipEvent
                };

                MemberStyleItems[i].SetData(setting);
            }
            else
            {
                MemberStyleItems[i].SetEnable(false);
            }
        }
    }
}
