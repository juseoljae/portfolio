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
#endif
#endregion Global project preprocessor directives.

using System.Collections.Generic;
using System.Linq;
using BehaviorDesigner.Runtime;
using GroupManagement.ManagementEnums;
using UnityEngine;
using static AchieveTableData;
using UniRx;
using static ManagementAPI;
public class ManagementDataManager : Singleton<ManagementDataManager>
{
	//Key-ID
	private Dictionary<long, YgBuildingData> YgBuildingDataDic = new Dictionary<long, YgBuildingData>();
	private Dictionary<long, SectionTableData> SectionTableDataDic = new Dictionary<long, SectionTableData>();
	//Key-Section_ID
	private Dictionary<long, List<SectionCreationTableData>> SectionCreationTableDataDic = new Dictionary<long, List<SectionCreationTableData>>();//
	private Dictionary<long, List<SectionLevelTableData>> SectionLevelTableDataDic = new Dictionary<long, List<SectionLevelTableData>>();
	//dont use anymore
	//private Dictionary<long, List<SectionInteriorData>> SectionInteriorDataDic = new Dictionary<long, List<SectionInteriorData>>();
	//Key-Job_Group_ID
	//private Dictionary<long, List<JobGroupData>> JobGroupDataDic = new Dictionary<long, List<JobGroupData>>();
	//Key-Training_Group_ID
	private Dictionary<long, List<ActivityData>> ActivityDataDic = new Dictionary<long, List<ActivityData>>();
	private Dictionary<string, ExternalBehavior> ActivityAvatarBT = new Dictionary<string, ExternalBehavior>();
	
	//<groupID, <AvatarType, AvatarTalkList>>
	private Dictionary<long, Dictionary<AVATAR_TYPE, List<ActivityTalkData>>> ActivityTalkDataDic = new ();
	//Key-ID
	private Dictionary<long, BuildingSkinData> BuildingSkinDataDic = new Dictionary<long, BuildingSkinData>();

	// 업적 테이블 <ID, row>
	private Dictionary<long, AchieveTableData> AchieveTableDic;
	// 업적 테이블을 그룹 테이블화 <Group_ID, <Group_Sequence, row>>
	private Dictionary<long, Dictionary<byte, AchieveTableData>> AchieveGroupTableDic;
	// 트로피 테이블 <ID, row>
	private Dictionary<long, TrophyTableData> TrophyDic;

	private Dictionary<AVATAR_TYPE, List<AvatarInteractionData>> AvatarInteractionDataDic = new Dictionary<AVATAR_TYPE, List<AvatarInteractionData>>();

	bool _isInit = false;

    private Dictionary<long, List<ActivityTalkData>> CharTalkDataDic = new Dictionary<long, List<ActivityTalkData>>();//key Group. for BT talk bubble

    //temporary
    //public long tempSectionID = 2902;

    public bool IsInit
	{
		get { return _isInit; }
		set { _isInit = value; }
	}

	public const int REQ_COUNT = 5;
    public const int REQTYPE_COUNT = 5;
    public const int CONSUME_COUNT = 2;
	public enum RCTYPE
    {
		REQUIRE = 0,
		CONSUME,
    }
	//------------------------------------------------------------------------------------

	#region LOAD_DATA
	public static void LoadYgBuildingData()
    {
		Instance._LoadYgBuildingData();
	}

	public static void LoadSectionData()
    {
		Instance._LoadSectionTableData();
	}

	public static void LoadSectionCreateionData()
	{
		Instance._LoadSectionCreateionData();
	}

	public static void LoadSectionLevelData()
	{
		Instance._LoadSectionLevelData();
	}

	//public static void LoadTrainingRoomLevelData()
 //   {
	//	Instance._LoadTrainingRoomLevelData();
	//}

	//public static void LoadSectionInteriorData()
	//{
	//	Instance._LoadSectionInteriorData();
	//}

	//public static void LoadJobGroupData()
	//{
	//	Instance._LoadJobGroupData();
	//}

	public static void LoadActivityTableData()
	{
		Instance._LoadActivityTableData();
	}

	public static void LoadActivityTalkData()
    {
		Instance._LoadActivityTalkData();
    }

	//public static void LoadProductionData()
	//{
	//	Instance._LoadProductionData();
	//}

	public static void LoadBuildingSkinData ()
	{
		Instance._LoadBuildingSkinData ();
	}

    public static void LoadTrophyData ()
    {
        Instance._loadTrophyData ();
    }

    public static void LoadAchieveData ()
	{
		Instance._loadAchieveData ();
	}

	public static void LoadAvatarInteractionData()
    {
		Instance._LoadAvatarInteractionData();
	}

	private void _LoadYgBuildingData()
	{
		DataTable table = CDataManager.GetTable(ETableDefine.TABLE_MANAGEMENT_YGBUILDING);
		if (null == table)
		{
			CDebug.LogError(string.Format("Not Found Table : [{0}]", ETableDefine.TABLE_MANAGEMENT_YGBUILDING));
			return;
		}

        if (YgBuildingDataDic != null)
            YgBuildingDataDic.Clear();
        else
            YgBuildingDataDic = new Dictionary<long, YgBuildingData>();

		for (int index = 0; index < table.RowCount; ++index)
		{
			YgBuildingData data = new YgBuildingData
			{
				ID = table.GetValue<long>("ID", index),
				Floors = table.GetValue<string>("Floors", index),
				Floor_Room1 = table.GetValue<long>("Floor_Room1", index),
				Floor_Room2 = table.GetValue<long>("Floor_Room2", index),
				Floor_Room3 = table.GetValue<long>("Floor_Room3", index),
				Floor_Room4 = table.GetValue<long>("Floor_Room4", index),
				Floor_Room5 = table.GetValue<long>("Floor_Room5", index),
				Floor_Room6 = table.GetValue<long>("Floor_Room6", index),
				Floor_Room7 = table.GetValue<long>("Floor_Room7", index),
				Floor_Room8 = table.GetValue<long>("Floor_Room8", index),

				//OpenRequireID = table.GetValue<long>("Req_ID", i),

				Open_Time = table.GetValue<float>("Open_Time", index),
				//Open_Event_ID = table.GetValue<byte>("Open_Event_ID", index),
				//End_Event_ID = table.GetValue<byte>("End_Event_ID", index)
			};

			for (int i = 1; i <= REQ_COUNT; i++)
			{
				CRequire req = new CRequire
				{
					Type = (REQUIRE_TYPES)table.GetValue<int>(string.Format($"Req{i}_Type"), index),
					Value1 = table.GetValue<long>(string.Format($"Req{i}_Value1"), index),
					Value2 = table.GetValue<long>(string.Format($"Req{i}_Value2"), index),
					Value3 = table.GetValue<long>(string.Format($"Req{i}_Value3"), index),

					StringID = table.GetValue<long>(string.Format($"Req{i}_String"), index),
					ShortcutLink = table.GetValue<long>(string.Format($"Req{i}_Link"), index),
				};

				if (req.Type != REQUIRE_TYPES.NULL)
				{
					//Debug.Log(data.ID + " / Require Type = "+req.Type +"/ value1 = "+req.Value1+"/value2 = "+req.Value2);
					data.Requires.Add(req);
				}
			}

			//Consume
			const int CONSUME_COUNT = 2;
			for (int j = 1; j <= CONSUME_COUNT; j++)
            {
				data.Consume[j-1] = new CConsume
				{
					Type = (REWARD_CONSUME_TYPES)table.GetValue<byte>($"Open_Consume_Type{j}", index),
					Value1 = table.GetValue<long>($"Open_Consume_Type{j}_Value1", index),
					Value2 = table.GetValue<long>($"Open_Consume_Type{j}_Value2", index),
				};

				if(data.Consume[j - 1].Type == REWARD_CONSUME_TYPES.NULL)
                {
					data.Consume[j - 1] = null;
				}
            }

            if (!YgBuildingDataDic.ContainsKey(data.ID))
            {
				YgBuildingDataDic.Add(data.ID, data);
			}
			else
			{
				CDebug.LogError(string.Format($"{ETableDefine.TABLE_MANAGEMENT_YGBUILDING} ID:[ {data.ID} ] is already contain data"));
			}
		}
	}

