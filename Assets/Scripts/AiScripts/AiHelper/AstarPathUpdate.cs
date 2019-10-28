using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AstarPathUpdate : MonoBehaviour
{
	public static AstarPathUpdate instance;

	List<IntVector4> terrainColliders = new List<IntVector4>();
	List<IntVector4> caveTerrain = new List<IntVector4>();

	List<Bounds> b45Bound = new List<Bounds>();
	List<Bounds> bounds = new List<Bounds>();
	//List<Bounds> waitUpdateBounds = new List<Bounds>();

	void Awake()
	{
		instance = this;
	}

    void OnEnable()
    {
        StartCoroutine(WaitForUpdateBounds());
        StartCoroutine(UpdateAstarPathBlock45());
    }

    void OnDisable()
    {
        StopAllCoroutines();
    }

	void Start()
	{
		//PlayerFactory.self.InitMainPlayerComplete += OnMainPlayerComplete;
		VFVoxelChunkGo.CreateChunkColliderEvent   += OnChunkColliderCreated;
		VFVoxelChunkGo.RebuildChunkColliderEvent  += OnChunkColliderRebuild;

		Block45Man.self.AttachEvents(OnBlock45BuildingColliderRebuild, null);
		if(LODOctreeMan.self != null)
			LODOctreeMan.self.AttachEvents(null, OnTerrainColliderDestroy, OnTerrainColliderCreated, null);
	}

	void Update()
	{
		if(Application.isEditor)
		{
//			foreach (Bounds bound in b45Bound) 
//			{
//				AiUtil.DrawBounds(null, bound, Color.red);	
//			}

//			foreach (Bounds bound in bounds) 
//			{
//				AiUtil.DrawBounds(null, bound, Color.blue);	
//			}
		}
	}

	void OnDestroy()
	{
        //if(PlayerFactory.mMainPlayer != null)
        //{
        //    PlayerFactory.mMainPlayer.OnCaveEnterEvent -= OnMainPlayerCaveEnter;
        //    PlayerFactory.mMainPlayer.OnCaveExitEvent -= OnMainPlayerCaveExit;
        //}

        //if(PlayerFactory.self != null)
        //    PlayerFactory.self.InitMainPlayerComplete  -= OnMainPlayerComplete;

		VFVoxelChunkGo.CreateChunkColliderEvent    -= OnChunkColliderCreated;
		VFVoxelChunkGo.RebuildChunkColliderEvent   -= OnChunkColliderRebuild;

        if (Block45Man.self != null)
			Block45Man.self.DetachEvents(OnBlock45BuildingColliderRebuild, null);

        if (LODOctreeMan.self != null)
		    LODOctreeMan.self.DetachEvents(null, OnTerrainColliderDestroy, OnTerrainColliderCreated, null);
	}

	public void UpdateGraphs(Bounds argBounds)
	{
        //if(AstarPath.active == null)
        //    return;

        //if(!AstarPath.active.isScanned || AstarPath.active.isScanning)
        //    waitUpdateBounds.Add(argBounds);
        //else
        //    UpdateGraphsFromBounds(argBounds);
	}

	void UpdateGraphsFromBounds(Bounds argBounds)
	{
		//GraphUpdateObject ob = new GraphUpdateObject(argBounds);
		//ob.requiresFloodFill = false;

        //if(AstarPath.active != null && AstarPath.active.gameObject.activeSelf)
        //    AstarPath.active.UpdateGraphs(argBounds);
	}

    //void OnMainPlayerComplete(Player player)
    //{
    //    //PlayerFactory.mMainPlayer.OnCaveEnterEvent += OnMainPlayerCaveEnter;
    //    //PlayerFactory.mMainPlayer.OnCaveExitEvent  += OnMainPlayerCaveExit;
    //}

	void OnBlock45BuildingColliderRebuild(Block45ChunkGo vfGo)
	{
		if(vfGo != null && vfGo._mc != null)
		{
			b45Bound.Add(vfGo._mc.bounds);
		}
	}

	void OnChunkColliderCreated(VFVoxelChunkGo chunk)
	{
		Vector3 min = chunk.transform.position;
		Vector3 max = chunk.transform.position + Vector3.one * VoxelTerrainConstants._numVoxelsPerAxis;
		
		Bounds bound = new Bounds();
		bound.SetMinMax(min, max);
		
		UpdateGraphs(bound);
	}
	
	void OnChunkColliderRebuild(VFVoxelChunkGo chunk)
	{
		Vector3 min = chunk.transform.position;
		Vector3 max = chunk.transform.position + Vector3.one * VoxelTerrainConstants._numVoxelsPerAxis;
		
		Bounds bound = new Bounds();
		bound.SetMinMax(min, max);
		
		UpdateGraphs(bound);
	}

	bool MatchCave(IntVector4 node1, IntVector4 node2)
	{
		IntVector3 intVec1 = new IntVector3(node1.x, node1.z, node1.w);
		IntVector3 intVec2 = new IntVector3(node2.x, node2.z, node2.w);
		
		return intVec1.Equals(intVec2);
	}

	void OnMainPlayerCaveEnter()
	{
		StartCoroutine(UpdateAstarPathGraphs());
	}
	
	void OnMainPlayerCaveExit()
	{
		StartCoroutine(UpdateAstarPathGraphs());
	}

	void OnTerrainColliderCreated(IntVector4 node)
	{
		RegisterTerrainCollider(node);
	}

	void OnTerrainColliderDestroy(IntVector4 node)
	{
		RemoveTerrainCollider(node);
	}

    void RegisterTerrainCollider(IntVector4 node)
	{
		if (!terrainColliders.Contains(node))
		{
			IntVector4 intVec = terrainColliders.Find(ret => MatchCave(ret, node));
			if (intVec != null)
			{
				IntVector4 caveNode = caveTerrain.Find(ret => MatchCave(ret, node));
				if (caveNode == null)
					caveTerrain.Add(intVec);
			}
			
			terrainColliders.Add(node);
		}
	}
	
    void RemoveTerrainCollider(IntVector4 node)
	{
		if (terrainColliders.Contains(node))
		{
			terrainColliders.Remove(node);
			
			IntVector4 intVec = terrainColliders.Find(ret => MatchCave(ret, node));
			if (intVec != null)
			{
				IntVector4 caveNode = caveTerrain.Find(ret => MatchCave(ret, node));
				if (caveNode != null)
					caveTerrain.Add(caveNode);
			}
		}
	}

	IEnumerator WaitForUpdateBounds()
	{
        //while (AstarPath.active != null) 
        //{
        //    if(AstarPath.active.isScanned && !AstarPath.active.isScanning)
        //    {
        //        foreach (Bounds bo in waitUpdateBounds) 
        //        {
        //            UpdateGraphsFromBounds(bo);
        //        }

        //        waitUpdateBounds.Clear();
        //        yield break;
        //    }

        //    yield return new WaitForSeconds(1.0f);
        //}
        yield break;
	}

	IEnumerator UpdateAstarPathBlock45()
	{ 
		while (true) 
		{
			if(Block45Man.self != null && !Block45Man.self.isColliderBuilding)
			{
				Bounds graphBound = new Bounds();
				for (int i = b45Bound.Count - 1; i >= 0; i--) 
				{
					if(graphBound.size == Vector3.zero)
						graphBound = b45Bound[i];

					Bounds tmpBound = graphBound;
					tmpBound.Expand(Block45Constants.ChunkPhysicalSize);

					if(tmpBound.Intersects(b45Bound[i]))
					{
						graphBound.Encapsulate(b45Bound[i].min);
						graphBound.Encapsulate(b45Bound[i].max);

						b45Bound.RemoveAt(i);
					}
				}

				if(graphBound.size != Vector3.zero)
				{
					graphBound.Expand(2);
					UpdateGraphs(graphBound);
					bounds.Add(graphBound);
				}
			}

			yield return new WaitForSeconds(0.5f);
		}

	}

	IEnumerator UpdateAstarPathGraphs()
	{
        //if (AstarPath.active != null)
        //{
        //    for (int i = caveTerrain.Count - 1; i >=0; i--)
        //    {
        //        if (caveTerrain[i] == null)
        //            continue;
				
        //        IntVector4 intVec = caveTerrain[i];
				
        //        Vector3 center = intVec.ToVector3() + Vector3.one * (VoxelTerrainConstants._numVoxelsPerAxis >> 1);
        //        Bounds bound = new Bounds(center, Vector3.one * (VoxelTerrainConstants._numVoxelsPerAxis + 2));
				
        //        UpdateGraphs(bound);
				
        //        yield return new WaitForSeconds(0.1f);
        //    }
        //}
        yield break;
	}
}
