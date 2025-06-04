//#define CUBEMAP_TEST
//#define PLANE_SHADOW

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using KSPlugins;

using Object = UnityEngine.Object;
//using Xft;



public enum eCharacter : byte
{
    NONE = 0,
    eWarrior = 1,
    eWizard,
    eArcher,
    eNPC,
    //소환수 타입 추가
    eHERONPC,
}

public enum eFACTORTYPE : byte
{
    ATTACK = 1,
    DEFENSE,
    HP
}

public enum eEquipment : byte
{
    eHead = 0,
    eUpperBody,
    eLowerBody,
    eArm,
    eLeg,
    eLeftWeapon,
    eRightWeapon,
}

public class CharacterFactory
{
    public static int magician_wpCnt;
    private static List<GameObject> newParts = new List<GameObject>();
    private static Transform leftWeaponParent = null;
    private static Transform rightWeaponParent = null;
    private static Transform leftWeaponParent_UI = null;
    private static Transform rightWeaponParent_UI = null;
    private static Transform[] children = null;
    private static Transform[] childrenMagicianWeapon = null;
    private static Transform[] magicianWeapons = new Transform[5];
    private static List<GameObject> magWeaponLists { get; set; }
    private static Dictionary<ItemData.eITEM_SUB_KIND, List<GameObject>> m_MagWeapons = new Dictionary<ItemData.eITEM_SUB_KIND, List<GameObject>>();

    public static GameObject CreateCharacter(PlayerInfo pInfo, SceneManager.eMAP_CREATE_TYPE eCreateType)
    {
        //Debug.Log("CreateCharacter jobtype : " + pInfo.jobType);

        GameObject prefab = InstantiateBasePrefab(pInfo.jobType);
        UtilManager.instance.ResetShader(prefab);

        //Navigation tmpNavi = null;

        SceneManager.instance.SetCurMapCreateType(eCreateType);

        CharacterBase character = prefab.AddComponent<CharacterBase>();

        character.m_CharacterType = pInfo.jobType;
        switch (pInfo.jobType)
        {
            case eCharacter.eWarrior:
                prefab.tag = "Warrior";
                break;
            case eCharacter.eWizard:
                prefab.tag = "Magician";
                break;
            case eCharacter.eArcher:
                prefab.tag = "Archer";
                break;
        }
         
        switch (SceneManager.instance.GetCurMapCreateType())
        {
            case SceneManager.eMAP_CREATE_TYPE.IN_GAME:
                {
                    EquipAllParts(prefab, pInfo, eCreateType);

                    AddCollider(prefab);
                    AddDamageUI(prefab);
                    //pInfo.shadowObj = InGameManager.AddShadow(prefab);//, false);
                    AddSkillSender(prefab);
                    if (PlayerManager.instance.m_PlayerInfo[PlayerManager.MYPLAYER_INDEX].strName.Equals(pInfo.strName))
                    {
                        pInfo.m_Navigation = AddNavigation(prefab);
                        pInfo.naviObj = pInfo.m_Navigation.m_trNavigtion.gameObject;
                    }
                    AddAuto(prefab);
                    //prefab.AddComponent<MeshCombine>();
                    SetLayer(prefab, LayerMask.NameToLayer("Character"));
#if CUBEMAP_TEST
                    ShaderCubeMapSetting(prefab, false);
#endif
                }
                break;
            case SceneManager.eMAP_CREATE_TYPE.UI:
                {
                    EquipAllParts(prefab, pInfo, eCreateType);

                    AddCollider(prefab);
                    Rigidbody rig = AddRigidBody(prefab);
                    rig.useGravity = false;

                    AddDragRolling(prefab);

                    ShaderUISetting(prefab, false);
#if CUBEMAP_TEST
                    ShaderCubeMapSetting(prefab, true);
#endif
                }
                break;
        }
        
        character.m_AnimationInterface = prefab.AddComponent<AnimationInterface>();
        character.m_AnimationInterface.effectSkillManager = prefab.AddComponent<EffectSkillManager>();
        character.m_AnimationInterface.effectSkillManager.gobjCharacter = prefab;

        return prefab;
    }

    protected static void SetLayer(GameObject goCharacter, int nLayer)
    {
        if (goCharacter == null) return;

        Transform[] childs = goCharacter.GetComponentsInChildren<Transform>();

        for ( int i = 0; i < childs.Length; i++ )
        {
            childs[i].gameObject.layer = nLayer;
        }
    }

