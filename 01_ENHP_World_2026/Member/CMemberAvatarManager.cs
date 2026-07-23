using SNG;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Game.RestAPI;
using UniRx;
using CharacterControl;

public class CMemberAvatarManager : SingletonMono<CMemberAvatarManager>
{
    public Dictionary<MEMBER_TYPE, CMemberAvatar> MemberAvatarDic = new ();

    public GameObject MemberAvatarRootObj = null;

    private MEMBER_TYPE UICurMemberType = MEMBER_TYPE.NONE;

    private List<TownNPCSimpleMove> TownNPCSimpleMoves = null;

    private MemberAnimationData MemberDanceAnimData;
    public float DanceAnimLength { get; set; }

    public bool IsBlockInteracting { get; set; }

    public void Init()
    {
        if (MemberAvatarDic == null)
        {
            MemberAvatarDic = new Dictionary<MEMBER_TYPE, CMemberAvatar>();
        }
        MemberAvatarRootObj = TownMapMgr.Inst.MemberRootObj;


        MemberDanceAnimData = null;

        CRuntimeAnimControllerManager.Instance.SetControllerDic();
    }


    public void LateUpdate()
    {
        InteractionProc();
    }

    private bool CheckCanInteractionProc()
    {
        if (CDirector.Instance.GetCurrentSceneID() != CSceneId.SNG_SCENE)
            return false;

        if (IsBlockInteracting)
            return false;

        if (LoadingLayer.Instance.IsLoading())
            return false;

        if (TownNPCSimpleMoves == null || TownNPCSimpleMoves.Count == 0)
            return false;
        return true;
    }

    private void InteractionProc()
    {
        if (!CheckCanInteractionProc())
            return;

        for (int i = 0; i < TownNPCSimpleMoves.Count; ++i)
        {
            for (int j = i + 1; j < TownNPCSimpleMoves.Count; ++j)
            {
                TownNPCSimpleMove avatar1 = TownNPCSimpleMoves[i];
                TownNPCSimpleMove avatar2 = TownNPCSimpleMoves[j];

                // if (!avatar1.CheckCanInteracting() || !avatar2.CheckCanInteracting())
                // {
                //     continue;
                // }
                bool a1CanInteract = avatar1.CheckCanInteracting();
                bool a2CanInteract = avatar2.CheckCanInteracting();

                //Debug.Log($"CheckCanInteracting: avatar1:{avatar1.gameObject.name}, avatar2: {avatar2.gameObject.name}, a1Can: {a1CanInteract}, a2Can: {a2CanInteract}");

                if (a1CanInteract && a2CanInteract)
                {
                    float distance = Vector3.Distance(avatar1.transform.position, avatar2.transform.position);
                    if (distance >= SNGDefines.MEMBER_INTERACTION_DISTANCE_MIN && distance < SNGDefines.MEMBER_INTERACTION_DISTANCE)
                    {
                        SetInteracting(avatar1, avatar2);
                        return;
                    }
                }
            }
        }
    }

    private void SetInteracting(TownNPCSimpleMove avatar1, TownNPCSimpleMove avatar2)
    {
        eAniBehavior random = (eAniBehavior)Random.Range((int)eAniBehavior.ANI_INTERACTION01, (int)eAniBehavior.ANI_INTERACTION04 + 1);

        avatar1.SetInteracting(random, avatar2);
        avatar2.SetInteracting(random, avatar1);

        SetIsBlockInteracting(true);
    }

    public void SetIsBlockInteracting(bool isBlock)
    {
        IsBlockInteracting = isBlock;
    }

    //callsed from server response
    public void SetMemberAvatarInfo(MEMBER_TYPE type, CMemberAvatar _avatar)
    {
        if (MemberAvatarDic.ContainsKey(type))
        {
            MemberAvatarDic[type] = _avatar;
        }
        else
        {
            MemberAvatarDic.Add(type, _avatar);
        }
    }

    public void UpdateMemberAvatarInfo(AvatarList avatarList, CMemberAvatar avatar)
    {
        if (MemberAvatarDic.ContainsKey(avatar.MemberType))
        {
            MemberAvatarDic[avatar.MemberType] = avatar;
        }
    }

    public void SetTownNpcSimpleMoveList()
    {
        if (TownNPCSimpleMoves == null && MemberAvatarDic != null)
        {
            TownNPCSimpleMoves = MemberAvatarDic.Values.Select(avatar => avatar.GetTownNPCSimpleMove()).ToList();
        }
    }

