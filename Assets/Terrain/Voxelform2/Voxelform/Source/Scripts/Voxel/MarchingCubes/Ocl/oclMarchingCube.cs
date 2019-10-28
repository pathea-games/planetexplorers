#define MC_COMPUTE_ASYNC
//#define PERF_LOG
#define USE_UNITY
#if USE_UNITY
using UnityEngine;
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
#if UNITY_STANDALONE_OSX
using OpenCLNetMac;
using OpenCLNet = OpenCLNetMac;
#else
using OpenCLNetWin;
using OpenCLNet = OpenCLNetWin;
#endif

// CSharp version of NV's marchingcube
// input: volume data, chunk desc(size,volumeData pos) array, 
public static partial class oclMarchingCube
{
	// DEBUG
	// performance counter
	//static int curTime = Environment.TickCount;

    // The number of threads to use for triangle generation (limited by shared memory size)
	const int VOLUME_VOXEL_X_LEN_SHIFT = 7;
	const int VOLUME_VOXEL_Y_LEN_SHIFT = 7;
	const int VOLUME_VOXEL_Z_LEN_SHIFT = 6;
	const int VOLUME_VOXEL_X_LEN = 1<<VOLUME_VOXEL_X_LEN_SHIFT;
	const int VOLUME_VOXEL_Y_LEN = 1<<VOLUME_VOXEL_Y_LEN_SHIFT;
	const int VOLUME_VOXEL_Z_LEN = 1<<VOLUME_VOXEL_Z_LEN_SHIFT;
	const int MAX_VOXELS = VOLUME_VOXEL_X_LEN * VOLUME_VOXEL_Y_LEN * VOLUME_VOXEL_Z_LEN;	//1024*1024, 2048*cube(8)
	// Normal chunk gen plane has about 6900-7800 verts. Assume 75% 8192 and 25% 65536, so average 22528->24576
	const int MAX_VERTS = 124576<<(VOLUME_VOXEL_X_LEN_SHIFT+VOLUME_VOXEL_Y_LEN_SHIFT+VOLUME_VOXEL_Z_LEN_SHIFT-3*CHUNK_VOXEL_DIM_LEN_SHIFT);// it is MAX_VOXELS/32*100 in nv's sample. For safe we should add a cond instruction in cl.
	const int VERT_DATA_SIZE = 7*4;										// 7 float, pos(3)+norm(3)+typeInfo(1)
	const int MAX_MEMORY_FOR_VERTS = MAX_VERTS*VERT_DATA_SIZE;			// 8 float(32 bytes) now 128MB, with 5*4*MAX_VERTS for opencl scan
	
	const int VOXEL_SIZE = VFVoxel.c_VTSize;
	const ChannelOrder CH_ORDER = ChannelOrder.RG;
	const int CHUNK_VOXEL_DIM_LEN_SHIFT = VoxelTerrainConstants._shift;

