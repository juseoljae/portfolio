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
using TGP.Core;
using UniRx;
using UniRx.Triggers;
using UnityEngine.AddressableAssets;


public class AvatarManager : Singleton<AvatarManager>
{
    private AvatarEquipFactory factory;

    private NetAvatarEquipFactory NetFactory;

    private AVATAR_TYPE curSelectEquipAvatarType;

    public AVATAR_TYPE CurSelectEquipAvatarType { get { return curSelectEquipAvatarType; } }
    public AvatarEquipFactory GetFactory { get { return factory; } }

    
    public string strBaseBodyPath = "Avatar/Common/StylingItems/Open/Prefab/Basic_Body.prefab";
    public string strBaseBodyPath_Jisoo = "Avatar/Common/StylingItems/Open/Prefab/JISOO_Basic_Body.prefab";
    public string strBaseBodyPath_Jennie = "Avatar/Common/StylingItems/Open/Prefab/JENNIE_Basic_Body.prefab";
    public string strBaseBodyPath_Lisa = "Avatar/Common/StylingItems/Open/Prefab/LISA_Basic_Body.prefab";
    public string strBaseBodyPath_Rose = "Avatar/Common/StylingItems/Open/Prefab/ROSE_Basic_Body.prefab";


    public CDNConfigService CDNConfig;
    public int DynamicBoneLODCheckDist;

    protected override void Init()
    {
        factory = new AvatarEquipFactory();

        NetFactory = new NetAvatarEquipFactory();


        CDNConfig = CCoreServices.GetCoreService<CDNConfigService>();
        DynamicBoneLODCheckDist = CDNConfig.GameServerConfig.SelectedSetting.Etc.test_num_1;

        CDebug.Log("AvatarManager ihnit~~~~~");
    }

    


    public static void EquipAllPatrsOnNetPlayers(AVATAR_TYPE mType, GameObject rootObj, NetAvatarStyleItem netPlayer)
    {
        CharAnimationEvent character = rootObj.GetComponent<CharAnimationEvent>();

        //acc_face같이 gameobject가 없는 경우가 있어서 data로 가져온다
        foreach (KeyValuePair<STYLE_ITEM_TYPE, CStyleItemAttr> pair in netPlayer.PlayerEquipItemDic)
        {
            if (pair.Value == null || pair.Key == STYLE_ITEM_TYPE.FACE)
            {
                continue;
            }

            if (STYLE_ITEM_TYPE.EFFECT_HAND_RIGHT <= pair.Key && pair.Key <= STYLE_ITEM_TYPE.EFFECT_ROOT_ALL)
            {
                
                Instance.NetFactory.EqiupEffectParts(netPlayer.PlayerEquipItemObjDic[pair.Key], pair.Key, netPlayer, character);
            }
            else if (STYLE_ITEM_TYPE.ACC_FACE == pair.Key)
            {

                Transform trsFace = rootObj.transform.Find("Basic_Face");
                if (trsFace != null)
                {
                    Instance.NetFactory.SetMakeUpFaceAcc(pair.Value, trsFace.gameObject);
                }
            }
            else
            {
                if (pair.Key == STYLE_ITEM_TYPE.ACC_PROP)
                {
                    Instance.NetFactory.DestroyEachPartList(netPlayer.CurEquipItemObjDic[pair.Key]);
                    GameObject broomDummyObj = character.GetBrromItemTypeToDummyObject();
                    GameObject CopyPartObj = Utility.AddChild(broomDummyObj, netPlayer.PlayerEquipItemObjDic[pair.Key]);
                    CopyPartObj.transform.localEulerAngles = Vector3.zero;

                    netPlayer.CurEquipItemObjDic[pair.Key].Add(CopyPartObj);

                    CopyPartObj.SetActive(true);
                }
                else
                    Instance.NetFactory.EquipParts(mType, rootObj, netPlayer.PlayerEquipItemObjDic[pair.Key], pair.Key, netPlayer, pair.Value.EquipItemCategory, pair.Value.EquipItemSubCategory);
            }
        }
    }

