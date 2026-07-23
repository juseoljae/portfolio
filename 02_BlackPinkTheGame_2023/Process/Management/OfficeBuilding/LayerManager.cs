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

//using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using TMPro;
using GroupManagement.ManagementEnums;
using System;

public class LayerManager : MonoBehaviour
{
    public ManagementWorldManager worldMgr;
    private Dictionary<int, LayerScript> dicLayerScriptDic;

    private float progressBarTargetValue;
    public float pbTargetValue
    {
        set { progressBarTargetValue = value; }
    }
    private ManagementMapData LayerInfo;
    public CStateMachine<LayerManager> layerStateMachine;

    //open timer
    private SingleAssignmentDisposable timerDisposerSectionOpen;// = new SingleAssignmentDisposable();
    private float remainTimeSeconds;
    private GameObject ConstructionScreenObj;

    //3D UI
    private GameObject go3DUIRoot;
    private GameObject goBorderRoot;
    private SpriteRenderer srProgressBarFG;
    private SpriteRenderer srProgressBarFill;
    private TextMeshPro txtTMPProgressBar;

    private PopupSectionTimeReduction sectionTimeReductionPopup;

    private const byte LAYER_ELEVATOR_SIZE = 8;
    private const byte LAYER_RIGHT_SIDE_SIZE = 2;
    private const byte LAYER_WORKSCREEN_SIDESPACE_SIZE = LAYER_ELEVATOR_SIZE + LAYER_RIGHT_SIDE_SIZE; // 엘리베이터 + 우측 외벽 //37; // 증축 공사중 이펙트 사이즈
    //private int CurExtendLayerFloor;
    

    private float LayerWidthMidPos;

    public void InitializeLayerManager()
    {
        this.worldMgr = ManagementManager.Instance.worldManager;
        layerStateMachine = new CStateMachine<LayerManager>(this);

        timerDisposerSectionOpen = new SingleAssignmentDisposable();

        LayerWidthMidPos = 
            ((worldMgr.GridElementWidth * Constants.GridSizeX) + LAYER_WORKSCREEN_SIDESPACE_SIZE) / 2 - LAYER_ELEVATOR_SIZE;

        //Transform midPosObj = transform.Find("MidPos");
        //LayerWidthMidPos = midPosObj.position.x;
        //midPosObj.gameObject.SetActive(false);
    }

    public void AddLayerScript(int floorno, LayerScript script)
    {
        if (dicLayerScriptDic == null)
        {
            dicLayerScriptDic = new Dictionary<int, LayerScript>();
        }

        if (dicLayerScriptDic.ContainsKey(floorno) == false)
        {
            CDebug.Log("LayerManager.AddLayer floor = "+floorno);
            dicLayerScriptDic.Add(floorno, script);
        }
        else
        {
            CDebug.Log("already exist!!");
            dicLayerScriptDic[floorno] = script;
        }
    }


    // Update is called once per frame
    void FixedUpdate()
    {
        if (layerStateMachine != null)
        {
            layerStateMachine.StateMachine_Update();
        }
    }



    //public LayerScript GetLayerScriptByFloorNo(int floorno)
    //{
    //    if(dicLayerScriptDic.ContainsKey(floorno))
    //    {
    //        return dicLayerScriptDic[floorno];
    //    }

    //    return null;
    //}

    public Vector3 GetExtendLayerMidPos()
    {
        int floor = worldMgr.GetTopFloor();
        Vector3 camPos = worldMgr.CamController.GetCameraPosition();
        Vector3 midPos = new Vector3(LayerWidthMidPos, floor * worldMgr.GridElementHeight + worldMgr.GridElementHeight/2, camPos.z);
        return midPos;
    }

    public void SetExtendLayerFocus()
    {
        Vector3 layerMidPos = GetExtendLayerMidPos();
        //worldMgr.SetCameraFocusToPosition(layerMidPos);
        worldMgr.SetCameraZoomToPosition(layerMidPos, false);
    }



