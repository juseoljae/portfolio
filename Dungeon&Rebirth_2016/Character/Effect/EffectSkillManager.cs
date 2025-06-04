using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using System.Text.RegularExpressions;


public class EffectSkillManager : MonoBehaviour
{
    public struct GuideEffectInfo
    {
        public Vector3 position;
        public Vector3 scale;
        public float angle;
        public int use;

        public GuideEffectInfo(Vector3 pos, Vector3 scl, float ang, int u)
        {
            position = pos;
            scale = scl;
            angle = ang;
            use = u;
        }
    }

    public enum EFFECT_TYPE
    {
        eNONE = 0,
        eATTACK,
        eBUFF,
        eHIT,
        eBODY,
        eMAX,
    }

    public List<SkillDataInfo.EffObjInfo> m_PlayingEffObjs = new List<SkillDataInfo.EffObjInfo>();

    private GameObject m_gobjCharacter;
    private CharacterBase m_CharacterBase;
    public List<SkillDataInfo.EffObjInfo> m_PlayingHitEffObjs = new List<SkillDataInfo.EffObjInfo>();

    void Start()
    {
        m_CharacterBase = this.GetComponent<CharacterBase>();
    }

    void Update()
    {
        EffectPlayProcess();

        if (m_CharacterBase.m_CharUniqID == PlayerManager.MYPLAYER_INDEX || m_CharacterBase.m_CharacterType == eCharacter.eHERONPC)
        {
            EffectHitPlayProcess();
        }
    }

    public void LoadEffectByInfo(CharacterBase cBase, List<SkillDataInfo.EffectResInfo> p_EffectResInfoList)
    {
        if (p_EffectResInfoList == null || p_EffectResInfoList.Count == 0)
        {
            return;
        }

        for (int i = 0; i < p_EffectResInfoList.Count; ++i)
        {
            LoadSkillEffectObject(cBase, p_EffectResInfoList[i]);
        }
    }

    public void AddProjectTileEffectList(CharacterBase cBase, List<SkillDataInfo.ProjectTileInfo> p_EffectResInfoList)
    {
        if (p_EffectResInfoList == null || p_EffectResInfoList.Count == 0)
        {
            return;
        }

        for (int i = 0; i < p_EffectResInfoList.Count; ++i)
        {
            LoadProjectTileEffectObject(cBase, p_EffectResInfoList[i]);
        }
    }

    public void LoadEffectGuide(CharacterBase cBase, SkillDataInfo.EffectResInfo effGuide)
    {
        if (effGuide == null)
            return;

        LoadSkillEffectObject(cBase, effGuide);
    }

    public void LoadSkillEffectObject(CharacterBase cBase, SkillDataInfo.EffectResInfo resInfo)
    {
        if (resInfo.byReuse > 0)
        {
            for (int i = 0; i < resInfo.byReuse; i++)
            {
                SkillDataInfo.EffObjInfo tmpEffObjInfo = LoadEffectObject(cBase, resInfo.unEffectID, resInfo.strEffectName, cBase.GetCharEffectBone(resInfo.strEffectTargetPosition));
                resInfo.m_EffObjList.Add(tmpEffObjInfo);
            }
        }
    }

