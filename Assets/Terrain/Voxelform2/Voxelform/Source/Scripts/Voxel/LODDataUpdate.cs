//#define DEBUG_LDUSingleThread
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading;
public class LODDataUpdate
{
	public static LODDataUpdate self;
	Thread workerThread;
	public class LODUpdateCmd
	{
		public VFVoxelChunkData chunk;
		
		public IntVector3 posToRead;
		public IntVector3 posToWrite;
		
		public LODUpdateCmd(VFVoxelChunkData _chunk)
		{
			chunk = _chunk;
			posToRead = null;
			posToWrite = null;
		}
		public static void PreReadOps(IntVector3 pos1)
		{
			// round it off to the cloesest 2's multiple
			if(pos1.x >= 0)pos1.x = (pos1.x >> 1) << 1;
			if(pos1.y >= 0)pos1.y = (pos1.y >> 1) << 1;
			if(pos1.z >= 0)pos1.z = (pos1.z >> 1) << 1;
		}
		// pos2 is the modPos.
		public static void PostReadOps(IntVector3 pos1, IntVector3 pos2)
		{
			
		}
		public override int GetHashCode ()	// In Dictionary, x,y,z must be unmodifiable to keep hash code constant
		{
			// In our game, w is used as lod almost, so 5bits can be enough to represent it
			return posToRead.x+(posToRead.z<<10)+(posToRead.y<<20)+(chunk.LOD<<28);
		}
		public override bool Equals (object obj)
		{
			LODUpdateCmd cmd = (LODUpdateCmd) obj;
			//if(vec != null)
			{
				return this.posToRead==cmd.posToRead && this.posToWrite == cmd.posToWrite &&
					this.chunk.ChunkPosLod.Equals(cmd.chunk.ChunkPosLod);
			}
			//return false;
		}
	};
	public void init()
	{
		for(int i = 0; i < 35; i++)
		{
			written[i] = 0;
		}
		#if !DEBUG_LDUSingleThread
		workerThread = new Thread(new ThreadStart(threadProc));
		workerThread.Start();
		#endif
		
//		chunkDataMutex = new Mutex(false);
		self = this;
		
	}
	public void Stop()
	{
#if !DEBUG_LDUSingleThread
		_bThreadOn = false;
		try{
			workerThread.Join();
			MonoBehaviour.print("[LODDataUpdate]Thread stopped");
		}
		catch{
			MonoBehaviour.print("[LODDataUpdate]Thread stopped with exception");
		}
#endif	
	}
	bool _bThreadOn = true;
	Mutex chunkDataMutex;
	List<IntVector3> inpFlaggedPosList;
	
