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
using GroupManagement;
using GroupManagement.ManagementEnums;
using UniRx;
using System.Linq;

public partial class ManagementWorldManager : MonoBehaviour
{
    [HideInInspector]
    public CManagementCameraController CamController;
    private Camera MainCamera;

    [HideInInspector]
	public const float NAME_TAG_SHOW_RATIO = 0.5f;
	public static float FOP_ADD_POSY = 2.0f;
	public static string CHARACTER_UI_DUMMY_BONE = "Dummy_Name";

    //----------- ROOM Size --------------------------------
    public float GridElementWidth = 3f;  // X-Size 
    public float GridElementHeight = 3f; // Y-Size
    public float GridElementDepth = 6.0f;  // Z-Size
    //------------------------------------------------------------

    public static bool PrevSequencePlay { get; set; } = false;


    [HideInInspector]
    public Vector3[] EVWaitPositions = new Vector3[2];
    public Vector3[] EVInnerPositions = new Vector3[2];
    public Vector3[] EVGetOffPositions = new Vector3[2];
    [HideInInspector]
    public float BaseElevatorDepth = 4.5f;



    CStateMachine<ManagementWorldManager> MainSM;

    private BuildingManager buildingMgr;
    private GridManager gridMgr;
    private SectionBuildManager buildMgr;
    private SectionManager sectionMgr;
    private LayerManager layerManager;
    private EVDoorManager EVDoorMgr;
    private GuideMailQuestManager guideMailQuestMgr;


    private ManagementCanvasManager canvasMgr;

    //Root Objects
    public Transform SectionRootObj;
    public Transform SelectorRootObj;

    GameObject SelectedSectionObj;
    SectionScript SelectedSectionScript;

    public GameObject ConstructionFxObj;
    public GameObject ConstructionFloorObj;
    public BoxCollider ConstructionFloorCol;

    private GridIndex BuildAreaGridPos;

    public ManagementAvatarManager AvatarMgr;
    public ManagementNpcManager NpcMgr;

    private GameObject SelectSectionBorderObj;
    private SpriteRenderer SelectSectionBorderSR;

    public float AI_Timer;
    public bool bPauseAI;

	public bool IsLoadFinishPageUI{ get; set; }

    public GameObject MailIconOriginObj;

    int reddot_LayerLevelUP;
    List<long> reddot_lstSectionLevelup = new List<long>();
    List<long> reddot_lstSectionCreation = new List<long>();

    public void Initialize()
    {
        MainSM = new CStateMachine<ManagementWorldManager>(this);
        
        // 레드닷 관련 데이터 초기화
        reddot_LayerLevelUP = 0;
        reddot_lstSectionLevelup.Clear();
        reddot_lstSectionCreation.Clear();

        MainCamera = transform.Find("MainViewCamera").GetComponent<Camera>() ;
        CamController = MainCamera.gameObject.GetComponent<CManagementCameraController>();

        SectionRootObj = transform.Find("World/Offices");

        // BuildingManager
        buildingMgr = transform.Find("World/Building").GetComponent<BuildingManager>();
        buildingMgr.InitializeBuildingManager();

        // Elevator Manager
        EVDoorMgr = transform.Find("World/Building/Elevators").GetComponent<EVDoorManager>();
        EVDoorMgr.Initialize(this);

        // RoomManager
        sectionMgr = gameObject.AddComponent<SectionManager>();
        sectionMgr.InitializeSectionManager(SectionRootObj);

        // GridManager
        gridMgr = gameObject.AddComponent<GridManager>();
        gridMgr.InitializeGridManager();

        SelectorRootObj = transform.Find("World/Selectors");

        // BuildManager
        buildMgr = gameObject.AddComponent<SectionBuildManager>();
        buildMgr.InitializeBuildManager();

        AvatarMgr = transform.Find("World/Members").GetComponent<ManagementAvatarManager>();
        AvatarMgr.Initialize();
        SetEvWaitPositions();
        
        NpcMgr = transform.Find("World/NPC").GetComponent<ManagementNpcManager>();
        NpcMgr.Initialize();

        CamController.Initialize();

        InitBorders();

        layerManager = transform.Find("World/Building/root_middlefloor").GetComponent<LayerManager>();
        layerManager.InitializeLayerManager();

        canvasMgr = (ManagementCanvasManager)ManagementManager.Instance.GetCanvasManager();

        if(guideMailQuestMgr == null)
        {
            guideMailQuestMgr = gameObject.GetComponent<GuideMailQuestManager>();
            if(guideMailQuestMgr == null)
            {
                guideMailQuestMgr = gameObject.AddComponent<GuideMailQuestManager>();
            }
            guideMailQuestMgr.Initialize();
        }

        LoadConstructionFxObj();

        LoadMailIconOriginObj();

    }

    public Transform GetMainCameraObj()
    {
        return MainCamera.transform;
    }

    public Camera GetMainCamera()
    {
        return MainCamera;
    }

    private void FixedUpdate ()
    {
        CamRatioActiveUpdate ();
    }

