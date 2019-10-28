using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;

public class UIRadioCtrl : UIBaseWidget
{
    [SerializeField]
    private UIButton m_StartOrPauseBtn;
    [SerializeField]
    private UISprite m_StartSprite;
    [SerializeField]
    private UISprite m_StopSprite;
    [SerializeField]
    private UIButton m_NextBtn;
    [SerializeField]
    private UIButton m_PreviousBtn;
    [SerializeField]
    private UISlider m_PlayingSlider;
    [SerializeField]
    private UILabel m_PlayingLb;
    [SerializeField]
    private UILabel m_CurSoundNameLb;
    [SerializeField]
    private UISlider m_VolumeSlider;
    [SerializeField]
    private GameObject m_ListItemPrefab;
    [SerializeField]
    private UIGrid m_ListParentGrid;
    [SerializeField]
    private UIPanel m_ListClipPanel;
    [SerializeField]
    private UIScrollBar m_ContentScrollBar;
    [SerializeField]
    private UIListItem m_BackupSelectItem;
    [SerializeField]
    private UITexture m_UISpectrumTex;
    [SerializeField]
    private UILabel m_Shape0Lb;
    [SerializeField]
    private UILabel m_Shape1Lb;
    [SerializeField]
    private UILabel m_Shape2Lb;
    [SerializeField]
    private UIPopupList m_PlayModePL;
    [SerializeField]
    private int m_SpectrumWidth = 600;
    [SerializeField]
    private int m_SpectrumHeight = 200;
    [SerializeField]
    private Color32 m_SpectrumTopCol;
    [SerializeField]
    private Color32 m_SpectrumBottomCol;
    [SerializeField]
    private int m_SpectrumXGridCount = 20;
    [SerializeField]
    private int m_SpectrumYGridCount = 15;
    [SerializeField]
    private int SpectrumGridBorderX = 5;
    [SerializeField]
    private int SpectrumGridBorderY = 5;
    [SerializeField]
    private int m_SampleLength = 10 << 2;
    [SerializeField]
    private float m_LeapInterval = 0.1f;
    [SerializeField]
    private UICheckbox m_OpenBgMusicCK;


    private Color32[] m_RandomCols = new Color32[]
    {
        new Color32(255,255,255,255),   //白
        new Color32(75,210,255,255),  //淡蓝
        new Color32(122,231,252,255),   //浅蓝
        new Color32(103,248,95,255),   //淡绿
        new Color32(255,255,162,255),   //淡黄
        new Color32(158,159,249,255),   //蓝紫
        new Color32(217,174,255,255),   //浅蓝紫
        new Color32(250,165,250,255),   //浅红
        new Color32(244,173,81,255),   //橘黄
    };

    private Color32 m_TopNextCol;
    private Color32 m_BottomNextCol;
    private float m_LerpT;
    private float m_StartTime;

    private GraphPlotter.GraphShapeType mGraphShapeType;
    private Texture2D m_SpectrumTex2d = null;
    private Color32[] m_SpectrumTexClos = null;
    private GraphPlotter m_Plotter;
    private bool m_UpdateSpectrum;
    private float[] m_SampleData;
    private float[] m_PlotData;
    private Queue<UIListItem> m_ListItemPools;
    private List<UIListItem> m_CurListItems;

    private bool m_UpdatePlayProgress;
    private Dictionary<string, RadioManager.SoundPlayMode> m_PlayModeDic;

