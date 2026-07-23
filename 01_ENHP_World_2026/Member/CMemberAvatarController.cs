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

#endregion

using Game.RestAPI;
using System;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using SNG;

namespace CharacterControl
{
    public class CMemberAvatarController : CBaseCharController
    {
        private CMemberAvatar AvatarInfo = null;
        private GameObject AvatarFaceObj = null;
        private Dictionary<string, Texture> FaceTexList = null;
        [SerializeField] private GameObject DummyAcc_Head;
        [SerializeField] private GameObject DummyAcc_Face;
        [SerializeField] private GameObject DummyAcc_Body;
        [SerializeField] private GameObject DummyAcc_HandR;
        [SerializeField] private GameObject DummyAcc_HandL;

        public Dictionary<STYLE_ITEM_TYPE, GameObject> DummyAccObjDic = null;
        public Dictionary<string, GameObject> CharDummyObjDic = null;
        private GameObject chatBaloonIcon = null;
        private GameObject AnimAttachObj = null;
        private GameObject AnimEffectObj = null;
        private string MemberName;
        private SingleAssignmentDisposable TouchAnimDisposer = null;
        private SingleAssignmentDisposable WearAnimDisposer = null;
        private SingleAssignmentDisposable MemberInteractionAnimDisposer = null;


        public void Init(CMemberAvatar avatarInfo, MEMBER_TYPE memberType = MEMBER_TYPE.NONE)
        {
            base.Init(CHARACTER_TYPE.MEMBER);
            AvatarInfo = avatarInfo;

#if UNITY_EDITOR
            if (memberType != MEMBER_TYPE.NONE)
            {
                AvatarInfo.MemberType = memberType;
            }
#endif

            if (FaceTexList == null)
            {
                FaceTexList = new Dictionary<string, Texture>();
            }

            if (DummyAccObjDic == null)
            {
                DummyAccObjDic = new Dictionary<STYLE_ITEM_TYPE, GameObject>();
            }

            DummyAccObjDic.Add(STYLE_ITEM_TYPE.ACC_HEAD, DummyAcc_Head);
            DummyAccObjDic.Add(STYLE_ITEM_TYPE.ACC_FACE, DummyAcc_Face);
            DummyAccObjDic.Add(STYLE_ITEM_TYPE.ACC_BODY, DummyAcc_Body);

            SetCharDummyObjDic();

            SetFatigrade(AvatarInfo.GetFatigrade());

            SetMemberName();

            SetAnim((int)AvatarInfo.MemberType, eAniBehavior.ANI_TOUCH, SNGDefines.SIGNATURE_ANIMCLIP_DEFAULT_NAME_PART);
        }

        public void InitGacha(CMemberAvatar avatarInfo, MEMBER_TYPE memberType)
        {
            base.Init(CHARACTER_TYPE.MEMBER);
            AvatarInfo = avatarInfo;

            AvatarList _info = CMemberAvatarServerDataManager.Instance.GetMemberAvatarInfo((int)memberType);
            CMemberAvatar avatar = new CMemberAvatar(memberType);
            avatar.SetMemberAvatarInfo(_info);

            //Init(avatar, memberType);
            //avc.SetSignatureAnim(MEMBER_TYPE.MEMBER_JAY);

            avatar.SetCurEquipSItemDic();

            var obj = GetComponentInChildren<CMemberAvatarController>();
            avatar.SetBaseMemberAvatarFromObj(obj.gameObject);

            avatar.EquipAllStylingItems();
            avatar.SetAvatarObjDefaultFaceTexture();
        }
        public void SetAvatarFaceObj(string faceState)
        {
            if (AvatarInfo != null)
            {
                AvatarFaceObj = FindFaceObj();
                ChangeAvatarFace(faceState);
            }
        }

        public MEMBER_TYPE GetMemberType()
        {
            if (AvatarInfo != null)
            {
                return AvatarInfo.MemberType;
            }

            return MEMBER_TYPE.NONE;
        }

        private GameObject FindFaceObj()
        {
            Transform[] objs = gameObject.GetComponentsInChildren<Transform>();
            foreach (var obj in objs)
            {
                if (obj.CompareTag(OBJ_TAG.MEMBER_FACE))
                {
                    return obj.gameObject;
                }
            }

            return null;
        }

        private void SetCharDummyObjDic()
        {
            if (CharDummyObjDic == null)
            {
                CharDummyObjDic = new Dictionary<string, GameObject>();
            }
            CharDummyObjDic.Add("Root", AvatarInfo.GetAvatarObj());
            CharDummyObjDic.Add("Dummy R Hand", DummyAcc_HandR);
            CharDummyObjDic.Add("Dummy L Hand", DummyAcc_HandL);
        }

