#define CAMFILTER

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum AISTATE : byte
{
    eNONE = 0,
    eIDLE,
    eIDLE_WALK,
    eATTACK_IDLE,
    eRun,
    eTRACE,
    eATTACK,
    eBE_ATTACKED,
    eDIE,






    STAND,
    SUSPEND,
    AWAKE,
    WAITFORSUMMON,
    AVOID,
    AVOIDATTACK,
    FEAR,
    FOLLOWMASTER,
    FOLLOWTARGET,
    FINDFOLLOWER,
    ATTACKPOSSEEK,
    ATTACKREADY,
    DASHFORATTACK,
    ATTACK,
    RANDOMPVPREADY,
    RANDOMPVPREAI,

}

public class ObjectStateMachine
{
    public ObjectState      m_CurrentState;
    public MOTION_STATE     m_CurrentEnumState;
    public MOTION_STATE     m_PreEnumState;

    public GameObject       m_charObj;
    public CharacterBase    m_charBaseScript;
    
    public bool m_bPause = false;

    public void SetMachine(GameObject My_character, CharacterBase scr)
    {
        m_charObj = My_character;
        m_charBaseScript = scr;
        m_CurrentState = null;// ObjectState_Idle.Instance();
    }

    public void Clear()
    {
        ChangeState(ObjectState_Idle.Instance());
    }

    public void ObjStateMachice_Update()
    {

        if (InGameManager.instance.m_pauseGame)
            return;

        if (m_CurrentState != null)
        {
            m_CurrentState.Execute(m_charObj, m_charBaseScript);

        }
    }

    public void ChangeState(ObjectState newState)
    {
        if (m_CurrentState != null)
        {
            m_CurrentState.Exit(m_charObj, m_charBaseScript);
        }

        m_charBaseScript.m_bMotionCancel = false;

        m_CurrentState = newState;

        m_CurrentState.Enter(m_charObj, m_charBaseScript);

        m_PreEnumState = m_CurrentEnumState;

        m_CurrentEnumState = m_CurrentState.GetEnumType();
    }
    public void DestroyStateMachine()
    {
        if (m_CurrentState != null) m_CurrentState = null;
        if (m_charObj != null) m_charObj = null;
        if (m_charBaseScript != null) m_charBaseScript = null;
    }

    public MOTION_STATE GetState()
    {
        return m_CurrentEnumState;
    }

    public MOTION_STATE GetPreState()
    {
        return m_PreEnumState;
    }

    public void Pause()
    {
        m_bPause = true;
    }

    public void Resume()
    {
        m_bPause = false;
    }

    public bool IsPause()
    {
        return m_bPause;
    }

}

public class ObjectState
{
    public virtual void Enter(GameObject charObj, CharacterBase charScr) { }

    public virtual void Execute(GameObject charObj, CharacterBase charScr) { }

    public virtual void Exit(GameObject charObj, CharacterBase charScr) { }

    public virtual MOTION_STATE GetEnumType() { return MOTION_STATE.eMOTION_NONE; }
}


public class ObjectState_Idle : ObjectState
{
    static ObjectState_Idle m_Instance = null;

    public static ObjectState_Idle Instance()
    {
        if (m_Instance == null)
        {
            m_Instance = new ObjectState_Idle();
        }

        return m_Instance;
    }

    public override MOTION_STATE GetEnumType()
    {
        return MOTION_STATE.eMOTION_IDLE;
    }

    public override void Enter(GameObject charObj, CharacterBase charScr)
    {
        if (charScr.m_CharAi == null) return;
        if (charScr.m_CharAi.m_eType == eAIType.ePC)
        {
            int iStanceIndex = (int)((AutoAI)charScr.m_CharAi).GetStance();

            PlayerStanceInfo StanceInfo = PlayerManager.instance.GetPlayerStanceInfo(charScr, iStanceIndex);
                
            if ( StanceInfo != null )
                charScr.ksAnimation.PlayAnim(SkillDataManager.instance.GetAnimationClipName(StanceInfo.m_nStanceIdle), 0.15f);
            else
                charScr.ksAnimation.PlayAnim("Idle", 0.15f);
        }
        else
        {
            charScr.ksAnimation.PlayAnim("Idle", 0.15f);
        }
    }

    public override void Execute(GameObject charObj, CharacterBase charScr)
    {
    }
}

public class ObjectState_IdleStand : ObjectState
{
    static ObjectState_IdleStand m_Instance = null;

    public static ObjectState_IdleStand Instance()
    {
        if (m_Instance == null)
        {
            m_Instance = new ObjectState_IdleStand();
        }

        return m_Instance;
    }

    public override MOTION_STATE GetEnumType()
    {
        return MOTION_STATE.eMOTION_IDLE;
    }

    public override void Enter(GameObject charObj, CharacterBase charScr)
    {
        //charScr.ksAnimation.Play(charScr.ksAnimation.GetAnimationData("Idle_Stand"), 0.1f, true);
        //charScr.ksAnimation.m_Animation.CrossFade("Idle_Stand");

        charScr.ksAnimation.PlayAnim("Idle_Stand", 0);
    }

    public override void Execute(GameObject charObj, CharacterBase charScr)
    {
    }
}

public class ObjectState_IdleWalk : ObjectState
{
    static ObjectState_IdleWalk m_Instance = null;

    #region     PRIVATE_MEMBERS
    //private float m_routineStartTime;
    //private float m_routineWalkStartTime;
    //private int m_routineWaitTime;

    //private Vector3 m_walkDest;

    //private bool m_doRoutineWalk;

    #endregion  PRIVATE_MEMBERS

    public static ObjectState_IdleWalk Instance()
    {
        if (m_Instance == null)
        {
            m_Instance = new ObjectState_IdleWalk();
        }

        return m_Instance;
    }

    public override MOTION_STATE GetEnumType()
    {
        return MOTION_STATE.eMOTION_IDLE_WALK;
    }

    public override void Enter(GameObject charObj, CharacterBase charScr)
    {
        charScr.ksAnimation.PlayAnim("Run", 0.2f);
    }

    public override void Execute(GameObject charObj, CharacterBase charScr)
    {
    }

    private void SetWalk(GameObject charObj, CharacterBase charScr)
    {
        charScr.ksAnimation.PlayAnim("Idle", 0.2f);
    }
}
public class ObjectState_SideWalk : ObjectState
{
    static ObjectState_SideWalk m_Instance = null;

    public static ObjectState_SideWalk Instance()
    {
        if (m_Instance == null)
        {
            m_Instance = new ObjectState_SideWalk();
        }

        return m_Instance;
    }

    public override MOTION_STATE GetEnumType()
    {
        return MOTION_STATE.eMOTION_SIDE_WALK;
    }

    public override void Enter(GameObject charObj, CharacterBase charScr)
    {
        float fSpeed = 1.0f;
        if ( charScr.m_PanicBMove.m_Dir.x == 1.0f )
            fSpeed = -1.0f;

        //charScr.ksAnimation.PlayAnim("Walk", 0.2f, fSpeed);
        charScr.ksAnimation.PlayAnim("Run", 0.2f, fSpeed);
    }

    public override void Execute(GameObject charObj, CharacterBase charScr)
    {
        if (charScr.attackTarget != null)
            charScr.SetAttackTarget(charScr.attackTarget);
    }
}

public class ObjectState_Run : ObjectState
{
    static ObjectState_Run m_Instance = null;

    public static ObjectState_Run Instance()
    {
        if (m_Instance == null)
        {
            m_Instance = new ObjectState_Run();
        }

        return m_Instance;
    }

    public override MOTION_STATE GetEnumType()
    {
        return MOTION_STATE.eMOTION_RUN;
    }

    public override void Enter(GameObject charObj, CharacterBase charScr)
    {
        float fMoveSpeed			= 1.0f;
		float fMoveSpeedRatio		= 0f;
		float fPlayerComboSpeed		= 0f;
		if (MapManager.instance.m_bVillage == false)
        {
            if( charScr.m_CharAi.m_eType == eAIType.ePC )
            {
				//fMoveSpeed = (charScr.damageCalculationData.fMOVE_SPEED + charScr.damageCalculationData.fCOMBOMOVE_SPEED) / PlayerManager.instance.m_PlayerDataInfo[(int)PlayerManager.instance.m_PlayerInfo[charScr.charUniqID].jobType].move_Speed;
				
				fMoveSpeedRatio			= charScr.damageCalculationData.fCOMBOMOVE_SPEED + charScr.damageCalculationData.fTOTAL_MOVESPEED_RATIO + (float)charScr.damageCalculationData.dSUMMONATTRIBUTE_RUN_SPEED_RATIO;
				fMoveSpeed				= (charScr.damageCalculationData.fTOTAL_MOVESPEED_ADD * (1 + (fMoveSpeedRatio * 0.01f))) / PlayerManager.instance.m_PlayerDataInfo[(int)PlayerManager.instance.m_PlayerInfo[charScr.charUniqID].jobType].move_Speed;
			}
            else
            {
                if(charScr.m_CharAi.m_eType == eAIType.eHERONPC)
                {
					//fMoveSpeed = ((charScr.damageCalculationData.fMOVE_SPEED + PlayerManager.instance.playerInfoList[PlayerManager.MYPLAYER_INDEX].playerCharBase.damageCalculationData.fCOMBOMOVE_SPEED) / charScr.m_CharAi.GetNpcProp().Run_Speed);
					fPlayerComboSpeed	= PlayerManager.instance.playerInfoList[PlayerManager.MYPLAYER_INDEX].playerCharBase.damageCalculationData.fCOMBOMOVE_SPEED;
					fMoveSpeedRatio		= fPlayerComboSpeed + charScr.damageCalculationData.fTOTAL_MOVESPEED_RATIO + (float)charScr.damageCalculationData.dSUMMONATTRIBUTE_RUN_SPEED_RATIO;
					fMoveSpeed			= (charScr.damageCalculationData.fTOTAL_MOVESPEED_ADD * (1 + (fMoveSpeedRatio * 0.01f))) / charScr.m_CharAi.GetNpcProp().Run_Speed;

				}
                else
                {
                    //fMoveSpeed = (charScr.damageCalculationData.fMOVE_SPEED + charScr.damageCalculationData.fCOMBOMOVE_SPEED) / charScr.m_CharAi.GetNpcProp().Run_Speed;

					fMoveSpeedRatio		= charScr.damageCalculationData.fTOTAL_MOVESPEED_RATIO + (float)charScr.damageCalculationData.dSUMMONATTRIBUTE_RUN_SPEED_RATIO;
					fMoveSpeed			= (charScr.damageCalculationData.fTOTAL_MOVESPEED_ADD * (1 + (fMoveSpeedRatio * 0.01f))) / charScr.m_CharAi.GetNpcProp().Run_Speed;
				}
			}
		}
        charScr.ksAnimation.PlayAnim("Run", 0, fMoveSpeed);
        
        charScr.m_mAnimCtr.bSpeedChange = false;
    }

