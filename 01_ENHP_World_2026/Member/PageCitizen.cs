using UnityEngine;
using CitizenUI;
using System;
using UniRx;
using System.Collections.Generic;

public class PageCitizen : PagePopup
{
    [SerializeField] PageCitizenTopUI TopUI;
    [SerializeField] PageCitizenMiddleUI MiddleUI;

    private BaseObject actuator;

    protected override void OnAwake()
    {
        base.OnAwake();
        Flags = PopupFlags.DISABLE_HIDE;
    }

    protected override void OnStart()
    {
        base.OnStart();

        if (MiddleUI != null)
        {
            MiddleUI.SetActiveMemberRawImgObj(false);
        }

        CCitizenUIDataManager.Instance.Dispose();

        if (TopUI != null)
        {
            TopUI.InitUI();
            TopUI.SetOnClickEvent(OnClickClose);
            TopUI.SetOnClickHelpEvent(OnClickHelpButton);
        }
        if (MiddleUI != null)
        {
            MiddleUI.Init();
        }

        Utility.SetFxLayerDimmed(false);

        // APIHelper.CitizenService.ReqCitizens().Subscribe(result =>
        // {
        //     CCitizenUIDataManager.Instance.Init();

        //     if (TopUI != null)
        //     {
        //         TopUI.InitUI();
        //         TopUI.SetOnClickEvent(OnClickClose);
        //     }

        //     if (MiddleUI != null)
        //     {
        //         MiddleUI.InitUI();
        //     }

        //     Utility.SetFxLayerDimmed(false);


        // }).AddTo(this);        



    }

    public void Init(BaseObject obj)
    {
        actuator = obj;
    }

    private void Dispose()
    {
        TopUI.Dispose();
        MiddleUI.Dispose();

        CNavigationManager.Instance.isEnableNavigation = false;
    }

    protected override bool OnPreBackEventProcess()
    {
        if (MiddleUI != null)
        {
            bool backProcess = MiddleUI.StartBackProcess();

            if (backProcess)
            {
                return true;
            }
        }

        OnPreClosePopup();

        return base.OnPreBackEventProcess();
    }


    public void OnClickClose()
    {
        bool canBack = MiddleUI.CanMoveOut(OnClosePopup);
        if (canBack)
        {
            List<TownNPCSimpleMove> memberAllAvatarSimpleMoves = SNGDataManager.Instance.GetMemberAllAvatarSimpleMove();

            if (memberAllAvatarSimpleMoves != null)
            {
                foreach (var npc in memberAllAvatarSimpleMoves)
                {
                    SkinnedMeshRenderer[] smr = npc.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                    foreach (SkinnedMeshRenderer rdr in smr)
                    {
                        QuickOutline outLine = rdr.GetComponent<QuickOutline>();
                        if (!outLine)
                        {
                            outLine = rdr.gameObject.AddComponent<QuickOutline>();
                            npc.AddOutLine(outLine);
                        }

                    }
                }
            }
            assetService.RemoveStaticResourcePool(CPREFAB_KEY.page_citizen);
            OnClosePopup();
        }
    }

    public void OnClickHelpButton()
    {
        if (MiddleUI == null)
        {
            HelpManager.Instance.ShowHelpWebview(HELP_TAG.AVATAR.ToString());
            return;
        }
        
        switch (MiddleUI.GetCurCharSelection())
        {
            case PageCitizenMiddleUI.CHAR_SELECTION.MEMBER:
                HelpManager.Instance.ShowHelpWebview(HELP_TAG.AVATAR.ToString());
                break;
            case PageCitizenMiddleUI.CHAR_SELECTION.CITIZEN:
                HelpManager.Instance.ShowHelpWebview(HELP_TAG.CITIZEN.ToString());
                break;
        }
    }

    public void OnPreClosePopup()
    {
        SNGDataManager.Instance.SetNewCitizenIconByReward(REWARD_TYPE.RW_CITIZEN);

        CCitizenUIDataManager.Instance.ReqCitizenAllocate();

        if (CDirector.Instance.GetCurrentSceneID() == CSceneId.SNG_SCENE)
        {
            actuator.RestoreCamera();
        }

        Dispose();
        CCitizenUIDataManager.Instance.Dispose();
        LoadingLayer.Instance.DisableMenu();
    }


    public PageCitizenMiddleUI GetMiddleUI()
    {
        return MiddleUI;
    }

    public void SetNavigation(int tabIdx)
    {
        if (MiddleUI != null)
        {
            MiddleUI.SetNavigation(tabIdx);
        }
    }

    protected override void OnDestroy()
    {
        OnPreClosePopup();
    }
}