    protected static Transform FindChild(Transform parent, string name)
    {
        if (parent == null)             return null;
        if (string.IsNullOrEmpty(name)) return null;

        Transform result = null;
        Transform childs = parent.GetComponentInChildren<Transform>();

        if(childs != null)
        {
            for (int i = 0; i < childs.childCount; i++)
            {
                if (childs.GetChild(i).name.Equals(name))
                    return childs.GetChild(i);

                result = childs.GetChild(i);
                result = FindChild(result, name);

                if (result != null) return result;
            }
        }

        return null;
    }
    #region     PRIVATE METHODS
    private static GameObject InstantiateBasePrefab(eCharacter jType)
    {
        string path = "";
        switch (jType)
        {
            case eCharacter.eWarrior:
                path = "Item/Warrior/Warrior";;
                break;
            case eCharacter.eWizard:
                path = "Item/Magician/Magician";
                break;
            case eCharacter.eArcher:
                path = "Item/Archer/Archer";
                break;
        }
        return KSPlugins.KSResource.AssetBundleInstantiate < GameObject>(path);
    }

    public static void ClearNewPartsList()
    {
        newParts.Clear();
    }

    public static void PreLoadBasePrefab(eCharacter jType)
    {
        string path = "";
        switch (jType)
        {
            case eCharacter.eWarrior:
                path = "Item/Warrior/Warrior"; ;
                break;
            case eCharacter.eWizard:
                path = "Item/Magician/Magician";
                break;
            case eCharacter.eArcher:
                path = "Item/Archer/Archer";
                break;
        }

        KSPlugins.KSResource.PreLoadResource(path,true,false);
    }

    private static void SetWeaponBone(GameObject character)
    {
        children = character.GetComponentsInChildren<Transform>();

        for (int i = 0; i < children.Length; i++)
        {
            if (children[i].CompareTag("Hand_Left"))
            {
                leftWeaponParent = children[i];
                break;
            }
        }
        for (int i = 0; i < children.Length; i++)
        {
            if (children[i].CompareTag("Hand_Right"))
            {
                rightWeaponParent = children[i];
                break;
            }
        }
    }


    private static void SetWeaponBoneUI(GameObject character)
    {
        children = character.GetComponentsInChildren<Transform>();

        for (int i = 0; i < children.Length; i++)
        {
            if (children[i].CompareTag("Hand_Left"))
            {
                leftWeaponParent_UI = children[i];
                break;
            }
        }
        for (int i = 0; i < children.Length; i++)
        {
            if (children[i].CompareTag("Hand_Right"))
            {
                rightWeaponParent_UI = children[i];
                break;
            }
        }
    }

    //private static void SetMagicianWeaponBone(GameObject goCharacter, PlayerInfo pInfo)
    //{
    //    if (pInfo.jobType != eCharacter.eWizard)
    //        return;

    //    //magician's Back weapon 5EA
    //    childrenMagicianWeapon = goCharacter.GetComponentsInChildren<Transform>();

    //    for (int i = 0; i < childrenMagicianWeapon.Length; i++)
    //    {
    //        if (childrenMagicianWeapon[i].CompareTag("Mag_Weapon_M"))
    //        {
    //            if (magician_wpCnt < magicianWeapons.Length)
    //            {
    //                magicianWeapons[magician_wpCnt++] = childrenMagicianWeapon[i];
    //            }
    //        }
    //    }
    //}

    public static void EquipAllParts(GameObject character, PlayerInfo pInfo, SceneManager.eMAP_CREATE_TYPE cType = SceneManager.eMAP_CREATE_TYPE.IN_GAME)
    {
        CharacterBase CharBase = character.GetComponent<CharacterBase>();

        if (CharBase != null)
        {
            CharBase.m_preFabRenderer.Clear();
        }

        if (cType == SceneManager.eMAP_CREATE_TYPE.IN_GAME)
        {
            m_MagWeapons.Clear();
            ClearNewPartsList();
            SetWeaponBone(character);                       
        }
        else if(cType == SceneManager.eMAP_CREATE_TYPE.UI)
        {
            SetWeaponBoneUI(character);
        }



        {
            ItemData itemData = itemInfo.Value;

            EquipParts(pInfo, character, itemData, itemInfo.Key, cType);
        }

        if (cType == SceneManager.eMAP_CREATE_TYPE.IN_GAME)
        {
            if (newParts.Count > 0 && CharBase != null)
            {
                for (int i = 0; i < newParts.Count; ++i)
                {
                    Renderer[] renderers = newParts[i].GetComponentsInChildren<Renderer>();

                    for (int j = 0; j < renderers.Length; ++j)
                    {
                        CharBase.m_preFabRenderer.Add(renderers[j]);
                    }
                }
            }
        }
    }