        private void SetMemberName()
        {
            if (AvatarInfo != null)
            {
                switch (AvatarInfo.MemberType)
                {
                    case MEMBER_TYPE.JUNGWON:
                        MemberName = "jungwon";
                        break;
                    case MEMBER_TYPE.HEESEUNG:
                        MemberName = "heeseung";
                        break;
                    case MEMBER_TYPE.JAY:
                        MemberName = "jay";
                        break;
                    case MEMBER_TYPE.JAKE:
                        MemberName = "jake";
                        break;
                    case MEMBER_TYPE.SUNGHOON:
                        MemberName = "sunghoon";
                        break;
                    case MEMBER_TYPE.SUNOO:
                        MemberName = "sunoo";
                        break;
                    case MEMBER_TYPE.NIKI:
                        MemberName = "niki";
                        break;
                }
            }
        }

        public void PlayWearAnim(STYLE_ITEM_TYPE itemType)
        {            
            DestroyAnimationObject();
            SetAnim((int)itemType, eAniBehavior.ANI_STYLE, SNGDefines.ANIM_STATE_WEAR);

            SetTrigger(SNGDefines.ANIM_STATE_WEAR);

            if (WearAnimDisposer != null)
            {
                WearAnimDisposer.Dispose();
            }
            WearAnimDisposer = new SingleAssignmentDisposable();

            float length = animInfo.GetAnimClipLength(SNGDefines.ANIM_STATE_WEAR);

            WearAnimDisposer.Disposable = Observable.Timer(TimeSpan.FromSeconds(length))
            .Subscribe(_ =>
            {
                PlayIdleAnim();
                WearAnimDisposer.Dispose();
            })
            .AddTo(this);
        }

        public void PlayInteractionAnim(eAniBehavior interactionType, Action<bool, float> finishInteraction)
        {
            SetAnim((int)MEMBER_TYPE.MEMBER_ALL, interactionType, SNGDefines.ANIM_STATE_INTERACTION);

            SetTrigger(SNGDefines.ANIM_STATE_INTERACTION);

            
            MemberAnimationData _data = CMemberAvatarDataManager.Instance.GetMemberAnimationData(interactionType, (int)MEMBER_TYPE.MEMBER_ALL);
            if (_data != null)
            {
                DestroyAnimationObject();
                SetAnimationEffect(_data);
            }

            DisposeMemberInteractionAnimDisposer();
            MemberInteractionAnimDisposer = new SingleAssignmentDisposable();

            float length = animInfo.GetAnimClipLength(SNGDefines.ANIM_STATE_INTERACTION);

            MemberInteractionAnimDisposer.Disposable = Observable.Timer(TimeSpan.FromSeconds(length))
            .Subscribe(_ =>
            {
                DestroyAnimationObject();
                PlayIdleAnim();
                if (finishInteraction != null)
                {
                    finishInteraction.Invoke(true, SNGDefines.MEMBER_FINISH_INTERACTION_DELAY);
                }
                MemberInteractionAnimDisposer.Dispose();
            })
            .AddTo(this);
        }

        public void DisposeMemberInteractionAnimDisposer()
        {
            if (MemberInteractionAnimDisposer != null)
            {
                MemberInteractionAnimDisposer.Dispose();
            }
        }


        public void PlayTouchAnim(bool bActiveBaloon = true)
        {
            string touchParam = GetParamName(SNGDefines.ANIM_STATE_TOUCH);

            // condition high  == SNGDefines.ANIM_FATIGABILITY_HIGH (just in animation)
            // signature -> touch_sig
            if (touchParam.Contains(SNGDefines.ANIM_FATIGABILITY_HIGH))
            {
                int randTouch = UnityEngine.Random.Range(1, 3);
                if (randTouch == 1)//50%
                {
                    touchParam = SNGDefines.ANIM_STATE_TOUCH_SIG;
                }
            }
            SetAnimState(MEMBER_ANIM_STATE.TOUCH);
            SetCurrentParamName(SNGDefines.ANIM_STATE_TOUCH);

            CSoundControl.Instance.EffectPlay(SOUND_ID.SFX_SNG_FRIENDS_TOUCH);
            if (touchParam.Equals(SNGDefines.ANIM_STATE_TOUCH_SIG))
            {
                PlayTouchSignatureAnim();
            }


            SetTrigger(touchParam);

            if (bActiveBaloon)
            {
                float clipLength = GetSignatureTouchAnimLength();
                CreateChatBaloon(clipLength);
            }
        }

