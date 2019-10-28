using UnityEngine;
using System.Collections;
using System;

public class VCEUIObjectLightlInspector : VCEUIComponentInspector
{
	public VCEUIVector3Input 	m_PositionInput;
	public VCEUIVector3Input 	m_RotationInput;
	public VCEUIVector3Input 	m_ScaleInput;
    public VCEUIColorPick 		m_ColorPicker;
	public UICheckbox			m_VisibleCheck;

	private VCObjectLightData m_Data;
	public override VCComponentData Get ()
	{
		VCObjectLightData data = m_Data.Copy() as VCObjectLightData;
		data.m_Position = m_PositionInput.Vector;
		data.m_Rotation = VCEMath.NormalizeEulerAngle(m_RotationInput.Vector);
        data.m_Scale = m_ScaleInput.Vector;
		data.m_Color = m_ColorPicker.FinalColor;
		data.m_Visible = m_VisibleCheck.isChecked;
        
		data.Validate();
		m_PositionInput.Vector = data.m_Position;
		m_RotationInput.Vector = data.m_Rotation;
        m_ScaleInput.Vector = data.m_Scale;
        m_ColorPicker.FinalColor = data.m_Color;
		m_VisibleCheck.isChecked = data.m_Visible;

		return data;
	}

	public override void Set (VCComponentData data)
	{
		data.Validate();
		m_Data = data.Copy() as VCObjectLightData;
		m_PositionInput.Vector = m_Data.m_Position;
		m_RotationInput.Vector = m_Data.m_Rotation;
		m_ScaleInput.Vector = m_Data.m_Scale;
        m_ColorPicker.FinalColor = m_Data.m_Color;
		m_VisibleCheck.isChecked = m_Data.m_Visible;
	}
	public void OnApplyClick ()
	{
		m_SelectBrush.ApplyInspectorChange();
		m_Data = Get().Copy() as VCObjectLightData;
	}
	protected override bool Changed ()
	{
		if ( !VCUtils.VectorApproximate( m_PositionInput.Vector, m_Data.m_Position, m_PositionInput.m_Format ) )
			return true;
		if ( !VCUtils.VectorApproximate( m_RotationInput.Vector, m_Data.m_Rotation, m_RotationInput.m_Format ) )
			return true;
        if (!VCUtils.VectorApproximate(m_ScaleInput.Vector, m_Data.m_Scale, m_ScaleInput.m_Format))
            return true;
        Color32 insc = m_ColorPicker.FinalColor;
		Color32 datac = m_Data.m_Color;
		if ( insc.r != datac.r )
			return true;
		if ( insc.g != datac.g )
			return true;
		if ( insc.b != datac.b )
			return true;
		if ( m_VisibleCheck.isChecked != m_Data.m_Visible )
			return true;
		return false;
	}
}
