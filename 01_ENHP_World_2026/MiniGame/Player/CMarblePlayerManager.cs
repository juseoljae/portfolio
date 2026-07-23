using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.RestAPI;

public class CMarblePlayerManager : MonoBehaviour
{
    public CMarbleWorldManager WorldMgr;
    private CMarblePlayer MarblePlayer;
    private CMemberAvatar MemberAvatar;
    
    private DicegameInfo GameInfo;

    private MEMBER_TYPE CurMemberType = MEMBER_TYPE.JUNGWON;


    public void Initialize(CMarbleWorldManager worldMgr)
    {
        WorldMgr = worldMgr;

        SetMarblePlayer();

    }

    public void SetMarblePlayer()
    {
        GameInfo = CMarbleServerDataManager.Instance.GetGameInfo();

        CurMemberType = WorldMgr.GetCurrentMemberType();

        SetPlayer();

        if (MemberAvatar != null)
        {
            MarblePlayer = MemberAvatar.MemberAvatarObj.AddComponent<CMarblePlayer>();
            MarblePlayer.Init(this, MemberAvatar, GameInfo);
        }
        //MarblePlayer = new CMarblePlayer();
        // MarblePlayer.Init(this, CurMemberType);


    }

    
    public void SetPlayer()
    {
        AvatarList _info = CMemberAvatarServerDataManager.Instance.GetMemberAvatarInfo((int)CurMemberType);
        if (_info == null)
        {
            Debug.LogError("CMarblePlayer::SetPlayer - Avatar info is null for type: " + CurMemberType);
            return;
        }

        MemberAvatar = new CMemberAvatar(CurMemberType);
        MemberAvatar.LoadFieldAvatarObject(_info, gameObject, CMarbleDefine.AVATAR_SCALE, CMarbleDefine.AVATAR_SPAWN_HEIGHT);
    }

    public void SetPlayerState(MARBLE_PLAYER_STATE state)
    {
        if (MarblePlayer != null)
        {
            MarblePlayer.SetPlayerState(state);
        }
    }

    public void SetRestorePlayer()
    {
        if (MarblePlayer != null)
        {
            MarblePlayer.RestorePlayer(0.2f);
        }
    }

    public void SetPlayerPosition()
    {
        if (MarblePlayer != null)
        {
            MarblePlayer.SetCurPosition();
        }
    }

    public void SetBlockStateByBlockType(CMarbleBlock curBlock, bool atFirst = false)
    {
        if (MarblePlayer != null)
        {
            MarblePlayer.SetBlockStateByBlockType(curBlock, atFirst);
        }
    }

    public void SetActiveMoveEff_CharTrail(bool isActive)
    {
        if (MarblePlayer != null)
        {
            MarblePlayer.SetActiveMoveEff_CharTrailObjs(isActive);
        }
    }

    public void SetActivePlayerObj(bool isActive)
    {
        if (MarblePlayer != null)
        {
            MarblePlayer.gameObject.SetActive(isActive);
        }
    }

    public MARBLE_PLAYER_STATE GetPlayerState()
    {
        if (MarblePlayer != null)
        {
            return MarblePlayer.GetPlayerState();
        }

        return MARBLE_PLAYER_STATE.IDLE;
    }

    public CState<CMarblePlayer> GetPlayerCurrentState()
    {
        if (MarblePlayer != null)
        {
            return MarblePlayer.GetCurrentState();
        }

        return null;
    }

    public MEMBER_TYPE GetCurrentMemberType()
    {
        return CurMemberType;
    }

    public void RestorePlayerAnimatorController()
    {
        if (MarblePlayer != null)
        {
            MarblePlayer.RestoreAnimatorController();
        }
    }

    // Update is called once per frame
    public void UpdatePlayerStateMachine()
    {
        if (MarblePlayer != null)
        {
            MarblePlayer.UpdateStateMachine();
        }
    }

    public GameObject GetCurrentPlayerObj()
    {
        if (MarblePlayer != null)
        {
            return MarblePlayer.gameObject;
        }

        return null;
    }

    public void SetIsRestorePlayer(bool isRestore)
    {
        if (MarblePlayer != null)
        {
            MarblePlayer.SetIsRestorePlayer(isRestore);
        }
    }

    public void Release()
    {
        if (MarblePlayer != null)
        {
            MarblePlayer.Release();
            MarblePlayer = null;
        }
    }
}
