using UnityEngine;
using System.Collections;
using System.IO;
using System;
using WhiteCat;

public class VCObjectLightData : VCPartData 
{
	public Color m_Color = Color.white;
	
	public override void Validate ()
	{
		PositionValidate();
		m_Scale.x = m_Scale.y = m_Scale.z = Mathf.Clamp(GetScaleValue(m_Scale), 0.25f, 4f);
	}


    static float GetScaleValue(Vector3 scale)
    {
        if (scale.x == scale.y)
        {
            return scale.z;
        }
        if (scale.x == scale.z)
        {
            return scale.y;
        }
        if (scale.y == scale.z)
        {
            return scale.x;
        }
        return Mathf.Max(Mathf.Max(scale.x, scale.y), scale.z);
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
			m_Color.r = r.ReadSingle();	// float
			m_Color.g = r.ReadSingle();	// float
			m_Color.b = r.ReadSingle();	// float
			m_Color.a = r.ReadSingle();	// float
			r.ReadSingle(); // float
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
			w.Write(m_Color.r);	// float
			w.Write(m_Color.g);	// float
			w.Write(m_Color.b);	// float
			w.Write(m_Color.a);	// float
			w.Write(2f); // flaot

			w.Close();
			byte [] retval = ms.ToArray();
			return retval;
		}
	}


	public override void UpdateEntity (bool for_editor)
	{
		if (for_editor)
		{
			VCEComponentTool tool = m_Entity.GetComponent<VCEComponentTool>();
			tool.m_Data = this;
		}
		m_Rotation = VCEMath.NormalizeEulerAngle(m_Rotation);
		m_Entity.transform.localPosition = m_Position;
		m_Entity.transform.localEulerAngles = m_Rotation;
		m_Entity.transform.localScale = m_Scale;

		VCPSimpleLight func = m_Entity.GetComponent<VCPSimpleLight>();
		if (func != null)
		{
			func.color = m_Color;
		}

		Renderer[] rs = m_Entity.GetComponentsInChildren<Renderer>(true);
		foreach (Renderer r in rs)
		{
			if (r is TrailRenderer)
				r.enabled = true;
			else if (r is ParticleRenderer)
				r.enabled = true;
			else if (r is ParticleSystemRenderer)
				r.enabled = true;
			else if (r is LineRenderer)
				r.enabled = true;
			else if (r is SpriteRenderer)
				r.enabled = true;
			else
				r.enabled = m_Visible;
		}
	}
}
