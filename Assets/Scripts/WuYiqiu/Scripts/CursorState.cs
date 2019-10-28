using UnityEngine;
using System.Collections;

public class CursorHandler
{
	public CursorState.EType Type;
}


public class CursorState : MonoBehaviour 
{
	private static CursorState _self;
	public static CursorState self 		
	{ 
		get
		{ 
			if (_self == null)
			{
				if (Application.isPlaying)
				{
					GameObject go = Resources.Load<GameObject>("Prefabs/CursorState");
					if (go != null)
					{
						GameObject.Instantiate(go);
					}
				}
			}
			return _self;
		} 
	}

	public enum EType
	{
		None,
		Normal,
		Hand
	}

	private CursorHandler m_Handler;
	public CursorHandler Handler { get { return m_Handler;} }
	
	public void SetHandler (CursorHandler handler)
	{
		if (handler != m_Handler)
			m_Handler = handler;
		
	}
	
	public void ClearHandler (CursorHandler handler)
	{
		if (m_Handler == handler)
			m_Handler = null;
	}

	[System.Serializable]
	public class CursorIcon
	{
		public Texture2D none;
		public Texture2D nml;
		public Texture2D hand;
	}

	public CursorIcon m_Icon;

	void Awake ()
	{
		_self = this;
	}

	// Use this for initialization
	void Start ()
	{
	
	}

	bool onece = false;
	// Update is called once per frame
	void Update () 
	{
		if (m_Handler != null)
		{
			switch (m_Handler.Type)
			{
			case EType.Normal:
				Cursor.SetCursor(m_Icon.nml, new Vector2(m_Icon.nml.width / 2, m_Icon.nml.height / 2), CursorMode.Auto);
				break;
			case EType.Hand:
				Cursor.SetCursor(m_Icon.hand, new Vector2(m_Icon.hand.width / 2, m_Icon.hand.height / 2), CursorMode.Auto);
				break;
			case EType.None:
				Cursor.SetCursor(m_Icon.none, new Vector2(m_Icon.none.width / 2, m_Icon.none.height / 2), CursorMode.Auto);
				break;
			default:
				Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
				break;
			}

			onece = false;
		}
		else
		{
			if (!onece )
			{
				Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
				onece = true;
			}
		}
	}
}
