using UnityEngine;
using System.Collections;

public struct ViewControllerParam
{
//	public Vector3 offset;
	
	public float camNearClip;
	public float camFarClip;
	
	public float camAspect;
	public float camFieldOfView;
	
	public bool orthographic;
	public float orthographicSize;
	
	public int texWidth;
	public int texHeight;
	
	
	
	// Default Params
	public static ViewControllerParam DefaultCharacter
	{
		get
		{
			ViewControllerParam param = new ViewControllerParam();
//			param.offset = new Vector3(0f, 1.27f, 1.8f);
			param.camNearClip = 0.1f;
			param.camFarClip = 10f;
			param.orthographic = false;
			param.camFieldOfView = 45f;
            param.texWidth = 620;
			param.texHeight = 960;
			param.camAspect = (float)param.texWidth / (float)param.texHeight;

			return param;
		}
	}
	
	public static ViewControllerParam DefaultPortrait
	{
		get
		{
			ViewControllerParam param = new ViewControllerParam();
//			param.offset = new Vector3(0, 1.27f, 1.8f);
			param.camNearClip = 0f;
			param.camFarClip = 10f;
			param.camAspect = 1f;
			param.orthographic = true;
			param.orthographicSize = 0.175f;
			param.texWidth = 256;
			param.texHeight = 256;
			
			return param;
		}
	}
	
    /// <summary>
    /// lz-2016.07.21 怪物图鉴的ViewController参数
    /// </summary>
    public static ViewControllerParam DefaultSpicies
    {
        get 
        {
            ViewControllerParam param = new ViewControllerParam();
            param.camNearClip = 0.1f;
            param.camFarClip = 100f;
            param.orthographic = false;
            param.camFieldOfView = 45f;
            param.texWidth = 550;
            param.texHeight = 330;
            param.camAspect = (float)param.texWidth / (float)param.texHeight;

            return param;
        }
    }
}

public class ViewController : MonoBehaviour 
{
	public int ID;

	[SerializeField] protected Camera  _viewCam;

	public Camera viewCam { get { return _viewCam; }}

	protected RenderTexture m_RenderTex;
	protected Transform m_Target;

	public RenderTexture RenderTex { get { return m_RenderTex;} }
	public Transform target { get { return m_Target; } }

	[SerializeField] protected Vector3 m_Offset;
	protected Quaternion m_Rot = Quaternion.identity;

	public void SetLocalTrans (Vector3 pos, Quaternion rot)
	{
		SetLocalPos (pos);
		SetLocalRot(rot);
	}

	public void SetLocalPos (Vector3 pos)
	{
		m_Offset = pos;
	}

	public void SetLocalRot (Quaternion rot)
	{
		m_Rot = rot;
	}

	public void Set ( Transform target, ViewControllerParam param)
	{
		SetTarget( target);
		
		SetParam(param);
	}

	public virtual void SetTarget (Transform target)
	{
		m_Target = target;
	}

	public virtual void SetParam (ViewControllerParam param)
	{
		if (m_RenderTex == null )
		{
			m_RenderTex = new RenderTexture(param.texWidth, param.texHeight, 16, RenderTextureFormat.ARGB32);
			m_RenderTex.isCubemap = false;
		}
		else if (m_RenderTex.width != param.texWidth || m_RenderTex.height == param.texHeight)
		{
			m_RenderTex.Release();
			RenderTexture.Destroy(m_RenderTex);
			
			m_RenderTex = new RenderTexture(param.texWidth, param.texHeight, 16, RenderTextureFormat.ARGB32);
			m_RenderTex.isCubemap = false;
		}
		
		_viewCam.targetTexture = m_RenderTex;
		_viewCam.nearClipPlane = param.camNearClip;
		_viewCam.farClipPlane = param.camFarClip;
		_viewCam.aspect = param.camAspect;
		_viewCam.fieldOfView = param.camFieldOfView;
		_viewCam.orthographic = param.orthographic;
		_viewCam.orthographicSize = param.orthographicSize;

	}

	public Texture2D GetTexture2D ()
	{
		RenderTexture currentRT = RenderTexture.active;
		RenderTexture.active = _viewCam.targetTexture;
		_viewCam.Render();
		Texture2D image = new Texture2D(_viewCam.targetTexture.width, _viewCam.targetTexture.height, TextureFormat.ARGB32, false);
		//Texture2D image = new Texture2D(cam.targetTexture.width, cam.targetTexture.height);
		image.ReadPixels(new Rect(0, 0, _viewCam.targetTexture.width, _viewCam.targetTexture.height), 0, 0);
		image.Apply();
		RenderTexture.active = currentRT;
		return image;
	}
	

}
