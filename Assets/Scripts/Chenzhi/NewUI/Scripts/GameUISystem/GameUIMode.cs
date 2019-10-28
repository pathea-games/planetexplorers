using UnityEngine;
using System.Collections;
using ItemAsset.PackageHelper;

public class GameUIMode : MonoBehaviour 
{
	static GameUIMode mInstance;
	public static GameUIMode Instance{ get { return mInstance;} }

	void Awake()
	{
		mInstance = this;
		curUIMode = UIMode.um_base;
	}

	void Start()
	{

	}


	bool _init = false;
	void Init()
	{
		if (!_init)
		{
			GameUI.Instance.mUIMainMidCtrl.onTweenFinished += OnMidCtrlTweenFinished;
			GameUI.Instance.mBuildBlock.onTweenFinished += OnBuildTweenFinished;
			_init = true;
		}
	}

	public enum UIMode
	{
		um_base = 0,
		um_building,
		um_driving
	}
	
	public UIMode curUIMode
	{
		get;
		private set;
	}

	public void GotoBaseMode()
	{
		ChangeMode(UIMode.um_base);
	}

	public void GotoBuildMode()
	{
		ChangeMode(UIMode.um_building);
	}

	public void GotoVehicleMode()
	{
		ChangeMode(UIMode.um_driving);
	}

	void ChangeMode(UIMode target)
	{
//		HideMode();
//		curUIMode = target;
//		ShowMode();
		Init();
		if (curUIMode != target)
		{
			if (curUIMode == UIMode.um_base)
				GameUI.Instance.mUIMainMidCtrl.PlayTween(true);
			else if (curUIMode == UIMode.um_building)
			{

				GameUI.Instance.mBuildBlock.PlayTween(false);
			}

			curUIMode = target;
		}
	}

	void OnMidCtrlTweenFinished (bool forward)
	{
		if (forward)
		{
			if (curUIMode == UIMode.um_building)
			{
//				GameUI.Instance.mUIMainMidCtrl.Hide();
				GameUI.Instance.mBuildBlock.EnterBlock();
				GameUI.Instance.mBuildBlock.PlayTween(true);
//				Invoke("PlayBuildTweenDelay");
			}
		}

	}
	
	void PlayBuildTweenDelay()
	{
		GameUI.Instance.mBuildBlock.PlayTween(false);
	}

	void OnBuildTweenFinished (bool forward)
	{
		if (forward)
		{
			if (curUIMode == UIMode.um_base)
			{
				GameUI.Instance.mBuildBlock.QuitBlock(); 
				GameUI.Instance.mUIMainMidCtrl.PlayTween(false);
			}
		}
	}

	void HideMode()
	{
		if (curUIMode == UIMode.um_base)
		{
			GameUI.Instance.mUIMainMidCtrl.Hide();
		}
		else if (curUIMode == UIMode.um_building)
		{
			GameUI.Instance.mBuildBlock.QuitBlock(); 
//			ToggleBuildCam(false);
		}
	}

	void ShowMode()
	{
		if (curUIMode == UIMode.um_base)
		{
			GameUI.Instance.mUIMainMidCtrl.Show();
		}
		
		else if (curUIMode == UIMode.um_building)
		{
			GameUI.Instance.mBuildBlock.EnterBlock();
//			ToggleBuildCam(true);
		}
	}

//	CamMode mBuildCam = null;
//	void ToggleBuildCam(bool bBuilding)
//	{
//		if (bBuilding)
//		{
//			if (null == mBuildCam)
//			{
//				mBuildCam = PECameraMan.Instance.EnterBuild(null);
//			}
//		}
//		else
//		{
//			PECameraMan.Instance.RemoveCamMode(mBuildCam);
//		}
//	}

}

