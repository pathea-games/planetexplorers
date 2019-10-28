using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using ItemAsset;

public class BuildingGui_N : MonoBehaviour 
{
	static BuildingGui_N mInstance;
	public static BuildingGui_N Instance{get{return mInstance;}}
	
	public enum OpType
	{
		BuildBlock,
		ItemSetting,
		NpcSetting,
		File
	}
	OpType	mOpType = OpType.BuildBlock;
	
	public UICheckbox mItemTab;
	public UICheckbox mNpcTab;
	
	public GameObject mItemOpWnd;
	public GameObject mNpcOpWnd;
	public GameObject mFileWnd;
	
	public GameObject mTranWnd;
	
	public UIInput	mPosX;
	public UIInput	mPosY;
	public UIInput	mPosZ;
	
	public UIInput	mRotX;
	public UIInput	mRotY;
	public UIInput	mRotZ;
	
	//Item
	public SelItemGrid_N	mItemPerfab;
	public UIGrid		mItemGrid;
	
//	[HideInInspector]
	SelItemGrid_N 		mCurrentSelItem;
	
	List<BuildOpItem>	mItemList = new List<BuildOpItem>();
	BuildOpItem			mPutOutItem;
	
	//AssetReq		mCurrentReq;
	
	class CreateReq
	{
		public bool		mIsLoad = false;
		public OpType 	mType;
		public int 		mItemId;
		public AssetReq mReq = null;
	}
	
	List<CreateReq> mCreateReqList = new List<CreateReq> ();
	
	//Npc
	public GameObject	mMalePerfab;
	public UIGrid		mNpcGrid;
	List<BuildOpItem> 	mNpcList = new List<BuildOpItem>();
	
	BuildOpItem 		mSelectedItem;
	
	//File
	public UIInput	mFileName;
	public UIGrid	mFileGrid;
	
	public FileNameSelItem_N mFileNamePerfab;
	
	List<FileNameSelItem_N> mFileList = new List<FileNameSelItem_N>();
	
	const int mVersion = 2;
	
	// Use this for initialization
	void Awake()
	{
		mInstance = this;
	}
	
	void Start () {
		// InitItem
        ItemProto.Mgr.Instance.Foreach((item) =>
        {
            if (item.IsBlock())
			{
				SelItemGrid_N addItem = Instantiate(mItemPerfab) as SelItemGrid_N;
				addItem.transform.parent = mItemGrid.transform;
				addItem.transform.localScale = Vector3.one;
                addItem.SetItem(item);
			}
        });

		mItemGrid.Reposition();
		
		// InitNpc
		SelItemGrid_N addNpc = Instantiate(mItemPerfab) as SelItemGrid_N;
		addNpc.transform.parent = mNpcGrid.transform;
		addNpc.transform.localScale = Vector3.one;
		addNpc.SetNpc("Model/PlayerModel/Male", "head_m02");
		mNpcGrid.Reposition();
		
		
		mPosX.text = "0";
		mPosY.text = "0";
		mPosZ.text = "0";
		mRotX.text = "0";
		mRotY.text = "0";
		mRotZ.text = "0";
	}
	
