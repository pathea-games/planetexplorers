using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LightControl : MonoBehaviour 
{
	//public float TurnOnHour = 20;
	//public float TurnOffHour = 6;
	static readonly Color eCol = new Color (0.388f, 0.749f, 1, 1);
	bool isO;
	void Start()
	{
		if (GameTime.Timer.CycleInDay > 0f)
			TurnOff ();
		else
			TurnOn ();
	}
	void Update()
	{
		if(GameTime.Timer.CycleInDay > 0f)
		{
			if(isO)			TurnOff();
		}
		else
		{
			if(!isO)		TurnOn();
		}
	}

	void TurnOn()
	{
		isO = true;
		ParticleSystem [] pss = gameObject.GetComponentsInChildren<ParticleSystem>(true);
		foreach ( ParticleSystem ps in pss )
		{
			ps.gameObject.SetActive(true);
		}
						
		Light [] lights = gameObject.GetComponentsInChildren<Light>();
		//int layerMask = (1<<gameObject.layer);
		foreach ( Light l in lights )
		{
			//l.cullingMask &= ~layerMask;
			l.enabled = true;
		}
		if (GetComponent<MeshRenderer> () != null && GetComponent<MeshRenderer> ().materials.Length > 1) {
			Material mat = GetComponent<MeshRenderer> ().materials[1];
			mat.SetColor("_EmissionColor", 6*eCol);

			//mat.EnableKeyword ("_EMISSION");
		}
		if (GetComponent<SkinnedMeshRenderer> () != null && GetComponent<SkinnedMeshRenderer> ().materials.Length > 0) {
			Material skmat = GetComponent<SkinnedMeshRenderer> ().materials[0];			
			skmat.SetColor("_EmissionColor", 6*eCol);	
		}
	}

	void TurnOff()
	{
		isO = false;
		ParticleSystem [] pss = gameObject.GetComponentsInChildren<ParticleSystem>(true);
		foreach ( ParticleSystem ps in pss )
		{
			ps.gameObject.SetActive(false);
		}
		
		Light [] lights = gameObject.GetComponentsInChildren<Light>();
		foreach ( Light l in lights )
		{
			l.enabled = false;
		}
		if (GetComponent<MeshRenderer> () != null && GetComponent<MeshRenderer> ().materials.Length > 1) {
			Material mat = GetComponent<MeshRenderer> ().materials[1];			
			mat.SetColor("_EmissionColor", 0*eCol);
			//mat.EnableKeyword ("_EMISSION");
		}
		if (GetComponent<SkinnedMeshRenderer> () != null && GetComponent<SkinnedMeshRenderer> ().materials.Length > 0) {
			Material skmat = GetComponent<SkinnedMeshRenderer> ().materials[0];			
			skmat.SetColor("_EmissionColor", 0*eCol);
		}
	}
}
//public class LightControl : MonoBehaviour 
//{
//	public float TurnOnHour = 20;
//	public float TurnOffHour = 6;
//	
//	// Use this for initialization
//	void Start ()
//	{
//	
//	}
//	
//	// Update is called once per frame
//	void Update ()
//	{
//		if ( TurnOnHour > 260000 )
//			TurnOnHour = 0;
//		if ( TurnOnHour < -260000 )
//			TurnOnHour = 0;
//		if ( TurnOffHour > 260000 )
//			TurnOffHour = 0;
//		if ( TurnOffHour < -260000 )
//			TurnOffHour = 0;
//		while ( TurnOnHour > 26 )
//			TurnOnHour -= 26;
//		while ( TurnOnHour < 0 )
//			TurnOnHour += 26;
//		while ( TurnOffHour > 26 )
//			TurnOffHour -= 26;
//		while ( TurnOffHour < 0 )
//			TurnOffHour += 26;
//		
//		if ( GameUI.Instance == null )
//			return;
////		if ( MainRightGui_N.Instance == null )
////			return;
////		float hour = (float)(GameGui_N.Instance.mMainRightGui.GameTime % 93600) / (float)(3600);
//		
////		float on1, off1, on2, off2;
////		on1 = TurnOnHour;
////		off1 = TurnOffHour;
////		if ( off1 < on1 )
////			off1 += 26;
////		on2 = on1 - 26;
////		off2 = off1 - 26;
//				
////		if ( hour > on1 && hour < off1 || 
////			 hour > on2 && hour < off2 )
//		if (GameConfig.IsNight) //QiYun Ran
//		{
//			if ( GetComponent<ParticleSystem>() != null )
//				gameObject.SetActive(true);
//			ParticleSystem [] pss = gameObject.GetComponentsInChildren<ParticleSystem>(true);
//			foreach ( ParticleSystem ps in pss )
//			{
//				ps.gameObject.SetActive(true);
//			}
//			
//			if ( GetComponent<Light>() != null )
//				GetComponent<Light>().enabled = true;
//			Light [] lights = gameObject.GetComponentsInChildren<Light>();
//			int layerMask = (1<<gameObject.layer);
//			foreach ( Light l in lights )
//			{
//				l.cullingMask &= ~layerMask;
//				l.enabled = true;
//			}
//		}
//		else
//		{
//			if ( GetComponent<ParticleSystem>() != null )
//				gameObject.SetActive(false);
//			ParticleSystem [] pss = gameObject.GetComponentsInChildren<ParticleSystem>(true);
//			foreach ( ParticleSystem ps in pss )
//			{
//				ps.gameObject.SetActive(false);
//			}
//			
//			if ( GetComponent<Light>() != null )
//				GetComponent<Light>().enabled = false;
//			Light [] lights = gameObject.GetComponentsInChildren<Light>();
//			foreach ( Light l in lights )
//			{
//				l.enabled = false;
//			}
//		}
//	}
//}
