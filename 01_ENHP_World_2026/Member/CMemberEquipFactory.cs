using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.Linq;
using Game.RestAPI;

public class CMemberEquipFactory
{
    private CMemberAvatar MemberAvatar = null;

    public CMemberEquipFactory(CMemberAvatar avatar)
    {
        MemberAvatar = avatar;
    }

    public void EquipParts(CMemberAvatar avatar, GameObject rootObj, StylingItemInfo baseItemInfo, STYLE_ITEM_TYPE partType, GameObject accDummy = null)
    {
        MemberAvatar = avatar;
        StylingItemInfo itemInfo = null;


        itemInfo = new StylingItemInfo(baseItemInfo.ItemData, rootObj);
        itemInfo.LoadItemObj();


        DestroyEachPart(partType);
        GameObject CopyPartObj = null;
        GameObject partObj = itemInfo.ItemObj;

        switch (partType)
        {   
            case STYLE_ITEM_TYPE.HAIR:
            case STYLE_ITEM_TYPE.SKIN:
                CopyPartObj = EquipPartTypeObject(partObj, rootObj, partType, itemInfo);
                break;
            default:
                if (itemInfo.ItemData.ItemSubType == BODYACC_SUB_TYPE.STYLE_SUBTYPE_ACC_BAG)
                {
                    CopyPartObj = EquipPartTypeObject(partObj, rootObj, partType, itemInfo);
                }
                else
                {
                    CopyPartObj = EquipAccTypeObject(partObj, partType, accDummy);
                }
                break;
        }

        itemInfo.ItemObj = CopyPartObj;
        MemberAvatar.SetCurPutOnSItemObjDic(partType, itemInfo);

        Utility.Destroy(CopyPartObj.transform.Find("Root")?.gameObject);
    }

    private GameObject EquipPartTypeObject(GameObject partObj, GameObject rootObj, STYLE_ITEM_TYPE partType, StylingItemInfo itemInfo)
    {
        GameObject CopyPartObj = null;

        var skinObj = partObj.GetComponentInChildren<SkinnedMeshRenderer>().gameObject;
        if (skinObj == null)
        {
            Debug.LogError("SkinObj(SkinnedMeshRenderer) skinObj is null");
            return null;
        }

        //Style Object Copy
        CopyPartObj = Utility.AddChild(rootObj, skinObj);
        CopyPartObj.name = $"{partType}";
        CopyPartObj.tag = itemInfo.ItemData.ItemTAG;
        CopyPartObj.SetActive(true);
        CopyPartObj.transform.localRotation = Quaternion.Euler(new Vector3(0, 180, 0));
        SkinnedMeshRenderer[] _smRenderers = CopyPartObj.GetComponentsInChildren<SkinnedMeshRenderer>();
        if (_smRenderers == null)
        {
            Debug.LogError("SkinObj(SkinnedMeshRenderer) _smRenderers is null");
            return null;
        }

        //linked Bone
        for (int i = 0; i < _smRenderers.Length; ++i)
        {
            Transform[] bones = new Transform[_smRenderers[i].bones.Length];

            for (int j = 0; j < bones.Length; ++j)
            {
                bones[j] = FindChild(rootObj, _smRenderers[i].bones[j].name);
            }

            _smRenderers[i].bones = bones;

            string strRootBoneName = _smRenderers[i].rootBone.name;
            _smRenderers[i].rootBone = FindChild(rootObj, strRootBoneName);
            _smRenderers[i].shadowCastingMode = _smRenderers[i].shadowCastingMode;
            _smRenderers[i].receiveShadows = _smRenderers[i].receiveShadows;
            _smRenderers[i].lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.BlendProbes;
            _smRenderers[i].reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.BlendProbes;
        }

        return CopyPartObj;
    }

    private GameObject EquipAccTypeObject(GameObject partObj, STYLE_ITEM_TYPE partType, GameObject accDummy = null)
    {
        GameObject CopyPartObj = null;

        CopyPartObj = Utility.AddChild(accDummy, partObj);
        CopyPartObj.name = $"{partType}";
        CopyPartObj.SetActive(true);
        CopyPartObj.transform.localRotation = Quaternion.Euler(Vector3.zero);

        return CopyPartObj;
    }


    protected Transform FindChild(GameObject parent, string name)
    {
        var findedGo = parent.DescendantsAndSelf().Where(go => go.name.Equals(name)).FirstOrDefault();
        return findedGo?.transform;
    }


    private GameObject GetPartObjByTag(GameObject rootObj, string tag)
    {
        Transform[] objs = rootObj.GetComponentsInChildren<Transform>();
        foreach (var obj in objs)
        {
            if (obj.CompareTag(tag))
            {
                return obj.gameObject;
            }
        }

        return null;
    }

    public void DestroyEachPart(STYLE_ITEM_TYPE type)
    {
        MemberAvatar.DestroyCurPonItem(type);
    }
}
