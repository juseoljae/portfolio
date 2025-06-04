using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using KSPlugins;

public class RaidManager : KSManager
{
    #region INNER_CLASS
    public class RaidMemberInfo
    {
        public long position;    // 방 안에서의 슬롯 위치
        public PlayerInfo rMemInfo;
        public bool bAI;
        public long pvpScore;
        public long pvpRank;

        //public 
        //public string name;       // 파티원 이름.
        //public eCharacter attrclass;
        //public int level;
        //public EquipedItems m_equipedItems;    // 장착해서 외부에 보이는 아이템들만 포함됨.
        //public List<EquipItem> m_inventoryItems = new List<EquipItem>();
    }

    public class RaidPcInfo
    {
        public long id;                  // 한 레이드 내에서 등장하는 오브젝트를 서버가 식별하는 식별자.
        public string name;
        public long iHP;
        public long imax_hp;
        public long istance;
        public Vector3 iStartPos;
    }

    public enum SelectedMode
    {
        NONE,
        RAID,
        PVP,
        FREE_PVP,
        FREE_FIGHT,
#if NONSYNC_PVP
        NONSYNC_PVP,
#endif
#if DAILYDG
        DAILYDG,
#endif
        MAX
    }

    //public enum eSKILLSTATUS : byte
    //{
    //    SKILL_STATUS_UNEQUIPPED = 0,
    //    SKILL_STATUS_EQUIPPED1,
    //    SKILL_STATUS_EQUIPPED2,
    //    SKILL_STATUS_EQUIPPED3,
    //    SKILL_STATUS_EQUIPPED4,
    //}

    //public class RaidMemberSkillInfo
    //{
    //    public int id;
    //    public eSKILLSTATUS status;
    //    public usingSeal;
    //    public 
    //}
    #endregion INNER_CLASS

    #region     INSTANCE
    private static RaidManager m_Instance;
    public static RaidManager instance
    {
        get
        {
            if (m_Instance == null)
            {
                if (MainManager.instance != null)
                    m_Instance = CreateManager(typeof(RaidManager).Name).AddComponent<RaidManager>();
                else
                    m_Instance = new RaidManager();
            }

            return m_Instance;
        }
    }
    #endregion  INSTANCE

    #region VARIABLES
    //public List<RaidMemberInfo> m_PartyMemberList;
    public RaidMemberInfo[] m_PartyMemberList;
#if NONSYNC_PVP
    public CharacterLobbyInfo m_NonPvpPlayer;
    public List<SummonData> m_NonSyncPvpPlayerPets;
    public List<SummonSlotData> m_NonSyncPvpPlayerPetSlot;
    public Dictionary<int, SkillComboSet> m_AttackComboSet;
    public Dictionary<int, blame_messages.SkillInfo> m_NonSyncPvpPlayerSkills;
    public List<int> m_NonSyncPvpPlayerSkillCodes;
#endif
    public string[] m_DeletedMembers;
    public int m_DeletedMemberCnt;
    public List<blame_messages.MultiNpcInfo> m_NpcList=null;
    public string m_MultiNewUserName;
    public string m_MasterRaidName;
    public bool m_bPublicRaid = true;

    //public List<string> m_DeletedMembers;

    public RaidPcInfo[] m_RaidPcInfo;

    public RaidStateData m_CurRaidStateData = new RaidStateData();

    public float m_fRaidBossFuryGage = 0.0f;
    private float m_fRaidBossGroggyRemainTime = 0.0f;
    private float m_fRaidBossGroggyTime = 0.0f;

    private int m_DungeonID;

    public SelectedMode m_SelectedMode = SelectedMode.NONE;

    public int m_strLastDungeonIndex = 0;
    public string m_strDungeonKey;

    public enum eRaidQuickPlay
    {
        NONE,
        QUICK,
        RETRY,
        INVITE,
        MAX
    }
    public eRaidQuickPlay m_eQuickPlay = eRaidQuickPlay.NONE;

    public GameObject RaicActorObj;


    #endregion VARIABLES

    #region CONST
    public const int CONST_RAID_MAX_MEMBER = 8;
    #endregion CONST