    RadioManager.SoundPlayState mPlayState;
    RadioManager.SoundPlayState m_PlayState
    {
        get { return mPlayState; }
        set
        {
            if (mPlayState != value)
            {
                mPlayState = value;
                switch (value)
                {
                    case RadioManager.SoundPlayState.Playing:
                        m_StopSprite.enabled = true;
                        m_StartSprite.enabled = false;
                        m_StartOrPauseBtn.tweenTarget = m_StopSprite.gameObject;
                        InitSpectrumArray();
                        m_UpdateSpectrum = true;
                        break;
                    case RadioManager.SoundPlayState.Stop:
                    case RadioManager.SoundPlayState.Pause:
                        m_StopSprite.enabled = false;
                        m_StartSprite.enabled = true;
                        m_StartOrPauseBtn.tweenTarget = m_StartSprite.gameObject;
                        m_UpdateSpectrum = false;
                        ResetSpectrumArray();
                        break;
                }

                if (value == RadioManager.SoundPlayState.Playing || value == RadioManager.SoundPlayState.Pause)
                {
                    if (!m_UISpectrumTex.enabled)
                        m_UISpectrumTex.enabled = true;
                }
                else
                {
                    if (m_UISpectrumTex.enabled)
                        m_UISpectrumTex.enabled = false;

                    m_PlayingSlider.sliderValue = 0;
                    m_PlayingLb.text = "--:--";
                }

                m_CurSoundNameLb.text = RadioManager.Instance.CurSoundInfo.Name;
            }
        }
    }

    #region override methods

    protected override void InitWindow()
    {
        base.InitWindow();
        base.SelfWndType = UIEnum.WndType.Radio;
        RadioManager.Instance.Init();
        Init();
    }

    public override void Show()
    {
        base.Show();
        RadioManager.Instance.RefreshSoundsList();
        LoadSoundsList();
        m_BackupSelectItem = null;
        UpdateSelectItem();
        RadioManager.Instance.UpdateSelectItemEvent += UpdateSelectItem;
        RadioManager.Instance.PlayErrorEvent += PlayErrorEvent;
    }

    protected override void OnHide()
    {
        base.OnHide();
        RecoveryListItem();
        RadioManager.Instance.UpdateSelectItemEvent -= UpdateSelectItem;
        RadioManager.Instance.PlayErrorEvent -= PlayErrorEvent;
    }

    #endregion

    #region mono methods

    void Update()
    {
        m_PlayState = RadioManager.Instance.PlayState;
        if (m_PlayState == RadioManager.SoundPlayState.Playing||(m_PlayState == RadioManager.SoundPlayState.Pause|| m_PlayState == RadioManager.SoundPlayState.Stop&& !m_UpdatePlayProgress))
        {
            float totalTime = RadioManager.Instance.TotalTime;
            float curTime = m_UpdatePlayProgress? RadioManager.Instance.CurTime: Mathf.Clamp01(m_PlayingSlider.sliderValue)* totalTime;
            
            if (m_UpdatePlayProgress)
            {
                m_PlayingSlider.sliderValue = totalTime <= 0 ? 0 : curTime / totalTime;
            }
            if (totalTime < 3600)
                m_PlayingLb.text = string.Format("{0:D2}:{1:D2}/{2:D2}:{3:D2}", (int)curTime / 60, (int)curTime % 60, (int)totalTime / 60, (int)totalTime % 60);
            else
                m_PlayingLb.text = string.Format("{0:D2}:{1:D2}:{2:D2}/{3:D2}:{4:D2}:{5:D2}", (int)curTime / 3600, (int)curTime / 60, (int)curTime % 60, (int)totalTime / 3600, (int)totalTime / 60, (int)totalTime % 60);
        }

        PlotSpectrum();
    }

    #endregion

    #region private methods

