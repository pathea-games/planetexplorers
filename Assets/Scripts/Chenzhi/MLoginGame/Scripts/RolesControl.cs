using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class ModeInfo
{
	public PlayerModel mModel = null;
	public MLRoleInfo mRoleInfo = null;

	public GameObject mMode
	{
		get
		{
			if (mRoleInfo != null)
				return mModel.mMode;
			else 
				return null;
		}
	}
}

public class RolesControl : MonoBehaviour 
{
	public Vector3 mRoleModeLocalPos_Male;
	public Vector3 mRoleModeLocalPos_Female;
	public Vector3 mRoleModeLocalScale;

	public Camera mainCamera;
	public GameObject[] Roles;
	private int selectedIndex = -1;
	public int GetSelectedIndex(){return selectedIndex;}
	public void SetSelectedIndex(int index){selectedIndex = index;}

	private int mouseMoveIndex = 0;
	private bool bMoseMoveIn = false;
	public GameObject Selected { get { return Roles[selectedIndex]; } }

	private RoleCompent[] roleCompents;
	
	public GameObject[] ModePrefab;
	public string[] SelectedRolesAnimatorContorl;
	public string[] MouseMoveRolesAnimatorContorl;

	int modesCount = 0;
	private List<ModeInfo> modeInfoList = new List<ModeInfo>();

	CustomCharactor.AvatarData mMaleAvataData;
	CustomCharactor.AvatarData mFemaleAvataData;

	void Awake()
	{
		mMaleAvataData = new CustomCharactor.AvatarData();
//		mMaleAvataData.SetPart(CustomCharactor.AvatarData.ESlot.Torso,"Model/PlayerModel/male03-torso");
//		mMaleAvataData.SetPart(CustomCharactor.AvatarData.ESlot.Legs,"Model/PlayerModel/male03-legs");
//		mMaleAvataData.SetPart(CustomCharactor.AvatarData.ESlot.Feet,"Model/PlayerModel/male03-feet");

		mFemaleAvataData = new CustomCharactor.AvatarData();
//		mFemaleAvataData.SetPart(CustomCharactor.AvatarData.ESlot.Torso,"Model/PlayerModel/female03-torso");
//		mFemaleAvataData.SetPart(CustomCharactor.AvatarData.ESlot.Legs,"Model/PlayerModel/female03-legs");
//		mFemaleAvataData.SetPart(CustomCharactor.AvatarData.ESlot.Feet,"Model/PlayerModel/female03-feet");
	}

	// Use this for initialization
	void Start () 
	{
		if(selectedIndex == -1)
		{
			selectedIndex = 0;
		}
		roleCompents = new RoleCompent[Roles.Length];
		
		for (int i=0; i<Roles.Length; i++ )
		{
			float angle = (float)i/(float)Roles.Length * Mathf.PI * 2.0f;
			Roles[i].transform.position = new Vector3 (10*Mathf.Cos(angle), 0, 10*Mathf.Sin(angle));
			roleCompents[i] = Roles[i].GetComponent<RoleCompent>();
		}
		
		UpdateModeInfo();
	}

	public void DeleteModeInfo(int deleteIndex)
	{
		GameObject.Destroy(modeInfoList[deleteIndex].mMode);
		modeInfoList[deleteIndex] = null;
	}

	public void UpdateModeInfo()
	{
		RebuildModels();
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (Input.GetMouseButtonDown(0))
		{
			MousePickRole();
			StopAnimator();
		}
		UpdateRoles();
		MouseMoveRole();

		PalyRolesAnimator();
		if(bMoseMoveIn)
			MouseMoveAnimator();
	}

