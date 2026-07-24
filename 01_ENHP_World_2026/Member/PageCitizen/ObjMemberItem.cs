using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

public class ObjMemberItem : MonoBehaviour
{
    [SerializeField] private Text MemberName;
    [SerializeField] private Image MemberIcon;
    [SerializeField] private GameObject SelectObj;        
    
    private PageMemberInfo MemberInfo = null;
    private SingleAssignmentDisposable BtnDisposable;
    private bool isNew;

    private MEMBER_TYPE MemberType = MEMBER_TYPE.NONE;
    private Action HideEvent;
    private Action SelectCharEvent;

    public void Init(PageMemberInfo memberInfo, MEMBER_TYPE mType, Action hideEvent, Action selCharEvent)
    {
        MemberType = mType;

        MemberInfo = memberInfo;

        HideEvent = hideEvent;

        SelectCharEvent = selCharEvent;

        string memberName = SNGDataManager.Instance.GetMemberName(mType);
        SetMemberName(memberName);

        MemberCharacterData charData = CMemberAvatarDataManager.Instance.GetMemberCharacterData(MemberType);
        if (charData != null)
        {
            CResourceManager.Instance.LoadImage(charData.IconResPath, MemberIcon);
            MemberIcon.gameObject.SetActive(true);
        }

        SetActiveSelectObj(mType == MEMBER_TYPE.JUNGWON);

        SetIsNew();

        Button Btn = gameObject.GetComponent<Button>();

        if (Btn != null)
        {
            if (BtnDisposable == null)
            {
                BtnDisposable = new SingleAssignmentDisposable();
            }
            else
            {
                BtnDisposable.Dispose();
                BtnDisposable = new SingleAssignmentDisposable();
            }

            BtnDisposable.Disposable = Btn.BindToOnClick(_ =>
            {
                OnClickSelectMember();

                return Observable.Timer(TimeSpan.FromSeconds(CDefines.BUTTONDELAYTIME)).AsUnitObservable();
            }).AddTo(this);
        }
    }

    private void SetIsNew()
    {
        GameObject newRootObj = transform.Find("obj_new").gameObject;
        if (newRootObj != null)
        {
            var newAlert = newRootObj.GetComponent<CNewAlert>();
            if (newAlert != null)
            {
                newAlert.alertType = NEWALERT_TYPE.CHRACTER_HOUSE_DROPDOWN;
                int memberid = (int)MemberType;
                newAlert.SetNewAlert(memberid);
            }
        }
    }

    private void SetMemberName(string name)
    {
        MemberName.text = name;
    }

    public void SetActiveSelectObj(bool isActive)
    {
        SelectObj.SetActive(isActive);
    }

    private void OnClickSelectMember()
    {
        if (MemberInfo.GetCurrentMemberType() == MemberType)
        {
            return;
        }

        if (HideEvent != null)
        {
            HideEvent.Invoke();
        }

        MemberInfo.SetMemberType(MemberType);
        MemberInfo.SetMemberObj();
        CMemberAvatarManager.Instance.SetUICurMemberType(MemberType);
        SetActiveSelectObj(true);
        
        if (SelectCharEvent != null)
        {
            SelectCharEvent.Invoke();
        }
        
    }
}
