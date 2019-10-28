using UnityEngine;
using System.Collections;
using Pathea.Maths;

namespace RedGrass
{
	public abstract class Chunk32DataIO : ChunkDataIO 
	{
		protected INTVECTOR3 ChunkPosToPos32 (int x_index, int z_index)
		{
			return new INTVECTOR3(x_index * (mEvni.CHUNKSIZE >> 5), 0, z_index * (mEvni.CHUNKSIZE >> 5));
		}

		protected  int Pos32ToIndex32 (int x_32, int z_32)
		{
			return (x_32 - mEvni.XStart / 32) + (z_32 - mEvni.ZStart /32) * mEvni.XTileCount;
		}
	}
}
