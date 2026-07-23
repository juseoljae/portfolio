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
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using BehaviorDesigner.Runtime;
using DG.Tweening;
using System.Linq;
using UniRx;

#if UNITY_EDITOR
using UnityEngine.SceneManagement;
using System;
#endif

public class CCharacterController : MonoBehaviour
{
    [HideInInspector]
    public Animator MyAnimator;
    private string SwitchRunAnimContrlllerKey;
    private long SwitchRunAnimContrlllerGroupID;
    private GameObject RunAnimContrlllerReleaseObj;
    private CCharacterAnimationInfo MyAnimationsInfo;
    private NavMeshAgent agent;
    private float AgentSpeed;
    private BehaviorTree B_Tree;
    public CStateMachine<CCharacterController> StateMachine;

    private List<CBTHoldObjectInfo> BT_HoldObjInfoList;

    [HideInInspector]
    public bool IsSpawned; //for BT
    private AVATAR_TYPE AvatarType;
    private CHARACTER_TYPE CharType;
    public SceneID CurSceneID;

    protected CharacterShadowManager shadowManager = null;
    protected CCharacterController_ColliderManager colliderManager = null;

	private GameObject thisObj;

    private long PlayerUID;
    //NPC
    private long NpcUID;
    private int AnimPlayingFrame;
    private float animStartTime;
    private bool IsFinishAnimPlay;

    #region BT_VAR
    private Vector3 TargetPosition;
    private Transform TargetObject;
    [HideInInspector]
    public string AnimationParam;
    private string AnimationClipName;
#if UNITY_EDITOR
    [HideInInspector]
    public CBTHoldObjectInfo HoldObjInfo;
#endif
    public long HoldObjDataID;
    private Dictionary<string, float> BTAnimLoopTimeDic;

    //jump
    public class JumpInfo
    {
        public Vector3 TargetPos;
        public Vector3 TargetRot;
        public float Height;
        //public float Height_Bottom;
        public string[] AnimParams;
        public float[] AnimClipLengthes;//0:prev 1:jump 2:final
    }
    private JumpInfo BTJumpInfo;

    private bool bNmAgentNeedNot;
	#endregion BT_VAR

	protected long myPlayerUID = 0;

    private long PlayShopMotionID = 0;

    private Dictionary<string, GameObject> OrgResHoldObjectDic = new Dictionary<string, GameObject>();
    public CharAnimationEvent AnimEvent { get; private set; }

    public CharacterShadowManager ShadowManager => shadowManager;
    public CCharacterController_ColliderManager ColliderManager => colliderManager;


	protected virtual void Awake()
	{

	}


    public virtual void Initialize(CHARACTER_TYPE cType)
    {
        CharType = cType;

        shadowManager = new CharacterShadowManager();
		colliderManager = new CCharacterController_ColliderManager();
		StateMachine = new CStateMachine<CCharacterController>( this );

#if UNITY_EDITOR
        if (SceneManager.GetActiveScene().name.Equals("NPC_TEST") || SceneManager.GetActiveScene().name.Equals("BPWorld_Test")) { }
        else
#endif
		{
			myPlayerUID = long.Parse(CPlayer.GetStatusLoginValue(PLAYER_STATUS.USER_ID));
        }

        MyAnimator = gameObject.GetComponent<Animator>();
        MyAnimationsInfo = new CCharacterAnimationInfo();
        MyAnimationsInfo.Initialize(MyAnimator);

        InitAnimPlayingFrame();
        IsSpawned = false;

#if UNITY_EDITOR
        if (CDirector.Instance != null)
#endif
		{
            CurSceneID = (SceneID)CDirector.Instance.GetCurrentSceneID();
        }

        B_Tree = gameObject.GetComponent<BehaviorTree>();

        BTAnimLoopTimeDic = new Dictionary<string, float>();

        AnimEvent = gameObject.GetComponent<CharAnimationEvent>();
        if (AnimEvent != null)
        {
            BT_HoldObjInfoList = new List<CBTHoldObjectInfo>();
            SetHoldableObject();
            AnimEvent.InitAnimEvent(this);
        }

#if UNITY_EDITOR
        HoldObjInfo = new CBTHoldObjectInfo();
#endif

        shadowManager.Initialize();
		colliderManager.Initialize( transform );
	}


