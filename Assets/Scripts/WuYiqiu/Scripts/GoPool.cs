using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class GoPool : MonoBehaviour 
{
	public GameObject m_GoPrefab;

	public int MaxPoolSize = 50;

	public int InitedNum = 25;

	List<GameObject> m_Gos = new List<GameObject>();
	bool m_Inited = false;
		
	Transform m_Trans;

	public void Init()
	{
		if (m_Inited)
			return;

		m_Trans = transform;

		for (int i = 0; i < InitedNum; i++)
		{
			GameObject go = GameObject.Instantiate(m_GoPrefab);
			go.transform.parent = m_Trans;
			go.transform.localPosition = Vector3.zero;
			go.transform.localRotation = Quaternion.identity;
			go.transform.localScale = Vector3.zero;
			go.SetActive (false);
			m_Gos.Add(go);
		}

		m_Inited = true;
	}

	public GameObject GetGo(Transform root, bool show)
	{
		if (m_Gos.Count != 0)
		{
			GameObject go = m_Gos[0];
			m_Gos.RemoveAt(0);
			go.transform.parent = root;
			ZeroSetTrans(go.transform);

			if (show)
				go.gameObject.SetActive(true);
			go.transform.localScale = Vector3.one;
			return go;
		}

		GameObject g = GameObject.Instantiate(m_GoPrefab);
		g.transform.parent = root;
		ZeroSetTrans(g.transform);
		if (show)
			g.SetActive (true);
//		g.transform.localScale = Vector3.one;

		return g;

	}

	public void GiveBackGo(GameObject go, bool hide)
	{
        if (go == null) return;
        if (hide)
			go.SetActive(false);
		if (m_Gos.Count > MaxPoolSize)
		{
			go.transform.parent = null;
			ZeroSetTrans(go.transform);
			Destroy(go);
			return;
		}

		go.transform.parent = m_Trans;
		ZeroSetTrans(go.transform);

		if (hide)
			go.SetActive(false);
		m_Gos.Add(go);
	}

	public static void ZeroSetTrans (Transform trans)
	{
		trans.localPosition = Vector3.zero;
		trans.localRotation = Quaternion.identity;
		trans.localScale = Vector3.one;
	}

	void Awake()
	{
		Init();
	}

}
