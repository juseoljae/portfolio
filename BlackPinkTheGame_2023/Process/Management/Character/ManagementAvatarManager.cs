using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GroupManagement;
using GroupManagement.ManagementEnums;
using UniRx;
using BehaviorDesigner.Runtime;
using System.Linq;

public class ManagementAvatarManager : MonoBehaviour
{
    private ManagementWorldManager WorldMgr;

    private Dictionary<AVATAR_TYPE, GameObject> AvatarObjDic;
    private Dictionary<AVATAR_TYPE, GameObject> AvatarShadowObjDic;
    private Dictionary<AVATAR_TYPE, Animator> AvatarAnimatorDic;

    private Dictionary<GridIndex, AVATAR_TYPE> AvatarGridPoolDic;
    private Dictionary<int, List<AVATAR_TYPE>> AvatarSpawnEachFloor;
    private Dictionary<AVATAR_TYPE, GridIndex> AvatarSpawnGridDic;
    private Dictionary<AVATAR_TYPE, GridIndex> AvatarCurrentGridDic;


    private Dictionary<AVATAR_TYPE, GameObject> AvatarUIDummyBoneDic;
    private Dictionary<AVATAR_TYPE, CAvatar> AvatarInfoDic;
    private Dictionary<AVATAR_TYPE, CCharacterController_Management> AvatarControllerDic;

    private Dictionary<AVATAR_TYPE, GameObject> OrgAvatarParent;
    private Dictionary<AVATAR_TYPE, Transform> PrevAvatarParent;
    private Dictionary<AVATAR_TYPE, Vector3> PrevAvatarPos;

    private Dictionary<AVATAR_TYPE, CEvent> AvatarEvent;
    private Dictionary<AVATAR_TYPE, EVENT_STATE> AvatarEventState;

    //Event Icon UI
    private Dictionary<AVATAR_TYPE, ManagementAvatarEventUI> AvatarEventIconObj;
	private Dictionary<AVATAR_TYPE, GameObject> AvatarNameTagObj;
    //Interaction Icon UI
    private Dictionary<AVATAR_TYPE, ManagementAvatarInteractionUI> AvatarInteractionIconObjDic;

    private Dictionary<AVATAR_TYPE, BehaviorTree> AvatarBTDic;
    private Dictionary<string, ExternalBehavior> AvatarBTFilePool;
    private Dictionary<AVATAR_TYPE, SectionScript> AvatarTrainingSectionScript;

    private SingleAssignmentDisposable[] DelayTimerDisposer;
    private SingleAssignmentDisposable[] SubDelayTimerDisposer;
    public SingleAssignmentDisposable[] InteractionDisposer;

    //public Dictionary<AVATAR_TYPE, MailIcon> AvatarMailIconObjDic;
    private Dictionary<AVATAR_TYPE, ManagementAvatarEventUI> AvatarMailQuestIconObj;
    public Dictionary<AVATAR_TYPE, QUEST_STATE> AvatarQuestStateDic;


    public void Initialize()
    {
        WorldMgr = ManagementManager.Instance.worldManager;

        AvatarObjDic = new Dictionary<AVATAR_TYPE, GameObject>();
        AvatarSpawnGridDic = new Dictionary<AVATAR_TYPE, GridIndex>();
        AvatarAnimatorDic = new Dictionary<AVATAR_TYPE, Animator>();
        AvatarCurrentGridDic = new Dictionary<AVATAR_TYPE, GridIndex>();
        AvatarGridPoolDic = new Dictionary<GridIndex, AVATAR_TYPE>();
        AvatarSpawnEachFloor = new Dictionary<int, List<AVATAR_TYPE>>();
        AvatarShadowObjDic = new Dictionary<AVATAR_TYPE, GameObject>();
        AvatarUIDummyBoneDic = new Dictionary<AVATAR_TYPE, GameObject>();
        AvatarInfoDic = new Dictionary<AVATAR_TYPE, CAvatar>();
        AvatarControllerDic = new Dictionary<AVATAR_TYPE, CCharacterController_Management>();
        

        OrgAvatarParent = new Dictionary<AVATAR_TYPE, GameObject>();
        PrevAvatarParent = new Dictionary<AVATAR_TYPE, Transform>();
        PrevAvatarPos = new Dictionary<AVATAR_TYPE, Vector3>();

        AvatarEvent = new Dictionary<AVATAR_TYPE, CEvent>();
        AvatarEventIconObj = new Dictionary<AVATAR_TYPE, ManagementAvatarEventUI>();
		AvatarNameTagObj = new Dictionary<AVATAR_TYPE, GameObject>();
        AvatarInteractionIconObjDic = new Dictionary<AVATAR_TYPE, ManagementAvatarInteractionUI>();
        AvatarEventState = new Dictionary<AVATAR_TYPE, EVENT_STATE>();

        AvatarBTDic = new Dictionary<AVATAR_TYPE, BehaviorTree>();
        AvatarBTFilePool = new Dictionary<string, ExternalBehavior>();
        AvatarTrainingSectionScript = new Dictionary<AVATAR_TYPE, SectionScript>();

        DelayTimerDisposer = new SingleAssignmentDisposable[4];
        SubDelayTimerDisposer = new SingleAssignmentDisposable[4];
        InteractionDisposer = new SingleAssignmentDisposable[4];

        //AvatarMailIconObjDic = new Dictionary<AVATAR_TYPE, MailIcon>();
        AvatarQuestStateDic = new Dictionary<AVATAR_TYPE, QUEST_STATE>();
        AvatarMailQuestIconObj = new Dictionary<AVATAR_TYPE, ManagementAvatarEventUI>();
    }

	private void Start()
	{
		
	}

	private void FixedUpdate()
	{
        CamRatioNameTagActiveUpdate();
	}

	private void CamRatioNameTagActiveUpdate()
	{
		if(WorldMgr != null && WorldMgr.CamController != null)
		{
			var currentCamSize = WorldMgr.CamController.GetCameraMoveRatioByZoom();
			var active = currentCamSize > ManagementWorldManager.NAME_TAG_SHOW_RATIO;
			foreach(AVATAR_TYPE avatarType in AvatarObjDic.Keys)
			{
				if(AvatarObjDic.ContainsKey(avatarType))
				{
					if(AvatarObjDic[avatarType].activeSelf)
					{
						AvatarNameTagObj[avatarType].SetActive(active);
					}
					else
					{
						AvatarNameTagObj[avatarType].SetActive(false);
					}
				}
			}
		}
	}

    public void InitDelayTimeDisposer(int idx)
    {
        if (DelayTimerDisposer[idx] == null)
        {
            DelayTimerDisposer[idx] = new SingleAssignmentDisposable();
        }
        else
        {
            DelayTimerDisposer[idx].Dispose();
            DelayTimerDisposer[idx] = new SingleAssignmentDisposable();
        }
    }

    public void InitSubDelayTimeDisposer(int idx)
    {
        if(SubDelayTimerDisposer[idx] == null)
        {
            SubDelayTimerDisposer[idx] = new SingleAssignmentDisposable();
        }
        else
        {
            SubDelayTimerDisposer[idx].Dispose();
            SubDelayTimerDisposer[idx] = new SingleAssignmentDisposable();
        }
    }

    public void InitInteractionDisposer(int idx)
    {
        if(InteractionDisposer[idx] == null)
        {
            InteractionDisposer[idx] = new SingleAssignmentDisposable();
        }
        else
        {
            InteractionDisposer[idx].Dispose();
            InteractionDisposer[idx] = new SingleAssignmentDisposable();
        }
    }

	public void RefreshAvatarNameTag()
	{
		for (int i = 0; i < CDefines.VIEW_AVATAR_TYPE.Length; i++)
        {
			AVATAR_TYPE avatarType = CDefines.VIEW_AVATAR_TYPE[i];
			if(AvatarNameTagObj.ContainsKey(avatarType))
			{
				var nameTagObj = AvatarNameTagObj[avatarType];
				var nameTab = nameTagObj.GetComponent<ManagementNameTag3D>();
				if(nameTab)
				{
					nameTab.SetLevel(CPlayer.GetAvatar(avatarType).Lv);
				}
			}
		}
	}