	void RebuildModels()
	{
		if(modesCount!=0)
		{
			for (int i=0;i<modeInfoList.Count;i++) 
			{
				if(modeInfoList[i].mMode != null)
					GameObject.Destroy(modeInfoList[i].mMode);
			}
			modeInfoList.Clear();
		}
		
		modesCount = MLPlayerInfo.Instance.GetMaxRoleCount();

		for (int i=0;i<modesCount;i++)
		{
			ModeInfo modeinfo = new ModeInfo();
			modeinfo.mRoleInfo =  MLPlayerInfo.Instance.GetRoleInfo(i);

			if(modeinfo.mRoleInfo == null)
			{
				modeinfo.mModel = null;
			}
			else
			{	
				bool bFemale = modeinfo.mRoleInfo.sex == 1;
				CustomCharactor.ESex sex = bFemale ? CustomCharactor.ESex.Female : CustomCharactor.ESex.Male;
				GameObject ob = GameObject.Instantiate(sex == CustomCharactor.ESex.Female ?  ModePrefab[0] :  ModePrefab[1]) as GameObject;

				modeinfo.mModel = ob.GetComponent<PlayerModel>();
				modeinfo.mModel.mAppearData = new AppearBlendShape.AppearData();
				modeinfo.mModel.mAppearData.Deserialize(modeinfo.mRoleInfo.mRoleInfo.appearData);
				modeinfo.mModel.mNude = new CustomCharactor.AvatarData();
				modeinfo.mModel.mNude.Deserialize(modeinfo.mRoleInfo.mRoleInfo.nudeData);
				modeinfo.mModel.mClothed = bFemale ? mFemaleAvataData : mMaleAvataData;
				
				modeinfo.mMode.transform.parent = roleCompents[i].m_boxCollider.transform;
				modeinfo.mMode.transform.localPosition = bFemale ? 
					new Vector3(mRoleModeLocalPos_Female.x,mRoleModeLocalPos_Female.y,mRoleModeLocalPos_Female.z) :
					new Vector3(mRoleModeLocalPos_Male.x,mRoleModeLocalPos_Male.y,mRoleModeLocalPos_Male.z);
				modeinfo.mMode.transform.localRotation = Quaternion.Euler(new Vector3(0,90,0));
				modeinfo.mMode.transform.localScale = new Vector3(mRoleModeLocalScale.x,mRoleModeLocalScale.y,mRoleModeLocalScale.x);
				modeinfo.mMode.SetActive(true);
				modeinfo.mModel.BuildModel();
			}

			modeInfoList.Add(modeinfo);
		}
	}

	int index = 0;
	string oldAnim = "";
	bool addAnminIndex = false;
	void PalyRolesAnimator()
	{
		if (selectedIndex< 0 || selectedIndex >= modesCount)
			return;

		if(modeInfoList.Count == 0)
			return;

		if(modeInfoList[selectedIndex] == null)
			return;

		if(modeInfoList[selectedIndex].mMode == null)
			return;

		int length = SelectedRolesAnimatorContorl.Length;
		if(length == 0)
			return;

		Animator anim = modeInfoList[selectedIndex].mMode.GetComponent<Animator>();

		int temp = Mathf.FloorToInt(anim.GetCurrentAnimatorStateInfo(0).normalizedTime);
		float animTime = anim.GetCurrentAnimatorStateInfo(0).normalizedTime - temp ;

		if(animTime > 0.95f  && addAnminIndex == false)
		{

			if("DressStand" != SelectedRolesAnimatorContorl[index])
			{
				anim.SetBool(SelectedRolesAnimatorContorl[index],true);
				oldAnim = SelectedRolesAnimatorContorl[index];
				Invoke("CloseAimn",0.1f);
			}

			//Debug.Log(SelectedRolesAnimatorContorl[index]);
			index++;
			if(index >= length)
				index = 0;
			addAnminIndex = true;

		}
		else if(addAnminIndex == true && animTime < 0.5f )
		{
			addAnminIndex = false;
		}
	}

