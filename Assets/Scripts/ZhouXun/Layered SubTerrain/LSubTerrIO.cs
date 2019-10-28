//#define SUBTERRAIN_EDITOR_BUILD

using UnityEngine;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

// Manage data I/O of the layered-subterrain system.
// zhouxun
public class LSubTerrIO
{
    #region LZ4_EXTERN
    [DllImport("lz4_dll")]
    public static extern int LZ4_compress(byte[] source, byte[] dest, int isize);
    [DllImport("lz4_dll")]
    public static extern int LZ4_uncompress(byte[] source, byte[] dest, int osize);
	[DllImport("lz4_dll")]
	public static extern int LZ4_uncompress_unknownOutputSize(byte[] source, byte[] dest, int isize, int maxOutputSize);
    #endregion
	public static string s_orgnFilePath{ 	get{return Path.Combine(OriginalSubTerrainDir, "subter.dat");       }}
	public static string OriginalSubTerrainDir = null;

	// Original subterrain data file vars
	private Thread _thread = null;
	private FileStream _orgnSubTerrFile = null;
	private int [] _orgnOfsData = null;
	private int [] _orgnLenData = null;
	private int [] _orgnUcmpLenData = null;
	private int _curDataIdxInBuff = -1;
	private int _curDataLenInBuff = 0;
	private byte[] _zippedBuff;
	private byte[] _unzippedBuff;
	public byte[] DataBuff{ get { return _unzippedBuff; } }
	public int DataIdx{ get { return _curDataIdxInBuff; } }
	public int DataLen{ get { return _curDataLenInBuff; } }
		
