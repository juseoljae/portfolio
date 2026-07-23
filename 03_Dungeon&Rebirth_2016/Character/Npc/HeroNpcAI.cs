using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using KSPlugins;

public enum SUMMONSIZE
{    
    eSUMMON_SIZE_NONE = 0,
    eSUMMON_SMALL_SIZE,  
    eSUMMON_MEDIUM_SIZE, 
    eSUMMON_BIG_SIZE,    
};


public enum SUMMONTYPE
{
    eSUMMON_TYPE_NONE = 0, 
    eSUMMON_BATTLE_TYPE,    
    eSUMMON_DEFENSIVE_TYPE, 
    eSUMMON_LONGATTACK_TYPE,
    eSUMMON_FIGHT_TYPE      
};


public class HeroNpcAI : AI
{
    public enum eNpcState : byte
    {
        ePEACE = 0,
        eGUARD,
        eGUARD_TRACE,
        eGUARD_MOVING,
        eATTACK_READY,
        eATTACK_READY_TRACE,
        eATTACK_READY_COMPLETE,
        eATTACK,
        eBEATTACKED,
        eMOVE,
        eDIE,
        eSKILL,
        eSPWAN,
        //ZUNGHOON ADD 
        eSPWAN_STANDBY,
        
        eNONE
    }
    public enum eAutoProcessState : byte
    {
        eNone = 0,
        eReady,
        ePlay,
        eEnding,
    }

    public float DIE_DELAY_TIME = 3.5f;

    #region 	PRIVATE_FIELD
    private eNpcState m_npcState = eNpcState.eSPWAN;

    public NpcInfo.NpcProp m_PlayerNpcProp;

    private List<KeyValuePair<GameObject, uint> >m_uAggroData = new List<KeyValuePair<GameObject, uint> >();
    private GameObject m_NextAggroChar = null;

    public CharacterBase m_CharBaseScr;

    private string m_NpcModelDataRoute; 
    private string m_NpcModelDataName; 
    private Transform m_NpcObj;
    private Navigation m_NaviGation = null;
    
    public eAutoProcessState m_eAutoPlayState = eAutoProcessState.eNone;

    private CharacterBase m_PrimaryTarget = null;
    private Vector3 pos;
    
    private NavMeshAgent m_NavMeshAgent;
    private NavMeshObstacle m_NavMeshObstacle;

    public bool m_bBattleStart = false;
    public bool m_bNpcNetworkController = false;

    public NpcActionController m_ActionController = null;

    public NpcAI_TimeTrigerController m_NpcAI_TimeTrigger = null;
    public NpcAI_PatternController m_NpcAI_Pattern = null;
    
    private int m_nNpcGroup;
    private int m_nFreeActivityRadius;
    private uint m_uiBattleSearchRange = 1000;
    private Vector3 m_v3SourcePosition;

    float m_fPatrolTimer = 0.0f;
    float m_fAIWaitTimer = 0.0f;
    float m_fSideMoveTimer = 0.0f;
    float m_fGuardMoveTimer = 0.0f;

    bool m_bUsePatternAI = false;

    public List<Material> m_DeathMaterial;
    public List<Material> m_DieMaterialProc;

    float m_LinkSkillCoolTime = 0.0f;

    public Color[] m_HColor;

	private Vector3 m_ThisSummonPosition;
	private Vector3 m_PlayerPosition;
	private float m_CallSummonTime = 4.0f;
	private bool m_CallSummonUpdateFlag = false;

	public struct DieVar
    {
        public bool m_bGowayWait;
        public float m_GowayWaitTime;
        public bool m_bGowayStart;
        public bool m_bStandingDeath;
        public float m_fGowayTime;
        public float m_fGowayTime_Dissolve;

        public void Init()
        {
            m_bGowayWait = false;
            m_GowayWaitTime = Time.time;
            m_bGowayStart = false;
            m_bStandingDeath = false;
            m_fGowayTime = 0.0f;
            m_fGowayTime_Dissolve = 1.5f;
        }
    }
    public DieVar m_DieVar;

    private Texture2D m_DeathTexture;
    private Texture2D m_DeathTexture2;
    public List<Renderer> m_PrefabRandererCom;

    #endregion	PRIVATE_FIELD

    public static int AggroMemeberComparer(KeyValuePair<GameObject, uint> x, KeyValuePair<GameObject, uint> y)
    {
        if (x.Value > y.Value)
            return -1;
        else if (x.Value < y.Value)
            return 1;
        return 0;
    }

    #region PROTECTED_FUNCTION
    protected void Awake()
    {
        m_ActionController = new NpcActionController(this);
        m_ActionController.Init();
        pos = new Vector3(0,0,0);
    }

    protected void Start()
    {
        m_CharBaseScr = GetComponent<CharacterBase>();
        
        m_NpcObj = m_CharBaseScr.m_MyObj;

#if NONSYNC_PVP
        if(SceneManager.instance.GetCurGameMode() == SceneManager.eGAMEMODE.eNONSYNC_PVP)
        {
            if (m_CharBaseScr.charUniqID == 0)
            {
                m_CharBaseScr.m_MyObj.gameObject.AddComponent<Navigation>();
                m_NaviGation = m_CharBaseScr.m_MyObj.gameObject.GetComponent<Navigation>();
            }
        }
        else
#endif
        {
            m_CharBaseScr.m_MyObj.gameObject.AddComponent<Navigation>();
            m_NaviGation = m_CharBaseScr.m_MyObj.gameObject.GetComponent<Navigation>();
        }

        m_DieVar = new DieVar();
        m_DieMaterialProc = new List<Material>();

        if (m_DeathMaterial == null )
        {
            m_DeathMaterial = new List<Material>();
            for (int i = 0; i < m_CharBaseScr.m_preFabRenderer.Count; ++i)
            {
                if (m_CharBaseScr.m_preFabRenderer[i].GetComponent<Renderer>().material.shader.name == "Keystudio/CartoonRim")
                {
                    Material mat = m_CharBaseScr.m_preFabRenderer[i].GetComponent<Renderer>().material;
                    Texture _Diffuse_map = m_CharBaseScr.m_preFabRenderer[i].material.GetTexture("_Diffuse");

                    m_DeathMaterial.Add(new Material(Shader.Find("Keystudio/CartoonRimDie")));
                    m_DeathMaterial[i].SetFloat("_Dissolve", 0.0f);
                    m_DeathMaterial[i].SetTexture("_Diffuse", _Diffuse_map);
                    m_DeathMaterial[i].SetFloat("_Diffuse_power", mat.GetFloat("_Diffuse_power"));
                    m_DeathMaterial[i].SetColor("_RimColor", mat.GetColor("_RimColor"));
                    m_DeathMaterial[i].SetFloat("_Emission_Power", mat.GetFloat("_Emission_Power"));
                    m_DeathMaterial[i].SetFloat("_RimIn", mat.GetFloat("_RimIn"));
                    m_DeathMaterial[i].SetFloat("_RimPower", mat.GetFloat("_RimPower"));
                }
                else
                {
                    m_DeathMaterial.Add(null);
                }
            }
        }


        if (m_CharBaseScr.damageCalculationData != null)
        {
            m_CharBaseScr.damageCalculationData.fCURRENT_HIT_POINT = m_CharBaseScr.damageCalculationData.fMAX_HIT_POINT;

            InGameManager.instance.UiEquipSummonHpRefresh(m_CharBaseScr);
            
        }
		m_ThisSummonPosition = new Vector3(0,0,0);
		m_PlayerPosition = new Vector3(0, 0, 0);

		SetAutoPlayState(HeroNpcAI.eAutoProcessState.eReady);

    }
    public override void End()
    {
        base.End();

        if (m_uAggroData != null)
            m_uAggroData.Clear();
        m_uAggroData = null;

        m_ActionController = null;
        m_NpcAI_TimeTrigger = null;
        m_NpcAI_Pattern = null;

        if (m_DeathMaterial != null)
            m_DeathMaterial.Clear();
        m_DeathMaterial = null;
    }

