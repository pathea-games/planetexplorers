using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using SoundAsset;

public enum AudioType
{
    Null,
    Music,
    Dialog,
    Effect,
    UI,
    Max
}

[RequireComponent(typeof(AudioSource))]
public class AudioController : MonoBehaviour 
{
	const string c_defAuName = "AuPool";
	static Stack<AudioController> _audioPool = new Stack<AudioController> ();
	public static void InitPool(){
		_audioPool.Clear ();
	}
	public static AudioController GetAudio(SESoundBuff buff, Vector3 pos, Transform parent, bool bPlay, bool bAutoDel)
	{
		AudioController au = null;
		if (_audioPool.Count > 0) {	au = _audioPool.Pop ();	} 
		if( au == null){ //au might be destroyed in somewhere.
			GameObject obj = new GameObject ();		
			au = obj.AddComponent<AudioController> ();
		}
		au.InitData (buff, bAutoDel);
		au.name = "Au" + buff.mID;		
		au.transform.position = pos;
		au.transform.parent = parent;
		
		if (bPlay)			au.PlayAudio();
		return au;
	}
	public static void FreeAudio(AudioController au)
	{
        if (!_audioPool.Contains(au))
        {
            au.mAudio.Stop();

            if (au.DestroyEvent != null)
                au.DestroyEvent(au);

            au.name = c_defAuName;
            au.mClipName = "";
            au.mAutoDel = false;
            au.mAudio.clip = null;
            au.StopAllCoroutines();
            au.CancelInvoke();
            au.transform.parent = AudioManager.instance.transform;

            if (!au.gameObject.activeSelf)
                au.gameObject.SetActive(true);

            _audioPool.Push(au);
        }
	}

	float mVolume;
    float mVolumeSpeed;
    float mVolumeStart;
    float mVolumeStartTime;
    float mVolumeTarget;
    string mClipName;
    bool mAutoDel;
	internal float mVolumeScale;
    internal AudioType mType = AudioType.Null;
    internal AudioSource mAudio;
	public Action<AudioController> DestroyEvent;

	float volumeScale
	{
		get
		{
            if (mType == AudioType.Null)					return 1.0f;
            else if (mType == AudioType.Music)				return SystemSettingData.Instance.SoundVolume * SystemSettingData.Instance.MusicVolume;
            else if (mType == AudioType.Dialog)				return SystemSettingData.Instance.SoundVolume * SystemSettingData.Instance.DialogVolume;
            else if (mType == AudioType.Effect)				return SystemSettingData.Instance.SoundVolume * SystemSettingData.Instance.EffectVolume;
            else if (mType == AudioType.UI)				    return SystemSettingData.Instance.SoundVolume * SystemSettingData.Instance.EffectVolume;
			else											return 1.0f;
		}
	}

    public bool autoDel { set { mAutoDel = value; } }

    public bool isPlaying		{        get { return mAudio != null ? mAudio.isPlaying : false; 	}    }
    public float volumeTarget	{        get { return mVolumeTarget; 		}    }
    public float volume			{        get { return mAudio != null ? mAudio.volume : 0.0f; 		}    }
    public float time			{        get { return mAudio != null ? mAudio.time : 0.0f; 			}    }
    public float length			{        get { return mAudio != null && mAudio.clip != null ? mAudio.clip.length : 0.0f; 	}    }

	public void OnUpdate()
	{
		if (mVolumeSpeed <= PETools.PEMath.Epsilon)
			mVolume = Mathf.Clamp01(mVolumeTarget);
		else
			mVolume = Mathf.Clamp01(Mathf.Lerp(mVolumeStart, mVolumeTarget, (Time.time - mVolumeStartTime) / mVolumeSpeed));
		
		mVolume = (Pathea.PeGameMgr.gamePause && mType != AudioType.UI) ? 0.0f : mVolume;
		mAudio.volume = mVolume * mVolumeScale * volumeScale;
	}

    public void SetVolume(float targetVolume, float delayTime = 0.0f)
    {
        if (Mathf.Abs(mVolumeTarget - targetVolume) > PETools.PEMath.Epsilon)
        {
            mVolumeStart = mVolume;
            mVolumeStartTime = Time.time;
            mVolumeSpeed = delayTime;
            mVolumeTarget = targetVolume;
        }
    }

    public void PlayAudio(float delayTime = 0.0f)
	{
        SetVolume(1.0f, delayTime);

        CancelInvoke();

        StartCoroutine(PlayAudioEnumerator());
    }

    public void PauseAudio(float delayTime = 0.0f)
    {
        SetVolume(0.0f, delayTime);

        if (mAudio != null && mAudio.isPlaying && !IsInvoking("Pause"))
            Invoke("Pause", delayTime);
    }
    public void StopAudio(float delayTime = 0.0f)
	{
        SetVolume(0.0f, delayTime);

        if (mAudio != null && mAudio.isPlaying && !IsInvoking("Stop"))
            Invoke("Stop", delayTime);
	}
	public void Delete(float delayTime = 0.0f)
	{
		if (delayTime < 0.01f) {
			Free ();
		} else {
            if (!IsInvoking("Free"))
			    Invoke ("Free", delayTime);
		}
	}

    IEnumerator LoadClip()
    {
        mAudio.clip = null;

        while (mAudio.clip == null)
        {
            mAudio.clip = AudioManager.instance.GetAudioClip(mClipName);
            yield return null;
        }
    }

    IEnumerator PlayAudioEnumerator()
    {
        while (mAudio.clip == null)
            yield return null;

        if (!mAudio.isPlaying)
            mAudio.Play();

        if (mAutoDel)
        {
            yield return new WaitForSeconds(mAudio.clip.length + 1f);

            while (mAudio.isPlaying)
                yield return null;

            Delete();
        }
    }

	// Funx for invoke
    void Stop() { if (mAudio != null)  mAudio.Stop();    }
    void Pause(){ if (mAudio != null) mAudio.Pause();   }
	void Free() { if (mAudio != null) FreeAudio (this); }

	void InitData(SESoundBuff buff, bool isAutoDel)
	{
        if(mAudio == null)
		    mAudio = GetComponent<AudioSource>();

        if (mAudio == null)
            mAudio = gameObject.AddComponent<AudioSource>();

        mAutoDel = isAutoDel;
        mClipName = buff.mName;
		mType = (AudioType)buff.mAudioType;
        mVolumeScale = buff.mVolume;

        if (mAudio != null)
        {
            mAudio.loop = buff.mLoop;
            mAudio.dopplerLevel = buff.mDoppler;
            mAudio.spatialBlend = buff.mSpatial;
            mAudio.rolloffMode = buff.mMode;
            mAudio.minDistance = buff.mMinDistance;
            mAudio.maxDistance = buff.mMaxDistance;
            mAudio.playOnAwake = false;
        }

        CancelInvoke();
        StopAllCoroutines();

        StartCoroutine(LoadClip());
	}    
}
