using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using Game.RestAPI;
using CitizenUI;
using Cysharp.Threading.Tasks;

public class PageClosetInfo : MonoBehaviour
{
    [SerializeField] private PageMemberClosetInfo MemberClosetInfo = null;
    [SerializeField] private DropdownExt DDExt;
    [SerializeField] private GameObject SlotONOFFObj;
    [SerializeField] private TabGroup Tab;
    [SerializeField] private MemberStyleItemList StyleItemInfos;

    private Image[] TabNewImgs;
    private PageCitizenMiddleUI MiddleUI;
    private List<StyleitemUIinfo> ItemInfos = new List<StyleitemUIinfo>();

    private MEMBER_TYPE CurrentMemberType = MEMBER_TYPE.NONE;
    private int prevDropDownValue;
    private SingleAssignmentDisposable DropDownDisposable;
    private SingleAssignmentDisposable ApiDisposable;

    public void Init(PageCitizenMiddleUI midUI)
    {
        MiddleUI = midUI;

        SetActiveDropDown(false);
        
        if (Tab != null)
        {
            SetTabNewImgs();
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


        InitMemberClosetInfo(CMemberAvatarManager.Instance.GetUICurMemberType());

        DDExt.value = (CurrentMemberType - MEMBER_TYPE.JUNGWON);
        prevDropDownValue = DDExt.value;
        SetDropDown();

    }

    public void InitMemberClosetInfo(MEMBER_TYPE mType, bool isChangeMember = false)
    {
        SlotONOFFObj.SetActive(true);

        if (MemberClosetInfo != null)
        {
            SetMemberType(mType);
            MemberClosetInfo.Init(StyleItemInfos, CurrentMemberType, MiddleUI, this, isChangeMember);
        }

        SetClosetItemIinfo();
        
        InitTab();
    }

    public void InitTab()
    {
        Tab.InitSelectTab(0);
        //UpdateAllTabNewMark();
        SetPageList(0);
        //SetAllSubTabNewMark();
    }

    public void SetMemberType(MEMBER_TYPE mType)
    {
        CurrentMemberType = mType;
    }

    private void SetClosetItemIinfo()
    {
        if (ItemInfos.Count == 0)
        {
            ItemInfos = GetCurrentItemInfos();
        }
    }
    
    public List<StyleitemUIinfo> GetCurrentItemInfos()
    {
        List<StyleitemUIinfo> _itemInfos = new List<StyleitemUIinfo>();
        
            //svr data
        List<StylingList> styleList = CStylingItemInvenManager.Instance.GetStyleItemListByTab(1, CurrentMemberType);
        foreach (var item in styleList)
        {
            //table
            StylingItemData styleData = CMemberAvatarDataManager.Instance.GetStylingItemData(item.style_id);
            if (styleData == null)
            {
                CDebug.LogError($"PageClosetInfo.Init() styleData of gdid:{item.style_id} is null");
                continue;
            }

            StyleitemUIinfo info = new StyleitemUIinfo
            {
                StyleItemData = styleData,
                StyleInfo = item
            };
            _itemInfos.Add(info);
        }

        return _itemInfos;
    }

    public void SetAllSubTabNewMark()
    {
        SetTabNewImgs();
        for (STYLING_TAB tab = STYLING_TAB.HAIR; tab < STYLING_TAB.MAX; tab++)
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
            SetSubTabNewMard(tab, itemType);
        }
    }

    private void SetTabNewImgs()
    {
        if (TabNewImgs == null)
        {
            TabNewImgs = new Image[(int)STYLING_TAB.MAX];
            for (STYLING_TAB tab = STYLING_TAB.HAIR ; tab < STYLING_TAB.MAX; tab++)
            {
                TabUI tabUI = Tab.GetTabUIByIdx((int)tab);
                if (tabUI != null)
                {
                    GameObject newImgObj = tabUI.transform.Find("obj_new/New_Info/Image").gameObject;
                    if (newImgObj != null)
                    {
                        TabNewImgs[(int)tab] = newImgObj.GetComponent<Image>();
                        TabNewImgs[(int)tab].enabled = false;
                    }
                }
            }
        }
    }

