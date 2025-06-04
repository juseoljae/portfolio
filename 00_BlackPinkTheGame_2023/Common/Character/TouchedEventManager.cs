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
using UnityEngine;
using UniRx;
using System.Linq;

public class TouchedEventManager : Singleton<TouchedEventManager>
{
    //Touch
    public static Dictionary<AVATAR_TYPE, TouchedEventInfo> AvatarTouchedEvtInfo;
    public static Dictionary<(AVATAR_TYPE, TOUCHEVENT_SCENE_TYPE), GameObject> AvatarTouchecEvt_UIObj;
    public static Dictionary<AVATAR_TYPE, RewardInfo> AvatarTouchEvtRewardInfo;
    public static Dictionary<AVATAR_TYPE, TouchOutputInfo> AvatarCurrentTouchOutputInfo;
    public static float AI_Timer;
    public static bool bPauseAI;
    private const int TOUCHEVENT_TIME = 3;


    public static void InitTouchEventDic()
    {
        if (AvatarTouchedEvtInfo == null) AvatarTouchedEvtInfo = new Dictionary<AVATAR_TYPE, TouchedEventInfo>();
        if (AvatarTouchecEvt_UIObj == null) AvatarTouchecEvt_UIObj = new Dictionary<(AVATAR_TYPE, TOUCHEVENT_SCENE_TYPE), GameObject>();
        if (AvatarTouchEvtRewardInfo == null) AvatarTouchEvtRewardInfo = new Dictionary<AVATAR_TYPE, RewardInfo>();

        if (AvatarCurrentTouchOutputInfo == null) AvatarCurrentTouchOutputInfo = new Dictionary<AVATAR_TYPE, TouchOutputInfo>();
    }

    #region TOUCH_EVENT
    /// <summary>
    /// [deprecated] not used _intimacyLv, _conditionLv
    /// </summary>
    /// <param name="aType"></param>
    /// <param name="sceneType"></param>
    /// <param name="avatarObj"></param>
    /// <param name="charCntlr"></param>
    public static void TouchAvatar(AVATAR_TYPE aType, TOUCHEVENT_SCENE_TYPE sceneType, GameObject avatarObj, CCharacterController_Management charCntlr)
    {
        CAvatar _avatar = CPlayer.GetAvatar(aType);
        int _intimacyLv = _avatar.intimacy_lv;
        int _conditionLv = _avatar.GetConditionLevel();
        bool isTraining = false;
        if (_avatar.status > AVATAR_ACTIVITY_STATE.REST 
            && _avatar.status <= AVATAR_ACTIVITY_STATE.TRAINING_ATTRACTIVENESS)
        {
            isTraining = true;
        }

        TouchOutputInfo _info = CCharacterTouchEventDataManager.Instance.GetTouchOutputInfo(aType, sceneType, isTraining, _intimacyLv, _conditionLv);
        if (_info == null)
        {
            CDebug.Log(string.Format($"AvatarMgr.TouchAvatar() TouchOutputInfo is null. avatar = {aType}, intimacy Lv = {_intimacyLv}, condition Lv = {_conditionLv}"));
            return;
        }

        if (AvatarCurrentTouchOutputInfo.ContainsKey(aType) == false)
        {
            AvatarCurrentTouchOutputInfo.Add(aType, _info);
        }

        if (AvatarTouchedEvtInfo.ContainsKey(aType) == false)
        {
            AvatarTouchedEvtInfo.Add(aType, new TouchedEventInfo());
        }

        SetTouchEvent_OutputInfo(aType, _info, avatarObj, sceneType, isTraining, _intimacyLv, _conditionLv, charCntlr);
    }

