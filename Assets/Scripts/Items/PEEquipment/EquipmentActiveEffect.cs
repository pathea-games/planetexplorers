using UnityEngine;
using System.Collections;
using Pathea.Effect;

public class EquipmentActiveEffect : MonoBehaviour 
{
	[SerializeField] int m_SoundID;
	[SerializeField] int m_EffectID;

	AudioController m_Audio;
	ControllableEffect m_Effect;

	public void SetActiveState(bool active)
	{
		if(active)
		{
			if(null == m_Audio)
			{
				m_Audio = AudioManager.instance.Create(transform.position, m_SoundID, transform, true, false);
				if(null != m_Audio)
				{
					m_Audio.SetVolume(0);
					m_Audio.SetVolume(1, 0.5f);
				}
			}
			
			if(null == m_Effect)
			{
				m_Effect = new ControllableEffect(m_EffectID, transform);
			}
		}
		else
		{
			if(null != m_Audio)
			{
				m_Audio.StopAudio(1f);
				m_Audio.Delete(1.1f);
				m_Audio = null;
			}
			
			if(null != m_Effect)
			{
				m_Effect.Destory();
				m_Effect = null;
			}
		}
	}

	void OnDestroy()
	{
		if(null != m_Effect)
		{
			m_Effect.Destory();
			m_Effect = null;
		}
	}
}
