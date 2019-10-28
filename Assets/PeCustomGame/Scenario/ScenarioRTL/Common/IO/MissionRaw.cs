using System.Xml;

namespace ScenarioRTL
{
	namespace IO
	{
		public class MissionRaw
		{
			public int id;
			public string name;
			public TriggerRaw[] triggers;
			public ParamRaw properties;

			public static MissionRaw Create (string xmlpath)
			{
				XmlDocument doc = new XmlDocument ();
				doc.Load(xmlpath);
				XmlElement elem = doc.DocumentElement;

				int cnt = elem.Attributes.Count;
				MissionRaw raw = new MissionRaw ();
				raw.properties = new ParamRaw(cnt - 2);
				
				for (int i = 0; i < cnt; ++i)
				{
					if (i == 0)
						raw.id = XmlConvert.ToInt32(elem.Attributes[i].Value);
					else if (i == 1)
						raw.name = System.Uri.UnescapeDataString(elem.Attributes[i].Value);
					else
						raw.properties.Set(i - 2, elem.Attributes[i].Name, System.Uri.UnescapeDataString(elem.Attributes[i].Value));
				}

				int tcnt = elem.ChildNodes.Count;
				raw.triggers = new TriggerRaw[tcnt];
				for (int i = 0; i < tcnt; ++i)
					raw.triggers[i] = TriggerRaw.Create(elem.ChildNodes[i] as XmlElement);

				return raw;
			}

			public override string ToString ()
			{
				return string.Format ("MissionRaw " + name);
			}
		}
	}
}