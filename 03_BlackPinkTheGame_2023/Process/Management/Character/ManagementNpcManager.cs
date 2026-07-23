using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using GroupManagement;
using GroupManagement.ManagementEnums;
using UniRx;
using BehaviorDesigner.Runtime;
using System;

public class ManagementNpcManager : MonoBehaviour
{
    private ManagementWorldManager worldMgr;

    public Dictionary<ENpcType, List<CNpcInfo>> NpcInfoDic;
    private Dictionary<long, long> NpcUIDDic; //Key uid(CreateNpcUID), Value npcTableID
    public Dictionary<long, GameObject> NpcObjDic; //key uid
    public Dictionary<long, GameObject> NpcShadowObjDic;
    public Dictionary<long, GameObject> NpcUIDummyDic; //key uid
    //public Dictionary<long, CCharacterController> NpcControllerDic;
    public Dictionary<long, CCharacterController_Management> NpcControllerDic;
    public Dictionary<long, SectionScript> NpcSectionScriptDic;

    private Dictionary<long, CEvent> NpcEventDic;
    private Dictionary<long, EVENT_STATE> NpcEventStateDic;

    //3D UI
    private Dictionary<long, ManagementAvatarEventUI> NpcEventIconObj;
	private Dictionary<long, GameObject> NpcNametagObj;

    private Dictionary<long, SingleAssignmentDisposable> DelayTimerDisposer;

    private Dictionary<long, BehaviorTree> NpcBTDic;

    private long CreateNpcUID;

    //private Dictionary<long, MailIcon> NpcMailIconObjDic;
    private Dictionary<long, ManagementAvatarEventUI> NpcMailQuestIconObj;

    public void Initialize()
    {
        worldMgr = ManagementManager.Instance.worldManager;

        NpcInfoDic = new Dictionary<ENpcType, List<CNpcInfo>>();
        NpcUIDDic = new Dictionary<long, long>();
        NpcObjDic = new Dictionary<long, GameObject>();
        NpcShadowObjDic = new Dictionary<long, GameObject>();
        NpcUIDummyDic = new Dictionary<long, GameObject>();
        //NpcControllerDic = new Dictionary<long, CCharacterController>();
        NpcControllerDic = new Dictionary<long, CCharacterController_Management>();
        NpcSectionScriptDic = new Dictionary<long, SectionScript>();

        NpcEventDic = new Dictionary<long, CEvent>();
        NpcEventStateDic = new Dictionary<long, EVENT_STATE>();
        NpcEventIconObj = new Dictionary<long, ManagementAvatarEventUI>();
		NpcNametagObj = new Dictionary<long, GameObject>();

        DelayTimerDisposer = new Dictionary<long, SingleAssignmentDisposable>();

        NpcBTDic = new Dictionary<long, BehaviorTree>();
        //NpcCurBTTypeDic = new Dictionary<long, BT_TYPE>();

        //NpcHoldObjDic = new Dictionary<long, List<CBTHoldObjectInfo>>();

        //NpcAnimEvtDic = new Dictionary<long, CharAnimationEvent>();

        //NpcMailIconObjDic = new Dictionary<long, MailIcon>();
        NpcMailQuestIconObj = new Dictionary<long, ManagementAvatarEventUI>();

        CreateNpcUID = 1000;
    }

	private void FixedUpdate()
	{
		CamRatioNameTagActiveUpdate();
	}

	private void CamRatioNameTagActiveUpdate()
	{
        if (ManagementManager.Instance.worldManager != null
            && ManagementManager.Instance.worldManager.CamController != null)
        {
            var currentCamSize = ManagementManager.Instance.worldManager.CamController.GetCameraMoveRatioByZoom();
			var active = currentCamSize > ManagementWorldManager.NAME_TAG_SHOW_RATIO;
			foreach(long npcUID in NpcNametagObj.Keys)
			{
				if(NpcObjDic.ContainsKey(npcUID))
				{
                    if(NpcObjDic[npcUID] != null)
                    {
                        if (NpcObjDic[npcUID].activeSelf)
                        {
                            NpcNametagObj[npcUID].SetActive(active);
                        }
                        else
                        {
                            NpcNametagObj[npcUID].SetActive(false);
                        }
                    }
                }
			}
		}
	}


    //Async--
    private Coroutine AsyncLoadCoroutine;
    public IObservable<long> AsyncLoadNpc(long npcID, Transform parent, SectionScript script)
    {
        CNpcInfo npcInfo = CNpcDataManager.Instance.GetNpcInfo(npcID);
        if (npcInfo == null)
        {
            CDebug.Log("LoadNpc npcInfo is null");
            return Observable.Return<long>(-1);
        }
        if (NpcInfoDic.ContainsKey(npcInfo.NpcType) == false)
        {
            NpcInfoDic.Add(npcInfo.NpcType, new List<CNpcInfo>());
            NpcInfoDic[npcInfo.NpcType].Add(npcInfo);
        }
        else
        {
            NpcInfoDic[npcInfo.NpcType].Add(npcInfo);
        }

        if (npcInfo.NpcType == ENpcType.NONE)
            return Observable.Return<long>(-1);

        
        
        return NpcObjectHelper.AsyncInstaniateNpcObject(npcInfo, parent, gameObject)
        .Select(asset => 
        {
            GameObject npcObj = asset as GameObject;
            CreateNpcUID = CreateNpcUID + 1;

            string snpcName = "SNPC_" + CreateNpcUID + "_" + npcInfo.ID;

            NpcObjectHelper.SetNameAndTagAndLayer(npcObj, snpcName, Management_LayerTag.Tag_NPC, LayerMask.NameToLayer(Management_LayerTag.Layer_SECTION));

            if (NpcUIDDic.ContainsKey(CreateNpcUID) == false)
            {
                NpcUIDDic.Add(CreateNpcUID, npcID);
            }
            else
            {
                NpcUIDDic[CreateNpcUID] = npcID;
            }

            if (NpcObjDic.ContainsKey(CreateNpcUID) == false)
            {
                NpcObjDic.Add(CreateNpcUID, npcObj);
            }
        
            //AvatarManager.SetActivateLOD(ref npcObj, false);

            var uiDummyBone = npcObj.transform.Find(ManagementWorldManager.CHARACTER_UI_DUMMY_BONE);
            var npcName = CResourceManager.Instance.GetString(npcInfo.NpcNameID);
            if (uiDummyBone == null)
            {
                Debug.LogError($"No No No~ Dummy_Name!!!!!!! It's NOT HERE!!! MISSING!!!!! npcID = {npcInfo.ID}, name = {npcName}");
            }
            NpcUIDummyDic.Add(CreateNpcUID, uiDummyBone.gameObject);
            NpcNametagObj.Add(CreateNpcUID, ManagementManager.Instance.AddNameTag3D(uiDummyBone.gameObject, npcName));


            StartCheckingEvent(CreateNpcUID);

            var resData = CResourceManager.Instance.GetResourceData(Constants.PATH_UI_EVENT);
            GameObject _uiObj = resData.Load<GameObject>(npcObj);
            GameObject insUIObj = Utility.AddChild(ManagementManager.Instance.GetEventIconLayer(), _uiObj);
            ManagementAvatarEventUI aeUI = insUIObj.GetComponent<ManagementAvatarEventUI>();
            aeUI.Initialize(RECEIVED_EVENT_TYPE.EVENT);
            aeUI.SetEventIcon(npcID);
            insUIObj.SetActive(false);

            //MailQuest Icon UI
            resData = CResourceManager.Instance.GetResourceData(Constants.PATH_UI_EVENT);
            _uiObj = resData.Load<GameObject>(npcObj);
            insUIObj = Utility.AddChild(ManagementManager.Instance.GetEventIconLayer(), _uiObj);
            insUIObj.name = "MailQuestIcon(NPC)_" + npcID;
            ManagementAvatarEventUI mailQuestUI = insUIObj.GetComponent<ManagementAvatarEventUI>();
            if (mailQuestUI == null)
            {
                mailQuestUI = insUIObj.AddComponent<ManagementAvatarEventUI>();
            }
            insUIObj.SetActive(false);
            mailQuestUI.Initialize(RECEIVED_EVENT_TYPE.MAIL_QUEST);
            if (NpcMailQuestIconObj.ContainsKey(CreateNpcUID) == false)
            {
                NpcMailQuestIconObj.Add(CreateNpcUID, mailQuestUI);
            }


            if (NpcEventIconObj.ContainsKey(CreateNpcUID) == false)
            {
                NpcEventIconObj.Add(CreateNpcUID, aeUI);
            }

            AsyncLoadCoroutine = StartCoroutine(EnumerableAsyncLoad(CreateNpcUID, npcID, npcObj, script));

            if (NpcSectionScriptDic.ContainsKey(CreateNpcUID) == false)
            {
                NpcSectionScriptDic.Add(CreateNpcUID, script);
            }

            if (NpcEventStateDic.ContainsKey(CreateNpcUID) == false)
            {
                NpcEventStateDic.Add(CreateNpcUID, EVENT_STATE.NONE);
            }

            //npcObj.SetActive( true );

            return CreateNpcUID;
        });
    }