    private static GameObject GetPartsObj(eCharacter job, ItemData.eITEM_SUB_KIND subKind, int modelType)
    {
        switch(job)
        {
            case eCharacter.eWarrior:
                for(int i=0; i<CDataManager.m_WarriorItemModelDic[subKind].Count; ++i)
                {
                    if(modelType == CDataManager.m_WarriorItemModelDic[subKind][i].ModelType)
                    {
                        return CDataManager.m_WarriorItemModelDic[subKind][i].ModelObject;
                    }
                }
                break;
            case eCharacter.eWizard:
                for (int i = 0; i < CDataManager.m_MagicianItemModelDic[subKind].Count; ++i)
                {
                    if (modelType == CDataManager.m_MagicianItemModelDic[subKind][i].ModelType)
                    {
                        return CDataManager.m_MagicianItemModelDic[subKind][i].ModelObject;
                    }
                }
                break;
        }
        return null;
    }

    private static string GetPartsObjPath(eCharacter job, ItemData.eITEM_SUB_KIND subKind, int modelType)
    {
        switch (job)
        {
            case eCharacter.eWarrior:
                for (int i = 0; i < CDataManager.m_WarriorItemModelDic[subKind].Count; ++i)
                {
                    if (modelType == CDataManager.m_WarriorItemModelDic[subKind][i].ModelType)
                    {
                        return CDataManager.m_WarriorItemModelDic[subKind][i].ModelObjPath;
                    }
                }
                break;
            case eCharacter.eWizard:
                for (int i = 0; i < CDataManager.m_MagicianItemModelDic[subKind].Count; ++i)
                {
                    if (modelType == CDataManager.m_MagicianItemModelDic[subKind][i].ModelType)
                    {
                        return CDataManager.m_MagicianItemModelDic[subKind][i].ModelObjPath;
                    }
                }
                break;
        }
        return null;
    }

    public static void SetHideEquipParts(bool bActive)
    {

        //zunghoon Modefy start 
        //2017.0308 foreach for 문 으로 수정
        /*
        foreach(KeyValuePair<ItemData.eITEM_SUB_KIND, GameObject> obj in PlayerManager.instance.m_PlayerInfo[PlayerManager.MYPLAYER_INDEX].equipedItems)
        {
            if (obj.Value != null)
            {
                obj.Value.SetActive(bActive);
            }
        }
        */
        var enumerator = PlayerManager.instance.m_PlayerInfo[PlayerManager.MYPLAYER_INDEX].equipedItems.GetEnumerator();
        while (enumerator.MoveNext())
        {
            GameObject obj = enumerator.Current.Value;
            if (obj != null)
            {
                obj.SetActive(bActive);
            }
        }
        
        //zunghoon Modefy ENd 



        //장판
        PlayerManager.instance.m_PlayerInfo[PlayerManager.MYPLAYER_INDEX].m_Navigation.m_FxNaviCircle.SetActive(bActive);

        //그림자
        //PlayerManager.instance.m_PlayerInfo[PlayerManager.MYPLAYER_INDEX].shadowObj.SetActive(bActive);

        //법사 팔무기
        if(PlayerManager.instance.m_PlayerInfo[PlayerManager.MYPLAYER_INDEX].jobType == eCharacter.eWizard)
        {
            //if(m_MagWeapons.ContainsKey(ItemData.eITEM_SUB_KIND.HAND_LEFT))
            {
                for (int i = 0; i < m_MagWeapons[ItemData.eITEM_SUB_KIND.HAND_LEFT].Count; ++i)
                {
                    m_MagWeapons[ItemData.eITEM_SUB_KIND.HAND_LEFT][i].SetActive(bActive);
                }
            }
        }
    }

