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


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GroupManagement;
using UniRx;
using DG.Tweening;
using GroupManagement.ManagementEnums;

public class CCharacterController_Management : CCharacterController
{
    //------------------------Management
    [HideInInspector]
    public ManagementWorldManager WorldMgr;
    private SectionScript MySectionScript;

    #region AI_VAR
    //AI
    [HideInInspector]
    public CStateMachine<CCharacterController_Management> AIStateMachine;
    private CCharacterController_Management ComingAvatarInEV;
    public bool isArriveComingAvatar;
    public CAIWayPoint WayPoint;
    private int CurFloor;
    public GridIndex AI_Target_Point;
    [HideInInspector]
    public AITime TimeAI;//ai time
    public float AI_Timer;

    public int IdxOfEVInnerPos;
    public bool IsInteractionPlayEnterAvatarEquip;

    //Touch Event
    private CState<CCharacterController_Management> StateBeforeInteraction;
    private CCharacterController_Management InteractionCoupleAvatar;

    private SingleAssignmentDisposable AITimeDisposer;
    public SingleAssignmentDisposable AIWaitDisposer;
    #endregion AI_VAR

    #region Activity
    int currentActivitySectionType;
    int currentActivitySectionSubType;
    #endregion Activity

    //NPC
    //private long NpcUID;

    public override void Initialize(CHARACTER_TYPE cType)
    {
        if (ManagementManager.Instance != null)
        {
            WorldMgr = ManagementManager.Instance.worldManager;
        }

        SetThisObject(gameObject);
        base.Initialize(cType);

        SetCharacterType(cType);

        AIStateMachine = new CStateMachine<CCharacterController_Management>(this);
        WayPoint = new CAIWayPoint();
        TimeAI = new AITime();

        if (GetCharacterType() == CHARACTER_TYPE.NPC)
        {
            if (StateMachine != null)
            {
                //default motion
                StateMachine.ChangeState(CharacterState_ManagementSpawnWait.Instance());
            }
            SetNpcUID(0);
        }



#if UNITY_EDITOR
        HoldObjInfo = new CBTHoldObjectInfo();
#endif

    }



    // Update is called once per frame
    protected override void Update()
    {
        if (GetCharacterType() == CHARACTER_TYPE.AVATAR)
        {
            if (AIStateMachine != null)
            {
                AIStateMachine.StateMachine_Update();
            }

            AI_TimerProc();
        }
    }




    //// ----  npc -----

    public void SetNPCSectionScript(SectionScript os)
    {
        MySectionScript = os;
    }

    public Vector3 GetTargetPositionWithSection(Vector3 pos)
    {
        Vector3 targetPos = pos;
        if (MySectionScript != null)
        {
            GridIndex _grid = MySectionScript.sectionInstanceData.GetLeftMostIndex();
            targetPos = new Vector3((_grid.X * WorldMgr.GridElementWidth) + pos.x, _grid.Y * WorldMgr.GridElementHeight + pos.y, pos.z);
        }

        return targetPos;
    }
    public void EventProc()
    {
        if (GetCharacterType() == CHARACTER_TYPE.NPC)
        {
            InvokeRepeating (nameof (CheckEventNpc), 3, 3);
        }
    }
    public void StopCheckingEventOnEachNPC()
    {
        CancelInvoke(nameof (CheckEventNpc));
    }

    public void CheckEventNpc()
    {
        if (GetNpcUID() == 0)
            return;

        WorldMgr.NpcMgr.CheckEvent(GetNpcUID());
    }

    //// ----  end of npc -----

    public CEvent GetOccurEvent()
    {
        AVATAR_TYPE avatarType = GetAvatarType();
        if (avatarType == AVATAR_TYPE.AVATAR_NONE)
            return null;

        return WorldMgr.AvatarMgr.GetEvent(avatarType);
    }

