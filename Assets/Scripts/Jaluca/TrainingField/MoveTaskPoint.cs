using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace TrainingScene
{
	public class MoveTaskPoint : MonoBehaviour
	{
		Transform playerTransform;
		Camera maincmr;
		//Camera uicmr;
		Vector3 scrPos;
		RectTransform targetSign;
		MoveTaskScreen screenBorder;
		[SerializeField] Transform marker;
		[SerializeField] string title;
		[SerializeField] string titleC;
		[SerializeField] string discription;
		[SerializeField] string discriptionC;
		[SerializeField] float detectDistancePow = 1f;

		void Start()
		{
            playerTransform = Pathea.PeCreature.Instance.mainPlayer.peTrans.trans;
			maincmr = Camera.main;
			//uicmr = GameUI.Instance.mUICamera;
			screenBorder = MoveTask.Instance.screenBorder;
		}
		public void InitStr()
		{
			targetSign = MoveTask.Instance.targetSign;
			targetSign.Find("Text").GetComponent<Text>().text = discriptionC;
			targetSign.Find("Title").GetComponent<Text>().text = titleC;
		}
		void Update()
		{
			scrPos = maincmr.WorldToScreenPoint(marker.position);
			if(0 < scrPos.z)			
				targetSign.position = new Vector3(scrPos.x + 100, scrPos.y + 25, 0);			
			else
				targetSign.position = Vector3.one * 10000;
			if((transform.position - playerTransform.position).sqrMagnitude < detectDistancePow)
			{
				screenBorder.gameObject.SetActive(true);
				screenBorder.FadeOnScreen();
				MoveTask.Instance.NextPoint();
			}
		}
	}
}
