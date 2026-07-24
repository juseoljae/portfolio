using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using Game.RestAPI;
using CitizenUI;

public class PageMemberInfo : MonoBehaviour
{

    [Header("상세 정보")]
    [SerializeField] private Button BtnInfo;
    [SerializeField] private CRedDot RedDot;

    [Header("멤버 배치")]
    [SerializeField] private Button BtnSet;
    [SerializeField] private Button BtnSetOff;

    //[Header("멤버 이름")]
    //[SerializeField] private Text TxtName;

    [SerializeField] private UIObjectTouchRotation TouchRotation;

    [Header("멤버 버튼")]
    [SerializeField] private GameObject ObjMemberItemParent;
    [SerializeField] private GameObject ObjMemberItem;
    [SerializeField] private GameObject DropDownObj;
    [SerializeField] private GameObject SlotObj;
    [SerializeField] private GameObject MemberFilterRoot1Obj;
    [SerializeField] private GameObject MemberFilterRoot2Obj;

    [Header("피로도")]
    [SerializeField] private GameObject FatabilityObj;
    [SerializeField] private Text FatabilityTxt;
    [SerializeField] private Button FatabilityBtn;
    [SerializeField] private GameObject FatabilityTooltipObj;
    [SerializeField] private Text FatabilityTooltipTitleTxt;
    [SerializeField] private Text FatabilityTooltipTxt;
    [SerializeField] private Button CloseFatabilityTooltipBtn;
    [SerializeField] private Image RotationImage;

    private PageCitizenMiddleUI MiddleUI;

    private Action OnClickDetailInfo = null;
    public Action SetOnClickDetailInfo { set { OnClickDetailInfo = value; } }

    private List<ObjMemberItem> MemberItemList = new List<ObjMemberItem>();
    private Dictionary<MEMBER_TYPE, CMemberAvatar> MemberAvatarDic = new Dictionary<MEMBER_TYPE, CMemberAvatar>();

    private MEMBER_TYPE CurrentMemberType = MEMBER_TYPE.NONE;
    private Dictionary<MEMBER_TYPE, bool> _avatarRequestedFlags = new Dictionary<MEMBER_TYPE, bool>();

    private Dictionary<int, GameObject> FatigabilityDic = null;
    private Dictionary<int, string> FatigabilityTitleStrDic = null;
    private Dictionary<int, string> FatigabilityStrDic = null;
    private List<ObjMemberItem> MemberItemObjList;
    private int MemberRecovery;
    private SingleAssignmentDisposable DetailPopupDisposable = null;
    private Action setNewEvent = null;
    public Action SetNewOnTab { set { setNewEvent = value; } }


    public void Init(PageCitizenMiddleUI midUI)
    {
        MiddleUI = midUI;
        DropDownObj.SetActive(false);
        SlotObj.SetActive(false);
        
        SetMemberType(CMemberAvatarManager.Instance.GetUICurMemberType());

        for (MEMBER_TYPE member = MEMBER_TYPE.JUNGWON; member <= MEMBER_TYPE.MAX; member++)
        {
            if (MemberAvatarDic.ContainsKey(member) == false)
            {
                AvatarList _info = CMemberAvatarServerDataManager.Instance.GetMemberAvatarInfo((int)member);
                CMemberAvatar avatar = new CMemberAvatar(member);
                avatar.SetMemberAvatarInfo(_info);
                avatar.SetCurEquipSItemDic();
                MemberAvatarDic.Add(member, avatar);
            }
        }

        SetMemberObj();

        if (TouchRotation != null)
        {
            TouchRotation.Initialized(MiddleUI.UICharacter.GetRotationObject);
            TouchRotation.SetOnClickEvent = OnClickCharacter;
        }

        if (DetailPopupDisposable != null)
        {
            DetailPopupDisposable.Dispose();
        }
        DetailPopupDisposable = new SingleAssignmentDisposable();

        DetailPopupDisposable.Disposable = BtnInfo.BindToOnClick(_ =>
        {
            OnClickMemberDetailInfo();
            return Observable.Timer(TimeSpan.FromSeconds(CDefines.BUTTONDELAYTIME)).AsUnitObservable();

        }).AddTo(this);


        SetMemberButtons();
        SetFatigabilityStatus();
    }

