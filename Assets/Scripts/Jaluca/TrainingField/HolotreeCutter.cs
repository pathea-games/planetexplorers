using UnityEngine;
using System.Collections;
using System;
using Pathea;

namespace TrainingScene
{
	public class HolotreeCutter : MonoBehaviour
	{
		public int MaxHP;        
		[HideInInspector] public int HP;
		PeEntity mplayer;
		AnimatorCmpt anmt;
		Action_Fell af;
		MotionMgrCmpt mmc;
		HolotreeTask ht;
		bool iscut0 = false;
		bool iscut1 = false;
		bool iscut2 = false;
		bool cutting = false;
		bool ishited = false;
		bool iscut3 = false;
		//bool loop = false;
		float ctime = 0f;
		float hppct = 1f;

		void Start()
		{
			if(MaxHP <=0)
				MaxHP = 1;
			HP = MaxHP;
			mplayer = PeCreature.Instance.mainPlayer;
			anmt = mplayer.animCmpt;
			mmc = mplayer.motionMgr;
			af = mmc.GetAction<Action_Fell>();
			ht = HolotreeTask.Instance;
		}
		void Update()
		{
            // 
            if (af.m_Axe && mmc.GetMaskState(PEActionMask.EquipmentHold) && !cutting
			   && PeInput.Get(PeInput.LogicFunction.Cut))
			{
				RaycastHit[] rhs = Physics.RaycastAll(new Ray(mplayer.position, mplayer.forward), 2f);
				foreach(RaycastHit rh in rhs)
				{
					if(rh.transform == transform.parent.parent)
					{
						mmc.EndImmediately(PEActionType.Move);
						mmc.EndImmediately(PEActionType.Sprint);
						mmc.EndImmediately(PEActionType.Rotate);

						ctime = -Time.deltaTime;
						iscut0 = true;
						anmt.SetBool("Fell", true);
						UITreeCut.Instance.Show();
						UITreeCut.Instance.SetSliderValue(null, hppct);
						break;
					}
				}
			}
			else if(!PeInput.Get(PeInput.LogicFunction.Cut))
			{
				anmt.SetBool("Fell", false);
			}				

			if(iscut0 && anmt.IsAnimPlaying("DoNothing", 1))
			{
				mmc.SetMaskState(PEActionMask.Fell, true);
				cutting = true;
				iscut0 = false;
				iscut1 = true;
			}

			if(cutting)
				ctime += Time.deltaTime;

			if(iscut1)
			{				
				if(ctime > TrainingRoomConfig.Felling1time)
				{
					iscut1 = false;
					iscut2 = true;
					ctime -= TrainingRoomConfig.Felling1time;
				}
			}
			else if(iscut2)
			{
				if(ctime >= TrainingRoomConfig.Felling2time)
				{
					ctime -= TrainingRoomConfig.Felling2time;
					ishited = false;
					if(!anmt.GetBool("Fell"))
					{
						iscut2 = false;
						iscut3 = true;
					}
				}
				else if(ctime >= TrainingRoomConfig.FellingHittime)
				{
					if(!ishited)
					{
						ishited = true;
						if(null != af.m_Axe)
						{
                            Transform trCutP = af.m_Axe.transform.Find("CutPoint");
                            ApplyCut(trCutP != null ? trCutP.position : Vector3.zero);
							ht.SubHP();
						}
						else
						{
							anmt.SetBool("Fell", false);
							cutting = false;
							iscut0 = false;
							iscut1 = false;
							iscut2 = false;
							iscut3 = false;
							mmc.SetMaskState(PEActionMask.Fell, false);
							UITreeCut.Instance.Hide();
						}
					}
					if(HP == 0)
						anmt.SetBool("Fell", false);
				}
			}
			else if(iscut3 && ctime >= TrainingRoomConfig.Felling3time)
			{
				cutting = false;
				iscut3 = false;
				mmc.SetMaskState(PEActionMask.Fell, false);
				UITreeCut.Instance.Hide();
			}
		}
		void ApplyCut(Vector3 effPoint)
		{
			hppct = --HP / (float)MaxHP;
			transform.GetComponent<HolotreeAppearance>().hppct = hppct;

			//HP slider
			if(hppct < 0.01f)
				UITreeCut.Instance.Hide();
			else
				UITreeCut.Instance.SetSliderValue(null, hppct);

			//effect
            if(effPoint != Vector3.zero)
            {
                GameObject pt = MonoBehaviour.Instantiate(Resources.Load("Prefab/Particle/FX_treeWood_01"), effPoint, Quaternion.identity) as GameObject;
                pt.AddComponent<DestroyTimer>();
                pt.GetComponent<DestroyTimer>().m_LifeTime = 3f;
            }
			

			//sound
			transform.GetComponent<AudioSource>().volume = SystemSettingData.Instance.EffectVolume * SystemSettingData.Instance.SoundVolume;
			transform.GetComponent<AudioSource>().Play();

			//tree is dead
			if(HP <= 0)
			{
                PeCreature.Instance.mainPlayer.GetCmpt<PlayerPackageCmpt>().Add(279, 1);
				TrainingTaskManager.Instance.CompleteMission();			                
				transform.parent.parent.GetComponent<Collider>().enabled = false;
			}
		}
	}
}