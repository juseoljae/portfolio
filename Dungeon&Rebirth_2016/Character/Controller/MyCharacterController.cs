using UnityEngine;
using System;
using System.Collections.Generic;
public enum eAttackType : int
{
    eAccackNone = -1,
    eBasicSkill,
    eSkill1 = 11,
    eSkill2,
    eSkill3,
    eSkill4,
    eComboSkill,
    eEventSkill,
}

public enum eSKILL_TYPE : byte
{
    eSKILL_0 = 0,
    eSKILL_1,
    eSKILL_2,
    eSKILL_3,
    eSKILL_4,
    eSKILL_5,
    //eSKILL_CHAIN,
    eSKILL_SPECIAL,
    eSKILL_AVOIDROLLING
}

public enum eATTACKCOMBO_TREE : byte
{
    START = 0,
    DASH,
    MAX_DASH,
    NORMAL,
    GOOD,
    GREAT,
    PERFECT,
    NONE
}

public enum eAttackCombo_Key : int
{
    eAttackCombo_Normal = 0,
    eAttackCombo_Good,
    //eAttackCombo_Great,
    eAttackCombo_Perfect,
    //eAttackCombo_Max,
}

public enum eSTATE_HIDE : int
{
    eSTATE_HIDE = 0,
    eSTATE_HIDENOT,
    eSTATE_NONE
}

public class MyCharacterController : MonoBehaviour
{
    #region CONST
    private const int AUTOTARGET_ANGLE = 45;
    private const float PRESSING_TIME_DELAY = 0.2f;    
    #endregion CONST

    #region MEMBER_VARIABLE
    //private CharObject m_charScr;
    private CharacterBase m_charScr;
    public PlayerDataInfo m_playerDataInfo;
    private GameObject m_charObj;

    public bool m_CheckPressing;
    private bool m_PressingAttack;
    public float m_PressingTime;

    public bool m_bAttacking;
    public bool m_bAbleToPressSkill;

    //Attack combo
    public Dictionary<int, SkillComboSet> m_AttackComboSet;
    public List<int> m_AtkComboQueue;
    public int m_AtkComboCnt; //When animation is Started, it count once
    public DPadMovePlayer m_DpadMP;
    public int m_prevAtkIndex;
    public eATTACKCOMBO_TREE m_AtkComboTree;
    private eAttackCombo_Key m_AtkComboState;
    public eAttackCombo_Key m_CurAtkComboState;
    
    private CameraManager m_CameraManager;

    public bool m_bAtkComboHold;
    public bool m_bAtkComboHoldAgn;

    #endregion MEMBER_VARIABLE

    #region MONO_FUNCTION
    // Use this for initialization
    void Start()
    {        
        PlayerInfo pInfo = PlayerManager.instance.m_PlayerInfo[PlayerManager.MYPLAYER_INDEX];
        m_charScr = gameObject.GetComponent<CharacterBase>();
        m_charObj = gameObject;
        m_AtkComboQueue = new List<int>();

        m_AttackComboSet = new Dictionary<int, SkillComboSet>();

        m_CameraManager = Camera.main.GetComponent<CameraManager>();

        

        m_DpadMP = gameObject.GetComponent<DPadMovePlayer>();

        //Temporary
        m_playerDataInfo = PlayerManager.instance.m_PlayerDataInfo[(int)m_charScr.m_CharAi.GetPlayProp().jobType];//PlayerManager.instance.m_PlayerInfo[m_charScr.charUniqID].playerDataInfo;//Taylor it will be change

        initialize();

        Init_Character_ComboSet_DataLoad();
        InitAttack_Combo_Set();


        if (  SceneManager.instance.GetCurMapCreateType() == SceneManager.eMAP_CREATE_TYPE.IN_GAME)
        {
            //Skill Slot Lengthes
            for (int i = 0; i < pInfo.curSkillSlots.Length; i++ )
            {
                if (pInfo.curSkillSlots[i] != 0)
                {
                    float fAniTime = m_charScr.ksAnimation.GetAnimationClip(m_charScr.m_AttackInfos[(int)pInfo.curSkillSlots[i]].skillinfo.aniResInfo[0].animation_name).length;
                    m_charScr.m_AttackInfos[(int)pInfo.curSkillSlots[i]].skillinfo.aniTime = fAniTime;

                    if (m_charScr.m_AttackInfos[(int)pInfo.curSkillSlots[i]].skillinfo.nextSkill_index != 0)
                    {
                        fAniTime = m_charScr.ksAnimation.GetAnimationClip(m_charScr.m_AttackInfos[(int)m_charScr.m_AttackInfos[(int)pInfo.curSkillSlots[i]].skillinfo.nextSkill_index].skillinfo.aniResInfo[0].animation_name).length;
                        m_charScr.m_AttackInfos[(int)pInfo.curSkillSlots[i]].skillinfo.aniTime += fAniTime;
                    }
                }
            }

        }

	}
	
