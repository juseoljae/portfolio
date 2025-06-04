//#define VESTIGE_BALL

using UnityEngine;
using System.Collections;

public class PitchingBallMgr : MonoBehaviour {
	
	private Vector3[][] inflectionPnt;//inflection Points Of Whole ballType
	private Vector3[][] P_IfcPnt;//inflection points picked by Pitcher
	private Vector3 prePitchingPnt;//Under(P:-36.66508, 20.57541, 681.3121, R:0, 36.98, 0)
	public Vector3 catchingPnt;
		
	public Vector3[] bezierResult;//default curve as bezier of Ball Type 
	int brThirdPnt;
	int brLast5Pnt;
// VESTIGE_BALL
	private  Vector3[] vesBallCrs; //vestige ball course curve
	private GameObject[] vesBallObj;//balls objects of vestige
	//private int vesStrikeZoneArrIdx;
	
	
	//Ball Property
	private float ballSpeed;	
	private float ballSpeedPerSec;
	private int pingBallFrmCnt;
	int pBallRotRate;
	float pBallFlyTime;//It's time arriving point of strike zone from released point
	bool bStrike;
	
	//Strike zone effect
	//Vector3 ballPosInSZone;
	Vector3 finalCatchPnt;
	int szEffFrmCnt;
	int btBallFrmCnt;
	//UI Change
	Transform strikeZoneEffObj;
	private GameObject[] szEffObj;//szEff_12, szEff_5,6
	bool bStartSzEff;
	Vector3[] resultTracePos;
	
	//Catcher Back to pitcher
	private Vector3[] backToPitcherPos;
	private Vector3[] bIfcPoints;
	private Vector3 btCatPos;
	private Vector3 btPitPos;
	
	//ScoreBoard
	Transform ScoreBoardObj;
	GameObject ScoreBoardBgObj;
	GameObject[] dBallObj;
	GameObject[] dStrikeObj;
	GameObject[] dOutObj;
	bool bRefereeCall;
	
	//Pitching machine
	Transform txtBallSpeedObj;
	UISysFontLabel txtBallSpeed;
	Transform ballTraceOfPitchedObj;
	GameObject[] ballTraceObj;
	UISprite[] ballTraceSpr;
	int ballTraceCnt;
	
	//BadControl Sign
	GameObject signGBObj;
	int badFrmCnt;
	
	// Use this for initialization
	void Start () {
		Debug.Log("PitchingBallMgr.Start() / Pitcher form idx : "+GlobalVar.myTeam.P_starting.formIdx);
		strikeZoneEffObj = GlobalVar.homeBaseZoneBox.FindChild("StrikeZoneEff");
		ScoreBoardObj = GlobalVar.playUIObj.FindChild("UI_Cam").FindChild("Panel").FindChild("Anchor_Top").FindChild("ScoreBoard");
		txtBallSpeedObj = GlobalVar.playUIObj.FindChild("UI_Cam").FindChild("Panel").FindChild("Anchor_TopRight").FindChild("PitchingMachine").FindChild("Txt_BallSpeed");
		ballTraceOfPitchedObj = GlobalVar.homeBaseZoneBox.FindChild("BallTraceOfPitched");
		ballTraceObj = new GameObject[10];
		ballTraceSpr = new UISprite[ballTraceObj.Length];
		for( int i=0 ; i<ballTraceObj.Length ; i++ )
		{
			ballTraceObj[i] = (GameObject)Instantiate(Resources.Load("Prefab/S_GameProc/Etc/BallTrace"));
			ballTraceObj[i].transform.parent = ballTraceOfPitchedObj;
			ballTraceObj[i].name = "bTrace_"+i;
			ballTraceSpr[i] = ballTraceObj[i].GetComponent<UISprite>();
			ballTraceSpr[i].alpha = 1-(i*0.1f);
			ballTraceObj[i].SetActiveRecursively(false);
		}
		txtBallSpeedObj.gameObject.SetActiveRecursively(false);
		txtBallSpeed = txtBallSpeedObj.GetComponent<UISysFontLabel>();
		inflectionPnt = new Vector3[ Constants.BALLTYPE_MAXNUM ][];
		P_IfcPnt = new Vector3[ Constants.BALLTYPE_MAX_PITCHER ][];
		//UI Change
		szEffObj = new GameObject[12];
		resultTracePos = new Vector3[9];
		
		switch( GlobalVar.myTeam.P_starting.formIdx )
		{
		case (int)GlobalVar.PITCHER_FORM.OVER:
			PitchingBallMgr_setupEachBallType_1stGroup();
			break;
		case (int)GlobalVar.PITCHER_FORM.SIDE:
		case (int)GlobalVar.PITCHER_FORM.UNDER:
			PitchingBallMgr_setupEachBallType_2ndGroup();
			break;
		}
		
		PitchingBallMgr_Load();
		
		
		prePitchingPnt = GlobalVar.pBallObj.position;
		catchingPnt = GlobalVar.strikeZoneObj.position;
		catchingPnt.z = -57;
		
		//Debug.Log(Mathf.Atan2(0.3073f, 10)*Mathf.Rad2Deg);//));
		
		PitchingBallMgr_setBackToPitcherBall();
	}	
	// Update is called once per frame
	void Update () {}
	
	public void PitchingBallMgr_Load()
	{
		Vector3 tmpPos = new Vector3(0,0,0);
		
		GlobalVar.pBallShadowObj = (GameObject)Instantiate( Resources.Load("Prefab/S_GameProc/Etc/pBallShadow") );
		GlobalVar.pBallShadowObj.transform.parent = GlobalVar.playGroundObj;
		tmpPos = GlobalVar.pBallObj.position;
		tmpPos.y = 1.5f;
		GlobalVar.pBallShadowObj.transform.position = tmpPos;
		GlobalVar.pBallShadowObj.SetActiveRecursively(false);		
		GlobalVar.pBallObj.gameObject.SetActiveRecursively(false);
		//UI Change
		for( int i=0 ; i<szEffObj.Length ; i++ )
		{
			szEffObj[i] = (GameObject)Instantiate(Resources.Load("Prefab/S_GameProc/StrikeZoneEff/szEff_"+i));
			szEffObj[i] = GlobalVar.utilMgr.createGameObjectAsLocal( szEffObj[i], strikeZoneEffObj, "szEff_"+i, false );
		}
		
		for( int i=0 ; i<resultTracePos.Length ; i++ )
		{
			resultTracePos[i] = new Vector3(0,0,0);
		}
		ScoreBoardBgObj = (GameObject)Instantiate(Resources.Load("Prefab/S_GameProc/ScoreBoard/SB_bg"));
		ScoreBoardBgObj = GlobalVar.utilMgr.createGameObjectAsLocal( ScoreBoardBgObj, ScoreBoardObj, "SB_Bg", true );
		dBallObj = new GameObject[3];			
		for( int i=0 ; i<dBallObj.Length ; i++ )
		{
			dBallObj[i] = (GameObject)Instantiate(Resources.Load("Prefab/S_GameProc/ScoreBoard/SB_sign_Ball"));
			dBallObj[i] = GlobalVar.utilMgr.createGameObjectAsLocal( dBallObj[i], ScoreBoardObj, "SB_sign_Ball_"+i, (i*20), false);
		}
		
		dStrikeObj = new GameObject[2];
		dOutObj = new GameObject[2];
		for( int i=0 ; i<dStrikeObj.Length ; i++ )
		{
			dStrikeObj[i] = (GameObject)Instantiate(Resources.Load("Prefab/S_GameProc/ScoreBoard/SB_sign_Strike"));
			dStrikeObj[i] = GlobalVar.utilMgr.createGameObjectAsLocal( dStrikeObj[i], ScoreBoardObj, "SB_sign_Strike_"+i, (i*20), false );
			dOutObj[i]  = (GameObject)Instantiate(Resources.Load("Prefab/S_GameProc/ScoreBoard/SB_sign_Out"));
			dOutObj[i] = GlobalVar.utilMgr.createGameObjectAsLocal( dOutObj[i], ScoreBoardObj, "SB_sign_Out_"+i, (i*20), false );
		}
		signGBObj = (GameObject)Instantiate(Resources.Load("Prefab/S_GameProc/Gesture/Sign_gestureBad"));
		signGBObj = GlobalVar.utilMgr.createGameObjectAsLocal( signGBObj, GlobalVar.homeBaseZoneBox, "Sign_gestureBad", false );
	}
	
//Initialize
	public void PitchingBallMgr_initialize()
	{
		ballSpeed = 0;
		bStrike = false;
		pingBallFrmCnt = 0;
		GlobalVar.time_RlsPnt = 0.0f;
		GlobalVar.time_hitPrs = 0.0f;
		GlobalVar.vesStrikeZoneArrIdx = -1;
		pBallFlyTime = 0.0f;//when ball pitching is Finished
		//szEffFrmCnt = 0;		
		GlobalVar.bStartBackToPit = false;
		GlobalVar.bBallInMitt = false;
		GlobalVar.bThrowToPit = false;
		GlobalVar.bGripBall = false;
		GlobalVar.bStartCatAnim = false;
		bStartSzEff = false;
		
		bRefereeCall = false;
		badFrmCnt = 0;
		Debug.Log("PitchingBallMgr_initialize() initialize");
	}
	
