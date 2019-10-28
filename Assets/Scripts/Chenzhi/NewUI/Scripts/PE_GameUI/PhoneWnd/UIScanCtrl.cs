using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Mono.Data.SqliteClient;
using System;



#region MetalScanData
public class MetalScanItem
{
    public string mMatName;
    public Color mColor;
    public byte mType;
    public string mTexName;
    public int mDesID;
}

public class MetalScanData
{
    public static Dictionary<int, MetalScanItem> mMetalDic = new Dictionary<int, MetalScanItem>();
    public static void LoadData()
    {
        SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("Mineral");
        while (reader.Read())
        {
            MetalScanItem addInfo = new MetalScanItem();
            addInfo.mMatName = reader.GetString(reader.GetOrdinal("IconName"));
            addInfo.mTexName = reader.GetString(reader.GetOrdinal("TexName"));
            addInfo.mDesID = Convert.ToInt32(reader.GetString(reader.GetOrdinal("Des")));
            addInfo.mType = Convert.ToByte(reader.GetString(reader.GetOrdinal("VoxelType")));
            string[] colorStrs = reader.GetString(reader.GetOrdinal("Color")).Split(',');
            addInfo.mColor = new Color(Convert.ToSingle(colorStrs[0]) / 255f, Convert.ToSingle(colorStrs[1]) / 255f, Convert.ToSingle(colorStrs[2]) / 255f);
            mMetalDic[Convert.ToInt32(reader.GetString(reader.GetOrdinal("ID")))] = addInfo;
        }
    }

    public static MetalScanItem GetItemByID(int ID)
    {
        if (mMetalDic.ContainsKey(ID))
            return mMetalDic[ID];
        return null;
    }

    public static MetalScanItem GetItemByVoxelType(byte type)
    {
        foreach (MetalScanItem msi in mMetalDic.Values)
            if (msi.mType == type)
                return msi;
        return null;
    }

    public static Color GetColorByType(byte type)
    {
        MetalScanItem msi = GetItemByVoxelType(type);
        if (null != msi)
            return msi.mColor;
        return Color.white;
    }

    public static List<int> m_ActiveIDList = new List<int>();
	public static List<bool> m_ScanState = new List<bool>();
    public delegate void OnAddMetalEvent();
    public static event OnAddMetalEvent e_OnAddMetal = null;

    public static void Clear()
    {
        m_ActiveIDList.Clear();
		m_ScanState.Clear();
    }

    public static bool HasMetal(int metalId)
    {
        return m_ActiveIDList.Contains(metalId);
    }

    public static void AddMetalScan(IEnumerable<int> metalID, bool openWnd = true)
    {
        bool addMetal = false;
        foreach (int id in metalID)
        {
            if (!m_ActiveIDList.Contains(id))
            {
                m_ActiveIDList.Add(id);
				m_ScanState.Add(true);
                if (m_ActiveIDList.Count == 1 && openWnd)
                    GameUI.Instance.mPhoneWnd.Show(UIPhoneWnd.PageSelect.Page_Scan);
                addMetal = true;
            }
        }
        if (e_OnAddMetal != null && addMetal)
            e_OnAddMetal();
    }
    public static void AddMetalScan(int metalID)
    {
        bool addMetal = false;

        if (!m_ActiveIDList.Contains(metalID))
        {
            m_ActiveIDList.Add(metalID);
			m_ScanState.Add(true);
            if (m_ActiveIDList.Count == 1)
                GameUI.Instance.mPhoneWnd.Show(UIPhoneWnd.PageSelect.Page_Scan);
            addMetal = true;
        }

        if (e_OnAddMetal != null && addMetal)
            e_OnAddMetal();
    }


    public static byte[] Serialize()
    {
        try
        {
            System.IO.MemoryStream ms = new System.IO.MemoryStream(200);
            using (System.IO.BinaryWriter bw = new System.IO.BinaryWriter(ms))
            {
                bw.Write(m_ActiveIDList.Count);
                for (int i = 0; i < m_ActiveIDList.Count; i++)
				{
					bw.Write(m_ActiveIDList[i]);
					bw.Write(m_ScanState[i]);
				}
            }
            return ms.ToArray();
        }
        catch (System.Exception e)
        {
            Debug.LogWarning(e);
            return null;
        }
    }

