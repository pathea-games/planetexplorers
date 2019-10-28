using UnityEngine;
using System.Collections;

public class UITalkItem : MonoBehaviour
{

	public UILabel mText;

	// Use this for initialization
	void Start () 
	{
	
	}
	
	// Update is called once per frame
	void Update () 
	{
	
	}

	public void SetText(string strtext)
	{
		if(mText == null)
			return;
		mText.text = strtext;
	}
}