    ///////////////////////////////////////////////////////////////////////////////////////////


    //public void SetLayerInfo(ManagementMapData info)
    //{
    //    LayerInfo = info;
    //}

    //Timer
    public void ShowLayerOpenUnderConstructionUI(bool bshow, int floorIdx)
    {
        if (bshow)
        {
            if (timerDisposerSectionOpen != null)
            {
                timerDisposerSectionOpen.Dispose();
            }

            //CurExtendLayerFloor = floorIdx;

            timerDisposerSectionOpen = new SingleAssignmentDisposable();

            string streamKey = CDefines.MANAGEMENT_LAYEROPEN_TIMESTREAM_KEY;

            if (GlobalTimer.Instance.HasTimeStream(streamKey))
            {
                TimeStream timeStream;
                GlobalTimer.Instance.GetTimeStream(out timeStream, streamKey);
                if (timeStream != null)
                {
                    timerDisposerSectionOpen.Disposable = timeStream.OnTimeStreamObservable().Subscribe(UpdateRemainTimeLayerOpen).AddTo(this);
                }
                else
                {
                    CDebug.Log("Do not exist timestream!!");
                }

                ShowUnderconstructionUIProgressBar();
            }
        }
        else
        {
            ReleaseTimer();
        }
    }

    public void ShowUnderconstructionUIProgressBar()
    {
        var resData = CResourceManager.Instance.GetResourceData(Constants.PATH_3DUI_GAUGE);
        GameObject obj = Instantiate(resData.Load<GameObject>(gameObject));
        obj.transform.parent = go3DUIRoot.transform;
        obj.transform.localPosition = new Vector3(LayerWidthMidPos, worldMgr.GridElementHeight * worldMgr.GetTopFloor() + worldMgr.GridElementHeight/2, -1.2f);
        srProgressBarFG = obj.transform.Find("fg").GetComponent<SpriteRenderer>();
        srProgressBarFill = obj.transform.Find("fill").GetComponent<SpriteRenderer>();
        txtTMPProgressBar = obj.transform.Find("Text").GetComponent<TextMeshPro>();
        txtTMPProgressBar.text = string.Empty;
        UI3DScaler scaler = obj.AddComponent<UI3DScaler>();
        scaler.Init(worldMgr.CamController);
    }

    public void ConstructionColliderOff()
    {
        if (worldMgr.ConstructionFloorObj)
        {
            worldMgr.ConstructionFloorCol.enabled = false;
        }
    }

    public void ConstructionColliderOn()
    {
        if (worldMgr.ConstructionFloorObj)
        {
            worldMgr.ConstructionFloorCol.enabled = true;
        }
    }

    private void UpdateRemainTimeLayerOpen(TimeStreamData timeStreamData)
    {
        if (timeStreamData.IsEnd == true)
        {
            timerDisposerSectionOpen.Dispose();

            ConstructionColliderOff();
            layerStateMachine.ChangeState(ManagementLayerState_OpenTimeComplete.Instance());
            //if (worldMgr.MainSM.CurState == ManagementWorldState_Relocation.Instance() ||
            //    worldMgr.GetBuildingManager().stateMachine.CurState == BuildingState_AddSection_SelectBuildPosition.Instance())
            //{
            //    layerStateMachine.ChangeState(ManagementLayerState_OpenTimeComplete_In_LockMode.Instance());
            //}
            //else
            //{
            //    layerStateMachine.ChangeState(ManagementLayerState_OpenTimeComplete.Instance());
            //}

            if (sectionTimeReductionPopup != null)
            {
                sectionTimeReductionPopup.SetPrice((long)remainTimeSeconds);
                sectionTimeReductionPopup.RefreshItemList();
            }

            //! 건물 증축(TimeOut)
            LocalPushManager.Instance.SetLayerExpansion();

            return;
        }

        remainTimeSeconds = timeStreamData.CurrentTime;
        //UnityEngine.Debug.Log("section open remain time : " + remainTimeSeconds);

        if (srProgressBarFill != null)
        {
            Vector2 size = srProgressBarFill.size;

            //UnityEngine.Debug.Log(" remain time : " + remainTimeSeconds+"//"+ progressBarTargetValue+"/"+ srProgressBarFG.size);

            size.x = ((progressBarTargetValue - remainTimeSeconds) / progressBarTargetValue) * srProgressBarFG.size.x;
            srProgressBarFill.size = size;
        }

        if (txtTMPProgressBar != null)
        {
            // 0017386 이슈
            txtTMPProgressBar.text = UtilityTimeFormat.GetTimeTextFormatByBPTG((long)remainTimeSeconds, true);//.ToString();
        }

        if (sectionTimeReductionPopup != null)
        {
            sectionTimeReductionPopup.SetPrice((long)remainTimeSeconds);
            //sectionTimeReductionPopup.RefreshItemList();
        }
    }