    #region     OVERRIDE_METHODS
    public override void Initialize()
    {
        //m_PartyMemberList = new List<RaidMemberInfo>();
        m_PartyMemberList               = new RaidMemberInfo[CONST_RAID_MAX_MEMBER];
        m_DeletedMembers                = new string[CONST_RAID_MAX_MEMBER];
        m_RaidPcInfo                    = new RaidPcInfo[CONST_RAID_MAX_MEMBER];

#if NONSYNC_PVP
        m_NonSyncPvpPlayerPets = new List<SummonData>();
        m_NonSyncPvpPlayerPetSlot = new List<SummonSlotData>();
#endif

        InitVarInRaidStart();

        //default value setting
        //dungeonID = 20001;
        dungeonID = 20010;

        m_NpcList = new List<blame_messages.MultiNpcInfo>();

        m_MultiNewUserName = "NONE";

//        m_SelectedMode = SelectedMode.NONE;
    }

    public void InitVarInRaidStart()
    {
        for (int i = 0; i < m_PartyMemberList.Length; i++)
        {
            m_PartyMemberList[i] = new RaidMemberInfo();
            m_PartyMemberList[i].rMemInfo = new PlayerInfo();
            m_PartyMemberList[i].position = -1;
            m_PartyMemberList[i].rMemInfo.strName = null;
        }

        for (int i = 0; i < m_DeletedMembers.Length; i++)
        {
            m_DeletedMembers[i] = null;
        }
        m_DeletedMemberCnt = 0;

        if (m_RaidPcInfo != null)
        {
            for (int i = 0; i < m_RaidPcInfo.Length; i++)
            {
                m_RaidPcInfo[i] = new RaidPcInfo();
                m_RaidPcInfo[i].id = -1;
                m_RaidPcInfo[i].name = null;
            }
        }

        if (SceneManager.instance.GetCurGameMode() == SceneManager.eGAMEMODE.eMODE_RAID)
        {
            if (NpcManager.instance.NpcBoss != null)
            {
                m_CurRaidStateData.m_iNpcIndex = (int)NpcManager.instance.NpcBoss.m_CharAi.GetNpcProp().Npc_Code;
                m_CurRaidStateData.m_iGroggySkill = CDataManager.dyRaidStateData[m_CurRaidStateData.m_iNpcIndex].m_iGroggySkill;
                m_CurRaidStateData.m_iFurySkill = CDataManager.dyRaidStateData[m_CurRaidStateData.m_iNpcIndex].m_iFurySkill;
                m_CurRaidStateData.m_iConditionMax = CDataManager.dyRaidStateData[m_CurRaidStateData.m_iNpcIndex].m_iConditionMax;
                m_CurRaidStateData.m_iGroggyTime = CDataManager.dyRaidStateData[m_CurRaidStateData.m_iNpcIndex].m_iGroggyTime;

                m_fRaidBossGroggyTime = m_CurRaidStateData.m_iGroggyTime;
                //            m_fRaidBossGroggyTime = (float)m_CurRaidStateData.m_iGroggyTime;
                //            m_fRaidBossFuryGage = (float)m_CurRaidStateData.m_iConditionMax;
            }
        }
        //bRaidStarted = false;
    }

    public void InitDeleteMember()
    {
        if (m_DeletedMembers != null)
        {
            for (int i = 0; i < m_DeletedMembers.Length; i++)
            {
                m_DeletedMembers[i] = null;
            }
            m_DeletedMemberCnt = 0;
        }
    }
    public override void Process()
    {
        if (m_fRaidBossGroggyRemainTime > 0.0f)
        {
            m_fRaidBossGroggyRemainTime -= Time.deltaTime;

            if (m_fRaidBossGroggyRemainTime <= 0.0f)
            {
                NormalBoss();
                m_fRaidBossGroggyRemainTime = 0.0f;
            }
        }
    }
    public override void Destroy()
    { 
    }

    public void Start()
    {
        if (m_NpcList == null)
            return;
        // npc 배치
        for( int i = 0 ; i < m_NpcList.Count ; ++i )
        {
            for ( int m = 0 ; m < NpcManager.instance.m_monsterCharObjects.Count ; ++m )
            {
                if( NpcManager.instance.m_monsterCharObjects[m].m_CharAi &&
                    NpcManager.instance.m_monsterCharObjects[m].m_CharAi.nUniqueID == m_NpcList[i].uid)
                {
                    // 정보 설정
                    NpcManager.instance.m_monsterCharObjects[m].m_IngameObjectID = m_NpcList[i].object_id;
                }
            }
        }

        InGameManager.instance.ShowUI();
    }

