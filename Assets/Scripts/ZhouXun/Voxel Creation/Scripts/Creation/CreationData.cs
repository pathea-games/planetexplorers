using UnityEngine;
using System.Collections;
using System.Timers;

//
// CreationData class saves a creation INSTANCE object data
//

// Structure of CreationData
//
public partial class CreationData
{
	// Object ID
	public int m_ObjectID;	// Server gen
	
	// Data source
	public byte[] m_Resource;	// Client send to server
	public float m_RandomSeed;	// Server gen
	
	// This part calculated from 'Data source'
	public VCIsoData m_IsoData;
	public ulong m_HashCode;	// CRC64
	public string HashString { get { return m_HashCode.ToString("X").PadLeft(16, '0'); } }
	public CreationAttr m_Attribute;
	
	// Runtime data
	//public float m_Hp;
	//public float m_Fuel;

	System.DateTime m_LastUseTime;

	public void UpdateUseTime()
	{
		m_LastUseTime = System.DateTime.Now;
	}

	public bool TooLongNoUse()
	{
		System.TimeSpan sub = System.DateTime.Now.Subtract(m_LastUseTime); 
		if(sub.TotalSeconds > 300)
			return true;
		else
			return false;
	}

	// Destruction
	public void Destroy ()
	{
		m_ObjectID = 0;
		m_Resource = null;
		m_RandomSeed = 0;
		m_HashCode = (ulong)(0);
		if ( m_IsoData != null )
		{
			m_IsoData.Destroy();
			m_IsoData = null;
		}
		m_Attribute = null;
		DestroyPrefab();
	}
}
