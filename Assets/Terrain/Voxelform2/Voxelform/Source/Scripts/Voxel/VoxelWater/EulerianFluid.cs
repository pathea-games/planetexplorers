using UnityEngine;
using System;
using System.Collections.Generic;
public enum EulerianMatrix
{
	None		= 0,
	Bottom		= 1,
	Left		= 2,
	Right		= 4,
	Forward		= 8,
	Backward	= 16,
}
public class EulerianFluidProcessor
{
	private byte _tmpTerVolume = 0;
	private byte[] _tmpTerDataVT = new byte[VoxelTerrainConstants.VOXEL_ARRAY_LENGTH_VT];
	private VFVoxelChunkData[] _tmpNeibourChunks = new VFVoxelChunkData[27];
	private VFVoxelChunkData[] _tmpDirtyNeibourChunks = new VFVoxelChunkData[27];
	private List<IntVector4> _tmpDirtyChunkPosList = new List<IntVector4>();
	private List<IntVector4> _dirtyChunkPosList = new List<IntVector4>();
	private List<VFVoxelChunkData> _tmpDirtyChunks = new List<VFVoxelChunkData>();
	//private HashSet<IntVector4> _allDirtyChunkPosList = new HashSet<IntVector4>();
	public List<IntVector4> DirtyChunkPosList{	get{return _dirtyChunkPosList;	}	}
	public List<VFVoxelChunkData> DirtyChunkList{	get{return _tmpDirtyChunks;	}	}

