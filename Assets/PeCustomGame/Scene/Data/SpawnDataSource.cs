// Custom game mode  Scene Spawn point data source
// (c) by Wu Yiqiu

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Pathea;

namespace PeCustom
{
	public class SpawnDataSource : PeCustomScene.SceneElement, Pathea.ISerializable
	{
		// Monster single Spawn point List 
		Dictionary<int, MonsterSpawnPoint> mMsts;

		// Area Spawn list
		Dictionary<int, MonsterSpawnArea> mMstAreas;

		// NPC SP
		Dictionary<int, NPCSpawnPoint>   mNpcs;

		// Doodad SP
		Dictionary<int, DoodadSpawnPoint> mDoodads;

		// Item Sp
		Dictionary<int, ItemSpwanPoint> mItems;

		// Effect : not save
		Dictionary<int, EffectSpwanPoint> mEffects;

		public Dictionary<int, MonsterSpawnPoint> monsters { get { return mMsts; }}
		public Dictionary<int, MonsterSpawnArea>  areas { get { return mMstAreas; }}
		public Dictionary<int, NPCSpawnPoint>	  npcs { get { return mNpcs; }}
		public Dictionary<int, DoodadSpawnPoint>  doodads { get { return mDoodads;}}
		public Dictionary<int, EffectSpwanPoint>  effects { get { return mEffects;}}
		public Dictionary<int, ItemSpwanPoint>	  items { get { return mItems;}}

        private int mMaxSpawnPointId = 10000;

        public	SpawnDataSource ()
		{
			mMsts = new Dictionary<int, MonsterSpawnPoint>(50);
			mMstAreas = new Dictionary<int, MonsterSpawnArea>(50);
			mNpcs = new Dictionary<int, NPCSpawnPoint>(50);
			mDoodads = new Dictionary<int, DoodadSpawnPoint>(20);
			mEffects = new Dictionary<int, EffectSpwanPoint>(20);
			mItems = new Dictionary<int, ItemSpwanPoint>(50);

			// Archive
			Pathea.ArchiveMgr.Instance.Register(ArchiveKey, this, false, Pathea.ArchiveMgr.RecordNameWorld, true);

		}

        public int GenerateId ()
        {
            return ++mMaxSpawnPointId;
        }

        public bool ContainMonster(int id)
        {
            return mMsts.ContainsKey(id);
        }

        public bool ContainArea(int id)
        {
            return mMstAreas.ContainsKey(id);
        }
        
        public bool ContainNpc(int id)
        {
            return mNpcs.ContainsKey(id);
        }

        public bool ContainDoodad (int id)
        {
            return mDoodads.ContainsKey(id);
        }

        public bool ContainItem (int id)
        {
            return mItems.ContainsKey(id);
        }

        public bool ContainEffect (int id)
        {
            return mEffects.ContainsKey(id);
        }

        public SpawnPoint GetSpawnPoint (int id)
        {
            if (ContainMonster(id))
                return GetMonster(id);
            else if (ContainArea(id))
                return GetMonsterArea(id);
            else if (ContainNpc(id))
                return GetNpc(id);
            else if (ContainDoodad(id))
                return GetDoodad(id);
            else if (ContainItem(id))
                return GetItem(id);
            else if (mEffects.ContainsKey(id))
                return GetEffect(id);
            
            return null;
        }

		public MonsterSpawnPoint GetMonster(int id)
		{
			if (!mMsts.ContainsKey(id))
				return null;

			return mMsts[id];
		}

		public MonsterSpawnArea GetMonsterArea(int id)
		{
			if (!mMstAreas.ContainsKey(id))
				return null;

			return mMstAreas[id];
		}

		public NPCSpawnPoint GetNpc (int id)
		{
			if (!mNpcs.ContainsKey(id))
				return null;

			return mNpcs[id];
		}

		public DoodadSpawnPoint GetDoodad (int id)
		{
			if (!mDoodads.ContainsKey(id))
				return null;

			return mDoodads[id];
		}

        public ItemSpwanPoint GetItem(int id)
        {
            return mItems[id];
        }

        public EffectSpwanPoint GetEffect(int id)
        {
            return mEffects[id];
        }

        public bool AddMonster (MonsterSpawnPoint msp)
        {
            if (!mMsts.ContainsKey(msp.ID))
            {
                mMsts.Add(msp.ID, msp);
                return true;
            }
            return false;
        }

