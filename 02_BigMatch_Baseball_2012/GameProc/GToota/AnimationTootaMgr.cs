using UnityEngine;
using System.Collections;

public class AnimationTootaMgr : MonoBehaviour {
	Transform ballInGlvObj;
	Transform ballInCGlvObj;
	Transform ballInCHandObj;
	
	//Pitcher
	string namePHd;
	string namePHd_windUp;//header name of animation of wind up
	
	//Hitter
	string nameHHd;	
	
	//Catcher
	string nameCHd;
	
	int P_formIdx;
	int P_handIdx;
	
	public void AnimationTootaMgr_initialize()
	{
		ballInGlvObj.gameObject.SetActiveRecursively(true);
		ballInCGlvObj.gameObject.SetActiveRecursively(false);
	}
	
	// Use this for initialization
	void Start () {
		GlobalVar.battingNum = 0;
		GlobalVar.battingNum_Pre = GlobalVar.battingNum;//-1;
		
		
		//Debug.Log("Prefab/S_GameProc/Players/P0"+GlobalVar.myTeam.P_starting.bodyIdx+"/CH0"+GlobalVar.myTeam.P_starting.bodyIdx+"_Pitcher_HDI0"+GlobalVar.myTeam.P_starting.handDir);
		
		AnimationTootaMgr_createPlayerWithAni();
		AnimationTootaMgr_initialize();
		AnimationTootaMgr_initAnimState();
	}
	
