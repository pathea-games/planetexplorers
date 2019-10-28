// Custom game mode  Creature manager
// (c) by Wu Yiqiu


using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Pathea;


namespace PeCustom
{
	public class CreatureMgr : Pathea.ArchivableSingleton<CreatureMgr>
	{
		public struct DataStrc
		{
			public int id;
			public string path;
			public byte[] datas;
		}

		public class EntityList:IEnumerable<DataStrc>
		{
	        Dictionary<int, DataStrc> mDatas;
			
			public EntityList(int capacity)
			{
                mDatas = new Dictionary<int, DataStrc>(capacity);

            }
			
			public void Add(int id)
			{

                DataStrc ds = new DataStrc();
                ds.id = id;
                ds.path = "";
                mDatas.Add(id, ds);
            }

			
			public bool Remove(int id)
			{
                return mDatas.Remove(id);
			}

            public bool Contain (int id)
            {
                return mDatas.ContainsKey(id);
            }

            public bool SetDataToEntity (PeEntity e)
            {
                if (mDatas.ContainsKey(e.Id))
                {
                    e.Import(mDatas[e.Id].datas);
                    return true;
                }
                return false;
            }

            public bool SetEntityToData (PeEntity e)
            {
                if (mDatas.ContainsKey(e.Id))
                {
                    DataStrc ds = mDatas[e.Id];
                    ds.datas = e.Export();
                    mDatas[e.Id] = ds;
                    return true;
                }
                return false;
            }
			
			public void Clear()
			{
                foreach (var kvp in mDatas)
                {
                    EntityMgr.Instance.Remove(kvp.Value.id);
                }
                mDatas.Clear();
			}

			public void CreateAllEntity()
			{
				//foreach (DataStrc ds in mList)
				//{
				//	PeEntity e = Create(ds.id, ds.path, Vector3.zero, Quaternion.identity, Vector3.one);

				//	if (e != null)
				//	{
				//		if (ds.datas != null)
				//		{
				//			e.Import(ds.datas);
				//		}
				//	}

				//}
			}

			#region import export
			public byte[] Export()
			{
#if UNITY_EDITOR
                //mList.RemoveAll((DataStrc ds) =>
                //                {
                //	PeEntity e = EntityMgr.Instance.Get(ds.id);

                //	if (e == null)
                //	{
                //		return true;
                //	}

                //	return false;
                //});

#endif
                try
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (BinaryWriter w = new BinaryWriter(ms))
                        {
                            w.Write((int)CURRENT_VERSION);

                            w.Write((int)mDatas.Count);

                            List<DataStrc> tempData = new List<DataStrc>(10);
                            foreach (var kvp in mDatas)
                            {
                                PeEntity e = EntityMgr.Instance.Get(kvp.Key);
                                if (e == null)
                                {
                                    if (kvp.Value.datas != null)
                                    {
                                        w.Write((int)kvp.Key);
                                        w.Write(kvp.Value.path);
                                        PETools.Serialize.WriteBytes(kvp.Value.datas, w);
                                    }
                                    else
                                    {
                                        w.Write(-1);
                                        Debug.LogError("cant find peentity with id:" + kvp.Key);
                                    }

                                }
                                else
                                {
                                    DataStrc ds = new DataStrc();
                                    ds.path = e.prefabPath;
                                    ds.id = e.Id;
                                    ds.datas = e.Export();

                                    if (ds.datas != null)
                                    {
                                        tempData.Add(ds);

                                        w.Write((int)kvp.Key);
                                        w.Write(e.prefabPath);
                                        PETools.Serialize.WriteBytes(ds.datas, w);
                                    }
                                    else
                                    {
                                        Debug.LogError("cant find peentity with id:" + kvp.Key);
                                    }
                                }
                            }

                            for (int i = 0; i < tempData.Count; i++)
                            {
                                mDatas[tempData[i].id] = tempData[i];
                            }

                            return ms.ToArray();
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError(ex.ToString());

                }

                return null;
            }

			
			public void Import(byte[] buffer)
			{
				if (buffer == null || buffer.Length == 0)
				{
					return;
				}
				
				using (MemoryStream ms = new MemoryStream(buffer, false))
				{
					using (BinaryReader r = new BinaryReader(ms))
					{
						int version = r.ReadInt32();
						if (version > CURRENT_VERSION)
						{
							Debug.LogError("error version:" + version);
						}
						
						int count = r.ReadInt32();
						
						for (int i = 0; i < count; i++)
						{
                            int id = r.ReadInt32();
                            if (id != -1)
                            {
                                DataStrc ds = new DataStrc();
                                ds.id = id;
                                ds.path = r.ReadString();
                                ds.datas = PETools.Serialize.ReadBytes(r);
                                mDatas.Add(ds.id, ds);
                            }

						}
					}
				}
			}
			#endregion
			
