using UnityEngine;
using System.Collections;

public class UIOnScrollMouse : MonoBehaviour 
{
//	public delegate void Notify();
//	public static bool mouseOnScroll = false;
//	
	//private BoxCollider mBoxClollider;

	// Use this for initialization
	void Start () 
	{
		//mBoxClollider = this.gameObject.GetComponent<BoxCollider>();
	}
//	// Update is called once per frame
//	void LateUpdate() 
//	{
//		UpdateMouseOnClider();
//	}
//
//	void OnDisable ()
//	{
//		mouseOnScroll = false;
//	}
//
//	void UpdateMouseOnClider()
//	{
//		if (mBoxClollider == null || UICamera.currentCamera == null)
//		{
//			return;
//		}
//
//		Ray ray = UICamera.currentCamera.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
//		RaycastHit rayHit;
//
//		mouseOnScroll = mBoxClollider.Raycast(ray,out rayHit,100);
//	}
}
