#define _DEBUG_MSG_ENABLED
#define _DEBUG_WARN_ENABLED
#define _DEBUG_ERROR_ENABLED

#region Global project preprocessor directives.

#if _DEBUG_MSG_ALL_DISABLED
#undef _DEBUG_MSG_ENABLED
#endif

#if _DEBUG_WARN_ALL_DISABLED
#undef _DEBUG_WARN_ENABLED
#endif

#if _DEBUG_ERROR_ALL_DISABLED
#undef _DEBUG_ERROR_ENABLED
#endif

#endregion Global project preprocessor directives.



using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;



public enum AVATAR_STATUS
{
    ID,
    LV,
    LV_OLD,
    EXP,
    INTIMACY_LV,
    INTIMACY_LV_OLD,
    INTIMACY_EXP,
    INTIMACY_EXP_MAX,
    LV_REWARD,
    CONDITION,
    CONDITION_REMAIN_TS,
    CONDITION_MAX,
    STATUS,
    STAT,
    TOUCH_REWARD_FIX_CNT,
    TOUCH_REWARD_FIX_REMAIN_MTS,
    TOUCH_REWARD_RANDOM_INTIMACY_CNT,
    TOUCH_REWARD_RANDOM_ITEM_CNT,
    TOUCH_REWARD_RANDOM_REMAIN_MTS, 
    DECORATION_POINT,

    //SENSE,
}

public class ClosetData
{
    public HouseAPI.HouseClosetResponseData closetData;
    public CStyleItemAttr itemAttr;
}

public class MemberStats
{
    public long stat_lv;
    public long stat_lv_old;
    public long stat_exp;
    public long stat_exp_old;
    public long stat_exp_max;
    public long stat_exp_max_old;

    public MemberStats(long lv, long lv_old, long exp, long exp_old, long expmax, long expmax_old)
    {
        stat_lv = lv;
        stat_lv_old = lv_old;
        stat_exp = exp;
        stat_exp_old = exp_old;
        stat_exp_max = expmax;
        stat_exp_max_old = expmax_old;

    }
}

// 멤버(캐릭터)의 데이터셋
public class CAvatar
{
    private AVATAR_TYPE AvatarType { get; set; }
    public int Lv { get; set; }
    public int Lv_old { get; set; }
    public long Exp { get; set; }
    private string _AvatarID { get; set; }
    public int intimacy_lv { get; set; }
    public long intimacy_exp { get; set; }
    public long intimacy_old_exp { get; set; }
    public long intimacy_exp_max { get; set; }
    public long condition { get; set; }
    public long condition_remain_ts { get; set; }
    public long condition_max { get; set; }
    public AVATAR_ACTIVITY_STATE status { get; set; } //
    //public long[] stat; //스탯
    public MemberStats[] stats; //스탯
    public List<long> lv_reward { get; set; }
    public long intimacy_lv_old { get; set; }
    public long touch_fix_reward_cnt { get; set; }
    public long touch_random_reward_intimacy_cnt { get; set; }
    public long touch_random_reward_item_cnt { get; set; }
    public long touch_fix_reward_remain_mts { get; set; }
    public float TouchEvtFixReward_CurTime { get; set; }
    public long touch_random_reward_remain_mts { get; set; }
    public long decoration_point { get; set; }


    /// <summary>
    public ReactiveProperty<int> lv_update;
    public ReactiveProperty<long> condition_update;

    public ReactiveProperty<int> intimacy_lv_update;
    public ReactiveProperty<long> intimacy_exp_update;
    /// </summary>

    public GameObject AvatarObj;
    //About DynamicBone
    public Dictionary<string, DynamicBoneCollider> DynamicBoneColDic;
    public Dictionary<string, DynamicBone> BaseBodyDBDic;  //Every DynamicBone in BaseBody. active is false
    public Dictionary<STYLE_ITEM_TYPE, List<string>> BeforeDBNameList;

    SingleAssignmentDisposable corDisposer;    
    public int Frame;

