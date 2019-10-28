using UnityEngine;
using System.Collections;

public class UIMLoginControl : MonoBehaviour 
{
	public N_ImageButton btnCreate;
	public N_ImageButton btnDelete;
	public UILabel lbRoleName;
	

	public RolesControl rc = null;
	public Transform[] roleTransform;

	
	public delegate void OnClickFun();
	public OnClickFun BtnCreate = null;
	public OnClickFun BtnDelete = null;
	public OnClickFun BtnEdit = null;
	public OnClickFun BtnEnter = null;
	public OnClickFun BtnBack = null;
	public OnClickFun BtnOK = null;
	public OnClickFun BtnCancel = null;

	public float testNamePos_Y = 4;
	private int deleteRoleIndex;
	
	// Use this for initialization
	void Start () 
	{


	}
	
	// Update is called once per frame
	void Update () 
	{
		UpdateButtonState();
		UpdateRolesName();
	}


	public void UpdateRolesName()
	{
		if(MLPlayerInfo.Instance.GetRoleInfo(rc.GetSelectedIndex()) != null )
		{
			if(MLPlayerInfo.Instance.GetRoleInfo(rc.GetSelectedIndex()).name != lbRoleName.text)
				lbRoleName.text = MLPlayerInfo.Instance.GetRoleInfo(rc.GetSelectedIndex()).name;
		}
		else 
		{
			lbRoleName.text = string.Empty;
		}
	}
	
	void UpdateButtonState()
	{
		if(MLPlayerInfo.Instance.GetRoleListNotNullCount() == 0 && btnDelete.isEnabled == true)
			EnableButton(btnDelete,false);
		else
		{
			if(MLPlayerInfo.Instance.GetRoleInfo(rc.GetSelectedIndex()) == null  && btnDelete.isEnabled == true)
				EnableButton(btnDelete,false);
			else if(MLPlayerInfo.Instance.GetRoleInfo(rc.GetSelectedIndex()) != null  &&  btnDelete.isEnabled == false)
				EnableButton(btnDelete,true);
		}


		if(MLPlayerInfo.Instance.GetRoleListNotNullCount() == MLPlayerInfo.Instance.GetMaxRoleCount())
		{
			if( btnCreate.isEnabled == true)
				EnableButton(btnCreate,false);
		}
		else 
		{
			if(btnCreate.isEnabled == false)
				EnableButton(btnCreate,true);
		}
	}


	//private int test =0;
	void BtnCreateOnClick()
	{
		Debug.Log ("btnCreate OnClick");

//------------------------------------test code------------------------------
//		AppearanceData data =  CharacterData.GetAppearanceData(1);
//		data.mSkinColor = new Color(0.88f, 0.88f, 0.88f, 1);
//		data.mEyeColor = new Color(0.88f, 0.88f, 0.88f, 1);
//		data.mHairColor =new Color(0.88f, 0.88f, 0.88f, 1);
//		data.mHeight = 1;
//		data.mWith = 1;
//		int[] equiID = {10100009,20210001,20310005,20210010,20510009,20610009};
//		//int[] equiID ={};
//
//		MLPlayerInfo.AddRoleInfo(test.ToString(), data ,equiID);
//		rc.UpdateModeInfo();
//		test++;
//---------------------------------------------------------------
		if(MLPlayerInfo.Instance.GetRoleListNotNullCount() == MLPlayerInfo.Instance.GetMaxRoleCount())
			return;

		//LogoGui_N.Instance.GotoCreateRole();
		GameClientLobby.Self.TryCreateRole();

		if(BtnCreate != null)
			BtnCreate();
	}

	void BtnDeleteOnClick()
	{
		Debug.Log ("btnDelete OnClick");

		if (MLPlayerInfo.Instance.GetRoleInfo(rc.GetSelectedIndex()) == null)
			return;
		deleteRoleIndex = rc.GetSelectedIndex();
		string msg = PELocalization.GetString(8000590)+ MLPlayerInfo.Instance.GetRoleInfo(deleteRoleIndex).name +"'?";
		MessageBox_N.ShowYNBox(msg,BtnOKOnClick,BtnCancelOnClick);
	
		if(BtnDelete != null)
			BtnDelete();
	}

