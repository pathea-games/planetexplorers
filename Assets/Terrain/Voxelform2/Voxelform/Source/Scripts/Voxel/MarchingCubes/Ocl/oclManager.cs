using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;
#if UNITY_STANDALONE_OSX
using OpenCLNetMac;
using OpenCLNet = OpenCLNetMac;
#else
using OpenCLNetWin;
using OpenCLNet = OpenCLNetWin;
#endif

public static class oclManager
{
	private const string OptCpuMask = "Cpu";
	private const string OptOclMask = "OpenCL";
	private static readonly string OclSourcePath = GameConfig.OclSourcePath;	//UnityEngine.Application.dataPath + "/Resources/OclKernel";
	private static readonly string OclBinaryPath = GameConfig.OclBinaryPath;	//UnityEngine.Application.dataPath + "/../clCache";
	
	//public static bool bRv7xxGpu = false;	// must be called before CreateOclManager because CreateOclManager will mod bRv7xxGpu
	//public static bool bCPU = false;
	public static OpenCLManager ActiveOclMan;
	public static CommandQueue  ActiveOclCQ;
	public static List<string> OclOptionList{get{ 	if(oclOptList == null){CreateOclOptList();}	return oclOptList;	}	}
	public static string CurOclOpt = OptCpuMask;
	
	private static List<string> oclOptList = null;
	private static List<OpenCLManager> instances = null;
    private static void CreateOclManagers()	// Create an OpenCLManager for each platform
    {
		instances = new List<OpenCLManager>();

		int nPlatforms = OpenCL.NumberOfPlatforms;
		Debug.Log("[OCLLOG]:Platform Count:"+nPlatforms);
		if (nPlatforms <= 0)
		{
			Debug.LogError("[OCLLOG]:OpenCLIs[NOT]Available");
			return;
		}

		for(int pIdx = 0; pIdx < nPlatforms; pIdx++)
		{
			OpenCLNet.Platform platform = OpenCLNet.OpenCL.GetPlatform(pIdx);
			OpenCLNet.Device[] devices = platform.QueryDevices(OpenCLNet.DeviceType.ALL);
			int nDevices = devices.Length;
			if(nDevices <= 0)
			{
				Debug.LogError("[OCLLOG]:No OpenCL devices found that matched filter criteria on Platform_"+platform.Name);
				continue;
			}
			// We might use the whole devices as CreateContext para, but we don't do so.
			// Here create a oclMan for each device because
			// the same platform will build kernel for each device in its context and can be failed
			// (such as apple, failed in building kernel on HD6630M, succeeded on intel cpu)
			for(int dIdx = 0; dIdx < nDevices; dIdx++)
			{
				try{
					Debug.Log("[OCLLOG]:Creating OCL on Platform_"+platform.Name+"+Device_"+devices[dIdx].Name);
					OpenCLManager oclMan = new OpenCLManager();
					oclMan.SourcePath = OclSourcePath;
					oclMan.BinaryPath = OclBinaryPath;

					IntPtr[] properties = new IntPtr[]
					{
						(IntPtr)ContextProperties.PLATFORM, platform,
						IntPtr.Zero
					};
					OpenCLNet.Device[] devicesEnum = new Device[]{devices[dIdx]};
					oclMan.CreateContext(platform, properties, devicesEnum);
					instances.Add(oclMan);
				}
				catch(Exception e){
					Debug.Log ("[OCLLOG]Exception at Platform_"+platform.Name+"+Device_"+devices[dIdx].Name+":"+e.Message);
				}
			}
		}
	}
	private static void CreateOclOptList()
	{
		oclOptList = new List<string>();
#if !UNITY_STANDALONE_OSX
		try{
			if(instances == null)	CreateOclManagers();
			int idxGpuDevice = 0;
			foreach(OpenCLManager oclMan in instances)
			{
				string platformName16 = oclMan.Platform.Name.Length > 16 ? oclMan.Platform.Name.Substring(0,16) : oclMan.Platform.Name.PadRight(16,' ');
				foreach(CommandQueue cq in oclMan.CQ)
				{
					string opt = (OptOclMask+"|"+platformName16+"|"+cq.Device.Name).Replace(' ', '_');
					if(cq.Device.DeviceType == OpenCLNet.DeviceType.GPU)
					{
						oclOptList.Insert(idxGpuDevice++,opt);
					}
					else
					{
						oclOptList.Add(opt);
					}
				}
			}
		}
		catch
		{
			Debug.Log ("[OCLLOG]Exception at CreateOclOptList");
		}
#endif
		oclOptList.Add(OptCpuMask);
	}
	public static bool InitOclFromOpt() // true: Succeeded; false: failed and use CPU instead
	{
		ActiveOclMan = null;
		ActiveOclCQ = null;
		if(0==string.Compare(OptCpuMask, 0, CurOclOpt, 0, OptCpuMask.Length)) 
		{
			Debug.Log("[OCLLOG]Succeed to init with "+CurOclOpt);
			return true;
		}
		
		if(instances == null)	CreateOclManagers();
		try{
			foreach(OpenCLManager oclMan in instances)
			{
				string platformName16 = oclMan.Platform.Name.Length > 16 ? oclMan.Platform.Name.Substring(0,16) : oclMan.Platform.Name.PadRight(16,' ');
				foreach(CommandQueue cq in oclMan.CQ)
				{
					string opt = (OptOclMask+"|"+platformName16+"|"+cq.Device.Name).Replace(' ', '_');
					if(0 == string.Compare(opt, CurOclOpt))
					{
						ActiveOclMan = oclMan;
						ActiveOclCQ = cq;
						if(oclMarchingCube.InitMC()){
							Debug.Log("[OCLLOG]Succeed to init with "+CurOclOpt);
							return true;
						} else {
							throw new Exception("Failed to init with "+CurOclOpt);
						}
					}
				}				
			}
		}
		catch{}

		Debug.Log("[OCLLOG]Fallback to CPU because of failure to init with "+CurOclOpt);
		ActiveOclMan = null;
		ActiveOclCQ = null;
		CurOclOpt = OptCpuMask;
		return false;
	}
	public static void Dispose()
	{
		try
		{
			if(instances != null)
			{
				foreach(OpenCLManager oclMan in instances)
				{
					oclMan.Dispose();
				}
				instances.Clear();
				instances = null;
			}
		}
		catch(Exception e)
		{
			Debug.Log("[OCLLOG]Fallback to dispose OpenCL:" + e.Message);
		}
	}
}