    public (bool isLevelUp, int oldLV) levelUpCheckTuple { get; private set; } = (false, -1);
    public void FinishlevelUpCheck () => levelUpCheckTuple = (false, Lv);
    public CAvatar ()
    {
        //stat = new long[(int)STAT_TYPE.TOTAL_STAT];
        stats = new MemberStats[(int)STAT_TYPE.TOTAL_STAT];

        condition_update = new ReactiveProperty<long>();
		lv_update = new ReactiveProperty<int>();

        intimacy_exp_update = new ReactiveProperty<long>();
        intimacy_lv_update = new ReactiveProperty<int>();

    }


    public void SetAvatarStatus(AVATAR_STATUS status, object setValue)
    {
        switch(status)
        {
            case AVATAR_STATUS.ID:
                {
                    var value = (string)Convert.ChangeType(setValue, typeof(string));
                    if (value != "")
                        _AvatarID = value;
                }
                break;

            case AVATAR_STATUS.LV:
                {
                    var value = (int)Convert.ChangeType (setValue, typeof (int));
                    if (value > 0)
                    {
                        Lv = value;
                        lv_update.Value = Lv;
                    }
                }
                break;

            case AVATAR_STATUS.LV_OLD:
                {
                    var value = (int)Convert.ChangeType (setValue, typeof (int));
                    if (value > 0)
                    {
                        Lv_old = value;
                        if (Lv > 0 &&  Lv != Lv_old)
                        {
                            levelUpCheckTuple = (true, Lv_old);
                        }
                    }
                }
                break;

            case AVATAR_STATUS.EXP:
                {
                    var value = (long)Convert.ChangeType(setValue, typeof(long));
                    if (value > 0)
                        Exp = value;
                }
                break;

            case AVATAR_STATUS.INTIMACY_LV:
                {
                    var value = (int)Convert.ChangeType(setValue, typeof(int));
                    if (value >= 0)
                    {
                        intimacy_lv = value;
                        intimacy_lv_update.Value = intimacy_lv;
                    }
                    else
                    {
                        //intimacy_lv = 1;
                        //Debug.LogError(string.Format("avatar info intimacy lv:[{0}] Error", value));
                    }
                }
                break;
            case AVATAR_STATUS.INTIMACY_EXP:
                {
                    var value = (long)Convert.ChangeType(setValue, typeof(long));
                    if (value >= 0)
                    {
                        intimacy_old_exp = intimacy_exp;
                        intimacy_exp = value;
                        intimacy_exp_update.Value = intimacy_exp;
                    }
                    else
                    {
                        intimacy_exp = 0;
                        //Debug.LogError(string.Format("avatar info intimacy exp:[{0}] Error", value));
                    }
                }
                break;
            case AVATAR_STATUS.INTIMACY_EXP_MAX:
                {
                    var value = (long)Convert.ChangeType(setValue, typeof(long));
                    if (value >= 0)
                        intimacy_exp_max = value;
                    else
                    {
                        intimacy_exp_max = 0;
                        //Debug.LogError(string.Format("avatar info intimacy exp max:[{0}] Error", value));
                    }
                }
                break;
            case AVATAR_STATUS.CONDITION:
                {
                    var value = (long)Convert.ChangeType(setValue, typeof(long));
                    if (value >= 0)
                    {
                        condition = value;
                        condition_update.Value = condition;
                    }
                    else
                    {
                        condition = 0;
                        //Debug.LogError(string.Format("avatar info condition:[{0}] Error", value));
                    }

                    //Debug.Log(string.Format($"avatar[{memberType}] info condition:[{value}] "));

                }
                break;
            case AVATAR_STATUS.CONDITION_REMAIN_TS:
                {
                    var value = (long)Convert.ChangeType(setValue, typeof(long));
                    if (value >= 0)
                        condition_remain_ts = value;
                    else
                    {
                        condition_remain_ts = 0;
                        //Debug.LogError(string.Format("avatar info condition_remain_ts:[{0}] Error", value));
                    }

                    //Debug.Log(string.Format($"member[{memberType}] info condition_remain_ts:[{value}] "));


                }
                break;
            case AVATAR_STATUS.CONDITION_MAX:
                {
                    var value = (long)Convert.ChangeType(setValue, typeof(long));
                    if (value >= 0)
                        condition_max = value;
                    else
                    {
                        condition_max = 0;
                        //Debug.LogError(string.Format("avatar info condition_max:[{0}] Error", value));
                    }

                    //Debug.Log(string.Format($"member[{memberType}] info condition_max:[{value}] "));

                }
                break;
            case AVATAR_STATUS.STATUS:
                {
                    var value = (int)Convert.ChangeType(setValue, typeof(int));
                    if (value >= 0)
                        this.status = (AVATAR_ACTIVITY_STATE)value;
                    else
                    {
                        this.status = AVATAR_ACTIVITY_STATE.REST;
                        //Debug.LogError(string.Format("avatar info status:[{0}] Error", value));
                    }
                }
                break;
            case AVATAR_STATUS.LV_REWARD:
                {
                    if (setValue != null)
                    {
                        var value = (List<long>)Convert.ChangeType (setValue, typeof (List<long>));
                        if (value.Count > 0)
                        {
                            this.lv_reward = value;
                            //for(int i=0; i<value.Count; ++i)
                            //{
                            //    this.lv_reward.Add(value[i]);
                            //}
                        }
                    }
                    //else
                    //{
                    //    Debug.Log("avatar info lv_reward count is 0");
                    //}
                }
                break;
            case AVATAR_STATUS.INTIMACY_LV_OLD:
                {
                    var value = (long)Convert.ChangeType(setValue, typeof(long));
                    if (value > 0)
                    {
                        this.intimacy_lv_old = value;
                    }
                    else
                    {
                        this.intimacy_lv_old = 0;
                        //Debug.Log("avatar info intimacy_lv_old count is 0");
                    }
                }
                break;
            case AVATAR_STATUS.TOUCH_REWARD_FIX_CNT:
                {
                    var value = (long)Convert.ChangeType(setValue, typeof(long));
                    if (value > 0)
                    {
                        this.touch_fix_reward_cnt = value;
                    }
                    else
                    {
                        this.touch_fix_reward_cnt = 0;
                        //Debug.Log("avatar info touch_fix_reward_cnt is 0");
                    }
                }
                break;
            case AVATAR_STATUS.TOUCH_REWARD_RANDOM_INTIMACY_CNT:
                {
                    var value = (long)Convert.ChangeType(setValue, typeof(long));
                    if (value > 0)
                    {
                        this.touch_random_reward_intimacy_cnt = value;
                    }
                    else
                    {
                        this.touch_random_reward_intimacy_cnt = 0;
                        //Debug.Log("avatar info touch_random_reward_intimacy_cnt is 0");
                    }
                }
                break;
            case AVATAR_STATUS.TOUCH_REWARD_RANDOM_ITEM_CNT:
                {
                    var value = (long)Convert.ChangeType(setValue, typeof(long));
                    if (value > 0)
                    {
                        this.touch_random_reward_item_cnt = value;
                    }
                    else
                    {
                        this.touch_random_reward_item_cnt = 0;
                        //Debug.Log("avatar info touch_random_reward_item_cnt is 0");
                    }
                }
                break;
            case AVATAR_STATUS.TOUCH_REWARD_FIX_REMAIN_MTS:
                {
                    var value = (long)Convert.ChangeType(setValue, typeof(long));

                    //if (GetAvatarType() == AVATAR_TYPE.AVATAR_JISOO)
                    //{
                    //    value = 100000;
                    //}
                    if (value > 0)
                    {
                        //    string streamKey = MainManager.TOUCH_FIX_REWARD_TIMEKEY + "_" + _avatar.GetAvatarType();
                        //    GlobalTimer.Instance.RemoveTimeStream(streamKey);
                        this.touch_fix_reward_remain_mts = value;
                    }
                    else
                    {
                        this.touch_fix_reward_remain_mts = 0;
                        //Debug.Log("avatar info touch_fix_reward_remain_mts is 0");
                    }
                }
                break;
            case AVATAR_STATUS.TOUCH_REWARD_RANDOM_REMAIN_MTS:
                {
                    var value = (long)Convert.ChangeType(setValue, typeof(long));
                    if (value > 0)
                    {
                        this.touch_random_reward_remain_mts = value;
                    }
                    else
                    {
                        this.touch_random_reward_remain_mts = 0;
                        //Debug.Log("avatar info touch_random_reward_remain_mts is 0");
                    }
                }
                break;
            case AVATAR_STATUS.DECORATION_POINT:
                {
                    var value = (long)Convert.ChangeType(setValue, typeof(long));
                    this.decoration_point = (value > 0) ? value : 0;
                }
                break;
        }
    }

