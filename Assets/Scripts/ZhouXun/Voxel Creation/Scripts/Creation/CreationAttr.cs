using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


// Discrete volume point for water floating
public struct VolumePoint
{
	public VolumePoint(Vector3 lpos, float pv, float nv)
	{
		localPosition = lpos;
		pos_volume = pv;
		neg_volume = nv;
	}

	public Vector3 localPosition;
	public float pos_volume;
	public float neg_volume;
}


// Creation property class only for temp saving and fast visit
[Serializable]
public class CreationAttr
{
	public CreationAttr ()
	{
		m_Cost = new Dictionary<int, int> ();
		m_Errors = new List<string> ();
		m_Warnings = new List<string> ();
	}

    public const float DefaultHeight = 1.3f;
	// Common properties
	public Vector3 m_CenterOfMass;
	public Dictionary<int, int> m_Cost;	// The real game item
	public ECreation m_Type = ECreation.Null;
	public float m_Volume;
	public float m_Weight;
	public float m_Durability;
	public float m_SellPrice;
    public Vector4 m_AtkHeight;    //sword AttackTrigger :Height值（双手剑有两个x,y:x_L,y_R)
	
	// Other properties
	public float m_Attack;
	public float m_Defense;				// 在 AI Turret 中用来表示是否可以充电
	public float m_MuzzleAtkInc;
	public float m_FireSpeed;
	public float m_Accuracy;
	public float m_DragCoef;
	public float m_MaxFuel;

	// For water floating
	public List<VolumePoint> m_FluidDisplacement;
	
	public List<string> m_Errors;
	public List<string> m_Warnings;

	//0.9 item id is change
	List<int> changeIDList = new List<int>();
	public void CheckCostId()
	{
		changeIDList.Clear();
		foreach (KeyValuePair<int,int> kv in m_Cost)
		{
			if ( kv.Key > 10100001)
			{
				changeIDList.Add(kv.Key);
			}
		}
		foreach (int key in changeIDList)
		{
			int id = ItemAsset.ItemProto.Mgr.GetIdFromPin(key);
			if (id != -1)
			{
				m_Cost.Add(id,m_Cost[key]);
				m_Cost.Remove(key);
			}
		}
	}
}
