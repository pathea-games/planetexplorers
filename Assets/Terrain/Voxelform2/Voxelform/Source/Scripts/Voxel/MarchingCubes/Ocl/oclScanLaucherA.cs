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
#if true
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

	static Kernel ckScanLargeKernel;	//scan large arrays
	static Kernel ckBlockAddiKernel;	//block addition
	static Kernel ckPrefixSumKernel;	//prefix scan
    static Kernel zeromemKernel;
	static List<Mem> blockSumBufferList = new List<Mem>();
	static List<Mem> outBufferList = new List<Mem>();	// Retain a mem for outbuffer
	static int MaxGroupSize;
	static int blockSize;
	static int blockSizeShift;
	static int blockSizeUnMask;
	static List<int> listComputeArrayLen = new List<int>(8);
	static int pass;
	static int SUM_DATA_SIZE = 8;
	static int OFS_DATA_SIZE;
	static int[] ofsDataArray;
	static GCHandle hofsDataArray;
#if CL_DEBUG
	static GCHandle houtArray,hsumArray;
	static uint[] outArray = new uint[32*32*1024];
	static uint[] sumArray = new uint[4*1024];
#endif
	//properties
	public static OffsetData ofsData = new OffsetData();
	public static SumData sumData = new SumData();
	public static Mem SumDataBuffer{	get{	return outBufferList[1];	}	}
	public static int OffsetOfSumDataBuffer(int offset){	return OFS_DATA_SIZE+offset;	}
	//functions
    public static void initScan(OpenCLManager OCLManager, CommandQueue cqCommandQueue, int numDataShift, int chunkSize){
        OCLManager.BuildOptions = "";
        OCLManager.Defines = "";
		Program cpProgram;
		try{
			TextAsset srcKernel = Resources.Load("OclKernel/ScanLargeArrays_Kernels") as TextAsset;
			Debug.Log("[OCLLOG]Build kernel:Scan");
			cpProgram = OCLManager.CompileSource(srcKernel.text);
			srcKernel = null;
		}
		catch(OpenCLBuildException e){
			string log = "[OCLLOG]Kernel Error: ";
			for(int i = 0; i < e.BuildLogs.Count; i++)
				log += e.BuildLogs[i];
			Debug.LogError(log);

			throw;
			//return;
		}
        ckScanLargeKernel = cpProgram.CreateKernel("ScanLargeArrays");
        ckBlockAddiKernel = cpProgram.CreateKernel("blockAddition");
        ckPrefixSumKernel = cpProgram.CreateKernel("prefixSum");
    	zeromemKernel = cpProgram.CreateKernel("zeromem");
		cpProgram = null;
		
		MaxGroupSize = (int)cqCommandQueue.Device.MaxWorkGroupSize;
		int maxWorkItemSize = cqCommandQueue.Device.MaxWorkItemSizes[0].ToInt32();
		int maxWorkItemSize1 = cqCommandQueue.Device.MaxWorkItemSizes[1].ToInt32();
		int maxWorkItemSize2 = cqCommandQueue.Device.MaxWorkItemSizes[2].ToInt32();
		Debug.Log("[OCLLOG]SCAN MaxGroup:"+MaxGroupSize+" MaxWorkItem:"+maxWorkItemSize+","+maxWorkItemSize1+","+maxWorkItemSize2);
		if(cqCommandQueue.Device.Name.Contains("RV7"))
		{
			MaxGroupSize = maxWorkItemSize = 32;
			Debug.Log("[OCLLOG]SCAN RV7xx lower MaxGroup:"+MaxGroupSize+"MaxWorkItem:"+maxWorkItemSize);
		}
#if UNITY_STANDALONE_OSX
		else
		{
			MaxGroupSize = maxWorkItemSize = maxWorkItemSize/4;
			Debug.Log("[OCLLOG]SCAN Apple lower(/4) MaxGroup:"+MaxGroupSize+"MaxWorkItem:"+maxWorkItemSize);
		}
#endif
        if (maxWorkItemSize > chunkSize) maxWorkItemSize = chunkSize;
		blockSize = 1;		blockSizeShift = 0;
		while(blockSize < maxWorkItemSize){		blockSize<<=1;			blockSizeShift++;	}
		//blockSize >>= 1;blockSizeShift--;
		blockSizeUnMask = ~(blockSize-1);
		
		// compute buffer length and offset
		Mem buffBlockSum;
		Mem buffTmpOutput;
		bool bSumDataAllocated = false;
		blockSumBufferList.Add(null);	// Add a placeholder for inputBuffer
        outBufferList.Add(null);	// Add a placeholder
		if( numDataShift < blockSizeShift){
			numDataShift = blockSizeShift;	// at least 1 even if blockSizeShift >= numDataShift
		}
		do{
			numDataShift -= blockSizeShift;
			buffBlockSum = OCLManager.Context.CreateBuffer(MemFlags.READ_WRITE, (1<<numDataShift) * sizeof(uint));
			blockSumBufferList.Add(buffBlockSum);
			if(bSumDataAllocated == false){
				OFS_DATA_SIZE = (1<<numDataShift);
                ofsDataArray = new int[OFS_DATA_SIZE + SUM_DATA_SIZE];
				hofsDataArray = GCHandle.Alloc(ofsDataArray, GCHandleType.Pinned);
                buffTmpOutput = OCLManager.Context.CreateBuffer(MemFlags.READ_WRITE, (OFS_DATA_SIZE + SUM_DATA_SIZE) * sizeof(uint));	
				bSumDataAllocated = true;
			}
			else{
				buffTmpOutput = OCLManager.Context.CreateBuffer(MemFlags.READ_WRITE, (1<<numDataShift) * sizeof(uint));	
			}
			outBufferList.Add(buffTmpOutput);
		}while(numDataShift > blockSizeShift);
#if CL_DEBUG
		houtArray = GCHandle.Alloc(outArray, GCHandleType.Pinned);
		hsumArray = GCHandle.Alloc(sumArray, GCHandleType.Pinned);
#endif
	}
    public static int prepareScan(int numData){
		listComputeArrayLen.Clear();
		// bscan
		do{
			numData = (numData+blockSize-1) & blockSizeUnMask;
			listComputeArrayLen.Add(numData);			
			numData >>= blockSizeShift;
		}while(numData > blockSize);
		pass = listComputeArrayLen.Count;
		
		// pscan
		int n = 0;
		while(numData > 1){
			n++;
			numData >>= 1;
		}
		listComputeArrayLen.Add(1<<(n+1));
		
		return blockSize;
	}
    public static void closeScan(){
		hofsDataArray.Free();
		if (blockSumBufferList.Count > 0)
			blockSumBufferList[0] = null;
		
		for(int i = 1; i < blockSumBufferList.Count; i++){
			blockSumBufferList[i].Dispose();
			blockSumBufferList[i] = null;
		}
		blockSumBufferList.Clear();
		
		if (outBufferList.Count > 0)
			outBufferList[0] = null;
		
		for(int i = 1; i < outBufferList.Count; i++){
			outBufferList[i].Dispose();
			outBufferList[i] = null;
		}
		outBufferList.Clear();
		if(ckScanLargeKernel!=null){	ckScanLargeKernel.Dispose(); ckScanLargeKernel = null;	}
		if(ckBlockAddiKernel!=null){	ckBlockAddiKernel.Dispose(); ckBlockAddiKernel = null;	}
		if(ckPrefixSumKernel!=null){	ckPrefixSumKernel.Dispose(); ckPrefixSumKernel = null;	}
		if(zeromemKernel!=null){		zeromemKernel.Dispose();	 zeromemKernel = null;	}
	}
	public static OpenCLNet.Event UpdateOfsnSumDataAsyn(CommandQueue cqCommandQueue, int chunkSize){
		ofsData.ofs = chunkSize>>blockSizeShift;
		
		OpenCLNet.Event eventRead = null;
		cqCommandQueue.EnqueueReadBuffer(SumDataBuffer, false, 0, ofsDataArray.Length * sizeof(uint), hofsDataArray.AddrOfPinnedObject(), 0, null, out eventRead);
		return eventRead;
	}
	static void	zero_mem(CommandQueue cqCommandQueue, Mem uArray, int offset, int size)
	{
		zeromemKernel.SetArg(0, uArray);
		zeromemKernel.SetArg(1, offset);

        cqCommandQueue.EnqueueNDRangeKernel(zeromemKernel, 1, null, new int[1]{size}, new int[1]{1});
	}
	/* Scan array, scan batch is blockSize */
	static void ScanLargeArray(CommandQueue cqCommandQueue,
		                       int len,
		                       Mem inputBuffer, 
		                       Mem outputBuffer,
		                       Mem blockSumBuffer)
	{
		int maxGlobalSize = blockSize*MaxGroupSize;
        int[] localWorkSize = new int[1], globalWorkSize = new int[1];
        localWorkSize[0] = blockSize / 2;
        globalWorkSize[0] = len > maxGlobalSize ? (maxGlobalSize/2) : (len / 2);
	
	    /* Set appropriate arguments to the kernel */
		ckScanLargeKernel.SetArg(0, outputBuffer);
		ckScanLargeKernel.SetArg(1, inputBuffer);
		ckScanLargeKernel.SetArg(2, (IntPtr)(blockSize * sizeof(uint)), IntPtr.Zero);
		ckScanLargeKernel.SetArg(3, blockSize);
		ckScanLargeKernel.SetArg(4, 0);		// start_index of input
		ckScanLargeKernel.SetArg(5, 0);		// start_index of sumBuf
		ckScanLargeKernel.SetArg(6, blockSumBuffer);
		
		//Debug.Log("[OCLLOG]Global:"+globalWorkSize[0]+"Local:"+localWorkSize[0]);
        cqCommandQueue.EnqueueNDRangeKernel(ckScanLargeKernel, 1, null, globalWorkSize, localWorkSize);
		
		int n = 1;
		while(len > maxGlobalSize)
		{
			len -= maxGlobalSize;
	        globalWorkSize[0] = len > maxGlobalSize ? (maxGlobalSize/2) : (len / 2);
			ckScanLargeKernel.SetArg(4, n*maxGlobalSize);	// start_index of input
			ckScanLargeKernel.SetArg(5, n*MaxGroupSize);	// start_index of sumBuf
			//Debug.Log("[OCLLOG]loop Global:"+globalWorkSize[0]+"Local:"+localWorkSize[0]);
        	cqCommandQueue.EnqueueNDRangeKernel(ckScanLargeKernel, 1, null, globalWorkSize, localWorkSize);
			n++;
		}
	}
	
	static void	PrefixSum(CommandQueue cqCommandQueue,
		                   int len,
	                       Mem inputBuffer,
	                       Mem outputBuffer,
	                       Mem sumDataBuffer,
	                       int sumDataOffset)
	{
        int[] localWorkSize = new int[1], globalWorkSize = new int[1];
        localWorkSize[0] = len / 2;
        globalWorkSize[0] = len / 2;

		/* Set appropriate arguments to the kernel */
		ckPrefixSumKernel.SetArg(0, outputBuffer);
		ckPrefixSumKernel.SetArg(1, inputBuffer);
		ckPrefixSumKernel.SetArg(2, (IntPtr)(len * sizeof(uint)), IntPtr.Zero);
		ckPrefixSumKernel.SetArg(3, len);
		ckPrefixSumKernel.SetArg(4, sumDataBuffer);
		ckPrefixSumKernel.SetArg(5, sumDataOffset);
	
        cqCommandQueue.EnqueueNDRangeKernel(ckPrefixSumKernel, 1, null, globalWorkSize, localWorkSize);
	}
	
	static void BlockAddition(CommandQueue cqCommandQueue,
		                       int len,
	                           Mem inputBuffer,
	                           Mem outputBuffer)
	{
		int maxGlobalSize = blockSize*MaxGroupSize;
		int[] localWorkSize = new int[1], globalWorkSize = new int[1];
        localWorkSize[0] = blockSize;
        globalWorkSize[0] = len > maxGlobalSize ? maxGlobalSize : len;

		/*** Set appropriate arguments to the kernel ***/
		ckBlockAddiKernel.SetArg(0, inputBuffer);
		ckBlockAddiKernel.SetArg(1, outputBuffer);
		ckBlockAddiKernel.SetArg(2, 0);		// start_index of input
		ckBlockAddiKernel.SetArg(3, 0);		// start_index of sumBuf
		
        cqCommandQueue.EnqueueNDRangeKernel(ckBlockAddiKernel, 1, null, globalWorkSize, localWorkSize);
		
		int n = 1;
		while(len > maxGlobalSize)
		{
			len -= maxGlobalSize;
	        globalWorkSize[0] = len > maxGlobalSize ? maxGlobalSize : len;
			ckBlockAddiKernel.SetArg(2, n*maxGlobalSize);	// start_index of input
			ckBlockAddiKernel.SetArg(3, n*MaxGroupSize);	// start_index of sumBuf
        	cqCommandQueue.EnqueueNDRangeKernel(ckBlockAddiKernel, 1, null, globalWorkSize, localWorkSize);
			n++;
		}
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
		blockSumBufferList[0] = d_Src;
		outBufferList[0] = d_Dst;
		
	    /* Do block-wise sum */
	    for(int i = 0; i < (int)pass; i++)
	    {
			if(listComputeArrayLen[i] > arrayLen){
				zero_mem(cqCommandQueue, blockSumBufferList[i], arrayLen, listComputeArrayLen[i]-arrayLen);
			}
			arrayLen = listComputeArrayLen[i]>>blockSizeShift;
	        ScanLargeArray(cqCommandQueue, listComputeArrayLen[i], blockSumBufferList[i], outBufferList[i], blockSumBufferList[i+1]);
#if CL_DEBUG
        	cqCommandQueue.EnqueueReadBuffer(outBufferList[i], true, 0, listComputeArrayLen[i] * sizeof(uint), houtArray.AddrOfPinnedObject());
        	cqCommandQueue.EnqueueReadBuffer(blockSumBufferList[i+1], true, 0, (listComputeArrayLen[i]>>blockSizeShift) * sizeof(uint), hsumArray.AddrOfPinnedObject());
#endif
	    }
	
		/* length == arrayLen>>(blockSizeShift*pass) */
	    /* Do scan to outBufferList[pass], here length should be power of 2 */
		/* TODO : mod code for those not power of 2 */
	    /* use tmpOutputBuffer as sumDataBuffer */
		if(listComputeArrayLen[pass] > arrayLen){
			zero_mem(cqCommandQueue, blockSumBufferList[pass], arrayLen, listComputeArrayLen[pass]-arrayLen);
		}
		PrefixSum(cqCommandQueue, listComputeArrayLen[pass], blockSumBufferList[pass], outBufferList[pass], outBufferList[1], sumDataPos+OFS_DATA_SIZE);
#if CL_DEBUG
    	cqCommandQueue.EnqueueReadBuffer(outBufferList[pass], true, 0, (listComputeArrayLen[pass-1]>>blockSizeShift) * sizeof(uint), houtArray.AddrOfPinnedObject());
#endif
	
		//throw new Exception ("TestException");
	    /* Do block-addition on outputBuffers */
	    for(int i = pass; i > 0; i--)
	    {
			BlockAddition(cqCommandQueue, listComputeArrayLen[i-1], outBufferList[i], outBufferList[i-1]);
	    }
	}
#endif
}