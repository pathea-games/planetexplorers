using UnityEngine;
using System.Collections;

public class UIScrollBox : MonoBehaviour
{
	public UISprite m_BackgroundSprite;
	public UIPanel m_DraggablePanel;
	public UIScrollBar m_VertScrollBar;
	public UIScrollBar m_HorzScrollBar;
	public BoxCollider m_DragContent;
	public int m_Width;
	public int m_Height;
	public bool m_UpdateSize = false;

	private Vector3 m_TmpDragScale;
	private UIDraggablePanel m_Drag;


	// Use this for initialization
	void Start ()
	{
		Reposition();
		if ( m_DraggablePanel )
		{
			m_Drag = m_DraggablePanel.GetComponent<UIDraggablePanel>();
			m_TmpDragScale = m_Drag.scale;
			m_DraggablePanel.transform.localPosition = new Vector3(0,0,-2);
		}
	}
	
	// Update is called once per frame
	void Update ()
	{
		if ( m_UpdateSize )
		{
			Reposition();
		}
		if ( m_DragContent != null )
		{
			m_DragContent.center = new Vector3 (m_Width*0.5f+10-m_DraggablePanel.transform.localPosition.x,
			                                    -m_Height*0.5f-10-m_DraggablePanel.transform.localPosition.y, 0);
			m_DragContent.size = new Vector3 (m_Width, m_Height, 0);
		}
	}

	void LateUpdate ()
	{
		if ( m_DraggablePanel != null )
		{
			if ( m_Drag != null )
			{
				m_Drag.scale = m_TmpDragScale;
				if ( m_VertScrollBar != null )
				{
					if ( m_VertScrollBar.background.gameObject.activeSelf == false )
					{
						Vector3 tmp_pos = m_DraggablePanel.transform.localPosition;
						tmp_pos.y = 0;
						m_DraggablePanel.transform.localPosition = tmp_pos;
						m_Drag.scale.y = 0;
					}
				}
				if ( m_HorzScrollBar != null )
				{
					if ( m_HorzScrollBar.background.gameObject.activeSelf == false )
					{
						Vector3 tmp_pos = m_DraggablePanel.transform.localPosition;
						tmp_pos.x = 0;
						m_DraggablePanel.transform.localPosition = tmp_pos;
						m_Drag.scale.x = 0;
					}
				}
			}
		}
	}

	public void Reposition ()
	{
		if ( m_BackgroundSprite != null )
		{
			m_BackgroundSprite.transform.localScale = new Vector3 (m_Width + 20, m_Height + 20, 1);
		}
		if ( m_DraggablePanel != null )
		{
			Vector3 pos = m_DraggablePanel.transform.localPosition;
			pos.x = 0;
			m_DraggablePanel.transform.localPosition = pos;
			m_DraggablePanel.clipRange = new Vector4 (m_Width*0.5f+10-m_DraggablePanel.transform.localPosition.x,
			                                          -m_Height*0.5f-10-m_DraggablePanel.transform.localPosition.y, 
			                                          m_Width+5, m_Height+5);
		}
		if ( m_VertScrollBar != null )
		{
			m_VertScrollBar.transform.localPosition = new Vector3 (m_Width + 25, -20, 0);
			Vector3 tmp_scl = m_VertScrollBar.background.transform.localScale;
			m_VertScrollBar.background.transform.localScale = new Vector3 (tmp_scl.x, m_Height - 20, 1);
		}
		if ( m_HorzScrollBar != null )
		{
			m_HorzScrollBar.transform.localPosition = new Vector3 (20, -m_Height - 25, 0);
			Vector3 tmp_scl = m_HorzScrollBar.background.transform.localScale;
			m_HorzScrollBar.background.transform.localScale = new Vector3 (m_Width - 20, tmp_scl.y, 1);
		}
	}
}
