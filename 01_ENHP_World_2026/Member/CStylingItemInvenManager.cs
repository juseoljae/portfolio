using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Game.RestAPI;
using System;
using UniRx;
using Unity.VisualScripting;

public class CStylingItemInvenManager : SingletonMono<CStylingItemInvenManager>
{
    public Dictionary<MEMBER_TYPE, List<StylingList>> StylingItemInvenDict = new Dictionary<MEMBER_TYPE, List<StylingList>>();//purchased also

    private Dictionary<int, Dictionary<MEMBER_TYPE, List<StylingList>>> StyleListDicByTab = null;

    private CompositeDisposable AvatarApiDisposables = new CompositeDisposable();
    public void Init()
    {
    }

    public void SetStylingItems(List<StylingList> info)
    {
        var _dic = new Dictionary<MEMBER_TYPE, List<StylingList>>();

        foreach (var itemInfo in info)
        {
            StylingItemData _data = CMemberAvatarDataManager.Instance.GetStylingItemData(itemInfo.style_id);
            if (_data != null)
            {
                MEMBER_TYPE mType = _data.MemberType;

                if (!_dic.TryGetValue(mType, out var outValues))
                {
                    outValues = new List<StylingList>();
                    _dic[mType] = outValues;
                }

                if (!outValues.Exists(x => x.style_id == itemInfo.style_id))
                {
                    outValues.Add(itemInfo);
                }
            }

        }

        foreach (KeyValuePair<MEMBER_TYPE, List<StylingList>> pair in _dic)
        {
            MEMBER_TYPE type = pair.Key;
            List<StylingList> itemInfos = pair.Value;

            if (StylingItemInvenDict.TryGetValue(type, out var existingList))
            {
                existingList.Clear();
                existingList.AddRange(itemInfos);
            }
            else
            {
                StylingItemInvenDict[type] = itemInfos;
            }
        }

    }

    public void SetStylingItemsByTab(int tabIndex, MEMBER_TYPE type, List<StylingList> info)
    {
        if (StyleListDicByTab == null)
        {
            StyleListDicByTab = new Dictionary<int, Dictionary<MEMBER_TYPE, List<StylingList>>>();
        }

        if (!StyleListDicByTab.ContainsKey(tabIndex))
        {
            StyleListDicByTab.Add(tabIndex, new Dictionary<MEMBER_TYPE, List<StylingList>>());
        }

        if (!StyleListDicByTab[tabIndex].ContainsKey(type))
        {
            StyleListDicByTab[tabIndex].Add(type, new List<StylingList>());
        }
        UpdateStyleListDicByTab(tabIndex, type, info);    }

    public void UpdateStyleListDicByTab(int tabIndex, MEMBER_TYPE type, List<StylingList> infos)
    {
        if (StyleListDicByTab.ContainsKey(tabIndex) == false)
        {
            return;
        }

        if (StyleListDicByTab[tabIndex].ContainsKey(type) == false)
        {
            SetStylingItemsByTab(tabIndex, type, infos);
        }

        var list = StyleListDicByTab[tabIndex][type];

        foreach (var info in infos)
        {
            var existing = list.FirstOrDefault(x => x.style_id == info.style_id);
            if (existing != null)
            {
                existing.having   = info.having;
                existing.puton    = info.puton;
                existing.sort     = info.sort;
                existing.new_flag = info.new_flag;
                existing.goods_id = info.goods_id;
            }
            else
            {
                list.Add(info);
            }
        }
    }

    public List<StylingList> GetOwnedStylingItems(MEMBER_TYPE type)
    {
        if (StylingItemInvenDict.ContainsKey(type))
        {
            return StylingItemInvenDict[type]
                .Where(item => item.having == 1)
                .ToList();
        }

        return null;
    }

    public StylingList GetStylingListByID(int id)
    {
        return StylingItemInvenDict.Values
            .SelectMany(list => list)
            .FirstOrDefault(item => item.style_id == id);
    }
    
    public List<StylingList> GetStyleItemListByTab(int tabIndex, MEMBER_TYPE type)
    {
        if (StyleListDicByTab != null)
        {
            if (StyleListDicByTab.ContainsKey(tabIndex) && StyleListDicByTab[tabIndex].ContainsKey(type))
            {
                return StyleListDicByTab[tabIndex][type];
            }

        }

        return null;
    }

