using UnityEngine;
using System.Collections;

public class AnimationTootaEvt : MonoBehaviour 
{	
	void Start(){}
	void Update(){}
		
	public void AnimationTootaEvt_getPitchingAniEvent( int evtIdx )
	{
		//Debug.Log("AnimationEvt_Toota_getAnimationEvent() evtName = "+evtName);
		switch ( evtIdx )
		{
		case Constants.CH01_PITCHER_RELEASEPOINT:
			//throw new System.ArgumentException();
			GlobalVar.aniTMgr.AnimationTootaMgr_setReleasePointOfPitcher();
			break;
		case Constants.CH01_PITCHER_WINDUPPOINT:
			//Debug.Log("@@@ CH01_PITCHER_WINDUPPOINT");
			GlobalVar.bStartPitWindup = true;
			break;
		}
	}
	
	public void AnimationTootaEvt_getCatcherAniEvent( int evtIdx )
	{
		switch ( evtIdx )
		{
		case Constants.CH01_CATCHER_RELEASEPOINT:
			GlobalVar.bThrowToPit = true;
			break;
		case Constants.CH01_CATCHER_GRIPBALL:
			GlobalVar.bGripBall = true;
			break;
		case Constants.CH01_CATCHER_PITPLAYRETURN:
			GlobalVar.statusAnim_PITCHER = Constants.ANISTATUS_PITCHER_RETURN;
			GlobalVar.aniTMgr.AnimationTootaMgr_setPitcherAnimByStatus();
			break;
		}
	}
}
