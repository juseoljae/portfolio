using UnityEngine;
using UniRx;
using System;
using System.Collections.Generic;
using SNG;
using UnityEngine.UI;
using Game.RestAPI;
using TouchScript.Examples.Tap;

namespace CitizenUI
{
    public class PageCitizenMiddleUI : MonoBehaviour
    {
        public enum CHAR_TAB
        {
            NONE = -1,
            CHAR = 0,
            CLOSET = 1,
            SHOP = 2
        }

        public enum CHAR_SELECTION
        {
            MEMBER = 1,
            CITIZEN = 2
        }

        [SerializeField] private TabGroup Tab;
        [SerializeField] private Button MemberBtn;
        [SerializeField] private Button CitizenBtn;
        [SerializeField] private CNewAlert CitizenNewAlert;
        [SerializeField] private Image CitizenRedDotImg;
        [SerializeField] private GameObject CharacterInfoObj;
        [SerializeField] private GameObject MemberInfoObj;
        [SerializeField] private GameObject MemberRawImgObj;
        [SerializeField] private GameObject CitizenInfoObj;
        [SerializeField] private GameObject ClosetObj;
        [SerializeField] private GameObject ShopObj;
        [SerializeField] private GameObject MemberBtnSelectedObj;
        [SerializeField] private GameObject CitizenBtnSelectedObj;
        [SerializeField] private GameObject MemberBottomInfoObj;
        [SerializeField] private GameObject CitizenBottomInfoObj;
        [SerializeField] private PageCitizenFilterContetns FilterContents;
        [SerializeField] private PageMemberInfo MemberInfoUI;
        [SerializeField] private PageCitizenInfo CitizenInfoUI;
        [SerializeField] private PageClosetInfo ClosetInfoUI;
        [SerializeField] private PageStyleShopInfo StyleShopInfoUI;
        [SerializeField] private CitizenScrollUI ScrollUI;
        [SerializeField] private PageCitizenSortFilter SortFilter;
        public PageMemberFilter MemberSortFilter;

        private CHAR_TAB CurrentTab;
        private CHAR_SELECTION CurCharSelection;

        private SingleAssignmentDisposable MemberBtnDisposable;
        private SingleAssignmentDisposable CitizenBtnDisposable;
        public UICharacterController UICharacter = null;
        
        private int NaviTabNum;

        // public GameObject CitizenRowImageObj;
        //private GameObject MemberClosetRowImageObj;

        public void Init()
        {
            if (Tab != null)
            {
                Tab.InitOnSelectEvent(SetPageList, IsCanMoveTab);
                //Tab.InitOnSelectEvent(SetPageList);
                Tab.InitSelectTab(0);
            }

            if (UICharacter == null)
            {
                UICharacter = this.gameObject.AddComponent<UICharacterController>();
                UICharacter.SetUICharacterBase();
            }



            SetMemberBtnState(true);
            if (MemberBtn != null)
            {
                if (MemberBtnDisposable == null)
                {
                    MemberBtnDisposable = new SingleAssignmentDisposable();
                }
                else
                {
                    MemberBtnDisposable.Dispose();
                    MemberBtnDisposable = new SingleAssignmentDisposable();
                }

                MemberBtnDisposable.Disposable = MemberBtn.BindToOnClick(_ =>
                {
                    OnClickSelectMemberInfo();

                    return Observable.Timer(TimeSpan.FromSeconds(CDefines.BUTTONDELAYTIME)).AsUnitObservable();
                }).AddTo(this);
            }

            if (CitizenBtn != null)
            {
                if (CitizenBtnDisposable == null)
                {
                    CitizenBtnDisposable = new SingleAssignmentDisposable();
                }
                else
                {
                    CitizenBtnDisposable.Dispose();
                    CitizenBtnDisposable = new SingleAssignmentDisposable();
                }

                CitizenBtnDisposable.Disposable = CitizenBtn.BindToOnClick(_ =>
                {
                    OnClickSelectCitizenInfo();

                    return Observable.Timer(TimeSpan.FromSeconds(CDefines.BUTTONDELAYTIME)).AsUnitObservable();
                }).AddTo(this);
            }

            MEMBER_TYPE curMemberType = CMemberAvatarManager.Instance.GetUICurMemberType();
            if (curMemberType == MEMBER_TYPE.NONE)
            {
                CMemberAvatarManager.Instance.SetUICurMemberType(MEMBER_TYPE.JUNGWON);
            }

            CurrentTab = CHAR_TAB.NONE;

            if (MemberSortFilter != null)
            {
                MemberSortFilter.Init(StyleShopInfoUI);
                MemberSortFilter.SetActive(false);
            }

            SetPageList(NaviTabNum);
            if (NaviTabNum != 0)
            {
                Tab.InitSelectTab(NaviTabNum);
            }

        }

