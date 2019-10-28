using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class ItemBoxMgr:Pathea.ISerializable
{
    const string ArchiveKey = "ArchiveKeyItemBox";

    static ItemBoxMgr mInstance;

    public static ItemBoxMgr Instance
    {
        get
        {
            if (mInstance == null)
            {
                mInstance = new ItemBoxMgr();
            }

            return mInstance;
        }
    }

    ItemBoxMgr()
    {
    }

    const int Version = 2;

    int LastItemID = 0;

    Dictionary<int, ItemBox> mItemBox;
    Transform transform;

    void Init()
    {
        mItemBox = new Dictionary<int, ItemBox>();
        transform = new GameObject("ItemBox").transform;
        Pathea.ArchiveMgr.Instance.Register(ArchiveKey, this);
    }

    ItemBox CreateItemBox()
    {
        GameObject obj = Object.Instantiate(Resources.Load("Prefab/Other/ItemBox")) as GameObject;
		return obj != null ? obj.GetComponent<ItemBox>()  : null ;
    }

	public List<int> mDropReq { get {return GameUI.Instance.mItemPackageCtrl.mDropItemWnd.DropReqList;} }

    public ItemBox AddItemMultiPlay(int id, int opID, Vector3 pos, MapObjNetwork netWork = null)
    {
        ItemBox newBox = CreateItemBox();
        newBox.transform.parent = transform;
        newBox.mID = id;
        newBox.mPos = pos;
        newBox.mNetWork = netWork;
        mItemBox[newBox.mID] = newBox;
		
        if (null != Pathea.PeCreature.Instance.mainPlayer)
		{
			Pathea.SkAliveEntity sk = GameUI.Instance.mMainPlayer.GetCmpt<Pathea.SkAliveEntity>();
			if (sk != null)
			{
				if(opID == sk.GetId())
				{
					newBox.InsertItem(mDropReq);
					mDropReq.Clear();
				}
			}
		}
        return newBox;
    }

    public void RemoveItemMultiPlay(int id)
    {
        if (mItemBox.ContainsKey(id))
        {
            Object.Destroy(mItemBox[id].gameObject);
            mItemBox.Remove(id);
        }
    }

    public ItemBox AddItemSinglePlay(Vector3 pos)
    {
        ItemBox newBox = CreateItemBox();
        newBox.transform.parent = transform;
        newBox.mID = ++LastItemID;
        newBox.mPos = pos;
        mItemBox[newBox.mID] = newBox;
        return newBox;
    }

    public void Remove(ItemBox box)
    {
        if (mItemBox.Remove(box.mID))
            GameObject.Destroy(box.gameObject);
    }

    byte[] Export()
    {
        MemoryStream ms = new MemoryStream();
        BinaryWriter _out = new BinaryWriter(ms);
        _out.Write(Version);
        _out.Write(LastItemID);
        _out.Write(mItemBox.Count);
        foreach (int id in mItemBox.Keys)
        {
            _out.Write(mItemBox[id].mID);
            _out.Write(mItemBox[id].mPos.x);
            _out.Write(mItemBox[id].mPos.y);
            _out.Write(mItemBox[id].mPos.z);
            _out.Write(mItemBox[id].ItemList.Count);
            for (int i = 0; i < mItemBox[id].ItemList.Count; i++)
                _out.Write(mItemBox[id].ItemList[i]);
        }
        _out.Close();
        ms.Close();
        byte[] retval = ms.ToArray();
        return retval;
    }

    void Import(byte[] buffer)
    {
        mItemBox.Clear();
        MemoryStream ms = new MemoryStream(buffer);
        BinaryReader _in = new BinaryReader(ms);
        int saveVersion = _in.ReadInt32();
        LastItemID = _in.ReadInt32();
        int count = _in.ReadInt32();
        switch (saveVersion)
        {
            case 1:
                for (int i = 0; i < count; i++)
                {
                    ItemBox newBox = CreateItemBox();
                    newBox.transform.parent = transform;
                    newBox.mID = _in.ReadInt32();
                    newBox.mPos = new Vector3(_in.ReadSingle(), _in.ReadSingle(), _in.ReadSingle());
                    mItemBox.Add(newBox.mID, newBox);
                }
                break;
            case 2:
                for (int i = 0; i < count; i++)
                {
                    ItemBox newBox = CreateItemBox();
                    newBox.transform.parent = transform;
                    newBox.mID = _in.ReadInt32();
                    newBox.mPos = new Vector3(_in.ReadSingle(), _in.ReadSingle(), _in.ReadSingle());
                    int size = _in.ReadInt32();
                    for (int j = 0; j < size; j++)
                        newBox.AddItem(_in.ReadInt32());
                    mItemBox.Add(newBox.mID, newBox);
                }
                break;
        }
        _in.Close();
        ms.Close();
    }

    public void New()
    {
        Init();
    }

    public void Restore()
    {
        Init();

        byte[] data = Pathea.ArchiveMgr.Instance.GetData(ArchiveKey);
        if (null != data)
        {
            Import(data);
        }
    }

    void Pathea.ISerializable.Serialize(Pathea.PeRecordWriter w)
    {
        w.Write(Export());
    }
}
