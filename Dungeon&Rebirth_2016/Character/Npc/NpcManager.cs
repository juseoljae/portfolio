//#define PLANE_SHADOW
//#define SUMMONTEST
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KSPlugins;

#region ENUM
public enum eNPC_BATTLE_TYPE
{
    eBATTLE = 1,
    eNON_BATTLE,
    eOBJECT
}
public enum eNPC_TYPE
{
    eNONE = 0,
    eMONSTER,
    eNPC,
    eDESTROYABLE_OBJ,
    eZONEWALL_OBJ
}
public enum eNPC_CLASS
{
    eNONE = 0,
    eNORMAL,
    eELITE,
    eBOSS,
    eRAIDBOSS
}

public enum eBOSS_DIFFICULTY
{
    EASY = 0,
    NORMAL,
    HARD,
    VERY_HARD,
    IMPOSSIBLE
}

#endregion ENUM

public sealed class NpcManager : KSManager
{
	#region MEMBERS
	public class NpcSpawnGroupInfo
	{
		public bool bEnable = false;
		public int Group_Index;
		public List<CharacterBase> m_NpcObjects;
	};
	public class SummonSlotPrice
	{
		public int		SlotPrice_1;
		public int		SlotPrice_2;
		public int		SlotPrice_3;
		public float	SlotLevelPerExpRatio;
	};
	////}

	private const int SLOT_MASTER = (int)blame_messages.PetSlotType.SLOT_MASTER;	

	public Dictionary<int, NpcSpawnGroupInfo> m_NpcSpawnGroupInfos;
	public Dictionary<uint, List<NpcAI_PatternInfo.Pattern_TableData>> m_SummonAI_PatternDatas;
	public Dictionary<uint, List<NpcAI_PatternInfo.Pattern_TableData>> m_NpcAI_PatternDatas;
	public Dictionary<uint, List<NpcAI_TimeTrigerInfo.TimeTriger_TableData>> m_NpcAI_TimeTrigerDatas;


	public List<CharacterBase> m_monsterCharObjects;
	public List<CharacterBase> m_SummonObjs;
	public List<NpcInfo.NpcProp> m_PlayerEquipSummonProp;
	public List<NpcInfo.NpcProp> m_PlayerSummonProp;
	public Dictionary<int, NpcSpawnGroupInfo> m_PlayerSummonSpawnGroupInfos;
	public List<CharacterBase> m_PlayerSummonObjs;
	public List<NpcInfo.NpcModelList> m_PlayerSummonModelList;
	//플레이어 소환수 리스트
	public List<SummonData> m_PlayerSummonList;

	public Dictionary<uint, uint> m_SummonDataIndexs;

	public Dictionary<uint, uint> m_NpcDataIndexs;
	public Dictionary<int, int> m_NpcModelDic;

	public Dictionary<int, List<string>> m_NpcModelInfoByThemeDic;
	public Dictionary<uint, int> m_NpcPatternListIndexs;

	public Dictionary<int, SummonSlotPrice> m_SummonSlavePriceData;


	public int m_TeamSlotGroupIndex;
	public int m_TeamSlotIndex;
	public int m_TeamSlotSummonIndex;
	public bool m_TeamSummonSlotChange;
    public bool m_TeamSwap;
	public Dictionary<uint, List<int>> m_SummonPatternDatas;
	public Dictionary<uint, List<int>> m_NpcPatternDatas;
	public Dictionary<uint, List<int>> m_NpcTimeTriggerDatas;

	private List<CharacterBase> m_NpcSpwanedList;

	private int m_ID;

	//about hierachy tree structure
	GameObject m_rootObj;
	GameObject m_npcRootObj;

	public CharacterBase NpcBoss { get; private set; }

	public const int SUMMONSLOTGROUP_MAX = 2;
	public const int SUMMONSLOT_MAX = 3;
	public const int SLAVESLOTLEVER_MAX = 5;

	private const int SUMMON_START_INDEX = 10000;
	private const float SUMMON_SCALE = 0.75f;
	private const float SUMMON_RUNSPEED = 1.75f;
	private const float SUMMON_ATTSPEED = 1.5f;
#if NONSYNC_PVP
	public Dictionary<int, uint> m_SummonIndexDic = new Dictionary<int, uint>();
	private List<int> m_SpawnPositionIndex = new List<int>();
#endif

	/// 드랍될 아이템들
	public List<DropItemInfo> m_dropItems = null;

	public bool m_bLoadNpcPrefab;

	//ProjectB
	public GameObject m_DropGoldObj;
	public GameObject m_DropCoinObj;
	public GameObject m_DropCashObj;
	const int m_SummonCount = 40;

	const int addLevel = 0;
	/////////////////////////////////////////////////////////////////////////////////////////////

	private bool m_coroutine_is_running;
	#endregion  MEMBERS



	#region GETTER

	public List<CharacterBase> GetMonsterCharObjects()
	{
		return m_monsterCharObjects;
	}

	//현재 Spawn된 npc List
	public List<CharacterBase> GetNpcTargetsBySpwaned()
	{
		return m_NpcSpwanedList;
	}
	public List<CharacterBase> GetHeroNpcTargetsBySpwaned()
	{
		return m_PlayerSummonObjs;
	}

	public CharacterBase GetNpcByIndex(int npcCode)
	{
		for (int i = 0; i < m_monsterCharObjects.Count; ++i)
		{
			if (m_monsterCharObjects[i].m_MyObj != null)
			{
				NpcAI npcAi = m_monsterCharObjects[i].m_MyObj.GetComponent<NpcAI>();
				if (npcAi != null)
				{
					if (npcAi.GetNpcProp().Npc_Code == npcCode)
					{
						return m_monsterCharObjects[i];
					}
				}
			}
		}

		return null;
	}

	public uint GetNpcBySpawnerID(int id)
	{
		for (int i = 0; i < SpawnDataManager.instance.NpcSpawnerDataList.Count; ++i)
		{
			if (SpawnDataManager.instance.NpcSpawnerDataList[i].m_nSpawnerID == id)
			{
				return (uint)SpawnDataManager.instance.NpcSpawnerDataList[i].m_nNpcCode;
			}
		}

		return 0;
	}
    
        
    public NpcInfo.NpcProp GetSummonNpcPropByIndex(int summonIndex)
    {
        NpcInfo.NpcProp npcProp = null;

        for (int i = 0; i < m_PlayerSummonProp.Count; ++i)
        {
            if (summonIndex == m_PlayerSummonProp[i].Summon_nContentIndex)
            {
                npcProp = m_PlayerSummonProp[i];
                break;
            }
        }

        return npcProp;
    }

    public CharacterBase GetSummonCharObject(Vector3 p_pPosition)
	{
		for (int i = 0; i < m_SummonObjs.Count; i++)
		{
			if (!m_SummonObjs[i].m_MyObj.gameObject.activeSelf)
			{
				m_SummonObjs[i].m_CharState = CHAR_STATE.ALIVE;
				NpcManager.instance.ReLoadAI(m_SummonObjs[i]);

				if (m_SummonObjs[i].m_Collider != null)
					m_SummonObjs[i].m_Collider.enabled = true;

				m_SummonObjs[i].m_MyObj.position = p_pPosition;
				m_SummonObjs[i].m_MyObj.gameObject.SetActive(true);

				m_SummonObjs[i].SetCollisionDetection(true);
				m_SummonObjs[i].damageCalculationData.fCURRENT_HIT_POINT = m_SummonObjs[i].damageCalculationData.fMAX_HIT_POINT;

				return m_SummonObjs[i];
			}
		}
		return null;
	}

	public List<CharacterBase> GetMonsterCharObjectsFindGroup(int p_nGroupIndex)
	{
		return m_monsterCharObjects;
	}


	#endregion GETTER

	#region     ENUMMEMBERS
	public enum eSTATUS : byte
	{
		ePREPARE = 0,
		eREADY,
		ePLAY,
		eEND,
		eNONE
	}

	private eSTATUS m_eStatus;
	#endregion ENUMMEMBERS

	#region     CONST_MEMBER
	private const byte CONST_AGGRESSIVETYPE_1 = 1;
	private const byte CONST_AGGRESSIVETYPE_2 = 2;
	private const byte CONST_AGGRESSIVETYPE_3 = 3;
	private const byte CONST_AGGRESSIVETYPE_4 = 4;
	private const byte CONST_AGGRESSIVETYPE_5 = 5;
	private const byte CONST_AGGRESSIVETYPE_6 = 6;
	private const byte CONST_AGGRESSIVETYPE_7 = 7;

	private const byte CONST_SUMMON_MAX = 10;
	#endregion  CONST_MEMBER

	#region     INSTANCE
	private static NpcManager m_Instance;
	public static NpcManager instance
	{
		get
		{
			if (m_Instance == null)
			{
				if (MainManager.instance != null)
					m_Instance = CreateManager(typeof(NpcManager).Name).AddComponent<NpcManager>();
				else
					m_Instance = new NpcManager();
			}

			return m_Instance;
		}
	}
	#endregion  INSTANCE


	#region     OVERRIDE METHODS
	public override void Initialize()
	{
		m_npcProp = new List<NpcInfo.NpcProp>();
		m_NpcPatternListIndexs = new Dictionary<uint, int>();
		//m_myAIData = new List<NpcInfo.NpcAIData>();
		m_monsterCharObjects = new List<CharacterBase>();
		m_SummonObjs = new List<CharacterBase>();
		//m_NpcGroupNumbers = new List<NpcInfo.NpcGroupNumber>();
		m_NpcSpawnGroupInfos = new Dictionary<int, NpcSpawnGroupInfo>();
		m_NpcSpwanedList = new List<CharacterBase>();

		//히어로 소환수 리스트 초기화
		m_PlayerSummonObjs = new List<CharacterBase>();

		m_PlayerSummonProp = new List<NpcInfo.NpcProp>();

		m_PlayerSummonModelList = new List<NpcInfo.NpcModelList>();
		m_SummonAI_PatternDatas = new Dictionary<uint, List<NpcAI_PatternInfo.Pattern_TableData>>();
		m_NpcAI_PatternDatas = new Dictionary<uint, List<NpcAI_PatternInfo.Pattern_TableData>>();
		m_NpcAI_TimeTrigerDatas = new Dictionary<uint, List<NpcAI_TimeTrigerInfo.TimeTriger_TableData>>();


		//Hierarchy structure
		m_rootObj = GameObject.Find("GamePlay");
		CreateNpcRootObject();
		//LoadExcelDataTable();

		m_eStatus = NpcManager.eSTATUS.eNONE;
		//m_eStatus = eSTATUS.ePREPARE;
		//SetNpcManagerStatus(eSTATUS.eNONE);

	}

	public eSTATUS npcMgrStatus
	{
		get { return m_eStatus; }
		set { m_eStatus = value; }

	}

    public void SetInGame2()
    {
        npcMgrStatus = NpcManager.eSTATUS.eREADY;
    }
    public void SetInGame()
	{
#if NONSYNC_PVP
		m_SpawnPositionIndex.Clear();
#endif
		//LoadExcelDataTable();
		npcMgrStatus = NpcManager.eSTATUS.ePREPARE;
	}

	public void LoadExcelDataTable()
	{
		m_NpcDataIndexs = new Dictionary<uint, uint>();
		m_NpcModelDic = new Dictionary<int, int>();
		m_NpcModelInfoByThemeDic = new Dictionary<int, List<string>>();
		m_NpcPatternDatas = new Dictionary<uint, List<int>>();
		m_SummonPatternDatas = new Dictionary<uint, List<int>>(); ;
		m_NpcTimeTriggerDatas = new Dictionary<uint, List<int>>();

		m_SummonDataIndexs = new Dictionary<uint, uint>();
		m_PlayerSummonList = new List<SummonData>();
        CExcelData_NPC_MODEL_LIST.instance.Create();
		CExcelData_NPC_MODEL_LIST.instance.MEMORYTEXT_Create();

		//to Load Data : Map_arrangement
		CExcelData_Map.instance.Create();
		CExcelData_Map.instance.MEMORYTEXT_Create();

		//to Load Data : Npc_Pattern_Data_Test
		CExcelData_PATTERN_DATA.instance.Create();
		CExcelData_PATTERN_DATA.instance.MEMORYTEXT_Create();

		//to Load Data :SUmmon_Pattern_Data_Test
		CExcelData_PATTERN_SUMMON_DATA.instance.Create();
		CExcelData_PATTERN_SUMMON_DATA.instance.MEMORYTEXT_Create();

		//to Load Data : Npc_TimeTriger_Data_Test
		CExcelData_TIME_TRIGER_DATA.instance.Create();
		CExcelData_TIME_TRIGER_DATA.instance.MEMORYTEXT_Create();

		//to Load Data : Npc_Data
		CExcelData_NPC_DATA.instance.Create();
		CExcelData_NPC_DATA.instance.MEMORYTEXT_Create();

		CExcelData_DIFFICULTY_DATA.instance.Create();

		CExcelData_NPC_FACTOR_DATA.instance.Create();
		CExcelData_NPC_FACTOR_DATA.instance.MEMORYTEXT_Create();


		LoadDataSummonPrice();
		for (int i = 0; i < CExcelData_NPC_DATA.instance.NPC_DATABASE_nRecordCount; ++i)
		{
			//Debug.Log(i + "th npc idx = " + CExcelData_NPC_DATA.instance.NPC_DATABASE_GetNPC_CODE(i).ToString());
			m_NpcDataIndexs.Add(CExcelData_NPC_DATA.instance.NPC_DATABASE_GetNPC_CODE(i), (uint)i);
		}

		for (int i = 0; i < CExcelData_NPC_MODEL_LIST.instance.NPC_MODEL_LISTBASE_nRecordCount; ++i)
		{
			m_NpcModelDic.Add((int)CExcelData_NPC_MODEL_LIST.instance.NPC_MODEL_LISTBASE_GetINDEX(i), i);
		}

		for (int i = 0; i < CExcelData_NPC_MODEL_LIST.instance.NPC_MODEL_LISTBASE_nRecordCount; ++i)
		{
			SetNpcPrefabInfo(CExcelData_NPC_MODEL_LIST.instance.NPC_MODEL_LISTBASE_GetTHEME(i),
			CExcelData_NPC_MODEL_LIST.instance.NPC_MODEL_LISTBASE_GetMODEL_DATA_ROUTE(i),
			CExcelData_NPC_MODEL_LIST.instance.NPC_MODEL_LISTBASE_GetPREFABS_NAME(i)
			);
		}
		for (int i = 0; i < CExcelData_PATTERN_SUMMON_DATA.instance.PATTERN_DATABASE_nRecordCount; ++i)
		{
			if (m_SummonPatternDatas.ContainsKey(CExcelData_PATTERN_SUMMON_DATA.instance.PATTERN_DATABASE_GetINDEX(i)))
			{
				m_SummonPatternDatas[CExcelData_PATTERN_SUMMON_DATA.instance.PATTERN_DATABASE_GetINDEX(i)].Add(i);
			}
			else
			{
				List<int> kTmp = new List<int>();
				kTmp.Add(i);
				m_SummonPatternDatas.Add(CExcelData_PATTERN_SUMMON_DATA.instance.PATTERN_DATABASE_GetINDEX(i), kTmp);
			}
		}

		for (int i = 0; i < CExcelData_PATTERN_DATA.instance.PATTERN_DATABASE_nRecordCount; ++i)
		{
			if (m_NpcPatternDatas.ContainsKey(CExcelData_PATTERN_DATA.instance.PATTERN_DATABASE_GetINDEX(i)))
			{
				m_NpcPatternDatas[CExcelData_PATTERN_DATA.instance.PATTERN_DATABASE_GetINDEX(i)].Add(i);
			}
			else
			{
				List<int> kTmp = new List<int>();
				kTmp.Add(i);
				m_NpcPatternDatas.Add(CExcelData_PATTERN_DATA.instance.PATTERN_DATABASE_GetINDEX(i), kTmp);
			}
		}

		for (int i = 0; i < CExcelData_TIME_TRIGER_DATA.instance.TIME_TRIGER_DATABASE_nRecordCount; ++i)
		{
			if (m_NpcTimeTriggerDatas.ContainsKey(CExcelData_TIME_TRIGER_DATA.instance.TIME_TRIGER_DATABASE_GetINDEX(i)))
			{
				m_NpcTimeTriggerDatas[CExcelData_TIME_TRIGER_DATA.instance.TIME_TRIGER_DATABASE_GetINDEX(i)].Add(i);
			}
			else
			{
				List<int> kTmp = new List<int>();
				kTmp.Add(i);
				m_NpcTimeTriggerDatas.Add(CExcelData_TIME_TRIGER_DATA.instance.TIME_TRIGER_DATABASE_GetINDEX(i), kTmp);
			}
		}
		LoadPlayerNpcList();
	}

	//소환수 슬레이브 슬롯 레벨별 가격 데이타 로드
	private void LoadDataSummonPrice()
	{
		CExcelData_PET_OPEN_LEVELPERPRICE.instance.Create();
		CExcelData_PET_OPEN_LEVELPERPRICE.instance.MEMORYTEXT_Create();

		m_SummonSlavePriceData = new Dictionary<int, SummonSlotPrice>();

		int nRecordCount = CExcelData_PET_OPEN_LEVELPERPRICE.instance.PET_OPEN_LEVELPERPRICEBASE_nRecordCount;
		for (int i = 0; i < nRecordCount; i++)
		{
			SummonSlotPrice a_kSlaveSlotPrice = new SummonSlotPrice();
			a_kSlaveSlotPrice.SlotPrice_1 = CExcelData_PET_OPEN_LEVELPERPRICE.instance.PET_OPEN_LEVELPERPRICEBASE_GetLEVELPERPRICE_1_SLOT(i);
			a_kSlaveSlotPrice.SlotPrice_2 = CExcelData_PET_OPEN_LEVELPERPRICE.instance.PET_OPEN_LEVELPERPRICEBASE_GetLEVELPERPRICE_2_SLOT(i);
			a_kSlaveSlotPrice.SlotPrice_3 = CExcelData_PET_OPEN_LEVELPERPRICE.instance.PET_OPEN_LEVELPERPRICEBASE_GetLEVELPERPRICE_3_SLOT(i);
			a_kSlaveSlotPrice.SlotLevelPerExpRatio = CExcelData_PET_OPEN_LEVELPERPRICE.instance.PET_OPEN_LEVELPERPRICEBASE_GetLEVEL_PER_EXPRATIO(i);
			m_SummonSlavePriceData.Add(i, a_kSlaveSlotPrice);
		}
	}
	
	private void LoadPlayerNpcList()
	{
		CExcelData_SUMMON_DATA.instance.Create();
		CExcelData_SUMMON_DATA.instance.MEMORYTEXT_Create();

		m_PlayerSummonList.Clear();


		int nRecordCount = CExcelData_SUMMON_DATA.instance.SUMMON_DATABASE_nRecordCount;
		for (int i = 0; i < CExcelData_SUMMON_DATA.instance.SUMMON_DATABASE_nRecordCount; i++)
		{
			m_SummonDataIndexs.Add(CExcelData_SUMMON_DATA.instance.SUMMON_DATABASE_GetSUMMON_INDEX(i), (uint)i);
			if (i < m_SummonCount)
			{
				SummonData tmpData = new SummonData();
				tmpData.m_PlayerSummonIndex = CExcelData_SUMMON_DATA.instance.SUMMON_DATABASE_GetSUMMON_INDEX(i);
				tmpData.m_PlayerSummonIndexCode = CExcelData_SUMMON_DATA.instance.SUMMON_DATABASE_GetNPC_CODE(i);
				tmpData.m_PlayerSummonName = CExcelData_SUMMON_DATA.instance.SUMMON_DATABASE_GetNPC_NAME_ID(i);
				tmpData.m_PlayerSummonIconName = CExcelData_SUMMON_DATA.instance.SUMMON_DATABASE_GetSUMMON_ICON(i);
				tmpData.m_PlayerSummonScale = CExcelData_SUMMON_DATA.instance.SUMMON_DATABASE_GetSUMMON_SCALE(i);
				tmpData.m_SummonClass = CExcelData_SUMMON_DATA.instance.SUMMON_DATABASE_GetSUMMON_CLASS(i);

				tmpData.ExpLevel = 1;
                tmpData.Exp = 0;
				tmpData.EnchantLevel = 0;
				tmpData.Grade = 1;
				tmpData.Equip = false;
				tmpData.EquipIdx = -1;
				tmpData.Acquire = false;
				tmpData.Die = false;
				tmpData.nCount = 0;
				tmpData.nContentIndex = i;


#if NONSYNC_PVP
				m_SummonIndexDic.Add(i, tmpData.m_PlayerSummonIndex);
#endif

				m_PlayerSummonList.Add(tmpData);
			}
		}
	}

