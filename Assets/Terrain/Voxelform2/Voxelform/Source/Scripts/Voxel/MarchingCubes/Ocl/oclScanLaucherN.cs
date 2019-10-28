#define USE_UNITY
//#define CL_DEBUG
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

// CSharp version of NV's scan
public static partial class oclScanLaucher
{
#if false
	public class OffsetData{
		public int ofs = 1;
		public int this[int pos]{
			get{
				return ofsDataArray[ofs*pos];
			}
		}
	}
	public class SumData{
		public int this[int pos]{
			get{
                return ofsDataArray[OFS_DATA_SIZE + pos];
			}
		}
	}
	public static void ClearOfsData()
	{
		Array.Clear(ofsDataArray, 0, ofsDataArray.Length);
	}

	static int[] WORKGROUP_SIZE_AVAILABLE = {        64,        128,        256,        512    };
    static Kernel[] ckScanExclusiveLocal1Array = new Kernel[4], ckScanExclusiveLocal2Array = new Kernel[4], ckUniformUpdateArray = new Kernel[4];
    static Kernel ckScanExclusiveLocal1, ckScanExclusiveLocal2, ckUniformUpdate;
    const int MAX_BATCH_ELEMENTS = 64 * 1048576;
    const int BUFFER_LENGTH = 4096;//+2048+8;	// 	OriginalSize(MaxOf max_voxel_num/(workgroup_size*4))=4096 (not precise, just reserve)
												//	+OfsDataSize(max_voxel_num/512)=2048
												//	+SumDataSize(8)= 8
	static int OFS_DATA_SIZE = 2048;
	const int SUM_DATA_SIZE = 8;

