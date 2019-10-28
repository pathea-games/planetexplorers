using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Steamworks;

public class UIWorkShopCtrl : UIBaseWnd
{
    public event OnGuiBtnClicked e_BtnClose = null;

    public GameObject mObjPageWorkShop_0;
    public GameObject mObjPageMyUpLoad_1;
    public GameObject mObjPageLocal_2;
    public GameObject mVCERightPanel;

    public UIPageWorkShopCtrl mPageWorkShopCtrl;
    public UIPageLocalCtrl mPageLocalCtrl;
    public UIPageUploadCtrl mPageUploadCtrl;
    public UILabel mLbInfoMsg;

    public int GridWidth;

    public UIWorkShopBgAdaptiveCtrl BgAdaptiveCtrl;
    public UIWorkShopPage0AdaptiveCtrl Page0AdaptiveCtrl;
    public UIWorkShopPage0AdaptiveCtrl Page1AdaptiveCtrl;
    public UIWorkShopPage2AdaptiveCtrl Page2AdaptiveCtrl;

    private static int m_CurColumnCount;
    private static int m_CurRowCount=3;

    private List<string> m_LocalDownLoaded;       //log:lz-2016.05.17 本地已经下载的，用来查询文件文件是否已经下载了
    private static UIWorkShopCtrl m_Instance;
    //private int mTitleIndex = -1;

    #region mono methods

    void Awake()
    {
        m_Instance = this;
        this.InitAllDownLoadedByLocal();
        m_CurColumnCount = this.GetCurColumnCountByScreenSize();
        this.BgAdaptiveCtrl.UpdateSizeByScreen(m_CurColumnCount, GridWidth);
        this.Page0AdaptiveCtrl.UpdateSizeByScreen(m_CurColumnCount, GridWidth);
    }
    #endregion

    #region private methods

    int GetCurColumnCountByScreenSize()
    {
        int screenWidth=Screen.width;
        if (screenWidth < 1366)
            return 3;
        else if (screenWidth < 1440)
            return 4;
        else if (screenWidth < 1920)
            return 5;
        else
            return 6;
    }

    void OnActivate_0(bool isActivate)
    {
        if (isActivate) //work shop
        {
            if (mObjPageWorkShop_0.activeSelf == false)
            {
                this.Page0AdaptiveCtrl.UpdateSizeByScreen(m_CurColumnCount, GridWidth);
                mObjPageWorkShop_0.SetActive(true);
                mObjPageMyUpLoad_1.SetActive(false);
                mObjPageLocal_2.SetActive(false);
                mVCERightPanel.SetActive(false);
            }
            //mTitleIndex = 0;
        }
    }
    void OnActivate_1(bool isActivate)  // upLoaded
    {
        if (isActivate)
        {
            if (mObjPageMyUpLoad_1.activeSelf == false)
            {
                this.Page1AdaptiveCtrl.UpdateSizeByScreen(m_CurColumnCount, GridWidth);
                mObjPageWorkShop_0.SetActive(false);
                mObjPageMyUpLoad_1.SetActive(true);
                mObjPageLocal_2.SetActive(false);
                mVCERightPanel.SetActive(false);
            }
            //mTitleIndex = 1;
        }
    }
    void OnActivate_2(bool isActivate) // local item
    {
        if (isActivate)
        {
            if (mObjPageLocal_2.activeSelf == false)
            {
                this.Page2AdaptiveCtrl.UpdateSizeByScreen(m_CurColumnCount+1, GridWidth);
                mObjPageWorkShop_0.SetActive(false);
                mObjPageMyUpLoad_1.SetActive(false);
                mObjPageLocal_2.SetActive(true);
                mVCERightPanel.SetActive(false);
            }
            //mTitleIndex = 2;
        }
    }


    void BtnCloseOnClick()
    {
        this.gameObject.SetActive(false);
        if (mPageWorkShopCtrl.mWorkShopMgr != null)
            mPageWorkShopCtrl.mWorkShopMgr.isActve = false;
        if (mPageUploadCtrl.mMyWorkShopMgr != null)
            mPageUploadCtrl.mMyWorkShopMgr.isActve = false;
        if (e_BtnClose != null)
            e_BtnClose();
    }

    void InitAllDownLoadedByLocal()
    {
        m_LocalDownLoaded = new List<string>();
        string filePath = VCConfig.s_IsoPath + "/Download/";
        if (Directory.Exists(filePath))
        {
            string[] fileNames = Directory.GetFiles(filePath, "*" + VCConfig.s_IsoFileExt);
            if (null != fileNames && fileNames.Length > 0)
            {
                for (int i = 0; i < fileNames.Length; i++)
                {
                    m_LocalDownLoaded.Add(Path.GetFileName(fileNames[i]));
                }
            }
        }
    }

