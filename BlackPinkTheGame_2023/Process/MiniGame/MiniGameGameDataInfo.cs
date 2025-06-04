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
using BPWPacketDefine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class RoundResultInfo
{
    public int PID;
    public long UID;
    public int PlayerSvrSeatID;
    public int TargetSvrMapID;
    public bool bGainSuccess;
    public int Quantity_GainCoin;
    public int Quantity_TotalCoin;
    public BPWPacketDefine.NunchiGameItemType GainItemType;
    //public BPWPacketDefine.NunchiGameItemType EffectItemType;
}

public class GameResultInfo
{
    public bool IsFinal;
    public bool IsPickingWinner; //is there draw player?
    public byte MyRank;
    public List<int> WinPlayerIDs = new List<int>();
    public List<StageRewardInfo> Rewards = new List<StageRewardInfo>();
}

public class RoundEffectiveItemInfo
{
    public int Round;
    public BPWPacketDefine.NunchiGameItemType EffectItemType;
}

public class StageRewardInfo
{
    public REWARD_CONSUME_TYPES RewardType;
    public long ItemID;
    public long Count;
    public long TotalOwnCount;
    public long TicketDailyCount;
}

public class FinalPlayerInfo
{
    public int Rank;
    public long UID;
    public int PID;
}