	// Update is called once per frame
    void Update()
    {
        //When CharacterType is Worrior
        if (m_charScr.m_bGuardPress)
        {
            //Debug.Log("Character Controller m_bGuardAvail = " + m_charScr.m_bGuardAvail);
            if(m_charScr.m_bGuardAvail)
            {
                //Debug.Log("GuardPress = " + m_charScr.m_bGuardPress + "/ eventEnd = " + m_charScr.ksAnimation.bEventEnd + "/m_GuardState = " + m_charScr.m_GuardState);
                if (m_charScr.ksAnimation.bEventEnd || (m_charScr.m_GuardState == SHIELDGUARD_STATE.eNONE && !m_charScr.ksAnimation.bEventEnd))
                {
                    BuffData_ReAction kReAction = m_charScr.m_BuffController.FindFrontReActionBuff();
                    if (kReAction != null && kReAction.m_iReActionType == BuffData_ReAction.eReActionType.REACTION )
                    {
                    }
                    else
                    {
                        m_charScr.m_currSkillIndex = PlayerManager.instance.GetCurSpecialSkill(m_charScr);

                        if (!m_charScr.CheckMontionStateChange(MOTION_STATE.eMOTION_SKILL, null))
                        {
                            m_charScr.m_currSkillIndex = 0;
                            m_charScr.m_ChainMove.m_AnimName = "";
                            return ;
                        }

                        //When Skill Button is pressed, reservated action is canceled.
                        if (m_AtkComboQueue.Count > 0)
                        {
                            m_AtkComboQueue.Clear();
                            m_AtkComboTree = eATTACKCOMBO_TREE.NONE;
                        }

                        if (((AutoAI)m_charScr.m_CharAi).GetAutoPlayState() != AutoAI.eAutoProcessState.ePlay)
                            m_charScr.SetChangeMotionState(MOTION_STATE.eMOTION_SKILL);
                    }
                }
            }
        }

        if (!m_PressingAttack)
        {
            if (m_bAtkComboHold == true)
                m_bAtkComboHold = false;
            return;
        }
        if (!m_CheckPressing) return;
        else //if Attack Button is keep pressed
        {
            if (Time.time - m_PressingTime > PRESSING_TIME_DELAY)
            {
                //Debug.Log("Pressing Time = " + Time.time);
                m_bAtkComboHold = true;
                m_PressingTime = Time.time;
            }
        }

        if(m_bAtkComboHold)
        {
            Proc_AttackComboControll();
        }

	}
    #endregion  MONO_FUNCTION

    #region MEMBER_FUNCTION
    private void initialize()
    {
        m_CheckPressing = false;
        m_PressingAttack = false;
        m_bAttacking = m_bAbleToPressSkill = false;
        m_PressingTime = 0.0f;
        m_bAtkComboHold = false;
        m_bAtkComboHoldAgn = false;
        //m_bPrevAtkCombo = false;

        //atk combo
        m_AtkComboTree = eATTACKCOMBO_TREE.NONE;
        m_AtkComboCnt = 0;
    }

    public void DestroyCharController()
    {
        if (m_charScr != null) m_charScr = null;
        if (m_playerDataInfo != null) m_playerDataInfo = null;
        if (m_AttackComboSet != null)
        {
            m_AttackComboSet.Clear();
            m_AttackComboSet = null;
        }
        if (m_AtkComboQueue != null)
        {
            m_AtkComboQueue.Clear();
            m_AtkComboQueue = null;
        }
        if (m_DpadMP != null)
        {
            m_DpadMP.DestroyDpadMovePlayer();
            m_DpadMP = null;
        }
        if (m_CameraManager != null) m_CameraManager = null;
    }

    public void Init_Character_ComboSet_DataLoad()
    {
        CExcelData_ATTACK_COMBO_SET.instance.Create();
        CExcelData_ATTACK_COMBO_SET.instance.MEMORYTEXT_Create();
    }

