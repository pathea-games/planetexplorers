using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
public class B45LODCompute {
	public byte[] Compute( List<byte[]> srcChunkData){
		byte[] LODChunkData = new byte[Block45Constants.VOXEL_ARRAY_LENGTH_VT];
		
		for(int z = 0; z < 2; z++){
			for(int y = 0; y < 2; y++){
				for(int x = 0; x < 2; x++){
					
					int chunkId = x + y * 2 + z * 4;
					ComputeOne(LODChunkData, srcChunkData, chunkId, new IntVector3(x,y,z));
					
				}
			}
		}
		return LODChunkData;
	}
	void ComputeOne(byte[] LODChunkData, List<byte[]> srcChunkData, int cid, IntVector3 destChunkOfs){
		byte[] eightTypes = new byte[8];
		byte[] eightMat = new byte[8];
		
		byte[] eightOccu = new byte[8];
		
		for(int z = 0; z < Block45Constants._numVoxelsPerAxis; z+=2){
			for(int y = 0; y < Block45Constants._numVoxelsPerAxis; y+=2){
				for(int x = 0; x < Block45Constants._numVoxelsPerAxis; x+=2){
					
					eightTypes[0] = (byte)(srcChunkData[cid][B45Block.Block45Size*B45ChunkData.OneIndex(x,y,z)]);
					eightTypes[1] = (byte)(srcChunkData[cid][B45Block.Block45Size*B45ChunkData.OneIndex(x,y,z + 1)]);
					eightTypes[2] = (byte)(srcChunkData[cid][B45Block.Block45Size*B45ChunkData.OneIndex(x + 1,y,z + 1)]);
					eightTypes[3] = (byte)(srcChunkData[cid][B45Block.Block45Size*B45ChunkData.OneIndex(x + 1,y,z)]);
					
					eightTypes[4] = (byte)(srcChunkData[cid][B45Block.Block45Size*B45ChunkData.OneIndex(x,y + 1,z)]);
					eightTypes[5] = (byte)(srcChunkData[cid][B45Block.Block45Size*B45ChunkData.OneIndex(x,y + 1,z + 1)]);
					eightTypes[6] = (byte)(srcChunkData[cid][B45Block.Block45Size*B45ChunkData.OneIndex(x + 1,y + 1,z + 1)]);
					eightTypes[7] = (byte)(srcChunkData[cid][B45Block.Block45Size*B45ChunkData.OneIndex(x + 1,y + 1,z)]);
					
					eightMat[0] = (byte)(srcChunkData[cid][B45Block.Block45Size*B45ChunkData.OneIndex(x,y,z) + 1]);
					eightMat[1] = (byte)(srcChunkData[cid][B45Block.Block45Size*B45ChunkData.OneIndex(x,y,z + 1) + 1]);
					eightMat[2] = (byte)(srcChunkData[cid][B45Block.Block45Size*B45ChunkData.OneIndex(x + 1,y,z + 1) + 1]);
					eightMat[3] = (byte)(srcChunkData[cid][B45Block.Block45Size*B45ChunkData.OneIndex(x + 1,y,z) + 1]);
					
					eightMat[4] = (byte)(srcChunkData[cid][B45Block.Block45Size*B45ChunkData.OneIndex(x,y + 1,z) + 1]);
					eightMat[5] = (byte)(srcChunkData[cid][B45Block.Block45Size*B45ChunkData.OneIndex(x,y + 1,z + 1) + 1]);
					eightMat[6] = (byte)(srcChunkData[cid][B45Block.Block45Size*B45ChunkData.OneIndex(x + 1,y + 1,z + 1) + 1]);
					eightMat[7] = (byte)(srcChunkData[cid][B45Block.Block45Size*B45ChunkData.OneIndex(x + 1,y + 1,z) + 1]);
					
					eightOccu[0] = TypeToOccupancy[eightTypes[0]>>2];
					eightOccu[1] = TypeToOccupancy[eightTypes[1]>>2];
					eightOccu[2] = TypeToOccupancy[eightTypes[2]>>2];
					eightOccu[3] = TypeToOccupancy[eightTypes[3]>>2];
					
					eightOccu[4] = TypeToOccupancy[eightTypes[4]>>2];
					eightOccu[5] = TypeToOccupancy[eightTypes[5]>>2];
					eightOccu[6] = TypeToOccupancy[eightTypes[6]>>2];
					eightOccu[7] = TypeToOccupancy[eightTypes[7]>>2];
					
					int sum =   eightOccu[0] + eightOccu[1] + eightOccu[2] + eightOccu[3] + 
								eightOccu[4] + eightOccu[5] + eightOccu[6] + eightOccu[7];
					
					///if( sum > 0){
					///	int sdl= 0;
					///}
					List<int> indices = GetSimilarIndices(sum, 3);
					int smallestError = 255;
					int bestMatchI = -1, bestMatchR = -1;
					for(int i = 0; i < indices.Count; i++)
					{
						int errorSum = CalculateError(eightOccu, indices[i], 0);
						if(smallestError > errorSum){
							smallestError = errorSum;
							bestMatchI = indices[i];
							bestMatchR = 3;
						}
						errorSum = CalculateError(eightOccu, indices[i], 1);
						if(smallestError > errorSum){
							smallestError = errorSum;
							bestMatchI = indices[i];
							bestMatchR = 2;
						}
						errorSum = CalculateError(eightOccu, indices[i], 2);
						if(smallestError > errorSum){
							smallestError = errorSum;
							bestMatchI = indices[i];
							bestMatchR = 1;
						}
						errorSum = CalculateError(eightOccu, indices[i], 3);
						if(smallestError > errorSum){
							smallestError = errorSum;
							bestMatchI = indices[i];
							bestMatchR = 0;
						}
					}
					B45Block blk;
					blk.blockType = B45Block.MakeBlockType(bestMatchI, bestMatchR);
					blk.materialType = 0;
					
					// determine the material type for this lod block.
					byte largestOccu = 0;
					for(int i = 0; i < 8; i++){
						if(eightOccu[i] > largestOccu)
						{
							largestOccu = eightOccu[i];
							blk.materialType = eightMat[i];
						}
					}
					int tmpIdx = B45Block.Block45Size*B45ChunkData.OneIndex(
						destChunkOfs.x * Block45Constants._numVoxelsPerAxis / 2 + x / 2,
						destChunkOfs.y * Block45Constants._numVoxelsPerAxis / 2 + y / 2,
						destChunkOfs.z * Block45Constants._numVoxelsPerAxis / 2 + z / 2
						);
					LODChunkData[tmpIdx] = blk.blockType;
					LODChunkData[tmpIdx + 1] = blk.materialType;
					
				}
			}
		}
	}
	byte[] TypeToOccupancy = new byte[]{
		0, 4, 2, 2, 1, 3, 1, 0,
		0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0,
		
		0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0,
	};
	int[] proximityIndices;
	const int NumBlockTypes = 7;
	
