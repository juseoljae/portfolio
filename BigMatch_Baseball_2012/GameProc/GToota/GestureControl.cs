using UnityEngine;
using System.Collections;

public class GestureControl : MonoBehaviour {
	
	public struct GestureBtn
	{
		public GameObject gestureObj;
		public GameObject gesturePrsObj;
		public UISprite gestureSpr;
		public UISprite gesturePrsSpr;
		public bool bGestureBtnOn;
		public byte gFrmCnt;
		public byte gCnt;
	};
	GestureBtn[] gBtn;
	UISprite gArrSpr;
	GameObject gBtn_Bad;
	UISprite gBtn_BadSpr;
	byte[][] gestureData_Right;
	byte[][] gestureData_Left;
	byte[][] gestureData;
	float gestureBtnScale;
	bool bStartGesture;
	bool bFinishGesture;
	int gesture1stBtnIdx;
	
	Transform controlGaugeObj;
	GameObject[] ctrGauge;	
	UISprite[] ctrGaugeSpr;
	GameObject[] ctrEffPerfect;	
	UISprite ctrEffPerfectTxtSpr;
	UISpriteAnimation ctrEffPerfectAnim;
	bool bStartCtrGauge;
	float ctrGgRotRAngle;
	float ctrGgRotLAngle;
	float ctrGgR;
	float ctrRotCircle;
	bool ctrbEndRot;
	byte ctrGgIdxCnt;
	//byte ctrStatus;// 0:Good / 1:Weak / 2:Over / 3:Bad
	int ctrGgBadBtnIdx;
	byte ctr_BadActionCnt;
	bool ctr_fWarnBadAction;
	byte ctr_warnCnt;
	byte ctr_BadBtnActionCnt;
	bool ctr_fWarnBadBtnAction;
	byte ctr_warnBtnCnt;
	float ctrGauge_SlidePosX;
	
	
	
	bool bHitterView;
	bool bFinishPitching;
	GameObject signGBObj;
	
	Transform dTxt;
	
	//bool bFinishGesture;
	
	//public GestureControl(){}
	
	// Use this for initialization
	void Start () {
		Debug.Log("GestureControl.Start()");
		
		controlGaugeObj = GlobalVar.playUIObj.FindChild("UI_Cam").FindChild("Panel").FindChild("Anchor_Center").FindChild("ControlGauge");
		
		GestureControl_initialize();
		GestureControl_initGestureArrowData();
		GestureControl_setGestureDir();
		
		GestureControl_Load();
	}
	
	// Update is called once per frame
	void Update () {}
	
//Loading
	public void GestureControl_Load()
	{
		//Each position of Gesture Buttons
		Vector3[] gBtnPos;
		Debug.Log("GestureConrol.Load()");
		
		gBtn = new GestureBtn[5];
		gBtnPos = new Vector3[gBtn.Length];
		gBtnPos[0] = new Vector3(-158, 251, 0);
		gBtnPos[1] = new Vector3(-158, 163, 0);
		gBtnPos[2] = new Vector3(-158, 75, 0);
		gBtnPos[3] = new Vector3(-246, 163, 0);
		gBtnPos[4] = new Vector3(-68, 163, 0);
		for( int i=0 ; i<gBtn.Length ; i++ )
		{
			gBtn[i] = new GestureBtn();
			gBtn[i].gestureObj = (GameObject)Instantiate( Resources.Load("Prefab/S_GameProc/Gesture/Gesture_Button") );
			gBtn[i].gestureObj = GlobalVar.utilMgr.createGameObjectAsLocal( gBtn[i].gestureObj, GlobalVar.gestureObj, "Gesture_Button_"+i, false );
			gBtn[i].gestureObj.transform.localPosition = gBtnPos[i];
			gBtn[i].gesturePrsObj = (GameObject)Instantiate( Resources.Load("Prefab/S_GameProc/Gesture/Gesture_Button_Press") );
			gBtn[i].gesturePrsObj = GlobalVar.utilMgr.createGameObjectAsLocal( gBtn[i].gesturePrsObj, GlobalVar.gestureObj, "Gesture_ButtonPress_"+i, false );
			gBtn[i].gesturePrsObj.transform.localPosition = gBtnPos[i];
			
			gBtn[i].gestureSpr = gBtn[i].gestureObj.GetComponent<UISprite>();
			gBtn[i].gesturePrsSpr = gBtn[i].gesturePrsObj.GetComponent<UISprite>();
			
		}
		gestureBtnScale = (gBtn[0].gestureObj.transform.localScale.x+6)/2;
		
		gBtn_Bad = (GameObject)Instantiate( Resources.Load("Prefab/S_GameProc/Gesture/Gesture_Button_Bad") );
		gBtn_Bad = GlobalVar.utilMgr.createGameObjectAsLocal( gBtn_Bad, GlobalVar.gestureObj, "Gesture_Button_Bad", false );
		gBtn_Bad.transform.localPosition = gBtnPos[0];
		gBtn_BadSpr = gBtn_Bad.GetComponent<UISprite>();
		//gBtnPos = null;
		
		ctrGauge = new GameObject[12];
		ctrGgIdxCnt = 0;
		for( int i=0 ; i<6 ; i++ )
		{
			ctrGauge[i] = (GameObject)Instantiate( Resources.Load("Prefab/S_GameProc/Gesture/ControlG_"+i) );
			ctrGauge[i] = GlobalVar.utilMgr.createGameObjectAsLocal( ctrGauge[i], controlGaugeObj, "ControlG_"+i, false);
		}
		for( int i=0; i<3 ; i++ )
		{
			for( int j=0 ; j<2 ; j++ )
			{
				ctrGauge[6+ctrGgIdxCnt] = (GameObject)Instantiate( Resources.Load("Prefab/S_GameProc/Gesture/ControlG_"+(i+10)) );
				ctrGauge[6+ctrGgIdxCnt] = GlobalVar.utilMgr.createGameObjectAsLocal( ctrGauge[6+ctrGgIdxCnt], controlGaugeObj, "ControlG_"+(6+ctrGgIdxCnt), false);
				ctrGgIdxCnt++;
			}
		}
		ctrGaugeSpr = new UISprite[11];
		for( int i=0 ; i<ctrGaugeSpr.Length ; i++ )
		{
			ctrGaugeSpr[i] = ctrGauge[i].GetComponent<UISprite>();
		}
		ctrEffPerfect = new GameObject[2];
		for( int i=0 ; i<ctrEffPerfect.Length ; i++ )
		{
			if(i==0) ctrEffPerfect[i] = (GameObject)Instantiate( Resources.Load("Prefab/S_GameProc/Gesture/CG_Eff_Perfect_00") );
			else ctrEffPerfect[i] = (GameObject)Instantiate( Resources.Load("Prefab/S_GameProc/Gesture/CG_Eff_Perfect_06") );
			ctrEffPerfect[i] = GlobalVar.utilMgr.createGameObjectAsLocal( ctrEffPerfect[i], controlGaugeObj, "CG_Eff_PF_"+i, false);
		}
		ctrEffPerfectTxtSpr = ctrEffPerfect[1].GetComponent<UISprite>();
		ctrEffPerfectAnim = ctrEffPerfect[0].GetComponent<UISpriteAnimation>();
		
		GestureControl_initObject();
	}
	
//Initialize
	public void GestureControl_initialize()
	{
		GlobalVar.control_Status = Constants.STATE_CG_PROC;//Good(Default)
		bStartGesture = bFinishGesture = false;
		gesture1stBtnIdx = -1;
		ctrGgBadBtnIdx = -1;
		bStartCtrGauge = false;
		ctrGgRotRAngle = 0;//angle
		ctrGgRotLAngle = 135;//angle
		ctrRotCircle = 0;
		ctrbEndRot = false;
		ctrGgR = 101;//pxel
		gArrSpr = null;
		ctr_BadActionCnt = ctr_warnCnt = 0;
		ctr_fWarnBadAction = false;
		ctr_BadBtnActionCnt = ctr_warnBtnCnt = 0;;
		ctr_fWarnBadBtnAction = false;
		ctrGauge_SlidePosX = 0;
		
		bFinishPitching = false;
		bHitterView = false;
		
	}
	
