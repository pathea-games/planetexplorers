using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VCEMirrorGL : MonoBehaviour
{
	public GameObject m_PlaneX;
	public GameObject m_PlaneY;
	public GameObject m_PlaneZ;
	public GameObject m_AxisXY;
	public GameObject m_AxisYZ;
	public GameObject m_AxisZX;
	public GameObject m_Point;

	public List<float> m_Xs;
	public List<float> m_Ys;
	public List<float> m_Zs;

	// Use this for initialization
	void Start ()
	{
		m_Xs = new List<float> ();
		m_Ys = new List<float> ();
		m_Zs = new List<float> ();
	}
	
	// Update is called once per frame
	void Update ()
	{
		m_Xs.Clear();
		m_Ys.Clear();
		m_Zs.Clear();
		if ( VCERefPlane.YRef > 0 )
			m_Ys.Add((float)VCERefPlane.YRef * VCEditor.s_Scene.m_Setting.m_VoxelSize);
		if ( VCEditor.s_Mirror.XPlane_Masked )
			m_Xs.Add(VCEditor.s_Mirror.m_PosX * VCEditor.s_Scene.m_Setting.m_VoxelSize);
		if ( VCEditor.s_Mirror.YPlane_Masked )
			m_Ys.Add(VCEditor.s_Mirror.m_PosY * VCEditor.s_Scene.m_Setting.m_VoxelSize);
		if ( VCEditor.s_Mirror.ZPlane_Masked )
			m_Zs.Add(VCEditor.s_Mirror.m_PosZ * VCEditor.s_Scene.m_Setting.m_VoxelSize);

		m_PlaneX.SetActive(VCEditor.s_Mirror.XPlane_Masked);
		m_PlaneY.SetActive(VCEditor.s_Mirror.YPlane_Masked);
		m_PlaneZ.SetActive(VCEditor.s_Mirror.ZPlane_Masked);
		m_AxisXY.SetActive(VCEditor.s_Mirror.XYAxis_Masked);
		m_AxisYZ.SetActive(VCEditor.s_Mirror.YZAxis_Masked);
		m_AxisZX.SetActive(VCEditor.s_Mirror.ZXAxis_Masked);
		m_Point.SetActive(VCEditor.s_Mirror.Point_Masked);

		m_PlaneX.transform.localPosition = new Vector3(VCEditor.s_Mirror.m_PosX, 0, 0) * VCEditor.s_Scene.m_Setting.m_VoxelSize;
		m_PlaneY.transform.localPosition = new Vector3(0, VCEditor.s_Mirror.m_PosY, 0) * VCEditor.s_Scene.m_Setting.m_VoxelSize;
		m_PlaneZ.transform.localPosition = new Vector3(0, 0, VCEditor.s_Mirror.m_PosZ) * VCEditor.s_Scene.m_Setting.m_VoxelSize;
		m_AxisXY.transform.localPosition = new Vector3(VCEditor.s_Mirror.m_PosX, VCEditor.s_Mirror.m_PosY, 0) * VCEditor.s_Scene.m_Setting.m_VoxelSize;
		m_AxisYZ.transform.localPosition = new Vector3(0, VCEditor.s_Mirror.m_PosY, VCEditor.s_Mirror.m_PosZ) * VCEditor.s_Scene.m_Setting.m_VoxelSize;
		m_AxisZX.transform.localPosition = new Vector3(VCEditor.s_Mirror.m_PosX, 0, VCEditor.s_Mirror.m_PosZ) * VCEditor.s_Scene.m_Setting.m_VoxelSize;
		m_Point.transform.localPosition = new Vector3(VCEditor.s_Mirror.m_PosX, VCEditor.s_Mirror.m_PosY, VCEditor.s_Mirror.m_PosZ) * VCEditor.s_Scene.m_Setting.m_VoxelSize;
	}
}
