using UnityEngine;
using System.Collections;

public class FootprintDecal : MonoBehaviour {
	const int MaxLifeTime = 420;
	const int FadeOutCnt = 300;
	[HideInInspector]public int _lifetime;
	Renderer _r;
	public void Reset(Vector3 pos, Quaternion rot)
	{
		transform.position = pos;
		transform.rotation = rot;
		_lifetime = MaxLifeTime;
		_r.enabled = true;
	}
	void Start()
	{
		_r = gameObject.GetComponent<Renderer>();
		_lifetime = MaxLifeTime;
		_r.enabled = true;
	}
	public void UpdateDecal () 
	{
		if(_lifetime > 0)
		{
			_lifetime--;
			VFVoxel groundVoxel = VFVoxelTerrain.self.Voxels.SafeRead((int)transform.position.x, (int)transform.position.y, (int)transform.position.z);
			if(groundVoxel.Volume <= 0x40)
			{
				_lifetime = 0;
			}

			if(_lifetime <= 0)
			{
				_r.material.color = new Color(1,1,1, 0);
				_r.enabled = false;
			}
			else if(_lifetime > FadeOutCnt)
			{
				_r.material.color = new Color(1,1,1, 1);
			}
			else
			{
				_r.material.color = new Color(1,1,1, ((float)_lifetime)/FadeOutCnt);
			}
		}
	}
}