	public void GestureControl_initObject()
	{
		Vector3 tmpPos = new Vector3(0,0,0);
		Color tmpClr = new Color(1,1,1);
		
		for( int i=0 ; i<gBtn.Length ; i++ )
		{
			gBtn[i].bGestureBtnOn = false;
			gBtn[i].gestureObj.SetActiveRecursively( false );
		}
		tmpPos.x = 96.8f;
		ctrGauge[3].transform.localPosition = tmpPos;
		ctrGauge[4].transform.localPosition = tmpPos;
		ctrGauge[5].transform.localPosition = tmpPos;
		tmpPos.x = ctrGgR*2;
		tmpPos.y = ctrGgR*2;
		tmpPos.z = 1;
		ctrGauge[0].transform.localScale = tmpPos;
		tmpPos.x = tmpPos.y = tmpPos.z = 0;
		ctrGauge[0].transform.localEulerAngles = tmpPos;
		for( int i=0 ; i<gBtn.Length ; i++ )
		{
			gBtn[i].gestureSpr.alpha = 1;
			gBtn[i].gesturePrsSpr.alpha = 1;
			gBtn[i].gestureObj.SetActiveRecursively(false);
			gBtn[i].gesturePrsObj.SetActiveRecursively(false);
		}
		gBtn_BadSpr.alpha=1;
		gBtn_Bad.SetActiveRecursively(false);
		
		for( int i=0 ; i<ctrGaugeSpr.Length ; i++ )
		{
			if(i>=3 && i<=5) ctrGaugeSpr[i].alpha = 0;
			else ctrGaugeSpr[i].alpha = 1;
		}
		GestureControl_setActiveRecursiveControlGauge(false);
		ctrGaugeSpr[0].color = tmpClr;
		ctrEffPerfectTxtSpr.alpha = 0;
		tmpPos.x = tmpPos.y = tmpPos.z = 0;
		tmpPos.x = 96.8f;
		ctrEffPerfect[1].transform.localPosition = tmpPos;
	}
	
	public void GestureControl_initGestureArrow()
	{
		Vector3 tmpScale;
		Vector3 tmpPos;
		
		GameObject tmpArr = (GameObject)Instantiate(Resources.Load("Prefab/S_GameProc/GestureArr/GestureArr_"+GlobalVar.ballTypeSelNum));
		tmpPos = tmpArr.transform.localPosition;
		tmpScale = tmpArr.transform.localScale;
		tmpArr.transform.parent = GlobalVar.gestureObj;		
		switch( GlobalVar.myTeam.P_starting.handDir )
		{
		case (int)GlobalVar.HANDDIR.LEFT:
			switch( GlobalVar.ballTypeSelNum )
			{
				case Constants.BALLTYPE_TWOSEAM:
				tmpPos.x = -205.5481f;//Mirror. it's Set up fixed position
				break;
				//other balltype arrow will be added
			}
			tmpScale.x *= (-1);
			break;
		}
		tmpArr.transform.localPosition = tmpPos;
		tmpArr.transform.localScale = tmpScale;
		tmpArr.name = "GestureArr_"+GlobalVar.ballTypeSelNum;
		gArrSpr = tmpArr.GetComponent<UISprite>();
	}
	
