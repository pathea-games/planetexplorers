using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using SkillAsset;

public partial class PlayerNetwork
{
	#region Action Callback APIs
	void RPC_S2C_InitAdminData(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
		byte[] data = stream.Read<byte[]>();
		ServerAdministrator.DeserializeAdminData(data);
    }

    void RPC_S2C_AddBlackList(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        int id = stream.Read<int>();
		ServerAdministrator.AddBlacklist(id);
    }

    void RPC_S2C_DeleteBlackList(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
		int id = stream.Read<int>();
		ServerAdministrator.DeleteBlacklist(id);
    }

	void RPC_S2C_ClearBlackList(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        ServerAdministrator.ClearBlacklist();
    }
    
	void RPC_S2C_AddAssistants(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
		int id = stream.Read<int>();
		ServerAdministrator.AddAssistant(id);
    }

	void RPC_S2C_DeleteAssistants(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
		int id = stream.Read<int>();
        ServerAdministrator.DeleteAssistant(id);
    }

	void RPC_S2C_ClearAssistants(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        ServerAdministrator.ClearAssistant();
    }

	void RPC_S2C_ClearVoxelData(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
		int index = stream.Read<int>();

		if (ChunkManager.Instance._areaBlockList.ContainsKey(index))
		{
			foreach (KeyValuePair<IntVector3, B45Block> iter in ChunkManager.Instance._areaBlockList[index])
				Block45Man.self.DataSource.SafeWrite(new B45Block(0, 0), iter.Key.x, iter.Key.y, iter.Key.z);

			ChunkManager.Instance._areaBlockList.Remove(index);
		}
    }

	void RPC_S2C_ClearAllVoxelData(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		foreach (KeyValuePair<int, Dictionary<IntVector3, B45Block>> kv in ChunkManager.Instance._areaBlockList)
		{
			foreach (KeyValuePair<IntVector3, B45Block> iter in kv.Value)
				Block45Man.self.DataSource.SafeWrite(new B45Block(0, 0), iter.Key.x, iter.Key.y, iter.Key.z);
		}

		ChunkManager.Instance._areaBlockList.Clear();
	}
    
	void RPC_S2C_LockArea(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        int index = stream.Read<int>();
		ServerAdministrator.LockArea(index);
    }

	void RPC_S2C_UnLockArea(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        int index = stream.Read<int>();
		ServerAdministrator.UnLockArea(index);
    }

	void RPC_S2C_BuildLock(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
		int id = stream.Read<int>();
		ServerAdministrator.BuildLock(id);
    }

	void RPC_S2C_BuildUnLock(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
		int id = stream.Read<int>();
		ServerAdministrator.BuildUnLock(id);
    }

	void RPC_S2C_ClearBuildLock(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
		ServerAdministrator.ClearBuildLock();
    }

	void RPC_S2C_BuildChunk(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
		bool allowModify = stream.Read<bool>();
		ServerAdministrator.SetBuildChunk(allowModify);
    }

	void RPC_S2C_JoinGame(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
		bool allowJoin = stream.Read<bool>();
		ServerAdministrator.SetJoinGame(allowJoin);
	}
	#endregion
}