    void ReleaseTimer()
    {
        if (timerDisposerSectionOpen != null)
        {
            timerDisposerSectionOpen.Dispose();
        }

        RemoveAll3DUIObjects();
    }

    public void RemoveAll3DUIObjects()
    {
        RemoveAllUIObjects(go3DUIRoot);
        go3DUIRoot = null;
    }

    public void Remove3DUIBorder()
    {
        RemoveAllUIObjects(goBorderRoot);
        goBorderRoot = null;
    }

    void RemoveAllUIObjects(GameObject targetobject)
    {
        Transform[] childList;
        if (targetobject != null)
        {
            childList = targetobject.transform.GetComponentsInChildren<Transform>(true);
            if (childList != null)
            {
                if (childList != null)
                {
                    for (int i = 1; i < childList.Length; i++)
                    {
                        if (childList[i] != transform)
                            Destroy(childList[i].gameObject);
                    }
                }
            }
        }
    }

    public void CreateLayer3DRoot()
    {
        if (go3DUIRoot == null)
        {
            go3DUIRoot = new GameObject(CDefines.MANAGEMENT_3DUI_OBJECT_ROOT);
            go3DUIRoot.transform.parent = transform;
            go3DUIRoot.transform.localPosition = Vector3.zero;
        }
        if (goBorderRoot == null)
        {
            goBorderRoot = new GameObject(CDefines.MANAGEMENT_3DUI_BODER_ROOT);
            goBorderRoot.transform.parent = transform;
            goBorderRoot.transform.localPosition = Vector3.zero;
        }
    }


    public void ShowLayerOpenCompleteBorder()
    {
        RemoveAll3DUIObjects();


        string strPath = Constants.PATH_3DUI_DONE;

        var resData = CResourceManager.Instance.GetResourceData(strPath);
        GameObject obj = Instantiate(resData.Load<GameObject>(gameObject));
        
        if (goBorderRoot == null)
        {
            goBorderRoot = new GameObject(CDefines.MANAGEMENT_3DUI_BODER_ROOT);
            goBorderRoot.transform.parent = transform;
            goBorderRoot.transform.localPosition = Vector3.zero;
        }
        
        obj.transform.parent = goBorderRoot.transform;

        SpriteRenderer SR = obj.transform.Find("line_bg").GetComponent<SpriteRenderer>();
        SR.size = new Vector2((worldMgr.GridElementWidth * Constants.GridSizeX) + LAYER_WORKSCREEN_SIDESPACE_SIZE, worldMgr.GridElementHeight);

        //obj.transform.GetComponent<BoxCollider>().size = new Vector3(SR.size.x, SR.size.y, 1f);
        BoxCollider col = obj.transform.GetComponent<BoxCollider>();
        if (col == null)
        {
            col = obj.AddComponent<BoxCollider>();
        }        
        col.size = new Vector3(SR.size.x, SR.size.y, 1f);
        obj.transform.localPosition = new Vector3(LayerWidthMidPos, worldMgr.GetTopFloor() * worldMgr.GridElementHeight + worldMgr.GridElementHeight / 2, -1.2f);

        UI3DScaler scaler = obj.AddComponent<UI3DScaler>();
        scaler.Init(worldMgr.CamController, true);

        ButtonByRay btnborder = obj.GetComponent<ButtonByRay>();
        if(btnborder == null)
        {
            btnborder = obj.AddComponent<ButtonByRay>();
        }
        btnborder.OnSelectObjectAsObservable
        .Subscribe(_ =>
        {
            //SoundManager.Instance.PlayEffect(6810033); // management sound : 증축 완료 아이콘 터치 (se_ui_017)
            //RemoveAllUIObjects(goBorderRoot);
            layerStateMachine.ChangeState(ManagementLayerState_OpenComplete.Instance());
        }).AddTo(this);
        obj.SetActive(true);
    }


