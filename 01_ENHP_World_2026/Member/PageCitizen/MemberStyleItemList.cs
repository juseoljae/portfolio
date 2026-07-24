using System;//
using System.Collections.Generic;
using UnityEngine;
using SuperScrollView;
using System.Linq;
using Game.RestAPI;

public class MemberStyleItemList : MonoBehaviour
{
    [SerializeField] private LoopListView2 Scroll;

    private PageMemberClosetInfo ClosetInfo;
    private List<StyleitemUIinfo> StylieItemList;// = new List<styleitemUIinfo>();
    private int ScrollCount = 0;
    private bool IsPage = true;
    private bool IsSelectUI = true;
    private const int ROW_COUNT = 4;
    private STYLING_TAB CurrentTab;
    private CMemberAvatar CurMemberAvatar;
    private Action<StyleitemUIinfo> OnClickSelect = null;

    public Action<StyleitemUIinfo> SetOnClickSelect { set { OnClickSelect = value; } }

    public void Init(STYLING_TAB tab, bool isPage = true, bool selectUI = true)
    {
        CurrentTab = tab;

        if (Scroll.IsListViewInited == false)
        {
            Scroll.InitListView(ScrollCount, OnGetItemByIndex);
        }

        IsPage = isPage;
        IsSelectUI = selectUI;
    }

    public void SetData(List<StyleitemUIinfo> item, PageMemberClosetInfo info)
    {
        StylieItemList = item;
        ClosetInfo = info;

        CurMemberAvatar = ClosetInfo.GetCurrentMemberAvatar();

        AddEmptyAccInfo();

        SortClosetItems();
    }

    private void AddEmptyAccInfo()
    {
        switch (CurrentTab)
        {
            case STYLING_TAB.HAIRACC:
            case STYLING_TAB.FACEACC:
            case STYLING_TAB.BODYACC:
                StylieItemList.Add(new StyleitemUIinfo());
                StylieItemList = StylieItemList.OrderBy(si => si.StyleItemData != null).ToList();
                break;
        }
    }

    public void SetStyleItemList(List<StyleitemUIinfo> items)
    {
        StylieItemList = items;
        STYLE_ITEM_TYPE itemType = STYLE_ITEM_TYPE.NONE;
        switch (CurrentTab)
        {
            case STYLING_TAB.HAIR: itemType = STYLE_ITEM_TYPE.HAIR; break;
            case STYLING_TAB.SKIN: itemType = STYLE_ITEM_TYPE.SKIN; break;
            case STYLING_TAB.HAIRACC: itemType = STYLE_ITEM_TYPE.ACC_HEAD; break;
            case STYLING_TAB.FACEACC: itemType = STYLE_ITEM_TYPE.ACC_FACE; break;
            case STYLING_TAB.BODYACC: itemType = STYLE_ITEM_TYPE.ACC_BODY; break;
        }
        StylieItemList = items.FindAll(x => x.StyleItemData.ItemType == itemType);

        AddEmptyAccInfo();

        SortClosetItems();
    }

    public void SortClosetItems()
    {
        switch (CurrentTab)
        {
            case STYLING_TAB.HAIRACC:
            case STYLING_TAB.FACEACC:
            case STYLING_TAB.BODYACC:
                StylieItemList = StylieItemList
                            .OrderBy(x => (x.StyleInfo == null) ? 0 : 1)
                            .ThenBy(x => (x?.StyleInfo != null && x.StyleInfo.puton == 1) ? 0 : 1)
                            .ToList();
                break;
            default:
                StylieItemList = StylieItemList
                            .OrderBy(x => (x?.StyleInfo != null && x.StyleInfo.puton == 1) ? 0 : 1)
                            .ToList();
                break;
        }

        SetUI();
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

    private LoopListViewItem2 OnGetItemByIndex(LoopListView2 listView, int index)
    {
        if (ScrollCount <= index) return null;
        if (0 > index) return null;

        var strPrefabName = listView.ItemPrefabDataList.First().mItemPrefab.name;
        var scrollItem = listView.NewListViewItem(strPrefabName);
        var item = scrollItem.GetComponent<ObjMemberItemList>();

        if (scrollItem.IsInitHandlerCalled == false)
        {
            scrollItem.IsInitHandlerCalled = true;
            item.Init();
        }

        item.SetCurrentTab(CurrentTab);

        int startIdx = index * ROW_COUNT;
        int endCnt = startIdx + (ROW_COUNT - 1);

        var list = GetStyleItemListRange(startIdx, endCnt);
        item.SetData(list, CurMemberAvatar, IsPage, IsSelectUI, OnClickItem, OnClickReleaseAcc, UpdateInfoAndRefresh);

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
        ClosetInfo.RegistStyleItemToSlot(itemInfo);
    }


    public void UpdateInfoAndRefresh()
    {
        if (ClosetInfo != null)
        {
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
        ClosetInfo.ReleaseStyleItemFromSlot(AccType);
        SortClosetItems();
    }
}