    public void InitAttack_Combo_Set()
    {
        for (int i = 0; i < CExcelData_ATTACK_COMBO_SET.instance.ATTACK_COMBO_SETBASE_nRecordCount; i++)
        {
            SkillComboSet tempSkillComboSet = new SkillComboSet();
            tempSkillComboSet.start_attack = CExcelData_ATTACK_COMBO_SET.instance.ATTACK_COMBO_SETBASE_GetSTART_ATTACK(i);
            tempSkillComboSet.Normal = CExcelData_ATTACK_COMBO_SET.instance.ATTACK_COMBO_SETBASE_GetNORMAL(i);
            tempSkillComboSet.Good = CExcelData_ATTACK_COMBO_SET.instance.ATTACK_COMBO_SETBASE_GetGOOD(i);
            tempSkillComboSet.Perfect = CExcelData_ATTACK_COMBO_SET.instance.ATTACK_COMBO_SETBASE_GetPERFECT(i);
            //Debug.Log(i + " th start:" + tempSkillComboSet.start_attack + "/ Nor:" + tempSkillComboSet.Normal + "/ Good:" + tempSkillComboSet.Good + "/Great:" + tempSkillComboSet.Great + "/ Per:" + tempSkillComboSet.Perfect);
            if (!m_AttackComboSet.ContainsKey(tempSkillComboSet.start_attack))
            {
                m_AttackComboSet.Add(tempSkillComboSet.start_attack, tempSkillComboSet);
            }
            tempSkillComboSet = null;
        }
    }

    public void SetAttackComboTree(eATTACKCOMBO_TREE arg)
    {
        m_AtkComboTree = arg;
    }

    private void SetFirstAnimationByTargetDist(float dist, CharacterBase target)
    {
        dist -= (target.GetRadius() + m_charScr.GetRadius());
        if (dist < 0.0f)
            dist = 0.0f;

        m_charScr.m_ComboDashDist = dist;

        if (target.m_CharacterType == eCharacter.eNPC)
        {
            if (dist > m_playerDataInfo.deadZone_Dist && dist <= m_playerDataInfo.semiTarget_Area)
            {
                NavMeshPath paths = new NavMeshPath();
                m_charScr.m_CharAi.GetNavMeshPath(target.m_MyObj.transform.position, ref paths);

                if (paths.corners.Length <= 2)
                {
                    m_charScr.attackTarget = target;
                    m_AtkComboTree = eATTACKCOMBO_TREE.DASH;
                }
                else
                    m_AtkComboTree = eATTACKCOMBO_TREE.START;
            }
            else 
            {
                m_AtkComboTree = eATTACKCOMBO_TREE.START;
            }
        }
        else
        {
            m_AtkComboTree = eATTACKCOMBO_TREE.START;
        }
    }

    private void SetComboAnimationByTargetDist(float dist, CharacterBase target)
    {
//        m_charScr.LookAtY(target.transform.position);

        dist -= (target.GetRadius() + m_charScr.GetRadius());
        if (dist < 0.0f)
            dist = 0.0f;

        m_charScr.m_ComboDashDist = dist;

        if (target.m_CharacterType == eCharacter.eNPC)
        {
            if (dist > m_playerDataInfo.deadZone_Dist && dist <= m_playerDataInfo.semiTarget_Area)
            {
                m_AtkComboQueue.Clear();
                m_AtkComboTree = eATTACKCOMBO_TREE.NONE;
            }
        }
    }
    //Below for Magician
    private void SetNormalAttackTargetDist(float dist, CharacterBase target)
    {
        m_charScr.LookAtY(target.transform.position);

        dist -= (target.GetRadius() + m_charScr.GetRadius());
        if (dist < 0.0f)
            dist = 0.0f;

        m_charScr.m_ComboDashDist = dist;

        if (target.m_CharacterType == eCharacter.eNPC)
        {
            m_charScr.attackTarget = target;
        }

    }

