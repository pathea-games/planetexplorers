using UnityEngine;
using System;
using System.Collections;

[Serializable]
public class VCESceneSetting
{
	public int m_Id = 0;
	public int m_ParentId = 0;
	public string m_Name = "";
	public EVCCategory m_Category;
	
	public IntVector3 m_EditorSize;
	public float m_VoxelSize;
	public int m_MajorInterval;
	public int m_MinorInterval;
	public float m_BlockUnit;
	public float m_DyeUnit;
	
	public Vector3 EditorWorldSize { get { return m_EditorSize.ToVector3() * m_VoxelSize; } }
}
