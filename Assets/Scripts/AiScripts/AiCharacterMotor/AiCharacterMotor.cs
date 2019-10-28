using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AiAsset;

public enum LifeArea
{
    LA_None,
    LA_Land,
    LA_Water,
    LA_Sky,
    LA_Max
}

public abstract class AiCharacterMotor : MonoBehaviour
{
    [System.NonSerialized]
    public float maxWalkSpeed = 1.5f;
    [System.NonSerialized]
    public float maxRunSpeed = 2f;

    //[System.NonSerialized]
    public float maxForwardSpeed = 1.5f;
    [System.NonSerialized]
    public float maxBackwardsSpeed = 1.5f;
    [System.NonSerialized]
    public float maxSidewaysSpeed = 1.5f;
    [System.NonSerialized]
    public float maxVelocityChange = 100.0f;
    [System.NonSerialized]
    public float maxRotationSpeed = 270;
    [HideInInspector]
    public LayerMask groundLayers;

    public bool showGizmos = false;
    public LifeArea habit;
    public float gravity = 10.0f;
    public bool canMove = true;
    public bool canJump = true;
    public float jumpHeight = 1.0f;
    public bool useCentricGravity = false;

    protected Vector3 forwardVector = Vector3.forward;
    protected Quaternion alignCorrection;

    protected AiSeeker seeker;

	AiObject aiObject;

    float mTurnAngle = 0;

    private Vector3 m_destination;
    public Vector3 desiredMoveDestination
    {
        get { return m_destination; }
        set
        {
            //if (seeker != null)
            //{
            //    if (value == Vector3.zero)
            //        seeker.ClearPath();
            //    else if (AiUtil.SqrMagnitude(m_destination - value) >= 1.0f * 1.0f)
            //    {
            //        if (AstarPath.active != null
            //            && AstarPath.active.gameObject.activeSelf
            //            && gravity > PETools.PEMath.Epsilon)
            //        {
            //            seeker.StartPath(value);
            //        }
            //    }
            //}

            m_destination = value;
        }
    }

    private Vector3 m_faceDirection = Vector3.zero;
    public Vector3 desiredFaceDirection
    {
        get { return m_faceDirection; }
        set
        {
            m_faceDirection = value;
            if (m_faceDirection.magnitude > 1) m_faceDirection = m_faceDirection.normalized;
        }
    }

    private Transform m_LooAtTran;
    public Transform desiredLookAtTran
    {
        get { return m_LooAtTran; }
        set { m_LooAtTran = value; }
    }

    private bool m_jump = false;
    public bool jump { get { return m_jump; } set { m_jump = value; } }

    public virtual Vector3 velocity { get { return Vector3.zero; } }
    public virtual float radius { get { return 0.0f; } }
    public virtual float height { get { return 0.0f; } }

    bool m_stuck = false;
    public bool stucking { get { return m_stuck; } }

    private bool m_Grounded = false;
    public bool grounded
    {
        get { return m_Grounded; }
        protected set { m_Grounded = value; }
    }

    private bool m_Jumping = false;
    public bool jumping
    {
        get { return m_Jumping; }
        protected set { m_Jumping = value; }
    }

    private Vector3 m_desiredMovementDirection;
    private Vector3 m_desiredFacingDirection;

    private Vector3 m_lastPosition;
    private Quaternion m_lastRotation;

    void Clear()
    {
        desiredLookAtTran = null;
        desiredMoveDestination = Vector3.zero;
        desiredMovementDirection = Vector3.zero;
    }

    public void FollowPath(Vector3[] paths)
    {
        if(seeker != null)
		{
			seeker.SetFollowPath(paths);
		}
    }

    public void ClearPath()
    {
        if(seeker != null)
		{
			seeker.ClearFollowPath();
		}
    }