    // Range [2,5] for opencl workgroupsize;
	const int CHUNK_VOXEL_DIM_LEN = 1<<CHUNK_VOXEL_DIM_LEN_SHIFT;
	const uint CHUNK_VOXEL_DIM_LEN_MASK = CHUNK_VOXEL_DIM_LEN-1;
#if UNITY_STANDALONE_LINUX
	public const int MAX_CHUNKS = (1<<(VOLUME_VOXEL_X_LEN_SHIFT+VOLUME_VOXEL_Y_LEN_SHIFT+VOLUME_VOXEL_Z_LEN_SHIFT - 3*CHUNK_VOXEL_DIM_LEN_SHIFT))>>1;
#else
	public const int MAX_CHUNKS = (1<<(VOLUME_VOXEL_X_LEN_SHIFT+VOLUME_VOXEL_Y_LEN_SHIFT+VOLUME_VOXEL_Z_LEN_SHIFT - 3*CHUNK_VOXEL_DIM_LEN_SHIFT));
#endif
	static readonly UInt4 CHUNK_VOXEL_DIM_LEN_SHIFT_PLUS = new UInt4(CHUNK_VOXEL_DIM_LEN_SHIFT,
	                                                                 CHUNK_VOXEL_DIM_LEN_SHIFT+CHUNK_VOXEL_DIM_LEN_SHIFT,
	                                                                 CHUNK_VOXEL_DIM_LEN_SHIFT+CHUNK_VOXEL_DIM_LEN_SHIFT+CHUNK_VOXEL_DIM_LEN_SHIFT,
	                                                                 0);
	static readonly UInt4 VOLUME_CHUNK_DIM_LEN_SHIFT_PLUS = new UInt4(	VOLUME_VOXEL_X_LEN_SHIFT-CHUNK_VOXEL_DIM_LEN_SHIFT,
																		VOLUME_VOXEL_X_LEN_SHIFT-CHUNK_VOXEL_DIM_LEN_SHIFT + VOLUME_VOXEL_Y_LEN_SHIFT-CHUNK_VOXEL_DIM_LEN_SHIFT,
																		VOLUME_VOXEL_X_LEN_SHIFT-CHUNK_VOXEL_DIM_LEN_SHIFT + VOLUME_VOXEL_Y_LEN_SHIFT-CHUNK_VOXEL_DIM_LEN_SHIFT + VOLUME_VOXEL_Z_LEN_SHIFT-CHUNK_VOXEL_DIM_LEN_SHIFT,
																		0);
	static readonly UInt4 VOLUME_CHUNK_DIM_LEN = new UInt4(1<<(VOLUME_VOXEL_X_LEN_SHIFT-CHUNK_VOXEL_DIM_LEN_SHIFT),
				                                           1<<(VOLUME_VOXEL_Y_LEN_SHIFT-CHUNK_VOXEL_DIM_LEN_SHIFT),
				                                           1<<(VOLUME_VOXEL_Z_LEN_SHIFT-CHUNK_VOXEL_DIM_LEN_SHIFT),
				                                           0);
	static readonly UInt4 VOLUME_CHUNK_DIM_LEN_MASK = new UInt4(VOLUME_CHUNK_DIM_LEN.S0-1,
	                                                   			VOLUME_CHUNK_DIM_LEN.S1-1,
	                                                   			VOLUME_CHUNK_DIM_LEN.S2-1,
	                                                   			0);
	const int CHUNK_VOXEL_NUM = CHUNK_VOXEL_DIM_LEN*CHUNK_VOXEL_DIM_LEN*CHUNK_VOXEL_DIM_LEN;
	const int CHUNK_VOXEL_DIM_LEN_REAL = CHUNK_VOXEL_DIM_LEN+VoxelTerrainConstants._numVoxelsPrefix+VoxelTerrainConstants._numVoxelsPostfix;
    const int CHUNK_VOXEL_NUM_REAL = CHUNK_VOXEL_DIM_LEN_REAL*CHUNK_VOXEL_DIM_LEN_REAL*CHUNK_VOXEL_DIM_LEN_REAL;
	const int VOLUME_VOXEL_X_LEN_REAL = VOLUME_VOXEL_X_LEN*CHUNK_VOXEL_DIM_LEN_REAL/CHUNK_VOXEL_DIM_LEN;
	const int VOLUME_VOXEL_Y_LEN_REAL = VOLUME_VOXEL_Y_LEN*CHUNK_VOXEL_DIM_LEN_REAL/CHUNK_VOXEL_DIM_LEN;
	const int VOLUME_VOXEL_Z_LEN_REAL = VOLUME_VOXEL_Z_LEN*CHUNK_VOXEL_DIM_LEN_REAL/CHUNK_VOXEL_DIM_LEN;
	
	// The number of threads to use for triangle generation (limited by shared memory size), use 32 based on NVidia MC Sample
    const int NTHREADS_SHIFT = 5;
    const int NTHREADS = 1<<NTHREADS_SHIFT;
	const int NTHREADS_MASK = NTHREADS-1;
	
    static OpenCLManager OCLManager;
    static CommandQueue OCLComQueue;
	static bool bImageFormatSupported;
    static Kernel classifyVoxelKernel;
    static Kernel compactVoxelsKernel;
    static Kernel generateTriangles2Kernel;
	
    static Mem d_pos;       // out
    static Mem d_norm01;    // out
	static Mem d_norm2t;    // out
    static Mem d_volume;
    static Mem d_voxelVerts;
    static Mem d_voxelVertsScan;
    static Mem d_voxelOccupied;
    static Mem d_voxelOccupiedScan;
    static Mem d_compVoxelArray;
	
