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



using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EVDoorManager : MonoBehaviour
{
    private ManagementWorldManager WorldMgr;
    private EVDoorManager EVDoorMgr;
    private Dictionary<int, CEVDoorController> EVDoorCntlrDic;
    

    //private bool StartCheckArrivingTargetFloorEV;




    private const string Elevator_res_path = "NPC/Elevator/Prefab/EV_Door01.prefab";
    private const float EV_DEPTH = -4.5f;


    public void Initialize(ManagementWorldManager mgr)
    {
        WorldMgr = mgr;
        EVDoorMgr = WorldMgr.GetEVDoorManager();

        EVDoorCntlrDic = new Dictionary<int, CEVDoorController>();
    }


    public void CreateWholeElevatorDoor(int maxFloor)
    {
        for (int i = 0; i <= maxFloor; ++i)
        {
            CreateElevatorDoor(i);
        }
    }

    public void CreateElevatorDoor(int floor)
    {
        GameObject _evRoot = EVDoorMgr.transform/*.Find("World/Building/Elevators")*/.gameObject;
        Vector3 _evPos = new Vector3(0.05f, 0, EV_DEPTH);
        var resourceData = CResourceManager.Instance.GetResourceData(Elevator_res_path);
        GameObject tmpEvObj = resourceData.Load<GameObject>(ManagementManager.Instance.WorldMgrObj);

        GameObject evObj = Utility.AddChild(_evRoot, tmpEvObj);

        _evPos.y = floor * WorldMgr.GridElementHeight;
        evObj.transform.localPosition = _evPos;

        CEVDoorController _evController = evObj.AddComponent<CEVDoorController>();
        AddEVControllerByFloor(floor, _evController);
    }

    private void Update()
    {
        if(EVDoorCntlrDic != null)
        {
            if(EVDoorCntlrDic.Count > 0)
            {
                CheckEVArriveAtStartFloor();

                //if(StartCheckArrivingTargetFloorEV)
                //{
                //    CheckEVArriveAtTargetFloor();
                //}
            }
        }
    }


    public void AddEVControllerByFloor(int floor, CEVDoorController controller)
    {
        CDebug.Log("    @@@@@@@@          AddEVControllerByFloor floor = " + floor + "/controller = " + controller);
        if (EVDoorCntlrDic.ContainsKey(floor) == false)
        {
            EVDoorCntlrDic.Add(floor, controller);
            EVDoorCntlrDic[floor].Initialize(WorldMgr, floor);
        }

        if (EVDoorCntlrDic[floor] == null)
        {
            EVDoorCntlrDic[floor] = controller;
            EVDoorCntlrDic[floor].Initialize(WorldMgr, floor);
        }
    }

    public void InitAllEvController()
    {
        foreach(CEVDoorController ev in EVDoorCntlrDic.Values)
        {
            ev.InitDoor();
        }
    }

    public void CallEVByFloor(int floor, AVATAR_TYPE aType)
    {
        CEVDoorController _evDoorCntlr = GetEVDoorController(floor);
        if (_evDoorCntlr != null)
        {
            _evDoorCntlr.CallEV(aType);
        }
    }

    private void CheckEVArriveAtStartFloor()
    {
        foreach (CEVDoorController evCntlr in EVDoorCntlrDic.Values)
        {
            //check ev can open door??
            //somebody(avatar) is coming this floor??
            //if come, wait
            if (evCntlr.HasArriveAtStartFloor())
            {
                //open door
                if(evCntlr.CanOpenEVDoor())
                {
                    evCntlr.SetChangeEVDoorState(EVDoorSM_OpenSFDoor.Instance());
                }
            }
        }
    }


    public bool HasArrivedStartFloorEV(int floor)
    {
        CEVDoorController _evDoorCntlr = GetEVDoorController(floor);
        if (_evDoorCntlr != null)
        {
            if (
                   _evDoorCntlr.GetCurrentEVDoorState() == EVDoorSM_OpenSFDoor.Instance()
                || _evDoorCntlr.GetCurrentEVDoorState() == EVDoorSM_OpenTFDoor.Instance()
                || _evDoorCntlr.GetCurrentEVDoorState() == EVDoorSM_Wait.Instance()
                )
            {
                return _evDoorCntlr.HasDoorOpened();// HasSFDoorOpened();
            }
        }

        return false;
    }

    public void SetFinishEVDoorOpen(int floor)
    {
        CEVDoorController _evDoorCntlr = GetEVDoorController(floor);
        if (_evDoorCntlr != null)
        {
            _evDoorCntlr.SetFinishSFDoorOpen(true);
        }
    }


    public bool HasEVSFDoorClose(int floor)
    {
        CEVDoorController _evDoorCntlr = GetEVDoorController(floor);
        if (_evDoorCntlr != null)
        {
            //if (_evDoorCntlr.GetCurrentEVDoorState() == EVDoorSM_Wait.Instance())
            {
                return _evDoorCntlr.HasDoorClosed();
            }
        }

        return false;
    }

    public CState<CEVDoorController> GetEVCurrentState(int floor)
    {
        CEVDoorController _evDoorCntlr = GetEVDoorController(floor);
        if(_evDoorCntlr != null)
        {
            return _evDoorCntlr.GetCurrentEVDoorState();
        }

        return null;
    }

    //all registed avatars finish getting in ev??
    //if not, wait
    public void CloseSFDoor(int floor)
    {
        CEVDoorController _evDoorCntlr = GetEVDoorController(floor);

        if (_evDoorCntlr != null)
        {
            if (_evDoorCntlr.CanCloseEVDoor())
            {
                _evDoorCntlr.SetChangeEVDoorState(EVDoorSM_CloseSFDoor.Instance());
            }
        }
    }

    public void OpenTFDoor(int floor)
    {
        CEVDoorController _evDoorCntlr = GetEVDoorController(floor);
        if (_evDoorCntlr != null)
        {
            if (_evDoorCntlr.CanOpenEVDoor())
            {
                _evDoorCntlr.SetChangeEVDoorState(EVDoorSM_OpenTFDoor.Instance());
            }
        }
    }

    public bool HasOpenTFDoor(int floor)
    {
        CEVDoorController _evDoorCntlr = GetEVDoorController(floor);
        if (_evDoorCntlr != null)
        {
            return _evDoorCntlr.HasDoorOpened();// HasTFDoorOpened();
        }

        return false;
    }

    public void CloseTFDoor(int floor)
    {
        CEVDoorController _evDoorCntlr = GetEVDoorController(floor);
        if (_evDoorCntlr != null)
        {
            if (_evDoorCntlr.CanCloseEVDoor())
            {
                _evDoorCntlr.SetChangeEVDoorState(EVDoorSM_CloseTFDoor.Instance());
            }
        }
    }

    public void AddPassenger(AVATAR_TYPE aType, int floor)
    {
        CEVDoorController _evDoorCntlr = GetEVDoorController(floor);
        if (_evDoorCntlr != null)
        {
            _evDoorCntlr.AddPassenger(aType);
        }
    }


    public CEVDoorController GetEVDoorController(int floor)
    {
        if (EVDoorCntlrDic.ContainsKey(floor))
        {
            return EVDoorCntlrDic[floor];
        }

        return null;
    }

    public List<AVATAR_TYPE> GetRegisterList(int floor)
    {
        CEVDoorController _evDoorCntlr = GetEVDoorController(floor);
        if (_evDoorCntlr != null)
        {
            return _evDoorCntlr.GetRegisterList();
        }

        return null;
    }

    public int GetAvatarRegisterIdx(int floor, AVATAR_TYPE aType)
    {
        List<AVATAR_TYPE> _list = GetRegisterList(floor);
        if (_list != null)
        {
            for (int i = 0; i < _list.Count; ++i)
            {
                if (aType == _list[i])
                {
                    return i;
                }
            }
        }

        return 0;
    }

    public List<AVATAR_TYPE> GetPassengerList(int floor)
    {
        CEVDoorController _evDoorCntlr = GetEVDoorController(floor);
        if (_evDoorCntlr != null)
        {
            return _evDoorCntlr.GetPassengerList();
        }

        return null;
    }

    public int GetAvatarPassengerIdx(int floor, AVATAR_TYPE aType)
    {
        List<AVATAR_TYPE> _list = GetPassengerList(floor);
        if(_list != null)
        {
            for(int i=0; i<_list.Count; ++i)
            {
                if(aType == _list[i])
                {
                    return i;
                }
            }
        }

        return 0;// -1;
    }

    public void CloseAllEvDoor()
    {
        foreach (KeyValuePair<int, CEVDoorController> ev in EVDoorCntlrDic)
        {
            if (ev.Value.CanCloseEVDoor())
            {
                ev.Value.MyAnimator.SetTrigger( "close" );
            }
            ev.Value.InitEVInfo();
        }
    }

    public void RemoveEVList(int startFloor, int targetFloor)
    {
        CEVDoorController _evDoorCntlr = GetEVDoorController(startFloor);
        if (_evDoorCntlr != null)
        {
            _evDoorCntlr.ClearAllRegister();
        }

        _evDoorCntlr = GetEVDoorController(targetFloor);
        if (_evDoorCntlr != null)
        {
            _evDoorCntlr.ClearAllPassenger();
        }
    }

    public void RemoveAvatarInEVList(AVATAR_TYPE aType, int startFloor, int targetFloor)
    {
        CEVDoorController _evDoorCntlr = GetEVDoorController(startFloor);
        if (_evDoorCntlr != null)
        {
            _evDoorCntlr.RemoveRegister(aType);
        }

        _evDoorCntlr = GetEVDoorController(targetFloor);
        if (_evDoorCntlr != null)
        {
            _evDoorCntlr.RemovePassenger(aType);
        }
    }

    public void Release()
    {
        if (EVDoorCntlrDic != null)
        {
            foreach (KeyValuePair<int, CEVDoorController> cntlr in EVDoorCntlrDic)
            {
                cntlr.Value.Release();
            }
            EVDoorCntlrDic.Clear();
            EVDoorCntlrDic = null;
        }
    }
}
