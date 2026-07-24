using CitizenUI;
using Game.RestAPI;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using CharacterControl;

public class UICharacterController : MonoBehaviour
{

    private ObjUICharacterBase ChatacterBase = null;
    //private Animator CharacterAnim;
    private GameObject ObjCharacter;
    private CCharacterController charController;
    private CMemberAvatarController MemberController;
    private GameObject UI_CharObj;

    private SingleAssignmentDisposable WearAnimDisposer = null;
    private const float VAMKIDZ_IMPROVE_CAM_ORTHO_SIZE = 26f;
    private const float VAMKIDZ_IMPROVE_CHAR_OFFSET_Y = 14f;

    public GameObject GetRotationObject
    {
        get
        {
            if (ChatacterBase != null)
            {
                return ChatacterBase.CharacterParent;
            }

            return null;
        }
    }

    public void SetUICharacterBase()
    {
        if (UI_CharObj == null)
        {
            GameObject objLoad = CResourceManager.Instance.LoadObject("obj_ui_character_base") as GameObject;
            UI_CharObj = GameObject.Instantiate(objLoad);
            ChatacterBase = UI_CharObj.GetComponent<ObjUICharacterBase>();
        }
    }

    public void SetVamKidzImproveUICharBase()
    {
        SetUICharacterBase();
        Camera cam = UI_CharObj.GetComponentInChildren<Camera>();
        if (cam != null)
        {
            Vector3 camPos = cam.transform.localPosition;
            camPos.y = VAMKIDZ_IMPROVE_CHAR_OFFSET_Y;
            cam.transform.localPosition = camPos;
            if (cam.orthographic)
            {
                cam.orthographicSize = VAMKIDZ_IMPROVE_CAM_ORTHO_SIZE;
            }
        }
    }

    public GameObject GetUICharacterBase()
    {
        if (ChatacterBase != null)
        {
            return UI_CharObj;
        }

        return null;
    }

    public void SetUICharacterBase(Vector3 pos)
    {
        SetUICharacterBase();
        ChatacterBase.SetPosition(pos);
    }

    public void SetCharacter(CitizenUIData data)
    {
        //캐릭터 3D Object 세팅하기
        CResourceData resData = CResourceManager.Instance.GetResourceData(data.TData.PrefabResPath);
        GameObject objLoadCharacter = resData.Load<GameObject>(gameObject);
        //GameObject objLoadCharacter = CResourceManager.Instance.LoadObject(strRes) as GameObject;
        ObjCharacter = Utility.AddChild(ChatacterBase.CharacterParent, objLoadCharacter);


        charController = ObjCharacter.GetComponent<CCharacterController>();
        if (charController == null)
        {
            charController = ObjCharacter.AddComponent<CCharacterController>();
        }

        int fatigabilityGrade = 0;
        CitizenList citizenInfo = CCitizenServerDataManager.Instance.GetCitizenInfo((int)data.GDID);
        if(citizenInfo != null)
        {
            fatigabilityGrade = citizenInfo.fatigability_grade;
        }
        charController.Init(data.GDID, data.TData, fatigabilityGrade);
        //charController.CreateShadow(ObjCharacter, SNGDefines.CITIZEN_OBJECT_LAYER_NAME_UI, SNGDefines.CITIZEN_SHADOW_TRACE_OBJ_NAME_UI);
    }

    public void SetMember(CMemberAvatar avatar)
    {
        avatar.SetCurEquipSItemDic();
        avatar.SetBaseMemberAvatar(ChatacterBase.CharacterParent, true);
        avatar.EquipAllStylingItems();
        avatar.SetAvatarObjDefaultFaceTexture();
        avatar.SetAvatarObjPosition(new Vector3(0, -0.1f, 0));
        MemberController = avatar.MemberAvatarController;
    }
    public void PlayCharacterTouchAnimation()
    {
        if (charController != null)
        {
            charController.PlayTouchAnim(false);

            float clipLength = charController.GetCitizenTouchAnimLength();
            var disposer = new SingleAssignmentDisposable();
            disposer.Disposable = Observable.Timer(TimeSpan.FromSeconds(clipLength))
            .Subscribe(_ =>
            {
                charController.PlayIdleAnim();
                disposer.Dispose();
            })
            .AddTo(this);
        }
    }

    public void PlayMemberTouchSignatureAnimation()
    {
        CSoundControl.Instance.EffectPlay(SOUND_ID.SFX_SNG_FRIENDS_TOUCH);
        MemberController.PlayTouchSignatureAnim(true);
    }

    public void PlayAnim(string param)
    {
        if (MemberController != null)
        {
            MemberController.SetTrigger(param);
        }
    }

    public void SwapWearAnimation(STYLE_ITEM_TYPE itemType)
    {
        if (MemberController != null)
        {
            MemberController.PlayWearAnim(itemType);
        }
    }

    public void ReleaseBaseCharObj()
    {
        if (UI_CharObj != null)
        {
            Utility.Destroy(UI_CharObj);
            UI_CharObj = null;
        }
    }

    public void ReleaseCharacter()
    {
        if (ObjCharacter != null)
        {
            Utility.Destroy(ObjCharacter);
            //CharacterAnim = null;
        }

    }

    public void Dispose()
    {
        ReleaseCharacter();

        if (ChatacterBase != null)
        {
            Utility.Destroy(ChatacterBase.gameObject);
            ChatacterBase = null;
        }

    }

}