    public override void Execute(GameObject charObj, CharacterBase charScr)
    {
        if( charScr.m_mAnimCtr.bSpeedChange == true )
        {
            Enter(charObj, charScr);
        }
    }
}

public class ObjectState_AttackCombo : ObjectState
{
    static ObjectState_AttackCombo m_Instance = null;
	

    public static ObjectState_AttackCombo Instance()
    {
        if (m_Instance == null)
        {
            m_Instance = new ObjectState_AttackCombo();
        }

        return m_Instance;
    }

    public override MOTION_STATE GetEnumType()
    {
        return MOTION_STATE.eMOTION_ATTACK_COMBO;
    }

    public override void Enter(GameObject charObj, CharacterBase charScr)
    {
		float fAttackSpeed						= 0;
		float fAttackSpeedRatio					= 0;
		float fPlayerComboAttackSpeed			= 0;
		charScr.nSkillEffectSequenceIndex		= 1;
        charScr.nSkillProjectileSequenceIndex	= 1;

        if (charScr.attackTarget != null && charScr.attackTarget != charScr)
            charScr.LookAtY(charScr.attackTarget.m_MyObj.position);

        charScr.m_UseSkillIndex = charScr.m_currSkillIndex = charScr.m_charController.m_AtkComboQueue[0];

		if(charScr.m_CharacterType == eCharacter.eWarrior || charScr.m_CharacterType == eCharacter.eWizard)
		{
			
			//fAttackSpeedRatio				= charScr.damageCalculationData.fCOMBOATTACK_SPEED + charScr.damageCalculationData.fATTACK_SPEED_UP_RATIO + (float)charScr.damageCalculationData.dSUMMONATTRIBUTE_ATT_SPEED_RATIO;
			fAttackSpeedRatio				= charScr.damageCalculationData.fCOMBOATTACK_SPEED + charScr.damageCalculationData.fTOTAL_ATTACKSPEED_RATIO + (float)charScr.damageCalculationData.dSUMMONATTRIBUTE_ATT_SPEED_RATIO;
			fAttackSpeed = charScr.damageCalculationData.fTOTAL_ATTACKSPEED_ADD * (1 + (fAttackSpeedRatio * 0.01f));
		}
		else if(charScr.m_CharacterType == eCharacter.eHERONPC)
		{
			fPlayerComboAttackSpeed			= PlayerManager.instance.playerInfoList[PlayerManager.MYPLAYER_INDEX].playerCharBase.damageCalculationData.fCOMBOATTACK_SPEED;
			//fAttackSpeedRatio				= fPlayerComboAttackSpeed + charScr.damageCalculationData.fATTACK_SPEED_UP_RATIO + (float)charScr.damageCalculationData.dSUMMONATTRIBUTE_ATT_SPEED_RATIO;
			fAttackSpeedRatio				= fPlayerComboAttackSpeed + charScr.damageCalculationData.fTOTAL_ATTACKSPEED_RATIO + (float)charScr.damageCalculationData.dSUMMONATTRIBUTE_ATT_SPEED_RATIO;
			fAttackSpeed					= charScr.damageCalculationData.fTOTAL_ATTACKSPEED_ADD * (1 + (fAttackSpeedRatio * 0.01f));
		}
		else
		{
			//fAttackSpeedRatio				= charScr.damageCalculationData.fATTACK_SPEED_UP_RATIO;
			fAttackSpeedRatio				= charScr.damageCalculationData.fTOTAL_ATTACKSPEED_RATIO;
			fAttackSpeed					= charScr.damageCalculationData.fTOTAL_ATTACKSPEED_ADD * (1 + (fAttackSpeedRatio * 0.01f));
		}
		charScr.ksAnimation.PlayNoBlend(charScr.m_AttackInfos[charScr.m_charController.m_AtkComboQueue[0]].skillinfo.aniResInfo[0].animation_name, fAttackSpeed);

		// net atk
		if (InGameManager.instance.m_bOffLine == false && PlayerManager.MYPLAYER_INDEX == charScr.charUniqID)
        {
            SkillDataInfo.SkillInfo kSkillInfo = charScr.m_AttackInfos[charScr.m_UseSkillIndex].skillinfo;

            List<CharacterBase> kTargets = new List<CharacterBase>();

            if( kSkillInfo.projectTileInfo.Count == 0)
            {
                charScr.m_skSender.DoSkill(charScr, charScr.GetAttackTarget(kSkillInfo.target_Type), kSkillInfo, 0, true);

                bool bClientAttackTarget = false;

                List<CharacterBase> kTargetInRange = charScr.m_skSender.GetTargetsInRange();
                if (kTargetInRange != null)
                {
                    for (int i = 0; i < kTargetInRange.Count; ++i)
                    {
                        if (charScr.attackTarget == kTargetInRange[i])
                            bClientAttackTarget = true;
                        kTargets.Add(kTargetInRange[i]);
                    }
                }
                if (charScr.attackTarget != null && bClientAttackTarget == false)
                    kTargets.Add(charScr.attackTarget);
            }

            NetworkManager.instance.networkSender.SendPcAttackReq(  charScr.m_IngameObjectID,
                                                                    charScr.m_UseSkillIndex,
                                                                    true, // ?쒖옉 ?좊땲硫붿씠???⑦궥
                                                                    charScr.m_MyObj.transform.position,
                                                                    charScr.m_MyObj.transform.forward,
                                                                    kTargets, // 鍮??寃?
                                                                    0,
                                                                    (int)charScr.m_charController.m_AtkComboTree);
        }
    }
    public override void Execute(GameObject charObj, CharacterBase charScr)
    {
        switch(charScr.m_charController.m_AtkComboTree)
        {
            case eATTACKCOMBO_TREE.DASH:
            case eATTACKCOMBO_TREE.MAX_DASH:
                if(charScr.m_pushInfo.m_bStart)
                {
                    float fRemainDash = charScr.m_pushInfo.m_fDuration - Time.deltaTime;
                    //float fDurationFreeze = Time.deltaTime;

                    if (fRemainDash < 0.0f)
                    {
                        fRemainDash = charScr.m_pushInfo.m_fDuration;
                        charScr.m_pushInfo.m_bStart = false;
                    }
                    else
                        fRemainDash = Time.deltaTime;

                    /// ?ㅼ쓬 ?꾨젅?꾩뿉 ?대룞???꾩튂
                    Vector3 movePosition = charScr.transform.forward * charScr.m_pushInfo.m_fSpeed * fRemainDash;

                    /// ?곸쓣 ?대룞 ?쒗궡
                    //SkillDataInfo.SkillInfo skillInfo = charScr.m_AttackInfos[charScr.m_UseSkillIndex].skillinfo;

                    /// 罹먮┃???대룞
                    charScr.Move(movePosition);

                    charScr.m_pushInfo.m_fDuration -= Time.deltaTime;
                }
                break;
        }
    }
}



public class ObjectState_BeAttacked : ObjectState
{
    static ObjectState_BeAttacked m_Instance = null;

    eAffectCode m_AffectCode;

    public static ObjectState_BeAttacked Instance()
    {
        if (m_Instance == null)
        {
            m_Instance = new ObjectState_BeAttacked();
        }

        return m_Instance;
    }

    public override MOTION_STATE GetEnumType()
    {
        return MOTION_STATE.eMOTION_IDLE;
    }

