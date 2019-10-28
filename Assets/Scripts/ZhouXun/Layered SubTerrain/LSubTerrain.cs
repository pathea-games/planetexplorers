using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

// A Layered-subterrain node
public class LSubTerrain
{
	#region POSITION_AND_INDEX
	private int X = 0;
	private int Z = 0;
	public int xIndex { get { return X; } }
	public int zIndex { get { return Z; } }
	public IntVector3 iPos { get { return new IntVector3(X,0,Z); } }
	public Vector3 wPos { get { return new Vector3(X * LSubTerrConstant.SizeF,0,Z * LSubTerrConstant.SizeF); } }
	public int Index { get { return LSubTerrUtils.PosToIndex(X,Z); } set { IntVector3 ipos = LSubTerrUtils.IndexToPos(value); X = ipos.x; Z = ipos.z; } }
	#endregion

	private IntVector3 _tmpVec3 = IntVector3.Zero;
	private bool m_FinishedProcess = false;
	private int _dataLen = 0;
	public int DataLen { get { return _dataLen; } } 
	public int TreeCnt { get { return m_listTrees.Count; } } 
	public int MapKeyCnt{ get { return m_mapTrees.Count; } }
	public bool HasData { get { return _dataLen > 0; } }
	public bool FinishedProcess { get { return m_FinishedProcess; } }
	
	private List<TreeInfo> m_listTrees = new List<TreeInfo> ();
	private Dictionary<int, TreeInfo> m_mapTrees = new Dictionary<int, TreeInfo> ();
	private Dictionary<TreeInfo, TreeInfo> m_mapTwoFeetTrees = new Dictionary<TreeInfo, TreeInfo> ();
	
	public TreeInfo GetTreeInfoListAtPos(IntVector3 pos)
	{
		int treeidx = LSubTerrUtils.TreePosToKey(pos);
		TreeInfo ti = null;
		if ( m_mapTrees.TryGetValue(treeidx, out ti) ){
			return ti;
		}
		return null;
	}