    //All three kernels run 512 threads per workgroup
    //Must be a power of two
    public static int WORKGROUP_SIZE;
#if CL_DEBUG
    static int MIN_SHORT_ARRAY_SIZE = 4;
    static int MAX_SHORT_ARRAY_SIZE = 4 * WORKGROUP_SIZE;
    static int MIN_LARGE_ARRAY_SIZE = 8 * WORKGROUP_SIZE;
    static int MAX_LARGE_ARRAY_SIZE = 4 * WORKGROUP_SIZE * WORKGROUP_SIZE;
	static GCHandle hVoxelScan;
	static uint[] voxelScanData;
#endif
	static Mem d_Buffer;
	static int[] ofsDataArray;
	static GCHandle hofsDataArray;
	//properties
	public static OffsetData ofsData = new OffsetData();
	public static SumData sumData = new SumData();
	public static Mem SumDataBuffer{	get{ return d_Buffer;	}	}
	public static int OffsetOfSumDataBuffer(int offset){	return BUFFER_LENGTH+OFS_DATA_SIZE+offset;	}
	//functions
    public static void initScan(OpenCLManager OCLManager, CommandQueue cqCommandQueue, int numDataShift, int chunkSize)
    {
        Debug.Log(" ...loading Scan.cl and creating scan program then build it\n");
		Program cpProgram;
        for (int i = 0; i < WORKGROUP_SIZE_AVAILABLE.Length; i++)
        {
            OCLManager.BuildOptions = "-D WORKGROUP_SIZE=" + WORKGROUP_SIZE_AVAILABLE[i];
            OCLManager.Defines = "";
			try{
				TextAsset srcKernel = Resources.Load("OclKernel/Scan") as TextAsset;
				cpProgram = OCLManager.CompileSource(srcKernel.text);	// TODO : seperate 4 file to reuse binary
				srcKernel = null;
			}
			catch(OpenCLBuildException e){
				string log = "CL Kernel Error: ";
				for(int j = 0; j < e.BuildLogs.Count; j++)
					log += e.BuildLogs[j];
				Debug.LogError(log);

				throw;
				return;
			}

            ckScanExclusiveLocal1Array[i] = cpProgram.CreateKernel("scanExclusiveLocal1");
            ckScanExclusiveLocal2Array[i] = cpProgram.CreateKernel("scanExclusiveLocal2");
            ckUniformUpdateArray[i] = cpProgram.CreateKernel("uniformUpdate");
        }
		cpProgram = null;

        /* TODO : check
            Debug.Log( " ...checking minimum supported workgroup size\n");
            //Check for work group size
            cl_device_id device;
            uint szScanExclusiveLocal1, szScanExclusiveLocal2, szUniformUpdate;
            ciErrNum  = clGetCommandQueueInfo(cqParamCommandQue, CL_QUEUE_DEVICE, sizeof(cl_device_id), &device, NULL);
            ciErrNum |= clGetKernelWorkGroupInfo(ckScanExclusiveLocal1,  device, CL_KERNEL_WORK_GROUP_SIZE, sizeof(uint), &szScanExclusiveLocal1, NULL);
            ciErrNum |= clGetKernelWorkGroupInfo(ckScanExclusiveLocal2, device, CL_KERNEL_WORK_GROUP_SIZE, sizeof(uint), &szScanExclusiveLocal2, NULL);
            ciErrNum |= clGetKernelWorkGroupInfo(ckUniformUpdate, device, CL_KERNEL_WORK_GROUP_SIZE, sizeof(uint), &szUniformUpdate, NULL);

            if( (szScanExclusiveLocal1 < WORKGROUP_SIZE) || (szScanExclusiveLocal2 < WORKGROUP_SIZE) || (szUniformUpdate < WORKGROUP_SIZE) ){
                Debug.Log("ERROR: Minimum work-group size %u required by this application is not supported on this device.\n", WORKGROUP_SIZE);
                return false;
            }
        */

        Debug.Log(" ...allocating internal buffers\n");
		// allocate offset data
		OFS_DATA_SIZE = (1<<(numDataShift-9));	// (max_voxel_num/512)
		ofsDataArray = new int[OFS_DATA_SIZE + SUM_DATA_SIZE];
		hofsDataArray = GCHandle.Alloc(ofsDataArray, GCHandleType.Pinned);
        d_Buffer = OCLManager.Context.CreateBuffer(MemFlags.READ_WRITE, (BUFFER_LENGTH + OFS_DATA_SIZE + SUM_DATA_SIZE) * sizeof(uint));
	}
    public static int prepareScan(int numData)
    {
		int groupSize = 512;
        if (numData <= 0x4000) groupSize = 64;
        else if (numData <= 0x10000) groupSize = 128;
        else if (numData <= 0x40000) groupSize = 256;
		
        for (int i = 0; i < WORKGROUP_SIZE_AVAILABLE.Length; i++)
        {
            if (groupSize == WORKGROUP_SIZE_AVAILABLE[i])
            {
                WORKGROUP_SIZE = WORKGROUP_SIZE_AVAILABLE[i];
                ckScanExclusiveLocal1 = ckScanExclusiveLocal1Array[i];
                ckScanExclusiveLocal2 = ckScanExclusiveLocal2Array[i];
                ckUniformUpdate = ckUniformUpdateArray[i];
#if CL_DEBUG
                MIN_SHORT_ARRAY_SIZE = 4;
                MAX_SHORT_ARRAY_SIZE = 4 * WORKGROUP_SIZE;
                MIN_LARGE_ARRAY_SIZE = 8 * WORKGROUP_SIZE;
                MAX_LARGE_ARRAY_SIZE = 4 * WORKGROUP_SIZE * WORKGROUP_SIZE;
					
				//debug
				voxelScanData = new uint[2048*512];
				hVoxelScan = GCHandle.Alloc(voxelScanData,GCHandleType.Pinned);
#endif
				break;
            }
        }
		return groupSize;
    }
    public static void closeScan()
    {
		hofsDataArray.Free();
		
        d_Buffer.Dispose();
		d_Buffer = null;
        for (int i = 0; i < WORKGROUP_SIZE_AVAILABLE.Length; i++)
        {
            ckUniformUpdate.Dispose();
            ckScanExclusiveLocal2.Dispose();
            ckScanExclusiveLocal1.Dispose();
        }
    }
	public static OpenCLNet.Event UpdateOfsnSumDataAsyn(CommandQueue cqCommandQueue, int chunkSize){
        ofsData.ofs = chunkSize >> 9;   // 512
		
		OpenCLNet.Event eventRead = null;
        cqCommandQueue.EnqueueReadBuffer(SumDataBuffer, false, BUFFER_LENGTH*sizeof(uint), ofsDataArray.Length * sizeof(uint), hofsDataArray.AddrOfPinnedObject(), 0, null, out eventRead);
		return eventRead;
	}
    ////////////////////////////////////////////////////////////////////////////////
    // Common definitions
    ////////////////////////////////////////////////////////////////////////////////
    static int iSnapUp(int dividend, int divisor)
    {
        return ((dividend % divisor) == 0) ? dividend : (dividend - dividend % divisor + divisor);
    }

#if CL_DEBUG
    static int factorRadix2(ref int log2L, int L)
    {
        if (L == 0)
        {
            log2L = 0;
            return 0;
        }
        else
        {
            for (log2L = 0; (L & 1) == 0; L >>= 1, log2L++) ;
            return L;
        }
    }
#endif
    ////////////////////////////////////////////////////////////////////////////////
    // Short scan launcher
    ////////////////////////////////////////////////////////////////////////////////
    static int scanExclusiveLocal1(
        CommandQueue cqCommandQueue,
        Mem d_Dst,
        Mem d_Src,
        int n,
        int size
    )
    {
        int[] localWorkSize = new int[1], globalWorkSize = new int[1];

        ckScanExclusiveLocal1.SetArg(0, d_Dst);
        ckScanExclusiveLocal1.SetArg(1, d_Src);
        ckScanExclusiveLocal1.SetArg(2, (IntPtr)(2 * WORKGROUP_SIZE * sizeof(uint)), IntPtr.Zero);
        ckScanExclusiveLocal1.SetArg(3, size);

        localWorkSize[0] = WORKGROUP_SIZE;
        globalWorkSize[0] = (n * size) / 4;

        cqCommandQueue.EnqueueNDRangeKernel(ckScanExclusiveLocal1, 1, null, globalWorkSize, localWorkSize);

        return localWorkSize[0];
    }

