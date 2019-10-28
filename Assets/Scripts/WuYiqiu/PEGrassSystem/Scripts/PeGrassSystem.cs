using UnityEngine;
using System.Collections;
using RedGrass;
using Pathea.Maths;

[RequireComponent(typeof(RGScene))]
public class PeGrassSystem : MonoBehaviour 
{

	public static PeGrassSystem _self = null;

	public static PeGrassSystem Self  { get { return _self;}}


	public RGScene scene;

	public static void Refresh (float density, int LOD_level)
	{
		if (Self == null){
			//Debug.LogError("The Grass System is not initialized");
			return;
		}

		bool refresh_desity = true;

		if (_self.scene.evniAsset.Density < density + 0.01f 
		    && _self.scene.evniAsset.Density > density - 0.01f)
			refresh_desity = false;

		ELodType new_lod = (ELodType) Mathf.Clamp(LOD_level, (int)ELodType.LOD_1_TYPE_1, (int)ELodType.LOD_4_TYPE_1);
		if (_self.scene.evniAsset.LODType == new_lod && !refresh_desity)
			return;

		_self.scene.evniAsset.SetDensity(density);

		if (_self.scene.dataIO as PeGrassDataIO_Story != null)
		{
			_self.scene.evniAsset.SetLODType(new_lod);

			_self.scene.evniAsset.LODDensities = new float[_self.scene.evniAsset.MaxLOD + 1];
			for (int i = 0; i < _self.scene.evniAsset.MaxLOD + 1; i ++)
			{
				if (i == 0)
					_self.scene.evniAsset.LODDensities[i] = 1;
				else
				{
					_self.scene.evniAsset.LODDensities[i] = 0.4f / (1 << (i-1));
				}
			}
		}
		else
		{
			_self.scene.evniAsset.SetLODType(new_lod);
		}

		_self.scene.RefreshImmediately();
	}

	public static bool DeleteAtPos (Vector3 voxelPos)
	{
		if (Self == null)
		{
			Debug.LogError("The Grass System is not initialized");
            return false;
        }

		INTVECTOR3 pos = new INTVECTOR3((int)voxelPos.x, (int)(voxelPos.y + 0.5f), (int)voxelPos.z);
		if ( _self.scene.data.Remove(pos.x, pos.y, pos.z))
		{
			GrassDataSL.AddDeletedGrass(pos);
            return true;
		}

        return false;
	}

	public static bool DeleteAtPos(Vector3 voxelPos, out Vector3 pos)
	{
		pos = Vector3.zero;
		if (Self == null)
		{
			Debug.LogError("The Grass System is not initialized");
			return false;
		}

		INTVECTOR3 ipos = new INTVECTOR3((int)voxelPos.x, (int)(voxelPos.y + 0.5f), (int)voxelPos.z);
		pos = ipos;
		if (_self.scene.data.Remove(ipos.x, ipos.y, ipos.z))
		{
			GrassDataSL.AddDeletedGrass(pos);
			return true;
		}

		return false;
	}


	public static void SetWaveTexture (RenderTexture rt)
	{
		if (Self == null)
		{
			return;
		}

		if (_self.scene.meshCreator.grassMat.HasProperty("_WaveTex"))
			_self.scene.meshCreator.grassMat.SetTexture("_WaveTex", rt);

		if (_self.scene.meshCreator.triMat.HasProperty("_WaveTex"))
			_self.scene.meshCreator.triMat.SetTexture("_WaveTex", rt);

	}

	public static void SetWaveCenter (Vector4 center)
	{
		if (Self == null)
		{
			return;
		}

		if (_self.scene.meshCreator.grassMat.HasProperty("_WaveCenter"))
			_self.scene.meshCreator.grassMat.SetVector("_WaveCenter", center);
		
		if (_self.scene.meshCreator.triMat.HasProperty("_WaveCenter"))
			_self.scene.meshCreator.triMat.SetVector("_WaveCenter", center);
	}

	#region UNITY_INNER_FUNC

	void Awake ()
	{
		if (_self == null)
			_self = this;
		else
			Debug.LogError("the Grass system is already exist");

		scene = gameObject.GetComponent<RGScene>();
	}

	// Use this for initialization
	void Start ()
	{
	
	}

	#endregion
	

}