        public void PlayTouchSignatureAnim(bool bFromUI = false)
        {
            SetTrigger(SNGDefines.ANIM_STATE_TOUCH_SIG);
            if (TouchAnimDisposer != null)
            {
                TouchAnimDisposer.Dispose();
            }
            TouchAnimDisposer = new SingleAssignmentDisposable();
            MemberAnimationData _data = CMemberAvatarDataManager.Instance.GetMemberAnimationData(eAniBehavior.ANI_TOUCH, (int)GetMemberType());
            if (_data != null)
            {
                SetAnimationObject(_data);
                SetAnimationEffect(_data);
            }

            float clipLength = animInfo.GetAnimClipLength(SNGDefines.SIGNATURE_ANIMCLIP_DEFAULT_NAME_PART);
            TouchAnimDisposer.Disposable = Observable.Timer(TimeSpan.FromSeconds(clipLength))
            .Subscribe(_ =>
            {
                SetAnimState(MEMBER_ANIM_STATE.NORMAL);
                DestroyAnimationObject();
                if (bFromUI) PlayIdleAnim();
                TouchAnimDisposer.Dispose();
            })
            .AddTo(this);
        }

        public void SetDanceAnim()
        {
            MemberAnimationData danceAnimData = CMemberAvatarManager.Instance.GetMemberDanceAnimData();
            if (danceAnimData != null)
            {
                AnimationClip danceClip = LoadAnimationClip(danceAnimData.ResPath);
                if (danceAnimData != null)
                {
                    animInfo.SwapAnimClip(danceClip, SNGDefines.ANIM_STATE_DANCE);
                }
            }

        }

        public void PlayDanceAnim()
        {
            SetTrigger(SNGDefines.ANIM_STATE_DANCE);
        }

        public float GetDanceAnimClipLength()
        {
            return animInfo.GetAnimClipLength(SNGDefines.ANIM_STATE_DANCE);
        }


        public void SetAnimationObject(MemberAnimationData animData)
        {
            DestroyAnimationObject();

            if (animData.ObjResPath.Equals("0") == false)
            {
                // Load Object
                var resData = CResourceManager.Instance.GetResourceData(animData.ObjResPath);
                if (resData != null)
                {
                    GameObject animObj = resData.Load<GameObject>(gameObject);
                    if (animObj != null)
                    {
                        AnimAttachObj = Instantiate(animObj);
                        // Set the parent to the avatar's transform
                        AnimAttachObj.transform.SetParent(CharDummyObjDic[animData.ObjDummyObj].transform);
                        AnimAttachObj.transform.localPosition = Vector3.zero;
                        AnimAttachObj.transform.localRotation = Quaternion.Euler(Vector3.zero);
                        AnimAttachObj.transform.localScale = Vector3.one;
                    }
                }
            }

        }

        public void SetAnimationEffect(MemberAnimationData animData)
        {
            var assetService = CCoreServices.GetCoreService<CAssetService>();
            if (assetService != null)
            {
                bool isOpenMemberHouse = assetService.IsOpenedPrefab(CPREFAB_KEY.page_citizen);
                if (isOpenMemberHouse)
                {
                    return;
                }
            }

            // Load Effect
            string effectResPath = CMemberAvatarDataManager.Instance.GetMemberEffectResPath(animData.EffectType);
            if (!string.IsNullOrEmpty(effectResPath))
            {
                var effectObj = CResourceManager.Instance.GetResourceData(effectResPath);
                if (effectObj != null)
                {
                    GameObject effectInstance = effectObj.Load<GameObject>(gameObject);
                    if (effectInstance != null)
                    {
                        AnimEffectObj = Instantiate(effectInstance);
                        // Set the parent to the avatar's transform
                        AnimEffectObj.transform.SetParent(FX_Mount.transform);
                        AnimEffectObj.transform.localPosition = Vector3.zero;
                        AnimEffectObj.transform.localRotation = Quaternion.Euler(Vector3.zero);
                        AnimEffectObj.transform.localScale = Vector3.one;
                    }
                    else
                    {
                        Debug.LogError($"{GetMemberType()}'s Effect resource not found for {animData.EffectType} at path: {effectResPath}");
                    }
                }

            }
        }

        public void DestroyAnimationObject()
        {
            if (AnimAttachObj != null)
            {
                Destroy(AnimAttachObj);
                AnimAttachObj = null;
            }

            if (AnimEffectObj != null)
            {
                Destroy(AnimEffectObj);
                AnimEffectObj = null;
            }
        }

