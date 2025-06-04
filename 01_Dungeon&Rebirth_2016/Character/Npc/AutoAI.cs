using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KSPlugins;

public class AutoAI : AI
{
    public enum eAutoState : byte
    {
        ePEACE = 0,
        eGUARD,
        eGUARD_TRACE,
        eGUARD_MOVING,
        eATTACK_READY,
        eATTACK_READY_TRACE,
        eATTACK_READY_COMPLETE,
        eATTACK,
        eSKILL,
        eBEATTACKED,
        eMOVE,
        eDIE,
        eCHANGESTANCE,
        eNONE
    }

    public enum eAutoProcessState : byte
    {
        eNone=0,
        eReady,
        ePlay,
        eEnding,
    }
    public enum eSTANCE
    {
        eSTANCE_0 = 0,
        eSTANCE_1,
        eSTANCE_MAX
    }
    ////// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// /// 

    #region 	PRIVATE_FIELD
    private eAutoState m_AutoState;
    public PlayerInfo m_AutoProp;

    //private AnimationInterface m_animInterface;
    public CharacterBase m_CharBaseScr;
    
    private NavMeshAgent m_NavMeshAgent;

    private bool m_bBattleStart = false;

    public NpcActionController m_ActionController = null;

    public eAutoProcessState m_eAutoPlayState = eAutoProcessState.eNone;

    public eATTACKCOMBO_TREE m_AtkComboTree;

    private Navigation m_NaviGation = null;

    private int m_iComboAttackIndex = 0;

    private float m_fDashCoolTime = 0.0f;
    private float m_fSkillCoolTime = 0.0f;

    private CharacterBase m_PrimaryTarget = null;

    private eSTANCE m_curStance = eSTANCE.eSTANCE_0;

	public class AddPlayer_SkillCondition
	{
		public int SkillSlotNumber;
		public long SkillIndex;
		public float fCoolTime;
		public float fMaxCoolTime;
		//public float fDistance;
	}
	Dictionary<int, AddPlayer_SkillCondition> m_dicAddPlayerSkillCondition = new Dictionary<int, AddPlayer_SkillCondition>();

	public class SkilluseCondition
    {
        public long iSkillIndex;
        public float fCoolTime;
        public float fMaxCoolTime;
        public float fDistance;
    }

    Dictionary<int, SkilluseCondition> m_dicAutoSkillCondition = new Dictionary<int, SkilluseCondition>();

    public bool m_bNetWorkAI = false;


    #endregion	PRIVATE_FIELD

    #region PROTECTED_FUNCTION
    protected void Awake()
    {
        //Debug.Log("AutoAI.Awake()");
        m_ActionController = new NpcActionController(this);
        m_ActionController.Init();
    }

    protected void Start()
    {
        //Debug.Log("AutoAI.Start()");
        m_CharBaseScr = GetComponent<CharacterBase>();

        SetPeaceState();

		

#if NONSYNC_PVP
		if (SceneManager.instance.GetCurGameMode() == SceneManager.eGAMEMODE.eNONSYNC_PVP)
		{
			m_OnlineUser = false;

			if (PlayerManager.instance.m_bAutoPlay)
			{
				SetAutoPlayState(AutoAI.eAutoProcessState.eReady);
			}
		}
		else
#endif
		{
			if (m_CharBaseScr.charUniqID != PlayerManager.MYPLAYER_INDEX)
			{
				m_OnlineUser = true;

				if (m_CharBaseScr.damageCalculationData != null)
					m_CharBaseScr.damageCalculationData.fCURRENT_HIT_POINT = m_CharBaseScr.damageCalculationData.fMAX_HIT_POINT;

				return;
			}
			else
			{
				m_OnlineUser = false;

				if (PlayerManager.instance.m_bAutoPlay)
				{
					SetAutoPlayState(AutoAI.eAutoProcessState.eReady);
				}
			}
		}

        if (SceneManager.instance.GetCurMapCreateType() == SceneManager.eMAP_CREATE_TYPE.IN_GAME)
        {
            PlayerInfo tmpInfo = PlayerManager.instance.m_PlayerInfo[m_CharBaseScr.charUniqID];

            for (int i = 0; i < tmpInfo.curSkillSlots.Length; ++i)
            {
                if (tmpInfo.curSkillSlots[i] > 0)
                {
                    SkilluseCondition kCondition = new SkilluseCondition();

                    SkillDataInfo.SkillInfo kSkillInfo = m_CharBaseScr.m_AttackInfos[(int)tmpInfo.curSkillSlots[i]].skillinfo;

                    kCondition.iSkillIndex = tmpInfo.curSkillSlots[i];

                    switch (kSkillInfo.target_Type)
                    {
                        case eTargetType.eSELF:
                        case eTargetType.eSELF_AREA:
                            kCondition.fDistance = kSkillInfo.area_Size * 0.5f + 1.0f;
                            break;
                        default:
                            kCondition.fDistance = kSkillInfo.skill_Dist;
                            break;
                    }

                    kCondition.fCoolTime = kCondition.fMaxCoolTime = kSkillInfo.coolTime;

                    m_dicAutoSkillCondition.Add((int)tmpInfo.curSkillSlots[i], kCondition);
                }
            }

			m_fSkillCoolTime = 2;
		}

		if (m_CharBaseScr.damageCalculationData != null)
            m_CharBaseScr.damageCalculationData.fCURRENT_HIT_POINT = m_CharBaseScr.damageCalculationData.fMAX_HIT_POINT;
     }

    public override void End()
    {
        base.End();

        if (m_dicAutoSkillCondition != null)
            m_dicAutoSkillCondition.Clear();

        m_ActionController = null;
    }

