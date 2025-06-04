using UnityEngine;
using System.Collections;

public class BallTypeSelect : MonoBehaviour {
	
	int selBTNum;
	bool bFinishSelBT;
	
	GameObject[] btSelObj;
	Transform  ballTypeSelObj;
	Vector3[] btSelBtnPos;
	
	Transform uiCam;
	

	// Use this for initialization
	void Start () {
		uiCam = GlobalVar.playUIObj.FindChild("UI_Cam");//tmpBTSel.parent.parent.parent;
		ballTypeSelObj = uiCam.FindChild("Panel").FindChild("Anchor_TopLeft").FindChild("BallTypeSel");
		
		BallTypeSelect_Load();
				
		BallTypeSelect_initialize();
		BallTypeSelect_initObject();
	}
	
	// Update is called once per frame
	void Update () {}
	
	public void BallTypeSelect_createBallType()
	{
	}
	
	public void BallTypeSelect_Load()
	{
		btSelBtnPos = new Vector3[5];
		btSelBtnPos[0] = new Vector3(162, -134, 0);
		btSelBtnPos[1] = new Vector3(85, -202, 0);
		btSelBtnPos[2] = new Vector3(58, -305, 0);
		btSelBtnPos[3] = new Vector3(68, -410, 0);
		btSelBtnPos[4] = new Vector3(111, -506, 0);
		
		btSelObj = new GameObject[5];
		for( int i=0 ; i<btSelObj.Length ; i++ )
		{
			btSelObj[i] = (GameObject)Instantiate(Resources.Load("Prefab/S_GameProc/BallType/BT_"+GlobalVar.myTeam.P_starting.ballType[i]));
			btSelObj[i] = GlobalVar.utilMgr.createGameObjectAsLocal( btSelObj[i], ballTypeSelObj, "BT_0"+i, true );
			btSelObj[i].transform.localPosition = btSelBtnPos[i];
		}
	}
	
	public void BallTypeSelect_initialize()
	{
		selBTNum = 0;
		bFinishSelBT = false;
		BallTypeSelect_setTurnOnOffGrpBox(true);
	}
	
	public void BallTypeSelect_initObject()
	{
		BallTypeSelect_setTurnOnOffGrpBox(true);
	}	
	
	public void BallTypeSelect_Process( bool touchOn )
	{
		Vector3 mPos = new Vector3(0,0,0);
		
		if( touchOn )
		{			
			mPos.x = Input.mousePosition.x * (GlobalVar.SCREEN_HFSIZE.x*2 / Screen.width);
			mPos.y = Input.mousePosition.y * (GlobalVar.SCREEN_HFSIZE.y*2 / Screen.height) - GlobalVar.SCREEN_HFSIZE.y*2;
			
			selBTNum = BallTypeSelect_getSelBT_BoxNum( mPos );
			if( selBTNum!=-1)
			{
				GlobalVar.ballTypeSelNum = selBTNum;
			}
		}
	}
	
//Get
	int BallTypeSelect_getSelBT_BoxNum( Vector3 pos )
	{
		Vector3 scrPntObj = new Vector3(0,0,0);		
		int boxHalfWid = 41;
		
		for( int i=0 ; i<GlobalVar.myTeam.P_starting.ballType.Length ; i++ )
		{
			//Debug.Log("pos="+pos+" / i:"+i+"/"+btSelObj[i].transform.localPosition);
			scrPntObj = btSelObj[i].transform.localPosition;//uiCam.camera.WorldToScreenPoint( btSelObj[i].transform.position );
			
			if( pos.x>=scrPntObj.x-boxHalfWid && pos.x<=scrPntObj.x+boxHalfWid && 
				pos.y>scrPntObj.y-boxHalfWid && pos.y<=scrPntObj.y+boxHalfWid )
			{
				return GlobalVar.myTeam.P_starting.ballType[i];
			}
		}

		return -1;
	}
	
//Set
	public void BallTypeSelect_setTurnOnOffGrpBox( bool sw)
	{
		ballTypeSelObj.gameObject.SetActiveRecursively( sw );
	}
	
