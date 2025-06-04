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
# endif

#endregion

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System.Linq;
using DG.Tweening;

public class MGNunchiMapManager
{
    private MGNunchiWorldManager WorldMgr;
    public MGNunchiMapInfo[][] MapInfos;

    //temorary map data
    public MGNunchiMapData[][] MapData;

    public GameObject MapRootObj;

    public GameObject OriginDustFXObj;
    public GameObject OriginHitFXObj;

    private Dictionary<long, MGNunchiMap> SelectedMapIndexPool;

    //private Dictionary<int, MGNunchiPlatformController> PlatformCntlrDic;

    private SingleAssignmentDisposable MessageBrokerDispose = new SingleAssignmentDisposable();

    public bool IsMapObjectClick { get; set; } = false;

    int MapSize = MGNunchiDefines.MAP_SIZE_4P_WIDTH * MGNunchiDefines.MAP_SIZE_4P_HEIGHT;

    public Dictionary<PLAT_STATE, MGNunchiShaderProp> ShaderPropDic = new Dictionary<PLAT_STATE, MGNunchiShaderProp>();
    

    public void Initialize(GameObject mapRootObj)
    {
        WorldMgr = MGNunchiManager.Instance.WorldManager;
        MapRootObj = mapRootObj;
        
        InitShaderPropDic();

        SelectedMapIndexPool = new Dictionary<long, MGNunchiMap>();
        //PlatformCntlrDic = new Dictionary<int, MGNunchiPlatformController>();

        MapData = new MGNunchiMapData[MGNunchiDefines.MAP_SIZE_4P_WIDTH][];
        MapInfos = new MGNunchiMapInfo[MGNunchiDefines.MAP_SIZE_4P_WIDTH][];
        for (int i = 0; i < MapData.Length; ++i)
        {
            MapData[i] = new MGNunchiMapData[MGNunchiDefines.MAP_SIZE_4P_HEIGHT];
            MapInfos[i] = new MGNunchiMapInfo[MGNunchiDefines.MAP_SIZE_4P_HEIGHT];
        }

        //temporary
        SetMapData();

        SetMapInfo();
        IsMapObjectClick = false;
        SetMapClickEvent();

        //MakeGameCoinOnEachMap();

        prevPickIdx = -1;

    }

    int prevPickIdx;
    
    public void SetMapClickEvent()
    {
        if (WorldMgr.GetCurrentState() == MGNunchiGameState_Tutorial.Instance())
        {
            return;
        }
        MessageBrokerDispose.Disposable =
            MessageBroker.Default.Receive<PickObject>()
            .Where(click => IsMapObjectClick == true)
            .Subscribe(pickInfo =>
            {
                //푸딩 위에 캐릭터가 있는지 체크
                bool isCharacterObject = false;
                WorldMgr.PlayerMgr.GetPlayerPositionIndex().ForEach((idx, posidx) =>
                {
                    if (posidx == pickInfo.Idx)
                    {
                        isCharacterObject = true;
                        return;
                    }
                });

                if (isCharacterObject) return;

                //애니메이션 출력
                MGNunchiMap map = ConvertIndexToMap(pickInfo.Idx);
                //CDebug.Log($"        #### call PlayJumpDownAnimByInfo() idx = {pickInfo.Idx}");
                PlayJumpDownAnimByInfo(map);

                //위에 아이템이 있는지 체크 한다
                if (pickInfo.IsLongPress)
                {
                    var itemType = WorldMgr.CoinMgr.GetItemType(pickInfo.Idx);
                    if (itemType != BPWPacketDefine.NunchiGameItemType.COIN)
                    {
                        WorldMgr.GetPageUI().ShowToolTip(itemType);
                        return;
                    }
                }
                OnClickSelectMap( pickInfo.Idx);
            });
    }

