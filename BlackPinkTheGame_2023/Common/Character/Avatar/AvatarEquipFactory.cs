using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

using System.Reflection;

public class AvatarEquipFactory
{
    //simbol. this branch is for combining face of basic_body with eyes from hair
    private Dictionary<STYLE_ITEM_TYPE, Dictionary<string, DynamicBone>> DynamicBoneDic = new Dictionary<STYLE_ITEM_TYPE, Dictionary<string, DynamicBone>>();
    /// <summary>
    /// Equip Styling Item Part
    /// </summary>
    /// <param name="mType"></param>
    /// <param name="rootObj"></param>
    /// <param name="partObj">New part prefab object to change</param>
    /// <param name="partType">New part type to change</param>
//    public void EquipParts(AVATAR_TYPE mType, GameObject rootObj, GameObject partObj, STYLE_ITEM_TYPE partType, bool bDefault = false)
    public void EquipParts(CAvatar avatar, GameObject rootObj, GameObject partObj, STYLE_ITEM_TYPE partType, CStyleItemInven inven, STYLE_ITEM_CATEGORY category, byte subCategory, bool bDefault = false)
    {
        //Needs Release previous part object
        DestroyEachPart(avatar, inven, partType);

        SetDynamicBoneDicByPartType(partObj, partType);

        //Style Object Copy
        GameObject CopyPartObj = Utility.AddChild(rootObj, partObj);
        CopyPartObj.name = $"{partType}";
        CopyPartObj.SetActive(true);
        CopyPartObj.transform.localRotation = Quaternion.Euler(new Vector3(0, 180, 0));

        SkinnedMeshRenderer[] _smRenderers = CopyPartObj.GetComponentsInChildren<SkinnedMeshRenderer>();

        //linked Bone
        for (int i = 0; i < _smRenderers.Length; ++i)
        {
            Transform[] bones = new Transform[_smRenderers[i].bones.Length];

            for (int j = 0; j < bones.Length; ++j)
            {
                bones[j] = FindChild(rootObj.transform, _smRenderers[i].bones[j].name);
            }

            _smRenderers[i].bones = bones;
            string strRootBoneName = _smRenderers[i].rootBone.name;
            SetDynamicBone(avatar, _smRenderers[i].bones, partType);
            _smRenderers[i].rootBone = FindChild(rootObj.transform, strRootBoneName);

            _smRenderers[i].shadowCastingMode = _smRenderers[i].shadowCastingMode;//UnityEngine.Rendering.ShadowCastingMode.On;
            _smRenderers[i].receiveShadows = _smRenderers[i].receiveShadows;
            _smRenderers[i].lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.BlendProbes;
            _smRenderers[i].reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.BlendProbes;

            if (partType == STYLE_ITEM_TYPE.HEAD)
            {
                SetMakeUpFaceByHairPart(inven, partType, rootObj, bDefault);
            }

            inven.SetCurPutOnStyleItemObjDic(partType, CopyPartObj);
            if (category == STYLE_ITEM_CATEGORY.ACC_ETC)
            {
                var childCnt = _smRenderers[i].transform.childCount;
                if (childCnt > 0)
                {
                    CDebug.Log("Have Effect Parts");
                    CharAnimationEvent character = rootObj.GetComponent<CharAnimationEvent>();
                    for (int k = 0; k < childCnt; k++)
                    {
                        var parent = character.GetDummyObject((CHARACTER_HAND_DIR)subCategory)?.FirstOrDefault();
                        if (parent != null)
                        {
                            var effectPartsTm = _smRenderers[i].transform.GetChild(k);
                            effectPartsTm.parent = parent.transform;
                            inven.SetCurPutOnStyleItemObjDic(partType, effectPartsTm.gameObject);
                        }
                    }
                }
            }
        }

        Utility.Destroy(CopyPartObj.transform.Find("Bip001")?.gameObject);

    }

