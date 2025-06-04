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
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Actually Move, Animation Controller
public class MGPihagiEnemyController : MonoBehaviour
{
    private MGPihagiEnemysManager EnemyMgr;
    public PihagiGameSnapshotEnemy EnemyInfo;

    private Animator EnemyAnimator;
    private string SwitchRunAnimContrlllerKey;
    private long SwitchRunAnimContrlllerGroupID;
    private GameObject RunAnimContrlllerReleaseObj;

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

    public ENEMY_JUNPSTATE JumpState { get; set; }
    public ENEMY_POSDIR EnemyDir { get; set; }

    public void Initialize(MGPihagiEnemysManager mgr, PihagiGameSnapshotEnemy info)
    {
        EnemyMgr = mgr;
        EnemyInfo = info;

        EnemyAnimator = gameObject.GetComponent<Animator>();

        JumpState = ENEMY_JUNPSTATE.POS_UP;
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

            if (JumpState == ENEMY_JUNPSTATE.POS_UP || JumpState == ENEMY_JUNPSTATE.POS_DOWN)
            {
                JumpState = EnemyMgr.GetEnemyJumpState(EnemyDir, transform.position);
            }

            //CDebug.Log("Enemy Move() JumpState = "+ JumpState);

            switch (JumpState)
            {
                case ENEMY_JUNPSTATE.POS_UP:
                    VariableYpos = MGPihagiDefine.ENEMY_SPAWN_YPOS;

                    if (ShadowObj.gameObject.activeSelf == false)
                    {
                        SetActiveEnemyShadowObject(true);
                    }
                    break;
                case ENEMY_JUNPSTATE.JUMPDOWN:
                    if (VariableYpos > 0)
                    {
                        VariableYpos -= (MGPihagiDefine.JUMP_STEP_PERFRAME + 0.05f);
                    }
                    else
                    {
                        VariableYpos = 0;
                        JumpState = ENEMY_JUNPSTATE.POS_DOWN;
                    }
                    
                    if(ShadowObj.gameObject.activeSelf)
                    {
                        SetActiveEnemyShadowObject(false);
                    }
                    break;
                case ENEMY_JUNPSTATE.POS_DOWN:
                    VariableYpos = 0;

                    if (ShadowObj.gameObject.activeSelf == false)
                    {
                        SetActiveEnemyShadowObject(true);
                    }
                    break;
                case ENEMY_JUNPSTATE.JUMPUP:
                    if(VariableYpos < MGPihagiDefine.ENEMY_SPAWN_YPOS)
                    {
                        VariableYpos += MGPihagiDefine.JUMP_STEP_PERFRAME;
                    }
                    else
                    {
                        VariableYpos = MGPihagiDefine.ENEMY_SPAWN_YPOS;
                        JumpState = ENEMY_JUNPSTATE.POS_UP;
                    }

                    if (ShadowObj.gameObject.activeSelf)
                    {
                        SetActiveEnemyShadowObject(false);
                    }
                    break;
            }

            MoveTargetPos.y = VariableYpos;

            Vector3 pos = Vector3.Lerp(transform.position, MoveTargetPos, MoveTimer);

            transform.position = pos;

            SetEnemyShadowPos(transform.position);
        }
    }


    public void SetEnemyShadowPos(Vector3 pos)
    {
        //Vector3 shadowPos = new Vector3(pos.x, ShadowObj.localPosition.y, pos.z);
        Vector3 shadowPos = new Vector3(pos.x, pos.y + 0.1f, pos.z);
        ShadowObj.localPosition = shadowPos;
    }

    public void SetShadowObj(Transform shadowObj)
    {
        ShadowObj = shadowObj;
    }

    public void SetActiveEnemyShadowObject(bool bActive)
    {
        ShadowObj.gameObject.SetActive(bActive);
    }

    public void SetAnimatonTrigger(string param)
    {
        EnemyAnimator.SetTrigger(param);
    }

    public void SetRuntimeAnimatorController(string key, long groupID, ANIMCONTROLLER_USE_TYPE useType, GameObject obj)
    {
        if (EnemyAnimator == null)
        {
            CDebug.LogError( "SetRuntimeAnimatorController() MyAnimator is null " );
            return;
        }
        SwitchRunAnimContrlllerKey = key;
        SwitchRunAnimContrlllerGroupID = groupID;


        string name = string.Format( "RunAnimContrlllerReleaseObj_PihagiEnemy_{0}", SwitchRunAnimContrlllerKey );
        RunAnimContrlllerReleaseObj = new GameObject( name );
        RunAnimContrlllerReleaseObj.transform.SetParent( obj.transform.parent );

        RuntimeAnimatorController _runController = CRuntimeAnimControllerSwitchManager.Instance.SetRuntimeAnimatorController( key, groupID, useType, RunAnimContrlllerReleaseObj );

        if (_runController != null)
        {
            EnemyAnimator.runtimeAnimatorController = _runController;
        }
        else
        {
            CDebug.LogError( $"SetRuntimeAnimatorController() key:{key}, groupID:{groupID} / RuntimeAnimatorController is null " );
        }
    }

    public void ReleaseRuntimeAnimatorController(bool bReleaseAll = false)
    {
        if (EnemyAnimator == null)
        {
            CDebug.LogError( "ReleaseRuntimeAnimatorController() MyAnimator is null " );
            return;
        }

        if (RunAnimContrlllerReleaseObj != null)
        {
            UnityEngine.Object.Destroy( RunAnimContrlllerReleaseObj );
            RunAnimContrlllerReleaseObj = null;
        }

        CRuntimeAnimControllerSwitchManager.Instance.ReleaseRuntimeAnimController( SwitchRunAnimContrlllerKey, SwitchRunAnimContrlllerGroupID, bReleaseAll );
        EnemyAnimator.runtimeAnimatorController = null;
    }

    public void CleanUp()
	{
		if (ShadowObj != null)
		{
			ShadowObj.gameObject.SetActive(false);
			//Destroy(ShadowObj);
			ShadowObj = null;
		}
	}
}
