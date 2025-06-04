using UnityEngine;
using System.Collections;

public class UI_InGameButton : MonoBehaviour
{
    #region ENUM

    public enum eTYPE
    {
        CLICK,
        PRESS,
    }

    public enum eCOOL_TIME
    {
        GLOBAL,
        SKILL,
        SKILL_SPECIAL
    }

    #endregion ENUM


    #region VARIABLES

    private EventDelegate.Callback m_clickCallback      = null;
    private EventDelegate.Callback m_pressCallback      = null;
    private EventDelegate.Callback m_releaseCallback    = null;

    private eTYPE m_eType                               = eTYPE.CLICK;

    public float m_fMaxCoolTime                        = 0;
    public float m_fCurrentCoolTime                     = 0;

    private eCOOL_TIME m_eCoolTime                      = eCOOL_TIME.GLOBAL;

    public UISprite m_spCoolTime                       = null;
    private UILabel m_lbCoolTime                        = null;
    private GameObject m_goBlock                        = null;

    private bool m_bIsForcedBlock                       = false;

    public GameObject m_goLock                         = null;
    private int m_nOpenLevel                            = 0;
    private long m_lPrevLevel                           = 0;

    #endregion VARIABLES

    ParticleSystem obj = null;
    float objLifeTime = 0;
    UIButton uib = null;

    #region UNITY FUNC

    private void Awake()
    {
        m_spCoolTime = this.transform.Find("Sp_CoolTime").GetComponent<UISprite>();
        m_lbCoolTime = this.transform.Find("Sp_CoolTime/Label").GetComponent<UILabel>();
        m_goBlock = this.transform.Find("Sp_Block").gameObject;

        Transform trLock = this.transform.Find("Sp_LockBack");
        if (trLock != null)
            m_goLock = trLock.gameObject;
    }

    private void Update()
    {
        if ( m_bIsForcedBlock == false )
        {
            if (m_fCurrentCoolTime <= 0)
            {
                if (m_spCoolTime.gameObject.activeSelf)
                {
                    m_spCoolTime.gameObject.SetActive(false);
                }
            }
            else
            {
                m_fCurrentCoolTime -= Time.deltaTime;

                if (m_eCoolTime == eCOOL_TIME.SKILL)
                {
                    m_spCoolTime.fillAmount = m_fCurrentCoolTime / m_fMaxCoolTime;

                    if (m_lbCoolTime.text != ((int)m_fCurrentCoolTime + 1).ToString())
                        m_lbCoolTime.text = ((int)m_fCurrentCoolTime + 1).ToString();

                    if (m_fCurrentCoolTime < 1)
                    {
                        m_lbCoolTime.text = m_fCurrentCoolTime.ToString("n1");
                    }
                }
                else if(m_eCoolTime == eCOOL_TIME.SKILL_SPECIAL)
                {
                    m_spCoolTime.fillAmount = m_fCurrentCoolTime / m_fMaxCoolTime;

                    m_lbCoolTime.text = m_fCurrentCoolTime.ToString("n1");
                }
                else if (m_eCoolTime == eCOOL_TIME.GLOBAL)
                {
                    m_spCoolTime.fillAmount = 1;

                    if (m_lbCoolTime.text != "")
                        m_lbCoolTime.text = "";
                }
                
                
                if (m_spCoolTime.fillAmount <= 0 && (m_eCoolTime == eCOOL_TIME.SKILL || m_eCoolTime == eCOOL_TIME.SKILL_SPECIAL))
                {
                    uib = m_spCoolTime.transform.parent.GetComponent<UIButton>();
                    Transform aa = uib.transform.Find("FX_UI_Ingame_Cooltime");
                    if (aa != null)
                    {
                        // 쿨타임이 끝날 때 확인용 이펙트를 활성화 시켜주자.
                        obj = aa.GetComponent<ParticleSystem>();
                        obj.gameObject.SetActive(true);
                        obj.Play();

                    }
                }
            }            
        }

        CheckOpen();
    }

    private void OnParticleDisable()
    {
        obj.gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        if (m_releaseCallback != null)
            m_releaseCallback();
    }

    #endregion UNITY FUNC

    #region PUBLIC FUNC

    public void Init(string strAtlas, string strSprite, EventDelegate.Callback clickCallback, int nOpenLevel)
    {
        SetButtonImage(m_goBlock.GetComponent<UISprite>(), strAtlas, strSprite);
        SetButtonImage(m_spCoolTime, strAtlas, strSprite);

        UISprite spIcon = this.transform.Find("Sp_Icon").GetComponent<UISprite>();
        SetButtonImage(spIcon, strAtlas, strSprite);

        m_eType = eTYPE.CLICK;
        
        m_clickCallback = clickCallback;

        m_nOpenLevel = nOpenLevel;
        long lCurLevel = PlayerManager.instance.GetMyPlayerInfo().level;
        if (m_nOpenLevel <= lCurLevel)
        {
            m_goBlock.SetActive(false);
        }


    }

