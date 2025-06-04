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

using UnityEngine;
using BPWPacketDefine;
using DG.Tweening;
using UnityEngine.EventSystems;
using UniRx;

public class MGPihagiPlayer
{
    public MGPihagiWorldManager WorldMgr;
    public MGPihagiPlayersManager PlayersMgr;
    private MGPihagiPageUI PageUI { get => WorldMgr.GetPageUI(); }

    public CCharacterController_BPWorld PlayerController;
    private CharacterController PlayerCharControllerInner;
    private MGPihagiPlayerController PlayerActController;

    public CStateMachine<MGPihagiPlayer> PlayerSM;

    private RectTransform CanvasRect;
    private BPWorldLayerObject LayerObj;

    private GameObject UIDummyBone;
    public static string CHARACTER_UI_DUMMY_BONE = "Dummy_Name";
    private GameObject DummyHeadBone;

    private GameObject PlayerRootObj;
	private CapsuleCollider PlayerCollider;

    public MGPihagiPlayerInfo PlayerInfo;
    public int PID;
    private Transform ShadowTransform;

    private GameObject StunFXObj;
    private GameObject InvincibleFXObj;
    private bool IsInvincible;

    private FixedJoystick Joystick;
    private CJoyStickController JoyStickController;

    public Vector3 DeathTargetPosition;

    private PihagiGamePlayerState PrevPlayerSnapShotState;
    private PihagiGamePlayerState CurPlayerSnapShotState;

    private GameObject indecatorObj;
    private SingleAssignmentDisposable IndicatorObservable;

    private long AnimControllerGroupID;
    private string AnimControllerKeyID;
    private GameObject RunAnimContrlllerReleaseObj;

    public void Initialize(int pid)
    {
        MGPihagiCanvasManager canvasMgr = (MGPihagiCanvasManager)MGPihagiManager.Instance.GetCanvasManager();
        CanvasRect = canvasMgr.GetComponent<RectTransform>();
        LayerObj = canvasMgr.LayerObjects;
        WorldMgr = MGPihagiManager.Instance.WorldManager;
        PlayersMgr = WorldMgr.PlayerMgr;
        PlayerRootObj = PlayersMgr.gameObject;

        PlayerSM = new CStateMachine<MGPihagiPlayer>(this);

        SetPlayerInfo(pid);

        //SetPlayerComponent(CHARACTER_TYPE.AVATAR);
        SetPlayerComponent(PlayerInfo.CharacterType);
        SetStunFXObj();
        SetInvincibleFXObj();

        LoadMyPlayerIndicator(PlayerInfo.UID);
    }

    private void SetPlayerInfo(int pid)
    {
        PlayerInfo = MGPihagiServerDataManager.Instance.GetGamePlayerInfo(pid);
        PID = pid;


        switch(PlayerInfo.CharacterType)
        {
            case CHARACTER_TYPE.AVATAR:
                AVATAR_TYPE aType = PlayerInfo.AvatarType;
                PlayerInfo.StyleItem.SetPlayerEquipItemDic(aType, PlayerInfo.StylingItemInfo);
                PlayerInfo.PlayerObj = PlayerInfo.StyleItem.LoadNetAvatarObject(PlayersMgr.MyPlayerUID, PlayerInfo.UID, aType, PlayerRootObj);
                AnimControllerKeyID = string.Format( "{0}", PlayerInfo.UID );
                AnimControllerGroupID = (long)aType;
                break;
            case CHARACTER_TYPE.NPC:
                long npcDataID = PlayerInfo.CharacterID;
                CNpcInfo npcData = CNpcDataManager.Instance.GetNpcInfo(npcDataID);
                CResourceData resData = CResourceManager.Instance.GetResourceData(npcData.NpcResID);
                GameObject _npcOrgObj = resData.Load<GameObject>(PlayerRootObj);

                PlayerInfo.PlayerObj = Utility.AddChild(PlayerRootObj, _npcOrgObj);
                AnimControllerKeyID = string.Format( "{0}{1}", PlayerInfo.UID, npcDataID );
                AnimControllerGroupID = npcData.AnimControllerGrpID;
                break;
        }

        PlayerInfo.PlayerAnimator = PlayerInfo.PlayerObj.GetComponent<Animator>();

        string name = string.Format( "RunAnimContrlllerReleaseObj_Pihagi_{0}", AnimControllerKeyID );
        RunAnimContrlllerReleaseObj = new GameObject( name );
        RunAnimContrlllerReleaseObj.transform.SetParent( PlayerInfo.PlayerObj.transform.parent );

        RuntimeAnimatorController _runAnimController = CRuntimeAnimControllerSwitchManager.Instance.SetRuntimeAnimatorController( AnimControllerKeyID, AnimControllerGroupID, ANIMCONTROLLER_USE_TYPE.BPW_MINIGAME, RunAnimContrlllerReleaseObj );
        if (_runAnimController != null)
        {
            PlayerInfo.PlayerAnimator.runtimeAnimatorController = _runAnimController;
        }

        AvatarManager.ChangeLayersRecursively(PlayerInfo.PlayerObj.transform, CDefines.AVATAR_BPW_OBJECT_LAYER_NAME);

        UIDummyBone = PlayerInfo.PlayerObj.transform.Find(CHARACTER_UI_DUMMY_BONE).gameObject;
        SetPlayerObject();
    }

