using UnityEngine;
using System.Collections;

public class HitEngineMgr : MonoBehaviour {
	
	//temp
	Transform tmpTxtAngle;
	UISysFontLabel txtAngle;
	
	string[] hitTimingStr = new string[]
	{
		"Very Early", "Early", "Good", "Perfect", "Late", "Very Late"
	};
	float[] rangeOfHT;//0:Early/Late, 1:Good, 2:Perfect
	
	int[] betLocExcVal;
	
	//Transform txtHitTimingObj;
	UISysFontLabel txtHitTiming;
	
	// Use this for initialization
	void Start () {
		Debug.Log("HitEngineMgr.Start()");
		GlobalVar.txtHitTimingObj = GlobalVar.playUIObj.FindChild("UI_Cam").FindChild("Panel").FindChild("Anchor_BtmRight").FindChild("PitchingMachine").FindChild("Txt_HitTiming");
		GlobalVar.txtHitTimingObj.gameObject.SetActiveRecursively(false);
		txtHitTiming = GlobalVar.txtHitTimingObj.GetComponent<UISysFontLabel>();		
		
		betLocExcVal = new int[6]{
			Constants.BALLTYPE_FOURSEAM, Constants.BALLTYPE_TWOSEAM, Constants.BALLTYPE_RISINGFB, Constants.BALLTYPE_HSLIDER,
			Constants.BALLTYPE_UPSHOOT, Constants.BALLTYPE_CURVE
		};
		
		
		//temp
		tmpTxtAngle = GlobalVar.playUIObj.FindChild("UI_Cam").FindChild("Panel").FindChild("Anchor_BtmRight").FindChild("PitchingMachine").FindChild("tmpTxtAngle");
		tmpTxtAngle.gameObject.SetActiveRecursively(false);
		txtAngle = tmpTxtAngle.GetComponent<UISysFontLabel>();
		
		
		
		rangeOfHT = new float[3];
		HitEngineMgr_setHitting();
	}
	
//Set
	void HitEngineMgr_setHitting()
	{
		HitEnginemgr_setHitTimingArea();
	}
	
	void HitEnginemgr_setHitTimingArea()
	{
		int accVar = 36;
		int areaVar = 5100;
		int wgt_perfect = 1;
		int wgt_good = 3;
		int wgt_el = 5;
		float hCalVar = 0.0f;
		
		hCalVar = (GlobalVar.myTeam.H_starting[GlobalVar.battingNum].ability_Accuracy + accVar);
		
		rangeOfHT[0] = hCalVar*wgt_el / areaVar;		//Early, Late Timing Area
		rangeOfHT[1] = hCalVar*wgt_good / areaVar;		//Good Timing Area
		rangeOfHT[2] = hCalVar*wgt_perfect / areaVar;	//Perfect Timing Area
		
		Debug.Log("Early/Late : "+rangeOfHT[0]);
		Debug.Log("Good : "+rangeOfHT[1]);
		Debug.Log("Perfect : "+rangeOfHT[2]);
	}
	
