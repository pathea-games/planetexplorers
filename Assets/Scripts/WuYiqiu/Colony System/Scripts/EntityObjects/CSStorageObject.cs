using UnityEngine;
using System.Collections;

public class CSStorageObject : CSCommonObject 
{
	
	public CSStorage m_Storage  { get { return m_Entity == null ? null : m_Entity as CSStorage;}}
	
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
