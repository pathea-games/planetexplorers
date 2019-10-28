using UnityEngine;
using System.Collections;

public class PEAudioPrefab : MonoBehaviour
{
    public int audioID;

    AudioController m_Audio;

	void Start () {
        if (audioID > 0)
            m_Audio = AudioManager.instance.Create(transform.position, audioID, transform, true, false);
	}
	
	void OnDestroy () {
        if (m_Audio != null)
            m_Audio.Delete();
	}
}
