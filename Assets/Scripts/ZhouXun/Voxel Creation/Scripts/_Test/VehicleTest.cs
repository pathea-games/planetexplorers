using UnityEngine;
using System.Collections.Generic;

public class VehicleTest : MonoBehaviour
{
	[SerializeField] Camera mMainCamera;
	[SerializeField] Transform ExportPosVehicle;
	[SerializeField] Transform ExportPosAircraft;
	[SerializeField] Transform ExportPosBoat;

	void Start ()
	{
		CreationMgr.Init();
	}

	void OpenEditor ()
	{
		VCEditor.Open();
	}

	CreationData new_creation;
	GameObject creation_obj;
	void ExportCreation ()
	{
		if (VCEditor.s_Scene == null || VCEditor.s_Scene.m_IsoData == null)
		{
			Debug.Log("VCEditor IsoData is null!");
			return;
		}
		
		new_creation = new CreationData ();
		new_creation.m_ObjectID = CreationMgr.QueryNewId();
		new_creation.m_RandomSeed = UnityEngine.Random.value;
		new_creation.m_Resource = VCEditor.s_Scene.m_IsoData.Export();
		new_creation.ReadRes();
		
		new_creation.GenCreationAttr(); // 计算属性
		
		if ( new_creation.SaveRes() )
		{
			new_creation.BuildPrefab();
			new_creation.Register();
			CreationMgr.AddCreation(new_creation);
		}
		else 
		{
			Debug.Log("Save creation resource file failed !");
			new_creation.Destroy();
			return;
		}

		GameObject obj = CreationMgr.InstantiateCreation(new_creation.m_ObjectID, 0, true, null);
		if (obj != null)
		{
			if (new_creation.m_Attribute.m_Type == ECreation.Vehicle)
				obj.transform.localPosition = ExportPosVehicle.position;
			if (new_creation.m_Attribute.m_Type == ECreation.Aircraft)
				obj.transform.localPosition = ExportPosAircraft.position;
			if (new_creation.m_Attribute.m_Type == ECreation.Boat)
				obj.transform.localPosition = ExportPosBoat.position;
			
			ZXCameraCtrl ZXCamera = mMainCamera.GetComponent<ZXCameraCtrl>();
			ZXCamera.Following = obj.transform;
		}
		creation_obj = obj;
	}

	void OnGUI ()
	{
		if (GUI.Button(new Rect(50,50,100,30), "Clear Monos"))
		{
			MonoBehaviour[] monos = creation_obj.GetComponentsInChildren<MonoBehaviour>(true);
			foreach (MonoBehaviour mono in monos)
			{
				if (mono is CreationMeshLoader)
					continue;
				if (mono is VCMeshMgr)
					continue;
				MonoBehaviour.Destroy(mono);
			}
			foreach (MonoBehaviour mono in monos)
			{
				if (mono is CreationMeshLoader)
					continue;
				if (mono is VCMeshMgr)
					continue;
				MonoBehaviour.Destroy(mono);
			}
		}
		if (GUI.Button(new Rect(50,100,100,30), "Init Test"))
		{
			VehiclePhysics.VehicleEngine engine = creation_obj.AddComponent<VehiclePhysics.VehicleEngine>();
			creation_obj.AddComponent<VehiclePhysics.VehicleDemo>().engine = engine;

			WheelCollider[] wcs = creation_obj.GetComponentsInChildren<WheelCollider>();

			foreach (WheelCollider wc in wcs)
				wc.enabled = true;
			
			engine.wheels = new VehiclePhysics.VehicleWheel[wcs.Length];
			for (int i = 0; i < engine.wheels.Length; ++i)
			{
				engine.wheels[i] = new VehiclePhysics.VehicleWheel ();
				engine.wheels[i].wheel = wcs[i];
				engine.wheels[i].model = wcs[i].transform.parent.GetChild(3);
				engine.wheels[i].maxMotorTorque = 5000;
				engine.wheels[i].staticBrakeTorque = 1200;
				engine.wheels[i].dynamicBrakeTorque = 400;
				engine.wheels[i].footBrakeTorque = 6000;
				engine.wheels[i].handBrakeTorque = 10000000;
			}
			engine.maxMotorTorque = 24000;
			engine.maxPower = 3000;
			engine.realMass = 15000;

			Transform[] ts = creation_obj.GetComponentsInChildren<Transform>(true);
			foreach (Transform t in ts)
			{
				if (t.gameObject.name == "MCollider")
					GameObject.Destroy(t.gameObject);
			}

			creation_obj.GetComponent<Rigidbody>().mass = 15000;
			creation_obj.GetComponent<Rigidbody>().isKinematic = false;
		}
	}
}