	public static GCHandle hPosArray;
	public static GCHandle hNorm01Array;
	public static GCHandle hNorm2tArray;
	static byte[] volumeZeroConst;
	
	// localWorkSize Note :	product < CL_DEVICE_MAX_WORK_GROUP_SIZE (rv7xx:128;cypress:256;NVidia:about1024;Cpu:1024)
	//						eachOne < corresponding  CL_DEVICE_MAX_WORK_ITEM_SIZE (rv7xx:128;cypress:256;NVidia:about1024;Cpu:1024)
    static int[] localWorkSize = new int[3]{CHUNK_VOXEL_DIM_LEN,1,1};   
    static int[] globalWorkSize = new int[3] { CHUNK_VOXEL_DIM_LEN, CHUNK_VOXEL_DIM_LEN, CHUNK_VOXEL_DIM_LEN };
	
	public
	static int numChunks	= 0;
    static int activeVoxels = 0;
    static int totalVerts   = 0;
    static float isoValue 	= 0.5f;
	
	static int oclMCStatus = 0;
	public static bool IsInitialed{	get{	return oclMCStatus != 0;	}	}
	public static bool IsAvailable{	get{	return oclMCStatus == 1;	}	}
	
    static void
    launch_classifyVoxel(int[] gridDim, int[] threadsDim, Mem voxelVerts, Mem voxelOccupied, Mem volume,
					      uint chunkSizeReal, UInt4 chunkSizeShiftPlus, UInt4 volumeChunkShiftPlus, UInt4 volumeChunkMask,
					      float isoValue)
    {
        classifyVoxelKernel.SetArg(0,voxelVerts);
        classifyVoxelKernel.SetArg(1,voxelOccupied);
        classifyVoxelKernel.SetArg(2,volume);
        classifyVoxelKernel.SetArg(3,chunkSizeReal);
        classifyVoxelKernel.SetArg(4,chunkSizeShiftPlus);
        classifyVoxelKernel.SetArg(5,volumeChunkShiftPlus);
        classifyVoxelKernel.SetArg(6,volumeChunkMask);
        classifyVoxelKernel.SetArg(7,isoValue);
		
        OCLComQueue.EnqueueNDRangeKernel(classifyVoxelKernel, gridDim.Length, null, gridDim, threadsDim);
    }
    static void
    launch_compactVoxels(int[] gridDim, int[] threadsDim, Mem compVoxelArray, Mem voxelOccupied, Mem voxelOccupiedScan, UInt4 chunkSizeShiftPlus)
    {
        compactVoxelsKernel.SetArg(0,compVoxelArray);
        compactVoxelsKernel.SetArg(1,voxelOccupied);
        compactVoxelsKernel.SetArg(2,voxelOccupiedScan);
        compactVoxelsKernel.SetArg(3, chunkSizeShiftPlus);

        OCLComQueue.EnqueueNDRangeKernel(compactVoxelsKernel, gridDim.Length, null, gridDim, threadsDim);
    }
    static void
    launch_generateTriangles2(int[] gridDim, int[] threadsDim,
                              Mem pos, Mem norm01, Mem norm2t, Mem compactedVoxelArray, Mem numVertsScanned, Mem volume,
                              uint chunkSizeReal, uint chunkSizeMask, UInt4 chunkSizeShiftPlus, UInt4 volumeChunkShiftPlus, UInt4 volumeChunkMask,
                              float isoValue, uint nActiveVoxels, uint nMaxVerts)
	{
		int i=0;
        generateTriangles2Kernel.SetArg(i++, pos);
        generateTriangles2Kernel.SetArg(i++, norm01);
		generateTriangles2Kernel.SetArg(i++, norm2t);
        generateTriangles2Kernel.SetArg(i++, compactedVoxelArray);
        generateTriangles2Kernel.SetArg(i++, numVertsScanned);
        generateTriangles2Kernel.SetArg(i++, volume);
        generateTriangles2Kernel.SetArg(i++, chunkSizeReal);
        generateTriangles2Kernel.SetArg(i++, chunkSizeMask);
        generateTriangles2Kernel.SetArg(i++, chunkSizeShiftPlus);
        generateTriangles2Kernel.SetArg(i++, volumeChunkShiftPlus);
        generateTriangles2Kernel.SetArg(i++, volumeChunkMask);
        generateTriangles2Kernel.SetArg(i++, isoValue);
		generateTriangles2Kernel.SetArg(i++, nActiveVoxels);
        generateTriangles2Kernel.SetArg(i++, nMaxVerts);

        OCLComQueue.EnqueueNDRangeKernel(generateTriangles2Kernel, gridDim.Length, null, gridDim, threadsDim);
    }
	
