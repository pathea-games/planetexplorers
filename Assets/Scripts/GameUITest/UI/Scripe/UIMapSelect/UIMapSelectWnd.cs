using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using Pathea;

public class UIMapSelectWnd : UIBaseWnd
{
    public UIHostCreateCtrl mHostCreatCtrl = null;

    [SerializeField]
    Transform Centent = null;

    [SerializeField]
    GameObject UIAMapSelectItemPrefab = null;

    [HideInInspector]
    public UIMapSelectItem mUIMapSelectItem = null;

    [SerializeField]
    UIGrid mGird;
    [SerializeField]
    UILabel mLbPathvalve;
    [SerializeField]
    GameObject mBackBtn;
    [SerializeField]
    GameObject mMassageLb;

    [SerializeField]
    UILabel mLbName;
    [SerializeField]
    UILabel mLbExtension;
    [SerializeField]
    UILabel mLbLastWriteTime;
    [SerializeField]
    UILabel mLbMapSize;
    [SerializeField]
    UITexture mMapTexture;

	[SerializeField]
	UIPopupList mPopPlayer;

    public bool ismulti = false;//默认不是联机

    List<UIMapSelectItem> mItemList = new List<UIMapSelectItem>();

    List<CustomGameData> mCustomGameData = new List<CustomGameData>();

    public string LbPathText { get { return mLbPathvalve.text; } set { mLbPathvalve.text = value; } }


	UIMapSelectItem mSelectMapItem = null;

    public enum ItemType
    {
        it_dir = 0,
        it_map

    }

    class ItemInfo
    {
        public ItemType type;
        public string text;
        public Texture MapTexture = null;
        public Vector3 size = Vector3.zero;
		public List<string> roles = new List<string>();
		public int dataIndex = 0;
    }

    List<ItemInfo> mInfoList = new List<ItemInfo>();

    GameObject AddUIPrefab(GameObject prefab, Transform parentTs)
    {
        GameObject o = GameObject.Instantiate(prefab) as GameObject;
        o.transform.parent = parentTs;
        o.layer = parentTs.gameObject.layer;
        o.transform.localPosition = Vector3.zero;
        o.transform.localScale = Vector3.one;
        return o;
    }

    void Awake()
    {
        GameObject gameUI = AddUIPrefab(UIAMapSelectItemPrefab, Centent);
        mUIMapSelectItem = gameUI.GetComponent<UIMapSelectItem>();
        gameUI.SetActive(false);

        mBackBtn.SetActive(false);
        mLbPathvalve.text = GameConfig.CustomDataDir;

        mMapTexture.gameObject.SetActive(false);
    }

    void Start()
    {

//        Pathea.CustomGameData.Mgr.Instance.Load(mLbPathvalve.text);
//        mCustomGameData = Pathea.CustomGameData.Mgr.Instance.GetCustomGameList();
//		
//
//		for (int i = 0; i < mCustomGameData.Count; i++)
//		{
//			Pathea.CustomGameData data = mCustomGameData[i];
//			ItemInfo Info = new ItemInfo();
//			
//			Info.type = ItemType.it_map;
//			Info.text = data.name;
//			Info.MapTexture = data.screenshot;
//			Info.size = data.size;
//			Info.dataIndex = i;
//			
//			PlayerDesc[] humanDescs = data.humanDescs;
//			foreach (PlayerDesc desc in humanDescs)
//				Info.roles.Add(desc.Name);
//			
//			mInfoList.Add(Info);
//		}
//
//        // GetDirectory(mLbPathvalve.text);
//        EnableBack(mLbPathvalve.text);
//        Reflsh();
    }


    void Reflsh()
    {
        Clear();
        foreach (ItemInfo info in mInfoList)
        {
            AddMapItem(info);
        }
        mGird.repositionNow = true;
    }


    void AddMapItem(ItemInfo info)
    {
        GameObject obj = GameObject.Instantiate(UIAMapSelectItemPrefab) as GameObject;
        obj.transform.parent = mGird.transform;
        obj.transform.localScale = Vector3.one;
        obj.transform.localPosition = Vector3.zero;
        obj.SetActive(true);

        UIMapSelectItem item = obj.GetComponent<UIMapSelectItem>();
        item.Text = info.text;
        item.Type = info.type;
        item.mTexture = info.MapTexture;
        item.mSize = info.size;
		item.index = info.dataIndex;
        item.e_ItemOnDbClick += ItemOnDbClick;
        item.e_ItemOnClick += ItemOnClick;
        mItemList.Add(item);
    }




