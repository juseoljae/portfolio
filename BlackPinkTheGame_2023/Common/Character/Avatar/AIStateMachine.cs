
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
using UniRx;
using GroupManagement;
using DG.Tweening;

public class AIStateMachine_Spawn : CState<CCharacterController_Management>
{
    static AIStateMachine_Spawn s_this = null;

    public static AIStateMachine_Spawn Instance()
    {
        if (s_this == null) { s_this = new AIStateMachine_Spawn(); }
        return s_this;
    }

    public override void Enter(CCharacterController_Management charController)
    {
        AVATAR_TYPE avatarType = charController.GetAvatarType();
       // CDebug.Log("    %%%% AIStateMachine_Spawn.Enter() avatar = " + avatarType);

        charController.SetAnimationTrigger("idle");
        charController.gameObject.SetActive(false);
        GridIndex grid = charController.WorldMgr.AvatarMgr.GetAvatarSpawnGrid(avatarType);
        Vector3 _pos = charController.WorldMgr.AvatarMgr.GetAvatarPositionByGrid(grid);
        charController.transform.localPosition = _pos;
        charController.transform.rotation = Quaternion.Euler(0, 180, 0);
        charController.SetNavMesh();
        charController.SetActiveNavMeshAgent(true);
        charController.gameObject.SetActive(true);


        charController.SetCurrentFloor(grid.Y);

        //Start Event Check
        //Event Checking proc continues.
        if (charController.GetPrevAISMState() != AIStateMachine_Wait.Instance())
        {
            charController.SetChangeAIStateMachine(AIStateMachine_Patrol.Instance());
        }
        else
        {
            charController.TimeAI.CheckTime = charController.GetAITimer();
            charController.TimeAI.TargetTime = 1;
        }
    }

    public override void Excute(CCharacterController_Management charController)
    {
        if (charController.GetPrevAISMState() == AIStateMachine_Wait.Instance())
        {
            float curTime = charController.GetAITimer() - charController.TimeAI.CheckTime;
            //CDebug.Log(GetAvatarType() + "/  time = " + (curTime) + "/ targetTime = " + TimeAI.TargetTime);
            if (curTime >= charController.TimeAI.TargetTime)
            {
                charController.ResumeWayPoint();
                charController.SetChangeAIStateMachine(AIStateMachine_Patrol.Instance());
            }

        }
    }

    public override void Exit(CCharacterController_Management charController)
    { }
}



public class AIStateMachine_Patrol : CState<CCharacterController_Management>
{
    static AIStateMachine_Patrol s_this = null;

    //float startTime;
    public static AIStateMachine_Patrol Instance()
    {
        if (s_this == null) { s_this = new AIStateMachine_Patrol(); }
        return s_this;
    }

    public override void Enter(CCharacterController_Management charController)
    {
        //ManagementAvatarManager.SpawnByRule()
        //ManagementAvatarManager.GetAvatarRandomGrid()
        //CCharacterController_Management.SetAIStateEnter_Patrol()
        AVATAR_TYPE avatarType = charController.GetAvatarType();
        //if (avatarType != AVATAR_TYPE.AVATAR_JISOO) return;
        //if (avatarType != AVATAR_TYPE.AVATAR_JISOO && avatarType != AVATAR_TYPE.AVATAR_JENNIE) return;

        //CDebug.Log("    %%%% AIStateMachine_Patrol.Enter() avatar = " + avatarType +"/ prev state = "+ charController.AIStateMachine.GetPreviousState());

        charController.SetAnimationTrigger("idle");

        //if (avatarType == AVATAR_TYPE.AVATAR_JISOO || avatarType == AVATAR_TYPE.AVATAR_JENNIE)
        charController.SetAIStateEnter_Patrol();
    }
    public override void Excute(CCharacterController_Management charController)
    {
    }

    public override void Exit(CCharacterController_Management charController)
    {
    }
}


public class AIStateMachine_GotoSameFloorTarget : CState<CCharacterController_Management>
{
    static AIStateMachine_GotoSameFloorTarget s_this = null;

    public static AIStateMachine_GotoSameFloorTarget Instance()
    {
        if (s_this == null) { s_this = new AIStateMachine_GotoSameFloorTarget(); }
        return s_this;
    }

    public override void Enter(CCharacterController_Management charController)
    {
        //CDebug.Log("    %%%% AIStateMachine_GotoSameFloorTarget.Enter() avatar = " + charController.GetAvatarType());

        Vector3 _wayPos = charController.WayPoint.GetWayPoint(0);
        charController.SetNavMeshDestination(_wayPos);
        GridIndex _targetGrid = charController.GetTargetGrid();
        charController.WayPoint.SetFloors(_targetGrid.Y, _targetGrid.Y);

        //CDebug.Log("    %%%% AIStateMachine_GotoSameFloorTarget.Enter() set animation Walk avatar = " + charController.GetAvatarType());
        charController.SetAnimationTrigger("walk");
    }