	public void InitComponents(CHARACTER_TYPE charType)
    {
        if (CharType == CHARACTER_TYPE.NONE)
        {
            CharType = charType;
        }

        if (MyAnimator == null)
        {
            MyAnimator = gameObject.GetComponent<Animator>();
        }

        if (MyAnimationsInfo == null)
        {
            MyAnimationsInfo = new CCharacterAnimationInfo();
            MyAnimationsInfo.Initialize( MyAnimator );
        }

        if(StateMachine == null)
        {

            StateMachine = new CStateMachine<CCharacterController>( this );
        }

        if (BTAnimLoopTimeDic == null)
        {
            BTAnimLoopTimeDic = new Dictionary<string, float>();
        }
    }


    public void SetPosition( Vector3 targetPosition )
    {
        transform.position = targetPosition;
    }

	public void SetActiveShadowObj(bool bActive, GameObject charObj)
	{
        shadowManager.SetCharShadowPos(charObj.transform.localPosition);

        shadowManager.SetActive_ShadowObject( bActive );
    }


    public void SetAvatarType(AVATAR_TYPE avatarType)
    {
        AvatarType = avatarType;
    }

    public AVATAR_TYPE GetAvatarType()
    {
        return AvatarType;
    }

    public void SetRuntimeAnimatorController(string key, long groupID, ANIMCONTROLLER_USE_TYPE useType, Animator animator = null, Action loadComplete = null)
    {
        Animator anim = animator;
        if(anim == null)
        {
            anim = MyAnimator;
        }

        if(anim == null)
        {
            CDebug.LogError( "SetRuntimeAnimatorController() MyAnimator is null " );
            return;
        }


        SwitchRunAnimContrlllerKey = key;
        SwitchRunAnimContrlllerGroupID = groupID;

        string name = string.Format( "RunAnimContrlllerReleaseObj_CharController_{0}", key );
        RunAnimContrlllerReleaseObj = new GameObject( name );
        RunAnimContrlllerReleaseObj.transform.SetParent( gameObject.transform.parent );

        RuntimeAnimatorController _runController = CRuntimeAnimControllerSwitchManager.Instance.SetRuntimeAnimatorController(key, groupID, useType, RunAnimContrlllerReleaseObj );

        if(_runController != null)
        {
            CDebug.Log( $" [avatar controller] SetRuntimeAnimatorController _runController load complete. key:{key}, groupID:{groupID}, type:{useType}" );
            anim.runtimeAnimatorController = _runController;
            MyAnimationsInfo.Initialize( anim );
            loadComplete?.Invoke();
        }
        else
        {
            CDebug.LogError( $"SetRuntimeAnimatorController() key:{key}, groupID:{groupID} / RuntimeAnimatorController is null " );
        }
    }

    public void ReleaseRuntimeAnimatorController(bool bReleaseAll = false, Animator anim = null)
    {
        Animator animator = anim;
        if(animator == null)
        {
            animator = MyAnimator;
        }
        if (animator == null)
        {
            CDebug.LogError( "ReleaseRuntimeAnimatorController() MyAnimator is null " );
            return;
        }
        CRuntimeAnimControllerSwitchManager.Instance.ReleaseRuntimeAnimController( SwitchRunAnimContrlllerKey, SwitchRunAnimContrlllerGroupID, bReleaseAll );
        animator.runtimeAnimatorController = null;
    }

    public void SetAnimationTrigger(string param)
    {
        //CDebug.Log("SetAnimationTrigger() param = " + param + " / " + GetAvatarType());
        MyAnimator.SetTrigger(param);
    }

    public Animator GetAnimatorController()
    {
        return MyAnimator;
    }

    #region NavMesh
    public void SetNmAgentNeedNot(bool bNeed)
    {
        bNmAgentNeedNot = bNeed;
    }