    public override void Enter(GameObject charObj, CharacterBase charScr)
    {

        if (charScr == null)
        {
            UnityEngine.Debug.Log("eKNOCKBACK     CharacterBase null~~~~");
            return;
        }
        if (charObj == null)
        {
            UnityEngine.Debug.Log("eKNOCKBACK     CharacterObj null~~~~");
            return;
        }

        charScr.m_ReactionAffectCode = charScr.m_ReactionBuffData.m_AffectCode;

        //Debug.Log(charObj+"/ Beattacked Enter affect code = " + SkillDataManager.instance.GetReactionType(charScr.m_ReactionAffectCode));

        // ?⑥뼱 ?덈떎硫??쒕윭?댁옄
        charScr.SetCharacterHide(eSTATE_HIDE.eSTATE_HIDENOT);

        switch (SkillDataManager.instance.GetReactionType(charScr.m_ReactionAffectCode))
        {
            case eREACTION_TYPE.ePHYGICAL_DAMAGE:
                charScr.m_DmgBMove.m_PrevPos = charScr.gameObject.transform.position;

                if (charScr.isCritical)
                {
                    switch (charScr.m_CharacterType)
                    {
                        case eCharacter.eNPC:
                            charScr.ksAnimation.PlayAnim("Damaged01", 0.05f, 3.0f);
                            break;
                        default:
                            //charScr.ksAnimation.PlayAnim("Damaged02", 0.05f);
                            charScr.ksAnimation.PlayAnim("Damaged01", 0.05f);
                            break;
                    }
                }
                else
                {
                    switch (charScr.m_CharacterType)
                    {
                        case eCharacter.eNPC:
                            charScr.ksAnimation.PlayAnim("Damaged01", 0.05f, 4.0f);
                            break;
                        default:
                            charScr.ksAnimation.PlayAnim("Damaged01", 0.05f);
                            break;
                    }
                }
                break;
            case eREACTION_TYPE.eSTUNNED:
                //charScr.ksAnimation.Play(charScr.ksAnimation.GetAnimationData("Stun"), 0.15f, true);
                charScr.ksAnimation.PlayAnim("Stun", 0.15f);
                break;
            case eREACTION_TYPE.ePANIC:
                //charScr.ksAnimation.Play(charScr.ksAnimation.GetAnimationData("Run"), 0.15f, true);
                charScr.ksAnimation.PlayAnim("Run", 0.15f);
                charScr.m_PanicBMove.m_fTime = charScr.m_ReactionBuffData.m_fLifeTime;
                charScr.m_ReactionBuffData.m_fSpecialTime = 0.0f;
                break;
            case eREACTION_TYPE.eKNOCKBACK:
                charScr.m_DmgBMove.m_PrevPos = charScr.m_ReactionBuffData.m_SourceChar.gameObject.transform.position;
                //charScr.m_ReactionBuffData.m_fSpecialTime = charScr.ksAnimation.GetAnimationData("KnockBack").clip.length;
                charScr.m_ReactionBuffData.m_fSpecialTime = charScr.ksAnimation.GetAnimationClip("KnockBack").length;

                if (InGameManager.instance.m_bOffLine == true || charScr.m_HitSkillNetData.m_NetBuffData.Count == 0)
                    charScr.LookAtY(charScr.m_ReactionBuffData.m_SourceChar.m_MyObj.transform.position);

                if (charScr.m_ReactionBuffData.m_AffectValue != 0)
                {
                    switch (charScr.m_CharacterType)
                    {
                        case eCharacter.eNPC:
                            charScr.m_DmgBMove.m_Speed = ((float)charScr.m_ReactionBuffData.m_AffectValue) / (charScr.m_ReactionBuffData.m_fSpecialTime);
                            break;
                        default:
                            charScr.m_DmgBMove.m_Speed = ((float)charScr.m_ReactionBuffData.m_AffectValue) / charScr.m_ReactionBuffData.m_fSpecialTime;
                            break;
                    }
                }
                //Debug.Log("@@@@@ KNOCKBACK who am I = "+charObj);
                //charScr.ksAnimation.Play(charScr.ksAnimation.GetAnimationData("KnockBack"), 0.05f, true);
                charScr.ksAnimation.PlayAnim("KnockBack", 0.05f);
                charScr.SetCollisionDetection(false);
                //charScr.ksAnimation.SetDontPlayAnymore(false);
                break;
            case eREACTION_TYPE.eKNOCKBACK_INT_2:
                charScr.m_DmgBMove.m_PrevPos = charScr.m_ReactionBuffData.m_SourceChar.gameObject.transform.position;
                //charScr.m_ReactionBuffData.m_fSpecialTime = charScr.ksAnimation.GetAnimationData("KnockBack2nd").clip.length;
                //charScr.m_ReactionBuffData.m_fSpecialTime = charScr.ksAnimation.GetAnimationClip("KnockBack2nd").length;
                charScr.m_ReactionBuffData.m_fSpecialTime = charScr.ksAnimation.GetAnimationClip("KnockBack").length;

                if (InGameManager.instance.m_bOffLine == true || charScr.m_HitSkillNetData.m_NetBuffData.Count == 0)
                    charScr.LookAtY(charScr.m_ReactionBuffData.m_SourceChar.m_MyObj.transform.position);

                if (charScr.m_ReactionBuffData.m_AffectValue != 0)
                {
                    switch (charScr.m_CharacterType)
                    {
                        case eCharacter.eNPC:
                            charScr.m_DmgBMove.m_Speed = ((float)charScr.m_ReactionBuffData.m_AffectValue) / (charScr.m_ReactionBuffData.m_fSpecialTime);
                            break;
                        default:
                            charScr.m_DmgBMove.m_Speed = ((float)charScr.m_ReactionBuffData.m_AffectValue) / charScr.m_ReactionBuffData.m_fSpecialTime;
                            break;
                    }
                }
                charScr.SetCollisionDetection(false);
                //charScr.ksAnimation.SetDontPlayAnymore(false);
                break;
            case eREACTION_TYPE.eKNOCKDOWN:
                charScr.ksAnimation.PlayAnim("KnockDown", 0.05f);
                if (charScr.m_CharState == CHAR_STATE.DEATH)
                    charScr.m_ReactionBuffData.m_fSpecialTime = charScr.ksAnimation.GetAnimationClip("KnockDown").length;
                charScr.SetCollisionDetection(false);
                break;
            case eREACTION_TYPE.eAIRBONE:
                charScr.ksAnimation.PlayAnim("KnockDown", 0.05f);
                break;
            case eREACTION_TYPE.ePULLING:
                {
                    charScr.LookAtY(charScr.m_ReactionBuffData.m_SourceChar.transform.position);
                    charScr.m_PullingMove.m_Dir = charObj.transform.forward * (-1);
                    float kfMaxSpeed = 15.0f;
                    float kfMaxDist = 10.0f;

                    if (charScr.m_CharacterType == eCharacter.eNPC && charScr.m_ReactionBuffData != null && charScr.m_ReactionBuffData.m_SourceChar != null && charScr.m_ReactionBuffData.m_SourceChar.m_CharacterType == eCharacter.eWarrior)
                    {
                        charScr.m_PullingMove.m_fFreezeTime = CharacterBase.PullingMove.m_fChainFreezingTime;

                        kfMaxDist = charScr.m_ReactionBuffData.m_SourceChar.GetAttackInfo(charScr.m_ReactionBuffData.m_SourceChar.m_currSkillIndex).skillinfo.skill_Dist;
                    }
                    else
                        charScr.m_PullingMove.m_fFreezeTime = 0.1f;
                    if (charScr.m_CharacterType != eCharacter.eNPC)
                    {
                        charScr.ksAnimation.PlayAnim("Damaged01", 0.02f);
                    }
                    else
                    {
                        charScr.ksAnimation.PlayAnim("Damaged01", 0.02f);
                    }

                    Vector3 vGoal = charScr.m_ReactionBuffData.m_SourceChar.m_MyObj.transform.position + ((charScr.m_PullingMove.m_Dir) * (charScr.m_ReactionBuffData.m_SourceChar.GetRadius() + charScr.GetRadius()));
                    float kDistance = Vector3.Distance(charObj.transform.position, vGoal);
                    float kAniPullTime = 0.49f;
                    charScr.m_PullingMove.m_fPullingTime = kAniPullTime - charScr.m_PullingMove.m_fFreezeTime;
                    charScr.m_PullingMove.m_Speed = kDistance / charScr.m_PullingMove.m_fPullingTime;
                    kfMaxSpeed = (kfMaxDist / charScr.m_PullingMove.m_fPullingTime) * 0.3f;

                    if (kfMaxSpeed > charScr.m_PullingMove.m_Speed)
                    {
                        charScr.m_PullingMove.m_fFreezeTime += (kDistance / charScr.m_PullingMove.m_Speed) - (kDistance / kfMaxSpeed);
                        charScr.m_PullingMove.m_Speed = kfMaxSpeed;

                        if (charScr.m_PullingMove.m_fFreezeTime > kAniPullTime)
                        {
                            charScr.m_PullingMove.m_fFreezeTime = kAniPullTime - 0.1f;
                        }
                        charScr.m_PullingMove.m_fPullingTime = kAniPullTime - charScr.m_PullingMove.m_fFreezeTime;
                    }
                    charScr.m_PullingMove.m_PrevPos = charObj.transform.position;

                    charScr.SetCollisionDetection(false);

                }
                break;
            case eREACTION_TYPE.ePUSHING:
                charScr.ksAnimation.PlayAnim("Damaged01", 0.02f);
                break;
            case eREACTION_TYPE.eSCREW:
                charScr.ksAnimation.PlayAnim("Damaged01", 1.0f);
                break;
            case eREACTION_TYPE.eSTANDUP:
                //charScr.ksAnimation.Play(charScr.ksAnimation.GetAnimationData("StandUp"), 0.15f, true);
                charScr.ksAnimation.PlayAnim("StandUp", 0.15f);
                break;
            case eREACTION_TYPE.eGUARDBREAK:
                //do ani
                charScr.ksAnimation.PlayAnim("W_SPECIAL_GUARD_END", 0.15f);
                break;
            case eREACTION_TYPE.eFROZEN:
                //do ani
                //charScr.ksAnimation.PlayAnim("Damaged02", 0.15f);
                charScr.ksAnimation.PlayAnim("Damaged01", 0.15f);
                break;
        }

    }
    public override void Execute(GameObject charObj, CharacterBase charScr)
    {
        if (charScr.m_ReactionAffectCode != charScr.m_ReactionBuffData.m_AffectCode)
            return;

        //AnimationData kAniData = null;
        switch (SkillDataManager.instance.GetReactionType(charScr.m_ReactionAffectCode))
        {
            case eREACTION_TYPE.ePHYGICAL_DAMAGE:

                //charScr.ksAnimation.GetCurrentAnimationData();

                //if (charScr.m_DmgBMove.m_bFirstHit && charScr.ksAnimation.IsPlaying(charScr.ksAnimation.GetCurrentClipName() ))
                if (charScr.m_DmgBMove.m_bFirstHit && charScr.ksAnimation.IsPlaying(charScr.ksAnimation.GetCurrentClipName()))
                {
                    charScr.ksAnimation.Pause(charScr.m_DmgBMove.m_fFreezeTime);
                    charScr.m_DmgBMove.m_bFirstHit = false;
                }

                if (charScr.m_DmgBMove.m_bAttackPush)
                {
                    float dist = 0f;
                    Vector3 movePos = new Vector3(charScr.m_DmgBMove.m_Dir.normalized.x, 0, charScr.m_DmgBMove.m_Dir.normalized.z) * Time.deltaTime * dist;
                    charScr.Move(-movePos, eAffectCode.eNONE);
                }
                break;
            case eREACTION_TYPE.eSTUNNED:
                if (charScr.m_ReactionBuffData.m_BuffState != BuffData.eBuffState.eUPDATE)
                {
                    //charScr.SendMessage("OnAniEventEnd", charScr.ksAnimation.GetAnimationData(charScr.ksAnimation.sPlayClipName));
                    charScr.SendMessage("OnAniEventEnd", charScr.ksAnimation.GetAnimationClip(charScr.ksAnimation.GetCurrentClipName()));
                }
                break;
            case eREACTION_TYPE.ePANIC:
                charScr.m_ReactionBuffData.m_fSpecialTime -= Time.deltaTime;

                float fMoveSpeedRatio = 0;
                float fMvoeSpeed = 0;


                if (InGameManager.instance.m_bOffLine == true || (InGameManager.instance.m_bOffLine == false && (
                                                                (InGameManager.instance.m_bHostPlay == true && (charScr.m_CharAi.m_eType == eAIType.eNPC || charScr.charUniqID == PlayerManager.MYPLAYER_INDEX || charScr.m_CharAi.m_eType == eAIType.eHERONPC)) ||
                                                                (InGameManager.instance.m_bHostPlay == false && charScr.charUniqID == PlayerManager.MYPLAYER_INDEX)) ||
                                                                (charScr.m_CharAi.m_eType == eAIType.ePC && charScr.m_CharAi.m_OnlineUser == false)))
                {
                    Vector3 movePos = Vector3.zero;

                    if (charScr.m_ReactionBuffData.m_fSpecialTime <= 0.0f)
                    {
                        int iSearchCount = 4;

                        int iDegree = 360;
                        int iOffset = 0;
                        Vector3 moveCorrectionPosition = Vector3.zero;

                        do
                        {
                            if (iOffset == 0)
                                charScr.m_PanicBMove.m_Dir = new Vector3(0.0f, 0.0f, 1.0f);
                            charScr.m_PanicBMove.m_Dir = Quaternion.Euler(0, Random.Range(0, iDegree) + iOffset, 0) * charScr.m_PanicBMove.m_Dir;
                            charScr.LookAtY(charScr.m_PanicBMove.m_Dir + charScr.m_MyObj.position);

                            float kLifeTime = charScr.m_PanicBMove.m_fTime;

                            charScr.m_ReactionBuffData.m_fSpecialTime = Mathf.Max(0.7f, Random.Range(kLifeTime * 0.1f, kLifeTime));

                            //movePos = new Vector3(charScr.m_PanicBMove.m_Dir.x, 0, charScr.m_PanicBMove.m_Dir.z) * Time.deltaTime * (charScr.damageCalculationData.fMOVE_SPEED + charScr.damageCalculationData.fCOMBOMOVE_SPEED);
                            fMoveSpeedRatio = charScr.damageCalculationData.fCOMBOMOVE_SPEED + charScr.damageCalculationData.fTOTAL_MOVESPEED_RATIO + (float)charScr.damageCalculationData.dSUMMONATTRIBUTE_RUN_SPEED_RATIO;
                            fMvoeSpeed = charScr.damageCalculationData.fTOTAL_MOVESPEED_ADD * (1 + (fMoveSpeedRatio * 0.01f));

                            movePos = new Vector3(charScr.m_PanicBMove.m_Dir.x, 0, charScr.m_PanicBMove.m_Dir.z) * Time.deltaTime * (fMvoeSpeed);

                            --iSearchCount;
                            if (iSearchCount == 0)
                                break;

                            iOffset = 90;
                            iDegree = 180;
                        }
                        while (!charScr.CheckCanMove(movePos, eAffectCode.eNONE, out moveCorrectionPosition));

                        if (InGameManager.instance.m_bOffLine == false)
                        {
                            float fMoveDist = charScr.m_ReactionBuffData.m_fSpecialTime * (charScr.damageCalculationData.fMOVE_SPEED + charScr.damageCalculationData.fCOMBOMOVE_SPEED) + charScr.GetRadius();
                            NetworkManager.instance.networkSender.SendPcMoveReq(charScr.m_IngameObjectID,
                                                                                  charScr.m_MyObj.transform.position,
                                                                                  charScr.m_MyObj.transform.position + charScr.m_PanicBMove.m_Dir * fMoveDist,
                                                                                  (charScr.damageCalculationData.fMOVE_SPEED + charScr.damageCalculationData.fCOMBOMOVE_SPEED),
                                                                                  (int)(eMoveType.eRUN));
                        }
                    }

                    //movePos = new Vector3(charScr.m_PanicBMove.m_Dir.x, 0, charScr.m_PanicBMove.m_Dir.z) * Time.deltaTime * charScr.damageCalculationData.fMOVE_SPEED;
                    fMoveSpeedRatio = charScr.damageCalculationData.fCOMBOMOVE_SPEED + charScr.damageCalculationData.fTOTAL_MOVESPEED_RATIO + (float)charScr.damageCalculationData.dSUMMONATTRIBUTE_RUN_SPEED_RATIO;
                    fMvoeSpeed = charScr.damageCalculationData.fTOTAL_MOVESPEED_ADD * (1 + (fMoveSpeedRatio * 0.01f));

                    movePos = new Vector3(charScr.m_PanicBMove.m_Dir.x, 0, charScr.m_PanicBMove.m_Dir.z) * Time.deltaTime * fMvoeSpeed;

                    if (charScr.charUniqID != PlayerManager.MYPLAYER_INDEX)
                        charScr.LookAtY(charScr.m_MyObj.transform.position + movePos);

                    if (charScr.Move(movePos, eAffectCode.eNONE) == false) 
                        charScr.m_ReactionBuffData.m_fSpecialTime *= 0.8f;
                }
                if (charScr.m_ReactionBuffData.m_BuffState != BuffData.eBuffState.eUPDATE)
                {
                    //charScr.SendMessage("OnAniEventEnd", charScr.ksAnimation.GetAnimationData(charScr.ksAnimation.sPlayClipName));
                    charScr.SendMessage("OnAniEventEnd", charScr.ksAnimation.GetAnimationClip(charScr.ksAnimation.GetCurrentClipName()));
                }
                break;
            case eREACTION_TYPE.eKNOCKBACK:
            case eREACTION_TYPE.eKNOCKBACK_INT_2:
                if (charScr.m_ReactionBuffData.m_AffectValue != 0)
                {
                    //Debug.Log("@@@@@@ StateMachine knockback");
                    float kDelta = charScr.m_ReactionBuffData.m_fSpecialTime - Time.deltaTime;

                    if (kDelta < 0.0f)
                    {
                        kDelta = charScr.m_ReactionBuffData.m_fSpecialTime;
                    }
                    else
                        kDelta = Time.deltaTime;

                    charScr.m_ReactionBuffData.m_fSpecialTime -= kDelta;

                    if (charScr.m_ReactionBuffData.m_fSpecialTime >= 0.0f)
                    {
                        Vector3 movePos = new Vector3(charScr.m_DmgBMove.m_Dir.normalized.x, 0, charScr.m_DmgBMove.m_Dir.normalized.z) * kDelta * charScr.m_DmgBMove.m_Speed;
                        charScr.Move(-movePos, eAffectCode.eNONE, eSkillActionType.eTYPE_PENETRATION);
                    }
                }
                break;
            case eREACTION_TYPE.eKNOCKDOWN:
                charScr.m_ReactionBuffData.m_fSpecialTime -= Time.deltaTime;
                if (charScr.m_ReactionBuffData.m_fSpecialTime <= 0.0f)
                {
                    //charScr.SendMessage("OnAniEventEnd", charScr.ksAnimation.GetAnimationData("KnockDown"));
                    charScr.SendMessage("OnAniEventEnd", charScr.ksAnimation.GetAnimationClip("KnockDown"));
                }
                break;
            case eREACTION_TYPE.eAIRBONE:
                break;
            case eREACTION_TYPE.ePULLING:
                {
                    Vector3 moveDir = -charScr.m_PullingMove.m_Dir;
                    Vector3 movePos = Vector3.zero;

                    float fBackTIme = Time.deltaTime;

                    if (charScr.m_PullingMove.m_fFreezeTime - fBackTIme <= 0.0f)
                        fBackTIme = charScr.m_PullingMove.m_fFreezeTime;

                    if (charScr.m_PullingMove.m_fFreezeTime > 0.0f)
                    {
                        charScr.m_PullingMove.m_fFreezeTime -= fBackTIme;
                        break;
                    }

                    movePos = moveDir * Time.deltaTime * (charScr.m_PullingMove.m_Speed);
                    charScr.Move(movePos, eAffectCode.ePULLING_N, eSkillActionType.eTYPE_PENETRATION);

                    charScr.m_PullingMove.m_fPullingTime -= Time.deltaTime;
                    if (charScr.m_PullingMove.m_fPullingTime < 0.0f)
                    {
                        Vector3 vGoal = charScr.m_ReactionBuffData.m_SourceChar.m_MyObj.transform.position + ((-moveDir) * (charScr.m_ReactionBuffData.m_SourceChar.GetRadius() + charScr.GetRadius()));
                        charScr.m_MyObj.transform.position = vGoal;
                        if (charScr.m_ReactionBuffData != null && charScr.m_ReactionBuffData.m_SourceChar != null && charScr.m_ReactionBuffData.m_SourceChar.m_CharacterType == eCharacter.eWarrior)
                        {
                            charScr.SendMessage("OnAniEventEnd", charScr.ksAnimation.GetAnimationClip("Damaged02"));
                            //charScr.SendMessage("OnAniEventEnd", charScr.ksAnimation.GetAnimationData("Damaged02"));
                        }
                    }
                }
                break;
            case eREACTION_TYPE.ePUSHING:
                {
                    float fDeltaTime = charScr.m_PushingMove.m_fPushingTime - Time.deltaTime;
                    Vector3 movePos = Vector3.zero;
                    float fDurationFreeze = Time.deltaTime;

                    if (charScr.ksAnimation.m_fPauseTimer > 0.0f)
                    {
                        charScr.m_pushInfo.m_fFreezeTime -= Time.deltaTime;
                        if (charScr.m_pushInfo.m_fFreezeTime > 0.0f)
                        {
                            fDurationFreeze = 0.0f;
                        }
                    }

                    charScr.m_PushingMove.m_fPushingTime -= fDurationFreeze;

                    BuffData_ReAction kBuff = charScr.m_BuffController.FindFrontReActionBuff();

                    if (kBuff != null && kBuff.m_AffectCode == eAffectCode.ePUSHING && kBuff.m_SourceChar != null)
                    {
                        movePos = kBuff.m_SourceChar.m_pushInfo.m_MoveDelta;
                    }
                    else
                        movePos = charScr.m_PushingMove.m_Dir * charScr.m_PushingMove.m_Speed * Time.deltaTime;

                    if (fDeltaTime <= 0.0f)
                    {
                        charScr.SendMessage("OnAniEventEnd", charScr.ksAnimation.GetAnimationClip(charScr.ksAnimation.GetCurrentClipName()));
                    }

                    charScr.Move(movePos, eAffectCode.eNONE, eSkillActionType.ePUSH);
                }
                break;
            case eREACTION_TYPE.eSCREW:
                {
                    Vector3 movePos = Vector3.zero;
                    float kDelta = charScr.m_ScrewMove.m_fScrewTotlaTime - Time.deltaTime;

                    if (kDelta < 0.0f)
                    {
                        kDelta = charScr.m_ScrewMove.m_fScrewTotlaTime;
                    }
                    else
                        kDelta = Time.deltaTime;

                    charScr.m_ScrewMove.m_fScrewTotlaTime -= kDelta;

                    charScr.m_ScrewMove.m_Dir = charScr.m_ScrewMove.m_CenterPos - charScr.m_MyObj.position;
                    charScr.m_ScrewMove.m_Dir.y = 0.0f;
                    charScr.m_ScrewMove.m_Dir.Normalize();

                    if (charScr.m_ScrewMove.m_fScrewTotlaTime <= 0.0f)
                    {
                        charScr.SendMessage("OnAniEventEnd", charScr.ksAnimation.GetAnimationClip(charScr.ksAnimation.GetCurrentClipName()));
                    }

                    charScr.m_ScrewMove.m_fScrewTime += kDelta;
                    if (charScr.m_ScrewMove.m_fScrewTime > charScr.m_ScrewMove.m_fScrewIntervalTime)
                    {
                        charScr.m_ScrewMove.m_fScrewTime = 0.0f;
                        charScr.ksAnimation.Rewind();
                    }

                    if (charScr.m_ScrewMove.m_Speed != 0.0f)
                    {
                        movePos = charScr.m_ScrewMove.m_Dir * charScr.m_ScrewMove.m_Speed * kDelta;
                        charScr.Move(movePos, eAffectCode.eNONE, eSkillActionType.ePUSH);
                    }
                }
                break;
            case eREACTION_TYPE.eGUARDBREAK:
                break;
            case eREACTION_TYPE.eFROZEN:

                AnimationState kAniState = charScr.ksAnimation.GetAnimationState(charScr.ksAnimation.GetCurrentClipName());
                if (kAniState != null)
                {
                    kAniState.speed -= (kAniState.length * 0.77f) * Time.deltaTime;
                    if (kAniState.speed <= 0.0f)
                        kAniState.speed = 0.0f;
                }
                charScr.m_ReactionBuffData.m_fSpecialTime -= Time.deltaTime;
                if (charScr.m_ReactionBuffData.m_fSpecialTime <= 0.0f)
                {
                    //charScr.SendMessage("OnAniEventEnd", charScr.ksAnimation.GetAnimationData(charScr.ksAnimation.sPlayClipName));
                    charScr.SendMessage("OnAniEventEnd", charScr.ksAnimation.GetAnimationClip(charScr.ksAnimation.GetCurrentClipName()));
                }
                break;
        }
    }
    public override void Exit(GameObject charObj, CharacterBase charScr)
    {
        bool bMovePacket = true;

        switch (SkillDataManager.instance.GetReactionType(charScr.m_ReactionAffectCode))
        {
            case eREACTION_TYPE.ePHYGICAL_DAMAGE:
                break;
            case eREACTION_TYPE.eSTUNNED:
                break;
            case eREACTION_TYPE.ePANIC:
                break;
            case eREACTION_TYPE.eKNOCKBACK:
            case eREACTION_TYPE.eKNOCKBACK_INT_2:
                // 異⑸룎 媛??
                if (charScr.m_CharState == CHAR_STATE.ALIVE)
                    charScr.SetCollisionDetection(true);
                break;
            case eREACTION_TYPE.eKNOCKDOWN:
                // 異⑸룎 媛??
                if (charScr.m_CharState == CHAR_STATE.ALIVE)
                    charScr.SetCollisionDetection(true);
                break;
            case eREACTION_TYPE.eAIRBONE:
                break;
            case eREACTION_TYPE.ePULLING:
                if (charScr.m_CharState == CHAR_STATE.ALIVE)
                    charScr.SetCollisionDetection(true);
                bMovePacket = false;
                break;
            case eREACTION_TYPE.ePUSHING:
                if (charScr.m_CharState == CHAR_STATE.ALIVE)
                    charScr.SetCollisionDetection(true);
                break;
            case eREACTION_TYPE.eSTANDUP:
                break;
        }
        charScr.m_ReactionAffectCode = eAffectCode.eNONE;

        // net move
        if (InGameManager.instance.m_bOffLine == false)
        {
            switch (charScr.m_CharAi.m_eType)
            {
                case eAIType.ePC:
                    if (charScr.charUniqID == PlayerManager.MYPLAYER_INDEX && bMovePacket)
                    {
                        charScr.m_charController.m_DpadMP.StopRun(eMoveType.eNONE);
                    }
                    break;
                //=========================== zunghoon 20161205 Add Start =============================================
                //?덉뼱濡?NPC ???異붽?
                case eAIType.eHERONPC:
                //=========================== zunghoon 20161205 Add End ===============================================
                case eAIType.eNPC:
                    if (InGameManager.instance.m_bHostPlay)
                    {
                        NetworkManager.instance.networkSender.SendPcMoveReq(charScr.m_IngameObjectID,
                                                                              charScr.m_MyObj.transform.forward,
                                                                              charScr.m_MyObj.transform.position,
                                                                              charScr.m_CharAi.GetNavMeshAgentSpeed(),
                                                                              (int)eMoveType.eNONE);
                    }
                    break;
            }
        }
    }
}