    void Init()
    {
        SetGraphShapeType( GraphPlotter.GraphShapeType.Grid);
        m_UpdateSpectrum = false;
        m_PlayState = RadioManager.SoundPlayState.Stop;
        m_LerpT = 0f;
        m_StartTime = Time.realtimeSinceStartup;

        m_Plotter = new GraphPlotter();
        m_Plotter.TextureWidth = m_SpectrumWidth;
        m_Plotter.TextureHeight = m_SpectrumHeight;

        m_TopNextCol = m_RandomCols[0];
        m_BottomNextCol= m_RandomCols[1];
        m_SpectrumTopCol = m_TopNextCol;
        m_SpectrumBottomCol = m_BottomNextCol;

        m_PlayingSlider.sliderValue = 0f;
        m_VolumeSlider.sliderValue = 1f;

        m_UpdatePlayProgress = true;

        m_PlayModePL.items.Clear();
        m_PlayModeDic = new Dictionary<string, RadioManager.SoundPlayMode>();
        m_PlayModeDic[PELocalization.GetString(8000972)] = RadioManager.SoundPlayMode.Single;
        m_PlayModeDic[PELocalization.GetString(8000973)] = RadioManager.SoundPlayMode.SingleLoop;
        m_PlayModeDic[PELocalization.GetString(8000974)] = RadioManager.SoundPlayMode.Order;
        m_PlayModeDic[PELocalization.GetString(8000975)] = RadioManager.SoundPlayMode.ListLoop;
        m_PlayModeDic[PELocalization.GetString(8000976)] = RadioManager.SoundPlayMode.Random;

        m_PlayModePL.items.AddRange(m_PlayModeDic.Keys.ToArray());
        m_PlayModePL.selection = PELocalization.GetString(8000975);

        m_PlayModePL.onSelectionChange = (item) =>
        {
            if (m_PlayModeDic.ContainsKey(item))
            {
                RadioManager.Instance.PlayMode= m_PlayModeDic[item];
            }
        };


        string str = PELocalization.GetString(8000970);
        m_Shape0Lb.text = string.Format("{0} 1", str);
        m_Shape1Lb.text = string.Format("{0} 2", str);
        m_Shape2Lb.text = string.Format("{0} 3", str);

        m_ListItemPools = new Queue<UIListItem>();
        m_CurListItems = new List<UIListItem>();

        if (null != m_UISpectrumTex)
        {
            m_SpectrumTex2d = new Texture2D(m_SpectrumWidth, m_SpectrumHeight, TextureFormat.ARGB32, false);
            m_UISpectrumTex.transform.localScale = new Vector3(m_SpectrumWidth, m_SpectrumHeight, 1);
            m_SpectrumTex2d.wrapMode = TextureWrapMode.Clamp;
            m_SpectrumTex2d.filterMode = FilterMode.Point;
            m_SpectrumTex2d.anisoLevel = 0;
            m_UISpectrumTex.mainTexture = m_SpectrumTex2d;
            m_SpectrumTexClos = m_SpectrumTex2d.GetPixels32();

        }

        UIEventListener.Get(m_StartOrPauseBtn.gameObject).onClick = (go) =>
        {
            if (null != RadioManager.Instance)
            {
                if (m_PlayState == RadioManager.SoundPlayState.Playing)
                    RadioManager.Instance.PauseCurSound();
                else
                    RadioManager.Instance.ContinueCurSound();

            }
        };

        UIEventListener.Get(m_NextBtn.gameObject).onClick = (go) =>
        {
            if (null != RadioManager.Instance)
            {
                RadioManager.Instance.PlayNextSound();
            }
        };

        UIEventListener.Get(m_PreviousBtn.gameObject).onClick = (go) =>
        {
            if (null != RadioManager.Instance)
            {
                RadioManager.Instance.PlayPreviousSounds();
            }
        };

        m_VolumeSlider.onValueChange += (val) =>
        {
            if (null != RadioManager.Instance)
            {
                RadioManager.Instance.SetVolume(m_VolumeSlider.sliderValue);
            };
        };

        UIEventListener.Get(m_PlayingSlider.gameObject).onPress += (go, isPress) =>
        {
            if (isPress)
            {
                m_UpdatePlayProgress = false;
            }
            else
            {
                RadioManager.Instance.SetTime(Mathf.Clamp01(m_PlayingSlider.sliderValue) * RadioManager.Instance.TotalTime);
                m_UpdatePlayProgress = true;
            }
        };

        UIEventListener.Get(m_PlayingSlider.thumb.gameObject).onPress += (go, isPress) =>
        {
            if (isPress)
            {
                m_UpdatePlayProgress = false;
            }
            else
            {
                RadioManager.Instance.SetTime(Mathf.Clamp01(m_PlayingSlider.sliderValue) * RadioManager.Instance.TotalTime);
                m_UpdatePlayProgress = true;
            }
        };

        m_OpenBgMusicCK.startsChecked = true;
        m_OpenBgMusicCK.onStateChange = (isCheck) =>
        {
            RadioManager.Instance.SetBgMusicState(isCheck);
        };

        RadioManager.Instance.PlayErrorEvent = null;
    }

