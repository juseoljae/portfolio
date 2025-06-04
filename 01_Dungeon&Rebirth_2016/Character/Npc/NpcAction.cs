using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using KSPlugins;

public class NpcAction : PoolItem
{
    public enum eNpcActionGrade
    {
        eBASE = 0,
        ePATTERN,
        eTIMETRIGER,
        eMAX_GRADE
    }

    public enum eNPpcActionType
    {
        eUSEBASEATTACK = 0,
        eUSESKILL,
        eMOVE,
        eCHANGEFORM,
        eCINEMA,
        eCHANGEPAGE,
        eNORMALATTACK,
        eETC,
        eBEATTACKTED = 1000,
    }

    public enum eNpcActionState
    {
        eREADY = 0,
        ePLAY,
        eEND
    }


    AI m_CharAi;
    public eNpcActionGrade m_eActionGrade;
    public eNpcActionState m_eNpcActionState = eNpcActionState.eREADY;

    public KeyValuePair<int, List<float>> m_ActionTypeAndInfo;
    public KeyValuePair<int, List<float>> m_TargetTypeAndInfo;

    public bool m_bCancelAble;

    private const float FREEZE_TIME = 30.0f;

    float m_WaitFreezeSolveTime = FREEZE_TIME;// Freezing Clear Time

    bool m_bDontStopAction = false;

    public override void Init()
    {
        m_CharAi = null;
        m_eActionGrade = eNpcActionGrade.eBASE;
        m_eNpcActionState = eNpcActionState.eREADY;
        if (m_ActionTypeAndInfo.Value != null)
        {
            m_ActionTypeAndInfo.Value.Clear();
        }

        if (m_TargetTypeAndInfo.Value != null)
        {
            m_TargetTypeAndInfo.Value.Clear();
        }

        m_bCancelAble = false;

    }
    public void InitAction(AI p_AI, eNpcActionGrade p_eActionGrade, int p_ActionType, float[] p_Action_Info, int p_TargetType, float[] p_Target_Info, bool p_bCancelAble, NpcAction p_NextAction)
    {
        m_CharAi = p_AI;
        m_eActionGrade = p_eActionGrade;

        List<float> tmpInfo = new List<float>();

        for (int j = 0; j < p_Action_Info.Length; ++j)
            tmpInfo.Add(p_Action_Info[j]);

        List<float> tmpInfo2 = new List<float>();
        for (int j = 0; j < p_Target_Info.Length; ++j)
            tmpInfo2.Add(p_Target_Info[j]);

        m_ActionTypeAndInfo = new KeyValuePair<int, List<float>>(p_ActionType, tmpInfo);
        m_TargetTypeAndInfo = new KeyValuePair<int, List<float>>(p_TargetType, tmpInfo2);

        m_bCancelAble = p_bCancelAble;

        m_eNpcActionState = eNpcActionState.eREADY;
    }

