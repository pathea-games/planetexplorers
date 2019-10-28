using UnityEngine;
using System.Collections;
using TownData;

public partial class PlayerNetwork
{
	public void RequestMakeMask (byte index, Vector3 pos, int iconId, string desc)
	{
		RPCServer (EPacketType.PT_InGame_MakeMask, index, pos, iconId, desc);
	}

	public void RequestRemoveMask (byte index)
	{
		RPCServer (EPacketType.PT_InGame_RemoveMask, index);
	}

	void RPC_S2C_MakeMask (uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		byte index = stream.Read<byte> ();
		Vector3 pos = stream.Read<Vector3> ();
		int iconId = stream.Read<int> ();
		string desc = stream.Read<string> ();

		if (ForceSetting.Instance.Conflict(Id, mainPlayerId))
			return;

		PeMap.UserLabel userLabel = (PeMap.UserLabel)PeMap.LabelMgr.Instance.Find (iter =>
		{
			if (iter is PeMap.UserLabel) {
				PeMap.UserLabel label = (PeMap.UserLabel)iter;
				return label.playerID == Id && label.index == index;
			}

			return false;
		});

		if (null != userLabel)
			PeMap.LabelMgr.Instance.Remove (userLabel);

		userLabel = new PeMap.UserLabel ();
		userLabel.pos = pos;
		userLabel.icon = iconId;
		userLabel.text = desc;
		userLabel.index = index;
		userLabel.playerID = Id;
		PeMap.LabelMgr.Instance.Add (userLabel);
	}
	
	void RPC_S2C_RemoveMask (uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		byte index = stream.Read<byte> ();

		if (ForceSetting.Instance.Conflict(Id, mainPlayerId))
			return;

		PeMap.UserLabel userLabel = (PeMap.UserLabel)PeMap.LabelMgr.Instance.Find (iter => 
		{
			if (iter is PeMap.UserLabel) {
				PeMap.UserLabel label = (PeMap.UserLabel)iter;
				return label.playerID == Id && label.index == index;
			}

			return false;
		});

		if (null != userLabel)
			PeMap.LabelMgr.Instance.Remove (userLabel);
	}

	void RPC_S2C_TownAreaList (uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		Vector3[] townArea = stream.Read<Vector3[]> ();

		if (ForceSetting.Instance.Conflict(Id, mainPlayerId))
			return;

		foreach (Vector3 townPos in townArea) {
			IntVector2 center = new IntVector2 (Mathf.RoundToInt (townPos.x), Mathf.RoundToInt (townPos.z));
			if (VArtifactTownManager.Instance.TownPosInfo.ContainsKey (center)) {
				VArtifactTown ti = VArtifactTownManager.Instance.TownPosInfo [center];
				//int id = ti.townId;

				if(VArtifactTownManager.Instance.IsCaptured(ti.townId))
					RandomMapIconMgr.AddDestroyedTownIcon(ti);
				else{
					RandomMapIconMgr.AddTownIcon (ti);
					DetectedTownMgr.Instance.AddDetectedTown (ti);
				}
				foreach (VArtifactUnit vau in ti.VAUnits)
				{
					vau.isDoodadNpcRendered = true;
				}
				ti.IsExplored = true;
				VArtifactTownManager.Instance.DetectTowns (ti);
			}

		}
	}

	void RPC_S2C_CampAreaList (uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		Vector3[] campArea = stream.Read<Vector3[]> ();

		if (ForceSetting.Instance.Conflict(Id, mainPlayerId))
			return;

		foreach (Vector3 townPos in campArea) {
			IntVector2 center = new IntVector2 (Mathf.RoundToInt (townPos.x), Mathf.RoundToInt (townPos.z));
			if (VArtifactTownManager.Instance.TownPosInfo.ContainsKey (center)) {
				//TownInfo ti = RandomTownManager.Instance.TownPosInfo[center];
				//WorldMapManager.Instance.AddCamp(townPos);
				//ti.IsExplored = true;
				VArtifactTown ti = VArtifactTownManager.Instance.TownPosInfo [center];
				//int id = ti.townId;
				if(VArtifactTownManager.Instance.IsCaptured(ti.townId))
					RandomMapIconMgr.AddDestroyedTownIcon(ti);
				else
					RandomMapIconMgr.AddNativeIcon (ti);
				foreach (VArtifactUnit vau in ti.VAUnits)
				{
					vau.isDoodadNpcRendered = true;
				}
				ti.IsExplored = true;
				VArtifactTownManager.Instance.DetectTowns (ti);
			}

		}
	}

