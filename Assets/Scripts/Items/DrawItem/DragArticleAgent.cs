using UnityEngine;

public class DragArticleAgent : DragItemAgent
{
    public DragItemLogic itemLogic;

    public ItemScript itemScript;

    SceneObjAdditionalSaveData _additionalData = new SceneObjAdditionalSaveData();

    public DragArticleAgent() { }

    public DragArticleAgent(ItemAsset.Drag drag, Vector3 pos, Vector3 scl, Quaternion rot, int id, NetworkInterface net = null)
        :base(drag, pos, scl, rot, id, net)
    {
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

            if (itemLogic != null)
            {
                itemLogic.id = value;
            }
        }
    }

    public override void Create()
    {
        TryLoadLogicGo();
        SetDataToLogic();
    }

    protected override void Destroy()
    {
        DestoryGameObject();
    }

	public override GameObject gameObject
	{
		get
		{
			if (itemLogic != null)
			{
				return itemLogic.gameObject;
			}

			if (itemScript != null)
			{
				return itemScript.gameObject;
			}

			return null;
		}
	}

	static Transform sRoot;
    public static Transform Root
    {
        get
        {
            if (sRoot == null)
            {
                sRoot = new GameObject("ArticleRoot").transform;
            }
            return sRoot;
        }
    }

    private void TryLoadLogicGo()
    {
        if (null == itemDrag)
        {
            return;
        }

        GameObject go = itemDrag.CreateLogicGameObject(InitTransform);
        if (go != null)
        {
            if (SceneMan.self != null)
            {
				go.transform.SetParent(Root, true);
			}

            itemLogic = go.GetComponent<DragItemLogic>();
        }
    }

    void SetDataToLogic()
    {
        if (itemLogic == null)
        {
            return;
        }

		InitTransform(itemLogic.transform);
        itemLogic.SetItemDrag(itemDrag);
        itemLogic.InitNetlayer(network);
        if (MissionManager.Instance != null)
        {
            if (MissionManager.Instance.m_PlayerMission.isRecordCreation && itemLogic.itemDrag.protoData.itemClassId == (int)WhiteCat.CreationItemClass.Aircraft
                && MissionManager.Instance.m_PlayerMission.recordCreationName.Count < 4)
                MissionManager.Instance.m_PlayerMission.recordCreationName.Add(itemLogic.transform.gameObject.name);
        }

        itemLogic.id = id;
    }

    void DestoryGameObject()
    {
        if (itemLogic != null)
        {
            Object.Destroy(itemLogic.gameObject);
        }

        if (itemScript != null)
        {
            Object.Destroy(itemScript.gameObject);
        }
    }

    public void TryForceCreateGO ()
    {
        if (itemScript == null)
        {
            TryLoadViewGo();
            if (null != itemScript)
            {
                itemScript.OnConstruct();
            }
        }
    }

    private void TryLoadViewGo()
    {
        if (null == itemDrag)
        {
            return;
        }

        GameObject go = itemDrag.CreateViewGameObject(InitTransform);

        if (go == null)
        {
            return;
        }

		InitTransform(go.transform);
		go.transform.SetParent(Root, true);

		itemScript = go.GetComponent<ItemScript>();
    }

    protected override void OnActivate()
    {
        base.OnActivate();
        if (itemLogic != null)
        {
            itemLogic.OnActivate();
        }
        else
        {
            if (null != itemScript)
            {
                itemScript.InitNetlayer(network);
                itemScript.SetItemObject(itemDrag.itemObj);
                itemScript.id = id;

                itemScript.OnActivate();
            }
        }
    }

    protected override void OnDeactivate()
    {
        base.OnDeactivate();

        if (itemLogic != null)
        {
            itemLogic.OnDeactivate();
        }
        else
        {
            if (null != itemScript)
            {
                itemScript.OnDeactivate();
            }
        }
    }

    protected override void OnConstruct()
    {
        if (itemLogic != null)
        {
            itemLogic.OnConstruct();
        }
        else
        {
            if (itemScript == null)
            {
                TryLoadViewGo();
                if (null != itemScript)
                {
                    itemScript.OnConstruct();
                }
            }
        }
    }

    protected override void OnDestruct()
    {
        if (itemLogic != null)
        {
            itemLogic.OnDestruct();
        }
        else
        {
            if (null != itemScript)
            {
                itemScript.OnDestruct();
            }

            DestoryGameObject();
        }
    }

    protected override void Deserialize(System.IO.BinaryReader br)
    {
        base.Deserialize(br);

        _additionalData.Deserialize(br);
        _additionalData.DispatchData(gameObject);
    }

    protected override void Serialize(System.IO.BinaryWriter bw)
    {
        base.Serialize(bw);

        _additionalData.CollectData(gameObject);
        _additionalData.Serialize(bw);
    }

    public static DragArticleAgent PutItemByProroId(int protoId, Vector3 pos, Quaternion rot)
    {
        return PutItemByProroId(protoId, pos, Vector3.one, rot);
    }

    public static ItemAsset.Drag CreateItemDrag(int protoId)
    {
        ItemAsset.ItemObject itemObj = ItemAsset.ItemMgr.Instance.CreateItem(protoId);
        if (null == itemObj)
        {
            Debug.LogError("create item failed, protoId:" + protoId);
            return null;
        }

        return itemObj.GetCmpt<ItemAsset.Drag>();        
    }

    public static DragArticleAgent PutItemByProroId(int protoId, Vector3 pos, Vector3 scl, Quaternion rot, bool pickable = true, bool attackable = false)
    {
        ItemAsset.Drag drag = CreateItemDrag(protoId);

        if (null == drag)
        {
            Debug.LogError("item has no drag, protoId:" + protoId);
            return null;
        }

        DragArticleAgent item = DragArticleAgent.Create(drag, pos, scl, rot);

        return item;
    }

    public static bool Destory(int id)
    {
        DragItemAgent item = GetById(id);

        return Destory(item);
    }

    public static DragArticleAgent Create(ItemAsset.Drag drag, Vector3 pos, Vector3 scl, Quaternion rot, int id = SceneMan.InvalidID, NetworkInterface net = null, bool isCreation = false)
    {
		DragArticleAgent agent;
		if (isCreation) agent = new DragCreationAgent(drag, pos, scl, rot, id, net);
		else agent = new DragArticleAgent(drag, pos, scl, rot, id, net);

        agent.Create();
        SceneMan.AddSceneObj(agent);
        return agent;
    }

    public static DragArticleAgent Create(ItemAsset.Drag drag, Vector3 pos, Quaternion rot)        
    {
        return Create(drag, pos, Vector3.one, rot, SceneMan.InvalidID);
    }

    public static DragArticleAgent Create(ItemAsset.Drag drag, Vector3 pos)
    {
        return Create(drag, pos, Vector3.one, Quaternion.identity);
    }
}