	public void InitSummonDatas()
	{
		for (int i = 0; i < m_PlayerSummonList.Count; ++i)
		{
			m_PlayerSummonList[i].ExpLevel = 1;
            m_PlayerSummonList[i].Exp = 0;
			m_PlayerSummonList[i].EnchantLevel = 0;
			m_PlayerSummonList[i].Grade = 1;
			m_PlayerSummonList[i].Equip = false;
			m_PlayerSummonList[i].EquipIdx = -1;
			m_PlayerSummonList[i].Acquire = false;
			m_PlayerSummonList[i].isAcquire = false;
			m_PlayerSummonList[i].Die = false;
			m_PlayerSummonList[i].nCount = 1;
			m_PlayerSummonList[i].nContentIndex = i;
		}
	}


	private NpcInfo.NpcProp GetNpcPropData(int npcIdx)
    {
        NpcInfo.NpcProp tmpProp = new NpcInfo.NpcProp();
        int rowIdx = (int)m_NpcDataIndexs[(uint)npcIdx];

        int CurStageIndex = 0;

        if (
         SceneManager.instance.GetCurGameMode() == SceneManager.eGAMEMODE.eMODE_CHAPTER
#if DAILYDG
   || SceneManager.instance.GetCurGameMode() == SceneManager.eGAMEMODE.eMODE_DAILY
#endif
   )
        {

            MapData CurMapData = MapManager.instance.LastPlayMapData;
            int index = CurMapData.ThemeIndex - 1;
            CurStageIndex = CurMapData.StageIndex;
        }

        tmpProp.Npc_Code = CExcelData_NPC_DATA.instance.NPC_DATABASE_GetNPC_CODE(rowIdx);
        tmpProp.Npc_NameID = CExcelData_NPC_DATA.instance.NPC_DATABASE_GetNPC_NAME_ID(rowIdx);
        tmpProp.Npc_Scale = CExcelData_NPC_DATA.instance.NPC_DATABASE_GetNPC_SCALE(rowIdx);
        tmpProp.Npc_ArcheType = CExcelData_NPC_DATA.instance.NPC_DATABASE_GetNPC_ARCHETYPE(rowIdx);
        tmpProp.Battle_Type = (eNPC_BATTLE_TYPE)CExcelData_NPC_DATA.instance.NPC_DATABASE_GetBATTLE_TYPE(rowIdx);
        tmpProp.Npc_Size = CExcelData_NPC_DATA.instance.NPC_DATABASE_GetNPC_SIZE(rowIdx);
        tmpProp.Npc_Type = (eNPC_TYPE)CExcelData_NPC_DATA.instance.NPC_DATABASE_GetNPC_TYPE(rowIdx);
        tmpProp.Npc_Class = (eNPC_CLASS)CExcelData_NPC_DATA.instance.NPC_DATABASE_GetNPC_CLASS(rowIdx);
        tmpProp.Att_Cancel = CExcelData_NPC_DATA.instance.NPC_DATABASE_GetATT_CANCEL(rowIdx);
        tmpProp.Hit_Ani = CExcelData_NPC_DATA.instance.NPC_DATABASE_GetHIT_ANI(rowIdx);
        tmpProp.Target_Ruling = CExcelData_NPC_DATA.instance.NPC_DATABASE_GetTARGET_RULING(rowIdx);

        int curFloor = PlayerManager.instance.m_PlayerInfo[PlayerManager.MYPLAYER_INDEX].m_CurFloor; // 현재 층
        if (SceneManager.instance.GetCurGameMode() == SceneManager.eGAMEMODE.eMODE_CHAPTER)
        {
            tmpProp.Npc_Level = (float)GetNpcLevel();

#if UNITY_EDITOR
            if (tmpProp.Npc_Class == eNPC_CLASS.eBOSS)
            {
                Debug.Log("NPC Boss Level  =========================" + (double)(tmpProp.Npc_Level));
            }
#endif
        }
        else if (SceneManager.instance.GetCurGameMode() == SceneManager.eGAMEMODE.eMODE_DAILY)
        {
            tmpProp.Npc_Level = CExcelData_NPC_DATA.instance.NPC_DATABASE_GetNPC_LEVEL(rowIdx);

            curFloor = (int)(tmpProp.Npc_Level);
        }

		/// 환생 레벨 특성 적용
		AttributeAffect a_kAttributeAffect	= PlayerManager.instance.m_PlayerInfo[ PlayerManager.MYPLAYER_INDEX ].GetAttributeAffect( EAttributeAffect.MONSTOR_LV_REDUCE );
		curFloor							= (int)(curFloor * ( 1 - a_kAttributeAffect.Value * 0.01f ));

        tmpProp.Npc_Exp = CExcelData_NPC_DATA.instance.NPC_DATABASE_GetNCP_EXP(rowIdx);
        tmpProp.Drop_Id = CExcelData_NPC_DATA.instance.NPC_DATABASE_GetDROP_ID(rowIdx);
        tmpProp.Drop_Count = CExcelData_NPC_DATA.instance.NPC_DATABASE_GetDROP_COUNT(rowIdx);
        tmpProp.Identity_Fnd = (eTargetState)CExcelData_NPC_DATA.instance.NPC_DATABASE_GetIDENTITY_FRIENDLY(rowIdx);
        tmpProp.Npc_Search = CExcelData_NPC_DATA.instance.NPC_DATABASE_GetNPC_SEARCH(rowIdx);

        //InfiniteDungeon Temp
        if (SceneManager.instance.GetCurGameMode() == SceneManager.eGAMEMODE.eGOLD_DUNGEON ||
         SceneManager.instance.GetCurGameMode() == SceneManager.eGAMEMODE.eMODE_TRIAL_DUNGEON)
        {
            tmpProp.Search_Range = 1000;
        }
        else
        {
            tmpProp.Search_Range = CExcelData_NPC_DATA.instance.NPC_DATABASE_GetSEARCH_RANGE(rowIdx);
        }
        tmpProp.Free_Activity = CExcelData_NPC_DATA.instance.NPC_DATABASE_GetFREE_ACTIVITY(rowIdx);
        tmpProp.Patrol_Link_Id = CExcelData_NPC_DATA.instance.NPC_DATABASE_GetPATROL_LINK_ID(rowIdx);
        tmpProp.First_Agro = CExcelData_NPC_DATA.instance.NPC_DATABASE_GetFIRST_AGRO(rowIdx);
        tmpProp.Family_Group = CExcelData_NPC_DATA.instance.NPC_DATABASE_GetFAMILY_GROUP(rowIdx);
        string strTmp = UtilManager.instance.SplitString("`", CExcelData_NPC_DATA.instance.NPC_DATABASE_GetENEMY_GROUP(rowIdx));
        if (string.IsNullOrEmpty(strTmp) != true)
        {
            tmpProp.Enemy_Group = UtilManager.instance.SplitIntArr(",", strTmp);
        }
        tmpProp.Ai_Type = CExcelData_NPC_DATA.instance.NPC_DATABASE_GetAI_TYPE(rowIdx);
        tmpProp.Ai_Pattern_id = CExcelData_NPC_DATA.instance.NPC_DATABASE_GetAI_PATTERN_ID(rowIdx);
        tmpProp.Time_Trigger_id = CExcelData_NPC_DATA.instance.NPC_DATABASE_GetTIME_TRIGGER_ID(rowIdx);
        tmpProp.Dead_Zone = CExcelData_NPC_DATA.instance.NPC_DATABASE_GetDEAD_ZONE(rowIdx);
        tmpProp.Alert_Distance_Min = CExcelData_NPC_DATA.instance.NPC_DATABASE_GetALERT_DISTANCE_MIN(rowIdx);
        tmpProp.Alert_Distance_Max = CExcelData_NPC_DATA.instance.NPC_DATABASE_GetALERT_DISTANCE_MAX(rowIdx);

        strTmp = UtilManager.instance.SplitString("`", CExcelData_NPC_DATA.instance.NPC_DATABASE_GetATT_COOLTIME(rowIdx));
        if (string.IsNullOrEmpty(strTmp) != true)
        {
            tmpProp.Att_CoolTime = UtilManager.instance.SplitFloatArr(",", strTmp);
        }

        tmpProp.Att_Speed = CExcelData_NPC_DATA.instance.NPC_DATABASE_GetATTACK_SPEED(rowIdx) / 100;
        //tmpProp.Casting_Speed = CExcelData_NPC_DATA.instance.NPC_DATABASE_GetCASTING_SPEED(rowIdx);
        tmpProp.Walk_Speed = CExcelData_NPC_DATA.instance.NPC_DATABASE_GetWALK_SPEED(rowIdx) / 100;
        tmpProp.Run_Speed = CExcelData_NPC_DATA.instance.NPC_DATABASE_GetRUN_SPEED(rowIdx) / 100;

        tmpProp.Default_Run_Speed = tmpProp.Run_Speed;
        tmpProp.Default_Att_Speed = tmpProp.Att_Speed;

        List<List<string>> splitListStr;
        string Normal_Attack_Id = CExcelData_NPC_DATA.instance.NPC_DATABASE_GetNORMAL_ATT_ID(rowIdx);
        tmpProp.link_SkillCode = new List<int>();
        tmpProp.link_SkillRate = new List<int>();
        splitListStr = UtilManager.instance.GetDataValueArr(Normal_Attack_Id);

        for (int n = 0; n < splitListStr.Count; ++n)
        {
            tmpProp.link_SkillCode.Add(int.Parse(splitListStr[n][0]));
            tmpProp.link_SkillRate.Add(int.Parse(splitListStr[n][1]));
        }

        strTmp = UtilManager.instance.SplitString("`", CExcelData_NPC_DATA.instance.NPC_DATABASE_GetNPC_SKILL_ID(rowIdx));
        if (string.IsNullOrEmpty(strTmp) != true)
        {
            tmpProp.Npc_skill_Id = UtilManager.instance.SplitIntArr(",", strTmp);
        }

        tmpProp.Npc_Attack_Min = CExcelData_NPC_DATA.instance.NPC_DATABASE_GetNPC_ATT_MIN(rowIdx);
        tmpProp.Npc_Attack_Max = CExcelData_NPC_DATA.instance.NPC_DATABASE_GetNPC_ATT_MAX(rowIdx);
        tmpProp.Npc_Def = CExcelData_NPC_DATA.instance.NPC_DATABASE_GetNPC_DEFENSE(rowIdx);
        tmpProp.Max_Hp = CExcelData_NPC_DATA.instance.NPC_DATABASE_GetNPC_MAX_HP(rowIdx);

        tmpProp.Critical_Rate = CExcelData_NPC_DATA.instance.NPC_DATABASE_GetNPC_CRITICAL_RATE(rowIdx);
        tmpProp.Critical_Power = CExcelData_NPC_DATA.instance.NPC_DATABASE_GetNPC_CRITICAL_POWER(rowIdx);
        tmpProp.Accuracy_Rate = CExcelData_NPC_DATA.instance.NPC_DATABASE_GetNPC_ACCURACY(rowIdx);
        tmpProp.Dodge_Rate = CExcelData_NPC_DATA.instance.NPC_DATABASE_GetNPC_DODGE(rowIdx);
        tmpProp.DeBuffResist = CExcelData_NPC_DATA.instance.NPC_DATABASE_GetNPC_DEBUFF_RESIST(rowIdx);
        tmpProp.Die_Effect_Idx = CExcelData_NPC_DATA.instance.NPC_DATABASE_GetDIE_EFFECT(rowIdx);
        tmpProp.SpawnEff = CExcelData_NPC_DATA.instance.NPC_DATABASE_GetSPAWN_EFFECT(rowIdx);

        tmpProp.Att_Revision = CExcelData_NPC_DATA.instance.NPC_DATABASE_GetATT_REVISION(rowIdx);
        tmpProp.HP_Revision = CExcelData_NPC_DATA.instance.NPC_DATABASE_GetHP_REVISION(rowIdx);

        switch (tmpProp.Npc_Class)
        {
            case eNPC_CLASS.eNORMAL:
            case eNPC_CLASS.eELITE:
                tmpProp.AttackBase = CExcelData_NPC_FACTOR_DATA.instance.NPC_FACTOR_DATABASE_GetATTACK_NORM(0);
                tmpProp.DefenseBase = CExcelData_NPC_FACTOR_DATA.instance.NPC_FACTOR_DATABASE_GetDEFENSE_NORM(0);
                tmpProp.HPBase = CExcelData_NPC_FACTOR_DATA.instance.NPC_FACTOR_DATABASE_GetHP_NORM(0);
                tmpProp.ExpBase = CExcelData_NPC_FACTOR_DATA.instance.NPC_FACTOR_DATABASE_GetEXP_NORM(0);
                tmpProp.ExpCorrec = CExcelData_NPC_FACTOR_DATA.instance.NPC_FACTOR_DATABASE_GetEXP_CORREC_NORM(0);
                break;
            case eNPC_CLASS.eBOSS:
                tmpProp.AttackBase = CExcelData_NPC_FACTOR_DATA.instance.NPC_FACTOR_DATABASE_GetATTACK_BOSS(0);
                tmpProp.DefenseBase = CExcelData_NPC_FACTOR_DATA.instance.NPC_FACTOR_DATABASE_GetDEFENSE_BOSS(0);
                tmpProp.HPBase = CExcelData_NPC_FACTOR_DATA.instance.NPC_FACTOR_DATABASE_GetHP_BOSS(0);
                tmpProp.ExpBase = CExcelData_NPC_FACTOR_DATA.instance.NPC_FACTOR_DATABASE_GetEXP_BOSS(0);
                tmpProp.ExpCorrec = CExcelData_NPC_FACTOR_DATA.instance.NPC_FACTOR_DATABASE_GetEXP_CORREC_BOSS(0);
                break;
            case eNPC_CLASS.eRAIDBOSS:
                tmpProp.AttackBase = CExcelData_NPC_FACTOR_DATA.instance.NPC_FACTOR_DATABASE_GetATTACK_RAID(0);
                tmpProp.DefenseBase = CExcelData_NPC_FACTOR_DATA.instance.NPC_FACTOR_DATABASE_GetDEFENSE_RAID(0);
                tmpProp.HPBase = CExcelData_NPC_FACTOR_DATA.instance.NPC_FACTOR_DATABASE_GetHP_RAID(0);
                tmpProp.ExpBase = CExcelData_NPC_FACTOR_DATA.instance.NPC_FACTOR_DATABASE_GetEXP_RAID(0);
                tmpProp.ExpCorrec = CExcelData_NPC_FACTOR_DATA.instance.NPC_FACTOR_DATABASE_GetEXP_CORREC_RAID(0);
                break;
        }

        float l_fStageCircle = CExcelData_NPC_FACTOR_DATA.instance.NPC_FACTOR_DATABASE_GetSTAGE_CIRCLE(0); // 난이도 스테이지 순환
        float l_fStageAddCircle = CExcelData_NPC_FACTOR_DATA.instance.NPC_FACTOR_DATABASE_GetSTAGE_ADD_CIRCLE(0); //  난이도 스테이지 추가 순환
        float l_fStageCoefficent = CExcelData_NPC_FACTOR_DATA.instance.NPC_FACTOR_DATABASE_GetSTAGE_COEFFICENT(0); // 난이도 증가 계수
        float l_fStageAddCoefficent = CExcelData_NPC_FACTOR_DATA.instance.NPC_FACTOR_DATABASE_GetSTAGE_ADD_COEFFICENT(0); // 난이도 추가 증가 계수

        float atkStart = CExcelData_NPC_FACTOR_DATA.instance.NPC_FACTOR_DATABASE_GetATTACK_START(0);
        float atkQuot = CExcelData_NPC_FACTOR_DATA.instance.NPC_FACTOR_DATABASE_GetATTACK_QUOT(0);
        tmpProp.Npc_AttackValue = (atkStart + (tmpProp.AttackBase * UtilManager.instance.CalcPow(curFloor, atkQuot)) * tmpProp.Att_Revision);

        float defStart = CExcelData_NPC_FACTOR_DATA.instance.NPC_FACTOR_DATABASE_GetDEFENSE_START(0);
        float defQuot = CExcelData_NPC_FACTOR_DATA.instance.NPC_FACTOR_DATABASE_GetDEFENSE_QUOT(0);
        tmpProp.Npc_DefenseValue = (defStart + tmpProp.DefenseBase * UtilManager.instance.CalcPow(curFloor, defQuot));

        float hpStart = CExcelData_NPC_FACTOR_DATA.instance.NPC_FACTOR_DATABASE_GetHP_START(0);
        float hpQuot = CExcelData_NPC_FACTOR_DATA.instance.NPC_FACTOR_DATABASE_GetHP_QUOT(0);
        if (tmpProp.Npc_Type == eNPC_TYPE.eDESTROYABLE_OBJ)
        {
            tmpProp.Npc_HPValue = 1;
        }
        else
        {
            tmpProp.Npc_HPValue =
                hpStart + (tmpProp.HPBase * UtilManager.instance.CalcPow(curFloor, hpQuot)) * tmpProp.HP_Revision;
        }

        if (curFloor > (l_fStageCircle))
        {
            tmpProp.Npc_AttackValue *= (int)(curFloor / l_fStageCircle) * (l_fStageCoefficent + l_fStageAddCoefficent * (int)(curFloor / l_fStageAddCircle));
            tmpProp.Npc_DefenseValue *= (int)(curFloor / l_fStageCircle) * (l_fStageCoefficent + l_fStageAddCoefficent * (int)(curFloor / l_fStageAddCircle));

            if (tmpProp.Npc_Type == eNPC_TYPE.eDESTROYABLE_OBJ)
            {
                tmpProp.Npc_HPValue = 1;
            }
            else
            {
                tmpProp.Npc_HPValue *= (int)(curFloor / l_fStageCircle) * (l_fStageCoefficent + l_fStageAddCoefficent * (int)(curFloor / l_fStageAddCircle));
            }
        }

        if (curFloor < 75 && SceneManager.instance.GetCurGameMode() == SceneManager.eGAMEMODE.eMODE_CHAPTER)
        {
            float l_fNpcRatio = 0.75f;
            float l_fFloorCoef = ((int)(curFloor / 15)) * 0.05f;

            tmpProp.Npc_AttackValue *= (l_fNpcRatio + l_fFloorCoef);
            tmpProp.Npc_DefenseValue *= (l_fNpcRatio + l_fFloorCoef);
            tmpProp.Npc_HPValue *= (l_fNpcRatio + l_fFloorCoef);
        }


        tmpProp.Npc_ExpValue = tmpProp.ExpBase / 2 + (tmpProp.ExpBase / 2 + curFloor) * (tmpProp.ExpCorrec + 1);

        return tmpProp;
    }

	public NpcInfo.NpcProp GetNpcPropDataPublic(int npcIdx)
	{
		return GetNpcPropData(npcIdx);
	}