	List<List<VFVoxelChunkData>> inpModifiedChunkMulList;
	int[] written = new int[35];
	public void threadProc()
	{
		//long lastTime;
		//long diffTime = -1;
#if !DEBUG_LDUSingleThread
		_bThreadOn = true;
		while(_bThreadOn)
#endif
		{
			
#if !DEBUG_LDUSingleThread
			if( UpdateChunkCoordList == null || UpdateChunkCoordList.Count == 0)
			{
				Thread.Sleep(100);
				continue;
			}
			List<IntVector3> tmpChunkCoordList = null;
			List<IntVector3> tmpLocalCoordList = null;
			
			lock(this)
			{
//				if(chunkDataMutex.WaitOne(200) == false)
//					continue;
#else
				if( UpdateChunkCoordList == null || UpdateChunkCoordList.Count == 0)
					return;
#endif
			
				//lastTime = Environment.TickCount;
				tmpChunkCoordList = UpdateChunkCoordList;
				UpdateChunkCoordList = null;
				
				tmpLocalCoordList = UpdateLocalCoordList;
				UpdateLocalCoordList = null;
			}
			#if !DEBUG_LDUSingleThread
//			chunkDataMutex.ReleaseMutex();
			#endif
			cmdCollision.Clear();
			List<LODUpdateCmd> updateCmdList = new List<LODUpdateCmd>();

			for(int i = 0; i < tmpChunkCoordList.Count; i++)
			{
				GenUpdateCmd(tmpChunkCoordList[i], tmpLocalCoordList[i], updateCmdList);
			}
			
			VFVoxel prevLODVoxel = new VFVoxel(0,0);
			
			for(int list_i = 0; list_i < updateCmdList.Count; list_i++){
				
				LODUpdateCmd thisCmd = updateCmdList[list_i];
				
				thisCmd.chunk = PrepareChunkData(thisCmd.chunk);
				
				if( thisCmd.chunk.LOD > 0){
					
					thisCmd.chunk.WriteVoxelAtIdx4LodUpdate(thisCmd.posToWrite.x, thisCmd.posToWrite.y, thisCmd.posToWrite.z, prevLODVoxel);
					VFVoxelTerrain.self.SaveLoad.SaveChunkToTmpFile(thisCmd.chunk);
				}
				
				prevLODVoxel = ReadVoxel(thisCmd.chunk, thisCmd.posToRead);
			}
			//diffTime = Environment.TickCount - lastTime;
		}
	}
	private VFVoxelChunkData PrepareChunkData(VFVoxelChunkData chunk)
	{
		VFVoxelChunkData chunkDataFromMem = null;
		
		// look for it in the lod data source first. read it from the disk if it is not there.
		if(chunk.LOD <= LODOctreeMan._maxLod){
			chunkDataFromMem = VFVoxelTerrain.self.Voxels.readChunk(chunk.ChunkPosLod.x, chunk.ChunkPosLod.y, chunk.ChunkPosLod.z, chunk.LOD);
		}
		
		if(chunkDataFromMem == null || chunkDataFromMem.DataVT == VFVoxelChunkData.S_ChunkDataNull){

			byte[] tmpData = VFVoxelTerrain.self.SaveLoad.TryGetChunkData(chunk.ChunkPosLod, false);
			if (tmpData != null){
				chunk.SetDataVT(tmpData);
			}
			else{
				decompressData(chunk);
			}
		}
		else{
			chunk = null;
			chunk = chunkDataFromMem;
		}
		return chunk;
	}
	VFVoxel ReadVoxel(VFVoxelChunkData inputChunk, IntVector3 InChunkPos)
	{
		int index = inputChunk.IsHollow ? 0 : VFVoxelChunkData.OneIndexNoPrefix(InChunkPos.x,InChunkPos.y,InChunkPos.z);
		int indexVT = index*VFVoxel.c_VTSize;
        return new VFVoxel(inputChunk.DataVT[indexVT],
		                   inputChunk.DataVT[indexVT+1]);
	}
	VFVoxel GetAverageVoxel2(VFVoxelChunkData inputChunk, IntVector3 InChunkPos)
	{
		int volumeSum = 0;
		byte voxelType = 0;
		//int typeCount = 0;
		VFVoxel[] eight = new VFVoxel[8];
		// sample 8 points
		for(int j = 0 ; j < 8; j++)
		{
			int xInc = (j & 1) > 0 ? 1 : 0;
			int yInc = (j & 2) > 0 ? 1 : 0;
			int zInc = (j & 4) > 0 ? 1 : 0;
			try
			{
				eight[j] = inputChunk.ReadVoxelAtIdx(InChunkPos.x + xInc, InChunkPos.y + yInc, InChunkPos.z + zInc);
			}
			catch{
				//int asdglfg = 0;
			}
//					int idx = VFVoxelChunkData.OneIndex(x + xInc,y + yInc,z + zInc) * VFVoxel.c_VTSize;
			volumeSum += eight[j].Volume;
			
			if(eight[j].Type > 0)
			{
				voxelType = eight[j].Type;
			}
		}
		VFVoxel ret = new VFVoxel((byte)(volumeSum >> 3), voxelType);
		return ret;
	}
	void decompressData(VFVoxelChunkData chunkData)
	{
		int lod = chunkData.LOD;
		int px, py, pz;
		VFFileUtil.WorldChunkPosToPiecePos(chunkData.ChunkPosLod, out px, out py, out pz);	

		int fx, fz; 
		VFFileUtil.PiecePos2FilePos(px, py, pz, lod, out fx, out fz);

		VFPieceDataClone pieceData = VFDataReaderClone.GetPieceDataSub(new IntVector4(px, py, pz, lod), VFDataReaderClone.GetFileSetSub(new IntVector4(fx, 0, fz, lod)));
		pieceData.Decompress();
		pieceData.SetChunkData(chunkData);
		pieceData._data = null;
		pieceData = null;
	}
	