    public static bool Deserialize(byte[] buf)
    {
        Clear();
        try
        {
            System.IO.MemoryStream ms = new System.IO.MemoryStream(buf, false);
            using (System.IO.BinaryReader br = new System.IO.BinaryReader(ms))
            {
				int count = br.ReadInt32();
				int byteCount = 4 * (count + 1);
				bool hasStateData = buf.Length > byteCount;
                for (int i = 0; i < count; i++)
                {
					m_ActiveIDList.Add(br.ReadInt32());
					if(hasStateData)
						m_ScanState.Add(br.ReadBoolean());
                }
				if(!hasStateData)
					for (int i = 0; i < count; i++)
						m_ScanState.Add(true);
                return true;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning(e);
            return false;
        }
    }

}

#endregion

public class UIScanCtrl : UIBaseWidget
{
    public UITexture mMatelScanTex;
    [SerializeField]
    UISprite mMetalSpr;
    [SerializeField]
    UILabel mMetalDes;
    [SerializeField]
    UIGrid mMetalScanGrid;
    [SerializeField]
    MetalScanItem_N mMetalScanItemPerfab;
    [SerializeField]
    UILabel mScanTextLabel;
    [SerializeField]
    Camera mMetalScanCam;

    //Debug use
    [Header("金")]
    public  Color AuCol;
    [Header("铜")]
    public  Color CuCol;
    [Header("铁")]
    public  Color FeCol;
    [Header("银")]
    public  Color AgCol;
    [Header("铝")]
    public  Color AlCol;
    [Header("石油")]
    public  Color OilCol;
    [Header("煤")]
    public  Color CoalCol;
    [Header("锌")]
    public  Color ZnCol;
    [SerializeField]
    private bool m_UseDebugMode = false;

    List<MetalScanItem_N> mMetalScanItemList = new List<MetalScanItem_N>();
    
    [SerializeField]
    float ViewDisMax = 400f;
    [SerializeField]
    float ViewDisMin = 10f; //lz-2016.07.14  摄像机最近距离改为10，唐小力说这个距离比较合适

    float mCamViewDis = 250f;
    float mCamDegX = -90f;
    float mCamDegY = 45f;
    AudioController mScanSoundEffect;
    const int m_ScanSoundID = 915;

    public override void Show()
    {
        base.Show();
    }

    public override void OnCreate()
    {
        MetalScanData.e_OnAddMetal += OnAddMetal;
        base.OnCreate();
        this.GetCurColor();
        ResetMetal();
    }

    public override void OnDelete()
    {
        MetalScanData.e_OnAddMetal -= OnAddMetal;
        base.OnDelete();
    }

    protected override void OnHide()
    {
        base.OnHide();
        if (MSScan.Instance.bInScanning)
        {
            StopScanSoundEffect();
            MSScan.Instance.bInScanning = false;
        }
    }


    // Use this for initialization
    void Start()
    {
        mMatelScanTex.mainTexture = mMetalScanCam.targetTexture = new RenderTexture(662, 360, 16);
        //// TestCode
        //List<int> activeId = new List<int>();
        //for (int i =0;i< 10;i++)
        //    activeId.Add(i);
        //MetalScanData.AddMetalScan(activeId);
    }