        public bool AddNpc (NPCSpawnPoint nsp)
        {
            if (!mNpcs.ContainsKey(nsp.ID))
            {
                mNpcs.Add(nsp.ID, nsp);
                return true;
            }

            return false;
        }

        public bool AddDoodad (DoodadSpawnPoint dsp)
        {
            if (!mDoodads.ContainsKey(dsp.ID))
            {
                mDoodads.Add(dsp.ID, dsp);
                return true;
            }
            
            return false;
        }

        public bool AddItem (ItemSpwanPoint isp)
        {
            if (!mItems.ContainsKey(isp.ID))
            {
                mItems.Add(isp.ID, isp);
                return true;
            }

            return false;
        }

        public bool RemoveMonster (int id)
        {
            return mMsts.Remove(id);
        }

        public bool RemoveNpc (int id)
        {
            return mNpcs.Remove(id);
        }

        public bool RemoveDoodad (int id)
        {
            return mDoodads.Remove(id);
        }

        public bool RemoveItem(int id)
        {
            return mItems.Remove(id);
        }

        public void ClearMonster()
		{
			mMsts.Clear();
			mMstAreas.Clear();
		}
		
		public void ClearNpc()
		{
			mNpcs.Clear();
		}
		
		public void ClearDoodad()
		{
			mDoodads.Clear();
		}

		public void ClearItems()
		{
			mItems.Clear();
		}

		public void ClearEffects()
		{
			mEffects.Clear();
		}


		void SetMonsters(IEnumerable<WEMonster> items)
		{
			if (items == null)
				return;
			// Clear First
			ClearMonster();


			foreach (WEMonster item in items)
			{
				// this item is a  area
				if (item.AreaSpwan)
				{
					MonsterSpawnArea sa = new MonsterSpawnArea(item);
					sa.CalcSpawns();

					mMstAreas.Add(sa.ID, sa);
				}
				else
				{
					MonsterSpawnPoint sp = new MonsterSpawnPoint(item);

					mMsts.Add(sp.ID, sp);
				}
			}
		}

		void SetNPCs ( IEnumerable<WENPC> items)
		{
			if (items == null)
				return;

			// Clear first 
			ClearNpc();

			foreach (WENPC item in items)
			{
				NPCSpawnPoint sp = new NPCSpawnPoint(item);


				mNpcs.Add(sp.ID, sp);
			}
		}

		void SetDoodads (IEnumerable<WEDoodad> items)
		{
			if (items == null)
				return;
			ClearDoodad();

			foreach (WEDoodad item in items)
			{
				DoodadSpawnPoint sp = new DoodadSpawnPoint(item);

				mDoodads.Add(sp.ID, sp);
			}
		}

		void SetItems (IEnumerable<WEItem> items)
		{
			if (items == null)
				return;

			ClearItems();

			foreach (WEItem item in items)
			{
				ItemSpwanPoint sp = new ItemSpwanPoint(item);

				mItems.Add(sp.ID, sp);
			}
		}

		void SetEffects (IEnumerable<WEEffect> items)
		{
			if (items == null)
				return;

			foreach (WEEffect item in items)
			{
				EffectSpwanPoint sp = new EffectSpwanPoint(item);

				mEffects.Add(sp.ID, sp);
			}
		}


		public void Restore(YirdData yird)
		{
			byte[] data = Pathea.ArchiveMgr.Instance.GetData(ArchiveKey);
			if (data == null || data.Length == 0)
			{
				New(yird);
			}
			else
				SetData(data);

			SetEffects(yird.GetEffects());
			
		}

		public void New(YirdData yird)
		{
			SetMonsters(yird.GetMonsters());
			SetNPCs(yird.GetNpcs());
			SetDoodads(yird.GetDoodads());
			SetItems(yird.GetItems());
			SetEffects(yird.GetEffects());
		}

		#region ISerializable


		public const string ArchiveKey = "CustomSceneSpawnPoints";
        //const int VERSION = 0x0000001;
        //const int VERSION = 0x0000002;
        //const int VERSION = 0x0000003;
        const int VERSION = 0x0000004;

        void Pathea.ISerializable.Serialize(Pathea.PeRecordWriter w)
		{
			w.Write(GetData());
		}

