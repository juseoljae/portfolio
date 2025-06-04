using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BPWorldMainCamFollower : MonoBehaviour
{
    private Transform MyTransform;
    private Transform BpwMainCamObj;
    public GameObject FarLODObj;
    public BPWorldLODChecker FarLODChecker;
    private BPWorldNetPlayersManager netPlayerManager;


    public void Init(Transform camObj)
    {
        MyTransform = transform;
        BpwMainCamObj = camObj;
        SetObject();
    }

    public void SetNetPlayerManager( BPWorldNetPlayersManager netPlayerManager )
    {
        this.netPlayerManager = netPlayerManager;
        if(FarLODChecker!= null )
        {
            FarLODChecker.SetNetPlayerManager( netPlayerManager );
        }
    }


    private void SetObject()
    {
        GameObject farObj = new GameObject( "DBLODChecker_Far" );
        FarLODObj = Utility.AddChild( gameObject, farObj );
        FarLODChecker = FarLODObj.AddComponent<BPWorldLODChecker>();
        FarLODChecker.Init( BPWorldLODChecker.DISTANCE_TYPE.FAR );

        SetMyPosition();
    }

    // Update is called once per frame
    void Update()
    {
        SetMyPosition();
    }

    private void SetMyPosition()
    {
        if (MyTransform == null || BpwMainCamObj == null)
        {
            return;
        }

        Vector3 pos = BpwMainCamObj.position;
        pos.y = 0;
        MyTransform.position = pos;
    }
}