    #endregion  OVERRIDE_METHODS

    public void BrokenBossFury()
    {
        if (NpcManager.instance.NpcBoss != null)
        {
            List<long> LinkBuff = new List<long>();
            int iAddBuffCheck = 0;
            int iSkillIndex = m_CurRaidStateData.m_iGroggySkill;
            NpcManager.instance.NpcBoss.BuffLinkSkill(NpcManager.instance.NpcBoss, NpcManager.instance.NpcBoss.GetAttackInfo(iSkillIndex).skillinfo, 0, false, ref LinkBuff, ref iAddBuffCheck);

            m_fRaidBossGroggyRemainTime = m_fRaidBossGroggyTime;
        }
    }
    public void FuryBossFury()
    {
        if (NpcManager.instance.NpcBoss != null)
        {
            //List<bool> LinkBuff = new List<bool>();
            int iSkillIndex = m_CurRaidStateData.m_iFurySkill;
            ((NpcAI)(NpcManager.instance.NpcBoss.m_CharAi)).SetFuryState(iSkillIndex);
            //NpcManager.instance.NpcBoss.BuffLinkSkill(NpcManager.instance.NpcBoss, NpcManager.instance.NpcBoss.GetAttackInfo(iSkillIndex).skillinfo, 0, false, ref LinkBuff, ref iAddBuffCheck);
        }
    }
    public void NormalBoss()
    {
        m_fRaidBossFuryGage = 0.0f;
    }

    public RaidMemberInfo GetMemberInfoByName(string name)
    {
        for(int i=0 ; i<m_PartyMemberList.Length ; i++)
        {
            if(m_PartyMemberList[i].rMemInfo.strName != null)
            {
                if (name.Equals(m_PartyMemberList[i].rMemInfo.strName))
                {
                    return m_PartyMemberList[i];
                }
            }
        }
        return null;
    }

    public int GetMemberIndexByName(string name)
    {
        for (int i = 0; i < m_PartyMemberList.Length; i++)
        {
            if (m_PartyMemberList[i].rMemInfo.strName != null)
            {
                if (name.Equals(m_PartyMemberList[i].rMemInfo.strName))
                {
                    return i;
                }
            }
        }
        return -1;
    }


    public int GetDeletedMemberCount()
    {
        for (int i = 0; i < m_DeletedMembers.Length; i++)
        {
            if(m_DeletedMembers[i]!=null)
            {
                m_DeletedMemberCnt++;
            }
        }

        return m_DeletedMemberCnt;
    }

    public List<MapData> GetMapDataPossibleRaidDungeon()
    {
        List<MapData> mapDatas = new List<MapData>();

        for (int i = 0; i < CDataManager.MapDatas.Count; i++ )
        {
            if(CDataManager.MapDatas[i].ModeType == eMAP_MODE_TYPE.eRAID)
            {
                mapDatas.Add(CDataManager.MapDatas[i]);
            }
        }

        return mapDatas;
    }

    public MapData GetMapDataCurRaidDungeon()
    {
        for (int i = 0; i < CDataManager.MapDatas.Count; i++)
        {
            if (CDataManager.MapDatas[i].StageIndex == m_DungeonID)
            {
                return CDataManager.MapDatas[i];
            }
        }
        return null;
    }

