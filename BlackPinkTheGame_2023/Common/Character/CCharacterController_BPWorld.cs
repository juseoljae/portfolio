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

using UnityEngine;
using System.Runtime.InteropServices;
using System;
using BehaviorDesigner.Runtime;
using BTInteractionState = CCharacterController_BPWorld_BTInteractionState;
using JumpController = CCharacterController_BPWorld_Jump;
using TGP.Core;

public class CCharacterController_BPWorld : CCharacterController
{
    private const float SHORTCUT_INDICATOR_HEIGHT = 0.3f;
    private const string SHORTCUT_INDICATOR_PATH = "Views/BlackpinkWorld/Open/Prefabs/shortcut_arrow_new.prefab";//"Views/Metaverse/Prefabs/shortcut_arrow.prefab";
    public const float MINIMUM_MOVE_DIST = 0.1f;

    private static readonly float DISTANCE_NOT_MOVE = 0.001f;
    private static readonly float DISTANCE_MIN_WALK = 0.3f;
    private static readonly float DISTANCE_MIN_RUN = 2.4f;
    private static readonly float DISTANCE_BT_CHECK = 1.0f;

    [StructLayout(LayoutKind.Explicit)]
    private struct FloatIntUnion
    {
        [FieldOffset(0)]
        public float f;

        [FieldOffset(0)]
        public int tmp;
    }
	public enum EOtherPlayerMoveState
	{
		STOP = 0,
		WALK,
		RUN,
	}
	//------------------------BPWorld
	private BPWorldAvatarAppearFXController appearFXController = null;
    private BPWorldCharacterData BpwCharData;
    private Transform MainCam;
    private CharacterController moveController;
    private float turnSmoothVelocity = 0.0f;
    private float turnSmoothTime = 0.1f;
    public float JoyStickMagnitude;
    private float warpDistance = 0.0f;
	private float fixPosDistance = 0.0f;

    #region CharacterController_Inner

    //Move
    public float MoveSpeed = 3f;
    public float WalkSpeed = 1.3f;
    public Vector3 MoveDir;
    public float MoveAngle;
    private float PrevPlayerAngle;

    //Move OtherPlayer
    private bool updateMoveByOtherPlayer;
	Vector3 oldDstPosWithoutY = Vector3.zero;
	Vector3 curDstPosWithoutY = Vector3.zero;
	float moveAmountUnitBySpeed = 0.0f;
	private EOtherPlayerMoveState otherPlayerMoveState = EOtherPlayerMoveState.STOP;

	#endregion CharacterController_Inner
    private BPWorldPositionYExtractor posYExtractor = null;
    private JumpController jumpController = null;
    private BTInteractionState btInteractionState = null;

    private Action funcOnCancelPropInteraction = null;
    private Action funcOnCancelNpcAnimalInteraction = null;


    public BPWorldNetPlayer_SpawnPosition SpawnInfo { get; private set; }
    public BPWorldAvatarAppearFXController AppearFXController => appearFXController;
    public CCharacterController_BPWorld_Jump JumpController => jumpController;
    public CCharacterController_BPWorld_BTInteractionState BTInteractionState => btInteractionState;
	public EOtherPlayerMoveState OtherPlayerMoveState => otherPlayerMoveState;


	protected override void Awake()
    {
    }


    public override void Initialize(CHARACTER_TYPE cType)
    {
        base.Initialize(cType);

        SetThisObject(gameObject);

        SetCharacterType(cType);

        appearFXController = new BPWorldAvatarAppearFXController();
        posYExtractor = new BPWorldPositionYExtractor();
        jumpController = new CCharacterController_BPWorld_Jump();
        btInteractionState = new CCharacterController_BPWorld_BTInteractionState();
        ConfigureData configData = Configure.GetConfigureDataArray(CONFIGURE_TYPE.USER_INFO)[3];
        warpDistance = Convert.ToSingle(configData.Value2);
		fixPosDistance = 6.0f;
		
		//default motion
		base.StateMachine.ChangeState(CharState_BPWorld_MetaverseSpawnWait.Instance());
    }


