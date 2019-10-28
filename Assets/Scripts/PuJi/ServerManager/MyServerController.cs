using System.IO;
using UnityEngine;
using System;

public class MyServerController : MonoBehaviour
{
	public static event Action<string, int> OnServerCloseEvent;

	public UIServerCtrl mServerCtrl;

	void Awake()
	{
		mServerCtrl.StartFunc += StartUIServerCtrl;
		mServerCtrl.BtnStart += StartServer;
		mServerCtrl.BtnDelete += DeleteServer;
		mServerCtrl.BtnClose += CloseServer;
		mServerCtrl.BtnRefresh += Refresh;
		mServerCtrl.checkListItem += SelectServerData;

		LoadServer.AddServerEventHandler += AddServer;
		P2PManager.OnServerDisconnectedEvent += SetServerOff;
	}

	void OnDestroy()
	{
		mServerCtrl.StartFunc -= StartUIServerCtrl;
		mServerCtrl.BtnStart -= StartServer;
		mServerCtrl.BtnDelete -= DeleteServer;
		mServerCtrl.BtnClose -= CloseServer;
		mServerCtrl.BtnRefresh -= Refresh;
		mServerCtrl.checkListItem -= SelectServerData;

		LoadServer.AddServerEventHandler -= AddServer;
		P2PManager.OnServerDisconnectedEvent -= SetServerOff;
	}

	void StartUIServerCtrl()
	{
		mServerCtrl.mList.mItems.Clear();
		foreach (MyServer ms in LoadServer.ServerList)
		{
			ConnectedServer cServer = P2PManager.GetServer(ms.gameName, ms.gameMode);
			if (cServer != null)
				mServerCtrl.mList.AddItem(ms.ToServerDataItem(), Color.green);
			else
				mServerCtrl.mList.AddItem(ms.ToServerDataItem());
		}

		mServerCtrl.mList.UpdateList();
	}


	void StartServer()
	{
		if (null == mServerCtrl || null == mServerCtrl.mList)
			return;

        if (mServerCtrl.mList.mSelectedIndex == -1)
            return;

        int index = mServerCtrl.mList.mSelectedIndex;
        string serverName = mServerCtrl.mList.mItems[index].mData[0];
        int sceneMode = (int)MyServer.AdventureOrBuild(mServerCtrl.mList.mItems[index].mData[2]);

        ConnectedServer cServer = P2PManager.GetServer(serverName, sceneMode);
        if (cServer != null)
        {
            Debug.Log("Server is running!");
            MessageBox_N.ShowOkBox(PELocalization.GetString(8000497));
            return;
        }

        MyServer ms = LoadServer.GetServer(serverName, sceneMode);
		if (null == ms)
			return;

        mServerCtrl.GetMyServerInfo(ms);

        if (ms.gameMode == (int)Pathea.PeGameMgr.ESceneMode.Custom)
        {
            Pathea.PeGameMgr.mapUID = ms.uid;
            string dataPath = Path.Combine(GameConfig.CustomDataDir, ms.mapName);
            Pathea.CustomGameData.Mgr.Instance.GetCustomData(Pathea.PeGameMgr.mapUID, dataPath);
        }

        MyServerManager.StartMyServer(ms);
    }

    void AddServer(string serverName, int sceneMode)
    {
        if (mServerCtrl != null)
        {
            MyServer ms = LoadServer.GetServer(serverName, sceneMode);
            if (null != ms)
            {
                string mode = ms.AdventureOrBuild();

                int index = mServerCtrl.mList.mItems.FindIndex(it => it.mData[0].Equals(serverName) && it.mData[2].Equals(mode));
                if (index == -1)
                {
                    mServerCtrl.mList.AddItem(ms.ToServerDataItem());
                    mServerCtrl.mList.UpdateList();
                }
            }
        }

        SetServerOn(serverName, sceneMode);
    }

    void DeleteServer()
    {
        if (null == mServerCtrl)
            return;

        if (mServerCtrl.mList.mSelectedIndex == -1)
            return;

        int index = mServerCtrl.mList.mSelectedIndex;
        string serverName = mServerCtrl.mList.mItems[index].mData[0];
        int sceneMode = (int)MyServer.AdventureOrBuild(mServerCtrl.mList.mItems[index].mData[2]);

        try
        {
            if (LoadServer.DeleteServer(serverName, sceneMode))
            {
                mServerCtrl.mList.mItems.RemoveAt(index);
                mServerCtrl.mList.UpdateList();
            }
        }
        catch
        {
            Debug.Log("Server is running!");
            MessageBox_N.ShowOkBox(PELocalization.GetString(8000497));
        }
    }

    public void SetServerOn(string serverName, int sceneMode)
    {
        if (mServerCtrl == null)
            return;

        MyServer ms = LoadServer.GetServer(serverName, sceneMode);
        if (null == ms)
            return;

        string mode = ms.AdventureOrBuild();

        int index = mServerCtrl.mList.mItems.FindIndex(it => it.mData[0].Equals(serverName) && it.mData[2].Equals(mode));
        if (index >= 0)
        {
            mServerCtrl.mList.SetColor(index, Color.green);
            mServerCtrl.mList.UpdateList();
        }
    }

    void SetServerOff(string serverName, int sceneMode)
    {
        if (mServerCtrl == null)
            return;

        MyServer ms = LoadServer.GetServer(serverName, sceneMode);
        if (null == ms)
            return;

        string mode = ms.AdventureOrBuild();

        int index = mServerCtrl.mList.mItems.FindIndex(it => it.mData[0].Equals(serverName) && it.mData[2].Equals(mode));
        if (index >= 0)
        {
            mServerCtrl.mList.SetColor(index, Color.white);
            mServerCtrl.mList.UpdateList();
        }
    }

    void CloseServer()
    {
        if (mServerCtrl.mList.mSelectedIndex == -1)
            return;

        int index = mServerCtrl.mList.mSelectedIndex;
        string serverName = mServerCtrl.mList.mItems[index].mData[0];
        int sceneMode = (int)MyServer.AdventureOrBuild(mServerCtrl.mList.mItems[index].mData[2]);

		if (null != OnServerCloseEvent)
			OnServerCloseEvent(serverName, sceneMode);
    }

    void Refresh()
    {
        LoadServer.LoadServers();
        StartUIServerCtrl();
    }

    void SelectServerData(int index)
    {
        if (index == -1)
            return;

        string serverName = mServerCtrl.mList.mItems[index].mData[0];
        int mode = (int)MyServer.AdventureOrBuild(mServerCtrl.mList.mItems[index].mData[2]);

        MyServer ms = LoadServer.GetServer(serverName, mode);
        if (null != ms)
            mServerCtrl.UpdateServerInfo(ms);
    }
}

