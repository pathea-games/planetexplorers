using UnityEngine;
using System.IO;
using System.Collections.Generic;
using Pathea;
using ItemAsset;

public class LootItemData
{
	public int 		id;
	public Vector3 	position;
	public double 	dropTime;

	public float	checkItemExistTime;

	ItemObject m_ItemObj;
	public ItemObject itemObj
	{
		get
		{
			if(null == m_ItemObj)
				m_ItemObj = ItemMgr.Instance.Get (id);
			return m_ItemObj;
		}
	}

	public byte[] Export()
	{
		using (MemoryStream ms = new MemoryStream())
		{
			using (BinaryWriter w = new BinaryWriter(ms))
			{
				Export(w);
				return ms.ToArray();
			}
		}
	}

	public void Import(byte[] data)
	{
		MemoryStream ms = new MemoryStream(data);
		BinaryReader _in = new BinaryReader(ms);
		Import (_in);
		_in.Close ();
		ms.Close ();
	}

	public void Export(BinaryWriter w)
	{
		w.Write(id);
		w.Write(position.x);
		w.Write(position.y);
		w.Write(position.z);
		w.Write(dropTime);
	}

	public void Import(BinaryReader _in)
	{
		id = _in.ReadInt32 ();
		position = new Vector3 (_in.ReadSingle(), _in.ReadSingle(), _in.ReadSingle());
		dropTime = _in.ReadDouble ();
	}
}

public class LootItemMgr : ArchivableSingleton<LootItemMgr>
{
	static readonly int 	CURRENT_VERSION = 2;
	static readonly float 	RemoveTime = 93600f;
	static readonly float 	GenerateSqrDis = 48f*48f;
	//static readonly float 	DestroySqrDis = 64f*64f;
	static readonly int 	UpdateNumPerFrame = 30;
	static readonly float	SqrFetchRange = 4f * 4f;

	int m_NextID;
	int m_UpdateGenerateIndex;
	int m_UpdateDestroyIndex;

	Dictionary<int, LootItemData> m_Datas = new Dictionary<int, LootItemData> ();
	List<LootItemData> m_NotGeneratedItems = new List<LootItemData>();
	List<MousePickableLootItem> m_SceneItems = new List<MousePickableLootItem> ();
	List<MousePickableLootItem> m_FetchItems = new List<MousePickableLootItem> ();
	List<MousePickableLootItem> m_NetFetchItems = new List<MousePickableLootItem> ();
	Stack<MousePickableLootItem> m_SceneItemStack = new Stack<MousePickableLootItem>();

	MousePickableLootItem m_Perfab;
	Transform m_Root;

	public LootItemMgr ()
	{
//		m_Perfab = Resources.Load ("Prefabs/LootItem/LootItem") as GameObject;
		m_Root = (new GameObject ("LootItemMgr")).transform;
		m_Root.position = Vector3.zero;
		m_Root.rotation = Quaternion.identity;
		m_Root.localScale = Vector3.one;
		m_Root.gameObject.AddComponent<UIPanel>();
		GameObject gameObj = GameObject.Instantiate(Resources.Load ("Prefab/LootItem/LootItem")) as GameObject;
		m_Perfab = gameObj.GetComponent<MousePickableLootItem>();
		m_Perfab.transform.parent = m_Root;
		Recycle (m_Perfab);
	}

	#region implemented abstract members of ArchivableSingleton
	
//	protected override bool GetYird()
//	{
//		return false;
//	}

	protected override void WriteData (BinaryWriter w)
	{
		w.Write(CURRENT_VERSION);
		w.Write(m_Datas.Count);
		foreach(LootItemData data in m_Datas.Values)
			data.Export(w);
	}

	protected override void SetData (byte[] data)
	{
		MemoryStream ms = new MemoryStream(data);
		BinaryReader _in = new BinaryReader(ms);
		
		int readVersion = _in.ReadInt32();
		if(readVersion != CURRENT_VERSION)
			return;
		int count = _in.ReadInt32();
		LootItemData lootData;
		for(int i = 0; i < count; i++)
		{
			lootData = new LootItemData();
			lootData.Import(_in);
			m_Datas[lootData.id] = lootData;
			if(null != lootData.itemObj)
				m_NotGeneratedItems.Add(lootData);
		}
		_in.Close();
		ms.Close();
	}
	#endregion