    public void Setup(long UID, BPWorldCharacterData charData, Transform mainViewCamera, Transform shadowTransform, float playerHeight, BPWorldNetPlayer_SpawnPosition spanwInfo, Action funcOnCancelInteraction = null, Action funcOnCancelNpcInteraction = null)
    {
        float radius = 0.3f;

		this.MainCam = mainViewCamera;
		this.SpawnInfo = spanwInfo;
        this.PrevPlayerAngle = SpawnInfo.SpawnRotation.eulerAngles.y;
        this.funcOnCancelPropInteraction = funcOnCancelInteraction;
        this.funcOnCancelNpcAnimalInteraction = funcOnCancelNpcInteraction;

        SetPlayerUID(UID);
        SetCharacterController(0.0001f, new Vector3(0, playerHeight * 0.5f, 0), radius, playerHeight);
        SetMetaverseCharacterData(charData);
        shadowManager.SetShadowObj(shadowTransform);

        if (appearFXController != null)
        {
            appearFXController.Initialize(transform.parent, transform, this);
        }

        if (jumpController != null)
        {
            jumpController.Initialize(MyAnimator, CurSceneID, charData.Jump_Velocity);
        }

        if (btInteractionState != null)
        {
            btInteractionState.Initialize();
        }

        ColliderManager.SetIgnoreLayerCollision("BPW_IGNORE_COL");

        posYExtractor?.Initialize();

        SetChangeMotionState(MOTION_STATE.SPAWN_METAVERSE);
    }

    public void SetSpawnInfo(BPWorldNetPlayer_SpawnPosition spanwInfo)
    {
        this.SpawnInfo = spanwInfo;
        SetChangeMotionState(MOTION_STATE.SPAWN_METAVERSE);
    }


    // Update is called once per frame
    protected override void Update()
    {
        if (CurSceneID == SceneID.BPWORLD_SCENE || CurSceneID == SceneID.MINIGAME_PIHAGI_SCENE)
        {
            bool isMyPlayer = base.myPlayerUID == GetPlayerUID();

            StateMachine?.StateMachine_Update();
            
            UpdateMoveByOtherPlayer(isMyPlayer);

#if UNITY_EDITOR
            if( Input.GetKeyUp(KeyCode.Space) && isMyPlayer == true && CurSceneID == SceneID.BPWORLD_SCENE )
            {
                jumpController.StartJump();
            }
#endif
        }
    }



    #region PROC_Metaverse
    public void SetCharacterController(float skinWidth, Vector3 center, float radius, float height)
    {
        if (moveController == null)
        {
            moveController = GetComponent<CharacterController>();
        }

        moveController.skinWidth = skinWidth;
        moveController.minMoveDistance = 0;
        moveController.center = center;
        moveController.radius = radius;
        moveController.height = height;
    }

    public void SetMetaverseCharacterData(BPWorldCharacterData charData)
    {
        BpwCharData = charData;
        SetMoveSpeed(charData.Run_Speed);
    }

    public void SetMoveSpeed(float speed)
    {
        MoveSpeed = speed;
        //CDebug.Log("MoveSpeed = " + speed);
    }

    public float SetSpeed(float amount)
    {
        float _moveAmountUnit = 0.0f;

        if (CurSceneID == SceneID.BPWORLD_SCENE)
        {
            if (amount == 0)
            {
                _moveAmountUnit = 0;
                SetMoveSpeed(BpwCharData.Run_Speed);
            }
            else if (amount > 0 && amount <= Constants.JOYSTICK_WALKLIMIT_VALUE)
            {
                _moveAmountUnit = Constants.JOYSTICK_WALKLIMIT_VALUE;//WalkSpeed
                if (MoveSpeed != BpwCharData.Walk_Speed)
                {
                    SetMoveSpeed(BpwCharData.Walk_Speed);
                }
            }
            else
            {
                _moveAmountUnit = Constants.JOYSTICK_RUNLIMIT_VALUE;
                if (MoveSpeed != BpwCharData.Run_Speed)
                {
                    SetMoveSpeed(BpwCharData.Run_Speed);
                }
            }
        }
        else if (CurSceneID == SceneID.MINIGAME_PIHAGI_SCENE)
        {
            if (amount == 0)
            {
                _moveAmountUnit = 0;
                SetMoveSpeed(BpwCharData.Run_Speed_Pihagi);
            }
            else
            {
                _moveAmountUnit = Constants.JOYSTICK_RUNLIMIT_VALUE;
                if (MoveSpeed != BpwCharData.Run_Speed_Pihagi)
                {
                    SetMoveSpeed(BpwCharData.Run_Speed_Pihagi);
                }
            }
        }

        return _moveAmountUnit;
    }

