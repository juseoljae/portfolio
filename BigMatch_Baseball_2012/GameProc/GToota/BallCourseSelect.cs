using UnityEngine;
using System.Collections;

public class BallCourseSelect : MonoBehaviour {
	
	Transform breakingArrObj;
	Transform joyStickPointer;
	Transform joyStickBg;
	GameObject[] ballBreakingArr;
	Vector3 preJSPos;
	Vector3 selectionBallPos;
	Vector3 sBallMovedPos;
	
	bool bStartControl;
	int brkArrNum;
	float brkArrAngle;
	Vector3 sBallRot;
	int sBallRotAngle;
	float distXToArr;
	float distYToArr;
	Vector2 ballZoneMax;
	
	// Use this for initialization
	void Start () {
		Debug.Log("BallCourseSelect.Start()");
		breakingArrObj = GlobalVar.homeBaseZoneBox.FindChild("SelectBallCourse").FindChild("BreakingArr");
		joyStickBg = GlobalVar.joyStickObj.FindChild("Joystick_Bg");
		joyStickPointer = GlobalVar.joyStickObj.FindChild("Joystick_Pointer");
		
		
		preJSPos = joyStickPointer.localPosition;
		selectionBallPos = new Vector3(0,0,0);
		sBallMovedPos = new Vector3(0,0,0);
		sBallRot = new Vector3(0,0,0);
		ballZoneMax = new Vector2(0,0);
		sBallRotAngle = 20;
		
		BallCourseSelect_Load();
		
		GlobalVar.strikeZoneSpr =  GlobalVar.strikeZoneObj.GetComponent<UISprite>();		
	}
	
	// Update is called once per frame
	void Update () {}
	
	public void BallCourseSelect_Load()
	{
		ballBreakingArr = new GameObject[4];
		for( int i=0 ; i<ballBreakingArr.Length ; i++ )
		{
			ballBreakingArr[i] = (GameObject)Instantiate( Resources.Load("Prefab/S_GameProc/JoyStick/BallBreakingArr") );
			ballBreakingArr[i].transform.parent = breakingArrObj;
			ballBreakingArr[i].name = "brkArr_"+i;
			ballBreakingArr[i].SetActiveRecursively(false);
		}
		
		BallCourseSelect_setActiveRecursive( false );
	}
		
	public void BallCourseSelect_initJoyStick()//BallMgr_initBallCourseObj
	{
		Debug.Log("BallCourseSelect_initJoyStick()");
		
		Vector3 tmpPos = new Vector3(0,0,0);
		tmpPos = GlobalVar.strikeZoneObj.position;
		tmpPos.z = 10;
		GlobalVar.selectionBall.localPosition = tmpPos;
		joyStickPointer.position = preJSPos;
		selectionBallPos.x = selectionBallPos.y = selectionBallPos.z = 0;
		sBallMovedPos.x = sBallMovedPos.y = sBallMovedPos.z = 0;
		GlobalVar.selectionBall.gameObject.SetActiveRecursively( true );
		brkArrNum = 0;
		distXToArr = distYToArr = 0;
		ballZoneMax.x = ballZoneMax.y = 0;
	}
	
	
		
//Set
	public void BallCourseSelect_setBreakingArr()
	{
		switch( GlobalVar.myTeam.P_starting.formIdx )
		{
		case (int)GlobalVar.PITCHER_FORM.OVER:
			BallCourseSelect_setBreakingArr_1stGroup();
			break;
		case (int)GlobalVar.PITCHER_FORM.SIDE:
		case (int)GlobalVar.PITCHER_FORM.UNDER:
			BallCourseSelect_setBreakingArr_2ndGroup();
			break;
		}
	}
	
