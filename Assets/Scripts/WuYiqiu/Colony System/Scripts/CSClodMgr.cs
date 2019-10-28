using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

// TODO : Clod type
//
public class ClodChunk
{
	public IntVec3 m_ChunkIndex;
	public Dictionary<IntVec3, Vector3> m_Clods;
	public Dictionary<IntVec3, int> m_IdleClods;
	
	public Vector3 ReservedPos = Vector3.zero;

	public ClodChunk()
	{
		m_Clods = new Dictionary<IntVec3, Vector3>();
		m_IdleClods = new Dictionary<IntVec3, int>();
	}

	public void AddClod(Vector3 pos, bool dirty = false)
	{
		IntVec3 int_pos = new IntVec3(pos);
		if (!m_Clods.ContainsKey(int_pos))
		{
			m_Clods.Add(int_pos, pos);
			if (!dirty && !m_IdleClods.ContainsKey(int_pos))
				m_IdleClods.Add(int_pos, 0);
		}
	}

	public void FreeClodByBouds(int plantItemId, Vector3 pos, bool dirty = false)
	{
		Bounds _bo = PlantInfo.GetPlantBounds(plantItemId, pos);

		foreach(var ivc in m_Clods.Keys)
		{
			if (_bo.Contains(m_Clods[ivc]))
			{
				IntVec3 int_pos = new IntVec3(m_Clods[ivc]);
				if (!dirty && !m_IdleClods.ContainsKey(int_pos))
					m_IdleClods.Add(int_pos, 0);
			}
		}
		
	}
	
	public void DeleteClod(Vector3 pos)
	{
		IntVec3 int_pos = new IntVec3(pos);
		if (m_Clods.ContainsKey(int_pos))
		{
			//if (!s_Instance.m_ClodLocas[pos])
			m_IdleClods.Remove(int_pos);
			m_Clods.Remove(int_pos);
		}
	}

	public void DirtyTheClod(Vector3 pos, bool dirty)
	{
		IntVec3 int_pos = new IntVec3(pos);
		if (m_Clods.ContainsKey(int_pos))
		{
			if (dirty)
				m_IdleClods.Remove(int_pos);
			else
				m_IdleClods[int_pos] = 0;
		}
	}

	public void DirtyTheClodByPlantBounds(int plantItemid,Vector3 pos,bool dirty)
	{
		Bounds _bo = PlantInfo.GetPlantBounds(plantItemid,pos);
		foreach(var ive3 in m_Clods.Keys)
		{
			if(_bo.Contains(m_Clods[ive3]))
			{
				DirtyTheClod(m_Clods[ive3],dirty);
			}
		}
	}

	public bool FindCleanClod (out Vector3 pos)
	{
		if (m_IdleClods.Count != 0)
		{
			foreach (IntVec3 ivec in m_IdleClods.Keys)
			{
                pos = m_Clods[ivec];
                return true;
			}

		}
        pos = Vector3.zero;
		return false;
	}

	public bool FindBetterClod (Vector3 center, float radius,CSFarm farm,int plantItemid,out Vector3 pos)
	{
		if (m_IdleClods.Count != 0)
		{
			foreach (IntVec3 ivec in m_IdleClods.Keys)
			{
				if(new Vector3(ivec.x-center.x,ivec.y-center.y,ivec.z-center.z).magnitude<radius
				   &&farm.checkRroundCanPlant(plantItemid,m_Clods[ivec]))
					{
						pos = m_Clods[ivec];
						return true;
					}
			}
			
		}
		pos = Vector3.zero;
		return false;
	}