    protected void Update()
    {
        if (InGameManager.instance.DontMoveEveryOne())
            return;

        {
            if (SceneManager.instance.GetCurGameMode() == SceneManager.eGAMEMODE.eMODE_PVP
#if NONSYNC_PVP
                || SceneManager.instance.GetCurGameMode() == SceneManager.eGAMEMODE.eNONSYNC_PVP
#endif
                )
            {
                if (m_CharBaseScr.charUniqID == PlayerManager.MYPLAYER_INDEX)
                {
                    if (m_CharBaseScr.m_charController.m_DpadMP.m_DPad == null)
                    {
                        m_CharBaseScr.m_charController.m_DpadMP.m_DPad = (UI_DPad)SceneManager.instance.GetUIComponent(SceneManager.eCOMPONENT.DPad);
                    }
                }
            }

            if (m_eAutoPlayState == eAutoProcessState.eReady && ((m_CharBaseScr.m_MotionState == MOTION_STATE.eMOTION_IDLE || m_CharBaseScr.m_MotionState == MOTION_STATE.eMOTION_NONE) && m_ActionController.IsCurActionCancelAble()) &&
                (m_CharBaseScr.charUniqID != PlayerManager.MYPLAYER_INDEX || 
                m_CharBaseScr.m_charController != null && m_CharBaseScr.m_charController.m_DpadMP.m_DPad != null && m_CharBaseScr.m_charController.m_DpadMP.m_DPad.gameObject.activeSelf && m_CharBaseScr.m_charController.m_bAtkComboHold == false) &&
                InGameManager.instance.m_eStatus == InGameManager.eSTATUS.ePLAY ||
                m_CharBaseScr.m_TauntMove.m_bTaunt /*도발*/)
            {
                m_eAutoPlayState = eAutoProcessState.ePlay;
            }
            if (m_eAutoPlayState == eAutoProcessState.ePlay)
            {
                UpdateBasicAction();

                if (SceneManager.instance.GetCurGameMode() == SceneManager.eGAMEMODE.eMODE_CHAPTER ||
#if DAILYDG
                    SceneManager.instance.GetCurGameMode() == SceneManager.eGAMEMODE.eMODE_DAILY ||
#endif
                    SceneManager.instance.GetCurGameMode() == SceneManager.eGAMEMODE.eMODE_RAID ||
                    SceneManager.instance.GetCurGameMode() == SceneManager.eGAMEMODE.eMODE_DAY_OF_THE_WEEK_DUNGEON ||
                    SceneManager.instance.GetCurGameMode() == SceneManager.eGAMEMODE.eGOLD_DUNGEON)
                {
                    if(NpcManager.instance.NpcBoss != null)
                    {
                        if (NpcManager.instance.NpcBoss.m_CharState == CHAR_STATE.DEATH)
                        {
                            m_eAutoPlayState = eAutoProcessState.eEnding;
                        }
                    }
                }
                else if (SceneManager.instance.GetCurGameMode() == SceneManager.eGAMEMODE.eMODE_PVP )
                {
                    if (m_CharBaseScr.attackTarget != null && (m_CharBaseScr.attackTarget != m_CharBaseScr && m_CharBaseScr.attackTarget.gameObject.activeSelf == false))
                    {
                        m_eAutoPlayState = eAutoProcessState.eEnding;
                    }
                }
            }
            else
                m_PrimaryTarget = null;
        }

        m_ActionController.UpdateActions();

        if (m_eAutoPlayState == eAutoProcessState.eEnding)
        {
            if (m_ActionController.IsCurActionCancelAble() || m_ActionController.GetCurActionState() == NpcAction.eNpcActionState.eEND)
            {
                StopAutoPlay();
                SetAutoPlayState(eAutoProcessState.eNone);
            }
        }

        if (m_fDashCoolTime > 0.0f)
            m_fDashCoolTime -= Time.deltaTime;

        if (m_fSkillCoolTime > 0.0f)
            m_fSkillCoolTime -= Time.deltaTime;

        if (m_CharBaseScr.charUniqID != PlayerManager.MYPLAYER_INDEX && m_OnlineUser == false)
        {
            var enumerator = m_dicAutoSkillCondition.GetEnumerator();
            while (enumerator.MoveNext())
            {
                SkilluseCondition Skills = enumerator.Current.Value;
                if (Skills.fCoolTime < Skills.fMaxCoolTime)
                {
                    Skills.fCoolTime += Time.deltaTime;
                }
            }
        }

		foreach (KeyValuePair<int, AddPlayer_SkillCondition> pair in m_dicAddPlayerSkillCondition)
		{
			if(pair.Value.fCoolTime > 0)
			{
				pair.Value.fCoolTime -= Time.deltaTime;
			}
		}
	}
	#endregion	 PROTECTED_FUNCTION

	#region 	SET_FUNCTION


	public void SetCharacter_Auto(PlayerInfo AutoProp)
    {
        m_CharBaseScr = gameObject.GetComponent<CharacterBase>();
        m_NavMeshAgent = gameObject.GetComponent<NavMeshAgent>();

        if (m_NavMeshAgent == null)
            m_NavMeshAgent = gameObject.AddComponent<NavMeshAgent>();

        m_NavMeshAgent.autoTraverseOffMeshLink = true;
        m_NavMeshAgent.autoBraking = false;
        m_NavMeshAgent.autoRepath = true;
        m_NavMeshAgent.acceleration = 9999;
        m_NavMeshAgent.angularSpeed = 360;

        if (m_CharBaseScr.charUniqID == PlayerManager.MYPLAYER_INDEX)
        {
            m_NavMeshAgent.obstacleAvoidanceType = ObstacleAvoidanceType.LowQualityObstacleAvoidance;
            m_NavMeshAgent.avoidancePriority = 50;
        }
        else
        {
            m_NavMeshAgent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
            m_NavMeshAgent.avoidancePriority = 60;
        }

        m_NavMeshAgent.stoppingDistance = m_NavMeshAgent.radius * 0.1f;
        m_NavMeshAgent.baseOffset = 0.0f;
        m_NavMeshAgent.autoRepath = false;

        m_OriginalNavMeshavoidancePriority = m_NavMeshAgent.avoidancePriority;

        m_AutoProp = AutoProp;
        m_CharBaseScr.m_currSkillIndex = 0;

        if (m_CharBaseScr.charUniqID == PlayerManager.MYPLAYER_INDEX)
        {
            m_NaviGation = m_CharBaseScr.GetComponent<Navigation>();
           
        }

#if UNITY_EDITOR
        gameObject.AddComponent<PathUtils>();
#endif
    }