	void BallCourseSelect_setBreakingArr_1stGroup()
	{
		Vector3 tmpPos = new Vector3(0,0,10);
		
		//Debug.Log("BallCourseSelect_setBreakingArr_1stGroup() "+GlobalVar.ballTypeSelNum);
		switch( GlobalVar.ballTypeSelNum )
		{
		case Constants.BALLTYPE_TWOSEAM:
		case Constants.BALLTYPE_SINKER:
		case Constants.BALLTYPE_CHANGEUP:
		case Constants.BALLTYPE_CIRCLECH:
		case Constants.BALLTYPE_SPLITER:
			tmpPos.x = GlobalVar.selectionBall.localPosition.x - GlobalVar.distFPArrX;
			tmpPos.y = GlobalVar.selectionBall.localPosition.y - GlobalVar.distFPArrY;
			ballBreakingArr[0].transform.localPosition = tmpPos;
			break;
		case Constants.BALLTYPE_FORK:
		case Constants.BALLTYPE_SLIDER:		
		case Constants.BALLTYPE_UPSHOOT:
		case Constants.BALLTYPE_FRISBEE:
		case Constants.BALLTYPE_CURVE:
		case Constants.BALLTYPE_PALM:
		case Constants.BALLTYPE_KNUCKLE:
		case Constants.BALLTYPE_SLOWCV:
		case Constants.BALLTYPE_HSLIDER:
			for( int i=0 ; i<brkArrNum ; i++ )
			{
				tmpPos.x = GlobalVar.selectionBall.localPosition.x - distXToArr*(i+2);
				tmpPos.y = GlobalVar.selectionBall.localPosition.y - distYToArr*(i+2);
				ballBreakingArr[i].transform.localPosition = tmpPos;
			}
			break;
		}
	}
	
	void BallCourseSelect_setBreakingArr_2ndGroup()
	{
		Vector3 tmpPos = new Vector3(0,0,10);
		switch( GlobalVar.ballTypeSelNum )
		{
		case Constants.BALLTYPE_TWOSEAM:
		case Constants.BALLTYPE_SINKER:
		case Constants.BALLTYPE_CHANGEUP:
		case Constants.BALLTYPE_CIRCLECH:
		case Constants.BALLTYPE_SPLITER:
			tmpPos.x = GlobalVar.selectionBall.localPosition.x - GlobalVar.distFPArrX;
			tmpPos.y = GlobalVar.selectionBall.localPosition.y - GlobalVar.distFPArrY;
			ballBreakingArr[0].transform.localPosition = tmpPos;
			break;
		case Constants.BALLTYPE_FORK:
		case Constants.BALLTYPE_SLIDER:		
		case Constants.BALLTYPE_UPSHOOT:
		case Constants.BALLTYPE_FRISBEE:
			for( int i=0 ; i<brkArrNum ; i++ )
			{
				tmpPos.x = GlobalVar.selectionBall.localPosition.x - distXToArr*(i+2);
				tmpPos.y = GlobalVar.selectionBall.localPosition.y - distYToArr*(i+2);
				ballBreakingArr[i].transform.localPosition = tmpPos;				
			}
			break;
		case Constants.BALLTYPE_CURVE:
			switch( GlobalVar.myTeam.P_starting.formIdx )
			{
			case (int)GlobalVar.PITCHER_FORM.SIDE:
				for( int i=0 ; i<brkArrNum ; i++ )
				{
					tmpPos.x = GlobalVar.selectionBall.localPosition.x - distXToArr*(i+2);
					tmpPos.y = GlobalVar.selectionBall.localPosition.y - distYToArr*(i+2);
					ballBreakingArr[i].transform.localPosition = tmpPos;
				}
				break;
			case (int)GlobalVar.PITCHER_FORM.UNDER:
				tmpPos.x = GlobalVar.selectionBall.localPosition.x - GlobalVar.distFPArrX;
				tmpPos.y = GlobalVar.selectionBall.localPosition.y - GlobalVar.distFPArrY;
				ballBreakingArr[0].transform.localPosition = tmpPos;
				break;
			}
			break;
		}
	}
	
	public void BallCourseSelect_setHideSelectionBallArr()
	{
		brkArrAngle = 0;
		for( int i=0 ; i<brkArrNum ; i++ )
		{
			ballBreakingArr[i].SetActiveRecursively( false );
		}
		brkArrNum = 0;
	}
	
