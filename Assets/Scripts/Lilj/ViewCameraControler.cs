using UnityEngine;
using System.Collections;
#if UNITY_5
using BlurEffect = UnityStandardAssets.ImageEffects.BlurOptimized;
#endif

/// <summary>
/// NODES : 旧的照相功能，现在已经没有用， 请使用新版
/// PeViewStudio 如有不明白，请垂询教主 
/// </summary>

public class ViewCameraControler : MonoBehaviour
{
	public enum ViewPart
	{
		VP_All,
		VP_Head,
		VP_BigHead
	}
	
	ViewPart 		mViewPart;
	
	RenderTexture 	mRenderTexture;
	bool 			mGetPictrue;
	public bool		mAlwaysActive;
	
	GameObject		mTargetObj;
//	int				mTargetSex;
	
	BlurEffect		mBlurEffect;
	
	Light[]			mLight;
	
	public Vector3  moffset = new Vector3(-0.03f,0.1f,-0.06f);
	
	Transform mTargetTran;
	
	public static ViewCameraControler CreatViewCamera()
	{
		GameObject cam = GameObject.Instantiate(Resources.Load("Prefab/Other/ViewCamera") ,  Vector3.one,Quaternion.identity ) as GameObject;
		return cam.GetComponent<ViewCameraControler>();
	}
	
	public void SetActive(bool active)
	{
		gameObject.SetActive(active);
	}

	private Camera mCamera = null;
	public Camera viewCamera 
	{
		get 
		{
			if (mCamera == null) 
				viewCamera = GetComponent<Camera>();
			return mCamera;
		}
		private set {mCamera = value;}
	}
	public void Init(bool alwaysActive)
	{
		GetComponent<Camera>().nearClipPlane = 0.01f;
		GetComponent<Camera>().farClipPlane = 3f;
		mGetPictrue = false;
		mBlurEffect = gameObject.AddComponent<BlurEffect>();
#if UNITY_5
		mBlurEffect.blurIterations = 0;
		mBlurEffect.blurSize = 0;
#else
		mBlurEffect.iterations = 0;
		mBlurEffect.blurSpread = 0;
#endif
		mLight = new Light[2];
		mLight[0] = transform.FindChild("Pointlight1").GetComponent<Light>();
		mLight[1] = transform.FindChild("Pointlight2").GetComponent<Light>();
		mAlwaysActive = alwaysActive;
		
		mRenderTexture = new RenderTexture(512, 512,16);
		mRenderTexture.isCubemap = false;
		GetComponent<Camera>().targetTexture = mRenderTexture;
	}
	
	void Update ()
	{
		if(!mRenderTexture.IsCreated())
		{
			Photo();
			mRenderTexture.Create();
		}
		if(!mAlwaysActive && !mGetPictrue)
			gameObject.SetActive(false);
		if(mGetPictrue)
			mGetPictrue = false;
		if(null != mTargetTran && mViewPart != ViewPart.VP_All)
			transform.LookAt(mTargetTran.position+0.02f*Vector3.up,Vector3.up);
	}
	
	public void SetTarget(GameObject targetObj,ViewPart viewPart = ViewPart.VP_Head)
	{
		mTargetObj = targetObj;
		SetViewPart(viewPart);
	}
	
	public RenderTexture GetTex()
	{
		return GetComponent<Camera>().targetTexture;
	}
	
	public void Photo()
	{
		mGetPictrue = true;
		gameObject.SetActive(true);
	}
	
	public void SetViewPart(ViewPart viewPart)
	{
		mViewPart = viewPart;
		
		switch(mViewPart)
		{
		case ViewPart.VP_All:
			{
				mTargetTran = mTargetObj.transform;
				if(mTargetTran != null)
				{
					transform.parent = mTargetTran;
					transform.localPosition = new Vector3(0f,0.9f,2f);
				}
				GetComponent<Camera>().orthographic = false;
				GetComponent<Camera>().aspect = 190f / 232f;
				SetLightState(true);
				transform.LookAt(mTargetTran.position+Vector3.up*0.9f,Vector3.up);
				mBlurEffect.enabled = false;
			}
			break;
		case ViewPart.VP_Head:
			{
				mTargetTran = mTargetObj.transform.FindChild("Bip01/Bip01 Pelvis/Bip01 Spine1/Bip01 Spine2/Bip01 Spine3/Bip01 Neck/Bip01 Head");
				if(mTargetTran != null)
				{
					transform.parent = mTargetTran;
					transform.localPosition = -1 *new Vector3(0.06378798f,-0.2264594f,0.1151667f);
				}
				GetComponent<Camera>().orthographic = true;
				GetComponent<Camera>().orthographicSize = 0.19f;
				GetComponent<Camera>().aspect = 1;
				transform.LookAt(mTargetTran.position,Vector3.up);
				mBlurEffect.enabled = false;
			}
			break;
		case ViewPart.VP_BigHead:
			{
				mTargetTran = mTargetObj.transform.FindChild("Bip01/Bip01 Pelvis/Bip01 Spine1/Bip01 Spine2/Bip01 Spine3/Bip01 Neck/Bip01 Head");
				if(mTargetTran != null)
				{
					transform.parent = mTargetTran;
					transform.localPosition = new Vector3(-0.06070369f,0.6686818f,-0.03194972f);
					transform.localRotation = Quaternion.Euler(new Vector3(85.67319f,-0.7489014f,86.21811f)) ;
				}
				GetComponent<Camera>().orthographic = true;
				GetComponent<Camera>().orthographicSize = 0.12f;
				GetComponent<Camera>().aspect = 1;
				SetLightState(true);
				mBlurEffect.enabled = true;
				transform.LookAt(mTargetTran.position,Vector3.up);
			}
			break;
		}
	}
	
	public void SetLightState(bool state)
	{
		if(mLight != null)
		{
			mLight[0].enabled = state;
			mLight[1].enabled = state;
		}
	}
}
