//#define STEER3D
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathea;
using Steer3D;
using PETools;

//[RequireComponent(typeof(Rigidbody))]
//[RequireComponent(typeof(CapsuleCollider))]
public class PEMotorPhysics : PEMotor
{
	public float maxRotationSpeed = 270;
    public float walkSmoothSpeed = 30.0f;
    public float runSmoothSpeed = 90.0f;
	public bool useCentricGravity = false;
	public LayerMask groundLayers;
	public Vector3 gravityCenter = Vector3.zero;

    Animator animator;
    Rigidbody rigid;
    MovementPrison m_Prison;
    PeEntity m_Entity;

    HashSet<string> parameters;

#if STEER3D
    SteerAgent m_Steer;
#endif

    bool animMoving;

    bool m_AnimRotation = false;
    Vector3 m_AnimPos = Vector3.zero;
    Quaternion m_AnimRot = Quaternion.identity;

    public void Init(PeEntity entity)
    {
        m_Prison = new MovementPrison(entity, this, rigid);
    }

    void Awake () {

        rigid = GetComponent<Rigidbody>();

        groundLayers = 1 << Pathea.Layer.VFVoxelTerrain
                        | 1 << Pathea.Layer.SceneStatic;

#if STEER3D
        m_Steer = GetComponent<SteerAgent>();
        if(m_Steer != null)
            m_Steer.manualUpdate = true;
#endif

        if (rigid != null)
        {
            rigid.freezeRotation = true;
            rigid.useGravity = false;
        }

        animator = GetComponentInChildren<Animator>();
        parameters = new HashSet<string>();

        if(animator != null)
        {
            AnimatorControllerParameter[] animParameters = animator.parameters;
            for (int i = 0; i < animParameters.Length; i++)
                parameters.Add(animParameters[i].name);
        }
	}

    new public void Start()
    {
        base.Start();

        m_Entity = GetComponentInParent<PeEntity>();
    }

    public override Vector3 velocity
    {
        get
        {
            return tracker != null ? tracker.velocity : (rigid != null ? rigid.velocity : Vector3.zero);
        }
    }

    public override Vector3 angleVelocity
    {
        get
        {
            return tracker != null ? tracker.angularVelocity : (rigid != null ? rigid.angularVelocity : Vector3.zero);
        }
    }

    void SetFloat(string name, float value)
    {
        if (animator != null && (parameters == null || parameters.Contains(name)))
            animator.SetFloat(name, value);
    }

    void SetFloat(string name, float value, float dampTime, float deltaTime)
    {
        if (animator != null && (parameters == null || parameters.Contains(name)))
            animator.SetFloat(name, value, dampTime, deltaTime);
    }

    Vector3 GetAdjuestedUp()
    {
        Vector3 adjuestUp = Vector3.zero;

        //for (int i=0; i<4; i++) {
        //	Vector3 rayStart =
        //		transform.position
        //			+ transform.up
        //			+ Quaternion.AngleAxis(360*i/4.0f, transform.up)
        //				* (transform.right*0.5f)
        //			+ desiredVelocity*0.2f;
        //	if ( Physics.Raycast(rayStart, -transform.up, out hit, 5.0f, groundLayers.value) ) 
        //          {
        //              adjuestUp += hit.normal;
        //	}
        //}

		RaycastHit hit;
        if (m_Entity != null)
        {
            Bounds bound = m_Entity.bounds;
            Vector3[] ver = new Vector3[8];
            for (int i = 0; i < 8; i++)
            {
                if ((i & 4) == 0)
                {
                    if ((i & 1) == 0)
                        ver[i] -= bound.extents.x * new Vector3(1, 0, 0);
                    else
                        ver[i] += bound.extents.x * new Vector3(1, 0, 0);

                    if ((i & 2) == 0)
                        ver[i] -= bound.extents.z * new Vector3(0, 0, 1);
                    else
                        ver[i] += bound.extents.z * new Vector3(0, 0, 1);
                }
                else
                {
                    if ((i & 2) == 0)
                    {
                        if ((i & 1) == 0)
                            ver[i] -= bound.extents.x * new Vector3(1, 0, 0);
                        else
                            ver[i] += bound.extents.x * new Vector3(1, 0, 0);
                    }
                    else
                    {
                        if ((i & 1) == 0)
                            ver[i] -= bound.extents.z * new Vector3(0, 0, 1);
                        else
                            ver[i] += bound.extents.z * new Vector3(0, 0, 1);
                    }
                }

                Vector3 pos = transform.TransformPoint(ver[i]);
                if (Physics.Raycast(pos, -transform.up, out hit, 5.0f, groundLayers.value))
                {
                    if(Vector3.Angle(hit.normal, Vector3.up) < 75.0f)
                        adjuestUp += hit.normal;

                    //Debug.DrawLine(pos, hit.point, Color.cyan);
                }

            }
        }
        return adjuestUp;
    }
	
