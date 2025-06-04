using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


//only for '3dui_avatar_chat'
public class TalkBubbleSetter : MonoBehaviour
{
    public void SetBubble(string message, TALK_DIRECTION dir)
    {
        RectTransform parentObj = GameObjectHelperUtils.FindComponent<RectTransform>( transform, "Canvas/chat/box" );
        RectTransform bubbleObj = GameObjectHelperUtils.FindComponent<RectTransform>( transform, "Canvas/chat/box/frame" );

        
        parentObj.anchoredPosition = new Vector2(0, -150);
        switch (dir)
        {
            case TALK_DIRECTION.LEFT:
                parentObj.pivot = new Vector2( 1, 0.5f );
                bubbleObj.localScale = new Vector3( -1, 1, 1 );
                break;
                
            case TALK_DIRECTION.RIGHT:
                parentObj.pivot = new Vector2( 0, 0.5f );
                bubbleObj.localScale = Vector3.one;
                break;
        }

        Text msgTxt = GameObjectHelperUtils.FindComponent<Text>(transform, "Canvas/chat/box/Text_Chat");
        if (msgTxt != null) 
        {
            msgTxt.text = message;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
