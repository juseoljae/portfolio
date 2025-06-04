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
using System;
using System.Collections.Generic;
using UnityEngine;
using UniRx;


public enum EMGNunchiEnterType
{
	NEED_REQ_LOGIN = 0,

	ROOM_ENTER = 1,

	DIRECT_NUNCHI = 2,
}

public class MGNunchiServerDataManager : Singleton<MGNunchiServerDataManager>
{
    private TournamentInfo GameRoomInfo;
    private Dictionary<int, MGNunchiPlayerInfo> GamePlayerInfoDic = new Dictionary<int, MGNunchiPlayerInfo>();//key : pid (but old uid)
    private MGNunchiCoinItemInfo[] GameItemInfo = new MGNunchiCoinItemInfo[5];//5 is place count
    private Dictionary<int, RoundResultInfo> RoundResultInfoDic = new Dictionary<int, RoundResultInfo>(); // key : pid (but old uid)
    private List<StageRewardInfo> SvrStageRewardInfos = new List<StageRewardInfo>();

    private Dictionary<int, RoundEffectiveItemInfo> RoundEffectiveItemInfoDic = new Dictionary<int, RoundEffectiveItemInfo>();// key : pid (but old uid)

    private int StageRank;
    private bool IsThereTiePlayers;
	private EMGNunchiEnterType enterType;
    public BPWPacketDefine.NunchiGameStageType StageType;

    private List<FinalPlayerInfo> FinallistInfos = new List<FinalPlayerInfo>();

	public EMGNunchiEnterType EnterType { get => enterType; set => enterType = value; }

    public bool IsSinglePlay = false;
    public bool IsCompleteTutorial = false;

    private int MYPlayerPID;

    private float StateLatencyTime;
    private SingleAssignmentDisposable LaytencyDisposer;

    public void SetTournamentGameRoomInfo(string ip, uint port, uint grpID, uint roomID, uint nodeID, bool isSinglePlay)
    {
        CDebug.Log($"SetTournamentGameRoomInfo() ip - {ip}, port - {port}, grpID - {grpID}, roomID - {roomID}, isSinglePlay - {isSinglePlay}");

        if(GameRoomInfo == null)
        {
            GameRoomInfo = new TournamentInfo();
        }

        IsSinglePlay = isSinglePlay;

        if(IsSinglePlay)
            IsCompleteTutorial = IsSinglePlay;

        StateLatencyTime = 0;

        GameRoomInfo.SetDefaultInfo(ip, port, grpID, roomID, nodeID);
    }

    public void SetTournamentGameRoomInfo(BPWPacketDefine.NunchiGameStageType type, int maxRound)
    {
        CDebug.Log($"SetTournamentGameRoomInfo() type - {type}, maxRound - {maxRound}", CDebugTag.NUNCH_SERVER_DATA_MANAGER);

        if (GameRoomInfo == null)
        {
            CDebug.LogError("SetTournamentGameInfo, GameRoomInfo(TournamentInfo) is null", CDebugTag.NUNCH_SERVER_DATA_MANAGER);
            return;
        }

        GameRoomInfo.StageType = type;
        GameRoomInfo.MaxRound = maxRound;
    }

    public TournamentInfo GetTournamentGameRoomInfo()
    {
        if(GameRoomInfo != null)
        {
            return GameRoomInfo;
        }

        return null;
    }

    public void SetMyPlayerPID(int PID)
    {
        MYPlayerPID= PID;
    }

    public int GetMYPlayerPID()
    {
        return MYPlayerPID;
    }

    public void SetGamePlayerInfoDic(int PID, MGNunchiPlayerInfo info)
    {
        CDebug.Log($"SetGamePlayerInfoDic() Add GamePlayerInfoDic uid:{PID}, name:{info.NickName}", CDebugTag.NUNCH_SERVER_DATA_MANAGER);
        if(GamePlayerInfoDic.ContainsKey(PID) == false)
        {
            GamePlayerInfoDic.Add(PID, info);
        }
        else
        {
            GamePlayerInfoDic[PID] = info;
        }
    }

