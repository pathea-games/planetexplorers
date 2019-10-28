using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SkillAsset;
using AiAsset;

public partial class AiObject : SkillRunner
{
    protected Vector3 m_spawnPosition;
    protected Vector3 m_offset;
    protected float m_speed;
    protected float m_turnSpeed;
    protected bool m_canMove;
    protected bool m_canRotate;
    protected bool m_canAttack;
    protected bool m_isStuck;
    protected Bounds m_bound;

    protected AiCharacterMotor m_motor;

    #region public variable
    public Bounds bound
    {
        get
        {
//            ColonyRunner cr = gameObject.GetComponent<ColonyRunner>();
//			if (cr != null)
//			{
//				CSEntityObject ceo = gameObject.GetComponent<CSEntityObject>();
//				if (ceo != null)
//					return AiUtil.TransfromOBB2AABB(transform, ceo.GetObjectBounds());
//				else
//					return AiUtil.TransfromOBB2AABB(transform, new Bounds(Vector3.zero, Vector3.one * 0.5f));
//			}
//			else
//			{
            	return GetComponent<Collider>() != null ? GetComponent<Collider>().bounds : new Bounds();
//			}
        }
    }
	
    public AiCharacterMotor motor { get { return m_motor; } }

    public Vector3 position { get { return transform.position; } }

    public Vector3 forward { get { return transform.forward * radius; } }

    public Vector3 up { get { return transform.up * bound.extents.y * 2; } }

    public Vector3 spawnPosition { get { return m_spawnPosition; } }

    public Vector3 forwardPoint { get { return transform.position + transform.forward * radius; } }

    public float width { get { return bound.extents.x; } }

    public virtual Vector3 center 
    { 
        get 
        {
            if (mCenter != null && mCenter.GetComponent<Rigidbody>() != null)
                return mCenter.GetComponent<Rigidbody>().worldCenterOfMass;
            else
                return transform.position + transform.up * bound.extents.y; 
        } 
    }

    public float height 
	{ 
		get 
		{ 
			if(GetComponent<Collider>() != null)
				return AiUtil.GetColliderHeight(GetComponent<Collider>());
			else
				return bound.extents.y * 2; 
		} 
	}

    public float radius 
	{ 
		get 
		{ 
			if(GetComponent<Collider>() != null)
				return AiUtil.GetColliderRadius(GetComponent<Collider>());
			else
				return Mathf.Max(bound.extents.x, bound.extents.z); 
		} 
	}

    public Vector3 extents 
    {
        get
        {
            if (GetComponent<Collider>() != null)
            {
                if (GetComponent<Collider>() is CapsuleCollider)
                {
                    CapsuleCollider capsule = GetComponent<Collider>() as CapsuleCollider;
                    if (capsule.direction == 2)
                    {
                        return new Vector3(capsule.radius, capsule.radius, capsule.height * 0.5f);
                    }
                    else if (capsule.direction == 1)
                    {
                        return new Vector3(capsule.radius, capsule.height * 0.5f, capsule.radius);
                    }
                }
                else if (GetComponent<Collider>() is CharacterController)
                {
                    CharacterController controller = GetComponent<Collider>() as CharacterController;
                    return new Vector3(controller.radius, controller.height * 0.5f, controller.radius);
                }
            }
            else
            {
				var crBound = VCUtils.GetComponentOrOnParent<WhiteCat.CreationController>(gameObject);

                if (crBound != null)
                	return AiUtil.TransfromOBB2AABB(transform, crBound.bounds).extents;
            }

            return Vector3.zero;
        }
    }

    public virtual float turnSpeed
    {
        get { return m_turnSpeed; }
        set
        {

            if (m_turnSpeed != value && m_motor != null)
                m_motor.maxRotationSpeed = value;

            m_turnSpeed = value;
        }
    }

    public float speed
    {
        get 
        {
            return m_speed;
        }
        set
        {
            if (value < 0.1)
            {
                //Debug.LogWarning("speed have set to 0.1");

                value = 0.1f;
            }

            if (m_motor != null)
            {
                m_motor.maxForwardSpeed = value;
                m_motor.maxSidewaysSpeed = value;
                m_motor.maxBackwardsSpeed = value;
            }

            m_speed = value;
        }
    }

    public bool canAttack
    {
        get { return m_canAttack; }
        set { m_canAttack = value; }
    }

    public bool canMove
    {
        get { return m_canMove; }
		set { m_canMove = value; }
    }

    public bool canRotate
    {
        get { return m_canRotate; }
		set { m_canRotate = value; }
    }

    #endregion

    public void StopMove()
    {
        desiredMoveDestination = Vector3.zero;
        desiredMovementDirection = Vector3.zero;
    }

    public void StopRotation()
    {
        desiredLookAtTransform = null;
        desiredFaceDirection = Vector3.zero;
    }

    public void StopMoveAndRotation()
    {
        if (!GameConfig.IsMultiMode || IsController)
        {
            StopMove();
            StopRotation();
        }
    }

    public bool IsMoving
    {
        get
        {
            if (motor.desiredMoveDestination == Vector3.zero)
            {
                return true;
            }

            return false;
        }
    }

    //new system
    public virtual Vector3 desiredMoveDestination
    {
        set
        {
            if (motor == null)
                return;

            if (!GameConfig.IsMultiMode || IsController)
            {
                if (CanMove())
                    motor.desiredMoveDestination = value;
                else
                    motor.desiredMoveDestination = Vector3.zero;
            }
        }
        get
        {
            return motor.desiredMoveDestination;
        }
    }

    public virtual Vector3 desiredMovementDirection
    {
        set
        {
            if (motor == null)
                return;

            if (!GameConfig.IsMultiMode || IsController)
            {
                if (CanMove())
                {
                    motor.desiredMoveDestination = Vector3.zero;
                    motor.desiredMovementDirection = value;
                }
                else
                    motor.desiredMovementDirection = Vector3.zero;
            }
        }
        get
        {
            return motor.desiredMovementDirection;
        }
    }

    public virtual Vector3 desiredFaceDirection
    {
        set
        {
            if (motor == null)
                return;

            if (!GameConfig.IsMultiMode || IsController)
            {
                if (CanRotate())
                {
                    motor.desiredFaceDirection = value;
                }
                else
                {
                    motor.desiredFaceDirection = Vector3.zero;
                }
            }
        }
    }

    public Transform desiredLookAtTransform
    {
        set
        {
            if (motor == null)
                return;

            motor.desiredLookAtTran = value;
        }
		get
		{
			return motor.desiredLookAtTran;
		}
    }

    public Vector3 offset
    {
        get
        {
            return m_offset;
        }
    }

    protected void InitializeControllerData()
    {
        speed = walkSpeed;
        m_canAttack = true;
        m_canMove = true;
        m_canRotate = true;
    }

    public virtual bool CanRotate()
    {
        return m_canRotate && !m_isDead && !OnBuff(Buff_Sp.STUNNED);
    }

    public virtual bool CanMove()
    {
        return m_canMove && !m_isDead && !OnBuff(Buff_Sp.MOVE_NOT) && !OnBuff(Buff_Sp.STUNNED);
    }

    public virtual bool CanAttack()
    {
        return m_canAttack && !OnBuff(SkillAsset.Buff_Sp.ATTACK_NOT);
    }

    public virtual bool CanAiWorking()
    {
        return !m_isDead && !OnBuff(Buff_Sp.STUNNED);
    }
}
