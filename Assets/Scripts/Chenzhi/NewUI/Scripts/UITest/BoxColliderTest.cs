using UnityEngine;
using System.Collections;

public class BoxColliderTest : MonoBehaviour 
{
	[SerializeField] BoxCollider mBgCollider;
	[SerializeField] BoxCollider mTopCollider;
	//bool isCover = false;
	//Bounds bounds;
	// Use this for initialization
	void Start () 
	{
		//bounds = mBgCollider.bounds;

	}

//	Rect rect 
//	{
//		get
//		{
//			float left = transform.position.x - transform.localScale.x/2;
//			float top = transform.position.y + transform.localScale.y/2;
//			float right = left +  transform.localScale.x;
//			float buttom = top -  transform.localScale.y;
//			return new Rect(left,top,right,height);
//		}
//	}

	// Update is called once per frame
	void Update () 
	{
//		Ray ray = new Ray(mBgCollider.transform.localPosition,Vector3.back);
//		Debug.DrawRay (mBgCollider.transform.position, Vector3.back * 10, Color.green);
//		float dis = 100f;
//		isCover = mTopCollider.bounds.IntersectRay(ray,out dis);
//		Debug.Log(isCover + " -------------------- ");

	}
}
