#define _DEBUG_MSG_ENABLED
#define _DEBUG_WARN_ENABLED
#define _DEBUG_ERROR_ENABLED

#define UNITY_EDITOR

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
using UnityEngine.EventSystems;
using GroupManagement.ManagementEnums;
using Utils.UI;
using UniRx;


public class CManagementCameraController : MonoBehaviour
{
    private ManagementWorldManager worldMgr;

    private Transform MyObj;
    private Camera MyCamera;

    private CamState _CamState;

    private CamMoveMode MoveMode;
    private CamMoveLockMode LockMode;
    private bool bLockAfterFocus;

    //----------------- ART Control --------------------
    public float FOV_MIN = 15;
    public float FOV_MAX = 28;
    public float ZOOMIN_Dist = -15;
    public float ZOOMOUT_Dist = -46;
    //-------------------------------------------------------
    private float fovDestDist;

    //Move
    private Vector3 DefaultCamPos;
    private const int CAM_POS_INC_VAL = 3;//by Constants.GridSurfaceY
    private float FirstCamDistance;
    private Vector3 MouseStart;
    private Vector3 MouseMove;
    [HideInInspector]
    public float smoothing = 1;
    private Vector3 camPrePos;
    private Vector3 camCurPos;
    private Rect defaultLimitRect;
    private float SCREEN_WIDTH_LIMITPOS_L;
    private float SCREEN_WIDTH_LIMITPOS_R;
    private float SCREEN_HIGHT_LIMITPOS_U;
    private float SCREEN_HIGHT_LIMITPOS_D;
    private float screenWidth;
    private float screenHeight;
    private float limitPos_leftVal;
    private float limitPos_rightVal;
    private float limitPos_upVal;
    private float limitPos_btmVal;
    private float MoveInputYpos;
    public GameObject CamDefaultPos_Dummy;
    private float BuildingMidPosX;

    //Extending Move
    private bool bExtend;
    [HideInInspector]
    public float exSmooth = 0.05f;

    //Bouncing
    private bool bBoucing;
    private Rect bouncingBorder;
    private float BOUNCE_WIDTH_POS;
    private float BOUNCE_HIGHT_POS;

    //Zoom
    private int preTouchCount;
    private float zoomVar = 0.06f;
    private bool bZoomStart;
    private float zoomZValue;
    private Vector2 PreMidPosBtnTouch;
    private bool IsBlockPinchZoom;

    private float preTabTime;
    private Vector3 preTabPos;
    private const float DOUBLE_TAB_TIME = 0.25f;

    //below use when room focused
    private Vector3 DoubleTabObjPosition;
    private DoubleTabState _DoubleTabState;
    private const float DOUBLETAB_ZOOM_TIME = 0.2f;
    private bool CurDBTabSectionZoomIn;
     
    private bool IsPinchZoom;

    private bool bStartFocusToObj;
    private Vector3 FocusTargetPos;

    //Touch
    private Vector3 preMyObjPos;
    PointerEventTarget eventTgt;

#if UNITY_EDITOR
    private float minGap;
    private float heightForMin;
#endif


    private CameraActionState _CamActionState;
    public CameraActionState CamActionState
    {
        get { return _CamActionState; }
        set { _CamActionState = value; }
    }

    public DoubleTabState DoubleTab_State
    {
        get { return _DoubleTabState; }
        set { _DoubleTabState = value; }
    }

    private const float PICKOBJ_DIST = 0.2f;

    //----------------- ART Control --------------------
    public float GAP_CAMLIMIT_RIGHT = 12;
    public float GAP_CAMLIMIT_UP = -1;
    public float GAP_CAMLIMIT_BOTTOM = 5;

    //Bottom Limit Line when Zoom IN
    //Limit Line is move GAP_CAMLIMIT_BOTTOM to GAP_CAMLIMIT_MIN_BOTTOM. When Zoom In/Out
    public float GAP_CAMLIMIT_MIN_BOTTOM = 4;
    //---------------------------------------------------
    [HideInInspector]
    public float camLimit_bottom_Min;
    private float ZOOM_RATIO_COEF;
    private float CurrentCamZoomRatio;

    private const int VISIBLE_CAMUPSIDELIMIT = 5;

    private void Awake()
    {
        MyObj = transform;
        MyCamera = MyObj.GetComponent<Camera>();


        //Debug.Log("    camera viewport Rect = "+MyCamera.ScreenToViewportPoint);
    }

    public void Initialize()
    {
        eventTgt = new PointerEventTarget();
        worldMgr = ManagementManager.Instance.worldManager;
        _CamState = CamState.NONE;
        MoveMode = CamMoveMode.NORMAL;
        LockMode = CamMoveLockMode.UNLOCK;
        _CamActionState = CameraActionState.ACTION;

        InitEnvironmentPosition(worldMgr.SectionRootObj);

        bExtend = false;
        bBoucing = false;
        bZoomStart = false;
        zoomZValue = FirstCamDistance;
        bStartFocusToObj = false;
        bLockAfterFocus = false;
        FocusTargetPos = Vector3.zero;
        MoveInputYpos = 0.0f;
        IsBlockPinchZoom = false;

        _DoubleTabState = DoubleTabState.ZOOM_NOT;

        preTabTime = Time.time;

        SetZoomRatioCoef();

        CheckDoubleTab();
    }

    public void InitEnvironmentPosition(Transform rootObj)
    {
        //Debug.Log("InitEnvironmentPosition() Width = "+Screen.width+"/"+Screen.height);
        screenWidth = Screen.width;
        screenHeight = Screen.height;

        float screenRatio = (screenWidth / screenHeight);
        if (screenRatio < 1.5f)
        {
            ZOOMIN_Dist = -12;
            ZOOMOUT_Dist = -65;
        }

        MyCamera.fieldOfView = FOV_MAX;
        BuildingMidPosX = 0.0f;

        Vector3 fovMin = new Vector3(0, 0, FOV_MIN);
        Vector3 fovMax = new Vector3(0, 0, FOV_MAX);
        fovDestDist = (fovMax - fovMin).sqrMagnitude;

        SetCameraDefaultPos();


    }