	// Update is called once per frame
	void Update () {}
	
//Initialize
	void AnimationTootaMgr_createPlayerWithAni()
	{
		//I'm Defence
		if( GlobalVar.myTeam.bBottom==1 )
		{		
			namePHd = "Ani_CI0"+GlobalVar.myTeam.P_starting.bodyIdx+"_HDI0"+GlobalVar.myTeam.P_starting.handDir;
			namePHd_windUp = "Ani_CI0"+GlobalVar.myTeam.P_starting.bodyIdx+"_F0"+GlobalVar.myTeam.P_starting.formIdx+"_HDI0"+GlobalVar.myTeam.P_starting.handDir;
			nameHHd = "Ani_CI0"+GlobalVar.myTeam.H_starting_agnst[GlobalVar.battingNum].bodyIdx+"_Hitter_HDI0"+GlobalVar.myTeam.H_starting_agnst[GlobalVar.battingNum].handDir;
			nameCHd = "Ani_CI0"+GlobalVar.myTeam.H_starting[GlobalVar.myTeam.idx_catcher].bodyIdx+"_Catcher_HDI00";
			
			//Pitcher
			GlobalVar.pitcherObj = (GameObject)Instantiate(Resources.Load("Prefab/S_GameProc/Players/P0"+GlobalVar.myTeam.P_starting.bodyIdx+"/CH0"+GlobalVar.myTeam.P_starting.bodyIdx+"_Pitcher_HDI0"+GlobalVar.myTeam.P_starting.handDir));
			GlobalVar.pitcherObj.transform.parent = GlobalVar.playGroundObj.FindChild("Players");
			GlobalVar.pitcherObj.name = "CH0"+GlobalVar.myTeam.P_starting.bodyIdx+"_Pitcher_HDI0"+GlobalVar.myTeam.P_starting.handDir;

			//Catcher
			GlobalVar.catcherObj = (GameObject)Instantiate(Resources.Load("Prefab/S_GameProc/Players/P0"+GlobalVar.myTeam.H_starting[GlobalVar.myTeam.idx_catcher].bodyIdx+"/CH0"+GlobalVar.myTeam.H_starting[GlobalVar.myTeam.idx_catcher].bodyIdx+"_Catcher_HDI00"));
			GlobalVar.catcherObj.transform.parent = GlobalVar.playGroundObj.FindChild("Players");
			GlobalVar.catcherObj.name = "CH0"+GlobalVar.myTeam.H_starting[GlobalVar.myTeam.idx_catcher].bodyIdx+"_Catcher_HDI00";

			//Hitter - Against
			GlobalVar.hitterObj = (GameObject)Instantiate(Resources.Load("Prefab/S_GameProc/Players/P0"+GlobalVar.myTeam.H_starting_agnst[GlobalVar.battingNum].bodyIdx+"/CH0"+GlobalVar.myTeam.H_starting_agnst[GlobalVar.battingNum].bodyIdx+"_Hitter_HDI0"+GlobalVar.myTeam.H_starting_agnst[GlobalVar.battingNum].handDir));
			GlobalVar.hitterObj.transform.parent = GlobalVar.playGroundObj.FindChild("Players");
			GlobalVar.hitterObj.name = "CH0"+GlobalVar.myTeam.H_starting_agnst[GlobalVar.battingNum].bodyIdx+"_Hitter_HDI0"+GlobalVar.myTeam.H_starting_agnst[GlobalVar.battingNum].handDir;
		}
		//I'm Attack
		else
		{
			namePHd = "Ani_CI0"+GlobalVar.myTeam.P_starting_agnst.bodyIdx+"_HDI0"+GlobalVar.myTeam.P_starting_agnst.handDir;
			namePHd_windUp = "Ani_CI0"+GlobalVar.myTeam.P_starting_agnst.bodyIdx+"_F0"+GlobalVar.myTeam.P_starting_agnst.formIdx+"_HDI0"+GlobalVar.myTeam.P_starting_agnst.handDir;
			nameHHd = "Ani_CI0"+GlobalVar.myTeam.H_starting[GlobalVar.battingNum].bodyIdx+"_Hitter_HDI0"+GlobalVar.myTeam.H_starting[GlobalVar.battingNum].handDir;
			nameCHd = "Ani_CI0"+GlobalVar.myTeam.H_starting_agnst[GlobalVar.myTeam.idx_catcher_agnst].bodyIdx+"_Catcher_HDI00";

			//Hitter
			GlobalVar.hitterObj = (GameObject)Instantiate(Resources.Load("Prefab/S_GameProc/Players/P0"+GlobalVar.myTeam.H_starting[GlobalVar.battingNum].bodyIdx+"/CH0"+GlobalVar.myTeam.H_starting[GlobalVar.battingNum].bodyIdx+"_Hitter_HDI0"+GlobalVar.myTeam.H_starting[GlobalVar.battingNum].handDir));
			GlobalVar.hitterObj.transform.parent = GlobalVar.playGroundObj.FindChild("Players");
			GlobalVar.hitterObj.name = "CH0"+GlobalVar.myTeam.H_starting[GlobalVar.battingNum].bodyIdx+"_Hitter_HDI0"+GlobalVar.myTeam.H_starting[GlobalVar.battingNum].handDir;

			//Pitcher - Against
			GlobalVar.pitcherObj = (GameObject)Instantiate(Resources.Load("Prefab/S_GameProc/Players/P0"+GlobalVar.myTeam.P_starting_agnst.bodyIdx+"/CH0"+GlobalVar.myTeam.P_starting_agnst.bodyIdx+"_Pitcher_HDI0"+GlobalVar.myTeam.P_starting_agnst.handDir));
			GlobalVar.pitcherObj.transform.parent = GlobalVar.playGroundObj.FindChild("Players");
			GlobalVar.pitcherObj.name = "CH0"+GlobalVar.myTeam.P_starting_agnst.bodyIdx+"_Pitcher_HDI0"+GlobalVar.myTeam.P_starting_agnst.handDir;
			
			//Catcher - Against
			GlobalVar.catcherObj = (GameObject)Instantiate(Resources.Load("Prefab/S_GameProc/Players/P0"+GlobalVar.myTeam.H_starting_agnst[GlobalVar.myTeam.idx_catcher_agnst].bodyIdx+"/CH0"+GlobalVar.myTeam.H_starting_agnst[GlobalVar.myTeam.idx_catcher_agnst].bodyIdx+"_Catcher_HDI00"));
			GlobalVar.catcherObj.transform.parent = GlobalVar.playGroundObj.FindChild("Players");
			GlobalVar.catcherObj.name = "CH0"+GlobalVar.myTeam.H_starting_agnst[GlobalVar.myTeam.idx_catcher_agnst].bodyIdx+"_Catcher_HDI00";

		}
		//Pitcher has ball
		ballInGlvObj = GlobalVar.pitcherObj.transform.FindChild("Bip002").FindChild("Bip002 Pelvis").FindChild("Bip002 Spine").FindChild("Bip002 Spine1").FindChild("Bip002 Spine2").FindChild("Bip002 Spine3").FindChild("Bip002 Neck").
						FindChild("Bip002 R Clavicle").FindChild("Bip002 R UpperArm").FindChild("Bip002 R Forearm").FindChild("Bip002 R Hand").FindChild("ballInGlove");
		
		ballInCGlvObj = GlobalVar.catcherObj.transform.FindChild("Bip002").FindChild("Bip002 Pelvis").FindChild("Bip002 Spine").FindChild("Bip002 Spine1").FindChild("Bip002 Spine2").FindChild("Bip002 Spine3").FindChild("Bip002 Neck").
						FindChild("Bip002 L Clavicle").FindChild("Bip002 L UpperArm").FindChild("Bip002 L Forearm").FindChild("Bip002 L Hand").FindChild("ballInCGlove");
		
		ballInCHandObj = GlobalVar.catcherObj.transform.FindChild("Bip002").FindChild("Bip002 Pelvis").FindChild("Bip002 Spine").FindChild("Bip002 Spine1").FindChild("Bip002 Spine2").FindChild("Bip002 Spine3").FindChild("Bip002 Neck").
						FindChild("Bip002 R Clavicle").FindChild("Bip002 R UpperArm").FindChild("Bip002 R Forearm").FindChild("Bip002 R Hand").FindChild("BallInCatHand");
	}
	
	
	public void AnimationTootaMgr_initAnimState()
	{
		if( GlobalVar.battingNum == GlobalVar.battingNum_Pre )
		{
			GlobalVar.statusAnim_PITCHER = Constants.ANISTATUS_PITCHER_BRETHING_AFTERPRE;
		}
		else
		{
			GlobalVar.statusAnim_PITCHER = Constants.ANISTATUS_PITCHER_PREPARE;
		}
		GlobalVar.statusAnim_CATCHER = Constants.ANISTATUS_CATCHER_PREPARE;
		ballInCHandObj.gameObject.SetActiveRecursively(false);
		GlobalVar.statusAnim_HITTER = Constants.ANISTATUS_HITTER_STANDING;
		AnimationTootaMgr_setPitcherAnimByStatus();
		AnimationTootaMgr_setHitterAnimByStatus();
		AnimationTootaMgr_setCatcherAnimByStatus();
	}
	