public class ObjectState_Die : ObjectState
{
    static ObjectState_Die m_Instance = null;
    static InGameManager m_IngameManager = null;
    float die_AniTime = 0.0f;
    float vintageTime = 0;
    public static ObjectState_Die Instance()
    {
        if (m_Instance == null)
        {
            m_Instance = new ObjectState_Die();
            m_IngameManager = InGameManager.instance;
        }

        return m_Instance;
    }

    public override MOTION_STATE GetEnumType()
    {
        return MOTION_STATE.eMOTION_IDLE;
    }

    public override void Enter(GameObject charObj, CharacterBase charScr)
    {
        if (charScr.m_bDieImed)
        {
            if (charScr.m_CharUniqID == 0)
            {
#if NONSYNC_PVP
                if (SceneManager.instance.GetCurGameMode() != SceneManager.eGAMEMODE.eNONSYNC_PVP)
#endif
#if CAMFILTER
                {
                    m_IngameManager.vintage.enabled = true;
                    m_IngameManager.vintage.Amount = 0.0f;
                }
#endif
            }
            switch (charScr.m_CharAi.m_eType)
            {
                case eAIType.ePC:
                    die_AniTime = 0.0f;
                    vintageTime = 0;
                    //charScr.ksAnimation.Play(charScr.ksAnimation.GetAnimationData("Die"), 0.15f, true);'
                    charScr.ksAnimation.PlayAnim("Die", 0.15f);
                    die_AniTime = 0.5f / charScr.ksAnimation.GetAnimationClip("Die").length;
                    break;
                case eAIType.eNPC:
                    {
                        charScr.ksAnimation.PlayAnim("Die", 0.15f);
                    }
                    break;
            }
            charScr.SetCollisionDetection(false);    
        }
        else
        {
            //Debug.Log("Call Die!!!!!!");
            charScr.SendMessage("OnAniEventEnd", charScr.ksAnimation.GetAnimationClip("Die"));
            //charScr.SendMessage("OnAniEventEnd", charScr.ksAnimation.GetAnimationData("Die"));
        }
        charScr.m_fDieTimeOut = 0.0f;
    }

