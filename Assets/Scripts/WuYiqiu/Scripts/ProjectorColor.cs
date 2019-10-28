using UnityEngine;
using System.Collections;

public class ProjectorColor : MonoBehaviour 
{
	
	[SerializeField]
	private ProjectorColorHandler _hanlder;
	
	[SerializeField]
	private Projector	_projector;
	
	public void FlareColor()
	{
		_hanlder.gameObject.SetActive(true);
		_hanlder.flare = true;
	}
	
	public void SetSize (float size,  float height)
	{
		_projector.orthographicSize = size;
		_projector.transform.localPosition = new Vector3(0f, height, 0f);
	}
	
	// Use this for initialization
	void Start () 
	{
	
	}
	
	// Update is called once per frame
	void Update () 
	{
	
	}
}