	void Update()
	{
		switch(mOpType)
		{
		case OpType.ItemSetting:
		case OpType.NpcSetting:
			
			if(null != mSelectedItem && !UICamera.inputHasFocus)
			{
				if(mPosX.text == "")
					mPosX.text = "0";
				if(mPosY.text == "")
					mPosY.text = "0";
				if(mPosZ.text == "")
					mPosZ.text = "0";
				if(mRotX.text == "")
					mRotX.text = "0";
				if(mRotY.text == "")
					mRotY.text = "0";
				if(mRotZ.text == "")
					mRotZ.text = "0";
				mSelectedItem.transform.position = new Vector3(Convert.ToSingle(mPosX.text), Convert.ToSingle(mPosY.text), Convert.ToSingle(mPosZ.text));
				mSelectedItem.transform.rotation = Quaternion.Euler(new Vector3(Convert.ToSingle(mRotX.text), Convert.ToSingle(mRotY.text), Convert.ToSingle(mRotZ.text)));
			}
			
			if(null != mPutOutItem)
			{
				if(Input.GetMouseButtonUp(0))
				{
					PutItemDown();
				}
				else
				{
					Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
					RaycastHit hitInfo;
					if(Physics.Raycast(ray, out hitInfo, 500f))
						mPutOutItem.transform.position = hitInfo.point;
				}
			}
			
			if(null != mSelectedItem)
			{
				if(Input.GetKeyDown(KeyCode.Delete))
				{
					mNpcList.Remove(mSelectedItem);
					mItemList.Remove(mSelectedItem);
					Destroy(mSelectedItem.gameObject);
					mSelectedItem = null;
				}
				
				if(Input.GetKeyDown(KeyCode.Escape))
				{
					mSelectedItem.SetActive(false);
					mSelectedItem = null;
				}
			}
			break;
		}
	}
	
	void Save(string name)
	{
		if(name == "")
		{
			Debug.LogError("Inputname is null");
			return;
		}

		string _strFilePath = GameConfig.GetUserDataPath() + "/PlanetExplorers/Building/";
        if (!Directory.Exists(_strFilePath))
            Directory.CreateDirectory(_strFilePath);
		
		_strFilePath += name + ".txt";
		
        using(FileStream fileStream = new FileStream(_strFilePath, FileMode.Create, FileAccess.Write))
        {
            BinaryWriter _out = new BinaryWriter(fileStream);
            if (_out == null)
            {
                Debug.LogError("On WriteRecord FileStream is null!");
                return;
            }
			
			if(Block45Man.self != null)
            {
				byte[] writeData = Block45Man.self.DataSource.ExportData();
                if(writeData != null)
                {
                    _out.Write(writeData);
                }
                else
                {
                    _out.Write(0);
                }
            }
            else
            {
                _out.Write(0);
            }
			
            _out.Close();
            fileStream.Close();
        }
		
		
		_strFilePath = GameConfig.GetUserDataPath() + "/PlanetExplorers/Building/" + name + "SubInfo.txt";
		using(FileStream fileStream = new FileStream(_strFilePath, FileMode.Create, FileAccess.Write))
		{
            BinaryWriter _out = new BinaryWriter(fileStream);
            if (_out == null)
            {
                Debug.LogError("On WriteRecord FileStream is null!");
                return;
            }
			_out.Write(mVersion);
			_out.Write(mNpcList.Count);
			for(int i = 0; i < mNpcList.Count; i++)
			{
				_out.Write(mNpcList[i].transform.position.x);
				_out.Write(mNpcList[i].transform.position.y);
				_out.Write(mNpcList[i].transform.position.z);
			}
			_out.Write(mItemList.Count);
			for(int i = 0; i < mItemList.Count; i++)
			{
				_out.Write(mItemList[i].transform.position.x);
				_out.Write(mItemList[i].transform.position.y);
				_out.Write(mItemList[i].transform.position.z);
				_out.Write(mItemList[i].transform.rotation.eulerAngles.x);
				_out.Write(mItemList[i].transform.rotation.eulerAngles.y);
				_out.Write(mItemList[i].transform.rotation.eulerAngles.z);
				_out.Write(mItemList[i].mItemID);
			}
            _out.Close();
            fileStream.Close();
		}
		ResetFileList();
	}
	
	void OnSaveBtn()
	{
		Save(mFileName.text);
	}
	