	public int Viscosity = 4;
	public bool UpdateFluid(bool bRebuild)
	{
		int nDirtyChunkPos = _dirtyChunkPosList.Count;
		if(nDirtyChunkPos == 0)			return false;

		_tmpDirtyChunks.Clear();
		_tmpDirtyChunkPosList.Clear();
		for(int n = 0; n < nDirtyChunkPos; n++)
		{
			IntVector4 chunkPos = _dirtyChunkPosList[n];
			VFVoxelChunkData curWaterChunk = VFVoxelWater.self.Voxels.readChunk(chunkPos.x, chunkPos.y, chunkPos.z, 0);
			if(curWaterChunk == null || curWaterChunk.DataVT.Length <= VFVoxel.c_VTSize)
			{
				if(!_tmpDirtyChunkPosList.Contains(chunkPos))
				{
					_tmpDirtyChunkPosList.Add(chunkPos);
				}
				continue;
			}

			int idx = 0;
			for(int z = -1; z <= 1; z++)
			{
				for(int y = -1; y <= 1; y++)
				{
					for(int x = -1; x <= 1; x++)
					{
						VFVoxelChunkData chunk= idx==13 ? curWaterChunk : 
												VFVoxelWater.self.Voxels.readChunk(chunkPos.x+x, chunkPos.y+y, chunkPos.z+z, 0);
						if(chunk == null || chunk.DataVT.Length < VFVoxel.c_VTSize)
								x = y = z = 2; //skip
						else	_tmpNeibourChunks[idx++] = chunk;
					}
				}
			}
			if(idx < 27)
			{
				if(!_tmpDirtyChunkPosList.Contains(chunkPos))
				{
					_tmpDirtyChunkPosList.Add(chunkPos);
				}
				continue;
			}

			Array.Clear(_tmpDirtyNeibourChunks, 0, _tmpDirtyNeibourChunks.Length);			// ready for output
			GenerateFluid(curWaterChunk, _tmpNeibourChunks, _tmpDirtyNeibourChunks, false);	// Not rebuild right now
			for(int i = 0; i < 27; i++)
			{
				if(_tmpDirtyNeibourChunks[i] != null)
				{	
					IntVector4 curChunkPos = _tmpDirtyNeibourChunks[i].ChunkPosLod;
					if(!_tmpDirtyChunkPosList.Contains(curChunkPos))
					{
						_tmpDirtyChunkPosList.Add(curChunkPos);
						_tmpDirtyChunks.Add(_tmpDirtyNeibourChunks[i]);		// for Fluid's flag of bRebuildNow=false
					}
				}
			}
		}
		List<IntVector4> _tmpPosList = _dirtyChunkPosList;
		_dirtyChunkPosList = _tmpDirtyChunkPosList;
		_tmpDirtyChunkPosList = _tmpPosList;
		if(bRebuild)
		{
			int n = _tmpDirtyChunks.Count;
			for(int i = 0; i < n; i++)
			{
				_tmpDirtyChunks[i].EndBatchWriteVoxels();
			}
		}
		return true;
	}
	private byte[] GetTerraDataForWater(VFVoxelChunkData curWaterChunk)
	{
		//return _tmpTerDataVT; // Test use
		VFVoxelChunkData terChunk = curWaterChunk.GetNodeData(VFVoxelTerrain.self.IdxInLODNodeData);
		byte volume = 0;
		if(terChunk != null)
		{
			int dataLen = terChunk.DataVT.Length;
			if(dataLen == VoxelTerrainConstants.VOXEL_ARRAY_LENGTH_VT)
				return terChunk.DataVT;

			if(dataLen == VFVoxel.c_VTSize)
				volume = terChunk.DataVT[0];
		}
		if(volume != _tmpTerVolume)
		{
			if(volume == 0)
			{
				Array.Clear(_tmpTerDataVT, 0, VoxelTerrainConstants.VOXEL_ARRAY_LENGTH_VT);
			}
			else
			{
				for(int i = 0; i < VoxelTerrainConstants.VOXEL_ARRAY_LENGTH_VT;)
				{	// We don't care about TYPE
					_tmpTerDataVT[i++] = volume; i++;
					_tmpTerDataVT[i++] = volume; i++;
					_tmpTerDataVT[i++] = volume; i++;
					_tmpTerDataVT[i++] = volume; i++;
					_tmpTerDataVT[i++] = volume; i++;
				}
			}
			_tmpTerVolume = volume;
		}
		return _tmpTerDataVT;
	}
	private VFVoxelChunkData[] GenerateFluid(VFVoxelChunkData curChunk, VFVoxelChunkData[] neibourChunks, VFVoxelChunkData[] dirtyNeibourChunks, bool bRebuildNow)
	{
		byte[] data = curChunk.DataVT;
		if (data.Length < VoxelTerrainConstants.VOXEL_ARRAY_LENGTH_VT)	// Safe code for multi mode may no data
			return dirtyNeibourChunks;

		byte[] terData= GetTerraDataForWater(curChunk);
		int fluidDataLevel1, fluidDataLevel2, fluidDataLevel3, fluidDataLevel4, fluidDataLevel5, fluidDataLevel6;
		int terraDataLevel6;
		for (int j = 0; j < VoxelTerrainConstants._numVoxelsPerAxis; j++)	// Y
		{
			for (int k = 0; k < VoxelTerrainConstants._numVoxelsPerAxis; k++)	// X
			{
				for (int l = 0; l < VoxelTerrainConstants._numVoxelsPerAxis; l++) // Z
				{
					int idx6 = VFVoxelChunkData.OneIndex(k, j, l);
					int idxVT6 = idx6*VFVoxel.c_VTSize; 
					fluidDataLevel6 = data[idxVT6];
					terraDataLevel6 = terData[idxVT6];
					// Stratege: Fluid only if volume>terVolume
					if (fluidDataLevel6 > terraDataLevel6)
					{
						byte type = data[idxVT6+1];
						bool bConstWater = type >= (byte)VFVoxel.EType.WaterSourceBeg;
						int idxVT3 = idxVT6 - VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE_VT; 			// y-1
						fluidDataLevel3 = data[idxVT3];
						if(fluidDataLevel3 < 255 && (terData[idxVT3] < 255 || terData[idxVT6] == 0))	// Add terData[idxVT6] == 0 to avoid seam between terrain and water
						{
							if(bConstWater) //TMP CODE
							{
								fluidDataLevel3 = 255;
								VFVoxelChunkData.ModVolumeType(curChunk, idxVT3, (byte)fluidDataLevel3, type, neibourChunks, dirtyNeibourChunks);
								if(data[idxVT6+VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE_VT] != 0) 
									fluidDataLevel6 = 255;
								if(data[idxVT6] != fluidDataLevel6)
								{
									VFVoxelChunkData.ModVolumeType(curChunk, idxVT6, (byte)fluidDataLevel6, type, neibourChunks, dirtyNeibourChunks);
									continue;
								}
							}
							else
							{
								this.FlowBottom(ref fluidDataLevel6, ref fluidDataLevel3);
								VFVoxelChunkData.ModVolume(curChunk, idxVT3, (byte)fluidDataLevel3, neibourChunks, dirtyNeibourChunks);
							}
						}
						if (fluidDataLevel6 > terraDataLevel6)
						{
							EulerianMatrix eulerianMatrix = EulerianMatrix.None;
							int idxVT1 = idxVT6 - VFVoxel.c_VTSize; 									// x-1
							int idxVT2 = idxVT6 + VFVoxel.c_VTSize; 									// x+1
							int idxVT4 = idxVT6 + VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SQUARED_VT; 	// z+1
							int idxVT5 = idxVT6 - VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SQUARED_VT; 	// z-1
							fluidDataLevel1 = data[idxVT1];
							fluidDataLevel2 = data[idxVT2];
							fluidDataLevel4 = data[idxVT4];
							fluidDataLevel5 = data[idxVT5];
							int maxFluidableLevel = fluidDataLevel6 - Viscosity;
							if(fluidDataLevel1 < maxFluidableLevel && terData[idxVT1] < fluidDataLevel6) 
								eulerianMatrix |= EulerianMatrix.Left;
							if(fluidDataLevel2 < maxFluidableLevel && terData[idxVT2] < fluidDataLevel6) 
								eulerianMatrix |= EulerianMatrix.Right;
							if(fluidDataLevel4 < maxFluidableLevel && terData[idxVT4] < fluidDataLevel6) 
								eulerianMatrix |= EulerianMatrix.Forward;
							if(fluidDataLevel5 < maxFluidableLevel && terData[idxVT5] < fluidDataLevel6) 
								eulerianMatrix |= EulerianMatrix.Backward;
							if (eulerianMatrix != EulerianMatrix.None)
							{
								switch (eulerianMatrix)
								{
								case EulerianMatrix.Left:
									this.Flow1(ref fluidDataLevel6, ref fluidDataLevel1);
									break;
								case EulerianMatrix.Right:
									this.Flow1(ref fluidDataLevel6, ref fluidDataLevel2);
									break;
								case EulerianMatrix.Left | EulerianMatrix.Right:
									this.Flow2(ref fluidDataLevel6, ref fluidDataLevel1, ref fluidDataLevel2);
									break;
								case EulerianMatrix.Forward:
									this.Flow1(ref fluidDataLevel6, ref fluidDataLevel4);
									break;
								case EulerianMatrix.Left | EulerianMatrix.Forward:
									this.Flow2(ref fluidDataLevel6, ref fluidDataLevel4, ref fluidDataLevel1);
									break;
								case EulerianMatrix.Right | EulerianMatrix.Forward:
									this.Flow2(ref fluidDataLevel6, ref fluidDataLevel4, ref fluidDataLevel2);
									break;
								case EulerianMatrix.Left | EulerianMatrix.Right | EulerianMatrix.Forward:
									this.Flow3(ref fluidDataLevel6, ref fluidDataLevel1, ref fluidDataLevel2, ref fluidDataLevel4);
									break;
								case EulerianMatrix.Backward:
									this.Flow1(ref fluidDataLevel6, ref fluidDataLevel5);
									break;
								case EulerianMatrix.Left | EulerianMatrix.Backward:
									this.Flow2(ref fluidDataLevel6, ref fluidDataLevel5, ref fluidDataLevel1);
									break;
								case EulerianMatrix.Right | EulerianMatrix.Backward:
									this.Flow2(ref fluidDataLevel6, ref fluidDataLevel5, ref fluidDataLevel2);
									break;
								case EulerianMatrix.Left | EulerianMatrix.Right | EulerianMatrix.Backward:
									this.Flow3(ref fluidDataLevel6, ref fluidDataLevel1, ref fluidDataLevel2, ref fluidDataLevel5);
									break;
								case EulerianMatrix.Forward | EulerianMatrix.Backward:
									this.Flow2(ref fluidDataLevel6, ref fluidDataLevel4, ref fluidDataLevel5);
									break;
								case EulerianMatrix.Left | EulerianMatrix.Forward | EulerianMatrix.Backward:
									this.Flow3(ref fluidDataLevel6, ref fluidDataLevel4, ref fluidDataLevel5, ref fluidDataLevel1);
									break;
								case EulerianMatrix.Right | EulerianMatrix.Forward | EulerianMatrix.Backward:
									this.Flow3(ref fluidDataLevel6, ref fluidDataLevel4, ref fluidDataLevel5, ref fluidDataLevel2);
									break;
								case EulerianMatrix.Left | EulerianMatrix.Right | EulerianMatrix.Forward | EulerianMatrix.Backward:
									this.Flow4(ref fluidDataLevel6, ref fluidDataLevel4, ref fluidDataLevel5, ref fluidDataLevel1, ref fluidDataLevel2);
									break;
								}
								// Stratege: Sea(type>=128) can not be flowed into except from top
								if(0 != (eulerianMatrix&EulerianMatrix.Left) && data[idxVT1+1] < (byte)VFVoxel.EType.WaterSourceBeg && data[idxVT1] != fluidDataLevel1 && data[idxVT1+1-VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE_VT] < (byte)VFVoxel.EType.WaterSourceBeg ) 
									VFVoxelChunkData.ModVolume(curChunk, idxVT1, (byte)fluidDataLevel1, neibourChunks, dirtyNeibourChunks);
								if(0 != (eulerianMatrix&EulerianMatrix.Right) && data[idxVT2+1] < (byte)VFVoxel.EType.WaterSourceBeg && data[idxVT2] != fluidDataLevel2 && data[idxVT2+1-VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE_VT] < (byte)VFVoxel.EType.WaterSourceBeg )  
									VFVoxelChunkData.ModVolume(curChunk, idxVT2, (byte)fluidDataLevel2, neibourChunks, dirtyNeibourChunks);
								if(0 != (eulerianMatrix&EulerianMatrix.Forward) && data[idxVT4+1] < (byte)VFVoxel.EType.WaterSourceBeg && data[idxVT4] != fluidDataLevel4 && data[idxVT4+1-VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE_VT] < (byte)VFVoxel.EType.WaterSourceBeg )  
									VFVoxelChunkData.ModVolume(curChunk, idxVT4, (byte)fluidDataLevel4, neibourChunks, dirtyNeibourChunks);
								if(0 != (eulerianMatrix&EulerianMatrix.Backward) && data[idxVT5+1] < (byte)VFVoxel.EType.WaterSourceBeg && data[idxVT5] != fluidDataLevel5 && data[idxVT5+1-VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE_VT] < (byte)VFVoxel.EType.WaterSourceBeg )  
									VFVoxelChunkData.ModVolume(curChunk, idxVT5, (byte)fluidDataLevel5, neibourChunks, dirtyNeibourChunks);
							}
						}
						if(data[idxVT6] != fluidDataLevel6 && !bConstWater)
						{
							VFVoxelChunkData.ModVolume(curChunk, idxVT6, (byte)fluidDataLevel6, neibourChunks, dirtyNeibourChunks);
						}
					}
				}
			}
		}
		if(bRebuildNow)
		{
			for(int i = 0; i < 27; i++)
			{
				if(dirtyNeibourChunks[i] != null)
					dirtyNeibourChunks[i].EndBatchWriteVoxels();
			}
		}
		return dirtyNeibourChunks;
	}
	private void FlowBottom(ref int cell, ref int bottom)
	{
		int a = 255 - bottom;
		int num = Mathf.Min(a, cell);
		bottom += num;
		cell -= num;
	}
	private void Flow1(ref int cell, ref int other)
	{
		int num1 = (other + cell) >>1;
		int num2 = (other + cell) & 1;
		other = num1 + num2;
		cell  = num1;
	}
	private void Flow2(ref int cell, ref int other1, ref int other2)
	{
		int num1 = (other1 + other2 + cell) / 3;
		int num2 = (other1 + other2 + cell) % 3;
		cell = num1 + num2;
		other1 = num1;
		other2 = num1;
	}
	private void Flow3(ref int cell, ref int other1, ref int other2, ref int other3)
	{
		int num1 = (other1 + other2 + other3 + cell) >>2;
		int num2 = (other1 + other2 + other3 + cell) & 3;
		cell = num1 + num2;
		other1 = num1;
		other2 = num1;
		other3 = num1;
	}
	private void Flow4(ref int cell, ref int other1, ref int other2, ref int other3, ref int other4)
	{
		int num1 = (other1 + other2 + other3 + other4 + cell) / 5;
		int num2 = (other1 + other2 + other3 + other4 + cell) % 5;
		cell = num1 + num2;
		other1 = num1;
		other2 = num1;
		other3 = num1;
		other4 = num1;
	}

