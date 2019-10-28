using System.Xml;

namespace ScenarioRTL
{
	namespace IO
	{
		public class StatementRaw
		{
			public string classname = "";
			public int order = 0;
			public ParamRaw parameters;

			public static StatementRaw Create (XmlElement elem)
			{
				int cnt = elem.Attributes.Count;
				bool has_order = elem.HasAttribute("order");

				StatementRaw raw = new StatementRaw ();
				raw.parameters = new ParamRaw(has_order ? cnt - 2 : cnt - 1);

				if (has_order)
				{
					for (int i = 0; i < cnt; ++i)
					{
						if (i == 0)
							raw.classname = elem.Attributes[i].Value;
						else if (i == 1)
							raw.order = XmlConvert.ToInt32(elem.Attributes[i].Value);
						else
							raw.parameters.Set(i - 2, elem.Attributes[i].Name, System.Uri.UnescapeDataString(elem.Attributes[i].Value));
					}
				}
				else
				{
					for (int i = 0; i < cnt; ++i)
					{
						if (i == 0)
							raw.classname = elem.Attributes[i].Value;
						else
							raw.parameters.Set(i - 1, elem.Attributes[i].Name, System.Uri.UnescapeDataString(elem.Attributes[i].Value));
					}
				}

				return raw;
			}

			public override string ToString ()
			{
				return string.Format ("StatementRaw " + classname);
			}
		}
	}
}