    // 카메라 거리에 따라 달라지는 이벤트
    private Action<bool> lastUpdateAction;
    private void CamRatioActiveUpdate ()
    {
        if (!SelectedSectionScript)
        {
            Release ();
            return;
        }
        if ((ENUM_BUILD_STATE)SelectedSectionScript.sectionStatusData.sectionStatus != ENUM_BUILD_STATE.COMPLETE)
        {
            Release ();
            return;
        }

        bool isActive = false;
        ENUM_SECTION_TYPE sectionType = (ENUM_SECTION_TYPE)SelectedSectionScript.sectionStatusData.GetSectionTableData().Section_Type;
        switch (sectionType)
        {
            case ENUM_SECTION_TYPE.Production:
                break;
            case ENUM_SECTION_TYPE.Support:
                break;

            case ENUM_SECTION_TYPE.Special:
                ENUM_SECTION_SUBTYPE_SPECIAL specialType = (ENUM_SECTION_SUBTYPE_SPECIAL)SelectedSectionScript.sectionStatusData.GetSectionTableData().Section_Sub_Type;

                switch (specialType)
                {
                    // 아카이브 정보창 띄우기
                    case ENUM_SECTION_SUBTYPE_SPECIAL.ARCHIVE:
                        if (GetCurrentState() == ManagementWorldState_Section_Info.Instance ())
                        {
                            isActive = NAME_TAG_SHOW_RATIO <= CamController.GetCameraMoveRatioByZoom ();
                            Release ();
                            lastUpdateAction = SelectedSectionScript.UpdateArchiveTrophyInfo;
                            lastUpdateAction.Invoke (isActive);
                        }
                        break;
                }
                break;
        }

        // 마지막 해제 처리
        if(isActive == false)
        {
            Release ();
        }
        void Release ()
        {
            lastUpdateAction?.Invoke (false);
            lastUpdateAction = null;
        }
    }

    public void SetCameraMovingLimit()
    {
        CamController.SetCameraMovingLimit();
    }

    public void SetBlockCameraPinchZoom(bool bBlock)
    {
        CamController.SetIsBlockPinchZoom(bBlock);
    }

#if UNITY_EDITOR
    public void SetCameraLimitRect(float right, float up, float bottom, float btm_min)
    {
        CamController.SetCameraLimitRect(right, up, bottom, btm_min);
    }

    int npcTestID = 181123201;
#endif

    public void UpdateCameraMovingLimit()
    {
        CamController.UpdateCamMovingLimit();
    }

    private void Update()
    {
        Update_StateMachine();

//#if UNITY_EDITOR
//        //cheat for quest(mail icon)
//        if (Input.GetKeyDown( KeyCode.M ))
//        {
//            AvatarMgr.SetAvatarMailQuestIcon( AVATAR_TYPE.AVATAR_JISOO );
//        }
//        if (Input.GetKeyDown( KeyCode.N ))
//        {
//            AvatarMgr.SetAvatarMailQuestIcon( AVATAR_TYPE.AVATAR_JENNIE );
//        }
//        if (Input.GetKeyDown( KeyCode.B ))
//        {
//            AvatarMgr.SetAvatarMailQuestIcon( AVATAR_TYPE.AVATAR_LISA );
//        }
//        if (Input.GetKeyDown( KeyCode.V ))
//        {
//            AvatarMgr.SetAvatarMailQuestIcon( AVATAR_TYPE.AVATAR_ROSE );
//        }
//        if (Input.GetKeyDown( KeyCode.C ))
//        {
//            NpcMgr.SetNpcMailIconObj( 181106101 );
//        }
//        if (Input.GetKeyDown( KeyCode.X ))
//        {
//            SectionStatusData statusdata = ManagementSectionStatusManager.Instance.GetPDRoomSectionStatusData();// GetSectionStatusData_GoodsShop();
//            SectionScript ss = GetSectionManager().GetSectionRefData( statusdata.sectionID_ServerUniqueID ).SectionScript;

//            ss.SetMailIconUIObj( npcTestID++, -1 );
//        }
//#endif
    }

    void Update_StateMachine()
    {
        if (IsStateMachineNull() == false)
        {
            MainSM.StateMachine_Update();
        }
    }

    public void ChangeState(CState<ManagementWorldManager> newState)
    {
        if (IsStateMachineNull() == false)
        {
            MainSM.ChangeState(newState);
        }
    }

    public CState<ManagementWorldManager> GetCurrentState()
    {
        if (IsStateMachineNull())
            return null;
        else
            return MainSM.GetCurrentState();
    }

    public bool IsStateMachineNull()
    {
        return (MainSM == null);
    }

    #region SECTIONOBJ_RAY
    void InitBorders()
    {
        var resData = CResourceManager.Instance.GetResourceData(Constants.PATH_3DUI_SELECT_SECTION);
        SelectSectionBorderObj = Instantiate(resData.Load<GameObject>(gameObject));

        SelectSectionBorderSR = SelectSectionBorderObj.transform.Find("line_bg").GetComponent<SpriteRenderer>();
 
        SelectSectionBorderObj.name = "Office_Selector";
        SetSelectSectionBorderParent();
    }

    public void SetSelectSectionBorderParent()
    {
        SelectSectionBorderObj.transform.parent = transform;
        SelectSectionBorderObj.gameObject.SetActive(false);
    }

    public void SetSelectSectionBorder(SectionScript script, GameObject objecct)
    {
        SetSelectedObject(objecct, script);
        SetActiveSelectSectionBorderObj(true);
    }

    public int GetClickedObjectIndexByTag(RaycastHit[] hits, string tag)
    {
        for (int i = 0; i < hits.Length; ++i)
        {
            RaycastHit hit = hits[i];

            if (hit.collider.tag.Equals(tag))
            {
                return i;
            }
        }
        return -1;
    }