	private NpcInfo.NpcProp GetPlayerSummonPropData(SummonData summonData, int Idx)
	{
		NpcInfo.NpcProp tmpProp = new NpcInfo.NpcProp();
		int rowIdx = (int)m_NpcDataIndexs[summonData.m_PlayerSummonIndexCode];

		tmpProp.Npc_Code = CExcelData_NPC_DATA.instance.NPC_DATABASE_GetNPC_CODE(rowIdx);
		tmpProp.Npc_NameID = CExcelData_NPC_DATA.instance.NPC_DATABASE_GetNPC_NAME_ID(rowIdx);
		tmpProp.Npc_Scale = CExcelData_NPC_DATA.instance.NPC_DATABASE_GetNPC_SCALE(rowIdx);
		tmpProp.Npc_ArcheType = CExcelData_NPC_DATA.instance.NPC_DATABASE_GetNPC_ARCHETYPE(rowIdx);
		tmpProp.Battle_Type = (eNPC_BATTLE_TYPE)CExcelData_NPC_DATA.instance.NPC_DATABASE_GetBATTLE_TYPE(rowIdx);
		tmpProp.Npc_Size = CExcelData_NPC_DATA.instance.NPC_DATABASE_GetNPC_SIZE(rowIdx);
		tmpProp.Npc_Type = (eNPC_TYPE)CExcelData_NPC_DATA.instance.NPC_DATABASE_GetNPC_TYPE(rowIdx);
		tmpProp.Npc_Class = (eNPC_CLASS)CExcelData_NPC_DATA.instance.NPC_DATABASE_GetNPC_CLASS(rowIdx);
		tmpProp.Att_Cancel = CExcelData_NPC_DATA.instance.NPC_DATABASE_GetATT_CANCEL(rowIdx);
		tmpProp.Hit_Ani = CExcelData_NPC_DATA.instance.NPC_DATABASE_GetHIT_ANI(rowIdx);
		tmpProp.Target_Ruling = CExcelData_NPC_DATA.instance.NPC_DATABASE_GetTARGET_RULING(rowIdx);

		tmpProp.Npc_Exp = CExcelData_NPC_DATA.instance.NPC_DATABASE_GetNCP_EXP(rowIdx);
		tmpProp.Drop_Id = CExcelData_NPC_DATA.instance.NPC_DATABASE_GetDROP_ID(rowIdx);
		tmpProp.Drop_Count = CExcelData_NPC_DATA.instance.NPC_DATABASE_GetDROP_COUNT(rowIdx);
		tmpProp.Identity_Fnd = (eTargetState)CExcelData_NPC_DATA.instance.NPC_DATABASE_GetIDENTITY_FRIENDLY(rowIdx);
		tmpProp.Npc_Search = CExcelData_NPC_DATA.instance.NPC_DATABASE_GetNPC_SEARCH(rowIdx);

		tmpProp.Search_Range = CExcelData_NPC_DATA.instance.NPC_DATABASE_GetSEARCH_RANGE(rowIdx);

		tmpProp.Free_Activity = CExcelData_NPC_DATA.instance.NPC_DATABASE_GetFREE_ACTIVITY(rowIdx);
		tmpProp.Patrol_Link_Id = CExcelData_NPC_DATA.instance.NPC_DATABASE_GetPATROL_LINK_ID(rowIdx);
		tmpProp.First_Agro = CExcelData_NPC_DATA.instance.NPC_DATABASE_GetFIRST_AGRO(rowIdx);
		tmpProp.Family_Group = CExcelData_NPC_DATA.instance.NPC_DATABASE_GetFAMILY_GROUP(rowIdx);
		//Debug.Log("LoadNpc()  " + npcIdx);
		string strTmp = UtilManager.instance.SplitString("`", CExcelData_NPC_DATA.instance.NPC_DATABASE_GetENEMY_GROUP(rowIdx));
		if (string.IsNullOrEmpty(strTmp) != true)
		{
			tmpProp.Enemy_Group = UtilManager.instance.SplitIntArr(",", strTmp);
		}
		tmpProp.Ai_Type = CExcelData_NPC_DATA.instance.NPC_DATABASE_GetAI_TYPE(rowIdx);
		tmpProp.Ai_Pattern_id = CExcelData_NPC_DATA.instance.NPC_DATABASE_GetAI_PATTERN_ID(rowIdx);
		tmpProp.Time_Trigger_id = CExcelData_NPC_DATA.instance.NPC_DATABASE_GetTIME_TRIGGER_ID(rowIdx);
		tmpProp.Dead_Zone = CExcelData_NPC_DATA.instance.NPC_DATABASE_GetDEAD_ZONE(rowIdx);
		tmpProp.Alert_Distance_Min = CExcelData_NPC_DATA.instance.NPC_DATABASE_GetALERT_DISTANCE_MIN(rowIdx);
		tmpProp.Alert_Distance_Max = CExcelData_NPC_DATA.instance.NPC_DATABASE_GetALERT_DISTANCE_MAX(rowIdx);

		strTmp = UtilManager.instance.SplitString("`", CExcelData_NPC_DATA.instance.NPC_DATABASE_GetATT_COOLTIME(rowIdx));
		if (string.IsNullOrEmpty(strTmp) != true)
		{
			tmpProp.Att_CoolTime = UtilManager.instance.SplitFloatArr(",", strTmp);
		}

		tmpProp.Att_Speed = CExcelData_NPC_DATA.instance.NPC_DATABASE_GetATTACK_SPEED(rowIdx) / 100;
		//tmpProp.Casting_Speed = CExcelData_NPC_DATA.instance.NPC_DATABASE_GetCASTING_SPEED(rowIdx);
		tmpProp.Walk_Speed = CExcelData_NPC_DATA.instance.NPC_DATABASE_GetWALK_SPEED(rowIdx) / 100;
		tmpProp.Run_Speed = CExcelData_NPC_DATA.instance.NPC_DATABASE_GetRUN_SPEED(rowIdx) / 100;

		List<List<string>> splitListStr;
		string Normal_Attack_Id = CExcelData_NPC_DATA.instance.NPC_DATABASE_GetNORMAL_ATT_ID(rowIdx);
		tmpProp.link_SkillCode = new List<int>();
		tmpProp.link_SkillRate = new List<int>();
		splitListStr = UtilManager.instance.GetDataValueArr(Normal_Attack_Id);

		for (int n = 0; n < splitListStr.Count; ++n)
		{
			tmpProp.link_SkillCode.Add(int.Parse(splitListStr[n][0]));
			tmpProp.link_SkillRate.Add(int.Parse(splitListStr[n][1]));
		}

		strTmp = UtilManager.instance.SplitString("`", CExcelData_NPC_DATA.instance.NPC_DATABASE_GetNPC_SKILL_ID(rowIdx));
		if (string.IsNullOrEmpty(strTmp) != true)
		{
			tmpProp.Npc_skill_Id = UtilManager.instance.SplitIntArr(",", strTmp);
		}

		tmpProp.Npc_Attack_Min = CExcelData_NPC_DATA.instance.NPC_DATABASE_GetNPC_ATT_MIN(rowIdx);
		tmpProp.Npc_Attack_Max = CExcelData_NPC_DATA.instance.NPC_DATABASE_GetNPC_ATT_MAX(rowIdx);
		tmpProp.Npc_Def = CExcelData_NPC_DATA.instance.NPC_DATABASE_GetNPC_DEFENSE(rowIdx);
		tmpProp.Max_Hp = CExcelData_NPC_DATA.instance.NPC_DATABASE_GetNPC_MAX_HP(rowIdx);

		tmpProp.Critical_Rate = CExcelData_NPC_DATA.instance.NPC_DATABASE_GetNPC_CRITICAL_RATE(rowIdx);
		tmpProp.Critical_Power = CExcelData_NPC_DATA.instance.NPC_DATABASE_GetNPC_CRITICAL_POWER(rowIdx);
		tmpProp.Accuracy_Rate = CExcelData_NPC_DATA.instance.NPC_DATABASE_GetNPC_ACCURACY(rowIdx);
		tmpProp.Dodge_Rate = CExcelData_NPC_DATA.instance.NPC_DATABASE_GetNPC_DODGE(rowIdx);
		tmpProp.DeBuffResist = CExcelData_NPC_DATA.instance.NPC_DATABASE_GetNPC_DEBUFF_RESIST(rowIdx);
		tmpProp.Die_Effect_Idx = CExcelData_NPC_DATA.instance.NPC_DATABASE_GetDIE_EFFECT(rowIdx);
		tmpProp.SpawnEff = CExcelData_NPC_DATA.instance.NPC_DATABASE_GetSPAWN_EFFECT(rowIdx);

		tmpProp.Att_Revision = CExcelData_NPC_DATA.instance.NPC_DATABASE_GetATT_REVISION(rowIdx);
		tmpProp.HP_Revision = CExcelData_NPC_DATA.instance.NPC_DATABASE_GetHP_REVISION(rowIdx);

		tmpProp.Summon_Check_equip = false;



		tmpProp.Summon_EnchantLevel = summonData.EnchantLevel;
		tmpProp.SummonExpLevel = (uint)summonData.ExpLevel;

        tmpProp.SummonExp = summonData.Exp;
		tmpProp.SummonLevelMaxExp = HeroMedalLvUpFactorData.GetSummonMaxExp((int)tmpProp.SummonExpLevel);

		summonData.SummonLevelMaxExp = tmpProp.SummonLevelMaxExp;

		tmpProp.Grade = summonData.Grade;
		tmpProp.Summon_nContentIndex = summonData.nContentIndex;

		tmpProp.Summon_IconPathName = summonData.m_PlayerSummonIconName;

		SetSummonData(tmpProp, tmpProp.Grade);
		return tmpProp;
	}
	public void SetSummonData(NpcInfo.NpcProp tmpProp, int Grade)
	{
		int keyCode = tmpProp.Summon_nContentIndex + 1;

		if (Grade == 0)
			Grade = 1;

		int roeIdx = (Grade - 1) * 1000 + keyCode;

		int Idx = (int)m_SummonDataIndexs[(uint)roeIdx];

		//Grade에 따른 옵션 설명
		tmpProp.Summon_TwoStartHis = CExcelData_SUMMON_DATA.instance.SUMMON_DATABASE_GetTWOSTAR_OPTION_DISC(Idx);
		tmpProp.Summon_ThreeStartHis = CExcelData_SUMMON_DATA.instance.SUMMON_DATABASE_GetTHREESTAR_OPTION_DISC(Idx);
		tmpProp.Summon_FourStartHis = CExcelData_SUMMON_DATA.instance.SUMMON_DATABASE_GetFOURSTAR_OPTION_DISC(Idx);
		tmpProp.Summon_FiveStartHis = CExcelData_SUMMON_DATA.instance.SUMMON_DATABASE_GetFIVESTAR_OPTION_DISC(Idx);
		tmpProp.SummonPropety.Clear();
		for (int i = 0; i < 4; i++)
		{
			Set_SummonProrety(tmpProp, tmpProp.Summon_nContentIndex, i);
			tmpProp.SetSummonAbilityLinkSkill(null, i);
		}

		Set_SummonInfo(tmpProp, Idx);
		NpcManager.instance.Set_SummonExpAbility(tmpProp);

	}
	public void Set_SummonProrety(NpcInfo.NpcProp tmpProp, int SummonIndex, int Grade)
	{

		NpcInfo.SummonPropetyOptions tmpPropety = new NpcInfo.SummonPropetyOptions();
		List<List<string>> splitListStr = new List<List<string>>();
		switch (Grade)
		{
			case 0:
				splitListStr = UtilManager.instance.GetDataValueArr(CExcelData_SUMMON_DATA.instance.SUMMON_DATABASE_GetTWOSTAR_OPTION(SummonIndex));
				break;
			case 1:
				splitListStr = UtilManager.instance.GetDataValueArr(CExcelData_SUMMON_DATA.instance.SUMMON_DATABASE_GetTHREESTAR_OPTION(SummonIndex));
				break;
			case 2:
				splitListStr = UtilManager.instance.GetDataValueArr(CExcelData_SUMMON_DATA.instance.SUMMON_DATABASE_GetFOURSTAR_OPTION(SummonIndex));
				break;
			case 3:
				splitListStr = UtilManager.instance.GetDataValueArr(CExcelData_SUMMON_DATA.instance.SUMMON_DATABASE_GetFIVESTAR_OPTION(SummonIndex));
				break;
			default:
				splitListStr.Clear();
				break;
		}

		for (int i = 0; i < splitListStr.Count; i++)
		{
			//tmpAtkInfo.skillinfo.system_Affect_Code.Add(GetAffectCode(splitListStr[i][0]));
			tmpPropety.AffectCode = ((eAffectCode)System.Enum.Parse(typeof(eAffectCode), "e" + splitListStr[i][0]));


			tmpPropety.Start_Rate = (int.Parse(splitListStr[i][2]));
			tmpPropety.Damage_Value = (float.Parse(splitListStr[i][3]));
			tmpPropety.DwellTime = (float.Parse(splitListStr[i][4]));
			tmpPropety.DamageSecond = (float.Parse(splitListStr[i][5]));
			tmpProp.SummonPropety.Add(tmpPropety);
		}
		splitListStr.Clear();

	}

	public void Set_SummonExpAbility(NpcInfo.NpcProp tmpProp)
    {
        int keyCode = tmpProp.Summon_nContentIndex + 1;
        int grade = tmpProp.Grade;
        // GUNNY ADD 20170622
        if (grade == 0)
            grade = 1;
        // GUNNY ADD 20170622 END

        int roeIdx = (grade - 1) * 1000 + keyCode;

        int Idx = (int)m_SummonDataIndexs[(uint)roeIdx];

        tmpProp.Npc_AttackValue = SummonAttackValue(tmpProp, Idx);
        tmpProp.Npc_DefenseValue = SummonDefenseValue(tmpProp, Idx);
        tmpProp.Npc_HPValue = SummonHpValue(tmpProp, Idx);

        tmpProp.Summon_EnchantLevelMax = (int)CExcelData_SUMMON_DATA.instance.SUMMON_DATABASE_GetMAX_UPGRADE(Idx);//1000

        //tmpProp.Npc_AttackValue						= HeroMedalLvUpFactorData.GetEnchantUpAbilityAttack(tmpProp.Npc_AttackValue, tmpProp.Summon_EnchantLevel - 1);
        //tmpProp.Npc_DefenseValue					= HeroMedalLvUpFactorData.GetEnchantUpAbilityDefense(tmpProp.Npc_DefenseValue, tmpProp.Summon_EnchantLevel - 1);
        if (tmpProp.Summon_EnchantLevel == 0)
        {
            return;
        }
        tmpProp.Npc_AttackValue		= HeroMedalLvUpFactorData.GetEnchantUpAbilityAttack(tmpProp.Npc_AttackValue, tmpProp.Summon_EnchantLevel, grade);
        tmpProp.Npc_DefenseValue	= HeroMedalLvUpFactorData.GetEnchantUpAbilityDefense(tmpProp.Npc_DefenseValue, tmpProp.Summon_EnchantLevel, grade);
		tmpProp.Npc_HPValue			= HeroMedalLvUpFactorData.GetEnchantUpAbilityHP(tmpProp.Npc_HPValue, tmpProp.Summon_EnchantLevel, grade);
	}

	public float SummonAttackValue(NpcInfo.NpcProp tmpProp, int Idx)
    {
        float AttackValue = 0;

        float atkStart = CExcelData_SUMMON_DATA.instance.SUMMON_DATABASE_GetATTACK_START(Idx);
        float attNorm = CExcelData_SUMMON_DATA.instance.SUMMON_DATABASE_GetATTACK_NORM(Idx);
        float atkQuot = CExcelData_SUMMON_DATA.instance.SUMMON_DATABASE_GetATTACK_QUOT(Idx);

        float l_fStatRatio = 1f;
        float l_fPetLevelCircle = CExcelData_NPC_FACTOR_DATA.instance.NPC_FACTOR_DATABASE_GetPET_LEVEL_CIRCLE(0);
        float l_fPetLevelCoefficent = CExcelData_NPC_FACTOR_DATA.instance.NPC_FACTOR_DATABASE_GetPET_LEVEL_COEFFICENT(0);

        if (tmpProp.SummonExpLevel > l_fPetLevelCircle)
        {
            l_fStatRatio = l_fPetLevelCoefficent * (int)(tmpProp.SummonExpLevel / l_fPetLevelCircle);
        }

        // (50레벨 능력치) * {((레벨) / 50 의 몫[INT] ) * (증가 계수)} + {((레벨)  % 50) 의 레벨 능력치}
        AttackValue = (atkStart + (attNorm * UtilManager.instance.CalcPow(tmpProp.SummonExpLevel, atkQuot))) * l_fStatRatio;

        return AttackValue;
    }
	public float SummonDefenseValue(NpcInfo.NpcProp tmpProp, int Idx)
    {
        float DefenseValue = 0;

        float defStart = CExcelData_SUMMON_DATA.instance.SUMMON_DATABASE_GetDEFENSE_START(Idx);
        float defNorm = CExcelData_SUMMON_DATA.instance.SUMMON_DATABASE_GetDEFENSE_NORM(Idx);
        float defQuot = CExcelData_SUMMON_DATA.instance.SUMMON_DATABASE_GetDEFENSE_QUOT(Idx);

        float l_fStatRatio = 1f;
        float l_fPetLevelCircle = CExcelData_NPC_FACTOR_DATA.instance.NPC_FACTOR_DATABASE_GetPET_LEVEL_CIRCLE(0);
        float l_fPetLevelCoefficent = CExcelData_NPC_FACTOR_DATA.instance.NPC_FACTOR_DATABASE_GetPET_LEVEL_COEFFICENT(0);

        if (tmpProp.SummonExpLevel > l_fPetLevelCircle)
        {
            l_fStatRatio = l_fPetLevelCoefficent * (int)(tmpProp.SummonExpLevel / l_fPetLevelCircle);
        }

        // (50레벨 능력치) * {((레벨) / 50 의 몫[INT] ) * (증가 계수)} + {((레벨)  % 50) 의 레벨 능력치}
        DefenseValue = (defStart + (defNorm * UtilManager.instance.CalcPow(tmpProp.SummonExpLevel, defQuot))) * l_fStatRatio;

        return DefenseValue;
    }

	public float SummonHpValue(NpcInfo.NpcProp tmpProp, int Idx)
    {
        float HPValue = 0;

        float hpStart = CExcelData_SUMMON_DATA.instance.SUMMON_DATABASE_GetHP_START(Idx);//1000
        float hpNorm = CExcelData_SUMMON_DATA.instance.SUMMON_DATABASE_GetHP_NORM(Idx);
        float hpQuot = CExcelData_SUMMON_DATA.instance.SUMMON_DATABASE_GetHP_QUOT(Idx);//1.4

        float l_fStatRatio = 1f;
        float l_fPetLevelCircle = CExcelData_NPC_FACTOR_DATA.instance.NPC_FACTOR_DATABASE_GetPET_LEVEL_CIRCLE(0);
        float l_fPetLevelCoefficent = CExcelData_NPC_FACTOR_DATA.instance.NPC_FACTOR_DATABASE_GetPET_LEVEL_COEFFICENT(0);

        if (tmpProp.SummonExpLevel > l_fPetLevelCircle)
        {
            l_fStatRatio = l_fPetLevelCoefficent * (int)(tmpProp.SummonExpLevel / l_fPetLevelCircle);
        }

        // (50레벨 능력치) * {((레벨) / 50 의 몫[INT] ) * (증가 계수)} + {((레벨)  % 50) 의 레벨 능력치}
        HPValue = (hpStart + (hpNorm * UtilManager.instance.CalcPow(tmpProp.SummonExpLevel, hpQuot))) * l_fStatRatio;

        return HPValue;
    }

	public void GetSummonAbilityData(NpcInfo.NpcProp tmpProp)
	{

		int keyCode = tmpProp.Summon_nContentIndex + 1;
		int roeIdx = (tmpProp.Grade - 1) * 1000 + keyCode;
		int Idx = (int)m_SummonDataIndexs[(uint)roeIdx];

		tmpProp.Summon_IconPathName = tmpProp.Summon_IconPathName;

		//훈장 레벨 능력 가중치
		float atkStart = CExcelData_SUMMON_DATA.instance.SUMMON_DATABASE_GetATTACK_START(Idx);
		float attNorm = CExcelData_SUMMON_DATA.instance.SUMMON_DATABASE_GetATTACK_NORM(Idx);
		float atkQuot = CExcelData_SUMMON_DATA.instance.SUMMON_DATABASE_GetATTACK_QUOT(Idx);

		tmpProp.Npc_AttackValue = atkStart + (attNorm * UtilManager.instance.CalcPow(tmpProp.SummonExpLevel, atkQuot));
		tmpProp.Npc_AttackValue = tmpProp.Npc_AttackValue + (tmpProp.Npc_AttackValue * tmpProp.medalAbility);

		float defStart = CExcelData_SUMMON_DATA.instance.SUMMON_DATABASE_GetDEFENSE_START(Idx);
		float defNorm = CExcelData_SUMMON_DATA.instance.SUMMON_DATABASE_GetDEFENSE_NORM(Idx);
		float defQuot = CExcelData_SUMMON_DATA.instance.SUMMON_DATABASE_GetDEFENSE_QUOT(Idx);
		tmpProp.Npc_DefenseValue = defStart + (defNorm * UtilManager.instance.CalcPow(tmpProp.SummonExpLevel, defQuot));
		tmpProp.Npc_DefenseValue = tmpProp.Npc_DefenseValue + (tmpProp.Npc_DefenseValue * tmpProp.medalAbility);

		float hpStart = CExcelData_SUMMON_DATA.instance.SUMMON_DATABASE_GetHP_START(Idx);//1000
		float hpNorm = CExcelData_SUMMON_DATA.instance.SUMMON_DATABASE_GetHP_NORM(Idx);
		float hpQuot = CExcelData_SUMMON_DATA.instance.SUMMON_DATABASE_GetHP_QUOT(Idx);//1.4
		tmpProp.Npc_HPValue = hpStart + (hpNorm * UtilManager.instance.CalcPow(tmpProp.SummonExpLevel, hpQuot));
		tmpProp.Npc_HPValue = tmpProp.Npc_HPValue + (tmpProp.Npc_HPValue * tmpProp.medalAbility);
	}

	public int NextGradePieceCount(NpcInfo.NpcProp tmpProp)
	{
		int keyCode = tmpProp.Summon_nContentIndex + 1;
		int roeIdx = (tmpProp.Grade - 1) * 1000 + keyCode;
		int Idx = (int)m_SummonDataIndexs[(uint)roeIdx];
		int count = CExcelData_SUMMON_DATA.instance.SUMMON_DATABASE_GetCHANGE_COUNT(Idx);//1000

		return count;
	}

