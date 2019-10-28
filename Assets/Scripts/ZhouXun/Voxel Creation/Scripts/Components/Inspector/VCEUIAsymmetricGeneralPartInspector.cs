using UnityEngine;
using System.Collections;

public class VCEUIAsymmetricGeneralPartInspector : VCEUIComponentInspector
{
	public VCEUIVector3Input m_PositionInput;
	public VCEUIVector3Input m_RotationInput;
	public UICheckbox m_Phase0Check;
	public UICheckbox m_Phase1Check;
	public UICheckbox m_VisibleCheck;
	public WhiteCat.UIToggleGroup m_Group;	/**/
	private VCAsymmetricGeneralPartData m_Data;
	public override VCComponentData Get ()
	{
		VCAsymmetricGeneralPartData data = m_Data.Copy() as VCAsymmetricGeneralPartData;
		data.m_Position = m_PositionInput.Vector;
		data.m_Rotation = VCEMath.NormalizeEulerAngle(m_RotationInput.Vector);
		data.m_Visible = m_VisibleCheck.isChecked;
		data.m_Positive = m_Phase0Check.isChecked;
		if (m_Group) data.m_ExtendData = m_Group.selected;   /**/
		data.Validate();
		m_PositionInput.Vector = data.m_Position;
		m_RotationInput.Vector = data.m_Rotation;
		m_VisibleCheck.isChecked = data.m_Visible;
		m_Phase0Check.isChecked = data.m_Positive;
		m_Phase1Check.isChecked = !data.m_Positive;
		return data;
	}
	public override void Set (VCComponentData data)
	{
		data.Validate();
		m_Data = data.Copy() as VCAsymmetricGeneralPartData;
		m_PositionInput.Vector = m_Data.m_Position;
		m_RotationInput.Vector = m_Data.m_Rotation;
		m_VisibleCheck.isChecked = m_Data.m_Visible;
		m_Phase0Check.isChecked = m_Data.m_Positive;
		m_Phase1Check.isChecked = !m_Data.m_Positive;
		if (m_Group) m_Group.selected = data.m_ExtendData;   /**/
	}
	public void OnApplyClick ()
	{
		m_SelectBrush.ApplyInspectorChange();
		m_Data = Get().Copy() as VCAsymmetricGeneralPartData;
	}
	protected override bool Changed ()
	{
		if ( !VCUtils.VectorApproximate( m_PositionInput.Vector, m_Data.m_Position, m_PositionInput.m_Format ) )
			return true;
		if ( !VCUtils.VectorApproximate( m_RotationInput.Vector, m_Data.m_Rotation, m_RotationInput.m_Format ) )
			return true;
		if ( m_VisibleCheck.isChecked != m_Data.m_Visible )
			return true;
		if ( m_Phase0Check.isChecked != m_Data.m_Positive )
			return true;
		if ( m_Phase1Check.isChecked == m_Data.m_Positive )
			return true;
		if (m_Group && m_Data.m_ExtendData != m_Group.selected)   /**/
			return true;

		return false;
	}
}
