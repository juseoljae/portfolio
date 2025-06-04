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

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MGNunchiScoreFxUI : MonoBehaviour
{
    private MGNunchiWorldManager WorldMgr;
    public Image IconImg;
    public Text SignTxt;//+, -
    public Text ScoreTxt;

    Transform coinSignObj;

    public Animation ScoreAnim;

    private Dictionary<BPWPacketDefine.NunchiGameItemType, Sprite> ItemIconSprDic;

    public void Initialize(MGNunchiWorldManager worldMgr)
    {
        WorldMgr = worldMgr;
        coinSignObj = transform.Find("coin_sign");
        ScoreAnim = coinSignObj.GetComponent<Animation>();

        IconImg = coinSignObj.Find("coin").GetComponent<Image>();
        SignTxt = coinSignObj.Find("tesx_sign").GetComponent<Text>();
        ScoreTxt = coinSignObj.Find("text_score").GetComponent<Text>();

        ItemIconSprDic = new Dictionary<BPWPacketDefine.NunchiGameItemType, Sprite>();

        SetItemIconSprDic(BPWPacketDefine.NunchiGameItemType.COIN, MGNunchiConstants.SCORE_ITEM_NAME_COIN);

        DefaultStrResData itemdouble = CResourceManager.Instance.GetDefaultData(DEFAULT_RES_ICON_TYPE.ITEM_DOUBLE);
        DefaultStrResData itemSteal = CResourceManager.Instance.GetDefaultData(DEFAULT_RES_ICON_TYPE.ITEM_STEAL);
        DefaultStrResData itemInvicible = CResourceManager.Instance.GetDefaultData(DEFAULT_RES_ICON_TYPE.ITEM_INVINCIBLE);

        SetItemIconSprDicRes(BPWPacketDefine.NunchiGameItemType.COIN_DOUBLE, itemdouble.IconResID);
        SetItemIconSprDicRes(BPWPacketDefine.NunchiGameItemType.STEAL, itemSteal.IconResID);
        SetItemIconSprDicRes(BPWPacketDefine.NunchiGameItemType.INVINCIBLE, itemInvicible.IconResID);
    }

    public void SetData(int gainScore, BPWPacketDefine.NunchiGameItemType itemType)
    {
        if (ItemIconSprDic.ContainsKey(itemType) == false || itemType == BPWPacketDefine.NunchiGameItemType.NONE)
        {
            Debug.LogError($"MGNunchiScoreFxUI.SetData() item type({itemType}) is not cointain in ItemIconSprDic");
            return;
        }

        IconImg.sprite = ItemIconSprDic[itemType];
        //CDebug.Log("                           ##### FX SCore UI IconImg.sprite name = "+ IconImg.sprite.name + "//// itemType = " + itemType);

        if (itemType == BPWPacketDefine.NunchiGameItemType.COIN)
        {
            //Debug.LogError($"    ***********   MGNunchiScoreFxUI.SetData() gainScore ({gainScore}) / {transform}");
            string score = Mathf.Abs(gainScore).ToString();
            ScoreTxt.text = score;
            SetActiveScore(true);
            ScoreAnim.Play(MGNunchiDefines.ANIM_SCORE_GAIN_COIN);
        }
        else
        {
            SetActiveScore(false);
            ScoreAnim.Play(MGNunchiDefines.ANIM_SCORE_GAIN_ITEM);
        }
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void SetActiveScore(bool bActive)
    {
        if(SignTxt != null) SignTxt.gameObject.SetActive(bActive);
        if(ScoreTxt != null) ScoreTxt.gameObject.SetActive(bActive);
    }

    private void SetItemIconSprDic(BPWPacketDefine.NunchiGameItemType type, string itemName)
    {
        string iconPath = string.Format(MGNunchiConstants.SCORE_ITEM_ICON_PATH, itemName);
        Sprite _spr = WorldMgr.GetResourceSprite(iconPath, this.gameObject);

        if(ItemIconSprDic.ContainsKey(type) == false)
        {
            ItemIconSprDic.Add(type, _spr);
        }
    }

    private void SetItemIconSprDicRes(BPWPacketDefine.NunchiGameItemType type, string iconPath)
    {
        //string iconPath = string.Format(MGNunchiConstants.SCORE_ITEM_ICON_PATH, itemName);
        Sprite _spr = WorldMgr.GetResourceSprite(iconPath, this.gameObject);

        if(ItemIconSprDic.ContainsKey(type) == false)
        {
            ItemIconSprDic.Add(type, _spr);
        }
    }

    private void OnDestroy()
    {
        if (ScoreAnim != null) ScoreAnim = null;
    }
}

