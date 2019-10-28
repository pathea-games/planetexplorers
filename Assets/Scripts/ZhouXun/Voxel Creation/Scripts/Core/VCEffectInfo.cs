using UnityEngine;
using System.Collections;

public enum EVCEffect
{
	Point = 1
}

public class VCEffectInfo
{
	public int m_ID;
	public int m_ItemID;
	public EVCEffect m_Type;
	public string m_Name;
	public string m_ResPath;
	public GameObject m_ResObj;
	public string m_IconPath;
	public Texture2D m_IconTex;
	public float m_SellPrice;
}