	public void RequestCreateLootItem(PeEntity entity)
	{
		if (null == entity)	return;		
		if(PeGameMgr.IsMulti)	return;

		CommonCmpt common = entity.commonCmpt;
		if (common != null)
		{
			List<ItemSample> items = ItemDropData.GetDropItems(common.ItemDropId);
			if (common.entityProto.proto == EEntityProto.Monster) 
			{
				if (items == null)
					items = GetSpecialItem.MonsterItemAdd(common.entityProto.protoId);
				else
					items.AddRange(GetSpecialItem.MonsterItemAdd(common.entityProto.protoId));   //特殊道具添加
			}
			if(items != null)
			{
				for(int i = 0; i < items.Count; ++i)
					AddLootItem(entity.position, items[i].protoId, items[i].stackCount);
			}
		}
	}

	public void AddLootItem(Vector3 pos, int instanceID)
	{
		LootItemData data = new LootItemData ();
		data.id = instanceID;
		data.position = pos;
		data.dropTime = GameTime.Timer.Second;
		AddLootItem (data, true);
	}

	public void AddLootItem(Vector3 pos, int itemProtoID, int num)
	{
		if(num < 1)
			return;
		ItemProto proto = ItemProto.Mgr.Instance.Get(itemProtoID);
		int itemCount = (num - 1)/proto.maxStackNum + 1;
		for(int i = 0; i < itemCount; ++i, num -= proto.maxStackNum)
		{
			ItemObject itemObj = ItemMgr.Instance.CreateItem (itemProtoID);
			if(num >= proto.maxStackNum)
				itemObj.SetStackCount (proto.maxStackNum);
			else
				itemObj.SetStackCount (num);
			LootItemData data = new LootItemData ();
			data.id = itemObj.instanceId;
			data.position = pos;
			data.dropTime = GameTime.Timer.Second;
			AddLootItem (data, true);
		}
	}
	
	void AddLootItem(LootItemData data, bool generateImmediately = false)
	{
		m_Datas [data.id] = data;
		if (generateImmediately)
			CreatSceneItem (data, true);
		else
			m_NotGeneratedItems.Add (data);
	}

	public void NetAddLootItem(Vector3 pos, int instanceId)
	{
		LootItemData data = new LootItemData ();
		data.id = instanceId;
		data.position = pos;
		data.dropTime = GameTime.Timer.Second;
		AddLootItem (data, true);
	}

	public void NetRemoveLootItem (int id)
	{
		RemoveLootItem (id);
	}

	public void RequestFetch(int id)
	{
		if (PeGameMgr.IsMulti)
			NetFetchRequest (id);
		else
			Fetch(id);
	}

	void NetFetchRequest(int id)
	{	
		PlayerNetwork.mainPlayer.RequestGetLootItemBack(id,false);
		for (int i = 0; i < m_SceneItems.Count; ++i)
		{
			if(id == m_SceneItems[i].data.id)
			{
				m_NetFetchItems.Add(m_SceneItems[i]);
				m_SceneItems.RemoveAt(i);
			}
		}
	}

	public void NetFetch(int lootItemid, int entityID)
	{
		PeEntity entity = EntityMgr.Instance.Get (entityID);
		if (null != entity && null != entity.centerBone)
			LootToTarget (lootItemid, entity.centerBone);
		else
			RemoveLootItem (lootItemid);
	}

	void Fetch(int lootItemid)
	{
		if (AddItemToPlayer (lootItemid)) 
			LootToTarget (lootItemid, MainPlayer.Instance.entity.centerBone);
	}

