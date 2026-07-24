using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using Game.RestAPI;
using CitizenUI;
using Cysharp.Threading.Tasks;

public class PageStyleShopInfo : MonoBehaviour
{
    [SerializeField] private PageMemberStyleShopInfo MemberShopInfo = null;
    [SerializeField] private DropdownExt DDExt;
    [SerializeField] private GameObject SlotONOFFObj;
    [SerializeField] private TabGroup Tab;
    [SerializeField] private MemberStyleShopItemList StyleItemInfos;
    [SerializeField] private Button FilterBtn;
    [SerializeField] private Image BoardingCoinIconImg;
    [SerializeField] private Text BoardingCoinValueTxt;
    private PageCitizenMiddleUI MiddleUI;
    private List<StyleitemUIinfo> ItemInfos = new List<StyleitemUIinfo>();
    private MEMBER_TYPE CurrentMemberType = MEMBER_TYPE.NONE;
    private int prevDropDownValue;
    private SingleAssignmentDisposable FilterBtnDisposable;
    private SingleAssignmentDisposable DropDownDisposable;
    private SingleAssignmentDisposable ApiDisposable;

    public void Init(PageCitizenMiddleUI midUI)
    {
        SetActiveDropDown(false);
        
        if (Tab != null)
        {
            for (STYLE_ITEM_TYPE type = STYLE_ITEM_TYPE.HAIR; type < STYLE_ITEM_TYPE.MAX; type++)
            {
                string iconPath = CMemberAvatarDataManager.Instance.GetStyleTabIconPath(type);
                if (!string.IsNullOrEmpty(iconPath))
                {
                    TabUI tab = Tab.GetTab((int)type);
                    tab.SetIconImage(iconPath);
                }
            }
            Tab.InitOnSelectEvent(SetPageList);
        }

        MiddleUI = midUI;

        InitMemberShopInfo(CMemberAvatarManager.Instance.GetUICurMemberType());

        SetDropDown();

        DDExt.value = (CurrentMemberType - MEMBER_TYPE.JUNGWON);
        prevDropDownValue = DDExt.value;

        if (FilterBtnDisposable != null)
        {
            FilterBtnDisposable.Dispose();
        }
        FilterBtnDisposable = new SingleAssignmentDisposable();
        FilterBtnDisposable.Disposable = FilterBtn.BindToOnClick(_ =>
        {
            OnClickFilterBtn();
            return Observable.Timer(TimeSpan.FromSeconds(CDefines.BUTTONDELAYTIME)).AsSingleUnitObservable();

        }).AddTo(this);

    }

    private void InitMemberShopInfo(MEMBER_TYPE mType, bool isChangeMember = false)
    {
        SlotONOFFObj.SetActive(true);

        if (MemberShopInfo != null)
        {
            SetMemberType(mType);
            MemberShopInfo.Init(MiddleUI, StyleItemInfos, CurrentMemberType, RefreshEachSubTabNewMark, isChangeMember);
        }

        SetBoardingCoinInfo();
        SetShopItemInfos();
        InitTab();
    }

    public void InitTab()
    {
        Tab.InitSelectTab(0);
        SetPageList(0);
        //SetAllSubTabNewMark();
    }

    public void SetMemberType(MEMBER_TYPE mType)
    {
        CurrentMemberType = mType;
    }

    public MEMBER_TYPE GetMemberType()
    {
        return CurrentMemberType;
    }

    public STYLING_TAB GetCurrentTab()
    {
        return StyleItemInfos.GetCurrentTab();
    }

    private void SetBoardingCoinInfo()
    {
        ItemData itemData = CItemDataManager.Instance.GetItemData(SNGDefines.SNG_STYLING_COIN_ITEM_ID);
        if (itemData != null)
        {
            CResourceManager.Instance.LoadImage(itemData.item_resource_image, BoardingCoinIconImg);
        }

        int boardingCoinCount = 0;
        ItemList boardingCoinInfo = ItemInventoryManager.Instance.GetItemData(SNGDefines.SNG_STYLING_COIN_ITEM_ID);
        if (boardingCoinInfo != null)
        {
            boardingCoinCount = boardingCoinInfo.count;
        }

        BoardingCoinValueTxt.text = boardingCoinCount.ToCommaString();
    }

