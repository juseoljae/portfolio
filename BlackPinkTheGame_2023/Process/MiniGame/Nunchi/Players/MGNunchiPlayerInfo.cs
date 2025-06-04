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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MGNunchiPlayerInfo : NetPlayerInfo
{
    public int PID;
    public int CoinScore;
    public MGNunchiMap Map_MyLocation;

    //       [0] up
    // [2] left   [3] right
    //       [1] down
    public bool[] IsMovable;
    public Vector3[] MovablePosition;
    public int[] MapIdxByDir;

    public int RotationValue;
    public BPWPacketDefine.NunchiGameItemType GainItem;


    public bool IsBot;
    public int SeatID;
    public StylingInfo StylingItemInfo;
}
