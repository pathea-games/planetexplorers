using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ItemAsset;

using Random = UnityEngine.Random;

//[RequireComponent(typeof(uLinkNetworkView))]
public class SPTerrainEvent : NetworkInterface
{
	public const int VoxelTerrainMaxLod = 3;
	public static SPTerrainEvent instance;

    public int minCount;
    public int maxCount;

	public int minCountCave;
	public int maxCountCave;

    Dictionary<IntVector2, SPTerrainRect> pointTable = new Dictionary<IntVector2, SPTerrainRect>();
    //List<IntVector2> requestList = new List<IntVector2>();
    List<IntVector4> mMeshNodes = new List<IntVector4>();

    Transform staticPoints;
    SimplexNoise mNoise;

    public List<IntVector4> meshNodes
    {
        get { return mMeshNodes; }
    }

	public bool isActive
	{
		get{return gameObject.activeSelf;}
	}
	 
    void Awake()
	{
		instance = this;
        mNoise = new SimplexNoise((long)(RandomMapConfig.RandomMapID + RandomMapConfig.RandSeed));

        if (Application.loadedLevelName.Equals(GameConfig.MainSceneName))
        {
            AISpawnPoint.Reset();
            LoadStaticSpawnPoints();
        }
	}

    void Start()
    {
		LODOctreeMan.self.AttachEvents(OnTerrainMeshCreated, OnTerrainMeshDestroy, null, OnTerrainPositionChange);
    }

	void OnDisable()
	{
		CleanupSPTerrainRect();
		LODOctreeMan.self.DetachEvents(OnTerrainMeshCreated, OnTerrainMeshDestroy, null, OnTerrainPositionChange);
	}

    void OnDestroy()
    {
		CleanupSPTerrainRect();
		LODOctreeMan.self.DetachEvents(OnTerrainMeshCreated, OnTerrainMeshDestroy, null, OnTerrainPositionChange);
    }

	public void Activate(bool value)
	{
		gameObject.SetActive(value);
	}

    public SPTerrainRect GetSPTerrainRect(IntVector2 mark)
    {
        if (pointTable.ContainsKey(mark))
            return pointTable[mark];
        else
            return null;
    }

	public void RegisterSPPoint(SPPoint point, bool updateIndex = false)
	{
        if (point == null)
            return;

        IntVector4 node = AiUtil.ConvertToIntVector4(point.position, 3);
        IntVector2 mark = new IntVector2(node.x, node.z);

		if(pointTable.ContainsKey(mark))
		{
			pointTable[mark].RegisterSPPoint(point);

            if (updateIndex)
            {
                point.index = pointTable[mark].nextIndex;
            }
		}
	}

    public SPTerrainRect GetSPTerrainRect(Vector3 position)
    {
         if (position == Vector3.zero)
            return null;

        IntVector4 node = AiUtil.ConvertToIntVector4(position, 3);
        IntVector2 mark = new IntVector2(node.x, node.z);

        if (pointTable.ContainsKey(mark))
        {
            return pointTable[mark];
        }

        return null;
    }

    void LoadStaticSpawnPoints()
    {
        if (Application.loadedLevelName.Equals(GameConfig.MainSceneName))
        {
            GameObject obj = new GameObject("StaticPoints");
            obj.transform.parent = transform;

			foreach (KeyValuePair<int,AISpawnPoint> pair in AISpawnPoint.s_spawnPointData)
            {
				AISpawnPoint point = pair.Value;
                point.spPoint = SPPoint.InstantiateSPPoint<SPPoint>(point.Position,
                                                                    Quaternion.Euler(point.euler),
                                                                    IntVector4.Zero,
                                                                    obj.transform,
                                                                    0,
                                                                    point.resId,
                                                                    point.isActive,
                                                                    false,
                                                                    false,
                                                                    false,
                                                                    false,
                                                                    null,
                                                                    point.OnSpawned);

                point.spPoint.name = ("Static id = " + point.id + " , " + "path id = " + point.resId + " : ") + point.spPoint.name;
            }
        }
    }

    void RegisterMeshNode(IntVector4 node)
    {
        if (!mMeshNodes.Contains(node))
        {
            mMeshNodes.Add(node);
        }
    }

    void RemoveMeshNode(IntVector4 node)
    {
        if (mMeshNodes.Contains(node))
        {
            mMeshNodes.Remove(node);
        }
    }