    void LoadSoundsList()
    {
        List<RadioManager.RadioFileInfo> soundsList = RadioManager.Instance.SoundsInfoList;
        if (null != soundsList && soundsList.Count > 0)
        {
            for (int i = 0; i < soundsList.Count; i++)
            {
                UIListItem item;
                if (m_ListItemPools.Count > 0)
                {
                    item = m_ListItemPools.Dequeue();
                    item.gameObject.SetActive(true);
                }
                else
                {
                    item = GetListItem();
                }

                item.UpdateInfo(i, soundsList[i].Name, soundsList[i].PlayError);
                item.SelectEvent = ListItemSelectEvent;
                m_CurListItems.Add(item);
            }
            Respotion();
        }
    }

    void PlayErrorEvent(int index)
    {
        if (null != m_CurListItems && index >= 0 && index < m_CurListItems.Count)
        {
            m_CurListItems[index].SetIsPlayError(true);
        }
    }

    void RecoveryListItem()
    {
        if(null!=m_CurListItems&& m_CurListItems.Count>0)
        {
            UIListItem item;
            for (int i = 0; i < m_CurListItems.Count; i++)
            {
                item=m_CurListItems[i];
                item.ResetItem();
                item.gameObject.SetActive(false);
                m_ListItemPools.Enqueue(item);
            }
            m_CurListItems.Clear();
        }
    }

    void UpdateSelectItem()
    {
        if (null==m_BackupSelectItem || m_BackupSelectItem.ID != RadioManager.Instance.CurSoundsIndex)
        {
            if(m_BackupSelectItem) m_BackupSelectItem.CancelSelect();
            int index = RadioManager.Instance.CurSoundsIndex;
            if (index >= 0 && index < m_CurListItems.Count)
            {
                m_BackupSelectItem = m_CurListItems[index];
                m_BackupSelectItem.Select();
                if (!m_ListClipPanel.IsVisible(m_BackupSelectItem.transform.position))
                {
                    m_ContentScrollBar.scrollValue = index / (m_CurListItems.Count-1);
                }
            }
        }
    }

    void Respotion()
    {
        m_ListParentGrid.Reposition();
        m_ContentScrollBar.scrollValue = 0f;
    }

    void ListItemSelectEvent(UIListItem item)
    {
        if (item != m_BackupSelectItem)
        {
            if (null != m_BackupSelectItem)
                m_BackupSelectItem.CancelSelect();
            if (null != RadioManager.Instance) RadioManager.Instance.PlaySounds(item.ID);
            m_BackupSelectItem = item;
        }
    }

    UIListItem GetListItem()
    {
        GameObject go = GameObject.Instantiate(m_ListItemPrefab.gameObject) as GameObject;
        go.transform.parent = m_ListParentGrid.gameObject.transform;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = Vector3.one;
        go.transform.localPosition = new Vector3(0, 0, 0);
        return go.GetComponent<UIListItem>();
    }

    void InitSpectrumArray()
    {
        m_SampleData = new float[m_SampleLength];
    }

    void ResetSpectrumArray()
    {
        m_SampleData = null;
    }

