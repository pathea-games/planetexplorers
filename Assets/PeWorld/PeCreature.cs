using System.Collections.Generic;
using System.IO;
using UnityEngine;

using Pathea.PeEntityExt;

namespace Pathea
{
    //this class must on server in multiplayer mode
    public class WorldInfoMgr : MonoLikeSingleton<WorldInfoMgr>, ISerializable
    {
        const string ArchiveKeyNameGenerator = "ArchiveKeyNameGenerator";
        const string ArchiveKeyIdGenerator = "ArchiveKeyIdGenerator";

        #region id Range define
        public const int IdBeginForAssign = 5000;
        public const int IdEndForAssign = 15000;

        public const int IdBeginForAuto = 20000;
        public const int IdEndForAuto = int.MaxValue - 1000000;

        public const int IdBeginNonRecord = IdEndForAuto;
        public const int IdEndNonRecord = int.MaxValue;
        #endregion
        //current id must be record.
        IdGenerator mRecordEntityIdGen;
        //current id will begin from IdBeginNonRecord every game launch.
        IdGenerator mNonRecordEntityIdGen;
        NameGenerater mNameGenerater;

        protected override void OnInit()
        {
            base.OnInit();
            mRecordEntityIdGen = new IdGenerator(IdBeginForAuto, IdBeginForAuto, IdEndForAuto);
            mNonRecordEntityIdGen = new IdGenerator(IdBeginNonRecord, IdBeginNonRecord, IdEndNonRecord);
            mNameGenerater = new NameGenerater();

            ArchiveMgr.Instance.Register(ArchiveKeyNameGenerator, this);
            ArchiveMgr.Instance.Register(ArchiveKeyIdGenerator, this);
        }

        void ISerializable.Serialize(PeRecordWriter w)
        {
            if (w.key == ArchiveKeyIdGenerator)
            {
                mRecordEntityIdGen.Export(w.binaryWriter);
            }
            else if (w.key == ArchiveKeyNameGenerator)
            {
                w.Write(mNameGenerater.Export());
            }
        }

        public void New()
        {
            //mRecordEntityIdGen = new IdGenerater(IdBeginForAuto, IdBeginForAuto, IdEndForAuto);
            //mNonRecordEntityIdGen = new IdGenerater(IdBeginNonRecord, IdBeginNonRecord, IdEndNonRecord);
            //mNameGenerater = new NameGenerater();
        }

        public void Restore()
        {
            byte[] data = ArchiveMgr.Instance.GetData(ArchiveKeyIdGenerator);
            if (null != data)
            {
                mRecordEntityIdGen.Import(data);
            }

            data = ArchiveMgr.Instance.GetData(ArchiveKeyNameGenerator);
            if (null != data)
            {
                mNameGenerater.Import(data);
            }
        }

        public bool IsAssignId(int id)
        {
            if (id >= IdBeginForAssign && id <= IdEndForAssign)
            {
                return true;
            }

            return false;
        }

        public bool IsNonRecordAutoId(int id)
        {
            if (id >= IdBeginNonRecord
                    && id < IdEndNonRecord)
            {
                return true;
            }

            return false;
        }

        public bool IsRecordAutoId(int id)
        {
            if (id >= IdBeginForAuto
                && id < IdEndForAuto)
            {
                return true;
            }

            return false;
        }

        public int FetchNonRecordAutoId()
        {
            return mNonRecordEntityIdGen.Fetch();
        }

        public int FetchRecordAutoId()
        {
            return mRecordEntityIdGen.Fetch();
        }

        public CharacterName FetchName(PeSex sex, int race)
        {
            return mNameGenerater.Fetch(sex, race);
        }
    }

    public class PeCreature : Pathea.ArchivableSingleton<PeCreature>
    {
        public class EntityList:IEnumerable<int>
        {
            List<int> mList;

            public EntityList(int capacity)
            {
                mList = new List<int>(capacity);
            }

            public bool Contains(int id)
            {
                return mList.Contains(id);
            }

            public void Add(int id)
            {
              mList.Add(id);
            }

            public int GetIdByIndex(int index)
            {
                if (index < 0 || index >= mList.Count)
                {
                    return IdGenerator.Invalid;
                }

                return mList[index];
            }