    public void PlayAction()
    {
        //Debug.Log("NpcAction PlayAction Key = "+ ((eNPpcActionType)m_ActionTypeAndInfo.Key));
        switch ((eNPpcActionType)m_ActionTypeAndInfo.Key)
        {
            case eNPpcActionType.eUSEBASEATTACK:
                { 
                    m_CharAi.GetCharacterBase().m_currSkillIndex = (int)m_ActionTypeAndInfo.Value[0];

                    SkillDataInfo.SkillInfo kSkillInfo = m_CharAi.GetCharacterBase().m_AttackInfos[m_CharAi.GetCharacterBase().m_currSkillIndex].skillinfo;

                    switch (kSkillInfo.target_Type)
                    {
                        case eTargetType.eTARGET:
                        case eTargetType.eTARGET_AREA:
                        case eTargetType.eSELF_AREA:
                        case eTargetType.eSELF:
                            break;
                        case eTargetType.eAREA:
                            m_CharAi.GetCharacterBase().attackPosition = (m_CharAi.GetCharacterBase().m_MyObj.forward * kSkillInfo.dist_Range) + m_CharAi.GetCharacterBase().m_MyObj.position;
                            break;
                    }
                    switch( m_CharAi.m_eType )
                    {
                        case eAIType.eHERONPC:
                        case eAIType.eNPC:
                            m_CharAi.GetCharacterBase().SetChangeMotionState(MOTION_STATE.eMOTION_NPCATTACK);
                            if (InGameManager.instance.m_bOffLine == false)
                                m_CharAi.m_eEndMotionState = MOTION_STATE.eMOTION_NONE;

                            // net atk
                            if (InGameManager.instance.m_bHostPlay)
                            {
                                List<CharacterBase> kTargets = new List<CharacterBase>();
                                kTargets.Add(m_CharAi.GetCharacterBase().attackTarget);

                                NetworkManager.instance.networkSender.SendPcAttackReq(m_CharAi.GetCharacterBase().m_IngameObjectID,
                                                                                        m_CharAi.GetCharacterBase().m_currSkillIndex,
                                                                                        true, 
                                                                                        m_CharAi.GetCharacterBase().m_MyObj.transform.position,
                                                                                        m_CharAi.GetCharacterBase().m_MyObj.transform.forward,
                                                                                        kTargets,
                                                                                        0,
                                                                                        0);
                            }

                            break;
                        case eAIType.ePC:
                            if (m_CharAi.GetCharacterBase().charUniqID == PlayerManager.MYPLAYER_INDEX && m_CharAi.GetCharacterBase().attackTarget != null && m_CharAi.GetCharacterBase().attackTarget != m_CharAi.GetCharacterBase())
                                m_CharAi.GetCharacterBase().SetAttackTarget(m_CharAi.GetCharacterBase().attackTarget);
                            m_CharAi.GetCharacterBase().SetChangeMotionState(MOTION_STATE.eMOTION_NPCATTACK);
                            if (InGameManager.instance.m_bOffLine == false)
                                m_CharAi.m_eEndMotionState = MOTION_STATE.eMOTION_NONE;

                            if (InGameManager.instance.m_bOffLine == false && (m_CharAi.GetCharacterBase().charUniqID == PlayerManager.MYPLAYER_INDEX || m_CharAi.m_OnlineUser == false)) // 硫?????��??�???�쭔 send ??
                            {
                                List<CharacterBase> kTargets = new List<CharacterBase>();
                                kTargets.Add(m_CharAi.GetCharacterBase().attackTarget);

                                NetworkManager.instance.networkSender.SendPcAttackReq(m_CharAi.GetCharacterBase().m_IngameObjectID,
                                                                                        m_CharAi.GetCharacterBase().m_currSkillIndex,
                                                                                        true,
                                                                                        m_CharAi.GetCharacterBase().m_MyObj.transform.position,
                                                                                        m_CharAi.GetCharacterBase().m_MyObj.transform.forward,
                                                                                        kTargets, 
                                                                                        0,
                                                                                        0);

                            }
                            break;
                    }                
                }
                break;
            case eNPpcActionType.eNORMALATTACK:
                m_ActionTypeAndInfo.Value[0] = (float)((NpcAI)m_CharAi).GetNextSkill_Index();
                goto case eNPpcActionType.eUSESKILL;
            case eNPpcActionType.eUSESKILL:
                {
                    if (m_CharAi.m_eType == eAIType.eNPC || m_CharAi.m_eType == eAIType.eHERONPC)
                    {
                       if( ( InGameManager.instance.m_bOffLine == true || InGameManager.instance.m_bHostPlay == true) && (m_CharAi.GetCharacterBase().m_BuffController.GetCharDisable(BuffController.eBuffCharDisable.eSKILL) > 0))
                        {
                            StopAction();
                            return;
                        }
                    }

                    m_CharAi.GetCharacterBase().m_currSkillIndex = (int)m_ActionTypeAndInfo.Value[0];

                    SkillDataInfo.SkillInfo kSkillInfo = m_CharAi.GetCharacterBase().m_AttackInfos[m_CharAi.GetCharacterBase().m_currSkillIndex].skillinfo;
                    NpcAction kAction = null;
                    bool bTracePosition = false;
                    Vector3 kVecMovePosition = Vector3.zero;
                    Vector3 kVecAreaPosition = Vector3.zero;
                    float fSkillRange = 0.0f;

                    switch (kSkillInfo.target_Type)
                    {
                        case eTargetType.eTARGET:
                        case eTargetType.eTARGET_AREA:
                            fSkillRange = kSkillInfo.skill_Dist;
                            break;
                        case eTargetType.eSELF_AREA:
                            fSkillRange = Mathf.Max(kSkillInfo.area_Size , kSkillInfo.skill_Dist);
                            break;
                        case eTargetType.eSELF:
                            fSkillRange = 100000.0f;
                            break;
                        case eTargetType.eAREA:
                            fSkillRange = kSkillInfo.skill_Dist;
                            if( kSkillInfo.action_Type != eSkillActionType.eMAGICCIRCLE )
                                m_CharAi.GetCharacterBase().attackPosition = (m_CharAi.GetCharacterBase().m_MyObj.forward * kSkillInfo.dist_Range) + m_CharAi.GetCharacterBase().m_MyObj.position;
                            break;
                    }

                    // Each Skill Type Action
                    switch ((PDT_TARGET_TYPE)m_TargetTypeAndInfo.Key)
                    {
                        case PDT_TARGET_TYPE.ePDT_TARGET_TYPE_NONE:
                            break;
                        case PDT_TARGET_TYPE.ePDT_TARGET_TYPE_GUARD_CURRENT:
                        case PDT_TARGET_TYPE.ePDT_TARGET_TYPE_CURRENT:
                            if (m_CharAi.m_eType == eAIType.eNPC)
                            {
                                if (m_CharAi.GetCharacterBase().attackTarget == null || m_CharAi.GetCharacterBase().attackTarget.m_CharState == CHAR_STATE.DEATH)
                                {
                                    eCharacter[] kCharClass = new eCharacter[] { eCharacter.eWarrior, eCharacter.eWizard, eCharacter.eArcher };
                                    //List<CharacterBase> kTarget = ((NpcAI)m_CharAi).FindTarget(eTargetState.eHOSTILE, kCharClass, 1);
                                    List<CharacterBase> kTarget = ((NpcAI)m_CharAi).FindTarget(eTargetState.eFRIENDLY, kCharClass, 1);
                                    if (kTarget.Count > 0)
                                    {
                                        m_CharAi.GetCharacterBase().SetAttackTarget(kTarget[0]);
                                    }
                                    else
                                    {
                                        StopAction();
                                        return;
                                    }
                                }
                            }

                            if (!m_CharAi.GetAvailableRange(fSkillRange))
                            {
								if (m_CharAi.m_eType == eAIType.eHERONPC)
								{
									if (m_CharAi.GetCharacterBase().attackTarget == null)
									{
										if (((HeroNpcAI)m_CharAi).Check_IsFindTargets() == false)
										{
											break;
										}
									}
								}

                                bTracePosition = true;
                                kVecMovePosition = new Vector3(m_CharAi.GetCharacterBase().attackTarget.transform.position.x, m_CharAi.GetCharacterBase().attackTarget.transform.position.y, m_CharAi.GetCharacterBase().attackTarget.transform.position.z);                            
                            }
                            break;
                        case PDT_TARGET_TYPE.ePDT_TARGET_TYPE_SKILLRULE:         // ??�궗 ???곕쫫... ( ??��?)

                            if ( InGameManager.instance.m_bOffLine == true || InGameManager.instance.m_bHostPlay == true )
                            {
                                if( m_CharAi.GetCharacterBase().m_CharacterType == eCharacter.eNPC )
                                {
                                    switch (kSkillInfo.target_Type)
                                    {
                                        case eTargetType.eTARGET:
                                        case eTargetType.eTARGET_AREA:
                                            List<CharacterBase> kAllTarget = new List<CharacterBase>();
                                            m_CharAi.GetCharacterBase().m_skSender.DoSkill(m_CharAi.GetCharacterBase(), m_CharAi.GetCharacterBase().GetAttackTarget(kSkillInfo.target_Type), kSkillInfo, 0, true);
                                            kAllTarget = m_CharAi.GetCharacterBase().m_skSender.GetTargetsInRange(kSkillInfo, kAllTarget, m_CharAi.GetCharacterBase().m_MyObj.position);
                                            if (kAllTarget.Count > 0)
                                                m_CharAi.GetCharacterBase().SetAttackTarget(kAllTarget[0]);
                                            break;
                                        case eTargetType.eSELF:
                                        case eTargetType.eSELF_AREA:
                                            m_CharAi.GetCharacterBase().SetAttackTarget(m_CharAi.GetCharacterBase());
                                            break;
                                        case eTargetType.eAREA:
                                            if( m_CharAi.GetCharacterBase().attackTarget == null )
                                                m_CharAi.GetCharacterBase().SetAttackTarget(m_CharAi.GetCharacterBase());
                                            break;
                                    }
                                }
                                else
                                {
                                    if (m_CharAi.GetCharacterBase().attackTarget == null)
                                        m_CharAi.GetCharacterBase().SetAttackTarget(m_CharAi.GetCharacterBase());
                                }
                            }
                            else
                            {
                                if (m_CharAi.GetCharacterBase().attackTarget == null)
                                    m_CharAi.GetCharacterBase().SetAttackTarget(m_CharAi.GetCharacterBase());
                            }

                            if (!m_CharAi.GetAvailableRange(fSkillRange))
                            {
                                bTracePosition = true;
                                kVecMovePosition = new Vector3(m_CharAi.GetCharacterBase().attackTarget.transform.position.x, m_CharAi.GetCharacterBase().attackTarget.transform.position.y, m_CharAi.GetCharacterBase().attackTarget.transform.position.z);
                            
                            }
                            break;
                        case PDT_TARGET_TYPE.ePDT_TARGET_TYPE_EOGEURORANKING: 
                            {
                                if (InGameManager.instance.m_bOffLine == true || InGameManager.instance.m_bHostPlay == true)
                                {
                                    eCharacter[] kCharClass = new eCharacter[] { eCharacter.eWarrior, eCharacter.eWizard, eCharacter.eArcher };
                                    List<CharacterBase> kTarget = m_CharAi.FindTarget(eTargetState.eFRIENDLY, kCharClass, (int)m_TargetTypeAndInfo.Value[0]);
                                    if (kTarget.Count > 0)
                                    {
                                        m_CharAi.GetCharacterBase().SetAttackTarget(kTarget[0]);
                                        m_TargetTypeAndInfo.Value[0] = 1.0f;
                                    }
                                    else  // skill rule
                                        m_CharAi.GetCharacterBase().SetAttackTarget(m_CharAi.GetCharacterBase().GetAttackTarget((eTargetType)kSkillInfo.target_Type));

                                    if (!m_CharAi.GetAvailableRange(fSkillRange))
                                    {
                                        bTracePosition = true;
                                        kVecMovePosition = new Vector3(m_CharAi.GetCharacterBase().attackTarget.transform.position.x, m_CharAi.GetCharacterBase().attackTarget.transform.position.y, m_CharAi.GetCharacterBase().attackTarget.transform.position.z);
                               
                                    }
                                }

                            }
                            break;
                        case PDT_TARGET_TYPE.ePDT_TARGET_TYPE_SELF:     
                            if (InGameManager.instance.m_bOffLine == true || InGameManager.instance.m_bHostPlay == true)
                                m_CharAi.GetCharacterBase().SetAttackTarget(m_CharAi.GetCharacterBase());
                            break;
                        case PDT_TARGET_TYPE.ePDT_TARGET_TYPE_NPC_OR_Object:     // Table ID NPC
                            {
                                if (InGameManager.instance.m_bOffLine == true || InGameManager.instance.m_bHostPlay == true)
                                {
                                    CharacterBase kTarget = NpcManager.instance.GetNpcByIndex((int)m_TargetTypeAndInfo.Value[0]);
                                    if (kTarget != null && kTarget.m_MyObj.gameObject.activeSelf && kTarget.m_CharState == CHAR_STATE.ALIVE)
                                    {
                                        m_CharAi.GetCharacterBase().SetAttackTarget(kTarget);
                                        if (!m_CharAi.GetAvailableRange(fSkillRange))
                                        {
                                            // 吏㏃????��?
                                            bTracePosition = true;
                                            kVecMovePosition = new Vector3(m_CharAi.GetCharacterBase().attackTarget.transform.position.x, m_CharAi.GetCharacterBase().attackTarget.transform.position.y, m_CharAi.GetCharacterBase().attackTarget.transform.position.z);
                                        }
                                    }
                                    else
                                    {
                                        StopAction();
                                        return;
                                    }
                                }
                            }
                            break;
                        case PDT_TARGET_TYPE.ePDT_TARGET_TYPE_CLASS:         
                            break;
                        case PDT_TARGET_TYPE.ePDT_TARGET_TYPE_RANDOM:        
                            if ((InGameManager.instance.m_bOffLine == true || InGameManager.instance.m_bHostPlay == true) && m_TargetTypeAndInfo.Value[0] == 0.0f) 
                            {
                                eCharacter[] kCharClass = new eCharacter[] { eCharacter.eWarrior, eCharacter.eWizard, eCharacter.eArcher };
                                //List<CharacterBase> kTarget = m_CharAi.FindTarget(eTargetState.eHOSTILE, kCharClass, 0);
                                List<CharacterBase> kTarget = m_CharAi.FindTarget(eTargetState.eFRIENDLY, kCharClass, 0);
                                if (kTarget.Count > 0)
                                {
                                    m_CharAi.GetCharacterBase().SetAttackTarget(kTarget[Random.Range(0, kTarget.Count)]);
                                    m_TargetTypeAndInfo.Value[0] = 1.0f;
                                }
                                // else none target Skill Shooting
                                if (!m_CharAi.GetAvailableRange(fSkillRange))
                                {
                                    // 吏㏃????��?
                                    bTracePosition = true;
                                    kVecMovePosition = new Vector3(m_CharAi.GetCharacterBase().attackTarget.transform.position.x, m_CharAi.GetCharacterBase().attackTarget.transform.position.y, m_CharAi.GetCharacterBase().attackTarget.transform.position.z);

                                }
                                else
                                    m_TargetTypeAndInfo.Value[0] = 0.0f;
                            }
                            break;
                        case PDT_TARGET_TYPE.ePDT_TARGET_TYPE_ALL_ENEMY:  
                                eCharacter[] kCharClass = new eCharacter[] { eCharacter.eWarrior, eCharacter.eWizard, eCharacter.eArcher };
                                m_CharAi.FindTarget(eTargetState.eFRIENDLY, kCharClass, 0);
                            }
                            break;
                        case PDT_TARGET_TYPE.ePDT_TARGET_TYPE_TERRAIN_POSITION:  
                            kVecAreaPosition = new Vector3(m_TargetTypeAndInfo.Value[0], m_TargetTypeAndInfo.Value[1], m_TargetTypeAndInfo.Value[2]);
                            break;
                        case PDT_TARGET_TYPE.ePDT_TARGET_TYPE_DIRECTION:      
                            break;
                        case PDT_TARGET_TYPE.ePDT_TARGET_TYPE_LONGEST: 
                            if (InGameManager.instance.m_bOffLine == true || InGameManager.instance.m_bHostPlay == true)
                            {
                                List<CharacterBase> kTarget = m_CharAi.FindTarget(eTargetState.eFRIENDLY, null, 0);
                                float fMinDist = 0.0f;
                                int iFindTargetIndex = -1;
                                Transform MyTransform = m_CharAi.GetCharacterBase().m_MyObj;
                                for (int i = 0; i < kTarget.Count; ++i )
                                {
                                    if (kTarget[i].m_CharState == CHAR_STATE.DEATH)
                                        continue;

                                    float fDist = (MyTransform.position - kTarget[i].m_MyObj.position).sqrMagnitude;
                                    if (fDist > fMinDist)
                                    {
                                        fMinDist = fDist;
                                        iFindTargetIndex = i;
                                    }
                                }
                                if (iFindTargetIndex >= 0)
                                    m_CharAi.GetCharacterBase().SetAttackTarget(kTarget[iFindTargetIndex]);
                                // else none target Skill Shooting
                                if (!m_CharAi.GetAvailableRange(fSkillRange))
                                {
                                    bTracePosition = true;
                                    kVecMovePosition = new Vector3(m_CharAi.GetCharacterBase().attackTarget.transform.position.x, m_CharAi.GetCharacterBase().attackTarget.transform.position.y, m_CharAi.GetCharacterBase().attackTarget.transform.position.z);
                                    m_TargetTypeAndInfo = new KeyValuePair<int, List<float>>((int)PDT_TARGET_TYPE.ePDT_TARGET_TYPE_CURRENT, m_TargetTypeAndInfo.Value);
                                }
                            }
                            break;
                    }
                    
                    if (bTracePosition && (InGameManager.instance.m_bOffLine || InGameManager.instance.m_bHostPlay == true) && (m_CharAi.m_eType == eAIType.eNPC || m_CharAi.m_eType == eAIType.eHERONPC))
                    {

						kAction = m_CharAi.m_Ai_ActionPool.ObjectNew();
						float[] fMovePos = new float[] { kVecMovePosition.x, kVecMovePosition.y, kVecMovePosition.z };
						float[] fTargetInfo = new float[] { fSkillRange };
						kAction.InitAction(m_CharAi, this.m_eActionGrade, (int)NpcAction.eNPpcActionType.eMOVE, fMovePos, (int)PDT_TARGET_TYPE.ePDT_TARGET_TYPE_CURRENT, fTargetInfo, true, this);
						m_CharAi.GetAIController().InsertFrontAction(this, kAction);
						m_CharAi.m_eMoveType = eMoveType.eRUN;
						return; 
					}
					if ( InGameManager.instance.m_bOffLine == true && m_CharAi.GetCharacterBase().attackTarget != m_CharAi.GetCharacterBase())
                    {
                        if (m_CharAi.m_eType == eAIType.eNPC || m_CharAi.m_eType == eAIType.eHERONPC)
                        {
                            m_CharAi.GetCharacterBase().SetAttackTarget(m_CharAi.GetCharacterBase().attackTarget);
                        }
                    }
                    if (m_CharAi.GetCharacterBase().m_CharacterType == eCharacter.eNPC || m_CharAi.GetCharacterBase().m_CharacterType == eCharacter.eHERONPC)
                    {
                        if ( kSkillInfo.action_Type == eSkillActionType.eAERO )
                        {
                            if (InGameManager.instance.m_bOffLine == true || InGameManager.instance.m_bHostPlay)
                            {
                                if( kVecAreaPosition != Vector3.zero )
                                {
                                    m_CharAi.GetCharacterBase().m_pushInfo.m_TargetPos = kVecAreaPosition;
                                }
                                else if (m_CharAi.GetCharacterBase().attackTarget != null)
                                {
                                    kVecAreaPosition = m_CharAi.GetCharacterBase().m_pushInfo.m_TargetPos = m_CharAi.GetCharacterBase().attackTarget.m_MyObj.transform.position;
                                }
                                m_CharAi.GetCharacterBase().attackPosition = kVecAreaPosition;
                            }
                        }
                    }
                    else
                    {
                    }

                    if (InGameManager.instance.m_bOffLine == false)
                        m_CharAi.m_eEndMotionState = MOTION_STATE.eMOTION_NONE;

                    if( (eNPpcActionType)m_ActionTypeAndInfo.Key == eNPpcActionType.eNORMALATTACK)
                    {
                        m_CharAi.GetCharacterBase().SetChangeMotionState(MOTION_STATE.eMOTION_NPCATTACK);
                    }
                    else
                    {
                        // Skill Shooting
                        m_CharAi.GetCharacterBase().SetChangeMotionState(MOTION_STATE.eMOTION_SKILL);
                    }

                    // net atk
                    if (InGameManager.instance.m_bHostPlay && (m_CharAi.m_eType == eAIType.eNPC || m_CharAi.m_eType == eAIType.eHERONPC))
                    {
                        List<CharacterBase> kTargets = new List<CharacterBase>();
                        kTargets.Add(m_CharAi.GetCharacterBase().attackTarget);

                        NetworkManager.instance.networkSender.SendPcActiveSkillReq(m_CharAi.GetCharacterBase().m_IngameObjectID,
                                                                                m_CharAi.GetCharacterBase().m_currSkillIndex,
                                                                                true,
                                                                                m_CharAi.GetCharacterBase().m_MyObj.transform.position,
                                                                                m_CharAi.GetCharacterBase().m_MyObj.transform.forward,
                                                                                kVecAreaPosition,
                                                                                kTargets,
                                                                                0);
                    }
                }
                break;
            case eNPpcActionType.eMOVE:

                switch (m_CharAi.m_eMoveType)
                {
                    case eMoveType.eWALK:
                        if (m_CharAi.GetCharacterBase().m_MotionState != MOTION_STATE.eMOTION_IDLE_WALK)
                            m_CharAi.GetCharacterBase().SetChangeMotionState(MOTION_STATE.eMOTION_IDLE_WALK);
                        break;
                    case eMoveType.eRUN:
                        if (m_CharAi.GetCharacterBase().m_MotionState != MOTION_STATE.eMOTION_TRACE)
                            m_CharAi.GetCharacterBase().SetChangeMotionState(MOTION_STATE.eMOTION_TRACE);
                        break;
                    case eMoveType.eSIDE:
                        Vector3 kDir = new Vector3(m_TargetTypeAndInfo.Value[0], 0 , 0 );
                        m_CharAi.GetCharacterBase().m_PanicBMove.m_Dir = kDir;
                        m_CharAi.GetCharacterBase().SetChangeMotionState(MOTION_STATE.eMOTION_SIDE_WALK);
                        m_CharAi.FreezeRotate(true);
                        break;
                    default:
                        if (m_CharAi.GetCharacterBase().m_MotionState != MOTION_STATE.eMOTION_TRACE)
                            m_CharAi.GetCharacterBase().SetChangeMotionState(MOTION_STATE.eMOTION_TRACE);
                        break;
                }
                if (m_CharAi.m_eType == eAIType.eNPC || m_CharAi.m_eType == eAIType.eHERONPC)
                {
                    switch ((PDT_TARGET_TYPE)m_TargetTypeAndInfo.Key)
                    {
                        case PDT_TARGET_TYPE.ePDT_TARGET_TYPE_CURRENT:
                        case PDT_TARGET_TYPE.ePDT_TARGET_TYPE_TERRAIN_POSITION:
                            m_CharAi.SetNavMeshobstacleAvoidanceType(ObstacleAvoidanceType.NoObstacleAvoidance);

                            break;
                        case PDT_TARGET_TYPE.ePDT_TARGET_TYPE_GUARD_CURRENT:
                            break;
                    }
                }

                if( InGameManager.instance.m_bOffLine == false)
                    m_CharAi.m_eEndMotionState = MOTION_STATE.eMOTION_NONE;
                //Debug.LogError("**** MOVE m_ActionTypeAndInfo.Value[" + 1 + "] ========= " + m_ActionTypeAndInfo.Value[1] + " ======== " + 0.5f);
                if(SceneManager.instance.GetCurGameMode() == SceneManager.eGAMEMODE.eMODE_CHAPTER)
                {
                    m_CharAi.SetNavMeshAgent(true, new Vector3(m_ActionTypeAndInfo.Value[0], 0.4f, m_ActionTypeAndInfo.Value[2]));
                }
                else
                {
                    m_CharAi.SetNavMeshAgent(true, new Vector3(m_ActionTypeAndInfo.Value[0], m_ActionTypeAndInfo.Value[1] + 0.5f, m_ActionTypeAndInfo.Value[2]));
                }

                if ( (m_CharAi.m_eType == eAIType.ePC && m_CharAi.m_OnlineUser == false))
                {
                    if ((m_CharAi.GetCharacterBase().charUniqID != PlayerManager.MYPLAYER_INDEX || m_CharAi.IsBattle() == true))
                    {
                        m_CharAi.SetNavMeshobstacleAvoidanceType(ObstacleAvoidanceType.NoObstacleAvoidance);
                    }
                }
                // net move
                if (InGameManager.instance.m_bOffLine == false)
                {
                    switch (m_CharAi.m_eType)
                    {
                        case eAIType.ePC:
                            if (m_CharAi.GetCharacterBase().charUniqID == PlayerManager.MYPLAYER_INDEX || m_CharAi.m_OnlineUser == false )
                                NetworkManager.instance.networkSender.SendPcMoveReq(m_CharAi.GetCharacterBase().m_IngameObjectID,
                                                                                      m_CharAi.GetCharacterBase().m_MyObj.transform.position,
                                                                                      m_CharAi.GetNavMeshAgentDest(),
                                                                                      m_CharAi.GetNavMeshAgentSpeed(),
                                                                                      (int)m_CharAi.m_eMoveType);
                            break;
                        case eAIType.eHERONPC:
                        case eAIType.eNPC:
                            if (InGameManager.instance.m_bHostPlay)
                                NetworkManager.instance.networkSender.SendPcMoveReq(m_CharAi.GetCharacterBase().m_IngameObjectID,
                                                                                      m_CharAi.GetCharacterBase().m_MyObj.transform.position,
                                                                                      m_CharAi.GetNavMeshAgentDest(),
                                                                                      m_CharAi.GetNavMeshAgentSpeed(),
                                                                                      (int)m_CharAi.m_eMoveType);
                            break;
                    }
                }
                break;
            case eNPpcActionType.eCINEMA:
                break;
            case eNPpcActionType.eCHANGEPAGE:
                ((NpcAI)m_CharAi).m_NpcAI_Pattern.JumpPageStart((int)m_ActionTypeAndInfo.Value[0]);
                break;
            case eNPpcActionType.eETC:
                m_CharAi.GetCharacterBase().SetETCMotion(ObjectState_ETC.eSTATE.CHANGESTANCE);
                break;
            case eNPpcActionType.eBEATTACKTED:

                m_CharAi.GetCharacterBase().SetChangeMotionState(MOTION_STATE.eMOTION_BEATTACKED);
                break;
        }