    void SetGraphShapeType(GraphPlotter.GraphShapeType type)
    {
        if (type != mGraphShapeType)
        {
            mGraphShapeType = type;
            switch (type)
            {
                case GraphPlotter.GraphShapeType.TopAndBottom:
                case GraphPlotter.GraphShapeType.Top:
                    if(null==m_PlotData|| m_PlotData.Length!= m_SampleLength)
                        m_PlotData = new float[m_SampleLength];
                    break;
                case GraphPlotter.GraphShapeType.Grid:
                    if (null == m_PlotData || m_PlotData.Length != m_SpectrumXGridCount)
                        m_PlotData = new float[m_SpectrumXGridCount];
                    break;
            }
        }
    }


    void PlotSpectrum()
    {
        if (!m_UpdateSpectrum) return;

        RadioManager.Instance.GetOutputData(m_SampleData, 0);

        if (null == m_SampleData || m_SampleData.Length <= 0)
            return;

        if (m_SpectrumTopCol.Equals(m_TopNextCol))
        {
            while (true)
            {
                int colIndex = UnityEngine.Random.Range(0, m_RandomCols.Length);
                if (!m_TopNextCol.Equals(m_RandomCols[colIndex]))
                {
                    m_TopNextCol = m_RandomCols[colIndex];
                    break;
                }
            }
            m_LerpT = 0f;
        }

        if (m_SpectrumBottomCol.Equals(m_BottomNextCol))
        {
            while (true)
            {
                int colIndex = UnityEngine.Random.Range(0, m_RandomCols.Length);
                if (!m_BottomNextCol.Equals(m_RandomCols[colIndex]) && !m_TopNextCol.Equals(m_RandomCols[colIndex]))
                {
                    m_BottomNextCol = m_RandomCols[colIndex];
                    break;
                }
            }
            m_LerpT = 0f;
        }

        m_Plotter.TopColor = m_SpectrumTopCol;
        m_Plotter.BottomColor = m_SpectrumBottomCol;

        if (Time.realtimeSinceStartup - m_StartTime >= m_LeapInterval)
        {
            m_LerpT += Time.deltaTime;
            m_SpectrumTopCol = Color32.Lerp(m_SpectrumTopCol, m_TopNextCol, m_LerpT);
            m_SpectrumBottomCol = Color32.Lerp(m_SpectrumBottomCol, m_BottomNextCol, m_LerpT);
            m_StartTime = Time.realtimeSinceStartup;
        }
        
        switch (mGraphShapeType)
        {
            case GraphPlotter.GraphShapeType.TopAndBottom:
                m_Plotter.PlotGraph(m_SampleData, m_PlotData, m_SpectrumTexClos);
                break;
            case GraphPlotter.GraphShapeType.Top:
                m_Plotter.PlotGraph2(m_SampleData, m_PlotData, m_SpectrumTexClos);
                break;
            case GraphPlotter.GraphShapeType.Grid:
                m_Plotter.PlotGraph3(m_SampleData, m_PlotData, m_SpectrumXGridCount, m_SpectrumYGridCount, SpectrumGridBorderX, SpectrumGridBorderY, m_SpectrumTexClos);
                break;
            default:
                break;
        }
        
        if (null != m_SpectrumTex2d && null != m_SpectrumTexClos)
        {
            m_SpectrumTex2d.SetPixels32(m_SpectrumTexClos);
            m_SpectrumTex2d.Apply();
        }

    }

    void OnShape0Ck(bool state)
    {
        if (state)
            SetGraphShapeType(GraphPlotter.GraphShapeType.Grid);
    }

    void OnShape1Ck(bool state)
    {
        if (state)
            SetGraphShapeType(GraphPlotter.GraphShapeType.TopAndBottom);
    }

    void OnShape2Ck(bool state)
    {
        if (state)
            SetGraphShapeType(GraphPlotter.GraphShapeType.Top);
    }
    #endregion

}
