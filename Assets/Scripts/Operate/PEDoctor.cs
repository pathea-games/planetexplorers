using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Pathea.Operate
{
	public class PEDoctor : Operation_Multiple
	{
		public PECure[] Doctors;
		
		public override List<Operation_Single> Singles
		{
			get { return new List<Operation_Single>(Doctors); }
		}

	}
}