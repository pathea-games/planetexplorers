using System.IO;
using System.Collections.Generic;

namespace ScenarioRTL
{
	public class VarCollection<TKey>
	{
		private Dictionary<TKey, VarScope> m_Collection;

		public VarCollection ()
		{
			m_Collection = new Dictionary<TKey, VarScope>();
		}

		public VarScope this [TKey key]
		{
			get
			{
				if (key == null)
					return null;
				if (m_Collection.ContainsKey(key))
					return m_Collection[key];
				return null;
			}
			set
			{
				if (key == null)
					return;
				if (value != null)
				{
					m_Collection[key] = value;
				}
				else
				{
					if (m_Collection.ContainsKey(key) && m_Collection[key] != null)
					{
						m_Collection[key].Clear();
						m_Collection.Remove(key);
					}
				}
			}
		}
	}
}