    public void SetAutoState(eAutoState state)
    {
        m_AutoState = state;

        switch (m_AutoState)
        {
            case eAutoState.ePEACE:
            case eAutoState.eGUARD:
            case eAutoState.eGUARD_MOVING:
                m_eMoveType = eMoveType.eWALK;
                break;
            case eAutoState.eGUARD_TRACE:
            case eAutoState.eATTACK_READY:
            case eAutoState.eATTACK_READY_TRACE:
            case eAutoState.eATTACK_READY_COMPLETE:
            case eAutoState.eATTACK:
            case eAutoState.eSKILL:
                m_eMoveType = eMoveType.eRUN;
                break;
            case eAutoState.eMOVE:
                    m_eMoveType = eMoveType.eRUN;
                break;
            case eAutoState.eBEATTACKED:
                break;
            case eAutoState.eDIE:
                //Taylor : not use anymore
            //case eAutoState.eCHANGESTANCE:
                m_eMoveType = eMoveType.eNONE;
                break;
        }
    }

    public override void SetNavMeshAgent(bool bTurnOn)
    {
        if (m_CharBaseScr.attackTarget != null)
            SetNavMeshAgent(bTurnOn, m_CharBaseScr.attackTarget.transform.position);
        else
        {
            if (!bTurnOn)
            {
                if (m_NavMeshAgent.enabled == false)
                    m_NavMeshAgent.enabled = true;

                m_NavMeshAgent.ResetPath();
            }
        }
    }
    public override void SetNavMeshAgent(bool bTurnOn, Vector3 DestPosition)
    {
        if (m_NavMeshAgent.enabled == false)
            m_NavMeshAgent.enabled = true;

        if (bTurnOn)
        {
            SetNavMeshAgentSpeed();

            m_NavMeshAgent.destination = DestPosition;
        }
        else
        {
            m_NavMeshAgent.ResetPath();
        }
    }

    public override void SetNavMeshobstacleAvoidanceType(ObstacleAvoidanceType p_eType)
    {
        if (m_NavMeshAgent.obstacleAvoidanceType != p_eType)
            m_NavMeshAgent.obstacleAvoidanceType = p_eType;
    }
    public override void SetNavMeshavoidancePriority(int p_iPriority)
    {
        m_NavMeshAgent.avoidancePriority = p_iPriority;
    }
    public override void RollBackNavMeshavoidancePriority()
    {
        m_NavMeshAgent.avoidancePriority = m_OriginalNavMeshavoidancePriority;
    }
    public override void SampleNavMesh(out NavMeshHit hit)
    {
        NavMesh.SamplePosition(m_CharBaseScr.m_MyObj.transform.position, out hit, 2.0f, 255);
    }
    public override bool RayCastNavMesh(Vector3 p_TargetPos, out NavMeshHit hit)
    {
        return m_NavMeshAgent.Raycast(p_TargetPos, out hit);
    }
    public override void GetNavMeshPath(Vector3 p_TargetPos, ref NavMeshPath p_Paths)
    {
        m_NavMeshAgent.CalculatePath(p_TargetPos, p_Paths);
    }
    public override Vector3 GetNavMeshAgentDest()
    {
        return m_NavMeshAgent.destination;
    }
    public override float GetNavMeshAgentSpeed()
    {
        return m_NavMeshAgent.speed;
    }
    public override void FreezeRotate(bool p_bOn)
    {
        m_NavMeshAgent.updateRotation = !p_bOn;
    }
    public override bool HasPath()
    {
        if (!m_NavMeshAgent.pathPending)
        {
            if (m_NavMeshAgent.path.corners.Length < 2)
                return false;
            if ((m_NavMeshAgent.pathEndPosition - m_CharBaseScr.m_MyObj.transform.position).sqrMagnitude < m_NavMeshAgent.stoppingDistance * m_NavMeshAgent.stoppingDistance)
                return false;
        }
        return true;
    }
    public override void NavMeshAgentMove( Vector3 p_Offset )
    {
        if (m_NavMeshAgent.enabled == false)
            m_NavMeshAgent.enabled = true;

        m_NavMeshAgent.Move(p_Offset);
    }
    public override NavMeshPathStatus NavPathState()
    {
        return m_NavMeshAgent.path.status;
    }
    private void SetPeaceState()
    {
        SetAutoState(eAutoState.ePEACE);
        m_iComboAttackIndex = 0;
    }

    private void SetGuardState()
    {
    }

    private void SetGuardTraceState()
    {
        SetNavMeshAgent(true);
        m_CharBaseScr.SetChangeMotionState(MOTION_STATE.eMOTION_TRACE);
        SetAutoState(eAutoState.eGUARD_TRACE);
    }

    private void SetGuardMovingState()
    {
        m_CharBaseScr.SetChangeMotionState(MOTION_STATE.eMOTION_IDLE_WALK);
        SetAutoState(eAutoState.eGUARD_MOVING);
    }

    private void SetAttackReadyState()
    {
        if( m_CharBaseScr.attackTarget == null )
            SetAutoState(eAutoState.ePEACE);
        else 
        {
#if NONSYNC_PVP
            if (SceneManager.instance.GetCurGameMode() == SceneManager.eGAMEMODE.eNONSYNC_PVP)
            {
                try
                {
                    if (m_CharBaseScr.charUniqID == 1)
                    {
                        m_CharBaseScr.m_currSkillIndex = GetComboAttackIndex(ref RaidManager.instance.m_AttackComboSet);

                    }
                    else
                    {
                        m_CharBaseScr.m_currSkillIndex = GetComboAttackIndex(ref PlayerManager.instance.m_PlayerInfo[m_CharBaseScr.charUniqID].playerCharBase.m_charController.m_AttackComboSet);
                    }
                }
                catch(System.Exception e)
                {
                    Debug.LogError("m_AttackComboSet.Error");
                }                
            }
            else
#endif
            {
                m_CharBaseScr.m_currSkillIndex = GetComboAttackIndex(ref PlayerManager.instance.m_PlayerInfo[PlayerManager.MYPLAYER_INDEX].playerCharBase.m_charController.m_AttackComboSet);
            }
            SetAutoState(eAutoState.eATTACK_READY);
        }
    }