	public void ApplyData(byte[] data, int len)
	{
		//_dataLen = len;
		if (len <= 0) {
			m_FinishedProcess = true;
			return;
		}

		TreeInfo tmpTi;
		#region READING_DATA
		//Profiler.BeginSample("AddTreeInfo");
		using ( MemoryStream ms = new MemoryStream(data) )
		{
			//IntVector3 tmpTreePos = IntVector3.Zero;
			BinaryReader br = new BinaryReader(ms);
			int mapSize = br.ReadInt32();
			for ( int i = 0; i != mapSize; ++i )
			{
				br.ReadInt32();
				br.ReadInt32();
				br.ReadInt32();
		        int _listSize = br.ReadInt32();
		        for ( int j = 0; j != _listSize; ++j )
		        {
					tmpTi = TreeInfo.GetTI();
					tmpTi.m_clr.r = br.ReadSingle();
					tmpTi.m_clr.g = br.ReadSingle();
					tmpTi.m_clr.b = br.ReadSingle();
					tmpTi.m_clr.a = br.ReadSingle();
		            tmpTi.m_heightScale = br.ReadSingle();
					tmpTi.m_lightMapClr.r = br.ReadSingle();
					tmpTi.m_lightMapClr.g = br.ReadSingle();
					tmpTi.m_lightMapClr.b = br.ReadSingle();
					tmpTi.m_lightMapClr.a = br.ReadSingle();
					tmpTi.m_pos.x = br.ReadSingle();
					tmpTi.m_pos.y = br.ReadSingle();
					tmpTi.m_pos.z = br.ReadSingle();
		            tmpTi.m_protoTypeIdx = br.ReadInt32();
		            tmpTi.m_widthScale = br.ReadSingle();

					AddTreeInfo(tmpTi);
		        }
			}
			br.Close();
			ms.Close();
		}
		//Profiler.EndSample();
		
		// cutting deleted tree recorded in saved data
		//Profiler.BeginSample("CutRecordedTree");
		List<Vector3> lstDelPos;
		if ( LSubTerrSL.m_mapDelPos.TryGetValue(Index, out lstDelPos) )
		{
			int nDelPos = lstDelPos.Count;
			for (int i = 0; i < nDelPos; i++) {
				Vector3 delpos = lstDelPos [i];
				_tmpVec3.x = Mathf.FloorToInt(delpos.x);
				_tmpVec3.y = Mathf.FloorToInt(delpos.y);
				_tmpVec3.z = Mathf.FloorToInt(delpos.z);
				int tmpKey = LSubTerrUtils.TreePosToKey(_tmpVec3);
				if (m_mapTrees.TryGetValue (tmpKey, out tmpTi)) {
					tmpTi = tmpTi.FindTi(delpos);
					if(tmpTi != null){
						TreeInfo secondFoot = DeleteTreeInfo(tmpTi);
						if(secondFoot != null){
							DeleteTreeInfo(secondFoot);
						}
					}
				}
			}
		}
		//Profiler.EndSample();
		#endregion
		
		#region ASSIGN_LAYERS
		//Profiler.BeginSample("AssignLayer");
		List<LSubTerrLayerOption> layers = LSubTerrainMgr.Instance.Layers;
		List<TreeInfo> [] layers_trees = new List<TreeInfo> [layers.Count];
		for ( int l = layers.Count - 1; l >= 0; --l )
		{
			layers_trees[l] = new List<TreeInfo> ();
		}
		foreach ( TreeInfo ti in m_listTrees )
		{
			float height = LSubTerrainMgr.Instance.GlobalPrototypeBounds[ti.m_protoTypeIdx].extents.y * 2F;
			for ( int l = layers.Count - 1; l >= 0; --l )
			{
				// Trees in this layer
				if ( layers[l].MinTreeHeight <= height && height < layers[l].MaxTreeHeight )
				{
					layers_trees[l].Add(ti);
					break;
				}
			}
		}
		for ( int l = layers.Count - 1; l >= 0; --l )
		{
			LSubTerrainMgr.Instance.LayerCreators[l].AddTreeBatch(Index, layers_trees[l]);
		}
		//Profiler.EndSample();
		#endregion
		
		m_FinishedProcess = true;
	}