    public void EqiupEffectParts(CAvatar avatar, GameObject partObj, STYLE_ITEM_TYPE partType, CStyleItemInven inven, CharAnimationEvent character, bool bDefault = false)
    {
        List<GameObject> list = new List<GameObject>();
        DestroyEachPart(avatar, inven, partType);

        GameObject equipObj = null;
        GameObject equipObj2 = null;

        CDebug.Log($"EqiupEffectParts : {partType}");

        switch (partType)
        {
            case STYLE_ITEM_TYPE.EFFECT_HAND_RIGHT:
                {
                    equipObj = Utility.AddChild(character.DummyHand_Right, partObj);
                }
                break;
            case STYLE_ITEM_TYPE.EFFECT_HAND_LEFT:
                {
                    equipObj = Utility.AddChild(character.DummyHand_Left, partObj);
                }
                break;
            case STYLE_ITEM_TYPE.EFFECT_HAND_BOTH:
                {
                    equipObj = Utility.AddChild(character.DummyHand_Right, partObj);
                    equipObj2 = Utility.AddChild(character.DummyHand_Left, partObj);
                }
                break;
            case STYLE_ITEM_TYPE.EFFECT_HEAD_TOP:
            case STYLE_ITEM_TYPE.EFFECT_HEAD_BACK:
            case STYLE_ITEM_TYPE.EFFECT_HEAD_NOSE:
            case STYLE_ITEM_TYPE.EFFECT_HEAD_MOUTH:
            case STYLE_ITEM_TYPE.EFFECT_HEAD_CHEEK:
                {
                    equipObj = Utility.AddChild(character.Dummy_Head, partObj);
                }
                break;

            case STYLE_ITEM_TYPE.EFFECT_SPINE_SHOULDER:
            case STYLE_ITEM_TYPE.EFFECT_SPINE_BACK:
            case STYLE_ITEM_TYPE.EFFECT_SPINE_BACK2:
                {
                    equipObj = Utility.AddChild(character.Dummy_Spine, partObj);
                }
                break;
            case STYLE_ITEM_TYPE.EFFECT_COM_WAIST:
            case STYLE_ITEM_TYPE.EFFECT_COM_BODY:
                {
                    equipObj = Utility.AddChild(character.Dummy_Com, partObj);
                }
                break;
            case STYLE_ITEM_TYPE.EFFECT_ROOT_ALL:
            case STYLE_ITEM_TYPE.EFFECT_ROOT_FLOOR:
                {
                    equipObj = Utility.AddChild(character.Dummy_Root, partObj);
                }
                break;
        }

        if (equipObj != null)
        {
            inven.SetCurPutOnStyleItemObjDic(partType, equipObj);
        }
        if (equipObj2 != null)
        {
            inven.SetCurPutOnStyleItemObjDic(partType, equipObj2);
        }
    }

    private void SetDynamicBone(CAvatar avatar, Transform[] bones, STYLE_ITEM_TYPE partType)
    {
        Dictionary<string, DynamicBone> _dic = DynamicBoneDic[partType];
        if (_dic == null || avatar.BaseBodyDBDic == null)
        {
            CDebug.LogError("AvatarEquipFactory SetDynamicBone() dic key partType( " + partType + " ) is not contain");
            return;
        }

        for (int i = 0; i < bones.Length; ++i)
        {
            if (_dic.ContainsKey(bones[i].name))
            {
                if (avatar.BaseBodyDBDic.ContainsKey(bones[i].name))
                {
                    avatar.SetBaseBodyDynamicBoneData(_dic[bones[i].name], bones[i]);
                    //SetDynamicBoneData(_dic[bones[i].name], member, bones[i]);
                }
            }
        }
    }

    private void DestroyEachPart(CAvatar avatar, CStyleItemInven inven, STYLE_ITEM_TYPE partType)
    {
        switch (partType)
        {
            case STYLE_ITEM_TYPE.TOP:
            case STYLE_ITEM_TYPE.BOTTOM:
                if (inven.GetCurPutOnStyleItemObjList(STYLE_ITEM_TYPE.SUIT).Count > 0)
                {
                    DestroyEachPartList(avatar, STYLE_ITEM_TYPE.SUIT);
                }
                break;
            case STYLE_ITEM_TYPE.SUIT:
                for (STYLE_ITEM_TYPE type = STYLE_ITEM_TYPE.TOP; type <= STYLE_ITEM_TYPE.BOTTOM; ++type)
                {
                    DestroyEachPartList(avatar, type);
                }
                //DestroyEachPartList(avatar, partType);
                break;
            case STYLE_ITEM_TYPE.EFFECT_HAND_BOTH:
                for (STYLE_ITEM_TYPE type = STYLE_ITEM_TYPE.EFFECT_HAND_RIGHT; type <= STYLE_ITEM_TYPE.EFFECT_HAND_LEFT; ++type)
                {
                    DestroyEachPartList(avatar, type);
                }
                //DestroyEachPartList(avatar, partType);
                break;
            case STYLE_ITEM_TYPE.EFFECT_HAND_LEFT:
            case STYLE_ITEM_TYPE.EFFECT_HAND_RIGHT:
                if (inven.GetCurPutOnStyleItemObjList(STYLE_ITEM_TYPE.EFFECT_HAND_BOTH).Count > 0)
                {
                    DestroyEachPartList(avatar, STYLE_ITEM_TYPE.EFFECT_HAND_BOTH);
                }
                //DestroyEachPartList(avatar, partType);
                break;
            default:
                break;
        }

        DestroyEachPartList(avatar, partType);

    }


