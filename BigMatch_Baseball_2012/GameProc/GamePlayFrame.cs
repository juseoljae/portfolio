using UnityEngine;
using System.Collections;

public class GamePlayFrame : MonoBehaviour {
	
	//Transform
	Transform skipBoxObj;
	GameObject[] skipResultObj;
	
	//BallType
	bool bTouchON;
	
	//ClassReference
	//AnimationTootaMgr aniTMgr;
	GestureControl gesture;
	BallTypeSelect btst;
	BallCourseSelect bCrsSel;
	PitchingBallMgr pBallMgr;	
	HitEngineMgr hitNgnMgr;
	//hoon
	ballForce ballforce;
	Vector3 flyBallPorp;
	
	
	int skipCount;
	bool bGoingSkip;
	
	
	//Temp
	public bool bHost;
	
	// Use this for initialization
	void Awake(){		
		Debug.Log("GamePlayFrame.Awake()");
		
		GlobalVar.playGroundObj = transform;
		GlobalVar.playUIObj = transform.parent.FindChild("PlayUI");
		GlobalVar.homeBaseZoneBox = GlobalVar.playGroundObj.FindChild("HomeBaseZoneBox");
		GlobalVar.selectionBall = GlobalVar.homeBaseZoneBox.FindChild("SelectBallCourse").FindChild("SelectionBall");
		
		GlobalVar.cam = new Transform[2];
		for( int i=0 ; i<GlobalVar.cam.Length ; i++ )
		{
			GlobalVar.cam[i] = GlobalVar.playGroundObj.FindChild("Cam#"+(i+1));
		}
		
		GlobalVar.joyStickObj = GlobalVar.playUIObj.FindChild("UI_Cam").FindChild("Panel").FindChild("Anchor_BtmLeft").FindChild("JoyStick");
		GlobalVar.gestureObj = GlobalVar.playUIObj.FindChild("UI_Cam").FindChild("Panel").FindChild("Anchor_BtmRight").FindChild("Gesture");
		skipBoxObj = GlobalVar.playUIObj.FindChild("UI_Cam").FindChild("Panel").FindChild("Anchor_Center").FindChild("SkipBox");
		
		GlobalVar.pBallObj = GlobalVar.playGroundObj.FindChild("pBall");//Under pitcher
		GlobalVar.strikeZoneObj = GlobalVar.playGroundObj.FindChild("HomeBaseZoneBox").FindChild("StrikeZone");
		
				
		//Temp
		GlobalVar.utilMgr = new UtilMgr();
		GlobalVar.myTeam = new MyTeamMgr();
		GlobalVar.myTeam.MyTeamMgr_LoadPlayers();
		
		skipResultObj = new GameObject[11];		
		for( int i=0 ; i<skipResultObj.Length ; i++ )
		{
			skipResultObj[i] = (GameObject)Instantiate(Resources.Load("Prefab/S_GameProc/SkipBox/skipResult_"+i));
			skipResultObj[i] = GlobalVar.utilMgr.createGameObjectAsLocal( skipResultObj[i], skipBoxObj, "SR_"+i, false );
		}
		
		//Debug.Log(GlobalVar.myTeam);
		if( GlobalVar.myTeam!=null )
		{
			GlobalVar.myTeam.MyTeamMgr_setStartingLineUp();
		}
		else
		{
			Debug.Log("Err..Can't Load MyTeamMgr_setStartingLineUp()");
			return;
		}
		
		skipCount = 0;
		bGoingSkip = false;
		//Debug.Log("init......bGoingSkip="+bGoingSkip);
		
		GamePlayFrame_initialize();
		GamePlayFrame_setBallReleasePos();
		
		//Pitching Ball Manager must set after load player
		pBallMgr = transform.GetComponent<PitchingBallMgr>();
		hitNgnMgr = transform.GetComponent<HitEngineMgr>();
		
		//constructing ref Class
		GlobalVar.aniTMgr = transform.GetComponent<AnimationTootaMgr>();
		
		btst = GlobalVar.playUIObj.FindChild("UI_Cam").FindChild("Panel").FindChild("Anchor_TopLeft").FindChild("BallTypeSel").GetComponent<BallTypeSelect>();
		bCrsSel = GlobalVar.joyStickObj.GetComponent<BallCourseSelect>();
		gesture = GlobalVar.gestureObj.GetComponent<GestureControl>();
		gesture.GestureControl_setGameView( bHost );
		
		//hoon
		GlobalVar.ball = GameObject.Find("Dball").transform;
		GlobalVar.defenceCam = GlobalVar.playGroundObj.FindChild("Cam#3").FindChild("Camera").camera.transform;//GameObject.Find("Cam#3").camera.transform;
		ballforce = GlobalVar.ball.GetComponent<ballForce>();
		GlobalVar.ball.gameObject.SetActiveRecursively(false);
		GlobalVar.defenceCam.gameObject.SetActiveRecursively(false);
		flyBallPorp = new Vector3(0,0,0);
		
		GlobalVar.GAMEPROC_STATE = Constants.STATE_GAMEPROC_PITCHER_SELBT;
	}
	