	void PitchingBallMgr_initSzEff()
	{
		//UI Change
		Vector3 tmpVec = new Vector3(0,0,0);
		
		int viewNeg = 1;
		
		if( GlobalVar.bHome )//Pitcher view
		{
			viewNeg = -1;
		
			//Debug.Log("idx = "+GlobalVar.vesStrikeZoneArrIdx+"/"+GlobalVar.ballTypeSelNum);
			GlobalVar.ballPosInSZone = vesBallCrs[ GlobalVar.vesStrikeZoneArrIdx ];
			tmpVec.x = GlobalVar.ballPosInSZone.x;
			tmpVec.y = GlobalVar.ballPosInSZone.y;
			for( int i=0 ; i<=8 ; i++ )
			{
				if( !(i>=0 && i<=2) )
				{
					tmpVec.z = szEffObj[i].transform.localPosition.z*viewNeg;
					szEffObj[i].transform.localPosition = tmpVec;
				}
			}
			tmpVec.z = szEffObj[10].transform.localPosition.z*viewNeg;
			szEffObj[10].transform.localPosition = tmpVec;
			
			
			finalCatchPnt = catchingPnt;
			tmpVec.x = finalCatchPnt.x;
			tmpVec.y = finalCatchPnt.y;
			
			tmpVec.z = szEffObj[9].transform.localPosition.z*viewNeg;
			szEffObj[9].transform.localPosition = tmpVec;
			tmpVec.z = szEffObj[11].transform.localPosition.z*viewNeg;
			szEffObj[11].transform.localPosition = tmpVec;
		}
		
		for( int i=0 ; i<szEffObj.Length ; i++ )
		{
			szEffObj[i].SetActiveRecursively(false);
		}
		vesBallCrs = null;
	}
	
	
	
//Set	
	/// <summary>
	/// The distance what ball type define as mit of catcher from releasing point of pitcher( 0~300 on Tool )
	/// </summary>
	void PitchingBallMgr_setupEachBallType_1stGroup()
	{
		int handDir_neg = 1;
		
		if( GlobalVar.myTeam.P_starting.handDir==(int)GlobalVar.HANDDIR.LEFT )
		{
			handDir_neg = -1;
		}
		
		for( int i=0 ; i<Constants.BALLTYPE_MAXNUM ; i++ )
		{
			switch( i )
			{
			case Constants.BALLTYPE_FOURSEAM:
				switch( GlobalVar.myTeam.P_starting.bodyIdx )
				{
				case (int)GlobalVar.BODYTYPE.BASIC:
					inflectionPnt[ Constants.BALLTYPE_FOURSEAM ] = new Vector3[3];
					inflectionPnt[ Constants.BALLTYPE_FOURSEAM ][0] = new Vector3(0, 0, 0);
					inflectionPnt[ Constants.BALLTYPE_FOURSEAM ][1] = new Vector3(0, -3.7185f, 398.56f);
					inflectionPnt[ Constants.BALLTYPE_FOURSEAM ][2] = new Vector3(0, 0.65f, 747.3f);
					break;
				case (int)GlobalVar.BODYTYPE.SLUGGER:
					break;
				case (int)GlobalVar.BODYTYPE.BIGBOY:
					break;
				}
				break;
			case Constants.BALLTYPE_TWOSEAM:
				switch( GlobalVar.myTeam.P_starting.bodyIdx )
				{
				case (int)GlobalVar.BODYTYPE.BASIC:
					inflectionPnt[ Constants.BALLTYPE_TWOSEAM ] = new Vector3[3];
					inflectionPnt[ Constants.BALLTYPE_TWOSEAM ][0] = new Vector3(0, 0, 0);
					inflectionPnt[ Constants.BALLTYPE_TWOSEAM ][1] = new Vector3(-1f*handDir_neg, -5.2f, 433.434f);
					inflectionPnt[ Constants.BALLTYPE_TWOSEAM ][2] = new Vector3(-4f*handDir_neg, 3f, 747.3f);
					break;
				case (int)GlobalVar.BODYTYPE.SLUGGER:
					break;
				case (int)GlobalVar.BODYTYPE.BIGBOY:
					break;
				}
				break;
			case Constants.BALLTYPE_SINKER:
				switch( GlobalVar.myTeam.P_starting.bodyIdx )
				{
				case (int)GlobalVar.BODYTYPE.BASIC:
					inflectionPnt[ Constants.BALLTYPE_SINKER ] = new Vector3[3];
					inflectionPnt[ Constants.BALLTYPE_SINKER ][0] = new Vector3(0, 0, 0);
					inflectionPnt[ Constants.BALLTYPE_SINKER ][1] = new Vector3(-2.2f*handDir_neg, -13, 453.362f);
					inflectionPnt[ Constants.BALLTYPE_SINKER ][2] = new Vector3(-5.9f*handDir_neg, 6.3f, 747.3f);
					break;
				case (int)GlobalVar.BODYTYPE.SLUGGER:
					break;
				case (int)GlobalVar.BODYTYPE.BIGBOY:
					break;
				}
				break;
			case Constants.BALLTYPE_CHANGEUP:
				switch( GlobalVar.myTeam.P_starting.bodyIdx )
				{
				case (int)GlobalVar.BODYTYPE.BASIC:
					inflectionPnt[ Constants.BALLTYPE_CHANGEUP ] = new Vector3[3];
					inflectionPnt[ Constants.BALLTYPE_CHANGEUP ][0] = new Vector3(0, 0, 0);
					inflectionPnt[ Constants.BALLTYPE_CHANGEUP ][1] = new Vector3(0, -15, 453.362f);
					inflectionPnt[ Constants.BALLTYPE_CHANGEUP ][2] = new Vector3(0, 9.3f, 747.3f);
					break;
				case (int)GlobalVar.BODYTYPE.SLUGGER:
					break;
				case (int)GlobalVar.BODYTYPE.BIGBOY:
					break;
				}
				break;
			case Constants.BALLTYPE_PALM:
				switch( GlobalVar.myTeam.P_starting.bodyIdx )
				{
				case (int)GlobalVar.BODYTYPE.BASIC:
					inflectionPnt[ Constants.BALLTYPE_PALM ] = new Vector3[3];
					inflectionPnt[ Constants.BALLTYPE_PALM ][0] = new Vector3(0, 0, 0);
					inflectionPnt[ Constants.BALLTYPE_PALM ][1] = new Vector3(0, -45.94f, 373.65f);
					inflectionPnt[ Constants.BALLTYPE_PALM ][2] = new Vector3(0, 17.5f, 747.3f);
					break;
				case (int)GlobalVar.BODYTYPE.SLUGGER:
					break;
				case (int)GlobalVar.BODYTYPE.BIGBOY:
					break;
				}
				break;
			case Constants.BALLTYPE_FORK:
				switch( GlobalVar.myTeam.P_starting.bodyIdx )
				{
				case (int)GlobalVar.BODYTYPE.BASIC:
					inflectionPnt[ Constants.BALLTYPE_FORK ] = new Vector3[3];
					inflectionPnt[ Constants.BALLTYPE_FORK ][0] = new Vector3(0, 0, 0);
					inflectionPnt[ Constants.BALLTYPE_FORK ][1] = new Vector3(0, -21.64f, 577.912f);
					inflectionPnt[ Constants.BALLTYPE_FORK ][2] = new Vector3(0, 21f, 747.3f);
					break;
				case (int)GlobalVar.BODYTYPE.SLUGGER:
					break;
				case (int)GlobalVar.BODYTYPE.BIGBOY:
					break;
				}
				break;
			case Constants.BALLTYPE_CURVE:
				switch( GlobalVar.myTeam.P_starting.bodyIdx )
				{
				case (int)GlobalVar.BODYTYPE.BASIC:
					inflectionPnt[ Constants.BALLTYPE_CURVE ] = new Vector3[3];
					inflectionPnt[ Constants.BALLTYPE_CURVE ][0] = new Vector3(0, 0, 0);
					inflectionPnt[ Constants.BALLTYPE_CURVE ][1] = new Vector3(0, -42.8f, 523.11f);
					inflectionPnt[ Constants.BALLTYPE_CURVE ][2] = new Vector3(4.4f*handDir_neg, 22f, 747.3f);
					break;
				case (int)GlobalVar.BODYTYPE.SLUGGER:
					break;
				case (int)GlobalVar.BODYTYPE.BIGBOY:
					break;
				}
				break;
			case Constants.BALLTYPE_SLIDER:
				switch( GlobalVar.myTeam.P_starting.bodyIdx )
				{
				case (int)GlobalVar.BODYTYPE.BASIC:
					inflectionPnt[ Constants.BALLTYPE_SLIDER ] = new Vector3[3];
					inflectionPnt[ Constants.BALLTYPE_SLIDER ][0] = new Vector3(0, 0, 0);
					inflectionPnt[ Constants.BALLTYPE_SLIDER ][1] = new Vector3(0.5f*handDir_neg, -13.575f, 438.416f);
					inflectionPnt[ Constants.BALLTYPE_SLIDER ][2] = new Vector3(8f*handDir_neg, 6.695f, 747.3f);
					break;
				case (int)GlobalVar.BODYTYPE.SLUGGER:
					break;
				case (int)GlobalVar.BODYTYPE.BIGBOY:
					break;
				}
				break;
			case Constants.BALLTYPE_CUTFB:
				switch( GlobalVar.myTeam.P_starting.bodyIdx )
				{
				case (int)GlobalVar.BODYTYPE.BASIC:
					inflectionPnt[ Constants.BALLTYPE_CUTFB ] = new Vector3[3];
					inflectionPnt[ Constants.BALLTYPE_CUTFB ][0] = new Vector3(0, 0, 0);
					inflectionPnt[ Constants.BALLTYPE_CUTFB ][1] = new Vector3(0, -12.18f, 453.362f);
					inflectionPnt[ Constants.BALLTYPE_CUTFB ][2] = new Vector3(2.1f*handDir_neg, 4.9f, 747.3f);
					break;
				case (int)GlobalVar.BODYTYPE.SLUGGER:
					break;
				case (int)GlobalVar.BODYTYPE.BIGBOY:
					break;
				}
				break;
			case Constants.BALLTYPE_RISINGFB:
				switch( GlobalVar.myTeam.P_starting.bodyIdx )
				{
				case (int)GlobalVar.BODYTYPE.BASIC:
					inflectionPnt[ Constants.BALLTYPE_RISINGFB ] = new Vector3[3];
					inflectionPnt[ Constants.BALLTYPE_RISINGFB ][0] = new Vector3(0, 0, 0);
					inflectionPnt[ Constants.BALLTYPE_RISINGFB ][1] = new Vector3(2f*handDir_neg, 1f, 555.961f);
					inflectionPnt[ Constants.BALLTYPE_RISINGFB ][2] = new Vector3(-4f*handDir_neg, -2f, 747.3f);
					break;
				case (int)GlobalVar.BODYTYPE.SLUGGER:
					break;
				case (int)GlobalVar.BODYTYPE.BIGBOY:
					break;
				}
				break;
			case Constants.BALLTYPE_SINKINGFB:
				switch( GlobalVar.myTeam.P_starting.bodyIdx )
				{
				case (int)GlobalVar.BODYTYPE.BASIC:
					inflectionPnt[ Constants.BALLTYPE_SINKINGFB ] = new Vector3[3];
					inflectionPnt[ Constants.BALLTYPE_SINKINGFB ][0] = new Vector3(0, 0, 0);
					inflectionPnt[ Constants.BALLTYPE_SINKINGFB ][1] = new Vector3(1f*handDir_neg, -11f, 455.961f);
					inflectionPnt[ Constants.BALLTYPE_SINKINGFB ][2] = new Vector3(-2f*handDir_neg, 5.3f, 747.3f);
					break;
				case (int)GlobalVar.BODYTYPE.SLUGGER:
					break;
				case (int)GlobalVar.BODYTYPE.BIGBOY:
					break;
				}
				break;
			case Constants.BALLTYPE_CIRCLECH:
				switch( GlobalVar.myTeam.P_starting.bodyIdx )
				{
				case (int)GlobalVar.BODYTYPE.BASIC:
					inflectionPnt[ Constants.BALLTYPE_CIRCLECH ] = new Vector3[3];
					inflectionPnt[ Constants.BALLTYPE_CIRCLECH ][0] = new Vector3(0, 0, 0);
					inflectionPnt[ Constants.BALLTYPE_CIRCLECH ][1] = new Vector3(-1f*handDir_neg, -20f, 460.835f);
					inflectionPnt[ Constants.BALLTYPE_CIRCLECH ][2] = new Vector3(-4f*handDir_neg, 8.5f, 747.3f);
					break;
				case (int)GlobalVar.BODYTYPE.SLUGGER:
					break;
				case (int)GlobalVar.BODYTYPE.BIGBOY:
					break;
				}
				break;
			case Constants.BALLTYPE_KNUCKLE:
				switch( GlobalVar.myTeam.P_starting.bodyIdx )
				{
				case (int)GlobalVar.BODYTYPE.BASIC:
					inflectionPnt[ Constants.BALLTYPE_KNUCKLE ] = new Vector3[6];
					inflectionPnt[ Constants.BALLTYPE_KNUCKLE ][0] = new Vector3(0, 0, 0);
					inflectionPnt[ Constants.BALLTYPE_KNUCKLE ][1] = new Vector3(0f*handDir_neg, -8.91f, 61.74f);
					inflectionPnt[ Constants.BALLTYPE_KNUCKLE ][2] = new Vector3(0f*handDir_neg, -3f, 1031.94f);
					inflectionPnt[ Constants.BALLTYPE_KNUCKLE ][3] = new Vector3(0, -10.567f, 626.106f);
					inflectionPnt[ Constants.BALLTYPE_KNUCKLE ][4] = new Vector3(0, -6.279f, 586.32f);
					inflectionPnt[ Constants.BALLTYPE_KNUCKLE ][5] = new Vector3(0, 12.62f, 747.3f);
					break;
				case (int)GlobalVar.BODYTYPE.SLUGGER:
					break;
				case (int)GlobalVar.BODYTYPE.BIGBOY:
					break;
				}
				break;
			case Constants.BALLTYPE_SPLITER:
				switch( GlobalVar.myTeam.P_starting.bodyIdx )
				{
				case (int)GlobalVar.BODYTYPE.BASIC:
					inflectionPnt[ Constants.BALLTYPE_SPLITER ] = new Vector3[3];
					inflectionPnt[ Constants.BALLTYPE_SPLITER ][0] = new Vector3(0, 0, 0);
					inflectionPnt[ Constants.BALLTYPE_SPLITER ][1] = new Vector3(0, -21.736f, 543.038f);
					inflectionPnt[ Constants.BALLTYPE_SPLITER ][2] = new Vector3(0f*handDir_neg, 10.5f, 747.3f);
					break;
				case (int)GlobalVar.BODYTYPE.SLUGGER:
					break;
				case (int)GlobalVar.BODYTYPE.BIGBOY:
					break;
				}
				break;
			case Constants.BALLTYPE_SLOWCV:
				switch( GlobalVar.myTeam.P_starting.bodyIdx )
				{
				case (int)GlobalVar.BODYTYPE.BASIC:
					inflectionPnt[ Constants.BALLTYPE_SLOWCV ] = new Vector3[3];
					inflectionPnt[ Constants.BALLTYPE_SLOWCV ][0] = new Vector3(0, 0, 0);
					inflectionPnt[ Constants.BALLTYPE_SLOWCV ][1] = new Vector3(0, -71.92f, 480.763f);
					inflectionPnt[ Constants.BALLTYPE_SLOWCV ][2] = new Vector3(2.965f*handDir_neg, 29.8f, 747.3f);
					break;
				case (int)GlobalVar.BODYTYPE.SLUGGER:
					break;
				case (int)GlobalVar.BODYTYPE.BIGBOY:
					break;
				}
				break;
			case Constants.BALLTYPE_HSLIDER:
				switch( GlobalVar.myTeam.P_starting.bodyIdx )
				{
				case (int)GlobalVar.BODYTYPE.BASIC:
					inflectionPnt[ Constants.BALLTYPE_HSLIDER ] = new Vector3[3];
					inflectionPnt[ Constants.BALLTYPE_HSLIDER ][0] = new Vector3(0, 0, 0);
					inflectionPnt[ Constants.BALLTYPE_HSLIDER ][1] = new Vector3(-3f*handDir_neg, -11.32f, 455.853f);
					inflectionPnt[ Constants.BALLTYPE_HSLIDER ][2] = new Vector3(13.6f*handDir_neg, 4.35f, 747.3f);
					break;
				case (int)GlobalVar.BODYTYPE.SLUGGER:
					break;
				case (int)GlobalVar.BODYTYPE.BIGBOY:
					break;
				}
				break;
			}
			//PitchingBallMgr_setEachFormNBodysBallType();
		}
		//Catcher return ball to Pitcher so it needs return bezier line
		bIfcPoints = new Vector3[3];
		bIfcPoints[0] = new Vector3(0,0,0);
		bIfcPoints[1] = new Vector3(0,-30,250);
		bIfcPoints[2] = new Vector3(0,0,705);
	}
	
