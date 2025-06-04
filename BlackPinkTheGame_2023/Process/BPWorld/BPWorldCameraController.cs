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

using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using BPWPacketDefine;
using UnityEngine.EventSystems;

public class BPWorldCameraController : MonoBehaviour
{
    private CinemachineFreeLook CineFreeLook;
    private CCharacterController_BPWorld CharController;

    private const string CineFL_XAxisName = "Mouse X";
    private const string CineFL_YAxisName = "Mouse Y";
    private const float DEFAULT_YAXIS_VALUE = 0.3f;
    private const float DEFAULT_ACCEL_TIME = 0.3f;
    private const int YAXIS_SPEED = 5;
    private const int XAXIS_SPEED = 180;
    private float[] defaultHeight;
    private float[] defaultRadius;

    private CinemachineFreeLook CineFL = null;
    private CinemachineFreeLook.Orbit[] originalOrbits = null;

    public enum LAYERTYPE
    {
        NONE = 0,
        UI,
        SCREEN
    }
    private List<LAYERTYPE> TouchLayerType = new List<LAYERTYPE>();

    private float[] HEIGHT_MIN;
    private float[] HEIGHT_MAX;
    private float[] RADIUS_MIN;
    private float[] RADIUS_MAX;

#if UNITY_EDITOR
    private float minScale = 0.6f;
    private float maxScale = 2;
    [Tooltip( "The zoom axis.  Value is 0..1.  How much to scale the orbits" )]
    [AxisStateProperty]
    public AxisState zAxis = new AxisState( 0, 2, false, true, 50f, 0.1f, 0.1f, "Mouse ScrollWheel", false );
#endif

    public void InitFreeLookCamera(CinemachineFreeLook camFreeLook)
    {
        CineFreeLook = camFreeLook;
        defaultHeight = new float[] { 8.8f, 5.3f, 0.8f };
        defaultRadius = new float[] { 5.3f, 8.3f, 8.8f };
        HEIGHT_MIN = new float[] { 4, 1.3f, -1.6f };
        RADIUS_MIN = new float[] { 1.7f, 3.3f, 1.7f };
        HEIGHT_MAX = new float[] { 12, 7.7f, 0.5f };
        RADIUS_MAX = new float[] { 4.3f, 9.9f, 12 };

        this.CineFL = camFreeLook;
        if (CineFL != null)
        {
            this.originalOrbits = new CinemachineFreeLook.Orbit[CineFL.m_Orbits.Length];

            for (int i = 0; i < CineFL.m_Orbits.Length; i++)
            {
                originalOrbits[i].m_Height = CineFL.m_Orbits[i].m_Height = defaultHeight[i];
                originalOrbits[i].m_Radius = CineFL.m_Orbits[i].m_Radius = defaultRadius[i];
            }

            CineFL.m_YAxis.Value = DEFAULT_YAXIS_VALUE;
            CineFL.m_YAxis.m_AccelTime = DEFAULT_ACCEL_TIME;
            SetFreeLookCamAxis( string.Empty, string.Empty, 0, 0 );

            SetFreeLookAngle();
        }
    }

    public void SetPlayerController(CCharacterController_BPWorld charCntlr)
    {
        CharController = charCntlr;
    }

    // Update is called once per frame
    void Update()
    {
        SetCMFLAxisName();

#if UNITY_EDITOR
        if (Input.GetAxis( "Mouse ScrollWheel" ) != 0)
        {
            MouseWheelZoomCamera();
        }
#endif

    }


