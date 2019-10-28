using UnityEngine;
using System.Collections;

public class VCEUIFreePartInspector : VCEUIComponentInspector
{
	public VCEUIVector3Input m_PositionInput;
	public VCEUIVector3Input m_RotationInput;
	public VCEUIVector3Input m_ScaleInput;
	public UICheckbox m_VisibleCheck;
	public WhiteCat.UIToggleGroup m_Group;	/**/
	private VCFreePartData m_Data;
	public override VCComponentData Get ()
	{
		VCFreePartData data = m_Data.Copy() as VCFreePartData;
		data.m_Position = m_PositionInput.Vector;
		if (m_RotationInput) data.m_Rotation = VCEMath.NormalizeEulerAngle(m_RotationInput.Vector);
		data.m_Scale = m_ScaleInput.Vector;
		data.m_Visible = m_VisibleCheck.isChecked;
		if (m_Group) data.m_ExtendData = m_Group.selected;   /**/
		data.Validate();
		m_PositionInput.Vector = data.m_Position;
		if (m_RotationInput) m_RotationInput.Vector = data.m_Rotation;
		m_ScaleInput.Vector = data.m_Scale;
		m_VisibleCheck.isChecked = data.m_Visible;
		
		return data;
	}
	public override void Set (VCComponentData data)
	{
		data.Validate();
		m_Data = data.Copy() as VCFreePartData;
		m_PositionInput.Vector = m_Data.m_Position;
		if (m_RotationInput) m_RotationInput.Vector = m_Data.m_Rotation;
		m_ScaleInput.Vector = m_Data.m_Scale;
		m_VisibleCheck.isChecked = m_Data.m_Visible;
		if (m_Group) m_Group.selected = data.m_ExtendData;   /**/
	}
	public void OnApplyClick ()
	{
		m_SelectBrush.ApplyInspectorChange();
		m_Data = Get().Copy() as VCFreePartData;
	}
	protected override bool Changed ()
	{
		if ( !VCUtils.VectorApproximate( m_PositionInput.Vector, m_Data.m_Position, m_PositionInput.m_Format ) )
			return true;
		if (m_RotationInput && !VCUtils.VectorApproximate( m_RotationInput.Vector, m_Data.m_Rotation, m_RotationInput.m_Format ) )
			return true;
		if ( !VCUtils.VectorApproximate( m_ScaleInput.Vector, m_Data.m_Scale, m_ScaleInput.m_Format ) )
			return true;
		if ( m_VisibleCheck.isChecked != m_Data.m_Visible )
			return true;
		if (m_Group && m_Data.m_ExtendData != m_Group.selected)   /**/
			return true;

		return false;
	}
}
