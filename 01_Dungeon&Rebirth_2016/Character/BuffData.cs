using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class BuffData : PoolItem
{
    public enum eBuffState
    {
        eNONE = 0,
        eSTART,
        eUPDATE,
        eEND,
        eFAIL
    }
    public override void Init()
    {
        m_uiBuffIndex = 0;
        m_iType = eBuffType.eNONE ;
        m_iGroup = 0;
        m_iLv = 0;
        m_iGroupIndex = 0;

        m_fBuffArea = 0.0f;

        m_iRemoveDead = 0;
        //    public int m_iRemoveLogOut;
        m_iRemoveStageOut = 0;
        m_iRemoveHit = 0;

        m_bOnce = false;
        m_fLifeTime = 0.0f;
        m_fTicTime = 0.0f;
        m_fTicTimeRemain = 0.0f;

        m_AffectCode = eAffectCode.eNONE;
        m_AffectValue = 0;

        m_BuffState = eBuffState.eNONE;
    
        m_SourceChar = null;

        m_Controller = null;

        m_bDontprevent = false;
    }
    public uint m_uiBuffIndex;
    public eBuffType m_iType;
    public int m_iGroup;
    public int m_iLv;
    public int m_iGroupIndex;
    public eBuff_Trait m_eTrait;

    public float m_fBuffArea;

    public int m_iRemoveDead;
    //    public int m_iRemoveLogOut;
    public int m_iRemoveStageOut;
    public int m_iRemoveHit;

    public int[] m_iCharDisable = new int[(int)BuffController.eBuffCharDisable.eMAX];

    public bool m_bOnce = false;
    public float m_fBuffTime;
    public float m_fLifeTime;
    public float m_fTicTime;
    public float m_fTicTimeRemain;

    public float m_fSpecialTime;

    public int m_iSkillCode;
    public eSkillActiveType m_eSkillType;
    public eAffectCode m_AffectCode;
    public long m_AffectValue;

    public List<int> m_iBuffHitEffectIndex;
    public List<int> m_iBuffEffectIndex;

    public eBuffState m_BuffState = eBuffState.eNONE;

    protected const float DEAD_BUFF_TIMEOUT = 15.0f;

    public string m_strAtlas;
    public string m_strSprite;
    public string m_strHistory;
    public string m_strName;
    public string m_strSkillAffect;

    public bool m_bDontprevent;

    public bool m_bCheckClass;
    public bool m_AddBuffFlag = false;
    public enum BUFF_STYLE
    {
        BUFF_STYLE_CALCULATION,
        BUFF_STYLE_REACTION,
        BUFF_STYLE_MAX
    }

    public BUFF_STYLE m_eBuffStyle;

    public CharacterBase m_SourceChar;

    public BuffController m_Controller;

    public virtual void Start()
    {
        m_BuffState = eBuffState.eSTART;
    }
    public virtual void ReStart(BuffData p_kBuffInfo)
    {
    }
    public virtual void Update(float p_fDeltaTime)
    {
        m_BuffState = eBuffState.eUPDATE;
    }
    public virtual void End()
    {
        m_BuffState = eBuffState.eEND;
    }
    public virtual void Fail()
    {
        m_BuffState = eBuffState.eFAIL;
    }
    public virtual void CopyBuff( BuffData p_SrcBuff)
    {

    }
}

