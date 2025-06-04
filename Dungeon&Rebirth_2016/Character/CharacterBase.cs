//#define _EFFECT_TOOL_
//#define _DAMAGE_
#define _DEBUG_CHARACTER_BASE_REQUEST
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using KSPlugins;

public enum MOTION_STATE : byte
{
    eMOTION_NONE = 0,    
    eMOTION_IDLE,        
    eMOTION_IDLE_STAND,
    eMOTION_IDLE_WALK,   
    eMOTION_SIDE_WALK,   
    eMOTION_WALK,
    eMOTION_RUN,         
    eMOTION_BEATTACKED,  
    eMOTION_DIE,         
    eMOTION_REVIVAL,

    eMOTION_ATTACK_COMBO,
    eMOTION_SKILL,       
    eMOTION_CATING,      

    //eMOTION_PEACE,     
    eMOTION_TRACE,       
    eMOTION_ATTACK_IDLE, 
    eMOTION_ATTACK_READY,
    eMOTION_NPCATTACK,   
    eMOTION_SIDEWALK,    
    eMOTION_SPWAN,       
    eMOTION_WARP_IN,
    eMOTION_WARP_OUT,

    eMOTION_ETC,         
    //ZUNGHOON ADD 
    eMOTION_SPWAN_STANDBY,
};

public enum DIE_TYPE : byte
{
    eDIE_NORMAL = 0,
    eDIE_STAND,
    eDIE_BOSS
}

public enum CHAR_STATE : byte
{
    ALIVE = 0,
    DEATH,
    //WAIT_LIVE
}

public enum ANIMATION_STATE : byte
{
    eANIM_START = 0,
    eANIM_LOOP,
    eANIM_END
}


public enum eDASH_TYPE
{
    NONE,     
    DEAD_ZONE,
    DASH,     
    MAXIMUN   
}

public enum CHAIN_STATE : byte
{
    eNONE = 0,
    eTHROWBEFORE,
    eTHROW,
    eHOLDING,
    eBACK
}
public enum CHANNELING_STATE : byte
{
    eNONE = 0,
    eSTART,
    eLOOPREADY,
    eLOOP,
    eEND
}

public enum SHIELDGUARD_STATE : byte
{
    eNONE = 0,
    eGUARDING,
    eBREAK,
}
public enum COMMON_STATE : byte
{
    eNONE = 0,
    eSTART,
    eUPDATE,
    eEND,
}

public partial class CharacterBase : KSMono
{
    public CHAR_STATE m_CharState;
    public CHAR_STATE m_CharTgrState;
    public MOTION_STATE m_MotionState;          // 동작 상태입니다..
    //public ANIMATION_STATE m_AnimState;
    public ObjectStateMachine m_fsm;                  // 오브젝트 애니메이션 동작을 시킵니다..
    public CapsuleCollider m_Collider;             // 충돌체크..
    public UnityEngine.CharacterController m_UnityCharController = null;
    public Transform m_MyObj;         // 오브젝트 트랜스폼..
    protected KSAnimation m_KSAnimation;          // 오브젝트 애니메이션 데이타를 로드합니다.
    private SoundSender m_SoundSednder;

    public Dictionary<string, string> m_CharacterAnimClipDic; // It's for basic stance animation clip loading
    private Dictionary<string, string> m_CharacterSkillAnimClipDic;//It's for skill animation clip loading
    private Dictionary<string, WrapMode> m_CharSkillAnimLoopDic;
    public eCharacter m_CharacterType;        // CharacterFactory.cs
    public AI m_CharAi;                // m_CharAi 가지고있는다.
    // gunny test start
    //protected CharacterDamageEffect m_CharacterDamageEffect;//
    // gunny test modified
    public CharacterDamageEffect m_CharacterDamageEffect;//
    // gunny test end
    public MyCharacterController m_charController;  //  
    public AnimationInterface m_AnimationInterface;   // GetDamage 함수에서 사용
    public int m_CharUniqID;
    public long m_IngameObjectID;
    public List<Renderer> m_preFabRenderer;
    Dictionary<Renderer, Material> mOldMaterials;

    public Dictionary<string, Transform> m_EffectBoneDic;

    //Moving of Animation Event
    public AniMoveForward m_AniMove;              //
    public bool m_bAttackMove;          //    
    public Vector3 m_atkMovingDir;        //
    //need to Check SK ////////////////////////
    public bool m_bPushStart = false;
    public Vector3 m_pushMoveDir;          //
    public float m_ComboDashDist;

    /// ////////////////////////////////////////////

    public DamageUI m_damageUI;
    public CameraManager m_CameraManager = null;

    public CharacterBase m_AttackTarget;
    public Vector3 m_AttackPosition;
    public SkillSender m_skSender;

    // 스킬 정보 변수들입니다. 차후에 수정입니다. 
    public Dictionary<int, CAttackInfo> m_AttackInfos;          // 스킬 정보들 입니다..
    public int m_UseSkillIndex;
    public int m_currSkillIndex;
    public int m_ArollingSkill;
    //private int m_myPlayerIdx;
    public int m_SysAffect_Idx;
    private bool m_IsCritical;
    public List<int> m_NormalAttackIndexes;

    //About Damage & Reaction
    //eREACTION_TYPE m_ReactionType;

    public BuffData_ReAction m_ReactionBuffData;
    public eAffectCode m_ReactionAffectCode;

    private int m_nSkillEffectSequenceIndex = 1;
    private int m_nSkillProjectileSequenceIndex = 1;

    private DamageCalculationData m_DamageCalculationData;

    public BuffController m_BuffController = null;

    public float m_fDieTimeOut = 0.0f; // 죽은지 20 초가 지나도 살아 있다면 강제로 죽임

    public EffectSkillManager m_EffectSkillManager = null;

    public bool m_bMotionCancel = false;

    public bool m_bDieImed;

    private EventDelegate.Callback m_moveEndCallBack = null;

    public float m_spwanTime = 0f;

    //Shield Guard
    public SHIELDGUARD_STATE m_GuardState;
    public bool m_bGuardPress;
    //public UI_InGameButtons m_IngameBtns;
    //public float m_MyShield;
    public bool m_bGuardAvail;
    //private const float RESTORE_SHIELD_RATE = 10;
    //private const float RESTORE_SHIELD_FREQUNCE = 1;
    public COMMON_STATE m_eGuardingHit;
    public float m_TimeGuardingHit;

    eSTATE_HIDE m_eHideState = eSTATE_HIDE.eSTATE_NONE;

    public bool m_bTriggerMoveArea;
    private EventTriggerData m_evtTriggerData;
    private Vector3 m_MoveAreaDestPos;

    public int m_nSpawnerId = 0;
    public HPbar m_hpBar = null;

    //boss cinema effect
    public List<SkillDataInfo.EffectResInfo> m_BossCinemaEffs;
    public bool m_bBossSuperArmor;

    public bool m_bFront;

    public SkinnedMeshRenderer[] m_charSRenderer;
    private MeshRenderer[] m_charMRenderer;
    public List<Transform> m_RendererParent;
    //public List<AddMaterialOnHit> m_FrostMaterials;

    public SkillDataInfo.EffectResInfo hitEffect;


    private bool bPvpLastDie = false;

	private	int m_Team	= 0;

	public struct SkillNetData
    {
        public bool m_bActive;
        public List<long> m_iApplyBuffListSend;
        public int m_iReactionStatus;
        public blame_messages.StatusEffect m_statusEffect;
        public List<BuffData> m_NetBuffData;
        public List<CharacterBase> m_Target;
    };
    public struct SkillNetReturnData
    {
        public bool m_Waiting;
        public long m_iSkillCode;
        public bool m_bPacketReceive;
        public CharacterBase m_Target;
        public float m_fTimeOut;
    }

    public SkillNetData m_HitSkillNetData;
    public SkillNetReturnData m_SkillNetReturnData;

    public struct Push_Info
    {
        public Vector3 m_PrePos;
        public Vector3 m_TargetPos;
        public Vector3 m_MoveDelta;
        public float m_fTime;
        public float m_fSpeed;
        public float m_fDistance;
        public float m_fDuration;
        public float m_fFreezeTime;
        public bool m_bStart;

        public void Init()
        {
            m_fTime = 0;
            m_fSpeed = 0;
            m_fDistance = 0;
            m_fDuration = 0;
            m_fFreezeTime = 0;
            m_bStart = false;
            m_PrePos = Vector3.zero;
        }

        public void Set(float fSpeed, float fDistance, bool bStart, Vector3 pre_pos, Vector3 target_pos)
        {
            m_PrePos = pre_pos;
            m_TargetPos = target_pos;
            m_bStart = bStart;
            m_fSpeed = fSpeed;
            m_fDistance = fDistance;
            m_fDuration = fDistance / fSpeed;
        }
    };
    public Push_Info m_pushInfo;

    public struct DamageBackMove
    {
        public Vector3 m_PrevPos;
        public Vector3 m_Dir;
        public float m_Speed;
        public bool m_bFirstHit;
        public float m_fFreezeTime;
        public bool m_bAttackPush;
    }
    public DamageBackMove m_DmgBMove;
    public struct PullingMove
    {
        public Vector3 m_PrevPos;
        public Vector3 m_Dir;
        public float m_Speed;
        public bool m_bFirstHit;
        public float m_fFreezeTime;
        public float m_fPullingTime;
        static public float m_fChainFreezingTime = 0.2f;
    }
    public PullingMove m_PullingMove;
    public struct PushingMove
    {
        public Vector3 m_Dir;
        public float m_Speed;
        public float m_fFreezeTime;
        public float m_fPushingTime;
    }
    public PushingMove m_PushingMove;
    public struct ScrewMove
    {
        public Vector3 m_CenterPos;
        public Vector3 m_Dir;
        public float m_Speed;
        public float m_fScrewTime;
        public float m_fScrewIntervalTime;
        public float m_fScrewTotlaTime;
        public float m_fShakeRadius;
    }
    public ScrewMove m_ScrewMove;
    public struct ChannelingState
    {
        public float m_fChannelingTime;
        public float m_fChannelingInterValTime;
        public bool m_bCancel;
        public CHANNELING_STATE m_eState;
    }
    public ChannelingState m_ChannelingState;
    public struct PanicMove
    {
        public Vector3 m_Dir;
        public float m_fTime;
    }
    public PanicMove m_PanicBMove;
    public struct ChainMove
    {
        public string m_AnimName;
        public CHAIN_STATE m_State;
        public Vector3 m_Dir;
        public float m_Speed;
        public float m_PlayTime;
        public GameObject m_ChainPuller;
        public LineRenderer m_ChainLine;
        public Chain m_ChainScript;
        public CharacterBase m_CatchChar;
    }
    public ChainMove m_ChainMove;

    public struct MultiAnimControl
    {
        public ANIMATION_STATE m_AnimState;

        public float castingTime;
        public bool bSpeedChange;
    }
    public MultiAnimControl m_mAnimCtr;
    public struct TauntMove
    {
        public bool m_bTaunt;
        public eMoveType m_eTauntMoveType;
        public CharacterBase m_TauntChar;
        public AutoAI.eAutoProcessState m_BackupAutoState;
		public HeroNpcAI.eAutoProcessState m_BackupHeroNpcAutoState;
	}
    public TauntMove m_TauntMove;
    
    protected void Awake()
    {
        m_MyObj = transform;
        m_AttackInfos = new Dictionary<int, CAttackInfo>();
        m_CharacterAnimClipDic = new Dictionary<string, string>();
        m_CharacterSkillAnimClipDic = new Dictionary<string, string>();
        m_CharSkillAnimLoopDic = new Dictionary<string, WrapMode>();
        m_KSAnimation = gameObject.AddComponent<KSAnimation>();
        m_SoundSednder = gameObject.AddComponent<SoundSender>();
        m_fsm = new ObjectStateMachine();

        m_EffectBoneDic = new Dictionary<string, Transform>();

        m_pushInfo = new Push_Info();
        m_DmgBMove = new DamageBackMove();
        m_PullingMove = new PullingMove();
        m_PushingMove = new PushingMove();
        m_ScrewMove = new ScrewMove();
        m_mAnimCtr = new MultiAnimControl();
        m_ChainMove = new ChainMove();
        m_PanicBMove = new PanicMove();
        m_ChannelingState = new ChannelingState();
        m_TauntMove = new TauntMove();

        m_HitSkillNetData = new SkillNetData();
        m_HitSkillNetData.m_Target = new List<CharacterBase>();
        m_HitSkillNetData.m_NetBuffData = new List<BuffData>();
        m_HitSkillNetData.m_iApplyBuffListSend = new List<long>();
        m_SkillNetReturnData = new SkillNetReturnData();
        m_SkillNetReturnData.m_Waiting = false;
        m_SkillNetReturnData.m_bPacketReceive = false;

        m_NormalAttackIndexes = new List<int>();

        SetCharacterState(CHAR_STATE.ALIVE);
        animState = ANIMATION_STATE.eANIM_START;
        //SetAnimationState(ANIMATION_STATE.eANIM_START);

        m_preFabRenderer = new List<Renderer>();
        //mOldMaterials = new Dictionary<Renderer, Material>();

        m_bBossSuperArmor = false;

    }

    protected void Start()
    {
        InitBasicStance();

        m_Collider = gameObject.GetComponent<CapsuleCollider>();
        //m_affectCode          = eAffectCode.eNONE;
        //dpmp                        = gameObject.GetComponent<DPadMovePlayer>();
        m_skSender = m_MyObj.GetComponent<SkillSender>();
        m_AniMove = gameObject.AddComponent<AniMoveForward>();
        m_charController = gameObject.GetComponent<MyCharacterController>();
        m_damageUI = gameObject.GetComponent<DamageUI>();
        m_UnityCharController = gameObject.GetComponent<UnityEngine.CharacterController>();
        m_EffectSkillManager = gameObject.GetComponent<EffectSkillManager>();

        //Taylor Finish
        if (m_Collider != null && m_CharUniqID != PlayerManager.MYPLAYER_INDEX)
        {
            m_Collider.enabled = true;
        }

        //SetCharEffectBone(transform);

        switch (m_CharacterType)
        {
            case eCharacter.eNPC:
                m_CharAi = gameObject.GetComponent<NpcAI>();
                if (m_Collider != null)
                {
                    switch (m_CharAi.GetNpcProp().Npc_Class)
                    {
                        case eNPC_CLASS.eNONE:    
                        case eNPC_CLASS.eNORMAL:  
                        case eNPC_CLASS.eELITE:   
                            m_Collider.radius *= 1.0f;
                            break;
                        case eNPC_CLASS.eBOSS:     
                        case eNPC_CLASS.eRAIDBOSS: 
                            //Debug.Log("Boss name = " + gameObject.name);
                            break;
                    }
                    m_Collider.height = (m_Collider.height + (m_Collider.radius * 2.0f));
                }
                break;
            default:
                m_CharAi = gameObject.GetComponent<AutoAI>();
                if (m_Collider != null)
                {
                    m_Collider.radius *= 1.0f;
                }
                break;
        }


        if (m_DamageCalculationData == null)
        {
            m_DamageCalculationData = new DamageCalculationData(this);
        }
        if (m_BuffController == null)
        {
            m_BuffController = new BuffController(this);
        }

        m_bDieImed = false;

        //m_bFront = false;

        m_SysAffect_Idx = 0;
        //m_ReactionType = eREACTION_TYPE.eNONE;

        m_bTriggerMoveArea = false;

        // 애니메이션 로딩합니다. 
        LoadAnimations();

        m_SoundSednder.SetObject(m_MyObj);

        ksAnimation.OnAnimationEnd += new KSAnimation.AnimEvent(OnAniEventEnd);


        if (m_fsm != null)
        {

            m_fsm.SetMachine(m_MyObj.gameObject, this);
            if (m_CharacterType != eCharacter.eNPC && m_CharacterType != eCharacter.eHERONPC)
            {
                if (MainManager.instance.currentFlow == eMAIN_FLOW.eINGAME)
                {
                    {
                        m_fsm.Clear();
                    }
                }
                else
                {
                    m_fsm.ChangeState(ObjectState_IdleStand.Instance());
                }

                if (SceneManager.instance.GetUIComponent(SceneManager.eCOMPONENT.PvpLobby) != null)
                {
                    if (SceneManager.instance.GetUIComponent(SceneManager.eCOMPONENT.PvpLobby).isActiveAndEnabled)
                    {
                        m_fsm.ChangeState(ObjectState_IdleStand.Instance());
                    }
                }

                if (SceneManager.instance.GetUIComponent(SceneManager.eCOMPONENT.CharacterSelect) != null)
                {
                    if (SceneManager.instance.GetUIComponent(SceneManager.eCOMPONENT.CharacterSelect).isActiveAndEnabled)
                    {
                        m_fsm.ChangeState(ObjectState_IdleStand.Instance());
                    }
                }

                if (SceneManager.instance.GetUIComponent(SceneManager.eCOMPONENT.PvpLoading) != null)
                {
                    if (SceneManager.instance.GetUIComponent(SceneManager.eCOMPONENT.PvpLoading).isActiveAndEnabled)
                    {
                        m_fsm.ChangeState(ObjectState_IdleStand.Instance());
                    }
                }

                if (SceneManager.instance.GetUIComponent(SceneManager.eCOMPONENT.MyCharacterInfo) != null)
                {
                    if (SceneManager.instance.GetUIComponent(SceneManager.eCOMPONENT.MyCharacterInfo).isActiveAndEnabled)
                    {
                        m_fsm.ChangeState(ObjectState_IdleStand.Instance());
                    }
                }
            }
            else if (m_CharacterType == eCharacter.eHERONPC)
            {
                SetChangeMotionState(MOTION_STATE.eMOTION_SPWAN);
                SetChangeMotionState(MOTION_STATE.eMOTION_IDLE);
            }

        }

        if (Camera.main != null)
            m_CameraManager = Camera.main.GetComponent<CameraManager>();

        // 데미지..
        if (m_CharacterDamageEffect == null)
        {
            m_CharacterDamageEffect = m_MyObj.gameObject.AddComponent<CharacterDamageEffect>();
            m_CharacterDamageEffect.Character = this;
            m_CharacterDamageEffect.SetMeshRenderer(m_preFabRenderer);

        }
        if (m_CharacterType == eCharacter.eNPC)
        {
            m_hpBar = gameObject.GetComponent<HPbar>();
            if (((NpcAI)m_CharAi).GetNpcProp().Battle_Type == eNPC_BATTLE_TYPE.eBATTLE)
            {

                SetFrozenExpression();
            }
        }
        else
        {

            if (SceneManager.instance.GetCurGameMode() == SceneManager.eGAMEMODE.eNONSYNC_PVP)
            {
                SetFrozenExpression();
            }
        }

	}
    private void SetFrozenExpression()
    {
        m_RendererParent = new List<Transform>();

        m_charSRenderer = gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
        for (int i = 0; i < m_charSRenderer.Length; ++i)
        {
            m_RendererParent.Add(m_charSRenderer[i].gameObject.transform);
        }

        m_charMRenderer = gameObject.GetComponentsInChildren<MeshRenderer>();
        for (int i = 0; i < m_charMRenderer.Length; ++i)
        {
            if (m_charMRenderer[i].gameObject.CompareTag("CharacterShadow") == false)
            {
                m_RendererParent.Add(m_charMRenderer[i].gameObject.transform);
            }
        }
    }

    private void OnDestroy()
    {
        if (m_fsm != null)
        {
            m_fsm.DestroyStateMachine();
            m_fsm = null;
        }
        if (m_Collider != null) m_Collider = null;
        if (m_UnityCharController != null) m_UnityCharController = null;
        if (m_KSAnimation != null)
        {
            m_KSAnimation.DestroyKSAnimation();
            m_KSAnimation = null;
        }
        if (m_CharacterAnimClipDic != null) m_CharacterAnimClipDic = null;
        if (m_CharacterSkillAnimClipDic != null) m_CharacterSkillAnimClipDic = null;
        if (m_CharSkillAnimLoopDic != null) m_CharSkillAnimLoopDic = null;
        if (m_CharAi != null)
        {
            m_CharAi = null;
        }
        if (m_CharacterDamageEffect != null) m_CharacterDamageEffect = null;
        if (m_charController != null) m_charController = null;
        if (m_AnimationInterface != null)
        {
            m_AnimationInterface.DestroyAnimInterface();
            m_AnimationInterface = null;
        }
        if (m_EffectBoneDic != null) m_EffectBoneDic = null;
        if (m_AniMove != null)
        {
            m_AniMove.DestroyAniMoveForward();
            m_AniMove = null;
        }
        if (m_damageUI != null) m_damageUI = null;
        if (m_CameraManager != null) m_CameraManager = null;
        if (m_skSender != null)
        {
            m_skSender.DestroySkillSender();
            m_skSender = null;
        }
        if (m_AttackInfos != null) m_AttackInfos = null;
        if (m_ReactionBuffData != null) m_ReactionBuffData = null;
        if (m_DamageCalculationData != null) m_DamageCalculationData = null;
        if (m_BuffController != null)
        {
            m_BuffController.DestroyBuffController();
            m_BuffController = null;
        }
        if (m_preFabRenderer != null)
        {
            for (int i = 0; i < m_preFabRenderer.Count; i++)
            {
                Destroy(m_preFabRenderer[i]);
            }
            m_preFabRenderer.Clear();
        }

        //m_preFabRenderer = null;

        if (m_RendererParent != null)
        {

            m_RendererParent.Clear();
            m_RendererParent = null;
        }

        if (m_charSRenderer != null)
        {
            for (int i = 0; i < m_charSRenderer.Length; i++)
            {
                Destroy(m_charSRenderer[i]);
            }
            m_charSRenderer = null;
        }

        if (m_charMRenderer != null)
        {
            for (int i = 0; i < m_charMRenderer.Length; i++)
            {
                Destroy(m_charMRenderer[i]);
            }
            m_charMRenderer = null;
        }

        StopAllCoroutines();

	    Destroy();
    }


    public bool GetNearDestinationPos(Vector3 pos)
    {
        if (Vector3.Distance(m_MyObj.transform.position, pos) < 0.5f)
        {
            return true;
        }
        return false;
    }

    public float GetRadius()
    {
        if (m_Collider == null)
            m_Collider = gameObject.GetComponent<CapsuleCollider>();

        if (m_Collider != null)
            return (m_Collider.radius) * m_MyObj.transform.localScale.x;

        return 0.5f;
    }

    public void SetCharType(eCharacter type)
    {
        m_CharacterType = type;
        InitCharType();
    }

    private void InitCharType()
    {
        if (m_MyObj.CompareTag("NPC"))
        {
            m_CharacterType = eCharacter.eNPC;
        }
    }


    public void SetWeaponEffectBone(Transform parentObj)
    {
        Transform tmpObj = null;

        if (m_CharacterType == eCharacter.eWarrior)
        {
            tmpObj = parentObj.FindChild("Bip001/Bip001 Prop1/Weapon_Sword");

            if (m_EffectBoneDic.ContainsKey("Weapon_Sword"))
            {
                if (m_EffectBoneDic["Weapon_Sword"] == null)
                {
                    m_EffectBoneDic["Weapon_Sword"] = tmpObj;
                }
            }
            else
            {
                m_EffectBoneDic.Add("Weapon_Sword", tmpObj);
            }
        }
        else if (m_CharacterType == eCharacter.eWizard)
        {
            tmpObj = parentObj.FindChild("Bip001/Bip001 Pelvis/Bip001 Spine/Bip001 Spine1/Bip001 Spine2/Bip001 Neck/Bip001 L Clavicle/Bip001 L UpperArm/Bip001 L Forearm/Weapon_L");
            if (m_EffectBoneDic.ContainsKey("Weapon_L"))
            {
                if (m_EffectBoneDic["Weapon_L"] == null)
                {
                    m_EffectBoneDic["Weapon_L"] = tmpObj;
                }
            }
            else
            {
                m_EffectBoneDic.Add("Weapon_L", tmpObj);
            }

            tmpObj = parentObj.FindChild("Bip001/Bip001 Pelvis/Bip001 Spine/Bip001 Spine1/Bip001 Spine2/Bip001 Neck/Bip001 R Clavicle/Bip001 R UpperArm/Bip001 R Forearm/Weapon_R");
            if (m_EffectBoneDic.ContainsKey("Weapon_R"))
            {
                if (m_EffectBoneDic["Weapon_R"] == null)
                {
                    m_EffectBoneDic["Weapon_R"] = tmpObj;
                }
            }
            else
            {
                m_EffectBoneDic.Add("Weapon_R", tmpObj);
            }
        }

        SetWeaponEffect();
    }

    public Transform GetCharEffectBone(string name)
    {
        Transform BontTransform = null;

        m_EffectBoneDic.TryGetValue(name, out BontTransform);

        return BontTransform;
    }
    
	public void SetWeaponEffect()
    {
        EffectSkillManager effSkillManager = m_MyObj.gameObject.GetComponent<EffectSkillManager>();
        if (effSkillManager != null)
        {
            PlayerInfo pInfo = PlayerManager.instance.m_PlayerInfo[PlayerManager.MYPLAYER_INDEX];

            ItemData itemData = pInfo.curEquipItems.m_dicItems[ItemData.eITEM_SUB_KIND.HAND_RIGHT];
            PlayerDataInfo pDinfo = PlayerManager.instance.m_PlayerDataInfo[(int)pInfo.jobType];

            if (itemData != null)
            {
                if (itemData.enchantLevel > 0 && itemData.Grade != ItemData.eGRADE.NORMAL)
                {
                    switch (m_CharacterType)
                    {
                        case eCharacter.eWarrior:
                            DestroyWeaponEff(effSkillManager);
                            if (itemData.enchantLevel >= 10 && itemData.enchantLevel < 20)
                            {
                                effSkillManager.LoadSkillEffectObject(this, pDinfo.weaponEffsDic[itemData.Grade].weaponEffResInfo[0]);
                                effSkillManager.FXEffectPlay(this, pDinfo.weaponEffsDic[itemData.Grade].weaponEffResInfo[0], GetCharEffectBone(pDinfo.weaponEffsDic[itemData.Grade].weaponEffResInfo[0].strEffectTargetPosition));
                            }
                            else if (itemData.enchantLevel >= 20)
                            {
                                effSkillManager.LoadSkillEffectObject(this, pDinfo.weaponEffsDic[itemData.Grade].weaponEffResInfo[1]);
                                effSkillManager.FXEffectPlay(this, pDinfo.weaponEffsDic[itemData.Grade].weaponEffResInfo[1], GetCharEffectBone(pDinfo.weaponEffsDic[itemData.Grade].weaponEffResInfo[1].strEffectTargetPosition));
                            }
                            break;
                        case eCharacter.eWizard:
                            DestroyWeaponEff(effSkillManager);
                            if (itemData.enchantLevel >= 10 && itemData.enchantLevel < 20)
                            {
                                effSkillManager.LoadSkillEffectObject(this, pDinfo.weaponEffsDic[itemData.Grade].weaponEffResInfo[0]);
                                effSkillManager.FXEffectPlay(this, pDinfo.weaponEffsDic[itemData.Grade].weaponEffResInfo[0], GetCharEffectBone("Weapon_L"));
                                effSkillManager.FXEffectPlay(this, pDinfo.weaponEffsDic[itemData.Grade].weaponEffResInfo[0], GetCharEffectBone("Weapon_R"));
                            }
                            else if (itemData.enchantLevel >= 20)
                            {
                                effSkillManager.LoadSkillEffectObject(this, pDinfo.weaponEffsDic[itemData.Grade].weaponEffResInfo[1]);
                                effSkillManager.FXEffectPlay(this, pDinfo.weaponEffsDic[itemData.Grade].weaponEffResInfo[1], GetCharEffectBone("Weapon_L"));
                                effSkillManager.FXEffectPlay(this, pDinfo.weaponEffsDic[itemData.Grade].weaponEffResInfo[1], GetCharEffectBone("Weapon_R"));
                            }
                            break;
                    }
                }
            }
        }
    }