    public void SetPlayerObject()
    {
        PlayerInfo.PlayerObj.transform.SetParent(PlayerRootObj.transform);
        PlayerInfo.PlayerObj.transform.localPosition = PlayerInfo.SpawnPosition;
        PlayerInfo.PlayerObj.transform.localRotation = Quaternion.Euler(new Vector3(0, 180, 0));

        ShadowTransform = AvatarManager.Instance.LoadCharacterShadow(PlayerInfo.PlayerObj).transform;

        PlayerInfo.PlayerObj.SetActive(true);
    }

    private void SetPlayerComponent(CHARACTER_TYPE charType)
    {        
        if (PlayerInfo.UID == PlayersMgr.MyPlayerUID)
        {
			PlayerController = PlayerInfo.PlayerObj.GetComponent<CCharacterController_BPWorld>();
			if(PlayerController == null)
			{
				PlayerController = PlayerInfo.PlayerObj.AddComponent<CCharacterController_BPWorld>();
			}
            PlayerController.Initialize(charType);
            PlayerController.SetPlayerUID(PlayerInfo.UID);

			//Unity Inner CharacterController
			PlayerCharControllerInner = GameObjectHelperUtils.GetOrAddComponent<CharacterController>( PlayerInfo.PlayerObj );
			PlayerCharControllerInner.stepOffset = 0;

			//Set JoyStick
			Joystick = PageUI.FixedJoystick;
			JoyStickController = GameObjectHelperUtils.GetOrAddComponent<CJoyStickController>( PlayerInfo.PlayerObj );
			JoyStickController.Initialize(PlayerController, null, Joystick);

            //BPWorldCharacterData charData = BPWorldDataManager.Instance.GetBPWorldCharacterDataByCharacterID((long)PlayerInfo.AvatarType);
            BPWorldCharacterData charData = BPWorldDataManager.Instance.GetBPWorldCharacterDataByCharacterID((long)PlayerInfo.CharacterID);
            BPWorldNetPlayer_SpawnPosition spawnPositionInfo = new BPWorldNetPlayer_SpawnPosition();
            spawnPositionInfo.SetSpawnInfo(PlayerInfo.PlayerObj.transform.localPosition, PlayerInfo.PlayerObj.transform.localRotation);
            PlayerController.Setup(PlayerInfo.UID, charData, Camera.main.transform, ShadowTransform, charData.Height, spawnPositionInfo);

            PlayerInfo.PlayerObj.transform.localRotation = Quaternion.Euler(new Vector3(0, 180, 0));
        }
		PlayerActController = PlayerInfo.PlayerObj.GetComponent<MGPihagiPlayerController>();
		if(PlayerActController == null)
		{
			PlayerActController = PlayerInfo.PlayerObj.AddComponent<MGPihagiPlayerController>();
		}

        PlayerActController.Initialize();

        if (PlayerInfo.UID != PlayersMgr.MyPlayerUID)
        {
            PlayerActController.SetShadowObj(ShadowTransform);	
			SetNetPlayerCollider();
        }
    }

