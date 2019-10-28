using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VCEUISelectVoxelInspector : VCEUISelectInspector
{
	public VCESelectVoxel m_SelectBrush;
	public GameObject m_FunctionGroup;

	public UICheckbox m_BoxMethodCheck;

	public GameObject m_VolumeGroup;
	public UILabel m_VolumeLabel;
	public UISlider m_VolumeSlider;

	public GameObject m_TextureBtnGO;
	public GameObject m_ColorBtnGO;
	public GameObject m_EraseBtnGO;

	VCEAction m_VolModifyAction = null;
	float oldVol = 0;
	int oldKey = -1;

	private float VolumeSliderValue
	{
		get { return Mathf.Clamp(m_VolumeSlider.sliderValue, 0.004f, 1f); }
		set { m_VolumeSlider.sliderValue = value; }
	}

	public void Update ()
	{
		int sel_count = VCEditor.Instance.m_VoxelSelection.m_Selection.Count;
		if ( sel_count == 0 )
		{
			m_SelectInfo.text = "[00FFFF]0[-] " + "voxel selected".ToLocalizationString() + ".";
			m_FunctionGroup.SetActive(false);
		}
		else if ( sel_count == 1 )
		{
			m_SelectInfo.text = "[00FFFF]1[-] " + "voxel selected".ToLocalizationString() + ".";
			m_FunctionGroup.SetActive(true);
		}
		else if ( sel_count < 10000 )
		{
			m_SelectInfo.text = "[00FFFF]" + sel_count.ToString("#,###") + "[-] " + "voxels selected".ToLocalizationString() + ".";
			m_FunctionGroup.SetActive(true);
		}
		else
		{
			m_SelectInfo.text = "[00FFFF]" + sel_count.ToString("#,###") + "[-]\r\n" + "voxels selected".ToLocalizationString() + ".";
			m_FunctionGroup.SetActive(true);
		}

		if ( sel_count == 1 )
		{
			m_VolumeGroup.SetActive(true);
			int key = -1;
			VCVoxel old_voxel = new VCVoxel ();
			VCVoxel new_voxel = new VCVoxel ();
			foreach ( KeyValuePair<int, byte> kvp in VCEditor.Instance.m_VoxelSelection.m_Selection )
			{
				key = kvp.Key;
				old_voxel = VCEditor.s_Scene.m_IsoData.GetVoxel(kvp.Key);
			}
			if ( key != oldKey )
			{
				m_VolModifyAction = null;
				oldKey = key;
			}
			if ( key >= 0 )
			{
				if ( m_VolModifyAction == null )
				{
					m_VolModifyAction = new VCEAction ();
					VolumeSliderValue = old_voxel.VolumeF;
					oldVol = VolumeSliderValue;
				}
				m_VolumeLabel.text = "Voxel volume".ToLocalizationString() + " = [FFFF00]" + (VolumeSliderValue*200-100).ToString("0") + "[-]";
				new_voxel = old_voxel;
				new_voxel.VolumeF = VolumeSliderValue;
				if ( Input.GetMouseButtonUp(0) && Mathf.Abs(oldVol - VolumeSliderValue) > 0.002f )
				{
					oldVol = VolumeSliderValue;
					VCEditor.s_Mirror.CalcPrepare(VCEditor.s_Scene.m_Setting.m_VoxelSize);
					// Mirror
					if ( VCEditor.s_Mirror.Enabled_Masked )
					{
						VCEditor.s_Mirror.MirrorVoxel(VCIsoData.KeyToIPos(key));
						for ( int i = 0; i < VCEditor.s_Mirror.OutputCnt; ++i )
						{
							if ( VCEditor.s_Scene.m_IsoData.IsPointIn(VCEditor.s_Mirror.Output[i]) )
							{
								int voxel_pos = VCIsoData.IPosToKey(VCEditor.s_Mirror.Output[i]);
								VCVoxel old_v = VCEditor.s_Scene.m_IsoData.GetVoxel(voxel_pos);
								if ( old_v.Volume == 0 )
									continue;
								VCVoxel new_v = old_v;
								new_v.VolumeF = VolumeSliderValue;
								if ( m_VolModifyAction.Modifies.Count == 0 )
								{
									VCEAlterVoxel modify = new VCEAlterVoxel (voxel_pos, old_v, new_v);
									m_VolModifyAction.Modifies.Add(modify);
									m_VolModifyAction.Do();
								}
								else
								{
									bool changed = false;
									foreach ( VCEModify modify in m_VolModifyAction.Modifies )
									{
										VCEAlterVoxel avmodify = modify as VCEAlterVoxel;
										if ( avmodify != null )
										{
											if ( avmodify.m_Pos == voxel_pos )
											{
												avmodify.m_New = new_v;
												changed = true;
											}
										}
									}
									if ( !changed )
									{
										VCEAlterVoxel modify = new VCEAlterVoxel (voxel_pos, old_v, new_v);
										m_VolModifyAction.Modifies.Add(modify);
									}
									m_VolModifyAction.DoButNotRegister();
								}
							}
						}
					}
					// No mirror
					else
					{
						if ( m_VolModifyAction.Modifies.Count == 0 )
						{
							VCEAlterVoxel modify = new VCEAlterVoxel (key, old_voxel, new_voxel);
							m_VolModifyAction.Modifies.Add(modify);
							m_VolModifyAction.Do();
						}
						else
						{
							bool changed = false;
							foreach ( VCEModify modify in m_VolModifyAction.Modifies )
							{
								VCEAlterVoxel avmodify = modify as VCEAlterVoxel;
								if ( avmodify != null )
								{
									if ( avmodify.m_Pos == key )
									{
										avmodify.m_New = new_voxel;
										changed = true;
									}
								}
							}
							if ( !changed )
							{
								VCEAlterVoxel modify = new VCEAlterVoxel (key, old_voxel, new_voxel);
								m_VolModifyAction.Modifies.Add(modify);
							}
							m_VolModifyAction.DoButNotRegister();
						}
					}
				}
			}
		}
		else
		{
			m_VolumeGroup.SetActive(false);
			m_VolModifyAction = null;
			oldKey = -1;
			oldVol = 0;
		}
		
		if ( m_BoxMethodCheck.isChecked )
		{
			m_SelectBrush.SelectMethod = EVCESelectMethod.Box;
		}
//		else if ( m_BoxMethodCheck.isChecked )
//		{
//			
//		}
		else
		{
			m_SelectBrush.SelectMethod = EVCESelectMethod.None;
		}
		
		if ( VCEditor.Instance.m_UI.m_MaterialTab.isChecked )
		{
			if ( VCEditor.SelectedVoxelType >= 0 )
				m_TextureBtnGO.SetActive(true);
			else
				m_TextureBtnGO.SetActive(false);
			m_ColorBtnGO.SetActive(false);
			m_EraseBtnGO.SetActive(false);
		}
		else if ( VCEditor.Instance.m_UI.m_PaintTab.isChecked )
		{
			m_TextureBtnGO.SetActive(false);
			m_ColorBtnGO.SetActive(true);
			m_EraseBtnGO.SetActive(true);
		}
		if ( m_FunctionGroup.activeSelf )
		{
			m_FunctionGroup.GetComponent<UIGrid>().Reposition();
		}
	}
	
	public void CancelAllMethod ()
	{
		m_BoxMethodCheck.isChecked = false;
		// .. .isChecked = false;
	}
	
	void OnDeleteClick()
	{
		m_SelectBrush.DeleteSelection();
	}
	
	void OnTextureClick()
	{
		m_SelectBrush.TextureSelection();
	}
	
	void OnColorClick()
	{
		m_SelectBrush.ColorSelection(VCEditor.SelectedColor);
	}
	
	void OnEraseColorClick()
	{
		m_SelectBrush.ColorSelection(VCIsoData.BLANK_COLOR);
	}
}
