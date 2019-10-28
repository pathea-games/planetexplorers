using UnityEngine;
using System;
using System.IO;
using System.Xml.Serialization;

namespace NovaEnv
{
	[Serializable]
	public abstract class Theme
	{
		public string Name;
		public float Weight;

		public abstract void Execute (Executor executor);

#if false
		public bool Serialize = false;
		public bool Deserialize = false;

		public static void SerializeToFile (Theme theme)
		{
			using (FileStream fs = new FileStream ("D:/" + theme.Name + ".xml", FileMode.Create))
			{
				XmlSerializer xmls = new XmlSerializer (theme.GetType());
				xmls.Serialize(fs, theme);
			}
		}
		
		public static Theme DeserializeToFile (Type t, string name)
		{
			using (FileStream fs = new FileStream ("D:/" + name + ".xml", FileMode.Open))
			{
				XmlSerializer xmls = new XmlSerializer (t);
				return xmls.Deserialize(fs) as Theme;
			}
		}
#endif

		public static Color Evaluate (Gradient g, float sunHeight, float ap)
		{
			if (ap <= 0.001f)
				return g.Evaluate(sunHeight);
			else if (ap >= 0.999f)
				return g.Evaluate(1F - sunHeight);
			else
				return Color.Lerp(g.Evaluate(sunHeight), g.Evaluate(1F - sunHeight), ap);
		}
		public static float Evaluate (AnimationCurve c, float sunHeight, float ap)
		{
			if (ap <= 0.001f)
				return c.Evaluate(sunHeight);
			else if (ap >= 0.999f)
				return c.Evaluate(1F - sunHeight);
			else
				return Mathf.Lerp(c.Evaluate(sunHeight), c.Evaluate(1F - sunHeight), ap);
		}
	}
}