    public static void TouchAvatar(AVATAR_TYPE aType, TOUCHEVENT_SCENE_TYPE sceneType, GameObject avatarObj, Action<float> actClipLength) /*CCharacterController_Management charCntlr = null)*/
    {
        CCharacterController_Management charCntlr = null; //매니지먼트 나중에..

        CAvatar _avatar = CPlayer.GetAvatar(aType);

        bool isTraining = false;
        if (_avatar.status > AVATAR_ACTIVITY_STATE.REST
            && _avatar.status <= AVATAR_ACTIVITY_STATE.TRAINING_ATTRACTIVENESS)
        {
            isTraining = true;
        }

        TouchOutputInfo _info = CCharacterTouchEventDataManager.Instance.GetTouchOutputInfo(aType, sceneType, isTraining);
        if (_info == null)
        {
            CDebug.Log(string.Format($"AvatarMgr.TouchAvatar() TouchOutputInfo is null. avatar = {aType}"));
            return;
        }

        if (AvatarCurrentTouchOutputInfo.ContainsKey(aType) == false)
        {
            AvatarCurrentTouchOutputInfo.Add(aType, _info);
        }

        if (AvatarTouchedEvtInfo.ContainsKey(aType) == false)
        {
            AvatarTouchedEvtInfo.Add(aType, new TouchedEventInfo());
        }

        SetTouchEvent_OutputInfo(aType, _info, avatarObj, sceneType, isTraining, charCntlr, actClipLength);
    }

    /// <summary>
    /// [deprecated] not used _intimacyLv & _conditionLv
    /// </summary>
    /// <param name="aType"></param>
    /// <param name="info"></param>
    /// <param name="avatarObj"></param>
    /// <param name="sceneType"></param>
    /// <param name="isTraining"></param>
    /// <param name="_intimacyLv"></param>
    /// <param name="_conditionLv"></param>
    /// <param name="charCntlr"></param>
    public static void SetTouchEvent_OutputInfo(AVATAR_TYPE aType, TouchOutputInfo info, GameObject avatarObj, TOUCHEVENT_SCENE_TYPE sceneType, bool isTraining, int _intimacyLv, int _conditionLv, CCharacterController_Management charCntlr = null)
    {
        CAvatar _avatar = CPlayer.GetAvatar(aType);
        var MotionData = CAvatarInfoDataManager.GetMotionDataByID(info.MotionID);

        AvatarTouchedEvtInfo[aType].MsgID = info.BubbleMsgID;
        AvatarTouchedEvtInfo[aType].AnimParam = MotionData.AnimParam;

        AvatarTouchedEvtInfo[aType].OtherAnimParam = info.OtherAnimation_Param;

        //Debug.Log("SetTouchEvent_OutputInfo() Checking phone build. resPath = " + info.Effect_ResPath);
        HoldObjectData _holdObjData = CPlayerDataManager.Instance.GetHoldObjectDataByID(MotionData.RandObjectID);
        
        var resData = CResourceManager.Instance.GetResourceData(_holdObjData.EffectPath);
        var charAnim = avatarObj.GetComponent<CharAnimationEvent>();
        var effectDummy = charAnim.GetDummyObject(_holdObjData.HandDirection);
        AvatarTouchedEvtInfo[aType].EffectObj = new List<GameObject>();
        effectDummy.ForEach((idx, dumyObj) =>
        {
            GameObject _obj = resData.Load<GameObject>(avatarObj);
            GameObject clone = Utility.AddChild(dumyObj, _obj);
            clone.gameObject.SetActive(false);
            AvatarTouchedEvtInfo[aType].EffectObj.Add(clone);
        });
		
		//removed touch reward just do taling and motion
		switch( sceneType )
		{
			case TOUCHEVENT_SCENE_TYPE.AVATARINFO:
				SetTouchState( aType, TOUCHEVENT_SCENE_TYPE.AVATARINFO, avatarObj, avatarObj.GetComponent<Animator>() );
				break;
		}

		if( charCntlr != null )
		{
			//change ai StateMachine
			charCntlr.SetChangeAIStateMachine( AIStateMachine_Touch.Instance() );
		}
	}
    
