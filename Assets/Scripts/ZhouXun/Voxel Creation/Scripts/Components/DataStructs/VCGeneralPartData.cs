using UnityEngine;
using System;
using System.Collections;
using System.IO;

public class VCGeneralPartData : VCPartData
{
	public override void Validate ()
	{
		PositionValidate();
		
		// [VCCase] - Some VCGeneralPartData should limit the transform
		if ( m_Type == EVCComponent.cpSideSeat || 
		     m_Type == EVCComponent.cpVehicleCockpit ||
		     m_Type == EVCComponent.cpVtolCockpit ||
		     m_Type == EVCComponent.cpShipCockpit)
		{
			m_Rotation.x = 0;
			m_Rotation.z = 0;
		}
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
			w.Close();
			byte [] retval = ms.ToArray();
			return retval;
		}
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
		Renderer[] rs = m_Entity.GetComponentsInChildren<Renderer>(true);
		foreach ( Renderer r in rs )
		{
			if ( r is TrailRenderer )
				r.enabled = true;
			else if ( r is ParticleRenderer )
				r.enabled = true;
			else if ( r is ParticleSystemRenderer )
				r.enabled = true;
			else if ( r is LineRenderer )
				r.enabled = true;
			else if ( r is SpriteRenderer )
				r.enabled = true;
			else
				r.enabled = m_Visible;
		}
	}
}
