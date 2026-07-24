using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SuperScrollView;
using System.Linq;
using Game.RestAPI;
using UniRx;
using UnityEngine.UI;
using CitizenUI;

public class MemberStyleShopItemList : MonoBehaviour
{
    [SerializeField] private LoopListView2 Scroll;
    [SerializeField] private Text NoItemText; 
    private PageMemberStyleShopInfo MemberStyleShopInfo;
    private List<StyleitemUIinfo> StylieItemList;// = new List<styleitemUIinfo>();
    private int ScrollCount = 0;
    private bool IsPage = true;
    private bool IsSelectUI = true;
    private const int ROW_COUNT = 4;
    private STYLING_TAB CurrentTab;
    private System.Action RefItemInfoEvent;
    private PageCitizenMiddleUI MiddleUI;

    public void Init(PageCitizenMiddleUI midUI, STYLING_TAB tab, System.Action refItemInfoEvt = null)
    {
        MiddleUI = midUI;
        CurrentTab = tab;
        RefItemInfoEvent = refItemInfoEvt;

        if (Scroll.IsListViewInited == false)
        {
            Scroll.InitListView(ScrollCount, OnGetItemByIndex);
        }

        IsPage = true;
        IsSelectUI = true;
    }

    public void SetData(List<StyleitemUIinfo> item, PageMemberStyleShopInfo Info, PageMemberFilter filter)
    {
        StylieItemList = item;
        MemberStyleShopInfo = Info;

        switch (CurrentTab)
        {
            case STYLING_TAB.HAIRACC:
            case STYLING_TAB.FACEACC:
            case STYLING_TAB.BODYACC:
                //StylieItemList.Add(new StyleitemUIinfo());
                StylieItemList = StylieItemList.OrderBy(si => si.StyleItemData != null).ToList();
                break;
        }

        filter.SetItemInfosByType(StylieItemList);
        StylieItemList = filter.GetFilteredItemInfos();

        SortByTab();

        SetActiveNoItemText();
        //List<StyleitemUIinfo> filteredInfos = filter.GetFilteredItemInfos();

        SetUI();
    }
    
    private void SortByTab()
    {
        switch (CurrentTab)
        {
            case STYLING_TAB.HAIRACC:
            case STYLING_TAB.FACEACC:
            case STYLING_TAB.BODYACC:
                StylieItemList = SortAccesoryItems();
                break;
            default:
                StylieItemList = SortPartItems();
                break;
        }
    }

    private List<StyleitemUIinfo> SortAccesoryItems()
    {
        return StylieItemList
                //null first
                .OrderBy(x => (x == null || x.StyleInfo == null || x.StyleGoodsData == null) ? 0 : 1)
                //sorting isSale==false
                .ThenBy(x => (x != null && x.StyleGoodsData != null && !x.StyleGoodsData.isSale) ? 1 : 0)
                //sorting having==0
                .ThenBy(x => (x != null && x.StyleInfo != null && x.StyleInfo.having == 0) ? 0 : 1)
                //sorting by sort value
                .ThenBy(x => x?.StyleGoodsData?.SortValue ?? int.MaxValue)
                .ToList();
    }

    private List<StyleitemUIinfo> SortPartItems()
    {
        return StylieItemList
                //sorting isSale==false
                .OrderBy(x => (x?.StyleGoodsData != null && !x.StyleGoodsData.isSale) ? 2 : 0)
                //sorting having==0
                .ThenBy(x => x?.StyleInfo?.having == 0 ? 0 : x?.StyleInfo?.having == 1 ? 1 : 2)
                //sorting by sort value
                .ThenBy(x => x?.StyleGoodsData?.SortValue ?? int.MaxValue)
                .ToList();
    }

    public List<StyleitemUIinfo> GetStyleItemList()
    {
        return StylieItemList;
    }

    public STYLING_TAB GetCurrentTab()
    {
        return CurrentTab;
    }

    public void UpdateStyleItemListFromFilter(List<StyleitemUIinfo> itemInfos)
    {
        if (StylieItemList != null)
        {
            StylieItemList.Clear();
            if (itemInfos != null)
            {
                // switch (CurrentTab)
                // {
                //     case STYLING_TAB.HAIRACC:
                //     case STYLING_TAB.FACEACC:
                //     case STYLING_TAB.BODYACC:
                //         StylieItemList.Add(new StyleitemUIinfo());
                //         break;
                // }
                foreach (var info in itemInfos)
                {
                    StylieItemList.Add(info);
                }
            }

            SortByTab();

            NoItemText.text = CStringDataManager.Instance.GetStringData("ui_filter_check_none");
            SetActiveNoItemText();
            SetUI();
        }
    }

