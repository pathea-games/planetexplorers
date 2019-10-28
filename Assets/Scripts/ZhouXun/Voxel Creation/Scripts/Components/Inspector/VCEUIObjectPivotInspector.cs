using UnityEngine;
using System.Collections;
using System;

public class VCEUIObjectPivotInspector : VCEUIComponentInspector
{
	public VCEUIVector3Input m_PositionInput;
	public VCEUIVector3Input m_RotationInput;
	public VCEUIVector3Input m_ScaleInput;
    public UICheckbox m_VisibleCheck;
	public UILabel m_AnlgeLable;
	public UISlider m_AnlgeSlider;


	private VCObjectPivotData m_Data;


	public int Float01ToIntAngle(float value01)
	{
		return Mathf.RoundToInt((value01 - 0.5f) * 144f) * 5;
	}


	public override VCComponentData Get ()
	{
		VCObjectPivotData data = m_Data.Copy() as VCObjectPivotData;
		data.m_Position = m_PositionInput.Vector;
		data.m_Rotation = VCEMath.NormalizeEulerAngle(m_RotationInput.Vector);
		data.m_Scale = m_ScaleInput.Vector;
		data.m_Visible = m_VisibleCheck.isChecked;
		int ang = Float01ToIntAngle(m_AnlgeSlider.sliderValue);
		data.m_PivotAng = ang;

		data.Validate();
		m_PositionInput.Vector = data.m_Position;
		m_RotationInput.Vector = data.m_Rotation;
        m_ScaleInput.Vector = data.m_Scale;
		m_VisibleCheck.isChecked = data.m_Visible;
		ang = data.m_PivotAng;
		m_AnlgeLable.text = ang.ToString();
		m_AnlgeSlider.sliderValue = Convert.ToSingle(ang) / 720 + 0.5f;

		return data;
	}


	public override void Set (VCComponentData data)
	{
		data.Validate();
		m_Data = data.Copy() as VCObjectPivotData;
		m_PositionInput.Vector = m_Data.m_Position;
		m_RotationInput.Vector = m_Data.m_Rotation;
        m_ScaleInput.Vector = m_Data.m_Scale;

        m_VisibleCheck.isChecked = m_Data.m_Visible;

		m_AnlgeLable.text = m_Data.m_PivotAng.ToString();
		m_AnlgeSlider.sliderValue = Convert.ToSingle(m_Data.m_PivotAng) / 720 + 0.5f;

	}


	public void OnApplyClick ()
	{
		m_SelectBrush.ApplyInspectorChange();
		m_Data = Get().Copy() as VCObjectPivotData;

		originValue01 = m_AnlgeSlider.sliderValue;
	}


	protected override bool Changed ()
	{
		if ( !VCUtils.VectorApproximate( m_PositionInput.Vector, m_Data.m_Position, m_PositionInput.m_Format ) )
			return true;
		if ( !VCUtils.VectorApproximate( m_RotationInput.Vector, m_Data.m_Rotation, m_RotationInput.m_Format ) )
			return true;
        if (!VCUtils.VectorApproximate(m_ScaleInput.Vector, m_Data.m_Scale, m_ScaleInput.m_Format))
            return true;
        if ( m_VisibleCheck.isChecked != m_Data.m_Visible )
			return true;
		if ( Float01ToIntAngle(m_AnlgeSlider.sliderValue) != m_Data.m_PivotAng )
			return true;
		return false;
	}


	WhiteCat.VCPPivot pivot;
	float originValue01;


	void Start()
	{
		pivot = null;

		if (VCEditor.s_Active)
		{
			VCESelectComponent brush = VCEditor.SelectComponentBrush;
			if (brush != null && brush.m_Selection != null && brush.m_Selection.Count == 1)
			{
				var component = brush.m_Selection[0].m_Component;
				if (component != null)
				{
					pivot = component.GetComponent<WhiteCat.VCPPivot>();
					if (pivot != null)
					{
						m_AnlgeSlider.onValueChange += ChangeAngle;
						originValue01 = m_AnlgeSlider.sliderValue;
					}
				}
			}
		}
	}


	void ChangeAngle(float value01)
	{
		pivot.Angle = Float01ToIntAngle(value01);
		m_AnlgeLable.text = ((int)pivot.Angle).ToString();
	}


	void OnDisable()
	{
		if (pivot != null)
		{
			m_AnlgeSlider.onValueChange -= ChangeAngle;
			ChangeAngle(originValue01);
		}
		pivot = null;
	}

}
