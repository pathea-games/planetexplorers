using UnityEngine;
using System.Collections;

public class AircarftPutForce : MonoBehaviour 
{
	[SerializeField] float mMaxPower;
	public AircaraftTest mAircaraft;


	// Use this for initialization
	void Start () 
	{
	
	}
	
	// Update is called once per frame
	void Update () 
	{
	
	}

	float currutPower = 0;
	void FixedUpdate()
	{
		if (mAircaraft != null)
		{
			// 计算 飞机上方向夹角  取值0到1
			//float updot = Mathf.Clamp01(Vector3.Dot(mAircaraft.transform.up, transform.up));
			// 移动力方向
			//float movedot = Mathf.Clamp01(Vector3.Dot(xMove + zMove, transform.up));
			// 根据位置旋转 计算力大小；
			//currutPower = 


			Vector3 force = transform.up * currutPower;
			// 加力位置 插值
			Vector3 point = Vector3.Lerp(transform.position, mAircaraft.mRigidbody.worldCenterOfMass, 0.97f);
			point.y = mAircaraft.mRigidbody.worldCenterOfMass.y;
			mAircaraft.mRigidbody.AddForceAtPosition(force,point);
		}

	}

	// 移动方向单位向量
	Vector3 zMove
	{
		get
		{
			if (mAircaraft.m_ForwardInput)
				return 	mAircaraft.transform.forward;
			else if (mAircaraft.m_BackwardInput)
				return  -mAircaraft.transform.forward;
			else
				return Vector3.zero;;
		}
	}

	//转动方向单位向量
	Vector3 xMove 
	{
		get
		{
			//发力点与中心方向
			Vector3 v = transform.position - mAircaraft.mRigidbody.worldCenterOfMass;
			if (mAircaraft.m_LeftInput)
				return 	-Vector3.Cross(mAircaraft.transform.up, v).normalized;
			else if (mAircaraft.m_RightInput)
				return  Vector3.Cross(mAircaraft.transform.up, v).normalized;
			else
				return Vector3.zero;;
		}
	}
	

}