    private void ReuseEffectObject(CharacterBase cBase, SkillDataInfo.EffectResInfo resInfo, bool bGuideEffect = false)
    {
        if (resInfo.m_EffObjList.Count > 0)
        {
            for (int i = 0; i < resInfo.m_EffObjList.Count; i++)
            {
                if (resInfo.m_EffObjList[i].m_EffPrefab != null)
                {
                    resInfo.m_EffObjList.Add(InstantiateEffectObj(cBase, resInfo.m_EffObjList[0], cBase.GetCharEffectBone(resInfo.strEffectTargetPosition)));
                    break;
                }
                else
                {
                    resInfo.m_EffObjList.Add(LoadEffectObject(cBase, resInfo.unEffectID, resInfo.strEffectName, cBase.GetCharEffectBone(resInfo.strEffectTargetPosition)));
                    break;
                }
            }
        }
        else
        {
            if (bGuideEffect)
            {
                if (m_PlayingEffObjs.Count > 0)
                {
                    for (int i = 0; i < m_PlayingEffObjs.Count; i++)
                    {
                        if (m_PlayingEffObjs[i].m_EffName.Contains(resInfo.strEffectName))
                        {
                            m_PlayingEffObjs[i].m_bIsPlaying = false;
                            m_PlayingEffObjs[i].m_bStopPlay = false;
                            m_PlayingEffObjs[i].m_fPlayTime = 0;
                            m_PlayingEffObjs[i].m_iNewCreate = 1;

                            resInfo.m_EffObjList.Add(m_PlayingEffObjs[i]);
                            return;
                        }
                    }

                    resInfo.m_EffObjList.Add(LoadEffectObject(cBase, resInfo.unEffectID, resInfo.strEffectName, cBase.GetCharEffectBone(resInfo.strEffectTargetPosition)));

                    StringBuilder strbPath = new StringBuilder();
                    strbPath.AppendFormat("{0}/{1}", resInfo.m_EffObjList[0].m_EffPrefab.name, cBase.name);
                    resInfo.m_EffObjList[0].m_EffPrefab.name = strbPath.ToString();
                }
                else
                {
                    resInfo.m_EffObjList.Add(LoadEffectObject(cBase, resInfo.unEffectID, resInfo.strEffectName, cBase.GetCharEffectBone(resInfo.strEffectTargetPosition)));

                    StringBuilder strbPath = new StringBuilder();
                    strbPath.AppendFormat("{0}/{1}", resInfo.m_EffObjList[0].m_EffPrefab.name, cBase.name);
                    resInfo.m_EffObjList[0].m_EffPrefab.name = strbPath.ToString();
                }
            }
            else
            {
                resInfo.m_EffObjList.Add(LoadEffectObject(cBase, resInfo.unEffectID, resInfo.strEffectName, cBase.GetCharEffectBone(resInfo.strEffectTargetPosition)));
            }
        }
    }
    public void LoadHitEffectObject(CharacterBase cBase, SkillDataInfo.EffectResInfo resInfo, int MaxHitEffect)
    {
        for (int i = 0; i < MaxHitEffect; i++)
        {
            if (m_PlayingHitEffObjs.Count == 0)
            {

                m_PlayingHitEffObjs.Add(LoadEffectObject(cBase, resInfo.unEffectID, resInfo.strEffectName, cBase.GetCharEffectBone(resInfo.strEffectTargetPosition)));

            }
            else if (m_PlayingHitEffObjs.Count > 0)
            {
                m_PlayingHitEffObjs.Add(InstantiateEffectObj(cBase, m_PlayingHitEffObjs[0], cBase.GetCharEffectBone(resInfo.strEffectTargetPosition)));
            }
            StringBuilder strbPath = new StringBuilder();
            strbPath.AppendFormat("{0}/{1}", m_PlayingHitEffObjs[i].m_EffPrefab.name, cBase.name);
            m_PlayingHitEffObjs[i].m_EffPrefab.name = strbPath.ToString();
        }
    }
    public void ReuseHitEffectObject(CharacterBase cBase, SkillDataInfo.EffectResInfo resInfo)
    {
        if (m_PlayingHitEffObjs.Count == 0)
        {
            m_PlayingHitEffObjs.Add(LoadEffectObject(cBase, resInfo.unEffectID, resInfo.strEffectName, cBase.GetCharEffectBone(resInfo.strEffectTargetPosition)));
        }
        else if (m_PlayingHitEffObjs.Count > 0)
        {
            m_PlayingHitEffObjs.Add(InstantiateEffectObj(cBase, m_PlayingHitEffObjs[0], cBase.GetCharEffectBone(resInfo.strEffectTargetPosition)));
        }
        StringBuilder strbPath = new StringBuilder();
        strbPath.AppendFormat("{0}/{1}", m_PlayingHitEffObjs[m_PlayingHitEffObjs.Count - 1].m_EffPrefab.name, cBase.name);
        m_PlayingHitEffObjs[m_PlayingHitEffObjs.Count - 1].m_EffPrefab.name = strbPath.ToString();
    }
    public void SetHitEffectObject(CharacterBase cBase, SkillDataInfo.EffectResInfo resInfo, int index)
    {
        Transform tempTransform = cBase.GetCharEffectBone(resInfo.strEffectTargetPosition);
        m_PlayingHitEffObjs[index].m_EffPrefab.transform.parent = tempTransform;
        m_PlayingHitEffObjs[index].m_EffPrefab.transform.localPosition = Vector3.zero;
        m_PlayingHitEffObjs[index].m_EffPrefab.transform.localScale = Vector3.one;

        if (m_PlayingHitEffObjs[index].m_EffPrefab.transform.parent != null)
        {
            m_PlayingHitEffObjs[index].m_EffPrefab.transform.localEulerAngles = Vector3.zero;// EffPrefab.transform.parent.transform.localEulerAngles;
        }
        m_PlayingHitEffObjs[index].m_EffPrefab.transform.parent = InGameManager.instance.m_gobjEffectStandParent.transform;
        m_PlayingHitEffObjs[index].m_EffPrefab.SetActive(false);


    }
    public void LoadProjectTileEffectObject(CharacterBase cBase, SkillDataInfo.ProjectTileInfo pTileEffData)
    {
        SkillDataInfo.EffObjInfo tmpEffObjInfo = null;

        tmpEffObjInfo = LoadEffectObject(cBase, 0, pTileEffData.infoName, cBase.GetCharEffectBone(pTileEffData.effTarget_position));
        tmpEffObjInfo.m_EffPrefab.AddComponent<SkillSender>();
        pTileEffData.pTileObjList.Add(tmpEffObjInfo);

        if (!pTileEffData.endName.Equals("NONE"))
        {
            tmpEffObjInfo = LoadEffectObject(cBase, 0, pTileEffData.endName, InGameManager.instance.m_gobjEffectStandParent.transform);
            tmpEffObjInfo.m_EffPrefab.AddComponent<SkillSender>();
            pTileEffData.pTileEndObjList.Add(tmpEffObjInfo);
        }
    }