    public static void EquipParts(PlayerInfo pInfo, GameObject goCharacter, ItemData item, ItemData.eITEM_SUB_KIND eItemSubKind, SceneManager.eMAP_CREATE_TYPE eCreateType = SceneManager.eMAP_CREATE_TYPE.IN_GAME)
    {
        if ( goCharacter != null && item != null )
        {
            if (item.ItemSubKind == ItemData.eITEM_SUB_KIND.EARRING ||
                item.ItemSubKind == ItemData.eITEM_SUB_KIND.NECKLACE ||
                item.ItemSubKind == ItemData.eITEM_SUB_KIND.RING)
                return;

            //UnityEngine.Debug.Log("### EquipParts 111 subKind = " + eItemSubKind + "/ createType = " + eCreateType);
            if (eCreateType == SceneManager.eMAP_CREATE_TYPE.IN_GAME)
            {
                if (pInfo.equipedItems[eItemSubKind] != null)
                {
                    //UnityEngine.Debug.Log("### EquipParts 222");
                    if (pInfo.curEquipItems.m_dicItems[eItemSubKind].ModelType == item.ModelType)
                    {
                        return;
                    }
                }
            }

            magician_wpCnt = 0;


            if (eCreateType == SceneManager.eMAP_CREATE_TYPE.IN_GAME)
            {
                //UnityEngine.Debug.Log("### EquipParts 333");
                switch (pInfo.jobType)
                {
                    case eCharacter.eWarrior:
                        /// 기존의 장착하고 있던 아이템 제거
                        if (pInfo.equipedItems[eItemSubKind] != null)
                        {
                            //pInfo.equipedItems[eItemSubKind].SetActive(false);
                            //pInfo.equipedItems[eItemSubKind].transform.parent = InGameManager.instance.m_PlayerAllItemRoot.transform;
                            //pInfo.equipedItems[eItemSubKind].transform.localPosition = Vector3.zero;

                            GameObject.DestroyImmediate(pInfo.equipedItems[eItemSubKind]);
                        }
                        break;
                    case eCharacter.eWizard:
                        if(eItemSubKind != ItemData.eITEM_SUB_KIND.HAND_LEFT)
                        {
                            ItemData.eITEM_SUB_KIND tmpSubKind = eItemSubKind;

                            if(eItemSubKind == ItemData.eITEM_SUB_KIND.HAND_RIGHT)
                            {
                                tmpSubKind = ItemData.eITEM_SUB_KIND.HAND_LEFT;
                            }

                            if (pInfo.equipedItems[tmpSubKind] != null)
                            {

                                GameObject.DestroyImmediate(pInfo.equipedItems[tmpSubKind]);
                            }

                            if (m_MagWeapons.ContainsKey(tmpSubKind))
                            {
                                for (int i = 0; i < m_MagWeapons[tmpSubKind].Count; i++)
                                {
                                    if (m_MagWeapons[tmpSubKind][i] != null)
                                    {
                                        GameObject.DestroyImmediate(m_MagWeapons[tmpSubKind][i]);
                                    }
                                }
                                m_MagWeapons[tmpSubKind].Clear();
                            }
                        }
                        break;
                }
            }


            Transform leftParent = null;
            Transform rightParent = null;

            GameObject instObj = KSPlugins.KSResource.AssetBundleInstantiate<GameObject>(GetPartsObjPath(pInfo.jobType, item.ItemSubKind, item.ModelType));

            if (eCreateType == SceneManager.eMAP_CREATE_TYPE.IN_GAME)
            {
                // gunny delete 20170627
                instObj.transform.parent = InGameManager.instance.m_PlayerAllItemRoot.transform;
                // gunny delete 20170627 end
                leftParent = leftWeaponParent;
                rightParent = rightWeaponParent;
            }
            else if(eCreateType == SceneManager.eMAP_CREATE_TYPE.UI)
            {
                leftParent = leftWeaponParent_UI;
                rightParent = rightWeaponParent_UI;
            }
            instObj.transform.localPosition = Vector3.zero;
            instObj.transform.localScale = Vector3.one;
            


#if PLANE_SHADOW
            List<Material> Shadowmats = new List<Material>();
            Renderer[] Renderers = instObj.GetComponentsInChildren<Renderer>();
            int nCount = Renderers.Length;

            for (int i = 0; i < nCount; ++i)
            {
                int iMatCount = Renderers[i].materials.Length;

                Material[] mats = new Material[iMatCount+1];

                for( int j = 0 ; j < iMatCount ; ++j )
                {
                    mats[j] = Renderers[i].materials[j];
                }
                mats[iMatCount] = new Material(Shader.Find("Blame/PlaneShadow"));
                Shadowmats.Add( mats[iMatCount] );

                Renderers[i].sharedMaterials = mats;
            }
#endif
            switch (item.ItemSubKind)
            {
                case ItemData.eITEM_SUB_KIND.HAND_LEFT:
                    {
                        switch(pInfo.jobType)
                        {
                            case eCharacter.eWarrior:
                                if (leftParent != null)
                                {
                                    instObj.transform.parent = leftParent;
                                    instObj.transform.localPosition = Vector3.zero;
                                    instObj.transform.localRotation = Quaternion.identity;
                                    instObj.transform.localScale = Vector3.one;
                                    instObj.tag = item.ItemSubKind.ToString();
                                    instObj.name = "Weapon_Shield";
                                    if (eCreateType == SceneManager.eMAP_CREATE_TYPE.UI)
                                    {
                                        SetLayer(instObj, LayerMask.NameToLayer("UIMenuCharacter"));
                                    }
                                    else if (eCreateType == SceneManager.eMAP_CREATE_TYPE.IN_GAME)
                                    {
                                        SetLayer(instObj, LayerMask.NameToLayer("Character"));
                                        pInfo.equipedItems[ItemData.eITEM_SUB_KIND.HAND_LEFT] = instObj;
                                        newParts.Add(instObj);
                                        AddEmptyPrefabRanderer(instObj);
                                    }
                                }
                                break;
                            case eCharacter.eWizard:
                                break;
                        }
                    }
                    break;
                case ItemData.eITEM_SUB_KIND.HAND_RIGHT:
                    {
                        switch(pInfo.jobType)
                        {
                            case eCharacter.eWarrior:
                                if (rightParent != null)
                                {
                                    instObj.transform.parent = rightParent;
                                    instObj.transform.localPosition = Vector3.zero;
                                    instObj.transform.localRotation = Quaternion.identity;
                                    instObj.transform.localScale = Vector3.one;
                                    instObj.tag = item.ItemSubKind.ToString();
                                    instObj.name = "Weapon_Sword";
                                    if (eCreateType == SceneManager.eMAP_CREATE_TYPE.UI)
                                    {
                                        SetLayer(instObj, LayerMask.NameToLayer("UIMenuCharacter"));
                                    }
                                    else if (eCreateType == SceneManager.eMAP_CREATE_TYPE.IN_GAME)
                                    {
                                        SetLayer(instObj, LayerMask.NameToLayer("Character"));
                                        pInfo.equipedItems[ItemData.eITEM_SUB_KIND.HAND_RIGHT] = instObj;

                                        newParts.Add(instObj);
                                        AddEmptyPrefabRanderer(instObj);
                                    }
                                }
                                break;
                            case eCharacter.eWizard:
                                if (leftParent != null)
                                {
                                    instObj.transform.parent = leftParent;
                                    instObj.transform.localPosition = Vector3.zero;
                                    instObj.transform.localRotation = Quaternion.identity;
                                    instObj.transform.localScale = Vector3.one;
                                    instObj.tag = item.ItemSubKind.ToString();
                                    instObj.name = "Magic_Bracelet";
                                    //SetLayer(instObj, LayerMask.NameToLayer("Character"));
                                    if (eCreateType == SceneManager.eMAP_CREATE_TYPE.UI)
                                    {
                                        SetLayer(instObj, LayerMask.NameToLayer("UIMenuCharacter"));
                                    }
                                    else if (eCreateType == SceneManager.eMAP_CREATE_TYPE.IN_GAME)
                                    {
                                        SetLayer(instObj, LayerMask.NameToLayer("Character"));
                                        pInfo.equipedItems[ItemData.eITEM_SUB_KIND.HAND_LEFT] = instObj;
                                        if (m_MagWeapons.ContainsKey(ItemData.eITEM_SUB_KIND.HAND_LEFT) == false)
                                        {
                                            magWeaponLists = new List<GameObject>();
                                            m_MagWeapons.Add(ItemData.eITEM_SUB_KIND.HAND_LEFT, magWeaponLists);
                                        }
                                        m_MagWeapons[ItemData.eITEM_SUB_KIND.HAND_LEFT].Add(instObj);

                                        newParts.Add(instObj);
                                        AddEmptyPrefabRanderer(instObj);
                                    }
                                    
                                    GameObject instObjMirror = null;
                                    instObjMirror = GameObject.Instantiate(instObj);
                                    instObjMirror.transform.parent = rightParent;
                                    instObjMirror.transform.localPosition = Vector3.zero;
                                    instObjMirror.transform.localRotation = Quaternion.identity;
                                    instObjMirror.transform.localScale = Vector3.one;
                                    instObjMirror.tag = item.ItemSubKind.ToString();
                                    instObjMirror.name = "Magic_Bracelet";
                                    //SetLayer(instObjMirror, LayerMask.NameToLayer("Character"));
                                    if (eCreateType == SceneManager.eMAP_CREATE_TYPE.UI)
                                    {
                                        SetLayer(instObjMirror, LayerMask.NameToLayer("UIMenuCharacter"));
                                    }
                                    else if (eCreateType == SceneManager.eMAP_CREATE_TYPE.IN_GAME)
                                    {
                                        SetLayer(instObjMirror, LayerMask.NameToLayer("Character"));
                                        m_MagWeapons[ItemData.eITEM_SUB_KIND.HAND_LEFT].Add(instObjMirror);
                                    }
                                }
                                break;
                        }
                    }
                    break;
                case ItemData.eITEM_SUB_KIND.CHEST:
                case ItemData.eITEM_SUB_KIND.GLOVES:
                case ItemData.eITEM_SUB_KIND.HELMET:
                case ItemData.eITEM_SUB_KIND.PANTS:
                case ItemData.eITEM_SUB_KIND.SHOES:
                    {
                        SkinnedMeshRenderer[] renderers = instObj.GetComponentsInChildren<SkinnedMeshRenderer>();
                        SkinnedMeshRenderer smr = null;
                        GameObject equipObj = null;
                        
                        for (int j = 0; j < renderers.Length; j++)
                        {
                            Transform[] bones = new Transform[renderers[j].bones.Length];
                            for (int i = 0; i < bones.Length; ++i)
                            {
                                bones[i] = FindChild(goCharacter.transform, renderers[j].bones[i].name);
                            }

                            equipObj = new GameObject(instObj.name);

                            smr = equipObj.AddComponent<SkinnedMeshRenderer>();
                            smr.bones = bones;
                            smr.sharedMesh = renderers[j].sharedMesh;
                            smr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                            smr.receiveShadows = true;
                            smr.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
                            smr.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
                            smr.materials = renderers[j].sharedMaterials;

                            equipObj.transform.parent = goCharacter.transform;
                            equipObj.transform.localPosition = Vector3.zero;
                            equipObj.transform.localRotation = Quaternion.identity;
                            equipObj.transform.localScale = Vector3.one;
                            equipObj.tag = item.ItemSubKind.ToString();
                                                        
                            if (eCreateType == SceneManager.eMAP_CREATE_TYPE.UI)
                            {
                                SetLayer(equipObj, LayerMask.NameToLayer("UIMenuCharacter"));
                            }
                            else if(eCreateType == SceneManager.eMAP_CREATE_TYPE.IN_GAME)
                            {
                                SetLayer(equipObj, LayerMask.NameToLayer("Character"));
                                pInfo.equipedItems[item.ItemSubKind] = equipObj;
                                newParts.Add(equipObj);
                                AddEmptyPrefabRanderer(equipObj);
                            }

                        }


                    }
                    break;
            }

            
            if (InventoryManager.instance.m_IngameEquipItem)
            {
                if(pInfo.jobType == eCharacter.eWizard)
                {
                    if(item.ItemSubKind == ItemData.eITEM_SUB_KIND.HAND_RIGHT)
                    {
                        if(pInfo.equipedItems.ContainsKey(ItemData.eITEM_SUB_KIND.HAND_RIGHT))
                        {
                            if (pInfo.equipedItems[ItemData.eITEM_SUB_KIND.HAND_RIGHT] == null)
                            {
                                SetMeshRenderer(pInfo, ItemData.eITEM_SUB_KIND.HAND_LEFT);
                            }
                            else
                            {
                                SetMeshRenderer(pInfo, item.ItemSubKind);
                            }
                        }
                        else
                        {
                            SetMeshRenderer(pInfo, item.ItemSubKind);
                        }
                    }
                }
                else
                {
                    SetMeshRenderer(pInfo, item.ItemSubKind);
                }
            }
        }
    }

