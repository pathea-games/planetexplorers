using UnityEngine;
using System.Collections;

public class WaterEffectHelper : MonoBehaviour 
{
	public GameObject m_UnderWaterEffect;
	public GameObject m_WaterSurfaceEffect;
	public GameObject m_AboveWaterEffect;
	public float m_SurfaceHeight = 1f;

	// Use this for initialization
	void Start () 
	{
		if(PETools.PEUtil.CheckPositionUnderWater(transform.position))
		{
			RaycastHit hitInfo;
			if(Physics.Raycast(transform.position + m_SurfaceHeight * Vector3.up, Vector3.down, out hitInfo, 2f, PEConfig.WaterLayer))
			{
				m_UnderWaterEffect.SetActive(false);
				m_WaterSurfaceEffect.SetActive(true);
				m_AboveWaterEffect.SetActive(false);
			}
			else 
			{
				m_UnderWaterEffect.SetActive(true);
				m_WaterSurfaceEffect.SetActive(false);
				m_AboveWaterEffect.SetActive(false);
			}
		}
		else
		{
			m_UnderWaterEffect.SetActive(false);
			m_WaterSurfaceEffect.SetActive(false);
			m_AboveWaterEffect.SetActive(true);
		}
	}

}
