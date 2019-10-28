using UnityEngine;
using System.Collections;
using System;
using Pathea;

namespace TrainingScene
{
	public class HoloherbGather : MonoBehaviour
	{
		public HoloherbAppearance appearance;
		PeEntity mplayer;
		Transform playerTrans;
		AnimatorCmpt anmt;
		MotionMgrCmpt mmc;
		bool isGathering = false;
		[HideInInspector] public bool isDead = false;
		float ctime = 0f;
		
		void Start()
		{
			mplayer = PeCreature.Instance.mainPlayer;
			playerTrans = mplayer.peTrans.trans;
			anmt = mplayer.GetCmpt<AnimatorCmpt>();
			mmc = mplayer.GetCmpt<MotionMgrCmpt>();
		}
		void Update()
		{
			if(!isDead && PeInput.Get(PeInput.LogicFunction.GatherHerb)
//			   && mplayer.rigidbody.velocity.magnitude < 0.5f
			   && !isGathering)
			{
				if(Vector3.SqrMagnitude(playerTrans.position - transform.position) < 7f )
				{
					mmc.EndImmediately(PEActionType.Move);
					mmc.EndImmediately(PEActionType.Sprint);
					mmc.EndImmediately(PEActionType.Rotate);

					ctime = -Time.deltaTime;
					isGathering = true;
					anmt.SetBool("Gather", true);
					mmc.SetMaskState(PEActionMask.Gather, true);
				}
			}
			if(isGathering)
			{
				ctime += Time.deltaTime;
				if(ctime < TrainingRoomConfig.HerbGatherTime)
				{
					if(!isDead && ctime >= TrainingRoomConfig.HerbHitTime)					
						ApplyCut();					
				}
				else
					isGathering = false;
			}
		}
		void ApplyCut()
		{
			isDead = true;
//			appearance.isNotDead = false;
			HoloherbTask.Instance.SubHerb(appearance);
			anmt.SetBool("Gather", false);
			mmc.SetMaskState(PEActionMask.Gather, false);
//			if(isDead)
//			{
//				if(++HoloherbTask.Instance.herbDead >= HoloherbTask.Instance.herbCount)
//					TrainingTaskManager.Instance.CompleteMission();
//			}
		}
	}
}