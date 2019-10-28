//using UnityEngine;
//using System.Collections;
//using System.Collections.Generic;

//public class VehicleSkillRunner : DrivingSkillRunner
//{
//	protected void Update()
//	{
//		base.Update();

//		//if ( Driver == PlayerFactory.mMainPlayer )
//		//{
//		//    // Creation Driving Tips
//		//    VehicleController vc = 
//		//    ( Controller as VehicleController );

//		//    if (null == vc)
//		//        return;

//		//    float mass_factor = 1;
//		//    mass_factor = Mathf.Min( 1.2f, vc.m_Engine.m_PermitLoad / ( vc.m_Engine.m_PermitLoad * 0.5f + vc.m_Rigidbody.mass ) );
//		//    if ( mass_factor < 0.7f )
//		//        CreationDrivingTips.Instance.AddWarning("Your vehicle is overloaded !");

//		//    if ( vc.m_Engine.PowerSupplyPercent < 0.5f )
//		//        CreationDrivingTips.Instance.AddWarning("Lack of power !");

//		//    if ( vc.InWater )
//		//    {
//		//        m_CreationData.m_Fuel *= (1-Time.deltaTime*0.05f);
//		//        CreationDrivingTips.Instance.AddWarning("Being underwater will drain your fuel cell!");
//		//    }

//		//    int steering_tire_cnt = 0;
//		//    foreach ( VCPVehicleWheelFunc wheel in vc.m_Wheels )
//		//    {
//		//        if (null == wheel)
//		//            continue;
//		//        if ( Mathf.Abs(wheel.m_Property.m_MaxSteerAngle) > 5f )
//		//            steering_tire_cnt++;
//		//    }
//		//    if ( steering_tire_cnt < 1 )
//		//        CreationDrivingTips.Instance.AddWarning("Your vehicle cannot turn !");
//		//}
//	}

//	int _lastDmgTime = 0;
//	protected override void ApplyImpulseDamage ()
//	{
//		Vector3 im_ = Impulse;
//		im_.y *= 2.5f;
//		float impf = im_.magnitude / 20.0f;
		
//		// Apply HP Change
//		if ( impf > 16 )
//		{
//			if ( Time.frameCount - _lastDmgTime > 5 )
//			{
//				ApplyHpChange(null, impf*impf * 0.09f, 1, 0);
//				_lastDmgTime = Time.frameCount;
//			}
//		}
//	}

//	protected void FixedUpdate ()
//	{
//		base.FixedUpdate();
//	}
//}