    public void InstantiateProjectTileObjListAdd(CharacterBase cBase, SkillDataInfo.ProjectTileInfo pTileEffData)
    {
        SkillDataInfo.EffObjInfo tmpEffObjInfo = null;

        if (pTileEffData.pTileEndObjList.Count > 0)
        {
            for (int i = 0; i < pTileEffData.pTileEndObjList.Count; i++)
            {
                if (pTileEffData.pTileEndObjList[0].m_EffPrefab != null)
                {
                    tmpEffObjInfo = InstantiateEffectObj(cBase, pTileEffData.pTileObjList[0], cBase.GetCharEffectBone(pTileEffData.effTarget_position));
                    tmpEffObjInfo.m_EffPrefab.AddComponent<SkillSender>();
                    pTileEffData.pTileObjList.Add(tmpEffObjInfo);

                    tmpEffObjInfo = InstantiateEffectObj(cBase, pTileEffData.pTileEndObjList[0], InGameManager.instance.m_gobjEffectStandParent.transform);
                    tmpEffObjInfo.m_EffPrefab.AddComponent<SkillSender>();
                    pTileEffData.pTileEndObjList.Add(tmpEffObjInfo);
                    break;
                }
                else
                {
                    LoadProjectTileEffectObject(cBase, pTileEffData);
                    break;
                }
            }
        }
        else
        {
            LoadProjectTileEffectObject(cBase, pTileEffData);
        }


    }

    public SkillDataInfo.EffObjInfo LoadEffectObject(CharacterBase cBase, uint iEffectID, string effName, Transform parentObj)
    {

        SkillDataInfo.EffObjInfo tmpEffObjInfo = new SkillDataInfo.EffObjInfo();
        float[] lifeTimes = null;

        tmpEffObjInfo.m_EffPrefab = Instantiate(EffectResourcesManager.instance.GetEffectFindAndLoadGameObject(effName)) as GameObject;
        tmpEffObjInfo.m_EffPrefab.transform.parent = null;
        tmpEffObjInfo.m_EffPrefab.transform.localPosition = Vector3.zero;

        tmpEffObjInfo.m_EffPrefab.transform.localEulerAngles = Vector3.zero;


        float fLifeTime = 0;
        bool bLooping = false;
        float fDuration = 0;
        float fScale = 1;
        ParticleSystem[] particles = tmpEffObjInfo.m_EffPrefab.GetComponentsInChildren<ParticleSystem>();
        if (particles.Length > 0)
        {
            lifeTimes = new float[particles.Length];

            for (int j = 0; j < particles.Length; ++j)
            {
                lifeTimes[j] = particles[j].duration + particles[j].startDelay + particles[j].startLifetime;
            }

            bLooping = particles[0].loop;
            fDuration = particles[0].duration;

            switch (cBase.m_CharacterType)
            {
                case eCharacter.eNPC:
                    if (cBase.m_CharAi != null)
                    {
                        fScale = cBase.m_MyObj.localScale.x * SkillDataManager.instance.GetEffectSize(iEffectID, cBase.m_CharAi.GetNpcProp().Npc_Size);
                    }
                    break;
                default:
                    fScale = cBase.m_MyObj.localScale.x * SkillDataManager.instance.GetEffectSize(iEffectID, 2);
                    break;
            }

            for (int j = 0; j < particles.Length; ++j)
            {
                particles[j].startSize *= fScale;

                if (fLifeTime < lifeTimes[j])
                {
                    if (particles[j].enableEmission)
                    {
                        fLifeTime = lifeTimes[j];
                    }
                }
            }
        }

        tmpEffObjInfo.m_EffPS = tmpEffObjInfo.m_EffPrefab.GetComponent<ParticleSystem>();
        tmpEffObjInfo.SetEffectSkillObjInfo(bLooping, fLifeTime, fDuration, effName);

        if (iEffectID >= 20000 && iEffectID <= 20113)
        {
            tmpEffObjInfo.m_EffPrefab.AddComponent<ParticleRenderQueue>();
        }

        tmpEffObjInfo.m_EffPrefab.SetActive(false);

        return tmpEffObjInfo;
    }

    private SkillDataInfo.EffObjInfo InstantiateEffectObj(CharacterBase cBase, SkillDataInfo.EffObjInfo refObjInfo, Transform parentObj)
    {
        SkillDataInfo.EffObjInfo tmpEffObjInfo = new SkillDataInfo.EffObjInfo();

        tmpEffObjInfo.m_EffPrefab = Instantiate(refObjInfo.m_EffPrefab) as GameObject;
        tmpEffObjInfo.m_EffPrefab.transform.parent = null;
        tmpEffObjInfo.m_EffPrefab.transform.localPosition = Vector3.zero;
        tmpEffObjInfo.m_EffPrefab.transform.localScale = Vector3.one;
        tmpEffObjInfo.m_EffPrefab.transform.localEulerAngles = Vector3.zero;
        tmpEffObjInfo.m_EffPrefab.SetActive(false);

        tmpEffObjInfo.m_EffPS = tmpEffObjInfo.m_EffPrefab.GetComponent<ParticleSystem>();
        tmpEffObjInfo.SetEffectSkillObjInfo(refObjInfo.m_bLooping, refObjInfo.m_fLifeTime, refObjInfo.m_fDuration, refObjInfo.m_EffName);

        return tmpEffObjInfo;
    }

