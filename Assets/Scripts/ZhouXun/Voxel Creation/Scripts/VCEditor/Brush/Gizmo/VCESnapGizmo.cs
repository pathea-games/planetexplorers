using UnityEngine;
using System.Collections;

public class VCESnapGizmo : MonoBehaviour
{
	public VCEGizmoMesh m_SnapGizmo;
	private float m_VoxelSize = 0.01f;
	
	// Use this for initialization
	void Start ()
	{
		SetSize(1,1);
	}
	
	// Update is called once per frame
	void Update ()
	{
		if ( VCEditor.DocumentOpen() )
		{
			SetVoxelSize(VCEditor.s_Scene.m_Setting.m_VoxelSize);
		}
	}
	
	void SetVoxelSize (float voxelsize)
	{
		m_VoxelSize = voxelsize;
	}
	
	public void SetNormal (Vector3 normal)
	{
		transform.rotation = Quaternion.LookRotation(normal);
	}
	public void SetSize (int w, int h)
	{
		m_SnapGizmo.m_MeshSizeX = m_VoxelSize * (w+0.1f);
		m_SnapGizmo.m_MeshSizeY = m_VoxelSize * (h+0.1f);
		m_SnapGizmo.m_BorderSize = m_VoxelSize * 0.3f;
	}
}
