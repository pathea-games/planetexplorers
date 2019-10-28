using UnityEngine;
using System.Collections;

public class BlackListItem_N : MonoBehaviour 
{
	public UILabel mName;
	private int mRoleId;

	public void SetInfo(UserAdmin ua)
	{
		mName.text = ua.RoleName;
		mRoleId = ua.Id;
	}

	public void Remove()
	{
        if (null != PlayerNetwork.mainPlayer)
			ServerAdministrator.RequestDeleteBlackList(mRoleId);
	}

	void OnSelected(bool selected)
	{
		//GameGui_N.Instance.mPersonnelManageGui.OnBlackListItemSelected(this, selected);
	}
}