        m_eNpcActionState = eNpcActionState.ePLAY;

        m_CharAi.OnEvent(this);

        m_WaitFreezeSolveTime = FREEZE_TIME;
    }

    public void UpdateAction()
    {
        float fMoveDelay = NetworkManager.NPC_MOVE_DELAY;

        if (InGameManager.instance.m_bOffLine == false)
            fMoveDelay = fMoveDelay * 2f;
        
        switch ((eNPpcActionType)m_ActionTypeAndInfo.Key)
        {
            case eNPpcActionType.eNORMALATTACK:
            case eNPpcActionType.eUSEBASEATTACK:
                switch (m_CharAi.m_eType)
                {
                    case eAIType.eHERONPC:
                    case eAIType.eNPC:
                        if (m_CharAi.m_eEndMotionState == MOTION_STATE.eMOTION_NPCATTACK)
                        {
                            m_CharAi.m_eEndMotionState = MOTION_STATE.eMOTION_NONE;
                            StopAction();
                        }
                        break;
                    case eAIType.ePC:
                        if (m_CharAi.m_eEndMotionState == MOTION_STATE.eMOTION_NPCATTACK)
                        {
                            m_CharAi.m_eEndMotionState = MOTION_STATE.eMOTION_NONE;
                            StopAction();
                        }
                        break;
                }
                break;
            case eNPpcActionType.eUSESKILL:
                if (m_CharAi.m_eEndMotionState == MOTION_STATE.eMOTION_SKILL)
                {
                    m_CharAi.m_eEndMotionState = MOTION_STATE.eMOTION_NONE;
                    StopAction();
                }
                break;
            case eNPpcActionType.eMOVE:
                if (m_CharAi.GetCharacterBase().m_CharacterType == eCharacter.eWarrior || m_CharAi.GetCharacterBase().m_CharacterType == eCharacter.eWizard || m_CharAi.GetCharacterBase().m_CharacterType == eCharacter.eHERONPC)
                {
                    m_CharAi.SetNavMeshavoidancePriority(0);
                }
                float fRadius = m_TargetTypeAndInfo.Value[0];
                Vector2 kCharPos = new Vector2(m_CharAi.GetCharacterBase().transform.position.x, m_CharAi.GetCharacterBase().transform.position.z);

                switch ((PDT_TARGET_TYPE)m_TargetTypeAndInfo.Key)
                {
                    case PDT_TARGET_TYPE.ePDT_TARGET_TYPE_NONE:           
                        break;
                    case PDT_TARGET_TYPE.ePDT_TARGET_TYPE_CURRENT:
                        if (m_WaitFreezeSolveTime < FREEZE_TIME - fMoveDelay && m_CharAi.GetCharacterBase().attackTarget != null) 
                        {
                            bool bReTrace = true;
                            if (m_ActionTypeAndInfo.Value[0] == m_CharAi.GetCharacterBase().attackTarget.transform.position.x &&
                                m_ActionTypeAndInfo.Value[2] == m_CharAi.GetCharacterBase().attackTarget.transform.position.z)
                                bReTrace = false;

                            if (bReTrace)
                            {
                                Transform TargetTransfrom = m_CharAi.GetCharacterBase().attackTarget.transform;
                                Vector2 kTargetPos = new Vector2(TargetTransfrom.position.x, TargetTransfrom.position.z);
                                m_ActionTypeAndInfo.Value[0] = TargetTransfrom.position.x;
//                                m_ActionTypeAndInfo.Value[1] = TargetTransfrom.position.y;
                                m_ActionTypeAndInfo.Value[2] = TargetTransfrom.position.z;

                                Vector2 kDir = (kTargetPos - kCharPos).normalized;
                                kDir *= (m_CharAi.GetCharacterBase().GetRadius() + m_CharAi.GetCharacterBase().attackTarget.GetRadius() );
                                m_CharAi.SetNavMeshAgent(true, (TargetTransfrom.position));
                                // net move
                                if (InGameManager.instance.m_bOffLine == false)
                                {
                                    switch (m_CharAi.m_eType)
                                    {
                                        case eAIType.ePC:
                                            if (m_CharAi.GetCharacterBase().charUniqID == PlayerManager.MYPLAYER_INDEX || m_CharAi.m_OnlineUser == false)
                                                NetworkManager.instance.networkSender.SendPcMoveReq(m_CharAi.GetCharacterBase().m_IngameObjectID,
                                                                                                        m_CharAi.GetCharacterBase().m_MyObj.transform.position,
                                                                                                        m_CharAi.GetNavMeshAgentDest(),
                                                                                                        m_CharAi.GetNavMeshAgentSpeed(),
                                                                                                        (int)m_CharAi.m_eMoveType);
                                            break;
                                        case eAIType.eNPC:
                                            if (InGameManager.instance.m_bHostPlay)
                                            {
                                                NetworkManager.instance.networkSender.SendPcMoveReq(m_CharAi.GetCharacterBase().m_IngameObjectID,
                                                                                                        m_CharAi.GetCharacterBase().m_MyObj.transform.position,
                                                                                                        m_CharAi.GetNavMeshAgentDest(),
                                                                                                        m_CharAi.GetNavMeshAgentSpeed(),
                                                                                                        (int)m_CharAi.m_eMoveType);
                                            }
                                            break;
                                    }
                                }
                            }
                            fRadius += m_CharAi.GetCharacterBase().attackTarget.GetRadius();

                            m_WaitFreezeSolveTime = FREEZE_TIME;
                        }
                        break;
                    case PDT_TARGET_TYPE.ePDT_TARGET_TYPE_DIRECTION:
                        if (m_WaitFreezeSolveTime < FREEZE_TIME - fMoveDelay && m_CharAi.GetCharacterBase().attackTarget != null) // 
                        {
                            m_CharAi.GetCharacterBase().SetAttackTarget(m_CharAi.GetCharacterBase().attackTarget);
                            m_WaitFreezeSolveTime = FREEZE_TIME;
                        }
                        break;
                    case  PDT_TARGET_TYPE.ePDT_TARGET_TYPE_GUARD_CURRENT:
                        {
                            if (m_WaitFreezeSolveTime < FREEZE_TIME - fMoveDelay && m_CharAi.GetCharacterBase().attackTarget != null) // 
                            {
                                float ftempDist = (m_CharAi.GetCharacterBase().m_MyObj.transform.position - m_CharAi.GetCharacterBase().attackTarget.m_MyObj.transform.position).sqrMagnitude;
                                if (ftempDist < m_TargetTypeAndInfo.Value[1] * m_TargetTypeAndInfo.Value[1])
                                    fRadius = float.MaxValue;
                                m_WaitFreezeSolveTime = FREEZE_TIME;
                            }
                        }
                        break;
                }
                float fDistance = Vector2.Distance(kCharPos, new Vector2(m_ActionTypeAndInfo.Value[0], m_ActionTypeAndInfo.Value[2]));
                
                if (m_CharAi.GetCharacterBase().m_Collider != null)
                {
                    fDistance -= m_CharAi.GetCharacterBase().GetRadius();
                }
                else
                {
                    if (fRadius == 0.0f)
                        fRadius += m_CharAi.GetCharacterBase().GetRadius();
                }
                fDistance -= fRadius;

                if (fDistance <= 0.0f || m_CharAi.HasPath() == false || m_CharAi.NavPathState() != NavMeshPathStatus.PathComplete)
                {
                    StopAction();
                }

                break;
            case eNPpcActionType.eCINEMA:
                break;
            case eNPpcActionType.eCHANGEPAGE:
                StopAction();
                break;
            case eNPpcActionType.eETC:
                if (m_CharAi.m_eEndMotionState == MOTION_STATE.eMOTION_ETC)
                {
                    m_CharAi.m_eEndMotionState = MOTION_STATE.eMOTION_NONE;
                    StopAction();
                }
                break;
            case eNPpcActionType.eBEATTACKTED:
                if (m_CharAi.m_eEndMotionState == MOTION_STATE.eMOTION_BEATTACKED)
                {
                    m_CharAi.m_eEndMotionState = MOTION_STATE.eMOTION_NONE;
                    StopAction();
                }
                break;
        }
        if (m_CharAi.GetCharacterBase().charUniqID == PlayerManager.MYPLAYER_INDEX || m_CharAi.m_OnlineUser != true || m_CharAi.m_eType == eAIType.eNPC || m_CharAi.m_eType == eAIType.eHERONPC)
        {
            m_WaitFreezeSolveTime -= Time.deltaTime;
            if (m_WaitFreezeSolveTime < 0.0f)
            {
                // Freeze Solve
                StopAction();
            }
        }

    }

    public void StopAction()
    {
        if ( !m_bDontStopAction )
        {
            switch ((eNPpcActionType)m_ActionTypeAndInfo.Key)
            {
                case eNPpcActionType.eUSEBASEATTACK:
                    break;
                case eNPpcActionType.eETC:
                    break;
                case eNPpcActionType.eMOVE:
                    if (m_CharAi.GetCharacterBase().charUniqID == PlayerManager.MYPLAYER_INDEX )
					{
                        m_CharAi.SetNavMeshobstacleAvoidanceType(ObstacleAvoidanceType.NoObstacleAvoidance);
                    }

					m_CharAi.SetNavMeshAgent(false);

                    m_CharAi.FreezeRotate(false);
                    // net move
                    if (InGameManager.instance.m_bOffLine == false)
                    {
                        switch (m_CharAi.m_eType)
                        {
                            case eAIType.ePC:
                                if( m_CharAi.GetCharacterBase().charUniqID == PlayerManager.MYPLAYER_INDEX )
                                {
                                    m_CharAi.GetCharacterBase().m_charController.m_DpadMP.StopRun(eMoveType.eNONE);
                                }
                                break;
                            case eAIType.eNPC:
                                if (InGameManager.instance.m_bHostPlay)
                                {
                                    NetworkManager.instance.networkSender.SendPcMoveReq(m_CharAi.GetCharacterBase().m_IngameObjectID,
                                                                                          m_CharAi.GetCharacterBase().m_MyObj.transform.forward,
                                                                                          m_CharAi.GetCharacterBase().m_MyObj.transform.position,
                                                                                          m_CharAi.GetNavMeshAgentSpeed(),
                                                                                          (int)eMoveType.eNONE);
                                }
                                break;
                        }
                    }

                    break;
                case eNPpcActionType.eCINEMA:
                    break;
                case eNPpcActionType.eCHANGEPAGE:
                    break;
                case eNPpcActionType.eBEATTACKTED:
                    break;
            }
        }
        m_bDontStopAction = false;
        m_eNpcActionState = eNpcActionState.eEND;

        m_CharAi.OnEvent(this);
    }

    public bool CancelAction()
    {
        if (m_bCancelAble)
        {
            StopAction();

            switch (m_CharAi.m_eType)
            {


                case eAIType.eNPC:
                    if( ((NpcAI)m_CharAi).m_NpcAI_Pattern != null )
                    {
                        ((NpcAI)m_CharAi).m_NpcAI_Pattern.CancelPattern(this);
                    }
                    break;
                case eAIType.eHERONPC:
                    if (((HeroNpcAI)m_CharAi).m_NpcAI_Pattern != null)
                    {
                        ((HeroNpcAI)m_CharAi).m_NpcAI_Pattern.CancelPattern(this);
                    }
                    break;

                case eAIType.ePC:
                    break;
            }
        }
        return m_bCancelAble;
    }

    public bool ChangeAction(NpcAction p_Action)
    {
        switch ((eNPpcActionType)m_ActionTypeAndInfo.Key)
        {
            case eNPpcActionType.eNORMALATTACK:
            case eNPpcActionType.eUSEBASEATTACK:
                {
                    bool bResult = false;
        
                    switch ((eREACTION_TYPE)(int)p_Action.m_ActionTypeAndInfo.Value[0])
                    {
                        case eREACTION_TYPE.ePHYGICAL_DAMAGE:
                            bResult = m_CharAi.GetCharacterBase().m_AttackInfos[(int)m_ActionTypeAndInfo.Value[0]].skillinfo.hit_Cancel == 0 /*&& m_CharAi.GetCharacterBase().animState <= ANIMATION_STATE.eANIM_LOOP*/;
                            break;
                        default:
                            bResult = true;
                            break;
                    }
                    if (!bResult && m_CharAi.m_eEndMotionState == MOTION_STATE.eMOTION_NPCATTACK) // Update 
                        bResult = true;

                    return bResult;
                }
            case eNPpcActionType.eUSESKILL:
                {
                    bool bResult = false;

                    switch ((eREACTION_TYPE)(int)p_Action.m_ActionTypeAndInfo.Value[0])
                    {
                        case eREACTION_TYPE.ePHYGICAL_DAMAGE:
                            bResult = m_CharAi.GetCharacterBase().m_AttackInfos[(int)m_ActionTypeAndInfo.Value[0]].skillinfo.hit_Cancel == 0 /*&& m_CharAi.GetCharacterBase().animState <= ANIMATION_STATE.eANIM_LOOP*/;
                            break;
                        case eREACTION_TYPE.eSTUNNED:
                        case eREACTION_TYPE.ePANIC:
                            bResult = false;
                            break;
                        default:
                            bResult = true;
                            break;
                    }
                    if (!bResult && m_CharAi.m_eEndMotionState == MOTION_STATE.eMOTION_SKILL) // Update 
                        bResult = true;

                    return bResult;
                }
                //break;
            case eNPpcActionType.eMOVE:
                if( (eNPpcActionType)p_Action.m_ActionTypeAndInfo.Key == eNPpcActionType.eMOVE)
                {
                    switch ((PDT_TARGET_TYPE)p_Action.m_TargetTypeAndInfo.Key)
                    {
                        case PDT_TARGET_TYPE.ePDT_TARGET_TYPE_NONE:     
                            m_bDontStopAction = true;
                            break;
                        case PDT_TARGET_TYPE.ePDT_TARGET_TYPE_CURRENT:
                            break;
                    }
                }

                return m_bCancelAble;
                //break;
            case eNPpcActionType.eCINEMA:
                break;
            case eNPpcActionType.eCHANGEPAGE:
                break;
            case eNPpcActionType.eETC:
                return true;
            case eNPpcActionType.eBEATTACKTED:
                if (p_Action.m_ActionTypeAndInfo.Key == (int)eNPpcActionType.eBEATTACKTED)
                {
                    switch (SkillDataManager.instance.GetReactionType(m_CharAi.GetCharacterBase().m_ReactionBuffData.m_AffectCode))
                    {
                        case eREACTION_TYPE.ePHYGICAL_DAMAGE:
                            switch ((eREACTION_TYPE)(int)p_Action.m_ActionTypeAndInfo.Value[0])
                            {
                                case eREACTION_TYPE.eKNOCKBACK:
                                case eREACTION_TYPE.eKNOCKBACK_INT_2:
                                case eREACTION_TYPE.eKNOCKDOWN:
                                case eREACTION_TYPE.eAIRBONE:
                                case eREACTION_TYPE.ePULLING:
                                case eREACTION_TYPE.ePHYGICAL_DAMAGE:
                                case eREACTION_TYPE.ePUSHING:
                                case eREACTION_TYPE.eSCREW:
                                case eREACTION_TYPE.eFROZEN:
                                    return true;
                            }
                            break;
                        case eREACTION_TYPE.eSTUNNED:
                        case eREACTION_TYPE.ePANIC:
                            return true;
                        case eREACTION_TYPE.eKNOCKBACK:
                            switch ((eREACTION_TYPE)(int)p_Action.m_ActionTypeAndInfo.Value[0])
                            {
                                case eREACTION_TYPE.eKNOCKDOWN:
                                case eREACTION_TYPE.eAIRBONE:
                                case eREACTION_TYPE.ePULLING:
                                case eREACTION_TYPE.eSTANDUP:
                                    return true;
                            }
                            break;
                        case eREACTION_TYPE.eKNOCKBACK_INT_2:
                            switch ((eREACTION_TYPE)(int)p_Action.m_ActionTypeAndInfo.Value[0])
                            {
                                case eREACTION_TYPE.ePULLING:
                                    return true;
                            }
                            break;
                        case eREACTION_TYPE.eKNOCKDOWN:
                            switch ((eREACTION_TYPE)(int)p_Action.m_ActionTypeAndInfo.Value[0])
                            {
                                case eREACTION_TYPE.eAIRBONE:
                                case eREACTION_TYPE.ePULLING:
                                case eREACTION_TYPE.eSTANDUP:
                                    return true;
                            }
                            break;
                        case eREACTION_TYPE.eAIRBONE:
                            switch ((eREACTION_TYPE)(int)p_Action.m_ActionTypeAndInfo.Value[0])
                            {
                                case eREACTION_TYPE.ePULLING:
                                case eREACTION_TYPE.eSTANDUP:
                                    return true;
                            }
                            break;
                        case eREACTION_TYPE.ePULLING:
                            switch ((eREACTION_TYPE)(int)p_Action.m_ActionTypeAndInfo.Value[0])
                            {
                                case eREACTION_TYPE.eKNOCKBACK:
                                case eREACTION_TYPE.eKNOCKBACK_INT_2:
                                case eREACTION_TYPE.eSTANDUP:
                                    return true;
                            }
                            break;
                        case eREACTION_TYPE.ePUSHING:
                            switch ((eREACTION_TYPE)(int)p_Action.m_ActionTypeAndInfo.Value[0])
                            {
                                case eREACTION_TYPE.ePUSHING:
                                    return false;
                                default:
                                    return true;
                            }
                        case eREACTION_TYPE.eSCREW:
                            if (m_eNpcActionState == eNpcActionState.eEND)
                                return true;

                            return false;
                        case eREACTION_TYPE.eGUARDBREAK:
                            switch ((eREACTION_TYPE)(int)p_Action.m_ActionTypeAndInfo.Value[0])
                            {
                                case eREACTION_TYPE.ePHYGICAL_DAMAGE:
                                    return false;
                                default:
                                    return true;
                            }
                        case eREACTION_TYPE.eFROZEN:
                            switch ((eREACTION_TYPE)(int)p_Action.m_ActionTypeAndInfo.Value[0])
                            {
                                case eREACTION_TYPE.eFROZEN:
                                    return true;
                            }
                            if (m_eNpcActionState == eNpcActionState.eEND)
                            {
                                return true;
                            }
                            return false;
                        case eREACTION_TYPE.eSTANDUP:
                            switch ((eREACTION_TYPE)(int)p_Action.m_ActionTypeAndInfo.Value[0])
                            {
                                case eREACTION_TYPE.eKNOCKBACK:
                                case eREACTION_TYPE.eKNOCKBACK_INT_2:
                                case eREACTION_TYPE.eKNOCKDOWN:
                                case eREACTION_TYPE.eAIRBONE:
                                case eREACTION_TYPE.ePULLING:
                                case eREACTION_TYPE.eSCREW:
                                case eREACTION_TYPE.eFROZEN:
                                    return true;
                            }
                            break;
                    }
                }
                else if( InGameManager.instance.m_bOffLine == false)
                {
                    if( m_eNpcActionState == eNpcActionState.eEND )
                    {
                        return true;
                    }
                    else
                    {
                        switch (SkillDataManager.instance.GetReactionType(m_CharAi.GetCharacterBase().m_ReactionBuffData.m_AffectCode))
                        {
                            case eREACTION_TYPE.eSTUNNED:
                            case eREACTION_TYPE.ePANIC:
                                return true;
                        }
                    }
                }
                break;
        }
        return false;
    }

    public bool CheckActionAddAble(NpcAction p_Action)
    {
        bool kReulst = false;
        // grade
        if ((int)m_eActionGrade < (int)p_Action.m_eActionGrade)
        {
            // state
            if (m_eNpcActionState != eNpcActionState.ePLAY)
            {
                kReulst = true;
            }
        }

//        if (kReulst)
        {
            if (m_bCancelAble)
            {
                CancelAction();
                kReulst = false;
            }
        }
        return kReulst;
    }
}
////// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// 

