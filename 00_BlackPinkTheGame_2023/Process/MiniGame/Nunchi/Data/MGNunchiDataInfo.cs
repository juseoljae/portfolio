
public class MGNunchiDataInfo
{
    public long ID { get; set; }
    public long GroupID { get; set; }
    public byte Round { get; set; }
    public byte RoundTime { get; set; }

    public CoinCapacity[] CoinNum = new CoinCapacity[4];

    //public byte[] CoinMin = new byte[4];

    //public byte[] CoinMax = new byte[4];
    public BPWPacketDefine.NunchiGameItemType SpecialType { get; set; }
    public byte SpecialCount { get; set; }
}