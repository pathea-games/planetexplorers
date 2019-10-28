using System.Collections.Generic;
using System.Xml;

namespace ScenarioRTL
{
	namespace IO
	{
		public class TriggerRaw
		{
			public string name;
			public int repeat;
			public bool multiThreaded;
			public string owner;
			public StatementRaw[] events;
			public List<StatementRaw[]> conditions;
			public List<StatementRaw[]> actions;

			public static TriggerRaw Create (XmlElement elem)
			{
				TriggerRaw raw = new TriggerRaw ();
				raw.name = System.Uri.UnescapeDataString(elem.Attributes["name"].Value);
				raw.repeat = XmlConvert.ToInt32(elem.Attributes["repeat"].Value);
				if (elem.HasAttribute("multi_threaded"))
					raw.multiThreaded = (elem.Attributes["multi_threaded"].Value.ToLower()) == "true";
				else
					raw.multiThreaded = false;
				if (elem.HasAttribute("owner"))
					raw.owner = System.Uri.UnescapeDataString(elem.Attributes["owner"].Value);
				else
					raw.owner = "-";

				XmlElement e_elem = elem["EVENTS"];
				XmlElement c_elem = elem["CONDITIONS"];
				XmlElement a_elem = elem["ACTIONS"];

				int ecnt = e_elem.ChildNodes.Count;
				raw.events = new StatementRaw[ecnt];
				for (int i = 0; i < ecnt; ++i)
					raw.events[i] = StatementRaw.Create(e_elem.ChildNodes[i] as XmlElement);

				int g_cnt = c_elem.ChildNodes.Count;
				raw.conditions = new List<StatementRaw[]> ();
				for (int i = 0; i < g_cnt; ++i)
				{
					XmlElement g_elem = c_elem.ChildNodes[i] as XmlElement;
					int cnt = g_elem.ChildNodes.Count;
					StatementRaw[] stmts = new StatementRaw[cnt];
					for (int j = 0; j < cnt; ++j)
						stmts[j] = StatementRaw.Create(g_elem.ChildNodes[j] as XmlElement);
					raw.conditions.Add(stmts);
				}
				
				g_cnt = a_elem.ChildNodes.Count;
				raw.actions = new List<StatementRaw[]> ();
				for (int i = 0; i < g_cnt; ++i)
				{
					XmlElement g_elem = a_elem.ChildNodes[i] as XmlElement;
					int cnt = g_elem.ChildNodes.Count;
					StatementRaw[] stmts = new StatementRaw[cnt];
					for (int j = 0; j < cnt; ++j)
						stmts[j] = StatementRaw.Create(g_elem.ChildNodes[j] as XmlElement);
					raw.actions.Add(stmts);
				}
				
				return raw;
			}

			public override string ToString ()
			{
				return string.Format ("TriggerRaw " + name + " " + repeat + "x");
			}
		}
	}
}