    private void SetSubTabNewMard(STYLING_TAB tab, STYLE_ITEM_TYPE itemType)
    {
        if (StyleItemInfos != null)
        {
            bool isEnable = false;
            List<StyleitemUIinfo> filteredItems = ItemInfos.FindAll(x => x.StyleItemData.ItemType == itemType);
            if (filteredItems != null && filteredItems.Count > 0)
            {
                isEnable = IsThereNewItem(filteredItems);
            }

            TabNewImgs[(int)tab].enabled = isEnable;
        }
    }


    private bool IsThereNewItem(List<StyleitemUIinfo> items)
    {
        var newItems = items.Where(item => item.StyleInfo != null && item.StyleInfo.new_flag == 1).ToList();
        return newItems.Count > 0;
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
            StyleItemInfos.Init(tab);
            StyleItemInfos.SetData(items, MemberClosetInfo);
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
        ApiDisposable.Disposable = APIHelper.SNGService.Req_AvatarStylingRead(((int)CurrentMemberType).ToString(), "1", (pageNum + 1).ToString())
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

        SetDropDownNewMark(DDExt.gameObject, CurrentMemberType);

        SetActiveDropDown(true);

        if (DropDownDisposable != null)
        {
            DropDownDisposable.Dispose();
        }
        DropDownDisposable = new SingleAssignmentDisposable();
        DropDownDisposable.Disposable = MessageBroker.Default.Receive<DropdownExt.DropdownShowEvent>().Subscribe(_ =>
        {
            ClosetDropdownShow().Forget();
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

    private async UniTaskVoid ClosetDropdownShow()
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
                SetDropDownNewMark(item.gameObject, (MEMBER_TYPE)((i - 1) + (int)MEMBER_TYPE.JUNGWON));
                // Transform imgObj = item.Find("obj_new/New_Info/Image");
                // if (imgObj != null)
                // {
                //     Image newImg = imgObj.GetComponent<Image>();
                // }
            }
            else
            {
                CDebug.LogError($"compoenent not found : {item.name}");
            }
        }
        // else
        // {
        //     CDebug.LogError("ScrollRect component not found on Dropdown.");
        // }
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
                newAlert.alertType = NEWALERT_TYPE.CHRACTER_HOUSE_CLOSET;//CHRACTER_HOUSE_DROPDOWN;
                int memberid = (int)mType;
                newAlert.SetNewAlert(memberid);
            }
        }
    }
     


    public bool IsDifferentStyleItemCurWithPutOn()
    {
        CMemberAvatar avatar = MemberClosetInfo.GetCurrentMemberAvatar();

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
            MiddleUI.ChangeMember(1, value, ChangeMemberClosetInfo);
        
            prevDropDownValue = value;
            DDExt.value = value;
            SetDropDownNewMark(DDExt.gameObject, memberType);
        }
    }

    private void CancelMemberChange()
    {
        DDExt.value = prevDropDownValue;
    }

    private void ChangeMemberClosetInfo(MEMBER_TYPE memberType)
    {
        CStylingItemInvenManager.Instance.DisposeAvatarApi();
        InitMemberClosetInfo(memberType, true);
    }

    public void Release()
    {
        if (MemberClosetInfo != null)
        {
            MemberClosetInfo.Release();
        }

        if (ItemInfos != null)
        {
            ItemInfos.Clear();
        }

        if (DropDownDisposable != null)
            {
                DropDownDisposable.Dispose();
                DropDownDisposable = null;
            }
        CStylingItemInvenManager.Instance.ClearStyleListDicByTab(1, CurrentMemberType);
    }
}


public class StyleitemUIinfo
{
    public StylingItemData StyleItemData;
    public StylingList StyleInfo;

    public StyleShopGoodsData StyleGoodsData;
}