public class NpcActionController
{
    //      NpcAction.eNpcActionGrade.
    private int[] m_iActionGradeCount = new int[(int)NpcAction.eNpcActionGrade.eMAX_GRADE];
    public AI m_NpcAI = null;

    private NpcActionController()
    {

    }
    public NpcActionController(AI p_NpcAI)
    {
        m_NpcAI = p_NpcAI;
    }
    public void Init()
    {
        System.Array.Clear(m_iActionGradeCount, 0, (int)NpcAction.eNpcActionGrade.eMAX_GRADE);
    }
    public void AddAction(NpcAction p_newAction)
    {
        ++m_iActionGradeCount[(int)p_newAction.m_eActionGrade];

        for (int i = 0; i < m_ActionQueue.Count; ++i)
        {
            //check for intercept 
            if (m_ActionQueue[i].CheckActionAddAble(p_newAction))
            {
                m_ActionQueue.Insert(i, p_newAction);
                return;
            }
        }
        // add bottom
        //if (m_NpcAI.GetCharacterBase().m_CharacterType == eCharacter.eWarrior)
        //{
        //    Debug.Log("Why??????????????????????");
        //}
        m_ActionQueue.Add(p_newAction);
    }
    public void InsertFrontAction(NpcAction p_TargetAction, NpcAction p_newAction)
    {
        ++m_iActionGradeCount[(int)p_newAction.m_eActionGrade];

        for (int i = 0; i < m_ActionQueue.Count; ++i)
        {
            // Insert
            if (m_ActionQueue[i] == p_TargetAction)
            {
                m_ActionQueue.Insert(i, p_newAction);
                return;
            }
        }
        // add bottom
        //if (m_NpcAI.GetCharacterBase().m_CharacterType == eCharacter.eWarrior)
        //{
        //    Debug.Log("Why??????????????????????");
        //}
        // add bottom
        m_ActionQueue.Add(p_newAction);
    }
    public void SuperCancelAction(NpcAction p_newAction)
    {
        ++m_iActionGradeCount[(int)p_newAction.m_eActionGrade];

        for (int i = 0; i < m_ActionQueue.Count; ++i)
        {
            if (m_ActionQueue[i].m_eActionGrade == NpcAction.eNpcActionGrade.eBASE && m_ActionQueue[i].m_ActionTypeAndInfo.Key != (int)NpcAction.eNPpcActionType.eBEATTACKTED)
            {
                m_ActionQueue[i].StopAction();
                m_ActionQueue.Insert(i, p_newAction);
                return;
            }
            else
            {
                if( m_ActionQueue[i].m_eNpcActionState == NpcAction.eNpcActionState.ePLAY )
                {
                    if (m_ActionQueue.Count > i + 1)
                    {
                        m_ActionQueue.Insert(i + 1, p_newAction);
                    }
                    else
                    {
                        m_ActionQueue.Add(p_newAction);
                    }
                    return;
                }
                else
                {
                    m_ActionQueue[i].StopAction();
                    m_ActionQueue.Insert(i, p_newAction);
                    return;
                }
            }
        }
        // add bottom
        //if (m_NpcAI.GetCharacterBase().m_CharacterType == eCharacter.eWarrior)
        //{
        //    Debug.Log("Why??????????????????????");
        //}
        m_ActionQueue.Add(p_newAction);
    }
    public void RemoveAction(NpcAction p_delAction)
    {
        for (int i = 0; i < m_ActionQueue.Count; i++)
        {
            if (m_ActionQueue[i] == p_delAction)
            {
                m_ActionQueue.Remove(m_ActionQueue[i]);
                --m_iActionGradeCount[(int)p_delAction.m_eActionGrade];
                break;
            }
        }
    }

