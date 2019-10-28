using UnityEngine;
using System.Collections;

namespace Pathea.Effect
{
	public class ControllableEffect 
	{
		GameObject m_EffectObj;
		
		public bool active
		{
			get { return (null != m_EffectObj) ? m_EffectObj.activeInHierarchy : false; }
			set { if(null != m_EffectObj) m_EffectObj.SetActive(value); }
		}
		
		public ControllableEffect(int effectID, Transform trnas)
		{
			Pathea.Effect.EffectBuilder.EffectRequest request =	Pathea.Effect.EffectBuilder.Instance.Register(effectID, null, trnas);
			request.SpawnEvent += OnEffectSpawn;
		}
		
		public void Destory()
		{
			if(null != m_EffectObj)
				GameObject.Destroy(m_EffectObj);
		}
		
		
		void OnEffectSpawn(GameObject gameObj)
		{
			m_EffectObj = gameObj;
		}
	}
}
