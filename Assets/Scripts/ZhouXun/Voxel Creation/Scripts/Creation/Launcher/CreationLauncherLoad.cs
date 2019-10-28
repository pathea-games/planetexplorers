//using UnityEngine;
//using System.Collections;
//using System.Collections.Generic;

//public class CreationLauncherLoad : MonoBehaviour
//{
//	public CreationController m_Target;
	
//	void Awake ()
//	{
////		CreationMgr.Init();
////
////		CreationLauncher.m_CreationData = new CreationData ();
////		CreationLauncher.m_CreationData.m_Attribute = new CreationAttr ();
////		CreationLauncher.m_CreationData.m_Attribute.m_Type = ECreation.Vehicle;
////		CreationLauncher.m_CreationData.m_Attribute.m_CenterOfMass = Vector3.down * 1;
////		CreationLauncher.m_CreationData.m_Attribute.m_Weight = 150000;
////		CreationLauncher.m_CreationData.m_Attribute.m_MaxFuel = 70000;
////		CreationLauncher.m_CreationData.m_Fuel = 70000;
////		CreationLauncher.m_CreationData.m_Attribute.m_FluidDisplacement = new List<VolumePoint> ();
////		for ( float z = -7; z < 7.01f; z += 1 )
////		{
////			for ( float x = -2; x < 2.01f; x += 1 )
////			{
////				for ( float y = -2; y < 2.01f; y += 1 )
////				{
////					float nv = 0;
////					if ( Mathf.Abs(z) > 4 || Mathf.Abs(x) > 1 )
////						nv = 1;
////					CreationLauncher.m_CreationData.m_Attribute.m_FluidDisplacement.Add(new VolumePoint (new Vector3(x,y,z), 1, nv));
////				}
////			}
////		}
////		m_Target.Init(CreationLauncher.m_CreationData);
//	}
	
//	void Start ()
//	{
		
//	}
//}
