//using UnityEngine;
//using System.Collections;
//using System.IO;
//using System.Collections.Generic;
//using System;


//public abstract class CreationItemController : CreationController
//{

//	private  VCPObjectFunc m_MouseOverPart;
//	// Parts
//	public List<VCPBedFunc> m_Beds;
//	public List<VCPLightFunc> m_Lights;
//	public VCPPivotFunc m_Pivot;
//	public bool m_OpenLights = false;
//	public bool m_OpenPivot = false;
//	public int  m_ItemState;

//	/*
//	public override void Init (CreationData crd, int itemInstanceId)
//	{
//		m_CreationData = crd;
//		m_Active = true;

//		LoadParts<VCPBedFunc>(ref m_Beds);
//		LoadParts<VCPLightFunc>(ref m_Lights);
//		LoadParts<VCPPivotFunc>(ref m_Pivot);

//		foreach (VCPBedFunc bed in m_Beds)
//		{
//			bed.m_Controller = this;
//			SleepObject obj = bed.gameObject.AddComponent<SleepObject>();
//			obj.mSleepPos = bed.m_PivotPoint;
//		}

//		foreach (VCPLightFunc light in m_Lights)
//		{
//			light.m_Controller = this;
//		}

//		if (m_Pivot != null)
//			m_Pivot.m_Controller = this;

//		LoadItemState();
//	}

//	protected override void Update()
//	{
//		UpdateItemState();
//	}

//	void LoadItemState()
//	{
//		m_ItemState = Convert.ToInt32(m_CreationData.m_Fuel);
//		m_OpenLights = (1 &  m_ItemState) == 1 ;
//		foreach (VCPLightFunc light in m_Lights)
//		{
//			light.m_OpenLight= m_OpenLights;
//		}
//		m_OpenPivot = (1 &  (m_ItemState>>1) ) == 1 ;
//	}


//	void UpdateItemState()
//	{
//		m_ItemState = (m_OpenLights ? 1 : 0);
//		m_ItemState += (m_OpenPivot ? 1 : 0) << 1;
//		m_CreationData.m_Fuel = m_ItemState;
//	}

//	public VCPartFunc GetMouseOverPart()
//	{
//		m_MouseOverPart = null;

//		foreach (VCPBedFunc bed in m_Beds)
//		{
//			if (bed.m_Selected)
//			{
//				if (m_MouseOverPart != null)
//				{
//					if (bed.m_Distence < m_MouseOverPart.m_Distence)
//						m_MouseOverPart = bed;
//				}
//				else 
//					m_MouseOverPart = bed;
//			}
//		}

//		foreach (VCPLightFunc light in m_Lights)
//		{
//			if (light.m_Selected)
//			{
//				if (m_MouseOverPart != null)
//				{
//					if (light.m_Distence < m_MouseOverPart.m_Distence)
//						m_MouseOverPart = light;
//				}
//				else 
//					m_MouseOverPart = light;
//			}
//		}

//		return m_MouseOverPart;
//	}


//	public override byte[] GetState ()
//	{
//		byte[] retval = null;
//		using ( MemoryStream ms = new MemoryStream () )
//		{
//			BinaryWriter w = new BinaryWriter (ms);
//			// 12 B
//			w.Write(transform.position.x);
//			w.Write(transform.position.y);
//			w.Write(transform.position.z);
//			// 4 B
//			int euler = VCUtils.CompressEulerAngle(transform.eulerAngles);
//			w.Write(euler);
//			// 1 B
//			w.Write(m_OpenLights);
//			w.Write(m_OpenPivot);
//			w.Close();
//			retval = ms.ToArray();
//			ms.Close();
//		}
//		return retval;
//	}
//	public override void SetState (byte[] buffer)
//	{
//		using ( MemoryStream ms = new MemoryStream (buffer) )
//		{
//			BinaryReader r = new BinaryReader (ms);
//			float px = r.ReadSingle();
//			float py = r.ReadSingle();
//			float pz = r.ReadSingle();
//			int euler = r.ReadInt32();
//			m_OpenLights = r.ReadBoolean();
//			m_OpenPivot = r.ReadBoolean();

//			transform.position = new Vector3(px, py, pz);
//			transform.eulerAngles = VCUtils.UncompressEulerAngle(euler);

//			r.Close();
//			ms.Close();
//		}
//	}
//	*/
//}