    public void UpdateActions()
    {
        if (m_ActionQueue.Count > 0)
        {
            //if (m_NpcAI.GetCharacterBase().m_CharacterType == eCharacter.eWarrior)
            //{
            //    Debug.Log("Why??????????????????????");
            //}
            if (m_ActionQueue[0].m_eNpcActionState == NpcAction.eNpcActionState.eREADY)
            {
                m_ActionQueue[0].PlayAction();
            }
            if (m_ActionQueue[0].m_eNpcActionState == NpcAction.eNpcActionState.ePLAY)
            {
                m_ActionQueue[0].UpdateAction();
            }
            if (m_ActionQueue[0].m_eNpcActionState == NpcAction.eNpcActionState.eEND)
            {
                //NpcAction.eNPpcActionType LastActionType = (NpcAction.eNPpcActionType)m_ActionQueue[0].m_ActionTypeAndInfo.Key;

                --m_iActionGradeCount[(int)m_ActionQueue[0].m_eActionGrade];
                m_NpcAI.m_Ai_ActionPool.ObjectDelete(m_ActionQueue[0]);

                m_ActionQueue.RemoveAt(0);

                if (m_ActionQueue.Count == 0)
                {
                    if (m_NpcAI.GetCharacterBase().m_CharState == CHAR_STATE.ALIVE)
                    {
                        m_NpcAI.GetCharacterBase().SetChangeMotionState(MOTION_STATE.eMOTION_IDLE);
                    }
                    else if (m_NpcAI.GetCharacterBase().m_CharState != CHAR_STATE.ALIVE && m_NpcAI.m_eType == eAIType.ePC && m_NpcAI.GetCharacterBase().m_MotionState != MOTION_STATE.eMOTION_DIE)
                    {
                        m_NpcAI.GetCharacterBase().SetChangeMotionState(MOTION_STATE.eMOTION_DIE);
                    }

                    if (m_NpcAI.m_eType == eAIType.eNPC && ((NpcAI)m_NpcAI).m_bNpcNetworkController == true)
                    {
						if (m_NpcAI.GetCharacterBase().m_CharState == CHAR_STATE.ALIVE)
						{
							if (((NpcAI)m_NpcAI).GetNpcState() != NpcAI.eNpcState.ePEACE)
								((NpcAI)m_NpcAI).SetPeaceState();
							((NpcAI)m_NpcAI).m_bNpcNetworkController = false;
						}
                    }
                }
            }
        }
    }