	public bool HasIdleClods()
	{
		return (m_IdleClods.Count != 0);
	}
	public bool HasIdleClodsInRange(float radius,Vector3 center){
		foreach(IntVec3 pos in m_IdleClods.Keys){
			if(new Vector3(pos.x-center.x,pos.y-center.y,pos.z-center.z).magnitude<radius)
				return true;
		}
		return false;
	}
	/// <summary>
	/// <CSVD> Clod chunk import
	/// </summary>
	public void Import( BinaryReader r, int VERSION )
	{
        //switch ( VERSION )
        //{
        //case 0x0108:
        //case 0x0109:
        //case 0x0110:
        //case 0x0111:
        //case 0x0112:
        //case 0x0113:
        //case 0x0114:
        //case 0x0115:
        //case 0x0116:
        //{
        if (VERSION >= CSDataMgr.VERSION000)
        {
            IntVec3 index = new IntVec3();
            index.x = r.ReadInt32();
            index.y = r.ReadInt32();
            index.z = r.ReadInt32();
            m_ChunkIndex = index;

            int count = r.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                Vector3 pos = new Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
                IntVec3 index_pos = new IntVec3(pos);
                m_Clods.Add(index_pos, pos);
                m_IdleClods.Add(index_pos, 0);
            }

            count = r.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                Vector3 pos = new Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
                IntVec3 int_pos = new IntVec3(pos);
                m_Clods.Add(int_pos, pos);
            }
        }
			
        //}
        //return true;
        //default: break;
        //}

        //return false;
	}

	public void Export(BinaryWriter w)
	{
		w.Write(m_ChunkIndex.x);
		w.Write(m_ChunkIndex.y);
		w.Write(m_ChunkIndex.z);

		Dictionary<IntVec3, Vector3> tempClods = new Dictionary<IntVec3, Vector3>();
		foreach (KeyValuePair<IntVec3, Vector3> kvp in m_Clods)
			tempClods.Add (kvp.Key, kvp.Value);
		
		w.Write(m_IdleClods.Count);
		foreach (IntVec3 iv in m_IdleClods.Keys)
		{
			Vector3 pos = tempClods[iv];
			w.Write(pos.x);
			w.Write(pos.y);
			w.Write(pos.z);
			tempClods.Remove(iv);
		}
		
		w.Write(tempClods.Count);
		foreach (KeyValuePair<IntVec3, Vector3> kvp in tempClods)
		{
			w.Write(kvp.Value.x);
			w.Write(kvp.Value.y);
			w.Write(kvp.Value.z);
		}
	}
}

// ----------------------------------------------------------------------
/// <summary>
/// Colony clods
/// </summary>

public class CSClod
{
	public int ID;
	
	public const byte TerrainType = 19;
	
	public const int CHUNK_SIZE = 8;
	
	public Dictionary<IntVec3, ClodChunk> m_ClodChunks;
	private Dictionary<IntVec3, int> m_IdleChunks;
	
	public CSClod(int id)
	{
		m_ClodChunks = new Dictionary<IntVec3, ClodChunk>();
		m_IdleChunks = new Dictionary<IntVec3, int>();

		ID = id;
	}
	
	public void Clear()
	{

		m_IdleChunks.Clear();
		m_ClodChunks.Clear();
	}
	
	public void AddClod(Vector3 pos, bool dirty = false)
	{
		IntVec3 index_pos = new IntVec3(pos);
		IntVec3 chunkIndex = GetChunkIndex(index_pos);
		if (!m_ClodChunks.ContainsKey(chunkIndex))
		{
			ClodChunk cc = new ClodChunk();
			cc.m_ChunkIndex = chunkIndex;
			m_ClodChunks.Add(chunkIndex, cc);
			m_ClodChunks[chunkIndex].AddClod(pos, dirty);
		}
		else
		{
			m_ClodChunks[chunkIndex].AddClod(pos, dirty);
		}
		
		m_IdleChunks[chunkIndex] = 0;
	}
	
	public void DeleteClod(Vector3 pos)
	{
		IntVec3 chunkIndex = GetChunkIndex(new IntVec3(pos));
		if (m_ClodChunks.ContainsKey(chunkIndex))
		{
			m_ClodChunks[chunkIndex].DeleteClod(pos);
			if (m_ClodChunks[chunkIndex].m_Clods.Count == 0)
			{
				m_IdleChunks.Remove(chunkIndex);
				m_ClodChunks.Remove(chunkIndex);
			}
		}
	}
	
