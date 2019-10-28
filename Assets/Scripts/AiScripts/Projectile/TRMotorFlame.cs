using UnityEngine;
using System.Collections;

public class TRMotorFlame : Trajectory
{	
	public float speed;
	public float gravity;
	public float angleMin;
	public float angleMax;
	
	Vector3 direction;
	Vector3 vx;
	Vector3 vy = Vector3.zero;

	void Start()
	{
		Emit();
	}

	public void Emit()
	{
		vx = Quaternion.AngleAxis(Random.Range(angleMin, angleMax),Vector3.forward) * Quaternion.AngleAxis(Random.value * 360f, Vector3.up) * Vector3.up * speed;
	}
	
	public override Vector3 Track(float deltaTime)
	{
		vy += Vector3.down * gravity * deltaTime;
		
		return (vx + vy) * deltaTime;
	}	
}