        public void SetActiveMemberRawImgObj(bool isActive)
        {
            if (MemberRawImgObj != null)
            {
                MemberRawImgObj.SetActive(isActive);
            }
        }

        public bool IsCanMoveTab(int nextTab)
        {
            bool isDiff = false;

            switch (CurrentTab)
            {
                case CHAR_TAB.CLOSET:
                    if ((CHAR_TAB)nextTab != CHAR_TAB.CLOSET && ClosetInfoUI != null)
                    {
                        isDiff = ClosetInfoUI.IsDifferentStyleItemCurWithPutOn();
                    }
                    break;
                case CHAR_TAB.SHOP:
                    if ((CHAR_TAB)nextTab != CHAR_TAB.SHOP && StyleShopInfoUI != null)
                    {
                        isDiff = StyleShopInfoUI.IsDifferentStyleItemCurWithPutOn();
                    }
                    break;
            }

            if (isDiff)
            {
                OpenResetPopup(SetNextTab, nextTab);
                return false;
            }


            return true;
        }

        public bool CanMoveOut(Action cloaseAction)
        {
            bool isDiff = false;

            switch (CurrentTab)
            {
                case CHAR_TAB.CLOSET:
                    if (ClosetInfoUI != null)
                    {
                        isDiff = ClosetInfoUI.IsDifferentStyleItemCurWithPutOn();
                    }
                    break;
                case CHAR_TAB.SHOP:
                    if (StyleShopInfoUI != null)
                    {
                        isDiff = StyleShopInfoUI.IsDifferentStyleItemCurWithPutOn();
                    }
                    break;
            }

            if (isDiff)
            {
                OpenResetPopupClose(cloaseAction);
                return false;
            }

            return true;
        }



        private void SetNextTab(int pageNum)
        {
            if (Tab != null)
            {
                Tab.InitSelectTab(pageNum);
                SetPageList(pageNum);
            }
        }

        private void CheckCitizenNewRedDot()
        {
            if (CitizenRedDotImg != null)
            {
                CitizenRedDotImg.enabled = CRedDotManager.Instance.CheckRedDot(REDDOT_TYPE.REDDOT_GETCITIZEN);
            }

            if (CitizenNewAlert != null)
            {
                CitizenNewAlert.alertType = NEWALERT_TYPE.CHARACTER_CITIZEN;
                CitizenNewAlert.SetNewAlert();
            }
        }

        public void ConfirmCitizenNewAlert()
        {
            if (CitizenNewAlert != null)
            {
                CitizenNewAlert.NewAlertConfirmed();
            }

            //CheckCitizenNewRedDot();
        }
        public void SetNavigation(int tabIdx)
        {
            NaviTabNum = tabIdx;
        }

