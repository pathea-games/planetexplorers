using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//public struct PeViewControllerParam
//{
//	public Vector3 offset;
//
//	public float camNearClip;
//	public float camFarClip;
//
//	public float camAspect;
//	public Color camBgColor;
//	public float camFieldOfView;
//
//	public bool orthographic;
//	public float orthographicSize;
//
//	public int texWidth;
//	public int texHeight;
//
//
//
//	// Default Params
//	public static PeViewControllerParam DefaultPerps
//	{
//		get
//		{
//			PeViewControllerParam param = new PeViewControllerParam();
//			param.offset = new Vector3(0f, 1.27f, 1.8f);
//			param.camNearClip = 0.1f;
//			param.camFarClip = 5f;
//			param.camAspect = 0.875f;
//			param.camBgColor = new Color(0.247f, 0.274f, 0.314f, 0.02f);
//			param.camFieldOfView = 60;
//			param.orthographic = false;
//			param.texWidth = 512;
//			param.texHeight = 512;
//
//			return param;
//		}
//	}
//
//	public static PeViewControllerParam DefaultOrtho
//	{
//		get
//		{
//			PeViewControllerParam param = new PeViewControllerParam();
//			param.offset = new Vector3(0, 1.27f, 1.8f);
//			param.camNearClip = 0.1f;
//			param.camFarClip = 5f;
//			param.camAspect = 0.875f;
//			param.camBgColor = new Color(0.247f, 0.274f, 0.314f, 0.02f);
//			param.camFieldOfView = 60;
//			param.orthographic = true;
//			param.orthographicSize = 0.19f;
//			param.texWidth = 512;
//			param.texHeight = 512;
//
//			return param;
//		}
//	}
//
//}

//public class PeViewController : MonoBehaviour 
//{
//	[SerializeField] Camera  _viewCam;
//
//	public Camera viewCam { get { return _viewCam; }}
//
//	RenderTexture m_RenderTex;
//	Transform m_Target;
//
//	public RenderTexture RenderTex { get { return m_RenderTex;} }
//
//	[SerializeField] Vector3 m_Offset;
//	
//
//	[SerializeField] bool UpdateNow = false;
//	
//	public void SetTarget (Transform target)
//	{
//		m_Target = target;
//
//	}
//
//	public void Set ( Transform target, PeViewControllerParam param)
//	{
//		m_Target = target;
//
//		SetParam(param);
//	}
//
//	public void SetParam (PeViewControllerParam param)
//	{
//
//		if (m_RenderTex == null )
//		{
//			m_RenderTex = new RenderTexture(param.texWidth, param.texHeight, 16, RenderTextureFormat.ARGB32);
//			m_RenderTex.isCubemap = false;
//		}
//		else if (m_RenderTex.width != param.texWidth || m_RenderTex.height == param.texHeight)
//		{
//			m_RenderTex.Release();
//			RenderTexture.Destroy(m_RenderTex);
//			
//			m_RenderTex = new RenderTexture(param.texWidth, param.texHeight, 16, RenderTextureFormat.ARGB32);
//			m_RenderTex.isCubemap = false;
//		}
//
//		_viewCam.targetTexture = m_RenderTex;
//		_viewCam.nearClipPlane = param.camNearClip;
//		_viewCam.farClipPlane = param.camFarClip;
//		_viewCam.aspect = param.camAspect;
//		_viewCam.backgroundColor = param.camBgColor;
//		_viewCam.fieldOfView = param.camFieldOfView;
//		_viewCam.orthographic = param.orthographic;
//		_viewCam.orthographicSize = param.orthographicSize;
//
//		m_Offset = param.offset;
//	}
//
//	void Update ()
//	{
//
//		if (UpdateNow)
//		{
//			if (m_Target != null)
//			{
//				_viewCam.transform.position = m_Target.TransformPoint(m_Offset);
//				_viewCam.transform.LookAt (m_Target.position+ 0.9f*Vector3.up,Vector3.up);
//				_viewCam.transform.parent = m_Target.parent;
//			}
//		}
//	}
//}

public class PeViewController : ViewController
{
	public MovingGizmo moveHandle;
	public RotatingGizmo rotateHandle;
	public ScalingGizmo scaleHandle;


	public override void SetTarget (Transform target)
	{
		base.SetTarget (target);

		target.transform.localPosition = new Vector3(ID * 100, 0, 0);
	}
}
