using UnityEngine;
using System;
using System.Collections;

namespace RedGrass
{
	[Serializable]
	public class RGPrototype
	{
		public RGPrototype ()
		{
			m_Diffuse = null;
			m_Particle = null;
			m_ParticleTintColor = Color.white;
			m_MinSize = Vector2.one;
			m_MaxSize = Vector2.one;
			m_BendFactor = 0.2f;
			m_LODBias = 0;
		}
		
		[SerializeField] public Texture2D m_Diffuse;
		[SerializeField] public Texture2D m_Particle;
		[SerializeField] public Color m_ParticleTintColor;
		[SerializeField] public Vector2 m_MinSize;
		[SerializeField] public Vector2 m_MaxSize;
		[SerializeField] public float m_BendFactor;
		[SerializeField] public float m_LODBias;
	}
}
