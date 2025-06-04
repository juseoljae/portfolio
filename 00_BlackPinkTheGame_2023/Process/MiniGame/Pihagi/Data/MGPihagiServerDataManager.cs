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
using System.Linq;
using UniRx;
using System.Security.Permissions;

public enum EMGPihagiEnterType
{
    NEED_REQ_LOGIN = 0,

    ROOM_ENTER = 1,

    DIRECT_PIHAGI = 2,
}


public class MGPihagiServerDataManager : Singleton<MGPihagiServerDataManager>
{
    private EMGPihagiEnterType enterType;
    private BPWPacketDefine.PihagiGameStageType stageType;

    private TournamentInfo GameRoomInfo;

    private Dictionary<int, MGPihagiPlayerInfo> GamePlayerInfoDic = new Dictionary<int, MGPihagiPlayerInfo>(); //Key:PlayerID

    //SnapShot
    private Dictionary<int, MGPihagiSnapShotPlayer> PlayerSnapShotInfoDic = new Dictionary<int, MGPihagiSnapShotPlayer>();
    private Dictionary<int, MGPihagiSnapShotEnemy> EnemySnapShotInfoDic = new Dictionary<int, MGPihagiSnapShotEnemy>();

    private GameResultInfo GameEnd_ResultInfo;

    private ulong serverTick;

    public bool IsSinglePlay = false;

    private int MYPlayerPID;

    private SingleAssignmentDisposable LaytencyDisposer;
    private float StateLatencyTime;
    public float SnapshotRecvTime;

    public EMGPihagiEnterType EnterType 
    { 
        set { enterType = value; }
        get { return enterType; } 
    }

    public BPWPacketDefine.PihagiGameStageType StageType
    {
        set { stageType = value; }
        get { return stageType; }
    }

    public ulong ServerTick
    {
        set { serverTick = value; }
        get { return serverTick; }
    }

    public void SetTournamentGameRoomInfo(string ip, uint port, uint grpID, uint roomID, uint nodeID, bool isSinglePlay)
    {
        if (GameRoomInfo == null)
        {
            GameRoomInfo = new TournamentInfo();
        }


        SnapshotRecvTime = 0;
        StateLatencyTime = 0;
        IsSinglePlay = isSinglePlay;

        GameRoomInfo.SetDefaultInfo(ip, port, grpID, roomID, nodeID);
    }




    public TournamentInfo GetTournamentGameRoomInfo()
    {
        if (GameRoomInfo != null)
        {
            return GameRoomInfo;
        }

        return null;
    }


    public void SetMyPlayerPID(int PID)
    {
        MYPlayerPID = PID;
    }

    public int GetMyPlayerPID()
    {
        return MYPlayerPID;
    }

    public void SetGamePlayerInfo(int PID, MGPihagiPlayerInfo info)
    {
        CDebug.Log($"SetGamePlayerInfo info:{info.PlayerID}, {info.NickName}" );
        if(GamePlayerInfoDic.ContainsKey(PID) == false)
        {
            GamePlayerInfoDic.Add(PID, info);
        }
        else
        {
            GamePlayerInfoDic[PID] = info;
        }
    }

    public MGPihagiPlayerInfo GetGamePlayerInfo(int PID)
    {
        if(GamePlayerInfoDic.ContainsKey(PID))
        {
            return GamePlayerInfoDic[PID];
        }

        return null;
    }

    public Dictionary<int, MGPihagiPlayerInfo> GetAllGamePlayerInfo()
    {
        return GamePlayerInfoDic;
    }

    public List<MGPihagiPlayerInfo> GetAllGamePlayerInfoList()
    {
        return GamePlayerInfoDic.Values.ToList();
    }

