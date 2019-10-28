using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UITreeGrid : MonoBehaviour
{
	[SerializeField] private bool m_RepositionNow = false;

	// line
	public bool m_DrawLine = false;
	public GameObject m_HorzLine;
	public GameObject m_VertLine;
	public float m_IconSize = 32;
	private List<GameObject> m_Lines = new List<GameObject> ();

	// settings
	public Transform m_Content;
	public float m_LineHeight = 60F;
	public float m_GridWidth = 80F;

	// children
	public List<UITreeGrid> m_Children = new List<UITreeGrid> ();

	// other info
	private int m_Expand = 0;
	public float TotalWidth { get { return m_Expand * m_GridWidth; } }

	// MonoBehaviour Update (Debug mode only)
	void Update ()
	{
		if ( m_RepositionNow )
		{
			m_RepositionNow = false;
			Reposition();
		}
	}

	// Content LOCAL position
	public Vector3 ContentPosition
	{
		get { return transform.localPosition + m_Content.localPosition; }
	}

	public void Reposition ()
	{
		// Reposition children
		foreach ( UITreeGrid grid in m_Children )
			grid.Reposition();

		// Postorder traversal

		// Calc expand
		m_Expand = 0;
		foreach ( UITreeGrid grid in m_Children )
			m_Expand += grid.m_Expand;
		m_Expand = Mathf.Max(m_Expand, 1);

		// Reposition children
		int sum = 0;
		foreach ( UITreeGrid grid in m_Children )
		{
			Vector3 _pos = grid.transform.localPosition;
			_pos.x = sum * m_GridWidth;
			grid.transform.localPosition = _pos;
			sum += grid.m_Expand;
		}

		// Reposition content
		if ( m_Content != null )
		{
			Vector3 _pos = Vector3.zero;
			int lst = m_Children.Count - 1;
			if ( lst >= 0 )
				_pos.x = (m_Children[0].ContentPosition.x + m_Children[lst].ContentPosition.x) * 0.5F;
			m_Content.localPosition = _pos;
		}

		// Reposition Y
		Vector3 pos = transform.localPosition;
		pos.y = -m_LineHeight;
		transform.localPosition = pos;

		// Reposition lines
		if ( m_DrawLine )
			CreateLine();
	}

	// Destroy all line objects
	void ClearLines ()
	{
		foreach ( GameObject line in m_Lines )
			GameObject.Destroy(line);
		m_Lines.Clear();
	}

	// Create a line object
	void DrawUILine (bool horz, Vector3 begin, Vector3 end)
	{
		GameObject line_res = horz ? m_HorzLine : m_VertLine;
		if ( line_res != null )
		{
			GameObject line = GameObject.Instantiate(line_res) as GameObject;
			line.transform.parent = this.transform;
			line.transform.localPosition = begin;
			line.transform.localRotation = Quaternion.identity;
			Vector3 scl = end - begin;
			if ( horz )
				scl.y = 2;
			else
				scl.x = 2;
			scl.z = 1;
			line.transform.localScale = scl;
			m_Lines.Add(line);
		}
	}

	// Create all line objects
	void CreateLine ()
	{
		ClearLines();
		int lst = m_Children.Count - 1;
		if ( lst >= 0 )
		{
			// 1
			DrawUILine(false, m_Content.localPosition + Vector3.down * m_LineHeight * 0.5F , 
			           m_Content.localPosition + Vector3.down * (m_IconSize * 0.5F + 1) );
			// 2
			DrawUILine(true, m_Children[0].ContentPosition + Vector3.up * m_LineHeight * 0.5F, 
			           m_Children[lst].ContentPosition + Vector3.up * m_LineHeight * 0.5F);
			// 3
			foreach ( UITreeGrid grid in m_Children )
				DrawUILine(false, grid.ContentPosition + Vector3.up * m_IconSize * 0.5F, 
				           grid.ContentPosition + Vector3.up * (m_LineHeight * 0.5F + 1));
		}
	}
}
