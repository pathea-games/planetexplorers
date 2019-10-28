using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

using CustomData;
using System.IO;
using NaturalResAsset;

internal class ChunkEventArgs : EventArgs
{
	internal Vector3 _pos;

	internal ChunkEventArgs(Vector3 pos)
	{
		_pos = pos;
	}
}

public class ChunkManager : MonoBehaviour
{
	private static ChunkManager _instance;
	public static ChunkManager Instance { get { return _instance; } }

	internal Dictionary<IntVector3, Dictionary<IntVector3, VFVoxel>> VoxelVolumes = new Dictionary<IntVector3, Dictionary<IntVector3, VFVoxel>>();
	internal Dictionary<int, Dictionary<IntVector3, B45Block>> _areaBlockList = new Dictionary<int, Dictionary<IntVector3, B45Block>>();

	void Awake()
	{
		_instance = this;
	}

	public void AddCacheReq(Vector3 pos)
	{
		if (!VoxelVolumes.ContainsKey(new IntVector3(pos)))
			return;

		ActionDelegate action = new ActionDelegate(this, new ActionEventHandler(OnChunkAddedEvent), new ChunkEventArgs(pos));
		ActionManager.AddAction(action);
	}

	void OnChunkAddedEvent(object sender, EventArgs args)
	{
		if (null == VFVoxelTerrain.self)
			return;

		ChunkEventArgs chunkArgs = args as ChunkEventArgs;
		IntVector3 chunkPos = new IntVector3(chunkArgs._pos.x, chunkArgs._pos.y, chunkArgs._pos.z);
		if (VoxelVolumes.ContainsKey(chunkPos))
		{
			foreach (KeyValuePair<IntVector3, VFVoxel> kv in VoxelVolumes[chunkPos])
			{
				VFVoxel voxel = VFVoxelTerrain.self.Voxels.SafeRead(kv.Key.x, kv.Key.y, kv.Key.z);
				voxel.Volume = kv.Value.Volume;
				voxel.Type = kv.Value.Type;
				VFVoxelTerrain.self.AlterVoxelInBuild(kv.Key, voxel);
			}
		}
	}

	internal static void Clear()
	{
		if(Instance == null)
			return;
		Instance.VoxelVolumes.Clear();
		Instance._areaBlockList.Clear();
	}

	public static void ApplyVoxelVolume(IntVector3 digPos, IntVector3 chunkPos, VFVoxel voxel)
	{
		if (!Instance.VoxelVolumes.ContainsKey(chunkPos))
			Instance.VoxelVolumes[chunkPos] = new Dictionary<IntVector3, VFVoxel>();

		Instance.VoxelVolumes[chunkPos][digPos] = new VFVoxel(voxel.Volume,voxel.Type);
    }

	#region Action Callback APIs
	public static void RPC_S2C_BlockDestroyInRange(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		byte[] data = stream.Read<byte[]>();
		DigTerrainManager.BlockDestroyInRangeNetReturn(data);
	}

	public static void RPC_S2C_TerrainDestroyInRange(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		Vector3 pos = stream.Read<Vector3>();
		float power = stream.Read<float>();
		float radius = stream.Read<float>();

		DigTerrainManager.TerrainDestroyInRangeNetReturn(pos, power, radius);
	}

	public static void RPC_S2C_VoxelData(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		/*int areaIndex = */stream.Read<int>();
		byte[] data = stream.Read<byte[]>();
		DigTerrainManager.ApplyVoxelData(data);
	}
	
	public static void RPC_S2C_BlockData(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		/*int index = */stream.Read<int>();
		byte[] data = stream.Read<byte[]>();
		DigTerrainManager.ApplyBlockData(data);
	}

    public static void RPC_S2C_BuildBlock(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        byte[] binData = stream.Read<byte[]>();
        DigTerrainManager.ApplyBSVoxelData(binData);
    }

    public static void RPC_SKDigTerrain(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        IntVector3 pos = stream.Read<IntVector3>();
        float durDec = stream.Read<float>();
        float radius = stream.Read<float>();
        float height = stream.Read<float>();
        bool bReturnItem = stream.Read<bool>();

        DigTerrainManager.DigTerrainNetReturn(pos, durDec, radius, height, bReturnItem);
    }

    public static void RPC_SKChangeTerrain(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
		IntVector3 intPos = stream.Read<IntVector3>();
		float radius = stream.Read<float>();
		byte targetType = stream.Read<byte>();
		byte[] data = stream.Read<byte[]>();

		DigTerrainManager.ChangeTerrainNetReturn(intPos, radius, targetType, data);
    }
    #endregion
}