    public static int scanExclusiveShort(
        CommandQueue cqCommandQueue,
        Mem d_Dst,
        Mem d_Src,
        int batchSize,
        int arrayLength
    )
    {
#if CL_DEBUG
        //Check power-of-two factorization
        int log2L = 0;
        int factorizationRemainder = factorRadix2(ref log2L, arrayLength);

        if (factorizationRemainder != 1 ||
            (arrayLength < MIN_SHORT_ARRAY_SIZE) || (arrayLength > MAX_SHORT_ARRAY_SIZE) ||
            (batchSize * arrayLength) > MAX_BATCH_ELEMENTS ||
            (batchSize * arrayLength) % (4 * WORKGROUP_SIZE) != 0
            )
            return -1;
#endif
        return scanExclusiveLocal1(
            cqCommandQueue,
            d_Dst,
            d_Src,
            batchSize,
            arrayLength
            );
    }

    ////////////////////////////////////////////////////////////////////////////////
    // Large scan launcher
    ////////////////////////////////////////////////////////////////////////////////
    static void scanExclusiveLocal2_1Batch(
        CommandQueue cqCommandQueue,
        Mem d_Buffer,
        Mem d_Dst,
        Mem d_Src,
        int arrayLen,
		int arrayLenPower2,
	    int sumDataPos
    )
    {
        int[] localWorkSize = new int[1], globalWorkSize = new int[1];

        ckScanExclusiveLocal2.SetArg(0, d_Buffer);
        ckScanExclusiveLocal2.SetArg(1, d_Dst);
        ckScanExclusiveLocal2.SetArg(2, d_Src);
        ckScanExclusiveLocal2.SetArg(3, (IntPtr)(2 * WORKGROUP_SIZE * sizeof(uint)), IntPtr.Zero);
        ckScanExclusiveLocal2.SetArg(4, arrayLen);
        ckScanExclusiveLocal2.SetArg(5, arrayLenPower2);
		ckScanExclusiveLocal2.SetArg(6, sumDataPos);

        localWorkSize[0] = WORKGROUP_SIZE;
        globalWorkSize[0] = iSnapUp(arrayLenPower2, WORKGROUP_SIZE);

        cqCommandQueue.EnqueueNDRangeKernel(ckScanExclusiveLocal2, 1, null, globalWorkSize, localWorkSize);
    }