    public override void Execute(GameObject charObj, CharacterBase charScr)
    {
        if (charScr.m_CharAi.m_eType == eAIType.ePC)
        {
            vintageTime += Time.deltaTime;


#if CAMFILTER
            m_IngameManager.vintage.Amount = vintageTime * die_AniTime;
            if (m_IngameManager.vintage.Amount >= 0.8f)
            {
                m_IngameManager.vintage.Amount = 0.8f;
            }
#endif
        }
        

    }
}

public class ObjectState_Revival : ObjectState
{
    static ObjectState_Revival m_Instance = null;

    public static ObjectState_Revival Instance()
    {
        if (m_Instance == null)
        {
            m_Instance = new ObjectState_Revival();
        }

        return m_Instance;
    }

    public override MOTION_STATE GetEnumType()
    {
        return MOTION_STATE.eMOTION_REVIVAL;
    }

    public override void Enter(GameObject charObj, CharacterBase charScr)
    {
        charScr.m_UseSkillIndex = charScr.m_currSkillIndex;
        charScr.nSkillEffectSequenceIndex = 1;
        charScr.nSkillProjectileSequenceIndex = 1;

        if (charScr.m_CharacterType != eCharacter.eNPC)
        {
            //charScr.ksAnimation.Play(charScr.ksAnimation.GetAnimationData(charScr.m_AttackInfos[charScr.m_currSkillIndex].skillinfo.aniResInfo[0].animation_name), 0, true);
            charScr.ksAnimation.PlayAnim(charScr.m_AttackInfos[charScr.m_currSkillIndex].skillinfo.aniResInfo[0].animation_name, 0);
        }
        if (InGameManager.instance.m_bOffLine == false && (PlayerManager.MYPLAYER_INDEX == charScr.charUniqID || (charScr.m_CharAi.m_eType == eAIType.ePC && charScr.m_CharAi.m_OnlineUser == false)))
        {
            List<CharacterBase> kTargets = new List<CharacterBase>();

            NetworkManager.instance.networkSender.SendPcActiveSkillReq(charScr.m_IngameObjectID,
                                                                        charScr.m_UseSkillIndex,
                                                                        true, // ?쒖옉 ?좊땲硫붿씠???⑦궥
                                                                        charScr.m_MyObj.transform.position,
                                                                        charScr.m_MyObj.transform.forward,
                                                                        Vector3.zero,
                                                                        kTargets, // 鍮??寃?
                                                                        0);
        }
    }

    public override void Execute(GameObject charObj, CharacterBase charScr)
    {
    }
    public override void Exit(GameObject charObj, CharacterBase charScr)
    {
        if (charScr.charUniqID != PlayerManager.MYPLAYER_INDEX && charScr.m_CharState == CHAR_STATE.DEATH)
        {
			if(SceneManager.instance.GetCurGameMode() == SceneManager.eGAMEMODE.eNONSYNC_PVP)
			{
				return;
			}
            charScr.m_BuffController.BuffEnd(charScr.m_ReactionBuffData);
            //ksAnimation.SetDontPlayAnymore(false);
            charScr.SetCharacterState(CHAR_STATE.ALIVE);
        }
    }
}
public class ObjectState_Warp : ObjectState
{
    static ObjectState_Warp m_Instance = null;

    public static ObjectState_Warp Instance()
    {
        if (m_Instance == null)
        {
            m_Instance = new ObjectState_Warp();
        }

        return m_Instance;
    }

    public override MOTION_STATE GetEnumType()
    {
        return MOTION_STATE.eMOTION_WARP_IN;
    }

    public override void Enter(GameObject charObj, CharacterBase charScr)
    {
        switch(charScr.m_MotionState)
        {
            case MOTION_STATE.eMOTION_WARP_IN:
                charScr.ksAnimation.PlayAnim("N_WARP_IN", 0.15f);
                break;
            case MOTION_STATE.eMOTION_WARP_OUT:
                charScr.ksAnimation.PlayAnim("N_WARP_OUT", 0.15f);
                break;
        }
    }

    public override void Execute(GameObject charObj, CharacterBase charScr) { }

    public override void Exit(GameObject charObj, CharacterBase charScr)
    {
        //switch (charScr.m_MotionState)
        //{
        //    case MOTION_STATE.eMOTION_WARP_IN:
        //        charScr.SendMessage("OnAniEventEnd", charScr.ksAnimation.GetAnimationClip("N_WARP_IN"));
        //        break;
        //    case MOTION_STATE.eMOTION_WARP_OUT:
        //        charScr.SendMessage("OnAniEventEnd", charScr.ksAnimation.GetAnimationClip("N_WARP_IN"));
        //        break;
        //}
    }
}


public class ObjectState_Spwan : ObjectState
{
    static ObjectState_Spwan m_Instance = null;

    public static ObjectState_Spwan Instance()
    {
        if (m_Instance == null)
        {
            m_Instance = new ObjectState_Spwan();
        }

        return m_Instance;
    }

    public override MOTION_STATE GetEnumType()
    {
        return MOTION_STATE.eMOTION_SPWAN;
    }

    public override void Enter(GameObject charObj, CharacterBase charScr)
    {
        if (charScr.m_CharAi.m_eType == eAIType.eNPC || charScr.m_CharAi.m_eType == eAIType.eHERONPC)
        {
            charScr.ksAnimation.PlayAnim("Idle", 0.15f);
        }
    }

    public override void Execute(GameObject charObj, CharacterBase charScr){ }