    public void MoveStartAllMemberAvatar(bool bStop = false)
    {
        if (MemberAvatarDic == null) return;
        foreach (var avatar in MemberAvatarDic.Values)
        {
            TownNPCSimpleMove simpleMove = avatar.GetTownNPCSimpleMove();
            if (simpleMove != null)
            {
                if (simpleMove.IsMemberInteracting)
                    simpleMove.FinishInteracting(false, 0);

                if (bStop)
                {
                    simpleMove.StopMoving();
                }
                else
                {
                    simpleMove.StartMoving(SNGDefines.CHAR_MOVE_DELAY);
                }
                simpleMove.SetActiveGiftIcon(!bStop);
                simpleMove.SetActiveChatBaloonIcon(!bStop);
            }
        }
    }

    public async Task LoadAllMember()
    {
        List<Task> allSpawnTasks = new List<Task>();
        List<Task> avatarSpawnTasks = GetAvatarSpawnTasks();
        allSpawnTasks.AddRange(avatarSpawnTasks);

        await Task.WhenAll(allSpawnTasks);
    }

    public List<Task> GetAvatarSpawnTasks()
    {
        List<Task> spawnTasks = new List<Task>();

        List<AvatarList> avatarInfos = CMemberAvatarServerDataManager.Instance.GetMemberAvatarInfoDic().Values.ToList();

        if (avatarInfos != null)
        {
            foreach (var avatarInfo in avatarInfos)
            {
                spawnTasks.Add(AsyncAvatarSpawn(avatarInfo));
            }
        }

        return spawnTasks;
    }

    public async Task AsyncAvatarSpawn(AvatarList avatarInfo)
    {
        MEMBER_TYPE mType = (MEMBER_TYPE)avatarInfo.character_id;
        CMemberAvatar avatar = new CMemberAvatar(mType);
        Task<CoupleData> loadTask = CMemberAvatarManager.Instance.AsyncLoadBaseAvatar(mType, avatarInfo, avatar);

        CoupleData coupleData = await loadTask;

        SetMemberAvatarInfo(mType, avatar);
        SpawnAvatarObj(avatar, avatarInfo);
    }

    public Task<CoupleData> AsyncLoadBaseAvatar(MEMBER_TYPE type, AvatarList avatarInfo, CMemberAvatar _avatar)
    {
        return _avatar.AsyncLoadBaseAvatarObj(avatarInfo, MemberAvatarRootObj);
    }

    public void AvatarSpawn()
    {
        Dictionary<long, AvatarList> infoDic = CMemberAvatarServerDataManager.Instance.GetMemberAvatarInfoDic();
        if (infoDic != null)
        {
            foreach (AvatarList avatarInfo in infoDic.Values)
            {
                MEMBER_TYPE mType = (MEMBER_TYPE)avatarInfo.character_id;
                LoadBaseAvatar(mType, avatarInfo);
            }
        }
    }

    private void LoadBaseAvatar(MEMBER_TYPE type, AvatarList avatarInfo)
    {
        CMemberAvatar _avatar = new CMemberAvatar(type);
        _avatar.LoadBaseAvatarObj(avatarInfo, MemberAvatarRootObj);

        SetMemberAvatarInfo(type, _avatar);
        SpawnAvatarObj(_avatar, avatarInfo);
    }

    private void SpawnAvatarObj(CMemberAvatar avatar, AvatarList avatarInfo)
    {
        CitizenSpawnInfo spawnInfo = CCitizenDataManager.Instance.GetSpawnInfoRandomly();
        GameObject avatarObj = avatar.GetAvatarObj();
        BasicTile tileComp = GetRandomSpawnTile(spawnInfo, avatarObj);

        MEMBER_TYPE mType = (MEMBER_TYPE)avatarInfo.character_id;


        avatar.SetOutLine();

        SetLayerRecursively(avatarObj, SNG_LayerTag.Layer_CHAR);

        avatarObj.transform.position = new Vector3(tileComp.transform.position.x, TownMapMgr.Inst.AvatarBaseHeight, tileComp.transform.position.z);

        CMemberAvatarController cntlr = avatar.GetMemberAvatarController();

        TownNPCSimpleMove simpleMove = avatar.GetTownNPCSimpleMove();

        if (avatarInfo.character_id == SNGDataManager.Instance.NeocityData.gift_avatar)
        {
            if (!SNGDataManager.Instance.IsFriendTown)
            {
                if (SNGDataManager.Instance.NeocityData.gift_state == 1 && SNGDataManager.Instance.NeocityData.gift_remain <= 0)
                {
                    simpleMove.CreateCharStatusIcon();
                    simpleMove.StartCoroutine("CoSetGiftState");
                }
                else
                {
                    simpleMove.StartMoving(SNGDefines.CHAR_MOVE_DELAY);
                }
            }
            else
            {
                simpleMove.StartCoroutine("CoSetGiftState");
            }
        }
        else
        {
            simpleMove.StartMoving(SNGDefines.CHAR_MOVE_DELAY);
        }

        TownNPCSimpleTouch simpletouch = avatarObj.GetComponent<TownNPCSimpleTouch>();
        if (!simpletouch)
        {
            simpletouch = avatarObj.AddComponent<TownNPCSimpleTouch>();
        }
        
        //CMemberAvatarController cntlr = avatar.GetMemberAvatarController();
        if (cntlr != null)
        {
            simpletouch.InitAvatarController(cntlr);
        }

    }