	Dictionary<IntVector4, LODUpdateCmd> cmdCollision = new Dictionary<IntVector4, LODUpdateCmd>();
	List<IntVector3> UpdateChunkCoordList;
	List<IntVector3> UpdateLocalCoordList;
	public void InsertUpdateCoord(int x, int y, int z, IntVector4 chunkPos)
	{
		//Stats (x,y,z, chunkPos);
		lock(this){
			//chunkDataMutex.WaitOne();
			if(UpdateChunkCoordList == null)
				UpdateChunkCoordList = new List<IntVector3>();
			
			if(UpdateLocalCoordList == null)
				UpdateLocalCoordList = new List<IntVector3>();
			
			UpdateChunkCoordList.Add(chunkPos.XYZ);
			
			UpdateLocalCoordList.Add(new IntVector3(x, y, z));
			//chunkDataMutex.ReleaseMutex();
		}
	}
	public void Stats(int x, int y, int z, IntVector3 chunkPos)
	{
		try
		{
			written[y]++;
		}
		catch
		{
			//int k = 0;
		}
	}
	public void GenUpdateCmd(IntVector3 chunkPos, IntVector3 localPos, List<LODUpdateCmd> updateCmdList)
	{
		//int dim = 0;
		IntVector3 modPos = null;
		
		localPos.x += 1;
		localPos.y += 1;
		localPos.z += 1;
		
		IntVector3 runningPos = new IntVector3(localPos);
		
		for(int i = 0; i < LODOctreeMan.MaxLod; i++ )
		{
			//dim = 32 * (1 << i);
			
			IntVector3 lodChunkPos = new IntVector3(chunkPos);
			
			lodChunkPos.x = lodChunkPos.x >> i << i;
			lodChunkPos.y = lodChunkPos.y >> i << i;
			lodChunkPos.z = lodChunkPos.z >> i << i;
			
			// attach a dummy octreenode
			VFVoxelChunkData cd = ForgeChunk(lodChunkPos, i);
			if( modPos == null){
				 modPos = cd.ChunkPosLod.XYZ;
			}
			
			LODUpdateCmd cmd = new LODUpdateCmd(cd);
			if(i > 0){
				if(runningPos.x < 0 || runningPos.y < 0 || runningPos.z < 0 ){
					break;
				}
				cmd.posToWrite = new IntVector3(runningPos);
			}
			
//			LODUpdateCmd.PreReadOps(runningPos);

			cmd.posToRead = new IntVector3(runningPos);
			//LODUpdateCmd.PostReadOps(runningPos, modPos);
			
			runningPos.x += (modPos.x % 2) * 35; 
			runningPos.y += (modPos.y % 2) * 35; 
			runningPos.z += (modPos.z % 2) * 35;
			
			int xIdx = voxelIndexInverted[runningPos.x];
			int yIdx = voxelIndexInverted[runningPos.y];
			int zIdx = voxelIndexInverted[runningPos.z];
			
			runningPos.x = xIdx;
			runningPos.y = yIdx;
			runningPos.z = zIdx;
			
			modPos.x = modPos.x >> 1;
			modPos.y = modPos.y >> 1;
			modPos.z = modPos.z >> 1;
			
			IntVector4 cmdVec = new IntVector4(cmd.posToRead.x, cmd.posToRead.y, cmd.posToRead.z, i);
			LODUpdateCmd outCmd;
			if(cmdCollision.TryGetValue(cmdVec, out outCmd) == true){
				if(!outCmd.Equals(cmd)){
					updateCmdList.Add(cmd);
				}
			}
			else{
				cmdCollision.Add(cmdVec, cmd);
				updateCmdList.Add(cmd);
			}
			
			
		}
	}
	static List<IntVector3> NeighbourCoords(IntVector3 pos, int lod)
    {

		int chunkNumX = LODOctreeMan._xChunkCount >> lod;
		int chunkNumY = LODOctreeMan._yChunkCount >> lod;
		int chunkNumZ = LODOctreeMan._zChunkCount >> lod;
        int shift = VoxelTerrainConstants._shift + lod;
        int cx = (pos.x >> shift);
        int cy = (pos.y >> shift);
        int cz = (pos.z >> shift);
        //int cxround = cx % chunkNumX;
        //int cyround = cy % chunkNumY;
        //int czround = cz % chunkNumZ;
        int vx = (pos.x >> lod) & VoxelTerrainConstants._mask;
        int vy = (pos.y >> lod) & VoxelTerrainConstants._mask;
        int vz = (pos.z >> lod) & VoxelTerrainConstants._mask;
        //if (!_lodman._lodTreeNodes[lod][cxround, cyround, czround]._data.WriteVoxelAtIdx(vx, vy, vz, voxel))
        //    return 0;

        int minIdx = VoxelTerrainConstants._numVoxelsPostfix;
        int maxIdx = VoxelTerrainConstants._numVoxelsPerAxis - VoxelTerrainConstants._numVoxelsPrefix;
        int fx = 0, fy = 0, fz = 0;
        int dirtyMask = 0x80;	// 0,1,2 bit for xyz dirty mask;4,5,6 bit for sign(neg->1);7 bit for current pos(now not used)

        // If write one edge's voxel may cause the other edge being modified
        if (vx < minIdx && cx > 0) { fx = -1; dirtyMask |= 0x11; }
        else
            if (vx >= maxIdx && cx < VoxelTerrainConstants._worldMaxCX - 1) { fx = 1; dirtyMask |= 0x01; }
        if (vy < minIdx && cy > 0) { fy = -1; dirtyMask |= 0x22; }
        else
            if (vy >= maxIdx && cy < VoxelTerrainConstants._worldMaxCY - 1) { fy = 1; dirtyMask |= 0x02; }
        if (vz < minIdx && cz > 0) { fz = -1; dirtyMask |= 0x44; }
        else
            if (vz >= maxIdx && cz < VoxelTerrainConstants._worldMaxCZ - 1) { fz = 1; dirtyMask |= 0x04; }
	
        List<IntVector3> ret = new List<IntVector3>();
        if (dirtyMask != 0x80)
        {
            for (int i = 1; i < 8; i++)
            {
                if ((dirtyMask & i) == i)
                {
					int dx = fx * VFVoxelChunkData.S_NearChunkOfs[i, 0], dy = fy * VFVoxelChunkData.S_NearChunkOfs[i, 1], dz = fz * VFVoxelChunkData.S_NearChunkOfs[i, 2];

                    ret.Add(new IntVector3(
                        (cx + dx) % chunkNumX,
                        (cy + dy) % chunkNumY,
                        (cz + dz) % chunkNumZ
                        ));

                    ret.Add(new IntVector3(
                        vx - dx * VoxelTerrainConstants._numVoxelsPerAxis,
                        vy - dy * VoxelTerrainConstants._numVoxelsPerAxis,
                        vz - dz * VoxelTerrainConstants._numVoxelsPerAxis
                        ));

                }
            }
        }
        return ret;
    }
	