	void HitEngineMgr_setTiming()
	{
		float betPos = 0.0f;
		float hitAngle = 0.0f;
		bool bExcLoc = false;	
		
		//comment about area of each timing
		Debug.Log("Hit timing = "+GlobalVar.time_hitPrs+" / Prefect Timing="+GlobalVar.time_hitPerfect);
		Debug.Log("EARLY Area="+(GlobalVar.time_hitPerfect-rangeOfHT[0])+"~"+(GlobalVar.time_hitPerfect-rangeOfHT[1])+" / area:"+GlobalVar.myTeam.H_starting[GlobalVar.battingNum].rangeArea[0]+" ~ "+GlobalVar.myTeam.H_starting[GlobalVar.battingNum].rangeArea[1]);
		Debug.Log("GOOD_early Area="+(GlobalVar.time_hitPerfect-rangeOfHT[1])+"~"+(GlobalVar.time_hitPerfect-rangeOfHT[2])+" / area:"+GlobalVar.myTeam.H_starting[GlobalVar.battingNum].rangeArea[1]+" ~ "+GlobalVar.myTeam.H_starting[GlobalVar.battingNum].rangeArea[2]);
		Debug.Log("Perfect Area="+(GlobalVar.time_hitPerfect-rangeOfHT[2])+"~"+(GlobalVar.time_hitPerfect+rangeOfHT[2])+" / area:"+GlobalVar.myTeam.H_starting[GlobalVar.battingNum].rangeArea[2]+" ~ "+GlobalVar.myTeam.H_starting[GlobalVar.battingNum].rangeArea[3]);
		Debug.Log("GOOD_late Area="+GlobalVar.time_hitPerfect+rangeOfHT[2]+"~"+GlobalVar.time_hitPerfect+rangeOfHT[1]+" / area:"+GlobalVar.myTeam.H_starting[GlobalVar.battingNum].rangeArea[3]+" ~ "+GlobalVar.myTeam.H_starting[GlobalVar.battingNum].rangeArea[4]);
		Debug.Log("Late Area="+GlobalVar.time_hitPerfect+rangeOfHT[1]+"~"+GlobalVar.time_hitPerfect+rangeOfHT[0]+" / area:"+GlobalVar.myTeam.H_starting[GlobalVar.battingNum].rangeArea[4]+" ~ "+GlobalVar.myTeam.H_starting[GlobalVar.battingNum].rangeArea[5]);
		
		
		//PERFECT
		if( GlobalVar.time_hitPrs>=GlobalVar.time_hitPerfect-rangeOfHT[2] && GlobalVar.time_hitPrs<=GlobalVar.time_hitPerfect+rangeOfHT[2] )
		{
			Debug.Log("Hit Timing Perfect!!");
			txtHitTiming.Text = hitTimingStr[3];			
			betPos = Random.Range( GlobalVar.myTeam.H_starting[GlobalVar.battingNum].rangeArea[2], GlobalVar.myTeam.H_starting[GlobalVar.battingNum].rangeArea[3] );
		}
		//GOOD Early
		else if( GlobalVar.time_hitPrs>=GlobalVar.time_hitPerfect-rangeOfHT[1] && GlobalVar.time_hitPrs<GlobalVar.time_hitPerfect-rangeOfHT[2] )
		{
			Debug.Log("Hit Timing Good-early");
			txtHitTiming.Text = hitTimingStr[2]+"(early)";
			bExcLoc = HitEngineMgr_bExchangeBetLoc();
			if( bExcLoc )
			{
				betPos = Random.Range( GlobalVar.myTeam.H_starting[GlobalVar.battingNum].rangeArea[1], GlobalVar.myTeam.H_starting[GlobalVar.battingNum].rangeArea[2] );
			}
			else
			{
				betPos = Random.Range( GlobalVar.myTeam.H_starting[GlobalVar.battingNum].rangeArea[3], GlobalVar.myTeam.H_starting[GlobalVar.battingNum].rangeArea[4] );
			}
		}
		//GOOD Late
		else if( GlobalVar.time_hitPrs>GlobalVar.time_hitPerfect+rangeOfHT[2] && GlobalVar.time_hitPrs<=GlobalVar.time_hitPerfect+rangeOfHT[1] )
		{
			Debug.Log("Hit Timing Good-late");
			txtHitTiming.Text = hitTimingStr[2]+"(late)";
			bExcLoc = HitEngineMgr_bExchangeBetLoc();
			if( bExcLoc )
			{
				betPos = Random.Range( GlobalVar.myTeam.H_starting[GlobalVar.battingNum].rangeArea[3], GlobalVar.myTeam.H_starting[GlobalVar.battingNum].rangeArea[4] );
			}
			else
			{
				betPos = Random.Range( GlobalVar.myTeam.H_starting[GlobalVar.battingNum].rangeArea[1], GlobalVar.myTeam.H_starting[GlobalVar.battingNum].rangeArea[2] );
			}
		}
		//EARLY
		else if( GlobalVar.time_hitPrs>=GlobalVar.time_hitPerfect-rangeOfHT[0] && GlobalVar.time_hitPrs<GlobalVar.time_hitPerfect-rangeOfHT[1] )
		{
			Debug.Log("Hit Timing Early");
			txtHitTiming.Text = hitTimingStr[1];
			bExcLoc = HitEngineMgr_bExchangeBetLoc();
			if( bExcLoc )
			{
				betPos = Random.Range( GlobalVar.myTeam.H_starting[GlobalVar.battingNum].rangeArea[0], GlobalVar.myTeam.H_starting[GlobalVar.battingNum].rangeArea[1] );
			}
			else
			{
				betPos = Random.Range( GlobalVar.myTeam.H_starting[GlobalVar.battingNum].rangeArea[4], GlobalVar.myTeam.H_starting[GlobalVar.battingNum].rangeArea[5] );
			}
		}
		//LATE
		else if( GlobalVar.time_hitPrs>GlobalVar.time_hitPerfect+rangeOfHT[1] && GlobalVar.time_hitPrs<=GlobalVar.time_hitPerfect+rangeOfHT[0] )
		{
			Debug.Log("Hit Timing Late");
			txtHitTiming.Text = hitTimingStr[4];
			bExcLoc = HitEngineMgr_bExchangeBetLoc();
			if( bExcLoc )
			{
				betPos = Random.Range( GlobalVar.myTeam.H_starting[GlobalVar.battingNum].rangeArea[4], GlobalVar.myTeam.H_starting[GlobalVar.battingNum].rangeArea[5] );
			}
			else
			{
				betPos = Random.Range( GlobalVar.myTeam.H_starting[GlobalVar.battingNum].rangeArea[0], GlobalVar.myTeam.H_starting[GlobalVar.battingNum].rangeArea[1] );
			}
		}
		//VERY EARLY
		else if( GlobalVar.time_hitPrs<GlobalVar.time_hitPerfect-rangeOfHT[0] )
		{
			Debug.Log("Hit Timing Very early");
			txtHitTiming.Text = hitTimingStr[0];
		}
		//VERY LATE
		else if( GlobalVar.time_hitPrs>GlobalVar.time_hitPerfect+rangeOfHT[0] )
		{
			Debug.Log("Hit Timing Very late");
			txtHitTiming.Text = hitTimingStr[5];
		}
		
		hitAngle = ( 360*betPos )/21.98f;
		Debug.Log("Y angle:"+hitAngle+"/ betPos:"+betPos+"///"+(( 360*1.9f )/21.98f));
		
		txtAngle.Text = "Y angle:"+hitAngle;
	}
	
	bool HitEngineMgr_bExchangeBetLoc()
	{
		int len = 0;
		//Im defencing
		if( GlobalVar.myTeam.bBottom==1 )
		{
			if( GlobalVar.myTeam.P_starting.formIdx == (int)GlobalVar.PITCHER_FORM.UNDER )	len = 6;
			else	len = 5;
		}
		else
		{
			if( GlobalVar.myTeam.P_starting_agnst.formIdx == (int)GlobalVar.PITCHER_FORM.UNDER )	len = 6;
			else	len = 5;
		}
		
		for( int i=0 ; i<len ; i++ )
		{
			if( GlobalVar.ballTypeSelNum == betLocExcVal[i] )	return true;
		}
		return false;
	}
	
	
	public void HitEngineMgr_swingByTouchDown()
	{
		GlobalVar.statusAnim_HITTER = Constants.ANISTATUS_HITTER_SWING;
		GlobalVar.aniTMgr.AnimationTootaMgr_setHitterAnimByStatus();
		GlobalVar.time_hitPrs = Time.time - GlobalVar.time_RlsPnt;
		Debug.Log("Hit Time = "+GlobalVar.time_hitPrs);
		HitEngineMgr_setTiming();
	}
}
