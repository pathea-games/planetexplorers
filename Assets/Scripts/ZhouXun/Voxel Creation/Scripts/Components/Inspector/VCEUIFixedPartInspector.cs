using UnityEngine;
using System.Collections;

public class VCEUIFixedPartInspector : VCEUIComponentInspector
{
	public VCEUIVector3Input m_PositionInput;
	public UICheckbox m_VisibleCheck;
	private VCFixedPartData m_Data;
	public override VCComponentData Get ()
	{
		VCFixedPartData data = m_Data.Copy() as VCFixedPartData;
		data.m_Position = m_PositionInput.Vector;
		data.m_Visible = m_VisibleCheck.isChecked;
		data.Validate();
		m_PositionInput.Vector = data.m_Position;
		m_VisibleCheck.isChecked = data.m_Visible;
		
		return data;
	}
	public override void Set (VCComponentData data)
	{
		data.Validate();
		m_Data = data.Copy() as VCFixedPartData;
		m_PositionInput.Vector = m_Data.m_Position;
		m_VisibleCheck.isChecked = m_Data.m_Visible;
	}
	public void OnApplyClick ()
	{
		m_SelectBrush.ApplyInspectorChange();
		m_Data = Get().Copy() as VCFixedPartData;
	}
	protected override bool Changed ()
	{
		if ( !VCUtils.VectorApproximate( m_PositionInput.Vector, m_Data.m_Position, m_PositionInput.m_Format ) )
			return true;
		if ( m_VisibleCheck.isChecked != m_Data.m_Visible )
			return true;
		return false;
	}
}