    private IEnumerator EnumerableAsyncLoad(long CreateNpcUID, long npcID, GameObject npcObj, SectionScript script)
    {

        // Shadow
        var shadowResData = CResourceManager.Instance.GetResourceData(Constants.PATH_CHAR_SHADOW);
        yield return shadowResData.LoadAsync<GameObject>(npcObj)
        .Do(asset => 
        {
            var shadow = UnityEngine.Object.Instantiate(asset);
            shadow.transform.parent = npcObj.transform;
            shadow.transform.localPosition = new Vector3(0, 0.01f, 0);//Vector3.zero;
        })
        .ToYieldInstruction();


        CNpcInfo npcInfo = CNpcDataManager.Instance.GetNpcInfo(npcID);
        var cntlr = npcObj.GetComponent<CCharacterController_Management>();
        cntlr ??= npcObj.AddComponent<CCharacterController_Management>();
        cntlr.Initialize(CHARACTER_TYPE.NPC);
        cntlr.SetNpcUID(CreateNpcUID);
        cntlr.SetNPCSectionScript(script);
        cntlr.SetRuntimeAnimatorController( CreateNpcUID.ToString(), npcInfo.AnimControllerGrpID, ANIMCONTROLLER_USE_TYPE.MANAGEMENT );
        NpcObjectHelper.SetNpcTransformByDefaultValues(npcObj);
        if (NpcControllerDic.ContainsKey(CreateNpcUID) == false)
        {
            NpcControllerDic.Add(CreateNpcUID, cntlr);
        }

        // BT 
        if (npcInfo.BT_ID.Equals("0") == false)
        {
            yield return AsyncSetNPCBT(CreateNpcUID, npcObj, script ).ToYieldInstruction();
        }

        cntlr.SetNavMeshSpeed(npcInfo.NpcWalkSpeed);
        //cntlr.SetNavMesh();
        //cntlr.SetActiveNavMeshAgent(true);

        //3D UI
        //var resData = CResourceManager.Instance.GetResourceData(Constants.PATH_UI_EVENT);
        //if(resData != null)
        //{
        //    yield return resData.LoadAsync<GameObject>(npcObj)
        //    .Do(asset => 
        //    {
        //        GameObject _uiObj = asset;
        //        GameObject insUIObj = Utility.AddChild(ManagementManager.Instance.GetEventIconLayer(), _uiObj);
        //        ManagementAvatarEventUI aeUI = insUIObj.GetComponent<ManagementAvatarEventUI>();
        //        aeUI.Initialize(RECEIVED_EVENT_TYPE.EVENT);
        //        aeUI.SetEventIcon(npcID);
        //        insUIObj.SetActive(false);
        //        if (NpcEventIconObj.ContainsKey(CreateNpcUID) == false)
        //        {
        //            NpcEventIconObj.Add(CreateNpcUID, aeUI);
        //        }
        //    })
        //    .ToYieldInstruction();
        //}


        ////MailQuest Icon UI
        //var resData2 = CResourceManager.Instance.GetResourceData(Constants.PATH_UI_EVENT);
        //if (resData2 != null)
        //{
        //    yield return resData2.LoadAsync<GameObject>(npcObj)
        //    .Do(asset => 
        //    {
        //        GameObject _uiObj = asset;
        //        GameObject insUIObj = Utility.AddChild(ManagementManager.Instance.GetEventIconLayer(), _uiObj);
        //        insUIObj.name = $"MailQuestIcon(NPC)_{npcID}";
        //        ManagementAvatarEventUI mailQuestUI = insUIObj.GetComponent<ManagementAvatarEventUI>();
        //        if (mailQuestUI == null)
        //        {
        //            mailQuestUI = insUIObj.AddComponent<ManagementAvatarEventUI>();
        //        }
        //        insUIObj.SetActive(false);
        //        mailQuestUI.Initialize(RECEIVED_EVENT_TYPE.MAIL_QUEST);
        //        if (NpcMailQuestIconObj.ContainsKey(CreateNpcUID) == false)
        //        {
        //            NpcMailQuestIconObj.Add(CreateNpcUID, mailQuestUI);
        //        }
        //    })
        //    .ToYieldInstruction();
        //}

    }




