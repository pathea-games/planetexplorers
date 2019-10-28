using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using Pathea;

//public class PersonnelManageGui_N : GUIWindowBase 
//{
//	public PersonnelItem_N	mPersonnelItemPerfab;
//	List<PersonnelItem_N>	mPersonnelItemList = new List<PersonnelItem_N>();
//	PersonnelItem_N			mSelecedPersonnelItem;
//	public UIGrid			mPersonnelListGrid;
//
//	public BlackListItem_N 	mBlackListItemPerfab;
//	List<BlackListItem_N>	mBlackListItemList = new List<BlackListItem_N>();
//	BlackListItem_N 		mSelectedBlackListItem;
//	public UIGrid			mBlackListGrid;
//	
//	public GameObject		mPersonnelWnd;
//	public GameObject		mBlackListWnd;
//
//	public GameObject		mAssistantMask;
//	public GameObject		mBuildEnableMask;
//	public GameObject		mPersonnelMask;
//
//	public UICheckbox		mForbidBuildingBox;
//	public UICheckbox		mForbidNewPlayer;
//	public UIButton			mBanBtn;
//	public UIButton			mBanAllBtn;
//
//	public UIButton			mRemoveBtn;
//	public UIButton			mRemoveAllBtn;
//
//	public UILabel			mPageContent;
//	int 					mCurrentpageIndex;
//	int						mMaxPageIndex;
//
//	const int 				mPageNum = 9;
//
//	bool					mInitWnd = false;
//
//	public override void AwakeWindow ()
//	{
//		base.AwakeWindow ();
//		InitPersonnelWnd();
//		OnPersonnelBtn(true);
//	}
//
//	public void OnPersonnelInfoChange()
//	{
//		if(mPersonnelWnd.activeSelf)
//		{
//			mMaxPageIndex = Mathf.Clamp((ServerAdministrator.UserAdminList.Count - 1) / mPageNum, 0, 99);
//			mCurrentpageIndex = Mathf.Clamp(mCurrentpageIndex, 0, mMaxPageIndex);
//			ResetPersonnelWnd(mCurrentpageIndex);
//		}
//	}
//
//	void OnPersonnelBtn(bool selected)
//	{
//		mPersonnelWnd.SetActive(selected);
//		if(selected)
//		{
//			mCurrentpageIndex = 0;
//			mMaxPageIndex = Mathf.Clamp((ServerAdministrator.UserAdminList.Count - 1) / mPageNum, 0, 99);
//			ResetPersonnelWnd(mCurrentpageIndex);
//		}
//	}
//
//	void InitPersonnelWnd()
//	{
//		if (mPersonnelItemList.Count <= 0)
//		{
//			for (int i = 0; i < mPageNum; i++)
//			{
//				PersonnelItem_N addItem = Instantiate(mPersonnelItemPerfab) as PersonnelItem_N;
//				addItem.transform.parent = mPersonnelListGrid.transform;
//				addItem.transform.localPosition = Vector3.zero;
//				addItem.transform.localScale = Vector3.one;
//				addItem.mSelectedBg.radioButtonRoot = mPersonnelListGrid.transform;
//				mPersonnelItemList.Add(addItem);
//			}
//		}
//	}
//
//	void ResetPersonnelWnd(int page)
//	{
//		mCurrentpageIndex = page;
//
//		int startIndex = page * mPageNum;
//		int curIndex = ServerAdministrator.UserAdminList.Count - startIndex;
//		curIndex = Mathf.Clamp(curIndex, 0, mPageNum);
//
//		for (int i = 0; i < mPageNum; i++)
//		{
//			if (i < curIndex)
//			{
//				UserAdmin ud = ServerAdministrator.UserAdminList.ElementAt(startIndex + i);
//
//				mPersonnelItemList[i].gameObject.SetActive(true);
//				mPersonnelItemList[i].SetPlayerInfo(ud);
//			}
//			else
//			{
//				mPersonnelItemList[i].gameObject.SetActive(false);
//			}
//		}
//
//		mPersonnelListGrid.Reposition();
//		//If is administrator
//		if (ServerAdministrator.IsAdmin(PeCreature.Instance.mainPlayer.Id))
//		{
//			mAssistantMask.SetActive(false);
//			mBuildEnableMask.SetActive(false);
//			mPersonnelMask.SetActive(false);
//			mForbidBuildingBox.gameObject.SetActive(true);
//			mForbidNewPlayer.gameObject.SetActive(true);
//			mBanBtn.isEnabled = true;
//			mBanAllBtn.isEnabled = true;
//		}
//        else if (ServerAdministrator.IsAssistant(PeCreature.Instance.mainPlayer.Id))
//		{
//			mAssistantMask.SetActive(true);
//			mBuildEnableMask.SetActive(false);
//			mPersonnelMask.SetActive(true);
//			mForbidBuildingBox.gameObject.SetActive(false);
//			mForbidNewPlayer.gameObject.SetActive(false);
//			mBanBtn.isEnabled = true;
//			mBanAllBtn.isEnabled = true;
//		}
//		else
//		{
//			mAssistantMask.SetActive(true);
//			mBuildEnableMask.SetActive(true);
//			mPersonnelMask.SetActive(true);
//			mForbidBuildingBox.gameObject.SetActive(false);
//			mForbidNewPlayer.gameObject.SetActive(false);
//			mBanBtn.isEnabled = false;
//			mBanAllBtn.isEnabled = false;
//		}
//
//		mForbidBuildingBox.isChecked = !ServerAdministrator.AllowModify;
//		mForbidNewPlayer.isChecked = !ServerAdministrator.AllowJoin;
//		mPageContent.text = (mCurrentpageIndex + 1).ToString() + " / " + (mMaxPageIndex + 1);
//	}
//
//	public void OnBlackListInfoChange()
//	{
//		if(mBlackListWnd.activeSelf)
//		{
//			mMaxPageIndex = Mathf.Clamp((ServerAdministrator.Instance.BlackRoles.Count() - 1) / mPageNum, 0, 99);
//			mCurrentpageIndex = Mathf.Clamp(mCurrentpageIndex, 0, mMaxPageIndex);
//			ResetBlackListWnd(mCurrentpageIndex);
//		}
//	}
//
//	void OnBlackListBtn(bool selected)
//	{
//		mBlackListWnd.SetActive(selected);
//		if(selected)
//		{
//			mCurrentpageIndex = 0;
//			mMaxPageIndex = Mathf.Clamp((ServerAdministrator.Instance.BlackRoles.Count() - 1) / mPageNum, 0, 99);
//			ResetBlackListWnd(mCurrentpageIndex);
//		}
//	}
//
//	void ResetBlackListWnd(int page)
//	{
//		mInitWnd = false;
//		mCurrentpageIndex = page;
//		for(int i = 0; i < mBlackListItemList.Count; i++)
//		{
//			mBlackListItemList[i].transform.parent = null;
//			Destroy(mBlackListItemList[i].gameObject);
//		}
//		mBlackListItemList.Clear();
//
//		foreach (UserAdmin iter in ServerAdministrator.Instance.BlackRoles)
//		{
//			BlackListItem_N addItem = Instantiate(mBlackListItemPerfab) as BlackListItem_N;
//			addItem.transform.parent = mBlackListGrid.transform;
//			addItem.transform.localPosition = Vector3.zero;
//			addItem.transform.localScale = Vector3.one;
//			addItem.GetComponent<UICheckbox>().radioButtonRoot = mBlackListGrid.transform;
//			addItem.SetInfo(iter);
//			mBlackListItemList.Add(addItem);
//		}
//		mBlackListGrid.Reposition();
//
//		//If is administrator
//        if (ServerAdministrator.IsAssistant(PeCreature.Instance.mainPlayer.Id))
//		{
//			mRemoveBtn.isEnabled = true;
//			mRemoveAllBtn.isEnabled = true;
//		}
//		else
//		{
//			mRemoveBtn.isEnabled = false;
//			mRemoveAllBtn.isEnabled = false;
//		}
//
//		mPageContent.text = (mCurrentpageIndex + 1).ToString() + " / " + (mMaxPageIndex + 1);
//		mInitWnd = true;
//	}
//
//	void OnPageUp()
//	{
//		if (mCurrentpageIndex < mMaxPageIndex)
//		{
//			if(mPersonnelWnd.activeSelf)
//				ResetPersonnelWnd(mCurrentpageIndex + 1);
//		}
//	}
//
//	void OnPageDown()
//	{
//		if(mCurrentpageIndex > 0)
//		{
//			if(mPersonnelWnd.activeSelf)
//				ResetBlackListWnd(mCurrentpageIndex - 1);
//		}
//	}
//
//	void OnForbidsBuildSelected(bool selected)
//	{
//		if (ServerAdministrator.AllowModify != selected)
//			return;
//
//		if (null != PlayerNetwork.MainPlayer)
//			PlayerNetwork.MainPlayer.SetBuildChunk(!selected);
//	}
//
//	void OnForbidsNewPlayerSelected(bool selected)
//	{
//		if (ServerAdministrator.AllowJoin != selected)
//			return;
//
//		if (null != PlayerNetwork.MainPlayer)
//			PlayerNetwork.MainPlayer.SetJoinGame(!selected);
//	}
//
//	public void OnPersonnelItemSelected(PersonnelItem_N selectedItem, bool selected)
//	{
//		if(selected)
//			mSelecedPersonnelItem = selectedItem;
//		else
//			mSelecedPersonnelItem = null;
//	}
//
//	void OnBanBtn()
//	{
//		if(null != mSelecedPersonnelItem)
//			mSelecedPersonnelItem.OnBan();
//	}
//
//	void OnBanAllBtn()
//	{
//		for (int i = 0; i < ServerAdministrator.UserAdminList.Count; i++)
//			mPersonnelItemList[i].OnBan();
//	}
//	
//	public void OnBlackListItemSelected(BlackListItem_N selectedItem, bool selected)
//	{
//		if(selected)
//			mSelectedBlackListItem = selectedItem;
//		else if(mSelectedBlackListItem == selectedItem)
//			mSelectedBlackListItem = null;
//	}
//
//	void OnRemoveBtn()
//	{
//		if(null != mSelectedBlackListItem)
//			mSelectedBlackListItem.Remove();
//	}
//
//	void OnRemoveAllBtn()
//	{
//		for(int i = 0; i < mBlackListItemList.Count; i++)
//			mBlackListItemList[i].Remove();
//	}
//}