    //public void LoadAvatar()
    public IObservable<Unit> LoadAvatar()
    {
        AddToAvatarBTFilePool();

        return Observable.FromCoroutine(cancelToken => EnumerableAsyncLoad(cancelToken));

        //멤버 보이는 순서로 로딩
        // for (int i = 0; i < CDefines.VIEW_AVATAR_TYPE.Length; i++)
        // {
        //     AVATAR_TYPE avatarType = CDefines.VIEW_AVATAR_TYPE[i];
        //     GameObject rootObj = transform.Find($"Member_{i + 1}").gameObject;

		// 	var avatar = CPlayer.GetAvatar(avatarType);
		// 	avatar.lv_update.Subscribe( _=>
		// 	{
		// 		RefreshAvatarNameTag();
		// 	}).AddTo(gameObject);

        //     AvatarInfoDic.Add(avatarType, avatar);

        //     StaticAvatarManager.InitAvatarBlendShape(avatarType);
        //     GameObject avatarObj = StaticAvatarManager.GetAvatarObject(avatarType);
        //     avatarObj.transform.SetParent(rootObj.transform);
        //     avatarObj.SetActive(true);

		// 	var uiDummyBone = avatarObj.transform.Find(ManagementWorldManager.CHARACTER_UI_DUMMY_BONE);
		// 	AvatarUIDummyBoneDic.Add(avatarType, uiDummyBone.gameObject);
		// 	var avatarName = CResourceManager.Instance.GetDefaultStr(avatarType);
        //     //AvatarNameTagObj.Add(avatarType, ManagementManager.Instance.AddNameTag(uiDummyBone.gameObject, avatarName, CPlayer.GetAvatar(avatarType).Lv));
        //     AvatarNameTagObj.Add(avatarType, ManagementManager.Instance.AddNameTag3D(uiDummyBone.gameObject, avatarName, CPlayer.GetAvatar(avatarType).Lv));

        //     TouchedEventManager.SetTouchEventUIObject(avatarType, TOUCHEVENT_SCENE_TYPE.MANAGEMENT, avatarObj, ManagementManager.Instance.GetTouchLayer());

        //     //Set Tag, Layer
        //     switch (avatarType)
        //     {
        //         case AVATAR_TYPE.AVATAR_JISOO:
        //             avatarObj.tag = Management_LayerTag.Tag_AVATAR_JISOO;
        //             break;
        //         case AVATAR_TYPE.AVATAR_JENNIE:
        //             avatarObj.tag = Management_LayerTag.Tag_AVATAR_JENNIE;
        //             break;
        //         case AVATAR_TYPE.AVATAR_LISA:
        //             avatarObj.tag = Management_LayerTag.Tag_AVATAR_LISA;
        //             break;
        //         case AVATAR_TYPE.AVATAR_ROSE:
        //             avatarObj.tag = Management_LayerTag.Tag_AVATAR_ROSE;
        //             break;
        //     }
        //     AvatarManager.ChangeLayersRecursively(avatarObj.transform, "Default");
        //     avatarObj.layer = LayerMask.NameToLayer(Management_LayerTag.Layer_SECTION);



        //     //Async--
        //     AsyncLoadCoroutine = StartCoroutine(EnumerableAsyncLoad(avatarObj, avatarType));

        //     AvatarEventState.Add(avatarType, EVENT_STATE.NONE);
        //     AvatarObjDic.Add(avatarType, avatarObj);
        //     OrgAvatarParent.Add(avatarType, rootObj);


            // GameObject shadowObj = AvatarManager.Instance.SetCharacterShadow(avatarObj);
            // AvatarShadowObjDic.Add(avatarType, shadowObj);

            // //LoadAvatarBT(avatarType, avatarObj);
            // InitAvatarBT(avatarType, avatarObj);

            // //Controller
            // CCharacterController_Management cntlr = avatarObj.AddComponent<CCharacterController_Management>();
            // cntlr.SetAvatarType(avatarType);
            // cntlr.Initialize(CHARACTER_TYPE.AVATAR);

            // AvatarControllerDic.Add(avatarType, cntlr);

            // if (AvatarAnimClipLenthDic.ContainsKey(avatarType) == false)
            // {
            //     Dictionary<string, float> animTimeList = new Dictionary<string, float>();
            //     AvatarAnimClipLenthDic.Add(avatarType, animTimeList);

            //     AnimationClip[] animClips = cntlr.GetAnimatorController().runtimeAnimatorController.animationClips;

            //     //애니메이션 클립을 다 가지고 있을 필요가 있을지 고민해본다
            //     animClips.ForEach((idx, clip) =>
            //     {
            //         if (AvatarAnimClipLenthDic[avatarType].ContainsKey(clip.name))
            //         {
            //             CDebug.Log($"Already animation clip name ::  {clip.name} added data = {clip.length}, adding data = {AvatarAnimClipLenthDic[avatarType][clip.name]}");
            //         }
            //         else
            //         {
            //             AvatarAnimClipLenthDic[avatarType].Add(clip.name, clip.length);
            //         }

            //     });
            // }

            // //Event Icon UI
            // var resData = CResourceManager.Instance.GetResourceData(Constants.PATH_UI_EVENT);
            // GameObject _uiObj = resData.Load<GameObject>(avatarObj);
            // GameObject insUIObj = Utility.AddChild(ManagementManager.Instance.GetEventIconLayer(), _uiObj);
            // ManagementAvatarEventUI aeUI = insUIObj.GetComponent<ManagementAvatarEventUI>();
            // if(aeUI == null)
            // {
            //     aeUI = insUIObj.AddComponent<ManagementAvatarEventUI>();
            // }
            // aeUI.Initialize( RECEIVED_EVENT_TYPE.EVENT );
            // aeUI.SetEventIcon((long)avatarType);
            // insUIObj.SetActive(false);
            // AvatarEventIconObj.Add(avatarType, aeUI);

            // //Interaction Icon UI
            // resData = CResourceManager.Instance.GetResourceData(Constants.PATH_UI_INTERACTION);
            // _uiObj = resData.Load<GameObject>(avatarObj);
            // insUIObj = Utility.AddChild(ManagementManager.Instance.GetEventIconLayer(), _uiObj);
            // ManagementAvatarInteractionUI interactionUI = insUIObj.GetComponent<ManagementAvatarInteractionUI>();
            // if(interactionUI == null)
            // {
            //     interactionUI = insUIObj.AddComponent<ManagementAvatarInteractionUI>();
            // }
            // interactionUI.Initialize(); 
            // insUIObj.SetActive(false);
            // AvatarInteractionIconObjDic.Add(avatarType, interactionUI);

            // //MailQuest Icon UI
            // resData = CResourceManager.Instance.GetResourceData( Constants.PATH_UI_EVENT );
            // _uiObj = resData.Load<GameObject>( avatarObj );
            // insUIObj = Utility.AddChild( ManagementManager.Instance.GetEventIconLayer(), _uiObj );
            // insUIObj.name = "MailQuestIcon_" + avatarType;
            // ManagementAvatarEventUI mailQuestUI = insUIObj.GetComponent<ManagementAvatarEventUI>();
            // if (mailQuestUI == null)
            // {
            //     mailQuestUI = insUIObj.AddComponent<ManagementAvatarEventUI>();
            // }
            // insUIObj.SetActive( false );
            // mailQuestUI.Initialize( RECEIVED_EVENT_TYPE.MAIL_QUEST );
            // AvatarMailQuestIconObj.Add( avatarType, mailQuestUI );
        //}

    }


    //Async--
    private IEnumerator EnumerableAsyncLoad(System.Threading.CancellationToken cancelToken)
    {
        
        //멤버 보이는 순서로 로딩
        for (int i = 0; i < CDefines.VIEW_AVATAR_TYPE.Length; i++)
        {
            AVATAR_TYPE avatarType = CDefines.VIEW_AVATAR_TYPE[i];
            GameObject rootObj = transform.Find($"Member_{i + 1}").gameObject;

            var avatar = CPlayer.GetAvatar(avatarType);
            avatar.lv_update.Subscribe(_ =>
           {
               RefreshAvatarNameTag();
           }).AddTo(gameObject);

            AvatarInfoDic.Add(avatarType, avatar);

            StaticAvatarManager.InitAvatarBlendShape(avatarType);
            GameObject avatarObj = StaticAvatarManager.GetAvatarObject(avatarType);
            Animator _animator = avatarObj.GetComponent<Animator>();
            AvatarAnimatorDic.Add( avatarType, _animator );

            //RuntimeAnimatorController _runAnimCntlr = CRuntimeAnimControllerSwitchManager.Instance.SetRuntimeAnimatorController( avatarType.ToString(), (long)avatarType, ANIMCONTROLLER_USE_TYPE.MANAGEMENT, avatarObj );
            //if(_runAnimCntlr != null )
            //{
            //    AvatarAnimatorDic[avatarType].runtimeAnimatorController = _runAnimCntlr;
            //}
            avatarObj.transform.SetParent(rootObj.transform);
            avatarObj.SetActive(false);

            var uiDummyBone = avatarObj.transform.Find(ManagementWorldManager.CHARACTER_UI_DUMMY_BONE);
            AvatarUIDummyBoneDic.Add(avatarType, uiDummyBone.gameObject);
            var avatarName = CResourceManager.Instance.GetDefaultStr(avatarType);
            //AvatarNameTagObj.Add(avatarType, ManagementManager.Instance.AddNameTag(uiDummyBone.gameObject, avatarName, CPlayer.GetAvatar(avatarType).Lv));
            AvatarNameTagObj.Add(avatarType, ManagementManager.Instance.AddNameTag3D(uiDummyBone.gameObject, avatarName, CPlayer.GetAvatar(avatarType).Lv));

            TouchedEventManager.SetTouchEventUIObject(avatarType, TOUCHEVENT_SCENE_TYPE.MANAGEMENT, avatarObj, ManagementManager.Instance.GetTouchLayer());

            //Set Tag, Layer
            switch (avatarType)
            {
                case AVATAR_TYPE.AVATAR_JISOO:
                    avatarObj.tag = Management_LayerTag.Tag_AVATAR_JISOO;
                    break;
                case AVATAR_TYPE.AVATAR_JENNIE:
                    avatarObj.tag = Management_LayerTag.Tag_AVATAR_JENNIE;
                    break;
                case AVATAR_TYPE.AVATAR_LISA:
                    avatarObj.tag = Management_LayerTag.Tag_AVATAR_LISA;
                    break;
                case AVATAR_TYPE.AVATAR_ROSE:
                    avatarObj.tag = Management_LayerTag.Tag_AVATAR_ROSE;
                    break;
            }
            AvatarManager.ChangeLayersRecursively(avatarObj.transform, "Default");
            avatarObj.layer = LayerMask.NameToLayer(Management_LayerTag.Layer_SECTION);

            InitAvatarBT(avatarType, avatarObj);

            // Shadow
            var shadowResData = CResourceManager.Instance.GetResourceData(Constants.PATH_CHAR_SHADOW);
            yield return shadowResData.LoadAsync<GameObject>(rootObj)
            .Do(asset =>
            {
                var shadowObj = asset;
                var shadow = UnityEngine.Object.Instantiate(shadowObj);
                shadow.transform.parent = avatarObj.transform;
                shadow.transform.localPosition = new Vector3(0, 0.01f, 0);//Vector3.zero;
                AvatarShadowObjDic.Add(avatarType, shadow);
            })
            .ToYieldInstruction();


            //Controller
            var cntlr = avatarObj.GetComponent<CCharacterController_Management>();
            cntlr ??= avatarObj.AddComponent<CCharacterController_Management>();
            cntlr.Initialize(CHARACTER_TYPE.NPC);
            cntlr.SetAvatarType(avatarType);
            cntlr.Initialize(CHARACTER_TYPE.AVATAR);
            cntlr.SetRuntimeAnimatorController( avatarType.ToString(), (long)avatarType, ANIMCONTROLLER_USE_TYPE.MANAGEMENT );
            AvatarControllerDic.Add(avatarType, cntlr);

            //Event Icon UI
            var resData = CResourceManager.Instance.GetResourceData(Constants.PATH_UI_EVENT);
            if (resData != null)
            {
                yield return resData.LoadAsync<GameObject>(rootObj)
                .Do(asset =>
                {
                    GameObject _uiObj = asset;
                    GameObject insUIObj = Utility.AddChild(ManagementManager.Instance.GetEventIconLayer(), _uiObj);
                    ManagementAvatarEventUI aeUI = insUIObj.GetComponent<ManagementAvatarEventUI>();
                    aeUI ??= insUIObj.AddComponent<ManagementAvatarEventUI>();
                    aeUI.Initialize(RECEIVED_EVENT_TYPE.EVENT);
                    aeUI.SetEventIcon((long)avatarType);
                    insUIObj.SetActive(false);
                    if (AvatarEventIconObj.ContainsKey(avatarType) == false)
                    {
                        AvatarEventIconObj.Add(avatarType, aeUI);
                    }
                })
                .ToYieldInstruction();
            }

            //Interaction Icon UI
            var resData3 = CResourceManager.Instance.GetResourceData(Constants.PATH_UI_INTERACTION);
            if (resData3 != null)
            {
                yield return resData3.LoadAsync<GameObject>(rootObj)
                .Do(asset =>
                {
                    GameObject _uiObj = asset;
                    GameObject insUIObj = Utility.AddChild(ManagementManager.Instance.GetEventIconLayer(), _uiObj);
                    ManagementAvatarInteractionUI interactionUI = insUIObj.GetComponent<ManagementAvatarInteractionUI>();
                    interactionUI ??= insUIObj.AddComponent<ManagementAvatarInteractionUI>();
                    interactionUI.Initialize();
                    insUIObj.SetActive(false);
                    if (AvatarInteractionIconObjDic.ContainsKey(avatarType) == false)
                    {
                        AvatarInteractionIconObjDic.Add(avatarType, interactionUI);
                    }
                })
                .ToYieldInstruction();
            }


            //MailQuest Icon UI
            var resData2 = CResourceManager.Instance.GetResourceData(Constants.PATH_UI_EVENT);
            if (resData2 != null)
            {
                yield return resData2.LoadAsync<GameObject>(rootObj)
                .Do(asset =>
                {
                    GameObject _uiObj = asset;
                    GameObject insUIObj = Utility.AddChild(ManagementManager.Instance.GetEventIconLayer(), _uiObj);
                    insUIObj.name = $"MailQuestIcon_{avatarType}";
                    ManagementAvatarEventUI mailQuestUI = insUIObj.GetComponent<ManagementAvatarEventUI>();
                    mailQuestUI ??= insUIObj.AddComponent<ManagementAvatarEventUI>();
                    insUIObj.SetActive(false);
                    mailQuestUI.Initialize(RECEIVED_EVENT_TYPE.MAIL_QUEST);
                    if (AvatarMailQuestIconObj.ContainsKey(avatarType) == false)
                    {
                        AvatarMailQuestIconObj.Add(avatarType, mailQuestUI);
                    }
                })
                .ToYieldInstruction();
            }

            //lod 끄기
            //AvatarManager.SetActivateLOD(ref avatarObj, false);

            AvatarEventState.Add(avatarType, EVENT_STATE.NONE);
            AvatarObjDic.Add(avatarType, avatarObj);
            OrgAvatarParent.Add(avatarType, rootObj);

        }

    }



