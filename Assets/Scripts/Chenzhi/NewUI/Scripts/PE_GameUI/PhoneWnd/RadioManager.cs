using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System;

public class RadioManager : MonoBehaviour
{
    public struct RadioFileInfo
    {
        public string Name { get; private set;}
        public string FilePath { get; private set; }
        public string Extension { get; private set; }
        public string ExtensionNotDot { get; private set; }
        public bool PlayError { get;private set; }

        public RadioFileInfo(string path)
        {
            FilePath = path;
            Name = Path.GetFileNameWithoutExtension(path);
            Extension = Path.GetExtension(path).ToLower();
            ExtensionNotDot = Extension.Substring(1, Extension.Length - 1);
            PlayError = false;
        }

        public void SetPlayError(bool isPlayError)
        {
            PlayError = isPlayError;
        }
    }

    public enum SoundPlayState
    {
        Playing,
        Stop,
        Pause
    }

    public enum SoundPlayMode
    {
        Single,         //单首播放
        SingleLoop,     //单首循环
        Order,           //顺序播放
        ListLoop,       //列表循环
        Random          //随机播放
    }

    #region static field
    private static RadioManager m_Instance;
    public static RadioManager Instance
    {
        get
        {
            if (m_Instance == null)
            {
                if (Application.isPlaying)
                {
                    GameObject go = Resources.Load<GameObject>("Prefab/GameUI/RadioManager");
                    if (go != null)
                    {
                        m_Instance = GameObject.Instantiate(go).GetComponent<RadioManager>();
                    }
                    DontDestroyOnLoad(go);
                }
            }
            return m_Instance;
        }
    }
    #endregion

    public List<RadioFileInfo> SoundsInfoList { get; private set; }
    public SoundPlayState PlayState { get; private set; }
    public SoundPlayMode PlayMode { get; set; }
    public int CurSoundsIndex { get; private set; }
    public float CurTime { get { return null == m_AudioSource ? 0 : m_AudioSource.time; } }
    public float TotalTime { get { return null == m_AudioSource||null== m_AudioSource.clip ? 0 : m_AudioSource.clip.length; } }
    public float CurVolume { get { return null == m_AudioSource ? 0 : m_AudioSource.volume; } }
    public int CurTimeSamples { get { return null == m_AudioSource ? 0 : m_AudioSource.timeSamples; } }
    public int Frequency { get { return null == m_AudioSource || null == m_AudioSource.clip ? 0 : m_AudioSource.clip.frequency; } }
    public RadioFileInfo CurSoundInfo { get; private set; }
    public float SwitchTime { get; private set; }
    public bool OpenBgMusic { get; private set; }

    public Action<int> PlayErrorEvent;

    public Action UpdateSelectItemEvent;

    private List<string> m_NAudioSupportFormat;
    private List<string> m_UnitySupportFormat;
    private AudioSource m_AudioSource;
    private bool m_SwitchSound;
    private float m_StartTime;
    private float m_BackupBgMusicVolumeValue;

    #region mono methods

    void Update()
    {
        if (PlayState == SoundPlayState.Playing)
        {
            if (null != m_AudioSource&&!m_AudioSource.isPlaying)
            {
                PlayState = SoundPlayState.Stop;
                SetTime(0);
                if (PlayMode != SoundPlayMode.Single)
                {
                    m_SwitchSound = true;
                    m_StartTime = Time.realtimeSinceStartup;
                }
            }
        }

        if (m_SwitchSound&&Time.realtimeSinceStartup- m_StartTime>=SwitchTime)
        {
            switch (PlayMode)
            {
                case SoundPlayMode.Single:
                    break;
                case SoundPlayMode.SingleLoop:
                    PlaySounds(CurSoundsIndex);
                    break;
                case SoundPlayMode.Order:
                    if (CurSoundsIndex >= 0 && CurSoundsIndex < (SoundsInfoList.Count - 1))
                    {
                        PlayNextSound();
                    }
                    break;
                case SoundPlayMode.ListLoop:
                    PlayNextSound();
                    break;
                case SoundPlayMode.Random:
                    PlaySounds(UnityEngine.Random.Range(0, SoundsInfoList.Count));
                    break;
            }
            m_SwitchSound = false;
        }
    }

    #endregion

    #region public methods

