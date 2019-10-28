using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathea;

/// <summary>
/// NOTES : 旧的照相功能，现在已经没有用， 请使用新版
/// PeViewStudio 如有不明白，请垂询教主 
/// </summary>

public class PhotoStudio : MonoBehaviour
{
	static PhotoStudio mInstance;
	
	public static PhotoStudio Instance
    {
        get
        {
            if (mInstance == null)
            {
                if (Application.isPlaying)
                {
                    GameObject go = Resources.Load<GameObject>("Prefab/Other/PhotoStudio");
                    if (go != null)
                    {
                        Object.Instantiate(go);
                    }
                }
            }

            return mInstance;
        }
    }
	
	public Camera mPhotoCam;
    public new GameObject light;

    void Awake()
    {
        mInstance = this;

        light.SetActive(false);
        mPhotoCam.aspect = 1;
		mPhotoCam.clearFlags = CameraClearFlags.Color;
		mPhotoCam.targetTexture = new RenderTexture(128,128,16, RenderTextureFormat.ARGB32);
		mPhotoCam.gameObject.SetActive(false);
	}
    /*
    public Texture2D TakePhoto(GameObject obj)
    {
        GameObject mCopyModel = Instantiate(obj.transform.FindChild("PersonModel").gameObject) as GameObject;
        mCopyModel.transform.parent = transform;
        mCopyModel.transform.localPosition = Vector3.zero;
        mCopyModel.layer = showModelLayer;
        MonoBehaviour[] Components = mCopyModel.GetComponents<MonoBehaviour>();

        foreach (MonoBehaviour comp in Components)
        {
            comp.enabled = false;
        }

        Transform tran = mCopyModel.transform.FindChild("Bip01/Bip01 Pelvis/Bip01 Spine1/Bip01 Spine2/Bip01 Spine3/Bip01 Neck/Bip01 Head");
        if (tran != null)
        {
            mPhotoCam.transform.parent = tran;
            mPhotoCam.transform.localPosition = new Vector3(-0.06070369f, 0.6686818f, -0.03194972f);
            mPhotoCam.transform.localRotation = Quaternion.Euler(new Vector3(85.67319f, -0.7489014f, 86.21811f));
        }

        light.transform.position = mPhotoCam.transform.position;

        SkinnedMeshRenderer[] smrArray = mCopyModel.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        foreach (SkinnedMeshRenderer smr in smrArray)
        {
            if (!smr.enabled)
            {
                smr.enabled = true;
            }
        }

        light.SetActive(true);
        Texture2D tex = RTImage(mPhotoCam);
        light.SetActive(false);

        mPhotoCam.transform.parent = transform;

        mCopyModel.SetActive(false);

        GameObject.Destroy(mCopyModel);

        return tex;
    }
    */
    public Texture2D TakePhoto(GameObject obj, int sex)
    {
        string path = "Prefab/Npc/";
        if (1 == sex)
        {
            path += "RandomNpcViewFemale";
        }
        else if (2 == sex)
        {
            path += "RandomNpcView";
        }

        GameObject mHeroViewModle = GameObject.Instantiate(Resources.Load(path)) as GameObject;
        if (null == mHeroViewModle)
        {
            return null;
        }

        mHeroViewModle.transform.parent = transform;
        mHeroViewModle.transform.localPosition = Vector3.zero;
        mHeroViewModle.transform.localRotation = Quaternion.identity;
        mHeroViewModle.transform.localScale = Vector3.one;

        GameObject mCopyModel = Instantiate(obj) as GameObject;

//		MonoBehaviour[] scripts =  
		Component[] cmpts  = mCopyModel.GetComponents<Component>();
		foreach (Component cmpt in cmpts)
		{
			if (cmpt.GetType() == typeof(SkinnedMeshRenderer) || cmpt.GetType() == typeof(Animator)
			    || cmpt.GetType() == typeof(Transform))
				continue;

			Component.Destroy(cmpt);
		}

        mCopyModel.transform.parent = mHeroViewModle.transform;
        mCopyModel.transform.localPosition = Vector3.zero;
        mCopyModel.transform.localRotation = Quaternion.identity;
        mCopyModel.transform.localScale = Vector3.one;
        mCopyModel.layer = Layer.ShowModel;

        SkinnedMeshRenderer[] smrArray = mCopyModel.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        foreach (SkinnedMeshRenderer smr in smrArray)
        {
			smr.gameObject.layer = Layer.ShowModel;
            if (!smr.enabled)
            {
                smr.enabled = true;
            }
        }

        mHeroViewModle.SetActive(true);

        Transform tran = mCopyModel.transform.FindChild("Bip01/Bip01 Pelvis/Bip01 Spine1/Bip01 Spine2/Bip01 Spine3/Bip01 Neck/Bip01 Head");
        if (tran != null)
        {
//			mPhotoCam.transform.parent = tran;
//			mPhotoCam.transform.localPosition = new Vector3(-0.06070369f, 0.6686818f, -0.03194972f);
//
//			mPhotoCam.transform.localRotation = Quaternion.Euler(new Vector3(87.07319f, -0.7489014f, 86.21811f));

			mPhotoCam.transform.parent = tran;
//			mPhotoCam.transform.localPosition = new Vector3(0.03f, 0.33f, -0.23f);
			mPhotoCam.transform.localPosition = new Vector3(-0.01f, 0.213f, -0.155f);
			
			mPhotoCam.transform.localRotation = Quaternion.Euler(new Vector3(54.1671f, 344.2581f, 77.74331f));
        }

        light.transform.position = mPhotoCam.transform.position;
		light.transform.localPosition = new Vector3(light.transform.localPosition.x, light.transform.localPosition.y - 1f, light.transform.localPosition.z);

        light.SetActive(true);
        Texture2D tex = RTImage(mPhotoCam);
        light.SetActive(false);

        mPhotoCam.transform.parent = transform;

        //mHeroViewModle.SetActive(false);

        GameObject.DestroyImmediate(mHeroViewModle);

        return tex;
    }