    public void SetAvatarStat(List<MemberStats> statdatas)
    {   
        if(statdatas.Count !=  (int)STAT_TYPE.TOTAL_STAT)
        {
            Debug.LogError(string.Format($"avatar info statLv count:[{statdatas.Count}] Error"));
            return;
        }

        for (int i = 0; i < (int)STAT_TYPE.TOTAL_STAT; i++)
        {
            this.stats[i] = statdatas[i];
        }
    }


    public void SetavatarInfo(AVATAR_TYPE avatarType, avatar_info info)
    {
        AvatarType = avatarType;
        //SetAvatarStatus(AVATAR_STATUS.ID, info.mid);
        SetAvatarStatus(AVATAR_STATUS.ID, (byte)avatarType);
        SetAvatarStatus(AVATAR_STATUS.INTIMACY_LV, info.intimacy_lv);
        SetAvatarStatus(AVATAR_STATUS.INTIMACY_EXP, info.intimacy_exp);
        SetAvatarStatus(AVATAR_STATUS.INTIMACY_EXP_MAX, info.intimacy_exp_max);
        SetAvatarStatus(AVATAR_STATUS.LV_REWARD, info.lv_reward);
        SetAvatarStatus(AVATAR_STATUS.INTIMACY_LV_OLD, info.intimacy_lv_old);
        SetAvatarStatus(AVATAR_STATUS.TOUCH_REWARD_FIX_CNT, info.touch_fix_reward_cnt);
        SetAvatarStatus(AVATAR_STATUS.TOUCH_REWARD_RANDOM_INTIMACY_CNT, info.touch_random_reward_intimacy_cnt);
        SetAvatarStatus(AVATAR_STATUS.TOUCH_REWARD_RANDOM_ITEM_CNT, info.touch_random_reward_item_cnt);
        SetAvatarStatus(AVATAR_STATUS.TOUCH_REWARD_FIX_REMAIN_MTS, info.touch_fix_reward_remain_mts);
        SetAvatarStatus(AVATAR_STATUS.TOUCH_REWARD_RANDOM_REMAIN_MTS, info.touch_random_reward_remain_mts);
        SetAvatarStatus(AVATAR_STATUS.STATUS, info.status);
    }

