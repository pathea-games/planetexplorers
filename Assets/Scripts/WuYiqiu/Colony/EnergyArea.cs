using UnityEngine;
using System.Collections;

public class EnergyArea : MonoBehaviour 
{
	[SerializeField]
	private EnergyAreaHandler _handler;
	
	[SerializeField]
	private Projector	_projector;
	
	// Attribute
	public float radius	   		{ get { return _projector.orthographicSize; }  set { _projector.orthographicSize = value; } }
	public float energyScale	{ get { return _handler.m_EnergyScale; }  set { _handler.m_EnergyScale = value; } }	
	
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