	public void PitchingBallMgr_setupEachBallType_2ndGroup()
	{
		int handDir_neg = 1;
		
		if( GlobalVar.myTeam.P_starting.handDir==(int)GlobalVar.HANDDIR.LEFT )
		{
			handDir_neg = -1;
		}
		
		for( int i=0 ; i<Constants.BALLTYPE_MAXNUM ; i++ )
		{
			switch( i )
			{
			case Constants.BALLTYPE_FOURSEAM:
				switch( GlobalVar.myTeam.P_starting.bodyIdx )
				{
				case (int)GlobalVar.BODYTYPE.BASIC:
					inflectionPnt[ Constants.BALLTYPE_FOURSEAM ] = new Vector3[4];
					inflectionPnt[ Constants.BALLTYPE_FOURSEAM ][0] = new Vector3(0, 0, 0);
					inflectionPnt[ Constants.BALLTYPE_FOURSEAM ][1] = new Vector3(0.691f*handDir_neg, -1.976f, 172.359f);
					inflectionPnt[ Constants.BALLTYPE_FOURSEAM ][2] = new Vector3(3.952f*handDir_neg, -12.35f, 420.875f);
					inflectionPnt[ Constants.BALLTYPE_FOURSEAM ][3] = new Vector3(-0.889f*handDir_neg, 2.273f, 747.3f);
					break;
				case (int)GlobalVar.BODYTYPE.SLUGGER:
					break;
				case (int)GlobalVar.BODYTYPE.BIGBOY:
					break;
				}
				break;
			case Constants.BALLTYPE_TWOSEAM:
				switch( GlobalVar.myTeam.P_starting.bodyIdx )
				{
				case (int)GlobalVar.BODYTYPE.BASIC:
					inflectionPnt[ Constants.BALLTYPE_TWOSEAM ] = new Vector3[4];
					inflectionPnt[ Constants.BALLTYPE_TWOSEAM ][0] = new Vector3(0, 0, 0);
					inflectionPnt[ Constants.BALLTYPE_TWOSEAM ][1] = new Vector3(-0.8f*handDir_neg, 0, 186.825f);
					inflectionPnt[ Constants.BALLTYPE_TWOSEAM ][2] = new Vector3(1.6f*handDir_neg, -13, 438.416f);
					inflectionPnt[ Constants.BALLTYPE_TWOSEAM ][3] = new Vector3(-4f*handDir_neg, 4.3f, 747.3f);
					break;
				case (int)GlobalVar.BODYTYPE.SLUGGER:
					break;
				case (int)GlobalVar.BODYTYPE.BIGBOY:
					break;
				}
				break;
			case Constants.BALLTYPE_SINKER:
				switch( GlobalVar.myTeam.P_starting.bodyIdx )
				{
				case (int)GlobalVar.BODYTYPE.BASIC:
					inflectionPnt[ Constants.BALLTYPE_SINKER ] = new Vector3[4];
					inflectionPnt[ Constants.BALLTYPE_SINKER ][0] = new Vector3(0, 0, 0);
					inflectionPnt[ Constants.BALLTYPE_SINKER ][1] = new Vector3(-0.8f*handDir_neg, 0, 186.825f);
					inflectionPnt[ Constants.BALLTYPE_SINKER ][2] = new Vector3(1.6f*handDir_neg, -13, 438.416f);
					inflectionPnt[ Constants.BALLTYPE_SINKER ][3] = new Vector3(-6.3f*handDir_neg, 6.3f, 747.3f);
					break;
				case (int)GlobalVar.BODYTYPE.SLUGGER:
					break;
				case (int)GlobalVar.BODYTYPE.BIGBOY:
					break;
				}
				break;
			case Constants.BALLTYPE_CHANGEUP:
				switch( GlobalVar.myTeam.P_starting.bodyIdx )
				{
				case (int)GlobalVar.BODYTYPE.BASIC:
					inflectionPnt[ Constants.BALLTYPE_CHANGEUP ] = new Vector3[4];
					inflectionPnt[ Constants.BALLTYPE_CHANGEUP ][0] = new Vector3(0, 0, 0);
					inflectionPnt[ Constants.BALLTYPE_CHANGEUP ][1] = new Vector3(0.691f*handDir_neg, -1.976f, 190.359f);
					inflectionPnt[ Constants.BALLTYPE_CHANGEUP ][2] = new Vector3(3.952f*handDir_neg, -16.35f, 420.875f);
					inflectionPnt[ Constants.BALLTYPE_CHANGEUP ][3] = new Vector3(-0.889f*handDir_neg, 9.2f, 747.3f);
					break;
				case (int)GlobalVar.BODYTYPE.SLUGGER:
					break;
				case (int)GlobalVar.BODYTYPE.BIGBOY:
					break;
				}
				break;
			case Constants.BALLTYPE_FORK:
				switch( GlobalVar.myTeam.P_starting.bodyIdx )
				{
				case (int)GlobalVar.BODYTYPE.BASIC:
					inflectionPnt[ Constants.BALLTYPE_FORK ] = new Vector3[4];
					inflectionPnt[ Constants.BALLTYPE_FORK ][0] = new Vector3(0, 0, 0);
					inflectionPnt[ Constants.BALLTYPE_FORK ][1] = new Vector3(2.373f*handDir_neg, 1.5f, 195.424f);
					inflectionPnt[ Constants.BALLTYPE_FORK ][2] = new Vector3(3.952f*handDir_neg, -20.035f, 500.295f);
					inflectionPnt[ Constants.BALLTYPE_FORK ][3] = new Vector3(-0.9f*handDir_neg, 20.9f, 747.3f);
					break;
				case (int)GlobalVar.BODYTYPE.SLUGGER:
					break;
				case (int)GlobalVar.BODYTYPE.BIGBOY:
					break;
				}
				break;
			case Constants.BALLTYPE_CURVE:
				switch( GlobalVar.myTeam.P_starting.bodyIdx )
				{
				case (int)GlobalVar.BODYTYPE.BASIC:
					inflectionPnt[ Constants.BALLTYPE_CURVE ] = new Vector3[4];
					inflectionPnt[ Constants.BALLTYPE_CURVE ][0] = new Vector3(0, 0, 0);
					switch( GlobalVar.myTeam.P_starting.formIdx )
					{
					case (int)GlobalVar.PITCHER_FORM.SIDE:
						inflectionPnt[ Constants.BALLTYPE_CURVE ][1] = new Vector3(1.58f*handDir_neg, -4.976f, 195.424f);
						inflectionPnt[ Constants.BALLTYPE_CURVE ][2] = new Vector3(2.371f*handDir_neg, -34.85f, 450.871f);
						inflectionPnt[ Constants.BALLTYPE_CURVE ][3] = new Vector3(4.695f*handDir_neg, 18f, 747.3f);
						break;
					case (int)GlobalVar.PITCHER_FORM.UNDER:
						inflectionPnt[ Constants.BALLTYPE_CURVE ][1] = new Vector3(0*handDir_neg, 0.79f, 195.424f);
						inflectionPnt[ Constants.BALLTYPE_CURVE ][2] = new Vector3(1.1856f*handDir_neg, -18.67f, 380.295f);
						inflectionPnt[ Constants.BALLTYPE_CURVE ][3] = new Vector3(4.695f*handDir_neg, 2.251f, 747.3f);
						break;
					}
					break;
				case (int)GlobalVar.BODYTYPE.SLUGGER:
					break;
				case (int)GlobalVar.BODYTYPE.BIGBOY:
					break;
				}
				break;
			case Constants.BALLTYPE_SLIDER:
				switch( GlobalVar.myTeam.P_starting.bodyIdx )
				{
				case (int)GlobalVar.BODYTYPE.BASIC:
					inflectionPnt[ Constants.BALLTYPE_SLIDER ] = new Vector3[4];
					inflectionPnt[ Constants.BALLTYPE_SLIDER ][0] = new Vector3(0, 0, 0);
					inflectionPnt[ Constants.BALLTYPE_SLIDER ][1] = new Vector3(-3.557f*handDir_neg, -1.58f, 139.496f);
					inflectionPnt[ Constants.BALLTYPE_SLIDER ][2] = new Vector3(0f*handDir_neg, -18.575f, 440.907f);
					inflectionPnt[ Constants.BALLTYPE_SLIDER ][3] = new Vector3(7.389f*handDir_neg, 7.195f, 747.3f);
					break;
				case (int)GlobalVar.BODYTYPE.SLUGGER:
					break;
				case (int)GlobalVar.BODYTYPE.BIGBOY:
					break;
				}
				break;
			case Constants.BALLTYPE_CUTFB:
				switch( GlobalVar.myTeam.P_starting.bodyIdx )
				{
				case (int)GlobalVar.BODYTYPE.BASIC:
					inflectionPnt[ Constants.BALLTYPE_CUTFB ] = new Vector3[4];
					inflectionPnt[ Constants.BALLTYPE_CUTFB ][0] = new Vector3(0, 0, 0);
					inflectionPnt[ Constants.BALLTYPE_CUTFB ][1] = new Vector3(-1.976f*handDir_neg, -1.976f, 159.424f);
					inflectionPnt[ Constants.BALLTYPE_CUTFB ][2] = new Vector3(-2.371f*handDir_neg, -18.18f, 458.344f);
					inflectionPnt[ Constants.BALLTYPE_CUTFB ][3] = new Vector3(2.6f*handDir_neg, 6.347f, 747.3f);
					break;
				case (int)GlobalVar.BODYTYPE.SLUGGER:
					break;
				case (int)GlobalVar.BODYTYPE.BIGBOY:
					break;
				}
				break;
			case Constants.BALLTYPE_RISINGFB:
				switch( GlobalVar.myTeam.P_starting.bodyIdx )
				{
				case (int)GlobalVar.BODYTYPE.BASIC:
					inflectionPnt[ Constants.BALLTYPE_RISINGFB ] = new Vector3[4];
					inflectionPnt[ Constants.BALLTYPE_RISINGFB ][0] = new Vector3(0, 0, 0);
					inflectionPnt[ Constants.BALLTYPE_RISINGFB ][1] = new Vector3(-1f*handDir_neg, -1.58f, 144.478f);
					inflectionPnt[ Constants.BALLTYPE_RISINGFB ][2] = new Vector3(1.8f*handDir_neg, -7.114f, 413.506f);
					inflectionPnt[ Constants.BALLTYPE_RISINGFB ][3] = new Vector3(-2.04f*handDir_neg, 1, 747.3f);
					break;
				case (int)GlobalVar.BODYTYPE.SLUGGER:
					break;
				case (int)GlobalVar.BODYTYPE.BIGBOY:
					break;
				}
				break;
			case Constants.BALLTYPE_SINKINGFB:
				switch( GlobalVar.myTeam.P_starting.bodyIdx )
				{
				case (int)GlobalVar.BODYTYPE.BASIC:
					inflectionPnt[ Constants.BALLTYPE_SINKINGFB ] = new Vector3[4];
					inflectionPnt[ Constants.BALLTYPE_SINKINGFB ][0] = new Vector3(0, 0, 0);
					inflectionPnt[ Constants.BALLTYPE_SINKINGFB ][1] = new Vector3(2.37f*handDir_neg, -1.186f, 156.933f);
					inflectionPnt[ Constants.BALLTYPE_SINKINGFB ][2] = new Vector3(3.952f*handDir_neg, -10.275f, 445.899f);
					inflectionPnt[ Constants.BALLTYPE_SINKINGFB ][3] = new Vector3(-2.7f*handDir_neg, 4.3f, 747.3f);
					break;
				case (int)GlobalVar.BODYTYPE.SLUGGER:
					break;
				case (int)GlobalVar.BODYTYPE.BIGBOY:
					break;
				}
				break;
			case Constants.BALLTYPE_CIRCLECH:
				switch( GlobalVar.myTeam.P_starting.bodyIdx )
				{
				case (int)GlobalVar.BODYTYPE.BASIC:
					inflectionPnt[ Constants.BALLTYPE_CIRCLECH ] = new Vector3[4];
					inflectionPnt[ Constants.BALLTYPE_CIRCLECH ][0] = new Vector3(0, 0, 0);
					inflectionPnt[ Constants.BALLTYPE_CIRCLECH ][1] = new Vector3(2.37f*handDir_neg, -3.395f, 122.059f);
					inflectionPnt[ Constants.BALLTYPE_CIRCLECH ][2] = new Vector3(3.952f*handDir_neg, -28.059f, 453.362f);
					inflectionPnt[ Constants.BALLTYPE_CIRCLECH ][3] = new Vector3(-5.1f*handDir_neg, 9.9f, 747.3f);
					break;
				case (int)GlobalVar.BODYTYPE.SLUGGER:
					break;
				case (int)GlobalVar.BODYTYPE.BIGBOY:
					break;
				}
				break;
			case Constants.BALLTYPE_SPLITER:
				switch( GlobalVar.myTeam.P_starting.bodyIdx )
				{
				case (int)GlobalVar.BODYTYPE.BASIC:
					inflectionPnt[ Constants.BALLTYPE_SPLITER ] = new Vector3[4];
					inflectionPnt[ Constants.BALLTYPE_SPLITER ][0] = new Vector3(0, 0, 0);
					inflectionPnt[ Constants.BALLTYPE_SPLITER ][1] = new Vector3(0.79f*handDir_neg, -1.58f, 141.987f);
					inflectionPnt[ Constants.BALLTYPE_SPLITER ][2] = new Vector3(-2.37f*handDir_neg, -18.57f, 518.128f);
					inflectionPnt[ Constants.BALLTYPE_SPLITER ][3] = new Vector3(0.7f*handDir_neg, 9.4f, 747.3f);
					break;
				case (int)GlobalVar.BODYTYPE.SLUGGER:
					break;
				case (int)GlobalVar.BODYTYPE.BIGBOY:
					break;
				}
				break;
			case Constants.BALLTYPE_UPSHOOT:
				switch( GlobalVar.myTeam.P_starting.bodyIdx )
				{
				case (int)GlobalVar.BODYTYPE.BASIC:
					inflectionPnt[ Constants.BALLTYPE_UPSHOOT ] = new Vector3[3];
					inflectionPnt[ Constants.BALLTYPE_UPSHOOT ][0] = new Vector3(0, 0, 0);
					inflectionPnt[ Constants.BALLTYPE_UPSHOOT ][1] = new Vector3(1.58f*handDir_neg, -0.4f, 351.231f);
					inflectionPnt[ Constants.BALLTYPE_UPSHOOT ][2] = new Vector3(11.8f*handDir_neg, -4f, 747.3f);
					break;
				case (int)GlobalVar.BODYTYPE.SLUGGER:
					break;
				case (int)GlobalVar.BODYTYPE.BIGBOY:
					break;
				}
				break;
			case Constants.BALLTYPE_FRISBEE:
				switch( GlobalVar.myTeam.P_starting.bodyIdx )
				{
				case (int)GlobalVar.BODYTYPE.BASIC:
					inflectionPnt[ Constants.BALLTYPE_FRISBEE ] = new Vector3[4];
					inflectionPnt[ Constants.BALLTYPE_FRISBEE ][0] = new Vector3(0, 0, 0);
					inflectionPnt[ Constants.BALLTYPE_FRISBEE ][1] = new Vector3(-3.952f*handDir_neg, -0.79f, 127.041f);
					inflectionPnt[ Constants.BALLTYPE_FRISBEE ][2] = new Vector3(0.4f*handDir_neg, -16.6f, 535.565f);
					inflectionPnt[ Constants.BALLTYPE_FRISBEE ][3] = new Vector3(17.8f*handDir_neg, 8.9f, 747.3f);
					break;
				case (int)GlobalVar.BODYTYPE.SLUGGER:
					break;
				case (int)GlobalVar.BODYTYPE.BIGBOY:
					break;
				}
				break;
			}
		}	
		//Debug.Log("inflectionPntArrLen : "+inflectionPnt.Length);
		//Debug.Log("inflectionPntArrLen : "+inflectionPnt[0].Length);
		//Catcher return ball to Pitcher so it needs return bezier line
		bIfcPoints = new Vector3[3];
		bIfcPoints[0] = new Vector3(0,0,0);
		bIfcPoints[1] = new Vector3(0,-50,300);
		bIfcPoints[2] = new Vector3(0,0,705);
	}
	