	void OnLoadBtn()
	{
		if(mFileName.text == "")
			return;
		
		string _strFilePath = GameConfig.GetUserDataPath() + "/PlanetExplorers/Building/";
        if (!Directory.Exists(_strFilePath))
            Directory.CreateDirectory(_strFilePath);
		
		
		for(int i = 0; i < mItemList.Count; i++)
			Destroy(mItemList[i].gameObject);
		mItemList.Clear();
		
		for(int i = 0; i < mNpcList.Count; i++)
			Destroy(mNpcList[i].gameObject);
		mNpcList.Clear();
		
		if(null != Block45Man.self.DataSource )
			Block45Man.self.DataSource.Clear();
		
        if(File.Exists(_strFilePath + mFileName.text + ".txt"))
        {
            using (FileStream _fileStream = new FileStream(_strFilePath + mFileName.text + ".txt", FileMode.Open, FileAccess.Read))
            {
		        BinaryReader _in = new BinaryReader(_fileStream);
				
				int readVersion = _in.ReadInt32();
				switch(readVersion)
				{
				case 2:
					int Size = _in.ReadInt32();
					for(int i = 0; i < Size; i++)
					{
						IntVector3 index = new IntVector3(_in.ReadInt32(),_in.ReadInt32(),_in.ReadInt32());
						Block45Man.self.DataSource.SafeWrite(new B45Block(_in.ReadByte(),_in.ReadByte()), index.x, index.y, index.z, 0);
					}
					break;
				}
		        _in.Close();
			}
		}
		
		if(File.Exists(_strFilePath + mFileName.text + "SubInfo.txt"))
		{
			using (FileStream _fileStream = new FileStream(_strFilePath + mFileName.text + "SubInfo.txt", FileMode.Open, FileAccess.Read))
			{
		    	BinaryReader _in = new BinaryReader(_fileStream);
				
				int version = _in.ReadInt32();
				int count = _in.ReadInt32();
				switch(version)
				{
				case 1:
					for(int i = 0; i < count; i++)
					{
						Vector3 npcPos = new Vector3(_in.ReadSingle(), _in.ReadSingle(), _in.ReadSingle());
						
						CreateReq req = new CreateReq();
						req.mIsLoad = true;
						req.mType = OpType.NpcSetting;
//						req.mReq = AssetBundlesMan.Instance.AddReq("Model/PlayerModel/Male", npcPos, Quaternion.identity);
//						req.mReq.ReqFinishWithReqHandler += OnSpawned;
//						mCreateReqList.Add(req);
						GameObject obj = Instantiate(Resources.Load("Model/PlayerModel/Male")) as GameObject;
						obj.transform.position = npcPos;
						CreateMode(obj, req);
					}
					break;
				case 2:
					for(int i = 0; i < count; i++)
					{
						Vector3 npcPos = new Vector3(_in.ReadSingle(), _in.ReadSingle(), _in.ReadSingle());
						
						CreateReq req = new CreateReq();
						req.mIsLoad = true;
						req.mType = OpType.NpcSetting;
//						req.mReq = AssetBundlesMan.Instance.AddReq("Model/PlayerModel/Male", npcPos, Quaternion.identity);
//						req.mReq.ReqFinishWithReqHandler += OnSpawned;
//						mCreateReqList.Add(req);
						GameObject obj = Instantiate(Resources.Load("Model/PlayerModel/Male")) as GameObject;
						obj.transform.position = npcPos;
						CreateMode(obj, req);
					}
					count = _in.ReadInt32();
					for(int i = 0; i < count; i++)
					{
						Vector3 itemPos = new Vector3(_in.ReadSingle(), _in.ReadSingle(), _in.ReadSingle());
						Quaternion ItemRot = Quaternion.Euler(new Vector3(_in.ReadSingle(), _in.ReadSingle(), _in.ReadSingle()));
						int itemID = _in.ReadInt32();

                        ItemProto itemData = ItemProto.GetItemData(itemID);
                        if (null == itemData || !itemData.IsBlock())
                        {
                            continue;
                        }

						CreateReq req = new CreateReq();
						req.mIsLoad = true;
						req.mType = OpType.ItemSetting;
						req.mItemId = itemID;
//						req.mReq = AssetBundlesMan.Instance.AddReq(ItemData.GetItemData(itemID).m_ModelPath, itemPos, ItemRot);
//						req.mReq.ReqFinishWithReqHandler += OnSpawned;
//						mCreateReqList.Add(req);
						GameObject obj = Instantiate(Resources.Load(ItemProto.GetItemData(itemID).resourcePath)) as GameObject;
						obj.transform.position = itemPos;
						obj.transform.localRotation = ItemRot;
						CreateMode(obj, req);
						
//						CreateMode(Instantiate(Resources.Load(ItemData.GetItemData(itemID).m_ModelPath)) as GameObject, req);
					}
					break;
				}
		        _in.Close();
			}
		}
	}
	