    public MGNunchiPlayerInfo GetGamePlayerInfo(int PID)
    {
        if (GamePlayerInfoDic.ContainsKey(PID))
        {
            return GamePlayerInfoDic[PID];
        }

        return null;
    }

    public Dictionary<int, MGNunchiPlayerInfo> GetGamePlayerInfoDic()
    {
        return GamePlayerInfoDic;
    }

    public void SetCoinItemInfo(BPWPacket.NunchiGameRoundInfoBc pktInfo)
    {
        for(int i=0; i<pktInfo.ItemSeatInfosLength; ++i)
        {
            BPWPacketDefine.NunchiGameItemSeatInfo pktItemInfo = pktInfo.ItemSeatInfos(i).Value;
            MGNunchiCoinItemInfo _info = new MGNunchiCoinItemInfo();
            _info.ItemType = pktItemInfo.Type;
            _info.Quantity = pktItemInfo.Quantity;
            _info.SeatID = pktItemInfo.SeatId;
            GameItemInfo[i] = _info;

            CDebug.Log($"SetCoinItemInfo() type:{_info.ItemType}, quantity:{_info.Quantity}, seatID:{_info.SeatID}", CDebugTag.NUNCH_SERVER_DATA_MANAGER);
        }
    }

    public void SetCoinItemInfo()
    {
        int count = 5;

        System.Random random = new System.Random();

        for (int i = 0; i < count; ++i)
        {
            MGNunchiCoinItemInfo _info = new MGNunchiCoinItemInfo();
            _info.ItemType = BPWPacketDefine.NunchiGameItemType.COIN;
            _info.Quantity = random.Next(1, 7); 
            _info.SeatID = i;
            GameItemInfo[i] = _info;
            CDebug.Log($"SetCoinItemInfo() type:{_info.ItemType}, quantity:{_info.Quantity}, seatID:{_info.SeatID}", CDebugTag.NUNCH_SERVER_DATA_MANAGER);
        }


        //for (int i = 0; i < pktInfo.ItemSeatInfosLength; ++i)
        //{
        //    BPWPacketDefine.NunchiGameItemSeatInfo pktItemInfo = pktInfo.ItemSeatInfos(i).Value;
        //    MGNunchiCoinItemInfo _info = new MGNunchiCoinItemInfo();
        //    _info.ItemType = pktItemInfo.Type;
        //    _info.Quantity = pktItemInfo.Quantity;
        //    _info.SeatID = pktItemInfo.SeatId;
        //    GameItemInfo[i] = _info;

        //    CDebug.Log($"SetCoinItemInfo() type:{_info.ItemType}, quantity:{_info.Quantity}, seatID:{_info.SeatID}", CDebugTag.NUNCH_SERVER_DATA_MANAGER);
        //}
    }




    public MGNunchiCoinItemInfo[] GetCoinItemInfos()
    {
        return GameItemInfo;
    }

    public void SetRoundResultInfoDic(int PID, RoundResultInfo info)
    {
        if (RoundResultInfoDic.ContainsKey(PID) == false)
        {
            RoundResultInfoDic.Add(PID, info);
            SetRoundResultInfoDeeply(PID, info);
        }
        else
        {
            SetRoundResultInfoDeeply(PID, info);
        }
    }

