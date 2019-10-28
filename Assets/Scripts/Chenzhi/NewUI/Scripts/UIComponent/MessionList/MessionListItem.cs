using UnityEngine;
using System.Collections;

public class MessionListItem : MonoBehaviour 
{

	public bool IsExpand
	{
		get {
			return _IsExpand;
		}
	}

	public UICheckbox mCheckBoxTag;
	public UILabel mLbTitle;
	public UISprite mSpBg;
	public UISprite mSpState;
	public GameObject mTeewnContent;

	public object mData = null;


	private bool _IsExpand = false;
	void Awake()
	{
		_IsExpand = mTeewnContent.activeSelf;
	}

	void TeewnContentOnExpand()
	{
		if (_IsExpand)
			_IsExpand = false;
		else 
			_IsExpand = true;

		mSpState.spriteName = _IsExpand ? "mission_open" : "mission_closed";
	}

}
