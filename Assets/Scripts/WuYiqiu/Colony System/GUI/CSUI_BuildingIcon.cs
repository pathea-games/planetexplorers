using UnityEngine;
using System.Collections;

public class CSUI_BuildingIcon : MonoBehaviour 
{
	[SerializeField]
	private UILabel		m_Description;
	
	public string 		Description		{ get{ return m_Description.text; } set { m_Description.text = value; } }
	
	[SerializeField]
	private UISlicedSprite	m_Icon;
	
	public string  IconName  		{ get{ return m_Icon.spriteName; } set { m_Icon.spriteName = value; } }
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
