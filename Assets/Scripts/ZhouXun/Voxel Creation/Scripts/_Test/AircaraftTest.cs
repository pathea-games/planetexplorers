using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AircaraftTest : MonoBehaviour 
{

	[SerializeField] public Rigidbody mRigidbody;
	[SerializeField] Transform mFwordLeft;
	[SerializeField] Transform mFwordRight;
	[SerializeField] Transform mBackLeft;
	[SerializeField] Transform mBackRight;
	[SerializeField] float mGrivate = 9.8f;
	int UpLevel = 0;
	float speed;

	[SerializeField] float mUpForce_k = 0.2f;
	[SerializeField] float OtherForceSize = 10.0f; 

	Vector3 MassCenter{get{return mRigidbody.centerOfMass;}} 
	float Grivate{get{return mRigidbody.mass * mGrivate;}}
	
	public bool m_ForwardInput = false;
	public bool m_BackwardInput = false;
	public bool m_LeftInput = false;
	public bool m_RightInput = false;
	public bool m_UpInput = false;
	public bool m_DownInput = false;

	public List<AircarftPutForce> forceList;
	// Use this for initialization
	void Start () 
	{
		forceList.AddRange(GetComponentsInChildren<AircarftPutForce>());
	}



	// Update is called once per frame
	void Update () 
	{
		m_ForwardInput = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow);
		m_BackwardInput = Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow);
		m_LeftInput = Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow);
		m_RightInput = Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow);
		m_UpInput = Input.GetKey(KeyCode.Space);
		m_DownInput = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);

		
		if ( Input.GetKey(KeyCode.Space) && Time.frameCount % 6 == 0 && UpLevel < 10 )
			UpLevel ++;
		if ( (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)) && Time.frameCount % 6 == 0 && UpLevel > -10 )
			UpLevel --;
	}


	void FixedUpdate()
	{
		//加重力 加到重心
		mRigidbody.AddForce(new Vector3(0,-Grivate,0));
		//up faorce
		float up_k = (1 + UpLevel * mUpForce_k); 
		mRigidbody.AddForce(new Vector3(0,Grivate *up_k,0));
		//Vector3.Lerp(gameObject.transform.position, mRigidbody.worldCenterOfMass, 0.9f)

		if (m_ForwardInput)
		{
			mRigidbody.AddForceAtPosition(BackLeftForce,mBackLeft.position);
			mRigidbody.AddForceAtPosition(BackRightForce,mBackRight.position);
		}

		if (m_BackwardInput)
		{
			mRigidbody.AddForceAtPosition(FwordLeftForce,mFwordLeft.position);
			mRigidbody.AddForceAtPosition(FwordRightForce,mFwordRight.position);
		}

		if (m_LeftInput)
		{
			mRigidbody.AddForceAtPosition(FwordRightForce,mFwordRight.position);
			mRigidbody.AddForceAtPosition(BackLeftForce,mBackLeft.position);
		}

		if (m_RightInput)
		{
			mRigidbody.AddForceAtPosition(FwordLeftForce,mFwordLeft.position);
			mRigidbody.AddForceAtPosition(BackRightForce,mBackRight.position);
		}

	}

	Vector3 FwordLeftForce
	{
		get { return mFwordLeft.up * OtherForceSize;}
	}

	Vector3 FwordRightForce
	{
		get { return mFwordRight.up * OtherForceSize;}
	}

	Vector3 BackLeftForce
	{
		get { return mBackLeft.up * OtherForceSize;}
	}

	Vector3 BackRightForce
	{
		get { return mBackRight.up * OtherForceSize;}
	}

	void OnGUI ()
	{
		GUI.Label(new Rect(50,80,500,20), "Level: " + UpLevel.ToString());
	}

}
