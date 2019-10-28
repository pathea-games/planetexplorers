using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using ItemAsset;
using Pathea.Operate;
using PETools;
using SkillSystem;
using Pathea.PeEntityExt;
using Pathea.PeEntityExtTrans;
using Mono.Data.SqliteClient;

namespace Pathea
{
	public class PeNpcGroup : MonoBehaviour {
		
		static  PeNpcGroup mInstance;
		public static PeNpcGroup Instance { get {return mInstance;}}
		
		private List<Camp>            mCamps;
		private List<PeEntity>        mCSNpcs;
		
		CSCreator mCScreator;
		CSNpcTeam mCSnpcTeam;


		IEnumerator CSTeam(float time)
		{
			if (SingleGameStory.curType == SingleGameStory.StoryScene.TrainingShip) 
				yield break;
			
			yield return new WaitForSeconds(time);
			while(true)
			{
				mCScreator = CSMain.GetCreator(CSConst.ciDefMgCamp);
				if(mCScreator != null)
				{
					mCSNpcs = CSMain.GetCSNpcs(mCScreator);
					mCSnpcTeam.setCSCreator(mCScreator);
					if(mCScreator.Assembly != null && mCSNpcs.Count >0)
					{
						mCSnpcTeam.InitTeam();
						mCSnpcTeam.AddInTeam(mCSNpcs,true);
						mCSnpcTeam.ReFlashTeam();
						
					}
				}

				yield return new WaitForSeconds(time);
			}

		}

		IEnumerator CampTeam(float time)
		{
			yield return new WaitForSeconds (time);
		}

		IEnumerator Tracking(PeEntity entity,float time)
		{
			while(true)
			{
				yield return new WaitForSeconds(time);
			}
		}

        public bool InDanger { get { return mCSnpcTeam != null ? mCSnpcTeam.mIndanger : false; } }

		public void OnCSAttackEnmey(PeEntity entity,PeEntity enmey)
		{
			bool canAttcak = PETools.PEUtil.CanAttackReputation(entity,enmey);
			if(canAttcak)
			{
				mCSnpcTeam.OnAlertInform(enmey);

                if (mCScreator != null)
                    NpcSupply.SupplyNpcsByCSAssembly(mCSNpcs,mCScreator.Assembly,ESupplyType.Ammo);
			}
		}

		public void OnCsAttackEnd()
		{
			mCSnpcTeam.OnClearAlert();
		}

		public void OnCsAttackTypeChange(PeEntity entity,AttackType oldType,AttackType newType)
		{
			if(mCScreator != null && mCScreator.Assembly != null && mCSnpcTeam != null)
			{
				mCSnpcTeam.OnAttackTypeChange(entity,oldType,newType);
			}
		}

		public void OnCSAddDamageHaterd(PeEntity enmey,PeEntity beDamage,float hater)
		{
			if(mCScreator == null || mCScreator.Assembly == null)
				return ;

			if(beDamage == null || enmey == null)
				return ;

			if(beDamage.NpcCmpt == null || beDamage.NpcCmpt.Creater == null || beDamage.NpcCmpt.Creater.Assembly == null)
				return ;

			float r = PETools.PEUtil.MagnitudeH(mCScreator.Assembly.Position,enmey.position);
			if(r <= mCScreator.Assembly.Radius * 1.5f )
			{
				OnCSAttackEnmey(beDamage,enmey);
			}


		}

		public void OnCsLineChange(PeEntity member,ELineType oldType,ELineType newType)
		{
			if(mCScreator != null && mCScreator.Assembly != null && mCSnpcTeam != null)
			{
				mCSnpcTeam.OnLineChange(member,oldType,newType);
			}
		}

		public void OnCsJobChange(PeEntity member,ENpcJob oldJob,ENpcJob newJob)
		{
			if(mCScreator != null && mCScreator.Assembly != null && mCSnpcTeam != null)
			{
				mCSnpcTeam.OnCsNpcJobChange(member,oldJob ,newJob);
			}
		}

		public void OnRemoveCsNpc(PeEntity member)
		{
			if(mCScreator != null && mCScreator.Assembly != null && mCSnpcTeam != null)
			{
				mCSnpcTeam.OnCsNPcRemove(member);
			}
		}

		public void OnEnemyLost(Enemy enemy)
		{
			if(mCScreator != null && mCScreator.Assembly != null && mCSnpcTeam != null)
			{
				mCSnpcTeam.OnTargetLost(enemy.entityTarget);
			}
		}

        public void OnAssemblyHpChange(SkillSystem.SkEntity caster, float hpChange)
        {
            if (mCScreator != null && mCScreator.Assembly != null && mCSnpcTeam != null)
            {
                mCSnpcTeam.OnAssemblyHpChange(caster,hpChange);
            }
        }

		void Awake()
		{
			mInstance = this;
			if(mCSnpcTeam == null)
				mCSnpcTeam = new CSNpcTeam();
			//InitGroup();
		}
		// Use this for initialization
		void Start () {

			StartCoroutine(CSTeam(5.0f));
			StartCoroutine(CampTeam(5.0f));
		}
	}
}
