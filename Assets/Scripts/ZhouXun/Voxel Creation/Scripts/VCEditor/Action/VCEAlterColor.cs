using UnityEngine;
using System.Collections;

public class VCEAlterColor : VCEModify
{
	public int m_Pos;
	public Color32 m_Old;
	public Color32 m_New;
	
	public VCEAlterColor ( int position, Color32 old_color, Color32 new_color )
	{
		m_Pos = position;
		m_Old = old_color;
		m_New = new_color;
	}
	
	public override void Undo ()
	{
		VCEditor.s_Scene.m_IsoData.SetColor(m_Pos, m_Old);
	}
	public override void Redo ()
	{
		VCEditor.s_Scene.m_IsoData.SetColor(m_Pos, m_New);
	}
}
