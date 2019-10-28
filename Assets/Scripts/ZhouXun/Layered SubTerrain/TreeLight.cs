using UnityEngine;
using System.Collections;

public class TreeLight : MonoBehaviour
{
	public float m_MaxIntensity = 1;
	public float m_FadeDist = 64;
	public float m_Falloff = 8;
	public Color m_MaxFlareColor = Color.black;
	LensFlare m_LensFlare = null;
	Light m_Light = null;

	// Use this for initialization
	void Start ()
	{
		m_Light = GetComponent<Light> ();
		m_MaxIntensity = m_Light.intensity;
		m_Light.enabled = false;

		m_LensFlare = GetComponent<LensFlare>();
		if (m_LensFlare != null)
			m_MaxFlareColor = m_LensFlare.color;
	}

	// Update is called once per frame
	void Update ()
	{
		if (PETools.PEUtil.MainCamTransform != null)
		{
			Vector3 eyepos = PETools.PEUtil.MainCamTransform.position;
			float dist_2 = (transform.position - eyepos).sqrMagnitude;
			float far_2 = m_FadeDist*m_FadeDist;
			float near_2 = (m_FadeDist - m_Falloff)*(m_FadeDist - m_Falloff);
			if (dist_2 < far_2)
			{
				m_Light.enabled = true;
				if (m_LensFlare != null)
					m_LensFlare.enabled = true;
				float dist_intensity = Mathf.Clamp01((far_2 - dist_2)/(far_2 - near_2));
				float sun_y = 1;
				// [Edit by zx]
//				if (NVWeatherSys.Instance != null)
//					sun_y = NVWeatherSys.Instance.Sun.m_Light.transform.forward.y;
				float night_intensity = Mathf.Clamp01((sun_y + 0.1f)*2);
				m_Light.intensity = m_MaxIntensity * dist_intensity * night_intensity;
				if (m_LensFlare != null)
					m_LensFlare.color = m_MaxFlareColor * night_intensity;
			}
			else
			{
				m_Light.enabled = false;
				if (m_LensFlare != null)
					m_LensFlare.enabled = false;
			}
		}
	}
}
