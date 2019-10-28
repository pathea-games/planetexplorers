using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Mono.Data.SqliteClient;
using SkillSystem;

namespace Pathea.Effect
{
    public interface ISkEffectEntity
    {
        SkInst Inst { set; }
    }

    public class EffectData
    {
        public int m_id;
        public string m_path;
        public float m_liveTime;
        public int m_direction;
        public string m_posStr;
        public bool m_bind;
        public bool m_Rot;
        public Vector3 m_Axis;

        private static Dictionary<int, EffectData> m_data = new Dictionary<int, EffectData>();

        public static EffectData GetEffCastData(int pID)
        {
            return m_data.ContainsKey(pID) ? m_data[pID] : null;
        }

        public static void LoadData()
        {
            SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("particle");
            while (reader.Read())
            {
                EffectData data = new EffectData();
                data.m_id = System.Convert.ToInt32(reader.GetString(reader.GetOrdinal("id")));
                data.m_path = reader.GetString(reader.GetOrdinal("path"));
                data.m_liveTime = System.Convert.ToSingle(reader.GetString(reader.GetOrdinal("live")));
                data.m_posStr = reader.GetString(reader.GetOrdinal("bone"));
                data.m_bind = System.Convert.ToBoolean(reader.GetInt32(reader.GetOrdinal("bind")));
                data.m_Rot = System.Convert.ToBoolean(reader.GetInt32(reader.GetOrdinal("rot")));
                data.m_Axis = PETools.PEUtil.ToVector3(reader.GetString(reader.GetOrdinal("axis")), ',');
                m_data.Add(data.m_id, data);
            }
        }
    }

    public class EffectBuilder : Singleton<EffectBuilder>
    {
        public class EffectRequest
        {
            public event OnEffectSpawned SpawnEvent;

            internal int m_ID;
            internal object m_Data;
            internal EffectData m_EffectData;

            public EffectRequest(int argid, object argData)
            {
                m_ID = argid;
                m_Data = argData;
                m_EffectData = EffectData.GetEffCastData(m_ID);
            }

            protected void OnSpawned(GameObject obj)
            {
                if (SpawnEvent != null)
                {
                    SpawnEvent(obj);
                }
            }

            public virtual bool IsValid() { return false; }
            public virtual GameObject Create() { return null; }
        }

        public class EffectRequestTransform : EffectRequest
        {
            public Transform _bone;
            public Transform _rootTrans;

            public EffectRequestTransform(int argid, object argData, Transform argTrans) : base(argid, argData)
            {
				_rootTrans = argTrans;

                if(m_EffectData != null)
                {
                    if (m_EffectData.m_posStr == "0" || m_EffectData.m_posStr == "CenterPos")
                        _bone = _rootTrans;
                    else
                        _bone = PETools.PEUtil.GetChild(_rootTrans, m_EffectData.m_posStr);
                }
            }

            public override bool IsValid()
            {
                return m_EffectData != null && _bone != null;
            }

            public override GameObject Create()
            {
                if (m_EffectData == null || _bone == null)
                    return null;

                //GameObject proto = AssetsLoader.Instance.LoadPrefabImm(data.m_path) as GameObject;
                GameObject proto = Instance.GetEffect(m_EffectData.m_path);
                if (proto != null)
                {
                    //StartCoroutine(InstantiateCoroutine(proto, bone, data.m_Axis, data.m_liveTime, data.m_bind));
                    Quaternion rot = Quaternion.identity;
                    if (m_EffectData.m_Rot)
                        rot = _bone.rotation;

                    GameObject effect = Instantiate(proto, _bone.position, rot) as GameObject;
                    //effect.AddComponent<DestroyTimer>().m_LifeTime = data.m_liveTime;
                    if(m_EffectData.m_liveTime > -PETools.PEMath.Epsilon)
                        GameObject.DestroyObject(effect, m_EffectData.m_liveTime);

                    if (m_EffectData.m_bind)
                    {
						EffectLateupdateHelper lateU    = effect.GetComponent<EffectLateupdateHelper>();
                        if (null != lateU)
                        {
                            lateU.Init(_bone);

                            if(m_EffectData.m_posStr == "CenterPos")
                            {
                                PeEntity entity = _rootTrans.GetComponentInParent<PeEntity>();
                                if(entity != null)
                                {
                                    lateU.local = entity.bounds.center;

                                    HitEffectScale scaleEffect = effect.GetComponent<HitEffectScale>();
                                    if (scaleEffect != null)
                                    {
                                        scaleEffect.Scale = entity.maxRadius * 2.0f;

                                        if (entity.peTrans != null)
                                            scaleEffect.EmissionScale = entity.peTrans.bound.size;
                                    }
                                }

                            }
                        }
                    }

                    if (m_Data != null)
                    {
						Profiler.BeginSample("GetMono");
                        MonoBehaviour[] behaviours = effect.GetComponentsInChildren<MonoBehaviour>();
						Profiler.EndSample();

                        foreach (MonoBehaviour behaviour in behaviours)
                        {
                            SkInst inst = m_Data as SkInst;
                            ISkEffectEntity skEffect = behaviour as ISkEffectEntity;
                            if (inst != null && skEffect != null && !skEffect.Equals(null))
                            {
                                skEffect.Inst = inst;
                            }
                        }
                    }

                    OnSpawned(effect);
                    return effect;
                }

                return null;
            }
        }