    public void SetConstructionScreenObject()
    {
        if (worldMgr.ConstructionFloorObj.activeSelf == false)
        {
            worldMgr.ConstructionFloorObj.transform.parent = transform.parent;
            worldMgr.ConstructionFloorObj.transform.localPosition = new Vector3(0, worldMgr.GetTopFloor() * worldMgr.GridElementHeight, -0.9f);
            
            worldMgr.ConstructionFloorCol.enabled = true;
            worldMgr.ConstructionFloorObj.SetActive(true);

            var ConstructionButtonByRay = worldMgr.ConstructionFloorObj.GetComponent<ButtonByRay>();

            if (ConstructionButtonByRay != null)
            {
				ConstructionButtonByRay.OnSelectObjectAsObservable.Subscribe(_ =>
				{
					sectionTimeReductionPopup = SetTimeReductionPopup();
				} ).AddTo(this);
            }
        }
    }

    public IObservable<PopupSectionTimeReduction.Result> SetTimeReductionPopupForWorkManPopup()
    {
        var ConstructionButtonByRay = worldMgr.ConstructionFloorObj.GetComponent<ButtonByRay>();
        ManagementCanvasManager canvasMgr = (ManagementCanvasManager)ManagementManager.Instance.GetCanvasManager();
        sectionTimeReductionPopup = canvasMgr.GetPopupService().ShowSectionTimeReductionPopup();
        sectionTimeReductionPopup.SetData(new PopupSectionTimeReduction.Setting()
        {
            sectionScript = null,
        });

        sectionTimeReductionPopup.SetPopupUI(
            TimeReducePopup_Type.FLOOREXPANSION,
            OnClickBtnCompleteNowCallback,
            OnClickReduceTimeCallback,
            OnClickADReduceTimeCallback
            );

        return sectionTimeReductionPopup.ShowAsObservable();
    }

    public PopupSectionTimeReduction SetTimeReductionPopup()
    {
		var ConstructionButtonByRay = worldMgr.ConstructionFloorObj.GetComponent<ButtonByRay>();
        ManagementCanvasManager canvasMgr = (ManagementCanvasManager)ManagementManager.Instance.GetCanvasManager();
        var popup = canvasMgr.GetPopupService().ShowSectionTimeReductionPopup();
        popup.SetData(new PopupSectionTimeReduction.Setting()
        {
            sectionScript = null,
        });

        popup.SetPopupUI(
            TimeReducePopup_Type.FLOOREXPANSION,
            OnClickBtnCompleteNowCallback,
            OnClickReduceTimeCallback,
            OnClickADReduceTimeCallback
            );

		SingleAssignmentDisposable disposer = new SingleAssignmentDisposable ();
        disposer.Disposable = popup.ShowAsObservable ().Subscribe (_ =>
        {

            disposer.Dispose ();
            if (popup != null)
            {
                disposer = null;
            }
        });
        //ShowAsObservable가 없는데 popup이 뜨는 이유를 찾아라.

        return popup;
    }

