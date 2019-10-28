using UnityEngine;
using System.Collections;

public class VCEUIFreeAirbrushInspector : VCEUIBrushInspector
{
	public VCEFreeAirbrush m_ParentBrush;
	
	public UISlider m_RadiusSlider;
	public UISlider m_HardnessSlider;
	public UISlider m_StrengthSlider;
	
	public UILabel m_RadiusLabel;
	public UILabel m_HardnessLabel;
	public UILabel m_StrengthLabel;
	
	void Start ()
	{
		m_RadiusSlider.sliderValue = m_ParentBrush.m_Radius / 16;
		m_HardnessSlider.sliderValue = m_ParentBrush.m_Hardness;
		m_StrengthSlider.sliderValue = (m_ParentBrush.m_Strength - 0.1f) / 0.9f;
	}
	
	void Update ()
	{
		m_ParentBrush.m_Radius = m_RadiusSlider.sliderValue * 16;
		if ( m_ParentBrush.m_Radius < 0.5 )
			m_ParentBrush.m_Radius = 0.5f;
		m_ParentBrush.m_Hardness = m_HardnessSlider.sliderValue;
		m_ParentBrush.m_Strength = m_StrengthSlider.sliderValue * 0.9f + 0.1f;
		
		m_RadiusLabel.text = m_ParentBrush.m_Radius.ToString("0.#");
		m_HardnessLabel.text = (m_ParentBrush.m_Hardness * 100).ToString("0") + "%";
		m_StrengthLabel.text = (m_ParentBrush.m_Strength * 100).ToString("0") + "%";
	}
}