        private void SetPageList(int pageNum)
        {
            MEMBER_TYPE curMemberType = CMemberAvatarManager.Instance.GetUICurMemberType();

            CHAR_TAB nextTxb = (CHAR_TAB)pageNum;

            if (CurrentTab == nextTxb)
            {
                return;
            }

            switch (nextTxb)
            {
                case CHAR_TAB.CHAR:
                    SetMemberBtnState(true);
                    CitizenInfoUI.ReleaseUICharObj();
                    if (CStylingItemInvenManager.Instance.GetStyleItemListByTab(0, curMemberType) == null)
                    {
                        APIHelper.SNGService.ReqSNG_Avatar(((int)curMemberType).ToString(), 0)
                            .Subscribe(result =>
                            {
                                ClosetInfoUI.Release();
                                StyleShopInfoUI.Release();
                                InitMemberUI();
                                SetActiveMemberRawImgObj(true);
                                SetTabNewMark();
                            }).AddTo(this);
                    }
                    else
                    {
                        ClosetInfoUI.Release();
                        StyleShopInfoUI.Release();
                        InitMemberUI();
                        SetActiveMemberRawImgObj(true);
                    }

                    CheckCitizenNewRedDot();

                    break;
                case CHAR_TAB.CLOSET:
                    CitizenInfoUI.ReleaseUICharObj();

                    APIHelper.SNGService.ReqSNG_Avatar(((int)curMemberType).ToString(), 1)
                        .Subscribe(result =>
                        {

                            // InitMemberUI();


                            MemberInfoUI.Release();
                            StyleShopInfoUI.Release();
                            ClosetInfoUI.Init(this);
                        }).AddTo(this);

                    break;
                case CHAR_TAB.SHOP:
                    CitizenInfoUI.ReleaseUICharObj();

                    APIHelper.SNGService.ReqSNG_Avatar(((int)curMemberType).ToString(), 2)
                        .Subscribe(result =>
                        {
                            MemberInfoUI.Release();
                            ClosetInfoUI.Release();
                            StyleShopInfoUI.Init(this);
                        }).AddTo(this);
                    break;
            }

            ShowTab((CHAR_TAB)pageNum);
        }

        public void SetTabNewMark()
        {
            for (int i = 0; i < Tab.TabCount; i++)
            {
                var tabUI = Tab.GetTabUIByIdx(i);
                var newAlert = tabUI.GetNewAlert();
                if (newAlert != null)
                {
                    if (i == 0)
                    {
                        newAlert.alertType = NEWALERT_TYPE.CHRACTER_HOUSE_DROPDOWN;
                        var redDot = tabUI.GetRedDot();
                        if (redDot != null)
                        {
                            redDot.redDotType = REDDOT_TYPE.REDDOT_GETCITIZEN;
                            redDot.SetRedDot();
                        }
                    }
                    else if (i == 1)
                        newAlert.alertType = NEWALERT_TYPE.CHRACTER_HOUSE_CLOSET;
                    else if (i == 2)
                        newAlert.alertType = NEWALERT_TYPE.CHRACTER_HOUSE_SHOP;
                    int memberid = (int)MemberInfoUI.GetCurrentMemberType();
                    newAlert.SetNewAlert(memberid);
                }
            }
        }

        public void SetSubTabOfEachTabNewMark(MEMBER_TYPE mType = MEMBER_TYPE.NONE)
        {
            if (mType != MEMBER_TYPE.NONE)
            {
                MemberInfoUI.SetMemberType(mType);
            }
            SetTabNewMark();
            ClosetInfoUI.SetAllSubTabNewMark();
        }

        public void ShowTab(CHAR_TAB tab)
        {
            CurrentTab = tab;
            CharacterInfoObj.SetActive(tab == CHAR_TAB.CHAR);
            MemberInfoObj.SetActive(tab == CHAR_TAB.CHAR);
            CitizenInfoObj.SetActive(!(tab == CHAR_TAB.CHAR));
            ClosetObj.SetActive(tab == CHAR_TAB.CLOSET);
            ShopObj.SetActive(tab == CHAR_TAB.SHOP);
        }

        public void InitMemberUI()
        {
            if (MemberInfoUI != null)
            {
                MemberInfoUI.Init(this);
                MemberInfoUI.SetNewOnTab = SetTabNewMark;
                //MemberInfoUI.SetOnClickDetailInfo = OnClickMemberDetailInfo;
            }
        }

