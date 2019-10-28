using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class Light_flares : MonoBehaviour
{
	public Light m_light;
	public float totalLifeTime = 0.0f;
	public float lightUp = 0.0f;
	public float lightDuration = 0.0f;
	public float decay = 0.0f;
	public float outBreak = 0.0f;
	public float extinguish = 1.0f;
	
	public float intensity0 = 0.0f;
	public float intensity1 = 0.0f;
	public float intensity2 = 0.0f;
	public float intensity3 = 0.0f;
	public float intensity4 = 0.0f;
	public float intensity5 = 0.0f;
	
	public float range0 = 0.0f;
	public float range1 = 0.0f;
	public float range2 = 0.0f;
	public float range3 = 0.0f;
	public float range4 = 0.0f;
	public float range5 = 0.0f;
	
	float elapsedTime = 0f;
	void Update ()
	{		
		elapsedTime += Time.deltaTime;
		float progress = elapsedTime / totalLifeTime;
		if(progress < 1.0f){
			if(progress <= lightUp){
				m_light.intensity = Mathf.Lerp(intensity0 , intensity1 , progress / lightUp);
				m_light.range = Mathf.Lerp(range0 , range1 , progress / lightUp);
			}
			else if(progress <= lightDuration){
				m_light.intensity = Mathf.Lerp(intensity1 , intensity2 , (progress - lightUp)/(lightDuration - lightUp));
				m_light.range = Mathf.Lerp(range1 , range2 , (progress - lightUp)/(lightDuration - lightUp));
			}
			else if(progress <= decay){
				m_light.intensity = Mathf.Lerp(intensity2 , intensity3 , (progress - lightDuration)/(decay - lightDuration));
				m_light.range = Mathf.Lerp(range2 , range3 , (progress - lightDuration)/(decay - lightDuration));
			}
			else if(progress <= outBreak){
				m_light.intensity = Mathf.Lerp(intensity3 , intensity4 , (progress - decay)/(outBreak - decay));
				m_light.range = Mathf.Lerp(range3 , range4 , (progress - decay)/(outBreak - decay));
			}
			else {
				m_light.intensity = Mathf.Lerp(intensity4 , intensity5 , (progress - outBreak)/(extinguish - outBreak));
				m_light.range = Mathf.Lerp(range4 , range5 , (progress - outBreak)/(extinguish - outBreak));
			}
		}
		else Destroy(gameObject);
	}
}


