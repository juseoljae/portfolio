using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BPWorldLODChecker : MonoBehaviour
{
    public enum DISTANCE_TYPE
    {
        NONE = 0,
        CLOSE,
        FAR
    }
    public DISTANCE_TYPE DistType;
    private BoxCollider Collider;

    private BPWorldNetPlayer Player;
    private BPWorldNetPlayersManager netPlayerManager;

    private const int CLOSE_SIZE = 6;
    private const int FAR_SIZE = 25;

    public void Init(DISTANCE_TYPE type)
    {
        this.DistType = type;

        Collider = gameObject.AddComponent<BoxCollider>();

        switch (DistType)
        {
            case DISTANCE_TYPE.CLOSE:
                break;
            case DISTANCE_TYPE.FAR:
                Collider.isTrigger = true;
                Collider.center = new Vector3( 0, 0, (FAR_SIZE / 2) );
                Collider.size = new Vector3( FAR_SIZE, 0.4f, FAR_SIZE );
                break;
        }
    }

    public void SetNetPlayerManager(BPWorldNetPlayersManager netPlayerManager)
    {
        this.netPlayerManager = netPlayerManager;
    }

    private void OnCollisionEnter(Collision collision)
    {
        
    }

    //void OnTriggerEnter(Collider other)
    //{
    //    BPWorldNetPlayerInfo playerInfo = other.gameObject.GetComponent<BPWorldNetPlayerInfo>();
    //    if(playerInfo != null)
    //    {
    //        BPWorldNetPlayer netPlayer = this.netPlayerManager.GetNetPlayer( playerInfo.GetUID );
    //        Debug.Log( "OnTriggerEnter UID = " + netPlayer.PlayerInfo.UID );
    //        if( netPlayer != null )
    //        {
    //            switch (DistType)
    //            {
    //                case DISTANCE_TYPE.CLOSE:
    //                    break;
    //                case DISTANCE_TYPE.FAR:
    //                    //turn on db
    //                    AvatarManager.Instance.SetActiveDynamicBone( netPlayer.PlayerInfo.StyleItem, true );
    //                    break;
    //            }
    //        }
    //    }
    //}

    //void OnTriggerExit(Collider other)
    //{
    //    BPWorldNetPlayerInfo playerInfo = other.gameObject.GetComponent<BPWorldNetPlayerInfo>();
    //    if (playerInfo != null)
    //    {
    //        BPWorldNetPlayer netPlayer = this.netPlayerManager.GetNetPlayer( playerInfo.GetUID );
    //        Debug.Log( "OnTriggerExit UID = " + netPlayer.PlayerInfo.UID );
    //        if (netPlayer != null)
    //        {
    //            switch (DistType)
    //            {
    //                case DISTANCE_TYPE.CLOSE:
    //                    break;
    //                case DISTANCE_TYPE.FAR:
    //                    //turn off db
    //                    AvatarManager.Instance.SetActiveDynamicBone( netPlayer.PlayerInfo.StyleItem, false );
    //                    break;
    //            }
    //        }
    //    }
    //}
}