    //Sync--
    public long LoadNpc(long npcID, Transform parent, SectionScript script)
    {
        CNpcInfo npcInfo = CNpcDataManager.Instance.GetNpcInfo(npcID);
        if(npcInfo == null)
        {
            CDebug.Log("LoadNpc npcInfo is null");
            return -1;
        }
        if (NpcInfoDic.ContainsKey(npcInfo.NpcType) == false)
        {
            NpcInfoDic.Add(npcInfo.NpcType, new List<CNpcInfo>());
            NpcInfoDic[npcInfo.NpcType].Add(npcInfo);
        }
        else
        {
            NpcInfoDic[npcInfo.NpcType].Add(npcInfo);
        }

        if (npcInfo.NpcType == ENpcType.NONE)
            return -1;

        GameObject npcObj = NpcObjectHelper.InstaniateNpcObject(npcInfo, parent, gameObject);
        CreateNpcUID = CreateNpcUID + 1;

        string snpcName = "SNPC_" + CreateNpcUID +"_"+npcInfo.ID;

        NpcObjectHelper.SetNameAndTagAndLayer( npcObj, snpcName, Management_LayerTag.Tag_NPC, LayerMask.NameToLayer(Management_LayerTag.Layer_SECTION) );

        if (NpcUIDDic.ContainsKey(CreateNpcUID) == false)
        {
            NpcUIDDic.Add(CreateNpcUID, npcID);
        }
        else
        {
            NpcUIDDic[CreateNpcUID] = npcID;
        }

        if (NpcObjDic.ContainsKey(CreateNpcUID) == false)
        {
            NpcObjDic.Add(CreateNpcUID, npcObj);
        }

        var uiDummyBone = npcObj.transform.Find(ManagementWorldManager.CHARACTER_UI_DUMMY_BONE);
		var npcName = CResourceManager.Instance.GetString(npcInfo.NpcNameID);
		if (uiDummyBone == null)
		{
			Debug.LogError($"No No No~ Dummy_Name!!!!!!! It's NOT HERE!!! MISSING!!!!! npcID = {npcInfo.ID}, name = {npcName}");
		}
		NpcUIDummyDic.Add(CreateNpcUID, uiDummyBone.gameObject);

		//NpcNametagObj.Add(CreateNpcUID, ManagementManager.Instance.AddNameTag(uiDummyBone.gameObject, npcName));
        NpcNametagObj.Add(CreateNpcUID, ManagementManager.Instance.AddNameTag3D(uiDummyBone.gameObject, npcName));

        GameObject shadowObj = AvatarManager.Instance.SetCharacterShadow(npcObj);
        NpcShadowObjDic.Add(CreateNpcUID, shadowObj);

        // CCharacterController cntlr = npcObj.GetComponent<CCharacterController>();
        // if(cntlr == null)
        // {
        //    cntlr = npcObj.AddComponent<CCharacterController>();
        // }
        // cntlr.Initialize(CHARACTER_TYPE.NPC);
        // CharAnimationEvent _animEvt = npcObj.GetComponent<CharAnimationEvent>();
        // if(_animEvt != null)
        // {
        //    if (NpcAnimEvtDic.ContainsKey(CreateNpcUID) == false)
        //    {
        //        NpcAnimEvtDic.Add(CreateNpcUID, _animEvt);
        //    }
        //    //Hold Object
        //    SetHoldableObject(CreateNpcUID, npcObj, npcInfo);
        //    NpcAnimEvtDic[CreateNpcUID].InitAnimEvent(cntlr);
        // }
        // if (npcInfo.BT_ID.Equals("0") == false)
        // {
        //    InitNpcBT(CreateNpcUID, npcObj);
        // }
        // cntlr.SetNpcUID(CreateNpcUID);
        // //cntlr.SetAnimationEvent();
        // cntlr.SetNPCSectionScript(script);
        // SetNpcTransform(npcObj);
        // cntlr.SetNavMeshSpeed(npcInfo.NpcWalkSpeed);
        // cntlr.SetNavMesh();
        // cntlr.SetActiveNavMeshAgent(true);
        CCharacterController_Management cntlr = npcObj.GetComponent<CCharacterController_Management>();
        if (cntlr == null)
        {
            cntlr = npcObj.AddComponent<CCharacterController_Management>();
        }
        cntlr.Initialize(CHARACTER_TYPE.NPC);
        // cntlr.SetThisObject(npcObj);
        // CharAnimationEvent _animEvt = npcObj.GetComponent<CharAnimationEvent>();
        // if (_animEvt != null)
        // {
        //    if (NpcAnimEvtDic.ContainsKey(CreateNpcUID) == false)
        //    {
        //        NpcAnimEvtDic.Add(CreateNpcUID, _animEvt);
        //    }
        //    //Hold Object
        //    SetHoldableObject(CreateNpcUID, npcObj, npcInfo);
        //    NpcAnimEvtDic[CreateNpcUID].InitAnimEvent(cntlr);
        // }
        if (npcInfo.BT_ID.Equals("0") == false)
        {
            InitNpcBT(CreateNpcUID, npcObj);
        }
        cntlr.SetNpcUID(CreateNpcUID);
        cntlr.SetNPCSectionScript(script);
        NpcObjectHelper.SetNpcTransformByDefaultValues(npcObj);
        cntlr.SetNavMeshSpeed(npcInfo.NpcWalkSpeed);
        cntlr.SetNavMesh();
        cntlr.SetActiveNavMeshAgent(true);


        if (NpcControllerDic.ContainsKey(CreateNpcUID) == false)
        {
            NpcControllerDic.Add(CreateNpcUID, cntlr);
        }

        StartCheckingEvent(CreateNpcUID);


        //Transform shadow = AvatarManager.Instance.LoadCharacterShadow(npcObj).transform;
        //cntlr.SetShadowObj(shadow);

        //3D UI
        var resData = CResourceManager.Instance.GetResourceData(Constants.PATH_UI_EVENT);
        GameObject _uiObj = resData.Load<GameObject>(npcObj);
		GameObject insUIObj = Utility.AddChild(ManagementManager.Instance.GetEventIconLayer(), _uiObj);
        ManagementAvatarEventUI aeUI = insUIObj.GetComponent<ManagementAvatarEventUI>();
        aeUI.Initialize( RECEIVED_EVENT_TYPE.EVENT );
        aeUI.SetEventIcon(npcID);
        insUIObj.SetActive(false);

        //MailQuest Icon UI
        resData = CResourceManager.Instance.GetResourceData( Constants.PATH_UI_EVENT );
        _uiObj = resData.Load<GameObject>( npcObj );
        insUIObj = Utility.AddChild( ManagementManager.Instance.GetEventIconLayer(), _uiObj );
        insUIObj.name = "MailQuestIcon(NPC)_" + npcID;
        ManagementAvatarEventUI mailQuestUI = insUIObj.GetComponent<ManagementAvatarEventUI>();
        if (mailQuestUI == null)
        {
            mailQuestUI = insUIObj.AddComponent<ManagementAvatarEventUI>();
        }
        insUIObj.SetActive( false );
        mailQuestUI.Initialize( RECEIVED_EVENT_TYPE.MAIL_QUEST );
        if (NpcMailQuestIconObj.ContainsKey( CreateNpcUID ) == false)
        {
            NpcMailQuestIconObj.Add( CreateNpcUID, mailQuestUI );
        }


        if (NpcEventIconObj.ContainsKey(CreateNpcUID) == false)
        {
            NpcEventIconObj.Add(CreateNpcUID, aeUI);
        }

        if(NpcSectionScriptDic.ContainsKey(CreateNpcUID) == false)
        {
            NpcSectionScriptDic.Add(CreateNpcUID, script);
        }

        if(NpcEventStateDic.ContainsKey(CreateNpcUID) == false)
        {
            NpcEventStateDic.Add(CreateNpcUID, EVENT_STATE.NONE);
        }

        return CreateNpcUID;
    }

    //public void SetHoldableObject(long npcUID, GameObject npcObj, CNpcInfo npcInfo)
    //{
    //    if(npcInfo != null)
    //    {
    //        if(NpcHoldObjDic.ContainsKey(npcUID) == false)
    //        {
    //            NpcHoldObjDic.Add(npcUID, new List<CBTHoldObjectInfo>());
    //        }