	private void AdjustToGravity() {
		int origLayer = gameObject.layer;
		gameObject.layer = 2;
		
		Vector3 currentUp = transform.up;
		//Vector3 gravityUp = (transform.position-gravityCenter).normalized;
		
		float damping = Mathf.Clamp01(Time.deltaTime*5);

		Vector3 desiredUp = GetAdjuestedUp();
        
		desiredUp = (currentUp+desiredUp).normalized;
		Vector3 newUp = (currentUp+desiredUp*damping).normalized;
		
		float angle = Vector3.Angle(currentUp,newUp);
		if (angle>0.01) {
			Vector3 axis = Vector3.Cross(currentUp,newUp).normalized;
			Quaternion rot = Quaternion.AngleAxis(angle,axis);
			transform.rotation = rot * transform.rotation;
		}
		
		gameObject.layer = origLayer;
	}
	
	private void UpdateFacingDirection() {
        if (m_Entity != null && m_Entity.netCmpt != null && !m_Entity.netCmpt.IsController)
        {
            SetFloat("Direction", angleVelocity.y, 0.25f, Time.deltaTime);
        }
        else
        {
            if (m_AnimRotation)
            {
                transform.rotation = m_AnimRot;
                SetFloat("Direction", 0.0f);
            }
            else
            {
                // Calculate which way character should be facing
                float facingWeight = desiredFacingDirection.magnitude;
                Vector3 combinedFacingDirection = (
                    transform.rotation * desiredMovementDirection * (1 - facingWeight)
                    + desiredFacingDirection * facingWeight
                );
                combinedFacingDirection = Util.ProjectOntoPlane(combinedFacingDirection, transform.up);
                combinedFacingDirection = alignCorrection * combinedFacingDirection;

                if (combinedFacingDirection.sqrMagnitude > 0.01f * 0.01f)
                {
                    Vector3 newForward = Util.ConstantSlerp(
                        transform.forward,
                        combinedFacingDirection,
                        maxRotationSpeed * Time.deltaTime
                    );

                    newForward = Util.ProjectOntoPlane(newForward, transform.up);
                    Quaternion q = new Quaternion();
                    q.SetLookRotation(newForward, transform.up);
                    transform.rotation = q;

                    float value = Vector3.Angle(transform.forward, combinedFacingDirection.normalized) / 135.0f;

                    if (Vector3.Cross(transform.forward, combinedFacingDirection.normalized).y > 0)
                        SetFloat("Direction", value, 0.25f, Time.deltaTime);
                    else
                        SetFloat("Direction", -value, 0.25f, Time.deltaTime);

                    //if (Vector3.Dot(transform.forward, combinedFacingDirection.normalized) > 0)
                    //    SetFloat("Direction", Vector3.Cross(transform.forward, combinedFacingDirection.normalized).y, 0.25f, Time.deltaTime);
                    //else
                    //    SetFloat("Direction", Vector3.Cross(transform.forward, combinedFacingDirection.normalized).y > 0 ? 1 : -1, 0.25f, Time.deltaTime);

                    //SetFloat("Angle", PETools.PEUtil.GetAngle(transform.forward, combinedFacingDirection, Vector3.up));
                }
                else
                {
                    //SetFloat("Angle", 0.0f);
                    SetFloat("Direction", 0.0f, 0.25f, Time.deltaTime);
                }
            }
        }
    }
	