	public void BallTypeSelect_setSZArrPickupIdx()
	{
		switch( GlobalVar.ballTypeSelNum )
		{
		case Constants.BALLTYPE_FOURSEAM:
			switch( GlobalVar.myTeam.P_starting.formIdx )
			{
			case (int)GlobalVar.PITCHER_FORM.OVER:					
				GlobalVar.vesStrikeZoneArrIdx = 23;
				break;
			case (int)GlobalVar.PITCHER_FORM.SIDE:
			case (int)GlobalVar.PITCHER_FORM.UNDER:
				GlobalVar.vesStrikeZoneArrIdx = 31;
				break;
			}
			break;
		case Constants.BALLTYPE_TWOSEAM:
			switch( GlobalVar.myTeam.P_starting.formIdx )
			{
			case (int)GlobalVar.PITCHER_FORM.OVER:					
				GlobalVar.vesStrikeZoneArrIdx = 21;
				break;
			case (int)GlobalVar.PITCHER_FORM.SIDE:
			case (int)GlobalVar.PITCHER_FORM.UNDER:					
				GlobalVar.vesStrikeZoneArrIdx = 30;
				break;
			}
			break;
		case Constants.BALLTYPE_SINKER:
			switch( GlobalVar.myTeam.P_starting.formIdx )
			{
			case (int)GlobalVar.PITCHER_FORM.OVER:					
				GlobalVar.vesStrikeZoneArrIdx = 19;
				break;
			case (int)GlobalVar.PITCHER_FORM.SIDE:
			case (int)GlobalVar.PITCHER_FORM.UNDER:					
				GlobalVar.vesStrikeZoneArrIdx = 30;
				break;
			}
			break;
		case Constants.BALLTYPE_CHANGEUP:
			switch( GlobalVar.myTeam.P_starting.formIdx )
			{
			case (int)GlobalVar.PITCHER_FORM.OVER:					
				GlobalVar.vesStrikeZoneArrIdx = 19;
				break;
			case (int)GlobalVar.PITCHER_FORM.SIDE:
			case (int)GlobalVar.PITCHER_FORM.UNDER:					
				GlobalVar.vesStrikeZoneArrIdx = 31;
				break;
			}
			break;
		case Constants.BALLTYPE_PALM:
			switch( GlobalVar.myTeam.P_starting.formIdx )
			{
			case (int)GlobalVar.PITCHER_FORM.OVER:					
				GlobalVar.vesStrikeZoneArrIdx = 24;
				break;
			}
			break;
		case Constants.BALLTYPE_FORK:
			switch( GlobalVar.myTeam.P_starting.formIdx )
			{
			case (int)GlobalVar.PITCHER_FORM.OVER:					
				GlobalVar.vesStrikeZoneArrIdx = 12;
				break;
			case (int)GlobalVar.PITCHER_FORM.SIDE:
			case (int)GlobalVar.PITCHER_FORM.UNDER:					
				GlobalVar.vesStrikeZoneArrIdx = 24;
				break;
			}
			break;
		case Constants.BALLTYPE_CURVE:
			switch( GlobalVar.myTeam.P_starting.formIdx )
			{
			case (int)GlobalVar.PITCHER_FORM.OVER:					
				GlobalVar.vesStrikeZoneArrIdx = 15;
				break;
			case (int)GlobalVar.PITCHER_FORM.SIDE:
				GlobalVar.vesStrikeZoneArrIdx = 28;
				break;
			case (int)GlobalVar.PITCHER_FORM.UNDER:					
				GlobalVar.vesStrikeZoneArrIdx = 35;
				break;
			}
			break;
		case Constants.BALLTYPE_SLIDER:
			switch( GlobalVar.myTeam.P_starting.formIdx )
			{
			case (int)GlobalVar.PITCHER_FORM.OVER:					
				GlobalVar.vesStrikeZoneArrIdx = 20;
				break;
			case (int)GlobalVar.PITCHER_FORM.SIDE:
			case (int)GlobalVar.PITCHER_FORM.UNDER:					
				GlobalVar.vesStrikeZoneArrIdx = 30;
				break;
			}
			break;
		case Constants.BALLTYPE_CUTFB:
			switch( GlobalVar.myTeam.P_starting.formIdx )
			{
			case (int)GlobalVar.PITCHER_FORM.OVER:					
				GlobalVar.vesStrikeZoneArrIdx = 19;
				break;
			case (int)GlobalVar.PITCHER_FORM.SIDE:
			case (int)GlobalVar.PITCHER_FORM.UNDER:					
				GlobalVar.vesStrikeZoneArrIdx = 28;
				break;
			}
			break;
		case Constants.BALLTYPE_RISINGFB:
			switch( GlobalVar.myTeam.P_starting.formIdx )
			{
			case (int)GlobalVar.PITCHER_FORM.OVER:					
				GlobalVar.vesStrikeZoneArrIdx = 13;
				break;
			case (int)GlobalVar.PITCHER_FORM.SIDE:
			case (int)GlobalVar.PITCHER_FORM.UNDER:					
				GlobalVar.vesStrikeZoneArrIdx = 32;
				break;
			}
			break;
		case Constants.BALLTYPE_SINKINGFB:
			switch( GlobalVar.myTeam.P_starting.formIdx )
			{
			case (int)GlobalVar.PITCHER_FORM.OVER:					
				GlobalVar.vesStrikeZoneArrIdx = 13;
				break;
			case (int)GlobalVar.PITCHER_FORM.SIDE:
			case (int)GlobalVar.PITCHER_FORM.UNDER:					
				GlobalVar.vesStrikeZoneArrIdx = 29;
				break;
			}
			break;
		case Constants.BALLTYPE_CIRCLECH:
			switch( GlobalVar.myTeam.P_starting.formIdx )
			{
			case (int)GlobalVar.PITCHER_FORM.OVER:					
				GlobalVar.vesStrikeZoneArrIdx = 19;
				break;
			case (int)GlobalVar.PITCHER_FORM.SIDE:
			case (int)GlobalVar.PITCHER_FORM.UNDER:					
				GlobalVar.vesStrikeZoneArrIdx = 29;
				break;
			}
			break;
		case Constants.BALLTYPE_KNUCKLE:
			switch( GlobalVar.myTeam.P_starting.formIdx )
			{
			case (int)GlobalVar.PITCHER_FORM.OVER:					
				GlobalVar.vesStrikeZoneArrIdx = 28;
				break;
			}
			break;
		case Constants.BALLTYPE_SPLITER:
			switch( GlobalVar.myTeam.P_starting.formIdx )
			{
			case (int)GlobalVar.PITCHER_FORM.OVER:					
				GlobalVar.vesStrikeZoneArrIdx = 14;
				break;
			case (int)GlobalVar.PITCHER_FORM.SIDE:
			case (int)GlobalVar.PITCHER_FORM.UNDER:					
				GlobalVar.vesStrikeZoneArrIdx = 23;
				break;
			}
			break;
		case Constants.BALLTYPE_SLOWCV:
			GlobalVar.vesStrikeZoneArrIdx = 17;
			break;
		case Constants.BALLTYPE_UPSHOOT:
			switch( GlobalVar.myTeam.P_starting.formIdx )
			{
			case (int)GlobalVar.PITCHER_FORM.OVER:					
				GlobalVar.vesStrikeZoneArrIdx = 35;
				break;
			case (int)GlobalVar.PITCHER_FORM.SIDE:
			case (int)GlobalVar.PITCHER_FORM.UNDER:					
				GlobalVar.vesStrikeZoneArrIdx = 25;
				break;
			}
			break;
		case Constants.BALLTYPE_HSLIDER:
			GlobalVar.vesStrikeZoneArrIdx = 19;
			break;
		case Constants.BALLTYPE_FRISBEE:
			switch( GlobalVar.myTeam.P_starting.formIdx )
			{
			case (int)GlobalVar.PITCHER_FORM.OVER:					
				GlobalVar.vesStrikeZoneArrIdx = 35;
				break;
			case (int)GlobalVar.PITCHER_FORM.SIDE:
			case (int)GlobalVar.PITCHER_FORM.UNDER:					
				GlobalVar.vesStrikeZoneArrIdx = 22;
				break;
			}
			break;
		}
		//Debug.Log("BTST_setSZArrIdx() = "+GlobalVar.vesStrikeZoneArrIdx);
	}
//Checker	
	public bool BallTypeSelect_bFinishSelBT()
	{		
		return bFinishSelBT;
	}
	
	public void BallTypeSelect_TouchUp()
	{
		if( selBTNum!=-1)
		{
			BallTypeSelect_setTurnOnOffGrpBox( false );
			bFinishSelBT = true;			
		}
	}
	
	public void BallTypeSelect_TouchDown()
	{
	}
}