    public override void Excute(CCharacterController_Management charController)
    {
        if (charController.IsArrivedNavMeshAgentDestination())
        {
            charController.SetLastWayPoint();
        }
        else
        {
            charController.CheckingAvatarInteractionCondition();
        }
    }

    public override void Exit(CCharacterController_Management charController)
    {
    }
}

public class AIStateMachine_GotoFrontStartEV : CState<CCharacterController_Management>
{
    static AIStateMachine_GotoFrontStartEV s_this = null;

    public static AIStateMachine_GotoFrontStartEV Instance()
    {
        if (s_this == null) { s_this = new AIStateMachine_GotoFrontStartEV(); }
        return s_this;
    }

    public override void Enter(CCharacterController_Management charController)
    {
        //CDebug.Log("    %%%% AIStateMachine_GotoFrontStartEV.Enter() avatar = " + charController.GetAvatarType());//

        //Set Destination 
        charController.IdxOfEVInnerPos = 0;

        CCharacterController_Management _avatarInSameFloor = charController.WorldMgr.AvatarMgr.GetAvatarInSameFloorByState(charController.GetAvatarType(), charController.GetStartFloor());
        if (_avatarInSameFloor != null)
        {
            //if state of _avatarInSameFloor below, _avatarInSameFloor goes/arrived ev.
            //so set my charController.IdxOfEVInnerPos
            if (_avatarInSameFloor.AIStateMachine.GetCurrentState() == AIStateMachine_GotoFrontStartEV.Instance()
                || _avatarInSameFloor.AIStateMachine.GetCurrentState() == AIStateMachine_PreWaitFrontStartEV.Instance()
                || _avatarInSameFloor.AIStateMachine.GetCurrentState() == AIStateMachine_WaitFrontStartEV.Instance())
            {
                float _myPosX = charController.transform.localPosition.x;
                float _otherPosX = _avatarInSameFloor.transform.localPosition.x;
                if (_myPosX >= _otherPosX)
                {
                    charController.IdxOfEVInnerPos = 1;
                }
            }
            //CDebug.Log($"    %%%% AIStateMachine_GotoFrontStartEV.Enter() avatar = {charController.GetAvatarType()} pos = {charController.IdxOfEVInnerPos}");
        }

        Vector3 _wayPos = charController.WorldMgr.EVWaitPositions[charController.IdxOfEVInnerPos];
        _wayPos.y += charController.WorldMgr.GridElementHeight * charController.GetStartFloor();
        charController.SetNavMeshDestination(_wayPos);

        //CDebug.Log("    %%%% AIStateMachine_GotoFrontStartEV.Enter() set animation Walk avatar = " + charController.GetAvatarType());
        charController.SetAnimationTrigger("walk");
    }

    public override void Excute(CCharacterController_Management charController)
    {
        //Check Reach dest pos
        //Then Change state
        if (charController.IsArrivedNavMeshAgentDestination())
        {
            //Arrived!!!! a front of EV
            charController.AvatarRotate(0);
            //other avatar is coming (target floor is my start floor)
            CCharacterController_Management _comingAvatar = charController.WorldMgr.AvatarMgr.OtherAvatarComeToThisFloor(charController.GetAvatarType(), charController.GetStartFloor());
            if (_comingAvatar != null)
            {
                charController.SetComingAvatarInEV(_comingAvatar);
                //just go prepare State
                charController.SetChangeAIStateMachine(AIStateMachine_PreWaitFrontStartEV.Instance());
            }
            else
            {
                //CDebug.Log($"AIStateMachine_GotoFrontStartEV.excute {charController.GetAvatarType()}, floor = {charController.GetStartFloor()} go to 11111 AIStateMachine_WaitFrontStartEV");
                charController.SetChangeAIStateMachine(AIStateMachine_WaitFrontStartEV.Instance());
            }
        }
        else
        {
            charController.CheckingAvatarInteractionCondition();

            CCharacterController_Management _avatarInSameFloor = charController.WorldMgr.AvatarMgr.GetAvatarInSameFloorByState(charController.GetAvatarType(), charController.GetStartFloor(), true);
            if (_avatarInSameFloor != null)
            {
                float _myPosX = charController.transform.localPosition.x;
                float _otherPosX = _avatarInSameFloor.transform.localPosition.x;
                if (_myPosX >= _otherPosX)
                {
                    charController.IdxOfEVInnerPos = 1;
                }
                else
                {
                    charController.IdxOfEVInnerPos = 0;
                }
                charController.SetChangeNavMeshDestByIdxOfPos();
            }
            else
            {
                if (charController.IdxOfEVInnerPos == 1)
                {
                    charController.IdxOfEVInnerPos = 0;
                    charController.SetChangeNavMeshDestByIdxOfPos();
                }
            }
        }
    }

    public override void Exit(CCharacterController_Management charController)
    {
    }
}


public class AIStateMachine_PreWaitFrontStartEV : CState<CCharacterController_Management>
{
    static AIStateMachine_PreWaitFrontStartEV s_this = null;