	void CloseAimn()
	{
		if (selectedIndex< 0 || selectedIndex >= modesCount)
			return;
		
		if(modeInfoList.Count == 0)
			return;
		
		if(modeInfoList[selectedIndex] == null)
			return;
		
		if(modeInfoList[selectedIndex].mMode == null)
			return;
		
		int length = SelectedRolesAnimatorContorl.Length;
		if(length == 0)
			return;

		Animator anim = modeInfoList[selectedIndex].mMode.GetComponent<Animator>();
		anim.SetBool(oldAnim,false);
	}


	int MouseMovePalyCount =0;
	void MouseMoveAnimator()
	{
		if(selectedIndex == mouseMoveIndex)
			return;

		if(modeInfoList[mouseMoveIndex] == null)
			return;

		if(modeInfoList[mouseMoveIndex].mMode == null)
			return;

		Animator anim = modeInfoList[mouseMoveIndex].mMode.GetComponent<Animator>();
		
		int length = MouseMoveRolesAnimatorContorl.Length;
		if(length == 0)
			return;
		int index = MouseMovePalyCount % length ;

		
		anim.SetBool(MouseMoveRolesAnimatorContorl[index],true);
		if(index!=0)
			anim.SetBool(MouseMoveRolesAnimatorContorl[index-1],false);
		else
			anim.SetBool(MouseMoveRolesAnimatorContorl[length-1],false);
		
		MouseMovePalyCount++;

	}


	void StopAnimator()
	{
		for (int i=0;i<modesCount;i++)
		{
			if(i == selectedIndex)
				continue;

			if(modeInfoList[i] == null)
				continue;

			if(modeInfoList[i].mMode == null)
				continue;

			Animator anim = modeInfoList[i].mMode.GetComponent<Animator>();
			
			anim.SetBool("Common1",false);
			anim.SetBool("Common2",false);
			anim.SetBool("DressIdle1",false);
			anim.SetBool("DressIdle2",false);
		}
	}
	
	void MousePickRole()
	{
		Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
		RaycastHit rayHit;
		float mindist = 1000f;
	
		for (int i=0; i<roleCompents.Length; i++ )
		{
			bool ok = roleCompents[i].m_boxCollider.Raycast(ray,out rayHit,100);
			if (ok)
			{
				if ( rayHit.distance < mindist )
				{
					mindist = rayHit.distance;
					selectedIndex = i;
				}
			}
		}
	}

	void UpdateRoles ()
	{
		Vector3 targetangle = new Vector3 (0, (float)selectedIndex/(float)Roles.Length * 360.0f, 0);
		Quaternion targetq = Quaternion.Euler(targetangle);  // 寻找球面最短路径
		transform.localRotation = Quaternion.Slerp(transform.localRotation, targetq, 0.05f);
	
		for ( int i=0;i<3;i++)
		{
			roleCompents[i].transform.rotation = Quaternion.identity;
		}

	}
	
	void MouseMoveRole()
	{
		Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
		RaycastHit rayHit;
		
		for (int i=0; i<Roles.Length; i++ )
		{
			if(selectedIndex == i)
			{
				continue;
			}
			else
			{
				bool ok = roleCompents[i].m_boxCollider.Raycast(ray,out rayHit,100);
				if (ok)
				{
					mouseMoveIndex = i;
					if( bMoseMoveIn == false)
					{
						MouseMovePalyCount = 0;
						bMoseMoveIn = true;
					}
				}
				else
				{
					if(i == mouseMoveIndex && bMoseMoveIn == true)
					{
						bMoseMoveIn =false;
						StopAnimator();
					}
				}
			}
		}
	}


	public void TakeRoleHerderTexture()
	{
		if (selectedIndex != -1 && selectedIndex < Roles.Length && modeInfoList[selectedIndex].mMode != null )
//			RoleHerderTexture.SetTexture( PhotoStudio.Instance.TakePhoto(modeInfoList[selectedIndex].mMode, modeInfoList[selectedIndex].mRoleInfo.sex ) );
			PeViewStudio.TakePhoto(modeInfoList[selectedIndex].mMode, 64, 64, PeViewStudio.s_HeadPhotoPos, PeViewStudio.s_HeadPhotoRot);
	}




}