    public void SetActiveNavMeshAgent(bool bActive)
    {
        if (agent == null ) return;
        agent.enabled = bActive;

        if(bNmAgentNeedNot || gameObject.activeSelf == false)
        {
            agent.enabled = false;
            SetActiveBehaviorTree( false );
        }
    }

    public void SetStopNavMeshAgent(bool bStop)
    {
        if (agent == null) return;

        if (agent.enabled)
        {
            agent.isStopped = bStop;
        }
    }

    public void SetNavMeshDestination(Vector3 pos)
    {
        if (agent == null || agent.enabled == false)
        {
            //Debug.LogError("         ### SetDestination agent is null or agent is not enable");
            return;
        }
        SetActiveNavMeshAgent(true);
        SetStopNavMeshAgent(false);
        agent.destination = pos;
    }

    public float GetNavMeshRemainDistance()
    {
        if (agent == null)
        {
            //Debug.Log("         ### SetDestination agent is null or agent is not enable");
            return -1;
        }

        return agent.remainingDistance;
    }

    public Vector3 GetNavMeshDestination()
    {
        if (agent == null)
        {
            CDebug.LogError("GetNavMeshDestination() Destination don't set NavMesh !!! ");
            return Vector3.zero;
        }

        return agent.destination;
    }

    public void SetNavMesh()
    {
        if (bNmAgentNeedNot) return;

        if(agent == null)
        {
            agent = gameObject.AddComponent<NavMeshAgent>();
            bNmAgentNeedNot = false;
        }

        float speed = 0.6f;

        //Speed, AngularSpeed, Acceleration, StoppingDistance, AutoBraking,......
        if (CharType == CHARACTER_TYPE.NPC)
        {
            speed = AgentSpeed;
            if (speed == 0)
            {
                speed = 1;
                //CDebug.Log("SetNavMesh() set speed wrong!!!! npcUid = " + NpcUID + "/ npcObj = " + transform);
            }
        }

        agent.speed = speed;// 0.6f;
        agent.angularSpeed = 320;
        agent.radius = 0.1f;
        agent.height = 1.5f;

    }

    //private float PrevAgentSpeed;
    public void SetNavMeshSpeed(float speed)
    {
        AgentSpeed = speed;
    }

    public void SetNavMeshSpeedForBT(float speed)
    {
        agent.speed = speed;
        //PrevAgentSpeed = speed;
    }

    public void RestoreNavMeshSpeedForBT()
    {
#if UNITY_EDITOR
        if(AgentSpeed == 0)
        {
            AgentSpeed = 0.6f;
        }
#endif
        agent.speed = AgentSpeed;
    }
    #endregion NavMesh

    public void SetThisObject(GameObject obj)
    {
        thisObj = obj;
    }
    
    public void SetCharacterType(CHARACTER_TYPE cType)
    {
        CharType = cType;
    }

    public CHARACTER_TYPE GetCharacterType()
    {
        return CharType;
    }

    //// ----  npc -----
    public void SetNpcUID(long npcUID)
    {
        NpcUID = npcUID;
    }

    public long GetNpcUID()
    {
        return NpcUID;
    }

    public Vector3 RoundVector3(Vector3 vec)
    {
        Vector3 _roundVec = Vector3.zero;

        _roundVec.x = Mathf.Round(vec.x);
        _roundVec.y = Mathf.Round(vec.y);
        _roundVec.z = Mathf.Round(vec.z);

        return _roundVec;
    }

    public void KillDoFunc()
    {
        transform.DOKill();
    }


    #region Char_StateMachine
    public void SetAnimationParam(string param, string clipName = "")
    {
        AnimationParam = param;
        AnimationClipName = clipName;
    }

    public string GetAnimationParam()
    {
        return AnimationParam;
    }

    public float AnimSpeed;

    public void SetAnimationSpeed(float speed)
    {
        if (MyAnimator == null)
            return;

        float animSpeed = speed;
        if(animSpeed <= 0) 
        {
            animSpeed = 1;
        }
        AnimSpeed = animSpeed;
        MyAnimator.speed = animSpeed;
    }

    //clipName is for bt
    //it's for checking this animation finish
    public void SetChangeMotionStateWithParameter(MOTION_STATE motion, string param, string clipName = "", bool isBT = false)
    {
        //AnimationParam = param;
        SetAnimationParam(param, clipName);
        SetChangeMotionState(motion, null, isBT);
    }

