using UnityEngine;
using System.Collections;

public class UISwitchLocalization : MonoBehaviour 
{
	// Use this for initialization
	void Start () 
	{
		UIPopupList[] findPopList = GetComponentsInChildren<UIPopupList> (true);
		foreach (UIPopupList pop in findPopList) 
		{
			for(int i = 0; i < pop.items.Count; ++i)
			{
				pop.items[i] = pop.items[i].ToLocalizationString();
				pop.selection = pop.selection.ToLocalizationString();
				pop.textLabel.MakePixelPerfect ();
			}
		}
		UILabel[] findLabels = GetComponentsInChildren<UILabel> (true);
		foreach (UILabel label in findLabels) {
			label.text = label.text.ToLocalizationString();
			label.MakePixelPerfect ();
		}

	}
}