    IEnumerator Stuck()
    {
        float time = 0.0f;

        while (true)
        {
            float sqrDis = (transform.position - m_lastPosition).sqrMagnitude;
            float ang = Quaternion.Angle(m_lastRotation, transform.rotation);
            if (desiredVelocity.sqrMagnitude < 0.15 * 0.15f
                || sqrDis > 0.15f * 0.15f 
                || ang > 1f)
            {
                m_stuck = false;
                time = Time.time;
            }
            else
            {
                if (Time.time - time > 5.0f)
                {
                    m_stuck = true;
                    Debug.LogWarning((aiObject != null ? aiObject.name : "") + " --> stucking!!");
                }
            }

            m_lastPosition = transform.position;
            m_lastRotation = transform.rotation;
            yield return new WaitForSeconds(0.5f);
        }
    }

    IEnumerator PathFinding()
    {
        while (true)
        {
            //if (AstarPath.active != null 
            //    && AstarPath.active.gameObject.activeSelf 
            //    && seeker != null 
            //    && gravity > PETools.PEMath.Epsilon
            //    && m_stuck)
            //{
            //    if (m_destination == Vector3.zero)
            //        seeker.ClearPath();
            //    else
            //        seeker.StartPath(m_destination);
            //}
            yield return new WaitForSeconds(1.0f);
        }
    }

    protected void DodgeNeighbours(ref Vector3 movement)
    {
        if (aiObject == null || movement == Vector3.zero)
            return;

        //float aiObjRadius = aiObject.radius + 0.5f;

        //RaycastHit hitInfo;
        //if (Physics.SphereCast(aiObject.center, aiObjRadius, movement, out hitInfo,
        //    aiObjRadius + 2.0f, AiManager.Manager.neighbourLayer))
        //{
        //    if(hitInfo.transform.IsChildOf(aiObject.transform))
        //    {
        //        mTurnAngle = 0.0f;
        //        return;
        //    }

        //    if (aiObject.enemy != null
        //        && aiObject.enemy.transform != null
        //        && hitInfo.transform.IsChildOf(aiObject.enemy.transform))
        //    {
        //        mTurnAngle = 0.0f;
        //        return;
        //    }

        //    if (Mathf.Abs(mTurnAngle) < 0.1f)
        //    {
        //        Vector3 cross = Vector3.Cross(movement.normalized, (hitInfo.point - transform.position).normalized);

        //        if (cross.y > PETools.PEMath.Epsilon)
        //            mTurnAngle = -60.0f;
        //        else
        //            mTurnAngle = 60.0f;
        //    }
        //}
        //else
        //    mTurnAngle = 0.0f;

        movement = Quaternion.AngleAxis(mTurnAngle, Vector3.up) * movement;
    }

    bool CheckMovementValidLand(Vector3 movement)
    {
        //DodgeNeighbours(ref movement);

        float deltaTime = (this is AiPhysicsCharacterMotor) ? Time.fixedDeltaTime : Time.deltaTime;

        Vector3 newMovement = movement;
        newMovement.y = 0.0f;

        Vector3 deltaMovement = newMovement.normalized * maxForwardSpeed * deltaTime * 10.0f;

        //Vector3 nextFeetPos = transform.position + transform.forward * radius + deltaMovement;
        //if (!AiUtil.CheckPositionOnGround(nextFeetPos, 5.0f, AiManager.Manager.groundedLayer))
        //    return false;

        Vector3 nextPos = aiObject != null ? aiObject.center : transform.position;
        nextPos += transform.forward * radius;
        nextPos += deltaMovement;

        if (AiUtil.CheckPositionUnderWater(nextPos))
            return false;

        return true;
    }

    bool CheckMovemenValidtWater(Vector3 movement)
    {
		//DodgeNeighbours(ref movement);

        //float deltaTime = (this is AiPhysicsCharacterMotor) ? Time.fixedDeltaTime : Time.deltaTime;
        //AiWaterMonster waterMonster = aiObject as AiWaterMonster;
        //float broach = waterMonster != null ? waterMonster.broachHeight : 1.0f;

        //Vector3 forwardPos = transform.position + movement.normalized*maxForwardSpeed*deltaTime * 5 + transform.up*height;
        //if (!Physics.Raycast(forwardPos, Vector3.down, 256.0f, AiManager.Manager.groundedLayer))
        //    return false;

        //Vector3 nextPos = transform.position 
        //        + transform.forward * radius
        //        + transform.up * (height * broach)
        //        + movement.normalized
        //        * maxForwardSpeed * deltaTime * 5;

        //if (movement.y < -PETools.PEMath.Epsilon || AiUtil.CheckPositionUnderWater(nextPos))
        //    return true;
        //else
            return false;
    }