        public class EffectRequestWorldPos : EffectRequest
        {
            Vector3 position;
            Quaternion rotation;
			Transform parent;

            public EffectRequestWorldPos(int argid, object argData, Vector3 worldPos, Quaternion worldRot, Transform parentTrans)
                : base(argid, argData)
            {
                position = worldPos;
                rotation = worldRot;
				parent = parentTrans;
            }

            public override bool IsValid()
            {
                return m_EffectData != null;
            }

            public override GameObject Create()
            {
                if (m_EffectData == null)
                    return null;

				//GameObject proto = AssetsLoader.Instance.LoadPrefabImm(data.m_path) as GameObject;
                GameObject proto = Instance.GetEffect(m_EffectData.m_path);
                if (proto != null)
                {
                    GameObject effect = Instantiate(proto, position, rotation) as GameObject;
                    //effect.AddComponent<DestroyTimer>().m_LifeTime = data.m_liveTime;
                    if(m_EffectData.m_liveTime > -PETools.PEMath.Epsilon)
                        GameObject.DestroyObject(effect, m_EffectData.m_liveTime);

                    if (m_Data != null)
                    {
                        MonoBehaviour[] behaviours = effect.GetComponentsInChildren<MonoBehaviour>();

                        foreach (MonoBehaviour behaviour in behaviours)
                        {
                            SkInst inst = m_Data as SkInst;
                            ISkEffectEntity skEffect = behaviour as ISkEffectEntity;
                            if (inst != null && skEffect != null && !skEffect.Equals(null))
                            {
                                skEffect.Inst = inst;
                            }
                        }
                    }

					if(null != parent)
						effect.transform.parent = parent;

                    OnSpawned(effect);
                    return effect;
                }

                return null;
            }
        }

        static Dictionary<string, GameObject> s_EffectPools = new Dictionary<string, GameObject>();

        public delegate void OnEffectSpawned(GameObject obj);

        List<EffectRequest> m_ReqList = new List<EffectRequest>();

        public GameObject GetEffect(string path)
        {
            GameObject obj = null;
            if (s_EffectPools.TryGetValue(path, out obj))
                return obj;
            else
            {
                Instance.StartCoroutine(LoadEffect(path));
                return null;
            }
        }

        IEnumerator LoadEffect(string path)
        {
            s_EffectPools[path] = null;
            ResourceRequest rr = Resources.LoadAsync<GameObject>(path);

            while (true)
            {
                if (rr.isDone)
                {
                    s_EffectPools[path] = rr.asset as GameObject;
                    yield break;
                }

                yield return null;
            }
        }

        void LateUpdate()
        {
            for (int i = m_ReqList.Count - 1; i >= 0; i--)
            {
                if (!m_ReqList[i].IsValid())
                    m_ReqList.RemoveAt(i);
                else
                {
                    GameObject obj = m_ReqList[i].Create();
                    if (obj != null)
                    {
                        if (obj.transform.parent == null)
                            obj.transform.parent = transform;

                        m_ReqList.RemoveAt(i);
                    }
                }
            }
        }

        public EffectRequest Register(int id, object data, Transform caster)
        {
            EffectRequest request = new EffectRequestTransform(id, data, caster);
            m_ReqList.Add(request);
            return request;
        }

        public EffectRequest Register(int id, object data, Vector3 position, Quaternion rotation, Transform parent = null)
        {
			EffectRequest request = new EffectRequestWorldPos(id, data, position, rotation, parent);
            m_ReqList.Add(request);
            return request;
        }

        public void RegisterEffectFromSkill(int id, SkRuntimeInfo info, Transform caster)
        {
            EffectData data = EffectData.GetEffCastData(id);
            if (data != null)
            {
				SkInst inst = info as SkInst;
				if(inst != null && data.m_posStr == "0")
				    Register(id, info, (inst._colInfo != null ? inst._colInfo.hitPos : caster.position), Quaternion.identity);
				else
					Register(id, info, caster);
            }
        }
    }
}