    //캐시로 즉시 완료
    private void OnClickBtnCompleteNowCallback()
    {
        Observable.ReturnUnit()
            .Do (_ => NotifiLayer.Active = false)
            .SelectMany(_=> APIHelper.ManagementSvc.Management_Layer_Buy_Complete (worldMgr.GetTopFloor ()))        
            .Do(res =>
            {
                if (res.managementMapInfo != null)
                {
                    if (res.managementMapInfo.status == (int)ENUM_BUILD_STATE.COMPLETE)
                    {
                        ManagementServerDataManager.Instance.UpdateLayerData(res.managementMapInfo);
                        ManagementServerDataManager.Instance.SetCompleteLayerOpenData(res.managementMapInfo);
                        ManagementServerDataManager.Instance.UpdateWorkerInfo(res.managementWorkerInfo);

                        layerStateMachine.ChangeState(ManagementLayerState.Instance());
                        ManagementPageUI pageUI = ((ManagementCanvasManager)ManagementManager.Instance.GetCanvasManager()).PageUI;
                        pageUI.roomUI.SetHasBeenStartBuilding(false);
                        pageUI.roomUI.SetActiveBuildState (true);
                        RemoveConstructionScreenObject();
                        RemoveTimeStream();
                        ReleaseTimer();

                        //! 건물 증축(즉시완료)
                        LocalPushManager.Instance.SetLayerExpansion();

                        if (sectionTimeReductionPopup != null)
                        {
                            sectionTimeReductionPopup.ClosePopup();
                        }
                    }
                }
            })
            .SelectMany (res => UIManager.Try_Unlock ().Select (_ => res))
            .Do (_ => NotifiLayer.Active = true)
            .Do (res =>
            {
                string msgcomplete = string.Format (CResourceManager.Instance.GetString (92280015), res.managementMapInfo.layer + 1);
                Toast.MakeTextForContent (msgcomplete, Msg.MSG_TYPE.Notice);
            })
            .Do(_ => ShopManager.Instance.RequireShopCheck())
            .Subscribe ()
            .AddTo(this);
    }