    private void SetFatigabilityStatus()
    {
        FatabilityBtn.BindToOnClick(_ =>
        {
            SetActiveTooltip(true);
            return Observable.Timer(TimeSpan.FromSeconds(CDefines.BUTTONDELAYTIME)).AsUnitObservable();
        }).AddTo(this);

        CloseFatabilityTooltipBtn.BindToOnClick(_ =>
        {
            SetActiveTooltip(false);
            return Observable.Timer(TimeSpan.FromSeconds(CDefines.BUTTONDELAYTIME)).AsUnitObservable();
        }).AddTo(this);

        if (FatigabilityTitleStrDic == null)
        {
            FatigabilityTitleStrDic = new Dictionary<int, string>();
        }
        else
        {
            FatigabilityTitleStrDic.Clear();
        }

        if (FatigabilityStrDic == null)
        {
            FatigabilityStrDic = new Dictionary<int, string>();
        }
        else
        {
            FatigabilityStrDic.Clear();
        }

        for (int i = 0; i < 4; ++i)
        {
            int idx = i + 1;
            FatigabilityTitleStrDic.Add(idx, TooltipConstants.TooltipStrings[i]);
            FatigabilityStrDic.Add(idx, TooltipConstants.TooltipStrings[i + 4]);
        }

        FatabilityTooltipObj.SetActive(false);

        if (FatigabilityDic == null)
        {
            FatigabilityDic = new Dictionary<int, GameObject>();
        }

        for (int i = 1; i < 5; ++i)
        {
            string path = "Status0{0}";
            GameObject obj = FatabilityObj.transform.Find(string.Format(path, i)).gameObject;
            if (FatigabilityDic.ContainsKey(i) == false)
            {
                FatigabilityDic.Add(i, obj);
            }
        }


        SetFatigabilityText();
    }

    private void HideAllFatigabilityObjs()
    {
        if (FatabilityObj != null) FatabilityObj.SetActive(false);
        foreach (GameObject obj in FatigabilityDic.Values)
        {
            obj.SetActive(false);
        }
    }

    public void SetFatigabilityText()
    {        
        HideAllFatigabilityObjs();
        FatabilityObj.SetActive(true);
        AvatarList sData = CMemberAvatarServerDataManager.Instance.GetMemberAvatarInfo((int)CurrentMemberType);
        if (sData != null)
        {
            MemberRecovery = sData.recovery;
            int grade = 1;

            if (MemberRecovery == 0)
            {
                grade = sData.fatigability_grade;
            }
            else if (MemberRecovery > 0)
            {
                grade = 4;
            }
            FatigabilityDic[grade].SetActive(true);
            FatabilityTxt.text = CStringDataManager.Instance.GetStringData(FatigabilityTitleStrDic[grade]);
            FatabilityTooltipTitleTxt.text = CStringDataManager.Instance.GetStringData(FatigabilityTitleStrDic[grade]);
            FatabilityTooltipTxt.text = CStringDataManager.Instance.GetStringData(FatigabilityStrDic[grade]);
        }
    }

    private void SetActiveTooltip(bool bActive)
    {
        RotationImage.raycastTarget = !bActive;
        FatabilityTooltipObj.SetActive(bActive);
    }

    public void SetMemberType(MEMBER_TYPE mType)
    {
        CurrentMemberType = mType;

        if (setNewEvent != null)setNewEvent.Invoke();
    }

    public MEMBER_TYPE GetCurrentMemberType()
    {
        return CurrentMemberType;
    }

    public void SetMemberObj()
    {
        if (MemberAvatarDic.ContainsKey(CurrentMemberType) == false) return;
        DestroyMemberAvatarObj();
        MiddleUI.UICharacter.SetMember(MemberAvatarDic[CurrentMemberType]);
    }