    protected void Update()
    {
        if (InGameManager.instance.DontMoveEveryOne())
            return;
        if (EventTriggerManager.instance.Cutscene_Boss != null)
        {
            if (EventTriggerManager.instance.Cutscene_Boss.State == CinemaDirector.Cutscene.CutsceneState.Playing || m_npcState == eNpcState.eNONE)
            {
                return;
            }
        }

        if (m_eAutoPlayState == eAutoProcessState.eReady && ((m_CharBaseScr.m_MotionState == MOTION_STATE.eMOTION_IDLE || m_CharBaseScr.m_MotionState == MOTION_STATE.eMOTION_NONE) && m_ActionController.IsCurActionCancelAble())
                                                 
                                                 && InGameManager.instance.m_eStatus == InGameManager.eSTATUS.ePLAY
                                                 || m_CharBaseScr.m_TauntMove.m_bTaunt)
        {
            m_eAutoPlayState = eAutoProcessState.ePlay;
        }
        if (m_eAutoPlayState == eAutoProcessState.ePlay)
        {

            CheckSkillCoolTime();
            UpdatePatternAction();
            UpdateBasicAction();
			CheckPositon();

			if (NpcManager.instance.NpcBoss != null)
            {
                if (NpcManager.instance.NpcBoss.m_CharState == CHAR_STATE.DEATH)
                {
                    m_eAutoPlayState = eAutoProcessState.eEnding;
                }
            }
        }
        else
        {
            m_PrimaryTarget = null;
        }

        m_ActionController.UpdateActions();

        if (m_eAutoPlayState == eAutoProcessState.eEnding)
        {
            if (m_ActionController.IsCurActionCancelAble() || m_ActionController.GetCurActionState() == NpcAction.eNpcActionState.eEND)
            {                
                SetAutoPlayState(eAutoProcessState.eNone);
            }
        }

        {
            //Debug.Log(" m_MotionState ============================================================================ " + m_MotionState);

#if NONSYNC_PVP
            if (SceneManager.instance.GetCurGameMode() != SceneManager.eGAMEMODE.eNONSYNC_PVP)
#endif
            {
				if (m_PlayerNpcProp.Summon_EquipIndex == -1)
				{
					switch (m_CharBaseScr.m_MotionState)
					{
						case MOTION_STATE.eMOTION_NONE:
						case MOTION_STATE.eMOTION_IDLE:
						case MOTION_STATE.eMOTION_IDLE_WALK:
						case MOTION_STATE.eMOTION_SIDE_WALK:
						case MOTION_STATE.eMOTION_WALK:
						case MOTION_STATE.eMOTION_RUN:
						case MOTION_STATE.eMOTION_SPWAN_STANDBY:
							NpcManager.instance.SetSummonStop(m_CharBaseScr);
							//m_NpcObj.gameObject.SetActive(false);
							break;
					}
				}
			}
        }

    }
    #endregion	 PROTECTED_FUNCTION

    #region 	SET_FUNCTION
    public void SetCharacter_Npc(NpcInfo.NpcProp PlayerNpcProp, NpcInfo.NpcModelList PlayerNpcModelList, List<NpcAI_PatternInfo.Pattern_TableData> PatternDatas)
    {
        m_CharBaseScr = gameObject.GetComponent<CharacterBase>();
        m_NavMeshAgent = gameObject.GetComponent<NavMeshAgent>();

        if (m_NavMeshAgent != null)
        {
            m_NavMeshAgent.autoTraverseOffMeshLink = true;
            m_NavMeshAgent.autoBraking = false;
            m_NavMeshAgent.autoRepath = false;
            m_NavMeshAgent.acceleration = 1000;
            m_NavMeshAgent.angularSpeed = 360;
            m_NavMeshAgent.stoppingDistance = m_NavMeshAgent.radius * 0.1f;
            m_NavMeshAgent.obstacleAvoidanceType = ObstacleAvoidanceType.LowQualityObstacleAvoidance;
            m_NavMeshAgent.radius = m_CharBaseScr.GetRadius() / m_CharBaseScr.m_MyObj.transform.localScale.x;
            switch (PlayerNpcProp.Npc_Class)
            {
                case eNPC_CLASS.eNORMAL:  
                    m_NavMeshAgent.avoidancePriority = 50;
                    break;
                case eNPC_CLASS.eELITE:   
					m_NavMeshAgent.avoidancePriority = 50;
					break;
                case eNPC_CLASS.eBOSS:  
					m_NavMeshAgent.avoidancePriority = 50;
					break;
                case eNPC_CLASS.eRAIDBOSS:
                case eNPC_CLASS.eNONE:    
					m_NavMeshAgent.avoidancePriority = 50;
					break;
            }
            switch (PlayerNpcProp.Npc_Size)
            {
                case 0: 
                case 1: 
					m_NavMeshAgent.avoidancePriority = 50;
					break;
                case 2: 
					m_NavMeshAgent.avoidancePriority = 50;
					break;
                case 3: 
					m_NavMeshAgent.avoidancePriority = 50;
					break;
            }


            if(PlayerNpcProp.Npc_Type == eNPC_TYPE.eMONSTER)
            {
                m_PrefabRandererCom = new List<Renderer>();
                m_DeathTexture = KSPlugins.KSResource.AssetBundleInstantiate<Texture2D>("Effect/Texture/512noise", true);
                m_DeathTexture2 = KSPlugins.KSResource.AssetBundleInstantiate<Texture2D>("Effect/Texture/gradient_ramp", true);
            }

            m_OriginalNavMeshavoidancePriority = m_NavMeshAgent.avoidancePriority;
        }

#if UNITY_EDITOR
        gameObject.AddComponent<PathUtils>();
#endif
        // 투두
        Rigidbody rig = this.m_CharBaseScr.gameObject.AddComponent<Rigidbody>();
        rig.isKinematic = true;
        rig.useGravity = false;

        Transform[] childs = m_CharBaseScr.gameObject.GetComponentsInChildren<Transform>();

        switch (PlayerNpcProp.Npc_Class)
        {
            case eNPC_CLASS.eNONE:   
            case eNPC_CLASS.eNORMAL: 
            case eNPC_CLASS.eELITE:  
            case eNPC_CLASS.eBOSS:   
                {
                    int iLayer = LayerMask.NameToLayer("HERONPC");
                    for (int i = 0; i < childs.Length; i++)
                    {
                        childs[i].gameObject.layer = iLayer;
                    }
                }
                break;
            case eNPC_CLASS.eRAIDBOSS: 
                {
                    int iLayer = LayerMask.NameToLayer("HERONPC");
                    for (int i = 0; i < childs.Length; i++)
                    {
                        childs[i].gameObject.layer = iLayer;
                    }
                }
                break;
        }

        m_NpcModelDataRoute = PlayerNpcModelList.Model_Data_route;
        m_NpcModelDataName = PlayerNpcModelList.Model_Prefabs_Name;

        //Taylor
        if (PlayerNpcProp.Die_Effect_Idx != 0)
        {
            string effName = CExcelData_EFFECT_LIST.instance.EFFECT_LISTBASE_GetEFFECT_NAME(SkillDataManager.instance.m_EffectCodeDic[PlayerNpcProp.Die_Effect_Idx]);
            PlayerNpcProp.Die_EffectObj = KSPlugins.KSResource.AssetBundleInstantiate<GameObject>("Effect/" + effName);
            UtilManager.instance.ResetShader(PlayerNpcProp.Die_EffectObj);
            /// ]
            PlayerNpcProp.Die_EffectObj.transform.parent = m_CharBaseScr.GetCharEffectBone("FX_base");
            PlayerNpcProp.Die_EffectObj.transform.localPosition = new Vector3(0, 0, 0);
            PlayerNpcProp.Die_EffectObj.SetActive(false);
        }
        m_PlayerNpcProp = PlayerNpcProp;
        m_uiBattleSearchRange = PlayerNpcProp.Search_Range;

        m_CharBaseScr.m_currSkillIndex = PlayerNpcProp.link_SkillCode[0];

        SetCharacter_NpcAI(PatternDatas);
    }
    public void SetCharacter_NpcAI( List<NpcAI_PatternInfo.Pattern_TableData> PatternDatas)
    {        
        // Add Npc Pattern Data Index 
        if (PatternDatas != null)
            m_NpcAI_Pattern = new NpcAI_PatternController(null, PatternDatas.ToArray(), this);


        SetSpwanState();
        SetPeaceState();
    }