    public void UpdateExtraActions()
    {
        if (m_ActionQueue.Count > 0)
        {
            if (m_ActionQueue[0].m_eNpcActionState == NpcAction.eNpcActionState.eREADY)
                m_ActionQueue[0].PlayAction();
            if (m_ActionQueue.Count <= 0)
                return;
            if (m_ActionQueue[0].m_eNpcActionState == NpcAction.eNpcActionState.ePLAY)
                m_ActionQueue[0].UpdateAction();
            if (m_ActionQueue.Count <= 0)
                return;
            if (m_ActionQueue[0].m_eNpcActionState == NpcAction.eNpcActionState.eEND)
            {
                //NpcAction.eNPpcActionType LastActionType = (NpcAction.eNPpcActionType)m_ActionQueue[0].m_ActionTypeAndInfo.Key;

                --m_iActionGradeCount[(int)m_ActionQueue[0].m_eActionGrade];
                m_NpcAI.m_Ai_ActionPool.ObjectDelete(m_ActionQueue[0]);

                m_ActionQueue.RemoveAt(0);

                if (m_ActionQueue.Count == 0)
                {
                    if (m_NpcAI.GetCharacterBase().m_CharState == CHAR_STATE.ALIVE)
                    {
                        m_NpcAI.GetCharacterBase().SetChangeMotionState(MOTION_STATE.eMOTION_BEATTACKED);
                    }
                    else if (m_NpcAI.m_eType == eAIType.ePC && m_NpcAI.GetCharacterBase().m_MotionState != MOTION_STATE.eMOTION_BEATTACKED)
                    {
                        m_NpcAI.GetCharacterBase().SetChangeMotionState(MOTION_STATE.eMOTION_BEATTACKED);
                    }
                }
            }
        }
    }