    //시간 단축 광고 사용
    private void OnClickADReduceTimeCallback()
    {
        ADTable.ADTableData adData = ADTable.Instance.GetAD(AD_TYPE.HASTE);
        //SoundManager.Instance.PlayEffect(6810018); // management sound : 증축 시간단축 아이템 사용 (se_ui_002)
        Observable.ReturnUnit ()
            .Do (_ => NotifiLayer.Active = false)
            .SelectMany (_ => APIHelper.ManagementSvc.Management_Layer_Use_Ad (worldMgr.GetTopFloor (), 1, adData.ID))        
            .Do(res =>
            {
                ManagementServerDataManager.Instance.UpdateLayerData(res.managementMapInfo);
                ManagementServerDataManager.Instance.UpdateWorkerInfo(res.managementWorkerInfo);
                var mapInfo = ManagementServerDataManager.Instance.GetManagementMapData(worldMgr.GetTopFloor());
                if (mapInfo != null)
                {
                    AdvertiseController.Instance.UpdateAdDic(res.managementAdInfo);

                    if (mapInfo.remain_mts <= 0)
                    {
                        //layerStateMachine.ChangeState(ManagementLayerState_OpenTimeComplete.Instance());

                        layerStateMachine.ChangeState(ManagementLayerState.Instance());
                        ManagementPageUI pageUI = ((ManagementCanvasManager)ManagementManager.Instance.GetCanvasManager()).PageUI;
                        pageUI.roomUI.SetHasBeenStartBuilding(false);
                        pageUI.roomUI.SetActiveBuildState(true);

                        RemoveTimeStream();
                        ReleaseTimer();
                        ConstructionColliderOff();
                        RemoveConstructionScreenObject();
                        timerDisposerSectionOpen.Dispose();

                        if (sectionTimeReductionPopup != null)
                        {
                            sectionTimeReductionPopup.ClosePopup();
                        }
                    }
                    else
                    {
                        TimeStream timeStream;
                        string streamKey = CDefines.MANAGEMENT_LAYEROPEN_TIMESTREAM_KEY;
                        ManagementMapData layerSvrInfo = GetLayerServerInfo();
                        double remainTimeMillieSeconds = layerSvrInfo.remain_mts;
                        double totalSecond = TimeSpan.FromMilliseconds(remainTimeMillieSeconds).TotalSeconds;

                        GlobalTimer.Instance.GetTimeStream(out timeStream, streamKey);
                        timeStream.SetTime(streamKey, (float)totalSecond, 0, TimeStreamType.DECREASE).OnTimeStreamObservable().Subscribe();
                        if (timerDisposerSectionOpen != null)
                        {
                            timerDisposerSectionOpen.Dispose();
                        }
                        timerDisposerSectionOpen = new SingleAssignmentDisposable();
                        timerDisposerSectionOpen.Disposable = timeStream.OnTimeStreamObservable().Subscribe(UpdateRemainTimeLayerOpen).AddTo(this);

                        if (sectionTimeReductionPopup != null)
                        {
                            sectionTimeReductionPopup.RefreshItemList();
                        }
                    }

                    //! 건물 증축(광고)
                    LocalPushManager.Instance.SetLayerExpansion();
                }
            })
            .SelectMany (res => UIManager.Try_Unlock ().Select (_ => res))
            .Do (_ => NotifiLayer.Active = true)
            .Do (res =>
            {
                var mapInfo = ManagementServerDataManager.Instance.GetManagementMapData (worldMgr.GetTopFloor ());
                if (mapInfo != null)
                {
                    if (mapInfo.remain_mts <= 0)
                    {
                        string msgcomplete = string.Format (CResourceManager.Instance.GetString (92280015), res.managementMapInfo.layer + 1);
                        Toast.MakeTextForContent (msgcomplete, Msg.MSG_TYPE.Notice);
                    }
                }
            })
            .Do(_ => ShopManager.Instance.RequireShopCheck())
            .Subscribe()
            .AddTo(this);
    }