    private void SetFirstAnimationByTarget()
    {
        float dist = 0.0f;
        
        if (m_AtkComboTree != eATTACKCOMBO_TREE.NONE) return;

        // DPAD 지정
        // AUTOTARGET_ANGLE 사거리 검사
        // DPAD 미지정
        // 360 사거리 검사
        CharacterBase autoTarget = null;
        AutoAI charAI = (AutoAI)(m_charScr.m_CharAi);
        Vector2 v2DpadDir = m_DpadMP.GetDpadDirToCharDir();

        if (m_DpadMP != null && v2DpadDir != Vector2.zero)
        {
            //Taylor
            autoTarget = charAI.FindAutoTargeting(eTargetState.eHOSTILE, v2DpadDir, AUTOTARGET_ANGLE, m_playerDataInfo.semiTarget_Area);
            if( autoTarget == null )
                autoTarget = charAI.FindAutoTargeting(eTargetState.eHOSTILE, Vector2.zero, m_playerDataInfo.semiTarget_Angle, m_playerDataInfo.semiTarget_Area);
        }
        else
        {
            autoTarget = charAI.FindAutoTargeting(eTargetState.eHOSTILE, Vector2.zero, m_playerDataInfo.semiTarget_Angle, m_playerDataInfo.semiTarget_Area);
        }

        if (autoTarget != null)
        {
            m_charScr.SetAttackTarget(autoTarget);

            dist = Vector3.Distance(m_charObj.transform.position, autoTarget.transform.position);
            SetFirstAnimationByTargetDist(dist, autoTarget);
        }
        else
        {
            m_AtkComboTree = eATTACKCOMBO_TREE.START;
            m_charScr.SetAttackTarget(null);
        }

        SetAttackCombo_Start();
    }

    public void SetComboByTarget()
    {
        //float dist = 0.0f;

        CharacterBase autoTarget = null;
        AutoAI charAI = (AutoAI)(m_charScr.m_CharAi);
        Vector2 DpadDir = m_DpadMP.GetDpadDirToCharDir();

        if (m_DpadMP != null && DpadDir != Vector2.zero)
        {
            autoTarget = charAI.FindAutoTargeting(eTargetState.eHOSTILE, DpadDir, AUTOTARGET_ANGLE, m_playerDataInfo.semiTarget_Area);
        }
        else
        {
            autoTarget = charAI.FindAutoTargeting(eTargetState.eHOSTILE, Vector2.zero, 360f, m_playerDataInfo.semiTarget_Area);
        }

        if (autoTarget != null && autoTarget != m_charScr.attackTarget && m_charScr.attackTarget != m_charScr)
        {
            m_charScr.SetAttackTarget(autoTarget);
        }
    }

    private void SetAttackCombo_Start()
    {
        int itmpAttackId = 0;
        switch(m_AtkComboTree)
        {
            case eATTACKCOMBO_TREE.START:
                itmpAttackId = PlayerManager.instance.GetCurStartAttack(m_charScr);
                m_AtkComboQueue.Add(itmpAttackId);
                m_prevAtkIndex = itmpAttackId;
                break;
            case eATTACKCOMBO_TREE.DASH:
                itmpAttackId = PlayerManager.instance.GetCurDashAttack(m_charScr);
                if (itmpAttackId == 0)
                    itmpAttackId = PlayerManager.instance.GetCurStartAttack(m_charScr);
                m_AtkComboQueue.Add(itmpAttackId);
                m_prevAtkIndex = itmpAttackId;
                break;
            case eATTACKCOMBO_TREE.MAX_DASH:
                break;
        }
        m_AtkComboCnt++;
        m_charScr.SetChangeMotionState(MOTION_STATE.eMOTION_ATTACK_COMBO);
    }

    public void SetAttackCombo_End()
    {
        if (m_AtkComboQueue.Count > 0)
        {
            m_AtkComboQueue.Clear();
            m_AtkComboTree = eATTACKCOMBO_TREE.NONE;
        }
    }

    #endregion MEMBER_FUNCTION

    #region UI_REFER
    public bool PressMove(bool bPress)
    {
        if (m_AtkComboTree != eATTACKCOMBO_TREE.NONE) return false;
////        Debug.Log("PressMove() "+bPress);
        if (bPress)
        {
            // 도발
            if (m_charScr.m_TauntMove.m_bTaunt)
                return false;
            
            // 이동 불가
            if (!m_charScr.CheckMontionStateChange(MOTION_STATE.eMOTION_RUN, null))
                return false;

            //Run
            m_charScr.SetChangeMotionState(MOTION_STATE.eMOTION_RUN);
        }
        else
        {
            if (!m_charScr.CheckMontionStateChange(MOTION_STATE.eMOTION_IDLE, null))
                return false;
            //Stop : Idle
            if (((AutoAI)m_charScr.m_CharAi).GetAutoPlayState() != AutoAI.eAutoProcessState.ePlay)
                m_charScr.SetChangeMotionState(MOTION_STATE.eMOTION_IDLE);
            else
            {
                if (m_DpadMP.m_bMovePositionCheck == false)
                {
                    ((AutoAI)m_charScr.m_CharAi).SetAutoPlayState(AutoAI.eAutoProcessState.eReady);

                    ((AutoAI)m_charScr.m_CharAi).StopAutoPlay();
                }
            }

            m_DpadMP.StopRun(eMoveType.eNONE);
        }
        return true;
    }