	public void _LoadSectionTableData()
	{
		DataTable table = CDataManager.GetTable(ETableDefine.TABLE_MANAGEMENT_SECTION);
		if (null == table)
		{
			CDebug.LogError(string.Format("Not Found Table : [{0}]", ETableDefine.TABLE_MANAGEMENT_SECTION));
			return;
		}

        if (SectionTableDataDic != null)
            SectionTableDataDic.Clear();
        else
            SectionTableDataDic = new Dictionary<long, SectionTableData>();


		//long[] npcSlot = new long[9];
		for (int i = 0; i < table.RowCount; ++i)
		{
			SectionTableData data = new SectionTableData
			{
				ID = table.GetValue<long>("ID", i),
				Section_Type = table.GetValue<byte>("Section_Type", i),
				Section_Sub_Type = table.GetValue<byte>("Section_Sub_Type", i),
				Section_Value = table.GetValue<int>("Section_Value", i),
				Move_On = table.GetValue<byte>("Move_On", i),
				List_On = table.GetValue<byte>("List_On", i),
				Section_Name = table.GetValue<long>("Section_Name", i),
				Section_Icon = table.GetValue<string>("Section_Icon", i),
				Sound_ID = table.GetValue<long>("Sound_ID", i),
				Section_Desc = table.GetValue<long>("Section_Desc", i),
				Upgrade_Type = table.GetValue<byte>("Upgrade_Type", i),
				Gather_Type = table.GetValue<byte>("Gather_Type", i),
			};

            data.SetSectionData();

			if (!SectionTableDataDic.ContainsKey(data.ID))
            {
				SectionTableDataDic.Add(data.ID, data);
			}
			else
			{
				CDebug.LogError(string.Format($"{ETableDefine.TABLE_MANAGEMENT_SECTION} ID:[ {data.ID} ] is already contain data"));
			}
		}
	}

	public void _LoadSectionCreateionData()
	{
		DataTable table = CDataManager.GetTable(ETableDefine.TABLE_MANAGEMENT_SECTION_CREATION);
		if (null == table)
		{
			CDebug.LogError(string.Format("Not Found Table : [{0}]", ETableDefine.TABLE_MANAGEMENT_SECTION_CREATION));
			return;
		}

        if (SectionCreationTableDataDic != null)
            SectionCreationTableDataDic.Clear();
        else
            SectionCreationTableDataDic = new Dictionary<long, List<SectionCreationTableData>>();

		long[] npcSlot = new long[9];
		for (int index = 0; index < table.RowCount; ++index)
        {
			SectionCreationTableData data = new SectionCreationTableData
			{
				ID = table.GetValue<long>("ID", index),
				Section_ID = table.GetValue<long>("Section_ID", index),
				Section_ID_No = table.GetValue<byte>("Section_ID_No", index),
                Producer_Lv = table.GetValue<int>("Producer_Lv", index),
				Creation_Time = table.GetValue<float>("Creation_Time", index)
			};

			//21.09.23 추가			
			for (int i = 1; i <= REQ_COUNT; i++)
			{
				CRequire req = new CRequire();

				req.Type = (REQUIRE_TYPES)table.GetValue<int>(string.Format($"Req{i}_Type"), index);
				req.Value1 = table.GetValue<long>(string.Format($"Req{i}_Value1"), index);
				req.Value2 = table.GetValue<long>(string.Format($"Req{i}_Value2"), index);
				req.Value3 = table.GetValue<long>(string.Format($"Req{i}_Value3"), index);

				if (i == 1)
                {
					req.StringID = table.GetValue<long>(string.Format($"Req{i}_String"), index);
					req.ShortcutLink = table.GetValue<long>(string.Format($"Req{i}_Link"), index);
				}

				if (req.Type != REQUIRE_TYPES.NULL)
					data.Requires.Add(req);
			}

			//Consume
			for (int j = 1; j <= CONSUME_COUNT; j++)
			{
				data.Consume[j - 1] = new CConsume
				{
					Type = (REWARD_CONSUME_TYPES)table.GetValue<byte>($"Creation_Consume_Type{j}", index),
					Value1 = table.GetValue<long>($"Creation_Consume_Type{j}_Value1", index),
					Value2 = table.GetValue<long>($"Creation_Consume_Type{j}_Value2", index),
				};
			} 

			if (SectionCreationTableDataDic.ContainsKey(data.Section_ID))
            {
				SectionCreationTableDataDic[data.Section_ID].Add(data);
			}
			else
            {
				SectionCreationTableDataDic.Add(data.Section_ID, new List<SectionCreationTableData>());
				SectionCreationTableDataDic[data.Section_ID].Add(data);
			}

			for (int j = 0; j < npcSlot.Length; ++j)
			{
				npcSlot[j] = table.GetValue<long>("NPC_Slot_" + j, index);
				if (npcSlot[j] != 0)
				{
					data.NpcSlot.Add(npcSlot[j]);
				}
			}
		}
	}

	private void _LoadSectionLevelData()
	{
		DataTable table = CDataManager.GetTable(ETableDefine.TABLE_MANAGEMENT_SECTION_LEVEL);
		if (null == table)
		{
			CDebug.LogError(string.Format("Not Found Table : [{0}]", ETableDefine.TABLE_MANAGEMENT_SECTION_LEVEL));
			return;
		}


        if (SectionLevelTableDataDic != null)
            SectionLevelTableDataDic.Clear();
        else
            SectionLevelTableDataDic = new Dictionary<long, List<SectionLevelTableData>>();



		for (int index = 0; index < table.RowCount; ++index)
		{
			SectionLevelTableData data = new SectionLevelTableData
			{
				ID = table.GetValue<long>("ID", index),
				Section_ID = table.GetValue<long>("Section_ID", index),
				Section_LV = table.GetValue<byte>("Section_LV", index),
                Producer_Lv = table.GetValue<int>("Producer_Lv", index),
				Section_Resources = table.GetValue<string>("Section_Resources", index),

				Activity_Group_ID = table.GetValue<long>("Activity_Group_ID", index),

				Function_Type = table.GetValue<byte>("Function_Type", index),
				Function_Value = table.GetValue<long>("Function_Value", index),
				Function_Icon = table.GetValue<string>("Function_Icon", index),

				Production_Value = table.GetValue<int>("Production_Value", index),
				Production_Time = table.GetValue<long>("Production_Time", index),
				Production_Gather = table.GetValue<long>("Production_Gather", index),
				Production_Storage = table.GetValue<long>("Production_Storage", index),


				Upgrade_Time = table.GetValue<int>("Upgrade_Time", index),

				Section_Size_X = table.GetValue<byte>("Section_Size_X", index),
				Section_Size_Y = table.GetValue<byte>("Section_Size_Y", index),
			};

			//Require
			//data.Require_ID = table.GetValue<long>("Req_ID", i);
			//21.09.23 추가
			for (int i = 1; i <= REQ_COUNT; i++)
			{
				CRequire req = new CRequire
				{
					Type = (REQUIRE_TYPES)table.GetValue<int>(string.Format($"Req{i}_Type"), index),
					Value1 = table.GetValue<long>(string.Format($"Req{i}_Value1"), index),
					Value2 = table.GetValue<long>(string.Format($"Req{i}_Value2"), index),
					Value3 = table.GetValue<long>(string.Format($"Req{i}_Value3"), index),

					StringID = table.GetValue<long>(string.Format($"Req{i}_String"), index),
					ShortcutLink = table.GetValue<long>(string.Format($"Req{i}_Link"), index)
				};

				//string reqstr = table.GetValue<string>(string.Format($"Req{i}_String"), index);

				if (req.Type != REQUIRE_TYPES.NULL)
				{
					data.Requires.Add(req);
					//data.ReqStrings.Add(reqstr);
				}
			}

			// Consume
			for (int j = 1; j <= CONSUME_COUNT; j++)
			{
				data.Consume[j - 1] = new CConsume
				{
					Type = (REWARD_CONSUME_TYPES)table.GetValue<byte>($"Upgrade_Consume_Type{j}", index),
					Value1 = table.GetValue<long>($"Upgrade_Consume_Type{j}_Value1", index),
					Value2 = table.GetValue<long>($"Upgrade_Consume_Type{j}_Value2", index),
				};
			}

			if (SectionLevelTableDataDic.ContainsKey(data.Section_ID))
            {
				SectionLevelTableDataDic[data.Section_ID].Add(data);
			}
			else
            {
				SectionLevelTableDataDic.Add(data.Section_ID, new List<SectionLevelTableData>());
				SectionLevelTableDataDic[data.Section_ID].Add(data);
			}
		}
	}

