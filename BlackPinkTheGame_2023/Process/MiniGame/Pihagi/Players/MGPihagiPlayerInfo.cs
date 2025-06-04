using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MGPihagiPlayerInfo : NetPlayerInfo
{
    public int PlayerID;//unique ID in game room
    public int LifeCount;
    public Vector3 SpawnPosition;

    public bool IsBot;
    public StylingInfo StylingItemInfo;
}

public class MGPihagiSnapShotPlayer
{
    public int PlayerID;
    public BPWPacketDefine.PihagiGamePlayerState PlayerState;
    public Vector3 Position;
    public int LifeCount;
}

public class MGPihagiSnapShotEnemy
{
    public int EnemyID;
    public long EnemyTID;
    public BPWPacketDefine.PihagiGameEnemyState EnemyState;
    public Vector3 Position;
}