    public void PressingAttack(bool bPressing)
    {
        //Debug.Log("MyCharacterController.PressingAttack( "+bPressing+" )");
        m_PressingAttack = bPressing;

        if (bPressing)
        {
            m_CheckPressing = true;
            if(m_bAtkComboHold)
            {
                m_bAtkComboHoldAgn = true;
            }
            Proc_AttackComboControll();
        }
        else
        {
            m_CheckPressing = false;
        }


        m_PressingTime = Time.time;
    }

    public bool CheckIsEndAttackID()
    {
        if (m_prevAtkIndex == PlayerManager.instance.GetCurEndAttack(m_charScr) )
        {
            return true;
        }
        return false;
    }

    private int GetNextStrSkillIndexByComboTiming()
    {
        //Debug.Log(" ########## GetNextSkillIndexByComboTiming() Start ##########");
        if (!CheckIsEndAttackID())
        {
            //Debug.Log("GetNextSkillIndexByComboTiming() atkComboState = " + atkComboState + "/ m_prevAtkIndex = " + m_prevAtkIndex + "/comboCnt = " + m_AtkComboCnt);

            if(m_bAtkComboHold)
            {
                m_CurAtkComboState = eAttackCombo_Key.eAttackCombo_Normal;
            }

            //Read combo index from ATTACK_COMBO table!!!!
            switch (m_CurAtkComboState)
            {
                case eAttackCombo_Key.eAttackCombo_Normal:
                    //Debug.Log("GetNextSkillIndexByComboTiming() normal return Value = " + m_AttackComboSet[m_prevAtkIndex].Normal);
                    return m_AttackComboSet[m_prevAtkIndex].Normal;
                case eAttackCombo_Key.eAttackCombo_Good:
                    //Debug.Log("GetNextSkillIndexByComboTiming() good return Value = " + m_AttackComboSet[m_prevAtkIndex].Good);
                    return m_AttackComboSet[m_prevAtkIndex].Good;
                case eAttackCombo_Key.eAttackCombo_Perfect:
                    //Debug.Log("GetNextSkillIndexByComboTiming() Perfect return Value = " + m_AttackComboSet[m_prevAtkIndex].Perfect);
                    return m_AttackComboSet[m_prevAtkIndex].Perfect;
            }
        }
        //Debug.Log(" ########## GetNextSkillIndexByComboTiming() Finish ##########");
        return -1;
    }

    private void Proc_AttackComboControll()
    {
        if (!m_charScr.CheckMontionStateChange(MOTION_STATE.eMOTION_ATTACK_COMBO, null))
            return;
        //Debug.Log("Proc_AttackComboControll() tree = " + m_AtkComboTree);
        if (m_charScr.m_bMotionCancel == false && m_charScr.m_MotionState == MOTION_STATE.eMOTION_SKILL) return;
        //first Attack
        if (m_AtkComboTree == eATTACKCOMBO_TREE.NONE)
        {
            SetFirstAnimationByTarget();
        }
        else
        {
            int nextSkillIdx = 0;
            //Debug.Log("Proc_AttackComboControll() tree = " + m_AtkComboTree + "/Queue Count = " + m_AtkComboQueue.Count );
            if (m_AtkComboQueue.Count >= 2) 
                return;

            //Magician Auto Targetting after first attack
            nextSkillIdx = GetNextStrSkillIndexByComboTiming();

            //Debug.Log("nextSkillIdx = " + nextSkillIdx + "/Queue Count = " + m_AtkComboQueue.Count);
            //Debug.Log("m_atkComboCnt = " + m_AtkComboCnt);

            if (nextSkillIdx > 0)
            {
                if( PlayerManager.instance.GetMaxNormalAttackCount(m_charScr,((AutoAI)m_charScr.m_CharAi).GetStanceForIndex()) > 1)
                {
                    m_prevAtkIndex = nextSkillIdx;
                    m_AtkComboQueue.Add(nextSkillIdx);
                }
            }
        }
    }

