
public class MGPihagiGameDataInfo
{
    public long ID { get; set; }
    public byte Time { get; set; }
    public long SpawnGroupID { get; set; }
    public byte SpawnPoint { get; set; }
    public long EnemyID { get; set; }
    public int EnemyDelayTime { get; set; }
}

public class MGPihagiEnemyInfo
{
    public long ID { get; set; }
    public ENEMY_TYPE Type { get; set; }
    public long ControllerGrpID { get; set; }
    public string Resource { get; set; }
    public int Speed { get; set; }

    //public ENEMY_POSDIR PosDir { get; set; }
    //public byte SpawnPosition { get; set; }
    //public int SpawnTime { get; set; }
    //public int DelayTime { get; set; }

}