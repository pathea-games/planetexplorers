using UnityEngine;
using System.Collections;


public class BillBoard : MonoBehaviour {

    public Camera cameraToLookAt;
	// Use this for initialization
	void Start () {
        cameraToLookAt = Camera.main;
	}
	
	// Update is called once per frame
	void Update () {
        //Vector3 v = cameraToLookAt.transform.position - transform.position;
        //v.x = v.z = 0.0f;
        //transform.LookAt(cameraToLookAt.transform.position - v);
		this.transform.rotation = PETools.PEUtil.MainCamTransform.rotation;
	}
}