	public void DirtyTheClod(Vector3 pos, bool dirty)
	{
		IntVec3 chunkIndex = GetChunkIndex(new IntVec3(pos)); 
		if (m_ClodChunks.ContainsKey(chunkIndex))
		{
			m_ClodChunks[chunkIndex].DirtyTheClod(pos, dirty);
			
			if (!dirty)
				m_IdleChunks[chunkIndex] = 0;
		}
	}

	
	public static IntVec3 GetChunkIndex (IntVec3 clodIndex)
	{
		return new IntVec3(clodIndex.x / CHUNK_SIZE, clodIndex.y / CHUNK_SIZE, clodIndex.z / CHUNK_SIZE);
	}
	
	public ClodChunk FindCleanChunk(Vector3 center, float radius)
	{	
		if (m_IdleChunks.Count != 0)
		{
			float sqrRad = (radius + CHUNK_SIZE/2 )* (radius + CHUNK_SIZE/2);
			foreach (IntVec3 ivec in m_IdleChunks.Keys)
			{
				Vector3 vec = new Vector3(center.x - ivec.x * CHUNK_SIZE, center.y - ivec.y * CHUNK_SIZE, center.z - ivec.z * CHUNK_SIZE);
				if ( vec.sqrMagnitude < sqrRad && m_ClodChunks[ivec].m_IdleClods.Count != 0)
					return m_ClodChunks[ivec]; 
			}
		}
		
		return null;
	}

	public ClodChunk FindHasIdleClodsChunk(Vector3 center, float radius,CSFarm farm,int plantItemid,out Vector3 pos)
	{		
		if (m_IdleChunks.Count != 0)
		{
			float sqrRad = (radius + CHUNK_SIZE)* (radius + CHUNK_SIZE);
			foreach (IntVec3 ivec in m_IdleChunks.Keys)
			{
				Vector3 vec = new Vector3(center.x - ivec.x * CHUNK_SIZE, center.y - ivec.y * CHUNK_SIZE, center.z - ivec.z * CHUNK_SIZE);
				if ( vec.sqrMagnitude < sqrRad && m_ClodChunks[ivec].HasIdleClodsInRange(radius,center))
				{
					if(m_ClodChunks[ivec].FindBetterClod(center,radius,farm,plantItemid ,out pos))
						return m_ClodChunks[ivec];
				}
			}
		}
		pos = Vector3.zero;
		return null;
	}
	
	public void DirtyTheChunk (IntVec3 chunkIndex, bool dirty)
	{
		if (m_ClodChunks.ContainsKey(chunkIndex))
		{
			if (dirty)
				m_IdleChunks.Remove(chunkIndex);
			else
				m_IdleChunks[chunkIndex] = 0;
		}
		
	}
	
	/// <summary>
	/// <CSVD> Clod manager main import
	/// </summary>
	public bool Import( BinaryReader r )
	{
		int version = r.ReadInt32();
		
		if(version>=CSDataMgr.VERSION000)
		{
		
			int count = r.ReadInt32();
			for (int i = 0; i < count; i++)
			{
				ClodChunk cc = new ClodChunk();
				cc.Import(r, version);
				m_ClodChunks.Add(cc.m_ChunkIndex, cc);
				m_IdleChunks.Add(cc.m_ChunkIndex, 0);
			}
			
			count = r.ReadInt32();
			for (int i = 0; i < count; i++)
			{
				ClodChunk cc = new ClodChunk();
				cc.Import(r, version);
				m_ClodChunks.Add(cc.m_ChunkIndex, cc);
				if (cc.HasIdleClods())
					m_IdleChunks[cc.m_ChunkIndex] = 0;
			}
		}
		return true;
	}
	