	void LootToTarget(int id, Transform targetTrans)
	{
		if (null == targetTrans) 
		{
			Debug.LogError ("LootItem can't find centerBone.");
			return;
		}

		if (m_Datas.ContainsKey (id))
			m_Datas.Remove (id);

		for(int i = 0; i < m_NotGeneratedItems.Count; ++i)
		{
			if(m_NotGeneratedItems[i].id == id)
			{
				m_NotGeneratedItems.RemoveAt(i);
				return;
			}
		}

		for (int i = 0; i < m_SceneItems.Count; ++i)
		{
			if(id == m_SceneItems[i].data.id)
			{
				m_FetchItems.Add(m_SceneItems[i]);
				m_SceneItems[i].SetMoveState(MousePickableLootItem.MoveState.Loot, OnFetchEnd, targetTrans);
				m_SceneItems.RemoveAt(i);
			}
		}

		for (int i = 0; i < m_NetFetchItems.Count; ++i) 
		{
			if(m_NetFetchItems[i].data.id == id)
			{
				m_FetchItems.Add(m_NetFetchItems[i]);
				m_NetFetchItems[i].SetMoveState(MousePickableLootItem.MoveState.Loot, OnFetchEnd, targetTrans);
				m_NetFetchItems.RemoveAt(i);
				break;
			}
		}
	}

	bool AddItemToPlayer(int id)
	{
		if(m_Datas.ContainsKey(id)
		   && null != MainPlayer.Instance.entity
		   && null != MainPlayer.Instance.entity.packageCmpt)
		{
			PlayerPackageCmpt playerPackage = MainPlayer.Instance.entity.packageCmpt as PlayerPackageCmpt;
			if(null != playerPackage && null != m_Datas[id].itemObj)
			{
				if(m_Datas[id].itemObj.protoData.maxStackNum > 1)
				{
					if(playerPackage.Add(m_Datas[id].itemObj.protoId, m_Datas[id].itemObj.stackCount))
					{
						ItemMgr.Instance.DestroyItem(m_Datas[id].itemObj.instanceId);
						return true;
					}
				}
				else
				{
					if(playerPackage.Add(m_Datas[id].itemObj, true))
						return true;
				}
			}
		}

		return false;
	}

	public void RemoveLootItem(int id)
	{
		if (m_Datas.ContainsKey (id))
			m_Datas.Remove (id);

		for(int i = 0; i < m_NotGeneratedItems.Count; ++i)
		{
			if(m_NotGeneratedItems[i].id == id)
			{
				m_NotGeneratedItems.RemoveAt(i);
				break;
			}
		}

		for (int i = 0; i < m_SceneItems.Count; ++i) 
		{
			if(m_SceneItems[i].data.id == id)
			{
				Recycle(m_SceneItems[i]);
				m_SceneItems.RemoveAt(i);
				break;
			}
		}

		for (int i = 0; i < m_NetFetchItems.Count; ++i) 
		{
			if(m_NetFetchItems[i].data.id == id)
			{
				Recycle(m_NetFetchItems[i]);
				m_NetFetchItems.RemoveAt(i);
				break;
			}
		}
	}

