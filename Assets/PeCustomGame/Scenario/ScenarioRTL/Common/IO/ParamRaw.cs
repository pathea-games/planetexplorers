
namespace ScenarioRTL
{
	namespace IO
	{
		public class ParamRaw
		{
			public ParamRaw (int cnt) { count = cnt; }

			private string [] names;
			private string [] values;

			public int count
			{
				get { return names.Length; }
				set
				{
					int cnt = value > 0 ? value : 0;
					names = new string[cnt];
					values = new string[cnt];
				}
			}

			public string this [int index] { get { return values[index]; } }
			public string this [string pname]
			{
				get
				{
					for (int i = 0; i < names.Length; ++i)
					{
						if (names[i] == pname)
							return values[i];
					}
					return "";
				}
			}

			public string GetName (int index) { return names[index]; }
			public string GetValue (int index) { return values[index]; }
			public void Set (int index, string name, string value)
			{
				names[index] = name;
				values[index] = value;
			}

			public override string ToString ()
			{
				string s = "";
				int i = 0;
				for (; i < 1 && i < count; ++i)
				{
					s += names[i] + " = " + values[i];
					if (i != count - 1)
						s += "; ";
				}
				if (i < count)
					s += "...";
				return s;
			}
		}
	}
}