    public void SetNpcState(eNpcState state)
    {
        m_npcState = state;

        if (InGameManager.instance.m_bOffLine == true || InGameManager.instance.m_bHostPlay == true)
        {
            switch (m_npcState)
            {
                case eNpcState.eSPWAN_STANDBY:
                    break;
                case eNpcState.eSPWAN:
                case eNpcState.ePEACE:
                    m_eMoveType = eMoveType.eWALK;
                    break;
                case eNpcState.eGUARD:
                case eNpcState.eGUARD_MOVING:
                    m_eMoveType = eMoveType.eSIDE;
                    break;
                case eNpcState.eGUARD_TRACE:
                case eNpcState.eATTACK_READY:
                case eNpcState.eATTACK_READY_TRACE:
                case eNpcState.eATTACK_READY_COMPLETE:
                case eNpcState.eATTACK:
                case eNpcState.eSKILL:
                    m_eMoveType = eMoveType.eRUN;
                    break;
                case eNpcState.eMOVE:
                    m_eMoveType = eMoveType.eRUN;
                    break;
                case eNpcState.eBEATTACKED:
                    break;
                case eNpcState.eDIE:
                    m_eMoveType = eMoveType.eNONE;
                    break;
            }
        }
    }
	public eAutoProcessState GetAutoPlayState()
	{
		return m_eAutoPlayState;
	}
	public void SetAutoPlayState(eAutoProcessState p_State)
    {
        m_eAutoPlayState = p_State;
    }
    public override void SetNavMeshAgent(bool bTurnOn)
    {
        if (!m_CharBaseScr.enabled)
        {
            UnityEngine.Debug.Log("m_CharBaseScr = > enabled false ");
            return;
        }

        if (m_CharBaseScr.attackTarget != null)
            SetNavMeshAgent(bTurnOn, m_CharBaseScr.attackTarget.transform.position);
        else
        {
            if (!bTurnOn)
            {
                if (m_NavMeshAgent.enabled == false)
                    m_NavMeshAgent.enabled = true;

                //m_NavMeshAgent.Stop(true);
                if (m_NavMeshAgent.enabled == true)
                    m_NavMeshAgent.ResetPath();
                //m_NavMeshObstacle.enabled = true;
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

            m_NavMeshAgent.SetDestination( DestPosition );
        }
        else
        {
            if (!m_CharBaseScr.enabled)
            {
                UnityEngine.Debug.Log("m_CharBaseScr = > enabled false ");
                return;
            }
            m_NavMeshAgent.ResetPath();
        }
    }
    public override void SetNavMeshAgentSpeed()
    {
        if (m_PlayerNpcProp.Walk_Speed == 0 || m_PlayerNpcProp.Run_Speed == 0)
            return;

        switch (m_eMoveType)
        {
            case eMoveType.eSIDE:
            case eMoveType.eWALK:
                m_NavMeshAgent.speed = (m_CharBaseScr.damageCalculationData.fMOVE_SPEED + m_CharBaseScr.damageCalculationData.fCOMBOMOVE_SPEED) * ((float)m_PlayerNpcProp.Walk_Speed / (float)m_PlayerNpcProp.Run_Speed);
                break;
            case eMoveType.eRUN:
				float fPlayerComboSpeed		= PlayerManager.instance.playerInfoList[PlayerManager.MYPLAYER_INDEX].playerCharBase.damageCalculationData.fCOMBOMOVE_SPEED;
				float fMoveSpeedRatio		= fPlayerComboSpeed + m_CharBaseScr.damageCalculationData.fTOTAL_MOVESPEED_RATIO + (float)m_CharBaseScr.damageCalculationData.dSUMMONATTRIBUTE_RUN_SPEED_RATIO;

				m_NavMeshAgent.speed		= m_CharBaseScr.damageCalculationData.fTOTAL_MOVESPEED_ADD * (1 + (fMoveSpeedRatio * 0.01f));
				break;
        }
        m_NavMeshAgent.speed += m_fCorrectionSpeed;
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
        NavMesh.SamplePosition(m_CharBaseScr.m_MyObj.transform.position, out hit, 10.0f, 255);
    }
    public override bool RayCastNavMesh(Vector3 p_TargetPos, out NavMeshHit hit)
    {
        return m_NavMeshAgent.Raycast(p_TargetPos, out hit);
    }
    public override void GetNavMeshPath(Vector3 p_TargetPos, ref NavMeshPath p_Paths)
    {

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
    public override bool IsBattle()
    {
        return m_bBattleStart;
    }

    public void SetSpwanState()
    {
        SetNpcState(eNpcState.eSPWAN);
    
    }

    public void SetSpwanStandByState()
    {
        SetNpcState(eNpcState.eSPWAN_STANDBY);        
    }

    public void SetPeaceState()
    {
        SetNpcState(eNpcState.ePEACE);
    }

	private void CheckPositon()
	{

        if (SceneManager.instance.GetCurGameMode() != SceneManager.eGAMEMODE.eMODE_CHAPTER)
		{
			return;
		}

		m_ThisSummonPosition = m_CharBaseScr.m_MyObj.transform.localPosition;
		m_PlayerPosition = m_CharBaseScr.m_MyObj.transform.localPosition;
		float a_kCheckPositionY = m_ThisSummonPosition.y - m_PlayerPosition.y;
		if (PlayerManager.instance.m_PlayerInfo[PlayerManager.MYPLAYER_INDEX].playerCharBase != null)
		{
			if(a_kCheckPositionY >= 3)
			{
				m_CallSummonUpdateFlag = true;
				CallSummon();
			}
			else
			{
				m_CallSummonUpdateFlag = false;
				m_CallSummonTime = 4.0f;
			}
		}		
	}

	private void CallSummon()
	{
		m_CallSummonTime -= Time.deltaTime;
		if(m_CallSummonTime <= 0)
		{
			m_CharBaseScr.m_MyObj.transform.localPosition = m_CharBaseScr.m_MyObj.transform.localPosition;
			m_CallSummonUpdateFlag = false;
			m_CallSummonTime = 4.0f;
		}
	}

    private void SetGuardState()
    {
        if (!GetAvailableRange(m_PlayerNpcProp.Alert_Distance_Max) && m_CharBaseScr.attackTarget != null)
        {
            float fRandDist = UtilManager.instance.Random(m_PlayerNpcProp.Alert_Distance_Min , m_PlayerNpcProp.Alert_Distance_Max);
            SetGuardTraceState(m_CharBaseScr.attackTarget.m_MyObj.transform.position + fRandDist * (m_CharBaseScr.m_MyObj.transform.position - m_CharBaseScr.attackTarget.m_MyObj.transform.position).normalized, fRandDist);
        }
        else
        {
            SetNpcState(eNpcState.eGUARD);
        }
    }
     
    private void SetGuardTraceState(Vector3 p_TargetPos,float p_fGuardRadius)
    {
        if ((InGameManager.instance.m_bOffLine == true || InGameManager.instance.m_bHostPlay == true) && (m_CharBaseScr.m_BuffController.GetCharDisable(BuffController.eBuffCharDisable.eMOVE) > 0))
        {
            SetPeaceState();
            return;
        }
        NpcAction kBaseAttackAction = m_Ai_ActionPool.ObjectNew();
        float[] fMovePos = new float[] { p_TargetPos.x, p_TargetPos.y, p_TargetPos.z };
        float[] fTargetInfo = new float[] { 0.0f, p_fGuardRadius };
        kBaseAttackAction.InitAction(this, NpcAction.eNpcActionGrade.eBASE, (int)NpcAction.eNPpcActionType.eMOVE, fMovePos, (int)PDT_TARGET_TYPE.ePDT_TARGET_TYPE_GUARD_CURRENT, fTargetInfo, true, null);

        if (m_ActionController.ChangeActionOrSkip(kBaseAttackAction))
        {
            SetNpcState(eNpcState.eGUARD_TRACE);
            m_fGuardMoveTimer = NetworkManager.NPC_MOVE_DELAY;
        }
        else
        {
            m_Ai_ActionPool.ObjectDelete(kBaseAttackAction);
        }
    }

    private void SetGuardMovingState()
    {
        if ((InGameManager.instance.m_bOffLine == true || InGameManager.instance.m_bHostPlay == true) && (m_CharBaseScr.m_BuffController.GetCharDisable(BuffController.eBuffCharDisable.eMOVE) > 0))
        {
            SetPeaceState();
            return;
        }
        Vector3 SideDir = (m_CharBaseScr.m_MyObj.transform.position - m_CharBaseScr.attackTarget.m_MyObj.transform.position).normalized;

        float fDist = Vector3.Distance(m_CharBaseScr.m_MyObj.transform.position, m_CharBaseScr.attackTarget.m_MyObj.transform.position) - (m_CharBaseScr.GetRadius() + m_CharBaseScr.attackTarget.GetRadius());

        int iBackStep = 0;

        if (GetAvailableRange( Mathf.Min(m_PlayerNpcProp.Alert_Distance_Min, m_PlayerNpcProp.Dead_Zone + m_PlayerNpcProp.Alert_Distance_Min * 0.5f) ))
        {
            fDist = UtilManager.instance.Random(m_PlayerNpcProp.Alert_Distance_Min, m_PlayerNpcProp.Alert_Distance_Max) - fDist;
            iBackStep = 1;
        }
        else 
        {
            Vector3 SideVec = Vector3.Cross(SideDir, Vector3.up);

            if (UtilManager.instance.RandomPercent(50))
                SideDir = SideVec;
            else
                SideDir = -SideVec;

            fDist = UtilManager.instance.Random(m_PlayerNpcProp.Alert_Distance_Min, m_PlayerNpcProp.Alert_Distance_Max) - m_PlayerNpcProp.Alert_Distance_Min + 1.0f;

            int iDist = (int)(fDist*1000.0f);
            iDist = Random.Range(iDist / 2, iDist);
            fDist = ((float)iDist) / 1000.0f;
        }
        SideDir = this.transform.position + SideDir * fDist;

        NpcAction kBaseAttackAction = m_Ai_ActionPool.ObjectNew();
        float[] fMovePos = new float[] { SideDir.x, SideDir.y, SideDir.z };
        float[] fTargetInfo = new float[] { (float)iBackStep };
        kBaseAttackAction.InitAction(this, NpcAction.eNpcActionGrade.eBASE, (int)NpcAction.eNPpcActionType.eMOVE, fMovePos, (int)PDT_TARGET_TYPE.ePDT_TARGET_TYPE_DIRECTION, fTargetInfo, true, null);

        if (m_ActionController.ChangeActionOrSkip(kBaseAttackAction))
        {
            SetNpcState(eNpcState.eGUARD_MOVING);
            m_fGuardMoveTimer = fDist / m_PlayerNpcProp.Walk_Speed * 2.0f;
        }
        else
        {
            m_Ai_ActionPool.ObjectDelete(kBaseAttackAction);
        }
    }

    private void SetAttackReadyState()
    {
        if( m_CharBaseScr.attackTarget == null || m_CharBaseScr.attackTarget.m_CharState == CHAR_STATE.DEATH )
            SetNpcState(eNpcState.ePEACE);
        else 
        {
            m_CharBaseScr.m_currSkillIndex = GetNextSkill_Index();
            SetNpcState(eNpcState.eATTACK_READY);
        }
    }

    private void SetAttackReadyTraceState()
    {
        if ((InGameManager.instance.m_bOffLine == true || InGameManager.instance.m_bHostPlay == true) && (m_CharBaseScr.m_BuffController.GetCharDisable(BuffController.eBuffCharDisable.eMOVE) > 0) ||
            m_CharBaseScr.gameObject.activeInHierarchy == false)
        {
            SetPeaceState();
            return;
        }
        
        NpcAction kBaseAttackAction = m_Ai_ActionPool.ObjectNew();
        float[] fMovePos = new float[] { m_CharBaseScr.attackTarget.transform.position.x, m_CharBaseScr.attackTarget.transform.position.y, m_CharBaseScr.attackTarget.transform.position.z };
        float[] fTargetInfo = new float[] { m_CharBaseScr.m_AttackInfos[m_CharBaseScr.m_currSkillIndex].skillinfo.skill_Dist * 0.5f};
        kBaseAttackAction.InitAction(this, NpcAction.eNpcActionGrade.eBASE, (int)NpcAction.eNPpcActionType.eMOVE, fMovePos, (int)PDT_TARGET_TYPE.ePDT_TARGET_TYPE_CURRENT, fTargetInfo, true, null);
        m_ActionController.AddAction(kBaseAttackAction);

        SetNpcState(eNpcState.eATTACK_READY_TRACE);
    }

    public void SetMoveState(Vector3 p_TargetPos, float p_fStopDistance = 0.0f)
    {
        if (m_CharBaseScr.m_CharState == CHAR_STATE.DEATH && m_CharBaseScr.charUniqID != PlayerManager.MYPLAYER_INDEX && m_NavMeshAgent.enabled == false)
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
            SetNpcState(eNpcState.eMOVE);
        }
        else
        {
            m_Ai_ActionPool.ObjectDelete(kBaseAttackAction);
        }
    }
    
    public bool SetFuryState(long p_iUseSkillIndex)
    {
        if (m_CharBaseScr.m_CharState == CHAR_STATE.DEATH ||
            m_CharBaseScr.gameObject.activeInHierarchy == false)
            {
                return false;
            }

        NpcAction newBaseAttackAction = m_Ai_ActionPool.ObjectNew();
        float[] fSkillIndex = new float[] { (float)p_iUseSkillIndex };
        float[] fTargetInfo = new float[] { 0.0f };
        newBaseAttackAction.InitAction(this, NpcAction.eNpcActionGrade.eTIMETRIGER, (int)NpcAction.eNPpcActionType.eUSESKILL, fSkillIndex, (int)PDT_TARGET_TYPE.ePDT_TARGET_TYPE_SKILLRULE, fTargetInfo, false, null);


        m_ActionController.SuperCancelAction(newBaseAttackAction);

        SetNpcState(eNpcState.eGUARD);

        return true;
    }

    public bool SetNormalAttackState(int p_iUseSkillIndex, NpcActionController p_ExtraController = null)
    {
        if (m_CharBaseScr.m_CharState == CHAR_STATE.DEATH ||
            ((InGameManager.instance.m_bOffLine == true || InGameManager.instance.m_bHostPlay == true) && (m_CharBaseScr.m_BuffController.GetCharDisable(BuffController.eBuffCharDisable.eATTACK) > 0)) ||
            m_CharBaseScr.gameObject.activeInHierarchy == false)
            return false;

        if (p_ExtraController == null )
        {
            NpcAction newBaseAttackAction = m_Ai_ActionPool.ObjectNew();
            float[] fSkillIndex = new float[] { (float)p_iUseSkillIndex };
            float[] fTargetInfo = new float[] { 0.0f };
            newBaseAttackAction.InitAction(this, NpcAction.eNpcActionGrade.eBASE, (int)NpcAction.eNPpcActionType.eUSEBASEATTACK, fSkillIndex, (int)PDT_TARGET_TYPE.ePDT_TARGET_TYPE_SKILLRULE, fTargetInfo, false, null);

            if (m_ActionController.ChangeActionOrSkip(newBaseAttackAction))
            {
                SetNpcState(eNpcState.eSKILL);
            }
            else
            {
                m_Ai_ActionPool.ObjectDelete(newBaseAttackAction);
            }
        }
        else
        {
            NpcAction newBaseAttackAction = m_Ai_ActionPool.ObjectNew();
            float[] fSkillIndex = new float[] { (float)p_iUseSkillIndex };
            float[] fTargetInfo = new float[] { 0.0f };
            newBaseAttackAction.InitAction(this, NpcAction.eNpcActionGrade.eBASE, (int)NpcAction.eNPpcActionType.eUSEBASEATTACK, fSkillIndex, (int)PDT_TARGET_TYPE.ePDT_TARGET_TYPE_SKILLRULE, fTargetInfo, false, null);

            p_ExtraController.AddAction(newBaseAttackAction);
        }

        return false;
    }
    public void SetAttackReadyCompleteState( long p_iSkillCode )
    {
//        SetNavMeshAgent(false);
        if (Check_IsAttackAble() || InGameManager.instance.m_bOffLine == false )
        {
            if(AttackReady_SkillCheck(p_iSkillCode))
            {
                SetNpcState(eNpcState.eATTACK);
                return;
            }

            if ( InGameManager.instance.m_bOffLine == true && m_CharBaseScr.attackTarget != null && m_CharBaseScr.attackTarget != m_CharBaseScr)
                m_CharBaseScr.LookAtY(m_CharBaseScr.attackTarget.m_MyObj.transform.position);
            NpcAction kBaseAttackAction = m_Ai_ActionPool.ObjectNew();
            float[] fSkillIndex = new float[] { (float)p_iSkillCode };
            float[] fTargetInfo = new float[] { 0.0f };
            kBaseAttackAction.InitAction(this, NpcAction.eNpcActionGrade.eBASE, (int)NpcAction.eNPpcActionType.eUSEBASEATTACK, fSkillIndex, (int)PDT_TARGET_TYPE.ePDT_TARGET_TYPE_CURRENT, fTargetInfo, false, null);

            if (m_ActionController.ChangeActionOrSkip(kBaseAttackAction))
            {
                SetNpcState(eNpcState.eATTACK);
            }
            else
                m_ActionController.AddAction(kBaseAttackAction);
        }
        else 
        {
            SetAttackReadyState();
        }
    }

    public bool SetDamageState(eREACTION_TYPE type)
    {
        if (m_PlayerNpcProp.Battle_Type != eNPC_BATTLE_TYPE.eBATTLE)
            return false;

        NpcAction kBaseAttackAction = m_Ai_ActionPool.ObjectNew();
        float[] fReactionType = new float[] { (float)type };
        float[] fTargetInfo = new float[] { 0.0f };
        kBaseAttackAction.InitAction(this, NpcAction.eNpcActionGrade.eBASE, (int)NpcAction.eNPpcActionType.eBEATTACKTED, fReactionType, (int)PDT_TARGET_TYPE.ePDT_TARGET_TYPE_SELF, fTargetInfo, false, null);

        if (m_PlayerNpcProp.Hit_Ani == 1 && type == eREACTION_TYPE.ePHYGICAL_DAMAGE)
        {
            m_Ai_ActionPool.ObjectDelete(kBaseAttackAction);
            return false;
        }
        if (m_ActionController.ChangeActionOrSkip(kBaseAttackAction))
        {
            SetNpcState(eNpcState.eBEATTACKED);
            return true;
        }
        else 
        {
            m_Ai_ActionPool.ObjectDelete(kBaseAttackAction);
        }
        return false;
    }

    public void SetDieState( DIE_TYPE p_eDieType )
    {
        ClearTimeTriggerAction();

        m_DieVar.Init();

        switch (m_PlayerNpcProp.Npc_Class)
        {
            case eNPC_CLASS.eNONE:
                //DIE_DELAY_TIME = 5.0f;
                DIE_DELAY_TIME = 2.0f;
                break;
            case eNPC_CLASS.eNORMAL:
            case eNPC_CLASS.eELITE: 
            case eNPC_CLASS.eBOSS:  
            case eNPC_CLASS.eRAIDBOSS: 
                DIE_DELAY_TIME = 1.0f;
                break;
        }
        
        SetNpcState(eNpcState.eDIE);
        
        m_ActionController.RemoveAllAction();
              

        // No more Battle
        m_bBattleStart = false;

        if( m_NavMeshAgent.enabled )
            SetNavMeshAgent(false);

        m_NavMeshAgent.enabled = false;

        if (GetNpcProp().Npc_Type == eNPC_TYPE.eDESTROYABLE_OBJ)
        {
            NavMeshObstacle obstacle = m_CharBaseScr.GetComponent<NavMeshObstacle>();
            if (obstacle != null)
                obstacle.enabled = false;
        }

        m_uAggroData.Clear();

        m_eEndMotionState = MOTION_STATE.eMOTION_NONE;

        switch ( p_eDieType )
        {
            case DIE_TYPE.eDIE_NORMAL:
                m_DieVar.m_bStandingDeath = false;
                break;
            case DIE_TYPE.eDIE_STAND:
                m_DieVar.m_bStandingDeath = true;
                break;
        }

        m_NextAggroChar = null;
    }
    public void SetRevival()
    {
        ClearTimeTriggerAction();

        m_ActionController.RemoveAllAction();
        m_bBattleStart = false;
		m_NavMeshAgent.enabled = false;
		if (m_NavMeshAgent.enabled)
            SetNavMeshAgent(false);


        m_uAggroData.Clear();
        m_eEndMotionState = MOTION_STATE.eMOTION_NONE;
        m_NextAggroChar = null;
    }

    public void SetStopState()
    {
        m_bBattleStart = false;
        ClearTimeTriggerAction();

        m_ActionController.RemoveAllAction();

        m_uAggroData.Clear();
        m_eEndMotionState = MOTION_STATE.eMOTION_NONE;
        m_CharBaseScr.attackTarget = null;
        SetNpcState(eNpcState.eNONE);
    }
    #endregion	SET_FUNCTION



    public int GetNextSkill_Index()
    {
        int skill_idx = 0;

        if (m_PlayerNpcProp.link_SkillCode.Count == 1) // exception 
            return m_PlayerNpcProp.link_SkillCode[0];

        skill_idx = m_PlayerNpcProp.link_SkillCode[UtilManager.instance.Random(0, m_PlayerNpcProp.link_SkillRate.Count)];

        if (skill_idx == m_CharBaseScr.m_currSkillIndex && UtilManager.instance.RandomPercent(50) )
        {
            skill_idx = m_PlayerNpcProp.link_SkillCode[UtilManager.instance.Random(0, m_PlayerNpcProp.link_SkillRate.Count)];
        }

        return skill_idx;
    }

    #endregion	GET_FUNCTION

    #region 	CHECK_FUNCTION
    public bool Check_IsThereTargets()
    {
        if (m_NaviGation != null)
        {
            if (m_PrimaryTarget != null && m_PrimaryTarget.m_CharState == CHAR_STATE.ALIVE)
            {
                if (m_PrimaryTarget != m_NaviGation.m_target && m_NaviGation.m_target != null
                    && (m_CharBaseScr.m_MyObj.transform.position - m_PrimaryTarget.m_MyObj.transform.position).sqrMagnitude > 4.666666f)
                {
                    m_PrimaryTarget = m_NaviGation.m_target;
                }
                m_CharBaseScr.SetAttackTarget(m_PrimaryTarget, false);
                return true;
            }
            else
                m_PrimaryTarget = null;

            if (m_NaviGation.m_target != null && m_NaviGation.m_target.m_CharState == CHAR_STATE.ALIVE)
            {
                if (m_PrimaryTarget == null)
                {
                    m_PrimaryTarget = m_NaviGation.m_target;
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
            for (int i = 0; i < kTarget.Count; ++i)
            {
                float fDistance = (kTarget[i].m_MyObj.position - m_CharBaseScr.m_MyObj.position).sqrMagnitude;
                if (kTarget[i].m_CharState == CHAR_STATE.ALIVE && fMaxDist > fDistance)
                {
                    fMaxDist = fDistance;
                    iTargetIndex = i;
                }
            }
            if (iTargetIndex >= 0)
            {
                m_CharBaseScr.SetAttackTarget(kTarget[iTargetIndex], false);
                return true;
            }
        }
        return false;        
    }
    public bool Check_IsFindTargets()
	{
		List<CharacterBase> kTarget = FindTarget(eTargetState.eHOSTILE, null, 1);
		int iTargetIndex = -1;
		float fMaxDist = float.MaxValue;
		for (int i = 0; i < kTarget.Count; ++i)
		{
			float fDistance = (kTarget[i].m_MyObj.position - m_CharBaseScr.m_MyObj.position).sqrMagnitude;
			if (kTarget[i].m_CharState == CHAR_STATE.ALIVE && fMaxDist > fDistance)
			{
				fMaxDist = fDistance;
				iTargetIndex = i;
			}
		}
		if (iTargetIndex >= 0)
		{
			m_CharBaseScr.SetAttackTarget(kTarget[iTargetIndex], false);
			return true;
		}
		return false;
	}

	public override List<CharacterBase> FindTarget( eTargetState p_TargetState , eCharacter[] p_TargetClass , int p_AggroRank )
    {
        List<CharacterBase> targets = new List<CharacterBase>();

        switch (p_TargetState)
        {
            case eTargetState.eNONE:
                targets.AddRange(FindAllPlayer());
                targets.AddRange(FindAllNpc());
                break;
            case eTargetState.eHOSTILE:

#if NONSYNC_PVP
                if (SceneManager.instance.GetCurGameMode() == SceneManager.eGAMEMODE.eNONSYNC_PVP)
                {
                    //PC 1 Side
                    targets.AddRange(FindEnemyPlayer());
                    //Debug.Log("FindTarget() this = "+this);

                    for (int i = 0; i < NpcManager.instance.m_PlayerSummonList.Count; ++i)
                    {
                        //if (NpcManager.instance.m_PlayerSummonList[i].Equip)
                        if(SummonManager.instance.GetMySummonEquipSlotIndex(NpcManager.instance.m_PlayerSummonList[i].nContentIndex) >= 0)
                        {
                            targets.Add(NpcManager.instance.m_PlayerSummonList[i].summonCBase);
                        }
                    }
                }
                else
#endif
                {
                    List<CharacterBase> npcList = FindAllNpc();
                    for (int npcs = 0; npcs < npcList.Count; ++npcs)
                    {
                        if (npcList[npcs].m_CharAi != null &&
                            npcList[npcs].m_CharAi.GetNpcProp().Identity_Fnd == eTargetState.eHOSTILE &&
                            npcList[npcs].m_CharAi.GetNpcProp().Npc_Type != eNPC_TYPE.eZONEWALL_OBJ && npcList[npcs].gameObject.activeSelf == true)
                            targets.Add(npcList[npcs]);
                    }
                }
                break;
            case eTargetState.eFRIENDLY:
                targets.AddRange(FindAllPlayer());
                break;
        }
        
        if (p_TargetClass != null)
        {
            for (int i = targets.Count - 1; i >= 0; --i)
            {
                bool bDelete = true;
                for (int j = 0; j < p_TargetClass.Length; ++j)
                {
                    if (p_TargetClass[j] == targets[i].m_CharacterType)
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
        for (int i = 0; i < PlayerManager.instance.m_PlayerInfo.Count; i++)
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

    private List<CharacterBase> FindAllNpc()
    {
        return NpcManager.instance.GetNpcTargetsBySpwaned();
    }

    private List<CharacterBase> FindEnemyNpc()
    {
        List<CharacterBase> targets = new List<CharacterBase>();
        List<CharacterBase> spwanedNpcs = NpcManager.instance.GetNpcTargetsBySpwaned();

        for(int i=0 ; i<spwanedNpcs.Count ; ++i)
        {
            NpcAI npcAi = (NpcAI)spwanedNpcs[i].m_CharAi;
            if(npcAi != null)
            {
                if (npcAi.GetNpcProp().Identity_Fnd == eTargetState.eFRIENDLY)
                {
                    targets.Add(spwanedNpcs[i]);
                }
            }
        }
        return targets;
    }
     
    public bool Check_IsAttackAble()
    {
        if (( InGameManager.instance.m_bOffLine == true || InGameManager.instance.m_bHostPlay == true) && (m_CharBaseScr.m_BuffController.GetCharDisable(BuffController.eBuffCharDisable.eATTACK) > 0))
            return false;

        if( m_CharBaseScr.m_currSkillIndex == 0 )
            return false;

        return true;
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
        return m_PlayerNpcProp;
    }

    public override bool HasPath()
    {
        if (!m_NavMeshAgent.pathPending && m_NavMeshAgent.enabled)
        {
            if (m_NavMeshAgent.remainingDistance <= m_NavMeshAgent.stoppingDistance)
            {
                if (!m_NavMeshAgent.hasPath || m_NavMeshAgent.velocity.sqrMagnitude == 0f)
                {
                    return false;
                }
            }
        }
        return true;
    }
    public override void NavMeshAgentMove(Vector3 p_Offset)
    {
        m_NavMeshAgent.Move(p_Offset);
    }
    public override NavMeshPathStatus NavPathState()
    {
        return m_NavMeshAgent.path.status;
    }
    #endregion

    #region 	PRIVATE_FUNCTION

    private void ClearPatternAction()
    {
        if (m_NpcAI_Pattern != null)
        {
            m_NpcAI_Pattern.Clear();
        }
    }
    private void ClearTimeTriggerAction()
    {
        if (m_NpcAI_TimeTrigger != null)
        {
            m_NpcAI_TimeTrigger.Clear();
        }
    }
    private void UpdateTimeTriggerAction()
    {
        if (m_npcState == eNpcState.eDIE || m_CharBaseScr.attackTarget == null)
            return;

        if (m_NpcAI_TimeTrigger != null)
        {
            m_NpcAI_TimeTrigger.UpdatePatern();
        }
    }
    private void UpdatePatternAction()
    {
        if (m_npcState == eNpcState.eDIE || m_CharBaseScr.attackTarget == null)
            return;
        
        if( m_NpcAI_Pattern != null )
        {
            if (m_ActionController.GetCountActionGrade(NpcAction.eNpcActionGrade.eTIMETRIGER) > 0)
                return;

            m_NpcAI_Pattern.UpdatePatern();
        }
    }
    private void UpdateBasicAction()
    {
        if ((m_PlayerNpcProp.Battle_Type != eNPC_BATTLE_TYPE.eBATTLE || m_PlayerNpcProp.Ai_Type == 0) && m_npcState != eNpcState.eDIE)
        { 
            return;
        }

        switch (m_npcState)
        {
            case eNpcState.ePEACE:
                Proc_PeaceState();
                break;
            case eNpcState.eGUARD:
                Proc_GuardState();
                break;
            case eNpcState.eGUARD_TRACE:
                Proc_GuardTraceState();
                break;
            case eNpcState.eGUARD_MOVING:
                Proc_GuardMovingState();
                break;
            case eNpcState.eATTACK_READY:
                Proc_AttackReady();
                break;
            case eNpcState.eATTACK_READY_TRACE:
                Proc_AttackReadyTraceState();
                break;
            case eNpcState.eATTACK_READY_COMPLETE:
                Proc_AttackReady_Complete();
                break;
            case eNpcState.eATTACK:
                Proc_AttackState();
                break;
            case eNpcState.eMOVE:
                Proc_MoveState();
                break;
            case eNpcState.eSKILL:
                Proc_Skill();
                break;
            case eNpcState.eSPWAN_STANDBY:
                Proc_SpwaneStanbyState();
                break;
            case eNpcState.eBEATTACKED:
                Proc_BeAttackState();
                break;
            case eNpcState.eDIE:
                Proc_DieState();
                break;
        }
    }

    #region 	PROC_FUNCTION
    private void Proc_PeaceState()
    {

        if (EventTriggerManager.instance.Cutscene_Boss != null)
        {
            if (EventTriggerManager.instance.Cutscene_Boss.State == CinemaDirector.Cutscene.CutsceneState.Playing)
            {
                return;
            }
        }
        if(Check_IsThereTargets())
        {
            SetAttackReadyState();

            m_bBattleStart = true;

        }
        else
        {
            if (
                SceneManager.instance.GetCurGameMode() == SceneManager.eGAMEMODE.eMODE_CHAPTER
#if DAILYDG
                || SceneManager.instance.GetCurGameMode() == SceneManager.eGAMEMODE.eMODE_DAILY
#endif
            )
            {
                m_bBattleStart = false;

                if (m_NaviGation != null)
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

    private void Proc_GuardState()
    {
        if (m_CharBaseScr.attackTarget == null || m_CharBaseScr.attackTarget.m_CharState == CHAR_STATE.DEATH)
        {
            SetNpcState(eNpcState.ePEACE);
            return;
        }

        if (!GetAvailableRange(m_PlayerNpcProp.Alert_Distance_Max))
        {
            float fRandDist = UtilManager.instance.Random(m_PlayerNpcProp.Alert_Distance_Min, m_PlayerNpcProp.Alert_Distance_Max);
            SetGuardTraceState(m_CharBaseScr.attackTarget.m_MyObj.transform.position + fRandDist * (m_CharBaseScr.m_MyObj.transform.position - m_CharBaseScr.attackTarget.m_MyObj.transform.position).normalized, fRandDist);
        }
        else
        {
            if (EventTriggerManager.instance.IsTalking == true)
            {
                return;
            }
            else
            {
                if ( m_fAIWaitTimer + m_LinkSkillCoolTime <= 0.0f)
                {
                    m_fSideMoveTimer = m_fAIWaitTimer = 0.0f;

                    SetAttackReadyState();
                }
                else if (m_CharBaseScr.attackTarget != null)
                {
                    m_CharBaseScr.SetAttackTarget(m_CharBaseScr.attackTarget);

                    if (m_fSideMoveTimer >= 0.0f)
                    {
                        m_fSideMoveTimer -= Time.deltaTime;
                    }
                }
                else
                {
                    SetPeaceState();
                }
            }
        }
    }

    private void Proc_GuardTraceState()
    {
        if (m_ActionController.GetCurActionState() == NpcAction.eNpcActionState.eEND)
        {
            if (m_CharBaseScr.attackTarget == null || m_CharBaseScr.attackTarget.m_CharState == CHAR_STATE.DEATH)
            {
                SetNpcState(eNpcState.ePEACE);
                return;
            }
            SetGuardState();
        }
        else
        {
            m_fGuardMoveTimer -= Time.deltaTime;
            if (m_CharBaseScr.attackTarget != null && 
                m_CharBaseScr.attackTarget.m_CharState != CHAR_STATE.DEATH &&
                m_fGuardMoveTimer <= 0.0f)
            {
                float fDist = (m_CharBaseScr.attackTarget.m_MyObj.position - m_CharBaseScr.m_MyObj.position).sqrMagnitude;
                float fMaxDist = (m_PlayerNpcProp.Alert_Distance_Max*2.0f);

                if( fDist > fMaxDist * fMaxDist )
                {
                    SetGuardState();
                }
            }
        }
    }

    private void Proc_GuardMovingState()
    {
        m_fGuardMoveTimer -= Time.deltaTime;
        if (m_ActionController.GetCurActionState() == NpcAction.eNpcActionState.eEND || m_fGuardMoveTimer < 0.0f)
        {
            if (m_CharBaseScr.attackTarget == null || m_CharBaseScr.attackTarget.m_CharState == CHAR_STATE.DEATH)
            {
                SetNpcState(eNpcState.ePEACE);
                return;
            }
            SetGuardState();
        }
    }

    private void Proc_AttackReady()
    {
        if (m_CharBaseScr.attackTarget == null || m_CharBaseScr.attackTarget.m_CharState == CHAR_STATE.DEATH)
        {
            SetNpcState(eNpcState.ePEACE);
            return;
        }

        m_CharBaseScr.SetAttackTarget(m_CharBaseScr.attackTarget);

        if (!GetAvailableRange(m_CharBaseScr.m_AttackInfos[m_CharBaseScr.m_currSkillIndex].skillinfo.skill_Dist))
        {
            SetAttackReadyTraceState();
        }
        else
        {
            m_CharBaseScr.SetAttackTarget(m_CharBaseScr.attackTarget);
            SetAttackReadyCompleteState(m_CharBaseScr.m_currSkillIndex);
        }
    }

    private void Proc_AttackReadyTraceState()
    {
        if (Check_IsThereTargets() && m_CharBaseScr.m_currSkillIndex != 0)
        {
            if (GetAvailableRange(Mathf.Max(m_CharBaseScr.m_AttackInfos[m_CharBaseScr.m_currSkillIndex].skillinfo.skill_Dist, m_CharBaseScr.m_AttackInfos[m_CharBaseScr.m_currSkillIndex].skillinfo.area_Size)))
            {
                SetAttackReadyCompleteState(m_CharBaseScr.m_currSkillIndex);
            }
            else
            {
                if (m_ActionController.GetCurActionState() == NpcAction.eNpcActionState.eEND)
                {
                    if (m_CharBaseScr.attackTarget == null || m_CharBaseScr.attackTarget.m_CharState == CHAR_STATE.DEATH)
                    {
                        SetNpcState(eNpcState.ePEACE);
                        return;
                    }
					else
					{
						if(m_ActionController.GetAction() == null)
						{
							SetPeaceState();
						}
					}

				}
			}
        }
        else
        {
            SetPeaceState();
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

    private void Proc_MoveState()
    {
        if (m_eAutoPlayState == eAutoProcessState.ePlay)
        {
            if (Check_IsThereTargets())
            {
                SetPeaceState();
                return;
            }
            else
            {
                float fDistance = 1.5f + m_CharBaseScr.GetRadius();
                RaycastHit hit;
                Ray ray = new Ray(m_CharBaseScr.m_MyObj.position, m_CharBaseScr.m_MyObj.forward);

                if (Physics.SphereCast(ray, m_NavMeshAgent.radius, out hit, fDistance, 1 << LayerMask.NameToLayer("NPC")) == true)
                {
                    if (hit.transform != null && hit.transform.CompareTag("NPC") == true)    
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

    private void Proc_Skill()
    {
#if UNITY_EDITOR
        if (SceneManager.instance.GetCurGameMode() == SceneManager.eGAMEMODE.eNPC_TEST_TOOL)
        {
            if (m_ActionController.GetCurActionState() == NpcAction.eNpcActionState.eEND)
            {
                if (m_bBattleStart == false)
                    SetPeaceState();
                else
                    SetGuardState();
            }
        }
#endif
    }

    private void Proc_BeAttackState()
    {
        if (m_ActionController.GetCurActionState() == NpcAction.eNpcActionState.eEND)
        {
            if (m_bBattleStart == false)
            {
                BattleBegin();
            }
            else
                SetGuardState();
        }
    }
    
    private void Proc_SpwaneStanbyState()
    {
        if (m_CharBaseScr.ksAnimation.GetAnimationClip("Spawn") != null)
        {
            if (m_CharBaseScr.ksAnimation.bEventEnd)
            {
                m_CharBaseScr.SetChangeMotionState(MOTION_STATE.eMOTION_IDLE);
                //SetPeaceState();
            }
        }
        else
        {
            if (m_CharBaseScr.m_spwanTime > ObjectState_SpwaneStanby.NpcspwanTime)
            {
                m_CharBaseScr.SetChangeMotionState(MOTION_STATE.eMOTION_IDLE);
                //SetPeaceState();
            }
        }
    }
    private void Proc_DieState()
    {
        Vector3 tmpPos = new Vector3();

        if (m_DieVar.m_bGowayWait)
        {
            if (Time.time - m_DieVar.m_GowayWaitTime > DIE_DELAY_TIME)
            {
                m_DieVar.m_bGowayWait = false;

                m_DieVar.m_bGowayStart = true;

                m_CharBaseScr.m_BuffController.End();
                                    
                switch (m_PlayerNpcProp.Npc_Class)
                {
                    case eNPC_CLASS.eNONE: 
                        m_DieVar.m_fGowayTime = 5.0f;
                        break;
                    case eNPC_CLASS.eNORMAL:   
                    case eNPC_CLASS.eELITE:    
                    case eNPC_CLASS.eBOSS:     
                    case eNPC_CLASS.eRAIDBOSS: 
                        m_DieVar.m_fGowayTime = 1.5f;
                        
                        if (m_CharBaseScr.m_CharacterDamageEffect.m_MeshRenderer != null)
                        {
                            if (m_DieMaterialProc != null)
                            {
                                m_DieMaterialProc.Clear();
                            }
                            for (int i = 0; i < m_CharBaseScr.m_CharacterDamageEffect.m_MeshRenderer.Length; ++i)
                            {
                                if (m_DeathMaterial[i] != null)
                                {
                                    m_CharBaseScr.m_CharacterDamageEffect.m_MeshRenderer[i].material = m_DeathMaterial[i];

                                    m_DieMaterialProc.Add(m_CharBaseScr.m_CharacterDamageEffect.m_MeshRenderer[i].material);
                                    m_DieMaterialProc[m_DieMaterialProc.Count - 1].SetFloat("_Dissolve", 0.0f);
                                    m_DieMaterialProc[m_DieMaterialProc.Count - 1].SetTexture("_m", m_DeathTexture);
                                    m_DieMaterialProc[m_DieMaterialProc.Count - 1].SetTextureScale("_m", new Vector2(3f, 3f));

                                    m_DieMaterialProc[m_DieMaterialProc.Count - 1].SetTexture("_Ramp", m_DeathTexture2);
                                    m_DieMaterialProc[m_DieMaterialProc.Count - 1].SetTextureScale("_Ramp", new Vector2(3f, 3f));

                                    m_DieMaterialProc[m_DieMaterialProc.Count - 1].SetFloat("_OutlineWidth", 0.1f);
                                    m_DieMaterialProc[m_DieMaterialProc.Count - 1].SetFloat("_Emission_Power", 1.0f);
                                    m_DieMaterialProc[m_DieMaterialProc.Count - 1].SetFloat("_Diffuse_power", 2.0f);

                                    m_DieMaterialProc[m_DieMaterialProc.Count - 1].SetFloat("_RimPower", 0.5f);
                                }
                            }
                        }
                        break;
                }

            }
        }

        if (m_DieVar.m_bGowayStart)
        {
            switch (m_PlayerNpcProp.Npc_Class)
            {
                case eNPC_CLASS.eNONE:  
                    tmpPos = m_NpcObj.position;
                    tmpPos.y = m_NpcObj.position.y - 1.666f * Time.deltaTime;
                    m_NpcObj.position = tmpPos;
                    break;
                case eNPC_CLASS.eNORMAL: 
                case eNPC_CLASS.eELITE:  
                case eNPC_CLASS.eBOSS:   
                case eNPC_CLASS.eRAIDBOSS: 
                    
                    if (m_CharBaseScr.m_CharacterDamageEffect.m_MeshRenderer != null)
                    {
                        float fBurnningTime = (1.0f - (m_DieVar.m_fGowayTime / m_DieVar.m_fGowayTime_Dissolve)) * 3.0f;

                        if (fBurnningTime > 6.0f)
                            fBurnningTime = 6.0f;
                        for (int i = 0; i < m_CharBaseScr.m_CharacterDamageEffect.m_MeshRenderer.Length; ++i)
                        {
                            if (m_DieMaterialProc.Count > i)
                            {
                                if (m_DieMaterialProc[i] == null)
                                {
                                    Debug.Log("m_DieMaterialProc == null~~~~~~~~~~~~~~~~~~~~~~~~~~");
                                }
                                else
                                {
                                    m_DieMaterialProc[i].SetFloat("_Dissolve", fBurnningTime);
                                }
                            }
                        }
                    }
                    else
                    {
                        tmpPos = m_NpcObj.position;
                        tmpPos.y = m_NpcObj.position.y - 2.0f * Time.deltaTime;
                        m_NpcObj.position = tmpPos;
                    }
                    break;
            }

            m_DieVar.m_fGowayTime -= Time.deltaTime;

            if (m_DieVar.m_fGowayTime < 0.0f)
            {
                m_DieVar.m_bGowayStart = false;
                m_CharBaseScr.m_CharacterDamageEffect.Spawn_RecoveryMaterial();

                m_NpcObj.gameObject.SetActive(false);
                SetNpcState(eNpcState.eNONE);

                m_CharBaseScr.attackTarget = null;

                if (m_DieMaterialProc != null)
                {
                    for (int i = 0; i < m_DieMaterialProc.Count; i++)
                    {
                        Destroy(m_DieMaterialProc[i]);
                    }
                    m_DieMaterialProc.Clear();
                }

            }
        }
             
    }

    public void BattleBegin()
    {
        if (m_bBattleStart == false)
        {
            m_bBattleStart = true;

            if (m_CharBaseScr.attackTarget != null)
            {
                GatherAggro(m_CharBaseScr.attackTarget.gameObject, m_PlayerNpcProp.First_Agro, true);
            }
        }
    }

    public void BattleEnd()
    {
        m_bBattleStart = false;
        m_bBattleStart = true;
    }

    public void GatherAggro( GameObject p_Source , uint p_iAggroValue , bool p_bImmedately = false)
    {
        bool bNewAggroTarget = true;
        for( int i = 0 ; i < m_uAggroData.Count ; ++i )
        {
            if( m_uAggroData[i].Key == p_Source )
            {
                m_uAggroData[i] = new KeyValuePair<GameObject,uint>(p_Source,m_uAggroData[i].Value + p_iAggroValue);
                bNewAggroTarget = false;
                break;
            }
        }
        if (bNewAggroTarget )
            m_uAggroData.Add( new KeyValuePair<GameObject,uint>(p_Source,p_iAggroValue));

        uint kiMax = 0;
        GameObject kAggroChar = null;
        for (int i = 0; i < m_uAggroData.Count; ++i )
        {
            if (m_uAggroData[i].Value > kiMax)
            {
                kiMax = m_uAggroData[i].Value;
                kAggroChar = m_uAggroData[i].Key;
            }
        }

        if (m_NextAggroChar == kAggroChar)
        {
            return; 
        }

        if (p_bImmedately || m_CharBaseScr.attackTarget == null)
        {
            m_uAggroData.Sort(AggroMemeberComparer);
            m_NextAggroChar = null;
        }
        else
        {
            m_NextAggroChar = kAggroChar;
            if (m_NextAggroChar != m_CharBaseScr.attackTarget.gameObject)
            {
                StopAllCoroutines();
                StartCoroutine("AggroTargetChange");
            }
        }
    }

    public void DisperseAggro( GameObject p_Source , uint p_iAggroValue  )
    {
        for (int i = 0; i < m_uAggroData.Count; ++i)
        {
            if (m_uAggroData[i].Key == p_Source)
            {
                if (p_iAggroValue == 0)
                    m_uAggroData[i] = new KeyValuePair<GameObject, uint>(p_Source, 0);
                else
                    m_uAggroData[i] = new KeyValuePair<GameObject, uint>(p_Source, m_uAggroData[i].Value - p_iAggroValue);

                if (m_CharBaseScr.attackTarget != null && p_Source == m_CharBaseScr.attackTarget.gameObject)
                    m_uAggroData.Sort(AggroMemeberComparer);

                break;
            }
        }
    }
    public void DisperseAllAggro()
    {
        m_uAggroData.Clear();
        m_CharBaseScr.SetAttackTarget(null,false);
    }

    public void SetCollisionDetection( bool p_bEnable )
    {
        m_NavMeshAgent.enabled = p_bEnable;
    }

    public override bool GetCollisionDetection()
    {
        return m_NavMeshAgent.enabled;
    }

    public void SetNaveMeshEnabled(bool sw)
    {
        m_NavMeshAgent.enabled = sw;
    }

    public eNpcState GetNpcState()
    {
        return m_npcState;
    }

    public float GetRemainSkillCoolTime()
    {
        return m_LinkSkillCoolTime;
    }

    public bool IsSkillCoolTime()
    {
        return m_LinkSkillCoolTime > 0.0f;
    }

    public void StartSkillCoolTime(float p_fCoolTime = 0.0f)
    {
       m_LinkSkillCoolTime = p_fCoolTime;
    }

    public void StopSkillCoolTime()
    {
        m_LinkSkillCoolTime = 0.0f;
    }

    private void CheckSkillCoolTime()
    {
        if (IsSkillCoolTime())
        {
            m_LinkSkillCoolTime -= Time.deltaTime;
            if (m_LinkSkillCoolTime <= 0.0f)
            {
                StopSkillCoolTime();
            }
        }
        if (m_fAIWaitTimer > 0.0f)
            m_fAIWaitTimer -= Time.deltaTime;
    }

    #endregion	PROC_FUNCTION

    #endregion	PRIVATE_FUNCTION




    #region     ANIM_EVENT_FUNCTION
    #endregion  ANIM_EVENT_FUNCTION

    #region PROPERTY    
    public string NpcModelDataRoute
    {
        get {   return m_NpcModelDataRoute;     }
        set {   m_NpcModelDataRoute = value;    }
    }

    public string NpcModelDataName
    {
        get { return m_NpcModelDataName; }
        set { m_NpcModelDataName = value; }
    }
    

    public int nNpcGroup
    {
        get { return m_nNpcGroup; }
        set { m_nNpcGroup = value; }
    }

    public int nFreeActivityRadius
    {
        get { return m_nFreeActivityRadius; }
        set { m_nFreeActivityRadius = value; }
    }

    public Vector3 v3SourcePosition
    {
        get { return m_v3SourcePosition; }
        set { m_v3SourcePosition = value; }
    }
    #endregion PROPERTY

    #region COROUTINE
    private IEnumerator AggroTargetChange()
    {
        yield return new WaitForSeconds(3);

        uint kiMax = 0;
        for (int i = 0; i < m_uAggroData.Count; ++i )
        {
            if (m_uAggroData[i].Value > kiMax)
            {
                kiMax = m_uAggroData[i].Value;
            }
        }
        m_uAggroData.Sort(AggroMemeberComparer);
        m_NextAggroChar = null;
    }
    #endregion COROUTINE

}