    public static AIStateMachine_PreWaitFrontStartEV Instance()
    {
        if (s_this == null) { s_this = new AIStateMachine_PreWaitFrontStartEV(); }
        return s_this;
    }

    public override void Enter(CCharacterController_Management charController)
    {
        charController.SetAnimationTrigger("idle");
    }

    public override void Excute(CCharacterController_Management charController)
    {
        ManagementAvatarManager _avatarMgr = charController.WorldMgr.AvatarMgr;

        bool _isComingAvatarArrived = _avatarMgr.IsComingAvatarArrived(charController.GetAvatarType());
        if (_isComingAvatarArrived)
        {
            CCharacterController_Management _comingAvatar = charController.GetComingAvatarInEV();
            _comingAvatar.SetisArriveComingAvatar(true);
            EVDoorManager _evDoorMgr = charController.WorldMgr.GetEVDoorManager();
            _evDoorMgr.SetFinishEVDoorOpen(_comingAvatar.GetTargetFloor());
            charController.SetChangeAIStateMachine(AIStateMachine_WaitFrontStartEV.Instance());
        }
    }

    public override void Exit(CCharacterController_Management charController)
    {
    }
}

public class AIStateMachine_WaitFrontStartEV : CState<CCharacterController_Management>
{
    static AIStateMachine_WaitFrontStartEV s_this = null;

    public static AIStateMachine_WaitFrontStartEV Instance()
    {
        if (s_this == null) { s_this = new AIStateMachine_WaitFrontStartEV(); }
        return s_this;
    }

    public override void Enter(CCharacterController_Management charController)
    {
        charController.SetAnimationTrigger("idle");

        EVDoorManager _evDoorMgr = charController.WorldMgr.GetEVDoorManager();
        _evDoorMgr.CallEVByFloor(charController.GetStartFloor(), charController.GetAvatarType());
        _evDoorMgr.AddPassenger(charController.GetAvatarType(), charController.GetTargetFloor());
    }

    public override void Excute(CCharacterController_Management charController)
    {
        EVDoorManager _evDoorMgr = charController.WorldMgr.GetEVDoorManager();
        if (_evDoorMgr.HasArrivedStartFloorEV(charController.GetStartFloor()))
        {
            charController.SetChangeAIStateMachine(AIStateMachine_GotoInnerStartEV.Instance());
        }
    }

    public override void Exit(CCharacterController_Management charController)
    {
    }
}

public class AIStateMachine_GotoInnerStartEV : CState<CCharacterController_Management>
{

    static AIStateMachine_GotoInnerStartEV s_this = null;

    public static AIStateMachine_GotoInnerStartEV Instance()
    {
        if (s_this == null) { s_this = new AIStateMachine_GotoInnerStartEV(); }
        return s_this;
    }

    public override void Enter(CCharacterController_Management charController)
    {
        //CDebug.Log("    %%%% AIStateMachine_GotoInnerStartEV.Enter() avatar = " + charController.GetAvatarType());
        ManagementAvatarManager avatarMgr = charController.WorldMgr.AvatarMgr;
        charController.IdxOfEVInnerPos = 0;

        //In here, Set IdxOfEVInnerPos by checking somebody goes to ev and already get in ev.
        EVDoorManager _evDoorMgr = charController.WorldMgr.GetEVDoorManager();
        int _posIdx = _evDoorMgr.GetAvatarRegisterIdx(charController.GetStartFloor(), charController.GetAvatarType());
        if (_posIdx != -1)
        {
            charController.IdxOfEVInnerPos = _posIdx;
        }

        //CDebug.Log("    %%%% AIStateMachine_GotoInnerStartEV.Enter() avatar = " + charController.GetAvatarType() +", passenger Cnt = "+ _evmgr.GetRegisterList(charController.GetStartFloor()).Count); 

        Vector3 _wayPos = charController.WorldMgr.EVInnerPositions[charController.IdxOfEVInnerPos];
        _wayPos.y += charController.WorldMgr.GridElementHeight * charController.GetStartFloor();
        charController.SetNavMeshDestination(_wayPos);

        CCharacterController_Management _comingAvatar = charController.GetComingAvatarInEV();

        if (_comingAvatar != null)
        {
            charController.SetComingAvatarInEV(null);
        }
        charController.SetisArriveComingAvatar(false);
        //charController.SetCantCallEV(false);

        //CDebug.Log("    %%%% AIStateMachine_GotoInnerStartEV.Enter() set animation Walk avatar = " + charController.GetAvatarType());
        charController.SetAnimationTrigger("walk");

        if (charController.WorldMgr.AvatarMgr.GetAvatarQuestState( charController.GetAvatarType() ) == GroupManagement.ManagementEnums.QUEST_STATE.RECEIVED)
        {
            charController.WorldMgr.AvatarMgr.SetActiveAvatarMailQuestIcon( charController.GetAvatarType(), false );
        }
    }