        public void InitCitizenUI()
        {
            if (FilterContents != null)
            {
                FilterContents.InitUI();
                FilterContents.SetOnClickFilter = OnClickFilter;
                FilterContents.SetOnClickSelectAll = OnClickSelectAll;
            }

            if (CitizenInfoUI != null)
            {
                CitizenInfoUI.InitUI(this);
                CitizenInfoUI.SetOnClickSet = OnClickSetAndSetOff;
                CitizenInfoUI.SetOnClickDetailInfo = OnClickDetailInfo;
            }

            if (ScrollUI != null)
            {
                ScrollUI.Init();
                ScrollUI.SetData(CCitizenUIDataManager.Instance.GetCurrentUIDataList);
                ScrollUI.SetOnClickSelect = OnClickSelectData;
            }

            if (SortFilter != null)
            {
                SortFilter.InitUI();
                SortFilter.SetOnEventClose = OnCloseFilter;
            }

            var firstData = CCitizenUIDataManager.Instance.GetCurrentCitizenUIData;

            CCitizenUIDataManager.Instance.SetCurrentSelectID(firstData.GDID);
            ScrollUI.RefreshScroll();
            CitizenInfoUI.SetData(firstData, ConfirmCitizenNewAlert);
        }


        private void OnClickSelectAll()
        {
            //팝업에서 사용 할 데이터 세팅
            CCitizenUIDataManager.Instance.InitAllocateSetAllCitizen();

            //팝업
            CPopupService popupService = CCoreServices.GetCoreService<CPopupService>();
            var popup = popupService.OpenPopupCitizenSetAll(new PopupCitizenSetAll.Setting()
            {
                UnlockSeq = RequestUnclock
            });

            SingleAssignmentDisposable disposable = new SingleAssignmentDisposable();
            disposable.Disposable = popup.ShowAsObservable().Subscribe(res =>
            {
                UpdateCommonUI();

                disposable.Dispose();
            });

        }

        private void OnClickFilter()
        {
            if (SortFilter != null)
            {
                SortFilter.ShowUI();
            }
        }

        //필터 UI 닫은 후 이벤트
        private void OnCloseFilter(bool isSave)
        {
            if (isSave)
            {
                CCitizenUIDataManager.Instance.SetCurrentUIDatListCurrentFilter();

                UpdateCommonUICitizenList();
            }
        }

        private void OnClickSetAndSetOff(bool isAllocate)
        {
            //배치 리스트에 보낸다
            if (isAllocate)
            {
                if (CCitizenUIDataManager.Instance.MaxChangeAllocateCitizen)
                {
                    CCitizenUIDataManager.Instance.AddCurrentAllocateCitizen();
                }
                else
                {
                    SingleAssignmentDisposable disposable = new SingleAssignmentDisposable();
                    var notice = CCoreServices.GetCoreService<CPopupService>().NoticeMessage("Citizen_Manage_Popup_Exception_01");
                    disposable.Disposable = notice.ShowAsObservable().Subscribe(_ => disposable.Dispose());
                }
            }
            else
            {
                CCitizenUIDataManager.Instance.RemoveCurrentAllocateCitizen();

            }

            UpdateCommonUI();

        }

        private void UpdateCommonUI()
        {
            //정보창 업데이트
            CitizenInfoUI.UpdateData(CCitizenUIDataManager.Instance.GetCurrentCitizenUIData);

            //스크롤 업데이트
            ScrollUI.RefreshScroll();
        }

        private void UpdateCommonUICitizenList()
        {
            ScrollUI.SetData(CCitizenUIDataManager.Instance.GetCurrentUIDataList);

            UpdateCommonUI();
        }