    public override void Exit(GameObject charObj, CharacterBase charScr)
    {
        if (charScr.m_CharAi.m_eType == eAIType.eNPC)
        {
            charScr.ksAnimation.SetAnimationCullType(AnimationCullingType.AlwaysAnimate);

#if NONSYNC_PVP
            if (SceneManager.instance.GetCurGameMode() == SceneManager.eGAMEMODE.eNONSYNC_PVP)
                ((HeroNpcAI)charScr.m_CharAi).SetPeaceState();
            else
#endif
            {
                ((NpcAI)charScr.m_CharAi).SetPeaceState();
            }
        }
        else if (charScr.m_CharAi.m_eType == eAIType.eHERONPC)
        {
            charScr.ksAnimation.SetAnimationCullType(AnimationCullingType.AlwaysAnimate);
//            ((HeroNpcAI)charScr.m_CharAi).SetPeaceState();
        }
    }
}

public class ObjectState_SpwaneStanby : ObjectState
{
    static ObjectState_SpwaneStanby m_Instance = null;

    static public float NpcspwanTime = 0.8f;
    public static ObjectState_SpwaneStanby Instance()
    {
        if (m_Instance == null)
        {
            m_Instance = new ObjectState_SpwaneStanby();
        }

        return m_Instance;
    }

    public override MOTION_STATE GetEnumType()
    {
        return MOTION_STATE.eMOTION_SPWAN;
    }

    public override void Enter(GameObject charObj, CharacterBase charScr)
    {
        if(charScr.m_CharacterType == eCharacter.eHERONPC)
        {
            NpcspwanTime = charScr.ksAnimation.GetAnimationClip("Spawn").length;
        }
        if (charScr.m_CharAi.GetNpcProp().Npc_Type == eNPC_TYPE.eMONSTER)
        {
            if(charScr.ksAnimation.GetAnimationClip("Spawn") != null)
            {
                charScr.ksAnimation.PlayAnim("Spawn", 1.0f);
                charScr.m_spwanTime = 0.3f;
                
                NpcspwanTime = charScr.ksAnimation.GetAnimationClip("Spawn").length;

                charScr.m_CharacterDamageEffect.SetSpawnMaterial(10f, NpcspwanTime);
            }
            else
            {
                charScr.m_spwanTime = 0f;                
                charScr.ksAnimation.PlayAnim("Idle", 1.0f);
                NpcspwanTime = 0.8f;

                charScr.m_CharacterDamageEffect.SetSpawnMaterial(10f, NpcspwanTime);
            }
        }
    }

    public override void Execute(GameObject charObj, CharacterBase charScr)
    {
        if (charScr.m_CharAi.GetNpcProp().Npc_Type == eNPC_TYPE.eMONSTER)
        {
            charScr.m_spwanTime += Time.deltaTime;
            charScr.m_CharacterDamageEffect.SpwanProcess(charScr.m_spwanTime);
        }
    }

    public override void Exit(GameObject charObj, CharacterBase charScr)
    {
        if (charScr.m_CharAi.m_eType == eAIType.eNPC)
        {
            charScr.m_CharacterDamageEffect.Spawn_RecoveryMaterial();
            charScr.ksAnimation.SetAnimationCullType(AnimationCullingType.AlwaysAnimate);
        }
        else if (charScr.m_CharAi.m_eType == eAIType.eHERONPC)
        {
            charScr.m_CharacterDamageEffect.Spawn_RecoveryMaterial();
            charScr.ksAnimation.SetAnimationCullType(AnimationCullingType.AlwaysAnimate);
        }
    }
}

public class ObjectState_Walk : ObjectState
{
    static ObjectState_Walk m_Instance = null;

    public static ObjectState_Walk Instance()
    {
        if (m_Instance == null)
        {
            m_Instance = new ObjectState_Walk();
        }

        return m_Instance;
    }

    public override MOTION_STATE GetEnumType()
    {
        return MOTION_STATE.eMOTION_WALK;
    }

    public override void Enter(GameObject charObj, CharacterBase charScr)
    {
        //charScr.ksAnimation.PlayAnim("Walk", 0.15f);
        charScr.ksAnimation.PlayAnim("Run", 0.15f);
    }

    public override void Execute(GameObject charObj, CharacterBase charScr)
    {
    }
    public override void Exit(GameObject charObj, CharacterBase charScr)
    {
    }
}

public class ObjectState_Casting : ObjectState
{
    static ObjectState_Casting m_Instance = null;

    public static ObjectState_Casting Instance()
    {
        if (m_Instance == null)
        {
            m_Instance = new ObjectState_Casting();
        }

        return m_Instance;
    }

    public override MOTION_STATE GetEnumType()
    {
        return MOTION_STATE.eMOTION_SPWAN;
    }

    public override void Enter(GameObject charObj, CharacterBase charScr)
    {
//        charScr.ksAnimation.Play("Idle", 0.15f);
    }

    public override void Execute(GameObject charObj, CharacterBase charScr)
    {
    }
    public override void Exit(GameObject charObj, CharacterBase charScr)
    {
    }
}

public class ObjectState_NpcAttack : ObjectState
{
    static ObjectState_NpcAttack m_Instance = null;

    public static ObjectState_NpcAttack Instance()
    {
        if (m_Instance == null)
        {
            m_Instance = new ObjectState_NpcAttack();
        }

        return m_Instance;
    }

    public override MOTION_STATE GetEnumType()
    {
        return MOTION_STATE.eMOTION_NPCATTACK;
    }

    public override void Enter(GameObject charObj, CharacterBase charScr)
    {
		float fAttackSpeed					= 0;
		float fAttackSpeedRatio				= 0;
		float fPlayerComboAttackSpeed		= 0;

		charScr.m_UseSkillIndex = charScr.m_currSkillIndex;

        charScr.nSkillEffectSequenceIndex = 1;
        charScr.nSkillProjectileSequenceIndex = 1;
        int animationIdx = (int)charScr.animState;
        if (charScr.animState == ANIMATION_STATE.eANIM_LOOP)
        {
            charScr.m_mAnimCtr.castingTime = charScr.m_AttackInfos[charScr.m_UseSkillIndex].skillinfo.casting_Time;
        }


		if (charScr.m_CharacterType == eCharacter.eWarrior || charScr.m_CharacterType == eCharacter.eWizard)        
        {
			fAttackSpeedRatio			= charScr.damageCalculationData.fCOMBOATTACK_SPEED + charScr.damageCalculationData.fTOTAL_ATTACKSPEED_RATIO + (float)charScr.damageCalculationData.dSUMMONATTRIBUTE_ATT_SPEED_RATIO;
			fAttackSpeed				= charScr.damageCalculationData.fTOTAL_ATTACKSPEED_ADD * (1 + (fAttackSpeedRatio * 0.01f));

			charScr.ksAnimation.PlayNoBlend(charScr.m_AttackInfos[charScr.m_UseSkillIndex].skillinfo.aniResInfo[animationIdx].animation_name, fAttackSpeed);
        }
        else if(charScr.m_CharacterType == eCharacter.eHERONPC)
        {
			fPlayerComboAttackSpeed		= PlayerManager.instance.playerInfoList[PlayerManager.MYPLAYER_INDEX].playerCharBase.damageCalculationData.fCOMBOATTACK_SPEED;
			//fAttackSpeedRatio			= fPlayerComboAttackSpeed + charScr.damageCalculationData.fATTACK_SPEED_UP_RATIO + (float)charScr.damageCalculationData.dSUMMONATTRIBUTE_ATT_SPEED_RATIO;
			fAttackSpeedRatio			= fPlayerComboAttackSpeed + charScr.damageCalculationData.fTOTAL_ATTACKSPEED_RATIO + (float)charScr.damageCalculationData.dSUMMONATTRIBUTE_ATT_SPEED_RATIO;
			fAttackSpeed				= charScr.damageCalculationData.fTOTAL_ATTACKSPEED_ADD * (1 + (fAttackSpeedRatio * 0.01f));

			charScr.ksAnimation.PlayNoBlend(charScr.m_AttackInfos[charScr.m_UseSkillIndex].skillinfo.aniResInfo[animationIdx].animation_name, fAttackSpeed);
        }
        else
		{
			//fAttackSpeedRatio			= charScr.damageCalculationData.fATTACK_SPEED_UP_RATIO;
			fAttackSpeedRatio			= charScr.damageCalculationData.fTOTAL_ATTACKSPEED_RATIO;
			fAttackSpeed				= charScr.damageCalculationData.fTOTAL_ATTACKSPEED_ADD * (1 + (fAttackSpeedRatio * 0.01f));

			charScr.ksAnimation.PlayNoBlend(charScr.m_AttackInfos[charScr.m_UseSkillIndex].skillinfo.aniResInfo[animationIdx].animation_name, fAttackSpeed);
		}            

        if (charScr.m_CharAi.m_eType == eAIType.ePC)
        {
            switch (((AutoAI)charScr.m_CharAi).m_AtkComboTree)
            {
                case eATTACKCOMBO_TREE.DASH:
                case eATTACKCOMBO_TREE.MAX_DASH:
                    if (charScr.attackTarget != null)
                        charScr.m_ComboDashDist = Vector3.Distance(charObj.transform.position, charScr.attackTarget.m_MyObj.transform.position);
                    break;
            }
        }
        if (charScr.m_CharAi.m_eType == eAIType.eNPC)
            charScr.m_CharAi.SetNavMeshavoidancePriority(1);
    }

    public override void Execute(GameObject charObj, CharacterBase charScr)
    {
        if (charScr.animState == ANIMATION_STATE.eANIM_LOOP)
        {
            if (0 < charScr.m_mAnimCtr.castingTime)
            {
                charScr.m_mAnimCtr.castingTime -= Time.deltaTime;
            }
            else
            {
                charScr.animState = ANIMATION_STATE.eANIM_END;
                charScr.ksAnimation.PlayAnim(charScr.m_AttackInfos[charScr.m_UseSkillIndex].skillinfo.aniResInfo[(byte)charScr.animState].animation_name, 0.15f);
            }


        }

        if ( charScr.m_CharAi.m_eType == eAIType.ePC )
        {
            switch (((AutoAI)charScr.m_CharAi).m_AtkComboTree)
            {
                case eATTACKCOMBO_TREE.DASH:
                case eATTACKCOMBO_TREE.MAX_DASH:
                    if (charScr.m_pushInfo.m_bStart)
                    {
                        float fRemainDash = charScr.m_pushInfo.m_fDuration - Time.deltaTime;
                        //float fDurationFreeze = Time.deltaTime;

                        if (fRemainDash < 0.0f)
                        {
                            fRemainDash = charScr.m_pushInfo.m_fDuration;
                            charScr.m_pushInfo.m_bStart = false;
                        }
                        else
                            fRemainDash = Time.deltaTime;

                        Vector3 movePosition = charScr.transform.forward * charScr.m_pushInfo.m_fSpeed * fRemainDash;

                        charScr.Move(movePosition);

                        charScr.m_pushInfo.m_fDuration -= Time.deltaTime;
                    }
                    break;
            }
        }
    }
    public override void Exit(GameObject charObj, CharacterBase charScr)
    {
        if (charScr.m_CharAi.m_eType == eAIType.eNPC)
            charScr.m_CharAi.RollBackNavMeshavoidancePriority();
        
        if (charScr.animState == ANIMATION_STATE.eANIM_END)
            charScr.animState = ANIMATION_STATE.eANIM_START;
}
}