    private RaycastHit[] GetSortRaycastHits(RaycastHit[] hits)
    {
        RaycastHit[] _rh = hits;

        Array.Sort(_rh, (a, b) => {
            if (a.collider.tag.Equals(Management_LayerTag.Tag_3DUI_EVENT))
                return -1;
            else 
                return 1;
        });

        for (int i = 0; i < _rh.Length; ++i)
        {
            CDebug.Log("#############################    tag = "+_rh[i].collider.tag);
        }
        return _rh;
    }

    public void SetTouchedObjectByPriority(RaycastHit[] hits)
    {
        //CDebug.Log("#############################    SetTouchedObjectByPriority");
        Vector3 pos = Vector3.zero;

        RaycastHit[] sortedHits = GetSortRaycastHits(hits);

        for (int i = 0; i < sortedHits.Length; ++i)
        {            
            RaycastHit hit = sortedHits[i];
            if (hit.collider.tag.Equals(Management_LayerTag.Tag_3DUI_EVENT))
            {
                ButtonByRay btnRay = hit.collider.gameObject.GetComponent<ButtonByRay>();
                btnRay.ButtonClicked(hit.collider.gameObject);
                //below 'return' doesn't need anymore because section double tab
                //return;
            }
            else if (hit.collider.tag.Equals(Management_LayerTag.Tag_3DUI_BUILDING))
            {
                ButtonByRay btnRay = hit.collider.gameObject.GetComponent<ButtonByRay>();
                if (btnRay != null)
                {
                    btnRay.ButtonClicked(hit.collider.gameObject);
                }
                else
                {
                    // 각 상태에서 Input 처리
                    GetCurrentState().InputProcess(hits);
                    return;
                }
            }

        }

        // 각 상태에서 Input 처리
        GetCurrentState().InputProcess(hits);
    }

    public void SetActiveSelectSectionBorderObj(bool bActive)
    {
        if(bActive)
        {
            SectionScript script = GetSelectedSectionScript();

            if(script != null)
            {
                SelectSectionBorderObj.transform.parent = script.transform;
                SelectSectionBorderSR.size = new Vector2(GridElementWidth * script.sectionInstanceData.GetSectionSizeX(), GridElementHeight * script.sectionInstanceData.GetSectionSizeY());
                SelectSectionBorderObj.transform.localPosition = Vector3.zero;
            }
        }

        SelectSectionBorderObj.SetActive(bActive);
    }

    public bool DoesTouchSameSection(SectionScript touchsection)
    {
        long prevSectionID = 0;
        long touchedSectionID = 0;

        SectionScript prevScript = GetSelectedSectionScript();

        if (prevScript != null)
        {
            prevSectionID = prevScript.GetSectionID_ServerUniqueID();
        }

        touchedSectionID = touchsection.GetSectionID_ServerUniqueID();

        if (prevSectionID == touchedSectionID)
        {
            return true;
        }

        return false;
    }

    public bool DoesTouchRejectRoom(SectionScript script)
    {
        if (
            script.sectionInstanceData.sectionType == (byte)ENUM_SECTION_TYPE.EmptyRoom || 
            (script.sectionInstanceData.sectionType == (byte)ENUM_SECTION_TYPE.Special && script.sectionInstanceData.sectionSubType == (byte)ENUM_SECTION_SUBTYPE_SPECIAL.LOBBY) ||
            (script.sectionInstanceData.sectionType == (byte)ENUM_SECTION_TYPE.Special && script.sectionInstanceData.sectionSubType == (byte)ENUM_SECTION_SUBTYPE_SPECIAL.PARKINGLOT) 
            )
        {
            return true;
        }

        return false;
    }

    public void SetTouchSection(SectionScript selectScript, GameObject selectObject)
    {
        if (CamController.CamActionState != CameraActionState.ACTION)
        {
            return;
        }
        if (DoesTouchSameSection(selectScript))
        {
            if (DoesTouchRejectRoom(selectScript))
                return;
        }
        else
        {
            if (DoesTouchRejectRoom(selectScript) == false)
            {
                //New Section Setup
                SetSelectSection(selectObject, selectScript);
            }
            else
            {
                ReleaseSectionInfoState();
            }
        }
    }

    //Select Section
    public void SetSelectSection(GameObject selObj, SectionScript selSc)
    {
        SetSelectedObject(selObj, selSc);
        SetActiveSelectSectionBorderObj(true);
    }

    //Select Section + ZoomIn
    public void SetSelectSectionWithZoom(GameObject selObj, SectionScript selSc, bool bZoomIn)
    {
        SetSelectSection(selObj, selSc);
        SetSelcectedSectionCamZoom(selSc, selObj, bZoomIn);
    }

    //Release Section
    public void ReleaseSelectedSection()
    {
        SetSelectedObject(null, null);
        SetActiveSelectSectionBorderObj(false);
    }

    public void ReleaseSectionInfoState()
    {
        SetSelectedObject(null, null);
        SetActiveSelectSectionBorderObj(false);
        ChangeState(ManagementWorldState_Common.Instance());
    }

    public GridIndex GetBuildAreaGridPos()
    {
        return BuildAreaGridPos;
    }

    public void SetBuildAreaGridPos(GridIndex pos)
    {
        BuildAreaGridPos = pos;
    }

    public void SetSelectedObject(GameObject selobj, SectionScript script)
    {
        SelectedSectionObj = selobj;
        SelectedSectionScript = script;
    }

    public SectionScript GetSelectedSectionScript()
    {
        return SelectedSectionScript;
    }