    public StylingList GetStyleItemByID(int tabIndex, MEMBER_TYPE type, int styleId)
    {
        if (StyleListDicByTab != null && StyleListDicByTab.ContainsKey(tabIndex) && StyleListDicByTab[tabIndex].ContainsKey(type))
        {
            return StyleListDicByTab[tabIndex][type].FirstOrDefault(item => item.style_id == styleId);
        }

        return null;
    }

    public void UpdateStyleItemListByTab(int tabIndex, MEMBER_TYPE type, List<StylingList> itemInfos)
    {
        foreach (var itemInfo in itemInfos)
        {
            UpdateStyleItemListByTab(tabIndex, type, itemInfo);
        }
    }

    public void UpdateStyleItemListByTab(int tabIndex, MEMBER_TYPE type, StylingList itemInfo)
    {
        if (StyleListDicByTab != null)
        {
            if (StyleListDicByTab.ContainsKey(tabIndex) && StyleListDicByTab[tabIndex].ContainsKey(type))
            {
                foreach (var item in StyleListDicByTab[tabIndex][type])
                {
                    if (item.style_id == itemInfo.style_id)
                    {
                        item.having = itemInfo.having;
                        item.puton = itemInfo.puton;
                        item.sort = itemInfo.sort;
                        item.new_flag = itemInfo.new_flag;
                        item.goods_id = itemInfo.goods_id;
                        return;
                    }
                }
            }

        }

    }
    
    public void ClearStyleListDicByTab(int tabIndex, MEMBER_TYPE memberType)
    {
        if (StyleListDicByTab != null && StyleListDicByTab.ContainsKey(tabIndex) && StyleListDicByTab[tabIndex].ContainsKey(memberType))
        {
            StyleListDicByTab[tabIndex][memberType].Clear();
        }
    }

    public void SetStylingInfoByTab(int tabIndex, MEMBER_TYPE memberType, Action<MEMBER_TYPE> recvEvent)
    {
        List<StylingList> styleList = CStylingItemInvenManager.Instance.GetStyleItemListByTab(tabIndex, memberType);
        //Debug.Log($"@@moo SetStylingInfoByTab tabIndex : {tabIndex}, memberType : {memberType}, styleList Count : {(styleList != null ? styleList.Count : 0)}");
        if (styleList != null && styleList.Count > 0)
        {
            if (recvEvent != null)
            {
                recvEvent.Invoke(memberType);
            }
        }
        else
        {
            APIHelper.SNGService.ReqSNG_Avatar(((int)memberType).ToString(), tabIndex)
                .Subscribe(result =>
                {
                    // 로드된 데이터 재확인 후 진행
                    List<StylingList> loadedList = CStylingItemInvenManager.Instance.GetStyleItemListByTab(tabIndex, memberType);
                    if (loadedList != null)
                    {
                        if (recvEvent != null)
                        {
                            recvEvent.Invoke(memberType);
                        }
                    }
                }).AddTo(AvatarApiDisposables);
        }
    }

    public void AddStylingItemInfo(RewardList sItem)
    {
        REWARD_TYPE rw = sItem.type.ToEnum<REWARD_TYPE>();
        if (rw != REWARD_TYPE.RW_STYLING) return;

        StylingItemData sItemData = CMemberAvatarDataManager.Instance.GetStylingItemData(sItem.sub);
        if (sItemData == null)
        {
            CDebug.LogError($"CStylingItemInvenManager.AddStylingItemInfo() Not found Style Item Data {sItem.sub}");
            return;
        }
        
        var itemInfo = new StylingList
        {
            style_id = sItemData.ID,
            sort = sItemData.Sort,
            having = 1,   
            puton = 0,    
            new_flag = 1, 
            goods_id = 0
        };

        var stylingItemList = new List<StylingList> { itemInfo };

        SetStylingItems(stylingItemList);
    }




    public void DisposeAvatarApi()
    {
        if (AvatarApiDisposables != null)
        {
            AvatarApiDisposables.Clear();
        }
    }

    public void ReleaseStyleListByTab()
    {
        if (StyleListDicByTab != null)
        {
            StyleListDicByTab.Clear();
            StyleListDicByTab = null;
        }
    }
}



