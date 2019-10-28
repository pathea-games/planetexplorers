using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CSUI_SubStorageHistory : MonoBehaviour 
{
	public int Day;

	#region UI_WIDGET

	[SerializeField] UITable  m_TableUI;

	[SerializeField] UILabel  m_HistoryLbPrefab;
	
	#endregion

	private List<GameObject>	m_HistoryObjs = new List<GameObject>();

	public delegate void RepositionDel();
	public RepositionDel onReposition;

	public bool IsEmpty 	{ get { return m_HistoryObjs.Count == 0;} }

	int repositionNow_cnt = 0;
	public void AddHistory (string history)
	{
		UILabel lb = Instantiate(m_HistoryLbPrefab) as UILabel;
		lb.transform.parent = m_TableUI.transform;
		lb.transform.localPosition = Vector3.zero;
		lb.transform.localRotation = Quaternion.identity;
		lb.MakePixelPerfect();

		lb.text = history;

		m_HistoryObjs.Add(lb.gameObject);

//		m_TableUI.repositionNow = true;
		repositionNow_cnt = 2;
	}

	public void PopImmediate()
	{
		if (m_HistoryObjs.Count != 0)
		{
			GameObject.DestroyImmediate( m_HistoryObjs[0]);
			m_HistoryObjs.RemoveAt(0);

			m_TableUI.repositionNow = true;
		}
	}
	

	private void OnReposition()
	{
		Transform trans = m_TableUI.transform;
		trans.localPosition = new Vector3(trans.localPosition.x, -m_TableUI.mVariableHeight,  trans.position.y);

		if (onReposition != null)
			onReposition();
	}

	void Awake ()
	{
		m_TableUI.onReposition = OnReposition;
	}

	void OnDestroy()
	{
		if (onReposition != null)
			onReposition();
	}

	// Use this for initialization
	void Start () 
	{
	
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (repositionNow_cnt > 0)
		{
			m_TableUI.repositionNow = true;
			repositionNow_cnt --;
		}
	}

	void LateUpdate ()
	{

	}
}
