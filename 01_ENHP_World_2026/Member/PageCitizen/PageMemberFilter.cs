using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Game.RestAPI;
using System.Linq;

public class PageMemberFilter : MonoBehaviour
{
    [SerializeField] private Button DimBtn = null;
    [SerializeField] private Button SaveBtn = null;
    [SerializeField] private GameObject SaveDisableObj = null;
    [SerializeField] private List<ObjMemberFilterGroup> FilterGroup = null;
    private PageStyleShopInfo ShopUIInfo;
    private Dictionary<FILTER_ITEM_OWN_TYPE, bool> CurEachOwnType;
    private Dictionary<int, bool> CurEachBuffData;
    private List<StyleitemUIinfo> ItemInfos;
    private List<StyleitemUIinfo> ItemInfosByType;
    private STYLING_TAB CurrentTab;
    private SingleAssignmentDisposable DimDisposable = null;
    private SingleAssignmentDisposable SaveDisableDisposable = null;
    private SingleAssignmentDisposable ApiDisposable = null;

    public void SetActive(bool isActive)
    {
        gameObject.SetActive(isActive);
    }

    public void Init(PageStyleShopInfo shopUIInfo)
    {
        ShopUIInfo = shopUIInfo;
        InitFilterGroup();
    }

    public void InitFilterGroup()
    {
        for (int i = 0; i < FilterGroup.Count; i++)
        {
            var groupType = (FILTER_GROUP_TYPE)i;
            switch (groupType)
            {
                case FILTER_GROUP_TYPE.OWN:
                    if (CurEachOwnType == null)
                    {
                        CurEachOwnType = new Dictionary<FILTER_ITEM_OWN_TYPE, bool>();
                    }
                    FilterGroup[i].Init(groupType, SetOwnTypeState, null);
                    break;
                case FILTER_GROUP_TYPE.BUFF:
                    if (CurEachBuffData == null)
                    {
                        CurEachBuffData = new Dictionary<int, bool>();
                    }
                    FilterGroup[i].Init(groupType, null, SetBuffTypeState);
                    break;
            }
        }
        SetButtons();
    }

    private void SetButtons()
    {
        if (DimDisposable != null)
        {
            DimDisposable.Dispose();
        }
        DimDisposable = new SingleAssignmentDisposable();
        DimDisposable.Disposable = DimBtn.BindToOnClick(_ =>
        {
            OnCkickClose();
            return Observable.Timer(TimeSpan.FromSeconds(CDefines.BUTTONDELAYTIME)).AsSingleUnitObservable();
        }).AddTo(this);

        if (SaveDisableDisposable != null)
        {
            SaveDisableDisposable.Dispose();
        }
        SaveDisableDisposable = new SingleAssignmentDisposable();
        SaveDisableDisposable.Disposable = SaveBtn.BindToOnClick(_ =>
        {
            OnClickFilterSaveBtn();
            return Observable.Timer(TimeSpan.FromSeconds(CDefines.BUTTONDELAYTIME)).AsSingleUnitObservable();
        }).AddTo(this);
    }

    private void SetItemInfos()
    {         
        if (ItemInfos == null) ItemInfos = new List<StyleitemUIinfo>();
        if (ItemInfos.Count == 0)
        {
            MEMBER_TYPE memberType = ShopUIInfo.GetMemberType();
            List<StylingList> styleList = CStylingItemInvenManager.Instance.GetStyleItemListByTab(2, memberType);
            foreach (var item in styleList)
            {
                //table
                StylingItemData styleData = CMemberAvatarDataManager.Instance.GetStylingItemData(item.style_id);
                if (styleData == null)
                {
                    CDebug.LogError($"PageMemberFilter.Init() styleData of gdid:{item.style_id} is null");
                    continue;
                }

                StyleShopGoodsData goodsData = CMemberAvatarDataManager.Instance.GetShopGoodsData(item.style_id);

                StyleitemUIinfo info = new StyleitemUIinfo
                {
                    StyleItemData = styleData,
                    StyleInfo = item,
                    StyleGoodsData = goodsData
                };
                ItemInfos.Add(info);
            }

            SetStyleItemInfosByItemType();
            
            // for (int i = 0; i < FilterGroup.Count; i++)
            // {
            //     FilterGroup[i].SetAllActiveFilterButtons(true);
            // }
        }
    }

