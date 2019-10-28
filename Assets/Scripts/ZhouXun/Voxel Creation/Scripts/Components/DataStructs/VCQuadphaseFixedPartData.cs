using UnityEngine;
using System;
using System.Collections;
using System.IO;
using WhiteCat;
public class VCQuadphaseFixedPartData : VCFixedPartData, IVCMultiphaseComponentData
{
    public int m_Phase = 0;
	public int Phase { get { return m_Phase; } set { m_Phase = (value % 8); } }


	public bool isSteerWheel
	{
		get { return (m_Phase & 2) == 2; }
	}


	public bool isMotorWheel
	{
		get { return (m_Phase & 4) != 4; }
	}


	public void InversePhase ()
	{
		m_Phase >>= 1;
		if ((m_Phase & 1) == 0)
			m_Phase = (m_Phase << 1) + 1;
		else
			m_Phase = m_Phase << 1;
	}
	public override void Validate ()
	{
		PositionValidate();
		m_Rotation = Vector3.zero;
		m_Scale = Vector3.one;
		m_Phase = m_Phase % 8;
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
			m_Phase = r.ReadInt32();	// int
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
			w.Write(m_Phase);	// int
			w.Close();
			byte [] retval = ms.ToArray();
			return retval;
		}
	}

	public override GameObject CreateEntity(bool for_editor, Transform parent)
	{
		base.CreateEntity(for_editor, parent);
		var wheel = m_Entity.GetComponent<VCPVehicleWheel>();
		if (wheel != null) wheel.InitLayer();
		return m_Entity;
	}

	public override void UpdateEntity (bool for_editor)
	{
		VCEComponentTool tool = m_Entity.GetComponent<VCEComponentTool>();
		if ( tool != null )
		{
			tool.SetPhase(m_Phase);
			if ( for_editor )
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


	protected override void UpdateComponent(VCPart part)
	{
		base.UpdateComponent(part);

		if (part is VCPVehicleWheel)
		{
			(part as VCPVehicleWheel).isSteerWheel = (Phase & 2) == 2;
			(part as VCPVehicleWheel).isMotorWheel = (Phase & 4) != 4;
		}
	}
}
