using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SoundAsset;

public class AudioManager : MonoBehaviour 
{
    static AudioManager _instance;

    public static AudioManager instance
    {
        get 
        {
            if (_instance == null)
            {
                GameObject obj = new GameObject("AudioManager");
                _instance = obj.AddComponent<AudioManager>();
				AudioController.InitPool();
            }
            return _instance;
        }
    }

	static Dictionary<string, AudioClip> s_AudioClipCaches = new Dictionary<string, AudioClip>();

	public List<AudioController> m_allAudios = new List<AudioController>();
	public List<AudioController> m_animFootStepAudios = new List<AudioController>();

    public AudioController CreateFootStepAudio( Vector3 position, 
                                                int clipId, 
                                                Transform parent = null, 
                                                bool isPlay = true, 
                                                bool isDelete = true)
    {
        AudioController footAudio = Create(position, clipId, parent, isPlay, isDelete);
        if(footAudio != null)
        {
            footAudio.DestroyEvent += OnDeleteFoot;
            m_animFootStepAudios.Add(footAudio);
        }
        return footAudio;
    }

    public AudioController Create(  Vector3 position, 
                                    int clipId, 
                                    Transform parent = null, 
                                    bool isPlay = true, 
                                    bool isDelete = true)
    {
        if (clipId <= 0)
            return null;

        SESoundBuff buff = SESoundBuff.GetSESoundData(clipId);
        if (buff == null){
            Debug.LogError("Can't find sound : " + clipId);
            return null;
        }

		AudioController au = AudioController.GetAudio (buff, position, parent ? parent : transform, isPlay, isDelete);
		au.DestroyEvent += OnDelete;
		m_allAudios.Add(au);
		return au;
    }

    public AudioClip GetAudioClip(string clipName)
    {
        AudioClip clip = null;
        if (s_AudioClipCaches.TryGetValue(clipName, out clip))
            return clip;
        else
        {
            _instance.StartCoroutine(LoadAudio(clipName));
            return null;
        }
    }

    IEnumerator LoadAudio(string clipName)
    {
        s_AudioClipCaches[clipName] = null;
        ResourceRequest rr = Resources.LoadAsync<AudioClip>("Sound/" + clipName);

        while (true)
        {
            if (rr.isDone)
            {
                AudioClip clip = rr.asset as AudioClip;

                if (clip != null && clip.loadState == AudioDataLoadState.Loaded)
                {
                    s_AudioClipCaches[clipName] = clip;
                    yield break;
                }
            }
            
            yield return null;
        }
    }

    void OnDeleteFoot(AudioController audioCtrl)
    {
        audioCtrl.DestroyEvent -= OnDeleteFoot;
        m_animFootStepAudios.Remove(audioCtrl);
    }

    void OnDelete(AudioController audioCtrl)
    {
        audioCtrl.DestroyEvent -= OnDelete;
        m_allAudios.Remove(audioCtrl);
    }

	void Update()
	{
		int n = m_allAudios.Count;
		for (int i = n-1; i >= 0; i--) {
			if(m_allAudios[i]){		m_allAudios[i].OnUpdate(); }
			else { 					m_allAudios.RemoveAt(i);   }
		}
	}
}