    public override void Excute(CCharacterController_Management charController)
    {
        //check avatar reach inner position
        if (charController.IsArrivedNavMeshAgentDestination())
        {
            charController.SetChangeAIStateMachine(AIStateMachine_InnerStartEV.Instance());
        }
    }

    public override void Exit(CCharacterController_Management charController)
    {
    }
}

public class AIStateMachine_InnerStartEV : CState<CCharacterController_Management>
{
    static AIStateMachine_InnerStartEV s_this = null;

    public static AIStateMachine_InnerStartEV Instance()
    {
        if (s_this == null) { s_this = new AIStateMachine_InnerStartEV(); }
        return s_this;
    }

    public override void Enter(CCharacterController_Management charController)
    {
        //CDebug.Log("    %%%% AIStateMachine_InnerStartEV.Enter() avatar = " + charController.GetAvatarType());

        charController.SetAnimationTrigger("idle");
        charController.AvatarRotate(180);

    }

    public override void Excute(CCharacterController_Management charController)
    {
        //Set Change state when ev door is close
        //Check somebody is coming around in ev and getting in ev
        ManagementWorldManager _worldMgr = charController.WorldMgr;
        ManagementAvatarManager _avatarMgr = _worldMgr.AvatarMgr;
        int _curFloor = charController.GetStartFloor();

        CCharacterController_Management _avatarInSameFloor = _avatarMgr.GetAvatarInSameFloorByCheckerState(charController.GetAvatarType(), _curFloor, Instance());
        if (_avatarInSameFloor != null)
        {
            //Check distance, then close door
            Vector3 _myPos = charController.transform.localPosition;
            Vector3 _avatarPosInSameFloor = _avatarInSameFloor.transform.localPosition;
            float _distance = Mathf.Abs(Vector3.Distance(_myPos, _avatarPosInSameFloor));

            if (_distance <= Constants.EV_CHK_GETIN_DISTANCE)
            {
                if (_distance <= Constants.EV_GETIN_DISTANCE)
                {
                    //close door
                    //HasEVDoorClose
                    //_worldMgr.GetEVDoorManager().CloseSFDoor(_curFloor);
                    charController.SetChangeAIStateMachine(AIStateMachine_WaitEVDoorClosed.Instance());
                }
            }
            else
            {
                DoorClose(_worldMgr, charController);
            }
        }
        else
        {
            DoorClose(_worldMgr, charController);
        }
    }

    public override void Exit(CCharacterController_Management charController)
    {
    }


    private void DoorClose(ManagementWorldManager worldMgr, CCharacterController_Management charController)
    {
        EVDoorManager _evDoorMgr = worldMgr.GetEVDoorManager();
        int _curFloor = charController.GetStartFloor();
        if (_evDoorMgr.GetEVCurrentState(_curFloor) != EVDoorSM_CloseSFDoor.Instance())
        {
            //close door
            worldMgr.GetEVDoorManager().CloseSFDoor(_curFloor);
            charController.SetChangeAIStateMachine(AIStateMachine_WaitEVDoorClosed.Instance());
        }
    }
}

public class AIStateMachine_WaitEVDoorClosed : CState<CCharacterController_Management>
{
    static AIStateMachine_WaitEVDoorClosed s_this = null;

    public static AIStateMachine_WaitEVDoorClosed Instance()
    {
        if (s_this == null) { s_this = new AIStateMachine_WaitEVDoorClosed(); }
        return s_this;
    }

    public override void Enter(CCharacterController_Management charController)
    {
    }

    public override void Excute(CCharacterController_Management charController)
    {
        EVDoorManager _evDoorMgr = charController.WorldMgr.GetEVDoorManager();
        if (_evDoorMgr.HasEVSFDoorClose(charController.GetStartFloor()))
        {
            charController.SetChangeAIStateMachine(AIStateMachine_GotoInnerTargetEV.Instance());
        }
    }

    public override void Exit(CCharacterController_Management charController)
    {
    }
}

public class AIStateMachine_GotoInnerTargetEV : CState<CCharacterController_Management>
{
    static AIStateMachine_GotoInnerTargetEV s_this = null;

    public static AIStateMachine_GotoInnerTargetEV Instance()
    {
        if (s_this == null) { s_this = new AIStateMachine_GotoInnerTargetEV(); }
        return s_this;
    }