		byte[] GetData()
		{
			byte[] data = null;
			using ( MemoryStream ms_iso = new MemoryStream () )
			{
				BinaryWriter w = new BinaryWriter (ms_iso);

				w.Write(VERSION);

                w.Write(mMaxSpawnPointId);

				w.Write(mMsts.Count);
				foreach (KeyValuePair<int, MonsterSpawnPoint> kvp in mMsts)
					kvp.Value.Serialize(w);

				w.Write(mMstAreas.Count);
				foreach (KeyValuePair<int, MonsterSpawnArea> kvp in mMstAreas)
					kvp.Value.Serialize(w);

				w.Write(mNpcs.Count);
				foreach (KeyValuePair<int, NPCSpawnPoint> kvp in mNpcs)
					kvp.Value.Serialize(w);

				w.Write(mDoodads.Count);
				foreach (KeyValuePair<int, DoodadSpawnPoint> kvp in mDoodads)
					kvp.Value.Serialize(w);

				w.Write(mItems.Count);
				foreach (KeyValuePair<int, ItemSpwanPoint> kvp in mItems)
					kvp.Value.Serialize(w);

				data = ms_iso.ToArray();
				w.Close();
			}
			return data;
		}

		void SetData(byte[] data)
		{
			using ( MemoryStream ms_iso = new MemoryStream (data) )
			{
				BinaryReader r = new BinaryReader (ms_iso);
				
				int version = r.ReadInt32();

				switch(version)
				{
				    case 0x0000001:
                    case 0x0000002:
                    case 0x0000003:
                        {
                            int count = r.ReadInt32();

                            for (int i = 0; i < count; i++)
                            {
                                MonsterSpawnPoint sp = new MonsterSpawnPoint();
                                sp.Deserialize(version, r);
                                mMsts.Add(sp.ID, sp);
                            }

                            //-- Area Spawn point list
                            count = r.ReadInt32();
                            for (int i = 0; i < count; i++)
                            {
                                MonsterSpawnArea sa = new MonsterSpawnArea();
                                sa.Deserialize(version, r);
                                mMstAreas.Add(sa.ID, sa);

                            }

                            // -- NPC
                            count = r.ReadInt32();
                            for (int i = 0; i < count; i++)
                            {
                                NPCSpawnPoint sp = new NPCSpawnPoint();
                                sp.Deserialize(version, r);
                                mNpcs.Add(sp.ID, sp);
                            }

                            // Doodad
                            count = r.ReadInt32();
                            for (int i = 0; i < count; i++)
                            {
                                DoodadSpawnPoint sp = new DoodadSpawnPoint();
                                sp.Deserialize(version, r);
                                mDoodads.Add(sp.ID, sp);
                            }

                            // Item
                            count = r.ReadInt32();
                            for (int i = 0; i < count; i++)
                            {
                                ItemSpwanPoint sp = new ItemSpwanPoint();
                                sp.Deserialize(version, r);
                                mItems.Add(sp.ID, sp);
                            }
                        }
                        break;
                    case 0x0000004:
                        {
                            mMaxSpawnPointId = r.ReadInt32();
                            int count = r.ReadInt32();
                           

                            for (int i = 0; i < count; i++)
                            {
                                MonsterSpawnPoint sp = new MonsterSpawnPoint();
                                sp.Deserialize(version, r);
                                mMsts.Add(sp.ID, sp);
                            }

                            //-- Area Spawn point list
                            count = r.ReadInt32();
                            for (int i = 0; i < count; i++)
                            {
                                MonsterSpawnArea sa = new MonsterSpawnArea();
                                sa.Deserialize(version, r);
                                mMstAreas.Add(sa.ID, sa);

                            }

                            // -- NPC
                            count = r.ReadInt32();
                            for (int i = 0; i < count; i++)
                            {
                                NPCSpawnPoint sp = new NPCSpawnPoint();
                                sp.Deserialize(version, r);
                                mNpcs.Add(sp.ID, sp);
                            }

                            // Doodad
                            count = r.ReadInt32();
                            for (int i = 0; i < count; i++)
                            {
                                DoodadSpawnPoint sp = new DoodadSpawnPoint();
                                sp.Deserialize(version, r);
                                mDoodads.Add(sp.ID, sp);
                            }

                            // Item
                            count = r.ReadInt32();
                            for (int i = 0; i < count; i++)
                            {
                                ItemSpwanPoint sp = new ItemSpwanPoint();
                                sp.Deserialize(version, r);
                                mItems.Add(sp.ID, sp);
                            }
                        }
                        break;

                    default: break;
				
				}
			}
		}

		#endregion
	}


}