    public void SetAvatarObject()
    {
        if(StaticAvatarManager.StaticAvatar == null)
        {
            CDebug.LogError("    %%%%  AvatarObj is null");
            return;
        }


        if (StaticAvatarManager.GetAvatarObject(AvatarType) != null)
        {
            CDebug.LogError("    %%%% "+ AvatarType + " has been loaded already");
            return;
        }

        //CDebug.LogError("    %%%% SetAvatarObject() avatarType = " + AvatarType + "  Load");

        GameObject avatarObj = AvatarManager.LoadAvatarObject( AvatarType, StaticAvatarManager.AvatarObjContainer );

        StaticAvatarManager.SetStaticAvatar(AvatarType, avatarObj );
    }

   public AVATAR_TYPE GetAvatarType()
    {
        return AvatarType;
    }

    public void SetDynamicBone(GameObject bodyObj)
    {
        SetDynamicColliderDic(bodyObj);
        SetDynamicBoneDic(bodyObj);
    }

    private void SetDynamicColliderDic(GameObject bodyObj)
    {        
        if (DynamicBoneColDic == null)
        {
            DynamicBoneColDic = new Dictionary<string, DynamicBoneCollider>();
        }

        DynamicBoneColDic.Clear();

        if (DynamicBoneColDic.Count == 0)
        {
            DynamicBoneCollider[] _col = bodyObj.GetComponentsInChildren<DynamicBoneCollider>();
            for (int i = 0; i < _col.Length; ++i)
            {
                DynamicBoneColDic.Add(_col[i].name, _col[i]);
            }
        }
    }

