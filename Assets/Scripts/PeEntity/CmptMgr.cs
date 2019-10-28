using UnityEngine;
using System.Collections.Generic;
using SkillSystem;
using PETools;

namespace Pathea
{
	public class CmptMgr: Pathea.MonoLikeSingleton<CmptMgr>
	{
		List<PeCmpt> _cmpts = new List<PeCmpt>();

		public override void Update()
		{
			int n = _cmpts.Count;
			for (int i = n - 1; i >= 0; i--) {
				PeCmpt cmpt = _cmpts[i];
				if(cmpt != null && !cmpt.Equals(null))
				{
					if(cmpt.isActiveAndEnabled) cmpt.OnUpdate();
				} else {
					_cmpts.RemoveAt(i);
				}
			}
		}

		public void AddCmpt(PeCmpt cmpt)
		{
			_cmpts.Add (cmpt);
		}
		public void RemoveCmpt(PeCmpt cmpt)
		{
			_cmpts.Remove (cmpt);
		}
	}
}