	void Start () {
		GamePlayFrame_setAnchor();
	}
	// Update is called once per frame
	void Update () {
		GamePlayFrame_Process();
		GamePlayFrame_TouchDown();
		GamePlayFrame_TouchUp();
		
	}
	
	void FixedUpdate()
	{
		GamePlayFrame_FixedProcess();
	}
	
//Initialize
	void GamePlayFrame_initialize()
	{
		Debug.Log("GamePlayFrame_initialize()");
		//Temp
		GlobalVar.bHome = bHost;
		
		if( bHost )
		{
			GlobalVar.myTeam.bBottom = 1;
		}
		else
		{
			GlobalVar.myTeam.bBottom = 0;
		}
		//Debug.Log("bHost="+bHost+"/ bBottom="+GlobalVar.myTeam.bBottom);
		
		GlobalVar.bFinishBallDir = false;
		GlobalVar.alphaDep = 1;
		GlobalVar.bStartPitWindup = false;
		
		
		GlobalVar.selectionBall.gameObject.SetActiveRecursively(false);
		GamePlayFrame_setHomeAway();
	}
	
	
//Set
	void GamePlayFrame_setAnchor()
	{
		Transform anchor;
		anchor = GlobalVar.playUIObj.FindChild("UI_Cam").FindChild("Panel").FindChild("Anchor_BtmLeft");
		GlobalVar.SCREEN_HFSIZE = new Vector2(GlobalVar.utilMgr.ABS(anchor.localPosition.x), GlobalVar.utilMgr.ABS(anchor.localPosition.y));
	}
	
	//Temp	
	void GamePlayFrame_setHomeAway()
	{
		//Home
		if( bHost )
		{
			GlobalVar.cam[0].gameObject.SetActiveRecursively(false);
			GlobalVar.cam[1].gameObject.SetActiveRecursively(true);
		}
		//Away
		else
		{
			GlobalVar.cam[0].gameObject.SetActiveRecursively(true);
			GlobalVar.cam[1].gameObject.SetActiveRecursively(false);
		}
		
	}
	