	private void _LoadActivityTableData()
	{
        DataTable table = CDataManager.GetTable(ETableDefine.TABLE_MANAGEMENT_ACTIVITY);
        if (null == table)
        {
            CDebug.LogError(string.Format("Not Found Table : [{0}]", ETableDefine.TABLE_MANAGEMENT_ACTIVITY));
            return;
        }

        if (ActivityDataDic != null)
			ActivityDataDic.Clear();
        else
			ActivityDataDic = new Dictionary<long, List<ActivityData>>();

        for (int i = 0; i < table.RowCount; ++i)
        {
			ActivityData data = new ActivityData
			{
                Activity_ID = table.GetValue<long>("Activity_ID", i),
				Activity_Group_ID = table.GetValue<long>("Activity_Group_ID", i),
				Activity_Type = table.GetValue<long>("Activity_Type", i),
				Activity_Sub_Type = table.GetValue<long>("Activity_Sub_Type", i),
				Stat_LV = table.GetValue<long>("Stat_LV", i),
				Activity_Icon = table.GetValue<string>("Activity_Icon", i),
				NPC_ID = table.GetValue<long>("NPC_ID", i),
				Activity_Time = table.GetValue<long>("Activity_Time", i),
				BT_ID = table.GetValue<string>("BT_ID", i),
				Activity_linktable_ID = table.GetValue<long>("Activity_linktable_ID", i),
				
				Consume_Condition_Value = table.GetValue<long>("Consume_Condition_Value", i),
				Consume_NPC = table.GetValue<long>("Consume_NPC", i),
				End_Talk_Group_ID = table.GetValue<long>("End_Talk_Group_ID", i),
				End_AvatarMotion_ID = table.GetValue<long>("End_AvatarMotion_ID", i),
				End_AvatarMotion2_ID = table.GetValue<long>("End_AvatarMotion2_ID", i),

                Reward_ID = table.GetValue<long>("Reward_ID", i),
				//Reward2_ID = table.GetValue<long>("Reward2_ID", i),
				//Reward3_ID = table.GetValue<long>("Reward3_ID", i),
				//Reward4_ID = table.GetValue<long>("Reward4_ID", i),
			};

			CConsume consume = new CConsume();
			consume.Type = (REWARD_CONSUME_TYPES)table.GetValue<byte>("Consume_Type1", i);
			consume.Value1 = table.GetValue<long>("Consume_Type1_Value1", i);
			consume.Value2 = table.GetValue<long>("Consume_Type1_Value2", i);
			data.Consume = consume;

            //BT
            if (data.BT_ID.Equals("0") == false)
            {
                if (ActivityAvatarBT.ContainsKey(data.BT_ID) == false)
                {
                    ActivityAvatarBT.Add(data.BT_ID, null);
                }
            }

			if (ActivityDataDic.ContainsKey(data.Activity_Group_ID))
			{
				ActivityDataDic[data.Activity_Group_ID].Add(data);
			}
			else
			{
				ActivityDataDic.Add(data.Activity_Group_ID, new List<ActivityData>());
				ActivityDataDic[data.Activity_Group_ID].Add(data);
			}
		}
    }

	private void _LoadActivityTalkData()
	{
		DataTable table = CDataManager.GetTable(ETableDefine.TABLE_MANAGEMENT_ACTIVITY_TALK);
		if (null == table)
		{
			CDebug.LogError(string.Format("Not Found Table : [{0}]", ETableDefine.TABLE_MANAGEMENT_ACTIVITY_TALK));
			return;
		}

		ActivityTalkDataDic?.Clear ();
		ActivityTalkDataDic = new ();

		if (CharTalkDataDic != null)
		{
			CharTalkDataDic.Clear();
		}
		else
		{
			CharTalkDataDic = new Dictionary<long, List<ActivityTalkData>>();
		}

        for (int i = 0; i < table.RowCount; ++i)
		{
			ActivityTalkData data = new ActivityTalkData
			{
				ID = table.GetValue<long>("ID", i),
				Talk_Group_ID = table.GetValue<long>("Talk_Group_ID", i),
				Avatar_Type = (AVATAR_TYPE)table.GetValue<byte>("Member_Type", i),
				Talk_String = table.GetValue<long>("Talk_String", i),
				Talk_Time = table.GetValue<byte>("Talk_Time", i)
            };


			if (data.Avatar_Type != 0)
			{
				if (ActivityTalkDataDic.TryGetValue(data.Talk_Group_ID, out var avatarDic) == false)
				{
					avatarDic = new Dictionary<AVATAR_TYPE, List<ActivityTalkData>>();
					ActivityTalkDataDic.Add(data.Talk_Group_ID, avatarDic);
				}
				if (avatarDic.TryGetValue(data.Avatar_Type, out var avatarTalkList) == false)
				{
					avatarTalkList = new List<ActivityTalkData>();
					avatarDic.Add(data.Avatar_Type, avatarTalkList);
				}
				avatarTalkList.Add(data);
			}

			if(CharTalkDataDic.ContainsKey(data.Talk_Group_ID) == false)
			{
                CharTalkDataDic.Add(data.Talk_Group_ID, new List<ActivityTalkData>());
				CharTalkDataDic[data.Talk_Group_ID].Add(data);
            }
			else
			{
                CharTalkDataDic[data.Talk_Group_ID].Add(data);
            }
		}
	}

	private void _LoadBuildingSkinData()
	{
		DataTable table = CDataManager.GetTable(ETableDefine.TABLE_MANAGEMENT_SKIN);
		if (null == table)
		{
			CDebug.LogError(string.Format("Not Found Table : [{0}]", ETableDefine.TABLE_MANAGEMENT_SKIN));
			return;
		}


        if (BuildingSkinDataDic != null)
            BuildingSkinDataDic.Clear();
        else
            BuildingSkinDataDic = new Dictionary<long, BuildingSkinData>();


        for (int i = 0; i < table.RowCount; ++i)
		{
			BuildingSkinData data = new BuildingSkinData
			{
				ID = table.GetValue<long>("ID", i),
				Skin_Type = table.GetValue<byte>("Skin_Type", i),
				Order = table.GetValue<byte>("Order", i),
				Enable = table.GetValue<byte>("Enable", i),
				Skin_Name = table.GetValue<long>("Skin_Name", i),
				Skin_Desc = table.GetValue<long>("Skin_Desc", i),
				Skin_Icon = table.GetValue<string>("Skin_Icon", i),
				Skin_Resources = table.GetValue<string>("Skin_Resources", i),
				Req_Type = table.GetValue<long>("Req_Type", i),
				Req_Value1 = table.GetValue<long>("Req_Value1", i),
				Req_Value2 = table.GetValue<long>("Req_Value2", i),
				Req_Value3 = table.GetValue<long>("Req_Value3", i),
				Req_String = table.GetValue<long>("Req_String", i),
				Consume_Type = table.GetValue<long>("Consume_Type", i),
				Consume_Value1 = table.GetValue<long>("Consume_Value1", i),
				Consume_Value2 = table.GetValue<long>("Consume_Value2", i),
			};

			if (!BuildingSkinDataDic.ContainsKey(data.ID))
			{
				BuildingSkinDataDic.Add(data.ID, data);
			}
			else
			{
				CDebug.LogError(string.Format($"{ETableDefine.TABLE_MANAGEMENT_SKIN} ID:[ {data.ID} ] is already contain data"));
			}
		}
	}