    public void SetAvatarActivity(ACTIVITY_TYPE activityType, int activitysectiontype, int activitysectionsubtype)
    {
        AVATAR_TYPE avatarType = GetAvatarType();
        currentActivitySectionType = activitysectiontype;
        currentActivitySectionSubType = activitysectionsubtype;

        if (
               AIStateMachine.GetCurrentState() == AIStateMachine_Interaction_Wait.Instance()
            || AIStateMachine.GetCurrentState() == AIStateMachine_Interaction_Play.Instance()
            )
        {
            switch(activityType)
            {
                case ACTIVITY_TYPE.TRAINING:
                    //Change me to training
                    WorldMgr.AvatarMgr.ExitAvatarInteraction(avatarType, AIStateMachine_Training.Instance());
                    break;
                case ACTIVITY_TYPE.CONDITION:
                case ACTIVITY_TYPE.PHOTOSTUDIO:
                case ACTIVITY_TYPE.TRENDY:
                    WorldMgr.AvatarMgr.ExitAvatarInteraction(avatarType, AIStateMachine_Training.Instance());
                    break;
            }

            //Then Change who 'couple avatar' to previous state
            CCharacterController_Management coupleCntlr = GetCoupleAvatarInteraction();
            coupleCntlr.SetActiveNavMeshAgent(true);
            WorldMgr.AvatarMgr.ExitAvatarInteraction(coupleCntlr.GetAvatarType(), coupleCntlr.GetStateBeforeAvatarInteraction());
            coupleCntlr.ResumeWayPoint();
        }
        else
        {
            WorldMgr.AvatarMgr.CancelEvent(avatarType);
                        
            switch (activityType)
            {
                case ACTIVITY_TYPE.TRAINING:
                    WorldMgr.AvatarMgr.ExitAvatarInteraction(avatarType, AIStateMachine_Training.Instance());
                    break;
                case ACTIVITY_TYPE.CONDITION:
                case ACTIVITY_TYPE.PHOTOSTUDIO:
                case ACTIVITY_TYPE.TRENDY:
                    //Change me to training
                    WorldMgr.AvatarMgr.ExitAvatarInteraction(avatarType, AIStateMachine_Training.Instance());
                    break;
            }
        }
    }

    public void InitializeCurrentActivitySectionTypeInfo()
    {
        currentActivitySectionType = 0;
        currentActivitySectionSubType = 0;
    }

    public int GetCurrentActivitySectionType()
    {
        return currentActivitySectionType;
    }

    public int GetCurrentActivitySectionSubType()
    {
        return currentActivitySectionSubType;
    }



    #region AI_STATE_PROC
    public void InitAIStateMachine(GridIndex grid)
    {
        if (AIStateMachine != null)
        {
            AI_Target_Point = grid;
            WayPoint.SetFloors(grid.Y, grid.Y);

            SetChangeAIStateMachine(AIStateMachine_Spawn.Instance());
        }
    }

    public void AI_TimerProc()
    {
        AI_Timer += Time.deltaTime;
    }

    public float GetAITimer()
    {
        return AI_Timer;
    }

    public void SetChangeNavMeshDestByIdxOfPos()
    {
        Vector3 _wayPos = WorldMgr.EVWaitPositions[IdxOfEVInnerPos];
        _wayPos.y += WorldMgr.GridElementHeight * GetStartFloor();
        SetNavMeshDestination(_wayPos);
    }

    public void MakeWayPoint(GridIndex targetGird)
    {
        AVATAR_TYPE _avatarType = GetAvatarType();
        AI_Target_Point = targetGird;
        GridIndex _curGrid = WorldMgr.AvatarMgr.GetAvatarCurrentGridDic(_avatarType);
        Vector3 _targetPos = WorldMgr.AvatarMgr.GetAvatarPositionByGrid(targetGird);

        WayPoint.Initialize();

        if (_curGrid != targetGird)
        {
            if (_curGrid.Y != targetGird.Y)
            {
                //--------- Start Floor ---------------
                WayPoint.SetFloors(_curGrid.Y, targetGird.Y);
                SetCurrentFloor(GetStartFloor());
                //pause point
                Vector3 _elevatorWaitPos = WorldMgr.EVWaitPositions[0];
                _elevatorWaitPos.y = GetStartFloor() * WorldMgr.GridElementHeight;
                WayPoint.SetAddWayPoint(WAYPOINT_WAIT_STATE.SF_WAIT, _elevatorWaitPos);

                //CDebug.Log($"MakeWayPoint {GetAvatarType()},  SF. ev wait pos = {_elevatorWaitPos}");

                //pause point
                Vector3 _elevatorInnerPos = _elevatorWaitPos;
                _elevatorInnerPos.z = WorldMgr.BaseElevatorDepth;
                WayPoint.SetAddWayPoint(WAYPOINT_WAIT_STATE.SF_INNER, _elevatorInnerPos);
                //CDebug.Log("MakeWayPoint CF. ev inner pos = " + _elevatorInnerPos);


                //--------- Target Floor ---------------
                Vector3 _elevatorTargetPos = _elevatorInnerPos;
                _elevatorTargetPos.y = GetTargetFloor() * WorldMgr.GridElementHeight;
                WayPoint.SetAddWayPoint(WAYPOINT_WAIT_STATE.TF_INNER, _elevatorTargetPos);
                //CDebug.Log("MakeWayPoint TF. ev inner pos = " + _elevatorTargetPos);

                Vector3 _elevatorTargetWaitPos = WorldMgr.EVWaitPositions[1];// _elevatorTargetPos;
                _elevatorTargetWaitPos.x = WorldMgr.EVWaitPositions[1].x + 0.3f;//WorldMgr.BaseElevatorWaitPosition.x + 0.4f;
                _elevatorTargetWaitPos.y = GetTargetFloor() * WorldMgr.GridElementHeight;
                WayPoint.SetAddWayPoint(WAYPOINT_WAIT_STATE.TF_WAIT, _elevatorTargetWaitPos);
                //CDebug.Log("MakeWayPoint TF. ev inner pos = " + _elevatorTargetWaitPos);
            }
        }

        //TargetPos
        WayPoint.SetAddWayPoint(WAYPOINT_WAIT_STATE.ARRIVED, _targetPos);
        //CDebug.Log("MakeWayPoint TF. target pos = " + _targetPos);
    }

