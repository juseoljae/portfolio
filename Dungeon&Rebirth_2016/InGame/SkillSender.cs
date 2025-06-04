#define TESTTEST

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SkillSender : MonoBehaviour
{
    private SkillDataInfo.SkillInfo m_skillInfo         = null;
    private CharacterBase m_target = null;
    private Vector3 m_destination                       = Vector3.zero;
    private List<CharacterBase> m_targetsInRange = null;
    private List<CharacterBase> m_Exceptiontargets = null;
    private Dictionary<CharacterBase, int> m_hitTargetInfo = null;
    private CharacterBase m_executioner = null;
    private int m_iHitIndex = 0;
    private Vector3 m_Line;
    public bool m_bCheckOnly = false;
    public void DestroySkillSender()
    {
        if (m_skillInfo != null) m_skillInfo = null;
        if (m_target != null) m_target = null;
        if (m_targetsInRange != null) m_targetsInRange = null;
        if (m_executioner != null) m_executioner = null;
    }


    static public bool IntersectCircle( Vector2 p_RayOrign , Vector2 p_RayDir ,  Vector2 p_Circle , float p_fRadius )
    {
        Vector2 m = p_RayOrign - p_Circle;

        float c = Vector2.Dot(m, m) - (p_fRadius * p_fRadius);
 
        if( c <= 0 )
            return true;

        float b = Vector2.Dot(m, p_RayDir);
 
        if( b> 0)     
            return false;
 
        float d = ( b * b ) - c;
 
        if( d < 0 )
            return false; 
 
        return true;  
    }

    #region PUBLIC FUNC

    public List<CharacterBase> GetTargetsInRange()
    {
        return m_targetsInRange;
    }
    public void DoSkill(CharacterBase executioner, GameObject goTarget, SkillDataInfo.SkillInfo skillInfo , int p_iHitIndex , bool p_bCheck = false)
    {
        DoSkill(executioner, (goTarget!=null)?goTarget.GetComponent<CharacterBase>():null, skillInfo, p_iHitIndex, p_bCheck);
    }
    public void DoSkill(CharacterBase executioner, CharacterBase goTarget, SkillDataInfo.SkillInfo skillInfo, int p_iHitIndex, bool p_bCheck = false)
    {
        m_target = goTarget;
        m_skillInfo = skillInfo;
        m_destination = Vector3.zero;
        m_targetsInRange = null;
        m_executioner = executioner;
        m_iHitIndex = p_iHitIndex;
        m_bCheckOnly = p_bCheck;

        Process();
    }
    public void DoSkill(CharacterBase executioner, List<CharacterBase> targetsInRange, SkillDataInfo.SkillInfo skillInfo, bool p_bCheck = false)
    {
        m_skillInfo = skillInfo;
        m_destination = Vector3.zero;
        m_targetsInRange = new List<CharacterBase>(targetsInRange);
        m_executioner = executioner;
        m_iHitIndex = 0;
        m_target = null;
        m_bCheckOnly = p_bCheck;

        for (int i = m_targetsInRange.Count - 1; i >= 0; --i)
        {
            if (m_Exceptiontargets != null)
            {
                for (int j = 0; j < m_Exceptiontargets.Count; ++j)
                {
                    if (m_Exceptiontargets[j] == m_targetsInRange[i])
                    {
                        m_targetsInRange.RemoveAt(i);
                        break;
                    }
                }
            }
        }
        if (m_targetsInRange.Count > 0)
            SendSkill(m_targetsInRange);

        EndSkill();
    }


    #endregion PUBLIC FUNC




    #region PRIVATE FUNC

   
    private void Process()
    {
		if(InGameManager.instance.m_bPauseAll == true)
		{
			return;
		}

		if (m_skillInfo != null)
        {
            if (m_target != null)
            {
 
                List<CharacterBase> allTargets      = GetAllTargets(m_executioner,m_skillInfo.target_State);
                List<CharacterBase> targetsInRange  = GetTargetsInRange(m_skillInfo, allTargets, this.transform.position);

                if( m_skillInfo.action_Type == eSkillActionType.eTYPE_PENETRATION )
                {
                    // 관통 라인 검사
                    List<CharacterBase> targetsInLine = GetTargetsInLine(allTargets, m_skillInfo.area_Size, m_executioner.m_MyObj.position, m_executioner.m_pushInfo.m_PrePos);
                    if( targetsInLine != null && targetsInLine.Count > 0 )
                    {
                        for (int m = 0; m < targetsInLine.Count; ++m )
                        {
                            bool bExist = false;
                            for (int n = 0; n < targetsInRange.Count; ++n)
                            {
                                if( targetsInRange[n] == targetsInLine[m] )
                                {
                                    bExist = true;
                                    break;
                                }
                            }
                            if( bExist == false )
                                targetsInRange.Add(targetsInLine[m]);
                        }
                    }
                    m_executioner.m_pushInfo.m_PrePos = m_executioner.m_MyObj.position;
                }

                m_targetsInRange                    = targetsInRange;
                SendSkill(targetsInRange);
                EndSkill();
            }
        }
    }
    
    public List<CharacterBase> GetTargetsInRange(SkillDataInfo.SkillInfo skillInfo, List<CharacterBase> targets, Vector3 skillCenterPosition)
    {
        if (skillInfo.target_Type == eTargetType.eSELF)
        {
            List<CharacterBase> SelfTarget = new List<CharacterBase>();
            SelfTarget.Add(m_executioner);
            return SelfTarget;
        }

        /// 원 범위 안의 적을 탐색
        List<CharacterBase> allTargets = targets;
        List<CharacterBase> targetsInCircle = new List<CharacterBase>();

        Vector2 centerPosition = new Vector2(skillCenterPosition.x, skillCenterPosition.z);

        // 타겟 타입은 타겟만 검사 해보면 됩니다.
        if (skillInfo.target_Type == eTargetType.eTARGET && m_target != null)
        {
            allTargets.Clear();
            allTargets.Add(m_target);
        }

        for (int i = 0; i < allTargets.Count; i++)
        {
            bool bContinue = false;

            // 데미지 계산에서 제외 될 사람
            if (m_Exceptiontargets != null)
            {
                for (int j = 0; j < m_Exceptiontargets.Count; ++j)
                {
                    if (m_Exceptiontargets[j] == allTargets[i])
                    {
                        bContinue = true;
                        break;
                    }
                }
            }
            if (bContinue)
                continue;

            Transform target            = allTargets[i].transform;
            Vector2 targetPosition      = new Vector2(target.position.x, target.position.z);

            //Transform thisTransform     = this.transform;
            float radius = 0.0f;
            float fDistance             = Vector2.Distance(centerPosition, targetPosition);
            if ( skillInfo.target_Type != eTargetType.eAREA )
            {
                radius = m_executioner.GetRadius();
            }

            //if ( fDistance < skillInfo.apply_area_size_circle )
            float fTargetRadius = (allTargets[i].m_Collider != null) ? allTargets[i].GetRadius() : 1.0f;
            if (fDistance - (radius + fTargetRadius) <= skillInfo.area_Size)
            {
                targetsInCircle.Add(allTargets[i]);
            }
        }

        // 타입별 
        switch (skillInfo.target_Type)
        {
            case eTargetType.eNONE:
            case eTargetType.eTARGET: // 싱글 타입이면 가장 가까운 타겟만 남기고 제거
                if (targetsInCircle.Count > 1)
                {
                    float fMinDist = float.MaxValue;
                    int iShortestIndex = 0;
                    for (int m = 0; m < targetsInCircle.Count; ++m)
                    {
                        float kDist = Vector2.Distance(centerPosition, new Vector2(targetsInCircle[m].transform.position.x, targetsInCircle[m].transform.position.z));

                        if (kDist < fMinDist)
                        {
                            fMinDist = kDist;
                            iShortestIndex = m;
                        }
                    }
                    CharacterBase kShortestChar = targetsInCircle[iShortestIndex];
                    targetsInCircle.Clear();
                    targetsInCircle.Add(kShortestChar);

                }
                break;
        }

        /// 스킬 방향이 모든 방향이면 그대로 리턴
        if (skillInfo.direction == eDirTypeSelect.eEVERYWHERE)
        {
            return targetsInCircle;
        }
        /// 스킬 타입이 각도가 있으면 원범위안의 적에서 각안의 적을 탐색
        else
        {

            List<CharacterBase> targetInAngle = new List<CharacterBase>();

            Vector2 direction = Vector2.zero;

            switch (skillInfo.direction)
            {
                case eDirTypeSelect.eFOWARD:
                        direction = new Vector2( this.transform.forward.x, this.transform.forward.z);
                    break;
                case eDirTypeSelect.eBACKWARD:
                        direction = new Vector2( this.transform.forward.x, this.transform.forward.z) * -1;
                    break;
            }

            float fAngle = skillInfo.area_Angle * 0.5f;

            for (int i = 0; i < targetsInCircle.Count; i++)
            {
                Vector2 targetVector = new Vector2(targetsInCircle[i].transform.position.x , targetsInCircle[i].transform.position.z);
                float fValue = Vector2.Angle(direction, targetVector - centerPosition);
                if (fValue <= fAngle)
                    targetInAngle.Add(targetsInCircle[i]);
                else
                {
                    // 좌측 레이
                    Vector3 vRayDir = Quaternion.AngleAxis(-fAngle, new Vector3(0, 1, 0)) * new Vector3(direction.x, 0, direction.y);
                    if (IntersectCircle(centerPosition, new Vector2(vRayDir.x, vRayDir.z), targetVector, targetsInCircle[i].GetRadius()))
                    {
                        targetInAngle.Add(targetsInCircle[i]);
                    }
                    else
                    {
                        // 우측 레이
                        vRayDir = Quaternion.AngleAxis(fAngle, new Vector3(0, 1, 0)) * new Vector3(direction.x, 0, direction.y);
                        if (IntersectCircle(centerPosition, new Vector2(vRayDir.x, vRayDir.z), targetVector, targetsInCircle[i].GetRadius()))
                        {
                            targetInAngle.Add(targetsInCircle[i]);
                        }
                    }
                }
            }
            return targetInAngle;
        }
    }

    public List<CharacterBase> GetTargetsInLine(SkillDataInfo.SkillInfo skillInfo, List<CharacterBase> targets, Vector3 vecCenter ,  Vector3 vecLine)
    {
        /// 원 범위 안의 적을 탐색
        List<CharacterBase> allTargets = targets;
        List<CharacterBase> targetsInCircle = new List<CharacterBase>();

        Vector2 centerPosition = new Vector2(vecCenter.x, vecCenter.z);

        for (int i = 0; i < allTargets.Count; i++)
        {
            if (allTargets[i].m_CharState == CHAR_STATE.DEATH)
                continue;

            bool bContinue = false;

            // 데미지 계산에서 제외 될 사람
            if (m_Exceptiontargets != null)
            {
                for (int j = 0; j < m_Exceptiontargets.Count; ++j)
                {
                    if (m_Exceptiontargets[j] == allTargets[i])
                    {
                        bContinue = true;
                        break;
                    }
                }
            }
            if (bContinue)
                continue;

            Transform target = allTargets[i].transform;
            Vector2 targetPosition = new Vector2(target.position.x, target.position.z);

            //Transform thisTransform     = this.transform;
            Vector2 LineDir = new Vector2(vecLine.x, vecLine.z);
            float fDistance = Vector2.Distance(centerPosition, targetPosition);
            Vector2 vecPlog = targetPosition - centerPosition;
            LineDir = LineDir - centerPosition;
            float fDotP = Vector2.Dot(vecPlog, LineDir);

            float fRadius = m_executioner.GetRadius() + allTargets[i].GetRadius();

            if (fDotP > 0.0f && fDistance - fRadius < skillInfo.skill_Dist)
            {
                Vector2 pProj = (fDotP / Vector3.Dot(LineDir, LineDir)) * LineDir;
                Vector2 H = (vecPlog - pProj);
                float fSkillArea = allTargets[i].GetRadius() + skillInfo.area_Size;
                if (H.sqrMagnitude < fSkillArea * fSkillArea)
                {
                    float fAngle = skillInfo.area_Angle * 0.5f;

                    Vector2 direction = Vector2.zero;

                    switch (skillInfo.direction)
                    {
                        case eDirTypeSelect.eFOWARD:
                            direction = new Vector2(this.transform.forward.x, this.transform.forward.z);
                            break;
                        case eDirTypeSelect.eBACKWARD:
                            direction = new Vector2(this.transform.forward.x, this.transform.forward.z) * -1;
                            break;
                    }

                    float fValue = Vector2.Angle(direction, targetPosition - centerPosition);
                    if (fValue <= fAngle)
                        targetsInCircle.Add(allTargets[i]);
                    else
                    {
                        // 좌측 레이
                        Vector3 vRayDir = Quaternion.AngleAxis(-fAngle, new Vector3(0, 1, 0)) * new Vector3(direction.x, 0, direction.y);
                        if (IntersectCircle(centerPosition, new Vector2(vRayDir.x, vRayDir.z), targetPosition, allTargets[i].GetRadius()))
                        {
                            targetsInCircle.Add(allTargets[i]);
                        }
                        else
                        {
                            // 우측 레이
                            vRayDir = Quaternion.AngleAxis(fAngle, new Vector3(0, 1, 0)) * new Vector3(direction.x, 0, direction.y);
                            if (IntersectCircle(centerPosition, new Vector2(vRayDir.x, vRayDir.z), targetPosition, allTargets[i].GetRadius()))
                            {
                                targetsInCircle.Add(allTargets[i]);
                            }
                        }
                    }
                }
            }
        }

        switch (skillInfo.target_Type)
        {
            case eTargetType.eNONE:
            case eTargetType.eTARGET: // 싱글 타입이면 가장 가까운 타겟만 남기고 제거
                if (targetsInCircle.Count > 1)
                {
                    float fMinDist = float.MaxValue;
                    int iShortestIndex = 0;
                    for (int m = 0; m < targetsInCircle.Count; ++m)
                    {
                        float kDist = Vector2.Distance(centerPosition, new Vector2(targetsInCircle[m].transform.position.x, targetsInCircle[m].transform.position.z));

                        if (kDist < fMinDist)
                        {
                            fMinDist = kDist;
                            iShortestIndex = m;
                        }
                    }
                    CharacterBase kShortestChar = targetsInCircle[iShortestIndex];
                    targetsInCircle.Clear();
                    targetsInCircle.Add(kShortestChar);

                }
                break;
        }

        return targetsInCircle;
    }
	////////////////////////////////////////////////////////////////////////////////////////////////////
	//zunghoon 20171116 Add	
	// PlayerSummonGetTargets(ref List<CharacterBase> targets, CharacterBase p_executioner)
	//--------------------------------------------------------------------------------------------------
	//	Remarks.
	//	플레이어 캐릭터 소환수 를 타겟에 잡기위해서 추가 0이면 플레이어 캐릭터 팀 소환수 1이면 적군 팀 소환수
	////////////////////////////////////////////////////////////////////////////////////////////////////
	public void PlayerSummonGetTargets(ref List<CharacterBase> targets, CharacterBase p_executioner)
	{
		for (int i = 0; i < NpcManager.instance.m_PlayerSummonObjs.Count; i++)
		{
			if (NpcManager.instance.m_PlayerSummonObjs[i] != null)
			{
				if (NpcManager.instance.m_PlayerSummonObjs[i].gameObject != null && NpcManager.instance.m_PlayerSummonObjs[i].gameObject.activeSelf)
				{
					if(NpcManager.instance.m_PlayerSummonObjs[i].chrState == CHAR_STATE.ALIVE)
					{
						targets.Add(NpcManager.instance.m_PlayerSummonObjs[i]);
					}
				}
			}
		}
	}

	public List<CharacterBase> GetAllTargets( CharacterBase p_executioner, eTargetState eTargetState)
    {
        List<CharacterBase> targets = new List<CharacterBase>();

		switch (eTargetState)
        {
            case global::eTargetState.eNONE:    // 적, 아군 모두 포함
                {
                    /// 아군 포함하는게 들어가야 함
                    for (int i = 0; i < PlayerManager.instance.m_PlayerInfo.Count; i++)
                    {
                        if (PlayerManager.instance.m_PlayerInfo[i].playerObj != null && 
                            PlayerManager.instance.m_PlayerInfo[i].playerObj.activeSelf &&
                            PlayerManager.instance.m_PlayerInfo[i].playerCharBase.m_CharState == CHAR_STATE.ALIVE)
                        {
                            if (PlayerManager.instance.m_PlayerInfo[i].IsAvoidSkill(PlayerManager.instance.m_PlayerInfo[i].playerCharBase.m_ArollingSkill) == false ||
                                PlayerManager.instance.m_PlayerInfo[i].Team == PlayerManager.instance.m_PlayerInfo[p_executioner.charUniqID].Team )
                                targets.Add(PlayerManager.instance.m_PlayerInfo[i].playerCharBase);
                        }
                    }
                    /// 

                    /// 적
                    /// //#region 섹터관련 수정 작업
                    List<CharacterBase> npcs = NpcManager.instance.GetNpcTargetsBySpwaned();

                    for (int i = 0; i < npcs.Count; i++)
                    {
                        if(npcs[i].m_CharState == CHAR_STATE.ALIVE)
                        {
                            if (npcs[i].m_CharAi != null)
                            {
                                NpcInfo.NpcProp tmpNpcProp = npcs[i].m_CharAi.GetNpcProp();

                                if (tmpNpcProp != null)
                                {
                                    //if (tmpNpcProp.Battle_Type != 3)
                                    if (tmpNpcProp.Npc_Type != eNPC_TYPE.eZONEWALL_OBJ)
                                    {
                                        if (npcs[i].gameObject.activeSelf)
                                            targets.Add(npcs[i]);
                                    }
                                }
                            }
                        }
                    }
                }

				PlayerSummonGetTargets(ref targets, p_executioner);
				if (SceneManager.instance.GetCurGameMode() == SceneManager.eGAMEMODE.eNONSYNC_PVP)
				{
					EnemySummonGetTargets(ref targets, p_executioner);
				}

				break;
            case global::eTargetState.eFRIENDLY:    // 아군
                {
                    /// npc 캐릭터
                    if ( m_executioner.m_CharacterType == eCharacter.eNPC)
                    {
                        //#region 섹터관련 수정 작업
                        List<CharacterBase> npcs = NpcManager.instance.GetNpcTargetsBySpwaned();

                        for (int i = 0; i < npcs.Count; i++)
                        {
                            if (npcs[i].m_CharState == CHAR_STATE.ALIVE)
                            {
                                if (npcs[i].m_CharAi != null)
                                {
                                    NpcInfo.NpcProp tmpNpcProp = npcs[i].m_CharAi.GetNpcProp();

                                    if (tmpNpcProp != null)
                                    {
                                        //if (tmpNpcProp.Battle_Type != 3)
                                        if (tmpNpcProp.Npc_Type != eNPC_TYPE.eZONEWALL_OBJ)
                                        {
                                            if (tmpNpcProp.Identity_Fnd == global::eTargetState.eFRIENDLY)
                                            {
                                                if (npcs[i].gameObject.activeSelf)
                                                    targets.Add(npcs[i]);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    /// 플레이어 캐릭터
                    else
                    {
                        for (int i = 0; i < PlayerManager.instance.m_PlayerInfo.Count; i++ )
                        {
                            if (PlayerManager.instance.m_PlayerInfo[i].playerObj != null && PlayerManager.instance.m_PlayerInfo[i].playerObj.activeSelf)
                            {
                                if ( PlayerManager.instance.m_PlayerInfo[i].IsAvoidSkill( PlayerManager.instance.m_PlayerInfo[i].playerCharBase.m_ArollingSkill ) == false ||
                                    PlayerManager.instance.m_PlayerInfo[i].Team == PlayerManager.instance.m_PlayerInfo[p_executioner.charUniqID].Team)
                                    targets.Add(PlayerManager.instance.m_PlayerInfo[i].playerCharBase);
                            }
                        }
						/// 아군 포함하는게 들어가야 함
						//자신팀 에 있는 소환수를 타겟으로 잡기위해 추가
						if(p_executioner.Team == 0)
						{
							PlayerSummonGetTargets(ref targets, p_executioner);
							
						}
						else if(p_executioner.Team == 1)
						{
							EnemySummonGetTargets(ref targets, p_executioner);
						}
						/// 
					}

				}
                break;
            case global::eTargetState.eHOSTILE:     // 적
                {
                    /// 내가 npc 캐릭터
                    if (p_executioner.m_CharacterType == eCharacter.eNPC)
                    {
                        for (int i = 0; i < PlayerManager.instance.m_PlayerInfo.Count; i++)
                        {
                            if (PlayerManager.instance.m_PlayerInfo[i].playerObj != null && PlayerManager.instance.m_PlayerInfo[i].playerObj.activeSelf)
                            {
                                if (PlayerManager.instance.m_PlayerInfo[i].IsAvoidSkill(PlayerManager.instance.m_PlayerInfo[i].playerCharBase.m_ArollingSkill) == false)
                                    targets.Add(PlayerManager.instance.m_PlayerInfo[i].playerCharBase);
                            }
                        }
						List<CharacterBase> npcs = NpcManager.instance.GetNpcTargetsBySpwaned();

                        for (int i = 0; i < npcs.Count; i++)
                        {
                            if (npcs[i].m_CharState == CHAR_STATE.ALIVE)
                            {
                                if (npcs[i].m_CharAi != null)
                                {

                                    NpcInfo.NpcProp tmpNpcProp = npcs[i].m_CharAi.GetNpcProp();

                                    if (tmpNpcProp != null)
                                    {
                                        //if (tmpNpcProp.Battle_Type != 3)
                                        if (tmpNpcProp.Npc_Type != eNPC_TYPE.eZONEWALL_OBJ)
                                        {
                                            if (tmpNpcProp.Identity_Fnd == global::eTargetState.eFRIENDLY)
                                            {
                                                if (npcs[i].gameObject.activeSelf)
                                                    targets.Add(npcs[i]);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    /// 플레이어 캐릭터 또는 Pet
                    else
                    {
                        if(SceneManager.instance.GetCurGameMode() == SceneManager.eGAMEMODE.eNONSYNC_PVP)
                        {
							for (int i = 0; i < PlayerManager.instance.m_PlayerInfo.Count; i++)
							{
								try
								{
									if (PlayerManager.instance.m_PlayerInfo[i].playerObj != null && PlayerManager.instance.m_PlayerInfo[i].playerObj.activeSelf &&
										PlayerManager.instance.m_PlayerInfo[i].playerCharBase.Team != PlayerManager.instance.m_PlayerInfo[p_executioner.charUniqID].playerCharBase.Team &&
										PlayerManager.instance.m_PlayerInfo[i].playerCharBase.chrState == CHAR_STATE.ALIVE &&
										PlayerManager.instance.m_PlayerInfo[i].playerCharBase.m_IngameObjectID != p_executioner.m_IngameObjectID)
									{
										if (PlayerManager.instance.m_PlayerInfo[i].IsAvoidSkill(PlayerManager.instance.m_PlayerInfo[i].playerCharBase.m_ArollingSkill) == false)
											targets.Add(PlayerManager.instance.m_PlayerInfo[i].playerCharBase);
									}
								}
								catch (System.Exception e)
								{
									Debug.LogError("SkillSender.GetAllTargets() err = " + e.ToString());
								}
							}
							if (p_executioner.Team == 0)
							{
								EnemySummonGetTargets(ref targets, p_executioner);
							}
							else if (p_executioner.Team == 1)
							{
								PlayerSummonGetTargets(ref targets, p_executioner);
							}
                        }
                        else
                        {
                            //#region 섹터관련 수정 작업
                            List<CharacterBase> npcs = NpcManager.instance.GetNpcTargetsBySpwaned();

                            //Taylor
                            if (npcs != null)
                            {
                                for (int i = 0; i < npcs.Count; i++)
                                {
                                    if (npcs[i].m_CharState == CHAR_STATE.ALIVE)
                                    {
                                        if (npcs[i].m_CharAi != null)
                                        {
                                            NpcInfo.NpcProp tmpNpcProp = npcs[i].m_CharAi.GetNpcProp();

                                            if (tmpNpcProp != null)
                                            {
                                                //if (tmpNpcProp.Battle_Type != 3)
                                                if (tmpNpcProp.Npc_Type != eNPC_TYPE.eZONEWALL_OBJ)
                                                {
                                                    if (tmpNpcProp.Identity_Fnd == global::eTargetState.eHOSTILE)
                                                    {
                                                        if (npcs[i].gameObject.activeSelf)
                                                            targets.Add(npcs[i]);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                break;
        }

        return targets;
    }



    private void SendSkill(List<CharacterBase> targets )
    {
        bool bContinue = false;
        for (int i = targets.Count-1; i >=0 ; --i)
        {
            bContinue = false;

            if (targets[i].enabled == false)
            {
                bContinue = true;
            }

            if( bContinue )
                continue;
            bool bHit = targets[i].GetHitSkill(m_executioner, m_skillInfo, m_iHitIndex, m_bCheckOnly);
            if (bHit && m_bCheckOnly == false)
            {
                if (m_executioner.m_MyObj == PlayerManager.instance.playerInfoList[PlayerManager.MYPLAYER_INDEX].playerCharBase.m_MyObj && targets[i].m_CharacterType == eCharacter.eNPC)
                {
                    if (m_skillInfo.system_Affect_Code[m_iHitIndex] == eAffectCode.ePHYSICAL_DAMAGE_RATIO)
                    {
                       int iStance = (int)UserManager.instance.m_CurPlayingCharInfo.playerInfo.m_eStanceType - 1;
                       PlayerStanceInfo StanceInfo = PlayerManager.instance.GetPlayerStanceInfo(PlayerManager.instance.playerInfoList[PlayerManager.MYPLAYER_INDEX].playerCharBase, iStance);

                       
                       List<AffectData> affectList = new List<AffectData>();
                        
                        
                       for(int j = 0; j < StanceInfo.system_ComboAffect_Code.Count; j++)
                       {
                           affectList.Add(CDataManager.GetAffect((int)StanceInfo.system_ComboAffect_Code[j]));                                                       
                       }
                       PlayerManager.instance.playerInfoList[PlayerManager.MYPLAYER_INDEX].playerCharBase.ComboOption_Set(StanceInfo);
                       SceneManager.instance.GetUIComponent(SceneManager.eCOMPONENT.Combo).GetComponent<UI_Combo>().DoCombo(affectList, StanceInfo, PlayerManager.instance.playerInfoList[PlayerManager.MYPLAYER_INDEX].playerCharBase.m_ComboCount);
                       affectList.Clear();
                       affectList = null;
                    }
                }
            }
            else if (bHit == false )
            {
                if (InGameManager.instance.m_bOffLine == true || m_bCheckOnly == true)
                    targets.RemoveAt(i);
            }
        }
    }


    private List<CharacterBase> GetTargetReachedCollider(CharacterBase excutioner, List<CharacterBase> allTargets, Vector3 moveVector)
    {
        List<CharacterBase> npcs = allTargets;
        List<CharacterBase> targets = new List<CharacterBase>();

        //Vector2 direction = excutioner.transform.forward;
        Vector2 direction = new Vector2();
        direction.x = excutioner.transform.forward.x;
        direction.y = excutioner.transform.forward.z;

        for ( int i = 0; i < npcs.Count; i++ )
        {
            float npcColliderRadius      = npcs[i].GetRadius();
            float pcColliderRadius       = m_executioner.GetRadius();
            float angle                  = m_skillInfo.area_Angle * 0.5f;

            Vector2 npcPosition         = new Vector2( npcs[i].transform.position.x, npcs[i].transform.position.z);
            Vector2 pcPosition          = new Vector2(excutioner.transform.position.x, excutioner.transform.position.z);

            float distance              = Vector2.Distance(pcPosition, npcPosition);
            float fValue                = Vector2.Angle(direction, npcPosition - pcPosition);

            if (npcColliderRadius + pcColliderRadius + m_skillInfo.area_Size >= distance)
            {
                if (fValue <= angle)
                    targets.Add(npcs[i]);
                else
                {
                    // 좌측 레이
                    Vector3 vRayDir = Quaternion.AngleAxis(-angle, new Vector3(0, 1, 0)) * new Vector3(direction.x, 0, direction.y);
                    if (IntersectCircle(pcPosition, new Vector2(vRayDir.x, vRayDir.z), npcPosition, npcColliderRadius))
                    {
                        targets.Add(npcs[i]);
                    }
                    else
                    {
                        // 우측 레이
                        vRayDir = Quaternion.AngleAxis(angle, new Vector3(0, 1, 0)) * new Vector3(direction.x, 0, direction.y);
                        if (IntersectCircle(pcPosition, new Vector2(vRayDir.x, vRayDir.z), npcPosition, npcColliderRadius))
                        {
                            targets.Add(npcs[i]);
                        }
                    }
                }
            }
        }
        return targets;
    }
    private void SendPush(List<CharacterBase> targets, Vector3 moveVector)
    {
        CharacterBase targetScr = null;
        List<CharacterBase> kTargets = new List<CharacterBase>();
        //List<int> kBuffList = new List<int>();

        for (int i = 0; i < targets.Count; i++)
        {
            /// 푸시 상태 전달 
            targetScr = targets[i];
            if (targetScr.m_CharAi.GetNpcProp() != null )
            {
                if (targetScr.m_CharAi.GetNpcProp().Battle_Type == eNPC_BATTLE_TYPE.eOBJECT)
                    break;
                if (targetScr.m_CharAi.GetNpcProp().Npc_Size >= 3)
                    break;
            }
            BuffData_ReAction kBuff = targetScr.m_BuffController.FindFrontReActionBuff();

            if (kBuff == null || kBuff.m_AffectCode != eAffectCode.ePUSHING)
            {
                BuffData kNewKnockBack = BuffController.CreateAttackReActionFactory(m_executioner, targetScr.m_BuffController, eAffectCode.ePUSHING, 0);
                if( targetScr.m_BuffController.AddBuff(kNewKnockBack) == BuffController.eBuffResult.eSUCCESS)
                {
                    targetScr.m_PushingMove.m_Dir = m_executioner.m_MyObj.forward;
                    targetScr.m_PushingMove.m_fPushingTime = m_executioner.m_pushInfo.m_fDuration;
                    targetScr.m_PushingMove.m_Speed = m_executioner.m_pushInfo.m_fSpeed;

                    if (InGameManager.instance.m_bOffLine == false)
                        kTargets.Add(targetScr);
                }
            }
        }
    }

    public List<CharacterBase> GetTargetsInLine(List<CharacterBase> targets,float p_fSize, Vector3 vecCenter, Vector3 vecLine)
    {
        List<CharacterBase> allTargets = targets;
        List<CharacterBase> targetsInCircle = new List<CharacterBase>();

        Vector2 centerPosition = new Vector2(vecCenter.x, vecCenter.z);

        for (int i = 0; i < allTargets.Count; i++)
        {
            if (allTargets[i].m_CharState == CHAR_STATE.DEATH)
                continue;

            bool bContinue = false;

            // 데미지 계산에서 제외 될 사람
            if (m_Exceptiontargets != null)
            {
                for (int j = 0; j < m_Exceptiontargets.Count; ++j)
                {
                    if (m_Exceptiontargets[j] == allTargets[i])
                    {
                        bContinue = true;
                        break;
                    }
                }
            }
            if (bContinue)
                continue;


            Transform target = allTargets[i].transform;
            Vector2 targetPosition = new Vector2(target.position.x, target.position.z);

            Vector2 LineDir = new Vector2(vecLine.x, vecLine.z);
            float fDistance = Vector2.Distance(centerPosition, targetPosition);
            Vector2 vecPlog = targetPosition - centerPosition;
            LineDir = LineDir - centerPosition;
            float fLineDist = LineDir.magnitude;
            float fDotP = Vector2.Dot(vecPlog, LineDir);

            float fRadius = m_executioner.GetRadius() + allTargets[i].GetRadius();

            if (fDotP > 0.0f && fDistance - fRadius < fLineDist)
            {
                Vector2 pProj = (fDotP / Vector3.Dot(LineDir, LineDir)) * LineDir;
                Vector2 H = (vecPlog - pProj);
                float fSkillArea = allTargets[i].GetRadius() + p_fSize;
                if (H.sqrMagnitude < fSkillArea * fSkillArea)
                {
                    targetsInCircle.Add(allTargets[i]);
                }
            }
        }
        return targetsInCircle;
    }

    public void AddExceptionTarget( CharacterBase p_Target )
    {
        if (p_Target == null)
            return;

        if( m_Exceptiontargets == null)
            m_Exceptiontargets = new List<CharacterBase>();

        m_Exceptiontargets.Add(p_Target);
    }
    public void ClearExceptionTarget()
    {
        if (m_Exceptiontargets != null)
            m_Exceptiontargets.Clear();

        if (m_hitTargetInfo != null)
            m_hitTargetInfo.Clear();
    }

#endregion PRIVATE FUNC
}