	//setup strike zone effect objects position
	void PitchingBallMgr_setupSzEff()
	{
		//UI Change
		Vector3 tmpVec = new Vector3(0,0,0);
		
		int viewNeg = 1;
		
		if( GlobalVar.bHome )//Pitcher view
		{
			viewNeg = -1;
		}
		
		//Debug.Log("111111 idx = "+GlobalVar.vesStrikeZoneArrIdx+"/"+GlobalVar.ballTypeSelNum+"/ Len:"+vesBallCrs.Length);
		//Debug.Log("00000 "+vesBallCrs.Length+" / "+GlobalVar.vesStrikeZoneArrIdx);
		GlobalVar.ballPosInSZone = vesBallCrs[ GlobalVar.vesStrikeZoneArrIdx ];
		tmpVec.x = GlobalVar.ballPosInSZone.x;
		tmpVec.y = GlobalVar.ballPosInSZone.y;
		bStrike = PitchingBallMgr_bStrike();
		//PitchingBallMgr_setRefereeDecisionStrikeOrBall();
		//if(tmpVec.y<0)tmpVec.y*=(-1);
		for( int i=0 ; i<=8 ; i++ )
		{
			if( !(i>=0 && i<=2) )
			{
				tmpVec.z = szEffObj[i].transform.localPosition.z*viewNeg;
				szEffObj[i].transform.localPosition = tmpVec;
			}
		}
		tmpVec.z = szEffObj[10].transform.localPosition.z*viewNeg;
		szEffObj[10].transform.localPosition = tmpVec;
		ballTraceCnt++;
		if( ballTraceCnt>10)ballTraceCnt = 10;
		PitchingBallMgr_setPitchedBalltrace( szEffObj[10].transform.localPosition );
		//Debug.Log("ballPosInStrikeZone = "+GlobalVar.ballPosInSZone+"/"+szEffObj[10].localPosition+"/"+szEffObj[10].localPosition);
		//
		
		finalCatchPnt = catchingPnt;
		tmpVec.x = finalCatchPnt.x;
		tmpVec.y = finalCatchPnt.y;
		
		tmpVec.z = szEffObj[9].transform.localPosition.z*viewNeg;
		szEffObj[9].transform.localPosition = tmpVec;
		tmpVec.z = szEffObj[11].transform.localPosition.z*viewNeg;
		szEffObj[11].transform.localPosition = tmpVec;
	}
	