	public void BallCourseSelect_setActiveRecursive( bool sw )
	{
		joyStickPointer.gameObject.SetActiveRecursively(sw);
		joyStickBg.gameObject.SetActiveRecursively(sw);
	}
	
	public void BallCourseSelect_setBrkArrActiveRecursive()
	{
		Vector3 tmpRot = new Vector3(0,0,0);
		
		brkArrAngle = Mathf.Atan2(GlobalVar.distFPArrX, GlobalVar.distFPArrY) * Mathf.Rad2Deg*(-1);
		//Debug.Log("000 BallCourseSelect_setBreakingArr_2ndGroup() angle="+brkArrAngle);
		
		switch( GlobalVar.ballTypeSelNum )
		{
		case Constants.BALLTYPE_TWOSEAM:
		case Constants.BALLTYPE_SINKER:
		case Constants.BALLTYPE_CHANGEUP:
		case Constants.BALLTYPE_CIRCLECH:
		case Constants.BALLTYPE_SPLITER:
			brkArrNum = 1;
			break;
		case Constants.BALLTYPE_FORK:
		case Constants.BALLTYPE_UPSHOOT:
		case Constants.BALLTYPE_SLOWCV:
		case Constants.BALLTYPE_HSLIDER:
			brkArrNum = 3;
			break;
		case Constants.BALLTYPE_CURVE:
			switch( GlobalVar.myTeam.P_starting.formIdx )
			{
			case (int)GlobalVar.PITCHER_FORM.OVER:
			case (int)GlobalVar.PITCHER_FORM.SIDE:
				brkArrNum = 3;
				break;
			case (int)GlobalVar.PITCHER_FORM.UNDER:
				brkArrNum = 1;
				break;
			}
			break;
		case Constants.BALLTYPE_SLIDER:
		case Constants.BALLTYPE_PALM:
		case Constants.BALLTYPE_KNUCKLE:
			brkArrNum = 2;
			break;
		case Constants.BALLTYPE_FRISBEE:
			brkArrNum = 4;
			break;
		}		
		
		if( brkArrNum>1 )
		{
			distXToArr = (GlobalVar.distFPArrX)/(brkArrNum+1);
			distYToArr = (GlobalVar.distFPArrY)/(brkArrNum+1);
		}
		
		tmpRot.z = brkArrAngle;
		for( int i=0 ; i<brkArrNum ; i++ )
		{
			ballBreakingArr[i].transform.localEulerAngles = tmpRot;
			ballBreakingArr[i].SetActiveRecursively( true );
		}
		GlobalVar.selectionBall.gameObject.SetActiveRecursively(true);
		BallCourseSelect_setBallZoneLimit();
	}
	
	void BallCourseSelect_setBallZoneLimit()
	{		
		switch( GlobalVar.ballTypeSelNum )
		{
		case Constants.BALLTYPE_SINKER:
			ballZoneMax.x = -7;
			ballZoneMax.y = 7;
			break;
		case Constants.BALLTYPE_CHANGEUP:
			ballZoneMax.x = 0;
			ballZoneMax.y = 7;
			break;
		case Constants.BALLTYPE_SLIDER:
			ballZoneMax.x = 15;
			ballZoneMax.y = 5;
			break;
		case Constants.BALLTYPE_PALM:
			ballZoneMax.x = 0;
			ballZoneMax.y = 15;
			break;
		case Constants.BALLTYPE_FORK:
			ballZoneMax.x = 0;
			ballZoneMax.y = 25;
			break;
		case Constants.BALLTYPE_CURVE:
			switch( GlobalVar.myTeam.P_starting.formIdx )
			{
			case (int)GlobalVar.PITCHER_FORM.OVER:
			case (int)GlobalVar.PITCHER_FORM.SIDE:
				ballZoneMax.x = 0;
				ballZoneMax.y = 25;
				break;
			case (int)GlobalVar.PITCHER_FORM.UNDER:
				ballZoneMax.x = 5;
				ballZoneMax.y = 0;
				break;
			}
			break;
		case Constants.BALLTYPE_SPLITER:
			ballZoneMax.x = 0;
			ballZoneMax.y = 5;
			break;
		case Constants.BALLTYPE_KNUCKLE:
			ballZoneMax.x = 0;
			ballZoneMax.y = 15;
			break;
		case Constants.BALLTYPE_SLOWCV:
			ballZoneMax.x = 5;
			ballZoneMax.y = 20;
			break;
		case Constants.BALLTYPE_UPSHOOT:
			ballZoneMax.x = 25;
			ballZoneMax.y = -4;
			break;
		case Constants.BALLTYPE_HSLIDER:
			ballZoneMax.x = 25;
			ballZoneMax.y = 5;
			break;
		case Constants.BALLTYPE_FRISBEE:
			ballZoneMax.x = 30;
			ballZoneMax.y = 7;
			break;
		default:
			ballZoneMax.x = ballZoneMax.y = 0;
			break;
		}
	}
	