	private void SetNetPlayerCollider()
	{
		GameObject colliderObj = new GameObject();
		colliderObj.transform.SetParent(PlayerInfo.PlayerObj.transform);
		colliderObj.transform.localPosition = Vector3.zero;
		CapsuleCollider col = colliderObj.AddComponent<CapsuleCollider>();
		colliderObj.layer = LayerMask.NameToLayer("Default");
		col.center = new Vector3(0, 1, 0);
		col.radius = 0.3f;
		col.height = 2;
	}

    private void SetStunFXObj()
    {
        //Stun FX
        CharAnimationEvent _cae = PlayerInfo.PlayerObj.GetComponent<CharAnimationEvent>();
        if (_cae == null)
        {
            CDebug.LogError($"{PlayerInfo.NickName} player(stun) doesn't have CharAnimationEvent Component !!!!!");
            return;
        }
        DummyHeadBone = _cae.Dummy_Head;
        StunFXObj = GameObject.Instantiate(PlayersMgr.StunFX_OrgObj);
        StunFXObj.transform.SetParent(DummyHeadBone.transform);
        StunFXObj.transform.localPosition = Vector3.zero;
        StunFXObj.transform.localRotation = Quaternion.Euler(Vector3.zero);
        StunFXObj.transform.localScale = Vector3.one;
        StunFXObj.SetActive(false);
    }

    private void SetInvincibleFXObj()
    {
        CharAnimationEvent _cae = PlayerInfo.PlayerObj.GetComponent<CharAnimationEvent>();
        if (_cae == null)
        {
            CDebug.LogError($"{PlayerInfo.NickName} player(Invincible) doesn't have CharAnimationEvent Component !!!!!");
            return;
        }
        DummyHeadBone = _cae.Dummy_Head;
        InvincibleFXObj = GameObject.Instantiate(PlayersMgr.InvcbFX_OrgObj);
        InvincibleFXObj.transform.SetParent(DummyHeadBone.transform);
        InvincibleFXObj.transform.localPosition = Vector3.zero;
        InvincibleFXObj.transform.localRotation = Quaternion.Euler(Vector3.zero);
        InvincibleFXObj.transform.localScale = Vector3.one;
        InvincibleFXObj.SetActive(false);
    }


    public GameObject GetUIDummyBone()
    {
        return UIDummyBone;
    }
	
    private void SetCollider()
    {
        PlayerCollider.center = new Vector3(0, 0.5f, 0);
        PlayerCollider.radius = 0.5f;
        PlayerCollider.height = 2;
    }

    public PihagiGamePlayerState GetPlayerState()
    {
        return CurPlayerSnapShotState;
    }