    // 트로피 테이블 로드
    private void _loadTrophyData ()
    {
        DataTable table = CDataManager.GetTable (ETableDefine.TABLE_MANAGEMENT_TROPHY);
        if (null == table)
        {
            CDebug.LogError (string.Format ("Not Found Table : [{0}]", ETableDefine.TABLE_MANAGEMENT_TROPHY));
            return;
        }

        TrophyDic?.Clear ();
        TrophyDic = new Dictionary<long, TrophyTableData> ();

        for (int i = 0; i < table.RowCount; ++i)
        {
            TrophyTableData rawData = new TrophyTableData
            {
                ID = table.GetValue<long> ("ID", i),
                Trophy_Name = table.GetValue<long> ("Trophy_Name", i),
                Trophy_Desc = table.GetValue<long> ("Trophy_Desc", i),
                Trophy_icon = table.GetValue<string> ("Trophy_icon", i),
                Trophy_Resource = table.GetValue<string> ("Trophy_Resource", i),
                Get_String = table.GetValue<long> ("Get_String", i),
            };
            if (!TrophyDic.ContainsKey (rawData.ID))
            {
                TrophyDic.Add (rawData.ID, rawData);
            }
            else
            {
                CDebug.LogError (string.Format ($"{ETableDefine.TABLE_MANAGEMENT_TROPHY} ID:[ {rawData.ID} ] is already contain data"));
            }
        }
    }

    // 어치브(업적) 테이블 로드
    private void _loadAchieveData ()
	{
		DataTable table = CDataManager.GetTable (ETableDefine.TABLE_MANAGEMENT_ACHIEVE);
		if (null == table)
		{
			CDebug.LogError (string.Format ("Not Found Table : [{0}]", ETableDefine.TABLE_MANAGEMENT_ACHIEVE));
			return;
		}

		AchieveTableDic?.Clear ();
		AchieveGroupTableDic?.Clear ();
		AchieveTableDic = new Dictionary<long, AchieveTableData> ();
		AchieveGroupTableDic = new Dictionary<long, Dictionary<byte, AchieveTableData>> ();

		for (int i = 0; i < table.RowCount; ++i)
		{
			AchieveTableData rawData = new AchieveTableData
			{
				ID = table.GetValue<long> ("ID", i),
				Achieve_Title = table.GetValue<long> ("Achieve_Title", i),
				Title_Icon = table.GetValue<string> ("Title_Icon", i),
				Group_ID = table.GetValue<long> ("Group_ID", i),
				Group_Sequence = table.GetValue<byte> ("Group_Sequence", i),
				Goal_String = table.GetValue<long> ("Goal_String", i),
				Goal_Req_Type = table.GetValue<long> ("Goal_Req_Type", i),
				Goal_Req_Value1 = table.GetValue<long> ("Goal_Req_Value1", i),
				Goal_Req_Value2 = table.GetValue<long> ("Goal_Req_Value2", i),
				Goal_Req_Value3 = table.GetValue<long> ("Goal_Req_Value3", i),
				Achieve_Point = table.GetValue<long> ("Achieve_Point", i),
				Reward_Check = table.GetValue<long> ("Reward_Check", i),
				Reward = table.GetValue<long> ("Reward", i),
			};
			// CRequire 추가
			rawData.GoalReq = new CRequire ()
			{
				Type = (REQUIRE_TYPES)rawData.Goal_Req_Type,
				Value1 = rawData.Goal_Req_Value1,
				Value2 = rawData.Goal_Req_Value2,
				Value3 = rawData.Goal_Req_Value3,
			};

            // enum 설정
            rawData.achieveRewardCheck = (AchieveTableData.AchieveRewardCheck)rawData.Reward_Check;
            rawData.achieveReward = (AchieveTableData.AchieveReward)rawData.Reward_Check;

			// reward 유효성 검사
            switch (rawData.achieveReward)
            {
                case AchieveReward.RewardTable:
                    if (CRewardConsumeDataManager.Instance.GetRewardTable (rawData.Reward) == null)
                    {
                        InvalidRewardLog ();
                    }
                    break;
                case AchieveReward.TrophyTable:
                    if (TrophyDic.ContainsKey (rawData.Reward) == false)
                    {
                        InvalidRewardLog ();
                    }
                    break;
            }
            void InvalidRewardLog ()
            {
                CDebug.LogError (string.Format ($"{ETableDefine.TABLE_MANAGEMENT_ACHIEVE} ID:[ {rawData.ID} ] {rawData.achieveReward} reward is invalid (RewardID : {rawData.Reward})"));

            }

            if (!AchieveTableDic.ContainsKey (rawData.ID))
			{
				AchieveTableDic.Add (rawData.ID, rawData);
			}
			else
			{
				CDebug.LogError (string.Format ($"{ETableDefine.TABLE_MANAGEMENT_ACHIEVE} ID:[ {rawData.ID} ] is already contain data"));
			}

			// 업적 테이블을 그룹ID, 시퀸스로 정리
            if (AchieveGroupTableDic.TryGetValue(rawData.Group_ID, out var innerDic) == false)
            {
                innerDic = new Dictionary<byte, AchieveTableData> ();
                AchieveGroupTableDic.Add (rawData.Group_ID, innerDic);
            }
            if (innerDic.TryAdd (rawData.Group_Sequence, rawData) == false)
            {
                CDebug.LogError (string.Format ($"{ETableDefine.TABLE_MANAGEMENT_ACHIEVE} ID:[ {rawData.ID} ] is already contain Group_Sequence [ {rawData.Group_Sequence} ]"));
            }
		}
	}


    private void _LoadAvatarInteractionData()
	{
		DataTable table = CDataManager.GetTable(ETableDefine.TABLE_MANAGEMENT_AVATAR_INTERACTION);
		if (null == table)
		{
			CDebug.LogError(string.Format("Not Found Table : [{0}]", ETableDefine.TABLE_MANAGEMENT_AVATAR_INTERACTION));
			return;
		}


		if (AvatarInteractionDataDic != null)
			AvatarInteractionDataDic.Clear();
		else
			AvatarInteractionDataDic = new Dictionary<AVATAR_TYPE, List<AvatarInteractionData>>();


		for (int i = 0; i < table.RowCount; ++i)
		{
			AvatarInteractionData _data = new AvatarInteractionData();
			_data.ID = table.GetValue<long>("ID", i);
			_data.Main_AvatarType = (AVATAR_TYPE)table.GetValue<long>("Main_Avatar_Type", i);
			//_data.Main_InteractionAni = table.GetValue<string>("Main_Interaction_Ani", i);
			_data.Main_Interaction_MotionID = table.GetValue<long>("Main_Interaction_MotionID", i);
			_data.Main_InteractionStr[0] = table.GetValue<long>("Main_Interaction_String1", i);
			_data.Main_InteractionStr[1] = table.GetValue<long>("Main_Interaction_String2", i);
			_data.Main_InteractionStr[2] = table.GetValue<long>("Main_Interaction_String3", i);
			_data.Main_InteractionStr[3] = table.GetValue<long>("Main_Interaction_String4", i);

			for (int subIdx = 0; subIdx < 4; subIdx++) 
			{
				int subid = subIdx + 1;
				_data.SubInteractionInfos[subIdx] = new AvatarSubInteractionInfo();
				//_data.SubInteractionInfos[subIdx].Interaction_Ani = table.GetValue<string>($"Sub{subid}_Interaction_Ani", i);
				_data.SubInteractionInfos[subIdx].Interaction_MotionID = table.GetValue<long>($"Sub{subid}_Interaction_MotionID", i);
				_data.SubInteractionInfos[subIdx].Interaction_Str = table.GetValue<long>($"Sub{subid}_Interaction_String", i);
			}

			_data.InteractionRate = (int)table.GetValue<long>("Interaction_Rate", i);
			_data.RewardID = table.GetValue<long>("Reward_ID", i);
			_data.FinishTime = (int)table.GetValue<long>("End_Time", i);
			_data.Sub_Delay_Time = table.GetValue<long>("Sub_Delay_Time", i);

			if (AvatarInteractionDataDic.ContainsKey(_data.Main_AvatarType) == false)
            {
				AvatarInteractionDataDic.Add(_data.Main_AvatarType, new List<AvatarInteractionData>());
				AvatarInteractionDataDic[_data.Main_AvatarType].Add(_data);
			}
			else
			{
				AvatarInteractionDataDic[_data.Main_AvatarType].Add(_data);
			}
		}

	}
	#endregion LOAD_DATA