	//setup trace position of result ball pitched
	void PitchingBallMgr_setSzBallTracePos()
	{
		//UI Change
		float tx=0.0f;
		float ty=0.0f;
		
		//set position by trace of result ball
		tx = ( GlobalVar.ballPosInSZone.x - finalCatchPnt.x ) / 10;
		ty = ( GlobalVar.ballPosInSZone.y - finalCatchPnt.y ) / 10;
		resultTracePos[0].x = finalCatchPnt.x + tx;
		resultTracePos[0].y = finalCatchPnt.y + ty;
		resultTracePos[0].z = -0.45f;
		for( int i=1 ; i<resultTracePos.Length ; i++ )
		{
			resultTracePos[i].x = resultTracePos[i-1].x + tx;
			resultTracePos[i].y = resultTracePos[i-1].y + ty;
			resultTracePos[i].z = szEffObj[11].transform.position.z;// -0.45f;
		}
	}
	
	void PitchingBallMgr_setPitchedBalltrace( Vector3 tPos )
	{
		if( ballTraceCnt>1 )
		{
			for( int i=(ballTraceCnt-1) ; i>=1 ; i--)
			{
				ballTraceObj[i].transform.localPosition = ballTraceObj[i-1].transform.localPosition;
				ballTraceObj[i].SetActiveRecursively(false);
			}
		}
		ballTraceObj[0].transform.localPosition = tPos;
		ballTraceObj[0].SetActiveRecursively(false);
	}
	
	public void PitchingBallMgr_setBallTraceActiveReculsive( bool sw )
	{
		if( sw )
		{
			if( GlobalVar.myTeam.bBottom==0 )
			{
				for( int i=0 ; i<ballTraceCnt ; i++ )
				{
					ballTraceObj[i].SetActiveRecursively( sw );
				}
			}
		}
	}
	
	void PitchingBallMgr_setBackToPitcherBall()
	{
		backToPitcherPos = new Vector3[30];
		btCatPos = new Vector3(4.9f, 59.5f, -64.6f);
		btPitPos = new Vector3(-17.4f, 79, 663);
		backToPitcherPos = PitchingBallMgr_getBackToPitcherBall( backToPitcherPos.Length, bIfcPoints.Length );
		//Debug.Log(backToPitcherPos[1]);
		PitchingBallMgr_finallyMakeBackToPitcherBall();
	}
	
	public void PitchingBallMgr_setCatchingPoint()
	{
		Vector3 tmpPos = new Vector3(0,0,0);
		tmpPos = GlobalVar.selectionBall.localPosition;
		tmpPos.z = -57;
		catchingPnt = tmpPos;
	}
	
	void PitchingBallMgr_setRefereeDecisionStrikeOrBall()
	{
		//Strike
		if( bStrike )
		{
			if( GlobalVar.count_Strike<dStrikeObj.Length )
			{
				dStrikeObj[GlobalVar.count_Strike++].SetActiveRecursively(true);
			}
			//OUT
			else
			{
				GlobalVar.count_Strike = 0;
				GlobalVar.battingNum_Pre = GlobalVar.battingNum;
				GlobalVar.battingNum++;
				for( int i=0 ; i<dStrikeObj.Length ; i++ )
				{
					dStrikeObj[i].SetActiveRecursively(false);
				}
				GlobalVar.count_Ball = 0;
				for( int i=0 ; i<dBallObj.Length ; i++ )
				{
					dBallObj[i].SetActiveRecursively(false);
				}
				
				if( GlobalVar.count_Out<dOutObj.Length )
				{
					dOutObj[GlobalVar.count_Out++].SetActiveRecursively(true);
				}
				else
				{
					GlobalVar.count_Out = 0;
					for( int i=0 ; i<dOutObj.Length ; i++ )
					{
						dOutObj[i].SetActiveRecursively(false);
					}
				}
			}
			
			//Debug.Log("Strike Count = "+GlobalVar.count_Strike);
		}
		//Ball
		else
		{
			if( GlobalVar.count_Ball<dBallObj.Length )
			{
				dBallObj[GlobalVar.count_Ball++].SetActiveRecursively(true);
			}
			//4Ball
			else
			{
				GlobalVar.count_Ball = 0;
				for( int i=0 ; i<dBallObj.Length ; i++ )
				{
					dBallObj[i].SetActiveRecursively(false);
				}
				GlobalVar.count_Strike = 0;
				for( int i=0 ; i<dStrikeObj.Length ; i++ )
				{
					dStrikeObj[i].SetActiveRecursively(false);
				}
			}
		}
	}
	
	public void PitchingBallMgr_setFinishPitching()
	{
		GlobalVar.pBallObj.gameObject.SetActiveRecursively(false);
		GlobalVar.pBallShadowObj.SetActiveRecursively(false);
		signGBObj.SetActiveRecursively(false);
		GlobalVar.pBallObj.position = prePitchingPnt;
		catchingPnt = GlobalVar.strikeZoneObj.position;
		catchingPnt.z = -57;
		GlobalVar.bBallInMitt = bStartSzEff;
		if( !bRefereeCall )
		{
			PitchingBallMgr_setRefereeDecisionStrikeOrBall();
			bRefereeCall = true;
		}
		
			
		GlobalVar.alphaDep = 1;
		GlobalVar.strikeZoneSpr.alpha = GlobalVar.alphaDep;
		GlobalVar.strikeZoneObj.gameObject.SetActiveRecursively(true);
		txtBallSpeedObj.gameObject.SetActiveRecursively(true);
	}
	
		
	public void PitchingBallMgr_setFinishBallCatToPit()
	{
		bezierResult = null;
		pingBallFrmCnt = 0;
		btBallFrmCnt = 0;
		szEffFrmCnt = 0;
		GlobalVar.bStartBackToPit = false;
		pBallRotRate = 0;
		PitchingBallMgr_initSzEff();
		GlobalVar.pBallObj.gameObject.SetActiveRecursively(false);
		GlobalVar.pBallShadowObj.SetActiveRecursively(false);
		txtBallSpeedObj.gameObject.SetActiveRecursively(false);
	}
	
//Get
	float PitchingBallMgr_getBallSpeed( int ballType )
	{
		float speed = 0.0f;
		float discountSpeed = 1;
		
		switch( GlobalVar.control_Status )
		{
		case Constants.STATE_CG_WEAK:
		case Constants.STATE_CG_NORMAL:
			discountSpeed = 0.8f + GlobalVar.CG_ctlDegree*0.2f;
			break;
		//case Constants.STATE_CG_OVER:
		//	break;
		case Constants.STATE_CG_BAD:
			discountSpeed = 0.8f;
			break;
		}
		
		
		
		//Debug.Log("discountSpeed = "+discountSpeed+"/"+GlobalVar.CG_ctlDegree*0.2f);
		
		//below each speed needs to read from FILE.
		switch( ballType )
		{
		case Constants.BALLTYPE_FOURSEAM:
			speed = Constants.BASE_SPEED_BALL*discountSpeed;
			break;
		case Constants.BALLTYPE_TWOSEAM:
			speed = Constants.BASE_SPEED_BALL*0.95f*discountSpeed;
			break;
		case Constants.BALLTYPE_SINKER:
			speed = Constants.BASE_SPEED_BALL*0.87f*discountSpeed;
			break;
		case Constants.BALLTYPE_CHANGEUP:
			speed = Constants.BASE_SPEED_BALL*0.77f*discountSpeed;
			break;
		case Constants.BALLTYPE_PALM:
			speed = Constants.BASE_SPEED_BALL*0.57f*discountSpeed;
			break;
		case Constants.BALLTYPE_FORK:
			speed = Constants.BASE_SPEED_BALL*0.84f*discountSpeed;//0.67f; It's changed
			break;
		case Constants.BALLTYPE_CURVE:
			speed = Constants.BASE_SPEED_BALL*0.8f*discountSpeed;
			break;
		case Constants.BALLTYPE_SLIDER:
			speed = Constants.BASE_SPEED_BALL*0.87f*discountSpeed;		
			break;
		case Constants.BALLTYPE_CUTFB:
		case Constants.BALLTYPE_SINKINGFB:
			speed = Constants.BASE_SPEED_BALL*0.86f*discountSpeed;
			break;
		case Constants.BALLTYPE_RISINGFB:
		case Constants.BALLTYPE_UPSHOOT:
			speed = Constants.BASE_SPEED_BALL*0.95f*discountSpeed;
			break;
		case Constants.BALLTYPE_CIRCLECH:
		case Constants.BALLTYPE_SPLITER:
			speed = Constants.BASE_SPEED_BALL*0.75f*discountSpeed;
			break;
		case Constants.BALLTYPE_KNUCKLE:
		case Constants.BALLTYPE_SLOWCV:
			speed = Constants.BASE_SPEED_BALL*0.67f*discountSpeed;
			break;
		case Constants.BALLTYPE_FRISBEE:
			speed = Constants.BASE_SPEED_BALL*0.78f*discountSpeed;
			break;
		case Constants.BALLTYPE_HSLIDER:
			speed = Constants.BASE_SPEED_BALL*0.85f*discountSpeed;
			break;
		}
		
		return speed;
	}
	
	int PitchingBallMgr_getPitchingBallFrame( float speed )
	{
		float time = 0.0f;
		int retVal = 0;
		
		ballSpeedPerSec = Constants.BALLSPEED_RATE * speed;
		ballSpeedPerSec = Constants.BT_TIME_HIT + (Constants.BT_TIME_HIT-ballSpeedPerSec);
		
		
		time = ballSpeedPerSec - ((ballSpeedPerSec*speed)/Constants.BASE_SPEED_BALL);
		time += ballSpeedPerSec;
		
		Debug.Log("#### Pitching Ball Frame() time="+time+"/ speed : "+speed);
		
		retVal = (int)(time/Constants.SECPERFRAME);
		
		Debug.Log("#### Pitching Ball Frame() retVal="+retVal);
		
		//Debug.Log("getPitchingBallFrame() speed:"+speed+"/perSec:"+ballSpeedPerSec+"/frame:"+retVal+"/"+(time/Constants.SECPERFRAME));
		
		return retVal;
	}
	
