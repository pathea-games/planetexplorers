using UnityEngine;
using System.Collections;

namespace TrainingScene
{
	public class HolotreeTask : MonoBehaviour 
	{
		private static HolotreeTask s_instance;
		public static HolotreeTask Instance{get{return s_instance;}}
		[SerializeField] HolotreeCutter cutter;
		[SerializeField] HolotreeAppearance appearance;
		[SerializeField] GameObject treeBase;
		[SerializeField] int MaxHP;
		int HP;
		//float hppct;

		void Awake()
		{
			s_instance = this;
		}
		public void InitScene()
		{		
			appearance.gameObject.SetActive(true);
			transform.GetComponent<Collider>().enabled = true;
			HP = MaxHP;
			//hppct = 1f;
			HoloCameraControl.Instance.renderObjs1.Add(treeBase);
			appearance.produce = true;
		}
		public void DestroyScene()
		{
			appearance.destroy = true;
			Invoke("CloseMission", appearance.fadeTime + 0.8f);
		}
		void CloseMission()
		{
			HoloCameraControl.Instance.renderObjs1.Clear();
			appearance.gameObject.SetActive(false);
            transform.GetComponent<Collider>().enabled = false;
		}
		public bool SubHP()
		{
			if(--HP == 0)
			{	
				appearance.destroy = true;
				DestroyScene();
				return true;
			}
			return false;
		}
	}
}