    //map index
    //(0) (1) (2)
    //(3) (4) (5)
    //(6) (7) (8)
    public void OnClickSelectMap(int mapIdx)
    {
        if (MGNunchiServerDataManager.Instance.IsSinglePlay)
        {
            if (mapIdx != 4)
            {
                return;
            }
        }


        CDebug.Log("        *********** OnClickSelectMap mapIDx = " + mapIdx);// +"/"+ MGNunchiServerDataManager.Instance.IsSinglePlay);

        //아이템이 없고 캐릭터가 갈 수 있는 오브젝트이면 선택한 상태로 놓는다
        var MyPlayer = WorldMgr.PlayerMgr.GetMyPlayer();

        bool CheckSelected = false;
        MyPlayer.PlayerInfo.MapIdxByDir
        .Where(mapid => MapSize > mapid && mapid >= 0)
        .ForEach((idx, mapid) =>
        {
            if (mapid == mapIdx)
            {
                CheckSelected = true;
            }
        });

        if (CheckSelected)
        {
            MyPlayer.PlayerInfo.MapIdxByDir
            .Where(mapid => MapSize > mapid && mapid >= 0)
            .ForEach((idx, mapid) =>
            {
                MGNunchiMap map = ConvertIndexToMap(mapid);
                if (MyPlayer.CheckMovableByMapInfo(map))
                {
                    if (mapIdx == mapid)
                    {
                        CDebug.Log($"    $$$$$ SetEffectActivate() SetMapEffect [Touched] / mapid = {mapid}");
                        MapInfos[map.X][map.Y].SetMapEffectTouched();

                        //서버에는 선택한 오브젝트가 바뀌었을 때만  보낸다(Req_RoundItemSeatChoice)
                        bool IsChange = MyPlayer.SetMapIndex(mapIdx);
                        if (IsChange)
                        {
                            SoundManager.Instance.PlayEffect( 6830013 ); //list 112
                            MyPlayer.SetTargetAngle();
                            WorldMgr.MapMgr.AddSelectMapIndex(MyPlayer.PlayerInfo.UID, MyPlayer.GetMapIndexByDirectioin());
                        }
                    }
                    else
                    {
                        if (MapInfos[map.X][map.Y].GetCurrentState() != MGNunchiMapEffectState_Selectable.Instance())
                        {
                            CDebug.Log($"    $$$$$ SetEffectActivate() SetMapEffect [Selectable] / mapid = {mapid}");
                            MapInfos[map.X][map.Y].SetMapEffectSelectable();
                        }
                    }
                }
            });
        }
    }


    private void InitShaderPropDic()
    {        
        Vector4 _albedo = new Vector4(1, 1, 1, 0);
        Vector4 _emission = new Vector4(0, 0, 0, 0);
        ShaderPropDic.Add(PLAT_STATE.DEFAULT, new MGNunchiShaderProp(_albedo, _emission));

        _albedo = new Vector4(1, 0.153f, 0, 1);
        ShaderPropDic.Add(PLAT_STATE.TOUCHED, new MGNunchiShaderProp(_albedo, _albedo));
        ShaderPropDic.Add(PLAT_STATE.SELECTED, new MGNunchiShaderProp(_albedo, _albedo));

        _albedo = new Vector4(1, 0.876f, 0, 0);
        _emission = new Vector4(1, 0.706f, 0, 1);
        ShaderPropDic.Add(PLAT_STATE.SELECTABLE, new MGNunchiShaderProp(_albedo, _emission));
    }

    public MGNunchiShaderProp GetShaderPropByState(PLAT_STATE state)
    {
        if(ShaderPropDic.ContainsKey(state))
        {
            return ShaderPropDic[state];
        }

        return null;
    }

    //temporary
    // map datas are going to set from table data.
    private void SetMapData()
    {
        for (int i = 0; i < MGNunchiDefines.MAP_SIZE_4P_WIDTH; ++i)
        {
            for (int j = 0; j < MGNunchiDefines.MAP_SIZE_4P_HEIGHT; ++j)
            {
                //Coin, Item place
                if( i == 0 && j == 0 || i == 0 && j == 2 || 
                            i == 1 && j == 1 ||
                    i == 2 && j == 0 || i == 2 && j == 2)
                {
                    MapData[i][j] = new MGNunchiMapData(MGNunchiDefines.COIN_ITEM_PLACE_LAYER);
                }
                else
                {
                    //Player place
                    MapData[i][j] = new MGNunchiMapData(MGNunchiDefines.PLAYERS_PLACE_LAYER);
                }
            }
        }
    }

    private void SetMapInfo()
    {
        //FX
        OriginDustFXObj = WorldMgr.GetResourceObj(MGNunchiConstants.COINITEM_FX_DUST_PATH);
        OriginHitFXObj = WorldMgr.GetResourceObj(MGNunchiConstants.COINITEM_FX_HIT_PATH);

        for (int x = 0; x < MGNunchiDefines.MAP_SIZE_4P_WIDTH; ++x)
        {
            for (int y = 0; y < MGNunchiDefines.MAP_SIZE_4P_HEIGHT; ++y)
            {
                int layerIdx = MapData[x][y].LayerIndex;
                CDebug.Log($"MapData[{x}][{y}].LayerIndex = {MapData[x][y].LayerIndex}");

                //temporary
                Transform _mapObj = MapRootObj.transform.Find(MGNunchiDefines.PLAT_NAME + "_" + GetMapIndexByXY(x, y));

                MapInfos[x][y] = new MGNunchiMapInfo();
                MapInfos[x][y].Initialize(WorldMgr, layerIdx, _mapObj);

                Transform _animObj = _mapObj.Find(MGNunchiDefines.PLAT_NAME);
                if (_animObj == null)
                {
                    Debug.LogError($"Animatio Object is null in {_mapObj}");
                }

                if (_animObj != null)
                {
                    Animation _anim = _animObj.GetComponent<Animation>();
                    MapInfos[x][y].InitPlatformAnim(_anim);
                }
                //Debug.Log("SetMapInfo() [" + x + ", " + y + "].layerIdx = " + layerIdx + "// map obj = " + _mapObj);
            }
        }
    }