    /// <summary> MOTION_WALK state process
    /// <para> Vector of Joystick horizental/vertical value</para>
    /// <seealso cref="CJoyStickController.FixedUpdate()"/>
    /// </summary>
    /// <param name="move"></param>

    bool isOldMoveState = false;
    public void MoveProc(Vector3 move)
    {
        bool isMyPlayer = base.myPlayerUID == GetPlayerUID();

        if (isMyPlayer == false)
        {
            return;
        }

        JoyStickMagnitude = move.magnitude;

        ///방향 설정 - heading은 조이 스틱에서 넘어온 값(그 값이 곧 방향이다)
        SetMoveDirection(move);

        //SendMyPositionCheckTime( transform.position );
        
        //조이스틱을 조금이라도 움직였거나, 점프 버튼을 눌렀다면
        if (JoyStickMagnitude >= MINIMUM_MOVE_DIST || jumpController.IsNowJumping() == true)
        {
            if (CurSceneID == SceneID.BPWORLD_SCENE)
            {
                ForceCancelPropInteraction();
                ForceCancelNpcAnimalInteraction();

                //MyAnimator.ResetTrigger("move_force");
                //MyAnimator.SetTrigger("move_force");
            }
        
            ExecuteMove(move);
        }
    }



    private void ExecuteMove(Vector3 heading)
    {
        Transform myTransform = transform;
        float targetAngle = 0.0f;
        bool isGrounded = false;
        Vector3 moveDirByNormalized = default;
        Vector3 distance = default;
        Vector3 moveValue = default;
        float moveAmount = 0.0f;
        float moveAmountUnit = 0.0f;
        float speed = 0.0f;

        if( MainCam == null || MainCam.eulerAngles == null )
        {
            return;
        }

		if( myTransform == null || myTransform.eulerAngles == null )
		{
			return;
		}

        if( moveController == null )
        {
            return;
        }

		targetAngle = Mathf.Atan2( heading.x, heading.z ) * Mathf.Rad2Deg + MainCam.eulerAngles.y;
		isGrounded = moveController.isGrounded;

		if (jumpController.IsNowJumping() == false)
        {
            PrevPlayerAngle = targetAngle;
        }
        else
        {
            targetAngle = PrevPlayerAngle;
        }

        ///Rotation 갱신
        MoveAngle = Mathf.SmoothDampAngle( myTransform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
		myTransform.rotation = Quaternion.Euler(0f, MoveAngle, 0f);

        ///방향
        moveDirByNormalized = (Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward).normalized;
        moveAmount = GetMoveAmount(heading);
        moveAmountUnit = SetSpeed(moveAmount);

        ///거리
        distance = moveDirByNormalized * moveAmountUnit;
        distance.y = jumpController.UpdateJump(isGrounded, MoveSpeed, BpwCharData, true, new Vector3( myTransform.position.x, 0.0f, myTransform.position.z));

        ///최종 계산 : y값은 지속적으로 바닥으로 캐릭터를 내리기 위해 0값을 사용하지 않는다.
        moveValue.x = distance.x * MoveSpeed * Time.fixedDeltaTime;
        moveValue.y = distance.y * ((jumpController.IsNowJumping() == false) ? 1.0f : 5.0f) * Time.fixedDeltaTime;
        moveValue.z = distance.z * MoveSpeed * Time.fixedDeltaTime;

        ///이동 처리
        moveController.Move(moveValue);

        //CDebug.Log($"ExecuteMove ## [{System.DateTime.Now.Second}:{System.DateTime.Now.Millisecond}]: pos({transform.position}), MoveSpeed({MoveSpeed}), jumpController.RevisionJumpVal({jumpController.RevisionJumpVal}), moveAmount({moveAmount}), moveAmountUnit({moveAmountUnit})");
        //Debug.Log("CharController Move() moveValue = " + moveValue + "/distance = " + distance + "/MoveSpeed = " + MoveSpeed + "/" + jumpController.RevisionJumpVal);

        OnUpdateShadow();
        SetChangeMotionState(MOTION_STATE.WALK_CONTROL);
    }


    private void OnUpdateState()
    {
        if (StateMachine.GetCurrentState() == CharState_BPWorld_WalkControl.Instance())
        {
            return;
        }


    }


    private void OnUpdateShadow()
    {
        if( jumpController == null || shadowManager == null )
        {
            return;
        }

        float jumpVelocity = jumpController.JumpVelocity;
        float directionY = jumpController.DirectionY;
        Vector3 shadowPosition = transform.position;

        shadowManager.SetCharShadowScale(jumpVelocity, directionY);
        shadowManager.SetCharShadowPos(shadowPosition);
    }



    public float GetMoveAmount(Vector3 move)
    {
        float xPow = move.x * move.x;
        float yPow = move.z * move.z;

        //CDebug.Log($"move = {move}, xPow = {xPow}, yPow = {yPow} //// _Sqrt = {_Sqrt(xPow + yPow)}");

        return _Sqrt(xPow + yPow);
    }

    public void SetMoveDirection(Vector3 dir)
    {
        //CDebug.Log($"dir = {dir}");
        MoveDir = dir;
    }

    public Vector3 GetMoveDirection()
    {
        return MoveDir;
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }

    public override void SetChangeMotionState(MOTION_STATE motion, BaseStateMotionInfo stateMotionInfo = null, bool isBT = false)
    {
        if (StateMachine == null)
        {
            CDebug.Log( "StateMachine is null. SetChangeMotionState me????? " + transform);
            return;
        }

#if UNITY_EDITOR
        if (CDirector.Instance != null)
#endif
        {
            if (StateMachine.GetCurrentState() == null)
            {
                CDebug.Log( "StateMachine.StateMachine.GetCurrentState() is null. " );
                return;
            }
        }
        
        //CDebug.Log($"motion : {motion}, CurState : {StateMachine.CurState}, GetCurrentState() : {StateMachine.GetCurrentState()}");
        switch (motion)
        {
            case MOTION_STATE.SPAWN_METAVERSE:
                if (StateMachine.GetCurrentState() != CharState_BPWorld_MetaverseSpawn.Instance())
                {
                    SetChangeStateMachine(CharState_BPWorld_MetaverseSpawn.Instance());
                }
                break;
            case MOTION_STATE.WALK_CONTROL:
                if (StateMachine.GetCurrentState() != CharState_BPWorld_WalkControl.Instance())
                {
                    SetChangeStateMachine(CharState_BPWorld_WalkControl.Instance());
                }
                break;
            case MOTION_STATE.INTERACTION_WITH_OBJ:
                if (StateMachine.GetCurrentState() != CharState_BPWorld_InteractionWithObj.Instance())
                {
                    StateMachine.ChangeState(CharState_BPWorld_InteractionWithObj.Instance(stateMotionInfo));
                }
                break;
            case MOTION_STATE.WALK_AUTO:
                if (StateMachine.GetCurrentState() != CharState_BPWorld_WalkAuto.Instance())
                {
                    SetChangeStateMachine(CharState_BPWorld_WalkAuto.Instance());
                }
                break;

            case MOTION_STATE.WALK_CONTROL_OTHER_PLAYER:
                if (StateMachine.GetCurrentState() != CharState_BPWorld_WalkControl_OtherPlayer.Instance())
                {
                    SetChangeStateMachine(CharState_BPWorld_WalkControl_OtherPlayer.Instance());
                }
                break;

            case MOTION_STATE.PLAYANIM:
                SetChangeStateMachine(CharacterState_PlayAnim.Instance());
                break;

            case MOTION_STATE.PLAYANIM_LOOP:
                SetChangeStateMachine(CharacterState_PlayAnimLoop.Instance());
                break;
        }
    }


    public void SetWarpByOtherPlayer(Vector3 recvPosition)
    {
        ForceCancelNpcAnimalInteraction();

        transform.position = recvPosition;
    }
    
    #endregion PROC_Metaverse

    #region Interaction
    public void PlayInteraction(Transform targetInteractionObj, string interactionAnimParamName, bool isMyPlayer)
    {
        Vector3 interactionObjectPosition = targetInteractionObj.position;
        Quaternion interactionObjectRotation = targetInteractionObj.rotation;

        transform.localPosition = interactionObjectPosition;
        transform.eulerAngles = interactionObjectRotation.eulerAngles;

        //MyAnimator.ResetTrigger( interactionAnimParamName );
        MyAnimator.SetTrigger(interactionAnimParamName);

        if (isMyPlayer == true)
        {
            PrevPlayerAngle = transform.rotation.eulerAngles.y;

            shadowManager.SetCharShadowPos(interactionObjectPosition);
            //SendMyPosition(transform.position);
        }
    }


    public void SetInteractionBT(ExternalBehavior bt)
    {
        base.SetExternalBehavior(bt);
    }

    public void InitBT()
    {
        base.InitBT();
    }

    private void ForceCancelPropInteraction()
    {
        funcOnCancelPropInteraction?.Invoke();
    }


    private void ForceCancelNpcAnimalInteraction()
    {
        if (btInteractionState == null)
        {
            return;
        }

        if (btInteractionState.NowPlayState != EBPWorldBTInteractionState.NOW_PLAY)
        {
            return;
        }

        //강제 취소 처리를 한다
        funcOnCancelNpcAnimalInteraction?.Invoke();

    }

    #endregion Field_Obj_Interaction

    public void SetChangeStateMachine(CState<CCharacterController> state)
    {
        StateMachine.ChangeState(state);
    }

    public static float _Sqrt(float z)
    {
        if (z == 0) return 0;
        FloatIntUnion u;
        u.tmp = 0;
        float xhalf = 0.5f * z;
        u.f = z;
        u.tmp = 0x5f375a86 - (u.tmp >> 1);
        u.f = u.f * (1.5f - xhalf * u.f * u.f);
        return u.f * z;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        updateMoveByOtherPlayer = false;

        if (appearFXController != null)
        {
            appearFXController.Destroy();
            appearFXController = null;
        }

        if (jumpController != null)
        {
            jumpController.Destroy();
            jumpController = null;
        }

        if (btInteractionState != null)
        {
            btInteractionState.Destroy();
            btInteractionState = null;
        }

        if (posYExtractor != null)
        {
            posYExtractor.Destroy();
            posYExtractor = null;
        }
    }


	public void SetMoveByOtherPlayer( Vector3 recvPosition )
	{
		if( CurSceneID != SceneID.BPWORLD_SCENE )
			return;

		oldDstPosWithoutY = curDstPosWithoutY;
		curDstPosWithoutY = new Vector3( recvPosition.x, 0.0f, recvPosition.z );
		Vector3 curCharPosWithoutY = new Vector3( transform.position.x, 0.0f, transform.position.z );

		float magnitude = (curDstPosWithoutY - curCharPosWithoutY).magnitude;

		///점프 체크
		if( jumpController.CanStartJump( curCharPosWithoutY ) )
        {
			jumpController.StartJump();
        }

		if( magnitude < DISTANCE_BT_CHECK && jumpController.IsNowJumping() == false )
		{
			///현재 NPC 인터랙션 중이라면 Move를 시키면 안된다
			///(ex. 동물과 하는 인터랙션 )
			if( BTInteractionState.NowPlayState == EBPWorldBTInteractionState.NOW_PLAY )
			{
				updateMoveByOtherPlayer = false;
				return;
			}
		}

		if( magnitude < DISTANCE_NOT_MOVE && jumpController.IsNowJumping() == false )
		{
			transform.position = recvPosition;

			OnUpdateShadow();
			updateMoveByOtherPlayer = false;
			otherPlayerMoveState = EOtherPlayerMoveState.STOP;
			return;
		}

		if( magnitude >= fixPosDistance )
		{
			ForceCancelNpcAnimalInteraction();
			updateMoveByOtherPlayer = false;
			transform.position = posYExtractor.ExtractPositionWithYCoord( recvPosition );
			otherPlayerMoveState = EOtherPlayerMoveState.STOP;
			OnUpdateShadow();
			return;
		}

		ForceCancelNpcAnimalInteraction();
		moveAmountUnitBySpeed = SetSpeedByDistance( (curDstPosWithoutY - oldDstPosWithoutY).magnitude );
		updateMoveByOtherPlayer = true;
	}


    /// <summary>
    /// 인터랙션 하기 전에 반드시 Move 움직임 처리를 중단해야 한다.
    /// </summary>
    public void SetForceStopUpdateMoveByOtherPlayer()
    {
        updateMoveByOtherPlayer = false;
    }

	private void UpdateMoveByOtherPlayer(bool isMyPlayer)
    {
        if (updateMoveByOtherPlayer == false)
            return;

        ExecuteMoveByOtherPlayer();
    }

    private void ExecuteMoveByOtherPlayer()
    {
        Vector3 curCharPosWithoutY = new Vector3(transform.position.x, 0.0f, transform.position.z);

        ///점프 체크
        if (jumpController.CanStartJump(curCharPosWithoutY))
            jumpController.StartJump();

        Vector3 dir = curDstPosWithoutY - curCharPosWithoutY;
        if (dir.magnitude < DISTANCE_NOT_MOVE && jumpController.IsNowJumping() == false)
        {
            updateMoveByOtherPlayer = false;
            otherPlayerMoveState = EOtherPlayerMoveState.STOP;
            return;
        }
            
        Vector3 heading = dir.normalized;

        float targetAngle = Mathf.Atan2(heading.x, heading.z) * Mathf.Rad2Deg;
        //CDebug.Log($"ExecuteMoveByOtherPlayer : ## targetAngle({targetAngle})");
        
        bool isGrounded = moveController.isGrounded;
        Vector3 moveDirByNormalized = default;
        Vector3 distance = default;
        Vector3 moveValue = default;
        
        if (jumpController.IsNowJumping() == false)
            PrevPlayerAngle = targetAngle;
        else
            targetAngle = PrevPlayerAngle;

        ///Rotation 갱신
        MoveAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
        transform.rotation = Quaternion.Euler(0f, MoveAngle, 0f);

        ///방향
        moveDirByNormalized = (Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward).normalized;

        ///거리
        distance = moveDirByNormalized * moveAmountUnitBySpeed;
        distance.y = jumpController.UpdateJump(isGrounded, MoveSpeed, BpwCharData, false, curCharPosWithoutY, false);
        
        ///최종 계산 : y값은 지속적으로 바닥으로 캐릭터를 내리기 위해 0값을 사용하지 않는다.
        moveValue.x = distance.x * MoveSpeed * Time.deltaTime;
        moveValue.y = distance.y * ((jumpController.IsNowJumping() == false) ? 1.0f : 5.0f) * Time.deltaTime;
        moveValue.z = distance.z * MoveSpeed * Time.deltaTime;

        ///이동 처리
        moveController.Move(moveValue);

        ///도착 좌표보다 더 멀리있으면 바로 도착좌표로 보정한다.
        Vector3 moveCharPosWithoutY = new Vector3(transform.position.x, 0.0f, transform.position.z);
        if ((curDstPosWithoutY - curCharPosWithoutY).magnitude < (moveCharPosWithoutY - curCharPosWithoutY).magnitude)
        {
            transform.position = new Vector3(curDstPosWithoutY.x, transform.position.y, curDstPosWithoutY.z);
            
            if (jumpController.IsNowJumping() == false)
            {
                SetMoveSpeed(0.0f);
                OnUpdateShadow();
                updateMoveByOtherPlayer = false;
                otherPlayerMoveState = EOtherPlayerMoveState.STOP;
                return;
            }

        }


        OnUpdateShadow();
        SetChangeMotionState(MOTION_STATE.WALK_CONTROL_OTHER_PLAYER);
    }

    private float SetSpeedByDistance(float distance)
    {
        if (distance == 0)
        {
            SetMoveSpeed(BpwCharData.Run_Speed);
            otherPlayerMoveState = EOtherPlayerMoveState.STOP;
            return 0;
        }

        if (distance <= DISTANCE_MIN_WALK)
        {
            if (MoveSpeed != BpwCharData.Walk_Speed)
                SetMoveSpeed(BpwCharData.Walk_Speed);

            otherPlayerMoveState = EOtherPlayerMoveState.WALK;
            return Constants.JOYSTICK_WALKLIMIT_VALUE;//WalkSpeed
        }
        
        if (distance <= DISTANCE_MIN_RUN)
        {
            float newSpeed = (distance * BpwCharData.Run_Speed / DISTANCE_MIN_RUN);
            if (MoveSpeed != newSpeed)
                SetMoveSpeed(newSpeed);
            otherPlayerMoveState = EOtherPlayerMoveState.RUN;
            //CDebug.Log($"SetSpeedByDistance : sec({System.DateTime.Now.Second}), ms({System.DateTime.Now.Millisecond}), distance({distance}), newSpeed({newSpeed})");
            return Constants.JOYSTICK_RUNLIMIT_VALUE;
        }

        if (MoveSpeed != BpwCharData.Run_Speed)
            SetMoveSpeed(BpwCharData.Run_Speed);
        otherPlayerMoveState = EOtherPlayerMoveState.RUN;
        return Constants.JOYSTICK_RUNLIMIT_VALUE;
    }
}