    //        for(int i = 0; i<npcInfo.Object_ID.Length; ++i)
    //        {
    //            if(npcInfo.Object_ID[i] != 0)
    //            {
    //                HoldObjectData _holdObjData = CPlayerDataManager.Instance.GetHoldObjectDataByID(npcInfo.Object_ID[i]);

    //                if (_holdObjData != null)
    //                {
    //                    NpcHoldObjDic[npcUID].Add(new CBTHoldObjectInfo());

    //                    int _idx = NpcHoldObjDic[npcUID].Count - 1;
    //                    NpcHoldObjDic[npcUID][_idx].SetData
    //                    (
    //                        _holdObjData, npcObj,
    //                        NpcAnimEvtDic[npcUID].DummyHand_Left.transform, NpcAnimEvtDic[npcUID].DummyHand_Right.transform                            
    //                    );
    //                }
    //            }
    //        }
    //    }
    //}

    //public CBTHoldObjectInfo GetHoldObjectInfo(long npcUID, long ObjectDataID)
    //{
    //    if (NpcHoldObjDic.ContainsKey(npcUID) == false)
    //    {
    //        CDebug.Log("GetHoldObjectInfo NpcHoldObjDic has not contain " + npcUID);
    //        return null;
    //    }

    //    List<CBTHoldObjectInfo> _list = NpcHoldObjDic[npcUID];

    //    for(int i=0; i<_list.Count; ++i)
    //    {
    //        if(ObjectDataID == _list[i].ObjectDataID)
    //        {
    //            return _list[i];
    //        }
    //    }

    //    return null;
    //}

    //BT use
    public CCharacterController GetNpcCharacterController(Transform obj, out long npc_uid)
    {
        foreach(KeyValuePair<long, GameObject> item in NpcObjDic)
        {
            if(ReferenceEquals(item.Value, obj.gameObject))
            {
                npc_uid = item.Key;
                if(NpcControllerDic.ContainsKey(npc_uid))
                {
                    return NpcControllerDic[npc_uid];
                }
            }
        }
        npc_uid = 0;
        return null;
    }

    //public CCharacterController GetNpcCharacterController(long npcUID)
    //{
    //    if (NpcControllerDic.ContainsKey(npcUID))
    //    {
    //        return NpcControllerDic[npcUID];
    //    }

    //    return null;
    //}

    public void SetTrainingNpcLocation(SectionScript script, long npcID, int slotIdx)
    {
        script.SetActivitySpNpcByID(npcID);
    }

    public GameObject GetManagementNpcObjectByNpcID(long npcID)
    {
        foreach(KeyValuePair<long, long> item in NpcUIDDic)
        {
            if(item.Value == npcID)
            {
                return NpcObjDic[item.Key];
            }
        }

        return null;
    }

    public long GetTrainingNpcUIDByID(long npcID)
    {
        foreach (KeyValuePair<long, long> item in NpcUIDDic)
        {
            if (item.Value == npcID)
            {
                if (NpcObjDic[item.Key] != null)
                {
                    return item.Key;
                }
            }
        }

        return -1;
    }

    public long GetNpcIDByUID(long npcUID)
    {
        return NpcUIDDic[npcUID];
    }

    public SectionScript GetManagementSectionScriptByNpcID(long npcUID)
    {
        return NpcSectionScriptDic[npcUID];
    }

    //----- Event ------------
    private GameObject GetSectionObjectIncudingNpc(Vector3 pos, out Vector3 iconPos)
    {
        GridIndex gridIdx = worldMgr.GetGridManager().GetXYGridIndexFromWorldPosition(pos);
        long sectionid_serveruniqueid = worldMgr.GetGridManager().GetGridTileSectionID_ServerUniqueID(gridIdx);
        if(sectionid_serveruniqueid == 0)
        {
            iconPos = Vector3.zero;
            return null;
        }
        SectionRef _data = worldMgr.GetSectionManager().GetSectionRefData(sectionid_serveruniqueid);
        iconPos = worldMgr.GetSectionCenterPosition(_data.SectionScript, _data.SectionScript.gameObject);

        return _data.SectionScript.gameObject;
    }

    public void SetNpcEventIcon(long npcUID, CEvent mEvent, SingleAssignmentDisposable disposer)
    {
        //Vector3 pos = NpcObjDic[npcUID].transform.localPosition;
        //pos.z = -1.5f;
        //NpcEventIconObj[npcUID].transform.localPosition = pos;

        if (NpcEventIconObj.ContainsKey(npcUID))
        {
            FollowObjectPositionFor2D fop = NpcEventIconObj[npcUID].transform.gameObject.AddComponent<FollowObjectPositionFor2D>();
            var canvasRect = worldMgr.GetCanvasManager().GetComponent<RectTransform>();
			fop.Init(canvasRect, NpcUIDummyDic[npcUID].transform);

            NpcEventIconObj[npcUID].SetData_Npc(mEvent, npcUID, disposer);

            NpcEventIconObj[npcUID].gameObject.SetActive(true);
        }
    }

    public Transform GetNpcEventIconObj(long npcUID)
    {
        if(NpcEventIconObj.ContainsKey(npcUID))
        {
            return NpcEventIconObj[npcUID].transform;
        }

        return null;
    }

	public void SetActiveNpcEventIcon(long npcUID, bool bActive)
	{
		if(GetNpcEventIconObj(npcUID))
		{
			GetNpcEventIconObj(npcUID).gameObject.SetActive(bActive);
		}
	}

	public Transform GetNpcNameTag(long npcUID)
	{
		if(NpcNametagObj.ContainsKey(npcUID))
        {
            return NpcNametagObj[npcUID].transform;
        }

        return null;
	}

	public void SetActiveNameTag(long npcUID, bool bActive)
	{
		if(GetNpcNameTag(npcUID))
		{
			GetNpcNameTag(npcUID).gameObject.SetActive(bActive);
		}
	}


    public void SetNpcEventIconObjPos(long npcUID, Vector3 pos)
    {
        if (NpcEventIconObj.ContainsKey(npcUID))
        {
            NpcEventIconObj[npcUID].transform.localPosition = pos;
        }
    }

    public CEvent GetNpcEvent(long npcUID)
    {
        if(NpcEventDic.ContainsKey(npcUID))
        {
            return NpcEventDic[npcUID];
        }

        return null;
    }

    private void InitDelayTimeDisposer(long npcUID)
    {
        if(DelayTimerDisposer.ContainsKey(npcUID))
        {
            if(DelayTimerDisposer[npcUID] == null)
            {
                DelayTimerDisposer[npcUID] = new SingleAssignmentDisposable();
            }
            else
            {
                DelayTimerDisposer[npcUID].Dispose();
                DelayTimerDisposer[npcUID] = new SingleAssignmentDisposable();
            }
        }
        else
        {
            DelayTimerDisposer.Add(npcUID, new SingleAssignmentDisposable());
        }
    }

    public void DisposeNpcTimeDisposer()
    {
        foreach(KeyValuePair<long, SingleAssignmentDisposable> item in DelayTimerDisposer)
        {
            DelayTimerDisposer[item.Key].Dispose();
        }
    }

