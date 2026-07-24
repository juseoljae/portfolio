using UniRx;
using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Unity.VisualScripting;

namespace CitizenUI
{
    public class PageCitizenInfo : MonoBehaviour
    {
        [Header("상세 정보")]
        [SerializeField] private Button BtnInfo;
        [SerializeField] private CRedDot RedDot;
        
        [Header("프렌즈 배치")]
        [SerializeField] private Button BtnSet;
        [SerializeField] private Button BtnSetOff;

        [Header("프렌즈 이름")]
        [SerializeField] private Text TxtName;

        [SerializeField] private UIObjectTouchRotation TouchRotation;

        [SerializeField] private GameObject ObjCitizen;
        [SerializeField] private GameObject FatabilityObj;
        [SerializeField] private Text FatabilityTxt;
        [SerializeField] private Button FatabilityBtn;
        [SerializeField] private GameObject FatabilityTooltipObj;
        [SerializeField] private Text FatabilityTooltipTitleTxt;
        [SerializeField] private Text FatabilityTooltipTxt;
        [SerializeField] private Button CloseFatabilityTooltipBtn;

        private PageCitizenMiddleUI MiddleUI = null;
        private int CurrentCitizenID;

        private Action<bool> OnClickSet = null;
        private Action OnClickDetailInfo = null;
        public Action<bool> SetOnClickSet { set { OnClickSet = value; } }
        public Action SetOnClickDetailInfo { set { OnClickDetailInfo = value; } }

        private Dictionary<int, GameObject> FatigabilityDic = null;
        private Dictionary<int, string> FatigabilityTitleStrDic = null;
        private Dictionary<int, string> FatigabilityStrDic = null;

        private Action NewConfirmAction = null;

        private SingleAssignmentDisposable InfoBtnDisposable = null;
        private SingleAssignmentDisposable SetBtnDisposable = null;
        private SingleAssignmentDisposable SetOffBtnDisposable = null;
        private SingleAssignmentDisposable FataBtnDisposable = null;
        private SingleAssignmentDisposable ToolTipBtnDisposable = null;

        public void InitUI(PageCitizenMiddleUI middleUI)
        {
            MiddleUI = middleUI;

            if (InfoBtnDisposable != null)
            {
                InfoBtnDisposable.Dispose();
            }
            InfoBtnDisposable = new SingleAssignmentDisposable();
            InfoBtnDisposable.Disposable = BtnInfo.BindToOnClick(_ =>
            {
                OnClickDetailInfo?.Invoke();
                return Observable.Timer(TimeSpan.FromSeconds(CDefines.BUTTONDELAYTIME)).AsUnitObservable();

            }).AddTo(this);

            
            if (SetBtnDisposable != null)
            {
                SetBtnDisposable.Dispose();
            }
            SetBtnDisposable = new SingleAssignmentDisposable();
            SetBtnDisposable.Disposable = BtnSet.BindToOnClick(_ =>
            {
                OnClickSet?.Invoke(true);

                return Observable.Timer(TimeSpan.FromSeconds(CDefines.BUTTONDELAYTIME)).AsUnitObservable();
            }).AddTo(this);

            
            if (SetOffBtnDisposable != null)
            {
                SetOffBtnDisposable.Dispose();
            }
            SetOffBtnDisposable = new SingleAssignmentDisposable();
            SetOffBtnDisposable.Disposable = BtnSetOff.BindToOnClick(_ =>
            {
                OnClickSet?.Invoke(false);
                return Observable.Timer(TimeSpan.FromSeconds(CDefines.BUTTONDELAYTIME)).AsUnitObservable();
            }).AddTo(this);

            if (TouchRotation != null)
            {
                TouchRotation.Initialized(MiddleUI.UICharacter.GetRotationObject);
                TouchRotation.SetOnClickEvent = OnClickCharacter;
            }

            SetFatigabilityStatus();
        }

