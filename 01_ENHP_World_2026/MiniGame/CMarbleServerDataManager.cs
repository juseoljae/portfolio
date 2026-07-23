using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.RestAPI;

public class CMarbleServerDataManager : Singleton<CMarbleServerDataManager>
{
    private DicegameInfo GameInfo;
    private DicegameInfo RenewalGameInfo;
    private DicegameAp GameApInfo;

    private List<DicegameTileMap> MapInfos;
    private List<DicegameTileMap> RenewalMapInfos;

    private List<RewardList> RewardInfos;
    private DicegameSelectedInfo MarbleMemberSelectInfo;
    private bool IsRefreshApInfo;

    public void SetMarbleGameInfo(DicegameInfo gameInfo, List<DicegameTileMap> mapInfos)
    {
        SetMarbleGameInfo(gameInfo);
        MapInfos = mapInfos;
    }

    public void SetMarbleGameInfo(DicegameInfo gameInfo)
    {
        GameInfo = gameInfo;
    }

    public void SetGameApInfo(DicegameAp gameApInfo)
    {
        GameApInfo = gameApInfo;
        IsRefreshApInfo = true;
    }

    public void SetMarbleMemberSelectInfo(DicegameSelectedInfo selectInfo)
    {
        MarbleMemberSelectInfo = selectInfo;
    }

    public void SetRenewalMapInfos(DicegameInfo gameInfo, List<DicegameTileMap> renewalMapInfos)
    {
        RenewalGameInfo = gameInfo;
        RenewalMapInfos = renewalMapInfos;
    }

    public void UpdateGameInfoToRenewal()
    {
        GameInfo = RenewalGameInfo;
        MapInfos = RenewalMapInfos;
    }

    public void SetRewardInfos(List<RewardList> rewardInfos)
    {
        RewardInfos = rewardInfos;
    }

    public DicegameInfo GetGameInfo()
    {
        return GameInfo;
    }

    public List<DicegameTileMap> GetMapInfos()
    {
        return MapInfos;
    }

    public DicegameAp GetGameApInfo()
    {
        return GameApInfo;
    }

    public List<RewardList> GetRewardInfos()
    {
        return RewardInfos;
    }

    public int GetCurrentAp()
    {
        if (GameApInfo != null)
        {
            return GameApInfo.ap;
        }
        return 0;
    }

    public MEMBER_TYPE GetMarbleMemberSelectInfo()
    {
        if (MarbleMemberSelectInfo != null)
        {
            if (MarbleMemberSelectInfo.member_id == 0)
            {
                return MEMBER_TYPE.NONE;
            }
            
            return MarbleMemberSelectInfo.member_id.ToEnum<MEMBER_TYPE>();
        }

        return MEMBER_TYPE.NONE;
    }

    public void SetIsRefreshApInfo(bool isRefresh)
    {
        IsRefreshApInfo = isRefresh;
    }

    public bool GetIsRefreshApInfo()
    {
        return IsRefreshApInfo;
    }

    public List<DicegameTileMap> GetRenewalMapInfos()
    {
        return RenewalMapInfos;
    }
}