    bool CheckMovementValidSky(Vector3 movement)
    {
        //float deltaTime = (this is AiPhysicsCharacterMotor) ? Time.fixedDeltaTime : Time.deltaTime;

        //Vector3 forwardPos = transform.position + movement.normalized * maxForwardSpeed * deltaTime * 5 + transform.up * height;
        //if (!Physics.Raycast(forwardPos, Vector3.down, 256.0f, AiManager.Manager.groundedLayer))
        //    return false;

        return true;
    }

    public virtual bool CheckMovementValid(Vector3 movement)
    {
        switch (habit)
        {
            case LifeArea.LA_None: 
                return true;
            case LifeArea.LA_Land: 
                return CheckMovementValidLand(movement);
            case LifeArea.LA_Water: 
                return CheckMovemenValidtWater(movement);
            case LifeArea.LA_Sky: 
                return CheckMovementValidSky(movement);
            case LifeArea.LA_Max: 
                return true;
            default: return true;
        }
    }

    protected virtual void OnEnable()
    {
        StopAllCoroutines();

        StartCoroutine(Stuck());
        StartCoroutine(PathFinding());
    }

    protected virtual void Start()
    {
        seeker = GetComponentInChildren<AiSeeker>();
		aiObject = GetComponent<AiObject>();

        alignCorrection = new Quaternion();
        alignCorrection.SetLookRotation(forwardVector, Vector3.up);
        alignCorrection = Quaternion.Inverse(alignCorrection);

        //groundLayers = AiManager.Manager.groundedLayer;

        if (habit == LifeArea.LA_Water || habit == LifeArea.LA_Sky)
        {
            gravity = 0.0f;
        }
    }

    public Vector3 desiredMovementDirection
    {
        get { return m_desiredMovementDirection; }
        set
        {
            m_desiredMovementDirection = value;
            if (m_desiredMovementDirection.magnitude > 1) m_desiredMovementDirection = m_desiredMovementDirection.normalized;
        }
    }
    public Vector3 desiredFacingDirection
    {
        get { 
			if(aiObject != null && !aiObject.CanRotate())
				return Vector3.zero;
			else
				return m_desiredFacingDirection; 
		}
        set
        {
            m_desiredFacingDirection = value;
            if (m_desiredFacingDirection.magnitude > 1) m_desiredFacingDirection = m_desiredFacingDirection.normalized;
        }
    }
    public Vector3 desiredVelocity
    {
        get
        {
            //return m_desiredVelocity;
            if (m_desiredMovementDirection == Vector3.zero || !canMove || (aiObject != null && !aiObject.CanMove())) return Vector3.zero;
            else
            {
				float zAxisEllipseMultiplier = (m_desiredMovementDirection.z > 0 ? maxForwardSpeed : maxBackwardsSpeed) / maxSidewaysSpeed;
                //Vector3 temp = new Vector3(m_desiredMovementDirection.x, 0, m_desiredMovementDirection.z / zAxisEllipseMultiplier).normalized;
                //float length = new Vector3(temp.x, 0, temp.z * zAxisEllipseMultiplier).magnitude * maxSidewaysSpeed;
                //Vector3 velocity = m_desiredMovementDirection * length;
                float vy = gravity > 0.0f ? 0.0f : m_desiredMovementDirection.y;
                Vector3 temp = new Vector3(m_desiredMovementDirection.x, vy, m_desiredMovementDirection.z / zAxisEllipseMultiplier).normalized;
                float length = new Vector3(temp.x, temp.y, temp.z * zAxisEllipseMultiplier).magnitude * maxSidewaysSpeed;
                Vector3 velocity = m_desiredMovementDirection;
                if (gravity > PETools.PEMath.Epsilon) velocity.y = 0.0f;
                velocity = velocity.normalized * length;

                return transform.rotation * velocity;
            }
        }
    }
}