    private void SetAvatarSpawnGridDic(AVATAR_TYPE avatarType, GridIndex grid)
    {
        if (AvatarSpawnGridDic.ContainsKey(avatarType) == false)
        {
            AvatarSpawnGridDic.Add(avatarType, grid);
        }
        else
        {
            AvatarSpawnGridDic[avatarType] = grid;
        }
    }

    public GridIndex GetAvatarSpawnGrid(AVATAR_TYPE avatarType)
    {
        if (AvatarSpawnGridDic.ContainsKey(avatarType) == false)
        {
            return null;
        }

        return AvatarSpawnGridDic[avatarType];
    }


    public void SetAvatarCurrentGridDic(AVATAR_TYPE avatarType, GridIndex grid)
    {
        if (AvatarCurrentGridDic.ContainsKey(avatarType) == false)
        {
            AvatarCurrentGridDic.Add(avatarType, grid);
        }
        else
        {
            AvatarCurrentGridDic[avatarType] = grid;
        }
    }

    public GridIndex GetAvatarCurrentGridDic(AVATAR_TYPE avatarType)
    {
        return AvatarCurrentGridDic[avatarType];
    }

    public void SetAvatarSpawn()
    {        
        SetAvatarsRandomSpawnGrid();
        //make random Grid in pool
        MakeRandomSpawn();
    }

    public void SetAllAvatarRespawn()
    {
        for (int i = 0; i < CDefines.VIEW_AVATAR_TYPE.Length; ++i)
        {
            AVATAR_TYPE avatarType = CDefines.VIEW_AVATAR_TYPE[i];
            AvatarControllerDic[avatarType].SetReSpawnAvatar();
        }
    }

    public void ShowAvatarRecvedEventIcon()
    {
        for (int i = 0; i < CDefines.VIEW_AVATAR_TYPE.Length; ++i)
        {
            SetActiveAvatarEventIcon(CDefines.VIEW_AVATAR_TYPE[i], true);
        }
    }

    //현재 섹션의 층이 스폰 풀보다 작거나 같은 경우에는 리스폰을 안한다.
    public void SetRefreshAvatarAIGridPool(int sectionFloor, bool bRespawn = false)
    {
        bool goingToRespawn = bRespawn;
        SetAvatarsRandomSpawnGrid();

        //Always goingToRespawn is true. sectionFloor is -1.
        if (sectionFloor != -1)
        {
            foreach( int floor in AvatarSpawnEachFloor.Keys)
            {
                if (sectionFloor <= floor)
                {
                    goingToRespawn = false;
                    break;
                }
            }
        }

        if(goingToRespawn)
        {
            MakeRandomSpawn();
        }
    }

    private bool bSkipSectionType(SectionInstanceData _insData)
    {
        if(_insData.sectionType == (byte)ENUM_SECTION_TYPE.Special)
        {
            if(_insData.sectionSubType == (byte)ENUM_SECTION_SUBTYPE_SPECIAL.PARKINGLOT)
            {
                return false;
            }
        }

        if (_insData.sectionType == (byte)ENUM_SECTION_TYPE.EmptyRoom)
        {
            return false;
        }

        return true;
    }

    public void SetAvatarsRandomSpawnGrid()
    {
        AvatarGridPoolDic.Clear();
        AvatarSpawnEachFloor.Clear();
        //make random pool(AvatarSpawnPoolDic)
        foreach (var item in WorldMgr.GetSectionManager().sectionDic)
        {
            SectionInstanceData _insData = item.Value.SectionScript.sectionInstanceData;

            if(bSkipSectionType(_insData))
            {
                for(int i=0; i<_insData.CoveredIndizes.Length; ++i)
                {
                    if(AvatarGridPoolDic.ContainsKey(_insData.CoveredIndizes[i]) == false)
                    {
                        //Debug.Log("    %%%%%% Pool " + _insData.CoveredIndizes[i].X+", "+ _insData.CoveredIndizes[i].Y);
                        AvatarGridPoolDic.Add(_insData.CoveredIndizes[i], AVATAR_TYPE.AVATAR_NONE);

                        int floor = _insData.CoveredIndizes[i].Y;
                        if (AvatarSpawnEachFloor.ContainsKey(floor) == false)
                        {
                            AvatarSpawnEachFloor.Add(floor, new List<AVATAR_TYPE>());
                        }
                    }
                }
            }
        }
    }

    private void MakeRandomSpawn()
    {
        List<int> _floorList = new List<int>(AvatarSpawnEachFloor.Keys);

        foreach (int floor in _floorList)
        {
            AvatarSpawnEachFloor[floor].Clear();
        }


        //All Avatars Make Spawn Position
        for (int i = 0; i < CDefines.VIEW_AVATAR_TYPE.Length; ++i)
        {
            AVATAR_TYPE avatarType = CDefines.VIEW_AVATAR_TYPE[i];
            if (AvatarControllerDic[avatarType].AIStateMachine.GetCurrentState() == AIStateMachine_Training.Instance()
                || AvatarControllerDic[avatarType].AIStateMachine.GetCurrentState() == AIStateMachine_Wait.Instance()
                ) continue;
            MakeRandomSpawnOfEachAvatar(avatarType);
        }

        //Then All Avatars set Spawn.
        //Then make Target position in patrol state
        for (int i = 0; i < CDefines.VIEW_AVATAR_TYPE.Length; ++i)
        {
            AVATAR_TYPE avatarType = CDefines.VIEW_AVATAR_TYPE[i];
            if (AvatarControllerDic[avatarType].AIStateMachine.GetCurrentState() == AIStateMachine_Training.Instance()
                || AvatarControllerDic[avatarType].AIStateMachine.GetCurrentState() == AIStateMachine_Wait.Instance()
                )
            {
                GetManagementAvatarObj(avatarType).SetActive(true);
            }
            else
            {
                GridIndex _myGrid = WorldMgr.AvatarMgr.GetGridIndexInGridPoolDicByAvatar(avatarType);
                if (_myGrid != null)
                {
                    AvatarControllerDic[avatarType].InitAIStateMachine(_myGrid);
                }
            }
        }

    }

    public GridIndex GetGridIndexInGridPoolDicByAvatar(AVATAR_TYPE aType)
    {
        foreach (KeyValuePair<GridIndex, AVATAR_TYPE> item in AvatarGridPoolDic)
        {
            if(item.Value == aType)
            {
                return item.Key;
            }
        }

        return null;
    }

    public List<GridIndex> GetAvatarGridPoolList()
    {
        List<GridIndex> keyList = new List<GridIndex>(AvatarGridPoolDic.Keys);

        foreach (KeyValuePair<GridIndex, AVATAR_TYPE> item in AvatarGridPoolDic)
        {
            if(item.Value != AVATAR_TYPE.AVATAR_NONE)
            {
                keyList.Remove(item.Key);
            }
        }

        return keyList;
    }

    public void MakeRandomSpawnOfEachAvatar(AVATAR_TYPE avatarType)
    {
        List<GridIndex> keyList = new List<GridIndex>(GetAvatarGridPoolList());

        RefreshAvatarGridPoolDic(avatarType);

        GridIndex randomKey = SpawnByRule(avatarType, keyList, true);
        AvatarGridPoolDic[randomKey] = avatarType;

        SetAvatarSpawnGridDic(avatarType, randomKey);
        SetAvatarCurrentGridDic(avatarType, randomKey);
    }

    private int GetTargetFloorCount(AVATAR_TYPE aType, int floor)
    {
        int retCount = 0;

        for (int i = 0; i < CDefines.VIEW_AVATAR_TYPE.Length; ++i)
        {
            AVATAR_TYPE _avatarType = CDefines.VIEW_AVATAR_TYPE[i];
            if (aType == _avatarType) continue;
            //if (_avatarType > aType) break;
            CCharacterController_Management _cntlr = GetManagementAvatarController(_avatarType);
            if (_cntlr != null)
            {
                if (_cntlr.GetTargetFloor() == floor)
                {
                    //CDebug.Log($"    @@@@@@ GetTargetFloorCount() me = {_avatarType}");
                    retCount++;
                }
            }
        }

        //CDebug.Log($"    @@@@@@ GetTargetFloorCount() me = {aType}, floor = {floor}, count = {retCount}");
        return retCount;
    }

    private List<GridIndex> GetGridIndexPool(AVATAR_TYPE me, List<GridIndex> list, int targetFloor = -1)
    {
        foreach (KeyValuePair<int, List<AVATAR_TYPE>> item in AvatarSpawnEachFloor)
        {
            //Remove floor what 2 avatars spawned in pool
            if(item.Value.Count > 1)
            {
                for (int i = (list.Count - 1); i >= 0; i--)
                {
                    if(targetFloor > -1)
                    {
                        if (list[i].Y != targetFloor)
                        {
                            list.RemoveAt(i);
                        }
                    }
                    else
                    {
                        if (list[i].Y == item.Key)
                        {
                            list.RemoveAt(i);
                        }
                    }
                }
            }
        }

        return list;
    }

    private List<GridIndex> GetTargetGridIndexPool(AVATAR_TYPE me, List<GridIndex> list, int targetFloor = -1)
    {
        foreach(int floor in AvatarSpawnEachFloor.Keys)
        {
            int avatarTargetFloorCnt = GetTargetFloorCount(me, floor);
            //CDebug.Log($"      ##### GetGridIndexPool {floor}'s spawn cnt = {cnt} , avatarTargetFloorCnt = {avatarTargetFloorCnt}");
            if (avatarTargetFloorCnt >= 2)
            {
                //CDebug.Log($"      ##### GetGridIndexPool remove floor = {floor}");
                for (int i = (list.Count - 1); i >= 0; i--)
                {
                    if (list[i].Y == floor)
                    {
                        //CDebug.Log($"      ##### GetGridIndexPool remove floor = {floor}");
                        list.RemoveAt(i);
                    }
                }
            }
        }

        return list;
    }