    private static int GetDamageEffRendererEmptyIndex()
    {
        CharacterBase cBase = PlayerManager.instance.m_PlayerInfo[PlayerManager.MYPLAYER_INDEX].playerCharBase;
        if (cBase == null)
            return -1;

        foreach(KeyValuePair<Renderer, Material[]> mat in cBase.m_CharacterDamageEffect.m_DefaultMaterial)
        {
            if(mat.Key == null)
            {
                cBase.m_CharacterDamageEffect.m_DefaultMaterial.Remove(mat.Key);
                break;
            }
        }

        for (int i = 0; i < cBase.m_CharacterDamageEffect.m_MeshRenderer.Length; ++i)
        {
            if (cBase.m_CharacterDamageEffect.m_MeshRenderer[i] == null)
            {
                return i;
            }
        }
        return -1;
    }

    private static void AddEmptyPrefabRanderer(GameObject equipObj)
    {
        CharacterBase cBase = PlayerManager.instance.m_PlayerInfo[PlayerManager.MYPLAYER_INDEX].playerCharBase;
        int index = -1;
        if (cBase == null)
            return;

        for (int i = 0; i < cBase.m_preFabRenderer.Count; ++i)
        {
            if(cBase.m_preFabRenderer[i] == null)
            {
                Renderer[] renderers = equipObj.GetComponentsInChildren<Renderer>();

                for (int j = 0; j < renderers.Length; ++j)
                {
                    cBase.m_preFabRenderer[i] = renderers[j];
                    index = GetDamageEffRendererEmptyIndex();
                    if(index != -1)
                    {
                        cBase.m_CharacterDamageEffect.m_MeshRenderer[index] = renderers[j];
                    }
                }                
            }
        }


    }