	#region GET

	// Section Data
	public SectionTableData GetSectionTableData(long sectiongdid)
    {
		if(SectionTableDataDic.ContainsKey(sectiongdid))
        {
			return SectionTableDataDic[sectiongdid];
        }
		return null;
    }

	public SectionTableData GetSectionTableDataBySectionType( byte sectionType )
	{
		SectionTableData sectionData = null;

		foreach( KeyValuePair<long, SectionTableData> item in SectionTableDataDic )
		{
			if( item.Value.Section_Type == sectionType )
			{
				sectionData = item.Value;
				break;
			}
		}

		return sectionData;
	}

	public SectionTableData GetSectionTableDataBySectionTypeAndSubType( byte sectionType, byte sectionSubType )
	{
		SectionTableData sectionData = null;

		foreach( KeyValuePair<long, SectionTableData> item in SectionTableDataDic )
		{
			if( item.Value.Section_Type == (byte)sectionType && item.Value.Section_Sub_Type == sectionSubType )
			{
				sectionData = item.Value;
				break;
			}
		}

		return sectionData;
	}

	public Vector2 GetSectionSize(long sectinID, int level)
    {
		List<SectionLevelTableData> _list = SectionLevelTableDataDic[sectinID];

		for(int i=0; i<_list.Count; ++i)
        {
			if(_list[i].Section_LV == level)
            {
				Vector2 gi = new Vector2(_list[i].Section_Size_X, _list[i].Section_Size_Y);
				return gi;
            }
        }

		return Vector2.one;
	}

	public SectionLevelTableData GetSectionLevelData(long sectionGDID, int level)
	{
		List<SectionLevelTableData> _list = SectionLevelTableDataDic[sectionGDID];

		for (int i = 0; i < _list.Count; ++i)
		{
			if (_list[i].Section_LV == level)
			{
				return _list[i];
			}
		}

		return null;
	}

	public Dictionary<int, SectionLevelTableData> GetSectionLevelDataList(long sectionGDID)
	{
		if (SectionLevelTableDataDic.ContainsKey(sectionGDID) == false)
			return null;

		Dictionary<int, SectionLevelTableData> dicLevelData = new Dictionary<int, SectionLevelTableData>();

		foreach (SectionLevelTableData data in SectionLevelTableDataDic[sectionGDID])
		{
			if (dicLevelData.ContainsKey(data.Section_LV))
			{
				CDebug.LogError($"{ETableDefine.TABLE_MANAGEMENT_SECTION_LEVEL} : Section_ID - {data.Section_ID} , Section_LV - {data.Section_LV}  already key");
			}
			else
			{
				dicLevelData.Add(data.Section_LV, data);
			}


		}

		return dicLevelData;
	}


    public int GetSectionMaxLevel(long sectionID)
    {
		List<SectionLevelTableData> _list = SectionLevelTableDataDic[sectionID];

		if(_list != null && _list.Count>0)
        {
			_list = _list.OrderByDescending(x => x.Section_LV).ToList();
			return _list[0].Section_LV;
		}

		return 0;
	}

	public SectionCreationTableData GetSectionCreationData(long sectionGDID, int currentCount)
	{
		List<SectionCreationTableData> _list = SectionCreationTableDataDic[sectionGDID];
		_list = _list.OrderBy(x => x.Section_ID_No).ToList();

		for (int i = 0; i < _list.Count; ++i)
		{
			if (_list[i].Section_ID_No == currentCount)
			{
				return _list[i];
			}
		}
		return _list[_list.Count - 1];
	}

	public SectionCreationTableData GetSectionCurrentCreationData(long sectionGDID, int currentCount)
    {
		List<SectionCreationTableData> _list = SectionCreationTableDataDic[sectionGDID];
		_list = _list.OrderBy(x => x.Section_ID_No).ToList();

		for (int i = 0; i < _list.Count; ++i)
		{
			if (_list[i].Section_ID_No > currentCount)
			{
				return _list[i];
			}
		}
		return _list[_list.Count-1];
	}

	public SectionCreationTableData GetSectionPossibleCreationData(long sectionID)
	{
		List<SectionCreationTableData> _list = SectionCreationTableDataDic[sectionID];
		_list = _list.OrderByDescending(x => x.Section_ID_No).ToList();

		for (int i = 0; i < _list.Count; ++i)
		{
			if(CRequireManager.CheckConditionAllRequire(_list[i].Requires))
			{
				return _list[i];
			}
		}
		return _list[_list.Count-1];
	}

	/// <summary>
	///  프로듀서부서의 레벨값으로 생성 가능한 부서 가져오기
	/// </summary>
	/// <param name="Producer_Lv"></param>
	/// <returns></returns>
	public List<long> GetSectionPossibleCreationData(int Producer_Lv)
	{
		List<long> _list = new List<long>();
		foreach (KeyValuePair<long, List<SectionCreationTableData>> kvp in SectionCreationTableDataDic)
        {
			foreach(SectionCreationTableData data in kvp.Value)
            {
				if(data.Producer_Lv == Producer_Lv)
				{
                    if (!_list.Contains(data.Section_ID))
					{
                    	_list.Add(data.Section_ID);
					}
				}
			}
		}

		return _list;
	}

	public List<long> GetSectionPossibleCreationData(CRequire findReq)
	{
		List<long> _list = new List<long>();
		foreach (KeyValuePair<long, List<SectionCreationTableData>> kvp in SectionCreationTableDataDic)
        {
			foreach(SectionCreationTableData data in kvp.Value)
            {
				foreach (CRequire req in data.Requires)
                {
					//if (req.Equal(findReq) && _list.Contains(data.Section_ID) == false)
					//	_list.Add(data.Section_ID);

					if (req.Type == REQUIRE_TYPES.SECTION && req.Value1 == findReq.Value1 && req.Value2 == findReq.Value2+1)	
                    {
						_list.Add(data.Section_ID);
						break;
					}
				}
				if (_list.Contains(data.Section_ID))
					break;
			}
		}

		return _list;
	}


    /// <summary>
    ///  프로듀서부서의 레벨값으로 생성 가능한 부서 가져오기
    /// </summary>
    /// <param name="Producer_Lv"></param>
    /// <returns></returns>
    public List<long> GetSectionPossibleCreationDataByCurrentPDRoomLevel(int Producer_Lv)
	{
		List<long> _list = new List<long>();
		foreach (KeyValuePair<long, List<SectionCreationTableData>> kvp in SectionCreationTableDataDic)
		{
			foreach (SectionCreationTableData data in kvp.Value)
			{
				if(data.Producer_Lv == Producer_Lv)
				{
					if (!_list.Contains(data.Section_ID))
					{
						_list.Add(data.Section_ID);
					}
				}
			}
		}

		return _list;
	}

	// public List<long> GetSectionPossibleCreationDataByCurrentPDRoomLevel(CRequire findReq)
	// {
	// 	List<long> _list = new List<long>();
	// 	foreach (KeyValuePair<long, List<SectionCreationTableData>> kvp in SectionCreationTableDataDic)
	// 	{
	// 		foreach (SectionCreationTableData data in kvp.Value)
	// 		{
	// 			foreach (CRequire req in data.Requires)
	// 			{
	// 				if (req.Type == REQUIRE_TYPES.SECTION && req.Value1 == findReq.Value1 && req.Value2 == findReq.Value2)
	// 				{
	// 					_list.Add(data.Section_ID);
	// 					break;
	// 				}
	// 			}
	// 			if (_list.Contains(data.Section_ID))
	// 				break;
	// 		}
	// 	}

	// 	return _list;
	// }


