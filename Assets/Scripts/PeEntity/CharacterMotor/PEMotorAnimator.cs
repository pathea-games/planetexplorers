using UnityEngine;
using System.Collections;
using Pathea;

public class PEMotorAnimator : PEMotor
{
    public float maxRotationSpeed = 270;

    Animator animator;

    Rigidbody rigid;

    Vector3 deltaPosition;
    Quaternion rootRotation;

    Locomotion locomotion;

    PeEntity m_Entity;

    public SpeedState speedState = SpeedState.None;

    void Awake()
    {
        animator = GetComponent<Animator>();
        rigid = GetComponent<Rigidbody>();

        locomotion = new Locomotion(animator);
    }

    new public void Start()
    {
        base.Start();

        m_Entity = GetComponentInParent<PeEntity>();
    }

    void FixedUpdate()
    {
        if (Time.deltaTime == 0 || Time.timeScale == 0)
            return;

        //UpdateFacingDirection();

        UpdateVelocity();
    }

    void UpdateFacingDirection()
    {
        // Calculate which way character should be facing
        float facingWeight = desiredFacingDirection.magnitude;
        Vector3 combinedFacingDirection = (
            transform.rotation * desiredMovementDirection * (1 - facingWeight)
            + desiredFacingDirection * facingWeight
        );
        combinedFacingDirection = Util.ProjectOntoPlane(combinedFacingDirection, transform.up);
        combinedFacingDirection = alignCorrection * combinedFacingDirection;

        if (combinedFacingDirection.sqrMagnitude > 0.01f)
        {
            Vector3 newForward = Util.ConstantSlerp(
                transform.forward,
                combinedFacingDirection,
                maxRotationSpeed * Time.deltaTime
            );
            newForward = Util.ProjectOntoPlane(newForward, transform.up);

            Vector3 faceDir = Vector3.Cross(transform.forward, combinedFacingDirection.normalized);

            //if (Vector3.Dot(transform.forward, combinedFacingDirection.normalized) > 0)
            //    animator.SetFloat("Direction", faceDir.y, 0.15f, Time.deltaTime);
            //else
            //    animator.SetFloat("Direction", faceDir.y > 0 ? 1 : -1, 0.15f, Time.deltaTime);

            animator.SetFloat("Direction", faceDir.y > 0 ? 1 : -1, 0.15f, Time.deltaTime);
        }
        else
        {
            animator.SetFloat("Direction", 0.0f, 0.15f, Time.deltaTime);
        }
    }

    void UpdateVelocity()
    {
        if (animator == null || m_Entity == null)
            return;

//        desiredMovementEffect = Mathf.Lerp(desiredMovementEffect.magnitude, 0.0f, 0.1f) * desiredMovementEffect.normalized;

        bool rootMotion = desiredMovementEffect.sqrMagnitude < 0.25f * 0.25f && (m_Entity.netCmpt == null || m_Entity.netCmpt.IsController);
        animator.applyRootMotion = rootMotion;

        if (desiredMovementEffect.sqrMagnitude < 0.25f * 0.25f)
        {
            float speed = 0.0f;

            if (m_Entity.netCmpt == null || m_Entity.netCmpt.IsController)
            {
                if (desiredVelocity != Vector3.zero)
                {
                    if (speedState == SpeedState.Retreat)
                        speed = -maxForwardSpeed;
                    else
                        speed = maxForwardSpeed;
                }
            }
            else
            {
                if(velocity.sqrMagnitude > 0.1f*0.1f)
                {
                    Vector3 direction = Util.ProjectOntoPlane(velocity, transform.up);
                    if (Vector3.Dot(transform.forward, direction.normalized) > 0 || Vector3.Angle(transform.forward, direction.normalized) < 165)
                        speed = maxForwardSpeed;
                    else
                        speed = -maxForwardSpeed;
                }
            }

            float angle = animator.GetFloat("Angle");

            locomotion.Do(speed, angle);
        }
        else
        {
            if (desiredMovementEffect.sqrMagnitude < 0.9f * 0.9f && rigid.velocity.sqrMagnitude < 1f * 1f)
                desiredMovementEffect = Vector3.zero;

            Vector3 velocity = rigid.velocity;

            if (grounded) velocity = Util.ProjectOntoPlane(velocity, transform.up);

            Vector3 newVelocity = desiredMovementEffect;

            Vector3 velocityChange = (newVelocity - velocity);

            rigid.AddForce(velocityChange, ForceMode.VelocityChange);
        }
    }

    //public void OnAnimatorMove()
    //{
    //    if (rigid != null)
    //    {
    //        rigid.MovePosition(rigid.position + animator.deltaPosition);
    //        rigid.MoveRotation(animator.deltaRotation * rigid.rotation);
    //    }
    //}

    void OnCollisionStay()
    {
        grounded = true;
    }

    void OnEnable()
    {
        animator.applyRootMotion = true;
    }

    void OnDisable()
    {
        animator.applyRootMotion = false;
        rigid.AddForce(-rigid.velocity, ForceMode.VelocityChange);
    }
}

public class Locomotion
{
    private Animator m_Animator = null;
    
    private int m_SpeedId = 0;
    private int m_AgularSpeedId = 0;
    private int m_DirectionId = 0;

    public float m_SpeedDampTime = 0.1f;
    public float m_AnguarSpeedDampTime = 0.25f;
    public float m_DirectionResponseTime = 0.2f;
    
    public Locomotion(Animator animator)
    {
        m_Animator = animator;

        m_SpeedId = Animator.StringToHash("Speed");
        m_AgularSpeedId = Animator.StringToHash("Direction");
        m_DirectionId = Animator.StringToHash("Angle");
    }

    public void Do(float speed, float direction)
    {
        AnimatorStateInfo state = m_Animator.GetCurrentAnimatorStateInfo(0);

        bool inTransition = m_Animator.IsInTransition(0);
        bool inIdle = state.IsName("Locomotion.Idle");
        bool inTurn = state.IsName("Locomotion.TurnOnSpot") || state.IsName("Locomotion.PlantNTurnLeft") || state.IsName("Locomotion.PlantNTurnRight");
        bool inWalkRun = state.IsName("Locomotion.Walk");

        float speedDampTime = inIdle || Mathf.Abs(speed) < 0.1f ? 0 : m_SpeedDampTime;
        float angularSpeedDampTime = inWalkRun || inTransition ? m_AnguarSpeedDampTime : 0;
        float directionDampTime = inTurn || inTransition ? 1000000 : 0;

        float angularSpeed = direction / m_DirectionResponseTime;
        
        m_Animator.SetFloat(m_SpeedId, speed, speedDampTime, Time.deltaTime);
        m_Animator.SetFloat(m_AgularSpeedId, angularSpeed, angularSpeedDampTime, Time.deltaTime);
        m_Animator.SetFloat(m_DirectionId, direction, directionDampTime, Time.deltaTime);
    }	
}