	public void AnimationTootaMgr_setPitcherAnimByStatus()
	{
		switch( GlobalVar.statusAnim_PITCHER )
		{
		case Constants.ANISTATUS_PITCHER_PREPARE:
			GlobalVar.pitcherObj.animation.Play(namePHd+"_Prepare_4");
			break;
		case Constants.ANISTATUS_PITCHER_BRETHING_AFTERPRE:
			GlobalVar.pitcherObj.animation.Play(namePHd+"_Breath_0");
			break;
		case Constants.ANISTATUS_PITCHER_SIGN:
			GlobalVar.pitcherObj.animation.Play(namePHd+"_Sign_1");
			break;
		case Constants.ANISTATUS_PITCHER_BRETHING_AFTERSIGN:
			GlobalVar.pitcherObj.animation.Play(namePHd+"_Breath_0");
			break;
		case Constants.ANISTATUS_PITCHER_WINDUP:
			//Debug.Log(namePHd_windUp+"_Pitching_windup_0");
			GlobalVar.pitcherObj.animation.Play(namePHd_windUp+"_Pitching_windup_0");
			break;
		case Constants.ANISTATUS_PITCHER_RETURN:
			GlobalVar.pitcherObj.animation.Play(namePHd+"_Return_0");
			break;
		}
	}
	