    public void SetAttackReadyCompleteState(long p_iSkillCode)
    {
        if (Check_IsAttackAble())
        {
            if (m_AtkComboTree == eATTACKCOMBO_TREE.DASH && m_CharBaseScr.attackTarget != null)
            {
                NavMeshPath paths = new NavMeshPath();
                m_CharBaseScr.m_CharAi.GetNavMeshPath(m_CharBaseScr.attackTarget.m_MyObj.transform.position, ref paths);

                if( paths.corners.Length > 2 )
                {
                    m_fDashCoolTime = 1.0f;
                    SetAttackReadyState();
                    return;
                }
            }

            NpcAction kBaseAttackAction = m_Ai_ActionPool.ObjectNew();

            float[] fSkillIndex = new float[] { (float)p_iSkillCode };
            float[] fTargetInfo = new float[] { 0.0f };

            kBaseAttackAction.InitAction(this, NpcAction.eNpcActionGrade.eBASE, (int)NpcAction.eNPpcActionType.eUSEBASEATTACK, fSkillIndex, (int)PDT_TARGET_TYPE.ePDT_TARGET_TYPE_CURRENT, fTargetInfo, false, null);

            if (m_ActionController.ChangeActionOrSkip(kBaseAttackAction))
            {
                SetAutoState(eAutoState.eATTACK);
            }
            else
                m_ActionController.AddAction(kBaseAttackAction);
        }
        else 
        {
            SetAttackReadyState();
        }
    }
    public void SetMoveState(Vector3 p_TargetPos , float p_fStopDistance = 0.0f )
    {
        if (m_CharBaseScr.m_CharState == CHAR_STATE.DEATH && m_CharBaseScr.charUniqID != PlayerManager.MYPLAYER_INDEX && m_NavMeshAgent.enabled == false )
        {
            m_NavMeshAgent.enabled = true;
        }

        if (m_CharBaseScr.charUniqID == PlayerManager.MYPLAYER_INDEX && m_CharBaseScr.m_BuffController.GetCharDisable(BuffController.eBuffCharDisable.eMOVE) > 0 ||
            m_CharBaseScr.gameObject.activeInHierarchy == false)
            return;

        NpcAction kBaseAttackAction = m_Ai_ActionPool.ObjectNew();
        float[] fMovePos = new float[] { p_TargetPos.x, p_TargetPos.y, p_TargetPos.z };
        float[] fTargetInfo = new float[] { p_fStopDistance };

        kBaseAttackAction.InitAction(this, NpcAction.eNpcActionGrade.eBASE, (int)NpcAction.eNPpcActionType.eMOVE, fMovePos, (int)PDT_TARGET_TYPE.ePDT_TARGET_TYPE_NONE, fTargetInfo, true, null);

        if (m_ActionController.ChangeActionOrSkip(kBaseAttackAction))
        {
            SetAutoState(eAutoState.eMOVE);
        }
        else
        {
            m_Ai_ActionPool.ObjectDelete(kBaseAttackAction);
        }
    }

    public bool SetDamageState(eREACTION_TYPE type)
    {
        NpcAction kBaseAttackAction = m_Ai_ActionPool.ObjectNew();
        float[] fReactionType = new float[] { (float)type };
        float[] fTargetInfo = new float[] { 0.0f };
        kBaseAttackAction.InitAction(this, NpcAction.eNpcActionGrade.eBASE, (int)NpcAction.eNPpcActionType.eBEATTACKTED, fReactionType, (int)PDT_TARGET_TYPE.ePDT_TARGET_TYPE_SELF, fTargetInfo, false, null);

        if (m_ActionController.ChangeActionOrSkip(kBaseAttackAction))
        {
            SetAutoState(eAutoState.eBEATTACKED);
            return true;
        }
        else 
        {
            m_Ai_ActionPool.ObjectDelete(kBaseAttackAction);
        }
        return false;
    }

    public void SetDieState()
    {
        SetAutoState(eAutoState.eDIE);

        m_ActionController.RemoveAllAction();

        // No more Battle
        m_bBattleStart = false;

        if( m_NavMeshAgent.enabled )
            SetNavMeshAgent(false);

        m_eEndMotionState = MOTION_STATE.eMOTION_NONE;
    }

    public bool SetSkillState(long p_iUseSkillIndex)
    {
        if (m_CharBaseScr.gameObject.activeInHierarchy == false)
            return false;

        m_iComboAttackIndex = 0;

        if (Check_IsAttackAble())
        {
            // trace action
            NpcAction newBaseAttackAction = m_Ai_ActionPool.ObjectNew();
            float[] fSkillIndex = new float[] { (float)p_iUseSkillIndex };
            float[] fTargetInfo = new float[] { 0.0f };
            newBaseAttackAction.InitAction(this, NpcAction.eNpcActionGrade.eBASE, (int)NpcAction.eNPpcActionType.eUSESKILL, fSkillIndex, (int)PDT_TARGET_TYPE.ePDT_TARGET_TYPE_SKILLRULE, fTargetInfo, false, null);

            if (m_ActionController.ChangeActionOrSkip(newBaseAttackAction))
            {
                SetAutoState(eAutoState.eSKILL);
                return true;
            }
            else
            {
                m_ActionController.AddAction(newBaseAttackAction);
                //m_Ai_ActionPool.ObjectDelete(newBaseAttackAction);
            }
        }
        else
        {
            SetAttackReadyState();
        }
        return false;
    }
    public void SetRevivalState()
    {
        if (m_CharBaseScr.charUniqID != PlayerManager.MYPLAYER_INDEX)
        {
            m_CharBaseScr.damageCalculationData.fCURRENT_HIT_POINT = m_CharBaseScr.damageCalculationData.fMAX_HIT_POINT;
            m_CharBaseScr.SetCollisionDetection(true);
        }

        m_CharBaseScr.SetCharRevival();
    }


