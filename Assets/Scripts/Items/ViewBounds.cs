using UnityEngine;
using System.Collections.Generic;

public class ViewBounds : MonoBehaviour 
{
	[SerializeField] LineRenderer[] mLineRenderers;
	[SerializeField] MeshRenderer mBoxRenderer;

	Material mBoxMat;
	Material mLineMat;
	
	public void SetPos(Vector3 centerPos)
	{
		transform.position = centerPos;
	}
	
	public void SetSize(Vector3 size)
	{
		transform.localScale = size;
		float length = size.x + size.y + size.z;
		for(int i = 0; i < mLineRenderers.Length; ++i)
			mLineRenderers[i].SetWidth(length * 0.01f, length * 0.01f);
	}
	
	public void SetColor(Color color)
	{
		mBoxMat.SetColor("_TintColor", color);
		mLineMat.color = color;
	}

	void Awake()
	{
		mBoxMat = Material.Instantiate (mBoxRenderer.material);
		mBoxRenderer.material = mBoxMat;
		mLineMat = Material.Instantiate (mLineRenderers[0].material);
		for (int i = 0; i < mLineRenderers.Length; ++i)
			mLineRenderers [i].material = mLineMat;
	}

	void Reset()
	{
		mBoxRenderer = GetComponentInChildren<MeshRenderer>();
		mLineRenderers = GetComponentsInChildren<LineRenderer>();
	}

	void OnDestroy()
	{
		Material.Destroy (mBoxMat);
		Material.Destroy (mLineMat);
	}
}

public class ViewBoundsMgr : MonoBehaviour
{
	static ViewBoundsMgr mInstance;
	public static ViewBoundsMgr Instance
	{
		get
		{
			if(null == mInstance)
			{
				GameObject obj = new GameObject("ViewBoundsMgr");
				obj.transform.position = Vector3.zero;
				obj.transform.rotation = Quaternion.identity;
				mInstance = obj.AddComponent<ViewBoundsMgr>();
			}
			return mInstance;
		}
	}

	Stack<ViewBounds> mStack;

	void Awake()
	{
		mStack = new Stack<ViewBounds> ();
		transform.position = Vector3.zero;
		transform.rotation = Quaternion.identity;
	}

	public ViewBounds Get()
	{
		ViewBounds retBounds = null;
		if(mStack.Count > 0)
			retBounds = mStack.Pop ();
		if (null == retBounds)
			retBounds = CreatViewBounds ();
		retBounds.gameObject.SetActive (true);
		return retBounds;
	}

	public void Recycle(ViewBounds bounds)
	{
		bounds.gameObject.SetActive (false);
		mStack.Push (bounds);
	}

	ViewBounds CreatViewBounds()
	{
		GameObject obj = GameObject.Instantiate (Resources.Load ("Prefab/Other/ViewBounds")) as GameObject;
		if (null == obj)
		{
			Debug.LogError("Can't find ViewBounds");
			return null;
		}
		obj.transform.parent = transform;
		return obj.GetComponent<ViewBounds>();
	}
}