	private Vector3[] PitchingBallMgr_getBezierResult( int ballType, int frame, int count )
	{
		Vector3[] retVal = new Vector3[frame];
		int cnt;
		double Uv;
		
		cnt = count;
		
		for(int i = 0; i  <frame; i++)
		{
			Uv = ( i / (double)frame);
			double UM = 1.0f - Uv;
			double UM_J = 1.0f;
			double U_J = 1.0f;
			double R_x;
			double R_y;
			double R_z;
						
			for( int j = 0; j < cnt - 1; j++)
			{
				UM_J *= UM;
				U_J *= Uv;
			}
			
			
			//Debug.Log("Uv:"+Uv+"/UM:"+UM+"/UM_J:"+UM_J+"/U_J:"+U_J);
			
			R_x = inflectionPnt[ballType][0].x*UM_J;
			R_y = inflectionPnt[ballType][0].y*UM_J;
			R_z = inflectionPnt[ballType][0].z*UM_J;
		
			//Debug.Log("R_z:"+R_z);
			
			for(int j = 0; j < cnt-2; j++)
			{
				double tU_J = 1.0f;
				double tUM_J = 1.0f;
				
				for(int k = 0; k < j+1; k++)
				{tU_J *= Uv;}
				
				for(int k = 0; k < (cnt-2-j); k++)
				{tUM_J *= UM;}
				
				float tX = inflectionPnt[ballType][ j+1 ].x;
				float tY = inflectionPnt[ballType][ j+1 ].y;
				float tZ = inflectionPnt[ballType][ j+1 ].z;
				
				R_x += (cnt-1) * tU_J * tUM_J * tX;
				R_y += (cnt-1) * tU_J * tUM_J * tY;
				R_z += (cnt-1) * tU_J * tUM_J * tZ;
			}
			R_x += inflectionPnt[ballType][cnt-1].x * U_J;
			R_y += inflectionPnt[ballType][cnt-1].y * U_J;			
			R_z += inflectionPnt[ballType][cnt-1].z * U_J;
						
			Vector3 tVector = new Vector3((float)R_x,(float)R_y,(float)R_z);
			//Debug.Log(i+"th vec:"+tVector);
			retVal[i] = tVector;
			//Debug.Log("retVal["+i+"]:"+retVal[i]);
		}
		return retVal;
	}
	
	float PitchingBallMgr_getPerfectHitTime( float ballSpeed )
	{
		float time_pitching = 0.0f;
		float time_tillSZone = 0.0f;
		float time_perfect = 0.0f;
		
		/*
		 * How to get ball speed coeficient(x)?
		 * 1000x/3600 : 1 = 18.44 : 0.6
		 * x = 49.36
		 * */
		
		time_pitching = 18.44f / ((ballSpeed-49.36f)*1000/3600);//'49.36' is coefficient to match game base rule. Base unit is meter
		time_tillSZone = (time_pitching*GlobalVar.dist_RP)/Constants.BASE_PITCHING_DIST;//Base unit is inch
		
		Debug.Log( "** Perfect Hit Timing = "+(((time_pitching*GlobalVar.dist_RP)/(Constants.BASE_PITCHING_DIST))+Constants.BATTING_TIME) );
		Debug.Log("PitchingBallMgr_getPerfectHitTime() Pitching Time = "+time_pitching);
		
		time_perfect = time_tillSZone;// - Constants.BATTING_TIME;
		
		return time_perfect;
	}
	
	
	private Vector3[] PitchingBallMgr_getBackToPitcherBall( int frame, int count )
	{
		Vector3[] retVal = new Vector3[frame];
		int cnt;
		double Uv;
		
		cnt = count;
		
		for(int i = 0; i  <frame; i++)
		{
			Uv = ( i / (double)frame);
			double UM = 1.0f - Uv;
			double UM_J = 1.0f;
			double U_J = 1.0f;
			double R_x;
			double R_y;
			double R_z;
						
			for( int j = 0; j < cnt - 1; j++)
			{
				UM_J *= UM;
				U_J *= Uv;
			}			
			
			R_x = bIfcPoints[0].x*UM_J;
			R_y = bIfcPoints[0].y*UM_J;
			R_z = bIfcPoints[0].z*UM_J;
		
			for(int j = 0; j < cnt-2; j++)
			{
				double tU_J = 1.0f;
				double tUM_J = 1.0f;
				
				for(int k = 0; k < j+1; k++)
				{tU_J *= Uv;}
				
				for(int k = 0; k < (cnt-2-j); k++)
				{tUM_J *= UM;}
				
				float tX = bIfcPoints[ j+1 ].x;
				float tY = bIfcPoints[ j+1 ].y;
				float tZ = bIfcPoints[ j+1 ].z;
				
				R_x += (cnt-1) * tU_J * tUM_J * tX;
				R_y += (cnt-1) * tU_J * tUM_J * tY;
				R_z += (cnt-1) * tU_J * tUM_J * tZ;
			}
			R_x += bIfcPoints[cnt-1].x * U_J;
			R_y += bIfcPoints[cnt-1].y * U_J;
			R_z += bIfcPoints[cnt-1].z * U_J;
			
			//Debug.Log( "bIfcPoints[ "+(cnt-1)+" ] = " + bIfcPoints[(cnt-1)] );
			
			Vector3 tVector = new Vector3((float)R_x,(float)R_y,(float)R_z);
			//Debug.Log(tVector);
			retVal[i] = tVector;
		}		
		return retVal;
	}

	// VESTIGE_BALL
	//get Vestige Ball Frame
	int PitchingBallMgr_getVesBallFrame( int ballType )
	{
		int retVal = 0;
		if( GlobalVar.myTeam.P_starting.formIdx == (int)GlobalVar.PITCHER_FORM.OVER )
		{
			switch( ballType )
			{
			case Constants.BALLTYPE_FOURSEAM:
				retVal = 25;
				break;
			case Constants.BALLTYPE_TWOSEAM:
				retVal = 23;
				break;
			case Constants.BALLTYPE_SINKER:
			case Constants.BALLTYPE_CHANGEUP:
			case Constants.BALLTYPE_CUTFB:
			case Constants.BALLTYPE_CIRCLECH:
			case Constants.BALLTYPE_HSLIDER:
				retVal = 21;
				break;
			case Constants.BALLTYPE_PALM:
				retVal = 26;
				break;
			case Constants.BALLTYPE_FORK:
				retVal = 14;
				break;
			case Constants.BALLTYPE_SLIDER:
				retVal = 22;
				break;
			case Constants.BALLTYPE_RISINGFB:
			case Constants.BALLTYPE_SINKINGFB:
				retVal = 15;
				break;
			case Constants.BALLTYPE_KNUCKLE:
				retVal = 30;
				break;
			case Constants.BALLTYPE_SPLITER:
				retVal = 16;
				break;
			case Constants.BALLTYPE_SLOWCV:
				retVal = 19;
				break;
			case Constants.BALLTYPE_UPSHOOT:
				retVal = 27;
				break;
			case Constants.BALLTYPE_FRISBEE:
				retVal = 24;
				break;
			case Constants.BALLTYPE_CURVE:
				retVal = 17;
				break;
			}
		}
		else
		{
			switch( ballType )
			{
			case Constants.BALLTYPE_FOURSEAM:
			case Constants.BALLTYPE_CHANGEUP:
				retVal = 33;
				break;
			case Constants.BALLTYPE_TWOSEAM:
			case Constants.BALLTYPE_SINKER:
			case Constants.BALLTYPE_SLIDER:
				retVal = 32;
				break;
			case Constants.BALLTYPE_FORK:
				retVal = 26;
				break;
			case Constants.BALLTYPE_CUTFB:
				retVal = 30;
				break;
			case Constants.BALLTYPE_RISINGFB:
				retVal = 34;
				break;
			case Constants.BALLTYPE_SINKINGFB:
			case Constants.BALLTYPE_CIRCLECH:
				retVal = 31;
				break;
			case Constants.BALLTYPE_SPLITER:
				retVal = 25;
				break;
			case Constants.BALLTYPE_UPSHOOT:
				retVal = 27;
				break;
			case Constants.BALLTYPE_FRISBEE:
				retVal = 24;
				break;
			case Constants.BALLTYPE_CURVE:
				switch( GlobalVar.myTeam.P_starting.formIdx )
				{
				case (int)GlobalVar.PITCHER_FORM.SIDE:
					retVal = 30;
					break;
				case (int)GlobalVar.PITCHER_FORM.UNDER:
					retVal = 37;
					break;
				}
				break;
			}
		}
		
		return retVal;
	}
	
	//below check is it Strike or Not.
	//True : Strike / Ball : Ball
	bool PitchingBallMgr_bStrike()
	{
		int strikeZoneWid = 10;
		float strikeZoneHt = 11.5f;
		
		if( GlobalVar.ballPosInSZone.x >= GlobalVar.strikeZoneObj.position.x-strikeZoneWid && GlobalVar.ballPosInSZone.x <= GlobalVar.strikeZoneObj.position.x+strikeZoneWid && 
			GlobalVar.ballPosInSZone.y >= GlobalVar.strikeZoneObj.position.y-strikeZoneHt && GlobalVar.ballPosInSZone.y <= GlobalVar.strikeZoneObj.position.y+strikeZoneHt )
		{
			//Debug.Log("Strike !! ");
			return true;
		}
		//Debug.Log("Ball !! ");
		return false;
	}
	

//Create Ball Curve Data (Default)
	public void PitchingBallMgr_createVesBallData( int ballType )
	{
		int cnt;
		//int tBallFrame = 0;
		// VESTIGE_BALL
		int vesBallFrame = 0;//vestige of BallType
		
		//Debug.Log("PitchingBallMgr_create'VES'BallData() ballType="+ballType);
		
		//Debug.Log("inflection = "+inflectionPnt);
		
		cnt = inflectionPnt[ ballType ].Length;
		
		//18.47m is the distance what's Pitching point to catching point
		ballSpeed = PitchingBallMgr_getBallSpeed( ballType );
		//tBallFrame = PitchingBallMgr_getPitchingBallFrame( ballSpeed );
		// VESTIGE_BALL
		vesBallFrame = PitchingBallMgr_getVesBallFrame( ballType );
		// VESTIGE_BALL
		vesBallCrs = new Vector3[ vesBallFrame ];
#if VESTIGE_BALL
		vesBallObj = new GameObject[ 50 ];
#endif
		// VESTIGE_BALL
		vesBallCrs = PitchingBallMgr_getBezierResult( ballType, vesBallFrame, cnt );
		PitchingBallMgr_firstMakeBallDataForArr();
	}
	
	public void PitchingBallMgr_createBallData( int ballType )
	{
		int cnt;
		int tBallFrame = 0;
		// VESTIGE_BALL
		//int vesBallFrame = 0;//vestige of BallType
		
		//Debug.Log("PitchingBallMgr_createBallData() ballType="+ballType);		
		//Debug.Log("inflection = "+inflectionPnt);
		
		cnt = inflectionPnt[ ballType ].Length;
		
		//18.47m is the distance what's Pitching point to catching point
		ballSpeed = PitchingBallMgr_getBallSpeed( ballType );
		tBallFrame = PitchingBallMgr_getPitchingBallFrame( ballSpeed );
		
		txtBallSpeed.Text = (int)ballSpeed+"Km";
		
		bezierResult = new Vector3[ tBallFrame ];
	
		//Debug.Log("inflection point.Length="+cnt+"/"+vesBallFrame);
		bezierResult = PitchingBallMgr_getBezierResult( ballType, tBallFrame, cnt );
		brThirdPnt = bezierResult.Length/3;
		brLast5Pnt = bezierResult.Length-5;
		GlobalVar.time_hitPerfect = PitchingBallMgr_getPerfectHitTime( ballSpeed );
		Debug.Log("Perfect Hit Time = "+GlobalVar.time_hitPerfect +" / ballSpeed = "+ballSpeed);
	}
	
