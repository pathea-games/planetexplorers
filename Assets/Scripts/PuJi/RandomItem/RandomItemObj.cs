using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Pathea;
using System.IO;



public class ItemIdCount
{
    public int protoId;
    public int count;

    public ItemIdCount()
    { }
    public ItemIdCount(int protoId, int count)
    {
        this.protoId = protoId;
        this.count = count;
    }

	public static List<ItemIdCount> ParseStringToList(string itemsStr){
		List<ItemIdCount> resultList = new List<ItemIdCount> ();
		if(itemsStr=="0")
			return resultList;
		string[] itemIdCount = itemsStr.Split(';');
		foreach(string iic in itemIdCount){
			string[] iicStr = iic.Split(',');
			int protoId = System.Convert.ToInt32(iicStr[0]);
			int count = System.Convert.ToInt32(iicStr[1]);
			resultList.Add(new ItemIdCount(protoId,count));
		}
		return resultList;
	}


    public static object Deserialize(uLink.BitStream stream, params object[] codecOptions)
    {
        try
        {
            int protoId = stream.ReadInt32();
            int count = stream.ReadInt32();
            ItemIdCount to = new ItemIdCount(protoId, count);
            return to;
        }
        catch (System.Exception e)
        {
            throw e;
        }
    }

    public static void Serialize(uLink.BitStream stream, object value, params object[] codecOptions)
    {
        try
        {
            ItemIdCount to = value as ItemIdCount;
            stream.WriteInt32(to.protoId);
            stream.WriteInt32(to.count);
        }
        catch (System.Exception e)
        {
            throw e;
        }
    }
	public override bool Equals (object obj)
	{
		ItemIdCount iic = obj as ItemIdCount;

		return protoId==iic.protoId&&count==iic.count;
	}

	// clear warnings
	public override int GetHashCode ()
	{
		return base.GetHashCode ();
	}
}

public enum RandomObjType{
    MonsterFeces,
    RandomItem,
    ProcessingItem
}
public class RandomItemObj:ISceneObjAgent
{
    public Vector3 genPos;
    public int boxId;
    public Vector3 position;
    public Quaternion rotation;
    public int id;
    public string path;
	public List<int> rareItemInstance = new List<int>();
	public List<ItemIdCount> rareItemProto = new List<ItemIdCount>();
    public int[] items;
    public GameObject gameObj;
    public bool isNew;
	public bool needToActivate=false;
	public bool tstYOnActivate=false;
    public RandomObjType type = RandomObjType.RandomItem;

	public RandomItemObj(){}

    public RandomItemObj(int boxId, Vector3 pos,Dictionary<int,int> itemIdNum,string path = "Prefab/Item/Scene/randomitem_test", int id = 0)
    {
        genPos = pos;
        this.boxId = boxId;
        position = pos;
        this.id = id;
        items = new int[itemIdNum.Keys.Count*2];
        int index = 0;
        foreach (KeyValuePair<int, int> item in itemIdNum)
        {
            items[index++] = item.Key;
            items[index++] = item.Value;
        }
        this.path = path;
        SceneMan.AddSceneObj(this);
    }
    public RandomItemObj(int boxId, Vector3 pos, int[] itemIdNum, string path, int id = 0)
    {
        genPos = pos;
        this.boxId = boxId;
        position = pos;
        rotation = Quaternion.Euler(0, (new System.Random()).Next(360), 0);
        this.id = id;
        items = itemIdNum;
        this.path = path;
        isNew = true;
        SceneMan.AddSceneObj(this);
    }

    public RandomItemObj(int boxId, Vector3 pos,Quaternion rot, int[] itemIdNum, string path, int id = 0)
    {
        genPos = pos;
        this.boxId = boxId;
        position = pos;
        rotation = Quaternion.Euler(0, (new System.Random()).Next(360), 0);
        this.id = id;
        items = itemIdNum;
        this.path = path;
        isNew = true;
        SceneMan.AddSceneObj(this);
    }