    public static void EquipPatrsOnNetPlayer(AVATAR_TYPE mType, GameObject rootObj, NetAvatarStyleItem netPlayer, CStyleItemAttr itemAttr)
    {
        CharAnimationEvent character = rootObj.GetComponent<CharAnimationEvent>();
        STYLE_ITEM_TYPE itemType = itemAttr.EquipItemType;

        //acc_face같이 gameobject가 없는 경우가 있어서 data로 가져온다
        if (itemType == STYLE_ITEM_TYPE.FACE)
        {
            return;
        }

        if (STYLE_ITEM_TYPE.EFFECT_HAND_RIGHT <= itemType && itemType <= STYLE_ITEM_TYPE.EFFECT_ROOT_ALL)
        {
            Instance.NetFactory.EqiupEffectParts(netPlayer.PlayerEquipItemObjDic[itemType], itemType, netPlayer, character);
        }
        else if (STYLE_ITEM_TYPE.ACC_FACE == itemType)
        {
            Transform trsFace = rootObj.transform.Find("Basic_Face");
            if (trsFace != null)
            {
                Instance.NetFactory.SetMakeUpFaceAcc(itemAttr, trsFace.gameObject);
            }
        }
        else
        {
            Instance.NetFactory.EquipParts(mType, rootObj, netPlayer.PlayerEquipItemObjDic[itemType], itemType, netPlayer, itemAttr.EquipItemCategory, itemAttr.EquipItemSubCategory);
        }

    }

    public void PutOffEquipPatrsOnNetPlayer(NetAvatarStyleItem netPlayer)
    {
        NetFactory.DestroyEachPartAll(netPlayer.CurEquipItemObjDic);
    }

    public void PutOffFaceAccOnNetPlayer(GameObject rootObj)
    {
        Transform trsFace = rootObj.transform.Find("Basic_Face");
        NetFactory.PutOffMakeUpFaceAcc(trsFace.gameObject);
    }



    public static void EquipEachPart(AVATAR_TYPE mType, GameObject rootObj, GameObject partObj, STYLE_ITEM_TYPE partType,  STYLE_ITEM_CATEGORY category, byte subCategory, bool bDefault = false)
    {
        CAvatar avatar = CPlayer.GetAvatar(mType);
        CStyleItemInven inven = CStyleItemInvenManager.Instance.GetStyleInvenInfo(mType);


        if (STYLE_ITEM_TYPE.EFFECT_HAND_RIGHT <= partType && partType <= STYLE_ITEM_TYPE.EFFECT_ROOT_ALL)
        {
            CharAnimationEvent character = rootObj.GetComponent<CharAnimationEvent>();
            Instance.factory.EqiupEffectParts(avatar, partObj, partType, inven, character, bDefault);
        }
        else
        {
            // 빗자루
            if (partType == STYLE_ITEM_TYPE.ACC_PROP)
            {
                inven.DestoryCurPutOnItemObj(partType);
                CharAnimationEvent character = rootObj.GetComponent<CharAnimationEvent>();
                GameObject broomDummyObj = character.GetBrromItemTypeToDummyObject();
                GameObject CopyPartObj = Utility.AddChild(broomDummyObj, partObj);
                CopyPartObj.transform.localEulerAngles = Vector3.zero;

                inven.SetCurPutOnStyleItemObjDic(partType, CopyPartObj);

                CopyPartObj.SetActive(true);
            }
            else
            {
                Instance.factory.EquipParts(avatar, rootObj, partObj, partType, inven, category, subCategory, bDefault);
            }
        }
    }

    public static void EquipEachFacePart(CStyleItemInven attr, bool putoff = false)
    {
        Instance.factory.SetMakeUpFaceAcc(attr, putoff);
    }

    public static bool SetChangePart_PartInstSuit(AVATAR_TYPE mType, GameObject rootObj, STYLE_ITEM_TYPE defaultType)
    {
        return Instance.factory.SetTakeOffSuitPart_Equip(mType, rootObj, defaultType);
    }

    public static void DestroyPutOnObject(AVATAR_TYPE avatarType, STYLE_ITEM_TYPE partType)
    {
        CAvatar avatar = CPlayer.GetAvatar(avatarType);
        Instance.factory.DestroyEachPartList(avatar, partType);
    }



    #region SET

    public static void SetCurrentAvatar(AVATAR_TYPE avatarType)
    {
        Instance.curSelectEquipAvatarType = avatarType;
    }

    public static void ChangeLayersRecursively(Transform trans, string name)
    {
        trans.gameObject.layer = LayerMask.NameToLayer(name);
        foreach (Transform child in trans)
        {
            ChangeLayersRecursively(child, name);
        }
    }

