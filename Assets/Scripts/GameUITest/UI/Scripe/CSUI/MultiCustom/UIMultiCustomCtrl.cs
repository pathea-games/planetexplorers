using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using PeCustom;

public class UIMultiCustomCtrl : MonoBehaviour
{

    [SerializeField]
    UICustomGameSelectWnd selectWnd;
    [SerializeField]
    UIHostCreateCtrl hostCreatCtrl;
    [SerializeField]
    BoxCollider mMask;

    public int pathCharCount = 300;

    public static bool ignoreIntegrityCheck = false;

    bool? mIntegrity = null;
    public bool? Integrity
    {
        get { return mIntegrity; }
        set { mIntegrity = value; }
    }

    void Awake()
    {
        Init();
    }

    public void Init()
    {
        selectWnd.onInit += OnWndInit;
        selectWnd.onOpen += OnWndOpen;
        selectWnd.onClose += OnWndClose;
        selectWnd.onBack += OnWndBack;

        selectWnd.onMapItemClick += OnWndMapItemClick;
        selectWnd.onMapItemDoubleClick += OnWndMapItemDoubleClick;

        selectWnd.onMapItemSetContent += OnWndMapItemSetContent;
        selectWnd.onMapItemClearContent += OnWndMapItemClearContent;

        selectWnd.onPlayerSelectedChanged += OnWndPlayerSelectChanged;

        selectWnd.onStartBtnClick += OnWndStartClick;
    }

    public void Open()
    {
        selectWnd.Open();
        hostCreatCtrl.OnCustomWndOpen();
    }

    public void Close()
    {
        selectWnd.Close();
        hostCreatCtrl.OnCustomWndClose();
    }

    #region UI_NOTIFY

    void OnWndInit()
    {

    }


    void OnWndOpen()
    {
        if (mCurPath == null)
            mCurPath = GameConfig.CustomDataDir;
        UpdateMapItem(mCurPath);
    }

    void OnWndClose()
    {

    }


    bool OnWndBack()
    {
        DirectoryInfo dirInfo = new DirectoryInfo(mCurPath);
        bool r = false;
        if (dirInfo.Parent != null && Path.GetFileName(mCurPath) != Path.GetFileName(GameConfig.CustomDataDir))
        {
            mCurPath = dirInfo.Parent.FullName;
            r = true;
        }
        else
        {
            mCurPath = GameConfig.CustomDataDir;
            r = false;
        }

        UpdateMapItem(mCurPath);

        return r;
    }

    void OnWndMapItemClick(UICustomGameSelectWnd.CMapInfo mapInfo, UICustomGameSelectWnd.CPlayerInfo playerInfo, UICustomMapItem item)
    {
        MapItemDescs desc = mMapItems[item.index];

        Pathea.CustomGameData customdata = null;
        try
        {
            customdata = Pathea.CustomGameData.Mgr.Instance.GetCustomData(desc.UID, desc.Path);
        }
        catch
        {
            mapInfo.texture.mainTexture = null;
            mapInfo.name.text = "crap";
            mapInfo.size.text = "crap";
        }
        finally
        {
            mapInfo.texture.mainTexture = customdata.screenshot;
            mapInfo.name.text = customdata.name.ToString();
            mapInfo.size.text = customdata.size.x.ToString() + "X" + customdata.size.z.ToString();

            //PlayerDesc[] human_descs = customdata.humanDescs;
            //playerInfo.playerList.items.Clear();
            //foreach (PlayerDesc pd in human_descs)
            //{
            //    playerInfo.playerList.items.Add(pd.Name);
            //}

            //playerInfo.playerList.selection = human_descs[0].Name;
        }
        hostCreatCtrl.UID = desc.UID;
    }

    void OnWndMapItemDoubleClick(UICustomMapItem item)
    {
        if (item.IsFile)
        {

            mCurPath = mMapItems[item.index].Path;

            UpdateMapItem(mCurPath);
        }
    }

    void OnWndPlayerSelectChanged(int playerIndex)
    {
        MapItemDescs desc = mMapItems[selectWnd.selectedItem.index];

        Pathea.CustomGameData customdata = Pathea.CustomGameData.Mgr.Instance.GetCustomData(desc.UID, desc.Path);
        if (customdata.DeterminePlayer(playerIndex))
        {
#if UNITY_EDITOR
            Debug.Log("Select the player [" + customdata.curPlayer.Name + "]");
#endif
        }

    }

    bool mIsProcessCheck = false;
    public bool OnWndStartClick()
    {
        if (mIsProcessCheck)
            return true;

        if (selectWnd.selectedItem != null && !selectWnd.selectedItem.IsFile)
        {
            mIntegrity = null;
            MapItemDescs mid = mMapItems[selectWnd.selectedItem.index];
            ScenarioIntegrityCheck check = ScenarioMapUtils.CheckIntegrityByPath(mid.Path);
            StartCoroutine(ProcessIntegrityCheck(check));
            selectWnd.HintBox.Msg = "Checking";
            selectWnd.HintBox.isProcessing = true;
            selectWnd.HintBox.Open();
            mMask.enabled = true;
        }

        return true;
    }

    public bool HasSelectMap()
    {
        if (selectWnd.selectedItem != null && !selectWnd.selectedItem.IsFile)
            return true;

        return false;
    }