	private void LoadNpc(int npcIdx, NpcSpawnerData npcSpawnerInfo, int idx)
	{
		NpcInfo.NpcProp tmpProp = GetNpcPropData(npcIdx);
		string strModelPath = null;

		m_npcProp.Add(tmpProp);

		NpcInfo.NpcModelList tmpModel = new NpcInfo.NpcModelList();
		tmpModel.Model_Data_route = CExcelData_NPC_MODEL_LIST.instance.NPC_MODEL_LISTBASE_GetMODEL_DATA_ROUTE(m_NpcModelDic[(int)tmpProp.Npc_ArcheType]);
		tmpModel.Model_Prefabs_Name = CExcelData_NPC_MODEL_LIST.instance.NPC_MODEL_LISTBASE_GetPREFABS_NAME(m_NpcModelDic[(int)tmpProp.Npc_ArcheType]);

		strModelPath = tmpModel.Model_Data_route + "/" + tmpModel.Model_Prefabs_Name;

		if (tmpProp.Npc_Type == eNPC_TYPE.eZONEWALL_OBJ)// || tmpProp.Npc_Type == 3)
		{
			LoadBaricadePrefabObj(strModelPath, idx, tmpModel, npcSpawnerInfo);
		}
		else
		{
			if (npcSpawnerInfo.m_nSpawnerID >= SUMMON_START_INDEX)
			{
				for (int i = 0; i < CONST_SUMMON_MAX; i++)
				{
					LoadPrefabObj(strModelPath, npcSpawnerInfo, tmpProp, tmpModel, true, true);
				}
			}
			else
			{
				LoadPrefabObj(strModelPath, npcSpawnerInfo, tmpProp, tmpModel, false, true);
			}

		}

	}

	private int LoadSummonEquipSlotGroup(Dictionary<int, List<SummonSlotData>> h_SummonslotGroup, int SummonIndex)
	{
		int a_hChaeckEquipIndex = -1;

		PlayerInfo a_kPlayerInfo = PlayerManager.instance.m_PlayerInfo[PlayerManager.MYPLAYER_INDEX];
		List<SummonSlotData> a_kSummonSlotData = new List<SummonSlotData>();

		if (h_SummonslotGroup == null)
		{
			return a_hChaeckEquipIndex;
		}

		h_SummonslotGroup.TryGetValue(1, out a_kSummonSlotData);
		for (int i = 0; i < a_kSummonSlotData.Count; i++)
		{
			if (a_kSummonSlotData[i].m_SlotSummonIndex == SummonIndex)
			{
				return a_hChaeckEquipIndex = i;
			}
		}
		return a_hChaeckEquipIndex;
	}

	private void LoadPlayerSummon(SummonData summonData, string NpcIconPathName, int idx)
	{
		if (tmpProp.Npc_Size <= 2)
		{
			tmpProp.Npc_Scale *= 0.9f;
		}
		else if (tmpProp.Npc_Size <= 4)
		{
			tmpProp.Npc_Scale *= 0.85f;
		}
		else
		{
			tmpProp.Npc_Scale *= 0.8f;
		}

		tmpProp.Run_Speed			= tmpProp.Run_Speed * SUMMON_RUNSPEED;
		tmpProp.Att_Speed			= tmpProp.Att_Speed * SUMMON_ATTSPEED;

		tmpProp.Npc_NameID = summonData.m_PlayerSummonName;

		tmpProp.Summon_Check_equip = summonData.Equip;
#if SUMMONTEST
        tmpProp.Summon_Check_Acquire     = true;
#else
		tmpProp.Summon_Check_Acquire = summonData.Acquire;
#endif


		tmpProp.Summon_Check_isAcquire = summonData.isAcquire;

		tmpProp.Summon_Check_Die = false;
		tmpProp.Summon_nCount = summonData.nCount;
		tmpProp.Summon_nContentIndex = summonData.nContentIndex;
		tmpProp.Summon_IconPathName = NpcIconPathName;

		tmpProp.Summon_EquipIndex = -1;
		tmpProp.Summon_EquipSlotGroupIndex = -1;


		tmpModel.Model_Data_route = CExcelData_NPC_MODEL_LIST.instance.NPC_MODEL_LISTBASE_GetMODEL_DATA_ROUTE(m_NpcModelDic[(int)tmpProp.Npc_ArcheType]);
		tmpModel.Model_Prefabs_Name = CExcelData_NPC_MODEL_LIST.instance.NPC_MODEL_LISTBASE_GetPREFABS_NAME(m_NpcModelDic[(int)tmpProp.Npc_ArcheType]);
		strModelPath = tmpModel.Model_Data_route + "/" + tmpModel.Model_Prefabs_Name;
		m_PlayerSummonModelList.Add(tmpModel);
	}


	public void LoadGameobjectEquipSummon()
	{
		PlayerInfo a_kPlayerInfo = PlayerManager.instance.m_PlayerInfo[PlayerManager.MYPLAYER_INDEX];
		Dictionary<int, List<SummonSlotData>> a_kSummonSlotGroup = a_kPlayerInfo.m_SummonSlotGroup;

		ISummonAbilities a_iSummonAbilities = PlayerManager.instance.GetPlayerSummonAbilities();
		if (a_kSummonSlotGroup == null)
		{
			return;
		}

        foreach (KeyValuePair<int, List<SummonSlotData>> SummonslotGroup in a_kSummonSlotGroup)
		{
			for (int i = 0; i < SummonslotGroup.Value.Count; i++)
			{
				int a_kSummonIndex = SummonslotGroup.Value[i].m_SlotSummonIndex;
				int a_kSlotIndex = i;
				int a_kSlotGroupIndex = SummonslotGroup.Key;
				if (a_kSummonIndex < 0)
				{
					continue;
				}

				NpcInfo.NpcProp a_kSummonProp = m_PlayerSummonProp[a_kSummonIndex];
				NpcInfo.NpcModelList a_kSummonModel = m_PlayerSummonModelList[a_kSummonIndex];
				SummonData a_kSummonData = m_PlayerSummonList[a_kSummonIndex];

				string a_kStrModelPathstrModelPath = a_kSummonModel.Model_Data_route + "/" + a_kSummonModel.Model_Prefabs_Name;

				m_PlayerSummonProp[a_kSummonIndex].Summon_EquipIndex = a_kSlotIndex;
				m_PlayerSummonProp[a_kSummonIndex].Summon_EquipSlotGroupIndex = a_kSlotGroupIndex;

                if (a_kSummonProp.curSummonEquipItems == null)
                {
                    a_kSummonProp.curSummonEquipItems = new EquipedItems();
                }

                if (a_kSummonData.equipItems != null)
                {
                    a_kSummonProp.curSummonEquipItems = a_kSummonData.equipItems;

                    if (a_kSummonProp.summonItemStatus == null)
                    {
                        a_kSummonProp.summonItemStatus = new ItemStatus(a_kSummonProp.curSummonEquipItems.m_dicItems, 0);
                        if (a_kSlotGroupIndex == 2)
                        {
                            InGameItemManager.instance.RemovePartyAffect(a_kSummonIndex);
                        }
                    }
                }
                else
                {
                    SetSummonEquipItems(a_kSummonIndex);
                    if (a_kSummonProp.summonItemStatus == null)
                    {
                        a_kSummonProp.summonItemStatus = new ItemStatus();
                    }
                }

                GameObject a_kObject = LoadPlayerSummonPrefabObj(a_kStrModelPathstrModelPath, a_kSummonProp, a_kSummonModel, a_kSummonProp.Summon_nContentIndex, false, a_kSummonData);
				CharacterBase a_kCharacterBase = a_kObject.GetComponent<CharacterBase>();

				AddPartyAbilities(a_kCharacterBase, PlayerManager.MYPLAYER_INDEX);
				a_iSummonAbilities.EquipSummon(a_kCharacterBase);

				if (SummonslotGroup.Key == 2)
				{
					SetActiveGameObject(a_kObject, false);
				}
			}
		}
	}


	public void LoadPvpGameobjectEquipSummon()
	{
		ISummonAbilities a_iSummonAbilities = PlayerManager.instance.GetPlayerSummonAbilities();

		for (int i = 0; i < m_PlayerSummonProp.Count; i++)
		{
			int a_kSummonIndex = m_PlayerSummonProp[i].Summon_nContentIndex;
		if (a_kSummonIndex < 0)
			{
				continue;
			}

			NpcInfo.NpcModelList a_kSummonModel = m_PlayerSummonModelList[i];
			SummonData a_kSummonData = m_PlayerSummonList[a_kSummonIndex];

            if (m_PlayerSummonProp[i].curSummonEquipItems == null)
            {
                m_PlayerSummonProp[i].curSummonEquipItems = new EquipedItems();
            }


            if (a_kSummonData.equipItems != null)
            {
                m_PlayerSummonProp[i].curSummonEquipItems = a_kSummonData.equipItems;

                if (m_PlayerSummonProp[i].summonItemStatus == null)
                {
                    m_PlayerSummonProp[i].summonItemStatus = new ItemStatus(m_PlayerSummonProp[i].curSummonEquipItems.m_dicItems, 0);
                }
            }
            else
            {
                if (m_PlayerSummonProp[i].summonItemStatus == null)
                {
                    m_PlayerSummonProp[i].summonItemStatus = new ItemStatus();
                }
            }

            string a_kStrModelPathstrModelPath = a_kSummonModel.Model_Data_route + "/" + a_kSummonModel.Model_Prefabs_Name;

			GameObject a_kObject = LoadPlayerSummonPrefabObj(a_kStrModelPathstrModelPath, m_PlayerSummonProp[i], a_kSummonModel, m_PlayerSummonProp[i].Summon_nContentIndex, false, a_kSummonData);
			CharacterBase a_kCharacterBase = a_kObject.GetComponent<CharacterBase>();

			m_PlayerSummonProp[i].Summon_EquipIndex = i;
			m_PlayerSummonProp[i].Summon_EquipSlotGroupIndex = 1;

			m_PlayerSummonList[a_kSummonIndex].summonCBase = a_kCharacterBase;
            AddPartyAbilities(a_kCharacterBase, PlayerManager.MYPLAYER_INDEX);
            a_iSummonAbilities.EquipSummon(a_kCharacterBase);

		}
	}

	public void SetActiveGameObject(GameObject go, bool kActive)
	{
		if (go.activeSelf != kActive)
		{
			go.SetActive(kActive);
		}
	}

	public void LoadPvpPet(SummonData summonData, int idx, bool bAgainst, ISummonAbilities iEnemySummonAbilities, int equipSlotIndex)
    {
        PlayerInfo a_kPlayerInfo = PlayerManager.instance.m_PlayerInfo[PlayerManager.PVPPLAYER_INDEX];
        Dictionary<int, List<SummonSlotData>> a_kSummonSlotGroup = a_kPlayerInfo.m_SummonSlotGroup;
        NpcInfo.NpcProp tmpProp = GetPlayerSummonPropData(summonData, idx);

		tmpProp.Npc_Scale = tmpProp.Npc_Scale * SUMMON_SCALE;
		tmpProp.Run_Speed = tmpProp.Run_Speed * SUMMON_RUNSPEED;
		tmpProp.Att_Speed = tmpProp.Att_Speed * SUMMON_ATTSPEED;

        tmpProp.Default_Run_Speed = tmpProp.Run_Speed;
        tmpProp.Default_Att_Speed = tmpProp.Att_Speed;

        tmpProp.Summon_nContentIndex = summonData.nContentIndex;
		tmpProp.Summon_EquipIndex = equipSlotIndex;
		tmpProp.Summon_EquipObjIndex = equipSlotIndex;


		iEnemySummonAbilities.AddSummon(summonData, tmpProp);

		string strModelPath = null;
		NpcInfo.NpcModelList tmpModel = new NpcInfo.NpcModelList();

		tmpModel.Model_Data_route = CExcelData_NPC_MODEL_LIST.instance.NPC_MODEL_LISTBASE_GetMODEL_DATA_ROUTE(m_NpcModelDic[(int)tmpProp.Npc_ArcheType]);
		tmpModel.Model_Prefabs_Name = CExcelData_NPC_MODEL_LIST.instance.NPC_MODEL_LISTBASE_GetPREFABS_NAME(m_NpcModelDic[(int)tmpProp.Npc_ArcheType]);
		strModelPath = tmpModel.Model_Data_route + "/" + tmpModel.Model_Prefabs_Name;


        foreach (KeyValuePair<int, List<SummonSlotData>> SummonslotGroup in a_kSummonSlotGroup)
        {
            for (int i = 0; i < SummonslotGroup.Value.Count; i++)
            {
                int a_kSlotGroupIndex = SummonslotGroup.Key;
                if( a_kSlotGroupIndex == 1)
                {
                    int a_kSummonIndex = SummonslotGroup.Value[i].m_SlotSummonIndex;
                    int a_kSlotIndex = i;

                    if (tmpProp.curSummonEquipItems == null)
                    {
                        tmpProp.curSummonEquipItems = new EquipedItems();
                    }

                    //a_kSummonData.equipItems가 null인 소환수는 장비 장착이 안됀 상태
                    if (summonData.equipItems != null)
                    {
                        tmpProp.curSummonEquipItems = summonData.equipItems;

                        if (tmpProp.summonItemStatus == null)
                        {
                            tmpProp.summonItemStatus = new ItemStatus(tmpProp.curSummonEquipItems.m_dicItems, 1);
                        }
                    }
                    else
                    {
                        if (tmpProp.summonItemStatus == null)
                        {
                            tmpProp.summonItemStatus = new ItemStatus();
                        }
                    }
                }
            }
        }

		GameObject pvpPlayerPetObj = LoadPlayerSummonPrefabObj(strModelPath, tmpProp, tmpModel, summonData.nContentIndex, bAgainst, summonData);
		summonData.summonCBase.m_MyObj = pvpPlayerPetObj.transform;

		iEnemySummonAbilities.EquipSummon(summonData.summonCBase, true);
	}

	private void LoadDataNpcPatternInfo(uint nAiPatternId)
	{
		if (nAiPatternId == 0)
			return;

		// List
		if (m_NpcPatternDatas.ContainsKey(nAiPatternId))
		{
			// Data
			for (int i = 0; i < m_NpcPatternDatas[nAiPatternId].Count; ++i)
			{
				NpcAI_PatternInfo.Pattern_TableData data = new NpcAI_PatternInfo.Pattern_TableData();
				data.Page_Index = CExcelData_PATTERN_DATA.instance.PATTERN_DATABASE_GetPAGE_INDEX(m_NpcPatternDatas[nAiPatternId][i]);
				data.End_ConDition = CExcelData_PATTERN_DATA.instance.PATTERN_DATABASE_GetEND_CONDITION(m_NpcPatternDatas[nAiPatternId][i]);
				data.Condition_value = CExcelData_PATTERN_DATA.instance.PATTERN_DATABASE_GetCONDITION_VALUE(m_NpcPatternDatas[nAiPatternId][i]);
				data.Pattern_Type = CExcelData_PATTERN_DATA.instance.PATTERN_DATABASE_GetPATTERN_TYPE(m_NpcPatternDatas[nAiPatternId][i]);
				data.Action_Index = CExcelData_PATTERN_DATA.instance.PATTERN_DATABASE_GetACTION_INDEX(m_NpcPatternDatas[nAiPatternId][i]);
				data.Action_Rate = CExcelData_PATTERN_DATA.instance.PATTERN_DATABASE_GetACTION_RATE(m_NpcPatternDatas[nAiPatternId][i]);
				data.Cool_Time = CExcelData_PATTERN_DATA.instance.PATTERN_DATABASE_GetCOOL_TIME(m_NpcPatternDatas[nAiPatternId][i]);
				data.Repeat_Time = CExcelData_PATTERN_DATA.instance.PATTERN_DATABASE_GetREPEAT_TIME(m_NpcPatternDatas[nAiPatternId][i]);
				data.Repeat_Type = CExcelData_PATTERN_DATA.instance.PATTERN_DATABASE_GetREPEAT_TYPE(m_NpcPatternDatas[nAiPatternId][i]);

				string strTmp = UtilManager.instance.SplitString("`", CExcelData_PATTERN_DATA.instance.PATTERN_DATABASE_GetACTION_TYPE(m_NpcPatternDatas[nAiPatternId][i]));
				if (string.IsNullOrEmpty(strTmp) != true)
				{
					data.Action_Type = UtilManager.instance.SplitIntArr(",", strTmp);
				}

				strTmp = CExcelData_PATTERN_DATA.instance.PATTERN_DATABASE_GetACTION_INFO(m_NpcPatternDatas[nAiPatternId][i]);
				if (string.IsNullOrEmpty(strTmp) != true)
				{
					List<List<string>> splitListStr;
					data.Action_Info = new List<List<float>>();
					splitListStr = UtilManager.instance.GetDataValueArr(strTmp);
					for (int n = 0; n < splitListStr.Count; ++n)
					{
						List<float> kInfoData = new List<float>();
						for (int m = 0; m < splitListStr[n].Count; ++m)
						{
							kInfoData.Add(float.Parse(splitListStr[n][m]));
						}
						data.Action_Info.Add(kInfoData);
					}
				}

				strTmp = UtilManager.instance.SplitString("`", CExcelData_PATTERN_DATA.instance.PATTERN_DATABASE_GetTARGET_TYPE(m_NpcPatternDatas[nAiPatternId][i]));
				if (string.IsNullOrEmpty(strTmp) != true)
				{
					data.Target_Type = UtilManager.instance.SplitIntArr(",", strTmp);
				}

				strTmp = CExcelData_PATTERN_DATA.instance.PATTERN_DATABASE_GetTARGET_INFO(m_NpcPatternDatas[nAiPatternId][i]);
				if (string.IsNullOrEmpty(strTmp) != true)
				{
					List<List<string>> splitListStr;
					data.Target_Info = new List<List<float>>();
					splitListStr = UtilManager.instance.GetDataValueArr(strTmp);
					for (int n = 0; n < splitListStr.Count; ++n)
					{
						List<float> kInfoData = new List<float>();
						for (int m = 0; m < splitListStr[n].Count; ++m)
						{
							kInfoData.Add(float.Parse(splitListStr[n][m]));
						}
						data.Target_Info.Add(kInfoData);
					}
				}

				strTmp = UtilManager.instance.SplitString("`", CExcelData_PATTERN_DATA.instance.PATTERN_DATABASE_GetWARNING_ID(m_NpcPatternDatas[nAiPatternId][i]));
				if (string.IsNullOrEmpty(strTmp) != true)
				{
					data.Warning_Id = UtilManager.instance.SplitIntArr(",", strTmp);
				}

				data.Pattern_Skip = CExcelData_PATTERN_DATA.instance.PATTERN_DATABASE_GetPATTERN_SKIP(m_NpcPatternDatas[nAiPatternId][i]);

				data.Pattern_Cancleable = CExcelData_PATTERN_DATA.instance.PATTERN_DATABASE_GetPATTERN_CANCEL(m_NpcPatternDatas[nAiPatternId][i]);

				if (m_NpcAI_PatternDatas.ContainsKey(nAiPatternId))
				{
					bool bAddData = true;
					for (int j = 0; j < m_NpcAI_PatternDatas[nAiPatternId].Count; ++j)
					{
						if (m_NpcAI_PatternDatas[nAiPatternId][j].Page_Index == data.Page_Index &&
							m_NpcAI_PatternDatas[nAiPatternId][j].Action_Index == data.Action_Index)
						{
							bAddData = false;
							break;
						}
					}
					if (bAddData)
						m_NpcAI_PatternDatas[nAiPatternId].Add(data);
				}
				else
				{
					List<NpcAI_PatternInfo.Pattern_TableData> temList = new List<NpcAI_PatternInfo.Pattern_TableData>();
					temList.Add(data);
					m_NpcAI_PatternDatas.Add(nAiPatternId, temList);
				}
			}
		}
	}


