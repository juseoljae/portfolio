using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Game.RestAPI;
using UniRx;
using Newtonsoft.Json;

public class MGMarbleService
{
    string UID { get { return CPlayer.GetStatusLoginValue(PLAYER_STATUS.USER_ID); } }
    
    public IObservable<MinigameDicegame_topRes.Root> MGMarbleTop(int fromNavi = 0)
    {
        return GameRestAPI.MinigameDicegame_top(UID, fromNavi.ToString())
        .Do(res =>
        {
            if (res.d.new_mark != null)
            {
                CNewAlertManager.Instance.SetNewMarkData(res.d.new_mark);
            }
            CDebug.Log($" $$$$ MGMarbleTop()");
        });
    }

    public IObservable<MinigameDicegame_rollRes.Root> MGMarbleDiceRoll(int rangeID)
    {
        return GameRestAPI.MinigameDicegame_roll(UID, rangeID.ToString())
        .Do(res =>
        {
            CMarbleServerDataManager.Instance.SetGameApInfo(res.d.dicegame_ap);
            CDebug.Log($" $$$$ MGMarbleDiceRoll()");
            try
            {
                CCoreServices.GetCoreService<GameLogService>().MGMarble_DiceRoll(res, rangeID);
            }
            catch (Exception e)
            {
                CDebug.Log(e?.Message);
            }
        });
    }

    public IObservable<MinigameDicegame_tile_selectRes.Root> MGMarbleBlockSelect(int blockIndex)
    {
        return GameRestAPI.MinigameDicegame_tile_select(UID, blockIndex.ToString())
        .Do(res =>
        {
            CDebug.Log($" $$$$ MGMarbleBlockSelect()");
            try
            {
                CCoreServices.GetCoreService<GameLogService>().MGMarble_BlockSelect(res, blockIndex);
            }
            catch (Exception e)
            {
                CDebug.Log(e?.Message);
            }
        });
    }

    public IObservable<MinigameDicegame_member_listRes.Root> MGMarbleMemberList()
    {
        return GameRestAPI.MinigameDicegame_member_list(UID)
        .Do(res =>
        {
            CDebug.Log($" $$$$ MGMarbleMemberList()");
        });
    }

    public IObservable<MinigameDicegame_member_selectRes.Root> MGMarbleMemberSelect(MEMBER_TYPE memberType)
    {
        int memberTypeIdx = (int)memberType;

        return GameRestAPI.MinigameDicegame_member_select(UID, memberTypeIdx.ToString())
        .Do(res =>
        {
            CMarbleServerDataManager.Instance.SetMarbleGameInfo(res.d.dicegame_info, res.d.dicegame_tile_map);
            CDebug.Log($" $$$$ MGMarbleMemberSelect()");
            try
            {
                CCoreServices.GetCoreService<GameLogService>().MGMarble_MemberSelect(res, memberTypeIdx);
            }
            catch (Exception e)
            {
                CDebug.Log(e?.Message);
            }
        });
    }

    public IObservable<MinigameDicegame_ap_refreshRes.Root> MGMarbleAP_Refresh()
    {
        return GameRestAPI.MinigameDicegame_ap_refresh(UID)
        .Do(res =>
        {
            CMarbleServerDataManager.Instance.SetGameApInfo(res.d.dicegame_ap);
            CDebug.Log($" $$$$ MGMarbleAP_Refresh()");
        });
    }
}