    public virtual void SetChangeMotionState(MOTION_STATE motion, BaseStateMotionInfo stateMotionInfo = null, bool isBT = false)
    {
        if (StateMachine == null)
        {
            //Debug.Log("SetChangeMotionState me????? " + transform);
            return;
        }

#if UNITY_EDITOR
        if (CDirector.Instance != null)
#endif
        {
            if (StateMachine.CurState == null && !isBT)
            {
                return;
            }
        }


        switch (motion)
        {
            case MOTION_STATE.WALK_AUTO:
                if (StateMachine.CurState != CharacterState_WalkAuto.Instance())
                {
                    StateMachine.ChangeState(CharacterState_WalkAuto.Instance());
                }
                break;
            case MOTION_STATE.WALK_ANIM:
                if (StateMachine.CurState != CharacterState_WalkWithAnim.Instance())
                {
                    StateMachine.ChangeState(CharacterState_WalkWithAnim.Instance());
                }
                break;
            case MOTION_STATE.PLAYANIM://
                //if (StateMachine.CurState != CharacterState_PlayAnim.Instance())
                {
                    StateMachine.ChangeState(CharacterState_PlayAnim.Instance());
                }
                break;
            case MOTION_STATE.PLAYANIM_LOOP:
                //if (StateMachine.CurState != CharacterState_PlayAnimLoop.Instance())
                {
                    StateMachine.ChangeState(CharacterState_PlayAnimLoop.Instance());
                }
                break;
        }
    }
    #endregion Char_StateMachine





    #region BT_FUNC
    //BT, Metaverse
    public void SetLocation(Vector3 pos, Vector3 rot)
    {

        SetActiveNavMeshAgent(false);
        transform.localPosition = pos;
        if (CurSceneID == SceneID.BPWORLD_SCENE)
        {
            shadowManager.SetCharShadowPos(transform.localPosition);
        }

        transform.localRotation = Quaternion.Euler(rot.x, rot.y, rot.z);
        IsSpawned = true;

        if (!bNmAgentNeedNot)
        {
            SetActiveNavMeshAgent( true );
        }
    }

    //BT
    public void SetRotation(Vector3 rot, float duration)
    {
        //transform.rotation = Quaternion.Euler(rot);
        transform.DORotate( rot, duration );
    }

    public void SetBTAnimationLoopTime(string animName, float time)
    {
        if (BTAnimLoopTimeDic.ContainsKey(animName) == false)
        {
            BTAnimLoopTimeDic.Add(animName, time);
        }
        else
        {
            BTAnimLoopTimeDic[animName] = time;
        }

    }

    public bool IsAnimationLoopFinish(string animName, float time)
    {
        if (BTAnimLoopTimeDic.ContainsKey(animName) == false)
        {
            return false;
        }

        if (time > BTAnimLoopTimeDic[animName])
        {
            return true;
        }

        return false;
    }

#if UNITY_EDITOR
    public void SetBTHoldObjectInfo(CBTHoldObjectInfo info)
    {
        if (info == null || HoldObjInfo == null)
        {
            CDebug.Log("CBTHoldObjectInfo is NULL");
            return;
        }

        HoldObjInfo.Hand_Direction = info.Hand_Direction;
        HoldObjInfo.ObjectID = info.ObjectID;
        HoldObjInfo.EffectID = info.EffectID;
        HoldObjInfo.Effect_LoopValue = info.Effect_LoopValue;
    }

    public CBTHoldObjectInfo GetBTHoldObjectInfo()
    {
        if (HoldObjInfo != null)
        {
            return HoldObjInfo;
        }

        return null;
    }
#endif
    public void DestroyBT()
    {
		if( B_Tree == null )
			return;

        GameObject.Destroy( B_Tree );
        B_Tree = null;
	}
    public void SetActiveBehaviorTree(bool bActive)
    {
        if (B_Tree == null) return;

        B_Tree.enabled = bActive;

        if(bNmAgentNeedNot)
        {
            B_Tree.enabled = false;
        }
    }