    static void uniformUpdate_1Batch(
        CommandQueue cqCommandQueue,
        Mem d_Dst,
        Mem d_Buffer,
        int n,
	    int ofsDataOffset
    )
    {
        int[] localWorkSize = new int[1], globalWorkSize = new int[1];

        ckUniformUpdate.SetArg(0, d_Dst);
        ckUniformUpdate.SetArg(1, d_Buffer);
        ckUniformUpdate.SetArg(2, ofsDataOffset);

        localWorkSize[0] = WORKGROUP_SIZE;
        globalWorkSize[0] = n * WORKGROUP_SIZE;

        cqCommandQueue.EnqueueNDRangeKernel(ckUniformUpdate, 1, null, globalWorkSize, localWorkSize);
    }

    public static void scanExclusiveLarge_1Batch(	// for batch size == 1
        CommandQueue cqCommandQueue,
        Mem d_Dst,
        Mem d_Src,
        int arrayLen,
		int arrayLenPower2,
	    int sumDataPos	// only 0-sumDataSize are valid
    )
    {
#if CL_DEBUG
        //Check power-of-two factorization
        int log2L = 0;
        int factorizationRemainder = factorRadix2(ref log2L, arrayLenPower2);
        if (factorizationRemainder != 1 ||
            (arrayLenPower2 < MIN_LARGE_ARRAY_SIZE) || (arrayLenPower2 > MAX_LARGE_ARRAY_SIZE) ||
            arrayLenPower2 > MAX_BATCH_ELEMENTS
            )
            return;
#endif
        scanExclusiveLocal1(
           cqCommandQueue,
           d_Dst,
           d_Src,
           arrayLen / (4 * WORKGROUP_SIZE),
           4 * WORKGROUP_SIZE
        );
#if CL_DEBUG
        cqCommandQueue.EnqueueReadBuffer(d_Dst, true, 0*sizeof(uint), arrayLenPower2 * sizeof(uint), hVoxelScan.AddrOfPinnedObject());
#endif
        scanExclusiveLocal2_1Batch(
            cqCommandQueue,
            d_Buffer,
            d_Dst,
            d_Src,
            arrayLen / (4 * WORKGROUP_SIZE),
            arrayLenPower2 / (4 * WORKGROUP_SIZE),
            BUFFER_LENGTH+OFS_DATA_SIZE+sumDataPos
        );
#if CL_DEBUG
        cqCommandQueue.EnqueueReadBuffer(d_Buffer, true, BUFFER_LENGTH * sizeof(uint), ofsDataArray.Length * sizeof(uint), hofsDataArray.AddrOfPinnedObject());
#endif
        uniformUpdate_1Batch(
            cqCommandQueue,
            d_Dst,
            d_Buffer,
            arrayLen / (4 * WORKGROUP_SIZE),
	    	BUFFER_LENGTH
        );
#if CL_DEBUG
        cqCommandQueue.EnqueueReadBuffer(d_Buffer, true, BUFFER_LENGTH * sizeof(uint), ofsDataArray.Length * sizeof(uint), hofsDataArray.AddrOfPinnedObject());
#endif
    }
#endif
}
