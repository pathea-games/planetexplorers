using ItemAsset;
using UnityEngine;
using System.Collections.Generic;

public abstract class DragItemAgent : ISceneSerializableObjAgent
{
    protected int mId;

	// ��ȡ�浵�򴴽�����ʱ�Ļ���״̬
    Vector3 _pos = Vector3.zero;
    Vector3 _sca = Vector3.one;
    Quaternion _rot = Quaternion.identity;

    public ItemAsset.Drag itemDrag;

    public NetworkInterface network;

	public Vector3 position
	{
		get { return transform ? (_pos = transform.position) : _pos; }
		set { _pos = value; if (transform) transform.position = value; }
	}

	public Quaternion rotation
	{
		get { return transform ? (_rot = transform.rotation) : _rot; }
		set { _rot = value; if (transform) transform.rotation = value; }
	}

	public Vector3 scale
	{
		get { return transform ? (_sca = transform.localScale) : _sca; }
		set { _sca = value; if (transform) transform.localScale = value; }
	}

    Pathea.PeTrans mPeTrans;
    public Pathea.PeTrans peTrans { get { return mPeTrans; } }
	protected void InitTransform(Transform t)
	{
		t.position = _pos;
		t.rotation = _rot;
		t.localScale = _sca;

        mPeTrans = t.GetComponent<Pathea.PeTrans>();
	}

	public DragItemAgent() { }

    public DragItemAgent(ItemAsset.Drag drag, Vector3 pos, Vector3 scl, Quaternion rot, int id, NetworkInterface net = null)
    {
        mId = id;
		_pos = pos;
		_sca = scl;
		_rot = rot;

        network = net;

        itemDrag = drag;
    }

    public DragItemAgent(ItemAsset.Drag drag, Vector3 pos, Vector3 scl, Quaternion rot)
        : this(drag, pos, scl, rot, SceneMan.InvalidID)
    {
    }

    public DragItemAgent(ItemAsset.Drag drag, Vector3 pos, Quaternion rot)
        : this(drag, pos, Vector3.one, rot, SceneMan.InvalidID)
    {
    }

    public DragItemAgent(ItemAsset.Drag drag, Vector3 pos)
        : this(drag, pos, Vector3.one, Quaternion.identity, SceneMan.InvalidID)
    {
    }

    public virtual void Create() { }

    protected virtual void Destroy() { }

    public virtual int id
    {
        get
        {
            return mId;
        }

        protected set
        {
            mId = value;
        }
    }
    public int ScenarioId { get; set; }

    public void Rotate(Vector3 v)
    {
		rotation *= Quaternion.Euler(v);
    }

    protected virtual void Serialize(System.IO.BinaryWriter bw)
    {
        bw.Write((int)itemDrag.itemObj.instanceId);

        bw.Write(mId);
        PETools.Serialize.WriteVector3(bw, position);
        PETools.Serialize.WriteVector3(bw, scale);
        PETools.Serialize.WriteQuaternion(bw, rotation);
    }

    protected virtual void Deserialize(System.IO.BinaryReader br)
    {
        int itemInstanceId = br.ReadInt32();

        ItemAsset.ItemObject itemObj = ItemMgr.Instance.Get(itemInstanceId);
        if (null != itemObj) {
            itemDrag = itemObj.GetCmpt<ItemAsset.Drag>();
        } else {
            Debug.LogError("[Error]Cant find item object by id:" + itemInstanceId);
            itemDrag = null;
        }

        mId = br.ReadInt32();
		_pos = PETools.Serialize.ReadVector3(br);
		_sca = PETools.Serialize.ReadVector3(br);
		_rot = PETools.Serialize.ReadQuaternion(br);

        Create();
    }

    protected virtual void OnActivate()
    {
		GameObject go = gameObject;
		if (null != go && null == go.GetComponent<PathfindingObstacle> ()) {
			go.AddComponent<PathfindingObstacle> ();
		}
    }

    protected virtual void OnDeactivate()
    {
    }

    protected virtual void OnConstruct()
    {
    }

    protected virtual void OnDestruct()
    {
    }

	protected virtual bool NeedToActivate { get { return true; } }
	protected virtual bool TstYOnActivate { get { return true; } }

    public abstract GameObject gameObject { get; }
	Transform transform
	{
		get
		{
			var go = gameObject;
			return go ? go.transform : null;
		}
	}

    #region implement ISceneObjAgent
    int ISceneObjAgent.Id
    {
        get
        {
            return id;
        }
        set
        {
            id = value;
        }
    }

    GameObject ISceneObjAgent.Go    {       get { return gameObject;	}   }
    Vector3 ISceneObjAgent.Pos	    {       get { return position; 		}   }	
	IBoundInScene ISceneObjAgent.Bound	{	get { return null;			}	}
	bool ISceneObjAgent.NeedToActivate	{	get { return NeedToActivate;}	}
	bool ISceneObjAgent.TstYOnActivate	{	get { return TstYOnActivate;}	}
	void ISceneObjAgent.OnConstruct()
    {
        OnConstruct();
    }

    void ISceneObjAgent.OnActivate()
    {
        OnActivate();
    }

    void ISceneObjAgent.OnDeactivate()
    {
        OnDeactivate();
    }

    void ISceneObjAgent.OnDestruct()
    {
        OnDestruct();
    }

    void ISerializable.Serialize(System.IO.BinaryWriter bw)
    {
        Serialize(bw);
    }

    void ISerializable.Deserialize(System.IO.BinaryReader br)
    {
        Deserialize(br);
    } 
    #endregion

    public static bool Destory(DragItemAgent item)
    {
        if (null == item)
        {
            return false;
        }

        SceneMan.RemoveSceneObj(item);

        item.Destroy();

        return true;
    }

    public static DragItemAgent GetById(int id)
    {
        return SceneMan.GetSceneObjById(id) as DragItemAgent;
    }
	public static void DestroyAllInDungeon(){
		List<ISceneObjAgent> allItem = SceneMan.FindAllDragItemInDungeon();
		for(int i=allItem.Count-1;i>=0;i--){
			DragItemAgent.Destory(allItem[i] as DragItemAgent);
		}
	}
}
