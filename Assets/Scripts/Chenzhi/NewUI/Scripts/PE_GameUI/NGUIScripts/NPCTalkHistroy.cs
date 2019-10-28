using UnityEngine;
using System.Collections.Generic;
using Pathea;
using System.IO;

public class NPCTalkHistroy : ArchivableSingleton<NPCTalkHistroy>
{
	const int CURRENT_VERSION = 1;
	const int SaveCount = 20;
	public struct Histroy
	{
		public string npcName;
		public string countent;

	}

	List<Histroy> m_Histroy = new List<Histroy>();

	public List<Histroy> histroies { get { return m_Histroy;} }

	public System.Action<Histroy> onAddHistroy;

	public System.Action onRemoveHistroy;
	
	public void AddHistroy(string npcName, string countent)
	{
		for(int i = 0; i < m_Histroy.Count; ++i)
			if(countent == m_Histroy[i].countent)
				return;
		Histroy histroy = new Histroy();
		histroy.npcName = npcName;
		histroy.countent = countent;
		m_Histroy.Add(histroy);
		if(null != onAddHistroy)
			onAddHistroy(histroy);
		if(m_Histroy.Count > SaveCount)
		{
			m_Histroy.RemoveAt(0);
			if(null != onRemoveHistroy)
				onRemoveHistroy();
		}
	}

	public void Clear()
	{
		m_Histroy.Clear();
	}

	#region implemented abstract members of ArchivableSingleton

	protected override bool GetYird ()
	{
		return false;
	}

	protected override void WriteData (System.IO.BinaryWriter bw)
	{		
		bw.Write(CURRENT_VERSION);
		bw.Write(m_Histroy.Count);
		for(int i = 0; i < m_Histroy.Count; ++i)
		{
			bw.Write(m_Histroy[i].npcName);
			bw.Write(m_Histroy[i].countent);
		}
	}

	protected override void SetData (byte[] data)
	{
		m_Histroy.Clear();
		MemoryStream ms = new MemoryStream(data);
		BinaryReader _in = new BinaryReader(ms);
		
		_in.ReadInt32();
		int count = _in.ReadInt32();
		for(int i = 0; i < count; i++)
		{
			Histroy histroy = new Histroy();
			histroy.npcName = _in.ReadString();
			histroy.countent = _in.ReadString();
			m_Histroy.Add(histroy);
		}
		_in.Close();
		ms.Close();
	}
	#endregion
}
