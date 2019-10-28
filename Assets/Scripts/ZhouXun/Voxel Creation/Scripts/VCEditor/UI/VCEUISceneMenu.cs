using UnityEngine;
using System.Collections;

public class VCEUISceneMenu : MonoBehaviour
{
	// Top Menu item resource
	public GameObject m_ItemPrefab;
	
	// Initialize, Top Menu item creation
	void Start ()
	{
		foreach ( VCESceneSetting scene in VCConfig.s_EditorScenes )
		{
			if ( scene.m_ParentId == 0 )
			{
				GameObject go = GameObject.Instantiate(m_ItemPrefab) as GameObject;
				Vector3 scale = go.transform.localScale;
				go.transform.parent = transform;
				go.transform.localPosition = Vector3.zero;
				go.transform.localScale = scale;
				go.name = "Scene " + scene.m_Id.ToString("00");
				go.GetComponent<VCEUISceneMenuItem>().m_SceneSetting = scene;
				go.SetActive(true);
			}
		}
		GetComponent<UIGrid>().Reposition();
	}
}
