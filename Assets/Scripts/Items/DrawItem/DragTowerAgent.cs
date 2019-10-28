using System;
using Pathea;
using UnityEngine;

public class DragTowerAgent : DragItemAgent
{
    PeEntity mTowerEntity;

    public DragTowerAgent() { }

    public DragTowerAgent(ItemAsset.Drag drag, Vector3 pos, Vector3 scl, Quaternion rot, int id, NetworkInterface net = null)
        :base(drag, pos, scl, rot, id, net)
    {
    }

    public DragTowerAgent(ItemAsset.Drag drag, Vector3 pos, Vector3 scl, Quaternion rot)
        : this(drag, pos, scl, rot, SceneMan.InvalidID)
    {
    }

    public DragTowerAgent(ItemAsset.Drag drag, Vector3 pos, Quaternion rot)
        : this(drag, pos, Vector3.one, rot, SceneMan.InvalidID)
    {
    }

    public DragTowerAgent(ItemAsset.Drag drag, Vector3 pos)
        : this(drag, pos, Vector3.one, Quaternion.identity, SceneMan.InvalidID)
    {
    }

    void CreateEntity()
    {
        int entityId = id;
        //multi use id as entity id
        if (!GameConfig.IsMultiMode)
        {
            entityId = Pathea.WorldInfoMgr.Instance.FetchNonRecordAutoId();
        }

		if (itemDrag == null || itemDrag.itemObj == null)
			return;

        ItemAsset.Tower tower = itemDrag.itemObj.GetCmpt<ItemAsset.Tower>();
        if(tower == null)
            return;

		mTowerEntity = Pathea.PeEntityCreator.Instance.CreateTower(entityId, tower.id, position, rotation, scale);
    }

    void DestoryEntity()
	{
		if(null == mTowerEntity) return;
        Pathea.EntityMgr.Instance.Destroy(mTowerEntity.Id);
    }

    void SetData()
    {
        if (mTowerEntity == null)
        {
            return;
        }

        ItemScript script = mTowerEntity.GetGameObject().GetComponent<ItemScript>();
        if (script != null)
        {
            script.InitNetlayer(network);
            script.SetItemObject(itemDrag.itemObj);
            script.id = id;
        }
    }

    public override int id
    {
        get
        {
            return base.id;
        }

        protected set
        {
            base.id = value;			
			if(null == mTowerEntity) return;
            ItemScript script = mTowerEntity.GetGameObject().GetComponent<ItemScript>();

            if (script != null)
            {
                script.id = id;
            }
        }
    }

    public override void Create()
    {
        CreateEntity();
        SetData();
    }

    protected override void Destroy()
	{
		if(itemDrag!=null&&itemDrag.itemObj!=null){
			PeMap.TowerMark findMask = PeMap.TowerMark.Mgr.Instance.Find(tower => itemDrag.itemObj.instanceId == tower.ID);
			if(null != findMask)
			{
				PeMap.LabelMgr.Instance.Remove(findMask);
				PeMap.TowerMark.Mgr.Instance.Remove(findMask);
			}
		}
        DestoryEntity();
    }

	public override GameObject gameObject
	{
		get
		{
			if (mTowerEntity == null) return null;
			return mTowerEntity.GetGameObject();
		}
	}

    protected override void Serialize(System.IO.BinaryWriter bw)
    {
        base.Serialize(bw);
		if (mTowerEntity != null) {
			PETools.Serialize.WriteData(mTowerEntity.Export, bw);
		} else {
			PETools.Serialize.WriteData(null, bw);
		}
    }

    protected override void Deserialize(System.IO.BinaryReader br)
    {
        base.Deserialize(br);

        byte[] buff = PETools.Serialize.ReadBytes(br);
        if (mTowerEntity != null
            && buff != null
            )
        {
            mTowerEntity.Import(buff);
        }
    }

    protected override void OnActivate()
    {
		base.OnActivate();
		if(null == mTowerEntity) return;
        DragEntityLodCmpt lod = mTowerEntity.GetComponent<DragEntityLodCmpt>();
        if (lod != null)
        {
            lod.Activate();
        }
    }

    protected override void OnDestruct()
	{
		if(null == mTowerEntity) return;
        DragEntityLodCmpt lod = mTowerEntity.GetComponent<DragEntityLodCmpt>();
        if (lod != null)
        {
            lod.Deactivate();
        }
    }
}