	public class IntPair
	{
		public int key;
		public int val;
		public IntPair(int _k, int _v)
		{
			key = _k;
			val = _v;
		}
	};
	List<IntPair> intPairList;
	
	public void Init()
	{
		// we are unable to use the dictionary class to do the indexing because
		// it does not permit fuzzy inputs.
		
		
		intPairList = new List<IntPair>();
		for(int i = 0; i < NumBlockTypes;i++){
			
			int sum = 0;
			int idx0 = i * 8;
			for(int j = 0; j < 8;j++){
				sum += OccupancyRates[idx0 + j];
			}
			intPairList.Add(new IntPair(sum,i));		
		}
		intPairList.Sort(
		    delegate(IntPair p1, IntPair p2)
		    {
				return (p1.key<p2.key)? -1: ((p1.key>p2.key)? 1:0);
		    }
		);
		
		initProximityIndices();
		
//		List<int> ret = GetSimilarIndices(35, 3);
//		for(int i = 0; i < ret.Count; i++)
//		{
//			MonoBehaviour.print(ret[i]);
//		}
	}
	int CalculateError(byte[] eightOccu, int type, int rot)
	{
		int err = 0;
		for(int i = 0; i < 4; i++)
		{
			int idx = (i + rot) % 4;
			
			err += Mathf.Abs(eightOccu[i] - OccupancyRates[type * 8 + idx]);
			err += Mathf.Abs(eightOccu[i+4] - OccupancyRates[type * 8 + 4 + idx]);
		}
		return err;
	}
	// the parameter key here is the sum of all eight cells' occupancy rates.
	List<int> GetSimilarIndices(int key, int count)
	{
		int pivot = proximityIndices[key];
		
		List<int> ret = new List<int>();
		ret.Add(intPairList[pivot].val);
		
		int lowerPtr = pivot - 1;
		int higherPtr = pivot + 1;
		
		int lowerDiff;
		int higherDiff;
		
		for(int i = 0; i < count - 1; i++)
		{
			
			if(lowerPtr < 0)
			{
				lowerDiff = 255;
			}
			else
				lowerDiff = Mathf.Abs(intPairList[lowerPtr].key - key);
			
			if(higherPtr >= NumBlockTypes)
			{
				higherDiff = 255;
			}
			else
				higherDiff = Mathf.Abs(intPairList[higherPtr].key - key);
			
			if(lowerDiff < higherDiff)
			{
				ret.Add(intPairList[lowerPtr].val);
				lowerPtr--;
				
			}else{
				ret.Add(intPairList[higherPtr].val);
				higherPtr++;
				
				
			}
			
		}
		return ret;
	}
	byte[] OccupancyRates = new byte[]{
		0,0,0,0, 0,0,0,0,
		4,4,4,4, 4,4,4,4,
		4,2,2,4, 2,0,0,2,
		4,2,0,2, 4,2,0,2,
		3,1,0,1, 1,0,0,0,
		4,4,3,4, 4,3,1,3,
		1,1,1,1, 0,0,0,0,
	};
	
	void initProximityIndices()
	{
		proximityIndices = new int[41];
		for(int i = 0; i <= 40; i++)
		{
			int closest = 256;
			int closestJ = -1;
			for(int j = 0; j < intPairList.Count;j++){
				if(Mathf.Abs(i - intPairList[j].key) < closest)
				{
					closest = Mathf.Abs(i - intPairList[j].key);
					closestJ = j;
				}
			}
			proximityIndices[i] = closestJ;
			
		}
	}

}
