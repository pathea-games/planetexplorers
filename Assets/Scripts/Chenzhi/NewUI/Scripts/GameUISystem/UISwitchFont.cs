using UnityEngine;
using System.Collections;

public class UISwitchFont : MonoBehaviour 
{
	bool mInit = false;

	IEnumerator Start()
	{
		while (true) {
			if (!mInit && UIFontMgr.Instance != null) {
				mInit = true;
			
				UILabel[] findLabels = GetComponentsInChildren<UILabel> (true);
				foreach (UILabel label in findLabels) {
					label.font = UIFontMgr.Instance.GetFontForLanguage (label.font);
					label.MakePixelPerfect ();
				}
			
				UIPopupList[] findPopList = GetComponentsInChildren<UIPopupList> (true);
				foreach (UIPopupList pop in findPopList) {
					pop.font = UIFontMgr.Instance.GetFontForLanguage (pop.font);
					pop.textLabel.MakePixelPerfect ();
				}
				yield break;
			}
			yield return 0;
		}
	}
}
