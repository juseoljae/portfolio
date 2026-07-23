using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.RestAPI;
using UnityEngine.UI;

public class ObjMarbleMember : MonoBehaviour
{
    private PopupMarbleMemberSelect PopupMemberSelect;
    private MarbleMemberInfo MemberInfo;

    [SerializeField] private GameObject SelectObj;
    [SerializeField] private Image MemberIcon;    
    [SerializeField] private Text MemberName;
    [SerializeField] private Text MemberLevel;
    [SerializeField] private Text SkillPoint;
    [SerializeField] private Image SkillPointIcon;
    [SerializeField] private GameObject PlayingMark;


    public void SetData(PopupMarbleMemberSelect parent, MarbleMemberInfo info)
    {
        PopupMemberSelect = parent;
        MemberInfo = info;

        MemberListData data = CMemberDataManager.Instance.GetMemberListData(info.MemberType);
        if (data == null)
        {
            CDebug.LogError($"ObjMarbleMember SetData Fail! No Member Data. MemberType : {info.MemberType}");
            return;
        }
        
        CResourceManager.Instance.LoadImage(data.vamKidzThumbnailResPath, MemberIcon);
        
        MemberName.text = SNGDataManager.Instance.GetMemberName(info.MemberType);
        MemberLevel.text = $"Lv.{info.Level}";
        //SkillPoint.text = info.ExpPoint.ToString();

        ItemData itemData = CItemDataManager.Instance.GetItemData(data.diceGameExpItemID);
        if (itemData != null)
        {
            CResourceManager.Instance.LoadImage(itemData.item_resource_image, SkillPointIcon);
        }

        ItemList itemInfo = ItemInventoryManager.Instance.GetItemData(data.diceGameExpItemID);
        if (itemInfo != null)
        {
            SkillPoint.text = itemInfo.count.ToString();
        }
        else
        {
            SkillPoint.text = "0";
        }

        if (info.IsSelected)
        {
            SetActiveSelectObj(true);
        }
        else
        {
            SetActiveSelectObj(false);
        }

        if (info.IsPlaying)
        {
            PlayingMark.SetActive(true);
        }
        else
        {
            PlayingMark.SetActive(false);
        }
    }

    public void SetActiveSelectObj(bool isActive)
    {
        SelectObj.SetActive(isActive);
    }

    public void OnClickSelectMember()
    {
        PopupMemberSelect.SetSelectedMember(MemberInfo.MemberType, true);
    }
}