	public List<long> GetSectionPossibleUpgradeData(CRequire findReq)
	{
		List<long> _list = new List<long>();
		foreach (KeyValuePair<long, List<SectionLevelTableData>> kvp in SectionLevelTableDataDic)
		{
			foreach (SectionLevelTableData data in kvp.Value)
			{
				foreach (CRequire req in data.Requires)
				{
					if (req.Type == REQUIRE_TYPES.SECTION && req.Value1 == findReq.Value1 && req.Value2 == findReq.Value2 + 1)
					{
						_list.Add(data.Section_ID);
						break;
					}
				}
				if (_list.Contains(data.Section_ID))
					break;
			}
		}

		return _list;
	}
    /// <summary>
    ///  프로듀서부서의 레벨값으로 업그레이드 가능한 부서 가져오기
    /// </summary>
    /// <param name="Producer_Lv"></param>
    /// <returns></returns>
    public List<long> GetSectionPossibleUpgradeDataByCurrentPDRoomLevel(int Producer_Lv)
	{
		List<long> _list = new List<long>();
		foreach (KeyValuePair<long, List<SectionLevelTableData>> kvp in SectionLevelTableDataDic)
		{
			foreach (SectionLevelTableData data in kvp.Value)
			{
                if (data.Producer_Lv == Producer_Lv)
                {
                    if (!_list.Contains(data.Section_ID))
                    {
                        _list.Add(data.Section_ID);
                    }
                }
			}
		}

		return _list;
	}

	// public List<long> GetSectionPossibleUpgradeDataByCurrentPDRoomLevel(CRequire findReq)
	// {
	// 	List<long> _list = new List<long>();
	// 	foreach (KeyValuePair<long, List<SectionLevelTableData>> kvp in SectionLevelTableDataDic)
	// 	{
	// 		foreach (SectionLevelTableData data in kvp.Value)
	// 		{
	// 			foreach (CRequire req in data.Requires)
	// 			{
	// 				if (req.Type == REQUIRE_TYPES.SECTION && req.Value1 == findReq.Value1 && req.Value2 == findReq.Value2)
	// 				{
	// 					_list.Add(data.Section_ID);
	// 					break;
	// 				}
	// 			}
	// 			if (_list.Contains(data.Section_ID))
	// 				break;
	// 		}
	// 	}

	// 	return _list;
	// }

	public List<SectionTableData> GetSectionTableDataListBySectionType(ENUM_SECTION_TYPE sectiontype)
    {
        List<SectionTableData> _list = new List<SectionTableData>();

        foreach (KeyValuePair<long, SectionTableData> item in SectionTableDataDic)
        {
            if ((ENUM_SECTION_TYPE)item.Value.Section_Type == sectiontype && item.Value.List_On == 1)
            {
                _list.Add(item.Value);
            }
        }
        return _list.Count > 0 ? _list : null;
    }

    //============================================//
    // Activity data
    public ActivityData GetActivityTrainingData (long activitygroupid, long statlv)
    {
        if (ActivityDataDic.ContainsKey (activitygroupid))
        {
            byte activitytype = (byte)ActivityDataDic[activitygroupid][0].Activity_Type;
            byte activitysubtype = (byte)ActivityDataDic[activitygroupid][0].Activity_Sub_Type;

            foreach (KeyValuePair<long, List<ActivityData>> data in ActivityDataDic)
            {
                if (activitytype == data.Value[0].Activity_Type
                    && activitysubtype == data.Value[0].Activity_Sub_Type)
                {
                    for (int i = 0; i < data.Value.Count; i++)
                    {
                        if (data.Value[i].Stat_LV == statlv)
                        {
                            return data.Value[i];
                        }
                    }
                }
            }
        }

        return null;
    }

	// groupid, type, sub_type 으로 데이터 찾기 
	public ActivityData GetActivityPhotoStudioData(long activitygroupid, long activitySubType)
	{
		try
		{
			if (ActivityDataDic.ContainsKey(activitygroupid))
			{
				var search = ActivityDataDic[activitygroupid]
					.Where(data => data.Activity_Sub_Type == activitySubType)
					.FirstOrDefault();
				return search;
			}
			return null;
		}
		catch
		{
			return null;
		}
	}

	public ActivityData GetActivityData (long activitygroupid, long statlv)
    {
        if (ActivityDataDic.ContainsKey (activitygroupid))
        {
            for (int i = 0; i < ActivityDataDic[activitygroupid].Count; i++)
            {
                if (ActivityDataDic[activitygroupid][i].Stat_LV == statlv)
                {
                    return ActivityDataDic[activitygroupid][i];
                }
            }
        }

        return null;
    }

    public ActivityData GetActivityData(long activitygdid)
	{
		foreach(KeyValuePair<long, List<ActivityData>> data in ActivityDataDic)
        {
			for(int i=0; i<data.Value.Count; i++)
            {
				if(data.Value[i].Activity_ID == activitygdid)
                {
					return data.Value[i];
                }
            }
        }

		return null;
    }

    // groupid, type, sub_type 으로 데이터 찾기 
    public ActivityData GetActivityData (long activitygroupid, ACTIVITY_TYPE activityType, long activitySubType, long statLv = 0)
    {
        try
        {
            if (ActivityDataDic.ContainsKey (activitygroupid))
            {
                var search = ActivityDataDic[activitygroupid]
                    .Where (data => statLv == 0 ?
                        data.ACTIVITY_TYPE == activityType && data.Activity_Sub_Type == activitySubType :
                        data.ACTIVITY_TYPE == activityType && data.Activity_Sub_Type == activitySubType && data.Stat_LV == statLv)
                    .FirstOrDefault ();
                return search;
            }
            return null;
        }
        catch
        {
            return null;
        }
    }

    //============================================//
    // activity 보상 그룹 찾기
    public (RewardTable rewardRow, List<RewardGroupTable> rewardGroupRowList) GetAvtivityReward (long activitygroupid, ACTIVITY_TYPE activityType, long activitySubType)
    {
        var activityData = GetActivityData (activitygroupid, activityType, activitySubType);
		if (activityData != null)
        {
            var rewardRow = CRewardConsumeDataManager.Instance.GetRewardTable (activityData.Reward_ID);
			if (rewardRow != null)
            {
                var rewardGroupRowList = CRewardConsumeDataManager.Instance.GetRewardGroup (rewardRow.Normal_Group_ID);
				if (rewardGroupRowList != null)
				{
					return (rewardRow, rewardGroupRowList);
                }
            }
        }
		return default;
    }
    // activity 보상 첫번쨰 찾기
    public (RewardTable rewardRow, CReward reward) GetAvtivityRewardFirst (long activitygroupid, ACTIVITY_TYPE activityType, long activitySubType)
    {
        var rewardData = GetAvtivityReward (activitygroupid, activityType, activitySubType);
		if (rewardData.rewardRow == null)
		{
			return default;
		}
        return (rewardData.rewardRow, rewardData.rewardGroupRowList.FirstOrDefault ()?.Value);
    }
    //============================================//

    public ACTIVITY_TYPE GetActivityType(long activitygroupid)
	{
		if (ActivityDataDic.ContainsKey(activitygroupid))
		{
			return (ACTIVITY_TYPE)(ActivityDataDic[activitygroupid][0].Activity_Type);
		}

		return ACTIVITY_TYPE.NONE;
	}

	// STAT_TYPE은 0부터 Activity_Sub_Type 은 1부터 사용중이라 받는 곳에서 추가 처리 필요.
	public int GetActivitySubType(long activitygroupid)
	{
		if (ActivityDataDic.ContainsKey(activitygroupid))
		{
			return (int)(ActivityDataDic[activitygroupid][0].Activity_Sub_Type);
		}

		return -1;
	}

	// 트레이닝 룸에 배치될 트레이너의 아이디 가져오기
	public long GetActivityTrainerID(long activitygroupid)
	{
		if (ActivityDataDic.ContainsKey(activitygroupid))
		{
			return (int)(ActivityDataDic[activitygroupid][0].NPC_ID);
		}

		return -1;
	}

	public int GetActivityStatMaxLevelInActivityGroup(long activitygroupid)
    {
		if (ActivityDataDic.ContainsKey(activitygroupid))
		{
			List<ActivityData> lstActivityData = ActivityDataDic[activitygroupid].OrderByDescending(x => x.Stat_LV).ToList();
			return (int)lstActivityData[0].Stat_LV;
		}

		return -1;
	}