	private void LoadDataSummonPatternInfo(uint nAiPatternId)
	{
		if (nAiPatternId == 0)
			return;

		// List
		if (m_SummonPatternDatas.ContainsKey(nAiPatternId))
		{
			// Data
			for (int i = 0; i < m_SummonPatternDatas[nAiPatternId].Count; ++i)
			{
				NpcAI_PatternInfo.Pattern_TableData data = new NpcAI_PatternInfo.Pattern_TableData();
				data.Page_Index = CExcelData_PATTERN_SUMMON_DATA.instance.PATTERN_DATABASE_GetPAGE_INDEX(m_SummonPatternDatas[nAiPatternId][i]);
				data.End_ConDition = CExcelData_PATTERN_SUMMON_DATA.instance.PATTERN_DATABASE_GetEND_CONDITION(m_SummonPatternDatas[nAiPatternId][i]);
				data.Condition_value = CExcelData_PATTERN_SUMMON_DATA.instance.PATTERN_DATABASE_GetCONDITION_VALUE(m_SummonPatternDatas[nAiPatternId][i]);
				data.Pattern_Type = CExcelData_PATTERN_SUMMON_DATA.instance.PATTERN_DATABASE_GetPATTERN_TYPE(m_SummonPatternDatas[nAiPatternId][i]);
				data.Action_Index = CExcelData_PATTERN_SUMMON_DATA.instance.PATTERN_DATABASE_GetACTION_INDEX(m_SummonPatternDatas[nAiPatternId][i]);
				data.Action_Rate = CExcelData_PATTERN_SUMMON_DATA.instance.PATTERN_DATABASE_GetACTION_RATE(m_SummonPatternDatas[nAiPatternId][i]);
				data.Cool_Time = CExcelData_PATTERN_SUMMON_DATA.instance.PATTERN_DATABASE_GetCOOL_TIME(m_SummonPatternDatas[nAiPatternId][i]);
				data.Repeat_Time = CExcelData_PATTERN_SUMMON_DATA.instance.PATTERN_DATABASE_GetREPEAT_TIME(m_SummonPatternDatas[nAiPatternId][i]);
				data.Repeat_Type = CExcelData_PATTERN_SUMMON_DATA.instance.PATTERN_DATABASE_GetREPEAT_TYPE(m_SummonPatternDatas[nAiPatternId][i]);

				string strTmp = UtilManager.instance.SplitString("`", CExcelData_PATTERN_SUMMON_DATA.instance.PATTERN_DATABASE_GetACTION_TYPE(m_SummonPatternDatas[nAiPatternId][i]));
				if (string.IsNullOrEmpty(strTmp) != true)
				{
					data.Action_Type = UtilManager.instance.SplitIntArr(",", strTmp);
				}

				strTmp = CExcelData_PATTERN_SUMMON_DATA.instance.PATTERN_DATABASE_GetACTION_INFO(m_SummonPatternDatas[nAiPatternId][i]);
				if (string.IsNullOrEmpty(strTmp) != true)
				{
					List<List<string>> splitListStr;
					data.Action_Info = new List<List<float>>();
					splitListStr = UtilManager.instance.GetDataValueArr(strTmp);
					for (int n = 0; n < splitListStr.Count; ++n)
					{
						List<float> kInfoData = new List<float>();
						for (int m = 0; m < splitListStr[n].Count; ++m)
						{
							kInfoData.Add(float.Parse(splitListStr[n][m]));
						}
						data.Action_Info.Add(kInfoData);
					}
				}

				strTmp = UtilManager.instance.SplitString("`", CExcelData_PATTERN_SUMMON_DATA.instance.PATTERN_DATABASE_GetTARGET_TYPE(m_SummonPatternDatas[nAiPatternId][i]));
				if (string.IsNullOrEmpty(strTmp) != true)
				{
					data.Target_Type = UtilManager.instance.SplitIntArr(",", strTmp);
				}

				strTmp = CExcelData_PATTERN_SUMMON_DATA.instance.PATTERN_DATABASE_GetTARGET_INFO(m_SummonPatternDatas[nAiPatternId][i]);
				if (string.IsNullOrEmpty(strTmp) != true)
				{
					List<List<string>> splitListStr;
					data.Target_Info = new List<List<float>>();
					splitListStr = UtilManager.instance.GetDataValueArr(strTmp);
					for (int n = 0; n < splitListStr.Count; ++n)
					{
						List<float> kInfoData = new List<float>();
						for (int m = 0; m < splitListStr[n].Count; ++m)
						{
							kInfoData.Add(float.Parse(splitListStr[n][m]));
						}
						data.Target_Info.Add(kInfoData);
					}
				}

				strTmp = UtilManager.instance.SplitString("`", CExcelData_PATTERN_SUMMON_DATA.instance.PATTERN_DATABASE_GetWARNING_ID(m_SummonPatternDatas[nAiPatternId][i]));
				if (string.IsNullOrEmpty(strTmp) != true)
				{
					data.Warning_Id = UtilManager.instance.SplitIntArr(",", strTmp);
				}

				data.Pattern_Skip = CExcelData_PATTERN_SUMMON_DATA.instance.PATTERN_DATABASE_GetPATTERN_SKIP(m_SummonPatternDatas[nAiPatternId][i]);

				data.Pattern_Cancleable = CExcelData_PATTERN_SUMMON_DATA.instance.PATTERN_DATABASE_GetPATTERN_CANCEL(m_SummonPatternDatas[nAiPatternId][i]);

				if (m_SummonAI_PatternDatas.ContainsKey(nAiPatternId))
				{
					bool bAddData = true;
					for (int j = 0; j < m_SummonAI_PatternDatas[nAiPatternId].Count; ++j)
					{
						if (m_SummonAI_PatternDatas[nAiPatternId][j].Page_Index == data.Page_Index &&
							m_SummonAI_PatternDatas[nAiPatternId][j].Action_Index == data.Action_Index)
						{
							bAddData = false;
							break;
						}
					}
					if (bAddData)
						m_SummonAI_PatternDatas[nAiPatternId].Add(data);
				}
				else
				{
					List<NpcAI_PatternInfo.Pattern_TableData> temList = new List<NpcAI_PatternInfo.Pattern_TableData>();
					temList.Add(data);
					m_SummonAI_PatternDatas.Add(nAiPatternId, temList);
				}
			}
		}
	}
	private void LoadDataNpcTimeTrigerInfo(uint nTimeTriggerId)
	{
		// List
		if (m_NpcTimeTriggerDatas.ContainsKey(nTimeTriggerId))
		{
			// Data
			for (int i = 0; i < m_NpcTimeTriggerDatas[nTimeTriggerId].Count; ++i)
			{
				NpcAI_TimeTrigerInfo.TimeTriger_TableData data = new NpcAI_TimeTrigerInfo.TimeTriger_TableData();
				data.Group_Index = CExcelData_TIME_TRIGER_DATA.instance.TIME_TRIGER_DATABASE_GetGROUP_INDEX(m_NpcTimeTriggerDatas[nTimeTriggerId][i]);
				data.Triger_ConDition = CExcelData_TIME_TRIGER_DATA.instance.TIME_TRIGER_DATABASE_GetTRIGER_CONDITION(m_NpcTimeTriggerDatas[nTimeTriggerId][i]);
				data.Condition_Value = CExcelData_TIME_TRIGER_DATA.instance.TIME_TRIGER_DATABASE_GetCONDITION_VALUE(m_NpcTimeTriggerDatas[nTimeTriggerId][i]);
				data.Event_Time = CExcelData_TIME_TRIGER_DATA.instance.TIME_TRIGER_DATABASE_GetEVETN_TIME(m_NpcTimeTriggerDatas[nTimeTriggerId][i]);
				data.Skip_Time = CExcelData_TIME_TRIGER_DATA.instance.TIME_TRIGER_DATABASE_GetSKIP_TIME(m_NpcTimeTriggerDatas[nTimeTriggerId][i]);
				data.Action_Index = CExcelData_TIME_TRIGER_DATA.instance.TIME_TRIGER_DATABASE_GetACTION_INDEX(m_NpcTimeTriggerDatas[nTimeTriggerId][i]);

				string strTmp = UtilManager.instance.SplitString("`", CExcelData_TIME_TRIGER_DATA.instance.TIME_TRIGER_DATABASE_GetACTION_TYPE(m_NpcTimeTriggerDatas[nTimeTriggerId][i]));
				if (string.IsNullOrEmpty(strTmp) != true)
				{
					data.Action_Type = UtilManager.instance.SplitIntArr(",", strTmp);
				}

				strTmp = CExcelData_TIME_TRIGER_DATA.instance.TIME_TRIGER_DATABASE_GetACTION_INFO(m_NpcTimeTriggerDatas[nTimeTriggerId][i]);
				if (string.IsNullOrEmpty(strTmp) != true)
				{
					List<List<string>> splitListStr;
					data.Action_Info = new List<List<float>>();
					splitListStr = UtilManager.instance.GetDataValueArr(strTmp);
					for (int n = 0; n < splitListStr.Count; ++n)
					{
						List<float> kInfoData = new List<float>();
						for (int m = 0; m < splitListStr[n].Count; ++m)
						{
							kInfoData.Add(float.Parse(splitListStr[n][m]));
						}
						data.Action_Info.Add(kInfoData);
					}
				}

				strTmp = UtilManager.instance.SplitString("`", CExcelData_TIME_TRIGER_DATA.instance.TIME_TRIGER_DATABASE_GetTARGET_TYPE(m_NpcTimeTriggerDatas[nTimeTriggerId][i]));
				if (string.IsNullOrEmpty(strTmp) != true)
				{
					data.Target_Type = UtilManager.instance.SplitIntArr(",", strTmp);
				}

				strTmp = CExcelData_TIME_TRIGER_DATA.instance.TIME_TRIGER_DATABASE_GetTARGET_INFO(m_NpcTimeTriggerDatas[nTimeTriggerId][i]);
				if (string.IsNullOrEmpty(strTmp) != true)
				{
					List<List<string>> splitListStr;
					data.Target_Info = new List<List<float>>();
					splitListStr = UtilManager.instance.GetDataValueArr(strTmp);
					for (int n = 0; n < splitListStr.Count; ++n)
					{
						List<float> kInfoData = new List<float>();
						for (int m = 0; m < splitListStr[n].Count; ++m)
						{
							kInfoData.Add(float.Parse(splitListStr[n][m]));
						}
						data.Target_Info.Add(kInfoData);
					}
				}

				strTmp = UtilManager.instance.SplitString("`", CExcelData_TIME_TRIGER_DATA.instance.TIME_TRIGER_DATABASE_GetWARNING_ID(m_NpcTimeTriggerDatas[nTimeTriggerId][i]));
				if (string.IsNullOrEmpty(strTmp) != true)
				{
					data.Warning_ID = UtilManager.instance.SplitIntArr(",", strTmp);
				}

				// save
				if (m_NpcAI_TimeTrigerDatas.ContainsKey(nTimeTriggerId))
				{
					m_NpcAI_TimeTrigerDatas[nTimeTriggerId].Add(data);
				}
				else
				{
					List<NpcAI_TimeTrigerInfo.TimeTriger_TableData> temList = new List<NpcAI_TimeTrigerInfo.TimeTriger_TableData>();
					temList.Add(data);
					m_NpcAI_TimeTrigerDatas.Add(nTimeTriggerId, temList);
				}
			}
		}
	}

	public void ClearLists()
	{
		if (m_NpcAI_PatternDatas != null)
		{
			m_NpcAI_PatternDatas.Clear();
		}
		if (m_SummonAI_PatternDatas != null)
		{
			m_SummonAI_PatternDatas.Clear();
		}

		if (m_NpcAI_TimeTrigerDatas != null)
		{
			m_NpcAI_TimeTrigerDatas.Clear();
		}
		if (m_npcProp != null)
		{
			m_npcProp.Clear();
		}
		if (m_SummonObjs != null)
		{
			m_SummonObjs.Clear();
		}
		if (m_NpcPatternListIndexs != null)
		{
			m_NpcPatternListIndexs.Clear();
		}

		if (m_NpcSpwanedList != null)
		{
			m_NpcSpwanedList.Clear();
		}
		if (m_monsterCharObjects != null)
		{
			m_monsterCharObjects.Clear();
		}

	}


	public void ClearAllNpcEffectObject()
	{
		List<CharacterBase> npcs = GetMonsterCharObjects();
		if (npcs != null)
		{
			for (int i = 0; i < npcs.Count; ++i)
			{
				npcs[i].m_EffectSkillManager.m_PlayingEffObjs.Clear();
			}
		}
	}

	public override void Process()
	{
		switch (m_eStatus)
		{
			case eSTATUS.ePREPARE:
                UnityEngine.Debug.Log("NpcManager Process eSTATUS.ePREPARE 1");
				Load();
                UnityEngine.Debug.Log("NpcManager Process eSTATUS.ePREPARE 2");
                m_eStatus = eSTATUS.eREADY;

                for (int i = 0; i < this.m_monsterCharObjects.Count; ++i)
                {
                    m_monsterCharObjects[i].gameObject.SetActive(true);
                }

                for (int i = 0; i < m_PlayerSummonObjs.Count; i++)
                {
                    if (m_PlayerSummonObjs[i] != null && m_PlayerSummonObjs[i].gameObject != null)
                    {
                        m_PlayerSummonObjs[i].gameObject.SetActive(true);
                    }
                }
                UnityEngine.Debug.Log("NpcManager Process eSTATUS.ePREPARE 3");
                break;
			case eSTATUS.eREADY:
                UnityEngine.Debug.Log("NpcManager Process eSTATUS.eREADY 1");
                if (m_monsterCharObjects != null)
                {
                    for (int m = 0; m < m_monsterCharObjects.Count; ++m)
                        m_monsterCharObjects[m].gameObject.SetActive(false);
                }
                for (int i = 0; i < m_PlayerSummonObjs.Count; i++)
                {
                    if (m_PlayerSummonObjs[i] != null)
                    {
                        if (m_PlayerSummonObjs[i].gameObject != null)
                        {
                            m_PlayerSummonObjs[i].gameObject.SetActive(false);
                        }
                    }
                }
                m_eStatus = eSTATUS.ePLAY;
                UnityEngine.Debug.Log("NpcManager Process eSTATUS.eREADY 2");
                break;
		}
	}

	public override void Destroy()
	{
		base.Destroy();
	}
    #endregion  OVERRIDE METHODS

    #region     LOAD METHODS

    public void Load()
    {
        List<NpcSpawnerData> npcSpawnerDataList = SpawnDataManager.instance.NpcSpawnerDataList;
        int nNpcSpawnerDataListCount = npcSpawnerDataList.Count;

        m_ID = PlayerManager.instance.m_PlayerInfo.Count + 1;

        //챕터 넘길때만 풀어줌
        if (m_bLoadNpcPrefab)
        {

            m_DropGoldObj = KSPlugins.KSResource.AssetBundleInstantiate<GameObject>("Drop/Gold/DropGold");

            m_DropGoldObj.SetActive(false);

            m_bLoadNpcPrefab = false;
        }
        if (SceneManager.instance.GetCurGameMode() != SceneManager.eGAMEMODE.eNONSYNC_PVP)
        {
            for (int i = 0; i < nNpcSpawnerDataListCount; i++)
            {
                LoadNpc(npcSpawnerDataList[i].m_nNpcCode, npcSpawnerDataList[i], i);
            }
        }
        ISummonAbilities a_iSummonAbilities = PlayerManager.instance.GetPlayerSummonAbilities();

        a_iSummonAbilities.ResetAllSummonConsumableAbility();

        if (m_PlayerSummonProp.Count == 0)
        {
            if (SceneManager.instance.GetCurGameMode() == SceneManager.eGAMEMODE.eNONSYNC_PVP)
            {
                for (int i = 0; i < m_PlayerSummonList.Count; i++)
                {
                    if (SummonManager.instance.GetMySummonEquipSlotIndex(m_PlayerSummonList[i].nContentIndex) > -1)
                    {
                        LoadPlayerSummon(m_PlayerSummonList[i], m_PlayerSummonList[i].m_PlayerSummonIconName, i);
                    }
                }
                LoadPvpGameobjectEquipSummon();
            }
            else
            {
                for (int i = 0; i < m_PlayerSummonList.Count; i++)
                {
                    LoadPlayerSummon(m_PlayerSummonList[i], m_PlayerSummonList[i].m_PlayerSummonIconName, i);
                }

                LoadGameobjectEquipSummon();
                a_iSummonAbilities.SendSummonAbillityInfo();
                //>
            }
        }
        else
        {
            for (int i = 0; i < m_PlayerSummonList.Count; i++)
            {
                if (m_PlayerSummonProp[i].Summon_Check_Die)
                {
                    m_PlayerSummonProp[i].Summon_Check_Die = false;
                    m_PlayerSummonObjs[m_PlayerSummonProp[i].Summon_EquipObjIndex].SetCharacterState(CHAR_STATE.ALIVE);
                }
            }
        }
        if (SceneManager.instance.GetCurGameMode() == SceneManager.eGAMEMODE.eNONSYNC_PVP)
        {
            ISummonAbilities a_iEnemySummonAbilities = PlayerManager.instance.GetEnemySummonAbilities();

            a_iEnemySummonAbilities.ResetAllSummonConsumableAbility();

            for (int i = 0; i < RaidManager.instance.m_NonSyncPvpPlayerPets.Count; ++i)
            {
                LoadPvpPet(RaidManager.instance.m_NonSyncPvpPlayerPets[i], (int)m_SummonIndexDic[(int)RaidManager.instance.m_NonSyncPvpPlayerPets[i].m_PlayerSummonIndex], true, a_iEnemySummonAbilities, i);
            }

            PartyMemberAbilities a_kPartyMemberAbilities = PlayerManager.instance.GetPartyMemberAbilities(PlayerManager.PVPPLAYER_INDEX);
            a_kPartyMemberAbilities.SetPVPPartyAffectOption();
            a_iEnemySummonAbilities.ReloadAllSummonAbility();

        }
        else
        {
            NpcSpawnGroupInfo npcSpawnGroupInfo = null;
            m_NpcSpawnGroupInfos.Clear();
            int nGroupIndex = 0;
            for (int i = 0; i < nNpcSpawnerDataListCount; i++)
            {
                nGroupIndex = SpawnDataManager.instance.NpcSpawnerDataList[i].m_nNpcSpawnerGroup;
                if (m_monsterCharObjects[i].m_CharAi != null)
                {
                    m_monsterCharObjects[i].m_CharAi.nUniqueID = SpawnDataManager.instance.NpcSpawnerDataList[i].m_nSpawnerID;
                    ((NpcAI)m_monsterCharObjects[i].m_CharAi).nNpcGroup = nGroupIndex;
                    ((NpcAI)m_monsterCharObjects[i].m_CharAi).nFreeActivityRadius = SpawnDataManager.instance.NpcSpawnerDataList[i].m_nFreeActivityRadius_cm;
                    ((NpcAI)m_monsterCharObjects[i].m_CharAi).v3SourcePosition = new Vector3(SpawnDataManager.instance.NpcSpawnerDataList[i].m_v3Position.x, SpawnDataManager.instance.NpcSpawnerDataList[i].m_v3Position.y + 0.5f, SpawnDataManager.instance.NpcSpawnerDataList[i].m_v3Position.z);
                }
                if (m_NpcSpawnGroupInfos.ContainsKey(nGroupIndex) == true)
                {
                    m_NpcSpawnGroupInfos[nGroupIndex].m_NpcObjects.Add(m_monsterCharObjects[i]);
                }
                else
                {
                    npcSpawnGroupInfo = new NpcSpawnGroupInfo();
                    npcSpawnGroupInfo.m_NpcObjects = new List<CharacterBase>();
                    npcSpawnGroupInfo.Group_Index = nGroupIndex;
                    npcSpawnGroupInfo.m_NpcObjects.Add(m_monsterCharObjects[i]);

                    //Debug.Log("NpcManager.Load() Adding SpawnerGroupInfos idx = "+nGroupIndex);
                    m_NpcSpawnGroupInfos.Add(nGroupIndex, npcSpawnGroupInfo);
                }
            }
        }
        //UnityEngine.Debug.Log("NpcManager Process Load() 6");
        //SpawnDataManager.instance.m_WayPointsDic.Add()

        //< modify ggango 2017.11.13
        a_iSummonAbilities.ReloadAllSummonAbility();
        //>
        //UnityEngine.Debug.Log("NpcManager Process Load() 7");
        if (SceneManager.instance.GetCurGameMode() != SceneManager.eGAMEMODE.eNONSYNC_PVP)
        {
            PlayerManager.instance.SetPlayerAttackPower();
        }
        //UnityEngine.Debug.Log("NpcManager Process Load() 8");
    }

    public void CleanUpBoss()
    {
        NpcBoss = null;
    }