    private static void SetMeshRenderer(PlayerInfo pInfo, ItemData.eITEM_SUB_KIND itemSubKind)
    {

        if(pInfo.equipedItems.ContainsKey(itemSubKind))
        {
            Renderer[] renderers = pInfo.equipedItems[itemSubKind].GetComponentsInChildren<Renderer>();


            //for (int i = 0; i < pInfo.playerCharBase.m_preFabRenderer.Count; i++)
            //{
            //    if (pInfo.playerCharBase.m_preFabRenderer[i].gameObject.CompareTag(pInfo.equipedItems[itemSubKind].tag))
            //    {
            //        pInfo.playerCharBase.m_preFabRenderer[i] = renderers[0];
            //        pInfo.playerCharBase.m_CharacterDamageEffect.SetMeshRenderer(pInfo.playerCharBase.m_preFabRenderer[i], i);
            //        InventoryManager.instance.m_IngameEquipItem = false;
            //        break;
            //    }
            //}



            ////UnityEngine.Debug.Log("### EquipParts 23  23");
            if(PlayerManager.instance.m_PlayerInfo[PlayerManager.MYPLAYER_INDEX].strName.Equals(pInfo.strName))
            {
                for (int i = 0; i < PlayerManager.instance.m_PlayerInfo[PlayerManager.MYPLAYER_INDEX].playerCharBase.m_preFabRenderer.Count; i++)
                {
                    if (PlayerManager.instance.m_PlayerInfo[PlayerManager.MYPLAYER_INDEX].equipedItems[itemSubKind] != null)
                    {
                        if (PlayerManager.instance.m_PlayerInfo[PlayerManager.MYPLAYER_INDEX].playerCharBase.m_preFabRenderer[i].gameObject.CompareTag(PlayerManager.instance.m_PlayerInfo[PlayerManager.MYPLAYER_INDEX].equipedItems[itemSubKind].tag))
                        {
                            PlayerManager.instance.m_PlayerInfo[PlayerManager.MYPLAYER_INDEX].playerCharBase.m_preFabRenderer[i] = renderers[0];
                            PlayerManager.instance.m_PlayerInfo[PlayerManager.MYPLAYER_INDEX].playerCharBase.m_CharacterDamageEffect.SetMeshRenderer(PlayerManager.instance.m_PlayerInfo[PlayerManager.MYPLAYER_INDEX].playerCharBase.m_preFabRenderer[i], i);
                            InventoryManager.instance.m_IngameEquipItem = false;
                            break;
                        }
                    }
                }
            }
            else
            {
                if (pInfo.playerCharBase == null)
                    return;
                for (int i = 0; i < pInfo.playerCharBase.m_preFabRenderer.Count; i++)
                {
                    if (pInfo.playerCharBase.m_preFabRenderer[i].gameObject.CompareTag(pInfo.equipedItems[itemSubKind].tag))
                    {
                        pInfo.playerCharBase.m_preFabRenderer[i] = renderers[0];
                        pInfo.playerCharBase.m_CharacterDamageEffect.SetMeshRenderer(pInfo.playerCharBase.m_preFabRenderer[i], i);
                        InventoryManager.instance.m_IngameEquipItem = false;
                        break;
                    }
                }
            }
        }
        //UnityEngine.Debug.Log("### EquipParts 22 22");
        InventoryManager.instance.m_IngameEquipItem = false;
    }