	public int GetActivityStatMinLevelInActivityGroup(long activitygroupid)
	{
		if (ActivityDataDic.ContainsKey(activitygroupid))
		{
			List<ActivityData> lstActivityData = ActivityDataDic[activitygroupid].OrderBy(x => x.Stat_LV).ToList();
			return (int)lstActivityData[0].Stat_LV;
		}

		return -1;
    }

	public long GetActivityNPCIDByActivityGroupID(long activitygroupid)
    {
		if(ActivityDataDic.ContainsKey(activitygroupid))
        {
			return ActivityDataDic[activitygroupid][0].NPC_ID;
        }

		return -1;
    }

    //public TraininglistData GetTrainingData(long groupid, long trainingid)
    //{
    //	foreach (KeyValuePair<long, List<TraininglistData>> item in TraininglistDataDic)
    //	{
    //		if (item.Key == groupid)
    //		{
    //			for(int i=0; i<item.Value.Count; i++)
    //               {
    //				if(item.Value[i].Training_ID == trainingid)
    //                   {
    //					return item.Value[i];
    //                   }
    //               }
    //		}
    //	}
    //	return null;
    //}

    //public TraininglistData GetTrainingData(long trainingid)
    //{
    //	foreach (KeyValuePair<long, List<TraininglistData>> item in TraininglistDataDic)
    //	{
    //		for (int i = 0; i < item.Value.Count; i++)
    //		{
    //			if (item.Value[i].Training_ID == trainingid)
    //			{
    //				return item.Value[i];
    //			}
    //		}
    //	}
    //	return null;
    //}

    //public List<TraininglistData> GetTrainingGroupList(long traininggroupid)
    //{
    //	foreach (KeyValuePair<long, List<TraininglistData>> item in TraininglistDataDic)
    //	{
    //		if (item.Key == traininggroupid)
    //		{
    //			return item.Value;
    //		}
    //	}
    //	return null;
    //}

    public List<string> GetAllAvatarActivityBTPath()
    {
		List<string> _list = new List<string>(ActivityAvatarBT.Keys);

		return _list;
	}

	//public float GetTrainingTime(long traininggroupid)
	//{
	//	var data = GetTrainingGroupList(traininggroupid);
	//	if(data != null)
	//	{
	//		for(int i = 0; i < data.Count; ++i)
	//		{
	//			return data[i].Trainer_Time;
	//		}
	//	}
	//	return 0.0f;
	//}

	//public List<TraininglistData> GetTrainingGroupListByTrainingType(long traininggroupid, ENUM_TRAINING_TYPES type)
	//{
	//	foreach (KeyValuePair<long, List<TraininglistData>> item in TraininglistDataDic)
	//	{
	//		if (item.Key == traininggroupid)
	//		{
	//			List<TraininglistData> listTraining = new List<TraininglistData>();
	//			for(int i=0; i<item.Value.Count; i++)
 //               {
	//				if((ENUM_TRAINING_TYPES)item.Value[i].Training_Type == type)
 //                   {
	//					listTraining.Add(item.Value[i]);
 //                   }
 //               }
	//			return listTraining;
	//		}
	//	}
	//	return null;
	//}
    //============================================//

	public List<ActivityTalkData> GetActivityTalkList(AVATAR_TYPE mType, long talkgroupid)
    {
        return ActivityTalkDataDic?[talkgroupid]?[mType];
    }

	public ActivityTalkData GetActivityTalkData(AVATAR_TYPE mType, long talkgroupid)
    {
        var list = ActivityTalkDataDic?[talkgroupid]?[mType];
		if (list != null && list.Count != 0)
		{
            return list[Random.Range (0, list.Count)];
        }
		return null;
	}

	public BuildingSkinData GetBuildingSkinData(long skinid)
	{
		if (BuildingSkinDataDic.ContainsKey(skinid))
		{
			return BuildingSkinDataDic[skinid];
		}
		return null;
	}

	public List<BuildingSkinData> GetBuildingSkinDataSet(byte order) // order => pd room level
	{
		List<BuildingSkinData> _list = new List<BuildingSkinData>();
		foreach (KeyValuePair<long, BuildingSkinData> item in BuildingSkinDataDic)
		{
			if (item.Value.Order == order)
			{
				_list.Add(item.Value);
			}
		}

		return _list;
	}

	public List<BuildingSkinData> GetCurrentLevelBuildingSkinDataSet(byte order) // order => pd room level
	{
		var queryDesc = BuildingSkinDataDic.OrderByDescending(d => d.Value.Order);

		int currentOrder = -1;
		foreach(var item in queryDesc)
        {
			if(item.Value.Order <= order)
            {
				currentOrder = item.Value.Order;
				break;
            }
        }

		if (currentOrder == -1)
			return null;

        List<BuildingSkinData> _list = new List<BuildingSkinData>();
        foreach (KeyValuePair<long, BuildingSkinData> item in BuildingSkinDataDic)
        {
            if (item.Value.Order == currentOrder)
            {
                _list.Add(item.Value);
            }
        }

        return _list;
    }

	public byte GetMaxBuildingSkinDataOrder()
    {
		return BuildingSkinDataDic.Values.Max(p => p.Order);
	}

	// 모든 업적 테이블의 dic 가져오기 <ID, data>
	public IReadOnlyDictionary<long, AchieveTableData> GetAchiveTableDic ()
	{
		return AchieveTableDic;
	}
	// 모든 업적의 그룹 dic 가져오기 <Group_ID, <Group_Sequence, data>>
	public IReadOnlyDictionary<long, Dictionary<byte, AchieveTableData>> GetAchiveGroupTableDic ()
	{
		return AchieveGroupTableDic;
	}
	// 트로피 dic 가져오기 <ID, data>
	public IReadOnlyDictionary<long, TrophyTableData> GetTrophyTableDic ()
	{
		return TrophyDic;
	}

	public float GetYgBuildingExtensionOpenTime(int nextFloor)
	{
		foreach (KeyValuePair<long, YgBuildingData> item in YgBuildingDataDic)
		{
			if (nextFloor == int.Parse(item.Value.Floors))
			{
				return item.Value.Open_Time;
			}
		}

		return 0.0f;
	}

	public List<CRequire> GetYgBuildingRequireByNextFloor(int nextFloor)
    {
		foreach(KeyValuePair<long, YgBuildingData> item in YgBuildingDataDic)
        {
			if(nextFloor == int.Parse(item.Value.Floors))
            {
				return item.Value.Requires;
            }
        }


		return null;
	}

	public CConsume[] GetYgBuildingConsumeByNextFloor(int nextFloor)
	{
		foreach (KeyValuePair<long, YgBuildingData> item in YgBuildingDataDic)
        {
			if (nextFloor == int.Parse(item.Value.Floors))
            {
				return item.Value.Consume;
            }
		}

		return null;
	}

	public long GetProductTargetGDID(ENUM_SECTION_SUBTYPE_PRODUCT targetsectionsubtype, long itemid)
    {
		long resultID = 0;
		foreach(KeyValuePair<long, SectionTableData> item in SectionTableDataDic)
        {
			SectionTableData data = item.Value;
			if (data.Section_Type == (byte)ENUM_SECTION_TYPE.Production 
				&& data.Section_Sub_Type == (byte)targetsectionsubtype
				&& data.GetDefaultLevelData().Function_Value == itemid)
            {
				resultID = data.ID;
            }
        }

		return resultID;
    }


	public AvatarInteractionData GetAvatarInteractionDataByRandomValue(AVATAR_TYPE avatarType)
    {
		List<AvatarInteractionData> _list = AvatarInteractionDataDic[avatarType];

		int total = 0;
		for (int i = 0; i < _list.Count; ++i)
		{
			total += _list[i].InteractionRate;
		}

		int randomValue = UnityEngine.Random.Range(0, total);

		int selectIdx = 0;
		for (int i = 0; i < _list.Count; ++i)
		{
			if (randomValue <= _list[i].InteractionRate)
			{
				selectIdx = i;
				break;
			}
			else
			{
				randomValue -= _list[i].InteractionRate;
			}
		}

		return _list[selectIdx];

	}

    //public List<ActivityTalkData> GetTalkDataGroupByCharID(long charID)
    //{
    //	if(CharTalkDataDic.ContainsKey(charID)) 
    //	{ 
    //		return CharTalkDataDic[charID];
    //       }