    public static void SetTouchEvent_OutputInfo(AVATAR_TYPE aType, TouchOutputInfo info, GameObject avatarObj, TOUCHEVENT_SCENE_TYPE sceneType, bool isTraining, CCharacterController_Management charCntlr = null, Action<float> actClipLength = null)
    {
        CAvatar _avatar = CPlayer.GetAvatar(aType);
        var MotionData = CAvatarInfoDataManager.GetMotionDataByID(info.MotionID);

        AvatarTouchedEvtInfo[aType].MsgID = info.BubbleMsgID;
        AvatarTouchedEvtInfo[aType].AnimParam = MotionData.AnimParam;

        AvatarTouchedEvtInfo[aType].OtherAnimParam = info.OtherAnimation_Param;
        AvatarTouchedEvtInfo[aType].EffectObj = new List<GameObject>();


        HoldObjectData _holdObjData = CPlayerDataManager.Instance.GetHoldObjectDataByID(MotionData.RandObjectID);


        if (_holdObjData != null)
        {
            string strObjectPath = "";
            if (_holdObjData.EffectPath != "0")
            {
                strObjectPath = _holdObjData.EffectPath;
            }

            if (_holdObjData.ObjectPath != "0")
            {
                strObjectPath = _holdObjData.ObjectPath;

            }

            if (!string.IsNullOrEmpty(strObjectPath))
            {
                var resData = CResourceManager.Instance.GetResourceData(strObjectPath);
                var charAnim = avatarObj.GetComponent<CharAnimationEvent>();
                var effectDummy = charAnim.GetDummyObject(_holdObjData.HandDirection);

                effectDummy.ForEach((idx, dumyObj) =>
                {
                    GameObject _obj = resData.Load<GameObject>(avatarObj);
                    GameObject clone = Utility.AddChild(dumyObj, _obj);
                    clone.transform.localScale = Vector3.one;
                    clone.transform.localRotation = Quaternion.identity;
                    clone.transform.localPosition = Vector3.zero;

                    clone.gameObject.SetActive(false);
                    if (clone != null)
                    {
                        AvatarTouchedEvtInfo[aType].EffectObj.Add(clone);
                    }
                    else
                    {
                        CDebug.Log("!! null added");
                    }
                });
            }
            else
            {
                CDebug.LogError($"Object Path is null or empty : [avatarmotiontable ID - {info.MotionID}], [objecttable ID - {_holdObjData.ID}]");
            }
        }

		//removed touch reward just do taling and motion
		switch( sceneType )
		{
			case TOUCHEVENT_SCENE_TYPE.AVATARINFO:
				SetTouchState( aType, TOUCHEVENT_SCENE_TYPE.AVATARINFO, avatarObj, avatarObj.GetComponent<Animator>(), actClipLength);
				break;
		}

		if( charCntlr != null )
		{
			//change ai StateMachine
			charCntlr.SetChangeAIStateMachine( AIStateMachine_Touch.Instance() );
		}
	}

    public static void SetTouchEventUIObject(AVATAR_TYPE aType, TOUCHEVENT_SCENE_TYPE sceneType, GameObject avatarObj, GameObject parent)
    {
        //After Tutorial
        InitTouchEventDic();

        var key = (aType, sceneType);
        if (AvatarTouchecEvt_UIObj.ContainsKey(key) == false)
        {
            AvatarTouchecEvt_UIObj.Add(key, GetTouchUIObject(avatarObj, parent, sceneType));
        }
        else
        {
            AvatarTouchecEvt_UIObj[key] = GetTouchUIObject(avatarObj, parent, sceneType);
        }
    }

    public static GameObject GetToucedEvtUIObject(AVATAR_TYPE aType, TOUCHEVENT_SCENE_TYPE sceneType)
    {
        var key = (aType, sceneType);
        if (AvatarTouchecEvt_UIObj.ContainsKey(key))
        {
            return AvatarTouchecEvt_UIObj[key];
        }

        return null;
    }

