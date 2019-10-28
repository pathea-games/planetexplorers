using UnityEngine;
using System.Collections;

public class CSDwellingsObject : CSEntityObject 
{
	public CSDwellings	m_Dwellings { get { return m_Entity == null ? null : m_Entity as CSDwellings;}}
	
	public CSDwellingsInfo	m_Info;
	
	// Bed Transform
	public Transform[]  m_BedTrans;
    public Transform[]  m_BedEdgeTrans;
	
	#region ENTITY_OBJECT_FUNC
	
	#endregion
	
	// Use this for initialization
	new void Start () 
	{
		base.Start();
		m_Info = CSInfoMgr.m_DwellingsInfo;
	}
	
	// Update is called once per frame
	new void Update () 
	{
		base.Update();
	}
}
