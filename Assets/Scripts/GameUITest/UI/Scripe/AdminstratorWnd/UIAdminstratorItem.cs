using UnityEngine;
using System.Collections;

public class UIAdminstratorItem : MonoBehaviour
{
	[SerializeField] UILabel mLbName;
	[SerializeField] UILabel mLbSet;
	[SerializeField] public UILabel mLbForbidden;

	[SerializeField] GameObject mAdminstratorBg;
	[SerializeField] GameObject mAdminstratorPitchBg;
	[SerializeField] GameObject mISPrivilegesBg;
	[SerializeField] GameObject mNotPrivilegesBg;

	//[SerializeField] public GameObject mForbiddenBtn;
	//[SerializeField] public GameObject mForbiddenNewPalyerBtn;

	[SerializeField] public  GameObject mSetBtn;
	[SerializeField] public GameObject mForbidenBtn;

	public delegate void ItemAdminOnClick(object sender,UserAdmin mUserAdmin);
	public event ItemAdminOnClick e_ItemAdminOnClick = null;

	public delegate void ItemAdminOnpitch(object sender,UserAdmin mUserAdmin,bool Ispitch);
	public event ItemAdminOnpitch e_ItemAdminOnpitch = null;

	public string NameText { get {return mLbName.text;} set{mLbName.text = value;} }

	private UserAdmin _mUserAdmin;

	public UserAdmin mUserAdmin{get {return _mUserAdmin;} set{_mUserAdmin = value;}}



	
	// Use this for initialization
	void Start () 
	{

		/*if(ServerAdministrator.IsAdmin(_mUserAdmin.Id))
		{
			mISPrivilegesBg.SetActive(true);
			mNotPrivilegesBg.SetActive(false);
		}
		else
		{
			mISPrivilegesBg.SetActive(false);
			mNotPrivilegesBg.SetActive(true);
		}*/
					
		IntShow();		
			/*if(_mUserAdmin.Privileges==0)
			{
				mISPrivilegesBg.SetActive(true);
			}
			else
			{
				mNotPrivilegesBg.SetActive(true);
			}*/
		ServerAdministrator.BuildLockChangedEvent = OnBuildLockChanged;
		ServerAdministrator.PlayerBanChangedEvent = OnPlayerBanChanged;
		
	}

	void OnDestroy()
	{
		ServerAdministrator.BuildLockChangedEvent = null;
		ServerAdministrator.PlayerBanChangedEvent = null;
	}

	void OnBuildLockChanged(bool Build)
	{
		BuildShow(Build);
		UIAdminstratorctr.ChangeAssistant(_mUserAdmin);
	}


	void OnPlayerBanChanged(bool ban)
	{
		
	}
	
	void IntShow()
	{
		if(_mUserAdmin.HasPrivileges(AdminMask.AssistRole))
		{
			mISPrivilegesBg.SetActive(true);
			mNotPrivilegesBg.SetActive(false);
			
			//mSetBtn.SetActive(true);
			//mForbidenBtn.SetActive(true);
			
		}
		else
		{
			mISPrivilegesBg.SetActive(false);
			mNotPrivilegesBg.SetActive(true);
			
			//mSetBtn.SetActive(false);
			//mForbidenBtn.SetActive(false);
		}	
	}
    void Update ()
	{
		//IntShow();
		//ReFlshForBidden();
		//RefalshPrivileges();
	}

	public  void PrivilegesShow(bool IS,bool Not)
	{
		mISPrivilegesBg.SetActive(IS);
		mNotPrivilegesBg.SetActive(Not);			
		if(IS) mLbSet.text="ReSet";
		else   mLbSet.text="Set";
	}

	public void BuildShow(bool Lock)
	{
		if(Lock) 
			mLbForbidden.text="BuildLock";
		else 
			mLbForbidden.text="Build";
	}

	//助手设置
	void OnSetBtn()
	{

		if(mLbSet.text=="Set")
		{
			ServerAdministrator.RequestAddAssistants(_mUserAdmin.Id);
		}
		else
		{
			ServerAdministrator.RequestDeleteAssistants(_mUserAdmin.Id);
		}
	}

	void RefalshPrivileges()
	{

		if(ServerAdministrator.IsAssistant(_mUserAdmin.Id))
		{
			mISPrivilegesBg.SetActive(true);
			mNotPrivilegesBg.SetActive(false);			
			mLbSet.text="ReSet";
		}
		else
		{
			mISPrivilegesBg.SetActive(false);
			mNotPrivilegesBg.SetActive(true);
			
			mLbSet.text="Set";
		}
		/*if(_mUserAdmin.HasPrivileges(AdminMask.AssistRole))
		{
			mISPrivilegesBg.SetActive(true);
			mNotPrivilegesBg.SetActive(false);			
			mLbSet.text="ReSet";
		}
		else 
		{
			mISPrivilegesBg.SetActive(false);
			mNotPrivilegesBg.SetActive(true);
			
			mLbSet.text="Set";
		}*/
	}


	public  void MoveToBlackList(int MovedId)
	{
		if(MovedId==_mUserAdmin.Id)
		{
			mSetBtn.SetActive(false);
			mForbidenBtn.SetActive(false);
		}
	}

	void ReFlshForBidden()
	{
		if(isForbiddenRelsh)
		{

			if(mLbForbidden.text=="Build")//ServerAdministrator.IsBuildLock(userAdmin.Id)
			{
				isForbidden=true;
				ServerAdministrator.RequestBuildLock(_mUserAdmin.Id);
			}
			else
			{
				isForbidden=false;
				ServerAdministrator.RequestBuildUnLock(_mUserAdmin.Id);
			}
			isForbiddenRelsh=false;

		}

	}

	public bool isForbiddenRelsh=false;

	private static bool isForbidden=false;

	void OnForbiddenBtn()
	{
		isForbiddenRelsh=true;

		isForbidden=!isForbidden;

		if(mLbForbidden.text=="Build")
		{
			ServerAdministrator.RequestBuildLock(_mUserAdmin.Id);
		  //_mUserAdmin.AddPrivileges(AdminMask.BuildLock);
		}
		else
		{
			ServerAdministrator.RequestBuildUnLock(_mUserAdmin.Id);
			//_mUserAdmin.RemovePrivileges(AdminMask.BuildLock);
		}

		//ReFlshForBidden();
		if (e_ItemAdminOnClick != null)
		{
			e_ItemAdminOnClick(this,_mUserAdmin);	

				
		}
	}

	private bool Ispitch=false;

	void OnpitchBtn()
	{
		Ispitch=!Ispitch;
		mAdminstratorPitchBg.SetActive(Ispitch);

		if(e_ItemAdminOnpitch!=null)
		{
			e_ItemAdminOnpitch(this,_mUserAdmin,Ispitch);
		}
	}

	void AdminstratorOver()
	{
		mAdminstratorBg.SetActive(true);
	}

	void AdminstratorOut()
	{
		mAdminstratorBg.SetActive(false);
	}
	// Update is called once per frame

	

}
