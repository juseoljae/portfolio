using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UI_InGameButtons : BaseUIComponent
{
    #region ENUM

    public enum eDIRECTION
    {
        NONE,
        LEFT,
        RIGHT,
    }

    #endregion ENUM



    #region VARIABLE

    private CharacterBase m_CharBase                = null;
    public UI_ButtonGroup m_ButtonGroup { get; private set; }

    public UI_AutoSkillButton m_uiAutoSkillButton = null;

    #endregion VARIABLE


    #region UNITY FUNC

    private void Update()
    {
#if UNITY_EDITOR
        Cheat();
#endif
    }


    

    #endregion UNITY FUNC



    #region PUBLIC FUNC

    public void Init(GameObject hero)
    {
        m_CharBase          = hero.GetComponent<CharacterBase>();

        m_ButtonGroup      = InitButtonGroup();

        Transform tfObj = transform.FindChild("UI_ButtonGroup/Bt_AutoSkill");
        m_uiAutoSkillButton = tfObj.GetComponent<UI_AutoSkillButton>();
        if (m_uiAutoSkillButton == null)
            m_uiAutoSkillButton = tfObj.gameObject.AddComponent<UI_AutoSkillButton>();
    }

    public void StartCoolTimes(float fAnimationTime, float fCoolTime, UI_InGameButton pressedButton)
    {
        for (int i = 0; i < m_ButtonGroup.Skills.Length; i++)
        {
            m_ButtonGroup.Skills[i].StartCoolTime(fAnimationTime, fCoolTime, pressedButton, true);
        }

        m_ButtonGroup.Ex.StartCoolTime(fAnimationTime, fCoolTime, pressedButton, false);
    }
    

    public void SetAutoSkillActive(bool bActive)
    {
        m_uiAutoSkillButton.SetShow(bActive);
    }



    #endregion PUBLIC FUNC



    #region PRIVATE FUNC


    private void CheckBlock()
    {
        if (PlayerManager.instance.GetMyPlayerInfo().playerCharBase == null || PlayerManager.instance.GetMyPlayerInfo().playerCharBase.m_BuffController == null)
            return;
        BuffController buffController = PlayerManager.instance.GetMyPlayerInfo().playerCharBase.m_BuffController;
        bool bActive = 0 < buffController.GetCharDisable(BuffController.eBuffCharDisable.eSKILL);
        m_ButtonGroup.Ex.SetBlock(bActive);
    }

    private UI_ButtonGroup InitButtonGroup()
    {
        UI_ButtonGroup buttonsGroup     = this.transform.Find("UI_ButtonGroup").gameObject.AddComponent<UI_ButtonGroup>();

        buttonsGroup.Init(m_CharBase);

        return buttonsGroup;
    }


    #endregion PRIVATE FUNC








    private void Cheat()
    {
        if (Input.GetKeyDown(KeyCode.A) == true)
        {
            m_ButtonGroup.Skills[0].OnClick();
        }
        if (Input.GetKeyDown(KeyCode.S) == true)
        {
            m_ButtonGroup.Skills[1].OnClick();
        }
        if (Input.GetKeyDown(KeyCode.D) == true)
        {
            m_ButtonGroup.Skills[2].OnClick();
        }
        if (Input.GetKeyDown(KeyCode.F) == true)
        {
            m_ButtonGroup.Skills[3].OnClick();
        }
        if (Input.GetKeyDown(KeyCode.G) == true)
        {
            m_ButtonGroup.Ex.OnPress(true);
        }
        if (Input.GetKeyUp(KeyCode.G) == true)
        {
            m_ButtonGroup.Ex.OnPress(false);
        }

    }

}