    private void DestroyWeaponEff(EffectSkillManager effMgr)
    {
        for (int k = 0; k < effMgr.m_PlayingEffObjs.Count; ++k)
        {
            if (effMgr.m_PlayingEffObjs[k].m_EffPrefab != null)
            {
                DestroyObject(effMgr.m_PlayingEffObjs[k].m_EffPrefab);
            }
        }
    }

    private SkillDataInfo.EffectResInfo GetEffResInfoInAtkInfo(int effID)
    {
        for (int i = 0; i < m_CharAi.GetNpcProp().Npc_skill_Id.Length; ++i)
        {
            if (m_AttackInfos.ContainsKey(m_CharAi.GetNpcProp().Npc_skill_Id[i]))
            {
                for (int k = 0; k < m_AttackInfos[m_CharAi.GetNpcProp().Npc_skill_Id[i]].skillinfo.effBodyInfo.Count; ++k)
                {
                    if (m_AttackInfos[m_CharAi.GetNpcProp().Npc_skill_Id[i]].skillinfo.effBodyInfo[k].unEffectID == effID)
                    {
                        return m_AttackInfos[m_CharAi.GetNpcProp().Npc_skill_Id[i]].skillinfo.effBodyInfo[k];
                    }
                }
            }
        }

        return null;
    }

    public void LoadEffect_BossCinema()
    {
        if (CinemaSceneManager.instance.IsExist(SpawnDataManager.instance.EventTriggerIndex) == false)
            return;

        List<int> effidx = CinemaSceneManager.instance.GetCinemaEffectIndexes(SpawnDataManager.instance.EventTriggerIndex);

        if (effidx != null)
        {
            for (int i = 0; i < effidx.Count; ++i)
            {
                if (effidx[i] != 0)
                {
                    SkillDataInfo.EffectResInfo tmpEffResInfo = GetEffResInfoInAtkInfo(effidx[i]);
                    if (tmpEffResInfo == null)
                    {
                        tmpEffResInfo = SkillDataManager.instance.GetEffectResInfoByEffID(effidx[i]);
                    }

                    m_BossCinemaEffs.Add(tmpEffResInfo);
                }
            }
        }
    }

    public void LoadBodyEffect(CAttackInfo p_AttackInfo)
    {
        p_AttackInfo.skillinfo.effectSkillManager.LoadEffectByInfo(this, p_AttackInfo.skillinfo.effBodyInfo);
    }

    public void LoadProjectTileEffectSkill(CAttackInfo p_AttackInfo)
    {
        p_AttackInfo.skillinfo.effectSkillManager.AddProjectTileEffectList(this, p_AttackInfo.skillinfo.projectTileInfo);
    }

    public void LoadBuffEffect(CAttackInfo p_AttackInfo)
    {
        p_AttackInfo.skillinfo.effectSkillManager.LoadEffectByInfo(this, p_AttackInfo.skillinfo.effBuffInfo);
    }

    private void InitNextSkill(int nIndex, ref EffectSkillManager effectSkillManager, bool bNextNext = false, List<SkillAttributeData> SkillAttribute = null)
    {
        if (m_AttackInfos.ContainsKey(nIndex) || bNextNext == true)
        {
            if (m_AttackInfos[nIndex].skillinfo.nextSkill_index != 0)
            {
                if (m_AttackInfos.ContainsKey(m_AttackInfos[nIndex].skillinfo.nextSkill_index) == false)
                {
                    CAttackInfo cAttackInfo = SkillDataManager.instance.Load(m_AttackInfos[nIndex].skillinfo.nextSkill_index);
                    SkillDataManager.instance.UpgradeSkillFromAtt(cAttackInfo, SkillAttribute);

                    cAttackInfo.skillinfo.effectSkillManager = effectSkillManager;
                    //cAttackInfo.skillinfo.nSoulStoneAbilityLinkIDList = p_nSoulStoneAbilityLinkIDList;
                    m_AttackInfos.Add(m_AttackInfos[nIndex].skillinfo.nextSkill_index, cAttackInfo);
                    cAttackInfo.skillinfo.attackType = eATTACK_TYPE.eATTACK_SKILL;
                    LoadBodyEffect(cAttackInfo);
                    LoadBuffEffect(cAttackInfo);
                    //cAttackInfo.skillinfo.effectSkillManager.SkillEffectLoad(m_MyObj.gameObject, cAttackInfo.skillinfo.effBodyInfo);

                    /// 애니메이션 등록 /// ksk
                    //Debug.Log(cAttackInfo.skillinfo.aniResInfo[0].animation_name);
                    for (int i = 0; i < cAttackInfo.skillinfo.aniResInfo.Count; i++)
                    {
                        SkillDataInfo.AnimationResInfo aniResInfo = cAttackInfo.skillinfo.aniResInfo[i];
                        if (m_CharacterSkillAnimClipDic.ContainsKey(aniResInfo.animation_name) == false)
                        {
                            m_CharacterSkillAnimClipDic.Add(aniResInfo.animation_name, aniResInfo.animation_name);
                            m_CharSkillAnimLoopDic.Add(aniResInfo.animation_name, aniResInfo.loopType);
                        }
                    }

                    InitAdditiveSkill(m_AttackInfos[nIndex].skillinfo.nextSkill_index, ref effectSkillManager, SkillAttribute);
                    //Debug.Log("InitNextSkill nextSkill Index = " + m_AttackInfos[nIndex].skillinfo.nextSkill_index + "/ next of next = " + m_AttackInfos[m_AttackInfos[nIndex].skillinfo.nextSkill_index].skillinfo.nextSkill_index);

                    InitNextSkill(m_AttackInfos[nIndex].skillinfo.nextSkill_index, ref effectSkillManager, true, SkillAttribute);
                }
            }
        }
    }
    private void InitChainSkill(int nIndex, ref EffectSkillManager effectSkillManager, List<SkillAttributeData> SkillAttribute = null)
    {
        // Link Code : Skill Index 
        int nCount = m_AttackInfos[nIndex].skillinfo.chain_Affect_Code.Count;
        if (nCount > 0)
        {
            for (int i = 0; i < nCount; ++i)
            {
                if (m_AttackInfos[nIndex].skillinfo.chain_Affect_Code[i] != 0)
                {
                    if (m_AttackInfos.ContainsKey(m_AttackInfos[nIndex].skillinfo.chain_Affect_Code[i]) == false)
                    {
                        CAttackInfo cAttackInfo = SkillDataManager.instance.Load(m_AttackInfos[nIndex].skillinfo.chain_Affect_Code[i]);
                        SkillDataManager.instance.UpgradeSkillFromAtt(cAttackInfo, SkillAttribute);

                        //Debug.Log(this + "link Skill Code = " + m_AttackInfos[nIndex].skillinfo.chain_Affect_Code[i]);
                        cAttackInfo.skillinfo.effectSkillManager = effectSkillManager;
                        cAttackInfo.skillinfo.attackType = eATTACK_TYPE.eATTACK_SKILL;
                        m_AttackInfos.Add(m_AttackInfos[nIndex].skillinfo.chain_Affect_Code[i], cAttackInfo);
                        LoadBodyEffect(cAttackInfo);
                        LoadBuffEffect(cAttackInfo);

                        InitAdditiveSkill(m_AttackInfos[nIndex].skillinfo.chain_Affect_Code[i], ref effectSkillManager, SkillAttribute);
                    }
                }
            }
        }
    }

    private void InitAdditiveSkill(int nIndex, ref EffectSkillManager effectSkillManager, List<SkillAttributeData> SkillAttribute = null, bool SummonNormalAtt = false)
    {
        if (m_AttackInfos.ContainsKey(nIndex) == false)
            return;

        // Link Code : Skill Index 
        int nCount = m_AttackInfos[nIndex].skillinfo.link_Skill_Code.Count;
        if (nCount > 0)
        {
            for (int i = 0; i < nCount; ++i)
            {
                if (m_AttackInfos[nIndex].skillinfo.link_Skill_Code[i] != 0)
                {
                    if (m_AttackInfos.ContainsKey(m_AttackInfos[nIndex].skillinfo.link_Skill_Code[i]) == false)
                    {
                        CAttackInfo cAttackInfo = SkillDataManager.instance.Load(m_AttackInfos[nIndex].skillinfo.link_Skill_Code[i]);
                        SkillDataManager.instance.UpgradeSkillFromAtt(cAttackInfo, SkillAttribute);

                        //Debug.Log(this + "link Skill Code = " + m_AttackInfos[nIndex].skillinfo.link_Skill_Code[i]);
                        cAttackInfo.skillinfo.effectSkillManager = effectSkillManager;
                        cAttackInfo.skillinfo.attackType = eATTACK_TYPE.eATTACK_SKILL;
                        //cAttackInfo.skillinfo.nSoulStoneAbilityLinkIDList = p_nSoulStoneAbilityLinkIDList;
                        m_AttackInfos.Add(m_AttackInfos[nIndex].skillinfo.link_Skill_Code[i], cAttackInfo);
                        LoadBodyEffect(cAttackInfo);
                        LoadBuffEffect(cAttackInfo);

                        for (int j = 0; j < cAttackInfo.skillinfo.link_Skill_Code.Count; ++j)
                        {
                            if (m_AttackInfos.ContainsKey(cAttackInfo.skillinfo.link_Skill_Code[j]) == false)
                            {
                                CAttackInfo cAttackInfoLink = SkillDataManager.instance.Load(cAttackInfo.skillinfo.link_Skill_Code[j]);

                                if (cAttackInfoLink == null)
                                    continue;

                                if (m_AttackInfos.ContainsKey(cAttackInfo.skillinfo.link_Skill_Code[j]) == false)
                                {
                                    cAttackInfoLink.skillinfo.effectSkillManager = effectSkillManager;
                                    cAttackInfoLink.skillinfo.attackType = eATTACK_TYPE.eATTACK_SKILL;
                                    //cAttackInfoLink.skillinfo.nSoulStoneAbilityLinkIDList = p_nSoulStoneAbilityLinkIDList;
                                    m_AttackInfos.Add(cAttackInfo.skillinfo.link_Skill_Code[j], cAttackInfoLink);
                                    LoadBodyEffect(cAttackInfoLink);
                                    LoadBuffEffect(cAttackInfoLink);
                                }
                            }
                        }
                    }
                }

                else
                {
                    if (m_CharacterType != eCharacter.eHERONPC)
                    {
                        continue;
                    }

                    NpcInfo.NpcProp a_kNpcProp = ((HeroNpcAI)m_CharAi).GetNpcProp();
                    Dictionary<int, NpcInfo.SummonPropetyLinkskill> a_kSummonPropetyLinkskill = a_kNpcProp._SummonPropetyLinkskill;

                    int a_nCount = 0;
                    int a_nSkillCode = 0;

                    foreach (KeyValuePair<int, NpcInfo.SummonPropetyLinkskill> pair in a_kSummonPropetyLinkskill)
                    {
                        if (pair.Key <= a_kNpcProp.Grade)
                        {
                            //if(!PetManager.Instance.GetNomalAtt(pair.Value, SummonNormalAtt))
                            if (!SummonNormalAtt)
                            {
                                continue;
                            }

                            if (a_nCount == 0)
                            {
                                Set_InitAddLinkPropety(a_nCount, nIndex, pair.Value, ref effectSkillManager, SkillAttribute);
                            }
                            else
                            {

                                Set_InitAddLinkPropety(a_nCount, a_nSkillCode, pair.Value, ref effectSkillManager, SkillAttribute);
                            }

                            a_nSkillCode = pair.Value.skillcode;
                        }

                        a_nCount++;
                    }
                }

            }
        }
    }

    private void Set_InitAddLinkPropety(int Addattribute_Index, int nSkillCode, NpcInfo.SummonPropetyLinkskill kLinkSkill, ref EffectSkillManager effectSkillManager, List<SkillAttributeData> SkillAttribute = null)
    {
        bool a_bExistCode = m_AttackInfos.ContainsKey(nSkillCode);

        if (a_bExistCode == false)
        {
            return;
        }

        CAttackInfo a_kAttackInfo = m_AttackInfos[nSkillCode];
        SkillDataInfo.SkillInfo a_kSkillInfo = a_kAttackInfo.skillinfo;
        List<int> a_listLinkSkillCodes = a_kSkillInfo.link_Skill_Code;
        List<int> a_listLinkSkillRates = a_kSkillInfo.link_Skill_Rate;
        int a_nLinkSkillCodeCount = a_listLinkSkillCodes.Count;
        int a_nLinkSkillCode = 0;

        /// 링크 스킬 코드 변경
        /// 
        if (m_AttackInfos.ContainsKey(kLinkSkill.skillcode) == false)
        {
            for (int nIndex = 0; nIndex < a_nLinkSkillCodeCount; ++nIndex)
            {
                a_listLinkSkillCodes[nIndex] = kLinkSkill.skillcode;
                a_listLinkSkillRates[nIndex] = kLinkSkill.Start_Rate;
            }
            a_kAttackInfo = SkillDataManager.instance.Load(kLinkSkill.skillcode);
            a_kSkillInfo = a_kAttackInfo.skillinfo;

            a_kSkillInfo.buff_Time[0] = kLinkSkill.DwellTime;
            a_kSkillInfo.buff_Time[1] = kLinkSkill.DamageSecond;
            a_kSkillInfo.system_Affect_Value[0] = (int)kLinkSkill.Damage_Value;

            SkillDataManager.instance.UpgradeSkillFromAtt(a_kAttackInfo, SkillAttribute);

            a_kSkillInfo.effectSkillManager = effectSkillManager;
            a_kSkillInfo.attackType = eATTACK_TYPE.eATTACK_SKILL;

            m_AttackInfos.Add(kLinkSkill.skillcode, a_kAttackInfo);
            LoadBodyEffect(a_kAttackInfo);
            LoadBuffEffect(a_kAttackInfo);

            a_listLinkSkillCodes = a_kSkillInfo.link_Skill_Code;
            a_nLinkSkillCodeCount = a_listLinkSkillCodes.Count;

            for (int nIndex = 0; nIndex < a_nLinkSkillCodeCount; ++nIndex)
            {
                a_nLinkSkillCode = a_listLinkSkillCodes[nIndex];

                if (m_AttackInfos.ContainsKey(a_nLinkSkillCode) == false)
                {
                    a_kAttackInfo = SkillDataManager.instance.Load(a_nLinkSkillCode);

                    if (a_kAttackInfo == null)
                        continue;

                    a_kSkillInfo = a_kAttackInfo.skillinfo;
                    a_kSkillInfo.effectSkillManager = effectSkillManager;
                    a_kSkillInfo.attackType = eATTACK_TYPE.eATTACK_SKILL;

                    m_AttackInfos.Add(a_nLinkSkillCode, a_kAttackInfo);
                    LoadBodyEffect(a_kAttackInfo);
                    LoadBuffEffect(a_kAttackInfo);
                }
            }
        }
        else
        {
            overlapInitAddLinkPropety(Addattribute_Index, a_kAttackInfo, kLinkSkill);

        }
    }
    private void overlapInitAddLinkPropety(int Addattribute_Index, CAttackInfo kAttackInfo, NpcInfo.SummonPropetyLinkskill kLinkSkill)
    {
        if (Addattribute_Index != 0)
        {
            return;
        }
        CAttackInfo kLink_AttackInfo = m_AttackInfos[kLinkSkill.skillcode];

        SkillDataInfo.SkillInfo a_kSkillInfo = kAttackInfo.skillinfo;

        List<int> a_listLinkSkillCodes = a_kSkillInfo.link_Skill_Code;
        List<int> a_listLinkSkillRates = a_kSkillInfo.link_Skill_Rate;
        int a_nLinkSkillCodeCount = a_listLinkSkillCodes.Count;

        /// 링크 스킬 코드 변경
        for (int nIndex = 0; nIndex < a_nLinkSkillCodeCount; ++nIndex)
        {
            a_listLinkSkillCodes[nIndex] = kLinkSkill.skillcode;
            a_listLinkSkillRates[nIndex] = kLinkSkill.Start_Rate;
        }
    }
    private void InitProjectTileSkill(int nIndex, ref EffectSkillManager effectSkillManager, List<SkillAttributeData> SkillAttribute = null)
    {
        if (m_AttackInfos.ContainsKey(nIndex) == false)
            return;

        for (int iCount = 0; iCount < m_AttackInfos[nIndex].skillinfo.projectTileInfo.Count; ++iCount)
        {
            //ProjectTile Seconds
            int[] ProjectTileSkills = ProjectileFactory.LoadAttackInfoProjectile(m_AttackInfos[nIndex].skillinfo, iCount);

            if (ProjectTileSkills != null)
            {
                for (int i = 0; i < ProjectTileSkills.Length; ++i)
                {
                    if (ProjectTileSkills[i] != 0)
                    {
                        if (m_AttackInfos.ContainsKey(ProjectTileSkills[i]) == false)
                        {
                            CAttackInfo cAttackInfo = SkillDataManager.instance.Load(ProjectTileSkills[i]);
                            SkillDataManager.instance.UpgradeSkillFromAtt(cAttackInfo, SkillAttribute);

                            //Debug.Log(this + "link Skill Code = " + ProjectTileSkills[i][i]);
                            cAttackInfo.skillinfo.effectSkillManager = effectSkillManager;
                            cAttackInfo.skillinfo.attackType = eATTACK_TYPE.eATTACK_SKILL;
                            //cAttackInfo.skillinfo.nSoulStoneAbilityLinkIDList = p_nSoulStoneAbilityLinkIDList;
                            m_AttackInfos.Add(ProjectTileSkills[i], cAttackInfo);
                            LoadBodyEffect(cAttackInfo);
                            LoadProjectTileEffectSkill(cAttackInfo);
                            LoadBuffEffect(cAttackInfo);

                            InitAdditiveSkill(ProjectTileSkills[i], ref effectSkillManager, SkillAttribute);
                        }
                    }
                }
            }
            ///////////////////////
        }
    }
    public void InitBuffSkill(int nIndex)
    {
        if (m_AttackInfos.ContainsKey(nIndex) == false)
        {
            CAttackInfo cAttackInfo = SkillDataManager.instance.Load(nIndex);

            cAttackInfo.skillinfo.effectSkillManager = m_EffectSkillManager;
            cAttackInfo.skillinfo.attackType = eATTACK_TYPE.eATTACK_SKILL;
            //cAttackInfo.skillinfo.nSoulStoneAbilityLinkIDList = null;
            m_AttackInfos.Add(nIndex, cAttackInfo);
            LoadBodyEffect(cAttackInfo);
            LoadBuffEffect(cAttackInfo);
        }

        InitAdditiveSkill(nIndex, ref m_EffectSkillManager);
    }

    private void InitBasicStance()
    {
        if (m_CharacterType != eCharacter.eNPC && m_CharacterType != eCharacter.eHERONPC)
        {
            for (int i = 0; i < UtilManager.instance.m_clipName_basic.Length; ++i)
            {
                m_CharacterAnimClipDic.Add(UtilManager.instance.m_clipName_basic[i], UtilManager.instance.m_clipName_basic[i]);
            }
        }
        else
        {
            for (int i = 0; i < UtilManager.instance.m_clipName_npc_basic.Length; ++i)
            {
                m_CharacterAnimClipDic.Add(UtilManager.instance.m_clipName_npc_basic[i], UtilManager.instance.m_clipName_npc_basic[i]);
            }
        }
    }
    private bool AddAttackInfo(int ID, CAttackInfo cAttackInfo)
    {
        if (!m_AttackInfos.ContainsKey(ID))
        {
            m_AttackInfos.Add(ID, cAttackInfo);
            return true;
        }
        return false;
    }

    private void AddNormalAttackIndex(int ID)
    {
        if (!m_NormalAttackIndexes.Contains(ID))
        {
            m_NormalAttackIndexes.Add(ID);
        }
    }