	public void DisposeNpcTimeDisposer( long npcUID )
	{
		if( DelayTimerDisposer != null )
		{
			if( DelayTimerDisposer.ContainsKey( npcUID ) )
			{
				DelayTimerDisposer[ npcUID ].Dispose();
			}
		}
	}

    private long GetNpcGdid(long npcUID)
    {
        if(NpcUIDDic.ContainsKey(npcUID))
        {
            return NpcUIDDic[npcUID];
        }

        return 0;
    }

    public GameObject GetNpcGameObject(long npcUID)
    {
        if(NpcObjDic.ContainsKey(npcUID))
        {
            if (NpcObjDic[npcUID] != null)
            {
                return NpcObjDic[npcUID];
            }
        }
        return null;
    }

	public GameObject GetNpcUIDummyGameOgject(long npcUID)
	{
		if(NpcUIDummyDic.ContainsKey(npcUID))
        {
            if (NpcUIDummyDic[npcUID] != null)
            {
                return NpcUIDummyDic[npcUID];
            }
        }
        return null;
	}

    public void SetActiveNpcGameObject(long npcUID, bool bActive)
    {
        if(NpcObjDic.ContainsKey(npcUID))
        {
			//if(!bActive)
			//{
			//	var bubbleObj = NpcObjDic[npcUID].transform.parent.GetComponentInChildren<FollowObjectPositionFor2D>();
			//	if(bubbleObj)
			//	{
			//		bubbleObj.gameObject.SetActive(false);
			//	}
			//}
            NpcObjDic[npcUID].SetActive(bActive);
        }
		SetActiveNameTag(npcUID, bActive);
		//SetActiveNpcEventIcon(npcUID, bActive);
    }

	public void SetActiveNpcGameObjectForRelocation(long npcUID, bool bActive)
    {
        if(NpcObjDic.ContainsKey(npcUID))
        {
			//if(!bActive)
			//{
			//	var bubbleObj = NpcObjDic[npcUID].transform.parent.GetComponentInChildren<FollowObjectPositionFor2D>();
			//	if(bubbleObj)
			//	{
			//		bubbleObj.gameObject.SetActive(false);
			//	}
			//}
            NpcObjDic[npcUID].SetActive(bActive);
        }
		SetActiveNameTag(npcUID, bActive);
		SetActiveNpcEventIcon(npcUID, bActive);
    }

    public void SetActiveAllNpcGameObject(bool bActive)
    {        
        foreach(KeyValuePair<long, GameObject> npc in NpcObjDic)
        {
            SetActiveNpcGameObject(npc.Key, bActive);
        }
    }

	public void SetActiveAllNpcGameObjectForRelocation(bool bActive)
    {        
        foreach(KeyValuePair<long, GameObject> npc in NpcObjDic)
        {
            SetActiveNpcGameObjectForRelocation(npc.Key, bActive);
        }
    }

    public void SetNpcEventState(long npcUID, EVENT_STATE state)
    {
        if (NpcObjDic == null)
        {
            return;
        }

        if (NpcObjDic.ContainsKey(npcUID))
        {
            if (state == EVENT_STATE.NONE)
            {
                if(NpcEventIconObj[npcUID] != null)
                {
                    if (NpcEventIconObj[npcUID].gameObject)
                    {
                        NpcEventIconObj[npcUID].gameObject.SetActive(false);
                    }
                }
            }
            NpcEventStateDic[npcUID] = state;
        }
    }

    public EVENT_STATE GetNpcEventState(long npcUID)
    {
        if(NpcEventStateDic.ContainsKey(npcUID))
        {
            return NpcEventStateDic[npcUID];
        }

        return EVENT_STATE.NONE;
    }

    public void DestroyManagementNpcEvent(long npcUID)
    {
        SetNpcEventState(npcUID, EVENT_STATE.NONE);
        NpcEventDic[npcUID] = null;
    }

    

    /// <summary>
    /// Avatar Member Event Check.
    /// </summary>
    /// <param name="mType">Avatar Member Type</param>
    public void CheckEvent(long npcUID)
    {
        if (GetNpcEventState(npcUID) == EVENT_STATE.START)	//npc가 메일인 경우에만 해당
            return;

        long npcID = GetNpcGdid(npcUID);
        if(npcID == 0)
        {
            CDebug.LogError("ManagementNpcManager.CheckEvent() npcUID = 0");
            return;
        }

        CEvent _npcEvent = CEventManager.Instance.SelectEvent_NPC(npcID);

        //If Member received Event
        if (_npcEvent != null)
        {
            InitDelayTimeDisposer(npcUID);

            //Event has been started but not Touch
            SetNpcEventState(npcUID, EVENT_STATE.START);
            //Debug.Log("Start Check Member Event......" + mType + "/" + memberEvent.Event_ID);

            DelayTimerDisposer[npcUID].Disposable = Observable.Timer(System.TimeSpan.FromSeconds(_npcEvent.Event_Delay_Time))
            .Subscribe(_ =>
            {
				DisposeNpcTimeDisposer(npcUID);
                CEventManager.Instance.RequestEventCancel(_npcEvent);
                DestroyManagementNpcEvent(npcUID);
                StartCheckingEvent(npcUID);
                CDebug.Log("Waitting finish(event delay time) for Event touch. restart Checking...");
            }).AddTo(this);

            StopCheckingEventOnEachNPC(npcUID);

            if (NpcEventDic.ContainsKey(npcUID) == false)
            {
                NpcEventDic.Add(npcUID, _npcEvent);
            }
            else
            {
                NpcEventDic[npcUID] = _npcEvent;
            }

            SetNpcEventIcon(npcUID, _npcEvent, DelayTimerDisposer[npcUID]);
        }
    }




    private IObservable<long> AsyncSetNPCBT(long npcUID, GameObject npcObj, SectionScript script)
    {
        BehaviorTree _bt = npcObj.AddComponent<BehaviorTree>();
        NpcBTDic.Add(npcUID, _bt);

        NpcBTDic[npcUID].enabled = false;

        long npcID = GetNpcIDByUID(npcUID);
        CNpcInfo npcInfo = CNpcDataManager.Instance.GetNpcInfo(npcID);
        var resData = CResourceManager.Instance.GetResourceData(npcInfo.BT_ID);

        return resData.LoadAsync<ExternalBehavior>(NpcObjDic[npcUID])
        .Do(asset =>
        {
            NpcBTDic[npcUID].ExternalBehavior = asset;
            NpcBTDic[npcUID].enabled = true;
        } )
        .Select(asset => npcUID);
    }


    private void InitNpcBT(long npcUID, GameObject npcObj)
    {
        BehaviorTree _bt = npcObj.AddComponent<BehaviorTree>();
        NpcBTDic.Add(npcUID, _bt);
        SetNpcBT(npcUID);
    }

    private bool IsPopupStore(SectionScript script)
    {
        ENUM_SECTION_TYPE_ID sectionValue = (ENUM_SECTION_TYPE_ID)script.sectionStatusData.GetSectionTableData().Section_Value;
        if (sectionValue == ENUM_SECTION_TYPE_ID.POPUP_STORE)
        {
            return true;
        }

        return false;
    }