    public static void SetDefaultSItemID(AVATAR_TYPE avatarType)
    {
        //CAvatar avatar = CPlayer.GetAvatar(avatarType);
        //CAvatar uiavatar = CPlayer.GetAvatar(CPlayer.GetEnumUIAvatarType(avatarType));

        CStyleItemInven inven = CStyleItemInvenManager.Instance.GetStyleInvenInfo(avatarType);
        CStyleItemInven uiInven = CStyleItemInvenManager.Instance.GetStyleInvenInfo(CPlayer.GetEnumUIAvatarType(avatarType));

        //avatar.DefaultStyleItemID = new Dictionary<STYLE_ITEM_TYPE, string>();
        //base Data id to DefaultStyleItemID
        CONFIGURE_TYPE avatarCfgType = Instance.GetAvatarConfigureType(avatarType);
        List<ConfigureData> baseData = Configure.GetConfigureDataArray(avatarCfgType);
        if(baseData != null)
        {
            inven.DefaultStyleItemID.Clear();
            uiInven.DefaultStyleItemID.Clear();

            for (int i = 0; i < baseData.Count; ++i)
            {
                STYLE_ITEM_TYPE itemType = Instance.GetDefaultStyleItemType(baseData[i].Value1);

                if (!inven.DefaultEquipedItemDic.ContainsKey(itemType))
                {
                    inven.DefaultStyleItemID.Add(itemType, baseData[i].Value2);
                }

                if (!uiInven.DefaultEquipedItemDic.ContainsKey(itemType))
                {
                    uiInven.DefaultStyleItemID.Add(itemType, baseData[i].Value2);
                }
            }
        }
        else
        {
            CDebug.Log(string.Format($"SetDefaultSItemID. baseData is null. avatarType = " + avatarType));
        }
    }

    //public void SetActiveDynamicBone(NetAvatarStyleItem netPlayer, bool bActive)
    //{
    //    foreach (KeyValuePair<STYLE_ITEM_TYPE, CStyleItemAttr> pair in netPlayer.PlayerEquipItemDic)
    //    {
    //        Instance.NetFactory.SetActiveDynamicBone( pair.Key, bActive );
    //    }

    //}

    #endregion SET



    #region GET

    public static AVATAR_TYPE GetCurrentAvatar()
    {
        return Instance.curSelectEquipAvatarType;
    }


    public CONFIGURE_TYPE GetAvatarConfigureType(AVATAR_TYPE avatarType)
    {
        CONFIGURE_TYPE cType = CONFIGURE_TYPE.NONE;
        
        switch (avatarType)
        {
            case AVATAR_TYPE.AVATAR_JISOO: cType = CONFIGURE_TYPE.BASE_STYLE_ITEM_JISOO; break;
            case AVATAR_TYPE.AVATAR_JENNIE: cType = CONFIGURE_TYPE.BASE_STYLE_ITEM_JENNIE; break;
            case AVATAR_TYPE.AVATAR_ROSE: cType = CONFIGURE_TYPE.BASE_STYLE_ITEM_ROSE; break;
            case AVATAR_TYPE.AVATAR_LISA: cType = CONFIGURE_TYPE.BASE_STYLE_ITEM_LISA; break;
        }
        return cType;
    }

    public STYLE_ITEM_TYPE GetDefaultStyleItemType(string value)
    {
        STYLE_ITEM_TYPE type = STYLE_ITEM_TYPE.NONE;
        int partID = int.Parse(value);
        switch (partID)
        {
            case 1: type = STYLE_ITEM_TYPE.HEAD; break;
            case 2: type = STYLE_ITEM_TYPE.TOP; break;
            case 3: type = STYLE_ITEM_TYPE.BOTTOM; break;
            case 6: type = STYLE_ITEM_TYPE.SHOES; break;
            case 12: type = STYLE_ITEM_TYPE.ACC_HAND; break;
            case 99: type = STYLE_ITEM_TYPE.FACE; break;
        }
        return type;
    }

    public static bool IsDefaultStyleItemID(AVATAR_TYPE avatarType, STYLE_ITEM_TYPE itemType, long ID)
    {
        CAvatar avatar = CPlayer.GetAvatar(avatarType);
        CStyleItemInven inven = CStyleItemInvenManager.Instance.GetStyleInvenInfo(avatarType);
        if (inven.DefaultStyleItemID.ContainsKey(itemType))
        {
            if(ID == long.Parse(inven.DefaultStyleItemID[itemType]))
            {
                return true;
            }
        }
        return false;
    }
    public static GameObject LoadAvatarObject(AVATAR_TYPE avatarType, GameObject parent, bool bMetaverse = false)
    {
        CAvatar avatar = CPlayer.GetAvatar(avatarType);
        GameObject avatarObj = Instance.LoadAvatarBaseObject(CPlayer.GetEnumAvatarType(avatarType), parent);
        avatarObj.name = avatarType.ToString();

        avatar.SetDynamicBone(avatarObj);

        CStyleItemInvenManager.EquipAllPartsStatic(avatarType, avatarObj);


        SetActivateLOD( ref avatarObj, false );

        return avatarObj;
    }

