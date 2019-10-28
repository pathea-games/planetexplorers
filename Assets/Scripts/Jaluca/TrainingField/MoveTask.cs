using UnityEngine;
using System.Collections;

namespace TrainingScene
{
	public class MoveTask : MonoBehaviour
	{
		private static MoveTask s_instance;
		public static MoveTask Instance{get{return s_instance;}}
		[SerializeField] MoveTaskPoint[] points;
		public RectTransform targetSign;
		public MoveTaskScreen screenBorder;
		int currentPoint = 0;

		void Awake()
		{
			s_instance = this;
		}
		public void InitScene()
		{
			targetSign.gameObject.SetActive(true);
			points[0].gameObject.SetActive(true);
			points[0].InitStr();

//			appearance[0].transform.parent.gameObject.SetActive(true);
//			herbDead = 0;
//			foreach(HoloherbGather gth in gather)
//				gth.isDead = false;
//			HoloCameraControl.Instance.renderObjs1.Add(herbsBase);
//			foreach(HoloherbAppearance app in appearance)	
//				app.produce = true;
		}
		public void DestroyScene()
		{
			currentPoint = 0;
			targetSign.gameObject.SetActive(false);
			foreach(MoveTaskPoint i in points)
			{
				i.gameObject.SetActive(false);
			}
//			Invoke("CloseMission", appearance[0].fadeTime + 0.2f);
		}
		void CloseMission()
		{
//			HoloCameraControl.Instance.renderObjs1.Clear();
//			appearance[0].transform.parent.gameObject.SetActive(false);
		}
		public void NextPoint()
		{
			points[currentPoint].gameObject.SetActive(false);
			if(++currentPoint != points.Length)
			{
				points[currentPoint].gameObject.SetActive(true);
				points[currentPoint].InitStr();
			}
			else
				DestroyScene();
		}
	}
}
