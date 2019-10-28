using UnityEngine;
using System.Collections;

public class CSFarmObject : CSCommonObject  
{

	public CSFarm m_Farm  { get { return m_Entity == null ? null : m_Entity as CSFarm;}}

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