    public void SetGameEnd_ResultInfo(BPWPacket.PihagiGameStateEndBc pktInfo)
    {
        if(GameEnd_ResultInfo == null)
        {
            GameEnd_ResultInfo = new GameResultInfo();
        }

        GameEnd_ResultInfo.IsFinal = pktInfo.IsFinal;
        GameEnd_ResultInfo.IsPickingWinner = pktInfo.IsPickWinner;
        GameEnd_ResultInfo.MyRank = pktInfo.Ranking;


        for (int i=0; i<pktInfo.WinnerPlayerIdsLength; ++i)
        {
            GameEnd_ResultInfo.WinPlayerIDs.Add(pktInfo.WinnerPlayerIds(i));
        }

        for(int i=0; i<pktInfo.RewardsLength; ++i)
        {
            BPWPacketDefine.PihagiGameRewardInfo rwInfo = pktInfo.Rewards(i).Value;

            StageRewardInfo rInfo = new StageRewardInfo();
            rInfo.RewardType = (REWARD_CONSUME_TYPES)rwInfo.Type;
            rInfo.ItemID = rwInfo.ItemId;
            rInfo.Count = rwInfo.Count;
            rInfo.TotalOwnCount = (long)rwInfo.TotalCount;
            rInfo.TicketDailyCount = rwInfo.DailyCount;

            if(rInfo.RewardType == REWARD_CONSUME_TYPES.ITEM)
            {
                ItemInvenInfo _rewardInfo = new ItemInvenInfo();
                _rewardInfo.gdid = rInfo.ItemID;
                _rewardInfo.cnt = (int)rInfo.TotalOwnCount;
                _rewardInfo.max_cnt = 0;
                CItemInvenManager.Instance.UpdateItemInven(_rewardInfo);
            }

            GameEnd_ResultInfo.Rewards.Add(rInfo);
        }
    }

    public GameResultInfo GetGameResultInfo()
    {
        return GameEnd_ResultInfo;
    }

    public List<int> GetWinnerList()
    {
        return GameEnd_ResultInfo.WinPlayerIDs;
    }

    //public List<StageRewardInfo> GetRewardInfoList()
    //{
    //    return GameEnd_ResultInfo.Rewards;
    //}

    #region SNAP_SHOT
    private MGPihagiSnapShotPlayer GetPihagiSnapShotPlayerInfo(BPWPacketDefine.PihagiGameSnapshotPlayer pInfo)
    {
        MGPihagiSnapShotPlayer _player = new MGPihagiSnapShotPlayer();
        _player.PlayerID = pInfo.PlayerId;
        _player.PlayerState = pInfo.State;
        _player.LifeCount = pInfo.Life;
        _player.Position = new Vector3(pInfo.PosX, 0, pInfo.PosZ);

        return _player;
    }

    private MGPihagiSnapShotEnemy GetPihagiSnapShotEnemyInfo(BPWPacketDefine.PihagiGameSnapshotEnemy eInfo)
    {
        MGPihagiSnapShotEnemy _enemy = new MGPihagiSnapShotEnemy();
        _enemy.EnemyID = eInfo.EnemyId;
        _enemy.EnemyTID = eInfo.PihagiGameTid;
        _enemy.EnemyState = eInfo.State;
        _enemy.Position = new Vector3(eInfo.PosX, 0, eInfo.PosZ);

        return _enemy;
    }

    public void SetPlayerSnapShotInfoDic(int PID, BPWPacketDefine.PihagiGameSnapshotPlayer pInfo)
    {
        MGPihagiSnapShotPlayer _playerInfo = GetPihagiSnapShotPlayerInfo(pInfo);

        if (PlayerSnapShotInfoDic.ContainsKey(PID) == false)
        {
            PlayerSnapShotInfoDic.Add(PID, _playerInfo);
        }
        else
        {
            PlayerSnapShotInfoDic[PID] = _playerInfo;
        }
    }

    public MGPihagiSnapShotPlayer GetPlayerSnapShotInfoDic(int PID)
    {
        if (PlayerSnapShotInfoDic.ContainsKey(PID))
        {
            return PlayerSnapShotInfoDic[PID];
        }

        return null;
    }

    public void SetEnemySnapShotInfoDic(int EID, BPWPacketDefine.PihagiGameSnapshotEnemy eInfo)
    {
        MGPihagiSnapShotEnemy _enemyInfo = GetPihagiSnapShotEnemyInfo(eInfo);

        if (EnemySnapShotInfoDic.ContainsKey(EID) == false)
        {
            EnemySnapShotInfoDic.Add(EID, _enemyInfo);
        }
        else
        {
            EnemySnapShotInfoDic[EID] = _enemyInfo;
        }
    }