    public DynamicBoneCollider GetDynamicCollider(string name)
    {
        if (DynamicBoneColDic != null)
        {
            if (DynamicBoneColDic.ContainsKey(name))
            {
                return DynamicBoneColDic[name];
            }
        }

        return null;
    }

    private void SetDynamicBoneDic(GameObject bodyObj)
    {
        if(BaseBodyDBDic == null)
        {
            BaseBodyDBDic = new Dictionary<string, DynamicBone>();
        }

        if(BeforeDBNameList == null)
        {
            BeforeDBNameList = new Dictionary<STYLE_ITEM_TYPE, List<string>>();
        }

        //if(BaseBodyDBDic.Count == 0)
        {
            DynamicBone[] _db = bodyObj.GetComponentsInChildren<DynamicBone>();
            if(_db.Length == 0)
            {
                CDebug.LogError(bodyObj + "'s DynamicBone count is 0");
            }

            for (int i = 0; i < _db.Length; ++i)
            {
                _db[i].enabled = false; 
                _db[i].IsActive = false;
                _db[i].obj_Name = "none";
                if (BaseBodyDBDic.ContainsKey(_db[i].name) == false)
                {
                    BaseBodyDBDic.Add(_db[i].name, _db[i]);
                }
                else
                {
                    BaseBodyDBDic[_db[i].name] = _db[i];
                }
            }
        }
    }


    public void SetBaseBodyDynamicBoneData(DynamicBone originDB, Transform boneObj)
    {
        if (BaseBodyDBDic[boneObj.name] == null)
        {
            return;
        }

        BaseBodyDBDic[boneObj.name].m_Root = boneObj;
        BaseBodyDBDic[boneObj.name].m_UpdateRate = originDB.m_UpdateRate;
        BaseBodyDBDic[boneObj.name].obj_Name = boneObj.gameObject.name;
        BaseBodyDBDic[boneObj.name].m_UpdateMode = originDB.m_UpdateMode;
        BaseBodyDBDic[boneObj.name].m_Damping = originDB.m_Damping;
        BaseBodyDBDic[boneObj.name].m_DampingDistrib = originDB.m_DampingDistrib;
        BaseBodyDBDic[boneObj.name].m_Elasticity = originDB.m_Elasticity;
        BaseBodyDBDic[boneObj.name].m_ElasticityDistrib = originDB.m_ElasticityDistrib;
        BaseBodyDBDic[boneObj.name].m_Stiffness = originDB.m_Stiffness;
        BaseBodyDBDic[boneObj.name].m_StiffnessDistrib = originDB.m_StiffnessDistrib;
        BaseBodyDBDic[boneObj.name].m_Inert = originDB.m_Inert;
        BaseBodyDBDic[boneObj.name].m_InertDistrib = originDB.m_InertDistrib;
        BaseBodyDBDic[boneObj.name].m_Radius = originDB.m_Radius;
        BaseBodyDBDic[boneObj.name].m_RadiusDistrib = originDB.m_RadiusDistrib;
        BaseBodyDBDic[boneObj.name].m_EndLength = originDB.m_EndLength;
        BaseBodyDBDic[boneObj.name].m_EndOffset = originDB.m_EndOffset;
        BaseBodyDBDic[boneObj.name].m_Gravity = originDB.m_Gravity;
        BaseBodyDBDic[boneObj.name].m_Force = originDB.m_Force;
        BaseBodyDBDic[boneObj.name].m_Friction = originDB.m_Friction;
        BaseBodyDBDic[boneObj.name].m_FrictionDistrib = originDB.m_FrictionDistrib;
        BaseBodyDBDic[boneObj.name].m_Colliders = new List<DynamicBoneColliderBase>();
        for (int i=0; i<originDB.m_Colliders.Count; ++i)
        {
            if (originDB.m_Colliders[i] == null)
                continue;
            DynamicBoneCollider originDO = GetDynamicCollider(originDB.m_Colliders[i].name);

            BaseBodyDBDic[boneObj.name].m_Colliders.Add(originDO);
        }
        BaseBodyDBDic[boneObj.name].IsActive = true;
        BaseBodyDBDic[boneObj.name].enabled = true;
    }

