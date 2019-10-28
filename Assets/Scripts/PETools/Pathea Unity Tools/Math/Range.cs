using UnityEngine;

namespace Pathea
{
	namespace Maths
	{
		public struct Range1D
		{
			private float m_Center;
			private float m_Extend;
			public float center
			{
				get { return m_Center; }
				set { m_Center = value; }
			}
			public float extend
			{
				get { return m_Extend; }
				set { m_Extend = value; }
			}
			public float min { get { return m_Center - m_Extend; } }
			public float max { get { return m_Center + m_Extend; } }

			// Set range min and max
			public void SetMinMax (float _min, float _max)
			{
				m_Center = (_min + _max) * 0.5f;
				m_Extend = Mathf.Abs((_max - _min) * 0.5f);
			}
			// Merge other Range1D into this Range1D
			public void Merge (Range1D other)
			{
				SetMinMax(Mathf.Min(this.min, other.min), Mathf.Max(this.max, other.max));
			}
		}
	}
}