    private static bool SaveToFile(byte[] fileData, string fileName, string filePath, string fileExt)
    {
        if (!Directory.Exists(filePath))
            Directory.CreateDirectory(filePath);

        string fileFullPath = filePath + fileName+fileExt;

        if (File.Exists(fileFullPath))
        {
            return false;
        }

        try
        {
			using (FileStream fileStream = new FileStream(fileFullPath, FileMode.Create, FileAccess.Write))
            {
                BinaryWriter bw = new BinaryWriter(fileStream);
                bw.Write(fileData);
                bw.Close();
                fileStream.Close();
            }
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError(string.Format("Save ISO to filepath:{0} Error:{1}",filePath,e.ToString()));
            return false;
        }
    }

    #endregion

    #region override methods
    public override void Show()
    {
        base.Show();
        if (mPageWorkShopCtrl.mWorkShopMgr != null)
            mPageWorkShopCtrl.mWorkShopMgr.isActve = true;
        if (mPageUploadCtrl.mMyWorkShopMgr != null)
            mPageUploadCtrl.mMyWorkShopMgr.isActve = true;
    }
    #endregion

    #region public methods
    public static bool CheckDownloadExist(string fileName)
    {
        if (null == fileName || string.IsNullOrEmpty(fileName.Trim()) || null == m_Instance||null == m_Instance.m_LocalDownLoaded) 
            return false;
        fileName=UIWorkShopCtrl.GetValidFileName(fileName);
        return m_Instance.m_LocalDownLoaded.Contains(fileName);
    }

    public static void AddDownloadFileName(string fileName, bool byUploadPage) //true ByUploadPage , false ByWorkShopPage
    {
        if (null == m_Instance && null == m_Instance.m_LocalDownLoaded) 
            return;
        if(byUploadPage)
            m_Instance.mPageWorkShopCtrl.SetItemIsDownloadedByFileName(fileName);
        else
            m_Instance.mPageUploadCtrl.SetItemIsDownloadedByFileName(fileName);

        fileName = UIWorkShopCtrl.GetValidFileName(fileName);
        m_Instance.m_LocalDownLoaded.Add(fileName);
    }

    public static void PublishFinishCellBack(int _upLoadindex, ulong publishID,ulong hash)
	{
        if (null == m_Instance||null==m_Instance.mPageLocalCtrl) 
            return;
        m_Instance.mPageLocalCtrl.PublishFinishCellBack(_upLoadindex, publishID, hash);
    }

    //Log:lz-2016.05.23 获取当前请求格子的数量
    public static uint GetCurRequestCount()
    {
        return (uint)(m_CurColumnCount * m_CurRowCount);
    }

    //Log:lz-2016.05.23 获取本地ISO界面一页显示的格子数量
    public static uint GetCurLocalShowCount()
    {
        return (uint)((m_CurColumnCount+1) * m_CurRowCount);
    }

    public static int GetCurColumnCount()
    {
        return (int)m_CurColumnCount;
    }

    //log-2016.07.28 获取有效的文件名
    public static string GetValidFileName(string fileName)
    {
        char[] invalidFileNameChars=Path.GetInvalidFileNameChars();
        if (fileName.IndexOfAny(invalidFileNameChars) >=0)
        {
            for (int j = 0; j < invalidFileNameChars.Length; j++)
            {
                //lz-2016.07.28 过滤无效的文件名字符，避免存储报错
                fileName=fileName.Replace(invalidFileNameChars[j].ToString(), string.Empty);
            }
        }
        return fileName;
    }

    public static string DownloadFileCallBack(byte[] fileData, PublishedFileId_t p_id, bool bOK)
    {
        string netCacheFilePath = VCConfig.s_CreationNetCachePath;
        string netCacheFileName = CRC64.Compute(fileData).ToString();
        if (bOK)
        {
            if (SaveToFile(fileData, netCacheFileName, netCacheFilePath, VCConfig.s_CreationNetCacheFileExt))
            {
                Debug.Log("ISO save to netCache filepath succeed!");
            }
            else
            {
                Debug.Log("ISO exist or save failed!");
            }
            return netCacheFilePath + netCacheFileName + VCConfig.s_CreationNetCacheFileExt;
        }
        else
        {
            Debug.Log("ISO download failed!");
        }
        return "";
	}

    #endregion
}