    private GridIndex SpawnByRule(AVATAR_TYPE aType, List<GridIndex> list, bool isSpawn = false)
    {
        int spawnIdx = 0;
        List<GridIndex> _tmpList = new List<GridIndex>(list);

        if (isSpawn)
        {
            _tmpList = GetGridIndexPool(aType, _tmpList);
        }
        else
        {
            _tmpList = GetTargetGridIndexPool(aType, _tmpList);
        }

        if (_tmpList.Count > 0)
        {
            spawnIdx = UnityEngine.Random.Range(0, _tmpList.Count);
//#if UNITY_EDITOR
//            if (isSpawn)
//            {
//                //AVATAR_TYPE avatarType = CDefines.VIEW_AVATAR_TYPE[(int)(aType - 1)];
//                if (aType == AVATAR_TYPE.AVATAR_JISOO)
//                {
//                    spawnIdx = 1;// 7;
//                }
//                else if (aType == AVATAR_TYPE.AVATAR_JENNIE)
//                {
//                    spawnIdx = 6;// 0;
//                }
//                else if (aType == AVATAR_TYPE.AVATAR_LISA)
//                {
//                    spawnIdx = 11;
//                }
//                else if (aType == AVATAR_TYPE.AVATAR_ROSE)
//                {
//                    spawnIdx = 12;
//                }
//            }
//            else
//            {
//                if (aType == AVATAR_TYPE.AVATAR_JISOO)
//                {
//                    spawnIdx = 0;// 7;
//                }
//                else if (aType == AVATAR_TYPE.AVATAR_JENNIE)
//                {
//                    spawnIdx = 1;// 0;
//                }
//                else if (aType == AVATAR_TYPE.AVATAR_LISA)
//                {
//                    //spawnIdx = 11;
//                }
//                else if (aType == AVATAR_TYPE.AVATAR_ROSE)
//                {
//                    //spawnIdx = 12;
//                }

//            }
//#endif
        }
        else
        {
            _tmpList = new List<GridIndex>(list);
            _tmpList = GetGridIndexPool(aType, _tmpList, AvatarControllerDic[aType].GetCurrentFloor());
            spawnIdx = UnityEngine.Random.Range(0, _tmpList.Count);
        }


        int floor = _tmpList[spawnIdx].Y;

        if(isSpawn)
        {
            AddAvatarToEachFloorDic(aType, floor);
        }

        return _tmpList[spawnIdx];
    }

    public void AddAvatarToEachFloorDic(AVATAR_TYPE aType, int floor)
    {
        List<AVATAR_TYPE> _list = AvatarSpawnEachFloor[floor];

        //Just return avatar's previous floor same with next floor
        for (int i = 0; i < _list.Count; ++i)
        {
            if (aType == _list[i])
            {
                CDebug.Log($"        ****** AddAvatarToEachFloorDic() {aType} already exist in {floor}");
                return;
            }
        }

        AvatarSpawnEachFloor[floor].Add(aType);
    }

    public void RemoveAvatarAtEachFloorDic(AVATAR_TYPE aType, int floor)
    {
        if (AvatarSpawnEachFloor[floor].Contains(aType))
        {
            AvatarSpawnEachFloor[floor].Remove(aType);
        }
    }

    public Vector3 GetAvatarPositionByGrid(GridIndex grid)
    {
        Vector3 _pos = WorldMgr.GetGridManager().GetWorldPositionFromGridIndex(grid);
        _pos.x += WorldMgr.GridElementWidth / 2;
        _pos.y += 0.1f;
        _pos.z = 0.7f;

        return _pos;
    }


    public GridIndex GetAvatarRandomTargetGrid(AVATAR_TYPE avatarType)
    {
        List<GridIndex> _list = new List<GridIndex>();
        foreach(var item in AvatarGridPoolDic)
        {
            if(item.Value == AVATAR_TYPE.AVATAR_NONE)
            {
                _list.Add(item.Key);
            }
        }

        //init AvatarGridPoolDic value by key
        RefreshAvatarGridPoolDic(avatarType);

        //int randomIdx = Random.Range(0, _list.Count);
        //#if UNITY_EDITOR
        //        if (avatarType == AVATAR_TYPE.AVATAR_JISOO)
        //        {
        //            randomIdx = 0; //0F
        //        }
        //        else if(avatarType == AVATAR_TYPE.AVATAR_JENNIE)
        //        {
        //            randomIdx = 4; //1F
        //            //randomIdx = 1; //0F
        //        }
        //#endif
        //GridIndex randomKey = _list[randomIdx];

        GridIndex randomKey = SpawnByRule(avatarType, _list);
        AvatarGridPoolDic[randomKey] = avatarType;

        //SetAvatarCurrentGridDic(avatarType, randomKey);

        return randomKey;
    }

    private void RefreshAvatarGridPoolDic(AVATAR_TYPE avatarType)
    {
        //init AvatarGridPoolDic value by key
        GridIndex key = GetKeyByValueInAvatarGridPoolDic(avatarType);
        if (key != null)
        {
            AvatarGridPoolDic[key] = AVATAR_TYPE.AVATAR_NONE;
        }
    }



    private GridIndex GetKeyByValueInAvatarGridPoolDic(AVATAR_TYPE avatarType)
    {
        foreach (var item in AvatarGridPoolDic)
        {
            if(item.Value == avatarType)
            {
                return item.Key;
            }
        }

        return null;
    }

    public CCharacterController_Management GetAvatarInSameFloor(AVATAR_TYPE aType, int floor)
    {
        foreach(KeyValuePair<AVATAR_TYPE, CCharacterController_Management> cntlr in AvatarControllerDic)
        {
            if(cntlr.Key != aType)
            {
                if(cntlr.Value.GetCurrentFloor() == floor)
                {
                    return cntlr.Value;
                }
            }
        }

        return null;
    }

    public CCharacterController_Management GetAvatarInSameFloorByState(AVATAR_TYPE aType, int floor, bool bCheckExcute = false)
    {
        CCharacterController_Management _avatar = GetAvatarInSameFloor(aType, floor);

        if(_avatar != null)
        {
            if(_avatar.GetTargetFloor() != floor)
            {
                if (bCheckExcute)
                {
                    if (
                        _avatar.GetCurrentAISMState() == AIStateMachine_GotoInnerStartEV.Instance()
                        || _avatar.GetCurrentAISMState() == AIStateMachine_GotoFrontStartEV.Instance()
                        || _avatar.GetCurrentAISMState() == AIStateMachine_PreWaitFrontStartEV.Instance()
                        || _avatar.GetCurrentAISMState() == AIStateMachine_WaitFrontStartEV.Instance()
                        )
                    {
                        return _avatar;
                    }
                }
                else
                {
                    if (_avatar.GetCurrentAISMState() == AIStateMachine_GotoFrontStartEV.Instance()
                        || _avatar.GetCurrentAISMState() == AIStateMachine_PreWaitFrontStartEV.Instance()
                        || _avatar.GetCurrentAISMState() == AIStateMachine_WaitFrontStartEV.Instance()
                        )
                    {
                        return _avatar;
                    }
                }

            }
        }

        return null;
    }

    public CCharacterController_Management GetAvatarInSameFloorByCheckerState(AVATAR_TYPE checker, int floor, CState<CCharacterController_Management> state, bool bCheckExcute = false)
    {
        CCharacterController_Management _avatarInSameFloor = GetAvatarInSameFloor(checker, floor);

        if(_avatarInSameFloor != null)
        {
            if (_avatarInSameFloor.GetTargetFloor() != floor)
            {
                if (state == AIStateMachine_GotoFrontStartEV.Instance())
                {
                    if (bCheckExcute)
                    {
                        if (
                               _avatarInSameFloor.GetCurrentAISMState() == AIStateMachine_GotoInnerStartEV.Instance()
                            || _avatarInSameFloor.GetCurrentAISMState() == AIStateMachine_GotoFrontStartEV.Instance()
                            || _avatarInSameFloor.GetCurrentAISMState() == AIStateMachine_PreWaitFrontStartEV.Instance()
                            || _avatarInSameFloor.GetCurrentAISMState() == AIStateMachine_WaitFrontStartEV.Instance()
                            )
                        {
                            return _avatarInSameFloor;
                        }
                    }
                    else
                    {
                        if ( 
                               _avatarInSameFloor.GetCurrentAISMState() == AIStateMachine_GotoFrontStartEV.Instance()
                            || _avatarInSameFloor.GetCurrentAISMState() == AIStateMachine_PreWaitFrontStartEV.Instance()
                            || _avatarInSameFloor.GetCurrentAISMState() == AIStateMachine_WaitFrontStartEV.Instance()
                            )
                        {
                            return _avatarInSameFloor;
                        }
                    }
                }
                else if(state == AIStateMachine_GotoInnerStartEV.Instance())
                {
                    if(
                           _avatarInSameFloor.GetCurrentAISMState() == AIStateMachine_GotoInnerStartEV.Instance()
                        || _avatarInSameFloor.GetCurrentAISMState() == AIStateMachine_InnerStartEV.Instance()
                        )
                    {
                        return _avatarInSameFloor;
                    }
                }
                else if(state == AIStateMachine_InnerStartEV.Instance())
                {
                    if (
                           _avatarInSameFloor.GetCurrentAISMState() == AIStateMachine_GotoFrontStartEV.Instance()
                        || _avatarInSameFloor.GetCurrentAISMState() == AIStateMachine_PreWaitFrontStartEV.Instance()
                        || _avatarInSameFloor.GetCurrentAISMState() == AIStateMachine_WaitFrontStartEV.Instance()
                        || _avatarInSameFloor.GetCurrentAISMState() == AIStateMachine_GotoInnerStartEV.Instance()
                        )
                    {
                        return _avatarInSameFloor;
                    }
                }
            }
        }

        return null;
    }

    private void SetTrainingSectionScript(AVATAR_TYPE avatarType, SectionScript scr)
    {
        if(AvatarTrainingSectionScript.ContainsKey(avatarType) == false)
        {
            AvatarTrainingSectionScript.Add(avatarType, scr);
        }
        else
        {
            AvatarTrainingSectionScript[avatarType] = scr;
        }
    }

    public SectionScript GetTrainingSectionScript(AVATAR_TYPE avatarType)
    {
        if (AvatarTrainingSectionScript.ContainsKey(avatarType))
        {
            return AvatarTrainingSectionScript[avatarType];
        }

        return null;
    }

    public void AddToAvatarBTFilePool()
    {
        List<string> _list = ManagementDataManager.Instance.GetAllAvatarActivityBTPath();

        for(int i=0; i<_list.Count; ++i)
        {
            string namePath = _list[i];

            if (AvatarBTFilePool.ContainsKey(namePath) == false)
            {
                var resData = CResourceManager.Instance.GetResourceData(namePath);
                ExternalBehavior _eb = resData.LoadExternalBehavior(WorldMgr.gameObject);

                AvatarBTFilePool.Add(namePath, _eb);
            }
        }
    }

    private ExternalBehavior GetAvatarBT(string btName)
    {
        if(AvatarBTFilePool.ContainsKey(btName))
        {
            return AvatarBTFilePool[btName];
        }

        return null;
    }

    private void InitAvatarBT(AVATAR_TYPE avatarType, GameObject obj)
    {
        BehaviorTree _bt = obj.AddComponent<BehaviorTree>();
        AvatarBTDic.Add(avatarType, _bt);
        SetActiveAvatarBT(avatarType, false);
    }

    public void SetAvatarBT(AVATAR_TYPE avatarType, string btName, SectionScript scr)
    {
        ExternalBehavior _bt = GetAvatarBT(btName);
        if(_bt == null)
        {
            CDebug.LogError($"SetAvatarBT {avatarType}. {btName} is not contain BT File Pool");
            return;
        }

        AvatarControllerDic[avatarType].IsSpawned = false;

        if (AvatarBTDic[avatarType].ExternalBehavior != _bt)
        {
            SetTrainingSectionScript(avatarType, scr);

            AvatarBTDic[avatarType].ExternalBehavior = _bt;
        }

        if (AvatarBTDic[avatarType].enabled == false)
        {
            SetActiveAvatarBT(avatarType, true);
        }
    }

