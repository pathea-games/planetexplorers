using UnityEngine;
using System.Collections;
using Pathea.Maths;

namespace RedGrass
{
	public static class Utils  
	{
		public static int PosToIndex (int x_index, int z_index)
		{
			return (((x_index+16384) & 32767) << 15) | ((z_index+16384) & 32767);
		}

		public static INTVECTOR3 IndexToPos (int index)
		{
			return new INTVECTOR3 ((index >> 15) - 16384, 0, (index & 32767) - 16384);
		}

		public static INTVECTOR3 FloorVec3 (Vector3 vec)
		{
			return new INTVECTOR3(Mathf.FloorToInt(vec.x), Mathf.FloorToInt(vec.y), Mathf.FloorToInt(vec.z));
		}

		public static INTVECTOR3 WorldPosToVoxelPos (Vector3 wpos)
		{
			return new INTVECTOR3((int)(wpos.x), (int)wpos.y, (int)wpos.z);
		}
	}
}