	void GestureControl_initGestureArrowData()
	{
		gestureData_Right = new byte[Constants.BALLTYPE_MAXNUM][];
		gestureData_Left = new byte[Constants.BALLTYPE_MAXNUM][];
		
		for( int i=0 ; i<Constants.BALLTYPE_MAXNUM ; i++ )
		{
			switch( i )
			{
			case Constants.BALLTYPE_FOURSEAM:
				gestureData_Right[i] = new byte[]{0, 1, 2};
				gestureData_Left [i] = new byte[]{0, 1, 2};
				break;
			case Constants.BALLTYPE_TWOSEAM:
				gestureData_Right[i] = new byte[]{0, 1, 4};
				gestureData_Left [i] = new byte[]{0, 1, 3};
				break;
			case Constants.BALLTYPE_SINKER:
				gestureData_Right[i] = new byte[]{0, 1, 4, 2};
				gestureData_Left [i] = new byte[]{0, 1, 3, 2};
				break;
			case Constants.BALLTYPE_CHANGEUP:
				gestureData_Right[i] = new byte[]{2, 1, 0};
				gestureData_Left [i] = new byte[]{2, 1, 0};
				break;
			case Constants.BALLTYPE_PALM:
				gestureData_Right[i] = new byte[]{4, 1, 3, 2};
				gestureData_Left [i] = new byte[]{3, 1, 4, 2};
				break;
			case Constants.BALLTYPE_FORK:
				gestureData_Right[i] = new byte[]{1, 2, 3, 0};
				gestureData_Left [i] = new byte[]{1, 2, 4, 0};
				break;
			case Constants.BALLTYPE_CURVE:
				gestureData_Right[i] = new byte[]{0, 3, 2, 1};
				gestureData_Left [i] = new byte[]{0, 4, 2, 1};
				break;
			case Constants.BALLTYPE_SLIDER:
				gestureData_Right[i] = new byte[]{1, 3, 0};
				gestureData_Left [i] = new byte[]{1, 4, 0};
				break;
			case Constants.BALLTYPE_CUTFB:
				gestureData_Right[i] = new byte[]{0, 1, 2, 3};
				gestureData_Left [i] = new byte[]{0, 1, 2, 4};
				break;
			case Constants.BALLTYPE_RISINGFB:
				gestureData_Right[i] = new byte[]{0, 1, 2, 4};
				gestureData_Left [i] = new byte[]{0, 1, 2, 3};
				break;
			case Constants.BALLTYPE_SINKINGFB:
				gestureData_Right[i] = new byte[]{0, 4, 2, 1};
				gestureData_Left [i] = new byte[]{0, 3, 2, 1};
				break;
			case Constants.BALLTYPE_CIRCLECH:
				gestureData_Right[i] = new byte[]{2, 3, 0};
				gestureData_Left [i] = new byte[]{2, 4, 0};
				break;
			case Constants.BALLTYPE_KNUCKLE:
				gestureData_Right[i] = new byte[]{4, 1, 2, 3};
				gestureData_Left [i] = new byte[]{3, 1, 2, 4};
				break;
			case Constants.BALLTYPE_SPLITER:
				gestureData_Right[i] = new byte[]{1, 0, 4, 2};
				gestureData_Left [i] = new byte[]{1, 0, 3, 2};
				break;
			case Constants.BALLTYPE_SLOWCV:
				gestureData_Right[i] = new byte[]{0, 3, 2, 4};
				gestureData_Left [i] = new byte[]{0, 4, 2, 3};
				break;
			case Constants.BALLTYPE_UPSHOOT:
				gestureData_Right[i] = new byte[]{1, 4, 0, 3};
				gestureData_Left [i] = new byte[]{1, 3, 0, 4};
				break;
			case Constants.BALLTYPE_HSLIDER:
				gestureData_Right[i] = new byte[]{4, 1, 3, 0};
				gestureData_Left [i] = new byte[]{3, 1, 4, 0};
				break;
			case Constants.BALLTYPE_FRISBEE:
				gestureData_Right[i] = new byte[]{4, 0, 1, 3};
				gestureData_Left [i] = new byte[]{3, 0, 1, 4};
				break;
			}
		}
	}
	
//Set
	public void GestureControl_setGameView( bool sw )
	{
		bHitterView = sw;
	}
	
	public void GestureControl_setFinishPitching( bool sw )
	{
		bFinishPitching = sw;
	}
	
	public void GestureControl_setActiveRecursiveBtn( bool sw )
	{
		for( int i=0 ; i<gBtn.Length ; i++ )
		{
			gBtn[i].gestureObj.SetActiveRecursively( sw );
		}
	}
	
	void GestureControl_setActiveRecursiveControlGauge( bool sw )
	{
		for( int i=0 ; i<ctrGauge.Length ; i++ )
		{
			ctrGauge[i].SetActiveRecursively(sw);
		}
		for( int i=0 ; i<ctrEffPerfect.Length ; i++ )
		{
			ctrEffPerfect[i].SetActiveRecursively(false);
		}
	}
	
	void GestureControl_setActiveRecursiveControls( bool sw )
	{
		ctrGauge[0].SetActiveRecursively(sw);
		ctrGauge[1].SetActiveRecursively(sw);
		ctrGauge[6].SetActiveRecursively(sw);
		ctrGauge[7].SetActiveRecursively(sw);
	}
	
	public void GestureControl_setGestureDir()
	{
		switch( GlobalVar.myTeam.P_starting.handDir )
		{
		case (int)GlobalVar.HANDDIR.RIGHT:
			gestureData = gestureData_Right;
			break;
		case (int)GlobalVar.HANDDIR.LEFT:
			gestureData = gestureData_Left;
			break;
		}
	}
	
	void GestureControl_setGestureObjectProp( int num )
	{
		//Debug.Log("BallMgr_setGestureObjectProp() num = "+num);
		gBtn[ num ].bGestureBtnOn = true;
		gBtn[ num ].gestureObj.SetActiveRecursively(false);
		gBtn[ num ].gesturePrsObj.SetActiveRecursively(true);
	}
	
