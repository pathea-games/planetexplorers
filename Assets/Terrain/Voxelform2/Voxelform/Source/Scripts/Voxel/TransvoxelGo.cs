using UnityEngine;
using System.Collections;

public class TransvoxelGo : MonoBehaviour
{
	public Mesh _mesh;
	
	void OnDestroy()
	{
		GameObject.Destroy(_mesh);
		_mesh = null;
	}
}
