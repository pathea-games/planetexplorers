using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class VCEUIFastCreateWnd : MonoBehaviour
{
	public GameObject m_Window;
	public UIEventListener m_BackButton;
	public UIEventListener m_CloseButton;
	public VCEUICreationTypeItem m_ItemPrefab;

	int parentId = 0;

	// Use this for initialization
	void OnEnable ()
	{
		m_BackButton.onClick += OnBackClick;
		m_CloseButton.onClick += OnCloseClick;
		CreateChildItems();
	}

	void OnDisable ()
	{
		m_BackButton.onClick -= OnBackClick;
		m_CloseButton.onClick -= OnCloseClick;

		parentId = 0;
		foreach (var item in items)
		{
			item.FadeOut();
			GameObject.Destroy(item.gameObject);
		}
		items.Clear();
		m_BackButton.gameObject.SetActive(false);
	}

	void OnDestroy()
	{

	}

	public bool WindowVisible ()
	{
		return m_Window.activeInHierarchy;
	}
	
	public void ShowWindow ()
	{
		m_Window.SetActive(true);
	}

	public void HideWindow ()
	{
		m_Window.SetActive(false);
	}

	public void OnBackClick (GameObject sender)
	{
		if (parentId != 0)
			parentId = VCConfig.s_EditorScenes.Find(iter => iter.m_Id == parentId).m_ParentId;
		if (parentId == 0)
			m_BackButton.gameObject.SetActive(false);
		CreateChildItems();
	}

	public void OnCloseClick (GameObject sender)
	{
		HideWindow();
	}

	public void OnItemClick (GameObject sender)
	{
		VCEUICreationTypeItem sender_item = sender.GetComponentInParent<VCEUICreationTypeItem>();
		int usage = sender_item.usage;
		VCESceneSetting scene = sender_item.m_Scene;

		// 树形菜单
		if (usage == 0)
		{
			parentId = scene.m_Id;
			m_BackButton.gameObject.SetActive(true);
			CreateChildItems();
		}
		// 创建新的ISO
		else if (usage == 1)
		{
			parentId = scene.m_Id;
			m_BackButton.gameObject.SetActive(true);
			CreateChildItems();
		}
		// 加载旧的ISO
		else if (usage == 2)
		{
			OnCloseClick(gameObject);
			VCEditor.Instance.m_UI.m_ISOTab.isChecked = true;
			VCEditor.Instance.m_UI.m_IsoTip.Show();
		}
		// Template
		else if (usage == 3)
		{
			TextAsset asset = Resources.Load<TextAsset>("Isos/" + scene.m_Id.ToString() + "/index");
			if (asset == null)
			{
				OnCloseClick(gameObject);
				VCEditor.NewScene(scene);
				return;
			}
			int count = 0;
			int.TryParse(asset.text, out count);
			if (count == 0)
			{
				OnCloseClick(gameObject);
				VCEditor.NewScene(scene);
				return;
			}
			int tmpIdx = (int)(Random.value * count - 0.00001f);
			OnCloseClick(gameObject);
			VCEditor.NewScene(scene, tmpIdx);
		}
		// Empty
		else if (usage == 4)
		{
			OnCloseClick(gameObject);
			VCEditor.NewScene(scene);
		}
	}

	List<VCEUICreationTypeItem> items = new List<VCEUICreationTypeItem>();
	public void CreateChildItems ()
	{
		foreach (var item in items)
		{
			item.FadeOut();
		}
		items.Clear();

		if (parentId > 0)
		{
			foreach (var scene in VCConfig.s_EditorScenes)
			{
				if (scene.m_ParentId == parentId)
				{
					VCEUICreationTypeItem item = VCEUICreationTypeItem.Instantiate(m_ItemPrefab);
					item.m_Scene = scene;
					item.transform.parent = m_ItemPrefab.transform.parent;
					item.transform.localScale = m_ItemPrefab.transform.localScale;
					item.gameObject.name = scene.m_Name;
					item.gameObject.SetActive(true);
					item.FadeIn();
					items.Add(item);
				}
			}
		}
		else
		{
			VCEUICreationTypeItem item = VCEUICreationTypeItem.Instantiate(m_ItemPrefab);
			item.m_Scene = VCConfig.s_EditorScenes.Find(iter => iter.m_Id == 1);
			item.usage = 1;
			item.transform.parent = m_ItemPrefab.transform.parent;
			item.transform.localScale = m_ItemPrefab.transform.localScale;
			item.m_NameLabel.text = "New".ToLocalizationString();
			item.gameObject.name = "New".ToLocalizationString();
			item.gameObject.SetActive(true);
			item.FadeIn();
			items.Add(item);

			item = VCEUICreationTypeItem.Instantiate(m_ItemPrefab);
			item.usage = 2;
			item.m_Scene = null;
			item.transform.parent = m_ItemPrefab.transform.parent;
			item.transform.localScale = m_ItemPrefab.transform.localScale;
			item.m_NameLabel.text = "Open".ToLocalizationString();
			item.gameObject.name = "Open".ToLocalizationString();
			item.gameObject.SetActive(true);
			item.FadeIn();
			items.Add(item);
		}

		if (items.Count == 0 && parentId > 0)
		{
			VCEUICreationTypeItem item = VCEUICreationTypeItem.Instantiate(m_ItemPrefab);
			item.m_Scene = VCConfig.s_EditorScenes.Find(iter => iter.m_Id == parentId);
			item.usage = 3;
			item.transform.parent = m_ItemPrefab.transform.parent;
			item.transform.localScale = m_ItemPrefab.transform.localScale;
			item.m_NameLabel.text = "Template".ToLocalizationString();
			item.gameObject.name = "Template".ToLocalizationString();
			item.gameObject.SetActive(true);
			item.FadeIn();
			items.Add(item);

			item = VCEUICreationTypeItem.Instantiate(m_ItemPrefab);
			item.m_Scene = VCConfig.s_EditorScenes.Find(iter => iter.m_Id == parentId);
			item.usage = 4;
			item.transform.parent = m_ItemPrefab.transform.parent;
			item.transform.localScale = m_ItemPrefab.transform.localScale;
			item.m_NameLabel.text = "Empty".ToLocalizationString();
			item.gameObject.name = "Empty".ToLocalizationString();
			item.gameObject.SetActive(true);
			item.FadeIn();
			items.Add(item);
		}

		if (items.Count == 1)
		{
			items[0].transform.localPosition = new Vector3(-100, 100);
		}
		else if (items.Count == 2)
		{
			items[0].transform.localPosition = new Vector3(-200, 100);
			items[1].transform.localPosition = new Vector3(0, 100);
		}
		else if (items.Count == 3)
		{
			items[0].transform.localPosition = new Vector3(-300, 100);
			items[1].transform.localPosition = new Vector3(-100, 100);
			items[2].transform.localPosition = new Vector3(100, 100);
		}
		else if (items.Count == 4)
		{
			items[0].transform.localPosition = new Vector3(-200, 200);
			items[1].transform.localPosition = new Vector3(0, 200);
			items[2].transform.localPosition = new Vector3(-200, 0);
			items[3].transform.localPosition = new Vector3(0, 0);
		}
		else if (items.Count == 5)
		{
			items[0].transform.localPosition = new Vector3(-300, 200);
			items[1].transform.localPosition = new Vector3(-100, 200);
			items[2].transform.localPosition = new Vector3(100, 200);
			items[3].transform.localPosition = new Vector3(-200, 0);
			items[4].transform.localPosition = new Vector3(0, 0);
		}
		else if (items.Count == 6)
		{
			items[0].transform.localPosition = new Vector3(-300, 200);
			items[1].transform.localPosition = new Vector3(-100, 200);
			items[2].transform.localPosition = new Vector3(100, 200);
			items[3].transform.localPosition = new Vector3(-300, 0);
			items[4].transform.localPosition = new Vector3(-100, 0);
			items[5].transform.localPosition = new Vector3(100, 0);
		}
		else if (items.Count == 7)
		{
			items[0].transform.localPosition = new Vector3(-400, 200);
			items[1].transform.localPosition = new Vector3(-200, 200);
			items[2].transform.localPosition = new Vector3(0, 200);
			items[3].transform.localPosition = new Vector3(200, 200);
			items[4].transform.localPosition = new Vector3(-300, 0);
			items[5].transform.localPosition = new Vector3(-100, 0);
			items[6].transform.localPosition = new Vector3(100, 0);
		}
		else if (items.Count == 8)
		{
			items[0].transform.localPosition = new Vector3(-400, 200);
			items[1].transform.localPosition = new Vector3(-200, 200);
			items[2].transform.localPosition = new Vector3(0, 200);
			items[3].transform.localPosition = new Vector3(200, 200);
			items[4].transform.localPosition = new Vector3(-400, 0);
			items[5].transform.localPosition = new Vector3(-200, 0);
			items[6].transform.localPosition = new Vector3(0, 0);
			items[7].transform.localPosition = new Vector3(200, 0);
		}
	}
}
