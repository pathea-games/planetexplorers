using UnityEngine;
using System.Collections.Generic;
using System.IO;

namespace Pathea
{
    public partial class PeEntity : MonoBehaviour
    {
		static List<MonoBehaviour> s_tmpLstCmps = new List<MonoBehaviour> (20);

        public const int VERSION_0000 = 0;
        public const int VERSION_0001 = VERSION_0000+1;
        public const int VERSION_0002 = VERSION_0001 + 1;
        public const int CURRENT_VERSION = VERSION_0002;
        //used for some no archive to add archive
        [System.NonSerialized]
        public int version = 0;

        string mPrefabPath;
        [SerializeField]
        int m_Id;

		List<IPeMsg> mMsgListener = new List<IPeMsg>();

        public int scenarioId = 0;

        #region public function
        public string prefabPath
        {
            get
            {
                return mPrefabPath;
            }
        }

        public EntityProto entityProto
        {
            get;
            set;
        }

        public void SetId(int id)
        {
            m_Id = id;
        }

        public int Id
        {
            get
            {
                return m_Id;
            }
        }

        public GameObject GetGameObject()
        {
            return gameObject;
        }

        public T Add<T>() where T : MonoBehaviour, IPeCmpt
        {
            return gameObject.AddComponent<T>();
        }

		public void AddMsgListener(IPeMsg peMsg)
		{
			mMsgListener.Add(peMsg);
		}
		public void RemoveMsgListener(IPeMsg peMsg)
		{
			mMsgListener.Remove(peMsg);
		}

        public bool Remove(IPeCmpt cmpt)
        {
            if (cmpt is MonoBehaviour)
            {
                Object.Destroy(cmpt as MonoBehaviour);
            }
            return true;
        }

        IPeCmpt GetCmpt(string cmptName)
        {
            return gameObject.GetComponent(cmptName) as IPeCmpt;
        }

		void GetCmpts(List<MonoBehaviour> lst)
        {
			lst.Clear ();
			gameObject.GetComponents<MonoBehaviour>(lst);

			lst.RemoveAll((item) =>
            {
                if (item is IPeCmpt)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            });
        }

        public T GetCmpt<T>() where T : MonoBehaviour, IPeCmpt
        {
            return gameObject.GetComponent<T>();
        }

        public void SendMsg(EMsg msg, params object[] args)
        {
			for(int i = 0; i < mMsgListener.Count; ++i)
            {
				mMsgListener[i].OnMsg(msg, args);
            }
        }

        #endregion

        #region import export
		public void Export(BinaryWriter w)
        {
#if UNITY_EDITOR
            if (gameObject == null)
            {
                Debug.Log("<color=yellow> entity:" + Id + " is destoryed, prefab path:" + mPrefabPath + "</color>");
            }
#endif
            w.Write((int)CURRENT_VERSION);
            if (entityProto != null)
            {
                w.Write((int)entityProto.proto);
                w.Write((int)entityProto.protoId);
            }
            else
            {
                w.Write((int)-1);
                w.Write((int)-1);
            }

			GetCmpts(s_tmpLstCmps);
			w.Write(s_tmpLstCmps.Count);

			for (int i = 0; i < s_tmpLstCmps.Count; i++) {
				IPeCmpt cmpt = s_tmpLstCmps [i] as IPeCmpt;
				if (cmpt != null) {
					w.Write (cmpt.GetTypeName ());
					PETools.Serialize.WriteData(cmpt.Serialize, w);
				}
			}
        }
		public byte[] Export()
		{
			using (MemoryStream ms = new MemoryStream()) {
				using (BinaryWriter w = new BinaryWriter(ms)) {
					Export(w);
				}
				return ms.ToArray();
			}
		}

        public void Import(byte[] buffer)
        {
            PETools.Serialize.Import(buffer, (r) =>
            {
                version = r.ReadInt32();
                if (version > CURRENT_VERSION)
                {
                    Debug.LogError("error version:" + version);
                    return;
                }

                if (version >= VERSION_0002)
                {
                    int entityPrototype = r.ReadInt32();
                    int entityPrototypeId = r.ReadInt32();

                    if (entityPrototype != -1)
                    {
                        entityProto = new EntityProto()
                        {
                            proto = (EEntityProto)entityPrototype,
                            protoId = entityPrototypeId
                        };
                    }

                }

                int count = r.ReadInt32();

                for (int i = 0; i < count; i++)
                {
                    string name = r.ReadString();

                    byte[] buff = PETools.Serialize.ReadBytes(r);
                    if (null == buff || buff.Length <= 0)
                    {
                        continue;
                    }

                    IPeCmpt c = GetCmpt(name);
                    if (null != c)
                    {
                        PETools.Serialize.Import(buff, (r1) =>
                        {
                            c.Deserialize(r1);
                        });
                    }
                }
            });
        }

        #endregion

        #region factory

        public static bool Destroy(PeEntity entity)
        {
            if (entity == null)
            {
                Debug.LogError("entity is null");
                return false;
            }

            Object.Destroy(entity.GetGameObject());
            return true;
        }

		public static PeEntity Create(string path, Vector3 pos, Quaternion rot, Vector3 scl)
        {
            try
            {
				//Debug.LogError("[PeEntity]load entity object:" + path);
                GameObject obj = AssetsLoader.Instance.InstantiateAssetImm(path, pos, rot, Vector3.one); // scl apply to PeTrans
                if (null == obj)
                {
                    Debug.LogError("cant load entity object:" + path);
                    return null;
                }

                PeEntity e = obj.GetComponent<PeEntity>();
                if (null == e)
                {
                    e = obj.AddComponent<PeEntity>();
                }

				PeTrans t = e.peTrans;				
				if (null != t)
				{
					t.position = pos;
					t.rotation = rot;
					t.scale = scl;
				}

                //for restore
                e.mPrefabPath = path;
                return e;
            }
            catch (System.Exception e)
            {
                Debug.LogError(e + " : " + path);
                return null;
            }
        }

        #endregion

        #region for debug
        EntityInfoCmpt mInfoCmpt;
        public override string ToString()
        {
            if (null == mInfoCmpt)
            {
                mInfoCmpt = GetCmpt<EntityInfoCmpt>();
            }

            if (null != mInfoCmpt)
            {
                return mInfoCmpt.characterName.fullName;
            }

            return "NoNameEntity";
        }
        #endregion
    }
}