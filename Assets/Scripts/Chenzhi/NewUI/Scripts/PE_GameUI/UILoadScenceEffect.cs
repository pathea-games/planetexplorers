using UnityEngine;
using System.Collections;

public class UILoadScenceEffect : MonoBehaviour 
{
	#region members
	static UILoadScenceEffect	mInstance;
	public static UILoadScenceEffect Instance
    {
        get
        {
            return mInstance;
        }
    }

	[SerializeField] GameObject mProcessUI;
	[SerializeField] UILabel   	mLbProcess;
	[SerializeField] UISprite	mLoadSpr;
	[SerializeField] UITexture 	mBlackTexture;
	[SerializeField] UITexture  mLogoTexture;
	[SerializeField] GameObject mMaskUI;


	const float mLogoFadeTime = 2f;
	const float mLogoShowTime = 1f;
	const float mWaitTime = 1.2f;

	bool isBeginScence = false;
	bool isEndScence = false;
	bool isStarLoad = false;

	public delegate void CallbackEnvet();
	private	event CallbackEnvet e_BeginScenceOK = null;
	private event CallbackEnvet e_EndScenceOK = null;
	private event CallbackEnvet e_LogoAllIn = null;
	private event CallbackEnvet e_LogoAllOut = null;

	public enum LogoState
	{
		Null,
		LogoIn,
		Logo,
		LogoOut
	}
	LogoState mLogoState;
	#endregion

	public bool isInProgress{ get { return isActiveAndEnabled && mLogoState != LogoState.Null; } }

	#region interface
	public void EnableProgress(bool enable)
	{
		gameObject.SetActive(true);
//		mBlackTexture.enabled = !enable;
		mBlackTexture.alpha = 1;
		mProcessUI.SetActive(enable);
		isStarLoad = enable;
		tempTime = 0;

	}

	public void SetProgress(int value)
	{
		mLbProcess.text = value.ToString() + "%";
	}

	public void BeginScence(CallbackEnvet beginScenceOK, bool enableProgressUI = false)
	{
		e_BeginScenceOK = beginScenceOK;
		gameObject.SetActive(true);
		mBlackTexture.alpha = 1f;
		mBlackTexture.enabled = true;
		mProcessUI.SetActive(enableProgressUI);
		SetProgress (0);
		tempTime = 0;
		mMaskUI.SetActive(true);
		isBeginScence = true;
		isEndScence = false;
	}

	public void EndScence(CallbackEnvet endScenceOK, bool enableProgressUI = false)
	{
		Pathea.PeGameMgr.yirdName = "";
		e_EndScenceOK = endScenceOK;
		gameObject.SetActive(true);
		mBlackTexture.enabled = true;
		if (enableProgressUI) {
			mBlackTexture.alpha = 1f;
			mProcessUI.SetActive (true);
			SetProgress (0);
			tempTime = mWaitTime;
		} else {
			mBlackTexture.alpha = 0f;
			mProcessUI.SetActive (false);
			SetProgress (0);
			tempTime = mWaitTime;
		}
		mMaskUI.SetActive(true);
		isEndScence = true;
		isBeginScence = false;
		
		if(null != Pathea.MainPlayerCmpt.gMainPlayer)
			Pathea.MainPlayerCmpt.gMainPlayer.StartInvincible();
	}
	public void PalyLogoTexture(CallbackEnvet logoAllIn, CallbackEnvet logoAllOut)
	{
		gameObject.SetActive(true);
		mBlackTexture.enabled = true;
		mProcessUI.SetActive(false);
		e_LogoAllIn = logoAllIn;
		e_LogoAllOut = logoAllOut;
		mLogoTexture.mainTexture = Resources.Load("Texture2d/Tex/leibang_intro_logo") as Texture2D;
		tempTime = 0;
//		mBlackTexture.enabled =false;
		mMaskUI.gameObject.SetActive(true);
		mLogoState = LogoState.LogoIn;
	}

	#endregion

	void  Awake()
	{
		mInstance = this;
		mLogoState = LogoState.Null;
		GameObject.DontDestroyOnLoad(gameObject);
	}

	float tempTime = 0f;
	// Update is called once per frame
	void Update () 
	{
		if (isStarLoad)
		{
			tempTime += Time.deltaTime;
			mLoadSpr.transform.rotation = Quaternion.Euler(0,0,(int)(tempTime/0.025f) * -14.4f);
		}
		else if (isBeginScence)
		{
//			tempTime += Time.deltaTime;
			mBlackTexture.alpha = Mathf.Clamp01(1 - tempTime / mWaitTime) ;

			if (tempTime >= mWaitTime)
			{
				isBeginScence = false;
				mMaskUI.SetActive(false);
				gameObject.SetActive(false);

                //lz-2016.06.23 场景加载完成恢复声音
                AudioListener.pause = false;
				if (e_BeginScenceOK != null)
				{
					e_BeginScenceOK();
					e_BeginScenceOK = null;
				}

			}

			tempTime += Time.deltaTime;
		}

		else if (isEndScence)
		{
			tempTime += Time.deltaTime;
			mBlackTexture.alpha =  Mathf.Clamp01(tempTime / mWaitTime) ; 


			if (tempTime >= mWaitTime)
			{
				isEndScence = false;
				if (e_EndScenceOK != null)
				{
					e_EndScenceOK();
					e_EndScenceOK = null;
				}

			}
		}	
		else
		{
			switch (mLogoState)
			{
			case LogoState.LogoIn:
				if(!mMaskUI.gameObject.activeSelf)
					mMaskUI.gameObject.SetActive(true);
				mBlackTexture.alpha = 1f;
				mLogoTexture.alpha = tempTime/mLogoFadeTime;
				tempTime += Time.deltaTime;
				if(tempTime > mLogoFadeTime)
				{
					if(e_LogoAllIn != null){
						e_LogoAllIn();
						e_LogoAllIn = null;
					}
					mLogoState = LogoState.Logo;
					tempTime = Time.realtimeSinceStartup;
				}
				break;
			case LogoState.Logo:
				mBlackTexture.alpha = 1f;
				mLogoTexture.alpha = 1f;				
				if(Time.realtimeSinceStartup - tempTime > mLogoShowTime)
				{
					mLogoState = LogoState.LogoOut;
					tempTime = 0;
				}
				break;
			case LogoState.LogoOut:
				mBlackTexture.alpha = 1f;
				mLogoTexture.alpha = 1f - tempTime/mLogoFadeTime;
				tempTime += Time.deltaTime;
				if(tempTime > mLogoFadeTime)
				{
					if (e_LogoAllOut != null)
					{
						e_LogoAllOut();
						e_LogoAllOut = null;
					}
					mMaskUI.gameObject.SetActive(false);
					tempTime = 0;
					mLogoTexture.mainTexture = null;
					mLogoState = LogoState.Null;
				}
				break;
			default :
				break;
			}
		}
	}
}