	GameObject LoadPrefabObj(string path, NpcSpawnerData arg, NpcInfo.NpcProp prop, NpcInfo.NpcModelList NpcModeList, bool bSummon, bool bMonsterListAdd)
	{
		//Vector3 tmpRot = new Vector3();
		CharacterBase tmpNpc = null;
		NpcAI tmpNpcAI = null;
		//SkillSender tmpSender = null;
		AnimationInterface tmpAniInter = null;
		NavMeshAgent tmpAgent;

		string tmpPath = path;
		string tmpName = NpcModeList.Model_Prefabs_Name;
		string tmpStr = tmpPath;

		GameObject tmpObj = KSPlugins.KSResource.AssetBundleInstantiate<GameObject>(tmpStr);
		UtilManager.instance.ResetShader(tmpObj);


		tmpNpc = tmpObj.AddComponent<CharacterBase>();

        tmpNpc.SetCharType(eCharacter.eNPC);

        tmpNpc.m_CharAi = tmpNpcAI = tmpObj.AddComponent<NpcAI>();
		tmpObj.AddComponent<SkillSender>();
		tmpAniInter = tmpObj.AddComponent<AnimationInterface>();
		tmpObj.AddComponent<DamageUI>();

		tmpAniInter.effectSkillManager = tmpObj.AddComponent<EffectSkillManager>();
		tmpAniInter.effectSkillManager.gobjCharacter = tmpObj;
		tmpAgent = tmpObj.GetComponent<NavMeshAgent>();


		if (m_npcRootObj != null)
			tmpObj.transform.parent = m_npcRootObj.transform;
		else
			tmpObj.transform.parent = null;

		tmpObj.transform.localPosition = arg.m_v3Position;//.point;
		tmpObj.transform.localEulerAngles = arg.m_v3EulerAngles;//.eulerAngles;
		tmpObj.transform.localScale = new Vector3(prop.Npc_Scale, prop.Npc_Scale, prop.Npc_Scale);
		tmpNpc.m_nSpawnerId = arg.m_nSpawnerID;
		tmpNpc.charUniqID = m_ID++;
		tmpNpc.SetTeam(1);



		tmpAgent.enabled = true;

		tmpNpc.SetCharEffectBone(tmpObj.transform);

		prop.SpawnEffResInfo = SkillDataManager.instance.GetEffectResInfoByEffID(prop.SpawnEff);
#if PLANE_SHADOW
        List<Material> Shadowmats = new List<Material>();
        Renderer[] Renderers = tmpObj.GetComponentsInChildren<Renderer>();
        int nCount = Renderers.Length;

        for (int i = 0; i < nCount; ++i)
        {
            int iMatCount = Renderers[i].materials.Length;

            Material[] mats = new Material[iMatCount + 1];

            for (int j = 0; j < iMatCount; ++j)
            {
                mats[j] = Renderers[i].materials[j];
            }
            mats[iMatCount] = new Material(Shader.Find("Blame/PlaneShadow"));
            Shadowmats.Add(mats[iMatCount]);

            Renderers[i].sharedMaterials = mats;

            PlaneShadow Shadow = Renderers[i].gameObject.AddComponent<PlaneShadow>();
            Shadow.m_transform = tmpNpc.m_MyObj;
            Shadow.m_Mat = Shadowmats;
        }
#endif
		Renderer[] renderer = tmpObj.GetComponentsInChildren<Renderer>();

		for (int i = 0; i < renderer.Length; ++i)
		{
			renderer[i].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
			renderer[i].receiveShadows = false;

			renderer[i].useLightProbes = false;
			renderer[i].reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
			for (int j = 0; j < renderer[i].sharedMaterials.Length; ++j)
			{
				if (renderer[i].sharedMaterials[j] != null)
				{
					tmpNpc.m_preFabRenderer.Add(renderer[i]);

					break;
				}
			}
		}

		if (bMonsterListAdd == true)
		{
			m_monsterCharObjects.Add(tmpNpc);

//#if UNITY_EDITOR
//            Debug.LogError("NPC Class = " + prop.Npc_Class);
//#endif
            if (prop.Npc_Class == eNPC_CLASS.eBOSS)
            {
                NpcBoss = tmpNpc;
            }
        }

		if (prop.Ai_Pattern_id > 0)
		{
			LoadDataNpcPatternInfo(prop.Ai_Pattern_id);       // 패턴 
		}
		if (prop.Time_Trigger_id > 0)
		{
			LoadDataNpcTimeTrigerInfo(prop.Time_Trigger_id);  // 타임 트리거
		}

		//InGameManager.AddShadow(tmpObj);

		tmpNpcAI.Init(eAIType.eNPC, tmpNpc.OnAiEventEnd);
		tmpNpcAI.SetCharacter_Npc(prop,
								NpcModeList,
								prop.Ai_Pattern_id > 0 ? m_NpcAI_PatternDatas[prop.Ai_Pattern_id] : null,
								prop.Time_Trigger_id > 0 ? m_NpcAI_TimeTrigerDatas[prop.Time_Trigger_id] : null);



		if (bSummon)
		{
			m_SummonObjs.Add(tmpNpc);
		}


		//tmpNpc.SetCharType(eCharacter.eNPC);

		tmpNpc.InitAttackInfos();

		tmpNpc.m_nSpawnerId = arg.m_nSpawnerID;

		tmpObj.SetActive(false);

		return tmpObj;
	}


	public GameObject LoadPrefabObjPublic(string path, NpcSpawnerData arg, NpcInfo.NpcProp prop, NpcInfo.NpcModelList NpcModeList, bool bSummon, bool bMonsterListAdd)
	{
		return LoadPrefabObj(path, arg, prop, NpcModeList, bSummon, bMonsterListAdd);
	}

	private void LoadBaricadePrefabObj(string path, int idx, NpcInfo.NpcModelList NpcModeList, NpcSpawnerData npcSpawnerInfo)
	{
		/// 어셋번들에서 불러오도록 변경 ksk
		/// [
		GameObject gobjBarricade = KSPlugins.KSResource.AssetBundleInstantiate<GameObject>(path);
		UtilManager.instance.ResetShader(gobjBarricade);
		//GameObject gobjBarricade = GameObject.Instantiate(Resources.Load(path)) as GameObject;
		/// ]
		//gobjBarricade.name = "OBJ_Barricade" + idx.ToString("00");
		gobjBarricade.tag = "Col";
		gobjBarricade.transform.position = SpawnDataManager.instance.NpcSpawnerDataList[idx].m_v3Position;
		gobjBarricade.transform.eulerAngles = SpawnDataManager.instance.NpcSpawnerDataList[idx].m_v3EulerAngles;
		gobjBarricade.transform.localScale = SpawnDataManager.instance.NpcSpawnerDataList[idx].m_v3LocalScale;
		CharacterBase characterBase = gobjBarricade.AddComponent<CharacterBase>();
		characterBase.enabled = false;
		m_monsterCharObjects.Add(characterBase);

		characterBase.m_nSpawnerId = npcSpawnerInfo.m_nSpawnerID;
	}
	public void ReLoadAI(CharacterBase p_Npc)
	{
		((NpcAI)p_Npc.m_CharAi).SetCharacter_NpcAI(((NpcAI)p_Npc.m_CharAi).m_NpcProp.Ai_Pattern_id > 0 ? m_NpcAI_PatternDatas[((NpcAI)p_Npc.m_CharAi).m_NpcProp.Ai_Pattern_id] : null,
										  ((NpcAI)p_Npc.m_CharAi).m_NpcProp.Time_Trigger_id > 0 ? m_NpcAI_TimeTrigerDatas[((NpcAI)p_Npc.m_CharAi).m_NpcProp.Time_Trigger_id] : null);

	}
	#endregion  LOAD METHODS

	#region FIELD
	void CreateNpcRootObject()
	{
		Vector3 tmpPos = new Vector3(0, 0, 0);
		if (m_npcRootObj == null || m_rootObj.transform.FindChild("NPC") == null)
		{
			m_npcRootObj = new GameObject("NPC");
		}
		m_npcRootObj.transform.parent = m_rootObj.transform;

		if (SceneManager.instance.GetCurGameMode() == SceneManager.eGAMEMODE.eNPC_TEST_TOOL)
		{
			m_npcRootObj.transform.localPosition = new Vector3(-62.03f, -6.23f, 32.82f);
		}
		else
		{
			m_npcRootObj.transform.localPosition = tmpPos;
		}
	}

	public void NpcSpwanedListAdd(CharacterBase cBase)
	{
		m_NpcSpwanedList.Add(cBase);
	}

	public void NpcSpwanedListRemove(CharacterBase cBase)
	{
		if (m_NpcSpwanedList != null)
			m_NpcSpwanedList.Remove(cBase);
	}
	#endregion FIELD

	#region AI

	public void DiePlayer(GameObject p_Source)
	{
		if (m_NpcSpwanedList != null)
		{
			for (int i = 0; i < m_NpcSpwanedList.Count; ++i)
			{
				if (m_NpcSpwanedList[i].m_CharAi != null)
					((NpcAI)m_NpcSpwanedList[i].m_CharAi).DisperseAggro(p_Source, 0);
			}
		}
	}
	public void AlarmGroup(CharacterBase p_NpcChar)
	{
		if (m_NpcSpawnGroupInfos != null)
		{
			if (NpcManager.instance.NpcBoss != p_NpcChar)
			{
				for (int i = 0; i < m_NpcSpawnGroupInfos[((NpcAI)p_NpcChar.m_CharAi).nNpcGroup].m_NpcObjects.Count; ++i)
				{
					if (m_NpcSpawnGroupInfos[((NpcAI)p_NpcChar.m_CharAi).nNpcGroup].m_NpcObjects[i].chrState == CHAR_STATE.ALIVE && p_NpcChar.m_CharAi.GetNpcProp().Battle_Type == eNPC_BATTLE_TYPE.eBATTLE)
						m_NpcSpawnGroupInfos[((NpcAI)p_NpcChar.m_CharAi).nNpcGroup].m_NpcObjects[i].SetAttackTarget(p_NpcChar.attackTarget);
				}
			}
			else
			{
				var enumerator = m_NpcSpawnGroupInfos.GetEnumerator();
				while (enumerator.MoveNext())
				{
					//Dictionary<int, NpcSpawnGroupInfo> GroupData = enumerator.Current;
					int GroupData = enumerator.Current.Key;
					if (((NpcAI)p_NpcChar.m_CharAi).nNpcGroup != GroupData)
					{
						for (int i = 0; i < m_NpcSpawnGroupInfos[GroupData].m_NpcObjects.Count; ++i)
						{
							if (m_NpcSpawnGroupInfos[GroupData].m_NpcObjects[i].chrState == CHAR_STATE.ALIVE &&
								m_NpcSpawnGroupInfos[GroupData].m_NpcObjects[i].m_CharAi != null &&
								m_NpcSpawnGroupInfos[GroupData].m_NpcObjects[i].m_CharAi.GetNpcProp().Battle_Type == eNPC_BATTLE_TYPE.eBATTLE)
								m_NpcSpawnGroupInfos[GroupData].m_NpcObjects[i].SetAttackTarget(p_NpcChar.attackTarget);
						}
					}
				}


			}
		}
	}

	public bool GetCheckAllNpcDied()
	{
		List<CharacterBase> npcs = NpcManager.instance.GetMonsterCharObjects();

		for (int i = 0; i < npcs.Count; ++i)
		{
			NpcAI npcai = ((NpcAI)npcs[i].m_CharAi);
			if (npcai != null)
			{
				//if (npcai.GetNpcProp().Npc_Class != eNPC_CLASS.eBOSS)
				{
					if (npcs[i].m_CharState == CHAR_STATE.ALIVE)
					{
						return false;
					}
				}
			}
		}
		return true;
	}

	#endregion AI

	public void SetNpcActionStop()
	{
		for (int i = 0; i < GetNpcTargetsBySpwaned().Count; ++i)
		{
			CharacterBase cBase = GetNpcTargetsBySpwaned()[i];
			if (cBase.m_CharAi != null)
			{
				NpcInfo.NpcProp npcProp = cBase.m_CharAi.GetNpcProp();
				if (npcProp != null)
				{
					if (npcProp.Npc_Type == eNPC_TYPE.eMONSTER)
					{
						//cBase.m_CharState = CHAR_STATE.DEATH;
						((NpcAI)cBase.m_CharAi).SetStopState();
						cBase.SetChangeMotionState(MOTION_STATE.eMOTION_IDLE);
						//cBase.ksAnimation.GetAnimation(cBase.ksAnimation.GetCurrentClipName()).speed = 0;
					}
				}
			}
		}
	}


	public void SetNpcActionPlay()
	{
		for (int i = 0; i < GetNpcTargetsBySpwaned().Count; ++i)
		{
			CharacterBase cBase = GetNpcTargetsBySpwaned()[i];
			if (cBase.m_CharAi != null)
			{
				NpcInfo.NpcProp npcProp = cBase.m_CharAi.GetNpcProp();
				if (npcProp != null)
				{
					if (npcProp.Npc_Type == eNPC_TYPE.eMONSTER)
					{
						cBase.m_CharState = CHAR_STATE.ALIVE;
						((NpcAI)cBase.m_CharAi).SetPeaceState();
						//cBase.ksAnimation.GetAnimation(cBase.ksAnimation.GetCurrentClipName()).speed = 0;
					}
				}
			}
		}
	}


	public void SelDeleteDamageUI()
	{
		for (int i = 0; i < GetNpcTargetsBySpwaned().Count; ++i)
		{
			CharacterBase cBase = GetNpcTargetsBySpwaned()[i];
			if (cBase.m_MyObj != null)
			{
				HPbar uiComp = cBase.m_MyObj.GetComponent<HPbar>();
				if (uiComp != null)
				{
					uiComp.TurnOff();
				}
			}
		}
	}

	public bool CheckAllDieInSpawnGroup(int groupID)
	{
		for (int i = 0; i < m_NpcSpawnGroupInfos[groupID].m_NpcObjects.Count; ++i)
		{
			if (m_NpcSpawnGroupInfos[groupID].m_NpcObjects[i].m_CharState == CHAR_STATE.ALIVE)
			{
				return false;
			}
		}
		return true;
	}


	public void SetAllNpcDie()
	{
		for (int i = 0; i < m_NpcSpwanedList.Count; ++i)
		{
			NpcAI npcAi = m_NpcSpwanedList[i].m_MyObj.GetComponent<NpcAI>();

			if (npcAi != null)
			{
				if (m_NpcSpwanedList[i].m_CharState != CHAR_STATE.DEATH)
				{
					npcAi.m_ActionController.RemoveAllAction();
				}
				m_NpcSpwanedList[i].damageCalculationData.fCURRENT_HIT_POINT = 0;
				m_NpcSpwanedList[i].CheckDie(DIE_TYPE.eDIE_NORMAL);
			}

		}
	}

	public void SetHeroNpcActionRemove()
	{
		ISummonAbilities a_iSummonAbilities = PlayerManager.instance.GetPlayerSummonAbilities();
		List<SummonAbility> a_listSummonAbility = a_iSummonAbilities.GetEquipSummonAbility();

		for (int i = 0; i < a_listSummonAbility.Count; ++i)
		{
			HeroNpcAI _ai = (HeroNpcAI)(a_listSummonAbility[i].SummonData.summonCBase.m_CharAi);

			if (_ai != null)
			{
				_ai.m_ActionController.RemoveAllAction();
			}
		}
	}

	private void SetNpcPrefabInfo(int themeIdx, string route, string pName)
	{
		string modelPath = route + "/" + pName;

		if (m_NpcModelInfoByThemeDic.ContainsKey(themeIdx) == false)
		{
			List<string> infoList = new List<string>();

			infoList.Add(modelPath);

			m_NpcModelInfoByThemeDic.Add(themeIdx, infoList);
		}
		else
		{
			m_NpcModelInfoByThemeDic[themeIdx].Add(modelPath);
		}
	}
	

	public void SetSummonAllStop(bool SummonObj)
	{
		if (m_PlayerSummonObjs != null)
		{
			for (int i = 0; i < m_PlayerSummonObjs.Count; i++)
			{
				if (m_PlayerSummonObjs[i].m_CharAi != null)
				{
					if (m_PlayerSummonObjs[i].m_CharAi.GetNpcProp().Summon_EquipIndex > -1 && m_PlayerSummonObjs[i].m_CharAi.GetNpcProp().Summon_EquipSlotGroupIndex == 1)
					{
						if (m_PlayerSummonObjs[i].m_MyObj.gameObject.activeSelf == true)
						{
							if (m_PlayerSummonObjs[i].m_CharState != CHAR_STATE.DEATH)
							{
								((HeroNpcAI)m_PlayerSummonObjs[i].m_CharAi).SetStopState();
								m_PlayerSummonObjs[i].m_CharacterDamageEffect.Spawn_RecoveryMaterial();
								m_PlayerSummonObjs[i].SetChangeMotionState(MOTION_STATE.eMOTION_IDLE);
								m_PlayerSummonObjs[i].m_MyObj.GetComponent<HPbar>().JustActive(false);
								if (SummonObj)
								{
									m_PlayerSummonObjs[i].m_MyObj.gameObject.SetActive(false);
									m_PlayerSummonObjs[i].m_MyObj.GetComponent<HPbar>().JustActive(false);
								}
							}
						}
						if (m_PlayerSummonObjs[i].m_CharState == CHAR_STATE.DEATH)
						{
							((HeroNpcAI)m_PlayerSummonObjs[i].m_CharAi).m_DieVar.m_bGowayStart = false;

							m_PlayerSummonObjs[i].m_CharAi.GetNpcProp().Summon_Check_Die = true;
							//쉐이더 다시 돌림
							m_PlayerSummonObjs[i].m_CharacterDamageEffect.Spawn_RecoveryMaterial();

							m_PlayerSummonObjs[i].m_MyObj.gameObject.SetActive(false);
							m_PlayerSummonObjs[i].m_MyObj.GetComponent<HPbar>().JustActive(false);
						}
					}
				}
			}
		}
	}

	public void SetSummonAllEffect_Init()
	{
		if (m_PlayerSummonObjs != null)
		{
			for (int i = 0; i < m_PlayerSummonObjs.Count; i++)
			{
				if (m_PlayerSummonObjs[i].m_CharAi.GetNpcProp().Summon_EquipIndex > -1 && m_PlayerSummonObjs[i].m_CharAi.GetNpcProp().Summon_EquipSlotGroupIndex == 1)
				{
					if (m_PlayerSummonObjs[i].m_MyObj.gameObject.activeSelf == true)
					{
						NpcInfo.NpcProp npcProp = m_PlayerSummonObjs[i].m_CharAi.GetNpcProp();

						for (int j = 0; j < m_PlayerSummonObjs[i].m_AttackInfos[m_PlayerSummonObjs[i].m_currSkillIndex].skillinfo.skill_Effect.Count; j++)
						{
							m_PlayerSummonObjs[i].m_EffectSkillManager.StopPlayingEffect(m_PlayerSummonObjs[i].m_AttackInfos[m_PlayerSummonObjs[i].m_currSkillIndex].skillinfo.skill_Effect[j]);
						}

					}
				}
			}
		}
	}
	public void SetSummonEffect_Init(CharacterBase charbase)
	{
		if (charbase != null)
		{
			if (charbase.m_MyObj.gameObject.activeSelf == true)
			{
				NpcInfo.NpcProp npcProp = charbase.m_CharAi.GetNpcProp();

				for (int j = 0; j < charbase.m_AttackInfos[charbase.m_currSkillIndex].skillinfo.skill_Effect.Count; j++)
				{
					charbase.m_EffectSkillManager.StopPlayingEffect(charbase.m_AttackInfos[charbase.m_currSkillIndex].skillinfo.skill_Effect[j]);
				}

			}

		}
	}
	public void SetSummonAllWorkReady()
	{
		if (m_PlayerSummonObjs != null)
		{
			for (int i = 0; i < m_PlayerSummonObjs.Count; i++)
			{
				m_PlayerSummonObjs[i].damageCalculationData.fCURRENT_HIT_POINT = m_PlayerSummonObjs[i].damageCalculationData.fMAX_HIT_POINT;

				if (m_PlayerSummonObjs[i].m_CharAi.GetNpcProp().Summon_EquipIndex > -1 && m_PlayerSummonObjs[i].m_CharAi.GetNpcProp().Summon_EquipSlotGroupIndex == 1)
				{
					InGameManager.instance.UiSetEquipSummon(m_PlayerSummonObjs[i].m_CharAi.GetNpcProp(), m_PlayerSummonObjs[i].m_CharAi.GetNpcProp().Summon_EquipIndex);
					if (m_PlayerSummonObjs[i].m_CharAi.GetNpcProp().Summon_Check_Die == true)
					{
						m_PlayerSummonObjs[i].m_CharAi.GetNpcProp().Summon_Check_Die = false;
						InGameManager.instance.m_uiSummon.m_SummonInfo.SetSummonCheckDie(m_PlayerSummonObjs[i].m_CharAi.GetNpcProp().Summon_nContentIndex, false);
						SummonRevival(m_PlayerSummonObjs[i]);
					}
				}
			}
		}
	}
	public void SetSummonAllWork(bool BossSceneEvent)
	{
		if (m_PlayerSummonObjs != null)
		{
			for (int i = 0; i < m_PlayerSummonObjs.Count; i++)
			{
				if (m_PlayerSummonObjs[i].m_CharAi.GetNpcProp().Summon_EquipIndex > -1 && m_PlayerSummonObjs[i].m_CharAi.GetNpcProp().Summon_EquipSlotGroupIndex == 1)
				{
					if (BossSceneEvent)
					{
						if (m_PlayerSummonObjs[i].m_CharState != CHAR_STATE.DEATH)
						{
							if (m_PlayerSummonObjs[i].m_MyObj.gameObject.activeSelf == true)
							{
								m_PlayerSummonObjs[i].SetCharacterState(CHAR_STATE.ALIVE);
								((HeroNpcAI)m_PlayerSummonObjs[i].m_CharAi).SetPeaceState();
								m_PlayerSummonObjs[i].m_hpBar.JustActive(true);
							}
						}
					}
					else
					{
						if (m_PlayerSummonObjs[i].m_MyObj.gameObject.activeSelf == true)
						{
							m_PlayerSummonObjs[i].SetCharacterState(CHAR_STATE.ALIVE);
							((HeroNpcAI)m_PlayerSummonObjs[i].m_CharAi).SetPeaceState();
						}
					}
				}
			}
		}
	}

