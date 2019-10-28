using UnityEngine;

// Voxel creation parts' information node from config. 

public class VCPartInfo
{
	public int m_ID;
	public int m_ItemID;
	public int m_CostCount;
	public EVCComponent m_Type;
	public string m_Name{ get { return ItemAsset.ItemProto.GetName(m_ItemID); } }
	public string m_ResPath;
	public GameObject m_ResObj;
	public string m_IconPath;
	public float m_SellPrice;
	public float m_Weight;
	public float m_Volume;
	public int m_MirrorMask;
	public int m_Symmetric;
}