			protected static PeEntity Create(int id, string path, Vector3 pos, Quaternion rot, Vector3 scl)
			{
				return EntityMgr.Instance.Create(id, path, pos, rot, scl);
			}
			
			IEnumerator<DataStrc> IEnumerable<DataStrc>.GetEnumerator()
			{
                return  mDatas.Values.GetEnumerator();
			}
			
			System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
			{
                return mDatas.GetEnumerator();
            }
		}

		const int VERSION_0000 = 0;
        const int VERSION_0001 = 1;
        const int CURRENT_VERSION = VERSION_0000;

		const string ArchiveKeyCreature = "ArchiveKeyCustomCreature";

		/// <summary>
		/// List of mapping world index and Entities		
		/// </summary>
		Dictionary<int, EntityList> mEntsMap = new Dictionary<int, EntityList>();

		//int mMainPlayerId = IdGenerator.Invalid;

		protected override bool GetYird()
		{
			return false;
		}

		protected override string GetArchiveKey()
		{
			return ArchiveKeyCreature;
		}

		protected override void SetData(byte[] data)
		{
			if (null != data)
			{
				Import(data);
			}
		}

		protected override void WriteData(BinaryWriter bw)
		{
			Export(bw);
		}

		#region import export
		public void Export(BinaryWriter w)
		{
			w.Write((int)CURRENT_VERSION);
			w.Write((int)mEntsMap.Count);
			foreach (KeyValuePair<int, EntityList> kvp in mEntsMap)
			{
				w.Write(kvp.Key);
				PETools.Serialize.WriteBytes(kvp.Value.Export(), w);
			}
		}
		
		public void Import(byte[] buffer)
		{
			PETools.Serialize.Import(buffer, (r) =>
			                         {
				int version = r.ReadInt32();
				if (version > CURRENT_VERSION)
				{
					Debug.LogError("error version:" + version);
                    return;
				}

				int count = r.ReadInt32();

				for (int i = 0; i < count; i++)
				{
					int key = r.ReadInt32 ();
					byte[] data = PETools.Serialize.ReadBytes(r);
					EntityList ents = new EntityList(200);
					ents.Import(data);
					mEntsMap.Add(key, ents);
				}
			}
			);

			//int world_index = CustomGameData.Mgr.Instance.curGameData.WorldIndex;
			//if ( mEntsMap.ContainsKey(world_index))
			//{
			//	mEntsMap[world_index].CreateAllEntity();
			//}

		}


		#endregion

		public PeEntity mainPlayer
		{
			get
			{
				return MainPlayer.Instance.entity;
			}
		}
		
		public int mainPlayerId
		{
			get
			{
				return MainPlayer.Instance.entityId;
			}
		}

		public PeEntity CreateDoodad(int world_index, int id, int protoId)
		{
			if (world_index == -1)
			{
				throw new System.Exception("world index:" + world_index + " is invalid world index, can not used here.");
			}

			if (Pathea.WorldInfoMgr.Instance.IsNonRecordAutoId(id))
			{
				throw new System.Exception("id:" + id + " is non record id, can not used here. use PeEntityCreator.Instance.CreateMonster()");
			}
			
			PeEntity entity = PeEntityCreator.Instance.CreateDoodad(id, protoId, Vector3.zero, Quaternion.identity, Vector3.one);
			if (entity == null)
			{
				return null;
			}

            if (mEntsMap.ContainsKey(world_index) && mEntsMap[world_index].Contain(id))
            {
                mEntsMap[world_index].SetDataToEntity(entity);
            }
            else
            {
                AddEntity(world_index, id);
                //mEntsMap[world_index].SetEntityToData(entity);
            }
			return entity;
		}

