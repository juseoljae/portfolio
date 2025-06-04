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


using BPWPacketDefine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MGPihagiPlayerController : MonoBehaviour
{
    //private MGPihagiPlayersManager PlayerMgr;
    //public PihagiGameSnapshotPlayer PlayerInfo;

    private Animator PlayerAnimator;

    private Transform ShadowObj;

    #region MOVE_VAR
    private bool MoveStart;
    private Vector3 MoveTargetPos;
    private float VariableYpos;
    private Vector3 MoveStartPos;
    private float MoveTimer;
    public Vector3 MoveDir;
    public float MoveAngle;
    #endregion MOVE_VAR

    public const float SEND_POSITION_SEQ = 0.1f;

    public void Initialize(/*MGPihagiPlayersManager mgr*/)
    {
        //PlayerMgr = mgr;
        //PlayerInfo = info;

        PlayerAnimator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        MoveUpdate();
    }

    public void SetPlayerMoveRecv(Vector3 recvPosition)
    {
        Vector3 nowPosition = transform.position;
        if (recvPosition == nowPosition)
        {
            return;
        }

        MoveStart = true;
        MoveTimer = 0.0f;
        MoveTargetPos = recvPosition;
        MoveStartPos = nowPosition;
        transform.LookAt(new Vector3(MoveTargetPos.x, transform.position.y, MoveTargetPos.z));
    }


    private void MoveUpdate()
    {
        if (MoveStart)
        {
            if (MoveTimer >= 1.0f)
            {
                MoveTimer = 1.0f;
                MoveStart = false;
            }
            MoveTimer += Time.deltaTime / SEND_POSITION_SEQ;

            Vector3 pos = Vector3.Lerp(MoveStartPos, MoveTargetPos, MoveTimer);

            transform.position = pos;

            SetShadowPos(transform.position);
        }
    }


    public void SetShadowPos(Vector3 pos)
    {
        //Vector3 shadowPos = new Vector3(pos.x, ShadowObj.localPosition.y, pos.z);
        Vector3 shadowPos = new Vector3(pos.x, pos.y + 0.1f, pos.z);
        ShadowObj.localPosition = shadowPos;
    }

    public void SetShadowObj(Transform shadowObj)
    {
        ShadowObj = shadowObj;
    }

    public void SetActiveShadowObject(bool bActive)
    {
        ShadowObj.gameObject.SetActive(bActive);
    }

    public void SetAnimatonTrigger(string param)
    {
        PlayerAnimator.SetTrigger(param);
    }

    public void SetAnimationBool(string param, bool bSet)
    {
        PlayerAnimator.SetBool(param, bSet);
    }

	public void Release()
	{
		if(PlayerAnimator != null) PlayerAnimator = null;
		if(ShadowObj != null)
		{
			ShadowObj.gameObject.SetActive(false);
			//GameObject.Destroy(ShadowObj);
			ShadowObj = null;
		}
	}
	
}