    public static void SetActiveTouchEvtUIObject(AVATAR_TYPE aType, TOUCHEVENT_SCENE_TYPE sceneType, bool bActive)
    {
        var key = (aType, sceneType);
        if (AvatarTouchecEvt_UIObj.ContainsKey(key))
        {
            var go = AvatarTouchecEvt_UIObj[key];
            if (go)
            {
                AvatarTouchecEvt_UIObj[key].SetActive(bActive);
            }
        }
    }


    public static GameObject GetTouchUIObject(GameObject obj, GameObject parent, TOUCHEVENT_SCENE_TYPE sceneType)
    {
        var resData = CResourceManager.Instance.GetResourceData(Constants.PATH_TOUCHEVENT_UI);
        GameObject _obj = resData.Load<GameObject>(obj);
        GameObject touchUIObj = Utility.AddChild(parent, _obj);

        switch (sceneType)
        {
            case TOUCHEVENT_SCENE_TYPE.MAINLOBBY:
                break;
            case TOUCHEVENT_SCENE_TYPE.MANAGEMENT:
                break;
            case TOUCHEVENT_SCENE_TYPE.AVATARINFO:
                break;
        }
        touchUIObj.SetActive(false);

        return touchUIObj;
    }

    public static void SetTouchUIObject(TOUCHEVENT_SCENE_TYPE sceneType, GameObject avatarObj, GameObject uiObj, CAvatar avatar, long strID, RectTransform canvasRect)
    {
        switch (sceneType)
        {
            case TOUCHEVENT_SCENE_TYPE.MAINLOBBY:
            case TOUCHEVENT_SCENE_TYPE.MANAGEMENT:
                var uiDummyBone = avatarObj.transform.Find(ManagementWorldManager.CHARACTER_UI_DUMMY_BONE);
                FollowObjectPositionFor2D fop = uiObj.GetComponent<FollowObjectPositionFor2D>();
                if (fop == null)
                {
                    fop = uiObj.AddComponent<FollowObjectPositionFor2D>();
                }
                fop.Init(canvasRect, uiDummyBone.transform);
                break;

            case TOUCHEVENT_SCENE_TYPE.AVATARINFO:
                (uiObj.transform as RectTransform).anchoredPosition = new Vector2(0, -100);
                break;
        }

        TouchedEventUI _ui = uiObj.GetComponent<TouchedEventUI>();
        _ui.Initialize();
        _ui.SetData(strID, avatar);
        _ui.transform.SetAsLastSibling();
        uiObj.SetActive(true);
    }