    public void Init(string strAtlas, string strSprite, EventDelegate.Callback pressCallback, EventDelegate.Callback releaseCallback)
    {
        SetButtonImage(m_goBlock.GetComponent<UISprite>(), strAtlas, strSprite);
        SetButtonImage(m_spCoolTime, strAtlas, strSprite);

        UISprite spIcon = this.transform.Find("Sp_Icon").GetComponent<UISprite>();
        SetButtonImage(spIcon, strAtlas, strSprite);

        m_eType = eTYPE.PRESS;

        m_pressCallback = pressCallback;
        m_releaseCallback = releaseCallback;
    }

    public void SetButtonImage(UISprite spIcon, string strAtlas, string strSprite)
    {
        UIAtlas atlas = ResourceManager.instance.GetAtlas(strAtlas + "_FOR_INGAME_A");

        spIcon.atlas = atlas;
        spIcon.spriteName = strSprite;
    }

    public void StartCoolTime(float fAnimationTime, float fCoolTime, UI_InGameButton pressedButton, bool bSkillBtn)
    {
        uib = m_spCoolTime.transform.parent.GetComponent<UIButton>();
        Transform aa = uib.transform.Find("FX_UI_Ingame_Cooltime");
        if (aa != null)
        {
            obj = aa.GetComponent<ParticleSystem>();
            obj.gameObject.SetActive(false);
        }

        if (pressedButton == this)
        {
            if (m_fCurrentCoolTime < fCoolTime)
            {
                if (bSkillBtn)
                {
                    m_eCoolTime = eCOOL_TIME.SKILL;
                }
                else
                {
                    m_eCoolTime = eCOOL_TIME.SKILL_SPECIAL;
                }

                m_fCurrentCoolTime = fCoolTime;
                m_fMaxCoolTime = fCoolTime;
            }
            
            m_spCoolTime.gameObject.SetActive(true);

        }
        else
        {
            if (m_fCurrentCoolTime < fAnimationTime)
            {
                m_eCoolTime = eCOOL_TIME.GLOBAL;

                m_fCurrentCoolTime = fAnimationTime;
                m_fMaxCoolTime = fAnimationTime;
            }

            m_spCoolTime.gameObject.SetActive(true);
        }
    }

    public void SkillChangeCoolTimes(float fMaxCoolTime, float fCoolTime, int Index)
    {
        m_fCurrentCoolTime = fCoolTime;
        m_fMaxCoolTime = fMaxCoolTime;

        m_eCoolTime = eCOOL_TIME.SKILL;
        m_spCoolTime.gameObject.SetActive(true);
    }

    public void SetCoolTimeActive(bool bActive)
    {
        if ( m_bIsForcedBlock != bActive )
        {
            m_spCoolTime.gameObject.SetActive(bActive);
            m_spCoolTime.fillAmount = 1;

            m_lbCoolTime.text = "";

            m_bIsForcedBlock = bActive;
        }
    }

    public void SetBlock(bool bActive)
    {
        m_goBlock.SetActive(bActive);
    }

    public void OnClick()
    {
        if (m_eType == eTYPE.CLICK)
        {
            if (m_clickCallback != null)
                m_clickCallback();
        }
    }

    public void OnPress(bool bPressed)
    {
        if (m_eType == eTYPE.PRESS)
        {
            if (bPressed == true)
            {
                if (m_pressCallback != null)
                    m_pressCallback();
            }
            else
            {
                if (m_releaseCallback != null)
                    m_releaseCallback();
            }
        }
    }

    public void ResetCoolTime()
    {
        m_fCurrentCoolTime = 0;
        m_fMaxCoolTime = 0;

        m_spCoolTime.gameObject.SetActive(false);
    }
    #endregion PUBLIC FUNC


    #region PRIVATE FUNC

    private void CheckOpen()
    {
        if (m_goLock != null && m_goLock.activeSelf == true)
        {
            long lCurLevel = PlayerManager.instance.GetMyPlayerInfo().level;
            
            if (lCurLevel != m_lPrevLevel)
            {
                m_lPrevLevel = lCurLevel;

                if (m_nOpenLevel <= lCurLevel)
                {
                    m_goLock.SetActive(false);
                    if(m_goBlock.activeSelf)
                    {
                        m_goBlock.SetActive(false);
                    }
                }

            }
        }
    }

    #endregion PRIVATE FUNC
}