	public void Export(BinaryWriter w)
	{
		w.Write(CSDataMgr.CUR_VERSION);

		w.Write(m_IdleChunks.Count);
		Dictionary<IntVec3, ClodChunk> tempChunks = new Dictionary<IntVec3, ClodChunk>();
		foreach (KeyValuePair<IntVec3, ClodChunk> kvp in m_ClodChunks)
			tempChunks.Add(kvp.Key, kvp.Value);
		
		foreach (IntVec3 vec3 in m_IdleChunks.Keys)  
		{
			ClodChunk cc = m_ClodChunks[vec3];
			cc.Export(w);
			
			tempChunks.Remove(vec3);
		}
		
		w.Write(tempChunks.Count);
		foreach (KeyValuePair<IntVec3, ClodChunk> kvp in tempChunks)
		{
			kvp.Value.Export(w);
		}
	}
}

// -----------------------------------------------------------------------

public class CSClodsMgr
{
	static private CSClodsMgr s_Instance = null;
	static public  CSClodsMgr Instance { get {return s_Instance; } }

	Dictionary<int, CSClod>  m_Clods;

	public CSClodsMgr()
	{
		m_Clods = new Dictionary<int, CSClod>();
	}

	public static void Init()
	{
		if (s_Instance == null)
		{
			s_Instance = new CSClodsMgr();
		}
	}

	public static CSClod CreateClod(int id)
	{
		if (Instance == null)
		{
			Debug.Log("The CSClodsMgr is null.");
			return null;
		}

		if ( !Instance.m_Clods.ContainsKey(id) )
			Instance.m_Clods.Add(id, new CSClod(id));

		return Instance.m_Clods[id];

	}

	public static void RemoveClod(int id)
	{
		if (Instance == null)
		{
			Debug.Log("The CSClodsMgr is null.");
			return;
		}

		Instance.m_Clods.Clear();
		Instance.m_Clods.Remove(id);
	}

	public static void Clear()
	{
		if (Instance == null)
		{
			Debug.Log("The CSClodsMgr is null.");
			return;
		}

		foreach (KeyValuePair<int, CSClod> kvp in Instance.m_Clods)
			kvp.Value.Clear();
		Instance.m_Clods.Clear();
	}


	// <CSVD> Main Data Managers Imports 
	public void Import( BinaryReader r )
	{
		int version = r.ReadInt32();

        //switch ( version )
        //{
        //case 0x0113:
        //case 0x0114:
        //case 0x0115:
        //case 0x0116:
        //{
        if (version >= CSDataMgr.VERSION000)
        {
            int count = r.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                int id = r.ReadInt32();
                CSClod clod = new CSClod(id);
                clod.Import(r);

                m_Clods.Add(id, clod);
            }
        }
        //} return true;
        //default: 
        //    break;
        //}
		
        //return false;
	}

	public void Export(BinaryWriter w)
	{
		w.Write(CSDataMgr.CUR_VERSION);

		w.Write(m_Clods.Count);

		foreach (KeyValuePair<int, CSClod> kvp in m_Clods)
		{
			w.Write(kvp.Key);
			kvp.Value.Export(w);
		}
	}
}


/// <summary>
/// --------- [!!! Obsolete !!!] ---------------------------------------------------
/// </summary>
/// 
public class CSClodMgr 
{
	public const byte TerrainType = 19;
	static private CSClodMgr s_Instance = null;
	static public CSClodMgr Instance { get {return s_Instance;} }

	public const int CHUNK_SIZE = 8;
	public Dictionary<IntVec3, Vector3> m_ClodLocas;
	private Dictionary<IntVec3, int> m_IdleClods;
	
	public Dictionary<IntVec3, ClodChunk> m_ClodChunks;
	private Dictionary<IntVec3, int> m_IdleChunks;
	
	public CSClodMgr()
	{
		m_ClodLocas = new Dictionary<IntVec3, Vector3>();
		//m_IdleClods = new List<IntVector3>();
		m_IdleClods = new Dictionary<IntVec3, int>();


		m_ClodChunks = new Dictionary<IntVec3, ClodChunk>();
		m_IdleChunks = new Dictionary<IntVec3, int>();
	}

