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

public class CAIWayPoint
{
    private List<WayPointInfo> WayPoints;
    //private int CurWayPointIdx;
    private bool bPauseWayPointCount;
    private int StartFloor;
    private int TargetFloor;
    private bool IsStart;

    public CAIWayPoint()
    {
        WayPoints = new List<WayPointInfo>();
    }

    public void Initialize()
    {
        //CurWayPointIdx = 0;

        for(int i=0; i<WayPoints.Count; ++i)
        {
            WayPoints[i] = null;
        }
        WayPoints.Clear();

        SetFloors(-1, -1);
    }

    public void SetFloors(int startFloor, int targetFloor)
    {
        StartFloor = startFloor;
        TargetFloor = targetFloor;
        //SetIsStart(false);

        //CDebug.Log("  ** SetFloor() cur Floor = " + StartFloor +" / target Floor = " + TargetFloor);
    }

    //public void SetIsStart(bool start)
    //{
    //    IsStart = start;
    //}

    //public bool GetIsStart()
    //{
    //    return IsStart;
    //}

    public int GetStartFloor()
    {
        return StartFloor;
    }

    public int GetTargetFloor()
    {
        return TargetFloor;
    }

    public void SetAddWayPoint(WAYPOINT_WAIT_STATE state, Vector3 point)
    {
        WayPointInfo _info = new WayPointInfo();

        _info.WaitState = state;
        _info.Point = point;
        _info.IsDone = false;

        WayPoints.Add(_info);
    }

    public WAYPOINT_WAIT_STATE GetWayPointWaitState(int idx)
    {
        if(WayPoints.Count == 0)
        {
            return WAYPOINT_WAIT_STATE.NONE;
        }
        return WayPoints[idx].WaitState;
    }

    public Vector3 GetWayPoint(int idx)
    {
        return WayPoints[idx].Point;
    }

    public Vector3 GetPositionByWayPointState(WAYPOINT_WAIT_STATE state)
    {
        for (int i = 0; i < WayPoints.Count; ++i)
        {
            if (WayPoints[i].WaitState == state)
            {
               return WayPoints[i].Point;
            }

        }

        return Vector3.zero;
    }

    public void SetWayPointWaitStateDone(int idx, bool isDone)
    {
        WayPoints[idx].IsDone = isDone;
    }

    public bool GetWayPointWaitStateDone(int idx)
    {
        return WayPoints[idx].IsDone;
    }

    public void PauseWayPoint()
    {
        bPauseWayPointCount = true;
    }

    public void ResumeWayPoint()
    {
        bPauseWayPointCount = false;
    }

    public bool bPauseWayPoint()
    {
        return bPauseWayPointCount;
    }
}


public enum WAYPOINT_WAIT_STATE
{
    NONE = 0,
    //Start Floor
    SF_WAIT,
    SF_INNER,
    //Target Floor
    TF_INNER,
    TF_WAIT,
    ARRIVED
}

public class WayPointInfo
{
    public WAYPOINT_WAIT_STATE WaitState;
    public Vector3 Point;
    public bool IsDone;
}

public class AITime
{
    public float TargetTime;
    public float CheckTime;
    public float PassTimeBfPause;
}