        public void OnClickDetailInfo()
        {
            CPopupService popupService = CCoreServices.GetCoreService<CPopupService>();
            var popup = popupService.OpenPopupCitizenDetail(new PopupCitizenDetail.Setting()
            {
                UIData = CCitizenUIDataManager.Instance.GetCurrentCitizenUIData
            });

            SingleAssignmentDisposable disposable = new SingleAssignmentDisposable();
            disposable.Disposable = popup.ShowAsObservable().Subscribe(res =>
            {

                //정보 갱신
                CitizenInfoUI.UpdateData(CCitizenUIDataManager.Instance.GetCurrentCitizenUIData);

                //스크롤 업데이트
                ScrollUI.RefreshScroll();
                disposable.Dispose();
            });
        }

        //citizen info setting
        private void OnClickSelectData(CitizenUIData data)
        {
            switch (data.State)
            {
                case CITIZEN_STATE.UNLOCK:
                    {
                        RequestUnclock(data.GDID);
                    }
                    break;
                default:
                    {
                        CCitizenUIDataManager.Instance.SetCurrentSelectID(data.GDID);
                        ScrollUI.RefreshScroll();
                        CitizenInfoUI.SetData(data);
                    }
                    break;

            }
        }

        private void SetMemberBtnState(bool isMemberSelected)
        {
            if (isMemberSelected)
            {
                CurCharSelection = CHAR_SELECTION.MEMBER;
            }
            else
            {
                CurCharSelection = CHAR_SELECTION.CITIZEN;
            }

            if (MemberInfoObj != null) MemberInfoObj.SetActive(isMemberSelected);
            if (MemberBtnSelectedObj != null) MemberBtnSelectedObj.SetActive(isMemberSelected);
            if (MemberInfoUI != null) MemberInfoUI.gameObject.SetActive(isMemberSelected);
            if (MemberBottomInfoObj != null) MemberBottomInfoObj.SetActive(isMemberSelected);

            if (CitizenInfoObj != null) CitizenInfoObj.SetActive(!isMemberSelected);
            if (CitizenBtnSelectedObj != null) CitizenBtnSelectedObj.SetActive(!isMemberSelected);
            if (CitizenInfoUI != null) CitizenInfoUI.gameObject.SetActive(!isMemberSelected);
            if (CitizenBottomInfoObj != null) CitizenBottomInfoObj.SetActive(!isMemberSelected);

            if (CurCharSelection == CHAR_SELECTION.CITIZEN)
            {
                if (TutorialManager.Instance.IsEnable(TUTORIAL_MODE.TUTORIAL_FREE, TUTORIAL_LOCATION.TUTORIAL_VAMPIRBOARDINGSCHOOL))
                    TutorialManager.Instance.FreePlay(TUTORIAL_LOCATION.TUTORIAL_VAMPIRBOARDINGSCHOOL);
            }
        }

        private void OnClickSelectMemberInfo()
        {
            if (CurCharSelection == CHAR_SELECTION.MEMBER)
            {
                return;
            }
            else if (CurCharSelection == CHAR_SELECTION.CITIZEN)
            {
                CCitizenUIDataManager.Instance.ReqCitizenAllocate();
            }
            SetMemberSelectState();
        }

        public void SetMemberSelectState()
        {
            SetMemberBtnState(true);
            CheckCitizenNewRedDot();
            CitizenInfoUI.ReleaseUICharObj();
            InitMemberUI();
        }

        private void OnClickSelectCitizenInfo()
        {
            if (CurCharSelection == CHAR_SELECTION.CITIZEN)
            {
                return;
            }
            APIHelper.CitizenService.ReqCitizens().Subscribe(result =>
            {
                SetMemberBtnState(false);
                CCitizenUIDataManager.Instance.Init();
                //MemberInfoUI.ReleaseUICharObj();
                MemberInfoUI.Release();
                InitCitizenUI();
                // if (TopUI != null)
                // {
                //     TopUI.InitUI();
                //     TopUI.SetOnClickEvent(OnClickClose);
                // }

                // if (MiddleUI != null)
                // {
                //     MiddleUI.InitUI();
                // }

                //Utility.SetFxLayerDimmed(false);


            }).AddTo(this);
        }


