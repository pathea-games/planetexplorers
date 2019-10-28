using UnityEngine;
using System.Collections;

namespace Pathea
{
	public enum EQuadrant
	{
		None,
		Q1,
		Q2,
		Q3,
		Q4
	}
	public   class  Quadrant
	{
		public static EQuadrant GetQuadrant(Vector3 v3)
		{
			int x;
			int z;
			
			EQuadrant Q = EQuadrant.None;
			if(v3.x == 0)
			{
				if(v3.z == 0)
				{
					x=1;
					z=1;
				}
				else
				{
					z=(int)(v3.z/Mathf.Abs(v3.z));
					x= z;
				}
			}
			else
			{
				if(v3.z == 0)
				{
					x=(int)(v3.x/Mathf.Abs(v3.x));
					z=x;
				}
				else
				{
					x=(int)(v3.x/Mathf.Abs(v3.x));
					z=(int)(v3.z/Mathf.Abs(v3.z));
				}
				
			}
			
			if(x >0)
			{
				if(z >0)
				{
					Q = EQuadrant.Q1; 
				}
				else
				{
					Q = EQuadrant.Q4;
				}
			}
			else
			{
				if(z >0)
				{
					Q = EQuadrant.Q2; 
				}
				else
				{
					Q = EQuadrant.Q3;
				}
			}
			return Q;
		}
		
		public static EQuadrant Add(EQuadrant q)
		{
			int n = (int)q + 1;
			if(n > 4)
				return EQuadrant.Q1;
			
			return (EQuadrant)n;
		}
		
		public static EQuadrant Minus(EQuadrant q)
		{
			int n = (int)q - 1;
			if(n <= 0)
				return EQuadrant.Q4;
			
			return (EQuadrant)n;
		}
	}
}

