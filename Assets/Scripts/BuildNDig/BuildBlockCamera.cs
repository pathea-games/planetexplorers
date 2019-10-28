using UnityEngine;
using System.Collections;

public class BuildBlockCamera : MonoBehaviour 
{
	public float m_camSpeed = 20.0f;
	public float m_accRatio = 2.0f;
	public float m_sensitivityX = 5.0f;
	public float m_sensitivityY = 5.0f;
	
	float mRotX = 0.0f;
	float mRotY = 0.0f;

	Vector3 	DefaultPos;
	Quaternion	DefaultRot;
	
	// Use this for initialization
	void Start () 
	{
		mRotX = transform.eulerAngles.y;
		mRotY = -transform.eulerAngles.x;

		DefaultPos = transform.position;
		DefaultRot = transform.rotation;
	}
	
	// Update is called once per frame
	void Update()
	{
		SceneMan.SetObserverTransform(this.transform);
		Debug.DrawLine(transform.position, transform.position + transform.forward * 100.0f);
		float _curSpeed = m_camSpeed;
		
		if (Input.GetKey(KeyCode.LeftShift))
			_curSpeed /= m_accRatio;
		if (Input.GetKey(KeyCode.LeftAlt))
			_curSpeed *= m_accRatio;
		
		//wsad¿ØÖÆÉãÏñ»úÒÆ¶¯
		if (Input.GetKey(KeyCode.W))
			transform.position += transform.forward * _curSpeed * Time.deltaTime;
		
		if (Input.GetKey(KeyCode.S))
			transform.position += -transform.forward * _curSpeed * Time.deltaTime;
		
		if (Input.GetKey(KeyCode.A))
			transform.position += -transform.right * _curSpeed * Time.deltaTime;
		
		if (Input.GetKey(KeyCode.D))
			transform.position += transform.right * _curSpeed * Time.deltaTime;
		
		if (Input.GetKey(KeyCode.Q))
			transform.position += transform.up * _curSpeed * Time.deltaTime;
		
		if (Input.GetKey(KeyCode.Z))
			transform.position += -transform.up * _curSpeed * Time.deltaTime;

		if (Input.GetKey(KeyCode.R))
		{
			transform.position = DefaultPos;
			transform.rotation = DefaultRot;
		}
		
		//Êó±ê¿ØÖÆ·½Ïò
		if(Input.GetKey(KeyCode.Mouse1))
		{
			Ray cameraRay = GetComponent<Camera>().ScreenPointToRay(new Vector3(Screen.width/2f,Screen.height/2f,0));
			RaycastHit hitInfo;
			if(Physics.Raycast(cameraRay, out hitInfo, 1000f))
			{
				mRotX += Input.GetAxis("Mouse X") * m_sensitivityX;
				mRotY += Input.GetAxis("Mouse Y") * m_sensitivityY;
				mRotY = Mathf.Clamp(mRotY, -89.9f, -5f);
				transform.localEulerAngles = new Vector3(-mRotY, mRotX, 0);
				transform.position = hitInfo.point - Vector3.Distance(GetComponent<Camera>().transform.position, hitInfo.point) * transform.forward;
			}
			else
			{
				cameraRay = GetComponent<Camera>().ScreenPointToRay(new Vector3(Screen.width/2f,Screen.height/3f,0));
				if(Physics.Raycast(cameraRay, out hitInfo, 1000f))
				{
					mRotX += Input.GetAxis("Mouse X") * m_sensitivityX;
					mRotY += Input.GetAxis("Mouse Y") * m_sensitivityY;
					mRotY = Mathf.Clamp(mRotY, -89.9f, -5f);
					transform.localEulerAngles = new Vector3(-mRotY, mRotX, 0);
					transform.position = hitInfo.point - Vector3.Distance(GetComponent<Camera>().transform.position, hitInfo.point) * transform.forward;
				}
			}
		}
		
		if (Input.GetKey(KeyCode.Mouse2))
		{
			Vector3 _cam = transform.localPosition;
			_cam += transform.right * Input.GetAxis("Mouse X") * 10.0f;
			_cam += transform.forward * Input.GetAxis("Mouse Y") * 10.0f;
			transform.localPosition = _cam;
		}
	}
}
