using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SuperScrollView;
using System.Linq;
using Game.RestAPI;
using UniRx;
using DG.Tweening;

public class PopupMarbleMemberSelect : CPopupUI<PopupMarbleMemberSelect.Setting, PopupMarbleMemberSelect.Result>
{
    private PageMarble MarblePage;
    //[SerializeField] private Button BtnCheck;
    [SerializeField] private LoopListView2 Scroll = null;

    private List<MarbleMemberInfo> MemberInfoList;
    private int ScrollCount;

    protected override void OnAwake()
    {
        base.OnAwake();

        subject = new Subject<Result>();
        result = new Result { IsSucess = false };

        Flags = PopupFlags.DISABLE_HIDE;
    }

    public override void SetData(Setting setting)
    {
        MarblePage = setting.Page;        
        
        MemberInfoList = new List<MarbleMemberInfo>();

        MEMBER_TYPE curMemberType = CMarbleServerDataManager.Instance.GetMarbleMemberSelectInfo();
        if (CDirector.Instance.GetCurrentSceneID() == CSceneId.MINIGAME_MARBLE_SCENE)
        {
            var gameInfo = CMarbleServerDataManager.Instance.GetGameInfo();
            if (gameInfo != null)
            {
                curMemberType = gameInfo.member_id.ToEnum<MEMBER_TYPE>();
                
            }
        }

        for (int i = 0; i < (int)MEMBER_TYPE.CNT; i++)
        {
            MarbleMemberInfo info = new MarbleMemberInfo();
            info.MemberType = (MEMBER_TYPE)((int)MEMBER_TYPE.JUNGWON + i);
            AvatarList memberavatarinfo = CMemberAvatarServerDataManager.Instance.GetMemberAvatarInfo((int)info.MemberType);
            if (memberavatarinfo != null)
            {
                if (memberavatarinfo.improve_list!= null && memberavatarinfo.improve_list.Count > 0)
                {
                    ImproveList improveinfo = memberavatarinfo.improve_list.FirstOrDefault();
                    if (improveinfo != null)
                    {
                        info.Level = improveinfo.lv;
                        info.ExpPoint = improveinfo.current_value;
                    }
                }                    
                
                info.IsPlaying = false;
                if (curMemberType != MEMBER_TYPE.NONE)
                {
                    if (info.MemberType == curMemberType)
                    {
                        info.IsPlaying = true;
                    }
                }
                info.IsSelected = false;

                MemberInfoList.Add(info);
            }
        }


        ScrollCount = MemberInfoList.Count;

        Scroll?.InitListView(ScrollCount, OnGetItemByIndex);
        
        if (curMemberType == MEMBER_TYPE.NONE)
        {
            MEMBER_TYPE lobbymemberType = LobbyDirector.Instance.GetLobbyMemberData().member;
            SetSelectedMember(lobbymemberType);
        }
        else
        {
            SetSelectedMember(curMemberType);
        }
    }

    private MEMBER_TYPE GetSelectedMemberType()
    {
        for (int i = 0; i < MemberInfoList.Count; i++)
        {
            if (MemberInfoList[i].IsSelected)
            {
                return MemberInfoList[i].MemberType;
            }
        }

        return MEMBER_TYPE.NONE;
    }

    private int GetMemberInfoListByMemberType(MEMBER_TYPE memberType)
    {
        for (int i = 0; i < MemberInfoList.Count; i++)
        {
            if (MemberInfoList[i].MemberType == memberType)
            {
                return i;
            }
        }

        return -1;
    }

    private void HideAllSelectObj()
    {
        foreach (var item in MemberInfoList)
        {
            item.IsSelected = false;
        }

        for (int i = 0; i < Scroll.ShownItemCount; i++)
        {
            var shownItem = Scroll.GetShownItemByIndex(i);
            if (shownItem != null)
            {
                var obj = shownItem.GetComponent<ObjMarbleMember>();
                if (obj != null)
                {
                    obj.SetActiveSelectObj(false);
                }
            }
        }
    }

