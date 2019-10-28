using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathea;
using System.IO;

[System.Serializable]
public class UIBuildMenuItemData
{
	public int m_Index;
	public int m_TargetIndex;
	public int m_Type;
	public int m_SubsetIndex; 
	public int m_ItemId;
	public string m_IconName;
}

public class UIBlockSaver : ArchivableSingleton<UIBlockSaver> 
{



	private Dictionary<int, UIBuildMenuItemData> m_Datas = new Dictionary<int, UIBuildMenuItemData>();
	public Dictionary<int, UIBuildMenuItemData> Datas { get { return m_Datas;} }

	private bool m_First = true;
	public bool First { get { return m_First; } set { m_First = value;}}

	public List<UIBuildMenuItemData> GetPageItemDatas (int page_index, int page_count)
	{
		List<UIBuildMenuItemData> result = new List<UIBuildMenuItemData>();
		foreach (var kvp in m_Datas)
		{
			int _pi = kvp.Key / page_count;

			if (_pi == page_index)
			{
				result.Add(kvp.Value);
			}
		}

		return result;
	}

	public bool Contains (int index)
	{
		return m_Datas.ContainsKey(index);
	}

	public void SetData (int index, UIBuildWndItem item)
	{
		UIBuildMenuItemData data = null;
		if (m_Datas.ContainsKey (index))
			data = m_Datas[index];
		else
		{
			data = new UIBuildMenuItemData();
			m_Datas.Add(index, data);
		}

		data.m_Index = item.mIndex;
		data.m_TargetIndex = item.mTargetIndex;
		data.m_Type = (int)item.mTargetItemType;
		data.m_IconName = item.mContentSprite.spriteName;
		data.m_SubsetIndex = item.mSubsetIndex;
		data.m_ItemId = item.ItemId;
	}

	public void AddData (UIBuildMenuItemData data)
	{
		m_Datas[data.m_Index] = data;
	}

	public bool RemoveData (int index)
	{
		return m_Datas.Remove (index);
	}


	protected override bool GetYird()
	{
		return false;
	}

	int m_Version = 0x0000002;

	protected override void SetData(byte[] data)
	{
		if (data == null)
			return;
		try
		{
			using ( MemoryStream ms_iso = new MemoryStream (data) )
			{
				BinaryReader r = new BinaryReader (ms_iso);

				int version = r.ReadInt32();

				switch (version)
				{
				case 0x0000001:
				{
					m_First = false;
					int count = r.ReadInt32();

					for (int i = 0; i < count; i++)
					{
						UIBuildMenuItemData item = new UIBuildMenuItemData();
						int key = r.ReadInt32();

						item.m_Index = r.ReadInt32();
						item.m_TargetIndex = r.ReadInt32();
						item.m_Type  = r.ReadInt32();
						item.m_SubsetIndex = r.ReadInt32();
						item.m_IconName = r.ReadString();
						item.m_ItemId = r.ReadInt32();

						m_Datas.Add(key, item);
					}
				}break;
				case 0x0000002:
				{
					m_First = r.ReadBoolean();

					int count = r.ReadInt32();
					
					for (int i = 0; i < count; i++)
					{
						UIBuildMenuItemData item = new UIBuildMenuItemData();
						int key = r.ReadInt32();
						
						item.m_Index = r.ReadInt32();
						item.m_TargetIndex = r.ReadInt32();
						item.m_Type  = r.ReadInt32();
						item.m_SubsetIndex = r.ReadInt32();
						item.m_IconName = r.ReadString();
						item.m_ItemId = r.ReadInt32();
						
						m_Datas.Add(key, item);
					}
				}break;
				default:
					break;
				}

				r.Close();
			}
		}
		catch (System.Exception e)
		{
			Debug.LogWarning(e);
		}
	}

	protected override void WriteData(BinaryWriter w)
	{
		w.Write(m_Version);
		w.Write(m_First);
		w.Write(m_Datas.Count);

		foreach (var kvp in m_Datas)
		{
			w.Write(kvp.Key);
			w.Write(kvp.Value.m_Index);
			w.Write(kvp.Value.m_TargetIndex);
			w.Write(kvp.Value.m_Type);
			w.Write(kvp.Value.m_SubsetIndex);
			w.Write(kvp.Value.m_IconName);
			w.Write(kvp.Value.m_ItemId);
		}
	}

	public override void New()
	{
		m_Datas = new Dictionary<int, UIBuildMenuItemData>();
	}
}