    public void SetAutoPlayState( eAutoProcessState p_State)
    {
        m_eAutoPlayState = p_State;
    }
    public void StopAutoPlay()
    {
        if( m_ActionController.GetActionCount() > 0 )
        {
            m_CharBaseScr.SetChangeMotionState(MOTION_STATE.eMOTION_IDLE);
        }
        m_ActionController.RemoveAllAction();
        m_eEndMotionState = MOTION_STATE.eMOTION_NONE;
        SetPeaceState();
        if (m_CharBaseScr.m_charController != null)
            m_CharBaseScr.m_charController.m_AtkComboTree = eATTACKCOMBO_TREE.NONE;
    }
#endregion	SET_FUNCTION

#region 	GET_FUNCTION
    public override bool GetAvailableRange(float range)
    {
        if (m_CharBaseScr.attackTarget == null)
            return false;

        range += m_CharBaseScr.GetRadius();
        if (m_CharBaseScr.attackTarget != m_CharBaseScr)
            range += m_CharBaseScr.attackTarget.GetRadius();

        if (Vector3.Distance(this.m_CharBaseScr.m_MyObj.position, m_CharBaseScr.attackTarget.transform.position) <= range)
        {
            return true;
        }
        return false;
    }

    private int GetComboAttackIndex( ref Dictionary<int, SkillComboSet> p_ComboSet)
    {
        int kUseBaseAttack = 0;
        m_AtkComboTree = eATTACKCOMBO_TREE.NORMAL;
        PlayerDataInfo kPlayerInfo = PlayerManager.instance.m_PlayerDataInfo[(int)PlayerManager.instance.m_PlayerInfo[m_CharBaseScr.charUniqID].jobType];

        if (kPlayerInfo != null)
        {
            float fsqrRadius = m_CharBaseScr.GetRadius() + m_CharBaseScr.attackTarget.GetRadius();
            fsqrRadius = (m_CharBaseScr.m_MyObj.transform.position - m_CharBaseScr.attackTarget.m_MyObj.transform.position).sqrMagnitude - (fsqrRadius * fsqrRadius);

            bool bDashAttack = m_CharBaseScr.m_CharacterType == eCharacter.eWarrior;

            if ( bDashAttack && fsqrRadius > ((kPlayerInfo.semiTarget_Area * 0.8f) * (kPlayerInfo.semiTarget_Area * 0.8f)))
            {
                switch( m_CharBaseScr.m_CharacterType )
                {
                    case eCharacter.eWarrior:
                        if (m_fDashCoolTime <= 0.0f && m_CharBaseScr.attackTarget.m_CharacterType == eCharacter.eNPC)
                        {
                            kUseBaseAttack = PlayerManager.instance.GetCurDashAttack(m_CharBaseScr);
                            m_CharBaseScr.m_AttackInfos[kUseBaseAttack].skillinfo.skill_Dist = (int)kPlayerInfo.semiTarget_Area;
                            m_fDashCoolTime = 1.0f;
                            m_AtkComboTree = eATTACKCOMBO_TREE.DASH;
                        }
                        else
                            kUseBaseAttack = PlayerManager.instance.GetCurStartAttack(m_CharBaseScr);
                        break;
                    case eCharacter.eWizard:
                        m_fDashCoolTime = 0.0f;
                        kUseBaseAttack = PlayerManager.instance.GetCurDashAttack(m_CharBaseScr);
                          if (kUseBaseAttack == 0)
                              kUseBaseAttack = PlayerManager.instance.GetCurStartAttack(m_CharBaseScr);
                          m_CharBaseScr.m_AttackInfos[kUseBaseAttack].skillinfo.skill_Dist = (int)kPlayerInfo.semiTarget_Area;
                        break;
                }

                m_iComboAttackIndex = kUseBaseAttack;
            }
            else
            {
                if( m_iComboAttackIndex == 0 )
                    m_iComboAttackIndex = PlayerManager.instance.GetCurStartAttack(m_CharBaseScr);
                else
                {
                    //Debug.Log("JobType = "+m_CharBaseScr.m_CharacterType);
                    if (p_ComboSet.ContainsKey(m_iComboAttackIndex))
                    {
                        m_iComboAttackIndex = p_ComboSet[m_iComboAttackIndex].Normal;
                    }
                    else
                    {
                        m_iComboAttackIndex = PlayerManager.instance.GetCurStartAttack(m_CharBaseScr);
                    }
                }
                kUseBaseAttack = m_iComboAttackIndex;
            }
        }

        if(kUseBaseAttack == 0)
        {
            m_iComboAttackIndex = kUseBaseAttack = PlayerManager.instance.GetCurStartAttack(m_CharBaseScr);
        }

        return kUseBaseAttack;
    }

    public eAutoProcessState GetAutoPlayState()
    {
        return m_eAutoPlayState;
    }

#endregion	GET_FUNCTION

#region 	CHECK_FUNCTION