	public void AnimationTootaMgr_setHitterAnimByStatus()
	{
		//Debug.Log("AnimationTootaMgr_setHitterAnimByStatus().state="+GlobalVar.statusAnim_HITTER);
		switch( GlobalVar.statusAnim_HITTER )
		{
		case Constants.ANISTATUS_HITTER_STANDING:
			GlobalVar.hitterObj.animation.Play(nameHHd+"_Standing_1");
			break;
		case Constants.ANISTATUS_HITTER_TAKEBACK:
			GlobalVar.hitterObj.animation.Play(nameHHd+"_TakeBack_0");
			break;
		case Constants.ANISTATUS_HITTER_SWING:
			GlobalVar.hitterObj.animation.Play(nameHHd+"_Swing_4");//_4 means midle of Strike Zone
			break;
		}
	}
	
	public void AnimationTootaMgr_setCatcherAnimByStatus()
	{
		switch( GlobalVar.statusAnim_CATCHER )
		{
		case Constants.ANISTATUS_CATCHER_PREPARE:
			GlobalVar.catcherObj.animation.Play(nameCHd+"_Prepare_0");
			break;
		case Constants.ANISTATUS_CATCHER_SIGN:
			GlobalVar.catcherObj.animation.Play(nameCHd+"_Sign_0");
			break;
		case Constants.ANISTATUS_CATCHER_BREATH:
			GlobalVar.catcherObj.animation.Play(nameCHd+"_Breath");
			break;
		case Constants.ANISTATUS_CATCHER_MOVE_LEFT:
			GlobalVar.catcherObj.animation.Play(nameCHd+"_Move_Left");
			break;
		case Constants.ANISTATUS_CATCHER_MOVE_RIGHT:
			GlobalVar.catcherObj.animation.Play(nameCHd+"_Move_Right");
			break;
		case Constants.ANISTATUS_CATCHER_CATCHREADY_0:
			GlobalVar.catcherObj.animation.Play(nameCHd+"_CatchReadyP1_0");
			break;
		case Constants.ANISTATUS_CATCHER_CATCHRWAIT:
			GlobalVar.catcherObj.animation.Play(nameCHd+"_CatchReadyWait_0");
			break;
		case Constants.ANISTATUS_CATCHER_CATCHREADY_1:
			GlobalVar.catcherObj.animation.Play(nameCHd+"_CatchReadyP2_0");
			break;
		case Constants.ANISTATUS_CATCHER_CATCH_0:
			GlobalVar.catcherObj.animation.Play(nameCHd+"_CatchP1_4");
			break;
		case Constants.ANISTATUS_CATCHER_CATCH_1:
			GlobalVar.catcherObj.animation.Play(nameCHd+"_CatchP2_4");
			break;
		case Constants.ANISTATUS_CATCHER_RETURN:
			GlobalVar.catcherObj.animation.Play(nameCHd+"_Return_0");
			break;
		}
	}
	