    private void InitAttackInfo_Player(ref EffectSkillManager effectSkillManager)
    {
        PlayerInfo pInfo = PlayerManager.instance.m_PlayerInfo[charUniqID];
        //PlayerDataInfo pDataInfo = PlayerManager.instance.m_PlayerDataInfo[(int)pInfo.jobType];//pInfo.playerDataInfo;   
        CAttackInfo cAttackInfo = null;

        int iStance = (int)UserManager.instance.m_CurPlayingCharInfo.playerInfo.m_eStanceType - 1;
        ((AutoAI)m_CharAi).SetStance(iStance);
        PlayerStanceInfo StanceInfo = PlayerManager.instance.GetPlayerStanceInfo(this, iStance);

        int iStartAtkID = PlayerManager.instance.GetStartAttack(this, iStance);
        int iEndAtkID = PlayerManager.instance.GetEndAttack(this, iStance);
        int iDashAtkID = StanceInfo.m_nDashAttackID;

        if (m_AttackInfos.ContainsKey(iStartAtkID) == false)
        {
            cAttackInfo = SkillDataManager.instance.Load(iStartAtkID);
            if (cAttackInfo != null)
            {
                cAttackInfo.skillinfo.effectSkillManager = effectSkillManager;
                //m_AttackInfos.Add(iStartAtkID, cAttackInfo);
                AddAttackInfo(iStartAtkID, cAttackInfo);
                m_NormalAttackIndexes.Add(iStartAtkID);
                cAttackInfo.skillinfo.attackType = eATTACK_TYPE.eATTACK_NORMAL;
                LoadBodyEffect(cAttackInfo);
                LoadBuffEffect(cAttackInfo);
                if (cAttackInfo.skillinfo.projectTileInfo.Count > 0)
                {
                    LoadProjectTileEffectSkill(cAttackInfo);
                }

                //cAttackInfo.skillinfo.effectSkillManager.SkillEffectLoad(m_MyObj.gameObject, cAttackInfo.skillinfo.effBodyInfo);

                InitAdditiveSkill(iStartAtkID, ref effectSkillManager);

                //투두
                InitProjectTileSkill(iStartAtkID, ref effectSkillManager);


                for (int k = 0; k < m_AttackInfos[iStartAtkID].skillinfo.aniResInfo.Count; ++k)
                {
                    if (m_CharacterAnimClipDic.ContainsKey(m_AttackInfos[iStartAtkID].skillinfo.aniResInfo[k].animation_name) == false)
                        m_CharacterAnimClipDic.Add(m_AttackInfos[iStartAtkID].skillinfo.aniResInfo[k].animation_name, m_AttackInfos[iStartAtkID].skillinfo.aniResInfo[k].animation_name);
                }
            }
        }
        if (m_AttackInfos.ContainsKey(iDashAtkID) == false)
        {
            //Dash
            cAttackInfo = SkillDataManager.instance.Load(iDashAtkID);
            if (cAttackInfo != null)
            {
                cAttackInfo.skillinfo.effectSkillManager = effectSkillManager;
                if (AddAttackInfo(iDashAtkID, cAttackInfo))
                {
                    m_NormalAttackIndexes.Add(iDashAtkID);
                    cAttackInfo.skillinfo.attackType = eATTACK_TYPE.eATTACK_NORMAL;
                    LoadBodyEffect(cAttackInfo);
                    LoadBuffEffect(cAttackInfo);
                    if (cAttackInfo.skillinfo.projectTileInfo.Count > 0)
                    {
                        LoadProjectTileEffectSkill(cAttackInfo);
                    }

                    InitAdditiveSkill(iDashAtkID, ref effectSkillManager);
                    for (int k = 0; k < m_AttackInfos[iDashAtkID].skillinfo.aniResInfo.Count; ++k)
                    {
                        if (m_CharacterAnimClipDic.ContainsKey(m_AttackInfos[iDashAtkID].skillinfo.aniResInfo[k].animation_name) == false)
                            m_CharacterAnimClipDic.Add(m_AttackInfos[iDashAtkID].skillinfo.aniResInfo[k].animation_name, m_AttackInfos[iDashAtkID].skillinfo.aniResInfo[k].animation_name);
                    }
                }
            }
        }

        //Normal Combo Attack
        for (int i = 0; i < PlayerManager.instance.GetMaxNormalAttackCount(this, iStance); i++)
        {
            int iNormalAtkID = PlayerManager.instance.GetNormalAttack(this, iStance, i);

            if (m_AttackInfos.ContainsKey(iNormalAtkID) == false)
            {
                cAttackInfo = SkillDataManager.instance.Load(iNormalAtkID);
                if (cAttackInfo != null)
                {
                    cAttackInfo.skillinfo.effectSkillManager = effectSkillManager;
                    //m_AttackInfos.Add(iNormalAtkID, cAttackInfo);
                    AddAttackInfo(iNormalAtkID, cAttackInfo);
                    m_NormalAttackIndexes.Add(iNormalAtkID); ;
                    cAttackInfo.skillinfo.attackType = eATTACK_TYPE.eATTACK_NORMAL;
                    LoadBodyEffect(cAttackInfo);
                    LoadBuffEffect(cAttackInfo);
                    if (cAttackInfo.skillinfo.projectTileInfo.Count > 0)
                    {
                        LoadProjectTileEffectSkill(cAttackInfo);
                    }
                    //cAttackInfo.skillinfo.effectSkillManager.SkillEffectLoad(m_MyObj.gameObject, cAttackInfo.skillinfo.effBodyInfo);

                    InitAdditiveSkill(iNormalAtkID, ref effectSkillManager);
                    for (int k = 0; k < m_AttackInfos[iNormalAtkID].skillinfo.aniResInfo.Count; ++k)
                    {
                        if (m_CharacterAnimClipDic.ContainsKey(m_AttackInfos[iNormalAtkID].skillinfo.aniResInfo[k].animation_name) == false)
                        {
                            if (m_CharacterAnimClipDic.ContainsKey(m_AttackInfos[iNormalAtkID].skillinfo.aniResInfo[k].animation_name) == false)
                                m_CharacterAnimClipDic.Add(m_AttackInfos[iNormalAtkID].skillinfo.aniResInfo[k].animation_name, m_AttackInfos[iNormalAtkID].skillinfo.aniResInfo[k].animation_name);
                        }
                    }
                }
            }
        }

        if (m_AttackInfos.ContainsKey(iEndAtkID) == false)
        {
            //End Combo Attack
            cAttackInfo = SkillDataManager.instance.Load(iEndAtkID);
            if (cAttackInfo != null)
            {
                cAttackInfo.skillinfo.effectSkillManager = effectSkillManager;
                LoadBodyEffect(cAttackInfo);
                LoadBuffEffect(cAttackInfo);
                if (cAttackInfo.skillinfo.projectTileInfo.Count > 0)
                {
                    LoadProjectTileEffectSkill(cAttackInfo);
                }
                //cAttackInfo.skillinfo.effectSkillManager.SkillEffectLoad(m_MyObj.gameObject, cAttackInfo.skillinfo.effBodyInfo);

                //m_AttackInfos.Add(iEndAtkID, cAttackInfo);
                AddAttackInfo(iEndAtkID, cAttackInfo);
                m_NormalAttackIndexes.Add(iEndAtkID); ;
                cAttackInfo.skillinfo.attackType = eATTACK_TYPE.eATTACK_NORMAL;
                InitAdditiveSkill(iEndAtkID, ref effectSkillManager);
                for (int k = 0; k < m_AttackInfos[iEndAtkID].skillinfo.aniResInfo.Count; ++k)
                {
                    if (m_CharacterAnimClipDic.ContainsKey(m_AttackInfos[iEndAtkID].skillinfo.aniResInfo[k].animation_name) == false)
                    {
                        if (m_CharacterAnimClipDic.ContainsKey(m_AttackInfos[iEndAtkID].skillinfo.aniResInfo[k].animation_name) == false)
                            m_CharacterAnimClipDic.Add(m_AttackInfos[iEndAtkID].skillinfo.aniResInfo[k].animation_name, m_AttackInfos[iEndAtkID].skillinfo.aniResInfo[k].animation_name);
                    }
                }
            }
        }

        for (int i = 0; i < PlayerManager.instance.GetMaxSpecialSkillCount(this, iStance); i++)
        {
            //Chain Skill
            int iSpecialSkillID = PlayerManager.instance.GetSpecialSkill(this, iStance, i);

            if (m_AttackInfos.ContainsKey(iSpecialSkillID) == false)
            {
                cAttackInfo = SkillDataManager.instance.Load(iSpecialSkillID);
                if (cAttackInfo != null)
                {
                    cAttackInfo.skillinfo.effectSkillManager = effectSkillManager; ;
                    cAttackInfo.skillinfo.attackType = eATTACK_TYPE.eATTACK_SKILL;
                    LoadBodyEffect(cAttackInfo);
                    LoadBuffEffect(cAttackInfo);

                    if (cAttackInfo.skillinfo.action_Type == eSkillActionType.eMOVEMENT)
                    {
                        pInfo.AddAvoidSkill(iSpecialSkillID);
                    }

                    //m_AttackInfos.Add(iSpecialSkillID, cAttackInfo);
                    AddAttackInfo(iSpecialSkillID, cAttackInfo);
                    InitAdditiveSkill(iSpecialSkillID, ref effectSkillManager);
                    for (int k = 0; k < m_AttackInfos[iSpecialSkillID].skillinfo.aniResInfo.Count; ++k)
                    {
                        SkillDataInfo.AnimationResInfo aniResInfo = m_AttackInfos[iSpecialSkillID].skillinfo.aniResInfo[k];
                        if (m_CharacterSkillAnimClipDic.ContainsKey(aniResInfo.animation_name) == false)
                        {
                            m_CharacterSkillAnimClipDic.Add(aniResInfo.animation_name, aniResInfo.animation_name);
                            m_CharSkillAnimLoopDic.Add(aniResInfo.animation_name, aniResInfo.loopType);
                        }
                    }
                }
            }
        }


        List<int> playerSkillCodes;
        if (charUniqID == 0)
        {
            playerSkillCodes = SkillDataManager.instance.mySkillCodes;
        }
        else if (charUniqID == 1)
        {
            playerSkillCodes = RaidManager.instance.m_NonSyncPvpPlayerSkillCodes;
            //playerSkillCodes = SkillDataManager.instance.mySkillCodes;
        }
        else
        {
            playerSkillCodes = new List<int>();
        }

        //Whole Skill
        for (int i = 0; i < playerSkillCodes.Count; ++i)
        {
            //int mySkillsID = (int)SkillDataManager.instance.mySkillCodes[i];
            int mySkillsID = (int)playerSkillCodes[i];
            cAttackInfo = SkillDataManager.instance.Load(mySkillsID);
            if (cAttackInfo != null)
            {
                cAttackInfo.skillinfo.effectSkillManager = effectSkillManager;
                AddAttackInfo(mySkillsID, cAttackInfo);
                cAttackInfo.skillinfo.attackType = eATTACK_TYPE.eATTACK_SKILL;
                LoadBodyEffect(cAttackInfo);
                LoadBuffEffect(cAttackInfo);
                if (cAttackInfo.skillinfo.projectTileInfo.Count > 0)
                {
                    LoadProjectTileEffectSkill(cAttackInfo);
                }

                List<SkillAttributeData> SkillAtt = null;
				InitAdditiveSkill(mySkillsID, ref effectSkillManager, SkillAtt);
                InitNextSkill(mySkillsID, ref effectSkillManager, false, SkillAtt);
                InitProjectTileSkill(mySkillsID, ref effectSkillManager, SkillAtt);
                InitChainSkill(mySkillsID, ref effectSkillManager, SkillAtt);

                for (int k = 0; k < m_AttackInfos[mySkillsID].skillinfo.aniResInfo.Count; ++k)
                {
                    SkillDataInfo.AnimationResInfo aniResInfo = m_AttackInfos[mySkillsID].skillinfo.aniResInfo[k];
                    if (m_CharacterSkillAnimClipDic.ContainsKey(aniResInfo.animation_name) == false)
                    {
                        m_CharacterSkillAnimClipDic.Add(aniResInfo.animation_name, aniResInfo.animation_name);
                        m_CharSkillAnimLoopDic.Add(aniResInfo.animation_name, aniResInfo.loopType);
                    }
                }
                SkillDataManager.instance.UpgradeSkillFromAtt(cAttackInfo, SkillAtt);

            }
        }

        //Resurrection skill 
        cAttackInfo = SkillDataManager.instance.Load(pInfo.reserrectionSkillIdx);
        if (cAttackInfo != null)
        {
            cAttackInfo.skillinfo.effectSkillManager = effectSkillManager;
            //m_AttackInfos.Add(pInfo.reserrectionSkillIdx, cAttackInfo);
            AddAttackInfo(pInfo.reserrectionSkillIdx, cAttackInfo);
            cAttackInfo.skillinfo.attackType = eATTACK_TYPE.eATTACK_SKILL;
            LoadBodyEffect(cAttackInfo);
            LoadBuffEffect(cAttackInfo);
            //cAttackInfo.skillinfo.effectSkillManager.SkillEffectLoad(m_MyObj.gameObject, cAttackInfo.skillinfo.effBodyInfo);
            InitAdditiveSkill(pInfo.reserrectionSkillIdx, ref effectSkillManager);
            InitNextSkill(pInfo.reserrectionSkillIdx, ref effectSkillManager);
            InitChainSkill(pInfo.reserrectionSkillIdx, ref effectSkillManager);

            for (int k = 0; k < m_AttackInfos[pInfo.reserrectionSkillIdx].skillinfo.aniResInfo.Count; ++k)
            {
                SkillDataInfo.AnimationResInfo aniResInfo = m_AttackInfos[pInfo.reserrectionSkillIdx].skillinfo.aniResInfo[k];
                if (m_CharacterSkillAnimClipDic.ContainsKey(aniResInfo.animation_name) == false)
                {
                    m_CharacterSkillAnimClipDic.Add(aniResInfo.animation_name, aniResInfo.animation_name);
                    m_CharSkillAnimLoopDic.Add(aniResInfo.animation_name, aniResInfo.loopType);
                }
            }
        }
		PartyMemberAbilities a_kPartyMemberAbilities = PlayerManager.instance.GetPartyMemberAbilities();

		a_kPartyMemberAbilities.SetPlayerPartyAbility(charUniqID);

		m_DamageCalculationData.SetCharacterInitAbility(charUniqID, m_BuffController, pInfo, pInfo.m_ItemStatus);
	}


	public void InitAttackInfo_Npc(ref EffectSkillManager effectSkillManager)
    {
        for (int i = 0; i < m_CharAi.GetNpcProp().link_SkillCode.Count; ++i)
        {
            if (m_CharAi.GetNpcProp().link_SkillCode[i] > 0)
            {
                CAttackInfo attackinfo = SkillDataManager.instance.Load(m_CharAi.GetNpcProp().link_SkillCode[i]);
                attackinfo.skillinfo.effectSkillManager = effectSkillManager;

                //Debug.Log("m_CharAi.GetNpcProp().link_SkillCode[i] " + m_CharAi.GetNpcProp().link_SkillCode[i]);
                m_AttackInfos.Add(m_CharAi.GetNpcProp().link_SkillCode[i], attackinfo);
                m_NormalAttackIndexes.Add(m_CharAi.GetNpcProp().link_SkillCode[i]);
                attackinfo.skillinfo.attackType = eATTACK_TYPE.eATTACK_NORMAL;

                LoadBodyEffect(attackinfo);
                LoadBuffEffect(attackinfo);
                if (attackinfo.skillinfo.projectTileInfo.Count > 0)
                {
                    LoadProjectTileEffectSkill(attackinfo);
                }
                attackinfo.skillinfo.effectSkillManager.LoadEffectGuide(this, attackinfo.skillinfo.effGuideInfo);

            }
        }


        //Normal Attack ID.. for normal attack loading by animation name
        for (int i = 0; i < m_CharAi.GetNpcProp().link_SkillCode.Count; ++i)
        {
            if (m_CharAi.GetNpcProp().link_SkillCode[i] > 0)
            {
                for (int k = 0; k < m_AttackInfos[m_CharAi.GetNpcProp().link_SkillCode[i]].skillinfo.aniResInfo.Count; k++)
                {
                    SkillDataInfo.AnimationResInfo aniResInfo = m_AttackInfos[m_CharAi.GetNpcProp().link_SkillCode[i]].skillinfo.aniResInfo[k];
                    if (m_CharacterSkillAnimClipDic.ContainsKey(aniResInfo.animation_name) == false)
                    {
                        m_CharacterSkillAnimClipDic.Add(aniResInfo.animation_name, aniResInfo.animation_name);
                        m_CharSkillAnimLoopDic.Add(aniResInfo.animation_name, aniResInfo.loopType);
                    }
                }
            }
        }

        //NPC_SKILL_ID. for setting attack infos
        for (int i = 0; i < m_CharAi.GetNpcProp().Npc_skill_Id.Length; ++i)
        {
            if (m_CharAi.GetNpcProp().Npc_skill_Id[i] > 0)
            {
                CAttackInfo attackinfo = SkillDataManager.instance.Load(m_CharAi.GetNpcProp().Npc_skill_Id[i]);
                attackinfo.skillinfo.effectSkillManager = effectSkillManager;
                if (m_AttackInfos.ContainsKey(m_CharAi.GetNpcProp().Npc_skill_Id[i]) == false)
                {
                    m_AttackInfos.Add(m_CharAi.GetNpcProp().Npc_skill_Id[i], attackinfo);
                }
                attackinfo.skillinfo.attackType = eATTACK_TYPE.eATTACK_SKILL;
                LoadBodyEffect(attackinfo);
                LoadBuffEffect(attackinfo);
                if (attackinfo.skillinfo.projectTileInfo.Count > 0)
                {
                    //Debug.Log("I am " + this + "/ Skill Code = " + attackinfo.skillinfo.skill_Code);
                    LoadProjectTileEffectSkill(attackinfo);
                }
                attackinfo.skillinfo.effectSkillManager.LoadEffectGuide(this, attackinfo.skillinfo.effGuideInfo);
                InitAdditiveSkill(m_CharAi.GetNpcProp().Npc_skill_Id[i], ref effectSkillManager);
            }
        }

        if (m_CharAi.GetNpcProp().Npc_Class == eNPC_CLASS.eBOSS || m_CharAi.GetNpcProp().Npc_Class == eNPC_CLASS.eRAIDBOSS)
        {
            m_BossCinemaEffs = new List<SkillDataInfo.EffectResInfo>();
            LoadEffect_BossCinema();
        }

        //NPC_SKILL_ID. for skill loading by animation name
        for (int i = 0; i < m_CharAi.GetNpcProp().Npc_skill_Id.Length; ++i)
        {
            if (m_CharAi.GetNpcProp().Npc_skill_Id[i] > 0)
            {
                for (int k = 0; k < m_AttackInfos[m_CharAi.GetNpcProp().Npc_skill_Id[i]].skillinfo.aniResInfo.Count; k++)
                {
                    SkillDataInfo.AnimationResInfo aniResInfo = m_AttackInfos[m_CharAi.GetNpcProp().Npc_skill_Id[i]].skillinfo.aniResInfo[k];
                    if (m_CharacterSkillAnimClipDic.ContainsKey(aniResInfo.animation_name) == false)
                    {
                        m_CharacterSkillAnimClipDic.Add(aniResInfo.animation_name, aniResInfo.animation_name);
                        m_CharSkillAnimLoopDic.Add(aniResInfo.animation_name, aniResInfo.loopType);
                    }
                }
            }
        }
        m_CharAi.GetNpcProp().SpawnEffResInfo.m_EffObjList.Add(effectSkillManager.LoadEffectObject(this, m_CharAi.GetNpcProp().SpawnEffResInfo.unEffectID, m_CharAi.GetNpcProp().SpawnEffResInfo.strEffectName, GetCharEffectBone(m_CharAi.GetNpcProp().SpawnEffResInfo.strEffectTargetPosition)));

        if (this.m_CharacterType == eCharacter.eHERONPC)
        {
            effectSkillManager.LoadHitEffectObject(this, m_AttackInfos[m_CharAi.GetNpcProp().link_SkillCode[0]].skillinfo.effHitInfo[0], 10);


        }
    }

    public void InitAttackInfos()
    {
        EffectSkillManager effectSkillManager = m_MyObj.gameObject.GetComponent<EffectSkillManager>();
        if (effectSkillManager == null)
        {
            return;
        }

        //Taylor  : 3D Village
        if (m_DamageCalculationData == null)
        {
            m_DamageCalculationData = new DamageCalculationData(this);
        }
        if (m_BuffController == null)
        {
            m_BuffController = new BuffController(this);
        }
        //Taylor Finish
        m_EffectSkillManager = effectSkillManager;

        switch (m_CharacterType)
        {
            case eCharacter.eWarrior:
            case eCharacter.eArcher:
            case eCharacter.eWizard: { InitAttackInfo_Player(ref effectSkillManager); break; }
            case eCharacter.eHERONPC:
				InitAttackInfo_Npc(ref effectSkillManager);
				break;
            case eCharacter.eNPC: { InitAttackInfo_Npc(ref effectSkillManager); break; }
            default:
                {
                    break;
                }

        }
    }



	private void LoadAnimations()
    {
        string path = null;
        switch (m_CharacterType)
        {
            case eCharacter.eWarrior:
                path = "Character/Warrior/Animations/";
                break;
            case eCharacter.eWizard:
                path = "Character/Magician/Animations/";
                break;
            case eCharacter.eArcher:
                path = "Character/Archer/Animations/";
                break;
            case eCharacter.eNPC:
                path = ((NpcAI)m_CharAi).NpcModelDataRoute + "/Animations/";
                //Debug.Log("LoadAnimations path : " + path);
                break;
            case eCharacter.eHERONPC:
                path = ((HeroNpcAI)m_CharAi).NpcModelDataRoute + "/Animations/";
                //Debug.Log("LoadAnimations path : " + path);
                break;
        }

        // Only Playerable Character
        //if (m_CharacterType != eCharacter.eNPC)
        {
            //Load Basic Stance
            m_KSAnimation.InitCharacterAnimations(path, m_CharacterAnimClipDic);
            //Load Skill Animation
            m_KSAnimation.InitCharacterSkillAnimations(path, m_CharacterSkillAnimClipDic, m_CharSkillAnimLoopDic);
        }

        switch (m_CharacterType)
        {
            case eCharacter.eHERONPC:
                m_DamageCalculationData.SetInitAttack_Move_Speed(m_BuffController, null, m_CharAi.GetNpcProp());
                m_KSAnimation.SetAnimationCullType(AnimationCullingType.BasedOnRenderers);
                break;
            case eCharacter.eNPC:
                m_DamageCalculationData.SetInitAttack_Move_Speed(m_BuffController, null, m_CharAi.GetNpcProp());
                m_KSAnimation.SetAnimationCullType(AnimationCullingType.BasedOnRenderers);
                break;
            default:
                if (LoadingManager.instance.flow == eMAIN_FLOW.eINGAME)
                {
					PlayerInfo a_kPlayerInfo = PlayerManager.instance.m_PlayerInfo[ PlayerManager.MYPLAYER_INDEX ];
					List<SetItemData> setItems = InventoryManager.instance.GetSetItemDatas( a_kPlayerInfo.curEquipItems );
                    m_DamageCalculationData.SetInitAttack_Move_Speed(m_BuffController, PlayerManager.instance.GetPlayerInfo(charUniqID), null);
                }

                m_KSAnimation.SetAnimationCullType(AnimationCullingType.AlwaysAnimate);

                break;
        }
    }

    public void LookAtY(Vector3 v3TargetPosition)
    {
        Vector3 targetPosition = new Vector3(v3TargetPosition.x, this.transform.position.y, v3TargetPosition.z);
        this.transform.LookAt(targetPosition);
    }

    private bool CheckCanNpcMove(ref eAffectCode eAffect, ref eSkillActionType sActionType)
    {

        return false;
    }

    public bool CheckObjectRay(Vector3 p_veOrigin, Vector3 p_vecDir, float p_fDist, out Collider[] p_outObject)
    {
        RaycastHit[] hit = Physics.RaycastAll(p_veOrigin, p_vecDir, p_fDist);

        if (hit.Length > 0)
        {
            p_outObject = new Collider[hit.Length];
            for (int i = 0; i < p_outObject.Length; ++i)
            {
                p_outObject[0] = hit[i].collider;
            }
            return true;
        }
        else
            p_outObject = null;
        return false;
    }
    public bool CheckCanMove(Vector3 movePosition, eAffectCode eAffect, out Vector3 moveCorrectionPosition, eSkillActionType sActionType = eSkillActionType.eNORMAL)
    {
        moveCorrectionPosition = Vector3.zero;

        if (sActionType == eSkillActionType.eAERO)
            return true;

        float colliderRadius = GetRadius();
        Vector3 origin = this.transform.position + new Vector3(0.0f, 0.5f, 0.0f);
        Ray ray = new Ray(origin, movePosition);
        float fDistance = movePosition.magnitude + colliderRadius;
        RaycastHit hit;

        int iLayer = 1 << 15/*"Barricade"*/ ;
        if (sActionType != eSkillActionType.eTYPE_PENETRATION)//|| sActionType == eSkillActionType.eMOVEMENT)
            iLayer |= (1 << 11/*"NPC"*/);

        if (Physics.Raycast(ray, out hit, fDistance, iLayer) == true)
        {
            if (hit.transform.CompareTag("Col") == true)       // 벽에 닿으면...
            {
                //Vector2 v2Point = new Vector2(hit.point.x, hit.point.z);
                //Vector2 v2MovePoint = new Vector2(movePosition.x, movePosition.z);

                return false;
            }
            else if (hit.collider.CompareTag("NPC") == true)   // Npc에 닿으면...
            {
                if (sActionType == eSkillActionType.eTYPE_PENETRATION)
                {
                    return true;
                }
                else if (sActionType == eSkillActionType.eMOVEMENT)
                {
                    if (gameObject.layer == LayerMask.NameToLayer("NPC"))
                        return true;
                    else
                    {
                        CharacterBase charBase = hit.collider.gameObject.GetComponent<CharacterBase>();
                        if (charBase != null)
                        {
                            NpcInfo.NpcProp tableData = charBase.m_CharAi.GetNpcProp();
                            if (tableData != null && tableData.Npc_Size < 3 && (tableData.Npc_Type == eNPC_TYPE.eMONSTER || tableData.Npc_Type == eNPC_TYPE.eNPC))
                            {
                                return true;
                            }
                        }
                    }
                    return false;
                }
                else if (sActionType == eSkillActionType.ePUSH)
                {
                    CharacterBase charBase = hit.collider.gameObject.GetComponent<CharacterBase>();
                    if (charBase != null)
                    {
                        if (charBase.m_CharAi.GetCollisionDetection())
                        {
                            return false;
                        }
                    }
                    return true;
                }
                else
                {
                    return false;
                }
                //return CheckCanNpcMove(ref eAffect, ref sActionType );
            }
            else if (hit.collider.CompareTag("Warrior") || hit.collider.CompareTag("Magician"))   //  캐릭터
            {
                if (sActionType == eSkillActionType.eTYPE_PENETRATION || sActionType == eSkillActionType.eMOVEMENT)
                {
                    return true;
                }
                else
                {
                    CharacterBase charBase = hit.collider.gameObject.GetComponent<CharacterBase>();
                    if (charBase != null)
                    {
                        if (charBase.m_CharAi.GetCollisionDetection())
                        {
                            return false;
                        }
                    }
                    return true;
                }
            }
            else         
            {
                return true;
            }
        }

        return true;
    }
    Vector3 GetHitFromStopPoint(Vector3 p_hit, Vector3 p_origin2D, Vector3 p_movePosition, Vector3 p_dir, float p_radius)
    {
        Vector3 v2Point = new Vector3(p_hit.x, 0, p_hit.z);
        Vector3 v3Plog = v2Point - p_origin2D;
        Vector3 vLine = p_movePosition + (p_dir * p_radius);
        float fDotP = Vector2.Dot(v3Plog, vLine);

        Vector3 pProj = (fDotP / Vector3.Dot(vLine, vLine)) * vLine;

        return (v2Point + (pProj - v3Plog)) - pProj;
    }
    public bool CheckCanMoveEx(Vector3 movePosition, eAffectCode eAffect, out Vector3 moveCorrectionPosition, eSkillActionType sActionType = eSkillActionType.eNORMAL)
    {
        moveCorrectionPosition = movePosition;

        if (sActionType == eSkillActionType.eAERO)
            return true;

        float colliderRadius = GetRadius();
        Vector3 origin = this.transform.position + new Vector3(0.0f, 0.5f, 0.0f);
        Vector3 origin2D = new Vector3(origin.x, 0, origin.z);

        RaycastHit hit;
        NavMeshHit navhit;

        // 맵을 벗어 났는지 검사
        if (NavMesh.Raycast(origin, origin + movePosition, out navhit, NavMesh.AllAreas))
        //if (m_CharAi.RayCastNavMesh(origin + movePosition,out navhit))
        {
            movePosition = (navhit.position - this.transform.position);
            movePosition.y = moveCorrectionPosition.y;
        }
        if (movePosition == Vector3.zero)
        {
            moveCorrectionPosition = this.transform.position;
            return false;
        }

        int iLayer = 0;

        iLayer = 1 << LayerMask.NameToLayer("Barricade") | 1 << LayerMask.NameToLayer("Default");

        if (sActionType != eSkillActionType.eTYPE_PENETRATION)//|| sActionType == eSkillActionType.eMOVEMENT)
        {
            iLayer |= (1 << LayerMask.NameToLayer("NPC"));
            iLayer |= (1 << LayerMask.NameToLayer("Character"));
        }
        Vector3 vDir = movePosition.normalized;

        // 충돌 검사
        Ray ray = new Ray(origin, movePosition);
        float fDistance = Vector3.Distance(Vector3.zero, movePosition);

        if (Physics.SphereCast(ray, colliderRadius, out hit, fDistance, iLayer) == true)
        {
            hit.normal = new Vector3(hit.normal.x, 0, hit.normal.z).normalized;

            if (hit.transform.CompareTag("Col") == true)       // 벽에 닿으면...
            {

                moveCorrectionPosition = GetHitFromStopPoint(hit.point, origin2D, movePosition, vDir, colliderRadius);
                moveCorrectionPosition.y = this.transform.position.y;

                return false;
            }
            else if (hit.collider.CompareTag("NPC") == true)   // Npc에 닿으면...
            {
                if (sActionType == eSkillActionType.eMOVEMENT ||
                    sActionType == eSkillActionType.eTYPE_PENETRATION)
                {
                    CharacterBase charBase = hit.collider.gameObject.GetComponent<CharacterBase>();
                    if (charBase != null)
                    {
                        NpcInfo.NpcProp tableData = charBase.m_CharAi.GetNpcProp();
                        if (tableData != null && (tableData.Npc_Type == eNPC_TYPE.eMONSTER || tableData.Npc_Type == eNPC_TYPE.eNPC))
                        {
                            if (tableData.Npc_Size < 3)
                            {
                                RaycastHit hitCol;
                                iLayer = 1 << LayerMask.NameToLayer("Barricade") | 1 << LayerMask.NameToLayer("Default");
                                if (Physics.SphereCast(ray, colliderRadius, out hitCol, fDistance, iLayer) == true)
                                {
                                    hit = hitCol;
                                }
                                else
                                {
                                    moveCorrectionPosition = movePosition;
                                    return true;
                                }
                            }
                            else
                            {
                                if (m_CharState == CHAR_STATE.ALIVE)
                                    SetCollisionDetection(true);
                            }
                        }

                    }
                    moveCorrectionPosition = GetHitFromStopPoint(hit.point, origin2D, movePosition, vDir, colliderRadius); ;
                    moveCorrectionPosition.y = this.transform.position.y;

                    return false;
                }
                else if (sActionType == eSkillActionType.ePUSH)
                {
                    CharacterBase charBase = hit.collider.gameObject.GetComponent<CharacterBase>();
                    if (charBase != null)
                    {
                        if (charBase.m_CharAi.GetCollisionDetection())
                        {
                            moveCorrectionPosition = GetHitFromStopPoint(hit.point, origin2D, movePosition, vDir, colliderRadius); ;
                            moveCorrectionPosition.y = movePosition.y;

                            return false;
                        }
                    }
                    return true;
                }
                else
                {

                    moveCorrectionPosition = GetHitFromStopPoint(hit.point, origin2D, movePosition, vDir, colliderRadius); ;
                    moveCorrectionPosition.y = this.transform.position.y;

                    return false;
                }
                //return CheckCanNpcMove(ref eAffect, ref sActionType );
            }
            else if (hit.collider.CompareTag("Warrior") || hit.collider.CompareTag("Magician"))   //  캐릭터
            {
                if (sActionType == eSkillActionType.eTYPE_PENETRATION || sActionType == eSkillActionType.eMOVEMENT)
                {
                    return true;
                }
                else
                {
                    CharacterBase charBase = hit.collider.gameObject.GetComponent<CharacterBase>();
                    if (charBase != null)
                    {
                        if (charBase.m_CharAi.GetCollisionDetection())
                        {

                            moveCorrectionPosition = GetHitFromStopPoint(hit.point, origin2D, movePosition, vDir, colliderRadius); ;
                            moveCorrectionPosition.y = this.transform.position.y;

                            return false;
                        }
                    }
                    return true;
                }
            }
            else        
            {
                return true;
            }
        }
        else
        {
            moveCorrectionPosition = movePosition;
        }

        return true;
    }