    public override void Enter(CCharacterController_Management charController)
    {
        charController.SetCurrentFloor(charController.GetTargetFloor());
        //Change EV by Floor
        //remove previous(start) floor at pool
        charController.WorldMgr.AvatarMgr.RemoveAvatarAtEachFloorDic(charController.GetAvatarType(), charController.GetStartFloor());
        //Add now(target) floor at pool
        charController.WorldMgr.AvatarMgr.AddAvatarToEachFloorDic(charController.GetAvatarType(), charController.GetTargetFloor());

        //Set avatar inner position by target floor
        //turn off navmesh
        charController.SetActiveNavMeshAgent(false);
        //다른층에서 현재층으로 왔을 때 이미 다른 멤버가 탑승 중인가를 체크 해줘야 할 것 같은데???
        EVDoorManager _evDoorMgr = charController.WorldMgr.GetEVDoorManager();
        int _myPassengerListIdx = _evDoorMgr.GetAvatarPassengerIdx(charController.GetTargetFloor(), charController.GetAvatarType());
        if(_myPassengerListIdx != -1)
        {
            charController.IdxOfEVInnerPos = _myPassengerListIdx;
        }
        //CDebug.Log($"    %%%% AIStateMachine_GotoInnerTargetEV.Enter() avatar = {charController.GetAvatarType()}, pos index = {charController.IdxOfEVInnerPos}");
        Vector3 _wayPos = charController.WorldMgr.EVInnerPositions[charController.IdxOfEVInnerPos];
        _wayPos.y = charController.GetCharYPositionByFloor();
        charController.transform.localPosition = _wayPos;
        charController.SetActiveNavMeshAgent(true);

        charController.SetTargetFloorEV_MoveFloor();
    }

    public override void Excute(CCharacterController_Management charController)
    {
    }

    public override void Exit(CCharacterController_Management charController)
    {
    }
}

public class AIStateMachine_CheckTargetEVAllArrived : CState<CCharacterController_Management>
{
    static AIStateMachine_CheckTargetEVAllArrived s_this = null;

    public static AIStateMachine_CheckTargetEVAllArrived Instance()
    {
        if (s_this == null) { s_this = new AIStateMachine_CheckTargetEVAllArrived(); }
        return s_this;
    }

    public override void Enter(CCharacterController_Management charController)
    {
        //CDebug.Log("    %%%% AIStateMachine_CheckTargetEVAllArrived.Enter() avatar = " + charController.GetAvatarType());
    }

    public override void Excute(CCharacterController_Management charController)
    {
        //CDebug.Log($"  @@@   AIStateMachine_CheckTargetEVAllArrived.Excute() {charController.GetAvatarType()} HasArrivedAtTargetFloor????");
        ManagementAvatarManager _avatarMgr = charController.WorldMgr.AvatarMgr;
        EVDoorManager _evDoorMgr = charController.WorldMgr.GetEVDoorManager();
        List<AVATAR_TYPE> _list = _evDoorMgr.GetPassengerList(charController.GetTargetFloor());

        if (_list != null)
        {
            if (_avatarMgr.IsArrivedAtTargetFloorEVAllRegisters(_list))
            {
                //CDebug.Log($"  @@@ 00  AIStateMachine_CheckTargetEVAllArrived.Excute() YES, {charController.GetAvatarType()} done !!");
                charController.SetChangeAIStateMachine(AIStateMachine_WaitTargetFloorEVDoorOpen.Instance());
            }
        }
        else
        {
            //CDebug.Log($"  @@@ 11  AIStateMachine_CheckTargetEVAllArrived.Excute() YES, {charController.GetAvatarType()} done !!");
            charController.SetChangeAIStateMachine(AIStateMachine_WaitTargetFloorEVDoorOpen.Instance());
        }
    }

    public override void Exit(CCharacterController_Management charController)
    {
    }
}

public class AIStateMachine_WaitTargetFloorEVDoorOpen : CState<CCharacterController_Management>
{
    static AIStateMachine_WaitTargetFloorEVDoorOpen s_this = null;

    public static AIStateMachine_WaitTargetFloorEVDoorOpen Instance()
    {
        if (s_this == null) { s_this = new AIStateMachine_WaitTargetFloorEVDoorOpen(); }
        return s_this;
    }

    public override void Enter(CCharacterController_Management charController)
    {
        EVDoorManager _evDoorMgr = charController.WorldMgr.GetEVDoorManager();
        _evDoorMgr.OpenTFDoor(charController.GetTargetFloor());
        //_evDoorMgr.CheckEVArriveAtTargetFloor(charController.GetTargetFloor());
    }

    public override void Excute(CCharacterController_Management charController)
    {
        EVDoorManager _evDoorMgr = charController.WorldMgr.GetEVDoorManager();
        if (_evDoorMgr.HasOpenTFDoor(charController.GetTargetFloor()))
        {
            //After Opened Door at Target floor 
            charController.SetChangeAIStateMachine(AIStateMachine_InnerTargetEV.Instance());
        }
    }

    public override void Exit(CCharacterController_Management charController)
    {
    }
}

public class AIStateMachine_InnerTargetEV : CState<CCharacterController_Management>
{
    static AIStateMachine_InnerTargetEV s_this = null;

    public static AIStateMachine_InnerTargetEV Instance()
    {
        if (s_this == null) { s_this = new AIStateMachine_InnerTargetEV(); }
        return s_this;
    }