    public void DestroyEachPartList(CAvatar avatar, STYLE_ITEM_TYPE partType)
    {
        if (avatar.BaseBodyDBDic != null)
        {
            CAvatar _avatar = avatar;
            AVATAR_TYPE type = avatar.GetAvatarType();
            if ((int)type >= (int)AVATAR_TYPE.AVATAR_FOR_UI)
            {
                long avatarID = (long)(type - AVATAR_TYPE.AVATAR_FOR_UI);
                _avatar = CPlayer.GetAvatar( (AVATAR_TYPE)avatarID );
            }

            if (DynamicBoneDic.ContainsKey( partType ))
            {
                //Set enable false in Base body DynamicBone
                foreach (KeyValuePair<string, DynamicBone> item in DynamicBoneDic[partType])
                {
                    if (avatar.BaseBodyDBDic.ContainsKey( item.Key ))
                    {
                        if (avatar.BaseBodyDBDic[item.Key] == null) continue;
                        avatar.BaseBodyDBDic[item.Key].IsActive = false;
                        avatar.BaseBodyDBDic[item.Key].obj_Name = "none";
                        avatar.BaseBodyDBDic[item.Key].enabled = false;
                        _avatar.SetBeforeDBNameDic( partType, item.Key );
                    }
                }
                //remove Dictionary Key        
                DynamicBoneDic.Remove( partType );
            }
        }

        CStyleItemInven inven = CStyleItemInvenManager.Instance.GetStyleInvenInfo(avatar.GetAvatarType());
        inven.DestoryCurPutOnItemObj(partType);

    }

    public bool SetTakeOffSuitPart_Equip(AVATAR_TYPE mType, GameObject rootObj, STYLE_ITEM_TYPE defaultType)
    {
        bool bTakeOffSuit = false;
        CAvatar avatar = CPlayer.GetAvatar(mType);
        CStyleItemInven inven = CStyleItemInvenManager.Instance.GetStyleInvenInfo(mType);

        if (inven.GetCurPutOnStyleItemObjList(STYLE_ITEM_TYPE.SUIT).Count > 0)
        {
            //remove Suit part
            DestroyEachPartList(avatar, STYLE_ITEM_TYPE.SUIT);
            //Equip default top or bottom object
			if(inven.DefaultEquipedItemObjDic.ContainsKey(defaultType))
			{
                var attr = inven.DefaultEquipedItemDic[defaultType];
                GameObject partObj = inven.DefaultEquipedItemObjDic[defaultType];
				AvatarManager.EquipEachPart(mType, rootObj, partObj, defaultType, attr.EquipItemCategory, attr.EquipItemSubCategory);
			}
            bTakeOffSuit = true;
        }
        return bTakeOffSuit;
    }

    protected Transform FindChild(Transform parent, string name)
    {
        if (parent == null) return null;
        if (string.IsNullOrEmpty(name)) return null;

        Transform result = null;
        Transform childs = parent.GetComponentInChildren<Transform>();
        //Debug.Log("parent root = "+parent.root+" / parent name = "+parent.name+"/ count = "+ childs.childCount+"/"+name);
        if (childs != null)
        {
            for (int i = 0; i < childs.childCount; i++)
            {
                if (childs.GetChild(i).name.Equals(name))
                {
                    Transform child = childs.GetChild(i);
                    return child;
                    //return childs.GetChild(i);
                }

                result = childs.GetChild(i);
                result = FindChild(result, name);

                if (result != null)
                {
                    return result;
                }
            }
        }

        return null;
    }