	void GamePlayFrame_setBallReleasePos()
	{
		Vector3 rbPos = new Vector3(0,0,0);
		
		//needs to saperate hand direction
		switch( GlobalVar.myTeam.P_starting.formIdx )
		{
		case (int)GlobalVar.PITCHER_FORM.OVER:
			rbPos.x = -19.82211f;
			rbPos.y = 67.86341f;
			rbPos.z = 682.0847f;
			GlobalVar.pBallObj.position = rbPos;
			break;
		case (int)GlobalVar.PITCHER_FORM.SIDE:
			rbPos.x = -28.58708f;
			rbPos.y = 43.21957f;
			rbPos.z = 672.9193f;
			GlobalVar.pBallObj.position = rbPos;
			break;
		case (int)GlobalVar.PITCHER_FORM.UNDER:
			rbPos.x = -36.66508f;
			rbPos.y = 20.57541f;
			rbPos.z = 681.3121f;
			GlobalVar.pBallObj.position = rbPos;
			break;
		}
		GlobalVar.dist_RP = GlobalVar.pBallObj.position.z;///39.370079f;
		//Debug.Log("distance of Released point = "+GlobalVar.dist_i2mRP);
	}
	
	
//Process	
	void GamePlayFrame_Process()
	{
		//Debug.Log("gameState = "+GlobalVar.GAMEPROC_STATE);
		
		switch( GlobalVar.GAMEPROC_STATE )
		{
		case Constants.STATE_GAMEPROC_HITTER_COMETOBASE:
			break;
		case Constants.STATE_GAMEPROC_PITCHER_SELBT_FADEIN:
			break;
		case Constants.STATE_GAMEPROC_PITCHER_SELBT:
			btst.BallTypeSelect_Process( bTouchON );
			//when finish Ball Type selecting
			if( btst.BallTypeSelect_bFinishSelBT() )
			{
				btst.BallTypeSelect_setSZArrPickupIdx();
				//Create Ball Default Curve Data of Ball Type
				//pBallMgr.PitchingBallMgr_createBallData( GlobalVar.ballTypeSelNum );
				pBallMgr.PitchingBallMgr_createVesBallData(GlobalVar.ballTypeSelNum);
				pBallMgr.PitchingBallMgr_firstMakeBallDataForArr();
				bCrsSel.BallCourseSelect_setBrkArrActiveRecursive();
				bCrsSel.BallCourseSelect_setBreakingArr();//Create Distance and Angle of Breaking Arrow of each BallType
				
				GlobalVar.joyStickObj.gameObject.SetActiveRecursively(true);
				bCrsSel.BallCourseSelect_setActiveRecursive(false);
				GamePlayFrame_setAnchor();
				GlobalVar.GAMEPROC_STATE = Constants.STATE_GAMEPROC_PITCHER_SELDIR;
			}
			break;
		case Constants.STATE_GAMEPROC_PITCHER_SELBT_FADEOUT:
			break;
		case Constants.STATE_GAMEPROC_PITCHER_SELDIR:
			bCrsSel.BallCourseSelect_Process();
			
			if( GlobalVar.strikeZoneSpr.alpha<=0 )
			{			
				pBallMgr.PitchingBallMgr_setBallTraceActiveReculsive(false);
			}
			if( GlobalVar.bFinishBallDir )
			{
				GlobalVar.bFinishBallDir = false;
				gesture.GestureControl_setActiveRecursiveBtn( true );
				gesture.GestureControl_initGestureArrow();
				GlobalVar.GAMEPROC_STATE = Constants.STATE_GAMEPROC_PITCHER_GESTURE;
			}
			break;
		case Constants.STATE_GAMEPROC_PITCHER_GESTURE:
			bCrsSel.BallCourseSelect_RotationSelectionBall();
			if( GlobalVar.strikeZoneSpr.alpha!=0 )
			{
				bCrsSel.BallCourseSelect_setStrikeZoneAlpha();
			}
			
			if( GlobalVar.strikeZoneSpr.alpha<=0 || gesture.GestureControl_bStartGesture() )
			{
				GlobalVar.selectionBall.gameObject.SetActiveRecursively(false);
				pBallMgr.PitchingBallMgr_setBallTraceActiveReculsive(false);
				bCrsSel.BallCourseSelect_setHideSelectionBallArr();
			}
			if( gesture.GestureControl_bFinishGesture() )
			{
				btst.BallTypeSelect_setSZArrPickupIdx();
				//Create Ball Default Curve Data of Ball Type
				pBallMgr.PitchingBallMgr_createBallData( GlobalVar.ballTypeSelNum );
				
				pBallMgr.PitchingBallMgr_setCatchingPoint();
				GlobalVar.GAMEPROC_STATE = Constants.STATE_GAMEPROC_PITCHER_READY;
			}
			break;
		case Constants.STATE_GAMEPROC_PITCHER_READY:
			Vector3 tmpBallPos = new Vector3(0,0,0);
			//Here! must make finally ball curve data
			pBallMgr.PitchingBallMgr_finallyMakeBallData();
			GlobalVar.strikeZoneObj.gameObject.SetActiveRecursively(false);
			GlobalVar.txtHitTimingObj.gameObject.SetActiveRecursively(false);
			//pBallMgr.PitchingBallMgr_setBallTraceActiveReculsive(false);
			
			tmpBallPos.x = pBallMgr.bezierResult[ 0 ].x;
			tmpBallPos.y = pBallMgr.bezierResult[ 0 ].y;
			tmpBallPos.z = pBallMgr.bezierResult[ 0 ].z;			
			GlobalVar.pBallObj.position = tmpBallPos;
			
			GlobalVar.GAMEPROC_STATE = Constants.STATE_GAMEPROC_PITCHER_WINDUP;
			break;
		case Constants.STATE_GAMEPROC_RETURN_SKIP:
			
			break;
		}
		
		if( GlobalVar.GAMEPROC_STATE>=Constants.STATE_GAMEPROC_PITCHER_GESTURE && GlobalVar.GAMEPROC_STATE<=Constants.STATE_GAMEPROC_PITCHER_WINDUP )
		{
			if(!gesture.GestureControl_bFinishBadActionEff() )
			{
				gesture.GestureControl_Process();
			}
		}
		
		
	}
	
	
	void GamePlayFrame_FixedProcess()
	{
		//Animation
		GlobalVar.aniTMgr.AnimationTootaMgr_process();
		
		//Debug.Log("GlobalVar.GAMEPROC_STATE="+GlobalVar.GAMEPROC_STATE+" / GlobalVar.bThrowToPit="+GlobalVar.bThrowToPit);
		
		switch( GlobalVar.GAMEPROC_STATE )
		{
		case Constants.STATE_GAMEPROC_PITCHER_WINDUP:
			//Debug.Log("GamePlayFrame_FixedProcess().pitcherWindup.."+GlobalVar.bStartPitWindup);
			if( GlobalVar.bStartPitWindup )
			{
				GlobalVar.statusAnim_HITTER = Constants.ANISTATUS_HITTER_TAKEBACK;
				GlobalVar.aniTMgr.AnimationTootaMgr_setHitterAnimByStatus();
				GlobalVar.bStartPitWindup = false;
				
			}
			break;
		case Constants.STATE_GAMEPROC_PITCHER_THROW:
			pBallMgr.PitchingBallMgr_pitching();
			break;
		case Constants.STATE_GAMEPROC_PITCHER_FINISH:
			GlobalVar.aniTMgr.AnimationTootaMgr_initialize();
			gesture.GestureControl_initialize();
			gesture.GestureControl_initObject();
			pBallMgr.PitchingBallMgr_initialize();
			GlobalVar.aniTMgr.AnimationTootaMgr_initialize();
			bCrsSel.BallCourseSelect_initJoyStick();
			GamePlayFrame_initialize();
			bGoingSkip = true;
			//Debug.Log("finish......bGoingSkip="+bGoingSkip);
			GlobalVar.GAMEPROC_STATE = Constants.STATE_GAMEPROC_RETURN_SKIP;// Constants.STATE_GAMEPROC_HITTER_COMETOBASE;
			break;
		case Constants.STATE_GAMEPROC_PITCHER_RETURN:
			//Debug.Log("Constants.STATE_GAMEPROC_PITCHER_RETURN:GlobalVar.bThrowToPit="+GlobalVar.bThrowToPit);
			if( GlobalVar.bThrowToPit )
			{
				pBallMgr.PitchingBallMgr_renderBallBackToPit();
			}
			break;
		case Constants.STATE_GAMEPROC_HITTER_HITBALL:
			
			break;
		}
		if( bGoingSkip )
		{
			GamePlayFrame_renderSkipResult();
		}
	}

//Render
	void GamePlayFrame_renderSkipResult()
	{
		//Debug.Log("STATE_GAMEPROC_RETURN_SKIP skipCount="+skipCount);
		switch( skipCount )
		{
		case 0:
			skipResultObj[0].SetActiveRecursively(true);
			break;
		case 1:
			skipResultObj[0].SetActiveRecursively(false);
			skipResultObj[1].SetActiveRecursively(true);
			
			btst.BallTypeSelect_initialize();
			btst.BallTypeSelect_initObject();
			GlobalVar.aniTMgr.AnimationTootaMgr_initAnimState();
			GlobalVar.aniTMgr.AnimationTootaMgr_setPitcherAnimByStatus();
			//GlobalVar.aniTMgr.AnimationTootaMgr_playAnimAtStart();
			//pBallMgr.PitchingBallMgr_setBallTraceActiveReculsive(true);
			GlobalVar.GAMEPROC_STATE = Constants.STATE_GAMEPROC_PITCHER_SELBT;
			break;
		case 2:
			skipResultObj[1].SetActiveRecursively(false);
			skipResultObj[2].SetActiveRecursively(true);
			break;
		case 3:
			skipResultObj[2].SetActiveRecursively(false);
			skipResultObj[3].SetActiveRecursively(true);
			break;
		case 4:
			skipResultObj[3].SetActiveRecursively(false);
			skipResultObj[4].SetActiveRecursively(true);
			break;
		case 5:
			skipResultObj[4].SetActiveRecursively(false);
			skipResultObj[5].SetActiveRecursively(true);
			break;
		case 6:
			skipResultObj[5].SetActiveRecursively(false);
			skipResultObj[6].SetActiveRecursively(true);
			break;
		case 7:
			skipResultObj[6].SetActiveRecursively(false);
			skipResultObj[7].SetActiveRecursively(true);
			break;
		case 8:
			skipResultObj[7].SetActiveRecursively(false);
			skipResultObj[8].SetActiveRecursively(true);
			break;
		case 9:
			skipResultObj[8].SetActiveRecursively(false);
			skipResultObj[6].SetActiveRecursively(true);
			break;
		case 10:
			skipResultObj[6].SetActiveRecursively(false);
			skipResultObj[9].SetActiveRecursively(true);
			break;
		case 11:
			skipResultObj[9].SetActiveRecursively(false);
			skipResultObj[10].SetActiveRecursively(true);
			break;
		case 12:
			skipResultObj[10].SetActiveRecursively(false);
			skipResultObj[1].SetActiveRecursively(true);
			break;
		case 13:
			skipResultObj[1].SetActiveRecursively(false);
			skipResultObj[0].SetActiveRecursively(true);
			break;
		case 14:
			for( int i=0 ; i<skipResultObj.Length ; i++ )
			{
				skipResultObj[i].SetActiveRecursively(false);
			}
			skipCount = 0;
			bGoingSkip = false;
			pBallMgr.PitchingBallMgr_setBallTraceActiveReculsive(true);
			//Debug.Log("end......bGoingSkip="+bGoingSkip);
			break;
		}
		skipCount++;
	}
	
//Touch Up, Down
	void GamePlayFrame_TouchUp()
	{
		if( Input.GetMouseButtonUp(0) )
		{
			switch( GlobalVar.GAMEPROC_STATE )
			{
			case Constants.STATE_GAMEPROC_PITCHER_SELBT:
				if( bTouchON )
				{
					bTouchON = false;
					btst.BallTypeSelect_TouchUp();
				}
				break;
			case Constants.STATE_GAMEPROC_PITCHER_SELDIR:
			case Constants.STATE_GAMEPROC_PITCHER_GESTURE:
				gesture.GestureControl_TouchUp();
				break;
			}
		}
	}
	