    public void SetRaidCutScene(bool bActive)
    {
        UI_SkipCinema skip = (UI_SkipCinema)SceneManager.instance.SetUIComponent(SceneManager.eCOMPONENT.SkipCinema);
        //UI_BossHP bossHP = (UI_BossHP)SceneManager.instance.SetUIComponent(SceneManager.eCOMPONENT.BossHP);
        UI_MultiPlayUserInfo mpInfo = (UI_MultiPlayUserInfo)SceneManager.instance.SetUIComponent(SceneManager.eCOMPONENT.MultiPlayUserInfo);

        //Turn on actor
        if (RaicActorObj != null)
        {
            RaicActorObj.SetActive(bActive ? false : true);
        }
        ////Show Players and Boss
        //NpcManager.instance.NpcBoss.m_MyObj.gameObject.SetActive(bActive);
        //for (int i = 0; i < PlayerManager.instance.m_PlayerInfo.Count; ++i)
        //{
        //    if (PlayerManager.instance.m_PlayerInfo[i].playerObj != null)
        //        PlayerManager.instance.m_PlayerInfo[i].playerObj.SetActive(bActive);
        //}

        if(!bActive)
        {
			InGameManager.instance.m_uiBossHP.gameObject.SetActive(false);
            mpInfo.gameObject.SetActive(false);
            skip.gameObject.SetActive(true);
        }
        else
        {
			//SoundManager.instance.PlayBGM(6);
			InGameManager.instance.m_uiBossHP.gameObject.SetActive(true);
            mpInfo.gameObject.SetActive(true);
            skip.gameObject.SetActive(false);
        }
    }

    public void InviateRaid( int p_iDungeonID ,string p_strDungeonKey )
    {
        RaidManager.instance.InitDeleteMember();

        RaidManager.instance.m_SelectedMode = RaidManager.SelectedMode.RAID;
        RaidManager.instance.m_strLastDungeonIndex = p_iDungeonID;
        RaidManager.instance.m_eQuickPlay = eRaidQuickPlay.INVITE;
        RaidManager.instance.m_strDungeonKey = p_strDungeonKey;


        SceneManager.instance.SetSceneState(SceneManager.eSCENE_STATE.WorldMap);
    }

#if NONSYNC_PVP
    public void SetNonSyncPvpPlayerPets(List<blame_messages.PetInfo> pets)
    {
        int petCount = pets.Count;
        //petCount = 1;
        for (int i=0; i< petCount; ++i)
        {
            SummonData tmpData = new SummonData();
            uint index = (uint)pets[i].index;
            int dataIndex = (int)index;// NpcManager.instance.m_SummonIndexDic[(int)index];
            tmpData.m_PlayerSummonIndex = index;

            tmpData.m_PlayerSummonIndexCode = CExcelData_SUMMON_DATA.instance.SUMMON_DATABASE_GetNPC_CODE(dataIndex);
            tmpData.m_PlayerSummonName = CExcelData_SUMMON_DATA.instance.SUMMON_DATABASE_GetNPC_NAME_ID(dataIndex);
            tmpData.m_PlayerSummonIconName = CExcelData_SUMMON_DATA.instance.SUMMON_DATABASE_GetSUMMON_ICON(dataIndex);
            tmpData.m_PlayerSummonScale = CExcelData_SUMMON_DATA.instance.SUMMON_DATABASE_GetSUMMON_SCALE(dataIndex);

            tmpData.Grade = (int)pets[i].grade;
            tmpData.ExpLevel = (int)pets[i].exp_level;
            tmpData.Exp = pets[i].exp;

		//< add ggango 2017.11.21
			tmpData.EnchantLevel = (int)pets[i].enchant_level;
		//>

            //tmpData.GoldLevel = (int)pets[i].level;
            //tmpData.MedalLevel = (int)pets[i].toughen_level;
            tmpData.Equip = pets[i].is_equip;
            tmpData.EquipIdx = (int)pets[i].equip_index;
            tmpData.Acquire = pets[i].is_acquire;
            tmpData.Die = pets[i].is_die;
            tmpData.nCount = (int)pets[i].count;
            tmpData.nContentIndex = (int)pets[i].cont_index;
            m_NonSyncPvpPlayerPets.Add(tmpData);
        }
    }