	void GestureControl_setControlGaugeStatus( byte state )
	{
		Color tmpClr = new Color(0,0,0);
		
		switch( state )
		{
		case Constants.STATE_CG_WEAK:
			ctrGaugeSpr[0].color = tmpClr;
			ctrGauge[6].SetActiveRecursively(false);
			ctrGauge[7].SetActiveRecursively(false);
			ctrGauge[5].SetActiveRecursively(true);
			ctrGauge[10].SetActiveRecursively(true);
			ctrGauge[11].SetActiveRecursively(true);
			ctrGauge[10].transform.localPosition = ctrGauge[6].transform.localPosition;
			ctrGauge[11].transform.localPosition = ctrGauge[7].transform.localPosition;
			GestureControl_setBallCourseAsControl();
			GlobalVar.CG_ctlDegree = ctrGgR/101;
			break;
		case Constants.STATE_CG_OVER:
			ctrGaugeSpr[0].color = tmpClr;
			ctrGauge[6].SetActiveRecursively(false);
			ctrGauge[7].SetActiveRecursively(false);
			ctrGauge[4].SetActiveRecursively(true);
			ctrGauge[10].SetActiveRecursively(true);
			ctrGauge[11].SetActiveRecursively(true);
			ctrGauge[10].transform.localPosition = ctrGauge[6].transform.localPosition;
			ctrGauge[11].transform.localPosition = ctrGauge[7].transform.localPosition;
			break;
		case Constants.STATE_CG_BAD:
			ctrGauge[0].SetActiveRecursively(false);
			ctrGauge[6].SetActiveRecursively(false);
			ctrGauge[7].SetActiveRecursively(false);
			
			ctrGauge[2].SetActiveRecursively(true);
			ctrGauge[8].SetActiveRecursively(true);
			ctrGauge[9].SetActiveRecursively(true);
			ctrGauge[3].SetActiveRecursively(true);
			
			ctrGauge[2].transform.localScale = ctrGauge[0].transform.localScale;
			ctrGauge[2].transform.localEulerAngles = ctrGauge[0].transform.localEulerAngles;
			ctrGauge[8].transform.localPosition = ctrGauge[6].transform.localPosition;
			ctrGauge[9].transform.localPosition = ctrGauge[7].transform.localPosition;
			gBtn[ctrGgBadBtnIdx].gestureObj.SetActiveRecursively(false);
			gBtn_Bad.transform.localPosition = gBtn[ctrGgBadBtnIdx].gestureObj.transform.localPosition;
			gBtn_Bad.SetActiveRecursively(true);
			
			GestureControl_setBadBallCourse();
			break;
		case Constants.STATE_CG_PERFECT:
			ctrGauge[0].SetActiveRecursively(false);
			ctrGauge[6].SetActiveRecursively(false);
			ctrGauge[7].SetActiveRecursively(false);
			ctrEffPerfect[0].SetActiveRecursively(true);
			ctrEffPerfect[1].SetActiveRecursively(true);
			ctrEffPerfectAnim.Reset();
			break;
		case Constants.STATE_CG_NORMAL:
			GestureControl_setBallCourseAsControl();
			GlobalVar.CG_ctlDegree = 1-(ctrGgR/101);
			break;
		}
		GlobalVar.control_Status = state;
		bStartCtrGauge = false;
		//Debug.Log("GestureControl_setControlGaugeStatus().bStartCtrGauge = "+bStartCtrGauge);
		ctrGauge[1].SetActiveRecursively(false);
		GlobalVar.selectionBall.gameObject.SetActiveRecursively(false);
		//Debug.Log("control gauge degree = "+GlobalVar.CG_ctlDegree);
	}
	
	void GestureControl_setBallCourseAsControl()
	{
		Vector3 tmpPos = new Vector3(0,0,0);
		int USERCONTRL_MAX_RADIUS = 10;
		float dPer = ctrGgR/101;
		float szR = dPer*USERCONTRL_MAX_RADIUS;
		Vector2 randPntInCircle = Random.insideUnitCircle * szR;
		
		tmpPos = GlobalVar.selectionBall.localPosition;
		tmpPos.x += randPntInCircle.x;
		tmpPos.y += randPntInCircle.y;
		GlobalVar.selectionBall.localPosition = tmpPos;
		//Debug.Log("Control Gauge Radius:"+ctrGgR+"/ decreased percent : "+(ctrGgR/101*100)+"/"+szR);
	}
	
	void GestureControl_setBadBallCourse()
	{
		Vector3 tmpPos = new Vector3(0,0,0);
		//int 
		GlobalVar.badBallCrsRandom = (int)Random.Range(0,2);
		
		//Debug.Log("GestureControl_setBadBallCourse() r:"+GlobalVar.badBallCrsRandom);
		
		//when Control is Bad, there are three case
		switch(GlobalVar.badBallCrsRandom)
		{
		case 0:
			//1. Middle course of Strike zone with slow ball speed
			//init selectionBall's position
			tmpPos = GlobalVar.strikeZoneObj.position;
			GlobalVar.selectionBall.localPosition = tmpPos;			
			//Then it makes middle position of Strike zone
			tmpPos = GlobalVar.selectionBall.localPosition;
			tmpPos.x += GlobalVar.distFPArrX;
			tmpPos.y += GlobalVar.distFPArrY;
			GlobalVar.selectionBall.localPosition = tmpPos;
			break;
		case 1:
		case 2:
			//3. out of Strike Zone, boundBall
			switch( GlobalVar.ballTypeSelNum )
			{
			case Constants.BALLTYPE_FOURSEAM:
			case Constants.BALLTYPE_HSLIDER:
				tmpPos.y = -4;//4;
				break;
			case Constants.BALLTYPE_TWOSEAM:
			case Constants.BALLTYPE_SINKER:
			case Constants.BALLTYPE_SLIDER:
			case Constants.BALLTYPE_CIRCLECH:
			case Constants.BALLTYPE_SPLITER:
			case Constants.BALLTYPE_CUTFB:
			case Constants.BALLTYPE_KNUCKLE:
			case Constants.BALLTYPE_FRISBEE:
				tmpPos.y = -2;
				break;
			case Constants.BALLTYPE_CHANGEUP:	
			case Constants.BALLTYPE_PALM:
				tmpPos.y = 2;
				break;
			case Constants.BALLTYPE_FORK:
			case Constants.BALLTYPE_SLOWCV:
				tmpPos.y = 6f;
				break;
			case Constants.BALLTYPE_CURVE:
				switch( GlobalVar.myTeam.P_starting.formIdx )
				{
				case (int)GlobalVar.PITCHER_FORM.OVER:
				case (int)GlobalVar.PITCHER_FORM.SIDE:			
					tmpPos.y = 3;
					break;
				case (int)GlobalVar.PITCHER_FORM.UNDER:			
					tmpPos.y = -5;
					break;
				}
				break;	
			case Constants.BALLTYPE_RISINGFB:
				tmpPos.y = -5;
				break;
			case Constants.BALLTYPE_SINKINGFB:
				switch( GlobalVar.myTeam.P_starting.formIdx )
				{
				case (int)GlobalVar.PITCHER_FORM.OVER:			
					tmpPos.y = -4;
					break;
				case (int)GlobalVar.PITCHER_FORM.SIDE:
				case (int)GlobalVar.PITCHER_FORM.UNDER:			
					tmpPos.y = -2;
					break;
				}
				break;
			case Constants.BALLTYPE_UPSHOOT:	
				tmpPos.y = 0f;
				break;
			}
			tmpPos.x = Random.Range(-11, 11);
			tmpPos.z = 0;
			GlobalVar.selectionBall.localPosition = tmpPos;
			break;
		}
	}
	