    public void SetPopupStore(long npcUID, bool open)
    {
        if(NpcSectionScriptDic.ContainsKey(npcUID))
        {
            //if(IsPopupStore( NpcSectionScriptDic[npcUID] ))
            {
                if(NpcObjDic.ContainsKey(npcUID))
                {
                    NpcObjDic[npcUID].SetActive( open );
                }

                if (NpcControllerDic.ContainsKey( npcUID ))
                {
                    NpcControllerDic[npcUID].SetNmAgentNeedNot( !open );
                    NpcControllerDic[npcUID].SetActiveBehaviorTree( open );
                    NpcControllerDic[npcUID].SetActiveNavMeshAgent( open );
                }

            }
        }
    }

    //private List<CBT_Info> GetBTInfos(long npcUID)
    //{
    //    if (NpcBTDic.ContainsKey(npcUID) == false)
    //    {
    //        CDebug.LogError("SetNpcBT npcUID is wrong");
    //        return null;
    //    }
    //    long npcID = GetNpcIDByUID(npcUID);
    //    CNpcInfo npcInfo = CNpcDataManager.Instance.GetNpcInfo(npcID);

    //    List<CBT_Info> _btInfos = CNpcDataManager.Instance.GetCBT_InfoList(npcInfo.BT_GroupID);
    //    if(_btInfos != null)
    //    {
    //        return _btInfos;
    //    }

    //    return null;
    //}

    //public void SetNpcBTByType(long npcUID, BT_TYPE type, long sectionid_serveruniqueid)
    //{
    //    if (NpcCurBTTypeDic.ContainsKey(npcUID))
    //    {
    //        if(NpcCurBTTypeDic[npcUID] == type)
    //        {
    //            CDebug.Log("SetNpcBTByType() npcUID["+npcUID+"] has been played. BT_Type = " + type);
    //            return;
    //        }
    //    }

    //    List<CBT_Info> _btInfos = GetBTInfos(npcUID);

    //    SectionStatusData ssd = ManagementSectionStatusManager.Instance.GetSectionStatusData(sectionid_serveruniqueid);

    //    for (int i = 0; i < _btInfos.Count; ++i)
    //    {
    //        if (ssd.sectionLevel >= _btInfos[i].SectionLv)
    //        {
    //            if (type == _btInfos[i].BT_Type)
    //            {
    //                var resData = CResourceManager.Instance.GetResourceData(_btInfos[i].BT_Name);
    //                NpcBTDic[npcUID].ExternalBehavior = resData.LoadExternalBehavior(NpcObjDic[npcUID]);

    //                if (!NpcCurBTTypeDic.ContainsKey(npcUID))
    //                {
    //                    NpcCurBTTypeDic.Add(npcUID, type);
    //                }
    //                else
    //                {
    //                    NpcCurBTTypeDic[npcUID] = type;
    //                }
    //            }
    //        }
    //    }
    //}





    public void SetNpcBT(long npcUID)
    {
        long npcID = GetNpcIDByUID(npcUID);
        CNpcInfo npcInfo = CNpcDataManager.Instance.GetNpcInfo(npcID);


        var resData = CResourceManager.Instance.GetResourceData(npcInfo.BT_ID);
        //sync--
        NpcBTDic[npcUID].ExternalBehavior = resData.LoadExternalBehavior(NpcObjDic[npcUID]);


        //Async--
        // BT 참조가 바로 일어나는듯....  
        // The behavior "Behavior" on GameObject "SNPC_1001_181106101" contains no root task. This behavior will be disabled. 에러 발생
        // var disposer = new SingleAssignmentDisposable();
        // disposer.Disposable = resData.LoadAsync<ExternalBehavior>(NpcObjDic[npcUID])
        // .Subscribe(asset => 
        // {
        //     NpcBTDic[npcUID].ExternalBehavior = asset;
        //     disposer.Dispose();
        // })
        // .AddTo(this);


    }

    public void SetNpcBTEnable(long npcUID, bool bEnable)
    {
        if (NpcBTDic.ContainsKey( npcUID ))
        {
            NpcBTDic[npcUID].enabled = bEnable;
        }
    }

    public void SetNpcObjectPosition(long npcUID, Vector3 position, Vector3 rot)
    {
        if (NpcBTDic.ContainsKey( npcUID ))
        {
            NpcControllerDic[npcUID].SetActiveNavMeshAgent( false );
            //NpcObjDic[npcUID].transform.localPosition= position;
            NpcObjDic[npcUID].transform.localRotation = Quaternion.Euler(rot);
            NpcControllerDic[npcUID].SetActiveNavMeshAgent( true );
        }
    }

    //private BT_TYPE GetBT_Type(SectionScript script, SectionStatusData sectionstatusdata)
    //{
    //    ProductionSection sectionProductionMgr = script.productionMgr;

    //    switch (sectionstatusdata.sectionTableData.Section_Type)
    //    {
    //        case (byte)Section_Type.Production:
    //            {
    //                switch(sectionstatusdata.sectionTableData.Section_Sub_Type)
    //                {
    //                    case (byte)SECTION_SUBTYPE_PRODUCT.PRODUCTION:
    //                        {
    //                            if(script.productionMgr.IsProductionStorageFull()) //생산중지
    //                            {
    //                                return BT_TYPE.PRODUCTION_WAIT;
    //                            }
    //                            else
    //                                return BT_TYPE.PRODUCTION_WORKING;
    //                        }
    //                    default:
    //                        return BT_TYPE.NONE;
    //                }
    //            }
    //        case (byte)Section_Type.Trainning:
    //            //트레이닝 대기 중 = 6               
    //            return BT_TYPE.TRAINING_WAIT;
    //        //default:
    //        //    return BT_TYPE.NONE;
    //    }


    //    //생선부서 이외의 BT_TYPE정의가 없기 때문에 정의가 생길 때까지 일단 아래와 같ㅇ 리턴한다
    //    return BT_TYPE.NONE;
    //}

	public void StartCheckingEvent(long npcUID)
    {
		if(NpcControllerDic.ContainsKey(npcUID))
		{
			NpcControllerDic[npcUID].EventProc();
			SetActiveNpcEventIcon(npcUID, false);
		}
    }

	public void StopCheckingEventOnEachNPC(long npcID)
    {
        NpcControllerDic[npcID].StopCheckingEventOnEachNPC();
    }

	public void SetAllStopCheckingEvent()
    {
        DisposeNpcTimeDisposer();

        //foreach (KeyValuePair<long, CCharacterController> cntlr in NpcControllerDic)
        foreach (KeyValuePair<long, CCharacterController_Management> cntlr in NpcControllerDic)
        {
            cntlr.Value.StopCheckingEventOnEachNPC();
        }
    }

    //public void SetChangeNpcBT(long npcUID, string btPath)
    //{
    //    if(NpcBTDic.ContainsKey(npcUID))
    //    {
    //        var resData = CResourceManager.Instance.GetResourceData(btPath);
    //        NpcBTDic[npcUID].ExternalBehavior = resData.LoadExternalBehavior(NpcObjDic[npcUID]);
    //    }
    //}

    public bool CheckHaveSameEvent(long eventID)
    {
        foreach(KeyValuePair<long, CEvent> item in NpcEventDic)
        {
			if(item.Value != null)
			{
				if(item.Value.ID == eventID)
				{
					return true;
				}
			}
        }
        return false;
    }

