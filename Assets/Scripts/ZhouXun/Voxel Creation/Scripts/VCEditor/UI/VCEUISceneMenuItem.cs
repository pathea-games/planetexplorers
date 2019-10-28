using UnityEngine;
using System.Collections;

// Scene menu item, (top item or popup item)
// Contains its child popup menu
public class VCEUISceneMenuItem : MonoBehaviour
{
	// Parent Item
	public VCEUISceneMenuItem m_ParentMenuItem;
	// Attached Scene setting
	public VCESceneSetting m_SceneSetting;
	// Widgets
	public UILabel m_Label = null;
	public GameObject m_PopupMenuBg = null;
	public UIGrid m_PopupMenuItemGroup = null;
	public GameObject m_PopupMenuSpriteTriangle = null;
	// Child popup menu item
	public string m_PopupMenuItemPrefabRes = "GUI/Prefabs/Scene popup menu item";
	public int m_ChildMenuCount = 0;
	// Tween effect
	public UIButtonTween m_PopupTween = null;
	
	// Use this for initialization
	void Start ()
	{
		// Label text
		m_Label.text = m_SceneSetting.m_Name.ToLocalizationString();
		
		// Create Child popup menu items
		// like a tree structure
		foreach ( VCESceneSetting scene in VCConfig.s_EditorScenes )
		{
			if ( scene.m_ParentId == m_SceneSetting.m_Id )
			{
				GameObject go = GameObject.Instantiate(Resources.Load(m_PopupMenuItemPrefabRes) as GameObject) as GameObject;
				Vector3 scale = go.transform.localScale;
				go.transform.parent = m_PopupMenuItemGroup.transform;
				go.transform.localPosition = Vector3.zero;
				go.transform.localScale = scale;
				go.name = "Scene " + scene.m_Id.ToString("00");
				VCEUISceneMenuItem smitem = go.GetComponent<VCEUISceneMenuItem>();
				smitem.m_SceneSetting = scene;
				smitem.m_ParentMenuItem = this;
				go.SetActive(true);
				m_ChildMenuCount++;
			}
		}
		// Calc bg's height
		if ( m_PopupMenuBg != null )
		{
			Vector3 oldScale = m_PopupMenuBg.transform.localScale;
			oldScale.y = m_PopupMenuItemGroup.cellHeight * m_ChildMenuCount + 7;
			m_PopupMenuBg.transform.localScale = oldScale;
		}
		// Reposition
		m_PopupMenuItemGroup.Reposition();
		// Enable/Disable child popup menu
		if ( m_ChildMenuCount > 0 )
		{
			EnablePopupMenu();
		}
		else
		{
			DisablePopupMenu();
		}
	}
	
	// Some private vars for popup menu's behaviour
	private bool bShouldExpand = false;
	private int ExpandState = 0;
	private float ParentExpandFactor = 0;
	private float notExpandTime = 0;
	private float ExpandingTime = 0;
	// Update is called once per frame
	void Update ()
	{
		// Calc bExpanded
		if ( m_ParentMenuItem != null )
			ParentExpandFactor = (m_ParentMenuItem.m_PopupTween.tweenTarget.transform.localScale - m_ParentMenuItem.m_PopupTween.tweenTarget.GetComponent<TweenScale>().from).magnitude;
		else
			ParentExpandFactor = 1;
		if ( m_PopupTween.tweenTarget.transform.localScale == m_PopupTween.tweenTarget.GetComponent<TweenScale>().from )
		{
			ExpandState = 0;
		}
		else if ( m_PopupTween.tweenTarget.transform.localScale == m_PopupTween.tweenTarget.GetComponent<TweenScale>().to )
		{
			ExpandState = 1;
		}
		else
		{
			ExpandState = -1;
		}
		if ( ExpandingTime < 0.1f )
			ExpandState = -1;
		notExpandTime += Time.deltaTime;
		ExpandingTime += Time.deltaTime;
		
		// Calc bShouldExpand
		if ( UICamera.selectedObject == null )
		{
			bShouldExpand = false;
		}
		else if ( notExpandTime < 0.3f )
		{
			bShouldExpand = false;
			UICamera.selectedObject = null;
		}
		else if ( ExpandState == -1 )
		{
			// No change
		}
		else
		{
			if ( this.m_PopupTween.gameObject == UICamera.selectedObject )
				bShouldExpand = true;
			else
				bShouldExpand = false;
			VCEUISceneMenuItem[] child_items = GetComponentsInChildren<VCEUISceneMenuItem>(true);
			foreach ( VCEUISceneMenuItem item in child_items )
			{
				if ( item.m_PopupTween.gameObject == UICamera.selectedObject )
				{
					bShouldExpand = true;
				}
			}
		}
		
		// Handle expanding
		if ( bShouldExpand && ExpandState == 0 )
		{
			m_PopupTween.Play(true);
			ExpandingTime = 0;
		}
		if ( !bShouldExpand && ExpandState == 1 )
		{
			m_PopupTween.Play(false);
			ExpandingTime = 0;
		}
		
		// Color changing
		if ( m_PopupMenuSpriteTriangle != null )
		{
			if ( bShouldExpand )
			{
				m_PopupMenuSpriteTriangle.GetComponent<UISprite>().color = Color.yellow;
				m_Label.color = new Color(1.0f, 1.0f, 0.0f, ParentExpandFactor);
			}
			else
			{
				m_PopupMenuSpriteTriangle.GetComponent<UISprite>().color = Color.white;
				m_Label.color = new Color(1.0f, 1.0f, 1.0f, ParentExpandFactor);
			}
		}
	}
	// Hover event for popup menu's behaviour
	public void OnBtnHover()
	{
		if ( notExpandTime > 0.3f )
			UICamera.selectedObject = this.m_PopupTween.gameObject;
	}
	
	// Enable/Disable popup menu
	public void EnablePopupMenu()
	{
		if ( m_PopupMenuBg != null )
		{
			m_PopupMenuBg.SetActive(true);
		}
		if ( m_PopupMenuItemGroup != null )
		{
			m_PopupMenuItemGroup.gameObject.SetActive(true);
		}
		if ( m_PopupMenuSpriteTriangle != null )
		{
			m_PopupMenuSpriteTriangle.SetActive(true);
		}
	}
	public void DisablePopupMenu()
	{
		if ( m_PopupMenuBg != null )
		{
			m_PopupMenuBg.SetActive(false);
		}
		if ( m_PopupMenuItemGroup != null )
		{
			m_PopupMenuItemGroup.gameObject.SetActive(false);
		}
		if ( m_PopupMenuSpriteTriangle != null )
		{
			m_PopupMenuSpriteTriangle.SetActive(false);
		}
	}
	
	// The menu item CLICK event
	public static VCESceneSetting s_SceneToCreate = null;
	public static void DoCreateSceneFromMsgBox()
	{
        if (s_SceneToCreate == null)
            s_SceneToCreate = VCConfig.FirstSceneSetting;

        VCEditor.NewScene(s_SceneToCreate);
	}
	public void OnBtnClick()
	{
		if ( m_SceneSetting.m_Category == EVCCategory.cgAbstract )
			return;
		UICamera.selectedObject = null;
		notExpandTime = 0;
		if ( VCEHistory.s_Modified )
		{
			s_SceneToCreate = m_SceneSetting;
			VCEMsgBox.Show(VCEMsgBoxType.SWITCH_QUERY);
		}
		else
		{
			VCEditor.NewScene(m_SceneSetting);
		}
	}
}