    public void SetActiveAvatarBT(AVATAR_TYPE avatarType, bool bActive)
    {
        if (avatarType == AVATAR_TYPE.AVATAR_NONE || AvatarBTDic.ContainsKey(avatarType) == false) return;

        AvatarBTDic[avatarType].enabled = bActive;
    }
    //    private void LoadAvatarBT(AVATAR_TYPE avatarType, GameObject obj)
    //    {
    //#if UNITY_EDITOR
    //        //if (mType != AVATAR_TYPE.MEMBER_JENNIE) return;
    //#endif

    //        string resPath = string.Empty;
    //        BehaviorTree _bt = obj.AddComponent<BehaviorTree>();
    //        AvatarBTDic.Add(avatarType, _bt);

    //        switch (avatarType)
    //        {
    //            case AVATAR_TYPE.AVATAR_JISOO:
    //                resPath = JISOO_BT_PATH;
    //                break;
    //            case AVATAR_TYPE.AVATAR_JENNIE:
    //                resPath = JENNIE_BT_PATH;
    //                break;
    //            case AVATAR_TYPE.AVATAR_LISA:
    //                resPath = LISA_BT_PATH;
    //                break;
    //            case AVATAR_TYPE.AVATAR_ROSE:
    //                resPath = ROSE_BT_PATH;
    //                break;
    //        }

    //        var resData = CResourceManager.Instance.GetResourceData(resPath);
    //        AvatarBTDic[avatarType].ExternalBehavior = resData.LoadExternalBehavior(obj);
    //    }

    //Object, Effect both
    //public void SetHoldableObject(AVATAR_TYPE avatarType, GameObject avatarObj)
    //{
    //    MotionData _data = CAvatarInfoDataManager.GetMotionDataByAvatarType(avatarType);
    //    if (_data != null)
    //    {
    //        if (AvatarHoldObjDic.ContainsKey(avatarType) == false)
    //        {
    //            AvatarHoldObjDic.Add(avatarType, new List<CBTHoldObjectInfo>());
    //        }

    //        for (int j = 0; j < _data.Object_ID.Length; ++j)
    //        {
    //            if (_data.Object_ID[j] != 0)
    //            {
    //                HoldObjectData _holdObjData = CPlayerDataManager.Instance.GetHoldObjectDataByID(_data.Object_ID[j]);

    //                if (_holdObjData != null)
    //                {
    //                    AvatarHoldObjDic[avatarType].Add(new CBTHoldObjectInfo());

    //                    int _idx = AvatarHoldObjDic[avatarType].Count - 1;
    //                    AvatarHoldObjDic[avatarType][_idx].SetData
    //                    (
    //                        _holdObjData, avatarObj,
    //                        AvatarAnimEvtDic[avatarType].DummyHand_Left.transform, AvatarAnimEvtDic[avatarType].DummyHand_Right.transform,
    //                        _data.AnimName
    //                    );
    //                }
    //            }
    //        }
    //    }
    //}

    //public CBTHoldObjectInfo GetHoldObjectInfoByAnimName(AVATAR_TYPE avatarType, string animName)
    //{
    //    if (AvatarHoldObjDic.ContainsKey(avatarType) == false)
    //    {
    //        CDebug.Log("GetHoldObjectInfoByAnimName AvatarHoldObjDic has not contain " + avatarType);
    //        return null;
    //    }

    //    List<CBTHoldObjectInfo> _list = AvatarHoldObjDic[avatarType];

    //    for (int i = 0; i < _list.Count; ++i)
    //    {
    //        if (animName.Equals(_list[i].animationName))
    //        {
    //            return _list[i];
    //        }
    //    }

    //    return null;
    //}

    //public CBTHoldObjectInfo GetHoldObjectByObjDataID(AVATAR_TYPE avatarType, long ID)
    //{
    //    if (AvatarHoldObjDic.ContainsKey(avatarType) == false)
    //    {
    //        CDebug.Log("GetHoldObjectByObjDataID AvatarHoldObjDic has not contain " + avatarType);
    //        return null;
    //    }

    //    List<CBTHoldObjectInfo> _list = AvatarHoldObjDic[avatarType];

    //    for (int i = 0; i < _list.Count; ++i)
    //    {
    //        if (ID == _list[i].ObjectDataID)
    //        {
    //            return _list[i];
    //        }
    //    }

    //    return null;
    //}

    //public void SpawnAllAvatars()
    //{
    //    for (AVATAR_TYPE mType = AVATAR_TYPE.AVATAR_JISOO; mType < AVATAR_TYPE.AVATAR_BLACKPINK; ++mType)
    //    {
    //        SpawnAvatar(mType);
    //    }
    //}

    //public void SpawnAvatar(AVATAR_TYPE mType)
    //{
    //    AvatarControllerDic[mType].SetChangeMotionState(MOTION_STATE.SPAWN_MANAGEMENT);
    //}

    public void SetPrevAvatarPosition(AVATAR_TYPE avatarType, Vector3 pos)
    {
        if (PrevAvatarPos.ContainsKey(avatarType))
        {
            PrevAvatarPos[avatarType] = pos;
        }
        else
        {
            PrevAvatarPos.Add(avatarType, pos);
        }
    }

  
    //For using only AI
    public void SetActiveAllAvatars(bool bActive)
    {
        for (int i = 0; i < CDefines.VIEW_AVATAR_TYPE.Length; ++i)
        {
            AVATAR_TYPE _avatarType = CDefines.VIEW_AVATAR_TYPE[i];
            TouchedEventManager.SetActiveTouchEvtUIObject(_avatarType, TOUCHEVENT_SCENE_TYPE.MANAGEMENT, bActive );
            SetActiveAvatarEventIcon(_avatarType, bActive );
            GetManagementAvatarController(_avatarType).SetChangeAIStateMachine(AIStateMachine_Wait.Instance());
            GetManagementAvatarObj(_avatarType).SetActive(bActive);
            SetActiveAvatarInteractionIcon( _avatarType, bActive );
            SetActiveAvatarMailQuestIcon( _avatarType, bActive );
        }
    }

    public int GetAvatarConditionLevel(AVATAR_TYPE avatarType)
    {
        return AvatarInfoDic[avatarType].GetConditionLevel();
    }

    public CAvatar GetManagementAvatarInfo(AVATAR_TYPE avatarType)
    {
        return AvatarInfoDic[avatarType];
    }

    public GameObject GetManagementAvatarObj(AVATAR_TYPE avatarType)
    {
        if (AvatarObjDic.TryGetValue(avatarType, out GameObject avatarObj))
        {
            return avatarObj;
        }
        CDebug.LogError ($"GetManagementAvatarObj : {avatarType} is invalid.");
        return null;
    }

	public GameObject GetManagementAvatarUIDummyObj(AVATAR_TYPE avatarType)
	{
		return AvatarUIDummyBoneDic[avatarType];
	}

    public CCharacterController_Management GetManagementAvatarController(AVATAR_TYPE avatarType)
    {
        return AvatarControllerDic[avatarType];
    }

    public CEvent GetManagementAvatarEvent(AVATAR_TYPE avatarType)
    {
        return AvatarEvent[avatarType];
    }

  //  public void PreSetTakingAvatarInOtherPlace(AVATAR_TYPE avatarType, Transform parent = null)
  //  {
  //      AvatarControllerDic[avatarType].PauseWayPoint();
  //      AvatarControllerDic[avatarType].SetActiveBehaviorTree(false);
  //      AvatarControllerDic[avatarType].SetActiveNavMeshAgent(false);
  //      if(parent != null)
  //      {
  //          SetPrevParent(avatarType, parent);
  //      }

  //      SetPrevAvatarPosition(avatarType, AvatarObjDic[avatarType].transform.localPosition);

		//if(
  //          AvatarControllerDic[avatarType].GetCurrentAISMState() == AIStateMachine_Interaction_Wait.Instance()
  //          || AvatarControllerDic[avatarType].GetCurrentAISMState() == AIStateMachine_Interaction_Play.Instance()
  //          )
		//{
  //          CCharacterController_Management _coupleAvatar = AvatarControllerDic[avatarType].GetCoupleAvatarInteraction();
  //          if(_coupleAvatar != null)
  //          {
  //              _coupleAvatar.SetReSpawnAvatar();
  //          }
  //      }

		//AvatarControllerDic[avatarType].IsInteractionPlayEnterAvatarEquip = true;
  //      AvatarControllerDic[avatarType].SetChangeAIStateMachine(AIStateMachine_Wait.Instance());
  //  }

	public void PreSetAiInteraction_play(AVATAR_TYPE avatarType)
	{
		if(AvatarControllerDic[avatarType].GetPrevAISMState() == AIStateMachine_Interaction_Play.Instance())
		{
            SetExitAvatarInteraction( avatarType );
            //AvatarControllerDic[avatarType].IsInteractionPlayEnterAvatarEquip = false;
            //ExitAvatarInteraction(avatarType, AvatarControllerDic[avatarType].GetStateBeforeAvatarInteraction());
        }
	}

    public void SetExitAvatarInteraction(AVATAR_TYPE avatarType)
    {
        AvatarControllerDic[avatarType].IsInteractionPlayEnterAvatarEquip = false;
        ExitAvatarInteraction( avatarType, AvatarControllerDic[avatarType].GetStateBeforeAvatarInteraction() );
    }

    //private void SetPrevParent(AVATAR_TYPE avatarType, Transform parent)
    //{
    //    if (PrevAvatarParent.ContainsKey(avatarType) == false)
    //    {
    //        PrevAvatarParent.Add(avatarType, parent);
    //    }
    //    else
    //    {
    //        PrevAvatarParent[avatarType] = parent;
    //    }
    //}

	public AVATAR_TYPE tempAvatarType;

    public void ReplaceAvatar(AVATAR_TYPE avatarType)
    {
        CAvatar _avatar = CPlayer.GetAvatar(avatarType);

        SetPrevPosition(avatarType);

        if (_avatar.status > AVATAR_ACTIVITY_STATE.REST 
            && _avatar.status <= AVATAR_ACTIVITY_STATE.TRAINING_ATTRACTIVENESS)
        {
            AvatarObjDic[avatarType].transform.localRotation = Quaternion.Euler(0, 0, 0);
            AvatarObjDic[avatarType].transform.localPosition = Vector3.zero;
        }
        else
        {
            AvatarControllerDic[avatarType].SetReSpawnAvatar();
            AvatarControllerDic[avatarType].SetActiveBehaviorTree(true);
        }
    }

	public void DelayResuleWayPoint()
	{
		AvatarControllerDic[tempAvatarType].ResumeWayPoint();
	}

    public void SetPrevPosition(AVATAR_TYPE avatarType)
    {        
        //AvatarControllerDic[avatarType].SetActiveNavMeshAgent(false);
        if (PrevAvatarParent.ContainsKey(avatarType) == false)
        {
            AvatarObjDic[avatarType].transform.parent = OrgAvatarParent[avatarType].transform;
        }
        else
        {
            AvatarObjDic[avatarType].transform.parent = PrevAvatarParent[avatarType];
        }

        AvatarObjDic[avatarType].transform.localScale = Vector3.one;
    }