	void GamePlayFrame_TouchDown()
	{
		
		
		if( bGoingSkip ) return;
		if( Input.GetMouseButtonDown(0) )
		{
			switch( GlobalVar.GAMEPROC_STATE )
			{
			case Constants.STATE_GAMEPROC_PITCHER_SELBT:
				bTouchON = true;    
				btst.BallTypeSelect_TouchDown();
				break;
			case Constants.STATE_GAMEPROC_PITCHER_SELDIR:
				break;
			case Constants.STATE_GAMEPROC_PITCHER_GESTURE:
				gesture.GestureControl_TouchDown();
				break;
			case Constants.STATE_GAMEPROC_PITCHER_THROW:
				hitNgnMgr.HitEngineMgr_swingByTouchDown();
				
				if(!GlobalVar.bHitBall)
				{
					Vector3 ballposition = new Vector3();
					GlobalVar.ball.gameObject.SetActiveRecursively(true);
					GlobalVar.defenceCam.gameObject.SetActiveRecursively(true);
					GlobalVar.bHitBall = true;
					
					int a = Random.Range(0, 2);//
					flyBallPorp.x = Random.Range(5, 45);//angleX
					flyBallPorp.y = Random.Range(-10, 50);//angleY
					flyBallPorp.z = Random.Range(800, 1600);//Power
					if(a == 0)
					{
						flyBallPorp.x = flyBallPorp.x*(-1);
					}
					else
					{
						flyBallPorp.x = flyBallPorp.x;
					}
					ballposition.x = 0;
					ballposition.y = 28;
					ballposition.z = 0;
					ballforce.ballForce_init( ballposition, flyBallPorp, true);
					
					for( int i=0 ; i<GlobalVar.cam.Length ; i++ )
					{
						GlobalVar.cam[i].gameObject.SetActiveRecursively(false);
					}

					GlobalVar.GAMEPROC_STATE = Constants.STATE_GAMEPROC_HITTER_HITBALL;
				}
				break;
			case Constants.STATE_GAMEPROC_HITTER_HITBALL:
				//if( GlobalVar.bHitBall )
				{
					pBallMgr.PitchingBallMgr_setFinishBallCatToPit();
					pBallMgr.PitchingBallMgr_setFinishPitching();
					GlobalVar.ball.gameObject.SetActiveRecursively(false);
					GlobalVar.defenceCam.gameObject.SetActiveRecursively(false);
					//for( int i=0 ; i<GlobalVar.cam.Length ; i++ )
					{
						GlobalVar.cam[0].gameObject.SetActiveRecursively(true);
					}
					GlobalVar.BallPosition_List.Clear();
					GlobalVar.BallPosition_List_Angle.Clear();
					GlobalVar.BallPosition_List = null;
					GlobalVar.BallPosition_List_Angle = null;
					GlobalVar.bHitBall = false;
					GlobalVar.aniTMgr.AnimationTootaMgr_initialize();
					gesture.GestureControl_initialize();
					gesture.GestureControl_initObject();
					pBallMgr.PitchingBallMgr_initialize();
					GlobalVar.aniTMgr.AnimationTootaMgr_initialize();
					bCrsSel.BallCourseSelect_initJoyStick();
					GamePlayFrame_initialize();
					bGoingSkip = true;
				}
				break;
			}
		}
	}
}