	void RegisterSPTerrainRect(IntVector4 node)
	{
		node.w = VoxelTerrainMaxLod;
		IntVector2 mark = AiUtil.ConvertToIntVector2FormLodLevel(node, VoxelTerrainMaxLod);
		IntVector4 maxNode = new IntVector4(mark.x, node.y, mark.y, node.w);

		//if(!GameConfig.IsMultiMode)
		{
			if (!pointTable.ContainsKey(mark))
			{
                if (!GameConfig.IsMultiMode)
                {
                    SPTerrainRect spTerrain = InstantiateSPTerrainRect(maxNode, minCount, maxCount + 1);
                    spTerrain.RegisterMeshNode(node);
                    pointTable.Add(mark, spTerrain);
                }

				//Debug.Log("Register SPTerrainRect at position[" + mark.x + " , " + mark.y + "]");
			}
			else
			{
				pointTable[mark].RegisterMeshNode(node);
			}
		}
        //else
        //{
        //    if(!pointTable.ContainsKey(mark))
        //    {
        //        if(!requestList.Contains(mark))
        //        {
        //            RequestSPTerrainFromServer(maxNode, minCount, maxCount+1);
        //            requestList.Add(mark);
        //        }
        //    }
        //    else
        //    {
        //        pointTable[mark].RegisterMeshNode(node);
        //    }
        //}
	}

	void RemoveSPTerrainRect(IntVector4 node)
	{
		IntVector2 mark = AiUtil.ConvertToIntVector2FormLodLevel(node, VoxelTerrainMaxLod);
		if (pointTable.ContainsKey(mark))
		{
			pointTable[mark].RemoveMeshNode(node);
			if(pointTable[mark].meshNodes.Count <= 0)
			{
				pointTable[mark].Destroy();
				pointTable.Remove(mark);

				//Debug.Log("Destroy SPTerrainRect at position[" + mark.x + " , " + mark.y + "]");
			}
		}
	}

	void CleanupSPTerrainRect()
	{
		foreach (KeyValuePair<IntVector2, SPTerrainRect> pair in pointTable) 
		{
			pair.Value.Cleanup();
		}

		pointTable.Clear();
	}

	void OnTerrainColliderCreated(IntVector4 node)
	{
        //if(GameConfig.IsMultiMode || node.w != 0)
        //    return;

        //if (Application.loadedLevelName.Equals(GameConfig.AdventureSceneName))
        //    return;
		
        //IntVector2 mark = AiUtil.ConvertToIntVector2FormLodLevel(node, VoxelTerrainMaxLod);
        //IntVector2 caveMark = new IntVector2(node.x, node.z);
        //if (pointTable.ContainsKey(mark))
        //{
        //    SPTerrainRect tRect = pointTable[mark];
        //    if(tRect != null && !tRect.IsMarkExistCave(caveMark))
        //    {
        //        if (Random.value < 0.5f)
        //        {
        //            IntVector4 newNode = node;

        //            Vector3 position = AiUtil.GetRandomPositionInCave(newNode, 5);

        //            if (position == Vector3.zero)
        //            {
        //                newNode.y -= VoxelTerrainConstants._numVoxelsPerAxis;

        //                position = AiUtil.GetRandomPositionInCave(newNode, 5);
        //            }

        //            if (position != Vector3.zero)
        //            {
        //                SPPoint pointCave = SPPoint.InstantiateSPPoint<SPPoint>(position,
        //                                                                        Quaternion.AngleAxis(Random.Range(0, 360),Vector3.up),
        //                                                                        tRect.nextIndex,
        //                                                                        tRect.transform,
        //                                                                        0,
        //                                                                        0,
        //                                                                        true,
        //                                                                        false);

        //                tRect.RegisterSPPoint(pointCave);
        //                tRect.RegisterCavMark(caveMark);

        //                pointCave.name = "Cave : " + pointCave.name;
        //                //Debug.LogError("Spawn a cave point at position : " + position);
        //            }
        //        }
        //    }
        //}
	}

    void OnTerrainMeshCreated(IntVector4 node)
    {
        if (node.w == 0)
        {
			IntVector2 mark = AiUtil.ConvertToIntVector2FormLodLevel(node, VoxelTerrainMaxLod);
            if (pointTable.ContainsKey(mark))
            {
                SPTerrainRect tRect = pointTable[mark];
                if (tRect != null)
                {
                    List<SPPoint> points = tRect.points;
                    points = points.FindAll(ret => Match(ret, node.x, node.z));

                    foreach (SPPoint point in points)
                    {
                        point.AttachEvent(node);
                        //point.position = new Vector3(point.position.x, node.y, point.position.z);
                    }
                }
            }

            RegisterMeshNode(node);

			LODOctreeMan.self.AttachNodeEvents(null, null, null, null, OnTerrainColliderCreated, node.ToVector3(), 0);
        }
    }

	void OnTerrainMeshDestroy(IntVector4 node)
	{
        if (node.w == 0)
        {
            RemoveMeshNode(node);
        }
	}