    private void SetStyleItemInfosByItemType()
    {        
        CurrentTab = ShopUIInfo.GetCurrentTab();

        STYLE_ITEM_TYPE itemType = STYLE_ITEM_TYPE.NONE;
        switch (CurrentTab)
        {
            case STYLING_TAB.HAIR:
                itemType = STYLE_ITEM_TYPE.HAIR;
                break;
            case STYLING_TAB.SKIN:
                itemType = STYLE_ITEM_TYPE.SKIN;
                break;
            case STYLING_TAB.HAIRACC:
                itemType = STYLE_ITEM_TYPE.ACC_HEAD;
                break;
            case STYLING_TAB.FACEACC:
                itemType = STYLE_ITEM_TYPE.ACC_FACE;
                break;
            case STYLING_TAB.BODYACC:
                itemType = STYLE_ITEM_TYPE.ACC_BODY;
                break;
        }

        
        ItemInfosByType = ItemInfos.FindAll(x => x.StyleItemData.ItemType == itemType);
    }

    public void SetActiveFilterBtn()
    {
        bool isActive = !gameObject.activeSelf;

        if (isActive)
        {
            SetItemInfos();
        }
        else
        {
            Release();
        }

        SetActive(isActive);
    }

    public void SetOwnTypeState(FILTER_ITEM_OWN_TYPE ownType, bool isActive)
    {
        // put in each owntype for sorting 
        if (CurEachOwnType == null) return;
        if (CurEachOwnType.ContainsKey(ownType))
        {
            CurEachOwnType[ownType] = isActive;
        }
        else
        {
            CurEachOwnType.Add(ownType, isActive);
        }

        //button object control
        ObjMemberFilterGroup filterGrp = FilterGroup[0];
        if (ownType == FILTER_ITEM_OWN_TYPE.ALL)
        {
            filterGrp.SetActiveAllFilterButtons(CurEachOwnType, FILTER_ITEM_OWN_TYPE.ALL);
        }
        else
        {
            filterGrp.SetActiveFilterButton(ownType, isActive);
        }

        SetActiveFilterSaveButton();
    }

    public void InitAllFilterButtons()
    {
        foreach (var filter in FilterGroup)
        {
            filter.SetAllActiveFilterButtons(true);
            //filter.SetPrevActiveAllFilterButtons();
        }
        SetFilter();
    }


    public void SetActiveFilterSaveButton()
    {
        SaveDisableObj.SetActive(FilterGroup[0].GetAllButtonHide() || FilterGroup[1].GetAllButtonHide());
    }

    private bool IsAllFilterButtonHide()
    {
        foreach (var fliter in FilterGroup)
        {
            if (!fliter.GetAllButtonHide())
            {
                return false;
            }
        }
        return true;
    }

    public void SetBuffTypeState(int idx, bool isActive)
    {
        if (CurEachBuffData == null) return;
        if (CurEachBuffData.ContainsKey(idx))
        {
            CurEachBuffData[idx] = isActive;
        }
        else
        {
            CurEachBuffData.Add(idx, isActive);
        }

        ObjMemberFilterGroup filterGrp = FilterGroup[1];
        if (idx == 0)
        {
            filterGrp.SetActiveAllFilterButtons(CurEachBuffData, 0);
        }
        else
        {
            filterGrp.SetActiveFilterButton(idx, isActive);
        }

        SetActiveFilterSaveButton();
    }

    private void SetActiveFilterSaveButton(bool bActive)
    {
        SaveDisableObj.SetActive(bActive);
    }

    public void SetItemInfosByType(List<StyleitemUIinfo> ItemInfosByType)
    {
        this.ItemInfosByType = ItemInfosByType;
    }


