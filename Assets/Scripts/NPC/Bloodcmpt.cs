using UnityEngine;
using System.Collections;

public class Bloodcmpt : MonoBehaviour {

	[SerializeField] Transform ForeGround;
	[SerializeField] Transform BackGround;



	public void setForeScale(Vector3 scale)
	{
		ForeGround.localScale = scale;
	}

	public void setBackScale(Vector3 scale)
	{
		BackGround.localScale = scale;
	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