	private void UpdateVelocity() {
        if(m_Entity != null && m_Entity.netCmpt != null && !m_Entity.netCmpt.IsController)
        {
            Vector3 direction = Util.ProjectOntoPlane(velocity, transform.up);
            if (velocity.sqrMagnitude <= 0.1f * 0.1f)
                SetFloat("Speed", 0.0f);
            else
            {
                float speedValue = PETools.PEUtil.Magnitude(velocity, false);
                if (Vector3.Dot(transform.forward, direction.normalized) > 0 || Vector3.Angle(transform.forward, direction.normalized) < 150)
                    SetFloat("Speed", speedValue, 0.15f, Time.deltaTime);
                else
                    SetFloat("Speed", -speedValue, 0.15f, Time.deltaTime);
            }
        }
        else
        {
//            desiredMovementEffect = Mathf.Lerp(desiredMovementEffect.magnitude, 0.0f, 0.1f) * desiredMovementEffect.normalized;

            if (m_AnimPos != Vector3.zero)
            {
                Vector3 velocityChange = m_AnimPos - transform.position;

                if (m_Prison == null || m_Prison.CalculateVelocity(ref velocityChange))
                    rigid.AddForce(velocityChange, ForceMode.VelocityChange);
                else
                    rigid.velocity = Vector3.zero;

                SetFloat("Speed", 0.0f);
            }
            else
            {
                //rigid.isKinematic = false;
                Vector3 curVelocity = rigid.velocity;
                if (grounded) curVelocity = Util.ProjectOntoPlane(curVelocity, transform.up);

                // Calculate how fast we should be moving
                jumping = false;
                if (grounded || Mathf.Abs(gravity) < PETools.PEMath.Epsilon)
                {
                    // Apply a force that attempts to reach our target velocity

                    Vector3 newVelocity = desiredVelocity;

                    if (desiredMovementEffect.sqrMagnitude > 0.1f * 0.1f)
                        newVelocity = desiredMovementEffect;

                    if (m_Prison != null && !m_Prison.CalculateVelocity(ref newVelocity))
                        newVelocity = Vector3.zero;

                     Vector3 velocityChange = (newVelocity - curVelocity);

                    if (velocityChange.magnitude > maxVelocityChange && desiredMovementEffect.sqrMagnitude < 0.1f*0.1f)
                    {
                        velocityChange = velocityChange.normalized * maxVelocityChange;
                    }

                    if (gravity > PETools.PEMath.Epsilon && velocityChange.y > 0.2f)
                    {
                        velocityChange = new Vector3(velocityChange.x, 0.2f, velocityChange.z);
                    }
                    
                    rigid.AddForce(velocityChange, ForceMode.VelocityChange);

                    // Jump
                    //if (canJump && Input.GetButton("Jump")) {
                    //    rigidbody.velocity = velocity + transform.up * Mathf.Sqrt(2 * jumpHeight * gravity);
                    //    jumping = true;
                    //}
                }
                else
                {
                    if (desiredVelocity == Vector3.zero && gravity > PETools.PEMath.Epsilon)
                    {
                        Vector3 v = curVelocity;
                        v.y = 0.0f;
                        rigid.AddForce(-v, ForceMode.VelocityChange);
                    }
                }

                // Apply downwards gravity
                rigid.AddForce(transform.up * -gravity * rigid.mass);

                Vector3 direction = Util.ProjectOntoPlane(rigid.velocity, transform.up);
                if (rigid.velocity.sqrMagnitude <= 0.1f * 0.1f)
                    SetFloat("Speed", 0.0f);
                else
                {
                    float speedValue = PETools.PEUtil.Magnitude(rigid.velocity, false);
                    if (Vector3.Dot(transform.forward, direction.normalized) > 0 || Vector3.Angle(transform.forward, direction.normalized) < 150)
                        SetFloat("Speed", speedValue, 0.15f, Time.deltaTime);
                    else
                        SetFloat("Speed", -speedValue, 0.15f, Time.deltaTime);
                }
            }
        }
		
		grounded = false;
	}