    public int GetMapIndexByXY(int x, int y)
    {
        return (x * MGNunchiDefines.MAP_SIZE_4P_WIDTH + y);
    }

    public MGNunchiMapInfo GetMapInfoByXY(int x, int y)
    {
        return MapInfos[x][y];
    }

    public void SetCoinItemOnMap()
    {
        MGNunchiCoinItemInfo[] _coinItemInfo = MGNunchiServerDataManager.Instance.GetCoinItemInfos();

        //Server Seat Index Order
        // [0]   [1]
        //    [4]
        // [2]   [3]
        for (int i=0; i<_coinItemInfo.Length; ++i)
        {
            MakeGameCoinOnEachMap(_coinItemInfo[i].ItemType, _coinItemInfo[i].Quantity, i);
        }




        //for (int i=0; i< WorldMgr.CoinItemSeatMapIdx.Length; ++i)
        //{
        //    if (WorldMgr.CoinItemSeatMapIdx[i] == MGNunchiConstants.MAP_CENTER_IDX)
        //    {
        //        int _count = 1;
        //        MGNUNCHI_ITEM_TYPE randType = WorldMgr.CoinMgr.MakeGameItemTypeRandomly();
        //        if(randType == MGNUNCHI_ITEM_TYPE.COIN)
        //        {
        //            _count = WorldMgr.CoinMgr.MakeGameCoinCountRandomly();
        //        }
        //        MakeGameCoinOnEachMap(randType, _count, i);
        //    }
        //    else
        //    {
        //        int _count = WorldMgr.CoinMgr.MakeGameCoinCountRandomly();
        //        MakeGameCoinOnEachMap(MGNUNCHI_ITEM_TYPE.COIN, _count, i);
        //    }
        //}
    }

    public void MakeGameCoinOnEachMap(BPWPacketDefine.NunchiGameItemType type, int count, int svrSeatIdx)
    {
        int _mapIdx = WorldMgr.CoinItemSeatMapIdx[svrSeatIdx];
        MGNunchiMap map = ConvertIndexToMap(_mapIdx);
        WorldMgr.CoinMgr.MakeGameCoinObject(type, count, svrSeatIdx, MapInfos[map.X][map.Y].MapObj);
    }

    public MGNunchiMap ConvertIndexToMap(int idx)
    {
        return new MGNunchiMap() { X = idx / MGNunchiDefines.MAP_SIZE_4P_WIDTH, Y = idx % MGNunchiDefines.MAP_SIZE_4P_HEIGHT };
    }

    private bool CheckMapIndex(int x, int y)
    {
        if (x < 0 || x >= MapInfos.Length || y < 0 || y >= MapInfos[x].Length)
            return false;
        return true;
    }

    public Vector3 GetMapPosition(int x, int y)
    {
        if (CheckMapIndex(x, y) == false)
            return Vector3.zero;            
                
        return MapInfos[x][y].GetMapPosition();
    }

    public bool IsSameWithPlayerSuccessMapIdx(int idx)
    {
        List<int> _playerSuccessMapIdxList = WorldMgr.PlayerMgr.GetSuccessMapIndexList();

        for(int i=0;i < _playerSuccessMapIdxList.Count; ++i)
        {
            if(_playerSuccessMapIdxList[i] == idx)
            {
                return true;
            }
        }

        return false;
    }

    public List<int> GetRemainedCoinMapIndexList()
    {
        List<int> _mapIdxList = new List<int>();

        if (WorldMgr.PLAYER_COUNT == MGNunchiDefines.PLAYERS_4P)
        {
            for(int i=0; i<WorldMgr.CoinItemSeatMapIdx.Length; ++i)
            {
                _mapIdxList.Add(WorldMgr.CoinItemSeatMapIdx[i]);
            }

            for(int i=(_mapIdxList.Count - 1); i>=0; --i)
            {
                int _mapIdx = _mapIdxList[i];
                if (IsSameWithPlayerSuccessMapIdx(_mapIdx))
                {
                    _mapIdxList.Remove(_mapIdx);
                }
            }
        }

        return _mapIdxList;
    }