    public void Init()
    {
        PlayState = SoundPlayState.Stop;
        PlayMode = SoundPlayMode.ListLoop;
        m_SwitchSound = false;
        SwitchTime = 1.5f;
        m_StartTime = Time.realtimeSinceStartup;

        m_NAudioSupportFormat = new List<string>();
        m_NAudioSupportFormat.AddRange(Enum.GetNames(typeof(NAudioPlayer.SupportFormatType)));

        //lz-2016.12.29 NAudioPlayer.SupportFormatType.mp3只有windows支持
        if (Application.platform != RuntimePlatform.WindowsPlayer && Application.platform != RuntimePlatform.WindowsEditor)
        {
            m_NAudioSupportFormat.Remove(NAudioPlayer.SupportFormatType.mp3.ToString());
        }

        m_UnitySupportFormat = new List<string>();
        m_UnitySupportFormat.Add("ogg");
        m_UnitySupportFormat.Add("wav");
        SoundsInfoList = new List<RadioFileInfo>();
        RefreshSoundsList();

        if (null == m_AudioSource)
        {
            m_AudioSource = gameObject.AddComponent<AudioSource>();
            m_AudioSource.loop = false;
            m_AudioSource.clip = null;
        }

        OpenBgMusic = true;
        m_BackupBgMusicVolumeValue = SystemSettingData.Instance.MusicVolume;
        UIOption.Instance.VolumeChangeEvent += () =>
        {
            m_BackupBgMusicVolumeValue = SystemSettingData.Instance.MusicVolume;
        };
    }

    public void RefreshSoundsList()
    {
        SoundsInfoList.Clear();
        LoadSoundsByPath(GameConfig.RadioSoundsPath);
        LoadSoundsByPath(GameConfig.OSTSoundsPath);
        if (!string.IsNullOrEmpty(CurSoundInfo.FilePath))
        {
            //如果当前播放的音乐还存在，就刷新当前音乐的下标
            if (File.Exists(CurSoundInfo.FilePath))
            {
                CurSoundsIndex = SoundsInfoList.FindIndex(a => a.FilePath == CurSoundInfo.FilePath);
            }
            else
            {
                StopCurSound();
                CurSoundsIndex = -1;
            }
        }

    }

    public bool PlaySounds(int index)
    {
        StopCurSound();

        if (index < 0 || index >= SoundsInfoList.Count) return false;

        CurSoundInfo = SoundsInfoList[index];
        CurSoundsIndex = index;
        if (null != UpdateSelectItemEvent)
        {
            UpdateSelectItemEvent();
        }

        if (!File.Exists(CurSoundInfo.FilePath)) return false;

        if (m_NAudioSupportFormat.Contains(CurSoundInfo.ExtensionNotDot))
        {
            NAudioPlayer.SupportFormatType type= Convert2NAduioSFT(CurSoundInfo.ExtensionNotDot);
            if (type == NAudioPlayer.SupportFormatType.NULL) return false;
            StartCoroutine(LoadFileByNAudio(CurSoundInfo.FilePath, type));
            return true;
        }
        else if (m_UnitySupportFormat.Contains(CurSoundInfo.ExtensionNotDot))
        {
            StartCoroutine(LoadFileByUnity(CurSoundInfo.FilePath));
            return true;
        }
        else
        {
            Debug.Log("Does not support audio formats:"+ CurSoundInfo.FilePath);
            return false;
        }
    }

    public bool PlayDefaultSound()
    {
        if (null != SoundsInfoList && SoundsInfoList.Count > 0)
        {
            if (CurSoundsIndex < 0 || CurSoundsIndex >= SoundsInfoList.Count)
                CurSoundsIndex = 0;
            return PlaySounds(CurSoundsIndex);
        }
        return false;
    }

    public bool PlayNextSound()
    {
        if (null != SoundsInfoList && SoundsInfoList.Count > 0)
        {
            ++CurSoundsIndex;
            if (CurSoundsIndex < 0|| CurSoundsIndex >= SoundsInfoList.Count)
                CurSoundsIndex = 0;
            return PlaySounds(CurSoundsIndex);
        }
        return false;
    }

    public bool PlayPreviousSounds()
    {
        if (null != SoundsInfoList && SoundsInfoList.Count > 0)
        {
            --CurSoundsIndex;
            if (CurSoundsIndex < 0 || CurSoundsIndex >= SoundsInfoList.Count)
                CurSoundsIndex = SoundsInfoList.Count - 1;
            return PlaySounds(CurSoundsIndex);
        }
        return false;
    }

    public void PauseCurSound()
    {
        if (null != m_AudioSource && null != m_AudioSource.clip && m_AudioSource.isPlaying)
        {
            m_AudioSource.Pause();
            PlayState =  SoundPlayState.Pause;
        }
    }

    public void ContinueCurSound()
    {
        if (null != m_AudioSource && null != m_AudioSource.clip)
        {
            if (!m_AudioSource.isPlaying)
            {
                m_AudioSource.Play();
                PlayState =  SoundPlayState.Playing;
            }
        }
        else
        {
            PlayDefaultSound();
        }
    }

    public void StopCurSound()
    {
        if (null != m_AudioSource && null != m_AudioSource.clip && m_AudioSource.isPlaying)
        {
            m_AudioSource.Stop();
            SetTime(0);
            m_AudioSource.clip = null;
            PlayState = SoundPlayState.Stop;
        }
    }

    public void SetTime(float time)
    {
        if (null != m_AudioSource)
        {
            m_AudioSource.time = Mathf.Clamp(time,0,TotalTime);
        }
    }

