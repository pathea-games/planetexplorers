using UnityEngine;
using System.Collections;

public class UIListHeaderItem : MonoBehaviour 
{
	public delegate void e_OnClickSort(int index,int sortState);
	public event e_OnClickSort eSortOnClick = null;

	public GameObject mSplite;
	public UILabel mText;
	public int mIndex;
	public BoxCollider mBoxCollider;
	public int mBoxCliderHeight;
	public int mSortState = -1;

	public GameObject mSort;
	public GameObject mSortUp;
	public GameObject mSortDn;
	public GameObject mSortDefault;
	public bool mCanSort;

	// Use this for initialization
	void Start () 
	{
	
	}
	
	// Update is called once per frame
	void Update () 
	{
	
	}

	public void Init(string text,float pos_x,int width,int index)
	{
		mText.text = text;
		this.gameObject.transform.localPosition = new Vector3(pos_x,0,0);
		mSplite.transform.localPosition = new Vector3(width-2,0,0);
		mIndex = index;

		if (mBoxCollider != null)
		{
			mBoxCollider.size =  new Vector3(width-2,mBoxCliderHeight,-2);
			mBoxCollider.center = new Vector3(width/2 -1,0,0);
			mBoxCollider.enabled =false;
		}

		if (mSort != null)
		{
			mSort.transform.localPosition = new Vector3(width-10,0,0);
		}
	}

	// can sort of list
	public void InitSort(bool CanSort)
	{
		mSort.SetActive(CanSort);
		mBoxCollider.enabled =CanSort;
		mCanSort = CanSort;
	}


	public void SetSortSatate(int sortState)
	{
		if (!mCanSort)
		{
			mSort.SetActive(false);
			return;
		}

		if (sortState == 0)
		{
			mSortUp.SetActive(false);
         	mSortDn.SetActive(false);
        	mSortDefault.SetActive(true);
		}
		else if (sortState == 1)
		{
			mSortUp.SetActive(true);
			mSortDn.SetActive(false);
			mSortDefault.SetActive(false);
		}
		else if (sortState == 2)
		{
			mSortUp.SetActive(false);
			mSortDn.SetActive(true);
			mSortDefault.SetActive(false);
		}
		else
		{
			mSortState = -1;
			return;
		}
		mSortState = sortState;
	}


	void OnClickSort()
	{
		if (!mCanSort)
		{
			mSort.SetActive(false);
			return;
		}

		if (eSortOnClick != null)
		{
			eSortOnClick(mIndex,mSortState);
		}
	}
}