    private void SetShopItemInfos()
    {
        if (ItemInfos.Count == 0)
        {
            //svr data
            List<StylingList> styleList = CStylingItemInvenManager.Instance.GetStyleItemListByTab(2, CurrentMemberType);
            foreach (var item in styleList)
            {
                //table
                StylingItemData styleData = CMemberAvatarDataManager.Instance.GetStylingItemData(item.style_id);
                if (styleData == null)
                {
                    CDebug.LogError($"PageClosetInfo.Init() styleData of gdid:{item.style_id} is null");
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
        }
    }

    public void SetAllSubTabNewMark()
    {
        for (STYLING_TAB tab = STYLING_TAB.HAIR ; tab < STYLING_TAB.MAX; tab++)
        {
            STYLE_ITEM_TYPE itemType = STYLE_ITEM_TYPE.NONE;
            switch (tab)
            {
                case STYLING_TAB.HAIR: itemType = STYLE_ITEM_TYPE.HAIR; break;
                case STYLING_TAB.SKIN: itemType = STYLE_ITEM_TYPE.SKIN; break;
                case STYLING_TAB.HAIRACC: itemType = STYLE_ITEM_TYPE.ACC_HEAD; break;
                case STYLING_TAB.FACEACC: itemType = STYLE_ITEM_TYPE.ACC_FACE; break;
                case STYLING_TAB.BODYACC: itemType = STYLE_ITEM_TYPE.ACC_BODY; break;
            }

            if (StyleItemInfos != null)
            {
                List<StyleitemUIinfo> filteredItems = ItemInfos.FindAll(x => x.StyleItemData.ItemType == itemType);
                if (filteredItems != null && filteredItems.Count > 0)
                {
                    TabUI tabUI = Tab.GetTabUIByIdx((int)tab);
                    if (tabUI != null)
                    {
                        GameObject newImgObj = tabUI.transform.Find("obj_new/New_Info/Image").gameObject;
                        if (newImgObj != null)
                        {
                            Image newImg = newImgObj.GetComponent<Image>();
                            if (newImg != null)
                            {
                                newImg.enabled = CNewAlertManager.Instance.IsNewShopInMemberHouseByItemType((int)CurrentMemberType, itemType);////IsThereNewItem(filteredItems);
                            }
                        }
                    }
                }
            }
        }
    }

    public void SetPageList(int pageNum)
    {
        STYLE_ITEM_TYPE itemType = STYLE_ITEM_TYPE.NONE;
        STYLING_TAB tab = (STYLING_TAB)pageNum;
        switch (tab)
        {
            case STYLING_TAB.HAIR: itemType = STYLE_ITEM_TYPE.HAIR; break;
            case STYLING_TAB.SKIN: itemType = STYLE_ITEM_TYPE.SKIN; break;
            case STYLING_TAB.HAIRACC: itemType = STYLE_ITEM_TYPE.ACC_HEAD; break;
            case STYLING_TAB.FACEACC: itemType = STYLE_ITEM_TYPE.ACC_FACE; break;
            case STYLING_TAB.BODYACC: itemType = STYLE_ITEM_TYPE.ACC_BODY; break;
        }

        if (StyleItemInfos != null)
        {
            List<StyleitemUIinfo> items = ItemInfos.FindAll(x => x.StyleItemData.ItemType == itemType);
            StyleItemInfos.Init(MiddleUI, (STYLING_TAB)pageNum, SetBoardingCoinInfo);
            StyleItemInfos.SetData(items, MemberShopInfo, MiddleUI.MemberSortFilter);
        }

        UpdateNewMark(itemType, pageNum);
        RefreshDropDownNewMark(CurrentMemberType);
    }

    private void UpdateNewMark(STYLE_ITEM_TYPE itemType, int pageNum)
    {
        SetAllSubTabNewMark();

        if (ApiDisposable != null)
        {
            ApiDisposable.Dispose();
        }
        ApiDisposable = new SingleAssignmentDisposable();
        ApiDisposable.Disposable = APIHelper.SNGService.Req_AvatarStylingRead(((int)CurrentMemberType).ToString(), "2", (pageNum + 1).ToString())
                                    .Subscribe(result =>
                                    {
                                        if (itemType != STYLE_ITEM_TYPE.NONE)
                                        {
                                            RefreshItemNewInfosBySubTab(itemType);
                                            
                                            MiddleUI.SetTabNewMark();
                                        }
                                    });
    }


    private void RefreshItemNewInfosBySubTab(STYLE_ITEM_TYPE itemType)
    {
        List<StyleitemUIinfo> items = ItemInfos.FindAll(x => x.StyleItemData.ItemType == itemType);
        foreach (var item in items)
        {
            item.StyleInfo.new_flag = 0;
        }
    }

    private void SetDropDown()
    {
        int optionIdx = 0;
        for (MEMBER_TYPE member = MEMBER_TYPE.JUNGWON; member <= MEMBER_TYPE.MAX; member++)
        {
            DDExt.options[optionIdx++].text = SNGDataManager.Instance.GetMemberName(member);
        }

        //DDExt.value = (CurrentMemberType - MEMBER_TYPE.JUNGWON);

        SetDropDownNewMark(DDExt.gameObject, CurrentMemberType);

        SetActiveDropDown(true);

        if (DropDownDisposable != null)
        {
            DropDownDisposable.Dispose();
        }
        DropDownDisposable = new SingleAssignmentDisposable();
        DropDownDisposable.Disposable = MessageBroker.Default.Receive<DropdownExt.DropdownShowEvent>().Subscribe(_ =>
        {
            Debug.Log($"DropdownExt.ShowEvent: ");
            DropdownShow().Forget();
        });
        
        DDExt.RefreshShownValue();
    }

    private void SetActiveDropDown(bool isActive)
    {
        if (DDExt != null)
        {
            DDExt.gameObject.SetActive(isActive);
        }
    }

    private async UniTaskVoid DropdownShow()
    {
        DDExt.onValueChanged.RemoveAllListeners();

        GameObject dropdownList = DDExt.transform.Find("Dropdown List").gameObject;
        if (dropdownList == null)
        {
            CDebug.Log("Dropdown List not found. wait");
            await UniTask.NextFrame(this.GetCancellationTokenOnDestroy());
        }

        Transform content = dropdownList.transform.Find("Viewport/Content");
        if (content == null)
            return;

        //resizing scroll height
        RectTransform templateRect = dropdownList.GetComponent<RectTransform>();
        int itemCount = content.childCount;

        //items setting
        for (int i = 1; i < content.childCount; i++)
        {
            Transform item = content.GetChild(i);

            if (item != null)
            {
                Transform imgObj = item.Find("obj_new/New_Info/Image");
                if (imgObj != null)
                {
                    SetDropDownNewMark(item.gameObject, (MEMBER_TYPE)((i - 1) + (int)MEMBER_TYPE.JUNGWON));
                }
            }
            else
            {
                CDebug.LogError($"compoenent not found : {item.name}");
            }
        }
        
        DDExt.onValueChanged.AddListener(OnClickDropdownMemberChange);
    }
    

    public void RefreshDropDownNewMark(MEMBER_TYPE mType)
    {
        bool noShownItem = false;
        foreach (var item in ItemInfos)
        {
            if (item.StyleInfo != null && item.StyleInfo.new_flag == 1)
            {
                noShownItem = true;
                break;
            }
        }

        if (!noShownItem)
        {
            SetDropDownNewMark(DDExt.gameObject, mType);
        }
    }

    private void SetDropDownNewMark(GameObject obj, MEMBER_TYPE mType)
    {
        GameObject newRootObj = obj.transform.Find("obj_new").gameObject;
        if (newRootObj != null)
        {
            var newAlert = newRootObj.GetComponent<CNewAlert>();
            if (newAlert != null)
            {
                newAlert.alertType = NEWALERT_TYPE.CHRACTER_HOUSE_SHOP;//CHRACTER_HOUSE_DROPDOWN;
                int memberid = (int)mType;
                newAlert.SetNewAlert(memberid);
            }
        }
    }

    public bool IsDifferentStyleItemCurWithPutOn()
    {
        CMemberAvatar avatar = MemberShopInfo.GetCurrentMemberAvatar();

        if (avatar != null)
        {
            return avatar.IsDifferentCurWithPutOn();
        }

        return false;
    }

    private void OnClickDropdownMemberChange(int value)
    {
        if (DDExt.value == prevDropDownValue) return;

        bool isDifferent = IsDifferentStyleItemCurWithPutOn();
        if (isDifferent)
        {
            MiddleUI.OpenResetPopup(ChangeMember, value, CancelMemberChange);
        }
        else
        {
            ChangeMember(value);
        }
    }

    private void ChangeMember(int value)
    {
        MEMBER_TYPE memberType = (MEMBER_TYPE)((int)MEMBER_TYPE.JUNGWON + value);
        if (CurrentMemberType != memberType)
        {
            ItemInfos.Clear();
            //prevmember need to match curequip with puton
            MemberShopInfo.SetPutonItemsFromEquipItems(CurrentMemberType);
            MiddleUI.ChangeMember(2, value, ChangeMemberStyleShopInfo);

            prevDropDownValue = value;
            DDExt.value = value;


            SetDropDownNewMark(DDExt.gameObject, memberType);
        }
    }

    private void CancelMemberChange()
    {
        DDExt.value = prevDropDownValue;
    }

    private void ChangeMemberStyleShopInfo(MEMBER_TYPE memberType)
    {
        CStylingItemInvenManager.Instance.DisposeAvatarApi();
        InitMemberShopInfo(memberType, true);
    }

    public void RefreshEachSubTabNewMark()
    {
        MiddleUI.SetSubTabOfEachTabNewMark();
    }

    public List<StyleitemUIinfo> GetShopStyleItemInfos()
    {
        return StyleItemInfos.GetStyleItemList();
    }

    public void UpdateStyleItemListFromFilter(List<StyleitemUIinfo> items)
    {
        StyleItemInfos.UpdateStyleItemListFromFilter(items);
    }

    private void OnClickFilterBtn()
    {
        MiddleUI.MemberSortFilter.SetActiveFilterBtn();
    }

    public void Release()
    {
        if (MemberShopInfo != null)
        {
            MemberShopInfo.Release();
        }

        if (DropDownDisposable != null)
        {
            DropDownDisposable.Dispose();
            DropDownDisposable = null;
        }

        CStylingItemInvenManager.Instance.ClearStyleListDicByTab(2, CurrentMemberType);

        if (ItemInfos != null)
        {
            ItemInfos.Clear();
        }
    }
}
