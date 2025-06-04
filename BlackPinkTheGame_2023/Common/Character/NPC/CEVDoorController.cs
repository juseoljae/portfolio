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

#endregion

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public class CEVDoorController : MonoBehaviour
{
    [HideInInspector]
    public ManagementWorldManager WorldMgr;
    [HideInInspector]
    public ManagementAvatarManager AvatarMgr;
    [HideInInspector]
    public EVDoorManager EVDoorMgr;
    [HideInInspector]
    public Animator MyAnimator;
    [HideInInspector]
    public CStateMachine<CEVDoorController> EVDoorSM;


    private int MyFloor;
    private List<AVATAR_TYPE> RegisterList;
    private List<AVATAR_TYPE> PassengerList;
    
    //checker
    private bool bCheckComing;
    private bool bArrivedAtStartFloor;
    private bool bFinishDoorOpen;
    

    private SingleAssignmentDisposable TimeDisposer;

    public void Initialize(ManagementWorldManager mgr, int floor)
    {
        WorldMgr = mgr;
        AvatarMgr = WorldMgr.AvatarMgr;
        EVDoorMgr = WorldMgr.GetEVDoorManager();
        MyAnimator = transform.GetComponent<Animator>();
        EVDoorSM = new CStateMachine<CEVDoorController>(this);

        MyFloor = floor;
        RegisterList = new List<AVATAR_TYPE>();
        PassengerList = new List<AVATAR_TYPE>();

        InitDoor();
    }

    public void InitEVInfo()
    {
        ClearAllPassenger();
        ClearAllRegister();
        InitDoor();
        InitTimeDisposer();
    }

    public void InitDoor()
    {
        SetFinishSFDoorOpen(false);
        InitCheckVar();
    }

    public void InitCheckVar()
    {
        bCheckComing = false;
        bArrivedAtStartFloor = false;
    }

    public void InitTimeDisposer()
    {
        if (TimeDisposer == null)
        {
            TimeDisposer = new SingleAssignmentDisposable();
        }
        else
        {
            TimeDisposer.Dispose();
            TimeDisposer = new SingleAssignmentDisposable();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (EVDoorSM != null)
        {
            EVDoorSM.StateMachine_Update();
        }

        //나에게 오고 있는 아바타가 있나? 있으면 도착할 때까지 체크.
        if(bCheckComing)
        {
            if (RegisterList.Count > 0)
            {
                if (WorldMgr.AvatarMgr.HasComingAvatarArrivedAtStartFloor(RegisterList[0]))
                {
                    if (bFinishDoorOpen == false)
                    {
                        bArrivedAtStartFloor = true;
                    }
                }
            }
        }
    }

    public void CallEV(AVATAR_TYPE register)
    {
        AddRegisterList(register);
    }

    public bool HasArriveAtStartFloor()
    {
        return bArrivedAtStartFloor;
    }

    public void SetFinishSFDoorOpen(bool bFinish)
    {
        bFinishDoorOpen = bFinish;
    }

    public bool HasDoorOpened()
    {
        return bFinishDoorOpen;
    }


    public bool HasDoorClosed()
    {
        return !bFinishDoorOpen;
    }

    public int GetEVFloor()
    {
        return MyFloor;
    }

    private void AddRegisterList(AVATAR_TYPE register)
    {
        if(RegisterList.Exists(x => x == register))
        {
            CDebug.LogError($"{register} already registed in my {MyFloor}F ev door");
            return;
        }

        RegisterList.Add(register);

        CCharacterController_Management _registerCntlr = WorldMgr.AvatarMgr.GetManagementAvatarController(register);        
        if(_registerCntlr.GetComingAvatarInEV())
        {
            bCheckComing = true;
            //CDebug.LogError($"register = {register}, ComingAvatar = {_registerCntlr} {MyFloor}F ev door bCheckComing = true");
        }
        else
        {
            if (bFinishDoorOpen == false)
            {
                //CDebug.LogError($"register = {register}, ComingAvatar = {_registerCntlr} {MyFloor}F ev door bArrivedAtStartFloor = true open SF door");
                bArrivedAtStartFloor = true;
            }
        }
    }

    public void ClearAllRegister()
    {
        RegisterList.Clear();
    }

    public List<AVATAR_TYPE> GetRegisterList()
    {
        return RegisterList;
    }

    public void RemoveRegister(AVATAR_TYPE aType)
    {
        if(RegisterList.Exists(avatar => avatar == aType))
        {
            RegisterList.Remove(aType);
        }
    }


    public void AddPassenger(AVATAR_TYPE passenger)
    {
        if (PassengerList.Exists(x => x == passenger))
        {
            CDebug.LogError($"{passenger} already registed in my {MyFloor}F ev door");
            return;
        }

        PassengerList.Add(passenger);
    }

    public void ClearAllPassenger()
    {
        PassengerList.Clear();
    }

    public List<AVATAR_TYPE> GetPassengerList()
    {
        return PassengerList;
    }

    public void RemovePassenger(AVATAR_TYPE aType)
    {
        if (PassengerList.Exists(avatar => avatar == aType))
        {
            PassengerList.Remove(aType);
        }
    }

    public void SetChangeEVDoorState(CState<CEVDoorController> state)
    {
        EVDoorSM.ChangeState(state);
    }

    public CState<CEVDoorController> GetCurrentEVDoorState()
    {
        return EVDoorSM.GetCurrentState();
    }

    public void OpenSFDoor()
    {
        //CDebug.Log($"   ### CEVDoorController.OpenSFDoor() {MyFloor} F");
        InitTimeDisposer();
        TimeDisposer.Disposable = Observable.Timer(System.TimeSpan.FromSeconds(1))
            .Subscribe(_ =>
            {
                //CDebug.Log($"   ### CEVDoorController.OpenSFDoor() {MyFloor} F   bFinishDoorClose = false");
                SetFinishSFDoorOpen(true);
                TimeDisposer.Dispose();
            });
    }


    public void OpenTFDoor()
    {
        //CDebug.Log($"   ### CEVDoorController.OpenTFDoor() {MyFloor} F");
        InitTimeDisposer();
        TimeDisposer.Disposable = Observable.Timer(System.TimeSpan.FromSeconds(1))
            .Subscribe(_ =>
            {
                //CDebug.Log($"   ### CEVDoorController.OpenTFDoor() {MyFloor} F   bFinishDoorClose = false");
                SetFinishSFDoorOpen(true);
                TimeDisposer.Dispose();
            });
    }


    public void CloseDoor()
    {
        //CDebug.Log($"   ### CEVDoorController.CloseDoor() {MyFloor} F");
        SetChangeEVDoorState(EVDoorSM_Wait.Instance());
        InitTimeDisposer();
        TimeDisposer.Disposable = Observable.Timer(System.TimeSpan.FromSeconds(3))
            .Subscribe(_ =>
            {
                //CDebug.Log($"   ### CEVDoorController.CloseDoor() {MyFloor} F   bFinishDoorClose = true");
                SetFinishSFDoorOpen(false);
                TimeDisposer.Dispose();
            });
    }

    public void CloseDoorAnyway()
    {
        if (GetCurrentEVDoorState() == EVDoorSM_OpenSFDoor.Instance() ||
            GetCurrentEVDoorState() == EVDoorSM_OpenTFDoor.Instance())
        {
            SetChangeEVDoorState(EVDoorSM_CloseTFDoor.Instance());
        }
    }


    public bool CanOpenEVDoor()
    {
        if (GetCurrentEVDoorState() == EVDoorSM_OpenSFDoor.Instance()) return false;
        if (GetCurrentEVDoorState() == EVDoorSM_OpenTFDoor.Instance()) return false;
        if (HasDoorOpened()) return false;

        return true;
    }

    public bool CanCloseEVDoor()
    {
        if (GetCurrentEVDoorState() == EVDoorSM_Wait.Instance()) return false;
        if (GetCurrentEVDoorState() == EVDoorSM_CloseSFDoor.Instance()) return false;
        if (GetCurrentEVDoorState() == EVDoorSM_CloseTFDoor.Instance()) return false;
        if (HasDoorClosed()) return false;

        return true;
    }


    public void Release()
    {
        if (WorldMgr != null) WorldMgr = null;
        if (AvatarMgr != null) AvatarMgr = null;
        if (EVDoorMgr != null) EVDoorMgr = null;
        if (MyAnimator != null) MyAnimator = null;
        if (EVDoorSM != null) EVDoorSM = null;
        if (RegisterList != null)
        {
            RegisterList.Clear();
            RegisterList = null;
        }
        if(PassengerList != null)
        {
            PassengerList.Clear();
            PassengerList = null;
        }
        if(TimeDisposer != null)
        {
            TimeDisposer.Dispose();
            TimeDisposer = null;
        }
    }
}