    public void SetPlayerState(PihagiGamePlayerState state, bool isInvincible)
    {
        CurPlayerSnapShotState = state;
        //CDebug.Log($"SetPlayerState() name :{PlayerInfo.NickName}, state:{state}");

        switch (state)
        {
            case PihagiGamePlayerState.DEATH:
                if (IsChangeState())
                {
                    CDebug.Log("SetPlayerState().PihagiGamePlayerState.DEATH !!!!!! name = " + PlayerInfo.NickName);
                    SetChangeState(MGPihagiPlayerState_Death.Instance());
                }
                break;
            case PihagiGamePlayerState.IDLE:
				if (IsChangeState())
				{
					if (PlayerInfo.UID != PlayersMgr.MyPlayerUID)
					{ 
						//CDebug.Log("SetPlayerState().PihagiGamePlayerState.IDLE !!!!!! name = " + PlayerInfo.NickName);					
						SetChangeState(MGPihagiPlayerState_MoveStop.Instance());
					}

				}
                break;
            case PihagiGamePlayerState.MOVE:
            //case PihagiGamePlayerState.INVINCIBLE:
                if (IsChangeState())
                {
                    if (PlayerInfo.UID != PlayersMgr.MyPlayerUID)
                    {
                        SetChangeState(MGPihagiPlayerState_Move.Instance());
                    }
      //              else
      //              {
						//CDebug.Log($"SetPlayerState() PlayerSM.GetCurrentState() = {PlayerSM.GetCurrentState()}");
      //                  if (PlayerSM.GetCurrentState() == MGPihagiPlayerState_Stun.Instance() ||
						//	PlayerSM.GetCurrentState() == MGPihagiPlayerState_MoveStop.Instance())
      //                  {
      //                      SetChangeState(MGPihagiPlayerState_Idle.Instance());
      //                      PlayerController.SetChangeStateMachine(CharState_BPWorld_WalkControl.Instance());
      //                  }
      //              }
                }
                break;
            case PihagiGamePlayerState.STUN:
                if (IsChangeState())
                {
                    CDebug.Log("@@@@@@@@@    SetPlayerState().PihagiGamePlayerState.STUN !!!!!! name = " + PlayerInfo.NickName +" / PlayerSM.GetCurrentState() ="+PlayerSM.GetCurrentState());
                    if (PlayerInfo.UID == PlayersMgr.MyPlayerUID)
                    {
                        if (PlayerSM.GetPreviousState() != MGPihagiPlayerState_Stun.Instance())
                        {
                            PlayerController.SetChangeStateMachine(CharState_BPWorld_MetaverseSpawnWait.Instance());
                        }
                    }
                    SetChangeState(MGPihagiPlayerState_Stun.Instance());
                }
                break;
        }

        if(!IsInvincible)
        {
            if(isInvincible)
            {
                SetInvincible();
                IsInvincible = true;
            }
        }
        else
        {
            if (!isInvincible)
            {
                ReleaseInvincible();
                IsInvincible = false;
            }
        }

        PrevPlayerSnapShotState = CurPlayerSnapShotState;
    }


    public void LoadMyPlayerIndicator(long uid)
    {
        if (indecatorObj == null)
        {
            if (PlayersMgr.MyPlayerUID == uid)
            {
                CResourceData resData = CResourceManager.Instance.GetResourceData(MGNunchiConstants.MY_PLAYER_INDICATOR_PATH);
                GameObject obj = resData.Load<GameObject>(PlayerInfo.PlayerObj);
                //indecatorObj = Utility.AddChild(PlayerInfo.PlayerObj, obj);
                indecatorObj = Utility.AddChild(UIDummyBone, obj);

                //float charHeight = 1.8f;//default
                //BPWorldCharacterData characterData = BPWorldDataManager.Instance.GetBPWorldCharacterDataByID(PlayerInfo.CharacterID);
                //if (characterData != null)
                //{
                //    charHeight = characterData.Height;
                //}
                //indecatorObj.transform.localPosition = new Vector3(0, charHeight, 0);

                IndicatorObservable = new SingleAssignmentDisposable();
                IndicatorObservable.Disposable = Observable.EveryUpdate()
                    .Subscribe(x =>
                    {
                        if (indecatorObj != null)
                        {
                            indecatorObj.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
                        }
                    }).AddTo(PlayerInfo.PlayerObj);

                SetActiveIndecatorObj(false);
            }
        }
    }

    public void SetActiveIndecatorObj(bool bActive)
    {
        indecatorObj.SetActive(bActive);
    }

    private bool IsChangeState()
    {
        if (PrevPlayerSnapShotState != CurPlayerSnapShotState)
        {
            return true;
        }

        return false;
    }

    public void SetSpawnPos(Vector3 pos)
    {
        PlayerInfo.PlayerObj.transform.localPosition = pos;
    }



    public void UpdateStateMachine()
    {
        if(PlayerSM != null)
        {
            PlayerSM.StateMachine_Update();
        }
    }