    public void RemoveAllAction()
    {
        if(m_ActionQueue != null)
        {
            for (int i = 0; i < m_ActionQueue.Count; i++)
            {
                m_ActionQueue[i].StopAction();
                m_NpcAI.m_Ai_ActionPool.ObjectDelete(m_ActionQueue[i]);
            }
            //zunghoon Modefy ENd 

            m_ActionQueue.Clear();
        }

        Init();
    }

    public bool ChangeActionOrSkip(NpcAction p_newAction)
    {
        if (m_ActionQueue.Count > 0)
        {
            if (m_ActionQueue[0].ChangeAction(p_newAction))
            {
                m_ActionQueue[0].m_bCancelAble = true;
                m_ActionQueue[0].CancelAction();
                p_newAction.m_eActionGrade = m_ActionQueue[0].m_eActionGrade;
                m_NpcAI.m_Ai_ActionPool.ObjectDelete(m_ActionQueue[0]);

				//m_ActionQueue[0] = p_newAction;
				//m_ActionQueue[0].m_bCancelAble = p_newAction.m_bCancelAble;
				//m_NpcAI.m_eEndMotionState = MOTION_STATE.eMOTION_NONE;
				if ((NpcAction.eNPpcActionType)p_newAction.m_ActionTypeAndInfo.Key == NpcAction.eNPpcActionType.eUSESKILL)
				{
					m_ActionQueue[0] = p_newAction;
					m_ActionQueue[0].m_bCancelAble = p_newAction.m_bCancelAble;
					m_NpcAI.m_eEndMotionState = MOTION_STATE.eMOTION_NONE;
				}
				else if ((NpcAction.eNPpcActionType)p_newAction.m_ActionTypeAndInfo.Key == NpcAction.eNPpcActionType.eBEATTACKTED)
				{
					m_ActionQueue[0] = p_newAction;
					m_ActionQueue[0].m_bCancelAble = p_newAction.m_bCancelAble;
					m_NpcAI.m_eEndMotionState = MOTION_STATE.eMOTION_NONE;
				}
				return true;
            }
        }
        else
        {
            AddAction(p_newAction);
            return true;
        }
        return false;
    }