            public bool Remove(int id)
            {
                return mList.Remove(id);
            }

            public void Clear()
            {
                foreach (int id in mList)
                {
                    EntityMgr.Instance.Remove(id);
                }

                mList.Clear();
            }

            #region import export
			public void Export(BinaryWriter bw)
            {
#if UNITY_EDITOR
                mList.RemoveAll((id) =>
                {
                    PeEntity e = EntityMgr.Instance.Get(id);

                    if (e == null)
                    {
                        return true;
                    }

                    return false;
                });
#endif

				bw.Write((int)CURRENT_VERSION);
                bw.Write((int)mList.Count);
                for (int i = 0; i < mList.Count; i++) {
					WriteEntity (bw, mList [i]);
				}
            }

            public static void WriteEntity(BinaryWriter w, int id)
            {
                PeEntity e = EntityMgr.Instance.Get(id);
                if (null != e)
                {
                    w.Write((int)id);
					w.Write(e.prefabPath);
					PETools.Serialize.WriteData(e.Export, w);
                }
                else
                {
					w.Write((int)id);
					w.Write(string.Empty);
					PETools.Serialize.WriteData(null, w);
                    Debug.LogError("cant find peentity with id:"+id);
                }
            }

            public static int ReadEntity(BinaryReader r)
            {
                int id = r.ReadInt32();
                string prefabPath = r.ReadString();
                byte[] data = PETools.Serialize.ReadBytes(r);

				if (!string.IsNullOrEmpty (prefabPath)) {
					PeEntity e = Create (id, prefabPath, Vector3.zero, Quaternion.identity, Vector3.one);
					e.Import (data);
				}
                return id;
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
                            //int id = r.ReadInt32();

                            //string prefabPath = r.ReadString();

                            //byte[] data = PETools.Serialize.ReadBytes(r);

                            //PeEntity e = Create(id, prefabPath);

                            //e.Import(data);

                            int id = ReadEntity(r);

                            Add(id);
                        }
                    }
                }
            }
            #endregion

			protected static PeEntity Create(int id, string path, Vector3 pos, Quaternion rot, Vector3 scl)
            {
				return EntityMgr.Instance.Create(id, path, pos, rot, scl);
            }

            IEnumerator<int> IEnumerable<int>.GetEnumerator()
            {
                return mList.GetEnumerator();                
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return mList.GetEnumerator();
            }
        }

        const int VERSION_0000 = 0;
        const int CURRENT_VERSION = VERSION_0000;

        const string ArchiveKeyCreature = "ArchiveKeyCreature";

        EntityList mList = new EntityList(100);

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
		public void Export(BinaryWriter bw)
        {
			bw.Write((int)CURRENT_VERSION);
			PETools.Serialize.WriteData(mList.Export, bw);
        }

        public void Import(byte[] buffer)
        {
            PETools.Serialize.Import(buffer, (r) =>
            {
                int version = r.ReadInt32();
                if (version > CURRENT_VERSION)
                {
                    Debug.LogError("error version:" + version);
                }

                byte[] data = PETools.Serialize.ReadBytes(r);
                mList.Import(data);
            }
            );
        }
        #endregion

        #region public interface

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

		public PeEntity CreateDoodad(int id, int protoId, Vector3 pos, Quaternion rot, Vector3 scl)
		{
			if (Pathea.WorldInfoMgr.Instance.IsNonRecordAutoId(id))
			{
				throw new System.Exception("id:" + id + " is non record id, can not used here. use PeEntityCreator.Instance.CreateMonster()");
			}
			
			PeEntity entity = PeEntityCreator.Instance.CreateDoodad(id, protoId, pos, rot, scl);
			if (entity == null)
			{
				return null;
			}
			mList.Add(id);
			return entity;
		}

		public PeEntity CreateMonster(int id, int protoId, Vector3 pos, Quaternion rot, Vector3 scl, float exScale = 1.0f)
        {
            if (Pathea.WorldInfoMgr.Instance.IsNonRecordAutoId(id))
            {
                throw new System.Exception("id:" + id + " is non record id, can not used here. use PeEntityCreator.Instance.CreateMonster()");
            }

			PeEntity entity = PeEntityCreator.Instance.CreateMonster(id, protoId, pos, rot, scl, exScale);
            if (entity == null)
            {
                return null;
            }
            mList.Add(id);
            return entity;
        }

		public PeEntity CreateNpc(int id, int protoId, Vector3 pos, Quaternion rot, Vector3 scl)
        {
            if (Pathea.WorldInfoMgr.Instance.IsNonRecordAutoId(id))
            {
                throw new System.Exception("id:" + id + " is non record id, can not used here. use PeEntityCreator.Instance.CreateMonster()");
            }

			PeEntity entity = PeEntityCreator.Instance.CreateNpc(id, protoId, pos, rot, scl);
            if (entity == null)
            {
                return null;
            }


			PeEntity robot = PeEntityCreator.Instance.CreateNpcRobot(id,protoId,pos,rot,scl);
			if(robot != null)
			    mList.Add(robot.Id);


            mList.Add(id);
            return entity;
        }

		public PeEntity CreateRandomNpc(int templateId, int id, Vector3 pos, Quaternion rot, Vector3 scl)
        {
            if (Pathea.WorldInfoMgr.Instance.IsNonRecordAutoId(id))
            {
                throw new System.Exception("id:" + id + " is non record id, can not used here. use PeEntityCreator.Instance.CreateMonster()");
            }

			PeEntity entity = PeEntityCreator.Instance.CreateRandomNpc(templateId, id, pos, rot, scl);
            if (null == entity)
            {
                return null;
            }

            mList.Add(id);
            return entity;
        }

		public System.Action<int> destoryEntityEvent;

		public bool Destory(int id)
		{
			mList.Remove(id);

			if (destoryEntityEvent != null)
				destoryEntityEvent(id);

			return EntityMgr.Instance.Destroy(id);
		}

        public void Add(int id)
        {
            if(!mList.Contains(id))
                mList.Add(id);
        }

        public bool Remove(int id)
        {
            return mList.Remove(id); ;
        }

        #endregion
    }

    public class MainPlayer : Pathea.ArchivableSingleton<MainPlayer>
    {
        const int VERSION_0000 = 0;
        const int CURRENT_VERSION = VERSION_0000;

        public class MainPlayerCreatedArg : PeEvent.EventArg
        {
            public int id;
        }

        public PeEvent.Event<MainPlayerCreatedArg> mainPlayerCreatedEventor = new PeEvent.Event<MainPlayerCreatedArg>();

        int mMainPlayerId = IdGenerator.Invalid;

        //cross scene
        protected override bool GetYird()
        {
            return false;
        }

        public int entityId
        {
            get
            {
                return mMainPlayerId;
            }
        }

		PeEntity mEntity;

        public PeEntity entity
        {
            get
            {
				if(null == mEntity)
					mEntity = EntityMgr.Instance.Get(mMainPlayerId);
				return mEntity;
            }
        }

        public void SetEntityId(int id)
        {
            mMainPlayerId = id;

            MainPlayerCreatedArg e = new MainPlayerCreatedArg();
            e.id = id;

            mainPlayerCreatedEventor.Dispatch(e);
        }

		protected override void WriteData(BinaryWriter w)
        {
            w.Write((int)CURRENT_VERSION);

            PeCreature.EntityList.WriteEntity(w, mMainPlayerId);
        }

        protected override void SetData(byte[] data)
        {
            PETools.Serialize.Import(data, (r) =>
            {
                int version = r.ReadInt32();
                if (version > CURRENT_VERSION)
                {
                    Debug.LogError("error version:" + version);
                }

                int id = PeCreature.EntityList.ReadEntity(r);
                SetEntityId(id);
            });
        }

		public PeEntity CreatePlayer(int id, Vector3 pos, Quaternion rot, Vector3 scl, CustomCharactor.CustomData data = null)
        {
			PeEntity entity = PeEntityCreator.Instance.CreatePlayer(id, pos, rot, scl, data);
            if (null == entity)
            {
                return null;
            }

            SetEntityId(id);
            return entity;
        }
    }
}