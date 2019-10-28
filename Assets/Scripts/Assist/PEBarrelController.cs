using UnityEngine;
using System.Collections;

public class PEBarrelController : MonoBehaviour 
{
    [System.Serializable]
    public class Chassis
    {
        public Transform chassis;
        public Vector3 pivot = Vector3.forward;
        public float angle;
    }

    [System.Serializable]
    public class Pitch
    {
        public Transform pitch;
        public Vector3 pivot = Vector3.forward;
        public Vector3 pivotAxis = Vector3.right;
        public float minAngle;
        public float maxAngle;
    }

    public Transform emitter;
    public Vector3 axis = Vector3.forward;
    public Chassis chassis;
    public Pitch pitch;

    bool m_AimChassis;
    bool m_AimPitch;

    int m_Layer;

    float m_AimStartTime; 
    Transform m_AimTarget;

    Bounds m_LocalBounds;

    public Transform AimTarget
    {
        get { return m_AimTarget; }
        set
        {
            if (m_AimTarget != value)
            {
                m_AimTarget = value;
                m_AimStartTime = Time.time;
            }
        }
    }

    Vector3 m_AimPosition;

    public Vector3 AimPosition
    {
        get { return m_AimPosition; }
        set { m_AimPosition = value; }
    }

    public bool Aim { set { enabled = value; } }

	public float ChassisY { get { return chassis.chassis.rotation.eulerAngles.y; } }
	public Vector3 PitchEuler { get { return pitch.pitch.rotation.eulerAngles; } }

	public void ApplyChassis(float rotY)
	{
		if (null != chassis && null != chassis.chassis)
		{
			Vector3 curEuler = chassis.chassis.rotation.eulerAngles;
			curEuler.y = rotY;
			chassis.chassis.rotation = Quaternion.Euler(curEuler);
		}
	}

	public void ApplyPitchEuler(Vector3 angleEuler)
	{
		if (null != pitch && null != pitch.pitch)
			pitch.pitch.rotation = Quaternion.Euler(angleEuler);
	}

    public bool IsAimed {
        get { return m_AimChassis && m_AimPitch; }
    }

    public bool Angle(Vector3 position, float angle)
    {
        if (chassis.chassis != null && position != Vector3.zero)
        {
            Vector3 forward = chassis.chassis.TransformDirection(chassis.pivot);
            forward.y = 0.0f;

            Vector3 direction = position - chassis.chassis.position;
            direction.y = 0.0f;

            return Vector3.Angle(forward, direction) <= angle;
        }

        return false;
    }

    public bool PitchAngle(Vector3 position, float angle)
    {
        if (pitch.pitch != null && position != Vector3.zero)
        {
            Vector3 forward = pitch.pitch.TransformDirection(pitch.pivot);
            forward = new Vector3(0.0f, forward.y, 0.0f);

            Vector3 direction = position - pitch.pitch.position;
            direction = new Vector3(0.0f, direction.y, 0.0f);

            if (direction.sqrMagnitude < 0.05f * 0.05f)
                return true;
            else
                return Vector3.Angle(forward, direction) <= angle;
        }

        return false;
    }

    public bool EstimatedAttack(Vector3 position, Transform target = null)
    {
        Vector3 center = transform.TransformPoint(m_LocalBounds.center);
        float distance = Vector3.Distance(center, position);
        RaycastHit hitInfo;
        return !Physics.Raycast(center, position - center, out hitInfo, distance, m_Layer) || (target == null || hitInfo.transform.IsChildOf(target));
    }

    public bool CanAttack(Vector3 position, Transform target = null)
    {
        if (emitter == null) return true;

        float distance = Vector3.Distance(emitter.position, position);
        Vector3 direction = emitter.TransformDirection(axis);

        RaycastHit hitInfo;
		if(!Physics.Raycast(emitter.position, direction, out hitInfo, distance, m_Layer))
			return true;

		if(target == null || hitInfo.transform.IsChildOf(target))
			return true;

		if(hitInfo.collider.gameObject.layer == Pathea.Layer.Water)
			return true;

        return false;
    }