public class BuffData_Calculation : BuffData
{
    public BuffData_Calculation() { }
    public void SettingBuff( CharacterBase p_SourceChar, BuffController p_Controller , SkillDataInfo.SkillInfo p_kBuffInfo , int p_iAffectIndex)
    {
        m_SourceChar = p_SourceChar;
        m_Controller = p_Controller;

        m_eBuffStyle = BUFF_STYLE.BUFF_STYLE_CALCULATION;

        m_iType = p_kBuffInfo.buff_Type;
        m_iGroup = p_kBuffInfo.buff_Group;
        m_iLv = p_kBuffInfo.buff_Lv;
        m_eTrait = p_kBuffInfo.buff_trait;

        if (m_eTrait == eBuff_Trait.eSUPERARMOR)
        {
            m_SourceChar.m_bBossSuperArmor = true;
        }

        m_fBuffArea = p_kBuffInfo.area_Size;

        m_fBuffTime = m_fLifeTime = p_kBuffInfo.buff_Time[0];
        m_fTicTime = p_kBuffInfo.buff_Time[1];
        m_fTicTimeRemain = 0.0f;
        m_fSpecialTime = 0.0f;

        m_iRemoveDead = p_kBuffInfo.buff_Dead;
//        m_iRemoveLogOut = p_kBuffInfo.buff_LogOut;
        m_iRemoveStageOut = p_kBuffInfo.buff_Stage_Out;
        m_iRemoveHit = p_kBuffInfo.buff_Hit_Cancel;

        m_iCharDisable[(int)BuffController.eBuffCharDisable.eATTACK] = p_kBuffInfo.buff_Atk_Control;
        m_iCharDisable[(int)BuffController.eBuffCharDisable.eMOVE] = p_kBuffInfo.buff_Move_Control;
        m_iCharDisable[(int)BuffController.eBuffCharDisable.eITEM] = p_kBuffInfo.buff_Item_Control;
        m_iCharDisable[(int)BuffController.eBuffCharDisable.eSKILL] = p_kBuffInfo.buff_Skill_Control;
        
        m_iSkillCode = p_kBuffInfo.skill_Code;
        m_eSkillType = p_kBuffInfo.skill_Type;
        m_AffectCode = p_kBuffInfo.system_Affect_Code[p_iAffectIndex];
        m_AffectValue = p_kBuffInfo.system_Affect_Value[p_iAffectIndex];

        m_iGroupIndex = p_iAffectIndex;

        m_iBuffEffectIndex = new List<int>(0);
        m_iBuffHitEffectIndex = new List<int>(0);

        if (p_kBuffInfo.Buff_Effect != null && p_kBuffInfo.Buff_Effect.Count > 0)
            m_iBuffEffectIndex.AddRange(p_kBuffInfo.Buff_Effect);

        if (p_kBuffInfo.skill_Hit_Effect != null && p_kBuffInfo.skill_Hit_Effect.Count > 0)
            m_iBuffHitEffectIndex.AddRange(p_kBuffInfo.skill_Hit_Effect);

        m_strAtlas          = p_kBuffInfo.strAtlasName;
        m_strSprite         = p_kBuffInfo.strSpriteName;
        m_strName           = p_kBuffInfo.skill_Name;
        m_strHistory        = p_kBuffInfo.skill_History;
        m_strSkillAffect    = p_kBuffInfo.skill_Affect;

        m_bCheckClass = p_kBuffInfo.AddClassBuff == 1;
    }
    public override void Start()
    {
        base.Start();

        if (m_eTrait == eBuff_Trait.eIGNITE)
        {
            BuffData burnData = m_Controller.FindBuff(eBuff_Trait.eBURN , m_SourceChar ) ;
            BuffData lacerationData = m_Controller.FindBuff(eBuff_Trait.eLACERATION , m_SourceChar ) ;
            int iDamage = 0;
            if ( burnData != null )
            {
                int iDamgePerTic = (int)(((float)burnData.m_AffectValue / (burnData.m_fBuffTime / burnData.m_fTicTime)));
                iDamage += iDamgePerTic * (int)(burnData.m_fLifeTime / burnData.m_fTicTime);
                m_Controller.BuffEnd(burnData);
            }
            if( lacerationData != null )
            {
                int iDamgePerTic = (int)(((float)lacerationData.m_AffectValue / (lacerationData.m_fBuffTime / lacerationData.m_fTicTime)));
                iDamage += iDamgePerTic * (int)(lacerationData.m_fLifeTime / lacerationData.m_fTicTime);
                m_Controller.BuffEnd(lacerationData);
            }
            m_AffectValue += iDamage;
        }

        if (m_fTicTime != 0.0f)
        {
            if (m_fTicTime == m_fLifeTime)
                m_fTicTimeRemain = m_fTicTime - 0.016f;
            else
                m_fTicTimeRemain = m_fTicTime;
        }
        m_Controller.PushBuffDataValue(this);
    }
    public override void ReStart(BuffData p_kBuffInfo)
    {
        base.ReStart(p_kBuffInfo);

        m_fBuffTime = m_fLifeTime = p_kBuffInfo.m_fLifeTime;

        m_Controller.ReSetBuffDataValue(this);
    }
    public override void Update(float p_fDeltaTime)
    {
        base.Update(p_fDeltaTime);

        if (m_fTicTimeRemain > 0.0f)
        {
            m_fTicTimeRemain -= p_fDeltaTime;
            if (m_fTicTimeRemain <= 0.0f)
            {
                m_Controller.PushBuffDataValue(this);

                if (m_fTicTime < m_fLifeTime)
                    m_fTicTimeRemain = m_fTicTime;
                else
                    m_fTicTimeRemain = 0.0f;
            }
        }
    }

    public override void End()
    {
        if (m_BuffState == eBuffState.eEND || m_BuffState == eBuffState.eFAIL)
            return;

        base.End();

        m_Controller.PopBuffDataValue(this);
    }

    public override void Fail()
    {
        base.Fail();
    }

    public override void CopyBuff(BuffData p_SrcBuff)
    {
        if (p_SrcBuff.m_eBuffStyle != BUFF_STYLE.BUFF_STYLE_CALCULATION)
            return;

        BuffData_Calculation srcBuff = (BuffData_Calculation)p_SrcBuff;

        m_iLv = srcBuff.m_iLv;

        m_fBuffArea = srcBuff.m_fBuffArea;

        m_fBuffTime = m_fLifeTime = srcBuff.m_fBuffTime;

        m_iSkillCode = srcBuff.m_iSkillCode;
        m_AffectCode = srcBuff.m_AffectCode;
        m_AffectValue = srcBuff.m_AffectValue;
    }
}