    //시간 단축 아이템 사용
    private void OnClickReduceTimeCallback(Management_TimeReduction_Type type)
    {
        long item_gdid = CItemInvenManager.Instance.GetItemIDByTimeReductionType(type);
        if (item_gdid == -1)
        {
            //SoundManager.Instance.PlayEffect(6810039); // management sound : 증축 시간단축 아이템 없음 (se_ui_023)
            Toast.MakeTextForContent(CResourceManager.Instance.GetString(9219008), Msg.MSG_TYPE.Notice);
            CDebug.LogError("RequestJobUseItem() item_gdid is -1. type = " + type);
            return;
        }

        int itemCount = CItemInvenManager.Instance.GetItemCount(item_gdid);
        if (itemCount <= 0)
        {
            //SoundManager.Instance.PlayEffect(6810039); // management sound : 증축 시간단축 아이템 없음 (se_ui_023)
            CDebug.Log("아이템이 0개입니다 = " + type.ToString());
            return;
        }

        //SoundManager.Instance.PlayEffect(6810018); // management sound : 증축 시간단축 아이템 사용 (se_ui_002)
        Observable.ReturnUnit()
            .Do (_ => NotifiLayer.Active = false)
            .SelectMany(_=> APIHelper.ManagementSvc.Management_Layer_Use_Item (worldMgr.GetTopFloor (), item_gdid))
            .Do (res =>
            {
                ManagementServerDataManager.Instance.UpdateLayerData (res.managementMapInfo);
                ManagementServerDataManager.Instance.UpdateWorkerInfo (res.managementWorkerInfo);
                var mapInfo = ManagementServerDataManager.Instance.GetManagementMapData (worldMgr.GetTopFloor ());
                if (mapInfo != null)
                {
                    if (mapInfo.remain_mts <= 0)
                    {
                        //layerStateMachine.ChangeState(ManagementLayerState_OpenTimeComplete.Instance());
                        layerStateMachine.ChangeState (ManagementLayerState.Instance ());
                        ManagementPageUI pageUI = ((ManagementCanvasManager)ManagementManager.Instance.GetCanvasManager ()).PageUI;
                        pageUI.roomUI.SetHasBeenStartBuilding (false);
                        pageUI.roomUI.SetActiveBuildState (true);

                        RemoveTimeStream ();
                        ReleaseTimer ();
                        ConstructionColliderOff ();
                        RemoveConstructionScreenObject ();
                        timerDisposerSectionOpen.Dispose ();

                        if (sectionTimeReductionPopup != null)
                        {
                            sectionTimeReductionPopup.ClosePopup ();
                        }
                    }
                    else
                    {
                        TimeStream timeStream;
                        string streamKey = CDefines.MANAGEMENT_LAYEROPEN_TIMESTREAM_KEY;
                        ManagementMapData layerSvrInfo = GetLayerServerInfo ();
                        double remainTimeMillieSeconds = layerSvrInfo.remain_mts;
                        double totalSecond = TimeSpan.FromMilliseconds (remainTimeMillieSeconds).TotalSeconds;

                        GlobalTimer.Instance.GetTimeStream (out timeStream, streamKey);
                        timeStream.SetTime (streamKey, (float)totalSecond, 0, TimeStreamType.DECREASE).OnTimeStreamObservable ().Subscribe ();
                        if (timerDisposerSectionOpen != null)
                        {
                            timerDisposerSectionOpen.Dispose ();
                        }
                        timerDisposerSectionOpen = new SingleAssignmentDisposable ();
                        timerDisposerSectionOpen.Disposable = timeStream.OnTimeStreamObservable ().Subscribe (UpdateRemainTimeLayerOpen).AddTo (this);

                        if (sectionTimeReductionPopup != null)
                        {
                            sectionTimeReductionPopup.RefreshItemList ();
                        }
                    }

                    //! 건물 증축(가속)
                    LocalPushManager.Instance.SetLayerExpansion();
                }
            })
            .SelectMany (res => UIManager.Try_Unlock ().Select (_ => res))
            .Do (_ => NotifiLayer.Active = true)
            .Do (res =>
            {
                var mapInfo = ManagementServerDataManager.Instance.GetManagementMapData (worldMgr.GetTopFloor ());
                if (mapInfo != null)
                {
                    if (mapInfo.remain_mts <= 0)
                    {
                        string msgcomplete = string.Format (CResourceManager.Instance.GetString (92280015), res.managementMapInfo.layer + 1);
                        Toast.MakeTextForContent (msgcomplete, Msg.MSG_TYPE.Notice);
                    }
                }
            })
            .Do(_ => ShopManager.Instance.RequireShopCheck())
            .Subscribe ()
            .AddTo(this);
    }

    private void RemoveTimeStream()
    {
        string streamKey = CDefines.MANAGEMENT_LAYEROPEN_TIMESTREAM_KEY;
        if (GlobalTimer.Instance.HasTimeStream(streamKey))
        {
            TimeStream timeStream;
            GlobalTimer.Instance.GetTimeStream(out timeStream, streamKey);
            if (timeStream != null)
            {
                timeStream.Remove();
            }
        }
    }

    public void RemoveConstructionScreenObject()
    {
        worldMgr.ConstructionFloorObj.SetActive(false);
		var ConstructionButtonByRay = worldMgr.ConstructionFloorObj.GetComponent<ButtonByRay>();
		ConstructionButtonByRay.Dispose();
        //DestroyImmediate(ConstructionScreenObj);
    }

    public ManagementMapData GetLayerServerInfo()
    {
        if (ManagementServerDataManager.Instance.newLayerInfo != null)
        {
            long layerIdx = ManagementServerDataManager.Instance.newLayerInfo.idx;

            return ManagementServerDataManager.Instance.GetLayerServerData(layerIdx);
        }

        return null;
    }


}
