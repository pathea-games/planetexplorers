using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UIListItemCtrl : MonoBehaviour 
{
	public delegate void OnCheckItem(int index);
	public event OnCheckItem ListItemChecked = null;

	public event OnCheckItem listItemDoubleClick = null;

	public GameObject mLabelPrefab;
	public GameObject mContent;
	public BoxCollider mCkboxCollider;
	public UISprite mCkSelectedBg;
	public UISprite mItemIco;

	public int mIndex;
	public List<string> mTextList = new List<string>(); 
	public List<GameObject> mLabelList = new  List<GameObject>();

	private int[] mItemWith;
	bool IsSelected = false;

	// Use this for initialization
	void Start () 
	{
		
	}
	
	// Update is called once per frame
	void Update () 
	{

	}


	public void SetIconActive(bool isActive)
	{
		if(mItemIco == null)
			return;
		mItemIco.enabled = isActive;
	}

	public void SetColor(Color _color)
	{
		for(int i=0;i<mLabelList.Count;i++)
		{
			// 特殊处理 更新单列 颜色
			if(i == 6 && this.GetComponent<UIUpdatePIngTextColor>() != null)
				continue;

			UILabel lb = mLabelList[i].GetComponent<UILabel>();
			lb.color = _color;
		}
	}
	public void DeleteItem()
	{

		this.gameObject.transform.parent = null;
		GameObject.Destroy(this.gameObject );

		mLabelList.Clear();
		mTextList.Clear();
	}

	public void SetActive(bool isActive)
	{
		mCkboxCollider.enabled = isActive;
	}

	public void SetSelected(bool _isSelected)
	{
		IsSelected = _isSelected;
		if(mCkSelectedBg.enabled != _isSelected)
			mCkSelectedBg.enabled = _isSelected;
	}

	public void ClearItemText()
	{
		mTextList.Clear();

		for(int i=0; i<mLabelList.Count ;i++)
		{
			UILabel lb = mLabelList[i].GetComponent<UILabel>();
			lb.text = "";
		}
	}

	public void SetItemText(List<string> strText)
	{

		if(mItemWith.Length < strText.Count)
			return;

		mTextList.Clear();

		for(int i=0;i<strText.Count;i++)
		{
			UILabel lb = mLabelList[i].GetComponent<UILabel>();
			lb.text = strText[i];
			lb.lineWidth = mItemWith[i] - 12;
			mTextList.Add(strText[i]);
		}
	}

	public void InitItem(int[] itemWidth)
	{
		int pos_x = 0;
		for(int i=0;i<itemWidth.Length;i++)
		{
			GameObject o = CreateLabel();

			o.transform.localPosition = new Vector3(pos_x,0,-2);
			o.transform.localScale = new Vector3(20,20,1);
			pos_x += itemWidth[i];

			mLabelList.Add(o);
		}
		mItemWith = itemWidth;
	}

	private GameObject CreateLabel()
	{
		GameObject o = GameObject.Instantiate(mLabelPrefab) as GameObject;
		o.transform.parent = mContent.transform;
		return o;
	}


	public void OnChecked()
	{
		if(Input.GetMouseButtonUp(0) && IsSelected == false )
		{
			if(ListItemChecked != null )
				ListItemChecked(mIndex);
		}
	}


	public void ItemOnDoubleClick()
	{
		if(listItemDoubleClick != null && Input.GetMouseButtonUp(0))
			listItemDoubleClick(mIndex);	
	}

}