    public bool Move(Vector3 movePosition, eAffectCode eAffectCode = eAffectCode.eNONE, eSkillActionType sActionType = eSkillActionType.eNORMAL)
    {
        if (m_BuffController.GetCharDisable(BuffController.eBuffCharDisable.eMOVE) > 0 && eAffectCode != eAffectCode.ePULLING_N)
            return false;

        Vector3 moveCorrectionPosition = Vector3.zero;

        if (m_UnityCharController != null && charUniqID == PlayerManager.MYPLAYER_INDEX && eAffectCode == eAffectCode.eMOVE_SPEED_UP_RATIO)
        {
            switch (sActionType)
            {
                case eSkillActionType.eTYPE_PENETRATION:
                case eSkillActionType.eMOVEMENT:
                    if (Physics.GetIgnoreLayerCollision(LayerMask.NameToLayer("Character"), LayerMask.NameToLayer("NPC")) == false)
                    {
                        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Character"), LayerMask.NameToLayer("NPC"));
                        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Character"), LayerMask.NameToLayer("Character"));
                    }
                    break;
                default:
                    if (Physics.GetIgnoreLayerCollision(LayerMask.NameToLayer("Character"), LayerMask.NameToLayer("NPC")))
                    {
                        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Character"), LayerMask.NameToLayer("NPC"), false);
                        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Character"), LayerMask.NameToLayer("Character"), false);
                    }
                    break;
            }
            CollisionFlags kflags = m_UnityCharController.Move(movePosition);

            bool bResult = false;

            switch (kflags)
            {
                case CollisionFlags.None:
                case CollisionFlags.CollidedBelow:
                case CollisionFlags.CollidedAbove:
                    bResult = true;
                    break;
                case CollisionFlags.CollidedSides:
                    bResult = false;
                    break;
                default:
                    switch (kflags)
                    {
                        case CollisionFlags.Above:
                        case CollisionFlags.Below:
                            bResult = true;
                            break;
                        case CollisionFlags.Sides:
                            bResult = false;
                            break;
                    }
                    break;
            }

            NavMeshHit hit;
            m_CharAi.SampleNavMesh(out hit);

            if (hit.hit)
                m_MyObj.position = hit.position;

            return bResult;
        }
        bool bCol = CheckCanMoveEx(movePosition, eAffectCode, out moveCorrectionPosition, sActionType);
        movePosition = moveCorrectionPosition;
        //        if (CheckCanMove(movePosition, eAffectCode, out moveCorrectionPosition, sActionType))
        //{
        if (!bCol)
            this.transform.position = movePosition;
        else
            this.transform.position += movePosition;

        bool bCheckGround = false;

        switch (m_CharacterType)
        {
            case eCharacter.eHERONPC:
                bCheckGround = !((HeroNpcAI)m_CharAi).GetCollisionDetection();
                break;
            case eCharacter.eNPC:
                bCheckGround = !((NpcAI)m_CharAi).GetCollisionDetection();
                break;
            default:
                bCheckGround = !((AutoAI)m_CharAi).GetCollisionDetection();
                break;
        }

        if (bCheckGround)
        {
            NavMeshHit hit;
            m_CharAi.SampleNavMesh(out hit);

            if (hit.hit)
                m_MyObj.position = hit.position;
        }

        return true;
    }
    
    public void SetCharacterState(CHAR_STATE state)
    {
        m_CharState = state;

        if (m_CharAi != null && (m_CharAi.enabled == true) && state == CHAR_STATE.ALIVE)
            SetCollisionDetection(true);
    }


    public void SetMotionState(MOTION_STATE eMotion_State)
    {

        m_MotionState = eMotion_State;
    }

    private void SetCantRisen()
    {
        if (m_ReactionBuffData != null)
        {
            m_BuffController.BuffEnd(m_ReactionBuffData);
        }

        if (m_charController != null)
        {
            m_charController.m_AtkComboTree = eATTACKCOMBO_TREE.NONE;
        }

        InitVintage();
        InGameManager.instance.SetAgainCurFloor();
    }

    private void GotoShop(UI_Shop.eSTATE state)
    {
        if (!InGameManager.instance.m_uiShop.gameObject.activeSelf)
            InGameManager.instance.m_uiShop.Show(state);
        else
            InGameManager.instance.m_uiShop.SetState(state);

        InGameManager.instance.m_uiShop.bFromDeath = true;
    }

    public void SendRisenPkt()
    {
        InGameManager.instance.m_bRisenPopup = false;

        if ((UserManager.instance.cash) >= payCash)
        {
            ConfirmRisen(payCash);
        }
        else
        {
            GotoShop(UI_Shop.eSTATE.CASH);
        }
    }

    public void ConfirmRisen(long payCash)
    {
        NetworkManager.instance.networkSender.SendRisenReq(blame_messages.RisenMode.MODE_CHAPTER);
    }



	public void SetCharRevival()
    {
        if (charUniqID == PlayerManager.MYPLAYER_INDEX)
        {
            m_currSkillIndex = PlayerManager.instance.m_PlayerInfo[PlayerManager.MYPLAYER_INDEX].reserrectionSkillIdx;
            damageCalculationData.fCURRENT_HIT_POINT = damageCalculationData.fMAX_HIT_POINT;

            PlayerManager.instance.m_PlayerInfo[PlayerManager.MYPLAYER_INDEX].playerCharBase.CameraAnimatorInit();
        }
        else
        {
            for (int i = PlayerManager.instance.playerInfoList.Count - 1; i >= 0; --i)
            {
                if (PlayerManager.instance.playerInfoList[i].playerCharBase != null && PlayerManager.instance.playerInfoList[i].playerCharBase.m_IngameObjectID == m_IngameObjectID)
                {
                    m_currSkillIndex = PlayerManager.instance.m_PlayerInfo[i].reserrectionSkillIdx;
                    break;
                }
            }
            damageCalculationData.fCURRENT_HIT_POINT = damageCalculationData.fMAX_HIT_POINT;
        }
        NpcManager.instance.SetSummonAllWorkReady();
        StopWiggle();

        ((AutoAI)m_CharAi).SetAutoPlayState(AutoAI.eAutoProcessState.eReady);

        SetChangeMotionState(MOTION_STATE.eMOTION_REVIVAL);

        InitVintage();

        InitCoolTime();

        if (m_Collider != null)
            m_Collider.enabled = true;
    }


    public void InitCoolTime()
    {
        //Init Cool Time Fire~
        UI_InGameButtons ingameButtons = (UI_InGameButtons)SceneManager.instance.GetUIComponent(SceneManager.eCOMPONENT.InGameButtons);
        for (int i = 0; i < ingameButtons.m_ButtonGroup.Skills.Length; ++i)
        {
            ingameButtons.m_ButtonGroup.Skills[i].ResetCoolTime();
        }
        ingameButtons.m_ButtonGroup.Ex.ResetCoolTime();

    }

    private void InitVintage()
    {
        InGameManager.instance.vintage.enabled = false;
    }

    public void CharRevivalPosition(blame_messages.BroadCastReviveRepl p_revival, bool m_bMy)
    {
        Vector3 v3Position = new Vector3((float)p_revival.revive_pos.x, (float)p_revival.revive_pos.y, (float)p_revival.revive_pos.z);
        m_MyObj.transform.position = v3Position;

        NavMeshHit hit;
        m_CharAi.SampleNavMesh(out hit);

        if (hit.hit)
            m_MyObj.transform.position = new Vector3(v3Position.x, hit.position.y, v3Position.z);
        else
            m_MyObj.transform.position = v3Position;

        SetCharacterState(CHAR_STATE.ALIVE);
        SetChangeMotionState(MOTION_STATE.eMOTION_IDLE);
        m_MyObj.transform.position = v3Position;

        if (p_revival.is_cash == true)
        {
            if (m_bMy == true)
            {
                SetCharRevival();
            }
        }
        else
        {
            damageCalculationData.fCURRENT_HIT_POINT = damageCalculationData.fMAX_HIT_POINT;
            SetCharacterState(CHAR_STATE.ALIVE);
            SetChangeMotionState(MOTION_STATE.eMOTION_IDLE);

            if (m_Collider != null)
            {
                m_Collider.enabled = true;
            }
        }
    }

    public void ImmediatelyRevival()
    {
        SceneManager.instance.DeleteUIComponent(SceneManager.eCOMPONENT.RevivalPopup);
        NetworkManager.instance.networkSender.SendPcRevive(true);
    }

    private void GoToMainLobby()
    {
        InGameManager.instance.ResumeGame();
        if (SceneManager.instance.GetCurGameMode() == SceneManager.eGAMEMODE.eMODE_RAID ||
            SceneManager.instance.GetCurGameMode() == SceneManager.eGAMEMODE.eMODE_PVP ||
            SceneManager.instance.GetCurGameMode() == SceneManager.eGAMEMODE.eMODE_FREEFIGHT)
        {
            for (int i = 0; i < RaidManager.instance.m_PartyMemberList.Length; i++)
            {
                if (RaidManager.instance.m_PartyMemberList[i].rMemInfo != null)
                {
                    RaidManager.instance.m_PartyMemberList[i].rMemInfo.strName = null;
                }
            }

            NetworkManager.instance.networkSender.SendExitRaid();
        }
        SceneManager.instance.GoDirectToMyLobby();

        EventTriggerManager.instance.Destroy();
        EffectResourcesManager.instance.m_EffectResObj.Clear();

        CinemaSceneManager.instance.DestroyCutSceneCameras();

        LoadingManager.instance.ChanageMainFlow(eMAIN_FLOW.eCONTENTS);
    }


    public void GoToPrevScene()
    {
        if (
            SceneManager.instance.GetCurGameMode() == SceneManager.eGAMEMODE.eMODE_CHAPTER
#if DAILYDG
             || SceneManager.instance.GetCurGameMode() == SceneManager.eGAMEMODE.eMODE_DAILY
#endif
            )
        {
            InGameManager.instance.SetAutoSave(true);
            SetCantRisen();
            StopWiggle();
            InGameManager.instance.m_bRisenPopup = false;
        }
        else
        {
            SceneManager.instance.ResetSceneStack(SetSceneStack(SceneManager.instance.GetCurGameMode()));

            EventTriggerManager.instance.Destroy();
            EffectResourcesManager.instance.m_EffectResObj.Clear();

            LoadingManager.instance.ChanageMainFlow(eMAIN_FLOW.eCONTENTS);

            if (SceneManager.instance.GetCurGameMode() == SceneManager.eGAMEMODE.eNPC_TEST_TOOL)
            {
                NpcTestToolManager.instance.createNpc = null;
                SceneManager.instance.SetCurGameMode(SceneManager.eGAMEMODE.eMODE_NONE);
            }
        }
        if (m_CharacterType != eCharacter.eNPC && m_CharacterType != eCharacter.eHERONPC)
        {
            m_CameraManager.m_tfCameraMove.position = new Vector3(m_MyObj.transform.position.x + 4.7f, m_MyObj.transform.position.y + 8.1f, m_MyObj.transform.position.z + 8.3f);
            m_CameraManager.m_tfCameraMove.eulerAngles = new Vector3(m_MyObj.transform.eulerAngles.x + 39, /*m_MyObj.transform.eulerAngles.y - 40*/-133, m_MyObj.transform.eulerAngles.z + 1);
        }
    }

    private Stack<SceneManager.eSCENE_STATE> SetSceneStack(SceneManager.eGAMEMODE gameMode)
    {
        Stack<SceneManager.eSCENE_STATE> sceneStack = new Stack<SceneManager.eSCENE_STATE>();

        switch (gameMode)
        {
            case SceneManager.eGAMEMODE.eMODE_CHAPTER:
#if DAILYDG
            case SceneManager.eGAMEMODE.eMODE_DAILY:
#endif
                {
                    //MyLobby
                    sceneStack.Push(SceneManager.eSCENE_STATE.MyLobby);
                    sceneStack.Push(SceneManager.eSCENE_STATE.StageSelect);
                }
                break;
            case SceneManager.eGAMEMODE.eMODE_RAID:
                {
                    //MyLobby
                    sceneStack.Push(SceneManager.eSCENE_STATE.MyLobby);
                    sceneStack.Push(SceneManager.eSCENE_STATE.WorldMap);
                    //sceneStack.Push(SceneManager.eSCENE_STATE.RaidReady);
                }
                break;
            case SceneManager.eGAMEMODE.eGOLD_DUNGEON:
            case SceneManager.eGAMEMODE.eMODE_TRIAL_DUNGEON:
            case SceneManager.eGAMEMODE.eMODE_DAY_OF_THE_WEEK_DUNGEON:
            case SceneManager.eGAMEMODE.eNPC_TEST_TOOL:
            case SceneManager.eGAMEMODE.eMODE_SCENARIO:
                {
                    //MyLobby
                    sceneStack.Push(SceneManager.eSCENE_STATE.MyLobby);
                    sceneStack.Push(SceneManager.eSCENE_STATE.WorldMap);
                }
                break;
            default:
                break;
        }

        return sceneStack;
    }
    protected void Update()
    {
#if UNITY_EDITOR // 투두
        if (charUniqID == 0 && MapManager.instance.m_bVillage == false)
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                Cheat(KeyCode.Q);
            }
            if (Input.GetKeyDown(KeyCode.Keypad1))
            {
                SkillDataInfo.SkillInfo pNewSkill = new SkillDataInfo.SkillInfo();
                pNewSkill.buff_Type = eBuffType.eBUFF;
                pNewSkill.buff_Time = new float[2];
                pNewSkill.buff_Time[0] = 60.0f;
                pNewSkill.buff_Time[1] = 0.0f;
                pNewSkill.buff_Lv = 1;
                pNewSkill.Buff_Effect = new List<int>();
                pNewSkill.Buff_Effect.Add(757);
                pNewSkill.system_Affect_Code = new List<eAffectCode>();
                pNewSkill.system_Affect_Value = new List<int>();

                pNewSkill.system_Affect_Code.Add(eAffectCode.ePHYSICAL_DEFENSE_UP_ADD);
                pNewSkill.system_Affect_Value.Add(20000);

                pNewSkill.strAtlasName = "SKILL_ICON_01_A";
                pNewSkill.strSpriteName = "warrior_AS_1";

                m_BuffController.AddBuff(BuffController.CreateBuffFactory(this, m_BuffController, pNewSkill, 0));
            }
            if (Input.GetKeyDown(KeyCode.Keypad2))
            {
                SkillDataInfo.SkillInfo pNewSkill = new SkillDataInfo.SkillInfo();
                pNewSkill.buff_Type = eBuffType.eBUFF;
                pNewSkill.buff_Time = new float[2];
                pNewSkill.buff_Time[0] = 60.0f;
                pNewSkill.buff_Time[1] = 0.0f;
                pNewSkill.buff_Lv = 1;
                pNewSkill.buff_trait = eBuff_Trait.eENRAGED;
                pNewSkill.Buff_Effect = new List<int>();
                pNewSkill.Buff_Effect.Add(761);
                pNewSkill.system_Affect_Code = new List<eAffectCode>();
                pNewSkill.system_Affect_Value = new List<int>();

                pNewSkill.system_Affect_Code.Add(eAffectCode.ePHYSICAL_ATTACK_UP_ADD);
                pNewSkill.system_Affect_Value.Add(20000);

                pNewSkill.strAtlasName = "SKILL_ICON_01_A";
                pNewSkill.strSpriteName = "warrior_AS_1";

                m_BuffController.AddBuff(BuffController.CreateBuffFactory(this, m_BuffController, pNewSkill, 0));
            }
            if (Input.GetKeyDown(KeyCode.Keypad3))
            {
                SkillDataInfo.SkillInfo pNewSkill = new SkillDataInfo.SkillInfo();
                pNewSkill.buff_Type = eBuffType.eBUFF;
                pNewSkill.buff_Time = new float[2];
                pNewSkill.buff_Time[0] = 60.0f;
                pNewSkill.buff_Time[1] = 0.0f;
                pNewSkill.buff_Lv = 1;
                pNewSkill.buff_trait = eBuff_Trait.eENRAGED;
                pNewSkill.Buff_Effect = new List<int>();
                pNewSkill.Buff_Effect.Add(761);
                pNewSkill.system_Affect_Code = new List<eAffectCode>();
                pNewSkill.system_Affect_Value = new List<int>();

                pNewSkill.system_Affect_Code.Add(eAffectCode.ePHYSICAL_ATTACK_DOWN_RATIO);
                pNewSkill.system_Affect_Value.Add(100);

                pNewSkill.strAtlasName = "SKILL_ICON_01_A";
                pNewSkill.strSpriteName = "warrior_AS_1";

                m_BuffController.AddBuff(BuffController.CreateBuffFactory(this, m_BuffController, pNewSkill, 0));
            }
        }
        if (charUniqID == 0)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                Cheat(KeyCode.E);
            }
        }
        if (charUniqID == 0)
        {
            if (Input.GetKeyDown(KeyCode.M))
            {
                Texture[] t2d = Resources.FindObjectsOfTypeAll(typeof(Texture)) as Texture[];

                float fBias = -1.5f;
                foreach (Texture t in t2d)
                {
                    t.mipMapBias = fBias;
                }
            }
        }
        if (charUniqID == 0)
        {
            if (Input.GetKeyDown(KeyCode.V))
            {
                Cheat(KeyCode.V);
            }
        }
        if (charUniqID == 0)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                for (int i = 0; i < PlayerManager.instance.playerInfoList.Count; ++i)
                {
                    if (PlayerManager.instance.playerInfoList[i].playerCharBase != null && PlayerManager.instance.playerInfoList[i].playerCharBase.charUniqID != 0)
                    {
                        ((AutoAI)PlayerManager.instance.playerInfoList[i].playerCharBase.m_CharAi).m_OnlineUser = false;
                        ((AutoAI)PlayerManager.instance.playerInfoList[i].playerCharBase.m_CharAi).SetAutoPlayState(AutoAI.eAutoProcessState.eReady);
                    }
                }
            }
        }
        if (charUniqID == 0)
        {
            if (Input.GetMouseButtonDown(1))
            {
                List<CharacterBase> targets = NpcManager.instance.GetNpcTargetsBySpwaned();

                if (targets != null)
                {
                    for (int i = 0; i < targets.Count; ++i)
                    {
                        if (targets[i].m_CharState == CHAR_STATE.ALIVE && targets[i].m_CharAi != null && ((NpcAI)targets[i].m_CharAi).m_NpcProp.Npc_Type != eNPC_TYPE.eDESTROYABLE_OBJ)
                        {
                            RaycastHit hit;
                            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, float.MaxValue, 1 << LayerMask.NameToLayer("Ground")))
                            {
                                ((NpcAI)targets[i].m_CharAi).SetMoveState(hit.point);
                            }

                            break;
                        }
                    }
                }
            }
        }