	public static void Init ()
	{
		if (s_Instance != null)
		{
			Debug.Log("The CSCloMgr is areadly.");
			return;
		}
		s_Instance = new CSClodMgr();
	}

	public static void Clear()
	{
		if (s_Instance != null)
		{
			s_Instance.m_ClodLocas.Clear();
			s_Instance.m_IdleClods.Clear();
			s_Instance.m_IdleChunks.Clear();
			s_Instance.m_ClodChunks.Clear();
		}
	}

	public static void AddClod(Vector3 pos, bool dirty = false)
	{
		if (s_Instance == null)
		{
			Debug.Log("Clod manager is not exist.");  
			return;
		}

		IntVec3 index_pos = new IntVec3(pos);
		IntVec3 chunkIndex = GetChunkIndex(index_pos);
		if (!s_Instance.m_ClodChunks.ContainsKey(chunkIndex))
		{
			ClodChunk cc = new ClodChunk();
			cc.m_ChunkIndex = chunkIndex;
			s_Instance.m_ClodChunks.Add(chunkIndex, cc);
			s_Instance.m_ClodChunks[chunkIndex].AddClod(pos, dirty);
		}
		else
		{
			s_Instance.m_ClodChunks[chunkIndex].AddClod(pos, dirty);
		}

		s_Instance.m_IdleChunks[chunkIndex] = 0;

	}

	public static void DeleteClod(Vector3 pos)
	{
		if (s_Instance == null)
		{
			Debug.Log("Clod manager is not exist.");
			return;
		}

		IntVec3 chunkIndex = GetChunkIndex(new IntVec3(pos));
		if (s_Instance.m_ClodChunks.ContainsKey(chunkIndex))
		{
			s_Instance.m_ClodChunks[chunkIndex].DeleteClod(pos);
			if (s_Instance.m_ClodChunks[chunkIndex].m_Clods.Count == 0)
			{
				s_Instance.m_IdleChunks.Remove(chunkIndex);
				s_Instance.m_ClodChunks.Remove(chunkIndex);
			}
		}

	}

	public static void DirtyTheClod(Vector3 pos, bool dirty)
	{
		if (s_Instance == null)
		{
			Debug.Log("Clod manager is not exist.");
			return;
		}

		IntVec3 chunkIndex = GetChunkIndex(new IntVec3(pos)); 
		if (s_Instance.m_ClodChunks.ContainsKey(chunkIndex))
		{
			s_Instance.m_ClodChunks[chunkIndex].DirtyTheClod(pos, dirty);

			if (!dirty)
				s_Instance.m_IdleChunks[chunkIndex] = 0;
		}

	}

	public static Vector3 FindCleanClod()
	{
		if (s_Instance == null)
		{
			Debug.Log("Clod manager is not exist.");
			return Vector3.zero;
		}

		return Vector3.zero;
	}
	
	public static Vector3 FindCleanClod (Vector3 center, float radius)
	{
		if (s_Instance == null)
		{
			Debug.Log("Clod manager is not exist.");
			return Vector3.zero;
		}

		if (s_Instance.m_IdleClods.Count != 0)
		{
			float sqrRad = radius*radius;
			foreach (IntVec3 ivec in s_Instance.m_IdleClods.Keys)
			{
				Vector3 vec = new Vector3(center.x - ivec.x, center.y - ivec.y, center.z - ivec.z);
				if ( vec.sqrMagnitude < sqrRad)
					return s_Instance.m_ClodLocas[ ivec ];
			}

			return Vector3.zero;
		}

		return Vector3.zero;
	}

	public static IntVec3 GetChunkIndex (IntVec3 clodIndex)
	{
		return new IntVec3(clodIndex.x / CHUNK_SIZE, clodIndex.y / CHUNK_SIZE, clodIndex.z / CHUNK_SIZE);
	}