	public TreeInfo AddTreeInfo(Vector3 wpos, int prototype, float wScale, float hScale)
	{
		TreeInfo ti = TreeInfo.GetTI();
		ti.m_clr = Color.white;
		ti.m_lightMapClr = Color.white;
		ti.m_widthScale = wScale;
		ti.m_heightScale = hScale;
		ti.m_protoTypeIdx = prototype;
		ti.m_pos = LSubTerrUtils.TreeWorldPosToTerrainPos(wpos);
		return AddTreeInfo (ti);
	}
	private TreeInfo AddTreeInfo(TreeInfo ti)
	{
		// Tree place holder works
		if (ti.m_protoTypeIdx == LSubTerrainMgr.TreePlaceHolderPrototypeIndex ||
			ti.m_pos.x < 0.00001 || ti.m_pos.z < 0.00001 || ti.m_pos.x > 0.99999 || ti.m_pos.z > 0.99999) { // Avoid bugs
			TreeInfo.FreeTI(ti);
			return null;
		}

		// Add to tree map
		_tmpVec3.x = Mathf.FloorToInt(ti.m_pos.x*LSubTerrConstant.SizeF);
		_tmpVec3.y = Mathf.FloorToInt(ti.m_pos.y*LSubTerrConstant.HeightF);
		_tmpVec3.z = Mathf.FloorToInt(ti.m_pos.z*LSubTerrConstant.SizeF);
		int tmpKey = LSubTerrUtils.TreePosToKey(_tmpVec3);
		TreeInfo tmpTi;
		if (m_mapTrees.TryGetValue (tmpKey, out tmpTi)) {
			tmpTi.AttachTi(ti);
		} else {
			m_mapTrees.Add(tmpKey, ti);
		}
		
		// Add to tree list
		m_listTrees.Add(ti);		
		// Add to 32 tree map
		if ( LSubTerrainMgr.HasCollider(ti.m_protoTypeIdx) || LSubTerrainMgr.HasLight(ti.m_protoTypeIdx) )
		{
			tmpKey = LSubTerrUtils.TreeWorldPosTo32Key(LSubTerrUtils.TreeTerrainPosToWorldPos(X,Z,ti.m_pos));
			List<TreeInfo> tmpTis;
			if ( !LSubTerrainMgr.Instance.m_map32Trees.TryGetValue(tmpKey, out tmpTis) ) {
				tmpTis = new List<TreeInfo>();
				LSubTerrainMgr.Instance.m_map32Trees.Add(tmpKey, tmpTis);
			}
			tmpTis.Add(ti);
		}

		// Tree place holder works
		LTreePlaceHolderInfo tphinfo = LSubTerrainMgr.GetTreePlaceHolderInfo(ti.m_protoTypeIdx);
		if ( tphinfo != null )
		{
			TreeInfo tph = TreeInfo.GetTI ();
			tph.m_clr = Color.white;
			tph.m_heightScale = tphinfo.m_HeightScale * ti.m_heightScale;
			tph.m_lightMapClr = Color.white;
			Vector3 tphoffset = tphinfo.TerrOffset;
			tphoffset.x *= ti.m_widthScale;
			tphoffset.y *= ti.m_heightScale;
			tphoffset.z *= ti.m_widthScale;
			tph.m_pos = ti.m_pos + tphoffset;
			tph.m_protoTypeIdx = LSubTerrainMgr.TreePlaceHolderPrototypeIndex;
			tph.m_widthScale = tphinfo.m_WidthScale * ti.m_widthScale;

			// Add to tree map
			_tmpVec3.x = Mathf.FloorToInt(tph.m_pos.x*LSubTerrConstant.SizeF);
			_tmpVec3.y = Mathf.FloorToInt(tph.m_pos.y*LSubTerrConstant.HeightF);
			_tmpVec3.z = Mathf.FloorToInt(tph.m_pos.z*LSubTerrConstant.SizeF);
			tmpKey = LSubTerrUtils.TreePosToKey(_tmpVec3);
			if (m_mapTrees.TryGetValue (tmpKey, out tmpTi)) {
				tmpTi.AttachTi(tph);
			} else {
				m_mapTrees.Add(tmpKey, tph);
			}

			// Add to tree list
			m_listTrees.Add(tph);
			m_mapTwoFeetTrees.Add(tph, ti);
			m_mapTwoFeetTrees.Add(ti,tph);
		}
		return ti;
	}
	public TreeInfo DeleteTreeInfo(TreeInfo ti)	// return SecondFoot
	{
		_tmpVec3.x = Mathf.FloorToInt(ti.m_pos.x*LSubTerrConstant.SizeF);
		_tmpVec3.y = Mathf.FloorToInt(ti.m_pos.y*LSubTerrConstant.HeightF);
		_tmpVec3.z = Mathf.FloorToInt(ti.m_pos.z*LSubTerrConstant.SizeF);
		int tmpKey = LSubTerrUtils.TreePosToKey(_tmpVec3);
		TreeInfo.RemoveTiFromDict (m_mapTrees, tmpKey, ti);
		if (m_listTrees.Remove (ti)) {
			TreeInfo.FreeTI (ti);
		}

		// Delete it in Mgr's m_map32Trees
		tmpKey = LSubTerrUtils.TreeWorldPosTo32Key(LSubTerrUtils.TreeTerrainPosToWorldPos(X,Z,ti.m_pos));
		List<TreeInfo> tmpTis;
		if (LSubTerrainMgr.Instance.m_map32Trees.TryGetValue (tmpKey, out tmpTis)) {
			tmpTis.Remove(ti);
			if ( tmpTis.Count == 0 ){
				LSubTerrainMgr.Instance.m_map32Trees.Remove(tmpKey);
			}
		}

		// Check if it's 2 feet tree
		TreeInfo secondFoot; 
		if ( m_mapTwoFeetTrees.TryGetValue(ti, out secondFoot) )
		{
			m_mapTwoFeetTrees.Remove(ti);
			m_mapTwoFeetTrees.Remove(secondFoot);	// remove 2nd foot to avoid from recursive
			return secondFoot;
		}
		return null;
	}