#endif

        if (m_BuffController != null)
            m_BuffController.UpdateBuff();

        if ((m_KSAnimation != null) && (m_fsm != null))
        {
            m_fsm.ObjStateMachice_Update();
        }

        if (m_evtTriggerData != null)
        {
            if (m_bTriggerMoveArea)
            {
                if (GetNearDestinationPos(((NpcAI)m_CharAi).GetNavMeshAgentDest()))
                {
                    m_bTriggerMoveArea = false;
                    ((NpcAI)m_CharAi).SetNavMeshAgent(false);
                    SetChangeMotionState(MOTION_STATE.eMOTION_IDLE);
                }
            }
        }

        if (m_fDieTimeOut > 0.0f)
        {
            m_fDieTimeOut -= Time.deltaTime;
            if (m_fDieTimeOut <= 0.0f && m_fsm.m_CurrentEnumState != MOTION_STATE.eMOTION_DIE)
            {
                BuffData_ReAction kReAction = m_BuffController.FindFrontReActionBuff();

                if (kReAction != null && kReAction.m_iReActionType == BuffData_ReAction.eReActionType.REACTION)
                {
                    m_BuffController.BuffEnd(kReAction);
                }
                SetDieState(DIE_TYPE.eDIE_STAND);
                SetChangeMotionState(MOTION_STATE.eMOTION_DIE);
            }
        }

        if (m_KSAnimation != null)
        {
            ksAnimation.UpdateAnimation();
        }

    }

    public void StartRadialBlur(float time, float value)
    {
        InGameManager.instance.Set_RadialBlur(time, value);
    }
    public void StopRadialBlur()
    {
        InGameManager.instance.StopRadialBlur();
    }


    public void SetChangeMotionState(ref ObjectState newStat)
    {
        if ((m_fsm == null) && (newStat == null)) return;
        m_fsm.ChangeState(newStat);
    }





    public void SetAttackTarget(CharacterBase newTarget, bool p_bLook = true)
    {

        if (m_TauntMove.m_bTaunt)
        {
            {
                // 도발중에는 도발자 만 설정
                if (newTarget != null && m_TauntMove.m_TauntChar != newTarget)
                {
                    newTarget = m_TauntMove.m_TauntChar;
                    attackTarget = newTarget;
                    LookAtY(attackTarget.transform.position);

                    return;
                }
            }
        }
        if (attackTarget == newTarget && attackTarget != null && p_bLook)
        {
            if (this.m_CharacterType == eCharacter.eHERONPC)
            {
                if (((HeroNpcAI)m_CharAi).m_PlayerNpcProp.Npc_Type != eNPC_TYPE.eDESTROYABLE_OBJ && ((HeroNpcAI)m_CharAi).m_PlayerNpcProp.Ai_Type != 0)
                    LookAtY(attackTarget.transform.position);
            }
            else
            {
                if (m_CharAi.m_eType == eAIType.ePC || ((NpcAI)m_CharAi).m_NpcProp.Npc_Type != eNPC_TYPE.eDESTROYABLE_OBJ && ((NpcAI)m_CharAi).m_NpcProp.Ai_Type != 0)
                    LookAtY(attackTarget.transform.position);
            }
            return;
        }
        /*
                if (attackTarget != null)
                {
                    // preTarget Send Some Event....
                }
        */
        attackTarget = newTarget;

        if (attackTarget != null && p_bLook)
        {
            //히어로 NPC 타입 추가로 수정
            if (m_CharAi.m_eType == eAIType.eHERONPC)
            {
                if (m_CharAi.m_eType == eAIType.ePC || ((HeroNpcAI)m_CharAi).m_PlayerNpcProp.Npc_Type != eNPC_TYPE.eDESTROYABLE_OBJ && ((HeroNpcAI)m_CharAi).m_PlayerNpcProp.Ai_Type != 0)
                {
                    LookAtY(attackTarget.transform.position);
                }
            }
            else
            {
                if (m_CharAi.m_eType == eAIType.ePC || ((NpcAI)m_CharAi).m_NpcProp.Npc_Type != eNPC_TYPE.eDESTROYABLE_OBJ && ((NpcAI)m_CharAi).m_NpcProp.Ai_Type != 0)
                {
                    LookAtY(attackTarget.transform.position);
                }
            }
        }
    }

    public void SetAttackPosition(Vector3 p_Pos, bool p_bLook = true)
    {
        attackPosition = p_Pos;

        if (p_bLook)
            LookAtY(p_Pos);
    }
    public CharacterBase GetAttackTarget(eTargetType type)
    {
        //Debug.Log(type);
        switch (type)
        {
            case eTargetType.eTARGET:
            case eTargetType.eTARGET_AREA:
                CAttackInfo attackInfo = GetAttackInfo(m_currSkillIndex);
                return GetTarget(attackInfo, attackInfo.skillinfo.area_Angle);/// 프로토 이후에 캐릭터에 범위값이 들어갈거라함
            case eTargetType.eSELF:
            case eTargetType.eSELF_AREA:
                return this;
            case eTargetType.eAREA:
                if (m_CharacterType == eCharacter.eNPC && m_CharacterType == eCharacter.eHERONPC)
                {
                    return null;
                }
                else
                    return this;
        }
        return null;
    }

    public void HitPoint_Attack(int hit_Idx)
    {
        CAttackInfo HitSkill = m_AttackInfos[m_UseSkillIndex];
        if (hit_Idx >= HitSkill.skillinfo.system_Affect_Code.Count)
        {
            return;
        }
        if (InGameManager.instance.m_bOffLine == false)
        {
            if (InGameManager.instance.m_bHostPlay == true)
            {
                if (charUniqID != PlayerManager.MYPLAYER_INDEX && this.m_CharacterType != eCharacter.eNPC && m_CharAi.m_OnlineUser == true)
                    return;
            }
            else 
            {
                if ((charUniqID != PlayerManager.MYPLAYER_INDEX && m_CharAi.m_eType == eAIType.ePC && m_CharAi.m_OnlineUser == true) || this.m_CharacterType == eCharacter.eNPC)
                    return;
            }
        }

            if (attackTarget == null)
                attackTarget = GetAttackTarget(HitSkill.skillinfo.target_Type);

    }

    // 데미지 판정...
    public bool GetHitSkill(CharacterBase attacker, SkillDataInfo.SkillInfo skillInfo, int hit_Idx, bool p_bCheck = false)
    {
        bool bHitFromEnemy = true;

        List<long> LinkBuffData = new List<long>();
        int iReactionStatis = 0;
        bool bBlockEffect = false;
        if (m_CharacterType != eCharacter.eNPC && m_CharacterType != eCharacter.eHERONPC && attacker != this)
        {
            if (m_MotionState == MOTION_STATE.eMOTION_SKILL)
            {
                return false;
            }
        }
        
        if (m_CharState == CHAR_STATE.DEATH)
        {
            return false;
        }

        // 온라인
        if (InGameManager.instance.m_bOffLine == false && p_bCheck == false)
        {
            switch (m_HitSkillNetData.m_statusEffect)
            {
                case blame_messages.StatusEffect.SE_CRITICAL:
                    isCritical = true;
                    break;
                case blame_messages.StatusEffect.SE_DODGE:
                    m_HitSkillNetData.m_iDamage = 0;
                    m_HitSkillNetData.m_NetBuffData.Clear();

                    if (m_CharUniqID == PlayerManager.MYPLAYER_INDEX || attacker.charUniqID == PlayerManager.MYPLAYER_INDEX)
                    {
                        if (m_CharUniqID == PlayerManager.MYPLAYER_INDEX)
                            m_damageUI.ShowEvasionUi(DamageUI.eDamage_UI_State.eEvasion);
                        else
                            m_damageUI.ShowEvasionUi(DamageUI.eDamage_UI_State.eMiss);
                    }
                    break;
                case blame_messages.StatusEffect.SE_NONE:
                    isCritical = false;
                    break;
                case blame_messages.StatusEffect.SE_HEAL:
                    break;
                case blame_messages.StatusEffect.SE_GUARD:
                    bBlockEffect = true;
                    break;
            }
        }

        else // 오프라인
		{
			if(SceneManager.instance.GetCurGameMode() == SceneManager.eGAMEMODE.eMODE_CHAPTER)
			{
				if(CheckChapterDieDamage(skillInfo, hit_Idx) == true)
				{
					return false;
				}
			}
			else if (SceneManager.instance.GetCurGameMode() == SceneManager.eGAMEMODE.eNONSYNC_PVP)
			{
				if(CheckPvpDieDamage(skillInfo, hit_Idx) == true)
				{
					return false;
				}
			}

			//적 하고만 데미지 계산
			bHitFromEnemy = CharacterBase.IsEnemy(attacker, this);

            if (bHitFromEnemy)
            {
                if (skillInfo.system_Affect_Code[hit_Idx] != eAffectCode.eNONE)
                {

                    if (DamageManager.instance.AccuracyValid(attacker.damageCalculationData, m_DamageCalculationData, skillInfo, hit_Idx) == true)
                    {

                        damage = DamageManager.instance.DamageProcess(attacker, this, skillInfo, hit_Idx, ref m_IsCritical, false);												

						SetDamageHpUI();
						if (m_CharacterType == eCharacter.eNPC || m_CharacterType == eCharacter.eHERONPC)
						{
							if (m_hpBar != null)
							{
								m_hpBar.SetHPStateDamage();
							}
						}
					}
                    else
                    {
                        if (m_CharUniqID == PlayerManager.MYPLAYER_INDEX || attacker.charUniqID == PlayerManager.MYPLAYER_INDEX)
                        {
                            if (m_CharUniqID == PlayerManager.MYPLAYER_INDEX)
                                m_damageUI.ShowEvasionUi(DamageUI.eDamage_UI_State.eEvasion);
                            else
                                m_damageUI.ShowEvasionUi(DamageUI.eDamage_UI_State.eMiss);
                        }
                        return false;
                    }
                }
                else
                {
                    damage = 0;
                }
            }
        }

        switch (skillInfo.action_Type)
        {
            case eSkillActionType.eRUSH:
                attacker.m_KSAnimation.Pause(skillInfo.Stop_time[hit_Idx]);
                attacker.m_pushInfo.m_fFreezeTime = skillInfo.Stop_time[hit_Idx];
                m_KSAnimation.Pause(skillInfo.Stop_time[hit_Idx]);
                m_pushInfo.m_fFreezeTime = skillInfo.Stop_time[hit_Idx];
                break;
            case eSkillActionType.eTYPE_PENETRATION:
                attacker.m_pushInfo.m_fFreezeTime = skillInfo.Stop_time[hit_Idx];
                break;

            default:
                //if (hit_Idx > skillInfo.Stop_time.Count - 1)
                //    Debug.Log("1");
                attacker.m_KSAnimation.Pause(skillInfo.Stop_time[hit_Idx]);
                m_DmgBMove.m_fFreezeTime = skillInfo.Stop_time[hit_Idx];
                if (charUniqID == PlayerManager.MYPLAYER_INDEX)
                    m_CameraManager.CameraAnimationPause(skillInfo.Stop_time[hit_Idx]);
                break;
        }

        if ((InGameManager.instance.m_bOffLine == true && bHitFromEnemy) || InGameManager.instance.m_bOffLine == false && m_HitSkillNetData.m_iDamage != 0)
        {
            switch (m_CharacterType)
            {
                case eCharacter.eHERONPC:   //npc part
                case eCharacter.eNPC:   //npc part
                    switch (skillInfo.direction)
                    {
                        case eDirTypeSelect.eFOWARD:
                            m_DmgBMove.m_Dir = -attacker.m_MyObj.forward;
                            break;
                        default:
                            m_DmgBMove.m_Dir = attacker.m_MyObj.position - m_MyObj.position;
                            m_DmgBMove.m_bFirstHit = true;
                            break;
                    }

                    if (damageCalculationData.fCURRENT_HIT_POINT > 0)
                    {
                        switch ((eHITANI_TYPE)skillInfo.hit_Ani[hit_Idx])
                        {
                            case eHITANI_TYPE.eNPC:
                            case eHITANI_TYPE.eNPC_PC:
                                m_BuffController.AddBuff(BuffController.CreateAttackReActionFactory(attacker, m_BuffController, skillInfo.system_Affect_Code[hit_Idx]), p_bCheck);
                                break;
                        }
                    }
                    break;
                default:    //Player Part
                    switch (skillInfo.direction)
                    {
                        case eDirTypeSelect.eFOWARD:
                            m_DmgBMove.m_Dir = -attacker.m_MyObj.forward;
                            break;
                        default:
                            m_DmgBMove.m_Dir = attacker.m_MyObj.position - m_MyObj.position;
                            m_DmgBMove.m_bFirstHit = true;
                            break;
                    }

                    if (damageCalculationData.fCURRENT_HIT_POINT > 0)
                    {
                        switch ((eHITANI_TYPE)skillInfo.hit_Ani[hit_Idx])
                        {
                            case eHITANI_TYPE.ePC:
                            case eHITANI_TYPE.eNPC_PC:
                                m_BuffController.AddBuff(BuffController.CreateAttackReActionFactory(attacker, m_BuffController, skillInfo.system_Affect_Code[hit_Idx]), p_bCheck);
                                break;
                        }
                    }
                    else
                    {
                        CameraAnimatorInit();
                        damageCalculationData.fCURRENT_HIT_POINT = 0;
                    }
                    break;
            }
        }


        m_DmgBMove.m_bAttackPush = skillInfo.AttackPush == 0;
        //net
        if (InGameManager.instance.m_bOffLine == false && p_bCheck == false)
        {
            if (m_HitSkillNetData.m_statusEffect == blame_messages.StatusEffect.SE_HEAL)
                recover = m_HitSkillNetData.m_iDamage;
            else
                damage = m_HitSkillNetData.m_iDamage;

            if (m_HitSkillNetData.m_NetBuffData.Count != 0)
            {
                for (int i = 0; i < m_HitSkillNetData.m_NetBuffData.Count; ++i)
                {
                    m_BuffController.AddBuff(m_HitSkillNetData.m_NetBuffData[i]);
                }
                m_HitSkillNetData.m_NetBuffData.Clear();
            }

            if (damage > 0)
            {
                if (m_CharUniqID == PlayerManager.MYPLAYER_INDEX || attacker.charUniqID == PlayerManager.MYPLAYER_INDEX || m_CharacterType == eCharacter.eHERONPC)
                {
                    if (skillInfo.system_Affect_Code[hit_Idx] != eAffectCode.eRESURRECTION_N)
                    {
                        m_damageUI.ShowDamageUI(damage, isCritical);
                    }
                }
                m_CharacterDamageEffect.DamageEffect();
            }


            if (m_currSkillIndex != 0)
            {
                if (m_GuardState == SHIELDGUARD_STATE.eGUARDING && bBlockEffect)
                {
                    m_eGuardingHit = COMMON_STATE.eSTART;
                    m_TimeGuardingHit = 0.2f;
                }
                else
                {
                    if (damage > 0)
                        PlayDamageEffect(skillInfo, this);
                }
            }
            else
            {
                if (damage > 0)
                    PlayDamageEffect(skillInfo, this);
            }

            if (recover > 0)
            {
                if (m_CharUniqID == PlayerManager.MYPLAYER_INDEX || attacker.charUniqID == PlayerManager.MYPLAYER_INDEX)
                {
                    m_damageUI.ShowHealUI((int)recover);
                }
            }


            //히어로 NPC 타입 추가로 수정
            if (m_CharAi.m_eType == eAIType.eNPC || m_CharAi.m_eType == eAIType.eHERONPC)
            {
                ((NpcAI)m_CharAi).GatherAggro(attacker.gameObject, (uint)( damage));
                if (((NpcAI)m_CharAi).IsBattle() == false && (InGameManager.instance.m_bOffLine == true || InGameManager.instance.m_bHostPlay == true))
                {
                    SetAttackTarget(attacker);
                }
            }

            CheckDie(DIE_TYPE.eDIE_NORMAL);

            return true;
        }
        else if (bHitFromEnemy)
        {
            if (p_bCheck == false)
            {
                if (damage > 0)
                {

                    damageCalculationData.fCURRENT_HIT_POINT -= damage;

                    SetDamageHpUI();
					
					
					if (m_CharacterType == eCharacter.eNPC || m_CharacterType == eCharacter.eHERONPC)
					{
						if (m_hpBar != null)
						{
							m_hpBar.SetHPStateDamage();
						}
					}

                    m_CharacterDamageEffect.DamageEffect();

                    {
                        switch (attacker.m_CharacterType)
                        {
                            case eCharacter.eWarrior:
                                if (isCritical)
                                {
                                    //ImpactName = "BladeExp_mpact";
                                    ImpactName = "BloodExp_Impact";
                                }
                                else
                                {
                                    ImpactName = "Blade_Impact";
                                }
                                break;
                            case eCharacter.eWizard:
                                ImpactName = "Magic_Normal_Impact";
                                break;
                            //히어로 소환수 추가
                            case eCharacter.eHERONPC:
                            case eCharacter.eNPC:
                                ImpactName = "Blade_Impact";
                                break;
                        }
                        m_SoundSednder.Play(ImpactName);
                    }
					if (InGameManager.instance.m_bOffLine)
                        PlayDamageEffect(skillInfo, this);

                }
                if (m_CharAi.m_eType == eAIType.eNPC)
                {
                    ((NpcAI)m_CharAi).GatherAggro(attacker.gameObject, (uint)(/*pInfo.factorInfo.anger_Ratio8 */ damage));

                    if (((NpcAI)m_CharAi).IsBattle() == false)
                    {
                        if (QuestIngameManager.instance.safeConductCharacterBase != this)
                        {
                            SetAttackTarget(attacker);
                        }
                    }
                }

                if (damageCalculationData.fCURRENT_HIT_POINT <= 0)
                {
                    damageCalculationData.fCURRENT_HIT_POINT = 0;
                }
                //else if (damage != 0)
                //{
                //    attacker.damageCalculationData.fCURRENT_HIT_POINT += DamageManager.instance.HPDrainProcess((int)damage, attacker.damageCalculationData, skillInfo, hit_Idx);
                //    if (damageCalculationData.fCURRENT_HIT_POINT > damageCalculationData.fMAX_HIT_POINT)
                //    {
                //        damageCalculationData.fCURRENT_HIT_POINT = damageCalculationData.fMAX_HIT_POINT;
                //    }
                //}
            }
        }

        if (hit_Idx < skillInfo.link_Skill_Code.Count && LinkBuffData.Count == 0)
        {
            int rnd = Random.Range(1, 100);

            if (0 <= (skillInfo.link_Skill_Rate[hit_Idx] - rnd))
            {
                BuffLinkSkill(attacker, skillInfo, hit_Idx, p_bCheck, ref LinkBuffData, ref iReactionStatis);
            }
        }

        if (hit_Idx < skillInfo.chain_Condition.Count)
        {
            for (int i = 0; i < skillInfo.chain_Condition.Count; ++i)
            {
                if (skillInfo.chain_Condition[i] != 0 &&
                    m_BuffController.FindBuff((eBuff_Trait)skillInfo.chain_Condition[i]) != null &&
                    UtilManager.instance.RandomPercent((int)skillInfo.chain_Affect_Rate[hit_Idx]))
                {
                    BuffChainSkill(attacker, skillInfo, hit_Idx, p_bCheck, ref LinkBuffData, ref iReactionStatis);
                }
            }
        }

        CheckDie(DIE_TYPE.eDIE_NORMAL);

        //net
        if (InGameManager.instance.m_bOffLine == false && p_bCheck == true)
        {
            m_HitSkillNetData.m_bActive = true;
            m_HitSkillNetData.m_iDamage = (long)damage;
            m_HitSkillNetData.m_iReactionStatus = iReactionStatis;
            m_HitSkillNetData.m_iApplyBuffListSend.Clear();
            m_HitSkillNetData.m_iApplyBuffListSend.AddRange(LinkBuffData);
        }

        isCritical = false;
        return true;
    }

	private bool CheckChapterDieDamage(SkillDataInfo.SkillInfo skillInfo, int hit_Idx)
    {
        if (charUniqID == PlayerManager.MYPLAYER_INDEX)
        {

            if (NpcManager.instance.NpcBoss != null && NpcManager.instance.NpcBoss.m_CharState == CHAR_STATE.DEATH)
            {
                //Debug.LogError("charUniqID == PlayerManager.MYPLAYER_INDEX, 보스 사망이후 데미지 무시");
                return true;
            }
        }
        else
        {

            if (PlayerManager.instance.m_PlayerInfo[PlayerManager.MYPLAYER_INDEX].playerCharBase.m_CharState == CHAR_STATE.DEATH &&
                (skillInfo.system_Affect_Code[hit_Idx] != eAffectCode.eRESURRECTION && skillInfo.system_Affect_Code[hit_Idx] != eAffectCode.eRESURRECTION_N))
            {

                return true;
            }
        }
        return false;
    }
	private bool CheckPvpDieDamage(SkillDataInfo.SkillInfo skillInfo, int hit_Idx)
	{
        if (m_CharState == CHAR_STATE.DEATH && (skillInfo.system_Affect_Code[hit_Idx] != eAffectCode.eRESURRECTION && skillInfo.system_Affect_Code[hit_Idx] != eAffectCode.eRESURRECTION_N))
		{		
			return true;
		}
		return false;
	}

    public void BuffLinkSkill(CharacterBase p_Attacker, SkillDataInfo.SkillInfo p_SkillInfo, int p_iHit, bool p_bCheck, ref List<long> p_LinkBuff, ref int p_iReactionStatus)
    {
        if (p_SkillInfo.link_Skill_Code.Count <= p_iHit)
            return;

        CAttackInfo attackInfo = p_Attacker.GetAttackInfo(p_SkillInfo.link_Skill_Code[p_iHit]);

        if (attackInfo == null)
            return;

        bool bEnemy = CharacterBase.IsEnemy(p_Attacker, this);

        for (int i = 0; i < attackInfo.skillinfo.system_Affect_Code.Count; ++i)
        {
            if ((!bEnemy && attackInfo.skillinfo.target_State == eTargetState.eFRIENDLY) ||
                (bEnemy && attackInfo.skillinfo.target_State == eTargetState.eHOSTILE) ||
                attackInfo.skillinfo.target_State == eTargetState.eNONE)
            {
                BuffData buff = BuffController.CreateBuffFactory(p_Attacker, m_BuffController, attackInfo.skillinfo, i);

                if (buff != null)
                {
                    if (buff.m_AffectCode == eAffectCode.eHP_DRAIN_RATIO)
                    {
                        p_LinkBuff.Add((int)p_Attacker.m_BuffController.AddBuff(buff, p_bCheck));
                    }
                    else
                    {
                        p_LinkBuff.Add((int)m_BuffController.AddBuff(buff, p_bCheck));
                    }
                    if (p_LinkBuff[p_LinkBuff.Count - 1] == (int)BuffController.eBuffResult.eSUCCESS && buff.m_eBuffStyle == BuffData.BUFF_STYLE.BUFF_STYLE_REACTION)
                    {
                        ++p_iReactionStatus;
                    }
                    else if (m_CharAi.m_eType == eAIType.eNPC && m_CharAi.GetNpcProp().Npc_Class == eNPC_CLASS.eRAIDBOSS && buff.m_eBuffStyle == BuffData.BUFF_STYLE.BUFF_STYLE_REACTION)
                    {
                        ++p_iReactionStatus;
                    }

                    if (InGameManager.instance.m_bOffLine == true)
                    {
                        if (p_LinkBuff[p_LinkBuff.Count - 1] == (int)BuffController.eBuffResult.eRESIST && buff.m_eBuffStyle == BuffData.BUFF_STYLE.BUFF_STYLE_REACTION)
                        {
                            if (m_CharUniqID == PlayerManager.MYPLAYER_INDEX)
                                m_damageUI.ShowStateUi(DamageUI.eDamage_UI_State.eResistMe);
                            else
                            {
                                try
                                {
                                    if (m_CharAi.GetNpcProp().Npc_Type != eNPC_TYPE.eDESTROYABLE_OBJ)
                                    {
                                        m_damageUI.ShowStateUi(DamageUI.eDamage_UI_State.eResistOther);
                                    }
                                }
                                catch(System.Exception e)
                                {
                                    Debug.Log("BuffLinkSkill error = "+e.ToString());
                                }
                            }
                        }
                    }
                }
            }
            else
                p_LinkBuff.Add((int)BuffController.eBuffResult.eNONE);
        }


    }

    public void BuffChainSkill(CharacterBase p_Attacker, SkillDataInfo.SkillInfo p_SkillInfo, int p_iHit, bool p_bCheck, ref List<long> p_LinkBuff, ref int p_iReactionStatus)
    {
        if (p_SkillInfo.chain_Affect_Code.Count <= p_iHit)
            return;

        CAttackInfo attackInfo = p_Attacker.GetAttackInfo(p_SkillInfo.chain_Affect_Code[p_iHit]);

        if (attackInfo == null)
            return;

        for (int i = 0; i < attackInfo.skillinfo.system_Affect_Code.Count; ++i)
        {
            BuffData buff = BuffController.CreateBuffFactory(p_Attacker, m_BuffController, attackInfo.skillinfo, i);
            p_LinkBuff.Add((int)m_BuffController.AddBuff(buff, p_bCheck));

            if (p_LinkBuff[p_LinkBuff.Count - 1] == (int)BuffController.eBuffResult.eSUCCESS && buff.m_eBuffStyle == BuffData.BUFF_STYLE.BUFF_STYLE_REACTION)
            {
                ++p_iReactionStatus;
            }
            else if (m_CharAi.m_eType == eAIType.eNPC && m_CharAi.GetNpcProp().Npc_Class == eNPC_CLASS.eRAIDBOSS && buff.m_eBuffStyle == BuffData.BUFF_STYLE.BUFF_STYLE_REACTION)
            {
                ++p_iReactionStatus;
            }
        }

        BuffLinkSkill(p_Attacker, attackInfo.skillinfo, p_iHit, p_bCheck, ref p_LinkBuff, ref p_iReactionStatus);
    }
    public void PlayDamageEffect(SkillDataInfo.SkillInfo skillInfo, CharacterBase cBase)
    {
        for (int i = 0; i < skillInfo.effHitInfo.Count; i++)
        {
            if (cBase.m_CharacterType == eCharacter.eWarrior || cBase.m_CharacterType == eCharacter.eWizard || cBase.m_CharacterType == eCharacter.eArcher || cBase.m_CharacterType == eCharacter.eHERONPC)
            {
                skillInfo.effectSkillManager.FXEffectPlay(cBase, skillInfo.effHitInfo[i], cBase.GetCharEffectBone(skillInfo.effHitInfo[i].strEffectTargetPosition));
            }
            else
            {
                skillInfo.effectSkillManager.FXHitEffectPlay(cBase, skillInfo.effHitInfo[i], cBase.GetCharEffectBone(skillInfo.effHitInfo[i].strEffectTargetPosition));
            }
        }
    }

    public void GetHitBuff(BuffData p_BuffData)
    {
        double damage = 0;
        double recover = 0;
        double fPerHp = 0;

        if (m_CharState == CHAR_STATE.DEATH || p_BuffData.m_SourceChar == null)
            return;

        CAttackInfo _skillInfo = p_BuffData.m_SourceChar.GetAttackInfo(p_BuffData.m_iSkillCode);

        // 버프 값
        switch (p_BuffData.m_AffectCode)
        {
            case eAffectCode.eDOT_DAMAGE_N:
                damage = (double)(_skillInfo.skillinfo.system_Affect_Value[0] / (_skillInfo.skillinfo.buff_Time[0] / _skillInfo.skillinfo.buff_Time[1]));
                break;
            case eAffectCode.eHP_RECOVERY_ADD:
                recover = (double)(_skillInfo.skillinfo.system_Affect_Value[0] / (_skillInfo.skillinfo.buff_Time[0] / _skillInfo.skillinfo.buff_Time[1]));
                break;
            case eAffectCode.eHP_RECOVERY_RATIO:
                fPerHp = (damageCalculationData.fMAX_HIT_POINT / 100.0f) * (double)_skillInfo.skillinfo.system_Affect_Value[0];
                recover = (double)(fPerHp / (_skillInfo.skillinfo.buff_Time[0] / _skillInfo.skillinfo.buff_Time[1]));
                break;
            case eAffectCode.eHP_DRAIN_RATIO:
                double Recover = damageCalculationData.fMAX_HIT_POINT;
                recover = (double)fPerHp;
                break;
            case eAffectCode.eDOT_DAMAGE_RATIO:
                double dotDamage = DamageManager.instance.DamageProcess(p_BuffData.m_SourceChar, this, _skillInfo.skillinfo, 0, ref m_IsCritical, true);
                damage = (dotDamage * p_BuffData.m_AffectValue /100);
                if (damage < 1) damage = 1;
                break;
            default:
                damage = DamageManager.instance.DamageProcess(p_BuffData.m_SourceChar, this, _skillInfo.skillinfo, 0, ref m_IsCritical, true);
                break;
        }

        if (InGameManager.instance.m_bOffLine == false && p_BuffData.m_SourceChar.charUniqID == PlayerManager.MYPLAYER_INDEX)
        {
            List<CharacterBase> kTargets = new List<CharacterBase>();
            kTargets.Add(this);

            m_HitSkillNetData.m_bActive = true;
            m_HitSkillNetData.m_iDamage = (long)damage;
            m_HitSkillNetData.m_iApplyBuffListSend.Clear();
            m_HitSkillNetData.m_iApplyBuffListSend.Add((int)BuffController.eBuffResult.eSUCCESS);

   
            return;
        }
        else
        {
            if (_skillInfo != null && _skillInfo.skillinfo != null)
            {
                if (damage > 0)
                {
                    damageCalculationData.fCURRENT_HIT_POINT -= damage;


					CheckDie(DIE_TYPE.eDIE_STAND);
                }
                else if (recover > 0)
                {
                    damageCalculationData.fCURRENT_HIT_POINT += recover;

                    if (damageCalculationData.fCURRENT_HIT_POINT >= damageCalculationData.fMAX_HIT_POINT)
                        damageCalculationData.fCURRENT_HIT_POINT = damageCalculationData.fMAX_HIT_POINT;

                    //if (m_CharUniqID == PlayerManager.MYPLAYER_INDEX || p_BuffData.m_SourceChar.charUniqID == PlayerManager.MYPLAYER_INDEX)
                    m_damageUI.ShowHealUI(recover);

                }
            }
        }

        if (m_CharacterType == eCharacter.eNPC || m_CharacterType == eCharacter.eHERONPC)
        {
            if (m_hpBar != null)
            {
				if(m_CharState == CHAR_STATE.ALIVE)
				m_hpBar.SetHPStateDamage();
            }
        }

    }

	public bool MyHellSkillHpCheck(CAttackInfo kSkillAttackInfo)
	{
		CAttackInfo a_kHellSkillCheck = null;
		int a_kLinkSkillCode = 0;

		if(m_AttackInfos.ContainsKey(kSkillAttackInfo.skillinfo.link_Skill_Code[0]) == false)
		{
			return true;

		}

		a_kLinkSkillCode = kSkillAttackInfo.skillinfo.link_Skill_Code[0];
		a_kHellSkillCheck = m_AttackInfos[a_kLinkSkillCode];

		if (a_kHellSkillCheck.skillinfo.system_Affect_Code[0] == eAffectCode.eHP_RECOVERY_RATIO)
		{
			if (damageCalculationData.fCURRENT_HIT_POINT < damageCalculationData.fMAX_HIT_POINT)
			{
				return true;
			}
		}
		else
		{
			return true;
		}

		return false;
	}
    private void SummonUIRefresh()
    {
        if (m_CharacterType != eCharacter.eHERONPC)
        {
            return;
        }
        InGameManager.instance.UiEquipSummonHpRefresh(this);
    }

    public void CheckDie(DIE_TYPE p_eDieType)
    {
        if (m_CharacterType == eCharacter.eNPC)
        {
            if (m_CharAi.GetNpcProp().Npc_Class == eNPC_CLASS.eBOSS)
            {
                //UnityEngine.Debug.Log("CheckDie  #### curHP = " + damageCalculationData.fCURRENT_HIT_POINT + "/m_CharState = " + m_CharState);
            }
        }

        if (damageCalculationData.fCURRENT_HIT_POINT <= 0 && m_CharState != CHAR_STATE.DEATH)
        {

            SetCharacterState(CHAR_STATE.DEATH);


			//Debug.Log("#### curGameMode = "+ SceneManager.instance.GetCurGameMode());
			
			if (SceneManager.instance.GetCurGameMode() == SceneManager.eGAMEMODE.eNONSYNC_PVP)
            {				
				switch ( m_CharUniqID )
				{
					case 0:
						InGameManager.instance.m_myPartys.RemoveAt( 0 );
						if( InGameManager.instance.m_myPartys.Count == 0 )
						{
							InGameItemManager.instance.ClearPvp( 1 );
							bPvpLastDie = true;
						}
						break;
					case 1:
						InGameManager.instance.m_otherPartys.RemoveAt( 0 );
						if( InGameManager.instance.m_otherPartys.Count == 0 )
						{
							InGameItemManager.instance.ClearPvp( 0 );
							bPvpLastDie = true;
						}
						break;
				}
            }
            else
            {
                if (EventTriggerManager.instance.m_bPressButtonBoss == false)
                {
                    InGameManager.instance.m_uiInGameInfo.SetKillCount();
                }
            }

            m_BuffController.DelBuffCondition(BuffController.eBuffDeleteCondition.eDEAD);

            BuffData_ReAction kReAction = m_BuffController.FindFrontReActionBuff();

            m_fDieTimeOut = 10.0f;
            m_bDieImed = true;

            switch (m_CharacterType)
            {
                // 바로 죽는 애니
                case eCharacter.eHERONPC:
                    {
                        HeroNpcAI Playernpcai = (HeroNpcAI)m_CharAi;

                        if (Playernpcai != null)
                        {
                            NpcInfo.NpcProp npcProp = Playernpcai.GetNpcProp();

                            if (InGameManager.instance.m_bOffLine == true)
                            {
                                if (kReAction != null && kReAction.m_iReActionType == BuffData_ReAction.eReActionType.REACTION && kReAction.m_AffectCode != eAffectCode.eSTANDUP)
                                {
                                    m_BuffController.BuffEnd(kReAction);
                                    //return; // 애니 후 죽음
                                }

                            }
                            else
                            {
                                m_BuffController.BuffEnd(kReAction);
                            }

                            if (SceneManager.instance.GetCurGameMode() == SceneManager.eGAMEMODE.eNONSYNC_PVP)
                            {
                                InGameManager.instance.m_uiIngameSummonInfo.Set_4v4PvpDieDisplay(this);
								m_BuffController.AllBuffEnd();
							}
                            else

                            {
                                InGameManager.instance.m_uiSummon.m_SummonInfo.SetSummonCheckDie(npcProp.Summon_nContentIndex, true);
                            }
                        }
                    }
                    break;
                case eCharacter.eNPC:
                    {
                        NpcAI npcai = (NpcAI)m_CharAi;
                        if (m_hpBar != null)
                        {
                            m_hpBar.JustActive(false);
                        }
                        if (npcai != null)
                        {
                            NpcInfo.NpcProp npcProp = npcai.GetNpcProp();
                            InGameItemManager.instance.SetNpcDropReward(m_MyObj.transform.position, npcProp);
                            if (npcProp.Battle_Type == eNPC_BATTLE_TYPE.eOBJECT)
                            {
                                m_EffectSkillManager.FX_Die(npcai.GetNpcProp());//Just only object type
                            }
#if ADD_TUTORIAL
							else
							{
								if (TutorialManager.instance.m_TutorialStepType == eTUTORIAL_STEP.NPCKILL &&
									((PlayerManager.instance.playerInfoList[PlayerManager.MYPLAYER_INDEX].m_CurFloor == 1 && PlayerManager.instance.playerInfoList[PlayerManager.MYPLAYER_INDEX].m_MaxRebirthTimes == 0)
									|| (PlayerManager.instance.playerInfoList[PlayerManager.MYPLAYER_INDEX].m_CurFloor == 2 && PlayerManager.instance.playerInfoList[PlayerManager.MYPLAYER_INDEX].m_MaxRebirthTimes == 0)))
								{
									TutorialManager.instance.Check_CompleteTutorial(PlayerManager.instance.playerInfoList[PlayerManager.MYPLAYER_INDEX].m_CurFloor);
								}
							}
#endif
						}


                        if (InGameManager.instance.m_bOffLine == true)
                        {
                            if (kReAction != null && kReAction.m_iReActionType == BuffData_ReAction.eReActionType.REACTION && kReAction.m_AffectCode != eAffectCode.eSTANDUP)
                            {
                                return; // 애니 후 죽음
                            }
                        }
                        else
                        {
                            m_BuffController.BuffEnd(kReAction);
                        }
                    }
                    break;
                default:
                    {
                        InGameManager.instance.SetSimpleCharacterInfo(InGameManager.eGAME_SAVE_STATE.NONE);

#if NONSYNC_PVP
                        else
                        {
                            if(bPvpLastDie)
                            {
                                Time.timeScale = 0.5f;
                            }
                        }
#endif

                        if (InGameManager.instance.m_bOffLine == true)
                        {
                            // 애니 후 죽음
                            if (kReAction != null && kReAction.m_iReActionType == BuffData_ReAction.eReActionType.REACTION && kReAction.m_AffectCode != eAffectCode.eSTANDUP)
                                return;
                        }
                        else
                        {
                            m_BuffController.BuffEnd(kReAction);
                        }
                    }
                    break;
            }
            // 사망
            ///ksAnimation.bDontPlay = false;
            SetChangeMotionState(MOTION_STATE.eMOTION_DIE);
            SetDieState(p_eDieType);
        }
    }

    public void AddExtraBuff(int p_iSkill_code, BuffData p_kBuff, float p_fExtraValue)
    {
        if (p_kBuff != null)
        {
            switch (p_kBuff.m_AffectCode)
            {
                case eAffectCode.ePUSHING:
                    if (p_kBuff.m_SourceChar != null)
                    {
                        m_PushingMove.m_Dir = p_kBuff.m_SourceChar.m_MyObj.forward;
                        m_PushingMove.m_fPushingTime = (float)p_kBuff.m_AffectValue / 1000.0f;
                        m_PushingMove.m_Speed = p_fExtraValue;
                    }
                    break;
                case eAffectCode.eSCREW:
                    if (p_kBuff.m_SourceChar != null)
                    {
                        CAttackInfo kAttackInfo = p_kBuff.m_SourceChar.GetAttackInfo(p_iSkill_code);

                        if (kAttackInfo != null)
                        {
                            Vector3 ScrewPosition = new Vector3(((float)p_kBuff.m_AffectValue) / 1000.0f, p_kBuff.m_SourceChar.m_MyObj.position.y, ((float)p_fExtraValue) / 1000.0f);

                            m_ScrewMove.m_CenterPos = ScrewPosition;
                            m_ScrewMove.m_Dir = (ScrewPosition - m_MyObj.position);
                            m_ScrewMove.m_Dir.y = 0.0f;
                            m_ScrewMove.m_Dir.Normalize();
                            if (p_kBuff.m_AffectValue == 0 && p_fExtraValue == 0.0f)
                                m_ScrewMove.m_Speed = 0.0f;
                            else
                                m_ScrewMove.m_Speed = kAttackInfo.skillinfo.projectTileInfo[0].Options[0];

                            m_ScrewMove.m_fScrewTime = 0.0f;

                            m_ScrewMove.m_fScrewTotlaTime = kAttackInfo.skillinfo.buff_Time[0];
                            m_ScrewMove.m_fScrewIntervalTime = kAttackInfo.skillinfo.buff_Time[1];
                            if (p_kBuff.m_AffectValue == 0 && p_fExtraValue == 0.0f)
                                m_ScrewMove.m_fShakeRadius = 0.0f;
                            else
                                m_ScrewMove.m_fShakeRadius = kAttackInfo.skillinfo.projectTileInfo[0].Options[1];
                        }
                    }
                    break;
                case eAffectCode.eGUARDBREAK:
                    if (p_kBuff.m_SourceChar != null)
                    {
                        //                        m_bGuardAvail = false;
                        ShieldGuard_Unlock();
                        if (charUniqID == PlayerManager.MYPLAYER_INDEX)
                        {
                        }

                    }
                    break;
            }
            m_BuffController.AddBuff(p_kBuff);
        }
    }

    public void GetNormalAttackTarget(float fSemiAngle, float fAutoAngle, int nNonDashDistance, int nDashDistance, int nMaxDashDistance, ref GameObject goTarget, ref eDASH_TYPE eDashType)
    {
        List<CharacterBase> monstersInSector = null;// NpcManager.instance.m_secNpcInfo[InGameManager.instance.m_curSectorCnt].m_secNpcCharObjects;

        CharacterBase goTargetInSemiAngle = GetTargetInAngle(fSemiAngle, nMaxDashDistance, monstersInSector);

        if (goTargetInSemiAngle != null)
        {
            eDashType = GetDashType(nNonDashDistance, nDashDistance, nMaxDashDistance, goTargetInSemiAngle.m_MyObj);
            goTarget = goTargetInSemiAngle.gameObject;
            return;
        }

        /// 세미오토의 범위 안에 적이 없으면 오토 범위 안의 적을 찾음
        CharacterBase goTargetInAutoAngle = GetTargetInAngle(fAutoAngle, nMaxDashDistance, monstersInSector);

        if (goTargetInAutoAngle != null)
        {
            eDashType = GetDashType(nNonDashDistance, nDashDistance, nMaxDashDistance, goTargetInAutoAngle.m_MyObj);
            goTarget = goTargetInAutoAngle.gameObject;
            return;
        }
        else if (goTargetInAutoAngle == null)
        {
            goTarget = null;
            eDashType = eDASH_TYPE.NONE;
        }
    }



    private eDASH_TYPE GetDashType(int nNonDashDistance, int nDashDistance, int nMaxDashDistance, Transform trTarget)
    {
        Vector2 v2This = new Vector2(m_MyObj.position.x, m_MyObj.position.z);
        Vector2 v2Target = new Vector2(trTarget.position.x, m_MyObj.position.z);

        float fTargetDistance = Vector2.Distance(v2This, v2Target);

        if (fTargetDistance <= nNonDashDistance)
            return eDASH_TYPE.DEAD_ZONE;
        else if (fTargetDistance <= nDashDistance)
            return eDASH_TYPE.DASH;
        else if (fTargetDistance <= nMaxDashDistance)
            return eDASH_TYPE.MAXIMUN;
        else
            return eDASH_TYPE.NONE;
    }

    private CharacterBase CheckTargetInAngle(float fAngle, float fDistance, CharacterBase target)
    {
        Vector2 v2Pos1 = new Vector2(m_MyObj.position.x, m_MyObj.position.z);
        Vector2 v2Pos2 = Vector2.zero;
        Vector2 v2ForwardDirection = new Vector2(m_MyObj.forward.x, m_MyObj.forward.z);

        //float fHalfAngle = fAngle * 0.5f;

        CharacterBase goClosedTarget = null;
        float fClosedTargetDistance = 0;

        /// 거리 체크
        v2Pos2 = new Vector2(target.transform.position.x, target.transform.position.z);

        float fTargetDist = Vector2.Distance(v2Pos1, v2Pos2);

        /// 사거리 안에 들어오면,,
        if (fTargetDist < fDistance)
        {
            Vector2 v2TargetDirection = v2Pos2 - v2Pos1;
            float fTargetAngle = Vector2.Angle(v2ForwardDirection, v2TargetDirection);

            /// 범위각 안에 들면,,
            if (fTargetAngle <= fAngle)
            {
                /// 타겟이 비어있으면 값 넣음
                if (goClosedTarget == null)
                {
                    goClosedTarget = target;
                    fClosedTargetDistance = fTargetDist;
                }
                else  /// 타겟이 비어있지 않은 경우
                {
                    if (fTargetDist < fClosedTargetDistance)
                    {
                        goClosedTarget = target;
                        fClosedTargetDistance = fTargetDist;
                    }
                }
            }
        }

        return goClosedTarget;
    }


    public CharacterBase GetTarget(CAttackInfo info, float fAngle)
    {
        CharacterBase target = null;

        switch (m_CharacterType)
        {
            case eCharacter.eNPC:
                return m_AttackTarget;
            default:
                //현재는 한 그룹뿐입니다. 차후에 수정해서 올리겠습니다.
                target = GetTargetInAngle(fAngle, info.skillinfo.skill_Dist, m_skSender.GetAllTargets(this, eTargetState.eHOSTILE));
                break;
        }

        if (target == null)
        {
            if (info.skillinfo.action_Type == eSkillActionType.eMAGICCIRCLE)
            {
                target = this;
            }
            return null;
        }
        else
        {
            return target;
        }
    }


    public void CameraAnimatorInit()
    {
        if (charUniqID == PlayerManager.MYPLAYER_INDEX)
            m_CameraManager.CameraAnimatorInit();
    }

    public void StartNormalComboDash(float time)
    {
        float speed = 0.0f;
        float dist = 0.0f;
        float pushTime = 0.0f;
        if (time != 0)
        {
            pushTime = time / 30;

            //Debug.Log("StartNormalComboDash() attack tartet = " + attackTarget);
            if (attackTarget != null)
            {
                //Debug.Log("StartComboDash() attack target = "+attackTarget);
                LookAtY(attackTarget.m_MyObj.position);
            }
            //else
            //{
            //    Debug.LogError("There is no Target!!!!!!");
            //}


            //need target
            if (m_UseSkillIndex != 0)
            {
                m_ComboDashDist -= m_AttackInfos[m_UseSkillIndex].skillinfo.area_Size * 0.5f;
            }
            dist = m_ComboDashDist;// Vector3.Distance(m_MyObj.position, attackTarget.transform.position);
            speed = dist / pushTime;
            //Debug.Log("StartNormalComboDash() time = " + time + "/ dist = " + m_pushInfo.m_fDistance + "/ pushTime = " + pushTime);

            m_pushInfo.Set(speed, dist, true, m_MyObj.transform.position, Vector3.zero);
        }
        else
        {
            m_ComboDashDist = 0.0f;
            m_pushInfo.Init();
        }
    }


    public void MotionCancel(int p_iAble)
    {
        m_bMotionCancel = p_iAble > 0;
    }
