using UnityEngine;
using System.Collections;

public class EffectSoundCtrl : MonoBehaviour
{
	// Update is called once per frame
	void Update ()
	{
		if(GetComponent<AudioSource>())
			GetComponent<AudioSource>().volume = SystemSettingData.Instance.EffectVolume * SystemSettingData.Instance.SoundVolume;
	}
}
