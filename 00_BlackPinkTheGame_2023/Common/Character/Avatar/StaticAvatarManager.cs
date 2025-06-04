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

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticAvatarManager : MonoBehaviour
{
    static StaticAvatarManager s_this = null;
    public static StaticAvatarManager Instance { get { return s_this; } }

    public static GameObject AvatarObjContainer;
    public static Dictionary<AVATAR_TYPE, CStaticAvatar> StaticAvatar = new Dictionary<AVATAR_TYPE, CStaticAvatar>();

    private IEnumerator DBControlCoroutine;
    private Dictionary<string, DynamicBone> ControllDBDic;
    public GameObject AvatarDBWeightObj;
    public AVATAR_TYPE AvatarTypeDBWeight;
    

    private void Awake()
    {
        DontDestroyOnLoad(this);
        s_this = this;

        StaticAvatar = new Dictionary<AVATAR_TYPE, CStaticAvatar>();

        AvatarObjContainer = new GameObject("AvatarObjContainer");
        AvatarObjContainer.transform.SetParent(Instance.transform);
        AvatarObjContainer.transform.localPosition = new Vector3(50, 0, 0);
    }


    public static void InitAllAvatarBlendShape()
    {
        foreach (AVATAR_TYPE type in StaticAvatar.Keys)
        {
            InitAvatarBlendShape(type);
        }
    }

    public static void InitAvatarBlendShape(AVATAR_TYPE avatarType)
    {
        StaticAvatar[avatarType].InitAvatarBlendShape();
    }

    public static void SetAvatars()
    {
        foreach (AVATAR_TYPE type in CDefines.VIEW_AVATAR_TYPE)
        {
            CAvatar avatar = CPlayer.GetAvatar(type);
            // 아바타 오브젝트 초기화, 이미 했으면 무시됨
            avatar.SetAvatarObject();

            //CRuntimeAnimControllerSwitchManager.Instance.SetRuntimeAnimatorController( (long)type, ANIMCONTROLLER_USE_TYPE.COMMON, GetAvatarObject( type ) );

            CStyleItemInvenManager.SetStaticDefaultEquipSItem(type);
            //avatar.SetAvatarDefaultEquipSItem();
            CStyleItemInvenManager.EquipDefaultPartsStatic(type, GetAvatarObject(type), null);
        }

        //이미 위에서 CStyleItemInvenManager.SetStaticDefaultEquipSItem(type); 하고 있어서 없어도 될 것 같음
        CStyleItemInvenManager.Instance.SetAvatarDefaultItem();
        //CPlayer.Instance.SetAvatarDefaultItem();

    }

    public static void SetAvatarObjBackWithLOD()
    {
        foreach(CStaticAvatar avatar in StaticAvatar.Values)
        {
            AvatarManager.SetActivateLOD( ref avatar.AvatarOBj, false );
        }

        SetAvatarObjBack();
    }


    public static void ForceDeleteFXElements()
    {
		Transform foundTransform = null;
        string elementName = null;

		foreach( CStaticAvatar avatar in StaticAvatar.Values )
        {
			int childCount = avatar.AvatarOBj.transform.childCount;

			for( int index = 0; index < childCount; ++index )
			{
				foundTransform = avatar.AvatarOBj.transform.GetChild( index );
				elementName = foundTransform.name;

				if( elementName == "CharShadow(Clone)" ||
					elementName == "fx_motion_musicnote(Clone)" ||
					elementName == "fx_motion_Flower(Clone)" )
				{
					if( foundTransform.gameObject.activeSelf == false )
					{
						GameObject.Destroy( foundTransform.gameObject );
					}
				}
			}
		}
    }



    public static void SetAvatarObjBack(bool bReplaceContentPos = false)
    {
        foreach (AVATAR_TYPE type in StaticAvatar.Keys)
        {
            InitAvatarBlendShape(type);
            SetAvatarObjBack(type, bReplaceContentPos);
        }
    }

    public static void SetAvatarObjBack(AVATAR_TYPE type, bool bReplaceContentPos = false)
    {
        if (bReplaceContentPos)
        {
            ReplaceAvatar(type);
        }
        else
        {
            if (StaticAvatar.ContainsKey(type))
            {
                if (StaticAvatar[type].AvatarOBj && AvatarObjContainer != null)
                {
#if UNITY_EDITOR
                   StaticAvatar[type].AvatarOBj.name = type.ToString();
#endif
					StaticAvatar[type].AvatarOBj.transform.SetParent (AvatarObjContainer.transform);
                    StaticAvatar[type].AvatarOBj.transform.localScale = Vector3.one;
                    StaticAvatar[type].AvatarOBj.transform.localRotation = Quaternion.Euler (Vector3.zero);
                    StaticAvatar[type].AvatarOBj.SetActive (false);
                }
            }
        }
    }


    public static void SetAvatarObjBackByBPWorld(AVATAR_TYPE type, bool bReplaceContentPos = false)
    {
        if (StaticAvatar.ContainsKey(type))
        {
#if UNITY_EDITOR
            StaticAvatar[ type ].AvatarOBj.name = type.ToString();
#endif
			StaticAvatar[type].AvatarOBj.transform.SetParent(AvatarObjContainer.transform);
            StaticAvatar[type].AvatarOBj.transform.localScale = Vector3.one;
            StaticAvatar[type].AvatarOBj.transform.localRotation = Quaternion.Euler(Vector3.zero);
            StaticAvatar[type].AvatarOBj.SetActive(false);
        }

        if (bReplaceContentPos)
        {
            ReplaceAvatar(type);
        }
    }



    private static void ReplaceAvatar(AVATAR_TYPE type)
    {
        SceneID _curSceneID = (SceneID)CDirector.Instance.GetCurrentSceneID();
        switch (_curSceneID)
        {
            case SceneID.LOBBY_SCENE:
                LobbyManager.ResumeMainLobbyTimeLine(type);
                break;
            case SceneID.MANAGEMENT_SCENE:
                ManagementManager.Instance.worldManager.ReplaceAvatar(type);
                ManagementManager.Instance.worldManager.AvatarMgr.PreSetAiInteraction_play(type);
                break;
            case SceneID.BPWORLD_SCENE:
                StaticAvatar[type].AvatarOBj.SetActive(false);
				BPWorldEventMessageFactory.Instance.Avatar.RegistReplaceAvatar( type );
                StaticAvatar[type].AvatarOBj.SetActive(true);
                break;
                //case SceneID.MEMBERDETAILINFO_SCENE:
                //    ManagementCanvasManager canvasMgr = (ManagementCanvasManager)ManagementManager.Instance.GetCanvasManager();
                //    //canvasMgr.PageUI
                //    break;
        }

        StaticAvatar[type].AvatarOBj.transform.localScale = Vector3.one;

    }

    public static void SetStaticAvatar(AVATAR_TYPE avatarType, GameObject avatarObj)
    {
        if (StaticAvatar.ContainsKey(avatarType))
        {
            StaticAvatar[avatarType] = new CStaticAvatar(avatarObj, avatarObj.GetComponent<Animator>());
        }
        else
        {
            StaticAvatar.Add(avatarType, new CStaticAvatar(avatarObj, avatarObj.GetComponent<Animator>()));
        }
    }

    public static GameObject GetAvatarObject(AVATAR_TYPE avatarType)
    {
        if (StaticAvatar.ContainsKey(avatarType))
        {
            return StaticAvatar[avatarType].AvatarOBj;
        }
        return null;

    }

    public static Animator GetAvatarAnimator(AVATAR_TYPE avatarType)
    {
        if (StaticAvatar.ContainsKey(avatarType))
        {
            return StaticAvatar[avatarType].AvatarAnimator;
        }
        return null;
    }

    public void SetControlDynamicBoneWeight(AVATAR_TYPE aType, GameObject avatarObj)
    {
        CAvatar avatar = CPlayer.GetAvatar(aType);

        AvatarTypeDBWeight = aType;
        AvatarDBWeightObj = avatarObj;
        ControllDBDic = avatar.GetDynamicBoneDic();
    }

    public void StartControlDynamicBoneWeightCorroutine()
    {
        DBControlCoroutine = ControlAvatarDynamicBoneWeight();
        StartCoroutine(DBControlCoroutine);
    }

    public void StopControlDynamicBoneWeightCorroutine()
    {
        SetDynamicBoneWeight(0.1f);
        DBControlCoroutine = ControlAvatarDynamicBoneWeight();
        StopCoroutine(DBControlCoroutine);
    }

    IEnumerator ControlAvatarDynamicBoneWeight()
    {
        //frame비교 숫자 1, 11, 7, 10등등은 자연스럼을 추구하고자 어쩔수 없이 하드코딩
        int WEIGHTMAX = 1;
        float WEIGHTSTEP = 0.1f;
        int MAXFRAME = 11;
        int frame = 0;

        while (frame < MAXFRAME)
        {
            if (frame == 1)
            {
                if (AvatarTypeDBWeight >= AVATAR_TYPE.AVATAR_JISOO_UI)
                {
                    AvatarDBWeightObj.SetActive(true);
                }
            }
            else if (frame == 7)
            {
                if (AvatarTypeDBWeight >= AVATAR_TYPE.AVATAR_JISOO_UI)
                {
                    AvatarManager.ChangeLayersRecursively(AvatarDBWeightObj.transform, LayerMask.LayerToName(AvatarDBWeightObj.layer));
                }
                else
                {
                    AvatarDBWeightObj.SetActive(true);
                }
            }
            else if (frame == (MAXFRAME - 1))
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
            ++frame;
        }
    }

    private void SetDynamicBoneWeight(float weight)	//다이나믹본 웨이트값 조절
    {
        if (AvatarDBWeightObj != null)
        {
            foreach(DynamicBone db in ControllDBDic.Values)
            {
                db.SetWeight(weight);
            }
        }
    }
}