	private VFVoxelChunkData ForgeChunk(IntVector3 pos, int lod)
	{
		LODOctreeNode lodNode = new LODOctreeNode(null, lod, 
		                                          pos.x << VoxelTerrainConstants._shift,
		                                          pos.y << VoxelTerrainConstants._shift,
		                                          pos.z << VoxelTerrainConstants._shift);
		VFVoxelChunkData cd = new VFVoxelChunkData(lodNode);
		cd.ChunkPosLod_w = new IntVector4(pos,lod);
		return cd;
	}
	IntVector3 genNeighbourChunkPos(IntVector3 pos, int dim)
	{
        int x = Mathf.FloorToInt(pos.x / dim);
        int y = Mathf.FloorToInt(pos.y / dim);
        int z = Mathf.FloorToInt(pos.z / dim);

        IntVector3 ret = new IntVector3(x * dim, y * dim, z * dim);

        return ret;
	}
	// 32,33 coorespond to 0,1
	// 34 may cause incorrect normal, but to use 34 will avoid read another chunkdata 
	//static int[] voxelIndex = new int[35]{
	//	0,1,3,5,7,9,11,13,15,17,19,21,23,25,27,29,31,
	//	1,3,5,7,9,11,13,15,17,19,21,23,25,27,29,32,33,34
	//};
	static int[] voxelIndexInverted = new int[70]{
		0, 1, -1, 2, -1, 3, -1, 4, -1, 5, -1, 6, -1, 7, -1, 8, -1, 9, -1, 10, -1, 11, -1, 12, -1, 13, -1, 14, -1, 15, -1, 16, -1, -1, -1,
		-1, 17, -1, 18, -1, 19, -1, 20, -1, 21, -1, 22, -1, 23, -1, 24, -1, 25, -1, 26, -1, 27, -1, 28, -1, 29, -1, 30, -1, 31, -1, -1, 32, 33, 34,
	};
}