	public static ClodChunk FindCleanChunk(Vector3 center, float radius)
	{
		if (s_Instance == null)
		{
			Debug.Log("Clod manager is not exist.");
			return null;
		}

		if (s_Instance.m_IdleChunks.Count != 0)
		{
			float sqrRad = (radius + 1 )* (radius + 1);
			foreach (IntVec3 ivec in s_Instance.m_IdleChunks.Keys)
			{
				Vector3 vec = new Vector3(center.x - ivec.x * CHUNK_SIZE, center.y - ivec.y * CHUNK_SIZE, center.z - ivec.z * CHUNK_SIZE);
				if ( vec.sqrMagnitude < sqrRad && s_Instance.m_ClodChunks[ivec].m_IdleClods.Count != 0)
					return s_Instance.m_ClodChunks[ivec]; 
			}
		}

		return null;
	}

	public static void DirtyTheChunk (IntVec3 chunkIndex, bool dirty)
	{
		if (s_Instance == null)
		{
			Debug.Log("Clod manager is not exist.");
			return;
		}

		if (s_Instance.m_ClodChunks.ContainsKey(chunkIndex))
		{
			if (dirty)
				s_Instance.m_IdleChunks.Remove(chunkIndex);
			else
				s_Instance.m_IdleChunks[chunkIndex] = 0;
		}

	}
	

	/// <summary>
	/// <CSVD> Clod manager main import
	/// </summary>
	public void Import( BinaryReader r )
	{
		int version = r.ReadInt32();

        //switch ( version )
        //{
        //case 0x0105:
        //case 0x0106:
        //case 0x0107:
        //{
        //    int count = r.ReadInt32();
        //    for (int i = 0; i < count; i++)
        //    {
        //        Vector3 vec = new Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
        //        IntVec3 index_pos = new IntVec3(vec);
        //        m_ClodLocas.Add(index_pos,vec);
        //        m_IdleClods.Add(index_pos, 0);
        //    }

        //    count = r.ReadInt32();
        //    for (int i = 0; i < count; i++)
        //    {
        //        Vector3 vec = new Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
        //        IntVec3 index_pos = new IntVec3(vec);
        //        m_ClodLocas.Add(index_pos, vec);
        //    }
        //}
        //return true;
        //case 0x0108:
        //case 0x0109:
        //case 0x0110:
        //case 0x0111:
        //case 0x0112:
        //case 0x0113:
        //case 0x0114:
        //case 0x0115:
        //case 0x0116:
        //{
            if (version >= CSDataMgr.VERSION000)
            {
                int count = r.ReadInt32();
                for (int i = 0; i < count; i++)
                {
                    ClodChunk cc = new ClodChunk();
                    cc.Import(r, version);
                    m_ClodChunks.Add(cc.m_ChunkIndex, cc);
                    m_IdleChunks.Add(cc.m_ChunkIndex, 0);
                }

                count = r.ReadInt32();
                for (int i = 0; i < count; i++)
                {
                    ClodChunk cc = new ClodChunk();
                    cc.Import(r, version);
                    m_ClodChunks.Add(cc.m_ChunkIndex, cc);
                    if (cc.HasIdleClods())
                        m_IdleChunks[cc.m_ChunkIndex] = 0;
                }
            }
			
        //}
        //return true;
        //default: 
        //    break;
        //}

        //return false;
	}

	public void Export(BinaryWriter w)
	{
		w.Write(CSDataMgr.VERSION000);

		w.Write(m_IdleChunks.Count);
		Dictionary<IntVec3, ClodChunk> tempChunks = new Dictionary<IntVec3, ClodChunk>();
		foreach (KeyValuePair<IntVec3, ClodChunk> kvp in m_ClodChunks)
			tempChunks.Add(kvp.Key, kvp.Value);

		foreach (IntVec3 vec3 in m_IdleChunks.Keys)  
		{
			ClodChunk cc = m_ClodChunks[vec3];
			cc.Export(w);

			tempChunks.Remove(vec3);
		}

		w.Write(tempChunks.Count);
		foreach (KeyValuePair<IntVec3, ClodChunk> kvp in tempChunks)
		{
			kvp.Value.Export(w);
		}
	}
	
}