public class BuffData_ReAction : BuffData
{
    public enum eReActionType
    {
        REACTION=0,
        CROWDCONTROL,
    }
    public eReActionType m_iReActionType;
    public float m_ReactionTimeOut = 0.0f;

    public BuffData_ReAction() { }
    public void SettingBuff(CharacterBase p_SourceChar, BuffController p_Controller, SkillDataInfo.SkillInfo p_kBuffInfo, int p_iAffectIndex, eReActionType p_eReactionType)
    {
        m_SourceChar = p_SourceChar;
        m_Controller = p_Controller;

        m_eBuffStyle = BUFF_STYLE.BUFF_STYLE_REACTION;

        m_iType = p_kBuffInfo.buff_Type;
        m_iGroup = p_kBuffInfo.buff_Group;
        m_iLv = p_kBuffInfo.buff_Lv;
        m_eTrait = p_kBuffInfo.buff_trait;

        m_fBuffArea = p_kBuffInfo.area_Size;

        m_fBuffTime = m_fLifeTime = p_kBuffInfo.buff_Time[0];
        m_fTicTime = p_kBuffInfo.buff_Time[1];
        m_fTicTimeRemain = 0.0f;
        m_fSpecialTime = 0.0f;

        m_iRemoveDead = p_kBuffInfo.buff_Dead;
//        m_iRemoveLogOut = p_kBuffInfo.buff_LogOut;
        m_iRemoveStageOut = p_kBuffInfo.buff_Stage_Out;
        m_iRemoveHit = p_kBuffInfo.buff_Hit_Cancel;

        m_iCharDisable[(int)BuffController.eBuffCharDisable.eATTACK] = p_kBuffInfo.buff_Atk_Control;
        m_iCharDisable[(int)BuffController.eBuffCharDisable.eMOVE] = p_kBuffInfo.buff_Move_Control;
        m_iCharDisable[(int)BuffController.eBuffCharDisable.eITEM] = p_kBuffInfo.buff_Item_Control;
        m_iCharDisable[(int)BuffController.eBuffCharDisable.eSKILL] = p_kBuffInfo.buff_Skill_Control;

        m_iSkillCode = p_kBuffInfo.skill_Code;
        m_eSkillType = eSkillActiveType.eBUFF;
        m_AffectCode = p_kBuffInfo.system_Affect_Code[p_iAffectIndex];
        m_AffectValue = p_kBuffInfo.system_Affect_Value[p_iAffectIndex];

        //Taylor : asked from pd. added state going to eKNOCKBACK_INT_2
        if ((m_AffectCode == eAffectCode.eKNOCK_BACK_N || m_AffectCode == eAffectCode.eAIRBORNE_N || m_AffectCode == eAffectCode.eDOWN_N) && p_Controller.m_CharBase.damageCalculationData.fCURRENT_HIT_POINT <= 0)
            m_AffectCode = eAffectCode.eKNOCKBACK_INT_2;

        m_iGroupIndex = p_iAffectIndex;

        m_iReActionType = p_eReactionType;

        m_ReactionTimeOut = m_fLifeTime + DEAD_BUFF_TIMEOUT;

        m_iBuffHitEffectIndex = new List<int>();
        m_iBuffEffectIndex = new List<int>();

        if (p_kBuffInfo.Buff_Effect != null && p_kBuffInfo.Buff_Effect.Count > 0)
            m_iBuffEffectIndex.AddRange(p_kBuffInfo.Buff_Effect);

        if (p_kBuffInfo.skill_Hit_Effect != null && p_kBuffInfo.skill_Hit_Effect.Count > 0)
            m_iBuffHitEffectIndex.AddRange(p_kBuffInfo.skill_Hit_Effect);

        m_strAtlas  = p_kBuffInfo.strAtlasName;
        m_strSprite = p_kBuffInfo.strSpriteName;

        m_bCheckClass = p_kBuffInfo.AddClassBuff == 1;
    }

    public override void Start()
    {
        base.Start();
        m_Controller.PushBuffDataValue(this);
    }

    public override void ReStart(BuffData p_kBuffInfo)
    {
        base.ReStart(p_kBuffInfo);
        m_fBuffTime = m_fLifeTime = p_kBuffInfo.m_fLifeTime;
        m_ReactionTimeOut = m_fLifeTime + DEAD_BUFF_TIMEOUT;

        m_Controller.ReSetBuffDataValue(this);
    }

    public override void Update(float p_fDeltaTime)
    {
        base.Update(p_fDeltaTime);

        m_ReactionTimeOut -= p_fDeltaTime;

        if( m_ReactionTimeOut < 0.0f )
        {
            End();
        }
    }

    public override void End()
    {
        if (m_BuffState == eBuffState.eEND || m_BuffState == eBuffState.eFAIL)
            return;

        base.End();
        m_Controller.PopBuffDataValue(this);
    }
    
    public override void Fail()
    {
        if (m_BuffState == eBuffState.eEND || m_BuffState == eBuffState.eFAIL)
            return;

        base.Fail();
//        m_Controller.PopBuffDataValue(this);
    }
}