	public void BallCourseSelect_setStrikeZoneAlpha()
	{
		if( GlobalVar.strikeZoneSpr.alpha>0 )
		{
			GlobalVar.alphaDep-=0.02f;
			if( GlobalVar.alphaDep<=0 ) 
			{
				GlobalVar.alphaDep = 0;
			}
			GlobalVar.strikeZoneSpr.alpha = GlobalVar.alphaDep;
		}
	}
	
	
//Process
	public void BallCourseSelect_Process(){		
		BallCourseSelect_controlJoyStick();
		BallCourseSelect_RotationSelectionBall();
		BallCourseSelect_TouchUp();
		BallCourseSelect_TouchDown();
	}	
		
	void BallCourseSelect_controlJoyStick()
	{
		Vector3 scrnTouchedPos = new Vector3( 0, 0, 0 );
		Vector3 JsMovingPos = new Vector3( 0, 0, 0 );
		Vector3 JsMovedPos = new Vector3( 0, 0, 0 );
		Vector3 sBallMovingPos = new Vector3(0,0,0);
		Vector3 preJStickPnterPos = new Vector3(0,0,0);
		Vector3 preSelBallPos = new Vector3(0,0,0);
		float JsMoveGapX = 0.0f;
		float JsMoveGapY = 0.0f;
		float zoneBall_gapWid = 50*3;//38;
		float zoneBall_gapHt = 40*3;
		int magni = 2*3;
		
		if( bStartControl )
		{
			
			scrnTouchedPos.x = Input.mousePosition.x * (GlobalVar.SCREEN_HFSIZE.x*2 / Screen.width);
			scrnTouchedPos.y = Input.mousePosition.y * (GlobalVar.SCREEN_HFSIZE.y*2 / Screen.height);
			
			JsMovingPos = JsMovedPos = preJStickPnterPos = joyStickPointer.localPosition;
			sBallMovingPos = preSelBallPos = GlobalVar.selectionBall.localPosition;
			
			//X axis
			JsMoveGapX = (GlobalVar.utilMgr.ABS(JsMovingPos.x) - GlobalVar.utilMgr.ABS(scrnTouchedPos.x));
			if( JsMoveGapX > 0 )
			{
				JsMovingPos.x = preJStickPnterPos.x - GlobalVar.utilMgr.ABS(JsMoveGapX);
				sBallMovingPos.x = preSelBallPos.x + GlobalVar.utilMgr.ABS(JsMoveGapX)/magni;
				
			}	
			else if( JsMoveGapX<0 )
			{
				JsMovingPos.x = preJStickPnterPos.x + GlobalVar.utilMgr.ABS(JsMoveGapX);
				sBallMovingPos.x = preSelBallPos.x - GlobalVar.utilMgr.ABS(JsMoveGapX)/magni;
			}
			
			//Y axis
			JsMoveGapY = (GlobalVar.utilMgr.ABS(JsMovingPos.y) - GlobalVar.utilMgr.ABS(scrnTouchedPos.y));
			if( JsMoveGapY>0 )
			{
				JsMovingPos.y = preJStickPnterPos.y - GlobalVar.utilMgr.ABS(JsMoveGapY);
				sBallMovingPos.y = preSelBallPos.y - GlobalVar.utilMgr.ABS(JsMoveGapY)/magni;
			}
			else if( JsMoveGapY<0 )
			{
				JsMovingPos.y = preJStickPnterPos.y + GlobalVar.utilMgr.ABS(JsMoveGapY);
				sBallMovingPos.y = preSelBallPos.y + GlobalVar.utilMgr.ABS(JsMoveGapY)/magni;
			}
			
			//Debug.Log("JoyStick POS:"+JsMovingPos+"///"+joyStickBg.localPosition+"//"+GlobalVar.selectionBall.localPosition);	
			//limit moving area
			if( JsMovingPos.x>joyStickBg.localPosition.x-zoneBall_gapWid+ballZoneMax.x && JsMovingPos.x<joyStickBg.localPosition.x+zoneBall_gapWid+ballZoneMax.x )
			{
				JsMovedPos.x = JsMovingPos.x;
				sBallMovedPos.x = sBallMovingPos.x;
				//Debug.Log("Arrow POS:"+ballBreakingArr[brkArrNum-1].transform.localPosition+"/"+(brkArrNum-1)+"///"+JsMovingPos+"//"+GlobalVar.strikeZoneObj.localPosition+"/"+GlobalVar.selectionBall.localPosition);
			}
			
			if( JsMovingPos.y>joyStickBg.localPosition.y-zoneBall_gapHt+ballZoneMax.y && JsMovingPos.y<joyStickBg.localPosition.y+zoneBall_gapHt+ballZoneMax.y )
			{
				JsMovedPos.y = JsMovingPos.y;
				sBallMovedPos.y = sBallMovingPos.y;
				//Debug.Log("Arrow POS:"+ballBreakingArr[brkArrNum-1].transform.localPosition+"/"+(brkArrNum-1)+"///"+JsMovingPos+"//"+GlobalVar.strikeZoneObj.localPosition+"/"+GlobalVar.selectionBall.localPosition);
			}
			
			sBallMovedPos.z = sBallMovingPos.z;
			
			joyStickPointer.localPosition = JsMovedPos;			
			GlobalVar.selectionBall.localPosition = sBallMovedPos;
			BallCourseSelect_setStrikeZoneAlpha();
			BallCourseSelect_setBreakingArr();
		}
	}
	