#if MAP_ATTRIBUTE
    private void FootStep()
    {
        if( MapManager.instance.m_AttributeField != null )
        {
            int iAttIndex = MapManager.instance.m_AttributeField.GetIndexFromPos(m_MyObj.transform.position);

            int iAttType = MapManager.instance.m_AttributeField.m_iAttributeTable[MapManager.instance.m_AttributeField.GetAttribute(iAttIndex)];

            switch( iAttType )
            {
                case 0:
                    break;
                case 1:
                case 2:
                case 3:
                case 4:
                case 5:
                case 6:
                case 7:
                    m_damageUI.ShowDamageUI(iAttType, false);
                    break;
            }
        }
    }
#endif

    private void OnAniEventEnd(AnimationClip data)
    {
        switch (m_MotionState)
        {
            case MOTION_STATE.eMOTION_ATTACK_COMBO:
                {
                    BuffData_ReAction kReAction = m_BuffController.FindFrontReActionBuff();
                    if (kReAction != null)
                    {
                        m_BuffController.ReActionPlay(kReAction);
                    }
                    else
                    {
                        if (m_charController.m_AtkComboQueue.Count > 0)
                        {
                            m_charController.m_AtkComboQueue.RemoveAt(0);
                            if (m_charController.m_bAtkComboHold)
                            {
                                //Debug.Log("*** When Hold, is that touch released? Clear Queue() !!!!!!!   " + m_charController.m_CheckPressing + "/" + m_charController.m_bAtkComboHoldAgn);

                                if (!m_charController.m_CheckPressing && !m_charController.m_bAtkComboHoldAgn)
                                {
                                    m_charController.m_bAtkComboHold = false;
                                    m_charController.m_AtkComboQueue.Clear();
                                }
                                else if (!m_charController.m_CheckPressing && m_charController.m_bAtkComboHoldAgn)
                                {
                                    //Debug.Log("***** Hold then Release, and Hold again !!!!! m_charController.m_AtkComboQueue[0] = " + m_charController.m_AtkComboQueue[0]);
                                    m_charController.m_bAtkComboHold = false;
                                    m_charController.m_bAtkComboHoldAgn = false;
                                }
                            }
                        }
                        //after remove combo List
                        if (m_charController.m_AtkComboQueue.Count > 0)
                        {
                            //Attack Combo continue
                            //Debug.Log("Called eMOTION_ATTACK_COMBO 111 / " + m_charController.m_AtkComboQueue.Count);
                            if (m_charController.m_DpadMP.GetDpadNorlized() != Vector2.zero)
                            {
                                m_MyObj.rotation = m_charController.m_DpadMP.GetCharRotationByDpad();
                            }
                            if ((m_CharAi.enabled == true && charUniqID != PlayerManager.MYPLAYER_INDEX) || m_CharAi.GetAIController().GetActionCount() > 0)
                                m_CharAi.SetAniEnd(MOTION_STATE.eMOTION_ATTACK_COMBO);
                            else
                                SetChangeMotionState(MOTION_STATE.eMOTION_ATTACK_COMBO);
                        }
                        else
                        {
                            //if (!m_charController.m_bPressNextAtkInAnim)
                            {
                                //Attack Combo is finished
                                //m_charController.m_AtkComboCnt = 0;
                                m_charController.SetAttackComboTree(eATTACKCOMBO_TREE.NONE);
                                if (m_charController.m_DpadMP.GetDpadNorlized() != Vector2.zero)
                                {
                                    //Debug.Log("@@@@@@@@@@@@@ End of Combo");
                                    if (!m_charController.CheckIsEndAttackID())
                                    {
                                        if ((m_CharAi.enabled == true && charUniqID != PlayerManager.MYPLAYER_INDEX) || m_CharAi.GetAIController().GetActionCount() > 0)
                                            m_CharAi.SetAniEnd(MOTION_STATE.eMOTION_RUN);
                                        else
                                            SetChangeMotionState(MOTION_STATE.eMOTION_RUN);
                                    }
                                }
                                else
                                {
                                    if ((m_CharAi.enabled == true && charUniqID != PlayerManager.MYPLAYER_INDEX) || m_CharAi.GetAIController().GetActionCount() > 0)
                                        m_CharAi.SetAniEnd(MOTION_STATE.eMOTION_IDLE);
                                    else
                                        SetChangeMotionState(MOTION_STATE.eMOTION_IDLE);
                                }
                            }
                        }
                    }
                    //after the first attack, target don't need anymore
                    if (attackTarget != null && (attackTarget.m_CharState == CHAR_STATE.DEATH))
                        attackTarget = null;

                    m_charController.SetComboByTarget();
                }
                break;
            case MOTION_STATE.eMOTION_NPCATTACK:
                {
                    bool bNpcAttackEnd = true;
                    if (data.name.Equals(m_AttackInfos[m_UseSkillIndex].skillinfo.aniResInfo[(byte)animState].animation_name))
                    {
                        if (m_AttackInfos[m_UseSkillIndex].skillinfo.aniResInfo.Count > 1)
                        {
                            switch (animState)
                            {
                                case ANIMATION_STATE.eANIM_START:

                                    SetChangeMotionState(MOTION_STATE.eMOTION_NPCATTACK);
                                    bNpcAttackEnd = false;
                                    break;
                                case ANIMATION_STATE.eANIM_END:
                                    animState = ANIMATION_STATE.eANIM_START;
                                    if (m_CharAi.m_eType == eAIType.eNPC || m_CharAi.m_eType == eAIType.eHERONPC)
                                    {
                                        m_CharAi.SetAniEnd(MOTION_STATE.eMOTION_NPCATTACK);
                                    }
                                    break;
                            }
                        }
                        else
                        {
                            m_CharAi.SetAniEnd(MOTION_STATE.eMOTION_NPCATTACK);
                        }
                    }
                    if (bNpcAttackEnd == true)
                    {
                        BuffData_ReAction kReAction = m_BuffController.FindFrontReActionBuff();
                        if (kReAction != null)
                        {
                            m_BuffController.ReActionPlay(kReAction);
                        }
                    }
                    break;
                }
            case MOTION_STATE.eMOTION_SKILL:
                switch (m_CharacterType)
                {
                    //히어로 NPC 타입 추가로 수정
                    case eCharacter.eHERONPC:
                    case eCharacter.eNPC:   //npc part      

                        //if (m_CharAi.m_eType == eAIType.eNPC)
                        if (m_CharAi.m_eType == eAIType.eNPC || m_CharAi.m_eType == eAIType.eHERONPC)
                        {
                            m_CharAi.SetAniEnd(MOTION_STATE.eMOTION_SKILL);

                            BuffData_ReAction kReAction = m_BuffController.FindFrontReActionBuff();
                            if (kReAction != null)
                            {
                                m_BuffController.ReActionPlay(kReAction);
                            }
                        }
                        break;

                    default:    //Player Part
                        {
                            if (data.name.Equals(m_AttackInfos[m_UseSkillIndex].skillinfo.aniResInfo[0].animation_name))
                            {
                                bool bSkillEnd = false;
                                switch (m_AttackInfos[m_UseSkillIndex].skillinfo.action_Type)
                                {
                                    case eSkillActionType.eSHOOT:
                                        {
                                            switch (m_ChannelingState.m_eState)
                                            {
                                                case CHANNELING_STATE.eSTART:
                                                    m_ChannelingState.m_eState = CHANNELING_STATE.eLOOPREADY;
                                                    break;
                                                case CHANNELING_STATE.eLOOP:
                                                    break;
                                                case CHANNELING_STATE.eEND:
                                                    bSkillEnd = true;
                                                    break;
                                                default:
                                                    bSkillEnd = true;
                                                    break;
                                            }
                                        }
                                        break;
                                    case eSkillActionType.eCHAIN:
                                        //case eSkillActionType.spe
                                        if (m_ChainMove.m_State == CHAIN_STATE.eTHROW)
                                        {
                                            m_ChainMove.m_AnimName = m_AttackInfos[m_UseSkillIndex].skillinfo.aniResInfo[1].animation_name;
                                            m_ChainMove.m_State = CHAIN_STATE.eHOLDING;
                                            SetChangeMotionState(MOTION_STATE.eMOTION_SKILL);
                                        }
                                        else if (m_ChainMove.m_State == CHAIN_STATE.eBACK)
                                        {
                                            //Taylor : Chain remove
                                            //PlayerManager.instance.m_PlayerInfo[charUniqID].swordObj.SetActive(true);
                                            bSkillEnd = true;
                                        }
                                        break;
                                    case eSkillActionType.eMOVEMENT:
                                        if (charUniqID == PlayerManager.MYPLAYER_INDEX) // 나만 함
                                        {
                                            if (PlayerManager.instance.m_PlayerInfo[PlayerManager.MYPLAYER_INDEX].IsAvoidSkill(m_ArollingSkill))
                                            {
                                                m_ArollingSkill = 0;
                                                bSkillEnd = true;
                                            }
                                        }
                                        else
                                        {
                                            if (PlayerManager.instance.m_PlayerInfo[charUniqID].IsAvoidSkill(m_UseSkillIndex))
                                            {
                                                m_ArollingSkill = 0;
                                                bSkillEnd = true;
                                            }
                                        }
                                        break;
                                    default:
                                        int nNextSkillIndex = m_AttackInfos[m_UseSkillIndex].skillinfo.nextSkill_index;
                                        /// next skill 정보가 있으면 플래이 해줌 /// ksk
                                        if (nNextSkillIndex != 0)
                                        {
                                            //Debug.Log("@@@@ CharacterBase.OnAniEventEnd charUniqID= " + charUniqID + "/ m_eType = " + m_CharAi.m_eType + " / online User = " + m_CharAi.m_OnlineUser);
                                            if (charUniqID == PlayerManager.MYPLAYER_INDEX || (m_CharAi.m_eType == eAIType.ePC && m_CharAi.m_OnlineUser == false)) // 나와 AI 유저
                                            {
                                                if (m_charController != null && m_charController.m_DpadMP.GetDpadNorlized() != Vector2.zero)
                                                {
                                                    m_MyObj.rotation = m_charController.m_DpadMP.GetCharRotationByDpad();
                                                }
                                                m_currSkillIndex = m_AttackInfos[m_UseSkillIndex].skillinfo.nextSkill_index;
                                                //Debug.Log("@@@@ CharacterBase.OnAniEventEnd m_currSkillIndex= " + m_currSkillIndex);

                                                SetChangeMotionState(MOTION_STATE.eMOTION_SKILL);

                                                if (charUniqID == PlayerManager.MYPLAYER_INDEX && m_CameraManager != null)
                                                {
                                                    m_CameraManager.fTimer = 0;
                                                }
                                            }
                                        }
                                        else
                                            bSkillEnd = true;

                                        break;
                                }
                                if (bSkillEnd)
                                {
                                    if ((m_CharAi.enabled == true && charUniqID != PlayerManager.MYPLAYER_INDEX) || m_CharAi.GetAIController().GetActionCount() > 0)
                                    {
                                        m_CharAi.SetAniEnd(MOTION_STATE.eMOTION_SKILL);

                                        BuffData_ReAction kReAction = m_BuffController.FindFrontReActionBuff();
                                        if (kReAction != null)
                                        {
                                            m_BuffController.ReActionPlay(kReAction);
                                        }
                                    }
                                    else
                                    {
                                        BuffData_ReAction kReAction = m_BuffController.FindFrontReActionBuff();
                                        if (kReAction != null)
                                        {
                                            m_ReactionBuffData = kReAction;
                                            SetChangeMotionState(MOTION_STATE.eMOTION_BEATTACKED);
                                        }
                                        else
                                            SetChangeMotionState(MOTION_STATE.eMOTION_IDLE);
                                    }
                                }

                            }
                            else if (m_AttackInfos[m_UseSkillIndex].skillinfo.aniResInfo.Count > 1 && data.name.Equals(m_AttackInfos[m_UseSkillIndex].skillinfo.aniResInfo[1].animation_name))
                            {
                                switch (m_AttackInfos[m_UseSkillIndex].skillinfo.action_Type)
                                {
                                    case eSkillActionType.eCHAIN:
                                        //m_ChainMove.m_AnimName = m_AttackInfos[m_UseSkillIndex].skillinfo.aniResInfo[1].animation_name;
                                        break;
                                }
                            }
                        }
                        break;
                }
                break;
            case MOTION_STATE.eMOTION_BEATTACKED:

                if (m_ReactionBuffData != null)
                {
                    CameraAnimatorInit();

                    // 예외처리 투두
                    if ((data.name.Equals("Damaged01") || data.name.Equals("Damaged02"))
                        && SkillDataManager.instance.GetReactionType(m_ReactionBuffData.m_AffectCode) != eREACTION_TYPE.ePHYGICAL_DAMAGE
                        && m_ReactionBuffData.m_AffectCode != eAffectCode.ePULLING_N
                        && m_ReactionBuffData.m_AffectCode != eAffectCode.ePUSHING
                        && m_ReactionBuffData.m_AffectCode != eAffectCode.eSCREW
                        && m_ReactionBuffData.m_AffectCode != eAffectCode.eFROZEN_N)
                    {
                        //Debug.Log("==================== Error BuffData =======================");
                    }
                    else
                    {
                        //Debug.Log("############### MOTION_STATE.eMOTION_BEATTACKED #################");
                        //Debug.Log("AffectCode = " + m_ReactionBuffData.m_AffectCode);
                        //Debug.Log("Reaction Type = " + SkillDataManager.instance.GetReactionType(m_ReactionBuffData.m_AffectCode));
                        switch (SkillDataManager.instance.GetReactionType(m_ReactionBuffData.m_AffectCode))
                        {
                            case eREACTION_TYPE.ePHYGICAL_DAMAGE:
                                m_BuffController.BuffEnd(m_ReactionBuffData);
                                break;
                            case eREACTION_TYPE.eAIRBONE:
                                m_BuffController.BuffEnd(m_ReactionBuffData);
                                // 살아야 일어나지
                                if (m_CharState != CHAR_STATE.DEATH)
                                {
                                    m_BuffController.AddBuff(BuffController.CreateAttackReActionFactory(null, m_BuffController, eAffectCode.eSTANDUP));
                                }
                                else
								{
									//리액션후 죽을 경우 다이모션이 아난오는 문제 수정
									// m_bDieImed = false;
								}
								break;
                            case eREACTION_TYPE.eKNOCKBACK:
                                //Debug.Log("KNOCKBACK Finished @@@@@@@@");
                                m_BuffController.BuffEnd(m_ReactionBuffData);
                                break;
                            case eREACTION_TYPE.eKNOCKBACK_INT_2:
                                m_BuffController.BuffEnd(m_ReactionBuffData);
                                // 살아야 일어나지
                                if (m_CharState != CHAR_STATE.DEATH)
								{
									m_BuffController.AddBuff(BuffController.CreateAttackReActionFactory(null, m_BuffController, eAffectCode.eSTANDUP));
								}      


								break;
                            case eREACTION_TYPE.eKNOCKDOWN:
                                if (m_ReactionBuffData.m_fSpecialTime <= 0.0f)
                                {
                                    m_BuffController.BuffEnd(m_ReactionBuffData);
                                    // 살아야 일어나지
                                    if (m_CharState != CHAR_STATE.DEATH)
                                        m_BuffController.AddBuff(BuffController.CreateAttackReActionFactory(null, m_BuffController, eAffectCode.eSTANDUP));
                                    else
									{
										m_bDieImed = false;
									}                                        
                                }
                                else
                                {
                                    m_bDieImed = false;
                                    return;
                                }
                                break;
                            case eREACTION_TYPE.eSTUNNED:
                                //Debug.Log("STUNNED @@@@@@@@");
                                break;
                            case eREACTION_TYPE.ePULLING:
                                if (m_PullingMove.m_fPullingTime <= 0.0f)
                                {
                                    m_BuffController.BuffEnd(m_ReactionBuffData);
                                    SetCollisionDetection(true);
                                    if (m_ReactionBuffData != null && m_ReactionBuffData.m_SourceChar != null && m_ReactionBuffData.m_SourceChar.m_CharacterType == eCharacter.eWarrior)
                                    {
                                        BuffData kNewKnockBack = BuffController.CreateAttackReActionFactory(m_ReactionBuffData.m_SourceChar, m_BuffController, eAffectCode.eKNOCK_BACK_N, 2);
                                        kNewKnockBack.m_bDontprevent = true;
                                        m_BuffController.AddBuff(kNewKnockBack);
                                    }
                                }
                                else
                                    return;

                                break;
                            case eREACTION_TYPE.ePUSHING:
                                m_BuffController.BuffEnd(m_ReactionBuffData);
                                SetCollisionDetection(true);
                                if (m_ReactionBuffData != null && m_ReactionBuffData.m_SourceChar != null)
                                {
                                    BuffData kNewKnockBack = BuffController.CreateAttackReActionFactory(m_ReactionBuffData.m_SourceChar, m_BuffController, eAffectCode.eKNOCK_BACK_N, 1);
                                    kNewKnockBack.m_bDontprevent = true;
                                    m_BuffController.AddBuff(kNewKnockBack);
                                }
                                break;
                            case eREACTION_TYPE.eSCREW:
                                return;
                            //break;
                            case eREACTION_TYPE.eGUARDBREAK:
                                //Debug.Log("CharacterBase.OnAniEventEnd() GUARDBREAK /m_ReactionBuffData = " + m_ReactionBuffData + "/GuardPress = " + m_bGuardPress);
                                m_GuardState = SHIELDGUARD_STATE.eNONE;
                                m_BuffController.BuffEnd(m_ReactionBuffData);
                                break;
                            case eREACTION_TYPE.eSTANDUP:
                                //ksAnimation.SetDontPlayAnymore(false);
                                m_BuffController.BuffEnd(m_ReactionBuffData);
                                break;
                            case eREACTION_TYPE.eFROZEN:
                                if (m_ReactionBuffData.m_fSpecialTime <= 0.0f)
                                    m_BuffController.BuffEnd(m_ReactionBuffData);
                                break;
                        }
                    }

                    // 리액션후 사망 처리
                    BuffData_ReAction kReAction = m_BuffController.FindFrontReActionBuff();
                    if (kReAction != null)
                    {
                        m_ReactionBuffData = kReAction;

                        switch (m_CharacterType)
                        {
                            case eCharacter.eNPC:   //npc part 
                                if (m_CharAi.GetAIController().ImmediatlyPlay() == false)
                                    SetChangeMotionState(MOTION_STATE.eMOTION_BEATTACKED);
                                break;
                            case eCharacter.eHERONPC:
                                if (m_CharAi.GetAIController().ImmediatlyPlay() == false)
                                    SetChangeMotionState(MOTION_STATE.eMOTION_BEATTACKED);
                                break;
                            default:    //Player Part
                                SetChangeMotionState(MOTION_STATE.eMOTION_BEATTACKED);
                                break;
                        }
                    }
                    else
                    {
                        switch (m_CharacterType)
                        {
                            case eCharacter.eNPC:   //npc part 
                                m_CharAi.SetAniEnd(MOTION_STATE.eMOTION_BEATTACKED);
                                break;
                            default:    //Player Part
                                if ((m_CharAi.enabled == true && charUniqID != PlayerManager.MYPLAYER_INDEX) || m_CharAi.GetAIController().GetActionCount() > 0)
                                    m_CharAi.SetAniEnd(MOTION_STATE.eMOTION_BEATTACKED);
                                else
                                    SetChangeMotionState(MOTION_STATE.eMOTION_IDLE);
                                break;
                        }
                        if (m_CharState == CHAR_STATE.DEATH)
                        {
                            SetDieState(DIE_TYPE.eDIE_STAND);
                            SetChangeMotionState(MOTION_STATE.eMOTION_DIE);
                        }
                        //else
                        //{
                        //    ksAnimation.SetDontPlayAnymore(false);
                        //}
                    }
                }
                break;
            case MOTION_STATE.eMOTION_DIE:
                //ksAnimation.SetDontPlayAnymore(true);
                switch (m_CharacterType)
                {
                    case eCharacter.eNPC:
                        ((NpcAI)m_CharAi).m_DieVar.m_bGowayWait = true;
                        break;
                    case eCharacter.eHERONPC:
                        ((HeroNpcAI)m_CharAi).m_DieVar.m_bGowayWait = true;
                        //((HeroNpcAI)m_CharAi).GetNpcProp().Npc_NameID

                        InGameManager.instance.m_uiNotice.Set_SummonDieNotic(((HeroNpcAI)m_CharAi).GetNpcProp().Npc_NameID);
                        InGameManager.instance.m_uiNotice.SetActive_SummonDie(true);
                        break;
                    default:
                        switch (SceneManager.instance.GetCurGameMode())
                        {
							case SceneManager.eGAMEMODE.eMODE_CHAPTER:
							case SceneManager.eGAMEMODE.eMODE_DAY_OF_THE_WEEK_DUNGEON:
							case SceneManager.eGAMEMODE.eNPC_TEST_TOOL:

							//< modify ggango 2017.11.21
								{ 
									bool a_bAutoResurrection = m_kSummonAbilities.UseConsumableAbility();

									if( a_bAutoResurrection )
									{
										PlayerInfo		a_kPlayerInfo		= PlayerManager.instance.m_PlayerInfo[ PlayerManager.MYPLAYER_INDEX ];
										CharacterBase	a_kCharacterBase	= a_kPlayerInfo.playerCharBase;

										a_kCharacterBase.SetAbillityCharRevival();
										//ConfirmRisen( 0 );
										break;
									}
									else
									{
										//InGameManager.instance.vintage.enabled = true; 
										InGameManager.instance.SetHideActiveMenu();
										ShowRisenPopup();

									}
								}
							//>


                                // ost delete start 20170222
                                //GameObject go = PopupManager.instance.m_PopupTwoButton.transform.FindChild("Sp_Layer").gameObject;

                                //go.SetActive(false);
                                // ost delete end 20170222
                                //================================
                                //================================
                                // gunny 20160411                                
                                //GameObject go2 = ((UI_InGamePause)SceneManager.instance.GetUIComponent(SceneManager.eCOMPONENT.InGamePause)).gameObject;
                                //go2.SetActive(false);
                                //================================

                                //PopupManager.instance.ShowPopupTwoButton("하시겠습니까?\n\n[ff0000]Cash 1[-]이 소모됩니다.", "아니오", "예", delegate { GoToPrevScene(); }, delegate { SetCharRevival(); });
                                if (SceneManager.instance.GetCurGameMode() != SceneManager.eGAMEMODE.eNONSYNC_PVP)
								{
									//StartWiggle();
									StopWiggle();
								}

                                    
									
                                break;

#if DAILYDG
                            case SceneManager.eGAMEMODE.eMODE_DAILY:
                                SetModeCheckDie();

                                break;
#endif
                            case SceneManager.eGAMEMODE.eMODE_RAID:
                                if (charUniqID == PlayerManager.MYPLAYER_INDEX)
                                {
                                    if (SceneManager.instance.GetUIComponent(SceneManager.eCOMPONENT.RaidEnd) == null)
                                    {
                                        //PopupManager.instance.PopupTwoButtonShow("하시겠습니까?\n\n아니오를 누르면 이전 화면으로 이동합니다\n\n[ff0000]Cash 1[-]이 소모됩니다.", "아니오", "예", delegate { GoToPrevScene(); }, delegate { SetCharRevival(); });

                                        SceneManager.instance.SetUIComponent(SceneManager.eCOMPONENT.PopupRaidRebirth);
                                    }
                                }
                                break;
                            case SceneManager.eGAMEMODE.eMODE_PVP:
                                break;
                            case SceneManager.eGAMEMODE.eMODE_FREEFIGHT:
                                // 투두 
                                // 임시 부활
                                if (charUniqID == PlayerManager.MYPLAYER_INDEX)
                                {
                                    UI_RevivalPopup popup = (UI_RevivalPopup)SceneManager.instance.SetUIComponent(SceneManager.eCOMPONENT.RevivalPopup);
                                    popup.PopupShow("해당 위치에서 대기시간없이 즉시 부활하시겠습니까?", CDataManager.m_LocalText["ok"], ImmediatelyRevival);

                                    //SetCharRevival();
                                }
                                break;
                            case SceneManager.eGAMEMODE.eMODE_TRIAL_DUNGEON:
                                TrialDungeonManager.instance.ShowMissionFailResult();
                                //=============================
                                // zunghoon add 20160418
                                m_CameraManager.m_tfCameraMove.position = new Vector3(m_MyObj.transform.position.x + 4.7f, m_MyObj.transform.position.y + 8.1f, m_MyObj.transform.position.z + 8.3f);
                                m_CameraManager.m_tfCameraMove.eulerAngles = new Vector3(m_MyObj.transform.eulerAngles.x + 39, /*m_MyObj.transform.eulerAngles.y - 40*/-133, m_MyObj.transform.eulerAngles.z + 1);

                                NpcManager.instance.AllClearNPCHPbar();
                                //=================================
                                break;
                        }
                        break;
                }

#if NONSYNC_PVP
                if (SceneManager.instance.GetCurGameMode() == SceneManager.eGAMEMODE.eNONSYNC_PVP)
                {
                    if (bPvpLastDie)
                    {
                        // pvp end

                        Time.timeScale = 1;
                        InGameManager.instance.m_bPauseAll = true;

                        bPvpLastDie = false;
                        NetworkManager.instance.networkSender.Send4v4PvpResult();
                    }
                }
#endif
                //SetChangeMotionState(MOTION_STATE.eMOTION_NONE);
                //ksAnimation.Pause(data.clip);
                break;
            case MOTION_STATE.eMOTION_REVIVAL:
                if (data.name.Equals(m_AttackInfos[m_UseSkillIndex].skillinfo.aniResInfo[0].animation_name))
                {
                    m_BuffController.BuffEnd(m_ReactionBuffData);
                    //ksAnimation.SetDontPlayAnymore(false);
                    SetCharacterState(CHAR_STATE.ALIVE);
                    SetChangeMotionState(MOTION_STATE.eMOTION_IDLE);

                    if (m_charController != null)
                    {
                        m_charController.m_AtkComboTree = eATTACKCOMBO_TREE.NONE;
                    }
                    NpcManager.instance.SetSummonAllWork(false);
                    NpcManager.instance.SetNpcActionPlay();
                }
                break;
            case MOTION_STATE.eMOTION_WARP_IN:

                SetChangeMotionState(MOTION_STATE.eMOTION_IDLE);
                if (PlayerManager.instance.playerInfoList[PlayerManager.MYPLAYER_INDEX].playerObj != null)
                {
                    //NONSYNC_PVP
                    //if (SceneManager.instance.GetCurGameMode() != SceneManager.eGAMEMODE.eMODE_PVP)
                    if (
                        SceneManager.instance.GetCurGameMode() == SceneManager.eGAMEMODE.eMODE_CHAPTER
#if DAILYDG
                        || SceneManager.instance.GetCurGameMode() == SceneManager.eGAMEMODE.eMODE_DAILY
#endif
                        )
                    {
                        InGameManager.instance.m_MainCam.m_CamZoomState = CameraManager.eBATTLEZOON_STATE.eNONE;
                        InstanceDungeonManager.Instance.SetDungeonGameStart();
                    }

                    AI ai = PlayerManager.instance.playerInfoList[PlayerManager.MYPLAYER_INDEX].playerObj.GetComponent<AI>();
                    ((AutoAI)ai).SetAutoPlayState(AutoAI.eAutoProcessState.eReady);
                }
                if (InGameManager.instance.m_uiSimulationResult != null)
                {
                    if (InGameManager.instance.m_uiSimulationResult.gameObject.activeSelf)
                    {
                        NpcManager.instance.SetSummonAllStop(false);
                    }
                    else
                    {
                        NpcManager.instance.SetSummonAllWork(false);
                    }
                }
                else
                {
                    NpcManager.instance.SetSummonAllWork(false);
                }
#if ADD_TUTORIAL
				if (PlayerManager.instance.playerInfoList[PlayerManager.MYPLAYER_INDEX].m_MaxRebirthTimes == 0 && PlayerManager.instance.playerInfoList[PlayerManager.MYPLAYER_INDEX].m_CurFloor == 1)
				{
					if (UserManager.instance.iUserCount > 1)
					{
                        if (TutorialManager.instance.m_CompleteTutoria == false)
                        {
                            InGameManager.instance.TutorialPopup();
                        }
					}
					else
					{
						TutorialManager.instance.SetTutorial_Change(PlayerManager.instance.playerInfoList[PlayerManager.MYPLAYER_INDEX].m_CurFloor);
					}
				}
				else
				{
					TutorialManager.instance.SetTutorial_Change(PlayerManager.instance.playerInfoList[PlayerManager.MYPLAYER_INDEX].m_CurFloor);
				}
				
#endif				
				InGameManager.instance.m_WarpInFinish = true;
                break;
            case MOTION_STATE.eMOTION_SPWAN_STANDBY:
                switch (m_CharAi.m_eType)
                {
                    case eAIType.eNPC:
                        ((NpcAI)m_CharAi).SetPeaceState();
                        break;
                    default:
                        break;
                }
                SetChangeMotionState(MOTION_STATE.eMOTION_IDLE);
                break;
            case MOTION_STATE.eMOTION_ETC:
                {
                    if (m_CharAi != null && ((m_CharAi.enabled == true && charUniqID != PlayerManager.MYPLAYER_INDEX) || m_CharAi.GetAIController().GetActionCount() > 0))
                        m_CharAi.SetAniEnd(MOTION_STATE.eMOTION_ETC);
                    else
                    {
                        if (m_CharAi == null)
                        {
                            SetMotionState(MOTION_STATE.eMOTION_IDLE);
                            ksAnimation.PlayAnim("Idle_Stand", 0.15f);
                        }
                        else
                        {
                            SetChangeMotionState(MOTION_STATE.eMOTION_IDLE);
                        }
                    }
                }
                break;
        }

#if NONSYNC_PVP
        if (SceneManager.instance.GetCurGameMode() != SceneManager.eGAMEMODE.eNONSYNC_PVP)
#endif
        {
            if (m_CharAi.m_eType == eAIType.eHERONPC)
            {
                if (((HeroNpcAI)m_CharAi).GetNpcProp().Summon_EquipIndex == -1)
                {
                    if (m_MotionState != MOTION_STATE.eMOTION_DIE)
                    {
                        NpcManager.instance.SetSummonStop(this);
                        //this.m_MyObj.gameObject.SetActive(false);
                    }
                }
            }
        }
    }


    public void SetMoveEndCallBack(EventDelegate.Callback callBack)
    {
        m_moveEndCallBack = callBack;
    }

    public void OnAiEventEnd(ref NpcAction p_Action)
    {
        switch ((NpcAction.eNPpcActionType)p_Action.m_ActionTypeAndInfo.Key)
        {
            case NpcAction.eNPpcActionType.eUSEBASEATTACK:
                break;
            case NpcAction.eNPpcActionType.eMOVE:
                switch (p_Action.m_eNpcActionState)
                {
                    case NpcAction.eNpcActionState.ePLAY:
                        break;
                    case NpcAction.eNpcActionState.eEND:
                        // 도착 혹은 멈춤
                        if (charUniqID == PlayerManager.MYPLAYER_INDEX)
                        {
                            // 플레이어
                            if (m_moveEndCallBack != null)
                                m_moveEndCallBack();

                        }
                        break;
                }
                break;
        }
    }

    public bool CharacterControlFreeze()
    {
        if (m_MotionState == MOTION_STATE.eMOTION_SKILL || m_MotionState == MOTION_STATE.eMOTION_REVIVAL)
        {
            return !m_bMotionCancel;
        }

        return false;
    }

    private void SetDieState(DIE_TYPE p_eDieType)
    {
        switch (m_CharacterType)
        {
            case eCharacter.eNPC:   //npc part
                NpcAI npcAI = (NpcAI)m_CharAi;

                npcAI.SetDieState(p_eDieType);

                if (
                    SceneManager.instance.GetCurGameMode() == SceneManager.eGAMEMODE.eMODE_CHAPTER
#if DAILYDG
                    || SceneManager.instance.GetCurGameMode() == SceneManager.eGAMEMODE.eMODE_DAILY
#endif
                    )
                {

                    float l_fNPCExp = npcAI.m_NpcProp.Npc_ExpValue;

                    l_fNPCExp *= (float)UserManager.instance.GetHotTimeValue(blame_messages.HotTimeType.ADD_EXP_INC);

                    InGameItemManager.instance.GainExp(l_fNPCExp);

                    PlayerManager.instance.playerInfoList[PlayerManager.MYPLAYER_INDEX].m_KillMonsterCount++;
                    MissionManager.instance.CheckMission_NpcType(npcAI.m_NpcProp);

                    InGameManager.instance.m_nCurretnGainExp += (int)npcAI.m_NpcProp.Npc_Exp;

                }
                break;
            case eCharacter.eHERONPC:   //npc part
                HeroNpcAI PlayernpcAI = (HeroNpcAI)m_CharAi;

                PlayernpcAI.SetDieState(p_eDieType);
                break;
            default:
                if (m_CharAi.enabled)
                {
                    ((AutoAI)m_CharAi).SetDieState();
                }
                NpcManager.instance.DiePlayer(gameObject);
                break;
        }
        m_Collider.enabled = false;
        m_pushInfo.Init();
        m_ChainMove.m_State = CHAIN_STATE.eNONE;

        ShieldGuard_Unlock();
    }

    private void SetAliveState()
    {
        m_CharState = CHAR_STATE.ALIVE;
        SetCollisionDetection(true);
    }

    public void BuffDataChangeCallBack(BuffData p_kBuffData)
    {
        if (p_kBuffData != null)
        {
            // 1회성 판별
            switch (p_kBuffData.m_AffectCode)
            {
                case eAffectCode.eDOT_DAMAGE_N:
                case eAffectCode.eDOT_DAMAGE_RATIO:
                case eAffectCode.eHP_RECOVERY_ADD:
                case eAffectCode.eHP_RECOVERY_RATIO:
                //zunghoon 흡혈 추가 20170616
                case eAffectCode.eHP_DRAIN_RATIO:
                    //zunghoon 흡혈 추가 20170616
                    switch (p_kBuffData.m_BuffState)
                    {
                        case BuffData.eBuffState.eSTART:
                            break;
                        case BuffData.eBuffState.eUPDATE:
                            GetHitBuff(p_kBuffData);
                            break;
                        case BuffData.eBuffState.eEND:
                            break;
                    }
                    break;
                case eAffectCode.ePHYSICAL_DAMAGE_ADD:
                case eAffectCode.ePHYSICAL_DAMAGE_RATIO:
                    break;
                case eAffectCode.eATTACK_SPEED_UP_RATIO:
                case eAffectCode.eATTACK_SPEED_DOWN_RATIO:
                case eAffectCode.eMOVE_SPEED_UP_RATIO:
                case eAffectCode.eMOVE_SPEED_DOWN_RATIO:
                    m_CharAi.SetNavMeshAgentSpeed();
                    m_mAnimCtr.bSpeedChange = true; // 애니 다시
                    break;
                case eAffectCode.eTAUNT_N:
                    switch (m_CharacterType)
                    {
                        case eCharacter.eNPC:
                            switch (p_kBuffData.m_BuffState)
                            {
                                case BuffData.eBuffState.eSTART:
                                    m_TauntMove.m_bTaunt = true;
                                    m_TauntMove.m_TauntChar = p_kBuffData.m_SourceChar;
                                    ((NpcAI)m_CharAi).m_ActionController.BreakOtherAction();
                                    ((NpcAI)m_CharAi).DisperseAllAggro();
                                    m_CharAi.GetCharacterBase().SetAttackTarget(m_TauntMove.m_TauntChar);
                                    ((NpcAI)m_CharAi).GatherAggro(m_TauntMove.m_TauntChar.gameObject, m_CharAi.GetNpcProp().First_Agro * 2, true);

                                    goto case BuffData.eBuffState.eUPDATE;
                                case BuffData.eBuffState.eUPDATE:
                                    break;
                                case BuffData.eBuffState.eEND:
                                    m_TauntMove.m_bTaunt = false;
                                    m_TauntMove.m_TauntChar = null;
                                    break;
                                case BuffData.eBuffState.eFAIL:
                                    break;
                            }
                            break;
                        default:
                            switch (p_kBuffData.m_BuffState)
                            {
                                case BuffData.eBuffState.eSTART:
                                    m_TauntMove.m_bTaunt = true;
                                    m_TauntMove.m_TauntChar = p_kBuffData.m_SourceChar;
                                    // 오토
									if(m_CharacterType == eCharacter.eHERONPC)
									{
										m_TauntMove.m_BackupHeroNpcAutoState = ((HeroNpcAI)m_CharAi).GetAutoPlayState();
										(((HeroNpcAI)m_CharAi)).SetAutoPlayState(HeroNpcAI.eAutoProcessState.eReady);
									}
									else if(m_CharacterType == eCharacter.eWarrior || m_CharacterType == eCharacter.eWizard)
									{
										m_TauntMove.m_BackupAutoState = (((AutoAI)m_CharAi)).GetAutoPlayState();
										(((AutoAI)m_CharAi)).SetAutoPlayState(AutoAI.eAutoProcessState.eReady);
									}

                                    goto case BuffData.eBuffState.eUPDATE;
                                case BuffData.eBuffState.eUPDATE:
                                    break;
                                case BuffData.eBuffState.eEND:
                                    m_TauntMove.m_bTaunt = false;
                                    m_TauntMove.m_TauntChar = null;

									if (m_CharacterType == eCharacter.eHERONPC)
									{
										(((HeroNpcAI)m_CharAi)).SetAutoPlayState(m_TauntMove.m_BackupHeroNpcAutoState);
									}
									else if (m_CharacterType == eCharacter.eWarrior || m_CharacterType == eCharacter.eWizard)
									{
										(((AutoAI)m_CharAi)).SetAutoPlayState(m_TauntMove.m_BackupAutoState);
									}
									break;
                                case BuffData.eBuffState.eFAIL:
                                    break;
                            }
                            break;
                    }
                    break;
                case eAffectCode.eDOWN_N: // 누워있는 시간을  m_fSpecialTime 으로해서 controller 에서 end 시킴을 방지
                case eAffectCode.eATTACKED:
                case eAffectCode.eKNOCK_BACK_N:// 미는 시간을  m_fSpecialTime 으로해서 controller 에서 end 시킴을 방지
                case eAffectCode.eKNOCKBACK_INT_2:
                case eAffectCode.ePULLING_N:
                case eAffectCode.eAIRBORNE_N:
                case eAffectCode.ePUSHING:
                case eAffectCode.eSCREW:
                case eAffectCode.eGUARDBREAK:
                case eAffectCode.eFROZEN_N:
                    switch (m_CharacterType)
                    {
                        case eCharacter.eNPC:
#if NONSYNC_PVP
                        case eCharacter.eHERONPC:
#endif
                            switch (p_kBuffData.m_BuffState)
                            {
                                case BuffData.eBuffState.eSTART:
                                    if (CheckMontionStateChange(MOTION_STATE.eMOTION_BEATTACKED, p_kBuffData))
                                    {
                                        // 쿨없으면 제거함
                                        if (m_ReactionBuffData != null && m_ReactionBuffData != p_kBuffData && m_ReactionBuffData.m_fLifeTime == 0.0f)
                                            m_BuffController.BuffEnd(m_ReactionBuffData);
                                        m_ReactionBuffData = (BuffData_ReAction)p_kBuffData;

                                        if (p_kBuffData.m_fLifeTime > 0.0f)
                                        {
                                            p_kBuffData.m_fSpecialTime = p_kBuffData.m_fLifeTime;
                                            p_kBuffData.m_fLifeTime = 0.0f;
                                        }
                                    }
                                    else if (((BuffData_ReAction)p_kBuffData).m_iReActionType == BuffData_ReAction.eReActionType.REACTION ||
                                            (((BuffData_ReAction)p_kBuffData).m_iReActionType == BuffData_ReAction.eReActionType.CROWDCONTROL &&
                                            ((BuffData_ReAction)p_kBuffData).m_fLifeTime == 0.0f))
                                    {
                                        m_BuffController.BuffEnd(p_kBuffData);
                                    }
                                    break;
                                case BuffData.eBuffState.eEND:
                                    break;
                                case BuffData.eBuffState.eFAIL:
                                    break;
                            }
                            break;
                        default:
                            switch (p_kBuffData.m_BuffState)
                            {
                                case BuffData.eBuffState.eSTART:
                                    if (CheckMontionStateChange(MOTION_STATE.eMOTION_BEATTACKED, p_kBuffData))
                                    {
                                        // 쿨없으면 제거함
                                        if (m_ReactionBuffData != null && m_ReactionBuffData != p_kBuffData && m_ReactionBuffData.m_fLifeTime == 0.0f)
                                            m_BuffController.BuffEnd(m_ReactionBuffData);

                                        m_ReactionBuffData = (BuffData_ReAction)p_kBuffData;

                                        if (m_charController != null)
                                            m_charController.SetAttackCombo_End();

                                        if (p_kBuffData.m_fLifeTime > 0.0f)
                                        {
                                            p_kBuffData.m_fSpecialTime = p_kBuffData.m_fLifeTime;
                                            p_kBuffData.m_fLifeTime = 0.0f;
                                        }
                                        if (m_CharacterType != eCharacter.eHERONPC)
                                        {
                                            if (((AutoAI)m_CharAi).CheckMontionStateChange() == false && charUniqID == PlayerManager.MYPLAYER_INDEX)
                                                SetChangeMotionState(MOTION_STATE.eMOTION_BEATTACKED);
                                        }

                                    }
                                    else if (((BuffData_ReAction)p_kBuffData).m_iReActionType == BuffData_ReAction.eReActionType.REACTION ||
                                            (((BuffData_ReAction)p_kBuffData).m_iReActionType == BuffData_ReAction.eReActionType.CROWDCONTROL &&
                                            ((BuffData_ReAction)p_kBuffData).m_fLifeTime == 0.0f))
                                    {
                                        m_BuffController.BuffEnd(p_kBuffData);
                                    }
                                    break;
                                case BuffData.eBuffState.eEND:
                                    break;
                                case BuffData.eBuffState.eFAIL:
                                    break;
                            }
                            //투두
                            break;
                    }
                    break;
                case eAffectCode.eSTUNNED_N:
                case eAffectCode.ePANIC_N:
                    switch (m_CharacterType)
                    {
                        case eCharacter.eNPC:
                            switch (p_kBuffData.m_BuffState)
                            {
                                case BuffData.eBuffState.eSTART:
                                case BuffData.eBuffState.eUPDATE:
                                    if (CheckMontionStateChange(MOTION_STATE.eMOTION_BEATTACKED, p_kBuffData))
                                    {
                                        if (m_ReactionBuffData != null && m_ReactionBuffData != p_kBuffData && m_ReactionBuffData.m_fLifeTime == 0.0f)
                                            m_BuffController.BuffEnd(m_ReactionBuffData);
                                        m_ReactionBuffData = (BuffData_ReAction)p_kBuffData;
                                    }
                                    break;
                                case BuffData.eBuffState.eEND:
                                    break;
                                case BuffData.eBuffState.eFAIL:
                                    break;
                            }
                            break;
                        default:
                            switch (p_kBuffData.m_BuffState)
                            {
                                case BuffData.eBuffState.eSTART:
                                case BuffData.eBuffState.eUPDATE:
                                    if (CheckMontionStateChange(MOTION_STATE.eMOTION_BEATTACKED, p_kBuffData))
                                    {
                                        if (m_ReactionBuffData != null && m_ReactionBuffData != p_kBuffData && m_ReactionBuffData.m_fLifeTime == 0.0f)
                                            m_BuffController.BuffEnd(m_ReactionBuffData);

                                        m_ReactionBuffData = (BuffData_ReAction)p_kBuffData;

                                        if (m_CharacterType != eCharacter.eHERONPC)
                                        {
                                            if (((AutoAI)m_CharAi).CheckMontionStateChange() == false && charUniqID == PlayerManager.MYPLAYER_INDEX)
                                                SetChangeMotionState(MOTION_STATE.eMOTION_BEATTACKED);

                                            if (m_charController != null)
                                                m_charController.SetAttackCombo_End();
                                        }
                                    }
                                    break;
                                case BuffData.eBuffState.eEND:
                                    break;
                                case BuffData.eBuffState.eFAIL:
                                    break;
                            }
                            break;
                    }
                    break;
                case eAffectCode.eSUMMON_NPC_N:
                    {
                        bool bBlock = true;
                        int iMaxCheck = 3;
                        Vector3 kSummonVec = Vector3.zero;
                        // 주변 검색 소환 가능 위치 찾기
                        do
                        {
                            float fSummonRange = Random.Range(GetRadius(), p_kBuffData.m_fBuffArea);
                            Vector3 kSummonDir = new Vector3(0.0f, 0.0f, 1.0f);
                            kSummonDir = Quaternion.Euler(0, UtilManager.instance.Random(0, 360), 0) * kSummonDir;

                            kSummonVec = m_MyObj.position + (kSummonDir * fSummonRange);
                            Collider[] kObstacles = null;
                            CheckObjectRay(m_MyObj.transform.position, kSummonDir, fSummonRange, out kObstacles);

                            if (kObstacles == null)
                            {
                                bBlock = false;
                            }
                            else
                            {
                                for (int i = 0; i < kObstacles.Length; ++i)
                                {
                                    if (kObstacles[i] != null && kObstacles[i].CompareTag("Col"))
                                    {
                                        bBlock = true;
                                        break;
                                    }
                                    else
                                        bBlock = false;
                                }
                            }
                            --iMaxCheck;
                            if (iMaxCheck < 0)
                            {
                                bBlock = false;
                                kSummonVec = m_MyObj.position;
                            }
                        }
                        while (bBlock);
                        CharacterBase kSummonObject = NpcManager.instance.GetSummonCharObject(kSummonVec);
                        if (kSummonObject != null)
                        {
                            switch (m_CharacterType)
                            {
                                case eCharacter.eNPC:
                                    kSummonObject.m_MyObj.transform.position = kSummonVec;
                                    // 같은 타겟을 대상으로 공격
                                    //List<CharacterBase> kTarget = m_CharAi.FindTarget(eTargetState.eHOSTILE, null, 1);
                                    List<CharacterBase> kTarget = m_CharAi.FindTarget(eTargetState.eFRIENDLY, null, 1);
                                    if (kTarget.Count > 0)
                                    {
                                        kSummonObject.SetAttackTarget(kTarget[0]);
                                    }
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                    break;

                case eAffectCode.eRESURRECTION_N:

                    switch (p_kBuffData.m_BuffState)
                    {
                        case BuffData.eBuffState.eSTART:
                            m_ReactionBuffData = (BuffData_ReAction)p_kBuffData;
                            switch (m_CharacterType)
                            {
                                case eCharacter.eNPC:
                                    if (p_kBuffData.m_SourceChar.m_CharacterType != eCharacter.eNPC)
                                    {
                                        m_BuffController.BuffEnd(p_kBuffData);
                                    }
                                    else
                                    {
                                        m_ReactionBuffData = (BuffData_ReAction)p_kBuffData;
                                    }
                                    break;
                                default:
                                    if (p_kBuffData.m_SourceChar.m_CharacterType == eCharacter.eNPC)
                                    {
                                        m_BuffController.BuffEnd(p_kBuffData);
                                    }
                                    else
                                    {
                                        m_ReactionBuffData = (BuffData_ReAction)p_kBuffData;
                                    }
                                    break;
                            }

                            SetAliveState();

                            break;
                        case BuffData.eBuffState.eEND:
                            break;
                        case BuffData.eBuffState.eFAIL:
                            break;
                    }

                    break;
                case eAffectCode.eSTANDUP:
                    break;
                default:
                    switch (m_CharacterType)
                    {
                        case eCharacter.eWarrior:
                        case eCharacter.eArcher:
                        case eCharacter.eWizard:
                            {
                                PlayerInfo pInfo = PlayerManager.instance.m_PlayerInfo[charUniqID];
                            }
                            break;
                    }
                    break;
            }
        }
    }
    public void SwapShader(Shader p_Shader)
    {
        for (int i = 0; i < m_preFabRenderer.Count; ++i)
        {
            mOldMaterials.Add(m_preFabRenderer[i], m_preFabRenderer[i].sharedMaterial);
            m_preFabRenderer[i].material.shader = p_Shader;
        }
    }
    public void RecoveryShader()
    {
        for (int i = 0; i < m_preFabRenderer.Count; ++i)
        {
            m_preFabRenderer[i].material = mOldMaterials[m_preFabRenderer[i]];
        }
        mOldMaterials.Clear();
    }

    public void GuideEffectRender(SkillDataInfo.SkillInfo p_Skill_Info, Vector3 p_Pos, Vector3 p_MovePos, float p_fLifeTime)
    {
        Vector3 kEffectPos = p_Pos;
        Vector3 kEffectScale = Vector3.one;
        float kEffectAngle = 0.0f;

        switch (p_Skill_Info.action_Type)
        {
            case eSkillActionType.eNORMAL:
                switch (p_Skill_Info.target_Type)
                {
                    case eTargetType.eNONE:
                        break;
                    case eTargetType.eTARGET:
                        break;
                    case eTargetType.eTARGET_AREA:
                        break;
                    case eTargetType.eSELF:
                        break;
                    case eTargetType.eSELF_AREA:
                        break;
                    case eTargetType.eAREA:
                        break;
                }
                //kEffectScale = new Vector3(p_Skill_Info.area_Size+ GetRadius(), p_Skill_Info.area_Size+ GetRadius(), p_Skill_Info.area_Size+ GetRadius()) * 0.2f;
                kEffectAngle = Vector2.Angle(new Vector2(0, 1), new Vector2(m_MyObj.transform.forward.x, m_MyObj.transform.forward.z));

                kEffectPos = p_Pos + m_MyObj.transform.forward * p_Skill_Info.dist_Range;
                kEffectPos += p_MovePos;
                break;
            case eSkillActionType.eSHOOT:
                break;
            case eSkillActionType.eMOVEMENT:
                break;
            case eSkillActionType.eRUSH:
            case eSkillActionType.eTYPE_PENETRATION:
                //kEffectScale = new Vector3(p_Skill_Info.area_Size, p_Skill_Info.area_Size, p_Skill_Info.skill_Dist) * 0.2f;
                kEffectAngle = Vector2.Angle(new Vector2(0, 1), new Vector2(m_MyObj.transform.forward.x, m_MyObj.transform.forward.z));
                //kEffectScale.z *= 0.5f;
                kEffectPos = p_Pos + m_MyObj.transform.forward * p_Skill_Info.skill_Dist * 0.5f;
                break;
            case eSkillActionType.eAERO:
                //kEffectScale = new Vector3(p_Skill_Info.area_Size + GetRadius(), p_Skill_Info.area_Size + GetRadius(), p_Skill_Info.area_Size + GetRadius()) * 0.2f;
                kEffectAngle = Vector2.Angle(new Vector2(0, 1), new Vector2(m_MyObj.transform.forward.x, m_MyObj.transform.forward.z));
                kEffectPos = m_AttackPosition;
                break;
        }
        kEffectAngle = (m_MyObj.transform.forward.x < 0.0f) ? 360.0f - kEffectAngle : kEffectAngle;

        kEffectPos.y += 0.02f;

        if (p_Skill_Info.Guide_Effect != 0)
        {
            m_EffectSkillManager.FX_PlayGuide(this, (int)p_Skill_Info.effGuideInfo.unEffectID, kEffectPos, kEffectScale, kEffectAngle, p_fLifeTime, true);
        }
    }

    public List<uint> GetSkillEffectIDList()
    {
        List<uint> nSkillEffectIDList = new List<uint>();
        if (m_currSkillIndex > 0)
        {
            int nCount = m_AttackInfos[m_currSkillIndex].skillinfo.effBodyInfo.Count;
            List<SkillDataInfo.EffectResInfo> effectResInfoList = m_AttackInfos[m_currSkillIndex].skillinfo.effBodyInfo;
            for (int i = 0; i < nCount; ++i)
            {
                nSkillEffectIDList.Add(effectResInfoList[i].unEffectID);
            }
        }
        return nSkillEffectIDList;
    }

    public List<SkillDataInfo.EffectResInfo> GetSkillEffectBodyResInfo()
    {
        return m_AttackInfos[m_currSkillIndex].skillinfo.effBodyInfo;
    }

    public int GetProjectileEffectIDList(int p_iHitIndex)
    {
        if (m_currSkillIndex > 0)
        {
            return m_AttackInfos[m_currSkillIndex].skillinfo.projectTileInfo[p_iHitIndex].id;
        }
        return 0;
    }

    public int nSkillEffectSequenceIndex
    {
        get { return m_nSkillEffectSequenceIndex; }
        set { m_nSkillEffectSequenceIndex = value; }
    }

    public int nSkillProjectileSequenceIndex
    {
        get { return m_nSkillProjectileSequenceIndex; }
        set { m_nSkillProjectileSequenceIndex = value; }
    }

    public void SetCollisionDetection(bool p_bEnable)
    {
        //        m_Collider.enabled = p_bEnable;

        switch (m_CharacterType)
        {
            case eCharacter.eWarrior:
            case eCharacter.eArcher:
            case eCharacter.eWizard:
                ((AutoAI)m_CharAi).SetCollisionDetection(p_bEnable);
                break;
            case eCharacter.eHERONPC:
                ((HeroNpcAI)m_CharAi).SetCollisionDetection(p_bEnable);
                break;
            case eCharacter.eNPC:
            default:
                {
                    ((NpcAI)m_CharAi).SetCollisionDetection(p_bEnable);
                    break;
                }
        }

    }

    //콤보에 따른 추가 버프 함수 추가
    public void ComboOption_UpData()
    {
        if (m_fComboDuration_time > 0)
        {
            m_fComboDuration_time -= Time.deltaTime;
            if (m_fComboDuration_time <= 0)
            {
                m_fComboDuration_time = 0;
                ComboOption_Remove();
                m_ComboCount = 0;
            }
        }
    }
    public void ComboOption_Set(PlayerStanceInfo StanceInfo)
    {
        if (InGameManager.instance.m_uiCombo != null)
        {
            InGameManager.instance.m_uiCombo.gameObject.SetActive(true);
            m_fComboDuration_time = InGameManager.instance.m_uiCombo.m_comboTime;
        }

        m_ComboCount++;

        for (int i = 0; i < StanceInfo.system_ComboAffect_Code.Count; i++)
        {
            switch (StanceInfo.system_ComboAffect_Code[i])
            {
                case eAffectCode.eATTACK_SPEED_UP_RATIO:
                    m_DamageCalculationData.ComboOption_Set(PlayerManager.instance.GetPlayerInfo(charUniqID), m_BuffController, m_ComboCount, (int)eBATTLE_PERFOMANCE.eATTACK_SPEED);
                    break;
                case eAffectCode.eMOVE_SPEED_UP_RATIO:
                    m_DamageCalculationData.ComboOption_Set(PlayerManager.instance.GetPlayerInfo(charUniqID), m_BuffController, m_ComboCount, (int)eBATTLE_PERFOMANCE.eMOVE_SPEED);
                    break;
            }
        }
    }
    public void ComboOption_Remove()
    {
        int iStance = (int)UserManager.instance.m_CurPlayingCharInfo.playerInfo.m_eStanceType - 1;
        PlayerStanceInfo StanceInfo = PlayerManager.instance.GetPlayerStanceInfo(PlayerManager.instance.playerInfoList[PlayerManager.MYPLAYER_INDEX].playerCharBase, iStance);

        m_ComboCount = 0;
        for (int i = 0; i < StanceInfo.system_ComboAffect_Code.Count; i++)
        {
            switch (StanceInfo.system_ComboAffect_Code[i])
            {
                case eAffectCode.eATTACK_SPEED_UP_RATIO:
                    m_DamageCalculationData.fCOMBOATTACK_SPEED = 0;
                    break;
                case eAffectCode.eMOVE_SPEED_UP_RATIO:
                    m_DamageCalculationData.fCOMBOMOVE_SPEED = 0;
                    break;
            }
        }
    }

    public void ComboStop()
    {
        m_fComboDuration_time = 0;
        m_ComboCount = 0;
    }

    public void SetCharacterHide(eSTATE_HIDE param)
    {
        if (m_CharacterType == eCharacter.eNPC || m_CharacterType == eCharacter.eHERONPC)
            return;

        PlayerInfo pInfo = PlayerManager.instance.m_PlayerInfo[charUniqID];

        switch (param)
        {
            case eSTATE_HIDE.eSTATE_HIDENOT:
                if (m_eHideState == eSTATE_HIDE.eSTATE_HIDE)
                {
                    pInfo.equipedItems[ItemData.eITEM_SUB_KIND.HELMET].SetActive(true);
                    pInfo.equipedItems[ItemData.eITEM_SUB_KIND.CHEST].SetActive(true);
                    pInfo.equipedItems[ItemData.eITEM_SUB_KIND.PANTS].SetActive(true);
                    pInfo.equipedItems[ItemData.eITEM_SUB_KIND.GLOVES].SetActive(true);
                    pInfo.equipedItems[ItemData.eITEM_SUB_KIND.SHOES].SetActive(true);
                    //for (int i = 0; i < pInfo.m_Weapons.Length; i++)
                    //{
                    //    pInfo.m_Weapons[i].SetActive(true);
                    //}
                    for (int i = 0; i < pInfo.m_MWeapons.Length; i++)
                    {
                        if (pInfo.m_MWeapons[i] != null)
                        {
                            pInfo.m_MWeapons[i].SetActive(true);
                        }
                    }
                    //Shadow
                    //pInfo.shadowObj.SetActive(true);
                    if (pInfo.naviObj != null)
                    {
                        pInfo.naviObj.SetActive(true);
                    }
                }
                break;
            case eSTATE_HIDE.eSTATE_HIDE:
                pInfo.equipedItems[ItemData.eITEM_SUB_KIND.HELMET].SetActive(false);
                pInfo.equipedItems[ItemData.eITEM_SUB_KIND.CHEST].SetActive(false);
                pInfo.equipedItems[ItemData.eITEM_SUB_KIND.PANTS].SetActive(false);
                pInfo.equipedItems[ItemData.eITEM_SUB_KIND.GLOVES].SetActive(false);
                pInfo.equipedItems[ItemData.eITEM_SUB_KIND.SHOES].SetActive(false);

                //< modify ggango 2017.07.31
                //for (int i = 0; i < pInfo.m_Weapons.Length; i++)
                //{
                //	if( pInfo.m_Weapons[i] != null )
                //	{ 
                //		pInfo.m_Weapons[i].SetActive(false);
                //	}
                //}

                for (int i = 0; i < pInfo.m_MWeapons.Length; i++)
                {
                    if (pInfo.m_MWeapons[i] != null)
                    {
                        pInfo.m_MWeapons[i].SetActive(false);
                    }
                }
                //>

                //Shadow
                //pInfo.shadowObj.SetActive(false);
                //Bottom Effect
                if (pInfo.naviObj != null)
                {
                    pInfo.naviObj.SetActive(false);
                }

                break;
        }

        m_eHideState = param;

    }


    public void ShieldGuard_Unlock()
    {
        m_bGuardPress = false;
        m_GuardState = SHIELDGUARD_STATE.eNONE;
        StopMagicianGuardEff();
    }

    public void StopMagicianGuardEff()
    {
        if (m_CharacterType == eCharacter.eWizard)
        {
            AutoAI autoAI = (AutoAI)m_CharAi;
            int pStance = (int)autoAI.GetStance();

            if (PlayerManager.instance.GetSpecialSkill(this, pStance, 0) == m_currSkillIndex)
            {
                for (int i = 0; i < m_AttackInfos[m_currSkillIndex].skillinfo.skill_Effect.Count; ++i)
                {
                    m_EffectSkillManager.StopPlayingEffect(m_AttackInfos[m_currSkillIndex].skillinfo.skill_Effect[i]);
                }
            }
        }
    }

    public void SetEventTriggerData(EventTriggerData data)
    {
        m_evtTriggerData = data;

        m_MoveAreaDestPos = GameObject.Find(data.ActionValue).transform.position;

        if (m_MoveAreaDestPos != Vector3.zero)
        {
            m_bTriggerMoveArea = true;
            LookAtY(m_MoveAreaDestPos);
            ((NpcAI)m_CharAi).SetNavMeshAgent(true, m_MoveAreaDestPos);
            SetChangeMotionState(MOTION_STATE.eMOTION_WALK);
        }
    }

    public static bool IsEnemy(CharacterBase p_Char1, CharacterBase p_Char2)
    {
        if (p_Char1 == null || p_Char2 == null || p_Char1 == p_Char2)
            return false;

        if (p_Char1.m_CharacterType == eCharacter.eNPC)
        {
            if (p_Char2.m_CharacterType == eCharacter.eNPC)
            {
                if (p_Char1.m_CharAi.GetNpcProp().Identity_Fnd != p_Char2.m_CharAi.GetNpcProp().Identity_Fnd)
                    return true;
            }
            else
            {
                if (p_Char1.m_CharAi.GetNpcProp().Identity_Fnd == eTargetState.eHOSTILE)
                    return true;
            }
        }
        else
        {
			if (p_Char2.m_CharacterType == eCharacter.eNPC)
			{
				if (p_Char2.m_CharAi.GetNpcProp().Identity_Fnd == eTargetState.eHOSTILE)
					return true;
			}
			else if (p_Char2.m_CharacterType == eCharacter.eHERONPC)
			{
				if (p_Char1.Team != p_Char2.Team)
				{
					return true;
				}
			}
			else if (p_Char2.m_CharacterType == eCharacter.eWarrior || p_Char2.m_CharacterType == eCharacter.eWizard)
			{
				if (p_Char1.Team != p_Char2.Team)
				{
					return true;
				}
			}
		}
		return false;
    }


    #region PROPERTY
    public KSAnimation ksAnimation
    {
        get { return m_KSAnimation; }
    }

    public CharacterBase attackTarget
    {
        set { m_AttackTarget = value; }
        get { return m_AttackTarget; }
    }
    public Vector3 attackPosition
    {
        set { m_AttackPosition = value; }
        get { return m_AttackPosition; }
    }
    //public int myPlayerIdx
    //{
    //    set { m_myPlayerIdx = value; }
    //    get { return m_myPlayerIdx; }
    //}

    public bool isCritical
    {
        set { m_IsCritical = value; }
        get { return m_IsCritical; }
    }

    public int charUniqID
    {
        set { m_CharUniqID = value; }
        get { return m_CharUniqID; }
    }

	public int Team
	{
		set { m_Team = value; }
		get { return m_Team; }
	}

	public CHAR_STATE chrState
    {
        set { m_CharState = value; }
        get { return m_CharState; }
    }

    public ANIMATION_STATE animState
    {
        set { m_mAnimCtr.m_AnimState = value; }
        get { return m_mAnimCtr.m_AnimState; }
    }
    public DamageCalculationData damageCalculationData
    {
        get { return m_DamageCalculationData; }
        set { m_DamageCalculationData = value; }
    }

    #endregion PROPERTY

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        if (m_CharUniqID == PlayerManager.MYPLAYER_INDEX)
            Gizmos.color = Color.green;

        if (m_currSkillIndex > 0 && m_AttackInfos != null && m_AttackInfos.ContainsKey(m_currSkillIndex) == true)
        {
            Vector3 pos = m_MyObj.transform.position;
            float fRadius = GetRadius();
            if (m_AttackInfos[m_currSkillIndex].skillinfo.target_Type == eTargetType.eAREA)
            {
                pos = attackPosition;
                fRadius = 0.0f;
            }

            float fAnble = m_AttackInfos[m_currSkillIndex].skillinfo.area_Angle * 0.5f;
            Vector3 vRayDir = Quaternion.AngleAxis(-fAnble, new Vector3(0, 1, 0)) * m_MyObj.transform.forward;
            Vector3 vLeft = pos + vRayDir * (m_AttackInfos[m_currSkillIndex].skillinfo.area_Size + fRadius);
            Gizmos.DrawLine(pos, vLeft);

            vRayDir = Quaternion.AngleAxis(fAnble, new Vector3(0, 1, 0)) * m_MyObj.transform.forward;
            Vector3 vRight = pos + vRayDir * (m_AttackInfos[m_currSkillIndex].skillinfo.area_Size + fRadius);
            Gizmos.DrawLine(pos, vRight);


            //drraw circle
            int iCircleCnt = (int)(m_AttackInfos[m_currSkillIndex].skillinfo.area_Angle) / 5;

            if (iCircleCnt >= 2)
            {
                vRayDir = (vLeft - pos).normalized;
                for (int i = 0; i < iCircleCnt - 1; ++i)
                {
                    vRayDir = Quaternion.AngleAxis(5, new Vector3(0, 1, 0)) * vRayDir;
                    Vector3 vMid = pos + vRayDir * (m_AttackInfos[m_currSkillIndex].skillinfo.area_Size + fRadius);
                    Gizmos.DrawLine(vLeft, vMid);
                    vLeft = vMid;
                }
            }
            Gizmos.DrawLine(vLeft, vRight);
        }
    }
#endif


}