		public PeEntity CreateMonster(int world_index, int id, int protoId)
		{
			if (world_index == -1)
			{
				throw new System.Exception("world index:" + world_index + " is invalid world index, can not used here.");
			}


			if (Pathea.WorldInfoMgr.Instance.IsNonRecordAutoId(id))
			{
				throw new System.Exception("id:" + id + " is non record id, can not used here. use PeEntityCreator.Instance.CreateMonster()");
			}
			
			PeEntity entity = PeEntityCreator.Instance.CreateMonster(id, protoId, Vector3.zero, Quaternion.identity, Vector3.one);
			if (entity == null)
			{
				return null;
			}

            if (mEntsMap.ContainsKey(world_index) && mEntsMap[world_index].Contain(id))
            {
                mEntsMap[world_index].SetDataToEntity(entity);
            }
            else
            {
                AddEntity(world_index, id);
                //mEntsMap[world_index].SetEntityToData(entity);
            }
			return entity;
		}

		public PeEntity CreateNpc(int world_index, int id, int protoId)
		{
			if (Pathea.WorldInfoMgr.Instance.IsNonRecordAutoId(id))
			{
				throw new System.Exception("id:" + id + " is non record id, can not used here. use PeEntityCreator.Instance.CreateMonster()");
			}
			
			PeEntity entity = PeEntityCreator.Instance.CreateNpc(id, protoId, Vector3.zero, Quaternion.identity, Vector3.one);
			if (entity == null)
			{
				return null;
			}


            if (mEntsMap.ContainsKey(world_index) && mEntsMap[world_index].Contain(id))
            {
                mEntsMap[world_index].SetDataToEntity(entity);
            }
            else
            {
                AddEntity(world_index, id);
                //mEntsMap[world_index].SetEntityToData(entity);
            }
			return entity;
		}

		public PeEntity CreateRandomNpc(int world_index, int templateId, int id)
		{
			if (Pathea.WorldInfoMgr.Instance.IsNonRecordAutoId(id))
			{
				throw new System.Exception("id:" + id + " is non record id, can not used here. use PeEntityCreator.Instance.CreateMonster()");
			}
			
			PeEntity entity = PeEntityCreator.Instance.CreateRandomNpc(templateId, id, Vector3.zero, Quaternion.identity, Vector3.one);
			if (null == entity)
			{
				return null;
			}
			
			AddEntity(world_index, id);
			return entity;
		}

		void AddEntity (int world_index, int entity_id)
		{
			if (mEntsMap.ContainsKey(world_index))
			{
				mEntsMap[world_index].Add(entity_id);
			}
			else
			{
				EntityList list = new EntityList(200);
				list.Add(entity_id);
				mEntsMap.Add(world_index, list);
			}


		}

		public bool Destory(int id)
		{

			foreach (KeyValuePair<int, EntityList> kvp in mEntsMap)
			{
				if ( kvp.Value.Remove(id))
					break;
			}
			return EntityMgr.Instance.Destroy(id);
		}

        public bool DestroyAndDontRemove(int id)
        {
            PeEntity e = EntityMgr.Instance.Get(id);
            if (e == null)
                return false;

            foreach (KeyValuePair<int, EntityList> kvp in mEntsMap)
            {
                if (kvp.Value.SetEntityToData(e))
                    break;
            }

            return EntityMgr.Instance.Destroy(id);
        }

		public void OnPeCreatureDestroyEntity(int id)
		{
			foreach (KeyValuePair<int, EntityList> kvp in mEntsMap)
			{
				if ( kvp.Value.Remove(id))
					break;
			}
		}
	}
}