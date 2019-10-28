using System.IO;
using System.Collections.Generic;

namespace ScenarioRTL
{
	public class VarScope
	{
		private VarScope m_ParentScope = null;
		private Dictionary<string, Var> m_Vars = null;

		public VarScope ()
		{
			m_ParentScope = null;
			m_Vars = new Dictionary<string, Var>();
		}

		private VarScope (VarScope parent_scope)
		{
			m_ParentScope = parent_scope;
			m_Vars = new Dictionary<string, Var>();
		}

		public Var this [string varname]
		{
			get
			{
				if (m_Vars.ContainsKey(varname))
					return m_Vars[varname];
				else if (m_ParentScope != null)
					return m_ParentScope[varname];
				else
					return Var.zero;
			}
			set
			{
				m_Vars[varname] = value;
			}
		}

		public bool VarDeclared (string varname)
		{
			return m_Vars.ContainsKey(varname);
		}

		public VarScope CreateChild ()
		{
			return new VarScope(this);
		}

		public void Clear ()
		{
			m_Vars.Clear();
		}

		public string[] declaredVars
		{
			get
			{
				string[] keys = new string[m_Vars.Count];
				m_Vars.Keys.CopyTo(keys, 0);
				return keys;
			}
		}

		public void Import (BinaryReader r)
		{
			Clear();
			int count = r.ReadInt32();
			for (int i = 0 ; i < count ; ++i )
			{
				string name = r.ReadString();
				string value = r.ReadString();
				Var v = new Var();
				v.data = value;
				m_Vars[name] = v;
			}
		}

		public void Export (BinaryWriter w)
		{
			w.Write((int)m_Vars.Count);
			foreach (var kvp in m_Vars)
			{
				w.Write(kvp.Key);
				w.Write(kvp.Value.data);
			}
		}
	}
}