	public void PitchingBallMgr_firstMakeBallDataForArr()
	{
		Vector3 rotAngle = new Vector3(0,0,0);
		Vector3 tmpBallCrs = new Vector3(0,0,0);
		Vector3 tmpBallPos = new Vector3(0,0,0);
		float distX=0.0f, distY=0.0f, distZ=0.0f;
		float dist_RATE = 747.3f;
		if( prePitchingPnt.x > catchingPnt.x )
		{
			distX = GlobalVar.utilMgr.ABS(prePitchingPnt.x + catchingPnt.x*(-1));
		}
		else
		{
			distX = (prePitchingPnt.x - catchingPnt.x);
		}
		
		if( prePitchingPnt.y > catchingPnt.y )
		{
			distY = (prePitchingPnt.y - catchingPnt.y)*(-1);
		}
		else
		{
			distY = GlobalVar.utilMgr.ABS( prePitchingPnt.y + catchingPnt.y*(-1));
		}
		distZ = prePitchingPnt.z;
		
		rotAngle.x = Mathf.Atan2(distX, distZ);
		rotAngle.y = Mathf.Atan2(distY, distZ);
		
		for( int i=0 ; i<vesBallCrs.Length ; i++ )
		{
			tmpBallCrs.x = (Mathf.Tan(rotAngle.x) * vesBallCrs[i].z)*(-1) + vesBallCrs[i].x;
			tmpBallCrs.y = (Mathf.Tan(rotAngle.y) * vesBallCrs[i].z)*(-1) + vesBallCrs[i].y;
			
			tmpBallCrs.x = tmpBallCrs.x/dist_RATE * (prePitchingPnt.z - catchingPnt.z);
			tmpBallCrs.y = tmpBallCrs.y*(-1)/dist_RATE * (prePitchingPnt.z - catchingPnt.z);
			tmpBallCrs.z = vesBallCrs[i].z*(-1)/dist_RATE  * (prePitchingPnt.z - catchingPnt.z);
			//Debug.Log(i+"/"+bezierResult[i].z+"/"+prePitchingPnt.z+"/"+catchingPnt.z+"/sum="+(prePitchingPnt.z - catchingPnt.z)+"/final="+tmpBallCrs.z);
			
			tmpBallPos.x = prePitchingPnt.x + (tmpBallCrs.x);
			tmpBallPos.y = prePitchingPnt.y + (tmpBallCrs.y);
			tmpBallPos.z = prePitchingPnt.z + (tmpBallCrs.z);
			
			if( i== GlobalVar.vesStrikeZoneArrIdx )
			{
				GlobalVar.distFPArrX = GlobalVar.selectionBall.localPosition.x - tmpBallPos.x;
				GlobalVar.distFPArrY = GlobalVar.selectionBall.localPosition.y - tmpBallPos.y;
				return;
			}
		}
	}
	
	public void PitchingBallMgr_finallyMakeBallData()
	{
		Vector3 rotAngle = new Vector3(0,0,0);
		Vector3 tmpBallCrs = new Vector3(0,0,0);
		Vector3 tmpBallPos = new Vector3(0,0,0);
		float distX=0.0f, distY=0.0f, distZ=0.0f;
		float dist_RATE = 747.3f;
		
		//prePitchingPnt => Released point of Pitcher
		if( prePitchingPnt.x > catchingPnt.x )
		{
			distX = GlobalVar.utilMgr.ABS(prePitchingPnt.x + catchingPnt.x*(-1));
		}
		else
		{
			distX = (prePitchingPnt.x - catchingPnt.x);
		}
		
		if( prePitchingPnt.y > catchingPnt.y )
		{
			distY = (prePitchingPnt.y - catchingPnt.y)*(-1);
		}
		else
		{
			distY = GlobalVar.utilMgr.ABS( prePitchingPnt.y + catchingPnt.y*(-1));
		}
		distZ = prePitchingPnt.z;
		
		rotAngle.x = Mathf.Atan2(distX, distZ);
		rotAngle.y = Mathf.Atan2(distY, distZ);
		
		for( int i=0 ; i<bezierResult.Length ; i++ )
		{
			tmpBallCrs.x = (Mathf.Tan(rotAngle.x) * bezierResult[i].z)*(-1) + bezierResult[i].x;
			tmpBallCrs.y = (Mathf.Tan(rotAngle.y) * bezierResult[i].z)*(-1) + bezierResult[i].y;
			
			tmpBallCrs.x = tmpBallCrs.x/dist_RATE * (prePitchingPnt.z - catchingPnt.z);
			tmpBallCrs.y = tmpBallCrs.y*(-1)/dist_RATE * (prePitchingPnt.z - catchingPnt.z);
			tmpBallCrs.z = bezierResult[i].z*(-1)/dist_RATE  * (prePitchingPnt.z - catchingPnt.z);
			
			tmpBallPos.x = prePitchingPnt.x + (tmpBallCrs.x);
			tmpBallPos.y = prePitchingPnt.y + (tmpBallCrs.y);
			tmpBallPos.z = prePitchingPnt.z + (tmpBallCrs.z);
			
			bezierResult[i].x = tmpBallPos.x;
			if( tmpBallPos.y>=0 )
			{
				bezierResult[i].y = tmpBallPos.y;
			}
			else
			{
				bezierResult[i].y = tmpBallPos.y*(-1);
			}
			bezierResult[i].z = tmpBallPos.z;
		}

		for( int i=0 ; i<vesBallCrs.Length ; i++ )
		{
			tmpBallCrs.x = (Mathf.Tan(rotAngle.x) * vesBallCrs[i].z)*(-1) + vesBallCrs[i].x;
			tmpBallCrs.y = (Mathf.Tan(rotAngle.y) * vesBallCrs[i].z)*(-1) + vesBallCrs[i].y;
			
			tmpBallCrs.x = tmpBallCrs.x/dist_RATE * (prePitchingPnt.z - catchingPnt.z);
			tmpBallCrs.y = tmpBallCrs.y*(-1)/dist_RATE * (prePitchingPnt.z - catchingPnt.z);
			tmpBallCrs.z = vesBallCrs[i].z*(-1)/dist_RATE  * (prePitchingPnt.z - catchingPnt.z);
			//Debug.Log(i+"/"+bezierResult[i].z+"/"+prePitchingPnt.z+"/"+catchingPnt.z+"/sum="+(prePitchingPnt.z - catchingPnt.z)+"/final="+tmpBallCrs.z);
			
			tmpBallPos.x = prePitchingPnt.x + (tmpBallCrs.x);
			tmpBallPos.y = prePitchingPnt.y + (tmpBallCrs.y);
			tmpBallPos.z = prePitchingPnt.z + (tmpBallCrs.z);
			
			vesBallCrs[i].x = tmpBallPos.x;
			if(tmpBallPos.y<0)tmpBallPos.y*=(-1);
			vesBallCrs[i].y = tmpBallPos.y;
			
			vesBallCrs[i].z = tmpBallPos.z;

#if VESTIGE_BALL		
			vesBallObj[i] = (GameObject)Instantiate(Resources.Load("Prefab/S_GameProc/Temp/vesBall"), new Vector3(vesBallCrs[i].x, vesBallCrs[i].y, vesBallCrs[i].z), Quaternion.identity );
			vesBallObj[i].transform.name = i+"vesBall";	
#endif
		}
		
		//Result Ball Effect Setup
		PitchingBallMgr_setupSzEff();
		PitchingBallMgr_setSzBallTracePos();
	}
	
	void PitchingBallMgr_finallyMakeBackToPitcherBall()
	{
		Vector3 rotAngle = new Vector3(0,0,0);
		Vector3 tmpBallCrs = new Vector3(0,0,0);
		Vector3 tmpBallPos = new Vector3(0,0,0);
		float distX=0.0f, distY=0.0f, distZ=0.0f;
		float dist_RATE = 1;//705;
		//GameObject a;
		
		//get position of trace in strike zone
		distX = btPitPos.x>btCatPos.x ? -(btPitPos.x - btCatPos.x): (btPitPos.x - btCatPos.x);
		distY = btPitPos.y>btCatPos.y ? -(btPitPos.y - btCatPos.y): (btPitPos.x - btCatPos.y);
		distZ = btPitPos.z;
		
		rotAngle.x = Mathf.Atan2(distX, distZ);
		rotAngle.y = Mathf.Atan2(distY, distZ);
		for( int i=0 ; i<backToPitcherPos.Length ; i++ )
		{
			tmpBallCrs.x = -(Mathf.Tan(rotAngle.x) * backToPitcherPos[i].z) + backToPitcherPos[i].x;
			tmpBallCrs.y = -(Mathf.Tan(rotAngle.y) * backToPitcherPos[i].z)+ backToPitcherPos[i].y;
			tmpBallCrs.z = backToPitcherPos[i].z;
						
			tmpBallPos.x = btPitPos.x + (tmpBallCrs.x/dist_RATE);
			tmpBallPos.y = btPitPos.y - (tmpBallCrs.y/dist_RATE);
			tmpBallPos.z = btPitPos.z - (tmpBallCrs.z/dist_RATE);
			
			backToPitcherPos[i].x = tmpBallPos.x;
			backToPitcherPos[i].y = tmpBallPos.y;
			backToPitcherPos[i].z = tmpBallPos.z;	
			
			//a = (GameObject)Instantiate(Resources.Load("Prefab/Temp/vesBall"), new Vector3(backToPitcherPos[i].x, backToPitcherPos[i].y, backToPitcherPos[i].z), Quaternion.identity );
		}
	}
	
	
	
//Process
	public void PitchingBallMgr_pitching()
	{
		Vector3 tmpBallPos = new Vector3(0,0,0);
		
		
		//Debug.Log(pingBallFrmCnt+"/"+bezierResult.Length);
		if( pingBallFrmCnt < bezierResult.Length )
		{
			tmpBallPos.x = bezierResult[ pingBallFrmCnt ].x;
			tmpBallPos.y = bezierResult[ pingBallFrmCnt ].y;
			if( tmpBallPos.y < 0 ) tmpBallPos.y *= (-1);
			tmpBallPos.z = bezierResult[ pingBallFrmCnt ].z;
			
			
			GlobalVar.pBallObj.position = tmpBallPos;
			pingBallFrmCnt++;
			tmpBallPos.y = 1.5f;
			GlobalVar.pBallShadowObj.transform.position = tmpBallPos;
			
			if(pingBallFrmCnt>=brThirdPnt)//( bezierResult.Length/3<=pingBallFrmCnt )
			{
				GlobalVar.bStartCatAnim = true;
			}
			
			
			
			PitchingBallMgr_ballRotation();
			
			if( GlobalVar.control_Status==Constants.STATE_CG_BAD && GlobalVar.badBallCrsRandom==0 )
			{
				if(pingBallFrmCnt < brLast5Pnt)//( pingBallFrmCnt < bezierResult.Length-5 )
				{
					if( badFrmCnt>=0 && badFrmCnt<5 )
					{
						signGBObj.SetActiveRecursively(false);
					}
					//On
					else if( badFrmCnt>=5 && badFrmCnt<10 )
					{
						signGBObj.SetActiveRecursively(true);
					}
					badFrmCnt++;
					if( badFrmCnt>=10 ) badFrmCnt=0;
				}
			}
			
			//Debug.Log(GlobalVar.pBallObj.position);
		}
		else
		{
			bStartSzEff = true;
			PitchingBallMgr_setFinishPitching();
		}
		
		if( bStartSzEff )
		{
			PitchingBallMgr_renderStrikeZoneEff();
		}
	}
	
