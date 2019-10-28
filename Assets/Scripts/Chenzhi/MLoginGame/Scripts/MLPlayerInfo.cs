using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ItemAsset;
using CustomData;
using System;

public class MLPlayerInfo : MonoBehaviour
{

	public int[]  equipID_Male = {};
	public int[]  equipID_FeMale = {};

	public RolesControl mRolesCtrl;

	private static MLPlayerInfo self = null;
	public static MLPlayerInfo Instance 
	{
		get
		{
			return self;
		}
	}

	private static string selectedRoleName ;

	private static int MaxRoleCount = 3;
	public int GetMaxRoleCount(){return MaxRoleCount;}
	private static List<MLRoleInfo> mRoleInfoList = new List<MLRoleInfo>();


	//Net InterFace ------------------------------------------------------
	

	public bool DeleteRole(int roleId)
	{
		int index = -1;

		if(mRoleInfoList.Count == 0)
			return false;

		for (int i=0;i<mRoleInfoList.Count;i++)
		{
			if(mRoleInfoList[i] != null)
			{
				if(mRoleInfoList[i].mRoleInfo.roleID == roleId)
				{
					index = i;
					break;
				}
			}
		}

		return deleteRoleInfo(index);
	}



	public void SetSelectedRole(string roleName)
	{
		selectedRoleName = roleName;
	}

	public void UpdateScene()
	{
		if (mRolesCtrl == null)
			return;

		UpdateRolesInfo();
		mRolesCtrl.UpdateModeInfo();
	}


	//---------------------------------------------------------------------

	public int GetRoleListNotNullCount()
	{
		int count =0;
		for (int i=0;i<mRoleInfoList.Count;i++)
			if(mRoleInfoList[i] != null)
				count++;
		return count;
	}  

	public MLRoleInfo GetRoleInfo(int index)
	{
		if(index >= MaxRoleCount )
			return null;
		if(mRoleInfoList.Count == 0)
			return null;
		return  mRoleInfoList[index];
	}

	private void Awake()
	{
		self = this ;

		UpdateRolesInfo();

	}
	 
	public void UpdateRolesInfo()
	{
		mRoleInfoList.Clear();
		if( GameClientLobby.Self != null)
		{
			for (int i=0;i<  GameClientLobby.Self.myRolesExisted.Count ;i++)
			{
				if( GameClientLobby.Self.myRolesExisted[i].deletedFlag != 1)
				{
					AddRoleInfo(GameClientLobby.Self.myRolesExisted[i]);
				}
			}
		}

		if(selectedRoleName != null)
		{
			int index = -1;
			if(mRoleInfoList.Count == 0)
				return;
			
			for (int i=0;i<mRoleInfoList.Count;i++)
			{
				if(mRoleInfoList[i] != null)
					if(mRoleInfoList[i].name == selectedRoleName)
					{
						index = i;
						break;
					}
			}
			mRolesCtrl.SetSelectedIndex(index);
		}
	}


	public void Destory()
	{
		mRoleInfoList.Clear();
	}

	private void AddRoleInfo(RoleInfo info)
	{
		int index = -1;

		if(mRoleInfoList.Count == 0)
		{
			for(int i=0;i<MaxRoleCount;i++)
			{
				mRoleInfoList.Add(null);
			}
		}

		for(int i=0;i<MaxRoleCount;i++)
		{
			if(mRoleInfoList[i] == null)
			{
				index = i;
				break;	
			}
		}
		if(index == -1)
			return;

		MLRoleInfo MLinfo = new MLRoleInfo();
		MLinfo.mRoleInfo = info;
		MLinfo.name = info.name;
		MLinfo.sex =info.sex;
		mRoleInfoList[index] = MLinfo;
		
	}

	private bool deleteRoleInfo(int ListIndex)
	{
		if(ListIndex == -1 )
			return false;
		if(ListIndex >= mRoleInfoList.Count )
			return false;
		mRoleInfoList[ListIndex] = null;
		mRolesCtrl.DeleteModeInfo(ListIndex);
		return true;
	}

}

public class MLRoleInfo
{
	public string name;
	public int sex;
	public RoleInfo mRoleInfo ;
}