    public static void SetTouchState(AVATAR_TYPE aType, TOUCHEVENT_SCENE_TYPE sceneType, GameObject avatarObj, Animator animator, Action<float> actPlayLength = null)
    {
        CAvatar _avatar = CPlayer.GetAvatar(aType);
        TouchedEventInfo _info = GetAvatarTouchedEvtInfo(aType);
        _info.CheckEventTime = Time.time;// GetAITimer();
        if (sceneType != TOUCHEVENT_SCENE_TYPE.AVATARINFO)
        {
            avatarObj.transform.localRotation = Quaternion.Euler(new Vector3(0, 180, 0));
        }

        //play effect with touch event effect
        _info.EffectObj.ForEach((idx, eff) => { eff.SetActive(true);});
        //_info.EffectObj.SetActive(true);

        //UI
        RectTransform canvasRect = GetCanvasRectBySceneType(sceneType);

        switch (sceneType)
        {
            case TOUCHEVENT_SCENE_TYPE.MAINLOBBY:
                //canvasRect = Instance.canvas_manager.GetComponent<RectTransform> ();
                var lobby = LobbyDirector.Instance.GetViewRootObject().GetComponent<LobbyTimeline>();

                CDefines.VIEW_AVATAR_TYPE.ForEach((_, t) =>
                {
                    bool isTarget = t == aType;
                    var selectAimParam = isTarget ? _info.AnimParam : _info.OtherAnimParam;
                    var selectCallback = isTarget ? () => SetTouchUIState (aType, sceneType, false) : new Action (delegate { });

                    if (selectAimParam.IsNullOrWhiteSpace () == false && selectAimParam != "0")
                    {
                        animator.SetTrigger (selectAimParam);
                        Observable.NextFrame (FrameCountType.EndOfFrame).Subscribe (_ =>
                        {
                            var clipInfoArr = animator.GetCurrentAnimatorClipInfo (0);
                            var clip = clipInfoArr.FirstOrDefault ().clip;
                            lobby.PlayAnimationClip (t, clip, selectCallback);
                        });


                    }

                });
                break;
            case TOUCHEVENT_SCENE_TYPE.MANAGEMENT:
                //Play Anim
                animator.SetTrigger(_info.AnimParam);
                break;
            case TOUCHEVENT_SCENE_TYPE.AVATARINFO:
                {
                    animator.SetTrigger(_info.AnimParam);
                    var clipInfo = animator.GetCurrentAnimatorClipInfo(0);
                    if (clipInfo != null)
                    {
                        if (clipInfo.Length > 0)
                        {
                            AnimationClip clip = clipInfo.First().clip;
                            if (clip != null)
                            {
                                actPlayLength?.Invoke(clip.length);
                            }
                        }
                    }
                }
                break;
        }

        if (canvasRect == null)
        {
            CDebug.LogError("SetTouchState() canvasRect == null");
            return;
        }

        GameObject _touchedEvtUI = GetToucedEvtUIObject(aType, sceneType);

        SetTouchUIObject(
            sceneType,
            avatarObj,
            _touchedEvtUI,
            _avatar,
            _info.MsgID,
            canvasRect);

        //Reward
        REWARD_CONSUME_TYPES _rewardType = GetTouchEventRewardType(aType);
        CDebug.Log("     ###### Touch Avatar $$ _rewardType = " + _rewardType);
        if (_rewardType != REWARD_CONSUME_TYPES.NULL)
        {
            string _rewardCnt = "+ " + GetTouchEventRewardCount(aType, _rewardType);
            if (sceneType == TOUCHEVENT_SCENE_TYPE.MANAGEMENT)
            {
                _touchedEvtUI.GetComponent<TouchedEventUI>().SetReward(_rewardType, _rewardCnt);
            }
            else
            {
                ShowTouchRewardUI(
                    avatarObj,
                    avatarObj.transform.parent,
                    _rewardType,
                    _rewardCnt,
                    sceneType);
            }
        }

    }


    public static bool Proc_TouchState(AVATAR_TYPE aType, float clipLen)
    {

        TouchedEventInfo _info = GetAvatarTouchedEvtInfo(aType);
        float curTime = /*GetAITimer()*/Time.time - _info.CheckEventTime;
        CDebug.Log($"[Animation Check] Proc_TouchState : clipLen = {clipLen} : curTime = {curTime}");

        if (curTime >= clipLen)
        {
            //Length of animation for TouchEvent need to fix
            SetTouchUIState(aType, TOUCHEVENT_SCENE_TYPE.MANAGEMENT, false);

            return true;
        }

        return false;
    }

    public static void ShowTouchRewardUI(GameObject avatarObj, Transform parent, REWARD_CONSUME_TYPES rewardType, string rewardCnt, TOUCHEVENT_SCENE_TYPE sceneType)
    {
        var resData = CResourceManager.Instance.GetResourceData(Constants.PATH_3DUI_PRODUCT_DONE);
        GameObject obj = UnityEngine.Object.Instantiate(resData.Load<GameObject>(avatarObj));

        TouchedEvent3DUI _ui = obj.AddComponent<TouchedEvent3DUI>();
        _ui.Initialize();
        _ui.SetData(rewardType, rewardCnt);
        _ui.SetTrans(avatarObj, parent, sceneType);
    }


    public static TouchedEventInfo GetAvatarTouchedEvtInfo(AVATAR_TYPE aType)
    {
        if (AvatarTouchedEvtInfo.ContainsKey(aType) == false)
        {
            return null;
        }

        return AvatarTouchedEvtInfo[aType];
    }