    public void UpdateMovePos(Vector3 pos)
    {
        if (PlayerInfo.UID != PlayersMgr.MyPlayerUID)
        {
            PlayerActController.SetPlayerMoveRecv(pos);
        }
    }

    public void SetLifeCount(int life)
    {
        PlayerInfo.LifeCount = life;
        //CDebug.Log("SetLifeCount() current life count = " + PlayerInfo.LifeCount +"/"+PlayerInfo.NickName);//
    }

	public void SetMyPlayerControl()
	{
		SetAnimationTrigger( "idle" );
		SetChangeState( MGPihagiPlayerState_Idle.Instance() );
		PlayerController.SetChangeStateMachine( CharState_BPWorld_WalkControl.Instance() );
	}

	public void SetStun()
	{
        StunFXObj.SetActive(true);
    }

    public void ReleaseStun()
    {
        StunFXObj.SetActive(false);
    }

    public void SetInvincible()
    {
        InvincibleFXObj.SetActive(true);
    }

    public void ReleaseInvincible()
    {
        InvincibleFXObj.SetActive(false);
    }

    public void SetDeath(MGPihagiEnemy attacker)
    {
        Transform enemyObj = attacker.EnemyObj.transform;

        DeathTargetPosition = GetDeathTargetPositionByAttackerDir(attacker.EnemyDir, enemyObj.position);

        PlayerInfo.PlayerObj.transform.LookAt(enemyObj);
    }

    private Vector3 GetDeathTargetPositionByAttackerDir(ENEMY_POSDIR attackerDir, Vector3 attackerPos)
    {
        Vector3 pos = WorldMgr.GetPlayerDeathTargetPos(attackerDir);
        switch (attackerDir)
        {
            case ENEMY_POSDIR.LEFT:
            case ENEMY_POSDIR.RIGHT:
                pos.z = attackerPos.z;
                break;
            case ENEMY_POSDIR.TOP:
                pos.x = attackerPos.x;
                break;
        }

        return pos;
    }

    public Vector3 GetDeathTargetPosition()
    {
        return DeathTargetPosition;
    }

	public void SetDeath()
	{
        PlayerInfo.PlayerObj.transform.DOLocalMove(DeathTargetPosition, 1.1f)
            .OnComplete(() =>
            {
                SetActivePlayerObj(false);
                SetChangeState(MGPihagiPlayerState_Observe.Instance());
            });
	}

    public void SetJump(bool bJump)
    {
        if (PlayerInfo.UID != PlayersMgr.MyPlayerUID)
        {
            PlayerActController.SetAnimationBool("isJump", bJump);
        }
    }



    public void LockJoyStick()
    {
        JoyStickController.enabled = false;
    }

    public void UnlockJoyStick()
    {
        JoyStickController.enabled = true;
    }

    public void SetAnimationTrigger(string param)
    {
        PlayerActController.SetAnimatonTrigger(param);
    }

    public void SetChangeState(CState<MGPihagiPlayer> state)
    {
        PlayerSM.ChangeState(state);
    }


    public void SetActivePlayerObj(bool bActive)
    {
        PlayerInfo.PlayerObj.SetActive(bActive);
		if(PlayerInfo.UID == PlayersMgr.MyPlayerUID)
		{
			PlayerController.SetActiveShadowObj(bActive, PlayerInfo.PlayerObj);
		}
		else
		{
			PlayerActController.SetActiveShadowObject(bActive);
		}
    }
	
	public void SetJoyStickStop(bool bStop)
	{
		JoyStickController.SetStopJoyStick(bStop);

		if(bStop)
		{
			PointerEventData curPosition = new PointerEventData( EventSystem.current );
			JoyStickController.SetJoyStickRelease( curPosition );
		}
	}

    public void PlayAnimation(string animParam)
    {
        if(PlayerController == null)
        {
            Debug.LogError($"MGNunchiPlayer.PlayAnimation. controller is null. param = {animParam}");
            return;
        }

        PlayerController.SetChangeMotionStateWithParameter(MOTION_STATE.PLAYANIM, animParam);
    }