    /// <summary>
    /// 자기 인벤토리에 있는 아이템이 아니라 외부 스타일로 입힐경우 사용한다
    /// </summary>
    /// <param name="avatarType"></param>
    /// <param name="parent"></param>
    /// <param name="netPlayer"></param>
    /// <returns></returns>
    public static GameObject LoadNetAvatarObject(AVATAR_TYPE avatarType, GameObject parent, NetAvatarStyleItem netPlayer)
    {
        CDebug.Log( "---------------------AVATAR TYPE : " + avatarType , CDebugTag.BPWORLD );
        GameObject avatarObj = Instance.LoadAvatarBaseObject(avatarType, parent);
        avatarObj.name = "NetPlayer_" + avatarType;

        //IsCheckDownloadedResourcesFileHash true이면 끔
        //netPlayer.SetDynamicBone(avatarObj, Instance.CDNConfig.GameServerConfig.SelectedSetting.Etc.IsCheckDownloadedResourcesFileHash);
        netPlayer.SetDynamicBone( avatarObj );

        EquipAllPatrsOnNetPlayers(avatarType, avatarObj, netPlayer);


        //Animator animator = avatarObj.GetComponent<Animator>();
        //animator.runtimeAnimatorController = Instance.GetAnimatorController(avatarType, avatarObj, true);

        SetActivateLOD( ref avatarObj, false );
        //CDebug.Log( $"        |||| Load'Net'AvatarObject |  animController = {animator.runtimeAnimatorController.name}" );
        return avatarObj;
    }

    public static IObservable<GameObject> AsyncLoadNetAvatarObject(AVATAR_TYPE avatarType, GameObject parent, NetAvatarStyleItem netPlayer)
    {
        CDebug.Log( "---------------------AVATAR TYPE : " + avatarType , CDebugTag.BPWORLD );

        return Instance.AsyncLoadAvatarBaseObject(avatarType, parent)
        .Do(asset => 
        {
            GameObject avatarObj = asset;
            avatarObj.name = "NetPlayer_" + avatarType;

            //IsCheckDownloadedResourcesFileHash true이면 끔
            //netPlayer.SetDynamicBone(avatarObj, Instance.CDNConfig.GameServerConfig.SelectedSetting.Etc.IsCheckDownloadedResourcesFileHash);
            netPlayer.SetDynamicBone( avatarObj );

            EquipAllPatrsOnNetPlayers(avatarType, avatarObj, netPlayer);

            //Animator animator = avatarObj.GetComponent<Animator>();
            //animator.runtimeAnimatorController = Instance.GetAnimatorController(avatarType, avatarObj, true);
        });
    }

    public GameObject LoadShadowResouce(GameObject obj)
    {
        var resData = CResourceManager.Instance.GetResourceData(Constants.PATH_CHAR_SHADOW);
        return (GameObject)UnityEngine.Object.Instantiate(resData.Load<GameObject>(obj));
    }


    public GameObject SetCharacterShadow(GameObject character)
    {
        GameObject shadow = LoadShadowResouce(character);
        shadow.transform.parent = character.transform;
        shadow.transform.localPosition = new Vector3(0, 0.01f, 0);//Vector3.zero;

        return shadow;
    }

    public GameObject LoadCharacterShadow(GameObject character)
    {
        GameObject shadow = LoadShadowResouce(character);
        shadow.transform.parent = character.transform.parent;
        shadow.transform.localPosition = new Vector3(0, 0.01f, 0);
        shadow.name = character.name + "_Shadow";

        return shadow;
    }

    #endregion GET


    #region LOD Disable 
    public static void SetActivateLOD(ref GameObject targetObject, bool isEnable)
    {

        LODGroup[] lodGroups = targetObject.GetComponentsInChildren<LODGroup>();

        lodGroups.ForEach((idx, lg) =>
        {
            lg.GetLODs().ForEach((idx2, lod) =>
            {
                lod.renderers.ForEach((rendereridx, renderer) =>
                {
                    if (renderer.name.Contains("_skinned_"))
                    {
                        renderer.enabled = isEnable;
                    }
                });
            });

            lg.enabled = isEnable;

        });
    }

    #endregion



    #region LOAD