    //	return null;
    //}
    public List<ActivityTalkData> GetTalkDataGroupByCharID(long groupID)
    {
        if (CharTalkDataDic.ContainsKey(groupID))
        {
            return CharTalkDataDic[groupID];
        }

        return null;
    }

    public ActivityTalkData GetTalkDataRandomly(long groupID)
	{
		List<ActivityTalkData> grpData = GetTalkDataGroupByCharID(groupID);

		int pickIndex = UnityEngine.Random.Range(0, grpData.Count);

		return grpData[pickIndex];
    }

	public ActivityTalkData GetTalkDataRandomly(long groupID, AVATAR_TYPE avatarType)
	{
		List<ActivityTalkData> grpData = GetTalkDataGroupByCharID(groupID)?.Where(d => d.Avatar_Type == avatarType).ToList();

		int pickIndex = UnityEngine.Random.Range(0, grpData.Count);

		return grpData[pickIndex];
    }
    #endregion GET


    #region Reddot Timer
    SingleAssignmentDisposable disposer_management_reddot;

	public void aaa(ManagementMapResponseData mapinfo, 
		List<ManagementSectionDetailResData> sectioninfos)
	{
        CDebug.Log($"&&&^^^%%% before reddot set");
        // 매니지먼트 레드닷 타이머 설정
        if (UIManager.HasClientReddot(UIContentType.MANAGEMENT) == false)
        {
            long min_remain_milseconds = 0;

            // 증축 진행 중일 경우 남은 시간을 구한다.
            if (mapinfo != null && mapinfo.remain_mts > 0)
            {
                CDebug.Log($"&&&^^^%%% mapinfo.remain_mts : {mapinfo.remain_mts} - 증축");
                min_remain_milseconds = mapinfo.remain_mts;
            }

            // 생산 부서 생산 시간 체크
            if (sectioninfos != null && sectioninfos.Count > 0)
            {
                for (int i = 0; i < sectioninfos.Count; i++)
                {
                    // 부서 신설, 업그레이드 진행 중인 부서들 중 최소 시간을 구한다. : status, remain_mts
                    if ((ENUM_BUILD_STATE)sectioninfos[i].status == ENUM_BUILD_STATE.START
                        || (ENUM_BUILD_STATE)sectioninfos[i].status == ENUM_BUILD_STATE.UPGRADE_START)
                    {
                        CDebug.Log($"&&&^^^%%% sectioninfos[{i}].remain_mts : {sectioninfos[i].remain_mts} - 부서 공사");
                        if (min_remain_milseconds > 0)
                        {
                            min_remain_milseconds = min_remain_milseconds > sectioninfos[i].remain_mts ? sectioninfos[i].remain_mts : min_remain_milseconds;
                        }
                        else { min_remain_milseconds = sectioninfos[i].remain_mts; }
                        continue;
                    }
                    else if (/*(ENUM_BUILD_STATE)sectioninfos[i].status == ENUM_BUILD_STATE.NOT_START ||*/
                    (ENUM_BUILD_STATE)sectioninfos[i].status == ENUM_BUILD_STATE.COMPLETE)
                    {
                        SectionTableData sectiontd = ManagementDataManager.Instance.GetSectionTableData(sectioninfos[i].gdid);
                        SectionLevelTableData sectionlvtd = sectiontd.GetLevelData(sectioninfos[i].level);

                        if (sectiontd == null)
                        {
                            CDebug.LogError("Not exist section table data!!");
                            continue;
                        }

                        // 재화 생산등이 진행 중인 부서들 중 최소 시간을 구한다. : storage, table정보 중 단위 생산량, 최대 저장량
                        if (sectiontd.Section_Type == (byte)ENUM_SECTION_TYPE.Production
                        && (sectiontd.Section_Sub_Type == (byte)ENUM_SECTION_SUBTYPE_PRODUCT.PRODUCTION                     // 음반
                        || sectiontd.Section_Sub_Type == (byte)ENUM_SECTION_SUBTYPE_PRODUCT.PRODUCTION_CAN_CHANGE_PRODUCT   // 별가루, 스케쥴 아이템
                        || sectiontd.Section_Sub_Type == (byte)ENUM_SECTION_SUBTYPE_PRODUCT.PRODUCTION_GOODS_SHOP           // 골드
                        || sectiontd.Section_Sub_Type == (byte)ENUM_SECTION_SUBTYPE_PRODUCT.PRODUCTION_POPUPSTORE))         // 보석
                        {
                            if (sectiontd.Section_Sub_Type == (byte)ENUM_SECTION_SUBTYPE_PRODUCT.PRODUCTION_POPUPSTORE
                            && sectioninfos[i].remain_mts <= 0)
                            {
                                continue;
                            }

                            long curproducts = sectioninfos[i].storage; // 현재 생산량
                            long storagesize = sectionlvtd.Production_Storage; // 최대 저장량
                            long needproducts = storagesize - curproducts; // 필요 생산량
                            if (needproducts > 0)
                            {
                                long productremaintime = ((needproducts / sectionlvtd.Production_Value) + 1) * sectionlvtd.Production_Time * 1000; // 필요 생산 시간
                                if (min_remain_milseconds > 0)
                                {
                                    min_remain_milseconds = min_remain_milseconds > productremaintime ? productremaintime : min_remain_milseconds;
                                }
                                else { min_remain_milseconds = productremaintime; }

                                CDebug.Log($"&&&^^^%%% productremaintime : {productremaintime} - 부서({sectiontd.ID}) 생산");
                            }
                        }
                    }
                }
            }

            // 진행중인 Activity들 중 최소 시간을 구한다. (트레이닝, 휴식, 카드 생산, 트렌디 생산) : activity data
            List<ManagementAPI.ManagementActivityData> activityDataList = ManagementServerDataManager.Instance.GetActivityDataList();
            if (activityDataList != null && activityDataList.Count > 0)
            {
                for (int i = 0; i < activityDataList.Count; i++)
                {
                    if (activityDataList[i].status == (byte)ACTIVITYPROGRESSTYPE.Start)
                    {
                        CDebug.Log($"&&&^^^%%% activityDataList[{i}].remain_mts : {activityDataList[i].remain_mts} - 부서 액티비티");
                        if (activityDataList[i].remain_mts > 0)
                        {
                            min_remain_milseconds = min_remain_milseconds > activityDataList[i].remain_mts ? activityDataList[i].remain_mts : min_remain_milseconds;
                        }
                        else { min_remain_milseconds = activityDataList[i].remain_mts; }
                    }
                }
            }
#if UNITY_EDITOR
            CDebug.Log($"&&&^^^%%% min_remain_milseconds : {min_remain_milseconds} - 최소 시간");
#endif
            // 최소 시간 min_remain_milseconds로 글로벌 타이머를 설정한다.
            if (min_remain_milseconds > 0)
            {
                DisposeReddotTimer();
                string timerKey = CDefines.MANAGEMENT_REDDOT_TIMESTREAM_KEY;
                TimeStream timestream = null;
                GlobalTimer.Instance.GetTimeStream(out timestream, timerKey);

                disposer_management_reddot = new SingleAssignmentDisposable();

                var sec = (long)(min_remain_milseconds * 0.001f);
                disposer_management_reddot.Disposable = timestream.SetTime(timerKey, sec, 0f, TimeStreamType.DECREASE)
                .OnTimeStreamObservable()
                .Subscribe(timestreamData =>
                {
#if UNITY_EDITOR
                    CDebug.Log($"&&&^^^%%% MainPage timestreamData.CurrentTime : {timestreamData.CurrentTime} - 남은 시간");
#endif
                    if (timestreamData.IsEnd)
                    {
                        disposer_management_reddot.Dispose();
                        UIManager.SetClientReddot(UIContentType.MANAGEMENT, true);
                    }
                });
            }
        }
    }

    public void DisposeReddotTimer()
    {
        if (disposer_management_reddot != null)
        {
            disposer_management_reddot.Dispose();
            disposer_management_reddot = null;
        }
    }
    #endregion Reddot Timer
}
