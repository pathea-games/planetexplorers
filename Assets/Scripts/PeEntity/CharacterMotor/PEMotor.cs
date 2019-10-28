using UnityEngine;
using System.Collections;

[RequireComponent(typeof(PETracker))]
public abstract class PEMotor : MonoBehaviour
{
	public float maxForwardSpeed = 1.5f;
	public float maxBackwardsSpeed = 1.5f;
	public float maxSidewaysSpeed = 1.5f;
	public float maxVelocityChange = 0.2f;
	
	public float gravity = 10.0f;
	public bool canJump = true;
	public float jumpHeight = 1.0f;
	
	public Vector3 forwardVector = Vector3.forward;
	protected Quaternion alignCorrection;
	
	private bool m_Grounded = false;
	public bool grounded {
		get { return m_Grounded; }
		protected set { m_Grounded = value; }
	}
	
	private bool m_Jumping = false;
	public bool jumping	{
		get { return m_Jumping; }
		protected set { m_Jumping = value; }
	}

    private PETracker m_Tracker;
    public PETracker tracker {
        get { return m_Tracker; }
    }

    public virtual Vector3 velocity {
        get { return m_Tracker != null ? m_Tracker.velocity : Vector3.zero; }
    }

    public virtual Vector3 angleVelocity
    {
        get { return m_Tracker != null ? m_Tracker.angularVelocity : Vector3.zero; }
    }

    public virtual void Reset() { }
    public virtual void Stop() { }

    private Vector3 m_desiredMovementEffect;
	private Vector3 m_desiredMovementDirection;
	private Vector3 m_desiredFacingDirection;

	public void Start () {
        //maxVelocityChange = 5.0f;

		alignCorrection = new Quaternion();
		alignCorrection.SetLookRotation(forwardVector, Vector3.up);
		alignCorrection = Quaternion.Inverse(alignCorrection);

        m_Tracker = GetComponent<PETracker>();
	}

    public Vector3 desiredMovementEffect
    {
        get { return m_desiredMovementEffect; }
        set {
            m_desiredMovementEffect = value;
            if (m_desiredMovementEffect.magnitude>10) m_desiredMovementEffect = m_desiredMovementEffect.normalized*10;
        }
    }
	
	public Vector3 desiredMovementDirection {
		get { return m_desiredMovementDirection; }
		set {
			m_desiredMovementDirection = value;
			if (m_desiredMovementDirection.magnitude>1) m_desiredMovementDirection = m_desiredMovementDirection.normalized;
		}
	}
	public Vector3 desiredFacingDirection {
		get { return m_desiredFacingDirection; }
		set {
			m_desiredFacingDirection = value;
			if (m_desiredFacingDirection.magnitude>1) m_desiredFacingDirection = m_desiredFacingDirection.normalized;
		}
	}
	public Vector3 desiredVelocity {
		get {
			//return m_desiredVelocity;
			if (m_desiredMovementDirection==Vector3.zero) return Vector3.zero;
			else {
                //if (gravity > PETools.PEMath.Epsilon) m_desiredMovementDirection = Util.ProjectOntoPlane(m_desiredMovementDirection, transform.up);
				float zAxisEllipseMultiplier = maxSidewaysSpeed != 0 ? (m_desiredMovementDirection.z>0 ? maxForwardSpeed : maxBackwardsSpeed) / maxSidewaysSpeed : 0f;
				Vector3 temp = new Vector3(m_desiredMovementDirection.x, m_desiredMovementDirection.y, zAxisEllipseMultiplier !=0f ? m_desiredMovementDirection.z/zAxisEllipseMultiplier : 0f).normalized;
				float length = new Vector3(temp.x, temp.y, temp.z*zAxisEllipseMultiplier).magnitude * maxSidewaysSpeed;
				Vector3 velocity = m_desiredMovementDirection * length;
				return transform.rotation * velocity;
			}
		}
	}
}