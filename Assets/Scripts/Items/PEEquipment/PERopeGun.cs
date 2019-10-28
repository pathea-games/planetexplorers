using UnityEngine;
using System.Collections.Generic;

public class PERopeGun : PEAimAbleEquip
{
	public Transform 		hook;
	public Transform 		gunMuzzle;
	public List<Transform>	lineList;
	
	public float			bulletSpeed = 100f;
	public float			climbSpeed = 100f;
	public Vector3			lineDownF = 0.01f * Vector3.down;
	public int				fireSound;
	public int				ropeSound;
	public float			range = 50f;
	public float			minDis = 8f;
	public float 			minClimbTime = 0.2f;
	public float 			moveDownSpeedScale = 1.5f;
	public int				hitEffectID;
	public LayerMask 		effectLayer;
}
