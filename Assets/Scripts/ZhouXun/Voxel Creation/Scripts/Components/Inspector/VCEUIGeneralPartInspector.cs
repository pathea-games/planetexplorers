using UnityEngine;
using System.Collections;

public class VCEUIGeneralPartInspector : VCEUIComponentInspector
{
	public VCEUIVector3Input m_PositionInput;
	public VCEUIVector3Input m_RotationInput;
	public UICheckbox m_VisibleCheck;
    private int m_ArmorPartIndex =0;
	private VCGeneralPartData m_Data;
	public override VCComponentData Get ()
	{
		VCGeneralPartData data = m_Data.Copy() as VCGeneralPartData;
		data.m_Position = m_PositionInput.Vector;
		data.m_Rotation = VCEMath.NormalizeEulerAngle(m_RotationInput.Vector);
		data.m_Visible = m_VisibleCheck ? m_VisibleCheck.isChecked : true;
		data.Validate();
        data.m_ExtendData = VCEditor.Instance.m_UI.bonePanel.ArmorPartIndex;
		m_PositionInput.Vector = data.m_Position;
		m_RotationInput.Vector = data.m_Rotation;
		if (m_VisibleCheck) m_VisibleCheck.isChecked = data.m_Visible;
        m_ArmorPartIndex = data.m_ExtendData;
		
		return data;
	}
	public override void Set (VCComponentData data)
	{
		data.Validate();
		m_Data = data.Copy() as VCGeneralPartData;
		m_PositionInput.Vector = m_Data.m_Position;
		m_RotationInput.Vector = m_Data.m_Rotation;
        m_ArmorPartIndex = m_Data.m_ExtendData;
		if (m_VisibleCheck) m_VisibleCheck.isChecked = m_Data.m_Visible;

        WhiteCat.VCPArmorPivot m_VCPArmorPivot = m_SelectBrush.GetVCPArmorPivotByIndex(m_Data.m_ExtendData);
        if (m_VCPArmorPivot) VCEditor.Instance.m_UI.bonePanel.Show(m_VCPArmorPivot);
	}
	public void OnApplyClick ()
	{
		m_SelectBrush.ApplyInspectorChange();
		m_Data = Get().Copy() as VCGeneralPartData;
	}
	protected override bool Changed ()
	{
		if ( !VCUtils.VectorApproximate( m_PositionInput.Vector, m_Data.m_Position, m_PositionInput.m_Format ) )
			return true;
		if ( !VCUtils.VectorApproximate( m_RotationInput.Vector, m_Data.m_Rotation, m_RotationInput.m_Format ) )
			return true;
		if (m_VisibleCheck && m_VisibleCheck.isChecked != m_Data.m_Visible )
			return true;
        if (VCEditor.Instance && VCEditor.Instance.m_UI && VCEditor.Instance.m_UI.bonePanel && m_ArmorPartIndex != VCEditor.Instance.m_UI.bonePanel.ArmorPartIndex)
            return true;
		return false;
	}
}
