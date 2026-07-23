using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CMarbleBlockReward : MonoBehaviour
{
    [SerializeField] private Image RewardIconImg;
    [SerializeField] private TextMeshProUGUI RewardValueText;
    [SerializeField] private GameObject StarRootObj;
    [SerializeField] private GameObject[] StarObjs;
    [SerializeField] private GameObject[] StarDisableObjs;
    [SerializeField] private GameObject[] StarEnableObjs;

    private CamController CamTransform;
    
    private Camera MainCam;
    
    public void Init(CMarbleWorldManager worldMgr)
    {
        CamTransform = worldMgr.GetCamController();
        if (CamTransform != null)
        {
            MainCam = CamTransform.SceneCamera;
        }
    }

    public void SetIconImage(string iconPath)
    {
        //Debug.Log($"#### [CMarbleBlockReward] SetIconImage - IconPath: {iconPath}, RewardIconImg: {RewardIconImg}");
        if (RewardIconImg != null && iconPath != string.Empty)
        {
            CResourceManager.Instance.LoadImage(iconPath, RewardIconImg);
        //Debug.Log($"#### [CMarbleBlockReward] Loaded IconImage - RewardIconImg: {RewardIconImg}, RewardIconImg: {RewardIconImg}");
        }
    }

    public void SetValueText(string valueText)
    {
        RewardValueText.text = valueText;
    }

    public void SetRewardType(REWARD_TYPE type, int subValue)
    {
        if (type == REWARD_TYPE.RW_STICKER)
        {
            if (StarRootObj != null)
            {
                StarRootObj.SetActive(true);
            }
            RewardValueText.enabled = false;
            SetStarGrade(subValue);
        }
        else
        {
            RewardValueText.enabled = true;
            if (StarRootObj != null)
            {
                StarRootObj.SetActive(false);
            }
        }
    }

    private void SetStarGrade(int stickerId)
    {
        StickerData stickerData = CStickerbookDataManager.Instance.GetStickerData(stickerId);
        if (stickerData != null)
        {
            CStickerbookUIManager.Instance.SetStartGrade(stickerData, StarObjs, StarEnableObjs);

        }
    }

    void LateUpdate()
    {
        if (CamTransform != null && MainCam != null)
        {
            transform.rotation = MainCam.transform.rotation;
        }
    }
}