    public bool Check_IsThereTargets()
    {
        if( m_NaviGation != null )
        {
			if (m_PrimaryTarget != null && m_PrimaryTarget.m_CharState == CHAR_STATE.ALIVE && m_PrimaryTarget.m_MotionState != MOTION_STATE.eMOTION_DIE)
			{
                if (m_PrimaryTarget != m_NaviGation.m_target && m_NaviGation.m_target != null
                    && (m_CharBaseScr.m_MyObj.transform.position - m_PrimaryTarget.m_MyObj.transform.position).sqrMagnitude > 4.666666f)
                {
                    m_PrimaryTarget = m_NaviGation.m_target;
                }
                if (m_PrimaryTarget.m_MotionState == MOTION_STATE.eMOTION_DIE)
                {
                    return false;
                }

                m_CharBaseScr.SetAttackTarget(m_PrimaryTarget, false);
                return true;
            }
            else
                m_PrimaryTarget = null;

			if (m_NaviGation.m_target != null && m_NaviGation.m_target.m_CharState == CHAR_STATE.ALIVE && m_NaviGation.m_target.m_MotionState != MOTION_STATE.eMOTION_DIE)
			{
                if (m_PrimaryTarget == null)
                {
                    // 전사는 한놈만 팸
                    if( m_CharBaseScr.m_CharacterType == eCharacter.eWarrior)
                        m_PrimaryTarget = m_NaviGation.m_target;
                }
                if (m_NaviGation.m_target.m_MotionState == MOTION_STATE.eMOTION_DIE)
                {
                    return false;
                }

                m_CharBaseScr.SetAttackTarget(m_NaviGation.m_target, false);
                return true;
            }
            else
                m_CharBaseScr.SetAttackTarget(null);
        }
        else
        {
            List<CharacterBase> kTarget = FindTarget(eTargetState.eHOSTILE, null, 1);
            int iTargetIndex = -1;
            float fMaxDist = float.MaxValue;
            for (int i = 0; i < kTarget.Count; ++i )
            {
                float fDistance = (kTarget[i].m_MyObj.position - m_CharBaseScr.m_MyObj.position).sqrMagnitude;
                if (kTarget[i].m_CharState == CHAR_STATE.ALIVE && fMaxDist > fDistance)
                {
                    fMaxDist = fDistance;
                    iTargetIndex = i;
                }

            }
            if ( iTargetIndex >=0 )
            {
                m_CharBaseScr.SetAttackTarget(kTarget[iTargetIndex],false);
                return true;
            }
        }
        return false;
    }
    
    public override List<CharacterBase> FindTarget( eTargetState p_TargetState , eCharacter[] p_TargetClass , int p_AggroRank )
    {
        List<CharacterBase> targets = new List<CharacterBase>();

        switch ( p_TargetState )
        {
            case eTargetState.eNONE :
                targets.AddRange(FindAllPlayer());
                targets.AddRange(FindAllNPC());
                break;
            case eTargetState.eHOSTILE:

#if NONSYNC_PVP
                if(SceneManager.instance.GetCurGameMode() == SceneManager.eGAMEMODE.eNONSYNC_PVP)
                {
                    //PC 1 Side
                    targets.AddRange(FindEnemyPlayer());
                    //Debug.Log("FindTarget() this = "+this);

                    for (int i = 0; i < NpcManager.instance.m_PlayerSummonList.Count; ++i)
                    {
                       if(SummonManager.instance.GetMySummonEquipSlotIndex(NpcManager.instance.m_PlayerSummonList[i].nContentIndex) >= 0)
                        {
                            targets.Add(NpcManager.instance.m_PlayerSummonList[i].summonCBase);
                        }
                    }
                }
                else
#endif
                {
                    List<CharacterBase> npcList = FindAllNPC();
                    for (int npcs = 0; npcs < npcList.Count; ++npcs)
                    {
                        if (npcList[npcs].m_CharAi != null &&
                            npcList[npcs].m_CharAi.GetNpcProp().Identity_Fnd == eTargetState.eHOSTILE &&
                            npcList[npcs].m_CharAi.GetNpcProp().Npc_Type != eNPC_TYPE.eZONEWALL_OBJ)
                            targets.Add(npcList[npcs]);
                    }

                    targets.AddRange(FindEnemyPlayer());
                }
                // pvp
                break;
            case eTargetState.eFRIENDLY:
                targets.AddRange(FindAllPlayer());
                break;
        }
        // remove class
        if (p_TargetClass != null)
        { 
            for (int i = targets.Count - 1; i >= 0; --i )
            {
                bool bDelete = true;
                for( int j = 0 ; j < p_TargetClass.Length ; ++j )
                {
                    if( p_TargetClass[j] == targets[i].m_CharacterType )
                    {
                        bDelete = false;
                        break;
                    }
                }
                if (bDelete)
                    targets.RemoveAt(i);
            }
        }

        return targets;
    }
    private List<CharacterBase> FindAllPlayer()
    {
        List<CharacterBase> targets = new List<CharacterBase>();

        for (int i = 0; i < PlayerManager.instance.m_PlayerInfo.Count; i++ )
        {
            if (PlayerManager.instance.m_PlayerInfo[i].playerObj != null && PlayerManager.instance.m_PlayerInfo[i].playerObj.activeSelf)
            {
                targets.Add(PlayerManager.instance.m_PlayerInfo[i].playerCharBase);
            }
        }
        return targets;
    }
    private List<CharacterBase> FindEnemyPlayer()
    {
        List<CharacterBase> targets = new List<CharacterBase>();

        for (int i = 0; i < PlayerManager.instance.m_PlayerInfo.Count; i++)
        {
            if (PlayerManager.instance.m_PlayerInfo[i].playerObj != null && 
                PlayerManager.instance.m_PlayerInfo[i].playerObj.activeSelf &&
                PlayerManager.instance.m_PlayerInfo[i].Team != PlayerManager.instance.m_PlayerInfo[m_CharBaseScr.charUniqID].Team &&
                PlayerManager.instance.m_PlayerInfo[i].playerCharBase.chrState == CHAR_STATE.ALIVE &&
                PlayerManager.instance.m_PlayerInfo[i].playerCharBase.m_IngameObjectID != m_CharBaseScr.m_IngameObjectID)
            {
                targets.Add(PlayerManager.instance.m_PlayerInfo[i].playerCharBase);
            }
        }
        return targets;
    }
    private List<CharacterBase> FindAllNPC()
    {
        return NpcManager.instance.GetNpcTargetsBySpwaned();
    }
     
    private bool Check_IsAttackAble()
    {
        if (m_CharBaseScr.charUniqID == PlayerManager.MYPLAYER_INDEX && m_CharBaseScr.m_BuffController.GetCharDisable(BuffController.eBuffCharDisable.eATTACK) > 0)
            return false;
#if NONSYNC_PVP
        else if (m_CharBaseScr.charUniqID == PlayerManager.PVPPLAYER_INDEX && m_CharBaseScr.m_BuffController.GetCharDisable(BuffController.eBuffCharDisable.eATTACK) > 0)
            return false;
#endif

        return true;
    }

