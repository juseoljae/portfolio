#if UNITY_EDITOR
using UnityEngine;
using UniRx;
using UnityEngine.SceneManagement;

public class CMemberToolManager : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void ToolStart()
    {        
        if ( !SceneManager.GetActiveScene().name.Contains("AvatarTool"))
            return;

        //CDataManager.Instance.PreloadDataForTool();        
        var disposable = new SingleAssignmentDisposable();
        disposable.Disposable = Observable.Empty<Unit>().StartWith(Unit.Default)
       .SelectMany(_ =>
       {
           Debug.Log("Preload data");     
           return CDataManager.RxPreload();
       }).Subscribe(_=>
       {         
            GameObject memberRootObj = GameObject.Find("world");

            CMemberControlEdit[] cntlr = memberRootObj.GetComponentsInChildren<CMemberControlEdit>();
            foreach (var c in cntlr)
            {
                c.SetMemberAvatar();
            }
       });
    }
}
#endif