    public List<StyleitemUIinfo> GetFilteredItemInfos()
    {
        List<StyleitemUIinfo> localFilteredItems = new List<StyleitemUIinfo>();
        // Sort out taking all filter groups sort info
        List<FilterSortInfo> ownTypeFilterSortInfos = FilterGroup[0].GetFilterSortInfos();
        List<FilterSortInfo> allTypeFilter = ownTypeFilterSortInfos
                                            .Where(info => info.ActiveSelf && info.OwnType == FILTER_ITEM_OWN_TYPE.ALL)
                                            .ToList();
        // if all type filter is true, pass below
        if (allTypeFilter.Count == 0)
        {
            foreach (var info in ownTypeFilterSortInfos)
            {
                if (info.ActiveSelf)
                {
                    int having = 0;
                    if (info.OwnType == FILTER_ITEM_OWN_TYPE.OWN)
                    {
                        having = 1;
                    }
                    else if (info.OwnType == FILTER_ITEM_OWN_TYPE.NOT_OWN)
                    {
                        having = 0;
                    }
                    localFilteredItems = ItemInfosByType.Where(item => item.StyleInfo != null && item.StyleInfo.having == having).ToList();
                }
            }
        }
        else
        {
            // if all type filter is true, pass below
            localFilteredItems = ItemInfosByType;
        }

        // it's not sort if all filger type is true
        // but if not, sort only the elements that are true
        List<FilterSortInfo> buffTypeFilterSortInfos = FilterGroup[1].GetFilterSortInfos();
        List<FilterSortInfo> allBuffTypeFilter = buffTypeFilterSortInfos
                                            .Where(info => info.ActiveSelf && info.CurIndex == 0)
                                            .ToList();

        // if all type filter is true, pass below
        if (allBuffTypeFilter.Count == 0)
        {
            List<FilterSortInfo> FilteredBuffInfo = new List<FilterSortInfo>();
            foreach (var info in buffTypeFilterSortInfos)
            {
                if (info.ActiveSelf)
                {
                    FilteredBuffInfo.Add(info);
                }
            }

            var filteredBuffTypes = FilteredBuffInfo
                                .Where(info => info.BuffFilterData != null)
                                .Select(info => info.BuffFilterData.BuffType)
                                .Distinct();



            MEMBER_TYPE memberType = ShopUIInfo.GetMemberType();
            List<StyleItemBuffData> memberBuffDatas = CMemberAvatarDataManager.Instance.GetMemberStyleItemBuffData(memberType);
            List<int> matchingBuffGroupIDs = memberBuffDatas
                                            .Where(buff => filteredBuffTypes.Contains(buff.BuffType))
                                            .Select(buff => buff.BuffGroupID)
                                            .ToList();

            localFilteredItems = localFilteredItems
                                .Where(item => item.StyleItemData != null && matchingBuffGroupIDs.Contains(item.StyleItemData.BuffGroupID))
                                .ToList();
        }

        //FilteredItemInfos = localFilteredItems;
        StyleitemUIinfo fInfo = localFilteredItems.FirstOrDefault();
        if (fInfo != null && fInfo.StyleItemData != null)
        {
            STYLE_ITEM_TYPE itemType = fInfo.StyleItemData.ItemType;
            switch (itemType)
            {
                case STYLE_ITEM_TYPE.ACC_HEAD:
                case STYLE_ITEM_TYPE.ACC_FACE:
                case STYLE_ITEM_TYPE.ACC_BODY:
                    localFilteredItems.Add(new StyleitemUIinfo());
                    break;
            }
        }

        return localFilteredItems;
    }

    private void OnClickFilterSaveBtn()
    {
        if (SaveDisableObj.activeSelf)
        {
            return;
        }
        // List<StyleitemUIinfo> localFilteredItems = GetFilteredItemInfos();

        // ShopUIInfo.UpdateStyleItemListFromFilter(localFilteredItems);

        // for (int i = 0; i < FilterGroup.Count; i++)
        // {
        //     FilterGroup[i].SetPrevActiveAllFilterButtons();
        // }
        SetFilter();
        SetActive(false);
        // Release();
    }

    public void SetFilter()
    {        
        List<StyleitemUIinfo> localFilteredItems = GetFilteredItemInfos();
        
        ShopUIInfo.UpdateStyleItemListFromFilter(localFilteredItems);

        for (int i = 0; i < FilterGroup.Count; i++)
        {
            FilterGroup[i].SetPrevActiveAllFilterButtons();
        }

        Release();
    }

    private void OnCkickClose()
    {
        SetActive(false);
        SaveDisableObj.SetActive(false);
        for (int i = 0; i < FilterGroup.Count; i++)
        {
            FilterGroup[i].SetPrevActiveAllFilterButtonsBySelectObjState();//.SetAllActiveFilterButtons(true);
        }
        Release();
    }
     

    public void Release()
    {
        if (ItemInfos != null)
        {
            ItemInfos.Clear();
            ItemInfos = null;
        }

        if (ItemInfosByType != null)
        {
            ItemInfosByType.Clear();
            ItemInfosByType = null;
        }
    }
}