    private static Navigation AddNavigation(GameObject character)
    {
        if ( character.GetComponent<Navigation>() == null )
        {
            return character.AddComponent<Navigation>();
        }
        return null;
    }
    private static void AddAuto(GameObject character)
    {
        if (character.GetComponent<AutoAI>() == null)
        {
            character.AddComponent<AutoAI>();
        }
    }

    private static void AddSkillSender(GameObject character)
    {
        if (character.GetComponent<SkillSender>() == null)
            character.AddComponent<SkillSender>();
    }

    private static void AddDamageUI(GameObject character)
    {
        character.AddComponent<DamageUI>();
    }

    private static void AddDragRolling(GameObject goCharacter)
    {
        goCharacter.AddComponent<CharacterDragRolling>();
    }
    private static void AddEmotion(GameObject goCharacter)
    {
        goCharacter.AddComponent<CharacterEmotion>();
    }
    private static void AddLobbyAni(GameObject goCharacter)
    {
        string path = null;
        CharacterBase CharBase = goCharacter.GetComponent<CharacterBase>();

        switch (CharBase.m_CharacterType)
        {
            case eCharacter.eWarrior:
                path = "Character/Warrior/Animations/";
                break;
            case eCharacter.eWizard:
                path = "Character/Magician/Animations/";
                break;
            case eCharacter.eArcher:
                path = "Character/Archer/Animations/";
                break;
        }

        Dictionary<string, string> dic = new Dictionary<string,string>();
        dic.Add("Lobby_Ani_1" , "Lobby_Ani_1");
        dic.Add("Lobby_Ani_2" , "Lobby_Ani_2");
        //Load Basic Stance
        CharBase.ksAnimation.InitCharacterAnimations(path, dic);
    }
    