    public void SetCameraDefaultPos()
    {
        GameObject parentObj = transform.parent.Find("World/Building/root_bottomfloor").gameObject;
        //CamDefaultPos_Dummy = GameObjectHelperUtils.FindGameObjectWithTagInChildren(parentObj, Management_LayerTag.Tag_CAMERA_MID_DUMMY);
        CamDefaultPos_Dummy = parentObj.transform.Find("room_dept_BD_floor/room_dept_BD_floor/camera").gameObject;
        if (CamDefaultPos_Dummy == null)
        {
            CDebug.LogError("ManagementCameraController doesn't have camera dummy object");
            return;
        }


        defaultLimitRect = new Rect(0, 0, 0, 0);
        float camXPos = CamDefaultPos_Dummy.transform.position.x;
        float camYPos = CamDefaultPos_Dummy.transform.position.y;

        if (BuildingMidPosX == 0.0f)
        {
            BuildingMidPosX = camXPos;
        }

        DefaultCamPos = new Vector3(camXPos, camYPos, ZOOMOUT_Dist); // Constants.GridSurfaceY ==0. non under ground
        MyObj.position = DefaultCamPos;

        FirstCamDistance = MyObj.position.z;  // Distance camera is above map
    }

    public void SetCameraDefaultPosAsSaved()
    {
        MyObj.position = DefaultCamPos;
    }

    public void SetCameraMovingLimit()
    {
        /*
         * In 3D perspective, Camera Z position is important
         * It returns differnt Point by this camera z position
         */
        Vector3 lb = MyCamera.ScreenToWorldPoint(new Vector3(0, 0, -MyObj.position.z));
        Vector3 rb = MyCamera.ScreenToWorldPoint(new Vector3(screenWidth, 0, -MyObj.position.z));

        float camLimit_left = lb.x - GAP_CAMLIMIT_RIGHT - 3;
        float camLimit_right = rb.x + GAP_CAMLIMIT_RIGHT + 4;
        float camLimit_bottom = lb.y - GAP_CAMLIMIT_BOTTOM;
        camLimit_bottom_Min = lb.y - GAP_CAMLIMIT_MIN_BOTTOM;
        //float camLimit_bottom = lb.y - (GAP_CAMLIMIT - 2 - 5);
#if UNITY_EDITOR
        minGap = GAP_CAMLIMIT_BOTTOM - GAP_CAMLIMIT_MIN_BOTTOM;
        //heightForMin = (SCREEN_HIGHT_LIMITPOS_U - SCREEN_HIGHT_LIMITPOS_D);
#endif

        float camLimit_Up = GetDefaultCamUpsideLimit();

        //defaultLimitRect = new Rect(L, R, U, D);//
        defaultLimitRect = new Rect(camLimit_left, camLimit_right, GAP_CAMLIMIT_UP, camLimit_bottom);//L, R, U, D
        SetMovingLimit(camLimit_Up);
        SetLimitValue(); ;
    }



#if UNITY_EDITOR //Set By Tool menu
    public void SetCameraLimitRect(float right, float up, float bottom, float btm_min)
    {
        if (_DoubleTabState == DoubleTabState.ZOOM_IN_STAY)
        {
            Toast.MakeTextForContent("줌 아웃상태에서 셋팅하세요", Msg.MSG_TYPE.Notice);
            return;
        }
        bExtend = false;
        bBoucing = false;
        SetCameraDefaultPos();

        Vector3 lb = MyCamera.ScreenToWorldPoint(new Vector3(0, 0, -MyObj.position.z));
        Vector3 rb = MyCamera.ScreenToWorldPoint(new Vector3(screenWidth, 0, -MyObj.position.z));

        GAP_CAMLIMIT_RIGHT = right;
        GAP_CAMLIMIT_UP = up;
        GAP_CAMLIMIT_BOTTOM = bottom;
        GAP_CAMLIMIT_MIN_BOTTOM = btm_min;

        minGap = GAP_CAMLIMIT_BOTTOM - GAP_CAMLIMIT_MIN_BOTTOM;
        heightForMin = (SCREEN_HIGHT_LIMITPOS_U - SCREEN_HIGHT_LIMITPOS_D);

        float camLimit_left = lb.x - GAP_CAMLIMIT_RIGHT;
        float camLimit_right = rb.x + GAP_CAMLIMIT_RIGHT;
        float camLimit_bottom = lb.y - GAP_CAMLIMIT_BOTTOM;
        camLimit_bottom_Min = lb.y - GAP_CAMLIMIT_MIN_BOTTOM;
        //float camLimit_bottom = lb.y - (GAP_CAMLIMIT - 2 - 5);

        float camLimit_Up = GetDefaultCamUpsideLimit();

        //defaultLimitRect = new Rect(L, R, U, D);//
        defaultLimitRect = new Rect(camLimit_left, camLimit_right, GAP_CAMLIMIT_UP, camLimit_bottom);//L, R, U, D
        SetMovingLimit(camLimit_Up);
        SetLimitValue(); ;
    }
#endif

    public void UpdateCamMovingLimit()
    {
        SetExtendFloorMovingLimit();
    }

    public void SetExtendFloorMovingLimit()
    {
        //One Floor Extend
        float extendVal = GetDefaultCamUpsideLimit() + worldMgr.GridElementHeight;
        SetMovingLimit(extendVal);
    }

    private void SetMovingLimit(float extendValue)
    {
        SCREEN_WIDTH_LIMITPOS_L = defaultLimitRect.x;
        SCREEN_WIDTH_LIMITPOS_R = defaultLimitRect.y;
        SCREEN_HIGHT_LIMITPOS_U = defaultLimitRect.width + extendValue;
        SCREEN_HIGHT_LIMITPOS_D = defaultLimitRect.height;

#if UNITY_EDITOR
        heightForMin = (SCREEN_HIGHT_LIMITPOS_U - SCREEN_HIGHT_LIMITPOS_D);
#endif

        SetLimitValue();
    }

    public void SetLimitValue()
    {
        float zVal = -MyObj.position.z;
        Vector3 leftBtm = MyCamera.ScreenToWorldPoint(new Vector3(0, 0, zVal));
        Vector3 rightTop = MyCamera.ScreenToWorldPoint(new Vector3(screenWidth, screenHeight, zVal));

        limitPos_leftVal = MyObj.position.x - (leftBtm.x - SCREEN_WIDTH_LIMITPOS_L);
        limitPos_rightVal = MyObj.position.x + (SCREEN_WIDTH_LIMITPOS_R - rightTop.x);
        limitPos_upVal = MyObj.position.y + (SCREEN_HIGHT_LIMITPOS_U - rightTop.y);
        //limitPos_btmVal = MyObj.position.y - (leftBtm.y - SCREEN_HIGHT_LIMITPOS_D);
        SetLimitBottomValue();
    }

    public void SetLimitBottomValue()
    {
        float zVal = -MyObj.position.z;
        Vector3 leftBtm = MyCamera.ScreenToWorldPoint(new Vector3(0, 0, zVal));
        limitPos_btmVal = MyObj.position.y - (leftBtm.y - SCREEN_HIGHT_LIMITPOS_D);
    }