    public long GetNpcUidByNpcID(long npcID)
    {
        foreach (KeyValuePair<long, long> item in NpcUIDDic)
        {
            if (item.Value == npcID)
            {
                return item.Key;
            }
        }

        return 0;
    }

    public void SetNpcMailIconObj(long npcID) //npc table ID
    {
        foreach (KeyValuePair<long, long> item in NpcUIDDic)
        {
            if (item.Value == npcID)
            {
                //SetNpcMailIconUI( item.Key, npcID );
                SetNpcMailQuestIconUI( item.Key, npcID );
            }
        }
    }

    private void SetNpcMailQuestIconUI(long npcUID, long npcID)//created UID, tableID
    {
        if(NpcMailQuestIconObj.ContainsKey( npcUID ))
        {
            Vector3 pos = NpcObjDic[npcUID].transform.localPosition;

            FollowObjectPositionFor2D fop = NpcMailQuestIconObj[npcUID].transform.gameObject.GetComponent<FollowObjectPositionFor2D>();
            if (fop == null)
            {
                fop = NpcMailQuestIconObj[npcUID].transform.gameObject.AddComponent<FollowObjectPositionFor2D>();
            }
            var canvasRect = worldMgr.GetCanvasManager().GetComponent<RectTransform>();
            fop.Init( canvasRect, NpcUIDummyDic[npcUID].transform );

            NpcMailQuestIconObj[npcUID].transform.localPosition = pos;

            NpcMailQuestIconObj[npcUID].SetMailQuestData_Npc( npcID );

            NpcMailQuestIconObj[npcUID].gameObject.SetActive( true );
        }
    }


    public void SetActiveAvatarMailQuestIcon(long npcID, bool bActive)
    {
        foreach (KeyValuePair<long, long> item in NpcUIDDic)
        {
            if (item.Value == npcID)
            {
                NpcMailQuestIconObj[item.Key].gameObject.SetActive( bActive );
            }
        }
    }

    public void SetActiveAvatarMailQuestIconEffect(long npcID, bool bActive)
    {
        if (NpcMailQuestIconObj.ContainsKey(npcID))
        {
            NpcMailQuestIconObj[npcID].SetActivateEffect(bActive);
        }
    }

    public void HideEffectAvatarMailQuestIcon()
    {
        NpcMailQuestIconObj.ForEach((idx, dictionary) => dictionary.Value.SetActivateEffect(false));
    }



    //-------- below 3d mail icon don't use
    //private void SetNpcMailIconUI(long npcUID, long npcID)//created UID, tableID
    //{
    //    if(NpcMailIconObjDic.ContainsKey(npcUID) == false)
    //    {
    //        MailIcon icon = new MailIcon();
    //        GameObject orgObj = worldMgr.GetMailIconOriginObj();
    //        icon.Obj = Utility.AddChild( GetNpcUIDummyGameOgject( npcUID ), orgObj );
    //        icon.Obj.layer = LayerMask.NameToLayer( Management_LayerTag.Layer_3DUI );
    //        icon.ObjCol = icon.Obj.GetComponent<Collider>();

    //        BillBoardUI board = icon.Obj.AddComponent<BillBoardUI>();
    //        board.SetCamera( worldMgr.GetMainCameraObj() );

    //        UI3DScaler scaler = icon.Obj.AddComponent<UI3DScaler>();
    //        scaler.Init( worldMgr.CamController );

    //        NpcMailIconObjDic.Add( npcUID, icon );

    //        NpcMailIconObjDic[npcUID].Obj.SetActive( true );

    //        NpcMailIconObjDic[npcUID].ClickBtn = icon.Obj.GetComponent<ButtonByRay>();
    //        if (NpcMailIconObjDic[npcUID].ClickBtn != null)
    //        {
    //            NpcMailIconObjDic[npcUID].ClickBtn.OnSelectObjectAsObservable.Subscribe( _ =>
    //            {
    //                //do something
    //                SetActiveNpcMailIconObj( npcID, false );
    //                worldMgr.RequestManagementInteraction(GetNpcIDByUID(npcUID));
    //            } ).AddTo( this );
    //        }
    //    }
    //    else
    //    {
    //        NpcMailIconObjDic[npcUID].Obj.SetActive( true );
    //    }
    //}

    //public void SetActiveNpcMailIconObj(long npcID, bool bActive) //npc table ID
    //{
    //    foreach (KeyValuePair<long, long> item in NpcUIDDic)
    //    {
    //        if (item.Value == npcID)
    //        {
    //            NpcMailIconObjDic[item.Key].Obj.SetActive( bActive );
    //        }
    //    }
    //}

    //public void SetActiveNpcMailIconCollider(long npcID, bool bActive) //npc table ID
    //{
    //    foreach (KeyValuePair<long, long> item in NpcUIDDic)
    //    {
    //        if (item.Value == npcID)
    //        {
    //            NpcMailIconObjDic[item.Value].ObjCol.enabled =  bActive ;
    //        }
    //    }
    //}


    public void ReleaseNpcByNpcUID(long npcUID)
    {
        if(NpcUIDDic != null)
        {
            if(NpcUIDDic.ContainsKey(npcUID))
            {
                NpcUIDDic.Remove(npcUID);
            }
        }

        if (NpcObjDic != null)
        {
            if (NpcObjDic.ContainsKey(npcUID))
            {
                if (NpcObjDic[npcUID] != null)
                {
                    Destroy(NpcObjDic[npcUID]);
                }
                NpcObjDic.Remove(npcUID);
            }
        }

        if(NpcShadowObjDic != null)
        {
            if (NpcShadowObjDic.ContainsKey(npcUID))
            {
                if (NpcShadowObjDic[npcUID] != null)
                {
                    Destroy(NpcShadowObjDic[npcUID]);
                }
                NpcShadowObjDic.Remove(npcUID);
            }
        }

        if (NpcControllerDic != null)
        {
            if (NpcControllerDic.ContainsKey(npcUID))
            {
                NpcControllerDic.Remove(npcUID);
            }
        }

        if (NpcSectionScriptDic != null)
        {
            if (NpcSectionScriptDic.ContainsKey(npcUID))
            {
                NpcSectionScriptDic.Remove(npcUID);
            }
        }

        if (NpcEventDic != null)
        {
            if (NpcEventDic.ContainsKey(npcUID))
            {
                NpcEventDic.Remove(npcUID);
            }
        }

        if (NpcEventStateDic != null)
        {
            if (NpcEventStateDic.ContainsKey(npcUID))
            {
                NpcEventStateDic.Remove(npcUID);
            }
        }

        if (NpcEventIconObj != null)
        {
            if (NpcEventIconObj.ContainsKey(npcUID))
            {
                if (NpcEventIconObj[npcUID] != null)
                {
                    Destroy(NpcEventIconObj[npcUID].gameObject);
                }
                NpcEventIconObj.Remove(npcUID);
            }
        }

        if (DelayTimerDisposer != null)
        {
            if (DelayTimerDisposer.ContainsKey(npcUID))
            {
                DelayTimerDisposer.Remove(npcUID);
            }
        }

        if (NpcBTDic != null)
        {
            if (NpcBTDic.ContainsKey(npcUID))
            {
                NpcBTDic.Remove(npcUID);
            }
        }

        //if(NpcCurBTTypeDic != null)
        //{
        //    if(NpcCurBTTypeDic.ContainsKey(npcUID))
        //    {
        //        NpcCurBTTypeDic.Remove(npcUID);
        //    }
        //}

		if(NpcNametagObj != null)
		{
			if (NpcNametagObj.ContainsKey(npcUID))
            {
				if(NpcNametagObj[npcUID] != null)
				{
					Destroy(NpcNametagObj[npcUID]);
				}
                NpcNametagObj.Remove(npcUID);
            }
		}

        if(NpcMailQuestIconObj != null)
        {
            if(NpcMailQuestIconObj.ContainsKey(npcUID))
            {
                if (NpcMailQuestIconObj[npcUID] != null)
                {
                    Destroy( NpcMailQuestIconObj[npcUID].gameObject );
                }
                NpcMailQuestIconObj.Remove( npcUID );
            }
        }
    }