    public void SetDisableBehaviorTree()
    {
        B_Tree.DisableBehavior();
    }

    public void SetEnableBehaviorTree()
    {
        if (B_Tree == null) return;

        B_Tree.EnableBehavior();
    }

    public bool GetBehaviorTreeEnabled()
    {
        return B_Tree.enabled;
    }

    public void InitBT()
    {
        if(B_Tree == null)
            B_Tree = gameObject.AddComponent<BehaviorTree>(); 
    }

    public void SetExternalBehavior(ExternalBehavior bt)
    {
        B_Tree.ExternalBehavior= bt;
    }


    public void SetMoveTargetObject(Transform obj)
    {
        TargetObject = obj;
    }

    public Transform GetMoveTargetObject()
    {
        return TargetObject;
    }

    public void SetMoveTargetPosition(Vector3 pos)
    {
        Vector3 targetPos = pos;

        //if (StateMachine.CurState == null)
        //{
        //    StateMachine.ChangeState(CharacterState_MetaverseSpawnWait.Instance());
        //}

        TargetPosition = targetPos;
        SetNavMeshDestination(targetPos);
    }

    public Vector3 GetMoveTargetPosition()
    {
        return TargetPosition;
    }


    public void SetHoldObjDataID(long ID)
    {
        if (ID != 0)
        {
            HoldObjDataID = ID;
            if (GetCharacterType() == CHARACTER_TYPE.NPC)
            {
                SetHoldableObject(ID);
            }
        }
    }

    public bool IsCharReachedToMoveTarget(Vector3 target)
    {
        return IsCharReachedToNavMeshDestnation(GetMoveTargetPosition());
    }

    private bool IsCharReachedToNavMeshDestnation(Vector3 targetPos)
    {
        Vector3 charPos = new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 _targetPos = new Vector3(targetPos.x, 0, targetPos.z);
        float _dist = Vector3.Distance(charPos, _targetPos);

        if (_dist < 0.1f)
        {
            SetStopNavMeshAgent(true);
            SetActiveNavMeshAgent(false);
            if(AnimEvent != null) AnimEvent.InitAnimEvent();
            return true;
        }

        return false;
    }


    public bool IsArrivedNavMeshAgentDestination()
    {
        float _dist = agent.remainingDistance;

        //if (_dist != Mathf.Infinity && agent.pathStatus == NavMeshPathStatus.PathComplete && agent.remainingDistance == 0)
        if (_dist != Mathf.Infinity && agent.remainingDistance == 0 && agent.stoppingDistance == 0)
        {
            return true;
        }

        return false;
    }

    public void InitAnimPlayingFrame()
    {
        AnimPlayingFrame = 0;
    }

    public void SetIsFinishAnimPlay(bool bFinish)
    {
        IsFinishAnimPlay = bFinish;
    }

    public bool GetIsFinishAnimPlay()
    {
        return IsFinishAnimPlay;
    }

    public void SetAnimStartTime(float time)
    {
        animStartTime = time;
    }

    public bool IsCurrentAnimClipFinished()
    {
        AnimatorClipInfo[] curClipInfo = MyAnimator.GetCurrentAnimatorClipInfo(0);

#if UNITY_EDITOR
        if (CDirector.Instance == null)
        {
            Application.targetFrameRate = 30;
        }
#endif

        if (curClipInfo.Length == 0) return false;

        var exitFrame = (int)(Application.targetFrameRate * curClipInfo[0].clip.length);
        bool retvalue = (AnimPlayingFrame++ >= exitFrame);
        return retvalue;
    }


    public float GetAnimationLength(string clipName)
    {

        return MyAnimationsInfo.GetAnimationLength(clipName);
    }

    public void SetBTJump(Vector3 targetPos, Vector3 targetRot, float height, string[] param, string[] clips)
    {
        BTJumpInfo = new JumpInfo();
        BTJumpInfo.TargetPos = targetPos;
        BTJumpInfo.TargetRot = targetRot;
        BTJumpInfo.Height = height;
        BTJumpInfo.AnimParams = new string[param.Length];
        BTJumpInfo.AnimClipLengthes = new float[clips.Length];
        for(int i=0; i<clips.Length; ++i)
        {
            BTJumpInfo.AnimParams[i] = param[i];
            BTJumpInfo.AnimClipLengthes[i] = GetAnimationLength( clips[i] );
        }
    }