	void BtnEditOnClick()
	{
		if(BtnEdit != null)
			BtnEdit();
	}

	void BtnEnterOnClick()
	{
		if (MLPlayerInfo.Instance.GetRoleInfo(rc.GetSelectedIndex()) == null)
			return;

		rc.TakeRoleHerderTexture();

		if(BtnEnter != null) BtnEnter();

		Invoke("EnterLobby",0.1f);
	}

	// 照相同时切换场景会崩溃
	void EnterLobby()
	{
        bool isEditor = false;
#if UNITY_EDITOR
        isEditor = true;
#endif
        if (isEditor)
        {
            CustomData.RoleInfo _Role = MLPlayerInfo.Instance.GetRoleInfo(rc.GetSelectedIndex()).mRoleInfo;
            if (!SystemSettingData.Instance.Tutorialed && _Role.name.Equals("tutorial"))
            {
                string msg = PELocalization.GetString(8000986) + "'?";
                MessageBox_N.ShowYNBox(msg,BtnYOnClick, BtnNoOnClick);
            }
            else
            {
                BtnNoOnClick();
            }

        }
        else
        {

            if (!SystemSettingData.Instance.Tutorialed)
            {
                string msg = PELocalization.GetString(8000986) + "'?";
                MessageBox_N.ShowYNBox(msg, BtnYOnClick, BtnNoOnClick);
            }
            else
            {
                BtnNoOnClick();
            }
        }
	}

    void BtnYOnClick()
    {
        PeSceneCtrl.Instance.GotToTutorial(MLPlayerInfo.Instance.GetRoleInfo(rc.GetSelectedIndex()).mRoleInfo);

    }

    void BtnNoOnClick()
    {
        GameClientLobby.Self.TryEnterLobby(MLPlayerInfo.Instance.GetRoleInfo(rc.GetSelectedIndex()).mRoleInfo.roleID);
    }

    void BtnBackOnClick()
	{
		PeSceneCtrl.Instance.GotoMainMenuScene();
		if(BtnBack != null)
			BtnBack();
	}

	// msgbox btnOK OnClick
	void BtnOKOnClick()
	{
		GameClientLobby.Self.TryDeleteRole(MLPlayerInfo.Instance.GetRoleInfo(deleteRoleIndex).mRoleInfo.roleID);
		if(BtnOK != null)
			BtnOK();
	}

	void BtnCancelOnClick()
	{

		if(BtnCancel != null)
			BtnCancel();
	}



	private void EnableButton(N_ImageButton btn,bool value)
	{
		if(value == true)
		{
			btn.isEnabled = true;
		}
		else
		{
			btn.isEnabled = false;
		}
	}


//	public float namePos_x = 3;
//	public float namePos_y = 0;
//	public float namePos_z = 0;
//
//	private void UpdateRolesName()
//	{
//		for (int i=0; i<MLPlayerInfo.Instance.GetMaxRoleCount(); i++)
//		{
//			if (roleName[i] == null)
//				return;
//			if(i<MLPlayerInfo.Instance.GetMaxRoleCount())
//			{
//				if(MLPlayerInfo.Instance.GetRoleInfo(i) == null)
//				{
//					roleName[i].enabled = false;
//					continue;
//				}
//
//				roleName[i].enabled = true;
//
//				roleName[i].text = MLPlayerInfo.Instance.GetRoleInfo(i).name;
//				Vector3 wordPos = roleTransform[i].position;
//				wordPos.x += namePos_x;
//				wordPos.y += namePos_y;
//				wordPos.z += namePos_z;
//				Vector3 screenPos = mainCamera.WorldToScreenPoint(wordPos);
//				roleName[i].transform.localPosition = screenPos;
//			}
//			else
//			{
//				roleName[i].enabled = false;
//			}
//
//		}
//	}
}
