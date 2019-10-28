// Custom game mode: Here is all Scene Object spawned infomation 
// (c) by Wu Yiqiu


using UnityEngine;
using System.Collections.Generic;
using System.IO;

namespace PeCustom
{
	public class SpawnPoint
	{
		public int ID;
		public int EntityID = -1;

		public Vector3 spawnPos;
		public Quaternion Rotation;
		
		public Vector3 Scale;

		public string Name = "No Name";

		public int Prototype;
		public int PlayerIndex;
		public bool IsTarget;
		public bool Visible;

		public bool isDead = false;

        public Vector3 entityPos;

        public bool Enable = true;

        public SpawnPoint()
		{

		}

		public SpawnPoint (WEEntity ety)
		{
			ID 				= ety.ID;
			Name			= ety.ObjectName;
            spawnPos        = ety.Position;
			Rotation		= ety.Rotation;
			Scale			= ety.Scale;
			Prototype		= ety.Prototype;
			PlayerIndex		= ety.PlayerIndex;
			IsTarget		= ety.IsTarget;
			Visible			= ety.Visible;

            entityPos = ety.Position;

        }

		public SpawnPoint (SpawnPoint sp)
		{
			ID 				= sp.ID;
			EntityID		= sp.EntityID;
			Name			= sp.Name;
            spawnPos        = sp.spawnPos;
			Rotation		= sp.Rotation;
			Scale			= sp.Scale;
			Prototype		= sp.Prototype;
			PlayerIndex		= sp.PlayerIndex;
			IsTarget		= sp.IsTarget;
			Visible			= sp.Visible;

            entityPos = sp.entityPos;
		}

		public virtual void Serialize(BinaryWriter bw)
		{
			bw.Write(ID);
			bw.Write(EntityID);
            bw.Write(Name);

			bw.Write(spawnPos.x); 
			bw.Write(spawnPos.y); 
			bw.Write(spawnPos.z);
			bw.Write(Rotation.x); 
			bw.Write(Rotation.y); 
			bw.Write(Rotation.z); 
			bw.Write(Rotation.w);
			bw.Write(Scale.x); 
			bw.Write(Scale.y); 
			bw.Write(Scale.z);

			bw.Write(Prototype);
			bw.Write(PlayerIndex);
			bw.Write(IsTarget);
			bw.Write(Visible);
			bw.Write(isDead);

            bw.Write(entityPos.x);
            bw.Write(entityPos.y);
            bw.Write(entityPos.z);

            bw.Write(Enable);
        }

		public virtual void Deserialize(int version, BinaryReader br)
		{
			switch (version)
			{
			    case 0x0000001:
                case 0x0000002:
                    {
                        ID = br.ReadInt32();
                        EntityID = br.ReadInt32();

                        spawnPos.x = br.ReadSingle();
                        spawnPos.y = br.ReadSingle();
                        spawnPos.z = br.ReadSingle();
                        Rotation.x = br.ReadSingle();
                        Rotation.y = br.ReadSingle();
                        Rotation.z = br.ReadSingle();
                        Rotation.w = br.ReadSingle();
                        Scale.x = br.ReadSingle();
                        Scale.y = br.ReadSingle();
                        Scale.z = br.ReadSingle();

                        Prototype = br.ReadInt32();
                        PlayerIndex = br.ReadInt32();
                        IsTarget = br.ReadBoolean();
                        Visible = br.ReadBoolean();
                        isDead = br.ReadBoolean();
                    }

				break;
                case 0x0000003:
                    {
                        ID = br.ReadInt32();
                        EntityID = br.ReadInt32();
                        Name     = br.ReadString();

                        spawnPos.x = br.ReadSingle();
                        spawnPos.y = br.ReadSingle();
                        spawnPos.z = br.ReadSingle();
                        Rotation.x = br.ReadSingle();
                        Rotation.y = br.ReadSingle();
                        Rotation.z = br.ReadSingle();
                        Rotation.w = br.ReadSingle();
                        Scale.x = br.ReadSingle();
                        Scale.y = br.ReadSingle();
                        Scale.z = br.ReadSingle();

                        Prototype = br.ReadInt32();
                        PlayerIndex = br.ReadInt32();
                        IsTarget = br.ReadBoolean();
                        Visible = br.ReadBoolean();
                        isDead = br.ReadBoolean();
                    }
                    break;
                case 0x0000004:
                    {
                        ID = br.ReadInt32();
                        EntityID = br.ReadInt32();
                        Name = br.ReadString();

                        spawnPos.x = br.ReadSingle();
                        spawnPos.y = br.ReadSingle();
                        spawnPos.z = br.ReadSingle();
                        Rotation.x = br.ReadSingle();
                        Rotation.y = br.ReadSingle();
                        Rotation.z = br.ReadSingle();
                        Rotation.w = br.ReadSingle();
                        Scale.x = br.ReadSingle();
                        Scale.y = br.ReadSingle();
                        Scale.z = br.ReadSingle();

                        Prototype = br.ReadInt32();
                        PlayerIndex = br.ReadInt32();
                        IsTarget = br.ReadBoolean();
                        Visible = br.ReadBoolean();
                        isDead = br.ReadBoolean();

                        entityPos.x = br.ReadSingle();
                        entityPos.y = br.ReadSingle();
                        entityPos.z = br.ReadSingle();
                        Enable = br.ReadBoolean();
                    }
                    break;
                default:
				break;
			}
		}
	}