    public GameObject GetSelectedSectionObject()
    {
        return SelectedSectionObj;
    }

    public void SetCameraFocusToAvatarObject(AVATAR_TYPE avatarType)
    {
        GameObject _avatarObj = AvatarMgr.GetManagementAvatarObj(avatarType);
        if (_avatarObj == null)
        {
            CDebug.Log("SetCameraFocusToAvatarObject() AVATAR_TYPE : " + avatarType + " is not exist");
            return;
        }

        SetCameraFocusToCharacterObject(_avatarObj);
    }

    public void SetCameraFocusToNPCObject(long npcUID)
    {
        GameObject _npcObj = NpcMgr.GetNpcGameObject(npcUID);
        if(_npcObj == null)
        {
            CDebug.Log("SetCameraFocusToNPCObject() npcUID : " + npcUID + " is not exist");
            return;
        }

        SetCameraFocusToCharacterObject(_npcObj);
    }


    /// <summary>
    /// Camera goes target object as zoom in
    /// </summary>
    /// <param name="obj">target object</param>
    public void SetCameraFocusToCharacterObject(GameObject obj)
    {
        Vector3 _pos = obj.transform.position;
        //absolutely zoom In
        Vector3 focusPos = new Vector3(_pos.x, _pos.y + 1, CamController.ZOOMIN_Dist);
        SetCameraFocusToPosition(focusPos);
    }

    public void SetCameraFocusToSection(SectionScript script, bool bZoomIn = false)
    {
        if (CamController.IsFinishFucusToObject())
            return;
        Vector3 centerPos = GetSectionCenterPosition(script, script.gameObject);
        float zoomDist = CamController.ZOOMIN_Dist;
        if (bZoomIn == false)
        {
            zoomDist = CamController.ZOOMOUT_Dist;
        }
        Vector3 focusPos = new Vector3(centerPos.x, centerPos.y, zoomDist );
        SetCameraFocusToPosition(focusPos);
    }

    //absolutely zoom In
    public void SetCameraFocusToPosition(Vector3 targetPos, bool bLockAfterFocus = false)
    {
        CamController.SetStopExtraMove();
        CamController.SetFocusToPosition(targetPos, bLockAfterFocus);
    }

    public Vector3 GetSectionCenterPosition(SectionScript script, GameObject sectionobj)
    {
        float xPos = (GridElementWidth * script.sectionInstanceData.GetSectionSizeX()) / 2;
        float yPos = (GridElementHeight * script.sectionInstanceData.GetSectionSizeY()) / 2;

        //return new Vector3(xPos, yPos, 0);
        return new Vector3(sectionobj.transform.position.x + xPos, sectionobj.transform.position.y + yPos, sectionobj.transform.position.z);
    }

    public void SetSelcectedSectionCamZoom(SectionScript sectionscript, GameObject sectionobj, bool bZoomIn)
    {
        if (CamController.CamActionState == CameraActionState.ACTION_PREPARE || CamController.CamActionState == CameraActionState.ACTION_STOP_PREPARE)
            return;
        Vector3 pos = GetSectionCenterPosition(sectionscript, sectionobj);
        SetCameraZoomToPosition(pos, bZoomIn);
    }

    public void SetCameraZoomToPosition(Vector3 targetPos, bool bZoomIn)
    {
        CamController.SetDoubleTabObjectPosition(targetPos);
        CamController.SetCameraZoomStateByZoom(bZoomIn);
    }

    public void SetCameraZoomToCurrentPosition(bool bZoomIn)
    {
        CamController.SetCameraCurrentPositionToZoom();
        CamController.SetCameraZoomStateByZoom(bZoomIn);
    }


    #endregion SECTIONOBJ_RAY

    #region Build Function
    public void HideRoomSelectors()
    {
        buildMgr.HideSectionSelectors();
    }

    GridIndex[] arrIndex;
    public void ShowSectionBorders(GridIndex[] indexArray, ENUM_SECTION_SIZES sectionsize, BORDERTYPE type)
    {
        if (arrIndex != null)
        {
            arrIndex = null;
        }

        arrIndex = indexArray;

        if (indexArray == null || indexArray.Length == 0) return;

        for (int i = 0; i < indexArray.Length; i++)
        {
            long sectionid_serveruniqueid = GetGridManager().GetGridTileSectionID_ServerUniqueID(indexArray[i]);
            SectionRef _data = GetSectionManager().GetSectionRefData(sectionid_serveruniqueid);
            _data.SectionScript.ShowBorder(type, sectionsize);
        }
    }

    public void RemoveSectionBorders()
    {
        if (arrIndex == null || arrIndex.Length == 0) return;

        for (int i = 0; i < arrIndex.Length; i++)
        {
            long sectionid_serveruniqueid = GetGridManager().GetGridTileSectionID_ServerUniqueID(arrIndex[i]);
            SectionRef _data = GetSectionManager().GetSectionRefData(sectionid_serveruniqueid);
            _data.SectionScript.RemoveAllBorders();
        }
    }

    public void CreateFloor(ManagementMapData layerdata)
    {
        buildMgr.CreateFloor(layerdata);
    }

    public void CreateSection(SectionStatusData statusdata, GridIndex buildpos)
    {
        buildMgr.CreateSection(statusdata, buildpos);
    }

    public int GetTopFloor()
    {
        return ManagementServerDataManager.Instance.GetTopFloor();
    }

