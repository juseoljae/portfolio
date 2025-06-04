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

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using DG.Tweening;

public class TouchedEventUI : MonoBehaviour
{
    private GameObject IntimacyRootObj;
    private Text BubbleMsg;
    private Slider IntimacySlider;
    private Text IntimacyLv;
    private Text IntimacyExp;

    private GameObject RewardRootObj;
    private Image RewardIconImg;
    private Text RewardCountTxt;

    private Animation Anim;

    private bool bSetData;
    private long OldLv;
    private long Lv;
    private float OldExpValue;
    private float ExpValue;

    public void Initialize()
    {
        IntimacyRootObj = transform.Find("intimacy").gameObject;
        BubbleMsg = transform.Find("speechbubble/text").GetComponent<Text>();
        IntimacySlider = transform.Find("intimacy/Slider").GetComponent<Slider>();
        IntimacyLv = transform.Find("intimacy/icon/level/text").GetComponent<Text>();
        IntimacyExp = transform.Find("intimacy/count/text").GetComponent<Text>();

        RewardRootObj = transform.Find("reward").gameObject;
        RewardIconImg = RewardRootObj.transform.Find("icon/img_icon").GetComponent<Image>();
        RewardCountTxt = RewardRootObj.transform.Find("count/text").GetComponent<Text>();
        RewardRootObj.SetActive(false);

        Anim = IntimacyRootObj.GetComponent<Animation>();

        bSetData = false;
    }

    public void SetData(long strID, CAvatar avatar)
    {
        gameObject.SetActive(true);

        string _msg = CResourceManager.Instance.GetString(strID);
        BubbleMsg.text = _msg;

        Lv = avatar.intimacy_lv;
        IntimacyLv.text = Lv.ToString();
        float _exp = (float)avatar.intimacy_exp;
        IntimacyExp.text = _exp.ToString();

        OldLv = avatar.intimacy_lv_old;

        if (Lv > OldLv)
        {
            avatar.intimacy_old_exp = 0;
        }

        float _oldExp = (float)avatar.intimacy_old_exp;
        float _exp_max = (float)avatar.intimacy_exp_max;
        //CDebug.Log($"SetData oldLv = {OldLv}, Lv = {Lv} / oldExp = {_oldExp}, Exp = {_exp}");

        OldExpValue = (float)(_oldExp / _exp_max);
        ExpValue = (float)(_exp / _exp_max);

        IntimacySlider.value = OldExpValue;

        if (_oldExp < _exp)
        {
            IntimacyRootObj.SetActive(true);
            bSetData = true;
        }
        else
        {
            IntimacyRootObj.SetActive(false);
            InitSetDataSW();
        }

        string ANIM_NAME = "fx_temp_touch_event_slider_glow";
        Anim.Play(ANIM_NAME);

        StartCoroutine(EndAnimation(ANIM_NAME));

        var disposer = new SingleAssignmentDisposable();
        disposer.Disposable = Observable.Timer(TimeSpan.FromSeconds(3))
        .Subscribe(_ =>
        {
            Release();
            disposer.Dispose();
        })
        .AddTo(this);
    }

    public void SetReward(REWARD_CONSUME_TYPES rewardType, string rewardCnt)
    {
        CResourceData resourceData = CResourceManager.Instance.GetDefaultIconRes(rewardType);
        RewardIconImg.sprite = resourceData.LoadSprite(this);

        RewardCountTxt.text = rewardCnt;

        RewardRootObj.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        if(bSetData)
        {
            DOTween.To(() => IntimacySlider.value, x => IntimacySlider.value = x, ExpValue, 1).OnComplete(InitSetDataSW);
            //CDebug.Log($"{bSetData}, OldExpValue = {OldExpValue}, ExpValue = {ExpValue}, Value = {IntimacySlider.value}");
        }
    }

    private void InitSetDataSW()
    {
        bSetData = false;
    }

    private IEnumerator EndAnimation(string checkAniName)
    {
        while (Anim.IsPlaying(checkAniName))
        {
            yield return null;
        }

        var ANIM_NAME = "fx_temp_touch_event_slider_glow_off";
        Anim.Play(ANIM_NAME);
        StartCoroutine(Hide_Product(ANIM_NAME));
    }

    private IEnumerator Hide_Product(string checkAniName)
    {
        while (Anim.IsPlaying(checkAniName))
        {
            yield return null;
        }

        Release();
        //Destroy(this.gameObject);
    }

    private void Release()
    {
        if (IntimacyRootObj != null) IntimacyRootObj = null;
        if (BubbleMsg != null) BubbleMsg = null;
        if (IntimacySlider != null) IntimacySlider = null;
        if (IntimacyLv != null) IntimacyLv = null;
        if (IntimacyExp != null) IntimacyExp = null;
        //Destroy(gameObject);
    }
}