    public Dictionary<string, DynamicBone> GetDynamicBoneDic()
    {
        return BaseBodyDBDic;
    }

    //===============================================//

    // 멤버의 컨디션 레벨 얻기 (1 ~ 5)
    public int GetConditionLevel()
    {
        return CalculateConditionLevel (Lv, condition, condition_max);
    }

    public AVATAR_CONDITION GetConditionStatus()
    {
        int conditionLv = GetConditionLevel();

        //if (conditionLv >= 1 && conditionLv <= 2)
        //    
        switch(conditionLv)
        {
            case 1:
            case 2:
                return AVATAR_CONDITION.BAD;
            case 5:
                return AVATAR_CONDITION.GOOD;
        }

        //3, 4 level is normal
        return AVATAR_CONDITION.NORMAL;
    }

    // 컨디션 레벨 계산 (1 ~ 5)
    public static int CalculateConditionLevel (int avatarLv, float conditionValue, float conditionMaxValue)
    {
        int classMin = 1;
        int classMax = 5;

        var conditionStepArr = CAvatarInfoDataManager.GetConditionClassData (avatarLv);
        if (conditionStepArr == null)
        {
            CDebug.LogError ($"avatar level {avatarLv} is not valid.");
            return classMin;
        }
        ConditionClassData conditonStep = null;
        foreach (var step in conditionStepArr)
        {
            if (step.Class_Value_Type == 1)
            {
                // 절대값 계산
                if (step.Class_Value <= (conditionValue))
                {
                    conditonStep = step;
                    break;
                }
            }
            else if (step.Class_Value_Type == 2)
            {
                // 백분율 계산 (기획서상에는 만분율이라고 적혀있긴 함)
                if(step.Class_Value <= (Mathf.InverseLerp (0, conditionMaxValue, conditionValue) * 100f))
                {
                    conditonStep = step;
                    break;
                }
            }
        }

        // 단계를 찾지 못하면 최저단계를 반환
        return Mathf.Clamp(conditonStep?.Class ?? classMin, classMin, classMax);
    }


    private void InitcorDisposer()
    {
        if(corDisposer == null)
        {
            corDisposer = new SingleAssignmentDisposable();
        }
        else
        {
            corDisposer.Dispose();
            corDisposer = new SingleAssignmentDisposable();
        }
    }


    public void StartControlDynamicBoneWeightCorroutine()
    {
        InitcorDisposer();
        corDisposer.Disposable = SetDBWeightInOrder().Subscribe();
    }

    public void StopControlDynamicBoneWeightCorroutine()
    {
        Frame = 0;
        if (corDisposer != null)
        {
            corDisposer.Dispose();
        }
        SetDynamicBoneWeight(0.1f);
    }

    private IObservable<Unit> SetDBWeightInOrder()
    {
        return Observable.FromCoroutine(observer => ControlAvatarDynamicBoneWeight());
    }

