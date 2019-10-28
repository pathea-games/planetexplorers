using UnityEngine;

namespace Pathea
{
	public static class Utils
	{
		public static Transform GetChild (Transform parent, string childName)
		{
			if (childName == "")
				return null;
			
			foreach (Transform it in parent)
			{
				if (it.name.Equals(childName))
					return it;
				else
				{
					Transform child = GetChild(it, childName);
					if (child != null)
					{
						return child;
					}
				}
			}
			return null;
		}
	}
}