    public Texture2D TakePhoto1(GameObject obj, int sex)
    {
        string path = "Prefab/Npc/";
        if (1 == sex)
        {
            path += "RandomNpcViewFemale";
        }
        else if (2 == sex)
        {
            path += "RandomNpcView";
        }

        GameObject mHeroViewModle = GameObject.Instantiate(Resources.Load(path)) as GameObject;
        if (null == mHeroViewModle)
        {
            return null;
        }

        mHeroViewModle.transform.parent = transform;
        mHeroViewModle.transform.localPosition = Vector3.zero;
        mHeroViewModle.transform.localRotation = Quaternion.identity;
        mHeroViewModle.transform.localScale = Vector3.one;

        GameObject mCopyModel = Instantiate(obj) as GameObject;
        mCopyModel.transform.parent = mHeroViewModle.transform;
        mCopyModel.transform.localPosition = Vector3.zero;
        mCopyModel.transform.localRotation = Quaternion.identity;
        mCopyModel.transform.localScale = Vector3.one;
        mCopyModel.layer = Layer.ShowModel;

        mHeroViewModle.SetActive(true);

        SkinnedMeshRenderer[] smrArray = mCopyModel.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        foreach (SkinnedMeshRenderer smr in smrArray)
        {
            if (!smr.enabled)
            {
                smr.enabled = true;
            }
        }

        Transform tran = mCopyModel.transform.FindChild("Bip01/Bip01 Pelvis/Bip01 Spine1/Bip01 Spine2/Bip01 Spine3/Bip01 Neck/Bip01 Head");
        if (tran != null)
        {
            mPhotoCam.transform.parent = tran;
            mPhotoCam.transform.localPosition = new Vector3(-0.06070369f, 0.6686818f, -0.03194972f);
			mPhotoCam.transform.localRotation = Quaternion.Euler(new Vector3(87.07319f, -0.7489014f, 86.21811f));
        }

        light.transform.position = mPhotoCam.transform.position;

        light.SetActive(true);
        Texture2D tex = RTImage(mPhotoCam);
        //light.SetActive(false);

        //mPhotoCam.transform.parent = transform;

        //mHeroViewModle.SetActive(false);

        //GameObject.Destroy(mHeroViewModle);

        return tex;
    }

    public static Texture2D RTImage(Camera cam)
    {
        RenderTexture currentRT = RenderTexture.active;
        RenderTexture.active = cam.targetTexture;
        cam.Render();
        Texture2D image = new Texture2D(cam.targetTexture.width, cam.targetTexture.height, TextureFormat.RGB24, false);
        //Texture2D image = new Texture2D(cam.targetTexture.width, cam.targetTexture.height);
        image.ReadPixels(new Rect(0, 0, cam.targetTexture.width, cam.targetTexture.height), 0, 0);
        image.Apply();
        RenderTexture.active = currentRT;
        return image;
    }
}