    public void SetVolume(float volmue)
    {
        if (null != m_AudioSource)
        {
            m_AudioSource.volume = Mathf.Clamp01(volmue);
        }
    }

    public bool GetSoundData(float[] dataArray, int offsetSamples)
    {
        if (null == m_AudioSource || null==m_AudioSource.clip|| !m_AudioSource.clip.GetData(dataArray, offsetSamples))
        {            
            return false;
        }
        return true;
    }


    public void GetSpectrumData(float[] dataArray, int channel, FFTWindow fftWindow)
    {
        if (null == m_AudioSource||null==m_AudioSource.clip)
        {
            return;
        }
        m_AudioSource.GetSpectrumData(dataArray, channel, fftWindow);
    }

    public void GetOutputData(float[] dataArray, int channel)
    {
        if (null == m_AudioSource || null == m_AudioSource.clip)
        {
            return;
        }
        m_AudioSource.GetOutputData(dataArray, channel);
    }

    public void SetBgMusicState(bool state)
    {
        SystemSettingData.Instance.MusicVolume = state ? m_BackupBgMusicVolumeValue : 0f;
        OpenBgMusic = state;
    }

    #endregion

    #region private methods

    void LoadSoundsByPath(string path)
    {
        if (Directory.Exists(path))
        {
            string[] filePaths = Directory.GetFiles(path);
            if (null != filePaths && filePaths.Length > 0)
            {
                string tempStr;
                for (int i = 0; i < filePaths.Length; i++)
                {
                    tempStr = filePaths[i];
                    if (m_NAudioSupportFormat.Any(a => tempStr.ToLower().EndsWith("." + a.ToLower()))
                    || m_UnitySupportFormat.Any(b => tempStr.ToLower().EndsWith("." + b.ToLower())))
                    {
                        SoundsInfoList.Add(new RadioFileInfo(tempStr));
                    }
                }
            }
        }
    }

    IEnumerator LoadFileByNAudio(string filePath, NAudioPlayer.SupportFormatType type)
    {
        if (type != NAudioPlayer.SupportFormatType.NULL)
        {
            WWW www = new WWW("file://" + filePath);
            yield return www;
            if (null != www && null != www.bytes && www.bytes.Length > 0)
            {
                AudioClip clip = NAudioPlayer.GetClipByType(www.bytes, type);
                if (null != clip)
                {
                    while (clip.loadState == AudioDataLoadState.Loading)
                        yield return null;
                    if (clip.loadState == AudioDataLoadState.Loaded)
                    {
                        clip.name = Path.GetFileNameWithoutExtension(filePath);
                       
                    }
                }
                if (!PlaySounds(clip))
                {
                    MarkPlayErrorItem(filePath);
                }
            }
        }
    }

    IEnumerator LoadFileByUnity(string filePath)
    {
        WWW www = new WWW("file://" + filePath);
        yield return www;
        if (null != www)
        {
            AudioClip clip = www.audioClip;
            if (null != clip)
            {
                while (clip.loadState == AudioDataLoadState.Loading)
                    yield return null;
                if (clip.loadState == AudioDataLoadState.Loaded)
                {
                    clip.name = Path.GetFileNameWithoutExtension(filePath);
                    
                }
            }
            if (!PlaySounds(clip))
            {
                MarkPlayErrorItem(filePath);
            }
        }
    }

    void MarkPlayErrorItem(string path)
    {
        if (null == SoundsInfoList || null== path) return;
        for (int i = 0; i < SoundsInfoList.Count; i++)
        {
            if (string.Equals(SoundsInfoList[i].FilePath, path))
            {
                SoundsInfoList[i].SetPlayError(true);
                if (null != PlayErrorEvent)
                {
                    PlayErrorEvent(i);
                }
                break;
            }
        }
        
    }

    NAudioPlayer.SupportFormatType Convert2NAduioSFT(string extension)
    {
        switch (extension)
        {
            case "mp3":
                return NAudioPlayer.SupportFormatType.mp3;
            case "flac":
                return NAudioPlayer.SupportFormatType.flac;
            //case "wav":
            //    return NAudioPlayer.SupportFormatType.wav;
            //case "aiff":
            //    return NAudioPlayer.SupportFormatType.aiff;
            default:
                return NAudioPlayer.SupportFormatType.NULL;
        }
    }

    bool PlaySounds(AudioClip clip)
    {
        if (null != m_AudioSource)
        {
            if (null != clip)
            {
                if (null != m_AudioSource.clip)
                    m_AudioSource.Stop();
                m_AudioSource.clip = clip;
                m_AudioSource.Play();
                PlayState = SoundPlayState.Playing;

                return true;
            }
            else
            {
                PeTipMsg.Register(string.Format("{0}:{1}", CurSoundInfo.Name,PELocalization.GetString(9500106)),PeTipMsg.EMsgLevel.Error);
            }
        }
        m_SwitchSound = true;
        return false;
    }

    #endregion
}