    public void PauseWayPoint()
    {
        SetStopNavMeshAgent(true);
        WayPoint.PauseWayPoint();
    }

    public void ResumeWayPoint()
    {
        SetStopNavMeshAgent(false);
        WayPoint.ResumeWayPoint();
    }

    public void SetLastWayPoint()
    {
        transform.DOLocalRotate(new Vector3(0, 180, 0), 0.5f).OnComplete(() =>
        {
            WorldMgr.AvatarMgr.SetAvatarCurrentGridDic(GetAvatarType(), AI_Target_Point);
            //Finish Way point
            SetChangeAIStateMachine(AIStateMachine_Patrol.Instance());
        });
    }

    public void SetReSpawnAvatar()
    {
        AVATAR_TYPE avatarType = GetAvatarType();
        WorldMgr.AvatarMgr.SetActiveAvatarEventIcon(avatarType, false);

        //Respawn when avatar get out of Training Room
        WorldMgr.AvatarMgr.MakeRandomSpawnOfEachAvatar(avatarType);

        GridIndex _myGrid = WorldMgr.AvatarMgr.GetGridIndexInGridPoolDicByAvatar(avatarType);
        if (_myGrid != null)
        {
            InitAIStateMachine(_myGrid);
        }
    }

    public void CheckAvatarQuestState()
    {
        if (WorldMgr.AvatarMgr.GetAvatarQuestState( GetAvatarType() ) == GroupManagement.ManagementEnums.QUEST_STATE.RECEIVED)
        {
            SetAvatarInteractionExit();

            CCharacterController_Management _coupleAvatar = GetCoupleAvatarInteraction();
            if (_coupleAvatar != null)
            {
                _coupleAvatar.WorldMgr.AvatarMgr.SetActiveAvatarInteractionIcon( _coupleAvatar.GetAvatarType(), false );
                _coupleAvatar.SetAvatarInteractionExit();
            }
        }
    }

    public void SetAvatarInteractionExit()
    {
        AnimEvent.HideAll();
        SetActiveNavMeshAgent( true );
        ResumeWayPoint();
        WorldMgr.AvatarMgr.SetExitAvatarInteraction( GetAvatarType() );
    }

    public GridIndex GetTargetGrid()
    {
        return AI_Target_Point;
    }

    public int GetStartFloor()
    {
        return WayPoint.GetStartFloor();
    }

    public int GetTargetFloor()
    {
        return WayPoint.GetTargetFloor();
    }

    public void SetCurrentFloor(int floor)
    {
        CurFloor = floor;
    }

    public int GetCurrentFloor()
    {
        return CurFloor;
    }


    public void SetAIStateEnter_Patrol()
    {
        AVATAR_TYPE _avatarType = GetAvatarType();
        GroupManagement.ManagementEnums.EVENT_STATE _eventState = WorldMgr.AvatarMgr.GetAvatarEventState(_avatarType);

        if (GetPrevAISMState() != AIStateMachine_Spawn.Instance())
        {
            if (_eventState != GroupManagement.ManagementEnums.EVENT_STATE.PROCESSING)
            {
                CEvent _avatarEvent = GetOccurEvent();

                //CDebug.Log("SetAIStateEnter_Patrol() GetOccurEvent = " + _avatarEvent);

                if (_avatarEvent != null)
                {
                    WorldMgr.AvatarMgr.OccurEventProc(_avatarType, _avatarEvent);
                    SetChangeAIStateMachine(AIStateMachine_Event.Instance());
                }
                else
                {
                    SetChangeAIStateMachine(AIStateMachine_WithOutEvent.Instance());
                }
            }
            else
            {
                WorldMgr.AvatarMgr.SetAvatarEventState(_avatarType, GroupManagement.ManagementEnums.EVENT_STATE.NONE);
                SetChangeAIStateMachine(AIStateMachine_WithOutEvent.Instance());
            }
        }
        else
        {
            //SetTargetGrid_InPatrol();
            SetChangeAIStateMachine(AIStateMachine_WithOutEvent.Instance());
        }
    }