    void OnCollisionStay()
    {
        grounded = true;
    }

	//void OnCollisionEnter () {
 //       m_CollisionCount++;

	//	grounded = true;
	//}

 //   void OnCollisionExit () {
 //       m_CollisionCount--;

 //       if(m_CollisionCount <= 0)
 //       {
 //           m_CollisionCount = 0;
	//	    grounded = false;
 //       }
	//}
	bool _bFixedUpdated = false;
	void FixedUpdate () {
        if (_bFixedUpdated)
            return;

        _bFixedUpdated = true;
#if STEER3D
        if (m_Steer != null) m_Steer.ManualUpdate();
#endif

		//if (useCentricGravity) AdjustToGravity();

        //if (!animMoving)
        {
            //UpdateFacingDirection();

            UpdateVelocity();
        }
	}

    void Update()
    {
		_bFixedUpdated = false;
        if (useCentricGravity) AdjustToGravity();

        UpdateFacingDirection();
    }

    public void OnAnimatorMove()
    {
        if (rigid == null) return;

        Vector3 velocity = animator.deltaPosition;
        if (velocity.sqrMagnitude > 0.1f * 0.1f)
        {
            if (m_AnimPos == Vector3.zero)
                rigid.velocity = Vector3.zero;

            m_AnimPos = transform.position + velocity;
        }
        else
        {
            if(m_AnimPos != Vector3.zero)
                rigid.velocity = Vector3.zero;

            m_AnimPos = Vector3.zero;
        }

        float qx = animator.deltaRotation.x;
        float qy = animator.deltaRotation.y;
        float qz = animator.deltaRotation.z;
        float qv = qx * qx + qy * qy + qz * qz;

        if (qv > 0.01f * 0.01f)
        {
            m_AnimRotation = true;
            m_AnimRot = animator.deltaRotation * transform.rotation;
        }
        else
        {
            m_AnimRotation = false;
            m_AnimRot = Quaternion.identity;
        }
    }

    public override void Stop()
    {
        base.Stop();

        if (rigid != null)
            rigid.velocity = Vector3.zero;
    }

    public override void Reset()
    {
        m_AnimPos = Vector3.zero;
        m_AnimRot = Quaternion.identity;
    }
}

public class MovementPrison
{
    PeEntity m_Entity;
    PEMotorPhysics m_Motor;
    Rigidbody m_Rigidbody;

    #region constructor
    public MovementPrison(PeEntity entity, PEMotorPhysics motor, Rigidbody rigid)
    {
        m_Entity = entity;
        m_Motor = motor;
        m_Rigidbody = rigid;
    }
    #endregion

    #region public interface

    public bool CalculateVelocity(ref Vector3 velocity)
    {
        if (m_Entity.Field == MovementField.Land)
            return CalculateVelocityLand(ref velocity);
        else if (m_Entity.Field == MovementField.water)
            return CalculateVelocityWater(ref velocity);
        else if (m_Entity.Field == MovementField.Sky)
            return CalculateVelocitySky(ref velocity);
        else if (m_Entity.Field == MovementField.Amphibian)
            return CalculateVelocityAmphibian(ref velocity);
        else if (m_Entity.Field == MovementField.All)
            return CalculateVelocityAll(ref velocity);
        else
            return true;
    }

    #endregion

    #region private function

