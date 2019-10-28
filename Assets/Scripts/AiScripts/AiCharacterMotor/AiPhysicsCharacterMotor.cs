using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
//[RequireComponent(typeof(CapsuleCollider))]
public class AiPhysicsCharacterMotor : AiCharacterMotor
{
    public Vector3 gravityCenter = Vector3.zero;

    public override Vector3 velocity
    {
        get
        {
            if (GetComponent<Rigidbody>() != null)
                return GetComponent<Rigidbody>().velocity;
            else
                return base.velocity;
        }
    }

    public override float radius
    {
        get
        {
            if (GetComponent<Rigidbody>().GetComponent<Collider>() != null)
            {
                Bounds bound = GetComponent<Rigidbody>().GetComponent<Collider>().bounds;
                return Mathf.Max(bound.extents.x, bound.extents.z);
            }
            else
            {
                return base.radius;
            }
        }
    }

    public override float height
    {
        get
        {
            if (GetComponent<Rigidbody>().GetComponent<Collider>() != null)
            {
                Bounds bound = GetComponent<Rigidbody>().GetComponent<Collider>().bounds;
                return bound.size.y;
            }
            else
            {
                return base.radius;
            }
        }
    }

    void Awake()
    {
        GetComponent<Rigidbody>().freezeRotation = true;
        GetComponent<Rigidbody>().useGravity = false;
    }

    private void AdjustToGravity()
    {
        int origLayer = gameObject.layer;
        gameObject.layer = 2;

        Vector3 currentUp = transform.up;
        //Vector3 gravityUp = (transform.position-gravityCenter).normalized;

        float damping = Mathf.Clamp01(Time.deltaTime * 5);

        RaycastHit hit;

        Vector3 desiredUp = Vector3.zero;
        for (int i = 0; i < 8; i++)
        {
            Vector3 rayStart =
                transform.position
                    + transform.up
                    + Quaternion.AngleAxis(360 * i / 8.0f, transform.up)
                        * (transform.right * 0.5f)
                    + desiredVelocity * 0.2f;
            if (Physics.Raycast(rayStart, transform.up * -2, out hit, 10.0f, groundLayers.value))
            {
                if (Mathf.Abs(hit.point.y - transform.position.y) > 1)
                    continue;

                if (Vector3.Angle(hit.normal, Vector3.up) > 45)
                    continue;

                desiredUp += hit.normal;
            }
        }
        desiredUp = (currentUp + desiredUp).normalized;
        Vector3 newUp = (currentUp + desiredUp * damping).normalized;

        float angle = Vector3.Angle(currentUp, newUp);
        if (angle > 0.01)
        {
            Vector3 axis = Vector3.Cross(currentUp, newUp).normalized;
            Quaternion rot = Quaternion.AngleAxis(angle, axis);
            transform.rotation = rot * transform.rotation;
        }

        gameObject.layer = origLayer;
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

        if (combinedFacingDirection.sqrMagnitude > 0.1f)
        {
            Vector3 newForward = Util.ConstantSlerp(
                transform.forward,
                combinedFacingDirection,
                maxRotationSpeed * Time.deltaTime
            );
            newForward = Util.ProjectOntoPlane(newForward, transform.up);
            //Debug.DrawLine(transform.position, transform.position+newForward, Color.yellow);
            Quaternion q = new Quaternion();
            q.SetLookRotation(newForward, transform.up);
            transform.rotation = q;
        }
    }

    protected virtual void UpdateVelocity()
    {
        Vector3 velocity = GetComponent<Rigidbody>().velocity;
        if (grounded) velocity = Util.ProjectOntoPlane(velocity, transform.up);

        // Calculate how fast we should be moving
        jumping = false;
        if (grounded || gravity < PETools.PEMath.Epsilon)
        {
            // Apply a force that attempts to reach our target velocity
            Vector3 velocityChange = (desiredVelocity - velocity);
            if (velocityChange.magnitude > maxVelocityChange)
            {
                velocityChange = velocityChange.normalized * maxVelocityChange;
            }
            GetComponent<Rigidbody>().AddForce(velocityChange, ForceMode.VelocityChange);

            // Jump
            //if (false && canJump && Input.GetButton("Jump"))
            //{
            //    rigidbody.velocity = velocity + transform.up * Mathf.Sqrt(2 * jumpHeight * gravity);
            //    jumping = true;
            //}
        }

        // Apply downwards gravity
        GetComponent<Rigidbody>().AddForce(transform.up * -gravity * GetComponent<Rigidbody>().mass);

        grounded = false;
    }

    private void UpdateMove()
    {
        Vector3 movement = Vector3.zero;

        if (desiredMoveDestination != Vector3.zero && showGizmos)
        {
            Debug.DrawLine(transform.position, desiredMoveDestination, Color.red);
        }

        if (seeker != null && (seeker.followPathing || desiredMoveDestination != Vector3.zero))
            movement = seeker.movement;

        if (desiredMoveDestination != Vector3.zero && movement == Vector3.zero)
            movement = desiredMoveDestination - transform.position;

        DodgeNeighbours(ref movement);

        if (desiredLookAtTran != null)
            desiredFacingDirection = (desiredLookAtTran.position - transform.position).normalized;
        else if (desiredFaceDirection != Vector3.zero)
            desiredFacingDirection = desiredFaceDirection.normalized;
        else if (movement != Vector3.zero)
            desiredFacingDirection = movement.normalized;
        else
            desiredFacingDirection = Vector3.zero;

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

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == Pathea.Layer.AIPlayer)
            GetComponent<Rigidbody>().constraints |= RigidbodyConstraints.FreezePositionY;
    }

    void OnCollisionStay()
    {
        grounded = true;
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.layer == Pathea.Layer.AIPlayer)
            GetComponent<Rigidbody>().constraints &= ~RigidbodyConstraints.FreezePositionY;
    }

    void Update()
    {
        if (useCentricGravity) AdjustToGravity();

        UpdateMove();

        UpdateFacingDirection();
    }

    void FixedUpdate()
    {
        //if (useCentricGravity) AdjustToGravity();

        //if (isUpdate) UpdateMove();

        //UpdateFacingDirection();

        UpdateVelocity();
    }

}