    public void SetTargetGrid_InPatrol()
    {
        GridIndex _targetGrid = WorldMgr.AvatarMgr.GetAvatarRandomTargetGrid(GetAvatarType());

        if (WayPoint.GetWayPointWaitState(0) == WAYPOINT_WAIT_STATE.SF_WAIT)
        {
            SetChangeAIStateMachine(AIStateMachine_GotoFrontStartEV.Instance());
        }
        else
        {
            SetChangeAIStateMachine(AIStateMachine_GotoSameFloorTarget.Instance());
        }
    }

    public bool CanCheckAvatarInteractionByState()
    {
        CState<CCharacterController_Management> _curState = GetCurrentAISMState();

        if (WorldMgr.AvatarMgr.GetAvatarQuestState( GetAvatarType() ) == QUEST_STATE.RECEIVED)
        {
            return false;
        }

        if ( _curState == AIStateMachine_GotoFrontStartEV.Instance()    || 
             _curState == AIStateMachine_GotoSameFloorTarget.Instance() ||
             _curState == AIStateMachine_OutofTargetEV.Instance())
        {
            return true;
        }

        return false;
    }

    public void CheckingAvatarInteractionCondition()
    {
        // 튜토리얼이 끝나지 않았으면 무시
        if (TutorialManager.Instance.IsRunTutorial || WorldMgr.AvatarMgr.GetAvatarQuestState(GetAvatarType()) == QUEST_STATE.RECEIVED)
        {
            return;
        }

        if (AIStateMachine.GetPreviousState() != AIStateMachine_Interaction_Play.Instance())
        {
            //checking direction
            //Search someone has left dir when my dir is right in same floor
            //Then check distance of both is less than 1.5
            //Interaction_Wait, Interaction_Play
            Vector3 _myDir = RoundVector3(transform.forward.normalized);//transform.localPosition.normalized;
            if (_myDir.Equals(Vector3.right))
            {
                if (CanCheckAvatarInteractionByState())
                {
                    CCharacterController_Management _someoneInFloor = WorldMgr.AvatarMgr.GetAvatarInSameFloor(GetAvatarType(), GetCurrentFloor());
                    if (_someoneInFloor != null)
                    {
                        //CDebug.Log($" _someoneInFloor = {_someoneInFloor.GetAvatarType()} state = {_someoneInFloor.AIStateMachine.GetCurrentState()}");
                        if (_someoneInFloor.CanCheckAvatarInteractionByState())
                        {
                            Vector3 _someoneDir = RoundVector3(_someoneInFloor.transform.forward.normalized);
                            CompareInteractionPosByDirection(_someoneInFloor, _someoneDir);
                        }
                    }
                }
            }
        }
    }

    private void CompareInteractionPosByDirection(CCharacterController_Management someoneInFloor, Vector3 someoneDir)
    {
        if (someoneDir.Equals(Vector3.left))
        {
            if (someoneInFloor.transform.localPosition.x > transform.localPosition.x)
            {
                float _dist = Vector3.Distance(transform.localPosition, someoneInFloor.transform.localPosition);
                SetAvatarInteraction(someoneInFloor, _dist);
            }
        }
    }