    bool CalculateVelocityLand(ref Vector3 velocity)
    {
        Vector3 v = new Vector3(velocity.x, 0.0f, velocity.z);
        if (m_Entity != null && m_Entity.peTrans != null && v.sqrMagnitude > 0.01f * 0.01f)
        {
            Vector3 vv = v.normalized * Time.deltaTime * m_Motor.maxForwardSpeed * 5.0f;

            Vector3 vff = m_Entity.peTrans.forwardBottom;
            Vector3 vfc = m_Entity.peTrans.forwardCenter;

            Vector3 nvff = vff + vv;
            Vector3 nvfc = vfc + vv;

            Vector3 vg;
            if (!PEUtil.CheckPositionOnGround(nvff, out vg, 5.0f, 5.0f, GameConfig.GroundLayer))
                return false;
            else
            {
                if (PEUtil.CheckPositionUnderWater(nvfc))
                {
                    if (!RandomDungenMgrData.InDungeon)
                    {
                        if (PEUtil.CheckPositionOnGround(nvfc + v.normalized * 32.0f, out vg, 128.0f, 128.0f, GameConfig.GroundLayer))
                        {
                            if (PEUtil.CheckPositionUnderWater(vg))
                                return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }

        return true;
    }

    bool CalculateVelocitySky(ref Vector3 velocity)
    {
        Vector3 _velocity = velocity;
        if (_velocity.sqrMagnitude > 0.01f * 0.01f)
        {
            _velocity = _velocity.normalized * Time.deltaTime * m_Motor.maxForwardSpeed * 10.0f;
            Vector3 _nextPosition = m_Entity.peTrans.position + _velocity;

            float height;
            if (PEUtil.GetWaterSurfaceHeight(_nextPosition, out height))
            {
                if (velocity.y < -PETools.PEMath.Epsilon)
                    velocity.y = 0.0f;

                if (m_Rigidbody.velocity.y < -PETools.PEMath.Epsilon)
                    velocity.y = -m_Rigidbody.velocity.y;
            }
        }

        return true;
    }

    bool CalculateVelocityWater(ref Vector3 velocity)
    {
        if (m_Entity != null && m_Entity.peTrans != null && velocity.sqrMagnitude > 0.01f * 0.01f)
        {
            float maxSpeed = m_Motor.maxForwardSpeed;
            Vector3 pos = m_Entity.peTrans.trans.TransformPoint(m_Entity.peTrans.bound.center) - Vector3.up * m_Entity.peTrans.bound.extents.y;
            Vector3 top = pos + Vector3.up * (m_Entity.peTrans.bound.size.y + 0.5f);
            Vector3 vel = Vector3.up * Time.deltaTime * maxSpeed * 5;
            if (!PEUtil.CheckPositionUnderWater(top + vel))
            {
                if(velocity.y > PETools.PEMath.Epsilon)
                    velocity.y = 0.0f;

                if (m_Rigidbody.velocity.y > PETools.PEMath.Epsilon)
                    velocity.y = -m_Rigidbody.velocity.y;

                float height = VFVoxelWater.self.DownToWaterSurface(top.x, top.y, top.z) ;
                if(height > PETools.PEMath.Epsilon)
                    velocity -= Vector3.up * Mathf.Clamp01(height);

                Vector3 movement = new Vector3(velocity.x, 0.0f, velocity.z);
                if (movement.sqrMagnitude > 0.01f * 0.01f)
                {
                    Vector3 dir = movement.normalized * maxSpeed * Time.deltaTime * 5.0f;
                    float distance = Mathf.Max(m_Entity.peTrans.bound.extents.x, m_Entity.peTrans.bound.extents.z) + dir.magnitude + 1.0f;
                    if (Physics.Raycast(pos, dir, distance, GameConfig.GroundLayer))
                        return false;
                }
            }
        }

        return true;
    }

    bool CalculateVelocityAmphibian(ref Vector3 velocity)
    {
        return true;
    }

    bool CalculateVelocityAll(ref Vector3 velocity)
    {
        return true;
    }

    #endregion
}