    public bool IsMovableMapByIndex(MGNunchiMap map)
    {
        int x = map.X;
        int y = map.Y;

        if (CheckMapIndex(x, y) == false)
            return false;

        if (MapInfos[x][y].Prop_Layer == MGNunchiDefines.COIN_ITEM_PLACE_LAYER)
        {
            //CDebug.Log($"    $$$$$$$$    IsMovableMapByIndex() x:{x}, y:{y}, propLayer = {MapInfos[x][y].Prop_Layer}");
            return true;
        }

        return false;
    }

    public void AddSelectMapIndex(long uid, MGNunchiMap map)
    {
        //CDebug.Log("AddSelectMapIndex() uid = " + uid + " / x = " + map.x + ", y = " + map.y);

        if (SelectedMapIndexPool.ContainsKey(uid))
        {
            SelectedMapIndexPool[uid] = map;
        }
        else
        {
            SelectedMapIndexPool.Add(uid, map);
        }
    }

    public bool CompareSameMapIndex(long uid, MGNunchiMap map)
    {
        foreach(KeyValuePair<long, MGNunchiMap> item in SelectedMapIndexPool)
        {
            if(item.Key != uid)
            {
                if(item.Value.X == map.X && item.Value.Y == map.Y)
                {
                    return true;
                }
            }
        }

        return false;
    }

    public int GetGaimCoinCurrentOnEachMap(MGNunchiMap map)
    {
        if (CheckMapIndex(map.X, map.Y) == false)
            return -1;

        return MapInfos[map.X][map.Y].GetGameCoinCurrentCount();
    }

    public void PlayJumpDownAnimByInfo(MGNunchiMap map)
    {
        MapInfos[map.X][map.Y].PlayJumpDownAnim();
    }

    //public void SetMapEffectTouched(MGNunchiMap map)
    //{
    //    MapInfos[map.X][map.Y].SetMapEffectTouched();
    //}

    public void SetAllmapEffectNormal()
    {
        for(int i=0; i< MapInfos.Length; ++i)
        {
            for(int j=0; j < MapInfos[i].Length; ++j)
            {
                MapInfos[i][j].SetMapEffectFadeOut();
            }
        }
    }

    public void SetEffectActivate()
    {
        var MyPlayer = WorldMgr.PlayerMgr.GetMyPlayer();
        float Length = MGNunchiDefines.MAP_SIZE_4P_WIDTH * MGNunchiDefines.MAP_SIZE_4P_HEIGHT;

        MyPlayer.PlayerInfo.IsMovable.ForEach((idx, isMoveble) =>
        {
            int mapIdx = MyPlayer.PlayerInfo.MapIdxByDir[idx];
            //Debug.Log($"SetEffectActivate idx:{idx}, dir : {mapIdx}");

            if(mapIdx >= 0 && mapIdx < Length)
            {
                MGNunchiMap map = ConvertIndexToMap(mapIdx);
                if (isMoveble)
                {
                    //CDebug.Log($"    ### SetEffectActivate() Selectable mapIdx = {mapIdx}");
                    MapInfos[map.X][map.Y].SetMapEffectSelectable();
                }
                else
                {
                    //CDebug.Log($"    ### SetEffectActivate() Normal mapIdx = {mapIdx}, map.X:{map.X}, map.Y:{map.Y}, MapInfos.length = {MapInfos.Length}");
                    MapInfos[map.X][map.Y].SetMapEffectNormal();
                }
            }
        });
    }

    //public void SetEffectAllActivate(bool isActive)
    //{
    //    MapInfos.ForEach((idxX, infoX )=>
    //    {
    //        infoX.ForEach((idxY, InfoY) =>
    //        {
    //            InfoY.MapEffect.SetEffectActive(isActive);
    //        });
    //    });
    //}

    public void Release()
    {
        SetAllmapEffectNormal();
        if (WorldMgr != null) WorldMgr = null;

        if(MapData != null)
        {
            for (int i = 0; i < MapData.Length; ++i)
            {
                MapData[i] = null;
            }
            MapData = null;
        }
        if (MapInfos != null)
        {
            for (int i = 0; i < MapInfos.Length; ++i)
            {
                MapInfos[i] = null;
            }
            MapInfos = null;
        }
        if(SelectedMapIndexPool != null)
        {
            SelectedMapIndexPool.Clear();
            SelectedMapIndexPool = null;
        }
        //if(PlatformCntlrDic != null)
        //{
        //    PlatformCntlrDic.Clear();
        //    PlatformCntlrDic = null;
        //}

        if (MessageBrokerDispose != null)
        {
            MessageBrokerDispose.Dispose();
        }
    }

}

public class MGNunchiMap
{
    public int X;
    public int Y;
}

public class MGNunchiShaderProp
{
    public Vector4 Albedo;
    public Vector4 Emission;

    public MGNunchiShaderProp(){ }
    public MGNunchiShaderProp(Vector4 albedo, Vector4 emission)
    {
        Albedo = albedo;
        Emission = emission;
    }
}
