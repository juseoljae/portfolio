using UnityEngine;
using System.Collections;

public class MyTeamMgr
{


	public byte bBottom;//0:Away first attack / 1:Home first Defence

	public struct PITCHER
	{
		public int uIdx;
		public string name;
		public int uNameIdx;
		public int photoIdx;
		public int teamIdx;
		public int bestYear;
		public int grade;
		//0:4Seam, 1:2Seam, 2:Sinker, 3:ChangeUp, 4:Spliter, 5:Fork, 6:Curve, 7:Slider, 8:Cutter, 9:RisingFB, 
		//10:SinkingFB, 11:CircleCH, 12:Palm, 13:Knuclke, 14:SlowCV, 15:HSlider, 16:Frisbee, 17:UpShoot
		public int[] ballType;// = new int[5];
		public int ability_Control;
		public int ability_Max;
		public byte posIdx;
		public byte bodyIdx;
		public int FORM;
		public int handDir;
		public int formIdx;
		public int health;
		public int contract;
		public int career;
		public int scout;
	};
	//Myteam
	public PITCHER[] pitchers = new PITCHER[11];//Line up
	public PITCHER P_starting = new PITCHER();//Starting 
	public int idx_catcher;
	//Against team
	public PITCHER[] pitchers_agnst = new PITCHER[11];//Line up
	public PITCHER P_starting_agnst = new PITCHER();//Starting 
	public int idx_catcher_agnst;


	public struct HITTER
	{
		public int uIdx;
		public string name;
		public int uNameIdx;
		public int photoIdx;
		public int teamIdx;
		public int bestYear;
		public int grade;
		public int ability_Power;
		public int ability_Accuracy;
		public int ability_Speed;
		public int ability_Defence;
		public int posIdx;
		public byte bodyIdx;
		public int tootaIdx;
		public int handDir;
		public int health;
		public int contract;
		public int career;
		public int scout;
		public float rangeBox;
		public float[] rangeArea;
	};
	//Myteam
	public HITTER[] hitters = new HITTER[15];//Line up(0~8 Starting, else bench)
	public HITTER[] H_starting = new HITTER[9];//Starting
											   //Against team
	public HITTER[] hitters_agnst = new HITTER[15];//Line up(0~8 Starting, else bench)
	public HITTER[] H_starting_agnst = new HITTER[9];//Starting



	public bool MyTeamMgr_LoadPlayers()
	{
		MyTeamMgr_LoadPitchers();
		MyTeamMgr_LoadHitters();

		MyTeamMgr_LoadPitchers_against();
		MyTeamMgr_LoadHitters_against();

		return true;
	}

	//LOAD
	//Myteam players load
	void MyTeamMgr_LoadPitchers()
	{
		//Temporary
		pitchers[0].uIdx = 6000;
		for (int i = 0; i < pitchers.Length; i++)
		{
			pitchers[i].uIdx++;
			pitchers[i].name = "C.H.Park";
			pitchers[i].uNameIdx = 0;
			pitchers[i].photoIdx = 0;
			pitchers[i].teamIdx = 0;
			pitchers[i].bestYear = 1999;
			pitchers[i].grade = 0;
			pitchers[i].ballType = new int[5];//[Constants.BALLTYPE_MAX_PITCHER]
											  //temp
			pitchers[i].ballType[0] = 0;
			pitchers[i].ballType[1] = 1;
			pitchers[i].ballType[2] = 5;
			pitchers[i].ballType[3] = 6;
			pitchers[i].ballType[4] = 17;

			pitchers[i].ability_Control = 70;
			pitchers[i].ability_Max = 99;
			pitchers[i].posIdx = 0;
			pitchers[i].bodyIdx = 1;
			pitchers[i].FORM = 2;//2:UNDER 0: over
			if (pitchers[i].FORM < 3)
			{
				pitchers[i].handDir = (int)GlobalVar.HANDDIR.RIGHT;
				pitchers[i].formIdx = pitchers[i].FORM;
			}
			else
			{
				pitchers[i].handDir = (int)GlobalVar.HANDDIR.LEFT;
				pitchers[i].formIdx = pitchers[i].FORM - 3;
			}


			pitchers[i].health = 99;
			pitchers[i].contract = 1;
			pitchers[i].career = 1;
			pitchers[i].scout = 0;

			//Debug.Log(i+" / "+pitchers[i].uIdx);
		}
		Debug.Log("MyTeamMgr.LoadPitchers() Load Complete !!");
	}

	void MyTeamMgr_LoadHitters()
	{
		hitters[0].uIdx = 0;
		for (int i = 0; i < hitters.Length; i++)
		{
			hitters[i].rangeArea = new float[6];
			hitters[i].uIdx++;
			hitters[i].name = "T.K.Kim";
			hitters[i].uNameIdx = 0;
			hitters[i].photoIdx = 0;
			hitters[i].teamIdx = 0;
			hitters[i].bestYear = 1999;
			hitters[i].grade = 0;
			hitters[i].ability_Power = 80;
			hitters[i].ability_Accuracy = 66;
			hitters[i].ability_Speed = 60;
			hitters[i].ability_Defence = 70;
			hitters[i].rangeBox = MyTeamMgr_getHitterRangeBox(hitters[i].ability_Accuracy);
			hitters[i].rangeArea = MyTeamMgr_getHitterRangeArea(hitters[i].rangeBox);
			hitters[i].posIdx = 1;
			hitters[i].bodyIdx = 1;
			hitters[i].tootaIdx = 0;
			hitters[i].handDir = 1;
			hitters[i].health = 99;
			hitters[i].contract = 1;
			hitters[i].career = 1;
			hitters[i].scout = 0;
		}
		idx_catcher = 4;
	}