    public override void Enter(CCharacterController_Management charController)
    {
        //CDebug.Log("    %%%% AIStateMachine_InnerTargetEV.Enter() avatar = " + charController.GetAvatarType());
        charController.SetisArriveComingAvatar(true);
        //wait target pos in ev
        Vector3 _wayPos = charController.WorldMgr.EVGetOffPositions[charController.IdxOfEVInnerPos];
        _wayPos.y += charController.WorldMgr.GridElementHeight * charController.GetTargetFloor();

        charController.SetNavMeshDestination(_wayPos);

        //CDebug.Log("    %%%% AIStateMachine_InnerTargetEV.Enter() set animation Walk avatar = " + charController.GetAvatarType());
        charController.SetAnimationTrigger("walk");
    }

    public override void Excute(CCharacterController_Management charController)
    {
        //check avatar reach inner position
        if (charController.IsArrivedNavMeshAgentDestination())
        {
            charController.SetChangeAIStateMachine(AIStateMachine_OutofTargetEV.Instance());
        }
    }

    public override void Exit(CCharacterController_Management charController)
    {
    }
}

public class AIStateMachine_OutofTargetEV : CState<CCharacterController_Management>
{
    static AIStateMachine_OutofTargetEV s_this = null;

    public static AIStateMachine_OutofTargetEV Instance()
    {
        if (s_this == null) { s_this = new AIStateMachine_OutofTargetEV(); }
        return s_this;
    }

    public override void Enter(CCharacterController_Management charController)
    {
        //CDebug.Log("    %%%% AIStateMachine_OutofTargetEV.Enter() avatar = " + charController.GetAvatarType());

        ////다른 아바타가 엘베를 기다리는 상태이면 문을 닫지 않아야 한다
        EVDoorManager _evDoorMgr = charController.WorldMgr.GetEVDoorManager();
        _evDoorMgr.RemoveEVList(charController.GetStartFloor(), charController.GetTargetFloor());

        if (charController.GetPrevAISMState() != AIStateMachine_Interaction_Play.Instance())
        {
            //다른 아바타가 엘베를 기다리는 상태이면 문을 닫지 않아야 한다
            if (charController.WorldMgr.AvatarMgr.IsSomebodyAroundOfEV(charController.GetAvatarType(), charController.GetTargetFloor()) == false)
            {
                //CDebug.Log("    %%%% AIStateMachine_OutofTargetEV.Enter() avatar = " + charController.GetAvatarType() + "/ floor = " + charController.GetTargetFloor());
                _evDoorMgr.CloseTFDoor(charController.GetTargetFloor());
            }

        }
        //set destination for last
        Vector3 _wayPos = charController.WayPoint.GetPositionByWayPointState(WAYPOINT_WAIT_STATE.ARRIVED);
        charController.SetNavMeshDestination(_wayPos);

        //CDebug.Log("    %%%% AIStateMachine_OutofTargetEV.Enter() set animation Walk avatar = " + charController.GetAvatarType());
        charController.SetAnimationTrigger("walk");

        if (charController.WorldMgr.AvatarMgr.GetAvatarQuestState( charController.GetAvatarType() ) == GroupManagement.ManagementEnums.QUEST_STATE.RECEIVED)
        {
            charController.WorldMgr.AvatarMgr.SetActiveAvatarMailQuestIcon( charController.GetAvatarType(), true );
        }
    }

    public override void Excute(CCharacterController_Management charController)
    {
        if (charController.IsArrivedNavMeshAgentDestination())
        {
            charController.SetLastWayPoint();
        }
        else
        {
            charController.CheckingAvatarInteractionCondition();
        }
    }

    public override void Exit(CCharacterController_Management charController)
    {
    }
}



////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////


public class AIStateMachine_Touch : CState<CCharacterController_Management>
{
    static AIStateMachine_Touch s_this = null;

    public static AIStateMachine_Touch Instance()
    {
        if (s_this == null) { s_this = new AIStateMachine_Touch(); }
        return s_this;
    }

    public override void Enter(CCharacterController_Management charController)
    {
        AVATAR_TYPE aType = charController.GetAvatarType();
        GameObject _avatarObj = charController.WorldMgr.AvatarMgr.GetManagementAvatarObj(aType);

        Animator _animator = charController.GetAnimatorController();
        //CDebug.Log("    %%%% AIStateMachine_Touch.Enter() avatar = " + aType);

        charController.KillDoFunc();
        charController.SetAITimeDispose();

        //ai pause setting then resume setting in each ai state machine
        //stop navmesh
        charController.PauseWayPoint();
        //stop elevator(time??)

        TouchedEventManager.SetTouchState(aType, TOUCHEVENT_SCENE_TYPE.MANAGEMENT, _avatarObj, _animator);
    }

