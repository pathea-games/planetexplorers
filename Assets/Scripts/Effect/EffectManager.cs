using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Mono.Data.SqliteClient;
using SkillAsset;
//using uLink;

public delegate void EffectInstantiateCompleted(EffCastData data, GameObject effect);

public class EffectInst
{
	public CoroutineEffect coroutine;
}

public class CoroutineEffect : IEnumerator
{
	public bool stop = false;
	IEnumerator enumerator;
	//MonoBehaviour behaviour;
	//Coroutine coroutine;

	public CoroutineEffect(MonoBehaviour behaviour, IEnumerator enumerator)
	{
		this.stop = false;
		//this.behaviour = behaviour;
		this.enumerator = enumerator;
		behaviour.StartCoroutine(this);
	}

	// Interface implementations----TODO : need tst stop and WaitForSeconds
	public object Current { get { return enumerator.Current; } }
	public bool MoveNext() { return !stop && enumerator.MoveNext(); }
	public void Reset() { enumerator.Reset(); }
}

public class SkCastData
{
    public int      m_id;
    public string   m_path;
    public float    m_delaytime;
    public float    m_liveTime;
    public int      m_soundid;
    public int      m_direction;
    public string   m_posStr;
    public bool     m_bind;
    public Vector3  m_Pivot;

    private static Dictionary<int, SkCastData> m_data = new Dictionary<int, SkCastData>();
    public static SkCastData GetEffCastData(int pID)
    {
        return m_data.ContainsKey(pID) ? m_data[pID] : null;
    }

    public static void LoadData()
    {
        SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("spellEffect");
        reader.Read();
        while (reader.Read())
        {
            SkCastData data = new SkCastData();
            data.m_id = System.Convert.ToInt32(reader.GetString(reader.GetOrdinal("id")));
            data.m_path = reader.GetString(reader.GetOrdinal("path"));
            data.m_delaytime = System.Convert.ToSingle(reader.GetString(reader.GetOrdinal("time_delay")));
            data.m_liveTime = System.Convert.ToSingle(reader.GetString(reader.GetOrdinal("time_live")));
            data.m_soundid = System.Convert.ToInt32(reader.GetString(reader.GetOrdinal("sound")));
            data.m_direction = System.Convert.ToInt32(reader.GetString(reader.GetOrdinal("direction")));
            data.m_posStr = reader.GetString(reader.GetOrdinal("position"));
            data.m_bind = System.Convert.ToBoolean(reader.GetInt32(reader.GetOrdinal("bind")));
            data.m_Pivot = PETools.PEUtil.ToVector3(reader.GetString(reader.GetOrdinal("bind")), ',');
            m_data.Add(data.m_id, data);
        }
    }
}

public class EffectManager : MonoBehaviour
{
    static EffectManager _Instance;

    public static EffectManager Instance
    {
        get
        {
            if (_Instance == null)
            {
                _Instance = new GameObject("EffectManager").AddComponent<EffectManager>();
            }

            return _Instance;
        }
    }

	public List<EffectInst> effectInstList;
	public List<Transform> effectList;

	void Awake()
	{
		effectInstList = new List<EffectInst>();
		effectList = new List<Transform>();
	}

	private IEnumerator InstantiateEffect(EffectInst inst, Object obj, Vector3 position, Quaternion rotation,
		EffCastData data, Transform parent = null, EffectInstantiateCompleted OnInstantiated = null)
	{
		effectInstList.Add(inst);

		if (data.m_delaytime > PETools.PEMath.Epsilon)
			yield return new WaitForSeconds(data.m_delaytime);

		GameObject effect = MonoBehaviour.Instantiate(obj, position + rotation * data.mOffsetPos, rotation) as GameObject;

		if (effect != null && OnInstantiated != null)
		{
			OnInstantiated(data, effect);
		}

        if (effect != null)
        {
            if (parent != null)
                effect.transform.parent = parent;
            else
                effect.transform.parent = transform;
        }

		if (effect != null && data.m_liveTime > PETools.PEMath.Epsilon)
        {
            yield return new WaitForSeconds(data.m_liveTime);
            Destroy(effect);
        }

		effectInstList.Remove(inst);
		yield break;
	}

	private IEnumerator InstantiateEffect(EffectInst inst, Object obj, Transform tr, EffCastData data,
		Transform parent = null, Transform target = null, EffectInstantiateCompleted OnInstantiated = null)
	{
		effectInstList.Add(inst);

		if (data.m_delaytime > PETools.PEMath.Epsilon)
			yield return new WaitForSeconds(data.m_delaytime);

		if (tr == null) yield break;

        Quaternion rot;

        if (data.m_direction == 0)
            rot = Quaternion.identity;
        else if (data.m_direction == 1)
            rot = tr.rotation;
        else
        {
            if (target != null)
                rot = Quaternion.LookRotation(target.position - tr.position);
            else
                rot = tr.rotation;
        }

        GameObject effect = MonoBehaviour.Instantiate(obj, tr.position + tr.rotation * data.mOffsetPos, rot) as GameObject;

		if (effect != null && OnInstantiated != null)
		{
			OnInstantiated(data, effect);
		}

        if (effect != null)
        {
            if (parent != null)
                effect.transform.parent = parent;
            else
                effect.transform.parent = transform;
        }

		if (effect != null && data.m_liveTime > PETools.PEMath.Epsilon)
        {
            yield return new WaitForSeconds(data.m_liveTime);
            Destroy(effect);
        }

		effectInstList.Remove(inst);
		yield break;
	}

	public void Instantiate(int id, Transform tr, Transform parent = null, Transform target = null, EffectInstantiateCompleted OnInstantiated = null)
	{
		EffCastData data = EffCastData.GetEffCastData(id);
		if (data == null) return;

		EffectInst inst = new EffectInst();
		inst.coroutine = new CoroutineEffect(this, this.InstantiateEffect(inst, Resources.Load(data.m_path), tr, data, parent, target, OnInstantiated));
	}

	public void Instantiate(int id, Vector3 position, Quaternion rotation, Transform parent = null, EffectInstantiateCompleted OnInstantiated = null)
	{
		EffCastData data = EffCastData.GetEffCastData(id);
		if (data == null) return;

		EffectInst inst = new EffectInst();
		inst.coroutine = new CoroutineEffect(this, this.InstantiateEffect(inst, Resources.Load(data.m_path), position, rotation, data, parent, OnInstantiated));
	}

    public void InstantiateEffect(int effId, Transform caster, Transform target = null, EffectInstantiateCompleted OnInstantiated = null)
	{
		EffCastData data = EffCastData.GetEffCastData(effId);
		if (data == null) return;

		Transform tr = null;
		if(string.IsNullOrEmpty(data.m_posStr) || data.m_posStr.Equals("0"))
			tr = caster;
		else
			tr = AiUtil.GetChild(caster, data.m_posStr);

		if (tr == null) return;

		Transform parent = data.m_bind ? tr : null;
		Instantiate(effId, tr, parent, target, OnInstantiated);
	}
}