    private void SetCMFLAxisName()
    {
        string layerName = string.Empty;
        int touchCnt = 0;

#if UNITY_EDITOR
        touchCnt = 1;
#else
        touchCnt = Input.touchCount;
#endif

        if (CineFL != null)
        {
            if (touchCnt > 0 && touchCnt < 3)
            {
                for (int i = 0; i < touchCnt; ++i)
                {
                    if (Input.GetMouseButtonDown( i ))
                    {
                        if (i == 0)
                        {
                            if (TouchLayerType.Count > 0)
                            {
                                TouchLayerType.Clear();
                            }
                        }

                        Vector2 touchPos = GetTouchPoint( i );

                        layerName = GetLayerName( touchPos );
                        if (layerName.Equals( "UI" ))
                        {
                            //CDebug.Log( $"      0000&&&       [touch ButtonDown] {i}th LAYERTYPE.UI add to TouchLayerType" );
                            if (TouchLayerType.Count < 2)
                            {
                                TouchLayerType.Add( LAYERTYPE.UI );
                            }
                        }
                        else
                        {
                            //CDebug.Log( $"      0000&&&       [touch ButtonDown] {i}th LAYERTYPE.SCREEN add to TouchLayerType" );
                            if (TouchLayerType.Count < 2)
                            {
                                TouchLayerType.Add( LAYERTYPE.SCREEN );
                            }
                        }
                    }
                }

                bool isPossibleZoom = CheckPinchZoomPossible( touchCnt );

                if (isPossibleZoom)
                {
                    PinchZoomCamera();

                    for (int i = 0; i < touchCnt; ++i)
                    {
                        if (Input.GetMouseButtonUp( i ))
                        {
                            Vector2 touchPos = GetTouchPoint( i );
                            layerName = GetLayerName( touchPos );
                            RemoveTouchLayer( layerName );
                            isPossibleZoom = false;
                            SetFreeLookCamAxis( string.Empty, string.Empty, 0, 0 );
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < touchCnt; ++i)
                    {
                        if (Input.GetMouseButtonDown( i ))
                        {
                            Vector2 touchPos = GetTouchPoint( i );
                            layerName = GetLayerName( touchPos );
                            if (layerName.Equals( "UI" ) == false)
                            {
                                SetFreeLookCamAxis( CineFL_XAxisName, CineFL_YAxisName, XAXIS_SPEED, YAXIS_SPEED );
                            }
                        }
                        else if (Input.GetMouseButtonUp( i ))
                        {
                            Vector2 touchPos = GetTouchPoint( i );
                            layerName = GetLayerName( touchPos );
                            RemoveTouchLayer( layerName );
                            SetFreeLookCamAxis( string.Empty, string.Empty, 0, 0 );
                        }
                    }
                }
            }
        }
    }

    private void SetFreeLookCamAxis(string xAxisName, string yAxisName, int xAxisSpeed, int yAxisSpeed)
    {
        CineFL.m_XAxis.m_InputAxisName = xAxisName;
        CineFL.m_YAxis.m_InputAxisName = yAxisName;
        CineFL.m_XAxis.m_MaxSpeed = xAxisSpeed;
        CineFL.m_YAxis.m_MaxSpeed = yAxisSpeed;
    }
    private void SetFreeLookAngle()
    {
        CineFL.m_XAxis.Value = 0;
        CineFL.m_YAxis.Value = DEFAULT_YAXIS_VALUE;

        EnterType enterType = BPWorldConnectionManager.Instance.EnterLeaveTypeGetter.GetEnterType();

        switch (enterType)
        {
            case EnterType.WORLD_FROM_NUNCH_GAME:
            case EnterType.WORLD_FROM_PIHAGI_GAME:
                CineFL.m_XAxis.Value = CharController.transform.eulerAngles.y;
                break;
        }
    }

    private bool CheckPinchZoomPossible(int touchCnt)
    {
        if (touchCnt < 2) return false;

        if (TouchLayerType.Count != 2 )
        {
            //CDebug.LogError( $"CheckPinchZoomPossible(). TouchLayerType count : {TouchLayerType.Count} is not match. return false" );
            return false;
        }

        //CDebug.Log($"   [CheckPinchZoomPossible()] TouchLayerType.Count = {TouchLayerType.Count}" );
        for (int i = 0; i < TouchLayerType.Count; ++i)
        {
            if (TouchLayerType[i] == LAYERTYPE.UI)
            {
                //CDebug.Log( $"   [CheckPinchZoomPossible()] {i}th TouchLayerType is LAYERTYPE.UI" );
                return false;
            }
        }

        //CDebug.Log( "   [CheckPinchZoomPossible()] ZOOM Possible" );
        return true;
    }

    public void ClearTouchLayerType()
    {
        TouchLayerType.Clear();
    }

    public void PinchZoomCamera()
    {
        if (Input.touchCount != 2)
        {
            //CDebug.LogError( $"PinchZoomCamera(). touch count :{Input.touchCount} is not avalible" );
            return;
        }

        SetFreeLookCamAxis( string.Empty, string.Empty, 0, 0 );

        Touch touch_1st = Input.GetTouch( 0 );
        Touch touch_2nd = Input.GetTouch( 1 );

        Vector2 preTouch_1stPos = touch_1st.position - touch_1st.deltaPosition;
        Vector2 preTouch_2ndPos = touch_2nd.position - touch_2nd.deltaPosition;

        float preMagnitude = (preTouch_1stPos - preTouch_2ndPos).magnitude;
        float curMagnitude = (touch_1st.position - touch_2nd.position).magnitude;

        float gapMagnitude = curMagnitude - preMagnitude;

        float scale = gapMagnitude * 0.06f;

        for (int i = 0; i < originalOrbits.Length; i++)
        {
            float height = Mathf.Clamp( CineFL.m_Orbits[i].m_Height - scale, HEIGHT_MIN[i], HEIGHT_MAX[i] );
            float radius = Mathf.Clamp( CineFL.m_Orbits[i].m_Radius - scale, RADIUS_MIN[i], RADIUS_MAX[i] );

            if (height > 0) CineFL.m_Orbits[i].m_Height = height;
            if (radius > 0) CineFL.m_Orbits[i].m_Radius = radius;
            //CDebug.Log( $"               [******   CineFL.m_Orbits] height = {height}, radius = {radius}" );
        }
    }

    private Vector2 GetTouchPoint(int idx)
    {
        Vector2 touchPos = Vector2.zero;
#if UNITY_EDITOR
        touchPos = new Vector2( Input.mousePosition.x, Input.mousePosition.y );
#else
        Touch touch = Input.GetTouch( idx );
        touchPos = touch.position;
#endif

        return touchPos;
    }

    private string GetLayerName(Vector2 touchpos)
    {
        PointerEventData pointer = new PointerEventData( EventSystem.current );
        pointer.position = new Vector2( touchpos.x, touchpos.y );//new Vector2( Input.mousePosition.x, Input.mousePosition.y );

        List<RaycastResult> raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll( pointer, raycastResults );

        Debug.Log( "raycastResults.Count = " + raycastResults.Count + " / " + pointer.position );
        if (raycastResults.Count > 0)
        {
            foreach (var go in raycastResults)
            {
                return LayerMask.LayerToName( go.gameObject.layer );
            }
        }

        return string.Empty;
    }

    private void RestoreOriginalOrbits()
    {
        if (originalOrbits != null)
        {
            for (int i = 0; i < originalOrbits.Length; i++)
            {
                CineFL.m_Orbits[i].m_Height = originalOrbits[i].m_Height;
                CineFL.m_Orbits[i].m_Radius = originalOrbits[i].m_Radius;
            }
        }
    }


    private void RemoveTouchLayer(string layer)
    {
        if(layer.Equals("UI"))
        {
            TouchLayerType.Remove( LAYERTYPE.UI );
        }
        else
        {
            TouchLayerType.Remove( LAYERTYPE.SCREEN );
        }
    }


#if UNITY_EDITOR
    public void MouseWheelZoomCamera()
    {
        if (originalOrbits != null && CineFL != null)
        {
            zAxis.Update( Time.deltaTime );
            float scale = Mathf.Lerp( minScale, maxScale, zAxis.Value );

            for (int i = 0; i < originalOrbits.Length; i++)
            {
                CineFL.m_Orbits[i].m_Height = originalOrbits[i].m_Height * scale;
                CineFL.m_Orbits[i].m_Radius = originalOrbits[i].m_Radius * scale;
            }
        }
    }


#endif

    public void Release()
    {
        if (CineFL != null)
        {
            CineFL = null;
        }
        if (originalOrbits != null)
        {
            originalOrbits = null;
        }
        if (defaultRadius != null)
        {
            defaultRadius = null;
        }
        if (HEIGHT_MIN != null)
        {
            HEIGHT_MIN = null;
        }
        if (HEIGHT_MAX != null)
        {
            HEIGHT_MAX = null;
        }
        if (RADIUS_MIN != null)
        {
            RADIUS_MIN = null;
        }
        if (RADIUS_MAX != null)
        {
            RADIUS_MAX = null;
        }
        if (TouchLayerType != null)
        {
            TouchLayerType.Clear();
            TouchLayerType = null;
        }
    }
}