    public static IObservable<GameObject> AsyncLoadResourceSItemObject(AVATAR_TYPE avatarType, STYLE_ITEM_TYPE partType, CStyleItemAttr item)
    {
        if (item == null)
        {
            return Observable.Return<GameObject>(null);
        }

        string resID = item.ResID;
        if (resID.Equals("0"))
        {
            CDebug.LogError(string.Format($"LoadResourceSItemObject. resID [{resID}] is 0."));
            return Observable.Return<GameObject>(null);
        }

        var resourceData = CResourceManager.Instance.GetResourceData(resID);
        if (resourceData == null)
        {
            return Observable.Return<GameObject>(null);
        }

        
        return resourceData.LoadAsync<GameObject>(null)
        .Do(asset => 
        {
            var disposable = new SingleAssignmentDisposable();
            disposable.Disposable = asset.OnDestroyAsObservable()
                .Do(_ =>
                {
                    Addressables.ReleaseInstance(asset);
                    disposable.Dispose();
                })
                .Subscribe();
        });
    }





    public static GameObject LoadResourceSItemObject(AVATAR_TYPE avatarType, STYLE_ITEM_TYPE partType, CStyleItemAttr item, GameObject releaseObj = null)
    {
        GameObject obj;

        if (item == null)
        {
            return null;
        }

        string resID = item.ResID;
        if (resID.Equals("0"))
        {
            CDebug.LogError(string.Format($"LoadResourceSItemObject. resID [{resID}] is 0."));
            return null;
        }

        obj = LoadResource(resID, releaseObj );

        return obj;
    }

    public static GameObject LoadResource(string path, GameObject releaseObj = null)
    {
        GameObject obj;
        var resourceData = CResourceManager.Instance.GetResourceData(path);
        if (resourceData == null)
            return null;

        obj = (GameObject)resourceData.LoadObject(); //resourceData.Load<GameObject>( releaseObj );//

        return obj;
    }

    public static IObservable<GameObject> AsyncLoadResource(string path, GameObject releaseObj = null)
    {
        var resourceData = CResourceManager.Instance.GetResourceData(path);
        if (resourceData == null)
            return Observable.Return<GameObject>(null);

        return resourceData.LoadAsync<GameObject>( null )
        .Do(asset =>
        {
            var disposable = new SingleAssignmentDisposable();
            disposable.Disposable = asset.OnDestroyAsObservable()
                .Do(_ =>
                {
                    Addressables.ReleaseInstance(asset);
                    disposable.Dispose();
                })
                .Subscribe();
        });
    }

    public GameObject LoadAvatarBaseObject(AVATAR_TYPE type, GameObject parentObj)
    {
        GameObject basicBodyObj = null;

        switch(type)
        {
            case AVATAR_TYPE.AVATAR_JISOO:
                basicBodyObj = LoadResource(strBaseBodyPath_Jisoo);
                break;
            case AVATAR_TYPE.AVATAR_JENNIE:
                basicBodyObj = LoadResource(strBaseBodyPath_Jennie);
                break;
            case AVATAR_TYPE.AVATAR_LISA:
                basicBodyObj = LoadResource(strBaseBodyPath_Lisa);
                break;
            case AVATAR_TYPE.AVATAR_ROSE:
                basicBodyObj = LoadResource(strBaseBodyPath_Rose);
                break;
        }

        GameObject avatarObj = Utility.AddChild(parentObj, basicBodyObj);
        avatarObj.transform.rotation = Quaternion.Euler(0, 180, 0);

        return avatarObj;
    }


    public IObservable<GameObject> AsyncLoadAvatarBaseObject(AVATAR_TYPE type, GameObject parentObj)
    {
        string path = strBaseBodyPath_Jisoo;

        switch(type)
        {
            case AVATAR_TYPE.AVATAR_JISOO: path = strBaseBodyPath_Jisoo;
                break;
            case AVATAR_TYPE.AVATAR_JENNIE: path = strBaseBodyPath_Jennie;
                break;
            case AVATAR_TYPE.AVATAR_LISA: path = strBaseBodyPath_Lisa;
                break;
            case AVATAR_TYPE.AVATAR_ROSE: path = strBaseBodyPath_Rose;
                break;
            default:
                CDebug.LogError( string.Format( "Invalid Param. AVATAR_TYPE : {0}", type ) );
                break;
        }


        return AsyncLoadResource(path, parentObj)
            .Select(asset => 
            {
                GameObject avatarObj = Utility.AddChild(parentObj, asset);
                avatarObj.transform.rotation = Quaternion.Euler(0, 180, 0);
                return avatarObj;
            });
    }


    #endregion LOAD
}