    public override void Excute(CCharacterController_Management charController)
    {
        AVATAR_TYPE aType = charController.GetAvatarType();
        Animator _animator = charController.GetAnimatorController();
        float _curClipLength = _animator.GetCurrentAnimatorClipInfo(0)[0].clip.length;

        TouchOutputInfo _outPutInfo = TouchedEventManager.GetCurrentTouchOutPutInfo(aType);

        ManagementAvatarManager _avatarMgr = charController.WorldMgr.AvatarMgr;
        MotionData _data = CAvatarInfoDataManager.GetMotionDataByID(_outPutInfo.MotionID);
        //float _fLength = _avatarMgr.GetAvatarClipLength(aType, _data.AnimName);
        string runAnimControllerKey = aType.ToString();
        long groupID = (long)aType;
        float _fLength = CRuntimeAnimControllerSwitchManager.Instance.GetAnimationClipLength( runAnimControllerKey, groupID, _data.AnimName, ANIMCONTROLLER_USE_TYPE.MANAGEMENT );
        if (_fLength > 0)
        {
            _curClipLength = _fLength;
        }

        if (TouchedEventManager.Proc_TouchState(aType, _curClipLength))
        {
            charController.SetChangePrevAIStateMachine();
        }
    }

    public override void Exit(CCharacterController_Management charController)
    {
        //charController.RemoveTouchEventReward();
        charController.ResumeWayPoint();
        //play animation with touch event param
        //play effect with touch event effect
    }
}


public class AIStateMachine_Event : CState<CCharacterController_Management>
{
    static AIStateMachine_Event s_this = null;

    public static AIStateMachine_Event Instance()
    {
        if (s_this == null) { s_this = new AIStateMachine_Event(); }
        return s_this;
    }

    public override void Enter(CCharacterController_Management charController)
    {
        charController.KillDoFunc();
        //CDebug.Log("    %%%% AIStateMachine_Event.Enter() avatar = " + charController.GetAvatarType());
        CEvent _evt = charController.WorldMgr.AvatarMgr.GetManagementAvatarEvent(charController.GetAvatarType());
        if (_evt != null)
        {
            charController.SetAnimationTrigger(_evt.Event_Target_Ani);
            charController.SetIsFinishAnimPlay(false);
        }
        else
        {
            CDebug.LogError("AIStateMachine_Event.Enter() CEvent is null");
        }


        SingleAssignmentDisposable disposer = new SingleAssignmentDisposable();
        disposer.Disposable = Observable.Timer( TimeSpan.FromSeconds( 3 ) )
            .Subscribe( _ =>
            {
                CEvent _evt = charController.WorldMgr.AvatarMgr.GetManagementAvatarEvent( charController.GetAvatarType() );
                if (_evt != null)
                {
                    charController.SetAnimationTrigger( _evt.Event_Delay_Ani );
                    charController.SetIsFinishAnimPlay( true );
                }
                else
                {
                    CDebug.LogError( "AIStateMachine_Event.Excute() CEvent is null" );
                }

                disposer.Dispose();
            } ).AddTo( charController.gameObject );
    }

    public override void Excute(CCharacterController_Management charController)
    {
    }

    public override void Exit(CCharacterController_Management charController)
    {
    }
}

public class AIStateMachine_WithOutEvent : CState<CCharacterController_Management>
{
    static AIStateMachine_WithOutEvent s_this = null;

    public static AIStateMachine_WithOutEvent Instance()
    {
        if (s_this == null) { s_this = new AIStateMachine_WithOutEvent(); }
        return s_this;
    }

    public override void Enter(CCharacterController_Management charController)
    {
        //CDebug.Log("    %%%% AIStateMachine_WithOutEvent.Enter() avatar = " + charController.GetAvatarType());

        charController.KillDoFunc();
        charController.SetAITimeDispose();
        charController.SetAnimationTrigger("idle");
        //Stop NavMeshAgent
        charController.PauseWayPoint();
        charController.SetActiveNavMeshAgent(false);

        int waitTime = UnityEngine.Random.Range(1, 3);
        //CDebug.Log("    %%%% AIStateMachine_WithOutEvent.Enter() avatar = " + charController.GetAvatarType()+"/ waitTime = "+ waitTime);
        charController.AIWaitDisposer = new SingleAssignmentDisposable();
        //charController.InitAiTimeDisposer();
        charController.AIWaitDisposer.Disposable = Observable.Timer(TimeSpan.FromSeconds(waitTime))
        .Subscribe(_ =>
        {
            //CDebug.Log("    %%%% AIStateMachine_WithOutEvent.Enter() avatar = " + charController.GetAvatarType() + "/ call SetTargetGrid_InPatrol");
            charController.SetActiveNavMeshAgent(true);
            charController.ResumeWayPoint();
            charController.SetTargetGrid_InPatrol();
            charController.AIWaitDisposer.Dispose();
        })
        .AddTo(charController);
    }

    public override void Excute(CCharacterController_Management charController)
    {
    }

    public override void Exit(CCharacterController_Management charController)
    {
    }
}


public class AIStateMachine_Interaction_Wait : CState<CCharacterController_Management>
{
    static AIStateMachine_Interaction_Wait s_this = null;

    public static AIStateMachine_Interaction_Wait Instance()
    {
        if (s_this == null) { s_this = new AIStateMachine_Interaction_Wait(); }
        return s_this;
    }