    private float GetDefaultCamUpsideLimit()
    {
        /*
         * why (WorldMgr.GetTopFloor() + VISIBLE_CAMUPSIDELIMIT)??
         * Because of roof must be shown
         */
        return (worldMgr.GetTopFloor() + VISIBLE_CAMUPSIDELIMIT + Constants.GridSurfaceY) * worldMgr.GridElementHeight;
    }

    IObservable<long> DoubleTabStream;
    private void CheckDoubleTab()
    {
        return;
        DoubleTabStream = Observable.EveryUpdate()
            .Where(_ => Input.GetMouseButtonUp(0));

        DoubleTabStream.Buffer(DoubleTabStream.Throttle(TimeSpan.FromMilliseconds(400)))
            .Where(xs => xs.Count == 2)
            .Subscribe(xs =>
            {
                //CDebug.Log("    @@@@@   DoubleClick Detected! CamActionState:" + CamActionState);
                if (CamActionState == CameraActionState.ACTION)
                {
                    if (/*xs.Count == 2 && */IsShootRayToSection)
                    {
                        //CDebug.Log("    @@@@@   DoubleClick Detected! Count:" + xs.Count);
                        SetDoubleTabZoom();
                    }
                }
            });
    }

    private bool IsDoubleTab()
    {
        //CDebug.Log($"%%%%% Double Tab BLOCK!!!!! 0000         CamActionState={CamActionState} ");
        if (CamActionState != CameraActionState.ACTION)
        {
            return false;
        }
        
        if (Input.GetMouseButtonDown(0))
        {
            float curTabTime = Time.time - preTabTime;

            //Debug.Log("    ^^^^    IsDoubleTab() " + curTabTime+"/"+ preTabTime + "/"+ IsShootRayToSection);

            if (curTabTime <= DOUBLE_TAB_TIME && IsShootRayToSection)
            {
                //It's not double tab, touch distance between prev and cur is longer section size.
                Vector3 CurTouchPnt0 = new Vector3(Input.mousePosition.x, -Input.mousePosition.y, 0);
                Vector3 CurTouchPnt = MyCamera.ScreenToWorldPoint(CurTouchPnt0);
                float tabDist = Vector3.Distance(CurTouchPnt, preTabPos);
               // CDebug.Log($"    ^^^^    Double Tab 00000!! tabDist={tabDist} / worldMgr.GridElementWidth = {worldMgr.GridElementWidth}");
                //Need to Change size as selected section size instead WorldMgr.GridElementWidth
                if (tabDist < (worldMgr.GridElementWidth * 3))
                {
                    //CDebug.Log("    ^^^^    Double Tab 11111!!");
                    return true;
                }
            }
            else
            {
                //First Tab
                Vector3 preTabPos0 = new Vector3(Input.mousePosition.x, -Input.mousePosition.y, 0);
                preTabPos = MyCamera.ScreenToWorldPoint(preTabPos0);
                preTabTime = Time.time;
                //Debug.Log("    ^^^^    IsDoubleTab() preTabTime = " + preTabTime);
                return false;
            }
        }

        return false;
    }

    //private bool IsDoubleTab = false;
    // Update is called once per frame
    void LateUpdate()
    {
        //CDebug.Log("    ^^^^    LateUpdate !!!!!!");
        //Network Loading Block
        //if (NetworkLoadingProgress.IsActive)
        //{
        //    CDebug.Log("    ^^^^    NetworkLoadingProgress.IsActive TRUE !!!!!!");
        //    return;
        //}


        //When UI Touch, Camera Stop
        //if (EventSystem.current.IsPointerOverGameObject())
        if (IsPointerOverUIObject())
        {
            CDebug.Log("    ^^^^    Pointer is over on UI !!!!!!");
            return;
        }

        if (IsDoubleTab())
        {
            CDebug.Log("    ^^^^    DOUBLE TAB !!!!!!");
            SetDoubleTabZoom();
            return;
        }


        if (GetCameraLockMode() == CamMoveLockMode.LOCK)
            return;

        //Debug.Log("LateUpdate _CamActionState State = " + _CamActionState);
        switch (_CamActionState)
        {
            case CameraActionState.ACTION:

                if (Input.touchCount < 2)
                {
                    if(IsPinchZoom)
                    {
                        IsPinchZoom = false;
                    }

                    ProcCamAction(_DoubleTabState);

                    if (bZoomStart)
                    {
                        bZoomStart = false;
                    }

                }
                else if (Input.touchCount == 2)
                {
                    if(IsPinchZoom == false)
                    {
                        IsPinchZoom = true;
                    }

                    if (bExtend)
                    {
                        bExtend = false;
                    }

                    ZoomCamera();
                }

                #if UNITY_EDITOR                    
                if(Input.GetKeyDown(KeyCode.Z))
                {
                    if (IsPinchZoom == false)
                    {
                        IsPinchZoom = true;
                    }
                }
                //                ZoomScrollWheel();
                #endif
                break;
            case CameraActionState.ACTION_STOP_PREPARE:
                ProcCamAction(_DoubleTabState);
                break;
            case CameraActionState.ACTION_PREPARE:
                ProcCamAction(_DoubleTabState);
                break;
        }

    }

    private void FixedUpdate()
    {
        if (bStartFocusToObj == false)
        {
            switch (_CamActionState)
            {
                case CameraActionState.ACTION:
                    if (bExtend)
                    {
                        MoveCameraExtendFoward();
                    }

                    if (bBoucing)
                    {
                        BounceCamera();
                    }
                    break;
            }
        }
    }

    public void SetStopExtraMove()
    {
        bExtend = false;
        bBoucing = false;
    }

    #region SET

    #endregion SET


    #region PROC

    /// <summary>
    /// ProcCamAction is Camera action process Move and Zoom by Focused
    /// </summary>
    /// <param name="state">DoubleTabState(deleted doubletab)</param>
    public void ProcCamAction(DoubleTabState state)
    {
        //Debug.Log("ProcCamAction double Tab State = "+state);
        switch (state)
        {
            case DoubleTabState.ZOOM_NOT:
            case DoubleTabState.ZOOM_IN_STAY:
                if (bStartFocusToObj)
                {
                    FocusedToObject();
                }
                else
                {
                    Move();
                }
                break;
            case DoubleTabState.ZOOM_IN_BEGAN:
                Zoom_FocusedToSectionCenter(DoubleTabObjPosition, ZOOMIN_Dist, DoubleTabState.ZOOM_IN_STAY);
                break;
            case DoubleTabState.ZOOM_OUT_BEGAN:
                Zoom_FocusedToSectionCenter(DoubleTabObjPosition, ZOOMOUT_Dist, DoubleTabState.ZOOM_NOT);
                break;
        }
    }