    //Touch Reward

    private static void SetTouchEventReward(AVATAR_TYPE aType, RewardInfo reward)
    {
        if (reward == null)
        {
            return;
        }

        if (AvatarTouchEvtRewardInfo.ContainsKey(aType) == false)
        {
            AvatarTouchEvtRewardInfo.Add(aType, reward);
        }
        else
        {
            AvatarTouchEvtRewardInfo[aType] = reward;
        }
    }

    public static bool ExistTouchEvtRewardInfo(AVATAR_TYPE aType)
    {
        if (AvatarTouchEvtRewardInfo.ContainsKey(aType) == false)
        {
            CDebug.Log($"MainManager.ExistTouchEvtRewardInfo() AvatarTouchEvtRewardInfo does not contain {aType}");
            return false;
        }
        else
        {
            if (AvatarTouchEvtRewardInfo[aType] == null)
            {
                CDebug.Log($"MainManager.ExistTouchEvtRewardInfo() AvatarTouchEvtRewardInfo[{aType}] is null");
                return false;
            }
        }

        return true;
    }

    private static REWARD_CONSUME_TYPES GetTouchEventRewardType(AVATAR_TYPE aType)
    {
        if (ExistTouchEvtRewardInfo(aType) == false)
        {
            return REWARD_CONSUME_TYPES.NULL;
        }

        REWARD_CONSUME_TYPES _type = REWARD_CONSUME_TYPES.NULL;

        if (AvatarTouchEvtRewardInfo[aType].gold > 0) _type = REWARD_CONSUME_TYPES.GOLD;
        else if (AvatarTouchEvtRewardInfo[aType].gem_e > 0 || AvatarTouchEvtRewardInfo[aType].gem_p > 0) _type = REWARD_CONSUME_TYPES.GEM;
        else if (AvatarTouchEvtRewardInfo[aType].intimacy != null && AvatarTouchEvtRewardInfo[aType].intimacy.Count > 0) _type = REWARD_CONSUME_TYPES.INTIMACY;

        return _type;
    }

    private static string GetTouchEventRewardCount(AVATAR_TYPE aType, REWARD_CONSUME_TYPES type)
    {
        string _count = "";

        if (ExistTouchEvtRewardInfo(aType) == false)
        {
            return string.Empty;
        }

        switch (type)
        {
            case REWARD_CONSUME_TYPES.GOLD:
                _count = AvatarTouchEvtRewardInfo[aType].gold.ToString();
                break;
            case REWARD_CONSUME_TYPES.GEM:
                if (AvatarTouchEvtRewardInfo[aType].gem_e > 0) _count = AvatarTouchEvtRewardInfo[aType].gem_e.ToString();
                else if (AvatarTouchEvtRewardInfo[aType].gem_p > 0) _count = AvatarTouchEvtRewardInfo[aType].gem_p.ToString();
                break;
            case REWARD_CONSUME_TYPES.INTIMACY:
                int _idx = (int)aType;
                for (int i = 0; i < AvatarTouchEvtRewardInfo[aType].intimacy.Count; ++i)
                {
                    if (AvatarTouchEvtRewardInfo[aType].intimacy[i].mid == _idx)
                    {
                        _count = AvatarTouchEvtRewardInfo[aType].intimacy[i].cnt.ToString();
                        break;
                    }
                }
                break;
        }

        return _count;
    }

    public static TouchOutputInfo GetCurrentTouchOutPutInfo(AVATAR_TYPE aType)
    {
        return AvatarCurrentTouchOutputInfo[aType];
    }

    public static void ClearTouchEventDic()
    {
        if (AvatarTouchedEvtInfo != null) AvatarTouchedEvtInfo.Clear();
        if (AvatarTouchecEvt_UIObj != null) AvatarTouchecEvt_UIObj.Clear();
        if (AvatarTouchEvtRewardInfo != null) AvatarTouchEvtRewardInfo.Clear();
    }
    #endregion TOUCH_EVENT
}
