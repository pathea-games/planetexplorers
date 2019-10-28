using UnityEngine;
using System.Collections;

public class RecordItem_N : MonoBehaviour
{
	public UILabel		mCountLabel;

	public UITexture	mMapTex;
	public UILabel		mAreaLabel;
	public UILabel		mPlayTimeLabel;
	public UILabel		mGameTimeLabel;
	public UILabel		mSaveTimeLabel;
	
	UISaveLoad		mParent;
	int					mIndex;
	
	public void SetItem(int Index, Texture tex, string area, string playTime
		, string gameTime, string saveTime, UISaveLoad parent)
	{
		mIndex = Index;
		if(mIndex == -1)
			mCountLabel.text = "Auto";
		else
			mCountLabel.text = Index.ToString();
			
		mMapTex.mainTexture = tex;
		mAreaLabel.text = area;
		mPlayTimeLabel.text = playTime;
		mGameTimeLabel.text = gameTime;
		mSaveTimeLabel.text = saveTime;
	}
	
	void OnClick ()
	{
//		parent.
	}
}