    public void FXHitEffectPlay(CharacterBase cBase, SkillDataInfo.EffectResInfo p_EffectResInfo, Transform targetPosObj, GuideEffectInfo info = default(GuideEffectInfo), float p_fLifeTime = -1, bool bMatBuff = false, int idx = -1)
    {
        int playableObjIdx = -1;

        playableObjIdx = GetPlayableHitEffectIndex();
        if (playableObjIdx != -1)
        {
            SetHitEffectObject(cBase, p_EffectResInfo, playableObjIdx);


            m_PlayingHitEffObjs[playableObjIdx].m_EffPrefab.SetActive(true);
            m_PlayingHitEffObjs[playableObjIdx].m_bIsPlaying = true;
        }
        else
        {
            ReuseHitEffectObject(cBase, p_EffectResInfo);
            FXHitEffectPlay(cBase, p_EffectResInfo, targetPosObj, info, p_fLifeTime);
        }
    }

    public void FXEffectPlay(CharacterBase cBase, SkillDataInfo.EffectResInfo p_EffectResInfo, Transform targetPosObj, GuideEffectInfo info = default(GuideEffectInfo), float p_fLifeTime = -1, bool bMatBuff = false, int idx = -1, bool ParticleDuration = false)
    {
        int playableObjIdx = -1;
        if (p_EffectResInfo.strEffectTargetPosition == "NONE")
        {
            return;
        }

        playableObjIdx = GetPlayableEffectIndex(p_EffectResInfo);
        
        if (playableObjIdx != -1)
        {
            GameObject EffPrefab = p_EffectResInfo.m_EffObjList[playableObjIdx].m_EffPrefab;
            if (info.use == 0)
            {
                EffPrefab.transform.parent = targetPosObj;
                EffPrefab.transform.localPosition = Vector3.zero;
                EffPrefab.transform.localScale = Vector3.one;

                if (EffPrefab.transform.parent != null)
                {
                    EffPrefab.transform.localEulerAngles = Vector3.zero;// EffPrefab.transform.parent.transform.localEulerAngles;
                }

                if (p_EffectResInfo.bOrphan == 0)
                {
                    EffPrefab.transform.parent = InGameManager.instance.m_gobjEffectStandParent.transform;
                }
            }
            else
            {
                EffPrefab.transform.parent = InGameManager.instance.m_gobjEffectStandParent.transform;
                EffPrefab.transform.localPosition = info.position;
                EffPrefab.transform.localScale = info.scale;
                EffPrefab.transform.localEulerAngles = new Vector3(0, info.angle, 0);                
            }

            if (p_fLifeTime != -1)
            {
                if (ParticleDuration)
                {
                    p_EffectResInfo.m_EffObjList[playableObjIdx].m_fLifeTime = p_EffectResInfo.m_EffObjList[playableObjIdx].m_fDuration;
                }
                else
                {
                    p_EffectResInfo.m_EffObjList[playableObjIdx].m_fLifeTime = p_fLifeTime;
                }
#if UNITY_EDITOR
#endif

            }
            else
            {
                //when Hit, add stopTime 
                if (cBase.m_currSkillIndex != 0 && cBase.m_MotionState != MOTION_STATE.eMOTION_SPWAN_STANDBY)
                {
                    p_EffectResInfo.m_EffObjList[playableObjIdx].m_fLifeTime += cBase.m_AttackInfos[cBase.m_currSkillIndex].skillinfo.Stop_time[0];
                }
            }

            if (SceneManager.instance.GetCurSceneState() == SceneManager.eSCENE_STATE.Inventory || SceneManager.instance.GetCurSceneState() == SceneManager.eSCENE_STATE.RaidLobby)
            {
                if (EffPrefab.CompareTag("WeaponEffect"))
                {
                    EffPrefab.SetActive(true);
                    if (PlayerManager.instance.m_WeaponEffObj != null)
                    {
                        PlayerManager.instance.m_WeaponEffObj_2nd = EffPrefab;
                    }
                    else
                    {
                        PlayerManager.instance.m_WeaponEffObj = EffPrefab;
                    }
                    UtilManager.instance.SetEffectRenderQueue(EffPrefab, 3600);
                }
            }
            p_EffectResInfo.m_EffObjList[playableObjIdx].m_bIsPlaying = true;
            if (info.use == 0)
            {
                m_PlayingEffObjs.Add(p_EffectResInfo.m_EffObjList[playableObjIdx]);
            }
            else
            {
                if (p_EffectResInfo.m_EffObjList[playableObjIdx].m_iNewCreate == 0)
                {
                    p_EffectResInfo.m_EffObjList[playableObjIdx].m_iNewCreate = 1; // 1 로 만들어서 가이드 이펙트라고 알려준다.
                    m_PlayingEffObjs.Add(p_EffectResInfo.m_EffObjList[playableObjIdx]);
                }
            }

            EffPrefab.SetActive(true);

            if (bMatBuff)
            {
                EffPrefab.GetComponent<AddMaterialOnHit>().UpdateMaterial(targetPosObj, p_EffectResInfo.m_EffObjList[playableObjIdx].m_fLifeTime);
            }

            if (EffPrefab.CompareTag("WeaponEffect"))
            {
                for (int k = 0; k < m_PlayingEffObjs.Count; ++k)
                {
                    if (m_PlayingEffObjs[k].m_EffPS != null)
                    {
                        m_PlayingEffObjs[k].m_EffPS.Play();
                    }
                }
            }
        }
        else
        {
            if (info.use == 1)
            {
                ReuseEffectObject(cBase, p_EffectResInfo, true);
            }
            else
            {
                ReuseEffectObject(cBase, p_EffectResInfo);
            }

            if (bMatBuff)
            {
                FXEffectPlay(cBase, p_EffectResInfo, targetPosObj, info, p_fLifeTime, true, idx);
            }
            else
            {
                FXEffectPlay(cBase, p_EffectResInfo, targetPosObj, info, p_fLifeTime);
            }
        }
    }