    void Clear()
    {
        foreach (UIMapSelectItem item in mItemList)
        {
            if (item != null)
            {
                GameObject.Destroy(item.gameObject);
                item.gameObject.transform.parent = null;
            }
        }
        mItemList.Clear();
    }


    void GetDirectory(string path)
    {
        mInfoList.Clear();

        string[] folder = Directory.GetDirectories(path);
        for (int i = 0; i < folder.Length; i++)
        {
            FileInfo FileInfo = new FileInfo(folder[i]);

            ItemInfo info = new ItemInfo();
            info.text = FileInfo.DirectoryName;//文件路径
            info.type = ItemType.it_dir;
            info.text = FileInfo.Name;
            mInfoList.Add(info);
            //FileInfo.Delete();
        }

        string[] flie = Directory.GetFiles(path);
        for (int i = 0; i < flie.Length; i++)
        {
            FileInfo FileInfo = new FileInfo(flie[i]);
            //string FilePath = FileInfo.FullName; //路径
            //string FileName = FileInfo.Name;
            //string FileExtension = FileInfo.Extension;      //获取文件扩展名.voxelform

            //if(FileExtension==".voxelform")
            //{
            ItemInfo info = new ItemInfo();
            info.text = FileInfo.DirectoryName;//文件路径
            info.type = ItemType.it_map;
            info.text = FileInfo.Name;
            mInfoList.Add(info);
            //}
        }

    }


    void EnableBack(string Path)
    {

        mBackBtn.SetActive(false);

    }

    void ItemOnDbClick(object sender)
    {
        UIMapSelectItem item = sender as UIMapSelectItem;
        if (item != null)
        {
            if (item.Type == ItemType.it_dir)
            {
                mLbPathvalve.text += "\\";
                mLbPathvalve.text += item.Text.ToString();

                GetDirectory(mLbPathvalve.text);
                EnableBack(mLbPathvalve.text);
                Reflsh();
                Debug.Log("ItemOnClick: -------------" + item.Text.ToString());
            }
            else
            {

            }


        }
    }

    string OnClickName;

    int Size_x = 0;
    int Size_z = 0;
    void ItemOnClick(object sender)
    {
        UIMapSelectItem item = sender as UIMapSelectItem;
        if (item != null)
        {
            /*            if (item.Type == ItemType.it_map)
                        {

                            FileInfo file = new FileInfo(mLbPathvalve.text.ToString() + "\\" + item.Text.ToString());//实例化FileInfo


                            mLbName.text = Path.GetFileNameWithoutExtension(item.Text.ToString()); //文件名
                            //				mLbExtension.text="voxelform";                          //后缀
                            //mLbLastWriteTime.text=file.LastWriteTime.ToString();              //文件修改时间
                            mLbLenth.text = file.Length.ToString() + " KB";
                            mMassageLb.SetActive(true);
                            Debug.Log("ItemOnClick: -------------" + item.Text.ToString());
                        }
                        else
                        {
                            mMassageLb.SetActive(false);
                        }*/

			mSelectMapItem = item;
            if (item.Type == ItemType.it_map)
            {

                mMapTexture.gameObject.SetActive(true);

                mLbName.text = item.Text.ToString();
                OnClickName = mLbName.text;
                Size_x = (int)item.mSize.x;
                Size_z = (int)item.mSize.z;

                mLbMapSize.text = Size_x.ToString() + "X" + Size_z.ToString();
                //=item.mSize.ToString();
                mMapTexture.mainTexture = item.mTexture;


				PlayerDesc[] humanDescs = mCustomGameData[item.index].humanDescs;

				if (humanDescs.Length != 0)
				{
					mPopPlayer.items.Clear();
					foreach (PlayerDesc pd in humanDescs)
						mPopPlayer.items.Add(pd.Name);
					mPopPlayer.selection  = mPopPlayer.items[0];

					mCustomGameData[item.index].DeterminePlayer(0);

	                mMassageLb.SetActive(true);
				}
            }

            if (ismulti)
            {
                if (mHostCreatCtrl != null)
                    mHostCreatCtrl.mMapName.text = item.Text.ToString();
            }

        }
    }

    void btnBackOnClick()
    {

        DirectoryInfo my = new DirectoryInfo(mLbPathvalve.text);
        DirectoryInfo Parent = my.Parent;
        mLbPathvalve.text = Parent.FullName;
        EnableBack(mLbPathvalve.text);
        GetDirectory(mLbPathvalve.text);

        mMassageLb.SetActive(false);
        Reflsh();
    }