    #endregion PROC

    #region MOVE
    private void Move()
    {
        //if (TutorialManager.Instance.IsTutorial)
        //{
        //    if (Input.GetMouseButtonUp( 0 ))
        //    {
        //        if (_CamState == CamState.MOVE)
        //        {
        //            _CamState = CamState.NONE;
        //            //Cell touch
        //            if (CanTouchObject())
        //            {
        //                TouchSection();
        //            }
        //        }
        //    }
        //    return;
        //}
        
        if (Input.GetMouseButtonDown(0))
        {
            //if (preTouchCount < 2)
            {
                //Debug.Log("Move GetMouseButtonDown(0)");
                _CamState = CamState.MOVE;
                preMyObjPos = MyObj.position;
                FirstCamDistance = zoomZValue = MyObj.position.z;

                MoveInputYpos = -Input.mousePosition.y;

                MouseStart = new Vector3(-Input.mousePosition.x, MoveInputYpos, FirstCamDistance);
                MouseStart = MyCamera.ScreenToWorldPoint(MouseStart);
                MouseStart.z = MyObj.position.z;
                //Debug.Log("####   MouseStart = "+ MouseStart);

                camPrePos = MyObj.position;
                preTouchCount = Input.touchCount;
                bExtend = false;
                bouncingBorder = Rect.zero;
                bBoucing = false;

            }
        }
        else if (Input.GetMouseButton(0))
        {
            if (preTouchCount < 2 && _CamState == CamState.MOVE)
            {
                //Debug.Log("Move GetMouseButton(0)");
                camPrePos = MyObj.position;

                //only horrizontal move in Interior mode
                //if(IsMoveHorrizontally() == false)
                //{
                MoveInputYpos = -Input.mousePosition.y;
                //}

                MouseMove = new Vector3(-Input.mousePosition.x, MoveInputYpos, FirstCamDistance);
                MouseMove = MyCamera.ScreenToWorldPoint(MouseMove);
                MouseMove.z = MyObj.position.z;
                //Debug.Log("####   MouseMove = " + MouseMove);

                Vector3 fPos = (MouseMove - MouseStart);
                //Debug.Log("####   fPos = " + fPos);

                //Smooth Follow
                camCurPos = (MyObj.position - fPos);
                camCurPos.z = FirstCamDistance;
                //Debug.Log("####    camCurPos = " + camCurPos);
                MoveCameraSmoothly(camCurPos, smoothing);

            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (preTouchCount < 2 && _CamState == CamState.MOVE)
            {
                _CamState = CamState.NONE;
                //Debug.Log("Move GetMouseButtonUp(0)");
                //Cell touch
                if (CanTouchObject())
                {
                    TouchSection();
                }
                //Move State
                else
                {
                    if (!bBoucing)
                    {
                        bExtend = true;
                    }
                }
            }
        }
    }

    private void MoveCameraSmoothly(Vector3 finishPos, float t)
    {
        Vector3 smoothPos = Vector3.Lerp(MyObj.position, finishPos, t);
        smoothPos.z = zoomZValue;
        MyObj.position = smoothPos;
        CameraMoveLimitly();
    }


    private void CameraMoveLimitly()
    {
        //CDebug.Log(" **** Management Camera Move limitly *****");

        float cx = Mathf.Clamp(MyObj.position.x, limitPos_leftVal, limitPos_rightVal);
        float cy = Mathf.Clamp(MyObj.position.y, limitPos_btmVal, limitPos_upVal);

        MyObj.position = new Vector3(cx, cy, MyObj.position.z);
    }

    private void MoveCameraExtendFoward()
    {
        Vector3 exPos = camCurPos + (camCurPos - camPrePos) * 10;
        MoveCameraSmoothly(exPos, exSmooth);

        bool bBouncingArea = isStartBouncing();

        if (bBouncingArea)
        {
            bExtend = false;
            bBoucing = true;
        }

        float dist = Vector3.Distance(MyObj.position, exPos);
        if (dist <= 0.5f)
        {
            bExtend = false;
        }
    }

    #endregion MOVE


    #region BOUNCING
    //Start Boucing or not
    private bool isStartBouncing()
    {
        bouncingBorder = CheckBouncing();

        if (bouncingBorder.x != 0 || bouncingBorder.width != 0 || bouncingBorder.y != 0 || bouncingBorder.height != 0)
        {
            return true;
        }

        return false;
    }

    //Checking Camera ViewPort is in Bouncing area
    private Rect CheckBouncing()
    {
        Rect rect = new Rect(0, 0, 0, 0);//-x, x, -y, y

        if (MyObj.position.x >= limitPos_leftVal && MyObj.position.x <= (limitPos_leftVal + 1))
        {
            rect.x = (limitPos_leftVal + 1);
        }
        else if (MyObj.position.x <= limitPos_rightVal && MyObj.position.x >= (limitPos_rightVal - 1))
        {
            rect.width = (limitPos_rightVal - 1);
        }

        if (MyObj.position.y <= limitPos_btmVal && MyObj.position.y >= (limitPos_btmVal - 1))
        {
            rect.y = (limitPos_btmVal + 1);
        }
        else if (MyObj.position.y <= limitPos_upVal && MyObj.position.y >= (limitPos_upVal - 1))
        {
            rect.height = (limitPos_upVal - 1);
        }

        return rect;
    }
    private void BounceCamera()
    {
        Vector3 bncPos = GetPosOfBouncingBorderEdge();

        MyObj.position = Vector3.Lerp(MyObj.position, bncPos, 0.05f);
    }

    private Vector3 GetPosOfBouncingBorderEdge()
    {
        Vector3 edgePos = new Vector3();

        if (bouncingBorder.x != 0)
        {
            edgePos.x = bouncingBorder.x;
        }
        else if (bouncingBorder.width != 0)
        {
            edgePos.x = bouncingBorder.width;
        }

        if (bouncingBorder.y != 0)
        {
            edgePos.y = bouncingBorder.y;
        }
        else if (bouncingBorder.height != 0)
        {
            edgePos.y = bouncingBorder.height;
        }

        //set pos if edgePos's empty
        if (edgePos.x == 0)
        {
            edgePos.x = MyObj.position.x;
        }
        if (edgePos.y == 0)
        {
            edgePos.y = MyObj.position.y;
        }

        edgePos.z = MyObj.position.z;

        return edgePos;
    }
    #endregion BOUNCING


    #region ZOOM
    private void ZoomCamera()
    {
        if(IsBlockPinchZoom)
        {
            return;
        }

        Touch touch_1st = Input.GetTouch(0);
        Touch touch_2nd = Input.GetTouch(1);

        if (touch_1st.phase == TouchPhase.Began || touch_1st.phase == TouchPhase.Stationary)
        {
            bExtend = false;
            preTouchCount = Input.touchCount;
        }

        _CamState = CamState.ZOOM;

        //if (bZoom == false)
        //{
        //    bZoom = true;
        //}

        //OrthograhicSize -----------------------------------------------------------------------
        Vector2 preTouch_1stPos = touch_1st.position - touch_1st.deltaPosition;
        Vector2 preTouch_2ndPos = touch_2nd.position - touch_2nd.deltaPosition;

        float preMagnitude = (preTouch_1stPos - preTouch_2ndPos).magnitude;
        float curMagnitude = (touch_1st.position - touch_2nd.position).magnitude;

        float gapMagnitude = curMagnitude - preMagnitude;

        float changeVal = (gapMagnitude * zoomVar);

        Vector3 zoomPos = MyObj.position;

        zoomZValue = Mathf.Clamp(MyObj.position.z + changeVal, ZOOMOUT_Dist, ZOOMIN_Dist);
        zoomPos.z = zoomZValue;

        MyObj.position = zoomPos;

        ProcFOVByZoom();
        //ProcBottomLimitByZoom();

        ZoomMoving(touch_1st, touch_2nd, gapMagnitude);

    }

    public void SetIsBlockPinchZoom(bool block)
    {
        IsBlockPinchZoom = block;
    }

    public bool GetIsBlockPinchZoom()
    {
        return IsBlockPinchZoom;
    }

    public float GetCameraMoveRatioByZoom()
    {
        Vector3 zoomIn = new Vector3(0, 0, ZOOMIN_Dist);
        Vector3 zoomOut = new Vector3(0, 0, ZOOMOUT_Dist);

        Vector3 MyObjZPos = new Vector3(0, 0, MyObj.position.z);

        float destDist = (zoomOut - zoomIn).sqrMagnitude;
        float curDist = (/*MyObj.position*/MyObjZPos - zoomIn).sqrMagnitude;
        //Debug.Log("GetCameraMoveRatioByZoom() destDist = " + destDist +"// out = " + zoomOut + " / in = "+ zoomIn);
        //Debug.Log("GetCameraMoveRatioByZoom() curDist = " + curDist + "// out = " + MyObj.position + " / in = " + zoomIn);

        return ((destDist - curDist) / destDist);
    }

    private void ProcFOVByZoom()
    {
        if (FOV_MIN == FOV_MAX)
        {
            return;
        }

        Vector3 zoomIn = new Vector3(0, 0, ZOOMIN_Dist);
        Vector3 zoomOut = new Vector3(0, 0, ZOOMOUT_Dist);

        float destDist = (zoomOut - zoomIn).sqrMagnitude;
        float curDist = (MyObj.position - zoomIn).sqrMagnitude;

        float camMoveRatio = curDist / destDist;
        //float camMoveRatio = GetCameraMoveRatioByZoom();

        //Vector3 fovMin = new Vector3(0, 0, FOV_MIN);
        //Vector3 fovMax = new Vector3(0, 0, FOV_MAX);
        //Vector3 fovCur = new Vector3(0, 0, MyCamera.fieldOfView);

        float destFov = (FOV_MAX - FOV_MIN) * camMoveRatio;
        //Debug.Log("destFov = " + destFov);

        MyCamera.fieldOfView = 7.5f + destFov;
    }

    private void ProcBottomLimitByZoom()
    {
        float camMoveRatio = GetCameraMoveRatioByZoom();
        float destValue = (/*defaultLimitRect.height*/SCREEN_HIGHT_LIMITPOS_D - camLimit_bottom_Min) * camMoveRatio;
        //Debug.Log("ProcBottomLimitByZoom() camMoveRatio = " + camMoveRatio + " / destVal = " + destValue + "/ HIGHT = " + SCREEN_HIGHT_LIMITPOS_D);
        //Debug.Log("ProcBottomLimitByZoom() destVal = " + destValue + " / "+ (defaultLimitRect.height - camLimit_bottom_Min) + " / camMoveRatio = " + camMoveRatio+"/ HIGHT = " + SCREEN_HIGHT_LIMITPOS_D);
        SCREEN_HIGHT_LIMITPOS_D -= destValue;

        SetLimitBottomValue();
    }

    private void ZoomMoving(Touch t1, Touch t2, float magnitude)
    {
        if (!bZoomStart)
        {
            bZoomStart = true;
            //bMaxZoomInFocused = false;
            PreMidPosBtnTouch = (GetWorldPositionOnPlane(t1.position, 0) + GetWorldPositionOnPlane(t2.position, 0)) * 0.5f;
        }
        Vector2 midPos = (GetWorldPositionOnPlane(t1.position, 0) + GetWorldPositionOnPlane(t2.position, 0)) * 0.5f;
        Vector3 zoomGap = (midPos - PreMidPosBtnTouch);


        MyObj.position -= zoomGap;
        SetLimitValue();
        CameraMoveLimitly();
        SetRatioByCameraZoom();
    }

    private void SetCameraMoveModeByZoomOut()
    {
        if (MyObj.position.z < ZOOMIN_Dist)
        {
            if (GetCameraMoveMode() == CamMoveMode.INFO)
            {
                SetCameraMoveMode(CamMoveMode.NORMAL);
                worldMgr.ChangeState(ManagementWorldState_Common.Instance());
            }
        }
    }

#if UNITY_EDITOR
    private void ZoomScrollWheel()
    {
        float scrollWheelValue = Input.GetAxis("Mouse ScrollWheel");

        //if (scrollWheelValue == 0 || IsMoveHorrizontally())
        //    return;

        Vector3 pos = MyObj.position;

        pos.z = MyObj.position.z + (20 * scrollWheelValue);
        if (pos.z >= ZOOMIN_Dist)
        {
            pos.z = ZOOMIN_Dist;
        }
        else if (pos.z <= ZOOMOUT_Dist)
        {
            pos.z = ZOOMOUT_Dist;
        }

        ProcFOVByZoom();

        MyObj.position = pos;
        SetLimitValue();
        SetCameraMoveModeByZoomOut();
    }
#endif

    public CamMoveMode GetCameraMoveMode()
    {
        return MoveMode;
    }

    public void SetCameraMoveMode(CamMoveMode mode)
    {
        MoveMode = mode;
    }

    public void SetCameraLockMode(CamMoveLockMode mode)
    {
        LockMode = mode;
    }

    public CamMoveLockMode GetCameraLockMode()
    {
        return LockMode;
    }

    private void Zoom_FocusedToSectionCenter(Vector3 targetPos, float DIST, DoubleTabState state)
    {
        Vector3 targetPosition = new Vector3(targetPos.x, targetPos.y, DIST);
        MyObj.position = Vector3.Lerp(MyObj.position, targetPosition, DOUBLETAB_ZOOM_TIME);

        //SetFov(targetPosition);
        SetFovByZoomState();
        
        //SetBottomLimitByZoomState();
        SetLimitValue();

        CameraMoveLimitly();
        SetRatioByCameraZoom();

        if (GetCamReachAroundDestination(MyObj.position.z, DIST))
        {
            //CDebug.Log($"    ^^^^    after [GetCamReachAroundDestination] _CamActionState = {_CamActionState}");
            switch (_CamActionState)
            {
                case CameraActionState.ACTION_PREPARE:
                    _CamActionState = CameraActionState.ACTION;
                    SetCameraMoveMode(CamMoveMode.NORMAL);
                    break;
                case CameraActionState.ACTION:
                    //Debug.Log("pos = " + MyObj.position.z + "/" + DIST +" / double tap state = " + _DoubleTabState);
                    SetZoomCameraDTState(state);
                    break;
                case CameraActionState.ACTION_STOP_PREPARE:
                    //헤드헌팅, 이벤트
                    //bMaxZoomInFocused = true;
                    SetCameraMoveMode(CamMoveMode.INFO);
                    _CamActionState = CameraActionState.ACTION;
                    break;
            }
            IsShootRayToSection = false;
        }
    }

    private void SetZoomRatioCoef()
    {
        float outDist = Mathf.Abs(ZOOMOUT_Dist);
        float inDist = Mathf.Abs(ZOOMIN_Dist);

        ZOOM_RATIO_COEF = 1 / (outDist - inDist);
        CurrentCamZoomRatio = 1;//default
    }

    public void SetRatioByCameraZoom()
    {
        float inDist = Mathf.Abs(ZOOMIN_Dist);
        float curCamZpos = Mathf.Abs(MyObj.position.z) - inDist;

        CurrentCamZoomRatio = Mathf.Round((curCamZpos * ZOOM_RATIO_COEF) * 100) * 0.01f;

        //CDebug.Log($"           ************ [SetRatioByCameraZoom] Ratio = {CurrentCamZoomRatio}");
    }

    public float GetRatioByCameraZoom()
    {
        return CurrentCamZoomRatio;
    }

    public bool IsZommingInCamera()
    {
        switch (_DoubleTabState)
        {
            case DoubleTabState.ZOOM_IN_BEGAN:
                return true;
        }

        if(IsPinchZoom)
        {
            return true;
        }

        return false;
    }


    public bool IsZommingInOutCamera()
    {
        switch (_DoubleTabState)
        {
            case DoubleTabState.ZOOM_IN_BEGAN:
            case DoubleTabState.ZOOM_OUT_BEGAN:
            //case DoubleTabState.ZOOM_PINCH:
                //CDebug.Log($"          [ManagementCamManager] IsZommingInOutCamera() / ratio = {GetRatioByCameraZoom()}");
                return true;
        }

        if(bStartFocusToObj)
        {
            return true;
        }

        if (IsPinchZoom)
        {
            return true;
        }



        return false;
    }

    //모든 focus는 zoom in상태로 한다    
    private void FocusedToObject()
    {
        MyObj.position = Vector3.Lerp(MyObj.position, FocusTargetPos, DOUBLETAB_ZOOM_TIME);

        //Debug.Log("Cam bStartFocusToObj = "+ bStartFocusToObj+ "/_DoubleTabState = " + _DoubleTabState+ " / FocusTargetPos = " + FocusTargetPos+"/"+ ZOOMIN_Dist);
        SetRatioByCameraZoom();
        SetLimitValue();
        //Zoom In Until FocusTargetPos.z position
        //if (GetCamReachAroundDestination(MyObj.position.x, FocusTargetPos.x))
        if (GetCamReachAroundDestDistance(MyObj.position, FocusTargetPos))
        {
            bStartFocusToObj = false;
            SetCameraMoveMode(CamMoveMode.INFO);
            SetZoomCameraDTState(DoubleTabState.ZOOM_IN_STAY);

            if (bLockAfterFocus)
            {
                bLockAfterFocus = false;
                SetCameraLockMode(CamMoveLockMode.LOCK);
            }
        }
    }

    public bool CheckFocusedToObject()
    {
        return !bStartFocusToObj;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="targetPos"></param>
    /// <param name="_bLockAfterFocus"> True is set Camera Lock Mode 'LOCK' after finishing focus</param>
    public void SetFocusToPosition(Vector3 targetPos, bool _bLockAfterFocus = false)
    {
        bLockAfterFocus = _bLockAfterFocus;
        bStartFocusToObj = true;
        FocusTargetPos = targetPos;
        SetZoomCameraDTState(DoubleTabState.ZOOM_NOT);
    }

    public bool IsFinishFucusToObject()
    {
        return bStartFocusToObj;
    }

    public void SetFocusToObject(bool bFocus)
    {
        bStartFocusToObj = bFocus;
    }

    public CameraActionState GetCameraActionState()
    {
        return _CamActionState;
    }

    private void SetFovByZoomState()
    {
        Vector3 curFov = new Vector3(0, 0, MyCamera.fieldOfView);
        Vector3 fov = Vector3.zero;
        switch (_DoubleTabState)
        {
            case DoubleTabState.ZOOM_IN_BEGAN:
                Vector3 fovMin = new Vector3(0, 0, FOV_MIN);
                fov = Vector3.Lerp(curFov, fovMin, DOUBLETAB_ZOOM_TIME);
                break;
            case DoubleTabState.ZOOM_OUT_BEGAN:
                Vector3 fovMax = new Vector3(0, 0, FOV_MAX);
                fov = Vector3.Lerp(curFov, fovMax, DOUBLETAB_ZOOM_TIME);
                break;
        }
        MyCamera.fieldOfView = fov.z;
    }

    //private void SetBottomLimitByZoomState()
    //{
    //    Vector3 curBtmLimit = new Vector3(0, 0, SCREEN_HIGHT_LIMITPOS_D);

    //    Vector3 resultValue = Vector3.zero;

    //    switch (_DoubleTabState)
    //    {
    //        case DoubleTabState.ZOOM_IN_BEGAN:
    //            Vector3 btmMin = new Vector3(0, 0, camLimit_bottom_Min);
    //            resultValue = Vector3.Lerp(curBtmLimit, btmMin, DOUBLETAB_ZOOM_TIME);
    //            break;
    //        case DoubleTabState.ZOOM_OUT_BEGAN:
    //            Vector3 btmMax = new Vector3(0, 0, defaultLimitRect.height);
    //            resultValue = Vector3.Lerp(curBtmLimit, btmMax, DOUBLETAB_ZOOM_TIME);
    //            break;
    //    }
    //    SCREEN_HIGHT_LIMITPOS_D = resultValue.z;
    //    //Debug.Log("SetBottomLimitByZoomState() SCREEN_HIGHT_LIMITPOS_D = "+ SCREEN_HIGHT_LIMITPOS_D);

    //    //SetLimitBottomValue();
    //    SetLimitValue();
    //}


    private void SetZoomCameraDTState(DoubleTabState state)
    {
        _DoubleTabState = state;
    }

    //private Vector3 GetZoomCamMovement(Vector3 targetPos, float camZPos)
    //{
    //    Vector3 targetPosition = new Vector3(targetPos.x, targetPos.y, camZPos);
    //    Vector3 zoomDir = (MyObj.position - targetPosition);
    //    Vector3 move = zoomDir.normalized * Time.deltaTime * zoomMoveSpeed;

    //    return move;
    //}

    private bool GetCamReachAroundDestination(float originPos, float destPos)
    {
        float DEST_AROUND = 0.1f;
        float objPosZval = Mathf.Abs(originPos);
        float limitVal = Mathf.Abs(destPos);

        float value = Mathf.Abs(limitVal - objPosZval);

        if (value <= DEST_AROUND)
        {
            return true;
        }
        return false;
    }

    private bool GetCamReachAroundDestDistance(Vector3 originPos, Vector3 destPos)
    {
        float DEST_AROUND = 0.1f;
        float value = Vector3.Distance(originPos, destPos);

        if (value <= DEST_AROUND)
        {
            return true;
        }

        return false;
    }

    public Vector3 GetWorldPositionOnPlane(Vector3 screenPosition, float z)
    {
        Ray ray = MyCamera.ScreenPointToRay(screenPosition);
        Plane xy = new Plane(Vector3.forward, new Vector3(0, 0, z));
        float distance;
        xy.Raycast(ray, out distance);
        return ray.GetPoint(distance);
    }
    #endregion ZOOM


    #region TOUCH
    private void TouchSection()
    {
        RayToSection();
    }

    public void SetDoubleTabObjectPosition(Vector3 position)
    {
        bBoucing = false;
        DoubleTabObjPosition = position;

        //Debug.Log("SetDoubleTabObjectPosition position = " + position + " / cam Pos = " + MyObj.position);
    }

    public void SetCameraCurrentPositionToZoom()
    {
        bBoucing = false;
        DoubleTabObjPosition = MyObj.position;
    }

    public void SetCameraZoomStateByZoom(bool bZoomIn)
    {
        //CDebug.LogError($"    ^^^^    [SetCameraZoomStateByZoom] zoomIn = {bZoomIn}");
        if (bZoomIn)
        {
            CamActionState = CameraActionState.ACTION_STOP_PREPARE;
            DoubleTab_State = DoubleTabState.ZOOM_IN_BEGAN;
        }
        else
        {
            CamActionState = CameraActionState.ACTION_PREPARE;
            DoubleTab_State = DoubleTabState.ZOOM_OUT_BEGAN;
        }
    }
    private bool IsShootRayToSection;
    private void RayToSection()
    {
        CDebug.Log("    ^^^^    JUST TOUCH RayToSection!!!!!!");
        RaycastHit[] hits;
        float rayDist = 100f;

        var sectionLayer = 1 << LayerMask.NameToLayer(Management_LayerTag.Layer_SECTION);
        var uiLayer = 1 << LayerMask.NameToLayer(Management_LayerTag.Layer_3DUI);

        var layerMask = sectionLayer | uiLayer;

        Ray cellTouchRay = MyCamera.ScreenPointToRay(Input.mousePosition);
        hits = Physics.RaycastAll(cellTouchRay, rayDist, layerMask);
        worldMgr.SetTouchedObjectByPriority(hits);
        IsShootRayToSection = true;
        InitIsShootRayToSection();
        CDebug.Log($"    ^^^^    JUST TOUCH RayToSection!!!!!!     IsShootRayToSection = {IsShootRayToSection}");
    }

    private void InitIsShootRayToSection()
    {
        var disposer = new SingleAssignmentDisposable();
        disposer.Disposable = Observable.Timer(TimeSpan.FromSeconds(DOUBLE_TAB_TIME))
        .Subscribe(_ =>
        {
            IsShootRayToSection = false;
            disposer.Dispose();
        })
        .AddTo(this);
    }

    private bool CanTouchObject()
    {
        float xMoveVal = Mathf.Abs(MyObj.position.x - preMyObjPos.x);
        float yMoveVal = Mathf.Abs(MyObj.position.y - preMyObjPos.y);

        if (CamActionState != CameraActionState.ACTION)
        {
            return false;
        }

        if (xMoveVal <= PICKOBJ_DIST && yMoveVal <= PICKOBJ_DIST)
        {
            //CDebug.LogError("    ^^^^    [CanTouchObject()] can touch");
            return true;
        }

        //CDebug.LogError("    ^^^^    [CanTouchObject()] can NOT touch");
        return false;
    }

    private void SetDoubleTabZoom()
    {
        SectionScript _sScript = worldMgr.GetSelectedSectionScript();
        GameObject _sObj = worldMgr.GetSelectedSectionObject();


        //CDebug.LogError($"    ^^^^    [SetDoubleTabZoom()] _sScript = {_sScript}, _sObj = {_sObj}");
        if (_sScript == null || _sObj == null)
        {
            return;
        }

        switch(_DoubleTabState)
        {
            case DoubleTabState.ZOOM_IN_STAY:
                CurDBTabSectionZoomIn = true;
                break;
            case DoubleTabState.ZOOM_NOT:
                CurDBTabSectionZoomIn = false;
                break;
        }

        CurDBTabSectionZoomIn = !CurDBTabSectionZoomIn;
        Vector3 _centerPos = worldMgr.GetSectionCenterPosition(_sScript, _sObj);
        SetDoubleTabObjectPosition(_centerPos);
        SetCameraZoomStateByZoom(CurDBTabSectionZoomIn);
        CDebug.LogError($"    ^^^^    [SetDoubleTabZoom()] CurDBTabSectionZoomIn = {CurDBTabSectionZoomIn}");
    }
    #endregion TOUCH


    /// <summary>
    /// IsPointerOverUIObject() is skip for all uis on 3d world used by 'PointerOverSkipper'
    /// </summary>
    private bool IsPointerOverUIObject()
    {
        if (Input.GetMouseButtonDown(0) || Input.touchCount == 2)
        {
            PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
            eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventDataCurrentPosition, results);

            //CDebug.Log($"    ^^^^    [IsPointerOverUIObject] results.Count = {results.Count} /// IsShootRayToSection = {IsShootRayToSection}");

            int overCount = 0;
            for(int i=0; i<results.Count; ++i)
            {
                RaycastResult result = results[i];
                //CDebug.Log($"    ^^^^    [IsPointerOverUIObject] result = {result.gameObject}");
                //SpriteRenderer sr = result.gameObject.GetComponent<SpriteRenderer>();
                NetworkLoadingProgress nlp = result.gameObject.GetComponent<NetworkLoadingProgress>();
                PointerOverSkipper skipper = result.gameObject.GetComponent<PointerOverSkipper>();
                //if (nlp == null && skipper == null)
                if (skipper == null)
                {
                    //CDebug.Log($"    ^^^^    [IsPointerOverUIObject] sr = {sr.gameObject}");
                    overCount ++;
                }
            }

            //CDebug.Log($"    ^^^^    [IsPointerOverUIObject] overCount = {overCount}");

            //return results.Count > 0;
            return overCount > 0;
        }

        return false;
    }

    public Rect GetScreenRect()
    {
        
        float zVal = 0f;
        if (MyObj != null)
            zVal = -MyObj.position.z;       
        
        
        Vector3 lb = MyCamera.ScreenToWorldPoint(new Vector3(0, 0, zVal));
        Vector3 lt = MyCamera.ScreenToWorldPoint(new Vector3(0, screenHeight, zVal));
        Vector3 rt = MyCamera.ScreenToWorldPoint(new Vector3(screenWidth, screenHeight, zVal));
        Rect screenRect = new Rect(lt.x, lt.y, rt.x - lt.x, rt.y - lb.y);

        return screenRect;
    }

    private float GetScreenRectX()
    {
        float zVal = 0f;
        if (MyObj != null)
            zVal = -MyObj.position.z;

        Vector3 lb = MyCamera.ScreenToWorldPoint(new Vector3(0, 0, zVal));

        return lb.x;
    }

    public float GetCamMidDummyPosX()
    {
        return BuildingMidPosX;
    }

    public Vector3 GetCameraPosition()
    {
        return MyObj.position;
    }

    private void OnDrawGizmos()
    {
        if( gameObject.activeSelf == false )
		{
            return;
		}

        if( MyCamera == null )
		{
            return;
		}

        float zVal = 0f;
        if (MyObj != null)
            zVal = -MyObj.position.z;

        float rect_x = SCREEN_WIDTH_LIMITPOS_L;
        float rect_y = SCREEN_HIGHT_LIMITPOS_U;
        float rect_wid = (SCREEN_WIDTH_LIMITPOS_R - SCREEN_WIDTH_LIMITPOS_L);
        float rect_hei = (SCREEN_HIGHT_LIMITPOS_U - SCREEN_HIGHT_LIMITPOS_D);

        Rect a = new Rect(rect_x, rect_y, rect_wid, rect_hei);

        Gizmos.color = Color.cyan;
        Vector3 leftLineFrom = new Vector3(a.x, a.y - a.height, 0);
        Vector3 leftLineTo = new Vector3(a.x, a.y, 0);
        Gizmos.DrawLine(leftLineFrom, leftLineTo);
        Vector3 upLineFrom = new Vector3(a.x, a.y, 0);
        Vector3 upLineTo = new Vector3(a.width + a.x, a.y, 0);
        Gizmos.DrawLine(upLineFrom, upLineTo);
        Vector3 rtLineFrom = new Vector3(a.width + a.x, a.y, 0);
        Vector3 rtLineTo = new Vector3(a.width + a.x, a.y - a.height, 0);
        Gizmos.DrawLine(rtLineFrom, rtLineTo);
        Vector3 dLineFrom = new Vector3(a.width + a.x, a.y - a.height, 0);
        Vector3 dLineTo = new Vector3(a.x, a.y - a.height, 0);
        Gizmos.DrawLine(dLineFrom, dLineTo);

        //Min Line
        Gizmos.color = Color.yellow;
        float gap = minGap;//(GAP_CAMLIMIT_BOTTOM - GAP_CAMLIMIT_MIN_BOTTOM)
        float yyPos = (a.y - heightForMin + gap);
        Vector3 dMLineFrom = new Vector3(a.width + a.x, yyPos, 0);
        Vector3 dMLineTo = new Vector3(a.x, yyPos, 0);
        Gizmos.DrawLine(dMLineFrom, dMLineTo);

        Rect b = GetScreenRect();

        Gizmos.color = Color.red;
        Vector3 lLineFrom = new Vector3(b.x, b.y - b.height, 0);
        Vector3 lLineTo = new Vector3(b.x, b.y, 0);
        Gizmos.DrawLine(lLineFrom, lLineTo);
        Vector3 uLineFrom = new Vector3(b.x, b.y, 0);
        Vector3 uLineTo = new Vector3(b.width + b.x, b.y, 0);
        Gizmos.DrawLine(uLineFrom, uLineTo);
        Vector3 rLineFrom = new Vector3(b.width + b.x, b.y, 0);
        Vector3 rLineTo = new Vector3(b.width + b.x, b.y - b.height, 0);
        Gizmos.DrawLine(rLineFrom, rLineTo);
        Vector3 dnLineFrom = new Vector3(b.width + b.x, b.y - b.height, 0);
        Vector3 dnLineTo = new Vector3(b.x, b.y - b.height, 0);
        Gizmos.DrawLine(dnLineFrom, dnLineTo);

    }

    public void Release()
    {
        if(worldMgr != null) worldMgr = null;
        if(MyCamera != null) MyCamera = null;
        if(MyObj != null) MyObj = null;
        if(eventTgt != null) eventTgt = null;
        if (DoubleTabStream != null) DoubleTabStream = null;
    }
}
