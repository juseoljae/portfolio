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


public class TournamentInfo
{
    public string IP;
    public uint PORT;
    public uint GAME_GROUP_ID;
    public uint ROOM_ID;
    public uint NODE_ID;

    public BPWPacketDefine.NunchiGameStageType StageType;
    public int MaxRound;

    public void SetDefaultInfo(string ip, uint port, uint grpID, uint roomID, uint nodeID)
    {
        IP = ip;
        PORT = port;
        GAME_GROUP_ID = grpID;
        ROOM_ID = roomID;
        NODE_ID = nodeID;

        CDebug.Log($"TournamentInfo {IP}:{PORT}, grpID:{GAME_GROUP_ID}, RoomId:{ROOM_ID}, NodeID:{NODE_ID}");
    }
}