    public void SetDynamicBoneDicByPartType(GameObject partObj, STYLE_ITEM_TYPE partType)
    {
        if (DynamicBoneDic == null) return;
        if (DynamicBoneDic.ContainsKey(partType) == false)
        {
            DynamicBoneDic.Add(partType, new Dictionary<string, DynamicBone>());
        }
             
        DynamicBone[] _db = partObj.GetComponentsInChildren<DynamicBone>();
        for (int i = 0; i < _db.Length; ++i)
        {
            if (DynamicBoneDic[partType].ContainsKey(_db[i].name) == false)
            {
                DynamicBoneDic[partType].Add(_db[i].name, _db[i]);
            }
        }
    }

    private string GetMakeUpResourceID(CStyleItemInven inven, STYLE_ITEM_TYPE partType, bool bDefault)
    {
        CStyleItemAttr item = null;
        if(bDefault == false)
        {
            if (inven.CurPuttingONStyleItemDic.ContainsKey(partType))
            {
                item = inven.CurPuttingONStyleItemDic[partType];
            }
            else
            {
                item = inven.GetCurEquipStyleItem(partType);
            }
        }
        else
        {
            item = inven.DefaultEquipedItemDic[partType];
        }

        if(item == null)
        {
            return string.Empty;
        }

        return item.MakeUP_ResID;
    }

    private void SetMakeUpFaceByHairPart(CStyleItemInven inven, STYLE_ITEM_TYPE partType, GameObject rootObj, bool bDefault)
    {
        GameObject faceObj = inven.GetCurEquipStyleItemObj(STYLE_ITEM_TYPE.FACE);

        if(faceObj != null)
        {
            SkinnedMeshRenderer[] _faceRenderers = faceObj.GetComponentsInChildren<SkinnedMeshRenderer>();

            string resID = GetMakeUpResourceID( inven, partType, bDefault );

            if (resID.Equals( string.Empty ) == false)
            {
                var resData = CResourceManager.Instance.GetResourceData( resID );
                Material[] makeUpMat = new Material[] { resData.LoadMaterial( faceObj ) };
                Material matTatoo = null;

                //얼굴에 타투 있습니다
                if (_faceRenderers.FirstOrDefault().materials.Count() > 1)
                {
                    matTatoo = _faceRenderers.FirstOrDefault().materials[1];
                }

                //오리지날 face가 메터리얼이 두개일 경우가 있지 않을 것 같음
                List<Material> matList = new List<Material>();
                matList.Add( makeUpMat[0] );
                if (matTatoo != null)
                {

                    matList.Add( matTatoo );
                }

                _faceRenderers.ToList().FirstOrDefault().materials = matList.ToArray();

                CStyleItemInvenManager.Instance.SetChangeEyesMaterialWhenEquipPart( rootObj, resID );
            }
        }
    }



    /// <summary>
    /// acc_face 는 기본 얼굴 메터리얼에 추가 메터리얼을 세팅 하는 방식이다
    /// </summary>
    /// <param name="inven"></param>
    /// <param name="bDefault"></param>
    public void SetMakeUpFaceAcc(CStyleItemInven inven, bool bDefault)
    {
        GameObject faceObj = inven.GetCurEquipStyleItemObj(STYLE_ITEM_TYPE.FACE);
        SkinnedMeshRenderer[] _faceRenderers = faceObj.GetComponentsInChildren<SkinnedMeshRenderer>();
        

        if (bDefault)
        {
            Material matOrigin = new Material(_faceRenderers.ToList().FirstOrDefault().materials[0]);
            List<Material> matList = new List<Material>();
            matList.Add(matOrigin);
            _faceRenderers.ToList().FirstOrDefault().materials = matList.ToArray();
        }
        else
        {
            string resID = GetMakeUpResourceID(inven, STYLE_ITEM_TYPE.ACC_FACE, bDefault);
            if (resID.Equals(string.Empty) == false)
            {
                var resData = CResourceManager.Instance.GetResourceData(resID);
                Material[] makeUpMat = new Material[] { resData.LoadMaterial(faceObj) };
                ///inven.SetCurrentEqiupStyleItemObj(STYLE_ITEM_TYPE.ACC_FACE, makeUpMat[0]);

                //오리지날 face가 메터리얼이 두개일 경우가 있지 않을 것 같음
                Material matOrigin = new Material(_faceRenderers.ToList().FirstOrDefault().materials[0]);
                List<Material> matList = new List<Material>();
                matList.Add(matOrigin);
                matList.Add(makeUpMat[0]);
                _faceRenderers.ToList().FirstOrDefault().materials = matList.ToArray();
            }
        }
        
    }
}