    public void FXEffectPlay(CharacterBase cBase, SkillDataInfo.ProjectTileInfo p_EffectResInfo, float p_fLifeTime = -1)
    {
        int playableObjIdx = -1;
        if (p_EffectResInfo.effTarget_position == "NONE")
        {
            return;
        }

        playableObjIdx = GetPlayableEffectIndex(p_EffectResInfo);

        if (playableObjIdx != -1)
        {
            p_EffectResInfo.pTileObjList[playableObjIdx].m_EffPrefab.transform.parent = cBase.GetCharEffectBone(p_EffectResInfo.effTarget_position);
            p_EffectResInfo.pTileObjList[playableObjIdx].m_EffPrefab.transform.localEulerAngles = p_EffectResInfo.pTileObjList[playableObjIdx].m_EffPrefab.transform.parent.transform.localEulerAngles;
            p_EffectResInfo.pTileObjList[playableObjIdx].m_EffPrefab.transform.localPosition = Vector3.zero;

            //Orphan
            if (p_EffectResInfo.effOrphan == 0)
            {
                p_EffectResInfo.pTileObjList[playableObjIdx].m_EffPrefab.transform.parent = InGameManager.instance.m_gobjEffectStandParent.transform;
            }

            ProjectileFactory.CreateProjectilet(p_EffectResInfo.pTileObjList[playableObjIdx].m_EffPrefab, cBase, cBase.m_AttackInfos[cBase.m_UseSkillIndex].skillinfo, cBase.m_SysAffect_Idx, playableObjIdx, p_EffectResInfo.effScale);


            if (p_fLifeTime != -1)
            {
                p_EffectResInfo.pTileObjList[playableObjIdx].m_fLifeTime = p_fLifeTime;
            }
            p_EffectResInfo.pTileObjList[playableObjIdx].m_bIsPlaying = true;

            p_EffectResInfo.pTileObjList[playableObjIdx].m_EffPrefab.SetActive(true);
        }
        else
        {
            InstantiateProjectTileObjListAdd(cBase, p_EffectResInfo);
            FXEffectPlay(cBase, p_EffectResInfo);
        }
    }


    public void EffectPlayStart(CharacterBase cBase, string p_strValues)
    {
        float fLifeTime = 0;
        string[] astrValues = p_strValues.Split(',');
        GameObject csEffObj = Instantiate(EffectResourcesManager.instance.GetEffectFindAndLoadGameObject(astrValues[0])) as GameObject;

        ParticleSystem[] particles = csEffObj.GetComponentsInChildren<ParticleSystem>();

        if (particles.Length > 0)
        {
            fLifeTime = particles[0].duration;
        }
        csEffObj.SetActive(false);

        csEffObj.transform.parent = cBase.GetCharEffectBone(astrValues[1]);
        csEffObj.transform.localPosition = Vector3.zero;
        csEffObj.transform.localScale = Vector3.one;
        if (csEffObj.transform.parent != null)
        {
            csEffObj.transform.localEulerAngles = Vector3.zero;
        }

        csEffObj.SetActive(true);
        GameObject.DestroyObject(csEffObj, fLifeTime);

    }