    public JumpInfo GetCurBTJumpInfo()
    {
        if(BTJumpInfo != null)
        {
            return BTJumpInfo;
        }

        return null;
    }

    public void SetChangeState(CState<CCharacterController> state)
    {
        StateMachine.ChangeState(state);
    }

    public CState<CCharacterController> GetCurState()
    {
        return StateMachine.GetCurrentState();
    }

    #endregion BT_FUNC




    public void SetHoldableObjByAvatarType(AVATAR_TYPE aType)
    {
        SetAvatarType( aType );
        SetHoldableObject();
    }

    public void SetHoldableObject()
    {
		AVATAR_TYPE avatarType = GetAvatarType();

        if( avatarType == AVATAR_TYPE.AVATAR_NONE )
        {
            return;
        }

		List<MotionData> _list = CAvatarInfoDataManager.GetMotionDataByAvatarType( avatarType );
        if (_list != null)
        {
            List<MotionData> _commonList = CAvatarInfoDataManager.GetMotionDataByAvatarType(AVATAR_TYPE.AVATAR_BLACKPINK);

            var totalList = _list.Concat(_commonList).Distinct().ToList();


            if (totalList != null)
            {
                for (int i = 0; i < totalList.Count; ++i)
                {
                    MotionData _data = totalList[i];
                    for (int j = 0; j < _data.Object_ID.Length; ++j)
                    {
                        SetHoldableObject(_data.Object_ID[j], _data.AnimName);
                    }
                }
            }
        }
    }


    public void SetHoldableObject(long objectID, string animName = "")
    {
        if (AnimEvent == null)
        {
            return;
        }
            
        if (objectID != 0)
        {
            HoldObjectData _holdObjData = CPlayerDataManager.Instance.GetHoldObjectDataByID(objectID);

            if (_holdObjData != null)
            {
                BT_HoldObjInfoList.Add(new CBTHoldObjectInfo());

                int _idx = BT_HoldObjInfoList.Count - 1;

                Transform bone1st = null;
                Transform bone2nd = null;

                List<GameObject> boneList = AnimEvent.GetDummyObject( _holdObjData.HandDirection );

                if(boneList != null)
                {
                    if(boneList.Count == 1)
                    {
                        bone1st = boneList[0].transform;
                        bone2nd = null;
                    }
                    else if(boneList.Count == 2)
                    {
                        bone1st = boneList[0].transform;
                        bone2nd = boneList[1].transform;
                    }
                }

                BT_HoldObjInfoList[_idx].SetData
                (
                    this,
                    _holdObjData, gameObject,
                    bone1st, bone2nd,
                    animName
                );
            }
        }
    }

    public void SetHoldableObjectLayer(string layerName)
    {
        for(int i=0; i< BT_HoldObjInfoList.Count; ++i)
        {
            BT_HoldObjInfoList[i].SetObjectLayer(layerName);
        }
    }


    public CBTHoldObjectInfo GetHoldObjectByObjDataID(long ID)
    {
        if (ID == 0) return null;
        if (BT_HoldObjInfoList == null)
        {
            CDebug.Log($"GetHoldObjectByObjDataID : BT_HoldObjInfoList is null  ObjectID - {ID}");
            return null;
        }

        for (int i = 0; i < BT_HoldObjInfoList.Count; ++i)
        {
            if (BT_HoldObjInfoList[i] == null)
            {
                CDebug.Log($"GetHoldObjectByObjDataID : BT_HoldObjInfoList info is null,  [ list index - {i}], [ count - {BT_HoldObjInfoList.Count} ], [ ObjectID - {ID} ]");
                continue;
            }

            if (ID == BT_HoldObjInfoList[i].ObjectDataID)
            {
                return BT_HoldObjInfoList[i];
            }
        }

        return null;
    }