    public bool Evaluate(Vector3 position)
    {
        Quaternion r1 = Quaternion.identity;
        Quaternion r2 = Quaternion.identity;

        if(chassis.chassis != null)
        {
            Vector3 direction = position - chassis.chassis.position;
            Transform _chassis = chassis.chassis;

            Vector3 _direction = _chassis.InverseTransformDirection(direction);
            Vector3 _forward = _chassis.InverseTransformDirection(_chassis.TransformDirection(chassis.pivot));

            _forward = Vector3.ProjectOnPlane(_forward, Vector3.up);
            _direction = Vector3.ProjectOnPlane(_direction, Vector3.up);

            r1 = Quaternion.FromToRotation(_forward, _direction);

            _chassis.rotation = r1 * _chassis.rotation;
        }

        if(pitch.pitch != null)
        {
            Transform _pitch = pitch.pitch;

            Vector3 direction = position - pitch.pitch.position;

            Vector3 _right = _pitch.TransformDirection(pitch.pivotAxis);
            Vector3 _forward = _pitch.TransformDirection(pitch.pivot);
            Vector3 _direction = Vector3.ProjectOnPlane(direction, _right);

            r2 = Quaternion.FromToRotation(_forward, _direction);

            _pitch.rotation = r2 * _pitch.rotation;
        }

        Vector3 pos = emitter.position;
        Vector3 dir = emitter.TransformDirection(axis);
        float distance = Vector3.Distance(pos, position);

        if(chassis.chassis != null)
            chassis.chassis.rotation = Quaternion.Inverse(r1) * chassis.chassis.rotation;

        if(pitch.pitch != null)
            pitch.pitch.rotation = Quaternion.Inverse(r2) * pitch.pitch.rotation;

        return !Physics.Raycast(emitter.position, dir, distance, m_Layer);
    }

    bool ChassisRotation()
    {
        if (chassis.chassis == null)
            return true;

        if (m_AimTarget == null && m_AimPosition == Vector3.zero)
            return false;

        Vector3 aimPosition = m_AimTarget != null ? m_AimTarget.position : m_AimPosition;
        if (PETools.PEUtil.SqrMagnitude(aimPosition, chassis.chassis.position, false) < 0.5f * 0.5f)
            return false;

        Vector3 direction = aimPosition - chassis.chassis.position;
        Transform _chassis = chassis.chassis;
        Vector3 _direction = _chassis.InverseTransformDirection(direction);
        Vector3 _forward = _chassis.InverseTransformDirection(_chassis.TransformDirection(chassis.pivot));

        _direction = Vector3.ProjectOnPlane(_direction, Vector3.up);
        _forward = Vector3.ProjectOnPlane(_forward, Vector3.up);

        float rotateSpeed = Mathf.Lerp(90.0f, 270.0f, (Time.time - m_AimStartTime) * 0.5f);
        //Vector3 _newForward = Vector3.Slerp(_forward, _direction, rotateSpeed * Time.deltaTime);
        Vector3 _newForward = Util.ConstantSlerp(_forward, _direction, rotateSpeed * Time.deltaTime);
        _chassis.rotation = Quaternion.FromToRotation(_forward, _newForward) * _chassis.rotation;

        return Vector3.Angle(_forward, _direction) < 5.0f;
    }

    bool PitchRotation()
    {
		if (pitch.pitch == null)
            return true;

        if (m_AimTarget == null && m_AimPosition == Vector3.zero)
            return false;

        Vector3 aimPosition = m_AimTarget != null ? m_AimTarget.position : m_AimPosition;
        if (PETools.PEUtil.SqrMagnitude(aimPosition, pitch.pitch.position, false) < 0.5f * 0.5f)
            return false;

        Vector3 direction = aimPosition - pitch.pitch.position;

        Transform _pitch = pitch.pitch;
        Vector3 _right = _pitch.TransformDirection(pitch.pivotAxis);
        Vector3 _forward = _pitch.TransformDirection(pitch.pivot);
        Vector3 _direction = Vector3.ProjectOnPlane(direction, _right);

        Vector3 _newForward = Vector3.Slerp(_forward, _direction, 270.0f * Time.deltaTime);
        _pitch.rotation = Quaternion.FromToRotation(_forward, _newForward) * _pitch.rotation;

        return Vector3.Angle(_forward, _direction) < 5.0f;
    }

    void Awake()
    {
        m_Layer = 1 << Pathea.Layer.VFVoxelTerrain
                    | 1 << Pathea.Layer.Building
                    | 1 << Pathea.Layer.SceneStatic
                    | 1 << Pathea.Layer.Unwalkable
                    | 1 << Pathea.Layer.NearTreePhysics;

        m_LocalBounds = PETools.PEUtil.GetLocalColliderBoundsInChildren(gameObject);
    }

    void LateUpdate()
    {
        m_AimChassis = ChassisRotation();

        if (!m_AimChassis)
            m_AimPitch = false;
        else
            m_AimPitch = PitchRotation();
    }
}