	void GestureControl_setFinalCourseByControl()
	{
		switch( GlobalVar.control_Status )
		{
		case Constants.STATE_CG_WEAK:
			break;
		case Constants.STATE_CG_OVER:
			break;
		case Constants.STATE_CG_BAD:
			break;
		case Constants.STATE_CG_PERFECT:
			break;
		case Constants.STATE_CG_NORMAL:
			break;
		}
	}
	
//Get
	int GestureControl_getMatchedBtnNumWithDrag( Vector3 pos )
	{
		//Rect Checking
		for( int i=0 ; i<5 ; i++ )
		{
			if( !gBtn[i].bGestureBtnOn )
			{
				if( pos.x>=gBtn[i].gestureObj.transform.localPosition.x-gestureBtnScale && pos.x<=gBtn[i].gestureObj.transform.localPosition.x+gestureBtnScale &&
					pos.y>=gBtn[i].gestureObj.transform.localPosition.y-gestureBtnScale && pos.y<=gBtn[i].gestureObj.transform.localPosition.y+gestureBtnScale )
				{
					return i;
				}
			}
		}
		
		float distX=0.0f;
		float distY=0.0f;
		float sqrDistX=0.0f;
		float sqrDistY=0.0f;
		float absPx = GlobalVar.utilMgr.ABS(pos.x);
		float absPy = GlobalVar.utilMgr.ABS(pos.y);
		float R = GlobalVar.utilMgr.SQRT( gestureBtnScale );
		
		Debug.Log("gestureBtnScale = "+gestureBtnScale);
		
		Debug.Log(pos+"/"+gBtn[0].gestureObj.localPosition);
		
		for( int i=0 ; i<5 ; i++ )
		{
			Debug.Log("@@@@@@@@  "+i+"st..btnOn = "+gBtn[i].bGestureBtnOn);
			if( !gBtn[i].bGestureBtnOn )
			{
				distX = GlobalVar.utilMgr.ABS(gBtn[i].gestureObj.transform.localPosition.x) - absPx;
				distY = GlobalVar.utilMgr.ABS(gBtn[i].gestureObj.transform.localPosition.y) - absPy;
				sqrDistX = GlobalVar.utilMgr.SQRT( distX );
				sqrDistY = GlobalVar.utilMgr.SQRT( distY );
					
				if( sqrDistX+sqrDistY <= R )
				{
					Debug.Log(distX+"/"+distY+"///"+sqrDistX+"/"+sqrDistY+"///"+R);
					Debug.Log("@@@@@@@@  "+i+"st..btnOn = "+gBtn[i].bGestureBtnOn);
					return i;
				}
			}
		}
		
		return -1;
	}

//Check	
	public bool GestureControl_bFinishGesture()
	{
		return bFinishGesture;
		//return false;
	}
	
	public bool GestureControl_bStartGesture()
	{
		return bStartGesture;
	}
	
	public bool GestureControl_bFinishBadActionEff()
	{
		if( ctr_fWarnBadBtnAction )
		{
			GestureControl_setActiveRecursiveControlGauge(false);			
		}
		return ctr_fWarnBadBtnAction;
	}
	
	int GestureControl_bMatchedTotalGesture()
	{
		//Debug.Log("Len:"+gestureData[ GlobalVar.ballTypeSelNum ].Length);
		for( int i=0 ; i<gestureData[ GlobalVar.ballTypeSelNum ].Length ; i++ )
		{
			//Debug.Log(i+"th BtnOn:"+gBtn[ gestureData[ GlobalVar.ballTypeSelNum ][i] ].bGestureBtnOn);
			if( !gBtn[ gestureData[ GlobalVar.ballTypeSelNum ][i] ].bGestureBtnOn )
			{
				//Debug.Log("Miss Gesture !! num = "+gestureData[ GlobalVar.ballTypeSelNum ][i]);
				return gestureData[ GlobalVar.ballTypeSelNum ][i];
			}
		}
		return 100;
	}
	
//Processing
	public void GestureControl_Process()
	{
		GestureControl_ControlGaugeProc();
		GestureControl_gestureActionProc();
	}
	