    ////////////////////////////////////////////////////////////////////////////////
    // initialize marching cubes
    ////////////////////////////////////////////////////////////////////////////////
    public static bool InitMC()
    {
		try{		
		    OCLManager = oclManager.ActiveOclMan;
		    OCLComQueue = oclManager.ActiveOclCQ;
	
			try
			{
				oclScanLaucher.initScan(OCLManager, OCLComQueue, VOLUME_VOXEL_X_LEN_SHIFT+VOLUME_VOXEL_Y_LEN_SHIFT+VOLUME_VOXEL_Z_LEN_SHIFT, CHUNK_VOXEL_NUM);
			}
			catch(Exception e)
			{
				string log = "[OCLLOG]Kernel Error: ";
				OpenCLBuildException eocl = e as OpenCLBuildException;
				if(eocl != null)
				{
					for(int i = 0; i < eocl.BuildLogs.Count; i++)
						log += eocl.BuildLogs[i];
				}
				else
				{
					log += e.Message;
				}
				Debug.LogError(log);
				throw;
			}
			
			ImageFormat imageFormat = new ImageFormat(CH_ORDER, ChannelType.UNORM_INT8);
			bImageFormatSupported = (!OCLComQueue.Device.Name.Contains("RV7") /*!bRV7xxGpu*/&&
				OCLComQueue.Context.SupportsImageFormat(MemFlags.READ_ONLY,MemObjectType.IMAGE3D,imageFormat.ChannelOrder,imageFormat.ChannelType));
			//bImageFormatSupported = false;
			OCLManager.BuildOptions = "-cl-mad-enable";
	        OCLManager.Defines = "";
			Program cpProgram = null;
			while(true)
			{
				try
				{
					string mcKernelPathName = bImageFormatSupported ? "OclKernel/marchingCubes_kernel_img" : ("OclKernel/marchingCubes_kernel_u"+VOXEL_SIZE+"b");
					TextAsset srcKernel = Resources.Load(mcKernelPathName) as TextAsset;
					Debug.Log("[OCLLOG]Build kernel:"+mcKernelPathName);
					cpProgram = OCLManager.CompileSource(srcKernel.text);
					srcKernel = null;
				}
				catch(Exception e)
				{
					string log = "[OCLLOG]Kernel Error: ";
					OpenCLBuildException eocl = e as OpenCLBuildException;
					if(eocl != null)
					{
						for(int i = 0; i < eocl.BuildLogs.Count; i++)
							log += eocl.BuildLogs[i];
					}
					else
					{
						log += e.Message;
					}
					Debug.LogError(log);
					if(bImageFormatSupported)
					{
						bImageFormatSupported = false;
						Debug.Log("[OCLLOG]Try to build kernel without img support:");
						continue;
					}
					
					throw;
				}
				break;
			}
			classifyVoxelKernel = cpProgram.CreateKernel("classifyVoxel");
			compactVoxelsKernel = cpProgram.CreateKernel("compactVoxels");
			generateTriangles2Kernel = cpProgram.CreateKernel("generateTriangles2_vec3");
			cpProgram = null;
			Debug.Log("[OCLLOG]All kernels are ready.");
			
			if(bImageFormatSupported)
			{
	        	d_volume = OCLManager.Context.CreateImage3D(MemFlags.READ_ONLY | MemFlags.ALLOC_HOST_PTR,
	                                        imageFormat,
	                                        VOLUME_VOXEL_X_LEN_REAL, VOLUME_VOXEL_Y_LEN_REAL, VOLUME_VOXEL_Z_LEN_REAL,
			                                0,0,IntPtr.Zero);
			}
			else
			{
				isoValue = 128f;
	        	d_volume = OCLManager.Context.CreateBuffer(MemFlags.READ_ONLY | MemFlags.ALLOC_HOST_PTR,
	                                        VOLUME_VOXEL_X_LEN_REAL*VOLUME_VOXEL_Y_LEN_REAL*VOLUME_VOXEL_Z_LEN_REAL*VOXEL_SIZE);
			}
			
	        // create VBOs --- now use 3 float, nvidia use 4 float originally.
	        Vector3[] posArray = new Vector3[MAX_VERTS];
	        Vector2[] norm01Array = new Vector2[MAX_VERTS];
			Vector2[] norm2tArray = new Vector2[MAX_VERTS];
	        hPosArray = GCHandle.Alloc(posArray, GCHandleType.Pinned);
	        hNorm01Array = GCHandle.Alloc(norm01Array, GCHandleType.Pinned);
			hNorm2tArray = GCHandle.Alloc(norm2tArray, GCHandleType.Pinned);
	        d_pos = OCLManager.Context.CreateBuffer(MemFlags.WRITE_ONLY|MemFlags.USE_HOST_PTR, MAX_VERTS * sizeof(float) * 3, hPosArray.AddrOfPinnedObject());
	        d_norm01 = OCLManager.Context.CreateBuffer(MemFlags.WRITE_ONLY | MemFlags.USE_HOST_PTR, MAX_VERTS * sizeof(float) * 2, hNorm01Array.AddrOfPinnedObject());
			d_norm2t = OCLManager.Context.CreateBuffer(MemFlags.WRITE_ONLY | MemFlags.USE_HOST_PTR, MAX_VERTS * sizeof(float) * 2, hNorm2tArray.AddrOfPinnedObject());
			
			// allocate device memory
	        uint memSize = sizeof(uint) * MAX_VOXELS;
	        d_voxelVerts = OCLManager.Context.CreateBuffer(MemFlags.READ_WRITE, memSize);
	        d_voxelVertsScan = OCLManager.Context.CreateBuffer(MemFlags.READ_WRITE, memSize);
	        d_voxelOccupied = OCLManager.Context.CreateBuffer(MemFlags.READ_WRITE, memSize);
	        d_voxelOccupiedScan = OCLManager.Context.CreateBuffer(MemFlags.READ_WRITE, memSize);
	        d_compVoxelArray = OCLManager.Context.CreateBuffer(MemFlags.READ_WRITE, memSize);
			
			oclMCStatus = 1;
		}catch{
			oclMCStatus = -1;
			Debug.LogError("[OCLLOG]OclMarchingCubes is not available.");
			return false;
		}		
		// Other const helper -- TODO : size should be computed according to IMG_FORMAT
		volumeZeroConst = new byte[CHUNK_VOXEL_NUM_REAL*4];	// all zero			
		numChunks = 0;
		return true;
	}
	public static void AddChunkVolumeData<T>(T[] volumeData)
	{
		int[] region = new int[3]{CHUNK_VOXEL_DIM_LEN_REAL,CHUNK_VOXEL_DIM_LEN_REAL,CHUNK_VOXEL_DIM_LEN_REAL};
		int[] origin = new int[3]{	(int)(CHUNK_VOXEL_DIM_LEN_REAL * (numChunks&VOLUME_CHUNK_DIM_LEN_MASK.S0)),
									(int)(CHUNK_VOXEL_DIM_LEN_REAL * ((numChunks>>(int)VOLUME_CHUNK_DIM_LEN_SHIFT_PLUS.S0)&VOLUME_CHUNK_DIM_LEN_MASK.S1)),
									(int)(CHUNK_VOXEL_DIM_LEN_REAL * ((numChunks>>(int)VOLUME_CHUNK_DIM_LEN_SHIFT_PLUS.S1)&VOLUME_CHUNK_DIM_LEN_MASK.S2)),
									};
		int row_pitch = CHUNK_VOXEL_DIM_LEN_REAL;
		int slice_pitch = CHUNK_VOXEL_DIM_LEN_REAL*CHUNK_VOXEL_DIM_LEN_REAL;

		GCHandle hVolumeData = GCHandle.Alloc(volumeData, GCHandleType.Pinned);
		IntPtr pVolData = hVolumeData.AddrOfPinnedObject();
		if(bImageFormatSupported)
		{
			//OCLComQueue.EnqueueWriteImage(d_volume,false,origin,region,row_pitch,slice_pitch,pVolData);
			OCLComQueue.EnqueueWriteImage(d_volume,false,origin,region,0,0,pVolData);
		}
		else
		{
			origin[0] *= VOXEL_SIZE;
			region[0] *= VOXEL_SIZE;
			OCLComQueue.EnqueueWriteBufferRect(d_volume,false,origin, new int[3]{0,0,0}, region, 
				VOLUME_VOXEL_X_LEN_REAL*VOXEL_SIZE, VOLUME_VOXEL_X_LEN_REAL*VOLUME_VOXEL_Y_LEN_REAL*VOXEL_SIZE, 
				row_pitch*VOXEL_SIZE, slice_pitch*VOXEL_SIZE, pVolData);
		}
		hVolumeData.Free();
        numChunks ++;
	}
    public static void Cleanup()
    {
		if(OCLManager == null)	return;	// not init
		
		oclScanLaucher.closeScan();
		
		hPosArray.Free();
		hNorm01Array.Free();
		hNorm2tArray.Free();

	    if(d_pos!= null)	{d_pos.Dispose();d_pos = null;}	       // out
	    if(d_norm01!= null)	{d_norm01.Dispose();d_norm01 = null;}	   // out
		if(d_norm2t!= null)	{d_norm2t.Dispose();d_norm2t = null;}
	    if(d_volume!= null)	{d_volume.Dispose();d_volume = null;}
	    if(d_voxelVerts!= null)	{d_voxelVerts.Dispose();d_voxelVerts = null;}
	    if(d_voxelVertsScan!= null)	{d_voxelVertsScan.Dispose();d_voxelVertsScan = null;}
	    if(d_voxelOccupied!= null)	{d_voxelOccupied.Dispose();d_voxelOccupied = null;}
	    if(d_voxelOccupiedScan!= null)	{d_voxelOccupiedScan.Dispose();d_voxelOccupiedScan = null;}
	    if(d_compVoxelArray!= null)	{d_compVoxelArray.Dispose();d_compVoxelArray = null;}
		
	    if(classifyVoxelKernel!= null)	{classifyVoxelKernel.Dispose();classifyVoxelKernel = null;}
	    if(compactVoxelsKernel!= null)	{compactVoxelsKernel.Dispose();compactVoxelsKernel = null;}
	    if(generateTriangles2Kernel!= null)	{generateTriangles2Kernel.Dispose();generateTriangles2Kernel = null;}
    }
	
