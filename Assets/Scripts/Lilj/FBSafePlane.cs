using UnityEngine;
using System.Collections;
using Pathea;

public class FBSafePlane : MonoBehaviour
{
	static FBSafePlane mInstance;
	public static FBSafePlane instance
	{
		get
		{
			if(null == mInstance)
			{
				GameObject go = new GameObject("FBSafePlane");
				mInstance = go.AddComponent<FBSafePlane>();
			}
			return mInstance;
		}

	}
	static readonly float BorderLength = 100f;
	
	BoxCollider m_Colldier;

	Vector3 m_ResetPos;

	void Awake()
	{
		gameObject.layer = Pathea.Layer.VFVoxelTerrain;
		m_Colldier = gameObject.AddComponent<BoxCollider>();
		m_Colldier.isTrigger = true;
		m_Colldier.center = Vector3.zero;
	}

	public void ResetCol(Vector3 min, Vector3 max, Vector3 resetPos)
	{
		Vector3 colMin = 0.5f * (min + max);
		colMin.y = min.y - 10f;
		transform.position = colMin;

		Vector3 colSize = max - min + 2 * BorderLength * Vector3.one;
		colSize.y = 5f;
		m_Colldier.size = colSize;

		m_ResetPos = resetPos;
	}

	public void DeleteCol()
	{
		if(null != m_Colldier)
			GameObject.Destroy(gameObject);
	}

	void OnTriggerEnter(Collider other)
	{
		PeEntity entity = other.transform.GetComponentInParent<PeEntity>();
		if(null != entity)
			entity.position = m_ResetPos;
	}
}
