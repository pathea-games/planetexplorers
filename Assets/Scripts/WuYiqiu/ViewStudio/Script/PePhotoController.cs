using UnityEngine;
using System.Collections;
using System;

public class PePhotoController : ViewController
{

	public GameObject LightRoot;

	public override void SetTarget (Transform target)
	{
		base.SetTarget (target);

	}

	public Texture2D TakePhoto ()
	{
		if (m_Target == null)
			return null;

		Transform tran = m_Target.FindChild("Bip01/Bip01 Pelvis/Bip01 Spine1/Bip01 Spine2/Bip01 Spine3/Bip01 Neck/Bip01 Head");
		Transform old_parent = transform.parent;
		if (tran != null)
		{
			//			mPhotoCam.transform.parent = tran;
			//			mPhotoCam.transform.localPosition = new Vector3(-0.06070369f, 0.6686818f, -0.03194972f);
			//
			//			mPhotoCam.transform.localRotation = Quaternion.Euler(new Vector3(87.07319f, -0.7489014f, 86.21811f));
			
			transform.parent = tran;
			//			mPhotoCam.transform.localPosition = new Vector3(0.03f, 0.33f, -0.23f);
			transform.localPosition = m_Offset;
			
			transform.localRotation = m_Rot;
		}
		else
		{
            return null;
            //throw new Exception("Cant find neck node of view mode");
            //Debug.LogError("Cant find neck node of view mode");
		}

		LightRoot.transform.position = transform.position;
		LightRoot.transform.localPosition = new Vector3(LightRoot.transform.localPosition.x, LightRoot.transform.localPosition.y - 1f, LightRoot.transform.localPosition.z);

		Texture2D tex = RTImage(_viewCam);

		transform.parent = old_parent;

		return tex;
	}

	public static Texture2D RTImage(Camera cam)
	{
		RenderTexture currentRT = RenderTexture.active;
		RenderTexture.active = cam.targetTexture;
		cam.Render();
		Texture2D image = new Texture2D(cam.targetTexture.width, cam.targetTexture.height, TextureFormat.ARGB32, false);
		//Texture2D image = new Texture2D(cam.targetTexture.width, cam.targetTexture.height);
		image.ReadPixels(new Rect(0, 0, cam.targetTexture.width, cam.targetTexture.height), 0, 0);
		image.Apply();
		RenderTexture.active = currentRT;
		image.wrapMode = TextureWrapMode.Clamp;
        return image;
	}
}