        //Unlock Process
        public void RequestUnclock(int gdid, Action actUpdate = null)
        {

            //정보를 UNUSED로 변경 후 정보를 업데이트 시켜준다
            SingleAssignmentDisposable disposable = new SingleAssignmentDisposable();
            disposable.Disposable =
            APIHelper.CitizenService.ReqUnlock(gdid).Subscribe(result =>
            {
                result.d.citizen_list.ForEach(citizen =>
                {
                    CCitizenUIDataManager.Instance.SetUIData(citizen);
                });

                CCitizenUIDataManager.Instance.SetCurrentSelectID(gdid);

                var data = CCitizenUIDataManager.Instance.GetCurrentCitizenUIData;
                CitizenInfoUI.SetData(data);

                SNGDataManager.Instance.SetNewCitizenIconByReward(REWARD_TYPE.RW_CITIZEN);

                //스크롤 업데이트
                ScrollUI.RefreshScroll();

                actUpdate?.Invoke();

                disposable.Dispose();
            });
        }

        public void OpenResetPopupClose(Action action)
        {
            OpenResetPopupBase(() => action?.Invoke());
        }

        public void OpenResetPopup(Action<int> nextAction, int value, Action Canceled = null)
        {
            OpenResetPopupBase(() => nextAction?.Invoke(value), Canceled);
        }

        public void OpenResetPopupBase(Action onConfirmed, Action onCanceled = null)
        {
            var title = CStringDataManager.Instance.GetStringData("button_check");
            var msg = CStringDataManager.Instance.GetStringData("ui_styleshop_out");

            var notice = CCoreServices.GetCoreService<CPopupService>()
                .OpenPopup_CommonAlert(title, msg, PopupAlert.BUTTON_TYPE.BTN_TWO);

            SingleAssignmentDisposable noticeDispose = new SingleAssignmentDisposable();

            noticeDispose.Disposable = notice.ShowAsObservable()
                .Do(result =>
                {
                    if (result.IsSucess)
                    {
                        if (onConfirmed != null)
                        {
                            onConfirmed.Invoke();
                        }
                    }
                    else
                    {
                        if (onCanceled != null)
                        {
                            onCanceled.Invoke();
                        }
                    }
                    noticeDispose.Dispose();
                })
                .Subscribe()
                .AddTo(this);
        }

        public void ChangeMember(int tabNum, int value, Action<MEMBER_TYPE> recvEvent)
        {
            MEMBER_TYPE memberType = (MEMBER_TYPE)((int)MEMBER_TYPE.JUNGWON + value);
            CMemberAvatarManager.Instance.SetUICurMemberType(memberType);
            CStylingItemInvenManager.Instance.SetStylingInfoByTab(tabNum, memberType, recvEvent);
            SetMemberInfoCurMemberType(memberType);
            MemberSortFilter.InitAllFilterButtons();
            // if (tabNum == 1)
            // {
            //     //ClosetInfoUI.InitTab();
            // }
            // else if (tabNum == 2)
            // {
            //     StyleShopInfoUI.InitTab();
            // }
        }

        public void SetMemberInfoCurMemberType(MEMBER_TYPE mType)
        {
            if (MemberInfoUI != null)
            {
                MemberInfoUI.SetMemberType(mType);
            }
        }

        public CHAR_SELECTION GetCurCharSelection()
        {
            return CurCharSelection;
        }

        public bool StartBackProcess()
        {
            return SortFilter.StartBackProcess();

        }


        public void Dispose()
        {
            if (FilterContents != null) FilterContents.Dispose();
            if (CitizenInfoUI != null) CitizenInfoUI.Dispose();
            if (SortFilter != null) SortFilter.Dispose();
            if (MemberInfoUI != null) MemberInfoUI.Release();
            if (StyleShopInfoUI != null) StyleShopInfoUI.Release();
            if (ClosetInfoUI != null) ClosetInfoUI.Release();
            CStylingItemInvenManager.Instance.ReleaseStyleListByTab();
        }
    }
}