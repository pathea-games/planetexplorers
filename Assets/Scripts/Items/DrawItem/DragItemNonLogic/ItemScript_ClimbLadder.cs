using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ItemAsset;

public class ItemScript_ClimbLadder : ItemScript_Connection
{
	public enum OpSide
	{
		Both,
		Forward,
		Backward,
	}
	public OpSide m_OpSide;

	[Tooltip("楼梯高度")]
	public float m_LadderHeight;
	[Tooltip("楼梯宽度")]
	public float m_LadderWith;

	Pathea.Operate.PEClimbLadder _opnd;
	public Pathea.Operate.PEClimbLadder opClimb
	{
		get
		{
			return _opnd;
		}
	}
	
	void Awake()
	{
		_opnd = GetComponent<Pathea.Operate.PEClimbLadder> ();
		if (_opnd == null) {
			_opnd = gameObject.AddComponent<Pathea.Operate.PEClimbLadder> ();
		}
		_opnd.opSide = m_OpSide;
	}
}