public class ObjectState_Skill : ObjectState
{
    static ObjectState_Skill m_Instance = null;

    //private static int m_AniIdx;
    //public static int aniIndex
    //{
    //    set { m_AniIdx = value; }
    //}

    public static ObjectState_Skill Instance()
    {
        if (m_Instance == null)
        {
            m_Instance = new ObjectState_Skill();
            //m_AniIdx = 0;
        }

        return m_Instance;
    }

    public override MOTION_STATE GetEnumType()
    {
        return MOTION_STATE.eMOTION_SKILL;
    }

    public override void Enter(GameObject charObj, CharacterBase charScr)
    {
        int pStance = 0;
        if (charScr.m_CharacterType != eCharacter.eNPC && charScr.m_CharacterType != eCharacter.eHERONPC)
        {
            AutoAI autoAI = (AutoAI)charScr.m_CharAi;
            pStance = (int)autoAI.GetStance();
        }
        else
        {
            pStance = 4;
        }
        
        charScr.m_UseSkillIndex = charScr.m_currSkillIndex;

        CAttackInfo kAttackInfo = charScr.m_AttackInfos[charScr.m_UseSkillIndex];
        SkillDataInfo.SkillInfo kSkillInfo = kAttackInfo.skillinfo;

        if( kAttackInfo.skillinfo.aniResInfo.Count > 0 && kAttackInfo.skillinfo.aniResInfo[0].animation_id != 0 )
        {
            float aniTime = charScr.ksAnimation.GetAnimationClip(kAttackInfo.skillinfo.aniResInfo[0].animation_name).length;
            charScr.m_CharacterDamageEffect.TestDamageEffect(pStance, aniTime);

            if(charScr.m_CharacterType == eCharacter.eWizard)
            {
                if(PlayerManager.instance.GetSpecialSkill(charScr, pStance, 0) == charScr.m_UseSkillIndex)
                {
                    if (!charScr.m_EffectSkillManager.IsPlayingSameEffect(kSkillInfo.skill_Effect[0]))
                    {
                        //Debug.Log("###### skill index = " + charScr.m_UseSkillIndex);
                        charScr.m_EffectSkillManager.FX_Play(charScr);
                    }
                }
            }
        }
        else
        {
            charScr.m_CharAi.m_eEndMotionState = MOTION_STATE.eMOTION_SKILL;
            charScr.HitPoint_Attack(0);
        }

        switch (kSkillInfo.action_Type)
        {
            case eSkillActionType.eRUSH:
                charScr.m_pushInfo.Init();
                break;
            case eSkillActionType.eTYPE_PENETRATION:
            case eSkillActionType.eAERO:
                charScr.m_pushInfo.Init();
                charScr.SetCollisionDetection(false);
                break;
            case eSkillActionType.eCHAIN:
                break;
            case eSkillActionType.eSHIELD_BLOCK:
                charScr.m_GuardState = SHIELDGUARD_STATE.eGUARDING;
                break;
            case eSkillActionType.eMOVEMENT:
                {
                    charScr.m_ArollingSkill = charScr.m_UseSkillIndex;
                    charScr.SetCollisionDetection(false);
                }
                break;
            case eSkillActionType.eSHOOT:
                {
                    switch (kSkillInfo.casting_Type)
                    {
                        case eCastingType.eCASTING:
                            break;
                        case eCastingType.eCHANNELING:
                            charScr.m_ChannelingState.m_eState = CHANNELING_STATE.eSTART;
                            break;
                    }
                }
                break;
            case eSkillActionType.eMAGICCIRCLE:
                if (PlayerManager.MYPLAYER_INDEX == charScr.charUniqID)
                {
                    if ((charScr.attackTarget == null || charScr.attackTarget == charScr || charScr.attackTarget.chrState == CHAR_STATE.DEATH))
                    {
                        charScr.SetAttackTarget(charScr.GetTarget(kAttackInfo, kSkillInfo.area_Angle));
                    }
                    if( charScr.attackTarget != null && charScr.attackTarget != charScr)
                    {
                        charScr.LookAtY(charScr.attackTarget.m_MyObj.position);
                    }
                }
                break;
        }

        if (InGameManager.instance.m_bOffLine == false &&   (PlayerManager.MYPLAYER_INDEX == charScr.charUniqID ||
                                                            (charScr.m_CharAi.m_eType == eAIType.ePC && charScr.m_CharAi.m_OnlineUser == false) ||
                                                            (InGameManager.instance.m_bHostPlay && charScr.m_CharacterType == eCharacter.eNPC)))
        {
            bool bSkipSend = false;
            // ?ъ뒳? 2踰??몄텧?⑥쑝濡?泥ル쾲吏몃쭔 ?좏슚?섎떎
            if (kSkillInfo.action_Type == eSkillActionType.eCHAIN && charScr.m_ChainMove.m_State != CHAIN_STATE.eNONE)
                bSkipSend = true;
            
            if( bSkipSend == false )
            {
                List<CharacterBase> kTargets = new List<CharacterBase>();

                switch (kSkillInfo.action_Type)
                {
                    case eSkillActionType.eCHAIN:
                        if (charScr.m_ChainMove.m_CatchChar != null)
                            kTargets.Add(charScr.m_ChainMove.m_CatchChar);
                        break;
                    default:
                        break;
                }
                NetworkManager.instance.networkSender.SendPcActiveSkillReq(charScr.m_IngameObjectID,
                                                                            charScr.m_UseSkillIndex,
                                                                            true, // ?쒖옉 ?좊땲硫붿씠???⑦궥
                                                                            charScr.m_MyObj.transform.position,
                                                                            charScr.m_MyObj.transform.forward,
                                                                            Vector3.zero,
                                                                            kTargets, // 鍮??寃?
                                                                            0);
            }
        }

        CAttackInfo atkInfo = kAttackInfo;
        charScr.nSkillEffectSequenceIndex = 1;
        charScr.nSkillProjectileSequenceIndex = 1;

        if (kSkillInfo.action_Type != eSkillActionType.eCHAIN)
        {
            if (atkInfo.skillinfo.aniResInfo.Count > 0 && atkInfo.skillinfo.aniResInfo[0].animation_id != 0)
                charScr.ksAnimation.PlayAnim(atkInfo.skillinfo.aniResInfo[0].animation_name, 0.15f);
        }
        else
        {
            charScr.m_ChainMove.m_AnimName = kSkillInfo.aniResInfo[0].animation_name;

            if (InGameManager.instance.m_bOffLine == false && charScr.m_ChainMove.m_State == CHAIN_STATE.eNONE)
            {
                charScr.m_SkillNetReturnData.m_Waiting = true;
                charScr.m_SkillNetReturnData.m_bPacketReceive = false;
                charScr.m_SkillNetReturnData.m_Target = null;
                charScr.m_SkillNetReturnData.m_iSkillCode = charScr.m_UseSkillIndex;
                charScr.m_SkillNetReturnData.m_fTimeOut = 3.0f;
            }
            charScr.ksAnimation.PlayAnim(charScr.m_ChainMove.m_AnimName, 0.15f);
        }

        if (charScr.m_CharAi.m_eType == eAIType.eNPC)
            charScr.m_CharAi.SetNavMeshavoidancePriority(1);

        if (atkInfo.skillinfo.Guide_Effect > 0 )
        {
            if (atkInfo.skillinfo.aniResInfo.Count > 0 && atkInfo.skillinfo.aniResInfo[0].animation_id != 0)
            {
                float fLifeTime = 2.0f;

                AnimationClip kSkillAniMation = charScr.ksAnimation.GetAnimationClip(atkInfo.skillinfo.aniResInfo[0].animation_name);

                AnimationEvent[] kEvents = kSkillAniMation.events;

                Vector3 MovePosition = Vector3.zero;
                //float MoveDist = 0.0f;
                for (int i = 0; i < kEvents.Length; ++i)
                {
                    if (string.Equals(kEvents[i].functionName, "MoveForward"))
                    {
                        string[] datas = kEvents[i].stringParameter.Split(',');

//                        float fTime = float.Parse(datas[0]);
                        float fDist = float.Parse(datas[1]);

                        MovePosition += (charScr.m_MyObj.transform.forward * fDist);

                    }
                    if (string.Equals(kEvents[i].functionName, "HitPoint_Attack"))
                    {
                        // 泥?? 源뚯?留?
                        fLifeTime = kEvents[i].time;
                        break;
                    }
                }

                charScr.GuideEffectRender(atkInfo.skillinfo, charScr.m_MyObj.transform.position , MovePosition, fLifeTime);
            }
        }
    }