        public GameObject GetDummyObj(STYLE_ITEM_TYPE type)
        {
            if (DummyAccObjDic.ContainsKey(type))
            {
                return DummyAccObjDic[type];
            }

            return null;
        }

        public GameObject GetDummyObjByName(string dummyName)
        {
            if (CharDummyObjDic.ContainsKey(dummyName))
            {
                return CharDummyObjDic[dummyName];
            }

            return null;
        }

        private Texture GetFaceTexture(string param)
        {
            Texture faceTex = null;

            if (FaceTexList.ContainsKey(param))
            {
                faceTex = FaceTexList[param];
            }
            else
            {
                string faceTexPath = string.Format(SNGDefines.FACE_TEX_BASE_PATH, MemberName, param);
                CDebug.Log("faceTexPath : " + faceTexPath);
                var resData = CResourceManager.Instance.GetResourceData(faceTexPath);
                if (resData != null)
                {
                    faceTex = resData.Load<Texture>(AvatarFaceObj);
                    if (faceTex != null)
                    {
                        FaceTexList.Add(param, faceTex);
                    }
                }
            }

            return faceTex;
        }

        private GameObject balloonPrefab;
        public GameObject GetBalloonPrefab()
        {
            if (balloonPrefab == null)
            {
                var resource = CResourceManager.Instance.GetResourceData(CommonType.obj_kkmaz_chat_baloon);
                balloonPrefab = resource.LoadObject(this) as GameObject;
            }
            return balloonPrefab;
        }

        public void CreateChatBaloon(float fPlayTime = 3)
        {
            MEMBER_TYPE memberType = AvatarInfo.MemberType;
            string storyKey = CMemberAvatarDataManager.Instance.GetTouchStory(memberType);
            string nameStr = SNGDataManager.Instance.GetMemberNameStr(memberType);

            if (!chatBaloonIcon)
            {
                var balloonPrefab = GetBalloonPrefab();

                chatBaloonIcon = GameObject.Instantiate(balloonPrefab) as GameObject;

                chatBaloonIcon.transform.SetParent(TouchMgr.Inst.Edit_Field_Icon.transform);
                chatBaloonIcon.transform.InitScale();

                Vector3 pos = Camera.main.WorldToViewportPoint(transform.position + new Vector3(0, /*sphereCollider.radius*/1, 0));
                Vector3 vPoint = TouchMgr.Inst.UICam.ViewportToWorldPoint(pos);
                vPoint.z = 0.1f;

                chatBaloonIcon.transform.position = vPoint;
                FollowText followText = chatBaloonIcon.GetOrAddComponent<FollowText>();
                followText.SetTarget(this.gameObject, null, new Vector3(0, 3f, 0));
            }

            var obj_kkmaz_chat_baloon = chatBaloonIcon.GetOrAddComponent<obj_kkmaz_chat_baloon>();
            obj_kkmaz_chat_baloon.Init(nameStr, storyKey, fPlayTime, this);

            SetActiveChatBaloon(true);
        }

        public void SetActiveChatBaloon(bool bActive)
        {
            if (chatBaloonIcon)
            {
                chatBaloonIcon.SetActive(bActive);
            }
        }

        private string GetFatigradeStr()
        {
            string fatigradeStr = string.Empty;
            FATIGABILITY fatigarde = GetFatigrade();

            switch (fatigarde)
            {
                case FATIGABILITY.LOW: //condition low
                    fatigradeStr = SNGDefines.ANIM_FATIGABILITY_LOW;
                    break;
                case FATIGABILITY.MID:
                    fatigradeStr = SNGDefines.ANIM_FATIGABILITY_MID;
                    break;
                case FATIGABILITY.HIGH: //condition high
                    fatigradeStr = SNGDefines.ANIM_FATIGABILITY_HIGH;
                    break;
            }

            return fatigradeStr;
        }


        public string GetIdleParamNameRandomly()
        {
            string fatigradeStr = GetFatigradeStr();
            int randIdle = UnityEngine.Random.Range(0, 3) + 1;

            return string.Format(SNGDefines.MEMBER_ANIM_IDLE_STR_FORMAT, fatigradeStr, randIdle);
        }

        private string GetParamName(string state)
        {
            string fatigradeStr = GetFatigradeStr();

            return string.Format(SNGDefines.ARG_2TYPE_STR_FORMAT, state, fatigradeStr);
        }


