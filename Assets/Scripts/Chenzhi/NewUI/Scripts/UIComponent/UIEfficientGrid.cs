using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public interface IListReceiver
{
	void SetContent(int index, GameObject go);
	void ClearContent(GameObject go);
}


public class UIEfficientGrid: MonoBehaviour 
{
	public GoPool itemGoPool;

	public GameObject itemPrefab;

	public UIPanel panel;
		
	public enum Arrangement
	{
		Horizontal,
		Vertical,
	}

	public Arrangement arrangement = Arrangement.Horizontal;

	public float cellWidth = 58;
	public float cellHeight = 200f;

	public bool repositionPosNow = false;
	public bool repositionVisibleNow = false;

	List<GameObject>  m_Items = new List<GameObject>();
	public List<GameObject> Gos { get { return m_Items;} }


	public delegate void DSetContent (int index, GameObject go);
	public delegate void DClearContent (GameObject go);
	public void UpdateList (int count, DSetContent setContent, DClearContent clearContent)
	{
		if (count > m_Items.Count)
		{
			for (int i = 0; i < m_Items.Count; i++)
			{
				Transform trans = m_Items[i].transform;
				trans.localPosition = new Vector3(0, - i * cellHeight, 0);
				if (setContent != null)
					setContent(i, m_Items[i]);
			}

			for (int i = m_Items.Count; i < count; i++)
			{
				GameObject go = CreateGo();
				go.transform.localPosition = new Vector3(0, - i * cellHeight, 0);
				if (setContent != null)
					setContent(i, go);
				m_Items.Add(go);
			}
		}
		else
		{
			for (int i = 0; i < count; i++)
			{
				Transform trans = m_Items[i].transform;
				trans.localPosition = new Vector3(0, - i * cellHeight, 0);
				if (setContent != null)
					setContent(i, m_Items[i]);
			}

			for (int i =  m_Items.Count-1; i >= count; i--)
			{
				if (clearContent != null)
					clearContent(m_Items[i]);
				DestroyGo(m_Items[i]);
			}

			m_Items.RemoveRange(count, m_Items.Count - count);
		}

	
		repositionPosNow = true;
		repositionVisibleNow = true;

		UIDraggablePanel drag = NGUITools.FindInParents<UIDraggablePanel>(gameObject);
		if (drag != null) drag.UpdateScrollbars(true);
	}

	public void UpdateList (int count, IListReceiver receiver)
	{
		if (receiver == null)
		{
			Debug.LogError("The receiver is null");
			return;
		}
		if (count > m_Items.Count)
		{
			for (int i = 0; i < m_Items.Count; i++)
			{
				Transform trans = m_Items[i].transform;
				trans.localPosition = new Vector3(0, - i * cellHeight, 0);
				receiver.SetContent(i, m_Items[i]);
			}
			
			for (int i = m_Items.Count; i < count; i++)
			{
				GameObject go = CreateGo();
				go.transform.localPosition = new Vector3(0, - i * cellHeight, 0);
				receiver.SetContent(i, go);
				m_Items.Add(go);
			}
		}
		else
		{
			for (int i = 0; i < count; i++)
			{
				Transform trans = m_Items[i].transform;
				trans.localPosition = new Vector3(0, - i * cellHeight, 0);
				receiver.SetContent(i, m_Items[i]);
			}
			
			for (int i =  m_Items.Count-1; i >= count; i--)
			{
				receiver.ClearContent(m_Items[i]);
                DestroyGo(m_Items[i]);
            }
            
            m_Items.RemoveRange(count, m_Items.Count - count);
        }
        
        
        repositionPosNow = true;
        repositionVisibleNow = true;
        
        UIDraggablePanel drag = NGUITools.FindInParents<UIDraggablePanel>(gameObject);
        if (drag != null) drag.UpdateScrollbars(true);
	}

	public void Reposition ()
	{
		if (arrangement == Arrangement.Vertical)
		{
			for (int i = 0; i < m_Items.Count; i++)
				m_Items[i].transform.localPosition = new Vector3(0, - i * cellHeight, 0);
		}
		else if (arrangement == Arrangement.Horizontal)
		{
			for (int i = 0; i < m_Items.Count; i++)
				m_Items[i].transform.localPosition = new Vector3(i * cellWidth, 0, 0);
		}
	}

	public void RepositionVisible()
	{
		Vector4 clipRange = panel.clipRange;

		// Vertical
		float miny = (clipRange.y + clipRange.w/2) + cellHeight * 5;
		float maxy = clipRange.y  - cellHeight * 5;

		float minx = (clipRange.x - clipRange.w/2) - cellHeight * 5;
		float maxx = clipRange.x + clipRange.w/2 + cellHeight * 5;
		
		
		for (int i = 1; i < m_Items.Count - 1; i++)
		{
			GameObject item = m_Items[i];
			Vector3 local_pos = GetLocalPosition(item.transform);
			if (local_pos.y > miny || local_pos.y < maxy
			    || local_pos.x < minx || local_pos.x > maxx)
			{
				if (item.activeInHierarchy)
					item.SetActive(false);
			}
			else
			{
				if (!item.activeInHierarchy)
					item.SetActive(true);
			}
		}

		_oldClipRangeX = clipRange.x;
		_oldClipRangeY = clipRange.y;
	}