    IEnumerator ControlAvatarDynamicBoneWeight()
    {
        //frame비교 숫자 1, 11, 7, 10등등은 자연스럼을 추구하고자 어쩔수 없이 하드코딩
        int WEIGHTMAX = 1;
        float WEIGHTSTEP = 0.1f;
        int MAXFRAME = 11;

        while (Frame < MAXFRAME)
        {
            if (Frame == 1)
            {
                if (AvatarType >= AVATAR_TYPE.AVATAR_JISOO_UI)
                {
                    AvatarObj.SetActive(true);
                }
            }
            else if (Frame == 7)
            {
                if (AvatarType >= AVATAR_TYPE.AVATAR_JISOO_UI)
                {
                    AvatarManager.ChangeLayersRecursively(AvatarObj.transform, CDefines.AVATAR_UI_OBJECT_LAYER_NAME);
                }
                else
                {
                    AvatarObj.SetActive(true);
                }
            }
            else if (Frame == (MAXFRAME - 1))
            {
                float tempvalue = 0.0f;

                while (tempvalue < WEIGHTMAX)
                {
                    yield return new WaitForFixedUpdate();
                    tempvalue += WEIGHTSTEP;
                    SetDynamicBoneWeight(tempvalue);
                }
            }
            yield return new WaitForFixedUpdate();
            ++Frame;
        }
    }

    private void SetDynamicBoneWeight(float weight)	//다이나믹본 웨이트값 조절
    {
        if (AvatarObj != null)
        {
            foreach (DynamicBone db in BaseBodyDBDic.Values)
            {
                db.SetWeight(weight);
            }
        }
    }

    public void SetBeforeDBNameDic(STYLE_ITEM_TYPE partType, string name)
    {
        if (BeforeDBNameList.ContainsKey( partType ) == false)
        {
            BeforeDBNameList.Add( partType, new List<string>() );
            BeforeDBNameList[partType].Add( name );
        }
        else
        {
            BeforeDBNameList[partType].Add( name );
        }
    }

    public void SetHideBeforeBaseDynamicBone()
    {
        foreach (List<string> names in BeforeDBNameList.Values)
        {
            foreach (string name in names)
            {
                if (BaseBodyDBDic.ContainsKey( name ))
                {
                    if (BaseBodyDBDic[name] == null) continue;
                    BaseBodyDBDic[name].IsActive = false;
                    BaseBodyDBDic[name].obj_Name = "none";
                    BaseBodyDBDic[name].enabled = false;
                }
            }
                
            names.Clear();
        }
        BeforeDBNameList.Clear();
    }



    public void Release()
    {
        //return;
        if (DynamicBoneColDic != null)
        {
            DynamicBoneColDic.Clear();
            DynamicBoneColDic = null;
        }

        if(BaseBodyDBDic != null)
        {
            BaseBodyDBDic.Clear();
            BaseBodyDBDic = null;
        }
    }
}

public class AvatarData
{
    public long ID { get; set; }
    public AVATAR_TYPE AvatarType { get; set; }
    public string resID_Head { get; set; }
    public string resID_Top { get; set; }
    public string resID_Bottom { get; set; }
    public string resID_Suit { get; set; }
    public string resID_Socks { get; set; }
    public string resID_Shoes { get; set; }
    public string resID_AccHead { get; set; }
    public string resID_AccEye { get; set; }
    public string resID_AccMouth { get; set; }
    public string resID_AccEaring { get; set; }
    public string resID_AccNeck { get; set; }
    public string resID_AccHand { get; set; }
    public string resID_AccWrist { get; set; }
    public string resID_AccNail { get; set; }
}

public class AvatarLevelData
{
    public int ID { get; set; }
    public byte Level { get; set; }
    public long Exp { get; set; }
    public long Condition { get; set; }
    public long Condition_Recovery { get; set; }
    public long Conditino_RecoveryTime { get; set; }
}

public class CondtionData
{
    public int ID { get; set; }
    public long LvGroup { get; set; }
    public byte Class { get; set; }
    public byte ClassValueType { get; set; }
    public long ClassValue { get; set; }
}