    public int GetNewFloor()
    {
        return ManagementServerDataManager.Instance.GetTopFloor() + 1;
    }

    public GridIndex[] GetPossibleBuildingindizes(ENUM_SECTION_SIZES size)
    {
        return gridMgr.GetPossibleBuildingindizes(size);
    }
      
    public void ChangeSectionPosition()
    {
        buildingMgr.stateMachine.ChangeState(BuildingState_Relocation_Request.Instance());
    }

#endregion Build Function



#region GetManager
    public SectionBuildManager GetBuildManager()
    {
        return buildMgr;
    }

    public SectionManager GetSectionManager()
    {
        return sectionMgr;
    }
    
    public GridManager GetGridManager()
    {
        return gridMgr;
    }

    public BuildingManager GetBuildingManager()
    {
        return buildingMgr;
    }

    public LayerManager GetLayerManager()
    {
        return layerManager;
    }

    public EVDoorManager GetEVDoorManager()
    {
        return EVDoorMgr;
    }

    public ManagementCanvasManager GetCanvasManager()
    {
        if(canvasMgr == null)
        {
            canvasMgr = (ManagementCanvasManager)ManagementManager.Instance.GetCanvasManager();
        }
        return canvasMgr;
    }

    public ManagementPageUI GetPageUIManager()
    {
        return GetCanvasManager().PageUI;
    }

    public GuideMailQuestManager GetGuideMailQuestManager()
    {
        return guideMailQuestMgr;
    }

	#endregion GetManager


	#region server data management
	public void UpdateLayerAllResponseData(ManagementAPI.ManagementLayerAllResponseData _resdata)
    {
        ManagementServerDataManager.Instance.UpdateLayerAllResponseData(_resdata);
    }

#endregion server data management


    public bool CheckSectionRequirements()
    {
        List<SectionTableData> listSectionTableData = ManagementDataManager.Instance.GetSectionTableDataListBySectionType(ENUM_SECTION_TYPE.Production);
        List<SectionTableData> trainningSectionTableData = ManagementDataManager.Instance.GetSectionTableDataListBySectionType(ENUM_SECTION_TYPE.Support);
        List<SectionTableData> etcSectionTableData = ManagementDataManager.Instance.GetSectionTableDataListBySectionType(ENUM_SECTION_TYPE.Special);

        if(trainningSectionTableData != null)
        {
            foreach (SectionTableData item in trainningSectionTableData)
            {
                listSectionTableData.Add(item);
            }
        }

        if (etcSectionTableData != null)
        {
            foreach (SectionTableData item in etcSectionTableData)
            {
                listSectionTableData.Add(item);
            }
        }

        if(listSectionTableData != null)
        {
            foreach (var data in listSectionTableData)
            {
                ManagementSectionCountData sectionCountData = ManagementServerDataManager.Instance.GetSectionCountData(data.ID);
                int curSectionCount = sectionCountData == null ? 0 : sectionCountData.cnt;
                SectionCreationTableData curCreationData = ManagementDataManager.Instance.GetSectionCurrentCreationData(data.ID, curSectionCount);
                SectionCreationTableData possibleCreationData = ManagementDataManager.Instance.GetSectionPossibleCreationData(data.ID);
                SectionCardConditions condition = CheckSectionConditions(curCreationData, sectionCountData, possibleCreationData, out _);

                if (condition == SectionCardConditions.Satisfy)
                    return true;
            }
        }

        return false;
    }

    public SectionCardConditions CheckSectionConditions(SectionCreationTableData curCreationData, ManagementSectionCountData sectionCountData, SectionCreationTableData possibleCreationData, out bool bConsumeCondition)
    {
        int CONSUME_MAX_COUNT = 2;
        bConsumeCondition = true;
                
        if (CRequireManager.CheckConditionAllRequire(curCreationData.Requires))
        {
            for (int i = 0; i < CONSUME_MAX_COUNT; i++)
            {
                if (curCreationData.Consume[i].Type == REWARD_CONSUME_TYPES.NULL)
                    continue;

                if (!curCreationData.Consume[i].CheckConsumeCondition())
                    bConsumeCondition = false;
            }

            if (sectionCountData != null)
            {
                if (sectionCountData.cnt >= possibleCreationData.Section_ID_No)
                {
                    return SectionCardConditions.OverLimit;
                }
                else
                {
                    if (!bConsumeCondition)
                        return SectionCardConditions.NotEnoughGoods;
                    else
                        return SectionCardConditions.Satisfy;
                }
            }
            else
            {
                if (!bConsumeCondition)
                    return SectionCardConditions.NotEnoughGoods;
                else
                    return SectionCardConditions.Satisfy;
            }
        }
        else
        {
            return SectionCardConditions.UnSatisfied;
        }
    }

    public void SetNavMeshData(GameObject prefab, string navMeshDataPath, float yValue)
    {
        var resData = CResourceManager.Instance.GetResourceData(navMeshDataPath);
        if (resData != null)
        {
            try
            {
                NavMeshData nmData = resData.Load<NavMeshData>(prefab);
                NavMeshUpdateOnEnable nmUpdate = prefab.GetComponent<NavMeshUpdateOnEnable>();
                if (nmUpdate == null)
                {
                    nmUpdate = prefab.AddComponent<NavMeshUpdateOnEnable>();
                }

                Vector3 pos = nmData.position;
                pos.y = yValue;
                nmUpdate.SetNavMeshData(nmData, pos);

                prefab.gameObject.SetActive(false);
                prefab.gameObject.SetActive(true);
            }
            catch(Exception e)
            {
                Debug.LogError(string.Format("[Navmesh]Prefab = {0}, path = {1}",e, navMeshDataPath));
            }
        }
    }

    
    public bool IsThereAvalableNavMesh()
    {
        NavMeshHit hit;
        if (NavMesh.SamplePosition(Vector3.zero, out hit, 1000.0f, NavMesh.AllAreas))
        {
            Vector3 result = hit.position;
            return true;
        }

        if(NavMesh.SamplePosition(Vector3.zero, out hit, 1000.0f, 1 << NavMesh.GetAreaFromName("Walkable")))
        {
            return true;
        }

        return false;
    }


