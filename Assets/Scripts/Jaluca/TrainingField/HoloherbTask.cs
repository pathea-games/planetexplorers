using UnityEngine;
using System.Collections;

namespace TrainingScene
{
	public class HoloherbTask : MonoBehaviour
	{
		private static HoloherbTask s_instance;
		public static HoloherbTask Instance{get{return s_instance;}}
		[SerializeField] HoloherbGather[] gather;
		[SerializeField] HoloherbAppearance[] appearance;
		[SerializeField] GameObject herbsBase;
		[SerializeField] int herbCount;
		int herbDead;

		void Awake()
		{
			s_instance = this;
		}
		public void InitScene()
		{
			appearance[0].transform.parent.gameObject.SetActive(true);
			herbDead = 0;
			foreach(HoloherbGather gth in gather)
				gth.isDead = false;
			HoloCameraControl.Instance.renderObjs1.Add(herbsBase);
			foreach(HoloherbAppearance app in appearance)	
				app.produce = true;
		}
		public void DestroyScene()
		{
			Invoke("CloseMission", appearance[0].fadeTime + 0.2f);
		}
		void CloseMission()
		{
			HoloCameraControl.Instance.renderObjs1.Clear();
			appearance[0].transform.parent.gameObject.SetActive(false);
		}
		public void SubHerb(HoloherbAppearance ha)
		{
			ha.destroy = true;
            GameObject.Destroy(ha.col);
			if(++herbDead == herbCount)
			{
				TrainingTaskManager.Instance.CompleteMission();
				DestroyScene();
			}
		}
	}
}
