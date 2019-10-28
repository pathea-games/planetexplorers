using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class AiNormalCharacterMotor : AiCharacterMotor
{
    private bool firstframe = true;
    private CharacterController controller;

    public override Vector3 velocity
    {
        get
        {
            if (controller != null)
                return controller.velocity;
            else
                return Vector3.zero;
        }
    }

    public override float radius
    {
        get
        {
            if (controller == null) return 0.0f;

            return controller.radius;
        }
    }

    public override float height
    {
        get
        {
            if (controller == null) return 0.0f;

            return controller.height;
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        firstframe = true;
        controller = GetComponent(typeof(CharacterController)) as CharacterController;
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
                    + Vector3.up
                    + Quaternion.AngleAxis(360 * i / 8.0f, Vector3.up)
                        * (Vector3.right * 0.5f)
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

        if (combinedFacingDirection.sqrMagnitude > 0.01f)
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

    private void UpdateVelocity()
    {
        if (controller == null || !controller.enabled)
            return;

        Vector3 velocity = controller.velocity;
        if (firstframe)
        {
            velocity = Vector3.zero;
            firstframe = false;
        }
        if (grounded) velocity = Util.ProjectOntoPlane(velocity, Vector3.up);

        // Calculate how fast we should be moving
        Vector3 movement = velocity;
        //bool hasJumped = false;
        jumping = false;
        if (grounded || gravity < PETools.PEMath.Epsilon)
        {
            // Apply a force that attempts to reach our target velocity
            Vector3 velocityChange = (desiredVelocity - velocity);
            if (velocityChange.magnitude > maxVelocityChange)
            {
                velocityChange = velocityChange.normalized * maxVelocityChange;
            }
            movement += velocityChange;

            if (desiredVelocity == Vector3.zero) movement = Vector3.zero;

            // Jump
            //if (canJump && false)
            //{
            //    //movement += transform.up * Mathf.Sqrt(2 * jumpHeight * gravity);
            //    movement += Vector3.up * Mathf.Sqrt(2 * jumpHeight * gravity);
            //    //hasJumped = true;
            //    jumping = true;
            //}
        }

        float maxVerticalVelocity = 1.0f;
        AlignmentTracker at = GetComponent<AlignmentTracker>();
        if (at != null && Mathf.Abs(at.velocitySmoothed.y) > maxVerticalVelocity)
        {
            movement *= Mathf.Max(0.0f, Mathf.Abs(maxVerticalVelocity / at.velocitySmoothed.y));
        }

        Vector3 moveDeta = movement * Time.deltaTime;
        if (!CheckMoveDeta(moveDeta))
        {
            movement = Vector3.zero;
        }

        // Apply downwards gravity
        //movement += transform.up * -gravity * Time.deltaTime;
        movement += Vector3.up * -gravity * Time.deltaTime;

        if (jumping)
        {
            //movement -= transform.up * -gravity * Time.deltaTime / 2;
            movement -= Vector3.up * -gravity * Time.deltaTime / 2;
        }

        // Apply movement
        CollisionFlags flags = controller.Move(movement * Time.deltaTime);
        grounded = (flags & CollisionFlags.CollidedBelow) != 0;
    }

    protected virtual bool CheckMoveDeta(Vector3 moveDeta)
    {
        return true;
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

    // Update is called once per frame
    void Update()
    {
        if (useCentricGravity) AdjustToGravity();

        UpdateMove();

        UpdateFacingDirection();

        UpdateVelocity();
    }
}
