using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathea;
using PETools;

public class PathfindingUpdate : MonoBehaviour 
{
	Bounds _b45Bound = new Bounds();
    List<Bounds> _chunkBounds;
    ProceduralGridMover _mover;

    void Awake () 
    {
		_chunkBounds = new List<Bounds>();
        _mover = GetComponent<ProceduralGridMover>();

        if (Block45Man.self != null)
			Block45Man.self.AttachEvents(OnBlock45ColliderBuild, null);

        VFVoxelChunkGo.CreateChunkColliderEvent += OnChunkColliderCreated;
        VFVoxelChunkGo.RebuildChunkColliderEvent += OnChunkColliderRebuild;

        LSubTerrainMgr.OnTreeColliderCreated += OnTreeColliderCreated;
        LSubTerrainMgr.OnTreeColliderDestroy += OnTreeColliderDestroy;

        StartCoroutine(UpdateAstarPathChunk());
        StartCoroutine(UpdateAstarPathBlock45());
	}

    void OnDestroy()
    {
        VFVoxelChunkGo.CreateChunkColliderEvent -= OnChunkColliderCreated;
        VFVoxelChunkGo.RebuildChunkColliderEvent -= OnChunkColliderRebuild;

        if (Block45Man.self != null)
            Block45Man.self.DetachEvents(OnBlock45ColliderBuild, null);

        LSubTerrainMgr.OnTreeColliderCreated -= OnTreeColliderCreated;
        LSubTerrainMgr.OnTreeColliderDestroy -= OnTreeColliderDestroy;
    }

    void AddChunk(Bounds bound)
    {
        if (!Contains(bound))
        {
            _chunkBounds.Add(bound);
        }
    }

    bool Contains(Bounds bound)
    {
		bool ret = false;
		Vector3 min = bound.min;
		int x = Mathf.RoundToInt (min.x);
		int z = Mathf.RoundToInt (min.z);
		int n = _chunkBounds.Count;
		for (int i = 0; i < n; i++) {
			Vector3 curMin = _chunkBounds[i].min;
			if(Mathf.RoundToInt (curMin.x) == x && Mathf.RoundToInt (curMin.z) == z){
				ret = true;
				break;
			}
		}
        return ret;
    }

	void OnBlock45ColliderBuild(Block45ChunkGo vfGo)
	{
		if (vfGo != null && vfGo._mc != null)
		{
			if (_b45Bound.size != Vector3.zero)
			{
				_b45Bound.Encapsulate(vfGo._mc.bounds);
			}
			else
			{
				_b45Bound = vfGo._mc.bounds;
			}
		}
	}	

	static readonly Vector3[] _ofsParts = new Vector3[]{
		new Vector3(0f,0f,0f),
		new Vector3(1f,0f,0f) * (VoxelTerrainConstants._numVoxelsPerAxis>>1),
		new Vector3(0f,0f,1f) * (VoxelTerrainConstants._numVoxelsPerAxis>>1),
		new Vector3(1f,0f,1f) * (VoxelTerrainConstants._numVoxelsPerAxis>>1),
	};
	static readonly Vector3 _sizeParts = new Vector3 (1f, 1f, 1f) * (VoxelTerrainConstants._numVoxelsPerAxis >> 1);
	void OnChunkColliderCreated(VFVoxelChunkGo chunk)
    {
#if true // divide into 4 blocks(because astar_grid is y_ignored, so the upper_4 bounds are ignored)
		for(int i = 0; i < 4; i++){
			Bounds b = new Bounds();
			Vector3 min = chunk.transform.position+_ofsParts[i];
			b.SetMinMax(min, min+_sizeParts);
			AddChunk(b);
		}
#else	
		Vector3 min = chunk.transform.position;
		Vector3 max = chunk.transform.position + Vector3.one * VoxelTerrainConstants._numVoxelsPerAxis;
		Bounds bound = new Bounds();
		bound.SetMinMax(min, max);		
		AddChunk(bound);
#endif
    }

    void OnChunkColliderRebuild(VFVoxelChunkGo chunk)
    {
        Vector3 min = chunk.transform.position;
        Vector3 max = chunk.transform.position + Vector3.one * VoxelTerrainConstants._numVoxelsPerAxis;
        Bounds bound = new Bounds();
        bound.SetMinMax(min, max);
        AddChunk(bound);
    }
	
    void OnTreeColliderCreated(GameObject obj)
    {
        Collider col = obj != null ? obj.GetComponent<Collider>() : null;
        if (col != null)
        {
            AstarPath.active.UpdateGraphs(col.bounds);
        }
    }

    void OnTreeColliderDestroy(GameObject obj)
    {
        Collider col = obj != null ? obj.GetComponent<Collider>() : null;
        if (col != null)
        {
            StartCoroutine(DelayUpdateGraph(col.bounds, 0.5f));
        }
    }

    IEnumerator DelayUpdateGraph(Bounds bounds, float delayTime)
    {
        yield return new WaitForSeconds(delayTime);

        AstarPath.active.UpdateGraphs(bounds);
    }

	IEnumerator UpdateAstarPathBlock45()
	{
		while (true)
		{
			if (_b45Bound.size != Vector3.zero)
			{
				AstarPath.active.UpdateGraphs(_b45Bound);
				_b45Bound = new Bounds();
			}
			
			yield return new WaitForSeconds(2.0f);
		}
	}

	IEnumerator UpdateAstarPathChunk()
	{
		while (true)
		{
            if (PeCreature.Instance.mainPlayer != null &&
                PeCreature.Instance.mainPlayer.hasView)
            {
                if (_mover != null && _mover.target == null)
                {
                    _mover.target = PeCreature.Instance.mainPlayer.peTrans.trans;
                    //AstarPath.active.Scan();
                }

                for (int i = _chunkBounds.Count - 1; i >= 0; i--)
                {
                    AstarPath.active.UpdateGraphs(_chunkBounds[i]);
                    _chunkBounds.RemoveAt(i);
                    //yield return new WaitForSeconds(0.1f);
                }
            }
            yield return new WaitForSeconds(1.0f);
        }
    }
}