	public void AnimationTootaMgr_process()
	{
	//Pitcher Animation Process
		switch( GlobalVar.statusAnim_PITCHER )
		{
		case Constants.ANISTATUS_PITCHER_PREPARE:
			if( !GlobalVar.pitcherObj.animation.IsPlaying(namePHd+"_Prepare_4") )
			{
				if( GlobalVar.GAMEPROC_STATE == Constants.STATE_GAMEPROC_PITCHER_SELDIR )
				{
					GlobalVar.statusAnim_PITCHER = Constants.ANISTATUS_PITCHER_SIGN;
					GlobalVar.statusAnim_CATCHER = Constants.ANISTATUS_CATCHER_SIGN;
					AnimationTootaMgr_setCatcherAnimByStatus();
				}
				else
				{
					GlobalVar.statusAnim_PITCHER = Constants.ANISTATUS_PITCHER_BRETHING_AFTERPRE;
				}
				AnimationTootaMgr_setPitcherAnimByStatus();
				//Debug.Log("End of Prepare status:"+GlobalVar.statusAnim_PITCHER);
			}
			break;
		case Constants.ANISTATUS_PITCHER_BRETHING_AFTERPRE:
			if( GlobalVar.GAMEPROC_STATE == Constants.STATE_GAMEPROC_PITCHER_SELDIR )
			{
				GlobalVar.statusAnim_PITCHER = Constants.ANISTATUS_PITCHER_SIGN;
				GlobalVar.statusAnim_CATCHER = Constants.ANISTATUS_CATCHER_SIGN;
				AnimationTootaMgr_setCatcherAnimByStatus();
				AnimationTootaMgr_setPitcherAnimByStatus();
			}
			else if( GlobalVar.GAMEPROC_STATE == Constants.STATE_GAMEPROC_PITCHER_GESTURE )
			{
				GlobalVar.statusAnim_PITCHER = Constants.ANISTATUS_PITCHER_BRETHING_AFTERSIGN;
			}
			//Debug.Log("after prepare : "+GlobalVar.GAMEPROC_STATE);
			break;
		case Constants.ANISTATUS_PITCHER_SIGN:
			if( !GlobalVar.pitcherObj.animation.IsPlaying(namePHd+"_Sign_1") )
			{
				GlobalVar.statusAnim_PITCHER = Constants.ANISTATUS_PITCHER_BRETHING_AFTERSIGN;
				AnimationTootaMgr_setPitcherAnimByStatus();
				//Debug.Log("EndOfSign "+GlobalVar.GAMEPROC_STATE);
			}
			//GlobalVar.pitcherObj.animation.Play(namePHd+"_Sign_1");
			break;
		case Constants.ANISTATUS_PITCHER_BRETHING_AFTERSIGN:
			if( GlobalVar.GAMEPROC_STATE == Constants.STATE_GAMEPROC_PITCHER_WINDUP )
			{
				GlobalVar.statusAnim_PITCHER = Constants.ANISTATUS_PITCHER_WINDUP;
				AnimationTootaMgr_setPitcherAnimByStatus();
			}
			break;
		}
		
	//Catcher Animation Process
		switch( GlobalVar.statusAnim_CATCHER )
		{
		case Constants.ANISTATUS_CATCHER_PREPARE:
			if( !GlobalVar.catcherObj.animation.IsPlaying(nameCHd+"_Prepare_0") )
			{
				GlobalVar.statusAnim_CATCHER = Constants.ANISTATUS_CATCHER_BREATH;
				AnimationTootaMgr_setCatcherAnimByStatus();
			}
			break;
		case Constants.ANISTATUS_CATCHER_SIGN:
			if( !GlobalVar.catcherObj.animation.IsPlaying(nameCHd+"_Sign_0") )
			{
				GlobalVar.statusAnim_CATCHER = Constants.ANISTATUS_CATCHER_BREATH;
				AnimationTootaMgr_setCatcherAnimByStatus();
			}
			//GlobalVar.catcherObj.animation.Play(nameCHd+"_Sign_0");
			break;
		case Constants.ANISTATUS_CATCHER_BREATH:
			if( GlobalVar.GAMEPROC_STATE == Constants.STATE_GAMEPROC_PITCHER_GESTURE )
			{
				//GlobalVar.statusAnim_CATCHER = Constants.ANISTATUS_CATCHER_MOVE_LEFT;
				GlobalVar.statusAnim_CATCHER = Constants.ANISTATUS_CATCHER_CATCHREADY_0;
				AnimationTootaMgr_setCatcherAnimByStatus();
			}
			//GlobalVar.catcherObj.animation.Play(nameCHd+"_Breath_1");
			break;
		case Constants.ANISTATUS_CATCHER_MOVE_LEFT:
			if( !GlobalVar.catcherObj.animation.IsPlaying(nameCHd+"_Move_Left") )
			{
				//if( GlobalVar.GAMEPROC_STATE == Constants.STATE_GAMEPROC_PITCHER_WINDUP )
				{
					GlobalVar.statusAnim_CATCHER = Constants.ANISTATUS_CATCHER_CATCHREADY_0;
					AnimationTootaMgr_setCatcherAnimByStatus();
				}
			}
			break;
		case Constants.ANISTATUS_CATCHER_MOVE_RIGHT:
			if( !GlobalVar.catcherObj.animation.IsPlaying(nameCHd+"_Move_Right") )
			{
				//if( GlobalVar.GAMEPROC_STATE == Constants.STATE_GAMEPROC_PITCHER_WINDUP )
				{
					GlobalVar.statusAnim_CATCHER = Constants.ANISTATUS_CATCHER_CATCHREADY_0;
					AnimationTootaMgr_setCatcherAnimByStatus();
				}
			}
			break;
		case Constants.ANISTATUS_CATCHER_CATCHREADY_0:
			if( !GlobalVar.catcherObj.animation.IsPlaying(nameCHd+"_CatchReadyP1_0") )
			{
				if( GlobalVar.GAMEPROC_STATE == Constants.STATE_GAMEPROC_PITCHER_WINDUP )
				{
					GlobalVar.statusAnim_CATCHER = Constants.ANISTATUS_CATCHER_CATCHREADY_1;
					AnimationTootaMgr_setCatcherAnimByStatus();
				}
			}
			break;
		case Constants.ANISTATUS_CATCHER_CATCHREADY_1:
			if( GlobalVar.bStartCatAnim )
			{
				GlobalVar.statusAnim_CATCHER = Constants.ANISTATUS_CATCHER_CATCH_0;
				AnimationTootaMgr_setCatcherAnimByStatus();
			}
			break;
		case Constants.ANISTATUS_CATCHER_CATCH_0:
			if( GlobalVar.bBallInMitt )
			{
				GlobalVar.statusAnim_CATCHER = Constants.ANISTATUS_CATCHER_CATCH_1;
				AnimationTootaMgr_setCatcherAnimByStatus();
				ballInCGlvObj.gameObject.SetActiveRecursively(true);
			}
			break;
		case Constants.ANISTATUS_CATCHER_CATCH_1:
			if( GlobalVar.bStartBackToPit )
			{
				GlobalVar.statusAnim_CATCHER = Constants.ANISTATUS_CATCHER_RETURN;
				AnimationTootaMgr_setCatcherAnimByStatus();
			}
			break;
		case Constants.ANISTATUS_CATCHER_RETURN:
			if( GlobalVar.bGripBall )
			{
				ballInCGlvObj.gameObject.SetActiveRecursively(false);
				ballInCHandObj.gameObject.SetActiveRecursively(true);
			}
			if( GlobalVar.bThrowToPit )
			{
				ballInCHandObj.gameObject.SetActiveRecursively(false);
			}
			break;
		}
	}
	
	public void AnimationTootaMgr_setReleasePointOfPitcher()
	{
		ballInGlvObj.gameObject.SetActiveRecursively(false);
		GlobalVar.pBallObj.gameObject.SetActiveRecursively(true);
		GlobalVar.pBallShadowObj.SetActiveRecursively(true);
		GlobalVar.time_RlsPnt = Time.time;
		Debug.Log("Time at Release Point = "+GlobalVar.time_RlsPnt);
		GlobalVar.GAMEPROC_STATE = Constants.STATE_GAMEPROC_PITCHER_THROW;
	}
}
