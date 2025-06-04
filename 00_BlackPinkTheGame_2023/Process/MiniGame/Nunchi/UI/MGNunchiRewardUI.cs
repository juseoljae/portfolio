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
# endif

#endregion

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MGNunchiRewardUI : MonoBehaviour
{
    private Image RewardItemIcon;
    private Text RewardCountTxt;
    private CConsume RewardConsume;


    public void Initialize()
    {
        RewardItemIcon = transform.Find("normal/item/img_item").GetComponent<Image>();
        RewardCountTxt = transform.Find("normal/count/Text").GetComponent<Text>();
    }

    public void SetData(StageRewardInfo rewardInfo)
    {
        RewardConsume = new CConsume()
        {
            Type = rewardInfo.RewardType,
            Value1 = rewardInfo.ItemID,
            Value2 = rewardInfo.Count
        };

        CDebug.Log(" @@2@@@@@ RewardUI itemID = " + rewardInfo.ItemID);

        CResourceData resourceData = RewardConsume.GetCommonResourceIDByType();
        RewardItemIcon.sprite = resourceData.LoadSprite(this.gameObject);

        RewardCountTxt.text = rewardInfo.Count.ToCommaString();//.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