	public void SetSummonWork(CharacterBase kCharacterBase)
	{
		if (kCharacterBase != null)
		{
			if (kCharacterBase.m_CharAi.GetNpcProp().Summon_EquipIndex > -1 && kCharacterBase.m_CharAi.GetNpcProp().Summon_EquipSlotGroupIndex == 1)
			{
				if (kCharacterBase.m_MyObj.gameObject.activeSelf == true)
				{
					kCharacterBase.SetCharacterState(CHAR_STATE.ALIVE);
					((HeroNpcAI)kCharacterBase.m_CharAi).SetPeaceState();
				}
			}
		}
	}

	public void SummonRevival(CharacterBase charBase)
	{
		Vector3 pos = new Vector3(0, 0, 0);
		if (charBase.m_MyObj.gameObject.activeSelf == false)
		{
			charBase.m_MyObj.gameObject.SetActive(true);
		}

		pos.x = PlayerManager.instance.GetMyPlayerInfo().playerCharBase.gameObject.transform.localPosition.x + ((charBase.m_CharAi.GetNpcProp().Summon_EquipIndex + 1) * 1);
		pos.y = PlayerManager.instance.GetMyPlayerInfo().playerCharBase.gameObject.transform.localPosition.y;
		pos.z = PlayerManager.instance.GetMyPlayerInfo().playerCharBase.gameObject.transform.localPosition.z + ((charBase.m_CharAi.GetNpcProp().Summon_EquipIndex + 1) * 1);
		charBase.m_MyObj.transform.localPosition = pos;

		pos.x = PlayerManager.instance.GetMyPlayerInfo().playerCharBase.gameObject.transform.position.x + ((charBase.m_CharAi.GetNpcProp().Summon_EquipIndex + 1) * 1);
		pos.y = PlayerManager.instance.GetMyPlayerInfo().playerCharBase.gameObject.transform.position.y;
		pos.z = PlayerManager.instance.GetMyPlayerInfo().playerCharBase.gameObject.transform.position.z + ((charBase.m_CharAi.GetNpcProp().Summon_EquipIndex + 1) * 1);
		charBase.m_MyObj.transform.position = pos;

		charBase.m_BuffController.DelBuffCondition(BuffController.eBuffDeleteCondition.eDEAD);
		BuffData_ReAction kReAction = charBase.m_BuffController.FindFrontReActionBuff();
		charBase.m_BuffController.BuffEnd(kReAction);
		((HeroNpcAI)charBase.m_CharAi).SetRevival();

		((HeroNpcAI)charBase.m_CharAi).SetNpcState(HeroNpcAI.eNpcState.eSPWAN_STANDBY);
		((HeroNpcAI)charBase.m_CharAi).m_ActionController.Init();
		((HeroNpcAI)charBase.m_CharAi).SetAutoPlayState(HeroNpcAI.eAutoProcessState.eReady);

		charBase.SetChangeMotionState(MOTION_STATE.eMOTION_SPWAN);
		charBase.SetChangeMotionState(MOTION_STATE.eMOTION_IDLE);
	}

	public void SetSummonStop(CharacterBase charBase)
	{
		if (charBase.m_MyObj.gameObject.activeSelf == true)
		{

			charBase.m_BuffController.DelBuffCondition(BuffController.eBuffDeleteCondition.eDEAD);

			BuffData_ReAction kReAction = charBase.m_BuffController.FindFrontReActionBuff();
			if (kReAction != null)
				charBase.m_BuffController.BuffEnd(kReAction);
			((HeroNpcAI)charBase.m_CharAi).SetStopState();
			SetSummonEffect_Init(charBase);
			charBase.m_MyObj.GetComponent<HPbar>().JustActive(false);
			charBase.m_MyObj.gameObject.SetActive(false);
		}
	}

	public void PetMenuStartCoroutine(Animation animation, string clipName, bool useTimeScale)
	{
		StartCoroutine(NpcManager.instance.Play(animation, clipName, useTimeScale));
	}
	public void PetMenuStopCoroutine()
	{
		if (m_coroutine_is_running)
		{
			StopCoroutine("Play");
			m_coroutine_is_running = false;
		}

	}
	public IEnumerator Play(Animation animation, string clipName, bool useTimeScale)
	{
		if (!useTimeScale)
		{
			m_coroutine_is_running = true;
			AnimationState _currState = animation[clipName];
			bool isPlaying = true;
			float _progressTime = 0F;
			float _timeAtLastFrame = 0F;

			animation.Play(clipName);

			_timeAtLastFrame = Time.realtimeSinceStartup;
			while (isPlaying)
			{
				if (_currState == null)
				{
					break;
				}
				_progressTime += Time.unscaledDeltaTime;// deltaTime;
				_currState.normalizedTime = _progressTime / _currState.length;

				if (_progressTime >= _currState.length)
				{
					if (_currState.wrapMode != WrapMode.Loop)
					{
						isPlaying = false;
					}
					else
					{
						_progressTime = 0.0f;
					}
				}

				yield return new WaitForEndOfFrame();
			}
			yield return null;
			m_coroutine_is_running = false;
		}
		else
		{
			animation.Play(clipName);
		}
	}


#if NONSYNC_PVP
	private Vector3 GetLookAtPosition(bool bMe)
	{
		int spawnID = 1;

		if (bMe)
		{
			spawnID = 2;
		}

		for (int i = 0; i < SpawnDataManager.instance.HeroSpawnerDataList.Count; ++i)
		{
			if (spawnID == SpawnDataManager.instance.HeroSpawnerDataList[i].m_nHeroSpawnerID)
			{
				return SpawnDataManager.instance.HeroSpawnerDataList[i].m_v3Position;
			}
		}

		return SpawnDataManager.instance.HeroSpawnerDataList[0].m_v3Position;
	}


	private bool CheckCanUseSpawnIndex(int idx)
	{
		for (int i = 0; i < m_SpawnPositionIndex.Count; ++i)
		{
			if (idx == m_SpawnPositionIndex[i])
			{
				return false;
			}
		}
		return true;
	}

	private Vector3 GetSpawnPosition(int groupNum)
	{
		for (int i = 0; i < SpawnDataManager.instance.NpcSpawnerDataList.Count; ++i)
		{
			if (groupNum == SpawnDataManager.instance.NpcSpawnerDataList[i].m_nNpcSpawnerGroup)
			{
				if (CheckCanUseSpawnIndex(i))
				{
					m_SpawnPositionIndex.Add(i);
					return SpawnDataManager.instance.NpcSpawnerDataList[i].m_v3Position;
				}
			}
		}
		return Vector3.zero;
	}
#endif

	private Vector3 GetPvPSummonSpawnPosition(int groupNum, NpcInfo.NpcProp kSummonProp)
	{
		for (int i = 0; i < SpawnDataManager.instance.NpcSpawnerDataList.Count; ++i)
		{
			if (groupNum == SpawnDataManager.instance.NpcSpawnerDataList[i].m_nNpcSpawnerGroup)
			{
				int SpawnID = SummonTypeSpawnID((int)kSummonProp.Summon_Type, kSummonProp.Summon_EquipIndex);
				if (SpawnDataManager.instance.NpcSpawnerDataList[i].m_nSpawnerID == SpawnID)
				{
					return SpawnDataManager.instance.NpcSpawnerDataList[i].m_v3Position;
				}
			}
		}

		return Vector3.zero;
	}
	private int SummonTypeSpawnID(int kSummonTypeIndex, int EquipSlotIndex)
	{
		int SpawnID = 0;
		int SummonTypeID = 0;
		int SummonEquipIndex = EquipSlotIndex + 1;
		switch (kSummonTypeIndex)
		{
			//공격형, 파이터형
			case 1:
			case 4:
				SummonTypeID = 1;
				break;
			//방어형
			case 2:
				SummonTypeID = 0;
				break;
			//원거리형
			case 3:
				SummonTypeID = 2;
				break;
		}
		return SpawnID = (SummonEquipIndex * 10) + SummonTypeID;
	}
	
	


#if NONSYNC_PVP

    public bool m_bPutPvpPet = false;
	public void SetPvpPlayerPets()
	{
		for (int i = 0; i < RaidManager.instance.m_NonSyncPvpPlayerPets.Count; ++i)
		{
			//Debug.Log(i+"th SetPvpPlayerPets() name = "+ RaidManager.instance.m_NonSyncPvpPlayerPets[i].summonCBase+ "/ charUniqID = " + RaidManager.instance.m_NonSyncPvpPlayerPets[i].summonCBase.charUniqID);
			RaidManager.instance.m_NonSyncPvpPlayerPets[i].summonCBase.damageCalculationData.fCURRENT_HIT_POINT = RaidManager.instance.m_NonSyncPvpPlayerPets[i].summonCBase.damageCalculationData.fMAX_HIT_POINT;
			RaidManager.instance.m_NonSyncPvpPlayerPets[i].summonCBase.m_CharAi.GetNpcProp().Summon_Check_Die = false;
			RaidManager.instance.m_NonSyncPvpPlayerPets[i].summonCBase.m_MyObj.gameObject.SetActive(true);

			//RaidManager.instance.m_NonSyncPvpPlayerPets[i].summonCBase.m_MyObj.transform.localPosition = GetSpawnPosition(2);
			NpcInfo.NpcProp EnemySummyProp = RaidManager.instance.m_NonSyncPvpPlayerPets[i].summonCBase.m_CharAi.GetNpcProp();
			RaidManager.instance.m_NonSyncPvpPlayerPets[i].summonCBase.m_MyObj.transform.localPosition = GetPvPSummonSpawnPosition(2, EnemySummyProp);

			Vector3 lookPos = GetLookAtPosition(false);
			RaidManager.instance.m_NonSyncPvpPlayerPets[i].summonCBase.LookAtY(lookPos);

			RaidManager.instance.m_NonSyncPvpPlayerPets[i].summonCBase.m_BuffController.DelBuffCondition(BuffController.eBuffDeleteCondition.eDEAD);
			BuffData_ReAction kReAction = RaidManager.instance.m_NonSyncPvpPlayerPets[i].summonCBase.m_BuffController.FindFrontReActionBuff();
			RaidManager.instance.m_NonSyncPvpPlayerPets[i].summonCBase.m_BuffController.BuffEnd(kReAction);
			((HeroNpcAI)RaidManager.instance.m_NonSyncPvpPlayerPets[i].summonCBase.m_CharAi).SetRevival();

			//((HeroNpcAI)RaidManager.instance.m_NonSyncPvpPlayerPets[i].summonCBase.m_CharAi).SetNpcState(HeroNpcAI.eNpcState.eSPWAN_STANDBY);
			((HeroNpcAI)(RaidManager.instance.m_NonSyncPvpPlayerPets[i].summonCBase.m_CharAi)).SetPeaceState();
			((HeroNpcAI)RaidManager.instance.m_NonSyncPvpPlayerPets[i].summonCBase.m_CharAi).m_ActionController.Init();
			((HeroNpcAI)RaidManager.instance.m_NonSyncPvpPlayerPets[i].summonCBase.m_CharAi).SetAutoPlayState(HeroNpcAI.eAutoProcessState.eReady);

			int summonUniqueID = RaidManager.instance.m_NonSyncPvpPlayerPets[i].summonCBase.charUniqID + 10;
			InGameManager.instance.m_otherPartys.Add(summonUniqueID);

			m_bPutPvpPet = true;
		}
			
		InGameManager.instance.m_uiIngameSummonInfo.Set4v4PvpSummonSlotInfo();
	}
#endif


	public void SetEquipSummon_RecoveryMaterial()
	{
		if (m_PlayerSummonObjs != null)
		{
			for (int i = 0; i < m_PlayerSummonObjs.Count; i++)
			{
				if (m_PlayerSummonObjs[i].m_CharAi.GetNpcProp().Summon_EquipIndex > -1 && m_PlayerSummonObjs[i].m_CharAi.GetNpcProp().Summon_EquipSlotGroupIndex == 1)
				{
					if (m_PlayerSummonObjs[i].m_CharAi.GetNpcProp().Summon_Check_Die == true)
					{
						m_PlayerSummonObjs[i].m_CharacterDamageEffect.Spawn_RecoveryMaterial();
					}
				}
			}
		}
	}
	public void SetEquipRefresh(CharacterBase tmp_CharacterBase)
	{
		Vector3 pos = new Vector3(0, 0, 0);
		if (tmp_CharacterBase != null)
		{
			if (tmp_CharacterBase.m_MyObj.gameObject.activeSelf == false)
			{
				tmp_CharacterBase.m_MyObj.gameObject.SetActive(true);
			}
			pos.x = PlayerManager.instance.GetMyPlayerInfo().playerCharBase.gameObject.transform.localPosition.x + ((tmp_CharacterBase.m_CharAi.GetNpcProp().Summon_EquipIndex + 1) * 1);
			pos.y = PlayerManager.instance.GetMyPlayerInfo().playerCharBase.gameObject.transform.localPosition.y;
			pos.z = PlayerManager.instance.GetMyPlayerInfo().playerCharBase.gameObject.transform.localPosition.z + ((tmp_CharacterBase.m_CharAi.GetNpcProp().Summon_EquipIndex + 1) * 1);
			tmp_CharacterBase.m_MyObj.transform.localPosition = pos;

			pos.x = PlayerManager.instance.GetMyPlayerInfo().playerCharBase.gameObject.transform.position.x + ((tmp_CharacterBase.m_CharAi.GetNpcProp().Summon_EquipIndex + 1) * 1);
			pos.y = PlayerManager.instance.GetMyPlayerInfo().playerCharBase.gameObject.transform.position.y;
			pos.z = PlayerManager.instance.GetMyPlayerInfo().playerCharBase.gameObject.transform.position.z + ((tmp_CharacterBase.m_CharAi.GetNpcProp().Summon_EquipIndex + 1) * 1);
			tmp_CharacterBase.m_MyObj.transform.position = pos;
			((HeroNpcAI)tmp_CharacterBase.m_CharAi).SetNpcState(HeroNpcAI.eNpcState.ePEACE);
			((HeroNpcAI)tmp_CharacterBase.m_CharAi).m_ActionController.Init();
			((HeroNpcAI)tmp_CharacterBase.m_CharAi).SetAutoPlayState(HeroNpcAI.eAutoProcessState.eReady);
			tmp_CharacterBase.m_CharacterDamageEffect.Spawn_RecoveryMaterial();


			EffectSkillManager effectSkillManager = tmp_CharacterBase.m_MyObj.gameObject.GetComponent<EffectSkillManager>();
			tmp_CharacterBase.m_AttackInfos.Clear();
			tmp_CharacterBase.InitAttackInfo_Npc(ref effectSkillManager);

			tmp_CharacterBase.SetChangeMotionState(MOTION_STATE.eMOTION_SPWAN);
			tmp_CharacterBase.SetChangeMotionState(MOTION_STATE.eMOTION_IDLE);
		}
	}
	
	public void CleanHeroObjects(CharacterBase SummoncharBase)
	{
		if (SummoncharBase == null)
			return;

		//List<CharacterBase> npcs = GetMonsterCharObjects();

		NpcInfo.NpcProp npcProp = SummoncharBase.m_CharAi.GetNpcProp();
		for (int k = 0; k < npcProp.link_SkillCode.Count; ++k)
		{
			CleanNpcObjects_Effect(SummoncharBase, npcProp.link_SkillCode[k]);
		}

		for (int k = 0; k < npcProp.Npc_skill_Id.Length; ++k)
		{
			CleanNpcObjects_Effect(SummoncharBase, npcProp.Npc_skill_Id[k]);
		}

		//Spawn Eff
		for (int k = 0; k < npcProp.SpawnEffResInfo.m_EffObjList.Count; ++k)
		{
			DestroyObject(npcProp.SpawnEffResInfo.m_EffObjList[k].m_EffPrefab);
		}

		//hpBar
		SummoncharBase.m_hpBar.DeleteUIHP();
		DestroyObject(SummoncharBase.m_hpBar);
		//Resources.UnloadAsset(SummoncharBase.m_hpBar);
		if (((HeroNpcAI)SummoncharBase.m_CharAi).m_PrefabRandererCom != null)
		{
			for (int k = 0; k < ((HeroNpcAI)SummoncharBase.m_CharAi).m_PrefabRandererCom.Count; k++)
			{
				Destroy(((HeroNpcAI)SummoncharBase.m_CharAi).m_PrefabRandererCom[k].material);
			}

		}
		if (((HeroNpcAI)SummoncharBase.m_CharAi).m_DeathMaterial != null)
		{
			for (int k = 0; k < ((HeroNpcAI)SummoncharBase.m_CharAi).m_DeathMaterial.Count; k++)
			{
				Destroy(((HeroNpcAI)SummoncharBase.m_CharAi).m_DeathMaterial[k]);
			}
		}

		for (int k = 0; k < SummoncharBase.m_preFabRenderer.Count; ++k)
		{
			Destroy(SummoncharBase.m_preFabRenderer[k].material);
		}
		if (SummoncharBase.m_CharacterDamageEffect.m_mtTempTest != null)
		{
			for (int k = 0; k < SummoncharBase.m_CharacterDamageEffect.m_mtTempTest.Count; k++)
			{
				Destroy(SummoncharBase.m_CharacterDamageEffect.m_mtTempTest[k]);
			}
			SummoncharBase.m_CharacterDamageEffect.m_mtTempTest.Clear();
		}

		if (((HeroNpcAI)SummoncharBase.m_CharAi).m_DieMaterialProc != null)
		{
			for (int k = 0; k < ((HeroNpcAI)SummoncharBase.m_CharAi).m_DieMaterialProc.Count; k++)
			{
				Destroy(((HeroNpcAI)SummoncharBase.m_CharAi).m_DieMaterialProc[k]);
			}
			((HeroNpcAI)SummoncharBase.m_CharAi).m_DieMaterialProc.Clear();
		}
		((HeroNpcAI)SummoncharBase.m_CharAi).m_DieMaterialProc = null;


		if (SummoncharBase.m_CharacterDamageEffect.m_DefaultMaterial != null)
		{
			SummoncharBase.m_CharacterDamageEffect.m_DefaultMaterial.Clear();
			SummoncharBase.m_CharacterDamageEffect.m_DefaultMaterial = null;
		}

		if (SummoncharBase.m_CharacterDamageEffect.m_MeshRenderer != null)
		{
			for (int k = 0; k < SummoncharBase.m_CharacterDamageEffect.m_MeshRenderer.Length; k++)
			{
				Destroy(SummoncharBase.m_CharacterDamageEffect.m_MeshRenderer[k]);
				SummoncharBase.m_CharacterDamageEffect.m_MeshRenderer[k] = null;
			}
			SummoncharBase.m_CharacterDamageEffect.m_MeshRenderer = null;
		}

		if (SummoncharBase.m_EffectSkillManager.m_PlayingHitEffObjs != null)
		{
			for (int i = 0; i < SummoncharBase.m_EffectSkillManager.m_PlayingHitEffObjs.Count; i++)
			{
				Destroy(SummoncharBase.m_EffectSkillManager.m_PlayingHitEffObjs[i].m_EffPrefab);
				Destroy(SummoncharBase.m_EffectSkillManager.m_PlayingHitEffObjs[i].m_EffPS);
				SummoncharBase.m_EffectSkillManager.m_PlayingHitEffObjs[i] = null;
			}
			SummoncharBase.m_EffectSkillManager.m_PlayingHitEffObjs.Clear();
			SummoncharBase.m_EffectSkillManager.m_PlayingHitEffObjs = null;
		}
		Destroy(SummoncharBase.m_MyObj.gameObject);
	}

