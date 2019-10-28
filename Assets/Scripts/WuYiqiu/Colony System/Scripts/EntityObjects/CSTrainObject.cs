using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


public class CSTrainObject:CSCommonObject
{
    public CSTraining m_Train { get { return m_Entity == null ? null : m_Entity as CSTraining; } }
	
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
