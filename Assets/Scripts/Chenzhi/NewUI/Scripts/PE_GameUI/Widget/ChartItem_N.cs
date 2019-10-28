using UnityEngine;
using System.Collections;

public class ChartItem_N : MonoBehaviour
{
	public UILabel mName;
	public UILabel mContent;
	
	public void SetText(string name, string content)
	{
		mName.text = name;
		mContent.text = content;
	}
}
