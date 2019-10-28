using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UIAdminstratorWnd : UIBaseWnd {

	[SerializeField]
	Transform Centent = null;

	[SerializeField]
	GameObject UIAdminstratorItemPrefab = null;

	[HideInInspector]
	public UIAdminstratorItem mUIAdminstratorItem = null;

	public UICheckbox mBlackListCheckbox;
	public UICheckbox mPersonnelCheckbox;

	public GameObject mPersonnelBg;
	public GameObject mBlackListBg;

	[SerializeField]
	UILabel mLbPage;
	[SerializeField]
	UIGrid mGird;
	//[SerializeField] GameObject ItemAdminstratorPrefab;
	[SerializeField]
	GameObject mForbidsBuildBtn;
	[SerializeField]
	GameObject mForbidsNewPalyerBtn;

	[SerializeField]
	GameObject mForbidsBuildBg;
	[SerializeField]
	GameObject mForbidsNewPalyerBg;

	[SerializeField]
	GameObject mBanBtn;
	[SerializeField]
	GameObject mBanAllBtn;

	[SerializeField]
	GameObject mReMoveBtn;
	[SerializeField]
	GameObject mReMoveAllBtn;

	public UserAdmin mUserAdmin;


	private List<UIAdminstratorItem> mItemList = new List<UIAdminstratorItem>();

	//	private List<UserAdmin> mPersonelInfoList = new List<UserAdmin>(); 

	//private List<UserAdmin> mBalckInfoList = new List<UserAdmin>(); 

	//private List<UserAdmin> mBPrivilegesInfoList = new List<UserAdmin>();

	private List<UserAdmin> mpitchAdminList = new List<UserAdmin>();

	//private List<UserAdmin> RefashList=null;

	//private List<UserAdmin> UserAdminList = null;

	//	private ArrayList ArrayPersonnelAdmin=new ArrayList();
	//	private ArrayList ArrayBlackAdmin=new ArrayList();

	private int mCurrentPage = 1;
	private int mEndPage = 0;

	private bool Ispersonnel = true;

	//ulong mMask = 2;


	GameObject AddUIPrefab(GameObject prefab, Transform parentTs)
	{
		GameObject o = GameObject.Instantiate(prefab) as GameObject;
		o.transform.parent = parentTs;
		o.layer = parentTs.gameObject.layer;
		o.transform.localPosition = Vector3.zero;
		o.transform.localScale = Vector3.one;
		return o;
	}


	void Awake()
	{
		//mInstance = this;
		Instant();
	}

	void Instant()
	{
		GameObject gameUI = AddUIPrefab(UIAdminstratorItemPrefab, Centent);
		mUIAdminstratorItem = gameUI.GetComponent<UIAdminstratorItem>();
		gameUI.SetActive(false);
	}
	// Use this for initialization
	void Start()
	{

		//Test();
		//RefashList=mPersonelInfoList;
		//Test();
		UIAdminstratorctr.UpdatamPersonel();

		ServerAdministrator.PrivilegesChangedEvent = OnPrivilegesChanged;
		ServerAdministrator.LockAreaChangedEvent = OnLockAreaChanged;


		UpdataPage();
		Reflsh(UIAdminstratorctr.mUIPersonelInfoList);

		mPersonnelBg.SetActive(true);
		//PlayerNetwork.MainPlayer.AddBlackList();
		//PlayerNetwork.MainPlayer
		//UserAdminList=ServerAdministrator.UserAdminList; 

	}

	void Updata()
	{
		//	UIAdminstratorctr.UpdatamPersonel();
		//	UpdataPage();
		//	Reflsh(UIAdminstratorctr.mUIPersonelInfoList);
	}

	void OnDestroy()
	{
		ServerAdministrator.PrivilegesChangedEvent = null;
		ServerAdministrator.LockAreaChangedEvent = null;
	}

	void OnPrivilegesChanged(UserAdmin ua)
	{
		UIAdminstratorctr.ChangeAssistant(ua);
		ShowPageLsit(UIAdminstratorctr.mUIPersonelInfoList,mCurrentPage,UIAdminstratorctr.UIArrayPersonnelAdmin);

		Reflsh(UIAdminstratorctr.mUIPersonelInfoList);
	}

	void OnLockAreaChanged(int ifon,bool Lock)
	{

	}

	void Test()
	{
		for(int i=0;i<35;i++)
		{
			UserAdmin info=new UserAdmin(i,"palyer"+i,0);
			if( i==0 ) info.AddPrivileges(AdminMask.AdminRole);
			ServerAdministrator.UserAdminList.Add(info);
		}
	}

/*	void Test()
	{

		for(int i=0;i<35;i++)
		{
			UserAdmin info=new UserAdmin(i,"palyer"+i,0);
			if( i==0 ) info.AddPrivileges(AdminMask.AdminRole);
			ArrayPersonnelAdmin.Add(info);
		}

		for(int i=(mCurrentPage-1)*8;i<ArrayPersonnelAdmin.Count;i++)
		{
			if(i<mCurrentPage*8)
				mPersonelInfoList.Add((UserAdmin)ArrayPersonnelAdmin[i]);
			else
				break;
		}

		if(ArrayPersonnelAdmin.Count%8==0)
		   mEndPage=ArrayPersonnelAdmin.Count/8;
		else
			mEndPage=ArrayPersonnelAdmin.Count/8+1;

		mLbPage.text=mCurrentPage.ToString()+"/"+mEndPage.ToString();
	}*/

	void UpdataPage()
	{

		for(int i=(mCurrentPage-1)*8;i<UIAdminstratorctr.UIArrayPersonnelAdmin.Count;i++)
		{
			if(i<mCurrentPage*8)
				UIAdminstratorctr.mUIPersonelInfoList.Add((UserAdmin)UIAdminstratorctr.UIArrayPersonnelAdmin[i]);
			else
				break;
		}

		if (UIAdminstratorctr.UIArrayPersonnelAdmin.Count%8==0)
			mEndPage=UIAdminstratorctr.UIArrayPersonnelAdmin.Count/8;
		else
			mEndPage=UIAdminstratorctr.UIArrayPersonnelAdmin.Count/8+1;
		
		mLbPage.text=mCurrentPage.ToString()+"/"+mEndPage.ToString();
	}

	//将一个成员从一个表移动到另一个
	void Move<T> (List<T> Fromlist1,T Info,List<T> Tolist2)
	{
		if(Info!=null)
		{
			Fromlist1.Remove(Info);
			Tolist2.Add(Info);
		}
	}

	//根据索引找到对应ItemInfo成员
	/*T ListItemInfoFind<T> (List<T> Fromlist,T Member)
	{
		foreach (T info in Fromlist)
		{
			if(info==Member)
				return info;
		}
		return null;
	}*/

	//列表更新
	void Reflsh(List<UserAdmin> refashList)
	{
		Clear();

		refashList.Sort((x, y) =>
		{
			if (PlayerNetwork.IsOnline(x.Id) && PlayerNetwork.IsOnline(y.Id))
				return 0;
			else if (PlayerNetwork.IsOnline(x.Id))
				return -1;  
			else
				return 1;
		});

		foreach (UserAdmin Admininfo in refashList)
		{
			AddAdminstItem(Admininfo);
		}
		mGird.repositionNow = true;
	}
	
	void ShowPageLsit(List<UserAdmin> PageList,int Page,ArrayList ArrayAdmin)
	{
		
		if(ArrayAdmin.Count%8==0)
			mEndPage=ArrayAdmin.Count/8;
		else
			mEndPage=ArrayAdmin.Count/8+1;
		
		PageList.Clear();
		for(int i=(Page-1)*8;i<ArrayAdmin.Count;i++)
		{
			if(i<Page*8)
				PageList.Add((UserAdmin)ArrayAdmin[i]);
			else
				break;
		}
		mLbPage.text=Page.ToString()+"/"+mEndPage.ToString();
	}

	
	//添加Guid下Item成员
	void AddAdminstItem(UserAdmin userAdmin)
	{
		GameObject obj = GameObject.Instantiate(UIAdminstratorItemPrefab) as GameObject;
		obj.transform.parent = mGird.transform;
		obj.transform.localScale = Vector3.one;
		obj.transform.localPosition = Vector3.zero;
		obj.SetActive(true);

		UIAdminstratorItem item = obj.GetComponent<UIAdminstratorItem>();

		if (PlayerNetwork.IsOnline(userAdmin.Id))
			item.NameText = "[33FF00]" + userAdmin.RoleName + "[-]";
		else
			item.NameText = "[999999]" + userAdmin.RoleName + "[-]";

		item.mUserAdmin=userAdmin;
		item.e_ItemAdminOnClick+=ItemAdminOnClick;
		item.e_ItemAdminOnpitch+=ItemAdminOnpitch;

		item.isForbiddenRelsh=true;

		UIAdminstratorctr.ShowAssistant(item);

		if(ServerAdministrator.IsAdmin(PlayerNetwork.mainPlayerId))//需要判断自己是否是管理员
		{
			if(Ispersonnel)
			{
				item.mSetBtn.SetActive(true);
				item.mForbidenBtn.SetActive(true);

				mForbidsBuildBtn.SetActive(true);
				mForbidsNewPalyerBtn.SetActive(true);

				mBanBtn.SetActive(true);
				mBanAllBtn.SetActive(true);


			}
			else
			{

				mBanBtn.SetActive(false);
				mBanAllBtn.SetActive(false);

				mReMoveBtn.SetActive(true);
				mReMoveAllBtn.SetActive(true);
			}
		}
		else 
		{
			mForbidsBuildBtn.SetActive(false);
			mForbidsNewPalyerBtn.SetActive(false);
		}

		/*if(item.mUserAdmin.HasPrivileges(AdminMask.AdminRole))
		{
			item.mSetBtn.SetActive(true);
			item.mForbidenBtn.SetActive(true);
		}*/
		mItemList.Add(item);
	}

	//清空guid
	void Clear()
	{
		foreach (UIAdminstratorItem item in  mItemList)
		{
			if (item != null)
			{
				GameObject.Destroy(item.gameObject);
				item.gameObject.transform.parent = null;
			}
		}
		mItemList.Clear();
	}

	//OnPersonnelBtn按键响应
	void OnPersonnelBtn()
	{
		mPersonnelBg.SetActive(true);
		mBlackListBg.SetActive(false);

		mForbidsBuildBtn.SetActive(true);
		mForbidsNewPalyerBtn.SetActive(true);

		Ispersonnel=true;
		mCurrentPage=1;

		ShowPageLsit(UIAdminstratorctr.mUIPersonelInfoList,mCurrentPage,UIAdminstratorctr.UIArrayPersonnelAdmin);

		if(UIAdminstratorctr.mUIPersonelInfoList.Count==0)
			MustShow();

		Reflsh(UIAdminstratorctr.mUIPersonelInfoList);
	}

	//OnBlackListBtn按键响应
	void OnBlackListBtn()
	{
		mPersonnelBg.SetActive(false);
		mBlackListBg.SetActive(true);

		mForbidsBuildBtn.SetActive(false);
		mForbidsNewPalyerBtn.SetActive(false);
		Ispersonnel=false;
		mCurrentPage=1;

		ShowPageLsit(UIAdminstratorctr.mUIBalckInfoList,mCurrentPage,UIAdminstratorctr.UIArrayBlackAdmin);
		Reflsh(UIAdminstratorctr.mUIBalckInfoList);
	}

	//checkBox启动响应
	void OnActivate()
	{
		mPersonnelBg.SetActive(true);
	}

	bool ForbidsBuild=false;

	void ForbidsBuildBtn()
	{
		ForbidsBuild=!ForbidsBuild;

		mForbidsBuildBg.SetActive(ForbidsBuild);

		UIAdminstratorctr.FobidenAll(ForbidsBuild);

/*		//ServerAdministrator.RequestBuildLock();
		if(ForbidsBuild)
		{

			foreach (UserAdmin userItem in UIAdminstratorctr.UIArrayPersonnelAdmin)
			{
				UIAdminstratorctr.UIBuildLock(userItem);
				//userItem.AddPrivileges(AdminMask.BuildLock);
			}
		}
		else
		{
			ServerAdministrator.ClearBuildLock();
		}*/

		Reflsh(UIAdminstratorctr.mUIPersonelInfoList);
	}

	bool ForbidsNewPalyer=false;
	void ForbidsNewPalyerBtn()
	{
		ForbidsNewPalyer=!ForbidsNewPalyer;

		mForbidsNewPalyerBg.SetActive(ForbidsNewPalyer);
		ServerAdministrator.SetJoinGame(ForbidsNewPalyer);
		if(ForbidsNewPalyer)
		{
			Debug.Log ("ForbidsNewPalyer!!");

		}
		else
		{
			Debug.Log ("AllowNewPalyer!!");
		}
		Reflsh(UIAdminstratorctr.mUIPersonelInfoList);
	}

	void SetBlackList(UserAdmin SetUser)
	{
		foreach(UIAdminstratorItem item in mItemList)
		{
			if(item.mUserAdmin.Id==SetUser.Id)
			{
				item.MoveToBlackList(SetUser.Id);
				return;
			}
		}

		return;
	}

	void OnBanBtn()
	{
		foreach(UserAdmin Userinfo in mpitchAdminList)
		{
			//Move<UserAdmin>(UIAdminstratorctr.mUIPersonelInfoList,Userinfo,UIAdminstratorctr.mUIBalckInfoList);
			UIAdminstratorctr.UIAddBlacklist(Userinfo);

		}
		mpitchAdminList.Clear();
		//

	/*	if(UIAdminstratorctr.UIArrayPersonnelAdmin.Count%8==0)
			mEndPage=UIAdminstratorctr.UIArrayPersonnelAdmin.Count/8;
		else
			mEndPage=UIAdminstratorctr.UIArrayPersonnelAdmin.Count/8+1;

		//mPersonelInfoList.Clear();
		UIAdminstratorctr.mUIPersonelInfoList.Clear();
		mCurrentPage=1;
		for(int i=(mCurrentPage-1)*8;i<UIAdminstratorctr.UIArrayPersonnelAdmin.Count;i++)
		{
			if(i<mCurrentPage*8)
				UIAdminstratorctr.mUIPersonelInfoList.Add((UserAdmin)UIAdminstratorctr.UIArrayPersonnelAdmin[i]);
			else
				break;
		}
		
		mLbPage.text=mCurrentPage.ToString()+"/"+mEndPage.ToString();

		Reflsh(UIAdminstratorctr.mUIPersonelInfoList);*/
	}

	void MustShow()
	{
		UIAdminstratorctr.UIArrayPersonnelAdmin.Clear();
		UIAdminstratorctr.mUIPersonelInfoList.Clear();
		
		if(UIAdminstratorctr._mUserAdmin!=null) 
			UIAdminstratorctr.mUIPersonelInfoList.Add(UIAdminstratorctr._mUserAdmin);
		
		if((UIAdminstratorctr._mUserAdmin!=null)
		   &&(UIAdminstratorctr._mSelfAdmin!=null) 
		   &&(UIAdminstratorctr._mUserAdmin!=UIAdminstratorctr._mSelfAdmin)
		   )
			UIAdminstratorctr.mUIPersonelInfoList.Add(UIAdminstratorctr._mSelfAdmin);
	}

	void OnBanAllBtn()
	{

		ArrayList ArrayPersonnelAdmin=UIAdminstratorctr.UIArrayPersonnelAdmin;

		foreach(UserAdmin Userinfo in ArrayPersonnelAdmin)
		{
			UIAdminstratorctr.UIAddBlacklist(Userinfo);
			//UIAdminstratorctr.UIAddAllBlacklist(Userinfo);
		}

		MustShow();
		//ArrayPersonnelAdmin.Clear();
		//mPersonelInfoList.Clear();
		//mLbPage.text="0/0";
		Reflsh(UIAdminstratorctr.mUIPersonelInfoList);
		ArrayPersonnelAdmin.Clear();
	}

	void OnRemoveBtn()
	{
		foreach(UserAdmin Userinfo in mpitchAdminList)
		{
			//ServerAdministrator.DeleteBlacklist(Userinfo.Id);
			//UIAdminstratorctr.UIArrayBlackAdmin.Remove(Userinfo);
			ServerAdministrator.RequestDeleteBlackList(Userinfo.Id);
		}

		mpitchAdminList.Clear();

		ShowPageLsit (UIAdminstratorctr.mUIBalckInfoList,mCurrentPage,UIAdminstratorctr.UIArrayBlackAdmin);
		Reflsh(UIAdminstratorctr.mUIBalckInfoList);
	}

	void OnRemoveAllBtn()
	{
		ServerAdministrator.RequestClearBlackList();
		UIAdminstratorctr.mUIBalckInfoList.Clear();
		UIAdminstratorctr.UIArrayBlackAdmin.Clear();
		mLbPage.text="0/0";
		Reflsh(UIAdminstratorctr.mUIBalckInfoList);
	}

	void OnRightBtn()
	{

		if(Ispersonnel)
		{
			if(mCurrentPage<mEndPage)
			{
				mCurrentPage++;

				ShowPageLsit(UIAdminstratorctr.mUIPersonelInfoList,mCurrentPage,UIAdminstratorctr.UIArrayPersonnelAdmin);
				Reflsh(UIAdminstratorctr.mUIPersonelInfoList);
			}
		}
		else
		{
			if(mCurrentPage<mEndPage)
			{
			mCurrentPage++;
				ShowPageLsit(UIAdminstratorctr.mUIBalckInfoList,mCurrentPage,UIAdminstratorctr.UIArrayBlackAdmin);
				Reflsh(UIAdminstratorctr.mUIBalckInfoList);
			}
		}
	}

	void OnLeftBtn()
	{
		if(Ispersonnel)
		{
			if(mCurrentPage>1)
			{
				mCurrentPage--;
				ShowPageLsit(UIAdminstratorctr.mUIPersonelInfoList,mCurrentPage,UIAdminstratorctr.UIArrayPersonnelAdmin);
				Reflsh(UIAdminstratorctr.mUIPersonelInfoList);
			}
		}
		else 
		{
			if(mCurrentPage>1)
			{
				mCurrentPage--;
				ShowPageLsit(UIAdminstratorctr.mUIBalckInfoList,mCurrentPage,UIAdminstratorctr.UIArrayBlackAdmin);
				Reflsh(UIAdminstratorctr.mUIBalckInfoList);
			}
		}

	}

	void OnLeftEndBtn()
	{

		mCurrentPage=1;
		if(Ispersonnel)
		{
			ShowPageLsit(UIAdminstratorctr.mUIPersonelInfoList,mCurrentPage,UIAdminstratorctr.UIArrayPersonnelAdmin);
			Reflsh(UIAdminstratorctr.mUIPersonelInfoList);
		}
		else
		{
			ShowPageLsit(UIAdminstratorctr.mUIBalckInfoList,mCurrentPage,UIAdminstratorctr.UIArrayBlackAdmin);
			Reflsh(UIAdminstratorctr.mUIBalckInfoList);
		}

		//Reflsh(mPersonelInfoList);
	}

	void OnRightEndBtn()
	{
        //lz-2017.01.03 crash bug 错误 #8065
        if (mEndPage <= 0) mEndPage = 1;

        mCurrentPage =mEndPage;

		if(Ispersonnel)
		{
			UIAdminstratorctr.mUIPersonelInfoList.Clear();
			for(int i=(mEndPage-1)*8;i<UIAdminstratorctr.UIArrayPersonnelAdmin.Count;i++)
			{
				UIAdminstratorctr.mUIPersonelInfoList.Add((UserAdmin)UIAdminstratorctr.UIArrayPersonnelAdmin[i]);
				
			}
			
			mLbPage.text=mCurrentPage.ToString()+"/"+mEndPage.ToString();
			Reflsh(UIAdminstratorctr.mUIPersonelInfoList);
		}
		else
		{
			UIAdminstratorctr.mUIBalckInfoList.Clear();
			for(int i=(mEndPage-1)*8;i<UIAdminstratorctr.UIArrayBlackAdmin.Count;i++)
			{
				UIAdminstratorctr.mUIBalckInfoList.Add((UserAdmin)UIAdminstratorctr.UIArrayBlackAdmin[i]);
				
			}
			
			mLbPage.text=mCurrentPage.ToString()+"/"+mEndPage.ToString();
			Reflsh(UIAdminstratorctr.mUIBalckInfoList);
		}

	}

	void ItemAdminOnClick(object sender,UserAdmin userAdmin)
	{
		UIAdminstratorItem item = sender as UIAdminstratorItem;
		if ((item != null)&&(userAdmin!=null))
		{

			/*    if(userAdmin.HasPrivileges(AdminMask.BuildLock))//ServerAdministrator.IsBuildLock(userAdmin.Id)
				{
				userAdmin.RemovePrivileges(AdminMask.BuildLock);
				item.mLbForbidden.text="Forbidden";
				}
			    else
				{
				userAdmin.AddPrivileges(AdminMask.BuildLock);
				item.mLbForbidden.text="UnForbidden";
				}*/	

			Reflsh(UIAdminstratorctr.mUIPersonelInfoList);
		}
	}


	
	void ItemAdminOnpitch(object sender,UserAdmin userAdmin,bool Ispitch)
	{
		UIAdminstratorItem item = sender as UIAdminstratorItem;
		if (item != null)//&&(PlayerNetwork.MainPlayerID!=userAdmin.Id)&&(!ServerAdministrator.IsAdmin(PlayerNetwork.MainPlayerID)))
		{
			if(Ispitch)
			{
				mpitchAdminList.Add(userAdmin);
			}
			else
			{
				mpitchAdminList.Remove(userAdmin);

			}
		}
	}
	
}