    public override void Enter(CCharacterController_Management charController)
    {
        //CDebug.Log("    %%%% AIStateMachine_Interaction_Wait.Enter() avatar = " + charController.GetAvatarType());

        //Stop NavMeshAgent
        charController.PauseWayPoint();
        charController.SetActiveNavMeshAgent(false);

        charController.SetAnimationTrigger("idle");
    }

    public override void Excute(CCharacterController_Management charController)
    {
        charController.CheckAvatarQuestState();
    }

    public override void Exit(CCharacterController_Management charController)
    {
    }
}

public class AIStateMachine_Interaction_Play : CState<CCharacterController_Management>
{
    static AIStateMachine_Interaction_Play s_this = null;

    public static AIStateMachine_Interaction_Play Instance()
    {
        if (s_this == null) { s_this = new AIStateMachine_Interaction_Play(); }
        return s_this;
    }

    public override void Enter(CCharacterController_Management charController)
    {
        string animParam = charController.GetAnimationParam();
        //CDebug.Log("    %%%% AIStateMachine_Interaction_Play.Enter() avatar = " + charController.GetAvatarType()+"/ animation = "+ animParam);
        charController.SetAnimationTrigger(animParam);
    }

    public override void Excute(CCharacterController_Management charController)
    {
        charController.CheckAvatarQuestState();
    }

    public override void Exit(CCharacterController_Management charController)
    {
        if (!charController.IsInteractionPlayEnterAvatarEquip)
        {
            charController.AnimEvent.HideAll();
            charController.SetActiveNavMeshAgent(true);
            charController.ResumeWayPoint();
        }
    }
}


public class AIStateMachine_Training : CState<CCharacterController_Management>
{
    static AIStateMachine_Training s_this = null;

    public static AIStateMachine_Training Instance()
    {
        if (s_this == null) { s_this = new AIStateMachine_Training(); }
        return s_this;
    }

    public override void Enter(CCharacterController_Management charController)
    {
        //CDebug.Log("    %%%% AIStateMachine_Training.Enter() avatar = " + charController.GetAvatarType());
        ManagementWorldManager _worldMgr = charController.WorldMgr;
        CState<CCharacterController_Management> prevState = charController.GetPrevAISMState();

        if (prevState != AIStateMachine_Wait.Instance())
        {
            AVATAR_TYPE avatarType = charController.GetAvatarType();
            GameObject avatarObj = _worldMgr.AvatarMgr.GetManagementAvatarObj(avatarType);
            StaticAvatarManager.Instance.SetControlDynamicBoneWeight(avatarType, avatarObj);
            StaticAvatarManager.Instance.StopControlDynamicBoneWeightCorroutine();
            charController.KillDoFunc();
            _worldMgr.AvatarMgr.DisposeDelayTimerDisposer(avatarType);
            _worldMgr.AvatarMgr.DisposeInteractionDisposer(avatarType);

            //EV Door
            charController.SetAvatarEvEncountTraining();

            //Touched UI
            TouchedEventManager.SetActiveTouchEvtUIObject(avatarType, TOUCHEVENT_SCENE_TYPE.MANAGEMENT, false);

            charController.SetAITimeDispose();
            //Stop NavMeshAgent
            charController.PauseWayPoint();
            charController.SetActiveNavMeshAgent(false);

            StaticAvatarManager.Instance.StartControlDynamicBoneWeightCorroutine();

            charController.SetAnimationParam("idle");
            charController.SetAnimationTrigger(charController.GetAnimationParam());
        }
    }

    public override void Excute(CCharacterController_Management charController)
    {
    }

    public override void Exit(CCharacterController_Management charController)
    {
    }
}


public class AIStateMachine_Wait : CState<CCharacterController_Management>
{
    static AIStateMachine_Wait s_this = null;

    public static AIStateMachine_Wait Instance()
    {
        if (s_this == null) { s_this = new AIStateMachine_Wait(); }
        return s_this;
    }

    public override void Enter(CCharacterController_Management charController)
    {
        //CDebug.Log("    %%%% AIStateMachine_Wait.Enter() avatar = " + charController.GetAvatarType());
        AVATAR_TYPE avatarType = charController.GetAvatarType();
        charController.KillDoFunc();
        charController.WorldMgr.AvatarMgr.SetActiveAvatarInteractionIcon(avatarType, false);
        charController.SetAITimeDispose();
        CCharacterController_Management _coupleAvatar = charController.GetCoupleAvatarInteraction();
        if (_coupleAvatar != null)
        {
            _coupleAvatar.WorldMgr.AvatarMgr.SetActiveAvatarInteractionIcon(avatarType, false);
        }

        charController.WorldMgr.AvatarMgr.InitAvatarHavingComingAvatar(avatarType);
        charController.WorldMgr.AvatarMgr.DisposeDelayTimerDisposer(avatarType);
        charController.WorldMgr.AvatarMgr.DisposeInteractionDisposer(avatarType);
    }

    public override void Excute(CCharacterController_Management charController)
    {
    }

    public override void Exit(CCharacterController_Management charController)
    {
    }
}