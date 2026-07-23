#if UNITY_EDITOR
			
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using Game.RestAPI;
using Unity.VisualScripting;

public class CMemberControlEdit : MonoBehaviour
{
    [SerializeField] private GameObject SkilObj;
    public CMemberAvatar MemberAvatar;
    public Animator MemberAnimator;
    private Dictionary<long, StylingItemData> MemberStylingItemDic = null;


    public void Init()
    {
        MemberAnimator = gameObject.GetComponentsInChildren<Animator>().FirstOrDefault();
    }

    public void SetMemberAvatar()
    {
        GameObject avatarRootObj = this.gameObject;// transform.Find("Avatar").gameObject;
        LoadTempData();
        List<AvatarList> avatarInfos = new List<AvatarList>();
        AvatarList avatarInfo = new AvatarList();
        // avatarInfo.gdid = 200;
        // avatarInfo.parts1 = 100;
        // avatarInfo.parts2 = 101;
        // avatarInfo.parts3 = 102;
        // avatarInfo.parts4 = 103;
        // avatarInfo.parts5 = 104;
        // avatarInfos.Add(avatarInfo);

        MEMBER_TYPE mType = (MEMBER_TYPE)avatarInfo.character_id;
        MemberAvatar = new CMemberAvatar(mType);
        MemberAvatar.SetToolMemberAvatarInfo(avatarInfo, avatarRootObj, MemberStylingItemDic);        
        
        //MemberAvatar.EquipAllStylingItems();

        GameObject avatarObj = MemberAvatar.GetAvatarObj();
        MemberAnimator = avatarObj.GetComponent<Animator>();

        CapsuleCollider collider = avatarObj.AddComponent<CapsuleCollider>();
        collider.center = new Vector3(0, 1.0f, 0);
        collider.radius = 0.7f;
        collider.height = 2.0f;

        avatarObj.AddComponent<CharacterRotation>();
    }

    private void LoadTempData()
    {        
        MemberStylingItemDic = new Dictionary<long, StylingItemData>();

        string[] resPath = new string[5]
        {
            "ch_sunoo_0002_hair",
            "ch_common_0001_body",
            "ch_m_hairacc_rabbit_01",
            "ch_acc_face_002",
            "ch_obj_cupid_wings"
        };

        for (int index = 0; index < 4; index++)
        {
            StylingItemData data = new StylingItemData
            {
                ID = 100 + index,
                NameStrID = "parts_name_string_id_" + index,
                MemberType = MEMBER_TYPE.JUNGWON,
                ItemType = (STYLE_ITEM_TYPE)(index + 1),
                ResourcePath = resPath[index]
            };

            if (MemberStylingItemDic.ContainsKey(data.ID) == false)
            {
                MemberStylingItemDic.Add(data.ID, data);
            }
        }
    }

    public void ChangeStyling(GameObject hairObj, GameObject skinObj, GameObject haObj, GameObject faObj, GameObject baObj)
    {
        Debug.Log("ChangeStyling");

        //Hair Type
        if (hairObj != null)
        {
            SetItemInfo(STYLE_ITEM_TYPE.HAIR, hairObj);
        }

        //Skin Type
        if (skinObj != null)
        {         
            SetItemInfo(STYLE_ITEM_TYPE.SKIN, skinObj);   
        }

        //Hair Acc Type
        if (haObj != null)
        {
            SetItemInfo(STYLE_ITEM_TYPE.ACC_HEAD, haObj);
        }

        //Face Acc Type
        if (faObj != null)
        {
            SetItemInfo(STYLE_ITEM_TYPE.ACC_FACE, faObj);
        }

        //Body Acc Type
        if (baObj != null)
        {
            SetItemInfo(STYLE_ITEM_TYPE.ACC_BODY, baObj);
        }
    }

    public void SetItemInfo(STYLE_ITEM_TYPE type, GameObject itemObj)
    {
        StylingItemData itemData = CMemberAvatarDataManager.Instance.GetStylingItemDataByResPath(itemObj.name);
        StylingItemInfo _itemInfo = new StylingItemInfo(itemData, MemberAvatar.GetAvatarObj());
        _itemInfo.LoadItemObjForTool(itemObj);

        MemberAvatar.SetCurEquipStylingItemDic(type, _itemInfo);

        MemberAvatar.EquipStylingItems(type, _itemInfo, 1.0f);
    }
     


    public void SetTrigger(string trigger)
    {
        MemberAnimator.SetTrigger(trigger);
    }
}
#endif