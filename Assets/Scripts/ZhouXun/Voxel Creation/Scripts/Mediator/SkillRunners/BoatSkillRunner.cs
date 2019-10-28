//using UnityEngine;
//using System.Collections;
//using System.Collections.Generic;

//public class BoatSkillRunner : DrivingSkillRunner
//{
//	protected void Update ()
//	{
//		base.Update();

//		// UI hotkey Control
////        if ( Driver == PlayerFactory.mMainPlayer && GameGui_N.Instance != null )
////        {
////            BoatController bc = 
////            ( Controller as BoatController );

////            if (null == bc)
////                return;

////            // Creation Driving Tips
////            float motivation = 0;
////            foreach ( VCPShipPropellerFunc propeller in bc.m_Propellers )
////            {
////                if (null == propeller)
////                    continue;

////                motivation += (propeller.m_Property.m_MaxPower * propeller.m_Property.m_Eta * propeller.transform.localScale.x * propeller.transform.localScale.z);
////            }
//////			if ( motivation < bc.m_Rigidbody.mass * 0.15f )
//////				CreationDrivingTips.Instance.AddWarning("Lack of thrust !");

////            if ( bc.m_Rigidbody.velocity.y < -5f )
////                CreationDrivingTips.Instance.AddWarning("SINK WARNING!");
////        }
//	}

//	int _lastDmgTime = 0;
//	protected override void ApplyImpulseDamage ()
//	{
//		Vector3 im_ = Impulse;
//		im_.y *= 5;
//		float impf = im_.magnitude / 20.0f;
		
//		// Apply HP Change
//		if ( impf > 15 )
//		{
//			if ( Time.frameCount - _lastDmgTime > 1 )
//			{
//				ApplyHpChange(null, Mathf.Pow(impf, 2.8f) * 0.018f, 1, 0);
//				_lastDmgTime = Time.frameCount;
//			}
//		}
//	}

//	protected void FixedUpdate ()
//	{
//		base.FixedUpdate();
//	}
//}