    public CharacterBase FindAutoTargeting( eTargetState p_TargetState ,Vector3 p_vForward ,float p_fAngle , float p_fDist )
    {
        List<CharacterBase> kTarget = FindTarget(p_TargetState, null, 1);
        int iTargetIndex = -1;
        float fMaxDist = float.MaxValue;
        for (int i = 0; i < kTarget.Count; ++i)
        {
            float fDistance = (kTarget[i].m_MyObj.position - m_CharBaseScr.m_MyObj.position).sqrMagnitude;
            float fCheckDistance = p_fDist + kTarget[i].GetRadius() + m_CharBaseScr.GetRadius();
            if (kTarget[i].m_CharState == CHAR_STATE.ALIVE && fMaxDist > fDistance && fDistance < fCheckDistance * fCheckDistance )
            {
                if( p_fAngle ==  360.0f )
                {
                    fMaxDist = fDistance;
                    iTargetIndex = i;
                }
                else
                {
                    Vector3 v3TargetDirection = kTarget[i].m_MyObj.position - m_CharBaseScr.m_MyObj.position;
                    Vector2 v2TargetDirection = new Vector2(v3TargetDirection.x, v3TargetDirection.z);
                    Vector2 v2Direction = new Vector2(p_vForward.x, p_vForward.y);
                    float fTargetAngle = Vector2.Angle(v2Direction , v2TargetDirection);
                    /// 범위각 안에 들면,,
                    if (fTargetAngle <= p_fAngle)
                    {
                        fMaxDist = fDistance;
                        iTargetIndex = i;
                    }
                }
            }
        }

        if (iTargetIndex >= 0)
        {
            return kTarget[iTargetIndex];
        }

        return null;
    }

#endregion	CHECK_FUNCTION

#region PUBLIC_FUNCTION
    public override CharacterBase GetCharacterBase()
    {
        return m_CharBaseScr;
    }
    public override NpcActionController GetAIController()
    {
        return m_ActionController;
    }
    public override void SetAniEnd(MOTION_STATE eMotionState)
    {
        m_eEndMotionState = eMotionState;
    }
    public override NpcInfo.NpcProp GetNpcProp()
    {
        return null;
    }
    public override PlayerNpcInfo.PlayerNpcProp GetPlayerNpcProp()
    {
        return null;
    }
    public override PlayerInfo GetPlayProp()
    {
        return m_AutoProp;
    }
    public override bool IsBattle()
    {
        return m_bBattleStart;
    }

#endregion

#region 	PRIVATE_FUNCTION

     private void UpdateBasicAction()
    {
        if( m_bNetWorkAI == false )
        {
            switch (m_AutoState)
            {
                case eAutoState.ePEACE:
                    Proc_PeaceState();
                    break;
                case eAutoState.eGUARD_TRACE:
                    Proc_GuardTraceState();
                    break;
                case eAutoState.eGUARD_MOVING:
                    Proc_GuardMovingState();
                    break;
                case eAutoState.eATTACK_READY:
                    Proc_AttackReady();
                    break;
                case eAutoState.eATTACK_READY_TRACE:
                    Proc_AttackReadyTraceState();
                    break;
                case eAutoState.eATTACK_READY_COMPLETE:
                    Proc_AttackReady_Complete();
                    break;
                case eAutoState.eATTACK:
                    Proc_AttackState();
                    break;
                case eAutoState.eSKILL:
                    Proc_Skill();
                    break;
                case eAutoState.eBEATTACKED:
                    Proc_BeAttackState();
                    break;
                case eAutoState.eMOVE:
                    Proc_MoveState();
                    break;
                case eAutoState.eDIE:
                    Proc_DieState();
                    break;
            }
        }
    }

#region 	PROC_FUNCTION
    private void Proc_PeaceState()
    {
        if (Check_IsThereTargets() )
        {
            SetAttackReadyState();

            m_bBattleStart = true;
        }
        else
        {
            // 보스 방으로 달리다
            if (
                SceneManager.instance.GetCurGameMode() == SceneManager.eGAMEMODE.eMODE_CHAPTER ||
#if DAILYDG
                SceneManager.instance.GetCurGameMode() == SceneManager.eGAMEMODE.eMODE_DAILY ||
#endif
                SceneManager.instance.GetCurGameMode() == SceneManager.eGAMEMODE.eMODE_RAID ||
                SceneManager.instance.GetCurGameMode() == SceneManager.eGAMEMODE.eMODE_DAY_OF_THE_WEEK_DUNGEON
                )
            {
                m_bBattleStart = false;

                if (m_NaviGation != null )
                {
                    Vector2 vec1 = new Vector2(m_NaviGation.m_trNavigationGoal.x, m_NaviGation.m_trNavigationGoal.z);
                    Vector2 vec2 = new Vector2(m_CharBaseScr.m_MyObj.position.x, m_CharBaseScr.m_MyObj.position.z);
                    float fDist = (vec1 - vec2).sqrMagnitude;
                    if (fDist > 1)
                        SetMoveState(m_NaviGation.m_trNavigationGoal);
                }
            }
        }
    }