    public CBTHoldObjectInfo GetHoldObjectInfoByAnimName(string animName)
    {
        for (int i = 0; i < BT_HoldObjInfoList.Count; ++i)
        {
            if (animName.Equals(BT_HoldObjInfoList[i].animationName))
            {
                return BT_HoldObjInfoList[i];
            }
        }

        return null;
    }

    public void HideAllObject()
    {
        if (BT_HoldObjInfoList != null)
        {
            for (int i = 0; i < BT_HoldObjInfoList.Count; ++i)
            {
                BT_HoldObjInfoList[i].SetActiveHandObj( false );
            }


            SetActiveHoldableEffect( false );
        }
    }

    public void SetActiveHoldableEffect(bool bActive)
    {
        if (BT_HoldObjInfoList != null)
        {
            for (int i = 0; i < BT_HoldObjInfoList.Count; ++i)
            {
                if (BT_HoldObjInfoList[i].HandEffect != null)
                {
                    BT_HoldObjInfoList[i].SetActiveHandEffObjs( bActive );
                }
            }
        }
    }
         

    public void RemoveHoldObjectInfo(long ID)
    {
        for (int i = 0; i < BT_HoldObjInfoList.Count; ++i)
        {
            if (ID == BT_HoldObjInfoList[i].ObjectDataID)
            {
                BT_HoldObjInfoList.RemoveAt(i);
                return;
            }
        }
    }

    public GameObject GetOrgHoldObject(string path, GameObject charObj)
    {
        if (path.Equals( "0" )) return null;

        if (OrgResHoldObjectDic.ContainsKey( path ))
        {
            return OrgResHoldObjectDic[path];
        }

        CResourceData resData = CResourceManager.Instance.GetResourceData( path );
        GameObject retObj = resData.Load<GameObject>( charObj );
        OrgResHoldObjectDic.Add( path, retObj );
        return retObj;
    }

    public void SetPlayShopMotionID(long motionID)
    {
        PlayShopMotionID = motionID;
    }

    public long GetPlayShopMotionID()
    {
        return PlayShopMotionID;
    }

    public void SetPlayerUID(long uid)
    {
        PlayerUID = uid;
    }

    public long GetPlayerUID()
    {
        return PlayerUID;
    }


    // Update is called once per frame
    protected virtual void Update()
    {
        if (StateMachine != null)
        {
            StateMachine.StateMachine_Update();
        }
    }

    public void ReleaseHoldableObjects()
    {
        if (BT_HoldObjInfoList == null)
        {
            CDebug.LogError ($"BT_HoldObjInfoList is NULL");
            return;
        }

        for(int i=0; i< BT_HoldObjInfoList.Count; ++i)
        {
            if (BT_HoldObjInfoList[i] != null)
            {
                BT_HoldObjInfoList[i].Release ();
            }
            else
            {
                CDebug.LogError ($"BT_HoldObjInfoList[{i}] is NULL");
            }
        }
        BT_HoldObjInfoList.Clear();
        BT_HoldObjInfoList = null;
    }

   
    public void DestroyComponents()
    {
		DestroyBT();

		if( colliderManager != null )
        {
            colliderManager.DestoryComponent();
        }

        if (agent != null)
        {
            Destroy(agent);
            agent = null;
        }


    }


    protected virtual void OnDestroy()
    {
        if (StateMachine != null)
        {
            StateMachine.DestroyStateMachine();
            StateMachine = null;
        }
        if (MyAnimator != null)
        {
            MyAnimator = null;
        }

        if(RunAnimContrlllerReleaseObj != null)
        {
            Destroy(RunAnimContrlllerReleaseObj);
            RunAnimContrlllerReleaseObj = null;
        }

        if (agent != null) agent = null;

        if( shadowManager != null )
		{
			shadowManager.DestroyShadow();
			shadowManager.Destroy();
            shadowManager = null;
        }

        if (OrgResHoldObjectDic != null)
        {
            OrgResHoldObjectDic.Clear();
            OrgResHoldObjectDic = null;
        }

        if( colliderManager != null )
        {
			colliderManager.Destroy();
            colliderManager = null;
		}

        DestroyComponents();
    }
}