    public List<GameObject> AllMemberAvatarObj()
    {
        return MemberAvatarDic?.Values
            .Where(v => v?.MemberAvatarObj != null)
            .Select(v => v.MemberAvatarObj)
            .ToList() ?? new List<GameObject>();
    }

    public void SetLayerRecursively(GameObject obj, string layerName)
    {
        if (obj == null) return;

        int layer = LayerMask.NameToLayer(layerName);
        if (layer == -1)
        {
            return;
        }
        obj.layer = layer;

        foreach (Transform child in obj.transform)
        {
            if (child != null)
            {
                SetLayerRecursively(child.gameObject, layerName);
            }
        }
    }



    private BasicTile GetRandomSpawnTile(CitizenSpawnInfo spawnData, GameObject charObj)
    {
        float rndX = 0;
        float rndZ = 0;
        if (spawnData != null)
        {
            rndX = UnityEngine.Random.Range(spawnData.SpawnPosX, spawnData.SpawnPosX + spawnData.SpawnRange);
            rndZ = UnityEngine.Random.Range(spawnData.SpawnPosZ, spawnData.SpawnPosZ + spawnData.SpawnRange);
        }

        return TownMapMgr.Inst.GetTileByPos(new Vector2(rndX, rndZ));
    }

    public void EquipMemberAvatarStylingItems(MEMBER_TYPE mType)
    {
        if (MemberAvatarDic.ContainsKey(mType))
        {
            CMemberAvatar _avatar = MemberAvatarDic[mType];

            _avatar.EquipAllStylingItems();
        }
    }

    public CMemberAvatar GetMemberAvatar(MEMBER_TYPE type)
    {
        if (MemberAvatarDic.ContainsKey(type))
        {
            return MemberAvatarDic[type];
        }
        return null;
    }

    public void SetUICurMemberType(MEMBER_TYPE type)
    {
        UICurMemberType = type;
    }

    public MEMBER_TYPE GetUICurMemberType()
    {
        return UICurMemberType;
    }

    public void UpdateMemberAvatarCurEquipSItem(AvatarList avatarList)
    {
        CMemberAvatarServerDataManager.Instance.UpdateMemberAvatarInfo(avatarList);
        MEMBER_TYPE mType = (MEMBER_TYPE)avatarList.character_id;
        CMemberAvatar avatar = GetMemberAvatar(mType);
        if (avatar != null)
        {
            avatar.UpdateCurEquipSItemDic(avatarList);
            avatar.EquipAllStylingItems();
            avatar.SetMemberAvatarObjScale();
        }
    }

    public AvatarList GetAvatarListToSaveStyleItemByItemType(STYLE_ITEM_TYPE itemType, CMemberAvatar memberAvatar, out int partID)
    {
        partID = -1;
        AvatarList avatarInfo = CMemberAvatarServerDataManager.Instance.GetMemberAvatarInfo((int)memberAvatar.MemberType);
        if (avatarInfo != null)
        {

            int id = GetStyleItemIDOnPutOn(memberAvatar, itemType);
            switch (itemType)
            {
                case STYLE_ITEM_TYPE.HAIR:
                    partID = avatarInfo.parts1;
                    if (id != -1 && partID != id)
                    {
                        avatarInfo.parts1 = id;
                    }
                    break;
                case STYLE_ITEM_TYPE.SKIN:
                    partID = avatarInfo.parts2;
                    if (id != -1 && partID != id)
                    {
                        avatarInfo.parts2 = id;
                    }
                    break;
                case STYLE_ITEM_TYPE.ACC_HEAD:
                    partID = avatarInfo.parts3;
                    if (id != -1 && partID != id)
                    {
                        avatarInfo.parts3 = id;
                    }
                    break;
                case STYLE_ITEM_TYPE.ACC_FACE:
                    partID = avatarInfo.parts4;
                    if (id != -1 && partID != id)
                    {
                        avatarInfo.parts4 = id;
                    }
                    break;
                case STYLE_ITEM_TYPE.ACC_BODY:
                    partID = avatarInfo.parts5;
                    if (id != -1 && partID != id)
                    {
                        avatarInfo.parts5 = id;
                    }
                    break;
            }
        }

        return avatarInfo;
    }