        // Get the parameter name by state for playing animation
        public string GetParamNameByState(string state)
        {
            string paramName = string.Empty;

            switch (state)
            {
                case SNGDefines.ANIM_STATE_IDLE:
                    paramName = GetIdleParamNameRandomly();
                    break;
                case SNGDefines.ANIM_STATE_WALK:
                case SNGDefines.ANIM_STATE_TOUCH:
                    paramName = GetParamName(state);
                    break;
                default:
                    paramName = state;
                    break;
            }

            SetCurrentParamName(state);

            return paramName;
        }


        public string GetDecoParamName(long aniID)
        {                
            var animClipPath = CMemberAvatarDataManager.Instance.GetAnimResClipPath(aniID);
            AnimationClip decoInteractClip = LoadAnimationClip(animClipPath);
            if (decoInteractClip != null)
            {
                animInfo.SwapAnimClip(decoInteractClip, SNGDefines.ANIM_STATE_DECO_INTERACTION);
            }                

            return SNGDefines.ANIM_STATE_DECO_INTERACTION;
        }


        public bool IsAnimationEffectPlaying(EFFECT_TYPE effectType)
        {
            return IsEffectPlaying(effectType);
        }

        public void SetAnimationEffect(string paramName)
        {
            if (paramName.Equals(SNGDefines.ANIM_STATE_IDLE))
            {
                return;
            }

            EFFECT_TYPE effType = GetLoopEffectTypeByParamName(paramName);
            if (effType == EFFECT_TYPE.NONE)
            {
                return;
            }

            SetEffectObject(effType);
        }

        #region ANIMATION_EVENT
        public void ChangeAvatarFace(string param)
        {
            if (AvatarFaceObj != null)
            {
                Texture faceTex = GetFaceTexture(param);

                if (faceTex != null)
                {
                    var smr = AvatarFaceObj.GetComponent<SkinnedMeshRenderer>();
                    if (smr != null && smr.material != null)
                    {
                        smr.material.SetTexture("_MainTex", faceTex);
                    }
                }
            }
        }

        public void normal()
        {
            ChangeAvatarFace(FACE_FUNC_NAME.NORMAL);
        }

        public void eyeclose1()
        {
            ChangeAvatarFace(FACE_FUNC_NAME.EYE_CLOSE1);
        }

        public void eyeclose2()
        {
            ChangeAvatarFace(FACE_FUNC_NAME.EYE_CLOSE2);
        }

        public void smiling()
        {
            ChangeAvatarFace(FACE_FUNC_NAME.SMILING);
        }

        public void confident()
        {
            ChangeAvatarFace(FACE_FUNC_NAME.CONFIDENT);
        }

        public void grinning()
        {
            ChangeAvatarFace(FACE_FUNC_NAME.GRINNING);
        }

        public void puzzled()
        {
            ChangeAvatarFace(FACE_FUNC_NAME.PUZZLED);
        }

        public void angry()
        {
            ChangeAvatarFace(FACE_FUNC_NAME.ANGRY);
        }

        public void cry1()
        {
            ChangeAvatarFace(FACE_FUNC_NAME.CRY1);
        }

        public void cry2()
        {
            ChangeAvatarFace(FACE_FUNC_NAME.CRY2);
        }

        public void tired()
        {
            ChangeAvatarFace(FACE_FUNC_NAME.TIRED);
        }

        public void thinking1()
        {
            ChangeAvatarFace(FACE_FUNC_NAME.THINKING1);
        }

        public void thinking2()
        {
            ChangeAvatarFace(FACE_FUNC_NAME.THINKING2);
        }

        public void appeal()
        {
            ChangeAvatarFace(FACE_FUNC_NAME.APPEAL);
        }

        public void yawning1()
        {
            ChangeAvatarFace(FACE_FUNC_NAME.YAWNING1);
        }

        public void yawning2()
        {
            ChangeAvatarFace(FACE_FUNC_NAME.YAWNING2);
        }

        public void listening()
        {
            ChangeAvatarFace(FACE_FUNC_NAME.LISTENING);
        }

        public void open1()
        {
            ChangeAvatarFace(FACE_FUNC_NAME.OPEN1);
        }

        public void open2()
        {
            ChangeAvatarFace(FACE_FUNC_NAME.OPEN2);
        }

        public void heart()
        {
            ChangeAvatarFace(FACE_FUNC_NAME.HEART);
        }

        #endregion ANIMATION_EVENT


        public void ODestroy()
        {
            if (FaceTexList != null)
            {
                FaceTexList.Clear();
                FaceTexList = null;
            }

            if (DummyAccObjDic != null)
            {
                DummyAccObjDic.Clear();
                DummyAccObjDic = null;
            }
        }
    }

}