    public int GetCountActionGrade(NpcAction.eNpcActionGrade kFindGrade)
    {
        return m_iActionGradeCount[(int)kFindGrade];
    }

    public bool ImmediatlyPlay()
    {
        if (m_ActionQueue.Count > 0)
        {
            if (m_ActionQueue[0].m_eNpcActionState == NpcAction.eNpcActionState.eREADY )
            {
                m_ActionQueue[0].PlayAction();
                return true;
            }
        }
        return false;
    }
    public NpcAction.eNpcActionState GetCurActionState()
    {
        if (m_ActionQueue.Count > 0)
        {
            return m_ActionQueue[0].m_eNpcActionState;
        }
        return NpcAction.eNpcActionState.eEND;
    }
    public bool BreakCurAction( NpcAction.eNPpcActionType p_ActionType )
    {
        if (m_ActionQueue.Count > 0)
        {
            if( m_ActionQueue[0].m_ActionTypeAndInfo.Key == (int)p_ActionType)
            {
                m_ActionQueue[0].StopAction();
                return true;
            }
        }
        return false;
    }
    public bool BreakOtherAction()
    {
        for (int i = 1; i < m_ActionQueue.Count; ++i)
        {
            // Insert
            m_ActionQueue[i].CancelAction();
        }
        return false;
    }
    public bool IsCurActionCancelAble()
    {
        if (m_ActionQueue.Count > 0)
        {
            return m_ActionQueue[0].m_bCancelAble;
        }
        return true;
    }
    public int GetActionCount()
    {
        if (m_ActionQueue.Count > 0)
        {
            return m_ActionQueue.Count;
        }
        return 0;
    }
    public NpcAction GetAction()
    {
        if (m_ActionQueue.Count > 0)
        {
            return m_ActionQueue[0];
        }
        return null;
    }
    public void ChangeActionTargetType( int p_TargetType, float[] p_Target_Info)
    {
        if (m_ActionQueue.Count > 0)
        {
            List<float> tmpInfo2 = new List<float>();
            for (int j = 0; j < p_Target_Info.Length; ++j)
                tmpInfo2.Add(p_Target_Info[j]);

            m_ActionQueue[0].m_TargetTypeAndInfo = new KeyValuePair<int, List<float>>(p_TargetType, tmpInfo2);
        }
    }

    private List<NpcAction> m_ActionQueue = new List<NpcAction>();
}