    ////트레이닝 아바타 set
    //public void SetTrainingAvatar(AVATAR_TYPE mType, string member_btid, SectionScript sectionScript, Transform parent)
    //{
    //    GameObject avatarObj = GetManagementAvatarObj(mType);
    //    avatarObj.transform.parent = parent;

    //    string _btName = member_btid;
    //    SetAvatarBT(mType, _btName, sectionScript);

    //    AvatarControllerDic[mType].SetAvatarTraining(
    //        sectionScript.sectionInstanceData.sectionType, 
    //        sectionScript.sectionInstanceData.sectionSubType);
    //}

    //액티비티 아바타 set
    public void SetActivityAvatar(AVATAR_TYPE mType, string member_btid, SectionScript sectionScript, Transform parent)
    {
        GameObject avatarObj = GetManagementAvatarObj(mType);
        if (avatarObj == false)
        {
            CDebug.LogError ($"SetActivityAvatar : {mType} is invalid.");
            return;
        }
        avatarObj.transform.parent = parent;

        string _btName = member_btid;
        SetAvatarBT(mType, _btName, sectionScript);
        //ManagementAPI.ManagementActivityData activitydata = 
        //    ManagementServerDataManager.Instance.GetActivityDataBySectionID(sectionScript.sectionInstanceData.sectionId_ServerUniqueID);
        ACTIVITY_TYPE activityType = ManagementDataManager.Instance.GetActivityType(sectionScript.sectionStatusData.GetSectionLevelTableData().Activity_Group_ID);
        AvatarControllerDic[mType].SetAvatarActivity(activityType, sectionScript.sectionInstanceData.sectionType, sectionScript.sectionInstanceData.sectionSubType);
    }

    public void SetExitTraining(AVATAR_TYPE avatarType)
    {
        CCharacterController_Management _cntlr = GetManagementAvatarController(avatarType);
        _cntlr.InitializeCurrentActivitySectionTypeInfo();

        SetActiveAvatarBT(avatarType, false);
        SetPrevPosition(avatarType);
        AvatarControllerDic[avatarType].SetReSpawnAvatar();
    }

    public void SetAvatarEventIcon(AVATAR_TYPE avatarType, CEvent mEvent, SingleAssignmentDisposable disposer)
    {
        Vector3 pos = AvatarObjDic[avatarType].transform.localPosition;

		FollowObjectPositionFor2D fop = AvatarEventIconObj[avatarType].transform.gameObject.GetComponent<FollowObjectPositionFor2D>();
        if(fop == null)
        {
            fop = AvatarEventIconObj[avatarType].transform.gameObject.AddComponent<FollowObjectPositionFor2D>();
        }
        var canvasRect = WorldMgr.GetCanvasManager().GetComponent<RectTransform>();
        fop.Init(canvasRect, AvatarUIDummyBoneDic[avatarType].transform);

        AvatarEventIconObj[avatarType].transform.localPosition = pos;

        AvatarEventIconObj[avatarType].SetData_Avatar(mEvent, avatarType, disposer);

        AvatarEventIconObj[avatarType].gameObject.SetActive(true);
    }

    public Transform GetAvatarEventIcon(AVATAR_TYPE avatarType)
    {
        if (avatarType == AVATAR_TYPE.AVATAR_NONE || AvatarEventIconObj.ContainsKey(avatarType) == false) return null;

        return AvatarEventIconObj[avatarType].transform;
    }

    public void SetActiveAvatarEventIcon(AVATAR_TYPE avatarType, bool bActive)
    {
        if(avatarType == AVATAR_TYPE.AVATAR_NONE)
        {
            CDebug.LogError("AVATAR_TYPE AVATAR_NONE이면 안돼요~~");
            return;
        }
        AvatarEventIconObj[avatarType].gameObject.SetActive(bActive);
    }


    public void StartAvatarInteraction(AVATAR_TYPE Me, AVATAR_TYPE other, AvatarInteractionData interactionData)
    {
        long subDelayTime = interactionData.Sub_Delay_Time / 100;
        //Notice to avatar manager
        //then start both interaction event
        long myFinishTime = (subDelayTime + interactionData.FinishTime);
        ShowAvatarInteractionBubbleMsg(Me, myFinishTime, subDelayTime);

        //Interaction Sequence
        int diposerIdx = (int)Me - 1;
        InitInteractionDisposer(diposerIdx);

        InteractionDisposer[diposerIdx].Disposable = Observable.Timer(TimeSpan.FromSeconds(subDelayTime))
        .Subscribe(_ =>
        {
            ShowAvatarInteractionBubbleMsg(other, interactionData.FinishTime);
            InteractionDisposer[diposerIdx].Dispose();
        }).AddTo(WorldMgr);
    }

    /// <summary>
    /// set ui of each avatar in interaction State
    /// interactionData is set by group rate
    /// </summary>
    /// <param name="aType"></param>
    /// <param name="other"></param>
    /// <param name="interacData"></param>
    /// <param name="amI_Main"></param>
    /// <param name="disposer"></param>
    public void SetAvatarInteractionIcon(AVATAR_TYPE aType, AVATAR_TYPE other, AvatarInteractionData interacData, bool amI_Main, SingleAssignmentDisposable disposer = null)
    {
        Vector3 _pos = AvatarObjDic[aType].transform.localPosition;

        FollowObjectPositionFor2D fop = AvatarInteractionIconObjDic[aType].transform.gameObject.GetComponent<FollowObjectPositionFor2D>();
        if (fop == null)
        {
            fop = AvatarInteractionIconObjDic[aType].transform.gameObject.AddComponent<FollowObjectPositionFor2D>();
        }
        var canvasRect = WorldMgr.GetCanvasManager().GetComponent<RectTransform>();
        fop.Init(canvasRect, AvatarUIDummyBoneDic[aType].transform);

        AvatarInteractionIconObjDic[aType].transform.localPosition = _pos;

        AvatarInteractionIconObjDic[aType].SetData(aType, other, interacData, amI_Main, disposer);

        SetActiveAvatarInteractionIcon(aType, true);
    }

    public void ShowAvatarInteractionBubbleMsg(AVATAR_TYPE aType, long finishTime, long subDelayTime = 0)
    {
        string animParam = AvatarInteractionIconObjDic[aType].GetInteractionAnimParam();
        AvatarControllerDic[aType].SetChangeAIStateMachine(AIStateMachine_Interaction_Play.Instance(), animParam);
        AvatarInteractionIconObjDic[aType].SetActiveBubbleMessage(true);

        int avatarDisposerIdx = (int)(aType - 1);
        InitDelayTimeDisposer(avatarDisposerIdx);

        if (subDelayTime > 0)
        {
            InitSubDelayTimeDisposer(avatarDisposerIdx);

            SubDelayTimerDisposer[avatarDisposerIdx].Disposable = Observable.Timer(System.TimeSpan.FromSeconds(subDelayTime))
            .Subscribe(_ =>
            {
                AvatarInteractionIconObjDic[aType].SetActiveBubbleMessage(false);
                SubDelayTimerDisposer[avatarDisposerIdx].Dispose();
            });
        }

        DelayTimerDisposer[avatarDisposerIdx].Disposable = Observable.Timer(System.TimeSpan.FromSeconds(finishTime))
        .Subscribe(_ =>
        {
            AvatarInteractionIconObjDic[aType].SetActiveBubbleMessage(false);
            ExitAvatarInteraction(aType, AvatarControllerDic[aType].GetStateBeforeAvatarInteraction());
            DelayTimerDisposer[avatarDisposerIdx].Dispose();
        });
    }

    public void SetActiveAvatarInteractionIcon(AVATAR_TYPE aType, bool bActive)
    {
        if (AvatarInteractionIconObjDic.ContainsKey(aType))
        {
            AvatarInteractionIconObjDic[aType].gameObject.SetActive(bActive);
        }
    }

    public ManagementAvatarInteractionUI GetAvatarInteractionUI(AVATAR_TYPE aType)
    {
        if (AvatarInteractionIconObjDic.ContainsKey( aType ))
        {
            return AvatarInteractionIconObjDic[aType];
        }

        return null;
    }

    public void ExitAvatarInteraction(AVATAR_TYPE aType, CState<CCharacterController_Management> where)
    {
        SetActiveAvatarInteractionIcon(aType, false);
        if (where != null)
        {
            AvatarControllerDic[aType].SetChangeAIStateMachine(where);
        }
    }

    public void HideInteractionAvatarIcon()
    {
        for (int i = 0; i < CDefines.VIEW_AVATAR_TYPE.Length; ++i)
        {
            AVATAR_TYPE avatarType = CDefines.VIEW_AVATAR_TYPE[i];
            AvatarControllerDic[avatarType].SetAITimeDispose();
            if (
                AvatarControllerDic[avatarType].GetCurrentAISMState() == AIStateMachine_Interaction_Wait.Instance()
                || AvatarControllerDic[avatarType].GetCurrentAISMState() == AIStateMachine_Interaction_Play.Instance()
                )
            {
                SetActiveAvatarInteractionIcon(avatarType, false);
            }

        }
    }

    public void StartCheckingEvent(AVATAR_TYPE avatarType)
    {
        SetActiveAvatarEventIcon(avatarType, false);
    }

    public void SetAllStopCheckingEvent()
    {
        for (int i = 0; i < DelayTimerDisposer.Length; ++i)
        {
            if(DelayTimerDisposer[i] != null)
            {
                DelayTimerDisposer[i].Dispose();
            }
        }
    }

    public void SetAvatarEventState(AVATAR_TYPE avatarType, EVENT_STATE state)
    {
        if (state == EVENT_STATE.NONE)
        {
            if(AvatarEventIconObj != null)
            {
                if(AvatarEventIconObj.ContainsKey(avatarType))
                {
                    if (AvatarEventIconObj[avatarType] != null)
                    {
                        AvatarEventIconObj[avatarType].gameObject.SetActive(false);
                    }
                }
            }
        }
        if (AvatarEventState != null)
        {
            if (AvatarEventState.ContainsKey(avatarType))
            {
                AvatarEventState[avatarType] = state;
            }
        }
    }

    public MotionData GetCurrentInteractionMotionInfo(AVATAR_TYPE avatarType)
    {
        if (AvatarInteractionIconObjDic.ContainsKey(avatarType))
        {
            return AvatarInteractionIconObjDic[avatarType].GetMotionData();
        }

        return null;
    }

    public long GetMotionObjectID(AVATAR_TYPE avatarType)
    {
        var mData = GetCurrentInteractionMotionInfo(avatarType);
        if (mData != null)
            return mData.RandObjectID;
        return 0;
    }

    public EVENT_STATE GetAvatarEventState(AVATAR_TYPE avatarType)
    {
        return AvatarEventState[avatarType];
    }

    /// <summary>
    /// Destroy Event when Cancel or Complete
    /// </summary>
    /// <param name="avatarType"></param>
    public void DestoryManagementAvatarEvent(AVATAR_TYPE avatarType)
    {
        SetAvatarEventState(avatarType, EVENT_STATE.NONE);
        if (AvatarEvent.ContainsKey(avatarType))
        {
            AvatarEvent[avatarType] = null;
        }
    }


    public CEvent GetEvent(AVATAR_TYPE avatarType)
    {
        CAvatar avatar = GetManagementAvatarInfo(avatarType);
        if (avatar.status > AVATAR_ACTIVITY_STATE.REST)
        {
            //트레이닝이나 다른 액티비티 중에는 이벤트를 받지 않는다
            return null;
        }

        CEvent avatarEvent = CEventManager.Instance.SelectEvent_Avatar(avatarType);

        return avatarEvent;
    }