	GameObject CreateGo()
	{
		if (itemGoPool != null)
		{
			return itemGoPool.GetGo(transform, false);
		}

		GameObject go = Instantiate(itemPrefab) as GameObject;
		go.transform.parent = transform;
		go.transform.localPosition = Vector3.zero;
		go.transform.localRotation = Quaternion.identity;
		go.transform.localScale = Vector3.one;
		return go;
	}

	void DestroyGo(GameObject go)
	{
        if (go)
        {
            if (itemGoPool != null)
            {
                itemGoPool.GiveBackGo(go, true);
                return;
            }
            GameObject.Destroy(go);
        }
	}

//	private GameObject tempObject = null;
//	private GameObject tempObjectTop = null; 
	void Awake()
	{
//		if (itemGoPool != null)
//			itemGoPool.m_GoPrefab = itemPrefab;
//
//		tempObject = GameObject.Instantiate(itemPrefab);
//		tempObject.name = ""
//		tempObject.transform.parent = transform;
//		tempObject.transform.localPosition = Vector3.zero;
//		tempObject.transform.localRotation = Quaternion.identity;
//		tempObject.transform.localScale = Vector3.one;
//
//		{
//
//			MonoBehaviour[] scripts = tempObject.GetComponents<MonoBehaviour>();
//			foreach (MonoBehaviour script in scripts)
//				Destroy(script);
//
//			UIWidget[] widgets = tempObject.GetComponentsInChildren<UIWidget>();
//			foreach (UIWidget w in widgets)
//			{
//				w.enabled = false;
//			}
//
//			Collider[] collders = tempObject.GetComponentsInChildren<Collider>();
//			foreach (Collider c in collders)
//				c.enabled = false;
//
//			Collider self_collider = tempObject.GetComponent<Collider>();
//			if (self_collider != null)
//				self_collider.enabled = false;
//		}
//
//		tempObjectTop = GameObject.Instantiate(itemPrefab);
//		tempObjectTop.transform.parent = transform;
//		tempObjectTop.transform.localPosition = Vector3.zero;
//		tempObjectTop.transform.localRotation = Quaternion.identity;
//		tempObjectTop.transform.localScale = Vector3.one;
//
//		{
//			MonoBehaviour[] scripts = tempObjectTop.GetComponents<MonoBehaviour>();
//			foreach (MonoBehaviour script in scripts)
//				Destroy(script);
//
//			UIWidget[] widgets = tempObjectTop.GetComponentsInChildren<UIWidget>();
//			foreach (UIWidget w in widgets)
//				w.enabled = false;
//
//			Collider[] collders = tempObjectTop.GetComponentsInChildren<Collider>();
//			foreach (Collider c in collders)
//				c.enabled = false;
//
//			Collider self_collider = tempObjectTop.GetComponent<Collider>();
//			if (self_collider != null)
//				self_collider.enabled = false;
//		}
	}

//	private Vector4 old_clip_range = Vector4.zero;
	float _oldClipRangeX = -10000;
	float _oldClipRangeY = -10000;


	void Update()
	{
		if (panel != null)
		{
			Vector4 clipRange = panel.clipRange;
			// Vertical
			if (clipRange.y != _oldClipRangeY || clipRange.x != _oldClipRangeX)
			{
				repositionVisibleNow = true;

				_oldClipRangeY = clipRange.y;
				_oldClipRangeX = clipRange.x;

			}

		}
	}

	void LateUpdate ()
	{
		if (repositionPosNow)
		{
			Reposition();
			repositionPosNow = false;
		}

		if (repositionVisibleNow)
		{
			RepositionVisible();
			repositionVisibleNow = false;
		}

		if (m_Items.Count != 0)
		{
//			tempObjectTop.transform.position = m_Items[0].transform.position;
//			tempObject.transform.position = m_Items[m_Items.Count -1].transform.position;
			m_Items[0].SetActive(true);
			m_Items[m_Items.Count - 1].SetActive(true);
		}
		else
		{
//			tempObject.SetActive(false);
//			tempObjectTop.SetActive(false);
		}
	}

	Vector3 GetLocalPosition (Transform trans)
	{
		Vector3 local_pos = trans.localPosition;
		Transform parent = trans.parent;
		while (parent != panel.transform)
		{
			if (parent == null)
			{
				Debug.LogError("This item is not ");
				return trans.localPosition;
			}

			local_pos += parent.localPosition ;
			parent = parent.parent;
		}

		return local_pos;
	}

}