	//Against Team Players Load
	void MyTeamMgr_LoadPitchers_against()
	{
		pitchers[0].uIdx = 6000;
		for (int i = 0; i < pitchers.Length; i++)
		{
			pitchers_agnst[i].uIdx++;
			pitchers_agnst[i].name = "C.H.Park";
			pitchers_agnst[i].uNameIdx = 0;
			pitchers_agnst[i].photoIdx = 0;
			pitchers_agnst[i].teamIdx = 0;
			pitchers_agnst[i].bestYear = 1999;
			pitchers_agnst[i].grade = 0;
			pitchers_agnst[i].ballType = new int[5];//[Constants.BALLTYPE_MAX_PITCHER]
													//temp
			pitchers_agnst[i].ballType[0] = 0;
			pitchers_agnst[i].ballType[1] = 1;
			pitchers_agnst[i].ballType[2] = 5;
			pitchers_agnst[i].ballType[3] = 6;
			pitchers_agnst[i].ballType[4] = 17;

			pitchers_agnst[i].ability_Control = 70;
			pitchers_agnst[i].ability_Max = 99;
			pitchers_agnst[i].posIdx = 0;
			pitchers_agnst[i].bodyIdx = 1;
			pitchers_agnst[i].FORM = 2;//2:UNDER 0: over
			if (pitchers_agnst[i].FORM < 3)
			{
				pitchers_agnst[i].handDir = (int)GlobalVar.HANDDIR.RIGHT;
				pitchers_agnst[i].formIdx = pitchers_agnst[i].FORM;
			}
			else
			{
				pitchers_agnst[i].handDir = (int)GlobalVar.HANDDIR.LEFT;
				pitchers_agnst[i].formIdx = pitchers_agnst[i].FORM - 3;
			}


			pitchers_agnst[i].health = 99;
			pitchers_agnst[i].contract = 1;
			pitchers_agnst[i].career = 1;
			pitchers_agnst[i].scout = 0;

			//Debug.Log(i+" / "+pitchers[i].uIdx);
		}
		Debug.Log("MyTeamMgr.MyTeamMgr_LoadPitchers_against() Load Complete !!");
	}

	void MyTeamMgr_LoadHitters_against()
	{
		hitters_agnst[0].uIdx = 0;
		for (int i = 0; i < hitters.Length; i++)
		{
			hitters_agnst[i].rangeArea = new float[6];
			hitters_agnst[i].uIdx++;
			hitters_agnst[i].name = "T.K.Kim";
			hitters_agnst[i].uNameIdx = 0;
			hitters_agnst[i].photoIdx = 0;
			hitters_agnst[i].teamIdx = 0;
			hitters_agnst[i].bestYear = 1999;
			hitters_agnst[i].grade = 0;
			hitters_agnst[i].ability_Power = 80;
			hitters_agnst[i].ability_Accuracy = 66;
			hitters_agnst[i].ability_Speed = 60;
			hitters_agnst[i].ability_Defence = 70;
			hitters_agnst[i].rangeBox = MyTeamMgr_getHitterRangeBox(hitters_agnst[i].ability_Accuracy);
			hitters_agnst[i].rangeArea = MyTeamMgr_getHitterRangeArea(hitters_agnst[i].rangeBox);
			hitters_agnst[i].posIdx = 1;
			hitters_agnst[i].bodyIdx = 1;
			hitters_agnst[i].tootaIdx = 0;
			hitters_agnst[i].handDir = 1;
			hitters_agnst[i].health = 99;
			hitters_agnst[i].contract = 1;
			hitters_agnst[i].career = 1;
			hitters_agnst[i].scout = 0;
		}
		idx_catcher_agnst = 7;
	}

	//SET
	public void MyTeamMgr_setStartingLineUp()
	{
		MyTeamMgr_setStartingLineUp_myTeam();
		MyTeamMgr_setStartingLineUp_against();
	}

	void MyTeamMgr_setStartingLineUp_myTeam()
	{
		//Pitcher
		P_starting = pitchers[0];

		//Hitter
		for (int i = 0; i < 9; i++)
		{
			H_starting[i] = hitters[i];
		}
	}

	void MyTeamMgr_setStartingLineUp_against()
	{
		//Pitcher
		P_starting_agnst = pitchers_agnst[0];

		//Hitter
		for (int i = 0; i < 9; i++)
		{
			H_starting_agnst[i] = hitters_agnst[i];
		}
	}

	//GET
	float MyTeamMgr_getHitterRangeBox(int contact)
	{
		//float range=0.0f;
		float rangeVar_0 = 1230.88f;
		float rangeVar_1 = 46;

		//Debug.Log("Range Box height = "+(rangeVar_0 / (contact + rangeVar_1)));
		return rangeVar_0 / (contact + rangeVar_1);		

	}

	float[] MyTeamMgr_getHitterRangeArea(float rBox)
	{
		float[] area = new float[6];

		area[0] = rBox / 2 * (-1);      //early
		area[1] = (rBox / 2 * 0.4f) * (-1);//good early
		area[2] = (rBox / 2 * 0.1f);//good early
		area[3] = (rBox / 2 * 0.2f) + area[2];//perfect
		area[4] = (rBox / 2 * 0.4f) + area[2];//good late
		area[5] = (rBox / 2);       //late

		return area;
	}
}