    // memberAvatar is for ui avatar. not static avatar.
    public AvatarList GetAvatarListToSaveStyleItemAsPutOn(CMemberAvatar memberAvatar)
    {
        AvatarList avatarInfo = CMemberAvatarServerDataManager.Instance.GetMemberAvatarInfo((int)memberAvatar.MemberType);
        if (avatarInfo != null)
        {
            int id = GetStyleItemIDOnPutOn(memberAvatar, STYLE_ITEM_TYPE.HAIR);
            if (id != -1)
            {
                if (avatarInfo.parts1 != id)
                {
                    avatarInfo.parts1 = id;
                }
            }

            id = GetStyleItemIDOnPutOn(memberAvatar, STYLE_ITEM_TYPE.SKIN);
            if (id != -1)
            {
                if (avatarInfo.parts2 != id)
                {
                    avatarInfo.parts2 = id;
                }
            }

            id = GetStyleItemIDOnPutOn(memberAvatar, STYLE_ITEM_TYPE.ACC_HEAD);
            if (id != -1)
            {
                if (avatarInfo.parts3 != id)
                {
                    avatarInfo.parts3 = id;
                }
            }
            else
            {
                avatarInfo.parts3 = 0;
            }

            id = GetStyleItemIDOnPutOn(memberAvatar, STYLE_ITEM_TYPE.ACC_FACE);
            if (id != -1)
            {
                if (avatarInfo.parts4 != id)
                {
                    avatarInfo.parts4 = id;
                }
            }
            else
            {
                avatarInfo.parts4 = 0;
            }

            id = GetStyleItemIDOnPutOn(memberAvatar, STYLE_ITEM_TYPE.ACC_BODY);
            if (id != -1)
            {
                if (avatarInfo.parts5 != id)
                {
                    avatarInfo.parts5 = id;
                }
            }
            else
            {
                avatarInfo.parts5 = 0;
            }
        }

        return avatarInfo;
    }

    private int GetStyleItemIDOnPutOn(CMemberAvatar memberAvatar, STYLE_ITEM_TYPE itemType)
    {
        if (memberAvatar != null)
        {
            if (memberAvatar.PutOnEquipSItemDic.TryGetValue(itemType, out var itemInfo) && itemInfo?.ItemData != null)
            {
                return itemInfo.ItemData.ID;
            }
        }

        return -1;
    }

    public bool IsNewMemberHouse()
    {
        foreach (var avatar in MemberAvatarDic.Values)
        {
            return CNewAlertManager.Instance.CheckNewAlert(NEWALERT_TYPE.CHRACTER_HOUSE_DROPDOWN, (int)avatar.MemberType);
        }

        return false;
    }
    public void SetMemberDanceAnimData()
    {
        List<MemberAnimationData> memberAnimDataList = CMemberAvatarDataManager.Instance.GetMemberAnimDataList(eAniBehavior.ANI_DANCE);
        if (memberAnimDataList != null && memberAnimDataList.Count > 0)
        {
            int randIdx = Random.Range(0, memberAnimDataList.Count);
            MemberDanceAnimData = memberAnimDataList[randIdx];
        }
    }

    public MemberAnimationData GetMemberDanceAnimData()
    {
        return MemberDanceAnimData;
    }

    

    public void UpdateAllAvatarFatigability()
    {
        foreach (var avatar in MemberAvatarDic.Values)
        {
            var avatarlist = CMemberAvatarServerDataManager.Instance.GetMemberAvatarInfo((int)avatar.MemberType);
            if (avatarlist != null)
            {
                avatar.MemberAvatarController.SetFatigrade(avatarlist.fatigability_grade);
            }
        }

        Dictionary<long, AvatarList> allAvatars = CMemberAvatarServerDataManager.Instance.GetMemberAvatarInfoDic();
        foreach (var avatarInfo in allAvatars.Values)
        {
            if (avatarInfo.fatigability > 0)
            {
                avatarInfo.fatigability -= 1;
            }
        }
    }

    public void Release()
    {
        if (TownNPCSimpleMoves != null)
        {
            foreach (var sm in TownNPCSimpleMoves)
            {
                sm.Release();
            }
            TownNPCSimpleMoves.Clear();
            TownNPCSimpleMoves = null;
        }

        if (MemberAvatarDic != null)
        {
            foreach (var avatar in MemberAvatarDic.Values)
            {
                avatar.Release();
            }
            MemberAvatarDic.Clear();
            MemberAvatarDic = null;
        }
    }
}