	public void CleanNpcObjects()
	{
		if (m_monsterCharObjects == null)
			return;

		//List<CharacterBase> npcs = GetMonsterCharObjects();
		for (int i = 0; i < m_monsterCharObjects.Count; ++i)
		{
			if (m_monsterCharObjects[i] != null)
			{
				if (m_monsterCharObjects[i].m_CharAi != null)
				{
					NpcInfo.NpcProp npcProp = m_monsterCharObjects[i].m_CharAi.GetNpcProp();

                    if(npcProp != null)
                    {
                        if(npcProp.link_SkillCode != null)
                        {
					        for (int k = 0; k < npcProp.link_SkillCode.Count; ++k)
					        {
						        CleanNpcObjects_Effect(m_monsterCharObjects[i], npcProp.link_SkillCode[k]);
					        }
                        }

                        if(npcProp.Npc_skill_Id != null)
                        {
					        for (int k = 0; k < npcProp.Npc_skill_Id.Length; ++k)
					        {
						        CleanNpcObjects_Effect(m_monsterCharObjects[i], npcProp.Npc_skill_Id[k]);
					        }
                        }

					    //Spawn Eff
                        if(npcProp.SpawnEffResInfo != null)
                        {
                            if(npcProp.SpawnEffResInfo.m_EffObjList != null)
                            {
					            for (int k = 0; k < npcProp.SpawnEffResInfo.m_EffObjList.Count; ++k)
					            {
						            DestroyObject(npcProp.SpawnEffResInfo.m_EffObjList[k].m_EffPrefab);
					            }
                            }
                        }
                    }

					//hpBar
                    if(m_monsterCharObjects[i].m_hpBar != null)
                    {
					    m_monsterCharObjects[i].m_hpBar.DeleteUIHP();
					    DestroyObject(m_monsterCharObjects[i].m_hpBar);
                    }

					NpcAI npcAI = (NpcAI)m_monsterCharObjects[i].m_CharAi;

                    if(npcAI != null)
                    {
                        if (npcAI.m_PrefabRandererCom != null)
                        {
                            for (int k = 0; k < npcAI.m_PrefabRandererCom.Count; k++)
                            {
                                Destroy(npcAI.m_PrefabRandererCom[k].material);
                            }

                        }

                        if (npcAI.m_DeathMaterial != null)
                        {
                            for (int k = 0; k < npcAI.m_DeathMaterial.Count; k++)
                            {
                                Destroy(npcAI.m_DeathMaterial[k]);
                            }
                        }

                        if (npcAI.m_DieMaterialProc != null)
                        {
                            for (int k = 0; k < npcAI.m_DieMaterialProc.Count; k++)
                            {
                                Destroy(npcAI.m_DieMaterialProc[k]);
                            }
                            npcAI.m_DieMaterialProc.Clear();
                        }
                        npcAI.m_DieMaterialProc = null;
                    }

                    if(m_monsterCharObjects[i].m_preFabRenderer != null)
                    {
					    for (int k = 0; k < m_monsterCharObjects[i].m_preFabRenderer.Count; ++k)
					    {
						    Destroy(m_monsterCharObjects[i].m_preFabRenderer[k].material);
					    }
                    }

                    if(m_monsterCharObjects[i].m_CharacterDamageEffect != null)
                    {
                        if (m_monsterCharObjects[i].m_CharacterDamageEffect.m_mtTempTest != null)
                        {
                            for (int k = 0; k < m_monsterCharObjects[i].m_CharacterDamageEffect.m_mtTempTest.Count; k++)
                            {
                                Destroy(m_monsterCharObjects[i].m_CharacterDamageEffect.m_mtTempTest[k]);
                            }
                            m_monsterCharObjects[i].m_CharacterDamageEffect.m_mtTempTest.Clear();
                        }
                        
                        //npc GameObject
                        if (m_monsterCharObjects[i].m_CharacterDamageEffect.m_DefaultMaterial != null)
                        {
                            m_monsterCharObjects[i].m_CharacterDamageEffect.m_DefaultMaterial.Clear();
                        }

                        if (m_monsterCharObjects[i].m_CharacterDamageEffect.m_MeshRenderer != null)
                        {
                            for (int k = 0; k < m_monsterCharObjects[i].m_CharacterDamageEffect.m_MeshRenderer.Length; k++)
                            {
                                Destroy(m_monsterCharObjects[i].m_CharacterDamageEffect.m_MeshRenderer[k]);
                            }
                        }
                    }

                    if (m_monsterCharObjects[i].m_MyObj != null)
                    {
                        Destroy(m_monsterCharObjects[i].m_MyObj.gameObject);
                    }
				}
			}



		}
		if (m_monsterCharObjects != null)
		{
			m_monsterCharObjects.Clear();
		}
	}

	public void CleanNpcObjects_additive(CharacterBase cBase, int skillCode)
	{
		if (cBase.m_AttackInfos.ContainsKey(skillCode) == false)
			return;

		for (int i = 0; i < cBase.m_AttackInfos[skillCode].skillinfo.link_Skill_Code.Count; ++i)
		{
			if (cBase.m_AttackInfos[skillCode].skillinfo.link_Skill_Code[i] != 0)
			{
				if (cBase.m_AttackInfos.ContainsKey(cBase.m_AttackInfos[skillCode].skillinfo.link_Skill_Code[i]) == true)
				{
					CleanNpcObjects_Effect(cBase, cBase.m_AttackInfos[skillCode].skillinfo.link_Skill_Code[i]);
				}
				for (int k = 0; k < cBase.m_AttackInfos[cBase.m_AttackInfos[skillCode].skillinfo.link_Skill_Code[i]].skillinfo.link_Skill_Code.Count; ++k)
				{
					CleanNpcObjects_additive(cBase, cBase.m_AttackInfos[cBase.m_AttackInfos[skillCode].skillinfo.link_Skill_Code[i]].skillinfo.link_Skill_Code[k]);
				}
			}

		}
	}

	public void CleanNpcObjects_Effect(CharacterBase cBase, int skillCode)
	{
		if (cBase.m_AttackInfos.ContainsKey(skillCode))
		{
			for (int i = 0; i < cBase.m_AttackInfos[skillCode].skillinfo.effBodyInfo.Count; ++i)
			{
				for (int k = 0; k < cBase.m_AttackInfos[skillCode].skillinfo.effBodyInfo[i].m_EffObjList.Count; ++k)
				{
					DestroyObject(cBase.m_AttackInfos[skillCode].skillinfo.effBodyInfo[i].m_EffObjList[k].m_EffPrefab);
				}
			}

			for (int i = 0; i < cBase.m_AttackInfos[skillCode].skillinfo.effBuffInfo.Count; ++i)
			{
				for (int k = 0; k < cBase.m_AttackInfos[skillCode].skillinfo.effBuffInfo[i].m_EffObjList.Count; ++k)
				{
					DestroyObject(cBase.m_AttackInfos[skillCode].skillinfo.effBuffInfo[i].m_EffObjList[k].m_EffPrefab);
				}
			}

			for (int i = 0; i < cBase.m_AttackInfos[skillCode].skillinfo.effHitInfo.Count; ++i)
			{
				for (int k = 0; k < cBase.m_AttackInfos[skillCode].skillinfo.effHitInfo[i].m_EffObjList.Count; ++k)
				{
					DestroyObject(cBase.m_AttackInfos[skillCode].skillinfo.effHitInfo[i].m_EffObjList[k].m_EffPrefab);
				}
			}

			for (int i = 0; i < cBase.m_AttackInfos[skillCode].skillinfo.projectTileInfo.Count; ++i)
			{
				for (int k = 0; k < cBase.m_AttackInfos[skillCode].skillinfo.projectTileInfo[i].pTileObjList.Count; ++k)
				{
					DestroyObject(cBase.m_AttackInfos[skillCode].skillinfo.projectTileInfo[i].pTileObjList[k].m_EffPrefab);
				}

				for (int k = 0; k < cBase.m_AttackInfos[skillCode].skillinfo.projectTileInfo[i].pTileEndObjList.Count; ++k)
				{
					DestroyObject(cBase.m_AttackInfos[skillCode].skillinfo.projectTileInfo[i].pTileEndObjList[k].m_EffPrefab);
				}

			}

			if (cBase.m_AttackInfos[skillCode].skillinfo.effGuideInfo != null)
			{
				for (int i = 0; i < cBase.m_AttackInfos[skillCode].skillinfo.effGuideInfo.m_EffObjList.Count; ++i)
				{
					DestroyObject(cBase.m_AttackInfos[skillCode].skillinfo.effGuideInfo.m_EffObjList[i].m_EffPrefab);
				}
			}

			CleanNpcObjects_additive(cBase, skillCode);

		}
	}

#if NONSYNC_PVP
	public void Clean4v4PvpPets()
	{
		for (int i = 0; i < RaidManager.instance.m_NonSyncPvpPlayerPets.Count; ++i)
		{
			CharacterBase petCbase = RaidManager.instance.m_NonSyncPvpPlayerPets[i].summonCBase;
			if (petCbase != null)
			{
				if (petCbase.m_CharAi != null)
				{
					NpcInfo.NpcProp npcProp = petCbase.m_CharAi.GetNpcProp();
					for (int k = 0; k < npcProp.link_SkillCode.Count; ++k)
					{
						CleanNpcObjects_Effect(petCbase, npcProp.link_SkillCode[k]);
					}

					for (int k = 0; k < npcProp.Npc_skill_Id.Length; ++k)
					{
						CleanNpcObjects_Effect(petCbase, npcProp.Npc_skill_Id[k]);
					}

					//Spawn Eff
					for (int k = 0; k < npcProp.SpawnEffResInfo.m_EffObjList.Count; ++k)
					{
						DestroyObject(npcProp.SpawnEffResInfo.m_EffObjList[k].m_EffPrefab);
					}

					//hpBar
					petCbase.m_hpBar.DeleteUIHP();
					DestroyObject(petCbase.m_hpBar);
					HeroNpcAI npcAI = (HeroNpcAI)petCbase.m_CharAi;
					if (npcAI.m_PrefabRandererCom != null)
					{
						for (int k = 0; k < npcAI.m_PrefabRandererCom.Count; k++)
						{
							Destroy(npcAI.m_PrefabRandererCom[k].material);
						}

					}
					if (npcAI.m_DeathMaterial != null)
					{
						for (int k = 0; k < npcAI.m_DeathMaterial.Count; k++)
						{
							Destroy(npcAI.m_DeathMaterial[k]);
						}
					}

					for (int k = 0; k < petCbase.m_preFabRenderer.Count; ++k)
					{
						Destroy(petCbase.m_preFabRenderer[k].material);
					}

					if (petCbase.m_CharacterDamageEffect.m_mtTempTest != null)
					{
						for (int k = 0; k < petCbase.m_CharacterDamageEffect.m_mtTempTest.Count; k++)
						{
							Destroy(petCbase.m_CharacterDamageEffect.m_mtTempTest[k]);
						}
						petCbase.m_CharacterDamageEffect.m_mtTempTest.Clear();
					}

					if (npcAI.m_DieMaterialProc != null)
					{
						for (int k = 0; k < npcAI.m_DieMaterialProc.Count; k++)
						{
							Destroy(npcAI.m_DieMaterialProc[k]);
						}
						npcAI.m_DieMaterialProc.Clear();
					}
					npcAI.m_DieMaterialProc = null;

					//npc GameObject
					//DestroyObject(petCbase.m_MyObj.gameObject);
					if (petCbase.m_CharacterDamageEffect.m_DefaultMaterial != null)
					{
						petCbase.m_CharacterDamageEffect.m_DefaultMaterial.Clear();
					}

					if (petCbase.m_CharacterDamageEffect.m_MeshRenderer != null)
					{
						for (int k = 0; k < petCbase.m_CharacterDamageEffect.m_MeshRenderer.Length; k++)
						{
							Destroy(petCbase.m_CharacterDamageEffect.m_MeshRenderer[k]);
						}

					}

					Destroy(petCbase.m_MyObj.gameObject);
				}
			}
		}
	}
#endif


	public void DestroyDropObjects()
	{
		if (m_DropGoldObj != null)
		{
			DestroyObject(m_DropGoldObj);
			m_DropGoldObj = null;
		}
		if (m_DropCoinObj != null)
		{
			DestroyObject(m_DropCoinObj);
			m_DropCoinObj = null;
		}
		if (m_DropCashObj != null)
		{
			DestroyObject(m_DropCashObj);
			m_DropCashObj = null;
		}
	}

	public void SetPetSlotInfo(List<blame_messages.PetSlotInfo> petInfos)
	{
		PlayerInfo a_hPlayerInfo = PlayerManager.instance.m_PlayerInfo[PlayerManager.MYPLAYER_INDEX];

		blame_messages.PetSlotType petSlotType;

		for (int i = 0; i < petInfos.Count; ++i)
		{
			petSlotType = (blame_messages.PetSlotType)petInfos[i].type;

			a_hPlayerInfo.m_SummonSlotGroup[(int)petSlotType][0].m_SlotLavel = (int)petInfos[i].first_slot_level;
			a_hPlayerInfo.m_SummonSlotGroup[(int)petSlotType][0].m_SlotSummonIndex = (int)petInfos[i].first_slot_petindex;
			a_hPlayerInfo.m_SummonSlotGroup[(int)petSlotType][1].m_SlotLavel = (int)petInfos[i].second_slot_level;
			a_hPlayerInfo.m_SummonSlotGroup[(int)petSlotType][1].m_SlotSummonIndex = (int)petInfos[i].second_slot_petindex;
			a_hPlayerInfo.m_SummonSlotGroup[(int)petSlotType][2].m_SlotLavel = (int)petInfos[i].third_slot_level;
			a_hPlayerInfo.m_SummonSlotGroup[(int)petSlotType][2].m_SlotSummonIndex = (int)petInfos[i].third_slot_petindex;
		}
	}



	public void SummonSlotGroupInit(Dictionary<int, List<SummonSlotData>> h_SummonSlotGroup)
	{
		int a_hGroupIndex = 0;
		if (h_SummonSlotGroup != null)
		{
			h_SummonSlotGroup.Clear();
		}
		for (int i = 0; i < SUMMONSLOTGROUP_MAX; i++)
		{
			List<SummonSlotData> a_hListSummonSlotData = new List<SummonSlotData>();

			for (int j = 0; j < SUMMONSLOT_MAX; j++)
			{
				SummonSlotData a_hSummonSlotData = new SummonSlotData();
				a_hSummonSlotData.m_SlotLavel = -1;
				a_hSummonSlotData.m_SlotSummonIndex = -1;
				a_hListSummonSlotData.Add(a_hSummonSlotData);
			}

			a_hGroupIndex = i + 1;
			h_SummonSlotGroup.Add(a_hGroupIndex, a_hListSummonSlotData);
		}
	}

	public void SetNowSlotVariable(int kSummonIndex, int kSlotIndex, int kTeamSlotGroupIndex, bool kSummonSlotChange)
	{		
		NpcManager.instance.m_TeamSlotSummonIndex = kSummonIndex;
		NpcManager.instance.m_TeamSlotIndex = kSlotIndex;
		NpcManager.instance.m_TeamSlotGroupIndex = kTeamSlotGroupIndex;
		NpcManager.instance.m_TeamSummonSlotChange = kSummonSlotChange;
	}
	public bool CheckSwapBlock()
	{
		bool					a_kSlaveSlotLock		= false;
		
		PlayerInfo				a_kPlayerInfo			= PlayerManager.instance.m_PlayerInfo[PlayerManager.MYPLAYER_INDEX];
		List<SummonSlotData>	a_kSummonSlotData		= new List<SummonSlotData>();


		if (a_kPlayerInfo.m_SummonSlotGroup == null)
		{
			return a_kSlaveSlotLock;
		}

		a_kPlayerInfo.m_SummonSlotGroup.TryGetValue(2, out a_kSummonSlotData);

		for (int i = 0; i < a_kSummonSlotData.Count; i++)
		{
			if(a_kSummonSlotData[i].m_SlotLavel == 0)
			{
				return a_kSlaveSlotLock;
			}
		}

		foreach (KeyValuePair<int, List<SummonSlotData>> SummonslotGroup in a_kPlayerInfo.m_SummonSlotGroup)
		{
			for (int i = 0; i < SummonslotGroup.Value.Count; i++)
			{
				NpcInfo.NpcProp a_kSummonProp = null;
				int a_kSlotIndex = i;
				int a_kGroupIndex = SummonslotGroup.Key;
				int a_kSummonIndex = -1;


				if (SummonslotGroup.Value[i].m_SlotSummonIndex > -1)
				{
					a_kSummonIndex = SummonslotGroup.Value[i].m_SlotSummonIndex;
					if(NpcManager.instance.m_PlayerSummonProp[a_kSummonIndex].Summon_Check_Die == true)
					{
						return false;
					}
					a_kSummonProp = NpcManager.instance.m_PlayerSummonProp[a_kSummonIndex];
				}
				if (SummonslotGroup.Key == 1)
				{
					if (a_kSummonSlotData[i].m_SlotLavel == 0)
					{
						return false;
					}
				}			
			}

		}

		return true;
	}
	public void SetDieInit()
	{
		PlayerInfo a_kPlayerInfo = PlayerManager.instance.m_PlayerInfo[PlayerManager.MYPLAYER_INDEX];
		UI_SummonInfo a_kSummonInfo = InGameManager.instance.m_uiSummon.m_SummonInfo;

		if (a_kPlayerInfo.m_SummonSlotGroup == null || a_kSummonInfo == null)
		{
			return;
		}

		foreach (KeyValuePair<int, List<SummonSlotData>> SummonslotGroup in a_kPlayerInfo.m_SummonSlotGroup)
		{
			for (int i = 0; i < SummonslotGroup.Value.Count; i++)
			{
				NpcInfo.NpcProp a_kSummonProp = null;
				int a_kSlotIndex = i;
				int a_kGroupIndex = SummonslotGroup.Key;
				int a_kSummonIndex = -1;

				if (SummonslotGroup.Value[i].m_SlotSummonIndex > -1)
				{
					a_kSummonIndex = SummonslotGroup.Value[i].m_SlotSummonIndex;
					a_kSummonProp = NpcManager.instance.m_PlayerSummonProp[a_kSummonIndex];
					a_kSummonInfo.SetDieInit(a_kSummonIndex, a_kGroupIndex, a_kSlotIndex);
				}
			}
		}
	}
	

	public void CheckLockOpen(int kSlotIndex)
	{

		PlayerInfo a_kPlayerInfo = PlayerManager.instance.m_PlayerInfo[PlayerManager.MYPLAYER_INDEX];

		List<SummonSlotData> a_kSummonSlotData = new List<SummonSlotData>();

		NpcManager.SummonSlotPrice a_kSummonSlotPrice = new NpcManager.SummonSlotPrice();

		

		if (InGameManager.instance.m_uiPurchasePopupTwoButton == null)
		{

			InGameManager.instance.m_uiPurchasePopupTwoButton = (UI_PurchasePopupTwoButton)SceneManager.instance.SetUIComponent(SceneManager.eCOMPONENT.PurchasePopupTwoButton);
			InGameManager.instance.m_uiPurchasePopupTwoButton.Initialized();
		}
		
		int a_kSlotLevelUpPrice = 0;

		if (a_kPlayerInfo.m_SummonSlotGroup == null)
		{
			return;
		}

		a_kPlayerInfo.m_SummonSlotGroup.TryGetValue(2, out a_kSummonSlotData);

		int a_nSlavePriceDataSlotLavel = a_kSummonSlotData[kSlotIndex].m_SlotLavel;

		if (a_nSlavePriceDataSlotLavel == -1)
		{
			a_nSlavePriceDataSlotLavel = 0;
		}

		m_SummonSlavePriceData.TryGetValue(a_nSlavePriceDataSlotLavel, out a_kSummonSlotPrice);

		
		if (kSlotIndex == 0)
		{
			a_kSlotLevelUpPrice = a_kSummonSlotPrice.SlotPrice_1;
		}
		else if (kSlotIndex == 1)
		{
			a_kSlotLevelUpPrice = a_kSummonSlotPrice.SlotPrice_2;
		}
		else
		{
			a_kSlotLevelUpPrice = a_kSummonSlotPrice.SlotPrice_3;
		}
		if (SummonManager.instance.CheckSlotLevelUpPriceChas(a_kSlotLevelUpPrice) == true)
		{
			int NowkSlotLevel = a_kSummonSlotData[kSlotIndex].m_SlotLavel;
			int NextSlotLevel = NowkSlotLevel + 1;
			string a_kkTitleMessage = SummonManager.instance.GetSlotLevelUp_TitleMsg(NowkSlotLevel);
			SummonManager.instance.CheckBuySlaveSlotSetActive(false);
			SummonManager.instance.SetActivePurchasePopup(true);
			SummonManager.instance.SetPurchasePopup(InGameManager.instance.m_uiPurchasePopupTwoButton, a_kSlotLevelUpPrice, kSlotIndex, NowkSlotLevel, a_kkTitleMessage, NowkSlotLevel.ToString(), NextSlotLevel.ToString());
		}
		else
		{
			//잼이 모자라는 팝업			
			int nGem = a_kSlotLevelUpPrice - (int)UserManager.instance.cash;

			SummonManager.instance.CheckBuySlaveSlotSetActive(false);
			SummonManager.instance.SetActivePurchasePopup(false);
			PopupManager.instance.ShowPopupOneButton(nGem +"  " + CDataManager.m_LocalText["not_enough_gem"], CDataManager.m_LocalText["ok"], delegate { SummonManager.instance.CheckBuySlaveSlotSetActive(true); } , false);
		}
	}

    public string GetBossName()
    {
        int rowIdx = (int)NpcManager.instance.m_NpcDataIndexs[MapManager.instance.LastPlayMapData.bossID];
        return CExcelData_NPC_DATA.instance.NPC_DATABASE_GetNPC_NAME_ID(rowIdx);
    }

    public void SetSummonEquipItems(int petIdx)
    {
        if (m_PlayerSummonList[petIdx].equipItems == null)
        {
            m_PlayerSummonList[petIdx].equipItems = new EquipedItems();
        }
    }

    public void SetSummonEquipItems(ItemData itemData, int petIdx)
    {
        NpcInfo.NpcProp npcProp = GetSummonNpcPropByIndex(petIdx);

        m_PlayerSummonList[petIdx].equipItems.SetEquip(itemData);

        npcProp.curSummonEquipItems.m_dicItems[itemData.ItemSubKind] = itemData;
    }

}