	/// <summary>
	/// The Doodad spawn point.
	/// </summary>
	public class DoodadSpawnPoint :SpawnPoint
	{
        public SceneStaticAgent agent;
		public DoodadSpawnPoint() : 
			base()
		{

		}

		public DoodadSpawnPoint(WEDoodad sp)
			: base (sp)
		{

		}

		public DoodadSpawnPoint(DoodadSpawnPoint sp)
			: base(sp)
		{
		}
	}

	public class EffectSpwanPoint : SpawnPoint
	{
		public EffectSpwanPoint() : 
			base()
		{
			
		}
		
		public EffectSpwanPoint(WEEffect obj)
		{
			ID 				= obj.ID;
			Name			= obj.ObjectName;
            spawnPos        = obj.Position;
			Rotation		= obj.Rotation;
			Scale			= obj.Scale;
			Prototype		= obj.Prototype;
			PlayerIndex		= -1;
            IsTarget		= false;
            Visible			= true;
        }
        
		public EffectSpwanPoint(EffectSpwanPoint sp)
            : base(sp)
		{

		}
	}

	public class ItemSpwanPoint : SpawnPoint
	{
		public int ItemObjId;
		public bool CanPickup;
        public bool isNew = true;

        public DragArticleAgent agent;

		public ItemSpwanPoint() : 
			base()
		{

		}
		
		public ItemSpwanPoint(WEItem obj)
			: base(obj)
		{
			ItemObjId = -1;
			CanPickup = obj.CanPickup;
		}
		
		public ItemSpwanPoint(EffectSpwanPoint sp)
			: base(sp)
		{
			
		}

		public override void Serialize (BinaryWriter bw)
		{
			base.Serialize(bw);
			bw.Write(ItemObjId);
			bw.Write(CanPickup);

            // Version 0x0000002 以后
            bw.Write(isNew);
        }

        public override void Deserialize (int version, BinaryReader br)
		{
			base.Deserialize (version, br);

			switch (version)
			{
			    case 0x0000001:
                    {
                        ItemObjId = br.ReadInt32();
                        CanPickup = br.ReadBoolean();
                    }
                    break;
                case 0x0000002:
                case 0x0000003:
                case 0x0000004:
                    {
                        ItemObjId = br.ReadInt32();
                        CanPickup = br.ReadBoolean();
                        isNew = br.ReadBoolean();
                    }
				    break;
			    default:
				    break;
			}
		}
	}
	

	/// <summary>
	/// The NPC spawn point.
	/// </summary>
	public class NPCSpawnPoint : SpawnPoint
	{
        public SceneEntityAgent agent;

        public NPCSpawnPoint()
		{

		}

		public NPCSpawnPoint(WENPC npc)
			: base(npc)
		{

		}

		public NPCSpawnPoint(NPCSpawnPoint sp)
			: base(sp)
		{
		}

