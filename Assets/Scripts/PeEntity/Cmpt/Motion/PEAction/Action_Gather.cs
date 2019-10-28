using UnityEngine;
using System.Collections.Generic;
using NaturalResAsset;

namespace Pathea
{
	[System.Serializable]
	public class Action_Gather : PEAction
	{
		public override PEActionType ActionType { get { return PEActionType.Gather; } }

		const int	SkillID = 20110003;

		const float	GatherMaxDis = 3f;

		bool m_EndAnim;
		
		GlobalTreeInfo mOpTreeInfo;
		GlobalTreeInfo mFindTreeInfo;
		public GlobalTreeInfo treeInfo{ get { return mOpTreeInfo; } set{mOpTreeInfo = mFindTreeInfo = value;}}

		public bool UpdateOPTreeInfo()
		{
			mFindTreeInfo = null;
			if(null == trans)
				return false;
			if(Vector3.Distance(trans.position, trans.position) > GatherMaxDis)
				return false;
			List<GlobalTreeInfo> grassInfoList;
			if(null != LSubTerrainMgr.Instance)
				grassInfoList = LSubTerrainMgr.Picking(trans.position, Vector3.forward, false, GatherMaxDis, 360f);
			else if(null != RSubTerrainMgr.Instance)
				grassInfoList = RSubTerrainMgr.Picking(trans.position, Vector3.forward, false, GatherMaxDis, 360f);
			else
				return false;
			
			for(int i = 0; i < grassInfoList.Count; i++)
			{
				NaturalRes resFind = NaturalResAsset.NaturalRes.GetTerrainResData(grassInfoList[i]._treeInfo.m_protoTypeIdx + 1000);
				if (null != resFind)
				{
					if (resFind.m_type == 10)
					{
						if(!PeCamera.cursorLocked)
						{
							if(null != LSubTerrainMgr.Instance)
							{
								Vector3 pos = grassInfoList[i].WorldPos;

								Bounds bound = new Bounds();
								bound.SetMinMax(pos + grassInfoList[i]._treeInfo.m_heightScale * LSubTerrainMgr.Instance.GlobalPrototypeBounds[grassInfoList[i]._treeInfo.m_protoTypeIdx].min,
								                pos + grassInfoList[i]._treeInfo.m_heightScale * LSubTerrainMgr.Instance.GlobalPrototypeBounds[grassInfoList[i]._treeInfo.m_protoTypeIdx].max);
								if(!bound.IntersectRay(PeCamera.mouseRay))
									continue;
							}
							else if(null != RSubTerrainMgr.Instance)
							{
								Vector3 pos = grassInfoList[i]._treeInfo.m_pos;
								Bounds bound = new Bounds();
								bound.SetMinMax(pos + grassInfoList[i]._treeInfo.m_heightScale * RSubTerrainMgr.Instance.GlobalPrototypeBounds[grassInfoList[i]._treeInfo.m_protoTypeIdx].min,
								                pos + grassInfoList[i]._treeInfo.m_heightScale * RSubTerrainMgr.Instance.GlobalPrototypeBounds[grassInfoList[i]._treeInfo.m_protoTypeIdx].max);
								if(!bound.IntersectRay(PeCamera.mouseRay))
									continue;
							}
						}

						mFindTreeInfo = grassInfoList[i];
						break;
					}
				}
			}

			return null != mFindTreeInfo;
		}
		
		public override bool CanDoAction (PEActionParam para = null)
		{
			return null != trans && null != mFindTreeInfo && !motionMgr.isInAimState;
		}

		public override void PreDoAction ()
		{
			base.PreDoAction ();
			motionMgr.SetMaskState(PEActionMask.Gather, true);
			mOpTreeInfo = mFindTreeInfo;
		}
		
		public override void DoAction (PEActionParam para = null)
		{
			if(null != anim)
			{
				anim.ResetTrigger("ResetFullBody");
				anim.SetTrigger("Gather");
			}

			m_EndAnim = false;
		}
		
		public override bool Update ()
		{
			if(null != anim)
			{
				if(m_EndAnim)
				{
					motionMgr.SetMaskState(PEActionMask.Gather, false);
					return true;
				}
			}
			else
			{
				return true;
			}
			return false;
		}
		
		public override void EndImmediately ()
		{
			motionMgr.SetMaskState(PEActionMask.Gather, false);
			if(null != anim)
			{
				anim.SetTrigger("ResetFullBody");
				anim.ResetTrigger("Gather");
			}
			mOpTreeInfo = mFindTreeInfo = null;
		}
		
		void Gather()
		{
			if(null != mOpTreeInfo && null != skillCmpt)
			{
				if(SkEntitySubTerrain.Instance.GetTreeHP(mOpTreeInfo.WorldPos) <= PETools.PEMath.Epsilon)
				{
					motionMgr.EndAction(ActionType);
					return;
				}
				skillCmpt.StartSkill(SkEntitySubTerrain.Instance, SkillID);
			}
		}

		protected override void OnAnimEvent (string eventParam)
		{			
			if(motionMgr.IsActionRunning(ActionType))
			{
				switch(eventParam)
				{
				case "Gather":
					Gather();
					break;
				case "GatherEnd":
					m_EndAnim = true;
					break;
				}
			}
		}
	}
}
