using UnityEngine;
using System.Collections;

public class UICompoundBtnEffct : MonoBehaviour 
{
	[SerializeField] UISprite mSpr;
	[SerializeField] float dtTime = 0.3f;
	N_ImageButton mBtn = null; 

	UICompoundWndControl comCtrl;
	// Use this for initialization
	void Start () 
	{
		mBtn = GetComponent<N_ImageButton>();
		if (GameUI.Instance != null)
			comCtrl = GameUI.Instance.mCompoundWndCtrl;
	}

	float time = 0;
	// Update is called once per frame
	void Update () 
	{
		if (mBtn != null && mSpr!= null)
		{
			if (comCtrl != null)
			{
				if (comCtrl.IsCompounding)
				{
					time += Time.deltaTime;
					if (time > dtTime)
					{
						mSpr.spriteName = (mSpr.spriteName == "Craft1") ? "Craft2" : "Craft1"; 
						time = 0;
					}
				}
				else
					mSpr.spriteName = "Craft1";
			}
		}
	}
}