    //for processing
	public RandomItemObj(Vector3 pos, int[] itemIdNum, string path = ProcessingConst.RESULT_MODEL_PATH, int id = 0)
	{
//		int type = 1;
        while (RandomItemMgr.Instance.ContainsPos(pos))
        {
            pos += new Vector3(0, 0.01f, 0);
        }
        genPos = pos;
        position = pos;
        rotation = Quaternion.Euler(0, (new System.Random()).Next(360), 0);
        this.id = id;
        items = itemIdNum;
        this.path = path;
        isNew = true;
        TryGenObject();
    }

    //for feces
    public RandomItemObj(string name, Vector3 pos, int[] itemIdNum, Quaternion rot,string path = FecesModelPath.PATH_01, int id = 0)
    {
        genPos = pos;
		needToActivate = true;
		tstYOnActivate = true;
        boxId = -1;
        position = pos;
        rotation = rot;// Quaternion.Euler(0, (new System.Random()).Next(360), 0);
        this.id = id;
        items = itemIdNum;
        this.path = path;
        isNew = true;
        TryGenFeces();
        type = RandomObjType.MonsterFeces;
    }

	public void AddRareProto(int id,int count){
		rareItemProto.Add(new ItemIdCount (id,count));
	}

	public void AddRareInstance(int id){
		rareItemInstance.Add(id);
		if(rareItemProto.Count>0)
			rareItemProto.RemoveAt(0);
		RefreshToPick();
	}

	public void RefreshToPick(){
		if(gameObj!=null){
			MousePickableRandomItem mouse = gameObj.GetComponent<MousePickableRandomItem>();
			if(mouse!=null){
				mouse.genPos = genPos;
				//additem
				mouse.RefreshItem(items,rareItemProto,rareItemInstance);
			}
		}
	}

    public void TryGenObject()
    {
        if (PeGameMgr.IsMulti) 
        {
        }
        else
        {
            RandomItemMgr.Instance.AddItemToManager(this);
            SceneMan.AddSceneObj(this);
        }
    }
    public void TryGenFeces()
    {
        //if (PeGameMgr.IsMulti)
        //{
        //}
        //else
        //{
            RandomItemMgr.Instance.AddFeces(this);
            SceneMan.AddSceneObj(this);
        //}
    }

    //multi processing result
    public RandomItemObj(Vector3 pos, int[] itemIdNum, Quaternion rot,string path = "Prefab/RandomItems/random_box01", int id = 0)
    {
        genPos = pos;
        position = pos;
        rotation = rot;
        this.id = id;
        items = itemIdNum;
        this.path = path;
        isNew = true;
        SceneMan.AddSceneObj(this);
    }

    public int Id
    {
        get
        {
            return id;
        }
        set
        {
            id = value ;
        }
    }

    public int ScenarioId { get; set; }
    public GameObject Go    	{	get { return gameObj; 			}  	}
    public Vector3 Pos	    	{	get { return position; 			}  	}
	public IBoundInScene Bound	{	get	{ return null;				}	}
	public bool NeedToActivate	{	get { return needToActivate ; 	}	}
	public bool TstYOnActivate 	{	get { return tstYOnActivate; 	}  	}
    public void OnActivate()
    {
        Rigidbody rb = gameObj.GetComponent<Rigidbody>();
        if (rb != null)
            rb.useGravity = true;
    }

    public void OnDeactivate()
    {
        Rigidbody rb = gameObj.GetComponent<Rigidbody>();
        if (rb != null)
            rb.useGravity = false;
    }

    public void OnConstruct()
    {
		GameObject prefab = AssetsLoader.Instance.LoadPrefabImm(path) as GameObject;
        if (null != prefab)
        {
            gameObj = Object.Instantiate(prefab) as GameObject;
            gameObj.transform.position = position;
            gameObj.transform.rotation = rotation;
//            ItemDropMousePickRandomItem iDrop = gameObj.AddComponent<ItemDropMousePickRandomItem>();
//            iDrop.genPos = genPos;
			
			if(RandomItemMgr.Instance!=null&&RandomItemMgr.Instance.gameObject!=null)
				gameObj.transform.SetParent(RandomItemMgr.Instance.gameObject.transform);

			MousePickableRandomItem mouse = gameObj.AddComponent<MousePickableRandomItem>();
            mouse.genPos = genPos;

			//additem
			mouse.RefreshItem(items,rareItemProto,rareItemInstance);
//            iDrop.RemoveDroppableItemAll();
//            for (int i = 0; i < items.Count(); i += 2)
//            {
//                iDrop.AddItem(items[i], items[i + 1]);
//            }
            Rigidbody rb = gameObj.GetComponent<Rigidbody>();
            if (rb != null)
                rb.useGravity = false;
        }
        else
        {
            Debug.LogError("randomItem model not found: " + path);
        }
    }