    public void OnClickMemberDetailInfo()
    {
        if (!_avatarRequestedFlags.ContainsKey(CurrentMemberType) || !_avatarRequestedFlags[CurrentMemberType])
        {
            _avatarRequestedFlags[CurrentMemberType] = true;

            APIHelper.SNGService.ReqSNG_AvatarStyling_having(((int)CurrentMemberType).ToString())
                .Subscribe(result =>
                {
                    OpenDetailPopup();
                }).AddTo(this);
        }
        else
        {
            OpenDetailPopup();
        }

    }

    private void OpenDetailPopup()
    {
        CPopupService popupService = CCoreServices.GetCoreService<CPopupService>();
        var popup = popupService.OpenPopupMemberDetail(new PopupMemberDetail.Setting()
        {
            MemberAvatar = MemberAvatarDic[CurrentMemberType],
            MemberName = SNGDataManager.Instance.GetMemberName(CurrentMemberType),

        });

        SingleAssignmentDisposable disposable = new SingleAssignmentDisposable();
        disposable.Disposable = popup.ShowAsObservable().Subscribe(res =>
        {
            disposable.Dispose();
        });
    }

    private void DestroyMemberAvatarObj()
    {
        foreach (CMemberAvatar member in MemberAvatarDic.Values)
        {
            if (member.GetAvatarObj() != null)
            {
                member.DestroyMemberAvatarObj();
            }
        }

    }

    private void SetMemberButtons()
    {
        if (MemberItemObjList == null)
        {
            MemberItemObjList = new List<ObjMemberItem>();
        }
        
        if (MemberItemObjList.Count == 0)
        {
            //int root1Count = (int)MEMBER_TYPE.JAKE;
            for (MEMBER_TYPE member = MEMBER_TYPE.JUNGWON; member <= MEMBER_TYPE.MAX; member++)
            {
                GameObject parentObj = MemberFilterRoot1Obj;
                if( member > MEMBER_TYPE.JAKE)
                {
                    parentObj = MemberFilterRoot2Obj;
                }
                GameObject obj = Utility.AddChild(parentObj, ObjMemberItem);//ObjMemberItemParent
                obj.SetActive(true); 
                ObjMemberItem item = obj.GetComponent<ObjMemberItem>();
                item.Init(this, member, HideAllMemberObjItem, SetFatigabilityText);
                if (member == GetCurrentMemberType())
                {
                    item.SetActiveSelectObj(true);
                }
                else
                {
                    item.SetActiveSelectObj(false);
                }
                MemberItemObjList.Add(item);
            }
        }
    }

    private void HideAllMemberObjItem()
    {
        foreach (ObjMemberItem item in MemberItemObjList)
        {
            if (item != null)
            {
                item.SetActiveSelectObj(false);
            }
        }
    }
     



    private void OnClickCharacter()
    {
        MiddleUI.UICharacter.PlayMemberTouchSignatureAnimation();
    }

    public void Release()
    {
        DestroyMemberAvatarObj();

        //if (MiddleUI.UICharacter != null) MiddleUI.UICharacter.ReleaseBaseCharObj();
        //DestroyMemberAvatarObj();

        if (_avatarRequestedFlags != null)
        {
            _avatarRequestedFlags.Clear();
        }

        if (MemberItemList != null)
        {
            foreach (ObjMemberItem item in MemberItemList)
            {
                if (item != null)
                {
                    Destroy(item.gameObject);
                }
            }
            MemberItemList.Clear();
        }
        
        if (MemberAvatarDic != null)
        {
            foreach (CMemberAvatar member in MemberAvatarDic.Values)
            {
                member.ClearEquipItem();
            }
            MemberAvatarDic.Clear();
        }

        if (MemberItemObjList != null)
        {
            foreach (ObjMemberItem item in MemberItemObjList)
            {
                if (item != null)
                {
                    Destroy(item.gameObject);
                }
            }
            MemberItemObjList.Clear();
            MemberItemObjList = null;
        }
    }
}
