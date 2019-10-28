using UnityEngine;
using System.Collections;

public class PlayerBuildGirdItem : MonoBehaviour 
{
	public delegate void ClickItemEvent(int index,Type _type);
	public event ClickItemEvent e_ClickItem = null;

	[SerializeField] UISprite mIcon;
	[SerializeField] UITexture mTexture;
	[SerializeField] UITexture mTexSelected;
	[SerializeField] UISprite mbg;	

	[HideInInspector] public int mIndex = 0;
	[HideInInspector] public Type mType = Type.Type_Null;

	public bool canSelected =false;

	bool mSelected = false;
	public bool isSelected
	{
		set
		{
			mSelected = value;
			if (canSelected)
				mTexSelected.enabled = mSelected;
		}
		get
		{
			return mSelected;
		}
	}

	public enum Type
	{
		Type_Null = 0,
		Type_Head = 1,
		Type_Face = 2,
		Type_Hair = 3,
		Type_Save = 4
	}

	// Use this for initialization
	void Start () 
	{
		Transform tran = transform.FindChild("Background");
		if(null != tran)
		{
			UIWidget uiw = tran.GetComponent<UIWidget>();
			if(null != uiw)
				uiw.MakePixelPerfect();
		}
	}

	public void InitItem(int index , Type _type)
	{
		mIndex = index;
		mType = _type;
	}

	public void SetItemInfo(string iconName)
	{
		mIcon.spriteName = iconName;
		mIcon.enabled = true;
		mTexture.enabled = false;
	}

	public void SetItemInfo(Texture2D texture)
	{
		mTexture.mainTexture = texture;
		mTexture.enabled = true;
		mIcon.enabled = false;
	}


	void OnClick ()
	{
		if(Input.GetMouseButtonUp(0))
		{
			if (e_ClickItem != null)
				e_ClickItem(mIndex,mType);
		}
	}
}
