using UnityEngine;
using System.Collections;

public class IKCombatLimit : MonoBehaviour 
{
    static int count = 100;

    public Transform rootBone;
    public Vector3 axis = Vector3.forward;
    [Range(0.0f, 180.0f)]public float limit;
    public float distance = 1f;

    Vector3 m_Pivot = Vector3.zero;

    public Vector3 pivot
    {
        get
        {
            return rootBone.TransformDirection(m_Pivot);
        }
    }

    void Start()
    {
        m_Pivot = rootBone.InverseTransformDirection(transform.TransformDirection(axis));
    }

    void OnDrawGizmosSelected()
    {
        Vector3 rotAxis = transform.TransformDirection(axis);
        Vector3 newVec = Vector3.ProjectOnPlane(Vector3.forward, rotAxis);

        float angle = 360.0f / count;
        for (int i = 0; i < count; i++)
        {
            Vector3 tmpVector3 = Quaternion.AngleAxis(angle * i, rotAxis) * newVec;
            Vector3 tmpAxis = Vector3.Cross(rotAxis, tmpVector3);
            Vector3 tmpDir = Quaternion.AngleAxis(limit, tmpAxis) * rotAxis;

            Gizmos.DrawRay(transform.position, tmpDir);
        }
    }
}
