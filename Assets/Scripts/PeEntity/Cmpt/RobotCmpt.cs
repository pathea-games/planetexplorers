using UnityEngine;
using System;
using System.Collections;
using SkillSystem;

namespace Pathea
{
	public class RobotCmpt : PeCmpt,IPeMsg
	{
		//PEBarrelController m_Barrel;




		#region public funtion
		public void Translate(Vector3 pos)
		{
			Vector3 Hpos = pos;
			Hpos.y += 5.0f;
			Entity.peTrans.position = Hpos;
		}

		#endregion

		#region unity fun
		// Use this for initialization
		//void Start () {
			
		//}
		
		// Update is called once per frame
		void Update () {
			
		}

		public override void OnDestroy()
		{
			base.OnDestroy();
			GameObject.DestroyObject(this.gameObject);
		}

		#endregion

		public void OnMsg(EMsg msg, params object[] args)
		{
			switch (msg)
			{
			case EMsg.View_Prefab_Build:
				//BiologyViewRoot viewRoot = (BiologyViewRoot)args[1];
				//m_Barrel = viewRoot.barrelController;
				break;
			default:
				break;
			}
		}
	}
}


