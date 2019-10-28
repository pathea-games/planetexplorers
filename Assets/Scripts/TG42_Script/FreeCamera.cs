using UnityEngine;
using System.Collections;

//自由摄像机
public class FreeCamera : MonoBehaviour 
{
	public float m_camSpeed = 20.0f;
    public float m_accRatio = 2.0f;
	public float m_sensitivityX = 5.0f;
	public float m_sensitivityY = 5.0f;
	
	float mRotX = 0.0f;
    float mRotY = 0.0f;

	static bool _freeCameraMode = false;
	public static bool FreeCameraMode{ get{ return _freeCameraMode; } }

	// Use this for initialization
	void Start () 
	{
		mRotX = transform.eulerAngles.y;
		mRotY = -transform.eulerAngles.x;
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

        //wsad控制摄像机移动
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

        //鼠标控制方向
        if(Input.GetKey(KeyCode.Mouse1))
        {
            mRotX += Input.GetAxis("Mouse X") * m_sensitivityX;
            mRotY += Input.GetAxis("Mouse Y") * m_sensitivityY;
			if(mRotY > 89.9f)
				mRotY = 89.9f;
			if(mRotY < -89.9f)
				mRotY = -89.9f;
            transform.localEulerAngles = new Vector3(-mRotY, mRotX, 0);
        }

        if (Input.GetKey(KeyCode.Mouse2))
        {
            Vector3 _cam = transform.localPosition;
            _cam += transform.right * Input.GetAxis("Mouse X") * 10.0f;
            _cam += transform.forward * Input.GetAxis("Mouse Y") * 10.0f;
            transform.localPosition = _cam;
        }
    }

	public static void SetFreeCameraMode()
	{
		if (SceneMan.self == null) {
			Debug.LogError("[SetFreeCameraMode]Invalid scene");
			return;
		}
		_freeCameraMode = !_freeCameraMode;
		if (_freeCameraMode) {
			CameraForge.CameraController camCtrlr = Camera.main.GetComponent<CameraForge.CameraController> ();
			if (camCtrlr != null) {
				camCtrlr.enabled = false;
			}
			FreeCamera freeCam = Camera.main.GetComponent<FreeCamera> ();
			if (freeCam != null) {
				camCtrlr.enabled = true;
			} else {
				Camera.main.gameObject.AddComponent<FreeCamera> ();
			}
		} else {
			FreeCamera freeCam = Camera.main.GetComponent<FreeCamera> ();
			if (freeCam != null) {
				GameObject.Destroy(freeCam);
			}
			
			CameraForge.CameraController camCtrlr = Camera.main.GetComponent<CameraForge.CameraController> ();
			if (camCtrlr != null) {
				camCtrlr.enabled = true;
			}
		}
		Debug.LogError("[SetFreeCameraMode]FreeCameraMode:"+_freeCameraMode);
		return;
	}
}