    public void CleanUpPlayer()
    {
        PlayerInfo.StyleItem.CleanUp();
		if (PlayerController != null)
		{
			PlayerController.ShadowManager.DestroyShadow();
		}
  //      if (PlayerCharControllerInner != null)
		//{
		//	GameObject.Destroy(PlayerCharControllerInner);
		//}
        if (PlayerActController != null)
		{		
			PlayerActController.Release();
			//GameObject.Destroy(PlayerActController);
		}
        //      if (JoyStickController != null)
        //{
        //	GameObject.Destroy(JoyStickController);
        //}
        if (PlayerInfo.CharacterType == CHARACTER_TYPE.AVATAR)
        {
            if (PlayerInfo.StyleItem != null)
            {
                PlayerInfo.StyleItem.CleanUp();
            }
        }

        if (PlayerInfo.PlayerID == PlayersMgr.MYPID)
        {
            if (indecatorObj != null)
            {
                IndicatorObservable.Dispose();
                UnityEngine.Object.Destroy(indecatorObj);
                indecatorObj = null;
            }
            if (PlayerInfo.CharacterType == CHARACTER_TYPE.AVATAR)
            {
                StaticAvatarManager.SetAvatarObjBack();
            }
            else
            {
                GameObject.Destroy(PlayerInfo.PlayerObj);
                PlayerInfo.PlayerObj = null;
            }
        }
        else
        {
            GameObject.Destroy(PlayerInfo.PlayerObj);
			//Object.Destroy(ShadowTransform);
            PlayerInfo.PlayerObj = null;
			ShadowTransform = null;
        }

        CRuntimeAnimControllerSwitchManager.Instance.ReleaseRuntimeAnimController( AnimControllerKeyID, AnimControllerGroupID );
        PlayerInfo.PlayerAnimator.runtimeAnimatorController = null;
        PlayerInfo.PlayerAnimator = null;
    }

    public void Release()
    {
        if (WorldMgr != null) WorldMgr = null;
        if (PlayersMgr != null) PlayersMgr = null;
        if (PlayerController != null) PlayerController = null;
  //      if (PlayerCharControllerInner != null)
		//{
		//	GameObject.Destroy(PlayerCharControllerInner);
		//	PlayerCharControllerInner = null;
		//}
        if (PlayerActController != null)
		{		
			PlayerActController.Release();
			GameObject.Destroy(PlayerActController);
			PlayerActController = null;
		}
        if (PlayerSM != null) PlayerSM = null;
        if (CanvasRect != null) CanvasRect = null;
        if (LayerObj != null) LayerObj = null;
        if (UIDummyBone != null) UIDummyBone = null;
        if (PlayerRootObj != null) PlayerRootObj = null;
        if (PlayerInfo != null)
        {
            CRuntimeAnimControllerSwitchManager.Instance.ReleaseRuntimeAnimController( AnimControllerKeyID, AnimControllerGroupID );
            PlayerInfo.PlayerAnimator.runtimeAnimatorController = null;
            PlayerInfo.PlayerAnimator = null;
            PlayerInfo.PlayerObj = null;
            PlayerInfo = null;
        }
		if (ShadowTransform != null) ShadowTransform = null;
		if (StunFXObj != null)
		{
			GameObject.Destroy(StunFXObj);
			StunFXObj = null;
		}
        if (InvincibleFXObj != null)
        {
            GameObject.Destroy(InvincibleFXObj);
            InvincibleFXObj = null;
        }
        if (Joystick != null) Joystick = null;
        if (JoyStickController != null)
		{
            JoyStickController.Release();
            GameObject.Destroy(JoyStickController);
			JoyStickController = null;
        }
        if (indecatorObj != null)
        {
            IndicatorObservable.Dispose();
            UnityEngine.Object.Destroy(indecatorObj);
            indecatorObj = null;
        }

        if (RunAnimContrlllerReleaseObj != null)
        {
            UnityEngine.Object.Destroy( RunAnimContrlllerReleaseObj );
            RunAnimContrlllerReleaseObj = null;
        }
    }
}