	void OnBlockBtn(bool selected)
	{
		if(selected)
		{
//			BuildBlockManager.self.EnterBuildMode();
			mTranWnd.SetActive(false);
			mOpType = OpType.BuildBlock;
		}
		else
		{
			//BuildBlockManager.self.QuitBuildMode();
		}
	}
	
	void OnItemSetting(bool selected)
	{
		mItemOpWnd.SetActive(selected);
		if(selected)
		{
			mOpType = OpType.ItemSetting;
			mItemGrid.Reposition();
			mTranWnd.SetActive(true);
		}
	}
	
	void OnNpcSetting(bool selected)
	{
		mNpcOpWnd.SetActive(selected);
		if(selected)
		{
			mOpType = OpType.NpcSetting; 
			mTranWnd.SetActive(true);	
		}
	}
	
	void OnFileBtn(bool selected)
	{
		mFileWnd.SetActive(selected);		
		if(selected)
		{
			mOpType = OpType.File;
			mTranWnd.SetActive(false);
			
			ResetFileList();
		}
	}
	
	void ResetFileList()
	{
		foreach(FileNameSelItem_N item in mFileList)
		{
			item.transform.parent = null;
			Destroy(item.gameObject);
		}
		mFileList.Clear();
		
		string _strFilePath = GameConfig.GetUserDataPath() + "/PlanetExplorers/Building/";
        if (!Directory.Exists(_strFilePath))
            Directory.CreateDirectory(_strFilePath);
		
		string[] fileNames = Directory.GetFiles(_strFilePath);
		
		foreach(string fileName in fileNames)
		{
			if(!fileName.Contains("SubInfo"))
			{
				FileNameSelItem_N item = Instantiate(mFileNamePerfab) as FileNameSelItem_N;
				item.transform.parent = mFileGrid.transform;
				item.transform.localPosition = Vector3.zero;
				item.transform.localScale = Vector3.one;
				item.SetText(System.IO.Path.GetFileNameWithoutExtension(fileName), gameObject);
				mFileList.Add(item);
			}
		}
		mFileGrid.Reposition();
	}
	
	public void OnItemPutOut(SelItemGrid_N selItem)
	{
		if(mCurrentSelItem != selItem)
		{
			mCurrentSelItem = selItem;
			
			CreateReq req = new CreateReq();
			req.mType = OpType.ItemSetting;
			req.mItemId = mCurrentSelItem.mItemData.id;
//			req.mReq = AssetBundlesMan.Instance.AddReq(mCurrentSelItem.mItemData.m_ModelPath, Vector3.zero, Quaternion.identity);
//			req.mReq.ReqFinishWithReqHandler += OnSpawned;
//			mCreateReqList.Add(req);
			
			CreateMode(Instantiate(Resources.Load(mCurrentSelItem.mItemData.resourcePath)) as GameObject, req);
		}
	}
	
	public void OnNpcPutOut(SelItemGrid_N selItem)
	{
		if(mCurrentSelItem != selItem)
		{
			mCurrentSelItem = selItem;
			
			CreateReq req = new CreateReq();
			req.mType = OpType.NpcSetting;
//			req.mReq = AssetBundlesMan.Instance.AddReq(mCurrentSelItem.mNpcPath, Vector3.zero, Quaternion.identity);
//			req.mReq.ReqFinishWithReqHandler += OnSpawned;
//			mCreateReqList.Add(req);
			
			CreateMode(Instantiate(Resources.Load(mCurrentSelItem.mNpcPath)) as GameObject, req);
		}
	}
	