    public void OccurEventProc(AVATAR_TYPE avatarType, CEvent avatarEvent)
    {
        int avatarDisposerIdx = (int)(avatarType - 1);
        InitDelayTimeDisposer(avatarDisposerIdx);

        //Event has been started but not Touch
        SetAvatarEventState(avatarType, EVENT_STATE.START);

        DelayTimerDisposer[avatarDisposerIdx].Disposable = Observable.Timer(System.TimeSpan.FromSeconds(avatarEvent.Event_Delay_Time))
        .Subscribe(_ =>
        {
            CEventManager.Instance.RequestEventCancel(avatarEvent);
            CancelEvent(avatarType);
            GetManagementAvatarController(avatarType).SetTargetGrid_InPatrol();
            DelayTimerDisposer[avatarDisposerIdx].Dispose();
        });

        if (AvatarEvent.ContainsKey(avatarType) == false)
        {
            AvatarEvent.Add(avatarType, avatarEvent);
        }
        else
        {
            AvatarEvent[avatarType] = avatarEvent;
        }

        //3D UI
        SetAvatarEventIcon(avatarType, avatarEvent, DelayTimerDisposer[avatarDisposerIdx]);
    }

    public void DisposeDelayTimerDisposer(AVATAR_TYPE aType)
    {
        int avatarDisposerIdx = (int)(aType - 1);
        if (DelayTimerDisposer[avatarDisposerIdx] != null)
        {
            DelayTimerDisposer[avatarDisposerIdx].Dispose();
        }
    }

    public void DisposeSubDelayTimerDisposer(AVATAR_TYPE aType)
    {
        int avatarDisposerIdx = (int)(aType - 1);
        if (SubDelayTimerDisposer[avatarDisposerIdx] != null)
        {
            SubDelayTimerDisposer[avatarDisposerIdx].Dispose();
        }
    }

    public void DisposeInteractionDisposer(AVATAR_TYPE aType)
    {
        int avatarDisposerIdx = (int)(aType - 1);
        if (InteractionDisposer[avatarDisposerIdx] != null)
        {
            InteractionDisposer[avatarDisposerIdx].Dispose();
        }
    }

    public void CancelEvent(AVATAR_TYPE avatarType)
    {
        //CEventManager.Instance.RequestEventCancel(avatarEvent);
        DestoryManagementAvatarEvent(avatarType);
        StartCheckingEvent(avatarType);
    }