    //BT use
    public CCharacterController GetCharacterController(Transform charObj, out long npc_uid)
    {
        //also Find Member Avatar's CCharacterController
        return NpcMgr.GetNpcCharacterController(charObj, out npc_uid);
    }

    //BT use
    public CCharacterController GetAvatarCharController(AVATAR_TYPE mType)
    {
        return AvatarMgr.GetManagementAvatarController(mType);
    }

    /// <summary>
    /// 임시 변수. 카메라 작업이 마무리 되면 아래 변수들은 제거합니다.
    /// 2021.08.30 부터 작업 시작 한다고 정훈형님께 전달 받음.
    /// (2021.08.25 성준엽)
    /// </summary>
    private bool isFirstCameraMove = false;
	private Vector3 originCamPosition = default;

    public AVATAR_TYPE GetAvatarTypeByTag(string tag)
    {
        AVATAR_TYPE avatarType = AVATAR_TYPE.AVATAR_NONE;
        if (tag.Equals(Management_LayerTag.Tag_AVATAR_JISOO))
        {
            avatarType = AVATAR_TYPE.AVATAR_JISOO;
        }
        else if (tag.Equals(Management_LayerTag.Tag_AVATAR_JENNIE))
        {
            avatarType = AVATAR_TYPE.AVATAR_JENNIE;
        }
        else if (tag.Equals(Management_LayerTag.Tag_AVATAR_LISA))
        {
            avatarType = AVATAR_TYPE.AVATAR_LISA;
        }
        else if (tag.Equals(Management_LayerTag.Tag_AVATAR_ROSE))
        {
            avatarType = AVATAR_TYPE.AVATAR_ROSE;
        }

        return avatarType;
    }

    public CHARACTER_TYPE GetCharType(string tag)
    {
        if (tag.Equals("NPC"))
        {
            return CHARACTER_TYPE.NPC;
        }

        return CHARACTER_TYPE.AVATAR;
    }

    public void ShowSectionInfoUI(long sectionidx)
    {
        GetCanvasManager()?.PageUI?.roomUI?.SetSectionInfoUI(sectionidx);
    }

    private void LoadConstructionFxObj()
    {
        //Construction Fx for Section
        var resData = CResourceManager.Instance.GetResourceData(Constants.PATH_CONST_NEWSECTION_OBJ);
        ConstructionFxObj = UnityEngine.Object.Instantiate(resData.Load<GameObject>(gameObject));
        ConstructionFxObj.SetActive(false);

        //Construction Fx for Expansion Floor
        var resExData = CResourceManager.Instance.GetResourceData(Constants.PATH_CONST_FLOOR_EXPANSION_OBJ);
        ConstructionFloorObj = UnityEngine.Object.Instantiate(resExData.Load<GameObject>(gameObject));
        ConstructionFloorCol = ConstructionFloorObj.GetComponent<BoxCollider>();
        ConstructionFloorObj.SetActive(false);
    }

    private void LoadMailIconOriginObj()
    {
        CResourceData resData = CResourceManager.Instance.GetResourceData( Constants.PATH_CONST_MAILICON_OBJ );
        MailIconOriginObj = resData.Load<GameObject>(gameObject);
    }

    public GameObject GetMailIconOriginObj()
    {
        return MailIconOriginObj;
    }

    public void ReplaceAvatar(AVATAR_TYPE avatarType)
    {
        AvatarMgr.ReplaceAvatar(avatarType);
    }

    #region guide mail quest
    //public void RequestManagementInteraction(long requestid) // npc id or member id
    //{
    //    //var planeRes = CResourceManager.Instance.GetResourceData("FX/prefab/fu_send_mail_to_plane.prefab");
    //    //if (planeRes != null)
    //    //{
    //    //    var planePrefab = planeRes.Load<GameObject>(gameObject);
    //    //    var planeInst = Utility.AddChild(CStaticCanvas.Instance.gameObject, planePrefab);
    //    //    SoundManager.Instance.PlayEffect(6810063); //비행기

    //    //    planeInst.transform.SearchAnimation().PlayFirstAsObservable().Subscribe(_ =>
    //    //    {
    //    //        Destroy(planeInst);
    //    //    });
    //    //}

    //    APIHelper.ManagementSvc.Management_Interaction(requestid)
    //    .Subscribe(res =>
    //    {
    //        // 가이드 메일 클릭 이벤트 실행 (삭제 확인)
    //        var planeRes = CResourceManager.Instance.GetResourceData("FX/Open/prefab/fu_send_mail_to_plane.prefab");
    //        if (planeRes != null)
    //        {
    //            var planePrefab = planeRes.Load<GameObject>(gameObject);
    //            var planeInst = Utility.AddChild(CStaticCanvas.Instance.gameObject, planePrefab);
    //            SoundManager.Instance.PlayEffect(6810063); //비행기