    #region CREATE_AND_DELETE
	private LSubTerrIO(){}
	public static LSubTerrIO CreateInst()
	{
		LSubTerrIO io = new LSubTerrIO ();
		// Allocate ofs and len
		io._orgnOfsData = new int [LSubTerrConstant.XZCount];
		io._orgnLenData = new int [LSubTerrConstant.XZCount];
		io._orgnUcmpLenData = new int [LSubTerrConstant.XZCount];

		try{	// Open original subterrain data file
			io._orgnSubTerrFile = new FileStream(s_orgnFilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
		}catch{
			Debug.LogWarning("No Layered subTerrain File, No Trees");
			io._orgnSubTerrFile = null;
		}

		if (io._orgnSubTerrFile != null)
		{
			// Read each subterrain's data offset, len, uncmp-len.
			BinaryReader br = new BinaryReader(io._orgnSubTerrFile);
			int orgnLenMax = 0;
			int orgnUcmpLenMax = 0;
			io._orgnOfsData[0] = br.ReadInt32();
			io._orgnUcmpLenData[0] = br.ReadInt32();
	        for ( int i = 1; i < LSubTerrConstant.XZCount; ++i )
	        {
				io._orgnOfsData[i] = br.ReadInt32();
				io._orgnLenData[i-1] = io._orgnOfsData[i] - io._orgnOfsData[i-1];
				if(io._orgnOfsData[i-1] > orgnLenMax){
					orgnLenMax = io._orgnOfsData[i-1];
				}

				io._orgnUcmpLenData[i] = br.ReadInt32();
				if(io._orgnUcmpLenData[i] > orgnUcmpLenMax){
					orgnUcmpLenMax = io._orgnUcmpLenData[i];
				}
	        }
			io._orgnLenData[LSubTerrConstant.XZCount - 1] = (int)io._orgnSubTerrFile.Length - io._orgnOfsData[LSubTerrConstant.XZCount - 1];
			if(io._orgnLenData[LSubTerrConstant.XZCount - 1] > orgnLenMax){
				orgnLenMax = io._orgnLenData[LSubTerrConstant.XZCount - 1];
			}

			io._curDataIdxInBuff = -1;
			io._curDataLenInBuff = 0;
			io._zippedBuff = new byte[orgnLenMax];
			io._unzippedBuff = new byte[orgnUcmpLenMax+1];

			io._thread = new Thread(new ThreadStart(io.ProcessReqs));
			io._thread.Start();
		}
		return io;
	}
	public static void DestroyInst(LSubTerrIO io)
	{
		if ( io != null && io._orgnSubTerrFile != null )
		{
			io._orgnSubTerrFile.Close();
			io._orgnSubTerrFile = null;
		}
	}
	#endregion
	
	public void ReadOrgDataToBuff( int index )
	{
#if SUBTERRAIN_EDITOR_BUILD
		if ( Application.isEditor ){
		    string MyDocPath = GameConfig.GetUserDataPath();
		    string filename = MyDocPath + GameConfig.CreateSystemData + "/SubTerrains/cache_" + index.ToString() + ".subter";
			// Creates or overwrites a file
			if ( File.Exists(filename) ){
				using ( FileStream fs = new FileStream( filename, FileMode.Open, FileAccess.Read, FileShare.Read ) ){
		        	BinaryReader r = new BinaryReader(fs);
					_curDataLenInBuff = r.Read(_unzippedBuff, 0, (int)fs.Length);
					_curDataIdxInBuff = index;
					r.Close();
					fs.Close();
				}
				return;
			}
		}
#endif
		if (_orgnSubTerrFile != null)
		{
			try{
				_orgnSubTerrFile.Seek(_orgnOfsData[index], SeekOrigin.Begin);
				_orgnSubTerrFile.Read(_zippedBuff, 0, _orgnLenData[index]);
				_curDataLenInBuff = LZ4_uncompress_unknownOutputSize(_zippedBuff, _unzippedBuff, _orgnLenData[index], _unzippedBuff.Length);
				//if(_curDataLenInBuff != m_orgnUcmpLenData[index]){	Debug.LogError("TerData Lenght Not Match:"+_curDataLenInBuff+"|"+m_orgnUcmpLenData[index]); }
				_curDataIdxInBuff = index;
			}catch{}
		}
	}

	int CntInProcess{ get{ lock (this) {	return _lstNodesIdx.Count;	} }	}
	//IntVector3 _curPosCompleted = new IntVector3(0, -1, 0);
	//IntVector3 _curPosInProcess = new IntVector3(0, -1, 0);
	List<int> _lstNodesIdx = new List<int> ();
	List<LSubTerrain> _lstNodes = new List<LSubTerrain> ();

	public bool TryFill(IntVector3 ipos, Dictionary<int, LSubTerrain> dicNodes)
	{
		int nExpand = LSubTerrainMgr.Instance.NumDataExpands;
		for (int x = ipos.x - nExpand; x <= ipos.x + nExpand; x++) {
			for (int z = ipos.z - nExpand; z <= ipos.z + nExpand; z++) {
				if (x < 0 || x >= LSubTerrConstant.XCount || 
				    z < 0 || z >= LSubTerrConstant.ZCount)
					continue;
				
				int idx = LSubTerrUtils.PosToIndex (x, z);
				if (!dicNodes.ContainsKey (idx)) {
					LSubTerrain node = new LSubTerrain();
					node.Index = idx;
					ReadOrgDataToBuff (node.Index);
					node.ApplyData(DataBuff, DataLen);
					dicNodes.Add (idx, node);
					return false;
				}
			}
		}
		return true;
	}
	public bool TryFill_T(IntVector3 ipos, Dictionary<int, LSubTerrain> dicNodes)
	{
		if (CntInProcess > 0)
			return false;

		int n = _lstNodes.Count;
		if(n > 0){
			for (int i = 0; i < n; i++) {
				int index = _lstNodes [i].Index;
				if (dicNodes.ContainsKey (index)) {
					Debug.LogError ("Adding an LSubTerrain node but it already exist in the map, the old node will be replaced !");
					dicNodes [index] = _lstNodes [i];
				} else {
					dicNodes.Add (index, _lstNodes [i]);
				}
			}
			_lstNodes.Clear ();
		}

		// Add into process
		bool bRet = true;
		int nExpand = LSubTerrainMgr.Instance.NumDataExpands;
		lock (_lstNodesIdx) {
			for (int x = ipos.x - nExpand; x <= ipos.x + nExpand; x++) {
				for (int z = ipos.z - nExpand; z <= ipos.z + nExpand; z++) {
					if (x < 0 || x >= LSubTerrConstant.XCount || 
						z < 0 || z >= LSubTerrConstant.ZCount)
							continue;

					int idx = LSubTerrUtils.PosToIndex (x, z);
					if (!dicNodes.ContainsKey (idx)) {
						_lstNodesIdx.Add(idx);
						bRet = false;
					}
				}
			}
		}
		return bRet;
	}
	void ProcessReqs()
	{
		while(LSubTerrainMgr.Instance != null)
		{
			int n = CntInProcess;
			if(n > 0)
			{
				try{
					for(int i = 0; i < n; i++){
						LSubTerrain node = new LSubTerrain();
						node.Index = _lstNodesIdx[i];
						ReadOrgDataToBuff (node.Index);
						node.ApplyData(DataBuff, DataLen);
						_lstNodes.Add(node);
					}
				} catch{}

				lock (_lstNodesIdx){
					_lstNodesIdx.Clear();
				}	
			}
			Thread.Sleep(1);
		}
	}
}