	void GestureControl_gestureActionProc()
	{
		Vector3 localScale = new Vector3(82, 82, 1);
		Vector3 bigLocalScale = new Vector3(92, 92, 1);
		Vector3 scrnTouchedPos;
		int touchedBtnNum = -1;
		
		
		//Debug.Log("bStartGesture="+bStartGesture+"/GlobalVar.SCREEN_HFSIZE="+GlobalVar.SCREEN_HFSIZE);
		if( bStartGesture )
		{
			//Debug.Log("input:"+Input.mousePosition);
			scrnTouchedPos.x = Input.mousePosition.x * (GlobalVar.SCREEN_HFSIZE.x*2 / Screen.width) - GlobalVar.SCREEN_HFSIZE.x*2;
			scrnTouchedPos.y = Input.mousePosition.y * (GlobalVar.SCREEN_HFSIZE.y*2 / Screen.height);
			scrnTouchedPos.z = 0;
			//Debug.Log("scrn:"+scrnTouchedPos+"/"+gBtn[0].gestureObj.transform.localPosition);
			
			
			if( gesture1stBtnIdx!=-1 )
			{
				//FIRST Button
				touchedBtnNum = gesture1stBtnIdx;
				if( gesture1stBtnIdx==gestureData[ GlobalVar.ballTypeSelNum ][0] )
				{
					GestureControl_setGestureObjectProp( gestureData[ GlobalVar.ballTypeSelNum ][0] );
					if( !gBtn[ gestureData[ GlobalVar.ballTypeSelNum ][1] ].bGestureBtnOn )
					{
						gBtn[ gestureData[ GlobalVar.ballTypeSelNum ][0] ].gesturePrsObj.transform.localScale = bigLocalScale;
					}
					
					//SECOND Button
					touchedBtnNum = GestureControl_getMatchedBtnNumWithDrag( scrnTouchedPos );
					if( gBtn[ gestureData[ GlobalVar.ballTypeSelNum ][0] ].bGestureBtnOn && touchedBtnNum == gestureData[ GlobalVar.ballTypeSelNum ][1])
					{
						//Debug.Log("2nd touchedBtnNum : "+touchedBtnNum+"/"+gBtn[ gestureData[ GlobalVar.ballTypeSelNum ][0] ].bGestureBtnOn+"/"+gBtn[ gestureData[ GlobalVar.ballTypeSelNum ][1] ].bGestureBtnOn);
						GestureControl_setGestureObjectProp( gestureData[ GlobalVar.ballTypeSelNum ][1] );
						gBtn[ gestureData[ GlobalVar.ballTypeSelNum ][0] ].gesturePrsObj.transform.localScale = localScale;
						gBtn[ gestureData[ GlobalVar.ballTypeSelNum ][1] ].gesturePrsObj.transform.localScale = bigLocalScale;
					}
					else if( touchedBtnNum!=-1 )
					{
						if( touchedBtnNum != gestureData[ GlobalVar.ballTypeSelNum ][1] && !gBtn[ gestureData[ GlobalVar.ballTypeSelNum ][1] ].bGestureBtnOn )
						{
							gBtn[ gestureData[ GlobalVar.ballTypeSelNum ][0] ].gesturePrsObj.transform.localScale = localScale;
							//BAD!
							ctrGgBadBtnIdx = touchedBtnNum;
							GestureControl_setControlGaugeStatus( Constants.STATE_CG_BAD );
						}
					}
					
					//THIRD Button
					touchedBtnNum = GestureControl_getMatchedBtnNumWithDrag( scrnTouchedPos );
					if( gBtn[ gestureData[ GlobalVar.ballTypeSelNum ][0] ].bGestureBtnOn && gBtn[ gestureData[ GlobalVar.ballTypeSelNum ][1] ].bGestureBtnOn && touchedBtnNum==gestureData[ GlobalVar.ballTypeSelNum ][2])
					{
							GestureControl_setGestureObjectProp( gestureData[ GlobalVar.ballTypeSelNum ][2] );
							gBtn[ gestureData[ GlobalVar.ballTypeSelNum ][1] ].gesturePrsObj.transform.localScale = localScale;
							gBtn[ gestureData[ GlobalVar.ballTypeSelNum ][2] ].gesturePrsObj.transform.localScale = bigLocalScale;
					}
					else if( touchedBtnNum!=-1 )
					{
						if( touchedBtnNum != gestureData[ GlobalVar.ballTypeSelNum ][2] && !gBtn[ gestureData[ GlobalVar.ballTypeSelNum ][2] ].bGestureBtnOn )
						{
							gBtn[ gestureData[ GlobalVar.ballTypeSelNum ][1] ].gesturePrsObj.transform.localScale = localScale;
							//BAD!
							ctrGgBadBtnIdx = touchedBtnNum;
							GestureControl_setControlGaugeStatus( Constants.STATE_CG_BAD );
						}
					}
					
					if( gestureData[ GlobalVar.ballTypeSelNum ].Length>3 )
					{
						//OVER THIRD Button
						touchedBtnNum = GestureControl_getMatchedBtnNumWithDrag( scrnTouchedPos );
						if( gBtn[gestureData[ GlobalVar.ballTypeSelNum ][0] ].bGestureBtnOn && 
							gBtn[gestureData[ GlobalVar.ballTypeSelNum ][1] ].bGestureBtnOn && 
							gBtn[gestureData[ GlobalVar.ballTypeSelNum ][2] ].bGestureBtnOn &&
							touchedBtnNum==gestureData[ GlobalVar.ballTypeSelNum ][3]
							)
						{
							GestureControl_setGestureObjectProp( gestureData[ GlobalVar.ballTypeSelNum ][3] );
							gBtn[ gestureData[ GlobalVar.ballTypeSelNum ][2] ].gesturePrsObj.transform.localScale = localScale;
							gBtn[ gestureData[ GlobalVar.ballTypeSelNum ][3] ].gesturePrsObj.transform.localScale = bigLocalScale;
						}
						else if( touchedBtnNum!=-1 )
						{
							if( touchedBtnNum != gestureData[ GlobalVar.ballTypeSelNum ][3] && !gBtn[ gestureData[ GlobalVar.ballTypeSelNum ][3] ].bGestureBtnOn )
							{
								gBtn[ gestureData[ GlobalVar.ballTypeSelNum ][2] ].gesturePrsObj.transform.localScale = localScale;
								//BAD!
								ctrGgBadBtnIdx = touchedBtnNum;
								GestureControl_setControlGaugeStatus( Constants.STATE_CG_BAD );
							}
						}
					}
				}
			}
		}
	}
	
	void GestureControl_ControlGaugeSideBallProc( int idx, float angle )
	{
		Vector3 rotPos = new Vector3(0,0,0);
		
		rotPos = ctrGauge[idx].transform.localPosition;
		rotPos.x = ctrGgR * Mathf.Cos(angle);
		rotPos.y = ctrGgR * Mathf.Sin(angle);
		ctrGauge[idx].transform.localPosition = rotPos;
	}
	
