using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace RedGrass
{
	// chunk size enum
	public enum EChunkSize : int
	{
		SIZE_32,
		SIZE_64,
		SIZE_128,
		SIZE_256,
		SIZE_512
	}
	
	// Lod size
	public enum ELodType
	{
		LOD_1_TYPE_1 = 0,		
		LOD_1_TYPE_2,
		LOD_2_TYPE_1,
		LOD_2_TYPE_2,
		LOD_3_TYPE_1,
		LOD_3_TYPE_2,
		LOD_4_TYPE_1,
		Max
	}

	public class EvniAsset : ScriptableObject
	{
		// File
		public int Tile = 32;
		public int XStart = -3072;
		public int ZStart = -3072;
		public int XTileCount  = 192;
		public int ZTileCount  = 192;
		
		public int XZTileCount = 0;
		public int XEnd = 0;
		public int ZEnd = 0;
		
		public int FileXCount = 3;
		public int FlieZCount = 3;

		[HideInInspector]public int FileXZcount = 0;


		/// <summary>
		/// size of each chunks	data chunk and render chunk
		/// </summary>
		[SerializeField] public EChunkSize ChunkSize = EChunkSize.SIZE_32;

		[SerializeField] int _chunkShift = 5;
		[SerializeField] int _chunkMask = 31;
		[SerializeField] int _chunkSize = 0;

		public int SHIFT 	{ get { return _chunkShift; } }
		public int MASK 	{ get { return _chunkMask; } }
		public int CHUNKSIZE  { get { return _chunkSize; } }

		/// <summary>
		/// The type of the LOD.
		/// </summary>
		public ELodType LODType;

		[SerializeField] int _maxLOD = 0;
		public int MaxLOD  { get { return _maxLOD;}}

		[SerializeField] int[] _LODExpandNums = null;
		public int[] LODExpandNum  { get { return _LODExpandNums;} }

		[SerializeField] int _dataExpandNums = 0;
		public int DataExpandNum { get { return _dataExpandNums; } }

		public float[] LODDensities = new float[0];

		/// <summary>
		/// 生成Mesh时 每一个最大Quad 数量
		/// </summary>
		public int MeshQuadMaxCount = 6000;

		/// <summary>
		/// The density of each voxel grass
		/// </summary>
		public float Density = 1.0f;


		#region SET
		public void SetLODType (ELodType type)
		{
			LODType = type;
			CalcLODExtraVars();
		}

		public void SetDensity (float density)
		{
			Density = density;
		}

		public void SetChunkSize (EChunkSize chunk_size)
		{
			ChunkSize = chunk_size;
			CalcExtraChunkSizeVars();
		}
		#endregion

		#region EDITOR_FUNC
		public void CalcExtraTileVars ()
		{
			XZTileCount = XTileCount * ZTileCount;
			XEnd = XStart + XTileCount * Tile;
			ZEnd = ZStart + ZTileCount * Tile;
		}

		public void CalcExtraFileVars ()
		{
			FileXZcount = FileXCount * FlieZCount;
		}

		public void CalcExtraChunkSizeVars ()
		{
			_chunkSize = 1 << ((int)ChunkSize + 5) ;
			_chunkShift = (int)ChunkSize + 5;
			_chunkMask = (1 << _chunkShift) - 1;
		}

		public string GetLodTypeDesc (ELodType lod)
		{
			string desc = "";


			desc += "Max Lod : 1  " ;

			for (int i = 0 ; i <= MaxLOD; i++)
			{
				if (i % 3 == 0)
					desc += "\r\n Lod " + i.ToString() + " expand num : " + _LODExpandNums[i].ToString() +" ;     "; 
				else
					desc += "Lod " + i.ToString() + " expand num : " + _LODExpandNums[i].ToString() +" ;     "; 
			}


			return desc;
		}

		public void CalcLODExtraVars ()
		{
			_maxLOD = GetMaxLod();

			_LODExpandNums = new int[_maxLOD + 1];

			for (int i = 0; i <= _maxLOD; i++)
				_LODExpandNums[i] = GetLODNumExpands(i);


			if (MaxLOD != LODDensities.Length - 1)
			{
				LODDensities = new float[MaxLOD + 1];
				for (int i = 0; i <= MaxLOD; i++)
				{
					LODDensities[i] = 1.0f / (1<<i); 
				}
			}


			_dataExpandNums = (((CHUNKSIZE << MaxLOD) * _LODExpandNums[MaxLOD] + (CHUNKSIZE << MaxLOD)) >> SHIFT) - 1;
//			switch (LODType)
//			{
//			case ELodType.LOD_1_TYPE_1:
//				_dataExpandNums = 10;
//				break;
//			case ELodType.LOD_1_TYPE_2:
//				_dataExpandNums = 10;
//				break;
//			case ELodType.LOD_2_TYPE_1:
//				_dataExpandNums = 12;
//				break;
//			case ELodType.LOD_2_TYPE_2:
//				_dataExpandNums = 16;
//				break;
//			case ELodType.LOD_3_TYPE_1:
//				_dataExpandNums = 23;
//				break;
//			case ELodType.LOD_3_TYPE_2:
//				_dataExpandNums = 31;
//				break;
//			case ELodType.LOD_4_TYPE_1:
//				_dataExpandNums = 42;
//				break;
//			default:
//				break;
//			}




		}

		#endregion


		int GetMaxLod ()
		{
			switch (LODType)
			{
			case ELodType.LOD_1_TYPE_1:
				return 0;
			case ELodType.LOD_1_TYPE_2:
				return 1;
			case ELodType.LOD_2_TYPE_1:
				return 2;
			case ELodType.LOD_2_TYPE_2:
				return 2;
			case ELodType.LOD_3_TYPE_1:
				return 3;
			case ELodType.LOD_3_TYPE_2:
				return 3;
			case ELodType.LOD_4_TYPE_1:
				return 4;
			}
			
			return -1;
		}

		int GetLODNumExpands(int LOD)
		{
			switch (LODType)
			{
			case ELodType.LOD_1_TYPE_1:
			{
				if (LOD == 0)
					return 3;
				else if (LOD == 1)
					return 2;
			}break;
			case ELodType.LOD_1_TYPE_2:
			{
				if (LOD == 0)
					return 4;
				else if (LOD == 1)
					return 4;
			}break;
			case ELodType.LOD_2_TYPE_1:
			{
				if (LOD == 0)
					return 2;
				else if (LOD == 1)
					return 2;
				else if (LOD == 2)
					return 2;
			}break;
			case ELodType.LOD_2_TYPE_2:
			{
				if (LOD == 0)
					return 3;
				else if (LOD == 1)
					return 3;
				else if (LOD == 2)
					return 3;
			}break;
			case ELodType.LOD_3_TYPE_1:
			{
				if (LOD == 0)
					return 2;
				else if (LOD == 1)
					return 2;
				else if (LOD == 2)
					return 2;
				else if (LOD == 3)
					return 2;
			}break;
			case ELodType.LOD_3_TYPE_2:
			{
				if (LOD == 0)
					return 3;
				else if (LOD == 1)
					return 3;
				else if (LOD == 2)
					return 3;
				else if (LOD == 3)
					return 3;
			}break;
			case ELodType.LOD_4_TYPE_1:
			{
				if (LOD == 0)
					return 2;
				else if (LOD == 1)
					return 2;
				else if (LOD == 2)
					return 2;
				else if (LOD == 3)
					return 2;
				else if (LOD == 4)
					return 2;
			}break;
			}
			
			return -1;
		}

	}
}

