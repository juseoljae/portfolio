using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ManagementNameTag3D : MonoBehaviour
{
    //private BillBoardUI BboardUI;
    private Text NameTxt;
    private Text LvText;

    public void Init(string name, Transform cam, int lv)
    {
        BillBoardUI BboardUI = gameObject.AddComponent<BillBoardUI>();
        BboardUI.SetCamera(cam);

        Vector3 nameTagPos = transform.localPosition;
        nameTagPos.y -= 0.2f;
        //transform.localPosition = nameTagPos;

        Transform NameObj = transform.Find("Canvas/layout/text_name");
        NameTxt = NameObj.GetComponent<Text>();
        NameTxt.text = name;

        Transform lvObj = transform.Find("Canvas/layout/text_level");
        LvText = lvObj.GetComponent<Text>();
        if (lv > 0)
        {
            var lvStr = CResourceManager.Instance.GetString(91130001);//Lv{0}
            LvText.text = string.Format(lvStr, lv);
            lvObj.gameObject.SetActive(true);
        }
        else
        {
            lvObj.gameObject.SetActive(false);
        }
    }

    public void SetLevel(int lv)
    {
        if (lv > 0)
        {
            var lvStr = CResourceManager.Instance.GetString(91130001);//Lv{0}
            LvText.text = string.Format(lvStr, lv);
            LvText.gameObject.SetActive(true);
        }
        else
        {
            LvText.gameObject.SetActive(false);
        }
    }
}