	public void OnBuildOpItemSel(BuildOpItem item)
	{
		
		switch(item.mType)
		{
		case OpType.ItemSetting:
			mItemTab.isChecked = true;
			break;
		case OpType.NpcSetting:
			mNpcTab.isChecked = true;
			break;
		}
		
		if(item != mSelectedItem)
		{
			if(mSelectedItem)
				mSelectedItem.SetActive(false);
			mSelectedItem = item;
			mSelectedItem.SetActive(true);
			mPosX.text = mSelectedItem.transform.position.x.ToString();
			if(mPosX.text.Length > 6)
				mPosX.text = mPosX.text.Substring(0,6);
			mPosY.text = mSelectedItem.transform.position.y.ToString();
			if(mPosY.text.Length > 6)
				mPosY.text = mPosY.text.Substring(0,6);
			mPosZ.text = mSelectedItem.transform.position.z.ToString();
			if(mPosZ.text.Length > 6)
				mPosZ.text = mPosZ.text.Substring(0,6);
			mRotX.text = mSelectedItem.transform.eulerAngles.x.ToString();
			if(mRotX.text.Length > 6)
				mRotX.text = mRotX.text.Substring(0,6);
			mRotY.text = mSelectedItem.transform.eulerAngles.y.ToString();
			if(mRotX.text.Length > 6)
				mRotX.text = mRotX.text.Substring(0,6);
			mRotZ.text = mSelectedItem.transform.eulerAngles.z.ToString();
			if(mRotX.text.Length > 6)
				mRotX.text = mRotX.text.Substring(0,6);
		}
		else if(mSelectedItem)
		{
			mSelectedItem.SetActive(false);
			mSelectedItem = null;
		}
	}
	
	void OnFileSelected(string fileName)
	{
		mFileName.text = fileName;
	}
	
	void CancelOp()
	{
		if(mSelectedItem)
			mSelectedItem.SetActive(false);
		mSelectedItem = null;
		mPutOutItem = null;
		//mCurrentReq = null;
		mCurrentSelItem = null;
	}
	
	void PutItemDown()
	{
		if(mPutOutItem)
			mPutOutItem.GetComponent<Collider>().enabled = true;
		mPutOutItem = null;
		mCurrentSelItem = null;
	}
	
	void CreateMode(GameObject go, CreateReq req)
	{
		BoxCollider bc = go.AddComponent<BoxCollider>();
		Bounds bound = bc.bounds;
		Collider[] childCols = go.GetComponentsInChildren<Collider>();
		
		foreach(Collider col in childCols)
		{
			bound.Encapsulate(col.bounds);
			if(!req.mIsLoad)
				col.enabled = false;
		}
	
		bc.center = bound.center - go.transform.position;
		bc.size = bound.size;
		
		BuildOpItem opItem = go.AddComponent<BuildOpItem>();
		
		switch(req.mType)
		{
		case OpType.ItemSetting:
			opItem.mType = OpType.ItemSetting;
			opItem.mItemID = req.mItemId;
			mItemList.Add(opItem);
			break;
		case OpType.NpcSetting:
			opItem.mType = OpType.NpcSetting;
			mNpcList.Add(opItem);
			bc.center += 1f * Vector3.up;
			bc.size += 0.5f * Vector3.up;
			break;
		}
		if(!req.mIsLoad)
			mPutOutItem = opItem;
	}
	
	public void OnSpawned(GameObject go, AssetReq req)
	{
		foreach(CreateReq cReq in mCreateReqList)
		{
			if(cReq.mReq == req)
			{
				CreateMode(go, cReq);
				return;
			}
		}
		Destroy(go);
	}
}
