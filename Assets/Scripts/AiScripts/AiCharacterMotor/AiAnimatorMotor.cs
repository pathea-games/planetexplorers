using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class AiAnimatorMotor : AiCharacterMotor 
{
    public float timeScale = 1.0f;

    private float m_startStuckTime;
	private Animator animator;
	
	public override Vector3 velocity {
		get {
			if(animator != null)
				return animator.deltaPosition;
			else
				return Vector3.zero;
		}
	}
	
	public override float radius {
		get {
			return GetComponent<CharacterController>().radius;
		}
	}
	
	void Awake()
	{
		animator = GetComponent<Animator>();
	}
	
	void Update () 
	{
        UpdateMove();

        UpdateFacingDirection();

        UpdateVelocity();
	}
	
	private void UpdateMove()
	{
        Vector3 movement = Vector3.zero;

        if (desiredMoveDestination != Vector3.zero)
        {
            Debug.DrawLine(transform.position, desiredMoveDestination, Color.red);
        }

        if (desiredMoveDestination != Vector3.zero && seeker != null) 
            movement = seeker.movement;

        if (desiredMoveDestination != Vector3.zero && movement == Vector3.zero)
            movement = desiredMoveDestination - transform.position;

        if (desiredMoveDestination != Vector3.zero &&
            AiUtil.SqrMagnitudeH(desiredMoveDestination - transform.position) < 1f*1f)
            movement = Vector3.zero;

        DodgeNeighbours(ref movement);

        if (desiredLookAtTran != null)
            desiredFacingDirection = (desiredLookAtTran.position - transform.position).normalized;
        else if (desiredFaceDirection != Vector3.zero)
            desiredFacingDirection = desiredFaceDirection.normalized;
        else if (movement != Vector3.zero)
            desiredFacingDirection = movement.normalized;
        else
            desiredFacingDirection = Vector3.zero;

        //if (movement != Vector3.zero && CheckMovementValid(movement))
        //    desiredMovementDirection = Quaternion.Inverse(transform.rotation) * movement;
        //else
        //    desiredMovementDirection = Vector3.zero;

        if (movement != Vector3.zero && CheckMovementValid(movement))
        {
            if (habit == LifeArea.LA_Land && desiredFaceDirection == Vector3.zero && desiredLookAtTran == null)
            {
                float angle = Vector3.Angle(transform.forward, Util.ProjectOntoPlane(movement, transform.up));
                if (angle < 30)
                    desiredMovementDirection = Quaternion.Inverse(transform.rotation) * movement;
                else
                    desiredMovementDirection = Vector3.zero;
            }
            else
                desiredMovementDirection = Quaternion.Inverse(transform.rotation) * movement;
        }
        else
            desiredMovementDirection = Vector3.zero;
	}

    private void UpdateFacingDirection()
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

            if (Vector3.Dot(transform.forward, combinedFacingDirection.normalized) > 0)
                animator.SetFloat("Direction", Vector3.Cross(transform.forward, combinedFacingDirection.normalized).y, 0.0f, Time.deltaTime);
            else
                animator.SetFloat("Direction", Vector3.Cross(transform.forward, combinedFacingDirection.normalized).y > 0 ? 1 : -1, 0.25f, Time.deltaTime);
        }
        else
        {
            animator.SetFloat("Direction", 0.0f, 0.25f, Time.deltaTime);
        }
    }

    private void UpdateVelocity()
	{
        if (animator == null)
            return;

        animator.speed = timeScale;

        if (desiredVelocity == Vector3.zero)
        {
            animator.SetFloat("Speed", 0.0f, 0.0f, Time.deltaTime);
        }
        else
        {
            float speed = 1f;
            if ( maxRunSpeed - maxWalkSpeed > PETools.PEMath.Epsilon)
            {
                speed = Mathf.Clamp((Mathf.Clamp(maxForwardSpeed, maxWalkSpeed, maxRunSpeed) - maxWalkSpeed) / (maxRunSpeed - maxWalkSpeed), 0.15f, 1f);
            }
            else
            {
                Debug.LogWarning(name + " maxRunSpeed[" + maxRunSpeed + "] not big than maxWalkSpeed["+maxWalkSpeed+"].");
            }

            Vector3 moveDirection = Util.ProjectOntoPlane(desiredVelocity, transform.up);
            if (Vector3.Dot(transform.forward, moveDirection.normalized) > 0)
                animator.SetFloat("Speed", speed, 0.25f, Time.deltaTime);
            else
            {
                if(desiredLookAtTran == null)
                    animator.SetFloat("Speed", 0.0f, 0.25f, Time.deltaTime);
                else
                    animator.SetFloat("Speed", -1.0f, 0.25f, Time.deltaTime);
            }
        }
	}
}
