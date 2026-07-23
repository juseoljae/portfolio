using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UI_ButtonGroup : MonoBehaviour
{
    #region ENUM

    public enum eBUTTON_STATE
    {
        CLICK,
        PRESS,
        RELEASE,
    }

    #endregion ENUM



    #region PROPERTISE

    public UI_InGameButton[] Skills { get; private set; }
    public UI_InGameButton Ex { get; private set; }

    #endregion PROPERTISE


    #region PUBLIC FUNC

 

    public void Init(CharacterBase characterBase)
    {
        Skills  = InitSkills(characterBase, false);
        Ex      = InitEx(characterBase);
    }
    

    public void SetSkill(CharacterBase cBase, bool changeSkill)

    {
        Skills = InitSkills(cBase, changeSkill);
    }
    #endregion PUBLIC FUNC


    #region PRIVATE FUNC

    private void DoBasicAttack(CharacterBase characterBase, eBUTTON_STATE eState)
    {
        if (characterBase == null)
            return;

        if (eState == eBUTTON_STATE.PRESS)
        {
            if (((AutoAI)characterBase.m_CharAi).GetAutoPlayState() == AutoAI.eAutoProcessState.eEnding)
                return;

            if (characterBase.m_charController == null || characterBase.CharacterControlFreeze() == true) return;


            if (!characterBase.CheckMontionStateChange(MOTION_STATE.eMOTION_ATTACK_COMBO, null))
            {
                return;
            }

            if (((AutoAI)characterBase.m_CharAi).GetAutoPlayState() != AutoAI.eAutoProcessState.ePlay)// || ((AutoAI)characterBase.m_CharAi).GetAutoState() == AutoAI.eAutoState.eMOVE)
            {
               characterBase.m_charController.PressingAttack(true);
            }
        }
        else if (eState == eBUTTON_STATE.RELEASE)
        {
            if (((AutoAI)characterBase.m_CharAi).CheckMontionStateChange() == false)
            {
                if (characterBase.m_charController == null || characterBase.CharacterControlFreeze() == true) return;

                characterBase.m_charController.PressingAttack(false);
            }
        }
    }
    private void DoSkill(UI_InGameButtons ingameButtons, CharacterBase characterBase,PlayerStanceInfo stanceInfo, eSKILL_TYPE eSkillType, int skillIndex ,UI_InGameButton button)
    {
        if (characterBase.m_charController == null || characterBase.CharacterControlFreeze() == true || InGameManager.instance.CanAccessState() == false)
            return;


        if (characterBase.m_charController.ButtonController(eSkillType, skillIndex) == true)
        {
            float fSkillCoolTime = characterBase.m_AttackInfos[stanceInfo.m_nActiveSkillID[skillIndex]].skillinfo.coolTime;
            float fAnimationTime = characterBase.m_AttackInfos[stanceInfo.m_nActiveSkillID[skillIndex]].skillinfo.aniTime;

            ingameButtons.StartCoolTimes(fAnimationTime, fSkillCoolTime, button);
        }
	}

    private void DoEx(UI_InGameButtons ingameButtons, CharacterBase characterBase, eBUTTON_STATE eState)
    {
        if (characterBase.m_charController == null || characterBase.CharacterControlFreeze() == true || InGameManager.instance.CanAccessState() == false)
            return;

        switch (eState)
        {
            case eBUTTON_STATE.PRESS:
                {
                    if (characterBase.m_charController == null || characterBase.CharacterControlFreeze() == true) return;

                    switch (characterBase.m_CharacterType)
                    {
                        case eCharacter.eWarrior:
                            if ( characterBase.m_charController.ButtonController(eSKILL_TYPE.eSKILL_SPECIAL, -1) )
                            {
                                float fSkillCoolTime = characterBase.m_AttackInfos[PlayerManager.instance.GetCurSpecialSkill(characterBase)].skillinfo.coolTime;
                                float fAnimationTime = 0;

                                ingameButtons.StartCoolTimes(fAnimationTime, fSkillCoolTime, Ex);
                            }
                            break;
                        case eCharacter.eWizard:
                            if (characterBase.m_charController.ButtonController(eSKILL_TYPE.eSKILL_SPECIAL, -1))
                            {
                                float fSkillCoolTime =characterBase.m_AttackInfos[PlayerManager.instance.GetCurSpecialSkill(characterBase)].skillinfo.coolTime;
                                float fAnimationTime = 0;

                                ingameButtons.StartCoolTimes(fAnimationTime, fSkillCoolTime, Ex);
                            }
                            break;
                        case eCharacter.eArcher:
                            break;
                    }
                }
                break;
            case eBUTTON_STATE.RELEASE:
                {
                    if (characterBase.m_GuardState == SHIELDGUARD_STATE.eGUARDING)
                    {
                        characterBase.ShieldGuard_Unlock();
                    }
                    else
                    {
                        characterBase.m_bGuardPress = false;
                    }
                }
                break;
        }
    }

    private UI_InGameButton InitAttack(CharacterBase characterBase)
    {
        return null;
        int nStartAttack        = PlayerManager.instance.GetStartAttack(characterBase, 0);
        CAttackInfo attackInfo  = SkillDataManager.instance.Load(nStartAttack);

        UI_InGameButton attack  = this.transform.Find("Bt_Attack").gameObject.AddComponent<UI_InGameButton>();

        attack.Init(attackInfo.skillinfo.strAtlasName, attackInfo.skillinfo.strSpriteName,
            delegate { DoBasicAttack(characterBase, eBUTTON_STATE.PRESS); },
            delegate { DoBasicAttack(characterBase, eBUTTON_STATE.RELEASE); }
            );

        return attack;
    }
    private UI_InGameButton[] InitSkills(CharacterBase characterBase, bool changeSkill)
    {
        UI_InGameButtons ingameButtons  = (UI_InGameButtons)SceneManager.instance.GetUIComponent(SceneManager.eCOMPONENT.InGameButtons);
        UI_InGameButton[] skills        = new UI_InGameButton[4];


        int nStance                     = (int)UserManager.instance.m_CurPlayingCharInfo.playerInfo.m_eStanceType;
        PlayerStanceInfo stanceInfo     = PlayerManager.instance.GetPlayerStanceInfo(characterBase, nStance - 1);

        for (int i = 0; i < stanceInfo.m_nActiveSkillID.Length; i++)
        {
            blame_messages.SkillInfo skillInfo = SkillDataManager.instance.MySkills[stanceInfo.m_nActiveSkillID[i]];

            if (skillInfo.stance == nStance && 1 <= skillInfo.slot_no && skillInfo.slot_no <= 4)
            {
                /// 클라는 0부터 시작함
                int nIndex_Client = (int)skillInfo.slot_no - 1;

                string strAtlas = characterBase.m_AttackInfos[(int)skillInfo.id].skillinfo.strAtlasName;
                string strSprite = characterBase.m_AttackInfos[(int)skillInfo.id].skillinfo.strSpriteName;
                eSKILL_TYPE eSkillType = (eSKILL_TYPE)nIndex_Client;
                int slotSkill_idx = i;
                float CoolTime = 0;
                float MaxCoolTime = 0;
                if (changeSkill)
                {
                    UI_InGameButton changeskill = this.transform.Find("Bt_Skill" + nIndex_Client).gameObject.GetComponent<UI_InGameButton>();
                    CoolTime = changeskill.m_fCurrentCoolTime;
                    MaxCoolTime = characterBase.m_AttackInfos[stanceInfo.m_nActiveSkillID[i]].skillinfo.coolTime;
                    changeskill = null;

                }

                skill.Init(strAtlas,
                    strSprite,
                    delegate
                    {
                        DoSkill(ingameButtons, characterBase, stanceInfo, eSkillType, slotSkill_idx, skill);

                    MissionManager.instance.CheckMission(eMISSION_KIND.eUSED_SKILL);
                },
                    stanceInfo.m_nOpenLevels[i]
                    );
                if (changeSkill)
                {
                    if(CoolTime > 0)
                    {
                        skill.SkillChangeCoolTimes(MaxCoolTime, CoolTime, (int)skillInfo.slot_no - 1);

                    }
                }
                skills[nIndex_Client] = skill;
            }
        }
       
        return skills;
    }

    private UI_InGameButton[] SwitchSkill(CharacterBase characterBase, blame_messages.SkillInfo newSkill, blame_messages.SkillInfo oldSkill)
    {
        UI_InGameButtons ingameButtons = (UI_InGameButtons)SceneManager.instance.GetUIComponent(SceneManager.eCOMPONENT.InGameButtons);

#if UNITY_EDITOR
        for (int i = 0; i < ingameButtons.m_ButtonGroup.Skills.Length; ++i)
        {
            Debug.Log("##### current Cool Time = " + ingameButtons.m_ButtonGroup.Skills[i].m_fCurrentCoolTime);
        }
#endif

        int nStance = (int)UserManager.instance.m_CurPlayingCharInfo.playerInfo.m_eStanceType;
        PlayerStanceInfo stanceInfo = PlayerManager.instance.GetPlayerStanceInfo(characterBase, nStance - 1);

        for (int i = 0; i < stanceInfo.m_nActiveSkillID.Length; i++)
        {
            blame_messages.SkillInfo skillInfo = SkillDataManager.instance.MySkills[stanceInfo.m_nActiveSkillID[i]];

            if(newSkill.id == skillInfo.id)
            {
                int nIndex_Client = (int)skillInfo.slot_no - 1;
                string strAtlas = characterBase.m_AttackInfos[(int)skillInfo.id].skillinfo.strAtlasName;
                string strSprite = characterBase.m_AttackInfos[(int)skillInfo.id].skillinfo.strSpriteName;
                eSKILL_TYPE eSkillType = (eSKILL_TYPE)nIndex_Client;
                int slotSkill_idx = i;
                ingameButtons.m_ButtonGroup.Skills[nIndex_Client].Init(strAtlas, strSprite, null, stanceInfo.m_nOpenLevels[i]);
                ingameButtons.m_ButtonGroup.Skills[nIndex_Client].Init(strAtlas,
                    strSprite,
                    delegate
                    {
                        DoSkill(ingameButtons, characterBase, stanceInfo, eSkillType, slotSkill_idx, ingameButtons.m_ButtonGroup.Skills[nIndex_Client]);
                        MissionManager.instance.CheckMission(eMISSION_KIND.eUSED_SKILL);
                    },
                    stanceInfo.m_nOpenLevels[i]);
            }
        }

            return ingameButtons.m_ButtonGroup.Skills;
    }

    private UI_InGameButton InitEx(CharacterBase characterBase)
    {
        int nEx = PlayerManager.instance.GetSpecialSkill(characterBase, 0, 0);
        CAttackInfo attackInfo = SkillDataManager.instance.Load(nEx);

        UI_InGameButtons ingameButtons  = (UI_InGameButtons)SceneManager.instance.GetUIComponent(SceneManager.eCOMPONENT.InGameButtons);

        /// 스페셜
        UI_InGameButton ex  = this.transform.Find("Bt_Special").gameObject.AddComponent<UI_InGameButton>();

        if (PlayerManager.instance.GetCurSpecialSkill(characterBase) != 0)
        {
            string strAtlas     = attackInfo.skillinfo.strAtlasName;
            string strSprite    = attackInfo.skillinfo.strSpriteName;

            ex.Init(
                strAtlas,
                strSprite,
                delegate { DoEx(ingameButtons, characterBase, eBUTTON_STATE.PRESS); },
                delegate { DoEx(ingameButtons, characterBase, eBUTTON_STATE.RELEASE); }
                );
        }

        return ex;
    }

    #endregion PRIVATE FUNC
}