	public void BallCourseSelect_RotationSelectionBall()
	{
		if( !GlobalVar.selectionBall.gameObject.active ) return;
		//Rotation selection ball object
		sBallRot.x += sBallRotAngle;
		sBallRot.y += sBallRotAngle;
		if( sBallRotAngle>360 )sBallRotAngle=0;
		GlobalVar.selectionBall.localEulerAngles = sBallRot;
	}
	
	void BallCourseSelect_TouchUp()
	{
		if( Input.GetMouseButtonUp(0) )
		{
			if( bStartControl )
			{
				joyStickPointer.localPosition = preJSPos;
				joyStickBg.localPosition = preJSPos;
				BallCourseSelect_setActiveRecursive( false );
				bStartControl = false;
				GlobalVar.bFinishBallDir = true;
			}
		}
	}
	
	void BallCourseSelect_TouchDown()
	{
		Vector3 tmpPos = new Vector3(0,0,0);
		if( Input.GetMouseButtonDown(0) )
		{
			if( !bStartControl )
			{
				tmpPos.x = Input.mousePosition.x * (GlobalVar.SCREEN_HFSIZE.x*2 / Screen.width);
				tmpPos.y = Input.mousePosition.y * (GlobalVar.SCREEN_HFSIZE.y*2 / Screen.height);
				
				joyStickBg.localPosition = tmpPos;
				joyStickPointer.localPosition = tmpPos;
				BallCourseSelect_setActiveRecursive( true );
				bStartControl = true;
			}
		}
	}
}