    //            planeInst.transform.SearchAnimation().PlayFirstAsObservable().Subscribe(_ =>
    //            {
    //                Destroy(planeInst);
    //            });

    //        }
    //        CDebug.Log("Management_Interaction() complete");
    //    });
    //}


    public IObservable<Unit> RequestManagementInteractionObservable(long requestid) // npc id or member id
    {
        return APIHelper.ManagementSvc.Management_Interaction(requestid)
        .SelectMany(res => 
        {
            if(res != null && res.Common != null && res.Common.AlarmInfo != null && res.Common.AlarmInfo.intraMail != null)
            {
                CheckGuideMailquest(res.Common.AlarmInfo.intraMail);
            }

            var planeRes = CResourceManager.Instance.GetResourceData("FX/Open/prefab/fu_send_mail_to_plane.prefab");
            if (planeRes != null)
            {
                var planePrefab = planeRes.Load<GameObject>(gameObject);
                var planeInst = Utility.AddChild(CStaticCanvas.Instance.gameObject, planePrefab);
                SoundManager.Instance.PlayEffect(6810063); //비행기

                CDebug.Log("Management_Interaction() complete");
                return planeInst.transform.SearchAnimation().PlayFirstAsObservable()
                .Do(_ =>
                {
                    Destroy(planeInst);
                })
                .AsUnitObservable();
            }
            else
            {
                return Observable.ReturnUnit();
            }
        });
    }

    public void CheckGuideMailquest(List<MainPageAPI.IntraMailAlarmData> lstmaildata)
    {
        if (lstmaildata == null)
            return;

        if (lstmaildata.Count > 0)
        {
            GuideMailQuestManager gmqmgr = ManagementManager.Instance.worldManager.GetGuideMailQuestManager();
            foreach (MainPageAPI.IntraMailAlarmData newmailinfo in lstmaildata)
            {
                IntraMailTableData data = IntraMailManager.GetIntraMaiTablelData(newmailinfo.gdid);
                if (data.Target_Use == 1
                    && (data.Target_Type == (byte)ENUM_GUIDE_QUEST_TARGET.MANAGEMENT_NPC
                    || data.Target_Type == (byte)ENUM_GUIDE_QUEST_TARGET.MANAGEMENT_MEMBER))
                {
                    CDebug.Log("NPC Quest 말풍선 사용");
                    gmqmgr.SetGuideMailQuest(newmailinfo.gdid, data);
                }
            }
        }
    }

    #endregion guide mail quest

    #region TOUCH_EVENT
    public void TouchAvatar(AVATAR_TYPE aType)
    {
        AvatarMgr.TouchAvatar(aType);
    }
    #endregion TOUCH_EVENT


    #region AI
    private void SetEvWaitPositions()
    {
        EVWaitPositions[0] = new Vector3(-2.9f, 0.1f, 1.3f);
        EVWaitPositions[1] = new Vector3(-2.3f, 0.1f, 1.5f);

        //EVInnerPositions
        EVInnerPositions[0] = new Vector3(-2.7f, 0.1f, BaseElevatorDepth);
        EVInnerPositions[1] = new Vector3(-2.1f, 0.1f, BaseElevatorDepth + 0.5f);

        //EVGetOffPositions
        EVGetOffPositions[0] = new Vector3(-2.7f, 0.1f, 1);
        EVGetOffPositions[1] = new Vector3(-2.1f, 0.1f, 1.2f);
    }

    public void SetStopHideAllAvatars()
    {
        AvatarMgr.SetActiveAllAvatars(false);
    }

    public void SetRefreshAvatarAI()
    {
        //GetEVManager().InitAllEV();
        //GetEVManager().ClearEVControllerDic();
        AvatarMgr.SetAllAvatarRespawn();
        
        //AvatarMgr.ShowAvatarRecvedEventIcon();
        AvatarMgr.SetRefreshAvatarAIGridPool(-1, true);
    }
    #endregion AI

    private void GetBackAvatars()
    {
        if(AvatarMgr != null)
        {
            AvatarMgr.TakeOffComponents();
            AvatarMgr.ReleaseRuntimeAnimControlelr();
        }
        //Avatar Object 들은 삭제되지 않게 제자리로 돌려놓는다
        StaticAvatarManager.SetAvatarObjBack();
    }

    #region reddot layer expansion
    public void InitRedDotForLayerExpansion()
    {
        reddot_LayerLevelUP = 0;
    }

    public void SetRedDotForLayerExpansion(int layerindex)
    {
        reddot_LayerLevelUP = layerindex;
    }

    public bool CheckRedDotForLayerExpansion()
    {
        return reddot_LayerLevelUP > 0;
    }
    #endregion reddot layer expansion


    //#region reddot section levelup    // 부서 선택시 req 날리는거로 방식을 바꿔 달래서 사용 안 함. 
    //public void InitRedDotForSectionLevelUP()
    //{
    //    reddot_lstSectionLevelup.Clear();
    //}

    //public void AddRedDotForSectionLevelUP(long sectionuniqueid)
    //{
    //    if(CheckRedDotForSectionLevelUP(sectionuniqueid) == false)
    //    {
    //        reddot_lstSectionLevelup.Add(sectionuniqueid);
    //    }
    //}