	void PitchingBallMgr_ballRotation()
	{
		Vector3 tmpBallRot = new Vector3(0,0,0);
		
		switch( GlobalVar.myTeam.P_starting.formIdx )
		{
		case (int)GlobalVar.PITCHER_FORM.OVER:			
			switch( GlobalVar.ballTypeSelNum )
			{
			case Constants.BALLTYPE_KNUCKLE:
				pBallRotRate += 3;
				tmpBallRot.x = pBallRotRate;
				tmpBallRot.z = 270;
				break;
			case Constants.BALLTYPE_PALM:
				pBallRotRate += 10;
				tmpBallRot.x = pBallRotRate;
				tmpBallRot.z = 270;
				break;
			case Constants.BALLTYPE_CURVE:
			case Constants.BALLTYPE_SLOWCV:
				pBallRotRate -= 32;
				tmpBallRot.x = pBallRotRate;
				tmpBallRot.z = 270;
				break;
			case Constants.BALLTYPE_SLIDER:
			case Constants.BALLTYPE_HSLIDER:
				pBallRotRate += 93;
				tmpBallRot.x = 90;
				tmpBallRot.z = pBallRotRate;
				break;
			default:
				pBallRotRate += 32;
				tmpBallRot.x = pBallRotRate;
				tmpBallRot.z = 270;
				break;
			}
			if( pBallRotRate>360 ) pBallRotRate=0;
			break;
		case (int)GlobalVar.PITCHER_FORM.SIDE:
		case (int)GlobalVar.PITCHER_FORM.UNDER:
			pBallRotRate += 36;
			if( pBallRotRate>360 ) pBallRotRate=0;
			tmpBallRot.y = pBallRotRate;
			break;
		}
		
		GlobalVar.pBallObj.localEulerAngles = tmpBallRot;
	}
	
	void PitchingBallMgr_renderStrikeZoneEff()
	{
		//Vector3 tmpVec = new Vector3(0,0,0);
		//UI Change	
		if( GlobalVar.ballTypeSelNum == Constants.BALLTYPE_FOURSEAM )
		{
			switch( szEffFrmCnt )
			{
			case 0:
				pBallFlyTime = Time.time;// - GlobalVar.time_RlsPnt;
				Debug.Log("Pitching Time = "+(pBallFlyTime-GlobalVar.time_RlsPnt)+"/ now Time="+pBallFlyTime);
				
				szEffObj[0].SetActiveRecursively(true);//box Eff 1st
				szEffObj[3].SetActiveRecursively(true);//white ball
				//szEffFrmCnt = 1;
				break;
			case 1:
				szEffObj[0].SetActiveRecursively(false);//box Eff 1st
				szEffObj[3].SetActiveRecursively(false);//white ball
				
				szEffObj[1].SetActiveRecursively(true);//box Eff 2nd
				szEffObj[4].SetActiveRecursively(true);//blue ball 1st
				szEffObj[8].SetActiveRecursively(true);//ballBG 1st
				szEffObj[10].SetActiveRecursively(true);//cleared image ball
				//throw new System.ArgumentOutOfRangeException();
				//szEffFrmCnt = 2;
				break;
			case 2:
				szEffObj[1].SetActiveRecursively(false);//box Eff 2nd
				szEffObj[4].SetActiveRecursively(false);//blue ball 1st
				szEffObj[2].SetActiveRecursively(true);//box Eff 3rd
				szEffObj[5].SetActiveRecursively(true);//blue ball 2nd
				//8, 10 idx still rendering
				//szEffFrmCnt = 3;
				break;
			case 3:
				szEffObj[2].SetActiveRecursively(false);//box Eff 3rd
				szEffObj[5].SetActiveRecursively(false);//blue ball 2nd
				szEffObj[6].SetActiveRecursively(true);//blue ball 3rd
				//8, 10 idx still rendering
				//szEffFrmCnt = 4;
				break;
			case 4:
				szEffObj[6].SetActiveRecursively(false);//blue ball 3rd
				szEffObj[7].SetActiveRecursively(true);//blue ball 4th
				//8, 10 idx still rendering
				//szEffFrmCnt = 5;
				break;
			case 5:
				szEffObj[7].SetActiveRecursively(false);//blue ball 4th
				break;
			}
			szEffFrmCnt++;
			
			if( szEffFrmCnt>26 )
			{
				szEffFrmCnt=26;
				bStartSzEff = false;				
				GlobalVar.bStartBackToPit = true;
			}
			//Debug.Log("EffCnt="+szEffFrmCnt+"/backtopit="+GlobalVar.bStartBackToPit);
		}
		else
		{
			if( szEffFrmCnt==0 )
			{
				szEffObj[0].SetActiveRecursively(true);//box Eff 1st
				szEffObj[3].SetActiveRecursively(true);//white ball
			}
			else if( szEffFrmCnt==1 )
			{
				szEffObj[0].SetActiveRecursively(false);//box Eff 1st
				szEffObj[3].SetActiveRecursively(false);//white ball
				
				szEffObj[1].SetActiveRecursively(true);//box Eff 2nd
				szEffObj[4].SetActiveRecursively(true);//blue ball 1st
				szEffObj[8].SetActiveRecursively(true);//ballBG 1st
				szEffObj[10].SetActiveRecursively(true);//cleared image ball
			}
			else if( szEffFrmCnt==2 )
			{
				szEffObj[1].SetActiveRecursively(false);//box Eff 2nd
				szEffObj[4].SetActiveRecursively(false);//blue ball 1st
				szEffObj[2].SetActiveRecursively(true);//box Eff 3rd
				szEffObj[5].SetActiveRecursively(true);//blue ball 2nd
			}
			else if( szEffFrmCnt==3 )
			{
				szEffObj[2].SetActiveRecursively(false);//box Eff 3rd
				szEffObj[5].SetActiveRecursively(false);//blue ball 2nd
				szEffObj[6].SetActiveRecursively(true);//blue ball 3rd
				//8 idx still rendering
				//szEffFrmCnt = 4;
			}
			else if( szEffFrmCnt==4 )
			{
				szEffObj[6].SetActiveRecursively(false);//blue ball 3rd
				szEffObj[7].SetActiveRecursively(true);//blue ball 4th
				//8 idx still rendering
				//szEffFrmCnt = 5;
			}
			else if( szEffFrmCnt==5 )
			{
				szEffObj[7].SetActiveRecursively(false);//blue ball 4th
				//8 idx still rendering
				//szEffFrmCnt = 6;
			}
			else if( szEffFrmCnt==6 )
			{
				if( GlobalVar.control_Status != Constants.STATE_CG_BAD )
				{
					szEffObj[9].SetActiveRecursively(true);//point Ball Bg
					szEffObj[11].SetActiveRecursively(true);//blurred ball
				}
				//throw new System.ArgumentOutOfRangeException();
				//8 idx still rendering				
				//szEffFrmCnt = 6;
			}
			else if( szEffFrmCnt>=17+10 && szEffFrmCnt<=25+10 )
			{
				if( GlobalVar.control_Status != Constants.STATE_CG_BAD )
				{
					//8,9,11 idx still rendering
					//11 idx is moving it'll 9 Frame
					//tmpVec = resultTracePos[szEffFrmCnt-27];
					szEffObj[11].transform.position = resultTracePos[szEffFrmCnt-27];
				}
			}		
			else if( szEffFrmCnt==40 )
			{
				if( GlobalVar.control_Status != Constants.STATE_CG_BAD )
				{
					szEffObj[11].SetActiveRecursively(false);//blurred ball
					//throw new System.ArgumentOutOfRangeException();
				}
			}
			
			szEffFrmCnt++;
			if( szEffFrmCnt>56 )
			{
				szEffFrmCnt=56;
				bStartSzEff = false;	
				GlobalVar.bStartBackToPit = true;
				//PitchingBallMgr_setBallTraceActiveReculsive(true);
				txtBallSpeedObj.gameObject.SetActiveRecursively(false);
				//GlobalVar.pBallObj.gameObject.SetActiveRecursively(true);
				//GlobalVar.pBallShadowObj.SetActiveRecursively(true);
			}
				
		}
		
		//Temporary
		if(	GlobalVar.bStartBackToPit )
		{
			GlobalVar.GAMEPROC_STATE = Constants.STATE_GAMEPROC_PITCHER_RETURN;
		}
	}
	
	public void PitchingBallMgr_renderBallBackToPit()
	{
		Vector3 tmpPos = new Vector3(0,0,0);
		Vector3 tmpRot = new Vector3(0,0,0);
		
		//Debug.Log("backtoPit() btBallFrmCnt="+btBallFrmCnt+"/Length="+backToPitcherPos.Length);
		
		if( btBallFrmCnt<backToPitcherPos.Length )
		{
			
			if( btBallFrmCnt==0 )
			{
				GlobalVar.pBallObj.gameObject.SetActiveRecursively(true);
				GlobalVar.pBallShadowObj.SetActiveRecursively(true);
			}
			
			if( btBallFrmCnt>backToPitcherPos.Length/2)
			{
				szEffObj[8].SetActiveRecursively(false);//ballBG 1st
				szEffObj[10].SetActiveRecursively(false);//cleared image ball
				szEffObj[9].SetActiveRecursively(false);
			}
			GlobalVar.pBallObj.position = backToPitcherPos[(backToPitcherPos.Length-1)-btBallFrmCnt];
			tmpPos = backToPitcherPos[(backToPitcherPos.Length-1)-btBallFrmCnt];
			tmpPos.y = 1.5f;
			GlobalVar.pBallShadowObj.transform.position = tmpPos;
			pBallRotRate += 36;
			if( pBallRotRate>360 ) pBallRotRate=0;
			tmpRot.x = pBallRotRate;
			tmpRot.z = 270;
			GlobalVar.pBallObj.eulerAngles = tmpRot;
			btBallFrmCnt++;
			//throw new System.ArgumentException();
		}
		else
		{
			PitchingBallMgr_setFinishBallCatToPit();
			GlobalVar.GAMEPROC_STATE = Constants.STATE_GAMEPROC_PITCHER_FINISH;
			//Debug.Log("######################### Status go to Finish");
		}
	}
}