	void GestureControl_ControlGaugeProc()
	{
		Vector3 rotPos = new Vector3(0,0,0);
		Vector3 rotBgPos = new Vector3(0,0,0);
		int matchedBtnNum;
		
		//Debug.Log(bStartCtrGauge+"/"+ctrStatus+"/"+ctr_fWarnBadAction);
		
		if( bStartCtrGauge )
		{
			switch( GlobalVar.control_Status )
			{
			case Constants.STATE_CG_PROC:
				if( !ctrbEndRot )
				{
					GestureControl_ControlGaugeSideBallProc( 6, ctrGgRotRAngle );
					ctrGgRotRAngle += 0.35f;
					//Debug.Log(ctrGgRotRAngle);
					GestureControl_ControlGaugeSideBallProc( 7, ctrGgRotLAngle );
					ctrGgRotLAngle += 0.35f;
					
					rotPos.x = ctrGgR*2.1f;
					rotPos.y = ctrGgR*2.1f;
					rotPos.z = 1;
					ctrGauge[0].transform.localScale = rotPos;
					ctrGgR-=4f;
					//Debug.Log(ctrGgR);
					if( ctrGgR<=-7 )
					{
						ctrGgR=-7;
						
						matchedBtnNum = GestureControl_bMatchedTotalGesture();
						if( matchedBtnNum==100 )
						{
							GestureControl_setControlGaugeStatus( Constants.STATE_CG_OVER );
						}
						else
						{
							ctrGgBadBtnIdx = matchedBtnNum;
							GestureControl_setControlGaugeStatus( Constants.STATE_CG_BAD );
						}
						//Debug.Log("## Proc Decided Over or Bad");
					}
					
					rotBgPos.z = ctrRotCircle;			
					ctrGauge[0].transform.localEulerAngles = rotBgPos;				
					ctrRotCircle+=5.625f; // Total 281.5 degree 
					//Debug.Log(ctrRotCircle);
					if( ctrGgR==0 )
					{
						ctrRotCircle=360;
						ctrbEndRot = true;
					}
				}
				else
				{
					rotPos = ctrGauge[6].transform.localPosition;
					rotPos.x = ctrGgR;
					ctrGauge[6].transform.localPosition = rotPos;
					rotPos = ctrGauge[7].transform.localPosition;
					rotPos.x = ctrGgR*(-1);
					ctrGauge[7].transform.localPosition = rotPos;
					rotPos.x = ctrGgR*2.1f;
					rotPos.y = ctrGgR*2.1f;
					rotPos.z = 1;
					ctrGauge[0].transform.localScale = rotPos;
					
					ctrGgR+=4f;
					if( ctrGgR>=101 )ctrGgR=101;
				}
				
				break;
			}
		}
		else
		{
			switch( GlobalVar.control_Status )
			{
			case Constants.STATE_CG_WEAK:
				GestureControl_ControlGaugeSlidingText(5);
				if( gArrSpr.alpha>=0 )
				{
					GestureControl_gBtnAlphaProc();					
					gArrSpr.alpha-=0.07f;
				}
				
				ctr_BadActionCnt++;				
				if( ctr_BadActionCnt==20) bFinishGesture = true;
				if( ctr_BadActionCnt>=40 ) ctr_fWarnBadBtnAction = true;
				break;
			case Constants.STATE_CG_OVER:
				rotPos = ctrGauge[10].transform.localPosition;
				rotPos.x = ctrGgR;
				ctrGauge[10].transform.localPosition = rotPos;
				rotPos = ctrGauge[11].transform.localPosition;
				rotPos.x = ctrGgR*(-1);
				ctrGauge[11].transform.localPosition = rotPos;
				
				rotPos.x = ctrGgR*2.1f;
				rotPos.y = ctrGgR*2.1f;
				rotPos.z = 1;
				ctrGauge[0].transform.localScale = rotPos;
				ctrGgR+=3f;
				if( ctrGgR>=101 )ctrGgR=101;
				
				GestureControl_ControlGaugeSlidingText(4);
				if( gArrSpr.alpha>=0 )
				{
					GestureControl_gBtnAlphaProc();					
					gArrSpr.alpha-=0.07f;
				}
				
				ctr_BadActionCnt++;				
				if( ctr_BadActionCnt==20)bFinishGesture = true;
				if( ctr_BadActionCnt>=40 )
				{
					ctr_fWarnBadBtnAction = true;
				}
				break;
			case Constants.STATE_CG_BAD:
				//gesture Button Part
				if( !ctr_fWarnBadAction )
				{
					//Off
					if( ctr_BadActionCnt>=0 && ctr_BadActionCnt<5 )
					{
						gBtn_Bad.SetActiveRecursively(false);
					}
					//On
					else if( ctr_BadActionCnt>=5 && ctr_BadActionCnt<10 )
					{
						gBtn_Bad.SetActiveRecursively(true);
					}
					ctr_BadActionCnt++;
					if( ctr_BadActionCnt>=10 )
					{
						ctr_BadActionCnt=0;
						ctr_warnCnt++;
						if( ctr_warnCnt==1 )bFinishGesture = true;
						if( ctr_warnCnt>=2 )
						{
							ctr_fWarnBadAction = true;
							ctr_BadActionCnt = 0;
							ctr_warnCnt = 0;
						}
					}
				}
				else
				{
					if( gBtn_BadSpr.alpha >= 0 )
					{
						GestureControl_gBtnAlphaProc();
						gBtn_BadSpr.alpha-=0.07f;
						gArrSpr.alpha-=0.07f;
					}
				}
				//ControlGauge part
				ctr_BadBtnActionCnt++;
				if( ctr_BadBtnActionCnt>=14 )
				{
					ctr_BadBtnActionCnt = 0;
					ctr_warnBtnCnt++;
					if( ctr_warnBtnCnt>2 )
					{
						ctr_fWarnBadBtnAction = true;
					}
				}
				GestureControl_ControlGaugeSlidingText(3);
				break;
			case Constants.STATE_CG_PERFECT:
				if( ctrEffPerfectAnim.mIndex==6 )
				{
					ctrEffPerfect[0].SetActiveRecursively(false);
				}
				ctr_BadBtnActionCnt++;
				
				ctrEffPerfectTxtSpr.alpha += 0.05f;
				if( ctrEffPerfect[1].transform.localPosition.x>=0 )
				{
					ctrGauge_SlidePosX -= 0.8f;
					rotPos = ctrEffPerfect[1].transform.localPosition;
					rotPos.x += ctrGauge_SlidePosX;
					if( rotPos.x<=0 ) rotPos.x=0;
					ctrEffPerfect[1].transform.localPosition = rotPos;
				}
				if( ctr_BadBtnActionCnt==20) bFinishGesture = true;				
				if( ctr_BadBtnActionCnt>=40)
				{
					ctr_fWarnBadBtnAction = true;
				}
				if( gArrSpr.alpha>=0 )
				{
					GestureControl_gBtnAlphaProc();					
					gArrSpr.alpha-=0.07f;
				}
				break;				
			case Constants.STATE_CG_NORMAL:
				ctr_BadBtnActionCnt++;	
				if( ctr_BadBtnActionCnt==20) bFinishGesture = true;				
				if( ctr_BadBtnActionCnt>=40)
				{
					ctr_fWarnBadBtnAction = true;
				}
				if( gArrSpr.alpha>=0 )
				{
					GestureControl_gBtnAlphaProc();					
					gArrSpr.alpha-=0.07f;
				}
				break;
			}
		}
	}
	
