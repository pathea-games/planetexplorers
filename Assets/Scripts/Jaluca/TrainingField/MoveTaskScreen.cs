using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace TrainingScene
{
	public class MoveTaskScreen : MonoBehaviour
	{
		[SerializeField] float minWidth = 40f;
		[SerializeField] float minHeight = 20f;
		[SerializeField] float maxWidth;
		[SerializeField] float maxHeight;
		[SerializeField] float fadeOnTime1 = 0.2f;
		[SerializeField] float fadeOnTime2 = 0.2f;
		float ctime;
		RectTransform rt;
		Vector2 wh = Vector2.zero;

		void Awake()
		{
			rt = GetComponent<RectTransform>();
		}

		public void FadeOnScreen()
		{
			rt.sizeDelta = new Vector2(minWidth, minHeight);
			ctime = 0f;
			StartCoroutine(FadeOnBorder(fadeOnTime1 + fadeOnTime2, maxWidth - minWidth, maxHeight - minHeight));
		}
		IEnumerator FadeOnBorder(float sumTime, float dx, float dy)
		{
			while(true)
			{
				yield return new WaitForFixedUpdate();
				ctime += Time.fixedDeltaTime;
				if(ctime < fadeOnTime1)
				{
					wh.y = minHeight;
					wh.x = minWidth + dx * ctime / fadeOnTime1;
					rt.sizeDelta = wh;
				}
				else if(ctime < sumTime)
				{
					wh.y = minHeight + dy * (ctime - fadeOnTime1) / fadeOnTime2;
					wh.x = maxWidth;
					rt.sizeDelta = wh;
				}
				else
				{
					wh.y = maxHeight;
					wh.x = maxWidth;
					rt.sizeDelta = wh;
					Invoke("FadeOutBorder", 4f);
					break;
				}
			}
		}
		void FadeOutBorder()
		{
			gameObject.SetActive(false);
		}
	}
}