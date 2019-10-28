using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UIAdminstratorctr : MonoBehaviour
{

	public static List<UserAdmin> mUIPersonelInfoList = new List<UserAdmin>(); 
	
	public static List<UserAdmin> mUIBalckInfoList = new List<UserAdmin>(); 

	public static ArrayList UIArrayPersonnelAdmin=new ArrayList();

	public static ArrayList UIArrayBlackAdmin=new ArrayList();

	public static UserAdmin _mUserAdmin=null;
	public static UserAdmin _mSelfAdmin=null;

	// Use this for initialization
	void Start () 
	{
	
	}

	public static void UpdatamPersonel()
	{
		//mPersonelInfoList.Clear();
		UIArrayPersonnelAdmin.Clear();
		foreach(UserAdmin AminInfo in ServerAdministrator.UserAdminList)
		{
			if(!ServerAdministrator.IsBlack(AminInfo.Id))
				UIArrayPersonnelAdmin.Add(AminInfo);
			else
				UIArrayBlackAdmin.Add(AminInfo);
		}

	}

	public static void FobidenAll(bool Lock)
	{
		UserAdmin userTemp;
		for(int i=0;i<UIArrayPersonnelAdmin.Count;i++)
		{
			userTemp=(UserAdmin)UIArrayPersonnelAdmin[i];

			if(Lock)
				ServerAdministrator.RequestBuildLock(userTemp.Id);
			else 
			    ServerAdministrator.RequestBuildUnLock(userTemp.Id);
		}
	}

	public static void ChangeAssistant(UserAdmin player)
	{
		ArrayList ArrayPersonnel=UIArrayPersonnelAdmin;

		for(int i=0;i<ArrayPersonnel.Count;i++)
		{
			if(ArrayPersonnel[i]==player)
			{
				if(player.HasPrivileges(AdminMask.BlackRole))
				{
					UIArrayPersonnelAdmin.Remove(ArrayPersonnel[i]);
					UIArrayBlackAdmin.Add(player);
				}
				else 
				{
					UIArrayPersonnelAdmin[i]=player;
				}
				break;
			}
		}

	}

	public static void  ShowAssistant(UIAdminstratorItem item)
	{
		
		if (null == item)
			return ;

		item.PrivilegesShow(ServerAdministrator.IsAssistant(item.mUserAdmin.Id),
		                    !ServerAdministrator.IsAssistant(item.mUserAdmin.Id));

		item.BuildShow(item.mUserAdmin.HasPrivileges(AdminMask.BuildLock));
		
	}
	
	public static void AddUIAssistant(UserAdmin player)
	{
		if (null == player)
			return;
		
		UserAdmin ua = ServerAdministrator.UserAdminList.Find(iter => iter.Id == player.Id);
		if (null == ua)
		{
			ua = new UserAdmin(player.Id, player.RoleName, 0);
			ServerAdministrator.UserAdminList.Add(ua);
		}
		
		ua.AddPrivileges(AdminMask.AssistRole);

	}

	public static void DeleteUIAssistant(UserAdmin player)
	{
		ServerAdministrator.DeleteAssistant(player.Id);
	}


	public static void UIBuildLock(UserAdmin player)
	{
		if (null == player)
			return;

		if(ServerAdministrator.IsAdmin(player.Id))
		{
			if(_mUserAdmin==null)
				_mUserAdmin=player;
			return;
		}
		
		if(PlayerNetwork.mainPlayerId==player.Id)
		{
			if(null==_mSelfAdmin)
				_mSelfAdmin=player;
			return;
		}

		UserAdmin ua = ServerAdministrator.UserAdminList.Find(iter => iter.Id == player.Id);
		if (null == ua)
		{
			ua = new UserAdmin(player.Id, player.RoleName, 0);
			ServerAdministrator.UserAdminList.Add(ua);
		}
		
		ua.AddPrivileges(AdminMask.BuildLock);

	}
	//

	public static void UIAddAllBlacklist(UserAdmin Player)
	{
		if (null == Player)
			return;

		if(ServerAdministrator.IsAdmin(Player.Id))
		{
			if(_mUserAdmin==null)
			  _mUserAdmin=Player;
			return;
		}

		if(PlayerNetwork.mainPlayerId==Player.Id)
		{
			if(null==_mSelfAdmin)
				_mSelfAdmin=Player;
			return;
		}

		UIArrayBlackAdmin.Add(Player);
		mUIBalckInfoList.Add(Player);
		UIAddBlacklist(Player);
	}

	public static void UIAddBlacklist(UserAdmin Player)
	{
		if (null == Player)
			return;

		if(ServerAdministrator.IsAdmin(Player.Id))
		{
			if(_mUserAdmin==null)
				_mUserAdmin=Player;
			return;
		}
		
		if(PlayerNetwork.mainPlayerId==Player.Id)
		{
			if(null==_mSelfAdmin)
				_mSelfAdmin=Player;
			return;
		}

		ServerAdministrator.RequestAddBlackList(Player.Id);

	/*	UserAdmin ua = ServerAdministrator.UserAdminList.Find(iter => iter.Id == Player.Id);
		if (null == ua)
		{
			ua = new UserAdmin(Player.Id, Player.RoleName, 0);
			ServerAdministrator.UserAdminList.Add(ua);

		}

		UIArrayBlackAdmin.Add(ua);
		mUIBalckInfoList.Add(ua);
		UIArrayPersonnelAdmin.Remove(ua);
		mUIPersonelInfoList.Remove(ua);

		ua.AddPrivileges(AdminMask.BlackRole);*/
		
	}


	

}
