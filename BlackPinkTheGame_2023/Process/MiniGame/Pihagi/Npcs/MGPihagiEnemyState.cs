
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


using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using DG.Tweening;
using BPWPacketDefine;

public class MGPihagiEnemyState_Spawn : CState<MGPihagiEnemy>       
{
    static MGPihagiEnemyState_Spawn s_this = null;

    public static MGPihagiEnemyState_Spawn Instance()
    {
        if(s_this == null) s_this = new MGPihagiEnemyState_Spawn();
        return s_this;
    }

    public override void Enter(MGPihagiEnemy enemy)
    {
        enemy.EnemyController.SetAnimatonTrigger("idle");
    }

    public override void Excute(MGPihagiEnemy enemy)
    {
    }

    public override void Exit(MGPihagiEnemy enemy)
    {
    }
}


public class MGPihagiEnemyState_Prepare : CState<MGPihagiEnemy>
{
    static MGPihagiEnemyState_Prepare s_this = null;

    public static MGPihagiEnemyState_Prepare Instance()
    {
        if (s_this == null) s_this = new MGPihagiEnemyState_Prepare();
        return s_this;
    }

    public override void Enter(MGPihagiEnemy enemy)
    {
        enemy.SetActiveGroundFX(true);
        if(enemy.EnemyMgr.EnemyCheckPlaySound.ContainsKey(enemy.EnemyDir) == false)
        {
            enemy.EnemyMgr.EnemyCheckPlaySound.Add( enemy.EnemyDir, new Dictionary<PihagiGameEnemyState, bool>() );
            if (enemy.EnemyMgr.EnemyCheckPlaySound[enemy.EnemyDir].ContainsKey( PihagiGameEnemyState.DELAY ) == false)
            {
                enemy.EnemyMgr.EnemyCheckPlaySound[enemy.EnemyDir].Add( PihagiGameEnemyState.DELAY, true );
                SoundManager.Instance.PlayEffect( 6830024 ); // list 124
                CDebug.Log( $"           [PIHAGI SOUND] dir{enemy.EnemyDir} =>  DELAY Sound" );
            }
        }
        enemy.EnemyController.SetAnimatonTrigger("cheer");

        if (MGPihagiServerDataManager.Instance.IsSinglePlay)
        {
            if (enemy.WorldMgr.GetTutorialStep() == TutorialStep.STEP_1)
            {
                TCPMGPihagiRequestManager.Instance.Req_GamePlayPause();
                enemy.WorldMgr.SetTutorial_Step2();
            }
        }
    }

    public override void Excute(MGPihagiEnemy enemy)
    {
    }

    public override void Exit(MGPihagiEnemy enemy)
    {
    }
}


public class MGPihagiEnemyState_Dash : CState<MGPihagiEnemy>
{
    static MGPihagiEnemyState_Dash s_this = null;

    public static MGPihagiEnemyState_Dash Instance()
    {
        if (s_this == null) s_this = new MGPihagiEnemyState_Dash();
        return s_this;
    }

    public override void Enter(MGPihagiEnemy enemy)
    {
        enemy.SetDash();
        if (enemy.EnemyMgr.EnemyCheckPlaySound.ContainsKey( enemy.EnemyDir ))
        {
            if (enemy.EnemyMgr.EnemyCheckPlaySound[enemy.EnemyDir].ContainsKey( PihagiGameEnemyState.MOVE ) == false)
            {
                SoundManager.Instance.PlayEffect( 6830025 ); // list 125
                SoundManager.Instance.PlayEffect( 6830026 ); // list 126
                enemy.EnemyMgr.EnemyCheckPlaySound[enemy.EnemyDir].Add( PihagiGameEnemyState.MOVE, true );
                CDebug.Log( $"           [PIHAGI SOUND] dir{enemy.EnemyDir} =>  MOVE Sound" );
            }
        }
        enemy.EnemyController.SetAnimatonTrigger("run");
    }

    public override void Excute(MGPihagiEnemy enemy)
    {
    }

    public override void Exit(MGPihagiEnemy enemy)
    {
        SoundManager.Instance.SoundStop(SoundCategory.EFFECT);
        enemy.ReleaseDash();
    }
}

public class MGPihagiEnemyState_Deth : CState<MGPihagiEnemy>
{
    static MGPihagiEnemyState_Deth s_this = null;

    public static MGPihagiEnemyState_Deth Instance()
    {
        if (s_this == null) s_this = new MGPihagiEnemyState_Deth();
        return s_this;
    }

    public override void Enter(MGPihagiEnemy enemy)
    {
        enemy.SetHideEnemyObject();
        if (enemy.EnemyMgr.EnemyCheckPlaySound.ContainsKey( enemy.EnemyDir ))
        {
            enemy.EnemyMgr.EnemyCheckPlaySound[enemy.EnemyDir].Clear();
            enemy.EnemyMgr.EnemyCheckPlaySound.Remove( enemy.EnemyDir );
        }

        if (MGPihagiServerDataManager.Instance.IsSinglePlay)
        {
            if (enemy.WorldMgr.GetTutorialStep() == TutorialStep.STEP_2)
            {
                TCPMGPihagiRequestManager.Instance.Req_GamePlayPause();
                enemy.WorldMgr.SetTutorial_Step3();
            }
        }
    }
    public override void Excute(MGPihagiEnemy enemy)
    {
    }

    public override void Exit(MGPihagiEnemy enemy)
    {
    }
}