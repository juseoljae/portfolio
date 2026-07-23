using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Rendering;

public class TouchedEvent3DUI : MonoBehaviour
{
    private SpriteRenderer RewardIcon;
    private TextMeshPro RewardCountTxt;
    private Animation RewardAnimation;

    public void Initialize()
    {
        SetObjects();
    }

    private void SetObjects()
    {
        RewardAnimation = transform.GetComponent<Animation>();

        GameObject _obj = transform.Find("icon_reward").gameObject;
        RewardIcon = _obj.GetComponent<SpriteRenderer>();
        _obj.SetActive(true);
        _obj = transform.Find("text_count").gameObject;
        RewardCountTxt = _obj.GetComponent<TextMeshPro>();
        _obj.SetActive(true);

        _obj = transform.Find("icon_reward_02").gameObject;
        _obj.SetActive(false);
        _obj = transform.Find("text_count_02").gameObject;
        _obj.SetActive(false);
        _obj = transform.Find("icon_bg").gameObject;
        _obj.SetActive(false);
        _obj = transform.Find("icon_complete").gameObject;
        _obj.SetActive(false);
        _obj = transform.Find("icon").gameObject;
        _obj.SetActive(false);
    }

    public void SetData(REWARD_CONSUME_TYPES type, string count)
    {
        RewardIcon.sprite = CResourceManager.Instance.GetDefaultIconRes(type)?.LoadSprite(this);
        CDebug.Log($"      ###### TouchedEvent3DUI.SetData({type}, {count}). sprite icon = {RewardIcon.sprite.name}");
        RewardCountTxt.text = count;

        string ANIM_NAME = "fx_3DUI_product_done_start";
        RewardAnimation.Play(ANIM_NAME);

        StartCoroutine(EndAnimation(ANIM_NAME));
    }

    // 부모, 위치, 크기 조정
    public void SetTrans (GameObject avatarObj, Transform parent, TOUCHEVENT_SCENE_TYPE sceneType)
    {
        transform.SetParent (parent);

        Vector3 avatarPos = avatarObj.transform.localPosition;
        switch (sceneType)
        {
            case TOUCHEVENT_SCENE_TYPE.MAINLOBBY:
                transform.localScale = new Vector3 (0.3f, 0.3f, 1f);
                transform.localPosition = new Vector3 (0.05f, 1.85f, 0f);
                break;
            case TOUCHEVENT_SCENE_TYPE.MANAGEMENT:
                transform.localScale = new Vector3 (1f, 1f, 1f);
                Vector3 tmpPos = avatarPos + new Vector3(0, 2.3f, -0.45f);
                tmpPos.z = -0.5f;
                transform.localPosition = tmpPos;
                break;
            case TOUCHEVENT_SCENE_TYPE.AVATARINFO:
                transform.localScale = new Vector3 (0.3f, 0.3f, 1f);
                transform.localPosition = new Vector3 (0f, 1f, -0.5f);
                break;
        }
    }

    private IEnumerator EndAnimation(string checkAniName)
    {
        while (RewardAnimation.IsPlaying(checkAniName))
        {
            yield return null;
        }

        var ANIM_NAME = "fx_3DUI_product_done_end";
        RewardAnimation.Play(ANIM_NAME);
        StartCoroutine(Hide_Product(ANIM_NAME));
    }

    private IEnumerator Hide_Product(string checkAniName)
    {
        while (RewardAnimation.IsPlaying(checkAniName))
        {
            yield return null;
        }

        Release();
        //Destroy(this.gameObject);
    }

    public void Release()
    {
        if(RewardIcon != null)
        {
            RewardIcon = null;
        }

        if(RewardCountTxt != null)
        {
            RewardCountTxt = null;
        }

        if(RewardAnimation != null)
        {
            RewardAnimation = null;
        }

        Destroy(this.gameObject);
    }
}