    void OnTerrainPositionChange(IntVector4 preNode, IntVector4 postNode)
    {
        if (preNode.x == postNode.x && preNode.z == postNode.z)
            return;

        if(preNode.x != -999 && preNode.y != -999 && preNode.z != -999)
        {
            OnTerrainPositionDetory(preNode);
        }

        OnTerrainPositionCreated(postNode);
    }

    void OnTerrainPositionCreated(IntVector4 node)
    {
		if(!isActive)
			return;

		if (node.w == LODOctreeMan._maxLod)
        {
			RegisterSPTerrainRect(node);
        }
    }

    void OnTerrainPositionDetory(IntVector4 node)
    {
		if (node.w == LODOctreeMan._maxLod)
        {
			RemoveSPTerrainRect(node);
        }
    }

    bool Match(SPPoint point, int x, int z)
    {
        if (point == null)
            return false;

        float dx = point.position.x - x;
        float dz = point.position.z - z;

        return dx >= PETools.PEMath.Epsilon && dx <= VoxelTerrainConstants._numVoxelsPerAxis
            && dz >= PETools.PEMath.Epsilon && dz <= VoxelTerrainConstants._numVoxelsPerAxis;
    }

    SPTerrainRect InstantiateSPTerrainRect(IntVector4 node, int min, int max)
    {
        SPTerrainRect tRect = SPTerrainRect.InstantiateSPTerrainRect(node, min, max, transform, mNoise);
        return tRect;
    }

	public void RegisterAIToServer(IntVector4 index, Vector3 position, int pathID)
	{
        if (!GameConfig.IsMultiMode || null == PlayerNetwork.mainPlayer)
            return;

        if(AIResource.IsGroup(pathID))
			NetworkManager.WaitCoroutine(PlayerNetwork.RequestCreateGroupAi(pathID, position));
        else
			NetworkManager.WaitCoroutine(PlayerNetwork.RequestCreateAi(pathID, position, -1, -1,-1));
	}

    public void RegisterGift(Vector3 position, int pathID)
    {
        NetworkManager.SyncServer(EPacketType.PT_AI_Gift, position, pathID);
    }

    public void CreateMultiNativeStatic(Vector3 pos, int id, int townID)
    {
        NetworkManager.SyncServer(EPacketType.PT_AI_NativeStatic, pos, id, townID);
    }
 //   public void RegisterAIGroupMemberToServer(IntVector4 index, Vector3 position, int pathID, uLink.NetworkViewID groupId)
 //   {
 //       if (!GameConfig.IsMultiMode || !GameConfig.MonsterYes)
 //           return;

  //      RPC("RPC_C2S_SpawnAIGroupMemberAtPoint", index, position, pathID, groupId);
//    }

	//#region RPC

	//[RPC]
	//void RPC_S2C_ReceiveSPTerrainRect(uLink.BitStream stream)
	//{
	//	SPTerrainRect spTerrain = stream.Read<SPTerrainRect>();

	//	if (spTerrain != null)
	//	{
	//		IntVector2 mark = new IntVector2(spTerrain.position.x, spTerrain.position.z);

	//		//Debug.Log("Receive SPTerrainRect from server[" + mark.x + " , " + mark.y + "]");

	//		if (pointTable.ContainsKey(mark))
	//		{
	//			pointTable.Remove(mark);
	//		}

	//		pointTable.Add(mark, spTerrain);

	//		foreach (SPPoint point in spTerrain.points) {
	//			point.Activate(true);
	//		}
	//	}
	//}

	//#endregion

    public void AddSPTerrainRect(SPTerrainRect spTerrain)
    {
        if (spTerrain != null)
        {
            IntVector2 mark = new IntVector2(spTerrain.position.x, spTerrain.position.z);

            if (pointTable.ContainsKey(mark))
            {
                pointTable.Remove(mark);
            }

            pointTable.Add(mark, spTerrain);

			foreach (SPPoint point in spTerrain.points) {
				point.Activate(true);
			}
        }
    }

    //type 0:城镇npc 1:建筑npc
    public void CreateAdNpcByIndex(Vector3 pos, int key,int type = 0)
    {
//        if (GameConfig.GameType == Pathea.PeGameMgr.EGameType.VS)
//            return;
        //TODO 条件判断
		NetworkManager.SyncServer(EPacketType.PT_NPC_CreateTown, pos, key, type);
    }

//	public void CreateAdNpcListByIndex(Vector3 pos, int key,int type = 0)
//	{
////        if (GameConfig.GameType == Pathea.PeGameMgr.EGameType.VS)
////            return;
//		//TODO 条件判断
//		RPC("RPC_C2S_CreateAdNpcListByIndex", pos, key, type);
//	}


}
