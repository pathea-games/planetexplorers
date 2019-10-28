using UnityEngine;
using System.Collections;
using Pathea;

/// <summary>
/// NOTES : 旧的照相功能，现在已经没有用， 请使用新版
/// PeViewStudio 如有不明白，请垂询教主 
/// </summary>

public class TakeEquipmentPhoto  
{
	public GameObject mEqPhoto;
	public GameObject mViewObj;

	public ViewCameraControler mBodyViewCtr;

	public void Init(Vector3 target)
	{
		if (mEqPhoto == null)
			mEqPhoto= new GameObject("EqPhotoObj");
		mEqPhoto.transform.parent = UIOthers.Inctence.transform;
		mEqPhoto.transform.localScale = Vector3.one;
		mEqPhoto.transform.localPosition =  target;
		
		mBodyViewCtr = ViewCameraControler.CreatViewCamera();
		mBodyViewCtr.Init(true);
		mBodyViewCtr.SetLightState(true);
		mBodyViewCtr.SetActive(false);
	}

	Vector3 modelAngle = Vector3.zero;

	public void ResetViewCmpt(Pathea.BiologyViewCmpt viewCmpt)
	{
		if (viewCmpt == null)
			return; 

		if (mViewObj != null)
		{
			modelAngle = mViewObj.transform.localRotation.eulerAngles;
			GameObject.Destroy(mViewObj);
		}
		
		mViewObj = viewCmpt.CloneModel();
		if (mViewObj == null)
			return ; 
		mViewObj.name = "Player";
		mViewObj.transform.parent = mEqPhoto.transform;
		mViewObj.transform.localPosition = Vector3.zero;
		mViewObj.transform.localRotation = Quaternion.Euler(modelAngle);

		mViewObj.layer = Layer.ShowModel;

		Renderer[] renders = mViewObj.GetComponentsInChildren<Renderer>();
		for (int i=0; i<renders.Length; i++)
			renders[i].gameObject.layer =
				renders[i].gameObject.layer == Layer.GIEProductLayer ? Layer.ShowModelCreation : Layer.ShowModel;

		Projector[] projectors = mViewObj.GetComponentsInChildren<Projector>();
		for (int i = 0; i < projectors.Length; i++)
			projectors[i].gameObject.layer = Layer.ShowModelCreation;

		mBodyViewCtr.SetTarget(mViewObj,ViewCameraControler.ViewPart.VP_All);
		Camera mCam = mBodyViewCtr.GetComponent<Camera>();
		mCam.depth = 0;
		mCam.transform.parent = null;
		mCam.transform.position = mViewObj.transform.position + new Vector3(0,1.31f,1.883728f);
		mCam.transform.localEulerAngles = new Vector3(11,180,0);
		mCam.cullingMask = Layer.Mask.ShowModel | Layer.Mask.ShowModelCreation;
		mCam.nearClipPlane = 0.1f;

		try
		{
			Component[] comps = mViewObj.GetComponentsInChildren<Component>(true);
			foreach(Component comp in comps)
			{
                //if(null != (comp as Rigidbody))
                //    mViewObj.GetComponent<Rigidbody>() .constraints = RigidbodyConstraints.FreezeAll;
                //else
                    if(null == (comp as Animator) 
				        && null == (comp as SkinnedMeshRenderer)
				        && null == (comp as Transform)
				        )
					MonoBehaviour.Destroy(comp);
			}
		}
		catch
		{
			Debug.Log("delect equipment modle cmpt error!");
		}

	}

	public Texture PhotoTex { get { return mBodyViewCtr.GetTex();}}

}