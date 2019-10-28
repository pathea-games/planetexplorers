using UnityEngine;
using System.Collections;

public class MoveClass : MonoBehaviour 
{
	//[SerializeField] UIButton btnTest;
	// Use this for initialization
	void Start () {
		//btnTest.enabled = false;
	}


	bool bMove = false;
	// Update is called once per frame
	void Update () 
	{
		if (bMove)
		{
			Vector3 oldPos = gameObject.transform.position;
			gameObject.transform.position = new Vector3(oldPos.x + 2,oldPos.y,oldPos.z); 
		}


	}


	public void StarMove()
	{
		Debug.Log ("Btn On Click!");
		bMove = !bMove;
	}
}