	// For water data generation, invoke GenerateFluidConstWater instead of GenerateFluid
	public bool UpdateFluidConstWater(bool bRebuild)
	{
		int nDirtyChunkPos = _dirtyChunkPosList.Count;
		if(nDirtyChunkPos == 0)			return false;
		
		_tmpDirtyChunks.Clear();
		_tmpDirtyChunkPosList.Clear();
		for(int n = 0; n < nDirtyChunkPos; n++)
		{
			IntVector4 chunkPos = _dirtyChunkPosList[n];
			VFVoxelChunkData curWaterChunk = VFVoxelWater.self.Voxels.readChunk(chunkPos.x, chunkPos.y, chunkPos.z, 0);
			if(curWaterChunk == null || curWaterChunk.DataVT.Length <= VFVoxel.c_VTSize)
			{
				if(!_tmpDirtyChunkPosList.Contains(chunkPos))
				{
					_tmpDirtyChunkPosList.Add(chunkPos);
				}
				continue;
			}
			
			int idx = 0;
			for(int z = -1; z <= 1; z++)
			{
				for(int y = -1; y <= 1; y++)
				{
					for(int x = -1; x <= 1; x++)
					{
						VFVoxelChunkData chunk= idx==13 ? curWaterChunk : 
							VFVoxelWater.self.Voxels.readChunk(chunkPos.x+x, chunkPos.y+y, chunkPos.z+z, 0);
						if(chunk == null || chunk.DataVT.Length < VFVoxel.c_VTSize)
							x = y = z = 2; //skip
						else	_tmpNeibourChunks[idx++] = chunk;
					}
				}
			}
			if(idx < 27)
			{
				if(!_tmpDirtyChunkPosList.Contains(chunkPos))
				{
					_tmpDirtyChunkPosList.Add(chunkPos);
				}
				continue;
			}
			
			Array.Clear(_tmpDirtyNeibourChunks, 0, _tmpDirtyNeibourChunks.Length);			// ready for output
			GenerateFluidConstWater(curWaterChunk, _tmpNeibourChunks, _tmpDirtyNeibourChunks, false);	// Not rebuild right now
			for(int i = 0; i < 27; i++)
			{
				if(_tmpDirtyNeibourChunks[i] != null)
				{	
					IntVector4 curChunkPos = _tmpDirtyNeibourChunks[i].ChunkPosLod;
					if(!_tmpDirtyChunkPosList.Contains(curChunkPos))
					{
						_tmpDirtyChunkPosList.Add(curChunkPos);
						_tmpDirtyChunks.Add(_tmpDirtyNeibourChunks[i]);		// for Fluid's flag of bRebuildNow=false
					}
				}
			}
		}
		List<IntVector4> _tmpPosList = _dirtyChunkPosList;
		_dirtyChunkPosList = _tmpDirtyChunkPosList;
		_tmpDirtyChunkPosList = _tmpPosList;
		if(bRebuild)
		{
			int n = _tmpDirtyChunks.Count;
			for(int i = 0; i < n; i++)
			{
				_tmpDirtyChunks[i].EndBatchWriteVoxels();
			}
		}
		return true;
	}
	private VFVoxelChunkData[] GenerateFluidConstWater(VFVoxelChunkData curChunk, VFVoxelChunkData[] neibourChunks, VFVoxelChunkData[] dirtyNeibourChunks, bool bRebuildNow)
	{
		byte[] data = curChunk.DataVT;
		byte[] terData= GetTerraDataForWater(curChunk);
		int fluidDataLevel1, fluidDataLevel2, fluidDataLevel3, fluidDataLevel4, fluidDataLevel5, fluidDataLevel6;
		int terraDataLevel6;
		for (int j = 0; j < VoxelTerrainConstants._numVoxelsPerAxis; j++)	// Y
		{
			for (int k = 0; k < VoxelTerrainConstants._numVoxelsPerAxis; k++)	// X
			{
				for (int l = 0; l < VoxelTerrainConstants._numVoxelsPerAxis; l++) // Z
				{
					int idx6 = VFVoxelChunkData.OneIndex(k, j, l);
					int idxVT6 = idx6*VFVoxel.c_VTSize; 
					fluidDataLevel6 = data[idxVT6];
					terraDataLevel6 = terData[idxVT6];
					// Stratege: Fluid only if volume>terVolume
					if (fluidDataLevel6 > terraDataLevel6)
					{
						byte type = data[idxVT6+1];
						int idxVT3 = idxVT6 - VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE_VT; 			// y-1
						fluidDataLevel3 = data[idxVT3];
						if(fluidDataLevel3 < 255 && (terData[idxVT3] < 255 || terraDataLevel6 == 0))	// avoiding water-mesh z-fight with ter-mesh
						{
							fluidDataLevel3 = 255;
							VFVoxelChunkData.ModVolumeType(curChunk, idxVT3, (byte)fluidDataLevel3, type, neibourChunks, dirtyNeibourChunks);
						}
						EulerianMatrix eulerianMatrix = EulerianMatrix.None;
						int idxVT1 = idxVT6 - VFVoxel.c_VTSize; 									// x-1
						int idxVT2 = idxVT6 + VFVoxel.c_VTSize; 									// x+1
						int idxVT4 = idxVT6 + VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SQUARED_VT; 	// z+1
						int idxVT5 = idxVT6 - VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SQUARED_VT; 	// z-1
						fluidDataLevel1 = data[idxVT1];
						fluidDataLevel2 = data[idxVT2];
						fluidDataLevel4 = data[idxVT4];
						fluidDataLevel5 = data[idxVT5];
						int maxFluidableLevel = fluidDataLevel6 - Viscosity;
						if(fluidDataLevel1 < maxFluidableLevel && terData[idxVT1] < fluidDataLevel6) 
							eulerianMatrix |= EulerianMatrix.Left;
						if(fluidDataLevel2 < maxFluidableLevel && terData[idxVT2] < fluidDataLevel6) 
							eulerianMatrix |= EulerianMatrix.Right;
						if(fluidDataLevel4 < maxFluidableLevel && terData[idxVT4] < fluidDataLevel6) 
							eulerianMatrix |= EulerianMatrix.Forward;
						if(fluidDataLevel5 < maxFluidableLevel && terData[idxVT5] < fluidDataLevel6) 
							eulerianMatrix |= EulerianMatrix.Backward;
						if (eulerianMatrix != EulerianMatrix.None)
						{
							if(0 != (eulerianMatrix&EulerianMatrix.Left) && data[idxVT1] != maxFluidableLevel) 
								VFVoxelChunkData.ModVolumeType(curChunk, idxVT1, (byte)maxFluidableLevel, type, neibourChunks, dirtyNeibourChunks);
							if(0 != (eulerianMatrix&EulerianMatrix.Right) && data[idxVT2] != maxFluidableLevel) 
								VFVoxelChunkData.ModVolumeType(curChunk, idxVT2, (byte)maxFluidableLevel, type, neibourChunks, dirtyNeibourChunks);
							if(0 != (eulerianMatrix&EulerianMatrix.Forward) && data[idxVT4] != maxFluidableLevel) 
								VFVoxelChunkData.ModVolumeType(curChunk, idxVT4, (byte)maxFluidableLevel, type, neibourChunks, dirtyNeibourChunks);
							if(0 != (eulerianMatrix&EulerianMatrix.Backward) && data[idxVT5] != maxFluidableLevel) 
								VFVoxelChunkData.ModVolumeType(curChunk, idxVT5, (byte)maxFluidableLevel, type, neibourChunks, dirtyNeibourChunks);
						}
					}
				}
			}
		}
		if(bRebuildNow)
		{
			for(int i = 0; i < 27; i++)
			{
				if(dirtyNeibourChunks[i] != null)
					dirtyNeibourChunks[i].EndBatchWriteVoxels();
			}
		}
		return dirtyNeibourChunks;
	}
}
