using UnityEngine;
using System.Collections;

public class RCGen : MonoBehaviour
{
	public int type = 1;
	// Use this for initialization
	void Start ()
	{

	}
	
	// Update is called once per frame
	void Update ()
	{
		RoadSystem.DataSource.AlterRoadCell(transform.position, new RoadCell(type));
	}
}
