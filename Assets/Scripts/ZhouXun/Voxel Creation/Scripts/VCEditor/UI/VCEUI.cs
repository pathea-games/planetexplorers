using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class VCEUI : MonoBehaviour
{
	public Camera m_UICamera;
	public Camera m_HoloBoardCamera;
	public GameObject m_MainUIPanel;
	public GameObject m_NearUIPanel;
	public UITweener m_LeftBox;
	public UITweener m_RightBox;
	public UITweener m_TopBox;
	public UITweener m_BottomBox;
	public GameObject m_ColorPickWindowRes;
	
	public GameObject m_LeftMotionCollapse;
	public GameObject m_LeftMotionExpand;
	public GameObject m_RightMotionCollapse;
	public GameObject m_RightMotionExpand;
	
	public UILabel m_TitleLabel;
	public UILabel m_FileLabel;
	public UILabel m_SceneSizeLabel;
	public UILabel m_SceneSizeLabel2;
	public UILabel m_ErrorLabel;
	
	public UICheckbox m_PartTab;
	public UICheckbox m_MaterialTab;
	public UICheckbox m_PaintTab;
	public UICheckbox m_DecalTab;
	public UICheckbox m_EffectTab;
	public UICheckbox m_ISOTab;
	
	public Transform m_InspectorGroup;
	public Transform m_StatisticsGroup;

	public VCEUIBrushGroup m_VoxelBrushItemGroup;
	public VCEUIBrushGroup m_PaintBrushItemGroup;
	
	public VCEUIPartList m_PartList;
	public VCEUIMaterialList m_MaterialList;
	public VCEUIDecalList m_DecalList;
	public VCEUIIsoList m_IsoList;
	public VCEUIStatisticsPanel m_StatPanel;
	public UIPopupList m_PartTypePopupList;
	public UIPopupList m_MatterPopupList;

	public VCEUIPartDescList m_PartDescList; // Holoboard

	public VCEUIColorPick m_PaintColorPick;
	public UISlider m_PaintBlendMethodSlider;
	
	public VCEUIReferencePlaneBar m_ReferencePlaneBar;
	
	public GameObject m_SelectComponentBrush;
	public GameObject m_DrawPartBrush;
	public GameObject m_DrawDecalBrush;
	private GameObject m_SelectComponentBrushInst;
	private GameObject m_DrawPartBrushInst;
	private GameObject m_DrawDecalBrushInst;
	
	public UICheckbox m_DefaultBrush_Material;
	public UICheckbox m_DefaultBrush_Color;

	public GameObject m_TransformTypeGroup;
	public UICheckbox m_TransformTypeNoneCheck;
	public UICheckbox m_TransformTypeMoveCheck;
	public UICheckbox m_TransformTypeRotateCheck;

	// Wnds
	public VCEUIMatWnd m_MaterialWindow;
	public VCEUIDecalWnd m_DecalWindow;
	public VCEUIMirrorWnd m_MirrorWindow;
	public VCEUISaveWnd m_SaveWindow;
	public VCEUIExportWnd m_ExportWindow;
	public VCEUIFastCreateWnd m_FastCreateWindow;

	// MENU
	public UIImageButton m_UndoButton;
	public UIImageButton m_RedoButton;
	public UIImageButton m_DeleteButton;

	// FUNC
	public UIImageButton m_MirrorButton;

	// Switches
	public UICheckbox m_HideParts_Material;
	public UICheckbox m_HideParts_Color;
	
	// Scene Gizmo
	public VCEUISceneGizmoCam m_SceneGizmo;
	public Vector2 m_SceneGizmoOffset;

	public bool m_ShowLaserGrid;


	// Bone Panel
	public WhiteCat.BonePanelUI bonePanel;

	// Tips
	public VCEUITips m_IsoTip;
	public VCEUITips m_ExportTip;
	public VCEUITips m_RefPlaneTip;

	public void Init ()
	{
		m_UICamera.enabled = false;
		m_MainUIPanel.SetActive(false);
		m_NearUIPanel.SetActive(false);
		
		m_PartList.Init();
		m_MaterialList.Init();
		m_DecalList.Init();
		m_IsoList.Init();
		m_SaveWindow.Init();
		m_ExportWindow.Init();
		m_StatPanel.Init();
		m_SceneGizmo = VCEditor.Instance.m_SceneGizmo;
    }

	bool firstvalid = true;
	public void OnSceneCreate ()
	{
		if ( VCEditor.DocumentOpen() )
		{
			VCESceneSetting setting = VCEditor.s_Scene.m_Setting;
			// Tab
			if ( !m_ISOTab.isChecked )
				m_PartTab.isChecked = true;
			if ( !VCEditor.s_Ready )
			{
				OnPartTab(false);
				OnMaterialTab(false);
				OnPaintTab(false);
				OnDecalTab(false);
				OnEffectTab(false);
				OnISOTab(false);
				m_PartTab.isChecked = true;
			}

			VCEditor.Instance.m_UI.m_IsoTip.Hide();
			firstvalid = true;
			UpdateErrorLabel();

			// Title label
			VCESceneSetting parentsetting = VCConfig.s_EditorScenes.Find(iter => iter.m_Id == setting.m_ParentId);
			if (parentsetting.m_Id == 1)
				m_TitleLabel.text = setting.m_Name.ToUpper().ToLocalizationString();
			else
				m_TitleLabel.text = parentsetting.m_Name.ToUpper().ToLocalizationString() + " - " + setting.m_Name.ToUpper().ToLocalizationString();
			if (VCEditor.s_Scene.m_Setting.m_Category == EVCCategory.cgObject)
				m_TitleLabel.text = "OBJECT".ToLocalizationString() + " - " + m_TitleLabel.text.ToLocalizationString();
			m_SceneSizeLabel.text = "  " + 
				setting.EditorWorldSize.x.ToString("0.##") + "m x " +
				setting.EditorWorldSize.z.ToString("0.##") + "m x " +
					setting.EditorWorldSize.y.ToString("0.##") + "m";
			m_SceneSizeLabel2.text = 
				VCUtils.LengthToString(setting.m_MajorInterval * setting.m_VoxelSize) + "\r\n" +
				VCUtils.LengthToString(setting.m_MinorInterval * setting.m_VoxelSize) + "\r\n" +
				VCUtils.LengthToString(setting.m_VoxelSize);
			
			// Part List
			m_PartList.InitTypeList();
			m_PartList.RefreshPartList(m_PartTypePopupList.selection);
			m_PartList.RepositionList();
			// Material List
			m_MaterialList.InitMatterList();
			m_MaterialList.RefreshMaterialList(m_MatterPopupList.selection);
			m_MaterialList.RepositionList();
			// Decal List
			m_DecalList.RefreshDecalList();
			m_DecalList.RepositionList();
			// Iso List
			m_IsoList.RefreshIsoList();
			m_IsoList.RepositionList();
			// Stat
			m_StatPanel.SetIsoIcon();
			m_StatPanel.OnCreationInfoRefresh();
		}
	}
	
	public void OnSceneClose ()
	{
		CloseWindows();
		m_MirrorWindow.Reset();
		m_ReferencePlaneBar.Reset();
	}

	void Update ()
	{
		UpdateSimpleLabels();
		UpdateFactoryGroups();
		UpdateUndoRedoButtons();
		UpdateFunctionButtons();
		UpdateComponentBrushes();
		UpdateTransformTypeGroup();
		UpdateSceneGizmo();
		UpdateErrorLabel();
        //lz-2016.09.30 检测鼠标是否进入UI
        CheckHoverInUI();
    }
	
	void LateUpdate ()
	{
		CheckRevertAnchors();
		CheckMotionButtons();
	}
	
	public void ShowUI()
	{
		StartCoroutine("ShowUICoroutine");
	}
	IEnumerator ShowUICoroutine()
	{
        m_MainUIPanel.SetActive(true);
		m_NearUIPanel.SetActive(true);
		yield return 0;
		yield return 0;
		yield return 0;
		AllBoxTweenIn();
		yield return 0;
		m_UICamera.enabled = true;	
		yield return 0;
		yield return 10;
		m_FastCreateWindow.ShowWindow();
    }
	public void HideUI()
	{
		CloseWindows();
		m_UICamera.enabled = false;
		m_MainUIPanel.SetActive(false);
		m_NearUIPanel.SetActive(false);
	}
	public void CloseWindows()
	{
		m_MaterialWindow.m_Window.SetActive(false);
		m_DecalWindow.HideWindow();
		m_MirrorWindow.HideWindow();
		m_SaveWindow.gameObject.SetActive(false);
		m_ExportWindow.gameObject.SetActive(false);
    }
	private List<BoxCollider> m_DisabledColliders = null;
	public void DisableFunctions()
	{
		BoxCollider[] bcs = GetComponentsInChildren<BoxCollider>(true);
		if ( m_DisabledColliders == null )
		{
			m_DisabledColliders = new List<BoxCollider> ();
		}
		foreach ( BoxCollider bc in bcs )
		{
			if ( bc.enabled )
			{
				bc.enabled = false;
				m_DisabledColliders.Add(bc);
			}
		}
	}
	public void EnableFunctions()
	{
		if ( m_DisabledColliders != null )
		{
			foreach ( BoxCollider bc in m_DisabledColliders )
			{
				if ( bc != null )
					bc.enabled = true;
			}
			m_DisabledColliders.Clear();
			m_DisabledColliders = null;
		}
	}
		
	// Anchors & Motion ----------------------------------------------
	private void EnableAllAnchors()
	{
		EnableLeftAnchors();
		EnableRightAnchors();
		EnableTopAnchors();
		EnableBottomAnchors();
	}
	private void EnableLeftAnchors()
	{
		UIAnchor[] anchors = m_LeftBox.GetComponentsInChildren<UIAnchor>(true);
		foreach ( UIAnchor anchor in anchors )
		{
			anchor.enabled = true;
		}
	}
	private void EnableRightAnchors()
	{
		UIAnchor[] anchors = m_RightBox.GetComponentsInChildren<UIAnchor>(true);
		foreach ( UIAnchor anchor in anchors )
		{
			anchor.enabled = true;
		}
	}
	private void EnableTopAnchors()
	{
		UIAnchor[] anchors = m_TopBox.GetComponentsInChildren<UIAnchor>(true);
		foreach ( UIAnchor anchor in anchors )
		{
			anchor.enabled = true;
		}
	}
	private void EnableBottomAnchors()
	{
		UIAnchor[] anchors = m_BottomBox.GetComponentsInChildren<UIAnchor>(true);
		foreach ( UIAnchor anchor in anchors )
		{
			anchor.enabled = true;
		}
	}
	private void DisableAllAnchors()
	{
		DisableLeftAnchors();
		DisableRightAnchors();
		DisableTopAnchors();
		DisableBottomAnchors();
	}
	private void DisableLeftAnchors()
	{
		UIAnchor[] anchors = m_LeftBox.GetComponentsInChildren<UIAnchor>(true);
		foreach ( UIAnchor anchor in anchors )
		{
			anchor.enabled = false;
		}
	}
	private void DisableRightAnchors()
	{
		UIAnchor[] anchors = m_RightBox.GetComponentsInChildren<UIAnchor>(true);
		foreach ( UIAnchor anchor in anchors )
		{
			anchor.enabled = false;
		}
	}
	private void DisableTopAnchors()
	{
		UIAnchor[] anchors = m_TopBox.GetComponentsInChildren<UIAnchor>(true);
		foreach ( UIAnchor anchor in anchors )
		{
			anchor.enabled = false;
		}
	}
	private void DisableBottomAnchors()
	{
		UIAnchor[] anchors = m_BottomBox.GetComponentsInChildren<UIAnchor>(true);
		foreach ( UIAnchor anchor in anchors )
		{
			anchor.enabled = false;
		}
	}
	public void AllBoxTweenIn()
	{
		DisableAllAnchors();
		m_LeftBox.method = UITweener.Method.BounceIn;
		m_RightBox.method = UITweener.Method.BounceIn;
		m_TopBox.method = UITweener.Method.BounceIn;
		m_BottomBox.method = UITweener.Method.BounceIn;
		m_LeftBox.duration = 1.3f;
		m_RightBox.duration = 1.3f;
		m_TopBox.duration = 1.3f;
		m_BottomBox.duration = 1.3f;
		m_DontCheckLeftAnchor = false;
		m_DontCheckRightAnchor = false;
		m_DontCheckTopAnchor = false;
		m_DontCheckBottomAnchor = false;
		if ( m_LeftBox.tweenFactor < 0.01f )
			m_LeftBox.Play(true);
		if ( m_RightBox.tweenFactor < 0.01f )
			m_RightBox.Play(true);
		if ( m_TopBox.tweenFactor < 0.01f )
			m_TopBox.Play(true);
		if ( m_BottomBox.tweenFactor < 0.01f )
			m_BottomBox.Play(true);
	}
	public void AllBoxTweenOut()
	{
		DisableAllAnchors();
		m_LeftBox.method = UITweener.Method.EaseIn;
		m_RightBox.method = UITweener.Method.EaseIn;
		m_TopBox.method = UITweener.Method.EaseIn;
		m_BottomBox.method = UITweener.Method.EaseIn;
		m_LeftBox.duration = 0.5f;
		m_RightBox.duration = 0.5f;
		m_TopBox.duration = 0.5f;
		m_BottomBox.duration = 0.5f;
		m_DontCheckLeftAnchor = true;
		m_DontCheckRightAnchor = true;
		m_DontCheckTopAnchor = true;
		m_DontCheckBottomAnchor = true;
		if ( m_LeftBox.tweenFactor > 0.99f )
			m_LeftBox.Play(false);
		if ( m_RightBox.tweenFactor > 0.99f )
			m_RightBox.Play(false);
		if ( m_TopBox.tweenFactor > 0.99f )
			m_TopBox.Play(false);
		if ( m_BottomBox.tweenFactor > 0.99f )
			m_BottomBox.Play(false);
	}
	public void LeftBoxTweenIn()
	{
		if ( m_LeftBox.tweenFactor < 0.01f )
		{
			m_LeftBox.method = UITweener.Method.BounceIn;
			m_LeftBox.duration = 1.0f;
			DisableLeftAnchors();
			m_DontCheckLeftAnchor = false;
			m_LeftBox.Play(true);
		}
	}
	public void LeftBoxTweenOut()
	{
		if ( m_LeftBox.tweenFactor > 0.99f )
		{
			m_LeftBox.method = UITweener.Method.EaseIn;
			m_LeftBox.duration = 0.5f;
			DisableLeftAnchors();
			m_DontCheckLeftAnchor = true;
			m_LeftBox.Play(false);
		}
	}
	public void RightBoxTweenIn()
	{
		if ( m_RightBox.tweenFactor < 0.01f )
		{
			m_RightBox.method = UITweener.Method.BounceIn;
			m_RightBox.duration = 1.0f;
			DisableRightAnchors();
			m_DontCheckRightAnchor = false;
			m_RightBox.Play(true);
		}
	}
	public void RightBoxTweenOut()
	{
		if ( m_RightBox.tweenFactor > 0.99f )
		{
			m_RightBox.method = UITweener.Method.EaseIn;
			m_RightBox.duration = 0.5f;
			DisableRightAnchors();
			m_DontCheckRightAnchor = true;
			m_RightBox.Play(false);
		}
	}
	
	private bool m_DontCheckLeftAnchor = false;
	private bool m_DontCheckRightAnchor = false;
	private bool m_DontCheckTopAnchor = false;
	private bool m_DontCheckBottomAnchor = false;

	private void CheckRevertAnchors()
	{
		if ( m_LeftBox.tweenFactor == 1f && !m_DontCheckLeftAnchor )
			EnableLeftAnchors();
		if ( m_RightBox.tweenFactor == 1f && !m_DontCheckRightAnchor )
			EnableRightAnchors();
		if ( m_TopBox.tweenFactor == 1f && !m_DontCheckTopAnchor )
			EnableTopAnchors();
		if ( m_BottomBox.tweenFactor == 1f && !m_DontCheckBottomAnchor )
			EnableBottomAnchors();
	}
	private void CheckMotionButtons()
	{
		if ( m_LeftBox.tweenFactor == 0f && VCEditor.s_Ready )
		{
			m_LeftMotionExpand.SetActive(true);
			m_LeftMotionCollapse.SetActive(false);
		}
		else if ( m_LeftBox.tweenFactor == 1f && VCEditor.s_Ready )
		{
			m_LeftMotionExpand.SetActive(false);
			m_LeftMotionCollapse.SetActive(true);
		}
		else
		{
			m_LeftMotionExpand.SetActive(false);
			m_LeftMotionCollapse.SetActive(false);
		}
		if ( m_RightBox.tweenFactor == 0f && VCEditor.s_Ready )
		{
			m_RightMotionExpand.SetActive(true);
			m_RightMotionCollapse.SetActive(false);
		}
		else if ( m_RightBox.tweenFactor == 1f && VCEditor.s_Ready )
		{
			m_RightMotionExpand.SetActive(false);
			m_RightMotionCollapse.SetActive(true);
		}
		else
		{
			m_RightMotionExpand.SetActive(false);
			m_RightMotionCollapse.SetActive(false);
		}
	}
	public void OnLeftMotionClick()
	{
		if ( m_LeftBox.tweenFactor == 0f )
			LeftBoxTweenIn();
		else if ( m_LeftBox.tweenFactor == 1f )
			LeftBoxTweenOut();
	}
	public void OnRightMotionClick()
	{
		if ( m_RightBox.tweenFactor == 0f )
			RightBoxTweenIn();
		else if ( m_RightBox.tweenFactor == 1f )
			RightBoxTweenOut();
	}
	
	private void UpdateUndoRedoButtons()
	{
		if ( VCEHistory.CanUndo() )
		{
			m_UndoButton.enabled = true;
			m_UndoButton.target.color = new Color(1,1,1,1);
		}
		else
		{
			// m_UndoButton.enabled = false;
			m_UndoButton.target.color = new Color(1,1,1,0.15f);
		}
		if ( VCEHistory.CanRedo() )
		{
			m_RedoButton.enabled = true;
			m_RedoButton.target.color = new Color(1,1,1,1);
		}
		else
		{
			// m_RedoButton.enabled = false;
			m_RedoButton.target.color = new Color(1,1,1,0.15f);
		}
	}

	private void UpdateFunctionButtons()
	{
		m_MirrorButton.target.color = (VCEditor.s_Mirror.m_Mask > 0) ? new Color(1,1,1,1) : new Color(1,1,1,0.15f);
	}

	private void UpdateTransformTypeGroup()
	{
		bool selected_brush = false;
		VCEBrush[] selbrs = VCEditor.SelectedBrushes;
		foreach ( VCEBrush br in selbrs )
		{
			if ( br is VCESelectComponent )
				selected_brush = true;
		}
		m_TransformTypeGroup.SetActive(selected_brush);
		if ( selected_brush )
		{
			if ( m_TransformTypeMoveCheck.isChecked )
			{
				VCEditor.TransformType = EVCETransformType.Move;
			}
			else if ( m_TransformTypeRotateCheck.isChecked )
			{
				VCEditor.TransformType = EVCETransformType.Rotate;
			}
			else
			{
				VCEditor.TransformType = EVCETransformType.None;
			}
		}
		else
		{
			VCEditor.TransformType = EVCETransformType.None;
		}
	}
	
	private void DestroyBrushInst(ref GameObject brush_inst)
	{
		if ( brush_inst != null )
		{
			GameObject.Destroy(brush_inst);
			brush_inst = null;
		}
	}
	private void CreateBrushInst(ref GameObject brush_inst, GameObject brush_prefab)
	{
		if ( brush_inst == null )
		{
			brush_inst = GameObject.Instantiate(brush_prefab) as GameObject;
			brush_inst.transform.parent = VCEditor.Instance.m_BrushGroup.transform;
			brush_inst.transform.localPosition = Vector3.zero;
			brush_inst.transform.localRotation = Quaternion.identity;
			brush_inst.transform.localScale = Vector3.one;
			brush_inst.SetActive(true);
		}
	}
	
	private void UpdateComponentBrushes ()
	{
		if ( m_PartTab.isChecked )
		{
			if ( VCEditor.SelectedPart != null )
			{
				DestroyBrushInst(ref m_SelectComponentBrushInst);
				CreateBrushInst(ref m_DrawPartBrushInst, m_DrawPartBrush);
			}
			else
			{
				DestroyBrushInst(ref m_DrawPartBrushInst);
				CreateBrushInst(ref m_SelectComponentBrushInst, m_SelectComponentBrush);
			}
		}
		else if ( m_DecalTab.isChecked )
		{
			if ( VCEditor.SelectedDecal != null )
			{
				DestroyBrushInst(ref m_SelectComponentBrushInst);
				CreateBrushInst(ref m_DrawDecalBrushInst, m_DrawDecalBrush);
			}
			else
			{
				DestroyBrushInst(ref m_DrawDecalBrushInst);
				CreateBrushInst(ref m_SelectComponentBrushInst, m_SelectComponentBrush);
			}
		}
		else if ( m_EffectTab.isChecked )
		{
			CreateBrushInst(ref m_SelectComponentBrushInst, m_SelectComponentBrush);
		}
	}

	bool first_in_mat_tab = true;
	private void UpdateSimpleLabels ()
	{
		if ( Time.frameCount % 30 == 0 )
		{
			m_FileLabel.text = "File".ToLocalizationString() + ":  " + VCEditor.s_Scene.m_DocumentPath.Substring(0, 
				VCEditor.s_Scene.m_DocumentPath.Length - VCConfig.s_IsoFileExt.Length) + "  " + 
				(File.Exists(VCConfig.s_IsoPath + VCEditor.s_Scene.m_DocumentPath) ? "  " : "[FFFF00][" + "NEW".ToLocalizationString() + "][-]  ") +
				(VCEHistory.s_Modified ? "[FFFF00][" + "Modified".ToLocalizationString() + "][-]" : "");

			if (first_in_mat_tab && m_MaterialTab.isChecked)
			{
				m_RefPlaneTip.Show();
				first_in_mat_tab = false;
			}
		}
	}
	
	private void UpdateFactoryGroups ()
	{
		if ( m_MaterialTab.isChecked )
		{
			VCEditor.Instance.m_PartGroup.SetActive(!m_HideParts_Material.isChecked);
			VCEditor.Instance.m_DecalGroup.SetActive(true);
			VCEditor.Instance.m_EffectGroup.SetActive(!m_HideParts_Material.isChecked);
		}
		else if ( m_PaintTab.isChecked )
		{
			VCEditor.Instance.m_PartGroup.SetActive(!m_HideParts_Color.isChecked);
			VCEditor.Instance.m_DecalGroup.SetActive(true);
			VCEditor.Instance.m_EffectGroup.SetActive(!m_HideParts_Color.isChecked);
		}
		else
		{
			VCEditor.Instance.m_PartGroup.SetActive(true);
			VCEditor.Instance.m_DecalGroup.SetActive(true);
			VCEditor.Instance.m_EffectGroup.SetActive(true);
		}
	}
	
	private void UpdateSceneGizmo ()
	{
		if ( m_RightBox.tweenFactor >= 0.99f && VCEditor.s_Ready )
		{
			m_SceneGizmo.gameObject.SetActive(true);
			m_SceneGizmo.m_RenderRect.x = m_RightBox.transform.localPosition.x + m_SceneGizmoOffset.x + Screen.width;
			m_SceneGizmo.m_RenderRect.y = Screen.height - m_SceneGizmoOffset.y;
			m_SceneGizmo.m_RenderRect.width = 100;
			m_SceneGizmo.m_RenderRect.height = 100;
		}
		else
		{
			m_SceneGizmo.gameObject.SetActive(false);
		}
	}

	CreationAttr attr = new CreationAttr();
	private void UpdateErrorLabel ()
	{
		if (Time.frameCount % 60 == 0)
		{
			if (VCEditor.s_Scene != null)
			{
				VCIsoData isodata = VCEditor.s_Scene.m_IsoData;
				attr.m_Errors.Clear();
				attr.m_Warnings.Clear();
				CreationData.CalcCreationType(isodata, attr);

				string errortext = "";
				if (isodata.m_Voxels.Count + isodata.m_Components.Count == 0)
				{
					errortext = "";
				}
				else
				{
					if ( attr.m_Errors.Count + attr.m_Warnings.Count > 0 )
					{
						errortext += "You must fix the issue(s) before you can export".ToLocalizationString();
					}
					else
					{
						errortext = "This ISO is ready to export".ToLocalizationString() + " !";
						if (firstvalid)
						{
							firstvalid = false;
							m_ExportTip.Show();
						}
					}

					if ( attr.m_Errors.Count > 0 )
					{
						errortext += "\r\n[FF0000]\r\n";
						foreach ( string e in attr.m_Errors )
						{
							errortext += (" -  " + e + "\r\n");
						}
						errortext += "[-]";
					}
					if ( attr.m_Warnings.Count > 0 )
					{
						errortext += "\r\n[FFFF00]\r\n";
						foreach ( string w in attr.m_Warnings )
						{
							errortext += (" -  " + w + "\r\n");
						}
						errortext += "[-]";
					}
				}
				m_ErrorLabel.text = errortext;
			}
		}
	}
	
	public VCEUIColorPickWnd ShowColorPickWindow(Transform parent, Vector3 localPos, Color color)
	{
		GameObject wnd = GameObject.Instantiate(m_ColorPickWindowRes) as GameObject;
		wnd.transform.parent = parent;
		wnd.transform.localScale = Vector3.one;
		wnd.transform.localPosition = localPos;
		wnd.name = m_ColorPickWindowRes.name;
		VCEUIColorPickWnd wndcomp = wnd.GetComponent<VCEUIColorPickWnd>();
		wndcomp.m_ColorPick.FinalColor = color;
		wndcomp.m_ColorPick.UpdateUI();
		wnd.SetActive(true);
		return wndcomp;
	}
	
	public void OnHidePartsClick(bool isChecked)
	{
		VCEditor.Instance.m_PartGroup.SetActive(!isChecked);
		VCEditor.Instance.m_EffectGroup.SetActive(!isChecked);
	}

	public void OnLaserGridClick(bool isChecked)
	{
		m_ShowLaserGrid = isChecked;
	}

	public void OnPartTab(bool isChecked)
	{
		if ( isChecked )
		{
			VCEditor.Instance.m_VoxelSelection.ClearSelection();
			VCEditor.Instance.m_NearVoxelIndicator.enabled = false;
			VCEStatusBar.ShowText("Add parts".ToLocalizationString(), 5, true);
		}
		else
		{
			VCEditor.DeselectBrushes();
			VCEditor.SelectedPart = null;
			DestroyBrushInst(ref m_SelectComponentBrushInst);
			DestroyBrushInst(ref m_DrawPartBrushInst);
		}
	}
	
	public void OnMaterialTab(bool isChecked)
	{
		if ( isChecked )
		{
			VCEditor.Instance.m_NearVoxelIndicator.enabled = true;
			//VCEStatusBar.ShowTextLowPriority("Draw voxel blocks", 5, true);
		}
		else
		{
			VCEditor.DeselectBrushes();
			VCEditor.SelectedMaterial = null;
			m_MaterialWindow.HideWindow();
		}
	}
	
	public void OnPaintTab(bool isChecked)
	{
		if ( isChecked )
		{
			VCEditor.Instance.m_NearVoxelIndicator.enabled = true;
			VCEditor.Instance.m_GLGroup.SetActive(false);
			VCEStatusBar.ShowText("Paint voxel blocks".ToLocalizationString(), 5, true);
		}
		else
		{
			VCEditor.DeselectBrushes();
			if ( VCEditor.s_Ready )
				VCEditor.Instance.m_GLGroup.SetActive(true);
		}
	}
	
	public void OnDecalTab(bool isChecked)
	{
		if ( isChecked )
		{
			VCEditor.Instance.m_VoxelSelection.ClearSelection();
			VCEditor.Instance.m_NearVoxelIndicator.enabled = false;
			VCEStatusBar.ShowText("Add decals".ToLocalizationString(), 5, true);
		}
		else
		{
			VCEditor.DeselectBrushes();
			VCEditor.SelectedDecal = null;
			DestroyBrushInst(ref m_SelectComponentBrushInst);
			DestroyBrushInst(ref m_DrawDecalBrushInst);
			m_DecalWindow.HideWindow();
		}
	}
	
	public void OnEffectTab(bool isChecked)
	{
		if ( isChecked )
		{
			VCEditor.Instance.m_VoxelSelection.ClearSelection();
			VCEditor.Instance.m_NearVoxelIndicator.enabled = false;
			VCEStatusBar.ShowText("Add effects".ToLocalizationString(), 5, true);
		}
		else
		{
			VCEditor.DeselectBrushes();
			DestroyBrushInst(ref m_SelectComponentBrushInst);
		}
	}
	
	public void OnISOTab(bool isChecked)
	{
		if ( isChecked )
		{
			VCEditor.Instance.m_MainCamera.GetComponent<VCEGL>().enabled = false;
			m_IsoList.enabled = true;
			m_IsoList.CreateInspector();
			m_IsoList.RefreshIsoList();
			VCEStatusBar.ShowText("ISO Browser".ToLocalizationString(), 5, true);
		}
		else
		{
			VCEditor.Instance.m_MainCamera.GetComponent<VCEGL>().enabled = true;
			m_IsoList.enabled = false;
			m_IsoList.DestroyInspector();
		}
	}
	
	public void OnInspectorTab(bool isChecked)
	{
		
	}
	public void OnStatTab(bool isChecked)
	{
		if ( isChecked )
		{
			m_StatPanel.OnCreationInfoRefresh();
		}
	}
	
	public void OnUndoClick()
	{
		if ( VCEditor.s_ProtectLock0 )
			return;
		if ( VCEHistory.Undo() )
		{
			VCEditor.ClearSelections();
			VCEditor.SelectedMaterial = VCEditor.SelectedMaterial;
			VCEStatusBar.ShowText("Undo".ToLocalizationString(), 2);
		}
	}
	public void OnRedoClick()
	{
		if ( VCEditor.s_ProtectLock0 )
			return;
		if ( VCEHistory.Redo() )
		{
			VCEditor.ClearSelections();
			VCEditor.SelectedMaterial = VCEditor.SelectedMaterial;
			VCEStatusBar.ShowText("Redo".ToLocalizationString(), 2);
		}
	}
	public void OnDeleteClick()
	{
		if ( VCEditor.s_ProtectLock0 )
			return;
		VCEBrush[] brs = VCEditor.Instance.m_BrushGroup.GetComponentsInChildren<VCEBrush>();
		foreach ( VCEBrush br in brs )
		{
			if ( br is VCESelectComponent )
			{
				VCESelectComponent sel = br as VCESelectComponent;
				sel.DeleteSelection();
			}
			else if ( br is VCESelectVoxel )
			{
				VCESelectVoxel sel = br as VCESelectVoxel;
				sel.DeleteSelection();
			}
		}
	}
	public void OnSaveClick()
	{
		if ( VCEditor.s_ProtectLock0 )
			return;
		VCEditor.ClearSelections();
		m_StatPanel.OnCreationInfoRefresh();
		if ( File.Exists(VCConfig.s_IsoPath + VCEditor.s_Scene.m_DocumentPath) )
		{
			if ( VCEditor.s_Scene.SaveIso() )
				VCEMsgBox.Show(VCEMsgBoxType.SAVE_OK);
			else
				VCEMsgBox.Show(VCEMsgBoxType.SAVE_FAILED);
			if ( m_ISOTab.isChecked )
				m_IsoList.RefreshIsoList();
		}
		else
		{
			OnSaveAsClick();
		}
	}
	public void OnSaveAsClick()
	{
		if ( VCEditor.s_ProtectLock0 )
			return;
		VCEditor.ClearSelections();
		m_StatPanel.OnCreationInfoRefresh();
		m_SaveWindow.Show(VCEditor.s_Scene.m_IsoData);
	}
	public void OnExportClick()
	{
		if ( VCEditor.s_ProtectLock0 )
			return;
		VCEditor.ClearSelections();
		m_StatPanel.OnCreationInfoRefresh();
		if ( !File.Exists(VCConfig.s_IsoPath + VCEditor.s_Scene.m_DocumentPath) )
		{
			VCEMsgBox.Show(VCEMsgBoxType.EXPORT_NOTSAVED);
			return;
		}
		m_ExportWindow.Show();
	}
	public void OnMirrorClick()
	{
		if ( VCEditor.s_ProtectLock0 )
			return;
		if ( VCEditor.s_Mirror.m_Mask == 0 )
		{
			if ( m_MirrorWindow.WindowVisible() )
				m_MirrorWindow.HideWindow();
		}
		else
		{
			if ( m_MirrorWindow.WindowVisible() )
				m_MirrorWindow.HideWindow();
			else
				m_MirrorWindow.ShowWindow();
		}
	}

	public void OnTutorialClick()
	{
		if ( VCEditor.s_ProtectLock0 )
			return;
		if ( TutorMgr.TutorLoaded() )
			TutorMgr.Destroy();
		else
			TutorMgr.Load();
	}

	public void OnQuitClick()
	{
		if ( VCEditor.s_ProtectLock0 )
			return;
		if ( VCEHistory.s_Modified )
			VCEMsgBox.Show(VCEMsgBoxType.CLOSE_QUERY);
		else
			VCEditor.Quit();
	}

    #region lz-2016.09.29 鼠标进入UI的时候禁用鼠标操作创建系统相机

    //lz-2016.09.30  禁用鼠标操作创建系统相机操作
    private void DisableVCECameraOperation(bool disable)
    {
        VCECamera.Instance.CanMove = !disable;
        VCECamera.Instance.CanOrbit = !disable;
        VCECamera.Instance.CanZoom = !disable;
    }

    //lz-2016.09.30  检测鼠标进入是否在UI上，来启用或禁用创建系统相机操作
    private void CheckHoverInUI()
    {
        DisableVCECameraOperation(UICamera.hoveredObject != null 
            && UICamera.hoveredObject.layer == Pathea.Layer.VCEGUI
            && UICamera.hoveredObject.name!= "SaveWndMask"); //lz-2016.09.29 这个需要旋转相机拍照
    }
    #endregion
}
