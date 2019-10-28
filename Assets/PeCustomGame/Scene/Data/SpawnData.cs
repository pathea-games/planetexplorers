using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace PeCustom
{
	public enum SpawnType
	{
		Monster,
		Npc,
		Doodad,
		Effect
	}

	public class SpawnData
	{
		public int ID;
		public int EntityID = -1;

		public SpawnType Type;
		
		public Vector3 Position;
		public Quaternion Rotation;
		
		public Vector3 Scale;
		
		public string Name = "No Name";
		
		public int Prototype;
		public int PlayerIndex;
		public bool IsTarget;
		public bool Visible;

		public bool isDead = false;

		public int MaxRespawnCount;
		public float RespawnTime;
		
		public Bounds bound;

		public void Serialize(BinaryWriter bw)
		{

		}

		public void Deserialize(int version, BinaryReader br)
		{

		}
	}

	public class SpawnAreaData
	{
		public List<int> SpawnIds;


		public int MaxRespawnCount;
		public float RespawnTime;

		public int SpawnAmount;

		public int AmountPerSocial;

		public bool IsSocial;

		public void Serialize(BinaryWriter bw)
		{
			
		}
		
		public void Deserialize(int version, BinaryReader br)
		{
			
		}
	}
}