    void OnWndMapItemSetContent(int index, UICustomMapItem item)
    {
        try
        {
            item.IsFile = mMapItems[index].IsDir;
            item.nameStr = mMapItems[index].Name;
        }
        catch (System.Exception e)
        {
            Debug.LogError(e.ToString());
        }
    }

    void OnWndMapItemClearContent(UICustomMapItem item)
    {

    }

    #endregion

    IEnumerator ProcessIntegrityCheck(ScenarioIntegrityCheck check)
    {
        mIsProcessCheck = true;

        while (true)
        {
            if (check.integrated == true)
            {
                try
                {
                    Pathea.PeGameMgr.loadArchive = Pathea.ArchiveMgr.ESave.New;
                    Pathea.PeGameMgr.sceneMode = Pathea.PeGameMgr.ESceneMode.Custom;
                    Pathea.PeGameMgr.gameName = selectWnd.selectedItem.nameStr;
                    Pathea.PeGameMgr.mapUID = mMapItems[selectWnd.selectedItem.index].UID;
                }
                catch
                {
                    Debug.Log("This map is wrong, please chose anther one");
                    MessageBox_N.ShowOkBox(PELocalization.GetString(8000484));
                    selectWnd.HintBox.Close();
                    mMask.enabled = false;
                    mIntegrity = true;
                    break;
                }

                selectWnd.HintBox.Msg = "Correct";
                selectWnd.HintBox.isProcessing = false;
                yield return new WaitForSeconds(0.5f);

                //Pathea.PeFlowMgr.Instance.LoadScene(Pathea.PeFlowMgr.EPeScene.RoleScene);
                mIntegrity = true;
                selectWnd.HintBox.Close();
                mMask.enabled = false;
                Debug.Log("Check Correct");

                break;
            }
            else if (check.integrated == false)
            {
                if (ignoreIntegrityCheck)
                {
                    try
                    {
                        Pathea.PeGameMgr.loadArchive = Pathea.ArchiveMgr.ESave.New;
                        Pathea.PeGameMgr.sceneMode = Pathea.PeGameMgr.ESceneMode.Custom;
                        Pathea.PeGameMgr.gameName = selectWnd.selectedItem.nameStr;
                        Pathea.PeGameMgr.mapUID = mMapItems[selectWnd.selectedItem.index].UID;
                        Pathea.PeFlowMgr.Instance.LoadScene(Pathea.PeFlowMgr.EPeScene.RoleScene);
                    }
                    catch
                    {
                        Debug.Log("This map is wrong, please chose anther one");
                        MessageBox_N.ShowOkBox(PELocalization.GetString(8000484));
                        selectWnd.HintBox.Close();
                        mMask.enabled = false;
                        mIntegrity = false;
                        break;
                    }
                }
                else
                {
                    Debug.Log("This map is wrong, please chose anther one");
                    MessageBox_N.ShowOkBox(PELocalization.GetString(8000484));
                    selectWnd.HintBox.Close();
                    mMask.enabled = false;
                    mIntegrity = false;
                    break;
                }
            }

            yield return 0;
        }

        mIsProcessCheck = false;
    }

    void UpdateMapItem(string dir)
    {
        string path = dir;
        if (path.Length > pathCharCount)
        {
            path = path.Substring(0, Mathf.Max(3, pathCharCount - 3));
            path += "...";
        }
        selectWnd.Path = path;

        mMapItems.Clear();
        GetMapItemDescs(mMapItems, dir);

        selectWnd.CreateMapItem(mMapItems.Count);
    }

    Dictionary<string, ScenarioMapDesc> mMapDescs = new Dictionary<string, ScenarioMapDesc>(10);
    public class MapItemDescs
    {
        public bool IsDir;
        public string UID;
        public string Name;
        public string Path;
    }

    //DirectoryInfo mBackDir = null;
    string mCurPath = null;

    List<MapItemDescs> mMapItems = new List<MapItemDescs>(10);

    void GetMapItemDescs(List<MapItemDescs> mapItemDesces, string dir)
    {
        DirectoryInfo dirInfo = new DirectoryInfo(dir);

        if (!dirInfo.Exists)
        {
            Debug.LogWarning("The dir[" + GameConfig.CustomDataDir + "] is not exsit");
            return;
        }

        DirectoryInfo[] subDirInfos = dirInfo.GetDirectories();

        if (subDirInfos == null || subDirInfos.Length == 0)
        {
            return;
        }


        mMapDescs.Clear();
        ScenarioMapDesc[] map_descs = ScenarioMapUtils.GetMapList(dir);
        for (int i = 0; i < map_descs.Length; i++)
        {
            mMapDescs.Add(map_descs[i].Name, map_descs[i]);
        }

        foreach (DirectoryInfo info in subDirInfos)
        {

            MapItemDescs midcs = new MapItemDescs();
            if (mMapDescs.ContainsKey(info.Name))
            {
                ScenarioMapDesc desc = mMapDescs[info.Name];
                midcs.IsDir = false;
                midcs.Name = desc.Name;
                midcs.Path = desc.Path;
                midcs.UID = desc.UID;
            }
            else
            {
                midcs.IsDir = true;
                midcs.Name = info.Name;
                midcs.Path = info.FullName;
                midcs.UID = null;
            }

            mapItemDesces.Add(midcs);
        }
    }




}
