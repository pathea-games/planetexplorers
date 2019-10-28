using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UIOptionMenu : UIStaticWnd 
{
	[SerializeField] GameObject mOptionPrefab;
	[SerializeField] UIGrid mGrid;
	[SerializeField] UISprite mSprBg;
	public int mItemHeight = 30;
	public int mItemWidth = 140;
	[HideInInspector]
	private List<UIOptionMenuItem> mItems = new List<UIOptionMenuItem>();
	private Camera uiCamera;
	private Collider mBgCollider;

	// Use this for initialization
	public void Init (Camera _uiCamera) 
	{
        //lz-2017.02.27 错误 #8726 crash bug
        if (null == this) return;
        if (mSprBg) mBgCollider = mSprBg.GetComponent<BoxCollider>();
        if (null != _uiCamera) uiCamera = _uiCamera;
    }
	
	public override void Show ()
	{
		Repostion();
		CalculatePos();
		base.Show ();
	}

	protected override void OnHide()
	{
		base.OnHide ();
	}
	
	public void AddOption(string text , UIOptionMenuItem.BaseMsgEvent clickCallBack)
	{
		GameObject obj = GameObject.Instantiate(mOptionPrefab) as GameObject;
		obj.transform.parent = mGrid.transform;
		obj.transform.localPosition = new Vector3(0,0,-2);
		obj.transform.localScale = Vector3.one;
		UIOptionMenuItem option = obj.GetComponent<UIOptionMenuItem>();
		option.e_OnClickItem += clickCallBack;
		option.Init(text,ItemsCount);
		mItems.Add(option);
	}

	public int ItemsCount{get{return mItems.Count;}}
	public UIOptionMenuItem GetItem(int index){return (index<ItemsCount) ? mItems[index] : null;}

	public void Repostion()
	{
		mGrid.repositionNow = true;
		mSprBg.transform.localScale = new Vector3(mItemWidth + 16, mItemHeight * ItemsCount + 16,1);
	} 

	public void Clear()
	{
		for (int i=0;i<ItemsCount;i++)
		{
			mItems[i].gameObject.transform.parent = null;
			GameObject.Destroy(mItems[i].gameObject);
		}
		mItems.Clear();
	}

	void CalculatePos()
	{
		if (uiCamera == null)
			return;
		Vector3 mPos = Input.mousePosition;
		Vector3 mSize = mSprBg.transform.localScale;

		mPos.x = Mathf.Clamp01(mPos.x / Screen.width);
		mPos.y = Mathf.Clamp01(mPos.y / Screen.height);
		
		// Calculate the ratio of the camera's target orthographic size to current screen size
		float activeSize = uiCamera.orthographicSize / gameObject.transform.parent.lossyScale.y;
		float ratio = (Screen.height * 0.5f) / activeSize;
		
		// Calculate the maximum on-screen size of the tooltip window
		Vector2 max = new Vector2(ratio * mSize.x / Screen.width, ratio * mSize.y / Screen.height);

		// Limit the tooltip to always be visible
		mPos.x = Mathf.Min(mPos.x, 1f - max.x);
		mPos.y = Mathf.Max(mPos.y, max.y);
		
		// Update the absolute position and save the local one
		gameObject.transform.position = uiCamera.ViewportToWorldPoint(mPos);
		mPos = gameObject.transform.localPosition;
		mPos.x = Mathf.Round(mPos.x);
		mPos.y = Mathf.Round(mPos.y);
		mPos.z = -100;
		gameObject.transform.localPosition = mPos;
	}


	bool IsMouseIn()
	{
		Ray ray = uiCamera.ScreenPointToRay(Input.mousePosition);
		RaycastHit rayHit;
		//float mindist = 100f;
		return mBgCollider.Raycast(ray,out rayHit,100);
	}

	// Update is called once per frame
	void Update () 
	{
		if (isShow && (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1)) )
			if (!IsMouseIn())
		   	 	Hide();
	}
}
