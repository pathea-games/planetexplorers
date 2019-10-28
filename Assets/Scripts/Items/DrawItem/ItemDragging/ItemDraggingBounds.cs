#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using PETools;

public class ItemDraggingBounds : MonoBehaviour 
{
	[SerializeField]Bounds m_Bounds;

	public bool showBounds;
	public bool activeState;

	ViewBounds mViewBounds;

	public Bounds selfBounds { get { return m_Bounds; } }

	public Bounds worldBounds 
	{
		get 
		{ 
			Vector3 size = transform.rotation * m_Bounds.size;
			size.x = Mathf.Abs(size.x);
			size.y = Mathf.Abs(size.y);
			size.z = Mathf.Abs(size.z);
			return new Bounds(transform.position + transform.rotation * m_Bounds.center, size);
		} 
	}

	public void ResetBounds(Vector3 center, Vector3 size)
	{
		m_Bounds.center = center;
		m_Bounds.size = size;
	}

	void Update()
	{
		if (showBounds) 
		{
			if(null == mViewBounds)
				mViewBounds = ViewBoundsMgr.Instance.Get();
			Bounds bounds = worldBounds;
			mViewBounds.SetPos(bounds.min);
			mViewBounds.SetSize(bounds.size);
			mViewBounds.SetColor(activeState?Color.green:Color.red);
		}
		else
		{
			if(null != mViewBounds)
			{
				ViewBoundsMgr.Instance.Recycle(mViewBounds);
				mViewBounds = null;
			}
		}
	}

	void OnDestroy()
	{
		if(null != mViewBounds)
			ViewBoundsMgr.Instance.Recycle(mViewBounds);
	}

#if UNITY_EDITOR
	public void Reset()
	{
		transform.position = Vector3.zero;
		transform.rotation = Quaternion.identity;

		m_Bounds.SetMinMax (Vector3.zero, Vector3.zero);
		Renderer[] renders = PEUtil.GetCmpts<Renderer>(transform);
		for (int i = 0; i < renders.Length; ++i)
			m_Bounds.Encapsulate(renders[i].bounds);

		Collider[] colliders = PEUtil.GetCmpts<Collider>(transform);
		for (int i = 0; i < colliders.Length; ++i)
			m_Bounds.Encapsulate (colliders[i].bounds);

		ItemDraggingBase draggingBase = GetComponent<ItemDraggingBase>();
		if (null != draggingBase)
			draggingBase.itemBounds = this;
	}

	void OnDrawGizmosSelected()
	{
		//if(null == m_Bounds)
		//	return;
		if (!Application.isPlaying) 
		{
			Vector3 blu = new Vector3(m_Bounds.min.x, m_Bounds.min.y, m_Bounds.max.z);
			Vector3 bru = new Vector3(m_Bounds.max.x, m_Bounds.min.y, m_Bounds.max.z);
			Vector3 brb = new Vector3(m_Bounds.max.x, m_Bounds.min.y, m_Bounds.min.z);
			
			Vector3 ulu = new Vector3(m_Bounds.min.x, m_Bounds.max.y, m_Bounds.max.z);
			Vector3 ulb = new Vector3(m_Bounds.min.x, m_Bounds.max.y, m_Bounds.min.z);
			Vector3 urb = new Vector3(m_Bounds.max.x, m_Bounds.max.y, m_Bounds.min.z);

			UnityEditor.Handles.color = Color.yellow;
			UnityEditor.Handles.DrawLine(m_Bounds.min, blu);
			UnityEditor.Handles.DrawLine(m_Bounds.min, ulb);
			UnityEditor.Handles.DrawLine(m_Bounds.min, brb);

			UnityEditor.Handles.DrawLine(m_Bounds.max, ulu);
			UnityEditor.Handles.DrawLine(m_Bounds.max, bru);
			UnityEditor.Handles.DrawLine(m_Bounds.max, urb);

			UnityEditor.Handles.DrawLine(blu, ulu);
			UnityEditor.Handles.DrawLine(ulb, ulu);
			UnityEditor.Handles.DrawLine(blu, bru);

			UnityEditor.Handles.DrawLine(bru, brb);
			UnityEditor.Handles.DrawLine(brb, urb);
			UnityEditor.Handles.DrawLine(ulb, urb);
		}
	}
#endif
}

#if UNITY_EDITOR
public partial class PeCustomMenu : EditorWindow
{	
	[MenuItem("Assets/ResetItemBounds")]
	static void ResetItemBounds()
	{
		GameObject[] selectedObjArray = Selection.gameObjects;
		
		for (int i = 0; i < selectedObjArray.Length; ++i) 
		{			
			ItemDraggingBounds border = selectedObjArray[i].GetComponent<ItemDraggingBounds>();
			
			if(null == border)
				border = Undo.AddComponent<ItemDraggingBounds>(selectedObjArray[i]);
			
			border.Reset();
			UnityEditor.EditorUtility.SetDirty(selectedObjArray[i]);
		}
	}
}
#endif