	public void Release()
	{
		_dataLen = 0;
		if ( m_mapTrees != null )
		{
			m_mapTrees.Clear();
		}
		if ( m_listTrees != null )
		{
			TreeInfo.FreeTIs (m_listTrees);
			m_listTrees.Clear();
		}
		if ( LSubTerrainMgr.Instance != null )
		{
			for ( int i = 0; i < LSubTerrainMgr.Instance.Layers.Count; ++i )
			{
				LSubTerrainMgr.Instance.LayerCreators[i].DelTreeBatch(Index);
			}
			for ( int i = X * 8; i < X * 8 + 8; i++ )
			{
				for ( int j = Z * 8; j < Z * 8 + 8; j++ )
				{
					int tmpKey = LSubTerrUtils.Tree32PosTo32Key(i,j);
					LSubTerrainMgr.Instance.m_map32Trees.Remove(tmpKey);
				}
			}
		}
	}

	
	// for editor
	public byte[] ExportSubTerrainData()
	{
		byte[] outbytes = new byte [0];
		using ( MemoryStream _memStream = new MemoryStream() )
		{
			BinaryWriter _out = new BinaryWriter (_memStream);
			
			_out.Write(m_listTrees.Count);
			foreach ( TreeInfo _ti in m_listTrees )
			{
				_out.Write((int)(0));
				_out.Write((int)(0));
				_out.Write((int)(0));
				_out.Write((int)(1));
				_out.Write(_ti.m_clr.r);
				_out.Write(_ti.m_clr.g);
				_out.Write(_ti.m_clr.b);
				_out.Write(_ti.m_clr.a);
				_out.Write(_ti.m_heightScale);
				_out.Write(_ti.m_lightMapClr.r);
				_out.Write(_ti.m_lightMapClr.g);
				_out.Write(_ti.m_lightMapClr.b);
				_out.Write(_ti.m_lightMapClr.a);
				_out.Write(_ti.m_pos.x);
				_out.Write(_ti.m_pos.y);
				_out.Write(_ti.m_pos.z);
				_out.Write(_ti.m_protoTypeIdx);
				_out.Write(_ti.m_widthScale);
			}
			
			outbytes = _memStream.ToArray();
			_out.Close();
			_memStream.Close();
		}
		return outbytes;
	}	
	// for editor
	public void SaveCache()
	{
		if ( Application.isEditor )
		{
			byte[] cachedata = ExportSubTerrainData();
		    string MyDocPath = GameConfig.GetUserDataPath();
			// Create missing folders
		    if (!Directory.Exists( MyDocPath + GameConfig.CreateSystemData + "/SubTerrains" ))
		        Directory.CreateDirectory( MyDocPath + GameConfig.CreateSystemData + "/SubTerrains" );
		    string path = MyDocPath + GameConfig.CreateSystemData + "/SubTerrains/";
			// Creates or overwrites a file
			using ( FileStream fs = new FileStream( path + "cache_" + Index.ToString() + ".subter", FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite ) )
			{
		        BinaryWriter w = new BinaryWriter(fs);
				w.Write(cachedata, 0, cachedata.Length);
				w.Close();
				fs.Close();
			}
		}
	}
}