    void OnDisable()
    {
        if (MSScan.Instance.bInScanning)
        {
            StopScanSoundEffect();
            MSScan.Instance.bInScanning = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (GameUI.Instance == null)
            return;

        if (UICamera.hoveredObject == mMatelScanTex.gameObject)
        {
            if (Input.GetMouseButton(0) || Input.GetMouseButton(1))
            {
                mCamDegX += Input.GetAxis("Mouse X") * 15f * (SystemSettingData.Instance.CameraHorizontalInverse ? 1 : -1);
                if (mCamDegX < 0)
                    mCamDegX += 360f;
                else if (mCamDegX > 360f)
                    mCamDegX -= 360f;

                mCamDegY = Mathf.Clamp(mCamDegY + Input.GetAxis("Mouse Y") * 5f * (SystemSettingData.Instance.CameraVerticalInverse ? 1 : -1), -80f, 80f);
            }
            mCamViewDis = Mathf.Clamp(mCamViewDis - Input.GetAxis("Mouse ScrollWheel") * 30.0f, ViewDisMin, ViewDisMax);
        }



        if (null != GameUI.Instance.mMainPlayer)
        {
            float degx = mCamDegX / 180f * Mathf.PI;
            float degy = mCamDegY / 180f * Mathf.PI;
            mMetalScanCam.transform.position = GameUI.Instance.mMainPlayer.position
                + mCamViewDis * new Vector3(Mathf.Cos(degx) * Mathf.Cos(degy), Mathf.Sin(degy), Mathf.Sin(degx) * Mathf.Cos(degy));
            mMetalScanCam.transform.LookAt(GameUI.Instance.mMainPlayer.position, Vector3.up);
        }
        if (mScanTextLabel.gameObject.activeSelf != MSScan.Instance.bInScanning)
        {
            mScanTextLabel.gameObject.SetActive(MSScan.Instance.bInScanning);
        }

        //lz-2016.10.14 错误 #4363 扫描声音开启后，关闭面板还继续播放
        if (MSScan.Instance.bInScanning)
        {
            PlayScanSoundEffect();
        }
        else
        {
             StopScanSoundEffect();
        }
    }

    protected override void InitWindow()
    {
        ResetMetal();
        base.InitWindow();
        base.SelfWndType = UIEnum.WndType.Scan;
    }

    void OnAddMetal()
    {
        //if (isShow)
        ResetMetal();
    }

    void ResetMetal()
    {
        if (GameUI.Instance != null && GameUI.Instance.mMainPlayer != null)
        {
//            foreach (MetalScanItem_N item in mMetalScanItemList)
//            {
//                item.transform.parent = null;
//                GameObject.Destroy(item.gameObject);
//            }
//            mMetalScanItemList.Clear();

			if(mMetalScanItemList.Count < MetalScanData.m_ActiveIDList.Count)
			{
				for(int i = MetalScanData.m_ActiveIDList.Count - mMetalScanItemList.Count; i >= 0; --i)
				{
					MetalScanItem_N item = Instantiate(mMetalScanItemPerfab) as MetalScanItem_N;
					item.transform.parent = mMetalScanGrid.transform;
					item.transform.localPosition = Vector3.back;
					item.transform.localScale = Vector3.one;
					item.e_OnClick += OnMetalSelected;
					item.mCheckBox.isChecked = true;
					mMetalScanItemList.Add(item);
				}
			}

            for (int i = 0; i < mMetalScanItemList.Count; i++)
            {
				if(i < MetalScanData.m_ActiveIDList.Count)
				{
	                MetalScanItem msi = MetalScanData.GetItemByID(MetalScanData.m_ActiveIDList[i]);
	                if (msi == null)
	                    continue;
					mMetalScanItemList[i].gameObject.SetActive(true);
					mMetalScanItemList[i].SetItem(msi.mMatName, msi.mColor, msi.mType,msi.mDesID);
					mMetalScanItemList[i].mCheckBox.isChecked = MetalScanData.m_ScanState[i];
				}
				else					
					mMetalScanItemList[i].gameObject.SetActive(false);
			}
			mMetalScanGrid.repositionNow = true;
        }
    }


    void OnMetalSelected(object sender)
    {
        MetalScanItem_N item = sender as MetalScanItem_N;
        if (item == null)
            return;
        byte voxelType = item.mType;
        MetalScanItem msi = MetalScanData.GetItemByVoxelType(voxelType);
        if (null != msi)
        {
            mMetalSpr.spriteName = msi.mTexName;
            mMetalSpr.MakePixelPerfect();
            mMetalDes.text =PELocalization.GetString(msi.mDesID);
        }

		for(int i = 0; i < mMetalScanItemList.Count; ++i)
		{
			if(mMetalScanItemList[i] == item)
			{
				MetalScanData.m_ScanState[i] = item.mCheckBox.isChecked;
				break;
			}
		}
//		MetalScanData.m_ScanState[i] = true;
    }

    void BtnOnScan()
    {
        this.DebugMetalColor();
        if (null != GameUI.Instance.mMainPlayer)
        {
            List<byte> matList = new List<byte>();
            for (int i = 0; i < mMetalScanItemList.Count; i++)
            {
                if (mMetalScanItemList[i].mCheckBox.isChecked && mMetalScanItemList[i].gameObject.activeSelf)
                {
                    if (mMetalScanItemList[i].mType != 0)
                        matList.Add(mMetalScanItemList[i].mType);
                }
            }
            if (matList.Count > 0)
            {
                MSScan.Instance.MakeAScan(GameUI.Instance.mMainPlayer.position, matList);
                StopScanSoundEffect();
                PlayScanSoundEffect();
            }
        }
    }

    //lz-2016.06.15  支持 #2397 添加UI音
    void PlayScanSoundEffect()
    {
        if (null == mScanSoundEffect)
        {
            mScanSoundEffect = AudioManager.instance.Create(Vector3.zero, m_ScanSoundID,null,false,false);
        }
        if (null != mScanSoundEffect && !mScanSoundEffect.isPlaying)
        {
            mScanSoundEffect.PlayAudio();
        }
    }

    //lz-2016.10.14 停止扫描音效
    void StopScanSoundEffect()
    {
        if (null != mScanSoundEffect)
        {
            if (mScanSoundEffect.isPlaying)
            {
                mScanSoundEffect.StopAudio();
            }
        }
    }

    //lz-2016.10.14 暂停扫描音效
    void PauseScanSoundEffect()
    {
        if (null != mScanSoundEffect)
        {
            if (mScanSoundEffect.isPlaying)
            {
                mScanSoundEffect.PauseAudio();
            }
        }
    }

    void BtnOnSeclectAll()
    {
        foreach (MetalScanItem_N item in mMetalScanItemList)
        {
            //byte voxelType = item.mType;
            //MetalScanItem msi = MetalScanData.GetItemByVoxelType(voxelType);
            //if (msi != null)
            //{
            //    mMetalSpr.spriteName = msi.mTexName;
            //    mMetalSpr.MakePixelPerfect();
            //    mMetalDes.text = msi.mDes;
            //}
            item.mCheckBox.isChecked = true;
		}
		for(int i = 0; i < MetalScanData.m_ScanState.Count; ++i)
			MetalScanData.m_ScanState[i] = true;
    }

    void BtnOnDeseclectAll()
    {
        foreach (MetalScanItem_N item in mMetalScanItemList)
        {
            //byte voxelType = item.mType;
            //MetalScanItem msi = MetalScanData.GetItemByVoxelType(voxelType);
            //if (msi != null)
            //{
            //    mMetalSpr.spriteName = msi.mTexName;
            //    mMetalSpr.MakePixelPerfect();
            //    mMetalDes.text = msi.mDes;
            //}
            item.mCheckBox.isChecked = false;
        }
		for(int i = 0; i < MetalScanData.m_ScanState.Count; ++i)
			MetalScanData.m_ScanState[i] = false;
    }

    private void DebugMetalColor()
    {
        if (Application.isEditor && m_UseDebugMode)
        {
            MetalScanData.mMetalDic[1].mColor = AuCol;
            MetalScanData.mMetalDic[2].mColor = CuCol;
            MetalScanData.mMetalDic[3].mColor = FeCol;
            MetalScanData.mMetalDic[4].mColor = AgCol;
            MetalScanData.mMetalDic[5].mColor = AlCol;
            MetalScanData.mMetalDic[6].mColor = OilCol;
            MetalScanData.mMetalDic[7].mColor = CoalCol;
            MetalScanData.mMetalDic[8].mColor = ZnCol;
        }
    }

    private void GetCurColor()
    {
          if (Application.isEditor && m_UseDebugMode)
        {
            AuCol=MetalScanData.mMetalDic[1].mColor;
            CuCol=MetalScanData.mMetalDic[2].mColor;
            FeCol=MetalScanData.mMetalDic[3].mColor;
            AgCol=MetalScanData.mMetalDic[4].mColor;
            AlCol=MetalScanData.mMetalDic[5].mColor;
            OilCol=MetalScanData.mMetalDic[6].mColor;
            CoalCol=MetalScanData.mMetalDic[7].mColor;
            ZnCol=MetalScanData.mMetalDic[8].mColor;
          }
    }

}


