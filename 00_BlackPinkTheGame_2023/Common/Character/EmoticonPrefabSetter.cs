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

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

#if UNITY_EDITOR
using UnityEngine.SceneManagement;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.U2D;
#endif

public class EmoticonPrefabSetter : MonoBehaviour
{
    private Image emoticonImg;
    private string emoticonPath;

#if UNITY_EDITOR
    private AsyncOperationHandle<Sprite> sOperation;
    public AssetReferenceSprite assetSprite;
#endif


    public void SetEmoticon(string emojiPath)
    {
        emoticonImg = GameObjectHelperUtils.FindComponent<Image>(transform, "Canvas/Emoji/Image");

        emoticonPath = emojiPath;

#if UNITY_EDITOR
        if (SceneManager.GetActiveScene().name.Equals("NPC_TEST"))
        {
            sOperation = Addressables.LoadAssetAsync<Sprite>(emoticonPath);
            sOperation.Completed += SpriteLoaded;
        }
        else
#endif
        {
            CResourceData resData = CResourceManager.Instance.GetResourceData(emoticonPath);
            emoticonImg.sprite = resData.LoadSprite(this);
        }
    }

#if UNITY_EDITOR
    private void SpriteLoaded(AsyncOperationHandle<Sprite> obj)
    {
        switch (obj.Status)
        {
            case AsyncOperationStatus.Succeeded:
                emoticonImg.sprite = obj.Result;
                CDebug.Log("Sprite load Succeeded.!!!");
                break;
            case AsyncOperationStatus.Failed:
                CDebug.LogError("Sprite load failed.");
                break;
            default:
                // case AsyncOperationStatus.None:
                break;
        }
    }

    private void OnDestroy()
    {
        if(sOperation.IsValid())
        {
            Addressables.Release(sOperation);
        }
    }
#endif

}