    private void SetRoundResultInfoDeeply(int PID, RoundResultInfo info)
    {
        RoundResultInfoDic[PID].UID = info.UID;
        RoundResultInfoDic[PID].PlayerSvrSeatID = info.PlayerSvrSeatID;
        RoundResultInfoDic[PID].TargetSvrMapID = info.TargetSvrMapID;
        RoundResultInfoDic[PID].bGainSuccess = info.bGainSuccess;
        RoundResultInfoDic[PID].Quantity_GainCoin = info.Quantity_GainCoin;
        RoundResultInfoDic[PID].Quantity_TotalCoin = info.Quantity_TotalCoin;
        //RoundResultInfoDic[PID].EffectItemType = info.EffectItemType;
        //CDebug.Log($"     !!!!!!!!  SetRoundResultInfoDeeply. {MGNunchiManager.Instance.WorldManager.GetRoundCount()} ROUND / PID:{PID}");

        //CDebug.Log($"     !!!!!!!!  SetRoundResultInfoDeeply uid:{PID}, target map = {RoundResultInfoDic[PID].TargetSvrMapID}");
        RoundResultInfoDic[PID].GainItemType = info.GainItemType;
    }

    public RoundResultInfo GetRoundResultInfoDic(int PID)
    {
        if (RoundResultInfoDic.ContainsKey(PID))
        {
            return RoundResultInfoDic[PID];
        }

        return null;
    }

    public void InitRoundResultInfo_GainItem(int PID)
    {
        if (RoundResultInfoDic.ContainsKey(PID))
        {
            RoundResultInfoDic[PID].GainItemType = BPWPacketDefine.NunchiGameItemType.NONE;
        }
    }


    public void SetRoundEffectiveItemInfoDic(int PID, int round, BPWPacketDefine.NunchiGameItemType effItem)
    {
        if(RoundEffectiveItemInfoDic.ContainsKey(PID) == false)
        {
            RoundEffectiveItemInfoDic.Add(PID, new RoundEffectiveItemInfo());
            RoundEffectiveItemInfoDic[PID].Round = round;
            RoundEffectiveItemInfoDic[PID].EffectItemType = effItem;
        }
        else
        {
            RoundEffectiveItemInfoDic[PID].Round = round;
            RoundEffectiveItemInfoDic[PID].EffectItemType = effItem;
        }
        CDebug.Log($"SetRoundEffectiveItemInfoDic // uid = {PID}, ROUND = {round}, effective Item = {effItem}", CDebugTag.NUNCH_SERVER_DATA_MANAGER);
    }

    public RoundEffectiveItemInfo GetRoundEffectiveItemInfo(int PID)
    {
        if (RoundEffectiveItemInfoDic.ContainsKey(PID))
        {
            return RoundEffectiveItemInfoDic[PID];
        }

        return null;
    }


    #region - 블핑월드 스탠드얼론 처리.. 
    public void SetStageRewardInfosFromRecvPacket(int changeCnt, int metaverse_minigame_reward)
    {
        StageRewardInfo _info = new StageRewardInfo();
        _info.RewardType = REWARD_CONSUME_TYPES.ITEM;
        _info.ItemID = Configure.GetConfigureData_Item(GOODS_TYPE_ITEM.WORLD_TICKET);
        _info.Count = changeCnt;
        _info.TotalOwnCount = 0;
        _info.TicketDailyCount = metaverse_minigame_reward;   // 누적 카운트만. .보여주기 때문에..

        CDebug.Log($"{_info.RewardType}, {_info.ItemID}, {_info.TicketDailyCount}", CDebugTag.NUNCH_SERVER_DATA_MANAGER);
        CDebug.Log("------------------", CDebugTag.NUNCH_SERVER_DATA_MANAGER);

        SetStageRewardInfo(_info);
    }
    #endregion


