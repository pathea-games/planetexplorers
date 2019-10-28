using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class PhysicsCharacterMotor : MonoBehaviour 
{
	[HideInInspector]
	public float maxForwardSpeed = 25f;
	[HideInInspector]
	public float maxBackwardsSpeed = 15f;
	[HideInInspector]
	public float maxSidewaysSpeed = 18f;
	
	//float maxVelocityChange = 10f;
	
	public float gravity = 10.0f;
	float mImpactTime = 0;
	
	public bool mInWater = false;
	public float mDisToWater = 0;
	
	float FluidDragF = 0.16f;
	float AreaDragF = 0.006f;
	float MoveAcc = 40f;
	public float SpeedReduce = 0.1f;
	public bool ExStop = false;
	
	//float MaxMoveAngle = 80f;
	
	//Vector3 forwardVector = Vector3.forward;
	protected Quaternion alignCorrection;
	
	private bool m_Grounded = false;
	
	private bool mFreezeGravity = true;

	public bool	mGliderMode = false;
	
	public bool FreezeGravity
	{
		get { return mFreezeGravity; }
		set { mFreezeGravity = value; }
	}
	
	public bool grounded {
		get { return m_Grounded; }
		protected set { m_Grounded = value; }
	}

	public Vector3 Velocity { get{ return GetComponent<Rigidbody>().velocity; } }
	
	public bool mJumpFlag = false;
#if UNITY_EDITOR
	public float mSpeedTimes = 1f;
#endif
	
	public void ResetSpeed(float mspeed)
	{
#if UNITY_EDITOR
		mspeed *= mSpeedTimes;
#endif
		maxForwardSpeed = mspeed;
		maxBackwardsSpeed = maxForwardSpeed*0.75f;
		maxSidewaysSpeed = maxForwardSpeed*0.95f;
		//maxVelocityChange = maxForwardSpeed*0.3f;
	}
	
	private Vector3 m_desiredMovementDirection;
	private Vector3 m_desiredFacingDirection;

	public Vector3 desiredMovementDirection {
		get { return m_desiredMovementDirection; }
		set {
			m_desiredMovementDirection = value;
			if (m_desiredMovementDirection.magnitude>1) m_desiredMovementDirection = m_desiredMovementDirection.normalized;
		}
	}
	public Vector3 desiredVelocity {
		get {
			//return m_desiredVelocity;
			if (m_desiredMovementDirection==Vector3.zero) return Vector3.zero;
			else {
				float zAxisEllipseMultiplier = (m_desiredMovementDirection.z>0 ? maxForwardSpeed : maxBackwardsSpeed) / maxSidewaysSpeed;
				Vector3 temp = new Vector3(m_desiredMovementDirection.x, 0, m_desiredMovementDirection.z/zAxisEllipseMultiplier).normalized;
				float length = new Vector3(temp.x, 0, temp.z*zAxisEllipseMultiplier).magnitude * maxSidewaysSpeed;
				Vector3 velocity = m_desiredMovementDirection * length;
				return transform.rotation * velocity;
			}
		}
	}
	
	void Awake () {
		GetComponent<Rigidbody>().freezeRotation = true;
		GetComponent<Rigidbody>().useGravity = false;
	}

	public void ApplyImpact(Vector3 speedImpact)
	{
		GetComponent<Rigidbody>().AddForce(speedImpact, ForceMode.VelocityChange);
		mImpactTime = Time.time;
		grounded = false;
	}
	
	private void UpdateVelocity() {
		if(GetComponent<Rigidbody>().isKinematic)
			return;
		if(!mGliderMode)
		{
			Vector3 velocity = GetComponent<Rigidbody>().velocity;
			if (grounded) velocity = Util.ProjectOntoPlane(velocity, transform.up);
			
			Vector3 opDir =  desiredVelocity;
			Vector3 vecChange = opDir - velocity;
			Vector3 moveDir = Vector3.zero;
			
			if(opDir.sqrMagnitude > PETools.PEMath.Epsilon)
			{
				if(vecChange.sqrMagnitude > PETools.PEMath.Epsilon)
				{
					if(!mInWater)
						vecChange.y = 0;
					moveDir = vecChange.normalized;
				}
			}
			else if(grounded && ExStop)
			{
				Vector3 curVec = GetComponent<Rigidbody>().velocity;
				curVec.y = 0;
				if(curVec.sqrMagnitude > PETools.PEMath.Epsilon)
					GetComponent<Rigidbody>().AddForce(-SpeedReduce * curVec, ForceMode.VelocityChange);
			}
			
			GetComponent<Rigidbody>().AddForce(moveDir * MoveAcc, ForceMode.Acceleration);
			
			// Apply drag
			if(mInWater)
				GetComponent<Rigidbody>().AddForce(-FluidDragF * GetComponent<Rigidbody>().velocity.sqrMagnitude *  GetComponent<Rigidbody>().velocity.normalized
					, ForceMode.Acceleration);
			else
				GetComponent<Rigidbody>().AddForce(-AreaDragF *  GetComponent<Rigidbody>().velocity.sqrMagnitude *  GetComponent<Rigidbody>().velocity.normalized
					, ForceMode.Acceleration);

			// Apply gravity
//			if(!FreezeGravity && !mInWater)
				GetComponent<Rigidbody>().AddForce(gravity * Vector3.down,ForceMode.Acceleration);
			
			if(transform.position.y < -10f)
				transform.position = new Vector3(transform.position.x,-10f,transform.position.z);
		}
		grounded = false;
	}
	
	void OnCollisionStay (Collision collisionInfo) 
	{
		//if(collisionInfo.gameObject.layer == Pathea.Layer.VFVoxelTerrain)
		if(Time.time - mImpactTime > 0.1f)
			grounded = true;
		
		if(!GetComponent<Rigidbody>().isKinematic && collisionInfo.gameObject.layer == Pathea.Layer.AIPlayer)
		{
			GetComponent<Rigidbody>().velocity = new Vector3(GetComponent<Rigidbody>().velocity.x, 0, GetComponent<Rigidbody>().velocity.y);
		}
	}

	void FixedUpdate () {
		UpdateVelocity();
		
		GetComponent<Rigidbody>().AddForce(50f * Dir, ForceMode.Acceleration);
	}
	
	public Vector3 Dir = Vector3.zero;
	
}
