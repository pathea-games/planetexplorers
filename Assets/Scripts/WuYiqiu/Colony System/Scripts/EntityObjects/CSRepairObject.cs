using UnityEngine;
using System.Collections;

public class CSRepairObject : CSCommonObject 
{
	public CSRepair		m_Repair	{ get { return m_Entity == null ? null : m_Entity as CSRepair;}}
	
	#region ENTITY_OBJECT_FUNC

	#endregion
	
	// Use this for initialization
	new void Start () 
	{
		base.Start();
	}
	
	// Update is called once per frame
	new void Update () 
	{
		base.Update();
	}
}