	public override void Update ()
	{
		base.Update ();
		if (null == MainPlayer.Instance.entity)
			return;

		Vector3 mainPlayerPos = MainPlayer.Instance.entity.position;
		int updateMaxIndex = Mathf.Min (m_UpdateGenerateIndex, m_NotGeneratedItems.Count - 1);
		int updateMinIndex = Mathf.Max (m_UpdateGenerateIndex - UpdateNumPerFrame, 0);
		m_UpdateGenerateIndex = (updateMinIndex == 0)? (m_NotGeneratedItems.Count - 1):updateMinIndex;
		for(int i = updateMaxIndex; i >= updateMinIndex; --i)
		{
			if(CheckItemRemove(m_NotGeneratedItems[i]))
			{
				if(PeGameMgr.IsMulti)
				{
					PlayerNetwork.mainPlayer.RequestGetLootItemBack(m_NotGeneratedItems[i].id,true);
				}
				else
				{
					ItemMgr.Instance.DestroyItem(m_NotGeneratedItems[i].id);
					m_Datas.Remove(m_NotGeneratedItems[i].id);
					m_NotGeneratedItems.RemoveAt(i);
				}
				continue;
			}

			if(Vector3.SqrMagnitude(mainPlayerPos - m_NotGeneratedItems[i].position) < GenerateSqrDis)
			{
				CreatSceneItem(m_NotGeneratedItems[i], true);
				m_NotGeneratedItems.RemoveAt(i);
			}
		}
		
		updateMaxIndex = Mathf.Min (m_UpdateDestroyIndex, m_SceneItems.Count - 1);
		updateMinIndex = Mathf.Max (m_UpdateDestroyIndex - UpdateNumPerFrame, 0);
		m_UpdateDestroyIndex = (updateMinIndex == 0)? (m_SceneItems.Count - 1):updateMinIndex;
        for(int i = updateMaxIndex; i >= updateMinIndex; --i)
		{
            //lz-2017.07.31 错误 #11342 Crash bug
            if (i < 0 || i >= m_SceneItems.Count)
            {
                continue;
            }

            if (null == m_SceneItems[i])
			{
				m_SceneItems.RemoveAt(i);
				continue;
			}

			if(CheckItemRemove(m_SceneItems[i].data))
			{
				if(PeGameMgr.IsMulti)
				{
					PlayerNetwork.mainPlayer.RequestGetLootItemBack(m_SceneItems[i].data.id,true);
				}
				else
				{
					ItemMgr.Instance.DestroyItem(m_SceneItems[i].data.id);
					m_Datas.Remove(m_SceneItems[i].data.id);
					Recycle(m_SceneItems[i]);
					m_SceneItems.RemoveAt(i);
				}
				continue;
			}
			float sqrDis = Vector3.SqrMagnitude(mainPlayerPos - m_SceneItems[i].transform.position);
			if(sqrDis < SqrFetchRange && !MainPlayer.Instance.entity.IsDeath())
			{
				RequestFetch(m_SceneItems[i].data.id);
				continue;
			}

			if(sqrDis > GenerateSqrDis)
			{
				Recycle(m_SceneItems[i]);
				m_NotGeneratedItems.Add(m_SceneItems[i].data);
				m_SceneItems.RemoveAt(i);
			}
		}
	}

	void ClearData()
	{
		m_Datas.Clear();
		m_NotGeneratedItems.Clear();
		m_SceneItems.Clear();
	}

	Vector3 GetLootPos(Vector3 pos)
	{
		return pos + new Vector3(Random.Range(-1f, 1f), Random.Range(0f, 1f), Random.Range(-1f, 1f));
	}

	MousePickableLootItem GetSceneLootItem()
	{
		MousePickableLootItem item;
		if (m_SceneItemStack.Count > 0) 
		{
			item = m_SceneItemStack.Pop ();
			if(null != item)
				return item;
		}
		item = GameObject.Instantiate (m_Perfab);
		item.transform.parent = m_Root;
		return item;
	}

	void Recycle(MousePickableLootItem item)
	{
		item.gameObject.SetActive (false);
		m_SceneItemStack.Push (item);
	}

	void CreatSceneItem(LootItemData data, bool move = false)
	{
		MousePickableLootItem item = GetSceneLootItem ();
		item.gameObject.SetActive (true);
		item.SetData (data);
		item.SetMoveState (move ? MousePickableLootItem.MoveState.Drop : MousePickableLootItem.MoveState.Stay);
		m_SceneItems.Add (item);
	}

	void OnFetchEnd(int id)
	{
		RemoveLootItem (id);
		for (int i = 0; i < m_FetchItems.Count; ++i) 
		{
			if(m_FetchItems[i].data.id == id)
			{
				Recycle(m_FetchItems[i]);
				m_FetchItems.RemoveAt(i);
				return;
			}
		}
	}

	bool CheckItemRemove(LootItemData data)
	{
		ItemObject itemobj = data.itemObj;

		if(Time.time > data.checkItemExistTime)
		{
			if(null == ItemMgr.Instance.Get(data.id))
				return true;
			else
				data.checkItemExistTime = Time.time + Random.Range(5f, 10f);
		}

		if (null != data.itemObj && itemobj.protoData.category != "Quest Item") 
			return GameTime.Timer.Second - data.dropTime > RemoveTime;
		return false;
	}
}
