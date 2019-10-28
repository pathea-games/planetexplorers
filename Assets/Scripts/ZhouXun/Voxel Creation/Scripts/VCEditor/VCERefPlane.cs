using UnityEngine;
using System.Collections;

public class VCERefPlane : MonoBehaviour
{
	public static int XRef = 0;
	public static int YRef = 0;
	public static int ZRef = 0;
	
	public static void Reset ()
	{
		XRef = YRef = ZRef = 0;
	}
	
	public GLGridPlane m_BasePlane;
	public Transform m_XRefTrans;
	public Transform m_YRefTrans;
	public Transform m_ZRefTrans;
	
	// Update is called once per frame
	void Update ()
	{
		if ( VCERefPlane.XRef > 0 )
			m_XRefTrans.gameObject.SetActive(true);
		else
			m_XRefTrans.gameObject.SetActive(false);
		if ( VCERefPlane.YRef > 0 )
		{
			m_YRefTrans.gameObject.SetActive(true);
			m_BasePlane.m_ShowGrid = false;
		}
		else
		{
			m_YRefTrans.gameObject.SetActive(false);
			m_BasePlane.m_ShowGrid = true;
		}
		if ( VCERefPlane.ZRef > 0 )
			m_ZRefTrans.gameObject.SetActive(true);
		else
			m_ZRefTrans.gameObject.SetActive(false);
		
		float voxelsize = (VCEditor.DocumentOpen()) ? (VCEditor.s_Scene.m_Setting.m_VoxelSize) : (0.01f);
		
		m_XRefTrans.localPosition = Vector3.right * (VCERefPlane.XRef * voxelsize);
		m_YRefTrans.localPosition = Vector3.up * (VCERefPlane.YRef * voxelsize);
		m_ZRefTrans.localPosition = Vector3.forward * (VCERefPlane.ZRef * voxelsize);
	}
}