    public void SetUI()
    {
        int originCnt = StylieItemList.Count;
        int divide = originCnt / ROW_COUNT;
        int remain = originCnt % ROW_COUNT;

        ScrollCount = divide + (remain > 0 ? 1 : 0);

        Scroll.SetListItemCount(ScrollCount, false, true);
        RefreshScroll();
    }

    public void RefreshScroll()
    {
        Scroll.RefreshAllShownItem();
    }

    private void SetActiveNoItemText()
    {
        if (StylieItemList.Count == 0)
        {
            NoItemText.gameObject.SetActive(true);
        }
        else
        {
            NoItemText.gameObject.SetActive(false);
        }

    }

    private LoopListViewItem2 OnGetItemByIndex(LoopListView2 listView, int index)
    {
        if (ScrollCount <= index) return null;
        if (0 > index) return null;

        var strPrefabName = listView.ItemPrefabDataList.First().mItemPrefab.name;
        var scrollItem = listView.NewListViewItem(strPrefabName);
        var item = scrollItem.GetComponent<ObjStyleShopItemList>();

        if (scrollItem.IsInitHandlerCalled == false)
        {
            scrollItem.IsInitHandlerCalled = true;
            item.Init();
        }

        item.SetCurrentTab(CurrentTab);

        int startIdx = index * ROW_COUNT;
        int endCnt = startIdx + (ROW_COUNT - 1);

        var list = GetStyleItemListRange(startIdx, endCnt);
        item.SetData(list, IsPage, IsSelectUI, OnClickItem, OnClickReleaseAcc, UpdateShopScrollItemsAndRefresh, UpdateCurrentMemberStyleItemInfo, ResetPutOnItem);

        return scrollItem;
    }

    private List<StyleitemUIinfo> GetStyleItemListRange(int startIdx, int endIdx)
    {
        int nCnt = ROW_COUNT;
        var list = StylieItemList;
        if (list.Count <= endIdx)
        {
            nCnt = list.Count % ROW_COUNT;
        }

        return list.GetRange(startIdx, nCnt);
    }

    public void OnClickItem(StyleitemUIinfo itemInfo)
    {
        //put on item
        MemberStyleShopInfo.RegistStyleItemToSlot(itemInfo);
    }


    public void UpdateShopScrollItemsAndRefresh(StyleitemUIinfo itemInfo)
    {
        if (MemberStyleShopInfo != null)
        {
            MemberStyleShopInfo.UpdateMemberSlotItem(itemInfo);
            
            foreach (var item in StylieItemList)
            {
                if (item.StyleInfo == null) continue;


                StylingList info = CStylingItemInvenManager.Instance.GetStylingListByID(item.StyleInfo.style_id);
                if (info != null)
                {
                    item.StyleInfo = info;
                }
            }
        }


        RefreshScroll();

        if (RefItemInfoEvent != null)
        {
            RefItemInfoEvent.Invoke();
        }
    }

    public void UpdateCurrentMemberStyleItemInfo(STYLE_ITEM_TYPE itemType)
    {
        MemberStyleShopInfo.UpdateMemberAvatarCurItemInfo(itemType);
        MemberStyleShopInfo.UpdateBuyResetButtonStatus();

        MiddleUI.SetSubTabOfEachTabNewMark(MemberStyleShopInfo.GetCurrentMemberType());
    }

    public void SavePurchasedItems(List<long> purchasedItemIds)
    {
        if (MemberStyleShopInfo != null)
        {
            MemberStyleShopInfo.UpdateAvatarPutOnItems(purchasedItemIds);
        }
    }

    public void SavePurchasedItems(List<StylingList> itemInfos)
    {
        if (MemberStyleShopInfo != null)
        {
            MemberStyleShopInfo.UpdateAvatarPutOnItems(itemInfos);
        }
    }

    public void ResetPutOnItem(StyleitemUIinfo itemInfo)
    {
        MemberStyleShopInfo.ResetPutonItem(itemInfo);
    }


    private void OnClickReleaseAcc(STYLING_TAB tab)
    {
        STYLE_ITEM_TYPE AccType = STYLE_ITEM_TYPE.NONE;
        switch (tab)
        {
            case STYLING_TAB.HAIRACC: AccType = STYLE_ITEM_TYPE.ACC_HEAD; break;
            case STYLING_TAB.FACEACC: AccType = STYLE_ITEM_TYPE.ACC_FACE; break;
            case STYLING_TAB.BODYACC: AccType = STYLE_ITEM_TYPE.ACC_BODY; break;
        }
        MemberStyleShopInfo.ReleaseStyleItemFromSlot(AccType);
    }
}
