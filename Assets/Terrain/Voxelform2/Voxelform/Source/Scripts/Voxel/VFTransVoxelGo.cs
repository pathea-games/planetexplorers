using UnityEngine;
using System.Collections;

public class VFTransVoxelGo : MonoBehaviour,IRecyclable
{
	public static Transform _defParent;
	public static Material _defMat;
	public static int _defLayer;
	
	public MeshRenderer _mr;
	public MeshFilter _mf;
	public int _faceMask;
	
	void Awake()
	{
		_mf = gameObject.AddComponent<MeshFilter>();
		_mr = gameObject.AddComponent<MeshRenderer>();
		_mr.sharedMaterial = _defMat;
		_mf.sharedMesh = new Mesh();
		transform.parent = _defParent;
		gameObject.layer = _defLayer;
		name = "trans_";
	}

	public void OnRecycle()
	{
		_mf.mesh.Clear();
		_faceMask = 0;
		transform.parent = _defParent;
		gameObject.SetActive(false);
	}
	
	void OnDestroy()
	{
		GameObject.Destroy(_mf.mesh);
	}
}