    public override void Execute(GameObject charObj, CharacterBase charScr)
    {

        SkillDataInfo.SkillInfo kSkillInfo = charScr.m_AttackInfos[charScr.m_UseSkillIndex].skillinfo;

        charScr.m_skSender.ClearExceptionTarget();

        //GameObject.FindGameObjectWithTag
        switch (kSkillInfo.action_Type)
        {
            case eSkillActionType.eNORMAL:
                //nothing to do during animation is played
                break;
            case eSkillActionType.eSHOOT:
                switch (kSkillInfo.casting_Type)
                {
                    case eCastingType.eCASTING:
                        break;
                    case eCastingType.eCHANNELING:

                        if (charScr.m_ChannelingState.m_eState == CHANNELING_STATE.eLOOPREADY)
                        {
                            charScr.m_ChannelingState.m_eState = CHANNELING_STATE.eLOOP;
                            charScr.ksAnimation.PlayAnim(kSkillInfo.aniResInfo[1].animation_name, 0.15f);
                        }

                        break;
                }
                break;
            case eSkillActionType.eMOVEMENT:
                break;
            case eSkillActionType.eRUSH:        
                {
                    if(charScr.m_pushInfo.m_bStart)
                    {
                        //Debug.Log("dist = "+dist);
                        float kSpeed = charScr.m_pushInfo.m_fSpeed;

                        float fRemainDash = charScr.m_pushInfo.m_fDuration - Time.deltaTime;
                        float fDurationFreeze = Time.deltaTime;

                        if (charScr.ksAnimation.m_fPauseTimer > 0.0f)
                        {
                            charScr.m_pushInfo.m_fFreezeTime -= Time.deltaTime;
                            if (charScr.m_pushInfo.m_fFreezeTime > 0.0f)
                            {
                                kSpeed *= 0.00f;
                                fDurationFreeze = 0.0f;
                            }
                        }
                        if (fRemainDash < 0.0f)
                        {
                            fRemainDash = charScr.m_pushInfo.m_fDuration;
                            charScr.m_pushInfo.m_bStart = false;
                        }
                        else
                            fRemainDash = Time.deltaTime;
                        {
                            Vector3 movePosition = charScr.transform.forward * kSpeed * fRemainDash;
                            if (InGameManager.instance.m_bOffLine == true || (PlayerManager.MYPLAYER_INDEX == charScr.charUniqID ||
                                                                             (charScr.m_CharAi.m_eType == eAIType.ePC && charScr.m_CharAi.m_OnlineUser == false) || 
                                                                             (InGameManager.instance.m_bHostPlay && charScr.m_CharacterType == eCharacter.eNPC)))
                            {
                               SkillDataInfo.SkillInfo skillInfo = kSkillInfo;

                               charScr.m_skSender.DoPush(charScr, skillInfo, movePosition, 0);
                            }

                            charScr.Move(movePosition);

                            charScr.m_pushInfo.m_MoveDelta = movePosition;
                        }
                        charScr.m_pushInfo.m_fDuration -= fDurationFreeze;
                    }
                }
                break;

            case eSkillActionType.eTYPE_PENETRATION:
                {
                    if (charScr.m_pushInfo.m_bStart)
                    {
                        float kSpeed = charScr.m_pushInfo.m_fSpeed;
                        float fRemainDash = charScr.m_pushInfo.m_fDuration - Time.deltaTime;

                        if (fRemainDash < 0.0f)
                        {
                            fRemainDash = charScr.m_pushInfo.m_fDuration;
                            charScr.m_pushInfo.m_bStart = false;
                        }
                        else
                            fRemainDash = Time.deltaTime;
                        {
                            Vector3 movePosition = charScr.transform.forward * kSpeed * fRemainDash;

                            charScr.Move(movePosition, eAffectCode.eNONE, eSkillActionType.eTYPE_PENETRATION);
                        }
                        charScr.m_pushInfo.m_fDuration -= Time.deltaTime;
                    }
                }
                break;
            case eSkillActionType.eAERO:
                if (charScr.m_pushInfo.m_bStart)
                {

                    
                    float kSpeed = charScr.m_pushInfo.m_fSpeed;
                    float fRemainDash = charScr.m_pushInfo.m_fDuration - Time.deltaTime;

                    if (fRemainDash < 0.0f)
                    {
                        fRemainDash = charScr.m_pushInfo.m_fDuration;
                        charScr.m_pushInfo.m_bStart = false;
                    }
                    else
                        //fRemainDash = Time.deltaTime;
                    {
                        fRemainDash = Time.deltaTime;
                        Vector3 movePosition = charScr.transform.forward * kSpeed * fRemainDash;

                        charScr.Move(movePosition, eAffectCode.eNONE, eSkillActionType.eAERO);
                    }
                    charScr.m_pushInfo.m_fDuration -= Time.deltaTime;
                }
                else
                {
                    if (charScr.m_pushInfo.m_TargetPos != Vector3.zero )
                        charScr.LookAtY(charScr.m_pushInfo.m_TargetPos);                    
                }
                break;
            case eSkillActionType.eCHAIN:
                break;
            case eSkillActionType.eSHIELD_BLOCK:
                if (charScr.m_GuardState != SHIELDGUARD_STATE.eGUARDING)
                {
                    charScr.SendMessage("OnAniEventEnd", charScr.ksAnimation.GetAnimationClip(kSkillInfo.aniResInfo[0].animation_name));
                }
                else
                {
                    if (charScr.m_eGuardingHit == COMMON_STATE.eSTART)
                    {
                        charScr.ksAnimation.PlayNoBlend("W_SPECIAL_GUARD_Hit", 1.0f);
                        charScr.m_eGuardingHit = COMMON_STATE.eUPDATE;
                    }

                    if (charScr.m_eGuardingHit == COMMON_STATE.eUPDATE)
                    {
                        float fDelta = charScr.m_TimeGuardingHit - Time.deltaTime;

                        if (fDelta < 0.0f)
                        {
                            fDelta = charScr.m_TimeGuardingHit;
                            charScr.m_eGuardingHit = COMMON_STATE.eEND;
                        }
                        else
                            fDelta = Time.deltaTime;

                        charScr.m_TimeGuardingHit -= fDelta;
                        if (charScr.m_TimeGuardingHit >= 0)
                        {
                            if (InGameManager.instance.m_bOffLine == true)
                            {
                                Vector3 movePos = new Vector3(charScr.m_DmgBMove.m_Dir.normalized.x, 0, charScr.m_DmgBMove.m_Dir.normalized.z) * fDelta;
                                charScr.Move(-movePos, eAffectCode.eNONE);
                            }
                        }

                        if (charScr.m_eGuardingHit == COMMON_STATE.eEND)
                        {
                            charScr.ksAnimation.PlayAnim(kSkillInfo.aniResInfo[0].animation_name, 0.15f);
                            charScr.m_eGuardingHit = COMMON_STATE.eNONE;
                        }
                    }
                }
                break;
        }
    }

    public override void Exit(GameObject charObj, CharacterBase charScr)
    {

        switch (charScr.m_AttackInfos[charScr.m_UseSkillIndex].skillinfo.action_Type)
        {
            case eSkillActionType.eNORMAL:
                //nothing to do during animation is played
                break;
            case eSkillActionType.eSHOOT:
                switch (charScr.m_AttackInfos[charScr.m_UseSkillIndex].skillinfo.casting_Type)
                {
                    case eCastingType.eCASTING:
                        break;
                    case eCastingType.eCHANNELING:
                        charScr.m_ChannelingState.m_eState = CHANNELING_STATE.eNONE;
                        charScr.m_ChannelingState.m_bCancel = true;
                        break;
                }
                break;
            case eSkillActionType.eMOVEMENT:
                if (charScr.m_CharState == CHAR_STATE.ALIVE)
                {
                    charScr.SetCollisionDetection(true);
                    charScr.m_ArollingSkill = 0;
                }
                break;
            case eSkillActionType.eRUSH:
            case eSkillActionType.eTYPE_PENETRATION:
                if( charScr.m_CharState == CHAR_STATE.ALIVE)
                    charScr.SetCollisionDetection(true);
                charScr.m_pushInfo.Init();
                break;
            case eSkillActionType.eCHAIN:
                //Taylor : Chain remove
                //if (charScr.m_CharacterType != eCharacter.eNPC && PlayerManager.instance.m_PlayerInfo[charScr.charUniqID].chainBoneEffObj != null )
                //    PlayerManager.instance.m_PlayerInfo[charScr.charUniqID].chainBoneEffObj.gameObject.SetActive(false);

                //if (PlayerManager.instance.m_PlayerInfo[charScr.charUniqID].swordObj != null)
                //{
                //    PlayerManager.instance.m_PlayerInfo[charScr.charUniqID].swordObj.SetActive(true);
                //    charScr.m_ChainMove.m_State = CHAIN_STATE.eNONE;
                //}

                break;
            case eSkillActionType.eSHIELD_BLOCK:
                charScr.m_GuardState = SHIELDGUARD_STATE.eNONE;
                break;
        }
        if (charScr.m_CharAi.m_eType == eAIType.eNPC)
            charScr.m_CharAi.RollBackNavMeshavoidancePriority();

        // net move
        if (InGameManager.instance.m_bOffLine == false)
        {
            switch (charScr.m_CharAi.m_eType)
            {
                case eAIType.ePC:
                    if (charScr.charUniqID == PlayerManager.MYPLAYER_INDEX)
                    {
                        charScr.m_charController.m_DpadMP.StopRun(eMoveType.eNONE);
                    }
                    break;
                case eAIType.eHERONPC:
                case eAIType.eNPC:
                    if (InGameManager.instance.m_bHostPlay)
                    {
                        NetworkManager.instance.networkSender.SendPcMoveReq(charScr.m_IngameObjectID,
                                                                              charScr.m_MyObj.transform.forward,
                                                                              charScr.m_MyObj.transform.position,
                                                                              charScr.m_CharAi.GetNavMeshAgentSpeed(),
                                                                              (int)eMoveType.eNONE);
                    }
                    break;
            }
        }

    }
}

public class ObjectState_ETC : ObjectState
{
    public enum eSTATE : byte
    {
        VICTORY,
        CHANGESTANCE,
        HAPTIC
    }

    static ObjectState_ETC m_Instance       = null;

    private CharacterBase m_characterBase   = null;
    private float m_fAnimaitionTime         = 0;
    AnimationClip AniData = null;
    AnimationState AniState = null;

    public static ObjectState_ETC Instance()
    {
        if (m_Instance == null)
        {
            m_Instance = new ObjectState_ETC();
        }

        return m_Instance;
    }

    public override MOTION_STATE GetEnumType()
    {
        return MOTION_STATE.eMOTION_ETC;
    }

    public override void Enter(GameObject charObj, CharacterBase charScr)
    {
        m_characterBase = charScr;
    }

    public override void Execute(GameObject charObj, CharacterBase charScr)
    {
        m_fAnimaitionTime -= Time.deltaTime;

       // if (m_fAnimaitionTime < 0.0f)
       if(AniState.normalizedTime >= 1)
            charScr.SendMessage("OnAniEventEnd", charScr.ksAnimation.GetAnimationClip("Idle"));
        //charScr.SendMessage("OnAniEventEnd", charScr.ksAnimation.FindAnimationData("Idle"));
    }

    public void StartAni(eSTATE eState)
    {
        switch (eState)
        {
            case eSTATE.VICTORY:
                {
                    AniData = m_characterBase.ksAnimation.GetAnimationClip("Victory01");
                    if (AniData != null)
                    {
                        m_characterBase.ksAnimation.PlayAnim("Victory01", 0.15f);
                        m_fAnimaitionTime = AniData.length;
                    }
                    //AnimationData AniData = m_characterBase.ksAnimation.FindAnimationData("Victory01");
                    //if (AniData != null)
                    //{
                    //    m_characterBase.ksAnimation.Play(AniData, 0.15f, true);
                    //    m_fAnimaitionTime = AniData.clip.length;
                    //}
                }
                break;
            case eSTATE.CHANGESTANCE:
                {
                    AniData = m_characterBase.ksAnimation.GetAnimationClip("StanceChange");
                    if (AniData != null)
                    {
                        m_characterBase.ksAnimation.PlayAnim("StanceChange", 0.15f);
                        m_fAnimaitionTime = AniData.length;
                    }
                }
                break;
            case eSTATE.HAPTIC:
                {
                    string[] Anis = { "Lobby_Ani_1", "Lobby_Ani_2" };
                    string PlayAni = Anis[Random.Range(0, 2)];
                    AniState = m_characterBase.ksAnimation.m_Animation[PlayAni];
                    AniData = m_characterBase.ksAnimation.GetAnimationClip(PlayAni);
                    if (AniData != null)
                    {
                        m_characterBase.ksAnimation.PlayAnim(PlayAni, 0.15f);
                        m_fAnimaitionTime = AniData.length;
                    }
                }
                break;
            default:
                break;
        }
    }
}