		public override void Serialize (BinaryWriter bw)
		{
			base.Serialize (bw);

		}

		public override void Deserialize (int version, BinaryReader br)
		{
			base.Deserialize (version, br);

			switch (version)
			{
			    case 0x0000001:
                case 0x0000002:
                case 0x0000003:
                case 0x0000004:
                    break;
			default:
				break;
			}

		}
	}

	/// <summary>
	/// The Monster spawn point.
	/// </summary>
	public class MonsterSpawnPoint : SpawnPoint
	{
		public int MaxRespawnCount;
		public float RespawnTime;

		public Bounds bound;

		public SceneEntityAgent agent;


		public MonsterSpawnPoint()
		{

		}

		public  MonsterSpawnPoint(WEMonster mst) 
			: base(mst)
		{
			MaxRespawnCount = mst.MaxRespawnCount;
			RespawnTime		= mst.RespawnTime;
		}
        
		public MonsterSpawnPoint(MonsterSpawnPoint sp)
			: base(sp)
		{

			MaxRespawnCount = sp.MaxRespawnCount;
			RespawnTime		= sp.RespawnTime;
		}


		#region IMPROT_EXPORT

		public override void Serialize(BinaryWriter bw)
		{

			base.Serialize(bw);
			bw.Write(MaxRespawnCount);
			bw.Write(RespawnTime);
			bw.Write(bound.center.x); bw.Write(bound.center.y); bw.Write(bound.center.z);
			bw.Write(bound.size.x); bw.Write(bound.size.y); bw.Write(bound.size.z);
		}