    private void Proc_AttackReady() 
    {
        if (!GetAvailableRange(Mathf.Max(m_CharBaseScr.m_AttackInfos[m_CharBaseScr.m_currSkillIndex].skillinfo.skill_Dist, m_CharBaseScr.m_AttackInfos[m_CharBaseScr.m_currSkillIndex].skillinfo.area_Size)))
        {
            if (m_AtkComboTree != eATTACKCOMBO_TREE.DASH )
            {
                m_iComboAttackIndex = 0;
#if NONSYNC_PVP
                if (SceneManager.instance.GetCurGameMode() == SceneManager.eGAMEMODE.eNONSYNC_PVP)
                {
                    m_CharBaseScr.m_currSkillIndex = PlayerManager.instance.m_PlayerStanceData[(int)PlayerManager.instance.m_PlayerInfo[m_CharBaseScr.charUniqID].jobType][0].m_nNormalAttackID[0];
                }
                else
#endif
                {
                    m_CharBaseScr.m_currSkillIndex = PlayerManager.instance.m_PlayerStanceData[(int)PlayerManager.instance.m_PlayerInfo[PlayerManager.MYPLAYER_INDEX].jobType][0].m_nNormalAttackID[0];
                }
            }

            SetAttackReadyTraceState();
        }
        else
        {

            m_CharBaseScr.SetAttackTarget(m_CharBaseScr.attackTarget);

            if (m_CharBaseScr.m_Silence == false) 

            {
                if ((m_CharBaseScr.charUniqID == PlayerManager.MYPLAYER_INDEX && PlayerManager.instance.m_bAutoPlay_SKill) || m_CharBaseScr.charUniqID == PlayerManager.PVPPLAYER_INDEX || m_OnlineUser == true)
                {
                    if (m_fSkillCoolTime <= 0.0f)
                    {
						int iUseSkillIndex = UtilManager.instance.Random(0, 4);
						
						if (m_CharBaseScr.charUniqID == PlayerManager.MYPLAYER_INDEX)
						{
							if(PlayerAutoSkillCheck() == true)
							{
								return;
							}
						}						
						else if(m_CharBaseScr.charUniqID == PlayerManager.PVPPLAYER_INDEX)
						{
							if (AddPlayerAutoSkillCherck() == true)
							{
								return;
							}
						}
						
						else if(m_CharBaseScr.charUniqID != PlayerManager.MYPLAYER_INDEX && m_OnlineUser == false)
						{
							int iCurIndex = 0;
							var enumerator = m_dicAutoSkillCondition.GetEnumerator();
							while (enumerator.MoveNext())
							{
								SkilluseCondition Skills = enumerator.Current.Value;
								if (iUseSkillIndex == iCurIndex && Skills.fCoolTime >= Skills.fMaxCoolTime)
								{
									SkillDataInfo.SkillInfo kSkillInfo = m_CharBaseScr.m_AttackInfos[(int)Skills.iSkillIndex].skillinfo;

									if (kSkillInfo != null)
									{
										if (GetAvailableRange(Skills.fDistance))
										{
											if (SetSkillState(Skills.iSkillIndex))
											{
												Skills.fCoolTime = 0.0f;
												return;
											}
										}
									}
								}
								++iCurIndex;
							}
						}
					}
				}
            }

            SetAttackReadyCompleteState(m_CharBaseScr.m_currSkillIndex);
        }
    }
	


    private void Proc_AttackState()
    {
        if (m_ActionController.GetCurActionState() == NpcAction.eNpcActionState.eEND)
        {
            if (Check_IsThereTargets())
            {
                SetAttackReadyState();
            }
            else
            {
                SetPeaceState();
            }
        }
    }
    private void Proc_BeAttackState()
    {
        m_iComboAttackIndex = 0;
        if (m_ActionController.GetCurActionState() == NpcAction.eNpcActionState.eEND)
        {
            SetAttackReadyState();
        }
    }
    private void Proc_TraceState()
    {
    }
    private void Proc_MoveState()
    {
        if (m_eAutoPlayState == eAutoProcessState.ePlay)
        {
            if (Check_IsThereTargets() )
            {
                SetPeaceState();
                return;
            }
            else
            {
                // 전방 검사 가로막은 상자 공격
                float fDistance = 1.5f + m_CharBaseScr.GetRadius();
                RaycastHit hit;
                Ray ray = new Ray(m_CharBaseScr.m_MyObj.position, m_CharBaseScr.m_MyObj.forward);

                if (Physics.SphereCast(ray, m_NavMeshAgent.radius, out hit, fDistance, 1 << LayerMask.NameToLayer("NPC")) == true)
                {
                    if (hit.transform != null && hit.transform.CompareTag("NPC") == true)       // 벽에 닿으면...
                    {
                        CharacterBase targetChar = hit.transform.GetComponent<CharacterBase>();
                        if (targetChar != null)
                        {
                            m_bBattleStart = true;
                            m_CharBaseScr.SetAttackTarget(targetChar);
                            SetAttackReadyState();
                        }
                    }
                }
            }
        }
        if (m_ActionController.GetCurActionState() == NpcAction.eNpcActionState.eEND)
        {
            SetPeaceState();
        }
    }
    private void Proc_DieState()
    {
        if (m_CharBaseScr.m_CharState == CHAR_STATE.ALIVE && m_CharBaseScr.m_MotionState == MOTION_STATE.eMOTION_IDLE)
            SetPeaceState();
    }

    private void Proc_Skill()
    {
        if (m_eAutoPlayState == eAutoProcessState.ePlay)
        {
            if (m_ActionController.GetCurActionState() == NpcAction.eNpcActionState.eEND)
            {
                SetPeaceState();
            }
        }
    }
    private void Proc_ChangeStanceState()
    {
        if (m_eAutoPlayState == eAutoProcessState.ePlay)
        {
            if (m_ActionController.GetCurActionState() == NpcAction.eNpcActionState.eEND)
            {
                SetPeaceState();
            }
        }
    }

    public void SetCollisionDetection( bool p_bEnable )
    {
        m_NavMeshAgent.enabled = p_bEnable;
    }
    public override bool GetCollisionDetection()
    {
        return m_NavMeshAgent.enabled;
    }
    public bool CheckMontionStateChange()
    {
        if( m_eAutoPlayState == eAutoProcessState.ePlay || m_eAutoPlayState == eAutoProcessState.eEnding )
        {
            return true;
        }
        return false;
    }
    public eAutoState GetAutoState()
    {
        return m_AutoState;
    }

    public eSTANCE GetStance()
    {
        return m_curStance;
    }
    public int GetStanceForIndex()
    {
        return (int)m_curStance;
    }

    public void SetStance(int stance)
    {
        m_curStance = (eSTANCE)stance;
    }

#endregion	PROC_FUNCTION

#endregion	PRIVATE_FUNCTION


#region     ANIM_EVENT_FUNCTION
#endregion  ANIM_EVENT_FUNCTION

#region PROPERTY    
#endregion PROPERTY
}