    public void FX_Play(CharacterBase cBase)
    {
        if (cBase.m_currSkillIndex == 0)
        {
            return;
        }

        List<SkillDataInfo.EffectResInfo> effectResInfo = cBase.GetSkillEffectBodyResInfo();
        for (int i = 0; i < effectResInfo.Count; ++i)
        {
            if (cBase.nSkillEffectSequenceIndex == effectResInfo[i].unEffectKeyID)
            {
                FXEffectPlay(cBase, effectResInfo[i], cBase.GetCharEffectBone(effectResInfo[i].strEffectTargetPosition));
            }
        }
        cBase.nSkillEffectSequenceIndex++;
    }

    public void FX_PlayProjectile(CharacterBase cBase, int p_iHitIndex)
    {
        if (cBase.m_currSkillIndex == 0)
        {
            return;
        }

        if (cBase.m_currSkillIndex != cBase.m_UseSkillIndex)
        {
            UnityEngine.Debug.Log("FX_PlayProjectile ()XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX");
            UnityEngine.Debug.Log("FX_PlayProjectile (),,,, m_UseSkillIndex  ===============================   " + cBase.m_UseSkillIndex);
            UnityEngine.Debug.Log("FX_PlayProjectile (),,,, m_currSkillIndex ===============================    " + cBase.m_currSkillIndex);
            UnityEngine.Debug.Log("FX_PlayProjectile ()XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX");
            cBase.m_currSkillIndex = cBase.m_UseSkillIndex;
        }
        
        SkillDataInfo.SkillInfo tmpInfo = cBase.m_AttackInfos[cBase.m_currSkillIndex].skillinfo;
        
        if (tmpInfo.projectTileInfo.Count <= p_iHitIndex)
        {
            UnityEngine.Debug.Log("FX_PlayProjectile (),,,, tmpInfo.projectTileInfo.Count  ===============================   " + tmpInfo.projectTileInfo.Count);
            UnityEngine.Debug.Log("FX_PlayProjectile (),,,, p_iHitIndex ===============================    " + p_iHitIndex);
        }

        SkillDataInfo.ProjectTileInfo pTileInfo = tmpInfo.projectTileInfo[p_iHitIndex];

        cBase.m_SysAffect_Idx = p_iHitIndex;

        FXEffectPlay(cBase, pTileInfo);

        cBase.nSkillProjectileSequenceIndex++;
    }

    public void FX_BUFF(CharacterBase cBase, SkillDataInfo.EffectResInfo resInfo, float p_fLifeTime, bool p_bOverLap, bool ParticleDuration = false)
    {
        Vector3 tmp = new Vector3(0, 0, 0);

        resInfo.bOverLap = p_bOverLap;
        GuideEffectInfo tmpInfo = new GuideEffectInfo(tmp, tmp, 0, 0);

        if (resInfo.strEffectTargetPosition.Equals("Renderer"))
        {
            for (int i = 0; i < cBase.m_RendererParent.Count; ++i)
            {
                FXEffectPlay(cBase, resInfo, cBase.m_RendererParent[i], tmpInfo, p_fLifeTime, true, i);
            }
        }
        else
        {
            if (ParticleDuration)
            {
                FXEffectPlay(cBase, resInfo, cBase.GetCharEffectBone(resInfo.strEffectTargetPosition), tmpInfo, p_fLifeTime, false, -1, ParticleDuration);
            }
            else
            {
                FXEffectPlay(cBase, resInfo, cBase.GetCharEffectBone(resInfo.strEffectTargetPosition), tmpInfo, p_fLifeTime);
            }


        }
    }

    public void FX_BUFF(CharacterBase cBase, int p_iBuffEffectIndex, float p_fLifeTime, bool p_bOverLap)
    {
        SkillDataInfo.EffectResInfo effectResInfo = SkillDataManager.instance.GetEffectResInfoByEffID(p_iBuffEffectIndex);
        Vector3 tmp = new Vector3(0, 0, 0);
        effectResInfo.bOverLap = p_bOverLap;

        GuideEffectInfo tmpInfo = new GuideEffectInfo(tmp, tmp, 0, 0);
        FXEffectPlay(cBase, effectResInfo, cBase.GetCharEffectBone(effectResInfo.strEffectTargetPosition), tmpInfo, p_fLifeTime);
    }

    public void FX_PlayGuide(CharacterBase cBase, int p_iGuideEffectIndex, Vector3 p_Pos, Vector3 p_Scale, float p_fAngle, float p_fLifeTime, bool p_bOverLap)
    {
        SkillDataInfo.EffectResInfo effectResInfo = SkillDataManager.instance.GetEffectResInfoByEffID(p_iGuideEffectIndex); //CDataManager.GetEffectExcelDataInformation((uint)p_iGuideEffectIndex);
        if (effectResInfo == null)
            return;

        GuideEffectInfo info = new GuideEffectInfo(p_Pos, p_Scale, p_fAngle, 1);

        effectResInfo.bOverLap = p_bOverLap;

        FXEffectPlay(cBase, effectResInfo, cBase.GetCharEffectBone(effectResInfo.strEffectTargetPosition), info, p_fLifeTime);
    }