    ////////////////////////////////////////////////////////////////////////////////
    //! Run the Cuda part of the computation 3-30ms
    ////////////////////////////////////////////////////////////////////////////////
    public static void computeIsosurface()
    {
		IEnumerator e = computeIsosurfaceAsyn();
		while(e.MoveNext());
	}

    public static IEnumerator computeIsosurfaceAsyn()
    {
		if(numChunks == 0)			yield break;
		
		// 8*8*8==512==4*128, so if groupsize >128, should be aligned by 4*groupsize
		OpenCLNet.Event evRead;
		int numVoxels = (int)numChunks * CHUNK_VOXEL_NUM;
        int groupSize = oclScanLaucher.prepareScan(numVoxels);
		int numVoxelsToScan = (4 * groupSize);
		while(numVoxelsToScan < numVoxels){
			numVoxelsToScan <<= 1;
		}
		
		while((numVoxels&(4*groupSize-1))!=0){
			AddChunkVolumeData(volumeZeroConst);
			numVoxels = (int)numChunks * CHUNK_VOXEL_NUM;
#if PERF_LOG
			Debug.Log("[MCLOG]Add a dummy chunk");
#endif
		}
		//int numGroups = numVoxels/(4 * groupSize);
		globalWorkSize[0] = (int)numChunks * CHUNK_VOXEL_DIM_LEN;

		// calculate number of vertices need per voxel
        launch_classifyVoxel(	globalWorkSize, localWorkSize, 
						    	d_voxelVerts, d_voxelOccupied, d_volume,
						    	CHUNK_VOXEL_DIM_LEN_REAL, CHUNK_VOXEL_DIM_LEN_SHIFT_PLUS, VOLUME_CHUNK_DIM_LEN_SHIFT_PLUS, VOLUME_CHUNK_DIM_LEN_MASK,
                            	isoValue);
        // scan voxel occupied array
		oclScanLaucher.scanExclusiveLarge_1Batch(
                   OCLComQueue,
                   d_voxelOccupiedScan,
                   d_voxelOccupied,
		           (int)numVoxels,
                   numVoxelsToScan,
		           1);
		// compact voxel index array
        launch_compactVoxels(   globalWorkSize, localWorkSize, 
								d_compVoxelArray, d_voxelOccupied, d_voxelOccupiedScan,
                                CHUNK_VOXEL_DIM_LEN_SHIFT_PLUS);
		// scan voxel vertex count array
		oclScanLaucher.scanExclusiveLarge_1Batch(
                   OCLComQueue,
                   d_voxelVertsScan,
                   d_voxelVerts,
		           (int)numVoxels,
                   numVoxelsToScan,
		           0);
		
        // readback total number of vertices
        {
			evRead = oclScanLaucher.UpdateOfsnSumDataAsyn(OCLComQueue, CHUNK_VOXEL_NUM);
#if MC_COMPUTE_ASYNC
			OCLComQueue.Flush();
			while(evRead.ExecutionStatus != ExecutionStatus.COMPLETE){				yield return 0;			}
#else
			OCLComQueue.Finish();
#endif
			activeVoxels = oclScanLaucher.sumData[1];	// read the result of the first scan
            totalVerts = oclScanLaucher.sumData[0];
        }
		if(totalVerts > 0)
		{
			if(totalVerts > MAX_VERTS)
			{
				string errStr = "[OCL]Error: total verts is out of bounds:"+totalVerts;
				for(int i = 0; i < numChunks; i++)
				{
					errStr += System.Environment.NewLine + oclScanLaucher.ofsData[i];
				}
				errStr += System.Environment.NewLine + activeVoxels +"//"+ totalVerts;
				Debug.LogWarning(errStr);
				oclScanLaucher.ClearOfsData();
				numChunks = 0;
				throw new System.ArgumentOutOfRangeException("totalVerts", totalVerts, errStr);
				//yield break;

				//totalVerts = MAX_VERTS;
			}
	        // generate triangles, writing to vertex buffers
	        int[] grid2 = new int[2] { (int)activeVoxels, 1 };
	        while(grid2[0] > 65535) {	// TODO :bug fix
	            grid2[0] = (grid2[0]+1)>>1; 
	            grid2[1] <<= 1;
	        }
			grid2[0] = (grid2[0]+NTHREADS_MASK)&~NTHREADS_MASK;
	        launch_generateTriangles2(	grid2, new int[]{NTHREADS,1},
										d_pos, d_norm01, d_norm2t, d_compVoxelArray, d_voxelVertsScan, d_volume,
	                                    CHUNK_VOXEL_DIM_LEN_REAL, CHUNK_VOXEL_DIM_LEN_MASK, CHUNK_VOXEL_DIM_LEN_SHIFT_PLUS, VOLUME_CHUNK_DIM_LEN_SHIFT_PLUS, VOLUME_CHUNK_DIM_LEN_MASK,
		                                isoValue, (uint)activeVoxels, (uint)totalVerts);
	
			// readback total number of vertices
            OCLComQueue.EnqueueReadBuffer(d_pos, false, 0, totalVerts * 3 * sizeof(float), hPosArray.AddrOfPinnedObject());
			OCLComQueue.EnqueueReadBuffer(d_norm01, false, 0, totalVerts * 2 * sizeof(float), hNorm01Array.AddrOfPinnedObject());
            OCLComQueue.EnqueueReadBuffer(d_norm2t, false, 0, totalVerts * 2 * sizeof(float), hNorm2tArray.AddrOfPinnedObject(), 0, null, out evRead);
			// Only AMD CPU ocl do not need read buffer, calling EnqueueMarker is enough
			//	OCLComQueue.EnqueueMarker(out evRead);
#if MC_COMPUTE_ASYNC
			OCLComQueue.Flush();
			while(evRead.ExecutionStatus != ExecutionStatus.COMPLETE){				yield return 0;			}
#else
			OCLComQueue.Finish();
#endif
		}

		numChunks = 0;
    }

	public static void Reset()
	{
		numChunks = 0;
	}
}