    //public void RemoveRedDotForSectionLevelUP(long sectionuniqueid)
    //{
    //    if(CheckRedDotForSectionLevelUP(sectionuniqueid) == true)
    //    {
    //        reddot_lstSectionLevelup.Remove(sectionuniqueid);
    //    }
    //}

    //public bool CheckRedDotForSectionLevelUP(long sectionuniqueid)
    //{
    //    return reddot_lstSectionLevelup.Contains(sectionuniqueid);
    //}
    //#endregion reddot section levelup

    #region reddot section creation
    public void InitRedDotForSectionCreation()
    {
        reddot_lstSectionCreation.Clear();
    }

    public void AddRedDotForSectionCreation(long sectionuniqueid)
    {
        if(CheckRedDotForSectionCreation(sectionuniqueid) == false)
        {
            reddot_lstSectionCreation.Add(sectionuniqueid);
        }
    }

    public void RemoveRedDotForSectionCreation(long sectionuniqueid)
    {
        if (CheckRedDotForSectionCreation(sectionuniqueid) == true)
        {
            reddot_lstSectionCreation.Remove(sectionuniqueid);
        }
    }

    public bool CheckRedDotForSectionCreation(long sectionuniqueid)
    {
        return reddot_lstSectionCreation.Contains(sectionuniqueid);
    }

    public bool CheckRedDotForSectionCreation()
    {
        return reddot_lstSectionCreation.Count > 0;
    }

    public bool CheckRedDotForSectionCreation(ENUM_SECTION_TYPE sectiontype)
    {
        if (reddot_lstSectionCreation != null)
        {
            foreach (long idx in reddot_lstSectionCreation)
            {
                SectionTableData data = ManagementDataManager.Instance.GetSectionTableData(idx);
                if (data != null)
                {
                    if ((ENUM_SECTION_TYPE)data.Section_Type == sectiontype)
                        return true;
                }
            }
        }

        return false;
    }
    #endregion reddot section creation

    #region popup store receivable
        
    public void CheckGatherForCarryOverGoods()
    {
        long goodscount = 0;
        if(PlayerPrefs.HasKey(Constants.SAVE_KEY_POPUPSTORE_RECEIVABLE))
        {
            long.TryParse(PlayerPrefs.GetString(Constants.SAVE_KEY_POPUPSTORE_RECEIVABLE), out goodscount);
        }

        if(goodscount > 0)
        {
            SectionTableData scdata = 
                ManagementDataManager.Instance.GetSectionTableDataBySectionTypeAndSubType(
                    (byte)ENUM_SECTION_TYPE.Production, 
                    (byte)ENUM_SECTION_SUBTYPE_PRODUCT.PRODUCTION_POPUPSTORE);

            string strdesc = string.Empty;
            strdesc = string.Format(CResourceManager.Instance.GetString(91280215), goodscount);

            PlayerPrefs.DeleteKey(Constants.SAVE_KEY_POPUPSTORE_RECEIVABLE);
            var popupService = CCoreServices.GetCoreService<CPopupService>();
            var popup = popupService.Alert().GatherForCarryOverGoods(
                false, 
                CResourceManager.Instance.GetString(scdata.Section_Name),
                strdesc);
            popup.ShowAsObservable()
                .Finally(() =>
                {

                })
                .Subscribe()
                .AddTo(popup);
        }
    }

    #endregion popup store receivable

    public void Release()
    {
        GetBackAvatars();

        if(guideMailQuestMgr != null)
        {
            guideMailQuestMgr.Release();
            guideMailQuestMgr = null;
        }

        if (CamController != null)
        {
            CamController.Release();
            CamController = null;
        }

        if (MainSM != null)
        {
            MainSM.ReleaseStateMachine();
            MainSM = null;
        }

        if (buildingMgr != null)
        {
            buildingMgr.Release();
            buildingMgr = null;
        }
        if (gridMgr != null)
        {
            gridMgr.Release();
            gridMgr = null;
        }
        if (buildMgr != null)
        {
            buildMgr.Release();
            buildMgr = null;
        }
        if (sectionMgr != null)
        {
            sectionMgr.Release();
            sectionMgr = null;
        }

        if (BuildAreaGridPos != null) BuildAreaGridPos = null;
        if (SectionRootObj != null) SectionRootObj = null;
        if (SelectorRootObj != null) SelectorRootObj = null;
        if (SelectedSectionObj != null) SelectedSectionObj = null;
        if (SelectedSectionScript != null) SelectedSectionScript = null;

        if(AvatarMgr != null)
        {
            AvatarMgr.Release();
            AvatarMgr = null;
        }

        if(NpcMgr != null)
        {
            NpcMgr.Release();
            NpcMgr = null;
        }

        if(EVDoorMgr != null)
        {
            EVDoorMgr.Release();
            EVDoorMgr = null;
        }

        if (canvasMgr != null)
        {
            canvasMgr = null;
        }
        
		IsLoadFinishPageUI = false;

        if(reddot_lstSectionLevelup != null)
        {
            reddot_lstSectionLevelup.Clear();
            reddot_lstSectionLevelup = null;
        }

        if(reddot_lstSectionCreation != null)
        {
            reddot_lstSectionCreation.Clear();
            reddot_lstSectionCreation = null;
        }

        CRuntimeAnimControllerSwitchManager.Instance.ClearUsingClipNameDic();
    }
}



public class MailIcon
{
    public GameObject Obj;
    public Collider ObjCol;
    public UI3DScaler Scaler;
    public ButtonByRay ClickBtn;
    public QUEST_STATE QuestState;
    public long ID;
}
