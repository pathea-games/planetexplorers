using UnityEngine;
using System.Collections;

public class VCEUIBoxMethodInspector : VCEUIInspector
{
	public VCESelectMethod_Box m_SelectMethod;
	public UILabel m_StatusLabel;
	public UILabel m_DepthLabel;
	public UISlider m_FeatherSlider;
	public UILabel m_FeatherValueLabel;
	public UICheckbox m_FeatherDepthCheck;
	public UICheckbox m_MaterialSelectCheck;
	
	// Use this for initialization
	void Start ()
	{
		m_FeatherSlider.sliderValue = (float)( VCESelectMethod_Box.s_RecentFeatherLength ) / 20.0f;
		m_FeatherValueLabel.text = VCESelectMethod_Box.s_RecentFeatherLength.ToString();
		m_FeatherDepthCheck.isChecked = !VCESelectMethod_Box.s_RecentPlaneFeather;
		m_MaterialSelectCheck.isChecked = VCESelectMethod_Box.s_RecentMaterialSelect;
		m_DepthLabel.text = "Select depth".ToLocalizationString() + ": " + VCESelectMethod_Box.s_RecentDepth.ToString() + " (+/-)";
	}
	
	// Update is called once per frame
	void Update ()
	{
		if ( m_SelectMethod == null ) return;
		IntVector3 selecting_size = m_SelectMethod.SelectingSize();
		if ( selecting_size.x * selecting_size.y > 0 )
			m_StatusLabel.gameObject.SetActive(true);
		else
			m_StatusLabel.gameObject.SetActive(false);
		m_StatusLabel.text = selecting_size.x.ToString() + " x " + selecting_size.y.ToString();
		m_DepthLabel.text = "Select depth".ToLocalizationString() + ": " + selecting_size.z.ToString() + " (+/-)";
		m_FeatherValueLabel.text = Mathf.RoundToInt(m_FeatherSlider.sliderValue * 20).ToString();
		m_SelectMethod.m_FeatherLength = Mathf.RoundToInt(m_FeatherSlider.sliderValue * 20);
		if ( m_SelectMethod.m_FeatherLength < 1 )
			m_FeatherDepthCheck.gameObject.SetActive(false);
		else
			m_FeatherDepthCheck.gameObject.SetActive(true);
		m_SelectMethod.m_PlaneFeather = !m_FeatherDepthCheck.isChecked;

//		if ( m_SelectMethod.m_SelectLength < 2 )
//			m_MaterialSelectCheck.gameObject.SetActive(false);
//		else
//			m_MaterialSelectCheck.gameObject.SetActive(true);

		m_SelectMethod.m_MaterialSelectChange = m_SelectMethod.m_MaterialSelect ^ m_MaterialSelectCheck.isChecked;
		m_SelectMethod.m_MaterialSelect = m_MaterialSelectCheck.isChecked;
	}
}
