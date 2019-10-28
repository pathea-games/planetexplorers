using UnityEngine;
using System.Collections;

public class PEBoneRotation : MonoBehaviour {

    public Transform target;
    public Vector3 axisForward;
    public Vector3 axisUp;
    public bool rotateAuto;
    public float rotateSpeed;

    Vector3 m_FaceDir;
	
	void LateUpdate ()
    {
        Vector3 up = transform.rotation * axisUp;
        //Vector3 forward = transform.rotation * axisForward;

        Vector3 desiredDirection = Vector3.ProjectOnPlane(m_FaceDir, up);
        if (rotateAuto)
            desiredDirection = Quaternion.AngleAxis(rotateSpeed*Time.deltaTime, up) * desiredDirection;
        {
            if (target != null)
                desiredDirection = Vector3.ProjectOnPlane(target.position - transform.position, up);
        }

        //Vector3 newForward = Vector3.Slerp(m_FaceDir, desiredDirection, rotateSpeed * Time.deltaTime);
        m_FaceDir = Util.ConstantSlerp(m_FaceDir, desiredDirection, rotateSpeed * Time.deltaTime);

        m_FaceDir = Vector3.ProjectOnPlane(m_FaceDir, up);
        transform.rotation = Quaternion.FromToRotation(transform.forward, m_FaceDir) * transform.rotation;
    }
}