    public bool IsAvatarTraining(AVATAR_TYPE avatarType)
    {
        foreach (KeyValuePair<long, SectionRef> item in WorldMgr.GetSectionManager().sectionDic)
        {
            long sectionID = item.Value.SectionScript.GetSectionGDID_TableData();
            //현재 이 프리펍 룸에서 트레이닝이 진행되고 있는가?
            //진행되고 있다면 어느 슬롯에 어느 avatar/npc가 진행하고 있는가
            List<ManagementAPI.ManagementActivityData> _list = ManagementServerDataManager.Instance.GetActivityDataList();
            for (int i = 0; i < _list.Count; ++i)
            {
                SectionStatusData ssd = ManagementSectionStatusManager.Instance.GetSectionStatusData(_list[i].section_idx);
                if (sectionID == ssd.sectionGDID_TableData)
                {
                    AVATAR_TYPE aType = (AVATAR_TYPE)_list[i].mid;
                    if (aType == avatarType)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }


    //Below Func will Change when Bt for Training is insert
    public bool IsAvatarInTrainingRoom(AVATAR_TYPE aType)
    {        
        if(AvatarObjDic[aType].transform.parent.name.Equals("0"))
        {
            return true;
        }

        return false;
    }


    #region TOUCH_EVENT
    public void TouchAvatar(AVATAR_TYPE aType)
    {
        //has been get Event
        if (GetAvatarEventState(aType) >= EVENT_STATE.START) return;

        if (AvatarControllerDic[aType].GetCurrentAISMState() == AIStateMachine_Touch.Instance()
            || AvatarControllerDic[aType].GetCurrentAISMState() == AIStateMachine_Interaction_Wait.Instance()
            || AvatarControllerDic[aType].GetCurrentAISMState() == AIStateMachine_Interaction_Play.Instance()
            )
        {
            return;
        }

        if(AvatarControllerDic[aType].transform.localPosition.z > 2)
        {
            return;
        }

        TouchedEventManager.TouchAvatar(aType, TOUCHEVENT_SCENE_TYPE.MANAGEMENT, AvatarObjDic[aType], AvatarControllerDic[aType]);
        //AvatarControllerDic[aType].SetAITargetAngle(180);
        //AvatarObjDic[aType].transform.localRotation = Quaternion.Euler(0, 180, 0);

        //CDebug.Log("         @@@@@ Touch Avatar ratation = " + AvatarObjDic[aType].transform.localRotation);
    }
    #endregion TOUCH_EVENT


    #region AI

    public void InitAvatarHavingComingAvatar(AVATAR_TYPE aType)
    {
        for (int i = 0; i < CDefines.VIEW_AVATAR_TYPE.Length; ++i)
        {
            AVATAR_TYPE avatarType = CDefines.VIEW_AVATAR_TYPE[i];

            CCharacterController_Management _comingAvatar = AvatarControllerDic[avatarType].GetComingAvatarInEV();
            if (_comingAvatar != null)
            {
                if(_comingAvatar.GetAvatarType() == aType)
                {
                    AvatarControllerDic[avatarType].SetComingAvatarInEV(null);
                }
            }
        }
    }


    public CCharacterController_Management OtherAvatarComeToThisFloor(AVATAR_TYPE aType, int floor)
    {
        for (int i = 0; i < CDefines.VIEW_AVATAR_TYPE.Length; ++i)
        {
            AVATAR_TYPE avatarType = CDefines.VIEW_AVATAR_TYPE[i];

            if (aType == avatarType) continue;

            //CDebug.Log($"     #### before check DoesAvatarGetInEVGoesToThisFloor {aType} startFloor:{AvatarControllerDic[aType].GetTargetFloor()}, ev Floor = {floor}");
            if (AvatarControllerDic[avatarType].GetTargetFloor() == floor)
            {
                CCharacterController_Management _comingAvatar = AvatarControllerDic[avatarType];
                //other avatar state is....below(it means coming)
                if (
                       _comingAvatar.GetCurrentAISMState() == AIStateMachine_GotoInnerStartEV.Instance()
                    || _comingAvatar.GetCurrentAISMState() == AIStateMachine_InnerStartEV.Instance()
                    || _comingAvatar.GetCurrentAISMState() == AIStateMachine_WaitEVDoorClosed.Instance()
                    || _comingAvatar.GetCurrentAISMState() == AIStateMachine_GotoInnerTargetEV.Instance()
                    || _comingAvatar.GetCurrentAISMState() == AIStateMachine_CheckTargetEVAllArrived.Instance()
                    || _comingAvatar.GetCurrentAISMState() == AIStateMachine_WaitTargetFloorEVDoorOpen.Instance()
                    )
                {
                    return _comingAvatar;
                }
            }
        }
        return null;
    }

    public bool HasComingAvatarArrivedAtStartFloor(AVATAR_TYPE aType)
    {
        CCharacterController_Management _comingAvatar = AvatarControllerDic[aType].GetComingAvatarInEV();
        if(_comingAvatar != null)
        {
            if(_comingAvatar.GetCurrentAISMState() == AIStateMachine_InnerTargetEV.Instance())
            {
                return true;
            }
        }

        return false;
    }

    public bool IsComingAvatarArrived(AVATAR_TYPE aType)
    {
        CCharacterController_Management _comingAvatar = AvatarControllerDic[aType].GetComingAvatarInEV();
        if (_comingAvatar != null)
        {
            if (_comingAvatar.GetCurrentAISMState() == AIStateMachine_InnerTargetEV.Instance())
            {
                return true;
            }
        }

        return false;
    }


    public bool IsArrivedAtTargetFloorEVAllRegisters(List<AVATAR_TYPE> passenger)
    {
        int arriveCnt = 0;
        for(int i=0; i< passenger.Count; ++i)
        {
            CCharacterController_Management _registCntlr = AvatarControllerDic[passenger[i]];
            if(_registCntlr != null)
            {
                if(
                    _registCntlr.GetCurrentAISMState() == AIStateMachine_CheckTargetEVAllArrived.Instance()
                    || _registCntlr.GetCurrentAISMState() == AIStateMachine_WaitTargetFloorEVDoorOpen.Instance()
                    || _registCntlr.GetCurrentAISMState() == AIStateMachine_InnerTargetEV.Instance()
                    )
                {
                    arriveCnt++;
                }
                //else
                //{
                //    CDebug.LogError($"   @@ IsArrivedAtTargetFloorEVAllRegisters {passenger[i]}'s state = {_registCntlr.GetCurrentAISMState()}");
                //}
            }
        }

        if (passenger.Count == arriveCnt)
        {
            return true;
        }

        return false;
    }


    public bool IsSomebodyAroundOfEV(AVATAR_TYPE aType, int floor)
    {
        for (int i = 0; i < CDefines.VIEW_AVATAR_TYPE.Length; ++i)
        {
            AVATAR_TYPE avatarType = CDefines.VIEW_AVATAR_TYPE[i];

            if (aType == avatarType) continue;

            CCharacterController_Management _cntlr = GetAvatarInSameFloor(aType, floor);

            //if (_cntlr.GetStartFloor() == floor)
            if(_cntlr != null)
            {
                if(   _cntlr.GetCurrentAISMState() == AIStateMachine_Interaction_Wait.Instance()
                   || _cntlr.GetCurrentAISMState() == AIStateMachine_Interaction_Play.Instance()
                    )
                {
                    return false;
                }

                if (   _cntlr.GetCurrentAISMState() == AIStateMachine_PreWaitFrontStartEV.Instance()
                    || _cntlr.GetCurrentAISMState() == AIStateMachine_WaitFrontStartEV.Instance()
                    || _cntlr.GetCurrentAISMState() == AIStateMachine_GotoInnerStartEV.Instance()
                    || _cntlr.GetCurrentAISMState() == AIStateMachine_InnerStartEV.Instance()
                    )
                {
                    return true;
                }

                if (_cntlr.GetCurrentAISMState() == AIStateMachine_GotoFrontStartEV.Instance())
                {
                    //check distance with ev
                    float _dist = _cntlr.GetNavMeshRemainDistance();
                    if(_dist <= Constants.EV_CHK_AROUND_DISTANCE)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }
    #endregion


    public void TakeOffComponents()
    {
        for (int i = 0; i < CDefines.VIEW_AVATAR_TYPE.Length; i++)
        {
            AVATAR_TYPE avatarType = CDefines.VIEW_AVATAR_TYPE[i];
            if (AvatarBTDic.ContainsKey(avatarType)) //when checked session off 
            {
                Destroy(AvatarBTDic[avatarType]);
            }
        }
    }

    public void SetAvatarQuestState(AVATAR_TYPE type, QUEST_STATE state)
    {
        if(AvatarQuestStateDic.ContainsKey( type ))
        {
            AvatarQuestStateDic[type] = state;
        }
        else
        {
            AvatarQuestStateDic.Add(type, state);
        }
    }

    public QUEST_STATE GetAvatarQuestState(AVATAR_TYPE type)
    {
        if(AvatarQuestStateDic.ContainsKey( type ))
        {
            CDebug.Log($"         [GetAvatarQuestState] avatar : {type}, Quest State : {AvatarQuestStateDic[type]}" );
            return AvatarQuestStateDic[type];
        }

        return QUEST_STATE.NONE;
    }



    public void SetAvatarMailQuestIcon(AVATAR_TYPE avatarType)
    {
        Vector3 pos = AvatarObjDic[avatarType].transform.localPosition;

        FollowObjectPositionFor2D fop = AvatarMailQuestIconObj[avatarType].transform.gameObject.GetComponent<FollowObjectPositionFor2D>();
        if (fop == null)
        {
            fop = AvatarMailQuestIconObj[avatarType].transform.gameObject.AddComponent<FollowObjectPositionFor2D>();
        }
        var canvasRect = WorldMgr.GetCanvasManager().GetComponent<RectTransform>();
        fop.Init( canvasRect, AvatarUIDummyBoneDic[avatarType].transform );

        AvatarMailQuestIconObj[avatarType].transform.localPosition = pos;

        AvatarMailQuestIconObj[avatarType].SetMailQuestData_Avatar( avatarType );

        AvatarMailQuestIconObj[avatarType].gameObject.SetActive( true );

        SetAvatarQuestState( avatarType, QUEST_STATE.RECEIVED );
    }


    public void SetActiveAvatarMailQuestIcon(AVATAR_TYPE type, bool bActive)
    {
        if (AvatarMailQuestIconObj.ContainsKey( type ))
        {
            AvatarMailQuestIconObj[type].gameObject.SetActive( bActive );
        }
    }

    public void SetActiveAvatarMailQuestIconEffect(AVATAR_TYPE type, bool bActive)
    {
        if (AvatarMailQuestIconObj.ContainsKey(type))
        {
            AvatarMailQuestIconObj[type].SetActivateEffect(bActive);
        }
    }

    public void HideEffectAvatarMailQuestIcon()
    {
        AvatarMailQuestIconObj.ForEach((idx, dictionary) => dictionary.Value.SetActivateEffect(false));
    }







    //------------ below 3d mail icon don't use : changed 2d icon ------------------------------------------------
    //public void SetAvatarMailIcon(AVATAR_TYPE type)
    //{
    //    if (AvatarMailIconObjDic.ContainsKey( type ) == false)
    //    {
    //        GameObject orgObj = WorldMgr.GetMailIconOriginObj();

    //        MailIcon icon = new MailIcon();
    //        icon.Obj = Utility.AddChild( GetManagementAvatarUIDummyObj( type ), orgObj );
    //        icon.Obj.layer = LayerMask.NameToLayer( Management_LayerTag.Layer_3DUI );

    //        icon.ObjCol = icon.Obj.GetComponent<Collider>();

    //        BillBoardUI board = icon.Obj.AddComponent<BillBoardUI>();
    //        board.SetCamera( WorldMgr.GetMainCameraObj() );

    //        UI3DScaler scaler = icon.Obj.AddComponent<UI3DScaler>();
    //        scaler.Init( WorldMgr.CamController );

    //        AvatarMailIconObjDic.Add( type, icon );

    //        AvatarMailIconObjDic[type].Obj.SetActive( true );

    //        ButtonByRay btn = AvatarMailIconObjDic[type].Obj.GetComponent<ButtonByRay>();
    //        if (btn != null)
    //        {
    //            btn.OnSelectObjectAsObservable
    //            .Subscribe( _ =>
    //            {
    //                //Debug.Log("******************************************************");
    //                SetAvatarQuestState( type, QUEST_STATE.COMPLETED );
    //                AvatarMailIconObjDic[type].Obj.SetActive( false );
    //                WorldMgr.RequestManagementInteraction((long)type);
    //            } ).AddTo( this );

    //        }
    //    }
    //    else
    //    {
    //        AvatarMailIconObjDic[type].Obj.SetActive( true );
    //    }

    //    SetAvatarQuestState( type, QUEST_STATE.RECEIVED );
    //}

    //public void SetActiveAvatarMailIconObj(AVATAR_TYPE type, bool bActive)
    //{
    //    if(AvatarMailIconObjDic.ContainsKey(type))
    //    {
    //        AvatarMailIconObjDic[type].Obj.SetActive( bActive );
    //    }
    //}

    //public void SetActiveAvatarMailIconCollider(AVATAR_TYPE type, bool bActive)
    //{
    //    if (AvatarMailIconObjDic.ContainsKey( type ))
    //    {
    //        AvatarMailIconObjDic[type].ObjCol.enabled =  bActive ;
    //    }
    //}

    public void ReleaseRuntimeAnimControlelr()
    {
        foreach(CCharacterController_Management cntlr in AvatarControllerDic.Values)
        {
            cntlr.ReleaseRuntimeAnimatorController(true);
        }
    }

    public void Release()
    {
        //Avatar를 옮기기 전에 붙은 Component 떼낸다
        foreach(KeyValuePair<AVATAR_TYPE, CCharacterController_Management> avatar in AvatarControllerDic)
        {
            avatar.Value.DisposerDispose();
            avatar.Value.DestroyComponents();
            avatar.Value.ReleaseHoldableObjects();

            Destroy(avatar.Value);
        }
        //Avatar Object 들은 삭제되지 않게 제자리로 돌려놓는다
        //TitleDirector.SetAvatarObjBack();
        

        if (WorldMgr != null) WorldMgr = null;
        if (AvatarObjDic != null)   
        {
            //확인해보자 이걸 이렇게 한다고?
            //AvatarObjDic.ForEach((idx, avatar) =>
            //{
            //    GameObject obj = avatar.Value;
            //    AvatarManager.SetActivateLOD(ref obj, true);
            //});

            AvatarObjDic.Clear();
            AvatarObjDic = null;
        }

        if(AvatarShadowObjDic != null)
        {
            foreach (KeyValuePair<AVATAR_TYPE, GameObject> item in AvatarShadowObjDic)
            {
                Destroy(item.Value);                    
            }
            AvatarShadowObjDic.Clear();
            AvatarShadowObjDic = null;
        }
        if (AvatarUIDummyBoneDic != null)
        {
            AvatarUIDummyBoneDic.Clear();
            AvatarUIDummyBoneDic = null;
        }
        if (AvatarInfoDic != null)
        {
            AvatarInfoDic.Clear();
            AvatarInfoDic = null;
        }
        if (AvatarControllerDic != null)
        {
            AvatarControllerDic.Clear();
            AvatarControllerDic = null;
        }
        if (OrgAvatarParent != null)
        {
            OrgAvatarParent.Clear();
            OrgAvatarParent = null;
        }
        if( PrevAvatarParent != null)
        {
            PrevAvatarParent.Clear();
            PrevAvatarParent = null;
        }
        if (PrevAvatarPos != null)
        {
            PrevAvatarPos.Clear();
            PrevAvatarPos = null;
        }
        if (AvatarEvent != null)
        {
            AvatarEvent.Clear();
            AvatarEvent = null;
        }
        if (AvatarEventState != null)
        {
            AvatarEventState.Clear();
            AvatarEventState = null;
        }
        if (AvatarEventIconObj != null)
        {
            foreach(ManagementAvatarEventUI icon in AvatarEventIconObj.Values)
            {
                icon.Release();
            }
            AvatarEventIconObj.Clear();
            AvatarEventIconObj = null;
        }
        if (AvatarNameTagObj != null)
        {
            foreach(GameObject obj in AvatarNameTagObj.Values)
            {
                if (obj != null)
                {
                    obj.SetActive(false);
                    Destroy(obj);
                }
            }
            AvatarNameTagObj.Clear();
            AvatarNameTagObj = null;
        }
        if(AvatarInteractionIconObjDic != null)
        {
            foreach(ManagementAvatarInteractionUI icon in AvatarInteractionIconObjDic.Values)
            {
                icon.Release();
            }
            AvatarInteractionIconObjDic.Clear();
            AvatarInteractionIconObjDic = null;
        }
        if (AvatarBTDic != null)
        {
            foreach (KeyValuePair<AVATAR_TYPE, BehaviorTree> bt in AvatarBTDic)
            {
                bt.Value.enabled = false;
                Destroy( bt.Value );
            }
            AvatarBTDic.Clear();
            AvatarBTDic = null;
        }
        if (AvatarBTFilePool != null)
        {
            AvatarBTFilePool.Clear();
            AvatarBTFilePool = null;
        }
        if (AvatarTrainingSectionScript != null)
        {
            AvatarTrainingSectionScript.Clear();
            AvatarTrainingSectionScript = null;
        }

        if (AvatarGridPoolDic != null)
        {
            AvatarGridPoolDic.Clear();
            AvatarGridPoolDic = null;
        }

        if( AvatarSpawnEachFloor != null)
        {
            AvatarSpawnEachFloor.Clear();
            AvatarSpawnEachFloor = null;
        }

        if (AvatarSpawnGridDic != null)
        {
            AvatarSpawnGridDic.Clear();
            AvatarSpawnGridDic = null;
        }

        if (AvatarCurrentGridDic != null)
        {
            AvatarCurrentGridDic.Clear();
            AvatarCurrentGridDic = null;
        }

        TouchedEventManager.ClearTouchEventDic();

        if (DelayTimerDisposer != null)
        {
            for (int i = 0; i < CDefines.VIEW_AVATAR_TYPE.Length; i++)
            {
                AVATAR_TYPE avatarType = CDefines.VIEW_AVATAR_TYPE[i];
                DisposeDelayTimerDisposer(avatarType);

            }            
            DelayTimerDisposer = null;
        }

        if (InteractionDisposer != null)
        {
            for (int i = 0; i < CDefines.VIEW_AVATAR_TYPE.Length; i++)
            {
                AVATAR_TYPE avatarType = CDefines.VIEW_AVATAR_TYPE[i];
                DisposeInteractionDisposer(avatarType);
            }
            InteractionDisposer = null;
        }

        if(SubDelayTimerDisposer != null)
        {
            for (int i = 0; i < CDefines.VIEW_AVATAR_TYPE.Length; i++)
            {
                AVATAR_TYPE avatarType = CDefines.VIEW_AVATAR_TYPE[i];
                DisposeSubDelayTimerDisposer(avatarType);
            }
            SubDelayTimerDisposer = null;
        }

        if(AvatarMailQuestIconObj != null)
        {
            foreach(ManagementAvatarEventUI icon in AvatarMailQuestIconObj.Values)
            {
                icon.Release();
            }
            AvatarMailQuestIconObj.Clear();
            AvatarMailQuestIconObj = null;
        }

        if(AvatarQuestStateDic != null)
        {
            AvatarQuestStateDic.Clear();
            AvatarQuestStateDic = null;
        }

        //if(AvatarMailIconObjDic != null)
        //{
        //    foreach (MailIcon IconObj in AvatarMailIconObjDic.Values)
        //    {
        //        if(IconObj.ClickBtn != null)
        //        {
        //            IconObj.ClickBtn.Dispose();
        //        }

        //        if (IconObj.Obj != null)
        //        {
        //            Destroy( IconObj.Obj );
        //        }
        //    }
        //    AvatarMailIconObjDic.Clear();
        //    AvatarMailIconObjDic = null;
        //}
    }
}