    public void SetStageRewardInfosFromRecvPacket(BPWPacketDefine.NunchiGameRewardInfo _pktInfo)
    {
        StageRewardInfo _info = new StageRewardInfo();
        _info.RewardType = (REWARD_CONSUME_TYPES)_pktInfo.Type;
        _info.ItemID = _pktInfo.ItemId;
        _info.Count = _pktInfo.Quantity;
        _info.TotalOwnCount = (long)_pktInfo.TotalQuantity;
        _info.TicketDailyCount = _pktInfo.DailyCount;

        switch (_info.RewardType)
        {
            case REWARD_CONSUME_TYPES.ITEM:
                ItemInvenInfo _rewardInfo = new ItemInvenInfo();
                _rewardInfo.gdid = _info.ItemID;
                _rewardInfo.cnt = (int)_info.TotalOwnCount;
                _rewardInfo.max_cnt = 0;// (int)_info.TotalOwnCount;
                CItemInvenManager.Instance.UpdateItemInven(_rewardInfo);
                break;
        }

        CDebug.Log($"{_info.RewardType}, {_info.ItemID}, {_info.Count}", CDebugTag.NUNCH_SERVER_DATA_MANAGER);
        CDebug.Log("------------------", CDebugTag.NUNCH_SERVER_DATA_MANAGER);

        SetStageRewardInfo(_info);
    }





    public void SetStageRewardInfo(StageRewardInfo _info)
    {
        SvrStageRewardInfos.Add(_info);
    }

    public List<StageRewardInfo> GetSvrStageRewardInfoList()
    {
        return SvrStageRewardInfos;
    }

    public int GetSvrStageRewardInfoListCountByType(REWARD_CONSUME_TYPES type)
    {
        int itemRewardCount = 0;
        for(int i=0; i< SvrStageRewardInfos.Count; ++i)
        {
            if(SvrStageRewardInfos[i].RewardType == type)
            {
                itemRewardCount++;
            }
        }
        return itemRewardCount;
    }

    public List<StageRewardInfo> GetStageRewardInfoListCountByType(REWARD_CONSUME_TYPES type)
    {
        List<StageRewardInfo> _list = new List<StageRewardInfo>();

        for (int i = 0; i < SvrStageRewardInfos.Count; ++i)
        {
            if (SvrStageRewardInfos[i].RewardType == type)
            {
                _list.Add(SvrStageRewardInfos[i]);  
            }
        }

        return _list;
    }

    public List<StageRewardInfo> GetStageRewardInfoByType(REWARD_CONSUME_TYPES type)
    {
        List<StageRewardInfo> _list = new List<StageRewardInfo>();
        for (int i = 0; i < SvrStageRewardInfos.Count; ++i)
        {
            if (SvrStageRewardInfos[i].RewardType == type)
            {
                _list.Add(SvrStageRewardInfos[i]);
            }
        }

        return _list;
    }

    

    public void SetMyStageRank(int rank)
    {
        CDebug.Log($"SetMyStageRank(int rank) - {rank}", CDebugTag.NUNCH_SERVER_DATA_MANAGER);

        StageRank = rank;
    }

    public int GetMyStageRank()
    {
        return StageRank;
    }


    public void SetIsThereTiePlayers(bool isTie)
    {
        IsThereTiePlayers = isTie;
    }

    public bool GetIsThereTiePlayers()
    {
        return IsThereTiePlayers;
    }

    public void SetFinallistInfo(FinalPlayerInfo info)
    {
        CDebug.Log($"NunchiServerDataManager uid = {info.UID}, Rank = {info.Rank} // FinallistInfos count = {FinallistInfos.Count}", CDebugTag.NUNCH_SERVER_DATA_MANAGER);
        FinallistInfos.Add(info);
    }

    public List<FinalPlayerInfo> GetFinallistInfoList()
    {
        return FinallistInfos;
    }

    //public void SetStateLatencyTime(float time, bool notCheck = false)
    //{
    //    if (IsSinglePlay) return;
    //    if (notCheck)
    //    {
    //        StateLatencyTime = 0;
    //        return;
    //    }

    //    if (StateLatencyTime != 0)
    //    {
    //        float latency = time - StateLatencyTime;
    //        if (latency > 20)
    //        {
    //            BPWorldExitEventProcessor.Instance.Regist( EBPWorldExitEventType.LATENCY_LOW_PIHAGI_GAME );
    //            return;
    //        }
    //    }

