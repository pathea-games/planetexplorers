using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Pathea.Operate
{
	public class PEPatients : Operation_Multiple 
	{
		public PELay[] Lays;
		
		public override List<Operation_Single> Singles
		{
			get { return new List<Operation_Single>(Lays); }
		}
	}
}