    private static void AddCollider(GameObject character)
    {
        CapsuleCollider coll = character.AddComponent<CapsuleCollider>();

        coll.center = Vector3.up;
        coll.radius = 0.5f;
        coll.height = 2;// 2.3f;

        if(SceneManager.instance.GetCurMapCreateType() == SceneManager.eMAP_CREATE_TYPE.IN_GAME)
        {
            coll.enabled = false;
        }

    }

    private static Rigidbody AddRigidBody(GameObject character)
    {
        Rigidbody rig = character.AddComponent<Rigidbody>();

        rig.mass = 1;

        rig.useGravity  = true;
        rig.isKinematic = false;
        rig.constraints = RigidbodyConstraints.FreezePositionX
                        | RigidbodyConstraints.FreezePositionY
                        | RigidbodyConstraints.FreezePositionZ
                        | RigidbodyConstraints.FreezeRotationX
                        | RigidbodyConstraints.FreezeRotationY
                        | RigidbodyConstraints.FreezeRotationZ;

        return rig;
    }

    private static void ShaderCubeMapSetting(GameObject p_gobjCharacter, bool p_bCubeMap)
    {
        Cubemap cubMap = Resources.Load("Character/CubeMap/Cubemap_Sunny") as Cubemap;
        Renderer[] Renderers = p_gobjCharacter.GetComponentsInChildren<Renderer>();
        int nCount = Renderers.Length;
        if(p_bCubeMap == true)
        {
            for(int i = 0; i < nCount; ++i)
            {
                Renderers[i].sharedMaterial.SetTexture("_Cubemap", cubMap);
            }
        }
        else
        {
            for (int i = 0; i < nCount; ++i)
            {
                Renderers[i].sharedMaterial.SetTexture("_Cubemap", null);
            }
        }
    }

    public static void ShaderUISetting(GameObject p_gobjCharacter , bool p_bShadow)
    {
        Renderer[] Renderers = p_gobjCharacter.GetComponentsInChildren<Renderer>();
        int nCount = Renderers.Length;

        for (int i = 0; i < nCount; ++i)
        {
            Renderers[i].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            Renderers[i].receiveShadows = p_bShadow;
        }
    }
    #endregion  PRIVATE METHODS

}