    protected override void OnClose()
    {
        //SelectItem_N.Instance.SetItem(null);
        Clear();
        mInfoList.Clear();
        //mLbPathvalve.text="D:\Pe0.9\CustomGames";
        base.OnClose();
    }


    void OnCancelBtn()
    {
        Destroy(gameObject);
        Pathea.PeFlowMgr.Instance.LoadScene(Pathea.PeFlowMgr.EPeScene.RoleScene);
    }


    void OnStartBtn()
    {
        if (OnClickName != null)
        {
            Pathea.PeGameMgr.playerType = Pathea.PeGameMgr.EPlayerType.Single;
            Pathea.PeGameMgr.loadArchive = Pathea.ArchiveMgr.ESave.New;
            Pathea.PeGameMgr.sceneMode = Pathea.PeGameMgr.ESceneMode.Custom;


            //mLbPathvalve.text
            Pathea.PeGameMgr.gameName = OnClickName;



            Pathea.PeFlowMgr.Instance.LoadScene(Pathea.PeFlowMgr.EPeScene.GameScene);
        }



    }

	void OnSelectionChange(string select_item)
	{
		if (mSelectMapItem == null)
			return;

		int index = mPopPlayer.items.FindIndex(item0 => item0 == select_item);
		mCustomGameData[mSelectMapItem.index].DeterminePlayer(index);

	}

    /*
    [SerializeField] GameObject m_Anchor;
    public UIInput mSearchInput;
    // Use this for initialization
    void Start () {
	
    }
	
    // Update is called once per frame
    void Update () {
	
    }

    protected override void OnClose ()
    {
        //SelectItem_N.Instance.SetItem(null);
        base.OnClose ();
    }

    void OnCancelBtn ()
    {
        Destroy(gameObject);
        Pathea.PeFlowMgr.Instance.LoadScene(Pathea.PeFlowMgr.EPeScene.RoleScene);
    }

    void OnStartBtn()
    {
        //Pathea.PeGameMgr.loadArchive = Pathea.ArchiveMgr.ESave.New;
        //Pathea.PeGameMgr.sceneMode = Pathea.PeGameMgr.ESceneMode.Custom;
        //Pathea.PeFlowMgr.Instance.LoadScene(Pathea.PeFlowMgr.EPeScene.GameScene);

        Pathea.PeGameMgr.playerType = Pathea.PeGameMgr.EPlayerType.Single;
        Pathea.PeGameMgr.loadArchive = Pathea.ArchiveMgr.ESave.New;
        Pathea.PeGameMgr.sceneMode = Pathea.PeGameMgr.ESceneMode.Custom;
        //Pathea.PeGameMgr.mapDataPath = @"E:/VoxelEditor/prjs/test/Publish";
		
        Pathea.PeGameMgr.customDataPath =System.IO.Path.Combine(GameConfig.PEDataPath,  "CustomGames/New World");
        //System.IO.Path.Combine(GameConfig.PEDataPath,  "CustomGames/New World/Mainland/");
		
        Pathea.PeFlowMgr.Instance.LoadScene(Pathea.PeFlowMgr.EPeScene.GameScene);
    }

    private void OnSearchBtn()
    {
        //m_Anchor.SetActive(true);
        string[] str=Directory.GetDirectories("CustomGames");
        FileInfo fi = new FileInfo (str[0]);
        string name=fi.Name;
        //fi.FullName
        VCEditor.Open();
//		string queryText = mSearchInput.text.Trim();
//		if(queryText != "You can type here" && queryText.Length>0)
//		{
    //		string[] str=Directory.GetDirectories();
            //Resources.LoadAssetAtPath();
            //ckboxAll.isChecked = true;
            //mRootType = ItemLabel.Root.all;
            //AfterLeftMeunChecked();
            //LoadPrefabByResources();
    //	}
    }

    // 通过Resources.LoadAtPath方法动态加载Prefab
//	private void LoadPrefabByResources()
    //{
    //	Object prefab = Resources.LoadAssetAtPath("CustomGames\New World\Mainland", typeof(GameObject));
    //	GameObject cube = (GameObject)Instantiate(prefab);
    //	cube.transform.parent = transform;
    //}

    private void ClearBtnOnClick()
    {
        if(mSearchInput.text != "You can type here" && mSearchInput.text.Length>0)
        {
            mSearchInput.text = string.Empty;
            //UpdateLeftList();
        }
    }
*/

}
