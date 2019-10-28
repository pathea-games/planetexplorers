using UnityEngine;
using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class SurfExtractorsMan
{
	// multiplier of 3
	public const int c_vertsCnt4Pool = 3999;	// most chunks have 7k~8K verts
	public const int c_vertsCntMax = 64998;

	public static int[] s_indiceMax = null;
	public static int[] s_indice4Pool = null;

	//public static bool TestThrowOcl = false;

#if false //null extractor
	private static IVxSurfExtractor _vxSurfExtractor = TmpSurfExtractor.Inst;
	private static IVxSurfExtractor _b45BuildSurfExtractor = TmpSurfExtractor.Inst;
	private static IVxSurfExtractor _b45CursorSurfExtractor = TmpSurfExtractor.Inst;
#else
	private static IVxSurfExtractor _vxSurfExtractor = null;
	private static IVxSurfExtractor _b45BuildSurfExtractor = null;
	private static IVxSurfExtractor _b45CursorSurfExtractor = null;
#endif
	private static string _vxSurfOpt = null;
	public static IVxSurfExtractor VxSurfExtractor{			get{ return _vxSurfExtractor;		} }
	public static IVxSurfExtractor B45BuildSurfExtractor{	get{ return _b45BuildSurfExtractor;	} }
	public static IVxSurfExtractor B45CursorSurfExtractor{	get{ return _b45CursorSurfExtractor;	} }
	public static void CheckGenSurfExtractor()
	{
		// Init indice
		s_indiceMax = new int[SurfExtractorsMan.c_vertsCntMax];	// max number of vertices in an unity mesh
		for (int i = 0; i < SurfExtractorsMan.c_vertsCntMax; i++) {
			s_indiceMax [i] = i;
		}		
		s_indice4Pool = new int[SurfExtractorsMan.c_vertsCnt4Pool];
		Array.Copy (s_indiceMax, s_indice4Pool, SurfExtractorsMan.c_vertsCnt4Pool);

		// Init extractor
		if(_vxSurfOpt != null && _vxSurfOpt.CompareTo(oclManager.CurOclOpt) == 0)
		{
			_vxSurfExtractor.Reset();
		}
		else
		{
			if(_vxSurfExtractor != null)	_vxSurfExtractor.CleanUp();
#if UNITY_EDITOR
			oclManager.CurOclOpt = oclManager.OclOptionList[0];
			//oclManager.CurOclOpt = "aa";
			//oclManager.CurOclOpt = "OpenCL|NVIDIA CUDA     |GeForce GTX 560".Replace(' ', '_');
			//oclManager.CurOclOpt = "OpenCL|Intel(R) OpenCL |        Intel(R) Core(TM) i3-2100 CPU @ 3.10GHz".Replace(' ', '_');
#endif
			oclManager.InitOclFromOpt();
			if(oclManager.ActiveOclMan != null)
			{
				_vxSurfExtractor = new SurfExtractorOclMC();
				_vxSurfExtractor.Init();
			}
			else
			{
				_vxSurfExtractor = new SurfExtractorCpuMC();
				_vxSurfExtractor.Init();
			}
			_vxSurfOpt = string.Copy(oclManager.CurOclOpt);
		}

		if(_b45BuildSurfExtractor != null)	_b45BuildSurfExtractor.CleanUp();
		_b45BuildSurfExtractor = new SurfExtractorCpuB45();
		_b45BuildSurfExtractor.Init();
	}
	public static void SwitchToVxCpu(List<IVxSurfExtractReq> reqsInProcess, Dictionary<int, IVxSurfExtractReq> reqsToProcess)
	{
		Debug.LogError ("[SurfExtractMan]: Start to switch to Cpu !");
		SurfExtractorCpuMC vxCpu = new SurfExtractorCpuMC();
		vxCpu.Init ();
		_vxSurfExtractor = vxCpu;	// To avoid AddReq when vxCpu not Init
		lock (reqsToProcess) {
			for (int i = 0; i < reqsInProcess.Count; i++) {
				_vxSurfExtractor.AddSurfExtractReq (reqsInProcess [i]);
			}
			foreach (KeyValuePair<int, IVxSurfExtractReq> pair in reqsToProcess) {
				_vxSurfExtractor.AddSurfExtractReq (pair.Value);
			}
			Debug.LogError ("[SurfExtractMan]: Finished to switch to Cpu:"+(reqsInProcess.Count+reqsToProcess.Count));
		}
	}
	public static void PostProc()
	{
		//if(Input.GetKeyUp(KeyCode.P)){
		//	SurfExtractorsMan.TestThrowOcl = true;
		//}
		if(_vxSurfExtractor != null) 		_vxSurfExtractor.OnFin ();
		if(_b45BuildSurfExtractor != null)	_b45BuildSurfExtractor.OnFin();
		if(_b45CursorSurfExtractor != null)	_b45CursorSurfExtractor.OnFin();
	}
	public static void CleanUp()
	{
		if(_vxSurfExtractor != null)			_vxSurfExtractor.CleanUp();
		if(_b45BuildSurfExtractor != null)	_b45BuildSurfExtractor.CleanUp();
		if(_b45CursorSurfExtractor != null)	_b45CursorSurfExtractor.CleanUp();
		_vxSurfExtractor = null;
		_b45BuildSurfExtractor = null;
		_b45CursorSurfExtractor = null;
		Debug.Log("Mem size after surfExtractor CleanUp :"+GC.GetTotalMemory(true));
	}
}