	void GestureControl_ControlGaugeSlidingText( int idx )
	{		
		Vector3 rotPos = new Vector3(0,0,0);
		
		ctrGaugeSpr[idx].alpha += 0.05f;
		if( ctrGauge[idx].transform.localPosition.x>=0 )
		{
			ctrGauge_SlidePosX -= 0.5f;
			rotPos = ctrGauge[idx].transform.localPosition;
			rotPos.x += ctrGauge_SlidePosX;
			if( rotPos.x<=0 ) rotPos.x=0;
			ctrGauge[idx].transform.localPosition = rotPos;
		}
	}
	
	void GestureControl_gBtnAlphaProc()
	{
		for( int i=0 ; i<gBtn.Length ; i++ )
		{
			gBtn[i].gestureSpr.alpha -= 0.07f;
			gBtn[i].gesturePrsSpr.alpha -= 0.07f;
		}
	}
	
	public void GestureControl_TouchUp()
	{
		int matchedBtnNum;
		if( Input.GetMouseButtonUp(0) )
		{
			
			if( gBtn[ gestureData[ GlobalVar.ballTypeSelNum ][0] ].bGestureBtnOn )
			{
				//if( ctrRotCircle<360 )
				if( ctrGgR>-7 )
				{
					if( GlobalVar.control_Status!=Constants.STATE_CG_OVER && GlobalVar.control_Status!=Constants.STATE_CG_BAD )
					{
						//Weak
						if( ctrGgR<=101 && ctrGgR>=60 )
						{
							matchedBtnNum = GestureControl_bMatchedTotalGesture();
							if( matchedBtnNum==100 )
							{
								GestureControl_setControlGaugeStatus( Constants.STATE_CG_WEAK );
							}
							else
							{
								ctrGgBadBtnIdx = matchedBtnNum;
								GestureControl_setControlGaugeStatus( Constants.STATE_CG_BAD );
							}
						}
						//Perfect
						else if( ctrGgR<=13 && ctrGgR>=-3 )
						{
							matchedBtnNum = GestureControl_bMatchedTotalGesture();
							if( matchedBtnNum==100 )
							{
								Debug.Log(" PERFECT PITCHING !");
								GestureControl_setControlGaugeStatus( Constants.STATE_CG_PERFECT);
							}
							else
							{
								ctrGgBadBtnIdx = matchedBtnNum;
								GestureControl_setControlGaugeStatus( Constants.STATE_CG_BAD );
							}
						}
						//Normal
						else
						{
							matchedBtnNum = GestureControl_bMatchedTotalGesture();
							if( matchedBtnNum==100 )
							{
								Debug.Log(" NORMAL PITCHING !");
								GestureControl_setControlGaugeStatus( Constants.STATE_CG_NORMAL);
							}
							else
							{
								ctrGgBadBtnIdx = matchedBtnNum;
								GestureControl_setControlGaugeStatus( Constants.STATE_CG_BAD );
								Debug.Log(" BAD !");
							}
						}
					}
				}
				else
				{
					if( GlobalVar.control_Status!=Constants.STATE_CG_OVER && GlobalVar.control_Status!=Constants.STATE_CG_BAD )
					{
						matchedBtnNum = GestureControl_bMatchedTotalGesture();
						if( matchedBtnNum==100 )
						{
							GestureControl_setControlGaugeStatus( Constants.STATE_CG_OVER );
						}
						else
						{
							ctrGgBadBtnIdx = matchedBtnNum;
							GestureControl_setControlGaugeStatus( Constants.STATE_CG_BAD );
						}
						Debug.Log("### Touch up decided Over or Bad");
					}
				}
			}
		}
	}
	
	
	
	public void GestureControl_TouchDown()
	{
		Vector3 scrnTouchedPos;
		
		if( Input.GetMouseButtonDown(0) )
		{
			scrnTouchedPos.x = Input.mousePosition.x * (GlobalVar.SCREEN_HFSIZE.x*2 / Screen.width) - GlobalVar.SCREEN_HFSIZE.x*2;
			scrnTouchedPos.y = Input.mousePosition.y * (GlobalVar.SCREEN_HFSIZE.y*2 / Screen.height);
			scrnTouchedPos.z = 0;
			gesture1stBtnIdx = GestureControl_getMatchedBtnNumWithDrag( scrnTouchedPos );
			
			bStartGesture = true;
			if( gesture1stBtnIdx!=-1 )
			{
				//TurnOn Gesture Control gauge
				GestureControl_setActiveRecursiveControls(true);
				if( gesture1stBtnIdx==gestureData[ GlobalVar.ballTypeSelNum ][0] )
				{
					bStartCtrGauge = true;
					//Debug.Log("good "+bStartCtrGauge);	
				}
				else
				{
					//BAD!
					ctrGgBadBtnIdx = gesture1stBtnIdx;				
					GestureControl_ControlGaugeSideBallProc( 6, ctrGgRotLAngle );
					GestureControl_ControlGaugeSideBallProc( 7, ctrGgRotRAngle );
					GestureControl_setControlGaugeStatus( Constants.STATE_CG_BAD );
					//Debug.Log("BAD "+bStartCtrGauge);	
				}
			}
		}
	}
}