    public void SetEffSPAWN(CharacterBase cBase)
    {
        if (cBase == null)
            return;

        NpcAI npcai = (NpcAI)cBase.m_CharAi;

        if (npcai != null)
        {
            NpcInfo.NpcProp prop = npcai.GetNpcProp();

            if (prop != null)
            {
                cBase.m_EffectSkillManager.FX_BUFF(cBase, prop.SpawnEffResInfo, -1, true);
            }
        }
    }

    public void FX_Die(NpcInfo.NpcProp prop)
    {
        if (prop != null)
        {
            if (prop.Die_EffectObj != null)
            {
                prop.Die_EffectObj.SetActive(true);
            }
        }
    }

    public int GetPlayableEffectIndex(SkillDataInfo.EffectResInfo resInfo, bool p_bOverLap = false)
    {
        for (int i = 0; i < resInfo.m_EffObjList.Count; ++i)
        {
            if (resInfo.m_EffObjList[i].m_EffPrefab != null)
            {
                if (!resInfo.m_EffObjList[i].m_bIsPlaying)
                {
                    return i;
                }
            }
        }
        return -1;
    }

    public int GetPlayableEffectIndex(SkillDataInfo.ProjectTileInfo resInfo, bool p_bOverLap = false)
    {
        for (int i = 0; i < resInfo.pTileObjList.Count; ++i)
        {
            if (!resInfo.pTileObjList[i].m_bIsPlaying)
            {
                return i;
            }
        }
        return -1;
    }

    public int GetPlayableHitEffectIndex()
    {
        for (int i = 0; i < m_PlayingHitEffObjs.Count; ++i)
        {
            if (m_PlayingHitEffObjs[i].m_EffPrefab != null)
            {
                if (!m_PlayingHitEffObjs[i].m_bIsPlaying)
                {
                    return i;
                }
            }
        }
        return -1;
    }

    private void EffectPlayProcess()
    {
        if (m_PlayingEffObjs == null) return;

        if (m_PlayingEffObjs.Count > 0)
        {
            for (int i = m_PlayingEffObjs.Count - 1; i >= 0; --i)
            {
                if (m_PlayingEffObjs[i].m_EffPrefab != null)
                {
                    m_PlayingEffObjs[i].m_fPlayTime += Time.deltaTime;
                    if (m_PlayingEffObjs[i].m_EffPrefab.activeSelf && m_PlayingEffObjs[i].m_EffPS != null)
                    {
                        if (m_PlayingEffObjs[i].m_EffPS.isPaused)
                        {
                            if (m_CharacterBase.ksAnimation.m_fPauseTimer <= 0.0f)
                                m_PlayingEffObjs[i].m_EffPS.Play();
                        }
                        else
                        {
                            if (m_CharacterBase.ksAnimation.m_fPauseTimer > 0.0f && m_PlayingEffObjs[i].m_EffPS.isPaused == false)
                                m_PlayingEffObjs[i].m_EffPS.Pause(true);
                        }
                    }

                    if (m_PlayingEffObjs[i].m_fPlayTime >= m_PlayingEffObjs[i].m_fLifeTime && m_PlayingEffObjs[i].m_EffPrefab.activeSelf)
                    {
                        SetPlayingEffectFinish(i);
                    }
                    EffectFinshCheckProcess(i);
                }
            }
        }
    }
    private void EffectFinshCheckProcess(int index)
    {
        if (index <= m_PlayingEffObjs.Count - 1)
        {
            if (m_PlayingEffObjs[index].m_EffPrefab != null)// && !m_PlayingEffObjs[i].m_bLooping)
            {
                if (m_PlayingEffObjs[index].m_EffPrefab.activeSelf == false)
                {
                    if (m_PlayingEffObjs[index].m_fPlayTime > 0 && m_PlayingEffObjs[index].m_iNewCreate != 1)
                    {
                        SetPlayingEffectFinish(index);
                    }
                }
            }
        }
    }

    private void EffectHitPlayProcess()
    {
        if (m_PlayingHitEffObjs == null) return;
        if (m_PlayingHitEffObjs.Count > 0)
        {
            for (int i = m_PlayingHitEffObjs.Count - 1; i >= 0; --i)
            {
                if (m_PlayingHitEffObjs[i].m_EffPrefab != null)// && !m_PlayingEffObjs[i].m_bLooping)
                {
                    if (m_PlayingHitEffObjs[i].m_EffPrefab.activeSelf)
                    {
                        m_PlayingHitEffObjs[i].m_fPlayTime += Time.deltaTime;
                        if (m_PlayingHitEffObjs[i].m_EffPS != null)
                        {
                            if (m_PlayingHitEffObjs[i].m_EffPS.isPaused)
                            {
                                if (m_CharacterBase.ksAnimation.m_fPauseTimer <= 0.0f)
                                    m_PlayingHitEffObjs[i].m_EffPS.Play();
                            }
                            else
                            {
                                if (m_CharacterBase.ksAnimation.m_fPauseTimer > 0.0f && m_PlayingHitEffObjs[i].m_EffPS.isPaused == false)
                                    m_PlayingHitEffObjs[i].m_EffPS.Pause(true);
                            }
                        }

                        if (!m_PlayingHitEffObjs[i].m_bLooping)
                        {
                            //Debug.Log("####### play Time = " + m_PlayingEffObjs[i].m_fPlayTime + "/ life Time = " + m_PlayingEffObjs[i].m_fLifeTime);
                            if (m_PlayingHitEffObjs[i].m_fPlayTime >= m_PlayingHitEffObjs[i].m_fLifeTime)
                            {
                                SetPlayingHitEffectFinish(i);
                            }
                        }
                        else
                        {
                            if (m_PlayingHitEffObjs[i].m_bStopPlay)
                            {
                                SetPlayingHitEffectFinish(i);
                            }
                        }
                    }
                    else
                    {

                        if (m_PlayingHitEffObjs[i].m_fPlayTime > 0)
                        {
                            SetPlayingHitEffectFinish(i);
                        }
                    }

                }
            }
        }
    }

