using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class VCDecalData : VCComponentData, IVCMultiphaseComponentData
{
	public static int s_ComponentId = 9901;
	public static string s_DecalPrefabPath = "Prefab/Decal Prefabs/Voxel Creation Decal";
	public ulong m_Guid = 0;
	public int m_AssetIndex = -1;
	public float m_Size = 0.1f;
	public float m_Depth = 0.01f;
	public bool m_Mirrored = false;
	public int m_ShaderIndex = 0;
	public Color m_Color = Color.white;

	public VCDecalData ()
	{
		m_ComponentId = s_ComponentId;
		m_Type = EVCComponent.cpDecal;
	}

	public int Phase
	{
		get { return m_Mirrored ? 1 : 0; }
		set { m_Mirrored = (value != 0); }
	}
	public void InversePhase ()
	{
		m_Mirrored = !m_Mirrored;
	}

	public override GameObject CreateEntity (bool for_editor, Transform parent)
	{
		if ( m_Entity != null )
			DestroyEntity();
		m_Entity = GameObject.Instantiate(Resources.Load(s_DecalPrefabPath) as GameObject) as GameObject;
		m_Entity.name = "Decal Image";
		if ( for_editor )
		{
			m_Entity.transform.parent = VCEditor.Instance.m_DecalGroup.transform;
			VCEComponentTool tool = m_Entity.GetComponent<VCEComponentTool>();
			tool.m_IsBrush = false;
			tool.m_InEditor = true;
			tool.m_ToolGroup.SetActive(true);
			tool.m_SelBound.enabled = false;
			tool.m_SelBound.GetComponent<Collider>().enabled = false;
			tool.m_SelBound.m_BoundColor = GLComponentBound.s_Blue;
			tool.m_Data = this;
			Collider[] cs = m_Entity.GetComponentsInChildren<Collider>(true);
			foreach ( Collider c in cs )
			{
				if ( c.gameObject != tool.m_SelBound.gameObject )
					c.enabled = false;
			}
		}
		else
		{
			m_Entity.transform.parent = parent;
			Transform[] trs = m_Entity.GetComponentsInChildren<Transform>(true);
			foreach ( Transform t in trs )
				t.gameObject.layer = VCConfig.s_ProductLayer;
		}
		UpdateEntity(for_editor);
		if (!for_editor) UpdateComponent();
		return m_Entity;
	}
	public override void UpdateEntity (bool for_editor)
	{
		if ( for_editor )
		{
			VCEComponentTool tool = m_Entity.GetComponent<VCEComponentTool>();
			tool.m_Data = this;
		}
		m_Rotation = VCEMath.NormalizeEulerAngle(m_Rotation);
		m_Entity.transform.localPosition = m_Position;
		m_Entity.transform.localEulerAngles = m_Rotation;
		m_Entity.transform.localScale = m_Scale;
		VCDecalHandler dh = m_Entity.GetComponent<VCDecalHandler>();
		if ( for_editor )
		{
			dh.m_Guid = m_Guid;
			dh.m_Iso = null;
			dh.m_AssetIndex = -1;
		}
		else
		{
			dh.m_Guid = 0;
			dh.m_Iso = m_CurrIso;
			dh.m_AssetIndex = m_AssetIndex;
		}
		dh.m_Size = m_Size;
		dh.m_Depth = m_Depth;
		dh.m_Mirrored = m_Mirrored;
		dh.m_ShaderIndex = m_ShaderIndex;
		dh.m_Color = m_Color;
	}
	public override void Validate ()
	{
		PositionValidate();
		m_Scale = Vector3.one;
	}
	public override void Import (byte[] buffer)
	{
		if ( buffer == null )
			return;
		using ( MemoryStream ms = new MemoryStream (buffer) )
		{
			BinaryReader r = new BinaryReader (ms);

			//m_ComponentId = r.ReadInt32();	// int
			int value32 = r.ReadInt32();        // int
			m_ComponentId = value32 & 0xFFFF;
			m_ExtendData = value32 >> 16;

			m_Type = (EVCComponent)(r.ReadInt32());	// int
			m_Position.x = r.ReadSingle();	// float
			m_Position.y = r.ReadSingle();	// float
			m_Position.z = r.ReadSingle();	// float
			m_Rotation.x = r.ReadSingle();	// float
			m_Rotation.y = r.ReadSingle();	// float
			m_Rotation.z = r.ReadSingle();	// float
			m_Scale.x = r.ReadSingle();	// float
			m_Scale.y = r.ReadSingle();	// float
			m_Scale.z = r.ReadSingle();	// float
			m_Visible = r.ReadBoolean();	// bool
			m_Guid = r.ReadUInt64();	// ulong
			m_AssetIndex = r.ReadInt32();	// int
			m_Size = r.ReadSingle();	// float
			m_Depth = r.ReadSingle();	// float
			m_Mirrored = r.ReadBoolean();	// bool
			m_ShaderIndex = r.ReadInt32();	// int
			m_Color.r = r.ReadSingle();	// float
			m_Color.g = r.ReadSingle();	// float
			m_Color.b = r.ReadSingle();	// float
			m_Color.a = r.ReadSingle();	// float
			r.Close();
		}
	}
	public override byte[] Export ()
	{
		using ( MemoryStream ms = new MemoryStream () )
		{
			BinaryWriter w = new BinaryWriter (ms);
			w.Write(m_ComponentId | (m_ExtendData << 16));  // int
			w.Write((int)(m_Type));	// int
			w.Write(m_Position.x);	// float
			w.Write(m_Position.y);	// float
			w.Write(m_Position.z);	// float
			w.Write(m_Rotation.x);	// float
			w.Write(m_Rotation.y);	// float
			w.Write(m_Rotation.z);	// float
			w.Write(m_Scale.x);	// float
			w.Write(m_Scale.y);	// float
			w.Write(m_Scale.z);	// float
			w.Write(m_Visible);	// bool
			w.Write(m_Guid);	// ulong
			w.Write(m_AssetIndex);	// int
			w.Write(m_Size);	// float
			w.Write(m_Depth);	// float
			w.Write(m_Mirrored);	// bool
			w.Write(m_ShaderIndex);	// int
			w.Write(m_Color.r);	// float
			w.Write(m_Color.g);	// float
			w.Write(m_Color.b);	// float
			w.Write(m_Color.a);	// float
			w.Close();
			byte [] retval = ms.ToArray();
			return retval;
		}
	}
}