	void RPC_S2C_MaskAreaList (uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		byte[] binData = stream.Read<byte[]> ();

		if (ForceSetting.Instance.Conflict(Id, mainPlayerId))
			return;

		PETools.Serialize.Import (binData, (r) =>
		{
			int count = BufferHelper.ReadInt32 (r);
			for (int i = 0; i < count; i++) {
				byte index = BufferHelper.ReadByte (r);
				int iconId = BufferHelper.ReadInt32 (r);
				Vector3 pos;
				BufferHelper.ReadVector3 (r, out pos);
				string desc = BufferHelper.ReadString (r);
				
				PeMap.UserLabel label = new PeMap.UserLabel ();
				label.pos = pos;
				label.icon = iconId;
				label.text = desc;
				label.index = index;
				label.playerID = Id;
				
				PeMap.LabelMgr.Instance.Add (label);
			}
		});

		//WorldMapManager.InitMaskArea(areas);
	}

	void RPC_S2C_AddTownArea (uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		Vector3 townPos = stream.Read<Vector3> ();

		if (LogFilter.logDebug) Debug.LogFormat("<color=blue>Add town pos:{0}</color>", townPos);

		if (ForceSetting.Instance.Conflict(Id, mainPlayerId))
			return;

		IntVector2 center = new IntVector2 (Mathf.RoundToInt (townPos.x), Mathf.RoundToInt (townPos.z));
		if (VArtifactTownManager.Instance.TownPosInfo.ContainsKey (center)) {
			VArtifactTown ti = VArtifactTownManager.Instance.TownPosInfo [center];
			//int id = ti.townId;
			DetectedTownMgr.Instance.AddDetectedTown (ti);
			RandomMapIconMgr.AddTownIcon (ti);
			foreach (VArtifactUnit vau in ti.VAUnits)
			{
				vau.isDoodadNpcRendered = true;
			}
			ti.IsExplored = true;
			VArtifactTownManager.Instance.DetectTowns (ti);
		}

	}

	void RPC_S2C_AddCampArea (uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		Vector3 townPos = stream.Read<Vector3> ();

		if (ForceSetting.Instance.Conflict(Id, mainPlayerId))
			return;

		IntVector2 center = new IntVector2 (Mathf.RoundToInt (townPos.x), Mathf.RoundToInt (townPos.z));
		if (VArtifactTownManager.Instance.TownPosInfo.ContainsKey (center)) {
			//TownInfo ti = RandomTownManager.Instance.TownPosInfo[center];
			//WorldMapManager.Instance.AddCamp(townPos);
			//ti.IsExplored = true;
			VArtifactTown ti = VArtifactTownManager.Instance.TownPosInfo [center];
			//int id = ti.townId;
			RandomMapIconMgr.AddNativeIcon (ti);
			foreach (VArtifactUnit vau in ti.VAUnits)
			{
				vau.isDoodadNpcRendered = true;
			}
			ti.IsExplored = true;
			VArtifactTownManager.Instance.DetectTowns (ti);
		}
	}

	void RPC_S2C_ExploredArea (uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int index = stream.Read<int> ();

		if (ForceSetting.Instance.Conflict(Id, mainPlayerId))
			return;

		Vector2 tilePos = PeMap.MaskTile.Mgr.Instance.GetCenterPos (index);
		byte type = PeMap.MaskTile.Mgr.Instance.GetType ((int)tilePos.x, (int)tilePos.y);

		PeMap.MaskTile tile = PeMap.MaskTile.Mgr.Instance.Get(index);
		if (null == tile)
		{
			tile = new PeMap.MaskTile();
			tile.index = index;
			tile.forceGroup = -1;
			tile.type = type;
			PeMap.MaskTile.Mgr.Instance.Add(index, tile);
		}
	}

	void RPC_S2C_ExploredAreas (uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int[] indexes = stream.Read<int[]> ();

		if (ForceSetting.Instance.Conflict(Id, mainPlayerId))
			return;

		foreach (int index in indexes)
		{
			Vector2 tilePos = PeMap.MaskTile.Mgr.Instance.GetCenterPos(index);
			byte type = PeMap.MaskTile.Mgr.Instance.GetType((int)tilePos.x, (int)tilePos.y);

			PeMap.MaskTile tile = PeMap.MaskTile.Mgr.Instance.Get(index);
			if (null == tile)
			{
				tile = new PeMap.MaskTile();
				tile.index = index;
				tile.forceGroup = -1;
				tile.type = type;
				PeMap.MaskTile.Mgr.Instance.Add(index, tile);
			}
		}
	}
}