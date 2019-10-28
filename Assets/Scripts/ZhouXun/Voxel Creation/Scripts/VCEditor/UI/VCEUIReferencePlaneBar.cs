using UnityEngine;
using System.Collections;

public class VCEUIReferencePlaneBar : MonoBehaviour
{
	public UISlider m_Slider;
	public UILabel m_Label;
	
	// Use this for initialization
	void Start ()
	{
	
	}
	
	// Update is called once per frame
	void Update ()
	{
		if ( !VCEditor.DocumentOpen() ) return;
		
		int MaxHeight = VCEditor.s_Scene.m_Setting.m_EditorSize.y;
		m_Slider.numberOfSteps = MaxHeight + 1;
		int yValue = Mathf.RoundToInt(m_Slider.sliderValue * MaxHeight);
		m_Label.text = "Y = " + yValue.ToString();
		VCERefPlane.YRef = yValue;
	}
	
	public void Reset ()
	{
		m_Slider.sliderValue = 0;
	}
}