		public override void Deserialize(int version, BinaryReader br)
		{
			base.Deserialize(version, br);
			switch (version)
			{
                case 0x0000001:
                case 0x0000002:
                case 0x0000003:
                case 0x0000004:
                    {
                        MaxRespawnCount = br.ReadInt32();
                        RespawnTime = br.ReadSingle();

                        bound = new Bounds(new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle()),
                                           new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle()));
                    }
				break;
			default:
				break;
			}
		}

		#endregion

		#region HELP_FUNC


		private float mCurTime;


		public bool UpdateRespawnTime(float time_delta)
		{
			if (isDead)
			{
				mCurTime += time_delta;

				if (mCurTime >= RespawnTime)
				{
					mCurTime = 0;
					return true;
				}
			}
			return false;
		}
		#endregion
	}

	public class MonsterSpawnArea : MonsterSpawnPoint
	{
		public int SpawnAmount
		{
			get;
			private set;
		}

		public int AmountPerSocial
		{
			get;
			private set;
		}

		public bool IsSocial
		{
			get;
			private set;
		}

		const float c_Unit = 1;

		List<Bounds> mInnerBounds = new List<Bounds>();
		public Bounds[] innerBounds  { get { return mInnerBounds.ToArray();} }

		// Social Part
		public class SocialSpawns
		{
			public List<MonsterSpawnPoint> spawnPoints;
			public bool isSocial;
			public MonsterSpawnPoint centerSP;
			
			public SocialSpawns(bool social, MonsterSpawnArea center)
			{
				isSocial = social;
				spawnPoints = new List<MonsterSpawnPoint>(10);
				centerSP = center;
            }
            
            public void Clear()
            {
                spawnPoints.Clear();
            }
        }
        
		List<SocialSpawns> mSpawns = new List<SocialSpawns>(10);
		public List<SocialSpawns> Spawns { get { return mSpawns; } }

		public void CalcSpawns ()
		{
			for (int i = 0; i < mSpawns.Count; i++)
				mSpawns[i].Clear();
			mSpawns.Clear();

			CalcInnerBounds(SpawnAmount);

			if (!IsSocial)
			{
				SocialSpawns ss = new SocialSpawns(false, this);
				RandomPoints(ss.spawnPoints);
				mSpawns.Add(ss);
			}
			else
			{
				RandomPointsForSocials(mSpawns);
			}
		}

		public MonsterSpawnArea()
		{

		}

		public MonsterSpawnArea (WEMonster mst)
			: base(mst)
		{
			SpawnAmount = mst.SpawnAmount;
			AmountPerSocial = mst.AmountPerSocial;
			IsSocial = mst.IsSocial;
		}

		public MonsterSpawnArea (MonsterSpawnArea sa)
			: base(sa)
		{
			SpawnAmount = sa.SpawnAmount;
			AmountPerSocial = sa.AmountPerSocial;

			mSpawns = new List<SocialSpawns>(sa.Spawns);
		}

		public override void Serialize (BinaryWriter bw)
		{
			base.Serialize (bw);
			bw.Write(SpawnAmount);
			bw.Write(AmountPerSocial);
			bw.Write(IsSocial);

			bw.Write(mSpawns.Count);
			foreach (SocialSpawns ss in mSpawns)
			{
				bw.Write(ss.isSocial);
				bw.Write(ss.spawnPoints.Count);
				foreach (MonsterSpawnPoint msp in ss.spawnPoints)
				{
					msp.Serialize(bw);
				}
			}
		}

		public override void Deserialize (int version, BinaryReader br)
		{
			base.Deserialize (version, br);

			switch (version)
			{
			    case 0x0000001:
                case 0x0000002:
                case 0x0000003:
                case 0x0000004:
                    {
                        SpawnAmount = br.ReadInt32();
                        AmountPerSocial = br.ReadInt32();
                        IsSocial = br.ReadBoolean();

                        int count = br.ReadInt32();
                        for (int i = 0; i < count; i++)
                        {
                            SocialSpawns ss = new SocialSpawns(br.ReadBoolean(), this);
                            int subcnt = br.ReadInt32();
                            for (int j = 0; j < subcnt; j++)
                            {
                                MonsterSpawnPoint msp = new MonsterSpawnPoint();
                                msp.Deserialize(version, br);
                                ss.spawnPoints.Add(msp);
                            }

                            mSpawns.Add(ss);

                        }
                    }
				    break;
			default:
				break;
			}

		}

		public void SetAmount (int amount)
		{
			SpawnAmount = amount;
        }
        
        public bool PointIn (Vector3 pos)
		{

			Vector3 inv_pos = spawnPos + Quaternion.Inverse(Rotation) * (pos - spawnPos);

			float minx = spawnPos.x - Scale.x * 0.5f;
			float miny = spawnPos.y - Scale.y * 0.5f;
			float minz = spawnPos.z - Scale.z * 0.5f;
			float maxx = spawnPos.x + Scale.x * 0.5f;
			float maxy = spawnPos.y + Scale.y * 0.5f;
			float maxz = spawnPos.z + Scale.z * 0.5f;


			if (inv_pos.x > minx && inv_pos.x < maxx
			    && inv_pos.y > miny && inv_pos.y < maxy
			    && inv_pos.z > minz && inv_pos.z < maxz)
				return true;

			return false;
		}

		void RandomPoints (List<MonsterSpawnPoint> points)
		{
			List<Bounds> bounds = new List<Bounds>(mInnerBounds);

			try
			{
				for (int i = 0; i < SpawnAmount; i++)
				{
					if (bounds.Count == 0)
						throw new System.Exception ("error");

					int index = Random.Range(0, bounds.Count - 1);
					
					MonsterSpawnPoint sp = new MonsterSpawnPoint(this);
					Vector3 center = bounds[index].center;
					Vector3 size = bounds[index].size;
					
					sp.spawnPos = new Vector3(center.x + (Random.value * 2 - 1) * size.x * 0.3f, 
					                          center.y + (Random.value * 2 - 1) * size.y * 0.3f,
					                          center.z + (Random.value * 2 - 1) * size.z * 0.3f);
					sp.spawnPos = center + Rotation * (sp.spawnPos - center);
					sp.Rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
                    sp.Scale = Vector3.one;
                    sp.bound = bounds[index];
                    points.Add(sp);
                    
                    bounds.RemoveAt(index);
				}
			}
			catch (System.Exception ex)
			{
				Debug.LogError(ex.ToString());
            }
		}

		void RandomPointsForSocials(List<SocialSpawns> list)
		{
			List<Bounds> bounds = new List<Bounds>(mInnerBounds);

			int curCount = SpawnAmount;

			while(curCount > 0)
			{
				// is Group
				if (curCount >= AmountPerSocial)
				{
					SocialSpawns ss = new SocialSpawns(true, this);
					for (int i = 0; i < AmountPerSocial; i++)
					{
						int index = Random.Range(0, bounds.Count - 1);
						
						MonsterSpawnPoint sp = new MonsterSpawnPoint(this);
						Vector3 center = bounds[index].center;
						Vector3 size = bounds[index].size;
						
						sp.spawnPos = new Vector3(center.x + (Random.value * 2 - 1) * size.x * 0.3f, 
						                          center.y + (Random.value * 2 - 1) * size.y * 0.3f,
						                          center.z + (Random.value * 2 - 1) * size.z * 0.3f);
						
						sp.spawnPos = spawnPos + Rotation * (sp.spawnPos - spawnPos);
						sp.Rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
						sp.Scale = Vector3.one;
						sp.bound = bounds[index];
						
						ss.spawnPoints.Add(sp);
						bounds.RemoveAt(index);
					}
					
					list.Add(ss);
				}
				else
				{
					SocialSpawns ss = new SocialSpawns(false, null);
					for (int i = 0; i < curCount; i++)
					{
						int index = Random.Range(0, bounds.Count - 1);
						MonsterSpawnPoint sp = new MonsterSpawnPoint(this);
						Vector3 center = bounds[index].center;
						Vector3 size = bounds[index].size;
                        
                        sp.spawnPos = new Vector3(center.x + (Random.value * 2 - 1) * size.x * 0.3f, 
                                                  center.y + (Random.value * 2 - 1) * size.y * 0.3f,
                                                  center.z + (Random.value * 2 - 1) * size.z * 0.3f);
                        sp.spawnPos = spawnPos + Rotation * (sp.spawnPos - spawnPos);
                        sp.Rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
                        sp.Scale = Vector3.one;
                        sp.bound = bounds[index];
                        
                        ss.spawnPoints.Add(sp);
                        bounds.RemoveAt(index);
                    }
                    
                    list.Add(ss);
                }
                
				curCount -= AmountPerSocial;
			}

		}

		public List<MonsterSpawnPoint> RandomPoints ()
		{
			List<MonsterSpawnPoint> points = new List<MonsterSpawnPoint>();

			List<Bounds> bounds = new List<Bounds>(mInnerBounds);

			try
			{
				for (int i = 0; i < SpawnAmount; i++)
				{
					if (bounds.Count == 0)
						throw new System.Exception ("error");
					int index = Random.Range(0, bounds.Count - 1);

					MonsterSpawnPoint sp = new MonsterSpawnPoint(this);
					Vector3 center = bounds[index].center;
					Vector3 size = bounds[index].size;

					sp.spawnPos = new Vector3(center.x + (Random.value * 2 - 1) * size.x * 0.3f, 
					                          center.y + (Random.value * 2 - 1) * size.y * 0.3f,
					                          center.z + (Random.value * 2 - 1) * size.z * 0.3f);
					sp.spawnPos = center + Rotation * (sp.spawnPos - center);
					sp.Rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
					sp.Scale = Vector3.one;
					sp.bound = bounds[index];
					points.Add(sp);

					bounds.RemoveAt(index);
				}
			}
			catch (System.Exception ex)
			{
				Debug.LogError(ex.ToString());
			}

            
			return points;
		}
        

		public List<SocialSpawns> RandomPointsForSocials ()
		{
			List<SocialSpawns> list = new List<SocialSpawns>();
			List<Bounds> bounds = new List<Bounds>(mInnerBounds);

			int curCount = SpawnAmount;
			
			while(curCount > 0)
			{
				// is Group
				if (curCount >= AmountPerSocial)
				{
					SocialSpawns ss = new SocialSpawns(true, this);
					for (int i = 0; i < AmountPerSocial; i++)
					{
						int index = Random.Range(0, bounds.Count - 1);

						MonsterSpawnPoint sp = new MonsterSpawnPoint(this);
						Vector3 center = bounds[index].center;
						Vector3 size = bounds[index].size;

						sp.spawnPos = new Vector3(center.x + (Random.value * 2 - 1) * size.x * 0.3f, 
						                          center.y + (Random.value * 2 - 1) * size.y * 0.3f,
						                          center.z + (Random.value * 2 - 1) * size.z * 0.3f);

						sp.spawnPos = spawnPos + Rotation * (sp.spawnPos - spawnPos);
						sp.Rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
						sp.Scale = Vector3.one;
						sp.bound = bounds[index];

						ss.spawnPoints.Add(sp);
						bounds.RemoveAt(index);
					}

					list.Add(ss);
				}
				else
				{
					SocialSpawns ss = new SocialSpawns(false, null);
					for (int i = 0; i < curCount; i++)
					{
						int index = Random.Range(0, bounds.Count - 1);
						MonsterSpawnPoint sp = new MonsterSpawnPoint(this);
						Vector3 center = bounds[index].center;
						Vector3 size = bounds[index].size;
						
						sp.spawnPos = new Vector3(center.x + (Random.value * 2 - 1) * size.x * 0.3f, 
						                          center.y + (Random.value * 2 - 1) * size.y * 0.3f,
						                          center.z + (Random.value * 2 - 1) * size.z * 0.3f);
						sp.spawnPos = spawnPos + Rotation * (sp.spawnPos - spawnPos);
						sp.Rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
						sp.Scale = Vector3.one;
						sp.bound = bounds[index];

						ss.spawnPoints.Add(sp);
						bounds.RemoveAt(index);
					}

					list.Add(ss);
				}

				curCount -= AmountPerSocial;
			}

			return list;
		}



		#region INNER_BOUNDS
		void CalcInnerBounds (int amount)
		{
			mInnerBounds.Clear();

			// x is min
			if (Scale.x <= Scale.y && Scale.x <= Scale.z)
			{

				int y_ratio = Mathf.Clamp( Mathf.FloorToInt( Scale.y / Mathf.Max(0.01f, Scale.x)), 1, 10);
				int x_ratio = 1;
				int z_ratio = Mathf.Clamp( Mathf.FloorToInt( Scale.z / Mathf.Max(0.01f, Scale.x)), 1, 10);
				
				
				int total_cnt = x_ratio * y_ratio * z_ratio;
				
				while (total_cnt < amount)
				{
					x_ratio ++;
					y_ratio = Mathf.Clamp( Mathf.FloorToInt( Scale.y / Mathf.Max(0.01f, Scale.x)), 1, 10) * x_ratio;
					z_ratio = Mathf.Clamp( Mathf.FloorToInt( Scale.z / Mathf.Max(0.01f, Scale.x)), 1, 10) * x_ratio;
					total_cnt = x_ratio * y_ratio * z_ratio;
				}

				bool firstY = (y_ratio < z_ratio);
				// correct ratio one more time
				while (total_cnt >= amount * 2)
				{
					if (firstY)
					{
						if (y_ratio != 1)
							y_ratio = Mathf.CeilToInt(y_ratio * 0.5f);
						else if (z_ratio != 1)
							z_ratio = Mathf.CeilToInt(z_ratio * 0.5f);
						else
							break;
					}
					else
					{
						if (z_ratio != 1)
							z_ratio = Mathf.CeilToInt(z_ratio * 0.5f);
						else if (y_ratio != 1)
							y_ratio = Mathf.CeilToInt(y_ratio * 0.5f);
						else
							break;
					}

					firstY = !firstY;


					total_cnt = x_ratio * y_ratio * z_ratio;
				}

				_calcInnerBounds(x_ratio, y_ratio, z_ratio);
			}
			// y is min
			else if (Scale.y <= Scale.z && Scale.y <= Scale.x)
			{
				int y_ratio = 1;
				int x_ratio = Mathf.Clamp(Mathf.FloorToInt( Scale.x / Mathf.Max(0.01f, Scale.y)), 1, 10);
				int z_ratio = Mathf.Clamp(Mathf.FloorToInt( Scale.z / Mathf.Max(0.01f, Scale.y)), 1, 10);
				

				int total_cnt = x_ratio * y_ratio * z_ratio;

				while (total_cnt < amount)
				{
					y_ratio ++;
					x_ratio =  Mathf.Clamp(Mathf.FloorToInt( Scale.x / Mathf.Max(0.01f, Scale.y)), 1, 10) * y_ratio;
					z_ratio = Mathf.Clamp(Mathf.FloorToInt( Scale.z / Mathf.Max(0.01f, Scale.y)), 1, 10) * y_ratio;
					total_cnt = x_ratio * y_ratio * z_ratio;
				}

				// correct ratio one more time
				bool firstX = (x_ratio < z_ratio);
				while (total_cnt >= amount * 2)
				{
					if (firstX)
					{
						if (x_ratio != 1)
							x_ratio = Mathf.CeilToInt(x_ratio * 0.5f);
						else if (z_ratio != 1)
							z_ratio = Mathf.CeilToInt(z_ratio * 0.5f);
						else
							break;
					}
					else
					{
						if (z_ratio != 1)
							z_ratio = Mathf.CeilToInt(z_ratio * 0.5f);
						else if (x_ratio != 1)
							x_ratio = Mathf.CeilToInt(x_ratio * 0.5f);
						else
							break;
					}

					firstX = !firstX;
//                        
                    total_cnt = x_ratio * y_ratio * z_ratio;
                }

				_calcInnerBounds(x_ratio, y_ratio, z_ratio);
			}
			// z is min
			else// if (Scale.z <= Scale.x && Scale.z <= Scale.y)
			{
				int y_ratio = Mathf.Clamp(Mathf.FloorToInt( Scale.y / Mathf.Max(0.01f, Scale.z)), 1, 10);
				int x_ratio = Mathf.Clamp(Mathf.FloorToInt( Scale.x / Mathf.Max(0.01f, Scale.z)), 1, 10);
				int z_ratio = 1;
				
				
				int total_cnt = x_ratio * y_ratio * z_ratio;
				
				while (total_cnt < amount)
				{
					z_ratio ++;
					y_ratio = Mathf.Clamp(Mathf.FloorToInt( Scale.y / Mathf.Max(0.01f, Scale.z)), 1, 10) * z_ratio;
					x_ratio = Mathf.Clamp(Mathf.FloorToInt( Scale.x / Mathf.Max(0.01f, Scale.z)), 1, 10) * z_ratio;
					total_cnt = x_ratio * y_ratio * z_ratio;
				}

				// correct ratio one more time
				bool firstX = (x_ratio < y_ratio);
				while (total_cnt >= amount * 2)
				{
					if (firstX)
					{
						if (x_ratio != 1)
							x_ratio = Mathf.CeilToInt(x_ratio * 0.5f);
						else if (y_ratio != 1)
							y_ratio = Mathf.CeilToInt(y_ratio * 0.5f);
						else
							break;
					}
					else
					{
						if (y_ratio != 1)
							y_ratio = Mathf.CeilToInt(y_ratio * 0.5f);
						else if (x_ratio != 1)
							x_ratio = Mathf.CeilToInt(x_ratio * 0.5f);
						else
							break;
					}

					firstX = !firstX;
                    
                    total_cnt = x_ratio * y_ratio * z_ratio;
                }
                
                _calcInnerBounds(x_ratio, y_ratio, z_ratio);
            }
        }
        
        
        void _calcInnerBounds (int xcnt, int ycnt, int zcnt)
        {
            float per_x = Scale.x / xcnt;
			float per_y = Scale.y / ycnt;
			float per_z = Scale.z / zcnt;
			float half_per_x = per_x * 0.5f;
			float half_per_y = per_y * 0.5f;
			float half_per_z = per_z * 0.5f;
			
			float start_x = spawnPos.x - (Scale.x * 0.5f);
			float start_y = spawnPos.y - (Scale.y * 0.5f);
			float start_z = spawnPos.z - (Scale.z * 0.5f);
			
			for (int x = 0; x < xcnt; x++)
			{
				for (int y = 0; y < ycnt; y++)
				{
					for (int z = 0; z < zcnt; z++)
					{
						Vector3 c = new Vector3(start_x + x * per_x + half_per_x, 
						                        start_y + y * per_y + half_per_y,
                                                start_z + z * per_z + half_per_z);
                        
                        Bounds bound = new Bounds(c, new Vector3(per_x, per_y, per_z));
                        mInnerBounds.Add(bound);
                    }
                }
            }
        }
		#endregion


	}
}
