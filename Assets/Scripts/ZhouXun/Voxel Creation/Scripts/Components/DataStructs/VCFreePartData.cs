using UnityEngine;
using System;
using System.Collections;
using System.IO;

public class VCFreePartData : VCPartData
{
	public override void Validate ()
	{
		PositionValidate();
		m_Scale.x = Mathf.Clamp(m_Scale.x, 0.1f, 10);
		m_Scale.y = Mathf.Clamp(m_Scale.y, 0.1f, 10);
		m_Scale.z = Mathf.Clamp(m_Scale.z, 0.1f, 10);

		// [VCCase] - Some VCFreePartData should limit the transform
		if ( m_Type == EVCComponent.cpVtolRotor )
		{
			m_Scale.x = Mathf.Clamp(m_Scale.x, 0.2f, 2);
			m_Scale.y = Mathf.Clamp(m_Scale.y, 0.2f, 2);
			m_Scale.z = Mathf.Clamp(m_Scale.z, 0.2f, 2);
		}
		else if (m_Type == EVCComponent.cpHeadLight || m_Type == EVCComponent.cpLight)
		{
			m_Scale.x = Mathf.Clamp(m_Scale.x, 0.2f, 2);
			m_Scale.y = Mathf.Clamp(m_Scale.y, 0.2f, 2);
			m_Scale.z = Mathf.Clamp(m_Scale.z, 0.2f, 2);
		}
		else if ( m_Type == EVCComponent.cpShipRudder )
		{
			m_Scale.x = Mathf.Clamp(m_Scale.x, 0.2f, 2);
			m_Scale.y = Mathf.Clamp(m_Scale.y, 0.2f, 2);
			m_Scale.z = Mathf.Clamp(m_Scale.z, 0.2f, 2);
			m_Rotation.x = 0;
			m_Rotation.y = 0;
			m_Rotation.z = 0;
		}
		else if ( m_Type == EVCComponent.cpShipPropeller )
		{
			m_Scale.x = Mathf.Clamp(m_Scale.x, 0.2f, 2);
			m_Scale.y = Mathf.Clamp(m_Scale.y, 0.2f, 2);
			m_Scale.z = Mathf.Clamp(m_Scale.z, 0.2f, 2);
		}
		else if ( m_Type == EVCComponent.cpAirshipThruster )
		{
			m_Scale.x = Mathf.Clamp(m_Scale.x, 0.2f, 2f);
			m_Scale.y = Mathf.Clamp(m_Scale.y, 0.2f, 2f);
			m_Scale.z = Mathf.Clamp(m_Scale.z, 0.2f, 2f);
		}
		else if (m_Type == EVCComponent.cpRobotBattery
			|| m_Type == EVCComponent.cpRobotController)
		{
			m_Scale.x = Mathf.Clamp(m_Scale.x, 0.2f, 2f);
			m_Scale.y = Mathf.Clamp(m_Scale.y, 0.2f, 2f);
			m_Scale.z = Mathf.Clamp(m_Scale.z, 0.2f, 2f);
		}
		else if (m_Type == EVCComponent.cpAITurretWeapon
			|| m_Type == EVCComponent.cpRobotWeapon)
		{
			m_Scale.x = Mathf.Clamp(m_Scale.x, 0.2f, 2f);
			m_Scale.y = Mathf.Clamp(m_Scale.y, 0.2f, 2f);
			m_Scale.z = Mathf.Clamp(m_Scale.z, 0.2f, 2f);

			float avg;

			if (m_Scale.x != m_Scale.y && m_Scale.x != m_Scale.z && m_Scale.y != m_Scale.z)
			{
				avg = (m_Scale.x + m_Scale.y + m_Scale.z) / 3f;
			}
			else
			{
				if (m_Scale.x == m_Scale.y) avg = m_Scale.z;
				else if (m_Scale.x == m_Scale.z) avg = m_Scale.y;
				else avg = m_Scale.x;
			}

			m_Scale.Set(avg, avg, avg);
		}
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