    private void SetAvatarInteraction(CCharacterController_Management someoneInFloor, float dist)
    {
        if (dist > Constants.AVATAR_INTERACTION_CHECK_MIN_DIST && dist < Constants.AVATAR_INTERACTION_CHECK_MAX_DIST)
        {
            //Set Couple Avatar each by each
            SetInteractionCoupleAvatar(someoneInFloor);
            someoneInFloor.SetInteractionCoupleAvatar(this);

            AVATAR_TYPE _myAvatar = GetAvatarType();

            //Select Interaction Data in group
            AvatarInteractionData _interacData = ManagementDataManager.Instance.GetAvatarInteractionDataByRandomValue(_myAvatar);
            //Main
            WorldMgr.AvatarMgr.SetAvatarInteractionIcon(_myAvatar, someoneInFloor.GetAvatarType(), _interacData, true);
            //Sub
            WorldMgr.AvatarMgr.SetAvatarInteractionIcon(someoneInFloor.GetAvatarType(), _myAvatar, _interacData, false);

            //AI State Machine - interaction_wait
            SetStateBeforeAvatarInteraction(AIStateMachine.CurState);
            SetChangeAIStateMachine(AIStateMachine_Interaction_Wait.Instance());
            someoneInFloor.SetStateBeforeAvatarInteraction(someoneInFloor.AIStateMachine.CurState);
            someoneInFloor.SetChangeAIStateMachine(AIStateMachine_Interaction_Wait.Instance());
        }
    }

    public void SetComingAvatarInEV(CCharacterController_Management avatar)
    {
        ComingAvatarInEV = avatar;
    }

    public CCharacterController_Management GetComingAvatarInEV()
    {
        return ComingAvatarInEV;
    }

    public void SetisArriveComingAvatar(bool arrived)
    {
        isArriveComingAvatar = arrived;
    }

    public bool GetisArriveComingAvatar()
    {
        return isArriveComingAvatar;
    }

    private void SetStateBeforeAvatarInteraction(CState<CCharacterController_Management> state)
    {
        StateBeforeInteraction = state;
    }

    public CState<CCharacterController_Management> GetStateBeforeAvatarInteraction()
    {
        return StateBeforeInteraction;
    }

    public void SetInteractionCoupleAvatar(CCharacterController_Management cntlr)
    {
        InteractionCoupleAvatar = cntlr;
    }

    public CCharacterController_Management GetCoupleAvatarInteraction()
    {
        return InteractionCoupleAvatar;
    }

    public void AvatarRotate(float angle)
    {
        transform.DOLocalRotate(new Vector3(0, angle, 0), 0.5f);
        //CDebug.Log($"       ######### AIStateMachine_PauseWayPoint.Excute AvatarRotate {charController.GetAvatarType()}, rotation = {charController.transform.localRotation}");
    }

    private void InitAiTimeDisposer()
    {
        if (AITimeDisposer == null)
        {
            AITimeDisposer = new SingleAssignmentDisposable();
        }
        else
        {
            AITimeDisposer.Dispose();
            AITimeDisposer = new SingleAssignmentDisposable();
        }
    }

    public void SetAITimeDispose()
    {
        if (AITimeDisposer != null)
            AITimeDisposer.Dispose();
        if(AIWaitDisposer != null)
        {
            AIWaitDisposer.Dispose();
        }
    }

    public void SetTargetFloorEV_MoveFloor()
    {
        int _startFloor = GetStartFloor();
        int _targetFloor = GetTargetFloor();
        float evMoveTime = Mathf.Abs(_targetFloor - _startFloor) /* * EVManager.EV_MOVE_TIME_AFLOOR*/;

        InitAiTimeDisposer();
        AITimeDisposer.Disposable = Observable.Timer(System.TimeSpan.FromSeconds(evMoveTime))
            .Subscribe(_ =>
            {
                SetChangeAIStateMachine(AIStateMachine_CheckTargetEVAllArrived.Instance());
                AITimeDisposer.Dispose();
            });
    }

    public void SetChangeAIStateMachine(CState<CCharacterController_Management> state, string animParam = "")
    {
        if (animParam.Equals("") == false)
        {
            SetAnimationParam(animParam);
        }
        AIStateMachine.ChangeState(state);
    }

    public CState<CCharacterController_Management> GetCurrentAISMState()
    {
        return AIStateMachine.GetCurrentState();
    }

    public CState<CCharacterController_Management> GetPrevAISMState()
    {
        return AIStateMachine.GetPreviousState();
    }

    public void SetChangePrevAIStateMachine()
    {
        AIStateMachine.ChangeState(AIStateMachine.GetPreviousState());
    }

    public float GetCharYPositionByFloor()
    {
        return 0.1f + WorldMgr.GridElementHeight * GetTargetFloor();
    }
    #endregion AI_STATE_PROC





    public void DisposerDispose()
    {
        if (AIWaitDisposer != null)
        {
            AIWaitDisposer.Dispose();
        }
    }


    protected override void OnDestroy()
	{
		base.OnDestroy();

		if( AIStateMachine != null )
		{
			AIStateMachine.DestroyStateMachine();
			AIStateMachine = null;
		}

		SetAITimeDispose();

        StopCheckingEventOnEachNPC();

    }
}