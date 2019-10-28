using UnityEngine;
using System.Collections;



public class CSUI_ToolMenuCtrl : MonoBehaviour 
{
	[System.Serializable]
	public class ToolMenu
	{
		public GameObject	  	mWnd;
		public UICheckbox		mChcekedBox;
		[HideInInspector]
		public bool 			mTweenState = false;
		[HideInInspector]
		public Collider 		mCollider;
		[HideInInspector]
		public UIButtonTween 	mBtnTween;
		
		public void Init()
		{
			mCollider = mChcekedBox.GetComponent<BoxCollider>();
			mBtnTween = mChcekedBox.GetComponent<UIButtonTween>();
		}
		
		public void UpdateBtnTweenState()
		{
			
			if (mBtnTween == null || mWnd == null || mChcekedBox == null || mCollider== null) 
				return;
			
			if (!mChcekedBox.isChecked && mWnd.activeSelf && mTweenState == true )
			{
				mBtnTween.Play(false);
				mTweenState = false;
				mCollider.enabled = false;
			}
			else if (!mWnd.activeSelf )
			{
				mTweenState = true;
				mCollider.enabled = true;
			}
		}
	}

	[SerializeField]
	public ToolMenu mOptionMenu;
	[SerializeField]
	public ToolMenu mWorkerMenu;
	[SerializeField]
	UISprite mMenuBg;
    [SerializeField]
    UIButton mHelpBtn;

	void Start()
	{
		mOptionMenu.Init();
		mWorkerMenu.Init();
	}
	
	void Update()
	{
		mOptionMenu.UpdateBtnTweenState();
		mWorkerMenu.UpdateBtnTweenState();

		UpdateWorkMenuState();
	}

	void UpdateWorkMenuState()
	{
		bool showOp = IsShowOptionMenu();
		bool showWork = IsShowWorkMenu();

		if (!showOp && mOptionMenu.mChcekedBox.isChecked)
		{
			mOptionMenu.mChcekedBox.isChecked = false;
		}

		if (!showWork && mWorkerMenu.mChcekedBox.isChecked)
		{
			mWorkerMenu.mChcekedBox.isChecked = false;
		}

		mOptionMenu.mChcekedBox.gameObject.SetActive(showOp);
		mWorkerMenu.mChcekedBox.gameObject.SetActive(showWork);

		if (showOp&&showWork)
			mMenuBg.transform.localScale = new Vector3(110,30,1);
		else if(showOp)
			mMenuBg.transform.localScale = new Vector3(75,30,1);
        else
            mMenuBg.transform.localScale = new Vector3(40, 30, 1);
    }

	bool IsShowWorkMenu()
	{
		int tag = CSUI_MainWndCtrl.Instance.mWndPartTag;
		// Assembly
		if (tag == CSConst.dtAssembly)
			return false;
		//etStorage
		else if (tag == CSConst.dtStorage)
			return true;
		//etPowerPlant
		else if (tag == CSConst.dtppCoal)
			return false;
		//dtDwelling
		else if (tag == CSConst.dtDwelling)
			return false;
		//dtEngineer
		else if (tag == CSConst.dtEngineer)
			return true;
		//dtFarm
		else if (tag == CSConst.dtFarm)
			return true;
		//dtFactory
		else if (tag == CSConst.dtFactory)
			return true;
		//etPersonnel
		else if (tag == CSConst.dtPersonnel)
			return false;
		return false;
	}


	bool IsShowOptionMenu()
	{
		int tag = CSUI_MainWndCtrl.Instance.mWndPartTag;
		if ((tag == CSConst.dtPersonnel)||(tag == CSConst.dtDwelling)
		    || tag == -1)
			return false;
		else 
			return true;
	}

    void CloseOptionWnd()
    {
        mOptionMenu.mChcekedBox.isChecked = false;
    }

    void CloseWorkWnd()
    {
        mWorkerMenu.mChcekedBox.isChecked = false;
    }
}
