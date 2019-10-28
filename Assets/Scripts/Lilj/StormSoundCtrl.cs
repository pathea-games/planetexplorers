using UnityEngine;
using System.Collections;

public class StormSoundCtrl : MonoBehaviour 
{
	// Update is called once per frame
	void Update () {
		if(GetComponent<AudioSource>() != null)
			GetComponent<AudioSource>().volume = SystemSettingData.Instance.EffectVolume * SystemSettingData.Instance.SoundVolume;
	}
}