    public MGPihagiSnapShotEnemy GetEnemySnapShotInfoDic(int EID)
    {
        if (EnemySnapShotInfoDic.ContainsKey(EID))
        {
            return EnemySnapShotInfoDic[EID];
        }

        return null;
    }

    public void SetRecvSnapshotTime(float time)
    {
        if (IsSinglePlay) return;
        if(SnapshotRecvTime != 0)
        {
            float timeLaytency = time - SnapshotRecvTime;

            //CDebug.Log( $"PihagiGameRoomSnapshotBc [SetRecvSnapshotTime] latency = {timeLaytency}" );
            if (timeLaytency > 5)
            {
                BPWorldExitEventProcessor.Instance.Regist( EBPWorldExitEventType.LATENCY_LOW_PIHAGI_GAME );
                return;
            }
        }
        SnapshotRecvTime = time;
    }



    public void SetStateLatencyTime(float time, bool notCheck = false, bool endbc = false)
    {
        if (IsSinglePlay) return;

        CDebug.Log( $" ______  Called SetStateLatencyTime notCheck:{notCheck},  time = {time}, StateLatencyTime = {StateLatencyTime} " );
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
            if(LaytencyDisposer != null)
            {
                LaytencyDisposer.Dispose();
                LaytencyDisposer = null;
            }
            LaytencyDisposer = new SingleAssignmentDisposable();

            CDebug.Log( $"[TCP] ______  SetStateLatencyTime LaytencyDisposer.Disposable" );
            LaytencyDisposer.Disposable = Observable.Interval( TimeSpan.FromSeconds( 1.0f ) ).Subscribe( x =>
            {
                long strID = 927012;
                if (endbc)
                {
                    strID = 927013;
                }
                float latency = Time.time - StateLatencyTime;
                CDebug.Log( $"[TCP] ______  SetStateLatencyTime latency = {latency}" );
                if (latency >= 10 && latency < 11)
                {
                    string strToastMessage = CResourceManager.Instance.GetString( strID ); //유저를 기다리는 중입니다.
                    Toast.MakeTextForContent( strToastMessage, Msg.MSG_TYPE.Notice );
                    LaytencyDisposer.Dispose();
                }
            } );

        }

    }

    public void ClearGamePlayerInfoDic()
    {
        if (GamePlayerInfoDic != null) GamePlayerInfoDic.Clear();
    }
    #endregion SNAP_SHOT


    public void CleanUpServerData()
    {
        //stageType = BPWPacketDefine.PihagiGameStageType.NONE;
        if (GameRoomInfo != null) GameRoomInfo = null;
        ClearGamePlayerInfoDic();
        if (PlayerSnapShotInfoDic != null) PlayerSnapShotInfoDic.Clear();
        if (EnemySnapShotInfoDic != null) EnemySnapShotInfoDic.Clear();
        if (GameEnd_ResultInfo != null) GameEnd_ResultInfo = null;
        SnapshotRecvTime = 0;
        StateLatencyTime = 0;
        if (LaytencyDisposer != null)
        {
            CDebug.Log( $"[TCP] ______  SetStateLatencyTime LaytencyDisposer.Dispose();" );
            LaytencyDisposer.Dispose();
        }
    }


    public void Release()
    {
        ClearGamePlayerInfoDic();
        if (PlayerSnapShotInfoDic != null)
        {
            PlayerSnapShotInfoDic.Clear();
        }
        if (EnemySnapShotInfoDic != null)
        {
            EnemySnapShotInfoDic.Clear();
        }
        if (GameEnd_ResultInfo != null)
        {
            GameEnd_ResultInfo = null;
        }


        if (LaytencyDisposer != null)
        {
            LaytencyDisposer.Dispose();
            LaytencyDisposer = null;
        }
        SnapshotRecvTime = 0;
        StateLatencyTime = 0;
    }
}