    public void OnDestruct()
    {
        if (gameObj != null)
        {
            position = gameObj.transform.position;
            rotation = gameObj.transform.rotation;
//            ItemDropMousePickRandomItem iDrop = gameObj.GetComponent<ItemDropMousePickRandomItem>();
//            if (iDrop != null)
//            {
//                items = iDrop.GetAllItem();
//            }
            GameObject.Destroy(gameObj);
        }
//        if (items.Count() <= 0)
//        {
//            SceneMan.RemoveSceneObj(this);
//        }
    }


    #region tool
    public bool CanFetch(int index, int protoId, int count, out int removeIndex)
    {
        removeIndex = 0;
        int realIndex = index * 2;
        if (realIndex > items.Count() || items[realIndex] != protoId || items[realIndex + 1] != count)
        {
            for (int i = 0; i < items.Count(); i += 2)
            {
                if (items[i] == protoId && items[i + 1] == count)
                {
                    removeIndex = i / 2;
                    return true;
                }
            }
            return false;
        }
        else
        {
            removeIndex = index;
            return true;
        }
    }
    public bool TryFetch(int index, int protoId, int count)
    {
        int removeIndex;
        if (CanFetch(index, protoId, count, out removeIndex))
        {
            List<ItemIdCount> itemlist = GetItems();
            itemlist.RemoveAt(removeIndex);
            SaveItems(itemlist);
            if (gameObj != null)
            {
                ItemDropMousePickRandomItem iDrop = gameObj.GetComponent<ItemDropMousePickRandomItem>();
                if (iDrop != null)
                {
                    iDrop.Remove(index, protoId, count);
                }
            }
            CheckDestroyObj();
            return true;
        }
        CheckDestroyObj();
        return false;
    }

    public List<ItemIdCount> TryFetchAll()
    {
        List<ItemIdCount> itemlist = GetItems();
        SaveItems(new List<ItemIdCount>());

        if (gameObj != null)
        {
            ItemDropMousePickRandomItem iDrop = gameObj.GetComponent<ItemDropMousePickRandomItem>();
            if (iDrop != null)
            {
                iDrop.RemoveAll();
            }
        }
        CheckDestroyObj();
        return itemlist;
    }
    public List<ItemIdCount> GetItems()
    {
        List<ItemIdCount> itemlist = new List<ItemIdCount>();
        for (int i = 0; i < items.Count(); i += 2)
        {
            if (items[i + 1] > 0)
                itemlist.Add(new ItemIdCount(items[i], items[i + 1]));
        }
        return itemlist;
    }

    public void SaveItems(List<ItemIdCount> itemlist)
    {
        items = new int[itemlist.Count * 2];
        int index = 0;
        foreach (ItemIdCount item in itemlist)
        {
            items[index++] = item.protoId;
            items[index++] = item.count;
        }
    }

    public void CheckDestroyObj()
    {
        if (items.Count() <= 0)
        {
			RandomItemMgr.Instance.RemoveRandomItemObj(this);
            if (gameObj != null)
                GameObject.Destroy(gameObj);
            SceneMan.RemoveSceneObj(this);
        }
    }

    //new
    public void ClickedInMultiMode(int[] items) {
        for (int i = 0; i < items.Length; i+=2) {
            LootItemMgr.Instance.AddLootItem(genPos, items[i], items[i+1]);
        }
        DestroySelf();
    }
    public void DestroySelf()
    {
        RandomItemMgr.Instance.RemoveRandomItemObj(this);
        if (gameObj != null)
            GameObject.Destroy(gameObj);
        SceneMan.RemoveSceneObj(this);
    }
    #endregion
}