    public void ReleaseNpcs()
    {
        if (NpcUIDDic != null)
        {
            foreach (KeyValuePair<long, long> npc in NpcUIDDic)
            {
                ReleaseNpcByNpcUID(npc.Key);
            }
            NpcUIDDic.Clear();
        }
    }

    public void ClearNpcDics()
    {
        if (NpcUIDDic != null)
        {
            NpcUIDDic.Clear();
        }

        if (NpcObjDic != null)
        {
            foreach (KeyValuePair<long, GameObject> npcObj in NpcObjDic)
            {
                if (npcObj.Value != null)
                {
                    Destroy(npcObj.Value);
                }
            }
            NpcObjDic.Clear();
        }

        if (NpcShadowObjDic != null)
        {
            foreach (KeyValuePair<long, GameObject> shadowObj in NpcShadowObjDic)
            {
                if (shadowObj.Value != null)
                {
                    Destroy(shadowObj.Value);
                }
            }
            NpcShadowObjDic.Clear();
        }

        if (NpcControllerDic != null)
        {
            NpcControllerDic.Clear();
        }

        if (NpcSectionScriptDic != null)
        {
            NpcSectionScriptDic.Clear();
        }

        if (NpcEventDic != null)
        {
            NpcEventDic.Clear();
        }

        if (NpcEventStateDic != null)
        {
            NpcEventStateDic.Clear();
        }

        if (NpcEventIconObj != null)
        {
			foreach (KeyValuePair<long, ManagementAvatarEventUI> IconObj in NpcEventIconObj)
            {
                if (IconObj.Value != null)
                {
                    Destroy(IconObj.Value.gameObject);
                }
            }
            NpcEventIconObj.Clear();
        }

        if (DelayTimerDisposer != null)
        {
            DelayTimerDisposer.Clear();
        }

        if (NpcBTDic != null)
        {
            NpcBTDic.Clear();
        }

        //if (NpcCurBTTypeDic != null)
        //{
        //    NpcCurBTTypeDic.Clear();
        //}

        if (NpcNametagObj != null)
        {
            foreach (KeyValuePair<long, GameObject> nameTagObj in NpcNametagObj)
            {
                if (nameTagObj.Value != null)
                {
                    Destroy(nameTagObj.Value);
                }
            }

            NpcNametagObj.Clear();
        }

    }

    public void Release()
    {
        if(AsyncLoadCoroutine != null)
        {
            StopCoroutine(AsyncLoadCoroutine);
            AsyncLoadCoroutine = null;
        }

        //public Dictionary<long, CCharacterController_Management> 
        foreach(KeyValuePair<long, CCharacterController_Management> npc in NpcControllerDic)
        {
            npc.Value.ReleaseRuntimeAnimatorController(true);
            npc.Value.DestroyComponents();
            npc.Value.ReleaseHoldableObjects();
            Destroy(npc.Value);
        }
        //ReleaseNpcs();

        if (NpcUIDDic != null)
        {
            NpcUIDDic.Clear();
            NpcUIDDic = null;
        }

        if (NpcObjDic != null)
        {
            foreach(KeyValuePair<long, GameObject> npcObj in NpcObjDic)
            {
                if(npcObj.Value != null)
                {
                    Destroy(npcObj.Value);
                }
            }
            NpcObjDic.Clear();
            NpcObjDic = null;
        }

        if (NpcShadowObjDic != null)
        {
            foreach (KeyValuePair<long, GameObject> shadowObj in NpcShadowObjDic)
            {
                if (shadowObj.Value != null)
                {
                    Destroy(shadowObj.Value);
                }
            }
            NpcShadowObjDic.Clear();
            NpcShadowObjDic = null;
        }

        if (NpcControllerDic != null)
        {
            NpcControllerDic.Clear();
            NpcControllerDic = null;
        }

        if (NpcSectionScriptDic != null)
        {
            NpcSectionScriptDic.Clear();
            NpcSectionScriptDic = null;
        }

        if (NpcEventDic != null)
        {
            NpcEventDic.Clear();
            NpcEventDic = null;
        }

        if (NpcEventStateDic != null)
        {
            NpcEventStateDic.Clear();
            NpcEventStateDic = null;
        }

        if (NpcEventIconObj != null)
        {
			foreach (KeyValuePair<long, ManagementAvatarEventUI> IconObj in NpcEventIconObj)
            {
                if (IconObj.Value != null)
                {
                    Destroy(IconObj.Value);
                }
            }
            NpcEventIconObj.Clear();
            NpcEventIconObj = null;
        }

        if (DelayTimerDisposer != null)
        {
            DelayTimerDisposer.Clear();
            DelayTimerDisposer = null;
        }

        if (NpcBTDic != null)
        {
            NpcBTDic.Clear();
            NpcBTDic = null;
        }

        //if (NpcCurBTTypeDic != null)
        //{
        //    NpcCurBTTypeDic.Clear();
        //    NpcCurBTTypeDic = null;
        //}

        if (NpcNametagObj != null)
        {
            foreach (KeyValuePair<long, GameObject> nameTagObj in NpcNametagObj)
            {
                if (nameTagObj.Value != null)
                {
                    Destroy(nameTagObj.Value);
                }
            }
            
            NpcNametagObj.Clear();
            NpcNametagObj = null;
        }

        if (NpcMailQuestIconObj != null)
        {
            foreach (ManagementAvatarEventUI IconObj in NpcMailQuestIconObj.Values)
            {
                IconObj.Release();
            }
            NpcMailQuestIconObj.Clear();
            NpcMailQuestIconObj = null;
        }

        //NpcMailQuestIconObj = new Dictionary<long, ManagementAvatarEventUI>();
        //if(NpcMailIconObjDic != null)
        //{
        //    foreach (MailIcon IconObj in NpcMailIconObjDic.Values)
        //    {
        //        if (IconObj.ClickBtn != null)
        //        {
        //            IconObj.ClickBtn.Dispose();
        //        }

        //        if (IconObj.Obj != null)
        //        {
        //            Destroy( IconObj.Obj );
        //        }
        //    }
        //    NpcMailIconObjDic.Clear();
        //    NpcMailIconObjDic = null;
        //}
    }
}