    public void SetSelectedMember(MEMBER_TYPE memberType, bool smooth = false)
    {
        HideAllSelectObj();

        int selectedIndex = GetMemberInfoListByMemberType(memberType);
        if (selectedIndex != -1)
        {
            if (selectedIndex < MemberInfoList.Count)
            {
                MemberInfoList[selectedIndex].IsSelected = true;
            }

            float offset = (ScrollCount > 1) ? (float)selectedIndex / (ScrollCount - 1) : 0.5f;

            if (smooth)
            {
                SmoothScrollToMember(selectedIndex, offset);
            }
            else
            {
                Scroll.MovePanelToItemIndex(selectedIndex, offset);
                UpdateSelectedItemVisual(selectedIndex);
            }
        }
    }

    private void SmoothScrollToMember(int selectedIndex, float offset)
    {
        Vector3 startPos = Scroll.ContainerTrans.anchoredPosition3D;

        Scroll.MovePanelToItemIndex(selectedIndex, offset);
        Vector3 targetPos = Scroll.ContainerTrans.anchoredPosition3D;
        Scroll.ContainerTrans.anchoredPosition3D = startPos;

        DOTween.To(() => Scroll.ContainerTrans.anchoredPosition3D,
                   v => Scroll.ContainerTrans.anchoredPosition3D = v,
                   targetPos, 0.3f)
               .SetEase(Ease.InOutSine)
               .OnComplete(() =>
               {
                   Scroll.MovePanelToItemIndex(selectedIndex, offset);
                   UpdateSelectedItemVisual(selectedIndex);
               });
    }

    private void UpdateSelectedItemVisual(int selectedIndex)
    {
        var shownItem = Scroll.GetShownItemByItemIndex(selectedIndex);
        if (shownItem != null)
        {
            var obj = shownItem.GetComponent<ObjMarbleMember>();
            if (obj != null)
            {
                obj.SetActiveSelectObj(true);
            }
        }
    }
    

    private LoopListViewItem2 OnGetItemByIndex(LoopListView2 listView, int index)
    {
        if (ScrollCount <= index) 
            return null;

        if (0 > index) 
            return null;

        var strPrefabName = listView.ItemPrefabDataList.First().mItemPrefab.name;
        var scrollItem = listView.NewListViewItem(strPrefabName);

        var info = MemberInfoList[index];

        var item = scrollItem.GetComponent<ObjMarbleMember>();        
        item.SetData(this, info);

        return scrollItem;
    }

    protected override void OnStart()
    {
        base.OnStart();
    }
    
    public void OnClickCheck()
    {
        MEMBER_TYPE selectedMemberType = GetSelectedMemberType();
        if (selectedMemberType == MEMBER_TYPE.NONE)
        {
            CDebug.LogError("PopupMarbleMemberSelect.OnClickCheck - Invalid member type: NONE");
            return;
        }
        
        if (MarblePage != null && selectedMemberType == MarblePage.GetCurrentMemberType())
        {
            OnClickClose();
        }
        else
        {
            SingleAssignmentDisposable _disposer = new SingleAssignmentDisposable();
            _disposer.Disposable = APIHelper.MarbleService.MGMarbleMemberSelect(selectedMemberType)
                                .Subscribe(resData =>
                                {    
                                    OnClickCloseWithoutActiveUI();                            
                                    BarManager.Instance.MoveToMiniGameMarble();
                                    _disposer.Dispose();
                                });
        }
    }


    public void OnClickClose()
    {
        base.Close();

        if (MarblePage != null)
        {
            MarblePage.SetActiveBottomObj(true);
            MarblePage.SetActiveAutoObj(true);
        }
    }

    public void OnClickCloseWithoutActiveUI()
    {
        base.Close();
    }

    protected override void OnBackEventProcess()
    {
        base.OnBackEventProcess();
        OnClickClose();
    }


    public class Setting
    {
        public PopupFlags Flags { get; set; }
        public PageMarble Page { get; set; }
    }

    public class Result
    {
        public bool IsSucess { get; set; }
    }
}

public class MarbleMemberInfo
{
    public MEMBER_TYPE MemberType;
    public int Level;
    public int ExpPoint;
    public bool IsPlaying;
    public bool IsSelected;
}