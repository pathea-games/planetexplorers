using UnityEngine;
using UnityEditor;
using System.Reflection;
using System;

[CustomEditor(typeof(UIMenuList), true)]
public class UIMenuListInspector : UIBaseInspector  
{
    private UIMenuList menuList;

	public override void OnEnable()
	{
		base.OnEnable();
		menuList = target as UIMenuList;
	}


	public override void OnInspectorGUI_Propertys()
	{
		// ----------------------------------Contents--------------------------------------------------------
		DrawPartLine("Contents");

		GameObject ItemPrefab = EditorGUILayout.ObjectField("UIMeunItemPrefab",menuList.UIMeunItemPrefab,typeof(GameObject), true, GUILayout.ExpandWidth(true)) as GameObject;
		if (ItemPrefab != menuList.UIMeunItemPrefab)
		{
			menuList.UIMeunItemPrefab = ItemPrefab;
		}
		UISlicedSprite SlicedSpriteBg = EditorGUILayout.ObjectField("SlicedSpriteBg",menuList.SlicedSpriteBg,typeof(UISlicedSprite), true, GUILayout.ExpandWidth(true)) as UISlicedSprite;
		if (SlicedSpriteBg != menuList.SlicedSpriteBg)
		{
			menuList.SlicedSpriteBg = SlicedSpriteBg;
		}

		GameObject ItemsContent = EditorGUILayout.ObjectField("ItemsContent",menuList.ItemsContent,typeof(GameObject), true, GUILayout.ExpandWidth(true)) as GameObject;
		if (ItemsContent != menuList.ItemsContent)
		{
			menuList.ItemsContent = ItemsContent;
		}
		GUILayout.Space(2);
		Vector4 margin = EditorGUILayout.Vector4Field("Margin",menuList.Margin);
		if (margin != menuList.Margin)
		{
			menuList.Margin = margin;
			menuList.UpdatePanelPositon();
		}
		GUILayout.Space(7);

		UIMenuPanel root = EditorGUILayout.ObjectField("rootPanel",menuList.rootPanel,typeof(UIMenuPanel), false, GUILayout.ExpandWidth(true)) as UIMenuPanel;
		if (root != menuList.rootPanel)
		{
			menuList.rootPanel = root;
		}

		// ----------------------------------items--------------------------------------------------------
		DrawPartLine("Items");
		GUILayout.Space(2);
	
		Vector2 v2 = EditorGUILayout.Vector2Field("Item Size",menuList.ItemSize);
		if (v2 != menuList.ItemSize)
		{
			menuList.ItemSize = v2;
			menuList.UpdatePanelPositon();
		}
		GUILayout.Space(2);

		Vector2 panelMargin = EditorGUILayout.Vector2Field("Panel Margin",menuList.PanelMargin);
		if (panelMargin != menuList.PanelMargin)
		{
			menuList.PanelMargin = panelMargin;
			menuList.UpdatePanelPositon();
		}


		EditorGUILayout.BeginHorizontal();
		GUILayout.Label("Add Item (Input a item text)");
		string strText = EditorGUILayout.TextField("");
		GUI.backgroundColor = Color.green;
		if (GUILayout.Button(insertContent,min_buttonWidth,min_buttonHeight))
		{
			if (strText.Length == 0)
				strText = "item_" +  menuList.Items.Count.ToString();
            menuList.AddItem(null,strText,UIGameMenuCtrl.MenuItemFlag.Flag_Null);
		}
		EditorGUILayout.EndHorizontal();



		for (int i=0;i<menuList.panels.Count;i++)
		{
			string name = (menuList.panels[i].parent == null) ? " root" : " " + menuList.panels[i].parent.Text;
			DrawPartLine(name);
			DrawMenuPanel(menuList.panels[i].parent);
		}

	}


	public override void OnInspectorGUI_Events()
	{
	
		FieldInfo[]	infos;
		infos = typeof(UIMenuList).GetFields();

		DrawConpomentEvents(infos,menuList); //统一处理
	}


	
	void DrawMenuPanel(UIMenuListItem parent)
	{
		for (int i=0;i<menuList.Items.Count;i++)
		{

			if (menuList.Items[i].Parent == parent)
			{
				EditorGUILayout.BeginHorizontal();
				
				GUI.color = Color.yellow;
				EditorGUIUtility.labelWidth = 20;
				GUILayout.Label("  " + i.ToString() + ": ");
				GUI.color = Color.white;
				
				if (menuList.Items[i] != null)
				{
					
					EditorGUIUtility.labelWidth = 60;
					UIMenuListItem item = EditorGUILayout.ObjectField("Parent:",menuList.Items[i].Parent, typeof(GameObject),false,GUILayout.ExpandWidth(true)) as UIMenuListItem;
					if (item != menuList.Items[i].Parent)
					{
						menuList.Items[i].Parent = item;
					}
					
					EditorGUIUtility.labelWidth = 40;
					string text = EditorGUILayout.TextField("Text:",menuList.Items[i].Text, GUILayout.ExpandWidth(true));
					if (text != menuList.Items[i].Text)
					{
						menuList.Items[i].Text = text;
					}
				}
				
				GUI.backgroundColor = Color.green;
				
				if (GUILayout.Button(insertContent,min_buttonWidth,min_buttonHeight))
				{
					string strText = "item_" +  menuList.Items.Count.ToString();
                    menuList.AddItem(i, strText, UIGameMenuCtrl.MenuItemFlag.Flag_Null);
				}
				
				if (GUILayout.Button(deleteContent,min_buttonWidth,min_buttonHeight))
				{
					menuList.DeleteItem(i);
				}
				
				EditorGUILayout.EndHorizontal();
			}
		}
	}
	
}