    public void PauseAllPlayingEffect()
    {
        if (m_PlayingEffObjs.Count > 0)
        {
            for (int i = 0; i < m_PlayingEffObjs.Count; ++i)
            {
                if (m_PlayingEffObjs[i].m_EffPrefab != null)
                {
                    if (m_PlayingEffObjs[i].m_EffPrefab.activeSelf && m_PlayingEffObjs[i].m_EffPS != null)
                    {
                        m_PlayingEffObjs[i].m_EffPS.Stop(true);
                    }
                }
            }
        }
    }

    public void StopPlayingEffect(int buffEffIdx)
    {
        SkillDataInfo.EffectResInfo effectResInfo = SkillDataManager.instance.GetEffectResInfoByEffID(buffEffIdx);

        if (effectResInfo != null)
        {
            for (int i = 0; i < m_PlayingEffObjs.Count; ++i)
            {
                if (m_PlayingEffObjs[i].m_EffName.Equals(effectResInfo.strEffectName))
                {
                    if (m_PlayingEffObjs[i].m_bIsPlaying)
                    {
                        if (!m_PlayingEffObjs[i].m_bLooping)
                        {
                            m_PlayingEffObjs[i].m_fPlayTime = 9999;
                        }
                        else
                        {
                            m_PlayingEffObjs[i].m_bStopPlay = true;
                        }
                    }
                }
            }
        }
    }

    public bool IsPlayingSameEffect(int effIdx)
    {
        SkillDataInfo.EffectResInfo effectResInfo = SkillDataManager.instance.GetEffectResInfoByEffID(effIdx);

        if (effectResInfo != null)
        {
            for (int i = 0; i < m_PlayingEffObjs.Count; ++i)
            {
                if (m_PlayingEffObjs[i].m_EffName.Equals(effectResInfo.strEffectName))
                {
                    if (m_PlayingEffObjs[i].m_bIsPlaying)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    public GameObject gobjCharacter
    {
        get
        {
            return m_gobjCharacter;
        }
        set
        {
            m_gobjCharacter = value;
        }
    }

    private void OnDestroy()
    {
        if (m_PlayingEffObjs != null)
        {
            if (m_PlayingEffObjs.Count > 0)
            {
                for (int i = 0; i < m_PlayingEffObjs.Count; ++i)
                {
                    m_PlayingEffObjs[i] = null;
                }
                m_PlayingEffObjs.Clear();
            }
        }
    }


    private void SetPlayingEffectFinish(int idx)
    {

        if (m_PlayingEffObjs[idx].m_EffPrefab.CompareTag("BuffMat"))
        {
            m_PlayingEffObjs[idx].m_EffPrefab.GetComponent<AddMaterialOnHit>().RemoveMaterials();
        }

        if (m_PlayingEffObjs[idx].m_iNewCreate == 1)
        {
            m_PlayingEffObjs[idx].m_EffPrefab.SetActive(false);

            m_PlayingEffObjs[idx].m_bIsPlaying = false;
            m_PlayingEffObjs[idx].m_bStopPlay = false;
            m_PlayingEffObjs[idx].m_fPlayTime = 0;
        }
        else
        {
            m_PlayingEffObjs[idx].m_EffPrefab.transform.parent = null;
            m_PlayingEffObjs[idx].m_EffPrefab.SetActive(false);

            m_PlayingEffObjs[idx].m_bIsPlaying = false;
            m_PlayingEffObjs[idx].m_bStopPlay = false;
            m_PlayingEffObjs[idx].m_fPlayTime = 0;
            m_PlayingEffObjs.RemoveAt(idx);
        }

    }

    private void SetPlayingHitEffectFinish(int idx)
    {
        m_PlayingHitEffObjs[idx].m_EffPrefab.transform.parent = null;
        m_PlayingHitEffObjs[idx].m_EffPrefab.SetActive(false);

        m_PlayingHitEffObjs[idx].m_bIsPlaying = false;
        m_PlayingHitEffObjs[idx].m_bStopPlay = false;
        m_PlayingHitEffObjs[idx].m_fPlayTime = 0;
    }

}