    public void SetNonSyncPvpPlayerPetSlotInfo(blame_messages.PetSlotInfo petSlotInfo)
    {
        if(petSlotInfo == null)
        {
            return;
        }
        if (m_NonSyncPvpPlayerPetSlot.Count > 0)
        {
            m_NonSyncPvpPlayerPetSlot.Clear();
        }

        PlayerInfo pInfo = PlayerManager.instance.m_PlayerInfo[PlayerManager.PVPPLAYER_INDEX];
        blame_messages.PetSlotType petSlotType;

        //First Slot
        m_NonSyncPvpPlayerPetSlot.Add(new SummonSlotData((int)petSlotInfo.first_slot_level, (int)petSlotInfo.first_slot_petindex));        
        //Second Slot
        m_NonSyncPvpPlayerPetSlot.Add(new SummonSlotData((int)petSlotInfo.second_slot_level, (int)petSlotInfo.second_slot_petindex));
        //Third Slot
        m_NonSyncPvpPlayerPetSlot.Add(new SummonSlotData((int)petSlotInfo.third_slot_level, (int)petSlotInfo.third_slot_petindex));
               
        //Set Pet Slot Group
        petSlotType = (blame_messages.PetSlotType)petSlotInfo.type;

        pInfo.m_SummonSlotGroup[(int)petSlotType][0].m_SlotLavel = (int)petSlotInfo.first_slot_level;
        pInfo.m_SummonSlotGroup[(int)petSlotType][0].m_SlotSummonIndex = (int)petSlotInfo.first_slot_petindex;
        pInfo.m_SummonSlotGroup[(int)petSlotType][1].m_SlotLavel = (int)petSlotInfo.second_slot_level;
        pInfo.m_SummonSlotGroup[(int)petSlotType][1].m_SlotSummonIndex = (int)petSlotInfo.second_slot_petindex;
        pInfo.m_SummonSlotGroup[(int)petSlotType][2].m_SlotLavel = (int)petSlotInfo.third_slot_level;
        pInfo.m_SummonSlotGroup[(int)petSlotType][2].m_SlotSummonIndex = (int)petSlotInfo.third_slot_petindex;
    }

    public void SetNonSyncPvpPlayerPetSyncHP()
    {
        for (int i = 0; i < m_NonSyncPvpPlayerPets.Count; ++i)
        {
            m_NonSyncPvpPlayerPets[i].summonCBase.damageCalculationData.SetSyncHP();
        }
    }

    public int GetPetSlotIndex(int summonIndex)
    {
        for(int i=0; i< m_NonSyncPvpPlayerPetSlot.Count; ++i)
        {
            if(summonIndex == m_NonSyncPvpPlayerPetSlot[i].m_SlotSummonIndex)
            {
                return i;
            }
        }
        return -1;
    }

    public int GetSummonIndex(int owner)
    {
        for (int i = 0; i < m_NonSyncPvpPlayerPets.Count; ++i)
        {
            if(m_NonSyncPvpPlayerPets[i].m_PlayerSummonIndex == owner)
            {
                return i;
            }
        }

        return -1;
    }
    public void SetPvpPlayerSkill(List<blame_messages.SkillInfo> skillInfos)
    {
        m_NonSyncPvpPlayerSkills = new Dictionary<int, blame_messages.SkillInfo>();
        m_NonSyncPvpPlayerSkillCodes = new List<int>();

        for (int i = 0; i < skillInfos.Count; i++)
        {
            if (skillInfos[i].id != 0)//skill id
            {
                m_NonSyncPvpPlayerSkills.Add((int)skillInfos[i].id, skillInfos[i]);
                m_NonSyncPvpPlayerSkillCodes.Add((int)skillInfos[i].id);
            }
        }
    }

    public void CleanUpPvp()
    {
        NpcManager.instance.Clean4v4PvpPets();
        m_NonSyncPvpPlayerPets.Clear();
                
        if (m_NonSyncPvpPlayerSkills != null)
        {
            m_NonSyncPvpPlayerSkills.Clear();
            m_NonSyncPvpPlayerSkills = null;
        }

        if (m_NonSyncPvpPlayerSkillCodes != null)
        {
            m_NonSyncPvpPlayerSkillCodes.Clear();
            m_NonSyncPvpPlayerSkillCodes = null;
        }

        if (m_AttackComboSet != null)
        {
            m_AttackComboSet.Clear();
            m_AttackComboSet = null;
        }

        CleanUpPvpPlayer();
    }

    private void CleanUpPvpPlayer()
    {
        if (m_NonPvpPlayer != null)
        {
            m_NonPvpPlayer.Destroy();
            m_NonPvpPlayer = null;
        }
    }
#endif

    #region PROPERTY
    public int dungeonID
    {
        get { return m_DungeonID; }
        set { m_DungeonID = value; }
    }

    //public bool bRaidStarted
    //{
    //    get { return m_bRaidHasBnStarted; }
    //    set { m_bRaidHasBnStarted = value; }
    //}
    #endregion PROPERTY
}
