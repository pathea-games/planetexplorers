using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RailwayPointGui_N : UIBaseWnd 
{
	public UILabel			mNameLable;
	public UIInput 			mNameInput;
	public UIPopupList 		mPreMenu;
	public UIPopupList 		mNextMenu;

	public N_ImageButton	mRecycleBtn;
	public N_ImageButton	mCloseBtn;
	
	public event OnGuiBtnClicked RunEvent;
	public event OnGuiBtnClicked RecycleEvent;

	public event OnMenuSelectionChange e_PreMenuChange;
	public event OnMenuSelectionChange e_NextPointChange;

	//public void 
	public string PointName
	{
		get{ return mNameInput.text; }
		set{
				mNameLable.text = mNameInput.text = value; 
				if (mPreMenu.isOpen) 
					GameObject.Destroy( mPreMenu.ChildPopupMenu ); 
				if (mNextMenu.isOpen)
					GameObject.Destroy( mNextMenu.ChildPopupMenu);
		}
	}
	
	void OnRunBtn()
	{
		if(null != RunEvent)
			RunEvent();
		Hide();
	}
	
	void OnRecycleBtn()
	{
		if(null != RecycleEvent)
			RecycleEvent();
		Hide();
	}

	protected override void OnHide()
	{
	 	base.OnHide ();
	}
	
	public void SetPrePoint(List<string> menuList, int index)
	{
		mPreMenu.items.Clear();
		foreach (string str in menuList)
			mPreMenu.items.Add( str ) ;
		if(index >= 0 && index < menuList.Count)
			mPreMenu.selection = menuList[index]; 
		else
			mPreMenu.selection = "";
	}
	
	public void SetNextPoint(List<string> menuList, int index)
	{
		mNextMenu.items.Clear();
		foreach (string str in menuList)
			mNextMenu.items.Add( str ) ;
		if(index >= 0 && index < menuList.Count)
			mNextMenu.selection = menuList[index];
		else
			mNextMenu.selection = "";
	}
	
	void OnPreMenuChange(string str)
	{
		int index = mPreMenu.items.FindIndex(itr=> itr == str);
		if (e_PreMenuChange != null)
			e_PreMenuChange(index,str);
	}

	
	void OnNextMenuChange(string str)
	{
		int index = mPreMenu.items.FindIndex(itr=> itr == str);
		if (e_NextPointChange != null)
			e_NextPointChange(index,str);
	}


	void Start()
	{
		mPreMenu.onSelectionChange += OnPreMenuChange;
		mNextMenu.onSelectionChange += OnNextMenuChange;
	}
}