    //public bool ButtonController(eSKILL_TYPE type)
    public bool ButtonController(eSKILL_TYPE type, int skillIndex)
    {
        if ( ((AutoAI)m_charScr.m_CharAi).GetAutoPlayState() == AutoAI.eAutoProcessState.eEnding )
            return false;

        PlayerInfo pInfo = PlayerManager.instance.m_PlayerInfo[m_charScr.charUniqID];
        //PlayerDataInfo pDataInfo = PlayerManager.instance.m_PlayerDataInfo[(int)pInfo.jobType];

        AutoAI autoAI = (AutoAI)m_charScr.m_CharAi;
        //Taylor : not use anymore
        //autoAI.ChangeReserveStance();

        int iStance = (int)UserManager.instance.m_CurPlayingCharInfo.playerInfo.m_eStanceType - 1;
        PlayerStanceInfo StanceInfo = PlayerManager.instance.GetPlayerStanceInfo(m_charScr, iStance);
        //PlayerStanceInfo StanceInfo = PlayerManager.instance.GetPlayerStanceInfo(m_charScr, (int)autoAI.GetStance());

        switch(type)
        {
            case eSKILL_TYPE.eSKILL_SPECIAL:
                m_charScr.m_currSkillIndex = PlayerManager.instance.GetCurSpecialSkill(m_charScr);

                switch ( m_charScr.GetAttackInfo(m_charScr.m_currSkillIndex).skillinfo.action_Type )
                {
                    case eSkillActionType.eMOVEMENT:
                        m_charScr.CameraAnimatorInit();
                        break;
                    case eSkillActionType.eSHIELD_BLOCK:
                        if (m_charScr.m_bGuardAvail)
                            m_charScr.m_bGuardPress = true;
                        else
                        {
                            m_charScr.m_currSkillIndex = 0;
                            m_charScr.m_ChainMove.m_AnimName = "";
                            return false;
                        }
                        return true;
                    case eSkillActionType.eCHAIN:
                        break;
                    default:
                        break;
                }
                break;
            case eSKILL_TYPE.eSKILL_AVOIDROLLING:
                m_charScr.m_currSkillIndex = PlayerManager.instance.GetCurSpecialSkill(m_charScr);
                m_charScr.CameraAnimatorInit();
                break;
            default:
                //m_charScr.m_currSkillIndex = StanceInfo.m_nActiveSkillID[(int)type];
                m_charScr.m_currSkillIndex = StanceInfo.m_nActiveSkillID[skillIndex];
                break;
        }

        if (m_charScr.charUniqID == PlayerManager.MYPLAYER_INDEX)
        {
            if (pInfo.m_SkillCutSceneDic.ContainsKey(m_charScr.m_currSkillIndex))
            {
                // gunny comment 20170313
                // 스킬 연출 카메라 배경 까맣게
                CinemaSceneManager.instance.m_CutSkillCutScene = pInfo.m_SkillCutSceneDic[m_charScr.m_currSkillIndex];
                CinemaSceneManager.instance.m_bSkillCutScenePlaying = true;;
                CinemaSceneManager.instance.SetCameraEffDarkness();
                CinemaSceneManager.instance.CutScenePlay(pInfo.m_SkillCutSceneDic[m_charScr.m_currSkillIndex]);
                
                //Debug.Break();
            }
        }

        if (!m_charScr.CheckMontionStateChange(MOTION_STATE.eMOTION_SKILL, null))
        {
            m_charScr.m_currSkillIndex = 0;
            m_charScr.m_ChainMove.m_AnimName = "";
            return false;
        }

        //When Skill Button is pressed, reservated action is canceled.
        if (m_AtkComboQueue.Count > 0)
        {
            m_AtkComboQueue.Clear();
            m_AtkComboTree = eATTACKCOMBO_TREE.NONE;
        }

        if (((AutoAI)m_charScr.m_CharAi).GetAutoPlayState() != AutoAI.eAutoProcessState.ePlay)
            m_charScr.SetChangeMotionState(MOTION_STATE.eMOTION_SKILL);
        
        if (m_CameraManager == null)
        {
            m_CameraManager.fTimer = 0;
        }
        return true;
    }

    #endregion  UI_REFER
    

    #region PROPERTY
    //public CharObject charScr
    public CharacterBase charScr
    {
        get { return m_charScr; }
        set { m_charScr = value; }
    }

    public eAttackCombo_Key atkComboState
    {
        get { return m_AtkComboState; }
        set { m_AtkComboState = value; }
    }
    #endregion  PROPERTY
}
