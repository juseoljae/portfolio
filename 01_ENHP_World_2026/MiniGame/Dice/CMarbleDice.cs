using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;

public class CMarbleDice : MonoBehaviour
{
    public CMarbleWorldManager WorldMgr;
    private CMarbleDiceManager DiceMgr;
    private Animator animator;
    private AnimationClip WorkClip;
    private GameObject[] DiceChildren;
    private float DiceWorkingTime;
    private List<GameObject> DiceDummyObjs;

    private CStateMachine<CMarbleDice> DiceSM;
    private Tween MoveTween;

    private const float DICE_SCALE = 30f;

    public void Init(CMarbleDiceManager diceMgr)
    {
        DiceMgr = diceMgr;
        WorldMgr = diceMgr.WorldMgr;

        DiceDummyObjs = new List<GameObject>();
        DiceChildren = new GameObject[2];

        DiceSM = new CStateMachine<CMarbleDice>(this);
        
        SetObject();
    }

    private void SetObject()
    { 
        Transform root = transform.Find("Bip001 Root");
        Transform dice1 = root.Find("Bip001 dice_01");
        Transform dice2 = root.Find("Bip001 dice_02");
        DiceDummyObjs.Add(dice1.gameObject);
        DiceDummyObjs.Add(dice2.gameObject);

        transform.localScale = new Vector3(DICE_SCALE, DICE_SCALE, DICE_SCALE);
        
        animator = gameObject.GetComponent<Animator>();
        SetAnimationInfo();
    }

    public void SetDice(GameObject dice1, GameObject dice2, DICE_STATE state)
    {
        DestroyDiceChildren();
        DiceChildren[0] = dice1;
        DiceChildren[1] = dice2;

        for (int i=0 ; i<DiceChildren.Length; i++)
        {
            DiceChildren[i].transform.SetParent(DiceDummyObjs[i].transform, true);
            SetDiceTransform(DiceChildren[i]);
        }

        SetDiceState(state);
    }

    public void SetDiceState(DICE_STATE state)
    {
        switch (state)
        {
            case DICE_STATE.READY:
                SetAnimatorTrigger(CMarbleDefine.ANIM_NAME_DICE_RESET);
                SetDiceObjActive(false);
                break;
            case DICE_STATE.IDLE:
                DiceSM.ChangeState(MarbleDiceState_Idle.Instance());
                break;
            case DICE_STATE.READYTO:
                DiceSM.ChangeState(MarbleDiceState_ReadyToWork.Instance());
                break;
            case DICE_STATE.WORK:
                DiceSM.ChangeState(MarbleDiceState_Work.Instance());
                break;
            case DICE_STATE.WORKFINISH:
                DiceSM.ChangeState(MarbleDiceState_WorkFinish.Instance());
                break;
        }
    }

    private void SetDiceTransform(GameObject obj)
    {
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.Euler(Vector3.zero);
        obj.transform.localScale = Vector3.one;
        SetActiveDiceObjs(obj, true);
    }

    private void SetActiveDiceObjs(GameObject obj, bool bActive)
    {
        obj.SetActive(bActive);
    }

    public void SetDiceObjActive(bool bActive)
    {
        gameObject.SetActive(bActive);
    }

    private void SetAnimationInfo()
    {
        if (animator.runtimeAnimatorController == null)
            return;
        
        string workClipName = "working";

        AnimationClip[] _clips = animator.runtimeAnimatorController.animationClips;
        foreach (AnimationClip clip in _clips)
        {
            string clipName = clip.name;

            if (clipName.Contains(workClipName))
            {
                DiceWorkingTime = clip.length;
                return;
            }
        }
    }

    public void SetAnimatorTrigger(string param)
    {
        if (animator != null)
        {
            animator.SetTrigger(param);
        }
    }

    public void SetDiceStartPosition()
    {
        transform.localPosition = GetDiceWorkStartPos();
    }

    public float GetDiceWorkingTime()
    {
        return DiceWorkingTime;
    }


    public Vector3 GetDiceWorkTargetPos()
    {
        return DiceMgr.GetDiceWorkTargetPos();
    }

    public Vector3 GetDiceWorkStartPos()
    {
        return DiceMgr.GetDiceWorkStartPos();
    }

    // Update is called once per frame
    public void UpdateStateMachine()
    {
        if (DiceSM != null)
        {
            DiceSM.StateMachine_Update();
        }
    }

    public void MoveToTarget()
    {
        Vector3 startPos = GetDiceWorkStartPos();
        Vector3 targetPos = GetDiceWorkTargetPos();
        
        if (MoveTween != null)
        {
            MoveTween.Kill();
            MoveTween = null;
        }

        transform.position = startPos;

        MoveTween = transform.DOMove(targetPos, DiceWorkingTime)
                  .SetEase(Ease.Linear)
                  .SetUpdate(UpdateType.Normal)
                  .OnComplete(() =>
                  {
                      SetDiceState(DICE_STATE.WORKFINISH);
                  });
    }

    public void DestroyDiceChildren()
    {
        for (int i = 0; i < DiceChildren.Length; i++)
        {
            if (DiceChildren[i] != null)
            {
                Destroy(DiceChildren[i]);
                DiceChildren[i] = null;
            }
        }
    }

    public void Release()
    {
        if (DiceSM != null)
        {
            DiceSM = null;
        }

        if (DiceChildren != null)
        {
            DestroyDiceChildren();
            DiceChildren = null;
        }

        if (DiceDummyObjs != null)
        {
            DiceDummyObjs.Clear();
            DiceDummyObjs = null;
        }

        if (MoveTween != null)
        {
            MoveTween.Kill();
            MoveTween = null;
        }
    }
}