        private void SetFatigabilityStatus()
        {
            
            if (FataBtnDisposable != null)
            {
                FataBtnDisposable.Dispose();
            }
            FataBtnDisposable = new SingleAssignmentDisposable();
            FataBtnDisposable.Disposable = FatabilityBtn.BindToOnClick(_ =>
            {
                SetActiveTooltip(true);
                return Observable.Timer(TimeSpan.FromSeconds(CDefines.BUTTONDELAYTIME)).AsUnitObservable();
            }).AddTo(this);

            
            if (ToolTipBtnDisposable != null)
            {
                ToolTipBtnDisposable.Dispose();
            }
            ToolTipBtnDisposable = new SingleAssignmentDisposable();
            ToolTipBtnDisposable.Disposable = CloseFatabilityTooltipBtn.BindToOnClick(_ =>
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

            HideAllFatigabilityObjs();
        }

        private void HideAllFatigabilityObjs()
        {
            if (FatabilityObj != null) FatabilityObj.SetActive(false);
            foreach (GameObject obj in FatigabilityDic.Values)
            {
                obj.SetActive(false);
            }
        }

        private void SetActiveTooltip(bool bActive)
        {
            FatabilityTooltipObj.SetActive(bActive);
        }

        public void SetData(CitizenUIData data, Action newConfirmAction = null)
        {
            NewConfirmAction = newConfirmAction;

            MiddleUI.UICharacter.ReleaseCharacter();
            MiddleUI.UICharacter.SetCharacter(data);
            if (data.GDID != CurrentCitizenID) TouchRotation.SetRotation(0);

            SetName(data.TData.NameStrID);
            UpdateData(data);
        }

        public void UpdateData(CitizenUIData data)
        {
            CurrentCitizenID = data.GDID;

            HideAllFatigabilityObjs();

            if (data.SData != null)
            {
                FatabilityObj.SetActive(true);
                int grade = 1;

                if (data.SData.recovery == 0)
                {
                    grade = data.SData.fatigability_grade;
                }
                else if (data.SData.recovery > 0)
                {
                    grade = 4;
                }

                FatigabilityDic[grade].SetActive(true);
                FatabilityTxt.text = CStringDataManager.Instance.GetStringData(FatigabilityTitleStrDic[grade]);
                FatabilityTooltipTitleTxt.text = CStringDataManager.Instance.GetStringData(FatigabilityTitleStrDic[grade]);
                FatabilityTooltipTxt.text = CStringDataManager.Instance.GetStringData(FatigabilityStrDic[grade]);
            }

            //획득 했는지
            if (data.IsGetCitizen)
            {
                bool isOn = CCitizenUIDataManager.Instance.IsAllocateCitizen(data.GDID);
                SetOnUI(!isOn);
                SetOffUI(isOn);

            }
            else
            {
                SetOnUI(false);
                SetOffUI(false);
            }

            SetReddot(data.GetRewardedState);
        }

        private void SetReddot(bool enable)
        {
            if( RedDot != null)
            {
                RedDot.SetRedDotActive(enable);
            }
       }

        private void OnClickCharacter()
        {
            MiddleUI.UICharacter.PlayCharacterTouchAnimation();
        }

        private void SetOnUI(bool isOn)
        {
            if (BtnSet != null)
            {
                BtnSet.gameObject.SetActive(isOn);
            }
        }
        private void SetOffUI(bool isOff)
        {
            if (BtnSetOff != null)
            {
                BtnSetOff.gameObject.SetActive(isOff);
            }

        }



        private void SetName(string strName)
        {
            var str = CStringDataManager.Instance.GetStringData(strName);

            if (TxtName != null)
            {
                TxtName.text = str;
            }
        }


        public void ReleaseUICharObj()
        {
            if (MiddleUI != null && MiddleUI.UICharacter != null)
            {
                //UICharacter.ReleaseBaseCharObj();
                MiddleUI.UICharacter.ReleaseCharacter();
            }

            if (NewConfirmAction != null)
            {
                NewConfirmAction.Invoke();
            }
        }

        public void Dispose()
        {
            if (MiddleUI != null && MiddleUI.UICharacter != null)
            {
                MiddleUI.UICharacter.Dispose();
            }

            if (FatigabilityDic != null)
            {
                foreach(GameObject obj in FatigabilityDic.Values)
                {
                    obj.Destroy();
                }
                FatigabilityDic.Clear();
                FatigabilityDic = null;
            }
        }
    }
}