    //    StateLatencyTime = time;
    //}
    public void SetStateLatencyTime(float time, bool notCheck = false, bool endbc = false)
    {
        if (IsSinglePlay) return;

        CDebug.Log( $" ______  Called SetStateLatencyTime notCheck:{notCheck},  time = {time}, StateLatencyTime = {StateLatencyTime} " , CDebugTag.NUNCH_SERVER_DATA_MANAGER);
        if (notCheck)
        {
            if (LaytencyDisposer != null)
            {
                LaytencyDisposer.Dispose();
            }
            StateLatencyTime = 0;
            return;
        }
        else
        {
            StateLatencyTime = time;
            if (LaytencyDisposer != null)
            {
                LaytencyDisposer.Dispose();
                LaytencyDisposer = null;
            }
            LaytencyDisposer = new SingleAssignmentDisposable();

            CDebug.Log( $" ______  SetStateLatencyTime LaytencyDisposer.Disposable"   ,  CDebugTag.NUNCH_SERVER_DATA_MANAGER);
            LaytencyDisposer.Disposable = Observable.Interval( TimeSpan.FromSeconds( 1.0f ) ).Subscribe( x =>
            {
                if (notCheck == false)
                {
                    long strID = 927012;
                    if (endbc)
                    {
                        strID = 927013;
                    }
                    float latency = Time.time - StateLatencyTime;
                    CDebug.Log( $" ______  SetStateLatencyTime latency = {latency}" ,  CDebugTag.NUNCH_SERVER_DATA_MANAGER);
                    if (latency >= 10 && latency < 11)
                    {
                        string strToastMessage = CResourceManager.Instance.GetString( strID ); //유저를 기다리는 중입니다.
                        Toast.MakeTextForContent( strToastMessage, Msg.MSG_TYPE.Notice );
                        LaytencyDisposer.Dispose();
                    }
                    //else if (latency >= 18)
                    //{
                    //    BPWorldExitEventProcessor.Instance.Regist( EBPWorldExitEventType.LATENCY_LOW_PIHAGI_GAME );
                    //    LaytencyDisposer.Dispose();
                    //    return;
                    //}
                }
            } );

        }

    }

    public void CleanUpServerData()
    {
        //if (GameRoomInfo != null) GameRoomInfo = null;
        if(GamePlayerInfoDic != null) GamePlayerInfoDic.Clear();

        for (int i = 0; i < GameItemInfo.Length; ++i)
        {
            if (GameItemInfo[i] != null)
            {
                GameItemInfo[i] = null;
            }
        }

        if(RoundResultInfoDic != null) RoundResultInfoDic.Clear();

        if (SvrStageRewardInfos != null) SvrStageRewardInfos.Clear();

        StageRank = 0;
        IsThereTiePlayers = false;

        if (FinallistInfos != null)
        {
            FinallistInfos.Clear();
        }
        StateLatencyTime = 0;
    }


    public void Clear()
    {
        if (GameRoomInfo != null) GameRoomInfo = null;
        if(GamePlayerInfoDic != null)
        {
            GamePlayerInfoDic.Clear();
        }
        if(RoundResultInfoDic != null)
        {
            RoundResultInfoDic.Clear();
        }
        if(RoundResultInfoDic != null)
        {
            RoundResultInfoDic.Clear();
        }
        if(SvrStageRewardInfos != null)
        {
            SvrStageRewardInfos.Clear();
        }
        if(RoundEffectiveItemInfoDic != null)
        {
            RoundEffectiveItemInfoDic.Clear();
        }
        IsThereTiePlayers = false;
        if(FinallistInfos != null)
        {
            FinallistInfos.Clear();
        }
        if (LaytencyDisposer != null)
        {
            LaytencyDisposer.Dispose();
            LaytencyDisposer = null;
        }
        StateLatencyTime = 0;
    }
}
    
