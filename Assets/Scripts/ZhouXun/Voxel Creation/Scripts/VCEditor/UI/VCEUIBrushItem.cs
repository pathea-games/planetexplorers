using UnityEngine;
using System.Collections;

public class VCEUIBrushItem : MonoBehaviour
{
	public VCEUIBrushGroup m_Group;
	public bool m_IsGeneralBrush;
	public GameObject m_BrushPrefab;
	public GameObject m_BrushInstance;

	// Use this for initialization
	void Start ()
	{
	
	}
	
	// Update is called once per frame
	void Update ()
	{
		if ( m_BrushPrefab == null )
			return;
		if ( !VCEditor.DocumentOpen() )
			return;
		UICheckbox cbox = GetComponent<UICheckbox>();
		if ( cbox != null )
		{
			if ( cbox.isChecked )
			{
				if ( m_BrushInstance == null )
				{
					m_BrushInstance = GameObject.Instantiate(m_BrushPrefab) as GameObject;
					m_BrushInstance.transform.parent = VCEditor.Instance.m_BrushGroup.transform;
					m_BrushInstance.transform.localPosition = Vector3.zero;
					m_BrushInstance.transform.localRotation = Quaternion.identity;
					m_BrushInstance.transform.localScale = Vector3.one;
					m_BrushInstance.SetActive(true);
				}
			}
			else
			{
				if ( m_BrushInstance != null )
				{
					GameObject.Destroy(m_BrushInstance);
					m_BrushInstance = null;
				}
			}
		}
	}
	
	void OnEnable ()
	{
		UICheckbox cbox = GetComponent<UICheckbox>();
		if ( cbox != null )
		{
			cbox.isChecked = cbox.startsChecked;
		}
	}
	void OnDisable ()
	{
		UICheckbox cbox = GetComponent<UICheckbox>();
		if ( cbox != null )
		{
			cbox.isChecked = cbox.startsChecked;
		}
		if ( m_BrushInstance != null )
		{
			GameObject.Destroy(m_BrushInstance);
			m_BrushInstance = null;
		}
	}

	void OnBrushSelect (bool isChecked)
	{
		if ( isChecked )
		{
			if ( m_IsGeneralBrush )
			{
				m_Group.m_LastGeneralBrush = this;
			}
		}
	}
}
